using Sdl3Sharp.SourceGeneration;
using Sdl3Sharp.SourceGeneration.RegisterDriver;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Gpu.Drivers;

// TODO: fix the doc once the methods and properties mentioned here are implemented
/// <summary>
/// Represents the Direct3D 12 GPU driver
/// </summary>
/// <remarks>
/// <para>
/// This GPU backend is only supported on Windows 10 or newer, Xbox One (GDK), and Xbox Series X/S (GDK)
/// and requires a GPU that supports Direct3D 12 feature level 11_0 and Resource Binding Tier 2 or above.
/// </para>
/// <para>
/// You can remove the Tier 2 resource binding requirement to support Intel Haswell and Broadwell GPUs
/// by using this property when creating the GPU device with <see cref="SDL_CreateGPUDeviceWithProperties"/>:
/// <list type="bullet">
///	<item><description><see cref="SDL_PROP_GPU_DEVICE_CREATE_D3D12_ALLOW_FEWER_RESOURCE_SLOTS_BOOLEAN"/></description></item>
/// </list>
/// </para>
/// </remarks>
[RegisterDriver(Name)]
public sealed partial class Direct3D12 : IGpuDriver
{
	/// <summary>
	/// The name of the Direct3D 12 GPU driver
	/// </summary>
	/// <remarks>
	/// <para>
	/// The value of this constant is equal to <c>"direct3d12"</c>.
	/// </para>
	/// </remarks>
	public const string Name = "direct3d12";

	[NotNull] static string? IGpuDriver.Name { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => Name; }

	[FormattedConstant($"{Name}\0")] private static partial ReadOnlySpan<byte> NameAscii { get; }

	static ReadOnlySpan<byte> IGpuDriver.NameAscii { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => NameAscii; }

	private Direct3D12() { }
}
