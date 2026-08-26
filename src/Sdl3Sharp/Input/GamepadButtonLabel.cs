namespace Sdl3Sharp.Input;

/// <summary>
/// Represents a simple label for a face button on a <see cref="Gamepad"/>
/// </summary>
/// <remarks>
/// <para>
/// This enumeration isn't a complete set and just contains the face buttons to make it easier to identify them in a platform-agnostic way.
/// You can use this to show button prompts in your application without having to worry about mapping <see cref="GamepadButton"/>s correctly based on the controller type.
/// </para>
/// </remarks>
public enum GamepadButtonLabel
{
	/// <summary>The label is unknown or not applicable for the button</summary>
	Unknown,

	/// <summary>The button labeled "A" on the gamepad</summary>
	A,

	/// <summary>The button labeled "B" on the gamepad</summary>
	B,

	/// <summary>The button labeled "X" on the gamepad</summary>
	X,

	/// <summary>The button labeled "Y" on the gamepad</summary>
	Y,

	/// <summary>The button labeled with a cross on the gamepad</summary>
	Cross,

	/// <summary>The button labeled with a circle on the gamepad</summary>
	Circle,

	/// <summary>The button labeled with a square on the gamepad</summary>
	Square,

	/// <summary>The button labeled with a triangle on the gamepad</summary>
	Triangle
}
