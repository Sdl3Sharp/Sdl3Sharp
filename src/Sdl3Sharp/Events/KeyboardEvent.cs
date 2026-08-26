using Sdl3Sharp.Input;
using Sdl3Sharp.Internal;
using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when a key is pressed or released
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.KeyDown"/></description></item>
/// <item><description><see cref="EventType.KeyUp"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct KeyboardEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWindowID;
	private uint mWhich;
	private Scancode mScancode;
	private Keycode mKey;
	private Keymod mMod;
	private ushort mRaw;
	private CBool mDown;
	private CBool mRepeat;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="KeyboardEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(KeyboardEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> associated with this event, or <c>0</c> if no window is associated with this event
	/// </value>
	/// <remarks>
	/// <para>
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="KeyboardEvent"/> is most likely the window that currently has keyboard focus, if any.
	/// </para>
	/// </remarks>
	public uint WindowId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWindowID;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWindowID = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Video.Windowing.Window"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Video.Windowing.Window"/> associated with this event, or <c><see langword="null"/></c> if no window is associated with this event
	/// </value>
	/// <remarks>
	/// <para>
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="KeyboardEvent"/> is most likely the window that currently has keyboard focus, if any.
	/// </para>
	/// </remarks>
	public Window? Window
	{
		readonly get
		{
			Window.TryGetFromId(mWindowID, out var window);
			return window;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWindowID = value?.Id ?? 0;
	}

	/// <summary>
	/// Gets or sets the <see cref="Keyboard.Id">ID</see> of the <see cref="Input.Keyboard"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Keyboard.Id">ID</see> of the <see cref="Input.Keyboard"/> associated with this event, or <c>0</c> if the keyboard is unknown or virtual
	/// </value>
	public uint KeyboardId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWhich;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWhich = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Input.Keyboard"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Input.Keyboard"/> associated with this event, or <c><see langword="null"/></c> if the keyboard is unknown or virtual
	/// </value>
	public Keyboard? Keyboard
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		readonly get
		{
			if (Input.Keyboard.TryGetFromId(mWhich, out var keyboard))
			{
				return keyboard;
			}

			return null;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		set => mWhich = value?.Id ?? 0;
	}

	/// <summary>
	/// Gets or set the physical keyboard scancode of the key
	/// </summary>
	/// <value>
	/// The physical keyboard scancode of the key
	/// </value>
	public Scancode Scancode
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mScancode;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mScancode = value;
	}

	/// <summary>
	/// Gets or set the virtual keycode of the key
	/// </summary>
	/// <value>
	/// The virtual keycode of the key
	/// </value>
	/// <remarks>
	/// <para>
	/// This property reflects the base <see cref="Input.Keycode"/> generated by pressing the <see cref="Scancode"/> using the current keyboard layout, applying any options specified via <see cref="Hint.KeycodeOptions"/>.
	/// You can get the <see cref="Keycode"/> corresponding to the event <see cref="Scancode"/> and <see cref="Modifiers"/> directly from the keyboard layout, bypassing <see cref="Hint.KeycodeOptions"/>, by calling <see cref="KeycodeExtensions.TryGetFromScancode(Scancode, Keymod, out Keycode)"/>.
	/// </para>
	/// </remarks>
	public Keycode Keycode
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mKey;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mKey = value;
	}

	/// <summary>
	/// Gets or sets the current key modifiers
	/// </summary>
	/// <value>
	/// The current key modifiers
	/// </value>
	public Keymod Modifiers
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mMod;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mMod = value;
	}

	/// <summary>
	/// Gets or sets the raw platform dependent scancode 
	/// </summary>
	/// <value>
	/// The raw platform dependent scancode
	/// </value>
	public ushort Raw
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mRaw;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mRaw = value;
	}

	/// <summary>
	/// Gets or sets a value indicating whether the key is pressed (currently held down)
	/// </summary>
	/// <value>
	/// A value indicating whether the key is pressed (currently held down)
	/// </value>
	public bool IsDown
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mDown;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mDown = value;
	}

	/// <summary>
	/// Gets or sets a value indication whether this event represents a repeated key event
	/// </summary>
	/// <value>
	/// A value indication whether this event represents a repeated key event
	/// </value>
	public bool IsRepeat
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mRepeat;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mRepeat = value;
	}

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {mCommon.ToPartialString()}, {
			nameof(WindowId)}: {mWindowID.ToString(format, formatProvider)}, {
			nameof(KeyboardId)}: {mWhich.ToString(format, formatProvider)}, {
			nameof(Scancode)}: {mScancode}, {
			nameof(Keycode)}: {mKey}, {
			nameof(Modifiers)}: {mMod}, {
			nameof(Raw)}: {mRaw.ToString(format, formatProvider)}, {
			nameof(IsDown)}: {(bool)mDown}, {
			nameof(IsRepeat)}: {(bool)mRepeat} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(WindowId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWindowID, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(KeyboardId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWhich, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(Scancode)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mScancode, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(Keycode)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mKey, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(Modifiers)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mMod, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(Raw)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mRaw, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(IsDown)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite((bool)mDown, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(IsRepeat)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite((bool)mRepeat, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
