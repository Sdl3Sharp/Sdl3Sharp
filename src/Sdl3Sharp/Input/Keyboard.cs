using Sdl3Sharp.Events;
using Sdl3Sharp.Internal;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents a keyboard device connected to the system
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="Id"/> of a keyboard is unique, remains unchanged while the keyboard is connected to the system, and is never reused for the lifetime of the application.
/// If a keyboard is disconnected and then reconnected, it will be assigned a new <see cref="Id"/>.
/// </para>
/// </remarks>
[DebuggerDisplay($"{{{nameof(DebuggerDisplay)},nq}}")]
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct Keyboard :
	IEquatable<Keyboard>, IFormattable, ISpanFormattable, IEqualityOperators<Keyboard, Keyboard, bool>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly string DebuggerDisplay => ToString(formatProvider: CultureInfo.InvariantCulture);

	private readonly uint mId;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal Keyboard(uint id) => mId = id;

	/// <summary>
	/// Gets a collection of all keyboards that are currently connected to the system
	/// </summary>
	/// <value>
	/// A collection of all keyboards that are currently connected to the system
	/// </value>
	/// <remarks>
	/// <para>
	/// The collection returned by this property will include any device or virtual driver that includes keyboard functionality, including some mice, KVM switches, motherboard power buttons, etc.
	/// You should wait for input from a keyboard device before you consider it actively in use.
	/// </para>
	/// <para>
	/// The value of this property will change with an <see cref="EventType.KeyboardAdded"/> or <see cref="EventType.KeyboardRemoved"/> event (<see cref="KeyboardDeviceEvent"/>) received.
	/// You should not query this property too often, but rather cache the result and update the cache when such an event is raised.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't get the keyboards connected to the system (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public static Keyboard[] ConnectedKeyboards
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out int count);

				var keyboards = SDL_GetKeyboards(&count);

				if (keyboards is null)
				{
					[DoesNotReturn]
					static void failCouldNotGetKeyboards() => throw new SdlException($"Couldn't get the keyboards connected to the system");

					failCouldNotGetKeyboards();
				}

				try
				{
					if (count is not > 0)
					{
						return [];
					}

					var result = GC.AllocateUninitializedArray<Keyboard>(count);

					var keyboardPtr = keyboards;
					foreach (ref var keyboard in result.AsSpan())
					{
						keyboard = new(*keyboardPtr++);
					}

					return result;
				}
				finally
				{
					Utilities.NativeMemory.Free(keyboards);
				}
			}
		}
	}

	/// <summary>
	/// Gets the window that currently has keyboard focus
	/// </summary>
	/// <value>
	/// The window that currently has keyboard focus, or <c><see langword="null"/></c> if no window has keyboard focus
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public static Window? FocusWindow
	{
		get
		{
			unsafe
			{
				Window.TryGetOrCreate(SDL_GetKeyboardFocus(), out var window);
				return window;
			}
		}
	}

	/// <summary>
	/// Gets the numeric ID of this keyboard
	/// </summary>
	/// <value>
	/// The numeric ID of this keyboard
	/// </value>
	/// <remarks>
	/// <para>
	/// The <see cref="Id"/> of a keyboard is unique, remains unchanged while the keyboard is connected to the system, and is never reused for the lifetime of the application.
	/// If a keyboard is disconnected and then reconnected, it will be assigned a new <see cref="Id"/>.
	/// </para>
	/// <para>
	/// An ID of <c>0</c> indicates an invalid keyboard.
	/// </para>
	/// </remarks>
	public readonly uint Id { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mId; }

	/// <summary>
	/// Gets a value indicating whether at least one keyboard is currently connected to the system
	/// </summary>
	/// <value>
	/// A value indicating whether at least one keyboard is currently connected to the system
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public static bool IsAnyConnected => SDL_HasKeyboard();

	/// <summary>
	/// Gets a value indicating whether the current platform supports screen keyboards
	/// </summary>
	/// <value>
	/// A value indicating whether the current platform supports screen keyboards
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public static bool IsScreenKeyboardSupported => SDL_HasScreenKeyboardSupport();

	/// <summary>
	/// Gets the name of this keyboard
	/// </summary>
	/// <value>
	/// The name of this keyboard, <see cref="string.Empty"><c>""</c></see> if the keyboard has no name, or <c><see langword="null"/></c> if the name couldn't be retrieved successfully (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public readonly string? Name
	{
		get
		{
			unsafe
			{
				// To be consistent with how `Mouse.Name` works, we return null here if the keyboard name is null, although that's considered an error by SDL.
				// Just like `Mouse.Name`, we document that the user should check for errors using Error.TryGet if the result is null.
				using var nameUtf16 = NativeStrings.FromUtf8ToUtf16(SDL_GetKeyboardNameForID(mId));
				return nameUtf16.ToManaged();
			}
		}
	}

	/// <summary>
	/// Gets or sets the current state of the keyboard modifier keys
	/// </summary>
	/// <value>
	/// The current state of the keyboard modifier keys
	/// </value>
	/// <remarks>
	/// <para>
	/// This property allows you to impose modifier key states by setting the value.
	/// Note that this will not change the actual state of the physical keyboard, only the key modifier flags that SDL reports.
	/// </para>
	/// </remarks>
	public static Keymod ModifiersState
	{
		get => SDL_GetModState();
		set => SDL_SetModState(value);
	}

	/// <summary>
	/// Gets a snapshot of the current state of the keyboard
	/// </summary>
	/// <remarks>
	/// <para>
	/// The property returns the current state after all events have been processed, so if a key or button has been pressed and released before processing events,
	/// then the pressed state will never show up in the value returned by this property.
	/// </para>
	/// <para>
	/// To update the key states returned by this property, call <see cref="SDL_PumpEvents()"/>.
	/// </para>
	/// <para>
	/// Note that this property doesn't take into account whether shift has been pressed or not.
	/// </para>
	/// </remarks>
	public static KeyStates State
	{
		get
		{
			unsafe
			{
				Unsafe.SkipInit(out int numkeys);

				var states = SDL_GetKeyboardState(&numkeys);

				return new(states, numkeys);
			}
		}
	}

	/// <inheritdoc/>
	public readonly override bool Equals([NotNullWhen(true)] object? obj) => obj is Keyboard other && Equals(other);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly bool Equals(Keyboard other) => mId == other.mId;

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public readonly override int GetHashCode() => mId.GetHashCode();

	/// <summary>
	/// Resets the state of the keyboard, clearing any key states
	/// </summary>
	/// <remarks>
	/// <para>
	/// This method will generate <see cref="EventType.KeyUp"/> (<see cref="KeyboardEvent"/>) events for all keys that are currently pressed.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static void Reset() => SDL_ResetKeyboard();

	/// <inheritdoc/>
	public readonly override string ToString() => ToString(format: default, formatProvider: default);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(IFormatProvider? formatProvider) => ToString(format: default, formatProvider);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public readonly string ToString(string? format) => ToString(format, formatProvider: default);

	/// <inheritdoc/>
	public readonly string ToString(string? format, IFormatProvider? formatProvider)
		=> $"{{ {nameof(Id)}: {mId.ToString(format, formatProvider)}, {
			nameof(Name)}: {Name switch { not null and var name => $"\"{name}\"", _ => "null" }} }}";

	/// <inheritdoc/>
	public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format = default, IFormatProvider? provider = default)
	{
		unsafe
		{
			charsWritten = 0;

			if ( !(SpanFormat.TryWrite($"{{ {nameof(Id)}: ", ref destination, ref charsWritten)
				&& SpanFormat.TryWrite(mId, ref destination, ref charsWritten, format, provider)
				&& SpanFormat.TryWrite($", {nameof(Name)}: ", ref destination, ref charsWritten)))
			{
				return false;
			}

			var name = SDL_GetKeyboardNameForID(mId);

			if (name is not null)
			{
				if ( !(SpanFormat.TryWrite('"', ref destination, ref charsWritten)
					&& SpanFormat.TryWriteUtf8(MemoryMarshal.CreateReadOnlySpanFromNullTerminated(name), ref destination, ref charsWritten)
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

			return SpanFormat.TryWrite(" }", ref destination, ref charsWritten);
		}
	}

	/// <summary>
	/// Tries to get a <see cref="Keyboard"/> by its numeric ID
	/// </summary>
	/// <param name="id">The numeric ID of the keyboard</param>
	/// <param name="keyboard">The <see cref="Keyboard"/> associated with the specified <paramref name="id"/>, if the method returns <c><see langword="true"/></c>; otherwise, <c><see langword="default"/>(<see cref="Keyboard"/>)</c></param>
	/// <returns><c><see langword="true"/></c>, if the <paramref name="id"/> represents a valid <see cref="Keyboard"/>; otherwise, <c><see langword="false"/></c></returns>
	public static bool TryGetFromId(uint id, out Keyboard keyboard)
	{
		if (id is 0)
		{
			keyboard = default;
			return false;
		}

		keyboard = new(id);
		return true;
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator ==(Keyboard left, Keyboard right) => left.Equals(right);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public static bool operator !=(Keyboard left, Keyboard right) => !(left == right);
}
