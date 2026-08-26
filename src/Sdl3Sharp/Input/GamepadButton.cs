namespace Sdl3Sharp.Input;

/// <summary>
/// Represents a button on a <see cref="Gamepad"/>
/// </summary>
/// <remarks>
/// <para>
/// Be aware of the differences between controller button layouts across different platforms and manufacturers, including regional differences.
/// Your application should allow for remapping actions based on user preferences, rather than assuming a specific button layout or function.
/// </para>
/// <para>
/// You might want to use <see cref="SDL_GetGamepadButtonLabel"/> to get <see cref="GamepadButtonLabel"/>s for the face buttons,
/// in order to show button prompts in your application without having to worry about mapping <see cref="GamepadButton"/>s correctly based on the controller type.
/// </para>
/// </remarks>
public enum GamepadButton
{
	/// <summary>Represent an invalid button</summary>
	Invalid = -1,

	/// <summary>The southern button of the diamond patterned face buttons</summary>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term>On Microsoft <em>Xbox</em> controllers</term>
	///			<description><em>A</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>Switch</em> controllers</term>
	///			<description><em>B</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>GameCube</em> controllers</term>
	///			<description><em>A</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Sony <em>PlayStation</em> controllers</term>
	///			<description><em>Cross</em> button</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// On controllers that don't use a diamond pattern for their face buttons, this button is usually the <em>primary action</em> button (likely labeled "A" or "1").
	/// </para>
	/// <para>
	/// This is often used as the "activate" or "confirm" button, but in some regions this is reversed with the <see cref="East"/> button ("cancel" or "go back" button).
	/// So your application should allow for remapping actions based on user preferences.
	/// </para>
	/// </remarks>
	South,

	/// <summary>The eastern button of the diamond patterned face buttons</summary>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term>On Microsoft <em>Xbox</em> controllers</term>
	///			<description><em>B</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>Switch</em> controllers</term>
	///			<description><em>A</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>GameCube</em> controllers</term>
	///			<description><em>X</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Sony <em>PlayStation</em> controllers</term>
	///			<description><em>Circle</em> button</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// On controllers that don't use a diamond pattern for their face buttons, this button is usually the <em>secondary action</em> button (likely labeled "B" or "2").
	/// </para>
	/// <para>
	/// This is often used as the "cancel" or "go back" button, but in some regions this is reversed with the <see cref="South"/> button ("activate" or "confirm" button).
	/// So your application should allow for remapping actions based on user preferences.
	/// </para>
	/// </remarks>
	East,

	/// <summary>The western button of the diamond patterned face buttons</summary>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term>On Microsoft <em>Xbox</em> controllers</term>
	///			<description><em>X</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>Switch</em> controllers</term>
	///			<description><em>Y</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>GameCube</em> controllers</term>
	///			<description><em>B</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Sony <em>PlayStation</em> controllers</term>
	///			<description><em>Square</em> button</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// On controllers that don't use a diamond pattern for their face buttons, this button is usually the <em>tertiary action</em> button (likely labeled "C" or "3").
	/// </para>
	/// </remarks>
	West,

	/// <summary>The northern button of the diamond patterned face buttons</summary>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term>On Microsoft <em>Xbox</em> controllers</term>
	///			<description><em>Y</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>Switch</em> controllers</term>
	///			<description><em>X</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>GameCube</em> controllers</term>
	///			<description><em>Y</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Sony <em>PlayStation</em> controllers</term>
	///			<description><em>Triangle</em> button</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// On controllers that don't use a diamond pattern for their face buttons, this button is usually the <em>quaternary action</em> button (likely labeled "D" or "4").
	/// </para>
	/// </remarks>
	North,

	/// <summary>The back button</summary>
	Back,

	/// <summary>The guide button</summary>
	Guide,

	/// <summary>The start button</summary>
	Start,

	/// <summary>The left stick button (i.e., pressing down on the left thumbstick)</summary>
	LeftStick,

	/// <summary>The right stick button (i.e., pressing down on the right thumbstick)</summary>
	RightStick,

	/// <summary>The left shoulder button</summary>
	LeftShoulder,

	/// <summary>The right shoulder button</summary>
	RightShoulder,

	/// <summary>The directional pad up button</summary>
	DPadUp,

