using Sdl3Sharp.Events;
using Sdl3Sharp.Internal;
using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.Utilities;
using Sdl3Sharp.Video.Blending;
using Sdl3Sharp.Video.Coloring;
using Sdl3Sharp.Video.Drawing;
using Sdl3Sharp.Video.Rendering.Drivers;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.Video.Rendering;

/// <summary>
/// Represents a rendering context (renderer)
/// </summary>
/// <remarks>
/// <para>
/// This is used to perform driver-specific 2D rendering operations, most commonly to a <see cref="Windowing.Window"/> or an off-screen render target.
/// </para>
/// <para>
/// You can create new renderers for a <see cref="Windowing.Window"/> using the <see cref="Window.TryCreateRenderer(out Renderer?, ReadOnlySpan{string})"/>
/// or <see cref="Window.TryCreateRenderer(out Renderer?, string?, ColorSpace?, RendererVSync?, Properties?)"/>
/// instance methods on a <see cref="Windowing.Window"/> instance.
/// </para>
/// <para>
/// If you create textures using a <see cref="Renderer"/>, please remember to dispose them <em>before</em> disposing the renderer.
/// <em>Do not</em> dispose the associated <see cref="Windowing.Window"/> before disposing the <see cref="Renderer"/> either!
/// Using an <see cref="Renderer"/> after its associated <see cref="Windowing.Window"/> has been disposed can cause undefined behavior, including crashes.
/// </para>
/// <para>
/// For the most part <see cref="Renderer"/>s are not thread-safe, and most of their properties and methods should only be accessed from the main thread!
/// </para>
/// <para>
/// <see cref="Renderer"/>s are not driver-agnostic! Most of the time instance of this abstract class are of the concrete <see cref="Renderer{TDriver}"/> type with a specific <see cref="IRenderingDriver">rendering driver</see> as the type argument.
/// However, the <see cref="Renderer"/> type exists as an abstraction to use common rendering operations.
/// </para>
/// <para>
/// To specify a concrete renderer type, use <see cref="Renderer{TDriver}"/> with a rendering driver that implements the <see cref="IRenderingDriver"/> interface (e.g. <see cref="Renderer{TDriver}">Renderer&lt;<see cref="OpenGL">OpenGL</see>&gt;</see>).
/// </para>
/// </remarks>
public abstract partial class Renderer : IDisposable
{
	private static readonly ConcurrentDictionary<IntPtr, WeakReference<Renderer>> mKnownInstances = [];

	private unsafe SDL_Renderer* mRenderer;

	private protected unsafe Renderer(SDL_Renderer* renderer, bool register)
	{
		mRenderer = renderer;

		if (register)
		{
			if (renderer is not null)
			{
				// Conversly to textures, renderers are not ref-counted, so there's no ref counter manipulating shenanigans here

				mKnownInstances.AddOrUpdate(unchecked((IntPtr)renderer), addRef, updateRef, this);
			}

			static WeakReference<Renderer> addRef(IntPtr renderer, Renderer newRenderer) => new(newRenderer);

			static WeakReference<Renderer> updateRef(IntPtr renderer, WeakReference<Renderer> previousRendererRef, Renderer newRenderer)
			{
				if (previousRendererRef.TryGetTarget(out var previousRenderer))
				{
#pragma warning disable IDE0079
#pragma warning disable CA1816
					GC.SuppressFinalize(previousRenderer);
#pragma warning restore CA1816
#pragma warning restore IDE0079

					// Conversly to textures, renderers are not ref-counted, so there's no ref counter manipulating shenanigans here
					previousRenderer.Dispose(disposing: true, forget: false);
				}

				previousRendererRef.SetTarget(newRenderer);

				return previousRendererRef;
			}
		}
	}

	/// <inheritdoc/>
	~Renderer() => Dispose(disposing: false, forget: true);

	/// <summary>
	/// Gets or sets the clipping rectangle for the current target
	/// </summary>
	/// <value>
	/// The clipping rectangle for the current target
	/// </value>
	/// <remarks>
	/// <para>
	/// If the value of this property is equal to the target's bounds, clipping is effectively disabled.
	/// </para>
	/// <para>
	/// Alternatively, you can use the <see cref="IsClippingEnabled"/> property to check whether clipping is enabled or not,
	/// and the <see cref="ResetClippingRect"/> method to reset the clipping rectangle.
	/// </para>
	/// <para>
	/// Each render target, <see cref="Window"/> or <see cref="Target"/> (or <see cref="RendererExtensions.get_Surface(Renderer{Drivers.Software})"/> in the case of a software renderer), has its own clipping rectangle.
	/// This property reflects the clipping rectangle of the <em>current</em> target.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public Rect<int> ClippingRect
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out Rect<int> rect);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderClipRect(mRenderer, &rect), filterError: GetInvalidRendererErrorMessage());

				return rect;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetRenderClipRect(mRenderer, &value), filterError: GetInvalidRendererErrorMessage());
			}
		}
	}

	/// <summary>
	/// Gets or sets the color scale for render operations
	/// </summary>
	/// <value>
	/// The color scale for render operations
	/// </value>
	/// <remarks>
	/// <para>
	/// The color scale is an additional scale multiplied into the pixel color value while rendering.
	/// This can be used to adjust the brightness of colors during HDR rendering, or changing HDR video brightness when playing on an SDR display.
	/// </para>
	/// <para>
	/// The color scale does not affect the alpha channel, only the color brightness.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public float ColorScale
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out float scale);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderColorScale(mRenderer, &scale), filterError: GetInvalidRendererErrorMessage());

				return scale;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetRenderColorScale(mRenderer, value), filterError: GetInvalidRendererErrorMessage());
			}
		}
	}

	/// <summary>
	/// Gets the <em>current</em> output size for the renderer
	/// </summary>
	/// <value>
	/// The <em>current</em> output size for the renderer, in pixels
	/// </value>
	/// <remarks>
	/// <para>
	/// If a rendering target is active, the value of this property will be the size of that target, otherwise it will be equal to the value of the <see cref="OutputSize"/> property.
	/// </para>
	/// <para>
	/// Either way, the resulting size will be adjusted by the current logical presentation state, dictated by the <see cref="LogicalPresentation"/> property.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public (int Width, int Height) CurrentOutputSize
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (int Width, int Height) result);

				SdlErrorHelper.ThrowIfFailed(SDL_GetCurrentRenderOutputSize(mRenderer, &result.Width, &result.Height), filterError: GetInvalidRendererErrorMessage());

				return result;
			}
		}
	}

#if SDL3_4_0_OR_GREATER

	/// <summary>
	/// Gets or sets the default scale mode for new textures created by the renderer
	/// </summary>
	/// <value>
	/// The default scale mode for new textures created by the renderer
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property defaults to <see cref="ScaleMode.Linear"/> for newly created renderers.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public ScaleMode DefaultTextureScaleMode
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out ScaleMode scaleMode);

				SdlErrorHelper.ThrowIfFailed(SDL_GetDefaultTextureScaleMode(mRenderer, &scaleMode), filterError: GetInvalidRendererErrorMessage());

				return scaleMode;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetDefaultTextureScaleMode(mRenderer, value), filterError: GetInvalidRendererErrorMessage());
			}
		}
	}

