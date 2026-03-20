using Sdl3Sharp.Video.Gpu.Drivers;

namespace Sdl3Sharp.Video.Gpu;

public sealed partial class GpuDevice<TDriver> : GpuDevice
	where TDriver : notnull, IGpuDriver
{
	internal unsafe GpuDevice(SDL_GPUDevice* device, bool register) : base(device, register)
	{ }
}
