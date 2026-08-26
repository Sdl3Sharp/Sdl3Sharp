#if SDL3_6_0_OR_GREATER

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents the type of capacitive sensing on a <see cref="Gamepad"/>
/// </summary>
public enum GamepadCapSenseType
{
	/// <summary>Represents an invalid capacitive sensing type</summary>
	Invalid = -1,

	/// <summary>Capacitive sensing type for the left thumbstick</summary>
	/// <remarks>
	/// <para>
	/// Most likely activated by the user touching the top of the left thumbstick.
	/// </para>
	/// </remarks>
	LeftStick,

	/// <summary>Capacitive sensing type for the right thumbstick</summary>
	/// <remarks>
	/// <para>
	/// Most likely activated by the user touching the top of the right thumbstick.
	/// </para>
	/// </remarks>
	RightStick,

	/// <summary>Capacitive sensing type for the left handle of the controller</summary>
	/// <remarks>
	/// <para>
	/// Most likely activated by the user gripping the left handle of the controller.
	/// </para>
	/// </remarks>
	LeftGrip,

	/// <summary>Capacitive sensing type for the right handle of the controller</summary>
	/// <remarks>
	/// <para>
	/// Most likely activated by the user gripping the right handle of the controller.
	/// </para>
	/// </remarks>
	RightGrip
}

#endif
