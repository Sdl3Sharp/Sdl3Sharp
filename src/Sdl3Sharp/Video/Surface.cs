using Sdl3Sharp.Internal;
using Sdl3Sharp.IO;
using Sdl3Sharp.Utilities;
using Sdl3Sharp.Video.Blending;
using Sdl3Sharp.Video.Coloring;
using Sdl3Sharp.Video.Drawing;
using Sdl3Sharp.Video.Rendering;
using Sdl3Sharp.Video.Rendering.Drivers;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.Video;

/// <summary>
/// Represents a buffer of pixels in memory, used for software rendering and as a source for textures
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Surface"/>s make serious efforts to manage images in various formats, and provide a reasonable toolbox for transforming the data, including copying between <see cref="Surface"/>s, filling rectangles in the image data, etc.
/// </para>
/// </remarks>
public partial class Surface : IDisposable
{
	private interface IUnsafeConstructorDispatch;

	private static readonly ConcurrentDictionary<IntPtr, WeakReference<Surface>> mKnownInstances = [];

	/// <exception cref="ArgumentException">
	/// <paramref name="pixels"/> is <see cref="Utilities.NativeMemory.IsValid">invalid</see>
	/// - or -
	/// <paramref name="pixels"/> is too small
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static unsafe void* ValidateAndPinNativeMemory(Utilities.NativeMemory pixels, int height, int pitch, out NativeMemoryPin nativeMemoryPin)
	{
		if (!pixels.IsValid)
		{
			failPixelsArgumentInvalid();
		}

		var required = unchecked((height is > 0 ? (nuint)height : 0) * (pitch is > 0 ? (nuint)pitch : 0));

		if (pixels.Length < required)
		{
			failPixelsArgumentTooSmall(required);
		}

		nativeMemoryPin = pixels.Pin();
		
		return pixels.RawPointer;

		[DoesNotReturn]
		static void failPixelsArgumentInvalid() => throw new ArgumentException(message: $"{nameof(pixels)} is invalid", paramName: nameof(pixels));

		[DoesNotReturn]
		static void failPixelsArgumentTooSmall(nuint required) => throw new ArgumentException(message: $"{nameof(pixels)} is too small. The buffer must be at least {required} in size.", paramName: nameof(pixels));
	}

	/// <exception cref="ArgumentException"><paramref name="pixels"/> is too small</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static unsafe void* ValidateAndPinMemory(Memory<byte> pixels, int height, int pitch, out MemoryHandle memoryHandle)
	{
		var required = unchecked((height is > 0 ? (nuint)height : 0) * (pitch is > 0 ? (nuint)pitch : 0));

		if (unchecked((nuint)pixels.Length) < required)
		{
			failPixelsArgumentTooSmall(required);
		}

		memoryHandle = pixels.Pin();

		return memoryHandle.Pointer;

		[DoesNotReturn]
		static void failPixelsArgumentTooSmall(nuint required) => throw new ArgumentException(message: $"{nameof(pixels)} is too small. The buffer must be at least {required} in size.", paramName: nameof(pixels));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private unsafe static byte* Utf8Convert(string? str, out byte* strUtf8) => strUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(str);

	private unsafe SDL_Surface* mSurface;
	private NativeMemoryPin? mNativeMemoryPin = null;
	private MemoryHandle mMemoryHandle = default;
	private Palette? mPalette = null; // Use this to keep the managed Palette alive

	/// <remarks>
	/// <para>
	/// Use this only for newly created SDL surfaces that start with native <see cref="SDL_Surface.RefCount"/> equal to <c>1</c>;
	/// do not call <see cref="TryGetOrCreate"/> for such owner instances.
	/// </para>
	/// </remarks>
	internal unsafe Surface(SDL_Surface* surface, bool register)
	{
		mSurface = surface;

		if (register)
		{
			if (surface is not null)
			{
				// Neither addRef nor updateRef increase the native ref counter for a very simple reason:
				// If we're on this constructor path, we created the native instance ourselves, so its ref counter is already set to 1.
				// That's totally right, since atm the managed wrapper is the sole borrower of a reference to the native instance.

				mKnownInstances.AddOrUpdate(unchecked((IntPtr)mSurface), addRef, updateRef, this);
			}

			static WeakReference<Surface> addRef(IntPtr surface, Surface newSurface) => new(newSurface);

			static WeakReference<Surface> updateRef(IntPtr surface, WeakReference<Surface> previousSurfaceRef, Surface newSurface)
			{
				if (previousSurfaceRef.TryGetTarget(out var previousSurface))
				{
#pragma warning disable IDE0079
#pragma warning disable CA1816
					GC.SuppressFinalize(previousSurface);
#pragma warning restore CA1816
#pragma warning restore IDE0079

					// Dispose calls SDL_DestroySurface and already decreases the ref count, so we don't need to do it here manually
					previousSurface.Dispose(disposing: true, forget: false);
				}

				previousSurfaceRef.SetTarget(newSurface);

				return previousSurfaceRef;
			}
		}
	}

	/// <exception cref="SdlException">The <see cref="Surface"/> could not be created (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	private unsafe Surface(int width, int height, PixelFormat format, IUnsafeConstructorDispatch? _ = default) :
		this(SDL_CreateSurface(width, height, format), register: true)
	{
		if (mSurface is null)
		{
			failCouldNotCreateSurface();
		}

		[DoesNotReturn]
		static void failCouldNotCreateSurface() => throw new SdlException($"Could not create the {nameof(Surface)}");
	}

	/// <summary>
	/// Creates a new <see cref="Surface"/> with the specified width, height and pixel format
	/// </summary>
	/// <param name="width">The width of the surface</param>
	/// <param name="height">The height of the surface</param>
	/// <param name="format">The pixel format of the surface</param>
	/// <remarks>
	/// <para>
	/// The pixel memory of the newly created surface is initialized to all zeroes.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="Surface(int, int, PixelFormat, IUnsafeConstructorDispatch?)"/>
	public Surface(int width, int height, PixelFormat format) :
#pragma warning disable IDE0034 // Keep it that way for explicitness sake
		this(width, height, format, default(IUnsafeConstructorDispatch?))
#pragma warning restore IDE0034
	{ }

	/// <inheritdoc cref="ValidateAndPinNativeMemory(Utilities.NativeMemory, int, int, out NativeMemoryPin)"/>
	private unsafe Surface(int width, int height, PixelFormat format, Utilities.NativeMemory pixels, int pitch, IUnsafeConstructorDispatch? _ = default) :
		this(width, height, format, ValidateAndPinNativeMemory(pixels, height, pitch, out var nativeMemoryPin), pitch)
	{
		mNativeMemoryPin = nativeMemoryPin;
	}

	/// <summary>
	/// Creates a new <see cref="Surface"/> with the specified width, height, pixel format and pre-allocated pixel memory
	/// </summary>
	/// <param name="width">The width of the surface</param>
	/// <param name="height">The height of the surface</param>
	/// <param name="format">The pixel format of the surface</param>
	/// <param name="pixels">The pre-allocated pixel memory</param>
	/// <param name="pitch">The pitch of the surface, in bytes</param>
	/// <remarks>
	/// <para>
	/// No copy is made of the <paramref name="pixels"/>, instead it's referenced directly as the pixel memory of the newly created surface.
	/// That also means that <paramref name="pixels"/> must remain valid and alive for the lifetime of the surface. This is not handled automatically for you!
	/// </para>
	/// <para>
	/// The <paramref name="pitch"/> is the offset in bytes between each vertical pixel row in the <paramref name="pixels"/> memory, e.g. <c>4 * <paramref name="width"/></c> for a <see cref="PixelFormat.Rgba8888"/> <paramref name="format"/>.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="Surface(int, int, PixelFormat, Utilities.NativeMemory, int, IUnsafeConstructorDispatch?)"/>
	public Surface(int width, int height, PixelFormat format, Utilities.NativeMemory pixels, int pitch) :
#pragma warning disable IDE0034 // Keep it that way for explicitness sake
		this(width, height, format, pixels, pitch, default(IUnsafeConstructorDispatch?))
#pragma warning restore IDE0034
	{ }

	/// <inheritdoc cref="ValidateAndPinMemory(Memory{byte}, int, int, out MemoryHandle)"/>
	private unsafe Surface(int width, int height, PixelFormat format, Memory<byte> pixels, int pitch, IUnsafeConstructorDispatch? _ = default) :
		this(width, height, format, ValidateAndPinMemory(pixels, height, pitch, out var memoryHandle), pitch)
	{
		mMemoryHandle = memoryHandle;
	}

	/// <summary>
	/// Creates a new <see cref="Surface"/> with the specified width, height, pixel format and pre-allocated pixel memory
	/// </summary>
	/// <param name="width">The width of the surface</param>
	/// <param name="height">The height of the surface</param>
	/// <param name="format">The pixel format of the surface</param>
	/// <param name="pixels">The pre-allocated pixel memory</param>
	/// <param name="pitch">The pitch of the surface, in bytes</param>
	/// <remarks>
	/// <para>
	/// No copy is made of the <paramref name="pixels"/>, instead it's referenced directly as the pixel memory of the newly created surface.
	/// That also means that <paramref name="pixels"/> must remain valid and alive for the lifetime of the surface. This is not handled automatically for you!
	/// </para>
	/// <para>
	/// The <paramref name="pitch"/> is the offset in bytes between each vertical pixel row in the <paramref name="pixels"/> memory, e.g. <c>4 * <paramref name="width"/></c> for a <see cref="PixelFormat.Rgba8888"/> <paramref name="format"/>.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="Surface(int, int, PixelFormat, Memory{byte}, int, IUnsafeConstructorDispatch?)"/>
	public Surface(int width, int height, PixelFormat format, Memory<byte> pixels, int pitch) :
#pragma warning disable IDE0034 // Keep it that way for explicitness sake
		this(width, height, format, pixels, pitch, default(IUnsafeConstructorDispatch?))
#pragma warning restore IDE0034
	{ }

	/// <summary>
	/// Creates a new <see cref="Surface"/> with the specified width, height, pixel format and pre-allocated pixel memory
	/// </summary>
	/// <param name="width">The width of the surface</param>
	/// <param name="height">The height of the surface</param>
	/// <param name="format">The pixel format of the surface</param>
	/// <param name="pixels">A pointer to the pre-allocated pixel memory</param>
	/// <param name="pitch">The pitch of the surface, in bytes</param>
	/// <remarks>
	/// <para>
	/// No copy is made of the <paramref name="pixels"/>, instead it's referenced directly as the pixel memory of the newly created surface.
	/// That also means that <paramref name="pixels"/> must remain valid and alive for the lifetime of the surface. This is not handled automatically for you!
	/// </para>
	/// <para>
	/// The <paramref name="pitch"/> is the offset in bytes between each vertical pixel row in the <paramref name="pixels"/> memory, e.g. <c>4 * <paramref name="width"/></c> for a <see cref="PixelFormat.Rgba8888"/> <paramref name="format"/>.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">The <see cref="Surface"/> could not be created (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public unsafe Surface(int width, int height, PixelFormat format, void* pixels, int pitch) :
		this(SDL_CreateSurfaceFrom(width, height, format, pixels, pitch), register: true)
	{
		if (mSurface is null)
		{
			failCouldNotCreateSurface();
		}

		[DoesNotReturn]
		static void failCouldNotCreateSurface() => throw new SdlException($"Could not create the {nameof(Surface)}");
	}

#if SDL3_4_0_OR_GREATER
	/// <exception cref="SdlException">The <see cref="Surface"/> could not be created (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	private unsafe Surface(string file, IUnsafeConstructorDispatch? _ = default) :
		this(SDL_LoadSurface(Utf8Convert(file, out var fileUtf8)), register: true)
	{
		try
		{
			if (mSurface is null)
			{
				failCouldNotCreateSurface();
			}
		}
		finally
		{
			Utf8StringMarshaller.Free(fileUtf8);
		}

		[DoesNotReturn]
		static void failCouldNotCreateSurface() => throw new SdlException($"Could not create the {nameof(Surface)}");
	}

	/// <summary>
	/// Creates a new <see cref="Surface"/> by loading a BMP or PNG image from the specified file
	/// </summary>
	/// <param name="file">A file path to the image</param>
	/// <remarks>
	/// <para>
	/// The format of the file must be BMP or PNG, otherwise this constructor will fail.
	/// </para>
	/// <para>
	/// Remember to <see cref="Dispose()">dispose</see> of the resulting surface when you are done with it. Not doing so will result in a memory leak.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="Surface(string, IUnsafeConstructorDispatch?)"/>
	public Surface(string file) :
#pragma warning disable IDE0034 // Keep it that way for explicitness sake
		this(file, default(IUnsafeConstructorDispatch?))
#pragma warning restore IDE0034
	{ }
#endif

#if SDL3_4_0_OR_GREATER
	/// <exception cref="SdlException">The <see cref="Surface"/> could not be created (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	private unsafe Surface(Stream source, bool closeAfterwards, IUnsafeConstructorDispatch? _ = default) :
		this(SDL_LoadSurface_IO(source is not null ? source.Pointer : null, closeAfterwards), register: true)
	{
		try
		{
			if (mSurface is null)
			{
				failCouldNotCreateSurface();
			}
		}
		finally
		{
			if (closeAfterwards)
			{
				source?.Dispose(close: false /* SDL_LoadSurface_IO already closed the stream */);
			}
		}

		[DoesNotReturn]
		static void failCouldNotCreateSurface() => throw new SdlException($"Could not create the {nameof(Surface)}");
	}

	/// <summary>
	/// Creates a new <see cref="Surface"/> by loading a BMP or PNG image from the specified stream
	/// </summary>
	/// <param name="source">The stream to load the image from</param>
	/// <param name="closeAfterwards">A value indicating whether the <paramref name="source"/> stream should be closed before this method returns (even in case of an error)</param>
	/// <remarks>
	/// <para>
	/// The format of the data in the <paramref name="source"/> stream must be BMP or PNG, otherwise this constructor will fail.
	/// </para>
	/// <para>
	/// Remember to <see cref="Dispose()">dispose</see> of the resulting surface when you are done with it. Not doing so will result in a memory leak.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="Surface(Stream, bool, IUnsafeConstructorDispatch?)"/>
	public Surface(Stream source, bool closeAfterwards = false) :
#pragma warning disable IDE0034 // Keep it that way for explicitness sake
		this(source, closeAfterwards, default(IUnsafeConstructorDispatch?))
#pragma warning restore IDE0034
	{ }
#endif

	/// <inheritdoc/>
	~Surface() => Dispose(disposing: false, forget: true);

	/// <summary>
	/// Gets or sets the alpha modulation value used in blit operations
	/// </summary>
	/// <value>
	/// The alpha modulation value used in blit operations
	/// </value>
	/// <remarks>
	/// <para>
	/// When this surface is blitted, during the blit operation the source alpha value is modulated by this alpha value according to the following formula:
	///	<code>srcA = srcA * (alpha / 255)</code>
	/// </para>
	/// </remarks>
	public byte AlphaMod
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out byte alpha);

				SDL_GetSurfaceAlphaMod(mSurface, &alpha);

