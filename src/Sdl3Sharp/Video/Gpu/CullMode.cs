namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the direction of triangle faces to cull
/// </summary>
/// <seealso cref="FrontFacing"/>
public enum CullMode
{
	/// <summary>No triangles are culled</summary>
	None,

	/// <summary>Front-facing triangles are culled</summary>
	Front,

	/// <summary>Back-facing triangles are culled</summary>
	Back
}
