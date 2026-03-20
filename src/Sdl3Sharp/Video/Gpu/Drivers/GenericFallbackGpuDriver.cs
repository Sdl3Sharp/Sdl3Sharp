using System;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Gpu.Drivers;

internal sealed class GenericFallbackGpuDriver : IGpuDriver
{
	static string? IGpuDriver.Name { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => null; }

	static ReadOnlySpan<byte> IGpuDriver.NameAscii { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => []; }

	private GenericFallbackGpuDriver() { }
}
