using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using unsafe SDL_EventFilter = delegate* unmanaged[Cdecl]<void*, Sdl3Sharp.Events.Event*, Sdl3Sharp.Internal.Interop.CBool>;

namespace Sdl3Sharp.Events;

/// <summary>
/// Provides methods and properties for managing SDL's event queue
/// </summary>
public static partial class EventQueue
{
	private static GCHandle mEventFilterHandle = default;

	/// <summary>
	/// Gets or sets the global event filter
	/// </summary>
	/// <value>
	/// The global event filter
	/// </value>
	/// <remarks>
	/// <para>
	/// The global event filter is used for filtering all events before they get pushed onto SDL's event queue.
	/// </para>
	/// <para>
	/// Returning <c><see langword="false"/></c> from the filter will prevent the event from being pushed onto the event queue.
	/// </para>
	/// <para>
	/// The filter is called for every event that gets pushed onto the event queue using <see cref="TryPushEvent(in Event)"/>,
	/// but events added to the queue using <see cref="TryAddEvents(ReadOnlySpan{Event}, out int)"/> bypass the filter.
	/// </para>
	/// <para>
	/// <see cref="EventTypeExtensions.set_Enabled(EventType, bool)">Disabled</see> events will never be passed to the filter nor will they be added to the event queue.
	/// </para>
	/// <para>
	/// You can use this property to chain multiple filters together by getting the current filter as a delegate, and then setting a new filter delegate that calls the previous filter as part of its implementation.
	/// </para>
	/// <para>
	/// Note that you should be very careful of what you do in the event filter, as it may run in a different thread!
	/// The exception to that is the handling of <see cref="EventType.WindowExposed"/>, which is guaranteed to be sent from the OS on the main thread and you are expected to redraw your window in response to this event.
	/// </para>
	/// </remarks>
	public static EventFilter? EventFilter
	{
		get
		{
			unsafe			
			{
				SDL_EventFilter filter;
				void* userdata;

				if (!SDL_GetEventFilter(&filter, &userdata) || filter is null)
				{
					return null;
				}

				if (userdata is not null && GCHandle.FromIntPtr(unchecked((IntPtr)userdata)) is { IsAllocated: true, Target: EventFilter eventFilter })
				{
					// The set event filter is a event filter we've set from managed side at some point, so we can simply return it

					return eventFilter;
				}

				// This seems to be an event filter that was not set from the managed side, so we create a local function that captures and wraps the unmanaged event filter in order to return it as a managed delegate

				// We need to copy the locals that are being captured to local function, since in C#, local variables can't have their address taken and captured by a local function at the same time
				var filterCapture = filter;
				var userdataCapture = userdata;

				bool wrappedEventFilter(ref Event @event)
				{
					fixed (Event* eventPtr = &@event)
					{
						return filterCapture(userdataCapture, eventPtr);
					}
				}

				return wrappedEventFilter;
			}
		}

		set
		{
			unsafe
			{
				if (mEventFilterHandle.IsAllocated)
				{
					// If there was a previous managed event filter set, we need to release the old handle in any case

					mEventFilterHandle.Free();
					mEventFilterHandle = default;
				}

				if (value is null)
				{
					// Just clear the event filter; no need to activate `DisposeReceiver`, since the old handle was already released above (in case there was one), and we don't set a new one

					SDL_SetEventFilter(null, null);
				}
				else
				{
					// We need to activate `DisposeReceiver` to clean up the handle when SDL is shutting down

					DisposeReceiver.Activate();

					mEventFilterHandle = GCHandle.Alloc(value, GCHandleType.Normal);

					SDL_SetEventFilter(&EventFilterImpl, unchecked((void*)GCHandle.ToIntPtr(mEventFilterHandle)));
				}
			}
		}
	}

	private static EventWatchWrapper? mEventWatchWrapper;

	/// <summary>
	/// Occurs when an event is pushed onto the event queue
	/// </summary>
	/// <remarks>
	/// <para>
	/// The event handler to this event is called for every event that gets pushed onto SDL's event queue using <see cref="TryPushEvent(in Event)"/>,
	/// but not for <see cref="EventTypeExtensions.set_Enabled(EventType, bool)">disabled</see> events,
	/// nor for events that were filtered out by the <see cref="EventFilter"/>,
	/// nor for events added to the queue using <see cref="TryAddEvents(ReadOnlySpan{Event}, out int)"/>.
	/// </para>
	/// <para>
	/// Note that you should be very careful of what you do in the event handler to this event, as it may run in a different thread!
	/// </para>
	/// </remarks>
	public static event EventWatch? EventWatch
	{
		add
		{
			if (value is null)
			{
				return;
			}

			DisposeReceiver.Activate();

			mEventWatchWrapper ??= new();
			mEventWatchWrapper.Watch += value;
		}

		remove
		{
			if (value is null || mEventWatchWrapper is null)
			{
				return;
			}
			
			mEventWatchWrapper.Watch -= value;

			if (mEventWatchWrapper.Watch is null)
			{
				mEventWatchWrapper.Dispose();
				mEventWatchWrapper = null;
			}
		}
	}

	private static void DisposeFromSdl()
	{
		if (mEventFilterHandle.IsAllocated)
		{
			mEventFilterHandle.Free();
			mEventFilterHandle = default;
		}

		mEventWatchWrapper?.Dispose();
		mEventWatchWrapper = null;
	}

	/// <summary>
	/// Filters events on the current event queue using the specified filter
	/// </summary>
	/// <param name="filter">The event filter to apply</param>
	/// <remarks>
	/// <para>
	/// Unlike the <see cref="EventFilter"/> property, this method only applies the filter <em>once</em> to the events that are currently in the event queue and potentially removes them based on that.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="filter"/> is <c><see langword="null"/></c></exception>
	public static void FilterEvents(EventFilter filter)
	{
		unsafe
		{
			if (filter is null)
			{
				[DoesNotReturn]
				static void failFilterArgumentNull() => throw new ArgumentNullException(nameof(filter));

				failFilterArgumentNull();
			}

			var eventFilterHandle = GCHandle.Alloc(filter, GCHandleType.Normal);
			try
			{
				SDL_FilterEvents(&EventFilterImpl, unchecked((void*)GCHandle.ToIntPtr(eventFilterHandle))); // resuing the existing `EventFilterImpl` logic
			}
			finally
			{
				eventFilterHandle.Free();
			}
		}
	}

