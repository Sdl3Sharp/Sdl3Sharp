using Sdl3Sharp.Internal;
using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when the <see cref="Clipboard"/> is updated
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.ClipboardUpdated"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct ClipboardEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString();

	private CommonEvent mCommon;
	private CBool mOwner;
	private readonly int mNumMimeTypes; // Since `mMimeTypes` is readonly, this field is also should be readonly as well.
	private unsafe readonly byte** mMimeTypes; // There's no safe way to set this field from the managed side, that's why it's readonly.

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="ClipboardEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(ClipboardEvent)}: {type}.", nameof(value));

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
	/// Gets or sets a value indicating whether SDL "owns" the clipboard (e.g., the clipboard update is internal one)
	/// </summary>
	/// <value>
	/// A value indicating whether SDL "owns" the clipboard (e.g., the clipboard update is internal one)
	/// </value>
	public bool IsOwned
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mOwner;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mOwner = value;
	}

	/// <summary>
	/// Gets the MIME types of the data currently on the clipboard
	/// </summary>
	/// <value>
	/// The MIME types of the data currently on the clipboard
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
				var mimeTypesPtr = mMimeTypes;

				if (mimeTypesPtr is null || mNumMimeTypes is not > 0)
				{
					return [];
				}

				var mimeTypes = GC.AllocateUninitializedArray<string>(mNumMimeTypes);

				foreach (ref var mimeType in mimeTypes.AsSpan())
				{
					using var mimeTypeUtf16 = NativeStrings.FromUtf8ToUtf16(*mimeTypesPtr++);
					mimeType = mimeTypeUtf16.ToManaged()!;
				}

				return mimeTypes;
			}
		}
	}

	/// <inheritdoc/>
	public readonly override string ToString()
	{
		unsafe
		{
			return $"{{ {mCommon.ToPartialString()}, {
				nameof(IsOwned)}: {(bool)mOwner}, {
				nameof(MimeTypes)}: [{(mNumMimeTypes is > 0 ? $" {string.Join(", ", NativeStrings.EnumerateFromUtf8ToUtf16(mMimeTypes, mNumMimeTypes).Select(static c => c is not null ? $"\"{c}\"" : "null"))} " : string.Empty)}] }}";
		}
	}

	/// <inheritdoc/>
	readonly string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString();

	/// <inheritdoc cref="ISpanFormattable.TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten)
	{
		unsafe
		{
			charsWritten = 0;

			if ( !(SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
				&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
				&& SpanFormat.TryWrite($", {nameof(IsOwned)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite((bool)mOwner, ref destination, ref charsWritten)
				&& SpanFormat.TryWrite($", {nameof(MimeTypes)}: [", ref destination, ref charsWritten)))
			{
				return false;
			}

			if (mMimeTypes is not null && mNumMimeTypes is > 0)
			{
				if (!SpanFormat.TryWrite(' ', ref destination, ref charsWritten))
				{
					return false;
				}

				var mimeTypesPtr = mMimeTypes;

				{
					using var mimeTypeUtf16 = NativeStrings.FromUtf8ToUtf16(*mimeTypesPtr++);
					
					if (mimeTypeUtf16.Buffer is not null)
					{
						if ( !(SpanFormat.TryWrite('"', ref destination, ref charsWritten)
							&& SpanFormat.TryWrite(mimeTypeUtf16.AsSpan(), ref destination, ref charsWritten)
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

				for (var i = 1; i < mNumMimeTypes; i++)
				{
					if (!SpanFormat.TryWrite(", ", ref destination, ref charsWritten))
					{
						return false;
					}

					using var mimeTypeUtf16 = NativeStrings.FromUtf8ToUtf16(*mimeTypesPtr++);

					if (mimeTypeUtf16.Buffer is not null)
					{
						if ( !(SpanFormat.TryWrite('"', ref destination, ref charsWritten)
							&& SpanFormat.TryWrite(mimeTypeUtf16.AsSpan(), ref destination, ref charsWritten)
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

				if (!SpanFormat.TryWrite(' ', ref destination, ref charsWritten))
				{
					return false;
				}
			}

			return SpanFormat.TryWrite("] }", ref destination, ref charsWritten);
		}
	}

	/// <inheritdoc/>
	readonly bool ISpanFormattable.TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider) => TryFormat(destination, out charsWritten);
}
