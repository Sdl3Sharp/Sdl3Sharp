#if SDL3_4_0_OR_GREATER

using Sdl3Sharp.Internal;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when a pinch gesture starts, updates, or ends
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.PinchBegin"/></description></item>
/// <item><description><see cref="EventType.PinchUpdated"/></description></item>
/// <item><description><see cref="EventType.PinchEnd"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct PinchFingerEvent : IFormattable, ISpanFormattable
{
	[SupportedOSPlatformGuard("android"), SupportedOSPlatformGuard("ios"), UnsupportedOSPlatformGuard("maccatalyst")]
	private static bool IsMobilePlatform => OperatingSystem.IsAndroid() || (OperatingSystem.IsIOS() && !OperatingSystem.IsMacCatalyst());

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private float mScale;
	private uint mWindowID;
	[SupportedOSPlatform("android"), SupportedOSPlatform("ios"), UnsupportedOSPlatform("maccatalyst")] private float mSpanX; // "Mobile platforms" only
	[SupportedOSPlatform("android"), SupportedOSPlatform("ios"), UnsupportedOSPlatform("maccatalyst")] private float mSpanY; // "Mobile platforms" only
	[SupportedOSPlatform("android"), SupportedOSPlatform("ios"), UnsupportedOSPlatform("maccatalyst")] private float mFocusX; // "Mobile platforms" only
	[SupportedOSPlatform("android"), SupportedOSPlatform("ios"), UnsupportedOSPlatform("maccatalyst")] private float mFocusY; // "Mobile platforms" only

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="PinchFingerEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(PinchFingerEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the scale change since the last <see cref="EventType.PinchUpdated"/> events
	/// </summary>
	/// <value>
	/// The scale change since the last <see cref="EventType.PinchUpdated"/> events, where values that are less then <c>1</c> mean "zoom out" and values that are greater than <c>1</c> mean "zoom in"
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException">
	/// When setting this property, the given value is less than <c>0</c>
	/// </exception>
	public float Scale
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mScale;

		set
		{
			if (value is < 0f)
			{
				[DoesNotReturn]
				static void failInvalidScale(float scale) => throw new ArgumentOutOfRangeException(nameof(value), scale, $"The {nameof(Scale)} property must be greater than or equal to 0.");

				failInvalidScale(value);
			}

			mScale = value;
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> where the pinch gesture occurred, if any
	/// </summary>
	/// <value>
	/// The <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> where the pinch gesture occurred, or <c>0</c> if there is no window or it cannot be determined
	/// </value>
	public uint WindowId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWindowID;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWindowID = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Video.Windowing.Window"/> where the pinch gesture occurred, if any
	/// </summary>
	/// <value>
	/// The <see cref="Video.Windowing.Window"/> where the pinch gesture occurred, or <c><see langword="null"/></c> if there is no window or it cannot be determined
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
	/// Gets or sets the average horizontal distance between the two pointers forming the pinch gesture, in window coordinates
	/// </summary>
	/// <value>
	/// The average horizontal distance between the two pointers forming the pinch gesture, in window coordinates, or <c>-1</c> if the <see cref="Type"/> is not <see cref="EventType.PinchBegin"/> or <see cref="EventType.PinchUpdated"/>
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is only available on mobile platforms. It has no effect or meaning on other platforms and might contain garbage data on those platforms.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException">
	/// When setting this property, the given value is less than <c>0</c> and not equal to <c>-1</c>
	/// </exception>
	[SupportedOSPlatform("android"), SupportedOSPlatform("ios"), UnsupportedOSPlatform("maccatalyst")]
	public float SpanX
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		readonly get => mSpanX;

		set
		{
			if (value is < 0f and not -1f)
			{
				[DoesNotReturn]
				static void failInvalidSpanX(float spanX) => throw new ArgumentOutOfRangeException(nameof(value), spanX, $"The {nameof(SpanX)} property must be greater than or equal to 0, or equal to -1.");

				failInvalidSpanX(value);
			}	

			mSpanX = value;
		}
	}

	/// <summary>
	/// Gets or sets the average vertical distance between the two pointers forming the pinch gesture, in window coordinates
	/// </summary>
	/// <value>
	/// The average vertical distance between the two pointers forming the pinch gesture, in window coordinates, or <c>-1</c> if the <see cref="Type"/> is not <see cref="EventType.PinchBegin"/> or <see cref="EventType.PinchUpdated"/>
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is only available on mobile platforms. It has no effect or meaning on other platforms and might contain garbage data on those platforms.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException">
	/// When setting this property, the given value is less than <c>0</c> and not equal to <c>-1</c>
	/// </exception>
	[SupportedOSPlatform("android"), SupportedOSPlatform("ios"), UnsupportedOSPlatform("maccatalyst")]
	public float SpanY
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mSpanY;

		set
		{
			if (value is < 0f and not -1f)
			{
				[DoesNotReturn]
				static void failInvalidSpanY(float spanY) => throw new ArgumentOutOfRangeException(nameof(value), spanY, $"The {nameof(SpanY)} property must be greater than or equal to 0, or equal to -1.");

				failInvalidSpanY(value);
			}

			mSpanY = value;
		}
	}

	/// <summary>
	/// Gets or sets the horizontal coordinate of the focal point of the pinch gesture, in window coordinates
	/// </summary>
	/// <value>
	/// The horizontal coordinate of the focal point of the pinch gesture, in window coordinates, or <c>-1</c> if the <see cref="Type"/> is not <see cref="EventType.PinchBegin"/> or <see cref="EventType.PinchUpdated"/>
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is only available on mobile platforms. It has no effect or meaning on other platforms and might contain garbage data on those platforms.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException">
	/// When setting this property, the given value is less than <c>0</c> and not equal to <c>-1</c>
	/// </exception>
	[SupportedOSPlatform("android"), SupportedOSPlatform("ios"), UnsupportedOSPlatform("maccatalyst")]
	public float FocusX
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		readonly get => mFocusX;

		set
		{
			if (value is < 0f and not -1f)
			{
				[DoesNotReturn]
				static void failInvalidFocusX(float focusX) => throw new ArgumentOutOfRangeException(nameof(value), focusX, $"The {nameof(FocusX)} property must be greater than or equal to 0, or equal to -1.");

				failInvalidFocusX(value);
			}

			mFocusX = value;
		}
	}

	/// <summary>
	/// Gets or sets the vertical coordinate of the focal point of the pinch gesture, in window coordinates
	/// </summary>
	/// <value>
	/// The vertical coordinate of the focal point of the pinch gesture, in window coordinates, or <c>-1</c> if the <see cref="Type"/> is not <see cref="EventType.PinchBegin"/> or <see cref="EventType.PinchUpdated"/>
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is only available on mobile platforms. It has no effect or meaning on other platforms and might contain garbage data on those platforms.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException">
	/// When setting this property, the given value is less than <c>0</c> and not equal to <c>-1</c>
	/// </exception>
	[SupportedOSPlatform("android"), SupportedOSPlatform("ios"), UnsupportedOSPlatform("maccatalyst")]
	public float FocusY
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		readonly get => mFocusY;

		set
		{
			if (value is < 0f and not -1f)
			{
				[DoesNotReturn]
				static void failInvalidFocusY(float focusY) => throw new ArgumentOutOfRangeException(nameof(value), focusY, $"The {nameof(FocusY)} property must be greater than or equal to 0, or equal to -1.");

				failInvalidFocusY(value);
			}

			mFocusY = value;
		}
	}

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> IsMobilePlatform
#pragma warning disable CA1416 // That's a false positive: For some reason, the analyzer doesn't correctly take `nameof` expressions into account and still reports CA1416 for them, even though the properties themselves are annoted with the correct attributes
			? $"{{ {mCommon.ToPartialString()}, {
				nameof(Scale)}: {mScale.ToString(format, formatProvider)}, {
				nameof(WindowId)}: {mWindowID.ToString(format, formatProvider)}, {
				nameof(SpanX)}: {mSpanX.ToString(format, formatProvider)}, {
				nameof(SpanY)}: {mSpanY.ToString(format, formatProvider)}, {
				nameof(FocusX)}: {mFocusX.ToString(format, formatProvider)}, {
				nameof(FocusY)}: {mFocusY.ToString(format, formatProvider)}, }}"
#pragma warning restore CA1416 
			: $"{{ {mCommon.ToPartialString()}, {
				nameof(Scale)}: {mScale.ToString(format, formatProvider)}, {
				nameof(WindowId)}: {mWindowID.ToString(format, formatProvider)}, }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		if ( !(SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(Scale)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mScale, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(WindowId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWindowID, ref destination, ref charsWritten, format, provider)))
		{
			return false;
		}

		if (IsMobilePlatform)
		{
#pragma warning disable CA1416 // That's a false positive: For some reason, the analyzer doesn't correctly take `nameof` expressions into account and still reports CA1416 for them, even though the properties themselves are annoted with the correct attributes
			if ( !(SpanFormat.TryWrite($", {nameof(SpanX)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mSpanX, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(SpanY)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mSpanY, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(FocusX)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mFocusX, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(FocusY)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mFocusY, ref destination, ref charsWritten, format, provider)))
#pragma warning restore CA1416
			{
				return false;
			}
		}

		return SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}

#endif