				return alpha;
			}
		}
		
		set
		{
			unsafe
			{
				SDL_SetSurfaceAlphaMod(mSurface, value);
			}
		}
	}

	/// <summary>
	/// Gets or sets the blend mode used for blit operations
	/// </summary>
	/// <value>
	/// The blend mode used for blit operations
	/// </value>
	/// <remarks>
	/// <para>
	/// To copy a surface to another surface without blending with the existing data, the <see cref="BlendMode"/> of the source surface should be set to <see cref="BlendMode.None"/>.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting this property, the specified blend mode is <see cref="BlendMode.Invalid"/> or not supported by the platform (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </exception>
	public BlendMode BlendMode
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out BlendMode blendMode);

				SDL_GetSurfaceBlendMode(mSurface, &blendMode);

				return blendMode;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetSurfaceBlendMode(mSurface, value), filterError: GetInvalidSurfaceErrorMessage());
				// throws if value is BlendMode.Invalid or value is an unsupported blend mode
			}
		}
	}

	/// <summary>
	/// Gets or sets the clipping rectangle for the surface
	/// </summary>
	/// <value>
	/// The clipping rectangle for the surface
	/// </value>
	/// <remarks>
	/// <para>
	/// When this surface is blitted onto, only the area within the clipping rectangle is drawn into.
	/// Note that blits are automatically clipped to the edges of the source and destination surfaces.
	/// </para>
	/// <para>
	/// If the value of this property is equal to the surface's bounds, clipping is effectively disabled.
	/// </para>
	/// <para>
	/// Alternatively, you can use the methods <see cref="ResetClippingRect"/> and <see cref="SetClippingRect(in Rect{int})"/> to manipulate the clipping rectangle.
	/// The latter returns a value indicating whether the set clipping rectangle intersects the surface area.
	/// </para>
	/// </remarks>
	public Rect<int> ClippingRect
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out Rect<int> rect);

				SDL_GetSurfaceClipRect(mSurface, &rect);

				return rect;
			}
		}

		set
		{
			unsafe
			{
				SDL_SetSurfaceClipRect(mSurface, &value);
			}
		}	
	}

	/// <summary>
	/// Gets or sets the color key (transparent pixel value) in the surface
	/// </summary>
	/// <value>
	/// The color key (transparent pixel value) in the surface, as an encoded pixel value used by the surface, or <c><see langword="null"/></c> if color keying is disabled
	/// </value>
	/// <remarks>
	/// <para>
	/// The color key defines a pixel value that will be treated as transparent in a blit.
	/// For example, one can use this to specify that cyan pixels should be considered transparent, and therefore not rendered.
	/// </para>
	/// <para>
	/// The value of this property uses the encoded pixel value format used by the surface, as generated by <see cref="MapColor(byte, byte, byte)"/>.
	/// </para>
	/// <para>
	/// Alternatively, if you just want to check whether color keying is enabled, you can use the <see cref="HasColorKey"/> property instead.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting this property, the specified color key is invalid for the surface's pixel format (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// For example, this can happen when the key is out of range for palettized formats.
	/// </exception>
	public uint? ColorKey
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out uint key);

				return SDL_GetSurfaceColorKey(mSurface, &key)
					? key
					: null;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetSurfaceColorKey(mSurface, enabled: value.HasValue, key: value.GetValueOrDefault()), filterError: GetInvalidSurfaceErrorMessage());
				// throws if the key as palette index is invalid
			}
		}
	}

	/// <summary>
	/// Gets or sets the color modulation values used in blit operations
	/// </summary>
	/// <value>
	/// The color modulation values used in blit operations
	/// </value>
	/// <remarks>
	/// <para>
	/// When this surface is blitted, during the blit operation each source color channel is modulated by the appropriate color value according to the following formula:
	/// <code>srcC = srcC * (color / 255)</code>
	/// </para>
	/// </remarks>
	public (byte R, byte G, byte B) ColorMod
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (byte R, byte G, byte B) color);

				SDL_GetSurfaceColorMod(mSurface, &color.R, &color.G, &color.B);

				return color;
			}
		}

		set
		{
			unsafe
			{
				SDL_SetSurfaceColorMod(mSurface, value.R, value.G, value.B);
			}
		}
	}

	/// <summary>
	/// Gets or sets the color space of the surface
	/// </summary>
	/// <value>
	/// The color space of the surface
	/// </value>
	/// <remarks>
	/// <para>
	/// The color space defaults to <see cref="ColorSpace.SrgbLinear"/> for floating point formats, <see cref="ColorSpace.Hdr10"/> for 10-bit formats, <see cref="ColorSpace.Srgb"/> for other RGB surfaces and <see cref="ColorSpace.Bt709Full"/> for YUV textures.
	/// </para>
	/// <para>
	/// Setting the color space doesn't change the pixels, only how they are interpreted in color operations.
	/// If you want to convert the pixels to a different color space, use an appropriate one of the TryConvert* methods.
	/// </para>
	/// </remarks>
	public ColorSpace ColorSpace
	{
		get
		{
			unsafe
			{
				return SDL_GetSurfaceColorspace(mSurface);
			}
		}

		set
		{
			unsafe
			{
				SDL_SetSurfaceColorspace(mSurface, value);
			}
		}
	}

	/// <summary>
	/// Gets the surface flags
	/// </summary>
	/// <value>
	/// The surface flags
	/// </value>
	/// <remarks>
	/// <para>
	/// Instead of examining the flags directly, you can use the specific properties <see cref="IsLocked"/>, <see cref="IsPreAllocated"/>, <see cref="IsSimdAligned"/>, and <see cref="MustLock"/> to check for particular surface characteristics.
	/// </para>
	/// </remarks>
	public SurfaceFlags Flags
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
				return mSurface is var surface
					&& surface is not null
						? surface->Flags
						: default;
			}
		}
	}

	/// <summary>
	/// Gets the pixel format of the surface
	/// </summary>
	/// <value>
	/// The pixel format of the surface
	/// </value>
	public PixelFormat Format
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
				return mSurface is var surface
					&& surface is not null
						? surface->Format
						: default;
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the surface has alternate images
	/// </summary>
	/// <value>
	/// <c><see langword="true"/></c>, if alternate versions of the surface are available; otherwise, <c><see langword="false"/></c>
	/// </value>
	public bool HasAlternateImages
	{
		get
		{
			unsafe
			{
				return SDL_SurfaceHasAlternateImages(mSurface);
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the surface has a color key (transparent pixel value) set
	/// </summary>
	/// <value>
	/// <c><see langword="true"/></c>, if the surface has a color key set; otherwise, <c><see langword="false"/></c>
	/// </value>
	public bool HasColorKey
	{
		get
		{
			unsafe
			{
				return SDL_SurfaceHasColorKey(mSurface);
			}
		}
	}

	/// <summary>
	/// Gets or sets the maximum dynamic range, in terms of the <see cref="SdrWhitePoint">SDR white point</see>
	/// </summary>
	/// <value>
	/// The maximum dynamic range, in terms of the <see cref="SdrWhitePoint">SDR white point</see>
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is used for HDR10 and floating point <see cref="Surface"/>s.
	/// </para>
	/// <para>
	/// The value of this property defines the maximum dynamic range used by the content, in terms of the <see cref="SdrWhitePoint">SDR white point</see>.
	/// </para>
	/// <para>
	/// The value defaults to <c>0.0</c>, which disables <see cref="TonemapOperator">tone mapping</see>.
	/// </para>
	/// </remarks>
	public float HdrHeadroom
	{
		get => Properties?.TryGetFloatValue(PropertyNames.HdrHeadroomFloat, out var hdrHeadroom) is true
			? hdrHeadroom
			: default;

		set => Properties?.TrySetFloatValue(PropertyNames.HdrHeadroomFloat, value);
	}

	/// <summary>
	/// Gets the height of the surface, in pixels
	/// </summary>
	/// <value>
	/// The height of the surface, in pixels
	/// </value>
	public int Height
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
				return mSurface is var surface
					&& surface is not null
						? surface->H
						: default;
			}
		}
	}

	/// <summary>
	/// Gets or sets the hotspot pixel offset from the left edge for a cursor
	/// </summary>
	/// <value>
	/// The hotspot pixel offset from the left edge for a cursor
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property defines the pixel offset to the hotspot from the left edge of the <see cref="Surface"/> used as a cursor.
	/// </para>
	/// </remarks>
	public long HotspotX
	{
		get => Properties?.TryGetNumberValue(PropertyNames.HotspotXNumber, out var hotspotX) is true
			? hotspotX
			: default;

		set => Properties?.TrySetNumberValue(PropertyNames.HotspotXNumber, value);
	}

	/// <summary>
	/// Gets or sets the hotspot pixel offset from the top edge for a cursor
	/// </summary>
	/// <value>
	/// The hotspot pixel offset from the top edge for a cursor
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property defines the pixel offset to the hotspot from the top edge of the <see cref="Surface"/> used as a cursor.
	/// </para>
	/// </remarks>
	public long HotspotY
	{
		get => Properties?.TryGetNumberValue(PropertyNames.HotspotYNumber, out var hotspotY) is true
			? hotspotY
			: default;

		set => Properties?.TrySetNumberValue(PropertyNames.HotspotYNumber, value);
	}

	/// <summary>
	/// Gets a list of all versions of the surface
	/// </summary>
	/// <value>
	/// A list of all versions of the surface, including itself
	/// </value>
	/// <remarks>
	/// <para>
	/// The returned array includes all alternate images of the surface, including itself as the first element.
	/// </para>
	/// <para>
	/// Reading this property can be very expensive, you should consider caching it's value and reusing it as long as you're sure that the alternate images of the surface don't change.
	/// </para>
	/// <para>
	/// You can manipulate the list of alternate images of a surface using the <see cref="TryAddAlternateImage(Surface)"/> and <see cref="ClearAlternateImages"/> methods.
	/// </para>
	/// </remarks>
	public Surface[] Images
	{
		get
		{
			unsafe
			{
				if (mSurface is var surface
					&& surface is null)
				{
					return [];
				}

				Unsafe.SkipInit(out int count);

				var images = SDL_GetSurfaceImages(mSurface, &count);
				
				if (images is null || count is not > 0)
				{
					return [];
				}				

				var result = GC.AllocateUninitializedArray<Surface>(count);

				var imagePtr = images;
				foreach (ref var image in result.AsSpan())
				{
					TryGetOrCreate(*imagePtr++, out image!);
				}

				Utilities.NativeMemory.SDL_free(images);

				return result;
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the surface is currently locked
	/// </summary>
	/// <value>
	/// <c><see langword="true"/></c>, if the surface is currently locked; otherwise, <c><see langword="false"/></c>
	/// </value>
	/// <remarks>
	/// <para>
	/// It's safe to access <see cref="UnsafePixels"/> only if this property is <c><see langword="true"/></c>.
	/// </para>
	/// <para>
	/// This property is meant to be used as part of the <see cref="MustLock"/> - <see cref="IsLocked"/> - <see cref="TryUnsafeLock"/> - <see cref="UnsafePixels"/> - <see cref="UnsafeUnlock"/> pattern,
	/// if you want to access the surface's pixel memory directly in a faster and more efficient way.
	/// If you're looking for a simpler and safer way to access the pixel memory, consider using <see cref="TryLock(out SurfacePixelMemoryManager?)"/> instead.
	/// </para>
	/// </remarks>
	public bool IsLocked
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
#pragma warning disable IDE0075 // It's more readable that way
				return mSurface is var surface
					&& surface is not null
					? (surface->Flags & SurfaceFlags.Locked) is SurfaceFlags.Locked
					: default;
#pragma warning restore IDE0075
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the surface uses pre-allocated pixel memory
	/// </summary>
	/// <value>
	/// <c><see langword="true"/></c>, if the surface uses pre-allocated pixel memory; otherwise, <c><see langword="false"/></c>
	/// </value>
	/// <remarks>
	/// <para>
	/// A surface uses pre-allocated pixel memory, for example, when it was created using <see cref="Surface(int, int, PixelFormat, Utilities.NativeMemory, int)"/>, <see cref="Surface(int, int, PixelFormat, Memory{byte}, int)"/>, or <see cref="Surface(int, int, PixelFormat, void*, int)"/>.
	/// </para>
	/// </remarks>
	public bool IsPreAllocated
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
#pragma warning disable IDE0075 // It's more readable that way
				return mSurface is var surface
					&& surface is not null
					? (surface->Flags & SurfaceFlags.PreAllocated) is SurfaceFlags.PreAllocated
					: default;
#pragma warning restore IDE0075
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the surface uses run-length encoding 
	/// </summary>
	/// <value>
	/// <c><see langword="true"/></c>, if the surface uses run-length encoding; otherwise, <c><see langword="false"/></c>
	/// </value>
	/// <remarks>
	/// <para>
	/// If RLE is enabled, color key and alpha blending blits are much faster, but the surface must be locked before directly accessing the pixels.
	/// </para>
	/// <para>
	/// When setting this property, it silently fails if the surface's pixel format is a <see cref="PixelFormatExtensions.get_IsFourCC(PixelFormat)">FourCC</see> format.
	/// </para>
	/// </remarks>
	public bool IsRle
	{
		get
		{
			unsafe
			{
				return SDL_SurfaceHasRLE(mSurface);
			}
		}

		set
		{
			unsafe
			{
				SDL_SetSurfaceRLE(mSurface, value);
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the surface uses pixel memory that's aligned for SIMD operations
	/// </summary>
	/// <value>
	/// <c><see langword="true"/></c>, if the surface uses pixel memory that's aligned for SIMD operations; otherwise, <c><see langword="false"/></c>
	/// </value>
	public bool IsSimdAligned
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
#pragma warning disable IDE0075 // It's more readable that way
				return mSurface is var surface
					&& surface is not null
					? (surface->Flags & SurfaceFlags.SimdAligned) is SurfaceFlags.SimdAligned
					: default;
#pragma warning restore IDE0075
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the surface must be locked before accessing it's pixel memory
	/// </summary>
	/// <value>
	/// <c><see langword="true"/></c>, if the surface must be locked before accessing it's pixel memory; otherwise, <c><see langword="false"/></c>
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is meant to be used as part of the <see cref="MustLock"/> - <see cref="IsLocked"/> - <see cref="TryUnsafeLock"/> - <see cref="UnsafePixels"/> - <see cref="UnsafeUnlock"/> pattern,
	/// if you want to access the surface's pixel memory directly in a faster and more efficient way.
	/// If you're looking for a simpler and safer way to access the pixel memory, consider using <see cref="TryLock(out SurfacePixelMemoryManager?)"/> instead.
	/// </para>
	/// <para>
	/// Not all surfaces require locking. If the value of this property is <c><see langword="false"/></c>,
	/// then you can read and write to the surface at any time, and the pixel format of the surface will not change.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Surface surface;
	/// 
	/// ...
	/// 
	/// if (surface.MustLock &amp;&amp; surface.TryUnsafeLock())
	/// {
	///		// It's safe to access surface.UnsafePixels here
	///		
	///		...
	///		
	///		surface.UnsafeUnlock(); 
	/// }
	/// </code>
	/// </example>
	public bool MustLock
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
#pragma warning disable IDE0075 // It's more readable that way
				return mSurface is var surface
					&& surface is not null
					? (surface->Flags & SurfaceFlags.LockNeeded) is SurfaceFlags.LockNeeded
					: default;
#pragma warning restore IDE0075
			}
		}
	}

	/// <summary>
	/// Gets or sets the palette used by the surface
	/// </summary>
	/// <value>
	/// The palette used by the surface, or <c><see langword="null"/></c> if the surface doesn't use a palette
	/// </value>
	/// <remarks>
	/// <para>
	/// A single <see cref="Coloring.Palette"/> can be shared between multiple <see cref="Surface"/>s.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting this property, the specified palette doesn't match the surface's pixel format (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </exception>
	public Palette? Palette
	{
		get
		{
			unsafe
			{
				if (!Palette.TryGetOrCreate(SDL_GetSurfacePalette(mSurface), out var palette))
				{
					return null;
				}

				return palette;
			}
		}

		set
		{
			unsafe
			{
				// We don't need to worry about SDL_SetSurfacePalette potentially destroying the old palette,
				// as we should handle that correctly and consistently via the way we handle ref counting in the managed Palette wrapper.
				// If SDL_SetSurfacePalette does destroy the old palette, there shouldn't be a registered managed wrapper around it anymore,
				// and if there would be a registered managed wrapper, SDL_SetSurfacePalette shouldn't destroy the native instance yet (as it's ref counter shouldn't go to zero).

				SdlErrorHelper.ThrowIfFailed(SDL_SetSurfacePalette(mSurface, value is not null ? value.Pointer : null), filterError: GetInvalidSurfaceErrorMessage());
			}
		}
	}

	/// <summary>
	/// Gets the pitch of the surface, in bytes
	/// </summary>
	/// <value>
	/// The pitch of the surface, in bytes
	/// </value>
	/// <remarks>
	/// <para>
	/// The pitch is the length of a single row of pixels in bytes.
	/// This value may be different from the width of the surface times the number of bytes per pixel for packing and alignment purposes.
	/// </para>
	/// <para>
	/// Pixels are arranged in memory in rows, with the top row first. Each row occupies an amount of memory given by the pitch (sometimes known as the "row stride").
	/// </para>
	/// <para>
	/// Within each row, pixels are arranged from left to right until the width is reached.
	/// Each pixel occupies a number of bits appropriate for its format, with most formats representing each pixel as one or more whole bytes (in some indexed formats, instead multiple pixels are packed into each byte), and a byte order given by the format.
	/// After encoding all pixels, any remaining bytes to reach the pitch are used as padding to reach a desired alignment, and have undefined contents.
	/// </para>
	/// <para>
	/// When a surface holds YUV format data, the planes are assumed to be contiguous without padding between them, e.g. a 32⨯32 surface in NV12 format with a pitch of 32 would consist of 32⨯32 bytes of Y plane followed by 32⨯16 bytes of UV plane.
	/// </para>
	/// <para>
	/// When a surface holds MJPG format data, pixels points at the compressed JPEG image and pitch is the length of that data.
	/// </para>
	/// </remarks>
	public int Pitch
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
				return mSurface is var surface
					&& surface is not null
					? surface->Pitch
					: default;
			}
		}
	}

	internal unsafe SDL_Surface* Pointer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mSurface; }

	/// <summary>
	/// Gets the properties associated with the surface
	/// </summary>
	/// <value>
	/// The properties associated with the surface, or <see langword="null"/> if the properties could not be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	public Properties? Properties
	{
		get
		{
			unsafe
			{
				return SDL_GetSurfaceProperties(mSurface) switch
				{
					0 => null,
					var id => Properties.GetOrCreate(id, registerWithSdl: false)
				};
			}
		}
	}

#if SDL3_4_0_OR_GREATER

	/// <summary>
	/// Gets or sets the rotation angle, in degrees, for the surface
	/// </summary>
	/// <value>
	/// The rotation angle, in clockwise degrees, for the surface
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property defines the number of degrees the surface's pixel data is meant to be rotated clockwise to make it right-side up.
	/// </para>
	/// <para>
	/// This is used by the camera API, if a mobile device is oriented differently than what its camera provides (i.e. the camera always provides portrait images but the phone is being held in landscape orientation).
	/// </para>
	/// <para>
	/// The value defaults to <c>0.0</c>.
	/// </para>
	/// </remarks> 
	public float Rotation
	{
		get => Properties?.TryGetFloatValue(PropertyNames.RotationFloat, out var rotation) is true
			? rotation
			: default;

		set => Properties?.TrySetFloatValue(PropertyNames.RotationFloat, value);
	}

