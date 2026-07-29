#if SDL3_4_0_OR_GREATER

using Sdl3Sharp.Internal;
using Sdl3Sharp.Video;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents a single frame of an animated cursor
/// </summary>
/// <param name="surface">The <see cref="Surface"/> used as image data for this cursor frame</param>
/// <param name="durationMilliseconds">The duration in milliseconds for which this cursor frame should be displayed, or <c>0</c> if the cursor frame should be displayed indefinitely</param>
/// <exception cref="ArgumentNullException"><paramref name="surface"/> is <c>null</c></exception>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
[method: SetsRequiredMembers, MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public readonly struct CursorFrameInfo(Surface surface, uint durationMilliseconds = 0) :
	IEquatable<CursorFrameInfo>, IFormattable, ISpanFormattable, IEqualityOperators<CursorFrameInfo, CursorFrameInfo, bool>
{
	[return: NotNull]
	private unsafe static Surface.SDL_Surface* ValidateSurface([NotNull] Surface surface, [CallerArgumentExpression(nameof(surface))] string? surfaceExpression = default)
	{
		if (surface is null)
		{
			[DoesNotReturn]
			static void failSurfaceArgumentNull(string? surfaceExpression) => throw new ArgumentNullException(surfaceExpression, $"{surfaceExpression} cannot be null");

			failSurfaceArgumentNull(surfaceExpression);
		}

		return surface.Pointer;
	}

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private unsafe readonly Surface.SDL_Surface* mSurface = ValidateSurface(surface);
	private readonly uint mDurationMilliseconds = durationMilliseconds;

	/// <summary>
	/// Gets the <see cref="Surface"/> used as image data for this cursor frame
	/// </summary>
	/// <value>
	/// The <see cref="Surface"/> used as image data for this cursor frame
	/// </value>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <c>null</c></exception>
#pragma warning disable CS8618 // I believe this is fine to suppress for members of value types (also it is already marked as `required`, so that's a bug in the analyzer)
	public required readonly Surface Surface
#pragma warning restore CS8618
	{
		get
		{
			unsafe
			{
				Surface.TryGetOrCreate(mSurface, out var surface);
				return surface!;
			}
		}

		init
		{
			unsafe
			{
				mSurface = ValidateSurface(value, nameof(Surface));
			}
		}
	}

	/// <summary>
	/// Gets the duration in milliseconds for which this cursor frame should be displayed
	/// </summary>
	/// <value>
	/// The duration in milliseconds for which this cursor frame should be displayed, or <c>0</c> if the cursor frame should be displayed indefinitely
	/// </value>
	public readonly uint DurationMilliseconds
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mDurationMilliseconds;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] init => mDurationMilliseconds = value;
	}

	/// <summary>
	/// Gets a value indicating whether this cursor frame should be displayed indefinitely
	/// </summary>
	/// <value>
	/// A value indicating whether this cursor frame should be displayed indefinitely
	/// </value>
	/// <remarks>
	/// <para>
	/// If the value of this property is <c><see langword="true"/></c>, then the cursor frame should be displayed indefinitely.
	/// It also means that the value of <see cref="DurationMilliseconds"/> is <c>0</c>.
	/// </para>
	/// </remarks>
	public readonly bool IsInfinite { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mDurationMilliseconds is 0; }

	/// <summary>
	/// Deconstructs the <see cref="CursorFrameInfo"/> into its associated <see cref="Surface"/> and duration in milliseconds
	/// </summary>
	/// <param name="surface">The <see cref="Surface"/> used as image data for this cursor frame</param>
	/// <param name="durationMilliseconds">The duration in milliseconds for which this cursor frame should be displayed, or <c>0</c> if the cursor frame should be displayed indefinitely</param>
	public readonly void Deconstruct(out Surface surface, out uint durationMilliseconds)
	{
		surface = Surface;
		durationMilliseconds = DurationMilliseconds;
	}

	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is CursorFrameInfo other && Equals(other);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly bool Equals(CursorFrameInfo other)
	{
		unsafe
		{
			return mSurface == other.mSurface
				&& mDurationMilliseconds == other.mDurationMilliseconds;
		}
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly override int GetHashCode()
	{
		unsafe
		{
			return HashCode.Combine(
				unchecked((IntPtr)mSurface),
				mDurationMilliseconds
			);
		}
	}

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)" />
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)" />
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {nameof(Surface)}: {Surface switch { null => "null", var surface => $"{surface}" }}, {
			nameof(DurationMilliseconds)}: {DurationMilliseconds.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite($"{{ {nameof(Surface)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(Surface switch { null => "null", var surface => $"{surface}" }, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(DurationMilliseconds)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(DurationMilliseconds, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator ==(CursorFrameInfo left, CursorFrameInfo right) => left.Equals(right);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator !=(CursorFrameInfo left, CursorFrameInfo right) => !(left == right);
}

#endif
