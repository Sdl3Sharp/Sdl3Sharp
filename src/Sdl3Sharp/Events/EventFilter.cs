namespace Sdl3Sharp.Events;

/// <summary>
/// Represents a method that filters events in the event queue
/// </summary>
/// <param name="event">The event to examine</param>
/// <returns><c><see langword="true"/></c> to permit the event, <c><see langword="false"/></c> to reject it</returns>
/// <remarks>
/// <para>
/// SDL may call filter delegates at any time from any thread; the application is responsible for locking resources that need to be protected.
/// </para>
/// </remarks>
public delegate bool EventFilter(ref Event @event);