	/// <summary>
	/// Clears all events from the current event queue
	/// </summary>
	/// <remarks>
	/// <para>
	/// This will unconditionally remove any events from the event queue.
	/// If you need to remove specific events, use <see cref="FlushEvents(EventType)"/> or <see cref="FlushEvents(EventType, EventType)"/> instead.
	/// </para>
	/// <para>
	/// You don't necessarily need to call this method. It is perfectly fine to just ignore events you don't care about in your event loop.
	/// </para>
	/// <para>
	/// This method only affects currently queued events.
	/// If you want to make sure that all pending OS events are flushed, you can call <see cref="PumpEvents"/> on the main thread immediately before calling this method.
	/// </para>
	/// <para>
	/// If you have user defined events with custom data that needs to be handled (e.g. disposed),
	/// you should use <see cref="TryGetEvents(Span{Event}, EventType, EventType, out int)"/> or <see cref="TryPeekEvents(Span{Event}, EventType, EventType, out int)"/>
	/// to retrieve and handle those events before flushing.
	/// </para>
	/// </remarks>
	public static void FlushEvents()
#pragma warning disable CS0618 // We can use them here, that's what they are there for
		=> SDL_FlushEvents(EventType.First, EventType.Last);
#pragma warning restore CS0618

	/// <summary>
	/// Clears events of the specified type from the current event queue
	/// </summary>
	/// <param name="type">The type of events to clear</param>
	/// <remarks>
	/// <para>
	/// This will unconditionally remove any events from the event queue that match the specified <paramref name="type"/>.
	/// If you need to remove a specific range of event types, use <see cref="FlushEvents(EventType, EventType)"/> instead.
	/// </para>
	/// <para>
	/// You don't necessarily need to call this method. It is perfectly fine to just ignore events you don't care about in your event loop.
	/// </para>
	/// <para>
	/// This method only affects currently queued events.
	/// If you want to make sure that all pending OS events are flushed, you can call <see cref="PumpEvents"/> on the main thread immediately before calling this method.
	/// </para>
	/// <para>
	/// If you have user defined events with custom data that needs to be handled (e.g. disposed),
	/// you should use <see cref="TryGetEvents(Span{Event}, EventType, EventType, out int)"/> or <see cref="TryPeekEvents(Span{Event}, EventType, EventType, out int)"/>
	/// to retrieve and handle those events before flushing.
	/// </para>
	/// </remarks>
	public static void FlushEvents(EventType type)
		=> SDL_FlushEvent(type);

	/// <summary>
	/// Clears events in the specified type range from the current event queue
	/// </summary>
	/// <param name="minType">The inclusive lower bound of the event type range to be cleared</param>
	/// <param name="maxType">The inclusive upper bound of the event type range to be cleared</param>
	/// <remarks>
	/// <para>
	/// This will unconditionally remove any events from the event queue in the range between <paramref name="minType"/> and <paramref name="maxType"/>, inclusive.
	/// If you need to remove a specific single event type only, use <see cref="FlushEvents(EventType)"/> instead.
	/// </para>
	/// <para>
	/// You don't necessarily need to call this method. It is perfectly fine to just ignore events you don't care about in your event loop.
	/// </para>
	/// <para>
	/// This method only affects currently queued events.
	/// If you want to make sure that all pending OS events are flushed, you can call <see cref="PumpEvents"/> on the main thread immediately before calling this method.
	/// </para>
	/// <para>
	/// If you have user defined events with custom data that needs to be handled (e.g. disposed),
	/// you should use <see cref="TryGetEvents(Span{Event}, EventType, EventType, out int)"/> or <see cref="TryPeekEvents(Span{Event}, EventType, EventType, out int)"/>
	/// to retrieve and handle those events before flushing.
	/// </para>
	/// </remarks>
	public static void FlushEvents(EventType minType, EventType maxType)
		=> SDL_FlushEvents(minType, maxType);

	/// <summary>
	/// Determines whether there are any events in the current event queue
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if there are events in the queue; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// If you need to check for specific event types, use <see cref="HasEvents(EventType)"/> or <see cref="HasEvents(EventType, EventType)"/> instead.
	/// </para>
	/// </remarks>
	public static void HasEvents()
#pragma warning disable CS0618 // We can use them here, that's what they are there for
		=> SDL_HasEvents(EventType.First, EventType.Last);
#pragma warning restore CS0618

	/// <summary>
	/// Determines whether there are any events of the specified type in the current event queue
	/// </summary>
	/// <param name="type">The type of the event to check for</param>
	/// <returns><c><see langword="true"/></c>, if there are events of the specified type in the queue; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// If you need to check for a specific range of event types, use <see cref="HasEvents(EventType, EventType)"/> instead.
	/// </para>
	/// </remarks>
	public static void HasEvents(EventType type)
		=> SDL_HasEvent(type);

	/// <summary>
	/// Determines whether there are any events in the specified type range in the current event queue
	/// </summary>
	/// <param name="minType">The inclusive lower bound of the event type range to check</param>
	/// <param name="maxType">The inclusive upper bound of the event type range to check</param>
	/// <returns><c><see langword="true"/></c>, if there are events in the specified type range; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// If you need to check for a specific single event type only, use <see cref="HasEvents(EventType)"/> instead.
	/// </para>
	/// </remarks>
	public static void HasEvents(EventType minType, EventType maxType)
		=> SDL_HasEvents(minType, maxType);

	/// <summary>
	/// Pumps the event queue, gathering events from the input devices
	/// </summary>
	/// <remarks>
	/// <para>
	/// This method updates the event queue and internal input device state.
	/// </para>
	/// <para>
	/// This method gathers all the pending input information from devices and places it in the event queue.
	/// Without calls to <see cref="PumpEvents"/> no events would ever be placed on the queue.
	/// Often the need for calls to <see cref="PumpEvents"/> is hidden from the user since methods like <see cref="TryPollEvent()"/> and <see cref="TryWaitForEvent(out Event)"/> implicitly call <see cref="PumpEvents"/>.
	/// However, if you are not polling or waiting for events (e.g. you are filtering them), then you must call <see cref="PumpEvents"/> to force an event queue update.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static void PumpEvents()
		=> SDL_PumpEvents();

