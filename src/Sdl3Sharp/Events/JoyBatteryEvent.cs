using Sdl3Sharp.Internal;
using Sdl3Sharp.Utilities;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when a joystick's battery status changes
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.JoystickBatteryUpdated"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct JoyBatteryEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWhich;
	private PowerState mState;
	private int mPercent;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="JoyBatteryEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(JoyBatteryEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the <see cref="Joystick.Id">ID</see> of the <see cref="Input.Joystick"/> associated with this event
	/// </summary>
	/// <value>
	/// The <see cref="Joystick.Id">ID</see> of the <see cref="Input.Joystick"/> associated with this event
	/// </value>
	public uint JoystickId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWhich;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWhich = value;
	}

	// TODO: Add a `Joystick` property once the `Joystick` type is implemented

	/// <summary>
	/// Gets or sets the current power state of the joystick battery
	/// </summary>
	/// <value>
	/// The current power state of the joystick battery
	/// </value>
	public PowerState State
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mState;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mState = value;
	}

	/// <summary>
	/// Gets or sets the percentage of battery life remaining for the joystick
	/// </summary>
	/// <value>
	/// The percentage of battery life remaining for the joystick
	/// </value>
	public int Percentage
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mPercent;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mPercent = value;
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
			nameof(JoystickId)}: {mWhich.ToString(format, formatProvider)}, {
			nameof(State)}: {State}, {
			nameof(Percentage)}: {Percentage.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(JoystickId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWhich, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(State)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(State.ToString(), ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(Percentage)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(Percentage, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
