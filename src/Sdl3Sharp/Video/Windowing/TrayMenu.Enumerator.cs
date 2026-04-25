using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Windowing;

partial class TrayMenu
{
	/// <summary>
	/// Enumerates the tray entries of a tray menu
	/// </summary>
	/// <param name="menu">The tray menu whose entries are to be enumerated</param>
	/// <remarks>
	/// <para>
	/// All of the properties and methods of this type are not thread-safe and must only be accessed from the thread that created the <see cref="Tray">tray</see> for the tray <paramref name="menu"/> that is being enumerated.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="menu"/> is <c><see langword="null"/></c></exception>
	[method: MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public struct Enumerator(TrayMenu menu) : IEnumerator<TrayEntry>
	{
		private static TrayMenu ValidateMenu(TrayMenu menu)
		{
			if (menu is null)
			{
				failMenuArgumentNull();
			}

			return menu;

			[DoesNotReturn]
			static void failMenuArgumentNull() => throw new ArgumentNullException(nameof(menu));
		}

		private readonly TrayMenu mMenu = ValidateMenu(menu);
		private TrayEntry mCurrent = default!;
		private int mPosition = 0;

		/// <inheritdoc/>
		public readonly TrayEntry Current { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mCurrent; }

		/// <inheritdoc/>
		readonly object IEnumerator.Current => Current;

		/// <inheritdoc/>
		public bool MoveNext()
		{
			unsafe
			{
				if (mMenu is null)
				{
					return false;
				}

				var menu = mMenu.mMenu;

				if (menu is null)
				{
					var entries = mMenu.mEntries;

					if (entries is null || mPosition >= entries.Count)
					{
						return false;
					}

					mCurrent = entries[mPosition];
					mPosition++;

					return true;
				}
				else
				{
					Unsafe.SkipInit(out int count);

					var entries = SDL_GetTrayEntries(menu, &count);

					if (entries is null || mPosition >= count)
					{
						return false;
					}

					if (!TrayEntry.TryGetOrCreate(entries[mPosition], out var entry))
					{
						failCouldNotGetEntry();
					}

					mCurrent = entry;
					mPosition++;

					return true;
				}
			}

			[DoesNotReturn]
			static void failCouldNotGetEntry() => throw new SdlException($"Could not get the {nameof(TrayEntry)} at the current position");
		}

		/// <inheritdoc/>
		public readonly void Dispose() { }

		/// <inheritdoc/>
		public void Reset()
		{
			mCurrent = default!;
			mPosition = 0;
		}
	}
}