	/// <summary>
	/// Tries to add new events to the current event queue
	/// </summary>
	/// <param name="events">The events to be added</param>
	/// <param name="eventsAdded">The number of events that were added to the event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenience shortcut for <c><see cref="TryPeepEvents(Span{Event}, EventAction, out int)">TryPeepEvents</see>(<paramref name="events"/>, <see cref="EventAction"/>.<see cref="EventAction.Add">Add</see>, <see langword="out"/> <paramref name="eventsAdded"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryAddEvents(ReadOnlySpan<Event> events, out int eventsAdded)
		// I'll admit that creating a writable span from a readonly span is a bit hacky, but this way at least we don't have to duplicate code
		=> TryPeepEvents(events: MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(events), events.Length), action: EventAction.Add, out eventsAdded);

	/// <summary>
	/// Tries to count any events in the current event queue
	/// </summary>
	/// <param name="eventsCounted">The number of events in the current event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenience shortcut for <c><see cref="TryPeepEvents(EventAction, out int)">TryPeepEvents</see>(<see cref="EventAction"/>.<see cref="EventAction.Peek">Peek</see>, <see langword="out"/> <paramref name="eventsCounted"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryCountEvents(out int eventsCounted)
		=> TryPeepEvents(action: EventAction.Peek, out eventsCounted);

	/// <summary>
	/// Tries to count events in the specified type range in the current event queue
	/// </summary>
	/// <param name="type">The type of the event to count</param>
	/// <param name="eventsCounted">The number of events that match the specified <paramref name="type"/> in the current event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenience shortcut for <c><see cref="TryPeepEvents(EventAction, EventType, out int)">TryPeepEvents</see>(<see cref="EventAction"/>.<see cref="EventAction.Peek">Peek</see>, <paramref name="type"/>, <see langword="out"/> <paramref name="eventsCounted"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryCountEvents(EventType type, out int eventsCounted)
		=> TryPeepEvents(action: EventAction.Peek, type, out eventsCounted);

	/// <summary>
	/// Tries to count events in the specified type range in the current event queue
	/// </summary>
	/// <param name="minType">The inclusive lower bound of the event type range to count</param>
	/// <param name="maxType">The inclusive upper bound of the event type range to count</param>
	/// <param name="eventsCounted">The number of events that match the specified <paramref name="minType"/> and <paramref name="maxType"/> range in the current event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenience shortcut for <c><see cref="TryPeepEvents(EventAction, EventType, EventType, out int)">TryPeepEvents</see>(<see cref="EventAction"/>.<see cref="EventAction.Peek">Peek</see>, <paramref name="minType"/>, <paramref name="maxType"/>, <see langword="out"/> <paramref name="eventsCounted"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryCountEvents(EventType minType, EventType maxType, out int eventsCounted)
		=> TryPeepEvents(action: EventAction.Peek, minType, maxType, out eventsCounted);

	/// <summary>
	/// Tries to count any events in the current event queue
	/// </summary>
	/// <param name="eventsCounted">The number of events in the current event queue</param>
	/// <param name="maxCount">The maximum number of events to count</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenience shortcut for <c><see cref="TryPeepEvents(int, EventAction, out int)">TryPeepEvents</see>(<paramref name="maxCount"/>, <see cref="EventAction"/>.<see cref="EventAction.Peek">Peek</see>, <see langword="out"/> <paramref name="eventsCounted"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryCountEvents(int maxCount, out int eventsCounted)
		=> TryPeepEvents(maxCount, action: EventAction.Peek, out eventsCounted);

	/// <summary>
	/// Tries to count events in the specified type range in the current event queue
	/// </summary>
	/// <param name="type">The type of the event to count</param>
	/// <param name="maxCount">The maximum number of events to count</param>
	/// <param name="eventsCounted">The number of events that match the specified <paramref name="type"/> in the current event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenience shortcut for <c><see cref="TryPeepEvents(int, EventAction, EventType, out int)">TryPeepEvents</see>(<paramref name="maxCount"/>, <see cref="EventAction"/>.<see cref="EventAction.Peek">Peek</see>, <paramref name="type"/>, <see langword="out"/> <paramref name="eventsCounted"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryCountEvents(int maxCount, EventType type, out int eventsCounted)
		=> TryPeepEvents(maxCount, action: EventAction.Peek, type, out eventsCounted);

	/// <summary>
	/// Tries to count events in the specified type range in the current event queue
	/// </summary>
	/// <param name="maxCount">The maximum number of events to count</param>
	/// <param name="minType">The inclusive lower bound of the event type range to count</param>
	/// <param name="maxType">The inclusive upper bound of the event type range to count</param>
	/// <param name="eventsCounted">The number of events that match the specified <paramref name="minType"/> and <paramref name="maxType"/> range in the current event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenience shortcut for <c><see cref="TryPeepEvents(int, EventAction, EventType, EventType, out int)">TryPeepEvents</see>(<paramref name="maxCount"/>, <see cref="EventAction"/>.<see cref="EventAction.Peek">Peek</see>, <paramref name="minType"/>, <paramref name="maxType"/>, <see langword="out"/> <paramref name="eventsCounted"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryCountEvents(int maxCount, EventType minType, EventType maxType, out int eventsCounted)
		=> TryPeepEvents(maxCount, action: EventAction.Peek, minType, maxType, out eventsCounted);

	/// <summary>
	/// Tries to retrieve any events from the current event queue and removes them
	/// </summary>
	/// <param name="events">The destination span to store the retrieved events</param>
	/// <param name="eventsWritten">The number of events that were retrieved and removed from the current event queue and written to <paramref name="events"/></param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenience shortcut for <c><see cref="TryPeepEvents(Span{Event}, EventAction, out int)">TryPeepEvents</see>(<paramref name="events"/>, <see cref="EventAction"/>.<see cref="EventAction.Get">Get</see>, <see langword="out"/> <paramref name="eventsWritten"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryGetEvents(Span<Event> events, out int eventsWritten)
		=> TryPeepEvents(events, action: EventAction.Get, out eventsWritten);

	/// <summary>
	/// Tries to retrieve events of a specified type from the current event queue and removes them
	/// </summary>
	/// <param name="events">The destination span to store the retrieved events</param>
	/// <param name="type">The type of events to retrieve</param>
	/// <param name="eventsWritten">The number of events that were retrieved and removed from the current event queue and written to <paramref name="events"/></param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenient shortcut for <c><see cref="TryPeepEvents(Span{Event}, EventAction, EventType, out int)">TryPeepEvents</see>(<paramref name="events"/>, <see cref="EventAction"/>.<see cref="EventAction.Get">Get</see>, <paramref name="type"/>, <see langword="out"/> <paramref name="eventsWritten"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryGetEvents(Span<Event> events, EventType type, out int eventsWritten)
		=> TryPeepEvents(events, action: EventAction.Get, type, out eventsWritten);

	/// <summary>
	/// Tries to retrieve events in the specified type range from the current event queue and removes them
	/// </summary>
	/// <param name="events">The destination span to store the retrieved events</param>
	/// <param name="minType">The inclusive lower bound of the event type range to retrieve</param>
	/// <param name="maxType">The inclusive upper bound of the event type range to retrieve</param>
	/// <param name="eventsWritten">The number of events that were retrieved and removed from the current event queue and written to <paramref name="events"/></param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenient shortcut for <c><see cref="TryPeepEvents(Span{Event}, EventAction, EventType, EventType, out int)">TryPeepEvents</see>(<paramref name="events"/>, <see cref="EventAction"/>.<see cref="EventAction.Get">Get</see>, <paramref name="minType"/>, <paramref name="maxType"/>, <see langword="out"/> <paramref name="eventsWritten"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryGetEvents(Span<Event> events, EventType minType, EventType maxType, out int eventsWritten)
		=> TryPeepEvents(events, action: EventAction.Get, minType, maxType, out eventsWritten);

	/// <summary>
	/// Tries to retrieve any events from the current event queue without removing them
	/// </summary>
	/// <param name="events">The destination span to store the retrieved events</param>
	/// <param name="eventsWritten">The number of events that were retrieved from the current event queue and written to <paramref name="events"/></param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenient shortcut for <c><see cref="TryPeepEvents(Span{Event}, EventAction, out int)">TryPeepEvents</see>(<paramref name="events"/>, <see cref="EventAction"/>.<see cref="EventAction.Peek">Peek</see>, <see langword="out"/> <paramref name="eventsWritten"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryPeekEvents(Span<Event> events, out int eventsWritten)
		=> TryPeepEvents(events, action: EventAction.Peek, out eventsWritten);

	/// <summary>
	/// Tries to retrieve events of a specified type from the current event queue without removing them
	/// </summary>
	/// <param name="events">The destination span to store the retrieved events</param>
	/// <param name="type">The event type to retrieve</param>
	/// <param name="eventsWritten">The number of events that were retrieved from the current event queue and written to <paramref name="events"/></param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenient shortcut for <c><see cref="TryPeepEvents(Span{Event}, EventAction, EventType, out int)">TryPeepEvents</see>(<paramref name="events"/>, <see cref="EventAction"/>.<see cref="EventAction.Peek">Peek</see>, <paramref name="type"/>, <see langword="out"/> <paramref name="eventsWritten"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryPeekEvents(Span<Event> events, EventType type, out int eventsWritten)
		=> TryPeepEvents(events, action: EventAction.Peek, type, out eventsWritten);

	/// <summary>
	/// Tries to retrieve events in the specified type range from the current event queue without removing them
	/// </summary>
	/// <param name="events">The destination span to store the retrieved events</param>
	/// <param name="minType">The inclusive lower bound of the event type range to retrieve</param>
	/// <param name="maxType">The inclusive upper bound of the event type range to retrieve</param>
	/// <param name="eventsWritten">The number of events that were retrieved from the current event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenient shortcut for <c><see cref="TryPeepEvents(Span{Event}, EventAction, EventType, EventType, out int)">TryPeepEvents</see>(<paramref name="events"/>, <see cref="EventAction"/>.<see cref="EventAction.Peek">Peek</see>, <paramref name="minType"/>, <paramref name="maxType"/>, <see langword="out"/> <paramref name="eventsWritten"/>)</c>.
	/// </para>
	/// </remarks>
	public static void TryPeekEvents(Span<Event> events, EventType minType, EventType maxType, out int eventsWritten)
		=> TryPeepEvents(events, action: EventAction.Peek, minType, maxType, out eventsWritten);

	/// <summary>
	/// Tries to check the current event queue for any events and optionally removes them
	/// </summary>
	/// <param name="action">The action to perform on the events</param>
	/// <param name="eventsCounted">The number of events that were affected</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="EventAction.Add"/></term>
	///			<description>Does nothing and will result in a failure (this method will return <c><see langword="false"/></c>)</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Peek"/></term>
	///			<description>
	///				Up to <see cref="int.MaxValue"/> events from the front of the event queue are counted <em>without removing them from the queue</em>.
	///				The <paramref name="eventsCounted"/> output parameter contains the number of events that were counted.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Get"/></term>
	///			<description>
	///				Up to <see cref="int.MaxValue"/> events from the front of the event queue are counted <em>and removed from the queue</em>.
	///				The <paramref name="eventsCounted"/> output parameter contains the number of events that were counted and removed from the current event queue.
	///			</description>
	///		</item>
	///	</list>
	/// </para>
	/// <para>
	///	You may have to call <see cref="PumpEvents"/> before calling this method. Otherwise, events may not be ready to be filtered when you call this method.
	/// </para>
	/// </remarks>
	public static bool TryPeepEvents(EventAction action, out int eventsCounted)
		=> TryPeepEvents(maxCount: int.MaxValue, action, out eventsCounted);

	/// <summary>
	/// Tries to check the current event queue for events of a specified type and optionally removes them
	/// </summary>
	/// <param name="action">The action to perform on the events</param>
	/// <param name="type">The type of the event to check for</param>
	/// <param name="eventsCounted">The number of events that were affected</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="EventAction.Add"/></term>
	///			<description>Does nothing and will result in a failure (this method will return <c><see langword="false"/></c>)</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Peek"/></term>
	///			<description>
	///				Up to <see cref="int.MaxValue"/> events from the front of the event queue that match the specified <paramref name="type"/> are counted <em>without removing them from the queue</em>.
	///				The <paramref name="eventsCounted"/> output parameter contains the number of events that were counted.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Get"/></term>
	///			<description>
	///				Up to <see cref="int.MaxValue"/> events from the front of the event queue that match the specified <paramref name="type"/> are counted <em>and removed from the queue</em>.
	///				The <paramref name="eventsCounted"/> output parameter contains the number of events that were counted and removed from the current event queue.
	///			</description>
	///		</item>
	///	</list>
	/// </para>
	/// <para>
	///	You may have to call <see cref="PumpEvents"/> before calling this method. Otherwise, events may not be ready to be filtered when you call this method.
	/// </para>
	/// </remarks>
	public static bool TryPeepEvents(EventAction action, EventType type, out int eventsCounted)
		=> TryPeepEvents(maxCount: int.MaxValue, action, type, out eventsCounted);

	/// <summary>
	/// Tries to check the current event queue for events in specified type range and optionally removes them
	/// </summary>
	/// <param name="action">The action to perform on the events</param>
	/// <param name="minType">The inclusive lower bound of the event type range to check</param>
	/// <param name="maxType">The inclusive upper bound of the event type range to check</param>
	/// <param name="eventsCounted">The number of events that were affected</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="EventAction.Add"/></term>
	///			<description>Does nothing and will result in a failure (this method will return <c><see langword="false"/></c>)</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Peek"/></term>
	///			<description>
	///				Up to <see cref="int.MaxValue"/> events from the front of the event queue that match the specified <paramref name="minType"/> and <paramref name="maxType"/> range are counted <em>without removing them from the queue</em>.
	///				The <paramref name="eventsCounted"/> output parameter contains the number of events that were counted.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Get"/></term>
	///			<description>
	///				Up to <see cref="int.MaxValue"/> events from the front of the event queue that match the specified <paramref name="minType"/> and <paramref name="maxType"/> range are counted <em>and removed from the queue</em>.
	///				The <paramref name="eventsCounted"/> output parameter contains the number of events that were counted and removed from the current event queue.
	///			</description>
	///		</item>
	///	</list>
	/// </para>
	/// <para>
	///	You may have to call <see cref="PumpEvents"/> before calling this method. Otherwise, events may not be ready to be filtered when you call this method.
	/// </para>
	/// </remarks>
	public static bool TryPeepEvents(EventAction action, EventType minType, EventType maxType, out int eventsCounted)
		=> TryPeepEvents(maxCount: int.MaxValue, action, minType, maxType, out eventsCounted);

	/// <summary>
	/// Tries to check the current event queue for any events and optionally removes them
	/// </summary>
	/// <param name="maxCount">The maximum number of events affected</param>
	/// <param name="action">The action to perform on the events</param>
	/// <param name="eventsCounted">The number of events that were affected</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="EventAction.Add"/></term>
	///			<description>Does nothing and will result in a failure (this method will return <c><see langword="false"/></c>)</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Peek"/></term>
	///			<description>
	///				Up to <paramref name="maxCount"/> events from the front of the event queue are counted <em>without removing them from the queue</em>.
	///				The <paramref name="eventsCounted"/> output parameter contains the number of events that were counted.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Get"/></term>
	///			<description>
	///				Up to <paramref name="maxCount"/> events from the front of the event queue are counted <em>and removed from the queue</em>.
	///				The <paramref name="eventsCounted"/> output parameter contains the number of events that were counted and removed from the current event queue.
	///			</description>
	///		</item>
	///	</list>
	/// </para>
	/// <para>
	///	You may have to call <see cref="PumpEvents"/> before calling this method. Otherwise, events may not be ready to be filtered when you call this method.
	/// </para>
	/// </remarks>
	public static bool TryPeepEvents(int maxCount, EventAction action, out int eventsCounted)
