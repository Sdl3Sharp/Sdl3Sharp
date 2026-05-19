using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Sdl3Sharp.Events2;

/// <summary>
/// Represents an event type for an <see cref="Event"/>
/// </summary>
public enum EventType : uint
{
	/// <summary>SDL_EVENT_FIRST</summary>
	/// <remarks>Do not use.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Do not use.")]
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
	/// The application is being terminated by the OS. This event must be handled in an event handler registered with <see cref="Sdl.EventWatch"/>.
	/// Called on iOS in <c>applicationWillTerminate()</c>.
	/// Called on Android in <c>onDestroy()</c>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="Event"/>.
	/// </para>
	/// </remarks>
	Terminating,

	/// <summary>The event type <em>LowMemory</em></summary>
	/// <remarks>
	/// <para>
	/// The application is low on memory, free memory if possible. This event must be handled in an event handler registered with <see cref="Sdl.EventWatch"/>.
	/// Called on iOS in <c>applicationDidReceiveMemoryWarning</c>().
	/// Called on Android in <c>onTrimMemory</c>().
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="Event"/>.
	/// </para>
	/// </remarks>
	LowMemory,

	/// <summary>The event type <em>WillEnterBackground</em></summary>
	/// <remarks>
	/// <para>
	/// The application is about to enter the background. This event must be handled in an event handler registered with <see cref="Sdl.EventWatch"/>.
	/// Called on iOS in <c>applicationWillResignActive()</c>.
	/// Called on Android in <c>onPause()</c>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="Event"/>.
	/// </para>
	/// </remarks>
	WillEnterBackground,

	/// <summary>The event type <em>DidEnterBackground</em></summary>
	/// <remarks>
	/// <para>
	/// The application did enter the background and may not get CPU for some time. This event must be handled in an event handler registered with <see cref="Sdl.EventWatch"/>.
	/// Called on iOS in <c>applicationDidEnterBackground()</c>.
	/// Called on Android in <c>onPause()</c>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="Event"/>.
	/// </para>
	/// </remarks>
	DidEnterBackground,

	/// <summary>The event type <em>WillEnterForeground</em></summary>
	/// <remarks>
	/// <para>
	/// The application is about to enter the foreground. This event must be handled in an event handler registered with <see cref="Sdl.EventWatch"/>.
	/// Called on iOS in <c>applicationWillEnterForeground()</c>.
	/// Called on Android in <c>onResume()</c>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="Event"/>.
	/// </para>
	/// </remarks>
	WillEnterForeground,

	/// <summary>The event type <em>DidEnterForeground</em></summary>
	/// <remarks>
	/// <para>
	/// The application is now interactive. This event must be handled in an event handler registered with <see cref="Sdl.EventWatch"/>.
	/// Called on iOS in <c>applicationDidBecomeActive()</c>.
	/// Called on Android in <c>onResume()</c>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="Event"/>.
	/// </para>
	/// </remarks>
	DidEnterForeground,

	/// <summary>The event type <em>LocaleChanged</em></summary>
	/// <remarks>
	/// <para>
	/// The user's locale preferences have changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="Event"/>.
	/// </para>
	/// </remarks>
	LocaleChanged,

	/// <summary>The event type <em>SystemThemeChanged</em></summary>
	/// <remarks>
	/// <para>
	/// The system theme changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="Event"/>.
	/// </para>
	/// </remarks>
	SystemThemeChanged,

	#endregion

	#region Display events

	/// <summary>The event type <em>DisplayOrientationChanged</em></summary>
	/// <remarks>
	/// <para>
	/// The display orientation has changed to <see cref="DisplayEvent.Data1"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayOrientationChanged = 0x151,

	/// <summary>The event type <em>DisplayAdded</em></summary>
	/// <remarks>
	/// <para>
	/// A display has been added to the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayAdded,

	/// <summary>The event type <em>DisplayRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// A display has been removed from the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayRemoved,

	/// <summary>The event type <em>DisplayMoved</em></summary>
	/// <remarks>
	/// <para>
	/// A display has changed position.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayMoved,

