using Sdl3Sharp.Internal;

namespace Sdl3Sharp.Video.Windowing.Drivers;

/// <summary>
/// Provides extension methods and properties for <see cref="IWindowingDriver"/> implementing windowing driver types
/// </summary>
public static class WindowingDriverExtensions
{
	private static class Cache<TDriver>
		where TDriver : IWindowingDriver
	{
		// The available windowing driver list never changes during the lifetime of the application,
		// and, in addition to that, all pre-defined windowing driver types never change their name (it's essentially a compile-time constant),
		// so that's why we can cache the availability of each driver statically here.
		public static bool? IsAvailable;
	}

	extension<TDriver>(TDriver)
		where TDriver : IWindowingDriver
	{
		/// <summary>
		/// Gets a value indicating whether the windowing driver is currently active
		/// </summary>
		/// <value>
		/// A value indicating whether the windowing driver is currently active (i.e. the one that SDL is currently using for its video subsystem)
		/// </value>
		public static bool IsActive
		{
			get
			{
				unsafe
				{
					fixed (byte* name = TDriver.NameAscii)
					{
						return NativeStrings.Equals(name, IWindowingDriver.SDL_GetCurrentVideoDriver());
					}
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the windowing driver is available in the loaded build of SDL
		/// </summary>
		/// <value>
		/// A value indicating whether the windowing driver is available in the loaded build of SDL
		/// </value>
		/// <remarks>
		/// <para>
		/// The availability of a certain windowing driver is only checked once and then cached afterwards,
		/// so the value of this property for individual windowing drivers won't change during the lifetime of the application.
		/// </para>
		/// <para>
		/// This property effectively checks whether or not the name of the windowing driver is present in <see cref="IWindowingDriver.AvailableDriverNames"/>.
		/// </para>
		/// </remarks>
		public static bool IsAvailable => Cache<TDriver>.IsAvailable ??= TDriver.Name switch { string name => IWindowingDriver.AvailableDriverNames.Contains(name), _ => false };
	}
}