#pragma warning disable CS0618 // We can use them here, that's what they are there for
		=> TryPeepEvents(maxCount, action, minType: EventType.First, maxType: EventType.Last, out eventsCounted);
#pragma warning restore CS0618

	/// <summary>
	/// Tries to check the current event queue for events of a specified type and optionally removes them
	/// </summary>
	/// <param name="maxCount">The maximum number of events affected</param>
	/// <param name="action">The action to perform on the events</param>
	/// <param name="type">The type of the event to check for</param>
	/// <param name="eventsCounted">The number of events that were affected</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="EventAction.Add"/></term>
	///			<description>Does nothing and will result in a failure (this method will return <c><see langword="false"/></c>)</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Peek"/></term>
	///			<description>
	///				Up to <paramref name="maxCount"/> events from the front of the event queue that match the specified <paramref name="type"/> are counted <em>without removing them from the queue</em>.
	///				The <paramref name="eventsCounted"/> output parameter contains the number of events that were counted.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Get"/></term>
	///			<description>
	///				Up to <paramref name="maxCount"/> events from the front of the event queue that match the specified <paramref name="type"/> are counted <em>and removed from the queue</em>.
	///				The <paramref name="eventsCounted"/> output parameter contains the number of events that were counted and removed from the current event queue.
	///			</description>
	///		</item>
	///	</list>
	/// </para>
	/// <para>
	///	You may have to call <see cref="PumpEvents"/> before calling this method. Otherwise, events may not be ready to be filtered when you call this method.
	/// </para>
	/// </remarks>
	public static bool TryPeepEvents(int maxCount, EventAction action, EventType type, out int eventsCounted)
		=> TryPeepEvents(maxCount, action, minType: type, maxType: type, out eventsCounted);

	/// <summary>
	/// Tries to check the current event queue for events in specified type range and optionally removes them
	/// </summary>
	/// <param name="maxCount">The maximum number of events affected</param>
	/// <param name="action">The action to perform on the events</param>
	/// <param name="minType">The inclusive lower bound of the event type range to check</param>
	/// <param name="maxType">The inclusive upper bound of the event type range to check</param>
	/// <param name="eventsCounted">The number of events that were affected</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="EventAction.Add"/></term>
	///			<description>Does nothing and will result in a failure (this method will return <c><see langword="false"/></c>)</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Peek"/></term>
	///			<description>
	///				Up to <paramref name="maxCount"/> events from the front of the event queue that match the specified <paramref name="minType"/> and <paramref name="maxType"/> range are counted <em>without removing them from the queue</em>.
	///				The <paramref name="eventsCounted"/> output parameter contains the number of events that were counted.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Get"/></term>
	///			<description>
	///				Up to <paramref name="maxCount"/> events from the front of the event queue that match the specified <paramref name="minType"/> and <paramref name="maxType"/> range are counted <em>and removed from the queue</em>.
	///				The <paramref name="eventsCounted"/> output parameter contains the number of events that were counted and removed from the current event queue.
	///			</description>
	///		</item>
	///	</list>
	/// </para>
	/// <para>
	///	You may have to call <see cref="PumpEvents"/> before calling this method. Otherwise, events may not be ready to be filtered when you call this method.
	/// </para>
	/// </remarks>
	public static bool TryPeepEvents(int maxCount, EventAction action, EventType minType, EventType maxType, out int eventsCounted)
	{
		unsafe
		{
			eventsCounted = SDL_PeepEvents(events: null, numevents: maxCount, action, minType, maxType);

			if (eventsCounted is < 0)
			{
				eventsCounted = 0;
				return false;
			}

			return true;
		}
	}

	/// <summary>
	/// Tries to check the current event queue for any events and retrieves them, or tries to add new events to the the current event queue
	/// </summary>
	/// <param name="events">A destination span to store the retrieved events or a source span of new events to add</param>
	/// <param name="action">The action to perform on the events</param>
	/// <param name="eventsWritten">The number of events stored in <paramref name="events"/> or the number of events added to the queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method tries to perform different actions on the event queue depending on the specified <paramref name="action"/>:
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="EventAction.Add"/></term>
	///			<description>
	///				All events from the source <paramref name="events"/> span are added to the event queue.
	///				The <paramref name="eventsWritten"/> output parameter contains the number of events that were added to the queue.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Peek"/></term>
	///			<description>
	///				Up to <paramref name="events"/>.<see cref="Span{T}.Length">Length</see> events from the front of the event queue are copied into the span <em>without removing them from the queue</em>.
	///				The <paramref name="eventsWritten"/> output parameter contains the number of events that were written to <paramref name="events"/>.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Get"/></term>
	///			<description>
	///				Up to <paramref name="events"/>.<see cref="Span{T}.Length">Length</see> events from the front of the event queue are copied into the span <em>and removed from the queue</em>.
	///				The <paramref name="eventsWritten"/> output parameter contains the number of events that were written to <paramref name="events"/> and removed from the current event queue.
	///			</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	///	You may have to call <see cref="PumpEvents"/> before calling this method. Otherwise, events may not be ready to be filtered when you call this method.
	/// </para>
	/// </remarks>
	public static bool TryPeepEvents(Span<Event> events, EventAction action, out int eventsWritten)
