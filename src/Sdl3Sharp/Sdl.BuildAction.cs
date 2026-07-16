namespace Sdl3Sharp;

partial class Sdl
{
	/// <summary>
	/// Represents an action that is performed right before an <see cref="Sdl"/> instance is created. Use the provided <paramref name="builder"/> argument to perfom some preliminaries before an <see cref="Sdl"/> instance is created.
	/// </summary>
	/// <param name="builder">A <see cref="Builder"/> that lets you perfom some preliminaries right before an <see cref="Sdl"/> instance is created</param>
	public delegate void BuildAction(Builder builder);
}