	/// <summary>The event type <em>DisplayDesktopModeChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A display has changed desktop mode.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayDesktopModeChanged,

	/// <summary>The event type <em>DisplayCurrentModeChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A display has changed current mode.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayCurrentModeChanged,

	/// <summary>The event type <em>DisplayContentScaleChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A display has changed content scale.
	/// </para> 
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayContentScaleChanged,

	/// <summary>The event type <em>DisplayUsableBoundsChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A display has changed usable bounds.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="DisplayEvent"/>.
	/// </para>
	/// </remarks>
	DisplayUsableBoundsChanged,

	#endregion

	#region Window events

	/// <summary>The event type <em>WindowShown</em></summary>
	/// <remarks>
	/// <para>
	/// A window has been shown.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowShown = 0x202,

	/// <summary>The event type <em>WindowHidden</em></summary>
	/// <remarks>
	/// <para>
	/// A window has been hidden.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowHidden,

	/// <summary>The event type <em>WindowExposed</em></summary>
	/// <remarks>
	/// <para>
	/// A window has been exposed and should be redrawn, and can be redrawn directly from event watchers/handlers for this event.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowExposed,

	/// <summary>The event type <em>WindowMoved</em></summary>
	/// <remarks>
	/// <para>
	/// A window has been moved to <see cref="WindowEvent.Data1"/>, <see cref="WindowEvent.Data2"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowMoved,

	/// <summary>The event type <em>WindowResized</em></summary>
	/// <remarks>
	/// <para>
	/// A window has been resized to <see cref="WindowEvent.Data1"/>×<see cref="WindowEvent.Data2"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowResized,

	/// <summary>The event type <em>WindowPixelSizeChanged</em></summary>
	/// <remarks>
	/// <para>
	/// The pixel size of a window has changed to <see cref="WindowEvent.Data1"/>×<see cref="WindowEvent.Data2"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowPixelSizeChanged,

	/// <summary>The event type <em>WindowMetalViewResized</em></summary>
	/// <remarks>
	/// <para>
	/// The pixel size of a Metal view associated with a window has changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowMetalViewResized,

	/// <summary>The event type <em>WindowMinimized</em></summary>
	/// <remarks>
	/// <para>
	/// A window has been minimized.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowMinimized,

	/// <summary>The event type <em>WindowMaximized</em></summary>
	/// <remarks>
	/// <para>
	/// A window has been maximized.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowMaximized,

	/// <summary>The event type <em>WindowRestored</em></summary>
	/// <remarks>
	/// <para>
	/// A window has been restored to normal size and position.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowRestored,

	/// <summary>The event type <em>WindowMouseEnter</em></summary>
	/// <remarks>
	/// <para>
	/// A window has gained mouse focus.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowMouseEnter,

	/// <summary>The event type <em>WindowMouseLeave</em></summary>
	/// <remarks>
	/// <para>
	/// A window has lost mouse focus.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowMouseLeave,

	/// <summary>The event type <em>WindowFocusGained</em></summary>
	/// <remarks>
	/// <para>
	/// A window has gained keyboard focus.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowFocusGained,

	/// <summary>The event type <em>WindowFocusLost</em></summary>
	/// <remarks>
	/// <para>
	/// A window has lost keyboard focus.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowFocusLost,

	/// <summary>The event type <em>WindowCloseRequested</em></summary>
	/// <remarks>
	/// <para>
	/// The window manager requests that a window should be closed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowCloseRequested,

	/// <summary>The event type <em>WindowHitTest</em></summary>
	/// <remarks>
	/// <para>
	/// A window had a hit test that wasn't <see cref="SDL_HITTEST_NORMAL"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowHitTest,

	/// <summary>The event type <em>WindowIccProfileChanged</em></summary>
	/// <remarks>
	/// <para>
	/// The ICC profile of a window's display has changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowIccProfileChanged,

	/// <summary>The event type <em>WindowDisplayChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A window has been moved to display <see cref="WindowEvent.Data1"/>.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowDisplayChanged,

