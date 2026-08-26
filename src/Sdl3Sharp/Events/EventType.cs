using Sdl3Sharp.Events;
using Sdl3Sharp.Input;
using Sdl3Sharp.Video.Rendering;
using Sdl3Sharp.Video.Windowing;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event type for an event structure
/// </summary>
public enum EventType : uint
{
	/// <summary>SDL_EVENT_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	First = 0,

	#region Application events

	/// <summary>The event type <em>Quit</em></summary>
	/// <remarks>
	/// <para>
	/// User-requested quit
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="QuitEvent"/>.
	/// </para>
	/// </remarks>
	Quit = 0x100,

	/// <summary>The event type <em>Terminating</em></summary>
	/// <remarks>
	/// <para>
	/// The application is being terminated by the OS. This event must be handled in an <see cref="EventWatch"/> registered with <see cref="EventQueue.EventWatch"/>.
	/// Called on iOS in <c>applicationWillTerminate()</c>.
	/// Called on Android in <c>onDestroy()</c>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CommonEvent"/>.
	/// </para>
	/// </remarks>
	Terminating,

	/// <summary>The event type <em>LowMemory</em></summary>
	/// <remarks>
	/// <para>
	/// The application is low on memory, free memory if possible. This event must be handled in an <see cref="EventWatch"/> registered with <see cref="EventQueue.EventWatch"/>.
	/// Called on iOS in <c>applicationDidReceiveMemoryWarning</c>().
	/// Called on Android in <c>onTrimMemory</c>().
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CommonEvent"/>.
	/// </para>
	/// </remarks>
	LowMemory,

	/// <summary>The event type <em>WillEnterBackground</em></summary>
	/// <remarks>
	/// <para>
	/// The application is about to enter the background. This event must be handled in an <see cref="EventWatch"/> registered with <see cref="EventQueue.EventWatch"/>.
	/// Called on iOS in <c>applicationWillResignActive()</c>.
	/// Called on Android in <c>onPause()</c>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CommonEvent"/>.
	/// </para>
	/// </remarks>
	WillEnterBackground,

	/// <summary>The event type <em>DidEnterBackground</em></summary>
	/// <remarks>
	/// <para>
	/// The application did enter the background and may not get CPU for some time. This event must be handled in an <see cref="EventWatch"/> registered with <see cref="EventQueue.EventWatch"/>.
	/// Called on iOS in <c>applicationDidEnterBackground()</c>.
	/// Called on Android in <c>onPause()</c>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CommonEvent"/>.
	/// </para>
	/// </remarks>
	DidEnterBackground,

	/// <summary>The event type <em>WillEnterForeground</em></summary>
	/// <remarks>
	/// <para>
	/// The application is about to enter the foreground. This event must be handled in an <see cref="EventWatch"/> registered with <see cref="EventQueue.EventWatch"/>.
	/// Called on iOS in <c>applicationWillEnterForeground()</c>.
	/// Called on Android in <c>onResume()</c>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CommonEvent"/>.
	/// </para>
	/// </remarks>
	WillEnterForeground,

	/// <summary>The event type <em>DidEnterForeground</em></summary>
	/// <remarks>
	/// <para>
	/// The application is now interactive. This event must be handled in an <see cref="EventWatch"/> registered with <see cref="EventQueue.EventWatch"/>.
	/// Called on iOS in <c>applicationDidBecomeActive()</c>.
	/// Called on Android in <c>onResume()</c>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CommonEvent"/>.
	/// </para>
	/// </remarks>
	DidEnterForeground,

	/// <summary>The event type <em>LocaleChanged</em></summary>
	/// <remarks>
	/// <para>
	/// The user's locale preferences have changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CommonEvent"/>.
	/// </para>
	/// </remarks>
	LocaleChanged,

	/// <summary>The event type <em>SystemThemeChanged</em></summary>
	/// <remarks>
	/// <para>
	/// The system theme changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CommonEvent"/>.
	/// </para>
	/// </remarks>
	SystemThemeChanged,

	/// <summary></summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	ApplicationFirst = Terminating,

	/// <summary></summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	ApplicationLast = SystemThemeChanged,

	#endregion

	#region Display events

	/// <summary>The event type <em>DisplayOrientation</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Display"/>'s orientation has changed to <see cref="DisplayEvent.Data1"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayOrientation = 0x151,