#endif

	/// <summary>
	/// Gets or sets the blend mode for drawing operations
	/// </summary>
	/// <remarks>
	/// <para>
	/// Drawing operations that make use of the blend mode defined by this property include, but are not limited to:
	/// <see cref="TryRenderDebugText(float, float, string)"/>, <see cref="TryRenderDebugText(float, float, string, ReadOnlySpan{object})"/>,
	/// <see cref="TryRenderFilledRect(in Rect{float})"/>, <see cref="TryRenderFilledRects(ReadOnlySpan{Rect{float}})"/>,
	/// <see cref="TryRenderLine(float, float, float, float)"/>, <see cref="TryRenderLines(ReadOnlySpan{Point{float}})"/>,
	/// <see cref="TryRenderPoint(float, float)"/>, <see cref="TryRenderPoints(ReadOnlySpan{Point{float}})"/>,
	/// <see cref="TryRenderRect()"/>, <see cref="TryRenderRect(in Rect{float})"/>, and <see cref="TryRenderRects(ReadOnlySpan{Rect{float}})"/>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting this property, the specified blend is <see cref="BlendMode.Invalid"/> or not supported by the renderer (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// - OR -
	/// When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public BlendMode DrawBlendMode
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out BlendMode blendMode);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderDrawBlendMode(mRenderer, &blendMode), filterError: GetInvalidRendererErrorMessage());

				return blendMode;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetRenderDrawBlendMode(mRenderer, value), filterError: GetInvalidRendererErrorMessage());
				// throws if value is BlendMode.Invalid or value is an unsupported blend mode for the renderer
				// Although the offical SDL docs say that "If the blend mode is not supported, the closest supported mode is chosen.",
				// that doesn't appear to be the case looking at the SDL source code.
				// It just fails early if the blend mode is not supported and doesn't try to choose a "closest supported mode".
			}
		}
	}

	/// <summary>
	/// Gets or sets the color used for drawing operations
	/// </summary>
	/// <value>
	/// The color used for drawing operations
	/// </value>
	/// <remarks>
	/// <para>
	/// Drawing operations that make use of the blend mode defined by this property include, but are not limited to:
	/// <see cref="TryRenderDebugText(float, float, string)"/>, <see cref="TryRenderDebugText(float, float, string, ReadOnlySpan{object})"/>,
	/// <see cref="TryRenderFilledRect(in Rect{float})"/>, <see cref="TryRenderFilledRects(ReadOnlySpan{Rect{float}})"/>,
	/// <see cref="TryRenderLine(float, float, float, float)"/>, <see cref="TryRenderLines(ReadOnlySpan{Point{float}})"/>,
	/// <see cref="TryRenderPoint(float, float)"/>, <see cref="TryRenderPoints(ReadOnlySpan{Point{float}})"/>,
	/// <see cref="TryRenderRect()"/>, <see cref="TryRenderRect(in Rect{float})"/>, and <see cref="TryRenderRects(ReadOnlySpan{Rect{float}})"/>.
	/// </para>
	/// <para>
	/// In addition to that, <see cref="DrawColor"/> also defines the color used for clearing the current render target when using the <see cref="TryClear"/> method.
	/// </para>
	/// <para>
	/// The <see cref="DrawBlendMode"/> specifies how the <see cref="Color{T}.A">alpha</see> component of this property is used in drawing operations.
	/// </para>
	/// <para>
	/// The component values of this property are equivalent to the component values of the <see cref="DrawColorFloat"/> property, multiplied by <c>255</c> and rounded towards zero.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public Color<byte> DrawColor
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (byte R, byte G, byte B, byte A) color);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderDrawColor(mRenderer, &color.R, &color.G, &color.B, &color.A), filterError: GetInvalidRendererErrorMessage());

				return Unsafe.BitCast<(byte R, byte G, byte B, byte A), Color<byte>>(color);
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetRenderDrawColor(mRenderer, value.R, value.G, value.B, value.A), filterError: GetInvalidRendererErrorMessage());
			}
		}
	}

	/// <summary>
	/// Gets or sets the color used for drawing operations
	/// </summary>
	/// <value>
	/// The color used for drawing operations
	/// </value>
	/// <remarks>
	/// <para>
	/// Drawing operations that make use of the blend mode defined by this property include, but are not limited to:
	/// <see cref="TryRenderDebugText(float, float, string)"/>, <see cref="TryRenderDebugText(float, float, string, ReadOnlySpan{object})"/>,
	/// <see cref="TryRenderFilledRect(in Rect{float})"/>, <see cref="TryRenderFilledRects(ReadOnlySpan{Rect{float}})"/>,
	/// <see cref="TryRenderLine(float, float, float, float)"/>, <see cref="TryRenderLines(ReadOnlySpan{Point{float}})"/>,
	/// <see cref="TryRenderPoint(float, float)"/>, <see cref="TryRenderPoints(ReadOnlySpan{Point{float}})"/>,
	/// <see cref="TryRenderRect()"/>, <see cref="TryRenderRect(in Rect{float})"/>, and <see cref="TryRenderRects(ReadOnlySpan{Rect{float}})"/>.
	/// </para>
	/// <para>
	/// In addition to that, <see cref="DrawColorFloat"/> also defines the color used for clearing the current render target when using the <see cref="TryClear"/> method.
	/// </para>
	/// <para>
	/// The <see cref="DrawBlendMode"/> specifies how the <see cref="Color{T}.A">alpha</see> component of this property is used in drawing operations.
	/// </para>
	/// <para>
	/// The component values of this property are equivalent to the component values of the <see cref="DrawColor"/> property, divided by <c>255</c>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public Color<float> DrawColorFloat
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (float R, float G, float B, float A) color);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderDrawColorFloat(mRenderer, &color.R, &color.G, &color.B, &color.A), filterError: GetInvalidRendererErrorMessage());

				return Unsafe.BitCast<(float R, float G, float B, float A), Color<float>>(color);
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetRenderDrawColorFloat(mRenderer, value.R, value.G, value.B, value.A), filterError: GetInvalidRendererErrorMessage());
			}
		}
	}

	/// <summary>
	/// Gets the additional high dynamic range that can be displayed, in terms of the <see cref="SdrWhitePoint">SDR white point</see>
	/// </summary>
	/// <value>
	/// The additional high dynamic range that can be displayed, in terms of the <see cref="SdrWhitePoint">SDR white point</see>
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property will be <c>1.0</c> when HDR is not enabled.
	/// </para>
	/// <para>
	/// The value of this property can change dynamically at runtime when a <see cref="EventType.WindowHdrStateChanged"/> event (<see cref="WindowEvent"/>) is sent.
	/// </para>
	/// </remarks>
	public float HdrHeadroom => Properties?.TryGetFloatValue(PropertyNames.HdrHeadroomFloat, out var hdrHeadroom) is true
		? hdrHeadroom
		: default;

	/// <summary>
	/// Gets a value indicating whether clipping on the current target is enabled or not
	/// </summary>
	/// <value>
	/// A value indicating whether clipping on the current target is enabled or not
	/// </value>
	/// <remarks>
	/// <para>
	/// Each render target, <see cref="Window"/> or <see cref="Target"/> (or <see cref="RendererExtensions.get_Surface(Renderer{Drivers.Software})"/> in the case of a software renderer), has its own clipping state.
	/// This property reflects the clipping state of the <em>current</em> target.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public bool IsClippingEnabled
	{
		get
		{
			unsafe
			{
				return SDL_RenderClipEnabled(mRenderer);
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether the renderer is presenting to a display with HDR enabled
	/// </summary>
	/// <value>
	/// A value indicating whether the renderer is presenting to a display with HDR enabled
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property can change dynamically at runtime when a <see cref="EventType.WindowHdrStateChanged"/> event (<see cref="WindowEvent"/>) is sent.
	/// </para>
	/// </remarks>
	public bool IsHdrEnabled => Properties?.TryGetBooleanValue(PropertyNames.HdrEnabledBoolean, out var isHdrEnabled) is true
		&& isHdrEnabled;

	/// <summary>
	/// Gets a value indicating whether an explicit rectangle was set as the current <see cref="Viewport"/> for the renderer
	/// </summary>
	/// <value>
	/// A value indicating whether an explicit rectangle was set as the current <see cref="Viewport"/> for the renderer
	/// </value>
	/// <remarks>
	/// <para>
	/// This is useful if you're saving and restoring the <see cref="Viewport"/> and want to know whether you should restore a specific rectangle or not.
	/// </para>
	/// <para>
	/// Each render target, <see cref="Window"/> or <see cref="Target"/> (or <see cref="RendererExtensions.get_Surface(Renderer{Drivers.Software})"/> in the case of a software renderer), has its own viewport.
	/// This property reflects the viewport of the <em>current</em> target.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public bool IsViewportSet
	{
		get
		{
			unsafe
			{
				return SDL_RenderViewportSet(mRenderer);
			}
		}
	}

	/// <summary>
	/// Gets or sets the device-independent resolution and presentation mode for rendering
	/// </summary>
	/// <value>
	/// The device-independent resolution and presentation mode for rendering
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property specifies the width and height of the logical rendering output. The renderer will act as if the current target is always the specified size, scaling to the actual resolution as necessary.
	/// </para>
	/// <para>
	/// This can be useful for games and applications that expect a fixed size, but would like to scale the output to whatever is available, regardless of how a user resizes a window, or if the display is high DPI.
	/// </para>
	/// <para>
	/// Logical presentation is disabled if the mode is set to <see cref="RendererLogicalPresentation.Disabled"/>, in which case the logical presentation size will be equal to the output size of the current target.
	/// It is safe to toggle the logical presentation mode during the rendering of a frame. E.g. you can do most of the rendering to a specified logical resolution, but to make text look sharper, you can temporarily disable logical presentation when rendering text.
	/// </para>
	/// <para>
	/// It might be useful to draw to a texture that matches the window dimensions with logical presentation enabled, and then draw that texture across the entire window with logical presentation disabled.
	/// Be careful not to render both with logical presentation enabled, however, as this could produce double-letterboxing, etc.
	/// </para>
	/// <para>
	/// Coordinates coming from an event can be converted to rendering coordinates using the <see cref="EventExtensions.TryConvertToRenderCoordinates{TEvent, TRenderer}(ref TEvent, TRenderer)"/> method.
	/// </para>
	/// <para>
	/// Each render target, <see cref="Window"/> or <see cref="Target"/> (or <see cref="RendererExtensions.get_Surface(Renderer{Drivers.Software})"/> in the case of a software renderer), has its own logical presentation size and mode.
	/// This property reflects the logical presentation size and mode of the <em>current</em> target.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public (int Width, int Height, RendererLogicalPresentation Mode) LogicalPresentation
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (int Width, int Height, RendererLogicalPresentation Mode) result);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderLogicalPresentation(mRenderer, &result.Width, &result.Height, &result.Mode), filterError: GetInvalidRendererErrorMessage());

				return result;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetRenderLogicalPresentation(mRenderer, value.Width, value.Height, value.Mode), filterError: GetInvalidRendererErrorMessage());
			}
		}
	}

	/// <summary>
	/// Gets the final presentation rectangle for rendering
	/// </summary>
	/// <value>
	/// The final presentation rectangle for rendering, in pixels
	/// </value>
	/// <remarks>
	/// <para>
	/// Each render target, <see cref="Window"/> or <see cref="Target"/> (or <see cref="RendererExtensions.get_Surface(Renderer{Drivers.Software})"/> in the case of a software renderer), has its own logical presentation size and mode.
	/// This property reflects the logical presentation rectangle of the <em>current</em> target.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public Rect<float> LogicalPresentationRect
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out Rect<float> rect);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderLogicalPresentationRect(mRenderer, &rect), filterError: GetInvalidRendererErrorMessage());

				return rect;
			}
		}
	}

	/// <summary>
	/// Gets the maximum texture size supported by the renderer
	/// </summary>
	/// <value>
	/// The maximum texture size supported by the renderer, in pixels
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property is in regards to both width and height of the texture.
	/// E.g. if the value is <c>4096</c>, then the renderer supports textures up to <c>4096</c>⨯<c>4096</c> in size.
	/// </para>
	/// </remarks>
	public int MaximumTextureSize
	{
		get => Properties?.TryGetNumberValue(PropertyNames.MaxTextureSizeNumber, out var size) is true
			? unchecked((int)size)
			: default;
	}

	/// <summary>
	/// Gets the name of the rendering driver used by the renderer
	/// </summary>
	/// <value>
	/// The name of the rendering driver used by the renderer, or <c><see langword="null"/></c> if the name is not available
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property can be compared to the <see cref="IRenderingDriver.Name">name</see> of any pre-defined rendering driver implementing the <see cref="IRenderingDriver"/> interface to determine whether the renderer is using that driver.
	/// </para>
	/// <para>
	/// Names of rendering drivers should all be simple, low-ASCII identifiers, like <c>"opengl"</c>, <c>"direct3d12"</c> or <c>"metal"</c>.
	/// These should never have Unicode characters, and are not meant to be proper names.
	/// </para>
	/// <para>
	/// You can see <see cref="IRenderingDriver.AvailableDriverNames"/> for a list of the names of all available rendering drivers in the current environment.
	/// </para>
	/// </remarks>
	public string? Name
	{
		get
		{
			unsafe
			{
				return Utf8StringMarshaller.ConvertToManaged(SDL_GetRendererName(mRenderer));
			}
		}
	}

	/// <summary>
	/// Gets the color space used by the renderer for presenting to the output display
	/// </summary>
	/// <value>
	/// The color space used by the renderer for presenting to the output display
	/// </value>
	public ColorSpace OutputColorSpace => Properties?.TryGetNumberValue(PropertyNames.OutputColorSpaceNumber, out var colorSpace) is true
		? unchecked((ColorSpace)colorSpace)
		: default;

	/// <summary>
	/// Gets the output size for the renderer
	/// </summary>
	/// <value>
	/// The output size for the renderer, in pixels
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property is the true output size in pixels, ignoring any render targets or logical presentation settings.
	/// </para>
	/// <para>
	/// To get the output size of the current target, adjusted by the current logical presentation settings, use the <see cref="CurrentOutputSize"/> property instead.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public (int Width, int Height) OutputSize
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (int Width, int Height) result);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderOutputSize(mRenderer, &result.Width, &result.Height), filterError: GetInvalidRendererErrorMessage());

				return result;
			}
		}
	}

	/// <summary>
	/// Gets the properties associated with the renderer
	/// </summary>
	/// <value>
	/// The properties associated with the renderer, or <c><see langword="null"/></c> if the properties could not be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	public Properties? Properties
	{
		get
		{
			unsafe
			{
				return SDL_GetRendererProperties(mRenderer) switch
				{
					0 => null,
					var id => Properties.GetOrCreate(id, registerWithSdl: false)
				};
			}
		}
	}

	internal unsafe SDL_Renderer* Pointer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mRenderer; }

	/// <summary>
	/// Gets the safe area for rendering within the current viewport
	/// </summary>
	/// <value>
	/// The safe area for rendering within the current viewport, in pixels
	/// </value>
	/// <remarks>
	/// <para>
	/// Some devices have portions of the screen which are partially obscured or not interactive, possibly due to on-screen controls, curved edges, camera notches, TV overscan, etc.
	/// This property provides the area of the current viewport which is safe to have interactible content.
	/// You should continue rendering into the rest of the render target, but it should not contain visually important or interactible content.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public Rect<int> SafeArea
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out Rect<int> rect);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderSafeArea(mRenderer, &rect), filterError: GetInvalidRendererErrorMessage());

				return rect;
			}
		}
	}

	/// <summary>
	/// Gets or sets the drawing scales for rendering on the current target
	/// </summary>
	/// <value>
	/// The drawing scales for rendering on the current target
	/// </value>
	/// <remarks>
	/// <para>
	/// The drawing coordinates are scaled by the values of this property before they are used by the renderer.
	/// This allows for resolution-independent drawing with a single coordinate system.
	/// </para>
	/// <para>
	/// If this results in scaling or subpixel drawing by the rendering backend, it will be handled using the appropriate quality hints.
	/// For best results use integer scaling factors.
	/// </para>
	/// <para>
	/// Each render target, <see cref="Window"/> or <see cref="Target"/> (or <see cref="RendererExtensions.get_Surface(Renderer{Drivers.Software})"/> in the case of a software renderer), has its own drawing scales.
	/// This property reflects the drawing scales of the <em>current</em> target.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public (float ScaleX, float ScaleY) Scale
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (float ScaleX, float ScaleY) scale);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderScale(mRenderer, &scale.ScaleX, &scale.ScaleY), filterError: GetInvalidRendererErrorMessage());

				return scale;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetRenderScale(mRenderer, value.ScaleX, value.ScaleY), filterError: GetInvalidRendererErrorMessage());
			}
		}
	}

	/// <summary>
	/// Gets the SDR white point in the <see cref="ColorSpace.SrgbLinear"/> color space
	/// </summary>
	/// <value>
	/// The SDR white point in the <see cref="ColorSpace.SrgbLinear"/> color space
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property is automatically multiplied into the color scale when HDR is enabled.
	/// </para>
	/// <para>
	/// The value of this property can change dynamically at runtime when a <see cref="EventType.WindowHdrStateChanged"/> event (<see cref="WindowEvent"/>) is sent.
	/// </para>
	/// </remarks>
	public float SdrWhitePoint => Properties?.TryGetFloatValue(PropertyNames.SdrWhitePointFloat, out var sdrWhitePoint) is true
		? sdrWhitePoint
		: default;

	/// <summary>
	/// Gets the texture formats supported by the renderer
	/// </summary>
	/// <value>
	/// The texture formats supported by the renderer
	/// </value>
	/// <remarks>
	/// <para>
	/// For performance reasons, the resulting collection is enumerated lazily, so the actual enumeration of the supported texture formats is deferred until you start enumerating over the collection.
	/// </para>
	/// <para>
	/// Because of that, the fact that you can enumerate the result just a single time, and the fact that the enumeration itself is quite an expensive operation,
	/// you should consider caching the result into a persistent collection.
	/// </para>
	/// <para>
	/// Note that the enumerator returned by this property can <see langword="throw"/> an <see cref="InvalidOperationException"/> if the reference to the texture formats is invalid or if it changes during the enumeration.
	/// </para>
	/// </remarks>
	public IEnumerable<PixelFormat> SupportedTextureFormats
	{
		get
		{
			if (Properties?.TryGetPointerValue(PropertyNames.TextureFormatsPointer, out var textureFormatsPtr) is true)
			{
				unsafe
				{
					if (unchecked((PixelFormat*)textureFormatsPtr) is null)
					{
						failTextureFormatsNull();
					}
				}

				var startPtr = textureFormatsPtr;
				var currentPtr = startPtr;

				while (true)
				{
					Unsafe.SkipInit(out PixelFormat current);
					unsafe
					{
						current = *unchecked((PixelFormat*)currentPtr++);
					}

					if (current is PixelFormat.Unknown)
					{
						break;
					}

					yield return current;

					if (Properties?.TryGetPointerValue(PropertyNames.TextureFormatsPointer, out textureFormatsPtr) is not true
						|| startPtr != textureFormatsPtr)
					{
						failTextureFormatsChanged();
					}
				}
			}

			[DoesNotReturn]
			static void failTextureFormatsNull() => throw new InvalidOperationException(message: "The reference to the texture formats is invalid");

			[DoesNotReturn]
			static void failTextureFormatsChanged() => throw new InvalidOperationException(message: "The reference to the texture formats has changed or the texture formats list itself has changed during the enumeration");
		}
	}

	/// <summary>
	/// Gets a value indicating whether the renderer supports texture address wrapping on non-power-of-two textures
	/// </summary>
	/// <value>
	/// A value indicating whether the renderer supports texture address wrapping on non-power-of-two textures
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property determines whether the renderer supports <see cref="TextureAddressMode.Wrap"/> on textures that don't have power-of-two dimensions.
	/// </para>
	/// <para>
	/// Texture address wrapping is always supported for power-of-two texture sizes.
	/// </para>
	/// </remarks>
	public bool SupportsNonPowerOfTwoTextureWrapping => Properties?.TryGetBooleanValue(PropertyNames.TextureWrappingBoolean, out var supportsWrapping) is true
		&& supportsWrapping;

	private protected abstract Texture? GetTargetImpl();

	/// <summary>
	/// Gets the current render target
	/// </summary>
	/// <value>
	/// The current render target, or <c><see langword="null"/></c> if the default render target is active
	/// </value>
	/// <remarks>
	/// <para>
	/// If the value of this property is <c><see langword="null"/></c>, then the default render target is active.
	/// The default render target is the <see cref="Windowing.Window"/> associated with the renderer, the <see cref="Surface"/> associated with a software renderer, or the off-screen target associated with a GPU renderer.
	/// </para>
	/// <para>
	/// You can reset the current render target to the default render target by either setting this property to <c><see langword="null"/></c>, or by using the <see cref="ResetTarget"/> method.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be used as render targets, and only if they were created with the <see cref="TextureAccess.Target"/> access mode.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="ObjectDisposedException">
	/// When setting this property, the specified texture was already disposed
	/// </exception>
	/// <exception cref="SdlException">
	/// When setting this property, the specified texture is not a valid render target (e.g. it wasn't created with the <see cref="TextureAccess.Target"/> access mode, or it wasn't created with this renderer) (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// - OR -
	/// When setting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public Texture? Target
	{
		get => GetTargetImpl();

		set
		{
			unsafe
			{
				Texture.SDL_Texture* texturePtr;
				if (value is null)
				{
					texturePtr = null;
				}
				else
				{
					texturePtr = value.Pointer;

					// This is actually one of the rare cases where we need to check if the object has been disposed manually.
					// That's because SDL_SetRenderTarget(..., null) has special semantics, in that it resets the render target to the window, if no texture is explicitly set as the render target (the texture argument is NULL).
					// Of course, if the user gives us a non-null Texture (managed), they expect it to be set as the SDL_Texture (native) render target.
					// If the given Texture has already been disposed, there's no SDL_Texture to set, its underlying SDL_Texture* pointer will be null.
					if (texturePtr is null)
					{
						failValueArgumentDisposed();
					}
				}

				SdlErrorHelper.ThrowIfFailed(SDL_SetRenderTarget(Pointer, texturePtr), filterError: GetInvalidRendererErrorMessage());
			}

			[DoesNotReturn]
			static void failValueArgumentDisposed() => throw new ObjectDisposedException(nameof(value), message: $"The given {nameof(Texture)} has already been disposed");
		}
	}

#if SDL3_4_0_OR_GREATER

	/// <summary>
	/// Gets or sets the texture address modes used in <see cref="TryRenderGeometry(ReadOnlySpan{Vertex}, Texture?)"/> and <see cref="TryRenderGeometry(ReadOnlySpan{Vertex}, ReadOnlySpan{int}, Texture?)"/>
	/// </summary>
	/// <value>
	/// The texture address modes used in <see cref="TryRenderGeometry(ReadOnlySpan{Vertex}, Texture?)"/> and <see cref="TryRenderGeometry(ReadOnlySpan{Vertex}, ReadOnlySpan{int}, Texture?)"/>.
	/// </value>
	/// <remarks>
	/// <para>
	/// The <see cref="TextureAddressMode"/>s specified by this property determine the horizontal addressing mode (UMode) and vertical addressing mode (VMode) respectively.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public (TextureAddressMode UMode, TextureAddressMode VMode) TextureAddressMode
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out (TextureAddressMode UMode, TextureAddressMode VMode) modes);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderTextureAddressMode(mRenderer, &modes.UMode, &modes.VMode), filterError: GetInvalidRendererErrorMessage());

				return modes;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetRenderTextureAddressMode(mRenderer, value.UMode, value.VMode), filterError: GetInvalidRendererErrorMessage());
			}
		}
	}

