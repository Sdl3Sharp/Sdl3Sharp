namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the type of a texture
/// </summary>
public enum GpuTextureType
{
	/// <summary>The texture is a 2-dimensional image</summary>
	Texture2D,

	/// <summary>The texture is an array of 2-dimensional images</summary>
	Texture2DArray,

	/// <summary>The texture is a 3-dimensional image</summary>
	Texture3D,

	/// <summary>The texture is a cube image</summary>
	TextureCube,

	/// <summary>The texture is an array of cube images</summary>
	TextureCubeArray
}
