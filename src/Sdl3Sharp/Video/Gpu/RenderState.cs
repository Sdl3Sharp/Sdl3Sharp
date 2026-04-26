#if SDL3_4_0_OR_GREATER

using Sdl3Sharp.Utilities;
using Sdl3Sharp.Video.Rendering;
using System;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents a custom GPU render state
/// </summary>
public sealed partial class RenderState : IDisposable
{
	private unsafe SDL_GPURenderState* mState;
	private RenderStateCreateInfo? mCreateInfo = null; // used to keep the managed RenderStateCreateInfo alive that was used to create this RenderState, if any
														  // this is necessary because the managed RenderStateCreateInfo in turn keeps to TextureSamplerBinding instances,
														  // the GpuTexture instances, and the GpuBuffer instances alive that it contains
														  // and the native SDL_GPURenderState may reference their underlying native resources at some point

	internal unsafe RenderState(SDL_GPURenderState* state, RenderStateCreateInfo? createInfo)
	{
		mState = state;
		mCreateInfo = createInfo;
	}

	internal unsafe SDL_GPURenderState* Pointer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mState; }

	/// <inheritdoc/>
	~RenderState() => DisposeImpl();

	/// <inheritdoc/>
	/// <remarks>
	/// <para>
	/// This method should be called on the thread that created the <see cref="Renderer{TDriver}">Renderer</see>&lt;<see cref="Rendering.Drivers.Gpu">Gpu</see>&gt;.
	/// </para>
	/// </remarks>
	public void Dispose()
	{
		DisposeImpl();
		GC.SuppressFinalize(this);
	}

	private unsafe void DisposeImpl()
	{
		if (mState is not null)
		{
			unsafe
			{
				SDL_DestroyGPURenderState(mState);
				mState = null;
			}
		}

		mCreateInfo = null;

#if SDL3_6_0_OR_GREATER

		mSamplerBindings = null;
		mStorageBuffers = null;
		mStorageTextures = null;

#endif
	}

	/// <summary>
	/// Tries to set fragment shader uniform variables in the GPU render state
	/// </summary>
	/// <param name="slotIndex">The fragment uniform slot index to push data to</param>
	/// <param name="data">The client data to write</param>
	/// <returns>
	/// <c><see langword="true"/></c>, if the fragment shader uniform variables were successfully set; otherwise, <c><see langword="false"/> (check <see cref="Error.TryGet(out string?)"/> for more information)</c>
	/// </returns>
	/// <remarks>
	/// <para>
	/// This given <paramref name="data"/> is copied and will be pushed using <see cref="SDL_PushGPUFragmentUniformData"/> during draw call execution.
	/// </para>
	/// <para>
	/// This method should be called on the thread that created the <see cref="Renderer{TDriver}">Renderer</see>&lt;<see cref="Rendering.Drivers.Gpu">Gpu</see>&gt;.
	/// </para>
	/// </remarks>
	public bool TrySetFragmentUniforms(uint slotIndex, NativeMemory data)
	{
		unsafe
		{
			if (!data.IsValid)
			{
				return false;
			}

			if (data.Length is > uint.MaxValue)
			{
				data = ((NativeMemory<byte>)data).Slice(0, uint.MaxValue);
			}

			return SDL_SetGPURenderStateFragmentUniforms(mState, slotIndex, data.RawPointer, unchecked((uint)data.Length));
		}
	}

	/// <summary>
	/// Tries to set fragment shader uniform variables in the GPU render state
	/// </summary>
	/// <param name="slotIndex">The fragment uniform slot index to push data to</param>
	/// <param name="data">The client data to write</param>
	/// <returns>
	/// <c><see langword="true"/></c>, if the fragment shader uniform variables were successfully set; otherwise, <c><see langword="false"/> (check <see cref="Error.TryGet(out string?)"/> for more information)</c>
	/// </returns>
	/// <remarks>
	/// <para>
	/// This given <paramref name="data"/> is copied and will be pushed using <see cref="SDL_PushGPUFragmentUniformData"/> during draw call execution.
	/// </para>
	/// <para>
	/// This method should be called on the thread that created the <see cref="Renderer{TDriver}">Renderer</see>&lt;<see cref="Rendering.Drivers.Gpu">Gpu</see>&gt;.
	/// </para>
	/// </remarks>
	public bool TrySetFragmentUniforms(uint slotIndex, ReadOnlySpan<byte> data)
	{
		unsafe
		{
			fixed (byte* dataPtr = data)
			{
				return SDL_SetGPURenderStateFragmentUniforms(mState, slotIndex, dataPtr, unchecked((uint)data.Length));
			}
		}
	}

	/// <summary>
	/// Tries to set fragment shader uniform variables in the GPU render state
	/// </summary>
	/// <param name="slotIndex">The fragment uniform slot index to push data to</param>
	/// <param name="data">The client data to write</param>
	/// <param name="lenght">The length of the data to write</param>
	/// <returns>
	/// <c><see langword="true"/></c>, if the fragment shader uniform variables were successfully set; otherwise, <c><see langword="false"/> (check <see cref="Error.TryGet(out string?)"/> for more information)</c>
	/// </returns>
	/// <remarks>
	/// <para>
	/// This given <paramref name="data"/> is copied and will be pushed using <see cref="SDL_PushGPUFragmentUniformData"/> during draw call execution.
	/// </para>
	/// <para>
	/// This method should be called on the thread that created the <see cref="Renderer{TDriver}">Renderer</see>&lt;<see cref="Rendering.Drivers.Gpu">Gpu</see>&gt;.
	/// </para>
	/// </remarks>
	public unsafe bool TrySetFragmentUniforms(uint slotIndex, void* data, uint lenght)
	{
		return SDL_SetGPURenderStateFragmentUniforms(mState, slotIndex, data, lenght);
	}

