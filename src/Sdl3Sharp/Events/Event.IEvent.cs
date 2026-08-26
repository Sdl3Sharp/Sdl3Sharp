using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Events;

partial struct Event : IEvent<Event>
{
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static bool IEvent<Event>.TryReadFromEvent(ref readonly Event @event, out Event result)
	{
		// `Event` is the "base" union type for all events, so naturally it accepts any event

		result = @event;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	readonly void IEvent<Event>.WriteToEvent(ref Event @event) => @event = this;
}
