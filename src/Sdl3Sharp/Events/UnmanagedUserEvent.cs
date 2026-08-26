using Sdl3Sharp.Internal;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an user-defined event that can be sent and received by user code and can be associated with custom data
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType"/>s obtained from <see cref="EventTypeExtensions.TryRegister(out EventType)"/> or <see cref="EventTypeExtensions.TryRegister(Span{EventType})"/></description></item>
/// </list>
/// </para>
/// <para>
/// <see cref="UnmanagedUserEvent"/> is a low-level representation of a user-defined event, and it is recommended to use event types inheriting from the <see cref="UserEvent"/> class for most use cases instead.
/// </para>
/// <para>
/// Note that <see cref="UnmanagedUserEvent"/> and <see cref="UserEvent"/> accept the same event types.
/// If you want to match against both in your code, you should make sure to match against <see cref="UserEvent"/> first, and only after that against <see cref="UnmanagedUserEvent"/>,
/// as <see cref="UnmanagedUserEvent"/> will accept all user-defined events, including managed <see cref="UserEvent"/>s.
/// Also, if you accept an <see cref="UnmanagedUserEvent"/> that is actually an <see cref="UserEvent"/>, you should never manipulate the <see cref="Data1"/> and <see cref="Data2"/> pointers, as they might point to managed data.
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct UnmanagedUserEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWindowID;
	private int mCode;
	private unsafe void* mData1;
	private unsafe void* mData2;

	/// <remarks>
	/// <para>
	/// <see cref="EventType"/>s used for <see cref="UnmanagedUserEvent"/> must be obtained from <see cref="EventTypeExtensions.TryRegister(out EventType)"/> or <see cref="EventTypeExtensions.TryRegister(Span{EventType})"/>.
	/// </para>
	/// </remarks>
	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for an <see cref="UnmanagedUserEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(UnmanagedUserEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the user-defined event code
	/// </summary>
	/// <value>
	/// The user-defined event code
	/// </value>
	/// <remarks>
	/// <para>
	/// You can freely set this to any value of your liking. For example, you could use this property to further distinguish your user-defined events.
	/// </para>
	/// </remarks>
	public int Code
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCode;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mCode = value;
	}

	/// <summary>
	/// Gets or sets the first user-defined data pointer
	/// </summary>
	/// <value>
	/// The first user-defined data pointer
	/// </value>
	/// <remarks>
	/// <para>
	/// You can freely set this to any value of your liking. For example, you could use this property to associate some custom data with your user-defined events.
	/// </para>
	/// </remarks>
	public unsafe void* Data1
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mData1;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mData1 = value;
	}

	/// <summary>
	/// Gets or sets the second user-defined data pointer
	/// </summary>
	/// <value>
	/// The second user-defined data pointer
	/// </value>
	/// <remarks>
	/// <para>
	/// You can freely set this to any value of your liking. For example, you could use this property to associate some custom data with your user-defined events.
	/// </para>
	/// </remarks>
	public unsafe void* Data2
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mData2;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mData2 = value;
	}

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
	{ 
		unsafe
		{
			return $"{{ {mCommon.ToPartialString()}, {
				nameof(WindowId)}: {mWindowID.ToString(format, formatProvider)}, {
				nameof(Code)}: {mCode.ToString(format, formatProvider)}, {
				nameof(Data1)}: 0x{mData1 switch { not null and var data1 => unchecked((IntPtr)data1).ToString(format: sizeof(void*) switch { < 8 => "X8", 8 => "X16", _ => "X32" /* good enough for now */ }), _ => "null" }}, {
				nameof(Data2)}: 0x{mData2 switch { not null and var data2 => unchecked((IntPtr)data2).ToString(format: sizeof(void*) switch { < 8 => "X8", 8 => "X16", _ => "X32" /* good enough for now */ }), _ => "null" }} }}";
		}
	}

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		unsafe
		{
			charsWritten = 0;

			if (!(SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
				&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
				&& SpanFormat.TryWrite($", {nameof(WindowId)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mWindowID, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(Code)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mCode, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(Data1)}: ", ref destination, ref charsWritten)))
			{
				return false;
			}

			if (mData1 is not null and var data1)
			{
				if ( !(SpanFormat.TryWrite("0x", ref destination, ref charsWritten)
					&& SpanFormat.TryWrite(unchecked((IntPtr)data1), ref destination, ref charsWritten, format: sizeof(void*) switch { < 8 => "X8", 8 => "X16", _ => "X32" /* good enough for now */ })))
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

			if (!SpanFormat.TryWrite($", {nameof(Data2)}: ", ref destination, ref charsWritten))
			{
				return false;
			}

			if (mData2 is not null and var data2)
			{
				if ( !(SpanFormat.TryWrite("0x", ref destination, ref charsWritten)
					&& SpanFormat.TryWrite(unchecked((IntPtr)data2), ref destination, ref charsWritten, format: sizeof(void*) switch { < 8 => "X8", 8 => "X16", _ => "X32" /* good enough for now */ })))
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
}
