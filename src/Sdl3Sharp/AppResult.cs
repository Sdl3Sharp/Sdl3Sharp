namespace Sdl3Sharp;

/// <summary>
/// Represents return values for an <see cref="App"/>'s main methods
/// </summary>
public enum AppResult
{
	/// <summary>A value that requests to continue from an <see cref="App"/>'s main methods</summary>
	Continue,

	/// <summary>A value that requests termination with success from an <see cref="App"/>'s main methods</summary>
	Success,

	/// <summary>A value that requests termination with error from an <see cref="App"/>'s main methods</summary>
	Failure
}
