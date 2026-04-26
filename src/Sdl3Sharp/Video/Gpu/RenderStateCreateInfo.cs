#if SDL3_4_0_OR_GREATER

using Sdl3Sharp.Utilities;
using Sdl3Sharp.Video.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the information needed to create a <see cref="RenderState"/>
/// </summary>
/// <remarks>
/// <para>
/// This type can be used to share the same creation parameters across multiple <see cref="RenderState"/> instances, or if you want to prepare the creation parameters in advance and move the heavy lifting away from the constructor call.
/// Alternatively, you can use the <see cref="RendererExtensions.TryCreateRenderState(Renderer{Rendering.Drivers.Gpu}, Shader, out RenderState?, ReadOnlySpan{GpuTextureSamplerBinding}, ReadOnlySpan{GpuTexture}, ReadOnlySpan{GpuBuffer}, Properties?)"/> method
/// to create a <see cref="RenderState"/> without needing to create a separate <see cref="RenderStateCreateInfo"/> instance.
/// </para>
/// </remarks>
public sealed partial class RenderStateCreateInfo() : IDisposable
{
	// RenderStateCreateInfo also doubles as a way to keep the managed GpuTextureSamplerBinding instances, the GpuTexture instances, and the GpuBuffer instances alive
	// that are referenced by this instances and used when creating a RenderState,
	// so that they don't get GC'd while the native SDL_GPURenderState may still reference their underlying native resources.

	private SDL_GPURenderStateCreateInfo mCreateInfo = default;

	/// <summary>
	/// Creates a new <see cref="RenderStateCreateInfo"/> with the specified fragment shader, additional fragment sampler bindings, storage textures, storage buffers, and optional properties for extensions
	/// </summary>
	/// <param name="fragmentShader">The fragment shader to use</param>
	/// <param name="samplerBindings">The additional fragment sampler bindings</param>
	/// <param name="storageTextures">The storage textures</param>
	/// <param name="storageBuffers">The storage buffers</param>
	/// <param name="properties">Optional properties for extensions</param>
	/// <exception cref="ArgumentNullException"><paramref name="fragmentShader"/> is <c><see langword="null"/></c></exception>
	/// <exception cref="InvalidOperationException">
	/// Could not set the fragment sampler bindings
	/// - OR -
	/// Could not set the storage textures
	/// - OR -
	/// Could not set the storage buffers
	/// </exception>
	[SetsRequiredMembers]
	public RenderStateCreateInfo(Shader fragmentShader, ReadOnlySpan<GpuTextureSamplerBinding> samplerBindings = default, ReadOnlySpan<GpuTexture> storageTextures = default, ReadOnlySpan<GpuBuffer> storageBuffers = default, Properties? properties = null)
		: this()
	{
		if (fragmentShader is null)
		{
			failFragmentShaderArgumentNull();
		}

		FragmentShader = fragmentShader;

		if (!TrySetSamplerBindings(samplerBindings))
		{
			failCouldNotSetSamplerBindings();
		}

		if (!TrySetStorageTextures(storageTextures))
		{
			failCouldNotSetStorageTextures();
		}

		if (!TrySetStorageBuffers(storageBuffers))
		{
			failCouldNotSetStorageBuffers();
		}

		Properties = properties;

		[DoesNotReturn]
		static void failFragmentShaderArgumentNull() => throw new ArgumentNullException(nameof(fragmentShader));

		[DoesNotReturn]
		static void failCouldNotSetSamplerBindings() => throw new InvalidOperationException($"Could not set the fragment sampler bindings for the {nameof(RenderStateCreateInfo)}");

		[DoesNotReturn]
		static void failCouldNotSetStorageTextures() => throw new InvalidOperationException($"Could not set the storage textures for the {nameof(RenderStateCreateInfo)}");

		[DoesNotReturn]
		static void failCouldNotSetStorageBuffers() => throw new InvalidOperationException($"Could not set the storage buffers for the {nameof(RenderStateCreateInfo)}");
	}

	internal ref readonly SDL_GPURenderStateCreateInfo AsNative { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => ref mCreateInfo; }

	/// <summary>
	/// Gets or sets the fragment shader to use when the render state created from this <see cref="RenderStateCreateInfo"/> is active
	/// </summary>
	/// <value>
	/// The fragment shader to use when the render state created from this <see cref="RenderStateCreateInfo"/> is active
	/// </value>
	/// <exception cref="ArgumentNullException">
	/// When setting this property to <see langword="null"/>
	/// </exception>
	public required Shader FragmentShader
	{
		get => field;

		set
		{
			unsafe
			{
				if (value is null)
				{
					failValueArgumentNull();
				}

				field = value;
				mCreateInfo.FragmentShader = value.Pointer;
			}

			[DoesNotReturn]
			static void failValueArgumentNull() => throw new ArgumentNullException(nameof(value));
		}
	}

