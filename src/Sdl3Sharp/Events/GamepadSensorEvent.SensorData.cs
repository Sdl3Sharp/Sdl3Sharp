using Sdl3Sharp.Internal;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Events;

partial struct GamepadSensorEvent
{
	/// <summary>
	/// Represents the sensor data of a <see cref="GamepadSensorEvent"/>
	/// </summary>
	[InlineArray(3)]
	public struct SensorData :
		IEquatable<SensorData>, IFormattable, ISpanFormattable, IEqualityOperators<SensorData, SensorData, bool>
	{
		private float _;

		/// <summary>
		/// Creates a new <see cref="SensorData"/> instance with the given sensor data values
		/// </summary>
		/// <param name="data0">The first sensor data value</param>
		/// <param name="data1">The second sensor data value</param>
		/// <param name="data2">The third sensor data value</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public SensorData(float data0, float data1, float data2)
		{
			this[0] = data0;
			this[1] = data1;
			this[2] = data2;
		}

		/// <inheritdoc/>
		public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is SensorData other && Equals(other);

		/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public readonly bool Equals(in SensorData other)
			=> this[0] == other[0]
			&& this[1] == other[1]
			&& this[2] == other[2];

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		readonly bool IEquatable<SensorData>.Equals(SensorData other) => Equals(other);

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public readonly override int GetHashCode() => HashCode.Combine(
			this[0],
			this[1],
			this[2]
		);

		/// <inheritdoc/>
		public readonly override string ToString() => ToString(format: default, formatProvider: default);

		/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
		public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

		/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
		public readonly string ToString(string? format) => ToString(format, formatProvider: default);

		/// <inheritdoc/>
		public readonly string ToString(string? format, IFormatProvider? formatProvider)
			=> $"[ {this[0].ToString(format, formatProvider)}, {
				this[1].ToString(format, formatProvider)}, {
				this[2].ToString(format, formatProvider)} ]";

		/// <inheritdoc/>
		public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
		{
			charsWritten = 0;

			return SpanFormat.TryWrite("[ ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(this[0], ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite(", ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(this[1], ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite(", ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(this[2], ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite(" ]", ref destination, ref charsWritten);
		}

		/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.operator=="/>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static bool operator ==(in SensorData left, in SensorData right) => left.Equals(in right);

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		static bool IEqualityOperators<SensorData, SensorData, bool>.operator ==(SensorData left, SensorData right) => left == right;

		/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.operator!="/>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public static bool operator !=(in SensorData left, in SensorData right) => !(left == right);

		/// <inheritdoc/>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		static bool IEqualityOperators<SensorData, SensorData, bool>.operator !=(SensorData left, SensorData right) => left != right;
	}
}