#if SDL3_6_0_OR_GREATER

	private GpuTextureSamplerBinding[]? mSamplerBindings = null; // used to keep the managed GpuTextureSamplerBinding instances alive that were used to set the sampler bindings of this RenderState, if any
																 // this is necessary because the native SDL_GPURenderState may reference their underlying native resources at some point

	/// <summary>
	/// Tries to set sampler bindings in the GPU render state
	/// </summary>
	/// <param name="samplerBindings">The additional sampler bindings to bind</param>
	/// <returns><c><see langword="true"/></c> if the sampler bindings were successfully set; otherwise, <c><see langword="false"/> (check <see cref="Error.TryGet(out string?)"/> for more information)</c></returns>
	/// <remarks>
	/// <para>
	/// The given <paramref name="samplerBindings"/> are copied and will be binded using <see cref="SDL_BindGPUFragmentSamplers"/> during draw call execution.
	/// </para>
	/// <para>
	/// This method should be called on the thread that created the <see cref="Renderer{TDriver}">Renderer</see>&lt;<see cref="Drivers.Gpu">Gpu</see>&gt;.
	/// </para>
	/// </remarks>
	public bool TrySetSamplerBindings(ReadOnlySpan<GpuTextureSamplerBinding> samplerBindings)
	{
		const int stackAllocThreshold = 8;

		unsafe
		{
			if (samplerBindings.IsEmpty)
			{
				// There's a reason for passing in a non-null pointer to a dummy SDL_GPUTextureSamplerBinding on stack,
				// even though we pass in 0 for num_sampler_bindings:
				// SDL_SetGPURenderStateSamplerBindings for some reason doesn't check for passed in null pointers
				// and calls SDL_memcpy unconditionally, even if num_sampler_bindings is 0,
				// and SDL_memcpy fails if it receives a null pointer for either the source or the destination,
				// which would let SDL_SetGPURenderStateSamplerBindings fail in turn.
				// But it's totally valid to set the sampler bindings to an empty array.

				Unsafe.SkipInit(out GpuTextureSamplerBinding.SDL_GPUTextureSamplerBinding dummy); // No worries about an uninitialized value, because num_sampler_bindings is 0, the dummy value will never actually be read from

				if (!(bool)SDL_SetGPURenderStateSamplerBindings(mState, num_sampler_bindings: 0, sampler_bindings: &dummy))
				{
					return false;
				}

				mSamplerBindings = null;

				return true;
			}

			GpuTextureSamplerBinding.SDL_GPUTextureSamplerBinding* samplerBindingsOnHeap, samplerBindingsPtr;

			if (samplerBindings.Length is <= stackAllocThreshold)
			{
				var samplerBindingsOnStack = stackalloc GpuTextureSamplerBinding.SDL_GPUTextureSamplerBinding[samplerBindings.Length];
				samplerBindingsOnHeap = null;
				samplerBindingsPtr = samplerBindingsOnStack;
			}
			else
			{
				samplerBindingsOnHeap = (GpuTextureSamplerBinding.SDL_GPUTextureSamplerBinding*)NativeMemory.Malloc(unchecked((nuint)samplerBindings.Length * (nuint)Unsafe.SizeOf<GpuTextureSamplerBinding.SDL_GPUTextureSamplerBinding>()));
				samplerBindingsPtr = samplerBindingsOnHeap;
			}

			try
			{
				var newSamplerBindings = GC.AllocateUninitializedArray<GpuTextureSamplerBinding>(samplerBindings.Length);

				for (var index = 0; index < samplerBindings.Length; index++)
				{
					ref readonly var samplerBinding = ref samplerBindings[index];

					newSamplerBindings[index] = samplerBinding;
					samplerBindingsPtr[index] = samplerBinding.ToNative();
				}

				if (!(bool)SDL_SetGPURenderStateSamplerBindings(mState, num_sampler_bindings: samplerBindings.Length, sampler_bindings: samplerBindingsPtr))
				{
					return false;
				}

				mSamplerBindings = newSamplerBindings; 

				return true;
			}
			finally
			{
				NativeMemory.Free(samplerBindingsOnHeap); // It's fine if samplerBindingsOnHeap is null, NativeMemory.Free does nothing in that case
			}
		}
	}

	private GpuBuffer[]? mStorageBuffers = null; // used to keep the managed GpuBuffer instances alive that were used to set the storage buffers of this RenderState, if any
												 // this is necessary because the native SDL_GPURenderState may reference their underlying native resources at some point

	/// <summary>
	/// Tries to set storage buffers in the GPU render state
	/// </summary>
	/// <param name="storageBuffers">The storage buffers to bind</param>
	/// <returns><c><see langword="true"/></c> if the storage buffers were successfully set; otherwise, <c><see langword="false"/> (check <see cref="Error.TryGet(out string?)"/> for more information)</c></returns>
	/// <remarks>
	/// <para>
	/// The given <paramref name="storageBuffers"/> are copied and will be binded using <see cref="SDL_BindGPUFragmentStorageBuffers"/> during draw call execution.
	/// </para>
	/// <para>
	/// This method should be called on the thread that created the <see cref="Renderer{TDriver}">Renderer</see>&lt;<see cref="Drivers.Gpu">Gpu</see>&gt;.
	/// </para>
	/// </remarks>
	public bool TrySetStorageBuffers(ReadOnlySpan<GpuBuffer> storageBuffers)
	{
		const int stackAllocThreshold = 16;

		unsafe
		{
			if (storageBuffers.IsEmpty)
			{
				// There's a reason for passing in a non-null pointer to a dummy SDL_SetGPURenderStateStorageBuffers on stack,
				// even though we pass in 0 for num_storage_buffers:
				// SDL_SetGPURenderStateStorageBuffers for some reason doesn't check for passed in null pointers
				// and calls SDL_memcpy unconditionally, even if num_storage_buffers is 0,
				// and SDL_memcpy fails if it receives a null pointer for either the source or the destination,
				// which would let SDL_SetGPURenderStateStorageBuffers fail in turn.
				// But it's totally valid to set the storage buffers to an empty array.

				Unsafe.SkipInit(out IntPtr dummy); // No worries about an uninitialized value, because num_storage_buffers is 0, the dummy value will never actually be read from

				if (!(bool)SDL_SetGPURenderStateStorageBuffers(mState, num_storage_buffers: 0, storage_buffers: unchecked((GpuBuffer.SDL_GPUBuffer**)&dummy)))
				{
					return false;
				}

				mStorageBuffers = null;

				return true;
			}

			GpuBuffer.SDL_GPUBuffer** storageBuffersOnHeap, storageBuffersPtr;

			if (storageBuffers.Length is <= stackAllocThreshold)
			{
				var storageBuffersOnStack = stackalloc GpuBuffer.SDL_GPUBuffer*[storageBuffers.Length];
				storageBuffersOnHeap = null;
				storageBuffersPtr = storageBuffersOnStack;
			}
			else
			{
				storageBuffersOnHeap = (GpuBuffer.SDL_GPUBuffer**)NativeMemory.Malloc(unchecked((nuint)storageBuffers.Length * (nuint)sizeof(GpuBuffer.SDL_GPUBuffer*)));
				storageBuffersPtr = storageBuffersOnHeap;
			}

			try
			{
				var newStorageBuffers = GC.AllocateUninitializedArray<GpuBuffer>(storageBuffers.Length);

				for (var index = 0; index < storageBuffers.Length; index++)
				{
					ref readonly var storageBuffer = ref storageBuffers[index];

					newStorageBuffers[index] = storageBuffer;
					storageBuffersPtr[index] = storageBuffer is not null ? storageBuffer.Pointer : null;
				}

				if (!(bool)SDL_SetGPURenderStateStorageBuffers(mState, num_storage_buffers: storageBuffers.Length, storage_buffers: storageBuffersPtr))
				{
					return false;
				}

				mStorageBuffers = newStorageBuffers;

				return true;
			}
			finally
			{
				NativeMemory.Free(storageBuffersOnHeap); // It's fine if storageBuffersOnHeap is null, NativeMemory.Free does nothing in that case
			}
		}
	}

	private GpuTexture[]? mStorageTextures = null; // used to keep the managed GpuTexture instances alive that were used to set the storage textures of this RenderState, if any
												   // this is necessary because the native SDL_GPURenderState may reference their underlying native resources at some point

	/// <summary>
	/// Tries to set storage textures in the GPU render state
	/// </summary>
	/// <param name="storageTextures">The storage textures to bind</param>
	/// <returns><c><see langword="true"/></c> if the storage textures were successfully set; otherwise, <c><see langword="false"/> (check <see cref="Error.TryGet(out string?)"/> for more information)</c></returns>
	/// <remarks>
	/// <para>
	/// The given <paramref name="storageTextures"/> are copied and will be binded using <see cref="SDL_BindGPUFragmentStorageTextures"/> during draw call execution.
	/// </para>
	/// <para>
	/// This method should be called on the thread that created the <see cref="Renderer{TDriver}">Renderer</see>&lt;<see cref="Drivers.Gpu">Gpu</see>&gt;.
	/// </para>
	/// </remarks>
	public bool TrySetStorageTextures(ReadOnlySpan<GpuTexture> storageTextures)
	{
		const int stackAllocThreshold = 16;

		unsafe
		{
			if (storageTextures.IsEmpty)
			{
				// There's a reason for passing in a non-null pointer to a dummy SDL_SetGPURenderStateStorageTextures on stack,
				// even though we pass in 0 for num_storage_textures:
				// SDL_SetGPURenderStateStorageTextures for some reason doesn't check for passed in null pointers
				// and calls SDL_memcpy unconditionally, even if num_storage_textures is 0,
				// and SDL_memcpy fails if it receives a null pointer for either the source or the destination,
				// which would let SDL_SetGPURenderStateStorageTextures fail in turn.
				// But it's totally valid to set the storage textures to an empty array.

				Unsafe.SkipInit(out IntPtr dummy); // No worries about an uninitialized value, because num_storage_textures is 0, the dummy value will never actually be read from

				if (!(bool)SDL_SetGPURenderStateStorageTextures(mState, num_storage_textures: 0, storage_textures: unchecked((GpuTexture.SDL_GPUTexture**)&dummy)))
				{
					return false;
				}

				mStorageTextures = null;

				return true;
			}
			GpuTexture.SDL_GPUTexture** storageTexturesOnHeap, storageTexturesPtr;

			if (storageTextures.Length is <= stackAllocThreshold)
			{
				var storageTexturesOnStack = stackalloc GpuTexture.SDL_GPUTexture*[storageTextures.Length];
				storageTexturesOnHeap = null;
				storageTexturesPtr = storageTexturesOnStack;
			}
			else
			{
				storageTexturesOnHeap = (GpuTexture.SDL_GPUTexture**)NativeMemory.Malloc(unchecked((nuint)storageTextures.Length * (nuint)sizeof(GpuTexture.SDL_GPUTexture*)));
				storageTexturesPtr = storageTexturesOnHeap;
			}

			try
			{
				var newStorageTextures = GC.AllocateUninitializedArray<GpuTexture>(storageTextures.Length);

				for (var index = 0; index < storageTextures.Length; index++)
				{
					ref readonly var storageTexture = ref storageTextures[index];

					newStorageTextures[index] = storageTexture;
					storageTexturesPtr[index] = storageTexture is not null ? storageTexture.Pointer : null;
				}

				if (!(bool)SDL_SetGPURenderStateStorageTextures(mState, num_storage_textures: storageTextures.Length, storage_textures: storageTexturesPtr))
				{
					return false;
				}

				mStorageTextures = newStorageTextures;

				return true;
			}
			finally
			{
				NativeMemory.Free(storageTexturesOnHeap); // It's fine if storageTexturesOnHeap is null, NativeMemory.Free does nothing in that case
			}
		}
	}

#endif
}

#endif
