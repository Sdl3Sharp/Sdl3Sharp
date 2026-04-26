#if SDL3_4_0_OR_GREATER

namespace Sdl3Sharp.Video.Gpu;

partial class RenderStateCreateInfo
{
	internal unsafe struct SDL_GPURenderStateCreateInfo
	{
		public Shader.SDL_GPUShader* FragmentShader;

		public int NumSamplerBindings;
		public GpuTextureSamplerBinding.SDL_GPUTextureSamplerBinding* SamplerBindings;

		public int NumStorageTextures;
		public GpuTexture.SDL_GPUTexture** StorageTextures;

		public int NumStorageBuffers;
		public GpuBuffer.SDL_GPUBuffer** StorageBuffers;

		public uint Props;
	}
}

#endif
