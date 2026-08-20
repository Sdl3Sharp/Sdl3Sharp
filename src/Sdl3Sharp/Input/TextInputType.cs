using Sdl3Sharp.Video.Windowing;

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents a text input type
/// used <see cref="Window.TryStartTextInput(Sdl3Sharp.Input.TextInputType?, Sdl3Sharp.Input.Capitalization?, bool?, bool?, string?, string?, string?, int?, Sdl3Sharp.Properties?)">when starting text input with a <see cref="Window"/></see>
/// </summary>
/// <remarks>
/// <para>
/// Note that not all of the defined input types are supported on all platforms, but if a value is not supported, a reasonable fallback will be used instead.
/// </para>
/// </remarks>
public enum TextInputType
{
	/// <summary>The type of input is generic text</summary>
	Text,

	/// <summary>The type of input is a person's name</summary>
	TextName,

	/// <summary>The type of input is an email address</summary>
	TextEmail,

	/// <summary>The type of input is an username</summary>
	TextUsername,

	/// <summary>The type of input is a password that is hidden</summary>
	TextPasswordHidden,

	/// <summary>The type of input is a password that is visible</summary>
	TextPasswordVisible,

	/// <summary>The type of input is a number</summary>
	Number,

	/// <summary>The type of input is a PIN that is hidden</summary>
	NumberPasswordHidden,

	/// <summary>The type of input is a PIN that is visible</summary>
	NumberPasswordVisible
}
