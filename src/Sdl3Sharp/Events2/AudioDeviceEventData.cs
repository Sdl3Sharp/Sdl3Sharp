using Sdl3Sharp.Internal;
using Sdl3Sharp.Internal.Interop;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events2;

partial struct Event
{
	[FieldOffset(0)] internal Event<AudioDeviceEventData> ADevice;
}

/// <summary>
/// Represents event data for an event that occurs when an <see cref="AudioDevice">audio device</see> is being <see cref="EventType.AudioDeviceAdded">added</see>, <see cref="EventType.AudioDeviceRemoved">removed</see>, or <see cref="EventType.AudioDeviceFormatChanged">changed</see>
/// </summary>
/// <remarks>
/// <para>
/// SDL will send an <see cref="Event{TEventData}">Event&lt;<see cref="AudioDeviceEventData">AudioDeviceEventData</see>&gt;</see> with <see cref="Type"/> <see cref="EventType.AudioDeviceAdded"/> for every audio device it discovers during initialization.
/// After that, <see cref="Event{TEventData}">Event&lt;<see cref="AudioDeviceEventData">AudioDeviceEventData</see>&gt;</see>s with <see cref="Type"/> <see cref="EventType.AudioDeviceAdded"/> will only arrive when an audio device is hotplugged during the application's runtime.
/// </para>
/// <para>
/// The associated <see cref="EventType"/>s are:
/// <list type="bullet">
/// <item><description><see cref="EventType.AudioDeviceAdded"/></description></item> 
/// <item><description><see cref="EventType.AudioDeviceRemoved"/></description></item> 
/// <item><description><see cref="EventType.AudioDeviceFormatChanged"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public struct AudioDeviceEventData : IEventData<AudioDeviceEventData>, IFormattable, ISpanFormattable
{
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static bool IEventData<AudioDeviceEventData>.AcceptsEventType(EventType type) => type is >= EventType.AudioDeviceAdded and <= EventType.AudioDeviceFormatChanged;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private uint mWhich;
	private CBool mRecording;
	private readonly byte mPadding1, mPadding2, mPadding3;

	/// <summary>
	/// Gets or sets the audio device ID for the <see cref="AudioDevice"/> being <see cref="EventType.AudioDeviceAdded">added</see>, <see cref="EventType.AudioDeviceRemoved">removed</see>, or <see cref="EventType.AudioDeviceFormatChanged">changed</see>
	/// </summary>
	/// <value>
	/// The audio device IDfor the <see cref="AudioDevice"/> being <see cref="EventType.AudioDeviceAdded">added</see>, <see cref="EventType.AudioDeviceRemoved">removed</see>, or <see cref="EventType.AudioDeviceFormatChanged">changed</see>
	/// </value>
	public uint AudioDeviceId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWhich;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWhich = value;
	}

	/// <summary>
	/// Gets or sets a value indicating if the <see cref="AudioDeviceId">specific audio device</see> is a recording device or a playback device
	/// </summary>
	/// <value>
	/// A value indicating if the <see cref="AudioDeviceId">specific audio device</see> is a recording device (when <c><see langword="true"/></c>) or a playback device (when <c><see langword="false"/></c>)
	/// </value>
	public bool IsRecordingDevice
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mRecording;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mRecording = value;
	}

	private readonly string ToPartialString(string? format, IFormatProvider? formatProvider)
		=> $"{nameof(AudioDeviceId)}: {AudioDeviceId.ToString(format, formatProvider)}, {
			nameof(IsRecordingDevice)}: {IsRecordingDevice}";

	readonly string IEventData<AudioDeviceEventData>.ToPartialString(string? format, IFormatProvider? formatProvider)
		=> $", {ToPartialString(format, formatProvider)}";

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {ToPartialString(format, formatProvider)} }}";

	private readonly bool TryPartiallyFormat(ref Span<char> destination, ref int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
		=> SpanFormat.TryWrite($"{nameof(AudioDeviceId)}: ", ref destination, ref charsWritten)
		&& SpanFormat.TryWrite(AudioDeviceId, ref destination, ref charsWritten, format, provider)
		&& SpanFormat.TryWrite($", {nameof(IsRecordingDevice)}: ", ref destination, ref charsWritten)
		&& SpanFormat.TryWrite(IsRecordingDevice, ref destination, ref charsWritten);

	readonly bool IEventData<AudioDeviceEventData>.TryPartiallyFormat(ref Span<char> destination, ref int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
		=> SpanFormat.TryWrite(", ", ref destination, ref charsWritten)
		&& TryPartiallyFormat(ref destination, ref charsWritten, format, provider);

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& TryPartiallyFormat(ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}