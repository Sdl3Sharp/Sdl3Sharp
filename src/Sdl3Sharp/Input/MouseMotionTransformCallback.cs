#if SDL3_4_0_OR_GREATER

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents an user-defined callback method to transform relative mouse inputs from raw values
/// </summary>
/// <param name="args">The arguments containing information about the mouse event and the values to transform</param>
/// <remarks>
/// <para>
/// If used with <see cref="Mouse.RelativeMouseTransform"/>, the callback will be invoked during SDL's handling of platform mouse events to scale the values of the resulting motion delta.
/// </para>
/// <para>
/// The callback method is called by SDL's internal mouse input processing procedure, which may be a thread separate from the main event loop that is run at realtime priority.
/// Stalling this thread with too much work in the callback method can therefore potentially freeze the entire system.
/// Care should be taken with proper synchronization practices when adding other side effects beyond mutation of the <see cref="MouseMotionTransformArgs.X"/> and <see cref="MouseMotionTransformArgs.Y"/> values.
/// </para>
/// </remarks>
public delegate void MouseMotionTransformCallback(in MouseMotionTransformArgs args);

#endif
