using Sdl3Sharp.Timing;
using System.Diagnostics.CodeAnalysis;

namespace Sdl3Sharp.Events;

/// <summary>
/// A commonly shared interface implemented by all event type structures
/// </summary>
/// <typeparam name="TSelf">The type of the event structure implementing this interface</typeparam>
public interface IEvent<TSelf>
	where TSelf : notnull, IEvent<TSelf>
{
	/// <summary>
	/// Gets or sets the <see cref="EventType"/> of this event
	/// </summary>
	/// <value>
	/// The <see cref="EventType"/> of this event
	/// </value>
	EventType Type { get; set; }

	/// <summary>
	/// Gets or sets the timestamp of this event
	/// </summary>
	/// <value>
	/// The timestamp of this event, in nanoseconds since the <see cref="Sdl(Sdl.BuildAction?)">initialization of SDL</see>
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property usually describes the time passed, in nanoseconds since the <see cref="Sdl(Sdl.BuildAction?)">initialization of SDL</see>.
	/// </para>
	/// <para>
	/// To properly populate this property, you can use <see cref="Timer.NanosecondTicks"/>.
	/// </para>
	/// </remarks>
	ulong Timestamp { get; set; }

	internal static abstract bool TryReadFromEvent(ref readonly Event @event, [NotNullWhen(true)] out TSelf? result);

	internal void WriteToEvent(ref Event @event);
}
