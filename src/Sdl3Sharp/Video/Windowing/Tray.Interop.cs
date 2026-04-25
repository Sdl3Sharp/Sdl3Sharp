using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Video.Windowing;

partial class Tray
{
	// opaque struct
	[StructLayout(LayoutKind.Sequential, Size = 0)]
	internal readonly struct SDL_Tray;

#if SDL3_6_0_OR_GREATER

	[ThreadStatic] private static TrayClickedEventArgs? mTrayClickedEventArgs;

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private unsafe static CBool TrayLeftClickCallback(void* userdata, SDL_Tray* tray)
	{
		if (userdata is not null && GCHandle.FromIntPtr(unchecked((IntPtr)userdata)) is { IsAllocated: true, Target: Tray { mTray: var trayPtr } managedTray } && trayPtr == tray)
		{
			mTrayClickedEventArgs ??= new();

			if (managedTray.mLeftClickCallback is var leftClickCallback && leftClickCallback is not null)
			{
				mTrayClickedEventArgs.ShowMenu = leftClickCallback(managedTray.mUserdata, tray);
			}
			else
			{
				mTrayClickedEventArgs.ShowMenu = true;
			}

			managedTray.OnLeftClicked(mTrayClickedEventArgs);

			return mTrayClickedEventArgs.ShowMenu;
		}

		return true; // Left-click defaults to showing the menu
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private unsafe static CBool TrayRightClickCallback(void* userdata, SDL_Tray* tray)
	{
		if (userdata is not null && GCHandle.FromIntPtr(unchecked((IntPtr)userdata)) is { IsAllocated: true, Target: Tray { mTray: var trayPtr } managedTray } && trayPtr == tray)
		{
			mTrayClickedEventArgs ??= new();

			if (managedTray.mRightClickCallback is var rightClickCallback && rightClickCallback is not null)
			{
				mTrayClickedEventArgs.ShowMenu = rightClickCallback(managedTray.mUserdata, tray);
			}
			else
			{
				mTrayClickedEventArgs.ShowMenu = true;
			}

			managedTray.OnRightClicked(mTrayClickedEventArgs);

			return mTrayClickedEventArgs.ShowMenu;
		}
		return true; // Right-click defaults to showing the menu
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private unsafe static CBool TrayMiddleClickCallback(void* userdata, SDL_Tray* tray)
	{
		if (userdata is not null && GCHandle.FromIntPtr(unchecked((IntPtr)userdata)) is { IsAllocated: true, Target: Tray { mTray: var trayPtr } managedTray } && trayPtr == tray)
		{
			mTrayClickedEventArgs ??= new();

			if (managedTray.mMiddleClickCallback is var middleClickCallback && middleClickCallback is not null)
			{
				mTrayClickedEventArgs.ShowMenu = middleClickCallback(managedTray.mUserdata, tray);
			}
			else
			{
				mTrayClickedEventArgs.ShowMenu = false;
			}

			managedTray.OnMiddleClicked(mTrayClickedEventArgs);

			return mTrayClickedEventArgs.ShowMenu;
		}

		return false; // Middle-click defaults to not showing the menu, when not set
	}

#endif

	/// <summary>
	/// Creates an icon to be placed in the operating system's tray, or equivalent
	/// </summary>
	/// <param name="icon">A surface to be used as icon. May be NULL.</param>
	/// <param name="tooltip">A tooltip to be displayed when the mouse hovers the icon in UTF-8 encoding. Not supported on all platforms. May be NULL.</param>
	/// <returns>Returns the newly created system tray icon</returns>
	/// <remarks>
	/// <para>
	/// Many platforms advise not using a system tray unless persistence is a necessary feature.
	/// Avoid needlessly creating a tray icon, as the user may feel like it clutters their interface.
	/// </para>
	/// <para>
	/// Using tray icons require the video subsystem.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateTray">SDL_CreateTray</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Tray* SDL_CreateTray(Surface.SDL_Surface* icon, byte* tooltip);

	/// <summary>
	/// Creates a menu for a system tray
	/// </summary>
	/// <param name="tray">The tray to bind the menu to</param>
	/// <returns>Returns the newly created menu</returns>
	/// <remarks>
	/// <para>
	/// This should be called at most once per tray icon.
	/// </para>
	/// <para>
	/// This function does the same thing as <see href="https://wiki.libsdl.org/SDL3/SDL_CreateTraySubmenu">SDL_CreateTraySubmenu</see>(),
	/// except that it takes a <see href="https://wiki.libsdl.org/SDL3/SDL_Tray">SDL_Tray</see> instead of a <see href="https://wiki.libsdl.org/SDL3/SDL_TrayEntry">SDL_TrayEntry</see>.
	/// </para>
	/// <para>
	/// A menu does not need to be destroyed; it will be destroyed with the tray.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateTrayMenu">SDL_CreateTrayMenu</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial TrayMenu.SDL_TrayMenu* SDL_CreateTrayMenu(SDL_Tray* tray);

#if SDL3_6_0_OR_GREATER

	/// <summary>
	/// Create an icon to be placed in the operating system's tray, or equivalent
	/// </summary>
	/// <param name="props">The properties to use</param>
	/// <returns>Returns the newly created system tray icon</returns>
	/// <remarks>
	/// <para>
	/// Many platforms advise not using a system tray unless persistence is a necessary feature.
	/// Avoid needlessly creating a tray icon, as the user may feel like it clutters their interface.
	/// </para>
	/// <para>
	/// Using tray icons require the video subsystem.
	/// </para>
	/// <para>
	/// These are the supported properties:
	/// <list type="bullet">
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_TRAY_CREATE_ICON_POINTER"><c>SDL_PROP_TRAY_CREATE_ICON_POINTER</c></see></term>
	///			<description>An <see href="https://wiki.libsdl.org/SDL3/SDL_Surface">SDL_Surface</see> to be used as the tray icon. May be NULL.</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_TRAY_CREATE_TOOLTIP_STRING"><c>SDL_PROP_TRAY_CREATE_TOOLTIP_STRING</c></see></term>
	///			<description>A tooltip to be displayed when the mouse hovers the icon in UTF-8 encoding. Not supported on all platforms. May be NULL.</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_TRAY_CREATE_USERDATA_POINTER"><c>SDL_PROP_TRAY_CREATE_USERDATA_POINTER</c></see></term>
	///			<description>An optional pointer to associate with the tray, which will be passed to click callbacks. May be NULL.</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_TRAY_CREATE_LEFTCLICK_CALLBACK_POINTER"><c>SDL_PROP_TRAY_CREATE_LEFTCLICK_CALLBACK_POINTER</c></see></term>
	///			<description>An <see href="https://wiki.libsdl.org/SDL3/SDL_TrayClickCallback">SDL_TrayClickCallback</see> to be invoked when the tray icon is left-clicked. Not supported on all platforms. The callback should return true to show the default menu, or false to skip showing it. May be NULL.</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_TRAY_CREATE_RIGHTCLICK_CALLBACK_POINTER"><c>SDL_PROP_TRAY_CREATE_RIGHTCLICK_CALLBACK_POINTER</c></see></term>
	///			<description>An <see href="https://wiki.libsdl.org/SDL3/SDL_TrayClickCallback">SDL_TrayClickCallback</see> to be invoked when the tray icon is right-clicked. Not supported on all platforms. The callback should return true to show the default menu, or false to skip showing it. May be NULL.</description>
	///		</item>
	///		<item>
	///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_TRAY_CREATE_MIDDLECLICK_CALLBACK_POINTER"><c>SDL_PROP_TRAY_CREATE_MIDDLECLICK_CALLBACK_POINTER</c></see></term>
	///			<description>An <see href="https://wiki.libsdl.org/SDL3/SDL_TrayClickCallback">SDL_TrayClickCallback</see> to be invoked when the tray icon is middle-clicked. Not supported on all platforms. The callback should return true to show the default menu, or false to skip showing it. May be NULL.</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateTrayWithProperties">SDL_CreateTrayWithProperties</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Tray* SDL_CreateTrayWithProperties(uint props);

#endif

	/// <summary>
	/// Destroys a tray object
	/// </summary>
	/// <param name="tray">The tray icon to be destroyed</param>
	/// <remarks>
	/// <para>
	/// This also destroys all associated menus and entries.
	/// </para>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_DestroyTray">SDL_DestroyTray</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_DestroyTray(SDL_Tray* tray);

	/// <summary>
	/// Gets a previously created tray menu
	/// </summary>
	/// <param name="tray">The tray entry to bind the menu to</param>
	/// <returns>Returns the newly created menu</returns>
	/// <remarks>
	/// <para>
	/// You should have called <see href="https://wiki.libsdl.org/SDL3/SDL_CreateTrayMenu">SDL_CreateTrayMenu</see>() on the tray object.
	/// This function allows you to fetch it again later.
	/// </para>
	/// <para>
	/// This function does the same thing as <see href="">SDL_GetTraySubmenu</see>(),
	/// except that it takes a <see href="https://wiki.libsdl.org/SDL3/SDL_Tray">SDL_Tray</see> instead of a <see href="https://wiki.libsdl.org/SDL3/SDL_TrayEntry">SDL_TrayEntry</see>.
	/// </para>
	/// <para>
	/// A menu does not need to be destroyed; it will be destroyed with the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetTrayMenu">SDL_GetTrayMenu</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial TrayMenu.SDL_TrayMenu* SDL_GetTrayMenu(SDL_Tray* tray);

	/// <summary>
	/// Updates the system tray icon's icon
	/// </summary>
	/// <param name="tray">The tray icon to be updated</param>
	/// <param name="icon">The new icon. May be NULL.</param>
	/// <remarks>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetTrayIcon">SDL_SetTrayIcon</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_SetTrayIcon(SDL_Tray* tray, Surface.SDL_Surface* icon);

	/// <summary>
	/// Updates the system tray icon's tooltip
	/// </summary>
	/// <param name="tray">The tray icon to be updated</param>
	/// <param name="tooltip">The new tooltip in UTF-8 encoding. May be NULL.</param>
	/// <remarks>
	/// <para>
	/// This function should be called on the thread that created the tray.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetTrayTooltip">SDL_SetTrayTooltip</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_SetTrayTooltip(SDL_Tray* tray, byte* tooltip);

	/// <summary>
	/// Update the trays
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is called automatically by the event loop and is only needed if you're using trays but aren't handling SDL events.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_UpdateTrays">SDL_UpdateTrays</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial void SDL_UpdateTrays();
}
