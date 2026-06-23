using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace Sdl3Sharp.Internal;

partial class NativeStrings
{
	internal static TransientString<byte> FromUtf16ToUtf8(ReadOnlySpan<char> value, bool nullTerminate = true, bool zeroMemoryUponDispose = false)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		static nuint getMaxUtf8ByteCountforUtf16CharCount(int charCount)
		{
			/* This is only ever used on Span<char>.Length, which is guaranteed to be non-negative, so we don't actually need to check for negative values
			 * 
			if (charCount is < 0)
			{
				[DoesNotReturn]
				static void failCharCountArgumentOutOfRange() => throw new ArgumentOutOfRangeException(nameof(charCount), "Character count cannot be negative.");

				failCharCountArgumentOutOfRange();
			}
			*/

			var charCountN = unchecked((nuint)charCount);

			if (charCountN >= (nuint.MaxValue / (3 * sizeof(byte))) - sizeof(byte)) // the -sizeof(byte) offset is a safety margin and includes a potential null terminator
			{
				[DoesNotReturn]
				static void failCharCountTooLarge() => throw new ArgumentOutOfRangeException(nameof(charCount), "Character count is too large to convert to UTF-8.");

				failCharCountTooLarge();
			}

			return unchecked(charCountN * (3 * sizeof(byte)));
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

			var converterBuffer = ConverterBuffer.Acquire(maxByteCount, zeroMemoryUponDispose);
			var buffer = unchecked((byte*)converterBuffer.Buffer);
			var capacity = converterBuffer.Capacity;

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

				return TransientString.Create<TransientString<byte>>(converterBuffer, length: unchecked((nuint)(remainingBuffer - buffer)));
			}
			catch
			{
				// If we're here, we might have already allocated a new buffer.
				// We should gracefully free it or give it back as the shared buffer to avoid dangling memory allocations.
				converterBuffer.Dispose();

				throw;
			}
		}
	}

	internal static TransientString<byte> FromUtf16ToUtf8(string? value, bool nullTerminate = true, bool zeroMemoryUponDispose = false)
	{
		unsafe
		{ 
			if (value is null)
			{
				return TransientString.Create<TransientString<byte>>(buffer: null, length: 0); // it's safe to return a null pointer
			}

			return FromUtf16ToUtf8(value.AsSpan(), nullTerminate, zeroMemoryUponDispose);
		}
	}	
}
