using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace Sdl3Sharp.Internal;

partial class NativeStrings
{
	internal static TransientUtf8String FromUtf16ToUtf8(ReadOnlySpan<char> value, out nuint length, bool nullTerminate = true, bool zeroMemoryUponDispose = false)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		static nuint getMaxUtf8ByteCountforUtf16CharCount(int charCount)
		{
			// We don't do anything fancy here, like he BCL's UTF8Encoding.GetMaxByteCount does.
			// For the sake of simplicity and performance, we just assume the worst case.

			const nuint maxUtf8BytesPerUtf16CodeUnit = 4; // worst case: 4 bytes per UTF-16 code unit

			if (charCount is < 0)
			{
				[DoesNotReturn]
				static void failCharCountArgumentOutOfRange() => throw new ArgumentOutOfRangeException(nameof(charCount), "Character count cannot be negative.");

				failCharCountArgumentOutOfRange();
			}

			var charCountN = unchecked((nuint)charCount);

			if (charCountN >= (nuint.MaxValue / maxUtf8BytesPerUtf16CodeUnit) - 2) // the -2 offest is a safety margin and includes a potential null terminator
			{
				[DoesNotReturn]
				static void failCharCountTooLarge() => throw new ArgumentOutOfRangeException(nameof(charCount), "Character count is too large to convert to UTF-8.");

				failCharCountTooLarge();
			}

			return unchecked(charCountN * maxUtf8BytesPerUtf16CodeUnit);
		}

		// This is so that we can avoid unnecessary bounds checks that the JIT compiler couldn't elide, but we for sure can give guarantees about
		// Note that I actually checked it, and even on the hot path the JIT compiler couldn't elide the bounds checks. This is not a premature optimization!
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		static ReadOnlySpan<T> unsafeSlice<T>(ReadOnlySpan<T> span, int start)
			=> MemoryMarshal.CreateReadOnlySpan(ref Unsafe.Add(ref Unsafe.AsRef(in MemoryMarshal.GetReference(span)), start), unchecked(span.Length - start));

		unsafe
		{
			var maxByteCount = getMaxUtf8ByteCountforUtf16CharCount(value.Length);

			if (nullTerminate)
			{
				maxByteCount += 1; // +1 for null terminator
			}

			var buffer = unchecked((byte*)AcquireConverterBuffer(maxByteCount, out var capacity));

			// In case anything exceptional happens, we append a catch-rethrow handler to gracefully free or return the buffer, so that we don't end up with dangling memory allocations
			try
			{
				var remainingBuffer = buffer;
				var remainingCapacity = capacity;

				while (true)
				{
					var bufferSpan = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(remainingBuffer), unchecked((int)nuint.Min(remainingCapacity, int.MaxValue)));

					var result = Utf8.FromUtf16(value, bufferSpan, out var charsRead, out var bytesWritten,
						replaceInvalidSequences: true, // we don't need to handle OperationStatus.InvalidData as it would never occur with this option enabled
						isFinalBlock: true // we already have all of the input data available in 'value', so we can enable this option and we don't have to handle the OperationStatus.NeedMoreData case
					);

					remainingBuffer += bytesWritten;
					remainingCapacity -= unchecked((nuint)bytesWritten);

					if (result is OperationStatus.Done)
					{
						break;
					}

					// we can be sure that result is OperationStatus.DestinationTooSmall at this point,
					// because of the options we enabled and the guarantees provided by Utf8.FromUtf16, so we don't need to handle any other cases

					value = unsafeSlice(value, charsRead);
				}

				if (nullTerminate)
				{
					if (remainingCapacity is not > 0)
					{
						[DoesNotReturn]
						static void failBufferTooSmall() => throw new InvalidOperationException("The buffer for UTF-8 conversion was allocated with insufficient capacity.");

						// This shouldn't be really possible, since we calculated the required buffer size beforehand,
						// but just on the off chance that our calculations were incorrect, we should gracefully fail instead of writing out of bounds.

						// the catch-rethrow handler will take care of freeing or returning the buffer, so it's safe to just throw
						failBufferTooSmall();
					}

					*remainingBuffer = (byte)'\0'; // add the null terminator					
				}

				length = unchecked((nuint)(remainingBuffer - buffer));

#pragma warning disable CS0618 // This is one of the few places where the constructor is allowed to be used - it's actually made for this
				return new(buffer, capacity, zeroMemoryUponDispose);
#pragma warning restore CS0618 
			}
			catch
			{
				// If we're here, we might have already allocated a new buffer.
				// We should gracefully free it or give it back as the shared buffer to avoid dangling memory allocations.

				ReturnConverterBuffer(buffer, capacity, zeroMemoryUponDispose);

				throw;
			}
		}
	}

	internal static TransientUtf8String FromUtf16ToUtf8(string? value, out nuint length, bool nullTerminate = true, bool zeroMemoryUponDispose = false)
	{
		unsafe
		{ 
			if (value is null)
			{
				length = 0;
#pragma warning disable CS0618 // This is one of the few places where the constructor is allowed to be used - it's actually made for this
				return new(null, 0, false); // it's safe to return a null pointer
#pragma warning restore CS0618
			}

			return FromUtf16ToUtf8(value.AsSpan(), out length, nullTerminate, zeroMemoryUponDispose);
		}
	}	
}
