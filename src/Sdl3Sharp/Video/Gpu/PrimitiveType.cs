namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the type of primitive topology used in a graphics pipeline
/// </summary>
/// <remarks>
/// <para>
/// If you intend to use <see cref="PointList"/>, you must include the point size in the vertex shader code:
/// <list type="bullet">
///		<item>
///			<term>For HLSL compiling to SPIR-V</term>
///			<description>You must decorate a float output with <c>[[vk::builtin("PointSize")]]</c></description>
///		</item>
///		<item>
///			<term>For GLSL</term>
///			<description>You must set the <c>gl_PointSize</c> builtin</description>
///		</item>
///		<item>
///			<term>For MSL</term>
///			<description>You must include a float output with the <c>[[point_size]]</c> decorator</description>
///		</item>
/// </list>
/// Note that sized point topology is totally unsupported on Direct3D 12.
/// Any point size other than 1 will be ignored.
/// </para>
/// <para>
/// In general, you should avoid using point topology for both compatibility and performance reasons.
/// </para>
/// </remarks>
public enum PrimitiveType
{
	/// <summary>A series of separate triangles</summary>
	/// <remarks>
	/// <para>
	/// Each triangle is defined by three vertices.
	/// Therefore, triangles don't necessarily share any vertices or edges.
	/// </para>
	/// </remarks>
	TriangleList,

	/// <summary>A series of connected triangles</summary>
	/// <remarks>
	/// <para>
	/// Each triangle (after the first) is defined by the previous two vertices and a new vertex.
	/// Therefore, consecutive triangles share two vertices and an edge.
	/// </para>
	/// </remarks>
	TriangleStrip,

	/// <summary>A series of separate lines</summary>
	/// <remarks>
	/// <para>
	/// Each line is defined by two vertices.
	/// Therefore, lines don't necessarily share any vertices.
	/// </para>
	/// </remarks>
	LineList,

	/// <summary>A series of connected lines</summary>
	/// <remarks>
	/// <para>
	/// Each line (after the first) is defined by the previous vertex and a new vertex.
	/// Therefore, consecutive lines share a vertex.
	/// </para>
	/// </remarks>
	LineStrip,

	/// <summary>A series of separate points</summary>
	PointList
}
