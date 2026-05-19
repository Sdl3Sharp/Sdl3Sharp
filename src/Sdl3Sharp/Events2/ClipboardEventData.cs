using Sdl3Sharp.Internal;
using Sdl3Sharp.Internal.Interop;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.Events2;

partial struct Event
{
	[FieldOffset(0)] internal Event<ClipboardEventData> Clipboard;
}

/// <summary>
/// Represents event data for an event that occurs when the contents of the clipboard have changed
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType.ClipboardUpdated"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public struct ClipboardEventData : IEventData<ClipboardEventData>, IFormattable, ISpanFormattable
{
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	static bool IEventData<ClipboardEventData>.AcceptsEventType(EventType type) => type is EventType.ClipboardUpdated;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString();

	private CBool mOwner;
	private int mNumMimeTypes;
	private unsafe byte** mMimeTypes;

	/// <summary>
	/// Gets or sets a value indicating whether SDL "owns" the clipboard, meaning it's an internal update event
	/// </summary>
	/// <value>
	/// A value indicating whether SDL "owns" the clipboard, meaning it's an internal update event
	/// </value>
	public bool IsOwned
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mOwner;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mOwner = value;
	}

	/// <summary>
	/// Gets a list of currently available mime types
	/// </summary>
	/// <value>
	/// A list of currently available mime types
	/// </value>
	/// <remarks>
	/// <para>
	/// Reading this property can be very expensive, you should consider caching it's value.
	/// </para>
	/// </remarks>
	public readonly string[] MimeTypes
	{
		get
		{
			unsafe
			{
				var mimeTypes = mMimeTypes;
				var numMimeTypes = mNumMimeTypes;

				if (mimeTypes is null || numMimeTypes is not > 0)
				{
					return [];
				}

				var result = GC.AllocateUninitializedArray<string>(numMimeTypes);

				foreach (ref var mimeType in result.AsSpan())
				{
					mimeType = Utf8StringMarshaller.ConvertToManaged(*mimeTypes++);
				}

				return result;
			}
		}
	}

	private readonly string ToPartialString()
		=> $"{nameof(IsOwned)}: {IsOwned}, {
			nameof(MimeTypes)}: [{MimeTypes switch { { Length: > 0 } mimeTypes => $" {string.Join(", ", mimeTypes.Select(static mimeType => $"\"{mimeType}\""))} ", _ => string.Empty }}]";

	readonly string IEventData<ClipboardEventData>.ToPartialString(string? format, IFormatProvider? formatProvider)
		=> $", {ToPartialString()}";

	/// <inheritdoc/>
	public readonly override string ToString()
		=> $"{{ {ToPartialString()} }}";

	readonly string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString();

	private readonly bool TryPartiallyFormat(ref Span<char> destination, ref int charsWritten)
	{
		unsafe
		{
			if ( !(SpanFormat.TryWrite($"{nameof(IsOwned)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(IsOwned, ref destination, ref charsWritten)
				&& SpanFormat.TryWrite($"{nameof(MimeTypes)}: [", ref destination, ref charsWritten)))
			{
				return false;
			}

			var mimeTypes = mMimeTypes;
			var mimeTypesEnd = mimeTypes + mNumMimeTypes;

			if (mimeTypes is not null && mimeTypes < mimeTypesEnd)
			{
				do
				{
					if ( !(SpanFormat.TryWrite(" \"", ref destination, ref charsWritten)
						&& SpanFormat.TryWriteUtf8(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(*mimeTypes++), ref destination, ref charsWritten)
						&& SpanFormat.TryWrite('\"', ref destination, ref charsWritten)))
					{
						return false;
					}
				}
				while (mimeTypes < mimeTypesEnd);

				if (!SpanFormat.TryWrite(' ', ref destination, ref charsWritten))
				{
					return false;
				}
			}

			return SpanFormat.TryWrite(']', ref destination, ref charsWritten);
		}
	}

	readonly bool IEventData<ClipboardEventData>.TryPartiallyFormat(ref Span<char> destination, ref int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
		=> SpanFormat.TryWrite(", ", ref destination, ref charsWritten)
		&& TryPartiallyFormat(ref destination, ref charsWritten);

	/// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)" />
	public readonly bool TryFormat(Span<char> destination, out int charsWritten)
	{
		charsWritten = 0;

		return SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
			&& TryPartiallyFormat(ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}

	readonly bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => TryFormat(destination, out charsWritten);
}
