using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Events2;

partial class EventTypeExtensions
{
	/// <summary>
	/// Query the state of processing events by type
	/// </summary>
	/// <param name="type">The type of event; see <see href="https://wiki.libsdl.org/SDL3/SDL_EventType">SDL_EventType</see> for detail</param>
	/// <returns>Returns true if the event is being processed, false otherwise</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_EventEnabled">SDL_EventEnabled</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial CBool SDL_EventEnabled(EventType type);

	/// <summary>
	/// Allocate a set of user-defined events, and return the beginning event number for that set of events
	/// </summary>
	/// <param name="numevents">The number of events to be allocated</param>
	/// <returns>Returns the beginning event number, or 0 if numevents is invalid or if there are not enough user-defined events left</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_RegisterEvents">SDL_RegisterEvents</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial EventType SDL_RegisterEvents(int numevents);

	/// <summary>
	/// Set the state of processing events by type
	/// </summary>
	/// <param name="type">The type of event; see <see href="https://wiki.libsdl.org/SDL3/SDL_EventType">SDL_EventType</see> for details</param>
	/// <param name="enabled">Whether to process the event or not</param>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetEventEnabled">SDL_SetEventEnabled</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial void SDL_SetEventEnabled(EventType type, CBool enabled);
}
