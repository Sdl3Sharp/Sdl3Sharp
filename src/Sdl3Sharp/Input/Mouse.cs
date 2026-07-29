using Sdl3Sharp.Events;
using Sdl3Sharp.Internal;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents a mouse device connected to the system
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Id"/> of a mouse is unique, remains unchanged while the mouse is connected to the system, and is never reused for the lifetime of the application.
/// If a mouse is disconnected and then reconnected, it will be assigned a new <see cref="Id"/>.
/// </para>
/// <para>
/// For the most part, <see cref="Mouse"/> is not thread-safe, and most of its properties and methods should only be accessed from the main thread.
/// </para>
/// </remarks>
public readonly partial struct Mouse
{
	private readonly uint mId;

	internal Mouse(uint id) => mId = id;

	/// <summary>
	/// Gets a collection of all mice that are currently connected to the system
	/// </summary>
	/// <value>
	/// A collection of all mice that are currently connected to the system
	/// </value>
	/// <remarks>
	/// <para>
	/// The collection returned by this property will include any device or virtual driver that includes mouse functionality, including some game controllers, KVM switches, etc.
	/// You should wait for input from a mouse device before you consider it actively in use.
	/// </para>
	/// <para>
	/// The value of this property will change with an <see cref="EventType.MouseAdded"/> or <see cref="EventType.MouseRemoved"/> event (<see cref="MouseDeviceEvent"/>) received.
	/// You should not query this property too often, but rather cache the result and update the cache when such an event is raised.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the mice connected to the system (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public static Mouse[] ConnectedMice
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out int count);

				var mice = SDL_GetMice(&count);

				if (mice is null)
				{
					[DoesNotReturn]
					static void failCouldNotGetMice() => throw new SdlException($"Couldn't get the mice connected to the system");

					failCouldNotGetMice();
				}

				try
				{
					if (count is not > 0)
					{
						return [];
					}

					var result = GC.AllocateUninitializedArray<Mouse>(count);

					var mousePtr = mice;
					foreach (ref var mouse in result.AsSpan())
					{
						mouse = new(*mousePtr++);
					}

					return result;
				}
				finally
				{
					Utilities.NativeMemory.Free(mice);
				}
			}
		}
	}

	/// <summary>
	/// Gets the window that currently has mouse focus
	/// </summary>
	/// <value>
	/// The window that currently has mouse focus, or <c><see langword="null"/></c> if no window has mouse focus
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public static Window? FocusWindow
	{
		get
		{
			unsafe
			{
				Window.TryGetOrCreate(SDL_GetMouseFocus(), out var window);
				return window;
			}
		}
	}

	/// <summary>
	/// Gets the asynchronous mouse button state and the desktop-relative cursor position
	/// </summary>
	/// <value>
	/// The asynchronous mouse button state and the desktop-relative cursor position
	/// </value>
	/// <remarks>
	/// <para>
	/// The <c>X</c> coordinate of the value of this property is the cursor's horizontal position from the left edge of the desktop,
	/// and the <c>Y</c> coordinate is the cursor's vertical position from the top edge of the desktop.
	/// </para>
	/// <para>
	/// Accessing this property queries the platform for the most recent asynchronous state.
	/// It's more expensive than accessing the cached state via the <see cref="State"/> property.
	/// </para>
	/// <para>
	/// If a window is in <see cref="Window.IsRelativeMouseModeEnabled">relative mouse mode</see>, the value of this property
	/// usually contradicts the cursor position as manually calculated from <see cref="State"/> and <see cref="Window.Position"/>.
	/// </para>
	/// <para>
	/// This property can be useful if you need to track the mouse outside of a specific window and <see cref="TryEnableCapture"/> doesn't fit your needs.
	/// For example, it could be useful if you need to track the mouse while dragging a window, where coordinates relative to a window might not be in sync at all times.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public static (MouseButtonFlags Button, float X, float Y) GlobalState
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out float x);
				Unsafe.SkipInit(out float y);

				var button = SDL_GetGlobalMouseState(&x, &y);

				return (button, x, y);
			}
		}
	}

	/// <summary>
	/// Gets the numeric ID of this mouse
	/// </summary>
	/// <value>
	/// The numeric ID of this mouse
	/// </value>
	/// <remarks>
	/// <para>
	/// The <see cref="Id"/> of a mouse is unique, remains unchanged while the mouse is connected to the system, and is never reused for the lifetime of the application.
	/// If a mouse is disconnected and then reconnected, it will be assigned a new <see cref="Id"/>.
	/// </para>
	/// <para>
	/// An id of <c>0</c> indicates an invalid mouse.
	/// </para>
	/// </remarks>
	public readonly uint Id { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mId; }

	/// <summary>
	/// Gets a value indicating whether at least one mouse is currently connected to the system
	/// </summary>
	/// <value>
	/// A value indicating whether at least one mouse is currently connected to the system
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public static bool IsAnyConnected => SDL_HasMouse();

	/// <summary>
	/// Gets the name of this mouse
	/// </summary>
	/// <value>
	/// The name of this mouse, <see cref="string.Empty"><c>""</c></see> if the mouse has no name, or <c><see langword="null"/></c> if the name couldn't be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public readonly string? Name
	{
		get
		{
			unsafe
			{
				// To be consistent with how Display.Name works, we return null here if the mouse name is null, although that's considered an error by SDL.
				// Just like Display.Name, we document that the user should check for errors using Error.TryGet if the result is null.
				return Utf8StringMarshaller.ConvertToManaged(SDL_GetMouseNameForID(mId));
			}
		}
	}

