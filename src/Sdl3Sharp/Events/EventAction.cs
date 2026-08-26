namespace Sdl3Sharp.Events;

/// <summary>
/// Represents action to take in <see cref="EventQueue.TryPeepEvents(System.Span{Event}, EventAction, EventType, EventType, out int)"/>
/// </summary>
public enum EventAction
{
	/// <summary>Adds events to the event queue</summary>
	Add,

	/// <summary>Retrieves events from the event queue without removing them</summary>
	Peek,

	/// <summary>Retrieves and removes events from the event queue</summary>
	Get
}
