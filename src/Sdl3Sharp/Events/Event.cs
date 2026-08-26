using Sdl3Sharp.Internal;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

#if NET11_0_OR_GREATER
/// <summary>
/// Represents a common base type for event structures as an union type
/// </summary>
/// <remarks>
/// <para>
/// You can use pattern matching (e.g., <c><see langword="switch"/></c>) or type checking (e.g., <c><see langword="is"/></c>) to determine the actual event type structure and access its specific properties.
/// Alternatively, you can use the <see cref="TryGet{TEvent}(out TEvent)"/> and <see cref="From{TEvent}(in TEvent)"/> methods to check and convert between the union type and the specific event type structures.
/// </para>
/// </remarks>
#else
/// <summary>
/// Represents a common base type for event type structures as an union type
/// </summary>
/// <remarks>
/// <para>
/// You can use the <see cref="TryGet{TEvent}(out TEvent)"/> and <see cref="From{TEvent}(in TEvent)"/> methods to check and convert between the union type and the specific event type structures.
/// </para>
/// </remarks>
#endif
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Explicit)]
public partial struct Event : IFormattable, ISpanFormattable
{
	[InlineArray(128)] private struct Padding { private byte _; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	[FieldOffset(0)] private EventType mType;
	[FieldOffset(0)] private readonly Padding mPadding; // This makes sure the struct is (at least) 128 bytes in size, just like it's done in SDL's C counterpart `SDL_Event`.
	                                                    // See the comment in the definition in https://wiki.libsdl.org/SDL3/SDL_Event for their explanation of why this is done that way.

	/// <inheritdoc/>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mType;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mType = value;
	}

	/// <inheritdoc/>
	public ulong Timestamp
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => Common.Timestamp;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => Common.Timestamp = value;
	}

	/// <summary>
	/// Creates a new <see cref="Event"/> from the given <typeparamref name="TEvent"/>
	/// </summary>
	/// <typeparam name="TEvent">The type of the event to create the <see cref="Event"/> from</typeparam>
	/// <param name="event">The event to create the <see cref="Event"/> from</param>
	/// <returns>A new <see cref="Event"/> representing the given <typeparamref name="TEvent"/></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static Event From<TEvent>(in TEvent @event)
		where TEvent : notnull, IEvent<TEvent>
	{
		Unsafe.SkipInit(out Event result);

		@event.WriteToEvent(ref result);

		return result;
	}

	/// <summary>
	/// Tries to get a <typeparamref name="TEvent"/> from this <see cref="Event"/>, if this <see cref="Event"/> is of the correct type
	/// </summary>
	/// <typeparam name="TEvent">The type of the event to try to get from this <see cref="Event"/></typeparam>
	/// <param name="event">The event to try to get from this <see cref="Event"/>, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c> or an uninitialized value</param>
	/// <returns><c><see langword="true"/></c>, if this <see cref="Event"/> is of the correct type and the <paramref name="event"/> was successfully retrieved; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// Do <em>not</em> use the resulting <paramref name="event"/> if this method returns <c><see langword="false"/></c>, as it might be uninitialized and may contain invalid data.
	/// </para>
	/// </remarks>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly bool TryGet<TEvent>([NotNullWhen(true)] out TEvent? @event)
		where TEvent : notnull, IEvent<TEvent>
		=> TEvent.TryReadFromEvent(in this, out @event);

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		// Can't use union type pattern matching here, since that's a .NET 11+ feature, and we want to support .NET 10 as well (possibly even with the same code without `#if` directives).
		// That's why we make use of the `TryGet<TEvent>(out TEvent)` method and a big if-else-if-...-else-chain instead.
		// TODO: add cases
		=> TryGet(out DisplayEvent displayEvent)                             ? $"{nameof(DisplayEvent)} {displayEvent}"
		 : TryGet(out WindowEvent windowEvent)                               ? $"{nameof(WindowEvent)} {windowEvent}"
		 : TryGet(out KeyboardDeviceEvent keyboardDeviceEvent)               ? $"{nameof(KeyboardDeviceEvent)} {keyboardDeviceEvent}"
		 : TryGet(out KeyboardEvent keyboardEvent)                           ? $"{nameof(KeyboardEvent)} {keyboardEvent}"
		 : TryGet(out TextEditingEvent textEditingEvent)                     ? $"{nameof(TextEditingEvent)} {textEditingEvent}"
		 : TryGet(out TextEditingCandidatesEvent textEditingCandidatesEvent) ? $"{nameof(TextEditingCandidatesEvent)} {textEditingCandidatesEvent}"
		 : TryGet(out TextInputEvent textInputEvent)                         ? $"{nameof(TextInputEvent)} {textInputEvent}"
		 : TryGet(out MouseDeviceEvent mouseDeviceEvent)                     ? $"{nameof(MouseDeviceEvent)} {mouseDeviceEvent}"
		 : TryGet(out MouseMotionEvent mouseMotionEvent)                     ? $"{nameof(MouseMotionEvent)} {mouseMotionEvent}"
		 : TryGet(out MouseButtonEvent mouseButtonEvent)                     ? $"{nameof(MouseButtonEvent)} {mouseButtonEvent}"
		 : TryGet(out MouseWheelEvent mouseWheelEvent)                       ? $"{nameof(MouseWheelEvent)} {mouseWheelEvent}"
		 : TryGet(out JoyDeviceEvent joyDeviceEvent)                         ? $"{nameof(JoyDeviceEvent)} {joyDeviceEvent}"
		 : TryGet(out JoyAxisEvent joyAxisEvent)                             ? $"{nameof(JoyAxisEvent)} {joyAxisEvent}"
		 : TryGet(out JoyBallEvent joyBallEvent)                             ? $"{nameof(JoyBallEvent)} {joyBallEvent}"
		 : TryGet(out JoyHatEvent joyHatEvent)                               ? $"{nameof(JoyHatEvent)} {joyHatEvent}"
		 : TryGet(out JoyButtonEvent joyButtonEvent)                         ? $"{nameof(JoyButtonEvent)} {joyButtonEvent}"
		 : TryGet(out JoyBatteryEvent joyBatteryEvent)                       ? $"{nameof(JoyBatteryEvent)} {joyBatteryEvent}"
		 : TryGet(out GamepadDeviceEvent gamepadDeviceEvent)                 ? $"{nameof(GamepadDeviceEvent)} {gamepadDeviceEvent}"
		 : TryGet(out GamepadAxisEvent gamepadAxisEvent)                     ? $"{nameof(GamepadAxisEvent)} {gamepadAxisEvent}"
		 : TryGet(out GamepadButtonEvent gamepadButtonEvent)                 ? $"{nameof(GamepadButtonEvent)} {gamepadButtonEvent}"
		 : TryGet(out GamepadTouchpadEvent gamepadTouchpadEvent)             ? $"{nameof(GamepadTouchpadEvent)} {gamepadTouchpadEvent}"
		 : TryGet(out GamepadSensorEvent gamepadSensorEvent) 				 ? $"{nameof(GamepadSensorEvent)} {gamepadSensorEvent}"
