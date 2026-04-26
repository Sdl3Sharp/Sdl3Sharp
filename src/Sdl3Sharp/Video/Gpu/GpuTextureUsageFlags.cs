using System;

namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents how a texture is intended to be used by the client
/// </summary>
/// <remarks>
/// <para>
/// A texture must have at least one usage flag.
/// Note that some usage flag combinations are invalid.
/// </para>
/// <para>
/// With regards to compute storage usage, <see cref="ComputeStorageRead"/> | <see cref="ComputeStorageWrite"/> means that you can have shader A that only writes into the texture and shader B that only reads from the texture and bind the same texture to either shader respectively.
/// <see cref="ComputeStorageSimultaneousReadWrite"/> means that you can do reads and writes within the same shader or compute pass.
/// It also implies that atomic ops can be used, since those are read-modify-write operations.
/// If you use <see cref="ComputeStorageSimultaneousReadWrite"/>, you are responsible for avoiding data races, as there is no data synchronization within a compute pass.
/// Note that <see cref="ComputeStorageSimultaneousReadWrite"/> usage is only supported by a limited number of texture formats.
/// </para>
/// </remarks>
[Flags]
public enum GpuTextureUsageFlags : uint
{
	/// <summary>The texture supports sampling</summary>
	Sampler = 1u << 0,

	/// <summary>The texture is a color render target</summary>
	ColorTarget = 1u << 1,

	/// <summary>The texture is a depth stencil render target</summary>
	DepthStencilTarget = 1u << 2,

	/// <summary>The texture supports storage reads in graphics stages</summary>
	GraphicsStorageRead = 1u << 3,

	/// <summary>The texture supports storage reads in compute stages</summary>
	ComputeStorageRead = 1u << 4,

	/// <summary>The texture supports storage writes in compute stages</summary>
	ComputeStorageWrite = 1u << 5,

	/// <summary>The texture supports simultaneous storage reads and writes in compute stages</summary>
	/// <remarks>
	/// <para>
	/// This is <em>not</em> the same as <c><see cref="ComputeStorageRead"/> | <see cref="ComputeStorageWrite"/></c>!
	/// </para>
	/// </remarks>
	ComputeStorageSimultaneousReadWrite = 1u << 6
}