	/// <summary>
	/// Gets or sets optional properties for extensions
	/// </summary>
	/// <value>
	/// The properties used for extensions, or <see langword="null"/> if no extensions are needed
	/// </value>
	public Properties? Properties
	{
		get => field;

		set
		{
			field = value;
			mCreateInfo.Props = value switch { { Id: var id } => id, _ => 0 };
		}
	}

	/// <inheritdoc/>
	~RenderStateCreateInfo() => DisposeImpl();

	/// <inheritdoc/>
	public void Dispose()
	{
		DisposeImpl();
		GC.SuppressFinalize(this);
	}

	private unsafe void DisposeImpl()
	{
		NativeMemory.Free(mCreateInfo.SamplerBindings);
		NativeMemory.Free(mCreateInfo.StorageTextures);
		NativeMemory.Free(mCreateInfo.StorageBuffers);

		mCreateInfo = default;

		FragmentShader = null!;
		Properties = null;

		mSamplerBindings = null;
		mStorageTextures = null;
		mStorageBuffers = null;
	}

	internal static bool TryCreate(Shader fragmentShader, [NotNullWhen(true)] out RenderStateCreateInfo? createInfo, ReadOnlySpan<GpuTextureSamplerBinding> samplerBindings = default, ReadOnlySpan<GpuTexture> storageTextures = default, ReadOnlySpan<GpuBuffer> storageBuffers = default, Properties? properties = default)
	{
		if (fragmentShader is null)
		{
			createInfo = null;
			return false;
		}

		createInfo = new RenderStateCreateInfo() { FragmentShader = fragmentShader, Properties = properties };

		if (!(createInfo.TrySetSamplerBindings(samplerBindings)
			&& createInfo.TrySetStorageTextures(storageTextures)
			&& createInfo.TrySetStorageBuffers(storageBuffers)))
		{
			createInfo.Dispose();

			return false;
		}

		return true;
	}

	private GpuTextureSamplerBinding[]? mSamplerBindings = null; // we need this to keep the managed GpuTextures and GpuSamplers instances referenced by the GpuTextureSamplerBinding alive

	/// <summary>
	/// Tries to set the additional fragment samplers to bind when the render state created from this <see cref="RenderStateCreateInfo"/> is active
	/// </summary>
	/// <param name="samplerBindings">The additional fragment samplers to bind</param>
	/// <returns><c><see langword="true"/></c> if the fragment sampler bindings were successfully set; otherwise, <c><see langword="false"/></c></returns>
	public bool TrySetSamplerBindings(ReadOnlySpan<GpuTextureSamplerBinding> samplerBindings)
	{
		unsafe
		{
			if (samplerBindings.IsEmpty)
			{
				mCreateInfo.NumSamplerBindings = 0;
				mCreateInfo.SamplerBindings = null;

				mSamplerBindings = null;

				return true;
			}

			var byteLength = unchecked((nuint)samplerBindings.Length * (nuint)Unsafe.SizeOf<GpuTextureSamplerBinding.SDL_GPUTextureSamplerBinding>());


			if (mCreateInfo.SamplerBindings is null)
			{
				var sampleBindingsPtr = (GpuTextureSamplerBinding.SDL_GPUTextureSamplerBinding*)NativeMemory.Malloc(byteLength);

				if (sampleBindingsPtr is null)
				{
					return false;
				}

				mCreateInfo.NumSamplerBindings = samplerBindings.Length;
				mCreateInfo.SamplerBindings = sampleBindingsPtr;
			}
			else if (mCreateInfo.NumSamplerBindings != samplerBindings.Length) // "!=" also shrinks the allocated memory, if necessary
			{
				var sampleBindingsPtr = (GpuTextureSamplerBinding.SDL_GPUTextureSamplerBinding*)NativeMemory.Realloc(mCreateInfo.SamplerBindings, byteLength);

				if (sampleBindingsPtr is null)
				{
					return false;
				}

				mCreateInfo.NumSamplerBindings = samplerBindings.Length;
				mCreateInfo.SamplerBindings = sampleBindingsPtr;
			}

			mSamplerBindings = GC.AllocateUninitializedArray<GpuTextureSamplerBinding>(samplerBindings.Length);

			for (var index = 0; index < samplerBindings.Length; index++)
			{
				ref readonly var samplerBinding = ref samplerBindings[index];

				mSamplerBindings[index] = samplerBinding;
				mCreateInfo.SamplerBindings[index] = samplerBinding.ToNative();
			}

			return true;
		}
	}

