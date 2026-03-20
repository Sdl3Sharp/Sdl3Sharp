namespace Sdl3Sharp.Video.Gpu.Drivers;

/// <summary>
/// Provides extension methods and properties for <see cref="IGpuDriver"/> implementing GPU driver types
/// </summary>
public static class GpuDriverExtensions
{
	private static class Cache<TDriver>
		where TDriver : IGpuDriver
	{
		// The available GPU driver list never changes during the lifetime of the application,
		// and, in addition to that, all pre-defined GPU driver types never change their name (it's essentially a compile-time constant),
		// so that's why we can cache the availability of each driver statically here.
		public static bool? IsAvailable;
	}

	extension<TDriver>(TDriver)
		where TDriver : IGpuDriver
	{
		/// <summary>
		/// Gets a value indicating whether the GPU driver is available in the current environment
		/// </summary>
		/// <value>
		/// A value indicating whether the GPU driver is available in the current environment
		/// </value>
		/// <remarks>
		/// <para>
		/// The availability of a certain GPU driver is only checked once and then cached afterwards,
		/// so the value of this property for individual GPU drivers won't change during the lifetime of the application.
		/// </para>
		/// <para>
		/// This property effectively checks whether or not the name of the GPU driver is present in <see cref="IGpuDriver.AvailableDriverNames"/>.
		/// </para>
		/// </remarks>
		public static bool IsAvailable => Cache<TDriver>.IsAvailable ??= TDriver.Name switch { string name => IGpuDriver.AvailableDriverNames.Contains(name), _ => false };
	}
}
