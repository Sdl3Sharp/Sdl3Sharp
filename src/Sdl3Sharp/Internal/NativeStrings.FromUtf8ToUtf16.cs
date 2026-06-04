using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Unicode;

namespace Sdl3Sharp.Internal;

partial class NativeStrings
{
	internal unsafe static string? FromUtf8ToUtf16(byte* value, nuint length = 0, bool zeroMemoryUponReturn = false)
	{
		if (value is null)
		{
			return null;
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
			|| charCount >= (nuint.MaxValue / (nuint)sizeof(char)) - 1) // the -1 offset is a safety margin
		{
			[DoesNotReturn]
			static void failLengthTooLarge() => throw new ArgumentOutOfRangeException(nameof(length), "Length is too large to convert to UTF-16.");

			failLengthTooLarge();
		}

		var maxByteCount = charCount * unchecked((nuint)sizeof(char));

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

				if (result is OperationStatus.Done)
				{
					break;
				}

				// we can be sure that result is either OperationStatus.DestinationTooSmall or OperationStatus.NeedMoreData (only if isFinalBlock is false) at this point,
				// because of the options we enabled and the guarantees provided by Utf8.ToUtf16, so we don't need to handle any other cases

				remainingCapacity -= charsWritten;

				remainingValue += bytesRead;
				remainingLength -= unchecked((nuint)bytesRead);
			}

			// The System.String constructor copies the given buffer, so we safely return or free it afterwards
			return new string(buffer, 0, unchecked((int)(remainingBuffer - buffer)));
		}
		finally
		{
			// Always return or free the buffer, even in case of any exceptions.
			// The managed string instance has a copy of the data at this point (if we got so far at all), so we don't need worry about that.
			ReturnConverterBuffer(buffer, capacity, zeroMemoryUponReturn);
		}
	}

	// Note that this overload can't return null
	internal static string FromUtf8ToUtf16(ReadOnlySpan<byte> value, bool zeroMemoryUponReturn = false)
	{
		unsafe
		{
			if (value.Length is 0)
			{
				// Early return an empty string for empty input, because an input with length of 0 would trigger FromUtf8ToUtf16 to look for a null terminator
				// which most certainly wouldn't be present in a span representation of the UTF-8 string.
				return string.Empty;
			}

			fixed (byte* valuePtr = value)
			{
				// FromUtf8ToUtf16 only returns null if the input pointer is null, but because we're pinning the address of the first element of the span,
				// that surely can't be the case here since the span must exist and has at least one element.
				return FromUtf8ToUtf16(valuePtr, unchecked((nuint)value.Length), zeroMemoryUponReturn)!;
			}
		}
	}
}
