using Sdl3Sharp.Internal;
using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when a list of keyboard IME candidates is displayed or updated
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.TextEditingCandidates"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct TextEditingCandidatesEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWindowID;
	private readonly byte** mCandidates; // There's no safe way to set this field from the managed side, that's why it's readonly.
	private readonly int mNumCandidates; // Since `mCandidates` is readonly, this field is also should be readonly as well.
	private int mSelectedCandidate;
	private CBool mHorizontal;
	private readonly byte mPadding1, mPadding2, mPadding3;

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="TextEditingCandidatesEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(TextEditingCandidatesEvent)}: {type}.", nameof(value));

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
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="TextEditingCandidatesEvent"/> is most likely the window that currently has keyboard focus, if any.
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
	/// The associated <see cref="Video.Windowing.Window"/> with a <see cref="TextEditingCandidatesEvent"/> is most likely the window that currently has keyboard focus, if any.
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
	/// Gets a collection of keyboard IME candidates
	/// </summary>
	/// <value>
	/// A collection of keyboard IME candidates, or an empty collection if there are no candidates
	/// </value>
	/// <remarks>
	/// <para>
	/// Reading this property can be very expensive, you should consider caching it's value.
	/// </para>
	/// </remarks>
	public readonly string[] Candidates
	{
		get
		{
			unsafe
			{
				var candidatesPtr = mCandidates;

				if (candidatesPtr is null || mNumCandidates is not > 0)
				{
					return [];
				}

				var candidates = GC.AllocateUninitializedArray<string>(mNumCandidates);

				foreach (ref var candidate in candidates.AsSpan())
				{
					using var candidateUtf16 = NativeStrings.FromUtf8ToUtf16(*candidatesPtr++);
					candidate = candidateUtf16.ToManaged()!;
				}

				return candidates;
			}
		}
	}

	/// <summary>
	/// Gets or sets the index of the selected keyboard IME candidate into the <see cref="Candidates"/> collection
	/// </summary>
	/// <value>
	/// The index of the selected keyboard IME candidate into the <see cref="Candidates"/> collection, or <c>-1</c> if no candidate is selected
	/// </value>
	/// <exception cref="ArgumentOutOfRangeException">
	/// When setting this property, the given value is out of range for the <see cref="Candidates"/> collection and not equal to <c>-1</c>
	/// </exception>
	public int SelectedCandidateIndex
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mSelectedCandidate;
		
		set
		{
			if (value is < -1 || value >= mNumCandidates)
			{
				[DoesNotReturn]
				static void failInvalidSelectedCandidateIndex(int value) => throw new ArgumentOutOfRangeException(nameof(value), value, $"The given selected candidate index is out of range for the {nameof(TextEditingCandidatesEvent)}.");

				failInvalidSelectedCandidateIndex(value);
			}

			mSelectedCandidate = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the list of keyboard IME candidates is horizontal
	/// </summary>
	/// <value>
	/// A value indicating whether the list of keyboard IME candidates is horizontal
	/// </value>
	public bool IsHorizontal
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mHorizontal;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mHorizontal = value;
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
			return $"{{ {mCommon.ToPartialString()}, {
				nameof(WindowId)}: {mWindowID.ToString(format, formatProvider)}, {
				nameof(Candidates)}: [{(mNumCandidates is > 0 ? $" {string.Join(", ", NativeStrings.EnumerateFromUtf8ToUtf16(mCandidates, mNumCandidates).Select(static c => c is not null ? $"\"{c}\"" : "null"))} " : string.Empty)}], {
				nameof(SelectedCandidateIndex)}: {mSelectedCandidate.ToString(format, formatProvider)}, {
				nameof(IsHorizontal)}: {mHorizontal} }}";
		}
	}

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		unsafe
		{
			charsWritten = 0;

			if (!(SpanFormat.TryWrite("{ ", ref destination, ref charsWritten)
				&& mCommon.TryPartiallyFormat(ref destination, ref charsWritten)
				&& SpanFormat.TryWrite($", {nameof(WindowId)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mWindowID, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(Candidates)}: [", ref destination, ref charsWritten)))
			{
				return false;
			}

			if (mCandidates is not null && mNumCandidates is > 0)
			{
				if (!SpanFormat.TryWrite(' ', ref destination, ref charsWritten))
				{
					return false;
				}

				var candidatesPtr = mCandidates;

				{
					using var candidateUtf16 = NativeStrings.FromUtf8ToUtf16(*candidatesPtr++);
					
					if (candidateUtf16.Buffer is not null)
					{
						if (!(SpanFormat.TryWrite('"', ref destination, ref charsWritten)
							&& SpanFormat.TryWrite(candidateUtf16.AsSpan(), ref destination, ref charsWritten)
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

				for (var i = 1; i < mNumCandidates; i++)
				{
					if (!SpanFormat.TryWrite(", ", ref destination, ref charsWritten))
					{
						return false;
					}

					using var candidateUtf16 = NativeStrings.FromUtf8ToUtf16(*candidatesPtr++);
					
					if (candidateUtf16.Buffer is not null)
					{
						if (!(SpanFormat.TryWrite('"', ref destination, ref charsWritten)
							&& SpanFormat.TryWrite(candidateUtf16.AsSpan(), ref destination, ref charsWritten)
							&& SpanFormat.TryWrite('"', ref destination, ref charsWritten)))
						{
							return false;
						}
						else
						{
							if (!SpanFormat.TryWrite("null", ref destination, ref charsWritten))
							{
								return false;
							}
						}
					}
				}

				if (!SpanFormat.TryWrite(' ', ref destination, ref charsWritten))
				{
					return false;
				}
			}

			return SpanFormat.TryWrite($"], {nameof(SelectedCandidateIndex)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mSelectedCandidate, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(IsHorizontal)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite((bool)mHorizontal, ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
		}
	}
}
