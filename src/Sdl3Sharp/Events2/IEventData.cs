using System;

namespace Sdl3Sharp.Events2;

/// <summary>
/// Represents an interface that marks the struct type <typeparamref name="TEventData"/> to be used as specific event data for an <see cref="Event{TEventData}"/>
/// </summary>
/// <typeparam name="TEventData">The struct type which is used as specific event data for an <see cref="Event{TEventData}"/></typeparam>
public interface IEventData<TEventData>
	where TEventData : unmanaged, IEventData<TEventData>
{
	internal static abstract bool AcceptsEventType(EventType type);

	internal string ToPartialString(string? format, IFormatProvider? formatProvider);

	internal bool TryPartiallyFormat(ref Span<char> destination, ref int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider);
}
