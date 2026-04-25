using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using unsafe SDL_TrayCallback = delegate* unmanaged[Cdecl]<void*, Sdl3Sharp.Video.Windowing.TrayEntry.SDL_TrayEntry*, void>;

namespace Sdl3Sharp.Video.Windowing;

partial class TrayEntry
{
	// opaque struct
	[StructLayout(LayoutKind.Sequential, Size = 0)]
	internal readonly struct SDL_TrayEntry;

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private unsafe static void TrayCallback(void* userdata, SDL_TrayEntry* entry)
	{
		if (userdata is not null && GCHandle.FromIntPtr(unchecked((IntPtr)userdata)) is { IsAllocated: true, Target: TrayEntry { mEntry: var entryPtr } managedEntry } && entryPtr == entry)
		{
			managedEntry.mFlags = (managedEntry.mFlags & ~TrayEntryFlags.Checked) | (SDL_GetTrayEntryChecked(entry) ? TrayEntryFlags.Checked : 0);
			managedEntry.OnSelected();
		}
	}

	/// <summary>
	/// Simulates a click on a tray entry
	/// </summary>
	/// <param name="entry">The entry to activate</param>
	/// <remarks>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_ClickTrayEntry">SDL_ClickTrayEntry</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_ClickTrayEntry(SDL_TrayEntry* entry);

	/// <summary>
	/// Creates a submenu for a system tray entry
	/// </summary>
	/// <param name="entry">The tray entry to bind the menu to</param>
	/// <returns>Returns the newly created menu</returns>
	/// <remarks>
	/// <para>
	/// This should be called at most once per tray entry.
	/// </para>
	/// <para>
	/// This function does the same thing as <see href="https://wiki.libsdl.org/SDL3/SDL_CreateTrayMenu">SDL_CreateTrayMenu</see>,
	/// except that it takes a <see href="https://wiki.libsdl.org/SDL3/SDL_TrayEntry">SDL_TrayEntry</see> instead of a <see href="https://wiki.libsdl.org/SDL3/SDL_Tray">SDL_Tray</see>.
	/// </para>
	/// <para>
	/// A menu does not need to be destroyed; it will be destroyed with the tray.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateTraySubmenu">SDL_CreateTraySubmenu</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial TrayMenu.SDL_TrayMenu* SDL_CreateTraySubmenu(SDL_TrayEntry* entry);

	/// <summary>
	/// Gets whether or not an entry is checked
	/// </summary>
	/// <param name="entry">The entry to be read</param>
	/// <returns>Returns true if the entry is checked; false otherwise</returns>
	/// <remarks>
	/// <para>
	/// The entry must have been created with the <see href="https://wiki.libsdl.org/SDL3/SDL_TRAYENTRY_CHECKBOX">SDL_TRAYENTRY_CHECKBOX</see> flag.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTrayEntryChecked">SDL_GetTrayEntryChecked</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetTrayEntryChecked(SDL_TrayEntry* entry);

	/// <summary>
	/// Gets whether or not an entry is enabled
	/// </summary>
	/// <param name="entry">The entry to be read</param>
	/// <returns>Returns true if the entry is enabled; false otherwise</returns>
	/// <remarks>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTrayEntryEnabled">SDL_GetTrayEntryEnabled</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetTrayEntryEnabled(SDL_TrayEntry* entry);

	/// <summary>
	/// Gets the label of an entry
	/// </summary>
	/// <param name="entry">The entry to be read</param>
	/// <returns>Returns the label of the entry in UTF-8 encoding</returns>
	/// <remarks>
	/// <para>
	/// If the returned value is NULL, the entry is a separator.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTrayEntryLabel">SDL_GetTrayEntryLabel</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial byte* SDL_GetTrayEntryLabel(SDL_TrayEntry* entry);

