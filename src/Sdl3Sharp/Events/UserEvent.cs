using Sdl3Sharp.Internal;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Events;

// Because we need managed lifetime management for "managed" user events, that's the only event type that is declared as a class, not a struct.
// Also, since this is a reference type, it allows all inheriting types to participate in union pattern matching as well (on .NET 11+).

/// <summary>
/// Represents a base class for user-defined events that can be sent and received by user code
/// </summary>
/// <remarks>
/// <para>
/// Associated <see cref="EventType"/>s:
/// <list type="bullet">
/// <item><description><see cref="EventType"/>s obtained from <see cref="EventTypeExtensions.TryRegister(out EventType)"/> or <see cref="EventTypeExtensions.TryRegister(Span{EventType})"/></description></item>
/// </list>
/// </para>
/// <para>
/// <see cref="UserEvent"/> is a high-level representation of a user-defined event, and it is recommended to use this class as a base class for user-defined events in most use cases.
/// If you need a more low-level representation of a user-defined event, you can use <see cref="UnmanagedUserEvent"/> instead,
/// but please make sure to read the remarks in the documentation for <see cref="UnmanagedUserEvent"/> first, as it has some important caveats regarding managed and unmanaged user-defined events and mixing them in the same code.
/// </para>
/// <para>
/// If you want to <see cref="EventQueue.TryPushEvent(in Event)">push an <see cref="UserEvent"/> (including types inheriting from it) to the event queue</see>, make sure to keep it alive until it is removed from the event queue (e.g., by <see cref="EventQueue.TryWaitForEvent(out Event)"/>, or by receiving it in <see cref="App.OnEvent(ref Event)"/>),
/// otherwise it might get disposed and garbage collected before you have a chance to receive it. Once you've received it, make sure to <see cref="Dispose()">dispose</see> it when you're done using it.
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
public abstract partial class UserEvent : IDisposable, IFormattable, ISpanFormattable
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private UnmanagedUserEvent mUnmanaged;

	/// <summary>
	/// Creates a new <see cref="UserEvent"/> instance
	/// </summary>
	protected UserEvent()
	{
		unsafe
		{
			// Unmanaged event data fields `Data1` and `Data2` are entirely controlled by this type, and won't be exposed to the user.

			mUnmanaged.Data1 = unchecked((void*)GCHandle.ToIntPtr(GCHandle.Alloc(this, GCHandleType.Normal))); // Manage the lifetime of this instance by allocating and storing a GCHandle into the unmanaged event data
			mUnmanaged.Data2 = null; // We don't use this field for anything, so we can just set it to null
		}
	}

	/// <inheritdoc/>
	~UserEvent() => DisposeImpl(disposing: false);

	/// <remarks>
	/// <para>
	/// <see cref="EventType"/>s used for <see cref="UserEvent"/> must be obtained from <see cref="EventTypeExtensions.TryRegister(out EventType)"/> or <see cref="EventTypeExtensions.TryRegister(Span{EventType})"/>.
	/// </para>
	/// </remarks>
	/// <inheritdoc/>
	/// <exception cref="ArgumentException">
	/// When setting this property, the given <see cref="EventType"/> is not a valid type for an <see cref="UnmanagedUserEvent"/>
	/// </exception>
	public required EventType Type
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mUnmanaged.Type;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mUnmanaged.Type = value; // `UnmanagedUserEvent` already validates the event type correctly
	}

	/// <inheritdoc/>
	public ulong Timestamp
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mUnmanaged.Timestamp;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mUnmanaged.Timestamp = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Window.Id">ID</see> of the <see cref="Video.Windowing.Window"/> associated with this event, or <c>0</c> if no window is associated with this event
	/// </value>
	public uint WindowId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mUnmanaged.WindowId;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mUnmanaged.WindowId = value;
	}

	/// <summary>
	/// Gets or sets the <see cref="Video.Windowing.Window"/> associated with this event, if any
	/// </summary>
	/// <value>
	/// The <see cref="Video.Windowing.Window"/> associated with this event, or <c><see langword="null"/></c> if no window is associated with this event
	/// </value>
	public Window? Window
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mUnmanaged.Window;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mUnmanaged.Window = value;
	}

	/// <summary>
	/// Gets or sets the user-defined event code
	/// </summary>
	/// <value>
	/// The user-defined event code
	/// </value>
	/// <remarks>
	/// <para>
	/// You can freely set this to any value of your liking. For example, you could use this property to further distinguish your user-defined events.
	/// </para>
	/// </remarks>
	public int Code
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mUnmanaged.Code;
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] set => mUnmanaged.Code = value;
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		DisposeImpl(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Returns a string representation of data of the user event, without surrounding formatting (e.g., braces)
	/// </summary>
	/// <param name="includeCode">A value indicating whether to include the <see cref="Code"/> property in the string representation</param>
	/// <returns>A string representation of data of the user event, without surrounding formatting (e.g., braces)</returns>
	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
#pragma warning disable CS1573 // We already pull the parameter documentation of the missing parameter from `IFormattable.ToString(string?, IFormatProvider?)` via `<inheritdoc/>`
	protected virtual string ToPartialString(string? format, IFormatProvider? formatProvider, bool includeCode = true)
#pragma warning restore CS1573
		=> $"{nameof(Type)}: {mUnmanaged.Type}, {
			nameof(Timestamp)}: {CommonEvent.FormatTimestamp(mUnmanaged.Timestamp)}{
			(includeCode
				? $", {nameof(Code)}: {mUnmanaged.Code.ToString(format, formatProvider)}"
				: string.Empty
			)}";

	/// <inheritdoc/>
	public sealed override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {ToPartialString(format, formatProvider)} }}";

	/// <summary>
	/// Tries to format the data of user event into the provided <paramref name="destination"/> span, without surrounding formatting (e.g., braces)
	/// </summary>
	/// <param name="includeCode">A value indicating whether to include the <see cref="Code"/> property in the string representation</param>
	/// <returns><c><see langword="true"/></c> if the formatting was successful; otherwise, <c><see langword="false"/></c></returns>
	/// <inheritdoc cref="TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)"/>
