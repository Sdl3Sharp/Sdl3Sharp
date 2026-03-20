using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Gpu.Drivers;

partial interface IGpuDriver
{
	/// <summary>
	/// Gets the name of a built in GPU driver
	/// </summary>
	/// <param name="index">The index of a GPU driver</param>
	/// <returns>Returns the name of the GPU driver with the given <em><paramref name="index"/></em></returns>
	/// <remarks>
	/// <para>
	/// The GPU drivers are presented in the order in which they are normally checked during initialization.
	/// </para>
	/// <para>
	/// The names of drivers are all simple, low-ASCII identifiers, like "vulkan", "metal" or "direct3d12".
	/// These never have Unicode characters, and are not meant to be proper names.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetGPUDriver">SDL_GetGPUDriver</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial byte* SDL_GetGPUDriver(int index);

	/// <summary>
	/// Gets the number of GPU drivers compiled into SDL
	/// </summary>
	/// <returns>Returns the number of built in GPU drivers</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetNumGPUDrivers">SDL_GetNumGPUDrivers</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial int SDL_GetNumGPUDrivers();
}
