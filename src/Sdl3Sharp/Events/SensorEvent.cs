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
/// Represents an event that occurs when a <see cref="Input.Sensor"/> is updated
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.SensorUpdated"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct SensorEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWhich;
	private SensorData mData;
	private ulong mSensorTimestamp;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="SensorEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(SensorEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the <see cref="Sensor.Id">ID</see> of the <see cref="Input.Sensor"/> associated with this event
	/// </summary>
	/// <value>
	/// The <see cref="Sensor.Id">ID</see> of the <see cref="Input.Sensor"/> associated with this event
	/// </value>
	public uint SensorId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWhich;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWhich = value;
	}

	/// <summary>
	/// Gets or sets the data from the sensor
	/// </summary>
	/// <value>
	/// The data from the sensor
	/// </value>
	/// <remarks>
	/// <para>
	/// If you need more data from the sensor (i.e., the sensor provides more than six <c><see cref="float"/></c> values),
	/// you can use the <see cref="Sensor.TryGetData(Span{float})"/> method to get the data from the <see cref="TryOpenSensor(out Sensor?)">opened sensor</see>.
	/// </para>
	/// </remarks>
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

	/// <summary>
	/// Tries to open the <see cref="Input.Sensor"/> associated with this event
	/// </summary>
	/// <param name="sensor">The opened sensor, or <c><see langword="null"/></c> if the sensor could not be opened</param>
	/// <returns><c><see langword="true"/></c> if the sensor was successfully opened; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// It is safe to open the same sensor multiple times, even if it was already opened by another component.
	/// </para>
	/// <para>
	/// Note that you should <em>always</em> <see cref="Sensor.Dispose()">dispose and close</see> a sensor you opened yourself when you don't need it anymore.
	/// You don't need to worry about other components that might have opened the same sensor, as the sensor is reference counted and will only be finally closed and released once all references to it have been disposed.
	/// </para>
	/// </remarks>
	public readonly bool TryOpenSensor([NotNullWhen(true)] out Sensor? sensor) => Sensor.TryOpenFromId(SensorId, out sensor);

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {mCommon.ToPartialString()}, {
			nameof(SensorId)}: {mWhich.ToString(format, formatProvider)}, {
			nameof(Data)}: {mData.ToString(format, formatProvider)}, {
			nameof(SensorTimestamp)}: {CommonEvent.FormatTimestamp(mSensorTimestamp)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(SensorId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWhich, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Data)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(in mData, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(SensorTimestamp)}: ", ref destination, ref charsWritten)
			&& CommonEvent.TryFormatTimestamp(mSensorTimestamp, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
