using Sdl3Sharp.Internal;
using Sdl3Sharp.Timing;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events2;

/// <summary>
/// Represents a general event
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Event"/>s are used to represent events in a more abstract way, without specifying the exact type of event data they contain.
/// Any <see cref="Event{TEventData}"/> can be stored in an <see cref="Event"/>.
/// </para>
/// <para>
/// Using <see cref="EventRef"/> and <see cref="EventRefReadOnly"/> to handle references to <see cref="Event"/>s is far more common than handling <see cref="Event"/>s directly,
/// and they can be safely turned into reference to specific <see cref="EventRef{TEventData}"/> or <see cref="EventRefReadOnly{TEventData}"/> by using the
/// <see cref="EventRef.TryAs{TEventData}(out EventRef{TEventData})"/>, <see cref="EventRef.TryAsReadOnly{TEventData}(out EventRefReadOnly{TEventData})"/>, and <see cref="EventRefReadOnly.TryAsReadOnly{TEventData}(out EventRefReadOnly{TEventData})"/> methods, respectively.
/// </para>
/// <para>
/// Some of SDL's events aren't associated with any specific event data, the related <see cref="EventType"/>s of those events are:
/// <list type="bullet">
/// <item><description><see cref="EventType.Terminating"/></description></item>
/// <item><description><see cref="EventType.LowMemory"/></description></item>
/// <item><description><see cref="EventType.WillEnterBackground"/></description></item>
/// <item><description><see cref="EventType.DidEnterBackground"/></description></item>
/// <item><description><see cref="EventType.WillEnterForeground"/></description></item>
/// <item><description><see cref="EventType.DidEnterForeground"/></description></item>
/// <item><description><see cref="EventType.LocaleChanged"/></description></item>
/// <item><description><see cref="EventType.SystemThemeChanged"/></description></item>
/// <item><description><see cref="EventType.KeymapChanged"/></description></item>
/// <item><description><see cref="EventType.ScreenKeyboardShown"/></description></item>
/// <item><description><see cref="EventType.ScreenKeyboardHidden"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Explicit)]
public partial struct Event : IFormattable, ISpanFormattable
{
	[InlineArray(128)] private struct Padding { private byte _; }

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	[FieldOffset(0)] private CommonEventData mCommonEventData;
	[FieldOffset(0)] private Padding mPadding;

	/// <summary>
	/// Gets or sets the <see cref="EventType">type</see> of the event
	/// </summary>
	/// <value>
	/// The <see cref="EventType">type</see> of the event
	/// </value>
	public EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommonEventData.Type;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mCommonEventData.Type = value;
	}

	/// <summary>
	/// Gets or sets the timestamp of the event
	/// </summary>
	/// <value>
	/// The timestamp of the event, in nanoseconds in nanoseconds since the <see cref="Sdl(Sdl.BuildAction?)">initialization of SDL</see>
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property usually describes the time passed, in nanoseconds since the <see cref="Sdl(Sdl.BuildAction?)">initialization of SDL</see>.
	/// It can be properly populated by using <see cref="Timer.NanosecondTicks"/>.
	/// </para>
	/// </remarks>
	public ulong Timestamp
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommonEventData.Timestamp;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mCommonEventData.Timestamp = value;
	}

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {mCommonEventData.ToPartialString()} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommonEventData.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
