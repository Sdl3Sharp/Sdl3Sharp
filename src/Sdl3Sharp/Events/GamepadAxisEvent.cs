using Sdl3Sharp.Input;
using Sdl3Sharp.Internal;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when a gamepad axis changes
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.GamepadAxisMotion"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct GamepadAxisEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWhich;
	private byte mAxis;
	private readonly byte mPadding1, mPadding2, mPadding3;
	private short mValue;
	private readonly ushort mPadding4;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="GamepadAxisEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(GamepadAxisEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the gamepad axis that changed
	/// </summary>
	/// <value>
	/// The gamepad axis that changed
	/// </value>
	public GamepadAxis Axis
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => unchecked((GamepadAxis)mAxis);
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mAxis = unchecked((byte)value);
	}

	/// <summary>
	/// Gets or sets the value for the <see cref="Axis"/> that changed
	/// </summary>
	/// <value>
	/// The value for the <see cref="Axis"/> that changed, in the range from <c><see cref="short.MinValue"/></c> to <c><see cref="short.MaxValue"/></c> and centered within around <c>8000</c> of <c>0</c> for thumbsticks,
	/// and in the range from <c>0</c> to <c><see cref="short.MaxValue"/></c> for triggers (note that this is not the same range that the corresponding joystick axis would have)
	/// </value>
	public short Value
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
			nameof(GamepadId)}: {mWhich.ToString(format, formatProvider)}, {
			nameof(Axis)}: {unchecked((GamepadAxis)mAxis)}, {
			nameof(Value)}: {mValue.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(GamepadId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWhich, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Axis)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(unchecked((GamepadAxis)mAxis), ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(Value)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mValue, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
