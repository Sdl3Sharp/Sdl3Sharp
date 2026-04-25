using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents a generic tray entry
/// </summary>
/// <remarks>
/// <para>
/// If you just want to a more specific type of tray entry, you should use one of the predefined tray entry types, <see cref="ButtonTrayEntry"/>, <see cref="CheckboxTrayEntry"/>, <see cref="SubmenuTrayEntry"/>, or <see cref="SeparatorTrayEntry"/>, instead.
/// </para>
/// </remarks>
public partial class TrayEntry : ITrayEntry
{
	private static readonly ConcurrentDictionary<IntPtr, WeakReference<TrayEntry>> mKnownInstances = [];

	private unsafe SDL_TrayEntry* mEntry;
	private GCHandle mSelfHandle;
	private string? mLabel;
	private TrayEntryFlags mFlags;
	private TrayMenu? mMenu, mParent;

	/// <summary>
	/// Creates a new <see cref="TrayEntry"/> with the specified label and flags
	/// </summary>
	/// <param name="label">The label of the tray entry, or <c><see langword="null"/></c>, if the entry should behave like a separator entry</param>
	/// <param name="flags">The flags specifying the behavior of the tray entry</param>
	/// <remarks>
	/// <para>
	/// Note that some combinations of <paramref name="flags"/> are not valid and may lead to undefined behavior.
	/// Only one of the <see cref="TrayEntryFlags.Button"/>, <see cref="TrayEntryFlags.Checkbox"/>, and <see cref="TrayEntryFlags.Submenu"/> flags should be set, and the <see cref="TrayEntryFlags.Checked"/> flag should only be set if the <see cref="TrayEntryFlags.Checkbox"/> flag is set.
	/// This constructor does not perform validation of the provided <paramref name="flags"/>, and issues (including exceptions) may occur later if the resulting tray entry is used.
	/// </para>
	/// <para>
	/// This constructor is primarily intended to specify <paramref name="flags"/> combinations that are not covered by the predefined derived tray entry types and as a base constructor for those derived types.
	/// If you just want to create a regular button, checkbox, submenu, or separator entry, you can just create a <see cref="ButtonTrayEntry"/>, <see cref="CheckboxTrayEntry"/>, <see cref="SubmenuTrayEntry"/>, or <see cref="SeparatorTrayEntry"/> instance, respectively, instead.
	/// </para>
	/// </remarks>
	public TrayEntry(string? label, TrayEntryFlags flags)
	{
		unsafe
		{
			mEntry = null;
			mSelfHandle = default;
			mLabel = label;
			mFlags = flags;
			mMenu = null;
			mParent = null;

			// we don't need to update the instance registry here, as the menu is not associated with a native menu yet
		}
	}

	private protected unsafe TrayEntry(SDL_TrayEntry* entry)
	{
		Adopt(entry); // updating the instance registry is handled by the Adopt method, so that should be everything we need to do here
	}

	internal TrayEntryFlags Flags { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mFlags; }

	/// <inheritdoc cref="ITrayEntry.IsChecked"/>
	protected bool IsChecked
	{
		get
		{
			unsafe
			{
				if (mEntry is null)
				{
					return (mFlags & (TrayEntryFlags.Checkbox | TrayEntryFlags.Checked)) is (TrayEntryFlags.Checkbox | TrayEntryFlags.Checked);
				}

				var result = SDL_GetTrayEntryChecked(mEntry);

				mFlags = (mFlags & ~TrayEntryFlags.Checked) | (result ? TrayEntryFlags.Checked : 0);

				return result;
			}
		}

		set
		{
			unsafe
			{
				if ((mFlags & TrayEntryFlags.Checkbox) is not 0)
				{
					mFlags = (mFlags & ~TrayEntryFlags.Checked) | (value ? TrayEntryFlags.Checked : 0);
				}

				if (mEntry is not null)
				{
					SDL_SetTrayEntryChecked(mEntry, value);
				}
			}
		}
	}

