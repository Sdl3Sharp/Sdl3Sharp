using Sdl3Sharp.Video.Gpu.Drivers;

namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the format of GPU shader code
/// </summary>
/// <remarks>
/// <para>
/// Each format corresponds to a specific <see cref="IGpuDriver"/> backend that accepts it:
/// <list type="bullet">
/// <item><description><see cref="SpirV"/> for <see cref="Vulkan"/></description></item>
/// <item><description><see cref="Dxbc"/> and <see cref="Dxil"/> for <see cref="Direct3D12"/></description></item>
/// <item><description><see cref="Msl"/> and <see cref="MetalLib"/> for <see cref="Metal"/></description></item>
/// </list>
/// </para>
/// </remarks>
public enum ShaderFormat : uint
{
	/// <summary>Represents an invalid shader format</summary>
	Invalid = 0,

	/// <summary>Shaders for private platforms</summary>
	Private = 1u << 0,

	/// <summary>SPIR-V shaders for <see cref="Vulkan"/></summary>
	SpirV = 1u << 1,

	/// <summary>DXBC SM5_1 shaders for <see cref="Direct3D12"/></summary>
	Dxbc = 1u << 2,

	/// <summary>DXIL SM6_0 shaders for <see cref="Direct3D12"/></summary>
	Dxil = 1u << 3,

	/// <summary>MSL shaders for <see cref="Metal"/></summary>
	Msl = 1u << 4,

	/// <summary>Pre-compiled metallib shaders for <see cref="Metal"/></summary>
	MetalLib = 1u << 5,
}
