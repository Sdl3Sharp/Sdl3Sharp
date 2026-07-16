using Sdl3Sharp.Events;
using Sdl3Sharp.Internal;
using Sdl3Sharp.Video.Drawing;
using Sdl3Sharp.Video.Windowing.Drivers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents a display connected to the system
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Id"/> of a display is unique, remains unchanged while the display is connected to the system, and is never reused for the lifetime of the application.
/// If a display is disconnected and then reconnected, it will get assigned a new <see cref="Id"/>.
/// </para>
/// <para>
/// For the most part <see cref="Display"/>s are not thread-safe, and most of their properties and methods should only be accessed from the main thread!
/// </para>
/// <para>
/// <see cref="Display"/>s are not driver-agnostic! Most of the time instance of this abstract class are of the concrete <see cref="Display{TDriver}"/> type with a specific <see cref="IWindowingDriver">windowing driver</see> as the type argument.
/// However, the <see cref="Display"/> type exists as a common abstraction.
/// </para>
/// <para>
/// To specify a concrete display type, use <see cref="Display{TDriver}"/> with a windowing driver that implements the <see cref="IWindowingDriver"/> interface (e.g. <see cref="Display{TDriver}">Display&lt;<see cref="Windows">Windows</see>&gt;</see>).
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
public abstract partial class Display : IFormattable, ISpanFormattable
{
	private static readonly ConcurrentDictionary<uint, WeakReference<Display>> mKnownInstances = [];

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private uint mDisplayId;

	private protected Display(uint displayId, bool register)
	{
		mDisplayId = displayId;

		if (register)
		{
			if (displayId is not 0)
			{
				mKnownInstances.AddOrUpdate(displayId, addRef, updateRef, this);
			}

			static WeakReference<Display> addRef(uint id, Display newDisplay) => new(newDisplay);

			static WeakReference<Display> updateRef(uint id, WeakReference<Display> previousDisplayRef, Display newDisplay)
			{
				/*
				 * Displays aren't disposable, so there's nothing to do for us here
				 * 
				if (previousDisplayRef.TryGetTarget(out var previousDisplay))
				{
					// dispose...
				}
				*/

				previousDisplayRef.SetTarget(newDisplay);

				return previousDisplayRef;
			}
		}
	}

	/// <inheritdoc/>
	~Display()
	{
		if (mDisplayId is not 0)
		{
			mKnownInstances.TryRemove(mDisplayId, out _);
		}

		mDisplayId = 0;
	}

