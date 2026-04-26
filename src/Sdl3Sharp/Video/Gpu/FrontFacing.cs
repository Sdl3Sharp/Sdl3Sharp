namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the vertex winding of triangles that is considered front-facing
/// </summary>
/// <seealso cref="CullMode"/>
public enum FrontFacing
{
	/// <summary>Triangles with counter-clockwise vertex winding are considered front-facing</summary>
	CounterClockwise,

	/// <summary>Triangles with clockwise vertex winding are considered front-facing</summary>
	Clockwise
}
