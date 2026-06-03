using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Sdl3Sharp.Internal;

partial class NativeStrings
{
	private const nuint SmallSharedConverterBufferSize = 1024; // 1 KB should be good enough for most strings (which are short); also this should be a small enough value to not allocate multiple pages for most platforms	

	private static IntPtr mConverterBuffer;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private unsafe static void* AcquireConverterBuffer(nuint requiredCapacity, out nuint actualCapacity)
	{
		[DoesNotReturn]
		static void failCouldNotAllocateBuffer() => throw new OutOfMemoryException("Could not allocate buffer for UTF-8 conversion.");

		void* buffer;
		if (requiredCapacity is <= SmallSharedConverterBufferSize)
		{
			// Acquire a potentially exisisting shared buffer
			buffer = unchecked((void*)Interlocked.Exchange(ref mConverterBuffer, unchecked((IntPtr)(void*)null)));

			// If there was no buffer available (e.g., it's currently borrowed or there wasn't one created yet), we just create a new one.
			// And we create a new one with a size of 'SmallSharedConverterBufferSize', so that we can potentially give it back later and make it the shared buffer for future conversions.

			if (buffer is null)
			{
				buffer = Utilities.NativeMemory.Malloc(SmallSharedConverterBufferSize);

				if (buffer is null)
				{
					failCouldNotAllocateBuffer();
				}
			}

			actualCapacity = SmallSharedConverterBufferSize;
		}
		else
		{
			// We always allocate large buffers anew (we don't share them).
			buffer = Utilities.NativeMemory.Malloc(requiredCapacity);

			if (buffer is null)
			{
				failCouldNotAllocateBuffer();
			}

			actualCapacity = requiredCapacity;
		}

		return buffer;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private unsafe static void ReturnConverterBuffer(void* buffer, nuint capacity, bool zeroMemoryUponReturn)
	{
		unsafe
		{
			if (buffer is null)
			{
				return;
			}

			if (zeroMemoryUponReturn)
			{
				Utilities.NativeMemory.MemSet(buffer, 0, capacity); // zero out the memory before returning or freeing it, as requested
			}

			if (capacity is SmallSharedConverterBufferSize)
			{
				// We only return the buffer, if it was the borrowed one.
				// And we only give back the borrowed buffer, if there wasn't another one created in the meantime.
				// We always just keep the most recently returned buffer as the shared buffer (this is just for the sake of simplicity, it might be bad for the principle of locality though).
				if (unchecked((void*)Interlocked.CompareExchange(ref mConverterBuffer, unchecked((IntPtr)buffer), unchecked((IntPtr)(void*)null))) is not null)
				{
					// We didn't return the buffer (because there was already another one in place), so we should free the buffer instead.
					Utilities.NativeMemory.Free(buffer);
				}
			}
			else
			{
				// We always free large buffers, since we don't share them.
				Utilities.NativeMemory.Free(buffer);
			}
		}
	}
}
