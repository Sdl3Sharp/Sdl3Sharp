using Sdl3Sharp.Internal;
using Sdl3Sharp.Timing;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events2;

/// <summary>
/// Represents a reference to an event with a specific type of event data
/// </summary>
/// <typeparam name="TEventData">The type of the event data</typeparam>
/// <param name="event">The event to reference</param>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public readonly ref struct EventRef<TEventData>(ref Event<TEventData> @event) :
	IEquatable<EventRef<TEventData>>, IEquatable<EventRefReadOnly>, IFormattable, ISpanFormattable
	where TEventData : unmanaged, IEventData<TEventData>
{
	/// <summary>
	/// Gets a reference with no target event
	/// </summary>
	/// <value>
	/// A reference with no target event (i.e. <see cref="HasTarget"/> is <see langword="false"/>)
	/// </value>
	public static EventRef<TEventData> Null { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new(ref Unsafe.NullRef<Event<TEventData>>()); }

	/// <exception cref="InvalidOperationException">The <see cref="EventRef{TEventData}"/> does not refer to a target event (<see cref="HasTarget"/> is <see langword="false"/>)</exception>
	[DoesNotReturn]
	internal static void FailTargetNull() => throw new InvalidOperationException($"The {nameof(EventRef<>)} does not refer to a target event.");

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private readonly ref Event<TEventData> mEvent = ref @event;

	/// <summary>
	/// Gets a value indicating whether the event reference refers to a target event
	/// </summary>
	/// <value>
	/// A value indicating whether the event reference refers to a target event
	/// </value>
	/// <remarks>
	/// <para>
	/// If this property returns <see langword="false"/>, you shouldn't attempt to use the event reference any further, except for comparisons and formatting,
	/// otherwise, an <see cref="InvalidOperationException"/> will be thrown.
	/// </para>
	/// </remarks>
	public readonly bool HasTarget { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => !Unsafe.IsNullRef(ref mEvent); }

	/// <summary>
	/// Gets or sets the <see cref="EventType">type</see> of the referenced event
	/// </summary>
	/// <value>
	/// The <see cref="EventType">type</see> of the referenced event
	/// </value>
	/// <remarks>
	/// <para>
	/// When setting this property, the given value must be compatible with the actual type of the referenced event's specific <typeparamref name="TEventData"/> type,
	/// otherwise an <see cref="ArgumentException"/> will be thrown.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentException">When setting this property, the given value is not compatible with the actual type of the referenced event's specific <typeparamref name="TEventData"/> type</exception>
	/// <inheritdoc cref="FailTargetNull"/>
	public EventType Type
	{
		readonly get => Target.Type;
		set => Target.Type = value;
	}

	/// <summary>
	/// Gets a runtime reference to the target event
	/// </summary>
	/// <value>
	/// A runtime reference to the target event
	/// </value>
	/// <remarks>
	/// <para>
	/// Do not attempt to access this property if the <see cref="HasTarget"/> property returns <see langword="false"/>, otherwise, an <see cref="InvalidOperationException"/> will be thrown.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="FailTargetNull"/>
	public readonly ref Event<TEventData> Target
	{
		get
		{
			if (!HasTarget)
			{
				FailTargetNull();
			}

			return ref mEvent;
		}
	}

	/// <summary>
	/// Gets or sets the timestamp of the referenced event
	/// </summary>
	/// <value>
	/// The timestamp of the referenced event
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property usually describes the time passed, in nanoseconds since the <see cref="Sdl(Sdl.BuildAction?)">initialization of SDL</see>.
	/// It can be properly populated by using <see cref="Timer.NanosecondTicks"/>.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="FailTargetNull"/>
	public ulong Timestamp
	{
		readonly get => Target.Timestamp;
		set => Target.Timestamp = value;
	}

	/// <summary>Calls to this method are not supported</summary>
	/// <param name="obj">Not supported</param>
	/// <returns>Not supported</returns>
	/// <exception cref="NotSupportedException">always</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete($"Calls to this method are not supported. This method will always throw an exception. Use the \"{nameof(Equals)}\" methods or the equality operators instead.")]
	[DoesNotReturn]
#pragma warning disable CS0809
	public override bool Equals([NotNullWhen(true)] object? obj) => throw new NotSupportedException("Calls to this method are not supported.");
#pragma warning restore CS0809

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly bool Equals(EventRef<TEventData> other) => Unsafe.AreSame(ref mEvent, ref other.mEvent);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly bool Equals(EventRefReadOnly other) => other.Equals(this);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly override int GetHashCode() { unsafe { return unchecked((nint)Unsafe.AsPointer(ref mEvent)).GetHashCode(); } }

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> HasTarget
			? mEvent.ToString(format, formatProvider)
			: "null";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return HasTarget
			? mEvent.TryFormat(destination, out charsWritten, format, provider)
			: SpanFormat.TryWrite("null", ref destination, ref charsWritten);
	}

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.operator=="/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator ==(EventRef<TEventData> left, EventRef<TEventData> right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.operator=="/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator ==(EventRef<TEventData> left, EventRefReadOnly right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.operator!="/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator !=(EventRef<TEventData> left, EventRef<TEventData> right) => !(left == right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.operator!="/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator !=(EventRef<TEventData> left, EventRefReadOnly right) => !(left == right);

	/// <summary>
	/// Converts a reference to an event with a specific type of event data to a reference to an event of unspecified type
	/// </summary>
	/// <param name="eventRef">The reference to the event with a specific type of event data to convert</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static implicit operator EventRef(EventRef<TEventData> eventRef) => new(ref Unsafe.As<Event<TEventData>, Event>(ref eventRef.mEvent));

	/// <summary>
	/// Converts a reference to an event with a specific type of event data to a read-only reference to an event of unspecified type
	/// </summary>
	/// <param name="eventRef">The reference to the event with a specific type of event data to convert</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static implicit operator EventRefReadOnly(EventRef<TEventData> eventRef) => new(ref Unsafe.As<Event<TEventData>, Event>(ref eventRef.mEvent));

	/// <summary>
	/// Converts a reference to an event with a specific type of event data to a read-only reference to an event with the same type of event data
	/// </summary>
	/// <param name="eventRef">The reference to the event with a specific type of event data to convert</param>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static implicit operator EventRefReadOnly<TEventData>(EventRef<TEventData> eventRef) => new(ref eventRef.mEvent);
}
