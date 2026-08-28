using Sdl3Sharp.Events;
using Sdl3Sharp.Internal;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents a <em>snapshot</em> of the state of a finger on a <see cref="TouchDevice"/> at a given point in time
/// </summary>
/// <param name="id">The numeric ID of the <see cref="Finger"/>, unique to the <see cref="TouchDevice"/></param>
/// <param name="x">The horizontal position of the <see cref="Finger"/> on the <see cref="TouchDevice"/></param>
/// <param name="y">The vertical position of the <see cref="Finger"/> on the <see cref="TouchDevice"/></param>
/// <param name="pressure">The pressure of the <see cref="Finger"/> applied on the <see cref="TouchDevice"/></param>
/// <remarks>
/// <para>
/// Please note that <see cref="Finger"/> is a <em>snapshot</em> of the state of a finger on a <see cref="TouchDevice"/> at a given point in time, as it's coming from <see cref="TouchDevice.Fingers"/>.
/// It's not meant to track the state of a finger over time.
/// If you want to track the state of the fingers on a <see cref="TouchDevice"/> over time, it's not recommended to repeatedly poll <see cref="TouchDevice.Fingers"/>, as that comes with a tremendous impact on performance.
/// You should instead listen to the <see cref="EventType.FingerDown"/>, <see cref="EventType.FingerUp"/>, <see cref="EventType.FingerMotion"/>, and <see cref="EventType.FingerCanceled"/> (<see cref="TouchFingerEvent"/>) events,
/// and implement your own tracking logic.
/// </para>
/// </remarks>
/// <exception cref="ArgumentOutOfRangeException"><paramref name="pressure"/> is less than <c>0</c> or greater than <c>1</c></exception>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization), SetsRequiredMembers]
public readonly struct Finger(ulong id, float x, float y, float pressure) :
	IEquatable<Finger>, IFormattable, ISpanFormattable, IEqualityOperators<Finger, Finger, bool>
{
	private static float ValidateBetweenZeroAndOne(float arg, [CallerArgumentExpression(nameof(arg))] string? argExpression = default)
	{
		if (arg is < 0.0f or > 1.0f)
		{
			[DoesNotReturn]
			static void failArgumentOutOfRange(float arg, string? argExpression) => throw new ArgumentOutOfRangeException(argExpression, arg, $"{argExpression} must be between 0.0 and 1.0.");

			failArgumentOutOfRange(arg, argExpression);
		}

		return arg;
	}

	private string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private readonly ulong mId = id;
	private readonly float mX = x;
	private readonly float mY = y;
	private readonly float mPressure = ValidateBetweenZeroAndOne(pressure);

	/// <summary>
	/// Gets or initializes the numeric ID of the finger, unique to the <see cref="TouchDevice"/>
	/// </summary>
	/// <value>
	/// The numeric ID of the finger, unique to the <see cref="TouchDevice"/>
	/// </value>
	/// <remarks>
	/// <para>
	/// The numeric ID of a finger is valid for the time the finger (or stylus) is touching the <see cref="TouchDevice"/> and will be unique across all fingers active on the <see cref="TouchDevice"/> at the same time.
	/// In other words, this ID also tracks the lifetime of a single continuous touch on the <see cref="TouchDevice"/> and will be different for each new touch.
	/// </para>
	/// <para>
	/// The value of this property may represent an index, a pointer, or some other unique ID, depending on the platform.
	/// </para>
	/// <para>
	/// An ID of <c>0</c> indicates an invalid finger.
	/// </para>
	/// </remarks>
	public required readonly ulong Id
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mId;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] init => mId = value;
	}

	/// <summary>
	/// Gets or initializes the horizontal position of the <see cref="Finger"/> on the <see cref="TouchDevice"/>
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
	public required readonly float X
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mX;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] init => mX = value;

	}

	/// <summary>
	/// Gets or initializes the vertical position of the <see cref="Finger"/> on the <see cref="TouchDevice"/>
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
	public required readonly float Y
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mY;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] init => mY = value;
	}

	/// <summary>
	/// Gets or initializes the pressure of the <see cref="Finger"/> applied on the <see cref="TouchDevice"/>
	/// </summary>
	/// <value>
	/// The pressure of the <see cref="Finger"/> applied on the <see cref="TouchDevice"/>, normalized between <c>0</c> and <c>1</c>, where <c>0</c> is no pressure and <c>1</c> is maximum pressure
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException">
	/// When initializing this property, the given value is less than <c>0</c> or greater than <c>1</c>
	/// </exception>
	public required readonly float Pressure
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mPressure;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] init => mPressure = ValidateBetweenZeroAndOne(value, nameof(Pressure));
	}

	/// <summary>
	/// Deconstructs the <see cref="Finger"/> into its components
	/// </summary>
	/// <param name="id">The numeric ID of the <see cref="Finger"/>, unique to the <see cref="TouchDevice"/></param>
	/// <param name="x">The horizontal position of the <see cref="Finger"/> on the <see cref="TouchDevice"/></param>
	/// <param name="y">The vertical position of the <see cref="Finger"/> on the <see cref="TouchDevice"/></param>
	/// <param name="pressure">The pressure of the <see cref="Finger"/> applied on the <see cref="TouchDevice"/></param>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly void Deconstruct(out ulong id, out float x, out float y, out float pressure)
	{
		id = mId;
		x = mX;
		y = mY;
		pressure = mPressure;
	}

	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is Finger other && Equals(other);

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly bool Equals(in Finger other)
		=> mId == other.mId
		&& mX == other.mX
		&& mY == other.mY
		&& mPressure == other.mPressure;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	readonly bool IEquatable<Finger>.Equals(Finger other) => Equals(other);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly override int GetHashCode() => HashCode.Combine(
		mId,
		mX,
		mY,
		mPressure
	);

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {nameof(Id)}: {mId.ToString(format, formatProvider)}, {
			nameof(X)}: {mX.ToString(format, formatProvider)}, {
			nameof(Y)}: {mY.ToString(format, formatProvider)}, {
			nameof(Pressure)}: {mPressure.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite($"{{ {nameof(Id)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mId, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(X)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mX, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Y)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mY, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Pressure)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mPressure, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.operator=="/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator ==(in Finger left, in Finger right) => left.Equals(right);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static bool IEqualityOperators<Finger, Finger, bool>.operator ==(Finger left, Finger right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.operator!="/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator !=(in Finger left, in Finger right) => !(left == right);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static bool IEqualityOperators<Finger, Finger, bool>.operator !=(Finger left, Finger right) => left != right;
}