	/// <summary>The event type <em>DisplayAdded</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Display"/> has been added to the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayAdded,

	/// <summary>The event type <em>DisplayRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Display"/> has been removed from the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayRemoved,

	/// <summary>The event type <em>DisplayMoved</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Display"/> has changed position.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayMoved,

	/// <summary>The event type <em>DisplayDesktopModeChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Display"/> has changed desktop mode.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayDesktopModeChanged,

	/// <summary>The event type <em>DisplayCurrentModeChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Display"/> has changed current mode.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayCurrentModeChanged,

	/// <summary>The event type <em>DisplayContentScaleChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Display"/> has changed content scale.
	/// </para> 
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayContentScaleChanged,

	/// <summary>The event type <em>DisplayUsableBoundsChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Display"/> has changed usable bounds.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayUsableBoundsChanged,

	/// <summary>SDL_EVENT_DISPLAY_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	DisplayFirst = DisplayOrientation,

	/// <summary>SDL_EVENT_DISPLAY_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	DisplayLast = DisplayUsableBoundsChanged,

	#endregion

	#region Window events

	/// <summary>The event type <em>WindowShown</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has been shown.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowShown = 0x202,

	/// <summary>The event type <em>WindowHidden</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has been hidden.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowHidden,

	/// <summary>The event type <em>WindowExposed</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has been exposed and should be redrawn, and can be redrawn directly from event watchers/handlers for this event.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowExposed,

	/// <summary>The event type <em>WindowMoved</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has been moved to <see cref="WindowEvent.Data1"/>, <see cref="WindowEvent.Data2"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowMoved,

	/// <summary>The event type <em>WindowResized</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has been resized to <see cref="WindowEvent.Data1"/>×<see cref="WindowEvent.Data2"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowResized,

	/// <summary>The event type <em>WindowPixelSizeChanged</em></summary>
	/// <remarks>
	/// <para>
	/// The pixel size of a <see cref="Window"/> has changed to <see cref="WindowEvent.Data1"/>×<see cref="WindowEvent.Data2"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowPixelSizeChanged,

	/// <summary>The event type <em>WindowMetalViewResized</em></summary>
	/// <remarks>
	/// <para>
	/// The pixel size of a Metal view associated with a <see cref="Window"/> has changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowMetalViewResized,

	/// <summary>The event type <em>WindowMinimized</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has been minimized.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowMinimized,

	/// <summary>The event type <em>WindowMaximized</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has been maximized.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowMaximized,

	/// <summary>The event type <em>WindowRestored</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has been restored to normal size and position.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowRestored,

	/// <summary>The event type <em>WindowMouseEnter</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has gained mouse focus.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowMouseEnter,

	/// <summary>The event type <em>WindowMouseLeave</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has lost mouse focus.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowMouseLeave,

	/// <summary>The event type <em>WindowFocusGained</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has gained keyboard focus.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowFocusGained,

	/// <summary>The event type <em>WindowFocusLost</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has lost keyboard focus.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowFocusLost,

	/// <summary>The event type <em>WindowCloseRequested</em></summary>
	/// <remarks>
	/// <para>
	/// The window manager requests that a <see cref="Window"/> should be closed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowCloseRequested,

	/// <summary>The event type <em>WindowHitTest</em></summary>
	/// <remarks>
	/// <para>
	/// A window had a <see cref="Window.HitTest">hit test</see> that wasn't <see cref="HitTestResult.Normal"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowHitTest,

	/// <summary>The event type <em>WindowIccProfileChanged</em></summary>
	/// <remarks>
	/// <para>
	/// The <see cref="Window.TryGetICCProfileData(out Utilities.NativeMemoryManager?)">ICC profile</see> of a <see cref="Window"/> has changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowIccProfileChanged,

	/// <summary>The event type <em>WindowDisplayChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has been moved to display <see cref="WindowEvent.Data1"/> (the ID of the <see cref="Display"/>).
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowDisplayChanged,

	/// <summary>The event type <em>WindowDisplayScaleChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/>'s display scale has been changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowDisplayScaleChanged,

	/// <summary>The event type <em>WindowSafeAreaChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/>'s safe area has been changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowSafeAreaChanged,