	/// <summary>
	/// Gets the menu containing a certain tray entry
	/// </summary>
	/// <param name="entry">The entry for which to get the parent menu</param>
	/// <returns>Returns the parent menu</returns>
	/// <remarks>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTrayEntryParent">SDL_GetTrayEntryParent</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial TrayMenu.SDL_TrayMenu* SDL_GetTrayEntryParent(SDL_TrayEntry* entry);

	/// <summary>
	/// Gets a previously created tray entry submenu
	/// </summary>
	/// <param name="entry">The tray entry to bind the menu to</param>
	/// <returns>Returns the newly created menu</returns>
	/// <remarks>
	/// <para>
	/// You should have called <see href="https://wiki.libsdl.org/SDL3/SDL_CreateTraySubmenu">SDL_CreateTraySubmenu</see>() on the entry object.
	/// This function allows you to fetch it again later.
	/// </para>
	/// <para>
	/// This function does the same thing as <see href="https://wiki.libsdl.org/SDL3/SDL_GetTrayMenu">SDL_GetTrayMenu</see>(),
	/// except that it takes a <see href="https://wiki.libsdl.org/SDL3/SDL_TrayEntry">SDL_TrayEntry</see> instead of a <see href="https://wiki.libsdl.org/SDL3/SDL_Tray">SDL_Tray</see>.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTraySubmenu">SDL_GetTraySubmenu</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial TrayMenu.SDL_TrayMenu* SDL_GetTraySubmenu(SDL_TrayEntry* entry);

	/// <summary>
	/// Removes a tray entry
	/// </summary>
	/// <param name="entry">The entry to be deleted</param>
	/// <remarks>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_RemoveTrayEntry">SDL_RemoveTrayEntry</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_RemoveTrayEntry(SDL_TrayEntry* entry);

	/// <summary>
	/// Sets a callback to be invoked when the entry is selected
	/// </summary>
	/// <param name="entry">The entry to be updated</param>
	/// <param name="callback">A callback to be invoked when the entry is selected</param>
	/// <param name="userdata">An optional pointer to pass extra data to the callback when it will be invoked</param>
	/// <remarks>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetTrayEntryCallback">SDL_SetTrayEntryCallback</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_SetTrayEntryCallback(SDL_TrayEntry* entry, SDL_TrayCallback callback, void* userdata);

	/// <summary>
	/// Sets whether or not an entry is checked
	/// </summary>
	/// <param name="entry">The entry to be updated</param>
	/// <param name="checked">true if the entry should be checked; false otherwise</param>
	/// <remarks>
	/// <para>
	/// The entry must have been created with the <see href="https://wiki.libsdl.org/SDL3/SDL_TRAYENTRY_CHECKBOX">SDL_TRAYENTRY_CHECKBOX</see> flag.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetTrayEntryChecked">SDL_SetTrayEntryChecked</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_SetTrayEntryChecked(SDL_TrayEntry* entry, CBool @checked);

	/// <summary>
	/// Sets whether or not an entry is enabled
	/// </summary>
	/// <param name="entry">The entry to be updated</param>
	/// <param name="enabled">true if the entry should be enabled; false otherwise</param>
	/// <remarks>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetTrayEntryChecked">SDL_SetTrayEntryChecked</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_SetTrayEntryEnabled(SDL_TrayEntry* entry, CBool enabled);

	/// <summary>
	/// Sets the label of an entry
	/// </summary>
	/// <param name="entry">The entry to be updated</param>
	/// <param name="label">The new label for the entry in UTF-8 encoding</param>
	/// <remarks>
	/// <para>
	/// An entry cannot change between a separator and an ordinary entry;
	/// that is, it is not possible to set a non-NULL label on an entry that has a NULL label (separators), or to set a NULL label to an entry that has a non-NULL label.
	/// The function will silently fail if that happens.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetTrayEntryLabel">SDL_SetTrayEntryLabel</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_SetTrayEntryLabel(SDL_TrayEntry* entry, byte* label);
}
