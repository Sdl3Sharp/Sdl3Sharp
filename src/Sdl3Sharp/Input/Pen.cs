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
/// Represents a pen input device associated with a pen surface device connected to the system
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Id"/> of a pen is unique, remains unchanged while the pen surface device, e.g., the device that the pen can touch, is connected to the system.
/// A pen leaving proximity (being taken far enough away from the pen surface device that it no longer responds) and then coming back should trigger <see cref="EventType.PenProximityIn"/> and <see cref="EventType.PenProximityOut"/> (<see cref="PenProximityEvent"/>) events,
/// but the <see cref="PenProximityEvent.Pen"/> should remain consistent, i.e., the <see cref="Id"/> should remain the same.
/// If a pen surface device is disconnected and then reconnected, it may cause future pen events to have different <see cref="Id"/>s, even for the same physical pen,
/// as SDL may not be able to determine that the hardware is the same.
/// </para>
/// <para>
/// Pens may provide more than simple touch input; they might have other axes, such as <see cref="PenAxis.Pressure">pressure</see>, <see cref="PenAxis.XTilt">tilt</see>, <see cref="PenAxis.Rotation">rotation</see>, etc.
/// </para>
/// <para>
/// Please note that various platforms vary in how and how well they support pen input.
/// If your pen supports some piece of functionality but SDL doesn't seem to, that might be caused by external platform-dependent factors like the operating system.
/// For example, some platforms can manage multiple devices at the same time, but others will make any connected pens look like a single logical device, much like how all USB mice connected to a computer will move the same system cursor.
/// Other platforms might not support pen buttons, or the distance axis, etc.
/// Very few platforms can even report what functionality the pen supports in the first place, so best practices is to either let the user configure their pens, or be prepared to handle new functionality for a pen the first time a respective event is received.
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct Pen :
	IEquatable<Pen>, IFormattable, ISpanFormattable, IEqualityOperators<Pen, Pen, bool>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private readonly uint mId;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal Pen(uint id) => mId = id;

#if SDL3_4_0_OR_GREATER

	/// <summary>
	/// Gets the type of surface device for this pen
	/// </summary>
	/// <value>
	/// The type of surface device for this pen, or <see cref="PenDeviceType.Invalid"/> if the type couldn't be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// Many platforms do not supply this information, so an application must always be prepared for the value of this property to be <see cref="PenDeviceType.Unknown"/>.
	/// </para>
	/// </remarks>
	public readonly PenDeviceType DeviceType => SDL_GetPenDeviceType(mId);

#endif

	/// <summary>
	/// Gets the numeric ID of this pen
	/// </summary>
	/// <value>
	/// The numeric ID of this pen
	/// </value>
	/// <remarks>
	/// <para>
	/// The <see cref="Id"/> of a pen is unique, remains unchanged while the pen surface device, e.g., the device that the pen can touch, is connected to the system.
	/// A pen leaving proximity (being taken far enough away from the pen surface device that it no longer responds) and then coming back should trigger <see cref="EventType.PenProximityIn"/> and <see cref="EventType.PenProximityOut"/> (<see cref="PenProximityEvent"/>) events,
	/// but the <see cref="PenProximityEvent.Pen"/> should remain consistent, i.e., the <see cref="Id"/> should remain the same.
	/// If a pen surface device is disconnected and then reconnected, it may cause future pen events to have different <see cref="Id"/>s, even for the same physical pen,
	/// as SDL may not be able to determine that the hardware is the same.
	/// </para>
	/// <para>
	/// An ID of <c>0</c> indicates an invalid pen.
	/// </para>
	/// </remarks>
	public readonly uint Id { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mId; }

	/// <summary>
	/// Gets the invalid <see cref="Pen"/> with an <see cref="Id"/> of <c>0</c>
	/// </summary>
	/// <value>
	/// The invalid <see cref="Pen"/> with an <see cref="Id"/> of <c>0</c>, i.e., <see cref="IsValid"/> will be <c><see langword="false"/></c> for this pen
	/// </value>
	public static Pen Invalid { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new(0); }

	/// <summary>
	/// Gets a value indicating whether this pen is valid
	/// </summary>
	/// <value>
	/// A value indicating whether this pen is valid, i.e., its <see cref="Id"/> is not <c>0</c>
	/// </value>
	public readonly bool IsValid { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mId is not 0; }

	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is Pen other && Equals(other);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly bool Equals(Pen other) => mId == other.mId;

	/// <summary>
	/// Gets a <see cref="Pen"/> by its numeric ID
	/// </summary>
	/// <param name="id">The numeric ID of the pen, or <c>0</c> to return <see cref="Invalid"/></param>
	/// <returns>The <see cref="Pen"/> with the specified <paramref name="id"/>, or <see cref="Invalid"/> if <paramref name="id"/> was <c>0</c></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static Pen FromId(uint id) => new(id);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly override int GetHashCode() => mId.GetHashCode();

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> mId is not 0
			? $"{{ {nameof(Id)}: {mId.ToString(format, formatProvider)} }}"
			: $"Invalid {nameof(Pen)}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
	{
		charsWritten = 0;

		if (mId is 0)
		{
			return SpanFormat.TryWrite($"Invalid {nameof(Pen)}", ref destination, ref charsWritten);
		}

		return SpanFormat.TryWrite($"{{ {nameof(Id)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mId, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator ==(Pen left, Pen right) => left.Equals(right);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator !=(Pen left, Pen right) => !(left == right);
}
