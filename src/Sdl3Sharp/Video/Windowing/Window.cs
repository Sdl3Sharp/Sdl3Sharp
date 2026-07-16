using Sdl3Sharp.Events;
using Sdl3Sharp.Internal;
using Sdl3Sharp.Utilities;
using Sdl3Sharp.Video.Coloring;
using Sdl3Sharp.Video.Drawing;
using Sdl3Sharp.Video.Rendering;
using Sdl3Sharp.Video.Rendering.Drivers;
using Sdl3Sharp.Video.Windowing.Drivers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents a window
/// </summary>
/// <remarks>
/// <para>
/// A window is used to display content on the screen and receive input from the user.
/// </para>
/// <para>
/// To display content in a window, you can either use a <see cref="Rendering.Renderer"/>,
/// constructed from any of the <c>TryCreateRenderer*</c> methods (e.g. <see cref="TryCreateRenderer(out Renderer?, ReadOnlySpan{string})"/>
/// or from the <see cref="TryCreateWithRenderer(string, int, int, out Window?, out Renderer?, WindowFlags)"/> methods,
/// or use the <see cref="Surface"/> property to get a <see cref="WindowSurface"/> you can draw on.
/// Using either one mutually excludes the other.
/// </para>
/// <para>
/// If you use a <see cref="Renderer"/> in conjunction with a <see cref="Window"/>, please make sure that you don't use it after the window has been <see cref="Dispose()">disposed</see>.
/// The recommended approach is to <see cref="Renderer.Dispose()">dispose</see> the renderer before the window.
/// </para>
/// <para>
/// Windows can be either top-level or be children of other windows. To make a window a child of another window, you just need to set its <see cref="Parent"/> property.
/// </para>
/// <para>
/// Special rules apply for child windows: they will be hidden with their parent and will be destroyed when their parent gets destroyed.
/// </para>
/// <para>
/// You can create new windows using any of the <c>TryCreate*</c> methods (e.g. <see cref="TryCreate(string, int, int, out Window?, WindowFlags)"/>) 
/// or create new popup windows using the <see cref="TryCreatePopup(int, int, int, int, out Window?, WindowFlags)"/> method.
/// The latter will create the new window as a child window of the window the method was called on.
/// </para>
/// <para>
/// For the most part <see cref="Window"/>s are not thread-safe, and most of their properties and methods should only be accessed from the main thread!
/// </para>
/// <para>
/// <see cref="Window"/>s are not driver-agnostic! Most of the time instance of this abstract class are of the concrete <see cref="Window{TDriver}"/> type with a specific <see cref="IWindowingDriver">windowing driver</see> as the type argument.
/// However, the <see cref="Window"/> type exists as a common abstraction.
/// </para>
/// <para>
/// To specify a concrete window type, use <see cref="Window{TDriver}"/> with a windowing driver that implements the <see cref="IWindowingDriver"/> interface (e.g. <see cref="Window{TDriver}">Window&lt;<see cref="Windows">Windows</see>&gt;</see>).
/// </para>
/// </remarks>
public abstract partial class Window : IDisposable
{
	private static readonly ConcurrentDictionary<IntPtr, WeakReference<Window>> mKnownInstances = [];

	private unsafe SDL_Window* mWindow;
	private GCHandle mSelfHandle = default;

	private protected unsafe Window(SDL_Window* window, bool register)
	{
		mWindow = window;
		mSelfHandle = GCHandle.Alloc(this, GCHandleType.Weak);

		// Register a 'WindowDestroy' event watch, so we get notified when a window gets destroyed,
		// even if it's done through other means (e.g. a child window getting destroyed alongside its parent).
		// This way we can automatically dispose the corresponding Window instance and remove it,
		// when the native window gets destroyed.
		Sdl.SDL_AddEventWatch(&EventWatchResizeOrDestroy, unchecked((void*)GCHandle.ToIntPtr(mSelfHandle)));

		if (register)
		{
			if (window is not null)
			{
				mKnownInstances.AddOrUpdate(unchecked((IntPtr)window), addRef, updateRef, this);
			}

			static WeakReference<Window> addRef(IntPtr window, Window newWindow) => new(newWindow);

			static WeakReference<Window> updateRef(IntPtr window, WeakReference<Window> previousWindowRef, Window newWindow)
			{
				if (previousWindowRef.TryGetTarget(out var previousWindow))
				{
#pragma warning disable IDE0079
#pragma warning disable CA1816
					GC.SuppressFinalize(previousWindow);
#pragma warning restore CA1816
#pragma warning restore IDE0079

					previousWindow.Dispose(disposing: true, forget: false);
				}

				previousWindowRef.SetTarget(newWindow);

				return previousWindowRef;
			}
		}
	}

	/// <inheritdoc/>
	~Window() => Dispose(disposing: false, forget: true);

