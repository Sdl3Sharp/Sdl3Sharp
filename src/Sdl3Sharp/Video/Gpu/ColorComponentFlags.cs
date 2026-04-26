using System;

namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents which color components are written in a graphics pipeline
/// </summary>
[Flags]
public enum ColorComponentFlags : byte
{
	/// <summary>The red component</summary>
	R = 1 << 0,

	/// <summary>The green component</summary>
	G = 1 << 1,

	/// <summary>The blue component</summary>
	B = 1 << 2,

	/// <summary>The alpha component</summary>
	A = 1 << 3,
}
