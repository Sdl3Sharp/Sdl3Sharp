using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace System;
#pragma warning restore IDE0130

internal readonly struct Index : IEquatable<Index>
{
	private readonly int mValue;

	private Index(int value) => mValue = value;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Index(int value, bool isFromEnd = false)
	{
		if (value is < 0)
		{
			failValueArgumentIsNegative();
		}

		mValue = isFromEnd ? ~value : value;

		[DoesNotReturn]
		static void failValueArgumentIsNegative() => throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} must be non-negative");
	}

	public static Index Start => new(0);

	public static Index End => new(~0);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Index FromStart(int value)
	{
		if (value is < 0)
		{
			failValueArgumentIsNegative();
		}

		return new(value);

		[DoesNotReturn]
		static void failValueArgumentIsNegative() => throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} must be non-negative");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Index FromEnd(int value)
	{
		if (value is < 0)
		{
			failValueArgumentIsNegative();
		}

		return new(~value);

		[DoesNotReturn]
		static void failValueArgumentIsNegative() => throw new ArgumentOutOfRangeException(nameof(value), $"{nameof(value)} must be non-negative");
	}

	public int Value => mValue is < 0 ? ~mValue : mValue;

	public bool IsFromEnd => mValue is < 0;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetOffset(int length)
	{
		var offset = mValue;

		if (mValue is < 0)
		{
			offset += length + 1;
		}

		return offset;
	}

	public bool Equals(Index other) => mValue == other.mValue;

	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Index other && Equals(other);

	public override int GetHashCode() => mValue;

	public override string ToString() => mValue is < 0 ? $"^{Value}" : $"{Value}";

	public static implicit operator Index(int value) => FromStart(value);
}
