using Sdl3Sharp.Internal;
using Sdl3Sharp.Timing;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event structure that contains commonly shared properties by all other event structures.
/// This structure is an event of its own, but can also be viewed as a foundation for other event structures.
/// </summary>
/// <remarks> 
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.Terminating"/></description></item>
/// <item><description><see cref="EventType.LowMemory"/></description></item>
/// <item><description><see cref="EventType.WillEnterBackground"/></description></item>
/// <item><description><see cref="EventType.DidEnterBackground"/></description></item>
/// <item><description><see cref="EventType.WillEnterForeground"/></description></item>
/// <item><description><see cref="EventType.DidEnterForeground"/></description></item>
/// <item><description><see cref="EventType.LocaleChanged"/></description></item>
/// <item><description><see cref="EventType.SystemThemeChanged"/></description></item>
/// <item><description><see cref="EventType.KeymapChanged"/></description></item>
/// <item><description><see cref="EventType.ScreenKeyboardShown"/></description></item>
/// <item><description><see cref="EventType.ScreenKeyboardHidden"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct CommonEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString();

	private EventType mType;
	private readonly uint mReserved;
	private ulong mTimestamp;

	/// <inheritdoc/>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mType;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mType = value;
	}

	/// <inheritdoc/>
	public ulong Timestamp
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mTimestamp;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mTimestamp = value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static void DeconstructTimestamp(ulong timestamp, out ulong hours, out ulong minutes, out ulong seconds, out ulong milliseconds, out ulong nanoseconds)
	{
		(timestamp, nanoseconds) = ulong.DivRem(timestamp, Time.NanosecondsPerMillisecond);
		(timestamp, milliseconds) = ulong.DivRem(timestamp, Time.MillisecondsPerSecond);
		(timestamp, seconds) = ulong.DivRem(timestamp, 60 /* seconds per minute */);
		(timestamp, minutes) = ulong.DivRem(timestamp, 60 /* minutes per hour */);
		hours = timestamp; // hours is the most significant unit of our timestamp deconstruction
	}

	internal static string FormatTimestamp(ulong timestamp)
	{
		DeconstructTimestamp(timestamp, out var hours, out var minutes, out var seconds, out var milliseconds, out var nanoseconds);

		return $"{hours:0}h {minutes:00}min {seconds:00}s {milliseconds:000}ms {nanoseconds:000000}ns";
	}

	internal static bool TryFormatTimestamp(ulong timestamp, ref Span<char> destination, ref int charsWritten)
	{
		DeconstructTimestamp(timestamp, out var hours, out var minutes, out var seconds, out var milliseconds, out var nanoseconds);

		return SpanFormat.TryWrite(hours, ref destination, ref charsWritten, format: "0")
			&& SpanFormat.TryWrite("h ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(minutes, ref destination, ref charsWritten, format: "00")
			&& SpanFormat.TryWrite("min ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(seconds, ref destination, ref charsWritten, format: "00")
			&& SpanFormat.TryWrite("s ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(milliseconds, ref destination, ref charsWritten, format: "000")
			&& SpanFormat.TryWrite("ms ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(nanoseconds, ref destination, ref charsWritten, format: "000000")
			&& SpanFormat.TryWrite("ns ", ref destination, ref charsWritten);
	}

	internal readonly string ToPartialString()
		=> $"{nameof(Type)}: {mType}, {
			nameof(Timestamp)}: {FormatTimestamp(mTimestamp)}";

	internal readonly bool TryPartiallyFormat(ref Span<char> destination, ref int charsWritten)
		=> SpanFormat.TryWrite($"{nameof(Type)}: ", ref destination, ref charsWritten)
		&& SpanFormat.TryWrite(mType, ref destination, ref charsWritten)
		&& SpanFormat.TryWrite($", {nameof(Timestamp)}: ", ref destination, ref charsWritten)
		&& TryFormatTimestamp(mTimestamp, ref destination, ref charsWritten);

	/// <inheritdoc/>
	public readonly override string ToString()
		=> $"{{ {ToPartialString()} }}";

	/// <inheritdoc/>
	readonly string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString();

	/// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}

	/// <inheritdoc/>
	readonly bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => TryFormat(destination, out charsWritten);
}