	/// <summary>The event type <em>WindowOccluded</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has been occluded.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowOccluded,

	/// <summary>The event type <em>WindowEnterFullscreen</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has entered fullscreen mode.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowEnterFullscreen,

	/// <summary>The event type <em>WindowLeaveFullscreen</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> has left fullscreen mode.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowLeaveFullscreen,

	/// <summary>The event type <em>WindowDestroyed</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/> with the associated <see cref="WindowEvent.WindowId"/> is being or has been destroyed.
	/// If this message is being handled in an <see cref="EventWatch"/>, the window handle is still valid and can still be used to retrieve any properties associated with the window.
	/// Otherwise, the handle has already been destroyed and all resources associated with it are invalid.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowDestroyed,

	/// <summary>The event type <em>WindowHdrStateChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/>'s HDR properties have changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowHdrStateChanged,

#if SDL3_6_0_OR_GREATER

	/// <summary>The event type <em>WindowSettingsChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Window"/>'s settings have changed (on visionOS).
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowSettingsChanged,

#endif

	/// <summary>SDL_EVENT_WINDOW_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	WindowFirst = WindowShown,

	/// <summary>SDL_EVENT_WINDOW_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	WindowLast =
#if SDL3_6_0_OR_GREATER
		WindowSettingsChanged,
#else
		WindowHdrStateChanged,
#endif

	#endregion

	#region Keyboard events

	/// <summary>The event type <em>KeyboardKeyDown</em></summary>
	/// <remarks>
	/// <para>
	/// Key pressed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="KeyboardEvent"/>.
	/// </para>
	/// </remarks>
	KeyDown = 0x300,

	/// <summary>The event type <em>KeyboardKeyUp</em></summary>
	/// <remarks>
	/// <para>
	/// Key released.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="KeyboardEvent"/>.
	/// </para>
	/// </remarks>
	KeyUp,

	/// <summary>The event type <em>KeyboardTextEditing</em></summary>
	/// <remarks>
	/// <para>
	/// Keyboard text editing (composition).
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="TextEditingEvent"/>.
	/// </para>
	/// </remarks>
	TextEditing,

	/// <summary>The event type <em>KeyboardTextInput</em></summary>
	/// <remarks>
	/// <para>
	/// Keyboard text input.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="TextInputEvent"/>.
	/// </para>
	/// </remarks>
	TextInput,

	/// <summary>The event type <em>KeyboardKeymapChanged</em></summary>
	/// <remarks>
	/// <para>
	/// Keymap changed due to a system event such as an input language or keyboard layout change.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CommonEvent"/>.
	/// </para>
	/// </remarks>
	KeymapChanged,

	/// <summary>The event type <em>KeyboardAdded</em></summary>
	/// <remarks>
	/// <para>
	/// A new <see cref="Keyboard"/> has been inserted into the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="KeyboardDeviceEvent"/>.
	/// </para>
	/// </remarks>
	KeyboardAdded,

	/// <summary>The event type <em>KeyboardRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Keyboard"/> has been removed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="KeyboardDeviceEvent"/>.
	/// </para>
	/// </remarks>
	KeyboardRemoved,

	/// <summary>The event type <em>KeyboardTextEditingCandidates</em></summary>
	/// <remarks>
	/// <para>
	/// Keyboard text editing candidates.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="TextEditingCandidatesEvent"/>.
	/// </para>
	/// </remarks>
	TextEditingCandidates,

	/// <summary>The event type <em>ScreenKeyboardShown</em></summary>
	/// <remarks>
	/// <para>
	/// The on-screen keyboard has been shown.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CommonEvent"/>.
	/// </para>
	/// </remarks>
	ScreenKeyboardShown,

	/// <summary>The event type <em>ScreenKeyboardHidden</em></summary>
	/// <remarks>
	/// <para>
	/// The on-screen keyboard has been hidden.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CommonEvent"/>.
	/// </para>
	/// </remarks>
	ScreenKeyboardHidden,

	/// <summary>SDL_EVENT_KEYBOARD_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	KeyboardFirst = KeyDown,

	/// <summary>SDL_EVENT_KEYBOARD_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	KeyboardLast = ScreenKeyboardHidden,

	#endregion

	#region Mouse events