#if SDL3_6_0_OR_GREATER
		 : TryGet(out GamepadCapSenseEvent gamepadCapSenseEvent)             ? $"{nameof(GamepadCapSenseEvent)} {gamepadCapSenseEvent}"
#endif
		 : TryGet(out AudioDeviceEvent audioDeviceEvent)                     ? $"{nameof(AudioDeviceEvent)} {audioDeviceEvent}"
		 : TryGet(out CameraDeviceEvent cameraDeviceEvent)                   ? $"{nameof(CameraDeviceEvent)} {cameraDeviceEvent}"
		 : TryGet(out SensorEvent sensorEvent)                               ? $"{nameof(SensorEvent)} {sensorEvent}"
		 : TryGet(out QuitEvent quitEvent)                                   ? $"{nameof(QuitEvent)} {quitEvent}"
		 : TryGet(out UserEvent? userEvent)                                  ? $"{userEvent.GetType().Name} {userEvent}" // `UserEvent` must be matched before `UnmanagedUserEvent`, because both accept the same event types, but `UserEvent` has more constraints, so it should be handled first
		 : TryGet(out UnmanagedUserEvent unmanagedUserEvent)                 ? $"{nameof(UnmanagedUserEvent)} {unmanagedUserEvent}" // `UnmanagedUserEvent` must be matched after `UserEvent`, because both accept the same event types, but `UserEvent` has more constraints, so it should be handled first
		 : TryGet(out TouchFingerEvent touchFingerEvent)                     ? $"{nameof(TouchFingerEvent)} {touchFingerEvent}"
#if SDL3_4_0_OR_GREATER
		 : TryGet(out PinchFingerEvent pinchFingerEvent)                     ? $"{nameof(PinchFingerEvent)} {pinchFingerEvent}"
