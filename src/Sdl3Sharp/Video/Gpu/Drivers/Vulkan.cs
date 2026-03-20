using Sdl3Sharp.SourceGeneration;
using Sdl3Sharp.SourceGeneration.RegisterDriver;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Gpu.Drivers;

// TODO: fix the doc once the methods and properties mentioned here are implemented
/// <summary>
/// Represents the Vulkan GPU driver
/// </summary>
/// <remarks>
/// <para>
/// This GPU backend is supported on Windows, Linux, Nintendo Switch, and certain Android devices
/// and requires Vulkan 1.0 or above with the following extensions and device features:
/// <list type="bullet">
///		<item><description><c>VK_KHR_swapchain</c></description></item>
///		<item><description><c>VK_KHR_maintenance1</c></description></item>
///		<item><description><c>independentBlend</c></description></item>
///		<item><description><c>imageCubeArray</c></description></item>
///		<item><description><c>depthClamp</c></description></item>
///		<item><description><c>shaderClipDistance</c></description></item>
///		<item><description><c>drawIndirectFirstInstance</c></description></item>
///		<item><description><c>sampleRateShading</c></description></item>
/// </list>
/// </para>
/// <para>
/// You can remove some of these requirements to increase compatibility with Android devices
/// by using these properties when creating the GPU device with <see cref="SDL_CreateGPUDeviceWithProperties"/>:
/// <list type="bullet">
/// <item><description><see cref="SDL_PROP_GPU_DEVICE_CREATE_FEATURE_CLIP_DISTANCE_BOOLEAN"/></description></item>
/// <item><description><see cref="SDL_PROP_GPU_DEVICE_CREATE_FEATURE_DEPTH_CLAMPING_BOOLEAN"/></description></item>
/// <item><description><see cref="SDL_PROP_GPU_DEVICE_CREATE_FEATURE_INDIRECT_DRAW_FIRST_INSTANCE_BOOLEAN"/></description></item>
/// <item><description><see cref="SDL_PROP_GPU_DEVICE_CREATE_FEATURE_ANISOTROPY_BOOLEAN"/></description></item>
/// </list>
/// </para>
/// </remarks>
[RegisterDriver(Name)]
public sealed partial class Vulkan : IGpuDriver
{
	/// <summary>
	/// The name of the Vulkan GPU driver
	/// </summary>
	/// <remarks>
	/// <para>
	/// The value of this constant is equal to <c>"vulkan"</c>.
	/// </para>
	/// </remarks>
	public const string Name = "vulkan";

	[NotNull] static string? IGpuDriver.Name { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => Name; }

	[FormattedConstant($"{Name}\0")] private static partial ReadOnlySpan<byte> NameAscii { get; }

	static ReadOnlySpan<byte> IGpuDriver.NameAscii { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => NameAscii; }

	private Vulkan() { }
}
