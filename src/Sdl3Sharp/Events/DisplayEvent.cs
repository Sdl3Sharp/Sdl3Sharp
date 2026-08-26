using Sdl3Sharp.Internal;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when a <see cref="Video.Windowing.Display"/> changes its state
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.DisplayOrientation"/></description></item> 
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
public partial struct DisplayEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mDisplayID;
	private int mData1;
	private int mData2;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="DisplayEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(DisplayEvent)}: {type}.", nameof(value));

				failInvalidEventType(value);
			}

			mCommon.Type = value;
		}
	}

	/// <inheritdoc/>
	public ulong Timestamp
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Timestamp;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mCommon.Timestamp = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Display.Id">ID</see> of the <see cref="Video.Windowing.Display"/> associated with this event
	/// </summary>
	/// <value>
	/// The <see cref="Display.Id">ID</see> of the <see cref="Video.Windowing.Display"/> associated with this event
	/// </value>
	public uint DisplayId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mDisplayID;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mDisplayID = value; // We silently allow setting the display id to 0 here. It's fine. The `Display` property will throw an exception if the display id is invalid (0).
	}

	/// <summary>
	/// Gets or sets the <see cref="Video.Windowing.Display"/> associated with this event
	/// </summary>
	/// <value>
	/// The <see cref="Video.Windowing.Display"/> associated with this event
	/// </value>
	/// <exception cref="InvalidOperationException">
	/// When getting this property, the associated <see cref="Video.Windowing.Display"/> is invalid (e.g. <see cref="DisplayId"/> is <c>0</c>)
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// When setting this property, the given <see cref="Video.Windowing.Display"/> is <c><see langword="null"/></c>
	/// </exception>
	public Display Display
	{
		readonly get
		{
			if (!Display.TryGetOrCreate(mDisplayID, out var display))
			{
				// `Display.TryGetOrCreate` only fails if the display id is 0, which should never be the case for a valid display event coming from SDL

				[DoesNotReturn]
				static void failInvalidDisplay() => throw new InvalidOperationException($"The associated {nameof(Video.Windowing.Display)} with the {nameof(DisplayEvent)} is invalid.");

				failInvalidDisplay();
			}

			return display;
		}

		set
		{
			if (value is null)
			{
				[DoesNotReturn]
				static void failDisplayNull() => throw new ArgumentNullException(nameof(value), $"The given {nameof(Video.Windowing.Display)} must not be null.");

				failDisplayNull();
			}

			mDisplayID = value.Id; // This still allows for setting the display id to 0, if the given display is invalid. This shouldn't be possible though, if the display is coming from SDL.
		}
	}

	/// <summary>
	/// Gets or sets the value of the first event dependent data slot
	/// </summary>
	/// <value>
	/// The value of the first event dependent data slot
	/// </value>
	/// <remarks>
	/// <para>
	/// The semantics of this property depend on the <see cref="Type"/> of the <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	public int Data1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mData1;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mData1 = value;
	}

	/// <summary>
	/// Gets or sets the value of the second event dependent data slot
	/// </summary>
	/// <value>
	/// The value of the second event dependent data slot
	/// </value>
	/// <remarks>
	/// <para>
	/// The semantics of this property depend on the <see cref="Type"/> of the <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	public int Data2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mData2;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mData2 = value;
	}

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {mCommon.ToPartialString()}, {
			nameof(DisplayId)}: {mDisplayID.ToString(format, formatProvider)}, {
			nameof(Data1)}: {mData1.ToString(format, formatProvider)}, {
			nameof(Data2)}: {mData2.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(DisplayId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mDisplayID, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Data1)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mData1, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Data2)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mData2, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
