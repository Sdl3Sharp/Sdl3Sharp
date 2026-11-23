using System;

namespace Sdl3Sharp.Input;

/// <summary>
/// Provides extension methods for <see cref="Sensor"/>
/// </summary>
public static class SensorExtensions
{
	extension(Sensor sensor)
	{
		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first data value as if it was obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		/// </list>
		/// For phones and tablets held in natural orientation and game controllers held in front of you, the axes are defined as follows:
		/// <list type="bullet">
		///		<item>
		///			<term>-X … +X</term>
		///			<description>Left … Right</description>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		/// </list>
		/// For phones and tablets held in natural orientation and game controllers held in front of you, the axes are defined as follows:
		/// <list type="bullet">
		///		<item>
		///			<term>-X … +X</term>
		///			<description>Left … Right</description>
		///		</item>
		/// </list>
		/// The gyroscope axis data is not changed when the device is rotated.
		/// </para>
		/// </remarks>
		public bool TryGetData(
			out float data0
		)
		{
			Span<float> data = [ 0f ];
			var result = sensor.TryGetData(data);

			data0 = data[0];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first two data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
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
		/// </list>
		/// The accelerometer axis data is not changed when the device is rotated.
		/// </para>
		/// <para>
		/// <b>Notes on gyroscopes:</b>
		/// The gyroscope returns the current rate of rotation in radians per second. The rotation is positive in the counter-clockwise direction.
		/// That is, an observer looking from a positive location on one of the axes would see positive rotation on that axis when it appeared to be rotating counter-clockwise.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
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
		/// </list>
		/// The gyroscope axis data is not changed when the device is rotated.
		/// </para>
		/// </remarks>
		public bool TryGetData(
			out float data0, out float data1
		)
		{
			Span<float> data = [ 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0 = data[0];
			data1 = data[1];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first three data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0, out float data1, out float data2
		)
		{
			Span<float> data = [ 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0 = data[0];
			data1 = data[1];
			data2 = data[2];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first four data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0, out float data1, out float data2, out float data3
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0 = data[0];
			data1 = data[1];
			data2 = data[2];
			data3 = data[3];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <param name="data4">The fifth value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first five data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0, out float data1, out float data2, out float data3,
			out float data4
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0 = data[0];
			data1 = data[1];
			data2 = data[2];
			data3 = data[3];
			data4 = data[4];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <param name="data4">The fifth value of the sensor data</param>
		/// <param name="data5">The sixth value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first six data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0, out float data1, out float data2, out float data3,
			out float data4, out float data5
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0 = data[0];
			data1 = data[1];
			data2 = data[2];
			data3 = data[3];
			data4 = data[4];
			data5 = data[5];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <param name="data4">The fifth value of the sensor data</param>
		/// <param name="data5">The sixth value of the sensor data</param>
		/// <param name="data6">The seventh value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first seven data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0, out float data1, out float data2, out float data3,
			out float data4, out float data5, out float data6
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0 = data[0];
			data1 = data[1];
			data2 = data[2];
			data3 = data[3];
			data4 = data[4];
			data5 = data[5];
			data6 = data[6];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <param name="data4">The fifth value of the sensor data</param>
		/// <param name="data5">The sixth value of the sensor data</param>
		/// <param name="data6">The seventh value of the sensor data</param>
		/// <param name="data7">The eighth value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first eight data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0, out float data1, out float data2, out float data3,
			out float data4, out float data5, out float data6, out float data7
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0 = data[0];
			data1 = data[1];
			data2 = data[2];
			data3 = data[3];
			data4 = data[4];
			data5 = data[5];
			data6 = data[6];
			data7 = data[7];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <param name="data4">The fifth value of the sensor data</param>
		/// <param name="data5">The sixth value of the sensor data</param>
		/// <param name="data6">The seventh value of the sensor data</param>
		/// <param name="data7">The eighth value of the sensor data</param>
		/// <param name="data8">The ninth value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first nine data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0, out float data1, out float data2, out float data3,
			out float data4, out float data5, out float data6, out float data7,
			out float data8
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0 = data[0];
			data1 = data[1];
			data2 = data[2];
			data3 = data[3];
			data4 = data[4];
			data5 = data[5];
			data6 = data[6];
			data7 = data[7];
			data8 = data[8];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <param name="data4">The fifth value of the sensor data</param>
		/// <param name="data5">The sixth value of the sensor data</param>
		/// <param name="data6">The seventh value of the sensor data</param>
		/// <param name="data7">The eighth value of the sensor data</param>
		/// <param name="data8">The ninth value of the sensor data</param>
		/// <param name="data9">The tenth value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first ten data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0, out float data1, out float data2, out float data3,
			out float data4, out float data5, out float data6, out float data7,
			out float data8, out float data9
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0 = data[0];
			data1 = data[1];
			data2 = data[2];
			data3 = data[3];
			data4 = data[4];
			data5 = data[5];
			data6 = data[6];
			data7 = data[7];
			data8 = data[8];
			data9 = data[9];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <param name="data4">The fifth value of the sensor data</param>
		/// <param name="data5">The sixth value of the sensor data</param>
		/// <param name="data6">The seventh value of the sensor data</param>
		/// <param name="data7">The eighth value of the sensor data</param>
		/// <param name="data8">The ninth value of the sensor data</param>
		/// <param name="data9">The tenth value of the sensor data</param>
		/// <param name="data10">The eleventh value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first eleven data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0, out float data1, out float data2,  out float data3,
			out float data4, out float data5, out float data6,  out float data7,
			out float data8, out float data9, out float data10
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0  = data[0];
			data1  = data[1];
			data2  = data[2];
			data3  = data[3];
			data4  = data[4];
			data5  = data[5];
			data6  = data[6];
			data7  = data[7];
			data8  = data[8];
			data9  = data[9];
			data10 = data[10];
			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <param name="data4">The fifth value of the sensor data</param>
		/// <param name="data5">The sixth value of the sensor data</param>
		/// <param name="data6">The seventh value of the sensor data</param>
		/// <param name="data7">The eighth value of the sensor data</param>
		/// <param name="data8">The ninth value of the sensor data</param>
		/// <param name="data9">The tenth value of the sensor data</param>
		/// <param name="data10">The eleventh value of the sensor data</param>
		/// <param name="data11">The twelfth value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first twelve data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0, out float data1, out float data2,  out float data3,
			out float data4, out float data5, out float data6,  out float data7,
			out float data8, out float data9, out float data10, out float data11
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0  = data[0];
			data1  = data[1];
			data2  = data[2];
			data3  = data[3];
			data4  = data[4];
			data5  = data[5];
			data6  = data[6];
			data7  = data[7];
			data8  = data[8];
			data9  = data[9];
			data10 = data[10];
			data11 = data[11];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <param name="data4">The fifth value of the sensor data</param>
		/// <param name="data5">The sixth value of the sensor data</param>
		/// <param name="data6">The seventh value of the sensor data</param>
		/// <param name="data7">The eighth value of the sensor data</param>
		/// <param name="data8">The ninth value of the sensor data</param>
		/// <param name="data9">The tenth value of the sensor data</param>
		/// <param name="data10">The eleventh value of the sensor data</param>
		/// <param name="data11">The twelfth value of the sensor data</param>
		/// <param name="data12">The thirteenth value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first thirteen data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0,  out float data1, out float data2,  out float data3,
			out float data4,  out float data5, out float data6,  out float data7,
			out float data8,  out float data9, out float data10, out float data11,
			out float data12
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0  = data[0];
			data1  = data[1];
			data2  = data[2];
			data3  = data[3];
			data4  = data[4];
			data5  = data[5];
			data6  = data[6];
			data7  = data[7];
			data8  = data[8];
			data9  = data[9];
			data10 = data[10];
			data11 = data[11];
			data12 = data[12];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <param name="data4">The fifth value of the sensor data</param>
		/// <param name="data5">The sixth value of the sensor data</param>
		/// <param name="data6">The seventh value of the sensor data</param>
		/// <param name="data7">The eighth value of the sensor data</param>
		/// <param name="data8">The ninth value of the sensor data</param>
		/// <param name="data9">The tenth value of the sensor data</param>
		/// <param name="data10">The eleventh value of the sensor data</param>
		/// <param name="data11">The twelfth value of the sensor data</param>
		/// <param name="data12">The thirteenth value of the sensor data</param>
		/// <param name="data13">The fourteenth value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first fourteen data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0,  out float data1,  out float data2,  out float data3,
			out float data4,  out float data5,  out float data6,  out float data7,
			out float data8,  out float data9,  out float data10, out float data11,
			out float data12, out float data13
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);
			data0 = data[0];
			data1 = data[1];
			data2 = data[2];
			data3 = data[3];
			data4 = data[4];
			data5 = data[5];
			data6 = data[6];
			data7 = data[7];
			data8 = data[8];
			data9 = data[9];
			data10 = data[10];
			data11 = data[11];
			data12 = data[12];
			data13 = data[13];
			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <param name="data4">The fifth value of the sensor data</param>
		/// <param name="data5">The sixth value of the sensor data</param>
		/// <param name="data6">The seventh value of the sensor data</param>
		/// <param name="data7">The eighth value of the sensor data</param>
		/// <param name="data8">The ninth value of the sensor data</param>
		/// <param name="data9">The tenth value of the sensor data</param>
		/// <param name="data10">The eleventh value of the sensor data</param>
		/// <param name="data11">The twelfth value of the sensor data</param>
		/// <param name="data12">The thirteenth value of the sensor data</param>
		/// <param name="data13">The fourteenth value of the sensor data</param>
		/// <param name="data14">The fifteenth value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns the first fifteen data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0,  out float data1,  out float data2,  out float data3,
			out float data4,  out float data5,  out float data6,  out float data7,
			out float data8,  out float data9,  out float data10, out float data11,
			out float data12, out float data13, out float data14
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0  = data[0];
			data1  = data[1];
			data2  = data[2];
			data3  = data[3];
			data4  = data[4];
			data5  = data[5];
			data6  = data[6];
			data7  = data[7];
			data8  = data[8];
			data9  = data[9];
			data10 = data[10];
			data11 = data[11];
			data12 = data[12];
			data13 = data[13];
			data14 = data[14];

			return result;
		}

		/// <summary>
		/// Tries to get the current state of an opened sensor
		/// </summary>
		/// <param name="data0">The first value of the sensor data</param>
		/// <param name="data1">The second value of the sensor data</param>
		/// <param name="data2">The third value of the sensor data</param>
		/// <param name="data3">The fourth value of the sensor data</param>
		/// <param name="data4">The fifth value of the sensor data</param>
		/// <param name="data5">The sixth value of the sensor data</param>
		/// <param name="data6">The seventh value of the sensor data</param>
		/// <param name="data7">The eighth value of the sensor data</param>
		/// <param name="data8">The ninth value of the sensor data</param>
		/// <param name="data9">The tenth value of the sensor data</param>
		/// <param name="data10">The eleventh value of the sensor data</param>
		/// <param name="data11">The twelfth value of the sensor data</param>
		/// <param name="data12">The thirteenth value of the sensor data</param>
		/// <param name="data13">The fourteenth value of the sensor data</param>
		/// <param name="data14">The fifteenth value of the sensor data</param>
		/// <param name="data15">The sixteenth value of the sensor data</param>
		/// <remarks>
		/// <para>
		/// This method returns all sixteen data values as if they were obtained by calling <see cref="Sensor.TryGetData(Span{float})"/>.
		/// </para>
		/// <para>
		/// The actual number of sensible data values and their interpretation is sensor dependent.
		/// </para>
		/// <para>
		/// <b>Notes on accelerometers:</b>
		/// The accelerometer returns the current acceleration in SI meters per second squared.
		/// This measurement includes the force of gravity, so a device at rest will have an value of <see cref="Sensor.StandardGravity"/> away from the center of the earth, which is a positive Y value.
		/// <list type="bullet">
		///		<item>
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Acceleration on the X axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Acceleration on the Y axis</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		///			<term><c><paramref name="data0"/></c></term>
		///			<description>Angular speed around the X axis (<em>pitch</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data1"/></c></term>
		///			<description>Angular speed around the Y axis (<em>yaw</em>)</description>
		///		</item>
		///		<item>
		///			<term><c><paramref name="data2"/></c></term>
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
		public bool TryGetData(
			out float data0,  out float data1,  out float data2,  out float data3,
			out float data4,  out float data5,  out float data6,  out float data7,
			out float data8,  out float data9,  out float data10, out float data11,
			out float data12, out float data13, out float data14, out float data15
		)
		{
			Span<float> data = [ 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f ];
			var result = sensor.TryGetData(data);

			data0  = data[0];
			data1  = data[1];
			data2  = data[2];
			data3  = data[3];
			data4  = data[4];
			data5  = data[5];
			data6  = data[6];
			data7  = data[7];
			data8  = data[8];
			data9  = data[9];
			data10 = data[10];
			data11 = data[11];
			data12 = data[12];
			data13 = data[13];
			data14 = data[14];
			data15 = data[15];

			return result;
		}
	}
}
