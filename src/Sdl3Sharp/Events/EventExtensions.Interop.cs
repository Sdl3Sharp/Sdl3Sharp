using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using Sdl3Sharp.Video.Windowing;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Events;

partial class EventExtensions
{
#if SDL3_4_0_OR_GREATER

	/// <summary>
	/// Generates an English description of an event
	/// </summary>
	/// <param name="event">An event to describe. May be NULL.</param>
	/// <param name="buf">The buffer to fill with the description string. May be NULL.</param>
	/// <param name="buflen">The maximum bytes that can be written to <c><paramref name="buf"/></c></param>
	/// <returns>Returns number of bytes needed for the full string, not counting the null-terminator byte</returns>
	/// <remarks>
	/// <para>
	/// This will fill <c><paramref name="buf"/></c> with a null-terminated string that might look something like this:
	/// <code>
	/// SDL_EVENT_MOUSE_MOTION (timestamp=1140256324 windowid=2 which=0 state=0 x=492.99 y=139.09 xrel=52 yrel=6)
	/// </code>
	/// The exact format of the string is not guaranteed; it is intended for logging purposes, to be read by a human, and not parsed by a computer.
	/// </para>
	/// <para>
	/// The returned value follows the same rules as <see href="https://wiki.libsdl.org/SDL3/SDL_snprintf">SDL_snprintf</see>(): <c><paramref name="buf"/></c> will always be NULL-terminated (unless <c><paramref name="buflen"/></c> is zero), and will be truncated if <c><paramref name="buflen"/></c> is too small.
	/// The return code is the number of bytes needed for the complete string, not counting the NULL-terminator, whether the string was truncated or not.
	/// Unlike <see href="https://wiki.libsdl.org/SDL3/SDL_snprintf">SDL_snprintf</see>(), though, this function never returns -1.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetEventDescription">SDL_GetEventDescription</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial int SDL_GetEventDescription(Event* @event, byte* buf, int buflen);

#endif

	/// <summary>
	/// Gets window associated with an event
	/// </summary>
	/// <param name="event">An event containing a <c>windowID</c></param>
	/// <returns>Returns the associated window on success or NULL if there is none</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowFromEvent">SDL_GetWindowFromEvent</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial Window.SDL_Window* SDL_GetWindowFromEvent(Event* @event);
}
