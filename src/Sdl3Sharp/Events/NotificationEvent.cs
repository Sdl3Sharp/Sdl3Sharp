#if SDL3_6_0_OR_GREATER

using Sdl3Sharp.Internal;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

/// <summary>
/// Represents an event that occurs when a <see cref="Utilities.Notification"/> is interacted with
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>:
/// <list type="bullet">
/// <item><description><see cref="EventType.NotificationActionInvoked"/></description></item>
/// </list>
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public partial struct NotificationEvent : IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private CommonEvent mCommon;
	private uint mWhich;
	private unsafe readonly byte* mActionId; // There's no safe way to set this field from the managed side, that's why it's readonly.

	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for a <see cref="NotificationEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mCommon.Type;

		set
		{
			if (!AcceptsEventType(value))
			{
				[DoesNotReturn]
				static void failInvalidEventType(EventType type) => throw new ArgumentException($"Invalid event type for {nameof(NotificationEvent)}: {type}.", nameof(value));

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
	/// Gets or sets the <see cref="Notification.Id">ID</see> of the <see cref="Utilities.Notification"/> associated with this event
	/// </summary>
	/// <value>
	/// The <see cref="Notification.Id">ID</see> of the <see cref="Utilities.Notification"/> associated with this event
	/// </value>
	public uint NotificationId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] readonly get => mWhich;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mWhich = value;
	}

	// TODO: Add a `Notification` property once the `Notification` type is implemented

	/// <summary>
	/// Gets the <see cref="NotificationActionButton.ActionId">action identifier</see> of the action that was invoked
	/// </summary>
	/// <value>
	/// The <see cref="NotificationActionButton.ActionId">action identifier</see> of the action that was invoked, or <see cref="Notification.DefaultActionId"/> (<c>"default"</c>) if the notification was interacted with without selecting a specific option (e.g., clicking on the notification itself)
	/// </value>
	public readonly string ActionId
	{
		get
		{
			unsafe
			{
				using var actionIdUtf16 = NativeStrings.FromUtf8ToUtf16(mActionId);
				return actionIdUtf16.ToManaged()!;
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
			using var actionIdUtf16 = NativeStrings.FromUtf8ToUtf16(mActionId);

			return $"{{ {mCommon.ToPartialString()}, {
				nameof(NotificationId)}: {mWhich.ToString(format, formatProvider)}, {
				nameof(ActionId)}: {actionIdUtf16.Buffer switch { not null => $"\"{actionIdUtf16.ToManaged()}\"", null => "null" }} }}";
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
				&& SpanFormat.TryWrite($", {nameof(NotificationId)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mWhich, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(ActionId)}: ", ref destination, ref charsWritten)))
			{
				return false;
			}

			using (var actionIdUtf16 = NativeStrings.FromUtf8ToUtf16(mActionId))
			{
				if (actionIdUtf16.Buffer is not null)
				{
					if (!(SpanFormat.TryWrite('"', ref destination, ref charsWritten)
						&& SpanFormat.TryWrite(actionIdUtf16.AsSpan(), ref destination, ref charsWritten)
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

#endif