	/// <summary>The event type <em>MouseMotion</em></summary>
	/// <remarks>
	/// <para>
	/// Mouse moved.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="MouseMotionEvent"/>.
	/// </para>
	/// </remarks>
	MouseMotion = 0x400,

	/// <summary>The event type <em>MouseButtonDown</em></summary>
	/// <remarks>
	/// <para>
	/// Mouse button pressed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="MouseButtonEvent"/>.
	/// </para>
	/// </remarks>
	MouseButtonDown,

	/// <summary>The event type <em>MouseButtonUp</em></summary>
	/// <remarks>
	/// <para>
	/// Mouse button released.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="MouseButtonEvent"/>.
	/// </para>
	/// </remarks>
	MouseButtonUp,

	/// <summary>The event type <em>MouseWheelMotion</em></summary>
	/// <remarks>
	/// <para>
	/// Mouse wheel motion.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="MouseWheelEvent"/>.
	/// </para>
	/// </remarks>
	MouseWheel,

	/// <summary>The event type <em>MouseAdded</em></summary>
	/// <remarks>
	/// <para>
	/// A new <see cref="Mouse"/> has been inserted into the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="MouseDeviceEvent"/>.
	/// </para>
	/// </remarks>
	MouseAdded,

	/// <summary>The event type <em>MouseRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Mouse"/> has been removed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="MouseDeviceEvent"/>.
	/// </para>
	/// </remarks>
	MouseRemoved,

	/// <summary>SDL_EVENT_MOUSE_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	MouseFirst = MouseMotion,

	/// <summary>SDL_EVENT_MOUSE_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	MouseLast = MouseRemoved,

	#endregion

	#region Joystick events

	/// <summary>The event type <em>JoystickAxisMotion</em></summary>
	/// <remarks>
	/// <para>
	/// Joystick axis motion.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="JoyAxisEvent"/>.
	/// </para>
	/// </remarks>
	JoystickAxisMotion = 0x600,

	/// <summary>The event type <em>JoystickBallMotion</em></summary>
	/// <remarks>
	/// <para>
	/// Joystick trackball motion.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="JoyBallEvent"/>.
	/// </para>
	/// </remarks>
	JoystickBallMotion,

	/// <summary>The event type <em>JoystickHatMotion</em></summary>
	/// <remarks>
	/// <para>
	/// Joystick hat position change.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="JoyHatEvent"/>.
	/// </para>
	/// </remarks>
	JoystickHatMotion,

	/// <summary>The event type <em>JoystickButtonDown</em></summary>
	/// <remarks>
	/// <para>
	/// Joystick button pressed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="JoyButtonEvent"/>.
	/// </para>
	/// </remarks>
	JoystickButtonDown,

	/// <summary>The event type <em>JoystickButtonUp</em></summary>
	/// <remarks>
	/// <para>
	/// Joystick button released.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="JoyButtonEvent"/>.
	/// </para>
	/// </remarks>
	JoystickButtonUp,

	/// <summary>The event type <em>JoystickAdded</em></summary>
	/// <remarks>
	/// <para>
	/// A new <see cref="Joystick"/> has been inserted into the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="JoyDeviceEvent"/>.
	/// </para>
	/// </remarks>
	JoystickAdded,

	/// <summary>The event type <em>JoystickRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// An opened <see cref="Joystick"/> has been removed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="JoyDeviceEvent"/>.
	/// </para>
	/// </remarks>
	JoystickRemoved,

	/// <summary>The event type <em>JoystickBatteryUpdated</em></summary>
	/// <remarks>
	/// <para>
	/// Joystick battery level change.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="JoyBatteryEvent"/>.
	/// </para>
	/// </remarks>
	JoystickBatteryUpdated,

	/// <summary>The event type <em>JoystickUpdateCompleted</em></summary>
	/// <remarks>
	/// <para>
	/// <see cref="Joystick"/> update is complete.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="JoyDeviceEvent"/>.
	/// </para>
	/// </remarks>
	JoystickUpdateCompleted,

	/// <summary>SDL_EVENT_JOYSTICK_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	JoystickFirst = JoystickAxisMotion,

	/// <summary>SDL_EVENT_JOYSTICK_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	JoystickLast = JoystickUpdateCompleted,

	#endregion

	#region Gamepad events

