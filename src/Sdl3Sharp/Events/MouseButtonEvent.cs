using Sdl3Sharp.Input;
using Sdl3Sharp.Internal;
using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when a mouse button is pressed or released
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.MouseButtonDown"/></description></item>
/// <item><description><see cref="EventType.MouseButtonUp"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct MouseButtonEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWindowID;
	private uint mWhich;
	private MouseButton mButton;
	private CBool mDown;
	private byte mClicks;
	private readonly byte mPadding;
	private float mX;
	private float mY;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="MouseButtonEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(MouseButtonEvent)}: {type}.", nameof(value));

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
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="MouseButtonEvent"/> is most likely the window that currently has mouse focus, if any.
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
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="MouseButtonEvent"/> is most likely the window that currently has mouse focus, if any.
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
	/// Gets or sets the mouse button
	/// </summary>
	/// <value>
	/// The mouse button
	/// </value>
	public MouseButton Button
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mButton;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mButton = value;
	}

	/// <summary>
	/// Gets or sets a value indicating whether the <see cref="Button"/> is pressed (currently held down)
	/// </summary>
	/// <value>
	/// A value indicating whether the <see cref="Button"/> is pressed (currently held down)
	/// </value>
	public bool IsDown
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mDown;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mDown = value;
	}

	/// <summary>
	/// Gets or sets the number of repeated clicks
	/// </summary>
	/// <value>
	/// The number of repeated clicks
	/// </value>
	/// <remarks>
	/// <para>
	/// <c>1</c> for a single-click, <c>2</c> for a double-click, etc.
	/// </para>
	/// </remarks>
	public byte Clicks
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mClicks;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mClicks = value;
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

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {mCommon.ToPartialString()}, {
			nameof(WindowId)}: {mWindowID.ToString(format, formatProvider)}, {nameof(MouseId)}: {
			mWhich.ToString(format, formatProvider)}, {
			nameof(Button)}: {mButton}, {
			nameof(IsDown)}: {(bool)mDown}, {
			nameof(Clicks)}: {mClicks.ToString(format, formatProvider)}, {
			nameof(X)}: {mX.ToString(format, formatProvider)}, {
			nameof(Y)}: {mY.ToString(format, formatProvider)} }}";

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
			&& SpanFormat.TryWrite($", {nameof(Button)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mButton, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(IsDown)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite((bool)mDown, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(Clicks)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mClicks, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(X)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mX, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Y)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mY, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
