using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Input;

partial class Sensor
{
	// Used for opaque pointers
	[StructLayout(LayoutKind.Sequential, Size = 0)]
	internal readonly struct SDL_Sensor;

	/// <summary>
	/// Closes a sensor previously opened with <see href="https://wiki.libsdl.org/SDL3/SDL_OpenSensor">SDL_OpenSensor</see>()
	/// </summary>
	/// <param name="sensor">The <see href="https://wiki.libsdl.org/SDL3/SDL_Sensor">SDL_Sensor</see> object to close</param>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CloseSensor">SDL_CloseSensor</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_CloseSensor(SDL_Sensor* sensor);

	/// <summary>
	/// Gets the current state of an opened sensor
	/// </summary>
	/// <param name="sensor">The <see href="https://wiki.libsdl.org/SDL3/SDL_Sensor">SDL_Sensor</see> object to query</param>
	/// <param name="data">A pointer filled with the current sensor state</param>
	/// <param name="num_values">The number of values to write to data</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// The number of values and interpretation of the data is sensor dependent.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetSensorData">SDL_GetSensorData</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetSensorData(SDL_Sensor* sensor, float* data, int num_values);

	/// <summary>
	/// Returns the <see href="https://wiki.libsdl.org/SDL3/SDL_Sensor">SDL_Sensor</see> associated with an instance ID
	/// </summary>
	/// <param name="instance_id">The sensor instance ID.</param>
	/// <returns>Returns an <see href="https://wiki.libsdl.org/SDL3/SDL_Sensor">SDL_Sensor</see> object or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetSensorFromID">SDL_GetSensorFromID</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Sensor* SDL_GetSensorFromID(uint instance_id);

	/// <summary>
	/// Gets the instance ID of a sensor
	/// </summary>
	/// <param name="sensor">The <see href="https://wiki.libsdl.org/SDL3/SDL_Sensor">SDL_Sensor</see> object to inspect</param>
	/// <returns>Returns the sensor instance ID, or 0 on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetSensorID">SDL_GetSensorID</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial uint SDL_GetSensorID(SDL_Sensor* sensor);

	/// <summary>
	/// Gets the implementation dependent name of a sensor
	/// </summary>
	/// <param name="sensor">The <see href="https://wiki.libsdl.org/SDL3/SDL_Sensor">SDL_Sensor</see> object</param>
	/// <returns>Returns the sensor name or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetSensorName">SDL_GetSensorName</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial byte* SDL_GetSensorName(SDL_Sensor* sensor);

	/// <summary>
	/// Gets the implementation dependent name of a sensor
	/// </summary>
	/// <param name="instance_id">The sensor instance ID</param>
	/// <returns>Returns the sensor name, or NULL if <c><paramref name="instance_id"/></c> is not valid</returns>
	/// <remarks>
	/// <para>
	/// This can be called before any sensors are opened.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetSensorNameForID">SDL_GetSensorNameForID</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial byte* SDL_GetSensorNameForID(uint instance_id);

	/// <summary>
	/// Gets the platform dependent type of a sensor
	/// </summary>
	/// <param name="sensor">The <see href="https://wiki.libsdl.org/SDL3/SDL_Sensor">SDL_Sensor</see> object to inspect</param>
	/// <returns>Returns the sensor platform dependent type, or -1 if <c><paramref name="sensor"/></c> is NULL</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetSensorNonPortableType">SDL_GetSensorNonPortableType</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial int SDL_GetSensorNonPortableType(SDL_Sensor* sensor);

	/// <summary>
	/// Gets the platform dependent type of a sensor
	/// </summary>
	/// <param name="instance_id">The sensor instance ID</param>
	/// <returns>Returns the sensor platform dependent type, or -1 if <c><paramref name="instance_id"/></c> is not valid</returns>
	/// <remarks>
	/// <para>
	/// This can be called before any sensors are opened.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetSensorNonPortableTypeForID">SDL_GetSensorNonPortableTypeForID</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial int SDL_GetSensorNonPortableTypeForID(uint instance_id);

	/// <summary>
	/// Gets the properties associated with a sensor
	/// </summary>
	/// <param name="sensor">The <see href="https://wiki.libsdl.org/SDL3/SDL_Sensor">SDL_Sensor</see> object</param>
	/// <returns>Returns a valid property ID on success or 0 on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetSensorProperties">SDL_GetSensorProperties</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial uint SDL_GetSensorProperties(SDL_Sensor* sensor);

	/// <summary>
	/// Gets a list of currently connected sensors
	/// </summary>
	/// <param name="count">A pointer filled in with the number of sensors returned, may be NULL</param>
	/// <returns>
	/// Returns a 0 terminated array of sensor instance IDs or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information.
	/// This should be freed with <see href="https://wiki.libsdl.org/SDL3/SDL_free">SDL_free</see>() when it is no longer needed.
	/// </returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetSensors">SDL_GetSensors</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial uint* SDL_GetSensors(int* count);

	/// <summary>
	/// Gets the type of a sensor
	/// </summary>
	/// <param name="sensor">The <see href="https://wiki.libsdl.org/SDL3/SDL_Sensor">SDL_Sensor</see> object to inspect</param>
	/// <returns>Returns the <see href="https://wiki.libsdl.org/SDL3/SDL_SensorType">SDL_SensorType</see> type, or <see href="https://wiki.libsdl.org/SDL3/SDL_SENSOR_INVALID"><c>SDL_SENSOR_INVALID</c></see> if <c><paramref name="sensor"/></c> is NULL</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetSensorType">SDL_GetSensorType</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SensorType SDL_GetSensorType(SDL_Sensor* sensor);

	/// <summary>
	/// Gets the type of a sensor
	/// </summary>
	/// <param name="instance_id">The sensor instance ID</param>
	/// <returns>Returns the <see href="https://wiki.libsdl.org/SDL3/SDL_SensorType">SDL_SensorType</see> type, or <see href="https://wiki.libsdl.org/SDL3/SDL_SENSOR_INVALID"><c>SDL_SENSOR_INVALID</c></see> if <c><paramref name="instance_id"/></c> is not valid</returns>
	/// <remarks>
	/// <para>
	/// This can be called before any sensors are opened.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetSensorTypeForID">SDL_GetSensorTypeForID</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial SensorType SDL_GetSensorTypeForID(uint instance_id);

	/// <summary>
	/// Opens a sensor for use
	/// </summary>
	/// <param name="instance_id">The sensor instance ID.</param>
	/// <returns>Returns an <see href="https://wiki.libsdl.org/SDL3/SDL_Sensor">SDL_Sensor</see> object or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_OpenSensor">SDL_OpenSensor</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Sensor* SDL_OpenSensor(uint instance_id);

	/// <summary>
	/// Updates the current state of the open sensors
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is called automatically by the event loop if sensor events are enabled.
	/// </para>
	/// <para>
	/// This needs to be called from the thread that initialized the sensor subsystem.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_UpdateSensors">SDL_UpdateSensors</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial void SDL_UpdateSensors();
}
