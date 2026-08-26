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
/// Represents an event that occurs when a <see cref="Video.Rendering.Renderer"/>'s targets have been reset and their contents need to be updated,
/// or when a render device has been reset (and all textures need to be recreated) or has been lost (and can't be recovered)
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.RenderTargetsReset"/></description></item>
/// <item><description><see cref="EventType.RenderDeviceReset"/></description></item>
/// <item><description><see cref="EventType.RenderDeviceLost"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct RenderEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWindowID;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="RenderEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(RenderEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> containing the <see cref="Video.Rendering.Renderer"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> containing the <see cref="Video.Rendering.Renderer"/> associated with this event, or <c>0</c> if the renderer is rendering to an off-screen target
	/// </value>
	public uint WindowId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWindowID;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWindowID = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Video.Windowing.Window"/> containing the <see cref="Video.Rendering.Renderer"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Video.Windowing.Window"/> containing the <see cref="Video.Rendering.Renderer"/> associated with this event, or <c><see langword="null"/></c> if the renderer is rendering to an off-screen target
	/// </value>
	public Window? Window
	{
		readonly get
		{
			Window.TryGetFromId(mWindowID, out var window);
			return window;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		set => mWindowID = value?.Id ?? 0;
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
			nameof(WindowId)}: {mWindowID.ToString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(WindowId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWindowID, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
