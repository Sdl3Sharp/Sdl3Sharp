using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Input;

partial struct TouchDevice
{
	/// <summary>
	/// Gets the touch device name as reported from the driver
	/// </summary>
	/// <param name="touchID">The touch device instance ID</param>
	/// <returns>Returns touch device name, or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTouchDeviceName">SDL_GetTouchDeviceName</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial byte* SDL_GetTouchDeviceName(ulong touchID);

	/// <summary>
	/// Gets a list of registered touch devices
	/// </summary>
	/// <param name="count">A pointer filled in with the number of devices returned, may be NULL.</param>
	/// <returns>
	/// Returns a 0 terminated array of touch device IDs or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information.
	/// This should be freed with <see href="https://wiki.libsdl.org/SDL3/SDL_free">SDL_free</see>() when it is no longer needed.
	/// </returns>
	/// <remarks>
	/// <para>
	/// On some platforms SDL first sees the touch device if it was actually used.
	/// Therefore the returned list might be empty, although devices are available.
	/// After using all devices at least once the number will be correct.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTouchDevices">SDL_GetTouchDevices</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial ulong* SDL_GetTouchDevices(int* count);

	/// <summary>
	/// Gets the type of the given touch device
	/// </summary>
	/// <param name="touchID">The ID of a touch device</param>
	/// <returns>Returns touch device type</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTouchDeviceType">SDL_GetTouchDeviceType</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial TouchDeviceType SDL_GetTouchDeviceType(ulong touchID);

	/// <summary>
	/// Gets a list of active fingers for a given touch device
	/// </summary>
	/// <param name="touchID">The ID of a touch device</param>
	/// <param name="count">A pointer filled in with the number of fingers returned, can be NULL</param>
	/// <returns>
	/// Returns a NULL terminated array of <see href="https://wiki.libsdl.org/SDL3/SDL_Finger">SDL_Finger</see> pointers or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information.
	/// This is a single allocation that should be freed with <see href="https://wiki.libsdl.org/SDL3/SDL_free">SDL_free</see>() when it is no longer needed.
	/// </returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTouchFingers">SDL_GetTouchFingers</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial Finger** SDL_GetTouchFingers(ulong touchID, int* count);
}
