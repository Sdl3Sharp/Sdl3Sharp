#if SDL3_4_0_OR_GREATER

using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using Sdl3Sharp.Video.Gpu;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Rendering;

partial class RendererExtensions
{
	/// <summary>
	/// Return the GPU device used by a renderer
	/// </summary>
	/// <param name="renderer">The rendering context</param>
	/// <returns>Returns the GPU device used by the renderer, or NULL if the renderer is not a GPU renderer; <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetGPURendererDevice">SDL_GetGPURendererDevice</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial GpuDevice.SDL_GPUDevice* SDL_GetGPURendererDevice(Renderer.SDL_Renderer* renderer);

	/// <summary>
	/// Set custom GPU render state
	/// </summary>
	/// <param name="renderer">The renderer to use.</param>
	/// <param name="state">The state to to use, or NULL to clear custom GPU render state</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function sets custom GPU render state for subsequent draw calls. This allows using custom shaders with the GPU renderer.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the renderer.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetGPURenderState">SDL_SetGPURenderState</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetGPURenderState(Renderer.SDL_Renderer* renderer, RenderState.SDL_GPURenderState* state);
}

#endif
