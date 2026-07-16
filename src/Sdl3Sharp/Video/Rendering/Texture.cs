using Sdl3Sharp.Internal;
using Sdl3Sharp.Utilities;
using Sdl3Sharp.Video.Blending;
using Sdl3Sharp.Video.Coloring;
using Sdl3Sharp.Video.Drawing;
using Sdl3Sharp.Video.Rendering.Drivers;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Video.Rendering;

/// <summary>
/// Represents a texture, created by an <see cref="Rendering.Renderer"/>
/// </summary>
/// <remarks>
/// <para>
/// This is an efficient driver-specific representation of pixel data.
/// </para>
/// <para>
/// You can create new textures using the <see cref="Renderer.TryCreateTextureImpl(PixelFormat, TextureAccess, int, int, out Texture?)"/>,
/// <see cref="Renderer.TryCreateTexture(out Texture?, ColorSpace?, PixelFormat?, TextureAccess?, int?, int?, Palette?, float?, float?, Properties?)"/>,
/// or <see cref="Renderer.TryCreateTextureFromSurface(Surface, out Texture?)"/> instance methods on an <see cref="Renderer"/> instance.
/// </para>
/// <para>
/// Please remember to dispose <see cref="Texture"/>s <em>before</em> disposing the <see cref="Renderer"/> that created them!
/// Using an <see cref="Texture"/> after its associated <see cref="Renderer"/> has been disposed can lead to undefined behavior, including corruption and crashes.
/// </para>
/// <para>
/// <see cref="Texture"/>s are not driver-agnostic! Most of the time instance of this abstract class are of the concrete <see cref="Texture{TDriver}"/> type with a specific <see cref="IRenderingDriver">rendering driver</see> as the type argument.
/// However, the <see cref="Texture"/> type exists as an abstraction to use them in common rendering operations with the <see cref="Renderer"/> instance that created them.
/// </para>
/// <para>
/// To specify a concrete texture type, use <see cref="Texture{TDriver}"/> with a rendering driver that implements the <see cref="IRenderingDriver"/> interface (e.g. <see cref="Texture{TDriver}">Texture&lt;<see cref="OpenGL">OpenGL</see>&gt;</see>).
/// </para>
/// </remarks>
public abstract partial class Texture : IDisposable
{
	private static readonly ConcurrentDictionary<IntPtr, WeakReference<Texture>> mKnownInstances = [];

	private unsafe SDL_Texture* mTexture;

	internal unsafe Texture(SDL_Texture* texture, bool register)
	{
		mTexture = texture;

		if (register)
		{
			if (texture is not null)
			{
				// Neither addRef nor updateRef increase the native ref counter for a very simple reason:
				// This method should only be called on a constructor path, where we created the native instance ourselves; thus, its ref counter is already set to 1.
				// That's totally right, since atm the managed wrapper is the sole borrower of a reference to the native instance.

				mKnownInstances.AddOrUpdate(unchecked((IntPtr)texture), addRef, updateRef, this);
			}

			static WeakReference<Texture> addRef(IntPtr texture, Texture newTexture) => new(newTexture);

			static WeakReference<Texture> updateRef(IntPtr texture, WeakReference<Texture> previousTextureRef, Texture newTexture)
			{
				if (previousTextureRef.TryGetTarget(out var previousTexture))
				{
#pragma warning disable IDE0079
#pragma warning disable CA1816
					GC.SuppressFinalize(previousTexture);
#pragma warning restore CA1816
#pragma warning restore IDE0079

					// Dispose should call SDL_DestroyTexture and in turn decrease the ref count, so we don't need to do it here manually
					previousTexture.Dispose(disposing: true, forget: false);
				}

				previousTextureRef.SetTarget(newTexture);

				return previousTextureRef;
			}
		}
	}

	/// <inheritdoc/>
	~Texture() => Dispose(disposing: false, forget: true);

	/// <summary>
	/// Gets the access mode of the texture
	/// </summary>
	/// <value>
	/// The access mode of the texture
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property determines whether the texture is lockable and/or can be used as a render target.
	/// </para>
	/// </remarks>
	public TextureAccess Access => Properties?.TryGetNumberValue(PropertyNames.AccessNumber, out var access) is true
		? unchecked((TextureAccess)access)
		: default;

	/// <summary>
	/// Gets or sets the additional alpha modulation value multiplied into render copy operations
	/// </summary>
	/// <value>
	/// The additional alpha modulation value multiplied into render copy operations, in the range <c>0</c> to <c>255</c>, where <c>0</c> is fully transparent and <c>255</c> is fully opaque
	/// </value>
	/// <remarks>
	/// <para>
	/// When this texture is rendered, during the copy operation the source alpha value is modulated by this alpha value according to the following formula:
	/// <code>
	/// srcA = srcA * (alpha / 255)
	/// </code>
	/// </para>
	/// <para>
	/// Alpha modulation is not always supported by the renderer.
	/// </para>
	/// <para>
	/// The value of this property is equivalent to <c><see cref="AlphaModFloat"/> * 255</c>, rounded to the nearest integer.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public byte AlphaMod
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out byte alpha);

				SDL_GetTextureAlphaMod(mTexture, &alpha);

				return alpha;
			}
		}

		set
		{
			unsafe
			{
				SDL_SetTextureAlphaMod(mTexture, value);
			}
		}
	}

	/// <summary>
	/// Gets or sets the additional alpha modulation value multiplied into render copy operations
	/// </summary>
	/// <value>
	/// The additional alpha modulation value multiplied into render copy operations, in the range <c>0.0</c> to <c>1.0</c>, where <c>0.0</c> is fully transparent and <c>1.0</c> is fully opaque
	/// </value>
	/// <remarks>
	/// <para>
	/// When this texture is rendered, during the copy operation the source alpha value is modulated by this alpha value according to the following formula:
	/// <code>
	/// srcA = srcA * alpha
	/// </code>
	/// </para>
	/// <para>
	/// Alpha modulation is not always supported by the renderer.
	/// </para>
	/// <para>
	/// The value of this property is equivalent to <c><see cref="AlphaMod"/> / 255</c>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public float AlphaModFloat
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out float alpha);

				SDL_GetTextureAlphaModFloat(mTexture, &alpha);

				return alpha;
			}
		}

		set
		{
			unsafe
			{
				SDL_SetTextureAlphaModFloat(mTexture, value);
			}
		}
	}

	/// <summary>
	/// Gets or sets the blend mode used for texture copy operations
	/// </summary>
	/// <value>
	/// The blend mode used for texture copy operations
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting this property, the specified blend is <see cref="BlendMode.Invalid"/> or not supported by the renderer (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </exception>
	public BlendMode BlendMode
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out BlendMode blendMode);

				SDL_GetTextureBlendMode(mTexture, &blendMode);

				return blendMode;
			}

		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetTextureBlendMode(mTexture, value), filterError: GetTextureInvalidTextureErrorMessage());
				// throws if value is BlendMode.Invalid or value is an unsupported blend mode for the renderer
				// Although the offical SDL docs say that "If the blend mode is not supported, the closest supported mode is chosen and this function returns false.",
				// that doesn't appear to be the case looking at the SDL source code.
				// It just fails early if the blend mode is not supported and doesn't try to choose a "closest supported mode".
			}
		}
	}

	/// <summary>
	/// Gets or sets the additional color modulation value multiplied into render copy operations
	/// </summary>
	/// <value>
	/// The additional color modulation value multiplied into render copy operations
	/// </value>
	/// <remarks>
	/// <para>
	/// When this texture is rendered, during the copy operation each source color channel is modulated by the appropriate color value according to the following formula:
	/// <code>
	/// srcC = srcC * (color / 255)
	/// </code>
	/// </para>
	/// <para>
	/// Color modulation is not always supported by the renderer.
	/// </para>
	/// <para>
	/// The value of this property is equivalent to <c>(<see cref="ColorModFloat"/>.R * 255, <see cref="ColorModFloat"/>.G * 255, <see cref="ColorModFloat"/>.B * 255)</c>, each component rounded to the nearest integer.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public (byte R, byte G, byte B) ColorMod
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (byte R, byte G, byte B) color);

				SDL_GetTextureColorMod(mTexture, &color.R, &color.G, &color.B);

				return color;
			}
		}

		set
		{
			unsafe
			{
				SDL_SetTextureColorMod(mTexture, value.R, value.G, value.B);
			}
		}
	}

	/// <summary>
	/// Gets or sets the additional color modulation value multiplied into render copy operations
	/// </summary>
	/// <value>
	/// The additional color modulation value multiplied into render copy operations
	/// </value>
	/// <remarks>
	/// <para>
	/// When this texture is rendered, during the copy operation each source color channel is modulated by the appropriate color value according to the following formula:
	/// <code>
	/// srcC = srcC * color
	/// </code>
	/// </para>
	/// <para>
	/// Color modulation is not always supported by the renderer.
	/// </para>
	/// <para>
	/// The value of this property is equivalent to <c>(<see cref="ColorMod"/>.R / 255, <see cref="ColorMod"/>.G / 255, <see cref="ColorMod"/>.B / 255)</c>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public (float R, float G, float B) ColorModFloat
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (float R, float G, float B) color);

				SDL_GetTextureColorModFloat(mTexture, &color.R, &color.G, &color.B);

				return color;
			}
		}

		set
		{
			unsafe
			{
				SDL_SetTextureColorModFloat(mTexture, value.R, value.G, value.B);
			}
		}
	}

	/// <summary>
	/// Gets the color space of the texture
	/// </summary>
	/// <value>
	/// The color space of the texture
	/// </value>
	public ColorSpace ColorSpace => Properties?.TryGetNumberValue(PropertyNames.ColorSpaceNumber, out var colorSpace) is true
		? unchecked((ColorSpace)colorSpace)
		: default;

	/// <summary>
	/// Gets the pixel format of the texture
	/// </summary>
	/// <value>
	/// The pixel format of the texture
	/// </value>
	public PixelFormat Format
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
				return mTexture is var texture
					&& texture is not null
						? texture->Format
						: default;
			}
		}
	}

	/// <summary>
	/// Gets the maximum dynamic range, in terms of the <see cref="SdrWhitePoint">SDR white point</see>
	/// </summary>
	/// <value>
	/// The maximum dynamic range, in terms of the <see cref="SdrWhitePoint">SDR white point</see>
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is used for HDR10 and floating point textures.
	/// </para>
	/// <para>
	/// The value of this property defaults to <c>1.0</c> for SDR textures, <c>4.0</c> for HDR10 textures, and has no default for floating point textures.
	/// </para>
	/// </remarks>
	public float HdrHeadroom => Properties?.TryGetFloatValue(PropertyNames.HdrHeadroomFloat, out var hdrHeadroom) is true
		? hdrHeadroom
		: default;

	/// <summary>
	/// Gets the height of the texture
	/// </summary>
	/// <value>
	/// The height of the texture, in pixels
	/// </value>
	public int Height
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
				return mTexture is var texture
					&& texture is not null
						? texture->H
						: default;
			}
		}
	}