	/// <summary>The event type <em>GamepadAxisMotion</em></summary>
	/// <remarks>
	/// <para>
	/// Gamepad axis motion.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadAxisEvent"/>.
	/// </para>
	/// </remarks>
	GamepadAxisMotion = 0x650,

	/// <summary>The event type <em>GamepadButtonDown</em></summary>
	/// <remarks>
	/// <para>
	/// Gamepad button pressed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadButtonEvent"/>.
	/// </para>
	/// </remarks>
	GamepadButtonDown,

	/// <summary>The event type <em>GamepadButtonUp</em></summary>
	/// <remarks>
	/// <para>
	/// Gamepad button released.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadButtonEvent"/>.
	/// </para>
	/// </remarks>
	GamepadButtonUp,

	/// <summary>The event type <em>GamepadAdded</em></summary>
	/// <remarks>
	/// <para>
	/// A new <see cref="Gamepad"/> has been inserted into the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadDeviceEvent"/>.
	/// </para>
	/// </remarks>
	GamepadAdded,

	/// <summary>The event type <em>GamepadRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Gamepad"/> has been removed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadDeviceEvent"/>.
	/// </para>
	/// </remarks>
	GamepadRemoved,

	/// <summary>The event type <em>GamepadRemapped</em></summary>
	/// <remarks>
	/// <para>
	/// <see cref="Gamepad"/> mapping was updated.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadDeviceEvent"/>.
	/// </para>
	/// </remarks>
	GamepadRemapped,

	/// <summary>The event type <em>GamepadTouchpadDown</em></summary>
	/// <remarks>
	/// <para>
	/// Gamepad touchpad was touched.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadTouchpadEvent"/>.
	/// </para>
	/// </remarks>
	GamepadTouchpadDown,

	/// <summary>The event type <em>GamepadTouchpadMotion</em></summary>
	/// <remarks>
	/// <para>
	/// Gamepad touchpad finger was moved.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadTouchpadEvent"/>.
	/// </para>
	/// </remarks>
	GamepadTouchpadMotion,

	/// <summary>The event type <em>GamepadTouchpadUp</em></summary>
	/// <remarks>
	/// <para>
	/// Gamepad touchpad finger was lifted.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadTouchpadEvent"/>.
	/// </para>
	/// </remarks>
	GamepadTouchpadUp,

	/// <summary>The event type <em>GamepadSensorUpdated</em></summary>
	/// <remarks>
	/// <para>
	/// Gamepad sensor was updated.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadSensorEvent"/>.
	/// </para>
	/// </remarks>
	GamepadSensorUpdated,

	/// <summary>The event type <em>GamepadUpdateCompleted</em></summary>
	/// <remarks>
	/// <para>
	/// <see cref="Gamepad"/> update is complete.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadDeviceEvent"/>.
	/// </para>
	/// </remarks>
	GamepadUpdateCompleted,

	/// <summary>The event type <em>GamepadSteamHandleUpdated</em></summary>
	/// <remarks>
	/// <para>
	/// <see cref="Gamepad"/> Steam handle has changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadDeviceEvent"/>.
	/// </para>
	/// </remarks>
	GamepadSteamHandleUpdated,

#if SDL3_6_0_OR_GREATER

	/// <summary>The event type <em>GamepadCapSenseTouched</em></summary>
	/// <remarks>
	/// <para>
	/// Gamepad capacitive sensing was activated (e.g., touched or gripped).
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadCapSenseEvent"/>.
	/// </para>
	/// </remarks>
	GamepadCapSenseTouched,

	/// <summary>The event type <em>GamepadCapSenseReleased</em></summary>
	/// <remarks>
	/// <para>
	/// Gamepad capacitive sensing was deactivated (e.g., released).
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadCapSenseEvent"/>.
	/// </para>
	/// </remarks>
	GamepadCapSenseReleased,

#endif

	/// <summary>SDL_EVENT_GAMEPAD_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	GamepadFirst = GamepadAxisMotion,

	/// <summary>SDL_EVENT_GAMEPAD_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	GamepadLast =
#if SDL3_6_0_OR_GREATER
		GamepadCapSenseReleased,
#else
		GamepadSteamHandleUpdated,
#endif

	#endregion

	#region Touch events

