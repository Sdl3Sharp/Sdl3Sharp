using Sdl3Sharp.Internal;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when the application is requested to quit
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.Quit"/></description></item>
/// </list>
/// </para>
/// </remarks>
public partial struct QuitEvent : IFormattable, ISpanFormattable
{
	private CommonEvent mCommon;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="QuitEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(QuitEvent)}: {type}.", nameof(value));

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

	/// <inheritdoc/>
	public readonly override string ToString()
		=> $"{{ {mCommon.ToPartialString()} }}";

	/// <inheritdoc/>
	readonly string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString();

	/// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}

	/// <inheritdoc/>
	readonly bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => TryFormat(destination, out charsWritten);
}
