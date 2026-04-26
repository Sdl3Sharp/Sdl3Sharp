namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents how stencil values are treated when a stencil tests fail or pass
/// </summary>
public enum StencilOperation
{
	/// <summary>Resprents an invalid stencil operation</summary>
	Invalid,

	/// <summary>The current stencil value will be kept</summary>
	Keep,

	/// <summary>The current stencil value will be set to <c>0</c></summary>
	Zero,

	/// <summary>The current stencil value will be replaced with a reference value</summary>
	Replace,

	/// <summary>The current stencil value will be incremente and clamped to the maximum representable value</summary>
	IncrementAndClamp,

	/// <summary>The current stencil value will be decremented and clamped to <c>0</c></summary>
	DecrementAndClamp,

	/// <summary>The current stencil value will be bitwise inverted</summary>
	Invert,

	/// <summary>The current stencil value will be incremented and wrapped to <c>0</c> when exceeding the maximum representable value</summary>
	IncrementAndWrap,

	/// <summary>The current stencil value will be decremented and wrapped to the maximum representable value when going below <c>0</c></summary>
	DecrementAndWrap
}