#endif

	/// <summary>
	/// Gets or sets the defining value for 100% white
	/// </summary>
	/// <value>
	/// The defining value for 100% white
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is used by HDR10 and floating point <see cref="Surface"/>s.
	/// </para>
	/// <para>
	/// The value of the property defines the value of 100% diffuse white, with higher values being displayed in the <see cref="HdrHeadroom">High Dynamic Range headroom</see>.
	/// </para>
	/// <para>
	/// The value defaults to <c>203</c> for HDR10 surfaces and <c>1.0</c> for floating point surfaces.
	/// </para>
	/// </remarks>
	public float SdrWhitePoint
	{
		get => Properties?.TryGetFloatValue(PropertyNames.SdrWhitePointFloat, out var sdrWhitePoint) is true
			? sdrWhitePoint
			: default;

		set => Properties?.TrySetFloatValue(PropertyNames.SdrWhitePointFloat, value);
	}

	/// <summary>
	/// Gets or sets the expression of the tonemap operator to used when compressing from a higher dynamic range to a lower dynamic range
	/// </summary>
	/// <value>
	/// The expression of the tonemap operator to used when compressing from a higher dynamic range to a lower dynamic range
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property defines the tone mapping operator used when compressing from a <see cref="Surface"/> with high dynamic range to another with lower dynamic range.
	/// Currently this supports the following values:
	/// <list type="bullet">
	///		<item>
	///			<term><c>"chrome"</c></term>
	///			<description>The same tone mapping that Chrome uses for HDR content</description>
	///		</item>
	///		<item>
	///			<term><c>"*=N"</c> where <c>N</c> is a floating point number</term>
	///			<description><c>N</c> is a floating point scale factor applied in linear. E.g. <c>"*=0.5"</c>.</description>
	///		</item>
	///		<item>
	///			<term><c>"none"</c></term>
	///			<description>Tone mapping is disabled</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// The value defaults to <c>"chrome"</c>.
	/// </para>
	/// </remarks>
	public string TonemapOperator
	{
		get => Properties?.TryGetStringValue(PropertyNames.TonemapOperatorString, out var tonemapOperator) is true
			&& tonemapOperator is not null
			? tonemapOperator
			: string.Empty;

		set => Properties?.TrySetStringValue(PropertyNames.TonemapOperatorString, value);
	}

	/// <summary>
	/// Gets the underlying pixel memory of the surface
	/// </summary>
	/// <value>
	/// The underlying pixel memory of the surface
	/// </value>
	/// <remarks>
	/// <para>
	/// It's unsafe to access this property for surfaces that <see cref="MustLock">must be locked</see> and are not currently <see cref="IsLocked">locked</see>.
	/// </para>
	/// <para>
	/// This property is meant to be used as part of the <see cref="MustLock"/> - <see cref="IsLocked"/> - <see cref="TryUnsafeLock"/> - <see cref="UnsafePixels"/> - <see cref="UnsafeUnlock"/> pattern,
	/// if you want to access the surface's pixel memory directly in a faster and more efficient way.
	/// If you're looking for a simpler and safer way to access the pixel memory, consider using <see cref="TryLock(out SurfacePixelMemoryManager?)"/> instead.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Surface surface;
	/// 
	/// ...
	/// 
	/// if (surface.MustLock &amp;&amp; surface.TryUnsafeLock())
	/// {
	///		// It's safe to access surface.UnsafePixels here
	///		
	///		...
	///		
	///		surface.UnsafeUnlock(); 
	/// }
	/// </code>
	/// </example>
	public Utilities.NativeMemory UnsafePixels
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
				return mSurface is var surface
					&& surface is not null
					&& surface->Pixels is var pixels
					&& pixels is not null
						? new(pixels, unchecked((nuint)surface->H * (nuint)surface->Pitch))
						: Utilities.NativeMemory.Empty;
			}
		}
	}

	/// <summary>
	/// Gets the width of the surface, in pixels
	/// </summary>
	/// <value>
	/// The width of the surface, in pixels
	/// </value>
	public int Width
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
				return mSurface is var surface
					&& surface is not null
						? surface->W
						: default;
			}
		}
	}

	/// <summary>
	/// Removes all alternate versions associated with the surface
	/// </summary>
	public void ClearAlternateImages()
	{
		unsafe
		{
			SDL_RemoveSurfaceAlternateImages(mSurface);
		}
	}

	private protected unsafe virtual void Destroy(SDL_Surface* surface)
	{
		// SDL_DestroySurface decreases the native ref counter, so we don't need to do it manually here
		SDL_DestroySurface(surface);
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(disposing: true, forget: true);
	}

	/// <summary>
	/// Disposes the surface
	/// </summary>
	/// <param name="disposing">A value indicating whether the call came from a call to <see cref="Dispose()"/> or from the finalizer</param>
	/// <param name="forget">A value indicating whether to forget the managed surface</param>
	/// <remarks>
	/// <para>
	/// This unpins any <see cref="Utilities.NativeMemory"/> instances or <see cref="Memory{T}"/> instances used to create the surface.
	/// </para>
	/// </remarks>
	protected virtual void Dispose(bool disposing, bool forget)
	{
		unsafe
		{
			if (mSurface is not null)
			{
				if (forget)
				{
					mKnownInstances.TryRemove(unchecked((IntPtr)mSurface), out _);
				}

				Destroy(mSurface);
				mSurface = null;
			}

#pragma warning disable IDE0031 // I like this more when it's explicit
			if (mNativeMemoryPin is not null)
			{
				mNativeMemoryPin.Dispose();
				mNativeMemoryPin = null;
			}
#pragma warning restore IDE0031

			if (mMemoryHandle.Pointer is not null)
			{
				mMemoryHandle.Dispose();
				mMemoryHandle = default;
			}
		}
	}

	/// <summary>
	/// Maps RGB component values to an <em>opaque</em> encoded pixel value for the surface
	/// </summary>
	/// <param name="r">The red component value to encode</param>
	/// <param name="g">The green component value to encode</param>
	/// <param name="b">The blue component value to encode</param>
	/// <returns>An encoded pixel value in the pixel format of the surface</returns>
	/// <remarks>
	/// <para>
	/// This method maps the RGB color value to the surface's pixel format and returns the encoded pixel value best approximating the given RGB color value for the pixel format.
	/// </para>
	/// <para>
	/// If the surface has a <see cref="Palette"/>, the index of the closest matching color in the palette will be returned.
	/// </para>
	/// <para>
	/// If the pixel format has an alpha component, the encoded pixel value's alpha component value will be set to be fully opaque.
	/// </para>
	/// <para>
	/// If the pixel format's bits-per-pixel value (color depth) is less than 32-bits per pixel then the unused upper bits of the returned encoded pixel value can safely be ignored
	/// (e.g., with a 16-bits per pixel format the returned encoded pixel value can be safely cast (truncated) to an <see cref="ushort"/> value, and similarly to a <see cref="byte"/> value for an 8-bits per pixel formats).
	/// </para>
	/// </remarks>
	public uint MapColor(byte r, byte g, byte b)
	{
		unsafe
		{
			return SDL_MapSurfaceRGB(mSurface, r, g, b);
		}
	}

	/// <summary>
	/// Maps RGBA component values to an encoded pixel value for the surface
	/// </summary>
	/// <param name="r">The red component value to encode</param>
	/// <param name="g">The green component value to encode</param>
	/// <param name="b">The blue component value to encode</param>
	/// <param name="a">The alpha component value to encode</param>
	/// <returns>An encoded pixel value in the pixel format of the surface</returns>
	/// <remarks>
	/// <para>
	/// This method maps the RGBA color value to the surface's pixel format and returns the encoded pixel value best approximating the given RGBA color value for the pixel format.
	/// </para>
	/// <para>
	/// If the surface has a <see cref="Palette"/>, the index of the closest matching color in the palette will be returned.
	/// </para>
	/// <para>
	/// If the pixel format has no alpha component, the given alpha component value will be ignored (as it will be in indexed formats with a <see cref="Palette"/>).
	/// </para>
	/// <para>
	/// If the pixel format's bits-per-pixel value (color depth) is less than 32-bits per pixel then the unused upper bits of the returned encoded pixel value can safely be ignored
	/// (e.g., with a 16-bits per pixel format the returned encoded pixel value can be safely cast (truncated) to an <see cref="ushort"/> value, and similarly to a <see cref="byte"/> value for an 8-bits per pixel formats).
	/// </para>
	/// </remarks>
	public uint MapColor(byte r, byte g, byte b, byte a)
	{
		unsafe
		{
			return SDL_MapSurfaceRGBA(mSurface, r, g, b, a);
		}
	}

	/// <summary>
	/// Maps a <see cref="Color{T}">Color</see>&lt;<see cref="byte"/>&gt; to an encoded pixel value for the surface
	/// </summary>
	/// <param name="color">The <see cref="Color{T}">Color</see>&lt;<see cref="byte"/>&gt; to encoded</param>
	/// <returns>An encoded pixel value in the pixel format of the surface</returns>
	/// <remarks>
	/// <para>
	/// This method maps the <see cref="Color{T}">Color</see>&lt;<see cref="byte"/>&gt; to the surface's pixel format and returns the encoded pixel value best approximating the given <see cref="Color{T}">Color</see>&lt;<see cref="byte"/>&gt; for the pixel format.
	/// </para>
	/// <para>
	/// If the surface has a <see cref="Palette"/>, the index of the closest matching color in the palette will be returned.
	/// </para>
	/// <para>
	/// If the pixel format has no alpha component, the given alpha component value will be ignored (as it will be in indexed formats with a <see cref="Palette"/>).
	/// </para>
	/// <para>
	/// If the pixel format's bits-per-pixel value (color depth) is less than 32-bits per pixel then the unused upper bits of the returned encoded pixel value can safely be ignored
	/// (e.g., with a 16-bits per pixel format the returned encoded pixel value can be safely cast (truncated) to an <see cref="ushort"/> value, and similarly to a <see cref="byte"/> value for an 8-bits per pixel formats).
	/// </para>
	/// </remarks>
	public uint MapColor(Color<byte> color) => MapColor(color.R, color.G, color.B, color.A);

	/// <summary>
	/// Disables the clipping rectangle for the surface
	/// </summary>
	/// <remarks>
	/// <para>
	/// This method disables clipping for the surface, essentially setting the clipping rectangle to the surface's bounds.
	/// </para>
	/// </remarks>
	public void ResetClippingRect()
	{
		unsafe
		{
			SDL_SetSurfaceClipRect(mSurface, rect: null);
		}
	}

	/// <summary>
	/// Sets the clipping rectangle for the surface
	/// </summary>
	/// <param name="rect">The clipping rectangle to set</param>
	/// <returns><c><see langword="true"/></c>, if the clipping rectangle set intersects with the surface area; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// Alternatively, you can set <see cref="ClippingRect"/> instead, if you're not interested in whether the new clipping rectangle intersects with the surface area.
	/// </para>
	/// </remarks>
	public bool SetClippingRect(in Rect<int> rect)
	{
		unsafe
		{
			fixed (Rect<int>* rectPtr = &rect)
			{
				return SDL_SetSurfaceClipRect(mSurface, rectPtr);
			}
		}
	}

	/// <summary>
	/// Tries to add an alternate version of the surface
	/// </summary>
	/// <param name="image">The alternate version to associate with the surface</param>
	/// <returns><c><see langword="true"/></c>, if the alternate version was added successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method adds an alternate version of this surface, usually used for content with high DPI representations like cursors or icons.
	/// The size, format, and content do not need to match the original surface, and these alternate versions will not be updated when the original surface changes.
	/// </para>
	/// </remarks>
	public bool TryAddAlternateImage(Surface image)
	{
		unsafe
		{
			// SDL_AddSurfaceAlternateImage actually increases the native ref counter of the added surface,
			// but we don't need to worry, because, when we dispose this surface, we call SDL_DestroySurface,
			// which in turn calls SDL_RemoveSurfaceAlternateImages, which decreases the native ref counter of all added alternate images (to their original state again).
			return SDL_AddSurfaceAlternateImage(mSurface, image is not null ? image.mSurface : null);
		}
	}

	/// <summary>
	/// Tries to perform a fast blit from a specified source surface to this surface with clipping
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to blit onto. The <see cref="Rect{T}.Width">width</see> and the <see cref="Rect{T}.Height">height</see> are ignored and taken from <paramref name="sourceRect"/> instead.</param>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to blit</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method blits a portion of the <paramref name="source"/> surface defined by <paramref name="sourceRect"/> to this surface at the position defined by <paramref name="destinationRect"/>
	/// while ensuring clipping by <see cref="ClippingRect"/>.
	/// The <see cref="Rect{T}.Width">width</see> and the <see cref="Rect{T}.Height">height</see> of <paramref name="destinationRect"/> are ignored and taken from <paramref name="sourceRect"/> instead.
	/// If you want to specify width and height for the destination rectangle, consider using <see cref="TryBlitScaled(in Rect{int}, Surface, in Rect{int}, ScaleMode)"/> instead.
	/// </para>
	/// <para>
	/// Neither this surface nor the <paramref name="source"/> surface must be locked when calling this method!
	/// </para>
	/// <para>
	/// The blitting semantics for surfaces with and without blending and color keying are defined as follows:
	/// <list type="bullet">
	///		<item>
	///			<term>RGBA 🡒 RGB</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source alpha-channel and per-surface alpha); the <paramref name="source"/>'s <see cref="ColorKey">ColorKey</see> is ignored
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy RGB; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key, ignoring alpha in the comparison
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///		<item>
	///			<term>RGB 🡒 RGBA</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source per-surface alpha); if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy RGB, set destination alpha to source per-surface alpha value; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///		<item>
	///			<term>RGBA 🡒 RGBA</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source alpha-channel and per-surface alpha); the <paramref name="source"/>'s <see cref="ColorKey">ColorKey</see> is ignored
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy all of RGBA; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key, ignoring alpha in the comparison
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///		<item>
	///			<term>RGB 🡒 RGB</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source per-surface alpha); if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy RGB; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	public bool TryBlit(in Rect<int> destinationRect, Surface source, in Rect<int> sourceRect)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect, srcrect = &sourceRect)
			{
				return SDL_BlitSurface(source is not null ? source.mSurface : null, srcrect, mSurface, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to perform a fast blit from a specified source surface to this surface with clipping
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to blit onto. The <see cref="Rect{T}.Width">width</see> and the <see cref="Rect{T}.Height">height</see> are ignored and taken from <paramref name="source"/> instead.</param>
	/// <param name="source">The source surface to blit from</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method blits the entire <paramref name="source"/> surface to this surface at the position defined by <paramref name="destinationRect"/>
	/// while ensuring clipping by <see cref="ClippingRect"/>.
	/// The <see cref="Rect{T}.Width">width</see> and the <see cref="Rect{T}.Height">height</see> of <paramref name="destinationRect"/> are ignored and taken from the <paramref name="source"/> instead.
	/// If you want to specify width and height for the destination rectangle, consider using <see cref="TryBlitScaled(in Rect{int}, Surface, ScaleMode)"/> instead.
	/// </para>
	/// <para>
	/// Neither this surface nor the <paramref name="source"/> surface must be locked when calling this method!
	/// </para>
	/// <para>
	/// The blitting semantics for surfaces with and without blending and color keying are defined as follows:
	/// <list type="bullet">
	///		<item>
	///			<term>RGBA 🡒 RGB</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source alpha-channel and per-surface alpha); the <paramref name="source"/>'s <see cref="ColorKey">ColorKey</see> is ignored
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy RGB; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key, ignoring alpha in the comparison
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///		<item>
	///			<term>RGB 🡒 RGBA</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source per-surface alpha); if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy RGB, set destination alpha to source per-surface alpha value; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///		<item>
	///			<term>RGBA 🡒 RGBA</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source alpha-channel and per-surface alpha); the <paramref name="source"/>'s <see cref="ColorKey">ColorKey</see> is ignored
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy all of RGBA; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key, ignoring alpha in the comparison
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///		<item>
	///			<term>RGB 🡒 RGB</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source per-surface alpha); if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy RGB; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	public bool TryBlit(in Rect<int> destinationRect, Surface source)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect)
			{
				return SDL_BlitSurface(source is not null ? source.mSurface : null, srcrect: null, mSurface, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to perform a fast blit from a specified source surface to this surface with clipping
	/// </summary>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to blit</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method blits a portion of the <paramref name="source"/> surface defined by <paramref name="sourceRect"/> to this surface at the position <c>(0, 0)</c>
	/// while ensuring clipping by <see cref="ClippingRect"/>.
	/// The width and the height of the destination blit area are taken from <paramref name="sourceRect"/>.
	/// If you want to specify width and height for the destination blit area, consider using <see cref="TryBlitScaled(Surface, in Rect{int}, ScaleMode)"/> instead.
	/// </para>
	/// <para>
	/// Neither this surface nor the <paramref name="source"/> surface must be locked when calling this method!
	/// </para>
	/// <para>
	/// The blitting semantics for surfaces with and without blending and color keying are defined as follows:
	/// <list type="bullet">
	///		<item>
	///			<term>RGBA 🡒 RGB</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source alpha-channel and per-surface alpha); the <paramref name="source"/>'s <see cref="ColorKey">ColorKey</see> is ignored
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy RGB; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key, ignoring alpha in the comparison
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///		<item>
	///			<term>RGB 🡒 RGBA</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source per-surface alpha); if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy RGB, set destination alpha to source per-surface alpha value; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///		<item>
	///			<term>RGBA 🡒 RGBA</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source alpha-channel and per-surface alpha); the <paramref name="source"/>'s <see cref="ColorKey">ColorKey</see> is ignored
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy all of RGBA; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key, ignoring alpha in the comparison
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///		<item>
	///			<term>RGB 🡒 RGB</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source per-surface alpha); if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy RGB; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	public bool TryBlit(Surface source, in Rect<int> sourceRect)
	{
		unsafe
		{
			fixed (Rect<int>* srcrect = &sourceRect)
			{
				return SDL_BlitSurface(source is not null ? source.mSurface : null, srcrect, mSurface, dstrect: null);
			}
		}
	}

	/// <summary>
	/// Tries to perform a fast blit from a specified source surface to this surface with clipping
	/// </summary>
	/// <param name="source">The source surface to blit from</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method blits the entire <paramref name="source"/> surface to this surface at the position <c>(0, 0)</c>
	/// while ensuring clipping by <see cref="ClippingRect"/>.
	/// The width and the height of the destination blit area are taken from the <paramref name="source"/>.
	/// If you want to specify width and height for the blit area, consider using <see cref="TryBlitScaled(Surface, ScaleMode)"/> instead.
	/// </para>
	/// <para>
	/// Neither this surface nor the <paramref name="source"/> surface must be locked when calling this method!
	/// </para>
	/// <para>
	/// The blitting semantics for surfaces with and without blending and color keying are defined as follows:
	/// <list type="bullet">
	///		<item>
	///			<term>RGBA 🡒 RGB</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source alpha-channel and per-surface alpha); the <paramref name="source"/>'s <see cref="ColorKey">ColorKey</see> is ignored
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy RGB; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key, ignoring alpha in the comparison
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///		<item>
	///			<term>RGB 🡒 RGBA</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source per-surface alpha); if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy RGB, set destination alpha to source per-surface alpha value; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///		<item>
	///			<term>RGBA 🡒 RGBA</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source alpha-channel and per-surface alpha); the <paramref name="source"/>'s <see cref="ColorKey">ColorKey</see> is ignored
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy all of RGBA; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key, ignoring alpha in the comparison
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	///		<item>
	///			<term>RGB 🡒 RGB</term>
	///			<description>
	///				<list type="bullet">
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.Blend">Blend</see></term>
	///						<description>
	///							alpha-blend (using the source per-surface alpha); if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///					<item>
	///						<term><paramref name="source"/>'s <see cref="BlendMode">BlendMode</see> is <see cref="BlendMode.None">None</see></term>
	///						<description>
	///							copy RGB; if the <paramref name="source"/>'s <see cref="ColorKey"/> is set, only copy pixels that do not match the color key
	///						</description>
	///					</item>
	///				</list>
	///			</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	public bool TryBlit(Surface source)
	{
		unsafe
		{
			return SDL_BlitSurface(source is not null ? source.mSurface : null, srcrect: null, mSurface, dstrect: null);
		}
	}

	/// <summary>
	/// Tries to perform a 9-grid scaled blit from a specified source surface to this surface, which maybe of a different format
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to blit onto</param>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to blit</param>
	/// <param name="leftWidth">The width, in pixels, of the left corners in <paramref name="sourceRect"/></param>
	/// <param name="rightWidth">The width, in pixels, of the right corners in <paramref name="sourceRect"/></param>
	/// <param name="topHeight">The height, in pixels, of the top corners in <paramref name="sourceRect"/></param>
	/// <param name="bottomHeight">The height, in pixels, of the bottom corners in <paramref name="sourceRect"/></param>
	/// <param name="scale">The scale to use to transform the corners of <paramref name="sourceRect"/> into the corners of <paramref name="destinationRect"/>, or <c>0</c> for an unscaled blit</param>
	/// <param name="scaleMode">The scale mode to use for the blit</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The pixels in the source surface are split into a 3⨯3 grid, using the different corner sizes for each corner, and the sides and center making up the remaining pixels.
	/// The corners are then scaled using <paramref name="scale"/> and fit into the corners of the <paramref name="destinationRect"/>.
	/// The sides and center are then stretched into place to cover the remaining portion of the <paramref name="destinationRect"/>.
	/// </para>
	/// <para>
	/// The region specified by <paramref name="sourceRect"/> from the <paramref name="source"/> surface is used as the 9-grid source and scaled to fit into the region specified by <paramref name="destinationRect"/> on this surface.
	/// </para>
	/// </remarks>
	public bool TryBlit9Grid(in Rect<int> destinationRect, Surface source, in Rect<int> sourceRect, int leftWidth, int rightWidth, int topHeight, int bottomHeight, float scale, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect, srcrect = &sourceRect)
			{
				return SDL_BlitSurface9Grid(source is not null ? source.mSurface : null, srcrect, leftWidth, rightWidth, topHeight, bottomHeight, scale, scaleMode, mSurface, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to perform a 9-grid scaled blit from a specified source surface to this surface, which maybe of a different format
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to blit onto</param>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="leftWidth">The width, in pixels, of the left corners in the <paramref name="source"/></param>
	/// <param name="rightWidth">The width, in pixels, of the right corners in the <paramref name="source"/></param>
	/// <param name="topHeight">The height, in pixels, of the top corners in the <paramref name="source"/></param>
	/// <param name="bottomHeight">The height, in pixels, of the bottom corners in the <paramref name="source"/></param>
	/// <param name="scale">The scale to use to transform the corners of the <paramref name="source"/> into the corners of <paramref name="destinationRect"/>, or <c>0</c> for an unscaled blit</param>
	/// <param name="scaleMode">The scale mode to use for the blit</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The pixels in the source surface are split into a 3⨯3 grid, using the different corner sizes for each corner, and the sides and center making up the remaining pixels.
	/// The corners are then scaled using <paramref name="scale"/> and fit into the corners of the <paramref name="destinationRect"/>.
	/// The sides and center are then stretched into place to cover the remaining portion of the <paramref name="destinationRect"/>.
	/// </para>
	/// <para>
	/// The entire <paramref name="source"/> surface is used as the 9-grid source and scaled to fit into the region specified by <paramref name="destinationRect"/> on this surface.
	/// </para>
	/// </remarks>
	public bool TryBlit9Grid(in Rect<int> destinationRect, Surface source, int leftWidth, int rightWidth, int topHeight, int bottomHeight, float scale, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect)
			{
				return SDL_BlitSurface9Grid(source is not null ? source.mSurface : null, srcrect: null, leftWidth, rightWidth, topHeight, bottomHeight, scale, scaleMode, mSurface, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to perform a 9-grid scaled blit from a specified source surface to this surface, which maybe of a different format
	/// </summary>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to blit</param>
	/// <param name="leftWidth">The width, in pixels, of the left corners in <paramref name="sourceRect"/></param>
	/// <param name="rightWidth">The width, in pixels, of the right corners in <paramref name="sourceRect"/></param>
	/// <param name="topHeight">The height, in pixels, of the top corners in <paramref name="sourceRect"/></param>
	/// <param name="bottomHeight">The height, in pixels, of the bottom corners in <paramref name="sourceRect"/></param>
	/// <param name="scale">The scale to use to transform the corners of <paramref name="sourceRect"/> into the corners of this surface, or <c>0</c> for an unscaled blit</param>
	/// <param name="scaleMode">The scale mode to use for the blit</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The pixels in the source surface are split into a 3⨯3 grid, using the different corner sizes for each corner, and the sides and center making up the remaining pixels.
	/// The corners are then scaled using <paramref name="scale"/> and fit into the corners of this surface.
	/// The sides and center are then stretched into place to cover the remaining portion of this surface.
	/// </para>
	/// <para>
	/// The region specified by <paramref name="sourceRect"/> from the <paramref name="source"/> surface is used as the 9-grid source and scaled to fit into the entire area of this surface.
	/// </para>
	/// </remarks>
	public bool TryBlit9Grid(Surface source, in Rect<int> sourceRect, int leftWidth, int rightWidth, int topHeight, int bottomHeight, float scale, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* srcrect = &sourceRect)
			{
				return SDL_BlitSurface9Grid(source is not null ? source.mSurface : null, srcrect, leftWidth, rightWidth, topHeight, bottomHeight, scale, scaleMode, mSurface, dstrect: null);
			}
		}
	}

	/// <summary>
	/// Tries to perform a 9-grid scaled blit from a specified source surface to this surface, which maybe of a different format
	/// </summary>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="leftWidth">The width, in pixels, of the left corners in the <paramref name="source"/></param>
	/// <param name="rightWidth">The width, in pixels, of the right corners in the <paramref name="source"/></param>
	/// <param name="topHeight">The height, in pixels, of the top corners in the <paramref name="source"/></param>
	/// <param name="bottomHeight">The height, in pixels, of the bottom corners in the <paramref name="source"/></param>
	/// <param name="scale">The scale to use to transform the corners of the <paramref name="source"/> into the corners of this surface, or <c>0</c> for an unscaled blit</param>
	/// <param name="scaleMode">The scale mode to use for the blit</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The pixels in the source surface are split into a 3⨯3 grid, using the different corner sizes for each corner, and the sides and center making up the remaining pixels.
	/// The corners are then scaled using <paramref name="scale"/> and fit into the corners of this surface.
	/// The sides and center are then stretched into place to cover the remaining portion of this surface.
	/// </para>
	/// <para>
	/// The entire <paramref name="source"/> surface is used as the 9-grid source and scaled to fit into the entire area of this surface.
	/// </para>
	/// </remarks>
	public bool TryBlit9Grid(Surface source, int leftWidth, int rightWidth, int topHeight, int bottomHeight, float scale, ScaleMode scaleMode)
	{
		unsafe
		{
			return SDL_BlitSurface9Grid(source is not null ? source.mSurface : null, srcrect: null, leftWidth, rightWidth, topHeight, bottomHeight, scale, scaleMode, mSurface, dstrect: null);
		}
	}

	/// <summary>
	/// Tries to perform a scaled blit from a specified source surface to this surface, which maybe of a different format
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to blit onto</param>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to blit</param>
	/// <param name="scaleMode">The scale mode to use for the blit</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The region specified by <paramref name="sourceRect"/> from the <paramref name="source"/> surface is used and scaled to fit into the region specified by <paramref name="destinationRect"/> on this surface.
	/// </para>
	/// </remarks>
	public bool TryBlitScaled(in Rect<int> destinationRect, Surface source, in Rect<int> sourceRect, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect, srcrect = &sourceRect)
			{
				return SDL_BlitSurfaceScaled(source is not null ? source.mSurface : null, srcrect, mSurface, dstrect, scaleMode);
			}
		}
	}

	/// <summary>
	/// Tries to perform a scaled blit from a specified source surface to this surface, which maybe of a different format
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to blit onto</param>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="scaleMode">The scale mode to use for the blit</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The entire <paramref name="source"/> surface is used and scaled to fit into the region specified by <paramref name="destinationRect"/> on this surface.
	/// </para>
	/// </remarks>
	public bool TryBlitScaled(in Rect<int> destinationRect, Surface source, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect)
			{
				return SDL_BlitSurfaceScaled(source is not null ? source.mSurface : null, srcrect: null, mSurface, dstrect, scaleMode);
			}
		}
	}

	/// <summary>
	/// Tries to perform a scaled blit from a specified source surface to this surface, which maybe of a different format
	/// </summary>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to blit</param>
	/// <param name="scaleMode">The scale mode to use for the blit</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The region specified by <paramref name="sourceRect"/> from the <paramref name="source"/> surface is used and scaled to fit into the entire area of this surface.
	/// </para>
	/// </remarks>
	public bool TryBlitScaled(Surface source, in Rect<int> sourceRect, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* srcrect = &sourceRect)
			{
				return SDL_BlitSurfaceScaled(source is not null ? source.mSurface : null, srcrect, mSurface, dstrect: null, scaleMode);
			}
		}
	}

	/// <summary>
	/// Tries to perform a scaled blit from a specified source surface to this surface, which maybe of a different format
	/// </summary>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="scaleMode">The scale mode to use for the blit</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The entire <paramref name="source"/> surface is used and scaled to fit into the entire area of this surface.
	/// </para>
	/// </remarks>
	public bool TryBlitScaled(Surface source, ScaleMode scaleMode)
	{
		unsafe
		{
			return SDL_BlitSurfaceScaled(source is not null ? source.mSurface : null, srcrect: null, mSurface, dstrect: null, scaleMode);
		}
	}

	/// <summary>
	/// Tries to perform a tiled blit from a specified source surface to this surface, which maybe of a different format
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to blit onto</param>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to blit from</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks> 
	/// <para>
	/// The region specified by <paramref name="sourceRect"/> from the <paramref name="source"/> surface is used and repeated as many times as needed to completely fill the region specified by <paramref name="destinationRect"/> on this surface.
	/// </para>
	/// </remarks>
	public bool TryBlitTiled(in Rect<int> destinationRect, Surface source, in Rect<int> sourceRect)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect, srcrect = &sourceRect)
			{
				return SDL_BlitSurfaceTiled(source is not null ? source.mSurface : null, srcrect, mSurface, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to perform a tiled blit from a specified source surface to this surface, which maybe of a different format
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to blit onto</param>
	/// <param name="source">The source surface to blit from</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks> 
	/// <para>
	/// The entire <paramref name="source"/> surface is used and repeated as many times as needed to completely fill the region specified by <paramref name="destinationRect"/> on this surface.
	/// </para>
	/// </remarks>
	public bool TryBlitTiled(in Rect<int> destinationRect, Surface source)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect)
			{
				return SDL_BlitSurfaceTiled(source is not null ? source.mSurface : null, srcrect: null, mSurface, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to perform a tiled blit from a specified source surface to this surface, which maybe of a different format
	/// </summary>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to blit from</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks> 
	/// <para>
	/// The region specified by <paramref name="sourceRect"/> from the <paramref name="source"/> surface is used and repeated as many times as needed to completely fill the entire area of this surface.
	/// </para>
	/// </remarks>
	public bool TryBlitTiled(Surface source, in Rect<int> sourceRect)
	{
		unsafe
		{
			fixed (Rect<int>* srcrect = &sourceRect)
			{
				return SDL_BlitSurfaceTiled(source is not null ? source.mSurface : null, srcrect, mSurface, dstrect: null);
			}
		}
	}

	/// <summary>
	/// Tries to perform a tiled blit from a specified source surface to this surface, which maybe of a different format
	/// </summary>
	/// <param name="source">The source surface to blit from</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks> 
	/// <para>
	/// The entire <paramref name="source"/> surface is used and repeated as many times as needed to completely fill the entire area of this surface.
	/// </para>
	/// </remarks>
	public bool TryBlitTiled(Surface source)
	{
		unsafe
		{
			return SDL_BlitSurfaceTiled(source is not null ? source.mSurface : null, srcrect: null, mSurface, dstrect: null);
		}
	}

	/// <summary>
	/// Tries to perform a tiled and scaled blit from a specified source surface to this surface with scaling, which maybe of a different format
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to blit onto</param>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to blit from</param>
	/// <param name="scale">The scale to transform the <paramref name="sourceRect"/> into the <paramref name="destinationRect"/></param>
	/// <param name="scaleMode">The scale mode to use</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="scale"/> is applied to the region specified by <paramref name="sourceRect"/> from the <paramref name="source"/> surface, e.g. a 32⨯32 region with a scale of <c>2</c> would become a 64⨯64 region.
	/// That scaled region is then used and repeated as many times as needed to completely fill the region specified by <paramref name="destinationRect"/> on this surface.
	/// </para>
	/// </remarks>
	public bool TryBlitTiledWithScale(in Rect<int> destinationRect, Surface source, in Rect<int> sourceRect, float scale, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect, srcrect = &sourceRect)
			{
				return SDL_BlitSurfaceTiledWithScale(source is not null ? source.mSurface : null, srcrect, scale, scaleMode, mSurface, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to perform a tiled and scaled blit from a specified source surface to this surface with scaling, which maybe of a different format
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to blit onto</param>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="scale">The scale to transform the <paramref name="source"/> into the <paramref name="destinationRect"/></param>
	/// <param name="scaleMode">The scale mode to use</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="scale"/> is applied to the entire <paramref name="source"/> surface, e.g. a 32⨯32 surface with a scale of <c>2</c> would become a 64⨯64 region.
	/// That scaled region is then used and repeated as many times as needed to completely fill the region specified by <paramref name="destinationRect"/> on this surface.
	/// </para>
	/// </remarks>
	public bool TryBlitTiledWithScale(in Rect<int> destinationRect, Surface source, float scale, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect)
			{
				return SDL_BlitSurfaceTiledWithScale(source is not null ? source.mSurface : null, srcrect: null, scale, scaleMode, mSurface, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to perform a tiled and scaled blit from a specified source surface to this surface with scaling, which maybe of a different format
	/// </summary>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to blit from</param>
	/// <param name="scale">The scale to transform the <paramref name="sourceRect"/> into this surface</param>
	/// <param name="scaleMode">The scale mode to use</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="scale"/> is applied to the region specified by <paramref name="sourceRect"/> from the <paramref name="source"/> surface, e.g. a 32⨯32 region with a scale of <c>2</c> would become a 64⨯64 region.
	/// That scaled region is then used and repeated as many times as needed to completely fill the entire area of this surface.
	/// </para>
	/// </remarks>
	public bool TryBlitTiledWithScale(Surface source, in Rect<int> sourceRect, float scale, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* srcrect = &sourceRect)
			{
				return SDL_BlitSurfaceTiledWithScale(source is not null ? source.mSurface : null, srcrect, scale, scaleMode, mSurface, dstrect: null);
			}
		}
	}

	/// <summary>
	/// Tries to perform a tiled and scaled blit from a specified source surface to this surface with scaling, which maybe of a different format
	/// </summary>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="scale">The scale to transform the <paramref name="source"/> into this surface</param>
	/// <param name="scaleMode">The scale mode to use</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="scale"/> is applied to the entire <paramref name="source"/> surface, e.g. a 32⨯32 surface with a scale of <c>2</c> would become a 64⨯64 region.
	/// That scaled region is then used and repeated as many times as needed to completely fill the entire area of this surface.
	/// </para>
	/// </remarks>
	public bool TryBlitTiledWithScale(Surface source, float scale, ScaleMode scaleMode)
	{
		unsafe
		{
			return SDL_BlitSurfaceTiledWithScale(source is not null ? source.mSurface : null, srcrect: null, scale, scaleMode, mSurface, dstrect: null);
		}
	}

	/// <summary>
	/// Tries to perform a low-level blit from a specified source surface to this surface
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to blit onto</param>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to blit from</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This is a semi-private blit method and it performs low-level surface blitting, assuming the input rectangles have already been clipped.
	/// </para>
	/// </remarks>
	public bool TryBlitUnchecked(in Rect<int> destinationRect, Surface source, in Rect<int> sourceRect)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect, srcrect = &sourceRect)
			{
				return SDL_BlitSurfaceUnchecked(source is not null ? source.mSurface : null, srcrect, mSurface, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to perform a low-level scaled blit from a specified source surface to this surface
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to blit onto</param>
	/// <param name="source">The source surface to blit from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to blit from</param>
	/// <param name="scaleMode">The scale mode to use</param>
	/// <returns><c><see langword="true"/></c>, if the blit was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This is a semi-private method and it performs low-level surface blitting, assuming the input rectangles have already been clipped.
	/// </para>
	/// </remarks>
	public bool TryBlitUncheckedScaled(in Rect<int> destinationRect, Surface source, in Rect<int> sourceRect, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect, srcrect = &sourceRect)
			{
				return SDL_BlitSurfaceUncheckedScaled(source is not null ? source.mSurface : null, srcrect, mSurface, dstrect, scaleMode);
			}
		}
	}

	/// <summary>
	/// Tries to clear the entire surface to the specified color
	/// </summary>
	/// <param name="r">The red component value of the color to clear to surface to. Usually in the range from <c>0</c> to <c>1</c>.</param>
	/// <param name="g">The green component value of the color to clear to surface to. Usually in the range from <c>0</c> to <c>1</c>.</param>
	/// <param name="b">The blue component value of the color to clear to surface to. Usually in the range from <c>0</c> to <c>1</c>.</param>
	/// <param name="a">The alpha component value of the color to clear to surface to. Usually in the range from <c>0</c> to <c>1</c>.</param>
	/// <returns><c><see langword="true"/></c>, if the surface was successfully cleared; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method handles all surface formats, and ignores any <see cref="ClippingRect"/>.
	/// </para>
	/// <para>
	/// If the surface is YUV, the color is assumed to be in the sRGB colorspace, otherwise the color is assumed to be in the colorspace of the suface.
	/// </para>
	/// </remarks>
	public bool TryClear(float r, float g, float b, float a)
	{
		unsafe
		{
			return SDL_ClearSurface(mSurface, r, g, b, a);
		}
	}

	/// <summary>
	/// Tries to clear the entire surface to the specified color
	/// </summary>
	/// <param name="color">The color to clear the surface to. Usually the range for each component is from <c>0</c> to <c>1</c>.</param>
	/// <returns><c><see langword="true"/></c>, if the surface was successfully cleared; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method handles all surface formats, and ignores any <see cref="ClippingRect"/>.
	/// </para>
	/// <para>
	/// If the surface is YUV, the color is assumed to be in the sRGB colorspace, otherwise the color is assumed to be in the colorspace of the suface.
	/// </para>
	/// </remarks>
	public bool TryClear(Color<float> color) => TryClear(color.R, color.G, color.B, color.A);

	/// <summary>
	/// Tries to create a copy of this surface converted to the specified format
	/// </summary>
	/// <param name="format">The pixel format to convert the surface to</param>
	/// <param name="result">The converted surface, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the surface was successfully converted; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is used to optimize images for faster <em>repeat</em> blitting.
	/// This is accomplished by converting the original and storing the result as a new surface.
	/// The new, optimized surface can then be used as the source for future blits, making them faster.
	/// </para>
	/// <para>
	/// If you are converting to an indexed surface and want to map colors to a palette, you can use <see cref="TryConvert(PixelFormat, Palette?, ColorSpace, Properties?, out Surface?)"/> instead.
	/// </para>
	/// <para>
	/// If the original surface has alternate images, the duplicated surface will have a reference to them as well.
	/// </para>
	/// </remarks>
	public bool TryConvert(PixelFormat format, [NotNullWhen(true)] out Surface? result)
	{
		unsafe
		{
			var surface = SDL_ConvertSurface(mSurface, format);

			if (surface is null)
			{
				result = null;
				return false;
			}

			result = new(surface, register: true);
			return true;
		}
	}

	/// <summary>
	/// Tries to create a copy of this surface converted to the specified format, using the specified palette, color space, and properties
	/// </summary>
	/// <param name="format">The pixel format to convert the surface to</param>
	/// <param name="palette">An optional palette used for indexed surfaces, or <c><see langword="null"/></c></param>
	/// <param name="colorSpace">The color space to convert the surface to</param>
	/// <param name="properties">Additional color properties to use for the converted surface, or <c><see langword="null"/></c></param>
	/// <param name="result">The converted surface, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the surface was successfully converted; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method converts an existing surface to a new format and color space and returns the new surface. This will perform any pixel format and color space conversions as needed.
	/// </para>
	/// <para>
	/// If the original surface has alternate images, the duplicated surface will have a reference to them as well.
	/// </para>
	/// </remarks>
	public bool TryConvert(PixelFormat format, Palette? palette, ColorSpace colorSpace, Properties? properties, [NotNullWhen(true)] out Surface? result)
	{
		unsafe
		{
			var surface = SDL_ConvertSurfaceAndColorspace(mSurface, format, palette is not null ? palette.Pointer : null, colorSpace, properties?.Id ?? 0);

			if (surface is null)
			{
				result = null;
				return false;
			}

			result = new(surface, register: true);
			return true;
		}
	}

	/// <summary>
	/// Tries to convert a block of pixels to a specified format
	/// </summary>
	/// <param name="width">The width of the block to convert, in pixels</param>
	/// <param name="height">The height of the block to convert, in pixels</param>
	/// <param name="sourceFormat">The pixel format of the source data</param>
	/// <param name="source">The source pixel data</param>
	/// <param name="sourcePitch">The pitch of the source pixel data, in bytes</param>
	/// <param name="destinationFormat">The desired pixel format of the destination data</param>
	/// <param name="destination">The destination pixel data</param>
	/// <param name="destinationPitch">The pitch of the destination pixel data, in bytes</param>
	/// <returns><c><see langword="true"/></c>, if the pixel data was successfully converted; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method additionally returns <c><see langword="false"/></c> if either <paramref name="source"/> or <paramref name="destination"/> are <see cref="Utilities.NativeMemory.IsValid">invalid</see>,
	/// <c><paramref name="source"/>.<see cref="ReadOnlyNativeMemory.Length">Length</see></c> is less than; <c><paramref name="height"/> * <paramref name="sourcePitch"/></c>,
	/// or <c><paramref name="destination"/>.<see cref="Utilities.NativeMemory.Length">Length</see></c> is less than <c><paramref name="height"/> * <paramref name="destinationPitch"/></c>.
	/// </para>
	/// </remarks>
	public static bool TryConvert(int width, int height, PixelFormat sourceFormat, ReadOnlyNativeMemory source, int sourcePitch, PixelFormat destinationFormat, Utilities.NativeMemory destination, int destinationPitch)
	{
		unsafe
		{
			if (!source.IsValid || !destination.IsValid)
			{
				return false;
			}

			var sourceRequired = unchecked((height is > 0 ? (nuint)height : 0) * (sourcePitch is > 0 ? (nuint)sourcePitch : 0));

			if (source.Length < sourceRequired)
			{
				return false;
			}

			var destinationRequired = unchecked((height is > 0 ? (nuint)height : 0) * (destinationPitch is > 0 ? (nuint)destinationPitch : 0));

			if (destination.Length < destinationRequired)
			{
				return false;
			}

			return SDL_ConvertPixels(width, height, sourceFormat, source.RawPointer, sourcePitch, destinationFormat, destination.RawPointer, destinationPitch);
		}
	}

	/// <summary>
	/// Tries to convert a block of pixels to a specified format
	/// </summary>
	/// <param name="width">The width of the block to convert, in pixels</param>
	/// <param name="height">The height of the block to convert, in pixels</param>
	/// <param name="sourceFormat">The pixel format of the source data</param>
	/// <param name="source">The source pixel data</param>
	/// <param name="sourcePitch">The pitch of the source pixel data, in bytes</param>
	/// <param name="destinationFormat">The desired pixel format of the destination data</param>
	/// <param name="destination">The destination pixel data</param>
	/// <param name="destinationPitch">The pitch of the destination pixel data, in bytes</param>
	/// <returns><c><see langword="true"/></c>, if the pixel data was successfully converted; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method additionally returns <c><see langword="false"/></c> if <c><paramref name="source"/>.<see cref="ReadOnlySpan{T}.Length">Length</see></c> is less than; <c><paramref name="height"/> * <paramref name="sourcePitch"/></c>,
	/// or <c><paramref name="destination"/>.<see cref="Span{T}.Length">Length</see></c> is less than <c><paramref name="height"/> * <paramref name="destinationPitch"/></c>.
	/// </para>
	/// </remarks>
	public static bool TryConvert(int width, int height, PixelFormat sourceFormat, ReadOnlySpan<byte> source, int sourcePitch, PixelFormat destinationFormat, Span<byte> destination, int destinationPitch)
	{
		unsafe
		{
			var sourceRequired = unchecked((height is > 0 ? (nuint)height : 0) * (sourcePitch is > 0 ? (nuint)sourcePitch : 0));

			if (unchecked((nuint)source.Length) < sourceRequired)
			{
				return false;
			}

			var destinationRequired = unchecked((height is > 0 ? (nuint)height : 0) * (destinationPitch is > 0 ? (nuint)destinationPitch : 0));

			if (unchecked((nuint)destination.Length) < destinationRequired)
			{
				return false;
			}

			fixed (byte* src = source, dst = destination)
			{
				return SDL_ConvertPixels(width, height, sourceFormat, src, sourcePitch, destinationFormat, dst, destinationPitch);
			}
		}
	}

	/// <summary>
	/// Tries to convert a block of pixels to a specified format
	/// </summary>
	/// <param name="width">The width of the block to convert, in pixels</param>
	/// <param name="height">The height of the block to convert, in pixels</param>
	/// <param name="sourceFormat">The pixel format of the source data</param>
	/// <param name="source">A pointer to the source pixel data</param>
	/// <param name="sourcePitch">The pitch of the source pixel data, in bytes</param>
	/// <param name="destinationFormat">The desired pixel format of the destination data</param>
	/// <param name="destination">A pointer to the destination pixel data</param>
	/// <param name="destinationPitch">The pitch of the destination pixel data, in bytes</param>
	/// <returns><c><see langword="true"/></c>, if the pixel data was successfully converted; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	public unsafe static bool TryConvert(int width, int height, PixelFormat sourceFormat, void* source, int sourcePitch, PixelFormat destinationFormat, void* destination, int destinationPitch)
	{
		return SDL_ConvertPixels(width, height, sourceFormat, source, sourcePitch, destinationFormat, destination, destinationPitch);
	}

	/// <summary>
	/// Tries to convert a block of pixels to a specified format, using the specified palette, color space, and properties
	/// </summary>
	/// <param name="width">The width of the block to convert, in pixels</param>
	/// <param name="height">The height of the block to convert, in pixels</param>
	/// <param name="sourceFormat">The pixel format of the source data</param>
	/// <param name="sourceColorSpace">The color space of the source data</param>
	/// <param name="sourceProperties">Additional color properties to use for the source data, or <c><see langword="null"/></c></param>
	/// <param name="source">The source pixel data</param>
	/// <param name="sourcePitch">The pitch of the source pixel data, in bytes</param>
	/// <param name="destinationFormat">The desired pixel format of the destination data</param>
	/// <param name="destinationColorSpace">The color space of the destination data</param>
	/// <param name="destinationProperties">Additional color properties to use for the destination data, or <c><see langword="null"/></c></param>
	/// <param name="destination">The destination pixel data</param>
	/// <param name="destinationPitch">The pitch of the destination pixel data, in bytes</param>
	/// <returns><c><see langword="true"/></c>, if the pixel data was successfully converted; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method additionally returns <c><see langword="false"/></c> if either <paramref name="source"/> or <paramref name="destination"/> are <see cref="Utilities.NativeMemory.IsValid">invalid</see>,
	/// <c><paramref name="source"/>.<see cref="ReadOnlyNativeMemory.Length">Length</see></c> is less than; <c><paramref name="height"/> * <paramref name="sourcePitch"/></c>,
	/// or <c><paramref name="destination"/>.<see cref="Utilities.NativeMemory.Length">Length</see></c> is less than <c><paramref name="height"/> * <paramref name="destinationPitch"/></c>.
	/// </para>
	/// </remarks>
	public static bool TryConvert(int width, int height, PixelFormat sourceFormat, ColorSpace sourceColorSpace, Properties? sourceProperties, ReadOnlyNativeMemory source, int sourcePitch, PixelFormat destinationFormat, ColorSpace destinationColorSpace, Properties? destinationProperties, Utilities.NativeMemory destination, int destinationPitch)
	{
		unsafe
		{
			if (!source.IsValid || !destination.IsValid)
			{
				return false;
			}

			var sourceRequired = unchecked((height is > 0 ? (nuint)height : 0) * (sourcePitch is > 0 ? (nuint)sourcePitch : 0));

			if (source.Length < sourceRequired)
			{
				return false;
			}

			var destinationRequired = unchecked((height is > 0 ? (nuint)height : 0) * (destinationPitch is > 0 ? (nuint)destinationPitch : 0));

			if (destination.Length < destinationRequired)
			{
				return false;
			}

			return SDL_ConvertPixelsAndColorspace(width, height, sourceFormat, sourceColorSpace, sourceProperties?.Id ?? 0, source.RawPointer, sourcePitch, destinationFormat, destinationColorSpace, destinationProperties?.Id ?? 0, destination.RawPointer, destinationPitch);
		}
	}

	/// <summary>
	/// Tries to convert a block of pixels to a specified format, using the specified palette, color space, and properties
	/// </summary>
	/// <param name="width">The width of the block to convert, in pixels</param>
	/// <param name="height">The height of the block to convert, in pixels</param>
	/// <param name="sourceFormat">The pixel format of the source data</param>
	/// <param name="sourceColorSpace">The color space of the source data</param>
	/// <param name="sourceProperties">Additional color properties to use for the source data, or <c><see langword="null"/></c></param>
	/// <param name="source">The source pixel data</param>
	/// <param name="sourcePitch">The pitch of the source pixel data, in bytes</param>
	/// <param name="destinationFormat">The desired pixel format of the destination data</param>
	/// <param name="destinationColorSpace">The color space of the destination data</param>
	/// <param name="destinationProperties">Additional color properties to use for the destination data, or <c><see langword="null"/></c></param>
	/// <param name="destination">The destination pixel data</param>
	/// <param name="destinationPitch">The pitch of the destination pixel data, in bytes</param>
	/// <returns><c><see langword="true"/></c>, if the pixel data was successfully converted; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method additionally returns <c><see langword="false"/></c> if <c><paramref name="source"/>.<see cref="ReadOnlySpan{T}.Length">Length</see></c> is less than; <c><paramref name="height"/> * <paramref name="sourcePitch"/></c>,
	/// or <c><paramref name="destination"/>.<see cref="Span{T}.Length">Length</see></c> is less than <c><paramref name="height"/> * <paramref name="destinationPitch"/></c>.
	/// </para>
	/// </remarks>
	public static bool TryConvert(int width, int height, PixelFormat sourceFormat, ColorSpace sourceColorSpace, Properties? sourceProperties, ReadOnlySpan<byte> source, int sourcePitch, PixelFormat destinationFormat, ColorSpace destinationColorSpace, Properties? destinationProperties, Span<byte> destination, int destinationPitch)
	{
		unsafe
		{
			var sourceRequired = unchecked((height is > 0 ? (nuint)height : 0) * (sourcePitch is > 0 ? (nuint)sourcePitch : 0));

			if (unchecked((nuint)source.Length) < sourceRequired)
			{
				return false;
			}

			var destinationRequired = unchecked((height is > 0 ? (nuint)height : 0) * (destinationPitch is > 0 ? (nuint)destinationPitch : 0));

			if (unchecked((nuint)destination.Length) < destinationRequired)
			{
				return false;
			}

			fixed (byte* src = source, dst = destination)
			{
				return SDL_ConvertPixelsAndColorspace(width, height, sourceFormat, sourceColorSpace, sourceProperties?.Id ?? 0, src, sourcePitch, destinationFormat, destinationColorSpace, destinationProperties?.Id ?? 0, dst, destinationPitch);
			}
		}
	}

	/// <summary>
	/// Tries to convert a block of pixels to a specified format, using the specified palette, color space, and properties
	/// </summary>
	/// <param name="width">The width of the block to convert, in pixels</param>
	/// <param name="height">The height of the block to convert, in pixels</param>
	/// <param name="sourceFormat">The pixel format of the source data</param>
	/// <param name="sourceColorSpace">The color space of the source data</param>
	/// <param name="sourceProperties">Additional color properties to use for the source data, or <c><see langword="null"/></c></param>
	/// <param name="source">A pointer to the source pixel data</param>
	/// <param name="sourcePitch">The pitch of the source pixel data, in bytes</param>
	/// <param name="destinationFormat">The desired pixel format of the destination data</param>
	/// <param name="destinationColorSpace">The color space of the destination data</param>
	/// <param name="destinationProperties">Additional color properties to use for the destination data, or <c><see langword="null"/></c></param>
	/// <param name="destination">A pointer to the destination pixel data</param>
	/// <param name="destinationPitch">The pitch of the destination pixel data, in bytes</param>
	/// <returns><c><see langword="true"/></c>, if the pixel data was successfully converted; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	public unsafe static bool TryConvert(int width, int height, PixelFormat sourceFormat, ColorSpace sourceColorSpace, Properties? sourceProperties, void* source, int sourcePitch, PixelFormat destinationFormat, ColorSpace destinationColorSpace, Properties? destinationProperties, void* destination, int destinationPitch)
	{
		return SDL_ConvertPixelsAndColorspace(width, height, sourceFormat, sourceColorSpace, sourceProperties?.Id ?? 0, source, sourcePitch, destinationFormat, destinationColorSpace, destinationProperties?.Id ?? 0, destination, destinationPitch);
	}

#if SDL3_4_0_OR_GREATER
	/// <summary>
	/// Tries to perform a stretched pixel copy from a specified source surface onto this surface
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to copy onto</param>
	/// <param name="source">The source surface to copy from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to copy</param>
	/// <param name="scaleMode">The scale mode to use for the copy</param>
	/// <returns><c><see langword="true"/></c>, if the copy was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The region specified by <paramref name="sourceRect"/> from the <paramref name="source"/> surface is copied and stretched to fit into the region specified by <paramref name="destinationRect"/> on this surface.
	/// </para>
	/// </remarks>
	public bool TryCopyStretched(in Rect<int> destinationRect, Surface source, in Rect<int> sourceRect, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect, srcrect = &sourceRect)
			{
				return SDL_StretchSurface(source is not null ? source.mSurface : null, srcrect, mSurface, dstrect, scaleMode);
			}
		}
	}

	/// <summary>
	/// Tries to perform a stretched pixel copy from a specified source surface onto this surface
	/// </summary>
	/// <param name="destinationRect">The destination rectangle on this surface to copy onto</param>
	/// <param name="source">The source surface to copy from</param>
	/// <param name="scaleMode">The scale mode to use for the copy</param>
	/// <returns><c><see langword="true"/></c>, if the copy was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The entire <paramref name="source"/> surface is copied and stretched to fit into the region specified by <paramref name="destinationRect"/> on this surface.
	/// </para>
	/// </remarks>
	public bool TryCopyStretched(in Rect<int> destinationRect, Surface source, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* dstrect = &destinationRect)
			{
				return SDL_StretchSurface(source is not null ? source.mSurface : null, srcrect: null, mSurface, dstrect, scaleMode);
			}
		}
	}

	/// <summary>
	/// Tries to perform a stretched pixel copy from a specified source surface onto this surface
	/// </summary>
	/// <param name="source">The source surface to copy from</param>
	/// <param name="sourceRect">The source rectangle on the source surface to copy</param>
	/// <param name="scaleMode">The scale mode to use for the copy</param>
	/// <returns><c><see langword="true"/></c>, if the copy was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The region specified by <paramref name="sourceRect"/> from the <paramref name="source"/> surface is copied and stretched to fit into the entire area of this surface.
	/// </para>
	/// </remarks>
	public bool TryCopyStretched(Surface source, in Rect<int> sourceRect, ScaleMode scaleMode)
	{
		unsafe
		{
			fixed (Rect<int>* srcrect = &sourceRect)
			{
				return SDL_StretchSurface(source is not null ? source.mSurface : null, srcrect, mSurface, dstrect: null, scaleMode);
			}
		}
	}

	/// <summary>
	/// Tries to perform a stretched pixel copy from a specified source surface onto this surface
	/// </summary>
	/// <param name="source">The source surface to copy from</param>
	/// <param name="scaleMode">The scale mode to use for the copy</param>
	/// <returns><c><see langword="true"/></c>, if the copy was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The entire <paramref name="source"/> surface is copied and stretched to fit into the entire area of this surface.
	/// </para>
	/// </remarks>
	public bool TryCopyStretched(Surface source, ScaleMode scaleMode)
	{
		unsafe
		{
			return SDL_StretchSurface(source is not null ? source.mSurface : null, srcrect: null, mSurface, dstrect: null, scaleMode);
		}
	}
