#if NET11_0_OR_GREATER

using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Events;

[Union]
partial struct Event : Event.IUnionMembers
{
	/// <summary>
	/// The union members provider for the <see cref="Event"/> union type
	/// </summary>
	public partial interface IUnionMembers
	{
		/// <summary>
		/// Gets a value indicating whether this union has a value
		/// </summary>
		/// <value>
		/// A value indicating whether this union has a value
		/// </value>
		/// <remarks>
		/// <para>
		/// In the case of the <see cref="Event"/> union type, this property will always return <c><see langword="true"/></c>.
		/// </para>
		/// </remarks>
		bool HasValue { get; }

		/// <summary>
		/// Gets the value of this union
		/// </summary>
		/// <value>
		/// The value of this union, or <c><see langword="null"/></c> if this union has no value
		/// </value>
		/// <remarks>
		/// <para>
		/// In the case of the <see cref="Event"/> union type, this property will return an instance of the appropriate event case type, or the same <see cref="Event"/> instance if no corresponding case type is found;
		/// it will never return <c><see langword="null"/></c> though.
		/// </para>
		/// </remarks>
		object? Value { get; }
	}

	readonly bool IUnionMembers.HasValue
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => true; // `Value` will never be null, since even in the case of an unrecognized event type, we will still return a `CommonEvent` instance, because `CommonEvent` accepts all event types.
		             // And even if we didn't have a `CommonEvent` case or if `CommonEvent` didn't accept all event types, `Value` would still return `this` as a fallback.
	}

	readonly object? IUnionMembers.Value
		// Can't use union type pattern matching here, since that's a .NET 11+ feature, and we want to support .NET 10 as well (possibly even with the same code without `#if` directives).
		// That's why we make use of the `TryGet<TEvent>(out TEvent)` method and a big if-else-if-...-else-chain instead.
		// Also it's important to box/cast the case values immediately to `object?`, otherwise, on .NET 11+ only, the common type of the nested tertiary expression chain will be inferred to be `Event`, the common union type of all the case types,
		// and it'd be rather stupid to extract the correct case type value just to then immediately copy it back into a `Event` instance, just for that to be boxed into an `object?` again and returned.
		// TODO: add cases
		=> TryGet(out DisplayEvent displayEvent)                             ? (object?)displayEvent
		 : TryGet(out WindowEvent windowEvent)                               ? (object?)windowEvent
		 : TryGet(out KeyboardDeviceEvent keyboardDeviceEvent)               ? (object?)keyboardDeviceEvent
		 : TryGet(out KeyboardEvent keyboardEvent)                           ? (object?)keyboardEvent
		 : TryGet(out TextEditingEvent textEditingEvent)                     ? (object?)textEditingEvent
		 : TryGet(out TextEditingCandidatesEvent textEditingCandidatesEvent) ? (object?)textEditingCandidatesEvent
		 : TryGet(out TextInputEvent textInputEvent)                         ? (object?)textInputEvent
		 : TryGet(out MouseDeviceEvent mouseDeviceEvent)                     ? (object?)mouseDeviceEvent
		 : TryGet(out MouseMotionEvent mouseMotionEvent)                     ? (object?)mouseMotionEvent
		 : TryGet(out MouseButtonEvent mouseButtonEvent)                     ? (object?)mouseButtonEvent
		 : TryGet(out MouseWheelEvent mouseWheelEvent)                       ? (object?)mouseWheelEvent
		 : TryGet(out JoyDeviceEvent joyDeviceEvent)                         ? (object?)joyDeviceEvent
		 : TryGet(out JoyAxisEvent joyAxisEvent)                             ? (object?)joyAxisEvent
		 : TryGet(out JoyBallEvent joyBallEvent)                             ? (object?)joyBallEvent
		 : TryGet(out JoyHatEvent joyHatEvent)                               ? (object?)joyHatEvent
		 : TryGet(out JoyButtonEvent joyButtonEvent)                         ? (object?)joyButtonEvent
		 : TryGet(out JoyBatteryEvent joyBatteryEvent)                       ? (object?)joyBatteryEvent
		 : TryGet(out GamepadDeviceEvent gamepadDeviceEvent)                 ? (object?)gamepadDeviceEvent
		 : TryGet(out GamepadAxisEvent gamepadAxisEvent)                     ? (object?)gamepadAxisEvent
		 : TryGet(out GamepadButtonEvent gamepadButtonEvent)                 ? (object?)gamepadButtonEvent
		 : TryGet(out GamepadTouchpadEvent gamepadTouchpadEvent)             ? (object?)gamepadTouchpadEvent
		 : TryGet(out GamepadSensorEvent gamepadSensorEvent)                 ? (object?)gamepadSensorEvent
#if SDL3_6_0_OR_GREATER
		 : TryGet(out GamepadCapSenseEvent gamepadCapSenseEvent)             ? (object?)gamepadCapSenseEvent
#endif
		 : TryGet(out AudioDeviceEvent audioDeviceEvent)                     ? (object?)audioDeviceEvent
		 : TryGet(out CameraDeviceEvent cameraDeviceEvent)                   ? (object?)cameraDeviceEvent
		 : TryGet(out SensorEvent sensorEvent)                               ? (object?)sensorEvent
		 : TryGet(out QuitEvent quitEvent)									 ? (object?)quitEvent
		 : TryGet(out UserEvent? userEvent)									 ? (object?)userEvent // `UserEvent` must be matched before `UnmanagedUserEvent`, because both accept the same event types, but `UserEvent` has more constraints, so it should be handled first
		 : TryGet(out UnmanagedUserEvent unmanagedUserEvent)                 ? (object?)unmanagedUserEvent // `UnmanagedUserEvent` must be matched after `UserEvent`, because both accept the same event types, but `UserEvent` has more constraints, so it should be handled first
		 : TryGet(out TouchFingerEvent touchFingerEvent)                     ? (object?)touchFingerEvent
#if SDL3_4_0_OR_GREATER
		 : TryGet(out PinchFingerEvent pinchFingerEvent)                     ? (object?)pinchFingerEvent
#endif
		 : TryGet(out PenProximityEvent penProximityEvent)                   ? (object?)penProximityEvent
		 : TryGet(out PenTouchEvent penTouchEvent)                           ? (object?)penTouchEvent
		 : TryGet(out PenMotionEvent penMotionEvent)                         ? (object?)penMotionEvent
		 : TryGet(out PenButtonEvent penButtonEvent)                         ? (object?)penButtonEvent
		 : TryGet(out PenAxisEvent penAxisEvent)                             ? (object?)penAxisEvent
		 : TryGet(out RenderEvent renderEvent)                               ? (object?)renderEvent
		 : TryGet(out DropEvent dropEvent)                                   ? (object?)dropEvent
		 : TryGet(out ClipboardEvent clipboardEvent)                         ? (object?)clipboardEvent
#if SDL3_6_0_OR_GREATER
		 : TryGet(out NotificationEvent notificationEvent)                   ? (object?)notificationEvent
#endif
		 : TryGet(out CommonEvent commonEvent)                               ? (object?)commonEvent // since `CommonEvent` accepts all event types, it should be handled last, as a fallback
		 :                                                                     (object?)this; // since the `CommonEvent` case before should already handle everything, this should never be reached; it's just here to make the whole thing exhaustive
}

#endif