#endif

	/// <summary>
	/// Gets or sets the drawing area for rendering on the current target
	/// </summary>
	/// <value>
	/// The drawing area for rendering on the current target, in pixels
	/// </value>
	/// <remarks>
	/// <para>
	/// Drawing operations will clip to the area defined by this property (separatly from any clipping defined by the <see cref="ClippingRect"/> property),
	/// and the top-left of the area will become the coordinates origin (0, 0) for future drawing operations.
	/// </para>
	/// <para>
	/// The area defined by this property must be ≥ 0.
	/// </para>
	/// <para>
	/// You can use the <see cref="IsViewportSet"/> property to check whether an explicit rectangle was set as the current viewport for the renderer or not,
	/// and the <see cref="ResetViewport"/> method to reset the viewport back to the default, which is the entire target area.
	/// </para>
	/// <para>
	/// Each render target, <see cref="Window"/> or <see cref="Target"/> (or <see cref="RendererExtensions.get_Surface(Renderer{Drivers.Software})"/> in the case of a software renderer), has it own viewport.
	/// This property reflects the viewport of the <em>current</em> target.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public Rect<int> Viewport
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out Rect<int> rect);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderViewport(mRenderer, &rect), filterError: GetInvalidRendererErrorMessage());

				return rect;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetRenderViewport(mRenderer, &value), filterError: GetInvalidRendererErrorMessage());
			}
		}
	}

	/// <summary>
	/// Gets or sets the vertical synchronization (VSync) mode or interval for the renderer
	/// </summary>
	/// <value>
	/// The vertical synchronization (VSync) mode or interval for the renderer
	/// </value>
	/// <remarks>
	/// <para>
	/// You can set the value of this property to <see cref="RendererVSync.Disabled"/> to disable VSync,
	/// <see cref="RendererVSync.Adaptive"/> to enable late swap tearing (adaptive VSync) if supported,
	/// or use the <see cref="RendererVSyncExtensions.Interval(int)"/> method to specify a custom VSync interval.
	/// You can specify a custom interval of <c>1</c> to synchronize to present of the renderer with <em>every</em> vertical refresh,
	/// <c>2</c> to synchronize it with <em>every second</em> vertical refresh, and so on.
	/// </para>
	/// <para>
	/// When a renderer is newly created, the value of this property defaults to <see cref="RendererVSync.Disabled"/>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting this property, the renderer is a software renderer associated with a <see cref="Surface"/> instead of a <see cref="Windowing.Window"/>, and you tried to enable VSync (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// - OR -
	/// When setting this property, the specified VSync mode or interval is not supported by the renderer (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// - OR -
	/// When setting or getting this property, SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public RendererVSync VSync
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out RendererVSync vsync);

				SdlErrorHelper.ThrowIfFailed(SDL_GetRenderVSync(mRenderer, &vsync), filterError: GetInvalidRendererErrorMessage());

				return vsync;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetRenderVSync(mRenderer, value), filterError: GetInvalidRendererErrorMessage());
			}
		}
	}

	/// <summary>
	/// Gets the <see cref="Windowing.Window"/> associated with the renderer
	/// </summary>
	/// <value>
	/// The <see cref="Windowing.Window"/> associated with the renderer, or <c><see langword="null"/></c> if there is no associated window
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property can be <c><see langword="null"/></c> if the renderer is a software renderer with an associated <see cref="Surface"/> instead
	/// or if the renderer is a GPU renderer associated with an off-screen target.
	/// </para>
	/// </remarks>
	public Window? Window
	{
		get
		{
			unsafe
			{
				Window.TryGetOrCreate(SDL_GetRenderWindow(mRenderer), out var window);

				return window;
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
			if (mRenderer is not null)
			{
				if (forget)
				{
					mKnownInstances.TryRemove(unchecked((IntPtr)mRenderer), out _);
				}

				SDL_DestroyRenderer(mRenderer);
				mRenderer = null;
			}
		}
	}

	/// <summary>
	/// Disables the clipping rectangle for the current target
	/// </summary>
	/// <remarks>
	/// <para>
	/// After a call to this method, the value of the <see cref="IsClippingEnabled"/> property will be <c><see langword="false"/></c>.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public void ResetClippingRect()
	{
		unsafe
		{
			SdlErrorHelper.ThrowIfFailed(SDL_SetRenderClipRect(mRenderer, rect: null), filterError: GetInvalidRendererErrorMessage());
		}
	}

	/// <summary>
	/// Resets the current target to the default render target
	/// </summary>
	/// <remarks>
	/// <para>
	/// After a call to this method, the value of the <see cref="Target"/> property will be <c><see langword="null"/></c>.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public void ResetTarget()
	{
		unsafe
		{
			SdlErrorHelper.ThrowIfFailed(SDL_SetRenderTarget(mRenderer, texture: null), filterError: GetInvalidRendererErrorMessage());
		}
	}

	/// <summary>
	/// Resets the drawing area for rendering on the current target to the entire target area
	/// </summary>
	/// <remarks>
	/// <para>
	/// After a call to this method, the value of the <see cref="IsViewportSet"/> property will be <c><see langword="false"/></c>.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// SDL failed with an error (check <see cref="Error.TryGet(out string?)"/> for more information).
	/// A common reason for this to happen is when the <see cref="Windowing.Window"/> associated with this renderer was already disposed, but the renderer itself wasn't disposed yet.
	/// </exception>
	public void ResetViewport()
	{
		unsafe
		{
			SdlErrorHelper.ThrowIfFailed(SDL_SetRenderViewport(mRenderer, rect: null), filterError: GetInvalidRendererErrorMessage());
		}
	}

	/// <summary>
	/// Tries to clear the current target with the drawing color
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if the target was cleared successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method clears the entire current rendering target with the current <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>), ignoring any <see cref="Viewport"/> or <see cref="ClippingRect"/> settings.
	/// The <see cref="DrawBlendMode"/> does not affect this method.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryClear()
	{
		unsafe
		{
			return SDL_RenderClear(mRenderer);
		}
	}

	/// <summary>
	/// Tries to get a point in window coordinates for a specified point in render coordinates
	/// </summary>
	/// <param name="x">The X coordinate in render coordinates to convert to window coordinates</param>
	/// <param name="y">The Y coordinate in render coordinates to convert to window coordinates</param>
	/// <param name="windowX">The X coordinate in window coordinates corresponding to the given render coordinates, if this method returns <c><see langword="true"/></c></param>
	/// <param name="windowY">The Y coordinate in window coordinates corresponding to the given render coordinates, if this method returns <c><see langword="true"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the conversion was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The conversion takes into account several factors:
	/// <list type="bullet">
	///	<item><description>The <see cref="Window"/> dimensions</description></item>
	///	<item><description>The <see cref="LogicalPresentation"/> settings</description></item>
	///	<item><description>The <see cref="Scale"/></description></item>
	///	<item><description>The <see cref="Viewport"/></description></item>
	/// </list>
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryConvertRenderToWindowCoordinates(float x, float y, out float windowX, out float windowY)
	{
		unsafe
		{
			Unsafe.SkipInit(out float xTmp);
			Unsafe.SkipInit(out float yTmp);

			bool result = SDL_RenderCoordinatesToWindow(mRenderer, x, y, &xTmp, &yTmp);

			windowX = xTmp;
			windowY = yTmp;

			return result;
		}
	}

	/// <summary>
	/// Tries to get a point in render coordinates for a specified point in window coordinates
	/// </summary>
	/// <param name="windowX">The X coordinate in window coordinates to convert to render coordinates</param>
	/// <param name="windowY">The Y coordinate in window coordinates to convert to render coordinates</param>
	/// <param name="x">The X coordinate in render coordinates corresponding to the given window coordinates, if this method returns <c><see langword="true"/></c></param>
	/// <param name="y">The Y coordinate in render coordinates corresponding to the given window coordinates, if this method returns <c><see langword="true"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the conversion was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The conversion takes into account several factors:
	/// <list type="bullet">
	///	<item><description>The <see cref="Window"/> dimensions</description></item>
	///	<item><description>The <see cref="LogicalPresentation"/> settings</description></item>
	///	<item><description>The <see cref="Scale"/></description></item>
	///	<item><description>The <see cref="Viewport"/></description></item>
	/// </list>
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryConvertWindowToRenderCoordinates(float windowX, float windowY, out float x, out float y)
	{
		unsafe
		{
			Unsafe.SkipInit(out float xTmp);
			Unsafe.SkipInit(out float yTmp);

			bool result = SDL_RenderCoordinatesFromWindow(mRenderer, windowX, windowY, &xTmp, &yTmp);

			x = xTmp;
			y = yTmp;

			return result;
		}
	}

	private protected abstract bool TryCreateTextureImpl(PixelFormat format, TextureAccess access, int width, int height, [NotNullWhen(true)] out Texture? texture);

	/// <summary>
	/// Tries to create a new texture for the renderer
	/// </summary>
	/// <param name="format">The pixel format of the texture. Should be one of the supported texture formats returned by the <see cref="SupportedTextureFormats"/> property.</param>
	/// <param name="access">The intended access pattern of the texture. Should be one of the pre-defined <see cref="TextureAccess"/> values.</param>
	/// <param name="width">The width of the texture in pixels</param>
	/// <param name="height">The height of the texture in pixels</param>
	/// <param name="texture">The resulting texture, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the texture was created successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The contents of a newly created texture are undefined.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryCreateTexture(PixelFormat format, TextureAccess access, int width, int height, [NotNullWhen(true)] out Texture? texture)
		=> TryCreateTextureImpl(format, access, width, height, out texture);

	private protected abstract bool TryCreateTextureImpl([NotNullWhen(true)] out Texture? texture, ColorSpace? colorSpace = default, PixelFormat? format = default, TextureAccess? access = default, int? width = default, int? height = default,
#if SDL3_4_0_OR_GREATER
		Palette? palette = default,
#endif
		float? sdrWhitePoint = default, float? hdrHeadroom = default, Properties? properties = default);

	// the inability to use preprocessor directives in XML documentation strikes again...
#if SDL3_4_0_OR_GREATER
	/// <summary>
	/// Tries to create a new texture for the renderer
	/// </summary>
	/// <param name="texture">The resulting texture, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <param name="colorSpace">
	/// The color space of the texture.
	/// If not specified, defaults to <see cref="ColorSpace.SrgbLinear"/> for floating point textures,
	/// <see cref="ColorSpace.Hdr10"/> for 10-bit textures, <see cref="ColorSpace.Srgb"/> for other RGB textures,
	/// and <see cref="ColorSpace.Jpeg"/> for YUV textures, or whatever the provided <paramref name="properties"/> specify.
	/// </param>
	/// <param name="format">
	/// The pixel format of the texture.
	/// Should be one of the supported texture formats returned by the <see cref="SupportedTextureFormats"/> property.
	/// If not specified, defaults to the best RGBA format available for the renderer, or whatever the provided <paramref name="properties"/> specify.
	/// </param>
	/// <param name="access">
	/// The intended access pattern of the texture.
	/// Should be one of the pre-defined <see cref="TextureAccess"/> values.
	/// If not specified, defaults to <see cref="TextureAccess.Static"/>, or whatever the provided <paramref name="properties"/> specify.
	/// </param>
	/// <param name="width">
	/// The width of the texture in pixels.
	/// Required if the provided <paramref name="properties"/> don't specify a width.
	/// </param>
	/// <param name="height">
	/// The height of the texture in pixels.
	/// Required if the provided <paramref name="properties"/> don't specify a height.
	/// </param>
	/// <param name="palette">The palette to use when creating a texture with a palettized pixel format</param>
	/// <param name="sdrWhitePoint">
	/// The defining value for 100% diffuse white for HDR10 and floating point textures.
	/// Defaults to <c>100</c> for HDR10 textures and <c>1.0</c> for floating point textures, or whatever the provided <paramref name="properties"/> specify.
	/// </param>
	/// <param name="hdrHeadroom">The maximum dynamic range for HDR10 and floating point textures in terms of the <paramref name="sdrWhitePoint"/></param>
	/// <param name="properties">Additional properties to use when creating the texture</param>
	/// <returns><c><see langword="true"/></c>, if the texture was created successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The contents of a newly created texture are undefined.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
#else
	/// <summary>
	/// Tries to create a new texture for the renderer
	/// </summary>
	/// <param name="texture">The resulting texture, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <param name="colorSpace">
	/// The color space of the texture.
	/// If not specified, defaults to <see cref="ColorSpace.SrgbLinear"/> for floating point textures,
	/// <see cref="ColorSpace.Hdr10"/> for 10-bit textures, <see cref="ColorSpace.Srgb"/> for other RGB textures,
	/// and <see cref="ColorSpace.Jpeg"/> for YUV textures, or whatever the provided <paramref name="properties"/> specify.
	/// </param>
	/// <param name="format">
	/// The pixel format of the texture.
	/// Should be one of the supported texture formats returned by the <see cref="SupportedTextureFormats"/> property.
	/// If not specified, defaults to the best RGBA format available for the renderer, or whatever the provided <paramref name="properties"/> specify.
	/// </param>
	/// <param name="access">
	/// The intended access pattern of the texture.
	/// Should be one of the pre-defined <see cref="TextureAccess"/> values.
	/// If not specified, defaults to <see cref="TextureAccess.Static"/>, or whatever the provided <paramref name="properties"/> specify.
	/// </param>
	/// <param name="width">
	/// The width of the texture in pixels.
	/// Required if the provided <paramref name="properties"/> don't specify a width.
	/// </param>
	/// <param name="height">
	/// The height of the texture in pixels.
	/// Required if the provided <paramref name="properties"/> don't specify a height.
	/// </param>
	/// <param name="sdrWhitePoint">
	/// The defining value for 100% diffuse white for HDR10 and floating point textures.
	/// Defaults to <c>100</c> for HDR10 textures and <c>1.0</c> for floating point textures, or whatever the provided <paramref name="properties"/> specify.
	/// </param>
	/// <param name="hdrHeadroom">The maximum dynamic range for HDR10 and floating point textures in terms of the <paramref name="sdrWhitePoint"/></param>
	/// <param name="properties">Additional properties to use when creating the texture</param>
	/// <returns><c><see langword="true"/></c>, if the texture was created successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The contents of a newly created texture are undefined.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