#if SDL3_4_0_OR_GREATER

	/// <summary>
	/// Gets or sets the palette used by the texture
	/// </summary>
	/// <value>
	/// The palette used by the texture, or <c><see langword="null"/></c> if the texture doesn't use a palette
	/// </value>
	/// <remarks>
	/// <para>
	/// A palette is only used by textures with an <em>indexed</em> <see cref="PixelFormat">pixel format</see>.
	/// </para>
	/// <para>
	/// A single <see cref="Coloring.Palette"/> can be shared between multiple textures.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting this property, the specified palette doesn't match the texture's pixel format (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </exception>
	public Palette? Palette
	{
		get
		{
			unsafe
			{
				if (!Palette.TryGetOrCreate(SDL_GetTexturePalette(mTexture), out var palette))
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
				// We don't need to worry about SDL_SetTexturePalette potentially destroying the old palette,
				// as we should handle that correctly and consistently via the way we handle ref counting in the managed Palette wrapper.
				// If SDL_SetTexturePalette does destroy the old palette, there shouldn't be a registered managed wrapper around it anymore,
				// and if there would be a registered managed wrapper, SDL_SetTexturePalette shouldn't destroy the native instance yet (as it's ref counter shouldn't go to zero).

				SdlErrorHelper.ThrowIfFailed(SDL_SetTexturePalette(mTexture, value is not null ? value.Pointer : null), filterError: GetTextureInvalidTextureErrorMessage());
			}
		}
	}

