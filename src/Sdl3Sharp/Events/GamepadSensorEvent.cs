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
/// Represents an event that occurs when a sensor on a gamepad is updated
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.GamepadSensorUpdated"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct GamepadSensorEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWhich;
	private SensorType mSensor;
	private SensorData mData;
	private ulong mSensorTimestamp;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="GamepadSensorEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(GamepadSensorEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the type of gamepad sensor
	/// </summary>
	/// <value>
	/// The type of gamepad sensor
	/// </value>
	public SensorType SensorType
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mSensor;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mSensor = value;
	}

	/// <summary>
	/// Gets or sets the data from the sensor
	/// </summary>
	/// <value>
	/// The data from the sensor
	/// </value>
	public SensorData Data
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mData;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mData = value;
	}

	/// <summary>
	/// Gets or sets the timestamp of the sensor reading
	/// </summary>
	/// <value>
	/// The timestamp of the sensor reading, in nanoseconds
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property is not necessarily synchronized with the system's clock.
	/// </para>
	/// </remarks>
	public ulong SensorTimestamp
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mSensorTimestamp;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mSensorTimestamp = value;
	}

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {mCommon.ToPartialString()}, {nameof(GamepadId)}: {mWhich.ToString(format, formatProvider)}, {
			nameof(SensorType)}: {mSensor}, {
			nameof(Data)}: {mData.ToString(format, formatProvider)}, {
			nameof(SensorTimestamp)}: {CommonEvent.FormatTimestamp(mSensorTimestamp)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(GamepadId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWhich, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(SensorType)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mSensor, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(Data)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(in mData, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(SensorTimestamp)}: ", ref destination, ref charsWritten)
			&& CommonEvent.TryFormatTimestamp(mSensorTimestamp, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
