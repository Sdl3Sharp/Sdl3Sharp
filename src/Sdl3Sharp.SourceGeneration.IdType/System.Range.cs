using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

#pragma warning disable IDE0130
namespace System;
#pragma warning restore IDE0130

internal readonly struct Range(Index start, Index end) : IEquatable<Range>
{
	public Index Start => start;
	public Index End => end;

	public static Range StartAt(Index start) => new(start, Index.End);

	public static Range EndAt(Index end) => new(Index.Start, end);

	public static Range All => new(Index.Start, Index.End);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public (int Offset, int Length) GetOffsetAndLength(int lenght)
	{
		var start = Start.GetOffset(lenght);
		var end = End.GetOffset(lenght);

		if ((uint)end > (uint)lenght || (uint)start > (uint)end)
		{
			failLengthArgumentOutOfRange();
		}

		return (start, end - start);

		[DoesNotReturn]
		static void failLengthArgumentOutOfRange() => throw new ArgumentOutOfRangeException(nameof(lenght));
	}

	public bool Equals(Range other) => Start.Equals(other.Start) && End.Equals(other.End);

	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Range other && Equals(other);

	public override int GetHashCode()
	{
		return combineHashCodes(Start.GetHashCode(), End.GetHashCode());

		static int combineHashCodes(int h1, int h2)
		{
			var rol5 = ((uint)h1 << 5) | ((uint)h1 >> 27);
			return ((int)rol5 + h1) ^ h2;
		}
	}

	public override string ToString() => $"{Start}..{End}";
}