	private GpuTexture[]? mStorageTextures = null; // we need this to keep the managed GpuTexture instances alive

	/// <summary>
	/// Tries to set the storage textures to bind when the render state created from this <see cref="RenderStateCreateInfo"/> is active
	/// </summary>
	/// <param name="storageTextures">The storage textures to bind</param>
	/// <returns><c><see langword="true"/></c> if the storage textures were successfully set; otherwise, <c><see langword="false"/></c></returns>
	public bool TrySetStorageTextures(ReadOnlySpan<GpuTexture> storageTextures)
	{
		unsafe
		{
			if (storageTextures.IsEmpty)
			{
				mCreateInfo.NumStorageTextures = 0;
				mCreateInfo.StorageTextures = null;

				mStorageTextures = null;

				return true;
			}

			var byteLength = unchecked((nuint)storageTextures.Length * (nuint)sizeof(GpuTexture.SDL_GPUTexture*));

			if (mCreateInfo.StorageTextures is null)
			{
				var storageTexturesPtr = (GpuTexture.SDL_GPUTexture**)NativeMemory.Malloc(byteLength);

				if (storageTexturesPtr is null)
				{
					return false;
				}

				mCreateInfo.NumStorageTextures = storageTextures.Length;
				mCreateInfo.StorageTextures = storageTexturesPtr;
			}
			else if (mCreateInfo.NumStorageTextures != storageTextures.Length) // "!=" also shrinks the allocated memory, if necessary
			{
				var storageTexturesPtr = (GpuTexture.SDL_GPUTexture**)NativeMemory.Realloc(mCreateInfo.StorageTextures, byteLength);

				if (storageTexturesPtr is null)
				{
					return false;
				}

				mCreateInfo.NumStorageTextures = storageTextures.Length;
				mCreateInfo.StorageTextures = storageTexturesPtr;
			}

			mStorageTextures = GC.AllocateUninitializedArray<GpuTexture>(storageTextures.Length);

			for (var index = 0; index < storageTextures.Length; index++)
			{
				ref readonly var storageTexture = ref storageTextures[index];

				mStorageTextures[index] = storageTexture;
				mCreateInfo.StorageTextures[index] = storageTexture is not null ? storageTexture.Pointer : null;
			}

			return true;
		}
	}

	private GpuBuffer[]? mStorageBuffers = null; // we need this to keep the managed GpuBuffer instances alive

	/// <summary>
	/// Tries to set the storage buffers to bind when the render state created from this <see cref="RenderStateCreateInfo"/> is active
	/// </summary>
	/// <param name="storageBuffers">The storage buffers to bind</param>
	/// <returns><c><see langword="true"/></c> if the storage buffers were successfully set; otherwise, <c><see langword="false"/></c></returns>
	public bool TrySetStorageBuffers(ReadOnlySpan<GpuBuffer> storageBuffers)
	{
		unsafe
		{
			if (storageBuffers.IsEmpty)
			{
				mCreateInfo.NumStorageBuffers = 0;
				mCreateInfo.StorageBuffers = null;

				mStorageBuffers = null;

				return true;
			}

			var byteLength = unchecked((nuint)storageBuffers.Length * (nuint)sizeof(GpuBuffer.SDL_GPUBuffer*));

			if (mCreateInfo.StorageBuffers is null)
			{
				var storageBuffersPtr = (GpuBuffer.SDL_GPUBuffer**)NativeMemory.Malloc(byteLength);

				if (storageBuffersPtr is null)
				{
					return false;
				}

				mCreateInfo.NumStorageBuffers = storageBuffers.Length;
				mCreateInfo.StorageBuffers = storageBuffersPtr;
			}
			else if (mCreateInfo.NumStorageBuffers != storageBuffers.Length) // "!=" also shrinks the allocated memory, if necessary
			{
				var storageBuffersPtr = (GpuBuffer.SDL_GPUBuffer**)NativeMemory.Realloc(mCreateInfo.StorageBuffers, byteLength);

				if (storageBuffersPtr is null)
				{
					return false;
				}

				mCreateInfo.NumStorageBuffers = storageBuffers.Length;
				mCreateInfo.StorageBuffers = storageBuffersPtr;
			}

			mStorageBuffers = GC.AllocateUninitializedArray<GpuBuffer>(storageBuffers.Length);

			for (var index = 0; index < storageBuffers.Length; index++)
			{
				ref readonly var storageBuffer = ref storageBuffers[index];

				mStorageBuffers[index] = storageBuffer;
				mCreateInfo.StorageBuffers[index] = storageBuffer is not null ? storageBuffer.Pointer : null;
			}

			return true;
		}
	}
}

#endif
