using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using unsafe SDL_MouseMotionTransformCallback = delegate* unmanaged[Cdecl]<void*, ulong, Sdl3Sharp.Video.Windowing.Window.SDL_Window*, uint, float*, float*, void>;

namespace Sdl3Sharp.Input;

partial struct Mouse
{
#if SDL3_4_0_OR_GREATER

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private unsafe static void MouseMotionTransformCallback(void* userdata, ulong timestamp, Window.SDL_Window* window, uint mouseId, float* x, float* y)
	{
		if (userdata is not null && GCHandle.FromIntPtr(unchecked((IntPtr)userdata)) is { IsAllocated: true, Target: MouseMotionTransformCallback callback })
		{
			callback(new(timestamp, window, mouseId, x, y));
		}
	}

#endif

	/// <summary>
	/// Captures the mouse and to track input outside an SDL window
	/// </summary>
	/// <param name="enabled">True to enable capturing, false to disable</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// Capturing enables your app to obtain mouse events globally, instead of just within your window. Not all video targets support this function.
	/// When capturing is enabled, the current window will get all mouse events, but unlike relative mode, no change is made to the cursor and it is not restrained to your window.
	/// </para>
	/// <para>
	/// This function may also deny mouse input to other windows--both those in your application and others on the system--so you should use this function sparingly, and in small bursts.
	/// For example, you might want to track the mouse while the user is dragging something, until the user releases a mouse button.
	/// It is not recommended that you capture the mouse for long periods of time, such as the entire time your app is running.
	/// For that, you should probably use <see href="https://wiki.libsdl.org/SDL3/SDL_SetWindowRelativeMouseMode">SDL_SetWindowRelativeMouseMode</see>() or <see href="https://wiki.libsdl.org/SDL3/SDL_SetWindowMouseGrab">SDL_SetWindowMouseGrab</see>(), depending on your goals.
	/// </para>
	/// <para>
	/// While captured, mouse events still report coordinates relative to the current (foreground) window, but those coordinates may be outside the bounds of the window (including negative values).
	/// Capturing is only allowed for the foreground window. If the window loses focus while capturing, the capture will be disabled automatically.
	/// </para>
	/// <para>
	/// While capturing is enabled, the current window will have the <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_MOUSE_CAPTURE"><c>SDL_WINDOW_MOUSE_CAPTURE</c></see> flag set.
	/// </para>
	/// <para>
	/// Please note that SDL will attempt to "auto capture" the mouse while the user is pressing a button; this is to try and make mouse behavior more consistent between platforms, and deal with the common case of a user dragging the mouse outside of the window.
	/// This means that if you are calling <see href="https://wiki.libsdl.org/SDL3/SDL_CaptureMouse">SDL_CaptureMouse</see>() only to deal with this situation, you do not have to (although it is safe to do so).
	/// If this causes problems for your app, you can disable auto capture by setting the <see href="https://wiki.libsdl.org/SDL3/SDL_HINT_MOUSE_AUTO_CAPTURE"><c>SDL_HINT_MOUSE_AUTO_CAPTURE</c></see> hint to zero.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CaptureMouse">SDL_CaptureMouse</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial CBool SDL_CaptureMouse(CBool enabled);

