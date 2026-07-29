#if SDL3_4_0_OR_GREATER

using Sdl3Sharp.Video.Windowing;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents the arguments passed to a <see cref="MouseMotionTransformCallback"/> delegate, containing information about the mouse event and the values to transform
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly ref struct MouseMotionTransformArgs
{
	private readonly ulong mTimestamp;
	private unsafe readonly Window.SDL_Window* mWindow;
	private readonly uint mMouseId;
	private unsafe readonly float* mX;
	private unsafe readonly float* mY;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal unsafe MouseMotionTransformArgs(ulong timestamp, Window.SDL_Window* window, uint mouseId, float* x, float* y)
	{
		mTimestamp = timestamp;
		mWindow = window;
		mMouseId = mouseId;
		mX = x;
		mY = y;
	}

	/// <summary>
	/// Gets the associated timestamp of the mouse event
	/// </summary>
	/// <value>
	/// The associated timestamp of the mouse event, in nanoseconds since the <see cref="Sdl(Sdl.BuildAction?)">initialization of SDL</see>
	/// </value>
	public readonly ulong Timestamp { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mTimestamp; }

	/// <summary>
	/// Gets the associated <see cref="Mouse"/> of the mouse event
	/// </summary>
	/// <value>
	/// The associated <see cref="Mouse"/> of the mouse event, which can be used to query the current state of the mouse device
	/// </value>
	public readonly Mouse Mouse { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new(mMouseId); }

	/// <summary>
	/// Gets a reference to the horizontal motion value to transform
	/// </summary>
	/// <value>
	/// A reference to the horizontal motion value to transform
	/// </value>
	/// <remarks>
	/// <para>
	/// If the <see cref="MouseMotionTransformCallback"/> delegate is used with <see cref="Mouse.RelativeMouseTransform"/>,
	/// you should treat the value of this property as a relative motion delta, which can be scaled or transformed as needed.
	/// </para>
	/// </remarks>
	public readonly ref float X { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get { unsafe { return ref Unsafe.AsRef<float>(mX); } } }

	/// <summary>
	/// Gets a reference to the vertical motion value to transform
	/// </summary>
	/// <value>
	/// A reference to the vertical motion value to transform
	/// </value>
	/// <remarks>
	/// <para>
	/// If the <see cref="MouseMotionTransformCallback"/> delegate is used with <see cref="Mouse.RelativeMouseTransform"/>,
	/// you should treat the value of this property as a relative motion delta, which can be scaled or transformed as needed.
	/// </para>
	/// </remarks>
	public readonly ref float Y { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get { unsafe { return ref Unsafe.AsRef<float>(mY); } } }

	/// <summary>
	/// Tries to get the associated <see cref="Window"/> of the mouse event to which the event was addressed, if any
	/// </summary>
	/// <param name="window">The associated <see cref="Window"/> of the mouse event to which the event was addressed, or <c><see langword="null"/></c> if no window is associated</param>
	/// <returns><c><see langword="true"/></c>, if a window is associated with the mouse event; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// It's quite expensive to retrieve the associated <see cref="Window"/> of the mouse event, 
	/// therefore this method should be used sparingly and only when necessary within a <see cref="MouseMotionTransformCallback"/> delegate.
	/// If you need to call this method, consider caching and reusing the result, as it won't change during the call to the delegate.
	/// </para>
	/// </remarks>
	public readonly bool TryGetWindow([NotNullWhen(true)] out Window? window) { unsafe { return Window.TryGetOrCreate(mWindow, out window); } }
}

#endif