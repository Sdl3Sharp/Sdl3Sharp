using Sdl3Sharp.Internal;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events2;

partial struct Event
{
	[FieldOffset(0)] internal Event<DisplayEventData> Display;
}

/// <summary>
/// Represents event data for an event that occurs when a <see cref="Display"/> changes its state
/// </summary>
/// <remarks>
/// <para>
/// Depending on the actual <see cref="Event{TEventData}.Type"/> the values of the properties <see cref="Data1"/> and <see cref="Data2"/> may reflect different data semantics.
/// </para>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.DisplayOrientationChanged"/></description></item> 
/// <item><description><see cref="EventType.DisplayAdded"/></description></item> 
/// <item><description><see cref="EventType.DisplayRemoved"/></description></item>
/// <item><description><see cref="EventType.DisplayMoved"/></description></item>
/// <item><description><see cref="EventType.DisplayDesktopModeChanged"/></description></item>
/// <item><description><see cref="EventType.DisplayCurrentModeChanged"/></description></item>
/// <item><description><see cref="EventType.DisplayContentScaleChanged"/></description></item>
/// <item><description><see cref="EventType.DisplayUsableBoundsChanged"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public struct DisplayEventData : IEventData<DisplayEventData>, IFormattable, ISpanFormattable
{
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static bool IEventData<DisplayEventData>.AcceptsEventType(EventType type) => type is >= EventType.AudioDeviceAdded and <= EventType.AudioDeviceFormatChanged;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private uint mDisplayID;
	private int mData1;
	private int mData2;

	/// <summary>
	/// Gets or sets the display ID of the <see cref="Display"/> which changes it's state
	/// </summary>
	/// <value>
	/// The display ID of the <see cref="Display"/> which changes it's state
	/// </value>
	public uint DisplayId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mDisplayID;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mDisplayID = value;
	}

	/// <summary>
	/// Gets or sets the value of first event dependent data slot
	/// </summary>
	/// <value>
	/// The value of the first event dependent data slot
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property may reflect different data semantics dependent on the actual <see cref="Event{TEventData}.Type"/>.
	/// </para>
	/// </remarks>
	public int Data1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mData1;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mData1 = value;
	}

	/// <summary>
	/// Gets or sets the value of second event dependent data slot
	/// </summary>
	/// <value>
	/// The value of the second event dependent data slot
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property may reflect different data semantics dependent on the actual <see cref="Event{TEventData}.Type"/>.
	/// </para>
	/// </remarks>
	public int Data2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mData2;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mData2 = value;
	}

	private readonly string ToPartialString(string? format, IFormatProvider? formatProvider)
		=> $"{nameof(DisplayId)}: {DisplayId.ToString(format, formatProvider)}, {
			nameof(Data1)}: {Data1.ToString(format, formatProvider)}, {
			nameof(Data2)}: {Data2.ToString(format, formatProvider)}";

	readonly string IEventData<DisplayEventData>.ToPartialString(string? format, IFormatProvider? formatProvider)
		=> $", {ToPartialString(format, formatProvider)}";

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {ToPartialString(format, formatProvider)} }}";

	private readonly bool TryPartiallyFormat(ref Span<char> destination, ref int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
		=> SpanFormat.TryWrite($"{nameof(DisplayId)}: ", ref destination, ref charsWritten)
		&& SpanFormat.TryWrite(DisplayId, ref destination, ref charsWritten, format, provider)
		&& SpanFormat.TryWrite($", {nameof(Data1)}: ", ref destination, ref charsWritten)
		&& SpanFormat.TryWrite(Data1, ref destination, ref charsWritten, format, provider)
		&& SpanFormat.TryWrite($", {nameof(Data2)}: ", ref destination, ref charsWritten)
		&& SpanFormat.TryWrite(Data2, ref destination, ref charsWritten, format, provider);

	readonly bool IEventData<DisplayEventData>.TryPartiallyFormat(ref Span<char> destination, ref int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
		=> SpanFormat.TryWrite(", ", ref destination, ref charsWritten)
		&& TryPartiallyFormat(ref destination, ref charsWritten, format, provider);

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& TryPartiallyFormat(ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
