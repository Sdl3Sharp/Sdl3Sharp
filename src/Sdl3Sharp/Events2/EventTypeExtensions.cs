using System;

namespace Sdl3Sharp.Events2;

/// <summary>
/// Provides extension methods and properties for <see cref="EventType"/>
/// </summary>
public static partial class EventTypeExtensions
{
	extension(EventType)
	{
		/// <summary>
		/// Tries to register a new used defined event
		/// </summary>
		/// <param name="eventType">The newly registered <see cref="EventType"/> when this method returns <c><see langword="true"/></c></param>
		/// <returns><c><see langword="true"/></c> if the user defined event was successfully registered; otherwise, <c><see langword="false"/></c></returns>
		public static bool TryRegister(out EventType eventType)
		{
			eventType = SDL_RegisterEvents(1);

			return eventType is not 0;
		}

		/// <summary>
		/// Tries to register new user defined events
		/// </summary>
		/// <param name="eventTypes">A <see cref="Span{T}"/> where the newly registered <see cref="EventType"/>s should be written to. It's <see cref="Span{T}.Length"/> determines how many user defined <see cref="EventType"/>s to register.</param>
		/// <returns><c><see langword="true"/></c> if the requested amount (by <see cref="Span{T}.Length"/>) of user defined events was successfully registered; otherwise, <c><see langword="false"/></c> (most likely there weren't enough free user defined <see cref="EventType"/>s left to register <see cref="Span{T}.Length"/> of them)</returns>
		/// <remarks>
		/// <para>
		/// Note: The value of <paramref name="eventTypes"/>'s <see cref="Span{T}.Length"/> property determines how many user defined <see cref="EventType"/>s to register.
		/// It's irrelevant what <paramref name="eventTypes"/> contains when calling this method.
		/// The contents of <paramref name="eventTypes"/> will be overriden with the newly registered user defined <see cref="EventType"/>s.
		/// </para>
		/// </remarks>
		public static bool TryRegister(Span<EventType> eventTypes)
		{
			var result = SDL_RegisterEvents(eventTypes.Length);

			if (result is 0)
			{
				return false;
			}

			foreach (ref var eventType in eventTypes)
			{
				eventType = result++;
			}

			return true;
		}
	}

	extension(EventType eventType)
	{
		/// <summary>
		/// Gets or sets a value determining whether the current <see cref="EventType"/> is enabled within SDL's event system or not
		/// </summary>
		/// <value>
		/// A value determining whether the current <see cref="EventType"/> is enabled within SDL's event system or not
		/// </value>
		/// <remarks>
		/// <para>
		/// Disabled <see cref="EventType"/>s won't get processed by SDL.
		/// </para>
		/// </remarks>
		public bool Enabled
		{
			get => SDL_EventEnabled(eventType);
			set => SDL_SetEventEnabled(eventType, value);
		}

		/// <summary>
		/// Tries to get the user value used to identify this user event type
		/// </summary>
		/// <param name="userValue">The user value used to identify this user event type</param>
		/// <returns><c><see langword="true"/></c> if this <see cref="EventType"/> represents a user event type and <paramref name="userValue"/> is the user value which identifies it; otherwise, <c><see langword="false"/></c></returns>
		public bool TryGetUserValue(out int userValue)
		{
#pragma warning disable CS0618 // Here's one of the few places we're allowed to use this (internally)
			if (eventType is >= EventType.User and <= EventType.Last)
			{
				userValue = unchecked((int)(eventType - EventType.User));

				return true;
			}

			userValue = default;

			return false;
#pragma warning restore CS0618
		}
	}
}
