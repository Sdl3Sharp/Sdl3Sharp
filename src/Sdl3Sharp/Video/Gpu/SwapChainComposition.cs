namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the texture format and color space of swap chain textures
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Sdr"/> will always be supported. Other compositions may not be supported on certain platforms.
/// It's recommended to check <see cref="SDL_WindowSupportsGPUSwapchainComposition"/> after claiming a window,
/// if you want to change the swap chain composition from the default <see cref="Sdr"/>.
/// </para>
/// </remarks>
public enum SwapChainComposition
{
	/// <summary>
	/// <c>B8G8R8A8</c> or <c>R8G8B8A8</c> swap chain.
	/// Pixel values are in sRGB encoding.
	/// </summary>
	Sdr,

	/// <summary>
	/// <c>B8G8R8A8_SRGB</c> or <c>R8G8B8A8_SRGB</c> swap chain.
	/// Pixel values are stored in memory in sRGB encoding, but are accessed in "linear" sRGB (sRGB but with a linear transfer function) in shaders.
	/// </summary>
	SdrLinear,

	/// <summary>
	/// <c>R16G16B16A16Float</c> swap chain.
	/// Pixel values are in extended linear sRGB encoding and can be outside the range of <c>0</c> to <c>1</c>.
	/// </summary>
	HdrExtendedLinear,

	/// <summary>
	/// <c>A2R10G10B10</c> or <c>A2B10G10R10</c> swap chain.
	/// Pixel values are in BT.2020 ST2084 (PQ) encoding.
	/// </summary>
	Hdr10St2084
}