	/// <summary>The event type <em>FingerDown</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Finger"/> was placed on a <see cref="TouchDevice"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="TouchFingerEvent"/>.
	/// </para>
	/// </remarks>
	FingerDown = 0x700,

	/// <summary>The event type <em>FingerUp</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Finger"/> was lifted from a <see cref="TouchDevice"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="TouchFingerEvent"/>.
	/// </para>
	/// </remarks>
	FingerUp,

	/// <summary>The event type <em>FingerMotion</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Finger"/> was moved on a <see cref="TouchDevice"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="TouchFingerEvent"/>.
	/// </para>
	/// </remarks>
	FingerMotion,

	/// <summary>The event type <em>FingerCanceled</em></summary>
	/// <remarks>
	/// <para>
	/// <see cref="Finger"/> motion on a <see cref="TouchDevice"/> was canceled.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="TouchFingerEvent"/>.
	/// </para>
	/// </remarks>
	FingerCanceled,

	/// <summary>SDL_EVENT_FINGER_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	FingerFirst = FingerDown,

	/// <summary>SDL_EVENT_FINGER_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	FingerLast = FingerCanceled,

	#endregion

#if SDL3_4_0_OR_GREATER

	#region Pinch events

	/// <summary>The event type <em>PinchBegin</em></summary>
	/// <remarks>
	/// <para>
	/// Pinch gesture started.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PinchFingerEvent"/>.
	/// </para>
	/// </remarks>
	PinchBegin = 0x710,

	/// <summary>The event type <em>PinchUpdate</em></summary>
	/// <remarks>
	/// <para>
	/// Pinch gesture updated.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PinchFingerEvent"/>.
	/// </para>
	/// </remarks>
	PinchUpdated,

	/// <summary>The event type <em>PinchEnd</em></summary>
	/// <remarks>
	/// <para>
	/// Pinch gesture ended.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PinchFingerEvent"/>.
	/// </para>
	/// </remarks>
	PinchEnd,

	/// <summary>SDL_EVENT_PINCH_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	PinchFirst = PinchBegin,

	/// <summary>SDL_EVENT_PINCH_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	PinchLast = PinchEnd,

	#endregion

#endif

	#region Clipboard events

	/// <summary>The event type <em>ClipboardUpdated</em></summary>
	/// <remarks>
	/// <para>
	/// The clipboard or primary selection changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="ClipboardEvent"/>.
	/// </para>
	/// </remarks>
	ClipboardUpdated = 0x900,

	/// <summary>SDL_EVENT_CLIPBOARD_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	ClipboardFirst = ClipboardUpdated,

	/// <summary>SDL_EVENT_CLIPBOARD_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	ClipboardLast = ClipboardUpdated,

	#endregion

	#region Drag and drop events

	/// <summary>The event type <em>DropFile</em></summary>
	/// <remarks>
	/// <para>
	/// The system requests a file open. <see cref="DropEvent.Data"/> contains the filename.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DropEvent"/>.
	/// </para>
	/// </remarks>
	DropFile = 0x1000,

	/// <summary>The event type <em>DropText</em></summary>
	/// <remarks>
	/// <para>
	/// A plain text drag-and-drop event occured. <see cref="DropEvent.Data"/> contains the text.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DropEvent"/>.
	/// </para>
	/// </remarks>
	DropText,

	/// <summary>The event type <em>DropBegin</em></summary>
	/// <remarks>
	/// <para>
	/// A new set of drops is beginning. <see cref="DropEvent.Data"/> is <em><see langword="null" /></em>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DropEvent"/>.
	/// </para>
	/// </remarks>
	DropBegin,

	/// <summary>The event type <em>DropCompleted</em></summary>
	/// <remarks>
	/// <para>
	/// Current set of drops is now complete. <see cref="DropEvent.Data"/> is <em><see langword="null" /></em>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DropEvent"/>.
	/// </para>
	/// </remarks>
	DropCompleted,

	/// <summary>The event type <em>DropPosition</em></summary>
	/// <remarks>
	/// <para>
	/// Position while moving over the window.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DropEvent"/>. <see cref="DropEvent.Data"/> is <em><see langword="null" /></em>.
	/// </para>
	/// </remarks>
	DropPosition,

	/// <summary>SDL_EVENT_DROP_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	DropFirst = DropFile,

	/// <summary>SDL_EVENT_DROP_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	DropLast = DropPosition,

