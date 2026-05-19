using Sdl3Sharp.Internal;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events2;

partial struct Event
{
	[FieldOffset(0)] internal Event<CameraDeviceEventData> CDevice;
}

/// <summary>
/// Represents event data for an event that occurs when a <see cref="Camera">camera device</see> is being <see cref="EventType.CameraDeviceAdded">added</see>, <see cref="EventType.CameraDeviceRemoved">removed</see>, <see cref="EventType.CameraDeviceApproved">approved</see>, or <see cref="EventType.CameraDeviceDenied">denied</see>
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.CameraDeviceAdded"/></description></item> 
/// <item><description><see cref="EventType.CameraDeviceRemoved"/></description></item> 
/// <item><description><see cref="EventType.CameraDeviceApproved"/></description></item>
/// <item><description><see cref="EventType.CameraDeviceDenied"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public struct CameraDeviceEventData : IEventData<CameraDeviceEventData>, IFormattable, ISpanFormattable
{
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static bool IEventData<CameraDeviceEventData>.AcceptsEventType(EventType type) => type is >= EventType.CameraDeviceAdded and <= EventType.CameraDeviceDenied;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private uint mWhich;

	/// <summary>
	/// Gets or sets the camera device ID for the <see cref="Camera"/> being <see cref="EventType.CameraDeviceAdded">added</see>, <see cref="EventType.CameraDeviceRemoved">removed</see>, <see cref="EventType.CameraDeviceApproved">approved</see>, or <see cref="EventType.CameraDeviceDenied">denied</see>
	/// </summary>
	/// <value>
	/// The camera device ID for the <see cref="Camera"/> being <see cref="EventType.CameraDeviceAdded">added</see>, <see cref="EventType.CameraDeviceRemoved">removed</see>, <see cref="EventType.CameraDeviceApproved">approved</see>, or <see cref="EventType.CameraDeviceDenied">denied</see>
	/// </value>
	public uint CameraId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWhich;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWhich = value;
	}

	private readonly string ToPartialString(string? format, IFormatProvider? formatProvider)
		=> $"{nameof(CameraId)}: {CameraId.ToString(format, formatProvider)}";

	readonly string IEventData<CameraDeviceEventData>.ToPartialString(string? format, IFormatProvider? formatProvider)
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
		=> SpanFormat.TryWrite($"{nameof(CameraId)}: ", ref destination, ref charsWritten)
		&& SpanFormat.TryWrite(CameraId, ref destination, ref charsWritten, format, provider);

	readonly bool IEventData<CameraDeviceEventData>.TryPartiallyFormat(ref Span<char> destination, ref int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
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