#endif

	/// <summary>
	/// Tries to create a new <see cref="Palette"/> for this surface
	/// </summary>
	/// <param name="palette">The newly created palette, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the palette was created successfully; otherwise, <c><see langword="false"/></c> (e.g. if the surface didn't have an index format, check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This function creates a palette compatible with the surface.
	/// The palette is then returned for you to modify, and the surface will automatically use the new palette in future operations.
	/// </para>
	/// <para>
	/// Bitmap surfaces (with format <see cref="PixelFormat.Index1Lsb"/> or <see cref="PixelFormat.Index1Msb"/>) will have the palette initialized with <c>0</c> as white and <c>1</c> as black.
	/// Other surfaces will get a palette initialized with white in every entry.
	/// </para>
	/// <para>
	/// If this method is called for on surface that already has a palette, a new palette will be created to replace it.
	/// </para>
	/// </remarks>
	public bool TryCreatePalette([NotNullWhen(true)] out Palette? palette)
	{
		unsafe
		{
			// See the comment in the Palette setter about why this is alright to do without worrying about SDL_CreateSurfacePalette potentially destroying the old palette

			var result = SDL_CreateSurfacePalette(mSurface);

			if (result is null)
			{
				palette = null;
				return false;
			}

			mPalette = palette = new Palette(result); // we actually want to override any existing managed Palette here
			return true;
		}
	}

	/// <summary>
	/// Tries to create a new software renderer for this surface
	/// </summary>
	/// <param name="renderer">The resulting software renderer, if the method returns <see langword="true"/>; otherwise, <see langword="null"/></param>
	/// <returns><c><see langword="true"/></c>, if the renderer was created successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method creates a software renderer that renders directly to this surface.
	/// </para>
	/// <para>
	/// As such, the resulting renderer won't be associated with any window. 
	/// You can also create software renderers for <see cref="Window"/>s using the appropriate methods on a <see cref="Window"/> instance.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryCreateRenderer([NotNullWhen(true)] out Renderer<Software>? renderer)
	{
		unsafe
		{
			var rendererPtr = Renderer.SDL_CreateSoftwareRenderer(mSurface);

			if (rendererPtr is null)
			{
				renderer = default;
				return false;
			}

			renderer = new(rendererPtr, register: true);
			return true;
		}
	}

	/// <summary>
	/// Tries to create a new software renderer for this surface
	/// </summary>
	/// <param name="renderer">The resulting software renderer, if the method returns <see langword="true"/>; otherwise, <see langword="null"/></param>
	/// <param name="outputColorSpace">
	/// The color space to be used by renderer for presenting the output.
	/// The <see cref="Direct3D11">Direct3D 11</see>, <see cref="Direct3D12">Direct3D 12</see>, and <see cref="Metal">Metal</see> renderers support <see cref="ColorSpace.SrgbLinear"/>,
	/// which is a linear color space and supports HDR output. In that case, drawing still uses the sRGB color space, but individual values can go beyond <c>1.0</c>
	/// and floating point textures can be used for HDR content.
	/// If this parameter is <see langword="null"/> (the default), the output color space defaults to <see cref="ColorSpace.Srgb"/>.
	/// </param>
	/// <param name="properties">Additional properties to use when creating the renderer</param>
	/// <returns><c><see langword="true"/></c>, if the renderer was created successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method creates a software renderer that renders directly to this surface.
	/// </para>
	/// <para>
	/// As such, the resulting renderer won't be associated with any window. 
	/// You can also create software renderers for <see cref="Window"/>s using the appropriate methods on a <see cref="Window"/> instance.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryCreateRenderer([NotNullWhen(true)] out Renderer<Software>? renderer, ColorSpace? outputColorSpace = default, Properties? properties = default)
	{
		unsafe
		{
			Properties propertiesUsed;
			Unsafe.SkipInit(out string? driverNameBackup);
			Unsafe.SkipInit(out IntPtr? surfaceBackup);
			Unsafe.SkipInit(out ColorSpace? outputColorSpaceBackup);

			if (properties is null)
			{
				propertiesUsed = [];

				// naturally, we want SDL_PROP_RENDERER_CREATE_NAME_STRING to be set to "software" when creating a software renderer
				propertiesUsed.TrySetStringValue(Renderer.PropertyNames.CreateNameString, Software.Name);

				// setting SDL_PROP_RENDERER_CREATE_SURFACE_POINTER is required, when creating a software renderer for a surface
				propertiesUsed.TrySetPointerValue(Renderer<Software>.PropertyNames.CreateSoftwareSurfacePointer, unchecked((IntPtr)(mSurface)));

				if (outputColorSpace is ColorSpace outputColorSpaceValue)
				{
					propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreateOutputColorSpaceNumber, unchecked((uint)outputColorSpaceValue));
				}
			}
			else
			{
				propertiesUsed = properties;

				driverNameBackup = propertiesUsed.TryGetStringValue(Renderer.PropertyNames.CreateNameString, out var existingDriverName)
					? existingDriverName
					: null;

				// naturally, we want SDL_PROP_RENDERER_CREATE_NAME_STRING to be set to "software" when creating a software renderer
				propertiesUsed.TrySetStringValue(Renderer.PropertyNames.CreateNameString, Software.Name);

				surfaceBackup = propertiesUsed.TryGetPointerValue(Renderer<Software>.PropertyNames.CreateSoftwareSurfacePointer, out var existingSurfacePtr)
					? existingSurfacePtr
					: null;

				// setting SDL_PROP_RENDERER_CREATE_SURFACE_POINTER is required, when creating a software renderer for a surface
				propertiesUsed.TrySetPointerValue(Renderer<Software>.PropertyNames.CreateSoftwareSurfacePointer, unchecked((IntPtr)(mSurface)));

				if (outputColorSpace is ColorSpace outputColorSpaceValue)
				{
					outputColorSpaceBackup = propertiesUsed.TryGetNumberValue(Renderer.PropertyNames.CreateOutputColorSpaceNumber, out var existingOutputColorSpaceValue)
						? unchecked((ColorSpace)existingOutputColorSpaceValue)
						: null;

					propertiesUsed.TrySetNumberValue(Renderer.PropertyNames.CreateOutputColorSpaceNumber, unchecked((uint)outputColorSpaceValue));
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

					if (surfaceBackup is IntPtr surfacePtr)
					{
						propertiesUsed.TrySetPointerValue(Renderer<Software>.PropertyNames.CreateSoftwareSurfacePointer, surfacePtr);
					}
					else
					{
						propertiesUsed.TryRemove(Renderer<Software>.PropertyNames.CreateSoftwareSurfacePointer);
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
				}
			}
		}
	}

	/// <summary>
	/// Tries to duplicate this surface
	/// </summary>
	/// <param name="duplicate">The duplicated surface, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the surface was duplicated successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// If the original surface has alternate images, the duplicated surface will have a reference to them as well.
	/// </para>
	/// </remarks>
	public bool TryDuplicate([NotNullWhen(true)] out Surface? duplicate)
	{
		unsafe
		{
			var duplicatePtr = SDL_DuplicateSurface(mSurface);

			if (duplicatePtr is null)
			{
				duplicate = null;

				return false;
			}

			duplicate = new(duplicatePtr, register: true);

			return true;
		}
	}

	/// <summary>
	/// Tries to perform a fast fill of the entire surface with the specified pixel value
	/// </summary>
	/// <param name="pixelValue">The pixel value to fill the surface with</param>
	/// <returns><c><see langword="true"/></c>, if the fill operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="pixelValue"/> argument should be a pixel value of the format used by the surface, and can be generated by <see cref="MapColor(byte, byte, byte)"/>, <see cref="MapColor(byte, byte, byte, byte)"/>, or <see cref="MapColor(Color{byte})"/>.
	/// Alternatively, you can use the <see cref="TryFill(byte, byte, byte)"/>, <see cref="TryFill(byte, byte, byte, byte)"/>, or <see cref="TryFill(Color{byte})"/> overloads which will do the mapping for you.
	/// </para>
	/// <para>
	/// If the color value contains an alpha component then the surface is simply filled with that alpha information, no blending takes place.
	/// </para>
	/// <para>
	/// If there is a <see cref="ClippingRect"/> set, then this method will fill based on the <see cref="ClippingRect"/>.
	/// </para>
	/// </remarks>
	public bool TryFill(uint pixelValue)
	{
		unsafe
		{
			return SDL_FillSurfaceRect(mSurface, rect: null, pixelValue);
		}
	}

	/// <summary>
	/// Tries to perform a fast fill of the entire surface with the specified color
	/// </summary>
	/// <param name="r">The red component value of the color to fill the surface with</param>
	/// <param name="g">The green component value of the color to fill the surface with</param>
	/// <param name="b">The blue component value of the color to fill the surface with</param>
	/// <returns><c><see langword="true"/></c>, if the fill operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The surface is simply filled fully opaque, no blending takes place.
	/// </para>
	/// <para>
	/// If there is a <see cref="ClippingRect"/> set, then this method will fill based on the <see cref="ClippingRect"/>.
	/// </para>
	/// </remarks>
	public bool TryFill(byte r, byte g, byte b) => TryFill(MapColor(r, g, b));

	/// <summary>
	/// Tries to perform a fast fill of the entire surface with the specified color
	/// </summary>
	/// <param name="r">The red component value of the color to fill the surface with</param>
	/// <param name="g">The green component value of the color to fill the surface with</param>
	/// <param name="b">The blue component value of the color to fill the surface with</param>
	/// <param name="a">The alpha component value of the color to fill the surface with</param>
	/// <returns><c><see langword="true"/></c>, if the fill operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The surface is simply filled with the <paramref name="a"/>lpha information, no blending takes place.
	/// </para>
	/// <para>
	/// If there is a <see cref="ClippingRect"/> set, then this method will fill based on the <see cref="ClippingRect"/>.
	/// </para>
	/// </remarks>
	public bool TryFill(byte r, byte g, byte b, byte a) => TryFill(MapColor(r, g, b, a));

	/// <summary>
	/// Tries to perform a fast fill of the entire surface with the specified color
	/// </summary>
	/// <param name="color">The color to fill the rectangle with</param>
	/// <returns><c><see langword="true"/></c>, if the fill operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>surface is simply filled with the <see cref="Color{T}.A"/> information, no blending takes place.
	/// </para>
	/// <para>
	/// If there is a <see cref="ClippingRect"/> set, then this method will fill based on the <see cref="ClippingRect"/>.
	/// </para>
	/// </remarks>
	public bool TryFill(Color<byte> color) => TryFill(MapColor(color));

	/// <summary>
	/// Tries to perform a fast fill of a rectangle on this surface with the specified pixel value
	/// </summary>
	/// <param name="destinationRect">The rectangle to fill</param>
	/// <param name="pixelValue">The pixel value to fill the rectangle with</param>
	/// <returns><c><see langword="true"/></c>, if the fill operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="pixelValue"/> argument should be a pixel value of the format used by the surface, and can be generated by <see cref="MapColor(byte, byte, byte)"/>, <see cref="MapColor(byte, byte, byte, byte)"/>, or <see cref="MapColor(Color{byte})"/>.
	/// Alternatively, you can use the <see cref="TryFill(in Rect{int}, byte, byte, byte)"/>, <see cref="TryFill(in Rect{int}, byte, byte, byte, byte)"/>, or <see cref="TryFill(in Rect{int}, Color{byte})"/> overloads which will do the mapping for you.
	/// </para>
	/// <para>
	/// If the color value contains an alpha component then the <paramref name="destinationRect"/> is simply filled with that alpha information, no blending takes place.
	/// </para>
	/// <para>
	/// If there is a <see cref="ClippingRect"/> set, then this method will fill based on the intersection of the <see cref="ClippingRect"/> and the <paramref name="destinationRect"/>.
	/// </para>
	/// </remarks>
	public bool TryFill(in Rect<int> destinationRect, uint pixelValue)
	{
		unsafe
		{
			fixed (Rect<int>* rect = &destinationRect)
			{
				return SDL_FillSurfaceRect(mSurface, rect, pixelValue);
			}
		}
	}

	/// <summary>
	/// Tries to perform a fast fill of a rectangle on this surface with the specified color
	/// </summary>
	/// <param name="destinationRect">The rectangle to fill</param>
	/// <param name="r">The red component value of the color to fill the rectangle with</param>
	/// <param name="g">The green component value of the color to fill the rectangle with</param>
	/// <param name="b">The blue component value of the color to fill the rectangle with</param>
	/// <returns><c><see langword="true"/></c>, if the fill operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="destinationRect"/> is simply filled fully opaque, no blending takes place.
	/// </para>
	/// <para>
	/// If there is a <see cref="ClippingRect"/> set, then this method will fill based on the intersection of the <see cref="ClippingRect"/> and the <paramref name="destinationRect"/>.
	/// </para>
	/// </remarks>
	public bool TryFill(in Rect<int> destinationRect, byte r, byte g, byte b) => TryFill(in destinationRect, MapColor(r, g, b));

	/// <summary>
	/// Tries to perform a fast fill of a rectangle on this surface with the specified color
	/// </summary>
	/// <param name="destinationRect">The rectangle to fill</param>
	/// <param name="r">The red component value of the color to fill the rectangle with</param>
	/// <param name="g">The green component value of the color to fill the rectangle with</param>
	/// <param name="b">The blue component value of the color to fill the rectangle with</param>
	/// <param name="a">The alpha component value of the color to fill the rectangle with</param>
	/// <returns><c><see langword="true"/></c>, if the fill operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="destinationRect"/> is simply filled with the <paramref name="a"/>lpha information, no blending takes place.
	/// </para>
	/// <para>
	/// If there is a <see cref="ClippingRect"/> set, then this method will fill based on the intersection of the <see cref="ClippingRect"/> and the <paramref name="destinationRect"/>.
	/// </para>
	/// </remarks>
	public bool TryFill(in Rect<int> destinationRect, byte r, byte g, byte b, byte a) => TryFill(in destinationRect, MapColor(r, g, b, a));

	/// <summary>
	/// Tries to perform a fast fill of a rectangle on this surface with the specified color
	/// </summary>
	/// <param name="destinationRect">The rectangle to fill</param>
	/// <param name="color">The color to fill the rectangle with</param>
	/// <returns><c><see langword="true"/></c>, if the fill operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="destinationRect"/> is simply filled with the <see cref="Color{T}.A"/> information, no blending takes place.
	/// </para>
	/// <para>
	/// If there is a <see cref="ClippingRect"/> set, then this method will fill based on the intersection of the <see cref="ClippingRect"/> and the <paramref name="destinationRect"/>.
	/// </para>
	/// </remarks>
	public bool TryFill(in Rect<int> destinationRect, Color<byte> color) => TryFill(in destinationRect, MapColor(color));

	/// <summary>
	/// Tries to perform a fast fill of multiple rectangles on this surface with the specified pixel value
	/// </summary>
	/// <param name="destinationRects">The list of rectangles to fill</param>
	/// <param name="pixelValue">The pixel value to fill the rectangles with</param>
	/// <returns><c><see langword="true"/></c>, if the fill operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="pixelValue"/> argument should be a pixel value of the format used by the surface, and can be generated by <see cref="MapColor(byte, byte, byte)"/>, <see cref="MapColor(byte, byte, byte, byte)"/>, or <see cref="MapColor(Color{byte})"/>.
	/// Alternatively, you can use the <see cref="TryFill(ReadOnlySpan{Rect{int}}, byte, byte, byte)"/>, <see cref="TryFill(ReadOnlySpan{Rect{int}}, byte, byte, byte, byte)"/>, or <see cref="TryFill(ReadOnlySpan{Rect{int}}, Color{byte})"/> overloads which will do the mapping for you.
	/// </para>
	/// <para>
	/// If the color value contains an alpha component then the rectangles in <paramref name="destinationRects"/> are simply filled with that alpha information, no blending takes place.
	/// </para>
	/// <para>
	/// If there is a <see cref="ClippingRect"/> set, then this method will fill based on the intersections of the <see cref="ClippingRect"/> and each rectangle in <paramref name="destinationRects"/>.
	/// </para>
	/// </remarks>
	public bool TryFill(ReadOnlySpan<Rect<int>> destinationRects, uint pixelValue)
	{
		unsafe
		{
			fixed (Rect<int>* rects = destinationRects)
			{
				return SDL_FillSurfaceRects(mSurface, rects, destinationRects.Length, pixelValue);
			}
		}
	}

	/// <summary>
	/// Tries to perform a fast fill of multiple rectangles on this surface with the specified color
	/// </summary>
	/// <param name="destinationRects">The list of rectangles to fill</param>
	/// <param name="r">The red component value of the color to fill the rectangle with</param>
	/// <param name="g">The green component value of the color to fill the rectangle with</param>
	/// <param name="b">The blue component value of the color to fill the rectangle with</param>
	/// <returns><c><see langword="true"/></c>, if the fill operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The rectangles in <paramref name="destinationRects"/> are simply filled fully opaque, no blending takes place.
	/// </para>
	/// <para>
	/// If there is a <see cref="ClippingRect"/> set, then this method will fill based on the intersections of the <see cref="ClippingRect"/> and each rectangle in <paramref name="destinationRects"/>.
	/// </para>
	/// </remarks>
	public bool TryFill(ReadOnlySpan<Rect<int>> destinationRects, byte r, byte g, byte b) => TryFill(destinationRects, MapColor(r, g, b));

	/// <summary>
	/// Tries to perform a fast fill of multiple rectangles on this surface with the specified color
	/// </summary>
	/// <param name="destinationRects">The list of rectangles to fill</param>
	/// <param name="r">The red component value of the color to fill the rectangles with</param>
	/// <param name="g">The green component value of the color to fill the rectangles with</param>
	/// <param name="b">The blue component value of the color to fill the rectangles with</param>
	/// <param name="a">The alpha component value of the color to fill the rectangles with</param>
	/// <returns><c><see langword="true"/></c>, if the fill operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The rectangles in <paramref name="destinationRects"/> are simply filled with the <paramref name="a"/>lpha information, no blending takes place.
	/// </para>
	/// <para>
	/// If there is a <see cref="ClippingRect"/> set, then this method will fill based on the intersections of the <see cref="ClippingRect"/> and each rectangle in <paramref name="destinationRects"/>.
	/// </para>
	/// </remarks>
	public bool TryFill(ReadOnlySpan<Rect<int>> destinationRects, byte r, byte g, byte b, byte a) => TryFill(destinationRects, MapColor(r, g, b, a));

	/// <summary>
	/// Tries to perform a fast fill of multiple rectangles on this surface with the specified color
	/// </summary>
	/// <param name="destinationRects">The list of rectangles to fill</param>
	/// <param name="color">The color to fill the rectangles with</param>
	/// <returns><c><see langword="true"/></c>, if the fill operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The rectangles in <paramref name="destinationRects"/> are simply filled with the <see cref="Color{T}.A"/> information, no blending takes place.
	/// </para>
	/// <para>
	/// If there is a <see cref="ClippingRect"/> set, then this method will fill based on the intersections of the <see cref="ClippingRect"/> and each rectangle in <paramref name="destinationRects"/>.
	/// </para>
	/// </remarks>
	public bool TryFill(ReadOnlySpan<Rect<int>> destinationRects, Color<byte> color) => TryFill(destinationRects, MapColor(color));

	/// <summary>
	/// Tries to flip the surface according to the specified flip mode
	/// </summary>
	/// <param name="flipMode">The direction to flip the surface</param>
	/// <returns><c><see langword="true"/></c>, if the surface was flipped successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	public bool TryFlip(FlipMode flipMode)
	{
		unsafe
		{
			return SDL_FlipSurface(mSurface, flipMode);
		}
	}

	internal unsafe static bool TryGet(SDL_Surface* surface, [NotNullWhen(true)] out Surface? result)
	{
		if (surface is not null
			&& mKnownInstances.TryGetValue(unchecked((IntPtr)surface), out var surfaceRef)
			&& surfaceRef.TryGetTarget(out result))
		{
			return true;
		}

		result = null;
		return false;
	}

	/// <remarks>
	/// <para>
	/// Use this only for <see cref="SDL_Surface"/> pointers owned by SDL (e.g. returned from other APIs);
	/// it borrows exactly one native reference for managed tracking and must not be used for owner-created surfaces.
	/// </para>
	/// </remarks>
	internal unsafe static bool TryGetOrCreate(SDL_Surface* surface, [NotNullWhen(true)] out Surface? result)
	{
		if (surface is null)
		{
			result = null;
			return false;
		}

		var surfaceRef = mKnownInstances.GetOrAdd(unchecked((IntPtr)surface), createRef);

		if (!surfaceRef.TryGetTarget(out result))
		{
			surfaceRef.SetTarget(result = create(surface));
		}

		return true;

		static WeakReference<Surface> createRef(IntPtr surface) => new(create(unchecked((SDL_Surface*)surface)));

		static Surface create(SDL_Surface* surface)
		{
			// create is called in both cases, either we register the instance for the first time,
			// or a managed instance was GC'ed and we need to recreate it (potentially for a different native instance).
			// In both cases, that's the ideal place to increase the native ref counter.

			// "Borrow" an additional native reference for remembering the managed instance
			surface->RefCount++;

			return new(surface, register: false);
		}
	}

	/// <summary>
	/// Tries to load a BMP image from a specified file into a new surface
	/// </summary>
	/// <param name="file">A file path to the BMP image to load</param>
	/// <param name="surface">The loaded surface, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the BMP image was loaded successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The format of the file must be BMP, otherwise this method will fail.
	/// </para>
	/// <para>
	/// Remember to <see cref="Dispose()">dispose</see> of the resulting surface when you are done with it. Not doing so will result in a memory leak.
	/// </para>
	/// </remarks>
	public static bool TryLoadBmp(string file, [NotNullWhen(true)] out Surface? surface)
	{
		unsafe
		{
			var fileUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(file);

			try
			{
				var surfacePtr = SDL_LoadBMP(fileUtf8);

				if (surfacePtr is null)
				{
					surface = null;
					return false;
				}

				surface = new(surfacePtr, register: true);
				return true;
			}
			finally
			{
				Utf8StringMarshaller.Free(fileUtf8);
			}
		}
	}

	/// <summary>
	/// Tries to load a BMP image from a specified stream into a new surface
	/// </summary>
	/// <param name="source">The stream to load the BMP image from</param>
	/// <param name="surface">The loaded surface, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <param name="closeAfterwards">A value indicating whether the <paramref name="source"/> stream should be closed before this method returns (even in case of an error)</param>
	/// <returns><c><see langword="true"/></c>, if the BMP image was loaded successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The format of the data in the <paramref name="source"/> stream must be BMP, otherwise this method will fail.
	/// </para>
	/// <para>
	/// Remember to <see cref="Dispose()">dispose</see> of the resulting surface when you are done with it. Not doing so will result in a memory leak.
	/// </para>
	/// </remarks>
	public static bool TryLoadBmp(Stream source, [NotNullWhen(true)] out Surface? surface, bool closeAfterwards = false)
	{
		unsafe
		{
			try
			{
				var surfacePtr = SDL_LoadBMP_IO(source is not null ? source.Pointer : null, closeAfterwards);

				if (surfacePtr is null)
				{
					surface = null;
					return false;
				}

				surface = new(surfacePtr, register: true);
				return true;
			}
			finally
			{
				if (closeAfterwards)
				{
					source?.Dispose(close: false /* SDL_LoadBMP_IO already closed the stream */);
				}
			}
		}
	}

