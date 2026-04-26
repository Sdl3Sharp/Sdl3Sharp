namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents a blend operation to used when pixels in a render target are blended with existing pixels in the texture
/// </summary>
/// <remarks>
/// <para>
/// The <c>source color</c> is the value written by the fragment shader.
/// The <c>destination color</c> is the value currently existing in the texture.
/// </para>
/// </remarks>
public enum GpuBlendOperation
{
	/// <summary>Represents an invalid blend operation</summary>
	Invalid,

	/// <summary>Blend operation: <c>(source color * source factor) + (destination color * destination factor)</c></summary>
	Add,

	/// <summary>Blend operation: <c>(source color * source factor) - (destination color * destination factor)</c></summary>
	Subtract,

	/// <summary>Blend operation: <c>(destination color * destination factor) - (source color * source factor)</c></summary>
	ReverseSubtract,

	/// <summary>Blend operation: <c>min(source color, destination color)</c></summary>
	Min,

	/// <summary>Blend operation: <c>max(source color, destination color)</c></summary>
	Max
}
