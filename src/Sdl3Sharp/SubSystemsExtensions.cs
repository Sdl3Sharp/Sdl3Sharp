namespace Sdl3Sharp;

/// <summary>
/// Provides extensions methods and properties for <see cref="SubSystems"/>
/// </summary>
public static class SubSystemsExtensions
{
	extension(SubSystems subSystems)
	{
		/// <summary>
		/// Gets a value indicating whether the specified <see cref="SubSystems"/> are currently initialized
		/// </summary>
		/// <value>
		/// A value indicating whether the <see cref="SubSystems"/> are currently initialized
		/// </value>
		public bool IsInitialized => Sdl.SDL_WasInit(subSystems) == subSystems;
	}
}
