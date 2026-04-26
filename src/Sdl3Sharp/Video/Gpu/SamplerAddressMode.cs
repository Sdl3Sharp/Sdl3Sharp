namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the behavíor of texture sampling when the coordinates exceed the usable range
/// </summary>
public enum SamplerAddressMode
{
	/// <summary>Coordinates will wrap around</summary>
	Repeat,

	/// <summary>Coordinates will wrap around mirrored</summary>
	MirroredRepeat,

	/// <summary>Coordinates will be clamped to the usable range</summary>
	ClampToEdge,
}
