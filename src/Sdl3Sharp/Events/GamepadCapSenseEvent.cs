#if SDL3_6_0_OR_GREATER

using Sdl3Sharp.Input;
using Sdl3Sharp.Internal;
using Sdl3Sharp.Internal.Interop;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when a capacitive sensing on a gamepad is activated or deactivated
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.GamepadCapSenseTouched"/></description></item>
/// <item><description><see cref="EventType.GamepadCapSenseReleased"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct GamepadCapSenseEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWhich;
	private byte mCapsense;
	private CBool mDown;
	private readonly byte mPadding1, mPadding2;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="GamepadCapSenseEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(GamepadCapSenseEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the type of capacitive sensing on the gamepad
	/// </summary>
	/// <value>
	/// The type of capacitive sensing on the gamepad
	/// </value>
	public GamepadCapSenseType CapSense
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => unchecked((GamepadCapSenseType)mCapsense);
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mCapsense = unchecked((byte)value);
	}

	/// <summary>
	/// Gets or sets a value indicating whether the <see cref="CapSense"/> is activated (e.g., currently touched or gripped)
	/// </summary>
	/// <value>
	/// A value indicating whether the <see cref="CapSense"/> is activated (e.g., currently touched or gripped)
	/// </value>
	public bool IsDown
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mDown;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mDown = value;
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
			nameof(CapSense)}: {unchecked((GamepadCapSenseType)mCapsense)}, {
			nameof(IsDown)}: {(bool)mDown} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(GamepadId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWhich, ref destination, ref charsWritten, format)
			&& SpanFormat.TryWrite($", {nameof(CapSense)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(unchecked((GamepadCapSenseType)mCapsense), ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(IsDown)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite((bool)mDown, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}

#endif
