namespace Sdl3Sharp.Input;

/// <summary>
/// Represents the type of a <see cref="TouchDevice"/>
/// </summary>
public enum TouchDeviceType
{
	/// <summary>Represents an invalid touch device type</summary>
	Invalid = -1,

	/// <summary>A touch device with window-relative coordinates (e.g., a touch screen)</summary>
	Direct,

	/// <summary>
	/// A touch device with absolute device coordinates (e.g., a trackpad)
	/// </summary>
	IndirectAbsolute,

	/// <summary>
	/// A touch device with screen cursor-relative coordinates (e.g., a trackpad)
	/// </summary>
	IndirectRelative
}
