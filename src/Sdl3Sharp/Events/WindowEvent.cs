using Sdl3Sharp.Internal;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

#if SDL3_6_0_OR_GREATER
/// <summary>
/// Represents an event that occurs when a <see cref="Video.Windowing.Window"/> changes its state
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.WindowShown"/></description></item>
/// <item><description><see cref="EventType.WindowHidden"/></description></item>
/// <item><description><see cref="EventType.WindowExposed"/></description></item>
/// <item><description><see cref="EventType.WindowMoved"/></description></item>
/// <item><description><see cref="EventType.WindowResized"/></description></item>
/// <item><description><see cref="EventType.WindowPixelSizeChanged"/></description></item>
/// <item><description><see cref="EventType.WindowMetalViewResized"/></description></item>
/// <item><description><see cref="EventType.WindowMinimized"/></description></item>
/// <item><description><see cref="EventType.WindowMaximized"/></description></item>
/// <item><description><see cref="EventType.WindowRestored"/></description></item>
/// <item><description><see cref="EventType.WindowMouseEnter"/></description></item>
/// <item><description><see cref="EventType.WindowMouseLeave"/></description></item>
/// <item><description><see cref="EventType.WindowFocusGained"/></description></item>
/// <item><description><see cref="EventType.WindowFocusLost"/></description></item>
/// <item><description><see cref="EventType.WindowCloseRequested"/></description></item>
/// <item><description><see cref="EventType.WindowHitTest"/></description></item>
/// <item><description><see cref="EventType.WindowIccProfileChanged"/></description></item>
/// <item><description><see cref="EventType.WindowDisplayChanged"/></description></item>
/// <item><description><see cref="EventType.WindowDisplayScaleChanged"/></description></item>
/// <item><description><see cref="EventType.WindowSafeAreaChanged"/></description></item>
/// <item><description><see cref="EventType.WindowOccluded"/></description></item>
/// <item><description><see cref="EventType.WindowEnterFullscreen"/></description></item>
/// <item><description><see cref="EventType.WindowLeaveFullscreen"/></description></item>
/// <item><description><see cref="EventType.WindowDestroyed"/></description></item>
/// <item><description><see cref="EventType.WindowHdrStateChanged"/></description></item>
/// <item><description><see cref="EventType.WindowSettingsChanged"/></description></item>
/// </list>
/// </para>
/// </remarks>
#else
/// <summary>
/// Represents an event that occurs when a <see cref="Video.Windowing.Window"/> changes its state
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.WindowShown"/></description></item>
/// <item><description><see cref="EventType.WindowHidden"/></description></item>
/// <item><description><see cref="EventType.WindowExposed"/></description></item>
/// <item><description><see cref="EventType.WindowMoved"/></description></item>
/// <item><description><see cref="EventType.WindowResized"/></description></item>
/// <item><description><see cref="EventType.WindowPixelSizeChanged"/></description></item>
/// <item><description><see cref="EventType.WindowMetalViewResized"/></description></item>
/// <item><description><see cref="EventType.WindowMinimized"/></description></item>
/// <item><description><see cref="EventType.WindowMaximized"/></description></item>
/// <item><description><see cref="EventType.WindowRestored"/></description></item>
/// <item><description><see cref="EventType.WindowMouseEnter"/></description></item>
/// <item><description><see cref="EventType.WindowMouseLeave"/></description></item>
/// <item><description><see cref="EventType.WindowFocusGained"/></description></item>
/// <item><description><see cref="EventType.WindowFocusLost"/></description></item>
/// <item><description><see cref="EventType.WindowCloseRequested"/></description></item>
/// <item><description><see cref="EventType.WindowHitTest"/></description></item>
/// <item><description><see cref="EventType.WindowIccProfileChanged"/></description></item>
/// <item><description><see cref="EventType.WindowDisplayChanged"/></description></item>
/// <item><description><see cref="EventType.WindowDisplayScaleChanged"/></description></item>
/// <item><description><see cref="EventType.WindowSafeAreaChanged"/></description></item>
/// <item><description><see cref="EventType.WindowOccluded"/></description></item>
/// <item><description><see cref="EventType.WindowEnterFullscreen"/></description></item>
/// <item><description><see cref="EventType.WindowLeaveFullscreen"/></description></item>
/// <item><description><see cref="EventType.WindowDestroyed"/></description></item>
/// <item><description><see cref="EventType.WindowHdrStateChanged"/></description></item>
/// </list>
/// </para>
/// </remarks>
#endif
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct WindowEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWindowID;
	private int mData1;
	private int mData2;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="WindowEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(WindowEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> associated with this event
	/// </summary>
	/// <value>
	/// The <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> associated with this event
	/// </value>
	public uint WindowId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWindowID;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWindowID = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Video.Windowing.Window"/> associated with this event
	/// </summary>
	/// <value>
	/// The <see cref="Video.Windowing.Window"/> associated with this event
	/// </value>
	/// <remarks>
	/// <para>
	/// If the <see cref="Type"/> of this event is <see cref="EventType.WindowDestroyed"/>, you should not use the <see cref="Video.Windowing.Window"/> returned by this property any longer, as its internal resources will already have been released.
	/// An exception from this is if you have received this event in a <see cref="EventWatch"/> where you can still use the associated <see cref="Window"/> during the callback, but you should not use it after the callback has returned.
	/// </para>
	/// </remarks>
	/// <exception cref="InvalidOperationException">
	/// When getting this property, the associated <see cref="Video.Windowing.Window"/> is invalid (e.g. <see cref="WindowId"/> is <c>0</c>)
	/// </exception>
	/// <exception cref="ArgumentNullException">
	/// When setting this property, the given <see cref="Video.Windowing.Window"/> is <c><see langword="null"/></c>
	/// </exception>
	public Window Window
	{
		readonly get
		{
			if (!Window.TryGetFromId(mWindowID, out var window))
			{
				[DoesNotReturn]
				static void failInvalidWindow() => throw new InvalidOperationException($"The associated {nameof(Video.Windowing.Window)} with the {nameof(WindowEvent)} is invalid.");

				failInvalidWindow();
			}

			return window;
		}

		set
		{
			if (value is null)
			{
				[DoesNotReturn]
				static void failNullWindow() => throw new ArgumentNullException(nameof(value), $"The given {nameof(Video.Windowing.Window)} must not be null.");

				failNullWindow();
			}

			mWindowID = value.Id;
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
	/// The semantics of this property depend on the <see cref="Type"/> of the <see cref="WindowEvent"/>.
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
	/// The semantics of this property depend on the <see cref="Type"/> of the <see cref="WindowEvent"/>.
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
			nameof(WindowId)}: {mWindowID.ToString(format, formatProvider)}, {
			nameof(Data1)}: {mData1.ToString(format, formatProvider)}, {
			nameof(Data2)}: {mData2.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(WindowId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWindowID, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Data1)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mData1, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Data2)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mData2, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
