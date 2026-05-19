using Sdl3Sharp.Internal;
using Sdl3Sharp.Timing;
using System;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events2;

[StructLayout(LayoutKind.Sequential)]
internal struct CommonEventData
{
	internal EventType Type;
	private readonly uint mReserved;
	internal ulong Timestamp;

	private readonly string FormatTimestamp()
	{
		var timestamp = Timestamp;

		(timestamp, var ns) = Math.DivRem(timestamp, Time.NanosecondsPerMillisecond);
		(timestamp, var ms) = Math.DivRem(timestamp, Time.MillisecondsPerSecond);
		(timestamp, var s) = Math.DivRem(timestamp, 60 /* seconds per minute */);
		(timestamp, var min) = Math.DivRem(timestamp, 60 /* minutes per hour */);
		var h = timestamp; // hours are the most significant part, so we can just assign it

		return $"{h:0}h {min:00}min {s:00}s {ms:000}ms {ns:000000}ns";
	}

	private readonly bool TryFormatTimestamp(ref Span<char> destination, ref int charsWritten)
	{
		var timestamp = Timestamp;

		(timestamp, var ns) = Math.DivRem(timestamp, Time.NanosecondsPerMillisecond);
		(timestamp, var ms) = Math.DivRem(timestamp, Time.MillisecondsPerSecond);
		(timestamp, var s) = Math.DivRem(timestamp, 60 /* seconds per minute */);
		(timestamp, var min) = Math.DivRem(timestamp, 60 /* minutes per hour */);
		var h = timestamp; // hours are the most significant part, so we can just assign it

		return SpanFormat.TryWrite(h, ref destination, ref charsWritten, format: "0")
			&& SpanFormat.TryWrite("h ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(min, ref destination, ref charsWritten, format: "00")
			&& SpanFormat.TryWrite("min ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(s, ref destination, ref charsWritten, format: "00")
			&& SpanFormat.TryWrite("s ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(ms, ref destination, ref charsWritten, format: "000")
			&& SpanFormat.TryWrite("ms ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(ns, ref destination, ref charsWritten, format: "000000")
			&& SpanFormat.TryWrite("ns", ref destination, ref charsWritten);
	}

	internal readonly string ToPartialString()
		=> $"{nameof(Type)}: {Type}, {
			nameof(Timestamp)}: {FormatTimestamp()}";

	internal readonly bool TryPartiallyFormat(ref Span<char> destination, ref int charsWritten)
		=> SpanFormat.TryWrite($"{nameof(Type)}: ", ref destination, ref charsWritten)
		&& SpanFormat.TryWrite(Type, ref destination, ref charsWritten)
		&& SpanFormat.TryWrite($", {nameof(Timestamp)}: ", ref destination, ref charsWritten)
		&& TryFormatTimestamp(ref destination, ref charsWritten);
}
