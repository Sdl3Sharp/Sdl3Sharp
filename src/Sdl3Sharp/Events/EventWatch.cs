namespace Sdl3Sharp.Events;

/// <summary>
/// Represents a method that watches events as they are added to the event queue
/// </summary>
/// <param name="event">The event to watch</param>
/// <remarks>
/// <para>
/// SDL may call watch delegates at any time from any thread; the application is responsible for locking resources that need to be protected.
/// </para>
/// </remarks>
public delegate void EventWatch(ref Event @event);
