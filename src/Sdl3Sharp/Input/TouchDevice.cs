using Sdl3Sharp.Events;
using Sdl3Sharp.Internal;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents a touch device that is currently connected to the system
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Id"/> of a touch device is unique, remains unchanged while the touch device is connected to the system, and is never reused for the lifetime of the application.
/// If a touch device is disconnected and then reconnected, it will be assigned a new <see cref="Id"/>.
/// </para>
/// <para>
/// The touch system, by default, will also send virtual mouse events; this can be useful for making a some desktop apps work on a phone without significant changes.
/// For apps that care about mouse and touch input separately, they should ignore mouse events with a <see cref="MouseButtonEvent.Mouse"/> or <see cref="MouseMotionEvent.Mouse"/> equal to <see cref="Mouse.SimulatedByTouch"/>.
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct TouchDevice :
	IEquatable<TouchDevice>, IFormattable, ISpanFormattable, IEqualityOperators<TouchDevice, TouchDevice, bool>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private readonly ulong mId;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal TouchDevice(ulong id) => mId = id;

	/// <summary>
	/// Gets a collection of all touch devices that are currently connected to the system
	/// </summary>
	/// <value>
	/// A collection of all touch devices that are currently connected to the system
	/// </value>
	/// <remarks>
	/// <para>
	/// On some platforms SDL first sees the touch device if it was actually used.
	/// Therefore the returned collection might be empty, although devices are available.
	/// After using all devices at least once the number will be correct.
	/// </para>
	/// <para>
	/// You should not query this property too often, as it may be expensive to retrieve the collection of connected touch devices.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the touch devices connected to the system (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public static TouchDevice[] ConnectedTouchDevices
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out int count);

				var touchDevices = SDL_GetTouchDevices(&count);

				if (touchDevices is null)
				{
					[DoesNotReturn]
					static void failCouldNotGetTouchDevices() => throw new SdlException($"Couldn't get the touch devices connected to the system.");

					failCouldNotGetTouchDevices();
				}

				try
				{
					if (count is not > 0)
					{
						return [];
					}

					var result = GC.AllocateUninitializedArray<TouchDevice>(count);

					var touchDevicePtr = touchDevices;
					foreach (ref var touchDevice in result.AsSpan())
					{
						touchDevice = new(*touchDevicePtr++);
					}

					return result;
				}
				finally
				{
					Utilities.NativeMemory.Free(touchDevices);
				}
			}
		}
	}

	/// <summary>
	/// Gets a collection of <em>snapshots</em> of the states of all active fingers on this touch device
	/// </summary>
	/// <value>
	/// A collection of <em>snapshots</em> of the states of all active fingers on this touch device
	/// </value>
	/// <remarks>
	/// <para>
	/// Please note that a <see cref="Finger"/> is a <em>snapshot</em> of the state of a finger on the touch device at the time of querying this property.
	/// The resulting <see cref="Finger"/> structures are not meant to track the state of the fingers on the touch device over time.
	/// If you want to track the state of the fingers on the touch device over time, it's not recommended to repeatedly poll this property, as that comes with a tremendous impact on performance.
	/// You should instead listen to the <see cref="EventType.FingerDown"/>, <see cref="EventType.FingerUp"/>, <see cref="EventType.FingerMotion"/>, and <see cref="EventType.FingerCanceled"/> (<see cref="TouchFingerEvent"/>) events,
	/// and implement your own tracking logic.
	/// </para>
	/// <para>
	/// You should not query this property too often, as it may be expensive to retrieve the collection of active fingers on the touch device.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the fingers for the touch device (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public readonly Finger[] Fingers
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out int count);

				var fingers = SDL_GetTouchFingers(mId, &count);

				if (fingers is null)
				{
					[DoesNotReturn]
					static void failCouldNotGetFingers(ulong id) => throw new SdlException($"Couldn't get the fingers for touch device {id}.");

					failCouldNotGetFingers(mId);
				}

				try
				{
					if (count is not > 0)
					{
						return [];
					}

					var result = GC.AllocateUninitializedArray<Finger>(count);

					var fingerPtr = fingers;
					foreach (ref var finger in result.AsSpan())
					{
						finger = *(*fingerPtr++);
					}

					return result;
				}
				finally
				{
					Utilities.NativeMemory.Free(fingers);
				}
			}
		}
	}

	/// <summary>
	/// Gets the numeric ID of this touch device
	/// </summary>
	/// <value>
	/// The numeric ID of this touch device
	/// </value>
	/// <remarks>
	/// <para>
	/// The <see cref="Id"/> of a touch device is unique, remains unchanged while the touch device is connected to the system, and is never reused for the lifetime of the application.
	/// If a touch device is disconnected and then reconnected, it will be assigned a new <see cref="Id"/>.
	/// </para>
	/// <para>
	/// An ID of <c>0</c> indicates an invalid touch device.
	/// </para>
	/// </remarks>
	public readonly ulong Id { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mId; }

	/// <summary>
	/// Gets the invalid <see cref="TouchDevice"/> with an <see cref="Id"/> of <c>0</c>
	/// </summary>
	/// <value>
	/// The invalid <see cref="TouchDevice"/> with an <see cref="Id"/> of <c>0</c>, i.e., <see cref="IsValid"/> will be <c><see langword="false"/></c> for this touch device
	/// </value>
	public static TouchDevice Invalid { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new(0); }

	/// <summary>
	/// Gets a value indicating whether this touch device is valid
	/// </summary>
	/// <value>
	/// A value indicating whether this touch device is valid, i.e., its <see cref="Id"/> is not <c>0</c>
	/// </value>
	public readonly bool IsValid { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mId is not 0; }

	/// <summary>
	/// Gets the name of this touch device
	/// </summary>
	/// <value>
	/// The name of this touch device, or <c><see langword="null"/></c> if the name couldn't be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	public readonly string? Name
	{
		get
		{
			unsafe
			{
				// To be consistent with how `Mouse.Name` works, we return null here if the touch device name is null, although that's considered an error by SDL.
				// Just like `Mouse.Name`, we document that the user should check for errors using `Error.TryGet` if the result is null.
				using var nameUtf16 = NativeStrings.FromUtf8ToUtf16(SDL_GetTouchDeviceName(mId));
				return nameUtf16.ToManaged();
			}
		}
	}

	/// <summary>
	/// Gets the <see cref="TouchDevice"/> instance that used for touch events that are simulated by mouse input
	/// </summary>
	/// <value>
	/// The <see cref="TouchDevice"/> instance that used for touch events that are simulated by mouse input
	/// </value>
	/// <remarks>
	/// <para>
	/// You can use the value of property to compare against <see cref="TouchFingerEvent.TouchDevice"/>
	/// to determine whether the touch event was simulated by mouse input.
	/// </para>
	/// </remarks>
	public static TouchDevice SimulatedByMouse { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new(ulong.MaxValue /* ((ulong)-1L) */); }

	/// <summary>
	/// Gets the <see cref="TouchDevice"/> instance that used for touch events that are simulated by pen input
	/// </summary>
	/// <value>
	/// The <see cref="TouchDevice"/> instance that used for touch events that are simulated by pen input
	/// </value>
	/// <remarks>
	/// <para>
	/// You can use the value of property to compare against <see cref="TouchFingerEvent.TouchDevice"/>
	/// to determine whether the touch event was simulated by pen input.
	/// </para>
	/// </remarks>
	public static TouchDevice SimulatedByPen { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => new(ulong.MaxValue - 1 /* ((ulong)-2L) */); }

	/// <summary>
	/// Gets the type of this touch device
	/// </summary>
	/// <value>
	/// The type of this touch device
	/// </value>
	public readonly TouchDeviceType Type => SDL_GetTouchDeviceType(mId);

	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is TouchDevice other && Equals(other);

	/// <inheritdoc/>
	public readonly bool Equals(TouchDevice other) => mId == other.mId;

	/// <summary>
	/// Gets a <see cref="TouchDevice"/> by its numeric ID
	/// </summary>
	/// <param name="id">The numeric ID of the touch device, or <c>0</c> to return <see cref="Invalid"/></param>
	/// <returns>The <see cref="TouchDevice"/> with the specified <paramref name="id"/>, or <see cref="Invalid"/> if <paramref name="id"/> was <c>0</c></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static TouchDevice FromId(uint id) => new(id);

	/// <inheritdoc/>
	public readonly override int GetHashCode() => mId.GetHashCode();

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> mId is not 0
			? $"{{ {nameof(Id)}: {mId.ToString(format, formatProvider)}, {
				nameof(Name)}: {Name switch { not null and var name => $"\"{name}\"", _ => "null" }} }}"
			: $"Invalid {nameof(TouchDevice)}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		unsafe
		{
			charsWritten = 0;

			if (mId is 0)
			{
				return SpanFormat.TryWrite($"Invalid {nameof(TouchDevice)}", ref destination, ref charsWritten);
			}

			if (!(SpanFormat.TryWrite($"{{ {nameof(Id)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mId, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(Name)}: ", ref destination, ref charsWritten)))
			{
				return false;
			}

			var name = SDL_GetTouchDeviceName(mId);

			if (name is not null)
			{
				if (!(SpanFormat.TryWrite('"', ref destination, ref charsWritten)
					&& SpanFormat.TryWriteUtf8(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(name), ref destination, ref charsWritten)
					&& SpanFormat.TryWrite('"', ref destination, ref charsWritten)))
				{
					return false;
				}
			}
			else
			{
				if (!SpanFormat.TryWrite("null", ref destination, ref charsWritten))
				{
					return false;
				}
			}

			return SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
		}
	}

	/// <inheritdoc/>
	public static bool operator ==(TouchDevice left, TouchDevice right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(TouchDevice left, TouchDevice right) => !(left == right);
}
