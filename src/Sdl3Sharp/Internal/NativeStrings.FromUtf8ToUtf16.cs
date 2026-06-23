using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace Sdl3Sharp.Internal;

partial class NativeStrings
{
	internal unsafe static TransientString<char> FromUtf8ToUtf16(byte* value, nuint length = 0, bool nullTerminate = false, bool zeroMemoryUponDispose = false)
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		static nuint getMaxUtf16ByteCountforUtf8ByteCount(nuint byteCount)
		{
			if (byteCount >= int.MaxValue - 1 // As you can see below, the worst case scenario is that for every byte of the UTF-8 input, we need 1 UTF-16 char. Since we know that we only want to output UTF-16 strings with a length of int.MaxValue or less, we guard against that here. The -1 offset is a safety margin and includes a potential null terminator.
				|| byteCount >= (nuint.MaxValue / sizeof(char)) - sizeof(char)) // the -sizeof(char) offset is a safety margin and includes a potential null terminator
			{
				[DoesNotReturn]
				static void failByteCountTooLarge() => throw new ArgumentOutOfRangeException(nameof(length), "Byte count is too large to convert to UTF-16.");

				failByteCountTooLarge();
			}

			return unchecked(byteCount * sizeof(char));
		}

		if (value is null)
		{
			return TransientString.Create<TransientString<char>>(buffer: null, length: 0); // it's safe to return a null pointer
		}

		if (length is not > 0)
		{
			// We interpret a given length of 0 as value being a null-terminated string.

			// We could do a whole other path where we look for an NT terminator while simultaneously converting,
			// but that's really hard to do and even harder if we want to do it efficiently.
			// Instead, we look for the terminator first, with known to be highly optimized methods, e.g. MemoryMarshal.CreateReadOnlySpanFromNullTerminated.

			length = unchecked((nuint)MemoryMarshal.CreateReadOnlySpanFromNullTerminated(value).Length);
		}

		var maxByteCount = getMaxUtf16ByteCountforUtf8ByteCount(length);

		if (nullTerminate)
		{
			maxByteCount += sizeof(char); // we need to account for the null terminator if we want to add it, which is 1 char (2 bytes) in UTF-16
		}

		var converterBuffer = ConverterBuffer.Acquire(maxByteCount, zeroMemoryUponDispose);
		var buffer = unchecked((char*)converterBuffer.Buffer);
		var capacity = converterBuffer.Capacity;

		try
		{
			var remainingBuffer = buffer;
			var remainingCapacity = unchecked((int)nuint.Min(capacity / (nuint)sizeof(char), int.MaxValue));

			var remainingValue = value;
			var remainingLength = length;

			while (true)
			{
				var (valueLenght, isFinalBlock) = remainingLength > int.MaxValue
					? (int.MaxValue, false)
					: (unchecked((int)remainingLength), true);

				var valueSpan = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(remainingValue), valueLenght);
				var bufferSpan = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<char>(remainingBuffer), remainingCapacity);

				var result = Utf8.ToUtf16(valueSpan, bufferSpan, out var bytesRead, out var charsWritten,
					replaceInvalidSequences: true, // we don't need to handle OperationStatus.InvalidData as it would never occur with this option enabled
					isFinalBlock: isFinalBlock // depends on whether or not this is the last input block
				);

				remainingBuffer += charsWritten;
				remainingCapacity -= charsWritten;

				if (result is OperationStatus.Done)
				{
					break;
				}

				// we can be sure that result is either OperationStatus.DestinationTooSmall or OperationStatus.NeedMoreData (only if isFinalBlock is false) at this point,
				// because of the options we enabled and the guarantees provided by Utf8.ToUtf16, so we don't need to handle any other cases

				remainingValue += bytesRead;
				remainingLength -= unchecked((nuint)bytesRead);
			}

			if (nullTerminate)
			{
				if (remainingCapacity is not > 0)
				{
					[DoesNotReturn]
					static void failBufferTooSmall() => throw new InvalidOperationException("The buffer for UTF-16 conversion was allocated with insufficient capacity.");

					// This shouldn't be really possible, since we calculated the required buffer size beforehand,
					// but just on the off chance that our calculations were incorrect, we should gracefully fail instead of writing out of bounds.

					// the catch-rethrow handler will take care of freeing or returning the buffer, so it's safe to just throw
					failBufferTooSmall();
				}

				*remainingBuffer = '\0'; // add the null terminator
			}

			return TransientString.Create<TransientString<char>>(converterBuffer, length: unchecked((nuint)(remainingBuffer - buffer)));
		}
		catch
		{
			// If we're here, we might have already allocated a new buffer.
			// We should gracefully free it or give it back as the shared buffer to avoid dangling memory allocations.
			converterBuffer.Dispose();

			throw;
		}
	}

	internal static TransientString<char> FromUtf8ToUtf16(ReadOnlySpan<byte> value, bool nullTerminate = false, bool zeroMemoryUponReturn = false)
	{
		unsafe
		{
			if (value.Length is 0)
			{
				// Early return an empty string for empty input, because an input with length of 0 would trigger FromUtf8ToUtf16 to look for a null terminator
				// which most certainly wouldn't be present in a span representation of the UTF-8 string.

				return TransientString.Create<TransientString<char>>(buffer: null, length: 0); // it's safe to return a null pointer
			}

			fixed (byte* valuePtr = value)
			{
				return FromUtf8ToUtf16(valuePtr, unchecked((nuint)value.Length), nullTerminate, zeroMemoryUponReturn);
			}
		}
	}
}
