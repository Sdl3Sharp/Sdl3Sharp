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
/// Represents an event that occurs the user has input text
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.TextInput"/></description></item>
/// </list>
/// </para>
/// <para>
/// Note that <see cref="TextInputEvent"/>s won't be received unless text input was started for a <see cref="Video.Windowing.Window"/> by calling <see cref="Window.TryStartTextInput()"/> or <see cref="Window.TryStartTextInput(Sdl3Sharp.Input.TextInputType?, Sdl3Sharp.Input.Capitalization?, bool?, bool?, string?, string?, string?, int?, Sdl3Sharp.Properties?)"/> on that window.
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct TextInputEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWindowID;
	private readonly byte* mText; // There's no safe way to set the text from the managed side, so the field is readonly

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="TextInputEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(TextInputEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> associated with this event, or <c>0</c> if no window is associated with this event
	/// </value>
	/// <remarks>
	/// <para>
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="TextInputEvent"/> is most likely the window that currently has keyboard focus, if any.
	/// </para>
	/// </remarks>
	public uint WindowId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWindowID;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWindowID = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Video.Windowing.Window"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Video.Windowing.Window"/> associated with this event, or <c><see langword="null"/></c> if no window is associated with this event
	/// </value>
	/// <remarks>
	/// <para>
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="TextInputEvent"/> is most likely the window that currently has keyboard focus, if any.
	/// </para>
	/// </remarks>
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
	/// Gets the input text
	/// </summary>
	/// <value>
	/// The input text
	/// </value>
	/// <exception cref="InvalidOperationException">The text that the <see cref="TextInputEvent"/> contains is <c><see langword="null"/></c></exception>
	public readonly string Text
	{
		get
		{
			unsafe
			{
				if (mText is null)
				{
					[DoesNotReturn]
					static void failTextNull() => throw new InvalidOperationException($"The {nameof(Text)} that the {nameof(TextInputEvent)} contains is null.");

					failTextNull();
				}

				using var textUtf16 = NativeStrings.FromUtf8ToUtf16(mText);
				return textUtf16.ToManaged()!; // this would only be null if `mText` is null
			}
		}
	}

	internal unsafe readonly byte* TextUtf8
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mText;
	}

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
	{
		unsafe
		{ 
			using var textUtf16 = NativeStrings.FromUtf8ToUtf16(mText);

			return $"{{ {mCommon.ToPartialString()}, {
				nameof(WindowId)}: {mWindowID.ToString(format, formatProvider)}, {
				nameof(Text)}: {textUtf16.Buffer switch { not null => $"\"{textUtf16.ToManaged()}\"", null => "null" }} }}";
			}
	}

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		unsafe
		{
			charsWritten = 0;

			if ( !(SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
				&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
				&& SpanFormat.TryWrite($", {nameof(WindowId)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mWindowID, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(Text)}", ref destination, ref charsWritten)))
			{
				return false;
			}

			using (var textUtf16 = NativeStrings.FromUtf8ToUtf16(mText))
			{
				if (textUtf16.Buffer is not null)
				{
					if (!(SpanFormat.TryWrite('"', ref destination, ref charsWritten)
						&& SpanFormat.TryWrite(textUtf16.AsSpan(), ref destination, ref charsWritten)
						&& SpanFormat.TryWrite('"', ref destination, ref charsWritten)))
					{
						return false;
					}
				}
				else
				{
					if (!(SpanFormat.TryWrite("null", ref destination, ref charsWritten)))
					{
						return false;
					}
				}
			}

			return SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
		}
	}
}
