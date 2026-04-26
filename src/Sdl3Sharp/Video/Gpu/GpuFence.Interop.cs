using System.Runtime.InteropServices;

namespace Sdl3Sharp.Video.Gpu;

partial class GpuFence
{
	// opaque struct
	[StructLayout(LayoutKind.Sequential, Size = 0)]
	internal readonly struct SDL_GPUFence;
}
