namespace Sdl3Sharp.Input;

/// <summary>
/// Represents various types of sensors
/// </summary>
/// <remarks>
/// <para>
/// Additional sensors may be available, using platform dependent semantics.
/// These are additionally available Android sensors: <see href="https://developer.android.com/reference/android/hardware/SensorEvent.html#values"/>
/// </para>
/// <para>
/// <b>Notes on accelerometers:</b>
/// The accelerometer returns the current acceleration in SI meters per second squared.
/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
/// <list type="bullet">
///		<item>
///			<term><c>data[0]</c></term>
///			<description>Acceleration on the X axis</description>
///		</item>
///		<item>
///			<term><c>data[1]</c></term>
///			<description>Acceleration on the Y axis</description>
///		</item>
///		<item>
///			<term><c>data[2]</c></term>
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
///			<term><c>data[0]</c></term>
///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
///		</item>
///		<item>
///			<term><c>data[1]</c></term>
///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
///		</item>
///		<item>
///			<term><c>data[2]</c></term>
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
public enum SensorType
{
	/// <summary>Invalid sensor type</summary>
	Invalid = -1,

	/// <summary>Unknown sensor type</summary>
	Unknown,

	/// <summary>Accelerometer</summary>
	Accelerometer,

	/// <summary>Gyroscope</summary>
	Gyroscope,

	/// <summary>Accelerometer for a left Joy-Con controller or a Wii Nunchuk</summary>
	LeftAccelerometer,

	/// <summary>Gyroscope for a left Joy-Con controller</summary>
	LeftGyroscope,

	/// <summary>Accelerometer for a right Joy-Con controller</summary>
	RightAccelerometer,

	/// <summary>Gyroscope for a right Joy-Con controller</summary>
	RightGyroscope
}
