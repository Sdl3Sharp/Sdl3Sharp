using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Video.Gpu;

partial struct GpuTextureSamplerBinding
{
	[StructLayout(LayoutKind.Sequential)]
	[method: MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal unsafe readonly struct SDL_GPUTextureSamplerBinding(GpuTexture.SDL_GPUTexture* texture, Sampler.SDL_GPUSampler* sampler)
	{
		public readonly GpuTexture.SDL_GPUTexture* Texture = texture;
		public readonly Sampler.SDL_GPUSampler* Sampler = sampler;
	}
}