	/// <summary>
	/// Gets the desktop area represented by this display
	/// </summary>
	/// <value>
	/// The desktop area represented by this display
	/// </value>
	/// <remarks>
	/// <para>
	/// The primary display is often located at the origin (0, 0) in virtual screen space, but may be placed differently depending on the monitor layout.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the display bounds for the display (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public Rect<int> Bounds
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out Rect<int> rect);

				if (!(bool)SDL_GetDisplayBounds(mDisplayId, &rect))
				{
					failCouldNotGetDisplayBounds();
				}

				return rect;
			}

			[DoesNotReturn]
			static void failCouldNotGetDisplayBounds() => throw new SdlException("Couldn't get the display bounds for the display");
		}
	}

	/// <summary>
	/// Gets a collection of all the displays that are currently connected to the system
	/// </summary>
	/// <value>
	/// A collection of all the displays that are currently connected to the system
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property remains unchanged until an <see cref="EventType.DisplayAdded"/> or <see cref="EventType.DisplayRemoved"/> event is raised.
	/// You should not query this property too often, but rather cache the result and update the cache when such an event is raised.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the displays connected to the system (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public static Display[] ConnectedDisplays
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out int count);

				var displays = SDL_GetDisplays(&count);
				try
				{
					if (displays is null)
					{
						failCouldNotGetDisplays();
					}

					if (count is not > 0)
					{
						return [];
					}

					var result = GC.AllocateUninitializedArray<Display>(count);

					var displayPtr = displays;
					foreach (ref var display in result.AsSpan())
					{
						// We silently assume here that the display ids returned by SDL_GetDisplays() are never 0 (the only case in which TryGetOrCreate would fail)
						TryGetOrCreate(*displayPtr++, out display!);
					}

					return result;
				}
				finally
				{
					Utilities.NativeMemory.SDL_free(displays);
				}
			}

			[DoesNotReturn]
			static void failCouldNotGetDisplays() => throw new SdlException("Couldn't get the displays connected to the system");
		}
	}

	/// <summary>
	/// Gets the content scale for this display
	/// </summary>
	/// <value>
	/// The content scale for this display, or <c>0</c> if the content scale couldn't be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// The content scale is the expected scale for content based on the DPI settings of the display.
	/// For example, a 4K display might have a <c>2.0</c> (200%) display scale, which means that the user expects UI elements to be twice as big on this display, to aid in readability.
	/// </para>
	/// <para>
	/// After window creation, <see cref="Window.DisplayScale"/> should be used to determine the content scale factor for individual windows instead of content scale of the display for a window.
	/// That's because the per-window content scale factor may differ from the content scale factor of the display it is on, particularly on high-DPI and/or multi-monitor desktop configurations.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public float ContentScale
	{
		get
		{
			// we don't throw an exception here, if the content scale is 0,
			// although that's considered an error by SDL, worthy of querying SDL_GetError()
			// -> we just doc, that if the content scale is 0 (which is an non-sensical content scale anyways), the user should check for errors using Error.TryGet(out string?)
			return SDL_GetDisplayContentScale(mDisplayId);
		}
	}

	/// <summary>
	/// Gets the current display mode used by this display
	/// </summary>
	/// <value>
	/// The current display mode used by this display
	/// </value>
	/// <remarks>
	/// <para>
	/// There's a difference between this property and <see cref="DesktopDisplayMode"/> when SDL runs fullscreen and has changed the resolution.
	/// In that case the value of this property reflects the current display mode and not the previous native display mode.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the current display mode for the display (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public DisplayMode CurrentDisplayMode
	{
		get
		{
			unsafe
			{
				if (!DisplayMode.TryGetOrCreateUnmanaged(SDL_GetCurrentDisplayMode(mDisplayId), out var result))
				{
					failDisplayModeNull();
				}

				return result;
			}

			[DoesNotReturn]
			static void failDisplayModeNull() => throw new SdlException("Couldn't get the current display mode for the display");
		}
	}

	/// <summary>
	/// Gets the current orientation of this display
	/// </summary>
	/// <value>
	/// The current orientation of this display, or <see cref="DisplayOrientation.Unknown"/> if the orientation isn't available
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public DisplayOrientation CurrentOrientation
	{
		get
		{
			return SDL_GetCurrentDisplayOrientation(mDisplayId);
		}
	}

	/// <summary>
	/// Gets the desktop display mode for this display
	/// </summary>
	/// <value>
	/// The desktop display mode for this display
	/// </value>
	/// <remarks>
	/// <para>
	/// There's a difference between this property and <see cref="CurrentDisplayMode"/> when SDL runs fullscreen and has changed the resolution.
	/// In that case the value of this property reflects the previous native display mode and not the current display mode.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the desktop display mode for the display (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public DisplayMode DesktopDisplayMode
	{
		get
		{
			unsafe
			{
				if (!DisplayMode.TryGetOrCreateUnmanaged(SDL_GetDesktopDisplayMode(mDisplayId), out var result))
				{
					failDisplayModeNull();
				}

				return result;
			}

			[DoesNotReturn]
			static void failDisplayModeNull() => throw new SdlException("Couldn't get the desktop display mode for the display");
		}
	}

	/// <summary>
	/// Gets a collection of the fullscreen display modes available for this display
	/// </summary>
	/// <value>
	/// A collection of the fullscreen display modes available for this display
	/// </value>
	/// <remarks>
	/// <para>
	/// The collection is sorted with the following priority:
	/// <list type="number">
	///		<item>
	///			<term><see cref="DisplayMode.Width"/></term>
	///			<description>Largest to smallest</description>
	///		</item>
	///		<item>
	///			<term><see cref="DisplayMode.Height"/></term>
	///			<description>Largest to smallest</description>
	///		</item>
	///		<item>
	///			<term><see cref="DisplayMode.Format">Bits per pixel</see></term>
	///			<description>More colors to fewer colors</description>
	///		</item>
	///		<item>
	///			<term><see cref="DisplayMode.Format">Packed pixel layout</see></term>
	///			<description>Largest to smallest</description>
	///		</item>
	///		<item>
	///			<term><see cref="DisplayMode.RefreshRate"/></term>
	///			<description>Highest to lowest</description>
	///		</item>
	///		<item>
	///			<term><see cref="DisplayMode.PixelDensity"/></term>
	///			<description>Lowest to highest</description>
	///		</item>
	///	</list>
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the fullscreen display modes for the display</exception>
	public DisplayMode[] FullscreenDisplayModes
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out int count);

				var modes = SDL_GetFullscreenDisplayModes(mDisplayId, &count);

				if (modes is null)
				{
					failCouldNotGetDisplayModes();
				}

				try
				{
					if (count is not > 0)
					{
						return [];
					}

					var result = GC.AllocateUninitializedArray<DisplayMode>(count);

					var modePtr = modes;
					foreach (ref var mode in result.AsSpan())
					{
						// We silently assume here that the display mode pointers returned by SDL_GetFullscreenDisplayModes() are never null (the only case in which TryGetOrCreateUnmanaged would fail)
						DisplayMode.TryGetOrCreateUnmanaged(*modePtr++, out mode!);
					}

					return result;
				}
				finally
				{
					Utilities.NativeMemory.SDL_free(modes);
				}
			}

			[DoesNotReturn]
			static void failCouldNotGetDisplayModes() => throw new SdlException("Couldn't get the fullscreen display modes for the display");
		}
	}

	/// <summary>
	/// Gets the id of this display
	/// </summary>
	/// <value>
	/// The id of this display
	/// </value>
	/// <remarks>
	/// <para>
	/// The <see cref="Id"/> of a display is unique, remains unchanged while the display is connected to the system, and is never reused for the lifetime of the application.
	/// If a display is disconnected and then reconnected, it will get assigned a new <see cref="Id"/>.
	/// </para>
	/// <para>
	/// An id of <c>0</c> indicates an invalid display.
	/// </para>
	/// </remarks>
	public uint Id { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mDisplayId; }

	/// <summary>
	/// Gets a value indicating whether this display has HDR headroom above the SDR white point
	/// </summary>
	/// <value>
	/// A value indicating whether this display has HDR headroom above the SDR white point
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is for informational and diagnostic purposes only, as not all platforms provide this information at the display level.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public bool IsHdrEnabled => Properties?.TryGetBooleanValue(PropertyNames.HdrEnabledBoolean, out var hdrEnabled) is true
		&& hdrEnabled;

	/// <summary>
	/// Gets the name of this display
	/// </summary>
	/// <value>
	/// The name of this display, or <c><see langword="null"/></c> if the name couldn't be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public string? Name
	{
		get
		{
			unsafe
			{
				// we don't throw an exception here, if the name is null,
				// although that's considered an error by SDL, worthy of querying SDL_GetError()
				// -> we just doc, that if the name is null, the user should check for errors using Error.TryGet(out string?)
				return Utf8StringMarshaller.ConvertToManaged(SDL_GetDisplayName(mDisplayId));
			}
		}
	}

	/// <summary>
	/// Gets the natural orientation of this display
	/// </summary>
	/// <value>
	/// The natural orientation of this display, or <see cref="DisplayOrientation.Unknown"/> if the natural orientation isn't available
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public DisplayOrientation NaturalOrientation
	{
		get
		{
			return SDL_GetNaturalDisplayOrientation(mDisplayId);
		}
	}

	/// <summary>
	/// Gets the primary display of the system
	/// </summary>
	/// <value>
	/// The primary display of the system
	/// </value>
	/// <remarks>
	/// <para>
	/// Accessing this property on a system with no displays connected will always <see langword="throw"/> an <see cref="SdlException"/>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the primary display (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public static Display PrimaryDisplay
	{
		get
		{
			if (!TryGetOrCreate(SDL_GetPrimaryDisplay(), out var result))
			{
				failCouldNotGetPrimaryDisplay();
			}

			return result;

			[DoesNotReturn]
			static void failCouldNotGetPrimaryDisplay() => throw new SdlException("Couldn't get the primary display");
		}
	}

	/// <summary>
	/// Gets the properties associated with this display
	/// </summary>
	/// <value>
	/// The properties associated with this display, or <c><see langword="null"/></c> if the properties could not be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
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
				// we don't throw an exception here, if the properties id is 0,
				// although that's considered an error by SDL, worthy of querying SDL_GetError()
				// -> we just doc, that if the properties id is 0 (the return value is null), the user should check for errors using Error.TryGet(out string?)
				return SDL_GetDisplayProperties(mDisplayId) switch
				{
					0 => null,
					var id => Properties.GetOrCreate(id, registerWithSdl: false)
				};
			}
		}
	}

	/// <summary>
	/// Gets the usable desktop area represented by this display
	/// </summary>
	/// <value>
	/// The usable desktop area represented by this display
	/// </value>
	/// <remarks>
	/// <para>
	/// This is the same area as the <see cref="Bounds"/> property, but with portions reserved by the system removed.
	/// For example, on Apple's macOS, this subtracts the area occupied by the menu bar and dock.
	/// </para>
	/// <para>
	/// Setting a window to be fullscreen generally bypasses these unusable areas, so these are good guidelines for the maximum space available to a non-fullscreen window.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the display usable bounds for the display (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public Rect<int> UsableBounds
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out Rect<int> rect);

				if (!(bool)SDL_GetDisplayUsableBounds(mDisplayId, &rect))
				{
					failCouldNotGetDisplayUsableBounds();
				}

				return rect;
			}

			[DoesNotReturn]
			static void failCouldNotGetDisplayUsableBounds() => throw new SdlException("Couldn't get the display usable bounds for the display");
		}
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {nameof(Id)}: {mDisplayId.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite($"{{ {nameof(Id)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mDisplayId, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}

	/// <summary>
	/// Tries to get the closest fullscreen match to the specified display mode for this display
	/// </summary>
	/// <param name="width">The desired width of the display mode, in pixels</param>
	/// <param name="height">The desired height of the display mode, in pixels</param>
	/// <param name="displayMode">The resulting closest matching display mode that is equal to or larger than the specified display mode, if this method returns <c><see langword="true"/></c></param>
	/// <param name="refreshRate">The desired refresh rate of the display mode, in Hz (hertz), or <c>0</c> (the default) for the desktop refresh rate</param>
	/// <param name="includeHighDensityModes">A value indicating whether high-density display modes should be included in the search</param>
	/// <returns><c><see langword="true"/></c>, if a matching display mode was found successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The available display modes are searched and the <paramref name="displayMode"/> parameter will be filled in with the closest mode matching the requested display mode.
	/// If <paramref name="refreshRate"/> is <c>0</c> then the requested refresh rate will default to the desktop refresh rate.
	/// The display modes are searched with size being a first priority, format being a second priority, and finally checking the refresh rate.
	/// If all the available display modes are too small, then <c><see langword="false"/></c> is returned.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryGetClosestFullscreenDisplayMode(int width, int height, out DisplayMode displayMode, float refreshRate = 0, bool includeHighDensityModes = false)
	{
		unsafe
		{
			fixed (DisplayMode.SDL_DisplayMode* displayModePtr = &DisplayMode.CreateManaged(out displayMode))
			{
				return SDL_GetClosestFullscreenDisplayMode(mDisplayId, width, height, refreshRate, includeHighDensityModes, displayModePtr);
			}
		}
	}

	internal static bool TryGetOrCreate(uint displayId, [NotNullWhen(true)] out Display? result)
	{
		if (displayId is 0)
		{
			result = null;
			return false;
		}

		var displayRef = mKnownInstances.GetOrAdd(displayId, createRef);

		if (!displayRef.TryGetTarget(out result))
		{
			displayRef.SetTarget(result = create(displayId));
		}

		return true;

		static WeakReference<Display> createRef(uint displayId) => new(create(displayId));

		static Display create(uint displayId)
		{
			if (!TryCreateFromRegisteredDriver(displayId, register: false, out var result))
			{
				result = new Display<GenericFallbackWindowingDriver>(displayId, register: false);
			}

			return result;
		}
	}

	internal static bool TryGetOrCreate<TDriver>(uint displayId, [NotNullWhen(true)] out Display<TDriver>? result)
		where TDriver : IWindowingDriver
	{
		if (displayId is 0)
		{
			result = null;
			return false;
		}

		var displayRef = mKnownInstances.GetOrAdd(displayId, createRef);

		if (!displayRef.TryGetTarget(out var baseResult))
		{
			displayRef.SetTarget(result = create(displayId));
		}
		else if (baseResult is Display<TDriver> typedResult)
		{
			// that's just fine

			result = typedResult;
		}
		else
		{
			// we just overwrite what's already there, assuming that the caller knows better about which driver the display should be associated with

			displayRef.SetTarget(result = create(displayId));
		}

		return true;

		static WeakReference<Display> createRef(uint displayId) => new(create(displayId));

		static Display<TDriver> create(uint displayId)
		{
			return new(displayId, register: false);
		}
	}

	/// <summary>
	/// Tries to get the display that contains the specified point in screen space
	/// </summary>
	/// <param name="point">The point in screen space to get the display for</param>
	/// <param name="display">The display that contains the specified point in screen space, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the display containing the specified point in screen space was found successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryGetFromPoint(in Point<int> point, [NotNullWhen(true)] out Display? display)
	{
		unsafe
		{
			fixed (Point<int>* pointPtr = &point)
			{
				return TryGetOrCreate(SDL_GetDisplayForPoint(pointPtr), out display);
			}
		}
	}

	/// <summary>
	/// Tries to get the display that contains the majority of the specified rectangle in screen space
	/// </summary>
	/// <param name="rect">The rectangle in screen space to get the display for</param>
	/// <param name="display">The display that contains the majority of the specified rectangle in screen space, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if a display containing the majority of the specified rectangle in screen space was found successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static bool TryGetFromRect(in Rect<int> rect, [NotNullWhen(true)] out Display? display)
	{
		unsafe
		{
			fixed (Rect<int>* rectPtr = &rect)
			{
				return TryGetOrCreate(SDL_GetDisplayForRect(rectPtr), out display);
			}
		}
	}
}
