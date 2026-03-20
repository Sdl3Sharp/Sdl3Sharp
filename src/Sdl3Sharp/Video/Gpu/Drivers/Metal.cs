using Sdl3Sharp.SourceGeneration;
using Sdl3Sharp.SourceGeneration.RegisterDriver;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Gpu.Drivers;

/// <summary>
/// Represents the Metal GPU driver
/// </summary>
/// <remarks>
/// <para>
/// This GPU backend is only supported on maxOS 10.14+ and iOS/tvOS 13.0+.
/// Hardware requirements vary by operating system:
/// <list type="bullet">
///		<item>
///			<term>macOS</term>
///			<description>Apple Silicon or <see href="https://developer.apple.com/documentation/metal/mtlfeatureset/mtlfeatureset_macos_gpufamily2_v1?language=objc">Intel Mac2 family</see> GPU</description>
///		</item>
///		<item>
///			<term>iOS/tvOS</term>
///			<description>A9 GPU or newer</description>
///		</item>
///		<item>
///			<term>iOS Simulator and tvOS Simulator</term>
///			<description>unsupported</description>
///		</item>
/// </list>
/// </para>
/// </remarks>
[RegisterDriver(Name)]
public sealed partial class Metal : IGpuDriver
{
	/// <summary>
	/// The name of the Metal GPU driver
	/// </summary>
	/// <remarks>
	/// <para>
	/// The value of this constant is equal to <c>"metal"</c>.
	/// </para>
	/// </remarks>
	public const string Name = "metal";

	[NotNull] static string? IGpuDriver.Name { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => Name; }

	[FormattedConstant($"{Name}\0")] private static partial ReadOnlySpan<byte> NameAscii { get; }

	static ReadOnlySpan<byte> IGpuDriver.NameAscii { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => NameAscii; }

	private Metal() { }
}
