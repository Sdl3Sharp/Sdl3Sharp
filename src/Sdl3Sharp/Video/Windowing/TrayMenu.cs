using Sdl3Sharp.Internal;
using Sdl3Sharp.SourceGeneration;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents a menu associated with a tray or a submenu entry, that can contain multiple tray entries
/// </summary>
public sealed partial class TrayMenu : ICollection<TrayEntry>
{
	private static readonly ConcurrentDictionary<IntPtr, WeakReference<TrayMenu>> mKnownInstances = [];

	private unsafe SDL_TrayMenu* mMenu;
	private List<TrayEntry>? mEntries;
	private TrayEntry? mParent;

	internal TrayMenu(TrayEntry? parent)
	{
		unsafe
		{
			mMenu = null;
			mEntries = null;
			mParent = parent;

			// we don't need to update the instance registry here, as the menu is not associated with a native menu yet
		}
	}

	private unsafe TrayMenu(SDL_TrayMenu* menu)
	{
		mMenu = menu;

		Unsafe.SkipInit(out int count);

		var entries = SDL_GetTrayEntries(mMenu, &count);
		if (entries is not null && count is > 0)
		{
			mEntries = [];

			var entriesEnd = entries + count;
			for (var entryPtr = entries; entryPtr < entriesEnd; entryPtr++)
			{
				if (TrayEntry.TryGetOrCreate(*entryPtr, out var entry))
				{
					mEntries.Add(entry);
				}
			}
		}
		else
		{
			mEntries = null;
		}

		mParent = null;

		// we don't need to update the instance registry here, because the only places where this constructor is called from, already take care of that themselves
	}

	/// <summary>
	/// Gets the number of tray entries contained in the tray menu
	/// </summary>
	/// <value>The number of tray entries contained in the tray menu</value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public int Count
	{
		get
		{
			unsafe
			{
				if (mMenu is null)
				{
					return mEntries?.Count ?? 0;
				}

				Unsafe.SkipInit(out int count);

				SDL_GetTrayEntries(mMenu, &count);

				return count;
			}
		}
	}

	/// <inheritdoc/>
	bool ICollection<TrayEntry>.IsReadOnly => false;

	/// <summary>
	/// Gets the tray for which the tray menu is a submenu, if any
	/// </summary>
	/// <value>
	/// The tray for which the tray menu is a submenu, or <c><see langword="null"/></c> if the tray menu is not a submenu of any tray
	/// </value>
	/// <remarks>
	/// <para>
	/// The values of the <see cref="ParentTrayEntry"/> property and the <see cref="ParentTray"/> property are mutually exclusive and are never both non-<c><see langword="null"/></c> at the same time.
	/// If the tray menu is a submenu of a tray, the value of this property will be the non-<c><see langword="null"/></c> parent tray, and the value of the <see cref="ParentTrayEntry"/> property will be guaranteed to be <c><see langword="null"/></c>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public Tray? ParentTray
	{
		get
		{
			unsafe
			{
				if (mMenu is null)
				{
					// TrayMenu instances that are direct children of Trays will always have a native menu associated with them,
					// so if this TrayMenu doesn't have a native menu associated, we can be sure that it doesn't have a parent tray

					return null;
				}

				Tray.TryGet(SDL_GetTrayMenuParentTray(mMenu), out var tray);

				return tray;
			}
		}
	}

	/// <summary>
	/// Gets the tray entry for which the tray menu is a submenu, if any
	/// </summary>
	/// <value>
	/// The tray entry for which the tray menu is a submenu, or <c><see langword="null"/></c> if the tray menu is not a submenu of any tray entry
	/// </value>
	/// <remarks>
	/// <para>
	/// The values of the <see cref="ParentTrayEntry"/> property and the <see cref="ParentTray"/> property are mutually exclusive and are never both non-<c><see langword="null"/></c> at the same time.
	/// If the tray menu is a submenu of a tray entry, the value of this property will be the non-<c><see langword="null"/></c> parent tray entry, and the value of the <see cref="ParentTray"/> property will be guaranteed to be <c><see langword="null"/></c>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public TrayEntry? ParentTrayEntry
	{
		get
		{
			unsafe
			{
				if (mMenu is null)
				{
					return mParent;
				}

				TrayEntry.TryGet(SDL_GetTrayMenuParentEntry(mMenu), out var entry);

				return entry;
			}
		}
	}

