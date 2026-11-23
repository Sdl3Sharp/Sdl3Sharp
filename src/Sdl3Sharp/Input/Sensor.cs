using Sdl3Sharp;
using Sdl3Sharp.Internal;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents a sensor connected to the system
/// </summary>
/// <remarks>
/// <para>
/// In order to use the <see cref="Sensor"/> functionality, the <see cref="SubSystems.Sensor"/> subsystem must have been <see cref="Sdl.Sdl(SubSystems)">initialized</see>,
/// as that will cause SDL to scan for connected sensors and load the appropriate drivers for them.
/// </para>
/// <para>
/// The <see cref="Id"/> of a sensor is unique, remains unchanged while the sensor is connected to the system, and is never reused for the lifetime of the application.
/// </para>
/// </remarks>
public sealed partial class Sensor : IDisposable
{
	/// <summary>
	/// The standard acceleration accelerometer sensors report due to gravity, in SI meters per second squared
	/// </summary>
	/// <remarks>
	/// <para>
	/// A resting device will report an acceleration of <see cref="StandardGravity"/> away from the center of the earth, which is a positive Y value.
	/// </para>
	/// </remarks>
	public const float StandardGravity = 9.80665f;

	private static readonly ConcurrentDictionary<uint, WeakReference<Sensor>> mKnownInstances = new(IdEqualityComparer.Instance);
	// private static readonly ConcurrentDictionary<uint, WeakReference<Sensor>>.AlternateLookup<IntPtr> mKnownInstancesByPointer = mKnownInstances.GetAlternateLookup<IntPtr>();

	private unsafe SDL_Sensor* mSensor;
	private ulong mRefCounter = 0;

	private unsafe Sensor(SDL_Sensor* sensor, bool register)
	{
		mSensor = sensor;
		mRefCounter = 1;

		if (register)
		{
			if (sensor is not null && SDL_GetSensorID(sensor) is not 0 and var id)
			{
				static WeakReference<Sensor> addRef(uint id, Sensor newSensor) => new(newSensor);

				static WeakReference<Sensor> updateRef(uint id, WeakReference<Sensor> existingSensorRef, Sensor newSensor)
				{
					if (existingSensorRef.TryGetTarget(out var existingSensor))
					{
#pragma warning disable IDE0079
#pragma warning disable CA1816
						GC.SuppressFinalize(existingSensor);
#pragma warning restore CA1816
#pragma warning restore IDE0079

						// Disposing the existing sensor also closes it and decreases the native ref counter
						existingSensor.Dispose(forget: false);
					}

					existingSensorRef.SetTarget(newSensor);

					return existingSensorRef;
				}

				mKnownInstances.AddOrUpdate(id, addRef, updateRef, this);
			}
		}
	}

	/// <inheritdoc/>
	~Sensor() =>Dispose(forget: true);

