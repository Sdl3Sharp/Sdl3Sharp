using Sdl3Sharp.Internal;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when a <see cref="Input.Finger"/> is placed on or lifted from a <see cref="Input.TouchDevice"/>,
/// when there's <see cref="Input.Finger"/> motion on a <see cref="Input.TouchDevice"/>, or when such a motion is canceled
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.FingerDown"/></description></item>
/// <item><description><see cref="EventType.FingerUp"/></description></item>
/// <item><description><see cref="EventType.FingerMotion"/></description></item>
/// <item><description><see cref="EventType.FingerCanceled"/></description></item>
/// </list>
/// </para>
/// </remarks>
public partial struct TouchFingerEvent : IFormattable, ISpanFormattable
{
	private CommonEvent mCommon;
	private ulong mTouchId;
	private ulong mFingerId;
	private float mX;
	private float mY;
	private float mDx;
	private float mDy;
	private float mPressure;
	private uint mWindowID;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="TouchFingerEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(TouchFingerEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the <see cref="TouchDevice.Id">ID</see> of the <see cref="Input.TouchDevice"/> associated with this event
	/// </summary>
	/// <value>
	/// The <see cref="TouchDevice.Id">ID</see> of the <see cref="Input.TouchDevice"/> associated with this event
	/// </value>
	public ulong TouchDeviceId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mTouchId;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mTouchId = value;
	}

	// TODO: Add a `TouchDevice` property once the `TouchDevice` type is implemented

	/// <summary>
	/// Gets or sets the <see cref="Finger.Id">ID</see> of the <see cref="Input.Finger"/> on the <see cref="Input.TouchDevice"/>
	/// </summary>
	/// <value>
	/// The <see cref="Finger.Id">ID</see> of the <see cref="Input.Finger"/> on the <see cref="Input.TouchDevice"/>
	/// </value>
	public ulong FingerId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mFingerId;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mFingerId = value;
	}

	// TODO: Add a `Finger` property once the `Finger` type is implemented

	/// <summary>
	/// Gets or sets the horizontal position of the <see cref="Finger"/> on the <see cref="TouchDevice"/>
	/// </summary>
	/// <value>
	/// The horizontal position of the <see cref="Finger"/> on the <see cref="TouchDevice"/>, normalized between <c>0</c> and <c>1</c>, where <c>0</c> is the left edge and <c>1</c> is the right edge
	/// </value>
	/// <remarks>
	/// <para>
	/// Note that while the coordinates are <em>normalized</em>, they are not <em>clamped</em>, which means in some circumstances you can get a value outside of this range.
	/// For example, a renderer using logical presentation might give a negative value when the touch is in the letterboxing.
	/// Some platforms might report a touch outside of the window, which will also be outside of the range.
	/// </para>
	/// </remarks>
	public float X
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mX;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mX = value;
	}

	/// <summary>
	/// Gets or sets the vertical position of the <see cref="Finger"/> on the <see cref="TouchDevice"/>
	/// </summary>
	/// <value>
	/// The vertical position of the <see cref="Finger"/> on the <see cref="TouchDevice"/>, normalized between <c>0</c> and <c>1</c>, where <c>0</c> is the top edge and <c>1</c> is the bottom edge
	/// </value>
	/// <remarks>
	/// <para>
	/// Note that while the coordinates are <em>normalized</em>, they are not <em>clamped</em>, which means in some circumstances you can get a value outside of this range.
	/// For example, a renderer using logical presentation might give a negative value when the touch is in the letterboxing.
	/// Some platforms might report a touch outside of the window, which will also be outside of the range.
	/// </para>
	/// </remarks>
	public float Y
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mY;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mY = value;
	}

	/// <summary>
	/// Gets or sets the change in horizontal position of the <see cref="Finger"/> on the <see cref="TouchDevice"/>
	/// </summary>
	/// <value>
	/// The change in horizontal position of the <see cref="Finger"/> on the <see cref="TouchDevice"/>, normalized between <c>-1</c> and <c>1</c>, where <c>-1</c> is a tranversal all the way from the right edge to the left edge, and <c>1</c> is a transversal all the way from the left edge to the right edge
	/// </value>
	public float DeltaX
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mDx;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mDx = value;
	}

	/// <summary>
	/// Gets or sets the change in vertical position of the <see cref="Finger"/> on the <see cref="TouchDevice"/>
	/// </summary>
	/// <value>
	/// The change in vertical position of the <see cref="Finger"/> on the <see cref="TouchDevice"/>, normalized between <c>-1</c> and <c>1</c>, where <c>-1</c> is a tranversal all the way from the bottom edge to the top edge, and <c>1</c> is a transversal all the way from the top edge to the bottom edge
	/// </value>
	public float DeltaY
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mDy;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mDy = value;
	}

	/// <summary>
	/// Gets or sets the pressure of the <see cref="Finger"/> applied on the <see cref="TouchDevice"/>
	/// </summary>
	/// <value>
	/// The pressure of the <see cref="Finger"/> applied on the <see cref="TouchDevice"/>, normalized between <c>0</c> and <c>1</c>, where <c>0</c> is no pressure and <c>1</c> is maximum pressure
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException">
	/// When setting this property, the given value is less than <c>0</c> or greater than <c>1</c>
	/// </exception>
	public float Pressure
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mPressure;

		set
		{
			if (value is < 0f or > 1f)
			{
				[DoesNotReturn]
				static void failInvalidPressure(float pressure) => throw new ArgumentOutOfRangeException(nameof(value), pressure, $"The {nameof(Pressure)} property must be between 0 and 1, inclusive.");

				failInvalidPressure(value);
			}

			mPressure = value;
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> underneath the <see cref="Finger"/>, if any
	/// </summary>
	/// <value>
	/// The <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> underneath the <see cref="Finger"/>, or <c>0</c> if there is no window underneath the finger or it cannot be determined
	/// </value>
	public uint WindowId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWindowID;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWindowID = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Video.Windowing.Window"/> underneath the <see cref="Finger"/>, if any
	/// </summary>
	/// <value>
	/// The <see cref="Video.Windowing.Window"/> underneath the <see cref="Finger"/>, or <c><see langword="null"/></c> if there is no window underneath the finger or it cannot be determined
	/// </value>
	public Window? Window
	{
		readonly get
		{
			Window.TryGetFromId(mWindowID, out var window);
			return window;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		set => mWindowID = value?.Id ?? 0;
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
			nameof(TouchDeviceId)}: {mTouchId.ToString(format, formatProvider)}, {
			nameof(FingerId)}: {mFingerId.ToString(format, formatProvider)}, {
			nameof(X)}: {mX.ToString(format, formatProvider)}, {
			nameof(Y)}: {mY.ToString(format, formatProvider)}, {
			nameof(DeltaX)}: {mDx.ToString(format, formatProvider)}, {
			nameof(DeltaY)}: {mDy.ToString(format, formatProvider)}, {
			nameof(Pressure)}: {mPressure.ToString(format, formatProvider)}, {
			nameof(WindowId)}: {mWindowID.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(TouchDeviceId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mTouchId, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(FingerId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mFingerId, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(X)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mX, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Y)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mY, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(DeltaX)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mDx, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(DeltaY)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mDy, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Pressure)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mPressure, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(WindowId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWindowID, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
