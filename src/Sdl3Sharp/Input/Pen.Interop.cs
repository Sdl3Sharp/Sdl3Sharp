using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Input;

partial struct Pen
{
#if SDL3_4_0_OR_GREATER

	/// <summary>
	/// Gets the device type of the given pen
	/// </summary>
	/// <param name="instance_id">The pen instance ID.</param>
	/// <returns>Returns the device type of the given pen, or <see href="https://wiki.libsdl.org/SDL3/SDL_PEN_DEVICE_TYPE_INVALID">SDL_PEN_DEVICE_TYPE_INVALID</see> on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// Many platforms do not supply this information, so an app must always be prepared to get an <see href="https://wiki.libsdl.org/SDL3/SDL_PEN_DEVICE_TYPE_UNKNOWN">SDL_PEN_DEVICE_TYPE_UNKNOWN</see> result.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetPenDeviceType">SDL_GetPenDeviceType</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]	
	internal static partial PenDeviceType SDL_GetPenDeviceType(uint instance_id);

#endif
}