	/// <summary>
	/// Gets a collection of the numeric IDs of all sensors that are connected to the system
	/// </summary>
	/// <value>
	/// A collection of the numeric IDs of all sensors that are connected to the system
	/// </value>
	/// <remarks>
	/// <para>
	/// For efficiency reasons, this property only retrieves the numeric IDs of the connected sensors and doesn't open them for you.
	/// You can use the <see cref="TryOpenFromId(uint, out Sensor?)"/> method to open a sensor from its numeric ID.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the sensors connected to the system (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public static uint[] ConnectedSensorIds
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out int count);

				var sensors = SDL_GetSensors(&count);				

				if (sensors is null)
				{
					[DoesNotReturn]
					static void failCouldNotGetSensors() => throw new SdlException("Couldn't get the sensors connected to the system");

					failCouldNotGetSensors();
				}

				try
				{
					if (count is not > 0)
					{
						return [];
					}

					var result = GC.AllocateUninitializedArray<uint>(count);

					MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<uint>(sensors), count).CopyTo(result);

					return result;
				}
				finally
				{
					Utilities.NativeMemory.SDL_free(sensors);
				}
			}
		}
	}

	/// <summary>
	/// Gets the numeric ID of this opened sensor
	/// </summary>
	/// <value>
	/// The numeric ID of this opened sensor
	/// </value>
	/// <remarks>
	/// <para>
	/// The <see cref="Id"/> of a sensor is unique, remains unchanged while the sensor is connected to the system, and is never reused for the lifetime of the application.
	/// </para>
	/// <para>
	/// An ID of <c>0</c> indicates an invalid or closed sensor.
	/// </para>
	/// </remarks>
	public uint Id
	{
		get
		{
			unsafe
			{
				return SDL_GetSensorID(mSensor);
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether this sensor is valid and opened
	/// </summary>
	/// <value>
	/// A value indicating whether this sensor is valid and opened, i.e., whether its <see cref="Id"/> is not <c>0</c>
	/// </value>
	public bool IsValid => Id is not 0;

	/// <summary>
	/// Gets the implementation dependent name of this opened sensor
	/// </summary>
	/// <value>
	/// The implementation dependent name of this opened sensor, or <c><see langword="null"/></c> if the name couldn't be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	public string? Name
	{
		get
		{
			unsafe
			{
				// don't fail on a null pointer, just return whatever SDL_GetSensorName, since there are multiple reasons for SDL_GetSensorName to fail
				// we just doc that the user should call Error.TryGet() whenever they get null back from this property
				using var nameUtf8 = NativeStrings.FromUtf8ToUtf16(SDL_GetSensorName(mSensor));
				return nameUtf8.ToManaged();
			}
		}
	}

	/// <summary>
	/// Gets the platform dependent type of this opened sensor
	/// </summary>
	/// <value>
	/// The platform dependent type of this opened sensor, or <c>-1</c> if the sensor is <see cref="IsValid">invalid</see>
	/// </value>
	public int NonPortableType
	{
		get
		{
			unsafe
			{
				return SDL_GetSensorNonPortableType(mSensor);
			}
		}
	}

	/// <summary>
	/// Gets the properties associated with this opened sensor
	/// </summary>
	/// <value>
	/// The properties associated with this opened sensor, or <c><see langword="null"/></c> if the properties could not be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	public Properties? Properties
	{
		get
		{
			unsafe
			{
				return SDL_GetSensorProperties(mSensor) switch
				{
					0 => null,
					var id => Properties.GetOrCreate(id, registerWithSdl: false)
				};
			}
		}
	}

	/// <summary>
	/// Gets the type of this opened sensor
	/// </summary>
	/// <value>
	/// The type of this opened sensor, or <c><see cref="SensorType.Invalid"/></c> if the sensor is <see cref="IsValid">invalid</see>
	/// </value>
	public SensorType Type
	{
		get
		{
			unsafe
			{
				return SDL_GetSensorType(mSensor);
			}
		}
	}

	/// <summary>
	/// Disposes and closes this opened sensor
	/// </summary>
	/// <remarks>
	/// <para>
	/// Note that you should <em>always</em> <see cref="Dispose()">dispose and close</see> a sensor you <see cref="TryOpenFromId(uint, out Sensor?)">opened</see> yourself when you don't need it anymore.
	/// You don't need to worry about other components that might have opened the same sensor, as the sensor is reference counted and will only be finally closed and released once all references to it have been disposed.
	/// </para>
	/// </remarks>
	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(forget: true);
	}

	private void Dispose(bool forget)
	{
		unsafe
		{
			if (mSensor is not null)
			{
				if (Interlocked.Decrement(ref mRefCounter) is > 0)
				{
					return;
				}

				if (forget)
				{
					mKnownInstances.TryRemove(unchecked(SDL_GetSensorID(mSensor)), out _);
				}

				SDL_CloseSensor(mSensor);
				mSensor = null;
			}
		}
	}

	/// <summary>
	/// Tries to get the current state of this opened sensor
	/// </summary>
	/// <param name="data">A span to receive the sensor data</param>
	/// <returns><c><see langword="true"/></c>, if the data was successfully retrieved; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method always writes as many values to <paramref name="data"/> as its <see cref="Span{T}.Length">length</see>, but at most 16 values.
	/// </para>
	/// <para>
	/// The actual number of sensible data values and their interpretation is sensor dependent.
	/// </para>
	/// <para>
	/// <b>Notes on accelerometers:</b>
	/// The accelerometer returns the current acceleration in SI meters per second squared.
	/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="StandardGravity"/> away from the center of the earth, which is a positive Y value.
	/// <list type="bullet">
	///		<item>
	///			<term><c><paramref name="data"/>[0]</c></term>
	///			<description>Acceleration on the X axis</description>
	///		</item>
	///		<item>
	///			<term><c><paramref name="data"/>[1]</c></term>
	///			<description>Acceleration on the Y axis</description>
	///		</item>
	///		<item>
	///			<term><c><paramref name="data"/>[2]</c></term>
	///			<description>Acceleration on the Z axis</description>
	///		</item>
	/// </list>
	/// For phones and tablets held in natural orientation and game controllers held in front of you, the axes are defined as follows:
	/// <list type="bullet">
	///		<item>
	///			<term>-X … +X</term>
	///			<description>Left … Right</description>
	///		</item>
	///		<item>
	///			<term>-Y … +Y</term>
	///			<description>Bottom … Top</description>
	///		</item>
	///		<item>
	///			<term>-Z … +Z</term>
	///			<description>Farther … Closer</description>
	///		</item>
	/// </list>
	/// The accelerometer axis data is not changed when the device is rotated.
	/// </para>
	/// <para>
	/// <b>Notes on gyroscopes:</b>
	/// The gyroscope returns the current rate of rotation in radians per second. The rotation is positive in the counter-clockwise direction.
	/// That is, an observer looking from a positive location on one of the axes would see positive rotation on that axis when it appeared to be rotating counter-clockwise.
	/// <list type="bullet">
	///		<item>
	///			<term><c><paramref name="data"/>[0]</c></term>
	///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
	///		</item>
	///		<item>
	///			<term><c><paramref name="data"/>[1]</c></term>
	///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
	///		</item>
	///		<item>
	///			<term><c><paramref name="data"/>[2]</c></term>
	///			<description>Angular speed around the Z axis (<em>roll</em>)</description>
	///		</item>
	/// </list>
	/// For phones and tablets held in natural orientation and game controllers held in front of you, the axes are defined as follows:
	/// <list type="bullet">
	///		<item>
	///			<term>-X … +X</term>
	///			<description>Left … Right</description>
	///		</item>
	///		<item>
	///			<term>-Y … +Y</term>
	///			<description>Bottom … Top</description>
	///		</item>
	///		<item>
	///			<term>-Z … +Z</term>
	///			<description>Farther … Closer</description>
	///		</item>
	/// </list>
	/// The gyroscope axis data is not changed when the device is rotated.
	/// </para>
	/// </remarks>
	public bool TryGetData(Span<float> data)
	{
		unsafe
		{
			fixed (float* dataPtr = data)
			{
				// Note: SDL_GetSensorData does neither fail if the provided buffer is too small nor fail if it is too large.
				// It just always writes the minimum number of elements between the provided buffer's length and the fixed array length of the sensor's data, which is 16.
				// This behavior definitely needs to be doc'd!
				return SDL_GetSensorData(mSensor, dataPtr, data.Length);
			}
		}
	}

	internal unsafe static bool TryGetOrCreate(SDL_Sensor* sensor, [NotNullWhen(true)] out Sensor? result)
	{
		if (sensor is null)
		{
			result = null;
			return false;
		}

		var id = SDL_GetSensorID(sensor);

		if (id is 0)
		{
			result = null;
			return false;
		}

		while (true)
		{
			var isNewInstance = false;

			Sensor create(IntPtr sensor)
			{
				var result = new Sensor(unchecked((SDL_Sensor*)sensor), register: false);
				isNewInstance = true;
				return result;
			}

			WeakReference<Sensor> createRef(uint id, IntPtr sensor) => new(create(sensor));

			var sensorRef = mKnownInstances.GetOrAdd(id, createRef, unchecked((IntPtr)sensor));

			if (!sensorRef.TryGetTarget(out result))
			{
				sensorRef.SetTarget(result = create(unchecked((IntPtr)sensor)));
			}

			if (!isNewInstance)
			{
				if (Interlocked.Read(ref result.mRefCounter) is not > 0)
				{
					// this is the very rare case where the sensor was disposed in between the time we obtained the reference
					// and before we could increment the ref count, so we need to try again
					continue;
				}

				// new instances already start with a ref count of 1, but for existing instances we need to increment the ref counter manually
				Interlocked.Increment(ref result.mRefCounter);
			}

			return true;
		}
	}

	/// <summary>
	/// Tries to get the implementation dependent name of a sensor from its numeric ID
	/// </summary>
	/// <param name="id">The numeric ID of the sensor</param>
	/// <param name="name">The implementation dependent name of the sensor, or <c><see langword="null"/></c> if it couldn't be retrieved successfully</param>
	/// <returns><c><see langword="true"/></c> if the name was successfully retrieved; otherwise, <c><see langword="false"/></c> (the given <paramref name="id"/> is most likely invalid)</returns>
	/// <remarks>
	/// <para>
	/// This method can be called before any sensors are opened.
	/// </para>
	/// </remarks>
	public static bool TryGetNameFromId(uint id, [NotNullWhen(true)] out string? name)
	{
		unsafe
		{
			using (var nameUtf8 = NativeStrings.FromUtf8ToUtf16(SDL_GetSensorNameForID(id)))
			{
				name = nameUtf8.ToManaged();
			}

			return name is not null;
		}
	}

	/// <summary>
	/// Tries to get the platform dependent type of a sensor from its numeric ID
	/// </summary>
	/// <param name="id">The numeric ID of the sensor</param>
	/// <param name="nonPortableType">The platform dependent type of the sensor, or <c>-1</c> if the given <paramref name="id"/> is invalid</param>
	/// <returns><c><see langword="true"/></c> if the type was successfully retrieved; otherwise, <c><see langword="false"/></c> (the given <paramref name="id"/> is invalid)</returns>
	/// <remarks>
	/// <para>
	/// This method can be called before any sensors are opened.
	/// </para>
	/// </remarks>
	public static bool TryGetNonPortableTypeFromId(uint id, out int nonPortableType)
	{
		nonPortableType = SDL_GetSensorNonPortableTypeForID(id);

		return nonPortableType is not -1;
	}

	/// <summary>
	/// Tries to get the type of a sensor from its numeric ID
	/// </summary>
	/// <param name="id">The numeric ID of the sensor</param>
	/// <param name="type">The type of the sensor, or <see cref="SensorType.Invalid"/> if the given <paramref name="id"/> is invalid</param>
	/// <returns><c><see langword="true"/></c> if the type was successfully retrieved; otherwise, <c><see langword="false"/></c> (the given <paramref name="id"/> is invalid)</returns>
	/// <remarks>
	/// <para>
	/// This method can be called before any sensors are opened.
	/// </para>
	/// </remarks>
	public static bool TryGetTypeFromId(uint id, out SensorType type)
	{
		type = SDL_GetSensorTypeForID(id);

		return type is not SensorType.Invalid;
	}

	/// <summary>
	/// Tries to open the sensor with the specified numeric ID
	/// </summary>
	/// <param name="id">The numeric ID of the sensor to open</param>
	/// <param name="sensor">The opened sensor, or <c><see langword="null"/></c> if the sensor could not be opened</param>
	/// <returns><c><see langword="true"/></c> if the sensor was successfully opened; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// It is safe to open the same sensor multiple times, even if it was already opened by another component.
	/// </para>
	/// <para>
	/// Note that you should <em>always</em> <see cref="Dispose()">dispose and close</see> a sensor you <see cref="TryOpenFromId(uint, out Sensor?)">opened</see> yourself when you don't need it anymore.
	/// You don't need to worry about other components that might have opened the same sensor, as the sensor is reference counted and will only be finally closed and released once all references to it have been disposed.
	/// </para>
	/// </remarks>
	public static bool TryOpenFromId(uint id, [NotNullWhen(true)] out Sensor? sensor)
	{
		unsafe
		{
			return TryGetOrCreate(SDL_OpenSensor(id), out sensor);
		}
	}

	/// <summary>
	/// Updates the current state of all opened sensors
	/// </summary>
	/// <remarks>
	/// <para>
	/// This method is automatically called by the event loop if sensor events are enabled.
	/// </para>
	/// <para>
	/// This method should only be called from the thread that initialized the sensor subsystem.
	/// </para>
	/// </remarks>
	public static void UpdateSensors()
	{
		unsafe
		{
			SDL_UpdateSensors();
		}
	}
}
