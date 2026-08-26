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
/// Represents an event that occurs when a file or plain text is drag-and-dropped, or when the system requests a file to be opened
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.DropFile"/></description></item>
/// <item><description><see cref="EventType.DropText"/></description></item>
/// <item><description><see cref="EventType.DropBegin"/></description></item>
/// <item><description><see cref="EventType.DropCompleted"/></description></item>
/// <item><description><see cref="EventType.DropPosition"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct DropEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWindowID;
	private float mX;
	private float mY;
	private unsafe readonly byte* mSource; // There's no safe way to set this text from the managed side, so the field is readonly
	private unsafe readonly byte* mData; // There's no safe way to set this text from the managed side, so the field is readonly

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="DropEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(DropEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> that is dropped onto, if any
	/// </summary>
	/// <value>
	/// The <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> that is dropped onto, or <c>0</c> if the window is not known
	/// </value>
	public uint WindowId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWindowID;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWindowID = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Video.Windowing.Window"/> that is dropped onto, if any
	/// </summary>
	/// <value>
	/// The <see cref="Video.Windowing.Window"/> that is dropped onto, or <c><see langword="null"/></c> if the window is not known
	/// </value>
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
	/// Gets or sets the horizontal position of the drop event, relative to the <see cref="Window"/> that is dropped onto, if any
	/// </summary>
	/// <value>
	/// The horizontal position of the drop event, relative to the <see cref="Window"/> that is dropped onto, if any. This value is not valid for <see cref="EventType.DropBegin"/> events.
	/// </value>
	public float X
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mX;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mX = value;
	}

	/// <summary>
	/// Gets or sets the vertical position of the drop event, relative to the <see cref="Window"/> that is dropped onto, if any
	/// </summary>
	/// <value>
	/// The vertical position of the drop event, relative to the <see cref="Window"/> that is dropped onto, if any. This value is not valid for <see cref="EventType.DropBegin"/> events.
	/// </value>
	public float Y
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mY;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mY = value;
	}

	/// <summary>
	/// Gets the source (application) of this drop event, if any
	/// </summary>
	/// <value>
	/// The source (application) of this drop event, or <c><see langword="null"/></c> if the source isn't available
	/// </value>
	public readonly string? Source // readonly because the associated field is readonly
	{
		get
		{
			unsafe
			{
				using var sourceUtf16 = NativeStrings.FromUtf8ToUtf16(mSource);
				return sourceUtf16.ToManaged();
			}
		}
	}

	/// <summary>
	/// Gets the data associated with this drop event, if any
	/// </summary>
	/// <value>
	/// The data associated with this drop event:
	/// <list type="bullet">
	///		<item>
	///			<term>When the <see cref="Type"/> is <see cref="EventType.DropText"/></term>
	///			<description>The plain text data that is dropped</description>
	///		</item>
	///		<item>
	///			<term>When the <see cref="Type"/> is <see cref="EventType.DropFile"/></term>
	///			<description>The file name of the file that is dropped or requested to be opened</description>
	///		</item>
	///		<item>
	///			<term>All values for <see cref="Type"/></term>
	///			<description><c><see langword="null"/></c></description>
	///		</item>
	/// </list>
	/// </value>
	public readonly string? Data // readonly because the associated field is readonly
	{
		get
		{
			unsafe
			{
				using var dataUtf16 = NativeStrings.FromUtf8ToUtf16(mData);
				return dataUtf16.ToManaged();
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
		unsafe
		{
			var sourceUtf16 = NativeStrings.FromUtf8ToUtf16(mSource);
			var dataUtf16 = NativeStrings.FromUtf8ToUtf16(mData);

			return $"{{ {mCommon.ToPartialString()}, {
				nameof(WindowId)}: {mWindowID.ToString(format, formatProvider)}, {
				nameof(X)}: {X.ToString(format, formatProvider)}, {
				nameof(Y)}: {Y.ToString(format, formatProvider)}, {
				nameof(Source)}: {sourceUtf16.Buffer switch { not null => $"\"{sourceUtf16.ToManaged()}\"", null => "null" }}, {
				nameof(Data)}: {dataUtf16.Buffer switch { not null => $"\"{dataUtf16.ToManaged()}\"", null => "null" }} }}";
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
				&& SpanFormat.TryWrite($", {nameof(X)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(X, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(Y)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(Y, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(Source)}: ", ref destination, ref charsWritten)))
			{
				return false;
			}

			using (var sourceUtf16 = NativeStrings.FromUtf8ToUtf16(mSource))
			{
				if (sourceUtf16.Buffer is not null)
				{
					if ( !(SpanFormat.TryWrite('"', ref destination, ref charsWritten)
						&& SpanFormat.TryWrite(sourceUtf16.AsSpan(), ref destination, ref charsWritten)
						&& SpanFormat.TryWrite('"', ref destination, ref charsWritten)))
					{
						return false;
					}
				}
				else
				{
					if (!SpanFormat.TryWrite("null", ref destination, ref charsWritten))
					{
						return false;
					}
				}
			}

			if (!SpanFormat.TryWrite($", {nameof(Data)}: ", ref destination, ref charsWritten))
			{
				return false;
			}

			using (var dataUtf16 = NativeStrings.FromUtf8ToUtf16(mData))
			{
				if (dataUtf16.Buffer is not null)
				{
					if ( !(SpanFormat.TryWrite('"', ref destination, ref charsWritten)
						&& SpanFormat.TryWrite(dataUtf16.AsSpan(), ref destination, ref charsWritten)
						&& SpanFormat.TryWrite('"', ref destination, ref charsWritten)))
					{
						return false;
					}
				}
				else
				{
					if (!SpanFormat.TryWrite("null", ref destination, ref charsWritten))
					{
						return false;
					}
				}
			}

			return SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
		}
	}
}
