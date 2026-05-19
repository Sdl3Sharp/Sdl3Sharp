using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Events2;

/// <summary>
/// Provides extension methods and properties for <see cref="Event"/>, <see cref="Event{TEventData}"/>, <see cref="EventRef"/>, <see cref="EventRef{TEventData}"/>, <see cref="EventRefReadOnly"/>, and <see cref="EventRefReadOnly{TEventData}"/>
/// </summary>
public static partial class EventExtensions
{
	extension(ref Event @event)
	{
		/// <summary>
		/// Creates an <see cref="EventRef"/> from a runtime reference to an <see cref="Event"/>
		/// </summary>
		/// <returns>The <see cref="EventRef"/> referencing the same <see cref="Event"/> as the given runtime reference</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public EventRef AsRef() => new(ref @event);

		/// <summary>
		/// Tries to create an <see cref="EventRef{TEventData}"/> from a runtime reference to an <see cref="Event"/> by treating it as an event of type <typeparamref name="TEventData"/>
		/// </summary>
		/// <typeparam name="TEventData">The type of the event to attempt to treat the given event as</typeparam>
		/// <param name="eventRef">The resulting reference to the event of type <typeparamref name="TEventData"/>, if this method returns <see langword="true"/>; otherwise, a reference with no target (i.e. <see cref="EventRef{TEventData}.HasTarget"/> is <see langword="false"/>)</param>
		/// <returns><see langword="true"/>, if the event can be treated as an event of type <typeparamref name="TEventData"/>; otherwise, <see langword="false"/></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public bool TryAsRef<TEventData>(out EventRef<TEventData> eventRef)
			where TEventData : unmanaged, IEventData<TEventData>
			=> @event.AsRef().TryAs(out eventRef);
	}

	extension(ref readonly Event @event)
	{
		/// <summary>
		/// Creates an <see cref="EventRefReadOnly"/> from a read-only runtime reference to an <see cref="Event"/>
		/// </summary>
		/// <returns>The <see cref="EventRefReadOnly"/> referencing the same <see cref="Event"/> as the given read-only runtime reference</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public EventRefReadOnly AsRefReadOnly() => new(in @event);

		/// <summary>
		/// Tries to create an <see cref="EventRefReadOnly{TEventData}"/> from a read-only runtime reference to an <see cref="Event"/> by treating it as an event of type <typeparamref name="TEventData"/>
		/// </summary>
		/// <typeparam name="TEventData">The type of the event to attempt to treat the given event as</typeparam>
		/// <param name="eventRef">The resulting read-only reference to the event of type <typeparamref name="TEventData"/>, if this method returns <see langword="true"/>; otherwise, a reference with no target (i.e. <see cref="EventRefReadOnly{TEventData}.HasTarget"/> is <see langword="false"/>)</param>
		/// <returns><see langword="true"/>, if the event can be treated as an event of type <typeparamref name="TEventData"/>; otherwise, <see langword="false"/></returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		public bool TryAsRefReadOnly<TEventData>(out EventRefReadOnly<TEventData> eventRef)
			where TEventData : unmanaged, IEventData<TEventData>
			=> @event.AsRefReadOnly().TryAsReadOnly(out eventRef);
	}
}
