using Sdl3Sharp.Events;
using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using Sdl3Sharp.Video.Coloring;
using Sdl3Sharp.Video.Drawing;
using Sdl3Sharp.Video.Rendering;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using unsafe SDL_HitTest = delegate* unmanaged[Cdecl]<Sdl3Sharp.Video.Windowing.Window.SDL_Window*, Sdl3Sharp.Video.Drawing.Point<int>*, void*, Sdl3Sharp.Video.Windowing.HitTestResult>;

namespace Sdl3Sharp.Video.Windowing;

partial class Window
{
	// Used for opaque pointers
	[StructLayout(LayoutKind.Sequential, Size = 0)]
	internal readonly struct SDL_Window;

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe CBool EventWatchResizeOrDestroy(void* userdata, Event* @event)
	{
		if (Unsafe.AsRef<Event>(@event).TryAsReadOnly<WindowEvent>(out var windowEventRef))
		{
			ref readonly var windowEvent = ref windowEventRef.GetReferenceOrNull();

			if (userdata is not null && GCHandle.FromIntPtr(unchecked((IntPtr)userdata)) is { IsAllocated: true, Target: Window { Id: var windowId } window } && windowEvent.WindowId == windowId)
			{
				switch (windowEvent)
				{
					case { Type: EventType.WindowResized }:
						// resizing the window invalidates the surfaces that's associated with it,
						// so we dispose the existing associated WindowSurface, if any
						// we don't destroy the surface, because SDL already did that while invalidating the surface after the resize
						if (window.mSurface is not null)
						{
							window.mSurface.DontDestroy = true;
							window.mSurface.Dispose();
							window.mSurface = null;
						}
						break;

					case { Type: EventType.WindowDestroyed }:
						window.Dispose();
						break;
				}
			}
		}

		return default; // The return value doesn't matter in event watches, it's ignored by SDL
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private static unsafe HitTestResult HitTestImpl(SDL_Window* win, Point<int>* area, void* data)
	{
		if (data is not null && GCHandle.FromIntPtr(unchecked((IntPtr)data)) is { IsAllocated: true, Target: Window { mWindow: var windowPtr } window }
		    && win == windowPtr)
		{
			window.mHitTest?.Invoke(window, in Unsafe.AsRef<Point<int>>(area));
		}

		return HitTestResult.Normal; // default to normal
	}

	/// <summary>
	/// Creates a child popup window of the specified parent window
	/// </summary>
	/// <param name="parent">The parent of the window, must not be NULL</param>
	/// <param name="offset_x">The x position of the popup window relative to the origin of the parent</param>
	/// <param name="offset_y">The y position of the popup window relative to the origin of the parent window</param>
	/// <param name="w">The width of the window</param>
	/// <param name="h">The height of the window</param>
	/// <param name="flags"><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_TOOLTIP">SDL_WINDOW_TOOLTIP</see> or <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_POPUP_MENU">SDL_WINDOW_POPUP_MENU</see>, and zero or more additional <see href="https://wiki.libsdl.org/SDL3/SDL_WindowFlags">SDL_WindowFlags</see> OR'd together</param>
	/// <returns>Returns the window that was created or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The window size is a request and may be different than expected based on the desktop layout and window manager policies.
	/// Your application should be prepared to handle a window of any size.
	/// </para>
	/// <para>
	/// The flags parameter <em>must</em> contain at least one of the following:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_TOOLTIP"><c>SDL_WINDOW_TOOLTIP</c></see></term>
	///			<description>The popup window is a tooltip and will not pass any input events</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_POPUP_MENU"><c>SDL_WINDOW_POPUP_MENU</c></see></term>
	///			<description>The popup window is a popup menu. The topmost popup menu will implicitly gain the keyboard focus.</description>
	///		</item>
	/// </list>
	/// The following flags are not relevant to popup window creation and will be ignored:
	/// <list type="bullet">
	/// <item><description><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_MINIMIZED"><c>SDL_WINDOW_MINIMIZED</c></see></description></item>
	/// <item><description><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_MAXIMIZED"><c>SDL_WINDOW_MAXIMIZED</c></see></description></item>
	/// <item><description><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_FULLSCREEN"><c>SDL_WINDOW_FULLSCREEN</c></see></description></item>
	/// <item><description><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_BORDERLESS"><c>SDL_WINDOW_BORDERLESS</c></see></description></item>
	/// </list>
	/// The following flags are incompatible with popup window creation and will cause it to fail:
	/// <list type="bullet">
	/// <item><description><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_UTILITY"><c>SDL_WINDOW_UTILITY</c></see></description></item>
	/// <item><description><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_MODAL"><c>SDL_WINDOW_MODAL</c></see></description></item>
	/// </list>
	/// </para>
	/// <para>
	/// The parent parameter <em>must</em> be non-null and a valid window. The parent of a popup window can be either a regular, toplevel window, or another popup window.
	/// </para>
	/// <para>
	/// Popup windows cannot be minimized, maximized, made fullscreen, raised, flash, be made a modal window, be the parent of a toplevel window, or grab the mouse and/or keyboard. Attempts to do so will fail.
	/// </para>
	/// <para>
	/// Popup windows implicitly do not have a border/decorations and do not appear on the taskbar/dock or in lists of windows such as alt-tab menus.
	/// </para>
	/// <para>
	/// By default, popup window positions will automatically be constrained to keep the entire window within display bounds.
	/// This can be overridden with the <see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_CONSTRAIN_POPUP_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_CONSTRAIN_POPUP_BOOLEAN</c></see> property.
	/// </para>
	/// <para>
	/// By default, popup menus will automatically grab keyboard focus from the parent when shown.
	/// This behavior can be overridden by setting the <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_NOT_FOCUSABLE"><c>SDL_WINDOW_NOT_FOCUSABLE</c></see> flag,
	/// setting the <see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_FOCUSABLE_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_FOCUSABLE_BOOLEAN</c></see> property to false,
	/// or toggling it after creation via the <c>SDL_SetWindowFocusable()</c> function.
	/// </para>
	/// <para>
	/// If a parent window is hidden or destroyed, any child popup windows will be recursively hidden or destroyed as well. Child popup windows not explicitly hidden will be restored when the parent is shown.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreatePopupWindow">SDL_CreatePopupWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Window* SDL_CreatePopupWindow(SDL_Window* parent, int offset_x, int offset_y, int w, int h, WindowFlags flags);

	/// <summary>
	/// Creates a window with the specified dimensions and flags
	/// </summary>
	/// <param name="title">The title of the window, in UTF-8 encoding</param>
	/// <param name="w">The width of the window</param>
	/// <param name="h">The height of the window</param>
	/// <param name="flags">0, or one or more <see href="https://wiki.libsdl.org/SDL3/SDL_WindowFlags">SDL_WindowFlags</see> OR'd together</param>
	/// <returns>Returns the window that was created or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The window size is a request and may be different than expected based on the desktop layout and window manager policies. Your application should be prepared to handle a window of any size.
	/// </para>
	/// <para>
	/// <c><paramref name="flags"/></c> may be any of the following OR'd together:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_FULLSCREEN"><c>SDL_WINDOW_FULLSCREEN</c></see></term>
	///			<description>Fullscreen window at desktop resolution</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_OPENGL"><c>SDL_WINDOW_OPENGL</c></see></term>
	///			<description>Window usable with an OpenGL context</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_HIDDEN"><c>SDL_WINDOW_HIDDEN</c></see></term>
	///			<description>Window is not visible</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_BORDERLESS"><c>SDL_WINDOW_BORDERLESS</c></see></term>
	///			<description>No window decoration</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_RESIZABLE"><c>SDL_WINDOW_RESIZABLE</c></see></term>
	///			<description>Window can be resized</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_MINIMIZED"><c>SDL_WINDOW_MINIMIZED</c></see></term>
	///			<description>Window is minimized</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_MAXIMIZED"><c>SDL_WINDOW_MAXIMIZED</c></see></term>
	///			<description>Window is maximized</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_MOUSE_GRABBED"><c>SDL_WINDOW_MOUSE_GRABBED</c></see></term>
	///			<description>Window has grabbed mouse focus</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_INPUT_FOCUS"><c>SDL_WINDOW_INPUT_FOCUS</c></see></term>
	///			<description>Window has input focus</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_MOUSE_FOCUS"><c>SDL_WINDOW_MOUSE_FOCUS</c></see></term>
	///			<description>Window has mouse focus</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_EXTERNAL"><c>SDL_WINDOW_EXTERNAL</c></see></term>
	///			<description>Window not created by SDL</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_MODAL"><c>SDL_WINDOW_MODAL</c></see></term>
	///			<description>Window is modal</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_HIGH_PIXEL_DENSITY"><c>SDL_WINDOW_HIGH_PIXEL_DENSITY</c></see></term>
	///			<description>Window uses high pixel density back buffer if possible</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_MOUSE_CAPTURE"><c>SDL_WINDOW_MOUSE_CAPTURE</c></see></term>
	///			<description>Window has mouse captured (unrelated to MOUSE_GRABBED)</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_ALWAYS_ON_TOP"><c>SDL_WINDOW_ALWAYS_ON_TOP</c></see></term>
	///			<description>Window should always be above others</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_UTILITY"><c>SDL_WINDOW_UTILITY</c></see></term>
	///			<description>Window should be treated as a utility window, not showing in the task bar and window list</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_TOOLTIP"><c>SDL_WINDOW_TOOLTIP</c></see></term>
	///			<description>Window should be treated as a tooltip and does not get mouse or keyboard focus, requires a parent window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_POPUP_MENU"><c>SDL_WINDOW_POPUP_MENU</c></see></term>
	///			<description>Window should be treated as a popup menu, requires a parent window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_KEYBOARD_GRABBED"><c>SDL_WINDOW_KEYBOARD_GRABBED</c></see></term>
	///			<description>Window has grabbed keyboard input</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_VULKAN"><c>SDL_WINDOW_VULKAN</c></see></term>
	///			<description>Window usable with a Vulkan instance</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_METAL"><c>SDL_WINDOW_METAL</c></see></term>
	///			<description>Window usable with a Metal instance</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_TRANSPARENT"><c>SDL_WINDOW_TRANSPARENT</c></see></term>
	///			<description>Window with transparent buffer</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_NOT_FOCUSABLE"><c>SDL_WINDOW_NOT_FOCUSABLE</c></see></term>
	///			<description>Window should not be focusable</description>
	///		</item>
	/// </list>
	/// The <see href="https://wiki.libsdl.org/SDL3/SDL_Window">SDL_Window</see> will be shown if <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_HIDDEN">SDL_WINDOW_HIDDEN</see> is not set.
	/// If hidden at creation time, <see href="https://wiki.libsdl.org/SDL3/SDL_ShowWindow">SDL_ShowWindow</see>() can be used to show it later.
	/// </para>
	/// <para>
	/// On Apple's macOS, you <em>must</em> set the NSHighResolutionCapable Info.plist property to YES, otherwise you will not receive a High-DPI OpenGL canvas.
	/// </para>
	/// <para>
	/// The window pixel size may differ from its window coordinate size if the window is on a high pixel density display.
	/// Use <see href="https://wiki.libsdl.org/SDL3/SDL_GetWindowSize">SDL_GetWindowSize</see>() to query the client area's size in window coordinates, and <see href="https://wiki.libsdl.org/SDL3/SDL_GetWindowSizeInPixels">SDL_GetWindowSizeInPixels</see>() or <see href="https://wiki.libsdl.org/SDL3/SDL_GetRenderOutputSize">SDL_GetRenderOutputSize</see>() to query the drawable size in pixels.
	/// Note that the drawable size can vary after the window is created and should be queried again if you get an <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED">SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED</see> event.
	/// </para>
	/// <para>
	/// If the window is created with any of the <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_OPENGL">SDL_WINDOW_OPENGL</see> or <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_VULKAN">SDL_WINDOW_VULKAN</see> flags,
	/// then the corresponding LoadLibrary function (<see href="https://wiki.libsdl.org/SDL3/SDL_GL_LoadLibrary">SDL_GL_LoadLibrary</see> or <see href="https://wiki.libsdl.org/SDL3/SDL_Vulkan_LoadLibrary">SDL_Vulkan_LoadLibrary</see>) is called and the corresponding UnloadLibrary function is called by <see href="https://wiki.libsdl.org/SDL3/SDL_DestroyWindow">SDL_DestroyWindow</see>().
	/// </para>
	/// <para>
	/// If <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_VULKAN">SDL_WINDOW_VULKAN</see> is specified and there isn't a working Vulkan driver, <see href="https://wiki.libsdl.org/SDL3/SDL_CreateWindow">SDL_CreateWindow</see>() will fail,
	/// because <see href="https://wiki.libsdl.org/SDL3/SDL_Vulkan_LoadLibrary">SDL_Vulkan_LoadLibrary</see>() will fail.
	/// </para>
	/// <para>
	/// If <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_METAL">SDL_WINDOW_METAL</see> is specified on an OS that does not support Metal, <see href="https://wiki.libsdl.org/SDL3/SDL_CreateWindow">SDL_CreateWindow</see>() will fail.
	/// </para>
	/// <para>
	/// If you intend to use this window with an <see href="https://wiki.libsdl.org/SDL3/SDL_Renderer">SDL_Renderer</see>, you should use <see href="https://wiki.libsdl.org/SDL3/SDL_CreateWindowAndRenderer">SDL_CreateWindowAndRenderer</see>() instead of this function, to avoid window flicker.
	/// </para>
	/// <para>
	/// On non-Apple devices, SDL requires you to either not link to the Vulkan loader or link to a dynamic library version. This limitation may be removed in a future version of SDL.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateWindow">SDL_CreateWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Window* SDL_CreateWindow(byte* title, int w, int h, WindowFlags flags);

	/// <summary>
	/// Create a window and default renderer
	/// </summary>
	/// <param name="title">The title of the window, in UTF-8 encoding</param>
	/// <param name="width">The width of the window</param>
	/// <param name="height">The height of the window</param>
	/// <param name="window_flags">The flags used to create the window (see <see href="https://wiki.libsdl.org/SDL3/SDL_CreateWindow">SDL_CreateWindow</see>())</param>
	/// <param name="window">A pointer filled with the window, or NULL on error</param>
	/// <param name="renderer">A pointer filled with the renderer, or NULL on error</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateWindowAndRenderer">SDL_CreateWindowAndRenderer</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_CreateWindowAndRenderer(byte* title, int width, int height, WindowFlags window_flags, SDL_Window** window, Renderer.SDL_Renderer** renderer);

	/// <summary>
	/// Create a window with the specified properties
	/// </summary>
	/// <param name="props">The properties to use</param>
	/// <returns>Returns the window that was created or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The window size is a request and may be different than expected based on the desktop layout and window manager policies.
	/// Your application should be prepared to handle a window of any size.
	/// </para>
	/// <para>
	/// These are the supported properties:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_ALWAYS_ON_TOP_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_ALWAYS_ON_TOP_BOOLEAN</c></see></term>
	///			<description>true if the window should be always on top</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_BORDERLESS_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_BORDERLESS_BOOLEAN</c></see></term>
	///			<description>true if the window has no window decoration</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_CONSTRAIN_POPUP_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_CONSTRAIN_POPUP_BOOLEAN</c></see></term>
	///			<description>true if the "tooltip" and "menu" window types should be automatically constrained to be entirely within display bounds (default), false if no constraints on the position are desired</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_EXTERNAL_GRAPHICS_CONTEXT_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_EXTERNAL_GRAPHICS_CONTEXT_BOOLEAN</c></see></term>
	///			<description>true if the window will be used with an externally managed graphics context</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_FOCUSABLE_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_FOCUSABLE_BOOLEAN</c></see></term>
	///			<description>true if the window should accept keyboard input (defaults true)</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_FULLSCREEN_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_FOCUSABLE_BOOLEAN</c></see></term>
	///			<description>true if the window should start in fullscreen mode at desktop resolution</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_HEIGHT_NUMBER"><c>SDL_PROP_WINDOW_CREATE_HEIGHT_NUMBER</c></see></term>
	///			<description>The height of the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_HIDDEN_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_HIDDEN_BOOLEAN</c></see></term>
	///			<description>true if the window should start hidden</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_HIGH_PIXEL_DENSITY_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_HIGH_PIXEL_DENSITY_BOOLEAN</c></see></term>
	///			<description>true if the window uses a high pixel density buffer if possible</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_MAXIMIZED_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_MAXIMIZED_BOOLEAN</c></see></term>
	///			<description>true if the window should start maximized</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_MENU_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_MENU_BOOLEAN</c></see></term>
	///			<description>true if the window is a popup menu</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_METAL_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_METAL_BOOLEAN</c></see></term>
	///			<description>true if the window will be used with Metal rendering</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_MINIMIZED_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_MINIMIZED_BOOLEAN</c></see></term>
	///			<description>true if the window should start minimized</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_MODAL_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_MINIMIZED_BOOLEAN</c></see></term>
	///			<description>true if the window is modal to its parent</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_MOUSE_GRABBED_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_MINIMIZED_BOOLEAN</c></see></term>
	///			<description>true if the window starts with grabbed mouse focus</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_OPENGL_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_OPENGL_BOOLEAN</c></see></term>
	///			<description>true if the window will be used with OpenGL rendering</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_PARENT_POINTER"><c>SDL_PROP_WINDOW_CREATE_PARENT_POINTER</c></see></term>
	///			<description>An <see href="https://wiki.libsdl.org/SDL3/SDL_Window">SDL_Window</see> that will be the parent of this window, required for windows with the "tooltip", "menu", and "modal" properties</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_RESIZABLE_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_RESIZABLE_BOOLEAN</c></see></term>
	///			<description>true if the window should be resizable</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_TITLE_STRING"><c>SDL_PROP_WINDOW_CREATE_TITLE_STRING</c></see></term>
	///			<description>The title of the window, in UTF-8 encoding</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_TRANSPARENT_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_TRANSPARENT_BOOLEAN</c></see></term>
	///			<description>true if the window show transparent in the areas with alpha of 0</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_TOOLTIP_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_TOOLTIP_BOOLEAN</c></see></term>
	///			<description>true if the window is a tooltip</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_UTILITY_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_UTILITY_BOOLEAN</c></see></term>
	///			<description>true if the window is a utility window, not showing in the task bar and window list</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_VULKAN_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_VULKAN_BOOLEAN</c></see></term>
	///			<description>true if the window will be used with Vulkan rendering</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_WIDTH_NUMBER"><c>SDL_PROP_WINDOW_CREATE_WIDTH_NUMBER</c></see></term>
	///			<description>The width of the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_X_NUMBER"><c>SDL_PROP_WINDOW_CREATE_X_NUMBER</c></see></term>
	///			<description>
	///				The x position of the window, or <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOWPOS_CENTERED"><c>SDL_WINDOWPOS_CENTERED</c></see>, defaults to <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOWPOS_UNDEFINED"><c>SDL_WINDOWPOS_UNDEFINED</c></see>.
	///				This is relative to the parent for windows with the "tooltip" or "menu" property set.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_Y_NUMBER"><c>SDL_PROP_WINDOW_CREATE_Y_NUMBER</c></see></term>
	///			<description>
	///				The y position of the window, or <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOWPOS_CENTERED"><c>SDL_WINDOWPOS_CENTERED</c></see>, defaults to <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOWPOS_UNDEFINED"><c>SDL_WINDOWPOS_UNDEFINED</c></see>.
	///				This is relative to the parent for windows with the "tooltip" or "menu" property set.
	///			</description>
	///		</item>
	///	</list>
	///	These are additional supported properties on macOS:
	///	<list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_COCOA_WINDOW_POINTER"><c>SDL_PROP_WINDOW_CREATE_COCOA_WINDOW_POINTER</c></see></term>
	///			<description>The <c>(__unsafe_unretained)</c> NSWindow associated with the window, if you want to wrap an existing window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_COCOA_VIEW_POINTER"><c>SDL_PROP_WINDOW_CREATE_COCOA_VIEW_POINTER</c></see></term>
	///			<description>The <c>(__unsafe_unretained)</c> NSView associated with the window, defaults to <c>[window contentView]</c></description>
	///		</item>
	///	</list>
	///	These are additional supported properties on iOS, tvOS, and visionOS:
	///	<list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_WINDOWSCENE_POINTER"><c>SDL_PROP_WINDOW_CREATE_WINDOWSCENE_POINTER</c></see></term>
	///			<description>The <c>(__unsafe_unretained)</c> UIWindowScene associated with the window, defaults to the active window scene</description>
	///		</item>
	///	</list>
	///	These are additional supported properties on Wayland:
	///	<list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_WAYLAND_SURFACE_ROLE_CUSTOM_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_WAYLAND_SURFACE_ROLE_CUSTOM_BOOLEAN</c></see></term>
	///			<description>
	///				true if the application wants to use the Wayland surface for a custom role and does not want it attached to an XDG toplevel window.
	///				See <see href="https://wiki.libsdl.org/SDL3/README-wayland">README-wayland</see> for more information on using custom surfaces.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_WAYLAND_CREATE_EGL_WINDOW_BOOLEAN"><c>SDL_PROP_WINDOW_CREATE_WAYLAND_CREATE_EGL_WINDOW_BOOLEAN</c></see></term>
	///			<description>true if the application wants an associated <c>wl_egl_window</c> object to be created and attached to the window, even if the window does not have the OpenGL property or <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_OPENGL">SDL_WINDOW_OPENGL</see> flag set</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_WAYLAND_WL_SURFACE_POINTER"><c>SDL_PROP_WINDOW_CREATE_WAYLAND_WL_SURFACE_POINTER</c></see></term>
	///			<description>
	///				The wl_surface associated with the window, if you want to wrap an existing window.
	///				See <see href="https://wiki.libsdl.org/SDL3/README-wayland">README-wayland</see> for more information.
	///			</description>
	///		</item>
	///	</list>
	///	These are additional supported properties on Windows:
	///	<list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_WIN32_HWND_POINTER"><c>SDL_PROP_WINDOW_CREATE_WIN32_HWND_POINTER</c></see></term>
	///			<description>The HWND associated with the window, if you want to wrap an existing window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_WIN32_PIXEL_FORMAT_HWND_POINTER"><c>SDL_PROP_WINDOW_CREATE_WIN32_PIXEL_FORMAT_HWND_POINTER</c></see></term>
	///			<description>Optional, another window to share pixel format with, useful for OpenGL windows</description>
	///		</item>
	///	</list>
	///	These are additional supported properties with X11:
	///	<list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_X11_WINDOW_NUMBER"><c>SDL_PROP_WINDOW_CREATE_X11_WINDOW_NUMBER</c></see></term>
	///			<description>The X11 Window associated with the window, if you want to wrap an existing window</description>
	///		</item>
	///	</list>
	/// </para>
	/// <para>
	/// The window is implicitly shown if the "hidden" property is not set.
	/// </para>
	/// <para>
	/// These are additional supported properties with Emscripten:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_EMSCRIPTEN_CANVAS_ID_STRING"><c>SDL_PROP_WINDOW_CREATE_EMSCRIPTEN_CANVAS_ID_STRING</c></see></term>
	///			<description>The id given to the canvas element. This should start with a '#' sign.</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_EMSCRIPTEN_KEYBOARD_ELEMENT_STRING"><c>SDL_PROP_WINDOW_CREATE_EMSCRIPTEN_KEYBOARD_ELEMENT_STRING</c></see></term>
	///			<description>
	///				Override the binding element for keyboard inputs for this canvas. The variable can be one of:
	///				<list type="bullet">
	///					<item>
	///						<term>"#window"</term>
	///						<description>The javascript window object (default)</description>
	///					</item>
	///					<item>
	///						<term>"#document"</term>
	///						<description>The javascript document object</description>
	///					</item>
	///					<item>
	///						<term>"#screen"</term>
	///						<description>The javascript window.screen object</description>
	///					</item>
	///					<item>
	///						<term>"#canvas"</term>
	///						<description>The WebGL canvas element</description>
	///					</item>
	///					<item>
	///						<term>"#none"</term>
	///						<description>Don't bind anything at all</description>
	///					</item>
	///					<item>
	///						<term>Any other string without a leading # sign applies to the element on the page with that ID</term>
	///						<description>Windows with the "tooltip" and "menu" properties are popup windows and have the behaviors and guidelines outlined in <see href="https://wiki.libsdl.org/SDL3/SDL_CreatePopupWindow">SDL_CreatePopupWindow</see>()</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///	</list>
	/// </para>
	/// <para>
	/// If this window is being created to be used with an <see href="https://wiki.libsdl.org/SDL3/SDL_Renderer">SDL_Renderer</see>, you should not add a graphics API specific property (<see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_OPENGL_BOOLEAN">SDL_PROP_WINDOW_CREATE_OPENGL_BOOLEAN</see>, etc), as SDL will handle that internally when it chooses a renderer.
	/// However, SDL might need to recreate your window at that point, which may cause the window to appear briefly, and then flicker as it is recreated.
	/// The correct approach to this is to create the window with the <see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_CREATE_HIDDEN_BOOLEAN">SDL_PROP_WINDOW_CREATE_HIDDEN_BOOLEAN</see> property set to true, then create the renderer, then show the window with <see href="https://wiki.libsdl.org/SDL3/SDL_ShowWindow">SDL_ShowWindow</see>().
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateWindowWithProperties">SDL_CreateWindowWithProperties</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Window* SDL_CreateWindowWithProperties(uint props);

	/// <summary>
	/// Destroys a window
	/// </summary>
	/// <param name="window">The window to destroy</param>
	/// <remarks>
	/// <para>
	/// Any child windows owned by the window will be recursively destroyed as well.
	/// </para>
	/// <para>
	/// Note that on some platforms, the visible window may not actually be removed from the screen until the SDL event loop is pumped again, even though the <see href="https://wiki.libsdl.org/SDL3/SDL_Window">SDL_Window</see> is no longer valid after this call.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_DestroyWindow">SDL_DestroyWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_DestroyWindow(SDL_Window* window);

	/// <summary>
	/// Requests a window to demand attention from the user
	/// </summary>
	/// <param name="window">The window to be flashed</param>
	/// <param name="operation">The operation to perform.</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_FlashWindow">SDL_FlashWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_FlashWindow(SDL_Window* window, FlashOperation operation);

	/// <summary>
	/// Gets the display associated with a window
	/// </summary>
	/// <param name="window">The window to query.</param>
	/// <returns>Returns the instance ID of the display containing the center of the window on success or 0 on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetDisplayForWindow">SDL_GetDisplayForWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial uint SDL_GetDisplayForWindow(SDL_Window* window);

	/// <summary>
	/// Gets the window that currently has an input grab enabled
	/// </summary>
	/// <returns>Returns the window if input is grabbed or NULL otherwise</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetGrabbedWindow">SDL_GetGrabbedWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Window* SDL_GetGrabbedWindow();

	/// <summary>
	/// Gets the renderer associated with a window
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns the rendering context on success or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetRenderer">SDL_GetRenderer</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial Renderer.SDL_Renderer* SDL_GetRenderer(SDL_Window* window);

	/// <summary>
	/// Gets the aspect ratio of a window's client area
	/// </summary>
	/// <param name="window">The window to query the width and height from</param>
	/// <param name="min_aspect">A pointer filled in with the minimum aspect ratio of the window, may be NULL</param>
	/// <param name="max_aspect">A pointer filled in with the maximum aspect ratio of the window, may be NULL</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowAspectRatio">SDL_GetWindowAspectRatio</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetWindowAspectRatio(SDL_Window* window, float* min_aspect, float* max_aspect);

	/// <summary>
	/// Gets the size of a window's borders (decorations) around the client area
	/// </summary>
	/// <param name="window">The window to query the size values of the border (decorations) from</param>
	/// <param name="top">Pointer to variable for storing the size of the top border; NULL is permitted</param>
	/// <param name="left">Pointer to variable for storing the size of the left border; NULL is permitted</param>
	/// <param name="bottom">Pointer to variable for storing the size of the bottom border; NULL is permitted</param>
	/// <param name="right">Pointer to variable for storing the size of the right border; NULL is permitted</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// Note: If this function fails (returns false), the size values will be initialized to 0, 0, 0, 0 (if a non-NULL pointer is provided), as if the window in question was borderless.
	/// </para>
	/// <para>
	/// Note: This function may fail on systems where the window has not yet been decorated by the display server (for example, immediately after calling <see href="https://wiki.libsdl.org/SDL3/SDL_CreateWindow">SDL_CreateWindow</see>).
	/// It is recommended that you wait at least until the window has been presented and composited, so that the window system has a chance to decorate the window and provide the border dimensions to SDL.
	/// </para>
	/// <para>
	/// This function also returns false if getting the information is not supported.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowBordersSize">SDL_GetWindowBordersSize</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetWindowBordersSize(SDL_Window* window, int* top, int* left, int* bottom, int* right);

	/// <summary>
	/// Gets the content display scale relative to a window's pixel size
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns the display scale, or 0.0f on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This is a combination of the window pixel density and the display content scale, and is the expected scale for displaying content in this window.
	/// For example, if a 3840x2160 window had a display scale of 2.0, the user expects the content to take twice as many pixels and be the same physical size as if it were being displayed in a 1920x1080 window with a display scale of 1.0.
	/// </para>
	/// <para>
	/// Conceptually this value corresponds to the scale display setting, and is updated when that setting is changed, or the window moves to a display with a different scale setting.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowDisplayScale">SDL_GetWindowDisplayScale</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial float SDL_GetWindowDisplayScale(SDL_Window* window);

	/// <summary>
	/// Gets the window flags
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns a mask of the SDL_WindowFlags associated with <c><paramref name="window"/></c></returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowFlags">SDL_GetWindowFlags</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial WindowFlags SDL_GetWindowFlags(SDL_Window* window);

	/// <summary>
	/// Gets a window from a stored ID
	/// </summary>
	/// <param name="id">The ID of the window</param>
	/// <returns>Returns the window associated with <c><paramref name="id"/></c> or NULL if it doesn't exist; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The numeric ID is what <see href="https://wiki.libsdl.org/SDL3/SDL_WindowEvent">SDL_WindowEvent</see> references, and is necessary to map these events to specific <see href="https://wiki.libsdl.org/SDL3/SDL_Window">SDL_Window</see> objects.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowFromID">SDL_GetWindowFromID</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Window* SDL_GetWindowFromID(uint id);

	/// <summary>
	/// Queries the display mode to use when a window is visible at fullscreen
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns a pointer to the exclusive fullscreen mode to use or NULL for borderless fullscreen desktop mode</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowFullscreenMode">SDL_GetWindowFullscreenMode</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial DisplayMode.SDL_DisplayMode* SDL_GetWindowFullscreenMode(SDL_Window* window);

	/// <summary>
	/// Gets the raw ICC profile data for the screen the window is currently on
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <param name="size">The size of the ICC profile</param>
	/// <returns>
	/// Returns the raw ICC profile data on success or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information.
	/// This should be freed with <see href="https://wiki.libsdl.org/SDL3/SDL_free">SDL_free</see>() when it is no longer needed.
	/// </returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowICCProfile">SDL_GetWindowICCProfile</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void* SDL_GetWindowICCProfile(SDL_Window* window, nuint* size);

	/// <summary>
	/// Gets the numeric ID of a window
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns the ID of the window on success or 0 on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The numeric ID is what <see href="https://wiki.libsdl.org/SDL3/SDL_WindowEvent">SDL_WindowEvent</see> references, and is necessary to map these events to specific <see href="https://wiki.libsdl.org/SDL3/SDL_Window">SDL_Window</see> objects.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowID">SDL_GetWindowID</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial uint SDL_GetWindowID(SDL_Window* window);

	/// <summary>
	/// Gets a window's keyboard grab mode
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns true if keyboard is grabbed, and false otherwise</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowKeyboardGrab">SDL_GetWindowKeyboardGrab</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetWindowKeyboardGrab(SDL_Window* window);

	/// <summary>
	/// Gets the maximum size of a window's client area
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <param name="w">A pointer filled in with the maximum width of the window, may be NULL</param>
	/// <param name="h">A pointer filled in with the maximum height of the window, may be NULL</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowMaximumSize">SDL_GetWindowMaximumSize</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetWindowMaximumSize(SDL_Window* window, int* w, int* h);

	/// <summary>
	/// Gets the minimum size of a window's client area
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <param name="w">A pointer filled in with the minimum width of the window, may be NULL</param>
	/// <param name="h">A pointer filled in with the minimum height of the window, may be NULL</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowMinimumSize">SDL_GetWindowMinimumSize</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetWindowMinimumSize(SDL_Window* window, int* w, int* h);

	/// <summary>
	/// Gets a window's mouse grab mode
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns true if mouse is grabbed, and false otherwise</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowMouseGrab">SDL_GetWindowMouseGrab</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetWindowMouseGrab(SDL_Window* window);

	/// <summary>
	/// Get the mouse confinement rectangle of a window
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns a pointer to the mouse confinement rectangle of a window, or NULL if there isn't one</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowMouseRect">SDL_GetWindowMouseRect</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial Rect<int>* SDL_GetWindowMouseRect(SDL_Window* window);

	/// <summary>
	/// Gets the opacity of a window
	/// </summary>
	/// <param name="window">The window to get the current opacity value from</param>
	/// <returns>Returns the opacity, (0.0f - transparent, 1.0f - opaque), or -1.0f on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// If transparency isn't supported on this platform, opacity will be returned as 1.0f without error.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowOpacity">SDL_GetWindowOpacity</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial float SDL_GetWindowOpacity(SDL_Window* window);

	/// <summary>
	/// Gets parent of a window
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns the parent of the window on success or NULL if the window has no parent</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowParent">SDL_GetWindowParent</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Window* SDL_GetWindowParent(SDL_Window* window);

	/// <summary>
	/// Gets the pixel density of a window
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns the pixel density or 0.0f on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This is a ratio of pixel size to window size. For example, if the window is 1920x1080 and it has a high density back buffer of 3840x2160 pixels, it would have a pixel density of 2.0.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowPixelDensity">SDL_GetWindowPixelDensity</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial float SDL_GetWindowPixelDensity(SDL_Window* window);

	/// <summary>
	/// Gets the pixel format associated with the window
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns the pixel format of the window on success or <see href="https://wiki.libsdl.org/SDL3/SDL_PIXELFORMAT_UNKNOWN">SDL_PIXELFORMAT_UNKNOWN</see> on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information.</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowPixelFormat">SDL_GetWindowPixelFormat</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial PixelFormat SDL_GetWindowPixelFormat(SDL_Window* window);

	/// <summary>
	/// Gets the position of a window
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <param name="x">A pointer filled in with the x position of the window, may be NULL</param>
	/// <param name="y">A pointer filled in with the y position of the window, may be NULL</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This is the current position of the window as last reported by the windowing system.
	/// </para>
	/// <para>
	/// If you do not need the value for one of the positions a NULL may be passed in the <c><paramref name="x"/></c> or <c><paramref name="y"/></c> parameter.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowPosition">SDL_GetWindowPosition</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetWindowPosition(SDL_Window* window, int* x, int* y);

#if SDL3_4_0_OR_GREATER

	/// <summary>
	/// Gets the state of the progress bar for the given window’s taskbar icon
	/// </summary>
	/// <param name="window">The window to get the current progress state from</param>
	/// <returns>Returns the progress state, or <see href="https://wiki.libsdl.org/SDL3/SDL_PROGRESS_STATE_INVALID">SDL_PROGRESS_STATE_INVALID</see> on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowProgressState">SDL_GetWindowProgressState</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial ProgressState SDL_GetWindowProgressState(SDL_Window* window);

	/// <summary>
	/// Gets the value of the progress bar for the given window’s taskbar icon
	/// </summary>
	/// <param name="window">The window to get the current progress value from</param>
	/// <returns>Returns the progress value in the range of [0.0f - 1.0f], or -1.0f on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowProgressValue">SDL_GetWindowProgressValue</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial float SDL_GetWindowProgressValue(SDL_Window* window);

#endif

	/// <summary>
	/// Gets the properties associated with a window
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns a valid property ID on success or 0 on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The following read-only properties are provided by SDL:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_SHAPE_POINTER"><c>SDL_PROP_WINDOW_SHAPE_POINTER</c></see></term>
	///			<description>The surface associated with a shaped window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_HDR_ENABLED_BOOLEAN"><c>SDL_PROP_WINDOW_HDR_ENABLED_BOOLEAN</c></see></term>
	///			<description>
	///				true if the window has HDR headroom above the SDR white point.
	///				This property can change dynamically when <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_HDR_STATE_CHANGED">SDL_EVENT_WINDOW_HDR_STATE_CHANGED</see> is sent.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_SDR_WHITE_LEVEL_FLOAT"><c>SDL_PROP_WINDOW_SDR_WHITE_LEVEL_FLOAT</c></see></term>
	///			<description>
	///				The value of SDR white in the <see href="https://wiki.libsdl.org/SDL3/SDL_COLORSPACE_SRGB_LINEAR">SDL_COLORSPACE_SRGB_LINEAR</see> colorspace.
	///				On Windows this corresponds to the SDR white level in scRGB colorspace, and on Apple platforms this is always 1.0 for EDR content.
	///				This property can change dynamically when <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_HDR_STATE_CHANGED">SDL_EVENT_WINDOW_HDR_STATE_CHANGED</see> is sent.
	///			</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_HDR_HEADROOM_FLOAT"><c>SDL_PROP_WINDOW_HDR_HEADROOM_FLOAT</c></see></term>
	///			<description>
	///				The additional high dynamic range that can be displayed, in terms of the SDR white point.
	///				When HDR is not enabled, this will be 1.0.
	///				This property can change dynamically when <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_HDR_STATE_CHANGED">SDL_EVENT_WINDOW_HDR_STATE_CHANGED</see> is sent.
	///			</description>
	///		</item>
	/// </list>
	/// On Android:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_ANDROID_WINDOW_POINTER"><c>SDL_PROP_WINDOW_ANDROID_WINDOW_POINTER</c></see></term>
	///			<description>The ANativeWindow associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_ANDROID_SURFACE_POINTER"><c>SDL_PROP_WINDOW_ANDROID_SURFACE_POINTER</c></see></term>
	///			<description>The EGLSurface associated with the window</description>
	///		</item>
	/// </list>
	/// On iOS:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_UIKIT_WINDOW_POINTER"><c>SDL_PROP_WINDOW_UIKIT_WINDOW_POINTER</c></see></term>
	///			<description>The <c>(__unsafe_unretained)</c> UIWindow associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_UIKIT_METAL_VIEW_TAG_NUMBER"><c>SDL_PROP_WINDOW_UIKIT_METAL_VIEW_TAG_NUMBER</c></see></term>
	///			<description>The NSInteger tag associated with metal views on the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_UIKIT_OPENGL_FRAMEBUFFER_NUMBER"><c>SDL_PROP_WINDOW_UIKIT_OPENGL_FRAMEBUFFER_NUMBER</c></see></term>
	///			<description>The OpenGL view's framebuffer object. It must be bound when rendering to the screen using OpenGL.</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_UIKIT_OPENGL_RENDERBUFFER_NUMBER"><c>SDL_PROP_WINDOW_UIKIT_OPENGL_RENDERBUFFER_NUMBER</c></see></term>
	///			<description>The OpenGL view's renderbuffer object. It must be bound when <see href="https://wiki.libsdl.org/SDL3/SDL_GL_SwapWindow">SDL_GL_SwapWindow</see> is called.</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_UIKIT_OPENGL_RESOLVE_FRAMEBUFFER_NUMBER"><c>SDL_PROP_WINDOW_UIKIT_OPENGL_RESOLVE_FRAMEBUFFER_NUMBER</c></see></term>
	///			<description>The OpenGL view's resolve framebuffer, when MSAA is used</description>
	///		</item>
	/// </list>
	/// On KMS/DRM:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_KMSDRM_DEVICE_INDEX_NUMBER"><c>SDL_PROP_WINDOW_KMSDRM_DEVICE_INDEX_NUMBER</c></see></term>
	///			<description>The device index associated with the window (e.g. the X in /dev/dri/cardX)</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_KMSDRM_DRM_FD_NUMBER"><c>SDL_PROP_WINDOW_KMSDRM_DRM_FD_NUMBER</c></see></term>
	///			<description>The DRM FD associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_KMSDRM_GBM_DEVICE_POINTER"><c>SDL_PROP_WINDOW_KMSDRM_GBM_DEVICE_POINTER</c></see></term>
	///			<description>The GBM device associated with the window</description>
	///		</item>
	/// </list>
	/// On macOS:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_COCOA_WINDOW_POINTER"><c>SDL_PROP_WINDOW_COCOA_WINDOW_POINTER</c></see></term>
	///			<description>The <c>(__unsafe_unretained)</c> NSWindow associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_COCOA_METAL_VIEW_TAG_NUMBER"><c>SDL_PROP_WINDOW_COCOA_METAL_VIEW_TAG_NUMBER</c></see></term>
	///			<description>The NSInteger tag associated with metal views on the window</description>
	///		</item>
	/// </list>
	/// On OpenVR:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_OPENVR_OVERLAY_ID_NUMBER"><c>SDL_PROP_WINDOW_OPENVR_OVERLAY_ID_NUMBER</c></see></term>
	///			<description>The OpenVR Overlay Handle ID for the associated overlay window</description>
	///		</item>
	/// </list>
	/// On QNX:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_QNX_WINDOW_POINTER"><c>SDL_PROP_WINDOW_QNX_WINDOW_POINTER</c></see></term>
	///			<description>The screen_window_t associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_QNX_SURFACE_POINTER"><c>SDL_PROP_WINDOW_QNX_SURFACE_POINTER</c></see></term>
	///			<description>The EGLSurface associated with the window</description>
	///		</item>
	/// </list>
	/// On Vivante:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_VIVANTE_DISPLAY_POINTER"><c>SDL_PROP_WINDOW_VIVANTE_DISPLAY_POINTER</c></see></term>
	///			<description>The EGLNativeDisplayType associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_VIVANTE_WINDOW_POINTER"><c>SDL_PROP_WINDOW_VIVANTE_WINDOW_POINTER</c></see></term>
	///			<description>The EGLNativeWindowType associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_VIVANTE_SURFACE_POINTER"><c>SDL_PROP_WINDOW_VIVANTE_SURFACE_POINTER</c></see></term>
	///			<description>The EGLSurface associated with the window</description>
	///		</item>
	/// </list>
	/// On Windows:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_WIN32_HWND_POINTER"><c>SDL_PROP_WINDOW_WIN32_HWND_POINTER</c></see></term>
	///			<description>The HWND associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_WIN32_HDC_POINTER"><c>SDL_PROP_WINDOW_WIN32_HDC_POINTER</c></see></term>
	///			<description>The HDC associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_WIN32_INSTANCE_POINTER"><c>SDL_PROP_WINDOW_WIN32_INSTANCE_POINTER</c></see></term>
	///			<description>The HINSTANCE associated with the window</description>
	///		</item>
	/// </list>
	/// On Wayland:
	/// <br/>
	/// Note: The <c>xdg_*</c> window objects do not internally persist across window show/hide calls. They will be null if the window is hidden and must be queried each time it is shown.
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_WAYLAND_DISPLAY_POINTER"><c>SDL_PROP_WINDOW_WAYLAND_DISPLAY_POINTER</c></see></term>
	///			<description>The wl_display associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_WAYLAND_SURFACE_POINTER"><c>SDL_PROP_WINDOW_WAYLAND_SURFACE_POINTER</c></see></term>
	///			<description>The wl_surface associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_WAYLAND_VIEWPORT_POINTER"><c>SDL_PROP_WINDOW_WAYLAND_VIEWPORT_POINTER</c></see></term>
	///			<description>The wp_viewport associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_WAYLAND_EGL_WINDOW_POINTER"><c>SDL_PROP_WINDOW_WAYLAND_EGL_WINDOW_POINTER</c></see></term>
	///			<description>The wl_egl_window associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_WAYLAND_XDG_SURFACE_POINTER"><c>SDL_PROP_WINDOW_WAYLAND_XDG_SURFACE_POINTER</c></see></term>
	///			<description>The xdg_surface associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_WAYLAND_XDG_TOPLEVEL_POINTER"><c>SDL_PROP_WINDOW_WAYLAND_XDG_TOPLEVEL_POINTER</c></see></term>
	///			<description>The xdg_toplevel role associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_WAYLAND_XDG_TOPLEVEL_EXPORT_HANDLE_STRING"><c>SDL_PROP_WINDOW_WAYLAND_XDG_TOPLEVEL_EXPORT_HANDLE_STRING</c></see></term>
	///			<description>The export handle associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_WAYLAND_XDG_POPUP_POINTER"><c>SDL_PROP_WINDOW_WAYLAND_XDG_POPUP_POINTER</c></see></term>
	///			<description>The xdg_popup role associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_WAYLAND_XDG_POSITIONER_POINTER"><c>SDL_PROP_WINDOW_WAYLAND_XDG_POSITIONER_POINTER</c></see></term>
	///			<description>The xdg_positioner associated with the window, in popup mode</description>
	///		</item>
	/// </list>
	/// On X11:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_X11_DISPLAY_POINTER"><c>SDL_PROP_WINDOW_X11_DISPLAY_POINTER</c></see></term>
	///			<description>The X11 Display associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_X11_SCREEN_NUMBER"><c>SDL_PROP_WINDOW_X11_SCREEN_NUMBER</c></see></term>
	///			<description>The screen number associated with the window</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_X11_WINDOW_NUMBER"><c>SDL_PROP_WINDOW_X11_WINDOW_NUMBER</c></see></term>
	///			<description>The X11 Window associated with the window</description>
	///		</item>
	/// </list>
	/// On Emscripten:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_EMSCRIPTEN_CANVAS_ID_STRING"><c>SDL_PROP_WINDOW_EMSCRIPTEN_CANVAS_ID_STRING</c></see></term>
	///			<description>The id the canvas element will have</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_WINDOW_EMSCRIPTEN_KEYBOARD_ELEMENT_STRING"><c>SDL_PROP_WINDOW_EMSCRIPTEN_KEYBOARD_ELEMENT_STRING</c></see></term>
	///			<description>The keyboard element that associates keyboard events to this window</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowProperties">SDL_GetWindowProperties</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial uint SDL_GetWindowProperties(SDL_Window* window);

	/// <summary>
	/// Queries whether relative mouse mode is enabled for a window
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns true if relative mode is enabled for a window or false otherwise</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowRelativeMouseMode">SDL_GetWindowRelativeMouseMode</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetWindowRelativeMouseMode(SDL_Window* window);

	/// <summary>
	/// Gets a list of valid windows
	/// </summary>
	/// <param name="count">A pointer filled in with the number of windows returned, may be NULL</param>
	/// <returns>
	///	Returns a NULL terminated array of SDL_Window pointers or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information.
	///	This is a single allocation that should be freed with <see href="https://wiki.libsdl.org/SDL3/SDL_free">SDL_free</see>() when it is no longer needed.
	///	</returns>
	///	<remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindows">SDL_GetWindows</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Window** SDL_GetWindows(int* count);

	/// <summary>
	/// Gets the safe area for this window
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <param name="rect">A pointer filled in with the client area that is safe for interactive content</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// Some devices have portions of the screen which are partially obscured or not interactive, possibly due to on-screen controls, curved edges, camera notches, TV overscan, etc.
	/// This function provides the area of the window which is safe to have interactable content.
	/// You should continue rendering into the rest of the window, but it should not contain visually important or interactable content.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowSafeArea">SDL_GetWindowSafeArea</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetWindowSafeArea(SDL_Window* window, Rect<int>* rect);

	/// <summary>
	/// Gets the size of a window's client area
	/// </summary>
	/// <param name="window">The window to query the width and height from</param>
	/// <param name="w">A pointer filled in with the width of the window, may be NULL</param>
	/// <param name="h">A pointer filled in with the height of the window, may be NULL</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The window pixel size may differ from its window coordinate size if the window is on a high pixel density display.
	/// Use <see href="https://wiki.libsdl.org/SDL3/SDL_GetWindowSizeInPixels">SDL_GetWindowSizeInPixels</see>() or <see href="https://wiki.libsdl.org/SDL3/SDL_GetRenderOutputSize">SDL_GetRenderOutputSize</see>() to get the real client area size in pixels.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowSize">SDL_GetWindowSize</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetWindowSize(SDL_Window* window, int* w, int* h);

	/// <summary>
	/// Get the size of a window's client area, in pixels
	/// </summary>
	/// <param name="window">The window from which the drawable size should be queried</param>
	/// <param name="w">A pointer to variable for storing the width in pixels, may be NULL</param>
	/// <param name="h">A pointer to variable for storing the height in pixels, may be NULL</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowSizeInPixels">SDL_GetWindowSizeInPixels</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetWindowSizeInPixels(SDL_Window* window, int* w, int* h);

	/// <summary>
	/// Gets the title of a window
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns the title of the window in UTF-8 format or "" if there is no title</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetWindowTitle">SDL_GetWindowTitle</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial byte* SDL_GetWindowTitle(SDL_Window* window);

	/// <summary>
	/// Hides a window
	/// </summary>
	/// <param name="window">The window to hide</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HideWindow">SDL_HideWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_HideWindow(SDL_Window* window);

	/// <summary>
	/// Requests that the window be made as large as possible
	/// </summary>
	/// <param name="window">The window to maximize</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// Non-resizable windows can't be maximized. The window must have the <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_RESIZABLE">SDL_WINDOW_RESIZABLE</see> flag set, or this will have no effect.
	/// </para>
	/// <para>
	/// On some windowing systems this request is asynchronous and the new window state may not have have been applied immediately upon the return of this function.
	/// If an immediate change is required, call <see href="https://wiki.libsdl.org/SDL3/SDL_SyncWindow">SDL_SyncWindow</see>() to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window state changes, an <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_MAXIMIZED">SDL_EVENT_WINDOW_MAXIMIZED</see> event will be emitted.
	/// Note that, as this is just a request, the windowing system can deny the state change.
	/// </para>
	/// <para>
	/// When maximizing a window, whether the constraints set via <see href="https://wiki.libsdl.org/SDL3/SDL_SetWindowMaximumSize">SDL_SetWindowMaximumSize</see>() are honored depends on the policy of the window manager.
	/// Win32 and macOS enforce the constraints when maximizing, while X11 and Wayland window managers may vary.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_MaximizeWindow">SDL_MaximizeWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_MaximizeWindow(SDL_Window* window);

	/// <summary>
	/// Requests that the window be minimized to an iconic representation
	/// </summary>
	/// <param name="window">The window to minimize</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// If the window is in a fullscreen state, this request has no direct effect. It may alter the state the window is returned to when leaving fullscreen.
	/// </para>
	/// <para>
	/// On some windowing systems this request is asynchronous and the new window state may not have been applied immediately upon the return of this function.
	/// If an immediate change is required, call <see href="https://wiki.libsdl.org/SDL3/SDL_SyncWindow">SDL_SyncWindow</see>() to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window state changes, an <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_MINIMIZED">SDL_EVENT_WINDOW_MINIMIZED</see> event will be emitted.
	/// Note that, as this is just a request, the windowing system can deny the state change.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_MinimizeWindow">SDL_MinimizeWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_MinimizeWindow(SDL_Window* window);

	/// <summary>
	/// Requests that a window be raised above other windows and gain the input focus
	/// </summary>
	/// <param name="window">The window to raise</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The result of this request is subject to desktop window manager policy, particularly if raising the requested window would result in stealing focus from another application.
	/// If the window is successfully raised and gains input focus, an <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_FOCUS_GAINED">SDL_EVENT_WINDOW_FOCUS_GAINED</see> event will be emitted,
	/// and the window will have the <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_INPUT_FOCUS">SDL_WINDOW_INPUT_FOCUS</see> flag set.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_RaiseWindow">SDL_RaiseWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_RaiseWindow(SDL_Window* window);

	/// <summary>
	/// Requests that the size and position of a minimized or maximized window be restored
	/// </summary>
	/// <param name="window">The window to restore</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// If the window is in a fullscreen state, this request has no direct effect. It may alter the state the window is returned to when leaving fullscreen.
	/// </para>
	/// <para>
	/// On some windowing systems this request is asynchronous and the new window state may not have been applied immediately upon the return of this function.
	/// If an immediate change is required, call <see href="https://wiki.libsdl.org/SDL3/SDL_SyncWindow">SDL_SyncWindow</see>() to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window state changes, an <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_RESTORED">SDL_EVENT_WINDOW_RESTORED</see> event will be emitted.
	/// Note that, as this is just a request, the windowing system can deny the state change.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_RestoreWindow">SDL_RestoreWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_RestoreWindow(SDL_Window* window);

	/// <summary>
	/// Sets the window to always be above the others
	/// </summary>
	/// <param name="window">The window of which to change the always on top state</param>
	/// <param name="on_top">true to set the window always on top, false to disable</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This will add or remove the window's <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_ALWAYS_ON_TOP"><c>SDL_WINDOW_ALWAYS_ON_TOP</c></see> flag.
	/// This will bring the window to the front and keep the window above the rest.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowAlwaysOnTop">SDL_SetWindowAlwaysOnTop</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowAlwaysOnTop(SDL_Window* window, CBool on_top);

	/// <summary>
	/// Requests that the aspect ratio of a window's client area be set
	/// </summary>
	/// <param name="window">The window to change</param>
	/// <param name="min_aspect">The minimum aspect ratio of the window, or 0.0f for no limit</param>
	/// <param name="max_aspect">The maximum aspect ratio of the window, or 0.0f for no limit</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The aspect ratio is the ratio of width divided by height, e.g. 2560x1600 would be 1.6. Larger aspect ratios are wider and smaller aspect ratios are narrower.
	/// </para>
	/// <para>
	/// If, at the time of this request, the window in a fixed-size state, such as maximized or fullscreen, the request will be deferred until the window exits this state and becomes resizable again.
	/// </para>
	/// <para>
	/// On some windowing systems this request is asynchronous and the new window state may not have been applied immediately upon the return of this function.
	/// If an immediate change is required, call <see href="https://wiki.libsdl.org/SDL3/SDL_SyncWindow">SDL_SyncWindow</see>() to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window size changes, an <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_RESIZED">SDL_EVENT_WINDOW_RESIZED</see> event will be emitted with the new window dimensions.
	/// Note that the new dimensions may not match the exact aspect ratio requested, as some windowing systems can restrict the window size in certain scenarios (e.g. constraining the size of the content area to remain within the usable desktop bounds).
	/// Additionally, as this is just a request, it can be denied by the windowing system.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowAspectRatio">SDL_SetWindowAspectRatio</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowAspectRatio(SDL_Window* window, float min_aspect, float max_aspect);

	/// <summary>
	/// Sets the border state of a window
	/// </summary>
	/// <param name="window">The window of which to change the border state</param>
	/// <param name="bordered">false to remove border, true to add border</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This will add or remove the window's <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_BORDERLESS"><c>SDL_WINDOW_BORDERLESS</c></see> flag and add or remove the border from the actual window.
	/// This is a no-op if the window's border already matches the requested state.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowBordered">SDL_SetWindowBordered</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowBordered(SDL_Window* window, CBool bordered);

	/// <summary>
	/// Set whether the window may have input focus
	/// </summary>
	/// <param name="window">The window to set focusable state</param>
	/// <param name="focusable">true to allow input focus, false to not allow input focus</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowFocusable">SDL_SetWindowFocusable</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowFocusable(SDL_Window* window, CBool focusable);

	/// <summary>
	/// Requests that the window's fullscreen state be changed
	/// </summary>
	/// <param name="window">The window to change</param>
	/// <param name="fullscreen">true for fullscreen mode, false for windowed mode</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// By default a window in fullscreen state uses borderless fullscreen desktop mode, but a specific exclusive display mode can be set using <see href="https://wiki.libsdl.org/SDL3/SDL_SetWindowFullscreenMode">SDL_SetWindowFullscreenMode</see>().
	/// </para>
	/// <para>
	/// On some windowing systems this request is asynchronous and the new fullscreen state may not have have been applied immediately upon the return of this function.
	/// If an immediate change is required, call <see href="https://wiki.libsdl.org/SDL3/SDL_SyncWindow">SDL_SyncWindow</see>() to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window state changes, an <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_ENTER_FULLSCREEN">SDL_EVENT_WINDOW_ENTER_FULLSCREEN</see> or <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_LEAVE_FULLSCREEN">SDL_EVENT_WINDOW_LEAVE_FULLSCREEN</see> event will be emitted.
	/// Note that, as this is just a request, it can be denied by the windowing system.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowFullscreen">SDL_SetWindowFullscreen</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowFullscreen(SDL_Window* window, CBool fullscreen);

	/// <summary>
	/// Sets the display mode to use when a window is visible and fullscreen
	/// </summary>
	/// <param name="window">The window to affect</param>
	/// <param name="mode">A pointer to the display mode to use, which can be NULL for borderless fullscreen desktop mode, or one of the fullscreen modes returned by <see href="https://wiki.libsdl.org/SDL3/SDL_GetFullscreenDisplayModes">SDL_GetFullscreenDisplayModes</see>() to set an exclusive fullscreen mode</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This only affects the display mode used when the window is fullscreen. To change the window size when the window is not fullscreen, use <see href="https://wiki.libsdl.org/SDL3/SDL_SetWindowSize">SDL_SetWindowSize</see>().
	/// </para>
	/// <para>
	/// If the window is currently in the fullscreen state, this request is asynchronous on some windowing systems and the new mode dimensions may not be applied immediately upon the return of this function.
	/// If an immediate change is required, call <see href="https://wiki.libsdl.org/SDL3/SDL_SyncWindow">SDL_SyncWindow</see>() to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the new mode takes effect, an <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_RESIZED">SDL_EVENT_WINDOW_RESIZED</see> and/or an <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED">SDL_EVENT_WINDOW_PIXEL_SIZE_CHANGED</see> event will be emitted with the new mode dimensions.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowFullscreenMode">SDL_SetWindowFullscreenMode</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowFullscreenMode(SDL_Window* window, DisplayMode.SDL_DisplayMode* mode);

	/// <summary>
	/// Provides a callback that decides if a window region has special properties
	/// </summary>
	/// <param name="window">The window to set hit-testing on</param>
	/// <param name="callback">The function to call when doing a hit-test</param>
	/// <param name="callback_data">An app-defined void pointer passed to <em><paramref name="callback"/></em></param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// Normally windows are dragged and resized by decorations provided by the system window manager (a title bar, borders, etc), but for some apps, it makes sense to drag them from somewhere else inside the window itself;
	/// for example, one might have a borderless window that wants to be draggable from any part, or simulate its own title bar, etc.
	/// </para>
	/// <para>
	/// This function lets the app provide a callback that designates pieces of a given window as special. This callback is run during event processing if we need to tell the OS to treat a region of the window specially;
	/// the use of this callback is known as "hit testing."
	/// </para>
	/// <para>
	/// Mouse input may not be delivered to your application if it is within a special area; the OS will often apply that input to moving the window or resizing the window and not deliver it to the application.
	/// </para>
	/// <para>
	/// Specifying NULL for a callback disables hit-testing. Hit-testing is disabled by default.
	/// </para>
	/// <para>
	/// Platforms that don't support this functionality will return false unconditionally, even if you're attempting to disable hit-testing.
	/// </para>
	/// <para>
	/// Your callback may fire at any time, and its firing does not indicate any specific behavior (for example, on Windows, this certainly might fire when the OS is deciding whether to drag your window,
	/// but it fires for lots of other reasons, too, some unrelated to anything you probably care about and when the mouse isn't actually at the location it is testing).
	/// Since this can fire at any time, you should try to keep your callback efficient, devoid of allocations, etc.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowHitTest">SDL_SetWindowHitTest</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowHitTest(SDL_Window* window, SDL_HitTest callback, void* callback_data);

	/// <summary>
	/// Sets the icon for a window
	/// </summary>
	/// <param name="window">The window to change</param>
	/// <param name="icon">An <see href="https://wiki.libsdl.org/SDL3/SDL_Surface">SDL_Surface</see> structure containing the icon for the window</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// If this function is passed a surface with alternate representations added using <see href="https://wiki.libsdl.org/SDL3/SDL_AddSurfaceAlternateImage">SDL_AddSurfaceAlternateImage</see>(), the surface will be interpreted as the content to be used for 100% display scale, and the alternate representations will be used for high DPI situations.
	/// For example, if the original surface is 32x32, then on a 2x macOS display or 200% display scale on Windows, a 64x64 version of the image will be used, if available. If a matching version of the image isn't available, the closest larger size image will be downscaled to the appropriate size and be used instead, if available.
	/// Otherwise, the closest smaller image will be upscaled and be used instead.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowIcon">SDL_SetWindowIcon</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowIcon(SDL_Window* window, Surface.SDL_Surface* icon);

	/// <summary>
	/// Sets a window's keyboard grab mode
	/// </summary>
	/// <param name="window">The window for which the keyboard grab mode should be set</param>
	/// <param name="grabbed">This is true to grab keyboard, and false to release</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// Keyboard grab enables capture of system keyboard shortcuts like Alt+Tab or the Meta/Super key. Note that not all system keyboard shortcuts can be captured by applications (one example is Ctrl+Alt+Del on Windows).
	/// </para>
	/// <para>
	/// This is primarily intended for specialized applications such as VNC clients or VM frontends. Normal games should not use keyboard grab.
	/// </para>
	/// <para>
	/// When keyboard grab is enabled, SDL will continue to handle Alt+Tab when the window is full-screen to ensure the user is not trapped in your application.
	/// If you have a custom keyboard shortcut to exit fullscreen mode, you may suppress this behavior with <see href="https://wiki.libsdl.org/SDL3/SDL_HINT_ALLOW_ALT_TAB_WHILE_GRABBED"><c>SDL_HINT_ALLOW_ALT_TAB_WHILE_GRABBED</c></see>.
	/// </para>
	/// <para>
	/// If the caller enables a grab while another window is currently grabbed, the other window loses its grab in favor of the caller's window.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowKeyboardGrab">SDL_SetWindowKeyboardGrab</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowKeyboardGrab(SDL_Window* window, CBool grabbed);

	/// <summary>
	/// Sets the maximum size of a window's client area
	/// </summary>
	/// <param name="window">The window to change</param>
	/// <param name="max_w">The maximum width of the window, or 0 for no limit</param>
	/// <param name="max_h">The maximum height of the window, or 0 for no limit.</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowMaximumSize">SDL_SetWindowMaximumSize</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowMaximumSize(SDL_Window* window, int max_w, int max_h);

	/// <summary>
	/// Sets the minimum size of a window's client area
	/// </summary>
	/// <param name="window">The window to change</param>
	/// <param name="min_w">The minimum width of the window, or 0 for no limit</param>
	/// <param name="min_h">The minimum height of the window, or 0 for no limit.</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowMinimumSize">SDL_SetWindowMinimumSize</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowMinimumSize(SDL_Window* window, int min_w, int min_h);

	/// <summary>
	/// Toggles the state of the window as modal
	/// </summary>
	/// <param name="window">The window on which to set the modal state</param>
	/// <param name="modal">true to toggle modal status on, false to toggle it off</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// To enable modal status on a window, the window must currently be the child window of a parent, or toggling modal status on will fail.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowModal">SDL_SetWindowModal</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowModal(SDL_Window* window, CBool modal);

	/// <summary>
	/// Sets a window's mouse grab mode
	/// </summary>
	/// <param name="window">The window for which the mouse grab mode should be set</param>
	/// <param name="grabbed">This is true to grab mouse, and false to release</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// Mouse grab confines the mouse cursor to the window.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowMouseGrab">SDL_SetWindowMouseGrab</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowMouseGrab(SDL_Window* window, CBool grabbed);

	/// <summary>
	/// Confines the cursor to the specified area of a window
	/// </summary>
	/// <param name="window">The window that will be associated with the barrier</param>
	/// <param name="rect">A rectangle area in window-relative coordinates. If NULL the barrier for the specified window will be destroyed.</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// Note that this does NOT grab the cursor, it only defines the area a cursor is restricted to when the window has mouse focus.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowMouseRect">SDL_SetWindowMouseRect</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowMouseRect(SDL_Window* window, Rect<int>* rect);

	/// <summary>
	/// Sets the opacity for a window
	/// </summary>
	/// <param name="window">The window which will be made transparent or opaque</param>
	/// <param name="opacity">The opacity value (0.0f - transparent, 1.0f - opaque)</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The parameter <c><paramref name="opacity"/></c> will be clamped internally between 0.0f (transparent) and 1.0f (opaque).
	/// </para>
	/// <para>
	/// This function also returns false if setting the opacity isn't supported.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowOpacity">SDL_SetWindowOpacity</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowOpacity(SDL_Window* window, float opacity);

	/// <summary>
	/// Sets the window as a child of a parent window
	/// </summary>
	/// <param name="window">The window that should become the child of a parent</param>
	/// <param name="parent">The new parent window for the child window</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// If the window is already the child of an existing window, it will be reparented to the new owner.
	/// Setting the parent window to NULL unparents the window and removes child window status.
	/// </para>
	/// <para>
	/// If a parent window is hidden or destroyed, the operation will be recursively applied to child windows.
	/// Child windows hidden with the parent that did not have their hidden status explicitly set will be restored when the parent is shown.
	/// </para>
	/// <para>
	/// Attempting to set the parent of a window that is currently in the modal state will fail.
	/// Use <see href="https://wiki.libsdl.org/SDL3/SDL_SetWindowModal">SDL_SetWindowModal</see>() to cancel the modal status before attempting to change the parent.
	/// </para>
	/// <para>
	/// Popup windows cannot change parents and attempts to do so will fail.
	/// </para>
	/// <para>
	/// Setting a parent window that is currently the sibling or descendent of the child window results in undefined behavior.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowParent">SDL_SetWindowParent</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowParent(SDL_Window* window, SDL_Window* parent);

	/// <summary>
	/// Requests that the window's position be set
	/// </summary>
	/// <param name="window">The window to reposition</param>
	/// <param name="x">The x coordinate of the window, or <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOWPOS_CENTERED"><c>SDL_WINDOWPOS_CENTERED</c></see> or <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOWPOS_UNDEFINED"><c>SDL_WINDOWPOS_UNDEFINED</c></see></param>
	/// <param name="y">The y coordinate of the window, or <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOWPOS_CENTERED"><c>SDL_WINDOWPOS_CENTERED</c></see> or <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOWPOS_UNDEFINED"><c>SDL_WINDOWPOS_UNDEFINED</c></see></param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// If the window is in an exclusive fullscreen or maximized state, this request has no effect.
	/// </para>
	/// <para>
	/// This can be used to reposition fullscreen-desktop windows onto a different display,
	/// however, as exclusive fullscreen windows are locked to a specific display, they can only be repositioned programmatically via <see href="https://wiki.libsdl.org/SDL3/SDL_SetWindowFullscreenMode">SDL_SetWindowFullscreenMode</see>().
	/// </para>
	/// <para>
	/// On some windowing systems this request is asynchronous and the new coordinates may not have have been applied immediately upon the return of this function.
	/// If an immediate change is required, call <see href="https://wiki.libsdl.org/SDL3/SDL_SyncWindow">SDL_SyncWindow</see>() to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window position changes, an <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_MOVED">SDL_EVENT_WINDOW_MOVED</see> event will be emitted with the window's new coordinates.
	/// Note that the new coordinates may not match the exact coordinates requested, as some windowing systems can restrict the position of the window in certain scenarios (e.g. constraining the position so the window is always within desktop bounds).
	/// Additionally, as this is just a request, it can be denied by the windowing system.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowPosition">SDL_SetWindowPosition</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowPosition(SDL_Window* window, int x, int y);

#if SDL3_4_0_OR_GREATER

	/// <summary>
	/// Sets the state of the progress bar for the given window’s taskbar icon.
	/// </summary>
	/// <param name="window">The window whose progress state is to be modified</param>
	/// <param name="state">The progress state. <see href="https://wiki.libsdl.org/SDL3/SDL_PROGRESS_STATE_NONE">SDL_PROGRESS_STATE_NONE</see> stops displaying the progress bar.</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowProgressState">SDL_SetWindowProgressState</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowProgressState(SDL_Window* window, ProgressState state);

	/// <summary>
	/// Sets the value of the progress bar for the given window’s taskbar icon.
	/// </summary>
	/// <param name="window">The window whose progress value is to be modified</param>
	/// <param name="value">The progress value in the range of [0.0f - 1.0f]. If the value is outside the valid range, it gets clamped.</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowProgressValue">SDL_SetWindowProgressValue</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowProgressValue(SDL_Window* window, float value);

#endif

	/// <summary>
	/// Sets relative mouse mode for a window
	/// </summary>
	/// <param name="window">The window to change</param>
	/// <param name="enabled">True to enable relative mode, false to disable</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// While the window has focus and relative mouse mode is enabled, the cursor is hidden, the mouse position is constrained to the window, and SDL will report continuous relative mouse motion even if the mouse is at the edge of the window.
	/// </para>
	/// <para>
	/// If you'd like to keep the mouse position fixed while in relative mode you can use <see href="https://wiki.libsdl.org/SDL3/SDL_SetWindowMouseRect">SDL_SetWindowMouseRect()</see>.
	/// If you'd like the cursor to be at a specific location when relative mode ends, you should use <see href="https://wiki.libsdl.org/SDL3/SDL_WarpMouseInWindow">SDL_WarpMouseInWindow</see>() before disabling relative mode.
	/// </para>
	/// <para>
	/// This function will flush any pending mouse motion for this window.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowRelativeMouseMode">SDL_SetWindowRelativeMouseMode</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowRelativeMouseMode(SDL_Window* window, bool enabled);

	/// <summary>
	/// Sets the user-resizable state of a window.
	/// </summary>
	/// <param name="window">The window of which to change the resizable state</param>
	/// <param name="resizable">true to allow resizing, false to disallow</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This will add or remove the window's <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_RESIZABLE"><c>SDL_WINDOW_RESIZABLE</c></see> flag and allow/disallow user resizing of the window.
	/// This is a no-op if the window's resizable state already matches the requested state.
	/// </para>
	/// <para>
	/// You can't change the resizable state of a fullscreen window.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowResizable">SDL_SetWindowResizable</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowResizable(SDL_Window* window, CBool resizable);

	/// <summary>
	/// Sets the shape of a transparent window
	/// </summary>
	/// <param name="window">The window</param>
	/// <param name="shape">The surface representing the shape of the window, or NULL to remove any current shape</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This sets the alpha channel of a transparent window and any fully transparent areas are also transparent to mouse clicks.
	/// If you are using something besides the SDL render API, then you are responsible for drawing the alpha channel of the window to match the shape alpha channel to get consistent cross-platform results.
	/// </para>
	/// <para>
	/// The shape is copied inside this function, so you can free it afterwards.
	/// If your shape surface changes, you should call <see href="https://wiki.libsdl.org/SDL3/SDL_SetWindowShape">SDL_SetWindowShape</see>() again to update the window.
	/// This is an expensive operation, so should be done sparingly.
	/// </para>
	/// <para>
	/// The window must have been created with the <see href="https://wiki.libsdl.org/SDL3/SDL_WINDOW_TRANSPARENT">SDL_WINDOW_TRANSPARENT</see> flag.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowShape">SDL_SetWindowShape</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowShape(SDL_Window* window, Surface.SDL_Surface* shape);

	/// <summary>
	/// Requests that the size of a window's client area be set
	/// </summary>
	/// <param name="window">The window to change</param>
	/// <param name="w">The width of the window, must be > 0</param>
	/// <param name="h">Rhe height of the window, must be > 0</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// If the window is in a fullscreen or maximized state, this request has no effect.
	/// </para>
	/// <para>
	/// To change the exclusive fullscreen mode of a window, use <see href="https://wiki.libsdl.org/SDL3/SDL_SetWindowFullscreenMode">SDL_SetWindowFullscreenMode</see>().
	/// </para>
	/// <para>
	/// On some windowing systems, this request is asynchronous and the new window size may not have have been applied immediately upon the return of this function.
	/// If an immediate change is required, call <see href="https://wiki.libsdl.org/SDL3/SDL_SyncWindow">SDL_SyncWindow</see>() to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window size changes, an <see href="https://wiki.libsdl.org/SDL3/SDL_EVENT_WINDOW_RESIZED">SDL_EVENT_WINDOW_RESIZED</see> event will be emitted with the new window dimensions.
	/// Note that the new dimensions may not match the exact size requested, as some windowing systems can restrict the window size in certain scenarios (e.g. constraining the size of the content area to remain within the usable desktop bounds).
	/// Additionally, as this is just a request, it can be denied by the windowing system.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowSize">SDL_SetWindowSize</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowSize(SDL_Window* window, int w, int h);

	/// <summary>
	/// Sets the title of a window
	/// </summary>
	/// <param name="window">The window to change</param>
	/// <param name="title">The desired window title in UTF-8 format</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This string is expected to be in UTF-8 encoding.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetWindowTitle">SDL_SetWindowTitle</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetWindowTitle(SDL_Window* window, byte* title);

	/// <summary>
	/// Shows a window
	/// </summary>
	/// <param name="window">The window to show.</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_ShowWindow">SDL_ShowWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_ShowWindow(SDL_Window* window);

	/// <summary>
	/// Displays the system-level window menu
	/// </summary>
	/// <param name="window">The window for which the menu will be displayed</param>
	/// <param name="x">The x coordinate of the menu, relative to the origin (top-left) of the client area</param>
	/// <param name="y">The y coordinate of the menu, relative to the origin (top-left) of the client area.</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This default window menu is provided by the system and on some platforms provides functionality for setting or changing privileged state on the window, such as moving it between workspaces or displays, or toggling the always-on-top property.
	/// </para>
	/// <para>
	/// On platforms or desktops where this is unsupported, this function does nothing.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_ShowWindowSystemMenu">SDL_ShowWindowSystemMenu</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_ShowWindowSystemMenu(SDL_Window* window, int x, int y);

	/// <summary>
	/// Blocks until any pending window state is finalized
	/// </summary>
	/// <param name="window">The window for which to wait for the pending state to be applied</param>
	/// <returns>Returns true on success or false if the operation timed out before the window was in the requested state</returns>
	/// <remarks>
	/// <para>
	/// On asynchronous windowing systems, this acts as a synchronization barrier for pending window state.
	/// It will attempt to wait until any pending window state has been applied and is guaranteed to return within finite time.
	/// Note that for how long it can potentially block depends on the underlying window system, as window state changes may involve somewhat lengthy animations that must complete before the window is in its final requested state.
	/// </para>
	/// <para>
	/// On windowing systems where changes are immediate, this does nothing.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SyncWindow">SDL_SyncWindow</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SyncWindow(SDL_Window* window);

	/// <summary>
	/// Returns whether the window has a surface associated with it
	/// </summary>
	/// <param name="window">The window to query</param>
	/// <returns>Returns true if there is a surface associated with the window, or false otherwise</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_WindowHasSurface">SDL_WindowHasSurface</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_WindowHasSurface(SDL_Window* window);
}
