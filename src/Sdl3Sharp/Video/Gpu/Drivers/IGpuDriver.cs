using System;
using System.Collections.Immutable;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.Video.Gpu.Drivers;

/// <summary>
/// Represents a GPU driver used to specify the GPU backend in <see cref="GpuDevice{TDriver}"/>
/// </summary>
/// <remarks>
/// <para> 
/// There are some pre-defined GPU drivers that SDL comes with.
/// Not all of them are necessarily available in every environment.
/// You can check the <see cref="AvailableDriverNames"/> property to see which GPU drivers are available in the current environment.
/// The GPU drivers that SDL comes with are:
/// <list type="bullet">
///		<item>
///			<term><see cref="Metal"/></term>
///			<description>Metal backend (only available on Apple platforms)</description>
///		</item>
///		<item>
///			<term><see cref="Direct3D12"/></term>
///			<description>Direct3D 12 backend (only available on Windows and Xbox platforms)</description>
///		</item>
///		<item>
///			<term><see cref="Vulkan"/></term>
///			<description>Vulkan backend (available on Windows, Linux, Nintendo Switch, and certain Android devices)</description>
///		</item>
/// </list>
/// </para>
/// </remarks>
public partial interface IGpuDriver
{
	// See Sdl3Sharp.Video.Rendering.Drivers.IRenderingDriver for more information about why this interface is designed this way and why we don't need something like CRTP here.

	// The available GPU driver list never changes because it's hard-compiled into the loaded build of SDL,
	// that's why we can cache it statically here
	private static ImmutableArray<string>? mAvailableDriverNames;

	/// <summary>
	/// Gets the list of names of available GPU drivers in the loaded build of SDL
	/// </summary>
	/// <value>
	/// The list of names of available GPU drivers in the loaded build of SDL
	/// </value>
	/// <remarks>
	/// <para>
	/// The list of available GPU drivers is only retrieved once and then cached afterwards,
	/// so the value of this property won't change during the lifetime of the application.
	/// </para>
	/// <para>
	/// The list is in the order that GPU drivers are normally checked during the initialization.
	/// </para>
	/// <para>
	/// The names of all pre-defined GPU drivers are all simple, low-ASCII identifiers, like "vulkan", "metal" or "direct3d12".
	/// These never have Unicode characters, and are not meant to be proper names.
	/// </para>
	/// <para>
	/// You can use the value of the <see cref="Name"/> properties of individual GPU driver types (e.g. <see cref="Vulkan.Name"/>) to check for the availability of a certain GPU driver;
	/// alternatively you can check the <see cref="GpuDriverExtensions.get_IsAvailable{TDriver}"/> property for that.
	/// </para>
	/// </remarks>
	public static ImmutableArray<string> AvailableDriverNames
	{
		get
		{
			return mAvailableDriverNames ??= buildAvailableDrivers();

			// build the available drivers list once
			static ImmutableArray<string> buildAvailableDrivers()
			{
				unsafe
				{
					var count = SDL_GetNumGPUDrivers();
					var builder = ImmutableArray.CreateBuilder<string>(count);

					for (var i = 0; i < count; i++)
					{
						builder.Add(Utf8StringMarshaller.ConvertToManaged(SDL_GetGPUDriver(i))!);
					}

					return builder.ToImmutable();
				}
			}
		}
	}

	/// <summary>
	/// Gets the name of the GPU driver
	/// </summary>
	/// <value>
	/// The name of the GPU driver, or <c><see langword="null"/></c> if the driver doesn't have a name (although all pre-defined windowing drivers do have a non-<c><see langword="null"/></c> name)
	/// </value>
	/// <remarks>
	/// <para>
	/// The names of all pre-defined windowing drivers are all simple, low-ASCII identifiers, like "vulkan", "metal" or "direct3d12".
	/// These never have Unicode characters, and are not meant to be proper names.
	/// </para>
	/// </remarks>
	static abstract string? Name { get; }

	internal static abstract ReadOnlySpan<byte> NameAscii { get; }
}
