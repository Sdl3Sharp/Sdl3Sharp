#if SDL3_4_0_OR_GREATER

using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
#if SDL3_6_0_OR_GREATER
using Sdl3Sharp.Video.Gpu;
#endif
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Video.Rendering;

partial class GpuRenderState
{
	// opaque struct
	[StructLayout(LayoutKind.Sequential, Size = 0)]
	internal readonly struct SDL_GPURenderState;

	/// <summary>
	/// Create custom GPU render state
	/// </summary>
	/// <param name="renderer">The renderer to use</param>
	/// <param name="createInfo">A struct describing the GPU render state to create</param>
	/// <returns>Returns a custom GPU render state or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should be called on the thread that created the renderer.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateGPURenderState">SDL_CreateGPURenderState</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_GPURenderState* SDL_CreateGPURenderState(Renderer.SDL_Renderer* renderer, GpuRenderStateCreateInfo.SDL_GPURenderStateCreateInfo* createInfo);

	/// <summary>
	/// Destroy custom GPU render state
	/// </summary>
	/// <param name="state">The state to destroy</param>
	/// <remarks>
	/// <para>
	/// This function should be called on the thread that created the renderer.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_DestroyGPURenderState">SDL_DestroyGPURenderState</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_DestroyGPURenderState(SDL_GPURenderState* state);

	/// <summary>
	/// Set fragment shader uniform variables in a custom GPU render state
	/// </summary>
	/// <param name="state">The state to modify</param>
	/// <param name="slot_index">The fragment uniform slot to push data to</param>
	/// <param name="data">Client data to write</param>
	/// <param name="length">The length of the data to write</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The data is copied and will be pushed using <see href="https://wiki.libsdl.org/SDL3/SDL_PushGPUFragmentUniformData">SDL_PushGPUFragmentUniformData</see>() during draw call execution.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the renderer.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetGPURenderStateFragmentUniforms">SDL_SetGPURenderStateFragmentUniforms</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetGPURenderStateFragmentUniforms(SDL_GPURenderState* state, uint slot_index, void* data, uint length);

#if SDL3_6_0_OR_GREATER

	/// <summary>
	/// Sets sampler bindings variables in a custom GPU render state
	/// </summary>
	/// <param name="state">The state to modify</param>
	/// <param name="num_sampler_bindings">The number of additional fragment samplers to bind</param>
	/// <param name="sampler_bindings">Additional fragment samplers to bind</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The data is copied and will be binded using <see href="https://wiki.libsdl.org/SDL3/SDL_BindGPUFragmentSamplers">SDL_BindGPUFragmentSamplers</see>() during draw call execution.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the renderer.
	/// </para>
	/// </remarks>
	/// <seealso cref="https://wiki.libsdl.org/SDL3/SDL_SetGPURenderStateSamplerBindings">SDL_SetGPURenderStateSamplerBindings</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetGPURenderStateSamplerBindings(SDL_GPURenderState* state, int num_sampler_bindings, GpuTextureSamplerBinding.SDL_GPUTextureSamplerBinding* sampler_bindings);

	/// <summary>
	/// Sets storage buffers variables in a custom GPU render state
	/// </summary>
	/// <param name="state">The state to modify</param>
	/// <param name="num_storage_buffers">The number of storage buffers to bind</param>
	/// <param name="storage_buffers">Storage buffers to bind</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The data is copied and will be binded using <see href="https://wiki.libsdl.org/SDL3/SDL_BindGPUFragmentStorageBuffers">SDL_BindGPUFragmentStorageBuffers</see>() during draw call execution.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the renderer.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetGPURenderStateStorageTextures"></seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetGPURenderStateStorageBuffers(SDL_GPURenderState* state, int num_storage_buffers, GpuBuffer.SDL_GPUBuffer** storage_buffers);

	/// <summary>
	/// Sets storage textures variables in a custom GPU render state
	/// </summary>
	/// <param name="state">The state to modify</param>
	/// <param name="num_storage_textures">The number of storage textures to bind</param>
	/// <param name="storage_textures">Storage textures to bind</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The data is copied and will be binded using <see href="https://wiki.libsdl.org/SDL3/SDL_BindGPUFragmentStorageTextures">SDL_BindGPUFragmentStorageTextures</see>() during draw call execution.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the renderer.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetGPURenderStateStorageTextures">SDL_SetGPURenderStateStorageTextures</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetGPURenderStateStorageTextures(SDL_GPURenderState* state, int num_storage_textures, GpuTexture.SDL_GPUTexture** storage_textures);

#endif
}

#endif
