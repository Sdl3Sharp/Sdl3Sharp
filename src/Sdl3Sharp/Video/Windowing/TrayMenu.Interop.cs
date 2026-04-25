using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Video.Windowing;

partial class TrayMenu
{
	// opaque struct
	[StructLayout(LayoutKind.Sequential, Size = 0)]
	internal readonly struct SDL_TrayMenu;

	/// <summary>
	/// Returns a list of entries in the menu, in order
	/// </summary>
	/// <param name="menu">The menu to get entries from</param>
	/// <param name="count">An optional pointer to obtain the number of entries in the menu</param>
	/// <returns>
	///		Returns a NULL-terminated list of entries within the given menu.
	///		The pointer becomes invalid when any function that inserts or deletes entries in the menu is called.
	///	</returns>
	///	<remarks>
	///	<para>
	///	This function should be called on the thread that created the tray.
	///	</para>
	///	</remarks>
	///	<seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTrayEntries">SDL_GetTrayEntries</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial TrayEntry.SDL_TrayEntry** SDL_GetTrayEntries(SDL_TrayMenu* menu, int* count);

	/// <summary>
	/// Gets the entry for which the menu is a submenu, if the current menu is a submenu
	/// </summary>
	/// <param name="menu">The menu for which to get the parent entry</param>
	/// <returns>Returns the parent entry, or NULL if this menu is not a submenu</returns>
	/// <remarks>
	/// <para>
	/// Either this function or <see href="https://wiki.libsdl.org/SDL3/SDL_GetTrayMenuParentTray">SDL_GetTrayMenuParentTray</see>() will return non-NULL for any given menu.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTrayMenuParentEntry">SDL_GetTrayMenuParentEntry</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial TrayEntry.SDL_TrayEntry* SDL_GetTrayMenuParentEntry(SDL_TrayMenu* menu);

	/// <summary>
	/// Gets the tray for which this menu is the first-level menu, if the current menu isn't a submenu
	/// </summary>
	/// <param name="menu">The menu for which to get the parent tray</param>
	/// <returns>Returns the parent tray, or NULL if this menu is a submenu</returns>
	/// <remarks>
	/// <para>
	/// Either this function or <see href="https://wiki.libsdl.org/SDL3/SDL_GetTrayMenuParentEntry">SDL_GetTrayMenuParentEntry</see>() will return non-NULL for any given menu.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTrayMenuParentTray">SDL_GetTrayMenuParentTray</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial Tray.SDL_Tray* SDL_GetTrayMenuParentTray(SDL_TrayMenu* menu);

	/// <summary>
	/// Insert a tray entry at a given position
	/// </summary>
	/// <param name="menu">The menu to append the entry to</param>
	/// <param name="pos">The desired position for the new entry. Entries at or following this place will be moved. If pos is -1, the entry is appended.</param>
	/// <param name="label">The text to be displayed on the entry, in UTF-8 encoding, or NULL for a separator.</param>
	/// <param name="flags">A combination of flags, some of which are mandatory</param>
	/// <returns>Returns the newly created entry, or NULL if pos is out of bounds</returns>
	/// <remarks>
	/// <para>
	/// If label is NULL, the entry will be a separator. Many functions won't work for an entry that is a separator.
	/// </para>
	/// <para>
	/// An entry does not need to be destroyed; it will be destroyed with the tray.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_InsertTrayEntryAt">SDL_InsertTrayEntryAt</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial TrayEntry.SDL_TrayEntry* SDL_InsertTrayEntryAt(SDL_TrayMenu* menu, int pos, byte* label, TrayEntryFlags flags);
}
