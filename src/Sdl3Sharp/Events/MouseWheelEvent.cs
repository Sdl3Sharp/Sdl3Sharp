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
/// Represents an event that occurs when a mouse wheel is scrolled
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.MouseWheel"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct MouseWheelEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWindowID;
	private uint mWhich;
	private float mX;
	private float mY;
	private MouseWheelDirection mDirection;
	private float mMouseX;
	private float mMouseY;
	private int mIntegerX;
	private int mIntegerY;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="MouseWheelEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(MouseWheelEvent)}: {type}.", nameof(value));

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
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="MouseWheelEvent"/> is most likely the window that currently has mouse focus, if any.
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
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="MouseWheelEvent"/> is most likely the window that currently has mouse focus, if any.
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

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWhich = value?.Id ?? 0;
	}

	/// <summary>
	/// Gets or sets the amount scrolled horizontally
	/// </summary>
	/// <value>
	/// The amount scrolled horizontally
	/// </value>
	/// <remarks>
	/// <para>
	/// If the value of this property is positive, the mouse wheel was scrolled to the right.
	/// If the value is negative, the mouse wheel was scrolled to the left.
	/// </para>
	/// <para>
	/// Notice that if the value of the <see cref="Direction"/> property is <see cref="MouseWheelDirection.Flipped"/>, the value of this property is inverted.
	/// You might want to multiply this value by <c>-1</c> in that case to change it back.
	/// </para>
	/// </remarks>
	public float X
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mX;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mX = value;
	}

	/// <summary>
	/// Gets or sets the amount scrolled vertically
	/// </summary>
	/// <value>
	/// The amount scrolled vertically
	/// </value>
	/// <remarks>
	/// <para>
	/// If the value of this property is positive, the mouse wheel was scrolled away from the user.
	/// If the value is negative, the mouse wheel was scrolled towards the user.
	/// </para>
	/// <para>
	/// Notice that if the value of the <see cref="Direction"/> property is <see cref="MouseWheelDirection.Flipped"/>, the value of this property is inverted.
	/// You might want to multiply this value by <c>-1</c> in that case to change it back.
	/// </para>
	/// </remarks>
	public float Y
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mY;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mY = value;
	}

	/// <summary>
	/// Gets or sets the direction type of the mouse wheel scrolling
	/// </summary>
	/// <value>
	/// The direction type of the mouse wheel scrolling
	/// </value>
	/// <remarks>
	/// <para>
	/// Notice that if the value of this property is <see cref="MouseWheelDirection.Flipped"/>, the values of the <see cref="X"/> and <see cref="Y"/> properties are inverted.
	/// You might want to multiply those values by <c>-1</c> in that case to change them back.
	/// </para>
	/// </remarks>
	public MouseWheelDirection Direction
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mDirection;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mDirection = value;
	}

	/// <summary>
	/// Gets or sets the X coordinate of the mouse cursor
	/// </summary>
	/// <value>
	/// The X coordinate of the mouse cursor, relative to the <see cref="Window"/>, if any
	/// </value>
	public float MouseX
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mMouseX;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mMouseX = value;
	}

	/// <summary>
	/// Gets or sets the Y coordinate of the mouse cursor
	/// </summary>
	/// <value>
	/// The Y coordinate of the mouse cursor, relative to the <see cref="Window"/>, if any
	/// </value>
	public float MouseY
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mMouseY;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mMouseY = value;
	}

	/// <summary>
	/// Gets or sets the amount scrolled horizontally, accumulated to whole scroll "ticks"
	/// </summary>
	/// <value>
	/// The amount scrolled horizontally, accumulated to whole scroll "ticks"
	/// </value>
	public int IntegerX
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mIntegerX;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mIntegerX = value;
	}

	/// <summary>
	/// Gets or sets the amount scrolled vertically, accumulated to whole scroll "ticks"
	/// </summary>
	/// <value>
	/// The amount scrolled vertically, accumulated to whole scroll "ticks"
	/// </value>
	public int IntegerY
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mIntegerY;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mIntegerY = value;
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
			nameof(X)}: {mX.ToString(format, formatProvider)}, {nameof(Y)}: {
			mY.ToString(format, formatProvider)}, {
			nameof(Direction)}: {mDirection}, {
			nameof(MouseX)}: {mMouseX.ToString(format, formatProvider)}, {
			nameof(MouseY)}: {mMouseY.ToString(format, formatProvider)}, {
			nameof(IntegerX)}: {mIntegerX.ToString(format, formatProvider)}, {
			nameof(IntegerY)}: {mIntegerY.ToString(format, formatProvider)} }}";

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
			&& SpanFormat.TryWrite($", {nameof(X)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mX, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Y)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mY, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Direction)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mDirection, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(MouseX)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mMouseX, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(MouseY)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mMouseY, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(IntegerX)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mIntegerX, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(IntegerY)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mIntegerY, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
