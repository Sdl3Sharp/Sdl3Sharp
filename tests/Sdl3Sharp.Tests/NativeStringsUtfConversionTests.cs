// DISCLAIMER: This source file was created 100% by AI (GitHub Copilot).

using System;
using System.Text;
using Sdl3Sharp.Internal;

namespace Sdl3Sharp.Tests;

public sealed class NativeStringsUtfConversionTests
{
	[Fact]
	public unsafe void FromUtf16ToUtf8_StringNull_ReturnsNullBufferAndZeroLength()
	{
		using var utf8 = NativeStrings.FromUtf16ToUtf8((string?)null);

		Assert.True(utf8.Buffer is null);
		Assert.Equal((nuint)0, utf8.Length);
		Assert.Equal((nuint)0, utf8.Capacity);
	}

	[Fact]
	public unsafe void FromUtf16ToUtf8_WithNullTerminator_WritesTerminatorOutsideReportedLength()
	{
		const string value = "Hello World";
		using var utf8 = NativeStrings.FromUtf16ToUtf8(value, nullTerminate: true);

		Assert.Equal(value, ToUtf8ManagedString(utf8));
		Assert.True(utf8.Buffer is not null);
		Assert.Equal((byte)'\0', utf8.Buffer[checked((int)utf8.Length)]);
		Assert.True(utf8.Capacity > utf8.Length);
	}

	[Fact]
	public unsafe void FromUtf16ToUtf8_WithoutNullTerminator_UsesExactUtf8ByteLength()
	{
		ReadOnlySpan<char> value = "Zażółć gęślą jaźń 😀";
		using var utf8 = NativeStrings.FromUtf16ToUtf8(value, nullTerminate: false);

		Assert.Equal(unchecked((nuint)Encoding.UTF8.GetByteCount(value)), utf8.Length);
		Assert.Equal(value.ToString(), ToUtf8ManagedString(utf8));
	}

	[Fact]
	public unsafe void FromUtf8ToUtf16_ReadOnlySpan_RoundTripsAndAppendsOptionalNullTerminator()
	{
		ReadOnlySpan<byte> utf8Source = "Hello World from UTF-8 😀"u8;
		using var utf16 = NativeStrings.FromUtf8ToUtf16(utf8Source, nullTerminate: true);

		Assert.Equal("Hello World from UTF-8 😀", utf16.ToManaged());
		Assert.True(utf16.Buffer is not null);
		Assert.Equal('\0', utf16.Buffer[checked((int)utf16.Length)]);
		Assert.True(utf16.Capacity > utf16.Length);
	}

	[Fact]
	public unsafe void FromUtf8ToUtf16_BytePointerWithLengthZero_TreatsInputAsNullTerminated()
	{
		ReadOnlySpan<byte> utf8NullTerminated = "Prefix\0Suffix"u8;

		fixed (byte* ptr = utf8NullTerminated)
		{
			using var utf16 = NativeStrings.FromUtf8ToUtf16(ptr, length: 0, nullTerminate: false);

			Assert.Equal("Prefix", utf16.ToManaged());
		}
	}

	[Fact]
	public unsafe void FromUtf8ToUtf16_NullPointer_ReturnsNullBufferAndNullManagedString()
	{
		using var utf16 = NativeStrings.FromUtf8ToUtf16((byte*)null, length: 0, nullTerminate: false);

		Assert.True(utf16.Buffer is null);
		Assert.Equal((nuint)0, utf16.Length);
		Assert.Equal((nuint)0, utf16.Capacity);
		Assert.Null(utf16.ToManaged());
	}

	[Fact]
	public unsafe void FromUtf8ToUtf16_EmptySpan_ReturnsNullBufferAndNullManagedString()
	{
		ReadOnlySpan<byte> utf8Source = [];
		using var utf16 = NativeStrings.FromUtf8ToUtf16(utf8Source, nullTerminate: false);

		Assert.True(utf16.Buffer is null);
		Assert.Equal((nuint)0, utf16.Length);
		Assert.Equal((nuint)0, utf16.Capacity);
		Assert.Null(utf16.ToManaged());
	}

	[Fact]
	public unsafe void FromUtf8ToUtf16_BytePointerWithExplicitLength_PreservesEmbeddedNullCharacters()
	{
		ReadOnlySpan<byte> utf8WithEmbeddedNull = "A\0B"u8;

		fixed (byte* ptr = utf8WithEmbeddedNull)
		{
			using var utf16 = NativeStrings.FromUtf8ToUtf16(ptr, length: unchecked((nuint)utf8WithEmbeddedNull.Length), nullTerminate: false);

			var managed = utf16.ToManaged();

			Assert.NotNull(managed);
			Assert.Equal(3, managed!.Length);
			Assert.Equal('A', managed[0]);
			Assert.Equal('\0', managed[1]);
			Assert.Equal('B', managed[2]);
		}
	}

	[Fact]
	public unsafe void FromUtf8ToUtf16_InvalidUtf8_ReplacesInvalidSequences()
	{
		ReadOnlySpan<byte> invalidUtf8 = [0xC3, 0x28];
		using var utf16 = NativeStrings.FromUtf8ToUtf16(invalidUtf8, nullTerminate: false);

		Assert.Equal(Encoding.UTF8.GetString(invalidUtf8), utf16.ToManaged());
	}

	[Fact]
	public unsafe void FromUtf16ToUtf8_InvalidUtf16_ReplacesInvalidSequences()
	{
		ReadOnlySpan<char> invalidUtf16 = ['\uD800', 'A'];
		using var utf8 = NativeStrings.FromUtf16ToUtf8(invalidUtf16, nullTerminate: false);

		var expected = Encoding.UTF8.GetBytes(invalidUtf16.ToArray());
		var actual = utf8.Buffer is null
			? ReadOnlySpan<byte>.Empty
			: new ReadOnlySpan<byte>(utf8.Buffer, checked((int)utf8.Length));

		Assert.True(actual.SequenceEqual(expected));
	}

	[Fact]
	public unsafe void ToManaged_WhenBufferIsNonNullAndLengthIsZero_ReturnsEmptyString()
	{
		ReadOnlySpan<byte> explicitNullTerminatedInput = "\0"u8;

		fixed (byte* ptr = explicitNullTerminatedInput)
		{
			using var utf16 = NativeStrings.FromUtf8ToUtf16(ptr, length: 0, nullTerminate: false);

			Assert.True(utf16.Buffer is not null);
			Assert.Equal((nuint)0, utf16.Length);
			Assert.Equal(string.Empty, utf16.ToManaged());
		}
	}

	private static unsafe string ToUtf8ManagedString(NativeStrings.TransientString<byte> value)
	{
		if (value.Buffer is null)
		{
			return string.Empty;
		}

		if (value.Length is 0)
		{
			return string.Empty;
		}

		var span = new ReadOnlySpan<byte>(value.Buffer, checked((int)value.Length));
		return Encoding.UTF8.GetString(span);
	}
}
