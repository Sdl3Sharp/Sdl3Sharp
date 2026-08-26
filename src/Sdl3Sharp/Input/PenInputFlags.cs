using System;

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents a bitmask of pen input states
/// </summary>
[Flags]
public enum PenInputFlags : uint
{
	/// <summary>The pen is pressed down</summary>
	Down = 1u << 0,

	/// <summary>The pen's first button is pressed</summary>
	Button1 = 1u << PenButton.Button1,

	/// <summary>The pen's second button is pressed</summary>
	Button2 = 1u << PenButton.Button2,

	/// <summary>The pen's third button is pressed</summary>
	Button3 = 1u << PenButton.Button3,

	/// <summary>The pen's fourth button is pressed</summary>
	Button4 = 1u << PenButton.Button4,

	/// <summary>The pen's fifth button is pressed</summary>
	Button5 = 1u << PenButton.Button5,

	/// <summary>The pen's eraser tip is in use</summary>
	EraserTip = 1u << 30,

#if SDL3_4_0_OR_GREATER

	/// <summary>The pen is in proximity</summary>
	InProximity = 1u << 31

#endif
}
