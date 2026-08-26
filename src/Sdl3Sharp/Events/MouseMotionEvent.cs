using Sdl3Sharp.Input;
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
/// Represents an event that occurs when a mouse is moved
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.MouseMotion"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct MouseMotionEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWindowID;
	private uint mWhich;
	private MouseButtonFlags mState;
	private float mX;
	private float mY;
	private float mXRel;
	private float mYRel;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="MouseMotionEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(MouseMotionEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> associated with this event, or <c>0</c> if no window is associated with this event
	/// </value>
	/// <remarks>
	/// <para>
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="MouseMotionEvent"/> is most likely the window that currently has mouse focus, if any.
	/// </para>
	/// </remarks>
	public uint WindowId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWindowID;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWindowID = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Video.Windowing.Window"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Video.Windowing.Window"/> associated with this event, or <c><see langword="null"/></c> if no window is associated with this event
	/// </value>
	/// <remarks>
	/// <para>
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="MouseMotionEvent"/> is most likely the window that currently has mouse focus, if any.
	/// </para>
	/// </remarks>
	public Window? Window
	{
		readonly get
		{
			Window.TryGetFromId(mWindowID, out var window);
			return window;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWindowID = value?.Id ?? 0;
	}

	/// <summary>
	/// Gets or sets the <see cref="Mouse.Id">ID</see> of the <see cref="Input.Mouse"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Mouse.Id">ID</see> of the <see cref="Input.Mouse"/> associated with this event, or <c>0</c> if the mouse is unknown
	/// </value>
	public uint MouseId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWhich;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWhich = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Input.Mouse"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Input.Mouse"/> associated with this event, or <c><see langword="null"/></c> if the mouse is unknown
	/// </value>
	public Mouse? Mouse
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		readonly get
		{
			if (Input.Mouse.TryGetFromId(mWhich, out var mouse))
			{
				return mouse;
			}

			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		set => mWhich = value?.Id ?? 0;
	}

	/// <summary>
	/// Gets or sets the current mouse button state
	/// </summary>
	/// <value>
	/// The current mouse button state
	/// </value>
	public MouseButtonFlags State
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mState;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mState = value;
	}

	/// <summary>
	/// Gets or sets the horizontal coordinate of the mouse cursor
	/// </summary>
	/// <value>
	/// The horizontal coordinate of the mouse cursor, relative to the <see cref="Window"/>, if any
	/// </value>
	public float X
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mX;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mX = value;
	}

	/// <summary>
	/// Gets or sets the vertical coordinate of the mouse cursor
	/// </summary>
	/// <value>
	/// The vertical coordinate of the mouse cursor, relative to the <see cref="Window"/>, if any
	/// </value>
	public float Y
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mY;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mY = value;
	}

	/// <summary>
	/// Gets or sets the relative motion in the horizontal direction
	/// </summary>
	/// <value>
	/// The relative motion in the horizontal direction
	/// </value>
	public float RelativeX
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mXRel;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mXRel = value;
	}

	/// <summary>
	/// Gets or sets the relative motion in the vertical direction
	/// </summary>
	/// <value>
	/// The relative motion in the vertical direction
	/// </value>
	public float RelativeY
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mYRel;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mYRel = value;
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
			nameof(MouseId)}: {mWhich.ToString(format, formatProvider)}, {
			nameof(State)}: {mState}, {
			nameof(X)}: {mX.ToString(format, formatProvider)}, {
			nameof(Y)}: {mY.ToString(format, formatProvider)}, {
			nameof(RelativeX)}: {mXRel.ToString(format, formatProvider)}, {
			nameof(RelativeY)}: {mYRel.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(WindowId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWindowID, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(MouseId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWhich, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(State)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mState, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(X)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mX, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Y)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mY, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(RelativeX)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mXRel, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(RelativeY)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mYRel, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
