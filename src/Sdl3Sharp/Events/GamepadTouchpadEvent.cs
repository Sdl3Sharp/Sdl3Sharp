using Sdl3Sharp.Internal;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when a finger touches or moves on a touchpad of a gamepad
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.GamepadTouchpadDown"/></description></item>
/// <item><description><see cref="EventType.GamepadTouchpadMotion"/></description></item>
/// <item><description><see cref="EventType.GamepadTouchpadUp"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct GamepadTouchpadEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWhich;
	private int mTouchpad;
	private int mFinger;
	private float mX;
	private float mY;
	private float mPressure;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="GamepadTouchpadEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(GamepadTouchpadEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the <see cref="Joystick.Id">ID</see> of the <see cref="Input.Gamepad"/> associated with this event
	/// </summary>
	/// <value>
	/// The <see cref="Joystick.Id">ID</see> of the <see cref="Input.Gamepad"/> associated with this event
	/// </value>
	public uint GamepadId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWhich;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWhich = value;
	}

	// TODO: Add a `Gamepad` property once the `Gamepad` type is implemented
	// Important note: `Gamepad`s are also `Joystick`s, but not all `Joystick`s are `Gamepad`s. This suggests that `Gamepad` should be a subclass of `Joystick`.
	// Also, this implies that `Gamepad` also use `Joystick` IDs, which is also what SDL does.

	/// <summary>
	/// Gets or sets the index of the touchpad on the gamepad
	/// </summary>
	/// <value>
	/// The index of the touchpad on the gamepad
	/// </value>
	public int Touchpad
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mTouchpad;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mTouchpad = value;
	}

	/// <summary>
	/// Gets or sets the index of the finger on the touchpad
	/// </summary>
	/// <value>
	/// The index of the finger on the touchpad
	/// </value>
	public int Finger
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mFinger;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mFinger = value;
	}

	/// <summary>
	/// Gets or sets the horizontal position of the finger on the touchpad
	/// </summary>
	/// <value>
	/// The horizontal position of the finger on the touchpad, normalized between <c>0</c> and <c>1</c>, where <c>0</c> is the left edge and <c>1</c> is the right edge
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException">
	/// When setting this property, the given value is less than <c>0</c> or greater than <c>1</c>
	/// </exception>
	public float X
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mX;
		
		set
		{
			if (value is < 0f or > 1f)
			{
				[DoesNotReturn]
				static void failInvalidX(float x) => throw new ArgumentOutOfRangeException(nameof(value), x, $"The {nameof(X)} property must be between 0 and 1, inclusive.");

				failInvalidX(value);
			}

			mX = value;
		}
	}

	/// <summary>
	/// Gets or sets the vertical position of the finger on the touchpad
	/// </summary>
	/// <value>
	/// The vertical position of the finger on the touchpad, normalized between <c>0</c> and <c>1</c>, where <c>0</c> is the top edge and <c>1</c> is the bottom edge
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException">
	/// When setting this property, the given value is less than <c>0</c> or greater than <c>1</c>
	/// </exception>
	public float Y
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mY;

		set
		{
			if (value is < 0f or > 1f)
			{
				[DoesNotReturn]
				static void failInvalidY(float y) => throw new ArgumentOutOfRangeException(nameof(value), y, $"The {nameof(Y)} property must be between 0 and 1, inclusive.");

				failInvalidY(value);
			}

			mY = value;
		}
	}

	/// <summary>
	/// Gets or sets the pressure of the finger applied on the touchpad
	/// </summary>
	/// <value>
	/// The pressure the finger applies on the touchpad, normalized between <c>0</c> and <c>1</c>, where <c>0</c> is no pressure and <c>1</c> is maximum pressure
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

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {mCommon.ToPartialString()}, {
			nameof(GamepadId)}: {mWhich.ToString(format, formatProvider)}, {
			nameof(Touchpad)}: {mTouchpad.ToString(format, formatProvider)}, {
			nameof(Finger)}: {mFinger.ToString(format, formatProvider)}, {
			nameof(X)}: {mX.ToString(format, formatProvider)}, {
			nameof(Y)}: {mY.ToString(format, formatProvider)}, {
			nameof(Pressure)}: {mPressure.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(GamepadId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWhich, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Touchpad)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mTouchpad, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Finger)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mFinger, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(X)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mX, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Y)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mY, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Pressure)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mPressure, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