#if SDL3_4_0_OR_GREATER
	/// <summary>
	/// Tries to load a PNG image from a specified file into a new surface
	/// </summary>
	/// <param name="file">A file path to the PNG image to load</param>
	/// <param name="surface">The loaded surface, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the PNG image was loaded successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The format of the file must be PNG, otherwise this method will fail.
	/// </para>
	/// <para>
	/// This method is intended as a convenience method for loading images from trusted sources.
	/// If you want to load arbitrary images you should use an image loading library designed with security in mind.
	/// </para>
	/// <para>
	/// Remember to <see cref="Dispose()">dispose</see> of the resulting surface when you are done with it. Not doing so will result in a memory leak.
	/// </para>
	/// </remarks>
	public static bool TryLoadPng(string file, [NotNullWhen(true)] out Surface? surface)
	{
		unsafe
		{
			var fileUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(file);

			try
			{
				var surfacePtr = SDL_LoadPNG(fileUtf8);

				if (surfacePtr is null)
				{
					surface = null;
					return false;
				}

				surface = new(surfacePtr, register: true);
				return true;
			}
			finally
			{
				Utf8StringMarshaller.Free(fileUtf8);
			}
		}
	}
#endif

#if SDL3_4_0_OR_GREATER
	/// <summary>
	/// Tries to load a PNG image from a specified stream into a new surface
	/// </summary>
	/// <param name="source">The stream to load the PNG image from</param>
	/// <param name="surface">The loaded surface, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <param name="closeAfterwards">A value indicating whether the <paramref name="source"/> stream should be closed before this method returns (even in case of an error)</param>
	/// <returns><c><see langword="true"/></c>, if the PNG image was loaded successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The format of the data in the <paramref name="source"/> stream must be PNG, otherwise this method will fail.
	/// </para>
	/// <para>
	/// This method is intended as a convenience method for loading images from trusted sources.
	/// If you want to load arbitrary images you should use an image loading library designed with security in mind.
	/// </para>
	/// <para>
	/// Remember to <see cref="Dispose()">dispose</see> of the resulting surface when you are done with it. Not doing so will result in a memory leak.
	/// </para>
	/// </remarks>
	public static bool TryLoadPng(Stream source, [NotNullWhen(true)] out Surface? surface, bool closeAfterwards = false)
	{
		unsafe
		{
			try
			{
				var surfacePtr = SDL_LoadPNG_IO(source is not null ? source.Pointer : null, closeAfterwards);

				if (surfacePtr is null)
				{
					surface = null;
					return false;
				}

				surface = new(surfacePtr, register: true);
				return true;
			}
			finally
			{
				if (closeAfterwards)
				{
					source?.Dispose(close: false /* SDL_LoadPNG_IO already closed the stream */);
				}
			}
		}
	}