	/// <summary>
	/// Gets a collection of all currently valid windows
	/// </summary>
	/// <value>
	/// A collection of all currently valid windows
	/// </value>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// <exception cref="SdlException">Couldn't get the list of windows</exception>
	public static Window[] AllWindows
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out int count);

				var windows = SDL_GetWindows(&count);

				if (windows is null)
				{
					failCouldNotGetWindows();
				}

				try
				{
					if (count is not > 0)
					{
						return [];
					}

					var result = GC.AllocateUninitializedArray<Window>(count);

					var windowsPtr = windows;
					foreach (ref var window in result.AsSpan())
					{
						// here we rely on that SDL_GetWindows doesn't include null pointers in the returned array,
						// but I'm pretty sure that's a reasonable assumption to make
						TryGetOrCreate(*windowsPtr++, out window!);
					}

					return result;
				}
				finally
				{
					Utilities.NativeMemory.SDL_free(windows);
				}
			}

			[DoesNotReturn]
			static void failCouldNotGetWindows() => throw new SdlException("Couldn't get the list of windows");
		}
	}

	/// <summary>
	/// Gets or sets the range of aspect ratios for the window's client area
	/// </summary>
	/// <value>
	/// The range of aspect ratios for the window's client area.
	/// The minimum is the lower limit of the aspect ratio, or <c>0</c> for no lower limit set.
	/// The maximum is the upper limit of the aspect ratio, or <c>0</c> for no upper limit set.
	/// </value>
	/// <remarks>
	/// <para>
	/// The aspect ratio is the ratio of width divided by height, e.g. <c>2560</c> by <c>1600</c> would be <c>1.6</c>.
	/// Larger aspect ratios are wider and smaller aspect ratios are narrower.
	/// </para>
	/// <para>
	/// If the window is in a fixed-size state, such as maximized or fullscreen, when setting this property,
	/// the request will be deferred until the window exits that state and becomes resizable again.
	/// </para>
	/// <para>
	/// On some windowing backends, a request to change this setting is asynchronous and the new window aspect ratio may not have have been applied immediately upon setting this property.
	/// If an immediate change is required, you can call <see cref="TrySync"/> to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window size changes, an <see cref="EventType.WindowResized"/> event will be emitted with the new window dimensions.
	/// Note that the new dimensions may not match the exact aspect ratio requested, as some windowing backends can restrict the window size in certain scenarios
	/// (e.g. constraining the size of the content area to remain within the usable desktop bounds).
	/// Additionally, windowing backends can also deny the request to change this setting altogether.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public (float Minimum, float Maximum) AspectRatio
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (float Minimum, float Maximum) aspectRatio);

				SdlErrorHelper.ThrowIfFailed(SDL_GetWindowAspectRatio(mWindow, &aspectRatio.Minimum, &aspectRatio.Maximum));

				return aspectRatio;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowAspectRatio(mWindow, value.Minimum, value.Maximum));
			}
		}
	}

	/// <summary>
	/// Gets the size of the window's borders (decorations) around the client area
	/// </summary>
	/// <value>
	/// The size of the window's borders (decorations) around the client area, in pixels
	/// </value>
	/// <remarks>
	/// <para>
	/// Note that if this property fails to retrieve the borders size, the size values will be initialized to <c>(0, 0, 0, 0)</c> instead.
	/// </para>
	/// <para>
	/// This property can fail to retrieve the borders size on platforms where the window has not yet been decorated by the display server (for example, immediately after creating the <see cref="Window"/>).
	/// It is recommended that you wait at least until the window has been presented and composited, so that the window backend has a chance to decorate the window and provide the border dimensions to SDL.
	/// </para>
	/// <para>
	/// This property can also fail to retrieve the borders size if getting that information is not supported.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public (int Top, int Left, int Bottom, int Right) BordersSize
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (int Top, int Left, int Bottom, int Right) borderSize);

				SDL_GetWindowBordersSize(mWindow, &borderSize.Top, &borderSize.Left, &borderSize.Bottom, &borderSize.Right);

				return borderSize;
			}
		}
	}

	private protected abstract Display GetDisplayImpl();

	/// <summary>
	/// Gets the display associated with the window
	/// </summary>
	/// <value>
	/// The display associated with the window, which is the display containing the center of the window
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of the property sometimes reports the <see cref="Display.PrimaryDisplay">primary display</see> instead,
	/// if SDL couldn't determine the actual display the window is on.
	/// </para>
	/// <para>
	/// Accessing this property can also <see langword="throw"/> an <see cref="SdlException"/> under some rare circumstances, for example if there are no connected displays at all.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the display for the window (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public Display Display => GetDisplayImpl();

	/// <summary>
	/// Gets the content display scale relative to the window's pixel size
	/// </summary>
	/// <value>
	/// The content display scale relative to the window's pixel size
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property is a combination of the window pixel density and the display content scale, and is the expected scale for displaying content in this window.
	/// For example, if a <c>3840</c> by <c>2160</c> window had a display scale of <c>2.0</c>,
	/// the user expects the content to take twice as many pixels and be the same physical size as if it were being displayed in a <c>1920</c> by <c>1080</c> window with a display scale of <c>1.0</c>.
	/// </para>
	/// <para>
	/// Conceptually this value corresponds to the scale display setting, and is updated when that setting is changed, or the window moves to a display with a different scale setting.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public float DisplayScale
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowDisplayScale(mWindow);
			}
		}
	}

	/// <summary>
	/// Gets the flags associated with the window
	/// </summary>
	/// <value>
	/// The flags associated with the window
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public WindowFlags Flags
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowFlags(mWindow);
			}
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="DisplayMode"/> to use when the window is visible at fullscreen
	/// </summary>
	/// <value>
	/// The <see cref="DisplayMode"/> to use when the window is visible at fullscreen, or <c><see langword="null"/></c> for borderless fullscreen desktop mode
	/// </value>
	/// <remarks>
	/// <para>
	/// This property only effects the display mode used when the window is in fullscreen mode.
	/// To change the window's size when it's not in fullscreen mode, use the <see cref="Size"/> property instead.
	/// </para>
	/// <para>
	/// If the window is currently in the fullscreen state, a request to change this setting is asynchronous and the new display mode may not have have been applied immediately upon setting this property.
	/// If an immediate change is required, you can call <see cref="TrySync"/> to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the new display mode takes effect, an <see cref="EventType.WindowResized"/> and/or an <see cref="EventType.WindowPixelSizeChanged"/> event will be emitted with the new display mode's dimensions.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public DisplayMode? FullscreenMode
	{
		get
		{
			unsafe
			{
				DisplayMode.TryGetOrCreateUnmanaged(SDL_GetWindowFullscreenMode(mWindow), out var mode);
				return mode;
			}
		}

		set
		{
			unsafe
			{
				if (value is not null)
				{
					fixed (DisplayMode.SDL_DisplayMode* mode = &value.Mode)
					{
						SdlErrorHelper.ThrowIfFailed(SDL_SetWindowFullscreenMode(mWindow, mode));
					}
				}
				else
				{
					SdlErrorHelper.ThrowIfFailed(SDL_SetWindowFullscreenMode(mWindow, null));
				}
			}
		}
	}

	/// <summary>
	/// Gets the window that currently has the input grab, if any
	/// </summary>
	/// <value>
	/// The window that currently has the input grab, or <c><see langword="null"/></c> if there's no such window
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public static Window? GrabbedWindow
	{
		get
		{
			unsafe
			{
				TryGetOrCreate(SDL_GetGrabbedWindow(), out var window);
				return window;
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the window has grabbed keyboard input
	/// </summary>
	/// <value>
	/// A value indicating whether the window has grabbed keyboard input
	/// </value>
	/// <remarks>
	/// <para>
	/// Keyboard grab enables capture of system keyboard shortcuts like <kbd>Alt</kbd>+<kbd>Tab</kbd> or the <kbd>Meta</kbd>/<kbd>Super</kbd> key.
	/// Note that not all system keyboard shortcuts can be captured by applications (one notable example is <kbd>Ctrl</kbd>+<kbd>Alt</kbd>+<kbd>Del</kbd> on Windows).
	/// </para>
	/// <para>
	/// This property is primarily intended for use by specialized applications such as VNC clients or VM frontends. Normal games should not use keyboard grab.
	/// </para>
	/// <para>
	/// When keyboard grab is enabled, SDL will continue to handle <kbd>Alt</kbd>+<kbd>Tab</kbd> when the window is fullscreen to ensure the user is not trapped in your application.
	/// If you have a custom keyboard shortcut to exit fullscreen mode, you may suppress this behavior with a setting of <see cref="Hint.AllowAltTabWhileGrabbed"/>.
	/// </para>
	/// <para>
	/// If the keyboard grab is enabled on a window while another window currently has the keyboard grab,
	/// the other window loses its grab in favor of the window where the grab was just enabled.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public bool HasKeyboardGrab
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowKeyboardGrab(mWindow);
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowKeyboardGrab(mWindow, value));
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the window has grabbed mouse input
	/// </summary>
	/// <value>
	/// A value indicating whether the window has grabbed mouse input
	/// </value>
	/// <remarks>
	/// <para>
	/// Mouse grab confines the mouse cursor to the area of the window.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public bool HasMouseGrab
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowMouseGrab(mWindow);
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowMouseGrab(mWindow, value));
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the window currently has a <see cref="Video.Surface"/> associated with it
	/// </summary>
	/// <value>
	/// A value indicating whether the window currently has a <see cref="Video.Surface"/> associated with it
	/// </value>
	/// <remarks>
	/// <para>
	/// If the value of this property is <see langword="true"/>, you can access the <see cref="Surface"/> property and get the existing associated surface,
	/// otherwise, if the value is <see langword="false"/>, accessing the <see cref="Surface"/> property will create a new surface and associate it with the window.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public bool HasSurface
	{
		get
		{
			unsafe
			{
				return SDL_WindowHasSurface(mWindow);
			}
		}
	}

	/// <summary>
	/// Gets the additional high dynamic range that can be displayed, in terms of the <see cref="SdrWhiteLevel">SDR white level</see>
	/// </summary>
	/// <value>
	/// The additional high dynamic range that can be displayed, in terms of the <see cref="SdrWhiteLevel">SDR white level</see>
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property will be <c>1.0</c> when HDR is not enabled.
	/// </para>
	/// <para>
	/// The value of this property can change dynamically at runtime when a <see cref="EventType.WindowHdrStateChanged"/> event is sent.
	/// </para>
	/// </remarks>
	public float HdrHeadroom => Properties?.TryGetFloatValue(PropertyNames.HdrHeadroomFloat, out var hdrHeadroom) is true
		? hdrHeadroom
		: default;

	private HitTest? mHitTest = null;

	/// <summary>
	/// Gets or sets the custom hit test deciding whether a window region has special properties
	/// </summary>
	/// <value>
	/// The custom hit test deciding whether a window region has special properties, or <c><see langword="null"/></c> if no custom hit test is set for the window
	/// </value>
	/// <remarks>
	/// <para>
	/// Normally windows are dragged and resized by decorations provided by the system window manager (a title bar, borders, etc), but for some applications, it makes sense to drag them from somewhere else inside the window itself.
	/// For example, you might want to create a borderless window that the user can drag from any part, or you to simulate your own title bar, etc.
	/// </para>
	/// <para>
	/// This property lets the application provide a delegate that's called to determine whether certain regions of the window are special.
	/// This delegate is run during event processing when SDL needs to tell the operating system to treat a region of the window specially. This process is called "hit testing."
	/// </para>
	/// <para>
	/// Mouse input may not be delivered to your application if it is within a special area.
	/// The operating system will often apply that input to moving the window or resizing the window and not deliver it to the application.
	/// </para>
	/// <para>
	/// Specifying <c><see langword="null"/></c> for this property disables hit testing. Hit testing is disabled by default.
	/// </para>
	/// <para>
	/// Some platforms may not support this functionality and will <see langword="throw"/> a <see cref="SdlException"/> when you attempt to set this property, even if you attempt to set it to <c><see langword="null"/></c> to disable hit testing.
	/// </para>
	/// <para>
	/// The provided delegate may be called at any time, and it being called does not indicate any specific behavior.
	/// For example, on Windows, the delegate certainly might called when the operating system is trying to decide whether to drag your window.
	/// But it will be called for lot of other reasons too, some of which might be unrelated to anything you probably care about and <em>even when the mouse isn't actually at the location it is testing</em>.
	/// Since the delegate can be called at any time, you should try to keep it as efficient as possible and try to avoid doing any heavy or potentially time-consuming/blocking work in it.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public HitTest? HitTest
	{
		get
		{
			// SDL doesn't provide a way to get the current hit test callback,
			// but since we need to keep the managed callback alive through a reference anyway,
			// we can just return that.
			// Note that this doesn't necessarily reflect a hit test callback that was set externally,
			// but that's probably fine since we expect the user to use the managed API exclusively anyway.

			return mHitTest;
		}

		set
		{
			unsafe
			{
				if (value is null)
				{
					var oldHitTest = mHitTest;
					mHitTest = null;

					if (!(bool)SDL_SetWindowHitTest(mWindow, null, null))
					{
						mHitTest = oldHitTest;

						SdlErrorHelper.ThrowIfFailed();
					}
				}
				else
				{
					if (!mSelfHandle.IsAllocated)
					{
						failDisposed();
					}

					var oldHitTest = mHitTest;
					mHitTest = value;

					if (!(bool)SDL_SetWindowHitTest(mWindow, &HitTestImpl, unchecked((void*)GCHandle.ToIntPtr(mSelfHandle))))
					{
						mHitTest = oldHitTest;

						SdlErrorHelper.ThrowIfFailed();
					}
				}
			}

			[DoesNotReturn]
			static void failDisposed() => throw new ObjectDisposedException(nameof(Window), "The window has already been disposed");
		}
	}

	/// <summary>
	/// Gets the numeric ID of the window
	/// </summary>
	/// <value>
	/// The numeric ID of the window, or <c>0</c> on failure (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// This the ID that SDL uses to identify windows in <see cref="WindowEvent"/>s and some other events.
	/// It's a necessity in order to map such events to their corresponding <see cref="Window"/> instances using <see cref="TryGetFromId(uint, out Window?)"/>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public uint Id
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowID(mWindow);
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the window should always be above other windows
	/// </summary>
	/// <value>
	/// A value indicating whether the window should always be above other windows
	/// </value>
	/// <remarks>
	/// <para>
	/// Setting this property to <c><see langword="true"/></c> will bring the window to the front and keep it above other windows.
	/// </para>
	/// <para>
	/// The value of this property is reflected as the <see cref="WindowFlags.AlwaysOnTop"/> flag in the <see cref="Flags"/> property of the window.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public bool IsAlwaysOnTop
	{
		get => (Flags & WindowFlags.AlwaysOnTop) is not 0;

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowAlwaysOnTop(mWindow, value));
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the window has a border (decorations) around the client area
	/// </summary>
	/// <value>
	/// A value indicating whether the window has a border (decorations) around the client area
	/// </value>
	/// <remarks>
	/// <para>
	/// Changing this property will add or remove the window's border (decorations) around the client area. It's a no-op if the window's border state already matches the given value.
	/// </para>
	/// <para>
	/// You can't change the border state of a fullscreen window!
	/// </para>
	/// <para>
	/// The inverse value of this property is reflected as the <see cref="WindowFlags.Borderless"/> flag in the <see cref="Flags"/> property of the window.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public bool IsBordered
	{
		get => (Flags & WindowFlags.Borderless) is 0;

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowBordered(mWindow, value));
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the window may have input focus
	/// </summary>
	/// <value>
	/// A value indicating whether the window may have input focus
	/// </value>
	/// <remarks>
	/// <para>
	/// The inverse value of this property is reflected as the <see cref="WindowFlags.NotFocusable"/> flag in the <see cref="Flags"/> property of the window.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public bool IsFocusable
	{
		get => (Flags & WindowFlags.NotFocusable) is 0;

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowFocusable(mWindow, value));
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the window is currently in fullscreen mode
	/// </summary>
	/// <value>
	/// A value indicating whether the window is currently in fullscreen mode
	/// </value>
	/// <remarks>
	/// <para>
	/// By default a window in fullscreen mode uses borderless fullscreen desktop mode, but you can specify an exclusive display mode using the <see cref="FullscreenMode"/> property.
	/// </para>
	/// <para>
	/// On some windowing backends, a request to change this setting is asynchronous and the new fullscreen state may not have have been applied immediately upon setting this property.
	/// If an immediate change is required, you can call <see cref="TrySync"/> to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the fullscreen state changed, an <see cref="EventType.WindowEnterFullscreen"/> or an <see cref="EventType.WindowLeaveFullscreen"/> event will be emitted.
	/// Windowing backends can also deny the request to change this setting altogether.
	/// </para>
	/// <para>
	/// The value of this property is reflected as the <see cref="WindowFlags.Fullscreen"/> flag in the <see cref="Flags"/> property of the window.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public bool IsFullscreen
	{
		get => (Flags & WindowFlags.Fullscreen) is not 0;

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowFullscreen(mWindow, value));
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the window has <see cref="HdrHeadroom">HDR headroom</see> above the <see cref="SdrWhiteLevel">SDR white level</see>
	/// </summary>
	/// <value>
	/// A value indicating whether the window has <see cref="HdrHeadroom">HDR headroom</see> above the <see cref="SdrWhiteLevel">SDR white level</see>
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property can change dynamically at runtime when a <see cref="EventType.WindowHdrStateChanged"/> event is sent.
	/// </para>
	/// </remarks>
	public bool IsHdrEnabled => Properties?.TryGetBooleanValue(PropertyNames.HdrEnabledBoolean, out var hdrEnabled) is true
		&& hdrEnabled;

	/// <summary>
	/// Gets or sets a value indicating whether the window is in relative mouse mode
	/// </summary>
	/// <value>
	/// A value indicating whether the window is in relative mouse mode
	/// </value>
	/// <remarks>
	/// <para>
	/// While the window has focus and relative mouse mode is enabled, the cursor is hidden, the mouse position is constrained to the window,
	/// and SDL will report continuous relative mouse motion even if the mouse is at the edge of the window.
	/// </para>
	/// <para>
	/// If you'd like to keep the mouse position fixed while in relative mode you can use the <see cref="MouseRect"/> property.
	/// If you'd like the cursor to be at a specific location when relative mode ends, you should use <see cref="WarpMouseInWindow"/> before disabling relative mode.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public bool IsRelativeMouseModeEnabled
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowRelativeMouseMode(mWindow);
			}
		}
		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowRelativeMouseMode(mWindow, value));
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the window is modal
	/// </summary>
	/// <value>
	/// A value indicating whether the window is modal
	/// </value>
	/// <remarks>
	/// <para>
	/// <em>To enable the modal state for a window, the window must be a child of another window</em>,
	/// or else this property will <see langword="throw"/> a <see cref="SdlException"/> when you attempt to set it to <c><see langword="true"/></c>.
	/// You can set the parent of a window using the <see cref="Parent"/> property.
	/// </para>
	/// <para>
	/// The value of this property is reflected as the <see cref="WindowFlags.Modal"/> flag in the <see cref="Flags"/> property of the window.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public bool IsModal
	{
		get => (Flags & WindowFlags.Modal) is not 0;

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowModal(mWindow, value));
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the window is resizable by the user
	/// </summary>
	/// <value>
	/// A value indicating whether the window is resizable by the user
	/// </value>
	/// <remarks>
	/// <para>
	/// Changing this property will allow or disallow the user from resizing the window. It's a no-op if the window's resizable state already matches the given value.
	/// </para>
	/// <para>
	/// The value of this property is reflected as the <see cref="WindowFlags.Resizable"/> flag in the <see cref="Flags"/> property of the window.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public bool IsResizable
	{
		get => (Flags & WindowFlags.Resizable) is not 0;

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowResizable(mWindow, value));
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the window is valid
	/// </summary>
	/// <value>
	/// A value indicating whether the window is valid
	/// </value>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> instance can become invalid after it's been <see cref="Dispose()">disposed</see>, or it's associated native window has been destroyed.
	/// </para>
	/// <para>
	/// The latter can be the case, for example, for child windows that become invalid when their parent window is disposed/destroyed.
	/// You need to be careful when handling the lifetime of child windows in relation to their parent windows.
	/// It's the best to <see cref="Dispose()">dispose</see> child windows before their parent windows, or resetting their parent to something else (or <c><see langword="null"/></c>) before their parent windows are disposed/destroyed.
	/// You can check if a window has a parent or change their parent using the <see cref="Parent"/> property.
	/// </para>
	/// </remarks>
	public bool IsValid { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get { unsafe { return mWindow is not null; } } }

	/// <summary>
	/// Gets or sets the maximum size of the window's client area
	/// </summary>
	/// <value>
	/// A value indicating the maximum size of the window's client area, or <c>0</c> individually for the width or height component to indicate no maximum limit set for that component
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public (int Width, int Height) MaximumSize
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (int Width, int Height) maxSize);

				SdlErrorHelper.ThrowIfFailed(SDL_GetWindowMaximumSize(mWindow, &maxSize.Width, &maxSize.Height));

				return maxSize;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowMaximumSize(mWindow, value.Width, value.Height));
			}
		}
	}

	/// <summary>
	/// Gets or sets the minimum size of the window's client area
	/// </summary>
	/// <value>
	/// A value indicating the minimum size of the window's client area, or <c>0</c> individually for the width or height component to indicate no minimum limit set for that component
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public (int Width, int Height) MinimumSize
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (int Width, int Height) minSize);

				SdlErrorHelper.ThrowIfFailed(SDL_GetWindowMinimumSize(mWindow, &minSize.Width, &minSize.Height));

				return minSize;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowMinimumSize(mWindow, value.Width, value.Height));
			}
		}
	}

	/// <summary>
	/// Gets or sets the area of the window the mouse cursor is confined to
	/// </summary>
	/// <value>
	/// The area of the window the mouse cursor is confined to, or <c><see langword="null"/></c> if the mouse cursor is not confined 
	/// </value>
	/// <remarks>
	/// <para>
	/// Note that setting the value of this property to some non-<c><see langword="null"/></c> area, does <em>not</em> grab the cursor,
	/// it only defines the area the mouse cursor is restricted to when the window has mouse focus.
	/// If you want to grab the cursor, you can use the <see cref="HasMouseGrab"/> property for that.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public Rect<int>? MouseRect
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowMouseRect(mWindow) switch
				{
					null => null,
					var rect => *rect
				};
			}
		}

		set
		{
			unsafe
			{
				fixed (Rect<int>* rect = &Nullable.GetValueRefOrDefaultRef(in value))
				{
					SdlErrorHelper.ThrowIfFailed(SDL_SetWindowMouseRect(mWindow, value.HasValue ? rect : null));
				}
			}
		}
	}

	/// <summary>
	/// Gets or sets the opacity of the window
	/// </summary>
	/// <value>
	/// The opacity of the window, where <c>1.0</c> is fully opaque, <c>0.0</c> is fully transparent, and <c>-1.0</c> indicates an error when getting the value (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// The value set will be clamped to the range of <c>0.0</c> to <c>1.0</c>.
	/// </para>
	/// <para>
	/// If transparency isn't supported on the current platform, the value of this property will be <c>1.0</c> (as opposed to <c>-1.0</c> to indicate an error) when getting it,
	/// but this property will still <see langword="throw"/> an <see cref="SdlException"/> when you attempt to set it.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public float Opacity
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowOpacity(mWindow);
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowOpacity(mWindow, value));
			}
		}
	}

	/// <summary>
	/// Gets or sets the parent of the window, if any
	/// </summary>
	/// <value>
	/// The parent of the window, or <c><see langword="null"/></c> if the window has no parent
	/// </value>
	/// <remarks>
	/// <para>
	/// Setting the value of this property to a non-<c><see langword="null"/></c> window will make the current window a child of that window, and setting it to <c><see langword="null"/></c> will make the current window a top-level window.
	/// If the window is already the child of an existing window, it will be reparented to the new parent window.
	/// </para>
	/// <para>
	/// If a parent window is hidden or disposed/destroyed, this will also be recursively applied to all child windows.
	/// </para>
	/// <para>
	/// Child windows that weren't explicitly hidden (e.g. via <see cref="TryHide"/>) but were only hidden because their parent was hidden,
	/// will be automatically shown again when their parent is shown again.
	/// </para>
	/// <para>
	/// You need to be careful when handling the lifetime of child windows in relation to their parent windows.
	/// It's the best to <see cref="Dispose()">dispose</see> child windows before their parent windows, or resetting their parent to something else (or <c><see langword="null"/></c>) before their parent windows are disposed/destroyed.
	/// </para>
	/// <para>
	/// Attempting to set the parent of a window that is currently in the modal state will <see langword="throw"/> a <see cref="SdlException"/>.
	/// You can use the <see cref="IsModal"/> property to check if a window is currently in the modal state, and set it to <c><see langword="false"/></c> to disable the modal state before attempting to change the parent.
	/// </para>
	/// <para>
	/// Additionally, popup windows cannot change parents and attempts to do so will <see langword="throw"/> a <see cref="SdlException"/>.
	/// </para>
	/// <para>
	/// Setting a parent window that is currently the sibling or descendent of the child window results in undefined behavior. Please try to avoid doing that.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public Window? Parent
	{
		get
		{
			unsafe
			{
				TryGetOrCreate(SDL_GetWindowParent(mWindow), out var parent);
				return parent;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowParent(mWindow, value is not null ? value.Pointer : null));
			}
		}
	}

	/// <summary>
	/// Gets the pixel density of the window
	/// </summary>
	/// <value>
	/// The pixel density of the window, or <c>0.0</c> on failure (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// The pixel density is the ratio of pixel size to window size.
	/// For example, if the window is <c>1920</c> by <c>1080</c> and it has a high density back buffer of <c>3840</c> by <c>2160</c> pixels, it would have a pixel density of <c>2.0</c>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public float PixelDensity
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowPixelDensity(mWindow);
			}
		}
	}

	/// <summary>
	/// Gets the pixel format associated with the window
	/// </summary>
	/// <value>
	/// The pixel format associated with the window, or <see cref="PixelFormat.Unknown"/> on failure (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public PixelFormat PixelFormat
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowPixelFormat(mWindow);
			}
		}
	}

	internal unsafe SDL_Window* Pointer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mWindow; }

	/// <summary>
	/// Gets the position of the window
	/// </summary>
	/// <value>
	/// The position of the window
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property is the position of the window as it was last reported by the windowing backend.
	/// </para>
	/// <para>
	/// If you want to set the position of the window, you can use the <see cref="TrySetPosition(WindowPosition, WindowPosition)"/> method instead.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public (int X, int Y) Position
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (int X, int Y) position);

				SdlErrorHelper.ThrowIfFailed(SDL_GetWindowPosition(mWindow, &position.X, &position.Y));

				return position;
			}
		}
	}

	/// <summary>
	/// Gets or sets the state of the progress bar displayed for the window's taskbar entry
	/// </summary>
	/// <value>
	/// The state of the progress bar displayed for the window's taskbar entry, or <see cref="ProgressState.Invalid"/> on failure (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// You can use this property in conjunction with the <see cref="ProgressValue"/> property to display a progress bar with a certain value in the taskbar entry for the window.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public ProgressState ProgressState
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowProgressState(mWindow);
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowProgressState(mWindow, value));
			}
		}
	}

	/// <summary>
	/// Gets or sets the value of the progress bar displayed for the window's taskbar entry
	/// </summary>
	/// <value>
	/// The value of the progress bar displayed for the window's taskbar entry, where <c>0.0</c> is no progress, <c>1.0</c> is full progress
	/// </value>
	/// <remarks>
	/// <para>
	/// The value set will be clamped to the range of <c>0.0</c> to <c>1.0</c>.
	/// </para>
	/// <para>
	/// You can use this property in conjunction with the <see cref="ProgressState"/> property to display a progress bar with a certain value in the taskbar entry for the window.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public float ProgressValue
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowProgressValue(mWindow);
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowProgressValue(mWindow, value));
			}
		}
	}

	/// <summary>
	/// Gets the properties associated with the window
	/// </summary>
	/// <value>
	/// The properties associated with the window, or <c><see langword="null"/></c> if the properties could not be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public Properties? Properties
	{
		get
		{
			unsafe
			{
				return SDL_GetWindowProperties(mWindow) switch
				{
					0 => null,
					var id => Properties.GetOrCreate(id, registerWithSdl: false)
				};
			}
		}
	}

	/// <summary>
	/// Gets the renderer associated with the window, if any
	/// </summary>
	/// <value>
	/// The renderer associated with the window, or <c><see langword="null"/></c> if the window has no renderer or the renderer couldn't be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// Initially, a window can have no renderer associated with it if it was created without one (e.g. it was created with <see cref="TryCreate(string, int, int, out Window?, WindowFlags)"/> instead of <see cref="TryCreateWithRenderer(string, int, int, out Window?, out Renderer?, WindowFlags)"/>).
	/// The value of this property will be <c><see langword="null"/></c> in that case, as long as no renderer has been associated with the window yet.
	/// </para>
	/// <para>
	/// The value of this property can also be <c><see langword="null"/></c> if the renderer couldn't be retrieved successfully.
	/// You should check <see cref="Error.TryGet(out string?)"/> for more information and to see if that's the case.
	/// </para>
	/// <para>
	/// You can set a renderer for a window by using any of the <c>TryCreateRenderer*</c> methods (e.g. <see cref="TryCreateRenderer(out Renderer?, ReadOnlySpan{string})"/>) on the window instance, but only once.
	/// Once a renderer is successfully associated with the window, it can't be replaced and subsequently calls to any of the <c>TryCreateRenderer*</c> methods will fail.
	/// </para>
	/// </remarks>
	public Renderer? Renderer
	{
		get
		{
			unsafe
			{
				Renderer.TryGetOrCreate(SDL_GetRenderer(mWindow), out var renderer);
				return renderer;
			}
		}
	}

	/// <summary>
	/// Gets the safe area of the window
	/// </summary>
	/// <value>
	/// The safe area of the window
	/// </value>
	/// <remarks>
	/// <para>
	/// Some devices have portions of the screen which are partially obscured or not interactive, possibly due to on-screen controls, curved edges, camera notches, TV overscan, etc.
	/// This property provides the area of the current viewport which is safe to have interactible content.
	/// You should continue rendering into the rest of the window, but it should not contain visually important or interactible content.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public Rect<int> SafeArea
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out Rect<int> rect);

				SDL_GetWindowSafeArea(mWindow, &rect);

				return rect;
			}
		}
	}

	/// <summary>
	/// Gets the value of SDR white in the <see cref="ColorSpace.Srgb"/> color space for the window
	/// </summary>
	/// <value>
	/// The value of SDR white in the <see cref="ColorSpace.Srgb"/> color space for the window
	/// </value>
	/// <remarks>
	/// <para>
	/// On Windows the value of this property corresponds to the SDR white level in scRGB color space, and on Apple platform this is always <c>1.0</c> for EDR content.
	/// </para>
	/// <para>
	/// The value of this property can change dynamically at runtime when a <see cref="EventType.WindowHdrStateChanged"/> event is sent.
	/// </para>
	/// </remarks>
	public float SdrWhiteLevel => Properties?.TryGetFloatValue(PropertyNames.SdrWhiteLevelFloat, out var sdrWhiteLevel) is true
		? sdrWhiteLevel
		: default;

	/// <summary>
	/// Gets or sets the shape associated with the window, if any
	/// </summary>
	/// <value>
	/// The shape associated with the window, or <c><see langword="null"/></c> if the window has no associated shape
	/// </value>
	/// <remarks>
	/// <para>
	/// The alpha channel of the shape associated with the transparent window is used to determine the shape "visible" to the mouse cursor.
	/// Fully transparent areas of the shape are also transparent to mouse clicks.
	/// </para>
	/// <para>
	/// If you are using something else besides SDL to render the window, then you are responsible for drawing the alpha channel of the window to match the shape's alpha channel to get consistent cross-platform results.
	/// </para>
	/// <para>
	/// The given <see cref="Video.Surface"/> when setting this property is copied, so you can safely dispose of it after setting the property if you don't need it anymore.
	/// This also means that changes to the given surface after setting this property won't affect the shape of the window and you'd need to set this property again with the updated surface to change the shape of the window.
	/// </para>
	/// <para>
	/// Setting this property is an expensive operation and should be done sparingly.
	/// </para>
	/// <para>
	/// The window must have been created with the <see cref="WindowFlags.Transparent"/> flag to be able to associate a shape with it.
	/// Attempting to set a shape for a window that wasn't created with the <see cref="WindowFlags.Transparent"/> flag will <see langword="throw"/> a <see cref="SdlException"/>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public Surface? Shape
	{
		get
		{
			unsafe
			{
				return Properties?.TryGetPointerValue(PropertyNames.ShapePointer, out var shapePtr) is true
					&& Video.Surface.TryGetOrCreate(unchecked((Surface.SDL_Surface*)shapePtr), out var shape)
					? shape
					: default;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowShape(mWindow, value is not null ? value.Pointer : null));
			}
		}
	}

	/// <summary>
	/// Gets or sets the size of the window's client area
	/// </summary>
	/// <value>
	/// The size of the window's client area
	/// </value>
	/// <remarks>
	/// <para>
	/// The size of the window's client area reported by this property might be different from the size in pixels of the window's client area.
	/// This is especially the case if the window is on a display with a high pixel density.
	/// To get the real client area size in pixels, you can use the <see cref="SizeInPixels"/> property or the <see cref="Renderer.OutputSize"/> property (on the associated <see cref="Renderer"/>) instead.
	/// </para>
	/// <para>
	/// Setting the size of a window when the window is in fullscreen mode or maximized has no effect.
	/// To change the size of a window in fullscreen mode, you can use the <see cref="FullscreenMode"/> property instead.
	/// </para>
	/// <para>
	/// On some windowing backends, a request to change this setting is asynchronous and the new window size may not have have been applied immediately upon setting this property.
	/// If an immediate change is required, you can call <see cref="TrySync"/> to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window size changes, an <see cref="EventType.WindowResized"/> event will be emitted with the new window dimensions.
	/// Note that the new dimensions may not match the exact size requested, as some windowing backends can restrict the window size in certain scenarios
	/// (e.g. constraining the size of the content area to remain within the usable desktop bounds).
	/// Additionally, windowing backends can also deny the request to change this setting altogether.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public (int Width, int Height) Size
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (int Width, int Height) size);

				SdlErrorHelper.ThrowIfFailed(SDL_GetWindowSize(mWindow, &size.Width, &size.Height));

				return size;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetWindowSize(mWindow, value.Width, value.Height));
			}
		}
	}

	/// <summary>
	/// Gets the size in pixels of the window's client area
	/// </summary>
	/// <value>
	/// The size in pixels of the window's client area
	/// </value>
	/// <remarks>
	/// <para>
	/// The size in pixels of the window's client area reported by this property might be different from the size of the window's client area reported by the <see cref="Size"/> property.
	/// This is especially the case if the window is on a display with a high pixel density.
	/// </para>
	/// <para>
	/// If you want to set the size of the window's client area, you must use the <see cref="Size"/> property instead, but keep in mind that the sizes might differ.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">When getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public (int Width, int Height) SizeInPixels
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (int Width, int Height) sizeInPixels);

				SdlErrorHelper.ThrowIfFailed(SDL_GetWindowSizeInPixels(mWindow, &sizeInPixels.Width, &sizeInPixels.Height));

				return sizeInPixels;
			}
		}
	}

	private WindowSurface? mSurface = null;

	/// <summary>
	/// Gets the <see cref="WindowSurface"/> associated with the window
	/// </summary>
	/// <value>
	/// The <see cref="WindowSurface"/> associated with the window
	/// </value>
	/// <remarks>
	/// <para>
	/// If necessary, a new surface will be created with the optimal format for the window.
	/// While you don't need to <see cref="Surface.Dispose()">dispose</see> of the surface manually, doing so will invalidate the surface and a new one will be created the next time you access this property.
	/// </para>
	/// <para>
	/// You can check the <see cref="HasSurface"/> to determine whether the window currently has a valid surface associated with it.
	/// If the value of the <see cref="HasSurface"/> property is <c><see langword="true"/></c>, you can access the <see cref="Surface"/> property and get the existing associated surface,
	/// otherwise, if the value is <see langword="false"/>, accessing the <see cref="Surface"/> property will create a new surface and associate it with the window.
	/// If you don't want to create a new surface unnecessarily, you should check the <see cref="HasSurface"/> property before accessing the <see cref="Surface"/> property!
	/// </para>
	/// <para>
	/// <see cref="WindowSurface"/>s can be used as an alternative to using a <see cref="Renderer"/> on the window.
	/// </para>
	/// <para>
	/// To reflect changes made to the surface, you need to call either the <see cref="WindowSurface.TryUpdate"/> or <see cref="WindowSurface.TryUpdateRects(ReadOnlySpan{Rect{int}})"/> method on the surface,
	/// in order for the surface to be copied to screen and displayed as the window's content. Changes to the surface won't be displayed until you call one of those methods.
	/// </para>
	/// <para>
	/// Do <em>not</em> attempt to use a <see cref="WindowSurface"/> together with a <see cref="Renderer"/> on the same <see cref="Window"/>!
	/// They are meant to be used mutually exclusively for the same <see cref="Window"/>.
	/// </para>
	/// <para>
	/// The surface associated with the window will be invalidated if the window is resized. After resizing a window you need to access this property again to get a new valid surface.
	/// </para>
	/// <para>
	/// This property is affected by the <see cref="Hint.FrameBufferAcceleration"/> hint.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public WindowSurface Surface
	{
		get
		{
			unsafe
			{
				var oldSurface = mSurface;
				try
				{
					SdlErrorHelper.ThrowIfFailed(WindowSurface.TryGetOrCreate(this, out mSurface));

					// We rely on SDL actually setting an error message when it fails to get or create the surface.
					// In that case the previous statement would have thrown an exception and mSurface would have been set to null (which is alright),
					// or else mSurface would have been set to the new or existing surface. Thus mSurface is non-null at this point.

					return mSurface!;
				}
				finally
				{
					if (!ReferenceEquals(oldSurface, mSurface) && oldSurface is not null)
					{
						oldSurface.DontDestroy = true; // SDL already destroyed the previous surface if it was invalidate or replaced with a new one
						oldSurface.Dispose();
					}
				}
			}
		}
	}

	/// <summary>
	/// Gets or sets the title of the window
	/// </summary>
	/// <value>
	/// The title of the window
	/// </value>
	/// <remarks>
	/// <para>
	/// Depending on the platform and windowing backend, Unicode characters may be allowed in the window title.
	/// </para>
	/// </remarks>
	public string Title
	{
		get
		{
			unsafe
			{
				return Utf8StringMarshaller.ConvertToManaged(SDL_GetWindowTitle(mWindow)) ?? string.Empty;
			}
		}

		set
		{
			unsafe
			{
				var valueUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(value);
				try
				{
					SdlErrorHelper.ThrowIfFailed(SDL_SetWindowTitle(mWindow, valueUtf8));
				}
				finally
				{
					Utf8StringMarshaller.Free(valueUtf8);
				}
			}
		}
	}

	/// <summary>
	/// Disposes the window
	/// </summary>
	/// <remarks>
	/// <para>
	/// After disposing a <see cref="Window"/>, it becomes <see cref="IsValid">invalid</see> and all of its associated resources (e.g. <see cref="Renderer"/>, <see cref="Surface"/>, etc.) will be invalidated as well.
	/// Do not attempt to access or use any of those resources after the window has been disposed.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(disposing: true, forget: true);
	}

	private protected virtual void Dispose(bool disposing, bool forget)
	{
		unsafe
		{
			if (mWindow is not null)
			{
				if (mSurface is not null)
				{
					mSurface.DontDestroy = true; // SDL_DestroyWindow will automatically destroy the surface if it exists
					mSurface.Dispose();
					mSurface = null;
				}

				if (forget)
				{
					mKnownInstances.TryRemove(unchecked((IntPtr)mWindow), out _);
				}

				if (mSelfHandle.IsAllocated)
				{
					// Hopefully 'EventWatchDestroyImpl' doesn't change its address during the lifetime of the window (it shouldn't)
					// and 'GCHandle.ToIntPtr' always returns the same value for the same GCHandle (it should).
					Sdl.SDL_RemoveEventWatch(&EventWatchResizeOrDestroy, unchecked((void*)GCHandle.ToIntPtr(mSelfHandle)));

					mSelfHandle.Free();
					mSelfHandle = default;
				}

				SDL_DestroyWindow(mWindow);
				mWindow = null;
			}
		}
	}

	// TODO: add cref for OpenGL and Vulkan library types once they are implemented
	/// <summary>
	/// Tries to create a new <see cref="Window"/> with the specified title, size, and flags
	/// </summary>
	/// <param name="title">The title of the window. Depending on the platform and windowing backend, Unicode characters may be allowed in the window title.</param>
	/// <param name="width">The width of the window</param>
	/// <param name="height">The height of the window</param>
	/// <param name="window">The resulting <see cref="Window"/>, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <param name="flags">The flags the window should be created with</param>
	/// <returns><c><see langword="true"/></c>, if the window was created successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The actual size of the created window may differ from the specified size, based on the desktop layout and window manager's policies. You should be prepared to handle windows of any size.
	/// </para>
	/// <para>
	/// The resulting window will be immediately shown if the <see cref="WindowFlags.Hidden"/> flag isn't specified, otherwise it will be created hidden and you can show it later using the <see cref="TryShow"/> method.
	/// </para>
	/// <para>
	/// On Apple's macOS, you <em>must</em> set the <c>NSHighResolutionCapable</c> property in your <c>Info.plist</c> to <c>YES</c>, otherwise you will not receive a high-DPI OpenGL canvas.
	/// </para>
	/// <para>
	/// The <see cref="SizeInPixels">window's size in pixels</see> may differ from its <see cref="Size">window's coordinate size</see> if the window is on a high pixel density display.
	/// You can use the <see cref="Size"/> property to get the window's coordinate size and the <see cref="SizeInPixels"/> property or the <see cref="Renderer.OutputSize"/> property on the associated <see cref="Renderer"/> to get the window's drawable size in pixels.
	/// Note that the drawable size of a window can vary after the window is created and should be checked again after an <see cref="EventType.WindowPixelSizeChanged"/> event is received.
	/// </para>
	/// <para>
	/// If the window is created with any of the <see cref="WindowFlags.OpenGL"/> or <see cref="WindowFlags.Vulkan"/> flags,
	/// then the corresponding library (<see cref=""/> or <see cref=""/>) will be loaded upon creation, if necessary.
	/// Afterwards, when the window is <see cref="Dispose()">disposed</see>/destroyed the corresponding library will be unloaded again.
	/// </para>
	/// <para>
	/// If the <see cref="WindowFlags.Vulkan"/> flag is specified and there isn't a working Vulkan driver SDL can use, this method will fail.
	/// </para>
	/// <para>
	/// If the <see cref="WindowFlags.Metal"/> flag is specified and the platform doesn't support Metal, this method will fail.
	/// </para>
	/// <para>
	/// If you intend to use the newly created window with a <see cref="Rendering.Renderer"/>, you can use the <see cref="TryCreateWithRenderer(string, int, int, out Window?, out Renderer?, WindowFlags)"/> method instead, to avoid window flicker.
	/// Otherwise, you can create a <see cref="Rendering.Renderer"/> for the window later using any of its <c>TryCreateRenderer*</c> methods (e.g. <see cref="TryCreateRenderer(out Renderer?, ReadOnlySpan{string})"/>).
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryCreate(string title, int width, int height, [NotNullWhen(true)] out Window? window, WindowFlags flags = WindowFlags.None)
	{
		unsafe
		{
			var titleUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(title);
			try
			{
				var windowPtr = SDL_CreateWindow(titleUtf8, width, height, flags);

				if (windowPtr is null)
				{
					window = null;
					return false;
				}

				window = RegisteredDriverOrGenericFallbackDriverFactory.Create(windowPtr, register: true);
				return true;
			}
			finally
			{
				Utf8StringMarshaller.Free(titleUtf8);
			}
		}
	}

	private static bool TryCreate<TFactory, TWindow>([NotNullWhen(true)] out TWindow? window, bool? alwaysOnTop = default, bool? bordered = default, bool? constrainPopup = default, bool? externalGraphicsContext = default, WindowFlags? flags = default,
		bool? focusable = default, bool? fullscreen = default, int? height = default, bool? hidden = default, bool? highPixelDensity = default, bool? maximized = default, bool? menu = default, bool? metal = default,
		bool? minimized = default, bool? modal = default, bool? mouseGrabbed = default, bool? openGL = default, Window? parent = default, bool? resizable = default, string? title = default, bool? transparent = default,
		bool? tooltip = default, bool? utility = default, bool? vulkan = default, int? width = default, WindowPosition? x = default, WindowPosition? y = default, Properties? properties = default)
		where TFactory : notnull, IFactory<TWindow>
		where TWindow : notnull, Window
	{
		unsafe
		{
			Properties propertiesUsed;
			Unsafe.SkipInit(out bool? alwaysOnTopBackup);
			Unsafe.SkipInit(out bool? borderedBackup);
			Unsafe.SkipInit(out bool? constrainPopupBackup);
			Unsafe.SkipInit(out bool? externalGraphicsContextBackup);
			Unsafe.SkipInit(out WindowFlags? flagsBackup);
			Unsafe.SkipInit(out bool? focusableBackup);
			Unsafe.SkipInit(out bool? fullscreenBackup);
			Unsafe.SkipInit(out int? heightBackup);
			Unsafe.SkipInit(out bool? hiddenBackup);
			Unsafe.SkipInit(out bool? highPixelDensityBackup);
			Unsafe.SkipInit(out bool? maximizedBackup);
			Unsafe.SkipInit(out bool? menuBackup);
			Unsafe.SkipInit(out bool? metalBackup);
			Unsafe.SkipInit(out bool? minimizedBackup);
			Unsafe.SkipInit(out bool? modalBackup);
			Unsafe.SkipInit(out bool? mouseGrabbedBackup);
			Unsafe.SkipInit(out bool? openGLBackup);
			Unsafe.SkipInit(out IntPtr? parentBackup);
			Unsafe.SkipInit(out bool? resizableBackup);
			Unsafe.SkipInit(out string? titleBackup);
			Unsafe.SkipInit(out bool? transparentBackup);
			Unsafe.SkipInit(out bool? tooltipBackup);
			Unsafe.SkipInit(out bool? utilityBackup);
			Unsafe.SkipInit(out bool? vulkanBackup);
			Unsafe.SkipInit(out int? widthBackup);
			Unsafe.SkipInit(out WindowPosition? xBackup);
			Unsafe.SkipInit(out WindowPosition? yBackup);

			if (properties is null)
			{
				propertiesUsed = [];

				if (alwaysOnTop is bool alwaysOnTopValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateAlwaysOnTopBoolean, alwaysOnTopValue);
				}

				if (bordered is bool borderedValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateBorderlessBoolean, !borderedValue);
				}

				if (constrainPopup is bool constrainPopupValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateConstrainPopupBoolean, constrainPopupValue);
				}

				if (externalGraphicsContext is bool externalGraphicsContextValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateExternalGraphicsContextBoolean, externalGraphicsContextValue);
				}

				if (flags is WindowFlags flagsValue)
				{
					propertiesUsed.TrySetNumberValue(PropertyNames.CreateFlagsNumber, unchecked((long)(ulong)flagsValue));
				}

				if (focusable is bool focusableValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateFocusableBoolean, focusableValue);
				}

				if (fullscreen is bool fullscreenValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateFullscreenBoolean, fullscreenValue);
				}

				if (height is int heightValue)
				{
					propertiesUsed.TrySetNumberValue(PropertyNames.CreateHeightNumber, heightValue);
				}

				if (width is int widthValue)
				{
					propertiesUsed.TrySetNumberValue(PropertyNames.CreateWidthNumber, widthValue);
				}

				if (hidden is bool hiddenValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateHiddenBoolean, hiddenValue);
				}

				if (highPixelDensity is bool highPixelDensityValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateHighPixelDensityBoolean, highPixelDensityValue);
				}

				if (maximized is bool maximizedValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMaximizedBoolean, maximizedValue);
				}

				if (menu is bool menuValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMenuBoolean, menuValue);
				}

				if (metal is bool metalValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMetalBoolean, metalValue);
				}

				if (minimized is bool minimizedValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMinimizedBoolean, minimizedValue);
				}

				if (modal is bool modalValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateModalBoolean, modalValue);
				}

				if (mouseGrabbed is bool mouseGrabbedValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMouseGrabbedBoolean, mouseGrabbedValue);
				}

				if (openGL is bool openGLValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateOpenGLBoolean, openGLValue);
				}

				if (parent is not null)
				{
					propertiesUsed.TrySetPointerValue(PropertyNames.CreateParentPointer, unchecked((IntPtr)parent.mWindow));
				}

				if (resizable is bool resizableValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateResizableBoolean, resizableValue);
				}

				if (title is not null)
				{
					propertiesUsed.TrySetStringValue(PropertyNames.CreateTitleString, title);
				}

				if (tooltip is bool tooltipValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateTooltipBoolean, tooltipValue);
				}

				if (transparent is bool transparentValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateTransparentBoolean, transparentValue);
				}

				if (utility is bool utilityValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateUtilityBoolean, utilityValue);
				}

				if (vulkan is bool vulkanValue)
				{
					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateVulkanBoolean, vulkanValue);
				}

				if (x is WindowPosition xValue)
				{
					propertiesUsed.TrySetNumberValue(PropertyNames.CreateXNumber, Unsafe.BitCast<WindowPosition, int>(xValue));
				}

				if (y is WindowPosition yValue)
				{
					propertiesUsed.TrySetNumberValue(PropertyNames.CreateYNumber, Unsafe.BitCast<WindowPosition, int>(yValue));
				}
			}
			else
			{
				propertiesUsed = properties;

				if (alwaysOnTop is bool alwaysOnTopValue)
				{
					alwaysOnTopBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateAlwaysOnTopBoolean, out var existingAlwaysOnTopValue)
						? existingAlwaysOnTopValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateAlwaysOnTopBoolean, alwaysOnTopValue);
				}

				if (bordered is bool borderedValue)
				{
					borderedBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateBorderlessBoolean, out var existingBorderlessValue)
						? !existingBorderlessValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateBorderlessBoolean, !borderedValue);
				}

				if (constrainPopup is bool constrainPopupValue)
				{
					constrainPopupBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateConstrainPopupBoolean, out var existingConstrainPopupValue)
						? existingConstrainPopupValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateConstrainPopupBoolean, constrainPopupValue);
				}

				if (externalGraphicsContext is bool externalGraphicsContextValue)
				{
					externalGraphicsContextBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateExternalGraphicsContextBoolean, out var existingExternalGraphicsContextValue)
						? existingExternalGraphicsContextValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateExternalGraphicsContextBoolean, externalGraphicsContextValue);
				}

				if (flags is WindowFlags flagsValue)
				{
					flagsBackup = propertiesUsed.TryGetNumberValue(PropertyNames.CreateFlagsNumber, out var existingFlagsValue)
						? unchecked((WindowFlags)(ulong)existingFlagsValue)
						: null;

					propertiesUsed.TrySetNumberValue(PropertyNames.CreateFlagsNumber, unchecked((long)(ulong)flagsValue));
				}

				if (focusable is bool focusableValue)
				{
					focusableBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateFocusableBoolean, out var existingFocusableValue)
						? existingFocusableValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateFocusableBoolean, focusableValue);
				}

				if (fullscreen is bool fullscreenValue)
				{
					fullscreenBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateFullscreenBoolean, out var existingFullscreenValue)
						? existingFullscreenValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateFullscreenBoolean, fullscreenValue);
				}

				if (height is int heightValue)
				{
					heightBackup = propertiesUsed.TryGetNumberValue(PropertyNames.CreateHeightNumber, out var existingHeightValue)
						? unchecked((int)existingHeightValue)
						: null;

					propertiesUsed.TrySetNumberValue(PropertyNames.CreateHeightNumber, heightValue);
				}

				if (hidden is bool hiddenValue)
				{
					hiddenBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateHiddenBoolean, out var existingHiddenValue)
						? existingHiddenValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateHiddenBoolean, hiddenValue);
				}

				if (highPixelDensity is bool highPixelDensityValue)
				{
					highPixelDensityBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateHighPixelDensityBoolean, out var existingHighPixelDensityValue)
						? existingHighPixelDensityValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateHighPixelDensityBoolean, highPixelDensityValue);
				}

				if (maximized is bool maximizedValue)
				{
					maximizedBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateMaximizedBoolean, out var existingMaximizedValue)
						? existingMaximizedValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMaximizedBoolean, maximizedValue);
				}

				if (menu is bool menuValue)
				{
					menuBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateMenuBoolean, out var existingMenuValue)
						? existingMenuValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMenuBoolean, menuValue);
				}

				if (metal is bool metalValue)
				{
					metalBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateMetalBoolean, out var existingMetalValue)
						? existingMetalValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMetalBoolean, metalValue);
				}

				if (minimized is bool minimizedValue)
				{
					minimizedBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateMinimizedBoolean, out var existingMinimizedValue)
						? existingMinimizedValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMinimizedBoolean, minimizedValue);
				}

				if (modal is bool modalValue)
				{
					modalBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateModalBoolean, out var existingModalValue)
						? existingModalValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateModalBoolean, modalValue);
				}

				if (mouseGrabbed is bool mouseGrabbedValue)
				{
					mouseGrabbedBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateMouseGrabbedBoolean, out var existingMouseGrabbedValue)
						? existingMouseGrabbedValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMouseGrabbedBoolean, mouseGrabbedValue);
				}

				if (openGL is bool openGLValue)
				{
					openGLBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateOpenGLBoolean, out var existingOpenGLValue)
						? existingOpenGLValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateOpenGLBoolean, openGLValue);
				}

				if (parent is not null)
				{
					parentBackup = propertiesUsed.TryGetPointerValue(PropertyNames.CreateParentPointer, out var existingParentValue)
						? existingParentValue
						: null;

					propertiesUsed.TrySetPointerValue(PropertyNames.CreateParentPointer, unchecked((IntPtr)parent.mWindow));
				}

				if (resizable is bool resizableValue)
				{
					resizableBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateResizableBoolean, out var existingResizableValue)
						? existingResizableValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateResizableBoolean, resizableValue);
				}

				if (title is not null)
				{
					titleBackup = propertiesUsed.TryGetStringValue(PropertyNames.CreateTitleString, out var existingTitleValue)
						? existingTitleValue
						: null;

					propertiesUsed.TrySetStringValue(PropertyNames.CreateTitleString, title);
				}

				if (transparent is bool transparentValue)
				{
					transparentBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateTransparentBoolean, out var existingTransparentValue)
						? existingTransparentValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateTransparentBoolean, transparentValue);
				}

				if (tooltip is bool tooltipValue)
				{
					tooltipBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateTooltipBoolean, out var existingTooltipValue)
						? existingTooltipValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateTooltipBoolean, tooltipValue);
				}

				if (utility is bool utilityValue)
				{
					utilityBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateUtilityBoolean, out var existingUtilityValue)
						? existingUtilityValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateUtilityBoolean, utilityValue);
				}

				if (vulkan is bool vulkanValue)
				{
					vulkanBackup = propertiesUsed.TryGetBooleanValue(PropertyNames.CreateVulkanBoolean, out var existingVulkanValue)
						? existingVulkanValue
						: null;

					propertiesUsed.TrySetBooleanValue(PropertyNames.CreateVulkanBoolean, vulkanValue);
				}

				if (width is int widthValue)
				{
					widthBackup = propertiesUsed.TryGetNumberValue(PropertyNames.CreateWidthNumber, out var existingWidthValue)
						? unchecked((int)existingWidthValue)
						: null;

					propertiesUsed.TrySetNumberValue(PropertyNames.CreateWidthNumber, widthValue);
				}

				if (x is WindowPosition xValue)
				{
					xBackup = propertiesUsed.TryGetNumberValue(PropertyNames.CreateXNumber, out var existingXValue)
						? Unsafe.BitCast<int, WindowPosition>(unchecked((int)existingXValue))
						: null;

					propertiesUsed.TrySetNumberValue(PropertyNames.CreateXNumber, Unsafe.BitCast<WindowPosition, int>(xValue));
				}

				if (y is WindowPosition yValue)
				{
					yBackup = propertiesUsed.TryGetNumberValue(PropertyNames.CreateYNumber, out var existingYValue)
						? Unsafe.BitCast<int, WindowPosition>(unchecked((int)existingYValue))
						: null;

					propertiesUsed.TrySetNumberValue(PropertyNames.CreateYNumber, Unsafe.BitCast<WindowPosition, int>(yValue));
				}
			}

			try
			{
				var windowPtr = SDL_CreateWindowWithProperties(propertiesUsed.Id);

				if (windowPtr is null)
				{
					window = null;
					return false;
				}

				window = TFactory.Create(windowPtr, register: true);
				return true;
			}
			finally
			{
				if (properties is null)
				{
					// propertiesUsed was just a temporary instance we created for this call, so we need to dispose it now

					propertiesUsed.Dispose();
				}
				else
				{
					// we restored the original properties values from the given properties instance

					if (alwaysOnTop.HasValue)
					{
						if (alwaysOnTopBackup is bool alwaysOnTopValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateAlwaysOnTopBoolean, alwaysOnTopValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateAlwaysOnTopBoolean);
						}
					}

					if (bordered.HasValue)
					{
						if (borderedBackup is bool borderedValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateBorderlessBoolean, !borderedValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateBorderlessBoolean);
						}
					}

					if (constrainPopup.HasValue)
					{
						if (constrainPopupBackup is bool constrainPopupValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateConstrainPopupBoolean, constrainPopupValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateConstrainPopupBoolean);
						}
					}

					if (externalGraphicsContext.HasValue)
					{
						if (externalGraphicsContextBackup is bool externalGraphicsContextValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateExternalGraphicsContextBoolean, externalGraphicsContextValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateExternalGraphicsContextBoolean);
						}
					}

					if (flags.HasValue)
					{
						if (flagsBackup is WindowFlags flagsValue)
						{
							propertiesUsed.TrySetNumberValue(PropertyNames.CreateFlagsNumber, unchecked((long)(ulong)flagsValue));
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateFlagsNumber);
						}
					}

					if (focusable.HasValue)
					{
						if (focusableBackup is bool focusableValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateFocusableBoolean, focusableValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateFocusableBoolean);
						}
					}

					if (fullscreen.HasValue)
					{
						if (fullscreenBackup is bool fullscreenValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateFullscreenBoolean, fullscreenValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateFullscreenBoolean);
						}
					}

					if (height.HasValue)
					{
						if (heightBackup is int heightValue)
						{
							propertiesUsed.TrySetNumberValue(PropertyNames.CreateHeightNumber, heightValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateHeightNumber);
						}
					}

					if (hidden.HasValue)
					{
						if (hiddenBackup is bool hiddenValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateHiddenBoolean, hiddenValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateHiddenBoolean);
						}
					}

					if (highPixelDensity.HasValue)
					{
						if (highPixelDensityBackup is bool highPixelDensityValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateHighPixelDensityBoolean, highPixelDensityValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateHighPixelDensityBoolean);
						}
					}

					if (maximized.HasValue)
					{
						if (maximizedBackup is bool maximizedValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMaximizedBoolean, maximizedValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateMaximizedBoolean);
						}
					}

					if (menu.HasValue)
					{
						if (menuBackup is bool menuValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMenuBoolean, menuValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateMenuBoolean);
						}
					}

					if (metal.HasValue)
					{
						if (metalBackup is bool metalValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMetalBoolean, metalValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateMetalBoolean);
						}
					}

					if (minimized.HasValue)
					{
						if (minimizedBackup is bool minimizedValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMinimizedBoolean, minimizedValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateMinimizedBoolean);
						}
					}

					if (modal.HasValue)
					{
						if (modalBackup is bool modalValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateModalBoolean, modalValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateModalBoolean);
						}
					}

					if (mouseGrabbed.HasValue)
					{
						if (mouseGrabbedBackup is bool mouseGrabbedValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateMouseGrabbedBoolean, mouseGrabbedValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateMouseGrabbedBoolean);
						}
					}

					if (openGL.HasValue)
					{
						if (openGLBackup is bool openGLValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateOpenGLBoolean, openGLValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateOpenGLBoolean);
						}
					}

					if (parent is not null)
					{
						if (parentBackup is IntPtr parentValue)
						{
							propertiesUsed.TrySetPointerValue(PropertyNames.CreateParentPointer, parentValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateParentPointer);
						}
					}

					if (resizable.HasValue)
					{
						if (resizableBackup is bool resizableValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateResizableBoolean, resizableValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateResizableBoolean);
						}
					}

					if (title is not null)
					{
						if (titleBackup is string titleValue)
						{
							propertiesUsed.TrySetStringValue(PropertyNames.CreateTitleString, titleValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateTitleString);
						}
					}

					if (transparent.HasValue)
					{
						if (transparentBackup is bool transparentValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateTransparentBoolean, transparentValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateTransparentBoolean);
						}
					}

					if (tooltip.HasValue)
					{
						if (tooltipBackup is bool tooltipValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateTooltipBoolean, tooltipValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateTooltipBoolean);
						}
					}

					if (utility.HasValue)
					{
						if (utilityBackup is bool utilityValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateUtilityBoolean, utilityValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateUtilityBoolean);
						}
					}

					if (vulkan.HasValue)
					{
						if (vulkanBackup is bool vulkanValue)
						{
							propertiesUsed.TrySetBooleanValue(PropertyNames.CreateVulkanBoolean, vulkanValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateVulkanBoolean);
						}
					}

					if (width.HasValue)
					{
						if (widthBackup is int widthValue)
						{
							propertiesUsed.TrySetNumberValue(PropertyNames.CreateWidthNumber, widthValue);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateWidthNumber);
						}
					}

					if (x.HasValue)
					{
						if (xBackup is WindowPosition xValue)
						{
							propertiesUsed.TrySetNumberValue(PropertyNames.CreateXNumber, Unsafe.BitCast<WindowPosition, int>(xValue));
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateXNumber);
						}
					}

					if (y.HasValue)
					{
						if (yBackup is WindowPosition yValue)
						{
							propertiesUsed.TrySetNumberValue(PropertyNames.CreateYNumber, Unsafe.BitCast<WindowPosition, int>(yValue));
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateYNumber);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Tries to create a new <see cref="Window"/> with the specified parameters
	/// </summary>
	/// <param name="window">The resulting <see cref="Window"/>, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <param name="alwaysOnTop">A value indicating whether the window should always be above others</param>
	/// <param name="bordered">A value indicating whether the window should have a border (decorations)</param>
	/// <param name="constrainPopup">
	/// A value indicating whether a "tooltip" or "menu" popup window should be automatically constrained to the bounds of the display.
	/// If this parameter is <c><see langword="null"/></c> (the default), the default behavior is to constrain popup windows.
	/// </param>
	/// <param name="externalGraphicsContext">A value indicating whether the window will be used with an externally managed graphics context</param>
	/// <param name="flags">The flags the window should be created with</param>
	/// <param name="focusable">
	/// A value indicating whether the window should accept input focus.
	/// If this parameter is <c><see langword="null"/></c> (the default), the default behavior is to accept input focus.
	/// </param>
	/// <param name="fullscreen">A value indicating whether the window should be created initially in fullscreen mode at desktop resolution</param>
	/// <param name="height">The height of the window</param>
	/// <param name="hidden">A value indicating whether the window should be created initially hidden</param>
	/// <param name="highPixelDensity">A value indicating whether the window should be created with a high pixel density buffer, if possible</param>
	/// <param name="maximized">A value indicating whether the window should be created initially maximized</param>
	/// <param name="menu">
	/// A value indicating whether the window should be created as a "menu" popup window.
	/// If this parameter is set to <c><see langword="true"/></c>, you must specify a <paramref name="parent"/> window for the window to be created.
	/// </param>
	/// <param name="metal">A value indicating whether the window will be used with Metal rendering</param>
	/// <param name="minimized">A value indicating whether the window should be created initially minimized</param>
	/// <param name="modal">
	/// A value indicating whether the window should be created as modal to its parent window.
	/// If this parameter is set to <c><see langword="true"/></c>, you must specify a <paramref name="parent"/> window for the window to be created.
	/// </param>
	/// <param name="mouseGrabbed">A value indicating whether the window should be created initially with the mouse grabbed</param>
	/// <param name="openGL">A value indicating whether the window will be used with OpenGL rendering</param>
	/// <param name="parent">
	/// The parent <see cref="Window"/> of the window to be created.
	/// You must specify a non-<c><see langword="null"/></c> window when you want to create a "tooltip" or "menu" popup window or a window that should be modal to its parent window.
	/// </param>
	/// <param name="resizable">A value indicating whether the window should be resizable by the user</param>
	/// <param name="title">The title of the window. Depending on the platform and windowing backend, Unicode characters may be allowed in the window title.</param>
	/// <param name="transparent">
	/// A value indicating whether the window should be created with a transparent buffer.
	/// The value of this parameter will determine whether the window will be shown transparent in areas where the alpha channel value of the window's buffer is equal to <c>0</c>.
	/// </param>
	/// <param name="tooltip">A value indicating whether the window should be created as a "tooltip" popup window</param>
	/// <param name="utility">A value indicating whether the window should be created as a utility window (e.g. not showing in the task bar and window list)</param>
	/// <param name="vulkan">A value indicating whether the window will be used with Vulkan rendering</param>
	/// <param name="width">The width of the window</param>
	/// <param name="x">
	/// The X coordinate of the window, <see cref="WindowPosition.Centered"/>, <see cref="WindowPosition.Undefined"/>, or a value obtained from using the <see cref="WindowPosition.CenteredOn(Display)"/> or <see cref="WindowPosition.UndefinedOn(Display)"/> methods.
	/// You can either specify definitive coordinates as the value for this parameter,
	/// or you can use <see cref="WindowPosition.Centered"/>, <see cref="WindowPosition.Undefined"/>, <see cref="WindowPosition.CenteredOn(Display)"/>, or <see cref="WindowPosition.UndefinedOn(Display)"/>
	/// to specify some special window positions to be determined in relation to the primary display or a specific display.
	/// </param>
	/// <param name="y">
	/// The Y coordinate of the window, <see cref="WindowPosition.Centered"/>, <see cref="WindowPosition.Undefined"/>, or a value obtained from using the <see cref="WindowPosition.CenteredOn(Display)"/> or <see cref="WindowPosition.UndefinedOn(Display)"/> methods.
	/// You can either specify definitive coordinates as the value for this parameter,
	/// or you can use <see cref="WindowPosition.Centered"/>, <see cref="WindowPosition.Undefined"/>, <see cref="WindowPosition.CenteredOn(Display)"/>, or <see cref="WindowPosition.UndefinedOn(Display)"/>
	/// to specify some special window positions to be determined in relation to the primary display or a specific display.
	/// </param>
	/// <param name="properties">Additional properties to use when creating the window</param>
	/// <returns><c><see langword="true"/></c>, if the window was created successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The actual size of the created window may differ from the specified size, based on the desktop layout and window manager's policies. You should be prepared to handle windows of any size.
	/// </para>
	/// <para>
	/// The resulting window will be immediately shown if the <paramref name="hidden"/> parameter isn't set to <c><see langword="true"/></c>, otherwise it will be created hidden and you can show it later using the <see cref="TryShow"/> method.
	/// </para>
	/// <para>
	/// If this window is intended to being be used with a <see cref="Rendering.Renderer"/>, you should not use a graphics API specific parameter (<paramref name="openGL"/>, <paramref name="vulkan"/>, etc), as SDL will handle that internally when it chooses a renderer,
	/// while creating the renderer with any of the <c>TryCreateRenderer*</c> methods.
	/// However, SDL might need to recreate your window at that point, which may cause the window to appear briefly, and then flicker as it is recreated.
	/// The correct approach to this is to create the window with the <paramref name="hidden"/> parameter set to <c><see langword="true"/></c>, then create the renderer, then show the window with <see cref="TryShow"/>.
	/// </para>
	/// <para>
	/// You can see the <see cref="TryCreate(string, int, int, out Window?, WindowFlags)"/> method for more in-depth information about creating windows and the available flags.
	/// </para>
	/// <para>
	/// You can see the <see cref="TryCreatePopup(int, int, int, int, out Window?, WindowFlags)"/> method for more in-depth information about creating popup windows and how "tooltip" and "menu" popup windows work.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryCreate([NotNullWhen(true)] out Window? window, bool? alwaysOnTop = default, bool? bordered = default, bool? constrainPopup = default, bool? externalGraphicsContext = default, WindowFlags? flags = default,
		bool? focusable = default, bool? fullscreen = default, int? height = default, bool? hidden = default, bool? highPixelDensity = default, bool? maximized = default, bool? menu = default, bool? metal = default,
		bool? minimized = default, bool? modal = default, bool? mouseGrabbed = default, bool? openGL = default, Window? parent = default, bool? resizable = default, string? title = default, bool? transparent = default,
		bool? tooltip = default, bool? utility = default, bool? vulkan = default, int? width = default, WindowPosition? x = default, WindowPosition? y = default, Properties? properties = default)
		=> TryCreate<RegisteredDriverOrGenericFallbackDriverFactory, Window>(
			out window, alwaysOnTop, bordered, constrainPopup, externalGraphicsContext, flags,
			focusable, fullscreen, height, hidden, highPixelDensity, maximized, menu, metal,
			minimized, modal, mouseGrabbed, openGL, parent, resizable, title, transparent,
			tooltip, utility, vulkan, width, x, y, properties
		);

	/// <inheritdoc cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)"/>
	/// <typeparam name="TDriver">
	/// The windowing driver type associated with the resulting window. 
	/// Note that this method will fail if the currently active windowing driver does not match the specified <typeparamref name="TDriver"/> type.
	/// </typeparam>
	public static bool TryCreate<TDriver>([NotNullWhen(true)] out Window<TDriver>? window, bool? alwaysOnTop = default, bool? bordered = default, bool? constrainPopup = default, bool? externalGraphicsContext = default, WindowFlags? flags = default,
		bool? focusable = default, bool? fullscreen = default, int? height = default, bool? hidden = default, bool? highPixelDensity = default, bool? maximized = default, bool? menu = default, bool? metal = default,
		bool? minimized = default, bool? modal = default, bool? mouseGrabbed = default, bool? openGL = default, Window? parent = default, bool? resizable = default, string? title = default, bool? transparent = default,
		bool? tooltip = default, bool? utility = default, bool? vulkan = default, int? width = default, WindowPosition? x = default, WindowPosition? y = default, Properties? properties = default)
		where TDriver : notnull, IWindowingDriver
	{
		if (!WindowingDriverExtensions.get_IsActive<TDriver>())
		{
			window = null;
			return false;
		}

		return TryCreateUnchecked(
			out window, alwaysOnTop, bordered, constrainPopup, externalGraphicsContext, flags,
			focusable, fullscreen, height, hidden, highPixelDensity, maximized, menu, metal,
			minimized, modal, mouseGrabbed, openGL, parent, resizable, title, transparent,
			tooltip, utility, vulkan, width, x, y, properties
		);
	}

	internal static bool TryCreateUnchecked<TDriver>([NotNullWhen(true)] out Window<TDriver>? window, bool? alwaysOnTop = default, bool? bordered = default, bool? constrainPopup = default, bool? externalGraphicsContext = default, WindowFlags? flags = default,
		bool? focusable = default, bool? fullscreen = default, int? height = default, bool? hidden = default, bool? highPixelDensity = default, bool? maximized = default, bool? menu = default, bool? metal = default,
		bool? minimized = default, bool? modal = default, bool? mouseGrabbed = default, bool? openGL = default, Window? parent = default, bool? resizable = default, string? title = default, bool? transparent = default,
		bool? tooltip = default, bool? utility = default, bool? vulkan = default, int? width = default, WindowPosition? x = default, WindowPosition? y = default, Properties? properties = default)
		where TDriver : notnull, IWindowingDriver
		=> TryCreate<Factory<TDriver>, Window<TDriver>>(
			out window, alwaysOnTop, bordered, constrainPopup, externalGraphicsContext, flags,
			focusable, fullscreen, height, hidden, highPixelDensity, maximized, menu, metal,
			minimized, modal, mouseGrabbed, openGL, parent, resizable, title, transparent,
			tooltip, utility, vulkan, width, x, y, properties
		);

	/// <summary>
	/// Tries to create a new <see cref="Window"/> with the specified title, size and flags, and an associated default <see cref="Rendering.Renderer"/>
	/// </summary>
	/// <param name="title">The title of the window. Depending on the platform and windowing backend, Unicode characters may be allowed in the window title.</param>
	/// <param name="width">The width of the window</param>
	/// <param name="height">The height of the window</param>
	/// <param name="window">The resulting <see cref="Window"/>, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <param name="renderer">The resulting default <see cref="Rendering.Renderer"/>, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <param name="flags">The flags the window should be created with</param>
	/// <returns><c><see langword="true"/></c> if the window and renderer were successfully created; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// You can also access the created <see cref="Rendering.Renderer"/> later using the <see cref="Renderer"/> property of the resulting <paramref name="window"/>.
	/// </para>
	/// <para>
	/// If a window is created with an associated <see cref="Rendering.Renderer"/>, don't try to create another renderer for the same window.
	/// </para>
	/// <para>
	/// You can see the <see cref="TryCreate(string, int, int, out Window?, WindowFlags)"/> method for more in-depth information about creating windows and the available flags.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryCreateWithRenderer(string title, int width, int height, [NotNullWhen(true)] out Window? window, [NotNullWhen(true)] out Renderer? renderer, WindowFlags flags = WindowFlags.None)
	{
		unsafe
		{
			var titleUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(title);
			try
			{
				SDL_Window* windowPtr;
				Renderer.SDL_Renderer* rendererPtr;

				if (!(bool)SDL_CreateWindowAndRenderer(titleUtf8, width, height, flags, &windowPtr, &rendererPtr)
					|| windowPtr is null || rendererPtr is null)
				{
					window = null;
					renderer = null;
					return false;
				}

				if (!Renderer.TryGetOrCreate(rendererPtr, out renderer))
				{
					SDL_DestroyWindow(windowPtr);

					window = null;
					renderer = null;
					return false;
				}

				window = RegisteredDriverOrGenericFallbackDriverFactory.Create(windowPtr, register: true);
				return true;
			}
			finally
			{
				Utf8StringMarshaller.Free(titleUtf8);
			}
		}
	}

	private protected abstract bool TryCreatePopupImpl(int x, int y, int width, int height, [NotNullWhen(true)] out Window? popupWindow, WindowFlags flags = default);

	/// <summary>
	/// Tries to create a new popup <see cref="Window"/> as a child of the window with the specified position, size and flags.
	/// </summary>
	/// <param name="x">The X coordinate of the popup window relative to the parent window</param>
	/// <param name="y">The Y coordinate of the popup window relative to the parent window</param>
	/// <param name="width">The width of the popup window</param>
	/// <param name="height">The height of the popup window</param>
	/// <param name="popupWindow">The resulting popup <see cref="Window"/>, if the method returns <see langword="true"/>; otherwise, <see langword="null"/></param>
	/// <param name="flags">The flags the window should be created with</param>
	/// <returns><c><see langword="true"/></c>, if the popup window was created successfully; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// The actual size of the created window may differ from the specified size, based on the desktop layout and window manager's policies. You should be prepared to handle windows of any size.
	/// </para>
	/// <para>
	/// The given <paramref name="flags"/> argument <em>must</em> contain at least one of the following flags:
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="WindowFlags.Tooltip"/></term>
	///			<description>The newly create popup window is a tooltip and will not pass any input events</description>
	///		</item>
	///		<item>
	///			<term><see cref="WindowFlags.PopupMenu"/></term>
	///			<description>The newly create popup window is a popup menu. The topmost popup menu will implicitly gain the keyboard focus</description>
	///		</item>
	/// </list>
	/// The <paramref name="flags"/> parameter defaults to <see cref="WindowFlags.Tooltip"/> if not specified.
	/// </para>
	/// <para>
	/// The following flags will be ignored if specified in the <paramref name="flags"/> argument:
	/// <list type="bullet">
	///		<item><description><see cref="WindowFlags.Minimized"/></description></item>
	///		<item><description><see cref="WindowFlags.Maximized"/></description></item>
	///		<item><description><see cref="WindowFlags.Fullscreen"/></description></item>
	///		<item><description><see cref="WindowFlags.Borderless"/></description></item>
	/// </list>
	/// </para>
	/// <para>
	/// The given <paramref name="flags"/> argument <em>must not</em> contain any of the following flags:
	/// <list type="bullet">
	///		<item><description><see cref="WindowFlags.Utility"/></description></item>
	///		<item><description><see cref="WindowFlags.Modal"/></description></item>
	/// </list>
	/// If the <paramref name="flags"/> argument contains any of the above flags, this method will fail.
	/// </para>
	/// <para>
	/// The parent of a popup window can be either a regular, toplevel window, or another popup window.
	/// </para>
	/// <para>
	/// Popup windows cannot be <see cref="TryMinimize">minimized</see>, be <see cref="TryMaximize">maximized</see>, be made <see cref="IsFullscreen">fullscreen</see>, <see cref="TryRaise">raised</see>, be made <see cref="TryFlash(FlashOperation)">flash</see>,
	/// be made a <see cref="IsModal">modal window</see>, be the <see cref="Parent">parent</see> of a toplevel window, or grab the <see cref="HasMouseGrab">mouse</see> and/or <see cref="HasKeyboardGrab">keyboard</see>.
	/// Attempts to do so will fail.
	/// </para>
	/// <para>
	/// Popup windows implicitly do not have borders or decorations and do not appear on the taskbar, dock, or in lists of windows such as <kbd>Alt</kbd>+<kbd>Tab</kbd>-menus.
	/// </para>
	/// <para>
	/// By default, popup window positions will automatically be constrained to keep the entire window within display bounds.
	/// If you want to create an unconstrained popup window, you must manually construct it using the
	/// <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)"/>
	/// method with the <c>constrainPopup</c> parameter set to <c><see langword="false"/></c>.
	/// </para>
	/// <para>
	/// By default, popup menus will automatically grab keyboard focus from the parent when shown. This behavior can be overridden by specifying the <see cref="WindowFlags.NotFocusable"/> flag,
	/// by constructing the popup menu using the
	/// <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)"/>
	/// method with the <c>focusable</c> parameter set to <c><see langword="false"/></c>,
	/// or by setting the <see cref="IsFocusable"/> property to <c><see langword="false"/></c> after creating the popup menu.
	/// </para>
	/// <para>
	/// If a parent window is hidden or disposed/destroyed, this will also be recursively applied to all child windows.
	/// </para>
	/// <para>
	/// Child windows that weren't explicitly hidden (e.g. via <see cref="TryHide"/>) but were only hidden because their parent was hidden,
	/// will be automatically shown again when their parent is shown again.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryCreatePopup(int x, int y, int width, int height, [NotNullWhen(true)] out Window? popupWindow, WindowFlags flags = WindowFlags.Tooltip)
		=> TryCreatePopupImpl(x, y, width, height, out popupWindow, flags);

	/// <summary>
	/// Tries to create a new <see cref="Renderer"/> for this window
	/// </summary>
	/// <param name="renderer">The resulting <see cref="Renderer"/>, if the method returns <see langword="true"/>; otherwise, <see langword="null"/></param>
	/// <param name="driverNames">An optional list of driver names to try, in order of preference. An empty list (the default) lets SDL automatically choose the best available driver for you.</param>
	/// <returns><c><see langword="true"/></c>, if the renderer was created successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// If you want to try specific rendering drivers, you can provide their names for the <paramref name="driverNames"/> parameter.
	/// You can use the <see cref="IRenderingDriver.Name"/> property of the pre-defined drivers for this (e.g. <see cref="OpenGL.Name"/>),
	/// or you can get the list of all available drivers at runtime using <see cref="IRenderingDriver.AvailableDriverNames"/> property.
	/// </para>
	/// <para>
	/// SDL will attempt to create a renderer using each of the specified driver names in order, and will return the first one that succeeds.
	/// </para>
	/// <para>
	/// Leaving the <paramref name="driverNames"/> parameter empty (the default) lets SDL automatically choose the best available driver for you,
	/// which is usually what you want unless you have specific requirements or want to test multiple drivers.
	/// </para>
	/// <para>
	/// The default renderering size of the resulting <paramref name="renderer"/> will match the size of the window in pixels,
	/// but you can change the content size and scaling later using the <see cref="Renderer.LogicalPresentation"/> property if needed.
	/// </para>
	/// <para>
	/// The resulting <paramref name="renderer"/> will be of the <see cref="Renderer{TDriver}"/> type with a specific <see cref="IRenderingDriver">rendering driver</see> as the type argument.
	/// If you need driver-specific functionality, you can type check and cast the resulting <paramref name="renderer"/> to the appropriate <see cref="Renderer{TDriver}"/> type later
	/// or you can use the <see cref="TryCreateRenderer{TDriver}(out Renderer{TDriver}?)"/> method alternatively.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryCreateRenderer([NotNullWhen(true)] out Renderer? renderer, params ReadOnlySpan<string> driverNames)
	{
		unsafe
		{
			byte* joinedNames = null;
			try
			{
				if (driverNames.Length is > 0)
				{
					// driver names will be joined into a single, ','-separated, null-terminated ASCII string on the stack

					var totalLength = 0;
					foreach (var name in driverNames)
					{
						totalLength += name.Length;
					}

					joinedNames = unchecked((byte*)Utilities.NativeMemory.Malloc(unchecked((nuint)(
						totalLength                // total number of characters
						+ (driverNames.Length - 1) // number of ',' separators
						+ 1                        // null-terminator
					))));

					if (joinedNames is null)
					{
						renderer = null;
						return false;
					}

					var index = 0;
					var joinedNamesPtr = joinedNames;
					while (true)
					{
						var name = driverNames[index];

						foreach (var ch in name)
						{
							if (ch is '\0')
							{
								break;
							}

							*joinedNamesPtr++ = ch is <= '\x7F'
								? unchecked((byte)ch)
								: (byte)'\xFF'; // replace non-ASCII characters with placeholder ('\xFF' is invalid in UTF-8 and therefore safe to use here)
						}

						if (!(++index < driverNames.Length))
						{
							break;
						}

						*joinedNamesPtr++ = (byte)',';
					}

					*joinedNamesPtr = (byte)'\0';
				}

				return Renderer.TryGetOrCreate(Renderer.SDL_CreateRenderer(mWindow, name: joinedNames), out renderer);
			}
			finally
			{
				Utilities.NativeMemory.Free(joinedNames);
			}
		}
	}

	private unsafe bool TryCreateRenderer<TDriver>([NotNullWhen(true)] out Renderer<TDriver>? renderer, byte* driverName)
		where TDriver : notnull, IRenderingDriver
	{
		var rendererPtr = Renderer.SDL_CreateRenderer(mWindow, name: driverName);

		if (rendererPtr is null)
		{
			renderer = null;
			return false;
		}

		renderer = new(rendererPtr, register: true);
		return true;
	}

	/// <summary>
	/// Tries to create a new <see cref="Renderer{TDriver}"/> for this window
	/// </summary>
	/// <typeparam name="TDriver">The rendering driver type associated with the resulting renderer</typeparam>
	/// <param name="renderer">The resulting <see cref="Renderer{TDriver}"/>, if the method returns <see langword="true"/>; otherwise, <see langword="null"/></param>
	/// <returns><c><see langword="true"/></c>, if the renderer was created successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The resulting <paramref name="renderer"/> will be of the <see cref="Renderer{TDriver}"/> type with the specified <typeparamref name="TDriver"/> as the type argument.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryCreateRenderer<TDriver>([NotNullWhen(true)] out Renderer<TDriver>? renderer)
		where TDriver : notnull, IRenderingDriver
	{
		unsafe
		{
			if (!TDriver.NameAscii.IsEmpty)
			{
				fixed (byte* driverName = TDriver.NameAscii)
				{
					return TryCreateRenderer(out renderer, driverName);
				}
			}
			else
			{
				return TryCreateRenderer(out renderer, driverName: null);
			}
		}
	}

	/// <summary>
	/// Tries to create a new <see cref="Renderer"/> for this window
	/// </summary>
	/// <param name="renderer">The resulting <see cref="Renderer"/>, if the method returns <see langword="true"/>; otherwise, <see langword="null"/></param>
	/// <param name="driverName">The name of the rendering driver to use, or <see langword="null"/> to let SDL automatically choose the best available driver for you</param>
	/// <param name="outputColorSpace">
	/// The color space to be used by renderer for presenting to the output display.
	/// The <see cref="Direct3D11">Direct3D 11</see>, <see cref="Direct3D12">Direct3D 12</see>, and <see cref="Metal">Metal</see> renderers support <see cref="ColorSpace.SrgbLinear"/>,
	/// which is a linear color space and supports HDR output. In that case, drawing still uses the sRGB color space, but individual values can go beyond <c>1.0</c>
	/// and floating point textures can be used for HDR content.
	/// If this parameter is <see langword="null"/> (the default), the output color space defaults to <see cref="ColorSpace.Srgb"/>.
	/// </param>
	/// <param name="presentVSync">
	/// The vertical synchronization (VSync) mode or interval to be used by the renderer.
	/// Can be specified to be <see cref="RendererVSync.Disabled"/> to disable VSync,
	/// <see cref="RendererVSync.Adaptive"/> to enable late swap tearing (adaptive VSync) if supported,
	/// or the result of the <see cref="RendererVSyncExtensions.Interval(int)"/> method to specify a custom VSync interval.
	/// You can specify a custom interval of <c>1</c> to synchronize to present of the renderer with <em>every</em> vertical refresh,
	/// <c>2</c> to synchronize it with <em>every second</em> vertical refresh, and so on.
	/// If this parameter is <see langword="null"/> (the default), the VSync mode defaults to <see cref="RendererVSync.Disabled"/>.
	/// </param>
	/// <param name="properties">Additional properties to use when creating the renderer</param>
	/// <returns><c><see langword="true"/></c>, if the renderer was created successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The resulting <paramref name="renderer"/> will be of the <see cref="Renderer{TDriver}"/> type with a specific <see cref="IRenderingDriver">rendering driver</see> as the type argument.
	/// If you need driver-specific functionality, you can type check and cast the resulting <paramref name="renderer"/> to the appropriate <see cref="Renderer{TDriver}"/> type later
	/// or you can use the <see cref="TryCreateRenderer{TDriver}(out Renderer{TDriver}?)"/> method alternatively.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryCreateRenderer([NotNullWhen(true)] out Renderer? renderer, string? driverName = default, ColorSpace? outputColorSpace = default, RendererVSync? presentVSync = default, Properties? properties = default)
	{
		unsafe
		{
			Properties propertiesUsed;
			Unsafe.SkipInit(out string? driverNameBackup);
			Unsafe.SkipInit(out IntPtr? windowBackup);
			Unsafe.SkipInit(out ColorSpace? outputColorSpaceBackup);
			Unsafe.SkipInit(out RendererVSync? presentVSyncBackup);

			if (properties is null)
			{
				propertiesUsed = [];

				if (driverName is not null)
				{
					propertiesUsed.TrySetStringValue(Renderer.PropertyNames.CreateNameString, driverName);
				}

				// setting SDL_PROP_RENDERER_CREATE_WINDOW_POINTER is required, except for when we want to create a software renderer,
				// and we'll handle software renderers separately via Renderer<Software>.TryCreateForSurface
				propertiesUsed.TrySetPointerValue(Renderer.PropertyNames.CreateWindowPointer, unchecked((IntPtr)mWindow));

				if (outputColorSpace is ColorSpace outputColorSpaceValue)
				{
					propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreateOutputColorSpaceNumber, unchecked((uint)outputColorSpaceValue));
				}

				if (presentVSync is RendererVSync presentVSyncValue)
				{
					propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreatePresentVSyncNumber, unchecked((int)presentVSyncValue));
				}
			}
			else
			{
				propertiesUsed = properties;

				if (driverName is not null)
				{
					driverNameBackup = propertiesUsed.TryGetStringValue(Renderer.PropertyNames.CreateNameString, out var existingDriverName)
						? existingDriverName
						: null;

					propertiesUsed.TrySetStringValue(Renderer.PropertyNames.CreateNameString, driverName);
				}

				windowBackup = propertiesUsed.TryGetPointerValue(Renderer.PropertyNames.CreateWindowPointer, out var existingWindowPtr)
					? existingWindowPtr
					: null;

				// setting SDL_PROP_RENDERER_CREATE_WINDOW_POINTER is required, except for when we want to create a software renderer,
				// and we'll handle software renderers separately via Renderer<Software>.TryCreateForSurface
				propertiesUsed.TrySetPointerValue(Renderer.PropertyNames.CreateWindowPointer, unchecked((IntPtr)mWindow));

				if (outputColorSpace is ColorSpace outputColorSpaceValue)
				{
					outputColorSpaceBackup = propertiesUsed.TryGetNumberValue(Renderer.PropertyNames.CreateOutputColorSpaceNumber, out var existingOutputColorSpaceValue)
						? unchecked((ColorSpace)existingOutputColorSpaceValue)
						: null;

					propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreateOutputColorSpaceNumber, unchecked((uint)outputColorSpaceValue));
				}

				if (presentVSync is RendererVSync presentVSyncValue)
				{
					presentVSyncBackup = propertiesUsed.TryGetNumberValue(Renderer.PropertyNames.CreatePresentVSyncNumber, out var existingPresentVSyncValue)
						? unchecked((RendererVSync)existingPresentVSyncValue)
						: null;

					propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreatePresentVSyncNumber, unchecked((int)presentVSyncValue));
				}
			}

			try
			{
				return Renderer.TryGetOrCreate(Renderer.SDL_CreateRendererWithProperties(propertiesUsed.Id), out renderer);
			}
			finally
			{
				if (properties is null)
				{
					// propertiesUsed was just a temporary instance we created for this call, so we need to dispose it now

					propertiesUsed.Dispose();
				}
				else
				{
					// we restored the original properties values from the given properties instance

					if (driverName is not null)
					{
						if (driverNameBackup is not null)
						{
							propertiesUsed.TrySetStringValue(Renderer.PropertyNames.CreateNameString, driverNameBackup);
						}
						else
						{
							propertiesUsed.TryRemove(Renderer.PropertyNames.CreateNameString);
						}
					}

					if (windowBackup is IntPtr windowPtr)
					{
						propertiesUsed.TrySetPointerValue(Renderer.PropertyNames.CreateWindowPointer, windowPtr);
					}
					else
					{
						propertiesUsed.TryRemove(Renderer.PropertyNames.CreateWindowPointer);
					}

					if (outputColorSpace.HasValue)
					{
						if (outputColorSpaceBackup is ColorSpace outputColorSpaceValue)
						{
							propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreateOutputColorSpaceNumber, unchecked((uint)outputColorSpaceValue));
						}
						else
						{
							propertiesUsed.TryRemove(Renderer.PropertyNames.CreateOutputColorSpaceNumber);
						}
					}

					if (presentVSync.HasValue)
					{
						if (presentVSyncBackup is RendererVSync presentVSyncValue)
						{
							propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreatePresentVSyncNumber, unchecked((int)presentVSyncValue));
						}
						else
						{
							propertiesUsed.TryRemove(Renderer.PropertyNames.CreatePresentVSyncNumber);
						}
					}
				}
			}
		}
	}

	// I know, I know, this isn't quite DRY..., but think about it: trying to abstract this into a shared method would require us to use some sort of aspect oriented programming.
	// We could achieve this with zero-cost abstractions by using static abstract interface members,
	// but than we would need to declare the interface, do the implementations by defining individual types for each one, and introduce a new generic parameter for the accepting method (and maybe more).
	// Or we could just duplicate the code, which is way simpler in this case.

	private unsafe bool TryCreateRenderer<TDriver>([NotNullWhen(true)] out Renderer<TDriver>? renderer, byte* driverName, ColorSpace? outputColorSpace = default, RendererVSync? presentVSync = default, Properties? properties = default)
		where TDriver : notnull, IRenderingDriver
	{
		unsafe
		{
			Properties propertiesUsed;
			Unsafe.SkipInit(out string? driverNameBackup);
			Unsafe.SkipInit(out IntPtr? windowBackup);
			Unsafe.SkipInit(out ColorSpace? outputColorSpaceBackup);
			Unsafe.SkipInit(out RendererVSync? presentVSyncBackup);

			if (properties is null)
			{
				propertiesUsed = [];

				if (driverName is not null)
				{
					propertiesUsed.TrySetNativeStringValue(Renderer.PropertyNames.CreateNameString, driverName);
				}

				// setting SDL_PROP_RENDERER_CREATE_WINDOW_POINTER is required, except for when we want to create a software renderer,
				// and we'll handle software renderers separately via Renderer<Software>.TryCreateForSurface
				propertiesUsed.TrySetPointerValue(Renderer.PropertyNames.CreateWindowPointer, unchecked((IntPtr)mWindow));

				if (outputColorSpace is ColorSpace outputColorSpaceValue)
				{
					propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreateOutputColorSpaceNumber, unchecked((uint)outputColorSpaceValue));
				}

				if (presentVSync is RendererVSync presentVSyncValue)
				{
					propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreatePresentVSyncNumber, unchecked((int)presentVSyncValue));
				}
			}
			else
			{
				propertiesUsed = properties;

				driverNameBackup = propertiesUsed.TryGetStringValue(Renderer.PropertyNames.CreateNameString, out var existingDriverName)
					? existingDriverName
					: null;

				// definitely overwrite existing driver name, even if null
				if (driverName is not null)
				{
					propertiesUsed.TrySetNativeStringValue(Renderer.PropertyNames.CreateNameString, driverName);
				}
				else
				{
					propertiesUsed.TryRemove(Renderer.PropertyNames.CreateNameString);
				}

				windowBackup = propertiesUsed.TryGetPointerValue(Renderer.PropertyNames.CreateWindowPointer, out var existingWindowPtr)
					? existingWindowPtr
					: null;

				// setting SDL_PROP_RENDERER_CREATE_WINDOW_POINTER is required, except for when we want to create a software renderer,
				// and we'll handle software renderers separately via Renderer<Software>.TryCreateForSurface
				propertiesUsed.TrySetPointerValue(Renderer.PropertyNames.CreateWindowPointer, unchecked((IntPtr)mWindow));

				if (outputColorSpace is ColorSpace outputColorSpaceValue)
				{
					outputColorSpaceBackup = propertiesUsed.TryGetNumberValue(Renderer.PropertyNames.CreateOutputColorSpaceNumber, out var existingOutputColorSpaceValue)
						? unchecked((ColorSpace)existingOutputColorSpaceValue)
						: null;

					propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreateOutputColorSpaceNumber, unchecked((uint)outputColorSpaceValue));
				}

				if (presentVSync is RendererVSync presentVSyncValue)
				{
					presentVSyncBackup = propertiesUsed.TryGetNumberValue(Renderer.PropertyNames.CreatePresentVSyncNumber, out var existingPresentVSyncValue)
						? unchecked((RendererVSync)existingPresentVSyncValue)
						: null;

					propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreatePresentVSyncNumber, unchecked((int)presentVSyncValue));
				}
			}

			try
			{
				var rendererPtr = Renderer.SDL_CreateRendererWithProperties(propertiesUsed.Id);

				if (rendererPtr is null)
				{
					renderer = null;
					return false;
				}

				renderer = new(rendererPtr, register: true);
				return true;
			}
			finally
			{
				if (properties is null)
				{
					// propertiesUsed was just a temporary instance we created for this call, so we need to dispose it now

					propertiesUsed.Dispose();
				}
				else
				{
					// we restored the original properties values from the given properties instance

					if (driverNameBackup is not null)
					{
						propertiesUsed.TrySetStringValue(Renderer.PropertyNames.CreateNameString, driverNameBackup);
					}
					else
					{
						propertiesUsed.TryRemove(Renderer.PropertyNames.CreateNameString);
					}

					if (windowBackup is IntPtr windowPtr)
					{
						propertiesUsed.TrySetPointerValue(Renderer.PropertyNames.CreateWindowPointer, windowPtr);
					}
					else
					{
						propertiesUsed.TryRemove(Renderer.PropertyNames.CreateWindowPointer);
					}

					if (outputColorSpace.HasValue)
					{
						if (outputColorSpaceBackup is ColorSpace outputColorSpaceValue)
						{
							propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreateOutputColorSpaceNumber, unchecked((uint)outputColorSpaceValue));
						}
						else
						{
							propertiesUsed.TryRemove(Renderer.PropertyNames.CreateOutputColorSpaceNumber);
						}
					}

					if (presentVSync.HasValue)
					{
						if (presentVSyncBackup is RendererVSync presentVSyncValue)
						{
							propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreatePresentVSyncNumber, unchecked((int)presentVSyncValue));
						}
						else
						{
							propertiesUsed.TryRemove(Renderer.PropertyNames.CreatePresentVSyncNumber);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Tries to create a new <see cref="Renderer{TDriver}"/> for this window
	/// </summary>
	/// <typeparam name="TDriver">The rendering driver type associated with the resulting renderer</typeparam>
	/// <param name="renderer">The resulting <see cref="Renderer{TDriver}"/>, if the method returns <see langword="true"/>; otherwise, <see langword="null"/></param>
	/// <param name="outputColorSpace">
	/// The color space to be used by renderer for presenting to the output display.
	/// The <see cref="Direct3D11">Direct3D 11</see>, <see cref="Direct3D12">Direct3D 12</see>, and <see cref="Metal">Metal</see> renderers support <see cref="ColorSpace.SrgbLinear"/>,
	/// which is a linear color space and supports HDR output. In that case, drawing still uses the sRGB color space, but individual values can go beyond <c>1.0</c>
	/// and floating point textures can be used for HDR content.
	/// If this parameter is <see langword="null"/> (the default), the output color space defaults to <see cref="ColorSpace.Srgb"/>.
	/// </param>
	/// <param name="presentVSync">
	/// The vertical synchronization (VSync) mode or interval to be used by the renderer.
	/// Can be specified to be <see cref="RendererVSync.Disabled"/> to disable VSync,
	/// <see cref="RendererVSync.Adaptive"/> to enable late swap tearing (adaptive VSync) if supported,
	/// or the result of the <see cref="RendererVSyncExtensions.Interval(int)"/> method to specify a custom VSync interval.
	/// You can specify a custom interval of <c>1</c> to synchronize to present of the renderer with <em>every</em> vertical refresh,
	/// <c>2</c> to synchronize it with <em>every second</em> vertical refresh, and so on.
	/// If this parameter is <see langword="null"/> (the default), the VSync mode defaults to <see cref="RendererVSync.Disabled"/>.
	/// </param>
	/// <param name="properties">Additional properties to use when creating the renderer</param>
	/// <returns><c><see langword="true"/></c>, if the renderer was created successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The resulting <paramref name="renderer"/> will be of the <see cref="Renderer{TDriver}"/> type with the specified <typeparamref name="TDriver"/> as the type argument.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryCreateRenderer<TDriver>([NotNullWhen(true)] out Renderer<TDriver>? renderer, ColorSpace? outputColorSpace = default, RendererVSync? presentVSync = default, Properties? properties = default)
		where TDriver : notnull, IRenderingDriver
	{
		unsafe
		{
			if (!TDriver.NameAscii.IsEmpty)
			{
				fixed (byte* driverName = TDriver.NameAscii)
				{
					return TryCreateRenderer(out renderer, driverName, outputColorSpace, presentVSync, properties);
				}

			}
			else
			{
				return TryCreateRenderer(out renderer, driverName: null, outputColorSpace, presentVSync, properties);
			}
		}
	}

	/// <summary>
	/// Tries to make the window flash in order to demand the user's attention
	/// </summary>
	/// <param name="operation">The flash operation to perform</param>
	/// <returns><c><see langword="true"/></c>, if the flash operation was successfully initiated or canceled; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryFlash(FlashOperation operation)
	{
		unsafe
		{
			return SDL_FlashWindow(mWindow, operation);
		}
	}

	/// <summary>
	/// Tries to get an existing <see cref="Window"/> by its numeric ID
	/// </summary>
	/// <param name="id">The numeric ID of the window</param>
	/// <param name="result">The existing <see cref="Window"/> associated with the specified <paramref name="id"/>, if the method returns <c><see langword="true"/></c>; otherwise, <see langword="null"/></param>
	/// <returns><c><see langword="true"/></c>, if a window with the specified <paramref name="id"/> exists; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// This method makes use of the ID that SDL uses to identify windows in <see cref="WindowEvent"/>s and some other events
	/// and maps such events to their corresponding <see cref="Window"/> instances using their <see cref="Id"/>.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryGetFromId(uint id, [NotNullWhen(true)] out Window? result)
	{
		unsafe
		{
			return TryGetOrCreate(SDL_GetWindowFromID(id), out result);
		}
	}

	/// <summary>
	/// Tries to get the raw ICC profile data associated with the screen the window is currently displayed on
	/// </summary>
	/// <param name="iccProfileData">The raw ICC profile data associated with the screen the window is currently displayed on, if the method returns <c><see langword="true"/></c>; otherwise, <see langword="null"/></param>
	/// <returns><c><see langword="true"/></c>, if the ICC profile data was successfully retrieved; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Please remember to <see cref="NativeMemoryManagerBase.Dispose()">dispose</see> the resulting <paramref name="iccProfileData"/> when you don't need it anymore to release the underlying unmanaged resources.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryGetICCProfileData([NotNullWhen(true)] out NativeMemoryManager? iccProfileData)
	{
		unsafe
		{
			if (mWindow is null)
			{
				// For some reason SDL_GetWindowICCProfile isn't null-safe, so we have to check for null ourselves

				iccProfileData = null;
				return false;
			}

			Unsafe.SkipInit(out nuint size);

			var iccPtr = SDL_GetWindowICCProfile(mWindow, &size);

			if (iccPtr is null)
			{
				// SDL_GetWindowICCProfile returns null if there was an error, at least that's what the documentation says
				// I don't know if the various implementations of SDL_GetWindowICCProfile for different windowing backends
				// actually return something non-null when a window doesn't have an ICC profile.
				// But this doesn't quite matter, as the user is encouraged to check Error.TryGet when this method returns false anyway.

				iccProfileData = null;
				return false;
			}

			iccProfileData = new(iccPtr, size, &Utilities.NativeMemory.SDL_free);
			return true;
		}
	}

	internal unsafe static bool TryGetOrCreate(SDL_Window* window, [NotNullWhen(true)] out Window? result)
	{
		if (window is null)
		{
			result = null;
			return false;
		}

		var windowRef = mKnownInstances.GetOrAdd(unchecked((IntPtr)window), createRef);

		if (!windowRef.TryGetTarget(out result))
		{
			windowRef.SetTarget(result = create(window));
		}

		return true;

		static WeakReference<Window> createRef(IntPtr window) => new(create(unchecked((SDL_Window*)window)));

		static Window create(SDL_Window* window)
		{
			if (!TryCreateFromRegisteredDriver(window, register: false, out var result))
			{
				result = new Window<GenericFallbackWindowingDriver>(window, register: false);
			}

			return result;
		}
	}

	internal unsafe static bool TryGetOrCreate<TDriver>(SDL_Window* window, [NotNullWhen(true)] out Window<TDriver>? result)
		where TDriver : notnull, IWindowingDriver
	{
		if (window is null)
		{
			result = null;
			return false;
		}

		var windowRef = mKnownInstances.GetOrAdd(unchecked((IntPtr)window), createRef);

		if (!windowRef.TryGetTarget(out var baseResult))
		{
			windowRef.SetTarget(result = create(window));
		}
		else if (baseResult is Window<TDriver> typedResult)
		{
			// that's just fine

			result = typedResult;
		}
		else if (baseResult.Pointer is not null)
		{
			// this also means that baseResult.Pointer == texture
			// this indicates that we actually need the window to be of a different managed type than it currently is,
			// and we can't just recreate windows (like we do for displays),
			// therefore we should just fail in that case

			result = null;
			return false;
		}
		else
		{
			// this indicates that we somehow managed to not properly forget a managed instance that was disposed,
			// so we need to fully recreate the managed instance with the new type here

			result = create(window);
		}

		return true;

		static WeakReference<Window> createRef(IntPtr window) => new(create(unchecked((SDL_Window*)window)));

		static Window<TDriver> create(SDL_Window* window)
		{
			return new(window, register: false);
		}
	}

	/// <summary>
	/// Tries to hide the window
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if the window was successfully hidden; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// To show a hidden window again, you can use the <see cref="TryShow"/> method.
	/// </para>
	/// <para>
	/// This method recursively hides all child windows of this window as well.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryHide()
	{
		unsafe
		{
			return SDL_HideWindow(mWindow);
		}
	}

	/// <summary>
	/// Tries to maximize the window (e.g. make it as large as possible)
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if the window was successfully maximized; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Non-resizable windows can't be maximized. The window must have the <see cref="WindowFlags.Resizable"/> flag set, or a call to this method will have no effect.
	/// </para>
	/// <para>
	/// On some windowing backends, a request to maximize a window is asynchronous and the new window state may not have have been applied immediately after calling this method.
	/// If an immediate change is required, you can call <see cref="TrySync"/> to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window state changes, an <see cref="EventType.WindowMaximized"/> event will be emitted.
	/// Windowing backends can also deny the request to change this setting altogether.
	/// </para>
	/// <para>
	/// When maximizing a window, whether the constraints set via <see cref="MaximumSize"/> are honored depends on the policy of the window manager.
	/// On Windows and macOS, the constraints are enforced when maximizing, while X11 and Wayland window managers may vary.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryMaximize()
	{
		unsafe
		{
			return SDL_MaximizeWindow(mWindow);
		}
	}

	/// <summary>
	/// Tries to minimize the window (e.g. bring it to an iconic representation)
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if the window was successfully minimized; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// If the window is in fullscreen mode, a call to this method has no direct effect, but it may alter the state the window is returned to when leaving fullscreen.
	/// </para>
	/// <para>
	/// On some windowing backends, a request to minimize a window is asynchronous and the new window state may not have have been applied immediately after calling this method.
	/// If an immediate change is required, you can call <see cref="TrySync"/> to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window state changes, an <see cref="EventType.WindowMinimized"/> event will be emitted.
	/// Windowing backends can also deny the request to change this setting altogether.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryMinimize()
	{
		unsafe
		{
			return SDL_MinimizeWindow(mWindow);
		}
	}

	/// <summary>
	/// Tries to raise the window above other windows and to let it gain the input focus
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if the window was successfully raised; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The result of a request to raise a window is subject to desktop window manager policy, particularly if raising the requested window would result in stealing focus from another application.
	/// If the window is successfully raised and gains input focus, an <see cref="EventType.WindowFocusGained"/> event will be emitted, and the window will have the <see cref="WindowFlags.InputFocus"/> flag set.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRaise()
	{
		unsafe
		{
			return SDL_RaiseWindow(mWindow);
		}
	}

	/// <summary>
	/// Tries to restore the size and position of the previously minimized or maximized window
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if the window was successfully restored; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// If the window is in fullscreen mode, a call to this method has no direct effect, but it may alter the state the window is returned to when leaving fullscreen.
	/// </para>
	/// <para>
	/// On some windowing backends, a request to restore a window is asynchronous and the new window state may not have have been applied immediately after calling this method.
	/// If an immediate change is required, you can call <see cref="TrySync"/> to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window state changes, an <see cref="EventType.WindowRestored"/> event will be emitted.
	/// Windowing backends can also deny the request to change this setting altogether.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRestore()
	{
		unsafe
		{
			return SDL_RestoreWindow(mWindow);
		}
	}

	/// <summary>
	/// Tries to set the icon for the window
	/// </summary>
	/// <param name="icon">The icon to set for the window</param>
	/// <returns><c><see langword="true"/></c>, if the icon was successfully set; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// If the given <paramref name="icon"/> is a surface with <see cref="Surface.Images">alternate representations</see>, the surface will be interpreted as the content to be used for 100% display scale, and the alternate representations will be used for high DPI situations.
	/// For example, if the original surface is <c>32</c> by <c>32</c>, then on a 2x macOS display or 200% display scale on Windows, a <c>64</c> by <c>64</c> version of the image will be used, if available.
	/// </para>
	/// <para>
	/// If a matching version of the image isn't available, the closest larger size image will be downscaled to the appropriate size and be used instead, if available.
	/// Otherwise, the closest smaller image will be upscaled and be used instead.
	/// </para>
	/// <para>
	/// The given <paramref name="icon"/> as well as all of its <see cref="Surface.Images">alternate representations</see> will be copied, so you can safely dispose of it after passing it if you don't need it anymore.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TrySetIcon(Surface icon)
	{
		unsafe
		{
			return SDL_SetWindowIcon(mWindow, icon is not null ? icon.Pointer : null);
		}
	}

	/// <summary>
	/// Tries to set the position of the window
	/// </summary>
	/// <param name="x">
	/// The new X coordinate of the window, <see cref="WindowPosition.Centered"/>, <see cref="WindowPosition.Undefined"/>, or a value obtained from using the <see cref="WindowPosition.CenteredOn(Display)"/> or <see cref="WindowPosition.UndefinedOn(Display)"/> methods.
	/// </param>
	/// <param name="y">
	/// The new Y coordinate of the window, <see cref="WindowPosition.Centered"/>, <see cref="WindowPosition.Undefined"/>, or a value obtained from using the <see cref="WindowPosition.CenteredOn(Display)"/> or <see cref="WindowPosition.UndefinedOn(Display)"/> methods.
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the position was successfully set; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// You can either specify definitive coordinates for the new window position,
	/// or you can use <see cref="WindowPosition.Centered"/>, <see cref="WindowPosition.Undefined"/>, <see cref="WindowPosition.CenteredOn(Display)"/>, or <see cref="WindowPosition.UndefinedOn(Display)"/>
	/// to specify some special window positions to be determined in relation to the primary display or a specific display.
	/// </para>
	/// <para>
	/// If the window is in exclusive fullscreen mode or maximized state, a call to this method has no effect.
	/// </para>
	/// <para>
	/// However, this method can be used to reposition fullscreen-desktop windows onto a different display.
	/// Whereas exclusive fullscreen windows are locked to a specific display, they can only be repositioned programmatically by setting <see cref="FullscreenMode"/>.
	/// </para>
	/// <para>
	/// On some windowing backends, a request to change the position of a window is asynchronous and the new window position may not have have been applied immediately after calling this method.
	/// If an immediate change is required, you can call <see cref="TrySync"/> to block until the changes have taken effect.
	/// </para>
	/// <para>
	/// When the window position changes, an <see cref="EventType.WindowMoved"/> event will be emitted with the new window position.
	/// Note that the new position may not match the exact coordinates requested, as some windowing backends can restrict the window position in certain scenarios
	/// (e.g. constraining the position so the window is always within desktop bounds).
	/// Additionally, windowing backends can also deny the request to change this setting altogether.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TrySetPosition(WindowPosition x, WindowPosition y)
	{
		unsafe
		{
			return SDL_SetWindowPosition(mWindow, Unsafe.BitCast<WindowPosition, int>(x), Unsafe.BitCast<WindowPosition, int>(y));
		}
	}

	/// <summary>
	/// Tries to show the previously hidden window
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if the window was successfully shown; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method recursively shows all child windows of this window that weren't explicitly hidden (e.g. via <see cref="TryHide"/>) but were only hidden because their parent was hidden.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryShow()
	{
		unsafe
		{
			return SDL_ShowWindow(mWindow);
		}
	}

	/// <summary>
	/// Tries to show the system-level menu for the window
	/// </summary>
	/// <param name="x">The X coordinate of the menu to show, relative to the top-left corner of the window's client area</param>
	/// <param name="y">The Y coordinate of the menu to show, relative to the top-left corner of the window's client area</param>
	/// <returns><c><see langword="true"/></c>, if the system-level menu was successfully shown; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This default window menu is provided by the system and on some platforms provides functionality for setting or changing privileged state on the window, such as moving it between workspaces or displays, or toggling the always-on-top property.
	/// </para>
	/// <para>
	/// On platforms or in environments where showing a system-level menu isn't supported, this method does nothing.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryShowSystemMenu(int x, int y)
	{
		unsafe
		{
			return SDL_ShowWindowSystemMenu(mWindow, x, y);
		}
	}

	/// <summary>
	/// Tries to block until any pending changes to the window state have been applied
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if the window state was successfully fully synchronized before timing out; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// On asynchronous windowing backends, this method acts as a synchronization barrier for pending window states.
	/// A call to this method will attempt to wait until any pending window state has been applied and is guaranteed to return within finite time.
	/// Note that for how long it can potentially block depends on the underlying window backend,
	/// as window state changes sometimes may involve somewhat lengthy animations that must complete before the window is in its final requested state.
	/// </para>
	/// <para>
	/// On windowing backends where changes are immediate, this does nothing.
	/// </para>
	/// </remarks>
	public bool TrySync()
	{
		unsafe
		{
			return SDL_SyncWindow(mWindow);
		}
	}
}
