using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace Sdl3Sharp.Internal;

partial class NativeStrings
{
	internal unsafe static TransientString<char> FromUtf8ToUtf16(byte* value, nuint length = 0, bool nullTerminate = false, bool zeroMemoryUponReturn = false)
	{
		if (value is null)
		{
#pragma warning disable CS0618 // This is one of the few places where the constructor is allowed to be used - it's actually made for this
			return new(buffer: null, capacity: 0, length: 0, zeroMemoryUponDispose: false); // it's safe to return a null pointer
#pragma warning restore CS0618
		}

		if (length is not > 0)
		{
			// We interpret a given length of 0 as value being a null-terminated string.

			// We could do a whole other path where we look for an NT terminator while simultaneously converting,
			// but that's really hard to do and even harder if we want to do it efficiently.
			// Instead, we look for the terminator first, with known to be highly optimized methods, e.g. MemoryMarshal.CreateReadOnlySpanFromNullTerminated.

			length = unchecked((nuint)MemoryMarshal.CreateReadOnlySpanFromNullTerminated(value).Length);
		}

		var charCount = length; // worst case: 1 UTF-16 code unit per UTF-8 byte, so the char count is at most the byte count

		if (charCount >= int.MaxValue - 1
			|| charCount >= (nuint.MaxValue / (nuint)sizeof(char)) - 2) // the -2 offset is a safety margin
		{
			[DoesNotReturn]
			static void failLengthTooLarge() => throw new ArgumentOutOfRangeException(nameof(length), "Length is too large to convert to UTF-16.");

			failLengthTooLarge();
		}

		var maxByteCount = charCount * unchecked((nuint)sizeof(char));

		if (nullTerminate)
		{
			maxByteCount += sizeof(char); // we need to account for the null terminator if we want to add it, which is 1 char (2 bytes) in UTF-16
		}

		var buffer = (char*)AcquireConverterBuffer(maxByteCount, out var capacity);

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

			var resultLength = unchecked((nuint)(remainingBuffer - buffer));

#pragma warning disable CS0618 // This is one of the few places where the constructor is allowed to be used - it's actually made for this
			return new(buffer, capacity, resultLength, zeroMemoryUponReturn);
#pragma warning restore CS0618
		}
		catch
		{
			// If we're here, we might have already allocated a new buffer.
			// We should gracefully free it or give it back as the shared buffer to avoid dangling memory allocations.

			ReturnConverterBuffer(buffer, capacity, zeroMemoryUponReturn);

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

#pragma warning disable CS0618 // This is one of the few places where the constructor is allowed to be used - it's actually made for this
				return new(buffer: null, capacity: 0, length: 0, zeroMemoryUponDispose: false);
#pragma warning restore CS0618
			}

			fixed (byte* valuePtr = value)
			{
				return FromUtf8ToUtf16(valuePtr, unchecked((nuint)value.Length), zeroMemoryUponReturn);
			}
		}
	}
}