#if SDL3_4_0_OR_GREATER

	private static GCHandle mRelativeMouseTransformHandle;

	/// <summary>
	/// Gets or sets an user-defined callback method to transform relative mouse inputs
	/// </summary>
	/// <value>
	/// The user-defined callback method to transform relative mouse inputs, or <c><see langword="null"/></c> if no callback is set
	/// </value>
	/// <remarks>
	/// <para>
	/// If a callback is set, relative system scale and relative speed hints are overridden and the callback is used to transform relative mouse inputs instead.
	/// </para>
	/// <para>
	/// This property must be set before enabling <see cref="Window.IsRelativeMouseModeEnabled">relative mouse mode</see> for any window, otherwise a <see cref="SdlException"/> will be thrown.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting this property, the callback method couln't be set (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </exception>
	public static MouseMotionTransformCallback? RelativeMouseTransform
	{
		get
		{
			// SDL doesn't provide a way to get the current relative mouse transform callback.
			// Since we have to store a handle to the callback anyway (in order to release it at some point), we can just return whatever the handle is currently referencing, if anything.
			// Of course, this way we can't detect callbacks set by other means, but we just return null in that case and call it a day.
			// I think that's a reasonable compromise, since otherwise we wouldn't have a getter at all.

			if (mRelativeMouseTransformHandle is { IsAllocated: true, Target: MouseMotionTransformCallback callback })
			{
				return callback;
			}

			return null;
		}

		set
		{
			unsafe
			{
				if (mRelativeMouseTransformHandle.IsAllocated)
				{
					// release the previous handle, if any

					mRelativeMouseTransformHandle.Free();
					mRelativeMouseTransformHandle = default;
				}

				if (value is null)
				{
					SdlErrorHelper.ThrowIfFailed(SDL_SetRelativeMouseTransform(null, null));
				}
				else
				{
					mRelativeMouseTransformHandle = GCHandle.Alloc(value, GCHandleType.Normal);

					try
					{
						SdlErrorHelper.ThrowIfFailed(SDL_SetRelativeMouseTransform(&MouseMotionTransformCallback, unchecked((void*)GCHandle.ToIntPtr(mRelativeMouseTransformHandle))));
					}
					catch
					{
						// in case of an error, release the handle we just allocated and reset it

						mRelativeMouseTransformHandle.Free();
						mRelativeMouseTransformHandle = default;

						throw;
					}
				}
			}
		}
	}

