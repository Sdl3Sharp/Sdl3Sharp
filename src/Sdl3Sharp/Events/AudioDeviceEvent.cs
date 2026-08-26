using Sdl3Sharp.Internal;
using Sdl3Sharp.Internal.Interop;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when an <see cref="Audio.AudioDevice"/> is added or removed, or when an <see cref="Audio.AudioDevice"/> changes its format
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.AudioDeviceAdded"/></description></item>
/// <item><description><see cref="EventType.AudioDeviceRemoved"/></description></item>
/// <item><description><see cref="EventType.AudioDeviceFormatChanged"/></description></item>
/// </list>
/// </para>
/// <para>
/// SDL will send <see cref="EventType.AudioDeviceAdded"/> (<see cref="AudioDeviceEvent"/>) events for each audio device that is already connected when <see cref="Sdl(Sdl3Sharp.Sdl.BuildAction?)">SDL is initialized</see>.
/// After that, <see cref="EventType.AudioDeviceAdded"/> (<see cref="AudioDeviceEvent"/>) events will only be sent when a new audio device is hotplugged into the system during the application's runtime.
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct AudioDeviceEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWhich;
	private CBool mRecording;
	private readonly byte mPadding1, mPadding2, mPadding3;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for an <see cref="AudioDeviceEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(AudioDeviceEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the <see cref="AudioDevice.Id">ID</see> of the <see cref="Audio.AudioDevice"/> associated with this event
	/// </summary>
	/// <value>
	/// The <see cref="AudioDevice.Id">ID</see> of the <see cref="Audio.AudioDevice"/> associated with this event
	/// </value>
	public uint AudioDeviceId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWhich;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWhich = value;
	}

	// TODO: Add a `AudioDevice` property once the `AudioDevice` type is implemented

	/// <summary>
	/// Gets or sets a value indicating whether the <see cref="AudioDevice"/> is a recording device or a playback device
	/// </summary>
	/// <value>
	/// A value indicating whether the <see cref="AudioDevice"/> is a recording device (<c><see langword="true"/></c>) or a playback device (<c><see langword="false"/></c>)
	/// </value>
	public bool IsRecordingDevice
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mRecording;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mRecording = value;
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
			nameof(AudioDeviceId)}: {mWhich.ToString(format, formatProvider)}, {
			nameof(IsRecordingDevice)}: {(bool)mRecording} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(AudioDeviceId)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mWhich, ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite($", {nameof(IsRecordingDevice)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite((bool)mRecording, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
