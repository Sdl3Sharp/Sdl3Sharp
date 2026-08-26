using Sdl3Sharp.Internal;
using System;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents a simple user-defined event that can be sent and received by user code and can be associated with custom data
/// </summary>
/// <para>
/// If you want to <see cref="EventQueue.TryPushEvent(in Event)">push an <see cref="SimpleUserEvent"/> to the event queue</see>, make sure to keep it alive until it is removed from the event queue (e.g., by <see cref="EventQueue.TryWaitForEvent(out Event)"/>, or by receiving it in <see cref="App.OnEvent(ref Event)"/>),
/// otherwise it might get disposed and garbage collected before you have a chance to receive it. Once you've received it, make sure to <see cref="UserEvent.Dispose()">dispose</see> it when you're done using it.
/// </para>
/// </remarks>
public sealed class SimpleUserEvent : UserEvent
{
	// `Data1` and `Data2` mimic the `Data1` and `Data2` fields of `UnmanagedUserEvent` in a managed way, allowing for arbitrary data to be associated with the event.
	// In that sense it's a managed interpretation of `SDL_UserEvent`, but it also serves as an example of how to create user-defined events inheriting from `UserEvent` and associating custom data with them.

	/// <summary>
	/// Gets or sets the first user-defined data slot
	/// </summary>
	/// <value>
	/// The first user-defined data slot
	/// </value>
	/// <remarks>
	/// <para>
	/// You can freely set this to any value of your liking. For example, you could use this property to associate some custom data with your user-defined events.
	/// </para>
	/// </remarks>
	public object? Data1 { get; set; }

	/// <summary>
	/// Gets or sets the second user-defined data slot
	/// </summary>
	/// <value>
	/// The second user-defined data slot
	/// </value>
	/// <remarks>
	/// <para>
	/// You can freely set this to any value of your liking. For example, you could use this property to associate some custom data with your user-defined events.
	/// </para>
	/// </remarks>
	public object? Data2 { get; set; }

	/// <inheritdoc/>
	protected override string ToPartialString(string? format, IFormatProvider? formatProvider, bool includeCode = true)
		=> $"{base.ToPartialString(format, formatProvider, includeCode)}, {
			nameof(Data1)}: {Data1 switch
			{
				IFormattable formattable => formattable.ToString(format, formatProvider),
				not null and var data1 => data1.ToString() ?? "null",
				_ => "null"
			}}, {
			nameof(Data2)}: {Data2 switch
			{
				IFormattable formattable => formattable.ToString(format, formatProvider),
				not null and var data2 => data2.ToString() ?? "null",
				_ => "null"
			}}";

	/// <inheritdoc/>
	protected override bool TryPartiallyFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = null, bool includeCode = true)
	{
		if (!base.TryPartiallyFormat(destination, out charsWritten, format, provider, includeCode))
		{
			return false;
		}

		if (!SpanFormat.TryWrite($", {nameof(Data1)}: ", ref destination, ref charsWritten))
		{
			return false;
		}

		switch (Data1)
		{
			case ISpanFormattable formattable:
				{
					var result = formattable.TryFormat(destination, out int tmpCharsWritten, format, provider);

					charsWritten += tmpCharsWritten;

					if (!result)
					{
						return false;
					}

					destination = destination[tmpCharsWritten..];
				}
				break;

			case not null and var data1:
				if (!SpanFormat.TryWrite(data1.ToString() ?? "null", ref destination, ref charsWritten))
				{
					return false;
				}
				break;

			default:
				if (!SpanFormat.TryWrite("null", ref destination, ref charsWritten))
				{
					return false;
				}
				break;
		}

		if (!SpanFormat.TryWrite($", {nameof(Data2)}: ", ref destination, ref charsWritten))
		{
			return false;
		}

		switch (Data2)
		{
			case ISpanFormattable formattable:
				{
					var result = formattable.TryFormat(destination, out int tmpCharsWritten, format, provider);

					charsWritten += tmpCharsWritten;

					if (!result)
					{
						return false;
					}

					destination = destination[tmpCharsWritten..];
				}
				break;

			case not null and var data2:
				if (!SpanFormat.TryWrite(data2.ToString() ?? "null", ref destination, ref charsWritten))
				{
					return false;
				}
				break;

			default:
				if (!SpanFormat.TryWrite("null", ref destination, ref charsWritten))
				{
					return false;
				}
				break;
		}

		return true;
	}
}
