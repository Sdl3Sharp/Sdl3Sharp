using Sdl3Sharp.Internal;
using Sdl3Sharp.Video.Windowing;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Provides extension methods for the <see cref="Event"/> structure
/// </summary>
public static partial class EventExtensions
{
	extension(in Event @event)
	{
#if SDL3_4_0_OR_GREATER

		/// <summary>
		/// Gets an English description of the event
		/// </summary>
		/// <value>
		/// An English description of the event
		/// </value>
		/// <remarks>
		/// <para>
		/// The value of this property might look something like this:
		/// <code>
		/// SDL_EVENT_MOUSE_MOTION (timestamp=1140256324 windowid=2 which=0 state=0 x=492.99 y=139.09 xrel=52 yrel=6)
		/// </code>
		/// </para>
		/// <para>
		/// The exact format of the string is not guaranteed; it is intended for logging purposes, to be read by a human, and not parsed by a computer.
		/// </para>
		/// <para>
		/// You might prefer to use the <c>ToString</c> or <c>TryFormat</c> methods of the event structure instead of this property.
		/// </para>
		/// </remarks>
		public string Description
		{
			get
			{
				unsafe
				{
					fixed (Event* eventPtr = &@event)
					{
						var length = SDL_GetEventDescription(eventPtr, buf: null, buflen: 0);
						length = unchecked(length + 1); // For null terminator

						byte* description;
						bool isHeapAllocated;

						if (length is <= 256)
						{
							var stackPtr = stackalloc byte[length];
							description = stackPtr;
							isHeapAllocated = false;
						}
						else
						{
							description = unchecked((byte*)Utilities.NativeMemory.Malloc(unchecked((nuint)length * sizeof(byte))));
							isHeapAllocated = true;
						}

						try
						{
							length = SDL_GetEventDescription(eventPtr, description, length);

							using var descriptionUtf16 = NativeStrings.FromUtf8ToUtf16(description, unchecked((nuint)length));

							return descriptionUtf16.ToManaged()!;
						}
						finally
						{
							if (isHeapAllocated)
							{
								Utilities.NativeMemory.Free(description);
							}
						}
					}
				}
			}
		}

#endif

		/// <summary>
		/// Gets the window associated with the event, if any
		/// </summary>
		/// <value>
		/// The window associated with the event, or <c><see langword="null"/></c> if there is none
		/// </value>
		/// <remarks>
		/// <para>
		/// This property can be used to get the window associated with the event, if any, without the need to check and convert the event to the appropriate type first.
		/// For example, if you have a <see cref="WindowEvent"/> or a <see cref="MouseMotionEvent"/>, you can use this property to get the value of their <see cref="WindowEvent.Window"/> or <see cref="MouseMotionEvent.Window"/> properties, respectively,
		/// without having to check the event type and convert it first.
		/// </para>
		/// </remarks>
		public Window? Window
		{
			get
			{
				unsafe
				{
					fixed (Event* eventPtr = &@event)
					{
						Window.TryGetOrCreate(SDL_GetWindowFromEvent(eventPtr), out var window);
						return window;
					}
				}
			}
		}
	}

	extension<TEvent>(TEvent @event)           // `in`/`ref readonly` receiver arguments in extension members are still not a thing in C# 14.0, so we have to "copy".
		where TEvent : notnull, IEvent<TEvent>
	{
#if SDL3_4_0_OR_GREATER

		/// <inheritdoc cref="get_Description(in Event)"/>
		public string Description
		{
			// These `get` implementations mitigate the need for a copy a bit, as they make a copy inevitable anyways (we need to make sure that we present a pointer to a *whole* `Event` structure to `SDL_GetEventDescription`).
			// So with aggressive inlining and optimization, hopefully that's just a single copy.
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#if NET11_0_OR_GREATER
			get => get_Description(@event);
#else
			get => Event.From(@event).Description;
#endif
		}

#endif
	}
}