	/// <summary>The event type <em>WindowDisplayScaleChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A window's display scale has been changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowDisplayScaleChanged,

	/// <summary>The event type <em>WindowSafeAreaChanged</em></summary>
	/// <remarks>
	/// <para>
	/// A window's safe area has been changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowSafeAreaChanged,

	/// <summary>The event type <em>WindowOccluded</em></summary>
	/// <remarks>
	/// <para>
	/// A window has been occluded.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowOccluded,

	/// <summary>The event type <em>WindowEnterFullscreen</em></summary>
	/// <remarks>
	/// <para>
	/// A window has entered fullscreen mode.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowEnterFullscreen,

	/// <summary>The event type <em>WindowLeaveFullscreen</em></summary>
	/// <remarks>
	/// <para>
	/// A window has left fullscreen mode.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowLeaveFullscreen,

	/// <summary>The event type <em>WindowDestroyed</em></summary>
	/// <remarks>
	/// <para>
	/// The window with the associated ID is being or has been destroyed.
	/// If this message is being handled in an event watcher, the window handle is still valid and can still be used to retrieve any properties associated with the window.
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
	/// A window's HDR properties have changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="WindowEvent"/>.
	/// </para>
	/// </remarks>
	WindowHdrStateChanged,

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
	/// Associated event structure: <see cref="Event"/>.
	/// </para>
	/// </remarks>
	KeymapChanged,

	/// <summary>The event type <em>KeyboardAdded</em></summary>
	/// <remarks>
	/// <para>
	/// A new keyboard has been inserted into the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="KeyboardDeviceEvent"/>.
	/// </para>
	/// </remarks>
	KeyboardAdded,

	/// <summary>The event type <em>KeyboardRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// A keyboard has been removed.
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
	/// Associated event structure: <see cref="Event"/>.
	/// </para>
	/// </remarks>
	ScreenKeyboardShown,

	/// <summary>The event type <em>ScreenKeyboardHidden</em></summary>
	/// <remarks>
	/// <para>
	/// The on-screen keyboard has been hidden.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="Event"/>.
	/// </para>
	/// </remarks>
	ScreenKeyboardHidden,

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
	/// A new mouse has been inserted into the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="MouseDeviceEvent"/>.
	/// </para>
	/// </remarks>
	MouseAdded,

	/// <summary>The event type <em>MouseRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// A mouse has been removed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="MouseDeviceEvent"/>.
	/// </para>
	/// </remarks>
	MouseRemoved,

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
	/// A new joystick has been inserted into the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="JoyDeviceEvent"/>.
	/// </para>
	/// </remarks>
	JoystickAdded,

	/// <summary>The event type <em>JoystickRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// An opened joystick has been removed.
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
	/// Joystick update is complete.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="JoyDeviceEvent"/>.
	/// </para>
	/// </remarks>
	JoystickUpdateCompleted,

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
	/// A new gamepad has been inserted into the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadDeviceEvent"/>.
	/// </para>
	/// </remarks>
	GamepadAdded,

	/// <summary>The event type <em>GamepadRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// A gamepad has been removed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadDeviceEvent"/>.
	/// </para>
	/// </remarks>
	GamepadRemoved,

	/// <summary>The event type <em>GamepadRemapped</em></summary>
	/// <remarks>
	/// <para>
	/// Gamepad mapping was updated.
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
	/// Gamepad update is complete.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadDeviceEvent"/>.
	/// </para>
	/// </remarks>
	GamepadUpdateCompleted,

	/// <summary>The event type <em>GamepadSteamHandleUpdated</em></summary>
	/// <remarks>
	/// <para>
	/// Gamepad Steam handle has changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="GamepadDeviceEvent"/>.
	/// </para>
	/// </remarks>
	GamepadSteamHandleUpdated,

	#endregion

	#region Touch events

	/// <summary>The event type <em>FingerDown</em></summary>
	/// <remarks>
	/// <para>
	/// Associated event structure: <see cref="TouchFingerEvent"/>.
	/// </para>
	/// </remarks>
	FingerDown = 0x700,

