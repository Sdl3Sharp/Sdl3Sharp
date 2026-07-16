using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp;

partial class Sdl
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate AppResult AppInit(void** appstate, int argc, byte** argv);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private unsafe delegate int Main(int argc, byte** argv);

    /// <summary>
    /// Get metadata about your app
    /// </summary>
    /// <param name="name">the name of the metadata property to get</param>
    /// <returns>Returns the current value of the metadata property, or the default if it is not set, NULL for properties with no default</returns>
    /// <remarks>
    /// This returns metadata previously set using <see href="https://wiki.libsdl.org/SDL3/SDL_SetAppMetadata">SDL_SetAppMetadata</see>() or <see href="https://wiki.libsdl.org/SDL3/SDL_SetAppMetadataProperty">SDL_SetAppMetadataProperty</see>(). See <see href="https://wiki.libsdl.org/SDL3/SDL_SetAppMetadataProperty">SDL_SetAppMetadataProperty</see>() for the list of available properties and their meanings.
    /// </remarks>
    /// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetAppMetadataProperty">SDL_GetAppMetadataProperty</seealso>
    [NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
    internal unsafe static partial byte* SDL_GetAppMetadataProperty(byte* name);    

    /// <summary>
    /// Get the code revision of SDL that is linked against your program
    /// </summary>
    /// <returns>Returns an arbitrary string, uniquely identifying the exact revision of the SDL library in use</returns>
    /// <remarks>
    /// This value is the revision of the code you are linked with and may be different from the code you are compiling with, which is found in the constant <see href="https://wiki.libsdl.org/SDL3/SDL_REVISION">SDL_REVISION</see>.
    ///
    /// The revision is arbitrary string (a hash value) uniquely identifying the exact revision of the SDL library in use, and is only useful in comparing against other revisions. It is NOT an incrementing number.
    ///
    /// If SDL wasn't built from a git repository with the appropriate tools, this will return an empty string.
    ///
    /// You shouldn't use this function for anything but logging it for debugging purposes. The string is not intended to be reliable in any way.
    /// </remarks>
    /// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetRevision">SDL_GetRevision</seealso>
    [NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
    internal unsafe static partial byte* SDL_GetRevision();

    /// <summary>
    /// Get the version of SDL that is linked against your program
    /// </summary>
    /// <returns>Returns the version of the linked library</returns>
    /// <remarks>
    /// If you are linking to SDL dynamically, then it is possible that the current version will be different than the version you compiled against. This function returns the current version, while <see href="https://wiki.libsdl.org/SDL3/SDL_VERSION">SDL_VERSION</see> is the version you compiled with.
    ///
    /// This function may be called safely at any time, even before <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see>().
    /// </remarks>
    /// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetVersion">SDL_GetVersion</seealso>
    [NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl), typeof(CallConvSuppressGCTransition)])]
    internal static partial Version SDL_GetVersion();

    /// <summary>
    /// Initialize the SDL library
    /// </summary>
    /// <param name="flags">subsystem initialization flags</param>
    /// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
    /// <remarks>
    /// <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see>() simply forwards to calling <see href="https://wiki.libsdl.org/SDL3/SDL_InitSubSystem">SDL_InitSubSystem</see>(). Therefore, the two may be used interchangeably. Though for readability of your code <see href="https://wiki.libsdl.org/SDL3/SDL_InitSubSystem">SDL_InitSubSystem</see>() might be preferred.
    /// The file I/O (for example: <see href="https://wiki.libsdl.org/SDL3/SDL_IOFromFile">SDL_IOFromFile</see>) and threading (<see href="https://wiki.libsdl.org/SDL3/SDL_CreateThread">SDL_CreateThread</see>) subsystems are initialized by default. Message boxes (<see href="https://wiki.libsdl.org/SDL3/SDL_ShowSimpleMessageBox">SDL_ShowSimpleMessageBox</see>) also attempt to work without initializing the video subsystem, in hopes of being useful in showing an error dialog when <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see> fails. You must specifically initialize other subsystems if you use them in your application.
    /// 
    /// Logging (such as <see href="https://wiki.libsdl.org/SDL3/SDL_Log">SDL_Log</see>) works without initialization, too.
    /// 
    /// <c>flags</c> may be any of the following OR'd together:
    /// <list type="bullet">
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_INIT_AUDIO"><c>SDL_INIT_AUDIO</c></see></term>
    ///			<description>audio subsystem; automatically initializes the events subsystem</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_INIT_VIDEO"><c>SDL_INIT_VIDEO</c></see></term>
    ///			<description>video subsystem; automatically initializes the events subsystem, should be initialized on the main thread.</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_INIT_JOYSTICK"><c>SDL_INIT_JOYSTICK</c></see></term>
    ///			<description>joystick subsystem; automatically initializes the events subsystem</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_INIT_HAPTIC"><c>SDL_INIT_HAPTIC</c></see></term>
    ///			<description>haptic (force feedback) subsystem</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_INIT_GAMEPAD"><c>SDL_INIT_GAMEPAD</c></see></term>
    ///			<description>gamepad subsystem; automatically initializes the joystick subsystem</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_INIT_EVENTS"><c>SDL_INIT_EVENTS</c></see></term>
    ///			<description>events subsystem</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_INIT_SENSOR"><c>SDL_INIT_SENSOR</c></see></term>
    ///			<description>sensor subsystem; automatically initializes the events subsystem</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_INIT_CAMERA"><c>SDL_INIT_CAMERA</c></see></term>
    ///			<description>camera subsystem; automatically initializes the events subsystem</description>
    ///		</item>
    /// </list>
    /// 
    /// Subsystem initialization is ref-counted, you must call <see href="https://wiki.libsdl.org/SDL3/SDL_QuitSubSystem">SDL_QuitSubSystem</see>() for each <see href="https://wiki.libsdl.org/SDL3/SDL_InitSubSystem">SDL_InitSubSystem</see>() to correctly shutdown a subsystem manually (or call <see href="https://wiki.libsdl.org/SDL3/SDL_Quit">SDL_Quit</see>() to force shutdown). If a subsystem is already loaded then this call will increase the ref-count and return.
    ///
    /// Consider reporting some basic metadata about your application before calling <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see>, using either <see href="https://wiki.libsdl.org/SDL3/SDL_SetAppMetadata">SDL_SetAppMetadata</see>() or <see href="https://wiki.libsdl.org/SDL3/SDL_SetAppMetadataProperty">SDL_SetAppMetadataProperty</see>().
    /// </remarks>
    /// <seealso href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</seealso>
    [NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial CBool SDL_Init(SubSystems flags);

	/// <summary>
	/// Compatibility function to initialize the SDL library
	/// </summary>
	/// <param name="flags">any of the flags used by <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see>(); see <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see> for details</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>This function and <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see>() are interchangeable</remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_InitSubSystem">SDL_InitSubSystem</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial CBool SDL_InitSubSystem(SubSystems flags);

	/// <summary>
	/// Clean up all initialized subsystems
	/// </summary>
	/// <remarks>
	/// You should call this function even if you have already shutdown each initialized subsystem with <see href="https://wiki.libsdl.org/SDL3/SDL_QuitSubSystem">SDL_QuitSubSystem</see>(). It is safe to call this function even in the case of errors in initialization.
	/// 
	/// You can use this function with atexit() to ensure that it is run when your application is shutdown, but it is not wise to do this from a library or other dynamically loaded code.
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_Quit">SDL_Quit</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial void SDL_Quit();

	/// <summary>
	/// Shut down specific SDL subsystems
	/// </summary>
	/// <param name="flags">any of the flags used by <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see>(); see <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see> for details</param>
	/// <remarks>
	/// You still need to call <see href="https://wiki.libsdl.org/SDL3/SDL_Quit">SDL_Quit</see>() even if you close all open subsystems with <see href="https://wiki.libsdl.org/SDL3/SDL_QuitSubSystem">SDL_QuitSubSystem</see>()
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_QuitSubSystem">SDL_QuitSubSystem</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial void SDL_QuitSubSystem(SubSystems flags);

    /// <summary>
    /// Specify basic metadata about your app
    /// </summary>
    /// <param name="appname">The name of the application (e.g., "My Game 2: Bad Guy's Revenge!")</param>
    /// <param name="appversion">The version of the application (e.g., "1.0.0beta5" or a git hash, or whatever makes sense)</param>
    /// <param name="appidentifier">A unique string in reverse-domain format that identifies this app (e.g., "com.example.mygame2")</param>
    /// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
    /// <remarks>
    /// You can optionally provide metadata about your app to SDL. This is not required, but strongly encouraged.
    ///
    /// There are several locations where SDL can make use of metadata (an "About" box in the macOS menu bar, the name of the app can be shown on some audio mixers, etc). Any piece of metadata can be left as NULL, if a specific detail doesn't make sense for the app.
    ///
    /// This function should be called as early as possible, before <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see>. Multiple calls to this function are allowed, but various state might not change once it has been set up with a previous call to this function.
    /// 
    /// Passing a NULL removes any previous metadata.
    ///
    /// This is a simplified interface for the most important information. You can supply significantly more detailed metadata with <see href="https://wiki.libsdl.org/SDL3/SDL_SetAppMetadataProperty">SDL_SetAppMetadataProperty</see>().
    /// </remarks>
    /// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetAppMetadata">SDL_SetAppMetadata</seealso>
    [NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
    internal unsafe static partial CBool SDL_SetAppMetadata(byte* appname, byte* appversion, byte* appidentifier);

    /// <summary>
    /// Specify metadata about your app through a set of properties
    /// </summary>
    /// <param name="name">the name of the metadata property to set</param>
    /// <param name="value">the value of the property, or NULL to remove that property</param>
    /// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
    /// <remarks>
    /// You can optionally provide metadata about your app to SDL. This is not required, but strongly encouraged.
    ///
    /// There are several locations where SDL can make use of metadata (an "About" box in the macOS menu bar, the name of the app can be shown on some audio mixers, etc). Any piece of metadata can be left out, if a specific detail doesn't make sense for the app.
    ///
    /// This function should be called as early as possible, before <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see>. Multiple calls to this function are allowed, but various state might not change once it has been set up with a previous call to this function.
    ///
    /// Once set, this metadata can be read using <see href="https://wiki.libsdl.org/SDL3/SDL_GetAppMetadataProperty">SDL_GetAppMetadataProperty</see>().
    ///
    /// These are the supported properties:
    ///
    /// <list type="bullet">
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_APP_METADATA_NAME_STRING"><c>SDL_PROP_APP_METADATA_NAME_STRING</c></see></term>
    ///			<description>The human-readable name of the application, like "My Game 2: Bad Guy's Revenge!". This will show up anywhere the OS shows the name of the application separately from window titles, such as volume control applets, etc. This defaults to "SDL Application".</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_APP_METADATA_VERSION_STRING"><c>SDL_PROP_APP_METADATA_VERSION_STRING</c></see></term>
    ///			<description>The version of the app that is running; there are no rules on format, so "1.0.3beta2" and "April 22nd, 2024" and a git hash are all valid options. This has no default.</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_APP_METADATA_IDENTIFIER_STRING"><c>SDL_PROP_APP_METADATA_IDENTIFIER_STRING</c></see></term>
    ///			<description>A unique string that identifies this app. This must be in reverse-domain format, like "com.example.mygame2". This string is used by desktop compositors to identify and group windows together, as well as match applications with associated desktop settings and icons. If you plan to package your application in a container such as Flatpak, the app ID should match the name of your Flatpak container as well. This has no default.</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_APP_METADATA_CREATOR_STRING"><c>SDL_PROP_APP_METADATA_CREATOR_STRING</c></see></term>
    ///			<description>The human-readable name of the creator/developer/maker of this app, like "MojoWorkshop, LLC"</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_APP_METADATA_COPYRIGHT_STRING"><c>SDL_PROP_APP_METADATA_COPYRIGHT_STRING</c></see></term>
    ///			<description>The human-readable copyright notice, like "Copyright (c) 2024 MojoWorkshop, LLC" or whatnot. Keep this to one line, don't paste a copy of a whole software license in here. This has no default.</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_APP_METADATA_URL_STRING"><c>SDL_PROP_APP_METADATA_URL_STRING</c></see></term>
    ///			<description>A URL to the app on the web. Maybe a product page, or a storefront, or even a GitHub repository, for user's further information This has no default.</description>
    ///		</item>
    ///		<item>
    ///			<term><see href="https://wiki.libsdl.org/SDL3/SDL_PROP_APP_METADATA_TYPE_STRING"><c>SDL_PROP_APP_METADATA_TYPE_STRING</c></see></term>
    ///			<description>The type of application this is. Currently this string can be "game" for a video game, "mediaplayer" for a media player, or generically "application" if nothing else applies. Future versions of SDL might add new types. This defaults to "application".</description>
    ///		</item>
    /// </list>
    /// </remarks>
    /// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetAppMetadataProperty">SDL_SetAppMetadataProperty</seealso>
    [NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
    internal unsafe static partial CBool SDL_SetAppMetadataProperty(byte* name, byte* value);

    /// <summary>
    /// Get a mask of the specified subsystems which are currently initialized
    /// </summary>
    /// <param name="flags">any of the flags used by <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see>(); see <see href="https://wiki.libsdl.org/SDL3/SDL_Init">SDL_Init</see> for details</param>
    /// <returns>Returns a mask of all initialized subsystems if <c><paramref name="flags"/></c> is 0, otherwise it returns the initialization status of the specified subsystems</returns>
    /// <seealso href="https://wiki.libsdl.org/SDL3/SDL_WasInit">SDL_WasInit</seealso>
    [NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
    internal static partial SubSystems SDL_WasInit(SubSystems flags);    
}
