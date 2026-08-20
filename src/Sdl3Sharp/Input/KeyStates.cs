using Sdl3Sharp.Internal.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents the states of all keys on the keyboard, where each key is represented by a <see cref="Scancode"/>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly partial struct KeyStates : IReadOnlyDictionary<Scancode, bool> // It's fine for `KeyStates` to not be a `ref struct`,
	                                                                           // the underlying `mStates` pointer is guaranteed to be unchanged and valid for the whole lifetime of the application anyways.
{
	private const Scancode FirstValidScancode = Scancode.A,
						   LastValidScancode = Scancode.EndCall;

	private unsafe readonly CBool* mStates;
	private readonly Scancode mEnd;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	internal unsafe KeyStates(CBool* states, int numKeys)
	{
		mStates = states;
		mEnd = unchecked((Scancode)int.Min(numKeys, unchecked((int)LastValidScancode + 1)));
	}

	/// <summary>
	/// Gets the number of key states in the current <see cref="KeyStates"/> collection
	/// </summary>
	/// <value>
	/// The number of key states in the current <see cref="KeyStates"/> collection
	/// </value>
	public readonly int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
		get => unchecked((int)mEnd - (int)FirstValidScancode);
	}

	/// <summary>
	/// Gets the state of the key represented by the given <see cref="Scancode"/>
	/// </summary>
	/// <param name="key">The <see cref="Scancode"/> representing the key whose state is to be retrieved</param>
	/// <returns><c><see langword="true"/></c>, if the key represented by the given <see cref="Scancode"/> is pressed; otherwise, <c><see langword="false"/></c></returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="key"/> does not represent a valid key on the keyboard</exception>
	public readonly bool this[Scancode key]
	{
		get
		{
			unsafe
			{
				if (key is < FirstValidScancode || key >= mEnd) // For performance reasons, we do not check for `Enum.IsDefined` here,
																// although we do that in the `Keys` and `Values` enumeration, and in the `ContainsKey` and `TryGetValue` methods.
				{
					[DoesNotReturn]
					static void failKeyArgumentOutOfRange(Scancode key) => throw new ArgumentOutOfRangeException(nameof(key), $"Invalid {nameof(Scancode)} value: {key}.");

					failKeyArgumentOutOfRange(key);
				}

				return mStates[unchecked((int)key)];
			}
		}
	}

	/// <inheritdoc/>
	IEnumerable<Scancode> IReadOnlyDictionary<Scancode, bool>.Keys
	{
		get
		{
			for (Scancode key = FirstValidScancode; key < mEnd; key++)
			{
				if (Enum.IsDefined(key))
				{
					yield return key;
				}
			}
		}
	}

	/// <inheritdoc/>
	IEnumerable<bool> IReadOnlyDictionary<Scancode, bool>.Values
	{
		get
		{
			for (Scancode key = FirstValidScancode; key < mEnd; key++)
			{
				if (Enum.IsDefined(key))
				{
					Unsafe.SkipInit(out bool value);
					unsafe
					{
						value = mStates[unchecked((int)key)];
					}

					yield return value;
				}
			}
		}
	}

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	readonly bool IReadOnlyDictionary<Scancode, bool>.ContainsKey(Scancode key)
		=> key is >= FirstValidScancode && key < mEnd && Enum.IsDefined(key);

	/// <inheritdoc/>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	readonly bool IReadOnlyDictionary<Scancode, bool>.TryGetValue(Scancode key, out bool value)
	{
		unsafe
		{
			if (key is >= FirstValidScancode && key < mEnd && Enum.IsDefined(key))
			{
				value = mStates[unchecked((int)key)];
				return true;
			}

			value = default;
			return false;
		}
	}
}