	/// <summary>The event type <em>FingerUp</em></summary>
	/// <remarks>
	/// <para>
	/// Associated event structure: <see cref="TouchFingerEvent"/>.
	/// </para>
	/// </remarks>
	FingerUp,

	/// <summary>The event type <em>FingerMotion</em></summary>
	/// <remarks>
	/// <para>
	/// Associated event structure: <see cref="TouchFingerEvent"/>.
	/// </para>
	/// </remarks>
	FingerMotion,

	/// <summary>The event type <em>FingerCanceled</em></summary>
	/// <remarks>
	/// <para>
	/// Associated event structure: <see cref="TouchFingerEvent"/>.
	/// </para>
	/// </remarks>
	FingerCanceled,

	#endregion

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

	#endregion

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
	/// Associated event structure: <see cref="DropEvent"/>.
	/// </para>
	/// </remarks>
	DropPosition,

	#endregion

	#region Audio hotplug events

	/// <summary>The event type <em>AudioDeviceAdded</em></summary>
	/// <remarks>
	/// <para>
	/// A new audio device is available.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="AudioDeviceEvent"/>.
	/// </para>
	/// </remarks>
	AudioDeviceAdded = 0x1100,

	/// <summary>The event type <em>AudioDeviceRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// An audio device has been removed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="AudioDeviceEvent"/>.
	/// </para>
	/// </remarks>
	AudioDeviceRemoved,

	/// <summary>The event type <em>AudioDeviceFormatChanged</em></summary>
	/// <remarks>
	/// <para>
	/// An audio device's format has been changed by the system.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="AudioDeviceEvent"/>.
	/// </para>
	/// </remarks>
	AudioDeviceFormatChanged,

	#endregion

	#region Sensor events

	/// <summary>The event type <em>SensorUpdated</em></summary>
	/// <remarks>
	/// <para>
	/// A sensor was updated.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="SensorEvent"/>.
	/// </para>
	/// </remarks>
	SensorUpdated = 0x1200,

	#endregion

	#region Pressure-sensitive pen events

	/// <summary>The event type <em>PenProximityIn</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive pen has become available.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenProximityEvent"/>.
	/// </para>
	/// </remarks>
	PenProximityIn = 0x1300,

	/// <summary>The event type <em>PenProximityOut</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive pen has become unavailable.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenProximityEvent"/>.
	/// </para>
	/// </remarks>
	PenProximityOut,

	/// <summary>The event type <em>PenDown</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive pen touched drawing surface.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenTouchEvent"/>.
	/// </para>
	/// </remarks>
	PenDown,

	/// <summary>The event type <em>PenUp</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive pen stopped touching drawing surface.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenTouchEvent"/>.
	/// </para>
	/// </remarks>
	PenUp,

	/// <summary>The event type <em>PenButtonDown</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive pen button pressed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenButtonEvent"/>.
	/// </para>
	/// </remarks>
	PenButtonDown,

	/// <summary>The event type <em>PenButtonUp</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive pen button released.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenButtonEvent"/>.
	/// </para>
	/// </remarks>
	PenButtonUp,

	/// <summary>The event type <em>PenMotion</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive pen is moving on the tablet.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenMotionEvent"/>.
	/// </para>
	/// </remarks>
	PenMotion,

	/// <summary>The event type <em>PenAxisChanged</em></summary>
	/// <remarks>
	/// <para>
	/// Pressure-sensitive pen angle/pressure/etc. changed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="PenAxisEvent"/>.
	/// </para>
	/// </remarks>
	PenAxisChanged,

	#endregion

	#region Camera hotplug events

	/// <summary>The event type <em>CameraDeviceAdded</em></summary>
	/// <remarks>
	/// <para>
	/// A new camera device is available.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CameraDeviceEventData"/>.
	/// </para>
	/// </remarks>
	CameraDeviceAdded = 0x1400,