#endif

	/// <summary>
	/// Gets the synchronous mouse button state and accumulated mouse delta since the last access to this property
	/// </summary>
	/// <value>
	/// The synchronous mouse button state and accumulated mouse delta since the last access to this property
	/// </value>
	/// <remarks>
	/// <para>
	/// The <c>Buttons</c> component of the value of this property is the cached synchronous state as SDL understands it from the last pump of the event queue,
	/// the <c>DeltaX</c> component is the accumulated horizontal mouse delta since the last access to this property,
	/// and the <c>DeltaY</c> component is the accumulated vertical mouse delta since the last access to this property.
	/// </para>
	/// <para>
	/// This property uses SDL's cached synchronous mouse state and is therefore more efficient.
	/// If you need an immediate asynchronous state, use the <see cref="GlobalState"/> property instead.
	/// </para>
	/// <para>
	/// This property can be useful for reducing overhead by processing relative mouse inputs in one go per-frame instead of individually per-event,
	/// at the expense of losing the order between events within the frame (e.g. quickly pressing and releasing a button within the same frame).
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public static (MouseButtonFlags Buttons, float DeltaX, float DeltaY) RelativeState
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out float x);
				Unsafe.SkipInit(out float y);

				var buttons = SDL_GetRelativeMouseState(&x, &y);

				return (buttons, x, y);
			}
		}
	}

	/// <summary>
	/// Gets the synchronous mouse button state and the window-relative cursor position
	/// </summary>
	/// <value>
	/// The synchronous mouse button state and the window-relative cursor position
	/// </value>
	/// <remarks>
	/// <para>
	/// The <c>Buttons</c> component of the value of this property is the cached synchronous state,
	/// the <c>X</c> component is the cursor's horizontal position from the left edge of the window that currently has mouse focus,
	/// the <c>Y</c> component is the cursor's vertical position from the top edge of the window that currently has mouse focus,
	/// all of which are as SDL understands them from the last pump of the event queue.
	/// </para>
	/// <para>
	/// This property uses SDL's cached synchronous mouse state and is therefore more efficient.
	/// If you need an immediate asynchronous state, use the <see cref="GlobalState"/> property instead.
	/// </para>
	/// <para>
	/// If a window is in <see cref="Window.IsRelativeMouseModeEnabled">relative mouse mode</see>, the value of this property
	/// usually contradicts the cursor position as manually calculated from <see cref="GlobalState"/> and <see cref="Window.Position"/>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public static (MouseButtonFlags Buttons, float X, float Y) State
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out float x);
				Unsafe.SkipInit(out float y);

				var buttons = SDL_GetMouseState(&x, &y);

				return (buttons, x, y);
			}
		}
	}

	/// <summary>
	/// Moves the cursor to the specified position within the given window
	/// </summary>
	/// <param name="window">The window in which to move the cursor</param>
	/// <param name="x">The horizontal position from the left edge of the window to move the cursor to</param>
	/// <param name="y">The vertical position from the top edge of the window to move the cursor to</param>
	/// <remarks>
	/// <para>
	/// This method generates a <see cref="EventType.MouseMotion"/> event (<see cref="MouseMotionEvent"/>), if <see cref="Window.IsRelativeMouseModeEnabled">relative mouse mode</see> is not enabled for the given <paramref name="window"/>.
	/// Otherwise, if it is enabled, you can force events to be generated by setting the <see cref="Hint.Mouse.RelativeWarpMotion"/> hint.
	/// </para>
	/// <para>
	/// If used over Microsoft Remove Desktop, this method will appear to succeed, but the cursor will not actually move.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static void WarpInWindow(Window? window, float x, float y)
	{
		unsafe
		{
			SDL_WarpMouseInWindow(window is not null ? window.Pointer : null, x, y);
		}
	}

	/// <summary>
	/// Tries to disable mouse capture if it was previously enabled
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if mouse capture was successfully disabled; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method tries to disable mouse capture if it was previously enabled by <see cref="TryEnableCapture"/>.
	/// <see cref="TryEnableCapture"/> and <see cref="TryDisableCapture"/> should be always used in pairs.
	/// </para>
	/// <para>
	/// <see cref="TryEnableCapture"/> for more information about mouse capture and its implications.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryDisableCapture() => SDL_CaptureMouse(false);

	/// <summary>
	/// Tries to enable mouse capture, which allows for tracking mouse input outside of <see cref="Window"/>s
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if mouse capture was successfully enabled; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Capturing the mouse enables your application to obtain mouse events globally, instead of just within your window.
	/// Not all video targets support this function.
	/// When capturing is enabled, the current window will get all mouse related events, but unlike <see cref="Window.IsRelativeMouseModeEnabled">relative mouse mode</see>, no change is made to the cursor and it is not restrained to your window.
	/// </para>
	/// <para>
	/// This method may also deny mouse input to other windows, both those in your application and others on the system, so you should use this method sparingly, paired with <see cref="TryDisableCapture"/> calls, and in small bursts.
	/// For example, you might want to track the mouse while the user is dragging something, until the user releases a mouse button.
	/// It is not recommended that you capture the mouse for long periods of time, such as the entire lifetime of your application.
	/// For that, you should probably use <see cref="Window.IsRelativeMouseModeEnabled"/> or <see cref="Window.HasMouseGrab"/> depending on your goals.
	/// </para>
	/// <para>
	/// While captured, mouse events still report coordinates relative to the current (foreground) window, but those coordinates may be outside the bounds of the window (including negative values).
	/// Capturing is only allowed for the foreground window.
	/// If the window loses focus while capturing, the capture will be disabled automatically.
	/// </para>
	/// <para>
	/// While capturing is enabled, the current window will have the <see cref="WindowFlags.MouseCapture"/> <see cref="Window.Flags">flag</see> set.
	/// </para>
	/// <para>
	/// Please note that SDL will attempt to "auto capture" the mouse while the user is pressing a button; this is to try and make mouse behavior more consistent between platforms, and deal with the common case of a user dragging the mouse outside of the window.
	/// This means that if you are trying to use mouse capture only to deal with this situation, you do not have to (although it is safe to do so).
	/// If this causes problems for your application, you can disable auto capture by setting the <see cref="Hint.Mouse.AutoCapture"/> hint.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryEnableCapture() => SDL_CaptureMouse(true);

	/// <summary>
	/// Moves the cursor to the specified position in global screen space
	/// </summary>
	/// <param name="x">The horizontal position in global screen coordinates to move the cursor to</param>
	/// <param name="y">The vertical position in global screen coordinates to move the cursor to</param>
	/// <returns><c><see langword="true"/></c>, if the cursor was successfully moved; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method generates a <see cref="EventType.MouseMotion"/> event (<see cref="MouseMotionEvent"/>).
	/// </para>
	/// <para>
	/// If this method fails, it usually means that the platform does not support moving the cursor in global screen space.
	/// </para>
	/// <para>
	/// If used over Microsoft Remove Desktop, this method will appear to succeed, but the cursor will not actually move.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryWarpGlobal(float x, float y) => SDL_WarpMouseGlobal(x, y);
}