#endif
	public bool TryCreateTexture([NotNullWhen(true)] out Texture? texture, ColorSpace? colorSpace = default, PixelFormat? format = default, TextureAccess? access = default, int? width = default, int? height = default,
#if SDL3_4_0_OR_GREATER
		Palette? palette = default,
#endif
		float? sdrWhitePoint = default, float? hdrHeadroom = default, Properties? properties = default)
		=> TryCreateTextureImpl(out texture, colorSpace, format, access, width, height,
#if SDL3_4_0_OR_GREATER
			palette,
#endif
			sdrWhitePoint, hdrHeadroom, properties
		);

	private protected abstract bool TryCreateTextureFromSurfaceImpl(Surface surface, [NotNullWhen(true)] out Texture? texture);

	/// <summary>
	/// Tries to create a texture from an exisiting <see cref="Surface"/>
	/// </summary>
	/// <param name="surface">The <see cref="Surface"/> to copy and create the texture from</param>
	/// <param name="texture">The resulting texture, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the texture was created successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The given <paramref name="surface"/>'s pixel data is copied into the texture and the <paramref name="surface"/> is not modified in any way.
	/// This means that the given <paramref name="surface"/> can be safely disposed of after this method returns, without affecting the resulting texture.
	/// </para>
	/// <para>
	/// The pixel format of the resulting texture my be different from the pixel format of the given <paramref name="surface"/>.
	/// The actual pixel format of the resulting texture can later be checked using the <see cref="Texture.Format"/> property.
	/// </para>
	/// <para>
	/// The <see cref="TextureAccess"/> of the resulting texture will be <see cref="TextureAccess.Static"/>.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryCreateTextureFromSurface(Surface surface, [NotNullWhen(true)] out Texture? texture)
		=> TryCreateTextureFromSurfaceImpl(surface, out texture);

	/// <summary>
	/// Tries to flush any pending rendering operations on the current target
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if the operation was successful; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// You <em>do not need to</em> (and in fact, <em>shouldn't</em>) call this method unless you are planning to call into OpenGL/Direct3D/Metal/whatever directly, in addition to using a <see cref="Renderer"/>.
	/// </para>
	/// <para>
	/// This method exists for a very-specific case: if you are using SDL's render API, and you plan to make OpenGL/D3D/whatever calls in addition to SDL render API calls.
	/// If this applies, you should call this method between calls to SDL's render API and the low-level API you're using in cooperation.
	/// </para>
	/// <para>
	/// <em>In all other cases, you can ignore this method!</em>
	/// </para>
	/// <para>
	/// A call to this method makes SDL flush any pending rendering work it was queueing up to do later in a single batch, and marks any internal cached state as invalid, so it'll prepare all its state again later, from scratch.
	/// </para>
	/// <para>
	/// This means you do not need to save state in your rendering code to protect the SDL renderer.
	/// However, there lots of arbitrary pieces of Direct3D and OpenGL state that can confuse things; you should use your best judgment and be prepared to make changes if specific state needs to be protected.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryFlush()
	{
		unsafe
		{
			return SDL_FlushRenderer(mRenderer);
		}
	}

	internal unsafe static bool TryGetOrCreate(SDL_Renderer* renderer, [NotNullWhen(true)] out Renderer? result)
	{
		if (renderer is null)
		{
			result = null;
			return false;
		}

		var rendererRef = mKnownInstances.GetOrAdd(unchecked((IntPtr)renderer), createRef);

		if (!rendererRef.TryGetTarget(out result))
		{
			rendererRef.SetTarget(result = create(renderer));
		}

		return true;

		static WeakReference<Renderer> createRef(IntPtr renderer) => new(create(unchecked((SDL_Renderer*)renderer)));

		static Renderer create(SDL_Renderer* renderer)
		{
			// Conversly to textures, renderers are not ref-counted, so there's no ref counter manipulating shenanigans here

			if (!TryCreateFromRegisteredDriver(renderer, register: false, out var result))
			{
				result = new Renderer<GenericFallbackRendereringDriver>(renderer, register: false);
			}

			return result;
		}
	}

	internal unsafe static bool TryGetOrCreate<TDriver>(SDL_Renderer* renderer, [NotNullWhen(true)] out Renderer<TDriver>? result)
		where TDriver : notnull, IRenderingDriver
	{
		if (renderer is null)
		{
			result = null;
			return false;
		}

		var rendererRef = mKnownInstances.GetOrAdd(unchecked((IntPtr)renderer), createRef);

		if (!rendererRef.TryGetTarget(out var baseResult))
		{
			rendererRef.SetTarget(result = create(renderer));
		}
		else if (baseResult is Renderer<TDriver> typedResult)
		{
			// we optimistically assume that everything's fine, if the managed types match

			result = typedResult;
		}
		else if (baseResult.Pointer is not null)
		{
			// this also means that baseResult.Pointer == renderer
			// this indicates that we actually need the renderer to be of a different managed type than it currently is,
			// we should just fail in that case

			result = null;
			return false;
		}
		else
		{
			// this indicates that we somehow managed to not properly forget a managed instance that was disposed,
			// so we need to fully recreate the managed instance with the new type here

			result = create(renderer);
		}

		return true;

		static WeakReference<Renderer> createRef(IntPtr renderer) => new(create(unchecked((SDL_Renderer*)renderer)));

		static Renderer<TDriver> create(SDL_Renderer* renderer)
		{
			// Conversly to textures, renderers are not ref-counted, so there's no ref counter manipulating shenanigans here

			return new(renderer, register: false);
		}
	}

	/// <summary>
	/// Tries to read the pixels of an area of the current target into a new <see cref="Surface"/>
	/// </summary>
	/// <param name="rect">The area of the current target to read the pixels from</param>
	/// <param name="pixels">The resulting <see cref="Surface"/> containing the read pixels, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the pixels were successfully read into a new <see cref="Surface"/>; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The resulting <see cref="Surface"/> contains a copy of the pixels in the area specified by the given <paramref name="rect"/> clipped to the current <see cref="Viewport"/>.
	/// </para>
	/// <para>
	/// Note that this method copies the actual pixels on the screen. So if you are using any form of <see cref="LogicalPresentation">logical presentation</see>,
	/// you should use <see cref="LogicalPresentationRect"/> to get the area containing your content.
	/// </para>
	/// <para>
	/// <em>WARNING</em>: This is a very slow operation, and should not be used frequently.
	/// If you're using this on the main rendering target, it should be called after rendering and before <see cref="TryRenderPresent"/>.
	/// </para>
	/// <para>
	/// Please remember to dispose of the resulting <see cref="Surface"/> when you're done using it.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryReadPixels(in Rect<int> rect, [NotNullWhen(true)] out Surface? pixels)
	{
		unsafe
		{
			Surface.SDL_Surface* surfacePtr;
			fixed (Rect<int>* rectPtr = &rect)
			{
				surfacePtr = SDL_RenderReadPixels(mRenderer, rectPtr);
			}

			if (surfacePtr is null)
			{
				pixels = null;
				return false;
			}

			// Interestingly, we can't use Surface.TryGetOrCreate here, because that would *additionally* increase its ref counter,
			// but the resulting surface is detached from the renderer's lifetime, and is explicitly stated as needing to be freed with SDL_DestroySurface.
			// That means, that if we were to use Surface.TryGetOrCreate here, the ref counter would be off by one, resulting in the need to call SDL_DestroySurface twice.
			// Therefore, this is one of the rare cases where we need to call the Surface constructor directly.
			pixels = new Surface(surfacePtr, register: true);
			return true;
		}
	}

	/// <summary>
	/// Tries to read the pixels of the entirety of the current target into a new <see cref="Surface"/>
	/// </summary>
	/// <param name="pixels">The resulting <see cref="Surface"/> containing the read pixels, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c>, if the pixels were successfully read into a new <see cref="Surface"/>; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The resulting <see cref="Surface"/> contains a copy of the pixels of the entire texture clipped to the current <see cref="Viewport"/>.
	/// </para>
	/// <para>
	/// Note that this method copies the actual pixels on the screen. So if you are using any form of <see cref="LogicalPresentation">logical presentation</see>,
	/// you should use <see cref="LogicalPresentationRect"/> to get the area containing your content.
	/// </para>
	/// <para>
	/// <em>WARNING</em>: This is a very slow operation, and should not be used frequently.
	/// If you're using this on the main rendering target, it should be called after rendering and before <see cref="TryRenderPresent"/>.
	/// </para>
	/// <para>
	/// Please remember to dispose of the resulting <see cref="Surface"/> when you're done using it.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryReadPixels([NotNullWhen(true)] out Surface? pixels)
	{
		unsafe
		{
			var surfacePtr = SDL_RenderReadPixels(mRenderer, rect: null);

			if (surfacePtr is null)
			{
				pixels = null;
				return false;
			}

			// Interestingly, we can't use Surface.TryGetOrCreate here, because that would *additionally* increase its ref counter,
			// but the resulting surface is detached from the renderer's lifetime, and is explicitly stated as needing to be freed with SDL_DestroySurface.
			// That means, that if we were to use Surface.TryGetOrCreate here, the ref counter would be off by one, resulting in the need to call SDL_DestroySurface twice.
			// Therefore, this is one of the rare cases where we need to call the Surface constructor directly.
			pixels = new Surface(surfacePtr, register: true);
			return true;
		}
	}

	//TODO: replace SDL_ttf in the xml doc with Sdl3Sharp's adaptation of it, once it's done
	/// <summary>
	/// Tries to draw debug text to the current target
	/// </summary>
	/// <param name="x">The top-left X coordinate where the text should be drawn</param>
	/// <param name="y">The top-left Y coordinate where the text should be drawn</param>
	/// <param name="text">The text to be drawn</param>
	/// <returns><c><see langword="true"/></c>, if the text was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method will render text to a <see cref="Renderer"/>.
	/// Note that this is a convenience method for debugging, with severe limitations, and not intended to be used for production applications and games.
	/// </para>
	/// <para>
	/// Among these limitations are:
	/// <list type="bullet">
	/// <item><description>
	/// It accepts Unicode strings, but will only render ASCII characters
	/// </description></item>
	/// <item><description>
	/// It has a single, tiny size (8x8 pixels). You can use <see cref="LogicalPresentation"/> or <see cref="Scale"/> to adjust for that.
	/// </description></item>
	/// <item><description>
	/// It uses a simple, hardcoded bitmap font. It does not allow different font selections and it does not support truetype, for proper scaling.
	/// </description></item>
	/// <item><description>
	/// It doesn't do word-wrapping and doesn't treat newline characters as a line break. If the text goes out of the target, it's gone.
	/// </description></item>
	/// </list>
	/// </para>
	/// <para>
	/// For more serious text rendering, there are several good options, such as <see href="https://wiki.libsdl.org/SDL3/SDL_ttf">SDL_ttf</see>.
	/// </para>
	/// <para>
	/// On first use, this will create an internal texture for rendering glyphs. This texture will live until the renderer is disposed.
	/// </para>
	/// <para>
	/// The text is drawn in the color specified by <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>) and <see cref="DrawBlendMode"/> determine how the text is blended with the existing content of the target.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderDebugText(float x, float y, string text)
	{
		unsafe
		{
			var textUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(text);
			try
			{
				return SDL_RenderDebugText(mRenderer, x, y, textUtf8);
			}
			finally
			{
				Utf8StringMarshaller.Free(textUtf8);
			}
		}
	}

	/// <summary>
	/// Tries to draw a debug format string to the current target
	/// </summary>
	/// <param name="x">The top-left X coordinate where the text should be drawn</param>
	/// <param name="y">The top-left Y coordinate where the text should be drawn</param>
	/// <param name="format">The C-style <c>printf</c> format string</param>
	/// <param name="args">The arguments corresponding to the format specifiers in <paramref name="format"/></param>
	/// <returns><c><see langword="true"/></c>, if the text was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method will render text to a <see cref="Renderer"/>.
	/// Note that this is a convenience method for debugging, with severe limitations, and not intended to be used for production applications and games.
	/// </para>
	/// <para>
	/// Among these limitations are:
	/// <list type="bullet">
	/// <item><description>
	/// It accepts Unicode strings, but will only render ASCII characters
	/// </description></item>
	/// <item><description>
	/// It has a single, tiny size (8x8 pixels). You can use <see cref="LogicalPresentation"/> or <see cref="Scale"/> to adjust for that.
	/// </description></item>
	/// <item><description>
	/// It uses a simple, hardcoded bitmap font. It does not allow different font selections and it does not support truetype, for proper scaling.
	/// </description></item>
	/// <item><description>
	/// It doesn't do word-wrapping and doesn't treat newline characters as a line break. If the text goes out of the target, it's gone.
	/// </description></item>
	/// </list>
	/// </para>
	/// <para>
	/// For more serious text rendering, there are several good options, such as <see href="https://wiki.libsdl.org/SDL3/SDL_ttf">SDL_ttf</see>.
	/// </para>
	/// <para>
	/// On first use, this will create an internal texture for rendering glyphs. This texture will live until the renderer is disposed.
	/// </para>
	/// <para>
	/// The text is drawn in the color specified by <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>) and <see cref="DrawBlendMode"/> determine how the text is blended with the existing content of the target.
	/// </para>
	/// <para>
	/// The <paramref name="format"/> parameter is interpreted as a C-style <c>printf</c> format string, and 
	/// the <paramref name="args"/> parameter supplies the values for its format specifiers. Supported argument 
	/// types include all integral types up to 64-bit (including <c><see langword="bool"/></c> and <c><see langword="char"/></c>), 
	/// floating point types (<c><see langword="float"/></c> and <c><see langword="double"/></c>), pointer types 
	/// (<c><see cref="IntPtr"/></c> and <c><see cref="UIntPtr"/></c>), and <c><see langword="string"/></c>.
	/// </para>
	/// <para>
	/// For a detailed explanation of C-style <c>printf</c> format strings and their specifiers, see 
	/// <see href="https://en.wikipedia.org/wiki/Printf#Format_specifier"/>.
	/// </para>
	/// <para>
	/// Consider using <see cref="TryRenderDebugText(float, float, string)"/> instead when possible, as it may be more efficient. 
	/// In many cases, you can use C# string interpolation to construct the message before logging.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderDebugText(float x, float y, string format, params ReadOnlySpan<object> args)
	{
		unsafe
		{
			return Variadic.Invoke<CBool>(in SDL_RenderDebugTextFormat_var(), 4, [unchecked((IntPtr)mRenderer), x, y, format, .. args]);
		}
	}

	/// <summary>
	/// Tries to draw a filled rectangle to an area of the current target
	/// </summary>
	/// <param name="rect">The area of the current target to draw the filled rectangle in</param>
	/// <returns><c><see langword="true"/></c>, if the filled rectangle was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The rectangle is filled with the color specified by <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>) and <see cref="DrawBlendMode"/> determines how the rectangle is blended with the existing content of the target.
	/// </para>
	/// <para>
	/// Drawing the rectangle works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderFilledRect(in Rect<float> rect)
	{
		unsafe
		{
			fixed (Rect<float>* rectPtr = &rect)
			{
				return SDL_RenderFillRect(mRenderer, rectPtr);
			}
		}
	}

	/// <summary>
	/// Tries to draw a filled rectangle to the entirety of the current target
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if the filled rectangle was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The rectangle is filled with the color specified by <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>) and <see cref="DrawBlendMode"/> determines how the rectangle is blended with the existing content of the target.
	/// </para>
	/// <para>
	/// Drawing the rectangle works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderFilledRect()
	{
		unsafe
		{
			return SDL_RenderFillRect(mRenderer, rect: null);
		}
	}

	/// <summary>
	/// Tries to draw multiple filled rectangles to areas of the current target
	/// </summary>
	/// <param name="rects">The list of areas of the current target to draw the filled rectangles in</param>
	/// <returns><c><see langword="true"/></c>, if the filled rectangles were drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The rectangles are filled with the color specified by <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>) and <see cref="DrawBlendMode"/> determines how the rectangles are blended with the existing content of the target.
	/// </para>
	/// <para>
	/// Drawing the rectangles works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderFilledRects(ReadOnlySpan<Rect<float>> rects)
	{
		unsafe
		{
			fixed (Rect<float>* rectsPtr = rects)
			{
				return SDL_RenderFillRects(mRenderer, rectsPtr, rects.Length);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private unsafe static bool TryGetTexturePointer(Texture? texture, out Texture.SDL_Texture* texturePtr)
	{
		if (texture is null)
		{
			texturePtr = null;
			return true;
		}
		else if (texture.Pointer is var pointer && pointer is not null)
		{
			texturePtr = pointer;
			return true;
		}
		else
		{
			// if we were given a non-null texture that has already been disposed, we fail here
			texturePtr = null;
			return false;
		}
	}

	/// <summary>
	/// Tries to draw a list of triangles specified by a list of indices into a list of vertices to the current target, optionally using a texture
	/// </summary>
	/// <param name="vertices">The vertices to use</param>
	/// <param name="indices">The list of indices into the vertex list to specify the triangles to draw</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// There are no surface-level checks for the validity of the given <paramref name="indices"/>, so you must make sure they are all within the bounds of the given <paramref name="vertices"/> list.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderGeometry(ReadOnlySpan<Vertex> vertices, ReadOnlySpan<int> indices, Texture? texture = default)
	{
		unsafe
		{
			if (!TryGetTexturePointer(texture, out var texturePtr))
			{
				// if we were given a non-null texture that has already been disposed, we fail early here
				return false;
			}

			fixed (Vertex* verticesPtr = vertices)
			fixed (int* indicesPtr = indices)
			{
				return SDL_RenderGeometry(mRenderer, texturePtr, verticesPtr, vertices.Length, indicesPtr, indices.Length);
			}
		}
	}

	/// <summary>
	/// Tries to draw a list of triangles specified by a list of vertices to the current target, optionally using a texture
	/// </summary>
	/// <param name="vertices">The list of vertices to specify the triangles to draw</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderGeometry(ReadOnlySpan<Vertex> vertices, Texture? texture = default)
	{
		unsafe
		{
			if (!TryGetTexturePointer(texture, out var texturePtr))
			{
				// if we were given a non-null texture that has already been disposed, we fail early here
				return false;
			}

			fixed (Vertex* verticesPtr = vertices)
			{
				return SDL_RenderGeometry(mRenderer, texturePtr, verticesPtr, vertices.Length, indices: null, num_indices: 0);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static int GetVerticesCount(ReadOnlyNativeMemory<float> xy, int xyStride, ReadOnlyNativeMemory<Color<float>> colors, int colorStride, ReadOnlyNativeMemory<float> uv, int uvStride)
	{
		// calculate the number of vertices based on the lengths and strides of the inputs and selecting the minimum among them
		return unchecked((int)nuint.Min(
			xyStride is > 0 ? unchecked(((ReadOnlyNativeMemory)xy).Length / (nuint)xyStride) : 0,
			nuint.Min(
				colorStride is > 0 ? unchecked(((ReadOnlyNativeMemory)colors).Length / (nuint)colorStride) : 0,
				nuint.Min(
					uvStride is > 0 ? unchecked(((ReadOnlyNativeMemory)uv).Length / (nuint)uvStride) : 0,
					int.MaxValue
				)
			)
		));
	}

	private bool TryRenderGeometryRawImpl<TIndex>(ReadOnlyNativeMemory<float> xy, int xyStride, ReadOnlyNativeMemory<Color<float>> colors, int colorStride, ReadOnlyNativeMemory<float> uv, int uvStride, ReadOnlySpan<TIndex> indices, Texture? texture = default)
		where TIndex : unmanaged
	{
		unsafe
		{
			if (!xy.IsValid || !colors.IsValid || !uv.IsValid)
			{
				return false;
			}

			if (!TryGetTexturePointer(texture, out var texturePtr))
			{
				// if we were given a non-null texture that has already been disposed, we fail early here
				return false;
			}

			var verticesCount = GetVerticesCount(xy, xyStride, colors, colorStride, uv, uvStride);

			fixed (TIndex* indicesPtr = indices)
			{
				return SDL_RenderGeometryRaw(mRenderer, texturePtr, xy.RawPointer, xyStride, colors.RawPointer, colorStride, uv.RawPointer, uvStride, verticesCount, indicesPtr, indices.Length, Unsafe.SizeOf<TIndex>());
			}
		}
	}

	/// <summary>
	/// Tries to draw a list of triangles specified by a list of <see langword="int"/> indices into separate lists of vertex attributes to the current target, optionally using a texture
	/// </summary>
	/// <param name="xy">The list of vertex positions, first the X coordinate and then the Y coordinate for each vertex</param>
	/// <param name="xyStride">The length, in bytes, to move from one element in the <paramref name="xy"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="colors">The list of vertex colors</param>
	/// <param name="colorStride">The length, in bytes, to move from one element in the <paramref name="colors"/> list to the next. Usually that's <c><see langword="sizeof"/>(<see cref="Color{T}">Color&lt;<see langword="float"/>&gt;</see>)</c>.</param>
	/// <param name="uv">The list of normalized texture coordinates per vertex, first the horizontal coordinate (U) and then the vertical coordinate (V) for each vertex</param>
	/// <param name="uvStride">The length, in bytes, to move from one element in the <paramref name="uv"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="indices">The list of <see cref="int"/> indices into the various vertex attribute lists to specify the triangles to draw</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The number of vertices specified by the various vertex attribute lists is determined by their byte lengths and strides, always selecting the smallest resulting vertex count among them.
	/// </para>
	/// <para>
	/// There are no surface-level checks for the validity of the given <paramref name="indices"/>, so you must make sure they are all within the bounds of the calculated vertex count from the given vertex attribute lists.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderGeometryRaw(ReadOnlyNativeMemory<float> xy, int xyStride, ReadOnlyNativeMemory<Color<float>> colors, int colorStride, ReadOnlyNativeMemory<float> uv, int uvStride, ReadOnlySpan<int> indices, Texture? texture = default)
		=> TryRenderGeometryRawImpl(xy, xyStride, colors, colorStride, uv, uvStride, indices, texture);

	/// <summary>
	/// Tries to draw a list of triangles specified by a list of <see langword="short"/> indices into separate lists of vertex attributes to the current target, optionally using a texture
	/// </summary>
	/// <param name="xy">The list of vertex positions, first the X coordinate and then the Y coordinate for each vertex</param>
	/// <param name="xyStride">The length, in bytes, to move from one element in the <paramref name="xy"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="colors">The list of vertex colors</param>
	/// <param name="colorStride">The length, in bytes, to move from one element in the <paramref name="colors"/> list to the next. Usually that's <c><see langword="sizeof"/>(<see cref="Color{T}">Color&lt;<see langword="float"/>&gt;</see>)</c>.</param>
	/// <param name="uv">The list of normalized texture coordinates per vertex, first the horizontal coordinate (U) and then the vertical coordinate (V) for each vertex</param>
	/// <param name="uvStride">The length, in bytes, to move from one element in the <paramref name="uv"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="indices">The list of <see cref="short"/> indices into the various vertex attribute lists to specify the triangles to draw</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The number of vertices specified by the various vertex attribute lists is determined by their byte lengths and strides, always selecting the smallest resulting vertex count among them.
	/// </para>
	/// <para>
	/// There are no surface-level checks for the validity of the given <paramref name="indices"/>, so you must make sure they are all within the bounds of the calculated vertex count from the given vertex attribute lists.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderGeometryRaw(ReadOnlyNativeMemory<float> xy, int xyStride, ReadOnlyNativeMemory<Color<float>> colors, int colorStride, ReadOnlyNativeMemory<float> uv, int uvStride, ReadOnlySpan<short> indices, Texture? texture = default)
		=> TryRenderGeometryRawImpl(xy, xyStride, colors, colorStride, uv, uvStride, indices, texture);

	/// <summary>
	/// Tries to draw a list of triangles specified by a list of <see langword="sbyte"/> indices into separate lists of vertex attributes to the current target, optionally using a texture
	/// </summary>
	/// <param name="xy">The list of vertex positions, first the X coordinate and then the Y coordinate for each vertex</param>
	/// <param name="xyStride">The length, in bytes, to move from one element in the <paramref name="xy"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="colors">The list of vertex colors</param>
	/// <param name="colorStride">The length, in bytes, to move from one element in the <paramref name="colors"/> list to the next. Usually that's <c><see langword="sizeof"/>(<see cref="Color{T}">Color&lt;<see langword="float"/>&gt;</see>)</c>.</param>
	/// <param name="uv">The list of normalized texture coordinates per vertex, first the horizontal coordinate (U) and then the vertical coordinate (V) for each vertex</param>
	/// <param name="uvStride">The length, in bytes, to move from one element in the <paramref name="uv"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="indices">The list of <see cref="sbyte"/> indices into the various vertex attribute lists to specify the triangles to draw</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The number of vertices specified by the various vertex attribute lists is determined by their byte lengths and strides, always selecting the smallest resulting vertex count among them.
	/// </para>
	/// <para>
	/// There are no surface-level checks for the validity of the given <paramref name="indices"/>, so you must make sure they are all within the bounds of the calculated vertex count from the given vertex attribute lists.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderGeometryRaw(ReadOnlyNativeMemory<float> xy, int xyStride, ReadOnlyNativeMemory<Color<float>> colors, int colorStride, ReadOnlyNativeMemory<float> uv, int uvStride, ReadOnlySpan<sbyte> indices, Texture? texture = default)
		=> TryRenderGeometryRawImpl(xy, xyStride, colors, colorStride, uv, uvStride, indices, texture);

	/// <summary>
	/// Tries to draw a list of triangles specified by separate lists of vertex attributes to the current target, optionally using a texture
	/// </summary>
	/// <param name="xy">The list of vertex positions, first the X coordinate and then the Y coordinate for each vertex</param>
	/// <param name="xyStride">The length, in bytes, to move from one element in the <paramref name="xy"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="colors">The list of vertex colors</param>
	/// <param name="colorStride">The length, in bytes, to move from one element in the <paramref name="colors"/> list to the next. Usually that's <c><see langword="sizeof"/>(<see cref="Color{T}">Color&lt;<see langword="float"/>&gt;</see>)</c>.</param>
	/// <param name="uv">The list of normalized texture coordinates per vertex, first the horizontal coordinate (U) and then the vertical coordinate (V) for each vertex</param>
	/// <param name="uvStride">The length, in bytes, to move from one element in the <paramref name="uv"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The number of vertices specified by the various vertex attribute lists is determined by their byte lengths and strides, always selecting the smallest resulting vertex count among them.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderGeometryRaw(ReadOnlyNativeMemory<float> xy, int xyStride, ReadOnlyNativeMemory<Color<float>> colors, int colorStride, ReadOnlyNativeMemory<float> uv, int uvStride, Texture? texture = default)
	{
		unsafe
		{
			if (!xy.IsValid || !colors.IsValid || !uv.IsValid)
			{
				return false;
			}

			if (!TryGetTexturePointer(texture, out var texturePtr))
			{
				// if we were given a non-null texture that has already been disposed, we fail early here
				return false;
			}

			var verticesCount = GetVerticesCount(xy, xyStride, colors, colorStride, uv, uvStride);

			return SDL_RenderGeometryRaw(mRenderer, texturePtr, xy.RawPointer, xyStride, colors.RawPointer, colorStride, uv.RawPointer, uvStride, verticesCount, indices: null, num_indices: 0, size_indices: 0);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static int GetVerticesCount(ReadOnlySpan<float> xy, int xyStride, ReadOnlySpan<Color<float>> colors, int colorStride, ReadOnlySpan<float> uv, int uvStride)
	{
		// calculate the number of vertices based on the lengths and strides of the inputs and selecting the minimum among them
		return int.Min(
			xyStride is > 0 ? unchecked(MemoryMarshal.AsBytes(xy).Length / xyStride) : 0,
			int.Min(
				colorStride is > 0 ? unchecked(MemoryMarshal.AsBytes(colors).Length / colorStride) : 0,
				int.Min(
					uvStride is > 0 ? unchecked(MemoryMarshal.AsBytes(uv).Length / uvStride) : 0,
					int.MaxValue
				)
			)
		);
	}

	private bool TryRenderGeometryRawImpl<TIndex>(ReadOnlySpan<float> xy, int xyStride, ReadOnlySpan<Color<float>> colors, int colorStride, ReadOnlySpan<float> uv, int uvStride, ReadOnlySpan<TIndex> indices, Texture? texture = default)
		where TIndex : unmanaged
	{
		unsafe
		{
			if (!TryGetTexturePointer(texture, out var texturePtr))
			{
				// if we were given a non-null texture that has already been disposed, we fail early here
				return false;
			}

			var verticesCount = GetVerticesCount(xy, xyStride, colors, colorStride, uv, uvStride);

			fixed (float* xyPtr = xy)
			fixed (Color<float>* colorsPtr = colors)
			fixed (float* uvPtr = uv)
			fixed (TIndex* indicesPtr = indices)
			{
				return SDL_RenderGeometryRaw(mRenderer, texturePtr, xyPtr, xyStride, colorsPtr, colorStride, uvPtr, uvStride, verticesCount, indicesPtr, indices.Length, Unsafe.SizeOf<TIndex>());
			}
		}
	}

	/// <summary>
	/// Tries to draw a list of triangles specified by a list of <see langword="int"/> indices into separate lists of vertex attributes to the current target, optionally using a texture
	/// </summary>
	/// <param name="xy">The list of vertex positions, first the X coordinate and then the Y coordinate for each vertex</param>
	/// <param name="xyStride">The length, in bytes, to move from one element in the <paramref name="xy"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="colors">The list of vertex colors</param>
	/// <param name="colorStride">The length, in bytes, to move from one element in the <paramref name="colors"/> list to the next. Usually that's <c><see langword="sizeof"/>(<see cref="Color{T}">Color&lt;<see langword="float"/>&gt;</see>)</c>.</param>
	/// <param name="uv">The list of normalized texture coordinates per vertex, first the horizontal coordinate (U) and then the vertical coordinate (V) for each vertex</param>
	/// <param name="uvStride">The length, in bytes, to move from one element in the <paramref name="uv"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="indices">The list of <see cref="int"/> indices into the various vertex attribute lists to specify the triangles to draw</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The number of vertices specified by the various vertex attribute lists is determined by their byte lengths and strides, always selecting the smallest resulting vertex count among them.
	/// </para>
	/// <para>
	/// There are no surface-level checks for the validity of the given <paramref name="indices"/>, so you must make sure they are all within the bounds of the calculated vertex count from the given vertex attribute lists.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderGeometryRaw(ReadOnlySpan<float> xy, int xyStride, ReadOnlySpan<Color<float>> colors, int colorStride, ReadOnlySpan<float> uv, int uvStride, ReadOnlySpan<int> indices, Texture? texture = default)
		=> TryRenderGeometryRawImpl(xy, xyStride, colors, colorStride, uv, uvStride, indices, texture);

	/// <summary>
	/// Tries to draw a list of triangles specified by a list of <see langword="short"/> indices into separate lists of vertex attributes to the current target, optionally using a texture
	/// </summary>
	/// <param name="xy">The list of vertex positions, first the X coordinate and then the Y coordinate for each vertex</param>
	/// <param name="xyStride">The length, in bytes, to move from one element in the <paramref name="xy"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="colors">The list of vertex colors</param>
	/// <param name="colorStride">The length, in bytes, to move from one element in the <paramref name="colors"/> list to the next. Usually that's <c><see langword="sizeof"/>(<see cref="Color{T}">Color&lt;<see langword="float"/>&gt;</see>)</c>.</param>
	/// <param name="uv">The list of normalized texture coordinates per vertex, first the horizontal coordinate (U) and then the vertical coordinate (V) for each vertex</param>
	/// <param name="uvStride">The length, in bytes, to move from one element in the <paramref name="uv"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="indices">The list of <see cref="short"/> indices into the various vertex attribute lists to specify the triangles to draw</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The number of vertices specified by the various vertex attribute lists is determined by their byte lengths and strides, always selecting the smallest resulting vertex count among them.
	/// </para>
	/// <para>
	/// There are no surface-level checks for the validity of the given <paramref name="indices"/>, so you must make sure they are all within the bounds of the calculated vertex count from the given vertex attribute lists.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderGeometryRaw(ReadOnlySpan<float> xy, int xyStride, ReadOnlySpan<Color<float>> colors, int colorStride, ReadOnlySpan<float> uv, int uvStride, ReadOnlySpan<short> indices, Texture? texture = default)
		=> TryRenderGeometryRawImpl(xy, xyStride, colors, colorStride, uv, uvStride, indices, texture);

	/// <summary>
	/// Tries to draw a list of triangles specified by a list of <see langword="sbyte"/> indices into separate lists of vertex attributes to the current target, optionally using a texture
	/// </summary>
	/// <param name="xy">The list of vertex positions, first the X coordinate and then the Y coordinate for each vertex</param>
	/// <param name="xyStride">The length, in bytes, to move from one element in the <paramref name="xy"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="colors">The list of vertex colors</param>
	/// <param name="colorStride">The length, in bytes, to move from one element in the <paramref name="colors"/> list to the next. Usually that's <c><see langword="sizeof"/>(<see cref="Color{T}">Color&lt;<see langword="float"/>&gt;</see>)</c>.</param>
	/// <param name="uv">The list of normalized texture coordinates per vertex, first the horizontal coordinate (U) and then the vertical coordinate (V) for each vertex</param>
	/// <param name="uvStride">The length, in bytes, to move from one element in the <paramref name="uv"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="indices">The list of <see cref="sbyte"/> indices into the various vertex attribute lists to specify the triangles to draw</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The number of vertices specified by the various vertex attribute lists is determined by their byte lengths and strides, always selecting the smallest resulting vertex count among them.
	/// </para>
	/// <para>
	/// There are no surface-level checks for the validity of the given <paramref name="indices"/>, so you must make sure they are all within the bounds of the calculated vertex count from the given vertex attribute lists.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderGeometryRaw(ReadOnlySpan<float> xy, int xyStride, ReadOnlySpan<Color<float>> colors, int colorStride, ReadOnlySpan<float> uv, int uvStride, ReadOnlySpan<sbyte> indices, Texture? texture = default)
		=> TryRenderGeometryRawImpl(xy, xyStride, colors, colorStride, uv, uvStride, indices, texture);

	/// <summary>
	/// Tries to draw a list of triangles specified by separate lists of vertex attributes to the current target, optionally using a texture
	/// </summary>
	/// <param name="xy">The list of vertex positions, first the X coordinate and then the Y coordinate for each vertex</param>
	/// <param name="xyStride">The length, in bytes, to move from one element in the <paramref name="xy"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="colors">The list of vertex colors</param>
	/// <param name="colorStride">The length, in bytes, to move from one element in the <paramref name="colors"/> list to the next. Usually that's <c><see langword="sizeof"/>(<see cref="Color{T}">Color&lt;<see langword="float"/>&gt;</see>)</c>.</param>
	/// <param name="uv">The list of normalized texture coordinates per vertex, first the horizontal coordinate (U) and then the vertical coordinate (V) for each vertex</param>
	/// <param name="uvStride">The length, in bytes, to move from one element in the <paramref name="uv"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// /// <remarks>
	/// <para>
	/// The number of vertices specified by the various vertex attribute lists is determined by their byte lengths and strides, always selecting the smallest resulting vertex count among them.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderGeometryRaw(ReadOnlySpan<float> xy, int xyStride, ReadOnlySpan<Color<float>> colors, int colorStride, ReadOnlySpan<float> uv, int uvStride, Texture? texture = default)
	{
		unsafe
		{
			if (!TryGetTexturePointer(texture, out var texturePtr))
			{
				// if we were given a non-null texture that has already been disposed, we fail early here
				return false;
			}

			var verticesCount = GetVerticesCount(xy, xyStride, colors, colorStride, uv, uvStride);

			fixed (float* xyPtr = xy)
			fixed (Color<float>* colorsPtr = colors)
			fixed (float* uvPtr = uv)
			{
				return SDL_RenderGeometryRaw(mRenderer, texturePtr, xyPtr, xyStride, colorsPtr, colorStride, uvPtr, uvStride, verticesCount, indices: null, num_indices: 0, size_indices: 0);
			}
		}
	}

	private unsafe bool TryRenderGeometryRawImpl<TIndex>(float* xy, int xyStride, Color<float>* colors, int colorStride, float* uv, int uvStride, int verticesCount, TIndex* indices, int indicesCount, Texture? texture = default)
		where TIndex : unmanaged
	{
		if (!TryGetTexturePointer(texture, out var texturePtr))
		{
			// if we were given a non-null texture that has already been disposed, we fail early here
			return false;
		}

		return SDL_RenderGeometryRaw(mRenderer, texturePtr, xy, xyStride, colors, colorStride, uv, uvStride, verticesCount, indices, indicesCount, Unsafe.SizeOf<TIndex>());
	}

	/// <summary>
	/// Tries to draw a list of triangles specified by a list of <see langword="int"/> indices into separate lists of vertex attributes to the current target, optionally using a texture
	/// </summary>
	/// <param name="xy">A pointer to a contiguous array of vertex positions, first the X coordinate and then the Y coordinate for each vertex. Must be dereferenceable for at least <c><paramref name="verticesCount"/> * <paramref name="xyStride"/></c> bytes.</param>
	/// <param name="xyStride">The length, in bytes, to move from one element in the <paramref name="xy"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="colors">A pointer to a contiguous array of vertex colors. Must be dereferenceable for at least <c><paramref name="verticesCount"/> * <paramref name="colorStride"/></c> bytes.</param>
	/// <param name="colorStride">The length, in bytes, to move from one element in the <paramref name="colors"/> list to the next. Usually that's <c><see langword="sizeof"/>(<see cref="Color{T}">Color&lt;<see langword="float"/>&gt;</see>)</c>.</param>
	/// <param name="uv">A pointer to a contiguous array of normalized texture coordinates per vertex, first the horizontal coordinate (U) and then the vertical coordinate (V) for each vertex. Must be dereferenceable for at least <c><paramref name="verticesCount"/> * <paramref name="uvStride"/></c> bytes.</param>
	/// <param name="uvStride">The length, in bytes, to move from one element in the <paramref name="uv"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="verticesCount">The number of vertices</param>
	/// <param name="indices">A pointer to a contiguous array of <see langword="int"/> indices into the various vertex attribute lists to specify the triangles to draw. Must be dereferenceable for at least <c><paramref name="indicesCount"/> * <see langword="sizeof"/>(<see langword="int"/>)</c> bytes.</param>
	/// <param name="indicesCount">The number of <see langword="int"/> indices</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// There are no surface-level checks for the validity of the given <paramref name="indices"/>, so you must make sure they are all within the bounds of the given <paramref name="verticesCount"/>.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public unsafe bool TryRenderGeometryRaw(float* xy, int xyStride, Color<float>* colors, int colorStride, float* uv, int uvStride, int verticesCount, int* indices, int indicesCount, Texture? texture = default)
		=> TryRenderGeometryRawImpl(xy, xyStride, colors, colorStride, uv, uvStride, verticesCount, indices, indicesCount, texture);

	/// <summary>
	/// Tries to draw a list of triangles specified by a list of <see langword="short"/> indices into separate lists of vertex attributes to the current target, optionally using a texture
	/// </summary>
	/// <param name="xy">A pointer to a contiguous array of vertex positions, first the X coordinate and then the Y coordinate for each vertex. Must be dereferenceable for at least <c><paramref name="verticesCount"/> * <paramref name="xyStride"/></c> bytes.</param>
	/// <param name="xyStride">The length, in bytes, to move from one element in the <paramref name="xy"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="colors">A pointer to a contiguous array of vertex colors. Must be dereferenceable for at least <c><paramref name="verticesCount"/> * <paramref name="colorStride"/></c> bytes.</param>
	/// <param name="colorStride">The length, in bytes, to move from one element in the <paramref name="colors"/> list to the next. Usually that's <c><see langword="sizeof"/>(<see cref="Color{T}">Color&lt;<see langword="float"/>&gt;</see>)</c>.</param>
	/// <param name="uv">A pointer to a contiguous array of normalized texture coordinates per vertex, first the horizontal coordinate (U) and then the vertical coordinate (V) for each vertex. Must be dereferenceable for at least <c><paramref name="verticesCount"/> * <paramref name="uvStride"/></c> bytes.</param>
	/// <param name="uvStride">The length, in bytes, to move from one element in the <paramref name="uv"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="verticesCount">The number of vertices</param>
	/// <param name="indices">A pointer to a contiguous array of <see langword="short"/> indices into the various vertex attribute lists to specify the triangles to draw. Must be dereferenceable for at least <c><paramref name="indicesCount"/> * <see langword="sizeof"/>(<see langword="short"/>)</c> bytes.</param>
	/// <param name="indicesCount">The number of <see langword="short"/> indices</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// There are no surface-level checks for the validity of the given <paramref name="indices"/>, so you must make sure they are all within the bounds of the given <paramref name="verticesCount"/>.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public unsafe bool TryRenderGeometryRaw(float* xy, int xyStride, Color<float>* colors, int colorStride, float* uv, int uvStride, int verticesCount, short* indices, int indicesCount, Texture? texture = default)
		=> TryRenderGeometryRawImpl(xy, xyStride, colors, colorStride, uv, uvStride, verticesCount, indices, indicesCount, texture);

	/// <summary>
	/// Tries to draw a list of triangles specified by a list of <see langword="sbyte"/> indices into separate lists of vertex attributes to the current target, optionally using a texture
	/// </summary>
	/// <param name="xy">A pointer to a contiguous array of vertex positions, first the X coordinate and then the Y coordinate for each vertex. Must be dereferenceable for at least <c><paramref name="verticesCount"/> * <paramref name="xyStride"/></c> bytes.</param>
	/// <param name="xyStride">The length, in bytes, to move from one element in the <paramref name="xy"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="colors">A pointer to a contiguous array of vertex colors. Must be dereferenceable for at least <c><paramref name="verticesCount"/> * <paramref name="colorStride"/></c> bytes.</param>
	/// <param name="colorStride">The length, in bytes, to move from one element in the <paramref name="colors"/> list to the next. Usually that's <c><see langword="sizeof"/>(<see cref="Color{T}">Color&lt;<see langword="float"/>&gt;</see>)</c>.</param>
	/// <param name="uv">A pointer to a contiguous array of normalized texture coordinates per vertex, first the horizontal coordinate (U) and then the vertical coordinate (V) for each vertex. Must be dereferenceable for at least <c><paramref name="verticesCount"/> * <paramref name="uvStride"/></c> bytes.</param>
	/// <param name="uvStride">The length, in bytes, to move from one element in the <paramref name="uv"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="verticesCount">The number of vertices</param>
	/// <param name="indices">A pointer to a contiguous array of <see langword="sbyte"/> indices into the various vertex attribute lists to specify the triangles to draw. Must be dereferenceable for at least <c><paramref name="indicesCount"/> * <see langword="sizeof"/>(<see langword="sbyte"/>)</c> bytes.</param>
	/// <param name="indicesCount">The number of <see langword="sbyte"/> indices</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// There are no surface-level checks for the validity of the given <paramref name="indices"/>, so you must make sure they are all within the bounds of the given <paramref name="verticesCount"/>.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public unsafe bool TryRenderGeometryRaw(float* xy, int xyStride, Color<float>* colors, int colorStride, float* uv, int uvStride, int verticesCount, sbyte* indices, int indicesCount, Texture? texture = default)
		=> TryRenderGeometryRawImpl(xy, xyStride, colors, colorStride, uv, uvStride, verticesCount, indices, indicesCount, texture);

	/// <summary>
	/// Tries to draw a list of triangles specified by separate lists of vertex attributes to the current target, optionally using a texture
	/// </summary>
	/// <param name="xy">A pointer to a contiguous array of vertex positions, first the X coordinate and then the Y coordinate for each vertex. Must be dereferenceable for at least <c><paramref name="verticesCount"/> * <paramref name="xyStride"/></c> bytes.</param>
	/// <param name="xyStride">The length, in bytes, to move from one element in the <paramref name="xy"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="colors">A pointer to a contiguous array of vertex colors. Must be dereferenceable for at least <c><paramref name="verticesCount"/> * <paramref name="colorStride"/></c> bytes.</param>
	/// <param name="colorStride">The length, in bytes, to move from one element in the <paramref name="colors"/> list to the next. Usually that's <c><see langword="sizeof"/>(<see cref="Color{T}">Color&lt;<see langword="float"/>&gt;</see>)</c>.</param>
	/// <param name="uv">A pointer to a contiguous array of normalized texture coordinates per vertex, first the horizontal coordinate (U) and then the vertical coordinate (V) for each vertex. Must be dereferenceable for at least <c><paramref name="verticesCount"/> * <paramref name="uvStride"/></c> bytes.</param>
	/// <param name="uvStride">The length, in bytes, to move from one element in the <paramref name="uv"/> list to the next. Usually that's <c>2 * <see langword="sizeof"/>(<see langword="float"/>)</c>.</param>
	/// <param name="verticesCount">The number of vertices</param>
	/// <param name="texture">An optional texture to use when drawing the triangles</param>
	/// <returns><c><see langword="true"/></c> if the geometry was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Color and alpha modulation is done per vertex, so <see cref="Texture.ColorMod"/> (or <see cref="Texture.ColorModFloat"/>) and <see cref="Texture.AlphaMod"/> (or <see cref="Texture.AlphaModFloat"/>) are ignored.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public unsafe bool TryRenderGeometryRaw(float* xy, int xyStride, Color<float>* colors, int colorStride, float* uv, int uvStride, int verticesCount, Texture? texture = default)
	{
		if (!TryGetTexturePointer(texture, out var texturePtr))
		{
			// if we were given a non-null texture that has already been disposed, we fail early here
			return false;
		}

		return SDL_RenderGeometryRaw(mRenderer, texturePtr, xy, xyStride, colors, colorStride, uv, uvStride, verticesCount, indices: null, num_indices: 0, size_indices: 0);
	}

	/// <summary>
	/// Tries to draw a line to the current target
	/// </summary>
	/// <param name="x1">The X coordinate of the start point of the line</param>
	/// <param name="y1">The Y coordinate of the start point of the line</param>
	/// <param name="x2">The X coordinate of the end point of the line</param>
	/// <param name="y2">The Y coordinate of the end point of the line</param>
	/// <returns><c><see langword="true"/></c>, if the line was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)s</returns>
	/// <remarks>
	/// <para>
	/// The line is drawn with the color specified by <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>) and <see cref="DrawBlendMode"/> determines how the line is blended with the existing content of the target.
	/// </para>
	/// <para>
	/// Drawing the line works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderLine(float x1, float y1, float x2, float y2)
	{
		unsafe
		{
			return SDL_RenderLine(mRenderer, x1, y1, x2, y2);
		}
	}

	/// <summary>
	/// Tries to draw a series of connected lines to the current target
	/// </summary>
	/// <param name="points">The list of positions specifying the vertices of the connected lines</param>
	/// <returns><c><see langword="true"/></c> if the lines were drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The lines are drawn with the color specified by <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>) and <see cref="DrawBlendMode"/> determines how the lines are blended with the existing content of the target.
	/// </para>
	/// <para>
	/// Drawing the lines works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderLines(ReadOnlySpan<Point<float>> points)
	{
		unsafe
		{
			fixed (Point<float>* pointsPtr = points)
			{
				return SDL_RenderLines(mRenderer, pointsPtr, points.Length);
			}
		}
	}

	/// <summary>
	/// Tries to draw a point to the current target
	/// </summary>
	/// <param name="x">The X coordinate of the point to draw</param>
	/// <param name="y">The Y coordinate of the point to draw</param>
	/// <returns><c><see langword="true"/></c> if the point was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The point is drawn with the color specified by <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>) and <see cref="DrawBlendMode"/> determines how the point is blended with the existing content of the target.
	/// </para>
	/// <para>
	/// Drawing the point works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderPoint(float x, float y)
	{
		unsafe
		{
			return SDL_RenderPoint(mRenderer, x, y);
		}
	}

	/// <summary>
	/// Tries to draw multiple points to the current target
	/// </summary>
	/// <param name="points">The list of positions where the points should be drawn</param>
	/// <returns><c><see langword="true"/></c> if the points were drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// /// <remarks>
	/// <para>
	/// The points are drawn with the color specified by <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>) and <see cref="DrawBlendMode"/> determines how the points are blended with the existing content of the target.
	/// </para>
	/// <para>
	/// Drawing the points works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderPoints(ReadOnlySpan<Point<float>> points)
	{
		unsafe
		{
			fixed (Point<float>* pointsPtr = points)
			{
				return SDL_RenderPoints(mRenderer, pointsPtr, points.Length);
			}
		}
	}

	/// <summary>
	/// Tries to update the screen with any rendering performed since the previous call
	/// </summary>
	/// <returns><c><see langword="true"/></c> if the screen was updated successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// SDL's rendering API operates on a backbuffer.
	/// E.g., performing a rendering operation such as <see cref="TryRenderLine(float, float, float, float)"/> does not directly draw a line on the screen, but rather updates the backbuffer.
	/// That means that you should compose the entire scene in the backbuffer and then present the composed backbuffer to the screen as a complete picture.
	/// </para>
	/// <para>
	/// When using SDL's rendering API, once per frame, do all drawing intended for the frame, and then call this method once to present the final drawing to the user.
	/// </para>
	/// <para>
	/// The backbuffer should be considered invalidated after each call to this method; do not assume that previous contents will exist between frames.
	/// It is strongly recommended to initialize the backbuffer before starting each new frame's drawing by calling <see cref="TryClear"/>. Even if you plan to overwrite every pixel.
	/// </para>
	/// <para>
	/// Please note, that in case of rendering to a texture target (<see cref="Target"/> is non-<see langword="null"/>), there is <em>no need</em> to call <see cref="TryRenderPresent"/> and in fact, it <em>shouldn't</em> be called.
	/// You are only required to change back the rendering target to default via <see cref="ResetTarget"/> or setting <see cref="Target"/> to <see langword="null"/> afterwards, as textures by themselves do not have a concept of backbuffers.
	/// Calling <see cref="TryRenderPresent"/> while rendering to a texture will fail.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderPresent()
	{
		unsafe
		{
			return SDL_RenderPresent(mRenderer);
		}
	}

	/// <summary>
	/// Tries to draw a rectangle to an area of the current target
	/// </summary>
	/// <param name="rect">The area of the current target to draw the rectangle in</param>
	/// <returns><c><see langword="true"/></c>, if the rectangle was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The rectangle is drawn with the color specified by <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>) and <see cref="DrawBlendMode"/> determines how the rectangle is blended with the existing content of the target.
	/// </para>
	/// <para>
	/// Drawing the rectangle works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderRect(in Rect<float> rect)
	{
		unsafe
		{
			fixed (Rect<float>* rectPtr = &rect)
			{
				return SDL_RenderRect(mRenderer, rectPtr);
			}
		}
	}

	/// <summary>
	/// Tries to draw a rectangle to the entirety of the current target
	/// </summary>
	/// <returns><c><see langword="true"/></c>, if the rectangle was drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The rectangle is drawn with the color specified by <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>) and <see cref="DrawBlendMode"/> determines how the rectangle is blended with the existing content of the target.
	/// </para>
	/// <para>
	/// Drawing the rectangle works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderRect()
	{
		unsafe
		{
			return SDL_RenderRect(mRenderer, rect: null);
		}
	}

	/// <summary>
	/// Tries to draw multiple rectangles to areas of the current target
	/// </summary>
	/// <param name="rects">The list of areas of the current target to draw the rectangles in</param>
	/// <returns><c><see langword="true"/></c>, if the rectangles were drawn successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The rectangles are drawn with the color specified by <see cref="DrawColor"/> (or <see cref="DrawColorFloat"/>) and <see cref="DrawBlendMode"/> determines how the rectangles are blended with the existing content of the target.
	/// </para>
	/// <para>
	/// Drawing the rectangles works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderRects(ReadOnlySpan<Rect<float>> rects)
	{
		unsafe
		{
			fixed (Rect<float>* rectsPtr = rects)
			{
				return SDL_RenderRects(mRenderer, rectsPtr, rects.Length);
			}
		}
	}

	/// <summary>
	/// Tries to copy a portion of a texture to an area of the current target
	/// </summary>
	/// <param name="destinationRect">The area of the current target to copy the texture to</param>
	/// <param name="texture">The texture to copy</param>
	/// <param name="sourceRect">The area of the texture to copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTexture(in Rect<float> destinationRect, Texture texture, in Rect<float> sourceRect)
	{
		unsafe
		{
			fixed (Rect<float>* dstrect = &destinationRect, srcrect = &sourceRect)
			{
				return SDL_RenderTexture(mRenderer, texture is not null ? texture.Pointer : null, srcrect, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to copy the entirety of a texture to an area of the current target
	/// </summary>
	/// <param name="destinationRect">The area of the current target to copy the texture to</param>
	/// <param name="texture">The texture to copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTexture(in Rect<float> destinationRect, Texture texture)
	{
		unsafe
		{
			fixed (Rect<float>* dstrect = &destinationRect)
			{
				return SDL_RenderTexture(mRenderer, texture is not null ? texture.Pointer : null, srcrect: null, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to copy a portion of a texture to entirety of the current target
	/// </summary>
	/// <param name="texture">The texture to copy</param>
	/// <param name="sourceRect">The area of the texture to copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTexture(Texture texture, in Rect<float> sourceRect)
	{
		unsafe
		{
			fixed (Rect<float>* srcrect = &sourceRect)
			{
				return SDL_RenderTexture(mRenderer, texture is not null ? texture.Pointer : null, srcrect, dstrect: null);
			}
		}
	}

	/// <summary>
	/// Tries to copy the entirety of a texture to entirety of the current target
	/// </summary>
	/// <param name="texture">The texture to copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTexture(Texture texture)
	{
		unsafe
		{
			return SDL_RenderTexture(mRenderer, texture is not null ? texture.Pointer : null, srcrect: null, dstrect: null);
		}
	}

	/// <summary>
	/// Tries to scalingly copy a portion of a texture to an area of the current target using the 9-grid algorithm
	/// </summary>
	/// <param name="destinationRect">The area of the current target to copy the texture to, in a 9-grid manner</param>
	/// <param name="texture">The texture to copy</param>
	/// <param name="sourceRect">The area of the texture to copy, in a 9-grid manner</param>
	/// <param name="leftWidth">The width, in pixels, of the left corners in <paramref name="sourceRect"/></param>
	/// <param name="rightWidth">The width, in pixels, of the right corners in <paramref name="sourceRect"/></param>
	/// <param name="topHeight">The height, in pixels, of the top corners in <paramref name="sourceRect"/></param>
	/// <param name="bottomHeight">The height, in pixels, of the bottom corners in <paramref name="sourceRect"/></param>
	/// <param name="scale">The scale to use to transform the corners of <paramref name="sourceRect"/> into the corners of <paramref name="destinationRect"/>, or <c>0</c> for an unscaled copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The pixels in the texture are split into a 3⨯3 grid, using the different corner sizes for each corner, and the sides and center making up the remaining pixels.
	/// The corners are then scaled using <paramref name="scale"/> and fit into the corners of the <paramref name="destinationRect"/>.
	/// The sides and center are then stretched into place to cover the remaining portion of the <paramref name="destinationRect"/>.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTexture9Grid(in Rect<float> destinationRect, Texture texture, in Rect<float> sourceRect, float leftWidth, float rightWidth, float topHeight, float bottomHeight, float scale)
	{
		unsafe
		{
			fixed (Rect<float>* dstrect = &destinationRect, srcrect = &sourceRect)
			{
				return SDL_RenderTexture9Grid(mRenderer, texture is not null ? texture.Pointer : null, srcrect, leftWidth, rightWidth, topHeight, bottomHeight, scale, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to scalingly copy the entirety of a texture to an area of the current target using the 9-grid algorithm
	/// </summary>
	/// <param name="destinationRect">The area of the current target to copy the texture to, in a 9-grid manner</param>
	/// <param name="texture">The texture to copy</param>
	/// <param name="leftWidth">The width, in pixels, of the left corners of the <paramref name="texture"/></param>
	/// <param name="rightWidth">The width, in pixels, of the right corners of the <paramref name="texture"/></param>
	/// <param name="topHeight">The height, in pixels, of the top corners of the <paramref name="texture"/></param>
	/// <param name="bottomHeight">The height, in pixels, of the bottom corners of the <paramref name="texture"/></param>
	/// <param name="scale">The scale to use to transform the corners of the <paramref name="texture"/> into the corners of <paramref name="destinationRect"/>, or <c>0</c> for an unscaled copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The pixels in the texture are split into a 3⨯3 grid, using the different corner sizes for each corner, and the sides and center making up the remaining pixels.
	/// The corners are then scaled using <paramref name="scale"/> and fit into the corners of the <paramref name="destinationRect"/>.
	/// The sides and center are then stretched into place to cover the remaining portion of the <paramref name="destinationRect"/>.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTexture9Grid(in Rect<float> destinationRect, Texture texture, float leftWidth, float rightWidth, float topHeight, float bottomHeight, float scale)
	{
		unsafe
		{
			fixed (Rect<float>* dstrect = &destinationRect)
			{
				return SDL_RenderTexture9Grid(mRenderer, texture is not null ? texture.Pointer : null, srcrect: null, leftWidth, rightWidth, topHeight, bottomHeight, scale, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to scalingly copy a portion of a texture to the entirety of the current target using the 9-grid algorithm
	/// </summary>
	/// <param name="texture">The texture to copy</param>
	/// <param name="sourceRect">The area of the texture to copy, in a 9-grid manner</param>
	/// <param name="leftWidth">The width, in pixels, of the left corners in <paramref name="sourceRect"/></param>
	/// <param name="rightWidth">The width, in pixels, of the right corners in <paramref name="sourceRect"/></param>
	/// <param name="topHeight">The height, in pixels, of the top corners in <paramref name="sourceRect"/></param>
	/// <param name="bottomHeight">The height, in pixels, of the bottom corners in <paramref name="sourceRect"/></param>
	/// <param name="scale">The scale to use to transform the corners of <paramref name="sourceRect"/> into the corners of the current target, or <c>0</c> for an unscaled copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The pixels in the texture are split into a 3⨯3 grid, using the different corner sizes for each corner, and the sides and center making up the remaining pixels.
	/// The corners are then scaled using <paramref name="scale"/> and fit into the corners of the current target.
	/// The sides and center are then stretched into place to cover the remaining portion of the current target.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTexture9Grid(Texture texture, in Rect<float> sourceRect, float leftWidth, float rightWidth, float topHeight, float bottomHeight, float scale)
	{
		unsafe
		{
			fixed (Rect<float>* srcrect = &sourceRect)
			{
				return SDL_RenderTexture9Grid(mRenderer, texture is not null ? texture.Pointer : null, srcrect, leftWidth, rightWidth, topHeight, bottomHeight, scale, dstrect: null);
			}
		}
	}

	/// <summary>
	/// Tries to scalingly copy a portion of a texture to the entirety of the current target using the 9-grid algorithm
	/// </summary>
	/// <param name="texture">The texture to copy</param>
	/// <param name="leftWidth">The width, in pixels, of the left corners of the <paramref name="texture"/></param>
	/// <param name="rightWidth">The width, in pixels, of the right corners of the <paramref name="texture"/></param>
	/// <param name="topHeight">The height, in pixels, of the top corners of the <paramref name="texture"/></param>
	/// <param name="bottomHeight">The height, in pixels, of the bottom corners of the <paramref name="texture"/></param>
	/// <param name="scale">The scale to use to transform the corners of the <paramref name="texture"/> into the corners of the current target, or <c>0</c> for an unscaled copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The pixels in the texture are split into a 3⨯3 grid, using the different corner sizes for each corner, and the sides and center making up the remaining pixels.
	/// The corners are then scaled using <paramref name="scale"/> and fit into the corners of the current target.
	/// The sides and center are then stretched into place to cover the remaining portion of the current target.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTexture9Grid(Texture texture, float leftWidth, float rightWidth, float topHeight, float bottomHeight, float scale)
	{
		unsafe
		{
			return SDL_RenderTexture9Grid(mRenderer, texture is not null ? texture.Pointer : null, srcrect: null, leftWidth, rightWidth, topHeight, bottomHeight, scale, dstrect: null);
		}
	}

#if SDL3_4_0_OR_GREATER

	/// <summary>
	/// Tries to scalingly copy a portion of a texture to an area of the current target using the 9-grid algorithm with tiling for the borders and the center
	/// </summary>
	/// <param name="destinationRect">The area of the current target to copy the texture to, in a 9-grid manner</param>
	/// <param name="texture">The texture to copy</param>
	/// <param name="sourceRect">The area of the texture to copy, in a 9-grid manner</param>
	/// <param name="leftWidth">The width, in pixels, of the left corners in <paramref name="sourceRect"/></param>
	/// <param name="rightWidth">The width, in pixels, of the right corners in <paramref name="sourceRect"/></param>
	/// <param name="topHeight">The height, in pixels, of the top corners in <paramref name="sourceRect"/></param>
	/// <param name="bottomHeight">The height, in pixels, of the bottom corners in <paramref name="sourceRect"/></param>
	/// <param name="scale">The scale to use to transform the corners of <paramref name="sourceRect"/> into the corners of <paramref name="destinationRect"/>, or <c>0</c> for an unscaled copy</param>
	/// <param name="tileScale">The scale to use to transform the borders and the center of <paramref name="sourceRect"/> into the border and the center of <paramref name="destinationRect"/>, or <c>1</c> for an unscaled copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The pixels in the texture are split into a 3⨯3 grid, using the different corner sizes for each corner, and the sides and center making up the remaining pixels.
	/// The corners are then scaled using <paramref name="scale"/> and fit into the corners of the <paramref name="destinationRect"/>.
	/// The sides and center are then <em>tiled</em> into place to cover the remaining portion of the <paramref name="destinationRect"/>.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTexture9GridTiled(in Rect<float> destinationRect, Texture texture, in Rect<float> sourceRect, float leftWidth, float rightWidth, float topHeight, float bottomHeight, float scale, float tileScale)
	{
		unsafe
		{
			fixed (Rect<float>* dstrect = &destinationRect, srcrect = &sourceRect)
			{
				return SDL_RenderTexture9GridTiled(mRenderer, texture is not null ? texture.Pointer : null, srcrect, leftWidth, rightWidth, topHeight, bottomHeight, scale, dstrect, tileScale);
			}
		}
	}

	/// <summary>
	/// Tries to scalingly copy the entirety of a texture to an area of the current target using the 9-grid algorithm with tiling for the borders and the center
	/// </summary>
	/// <param name="destinationRect">The area of the current target to copy the texture to, in a 9-grid manner</param>
	/// <param name="texture">The texture to copy</param>
	/// <param name="leftWidth">The width, in pixels, of the left corners of the <paramref name="texture"/></param>
	/// <param name="rightWidth">The width, in pixels, of the right corners of the <paramref name="texture"/></param>
	/// <param name="topHeight">The height, in pixels, of the top corners of the <paramref name="texture"/></param>
	/// <param name="bottomHeight">The height, in pixels, of the bottom corners of the <paramref name="texture"/></param>
	/// <param name="scale">The scale to use to transform the corners of the <paramref name="texture"/> into the corners of <paramref name="destinationRect"/>, or <c>0</c> for an unscaled copy</param>
	/// <param name="tileScale">The scale to use to transform the borders and the center of the <paramref name="texture"/> into the border and the center of <paramref name="destinationRect"/>, or <c>1</c> for an unscaled copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The pixels in the texture are split into a 3⨯3 grid, using the different corner sizes for each corner, and the sides and center making up the remaining pixels.
	/// The corners are then scaled using <paramref name="scale"/> and fit into the corners of the <paramref name="destinationRect"/>.
	/// The sides and center are then <em>tiled</em> into place to cover the remaining portion of the <paramref name="destinationRect"/>.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTexture9GridTiled(in Rect<float> destinationRect, Texture texture, float leftWidth, float rightWidth, float topHeight, float bottomHeight, float scale, float tileScale)
	{
		unsafe
		{
			fixed (Rect<float>* dstrect = &destinationRect)
			{
				return SDL_RenderTexture9GridTiled(mRenderer, texture is not null ? texture.Pointer : null, srcrect: null, leftWidth, rightWidth, topHeight, bottomHeight, scale, dstrect, tileScale);
			}
		}
	}

	/// <summary>
	/// Tries to scalingly copy a portion of a texture to the entirety of the current target using the 9-grid algorithm with tiling for the borders and the center
	/// </summary>
	/// <param name="texture">The texture to copy</param>
	/// <param name="sourceRect">The area of the texture to copy, in a 9-grid manner</param>
	/// <param name="leftWidth">The width, in pixels, of the left corners in <paramref name="sourceRect"/></param>
	/// <param name="rightWidth">The width, in pixels, of the right corners in <paramref name="sourceRect"/></param>
	/// <param name="topHeight">The height, in pixels, of the top corners in <paramref name="sourceRect"/></param>
	/// <param name="bottomHeight">The height, in pixels, of the bottom corners in <paramref name="sourceRect"/></param>
	/// <param name="scale">The scale to use to transform the corners of <paramref name="sourceRect"/> into the corners of the current target, or <c>0</c> for an unscaled copy</param>
	/// <param name="tileScale">The scale to use to transform the borders and the center of <paramref name="sourceRect"/> into the border and the center of the current target, or <c>1</c> for an unscaled copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The pixels in the texture are split into a 3⨯3 grid, using the different corner sizes for each corner, and the sides and center making up the remaining pixels.
	/// The corners are then scaled using <paramref name="scale"/> and fit into the corners of the current target.
	/// The sides and center are then <em>tiled</em> into place to cover the remaining portion of the current target.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTexture9GridTiled(Texture texture, in Rect<float> sourceRect, float leftWidth, float rightWidth, float topHeight, float bottomHeight, float scale, float tileScale)
	{
		unsafe
		{
			fixed (Rect<float>* srcrect = &sourceRect)
			{
				return SDL_RenderTexture9GridTiled(mRenderer, texture is not null ? texture.Pointer : null, srcrect, leftWidth, rightWidth, topHeight, bottomHeight, scale, dstrect: null, tileScale);
			}
		}
	}

	/// <summary>
	/// Tries to scalingly copy the entirety of a texture to the entirety of the current target using the 9-grid algorithm with tiling for the borders and the center
	/// </summary>
	/// <param name="texture">The texture to copy</param>
	/// <param name="leftWidth">The width, in pixels, of the left corners of the <paramref name="texture"/></param>
	/// <param name="rightWidth">The width, in pixels, of the right corners of the <paramref name="texture"/></param>
	/// <param name="topHeight">The height, in pixels, of the top corners of the <paramref name="texture"/></param>
	/// <param name="bottomHeight">The height, in pixels, of the bottom corners of the <paramref name="texture"/></param>
	/// <param name="scale">The scale to use to transform the corners of the <paramref name="texture"/> into the corners of the current target, or <c>0</c> for an unscaled copy</param>
	/// <param name="tileScale">The scale to use to transform the borders and the center of the <paramref name="texture"/> into the border and the center of the current target, or <c>1</c> for an unscaled copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The pixels in the texture are split into a 3⨯3 grid, using the different corner sizes for each corner, and the sides and center making up the remaining pixels.
	/// The corners are then scaled using <paramref name="scale"/> and fit into the corners of the current target.
	/// The sides and center are then <em>tiled</em> into place to cover the remaining portion of the current target.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTexture9GridTiled(Texture texture, float leftWidth, float rightWidth, float topHeight, float bottomHeight, float scale, float tileScale)
	{
		unsafe
		{
			return SDL_RenderTexture9GridTiled(mRenderer, texture is not null ? texture.Pointer : null, srcrect: null, leftWidth, rightWidth, topHeight, bottomHeight, scale, dstrect: null, tileScale);
		}
	}

#endif

	/// <summary>
	/// Tries to copy a portion of a texture to the current target with a specified affine transformation
	/// </summary>
	/// <param name="destinationOrigin">A point specifying where the top-left corner of <paramref name="sourceRect"/> should be mapped to on the current target, or <c><see langword="null"/></c> to use the current target's origin</param>
	/// <param name="destinationRight">A point specifying where the top-right corner of <paramref name="sourceRect"/> should be mapped to on the current target, or <c><see langword="null"/></c> to use the current target's top-right corner</param>
	/// <param name="destinationDown">A point specifying where the bottom-left corner of <paramref name="sourceRect"/> should be mapped to on the current target, or <c><see langword="null"/></c> to use the current target's bottom-left corner</param>
	/// <param name="texture">The texture to copy</param>
	/// <param name="sourceRect"></param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTextureAffine(in Point<float>? destinationOrigin, in Point<float>? destinationRight, in Point<float>? destinationDown, Texture texture, in Rect<float> sourceRect)
	{
		unsafe
		{
			fixed (Point<float>* origin = &Nullable.GetValueRefOrDefaultRef(in destinationOrigin), right = &Nullable.GetValueRefOrDefaultRef(in destinationRight), down = &Nullable.GetValueRefOrDefaultRef(in destinationDown))
			fixed (Rect<float>* srcrect = &sourceRect)
			{
				return SDL_RenderTextureAffine(mRenderer, texture is not null ? texture.Pointer : null, srcrect, destinationOrigin.HasValue ? origin : null, destinationRight.HasValue ? right : null, destinationDown.HasValue ? down : null);
			}
		}
	}

	/// <summary>
	/// Tries to copy the entirety of a texture to the current target with a specified affine transformation
	/// </summary>
	/// <param name="destinationOrigin">A point specifying where the top-left corner of the <paramref name="texture"/> should be mapped to on the current target, or <c><see langword="null"/></c> to use the current target's origin</param>
	/// <param name="destinationRight">A point specifying where the top-right corner of the <paramref name="texture"/> should be mapped to on the current target, or <c><see langword="null"/></c> to use the current target's top-right corner</param>
	/// <param name="destinationDown">A point specifying where the bottom-left corner of the <paramref name="texture"/> should be mapped to on the current target, or <c><see langword="null"/></c> to use the current target's bottom-left corner</param>
	/// <param name="texture">The texture to copy</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTextureAffine(in Point<float>? destinationOrigin, in Point<float>? destinationRight, in Point<float>? destinationDown, Texture texture)
	{
		unsafe
		{
			fixed (Point<float>* origin = &Nullable.GetValueRefOrDefaultRef(in destinationOrigin), right = &Nullable.GetValueRefOrDefaultRef(in destinationRight), down = &Nullable.GetValueRefOrDefaultRef(in destinationDown))
			{
				return SDL_RenderTextureAffine(mRenderer, texture is not null ? texture.Pointer : null, srcrect: null, destinationOrigin.HasValue ? origin : null, destinationRight.HasValue ? right : null, destinationDown.HasValue ? down : null);
			}
		}
	}

	/// <summary>
	/// Tries to copy a portion of a texture to an area of the current target with a specified rotation and flipping
	/// </summary>
	/// <param name="destinationRect">The area of the current target to copy the texture to</param>
	/// <param name="texture">The texture to copy</param>
	/// <param name="sourceRect">The area of the texture to copy</param>
	/// <param name="angle">The angle, in degrees, to rotate the texture clockwise around the center point</param>
	/// <param name="centerPoint">An optional point specifying the center of rotation, or <c><see langword="null"/></c> to rotate around the center of <paramref name="destinationRect"/></param>
	/// <param name="flip">An optional flipping direction to flip the texture in addition to rotating it</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// You can also use this method to just render a flipped texture by specifying the <paramref name="angle"/> as <c>0</c> and leaving the <paramref name="centerPoint"/> as unspecified.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTextureRotated(in Rect<float> destinationRect, Texture texture, in Rect<float> sourceRect, double angle, in Point<float>? centerPoint, FlipMode flip)
	{
		unsafe
		{
			fixed (Rect<float>* dstrect = &destinationRect, srcrect = &sourceRect)
			fixed (Point<float>* center = &Nullable.GetValueRefOrDefaultRef(in centerPoint))
			{
				return SDL_RenderTextureRotated(mRenderer, texture is not null ? texture.Pointer : null, srcrect, dstrect, angle, centerPoint.HasValue ? center : null, flip);
			}
		}
	}

	/// <summary>
	/// Tries to copy the entirety of a texture to an area of the current target with a specified rotation and flipping
	/// </summary>
	/// <param name="destinationRect">The area of the current target to copy the texture to</param>
	/// <param name="texture">The texture to copy</param>
	/// <param name="angle">The angle, in degrees, to rotate the texture clockwise around the center point</param>
	/// <param name="centerPoint">An optional point specifying the center of rotation, or <c><see langword="null"/></c> to rotate around the center of <paramref name="destinationRect"/></param>
	/// <param name="flip">An optional flipping direction to flip the texture in addition to rotating it</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// You can also use this method to just render a flipped texture by specifying the <paramref name="angle"/> as <c>0</c> and leaving the <paramref name="centerPoint"/> as unspecified.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTextureRotated(in Rect<float> destinationRect, Texture texture, double angle, in Point<float>? centerPoint, FlipMode flip)
	{
		unsafe
		{
			fixed (Rect<float>* dstrect = &destinationRect)
			fixed (Point<float>* center = &Nullable.GetValueRefOrDefaultRef(in centerPoint))
			{
				return SDL_RenderTextureRotated(mRenderer, texture is not null ? texture.Pointer : null, srcrect: null, dstrect, angle, centerPoint.HasValue ? center : null, flip);
			}
		}
	}

	/// <summary>
	/// Tries to copy a portion of a texture to the entirety of the current target with a specified rotation and flipping
	/// </summary>
	/// <param name="texture">The texture to copy</param>
	/// <param name="sourceRect">The area of the texture to copy</param>
	/// <param name="angle">The angle, in degrees, to rotate the texture clockwise around the center point</param>
	/// <param name="centerPoint">An optional point specifying the center of rotation, or <c><see langword="null"/></c> to rotate around the center of the current target</param>
	/// <param name="flip">An optional flipping direction to flip the texture in addition to rotating it</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// You can also use this method to just render a flipped texture by specifying the <paramref name="angle"/> as <c>0</c> and leaving the <paramref name="centerPoint"/> as unspecified.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTextureRotated(Texture texture, in Rect<float> sourceRect, double angle, in Point<float>? centerPoint, FlipMode flip)
	{
		unsafe
		{
			fixed (Rect<float>* srcrect = &sourceRect)
			fixed (Point<float>* center = &Nullable.GetValueRefOrDefaultRef(in centerPoint))
			{
				return SDL_RenderTextureRotated(mRenderer, texture is not null ? texture.Pointer : null, srcrect, dstrect: null, angle, centerPoint.HasValue ? center : null, flip);
			}
		}
	}

	/// <summary>
	/// Tries to copy the entirety of a texture to the entirety of the current target with a specified rotation and flipping
	/// </summary>
	/// <param name="texture">The texture to copy</param>
	/// <param name="angle">The angle, in degrees, to rotate the texture clockwise around the center point</param>
	/// <param name="centerPoint">An optional point specifying the center of rotation, or <c><see langword="null"/></c> to rotate around the center of the current target</param>
	/// <param name="flip">An optional flipping direction to flip the texture in addition to rotating it</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// You can also use this method to just render a flipped texture by specifying the <paramref name="angle"/> as <c>0</c> and leaving the <paramref name="centerPoint"/> as unspecified.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTextureRotated(Texture texture, double angle, in Point<float>? centerPoint, FlipMode flip)
	{
		unsafe
		{
			fixed (Point<float>* center = &Nullable.GetValueRefOrDefaultRef(in centerPoint))
			{
				return SDL_RenderTextureRotated(mRenderer, texture is not null ? texture.Pointer : null, srcrect: null, dstrect: null, angle, centerPoint.HasValue ? center : null, flip);
			}
		}
	}

	/// <summary>
	/// Tries to tilingly copy a portion of a texture to an area of the current target with a specified scale
	/// </summary>
	/// <param name="destinationRect">The area of the current target to copy the texture to</param>
	/// <param name="texture">The texture to copy</param>
	/// <param name="sourceRect">The area of the texture to copy</param>
	/// <param name="scale">The scale used to transform the <paramref name="sourceRect"/> into the <paramref name="destinationRect"/></param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="scale"/> is applied to the region of the texture specified by <paramref name="sourceRect"/>, e.g. 32⨯32 region with a scale of <c>2</c> would become a 64⨯64 region.
	/// That scaled region is then used and repeated as many times as needed to completely fill the region specified by <paramref name="destinationRect"/> on the current target.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTextureTiled(in Rect<float> destinationRect, Texture texture, in Rect<float> sourceRect, float scale)
	{
		unsafe
		{
			fixed (Rect<float>* dstrect = &destinationRect, srcrect = &sourceRect)
			{
				return SDL_RenderTextureTiled(mRenderer, texture is not null ? texture.Pointer : null, srcrect, scale, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to tilingly copy the entirety of a texture to an area of the current target with a specified scale
	/// </summary>
	/// <param name="destinationRect">The area of the current target to copy the texture to</param>
	/// <param name="texture">The texture to copy</param>
	/// <param name="scale">The scale used to transform the <paramref name="texture"/> into the <paramref name="destinationRect"/></param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="scale"/> is applied to the entire <paramref name="texture"/>, e.g. 32⨯32 texture with a scale of <c>2</c> would become a 64⨯64 region.
	/// That scaled region is then used and repeated as many times as needed to completely fill the region specified by <paramref name="destinationRect"/> on the current target.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTextureTiled(in Rect<float> destinationRect, Texture texture, float scale)
	{
		unsafe
		{
			fixed (Rect<float>* dstrect = &destinationRect)
			{
				return SDL_RenderTextureTiled(mRenderer, texture is not null ? texture.Pointer : null, srcrect: null, scale, dstrect);
			}
		}
	}

	/// <summary>
	/// Tries to tilingly copy a portion of a texture to the entirety of the current target with a specified scale
	/// </summary>
	/// <param name="texture">The texture to copy</param>
	/// <param name="sourceRect">The area of the texture to copy</param>
	/// <param name="scale">The scale used to transform the <paramref name="sourceRect"/> into the current target</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="scale"/> is applied to the region of the texture specified by <paramref name="sourceRect"/>, e.g. 32⨯32 region with a scale of <c>2</c> would become a 64⨯64 region.
	/// That scaled region is then used and repeated as many times as needed to completely fill the entire area of the current target.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTextureTiled(Texture texture, in Rect<float> sourceRect, float scale)
	{
		unsafe
		{
			fixed (Rect<float>* srcrect = &sourceRect)
			{
				return SDL_RenderTextureTiled(mRenderer, texture is not null ? texture.Pointer : null, srcrect, scale, dstrect: null);
			}
		}
	}

	/// <summary>
	/// Tries to tilingly copy the entirety of a texture to the entirety of the current target with a specified scale
	/// </summary>
	/// <param name="texture">The texture to copy</param>
	/// <param name="scale">The scale used to transform the <paramref name="texture"/> into the current target</param>
	/// <returns><c><see langword="true"/></c>, if the texture was rendered successfully; otherwise, <c>false</c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="scale"/> is applied to the entire <paramref name="texture"/>, e.g. 32⨯32 texture with a scale of <c>2</c> would become a 64⨯64 region.
	/// That scaled region is then used and repeated as many times as needed to completely fill the entire area of the current target.
	/// </para>
	/// <para>
	/// Only textures created with this renderer can be rendered with this method.
	/// </para>
	/// <para>
	/// Rendering the texture works with sub-pixel precision.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public bool TryRenderTextureTiled(Texture texture, float scale)
	{
		unsafe
		{
			return SDL_RenderTextureTiled(mRenderer, texture is not null ? texture.Pointer : null, srcrect: null, scale, dstrect: null);
		}
	}
}