#endif
		 : TryGet(out PenProximityEvent penProximityEvent)                   ? $"{nameof(PenProximityEvent)} {penProximityEvent}"
		 : TryGet(out PenTouchEvent penTouchEvent)                           ? $"{nameof(PenTouchEvent)} {penTouchEvent}"
		 : TryGet(out PenMotionEvent penMotionEvent)                         ? $"{nameof(PenMotionEvent)} {penMotionEvent}"
		 : TryGet(out PenButtonEvent penButtonEvent)                         ? $"{nameof(PenButtonEvent)} {penButtonEvent}"
		 : TryGet(out PenAxisEvent penAxisEvent)                             ? $"{nameof(PenAxisEvent)} {penAxisEvent}"
		 : TryGet(out RenderEvent renderEvent)                               ? $"{nameof(RenderEvent)} {renderEvent}"
		 : TryGet(out DropEvent dropEvent)                                   ? $"{nameof(DropEvent)} {dropEvent}"
		 : TryGet(out ClipboardEvent clipboardEvent)                         ? $"{nameof(ClipboardEvent)} {clipboardEvent}"
#if SDL3_6_0_OR_GREATER
		 : TryGet(out NotificationEvent notificationEvent)                   ? $"{nameof(NotificationEvent)} {notificationEvent}"
