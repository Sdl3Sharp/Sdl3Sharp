#if SDL3_4_0_OR_GREATER

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents the type of surface device that a <see cref="Pen"/> touches
/// </summary>
public enum PenDeviceType
{
	/// <summary>Represents an invalid pen surface device type</summary>
	Invalid = -1,

	/// <summary>The specifics of the pen surface device are unknown</summary>
	Unknown,

	/// <summary>A pen touches a display (e.g., the screen) directly</summary>
	/// <remarks>
	/// <para>
	/// The surface that the pen touches is a graphic display.
	/// It could be the main screen of the device (e.g., a tablet device) or an external surface that <em>is</em> a display.
	/// </para>
	/// </remarks>
	Direct,

	/// <summary>A pen touches a surface that is not a display (e.g., a graphics tablet)</summary>
	/// <remarks>
	/// <para>
	/// The surface that the pen touches <em>is not</em> a graphic display.
	/// Most commonly, this is an external graphics tablet that is connected to the device.
	/// </para>
	/// </remarks>
	Indirect
}

#endif