	#endregion

	#region Audio hotplug events

	/// <summary>The event type <em>AudioDeviceAdded</em></summary>
	/// <remarks>
	/// <para>
	/// A new <see cref="AudioDevice"/> is available.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="AudioDeviceEvent"/>.
	/// </para>
	/// </remarks>
	AudioDeviceAdded = 0x1100,

	/// <summary>The event type <em>AudioDeviceRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// An <see cref="AudioDevice"/> has been removed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="AudioDeviceEvent"/>.
	/// </para>
	/// </remarks>
	AudioDeviceRemoved,

	/// <summary>The event type <em>AudioDeviceFormatChanged</em></summary>
	/// <remarks>
	/// <para>
	/// An <see cref="AudioDevice"/>'s format has been changed by the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="AudioDeviceEvent"/>.
	/// </para>
	/// </remarks>
	AudioDeviceFormatChanged,

	/// <summary>SDL_EVENT_AUDIO_DEVICE_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	AudioDeviceFirst = AudioDeviceAdded,

	/// <summary>SDL_EVENT_AUDIO_DEVICE_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	AudioDeviceLast = AudioDeviceFormatChanged,

	#endregion

	#region Sensor events

	/// <summary>The event type <em>SensorUpdated</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Sensor"/> was updated.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="SensorEvent"/>.
	/// </para>
	/// </remarks>
	SensorUpdated = 0x1200,

	/// <summary>SDL_EVENT_SENSOR_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	SensorFirst = SensorUpdated,

	/// <summary>SDL_EVENT_SENSOR_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	SensorLast = SensorUpdated,

	#endregion

	#region Pressure-sensitive pen events

	/// <summary>The event type <em>PenProximityIn</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive <see cref="Pen"/> has become available.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenProximityEvent"/>.
	/// </para>
	/// </remarks>
	PenProximityIn = 0x1300,

	/// <summary>The event type <em>PenProximityOut</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive <see cref="Pen"/> has become unavailable.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenProximityEvent"/>.
	/// </para>
	/// </remarks>
	PenProximityOut,

	/// <summary>The event type <em>PenDown</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive <see cref="Pen"/> touched drawing surface.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenTouchEvent"/>.
	/// </para>
	/// </remarks>
	PenDown,

	/// <summary>The event type <em>PenUp</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive <see cref="Pen"/> stopped touching drawing surface.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenTouchEvent"/>.
	/// </para>
	/// </remarks>
	PenUp,

	/// <summary>The event type <em>PenButtonDown</em></summary>
	/// <remarks>
	/// <para>
	/// Pen button pressed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenButtonEvent"/>.
	/// </para>
	/// </remarks>
	PenButtonDown,

	/// <summary>The event type <em>PenButtonUp</em></summary>
	/// <remarks>
	/// <para>
	/// Pen button released.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenButtonEvent"/>.
	/// </para>
	/// </remarks>
	PenButtonUp,

	/// <summary>The event type <em>PenMotion</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive <see cref="Pen"/> is moving on the tablet.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenMotionEvent"/>.
	/// </para>
	/// </remarks>
	PenMotion,

	/// <summary>The event type <em>PenAxis</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive <see cref="Pen"/> axis (angle, pressure, etc.) changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenAxisEvent"/>.
	/// </para>
	/// </remarks>
	PenAxis,

	/// <summary>SDL_EVENT_PEN_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	PenFirst = PenProximityIn,

	/// <summary>SDL_EVENT_PEN_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	PenLast = PenAxis,

	#endregion

	#region Camera hotplug events

	/// <summary>The event type <em>CameraDeviceAdded</em></summary>
	/// <remarks>
	/// <para>
	/// A new <see cref="Camera"/> is available.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CameraDeviceEvent"/>.
	/// </para>
	/// </remarks>
	CameraDeviceAdded = 0x1400,

	/// <summary>The event type <em>CameraDeviceRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Camera"/> device has been removed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CameraDeviceEvent"/>.
	/// </para>
	/// </remarks>
	CameraDeviceRemoved,

	/// <summary>The event type <em>CameraDeviceApproved</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Camera"/> device has been approved for use by the user.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CameraDeviceEvent"/>.
	/// </para>
	/// </remarks>
	CameraDeviceApproved,