	/// <inheritdoc/>
	bool ITrayEntry.IsChecked
	{
		get => IsChecked;
		set => IsChecked = value;
	}

	/// <inheritdoc cref="ITrayEntry.IsEnabled"/>
	protected bool IsEnabled
	{
		get
		{
			unsafe
			{
				if (mEntry is null)
				{
					return (mFlags & TrayEntryFlags.Disabled) is 0;
				}

				var result = SDL_GetTrayEntryEnabled(mEntry);

				mFlags = (mFlags & ~TrayEntryFlags.Disabled) | (result ? 0 : TrayEntryFlags.Disabled);

				return result;
			}
		}

		set
		{
			unsafe
			{
				mFlags = (mFlags & ~TrayEntryFlags.Disabled) | (value ? 0 : TrayEntryFlags.Disabled);

				if (mEntry is not null)
				{
					SDL_SetTrayEntryEnabled(mEntry, value);
				}
			}
		}
	}

	/// <inheritdoc/>
	bool ITrayEntry.IsEnabled
	{
		get => IsEnabled;
		set => IsEnabled = value;
	}

	/// <inheritdoc cref="ITrayEntry.Label"/>
	protected internal string? Label
	{
		get
		{
			unsafe
			{
				if (mEntry is null)
				{
					return mLabel;
				}

				return Utf8StringMarshaller.ConvertToManaged(SDL_GetTrayEntryLabel(mEntry));
			}
		}

		set
		{
			unsafe
			{
				if (mEntry is null)
				{
					mLabel = value;
				}
				else
				{
					var valueUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(value);
					try
					{
						SDL_SetTrayEntryLabel(mEntry, valueUtf8);
					}
					finally
					{
						Utf8StringMarshaller.Free(valueUtf8);
					}
				}
			}
		}
	}

	/// <inheritdoc/>
	string? ITrayEntry.Label
	{
		get => Label;
		set => Label = value;
	}

	/// <summary>
	/// Gets the submenu associated with the tray entry
	/// </summary>
	/// <value>
	/// The submenu associated with the tray entry, or <c><see langword="null"/></c> if the submenu has not yet been created
	/// </value>
	/// <remarks>
	/// <para>
	/// You can use the <see cref="TryCreateMenu"/> method to create the submenu if it does not exist yet.
	/// Afterwards, the value of the property will be the created submenu.
	/// </para>
	/// <para>
	/// Tray entries other than <see cref="SubmenuTrayEntry">submenu entries</see> may always return <c><see langword="null"/></c> for this property.
	/// </para>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	protected TrayMenu? Menu
	{
		get
		{
			unsafe
			{
				if (mEntry is null)
				{
					return (mFlags & TrayEntryFlags.Submenu) is not 0
						? mMenu
						: null;
				}

				TrayMenu.TryGetOrCreate(SDL_GetTraySubmenu(mEntry), out mMenu);

				return mMenu;
			}
		}
	}

	/// <inheritdoc/>
	TrayMenu? ITrayEntry.Menu => Menu;

	/// <inheritdoc/>
	public TrayMenu? Parent
	{
		get
		{
			unsafe
			{
				if (mEntry is null)
				{
					return mParent;
				}

				TrayMenu.TryGetOrCreate(SDL_GetTrayEntryParent(mEntry), out var result);

				return result;
			}
		}
	}

