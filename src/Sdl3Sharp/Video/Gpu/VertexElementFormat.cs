namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the format of a vertex attribute
/// </summary>
public enum VertexElementFormat
{
	/// <summary>Represents an invalid vertex element format</summary>
	Invalid,

	#region 32-bit signed integer formats

	/// <summary>Scalar 32-bit signed integer value (<c>int</c>)</summary>
	Int,

	/// <summary>Vector of 2 32-bit signed integer values (<c>int2</c>)</summary>
	Int2,

	/// <summary>Vector of 3 32-bit signed integer values (<c>int3</c>)</summary>
	Int3,

	/// <summary>Vector of 4 32-bit signed integer values (<c>int4</c>)</summary>
	Int4,

	#endregion

	#region 32-bit unsigned integer formats

	/// <summary>Scalar 32-bit unsigned integer value</summary>
	UInt,

	/// <summary>Vector of 2 32-bit unsigned integer values</summary>
	UInt2,

	/// <summary>Vector of 3 32-bit unsigned integer values</summary>
	UInt3,

	/// <summary>Vector of 4 32-bit unsigned integer values</summary>
	UInt4,

	#endregion

	#region 32-bit float formats

	/// <summary>Scalar 32-bit floating point value</summary>
	Float,

	/// <summary>Vector of 2 32-bit floating point values</summary>
	Float2,

	/// <summary>Vector of 3 32-bit floating point values</summary>
	Float3,

	/// <summary>Vector of 4 32-bit floating point values</summary>
	Float4,

	#endregion

	#region 8-bit signed integer formats

	/// <summary>Vector of 2 8-bit signed integer values</summary>
	Byte2,

	/// <summary>Vector of 4 8-bit signed integer values</summary>
	Byte4,

	#endregion

	#region 8-bit unsigned integer formats

	/// <summary>Vector of 2 8-bit unsigned integer values</summary>
	UByte2,

	/// <summary>Vector of 4 8-bit unsigned integer values</summary>
	UByte4,

	#endregion

	#region 8-bit normalized signed integer formats

	/// <summary>Vector of 2 8-bit normalized signed integer values</summary>
	Byte2Norm,

	/// <summary>Vector of 4 8-bit normalized signed integer values</summary>
	Byte4Norm,

	#endregion

	#region 8-bit normalized unsigned integer formats

	/// <summary>Vector of 2 8-bit normalized unsigned integer values</summary>
	UByte2Norm,

	/// <summary>Vector of 4 8-bit normalized unsigned integer values</summary>
	UByte4Norm,

	#endregion

	#region 16-bit singed integer formats

	/// <summary>Vector of 2 16-bit signed integer values</summary>
	Short2,

	/// <summary>Vector of 4 16-bit signed integer values</summary>
	Short4,

	#endregion

	#region 16-bit unsigned integer formats

	/// <summary>Vector of 2 16-bit unsigned integer values</summary>
	UShort2,

	/// <summary>Vector of 4 16-bit unsigned integer values</summary>
	UShort4,

	#endregion

	#region 16-bit normalized signed integer formats

	/// <summary>Vector of 2 16-bit normalized signed integer values</summary>
	Short2Norm,

	/// <summary>Vector of 4 16-bit normalized signed integer values</summary>
	Short4Norm,

	#endregion

	#region 16-bit normalized unsigned integer formats

	/// <summary>Vector of 2 16-bit normalized unsigned integer values</summary>
	UShort2Norm,

	/// <summary>Vector of 4 16-bit normalized unsigned integer values</summary>
	UShort4Norm,

	#endregion

	#region 16-bit float formats

	/// <summary>Vector of 2 16-bit floating point values</summary>
	Half2,

	/// <summary>Vector of 4 16-bit floating point values</summary>
	Half4,

	#endregion
}