#endif

	internal unsafe SDL_Texture* Pointer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mTexture; }

	/// <summary>
	/// Gets the properties associated with the texture
	/// </summary>
	/// <value>
	/// The properties associated with the texture, or <c><see langword="null"/></c> if the properties could not be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	public Properties? Properties
	{
		get
		{
			unsafe
			{
				return SDL_GetTextureProperties(mTexture) switch
				{
					0 => null,
					var id => Properties.GetOrCreate(id, registerWithSdl: false)
				};
			}
		}
	}

	private protected abstract Renderer? GetRendererImpl();

	/// <summary>
	/// Gets the renderer that created the texture
	/// </summary>
	/// <value>
	/// The renderer that created the texture, or <c><see langword="null"/></c> if the renderer could not be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	public Renderer? Renderer => GetRendererImpl();

	/// <summary>
	/// Gets or sets the scale mode used for texture scale operations
	/// </summary>
	/// <value>
	/// The scale mode used for texture scale operations
	/// </value>
	/// <remarks>
	/// <para>
	/// The default value of this property is <see cref="ScaleMode.Linear"/>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting this property, the specified scale is <see cref="ScaleMode.Invalid"/> or none of the defined scale modes in <see cref="ScaleMode"/>
	/// </exception>
	public ScaleMode ScaleMode
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out ScaleMode scaleMode);

				SDL_GetTextureScaleMode(mTexture, &scaleMode);

				return scaleMode;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetTextureScaleMode(mTexture, value), filterError: GetTextureInvalidTextureErrorMessage());
				// if value is ScaleMode.Invalid or none of the defined scale modes
				// Although the offical SDL docs say that "If the scale mode is not supported, the closest supported mode is chosen.",
				// that doesn't appear to be the case looking at the SDL source code.
				// It just fails early if the scale mode is not supported and doesn't try to choose a "closest supported mode".
			}
		}
	}

	/// <summary>
	/// Gets the defining value for 100% white
	/// </summary>
	/// <value>
	/// The defining value for 100% white
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is used for HDR10 and floating point textures.
	/// </para>
	/// <para>
	/// The value of this property defines the value of 100% diffuse white, with higher values being displayed in the <see cref="HdrHeadroom">High Dynamic Range headroom</see>.
	/// </para>
	/// <para>
	/// The value defaults to <c>100</c> for HDR textures and <c>1.0</c> for other textures.
	/// </para>
	/// </remarks>
	public float SdrWhitePoint => Properties?.TryGetFloatValue(PropertyNames.SdrWhitePointFloat, out var sdrWhitePoint) is true
		? sdrWhitePoint
		: default;

	/// <summary>
	/// Gets the size of the texture as floating point values
	/// </summary>
	/// <value>
	/// The size (<see cref="Width">width</see>, <see cref="Height">height</see>) of the texture as floating point values, in pixels
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public (float Width, float Height) Size
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (float Width, float Height) size);

				SDL_GetTextureSize(mTexture, &size.Width, &size.Height);

				return size;
			}
		}
	}

	/// <summary>
	/// Gets the width of the texture
	/// </summary>
	/// <value>
	/// The width of the texture, in pixels
	/// </value>
	public int Width
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get
		{
			unsafe
			{
				return mTexture is var texture
					&& texture is not null
						? texture->W
						: default;
			}
		}
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(disposing: true, forget: true);
	}

	private protected virtual void Dispose(bool disposing, bool forget)
	{
		unsafe
		{
			if (mTexture is not null)
			{
				if (forget)
				{
					mKnownInstances.TryRemove(unchecked((IntPtr)mTexture), out _);

				}

				SDL_DestroyTexture(mTexture);
				mTexture = null;
			}
		}
	}

	internal unsafe static bool TryGetOrCreate(SDL_Texture* texture, [NotNullWhen(true)] out Texture? result)
	{
		if (texture is null)
		{
			result = null;
			return false;
		}

		var textureRef = mKnownInstances.GetOrAdd(unchecked((IntPtr)texture), createRef);

		if (!textureRef.TryGetTarget(out result))
		{
			textureRef.SetTarget(result = create(texture));
		}

		return true;

		static WeakReference<Texture> createRef(IntPtr texture) => new(create(unchecked((SDL_Texture*)texture)));

		static Texture create(SDL_Texture* texture)
		{
			// create is called in both cases, either we register the instance for the first time,
			// or a managed instance was GC'ed and we need to recreate it (potentially for a different native instance).
			// In both cases, that's the ideal place to increase the native ref counter.
			// "Borrow" an additional native reference for remembering the managed instance
			texture->RefCount++;

			// try to identify the best matching registered driver for the texture and create the managed wrapper accordingly,
			// if that fails, fall back to the generic unknown driver
			if (!TryCreateFromRegisteredDriver(texture, register: false, out var result))
			{
				result = new Texture<GenericFallbackRendereringDriver>(texture, register: false);
			}

			return result;
		}
	}

	private protected unsafe static bool TryGetOrCreate<TDriver>(SDL_Texture* texture, [NotNullWhen(true)] out Texture<TDriver>? result)
		where TDriver : notnull, IRenderingDriver
	{
		if (texture is null)
		{
			result = null;
			return false;
		}

		var textureRef = mKnownInstances.GetOrAdd(unchecked((IntPtr)texture), createRef);

		if (!textureRef.TryGetTarget(out var baseResult))
		{
			textureRef.SetTarget(result = create(texture));
		}
		else if (baseResult is Texture<TDriver> typedResult)
		{
			// we optimistically assume that everything's fine, if the managed types match

			result = typedResult;
		}
		else if (baseResult.Pointer is not null)
		{
			// this also means that baseResult.Pointer == texture
			// this indicates that we actually need the texture to be of a different managed type than it currently is,
			// we should just fail in that case

			result = null;
			return false;
		}
		else
		{
			// this indicates that we somehow managed to not properly forget a managed instance that was disposed,
			// so we need to fully recreate the managed instance with the new type here, including increasing the native ref counter

			result = create(texture);
		}

		return true;

		static WeakReference<Texture> createRef(IntPtr texture) => new(create(unchecked((SDL_Texture*)texture)));

		static Texture<TDriver> create(SDL_Texture* texture)
		{
			// create is called in both cases, either we register the instance for the first time,
			// or a managed instance was GC'ed and we need to recreate it (potentially for a different native instance).
			// In both cases, that's the ideal place to increase the native ref counter.
			// "Borrow" an additional native reference for remembering the managed instance
			texture->RefCount++;

			return new(texture, register: false);
		}
	}

	private protected abstract bool TryLockImpl(in Rect<int> rect, [NotNullWhen(true)] out TexturePixelMemoryManager? pixelManager);

	/// <summary>
	/// Tries to lock an area of the texture for write-only pixel access and set up a pixel memory manager
	/// </summary>
	/// <param name="rect">The area of the texture to lock for write-only pixel access</param>
	/// <param name="pixelManager">The pixel memory manager meant to be used to access the texture's pixels, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully locked for write-only pixel access successfully and the <paramref name="pixelManager"/> was created; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used as a simpler and safer alternative to using <see cref="TryUnsafeLock(in Rect{int}, out Utilities.NativeMemory, out int)"/> and <see cref="UnsafeUnlock"/>.
	/// </para>
	/// <para>
	/// This method fails and returns <c><see langword="false"/></c> if the texture's <see cref="Access"/> is not <see cref="TextureAccess.Streaming"/>.
	/// </para>
	/// <para>
	/// If the <paramref name="pixelManager"/> was created successfully, you can use its <see cref="TexturePixelMemoryManager.Memory"/> property to write the pixel memory of the locked area, using this texture's <see cref="Format"/>.
	/// Once you're done accessing the pixel memory, you should dispose the <paramref name="pixelManager"/> to unlock the texture and apply the changes.
	/// </para>
	/// <para>
	/// As an optimization, the pixels made available for editing don't necessarily contain the old texture data.
	/// This is a write-only operation, and if you need to keep a copy of the texture data you should do that at the application level.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it (disposing the <paramref name="pixelManager"/>), as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	/// 
	/// ...
	/// 
	/// if (texture.TryLock(in rect, out var pixelManager))
	/// {
	///		using (pixelManager)
	///		{
	///			// Write-only access to the texture's pixels as a Surface through 'pixelManager.Memory' using 'pixelManager.Pitch'
	///			// Make sure to fully initialize ALL the pixels locked
	///			
	///			...
	///		}
	///	}
	/// </code>
	/// </example>
	public bool TryLock(in Rect<int> rect, [NotNullWhen(true)] out TexturePixelMemoryManager? pixelManager)
		=> TryLockImpl(in rect, out pixelManager);

	private protected abstract bool TryLockImpl([NotNullWhen(true)] out TexturePixelMemoryManager? pixelManager);

	/// <summary>
	/// Tries to lock the entire texture for write-only pixel access and set up a pixel memory manager
	/// </summary>
	/// <param name="pixelManager">The pixel memory manager meant to be used to access the texture's pixels, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully locked for write-only pixel access successfully and the <paramref name="pixelManager"/> was created; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used as a simpler and safer alternative to using <see cref="TryUnsafeLock(out Utilities.NativeMemory, out int)"/> and <see cref="UnsafeUnlock"/>.
	/// </para>
	/// <para>
	/// This method fails and returns <c><see langword="false"/></c> if the texture's <see cref="Access"/> is not <see cref="TextureAccess.Streaming"/>.
	/// </para>
	/// <para>
	/// If the <paramref name="pixelManager"/> was created successfully, you can use its <see cref="TexturePixelMemoryManager.Memory"/> property to write the pixel memory of the entire texture, using this texture's <see cref="Format"/>.
	/// Once you're done accessing the pixel memory, you should dispose the <paramref name="pixelManager"/> to unlock the texture and apply the changes.
	/// </para>
	/// <para>
	/// As an optimization, the pixels made available for editing don't necessarily contain the old texture data.
	/// This is a write-only operation, and if you need to keep a copy of the texture data you should do that at the application level.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it (disposing the <paramref name="pixelManager"/>), as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	/// 
	/// ...
	/// 
	/// if (texture.TryLock(out var pixelManager))
	/// {
	///		using (pixelManager)
	///		{
	///			// Write-only access to the texture's pixels as a Surface through 'pixelManager.Memory' using 'pixelManager.Pitch'
	///			// Make sure to fully initialize ALL the pixels locked
	///			
	///			...
	///		}
	///	}
	/// </code>
	/// </example>
	public bool TryLock([NotNullWhen(true)] out TexturePixelMemoryManager? pixelManager)
		=> TryLockImpl(out pixelManager);

	private protected abstract bool TryLockToSurfaceImpl(in Rect<int> rect, [NotNullWhen(true)] out TextureSurfaceManager? surfaceManager);

	/// <summary>
	/// Tries to lock an area of the texture for write-only pixel access and set up a surface manager
	/// </summary>
	/// <param name="rect">The area of the texture to lock for write-only pixel access</param>
	/// <param name="surfaceManager">The surface manager meant to be used to access the texture's pixels, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully locked for write-only pixel access successfully and the <paramref name="surfaceManager"/> was created; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used as a simpler and safer alternative to using <see cref="TryUnsafeLockToSurface(in Rect{int}, out Surface?)"/> and <see cref="UnsafeUnlock"/>.
	/// </para>
	/// <para>
	/// This method fails and returns <c><see langword="false"/></c> if the texture's <see cref="Access"/> is not <see cref="TextureAccess.Streaming"/>.
	/// </para>
	/// <para>
	/// If the <paramref name="surfaceManager"/> was created successfully, you can use its <see cref="TextureSurfaceManager.Surface"/> property to access the pixels of the locked area as a <see cref="Surface"/>.
	/// Once you're done accessing the pixels, you should dispose the <paramref name="surfaceManager"/> to unlock the texture and apply the changes.
	/// </para>
	/// <para>
	/// As an optimization, the pixels made available for editing don't necessarily contain the old texture data.
	/// This is a write-only operation, and if you need to keep a copy of the texture data you should do that at the application level.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it (disposing the <paramref name="surfaceManager"/>), as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	/// 
	/// ...
	/// 
	/// if (texture.TryLockToSurface(in rect, out var surfaceManager))
	/// {
	///		using (surfaceManager)
	///		{
	///			// Write-only access to the texture's pixels as a Surface through 'surfaceManager.Surface'
	///			// Make sure to fully initialize ALL the pixels locked
	///			
	///			...
	///		}
	///	}
	/// </code>
	/// </example>
	public bool TryLockToSurface(in Rect<int> rect, [NotNullWhen(true)] out TextureSurfaceManager? surfaceManager)
		=> TryLockToSurfaceImpl(in rect, out surfaceManager);

	private protected abstract bool TryLockToSurfaceImpl([NotNullWhen(true)] out TextureSurfaceManager? surfaceManager);

	/// <summary>
	/// Tries to lock the entire texture for write-only pixel access and set up a surface manager
	/// </summary>
	/// <param name="surfaceManager">The surface manager meant to be used to access the texture's pixels, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully locked for write-only pixel access successfully and the <paramref name="surfaceManager"/> was created; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used as a simpler and safer alternative to using <see cref="TryUnsafeLockToSurface(out Surface?)"/> and <see cref="UnsafeUnlock"/>.
	/// </para>
	/// <para>
	/// This method fails and returns <c><see langword="false"/></c> if the texture's <see cref="Access"/> is not <see cref="TextureAccess.Streaming"/>.
	/// </para>
	/// <para>
	/// If the <paramref name="surfaceManager"/> was created successfully, you can use its <see cref="TextureSurfaceManager.Surface"/> property to access the pixels of the entire texture as a <see cref="Surface"/>.
	/// Once you're done accessing the pixels, you should dispose the <paramref name="surfaceManager"/> to unlock the texture and apply the changes.
	/// </para>
	/// <para>
	/// As an optimization, the pixels made available for editing don't necessarily contain the old texture data.
	/// This is a write-only operation, and if you need to keep a copy of the texture data you should do that at the application level.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it (disposing the <paramref name="surfaceManager"/>), as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	/// 
	/// ...
	/// 
	/// if (texture.TryLockToSurface(out var surfaceManager))
	/// {
	///		using (surfaceManager)
	///		{
	///			// Write-only access to the texture's pixels as a Surface through 'surfaceManager.Surface'
	///			// Make sure to fully initialize ALL the pixels locked
	///			
	///			...
	///		}
	///	}
	/// </code>
	/// </example>
	public bool TryLockToSurface([NotNullWhen(true)] out TextureSurfaceManager? surfaceManager)
		=> TryLockToSurfaceImpl(out surfaceManager);

	/// <summary>
	/// Tries to lock an area of the texture for write-only pixel access
	/// </summary>
	/// <param name="rect">The area of the texture to lock for write-only pixel access</param>
	/// <param name="pixels">The pixel memory of the locked area, if this method returns <c><see langword="true"/></c></param>
	/// <param name="pitch">
	/// The pitch of the locked area, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the width of the locked area, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully locked for write-only pixel access successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used in conjuction with <see cref="UnsafeUnlock"/>, if you want to access the texture's pixels directly in a faster and more efficient way.
	/// If you're locking for a simpler and safer way to access the texture's pixels, consider using <see cref="TryLock(in Rect{int}, out TexturePixelMemoryManager)"/> instead.
	/// </para>
	/// <para>
	/// Once you're done accessing the pixel memory, you should call <see cref="UnsafeUnlock"/> to unlock the texture and apply the changes.
	/// </para>
	/// <para>
	/// This method fails and returns <c><see langword="false"/></c> if the texture's <see cref="Access"/> is not <see cref="TextureAccess.Streaming"/>.
	/// </para>
	/// <para>
	/// As an optimization, the pixels made available for editing don't necessarily contain the old texture data.
	/// This is a write-only operation, and if you need to keep a copy of the texture data you should do that at the application level.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it, as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	/// 
	/// ...
	/// 
	/// if (texture.TryUnsafeLock(in rect, out NativeMemory pixels, out var pitch))
	/// {
	///		// Write-only access to the pixel memory using 'pitch'
	///		// Make sure to fully initialize ALL the pixels locked
	///		
	///		...
	///		
	///		texture.UnsafeUnlock();
	/// }
	/// </code>
	/// </example>
	public bool TryUnsafeLock(in Rect<int> rect, out Utilities.NativeMemory pixels, out int pitch)
	{
		unsafe
		{
			void* pixelsTmp;
			Unsafe.SkipInit(out int pitchTmp);

			bool result;
			fixed (Rect<int>* rectPtr = &rect)
			{
				result = SDL_LockTexture(mTexture, rectPtr, &pixelsTmp, &pitchTmp);
			}

			pitch = pitchTmp;

			if (!result)
			{
				pixels = Utilities.NativeMemory.Empty;
				return false;
			}

			pixels = new(pixelsTmp, unchecked((nuint)rect.Height * (nuint)pitch));
			return true;
		}
	}

	/// <summary>
	/// Tries to lock an area of the texture for write-only pixel access
	/// </summary>
	/// <param name="rect">The area of the texture to lock for write-only pixel access</param>
	/// <param name="pixels">The pixel memory of the locked area, if this method returns <c><see langword="true"/></c></param>
	/// <param name="pitch">
	/// The pitch of the locked area, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the width of the locked area, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully locked for write-only pixel access successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used in conjuction with <see cref="UnsafeUnlock"/>, if you want to access the texture's pixels directly in a faster and more efficient way.
	/// If you're locking for a simpler and safer way to access the texture's pixels, consider using <see cref="TryLock(in Rect{int}, out TexturePixelMemoryManager)"/> instead.
	/// </para>
	/// <para>
	/// Once you're done accessing the pixel memory, you should call <see cref="UnsafeUnlock"/> to unlock the texture and apply the changes.
	/// </para>
	/// <para>
	/// This method fails and returns <c><see langword="false"/></c> if the texture's <see cref="Access"/> is not <see cref="TextureAccess.Streaming"/>.
	/// </para>
	/// <para>
	/// As an optimization, the pixels made available for editing don't necessarily contain the old texture data.
	/// This is a write-only operation, and if you need to keep a copy of the texture data you should do that at the application level.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it, as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	/// 
	/// ...
	/// 
	/// if (texture.TryUnsafeLock(in rect, out Span&lt;byte&gt; pixels, out var pitch))
	/// {
	///		// Write-only access to the pixel memory using 'pitch'
	///		// Make sure to fully initialize ALL the pixels locked
	///		
	///		...
	///		
	///		texture.UnsafeUnlock();
	/// }
	/// </code>
	/// </example>
	public bool TryUnsafeLock(in Rect<int> rect, out Span<byte> pixels, out int pitch)
	{
		unsafe
		{
			void* pixelsTmp;
			Unsafe.SkipInit(out int pitchTmp);

			bool result;
			fixed (Rect<int>* rectPtr = &rect)
			{
				result = SDL_LockTexture(mTexture, rectPtr, &pixelsTmp, &pitchTmp);
			}

			pitch = pitchTmp;

			if (!result)
			{
				pixels = [];
				return false;
			}

			pixels = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(pixelsTmp), unchecked(rect.Height * pitch) switch { < 0 => 0, var length => length });
			return true;
		}
	}

	/// <summary>
	/// Tries to lock an area of the texture for write-only pixel access
	/// </summary>
	/// <param name="rect">The area of the texture to lock for write-only pixel access</param>
	/// <param name="pixels">The pixel memory of the locked area, if this method returns <c><see langword="true"/></c></param>
	/// <param name="pitch">
	/// The pitch of the locked area, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the width of the locked area, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully locked for write-only pixel access successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used in conjuction with <see cref="UnsafeUnlock"/>, if you want to access the texture's pixels directly in a faster and more efficient way.
	/// If you're locking for a simpler and safer way to access the texture's pixels, consider using <see cref="TryLock(in Rect{int}, out TexturePixelMemoryManager)"/> instead.
	/// </para>
	/// <para>
	/// Once you're done accessing the pixel memory, you should call <see cref="UnsafeUnlock"/> to unlock the texture and apply the changes.
	/// </para>
	/// <para>
	/// This method fails and returns <c><see langword="false"/></c> if the texture's <see cref="Access"/> is not <see cref="TextureAccess.Streaming"/>.
	/// </para>
	/// <para>
	/// As an optimization, the pixels made available for editing don't necessarily contain the old texture data.
	/// This is a write-only operation, and if you need to keep a copy of the texture data you should do that at the application level.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it, as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	/// 
	/// ...
	/// 
	/// if (texture.TryUnsafeLock(in rect, out void* pixels, out var pitch))
	/// {
	///		// Write-only access to the pixel memory using 'pitch'
	///		// Make sure to fully initialize ALL the pixels locked
	///		
	///		...
	///		
	///		texture.UnsafeUnlock();
	/// }
	/// </code>
	/// </example>
	public unsafe bool TryUnsafeLock(in Rect<int> rect, out void* pixels, out int pitch)
	{
		void* pixelsTmp;
		Unsafe.SkipInit(out int pitchTmp);

		bool result;
		fixed (Rect<int>* rectPtr = &rect)
		{
			result = SDL_LockTexture(mTexture, rectPtr, &pixelsTmp, &pitchTmp);
		}

		pitch = pitchTmp;
		pixels = pixelsTmp;
		return result;
	}

	/// <summary>
	/// Tries to lock the entire texture for write-only pixel access
	/// </summary>
	/// <param name="pixels">The pixel memory of the texture, if this method returns <c><see langword="true"/></c></param>
	/// <param name="pitch">
	/// The pitch of the texture, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the width of the texture, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully locked for write-only pixel access successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used in conjuction with <see cref="UnsafeUnlock"/>, if you want to access the texture's pixels directly in a faster and more efficient way.
	/// If you're locking for a simpler and safer way to access the texture's pixels, consider using <see cref="TryLock(out TexturePixelMemoryManager)"/> instead.
	/// </para>
	/// <para>
	/// Once you're done accessing the pixel memory, you should call <see cref="UnsafeUnlock"/> to unlock the texture and apply the changes.
	/// </para>
	/// <para>
	/// This method fails and returns <c><see langword="false"/></c> if the texture's <see cref="Access"/> is not <see cref="TextureAccess.Streaming"/>.
	/// </para>
	/// <para>
	/// As an optimization, the pixels made available for editing don't necessarily contain the old texture data.
	/// This is a write-only operation, and if you need to keep a copy of the texture data you should do that at the application level.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it, as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	/// 
	/// ...
	/// 
	/// if (texture.TryUnsafeLock(out NativeMemory pixels, out var pitch))
	/// {
	///		// Write-only access to the pixel memory using 'pitch'
	///		// Make sure to fully initialize ALL the pixels locked
	///		
	///		...
	///		
	///		texture.UnsafeUnlock();
	/// }
	/// </code>
	/// </example>
	public bool TryUnsafeLock(out Utilities.NativeMemory pixels, out int pitch)
	{
		unsafe
		{
			void* pixelsTmp;
			Unsafe.SkipInit(out int pitchTmp);

			bool result = SDL_LockTexture(mTexture, null, &pixelsTmp, &pitchTmp);

			pitch = pitchTmp;

			if (!result)
			{
				pixels = Utilities.NativeMemory.Empty;
				return false;
			}

			pixels = new(pixelsTmp, unchecked((nuint)Height * (nuint)pitch));
			return true;
		}
	}

	/// <summary>
	/// Tries to lock the entire texture for write-only pixel access
	/// </summary>
	/// <param name="pixels">The pixel memory of the texture, if this method returns <c><see langword="true"/></c></param>
	/// <param name="pitch">
	/// The pitch of the texture, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the width of the texture, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully locked for write-only pixel access successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used in conjuction with <see cref="UnsafeUnlock"/>, if you want to access the texture's pixels directly in a faster and more efficient way.
	/// If you're locking for a simpler and safer way to access the texture's pixels, consider using <see cref="TryLock(out TexturePixelMemoryManager)"/> instead.
	/// </para>
	/// <para>
	/// Once you're done accessing the pixel memory, you should call <see cref="UnsafeUnlock"/> to unlock the texture and apply the changes.
	/// </para>
	/// <para>
	/// This method fails and returns <c><see langword="false"/></c> if the texture's <see cref="Access"/> is not <see cref="TextureAccess.Streaming"/>.
	/// </para>
	/// <para>
	/// As an optimization, the pixels made available for editing don't necessarily contain the old texture data.
	/// This is a write-only operation, and if you need to keep a copy of the texture data you should do that at the application level.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it, as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	/// 
	/// ...
	/// 
	/// if (texture.TryUnsafeLock(out Span&lt;byte&gt; pixels, out var pitch))
	/// {
	///		// Write-only access to the pixel memory using 'pitch'
	///		// Make sure to fully initialize ALL the pixels locked
	///		
	///		...
	///		
	///		texture.UnsafeUnlock();
	/// }
	/// </code>
	/// </example>
	public bool TryUnsafeLock(out Span<byte> pixels, out int pitch)
	{
		unsafe
		{
			void* pixelsTmp;
			Unsafe.SkipInit(out int pitchTmp);

			bool result = SDL_LockTexture(mTexture, null, &pixelsTmp, &pitchTmp);

			pitch = pitchTmp;

			if (!result)
			{
				pixels = [];
				return false;
			}

			pixels = MemoryMarshal.CreateSpan(ref Unsafe.AsRef<byte>(pixelsTmp), unchecked(Height * pitch) switch { < 0 => 0, var length => length });
			return true;
		}
	}

	/// <summary>
	/// Tries to lock the entire texture for write-only pixel access
	/// </summary>
	/// <param name="pixels">The pixel memory of the texture, if this method returns <c><see langword="true"/></c></param>
	/// <param name="pitch">
	/// The pitch of the texture, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the width of the texture, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully locked for write-only pixel access successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used in conjuction with <see cref="UnsafeUnlock"/>, if you want to access the texture's pixels directly in a faster and more efficient way.
	/// If you're locking for a simpler and safer way to access the texture's pixels, consider using <see cref="TryLock(out TexturePixelMemoryManager)"/> instead.
	/// </para>
	/// <para>
	/// Once you're done accessing the pixel memory, you should call <see cref="UnsafeUnlock"/> to unlock the texture and apply the changes.
	/// </para>
	/// <para>
	/// This method fails and returns <c><see langword="false"/></c> if the texture's <see cref="Access"/> is not <see cref="TextureAccess.Streaming"/>.
	/// </para>
	/// <para>
	/// As an optimization, the pixels made available for editing don't necessarily contain the old texture data.
	/// This is a write-only operation, and if you need to keep a copy of the texture data you should do that at the application level.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it, as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	/// 
	/// ...
	/// 
	/// if (texture.TryUnsafeLock(out void* pixels, out var pitch))
	/// {
	///		// Write-only access to the pixel memory using 'pitch'
	///		// Make sure to fully initialize ALL the pixels locked
	///		
	///		...
	///		
	///		texture.UnsafeUnlock();
	/// }
	/// </code>
	/// </example>
	public unsafe bool TryUnsafeLock(out void* pixels, out int pitch)
	{
		void* pixelsTmp;
		Unsafe.SkipInit(out int pitchTmp);

		bool result = SDL_LockTexture(mTexture, null, &pixelsTmp, &pitchTmp);

		pitch = pitchTmp;
		pixels = pixelsTmp;

		return result;
	}

	/// <summary>
	/// Tries to lock an area of the texture for write-only pixel access and set up a <see cref="Surface"/> for it
	/// </summary>
	/// <param name="rect">The area of the texture to lock for write-only pixel access</param>
	/// <param name="surface">The surface meant to be used to access the texture's pixels, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully locked for write-only pixel access and the <see cref="Surface"/> was created; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used in conjuction with <see cref="UnsafeUnlock"/>, if you want to access the texture's pixels directly in a faster and more efficient way.
	/// If you're locking for a simpler and safer way to access the texture's pixels, consider using <see cref="TryLockToSurface(in Rect{int}, out TextureSurfaceManager?)"/> instead.
	/// </para>
	/// <para>
	/// Once you're done accessing the pixel memory, you should call <see cref="UnsafeUnlock"/> to unlock the texture and apply the changes.
	/// </para>
	/// <para>
	/// This method fails and returns <c><see langword="false"/></c> if the texture's <see cref="Access"/> is not <see cref="TextureAccess.Streaming"/>.
	/// </para>
	/// <para>
	/// As an optimization, the pixels made available for editing don't necessarily contain the old texture data.
	/// This is a write-only operation, and if you need to keep a copy of the texture data you should do that at the application level.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it, as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	///
	/// ...
	///
	/// if (texture.TryUnsafeLockToSurface(in rect, out var surface))
	/// {
	///		// Write-only access to the pixel memory using 'surface'
	///		// Make sure to fully initialize ALL the pixels locked
	///		
	///		...
	///		
	///		texture.UnsafeUnlock();
	/// }
	/// </code>
	/// </example>
	public bool TryUnsafeLockToSurface(in Rect<int> rect, [NotNullWhen(true)] out Surface? surface)
	{
		unsafe
		{
			Surface.SDL_Surface* surfacePtr;

			fixed (Rect<int>* rectPtr = &rect)
			{
				if (!(bool)SDL_LockTextureToSurface(mTexture, rectPtr, &surfacePtr))
				{
					surface = null;
					return false;
				}
			}

			if (!Surface.TryGetOrCreate(surfacePtr, out surface))
			{
				// if we somehow fail to create the surface, we need to unlock the texture in order for the native surface to be safely disposed
				SDL_UnlockTexture(mTexture);

				surface = null;
				return false;
			}

			// TODO: docs: warn user that they must dispose the returned surface before unlocking the texture!
			return true;
		}
	}

	/// <summary>
	/// Tries to lock the entire texture for write-only pixel access and set up a <see cref="Surface"/> for it
	/// </summary>
	/// <param name="surface">The surface meant to be used to access the texture's pixels, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully locked for write-only pixel access and the <see cref="Surface"/> was created; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant to be used in conjuction with <see cref="UnsafeUnlock"/>, if you want to access the texture's pixels directly in a faster and more efficient way.
	/// If you're locking for a simpler and safer way to access the texture's pixels, consider using <see cref="TryLockToSurface(out TextureSurfaceManager?)"/> instead.
	/// </para>
	/// <para>
	/// Once you're done accessing the pixel memory, you should call <see cref="UnsafeUnlock"/> to unlock the texture and apply the changes.
	/// </para>
	/// <para>
	/// This method fails and returns <c><see langword="false"/></c> if the texture's <see cref="Access"/> is not <see cref="TextureAccess.Streaming"/>.
	/// </para>
	/// <para>
	/// As an optimization, the pixels made available for editing don't necessarily contain the old texture data.
	/// This is a write-only operation, and if you need to keep a copy of the texture data you should do that at the application level.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it, as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	///
	/// ...
	///
	/// if (texture.TryUnsafeLockToSurface(out var surface))
	/// {
	///		// Write-only access to the pixel memory using 'surface'
	///		// Make sure to fully initialize ALL the pixels locked
	///		
	///		...
	///		
	///		texture.UnsafeUnlock();
	/// }
	/// </code>
	/// </example>
	public bool TryUnsafeLockToSurface([NotNullWhen(true)] out Surface? surface)
	{
		unsafe
		{
			Surface.SDL_Surface* surfacePtr;

			if (!(bool)SDL_LockTextureToSurface(mTexture, null, &surfacePtr))
			{
				surface = null;
				return false;
			}

			if (!Surface.TryGetOrCreate(surfacePtr, out surface))
			{
				// if we somehow fail to create the surface, we need to unlock the texture in order for the native surface to be safely disposed
				SDL_UnlockTexture(mTexture);

				surface = null;
				return false;
			}

			// TODO: docs: warn user that they must dispose the returned surface before unlocking the texture!
			return true;
		}
	}

	/// <summary>
	/// Tries to update an area of the texture with new pixel data
	/// </summary>
	/// <param name="rect">The area of the texture to update with the new pixel data</param>
	/// <param name="pixels">The new pixel data to be copied into the specified area of the texture, in the <see cref="Format">pixel format</see> of the texture</param>
	/// <param name="pitch">
	/// The pitch used in the given <paramref name="pixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="pixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The given <paramref name="pixels"/> data must be in the <see cref="Format">pixel format</see> of the texture.
	/// </para>
	/// <para>
	/// This is a fairly slow operation, intended for use with <see cref="TextureAccess.Static">static</see> textures that do not change often.
	/// </para>
	/// <para>
	/// If the texture is intended to be updated often, you should prefer to create the texture as <see cref="TextureAccess.Streaming">streaming</see> and use one of the locking mechanisms to manipulate the texture's pixels (e.g. <see cref="TryLock(in Rect{int}, out TexturePixelMemoryManager)"/>) instead.
	/// While this method still works with <see cref="TextureAccess.Streaming">streaming</see> textures, for optimization reasons you may not get the pixels back if you lock the texture afterward.
	/// </para>
	/// <para>
	/// Using this method for NV12, NV21, YV12 or IYUV textures is perfectly fine as long as the given <paramref name="pixels"/> data is a contiguous block of NV12/N21 planes or Y and UV planes, respectively, in the proper order.
	/// If not, you should use <see cref="TryUpdateNv(in Rect{int}, ReadOnlyNativeMemory{byte}, int, ReadOnlyNativeMemory{byte}, int)"/> or <see cref="TryUpdateYuv(in Rect{int}, ReadOnlyNativeMemory{byte}, int, ReadOnlyNativeMemory{byte}, int, ReadOnlyNativeMemory{byte}, int)"/>, respectively, instead.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryUpdate(in Rect<int> rect, ReadOnlyNativeMemory pixels, int pitch)
	{
		unsafe
		{
			if (!pixels.IsValid)
			{
				return false;
			}

			var required = unchecked((rect.Height is > 0 ? (nuint)rect.Height : 0) * (pitch is > 0 ? (nuint)pitch : 0));

			if (pixels.Length < required)
			{
				return false;
			}

			fixed (Rect<int>* rectPtr = &rect)
			{
				return SDL_UpdateTexture(mTexture, rectPtr, pixels.RawPointer, pitch);
			}
		}
	}

	/// <summary>
	/// Tries to update an area of the texture with new pixel data
	/// </summary>
	/// <param name="rect">The area of the texture to update with the new pixel data</param>
	/// <param name="pixels">The new pixel data to be copied into the specified area of the texture, in the <see cref="Format">pixel format</see> of the texture</param>
	/// <param name="pitch">
	/// The pitch used in the given <paramref name="pixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="pixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The given <paramref name="pixels"/> data must be in the <see cref="Format">pixel format</see> of the texture.
	/// </para>
	/// <para>
	/// This is a fairly slow operation, intended for use with <see cref="TextureAccess.Static">static</see> textures that do not change often.
	/// </para>
	/// <para>
	/// If the texture is intended to be updated often, you should prefer to create the texture as <see cref="TextureAccess.Streaming">streaming</see> and use one of the locking mechanisms to manipulate the texture's pixels (e.g. <see cref="TryLock(in Rect{int}, out TexturePixelMemoryManager)"/>) instead.
	/// While this method still works with <see cref="TextureAccess.Streaming">streaming</see> textures, for optimization reasons you may not get the pixels back if you lock the texture afterward.
	/// </para>
	/// <para>
	/// Using this method for NV12, NV21, YV12 or IYUV textures is perfectly fine as long as the given <paramref name="pixels"/> data is a contiguous block of NV12/N21 planes or Y and UV planes, respectively, in the proper order.
	/// If not, you should use <see cref="TryUpdateNv(in Rect{int}, ReadOnlySpan{byte}, int, ReadOnlySpan{byte}, int)"/> or <see cref="TryUpdateYuv(in Rect{int}, ReadOnlySpan{byte}, int, ReadOnlySpan{byte}, int, ReadOnlySpan{byte}, int)"/>, respectively, instead.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryUpdate(in Rect<int> rect, ReadOnlySpan<byte> pixels, int pitch)
	{
		unsafe
		{
			var required = unchecked((rect.Height is > 0 ? (nuint)rect.Height : 0) * (pitch is > 0 ? (nuint)pitch : 0));

			if (unchecked((nuint)pixels.Length) < required)
			{
				return false;
			}

			fixed (byte* pixelsPtr = pixels)
			fixed (Rect<int>* rectPtr = &rect)
			{
				return SDL_UpdateTexture(mTexture, rectPtr, pixelsPtr, pitch);
			}
		}
	}

	/// <summary>
	/// Tries to update an area of the texture with new pixel data
	/// </summary>
	/// <param name="rect">The area of the texture to update with the new pixel data</param>
	/// <param name="pixels">The new pixel data to be copied into the specified area of the texture, in the <see cref="Format">pixel format</see> of the texture</param>
	/// <param name="pitch">
	/// The pitch used in the given <paramref name="pixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="pixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The given <paramref name="pixels"/> data must be in the <see cref="Format">pixel format</see> of the texture.
	/// </para>
	/// <para>
	/// This is a fairly slow operation, intended for use with <see cref="TextureAccess.Static">static</see> textures that do not change often.
	/// </para>
	/// <para>
	/// If the texture is intended to be updated often, you should prefer to create the texture as <see cref="TextureAccess.Streaming">streaming</see> and use one of the locking mechanisms to manipulate the texture's pixels (e.g. <see cref="TryLock(in Rect{int}, out TexturePixelMemoryManager)"/>) instead.
	/// While this method still works with <see cref="TextureAccess.Streaming">streaming</see> textures, for optimization reasons you may not get the pixels back if you lock the texture afterward.
	/// </para>
	/// <para>
	/// Using this method for NV12, NV21, YV12 or IYUV textures is perfectly fine as long as the given <paramref name="pixels"/> data is a contiguous block of NV12/N21 planes or Y and UV planes, respectively, in the proper order.
	/// If not, you should use <see cref="TryUpdateYuv(in Rect{int}, byte*, int, byte*, int, byte*, int)"/> or <see cref="TryUpdateYuv(in Rect{int}, byte*, int, byte*, int, byte*, int)"/>, respectively, instead.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public unsafe bool TryUpdate(in Rect<int> rect, void* pixels, int pitch)
	{
		fixed (Rect<int>* rectPtr = &rect)
		{
			return SDL_UpdateTexture(mTexture, rectPtr, pixels, pitch);
		}
	}

	/// <summary>
	/// Tries to update the entire texture with new pixel data
	/// </summary>
	/// <param name="pixels">The new pixel data to be copied into the entirety of the texture, in the <see cref="Format">pixel format</see> of the texture</param>
	/// <param name="pitch">
	/// The pitch used in the given <paramref name="pixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="pixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The given <paramref name="pixels"/> data must be in the <see cref="Format">pixel format</see> of the texture.
	/// </para>
	/// <para>
	/// This is a fairly slow operation, intended for use with <see cref="TextureAccess.Static">static</see> textures that do not change often.
	/// </para>
	/// <para>
	/// If the texture is intended to be updated often, you should prefer to create the texture as <see cref="TextureAccess.Streaming">streaming</see> and use one of the locking mechanisms to manipulate the texture's pixels (e.g. <see cref="TryLock(out TexturePixelMemoryManager)"/>) instead.
	/// While this method still works with <see cref="TextureAccess.Streaming">streaming</see> textures, for optimization reasons you may not get the pixels back if you lock the texture afterward.
	/// </para>
	/// <para>
	/// Using this method for NV12, NV21, YV12 or IYUV textures is perfectly fine as long as the given <paramref name="pixels"/> data is a contiguous block of NV12/N21 planes or Y and UV planes, respectively, in the proper order.
	/// If not, you should use <see cref="TryUpdateNv(ReadOnlyNativeMemory{byte}, int, ReadOnlyNativeMemory{byte}, int)"/> or <see cref="TryUpdateYuv(ReadOnlyNativeMemory{byte}, int, ReadOnlyNativeMemory{byte}, int, ReadOnlyNativeMemory{byte}, int)"/>, respectively, instead.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryUpdate(ReadOnlyNativeMemory pixels, int pitch)
	{
		unsafe
		{
			if (!pixels.IsValid)
			{
				return false;
			}

			var height = Height;

			var required = unchecked((height is > 0 ? (nuint)height : 0) * (pitch is > 0 ? (nuint)pitch : 0));

			if (pixels.Length < required)
			{
				return false;
			}

			return SDL_UpdateTexture(mTexture, rect: null, pixels.RawPointer, pitch);
		}
	}

	/// <summary>
	/// Tries to update the entire texture with new pixel data
	/// </summary>
	/// <param name="pixels">The new pixel data to be copied into the entirety of the texture, in the <see cref="Format">pixel format</see> of the texture</param>
	/// <param name="pitch">
	/// The pitch used in the given <paramref name="pixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="pixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The given <paramref name="pixels"/> data must be in the <see cref="Format">pixel format</see> of the texture.
	/// </para>
	/// <para>
	/// This is a fairly slow operation, intended for use with <see cref="TextureAccess.Static">static</see> textures that do not change often.
	/// </para>
	/// <para>
	/// If the texture is intended to be updated often, you should prefer to create the texture as <see cref="TextureAccess.Streaming">streaming</see> and use one of the locking mechanisms to manipulate the texture's pixels (e.g. <see cref="TryLock(out TexturePixelMemoryManager)"/>) instead.
	/// While this method still works with <see cref="TextureAccess.Streaming">streaming</see> textures, for optimization reasons you may not get the pixels back if you lock the texture afterward.
	/// </para>
	/// <para>
	/// Using this method for NV12, NV21, YV12 or IYUV textures is perfectly fine as long as the given <paramref name="pixels"/> data is a contiguous block of NV12/N21 planes or Y and UV planes, respectively, in the proper order.
	/// If not, you should use <see cref="TryUpdateNv(ReadOnlySpan{byte}, int, ReadOnlySpan{byte}, int)"/> or <see cref="TryUpdateYuv(ReadOnlySpan{byte}, int, ReadOnlySpan{byte}, int, ReadOnlySpan{byte}, int)"/>, respectively, instead.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryUpdate(ReadOnlySpan<byte> pixels, int pitch)
	{
		unsafe
		{
			var height = Height;

			var required = unchecked((height is > 0 ? (nuint)height : 0) * (pitch is > 0 ? (nuint)pitch : 0));

			if (unchecked((nuint)pixels.Length) < required)
			{
				return false;
			}

			fixed (byte* pixelsPtr = pixels)
			{
				return SDL_UpdateTexture(mTexture, rect: null, pixelsPtr, pitch);
			}
		}
	}

	/// <summary>
	/// Tries to update the entire texture with new pixel data
	/// </summary>
	/// <param name="pixels">The new pixel data to be copied into the entirety of the texture, in the <see cref="Format">pixel format</see> of the texture</param>
	/// <param name="pitch">
	/// The pitch used in the given <paramref name="pixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="pixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The given <paramref name="pixels"/> data must be in the <see cref="Format">pixel format</see> of the texture.
	/// </para>
	/// <para>
	/// This is a fairly slow operation, intended for use with <see cref="TextureAccess.Static">static</see> textures that do not change often.
	/// </para>
	/// <para>
	/// If the texture is intended to be updated often, you should prefer to create the texture as <see cref="TextureAccess.Streaming">streaming</see> and use one of the locking mechanisms to manipulate the texture's pixels (e.g. <see cref="TryLock(out TexturePixelMemoryManager)"/>) instead.
	/// While this method still works with <see cref="TextureAccess.Streaming">streaming</see> textures, for optimization reasons you may not get the pixels back if you lock the texture afterward.
	/// </para>
	/// <para>
	/// Using this method for NV12, NV21, YV12 or IYUV textures is perfectly fine as long as the given <paramref name="pixels"/> data is a contiguous block of NV12/N21 planes or Y and UV planes, respectively, in the proper order.
	/// If not, you should use <see cref="TryUpdateNv(byte*, int, byte*, int)"/> or <see cref="TryUpdateYuv(byte*, int, byte*, int, byte*, int)"/>, respectively, instead.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public unsafe bool TryUpdate(void* pixels, int pitch)
	{
		return SDL_UpdateTexture(mTexture, rect: null, pixels, pitch);
	}

	/// <summary>
	/// Tries to update an area of a planar NV12 or NV21 texture with new pixel data
	/// </summary>
	/// <param name="rect">The area of the texture to update with the new pixel data</param>
	/// <param name="yPixels">The new pixel data for the Y plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="yPitch">
	/// The pitch used in the given <paramref name="yPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="yPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="uvPixels">The new pixel data for the UV plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="uvPitch">
	/// The pitch used in the given <paramref name="uvPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="uvPixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant as an alternative to <see cref="TryUpdate(in Rect{int}, ReadOnlyNativeMemory, int)"/> for planar NV12 or NV21 textures when your pixel data for the individual planes is not stored as a contiguous block or you need to specify different pitch values for the individual planes.
	/// Please see the documentation of <see cref="TryUpdate(in Rect{int}, ReadOnlyNativeMemory, int)"/> for more general information about update operations, as the details mentioned there also apply to this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryUpdateNv(in Rect<int> rect, ReadOnlyNativeMemory<byte> yPixels, int yPitch, ReadOnlyNativeMemory<byte> uvPixels, int uvPitch)
	{
		unsafe
		{
			if (!yPixels.IsValid || !uvPixels.IsValid)
			{
				return false;
			}

			var yRequired = unchecked((rect.Height is > 0 ? (nuint)rect.Height : 0) * (yPitch is > 0 ? (nuint)yPitch : 0));

			if (yPixels.Length < yRequired)
			{
				return false;
			}

			var uvRequired = unchecked((rect.Height is > 0 ? (nuint)rect.Height : 0) * (uvPitch is > 0 ? (nuint)uvPitch : 0));

			if (uvPixels.Length < uvRequired)
			{
				return false;
			}

			fixed (Rect<int>* rectPtr = &rect)
			{
				return SDL_UpdateNVTexture(mTexture, rectPtr, yPixels.RawPointer, yPitch, uvPixels.RawPointer, uvPitch);
			}
		}
	}

	/// <summary>
	/// Tries to update an area of a planar NV12 or NV21 texture with new pixel data
	/// </summary>
	/// <param name="rect">The area of the texture to update with the new pixel data</param>
	/// <param name="yPixels">The new pixel data for the Y plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="yPitch">
	/// The pitch used in the given <paramref name="yPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="yPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="uvPixels">The new pixel data for the UV plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="uvPitch">
	/// The pitch used in the given <paramref name="uvPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="uvPixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant as an alternative to <see cref="TryUpdate(in Rect{int}, ReadOnlySpan{byte}, int)"/> for planar NV12 or NV21 textures when your pixel data for the individual planes is not stored as a contiguous block or you need to specify different pitch values for the individual planes.
	/// Please see the documentation of <see cref="TryUpdate(in Rect{int}, ReadOnlySpan{byte}, int)"/> for more general information about update operations, as the details mentioned there also apply to this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryUpdateNv(in Rect<int> rect, ReadOnlySpan<byte> yPixels, int yPitch, ReadOnlySpan<byte> uvPixels, int uvPitch)
	{
		unsafe
		{
			var yRequired = unchecked((rect.Height is > 0 ? (nuint)rect.Height : 0) * (yPitch is > 0 ? (nuint)yPitch : 0));

			if (unchecked((nuint)yPixels.Length) < yRequired)
			{
				return false;
			}

			var uvRequired = unchecked((rect.Height is > 0 ? (nuint)rect.Height : 0) * (uvPitch is > 0 ? (nuint)uvPitch : 0));

			if (unchecked((nuint)uvPixels.Length) < uvRequired)
			{
				return false;
			}

			fixed (Rect<int>* rectPtr = &rect)
			fixed (byte* yPixelsPtr = yPixels, uvPixelsPtr = uvPixels)
			{
				return SDL_UpdateNVTexture(mTexture, rectPtr, yPixelsPtr, yPitch, uvPixelsPtr, uvPitch);
			}
		}
	}

	/// <summary>
	/// Tries to update an area of a planar NV12 or NV21 texture with new pixel data
	/// </summary>
	/// <param name="rect">The area of the texture to update with the new pixel data</param>
	/// <param name="yPixels">The new pixel data for the Y plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="yPitch">
	/// The pitch used in the given <paramref name="yPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="yPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="uvPixels">The new pixel data for the UV plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="uvPitch">
	/// The pitch used in the given <paramref name="uvPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="uvPixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant as an alternative to <see cref="TryUpdate(in Rect{int}, void*, int)"/> for planar NV12 or NV21 textures when your pixel data for the individual planes is not stored as a contiguous block or you need to specify different pitch values for the individual planes.
	/// Please see the documentation of <see cref="TryUpdate(in Rect{int}, void*, int)"/> for more general information about update operations, as the details mentioned there also apply to this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public unsafe bool TryUpdateNv(in Rect<int> rect, byte* yPixels, int yPitch, byte* uvPixels, int uvPitch)
	{
		fixed (Rect<int>* rectPtr = &rect)
		{
			return SDL_UpdateNVTexture(mTexture, rectPtr, yPixels, yPitch, uvPixels, uvPitch);
		}
	}

	/// <summary>
	/// Tries to update an entire planar NV12 or NV21 texture with new pixel data
	/// </summary>
	/// <param name="yPixels">The new pixel data for the Y plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="yPitch">
	/// The pitch used in the given <paramref name="yPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="yPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="uvPixels">The new pixel data for the UV plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="uvPitch">
	/// The pitch used in the given <paramref name="uvPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="uvPixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant as an alternative to <see cref="TryUpdate(ReadOnlyNativeMemory, int)"/> for planar NV12 or NV21 textures when your pixel data for the individual planes is not stored as a contiguous block or you need to specify different pitch values for the individual planes.
	/// Please see the documentation of <see cref="TryUpdate(ReadOnlyNativeMemory, int)"/> for more general information about update operations, as the details mentioned there also apply to this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryUpdateNv(ReadOnlyNativeMemory<byte> yPixels, int yPitch, ReadOnlyNativeMemory<byte> uvPixels, int uvPitch)
	{
		unsafe
		{
			if (!yPixels.IsValid || !uvPixels.IsValid)
			{
				return false;
			}

			var height = Height;

			var yRequired = unchecked((height is > 0 ? (nuint)height : 0) * (yPitch is > 0 ? (nuint)yPitch : 0));

			if (yPixels.Length < yRequired)
			{
				return false;
			}

			var uvRequired = unchecked((height is > 0 ? (nuint)height : 0) * (uvPitch is > 0 ? (nuint)uvPitch : 0));

			if (uvPixels.Length < uvRequired)
			{
				return false;
			}

			return SDL_UpdateNVTexture(mTexture, rect: null, yPixels.RawPointer, yPitch, uvPixels.RawPointer, uvPitch);
		}
	}

	/// <summary>
	/// Tries to update an entire planar NV12 or NV21 texture with new pixel data
	/// </summary>
	/// <param name="yPixels">The new pixel data for the Y plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="yPitch">
	/// The pitch used in the given <paramref name="yPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="yPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="uvPixels">The new pixel data for the UV plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="uvPitch">
	/// The pitch used in the given <paramref name="uvPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="uvPixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant as an alternative to <see cref="TryUpdate(ReadOnlySpan{byte}, int)"/> for planar NV12 or NV21 textures when your pixel data for the individual planes is not stored as a contiguous block or you need to specify different pitch values for the individual planes.
	/// Please see the documentation of <see cref="TryUpdate(ReadOnlySpan{byte}, int)"/> for more general information about update operations, as the details mentioned there also apply to this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryUpdateNv(ReadOnlySpan<byte> yPixels, int yPitch, ReadOnlySpan<byte> uvPixels, int uvPitch)
	{
		unsafe
		{
			var height = Height;

			var yRequired = unchecked((height is > 0 ? (nuint)height : 0) * (yPitch is > 0 ? (nuint)yPitch : 0));

			if (unchecked((nuint)yPixels.Length) < yRequired)
			{
				return false;
			}

			var uvRequired = unchecked((height is > 0 ? (nuint)height : 0) * (uvPitch is > 0 ? (nuint)uvPitch : 0));

			if (unchecked((nuint)uvPixels.Length) < uvRequired)
			{
				return false;
			}

			fixed (byte* yPixelsPtr = yPixels, uvPixelsPtr = uvPixels)
			{
				return SDL_UpdateNVTexture(mTexture, rect: null, yPixelsPtr, yPitch, uvPixelsPtr, uvPitch);
			}
		}
	}

	/// <summary>
	/// Tries to update an entire planar NV12 or NV21 texture with new pixel data
	/// </summary>
	/// <param name="yPixels">The new pixel data for the Y plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="yPitch">
	/// The pitch used in the given <paramref name="yPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="yPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="uvPixels">The new pixel data for the UV plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="uvPitch">
	/// The pitch used in the given <paramref name="uvPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="uvPixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant as an alternative to <see cref="TryUpdate(void*, int)"/> for planar NV12 or NV21 textures when your pixel data for the individual planes is not stored as a contiguous block or you need to specify different pitch values for the individual planes.
	/// Please see the documentation of <see cref="TryUpdate(void*, int)"/> for more general information about update operations, as the details mentioned there also apply to this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public unsafe bool TryUpdateNv(byte* yPixels, int yPitch, byte* uvPixels, int uvPitch)
	{
		return SDL_UpdateNVTexture(mTexture, rect: null, yPixels, yPitch, uvPixels, uvPitch);
	}

	/// <summary>
	/// Tries to update an area of a planar YV12 or IYUV texture with new pixel data
	/// </summary>
	/// <param name="rect">The area of the texture to update with the new pixel data</param>
	/// <param name="yPixels">The new pixel data for the Y plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="yPitch">
	/// The pitch used in the given <paramref name="yPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="yPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="uPixels">The new pixel data for the U plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="uPitch">
	/// The pitch used in the given <paramref name="uPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="uPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="vPixels">The new pixel data for the V plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="vPitch">
	/// The pitch used in the given <paramref name="vPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="vPixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant as an alternative to <see cref="TryUpdate(in Rect{int}, ReadOnlyNativeMemory, int)"/> for planar YV12 or IYUV textures when your pixel data for the individual planes is not stored as a contiguous block or you need to specify different pitch values for the individual planes.
	/// Please see the documentation of <see cref="TryUpdate(in Rect{int}, ReadOnlyNativeMemory, int)"/> for more general information about update operations, as the details mentioned there also apply to this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryUpdateYuv(in Rect<int> rect, ReadOnlyNativeMemory<byte> yPixels, int yPitch, ReadOnlyNativeMemory<byte> uPixels, int uPitch, ReadOnlyNativeMemory<byte> vPixels, int vPitch)
	{
		unsafe
		{
			if (!yPixels.IsValid || !uPixels.IsValid || !vPixels.IsValid)
			{
				return false;
			}

			var yRequired = unchecked((rect.Height is > 0 ? (nuint)rect.Height : 0) * (yPitch is > 0 ? (nuint)yPitch : 0));

			if (yPixels.Length < yRequired)
			{
				return false;
			}

			var uRequired = unchecked((rect.Height is > 0 ? (nuint)rect.Height : 0) * (uPitch is > 0 ? (nuint)uPitch : 0));

			if (uPixels.Length < uRequired)
			{
				return false;
			}

			var vRequired = unchecked((rect.Height is > 0 ? (nuint)rect.Height : 0) * (vPitch is > 0 ? (nuint)vPitch : 0));

			if (vPixels.Length < vRequired)
			{
				return false;
			}

			fixed (Rect<int>* rectPtr = &rect)
			{
				return SDL_UpdateYUVTexture(mTexture, rectPtr, yPixels.RawPointer, yPitch, uPixels.RawPointer, uPitch, vPixels.RawPointer, vPitch);
			}
		}
	}

	/// <summary>
	/// Tries to update an area of a planar YV12 or IYUV texture with new pixel data
	/// </summary>
	/// <param name="rect">The area of the texture to update with the new pixel data</param>
	/// <param name="yPixels">The new pixel data for the Y plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="yPitch">
	/// The pitch used in the given <paramref name="yPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="yPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="uPixels">The new pixel data for the U plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="uPitch">
	/// The pitch used in the given <paramref name="uPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="uPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="vPixels">The new pixel data for the V plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="vPitch">
	/// The pitch used in the given <paramref name="vPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="vPixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant as an alternative to <see cref="TryUpdate(in Rect{int}, ReadOnlySpan{byte}, int)"/> for planar YV12 or IYUV textures when your pixel data for the individual planes is not stored as a contiguous block or you need to specify different pitch values for the individual planes.
	/// Please see the documentation of <see cref="TryUpdate(in Rect{int}, ReadOnlySpan{byte}, int)"/> for more general information about update operations, as the details mentioned there also apply to this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryUpdateYuv(in Rect<int> rect, ReadOnlySpan<byte> yPixels, int yPitch, ReadOnlySpan<byte> uPixels, int uPitch, ReadOnlySpan<byte> vPixels, int vPitch)
	{
		unsafe
		{
			var yRequired = unchecked((rect.Height is > 0 ? (nuint)rect.Height : 0) * (yPitch is > 0 ? (nuint)yPitch : 0));

			if (unchecked((nuint)yPixels.Length) < yRequired)
			{
				return false;
			}

			var uRequired = unchecked((rect.Height is > 0 ? (nuint)rect.Height : 0) * (uPitch is > 0 ? (nuint)uPitch : 0));

			if (unchecked((nuint)uPixels.Length) < uRequired)
			{
				return false;
			}

			var vRequired = unchecked((rect.Height is > 0 ? (nuint)rect.Height : 0) * (vPitch is > 0 ? (nuint)vPitch : 0));

			if (unchecked((nuint)vPixels.Length) < vRequired)
			{
				return false;
			}

			fixed (Rect<int>* rectPtr = &rect)
			fixed (byte* yPixelsPtr = yPixels, uPixelsPtr = uPixels, vPixelsPtr = vPixels)
			{
				return SDL_UpdateYUVTexture(mTexture, rectPtr, yPixelsPtr, yPitch, uPixelsPtr, uPitch, vPixelsPtr, vPitch);
			}
		}
	}

	/// <summary>
	/// Tries to update an area of a planar YV12 or IYUV texture with new pixel data
	/// </summary>
	/// <param name="rect">The area of the texture to update with the new pixel data</param>
	/// <param name="yPixels">The new pixel data for the Y plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="yPitch">
	/// The pitch used in the given <paramref name="yPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="yPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="uPixels">The new pixel data for the U plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="uPitch">
	/// The pitch used in the given <paramref name="uPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="uPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="vPixels">The new pixel data for the V plane of the texture, to be copied into the specified area of the texture</param>
	/// <param name="vPitch">
	/// The pitch used in the given <paramref name="vPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="vPixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant as an alternative to <see cref="TryUpdate(in Rect{int}, void*, int)"/> for planar YV12 or IYUV textures when your pixel data for the individual planes is not stored as a contiguous block or you need to specify different pitch values for the individual planes.
	/// Please see the documentation of <see cref="TryUpdate(in Rect{int}, void*, int)"/> for more general information about update operations, as the details mentioned there also apply to this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public unsafe bool TryUpdateYuv(in Rect<int> rect, byte* yPixels, int yPitch, byte* uPixels, int uPitch, byte* vPixels, int vPitch)
	{
		fixed (Rect<int>* rectPtr = &rect)
		{
			return SDL_UpdateYUVTexture(mTexture, rectPtr, yPixels, yPitch, uPixels, uPitch, vPixels, vPitch);
		}
	}

	/// <summary>
	/// Tries to update an entire planar YV12 or IYUV texture with new pixel data
	/// </summary>
	/// <param name="yPixels">The new pixel data for the Y plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="yPitch">
	/// The pitch used in the given <paramref name="yPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="yPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="uPixels">The new pixel data for the U plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="uPitch">
	/// The pitch used in the given <paramref name="uPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="uPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="vPixels">The new pixel data for the V plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="vPitch">
	/// The pitch used in the given <paramref name="vPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="vPixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant as an alternative to <see cref="TryUpdate(ReadOnlyNativeMemory, int)"/> for planar YV12 or IYUV textures when your pixel data for the individual planes is not stored as a contiguous block or you need to specify different pitch values for the individual planes.
	/// Please see the documentation of <see cref="TryUpdate(ReadOnlyNativeMemory, int)"/> for more general information about update operations, as the details mentioned there also apply to this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryUpdateYuv(ReadOnlyNativeMemory<byte> yPixels, int yPitch, ReadOnlyNativeMemory<byte> uPixels, int uPitch, ReadOnlyNativeMemory<byte> vPixels, int vPitch)
	{
		unsafe
		{
			if (!yPixels.IsValid || !uPixels.IsValid || !vPixels.IsValid)
			{
				return false;
			}

			var height = Height;

			var yRequired = unchecked((height is > 0 ? (nuint)height : 0) * (yPitch is > 0 ? (nuint)yPitch : 0));

			if (yPixels.Length < yRequired)
			{
				return false;
			}

			var uRequired = unchecked((height is > 0 ? (nuint)height : 0) * (uPitch is > 0 ? (nuint)uPitch : 0));

			if (uPixels.Length < uRequired)
			{
				return false;
			}

			var vRequired = unchecked((height is > 0 ? (nuint)height : 0) * (vPitch is > 0 ? (nuint)vPitch : 0));

			if (vPixels.Length < vRequired)
			{
				return false;
			}

			return SDL_UpdateYUVTexture(mTexture, rect: null, yPixels.RawPointer, yPitch, uPixels.RawPointer, uPitch, vPixels.RawPointer, vPitch);
		}
	}

	/// <summary>
	/// Tries to update an entire planar YV12 or IYUV texture with new pixel data
	/// </summary>
	/// <param name="yPixels">The new pixel data for the Y plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="yPitch">
	/// The pitch used in the given <paramref name="yPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="yPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="uPixels">The new pixel data for the U plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="uPitch">
	/// The pitch used in the given <paramref name="uPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="uPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="vPixels">The new pixel data for the V plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="vPitch">
	/// The pitch used in the given <paramref name="vPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="vPixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant as an alternative to <see cref="TryUpdate(ReadOnlySpan{byte}, int)"/> for planar YV12 or IYUV textures when your pixel data for the individual planes is not stored as a contiguous block or you need to specify different pitch values for the individual planes.
	/// Please see the documentation of <see cref="TryUpdate(ReadOnlySpan{byte}, int)"/> for more general information about update operations, as the details mentioned there also apply to this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryUpdateYuv(ReadOnlySpan<byte> yPixels, int yPitch, ReadOnlySpan<byte> uPixels, int uPitch, ReadOnlySpan<byte> vPixels, int vPitch)
	{
		unsafe
		{
			var height = Height;

			var yRequired = unchecked((height is > 0 ? (nuint)height : 0) * (yPitch is > 0 ? (nuint)yPitch : 0));

			if (unchecked((nuint)yPixels.Length) < yRequired)
			{
				return false;
			}

			var uRequired = unchecked((height is > 0 ? (nuint)height : 0) * (uPitch is > 0 ? (nuint)uPitch : 0));

			if (unchecked((nuint)uPixels.Length) < uRequired)
			{
				return false;
			}

			var vRequired = unchecked((height is > 0 ? (nuint)height : 0) * (vPitch is > 0 ? (nuint)vPitch : 0));

			if (unchecked((nuint)vPixels.Length) < vRequired)
			{
				return false;
			}

			fixed (byte* yPixelsPtr = yPixels, uPixelsPtr = uPixels, vPixelsPtr = vPixels)
			{
				return SDL_UpdateYUVTexture(mTexture, rect: null, yPixelsPtr, yPitch, uPixelsPtr, uPitch, vPixelsPtr, vPitch);
			}
		}
	}

	/// <summary>
	/// Tries to update an entire planar YV12 or IYUV texture with new pixel data
	/// </summary>
	/// <param name="yPixels">The new pixel data for the Y plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="yPitch">
	/// The pitch used in the given <paramref name="yPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="yPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="uPixels">The new pixel data for the U plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="uPitch">
	/// The pitch used in the given <paramref name="uPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="uPixels"/> data, expressed in bytes)
	/// </param>
	/// <param name="vPixels">The new pixel data for the V plane of the texture, to be copied into the entirety of the texture</param>
	/// <param name="vPitch">
	/// The pitch used in the given <paramref name="vPixels"/> data, in bytes;
	/// that is the length between the start of a row of pixels and the start of the next row of pixels (which may be greater than the pixel width of the given <paramref name="vPixels"/> data, expressed in bytes)
	/// </param>
	/// <returns><c><see langword="true"/></c>, if the texture was successfully updated with the new pixel data; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method is meant as an alternative to <see cref="TryUpdate(void*, int)"/> for planar YV12 or IYUV textures when your pixel data for the individual planes is not stored as a contiguous block or you need to specify different pitch values for the individual planes.
	/// Please see the documentation of <see cref="TryUpdate(void*, int)"/> for more general information about update operations, as the details mentioned there also apply to this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public unsafe bool TryUpdateYuv(byte* yPixels, int yPitch, byte* uPixels, int uPitch, byte* vPixels, int vPitch)
	{
		return SDL_UpdateYUVTexture(mTexture, rect: null, yPixels, yPitch, uPixels, uPitch, vPixels, vPitch);
	}

	/// <summary>
	/// Unlocks the texture after write-only pixel access
	/// </summary>
	/// <remarks>
	/// <para>
	/// This method is meant to be used in conjuction with any of the "TryUnsafeLock" or "TryUnsafeLockToSurface" methods (e.g. <see cref="TryUnsafeLock(in Rect{int}, out Utilities.NativeMemory, out int)"/>), if you want to access the texture's pixels directly in a faster and more efficient way.
	/// If you're locking for a simpler and safer way to access the texture's pixels, consider using any of the "TryLock" or "TryLockToSurface" methods (e.g. <see cref="TryLock(in Rect{int}, out TexturePixelMemoryManager?)"/>) instead.
	/// </para>
	/// <para>
	/// <em>Warning</em>: Please note that locking a texture is intended to be write-only; it will not guarantee the previous contents of the texture will be provided.
	/// You <em>must</em> fully initialize any area of a texture that you locked before unlocking it, as the pixels might otherwise be uninitialized memory.
	/// E.g., if you locked a texture and immediately unlocked it again without writing any pixel data, the texture could end up in a corrupted state, depending on the renderer in use.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <example>
	/// Example usage:
	/// <code>
	/// Texture texture;
	/// 
	/// ...
	/// 
	/// if (texture.TryUnsafeLock(out NativeMemory pixels, out var pitch))
	/// {
	///		// Write-only access to the pixel memory using 'pitch'
	///		// Be sure to fully initialize ALL the pixels locked
	///		
	///		...
	///		
	///		texture.UnsafeUnlock();
	/// }
	/// </code>
	/// </example>
	public void UnsafeUnlock()
	{
		unsafe
		{
			SDL_UnlockTexture(mTexture);
		}
	}
}