#endif
		 : TryGet(out CommonEvent commonEvent)                               ? $"{nameof(CommonEvent)} {commonEvent}" // since `CommonEvent` accepts all event types, it should be handled last, as a fallback
		 :                                                                     $"{{ {Common.ToPartialString()} }}"; // since the `CommonEvent` case before should already handle everything, this should never be reached; it's just here to make the whole thing exhaustive

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		// Can't use union type pattern matching here, since that's a .NET 11+ feature, and we want to support .NET 10 as well (possibly even with the same code without `#if` directives).
		// That's why we make use of the `TryGet<TEvent>(out TEvent)` method and a big if-else-if-...-else-chain instead.
		// TODO: add cases
		return TryGet(out DisplayEvent displayEvent)                              ? SpanFormat.TryWrite($"{nameof(DisplayEvent)} ", ref destination, ref charsWritten)
											                                     && SpanFormat.TryWrite(in displayEvent, ref destination, ref charsWritten)
		     : TryGet(out WindowEvent windowEvent)                                ? SpanFormat.TryWrite($"{nameof(WindowEvent)} ", ref destination, ref charsWritten)
											                                     && SpanFormat.TryWrite(in windowEvent, ref destination, ref charsWritten)
		     : TryGet(out KeyboardDeviceEvent keyboardDeviceEvent)                ? SpanFormat.TryWrite($"{nameof(KeyboardDeviceEvent)} ", ref destination, ref charsWritten)
											                                     && SpanFormat.TryWrite(in keyboardDeviceEvent, ref destination, ref charsWritten)
		     : TryGet(out KeyboardEvent keyboardEvent)                            ? SpanFormat.TryWrite($"{nameof(KeyboardEvent)} ", ref destination, ref charsWritten)
											                                     && SpanFormat.TryWrite(in keyboardEvent, ref destination, ref charsWritten)
		     : TryGet(out TextEditingEvent textEditingEvent)                      ? SpanFormat.TryWrite($"{nameof(TextEditingEvent)} ", ref destination, ref charsWritten)
											                                     && SpanFormat.TryWrite(in textEditingEvent, ref destination, ref charsWritten)
		     : TryGet(out TextEditingCandidatesEvent textEditingCandidatesEvent)  ? SpanFormat.TryWrite($"{nameof(TextEditingCandidatesEvent)} ", ref destination, ref charsWritten)
											                                     && SpanFormat.TryWrite(in textEditingCandidatesEvent, ref destination, ref charsWritten)
		     : TryGet(out TextInputEvent textInputEvent)                          ? SpanFormat.TryWrite($"{nameof(TextInputEvent)} ", ref destination, ref charsWritten)
											                                     && SpanFormat.TryWrite(in textInputEvent, ref destination, ref charsWritten)
		     : TryGet(out MouseDeviceEvent mouseDeviceEvent)                      ? SpanFormat.TryWrite($"{nameof(MouseDeviceEvent)} ", ref destination, ref charsWritten)
											                                     && SpanFormat.TryWrite(in mouseDeviceEvent, ref destination, ref charsWritten)
		     : TryGet(out MouseMotionEvent mouseMotionEvent)                      ? SpanFormat.TryWrite($"{nameof(MouseMotionEvent)} ", ref destination, ref charsWritten)
											                                     && SpanFormat.TryWrite(in mouseMotionEvent, ref destination, ref charsWritten)
		     : TryGet(out MouseButtonEvent mouseButtonEvent)                      ? SpanFormat.TryWrite($"{nameof(MouseButtonEvent)} ", ref destination, ref charsWritten)
			                                                                     && SpanFormat.TryWrite(in mouseButtonEvent, ref destination, ref charsWritten)
			 : TryGet(out MouseWheelEvent mouseWheelEvent)                        ? SpanFormat.TryWrite($"{nameof(MouseWheelEvent)} ", ref destination, ref charsWritten)
											                                     && SpanFormat.TryWrite(in mouseWheelEvent, ref destination, ref charsWritten)
			 : TryGet(out JoyDeviceEvent joyDeviceEvent)                          ? SpanFormat.TryWrite($"{nameof(JoyDeviceEvent)} ", ref destination, ref charsWritten)
											                                     && SpanFormat.TryWrite(in joyDeviceEvent, ref destination, ref charsWritten)
			 : TryGet(out JoyAxisEvent joyAxisEvent)                              ? SpanFormat.TryWrite($"{nameof(JoyAxisEvent)} ", ref destination, ref charsWritten)
			                                                                     && SpanFormat.TryWrite(in joyAxisEvent, ref destination, ref charsWritten)
			 : TryGet(out JoyBallEvent joyBallEvent)                              ? SpanFormat.TryWrite($"{nameof(JoyBallEvent)} ", ref destination, ref charsWritten)
												                                 && SpanFormat.TryWrite(in joyBallEvent, ref destination, ref charsWritten)
			 : TryGet(out JoyHatEvent joyHatEvent)                                ? SpanFormat.TryWrite($"{nameof(JoyHatEvent)} ", ref destination, ref charsWritten)
			                                                                     && SpanFormat.TryWrite(in joyHatEvent, ref destination, ref charsWritten)
			 : TryGet(out JoyButtonEvent joyButtonEvent)                          ? SpanFormat.TryWrite($"{nameof(JoyButtonEvent)} ", ref destination, ref charsWritten)
			                                                                     && SpanFormat.TryWrite(in joyButtonEvent, ref destination, ref charsWritten)
			 : TryGet(out JoyBatteryEvent joyBatteryEvent)                        ? SpanFormat.TryWrite($"{nameof(JoyBatteryEvent)} ", ref destination, ref charsWritten)
			                                                                     && SpanFormat.TryWrite(in joyBatteryEvent, ref destination, ref charsWritten)
			 : TryGet(out GamepadDeviceEvent gamepadDeviceEvent)                  ? SpanFormat.TryWrite($"{nameof(GamepadDeviceEvent)} ", ref destination, ref charsWritten)
			                                                                     && SpanFormat.TryWrite(in gamepadDeviceEvent, ref destination, ref charsWritten)
			 : TryGet(out GamepadAxisEvent gamepadAxisEvent)                      ? SpanFormat.TryWrite($"{nameof(GamepadAxisEvent)} ", ref destination, ref charsWritten)
			 																	 && SpanFormat.TryWrite(in gamepadAxisEvent, ref destination, ref charsWritten)
			 : TryGet(out GamepadButtonEvent gamepadButtonEvent)                  ? SpanFormat.TryWrite($"{nameof(GamepadButtonEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in gamepadButtonEvent, ref destination, ref charsWritten)
			 : TryGet(out GamepadTouchpadEvent gamepadTouchpadEvent)              ? SpanFormat.TryWrite($"{nameof(GamepadTouchpadEvent)} ", ref destination, ref charsWritten)
			 																	 && SpanFormat.TryWrite(in gamepadTouchpadEvent, ref destination, ref charsWritten)
			 : TryGet(out GamepadSensorEvent gamepadSensorEvent)                  ? SpanFormat.TryWrite($"{nameof(GamepadSensorEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in gamepadSensorEvent, ref destination, ref charsWritten)
#if SDL3_6_0_OR_GREATER
			 : TryGet(out GamepadCapSenseEvent gamepadCapSenseEvent)              ? SpanFormat.TryWrite($"{nameof(GamepadCapSenseEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in gamepadCapSenseEvent, ref destination, ref charsWritten)
#endif
			 : TryGet(out AudioDeviceEvent audioDeviceEvent)                      ? SpanFormat.TryWrite($"{nameof(AudioDeviceEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in audioDeviceEvent, ref destination, ref charsWritten)
			 : TryGet(out CameraDeviceEvent cameraDeviceEvent)                    ? SpanFormat.TryWrite($"{nameof(CameraDeviceEvent)} ", ref destination, ref charsWritten)
			 																	 && SpanFormat.TryWrite(in cameraDeviceEvent, ref destination, ref charsWritten)
			 : TryGet(out SensorEvent sensorEvent)                                ? SpanFormat.TryWrite($"{nameof(SensorEvent)} ", ref destination, ref charsWritten)
			 																	 && SpanFormat.TryWrite(in sensorEvent, ref destination, ref charsWritten)
			 : TryGet(out QuitEvent quitEvent)                                    ? SpanFormat.TryWrite($"{nameof(QuitEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in quitEvent, ref destination, ref charsWritten)
			 : TryGet(out UserEvent? userEvent)                                   ? SpanFormat.TryWrite(userEvent.GetType().Name, ref destination, ref charsWritten) // `UserEvent` must be matched before `UnmanagedUserEvent`, because both accept the same event types, but `UserEvent` has more constraints, so it should be handled first
																				 && SpanFormat.TryWrite(' ', ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in userEvent, ref destination, ref charsWritten)
			 : TryGet(out UnmanagedUserEvent unmanagedUserEvent)                  ? SpanFormat.TryWrite($"{nameof(UnmanagedUserEvent)} ", ref destination, ref charsWritten) // `UnmanagedUserEvent` must be matched after `UserEvent`, because both accept the same event types, but `UserEvent` has more constraints, so it should be handled first
																				 && SpanFormat.TryWrite(in unmanagedUserEvent, ref destination, ref charsWritten)
			 : TryGet(out TouchFingerEvent touchFingerEvent)                      ? SpanFormat.TryWrite($"{nameof(TouchFingerEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in touchFingerEvent, ref destination, ref charsWritten)
#if SDL3_4_0_OR_GREATER
			 : TryGet(out PinchFingerEvent pinchFingerEvent)                     ? SpanFormat.TryWrite($"{nameof(PinchFingerEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in pinchFingerEvent, ref destination, ref charsWritten)
#endif
			 : TryGet(out PenProximityEvent penProximityEvent)                    ? SpanFormat.TryWrite($"{nameof(PenProximityEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in penProximityEvent, ref destination, ref charsWritten)
			 : TryGet(out PenTouchEvent penTouchEvent)                            ? SpanFormat.TryWrite($"{nameof(PenTouchEvent)} ", ref destination, ref charsWritten)
			 																	 && SpanFormat.TryWrite(in penTouchEvent, ref destination, ref charsWritten)
			 : TryGet(out PenMotionEvent penMotionEvent) 						  ? SpanFormat.TryWrite($"{nameof(PenMotionEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in penMotionEvent, ref destination, ref charsWritten)
			 : TryGet(out PenButtonEvent penButtonEvent)                          ? SpanFormat.TryWrite($"{nameof(PenButtonEvent)} ", ref destination, ref charsWritten)
			 																	 && SpanFormat.TryWrite(in penButtonEvent, ref destination, ref charsWritten)
			 : TryGet(out PenAxisEvent penAxisEvent)                              ? SpanFormat.TryWrite($"{nameof(PenAxisEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in penAxisEvent, ref destination, ref charsWritten)
			 : TryGet(out RenderEvent renderEvent)                                ? SpanFormat.TryWrite($"{nameof(RenderEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in renderEvent, ref destination, ref charsWritten)
			 : TryGet(out DropEvent dropEvent)                                    ? SpanFormat.TryWrite($"{nameof(DropEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in dropEvent, ref destination, ref charsWritten)
			 : TryGet(out ClipboardEvent clipboardEvent)                          ? SpanFormat.TryWrite($"{nameof(ClipboardEvent)} ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in clipboardEvent, ref destination, ref charsWritten)
#if SDL3_6_0_OR_GREATER
			 : TryGet(out NotificationEvent notificationEvent)                    ? SpanFormat.TryWrite($"{nameof(Notification)}, ", ref destination, ref charsWritten)
																				 && SpanFormat.TryWrite(in notificationEvent, ref destination, ref charsWritten)
#endif
			 : TryGet(out CommonEvent commonEvent)                                ? SpanFormat.TryWrite($"{nameof(CommonEvent)} ", ref destination, ref charsWritten) // since `CommonEvent` accepts all event types, it should be handled last, as a fallback
												                                 && SpanFormat.TryWrite(in commonEvent, ref destination, ref charsWritten)
			 :                                                                      SpanFormat.TryWrite("{ ", ref destination, ref charsWritten) // since the `CommonEvent` case before should already handle everything, this should never be reached; it's just here to make the whole thing exhaustive
												                                 && Common.TryPartiallyFormat(ref destination, ref charsWritten)
												                                 && SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
