using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Gpu;

[method: MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization), SetsRequiredMembers]
public readonly partial struct GpuTextureSamplerBinding(GpuTexture texture, Sampler sampler)
{
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static GpuTexture ValidateTexture(GpuTexture texture)
	{
		if (texture is null)
		{
			failTextureArgumentNull();
		}

		return texture;

		[DoesNotReturn]
		static void failTextureArgumentNull() => throw new ArgumentNullException(nameof(texture));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private static Sampler ValidateSampler(Sampler sampler)
	{
		if (sampler is null)
		{
			failSamplerArgumentNull();
		}

		return sampler;

		[DoesNotReturn]
		static void failSamplerArgumentNull() => throw new ArgumentNullException(nameof(sampler));
	}

	public required readonly GpuTexture Texture
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => field;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] init => field = ValidateTexture(value);
	} = ValidateTexture(texture);

	public required readonly Sampler Sampler
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => field;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] init => field = ValidateSampler(value);
	} = ValidateSampler(sampler);

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly void Deconstruct(out GpuTexture texture, out Sampler sampler) { texture = Texture; sampler = Sampler; }

	internal unsafe SDL_GPUTextureSamplerBinding ToNative() => new(Texture is not null ? Texture.Pointer : null, Sampler is not null ? Sampler.Pointer : null);
}
