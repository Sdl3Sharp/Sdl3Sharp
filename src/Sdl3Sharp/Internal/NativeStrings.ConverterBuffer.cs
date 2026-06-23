using Sdl3Sharp.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Sdl3Sharp.Internal;

partial class NativeStrings
{
	private sealed class ConverterBuffer : IDisposable
	{
		private const nuint SmallBufferSize = 1024; // 1 KB should be good enough for most strings (which are short); also this should be a small enough value to not allocate multiple pages for most platforms	

		[Flags]
		private enum State : uint
		{
			None         = 0,
			Alive        = 1u << 0, // whether the buffer is "alive" and needs to be returned (i.e., whether it was not disposed yet)
			NeedsZeroing = 1u << 1, // whether the buffer needs to be zeroed upon return
		}

		private static ConverterBuffer? mShared;

		private unsafe readonly void* mBuffer;
		private readonly nuint mCapacity;
		private volatile State mState;

		private unsafe ConverterBuffer(void* buffer, nuint capacity, bool needsZeroing)
		{
			mBuffer = buffer;
			mCapacity = capacity;
			mState = State.Alive | (needsZeroing ? State.NeedsZeroing : State.None);
		}

		~ConverterBuffer() => Dispose(disposing: false);

		public unsafe void* Buffer
		{
			get
			{
				ValidateIsAlive();
				return mBuffer;
			}
		}

		public nuint Capacity
		{
			get
			{
				ValidateIsAlive();
				return mCapacity;
			}
		}

		public static ConverterBuffer Acquire(nuint requiredCapacity, bool needsZeroing)
		{
			unsafe
			{
				if (requiredCapacity is <= SmallBufferSize)
				{
					// Acquire a potentially exisisting shared buffer
					var existing = Interlocked.Exchange(ref mShared, null);

					if (existing is not null)
					{
						// If there was an exisisting buffer that we borrowed now, we reuse it by adjusting its state and returning it.
						existing.mState = State.Alive | (needsZeroing ? State.NeedsZeroing : State.None);

						return existing;
					}

					// There was no shared buffer available at this point.
					// We set the required capacity to 'SmallBufferSize' in order to create a new buffer which we can potentially give back later and make it the shared buffer.
					requiredCapacity = SmallBufferSize;
				}

				// For larger buffers and in the case where there was no shared buffer available, we always allocate a new buffer.
				var buffer = NativeMemory.Malloc(requiredCapacity);

				if (buffer is null)
				{
					[DoesNotReturn]
					static void failCouldNotAllocateBuffer() => throw new OutOfMemoryException("Could not allocate buffer for Unicode conversion.");

					failCouldNotAllocateBuffer();
				}

				return new(buffer, requiredCapacity, needsZeroing);
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			Dispose(disposing: true);
		}

		// This acts as our "return buffer" method
		private void Dispose(bool disposing)
		{
			unsafe
			{
#pragma warning disable CS0420 // A reference to 'mState' is used with 'Interlocked.And' here. It's totally fine to cast away the volatile modifier, since we are using an interlocked operation here anyway.
				if ((unchecked((State)Interlocked.And(ref Unsafe.As<State, uint>(ref mState), unchecked((uint)~State.Alive))) & State.Alive) is 0) // unset the 'Alive' flag, since the buffer is either already disposed or is going to be disposed now,
																																				   // and at the same time check whether the buffer actually needs disposal up until this point
#pragma warning restore CS0420
				{
					// 'Alive' flag was already unset, so we don't do anything
					return;
				}


				if ((mState & State.NeedsZeroing) is not 0) // check whether we need to zero the memory upon return
				{
					NativeMemory.MemSet(mBuffer, 0, mCapacity); // zero out the memory before returning or freeing it, as requested

					mState &= ~State.NeedsZeroing; // technically not needed, since we'll overwrite this upon reacquisition anyway, but it's good practice to do so
				}

				if (mCapacity is SmallBufferSize && disposing)
				{
					// We only return the buffer if it was the borrowed one and only if we're not on the finalizer path.
					// And we only give back the borrowed buffer, if there wasn't another one created in the meantime.
					// We always just keep the most recently returned buffer as the shared buffer (this is just for the sake of simplicity, it might be bad for the principle of locality though).
					if (Interlocked.CompareExchange(ref mShared, this, null) is null)
					{
						// We successfully returned the buffer, so we don't free it,
						// instead we reregister it for potential finalization.
						GC.ReRegisterForFinalize(this);

						return;
					}
				}

				// We always free larger buffers or non-shared buffers, or if we are on the finalizer path.
				NativeMemory.Free(mBuffer);
			}
		}

		private void ValidateIsAlive()
		{
			if ((mState & State.Alive) is 0) // check whether the buffer is alive (i.e., not disposed yet)
			{
				[DoesNotReturn]
				static void failBufferAlreadyDisposed() => throw new ObjectDisposedException(nameof(ConverterBuffer), "The buffer has already been disposed and cannot be used anymore.");

				failBufferAlreadyDisposed();
			}
		}
	}
}