	internal unsafe SDL_TrayMenu* Pointer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mMenu; }

	/// <summary>
	/// Adds the specified tray entry to the end of the tray menu
	/// </summary>
	/// <param name="entry">The tray entry to add</param>
	/// <remarks>
	/// <para>
	/// The specified <paramref name="entry"/> must not already have a <see cref="TrayEntry.Parent">parent</see> tray menu.
	/// </para>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> is <c><see langword="null"/></c></exception>
	/// <exception cref="InvalidOperationException"><paramref name="entry"/> already has <see cref="TrayEntry.Parent">parent</see> tray menu</exception>
	/// <exception cref="SdlException">The provided <paramref name="entry"/> could not be added to the end of the tray menu (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public void Add(TrayEntry entry)
		=> Insert(-1, entry);

	/// <summary>
	/// Creates a new tray entry with the specified label and flags, and adds it to the end of the tray menu
	/// </summary>
	/// <param name="label">The label of the tray entry, or <c><see langword="null"/></c>, if the entry should behave like a separator entry</param>
	/// <param name="flags">The flags specifying the behavior of the tray entry</param>
	/// <returns>The newly created tray entry, if it was successfully created and added at the end of the tray menu; otherwise, <c><see langword="null"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Note that some combinations of <paramref name="flags"/> are not valid and may lead to undefined behavior.
	/// Only one of the <see cref="TrayEntryFlags.Button"/>, <see cref="TrayEntryFlags.Checkbox"/>, and <see cref="TrayEntryFlags.Submenu"/> flags should be set, and the <see cref="TrayEntryFlags.Checked"/> flag should only be set if the <see cref="TrayEntryFlags.Checkbox"/> flag is set.
	/// </para>
	/// <para>
	/// This method is primarily intended to specify <paramref name="flags"/> combinations that are not covered by the predefined derived tray entry types.
	/// If you just want to add a regular button, checkbox, submenu, or separator entry, you can just create a new <see cref="ButtonTrayEntry"/>, <see cref="CheckboxTrayEntry"/>, <see cref="SubmenuTrayEntry"/>, or <see cref="SeparatorTrayEntry"/> instance, respectively, and add it using the <see cref="Add(TrayEntry)"/> method.
	/// </para>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public TrayEntry? AddEntry(string? label, TrayEntryFlags flags)
		=> InsertEntry(position: -1, label, flags);

	internal unsafe void Adopt([NotNull] SDL_TrayMenu* menu)
	{
		if (mMenu is not null)
		{
			// very exceptional: if we did everything right, this should never happen
			failMenuAlreadyAssociated();
		}

		mMenu = menu;

		if (mEntries is not null)
		{
			foreach (var entry in mEntries)
			{
				TrayEntry.SDL_TrayEntry* entryPtr;
				var labelUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(entry.Label);
				try
				{
					entryPtr = SDL_InsertTrayEntryAt(mMenu, -1, labelUtf8, entry.Flags);
				}
				finally
				{
					Utf8StringMarshaller.Free(labelUtf8);
				}

				if (entryPtr is null)
				{
					failCouldNotCreateEntry();
				}

				entry.Adopt(entryPtr);
			}

			// we keep mEntries around even after adopting the native menu, so that we still keep a strong ownership reference to the managed entries
		}

		mParent = null;

		Register(menu);

		[DoesNotReturn]
		static void failMenuAlreadyAssociated() => throw new InvalidOperationException($"The {nameof(TrayMenu)} is already associated with an {nameof(SDL_TrayMenu)}* and cannot be associated with another one");

		[DoesNotReturn]
		static void failCouldNotCreateEntry() => throw new SdlException($"Could not create a native tray entry for an existing managed {nameof(TrayEntry)}");
	}

	internal void AdoptBack(TrayEntry? parent)
	{
		const int entryPointersOnStackThreshold = 16;

		unsafe
		{
			if (mMenu is null)
			{
				return;
			}

			mParent = parent;

			Unsafe.SkipInit(out int count);

			var entries = SDL_GetTrayEntries(mMenu, &count);
			if (entries is not null && count is > 0)
			{
				// for safety reasons, we just recreate the managed tray entries entirely based on the native tray entries

				// we need to make a copy of the tray entry pointers, because 'entries' might not be stable across manipulations of the tray menu
				var entriesCopy = count <= entryPointersOnStackThreshold
					? stackalloc IntPtr[count]
					: GC.AllocateUninitializedArray<IntPtr>(count);

				foreach (ref var entry in entriesCopy)
				{
					entry = unchecked((IntPtr)(*entries++));
				}

				var oldEntries = mEntries; // just to make sure that none of the managed entries get collected while we're still recreating the entries list				
				mEntries = [];

				foreach (var entry in entriesCopy)
				{
					var entryPtr = unchecked((TrayEntry.SDL_TrayEntry*)entry);
					
					if (TrayEntry.TryGetOrCreate(entryPtr, out var managedEntry))
					{
						managedEntry.AdoptBack(parent: this);
						mEntries.Add(managedEntry);
					}
				}

				oldEntries?.Clear();
				oldEntries = null;
			}
			else
			{
				mEntries = null;
			}

			mKnownInstances.TryRemove(unchecked((IntPtr)mMenu), out _);

			mMenu = null;
		}
	}

	/// <summary>
	/// Removes all tray entries from the tray menu
	/// </summary>
	/// <remarks>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public void Clear()
	{
		const int entryPointersOnStackThreshold = 16;

		unsafe
		{
			if (mMenu is null)
			{
				if (mEntries is { Count: var count })
				{
					while (count is > 0)
					{
						var entry = mEntries[^1];

						entry.AdoptBack(parent: null);

						mEntries.RemoveAt(count - 1);

						count = mEntries.Count;
					}
				}
			}
			else
			{
				Unsafe.SkipInit(out int count);

				var entries = SDL_GetTrayEntries(mMenu, &count);
				if (entries is not null && count is > 0)
				{
					// we need to make a copy of the tray entry pointers, because 'entries' might not be stable across manipulations of the tray menu
					var entriesCopy = count <= entryPointersOnStackThreshold
						? stackalloc IntPtr[count]
						: GC.AllocateUninitializedArray<IntPtr>(count);

					foreach (ref var entry in entriesCopy)
					{
						entry = unchecked((IntPtr)entries[--count]);
					}

					foreach (var entry in entriesCopy)
					{
						var entryPtr = unchecked((TrayEntry.SDL_TrayEntry*)entry);

						if (TrayEntry.TryGet(entryPtr, out var managedEntry))
						{
							managedEntry.AdoptBack(parent: null);
						}
						else if (entryPtr is not null)
						{
							TrayEntry.SDL_RemoveTrayEntry(entryPtr);
						}
					}
				}

				mEntries?.Clear(); // don't forget to clear the managed entries list as well
			}

			mEntries = null;
		}
	}

	/// <summary>
	/// Determines whether the tray menu contains a specific tray entry
	/// </summary>
	/// <param name="entry">The tray entry to locate in the tray menu</param>
	/// <returns><c><see langword="true"/></c>, if <paramref name="entry"/> is found in the tray menu; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public bool Contains(TrayEntry entry)
	{
		unsafe
		{
			if (mMenu is null)
			{
				return mEntries?.Contains(entry) ?? false;
			}

			var entries = SDL_GetTrayEntries(mMenu, null);

			if (entries is not null)
			{
				while (*entries++ is var entryPtr && entryPtr is not null)
				{
					if (entry.Pointer == entryPtr)
					{
						return true;
					}
				}
			}

			return false;
		}
	}

	/// <summary>
	/// Copies the tray entries of the tray menu to a provided array, starting at a specified index in the array
	/// </summary>
	/// <param name="array">The destination array to which the tray entries will be copied</param>
	/// <param name="arrayIndex">An optional index in the array at which copying begins</param>
	/// <returns>The number of tray entries that were copied to the array</returns>
	/// <remarks>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="array"/> is <c><see langword="null"/></c></exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than <c>0</c>, or greater than the length of the <paramref name="array"/></exception>
	public int CopyTo(TrayEntry[] array, int arrayIndex = 0)
	{
		if (array is null)
		{
			failArrayArgumentNull();
		}

		if (arrayIndex is < 0 || arrayIndex > array.Length)
		{
			failArrayIndexArgumentOutOfRange();
		}

		return CopyTo(array.AsSpan(arrayIndex));

		[DoesNotReturn]
		static void failArrayArgumentNull() => throw new ArgumentNullException(nameof(array));

		[DoesNotReturn]
		static void failArrayIndexArgumentOutOfRange() => throw new ArgumentOutOfRangeException(nameof(arrayIndex));
	}

	/// <inheritdoc/>
	void ICollection<TrayEntry>.CopyTo(TrayEntry[] array, int arrayIndex)
		=> CopyTo(array, arrayIndex);

	/// <summary>
	/// Copies the tray entries of the tray menu to a provided span
	/// </summary>
	/// <param name="destination">The destination span to which the tray entries will be copied</param>
	/// <returns>The number of tray entries that were copied to the destination span</returns>
	/// <remarks>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentException">The provided <paramref name="destination"/> span is too small to hold all the tray entries of the tray menu</exception>
	public int CopyTo(Span<TrayEntry> destination)
	{
		unsafe
		{
			Unsafe.SkipInit(out int count);

			if (mMenu is null)
			{
				if (mEntries is not null)
				{
					count = mEntries.Count;

					if (destination.Length < count)
					{
						failDestinationTooSmall(count);
					}

					mEntries.CopyTo(destination);

					return count;
				}
			}
			else
			{
				var entries = SDL_GetTrayEntries(mMenu, &count);

				if (entries is not null && count is > 0)
				{
					if (destination.Length < count)
					{
						failDestinationTooSmall(count);
					}

					var entriesWritten = 0;
					var entriesEnd = entries + count;
					foreach (ref var entry in destination[..count])
					{
						do
						{
							if (entries >= entriesEnd)
							{
								return entriesWritten;
							}
						}
						while (!TrayEntry.TryGetOrCreate(*entries++, out entry!));

						entriesWritten++;
					}

					return entriesWritten;
				}
			}

			return 0;
		}

		[DoesNotReturn]
		static void failDestinationTooSmall(int requiredCount) => throw new ArgumentException($"The given {nameof(destination)} {nameof(Span<>)} is too small to hold all {requiredCount} {nameof(TrayEntry)}s");
	}

	internal void DisposeInternal()
	{
		unsafe
		{
			mParent = null;

			if (mMenu is null)
			{
				if (mEntries is not null)
				{
					foreach (var entry in mEntries)
					{
						entry.DisposeInternal();
					}
				}
			}
			else
			{
				Unsafe.SkipInit(out int count);

				var entries = SDL_GetTrayEntries(mMenu, &count);
				if (entries is not null && count is > 0)
				{
					var entriesEnd = entries + count;
					for (var entryPtr = entries; entryPtr < entriesEnd; entryPtr++)
					{
						if (TrayEntry.TryGet(*entryPtr, out var entry))
						{
							entry.DisposeInternal();
						}
					}
				}
			}

			mEntries?.Clear();
			mEntries = null;

			if (mMenu is not null)
			{
				mKnownInstances.TryRemove(unchecked((IntPtr)mMenu), out _);
			}

			mMenu = null;
		}
	}

	/// <summary>
	/// Gets a new <see cref="Enumerator"/> enumerating the tray entries of the tray menu
	/// </summary>
	/// <returns>A new <see cref="Enumerator"/> enumerating the tray entries of the tray menu</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	public Enumerator GetEnumerator() => new(this);

	/// <inheritdoc/>
	IEnumerator<TrayEntry> IEnumerable<TrayEntry>.GetEnumerator() => GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	/// <summary>
	/// Determines the index of a specific tray entry in the tray menu
	/// </summary>
	/// <param name="entry">The tray entry to locate in the tray menu</param>
	/// <returns>The index of <paramref name="entry"/> if found in the tray menu; otherwise, <c>-1</c></returns>
	/// <remarks>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public int IndexOf(TrayEntry entry)
	{
		unsafe
		{
			if (mMenu is null)
			{
				return mEntries?.IndexOf(entry) ?? -1;
			}

			var entries = SDL_GetTrayEntries(mMenu, null);

			if (entries is not null)
			{
				var index = 0;
				while (*entries++ is var entryPtr && entryPtr is not null)
				{
					if (entry.Pointer == entryPtr)
					{
						return index;
					}

					index++;
				}
			}

			return -1;
		}
	}

	/// <summary>
	/// Inserts a specific tray entry at a specified position in the tray menu
	/// </summary>
	/// <param name="position">The zero-based position at which to insert the tray entry in the tray menu, or <c>-1</c> to insert the tray entry at the end of the tray menu</param>
	/// <param name="entry">The tray entry to insert</param>
	/// <remarks>
	/// <para>
	/// The specified <paramref name="entry"/> must not already have a <see cref="TrayEntry.Parent">parent</see> tray menu.
	/// </para>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="entry"/> is <c><see langword="null"/></c></exception>
	/// <exception cref="InvalidOperationException"><paramref name="entry"/> already has <see cref="TrayEntry.Parent">parent</see> tray menu</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> is less than <c>-1</c>, or greater than the <see cref="Count">number of entries</see> in the tray menu</exception>
	/// <exception cref="SdlException">The provided <paramref name="entry"/> could not be inserted at the specified <paramref name="position"/> in the tray menu (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public void Insert(int position, TrayEntry entry)
	{
		unsafe
		{
			if (entry is null)
			{
				failEntryArgumentNull();
			}

			if (entry.Pointer is not null // if the entry is already associated with a native entry, we assume that it already has a parent
				|| entry.Parent is not null // if the entry already has a parent (even if it's us self), we cannot insert it
			)
			{
				failEntryAlreadyHasParent();
			}

			if (position is < -1)
			{
				failPositionArgumentOutOfRange();
			}
			else if (mEntries is null)
			{
				if (position is > 0)
				{
					failPositionArgumentOutOfRange();
				}

				mEntries = [];
			}
			else if (position > mEntries.Count)
			{
				failPositionArgumentOutOfRange();
			}

			entry.AdoptBack(parent: this);

			if (mMenu is not null)
			{
				TrayEntry.SDL_TrayEntry* entryPtr;
				var labelUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(entry.Label);
				try
				{
					entryPtr = SDL_InsertTrayEntryAt(mMenu, position, labelUtf8, entry.Flags);
				}
				finally
				{
					Utf8StringMarshaller.Free(labelUtf8);
				}

				if (entryPtr is null)
				{
					entry.AdoptBack(parent: null); // reset the parent

					failCouldNotInsertEntry();
				}

				entry.Adopt(entryPtr);
			}

			if (position is -1)
			{
				mEntries.Add(entry);
			}
			else
			{
				mEntries.Insert(position, entry);
			}
		}

		[DoesNotReturn]
		static void failEntryArgumentNull() => throw new ArgumentNullException(nameof(entry));

		[DoesNotReturn]
		static void failEntryAlreadyHasParent() => throw new InvalidOperationException($"The given {nameof(TrayEntry)} already has a parent {nameof(TrayMenu)}");

		[DoesNotReturn]
		static void failPositionArgumentOutOfRange() => throw new ArgumentOutOfRangeException(nameof(position), $"{nameof(position)} must be greater than or equal to -1, and less than or equal to the number of entries in the tray menu");

		[DoesNotReturn]
		static void failCouldNotInsertEntry() => throw new SdlException($"Could not insert the given {nameof(TrayEntry)} at the given {nameof(position)}");
	}

	[FormattedConstant(SdlErrorHelper.ParameterInvalidErrorFormat, nameof(pos))]
	private static partial string PosParamInvalidError(int pos = default);

	/// <summary>
	/// Creates a new tray entry with the specified label and flags, and inserts it at a specified position in the tray menu
	/// </summary>
	/// <param name="position">The zero-based position at which to insert the new tray entry in the tray menu, or <c>-1</c> to insert the new tray entry at the end of the tray menu</param>
	/// <param name="label">The label of the tray entry, or <c><see langword="null"/></c>, if the entry should behave like a separator entry</param>
	/// <param name="flags">The flags specifying the behavior of the tray entry</param>
	/// <returns>The newly created tray entry, if it was successfully created and inserted at the specified <paramref name="position"/> in the tray menu; otherwise, <c><see langword="null"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Note that some combinations of <paramref name="flags"/> are not valid and may lead to undefined behavior.
	/// Only one of the <see cref="TrayEntryFlags.Button"/>, <see cref="TrayEntryFlags.Checkbox"/>, and <see cref="TrayEntryFlags.Submenu"/> flags should be set, and the <see cref="TrayEntryFlags.Checked"/> flag should only be set if the <see cref="TrayEntryFlags.Checkbox"/> flag is set.
	/// </para>
	/// <para>
	/// This method is primarily intended to specify <paramref name="flags"/> combinations that are not covered by the predefined derived tray entry types.
	/// If you just want to insert a regular button, checkbox, submenu, or separator entry, you can just create a new <see cref="ButtonTrayEntry"/>, <see cref="CheckboxTrayEntry"/>, <see cref="SubmenuTrayEntry"/>, or <see cref="SeparatorTrayEntry"/> instance, respectively, and insert it using the <see cref="Insert(int, TrayEntry)"/> method.
	/// </para>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> is less than <c>-1</c>, or greater than the <see cref="Count">number of entries</see> in the tray menu</exception>"
	public TrayEntry? InsertEntry(int position, string? label, TrayEntryFlags flags)
	{
		unsafe
		{
			if (position is < -1)
			{
				failPositionArgumentOutOfRange();
			}
			else if (mEntries is null)
			{
				if (position is > 0)
				{
					failPositionArgumentOutOfRange();
				}

				mEntries = [];
			}
			else if (position > mEntries.Count)
			{
				failPositionArgumentOutOfRange();
			}

			TrayEntry entry;
			if (mMenu is null)
			{
				entry = TrayEntry.Create(label, flags, parent: this);
			}
			else
			{
				TrayEntry.SDL_TrayEntry* entryPtr;
				var labelUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(label);
				try
				{
					entryPtr = SDL_InsertTrayEntryAt(mMenu, position, labelUtf8, flags);
				}
				finally
				{
					Utf8StringMarshaller.Free(labelUtf8);
				}

				if (entryPtr is null)
				{
					return null;
				}

				entry = TrayEntry.Create(label, flags, parent: this);

				entry.Adopt(entryPtr);
			}

			if (position is -1)
			{
				mEntries.Add(entry);
			}
			else
			{
				mEntries.Insert(position, entry);
			}

			return entry;
		}

		[DoesNotReturn]
		static void failPositionArgumentOutOfRange() => throw new ArgumentOutOfRangeException(nameof(position), $"{nameof(position)} must be greater than or equal to -1, and less than or equal to the number of entries in the tray menu");
	}

	/// <summary>
	/// Gets the tray entry at a specified position in the tray menu
	/// </summary>
	/// <param name="position">The zero-based position of the tray entry to get in the tray menu</param>
	/// <returns>The tray entry at the specified <paramref name="position"/> in the tray menu</returns>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> is less than <c>0</c>, or greater than or equal to the number of tray entries in the tray menu</exception>
	/// <exception cref="SdlException">The tray entry was located at the specified <paramref name="position"/>, but it could not be retrieved</exception>
	public TrayEntry this[int position]
	{
		get
		{
			unsafe
			{
				if (mMenu is null)
				{
					if (mEntries is not null && position >= 0 && position < mEntries.Count)
					{
						return mEntries[position];
					}

					failPositionArgumentOutOfRange();
				}

				if (position is < 0)
				{
					failPositionArgumentOutOfRange();
				}

				Unsafe.SkipInit(out int count);

				var entries = SDL_GetTrayEntries(mMenu, &count);

				if (entries is null || position >= count)
				{
					failPositionArgumentOutOfRange();
				}

				if (!TrayEntry.TryGetOrCreate(entries[position], out var entry))
				{
					failCouldNotGetEntry();
				}

				return entry;
			}

			[DoesNotReturn]
			static void failPositionArgumentOutOfRange() => throw new ArgumentOutOfRangeException(nameof(position));

			[DoesNotReturn]
			static void failCouldNotGetEntry() => throw new SdlException($"Could not get the {nameof(TrayEntry)} at the given {nameof(position)}");
		}
	}

	private unsafe void Register([NotNull] SDL_TrayMenu* menuPtr)
	{
		mKnownInstances.AddOrUpdate(unchecked((IntPtr)menuPtr), addRef, updateRef, this);

		static WeakReference<TrayMenu> addRef(IntPtr menu, TrayMenu newMenu) => new(newMenu);

		static WeakReference<TrayMenu> updateRef(IntPtr menu, WeakReference<TrayMenu> previousMenuRef, TrayMenu newMenu)
		{
			if (previousMenuRef.TryGetTarget(out var previousMenu))
			{
				// Nothing to do here, just silently forget about the old menu
			}

			previousMenuRef.SetTarget(newMenu);

			return previousMenuRef;
		}
	}

	/// <summary>
	/// Removes a specific tray entry from the tray menu
	/// </summary>
	/// <param name="entry">The tray entry to remove from the tray menu</param>
	/// <returns><c><see langword="true"/></c>, if <paramref name="entry"/> was successfully removed from the tray menu; otherwise, <c><see langword="false"/></c></returns>
	/// <remarks>
	/// <para>
	/// This method will return <c><see langword="false"/></c> and do nothing if the specified <paramref name="entry"/> is not found in the tray menu.
	/// </para>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public bool Remove(TrayEntry entry)
	{
		unsafe
		{
			if (mMenu is null)
			{
				if (mEntries?.Remove(entry) is true)
				{
					entry.AdoptBack(parent: null);

					return true;
				}
			}
			else
			{
				var entries = SDL_GetTrayEntries(mMenu, null);

				if (entries is null)
				{
					return false;
				}

				while (*entries++ is var entryPtr && entryPtr is not null)
				{
					if (entryPtr == entry.Pointer)
					{
						mEntries?.Remove(entry);
						entry.AdoptBack(parent: null);

						return true;
					}
				}
			}

			return false;
		}
	}

	internal unsafe static bool TryGet(SDL_TrayMenu* menu, [NotNullWhen(true)] out TrayMenu? result)
	{
		if (menu is null
			|| !mKnownInstances.TryGetValue(unchecked((IntPtr)menu), out var menuRef)
			|| !menuRef.TryGetTarget(out result))
		{
			result = null;
			return false;
		}

		return true;
	}

	internal unsafe static bool TryGetOrCreate(SDL_TrayMenu* menu, [NotNullWhen(true)] out TrayMenu? result)
	{
		if (menu is null)
		{
			result = null;
			return false;
		}

		var menuRef = mKnownInstances.GetOrAdd(unchecked((IntPtr)menu), createRef);

		if (!menuRef.TryGetTarget(out result))
		{
			menuRef.SetTarget(result = create(menu));
		}

		return true;

		static WeakReference<TrayMenu> createRef(IntPtr menu) => new(create(unchecked((SDL_TrayMenu*)menu)));

		static TrayMenu create(SDL_TrayMenu* menu) => new(menu); // this constructor does not register
	}
}
