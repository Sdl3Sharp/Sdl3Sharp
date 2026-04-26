namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents a blend factor to used when pixels in a render target are blended with existing pixels in the texture
/// </summary>
/// <remarks>
/// <para>
/// The <c>source color</c> is the value written by the fragment shader.
/// The <c>destination color</c> is the value currently existing in the texture.
/// </para>
/// </remarks>
public enum GpuBlendFactor
{
	/// <summary>Represents an invalid blend factor</summary>
	Invalid,

	/// <summary>Blend factor: <c>0</c></summary>
	Zero,

	/// <summary>Blend factor: <c>1</c></summary>
	One,

	/// <summary>Blend factor: <c>source color</c></summary>
	SourceColor,

	/// <summary>Blend factor: <c>1 - source color</c></summary>
	OneMinusSourceColor,

	/// <summary>Blend factor: <c>destination color</c></summary>
	DestinationColor,

	/// <summary>Blend factor: <c>1 - destination color</c></summary>
	OneMinusDestinationColor,

	/// <summary>Blend factor: <c>source alpha</c></summary>
	SourceAlpha,

	/// <summary>Blend factor: <c>1 - source alpha</c></summary>
	OneMinusSourceAlpha,

	/// <summary>Blend factor: <c>destination alpha</c></summary>
	DestinationAlpha,

	/// <summary>Blend factor: <c>1 - destination alpha</c></summary>
	OneMinusDestinationAlpha,

	/// <summary>Blend factor: <c>constant color</c></summary>
	ConstantColor,

	/// <summary>Blend factor: <c>1 - constant color</c></summary>
	OneMinusConstantColor,

	/// <summary>Blend factor: <c>min(source alpha, 1 - destination alpha)</c></summary>
	SourceAlphaSaturate
}
