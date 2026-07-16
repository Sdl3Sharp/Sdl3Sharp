using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using unsafe SDL_CleanupPropertyCallback = delegate* unmanaged[Cdecl]<void*, void*, void>;
using unsafe SDL_EnumeratePropertiesCallback = delegate* unmanaged[Cdecl]<void*, uint, byte*, void>;

namespace Sdl3Sharp;

partial class Properties
{
	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private unsafe static void CleanupPropertyCallback(void* userdata, void* value)
	{
		if (userdata is not null && GCHandle.FromIntPtr(unchecked((IntPtr)userdata)) is { IsAllocated: true, Target: Action<IntPtr> cleanup } gcHandle)
		{
			try
			{
				cleanup(unchecked((IntPtr)value));
			}
			finally
			{
				gcHandle.Free();
			}
		}
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private unsafe static void EnumeratePropertiesCallback(void* userdata, uint props, byte* name)
	{
		if (userdata is not null && GCHandle.FromIntPtr(unchecked((nint)userdata)) is { IsAllocated: true, Target: Action<string> action })
		{
			var nameUtf16 = Utf8StringMarshaller.ConvertToManaged(name);

			if (!string.IsNullOrWhiteSpace(nameUtf16))
			{
				action(nameUtf16);
			}
		}
	}

	/// <summary>
	/// Clear a property from a group of properties
	/// </summary>
	/// <param name="props">the properties to modify</param>
	/// <param name="name">the name of the property to clear</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_ClearProperty">SDL_ClearProperty</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_ClearProperty(uint props, byte* name);

	/// <summary>
	/// Copy a group of properties
	/// </summary>
	/// <param name="src">the properties to copy</param>
	/// <param name="dst">the destination properties</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// Copy all the properties from one group of properties to another, with the exception of properties requiring cleanup (set using <see href="https://wiki.libsdl.org/SDL3/SDL_SetPointerPropertyWithCleanup">SDL_SetPointerPropertyWithCleanup</see>()), which will not be copied. Any property that already exists on dst will be overwritten.
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CopyProperties">SDL_CopyProperties</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial CBool SDL_CopyProperties(uint src, uint dst);

	/// <summary>
	/// Create a group of properties
	/// </summary>
	/// <returns>Returns an ID for a new group of properties, or 0 on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// All properties are automatically destroyed when <see href="https://wiki.libsdl.org/SDL3/SDL_Quit">SDL_Quit</see>() is called
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateProperties">SDL_CreateProperties</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial uint SDL_CreateProperties();

	/// <summary>
	/// Destroy a group of properties
	/// </summary>
	/// <param name="props">the properties to destroy</param>
	/// <remarks>
	/// All properties are deleted and their cleanup functions will be called, if any
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_DestroyProperties">SDL_DestroyProperties</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial void SDL_DestroyProperties(uint props);

	/// <summary>
	/// Enumerate the properties contained in a group of properties
	/// </summary>
	/// <param name="props">the properties to query</param>
	/// <param name="callback">the function to call for each property</param>
	/// <param name="userdata">a pointer that is passed to <c>callback</c></param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// The callback function is called for each property in the group of properties. The properties are locked during enumeration.
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_EnumerateProperties">SDL_EnumerateProperties</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_EnumerateProperties(uint props, SDL_EnumeratePropertiesCallback callback, void* userdata);

	/// <summary>
	/// Get a boolean property from a group of properties
	/// </summary>
	/// <param name="props">the properties to query</param>
	/// <param name="name">the name of the property to query</param>
	/// <param name="default_value">the default value of the property</param>
	/// <returns>Returns the value of the property, or <c>default_value</c> if it is not set or not a boolean property</returns>
	/// <remarks>
	/// You can use <see href="https://wiki.libsdl.org/SDL3/SDL_GetPropertyType">SDL_GetPropertyType</see>() to query whether the property exists and is a boolean property
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetBooleanProperty">SDL_GetBooleanProperty</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_GetBooleanProperty(uint props, byte* name, CBool default_value);

	/// <summary>
	/// Get a floating point property from a group of properties
	/// </summary>
	/// <param name="props">the properties to query</param>
	/// <param name="name">the name of the property to query</param>
	/// <param name="default_value">the default value of the property</param>
	/// <returns>Returns the value of the property, or <c>default_value</c> if it is not set or not a float property</returns>
	/// <remarks>
	/// You can use <see href="https://wiki.libsdl.org/SDL3/SDL_GetPropertyType">SDL_GetPropertyType</see>() to query whether the property exists and is a floating point property
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetFloatProperty">SDL_GetFloatProperty</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial float SDL_GetFloatProperty(uint props, byte* name, float default_value);

	/// <summary>
	/// Get the global SDL properties
	/// </summary>
	/// <returns>Returns a valid property ID on success or 0 on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError()</see> for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetGlobalProperties">SDL_GetGlobalProperties</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial uint SDL_GetGlobalProperties();

	/// <summary>
	/// Get a number property from a group of properties
	/// </summary>
	/// <param name="props">the properties to query</param>
	/// <param name="name">the name of the property to query</param>
	/// <param name="default_value">the default value of the property</param>
	/// <returns>Returns the value of the property, or <c>default_value</c> if it is not set or not a number property</returns>
	/// <remarks>You can use <see href="https://wiki.libsdl.org/SDL3/SDL_GetPropertyType">SDL_GetPropertyType</see>() to query whether the property exists and is a number property</remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetNumberProperty">SDL_GetNumberProperty</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial long SDL_GetNumberProperty(uint props, byte* name, long default_value);

	/// <summary>
	/// Get a pointer property from a group of properties
	/// </summary>
	/// <param name="props">the properties to query</param>
	/// <param name="name">the name of the property to query</param>
	/// <param name="default_value">the default value of the property</param>
	/// <returns>Returns the value of the property, or <c>default_value</c> if it is not set or not a pointer property</returns>
	/// <remarks>
	/// By convention, the names of properties that SDL exposes on objects will start with "SDL.", and properties that SDL uses internally will start with "SDL.internal.". These should be considered read-only and should not be modified by applications.
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetPointerProperty">SDL_GetPointerProperty</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void* SDL_GetPointerProperty(uint props, byte* name, void* default_value);

	/// <summary>
	/// Get the type of a property in a group of properties
	/// </summary>
	/// <param name="props">the properties to query</param>
	/// <param name="name">the name of the property to query</param>
	/// <returns>Returns the type of the property, or <see href="https://wiki.libsdl.org/SDL3/SDL_PROPERTY_TYPE_INVALID">SDL_PROPERTY_TYPE_INVALID</see> if it is not set</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetPropertyType">SDL_GetPropertyType</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial PropertyType SDL_GetPropertyType(uint props, byte* name);

	/// <summary>
	/// Get a string property from a group of properties
	/// </summary>
	/// <param name="props">the properties to query</param>
	/// <param name="name">the name of the property to query</param>
	/// <param name="default_value">the default value of the property</param>
	/// <returns>Returns the value of the property, or <c>default_value</c> if it is not set or not a string property</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetStringProperty">SDL_GetStringProperty</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial byte* SDL_GetStringProperty(uint props, byte* name, byte* default_value);

	/// <summary>
	/// Return whether a property exists in a group of properties
	/// </summary>
	/// <param name="props">the properties to query</param>
	/// <param name="name">the name of the property to query</param>
	/// <returns>Returns true if the property exists, or false if it doesn't</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HasProperty">SDL_HasProperty</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_HasProperty(uint props, byte* name);

	/// <summary>
	/// Lock a group of properties
	/// </summary>
	/// <param name="props">the properties to lock</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// Obtain a multi-threaded lock for these properties. Other threads will wait while trying to lock these properties until they are unlocked. Properties must be unlocked before they are destroyed.
	///
	/// The lock is automatically taken when setting individual properties, this function is only needed when you want to set several properties atomically or want to guarantee that properties being queried aren't freed in another thread.
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_LockProperties">SDL_LockProperties</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial CBool SDL_LockProperties(uint props);

	/// <summary>
	/// Set a boolean property in a group of properties
	/// </summary>
	/// <param name="props">the properties to modify</param>
	/// <param name="name">the name of the property to modify</param>
	/// <param name="value">the new value of the property</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetBooleanProperty">SDL_SetBooleanProperty</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetBooleanProperty(uint props, byte* name, CBool value);

	/// <summary>
	/// Set a floating point property in a group of properties
	/// </summary>
	/// <param name="prop">the properties to modify</param>
	/// <param name="name">the name of the property to modify</param>
	/// <param name="value">the new value of the property</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetFloatProperty">SDL_SetFloatProperty</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetFloatProperty(uint prop, byte* name, float value);

	/// <summary>
	/// Set an integer property in a group of properties
	/// </summary>
	/// <param name="props">the properties to modify</param>
	/// <param name="name">the name of the property to modify</param>
	/// <param name="value">the new value of the property</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetNumberProperty">SDL_SetNumberProperty</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetNumberProperty(uint props, byte* name, long value);

	/// <summary>
	/// Set a pointer property in a group of properties
	/// </summary>
	/// <param name="props">the properties to modify</param>
	/// <param name="name">the name of the property to modify</param>
	/// <param name="value">the new value of the property, or NULL to delete the property</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetPointerProperty">SDL_SetPointerProperty</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetPointerProperty(uint props, byte* name, void* value);

	/// <summary>
	/// Set a pointer property in a group of properties with a cleanup function that is called when the property is deleted
	/// </summary>
	/// <param name="props">the properties to modify</param>
	/// <param name="name">the name of the property to modify</param>
	/// <param name="value">the new value of the property, or NULL to delete the property</param>
	/// <param name="cleanup">the function to call when this property is deleted, or NULL if no cleanup is necessary</param>
	/// <param name="userdata">a pointer that is passed to the cleanup function</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// The cleanup function is also called if setting the property fails for any reason.
	/// 
	/// For simply setting basic data types, like numbers, bools, or strings, use <see href="https://wiki.libsdl.org/SDL3/SDL_SetNumberProperty">SDL_SetNumberProperty</see>, <see href="https://wiki.libsdl.org/SDL3/SDL_SetBooleanProperty">SDL_SetBooleanProperty</see>, or <see href="https://wiki.libsdl.org/SDL3/SDL_SetStringProperty">SDL_SetStringProperty</see> instead, as those functions will handle cleanup on your behalf. This function is only for more complex, custom data.
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetPointerPropertyWithCleanup">SDL_SetPointerPropertyWithCleanup</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetPointerPropertyWithCleanup(uint props, byte* name, void* value, SDL_CleanupPropertyCallback cleanup, void* userdata);

	/// <summary>
	/// Set a string property in a group of properties
	/// </summary>
	/// <param name="props">the properties to modify</param>
	/// <param name="name">the name of the property to modify</param>
	/// <param name="value">the new value of the property, or NULL to delete the property</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// This function makes a copy of the string; the caller does not have to preserve the data after this call completes
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetStringProperty">SDL_SetStringProperty</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetStringProperty(uint props, byte* name, byte* value);

	/// <summary>
	/// Unlock a group of properties
	/// </summary>
	/// <param name="props">the properties to unlock</param>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_UnlockProperties">SDL_UnlockProperties</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial void SDL_UnlockProperties(uint props);
}
