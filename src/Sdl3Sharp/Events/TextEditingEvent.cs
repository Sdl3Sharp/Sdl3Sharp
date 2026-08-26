using Sdl3Sharp.Internal;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when the user is editing text
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.TextEditing"/></description></item>
/// </list>
/// </para>
/// <para>
/// Note that <see cref="TextEditingEvent"/>s won't be received unless text input was started for a <see cref="Video.Windowing.Window"/> by calling <see cref="Window.TryStartTextInput()"/> or <see cref="Window.TryStartTextInput(Sdl3Sharp.Input.TextInputType?, Sdl3Sharp.Input.Capitalization?, bool?, bool?, string?, string?, string?, int?, Sdl3Sharp.Properties?)"/> on that window.
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct TextEditingEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWindowID;
	private unsafe readonly byte* mText; // Because we have no safe way to set text from the managed side, this field is readonly.
	private int mStart;
	private int mLength;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="TextEditingEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(TextEditingEvent)}: {type}.", nameof(value));

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
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="TextEditingEvent"/> is most likely the window that currently has keyboard focus, if any.
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
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="TextEditingEvent"/> is most likely the window that currently has keyboard focus, if any.
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
	/// Gets the editing text
	/// </summary>
	/// <value>
	/// The editing text
	/// </value>
	/// <remarks>
	/// <para>
	/// Reading this property can be very expensive, you should consider caching it's value.
	/// If you want to get the <see cref="Text"/>, <see cref="Start"/>, and <see cref="Length"/> simultaneously, consider using the <see cref="GetTextStartAndLength"/> method as it can be more efficient to get all three quantities at once. 
	/// </para>
	/// </remarks>
	/// <exception cref="InvalidOperationException">The text that the <see cref="TextEditingEvent"/> contains is <c><see langword="null"/></c></exception>
	public readonly string Text // This property is readonly because we have no safe way to set text from the managed side
	{
		get
		{
			unsafe
			{
				if (mText is null)
				{
					[DoesNotReturn]
					static void failTextNull() => throw new InvalidOperationException($"The {nameof(Text)} that the {nameof(TextEditingEvent)} contains is null.");

					failTextNull();
				}

				using var textUtf16 = NativeStrings.FromUtf8ToUtf16(mText);
				return textUtf16.ToManaged()!; // this would only return null if the `mText` pointer is null
			}
		}
	}

	internal unsafe readonly byte* TextUtf8 // This property is readonly because we have no safe way to set text from the managed side
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mText;
	}

	/// <summary>
	/// Gets or sets the starting cursor position of the selected part of the <see cref="Text">editing text</see>
	/// </summary>
	/// <value>
	/// The starting cursor position of the selected part of the <see cref="Text">editing text</see>, or <c>-1</c> if not set
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of the property represents the character position into <see cref="Text"/> where new typing will be inserted.
	/// </para>
	/// <para>
	/// Reading and writing this property can be very expensive, you should consider caching it's value.
	/// If you want to get the <see cref="Text"/>, <see cref="Start"/>, and <see cref="Length"/> simultaneously, consider using the <see cref="GetTextStartAndLength"/> method as it can be more efficient to get all three quantities at once.
	/// </para>
	/// </remarks>
	/// <exception cref="InvalidOperationException">
	/// When getting this property, the text that the <see cref="TextEditingEvent"/> contains is <c><see langword="null"/></c> and the value of <see cref="Start"/> value is not <c>-1</c>
	/// - OR -
	/// When getting this property, the value of <see cref="Start"/> is out of range for the current <see cref="Text"/>
	/// - OR -
	/// When getting or setting this property, the text that the <see cref="TextEditingEvent"/> contains has invalid UTF-8 data
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// When setting this property, the text that the <see cref="TextEditingEvent"/> contains is <c><see langword="null"/></c> and the value being set is not <c>-1</c>
	/// - OR -
	/// When setting this property, the value being set is out of range for the current <see cref="Text"/>
	/// </exception>
	public int Start
	{
		readonly get
		{
			unsafe
			{
				if (mStart is < 0)
				{
					return -1;
				}

				if (mText is null)
				{
					[DoesNotReturn]
					static void failTextNull() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains an invalid {nameof(Start)} value that is out of range when {nameof(Text)} is null.");

					failTextNull();
				}

				if (mStart is 0)
				{
					return 0;
				}

				var text = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(mText);
				var startUtf16 = 0;

				for (var scalar = 0; scalar < mStart; scalar++)
				{
					if (text.Length is not > 0)
					{
						// this means the `mStatrt` value is out of range for the current text -> throw an exception

						[DoesNotReturn]
						static void failOutOfRange() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains an invalid {nameof(Start)} value that is out of range for the current {nameof(Text)}.");

						failOutOfRange();
					}

					if (Rune.DecodeFromUtf8(text, out var rune, out var bytesConsumed) is not OperationStatus.Done)
					{
						// this is actually bad and an error we can't really recover from -> throw an exception

						[DoesNotReturn]
						static void failInvalidUtf8() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains invalid UTF-8 data in its {nameof(TextUtf8)}.");

						failInvalidUtf8();
					}

					startUtf16 += rune.Utf16SequenceLength;
					text = text[bytesConsumed..];
				}

				return startUtf16;
			}
		}

		set
		{
			unsafe
			{
				if (value is < 0)
				{
					mStart = -1;
					return;
				}

				if (mText is null)
				{
					[DoesNotReturn]
					static void failTextNull(int value) => throw new ArgumentOutOfRangeException(nameof(value), value, $"Cannot set the {nameof(Start)} value to anything other than -1 when {nameof(Text)} is null.");

					failTextNull(value);
				}

				if (value is 0)
				{
					mStart = 0;
					return;
				}

				var text = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(mText);
				var scalar = 0;

				for (var startUtf16 = 0; startUtf16 < value; scalar++)
				{
					if (text.Length is not > 0)
					{
						// this means the given value is out of range for the current text -> throw an exception

						[DoesNotReturn]
						static void failOutOfRange(int value) => throw new ArgumentOutOfRangeException(nameof(value), value, $"The given {nameof(value)} is out of range for the current {nameof(Text)}.");

						failOutOfRange(value);
					}

					if (Rune.DecodeFromUtf8(text, out var rune, out var bytesConsumed) is not OperationStatus.Done)
					{
						// this is actually bad and an error we can't really recover from -> throw an exception

						[DoesNotReturn]
						static void failInvalidUtf8() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains invalid UTF-8 data in its {nameof(TextUtf8)}.");

						failInvalidUtf8();
					}

					startUtf16 += rune.Utf16SequenceLength;
					text = text[bytesConsumed..];
				}

				mStart = scalar;
			}
		}
	}

	internal int StartUtf8
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mStart;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mStart = value;
	}

	/// <summary>
	/// Gets or sets the length of the selected part of the <see cref="Text">editing text</see>
	/// </summary>
	/// <value>
	/// The length of the selected part of the <see cref="Text">editing text</see>, or <c>-1</c> if not set
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property represents the number of characters that will be replaced in <see cref="Text"/> by the new typing.
	/// </para>
	/// <para>
	/// Reading and writing this property can be very expensive, you should consider caching it's value.
	/// If you want to get the <see cref="Text"/>, <see cref="Start"/>, and <see cref="Length"/> simultaneously, consider using the <see cref="GetTextStartAndLength"/> method as it can be more efficient to get all three quantities at once.
	/// </para>
	/// </remarks>
	/// <exception cref="InvalidOperationException">
	/// When getting this property, the text that the <see cref="TextEditingEvent"/> contains is <c><see langword="null"/></c> and the value of <see cref="Length"/> is not <c>-1</c> or <c>0</c>
	/// - OR -
	/// When getting this property, the value of <see cref="Start"/> or <see cref="Length"/> is out of range for the current <see cref="Text"/>
	/// - OR -
	/// When getting or setting this property, the text that the <see cref="TextEditingEvent"/> contains has invalid UTF-8 data
	/// </exception>
	/// <exception cref="ArgumentOutOfRangeException">
	/// When setting this property, the text that the <see cref="TextEditingEvent"/> contains is <c><see langword="null"/></c> and the value being set is not <c>-1</c> or <c>0</c>
	/// - OR -
	/// When setting this property, the value being set is out of range for the current <see cref="Text"/>
	/// </exception>
	public int Length
	{
		readonly get
		{
			unsafe
			{
				if (mLength is < 0)
				{
					return -1;
				}

				if (mLength is 0)
				{
					return 0;
				}

				if (mText is null)
				{
					[DoesNotReturn]
					static void failTextNull() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains an invalid {nameof(Length)} value that is out of range when {nameof(Text)} is null.");

					failTextNull();
				}

				var text = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(mText);

				for (var scalar = 0; scalar < mStart; scalar++)
				{
					if (text.Length is not > 0)
					{
						// this means the `mStart` value is out of range for the current text -> throw an exception

						[DoesNotReturn]
						static void failOutOfRange() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains an invalid {nameof(Start)} value that is out of range for the current {nameof(Text)}.");

						failOutOfRange();
					}

					if (Rune.DecodeFromUtf8(text, out _, out var bytesConsumed) is not OperationStatus.Done)
					{
						// this is actually bad and an error we can't really recover from -> throw an exception

						[DoesNotReturn]
						static void failInvalidUtf8() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains invalid UTF-8 data in its {nameof(TextUtf8)}.");

						failInvalidUtf8();
					}

					text = text[bytesConsumed..];
				}

				var lengthUtf16 = 0;

				for (var scalar = 0; scalar < mLength; scalar++)
				{
					if (text.Length is not > 0)
					{
						// this means the `mLength` value is out of range for the current text -> throw an exception

						[DoesNotReturn]
						static void failOutOfRange() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains an invalid {nameof(Length)} value that is out of range for the current {nameof(Text)}.");
						failOutOfRange();
					}

					if (Rune.DecodeFromUtf8(text, out var rune, out var bytesConsumed) is not OperationStatus.Done)
					{
						// this is actually bad and an error we can't really recover from -> throw an exception
						[DoesNotReturn]
						static void failInvalidUtf8() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains invalid UTF-8 data in its {nameof(TextUtf8)}.");

						failInvalidUtf8();
					}

					lengthUtf16 += rune.Utf16SequenceLength;
					text = text[bytesConsumed..];
				}

				return lengthUtf16;
			}
		}

		set
		{
			unsafe
			{
				if (value is < 0)
				{
					mLength = -1;
					return;
				}

				if (value is 0)
				{
					mLength = 0;
					return;
				}

				if (mText is null)
				{
					[DoesNotReturn]
					static void failTextNull() => throw new InvalidOperationException($"Cannot set the {nameof(Length)} value to anything other than -1 or 0 when {nameof(Text)} is null.");

					failTextNull();
				}

				var text = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(mText);
				var scalar = 0;

				for (; scalar < mStart; scalar++)
				{
					if (text.Length is not > 0)
					{
						// this means the given value is out of range for the current text -> throw an exception

						[DoesNotReturn]
						static void failOutOfRange(int value) => throw new ArgumentOutOfRangeException(nameof(value), value, $"The given {nameof(value)} is out of range for the current {nameof(Text)}.");

						failOutOfRange(value);
					}

					if (Rune.DecodeFromUtf8(text, out _, out var bytesConsumed) is not OperationStatus.Done)
					{
						// this is actually bad and an error we can't really recover from -> throw an exception

						[DoesNotReturn]
						static void failInvalidUtf8() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains invalid UTF-8 data in its {nameof(TextUtf8)}.");

						failInvalidUtf8();
					}

					text = text[bytesConsumed..];
				}

				scalar = 0;

				for (var lengthUtf16 = 0; lengthUtf16 < value; scalar++)
				{
					if (text.Length is not > 0)
					{
						// this means the given value is out of range for the current text -> throw an exception

						[DoesNotReturn]
						static void failOutOfRange(int value) => throw new ArgumentOutOfRangeException(nameof(value), value, $"The given {nameof(value)} is out of range for the current {nameof(Text)}.");

						failOutOfRange(value);
					}

					if (Rune.DecodeFromUtf8(text, out var rune, out var bytesConsumed) is not OperationStatus.Done)
					{
						// this is actually bad and an error we can't really recover from -> throw an exception

						[DoesNotReturn]
						static void failInvalidUtf8() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains invalid UTF-8 data in its {nameof(TextUtf8)}.");

						failInvalidUtf8();
					}

					lengthUtf16 += rune.Utf16SequenceLength;
					text = text[bytesConsumed..];
				}

				mLength = scalar;
			}
		}
	}

	internal int LengthUtf8
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mLength;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mLength = value;
	}

	/// <summary>
	/// Gets the editing text, the starting cursor position of the selected part of the editing text, and the length of the selected part of the editing text simultaneously
	/// </summary>
	/// <param name="text">The editing text</param>
	/// <param name="start">The starting cursor position of the selected part of the editing text, or <c>-1</c> if not set</param>
	/// <param name="length">The length of the selected part of the editing text, or <c>-1</c> if not set</param>
	/// <exception cref="InvalidOperationException">
	/// The text that the <see cref="TextEditingEvent"/> contains is <c><see langword="null"/></c>
	/// - OR -
	/// The value of <see cref="Start"/> or <see cref="Length"/> is out of range for the current <see cref="Text"/>
	/// - OR -
	/// The text that the <see cref="TextEditingEvent"/> contains has invalid UTF-8 data
	/// </exception>
	public readonly void GetTextStartAndLength(out string text, out int start, out int length)
	{
		unsafe
		{
			if (mText is null)
			{
				[DoesNotReturn]
				static void failTextNull() => throw new InvalidOperationException($"The {nameof(Text)} that the {nameof(TextEditingEvent)} contains is null.");

				failTextNull();
			}

			using (var textUtf16 = NativeStrings.FromUtf8ToUtf16(mText))
			{
				text = textUtf16.ToManaged()!;
			}

			var textSpan = text.AsSpan();

			if (mStart is < 0)
			{
				start = -1;
			}
			else if (mStart is 0)
			{
				start = 0;
			}
			else
			{
				start = 0;

				for (var scalar = 0; scalar < mStart; scalar++)
				{
					if (text.Length is not > 0)
					{
						// this means the `mStart` value is out of range for the current text -> throw an exception

						[DoesNotReturn]
						static void failOutOfRange() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains an invalid {nameof(Start)} value that is out of range for the current {nameof(Text)}.");

						failOutOfRange();
					}

					if (Rune.DecodeFromUtf16(textSpan, out _, out var charsConsumed) is not OperationStatus.Done)
					{
						// this is actually bad and an error we can't really recover from -> throw an exception

						[DoesNotReturn]
						static void failInvalidUtf16() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains invalid data in its {nameof(Text)}.");

						failInvalidUtf16();
					}

					start += charsConsumed;
					textSpan = textSpan[charsConsumed..];
				}
			}

			if (mLength is < 0)
			{
				length = -1;
			}
			else if (mLength is 0)
			{
				length = 0;
			}
			else
			{
				length = 0;

				for (var scalar = 0; scalar < mLength; scalar++)
				{
					if (text.Length is not > 0)
					{
						// this means the `mLength` value is out of range for the current text -> throw an exception
						[DoesNotReturn]

						static void failOutOfRange() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains an invalid {nameof(Length)} value that is out of range for the current {nameof(Text)}.");
						failOutOfRange();
					}

					if (Rune.DecodeFromUtf16(textSpan, out _, out var charsConsumed) is not OperationStatus.Done)
					{
						// this is actually bad and an error we can't really recover from -> throw an exception

						[DoesNotReturn]
						static void failInvalidUtf16() => throw new InvalidOperationException($"The {nameof(TextEditingEvent)} contains invalid data in its {nameof(Text)}.");

						failInvalidUtf16();
					}

					length += charsConsumed;
					textSpan = textSpan[charsConsumed..];
				}
			}
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
	{
		const string fallbackValueMessage = "could not get value";

		unsafe
		{
			using var textUtf16 = NativeStrings.FromUtf8ToUtf16(mText);

			Unsafe.SkipInit(out int? start);
			try
			{
				start = Start;
			}
			catch
			{
				start = null;
			}

			Unsafe.SkipInit(out int? length);
			try
			{
				length = Length;
			}
			catch
			{
				length = null;
			}

			return $"{{ {mCommon.ToPartialString()}, {
				nameof(WindowId)}: {mWindowID.ToString(format, formatProvider)}, {
				nameof(Text)}: {textUtf16.Buffer switch { not null => $"\"{textUtf16.ToManaged()}\"", null => "null" }}, {
				nameof(Start)}: {start switch { int startValue => startValue.ToString(format, formatProvider), null => fallbackValueMessage }}, {
				nameof(Length)}: {length switch { int lengthValue => lengthValue.ToString(format, formatProvider), null => fallbackValueMessage }} }}";
		}
	}

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		const string fallbackValueMessage = "could not get value";

		unsafe
		{
			charsWritten = 0;

			if ( !(SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
				&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
				&& SpanFormat.TryWrite($", {nameof(WindowId)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mWindowID, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(Text)}: ", ref destination, ref charsWritten)))
			{
				return false;
			}

			using var textUtf16 = NativeStrings.FromUtf8ToUtf16(mText);

			if (textUtf16.Buffer is not null)
			{
				if ( !(SpanFormat.TryWrite('"', ref destination, ref charsWritten)
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

			if (!SpanFormat.TryWrite($", {nameof(Start)}: ", ref destination, ref charsWritten))
			{
				return false;
			}

			Unsafe.SkipInit(out int? start);
			try
			{
				start = Start;
			}
			catch
			{
				start = null;
			}

			if (start is int startValue)
			{
				if (!SpanFormat.TryWrite(startValue, ref destination, ref charsWritten, format, provider))
				{
					return false;
				}
			}
			else
			{
				if (!SpanFormat.TryWrite(fallbackValueMessage, ref destination, ref charsWritten))
				{
					return false;
				}
			}

			if (!SpanFormat.TryWrite($", {nameof(Length)}: ", ref destination, ref charsWritten))
			{
				return false;
			}

			Unsafe.SkipInit(out int? length);
			try
			{
				length = Length;
			}
			catch
			{
				length = null;
			}

			if (length is int lengthValue)
			{
				if (!SpanFormat.TryWrite(lengthValue, ref destination, ref charsWritten, format, provider))
				{
					return false;
				}
			}
			else
			{
				if (!SpanFormat.TryWrite(fallbackValueMessage, ref destination, ref charsWritten))
				{
					return false;
				}
			}

			return SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
		}
	}
}
