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
/// Represents a read-only reference to an event of unspecified type
/// </summary>
/// <param name="event">The event to reference</param>
/// <remarks>
/// <para>
/// <see cref="EventRefReadOnly"/>s can be safely turned into references to events of specific type using the <see cref="TryAsReadOnly{TEventData}(out EventRefReadOnly{TEventData})"/> method.
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
public readonly ref struct EventRefReadOnly(ref readonly Event @event) :
	IEquatable<EventRefReadOnly>, IFormattable, ISpanFormattable
{
	/// <summary>
	/// Gets a reference with no target event
	/// </summary>
	/// <value>
	/// A reference with no target event (i.e. <see cref="HasTarget"/> is <see langword="false"/>)
	/// </value>
	public static EventRefReadOnly Null { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new(ref Unsafe.NullRef<Event>()); }

	/// <exception cref="InvalidOperationException">The <see cref="EventRefReadOnly"/> does not refer to a target event (<see cref="HasTarget"/> is <see langword="false"/>)</exception>
	[DoesNotReturn]
	internal static void FailTargetNull() => throw new InvalidOperationException($"The {nameof(EventRefReadOnly)} does not refer to a target event.");

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private readonly ref readonly Event mEvent = ref @event;

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
	public readonly bool HasTarget { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => !Unsafe.IsNullRef(in mEvent); }

	/// <summary>
	/// Gets the <see cref="EventType">type</see> of the referenced event
	/// </summary>
	/// <value>
	/// The <see cref="EventType">type</see> of the referenced event
	/// </value>
	/// <inheritdoc cref="FailTargetNull"/>
	public readonly EventType Type { get => Target.Type; }

	internal readonly ref readonly Event Target
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
	/// Gets the timestamp of the referenced event
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
	public readonly ulong Timestamp { get => Target.Timestamp; }

	/// <summary>Calls to this method are not supported</summary>
	/// <param name="obj">Not supported</param>
	/// <returns>Not supported</returns>
	/// <exception cref="NotSupportedException">always</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete($"Calls to this method are not supported. This method will always throw an exception. Use the \"{nameof(Equals)}\" method or the equality operators instead.")]
	[DoesNotReturn]
#pragma warning disable CS0809
	public override bool Equals([NotNullWhen(true)] object? obj) => throw new NotSupportedException("Calls to this method are not supported.");
#pragma warning restore CS0809

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly bool Equals(EventRefReadOnly other) => Unsafe.AreSame(in mEvent, in other.mEvent);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly override int GetHashCode() { unsafe { return unchecked((nint)Unsafe.AsPointer(in mEvent)).GetHashCode(); } }

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

	/// <summary>
	/// Tries to treat the event reference as a read-only reference to an event of type <typeparamref name="TEventData"/>
	/// </summary>
	/// <typeparam name="TEventData">The type of the event to attempt to treat the event reference as</typeparam>
	/// <param name="eventRef">The resulting read-only reference to the event of type <typeparamref name="TEventData"/>, if this method returns <see langword="true"/>; otherwise, a reference with no target (i.e. <see cref="EventRefReadOnly{TEventData}.HasTarget"/> is <see langword="false"/>)</param>
	/// <returns><see langword="true"/>, if the event reference can be treated as a read-only reference to an event of type <typeparamref name="TEventData"/>; otherwise, <see langword="false"/></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly bool TryAsReadOnly<TEventData>(out EventRefReadOnly<TEventData> eventRef)
		where TEventData : unmanaged, IEventData<TEventData>
	{
		ref readonly var @event = ref Target;

		if (TEventData.AcceptsEventType(@event.Type))
		{
			eventRef = new(ref Unsafe.As<Event, Event<TEventData>>(ref Unsafe.AsRef(in @event)));
			return true;
		}

		eventRef = EventRefReadOnly<TEventData>.Null;
		return false;
	}

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
	public static bool operator ==(EventRefReadOnly left, EventRefReadOnly right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.operator!="/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator !=(EventRefReadOnly left, EventRefReadOnly right) => !(left == right);
}
