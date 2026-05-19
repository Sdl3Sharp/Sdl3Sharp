using Sdl3Sharp.Internal;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events2;

/// <summary>
/// Represents a specific event
/// </summary>
/// <typeparam name="TEventData">The type of the <see cref="EventData">event data</see> of the event</typeparam>
/// <remarks>
/// <para>
/// <see cref="Event{TEventData}"/>s are used to represent events in a more specific way, by specifying the exact type of event data they contain.
/// Any <see cref="Event{TEventData}"/> can be stored in a more abstract <see cref="Event"/>.
/// </para>
/// <para>
/// Using <see cref="EventRef{TEventData}"/> and <see cref="EventRefReadOnly{TEventData}"/> to handle references to <see cref="Event{TEventData}"/>s is far more common than handling <see cref="Event{TEventData}"/>s directly.
/// Unspecific event references, <see cref="EventRef"/> and <see cref="EventRefReadOnly"/>, can be safely turned into references to specific <see cref="EventRef{TEventData}"/> or <see cref="EventRefReadOnly{TEventData}"/> by using the
/// <see cref="EventRef.TryAs{TEventData}(out EventRef{TEventData})"/>, <see cref="EventRef.TryAsReadOnly{TEventData}(out EventRefReadOnly{TEventData})"/>, and <see cref="EventRefReadOnly.TryAsReadOnly{TEventData}(out EventRefReadOnly{TEventData})"/> methods, respectively.
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public struct Event<TEventData> : IFormattable, ISpanFormattable
	where TEventData : unmanaged, IEventData<TEventData>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEventData mCommonEventData;

	/// <summary>
	/// The event data of the event
	/// </summary>
	/// <remarks>
	/// <para>
	/// This field contains the event specific data for the event.
	/// </para>
	/// </remarks>
	public TEventData EventData;

	/// <inheritdoc cref="Event.Type"/>
	/// <remarks>
	/// <para>
	/// When setting this property, the given value must be compatible with the actual type of the event's specific <typeparamref name="TEventData"/> type,
	/// otherwise an <see cref="ArgumentException"/> will be thrown.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentException">When setting this property, the given value is not compatible with the actual type of the event's specific <typeparamref name="TEventData"/> type</exception>
	public EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		readonly get => mCommonEventData.Type;

		set
		{
			if (!TEventData.AcceptsEventType(value))
			{
				failValueArgumentIsNotValid();
			}

			mCommonEventData.Type = value;

			[DoesNotReturn]
			static void failValueArgumentIsNotValid() => throw new ArgumentException($"The given {nameof(value)} is not a valid value for the {nameof(Type)} of this {nameof(Event<>)}", paramName: nameof(value));
		}
	}

	/// <inheritdoc cref="Event.Timestamp"/>
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
		=> $"{{ {mCommonEventData.ToPartialString()}, {EventData.ToPartialString(format, formatProvider)} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommonEventData.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(", ", ref destination, ref charsWritten)
			&& EventData.TryPartiallyFormat(ref destination, ref charsWritten, format, provider)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}
}