	/// <summary>The event type <em>CameraDeviceDenied</em></summary>
	/// <remarks>
	/// <para>
	/// A <see cref="Camera"/> device has been denied for use by the user.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CameraDeviceEvent"/>.
	/// </para>
	/// </remarks>
	CameraDeviceDenied,

	/// <summary>SDL_EVENT_CAMERA_DEVICE_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	CameraDeviceFirst = CameraDeviceAdded,

	/// <summary>SDL_EVENT_CAMERA_DEVICE_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	CameraDeviceLast = CameraDeviceDenied,

	#endregion

#if SDL3_6_0_OR_GREATER

	#region Notification events

	/// <summary>The event type <em>NotificationActionInvoked</em></summary>
	/// <remarks>
	/// <para>
	/// A user response to a system notification has been received.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="NotificationEvent"/>.
	/// </para>
	/// </remarks>
	NotificationActionInvoked = 0x1500,

	/// <summary>SDL_EVENT_NOTIFICATION_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	NotificationFirst = NotificationActionInvoked,

	/// <summary>SDL_EVENT_NOTIFICATION_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	NotificationLast = NotificationActionInvoked,

	#endregion

#endif

	#region Render events

	/// <summary>The event type <em>RenderTargetsReset</em></summary>
	/// <remarks>
	/// <para>
	/// The <see cref="Renderer"/>'s targets have been reset and their contents need to be updated.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="RenderEvent"/>.
	/// </para>
	/// </remarks>
	RenderTargetsReset = 0x2000,

	/// <summary>The event type <em>RenderDeviceReset</em></summary>
	/// <remarks>
	/// <para>
	/// The render device has been reset and all textures need to be recreated.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="RenderEvent"/>.
	/// </para>
	/// </remarks>
	RenderDeviceReset,

	/// <summary>The event type <em>RenderDeviceLost</em></summary>
	/// <remarks>
	/// <para>
	/// The render device has been lost and can't be recovered.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="RenderEvent"/>.
	/// </para>
	/// </remarks>
	RenderDeviceLost,

	/// <summary>SDL_EVENT_RENDER_FIRST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	RenderFirst = RenderTargetsReset,

	/// <summary>SDL_EVENT_RENDER_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	RenderLast = RenderDeviceLost,

	#endregion

	#region Reserved events for private platforms

	/// <summary>SDL_EVENT_PRIVATE0</summary>
	/// <remarks>Reserved event for private platforms. Do not use.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Experimental("SDL6010")] //TODO: make 'SDL6010' the diagnostics id for 'reserved events for private platforms'
	Private0 = 0x4000,

	/// <summary>SDL_EVENT_PRIVATE1</summary>
	/// <remarks>Reserved event for private platforms. Do not use.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Experimental("SDL6010")] //TODO: make 'SDL6010' the diagnostics id for 'reserved events for private platforms'
	Private1,

	/// <summary>SDL_EVENT_PRIVATE2</summary>
	/// <remarks>Reserved event for private platforms. Do not use.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Experimental("SDL6010")] //TODO: make 'SDL6010' the diagnostics id for 'reserved events for private platforms'
	Private2,

	/// <summary>SDL_EVENT_PRIVATE3</summary>
	/// <remarks>Reserved event for private platforms. Do not use.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Experimental("SDL6010")] //TODO: make 'SDL6010' the diagnostics id for 'reserved events for private platforms'
	Private3,

	#endregion

	#region Internal events

	/// <summary>SDL_EVENT_POLL_SENTINEL</summary>
	/// <remarks>Internal use only. Signals the end of an event poll cycle.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	PollSentinel = 0x7F00,

	#endregion

	#region User events

	/// <summary>SDL_EVENT_USER</summary>
	/// <remarks>Internal use only. Use <see cref="EventTypeExtensions.TryRegister(out EventType)"/> or <see cref="EventTypeExtensions.TryRegister(Span{EventType})"/> instead.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only. Use the EventTypeExtensions.TryRegister(out EventType) or EventTypeExtensions.TryRegister(Span{EventType}) extension methods instead.")]
	User = 0x8000,

	#endregion

	/// <summary>SDL_EVENT_LAST</summary>
	/// <remarks>Internal use only.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Internal use only.")]
	Last = 0xFFFF,
}
