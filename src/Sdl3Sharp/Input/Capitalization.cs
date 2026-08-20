using Sdl3Sharp.Video.Windowing;

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents an auto-capitalization type
/// used <see cref="Window.TryStartTextInput(Sdl3Sharp.Input.TextInputType?, Sdl3Sharp.Input.Capitalization?, bool?, bool?, string?, string?, string?, int?, Sdl3Sharp.Properties?)">when starting text input with a <see cref="Window"/></see>
/// </summary>
/// <remarks>
/// <para>
/// Note that not all of the defined input types are supported on all platforms, but if a value is not supported, a reasonable fallback will be used instead.
/// </para>
/// </remarks>
public enum Capitalization
{
	/// <summary>No auto-capitalization is applied</summary>
	None,

	/// <summary>The first letter of each sentence is automatically capitalized</summary>
	Sentences,

	/// <summary>The first letter of each word is automatically capitalized</summary>
	Words,

	/// <summary>All letters are automatically capitalized</summary>
	Letters
}