	internal unsafe SDL_TrayEntry* Pointer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mEntry; }

	/// <inheritdoc cref="ITrayEntry.Selected"/>
	protected event TrayEntrySelectedEventHandler<TrayEntry>? Selected;

	/// <inheritdoc/>
	event TrayEntrySelectedEventHandler<ITrayEntry>? ITrayEntry.Selected
	{
		add => Selected += value;
		remove => Selected -= value;
	}

	internal unsafe void Adopt([NotNull] SDL_TrayEntry* entry)
	{
		if (mEntry is not null)
		{
			// very exceptional: if we did everything right, this should never happen
			failEntryAlreadyAssociated();
		}

		mEntry = entry;

		if (!mSelfHandle.IsAllocated || !ReferenceEquals(this, mSelfHandle.Target))
		{
			mSelfHandle = GCHandle.Alloc(this, GCHandleType.Normal);
		}

		SDL_SetTrayEntryCallback(mEntry, &TrayCallback, unchecked((void*)GCHandle.ToIntPtr(mSelfHandle)));

		mLabel = null;

		if (mMenu is not null)
		{
			var menu = SDL_GetTraySubmenu(mEntry);

			if (menu is null)
			{
				menu = SDL_CreateTraySubmenu(mEntry);

				if (menu is null)
				{
					failCouldNotCreateMenu();
				}
			}

			mMenu.Adopt(menu);

			// we keep mMenu around even after adopting the native entry, so that we still keep a strong ownership reference to the submenu
		}

		mParent = null;

		Update(entry);

		[DoesNotReturn]
		static void failEntryAlreadyAssociated() => throw new InvalidOperationException($"The {nameof(TrayEntry)} is already associated with an {nameof(SDL_TrayEntry)}* and cannot be associated with another one");

		[DoesNotReturn]
		static void failCouldNotCreateMenu() => throw new SdlException($"Could not create a native tray menu for an existing managed {nameof(TrayMenu)}");
	}

	internal void AdoptBack(TrayMenu? parent)
	{
		unsafe
		{
			mParent = parent;

			if (mEntry is null)
			{
				return;
			}

			// for safety reasons, we just recreate the managed submenu entirely base on the native submenu

			if (TrayMenu.TryGetOrCreate(SDL_GetTraySubmenu(mEntry), out var menu))
			{
				menu.AdoptBack(parent: this);
				mMenu = menu;
			}
			else
			{
				mMenu = null;
			}

			mLabel = Utf8StringMarshaller.ConvertToManaged(SDL_GetTrayEntryLabel(mEntry));

			SDL_SetTrayEntryCallback(mEntry, null, null);

			if (mSelfHandle.IsAllocated)
			{
				mSelfHandle.Free();
				mSelfHandle = default;
			}

			SDL_RemoveTrayEntry(mEntry);

			mKnownInstances.TryRemove(unchecked((IntPtr)mEntry), out _);

			mEntry = null;
		}
	}

	internal static TrayEntry Create(string? label, TrayEntryFlags flags, TrayMenu parent)
	{
		if (label is null)
		{
			return new SeparatorTrayEntry() { mFlags = flags, mParent = parent };
		}
		else if ((flags & TrayEntryFlags.Button) is not 0 && (flags & (TrayEntryFlags.Checkbox | TrayEntryFlags.Submenu)) is 0)
		{
			return new ButtonTrayEntry(label, isEnabled: (flags & TrayEntryFlags.Disabled) is 0) { mFlags = flags, mParent = parent };
		}
		else if ((flags & TrayEntryFlags.Checkbox) is not 0 && (flags & TrayEntryFlags.Submenu) is 0)
		{
			return new CheckboxTrayEntry(label, isChecked: (flags & TrayEntryFlags.Checked) is not 0, isEnabled: (flags & TrayEntryFlags.Disabled) is 0) { mFlags = flags, mParent = parent };
		}
		else if ((flags & TrayEntryFlags.Submenu) is not 0)
		{
			return new SubmenuTrayEntry(label, isEnabled: (flags & TrayEntryFlags.Disabled) is 0) { mFlags = flags, mParent = parent };
		}
		else
		{
			return new TrayEntry(label, flags) { mParent = parent };
		}
	}

	/// <inheritdoc cref="ITrayEntry.Click"/>
	protected void Click()
	{
		unsafe
		{
			if (mEntry is null)
			{
				OnSelected();

				return;
			}

			SDL_ClickTrayEntry(mEntry);
		}
	}

	/// <inheritdoc/>
	void ITrayEntry.Click() => Click();

	internal void DisposeInternal()
	{
		unsafe
		{
			Selected = null;
			mParent = null;

			if (mEntry is null)
			{
				mMenu?.DisposeInternal();
			}
			else
			{
				if (TrayMenu.TryGet(SDL_GetTraySubmenu(mEntry), out var menu))
				{
					menu.DisposeInternal();
				}
			}

			mMenu = null;
			mLabel = null;

			if (mSelfHandle.IsAllocated)
			{
				mSelfHandle.Free();
				mSelfHandle = default;
			}

			if (mEntry is not null)
			{
				mKnownInstances.TryRemove(unchecked((IntPtr)mEntry), out _);
			}

			mEntry = null;
		}
	}

	/// <summary>
	/// Raises the <see cref="Selected"/> event
	/// </summary>
	protected virtual void OnSelected() => Selected?.Invoke(this);

	/// <summary>
	/// Creates the submenu associated with the tray entry, if it does not exist yet
	/// </summary>
	/// <param name="menu">The created submenu, if this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns>
	/// <c><see langword="true"/></c>, if the submenu was successfully created; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This method may return <c><see langword="false"/></c>, if the tray entry does not support submenus. Submenus are always supported for <see cref="SubmenuTrayEntry"/> entries.
	/// </para>
	/// <para>
	/// This method may return <c><see langword="true"/></c>, if the entry already has a submenu associated with it. In this case, the <paramref name="menu"/> parameter will be set to the existing submenu.
	/// </para>
	/// <para>
	/// After this method returns <c><see langword="true"/></c>, the value of the <see cref="Menu"/> property will reflect the created submenu.
	/// </para>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	protected bool TryCreateMenu([NotNullWhen(true)] out TrayMenu? menu)
	{
		unsafe
		{
			if (mMenu is null)
			{
				if (mEntry is null)
				{
					if ((mFlags & TrayEntryFlags.Submenu) is not 0)
					{
						mMenu = new(parent: this);
					}
				}
				else
				{
					TrayMenu.TryGetOrCreate(SDL_CreateTraySubmenu(mEntry), out mMenu);
				}
			}

			return (menu = mMenu) is not null;
		}
	}

	internal unsafe static bool TryGet(SDL_TrayEntry* entry, [NotNullWhen(true)] out TrayEntry? result)
	{
		if (entry is null
			|| !mKnownInstances.TryGetValue((IntPtr)entry, out var entryRef)
			|| !entryRef.TryGetTarget(out result))
		{
			result = null;
			return false;
		}

		return true;
	}

	internal unsafe static bool TryGetOrCreate(SDL_TrayEntry* entry, [NotNullWhen(true)] out TrayEntry? result)
	{
		if (entry is null)
		{
			result = null;
			return false;
		}

		var entryRef = mKnownInstances.GetOrAdd(unchecked((IntPtr)entry), createRef);

		if (!entryRef.TryGetTarget(out result))
		{
			entryRef.SetTarget(result = create(entry));
		}

		return true;

		static WeakReference<TrayEntry> createRef(IntPtr entry) => new(create(unchecked((SDL_TrayEntry*)entry)));

		static TrayEntry create(SDL_TrayEntry* entry) => new(entry); // as a fallback, we create a generic TrayEntry
	}

	private unsafe void Update([NotNull] SDL_TrayEntry* entry)
	{
		mKnownInstances.AddOrUpdate(unchecked((IntPtr)entry), addRef, createRef, this);

		static WeakReference<TrayEntry> addRef(IntPtr entry, TrayEntry newEntry) => new(newEntry);

		static WeakReference<TrayEntry> createRef(IntPtr entry, WeakReference<TrayEntry> previousEntryRef, TrayEntry newEntry)
		{
			if (previousEntryRef.TryGetTarget(out var previousEntry))
			{
				// Nothing to do here, just silently forget about the old entry
			}

			previousEntryRef.SetTarget(newEntry);

			return previousEntryRef;
		}
	}
}
