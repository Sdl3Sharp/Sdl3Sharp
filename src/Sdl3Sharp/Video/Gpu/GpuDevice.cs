using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Gpu;

public abstract partial class GpuDevice
{
	private unsafe SDL_GPUDevice* mDevice;

	// TODO: IMPLEMENT!
	private protected unsafe GpuDevice(SDL_GPUDevice* device, bool register)
	{
		mDevice = device;
	}

	internal unsafe SDL_GPUDevice* Pointer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mDevice; }

	// TODO: IMPLEMENT!
	internal unsafe static bool TryGetOrCreate(SDL_GPUDevice* device, [NotNullWhen(true)] out GpuDevice? result)
	{
		result = null;
		return false;
	}
}
