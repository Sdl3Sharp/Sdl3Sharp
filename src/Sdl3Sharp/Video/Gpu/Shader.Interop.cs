using System.Runtime.InteropServices;

namespace Sdl3Sharp.Video.Gpu;

partial class Shader
{
	// opaque struct
	[StructLayout(LayoutKind.Sequential, Size = 0)]
	internal readonly struct SDL_GPUShader;
}
