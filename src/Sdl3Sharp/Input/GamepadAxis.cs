namespace Sdl3Sharp.Input;

/// <summary>
/// Represents the axes of a <see cref="Gamepad"/>
/// </summary>
public enum GamepadAxis
{
	/// <summary>Represents an invalid axis</summary>
	Invalid = -1,

	/// <summary>Left thumbstick's horizontal axis</summary>
	/// <remarks>
	/// <para>
	/// Ranges from <c><see cref="short.MinValue"/></c> to <c><see cref="short.MaxValue"/></c> and is centered within around <c>8000</c> of <c>0</c>.
	/// </para>
	/// </remarks>
	LeftX,

	/// <summary>Left thumbstick's vertical axis</summary>
	/// <remarks>
	/// <para>
	/// Ranges from <c><see cref="short.MinValue"/></c> to <c><see cref="short.MaxValue"/></c> and is centered within around <c>8000</c> of <c>0</c>.
	/// </para>
	/// </remarks>
	LeftY,

	/// <summary>Right thumbstick's horizontal axis</summary>
	/// <remarks>
	/// <para>
	/// Ranges from <c><see cref="short.MinValue"/></c> to <c><see cref="short.MaxValue"/></c> and is centered within around <c>8000</c> of <c>0</c>.
	/// </para>
	/// </remarks>
	RightX,

	/// <summary>Right thumbstick's vertical axis</summary>
	/// <remarks>
	/// <para>
	/// Ranges from <c><see cref="short.MinValue"/></c> to <c><see cref="short.MaxValue"/></c> and is centered within around <c>8000</c> of <c>0</c>.
	/// </para>
	/// </remarks>
	RightY,

	/// <summary>Left trigger axis</summary>
	/// <remarks>
	/// <para>
	/// Ranges from <c>0</c> to <c><see cref="short.MaxValue"/></c>.
	/// Note that this is not the same range that the corresponding <see cref="Joystick"/> axis would have.
	/// </para>
	/// </remarks>
	LeftTrigger,

	/// <summary>Right trigger axis</summary>
	/// <remarks>
	/// <para>
	/// Ranges from <c>0</c> to <c><see cref="short.MaxValue"/></c>.
	/// Note that this is not the same range that the corresponding <see cref="Joystick"/> axis would have.
	/// </para>
	/// </remarks>
	RightTrigger
}