#pragma warning disable CS0618 // We can use them here, that's what they are there for
		=> TryPeepEvents(events, action, minType: EventType.First, maxType: EventType.Last, out eventsWritten);
#pragma warning restore CS0618

	/// <summary>
	/// Tries to check the current event queue for events of a specified type and retrieves them, or tries to add new events to the the current event queue
	/// </summary>
	/// <param name="events">A destination span to store the retrieved events or a source span of new events to add</param>
	/// <param name="action">The action to perform on the events</param>
	/// <param name="type">The type of the event to check for</param>
	/// <param name="eventsWritten">The number of events stored in <paramref name="events"/> or the number of events added to the queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method tries to perform different actions on the event queue depending on the specified <paramref name="action"/>:
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="EventAction.Add"/></term>
	///			<description>
	///				All events from the source <paramref name="events"/> span are added to the event queue.
	///				The <paramref name="type"/> parameter is ignored in this case.
	///				The <paramref name="eventsWritten"/> output parameter contains the number of events that were added to the queue.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Peek"/></term>
	///			<description>
	///				Up to <paramref name="events"/>.<see cref="Span{T}.Length">Length</see> events from the front of the event queue that match the specified <paramref name="type"/> are copied into the span <em>without removing them from the queue</em>.
	///				The <paramref name="eventsWritten"/> output parameter contains the number of events that were written to <paramref name="events"/>.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Get"/></term>
	///			<description>
	///				Up to <paramref name="events"/>.<see cref="Span{T}.Length">Length</see> events from the front of the event queue that match the specified <paramref name="type"/> are copied into the span <em>and removed from the queue</em>.
	///				The <paramref name="eventsWritten"/> output parameter contains the number of events that were written to <paramref name="events"/> and removed from the current event queue.
	///			</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	///	You may have to call <see cref="PumpEvents"/> before calling this method. Otherwise, events may not be ready to be filtered when you call this method.
	/// </para>
	/// </remarks>
	public static bool TryPeepEvents(Span<Event> events, EventAction action, EventType type, out int eventsWritten)
		=> TryPeepEvents(events, action, minType: type, maxType: type, out eventsWritten);

	/// <summary>
	/// Tries to check the current event queue for events in specified type range and retrieves them, or tries to add new events to the the current event queue
	/// </summary>
	/// <param name="events">A destination span to store the retrieved events or a source span of new events to add</param>
	/// <param name="action">The action to perform on the events</param>
	/// <param name="minType">The inclusive lower bound of the event type range to check</param>
	/// <param name="maxType">The inclusive upper bound of the event type range to check</param>
	/// <param name="eventsWritten">The number of events stored in <paramref name="events"/> or the number of events added to the queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method tries to perform different actions on the event queue depending on the specified <paramref name="action"/>:
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="EventAction.Add"/></term>
	///			<description>
	///				All events from the source <paramref name="events"/> span are added to the event queue.
	///				The <paramref name="minType"/> and <paramref name="maxType"/> parameters are ignored in this case.
	///				The <paramref name="eventsWritten"/> output parameter contains the number of events that were added to the queue.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Peek"/></term>
	///			<description>
	///				Up to <paramref name="events"/>.<see cref="Span{T}.Length">Length</see> events from the front of the event queue that match the specified <paramref name="minType"/> and <paramref name="maxType"/> range are copied into the span <em>without removing them from the queue</em>.
	///				The <paramref name="eventsWritten"/> output parameter contains the number of events that were written to <paramref name="events"/>.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see cref="EventAction.Get"/></term>
	///			<description>
	///				Up to <paramref name="events"/>.<see cref="Span{T}.Length">Length</see> events from the front of the event queue that match the specified <paramref name="minType"/> and <paramref name="maxType"/> range are copied into the span <em>and removed from the queue</em>.
	///				The <paramref name="eventsWritten"/> output parameter contains the number of events that were written to <paramref name="events"/> and removed from the current event queue.
	///			</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	///	You may have to call <see cref="PumpEvents"/> before calling this method. Otherwise, events may not be ready to be filtered when you call this method.
	/// </para>
	/// </remarks>
	public static bool TryPeepEvents(Span<Event> events, EventAction action, EventType minType, EventType maxType, out int eventsWritten)
	{
		unsafe
		{
			fixed (Event* eventsPtr = events)
			{
				eventsWritten = SDL_PeepEvents(events: eventsPtr, numevents: events.Length, action, minType, maxType);

				if (eventsWritten is < 0)
				{
					eventsWritten = 0;
					return false;
				}

				return true;
			}
		}
	}

	/// <summary>
	/// Tries to poll for pending events from the current event queue
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if there is a pending event in the event queue; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// If there is a pending event in the current event queue, it will <em>not</em> be removed from the event queue, and this method will simply return <c><see langword="true"/></c>.
	/// In the case this method returned <c><see langword="false"/></c>, there was no pending event in the current event queue.
	/// The event queue remains unchanged in any case. Therefore you can use this method to check if there are pending events in the current event queue.
	/// </para>
	/// <para>
	/// This method may implicitly call <see cref="PumpEvents"/>.
	/// </para>
	/// <para>
	/// Note that Windows (and possibly other platforms) has a quirk about how it handles events while dragging/resizing a window, which can cause this function to block for significant amounts of time.
	/// Technical explanations and solutions are discussed on the wiki: <see href="https://wiki.libsdl.org/SDL3/AppFreezeDuringDrag"/>.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryPollEvent()
	{
		unsafe
		{
			return SDL_PollEvent(@event: null);
		}
	}

	/// <summary>
	/// Tries to poll for pending events from the current event queue
	/// </summary>
	/// <param name="event">The next pending event from the event queue, if this method returns <c><see langword="true"/></c></param>
	/// <returns><c><see langword="true"/></c>, if there was a pending event in the event queue and it was copied into <paramref name="event"/>; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// If there is a pending event in the current event queue, it will be copied into the <paramref name="event"/> argument and removed from the event queue, and then this method will return <c><see langword="true"/></c>.
	/// In the case this method returned <c><see langword="false"/></c>, there was no pending event in the current event queue and the event queue remained unchanged.
	/// </para>
	/// <para>
	/// This method may implicitly call <see cref="PumpEvents"/>.
	/// </para>
	/// <para>
	/// <see cref="TryPollEvent(out Event)"/> is the favored way of receiving system events since it can be done from the main loop and does not suspend the main loop while waiting on an event to be posted.
	/// </para>
	/// <para>
	/// The common practice is to fully process the current event queue once every frame, usually as a first step before updating the game's state.
	/// </para>
	/// <para>
	/// Note that Windows (and possibly other platforms) has a quirk about how it handles events while dragging/resizing a window, which can cause this function to block for significant amounts of time.
	/// Technical explanations and solutions are discussed on the wiki: <see href="https://wiki.libsdl.org/SDL3/AppFreezeDuringDrag"/>.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryPollEvent(out Event @event)
	{
		unsafe
		{
			fixed (Event* eventPtr = &@event)
			{
				return SDL_PollEvent(@event: eventPtr);
			}
		}
	}

	/// <summary>
	/// Tries to add a new event to the current event queue
	/// </summary>
	/// <param name="event">The event to add to the queue</param>
	/// <returns><c><see langword="true"/></c>, if the event was successfully added to the queue; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Events that are supposed to be added via this method are passed to the <see cref="EventFilter"/> and are susceptible to being <see cref="EventTypeExtensions.set_Enabled(EventType, bool)">disabled</see>.
	/// If the event is filtered out, this method will return <c><see langword="false"/></c> (use <see cref="Error.TryGet(out string?)"/> to check if the event was filtered out or if there was another kind of failure).
	/// If you want to bypass the filter you can use <see cref="TryAddEvents(ReadOnlySpan{Event}, out int)"/> instead.
	/// </para>
	/// <para>
	/// Another common reason for this method failing is the event queue being full.
	/// </para>
	/// <para>
	/// The event queue can actually be used as a two way communication channel.
	/// Not only can events be read from the queue, but the user can also push their own events onto it.
	/// The <paramref name="event"/> will be copied into the queue, and afterwards user can forget about it until it's (<see cref="TryPollEvent(out Event)">polled</see>) by themselves or somewhere else.
	/// </para>
	/// <para>
	/// Note: Pushing device input events onto the queue doesn't modify the state of the device within SDL.
	/// </para>
	/// <para>
	/// For pushing user defined custom events, please use <see cref="EventTypeExtensions.TryRegister(out EventType)"/> or <see cref="EventTypeExtensions.TryRegister(Span{EventType})"/> to get event types that does not conflict with other code that also wants its own custom event types.
	/// </para>
	/// </remarks>
	public static bool TryPushEvent(in Event @event)
	{
		unsafe
		{
			fixed (Event* eventPtr = &@event)
			{
				return SDL_PushEvent(@event: eventPtr);
			}
		}
	}

	/// <summary>
	/// Tries to remove any events from the current event queue
	/// </summary>
	/// <param name="eventsRemoved">The number of events that were removed from the current event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenient shortcut for <c><see cref="TryPeepEvents(EventAction, out int)">TryPeepEvents</see>(<see cref="EventAction"/>.<see cref="EventAction.Get">Get</see>, <see langword="out"/> <paramref name="eventsRemoved"/>)</c>.
	/// </para>
	/// </remarks>
	public static bool TryRemoveEvents(out int eventsRemoved)
		=> TryPeepEvents(action: EventAction.Get, out eventsRemoved);

	/// <summary>
	/// Tries to remove events of a specified type from the current event queue
	/// </summary>
	/// <param name="type">The type of events to remove</param>
	/// <param name="eventsRemoved">The number of events that were removed from the current event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenient shortcut for <c><see cref="TryPeepEvents(EventAction, EventType, out int)">TryPeepEvents</see>(<see cref="EventAction"/>.<see cref="EventAction.Get">Get</see>, <paramref name="type"/>, <see langword="out"/> <paramref name="eventsRemoved"/>)</c>.
	/// </para>
	/// </remarks>
	public static bool TryRemoveEvents(EventType type, out int eventsRemoved)
		=> TryPeepEvents(action: EventAction.Get, type, out eventsRemoved);

	/// <summary>
	/// Tries to remove events in the specified type range from the current event queue
	/// </summary>
	/// <param name="minType">The inclusive lower bound of the event type range to remove</param>
	/// <param name="maxType">The inclusive upper bound of the event type range to remove</param>
	/// <param name="eventsRemoved">The number of events that were removed from the current event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenient shortcut for <c><see cref="TryPeepEvents(EventAction, EventType, EventType, out int)">TryPeepEvents</see>(<see cref="EventAction"/>.<see cref="EventAction.Get">Get</see>, <paramref name="minType"/>, <paramref name="maxType"/>, <see langword="out"/> <paramref name="eventsRemoved"/>)</c>.
	/// </para>
	/// </remarks>
	public static bool TryRemoveEvents(EventType minType, EventType maxType, out int eventsRemoved)
		=> TryPeepEvents(action: EventAction.Get, minType, maxType, out eventsRemoved);

	/// <summary>
	/// Tries to remove any events from the current event queue
	/// </summary>
	/// <param name="maxCount">The maximum number of events to remove</param>
	/// <param name="eventsRemoved">The number of events that were removed from the current event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenient shortcut for <c><see cref="TryPeepEvents(int, EventAction, out int)">TryPeepEvents</see>(<paramref name="maxCount"/>, <see cref="EventAction"/>.<see cref="EventAction.Get">Get</see>, <see langword="out"/> <paramref name="eventsRemoved"/>)</c>.
	/// </para>
	/// </remarks>
	public static bool TryRemoveEvents(int maxCount, out int eventsRemoved)
		=> TryPeepEvents(maxCount, action: EventAction.Get, out eventsRemoved);

	/// <summary>
	/// Tries to remove events of a specified type from the current event queue
	/// </summary>
	/// <param name="maxCount">The maximum number of events to remove</param>
	/// <param name="type">The type of events to remove</param>
	/// <param name="eventsRemoved">The number of events that were removed from the current event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenient shortcut for <c><see cref="TryPeepEvents(int, EventAction, EventType, out int)">TryPeepEvents</see>(<paramref name="maxCount"/>, <see cref="EventAction"/>.<see cref="EventAction.Get">Get</see>, <paramref name="type"/>, <see langword="out"/> <paramref name="eventsRemoved"/>)</c>.
	/// </para>
	/// </remarks>
	public static bool TryRemoveEvents(int maxCount, EventType type, out int eventsRemoved)
		=> TryPeepEvents(maxCount, action: EventAction.Get, type, out eventsRemoved);

	/// <summary>
	/// Tries to remove events in the specified type range from the current event queue
	/// </summary>
	/// <param name="maxCount">The maximum number of events to remove</param>
	/// <param name="minType">The inclusive lower bound of the event type range to remove</param>
	/// <param name="maxType">The inclusive upper bound of the event type range to remove</param>
	/// <param name="eventsRemoved">The number of events that were removed from the current event queue</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is a convenient shortcut for <c><see cref="TryPeepEvents(int, EventAction, EventType, EventType, out int)">TryPeepEvents</see>(<paramref name="maxCount"/>, <see cref="EventAction"/>.<see cref="EventAction.Get">Get</see>, <paramref name="minType"/>, <paramref name="maxType"/>, <see langword="out"/> <paramref name="eventsRemoved"/>)</c>.
	/// </para>
	/// </remarks>
	public static bool TryRemoveEvents(int maxCount, EventType minType, EventType maxType, out int eventsRemoved)
		=> TryPeepEvents(maxCount, action: EventAction.Get, minType, maxType, out eventsRemoved);

	/// <summary>
	/// Tries to indefinitely wait for pending events from the current event queue
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if there was a pending event received in the event queue; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// If there is a pending event recieved in the current event queue, it will <em>not</em> be removed from the event queue, and this method will simply return <c><see langword="true"/></c>.
	/// In the case this method returned <c><see langword="false"/></c>, there was an error while waiting and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// The event queue remains unchanged in any case. Therefore you can use this method to block and wait until there are pending events recieved in the current event queue.
	/// </para>
	/// <para>
	/// This method may implicitly call <see cref="PumpEvents"/>.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryWaitForEvent()
	{
		unsafe
		{
			return SDL_WaitEvent(@event: null);
		}
	}

	/// <summary>
	/// Tries to indefinitely wait for pending events from the current event queue
	/// </summary>
	/// <param name="event">The next pending event from the event queue</param>
	/// <returns><c><see langword="true"/></c>, if there was a pending event received in the event queue and it was copied into <paramref name="event"/>; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// If there is a pending event recieved in the current event queue, it will be copied into the <paramref name="event"/> argument and removed from the event queue, and then this method will return <c><see langword="true"/></c>.
	/// In the case this method returned <c><see langword="false"/></c>, there was an error while waiting and you should check <see cref="Error.TryGet(out string?)"/> for more information.
	/// </para>
	/// <para>
	/// This method may implicitly call <see cref="PumpEvents"/>.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryWaitForEvent(out Event @event)
	{
		unsafe
		{
			fixed (Event* eventPtr = &@event)
			{
				return SDL_WaitEvent(@event: eventPtr);
			}
		}
	}

	/// <summary>
	/// Tries to wait for pending events from the current event queue until a specified timeout
	/// </summary>
	/// <param name="timeoutMs">The timeout in milliseconds, or <c>-1</c> to wait indefinitely</param>
	/// <returns><c><see langword="true"/></c>, if there was a pending event recieved in the event queue before the timeout; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// If there is a pending event recieved in the current event queue before the timeout, it will <em>not</em> be removed from the event queue, and this method will simply return <c><see langword="true"/></c>.
	/// In the case this method returned <c><see langword="false"/></c>, there was no pending event recieved in the current event queue until the specified timeout.
	/// The event queue remains unchanged in any case. Therefore you can use this method to block and wait until there are pending events recieved in the current event queue.
	/// </para>
	/// <para>
	/// This method may implicitly call <see cref="PumpEvents"/>.
	/// </para>
	/// <para>
	/// The specified timeout is not guaranteed, the actual wait time could be longer due to system scheduling.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryWaitForEvent(int timeoutMs)
	{
		unsafe
		{
			return SDL_WaitEventTimeout(@event: null, timeoutMs);
		}
	}

	/// <summary>
	/// Tries to wait for pending events from the current event queue until a specified timeout
	/// </summary>
	/// <param name="timeout">The positive timeout, or a negative value to wait indefinitely</param>
	/// <returns><c><see langword="true"/></c>, if there was a pending event recieved in the event queue before the timeout; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// If there is a pending event recieved in the current event queue before the timeout, it will <em>not</em> be removed from the event queue, and this method will simply return <c><see langword="true"/></c>.
	/// In the case this method returned <c><see langword="false"/></c>, there was no pending event recieved in the current event queue until the specified timeout.
	/// The event queue remains unchanged in any case. Therefore you can use this method to block and wait until there are pending events recieved in the current event queue.
	/// </para>
	/// <para>
	/// This method may implicitly call <see cref="PumpEvents"/>.
	/// </para>
	/// <para>
	/// The specified timeout is not guaranteed, the actual wait time could be longer due to system scheduling.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryWaitForEvent(TimeSpan timeout)
	{
		return TryWaitForEvent(
			timeoutMs: timeout.TotalMilliseconds switch
			{
				< 0 => -1,
				var value => (int)double.Min(double.Ceiling(value), int.MaxValue)
			}
		);
	}

	/// <summary>
	/// Tries to wait until a specified timeout for pending events from the current event queue
	/// </summary>
	/// <param name="event">The next pending event from the event queue, if this method returns <c><see langword="true"/></c></param>
	/// <param name="timeoutMs">The timeout in milliseconds, or <c>-1</c> to wait indefinitely</param>
	/// <returns><c><see langword="true"/></c>, if there was a pending event recieved in the event queue before the timeout and it was copied into <paramref name="event"/>; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// If there is a pending event recieved in the current event queue before the timeout, it will be copied into the <paramref name="event"/> argument and removed from the event queue, and then this method will return <c><see langword="true"/></c>.
	/// In the case this method returned <c><see langword="false"/></c>, there was no pending event recieved in the current event queue until the specified timeout.
	/// </para>
	/// <para>
	/// This method may implicitly call <see cref="PumpEvents"/>.
	/// </para>
	/// <para>
	/// The specified timeout is not guaranteed, the actual wait time could be longer due to system scheduling.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryWaitForEvent(out Event @event, int timeoutMs)
	{
		unsafe
		{
			fixed (Event* eventPtr = &@event)
			{
				return SDL_WaitEventTimeout(@event: eventPtr, timeoutMs);
			}
		}
	}

	/// <summary>
	/// Tries to wait until a specified timeout for pending events from the current event queue
	/// </summary>
	/// <param name="event">The next pending event from the event queue, if this method returns <c><see langword="true"/></c></param>
	/// <param name="timeout">The positive timeout, or a negative value to wait indefinitely</param>
	/// <returns><c><see langword="true"/></c>, if there was a pending event recieved in the event queue before the timeout and it was copied into <paramref name="event"/>; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// If there is a pending event recieved in the current event queue before the timeout, it will be copied into the <paramref name="event"/> argument and removed from the event queue, and then this method will return <c><see langword="true"/></c>.
	/// In the case this method returned <c><see langword="false"/></c>, there was no pending event recieved in the current event queue until the specified timeout.
	/// </para>
	/// <para>
	/// This method may implicitly call <see cref="PumpEvents"/>.
	/// </para>
	/// <para>
	/// The specified timeout is not guaranteed, the actual wait time could be longer due to system scheduling.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryWaitForEvent(out Event @event, TimeSpan timeout)
	{
		return TryWaitForEvent(
			out @event,
			timeoutMs: timeout.TotalMilliseconds switch
			{
				< 0 => -1,
				var value => (int)double.Min(double.Ceiling(value), int.MaxValue)
			}
		);
	}
}
