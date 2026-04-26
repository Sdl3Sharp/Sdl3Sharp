namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents how the contents of a texture attached to a render pass are treated at the beginning of the render pass
/// </summary>
public enum LoadOperation
{
	/// <summary>The previous contents of the texture will be preserved</summary>
	Load,

	/// <summary>The previous contents of the texture will be cleared to a color</summary>
	Clear,

	/// <summary>
	/// The previous contents of the texture don't need to be preserved.
	/// The contents of the texture will be undefined.
	/// </summary>
	DontCare
}
