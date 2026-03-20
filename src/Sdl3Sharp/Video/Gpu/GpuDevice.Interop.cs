using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Video.Gpu;

partial class GpuDevice
{
	// opaque struct
	[StructLayout(LayoutKind.Sequential, Size = 0)]
	internal readonly struct SDL_GPUDevice;

	/// <summary>
	/// Returns the name of the backend used to create this GPU context
	/// </summary>
	/// <param name="device">A GPU context to query</param>
	/// <returns>Returns the name of the device's driver, or NULL on error</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetGPUDeviceDriver">SDL_GetGPUDeviceDriver</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial byte* SDL_GetGPUDeviceDriver(SDL_GPUDevice* device);
}