	/// <summary>The directional pad down button</summary>
	DPadDown,

	/// <summary>The directional pad left button</summary>
	DPadLeft,

	/// <summary>The directional pad right button</summary>
	DPadRight,

	/// <summary>The first additional button</summary>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term>On Microsoft <em>Xbox Series X</em> controllers</term>
	///			<description><em>Share</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Sony <em>PlayStation 5</em> controllers</term>
	///			<description><em>Microphone</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>Switch Pro</em> controllers</term>
	///			<description><em>Capture</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Valve <em>Steam</em> controllers</term>
	///			<description><em>QAM</em> button</description>
	///		</item>		
	///		<item>
	///			<term>On Amazon <em>Luna</em> controllers</term>
	///			<description><em>Microphone</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Google <em>Stadia</em> controllers</term>
	///			<description><em>Capture</em> button</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	Miscellaneous1,

	/// <summary>The upper or primary paddle, located at the right hand</summary>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term>On Microsoft <em>Xbox Elite</em> controllers</term>
	///			<description>Paddle <em>P1</em></description>
	///		</item>
	///		<item>
	///			<term>On Sony <em>DualSense Edge</em> controllers</term>
	///			<description><em>RB</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>Switch right Joy-Con</em> controllers</term>
	///			<description><em>SR</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Valve <em>Steam</em> controllers</term>
	///			<description><em>R4</em> button</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	RightPaddle1,

	/// <summary>The upper or primary paddle, located at the left hand</summary>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term>On Microsoft <em>Xbox Elite</em> controllers</term>
	///			<description>Paddle <em>P3</em></description>
	///		</item>
	///		<item>
	///			<term>On Sony <em>DualSense Edge</em> controllers</term>
	///			<description><em>LB</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>Switch left Joy-Con</em> controllers</term>
	///			<description><em>SL</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Valve <em>Steam</em> controllers</term>
	///			<description><em>R4</em> button</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	LeftPaddle1,

	/// <summary>The lower or secondary paddle, located at the right hand</summary>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term>On Microsoft <em>Xbox Elite</em> controllers</term>
	///			<description>Paddle <em>P2</em></description>
	///		</item>
	///		<item>
	///			<term>On Sony <em>DualSense Edge</em> controllers</term>
	///			<description><em>Right Fn</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>Switch right Joy-Con</em> controllers</term>
	///			<description><em>SL</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Valve <em>Steam</em> controllers</term>
	///			<description><em>R5</em> button</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	RightPaddle2,

	/// <summary>The lower or secondary paddle, located at the left hand</summary>
	/// <remarks>
	/// <para>
	/// <list type="bullet">
	///		<item>
	///			<term>On Microsoft <em>Xbox Elite</em> controllers</term>
	///			<description>Paddle <em>P4</em></description>
	///		</item>
	///		<item>
	///			<term>On Sony <em>DualSense Edge</em> controllers</term>
	///			<description><em>Left Fn</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Nintendo <em>Switch left Joy-Con</em> controllers</term>
	///			<description><em>SR</em> button</description>
	///		</item>
	///		<item>
	///			<term>On Valve <em>Steam</em> controllers</term>
	///			<description><em>L5</em> button</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	LeftPaddle2,

	/// <summary>The touchpad button</summary>
	/// <remarks>
	/// <para>
	/// On Sony <em>PlayStation</em> controllers, this button is activated by pressing down on the touchpad.
	/// </para>
	/// </remarks>
	Touchpad,

	/// <summary>The second miscellaneous button</summary>
	Miscellaneous2,

	/// <summary>The third miscellaneous button</summary>
	/// <remarks>
	/// <para>
	/// On Nintendo <em>GameCube</em> controllers, this button is the left trigger "click" button.
	/// </para>
	/// </remarks>
	Miscellaneous3,

	/// <summary>The fourth miscellaneous button</summary>
	/// <remarks>
	/// <para>
	/// On Nintendo <em>GameCube</em> controllers, this button is the right trigger "click" button.
	/// </para>
	/// </remarks>
	Miscellaneous4,

	/// <summary>The fifth miscellaneous button</summary>
	Miscellaneous5,

	/// <summary>The sixth miscellaneous button</summary>
	Miscellaneous6
}