#endif

	/// <summary>
	/// Tries to set up a pixel memory manager for direct pixel access
	/// </summary>
	/// <param name="pixelManager">The pixel memory manager meant to be used to access the surface's pixel memory, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the surface was locked successfully and the <paramref name="pixelManager"/> was created; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used as a simpler and safer alternative to the <see cref="MustLock"/> - <see cref="IsLocked"/> - <see cref="TryUnsafeLock"/> - <see cref="UnsafePixels"/> - <see cref="UnsafeUnlock"/> pattern.
	/// </para>
	/// <para>
	/// If the <paramref name="pixelManager"/> was created successfully, you can use its <see cref="SurfacePixelMemoryManager.Memory"/> property to read and write the pixel memory of this surface, using this surface's <see cref="Format"/>.
	/// Once you are done accessing the pixel memory, you should <see cref="NativeMemoryManagerBase.Dispose()">dispose</see> the <paramref name="pixelManager"/> to release the surface.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Surface surface;
	/// 
	/// ...
	/// 
	/// if (surface.TryLock(out var pixelManager))
	/// {
	///		using (pixelManager)
	///		{
	///			// You can use pixelManager.Memory here to access the surface's pixel memory here
	///			
	///			...
	///		}
	/// }
	/// </code>
	/// </example>
	public bool TryLock([NotNullWhen(true)] out SurfacePixelMemoryManager? pixelManager) => SurfacePixelMemoryManager.TryCreate(this, out pixelManager);

	/// <summary>
	/// Tries to premultiply the alpha channel into the color channels of this surface
	/// </summary>
	/// <param name="linear">A value indicating whether to convert from sRGB into linear space to do the alpha multiplication, or to do the multiplication in sRGB space</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	public bool TryPremultiplyAlpha(bool linear)
	{
		unsafe
		{
			return SDL_PremultiplySurfaceAlpha(mSurface, linear);
		}
	}

	/// <summary>
	/// Tries to premultiply the alpha channel into the color channels on a block of pixels
	/// </summary>
	/// <param name="width">The width of the block to convert, in pixels</param>
	/// <param name="height">The height of the block to convert, in pixels</param>
	/// <param name="sourceFormat">The pixel format of the source data</param>
	/// <param name="source">The source pixel data</param>
	/// <param name="sourcePitch">The pitch of the source pixel data, in bytes</param>
	/// <param name="destinationFormat">The pixel format of the destination data</param>
	/// <param name="destination">The destination pixel data</param>
	/// <param name="destinationPitch">The pitch of the destination pixel data, in bytes</param>
	/// <param name="linear">A value indicating whether to convert from sRGB into linear space to do the alpha multiplication, or to do the multiplication in sRGB space</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is safe to use when the <paramref name="source"/> pixel data and the <paramref name="destination"/> pixel data start at the same memory location, but they cannot overlap otherwise.
	/// </para>
	/// <para>
	/// This method additionally returns <c><see langword="false"/></c> if either <paramref name="source"/> or <paramref name="destination"/> are <see cref="Utilities.NativeMemory.IsValid">invalid</see>,
	/// <c><paramref name="source"/>.<see cref="ReadOnlyNativeMemory.Length">Length</see></c> is less than; <c><paramref name="height"/> * <paramref name="sourcePitch"/></c>,
	/// or <c><paramref name="destination"/>.<see cref="Utilities.NativeMemory.Length">Length</see></c> is less than <c><paramref name="height"/> * <paramref name="destinationPitch"/></c>.
	/// </para>
	/// </remarks>
	public static bool TryPremultiplyAlpha(int width, int height, PixelFormat sourceFormat, ReadOnlyNativeMemory source, int sourcePitch, PixelFormat destinationFormat, Utilities.NativeMemory destination, int destinationPitch, bool linear)
	{
		unsafe
		{
			if (!source.IsValid || !destination.IsValid)
			{
				return false;
			}

			var sourceRequired = unchecked((height is > 0 ? (nuint)height : 0) * (sourcePitch is > 0 ? (nuint)sourcePitch : 0));

			if (source.Length < sourceRequired)
			{
				return false;
			}

			var destinationRequired = unchecked((height is > 0 ? (nuint)height : 0) * (destinationPitch is > 0 ? (nuint)destinationPitch : 0));

			if (destination.Length < destinationRequired)
			{
				return false;
			}

			return SDL_PremultiplyAlpha(width, height, sourceFormat, source.RawPointer, sourcePitch, destinationFormat, destination.RawPointer, destinationPitch, linear);
		}
	}

	/// <summary>
	/// Tries to premultiply the alpha channel into the color channels on a block of pixels
	/// </summary>
	/// <param name="width">The width of the block to convert, in pixels</param>
	/// <param name="height">The height of the block to convert, in pixels</param>
	/// <param name="sourceFormat">The pixel format of the source data</param>
	/// <param name="source">The source pixel data</param>
	/// <param name="sourcePitch">The pitch of the source pixel data, in bytes</param>
	/// <param name="destinationFormat">The pixel format of the destination data</param>
	/// <param name="destination">The destination pixel data</param>
	/// <param name="destinationPitch">The pitch of the destination pixel data, in bytes</param>
	/// <param name="linear">A value indicating whether to convert from sRGB into linear space to do the alpha multiplication, or to do the multiplication in sRGB space</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is safe to use when the <paramref name="source"/> pixel data and the <paramref name="destination"/> pixel data start at the same memory location, but they cannot overlap otherwise.
	/// </para>
	/// <para>
	/// This method additionally returns <c><see langword="false"/></c> if <c><paramref name="source"/>.<see cref="ReadOnlySpan{T}.Length">Length</see></c> is less than; <c><paramref name="height"/> * <paramref name="sourcePitch"/></c>,
	/// or <c><paramref name="destination"/>.<see cref="Span{T}.Length">Length</see></c> is less than <c><paramref name="height"/> * <paramref name="destinationPitch"/></c>.
	/// </para>
	/// </remarks>
	public static bool TryPremultiplyAlpha(int width, int height, PixelFormat sourceFormat, ReadOnlySpan<byte> source, int sourcePitch, PixelFormat destinationFormat, Span<byte> destination, int destinationPitch, bool linear)
	{
		unsafe
		{
			var sourceRequired = unchecked((height is > 0 ? (nuint)height : 0) * (sourcePitch is > 0 ? (nuint)sourcePitch : 0));

			if (unchecked((nuint)source.Length) < sourceRequired)
			{
				return false;
			}

			var destinationRequired = unchecked((height is > 0 ? (nuint)height : 0) * (destinationPitch is > 0 ? (nuint)destinationPitch : 0));

			if (unchecked((nuint)destination.Length) < destinationRequired)
			{
				return false;
			}

			fixed (byte* src = source, dst = destination)
			{
				return SDL_PremultiplyAlpha(width, height, sourceFormat, src, sourcePitch, destinationFormat, dst, destinationPitch, linear);
			}
		}
	}

	/// <summary>
	/// Tries to premultiply the alpha channel into the color channels on a block of pixels
	/// </summary>
	/// <param name="width">The width of the block to convert, in pixels</param>
	/// <param name="height">The height of the block to convert, in pixels</param>
	/// <param name="sourceFormat">The pixel format of the source data</param>
	/// <param name="source">A pointer to the source pixel data</param>
	/// <param name="sourcePitch">The pitch of the source pixel data, in bytes</param>
	/// <param name="destinationFormat">The pixel format of the destination data</param>
	/// <param name="destination">A pointer to the destination pixel data</param>
	/// <param name="destinationPitch">The pitch of the destination pixel data, in bytes</param>
	/// <param name="linear">A value indicating whether to convert from sRGB into linear space to do the alpha multiplication, or to do the multiplication in sRGB space</param>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is safe to use when the <paramref name="source"/> pixel data and the <paramref name="destination"/> pixel data start at the same memory location, but they cannot overlap otherwise.
	/// </para>
	/// </remarks>
	public unsafe static bool TryPremultiplyAlpha(int width, int height, PixelFormat sourceFormat, void* source, int sourcePitch, PixelFormat destinationFormat, void* destination, int destinationPitch, bool linear)
	{
		return SDL_PremultiplyAlpha(width, height, sourceFormat, source, sourcePitch, destinationFormat, destination, destinationPitch, linear);
	}

	/// <summary>
	/// Tries to read a single pixel from the surface at the specified coordinates
	/// </summary>
	/// <param name="x">The X coordinate. Must be <c>0</c> ≤ <paramref name="x"/> &lt; <see cref="Width"/>.</param>
	/// <param name="y">The Y coordinate. Must be <c>0</c> ≤ <paramref name="y"/> &lt; <see cref="Height"/>.</param>
	/// <param name="r">The red component value of the pixel, if this method returns <c><see langword="true"/></c>. Uses the entire range from <c>0</c> to <c>255</c>.</param>
	/// <param name="g">The green component value of the pixel, if this method returns <c><see langword="true"/></c>. Uses the entire range from <c>0</c> to <c>255</c>.</param>
	/// <param name="b">The blue component value of the pixel, if this method returns <c><see langword="true"/></c>. Uses the entire range from <c>0</c> to <c>255</c>.</param>
	/// <param name="a">The alpha component value of the pixel, if this method returns <c><see langword="true"/></c>. Uses the entire range from <c>0</c> to <c>255</c>.</param>
	/// <returns><c><see langword="true"/></c>, if the pixel was read successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method prioritizes correctness over speed: it is suitable for unit tests, but is not intended to be used in game engines.
	/// </para>
	/// <para>
	/// Like <see cref="PixelFormatDetails.GetColor(uint, Palette?, out byte, out byte, out byte, out byte)"/>, this method uses the entire 8-bit (<c>0</c>-<c>255</c>) range when converting color components from pixel formats with less than 8 bits per RGB component
	/// (e.g., a completely white pixel in 16-bit <see cref="PixelFormat.Rgb565"/> format would return (<paramref name="r"/>, <paramref name="g"/>, <paramref name="b"/>) = (<c>0xff</c>, <c>0xff</c>, <c>0xff</c>),
	/// <em>not</em> (<c>0xf8</c>, <c>0xfc</c>, <c>0xf8</c>)).
	/// </para>
	/// <para>
	/// If the surface has no alpha component, the <paramref name="a"/>lpha component value will be returned as <c>0xff</c> (fully opaque).
	/// </para>
	/// </remarks>
	[OverloadResolutionPriority(1)] // To choose TryReadPixel(int, int, out byte, out byte, out byte, out byte) over TryReadPixel(int, int, out float, out float, out float, out float) when using 'out var'
	public bool TryReadPixel(int x, int y, out byte r, out byte g, out byte b, out byte a)
	{
		unsafe
		{
			Unsafe.SkipInit(out byte rTmp);
			Unsafe.SkipInit(out byte gTmp);
			Unsafe.SkipInit(out byte bTmp);
			Unsafe.SkipInit(out byte aTmp);

			bool result = SDL_ReadSurfacePixel(mSurface, x, y, &rTmp, &gTmp, &bTmp, &aTmp);

			r = rTmp;
			g = gTmp;
			b = bTmp;
			a = aTmp;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a single pixel from the surface at the specified coordinates
	/// </summary>
	/// <param name="x">The X coordinate. Must be <c>0</c> ≤ <paramref name="x"/> &lt; <see cref="Width"/>.</param>
	/// <param name="y">The Y coordinate. Must be <c>0</c> ≤ <paramref name="y"/> &lt; <see cref="Height"/>.</param>
	/// <param name="color">The color of the pixel, if this method returns <c><see langword="true"/></c>. Uses the entire range from <c>0</c> to <c>255</c> for each component.</param>
	/// <returns><c><see langword="true"/></c>, if the pixel was read successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method prioritizes correctness over speed: it is suitable for unit tests, but is not intended to be used in game engines.
	/// </para>
	/// <para>
	/// Like <see cref="PixelFormatDetails.GetColor(uint, Palette?, out Color{byte})"/>, this method uses the entire 8-bit (<c>0</c>-<c>255</c>) range when converting color components from pixel formats with less than 8 bits per RGB component
	/// (e.g., a completely white pixel in 16-bit <see cref="PixelFormat.Rgb565"/> format would return (<see cref="Color{T}.R"/>, <see cref="Color{T}.G"/>, <see cref="Color{T}.B"/>) = (<c>0xff</c>, <c>0xff</c>, <c>0xff</c>),
	/// <em>not</em> (<c>0xf8</c>, <c>0xfc</c>, <c>0xf8</c>))
	/// </para>
	/// <para>
	/// If the surface has no alpha component, the <see cref="Color{T}.A"/> value will be returned as <c>0xff</c> (fully opaque).
	/// </para>
	/// </remarks>
	[OverloadResolutionPriority(1)] // To choose TryReadPixel(int, int, out Color<byte>) over TryReadPixel(int, int, out Color<float>) when using 'out var'
	public bool TryReadPixel(int x, int y, out Color<byte> color)
	{
		if (TryReadPixel(x, y, out byte r, out byte g, out byte b, out byte a))
		{
			color = new(r, g, b, a);
			return true;
		}

		color = default;
		return false;
	}

	/// <summary>
	/// Tries to read a single pixel from the surface at the specified coordinates
	/// </summary>
	/// <param name="x">The X coordinate. Must be <c>0</c> ≤ <paramref name="x"/> &lt; <see cref="Width"/>.</param>
	/// <param name="y">The Y coordinate. Must be <c>0</c> ≤ <paramref name="y"/> &lt; <see cref="Height"/>.</param>
	/// <param name="r">The red component value of the pixel, if this method returns <c><see langword="true"/></c>. Usually in the range from <c>0</c> to <c>1</c>.</param>
	/// <param name="g">The green component value of the pixel, if this method returns <c><see langword="true"/></c>. Usually in the range from <c>0</c> to <c>1</c>.</param>
	/// <param name="b">The blue component value of the pixel, if this method returns <c><see langword="true"/></c>. Usually in the range from <c>0</c> to <c>1</c>.</param>
	/// <param name="a">The alpha component value of the pixel, if this method returns <c><see langword="true"/></c>. Usually in the range from <c>0</c> to <c>1</c>.</param>
	/// <returns><c><see langword="true"/></c>, if the pixel was read successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method prioritizes correctness over speed: it is suitable for unit tests, but is not intended to be used in game engines.
	/// </para>
	/// <para>
	/// If the surface has no alpha component, the <paramref name="a"/>lpha component value will be returned as <c>1</c> (fully opaque).
	/// </para>
	/// </remarks>
	public bool TryReadPixel(int x, int y, out float r, out float g, out float b, out float a)
	{
		unsafe
		{
			Unsafe.SkipInit(out float rTmp);
			Unsafe.SkipInit(out float gTmp);
			Unsafe.SkipInit(out float bTmp);
			Unsafe.SkipInit(out float aTmp);

			bool result = SDL_ReadSurfacePixelFloat(mSurface, x, y, &rTmp, &gTmp, &bTmp, &aTmp);

			r = rTmp;
			g = gTmp;
			b = bTmp;
			a = aTmp;

			return result;
		}
	}

	/// <summary>
	/// Tries to read a single pixel from the surface at the specified coordinates
	/// </summary>
	/// <param name="x">The X coordinate. Must be <c>0</c> ≤ <paramref name="x"/> &lt; <see cref="Width"/>.</param>
	/// <param name="y">The Y coordinate. Must be <c>0</c> ≤ <paramref name="y"/> &lt; <see cref="Height"/>.</param>
	/// <param name="color">The color of the pixel, if this method returns <c><see langword="true"/></c>. Usually the range for each component is from <c>0</c> to <c>1</c>.</param>
	/// <returns><c><see langword="true"/></c>, if the pixel was read successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method prioritizes correctness over speed: it is suitable for unit tests, but is not intended to be used in game engines.
	/// </para>
	/// <para>
	/// If the surface has no alpha component, the <see cref="Color{T}.A"/> value will be returned as <c>1</c> (fully opaque).
	/// </para>
	/// </remarks>
	public bool TryReadPixel(int x, int y, out Color<float> color)
	{
		if (TryReadPixel(x, y, out float r, out float g, out float b, out float a))
		{
			color = Color.From(r, g, b, a);
			return true;
		}

		color = default;
		return false;
	}

#if SDL3_4_0_OR_GREATER
	/// <summary>
	/// Tries to create a copy of this surface rotated clockwise by the specified angle
	/// </summary>
	/// <param name="angle">The angle to rotate the surface clockwise, in degrees</param>
	/// <param name="rotatedSurface">The rotated surface, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the surface was rotated successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="angle"/> of rotation can be negative for counter-clockwise rotation.
	/// </para>
	/// <para>
	/// When the rotation isn't a multiple of 90°, the resulting surface is larger than the original, with the background filled in with the <see cref="ColorKey"/>, if available,
	/// or the color (<see cref="Color{T}.R">R</see> = 255, <see cref="Color{T}.G">G</see> = 255, <see cref="Color{T}.B">B</see> = 255, <see cref="Color{T}.A">A</see> = 0) if not.
	/// </para>
	/// <para>
	/// If surface has the <see cref="Rotation"/> property (<see cref="PropertyNames.RotationFloat"/>) set on it, the new copy will have the adjusted value set:
	/// if the value of the <see cref="Rotation"/> property is <c>90</c> and <paramref name="angle"/> was <c>30</c>, the new surface will have a <see cref="Rotation"/> property value of <c>60</c> (that is: to be upright vs. gravity, the surface needs to rotate 60° more).
	/// However, note that further rotations on the new surface in this example will produce unexpected results, since the image will have resized and padded to accommodate the not-90° angle.
	/// </para>
	/// </remarks>
	public bool TryRotate(float angle, [NotNullWhen(true)] out Surface? rotatedSurface)
	{
		unsafe
		{
			var rotatedPtr = SDL_RotateSurface(mSurface, angle);

			if (rotatedPtr is null)
			{
				rotatedSurface = null;
				return false;
			}

			rotatedSurface = new(rotatedPtr, register: true);
			return true;
		}
	}
#endif

	/// <summary>
	/// Tries to save the surface as a BMP image to the specified file
	/// </summary>
	/// <param name="file">A file path to save the BMP image</param>
	/// <returns><c><see langword="true"/></c>, if the surface was saved successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Surfaces with a 24-bit, 32-bit and paletted 8-bit format get saved in the BMP directly.
	/// Other RGB formats with 8-bit or higher get converted to a 24-bit surface or, if they have an alpha mask or a <see cref="ColorKey"/>, to a 32-bit surface before they are saved.
	/// </para>
	/// <para>
	/// YUV and paletted 1-bit and 4-bit formats are not supported.
	/// </para>
	/// </remarks>
	public bool TrySaveBmp(string file)
	{
		unsafe
		{
			var fileUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(file);
			try
			{
				return SDL_SaveBMP(mSurface, fileUtf8);
			}
			finally
			{
				Utf8StringMarshaller.Free(fileUtf8);
			}
		}
	}

	/// <summary>
	/// Tries to save the surface as a BMP image to the specified stream
	/// </summary>
	/// <param name="destination">The stream to save the BMP image to</param>
	/// <param name="closeAfterwards">A value indicating whether the <paramref name="destination"/> stream should be closed before this method returns (even in case of an error)</param>
	/// <returns><c><see langword="true"/></c>, if the surface was saved successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Surfaces with a 24-bit, 32-bit and paletted 8-bit format get saved in the BMP directly.
	/// Other RGB formats with 8-bit or higher get converted to a 24-bit surface or, if they have an alpha mask or a <see cref="ColorKey"/>, to a 32-bit surface before they are saved.
	/// </para>
	/// <para>
	/// YUV and paletted 1-bit and 4-bit formats are not supported.
	/// </para>
	/// </remarks>
	public bool TrySaveBmp(Stream destination, bool closeAfterwards = false)
	{
		unsafe
		{
			try
			{
				return SDL_SaveBMP_IO(mSurface, destination is not null ? destination.Pointer : null, closeAfterwards);
			}
			finally
			{
				if (closeAfterwards)
				{
					destination?.Dispose(close: false /* SDL_SaveBMP_IO already closed the stream */);
				}
			}
		}
	}

#if SDL3_4_0_OR_GREATER
	/// <summary>
	/// Tries to save the surface as a PNG image to the specified file
	/// </summary>
	/// <param name="file">A file path to save the PNG image</param>
	/// <returns><c><see langword="true"/></c>, if the surface was saved successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	public bool TrySavePng(string file)
	{
		unsafe
		{
			var fileUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(file);
			try
			{
				return SDL_SavePNG(mSurface, fileUtf8);
			}
			finally
			{
				Utf8StringMarshaller.Free(fileUtf8);
			}
		}
	}
#endif

#if SDL3_4_0_OR_GREATER
	/// <summary>
	/// Tries to save the surface as a PNG image to the specified stream
	/// </summary>
	/// <param name="destination">The stream to save the PNG image to</param>
	/// <param name="closeAfterwards">A value indicating whether the <paramref name="destination"/> stream should be closed before this method returns (even in case of an error)</param>
	/// <returns><c><see langword="true"/></c>, if the surface was saved successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	public bool TrySavePng(Stream destination, bool closeAfterwards = false)
	{
		unsafe
		{
			try
			{
				return SDL_SavePNG_IO(mSurface, destination is not null ? destination.Pointer : null, closeAfterwards);
			}
			finally
			{
				if (closeAfterwards)
				{
					destination?.Dispose(close: false /* SDL_SavePNG_IO already closed the stream */);
				}
			}
		}
	}
#endif

	/// <summary>
	/// Tries to create a copy of this surface scaled to the specified width and height
	/// </summary>
	/// <param name="width">The desired width of the scaled surface</param>
	/// <param name="height">The desired height of the scaled surface</param>
	/// <param name="scaleMode">The scaling mode to use</param>
	/// <param name="scaledSurface">The scaled surface, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c> if the surface was scaled successfully; otherwise, <c><see langword="false"/></c></returns>
	public bool TryScale(int width, int height, ScaleMode scaleMode, [NotNullWhen(true)] out Surface? scaledSurface)
	{
		unsafe
		{
			var scaledPtr = SDL_ScaleSurface(mSurface, width, height, scaleMode);

			if (scaledPtr is null)
			{
				scaledSurface = null;
				return false;
			}

			scaledSurface = new(scaledPtr, register: true);
			return true;
		}
	}

	/// <summary>
	/// Tries to set up the surface for direct pixel access
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if the surface was locked successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// It's unsafe to access the <see cref="UnsafePixels"/> property for surfaces that <see cref="MustLock">must be locked</see> and are not currently <see cref="IsLocked">locked</see>.
	/// And it's only safe to do so, if this method returned <see langword="true"/>.
	/// </para>
	/// <para>
	/// Between calls to <see cref="TryUnsafeLock"/> / <see cref="UnsafeUnlock"/>, you can write to and read from <see cref="UnsafePixels"/>, using the surface's <see cref="Format"/>.
	/// Once you are done accessing the surface, you should use <see cref="UnsafeUnlock"/> to release it.
	/// </para>
	/// <para>
	/// This method is meant to be used as part of the <see cref="MustLock"/> - <see cref="IsLocked"/> - <see cref="TryUnsafeLock"/> - <see cref="UnsafePixels"/> - <see cref="UnsafeUnlock"/> pattern,
	/// if you want to access the surface's pixel memory directly in a faster and more efficient way.
	/// If you're looking for a simpler and safer way to access the pixel memory, consider using <see cref="TryLock(out SurfacePixelMemoryManager?)"/> instead.
	/// </para>
	/// <para>
	/// Not all surfaces require locking. If the value of the <see cref="MustLock"/> property is <c><see langword="false"/></c>,
	/// then you can read and write to the surface at any time, and the pixel format of the surface will not change.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Surface surface;
	/// 
	/// ...
	/// 
	/// if (surface.MustLock &amp;&amp; surface.TryUnsafeLock())
	/// {
	///		// It's safe to access surface.UnsafePixels here
	///		
	///		...
	///		
	///		surface.UnsafeUnlock(); 
	/// }
	/// </code>
	/// </example>
	public bool TryUnsafeLock()
	{
		unsafe
		{
			return SDL_LockSurface(mSurface);
		}
	}

	/// <summary>
	/// Tries to write a single pixel to the surface at the specified coordinates
	/// </summary>
	/// <param name="x">The X coordinate of the pixel. Must be <c>0</c> ≤ <paramref name="x"/> &lt; <see cref="Width"/>.</param>
	/// <param name="y">The Y coordinate of the pixel. Must be <c>0</c> ≤ <paramref name="y"/> &lt; <see cref="Height"/>.</param>
	/// <param name="r">The red component value of the pixel. Uses the entire range from <c>0</c> to <c>255</c>.</param>
	/// <param name="g">The green component value of the pixel. Uses the entire range from <c>0</c> to <c>255</c>.</param>
	/// <param name="b">The blue component value of the pixel. Uses the entire range from <c>0</c> to <c>255</c>.</param>
	/// <param name="a">The alpha component value of the pixel. Uses the entire range from <c>0</c> to <c>255</c>.</param>
	/// <returns><c><see langword="true"/></c>, if the pixel was written successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method prioritizes correctness over speed: it is suitable for unit tests, but is not intended to be used in game engines.
	/// </para>
	/// <para>
	/// Like <see cref="PixelFormatDetails.MapColor(Palette?, byte, byte, byte, byte)"/>, this method best approximates a color in the surface's pixel format for the given RGBA color value.
	/// </para>
	/// <para>
	/// If the pixel format has no alpha component, the given <paramref name="a"/>lpha component value will be ignored (as it will be in indexed formats with a <see cref="Palette"/>).
	/// </para>
	/// </remarks>
	[OverloadResolutionPriority(1)] // Actually, that shouldn't be an issue since C# doesn't know byte literals aside from the ambiguous 'default', but we do that for symmetry with 'TryReadPixel'
	public bool TryWritePixel(int x, int y, byte r, byte g, byte b, byte a)
	{
		unsafe
		{
			return SDL_WriteSurfacePixel(mSurface, x, y, r, g, b, a);
		}
	}

	/// <summary>
	/// Tries to write a single pixel to the surface at the specified coordinates
	/// </summary>
	/// <param name="x">The X coordinate of the pixel. Must be <c>0</c> ≤ <paramref name="x"/> &lt; <see cref="Width"/>.</param>
	/// <param name="y">The Y coordinate of the pixel. Must be <c>0</c> ≤ <paramref name="y"/> &lt; <see cref="Height"/>.</param>
	/// <param name="color">The color of the pixel. Uses the entire range from <c>0</c> to <c>255</c> for each component.</param>
	/// <returns><c><see langword="true"/></c>, if the pixel was written successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method prioritizes correctness over speed: it is suitable for unit tests, but is not intended to be used in game engines.
	/// </para>
	/// <para>
	/// Like <see cref="PixelFormatDetails.MapColor(Palette?, Color{byte})"/>, this method best approximates a color in the surface's pixel format for the <see cref="Color{T}">Color</see>&lt;<see cref="byte"/>&gt;.
	/// </para>
	/// <para>
	/// If the pixel format has no alpha component, the given <see cref="Color{T}.A"/> value will be ignored (as it will be in indexed formats with a <see cref="Palette"/>).
	/// </para>
	/// </remarks>
	[OverloadResolutionPriority(1)] // Actually, that shouldn't be an issue since C# doesn't know literal of custom struct types aside from the ambiguous 'default', but we do that for symmetry with 'TryReadPixel'
	public bool TryWritePixel(int x, int y, Color<byte> color)
		=> TryWritePixel(x, y, color.R, color.G, color.B, color.A);

	/// <summary>
	/// Tries to write a single pixel to the surface at the specified coordinates
	/// </summary>
	/// <param name="x">The X coordinate of the pixel. Must be <c>0</c> ≤ <paramref name="x"/> &lt; <see cref="Width"/>.</param>
	/// <param name="y">The Y coordinate of the pixel. Must be <c>0</c> ≤ <paramref name="y"/> &lt; <see cref="Height"/>.</param>
	/// <param name="r">The red component value of the pixel. Usually in the range from <c>0</c> to <c>1</c>.</param>
	/// <param name="g">The green component value of the pixel. Usually in the range from <c>0</c> to <c>1</c>.</param>
	/// <param name="b">The blue component value of the pixel. Usually in the range from <c>0</c> to <c>1</c>.</param>
	/// <param name="a">The alpha component value of the pixel. Usually in the range from <c>0</c> to <c>1</c>.</param>
	/// <returns><c><see langword="true"/></c>, if the pixel was written successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method prioritizes correctness over speed: it is suitable for unit tests, but is not intended to be used in game engines.
	/// </para>
	/// <para>
	/// If the pixel format has no alpha component, the given <paramref name="a"/>lpha component value will be ignored (as it will be in indexed formats with a <see cref="Palette"/>).
	/// </para>
	/// </remarks>
	public bool TryWritePixel(int x, int y, float r, float g, float b, float a)
	{
		unsafe
		{
			return SDL_WriteSurfacePixelFloat(mSurface, x, y, r, g, b, a);
		}
	}

	/// <summary>
	/// Tries to write a single pixel to the surface at the specified coordinates
	/// </summary>
	/// <param name="x">The X coordinate of the pixel. Must be <c>0</c> ≤ <paramref name="x"/> &lt; <see cref="Width"/>.</param>
	/// <param name="y">The Y coordinate of the pixel. Must be <c>0</c> ≤ <paramref name="y"/> &lt; <see cref="Height"/>.</param>
	/// <param name="color">The color of the pixel. Usually the range for each component is from <c>0</c> to <c>1</c>.</param>
	/// <returns><c><see langword="true"/></c>, if the pixel was written successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method prioritizes correctness over speed: it is suitable for unit tests, but is not intended to be used in game engines.
	/// </para>
	/// <para>
	/// If the pixel format has no alpha component, the given <see cref="Color{T}.A"/> value will be ignored (as it will be in indexed formats with a <see cref="Palette"/>).
	/// </para>
	/// </remarks>
	public bool TryWritePixel(int x, int y, Color<float> color)
		=> TryWritePixel(x, y, color.R, color.G, color.B, color.A);

	/// <summary>
	/// Releases the surface after direct pixel access
	/// </summary>
	/// <remarks>
	/// <para>
	/// This method is meant to be used as part of the <see cref="MustLock"/> - <see cref="IsLocked"/> - <see cref="TryUnsafeLock"/> - <see cref="UnsafePixels"/> - <see cref="UnsafeUnlock"/> pattern,
	/// if you want to access the surface's pixel memory directly in a faster and more efficient way.
	/// If you're looking for a simpler and safer way to access the pixel memory, consider using <see cref="TryLock(out SurfacePixelMemoryManager?)"/> instead.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Surface surface;
	/// 
	/// ...
	/// 
	/// if (surface.MustLock &amp;&amp; surface.TryUnsafeLock())
	/// {
	///		// It's safe to access surface.UnsafePixels here
	///		
	///		...
	///		
	///		surface.UnsafeUnlock(); 
	/// }
	/// </code>
	/// </example>
	public void UnsafeUnlock()
	{
		unsafe
		{
			SDL_UnlockSurface(mSurface);
		}
	}
}
