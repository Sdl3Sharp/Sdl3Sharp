namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the rate at which vertex attributes are pulled from buffers
/// </summary>
public enum VertexInputRate
{
	/// <summary>The rate of vertex attribute pulling is a function of the vertex index</summary>
	Vertex,

	/// <summary>The rate of vertex attribute pulling is a function of the instance index</summary>
	Instance
}
