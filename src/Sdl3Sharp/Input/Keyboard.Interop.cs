using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using Sdl3Sharp.Video.Windowing;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Input;

partial struct Keyboard
{
	/// <summary>
	/// Queries the window which currently has keyboard focus
	/// </summary>
	/// <returns>Returns the window with keyboard focus</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetKeyboardFocus">SDL_GetKeyboardFocus</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial Window.SDL_Window* SDL_GetKeyboardFocus();

	/// <summary>
	/// Gets the name of a keyboard
	/// </summary>
	/// <param name="instance_id">The keyboard instance ID.</param>
	/// <returns>Returns the name of the selected keyboard or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function returns "" if the keyboard doesn't have a name.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetKeyboardNameForID">SDL_GetKeyboardNameForID</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial byte* SDL_GetKeyboardNameForID(uint instance_id);

	/// <summary>
	/// Gets a list of currently connected keyboards
	/// </summary>
	/// <param name="count">A pointer filled in with the number of keyboards returned, may be NULL</param>
	/// <returns>
	/// Returns a 0 terminated array of keyboards instance IDs or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information.
	/// This should be freed with <see href="https://wiki.libsdl.org/SDL3/SDL_free">SDL_free</see>() when it is no longer needed.
	/// </returns>
	/// <remarks>
	/// <para>
	/// Note that this will include any device or virtual driver that includes keyboard functionality, including some mice, KVM switches, motherboard power buttons, etc.
	/// You should wait for input from a device before you consider it actively in use.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetKeyboards">SDL_GetKeyboards</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial uint* SDL_GetKeyboards(int* count);

	/// <summary>
	/// Gets a snapshot of the current state of the keyboard
	/// </summary>
	/// <param name="numkeys">If non-NULL, receives the length of the returned array</param>
	/// <returns>Returns a pointer to an array of key states</returns>
	/// <remarks>
	/// <para>
	/// The pointer returned is a pointer to an internal SDL array.
	/// It will be valid for the whole lifetime of the application and should not be freed by the caller.
	/// </para>
	/// <para>
	/// A array element with a value of true means that the key is pressed and a value of false means that it is not.
	/// Indexes into this array are obtained by using <see href="https://wiki.libsdl.org/SDL3/SDL_Scancode">SDL_Scancode</see> values.
	/// </para>
	/// <para>
	/// Use <see href="https://wiki.libsdl.org/SDL3/SDL_PumpEvents">SDL_PumpEvents</see>() to update the state array.
	/// </para>
	/// <para>
	/// This function gives you the current state after all events have been processed,
	/// so if a key or button has been pressed and released before you process events,
	/// then the pressed state will never show up in the <see href="https://wiki.libsdl.org/SDL3/SDL_GetKeyboardState">SDL_GetKeyboardState</see>() calls.
	/// </para>
	/// <para>
	/// Note: This function doesn't take into account whether shift has been pressed or not.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetKeyboards">SDL_GetKeyboards</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool* SDL_GetKeyboardState(int* numkeys);

	/// <summary>
	/// Gets the current key modifier state for the keyboard
	/// </summary>
	/// <returns>Returns an OR'd combination of the modifier keys for the keyboard</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetModState">SDL_GetModState</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial Keymod SDL_GetModState();

	/// <summary>
	/// Returns whether a keyboard is currently connected
	/// </summary>
	/// <returns>Returns true if a keyboard is connected, false otherwise</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HasKeyboard">SDL_HasKeyboard</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial CBool SDL_HasKeyboard();

	/// <summary>
	/// Checks whether the platform has screen keyboard support
	/// </summary>
	/// <returns>Returns true if the platform has some screen keyboard support or false if not</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HasScreenKeyboardSupport">SDL_HasScreenKeyboardSupport</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial CBool SDL_HasScreenKeyboardSupport();

	/// <summary>
	/// Clears the state of the keyboard
	/// </summary>
	/// <remarks>
	/// <para>
	/// This function will generate key up events for all pressed keys.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_ResetKeyboard">SDL_ResetKeyboard</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial void SDL_ResetKeyboard();

	/// <summary>
	/// Sets the current key modifier state for the keyboard
	/// </summary>
	/// <param name="modstate">The desired <see href="https://wiki.libsdl.org/SDL3/SDL_Keymod">SDL_Keymod</see> for the keyboard</param>
	/// <remarks>
	/// <para>
	/// The inverse of <see href="https://wiki.libsdl.org/SDL3/SDL_GetModState">SDL_GetModState</see>(), <see href="https://wiki.libsdl.org/SDL3/SDL_SetModState">SDL_SetModState</see>() allows you to impose modifier key states on your application.
	/// Simply pass your desired modifier states into modstate.
	/// This value may be a bitwise, OR'd combination of <see href="https://wiki.libsdl.org/SDL3/SDL_Keymod">SDL_Keymod</see> values.
	/// </para>
	/// <para>
	/// This does not change the keyboard state, only the key modifier flags that SDL reports.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetModState">SDL_SetModState</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial void SDL_SetModState(Keymod modstate);
}
