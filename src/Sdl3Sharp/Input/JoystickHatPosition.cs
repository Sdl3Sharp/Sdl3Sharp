namespace Sdl3Sharp.Input;

/// <summary>
/// Represents the position of a POV hat on a joystick
/// </summary>
public enum JoystickHatPosition : byte
{
	/// <summary>Centered position</summary>
	Centered = 0x00,

	/// <summary>Up position</summary>
	Up = 0x01,

	/// <summary>Right position</summary>
	Right = 0x02,

	/// <summary>Down position</summary>
	Down = 0x04,

	/// <summary>Left position</summary>
	Left = 0x08,

	/// <summary>Right-Up position</summary>
	RightUp = Right | Up,

	/// <summary>Right-Down position</summary>
	RightDown = Right | Down,

	/// <summary>Left-Up position</summary>
	LeftUp = Left | Up,

	/// <summary>Left-Down position</summary>
	LeftDown = Left | Down,
}
