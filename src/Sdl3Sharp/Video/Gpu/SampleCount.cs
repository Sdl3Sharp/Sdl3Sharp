namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the number of samples used when a texture is used as a multisample render target
/// </summary>
public enum SampleCount
{
	/// <summary>No multisampling</summary>
	X1,

	/// <summary>MSAA 2x</summary>
	X2,

	/// <summary>MSAA 4x</summary>
	X4,

	/// <summary>MSAA 8x</summary>
	X8
}