#pragma warning disable CS1573 // We already pull the parameter documentation of the missing parameter from `TryFormat(Span{char}, out int, ReadOnlySpan{char}, IFormatProvider?)` via `<inheritdoc/>`
	protected virtual bool TryPartiallyFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default, bool includeCode = true)
#pragma warning restore CS1573
	{
		charsWritten = 0;

		if ( !(SpanFormat.TryWrite($"{nameof(Type)}: ", ref destination, ref charsWritten)
			&& SpanFormat.TryWrite(mUnmanaged.Type, ref destination, ref charsWritten)
			&& SpanFormat.TryWrite($", {nameof(Timestamp)}: ", ref destination, ref charsWritten)
			&& CommonEvent.TryFormatTimestamp(mUnmanaged.Timestamp, ref destination, ref charsWritten)))
		{
			return false;
		}

		if (includeCode)
		{
			if ( !(SpanFormat.TryWrite($", {nameof(Code)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mUnmanaged.Code, ref destination, ref charsWritten, format, provider)))
			{
				return false;
			}
		}

		return true;
	}

	/// <inheritdoc/>
	public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		charsWritten = 0;

		if (!SpanFormat.TryWrite("{ ", ref destination, ref charsWritten))
		{
			return false;
		}

		var result = TryPartiallyFormat(destination, out int tmpCharsWritten, format, provider);

		charsWritten += tmpCharsWritten;

		if (!result)
		{
			return false;
		}

		destination = destination[tmpCharsWritten..];

		return SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
	}

	/// <summary>
	/// Disposes the user event
	/// </summary>
	/// <param name="disposing">A value indicating whether the call came from a call to <see cref="Dispose()"/> or from the finalizer</param>
	protected virtual void Dispose(bool disposing) { }

	private void DisposeImpl(bool disposing)
	{
		unsafe
		{
			if (mUnmanaged.Data1 is not null and var handle)
			{
				try
				{
					Dispose(disposing);
				}
				finally
				{
					// Always make sure to release the GCHandle when the managed user event is disposed.
					// That's why this code is here and not in the `Dispose(bool)` method, to make sure it's executed even if an overriding class doesn't call base.Dispose(disposing).

					if (GCHandle.FromIntPtr(unchecked((IntPtr)handle)) is { IsAllocated: true, Target: UserEvent userEvent } gcHandle && ReferenceEquals(this, userEvent))
					{
						gcHandle.Free();
					}

					mUnmanaged.Data1 = null;
				}
			}
		}
	}
}