	/// <summary>
	/// Queries the platform for the asynchronous mouse button state and the desktop-relative platform-cursor position
	/// </summary>
	/// <param name="x">A pointer to receive the platform-cursor's x-position from the desktop's top left corner, can be NULL if unused</param>
	/// <param name="y">A pointer to receive the platform-cursor's y-position from the desktop's top left corner, can be NULL if unused</param>
	/// <returns>Returns a 32-bit bitmask of the button state that can be bitwise-compared against the <see href="https://wiki.libsdl.org/SDL3/SDL_BUTTON_MASK">SDL_BUTTON_MASK</see>(X) macro</returns>
	/// <remarks>
	/// <para>
	/// This function immediately queries the platform for the most recent asynchronous state, more costly than retrieving SDL's cached state in <see href="https://wiki.libsdl.org/SDL3/SDL_GetMouseState">SDL_GetMouseState</see>().
	/// </para>
	/// <para>
	/// Passing non-NULL pointers to <c><paramref name="x"/></c> or <c><paramref name="y"/></c> will write the destination with respective x or y coordinates relative to the desktop.
	/// </para>
	/// <para>
	/// In Relative Mode, the platform-cursor's position usually contradicts the SDL-cursor's position as manually calculated from <see href="https://wiki.libsdl.org/SDL3/SDL_GetMouseState">SDL_GetMouseState</see>() and <see href="https://wiki.libsdl.org/SDL3/SDL_GetWindowPosition">SDL_GetWindowPosition</see>.
	/// </para>
	/// <para>
	/// This function can be useful if you need to track the mouse outside of a specific window and <see href="https://wiki.libsdl.org/SDL3/SDL_CaptureMouse">SDL_CaptureMouse</see>() doesn't fit your needs.
	/// For example, it could be useful if you need to track the mouse while dragging a window, where coordinates relative to a window might not be in sync at all times.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetGlobalMouseState">SDL_GetGlobalMouseState</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial MouseButtonFlags SDL_GetGlobalMouseState(float* x, float* y);

	/// <summary>
	/// Gets a list of currently connected mice
	/// </summary>
	/// <param name="count">A pointer filled in with the number of mice returned, may be NULL</param>
	/// <returns>
	///	Returns a 0 terminated array of mouse instance IDs or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information.
	///	This should be freed with <see href="https://wiki.libsdl.org/SDL3/SDL_free">SDL_free</see>() when it is no longer needed.
	///	</returns>
	///	<remarks>
	///	<para>
	///	Note that this will include any device or virtual driver that includes mouse functionality, including some game controllers, KVM switches, etc.
	///	You should wait for input from a device before you consider it actively in use.
	///	</para>
	///	<para>
	///	This function should only be called on the main thread.
	///	</para>
	///	</remarks>
	///	<seealso href="https://wiki.libsdl.org/SDL3/SDL_GetMice">SDL_GetMice</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial uint* SDL_GetMice(int* count);

	/// <summary>
	/// Gets the window which currently has mouse focus
	/// </summary>
	/// <returns>Returns the window with mouse focus</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetMouseFocus">SDL_GetMouseFocus</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial Window.SDL_Window* SDL_GetMouseFocus();

	/// <summary>
	/// Gets the name of a mouse
	/// </summary>
	/// <param name="instance_id">The mouse instance ID</param>
	/// <returns>Returns the name of the selected mouse, or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function returns "" if the mouse doesn't have a name.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetMouseNameForID">SDL_GetMouseNameForID</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial byte* SDL_GetMouseNameForID(uint instance_id);

	/// <summary>
	/// Queries SDL's cache for the synchronous mouse button state and the window-relative SDL-cursor position
	/// </summary>
	/// <param name="x">A pointer to receive the SDL-cursor's x-position from the focused window's top left corner, can be NULL if unused</param>
	/// <param name="y">A pointer to receive the SDL-cursor's y-position from the focused window's top left corner, can be NULL if unused</param>
	/// <returns>Returns a 32-bit bitmask of the button state that can be bitwise-compared against the <see href="https://wiki.libsdl.org/SDL3/SDL_BUTTON_MASK">SDL_BUTTON_MASK</see>(X) macro</returns>
	/// <remarks>
	/// <para>
	/// This function returns the cached synchronous state as SDL understands it from the last pump of the event queue.
	/// </para>
	/// <para>
	/// To query the platform for immediate asynchronous state, use <see href="https://wiki.libsdl.org/SDL3/SDL_GetGlobalMouseState">SDL_GetGlobalMouseState</see>.
	/// </para>
	/// <para>
	/// Passing non-NULL pointers to x or y will write the destination with respective x or y coordinates relative to the focused window.
	/// </para>
	/// <para>
	/// In Relative Mode, the platform-cursor's position usually contradicts the SDL-cursor's position as manually calculated from <see href="https://wiki.libsdl.org/SDL3/SDL_GetGlobalMouseState">SDL_GetGlobalMouseState</see>() and <see href="https://wiki.libsdl.org/SDL3/SDL_GetWindowPosition">SDL_GetWindowPosition</see>.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetMouseState">SDL_GetMouseState</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial MouseButtonFlags SDL_GetMouseState(float* x, float* y);

