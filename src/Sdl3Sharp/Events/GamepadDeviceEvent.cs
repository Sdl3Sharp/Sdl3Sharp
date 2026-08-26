using Sdl3Sharp.Internal;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when a <see cref="Input.Gamepad"/> device is added or removed, when a <see cref="Input.Gamepad"/> device is remapped,
/// when a <see cref="Input.Gamepad"/> update is completed, or when a <see cref="Input.Gamepad"/>'s Steam handle is updated
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.GamepadAdded"/></description></item>
/// <item><description><see cref="EventType.GamepadRemoved"/></description></item>/// 
/// <item><description><see cref="EventType.GamepadRemapped"/></description></item>
/// <item><description><see cref="EventType.GamepadUpdateCompleted"/></description></item>
/// <item><description><see cref="EventType.GamepadSteamHandleUpdated"/></description></item>
/// </list>
/// </para>
/// <para>
/// SDL will send <see cref="EventType.JoystickAdded"/> (<see cref="JoyDeviceEvent"/>) as well as <see cref="EventType.GamepadAdded"/> (<see cref="GamepadDeviceEvent"/>) for all joystick devices that are recognized as gamepads.
/// </para>
/// <para>
/// SDL will send <see cref="EventType.GamepadAdded"/> (<see cref="GamepadDeviceEvent"/>) events for joystick devices recognized as gamepads that are already connected when <see cref="Sdl(Sdl3Sharp.Sdl.BuildAction?)">SDL is initialized</see>.
/// It will also send <see cref="EventType.GamepadAdded"/> (<see cref="GamepadDeviceEvent"/>) events for joystick devices that get gamepad mappings at runtime.
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct GamepadDeviceEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWhich;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="GamepadDeviceEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(GamepadDeviceEvent)}: {type}.", nameof(value));

				failInvalidEventType(value);
			}

			mCommon.Type = value;
		}
	}

	/// <inheritdoc/>
	public ulong Timestamp
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Timestamp;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mCommon.Timestamp = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Joystick.Id">ID</see> of the <see cref="Input.Gamepad"/> associated with this event
	/// </summary>
	/// <value>
	/// The <see cref="Joystick.Id">ID</see> of the <see cref="Input.Gamepad"/> associated with this event
	/// </value>
	public uint GamepadId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWhich;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWhich = value;
	}

	// TODO: Add a `Gamepad` property once the `Gamepad` type is implemented
	// Important note: `Gamepad`s are also `Joystick`s, but not all `Joystick`s are `Gamepad`s. This suggests that `Gamepad` should be a subclass of `Joystick`.
	// Also, this implies that `Gamepad` also use `Joystick` IDs, which is also what SDL does.

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {mCommon.ToPartialString()}, {nameof(GamepadId)}: {mWhich.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(GamepadId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWhich, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
