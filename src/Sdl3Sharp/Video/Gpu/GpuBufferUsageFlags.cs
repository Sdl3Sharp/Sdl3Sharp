using System;

namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents how a buffer is intended to be used by the client
/// </summary>
/// <remarks>
/// <para>
/// A buffer must have at least one usage flag.
/// Note that some usage flag combinations are invalid.
/// </para>
/// <para>
/// Unlike <see cref="GpuTextureUsageFlags"/>, <see cref="ComputeStorageRead"/> and <see cref="ComputeStorageWrite"/> flags can be used for simultaneous read-write usage.
/// The same data synchronization concerns as textures apply.
/// </para>
/// <para>
/// If you use any of the <see cref="GraphicsStorageRead"/>, <see cref="ComputeStorageRead"/>, or <see cref="ComputeStorageWrite"/> flags, the data in the buffer must respect "std140" layout conventions.
/// In practical terms this means you must ensure that <c>vec3</c> and <c>vec4</c> fields are 16-byte aligned.
/// </para>
/// </remarks>
[Flags]
public enum GpuBufferUsageFlags : uint
{
	/// <summary>The buffer is a vertex buffer</summary>
	Vertex = 1u << 0,

	/// <summary>The buffer is an index buffer</summary>
	Index = 1u << 1,

	/// <summary>The buffer is an indirect buffer</summary>
	Indirect = 1u << 2,

	/// <summary>The buffer supports storage reads in graphics stages</summary>
	GraphicsStorageRead = 1u << 3,

	/// <summary>The buffer supports storage reads in compute stages</summary>
	ComputeStorageRead = 1u << 4,

	/// <summary>The buffer supports storage writes in compute stages</summary>
	ComputeStorageWrite = 1u << 5,
}