	/// <summary>
	/// Queries SDL's cache for the synchronous mouse button state and accumulated mouse delta since last call
	/// </summary>
	/// <param name="x">A pointer to receive the x mouse delta accumulated since last call, can be NULL if unused</param>
	/// <param name="y">A pointer to receive the y mouse delta accumulated since last call, can be NULL if unused</param>
	/// <returns>Returns a 32-bit bitmask of the button state that can be bitwise-compared against the <see href="https://wiki.libsdl.org/SDL3/SDL_BUTTON_MASK">SDL_BUTTON_MASK</see>(X) macro</returns>
	/// <remarks>
	/// <para>
	/// This function returns the cached synchronous state as SDL understands it from the last pump of the event queue.
	/// </para>
	/// <para>
	/// To query the platform for immediate asynchronous state, use <see href="https://wiki.libsdl.org/SDL3/SDL_GetGlobalMouseState">SDL_GetGlobalMouseState</see>.
	/// </para>
	/// <para>
	/// Passing non-NULL pointers to x or y will write the destination with respective x or y deltas accumulated since the last call to this function (or since event initialization).
	/// </para>
	/// <para>
	/// This function is useful for reducing overhead by processing relative mouse inputs in one go per-frame instead of individually per-event, at the expense of losing the order between events within the frame (e.g. quickly pressing and releasing a button within the same frame).
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetRelativeMouseState">SDL_GetRelativeMouseState</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial MouseButtonFlags SDL_GetRelativeMouseState(float* x, float* y);

	/// <summary>
	/// Returns whether a mouse is currently connected
	/// </summary>
	/// <returns>Returns true if a mouse is connected, false otherwise</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HasMouse">SDL_HasMouse</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial CBool SDL_HasMouse();

#if SDL3_4_0_OR_GREATER

	/// <summary>
	/// Sets a user-defined function by which to transform relative mouse inputs
	/// </summary>
	/// <param name="callback">A callback used to transform relative mouse motion, or NULL for default behavior</param>
	/// <param name="userdata">A pointer that will be passed to <c><paramref name="callback"/></c></param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This overrides the relative system scale and relative speed scale hints. Should be called prior to enabling relative mouse mode, fails otherwise.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetRelativeMouseTransform">SDL_SetRelativeMouseTransform</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetRelativeMouseTransform(SDL_MouseMotionTransformCallback callback, void* userdata);

#endif

	/// <summary>
	/// Moves the mouse to the given position in global screen space
	/// </summary>
	/// <param name="x">The x coordinate</param>
	/// <param name="y">The y coordinate</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function generates a mouse motion event.
	/// </para>
	/// <para>
	/// A failure of this function usually means that it is unsupported by a platform.
	/// </para>
	/// <para>
	/// Note that this function will appear to succeed, but not actually move the mouse when used over Microsoft Remote Desktop.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_WarpMouseGlobal">SDL_WarpMouseGlobal</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_WarpMouseGlobal(float x, float y);

	/// <summary>
	/// Moves the mouse cursor to the given position within the window
	/// </summary>
	/// <param name="window">The window to move the mouse into, or NULL for the current mouse focus</param>
	/// <param name="x">The x coordinate within the window</param>
	/// <param name="y">The y coordinate within the window</param>
	/// <remarks>
	/// <para>
	/// This function generates a mouse motion event if relative mode is not enabled. If relative mode is enabled,
	/// you can force mouse events for the warp by setting the <see href="https://wiki.libsdl.org/SDL3/SDL_HINT_MOUSE_RELATIVE_WARP_MOTION">SDL_HINT_MOUSE_RELATIVE_WARP_MOTION</see> hint.
	/// </para>
	/// <para>
	/// Note that this function will appear to succeed, but not actually move the mouse when used over Microsoft Remote Desktop.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_WarpMouseInWindow">SDL_WarpMouseInWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_WarpMouseInWindow(Window.SDL_Window* window, float x, float y);
}