	/// <summary>The event type <em>CameraDeviceRemoved</em></summary>
	/// <remarks>
	/// <para>
	/// A camera device has been removed.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CameraDeviceEventData"/>.
	/// </para>
	/// </remarks>
	CameraDeviceRemoved,

	/// <summary>The event type <em>CameraDeviceApproved</em></summary>
	/// <remarks>
	/// <para>
	/// A camera device has been approved for use by the user.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CameraDeviceEventData"/>.
	/// </para>
	/// </remarks>
	CameraDeviceApproved,

	/// <summary>The event type <em>CameraDeviceDenied</em></summary>
	/// <remarks>
	/// <para>
	/// A camera device has been denied for use by the user.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="CameraDeviceEventData"/>.
	/// </para>
	/// </remarks>
	CameraDeviceDenied,

	#endregion

	#region Render events

	/// <summary>The event type <em>RenderTargetsReset</em></summary>
	/// <remarks>
	/// <para>
	/// The render targets have been reset and their contents need to be updated.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="RenderEvent"/>.
	/// </para>
	/// </remarks>
	RenderTargetsReset = 0x2000,

	/// <summary>The event type <em>RenderDeviceReset</em></summary>
	/// <remarks>
	/// <para>
	/// The device has been reset and all textures need to be recreated.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="RenderEvent"/>.
	/// </para>
	/// </remarks>
	RenderDeviceReset,

	/// <summary>The event type <em>RenderDeviceLost</em></summary>
	/// <remarks>
	/// <para>
	/// The device has been lost and can't be recovered.
	/// </para>
	/// <para>
	/// Associated event structure: <see cref="RenderEvent"/>.
	/// </para>
	/// </remarks>
	RenderDeviceLost,

	#endregion

	#region Reserved events for private platforms

	/// <summary>SDL_EVENT_PRIVATE0</summary>
	/// <remarks>Reserved event for private platforms. Do not use.</remarks>
	[Experimental("SDL6010")] //TODO: make 'SDL6010' the diagnostics id for 'reserved events for private platforms'
	Private0 = 0x4000,

	/// <summary>SDL_EVENT_PRIVATE1</summary>
	/// <remarks>Reserved event for private platforms. Do not use.</remarks>
	[Experimental("SDL6010")] //TODO: make 'SDL6010' the diagnostics id for 'reserved events for private platforms'
	Private1,

	/// <summary>SDL_EVENT_PRIVATE2</summary>
	/// <remarks>Reserved event for private platforms. Do not use.</remarks>
	[Experimental("SDL6010")] //TODO: make 'SDL6010' the diagnostics id for 'reserved events for private platforms'
	Private2,

	/// <summary>SDL_EVENT_PRIVATE3</summary>
	/// <remarks>Reserved event for private platforms. Do not use.</remarks>
	[Experimental("SDL6010")] //TODO: make 'SDL6010' the diagnostics id for 'reserved events for private platforms'
	Private3,

	#endregion

	#region Internal events

	/// <summary>SDL_EVENT_POLL_SENTINEL</summary>
	/// <remarks>Do not use. Signals the end of an event poll cycle.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Do not use.")]
	PollSentinel = 0x7F00,

	/// <summary>SDL_EVENT_USER</summary>
	/// <remarks>Do not use directly. Use <see cref="EventTypeExtensions.TryRegister(out EventType)"/> or <see cref="EventTypeExtensions.TryRegister(Span{EventType})"/> instead.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Do not use. Use the EventTypeExtensions.TryRegister(out EventType) or EventTypeExtensions.TryRegister(Span{EventType}) extension methods instead.")]
	User = 0x8000,

	#endregion

	/// <summary>SDL_EVENT_LAST</summary>
	/// <remarks>Do not use directly. Use <see cref="EventTypeExtensions.TryRegister(out EventType)"/> or <see cref="EventTypeExtensions.TryRegister(Span{EventType})"/> instead.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Obsolete("Do not use. Use the EventTypeExtensions.TryRegister(out EventType) or EventTypeExtensions.TryRegister(Span{EventType}) extension methods instead.")]
	Last = 0xFFFF,
}
