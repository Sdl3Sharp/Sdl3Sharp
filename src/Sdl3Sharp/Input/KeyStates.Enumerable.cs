using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Input;

partial struct KeyStates
{
	/// <summary>
	/// Enumerates the key states in a <see cref="KeyStates"/> collection
	/// </summary>
	/// <param name="keyStates">The <see cref="KeyStates"/> collection to enumerate</param>
	[StructLayout(LayoutKind.Sequential)]
	[method: MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public struct Enumerator(KeyStates keyStates) : IEnumerator<KeyValuePair<Scancode, bool>>
	{
		private readonly KeyStates mKeyStates = keyStates;
		private KeyValuePair<Scancode, bool> mCurrent = new(unchecked(FirstValidScancode - 1), default);

		/// <inheritdoc/>
		public readonly KeyValuePair<Scancode, bool> Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
			get => mCurrent;
		}

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;

		/// <inheritdoc/>
		public bool MoveNext()
		{
			unsafe
			{
				if (mKeyStates.mStates is null)
				{
					return false;
				}

				var nextKey = unchecked(mCurrent.Key + 1);

				while (nextKey < mKeyStates.mEnd)
				{
					if (Enum.IsDefined(nextKey))
					{
						mCurrent = new(nextKey, mKeyStates.mStates[unchecked((int)nextKey)]);
						return true;
					}

					nextKey = unchecked(nextKey + 1);
				}

				return false;
			}
		}

		/// <inheritdoc/>
		readonly void IDisposable.Dispose() { }
		
		/// <inheritdoc/>
		void IEnumerator.Reset()
		{
			mCurrent = new(unchecked(FirstValidScancode - 1), default);
		}
	}

	/// <summary>
	/// Gets an <see cref="Enumerator"/> for the current <see cref="KeyStates"/> collection
	/// </summary>
	/// <returns>An <see cref="Enumerator"/> for the current <see cref="KeyStates"/> collection you can use to enumerate its key states</returns>
	public readonly Enumerator GetEnumerator() => new(this);

	/// <inheritdoc/>
	readonly IEnumerator<KeyValuePair<Scancode, bool>> IEnumerable<KeyValuePair<Scancode, bool>>.GetEnumerator() => GetEnumerator();

	/// <inheritdoc/>
	readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
