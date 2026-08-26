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
/// Represents an event that occurs when a <see cref="Input.Pen"/> axis (angle, pressure, etc.) changes
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.PenAxis"/></description></item>
/// </list>
/// </para>
/// <para>
/// SDL might even send these events when the pen is not touching a surface.
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct PenAxisEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWindowID;
	private uint mWhich;
	private PenInputFlags mPenState;
	private float mX;
	private float mY;
	private PenAxis mAxis;
	private float mValue;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="PenAxisEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(PenAxisEvent)}: {type}.", nameof(value));

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
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="PenAxisEvent"/> is most likely the window that currently has pen focus, if any.
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
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="PenAxisEvent"/> is most likely the window that currently has pen focus, if any.
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
	/// Gets or sets the <see cref="Pen.Id">ID</see> of the <see cref="Input.Pen"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Pen.Id">ID</see> of the <see cref="Input.Pen"/> associated with this event
	/// </value>
	public uint PenId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWhich;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWhich = value;
	}

	// TODO: Add a `Pen` property once the `Pen` type is implemented

	/// <summary>
	/// Gets or sets the state of the <see cref="Pen"/> at the time of this event
	/// </summary>
	/// <value>
	/// The state of the <see cref="Pen"/> at the time of this event
	/// </value>
	public PenInputFlags PenState
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mPenState;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mPenState = value;
	}

	/// <summary>
	/// Gets or sets the horizontal coordinate of the pen
	/// </summary>
	/// <value>
	/// The horizontal coordinate of the pen, relative to the <see cref="Window"/>, if any
	/// </value>
	public float X
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mX;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mX = value;
	}

	/// <summary>
	/// Gets or sets the vertical coordinate of the pen
	/// </summary>
	/// <value>
	/// The vertical coordinate of the pen, relative to the <see cref="Window"/>, if any
	/// </value>
	public float Y
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mY;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mY = value;
	}

	/// <summary>
	/// Gets or sets the gamepad axis that changed
	/// </summary>
	/// <value>
	/// The gamepad axis that changed
	/// </value>
	public PenAxis Axis
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mAxis;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mAxis = value;
	}

	/// <summary>
	/// Gets or sets the value for the <see cref="Axis"/> that changed
	/// </summary>
	/// <value>
	/// The value for the <see cref="Axis"/> that changed.
	/// The allowed range and meaning of this value depends on the kind of <see cref="Axis"/> that changed, please refer to the <see cref="PenAxis"/> documentation for more information about the values for each kind of axis.
	/// </value>
	public float Value
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mValue;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mValue = value;
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
			nameof(PenId)}: {mWhich.ToString(format, formatProvider)}, {
			nameof(PenState)}: {mPenState}, {
			nameof(X)}: {mX.ToString(format, formatProvider)}, {
			nameof(Y)}: {mY.ToString(format, formatProvider)}, {
			nameof(Axis)}: {mAxis}, {
			nameof(Value)}: {mValue.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(WindowId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWindowID, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(PenId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWhich, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(PenState)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mPenState, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(X)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mX, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Y)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mY, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Axis)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mAxis, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(Value)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mValue, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
