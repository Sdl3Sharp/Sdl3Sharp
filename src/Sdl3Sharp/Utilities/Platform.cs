using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.Internal.Interop.NativeImportConditions;
using Sdl3Sharp.SourceGeneration;
using Sdl3Sharp.Threading;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.Versioning;

namespace Sdl3Sharp.Utilities;

/// <summary>
/// Provides methods and properties to identify the current executing platform as well as to interact with it on a lower level in a more platform-specific way.
/// </summary>
/// <remarks>
/// <para>
/// Most application can make do without using these methods,
/// but they can be useful for integrating with other parts of a specific system, 
/// adding platform-specific polish to an applicaiton,
/// or solving problems that only affect one target.
/// </para>
/// </remarks>
public static partial class Platform
{
	/// <summary>
	/// Gets a value indicating whether the current platform is a Chromebook
	/// </summary>
	/// <value>
	/// A value indicating whether the current platform is a Chromebook
	/// </value>
	/// <remarks>
	/// <para>
	/// Note: This property is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	[SupportedOSPlatform("Android")]
	public static bool IsChromebook
	{
		get
		{
			if (!INativeImportCondition.Evaluate<IsAndroid>())
			{
				failPlatformNotSupported();
			}

			return SDL_IsChromebook();

			[DoesNotReturn]
			static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(IsChromebook)} is only supported on Android.");
		}
	}

	/// <summary>
	/// Gets a value indicating whether the current platform is running Samsung's DeX mode
	/// </summary>
	/// <value>
	/// A value indicating whether the current platform is running Samsung's DeX mode
	/// </value>
	/// <remarks>
	/// <para>
	/// Note: This property is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	[SupportedOSPlatform("Android")]
	public static bool IsDeX
	{
		get
		{
			if (!INativeImportCondition.Evaluate<IsAndroid>())
			{
				failPlatformNotSupported();
			}

			return SDL_IsDeXMode();

			[DoesNotReturn]
			static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(IsDeX)} is only supported on Android.");
		}
	}

	/// <summary>
	/// Gets a value indicating whether the current platform is a tablet
	/// </summary>
	/// <value>
	/// A value indicating whether the current platform is a tablet
	/// </value>
	/// <remarks>
	/// <para>
	/// If SDL can't determine whether the current platform is a tablet, the value of this property will be <c><see langword="false"/></c>.
	/// </para>
	/// </remarks>
	public static bool IsTablet => SDL_IsTablet();

	/// <summary>
	/// Gets a value indicating whether the current platform is a (smart) TV
	/// </summary>
	/// <value>
	/// A value indicating whether the current platform is a (smart) TV
	/// </value>
	/// <remarks>
	/// <para>
	/// If SDL can't determine whether the current platform is a (smart) TV, the value of this property will be <c><see langword="false"/></c>.
	/// </para>
	/// </remarks>
	public static bool IsTV => SDL_IsTV();

	/// <summary>
	/// Gets the name of the platform
	/// </summary>
	/// <value>
	/// The name of the platform, if it's available; otherwise a string beginning with <c>"Unknown"</c>
	/// </value>
	/// <remarks>
	/// <para>
	/// If the platform is not available, the value of this property is a string beginning with <c>"Unknown"</c>.
	/// </para>
	/// <para>
	/// The following is an incomplete list of supported platform names:
	/// <list type="bullet">
	/// <item><description><c>"Windows"</c></description></item>
	/// <item><description><c>"macOS"</c></description></item>
	/// <item><description><c>"Linux"</c></description></item>
	/// <item><description><c>"iOS"</c></description></item>
	/// <item><description><c>"Android"</c></description></item>
	/// </list>
	/// </para>
	/// </remarks>
	public static string Name
	{
		get
		{
			unsafe
			{
				return Utf8StringMarshaller.ConvertToManaged(SDL_GetPlatform())!;
			}
		}
	}

	/// <summary>
	/// Gets the <see cref="Sandbox"/> environment of the application
	/// </summary>
	/// <value>
	/// The <see cref="Sandbox"/> environment of the application,
	/// including <see cref="Sandbox.None"/>, if the application isn't execute in a <see cref="Sandbox"/> environment.
	/// </value>
	public static Sandbox Sandbox
	{
		get => SDL_GetSandbox();
	}

	/// <summary>
	/// Gets the current state of external storage for the Android application
	/// </summary>
	/// <returns>
	/// The current state of external storage for the Android application,
	/// including <see cref="AndroidExternalStorageState.Unavailable"/>, if external storage is currently unavailable
	/// </returns>
	/// <remarks>
	/// <para>
	/// Note: This method is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	[SupportedOSPlatform("Android")]
	public static AndroidExternalStorageState GetAndroidExternalStorageState()
	{
		if (!INativeImportCondition.Evaluate<IsAndroid>())
		{
			failPlatformNotSupported();
		}

		return SDL_GetAndroidExternalStorageState();

		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(GetAndroidExternalStorageState)} is only supported on Android.");
	}

	/// <summary>
	/// Gets the Android API level of the current device
	/// </summary>
	/// <returns>The Android API level of the current device</returns>
	/// <remarks>
	/// <para>
	/// This method returns the Android API level of the current device, not the Android version.
	/// The following is an incomplete mapping from Android API levels to Android versions:
	/// <list type="bullet">
	/// <item>
	///		<term>API level 35</term>
	///		<description>Android 15 (VANILLA_ICE_CREAM)</description>
	/// </item>
	/// <item>
	///		<term>API level 34</term>
	///		<description>Android 14 (UPSIDE_DOWN_CAKE)</description>
	/// </item>
	/// <item>
	///		<term>API level 33</term>
	///		<description>Android 13 (TIRAMISU)</description>
	/// </item>
	/// <item>
	///		<term>API level 32</term>
	///		<description>Android 12L (S_V2)</description>
	/// </item>
	/// <item>
	///		<term>API level 31</term>
	///		<description>Android 12 (S)</description>
	/// </item>
	/// <item>
	///		<term>API level 30</term>
	///		<description>Android 11 (R)</description>
	/// </item>
	/// <item>
	///		<term>API level 29</term>
	///		<description>Android 10 (Q)</description>
	/// </item>
	/// <item>
	///		<term>API level 28</term>
	///		<description>Android 9 (P)</description>
	/// </item>
	/// <item>
	///		<term>API level 27</term>
	///		<description>Android 8.1 (O_MR1)</description>
	/// </item>
	/// <item>
	///		<term>API level 26</term>
	///		<description>Android 8.0 (O)</description>
	/// </item>
	/// <item>
	///		<term>API level 25</term>
	///		<description>Android 7.1 (N_MR1)</description>
	/// </item>
	/// <item>
	///		<term>API level 24</term>
	///		<description>Android 7.0 (N)</description>
	/// </item>
	/// <item>
	///		<term>API level 23</term>
	///		<description>Android 6.0 (M)</description>
	/// </item>
	/// <item>
	///		<term>API level 22</term>
	///		<description>Android 5.1 (LOLLIPOP_MR1)</description>
	/// </item>
	/// <item>
	///		<term>API level 21</term>
	///		<description>Android 5.0 (LOLLIPOP, L)</description>
	/// </item>
	/// <item>
	///		<term>API level 20</term>
	///		<description>Android 4.4W (KITKAT_WATCH)</description>
	/// </item>
	/// <item>
	///		<term>API level 19</term>
	///		<description>Android 4.4 (KITKAT)</description>
	/// </item>
	/// <item>
	///		<term>API level 18</term>
	///		<description>Android 4.3 (JELLY_BEAN_MR2)</description>
	/// </item>
	/// <item>
	///		<term>API level 17</term>
	///		<description>Android 4.2 (JELLY_BEAN_MR1)</description>
	/// </item>
	/// <item>
	///		<term>API level 16</term>
	///		<description>Android 4.1 (JELLY_BEAN)</description>
	/// </item>
	/// <item>
	///		<term>API level 15</term>
	///		<description>Android 4.0.3 (ICE_CREAM_SANDWICH_MR1)</description>
	/// </item>
	/// <item>
	///		<term>API level 14</term>
	///		<description>Android 4.0 (ICE_CREAM_SANDWICH)</description>
	/// </item>
	/// <item>
	///		<term>API level 13</term>
	///		<description>Android 3.2 (HONEYCOMB_MR2)</description>
	/// </item>
	/// <item>
	///		<term>API level 12</term>
	///		<description>Android 3.1 (HONEYCOMB_MR1)</description>
	/// </item>
	/// <item>
	///		<term>API level 11</term>
	///		<description>Android 3.0 (HONEYCOMB)</description>
	/// </item>
	/// <item>
	///		<term>API level 10</term>
	///		<description>Android 2.3.3 (GINGERBREAD_MR1)</description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	[SupportedOSPlatform("Android")]
	public static int GetAndroidSDKVersion()
	{
		if (!INativeImportCondition.Evaluate<IsAndroid>())
		{
			failPlatformNotSupported();
		}

		return SDL_GetAndroidSDKVersion();
		
		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(GetAndroidSDKVersion)} is only supported on Android.");
	}

	/// <summary>
	/// Report <c>onApplicationDidChangeStatusBarOrientation</c> on Apple iOS when using external event handling
	/// </summary>
	/// <remarks> 
	/// <para>
	/// This method allows apps that have their own event handling to hook into SDL to generate SDL events.
	/// It maps directly to the iOS-specific event <c>onApplicationDidChangeStatusBarOrientation</c>.
	/// </para>
	/// <para>
	/// Most apps do not need to use this method directly; SDL's internal event code will handle all this for windows created by <see cref="SDL_CreateWindow"/>!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException"></exception>
	[SupportedOSPlatform("iOS")]
	public static void OnApplicationDidChangeStatusBarOrientation()
	{
		if (!INativeImportCondition.Evaluate<IsIOS>())
		{
			failPlatformNotSupported();
		}

		SDL_OnApplicationDidChangeStatusBarOrientation();
		
		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(OnApplicationDidChangeStatusBarOrientation)} is only supported on iOS.");
	}

	/// <summary>
	/// Report <c>onApplicationDidEnterBackground</c> on Apple iOS when using external event handling
	/// </summary>
	/// <remarks> 
	/// <para>
	/// This method allows apps that have their own event handling to hook into SDL to generate SDL events.
	/// It maps directly to the iOS-specific event <c>onApplicationDidEnterBackground</c>.
	/// </para>
	/// <para>
	/// Since this method doesn't do anything iOS-specific internally, it is available on all platforms, in case it might be useful for some specific paradigm.
	/// </para>
	/// <para>
	/// Most apps do not need to use this method directly; SDL's internal event code will handle all this for windows created by <see cref="SDL_CreateWindow"/>!
	/// </para>
	/// </remarks>
	public static void OnApplicationDidEnterBackground() => SDL_OnApplicationDidEnterBackground();

	/// <summary>
	/// Report <c>onApplicationDidBecomeActive</c> on Apple iOS when using external event handling
	/// </summary>
	/// <remarks> 
	/// <para>
	/// This method allows apps that have their own event handling to hook into SDL to generate SDL events.
	/// It maps directly to the iOS-specific event <c>onApplicationDidBecomeActive</c>.
	/// </para>
	/// <para>
	/// Since this method doesn't do anything iOS-specific internally, it is available on all platforms, in case it might be useful for some specific paradigm.
	/// </para>
	/// <para>
	/// Most apps do not need to use this method directly; SDL's internal event code will handle all this for windows created by <see cref="SDL_CreateWindow"/>!
	/// </para>
	/// </remarks>
	public static void OnApplicationDidEnterForeground() => SDL_OnApplicationDidEnterForeground();

	/// <summary>
	/// Report <c>onApplicationDidReceiveMemoryWarning</c> on Apple iOS when using external event handling
	/// </summary>
	/// <remarks> 
	/// <para>
	/// This method allows apps that have their own event handling to hook into SDL to generate SDL events.
	/// It maps directly to the iOS-specific event <c>onApplicationDidReceiveMemoryWarning</c>.
	/// </para>
	/// <para>
	/// Since this method doesn't do anything iOS-specific internally, it is available on all platforms, in case it might be useful for some specific paradigm.
	/// </para>
	/// <para>
	/// Most apps do not need to use this method directly; SDL's internal event code will handle all this for windows created by <see cref="SDL_CreateWindow"/>!
	/// </para>
	/// </remarks>
	public static void OnApplicationDidReceiveMemoryWarning() => SDL_OnApplicationDidReceiveMemoryWarning();

	/// <summary>
	/// Report <c>onApplicationWillResignActive</c> on Apple iOS when using external event handling
	/// </summary>
	/// <remarks> 
	/// <para>
	/// This method allows apps that have their own event handling to hook into SDL to generate SDL events.
	/// It maps directly to the iOS-specific event <c>onApplicationWillResignActive</c>.
	/// </para>
	/// <para>
	/// Since this method doesn't do anything iOS-specific internally, it is available on all platforms, in case it might be useful for some specific paradigm.
	/// </para>
	/// <para>
	/// Most apps do not need to use this method directly; SDL's internal event code will handle all this for windows created by <see cref="SDL_CreateWindow"/>!
	/// </para>
	/// </remarks>
	public static void OnApplicationWillEnterBackground() => SDL_OnApplicationWillEnterBackground();

	/// <summary>
	/// Report <c>onApplicationWillEnterForeground</c> on Apple iOS when using external event handling
	/// </summary>
	/// <remarks> 
	/// <para>
	/// This method allows apps that have their own event handling to hook into SDL to generate SDL events.
	/// It maps directly to the iOS-specific event <c>onApplicationWillEnterForeground</c>.
	/// </para>
	/// <para>
	/// Since this method doesn't do anything iOS-specific internally, it is available on all platforms, in case it might be useful for some specific paradigm.
	/// </para>
	/// <para>
	/// Most apps do not need to use this method directly; SDL's internal event code will handle all this for windows created by <see cref="SDL_CreateWindow"/>!
	/// </para>
	/// </remarks>
	public static void OnApplicationWillEnterForeground() => SDL_OnApplicationWillEnterForeground();

	/// <summary>
	/// Report <c>onApplicationWillTerminate</c> on Apple iOS when using external event handling
	/// </summary>
	/// <remarks> 
	/// <para>
	/// This method allows apps that have their own event handling to hook into SDL to generate SDL events.
	/// It maps directly to the iOS-specific event <c>onApplicationWillTerminate</c>.
	/// </para>
	/// <para>
	/// Since this method doesn't do anything iOS-specific internally, it is available on all platforms, in case it might be useful for some specific paradigm.
	/// </para>
	/// <para>
	/// Most apps do not need to use this method directly; SDL's internal event code will handle all this for windows created by <see cref="SDL_CreateWindow"/>!
	/// </para>
	/// </remarks>
	public static void OnApplicationWillTerminate() => SDL_OnApplicationWillTerminate();

	/// <summary>
	/// Triggers the Android system back button behavior
	/// </summary>
	/// <remarks>
	/// <para>
	/// Note: This method is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	[SupportedOSPlatform("Android")]
	public static void SendAndroidBackButton()
	{
		if (!INativeImportCondition.Evaluate<IsAndroid>())
		{
			failPlatformNotSupported();
		}

		SDL_SendAndroidBackButton();
		
		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(SendAndroidBackButton)} is only supported on Android.");
	}

	/// <summary>
	/// Enables or disables the SDL event pump on Apple iOS
	/// </summary>
	/// <param name="enabled"><c><see langword="true"/></c> to enable the SDL event pump; otherwise, <c><see langword="false"/></c> to disable it</param>
	/// <remarks>
	/// <para>
	/// Note: This method is only available on <em>Apple iOS</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Apple iOS</em></exception>
	[SupportedOSPlatform("iOS")]
	public static void SetIOSEventPump(bool enabled)
	{
		if (!INativeImportCondition.Evaluate<IsIOS>())
		{
			failPlatformNotSupported();
		}

		SDL_SetiOSEventPump(enabled);

		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(SendAndroidBackButton)} is only supported on iOS.");
	}

	private static GCHandle mWindowsMessageHook = default;

	/// <summary>
	/// Sets a callback for every Windows message
	/// </summary>
	/// <param name="callback">The <see cref="Utilities.WindowsMessageHook"/> to invoke for every Windows message</param>
	/// <remarks>
	/// <para>
	/// The <paramref name="callback"/> may modify the message,
	/// and should return <c><see langword="true"/></c> if the message should continue to be processed, or <c><see langword="false"/></c> to prevent further processing.
	/// </para>
	/// <para>
	/// The <paramref name="callback"/> is invoked before <c><see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-translatemessage">TranslateMessage</see></c>.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Windows</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Windows</em></exception>
	/// <exception cref="ArgumentNullException"><paramref name="callback"/> is <c><see langword="null"/></c></exception>
	[SupportedOSPlatform("Windows")]
	public static void SetWindowsMessageHook(WindowsMessageHook callback)
	{
		if (!INativeImportCondition.Evaluate<IsIOS>())
		{
			failPlatformNotSupported();
		}

		unsafe
		{
			if (callback is null)
			{
				failCallbackArgumentNull();
			}

			if (mWindowsMessageHook is { IsAllocated: true, Target: not null })
			{
				mWindowsMessageHook.Free();

				mWindowsMessageHook = default;
			}

			mWindowsMessageHook = GCHandle.Alloc(callback, GCHandleType.Normal);

			SDL_SetWindowsMessageHook(&WindowsMessageHook, unchecked((void*)GCHandle.ToIntPtr(mWindowsMessageHook)));
		}
		
		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(SetWindowsMessageHook)} is only supported on Windows.");

		[DoesNotReturn]
		static void failCallbackArgumentNull() => throw new ArgumentNullException(nameof(callback));
	}

	private static GCHandle mX11EventHook = default;

	/// <summary>
	/// Sets a callback for every X11 event
	/// </summary>
	/// <param name="callback">The <see cref="Utilities.X11EventHook"/> to invoke for every X11 event</param>
	/// <remarks>
	/// <para>
	/// The <paramref name="callback"/> may modify the event,
	/// and should return <c><see langword="true"/></c> if the event should continue to be processed, or <c><see langword="false"/></c> to prevent further processing.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="callback"/> is <c><see langword="null"/></c></exception>
	public static void SetX11EventHook(X11EventHook callback)
	{
		unsafe
		{
			if (callback is null)
			{
				failCallbackArgumentNull();
			}

			if (mX11EventHook is { IsAllocated: true, Target: not null })
			{
				mX11EventHook.Free();

				mX11EventHook = default;
			}

			mX11EventHook = GCHandle.Alloc(callback, GCHandleType.Normal);

			SDL_SetX11EventHook(&X11EventHook, unchecked((void*)GCHandle.ToIntPtr(mX11EventHook)));
		}

		[DoesNotReturn]
		static void failCallbackArgumentNull() => throw new ArgumentNullException(nameof(callback));
	}

	/// <summary>
	/// Tries to retrieve the Java instance of the Android activity class
	/// </summary>
	/// <param name="activity">
	/// The <see href="https://docs.oracle.com/javase/1.5.0/docs/guide/jni/spec/types.html#wp15954">jobject</see> representing the instance of the Activity class of the Android application,
	/// when this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="default"/>(<see cref="IntPtr"/>)</c>
	/// </param>
	/// <returns><c><see langword="true"/></c> if the Java instance of the Android activity class could be succesfully retrieved; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="activity"/> parameter is declared as <see cref="IntPtr"/> here, but it's actually of type <c><see href="https://docs.oracle.com/javase/1.5.0/docs/guide/jni/spec/types.html#wp15954">jobject</see></c> (a pointer type) defined like in <c>jni.h</c>.
	/// </para>
	/// <para>
	/// The resulting <see href="https://docs.oracle.com/javase/1.5.0/docs/guide/jni/spec/types.html#wp15954">jobject</see> <paramref name="activity"/> is a local reference and must be released by the caller.
	/// See the <c>PushLocalFrame</c> and <c>PopLocalFrame</c> or <c>DeleteLocalRef</c> functions of the Java native interface: <see href="https://docs.oracle.com/javase/1.5.0/docs/guide/jni/spec/functions.html"/>.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	[SupportedOSPlatform("Android")]
	public static bool TryGetAndroidActivity(out IntPtr activity)
	{		
		if (!INativeImportCondition.Evaluate<IsAndroid>())
		{
			failPlatformNotSupported();
		}

		unsafe
		{
			activity = unchecked((IntPtr)SDL_GetAndroidActivity());

			return activity != IntPtr.Zero;
		}

		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TryGetAndroidActivity)} is only supported on Android.");
	}

	/// <summary>
	/// Tries to get the path used for caching data for the Android application
	/// </summary>
	/// <param name="cachePath">The path used for caches for the Android application, when this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c> if the path used for caching data for the Android application could be succesfully retrieved; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The resulting path is unique to your application, but is public and can be written to by other applications.
	/// Its typically <c>"/data/data/your.app.package/cache/"</c>.
	/// </para>
	/// <para>
	/// This effectively wraps <c><see href="https://developer.android.com/reference/android/content/Context#getCacheDir()">android.content.Context.getCacheDir</see></c>.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	[SupportedOSPlatform("Android")]
	public static bool TryGetAndroidCachePath([NotNullWhen(true)] out string? cachePath)
	{			
		if (!INativeImportCondition.Evaluate<IsAndroid>())
		{
			failPlatformNotSupported();
		}

		unsafe
		{
			cachePath = Utf8StringMarshaller.ConvertToManaged(SDL_GetAndroidCachePath());

			return cachePath is not null;
		}	

		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TryGetAndroidCachePath)} is only supported on Android.");
	}

	/// <summary>
	/// Tries to get the path used for external storage for the Android application
	/// </summary>
	/// <param name="externalStoragePath">The path used for external storage for the Android application, when this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c> if the path used for external storage for the Android application could be succesfully retrieved; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The resulting path is unique to your application, but is public and can be written to by other applications.
	/// It's typically <c>"/storage/sdcard0/Android/data/your.app.package/files"</c>.
	/// </para>
	/// <para>
	/// This effectively wraps <c><see href="https://developer.android.com/reference/android/content/Context#getExternalFilesDir()">android.content.Context.getExternalFilesDir</see></c>.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	[SupportedOSPlatform("Android")]
	public static bool TryGetAndroidExternalStoragePath([NotNullWhen(true)] out string? externalStoragePath)
	{
		if (!INativeImportCondition.Evaluate<IsAndroid>())
		{
			failPlatformNotSupported();
		}

		unsafe
		{
			externalStoragePath = Utf8StringMarshaller.ConvertToManaged(SDL_GetAndroidExternalStoragePath());

			return externalStoragePath is not null;
		}

		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TryGetAndroidExternalStoragePath)} is only supported on Android.");
	}

	/// <summary>
	/// Tries to get the path used for internal storage for the Android application
	/// </summary>
	/// <param name="internalStoragePath">The path used for internal storage for the Android application, when this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="null"/></c></param>
	/// <returns><c><see langword="true"/></c> if the path used for internal storage for the Android application could be succesfully retrieved; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The resulting path is unique to your application, but is public and cannot be written to by other applications.
	/// It's typically <c>"/data/data/your.app.package/files"</c>.
	/// </para>
	/// <para>
	/// This effectively wraps <c><see href="https://developer.android.com/reference/android/content/Context#getFilesDir()">android.content.Context.getFilesDir</see></c>.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	[SupportedOSPlatform("Android")]
	public static bool TryGetAndroidInternalStoragePath([NotNullWhen(true)] out string? internalStoragePath)
	{
		if (!INativeImportCondition.Evaluate<IsAndroid>())
		{
			failPlatformNotSupported();
		}

		unsafe
		{
			internalStoragePath = Utf8StringMarshaller.ConvertToManaged(SDL_GetAndroidInternalStoragePath());

			return internalStoragePath is not null;
		}

		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TryGetAndroidInternalStoragePath)} is only supported on Android.");
	}

	/// <summary>
	/// Tries to get the Android Java Native Interface Environment of the current thread
	/// </summary>
	/// <param name="jniEnv">
	/// The <see href="https://docs.oracle.com/javase/1.5.0/docs/guide/jni/spec/functions.html#wp23720">Java native interface object (JNIEnv)</see> to which the current thread is attached,
	/// when this method returns <c><see langword="true"/></c>; otherwise, <c><see langword="default"/>(<see cref="IntPtr"/>)</c>
	/// </param>
	/// <returns><c><see langword="true"/></c> if the Android Java Native Interface Environment could be succesfully retrieved; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="jniEnv"/> parameter is declared as <see cref="IntPtr"/> here, but it's actually of type <c><see href="https://docs.oracle.com/javase/1.5.0/docs/guide/jni/spec/functions.html#wp23720">JNIEnv</see></c> (a pointer type) defined like in <c>jni.h</c>.
	/// </para>
	/// <para>
	/// The resulting <see href="https://docs.oracle.com/javase/1.5.0/docs/guide/jni/spec/functions.html#wp23720">JNIEnv</see> <paramref name="jniEnv"/> is needed to access the Java virtual machine and many Android APIs from native code.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	[SupportedOSPlatform("Android")]
	public static bool TryGetAndroidJniEnv(out IntPtr jniEnv)
	{
		if (!INativeImportCondition.Evaluate<IsAndroid>())
		{
			failPlatformNotSupported();
		}

		unsafe
		{
			jniEnv = unchecked((IntPtr)SDL_GetAndroidJNIEnv());

			return jniEnv != IntPtr.Zero;
		}

		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TryGetAndroidJniEnv)} is only supported on Android.");
	}

	/// <summary>
	/// Tries to get the D3D9 adapter index that matches a specified display
	/// </summary>
	/// <param name="displayId">The display id of the display that the resulting D3D9 adapter index should match</param>
	/// <param name="adapterIndex">The D3D9 adapter index that matches the given <paramref name="displayId"/> when this method returns <c><see langword="true"/></c>; otherwise, <c>-1</c></param>
	/// <returns><c><see langword="true"/></c> if the D3D9 apapter index matching the given <paramref name="displayId"/> could be succesfully retrieved; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The resulting adapter index (<paramref name="adapterIndex"/>) can be passed to <c><see href="https://learn.microsoft.com/en-us/windows/win32/api/d3d9helper/nf-d3d9helper-idirect3d9-createdevice">IDirect3D9::CreateDevice</see></c>
	/// and controls on which monitor a full screen application will appear.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Windows</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Windows</em></exception>
	[SupportedOSPlatform("Windows")]
	public static bool TryGetDirect3D9AdapterIndex(uint displayId, out int adapterIndex)
	{
		if (!INativeImportCondition.Evaluate<OrElse<IsWin32, IsWinGDK>>())
		{
			failPlatformNotSupported();
		}

		adapterIndex = SDL_GetDirect3D9AdapterIndex(displayId);

		return adapterIndex is not -1;
		
		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TryGetDirect3D9AdapterIndex)} is only supported on Windows.");
	}

	/// <summary>
	/// Tries to get the DXGI adapter index and output index for a specified display
	/// </summary>
	/// <param name="displayId">The display id of the display that the resulting DXGI adapter index and output index should match</param>
	/// <param name="adapterIndex">The DXGI adapter index that matches the given <paramref name="displayId"/> when this method returns <c><see langword="true"/></c>; otherwise, unspecified</param>
	/// <param name="outputIndex">The DXGI output index that matches the given <paramref name="displayId"/> when this method returns <c><see langword="true"/></c>; otherwise, unspecified</param>
	/// <returns><c><see langword="true"/></c> if the DXGI apapter index and output index matching the given <paramref name="displayId"/> could be succesfully retrieved; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The resulting DXGI adapter index (<paramref name="adapterIndex"/>) and output index (<paramref name="outputIndex"/>) can be passed to <c><see href="https://learn.microsoft.com/en-us/windows/win32/api/dxgi/nf-dxgi-idxgifactory-enumadapters">IDXGIFactory::EnumAdapters</see></c>
	/// and <c><see href="https://learn.microsoft.com/en-us/windows/win32/api/dxgi/nf-dxgi-idxgiadapter-enumoutputs">IDXGIAdapter::EnumOutputs</see></c> respectively to get the objects required to create a DX10 or DX11 device and swap chain.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Windows</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Windows</em></exception>
	[SupportedOSPlatform("Windows")]
	public static bool TryGetDXGIOutputInfo(uint displayId, out int adapterIndex, out int outputIndex)
	{
		if (!INativeImportCondition.Evaluate<OrElse<IsWin32, IsWinGDK>>())
		{
			failPlatformNotSupported();
		}

		unsafe
		{
			int localAdapterIndex, localOutputIndex;

			var result = SDL_GetDXGIOutputInfo(displayId, &localAdapterIndex, &localOutputIndex);

			adapterIndex = localAdapterIndex;
			outputIndex = localOutputIndex;

			return result;
		}
		
		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TryGetDXGIOutputInfo)} is only supported on Windows.");
	}

	/// <summary>
	/// Tries to get a handle to the default user for GDK
	/// </summary>
	/// <param name="userHandle">The default user handle for GDK when this method returns <c><see langword="true"/></c>; otherwise, unspecified</param>
	/// <returns><c><see langword="true"/></c> if the handle to the default user for GDK could be succesfully retrieved; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// The <paramref name="userHandle"/> parameter is declared as <see cref="IntPtr"/> here, but it's actually of type <c><see href="https://learn.microsoft.com/en-us/gaming/gdk/docs/features/common/user/player-identity-xuser">XUserHandle</see></c> (a handle type).
	/// </para>
	/// <para>
	/// This is effectively a synchronous version of <c><see href="https://learn.microsoft.com/en-us/gaming/gdk/docs/reference/system/xuser/functions/xuseraddasync">XUserAddAsync</see></c>, which always prefers the default user and allows a sign-in UI.
	/// </para> 
	/// <para>
	/// Note: This method is only available on platforms using <em>Microsoft's <see href="https://learn.microsoft.com/en-us/gaming/gdk/">Game Development Kit (GDK)</see></em>!
	/// Since there's currently no way to use GDK and Sdl3Sharp together, this method will <em>always</em> throw a <see cref="PlatformNotSupportedException"/>!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not using <em>Microsoft's <see href="https://learn.microsoft.com/en-us/gaming/gdk/">Game Development Kit (GDK)</see></em></exception>
	public static bool TryGetGDKDefaultUser(out IntPtr userHandle)
	{
		if (!INativeImportCondition.Evaluate<IsGDK>())
		{
			failPlatformNotSupported();
		}

		unsafe
		{
			void* localUserHandle;

			var result = SDL_GetGDKDefaultUser(&localUserHandle);

			userHandle = unchecked((IntPtr)localUserHandle);

			return result;
		}

		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TryGetGDKDefaultUser)} is only supported on GDK.");
	}

	/// <summary>
	/// Tries to get a handle to the global async task queue handle for GDK, initializing it if needed
	/// </summary>
	/// <param name="taskQueueHandle">The global async task queue handle for GDK when this method returns <c><see langword="true"/></c>; otherwise, unspecified</param>
	/// <returns><c><see langword="true"/></c> if the handle to the global async task queue for GDK could be succesfully retrieved; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks> 
	/// <para>
	/// The <paramref name="taskQueueHandle"/> parameter is declared as <see cref="IntPtr"/> here, but it's actually of type <c><see href="https://learn.microsoft.com/en-us/gaming/gdk/docs/features/common/async/async-libraries/async-library-xtaskqueue">XTaskQueueHandle</see></c> (a handle type).
	/// </para>
	/// <para>
	/// Once you are done with the task queue, you should call <c><see href="https://learn.microsoft.com/en-us/gaming/gdk/docs/reference/system/xtaskqueue/functions/xtaskqueueclosehandle">XTaskQueueCloseHandle</see></c> to reduce the reference count to avoid a resource leak.
	/// </para>
	/// <para>
	/// Note: This method is only available on platforms using <em>Microsoft's <see href="https://learn.microsoft.com/en-us/gaming/gdk/">Game Development Kit (GDK)</see></em>!
	/// Since there's currently no way to use GDK and Sdl3Sharp together, this method will <em>always</em> throw a <see cref="PlatformNotSupportedException"/>!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not using <em>Microsoft's <see href="https://learn.microsoft.com/en-us/gaming/gdk/">Game Development Kit (GDK)</see></em></exception>
	public static bool TryGetGDKTaskQueue(out IntPtr taskQueueHandle)
	{
		if (!INativeImportCondition.Evaluate<IsGDK>())
		{
			failPlatformNotSupported();
		}

		unsafe
		{
			void* localTaskQueueHandle;

			var result = SDL_GetGDKTaskQueue(&localTaskQueueHandle);

			taskQueueHandle = unchecked((IntPtr)localTaskQueueHandle);

			return result;
		}
		
		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TryGetGDKTaskQueue)} is only supported on GDK.");
	}

	/// <summary>
	/// Tries to request an Android permission at runtime, asynchronously
	/// </summary>
	/// <param name="permission">The permission to request. Should be one of those values: <see href="https://developer.android.com/reference/android/Manifest.permission"/>.</param>
	/// <param name="callback">A <see cref="AndroidRequestPermissionCallback"/> that will be invoked when the request has a response</param>
	/// <returns><c><see langword="true"/></c> if the request was succesfully submitted; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Note: The returned value of a call to this method just indicates if the request was succesfully submitted, not if the permission was granted.
	/// Only the given <paramref name="callback"/> will receive a response the request.
	/// </para>
	/// <para>
	/// You do not need to call this for built-in functionality of SDL.
	/// When using standard SDL APIs (e.g. recording from a microphone or reading images from a camera), SDL will manage permission requests for you.
	/// </para>
	/// <para>
	/// This method never blocks. Instead, the given <paramref name="callback"/> will be invoked when a decision has been made.
	/// That <paramref name="callback"/> may get invoked on a different thread, and possibly much later, as it might wait on a user to respond to a system dialog.
	/// If permission has already been granted for a specific entitlement, the <paramref name="callback"/> will still be invoked, probably on the current thread and before this method returns.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	/// <exception cref="ArgumentNullException"><paramref name="callback"/> is <c><see langword="null"/></c></exception>
	[SupportedOSPlatform("Android")]
	public static bool TryRequestAndroidPermission(string permission, AndroidRequestPermissionCallback callback)
	{
		if (!INativeImportCondition.Evaluate<IsAndroid>())
		{
			failPlatformNotSupported();
		}

		unsafe
		{
			if (callback is null)
			{
				failCallbackArgumentNull();
			}

			var permissionUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(permission);

			try
			{
				var gcHandle = GCHandle.Alloc(callback, GCHandleType.Normal);

				var result = SDL_RequestAndroidPermission(permissionUtf8, &RequestAndroidPermissionCallback, unchecked((void*)GCHandle.ToIntPtr(gcHandle)));

				if (!result.AsClrBool
					|| unchecked((ulong)result.RawValue) == (1ul << (Unsafe.SizeOf<CBool>() * 8)) - 1ul // catastrophic failure: check if the result is -1 (all bit's set to one, depended on this size of CBool)
				)
				{
					gcHandle.Free();

					return false;
				}

				return true;
			}
			finally
			{
				Utf8StringMarshaller.Free(permissionUtf8);
			}
		}
		
		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TryRequestAndroidPermission)} is only supported on Android.");

		[DoesNotReturn]
		static void failCallbackArgumentNull() => throw new ArgumentNullException(nameof(callback));
	}

	/// <summary>
	/// Tries to send a user command to the Android SDLActivity
	/// </summary>
	/// <param name="command">The user command. Must be greater or equal to <c>0x8000</c>.</param>
	/// <param name="parameter">The user parameter</param>
	/// <returns><c><see langword="true"/></c> if the user command was succesfully sent; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// You would override <c>boolean onUnhandledMessage(Message msg)</c> on the Android side to handle such a message.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	[SupportedOSPlatform("Android")]
	public static bool TrySendAndroidMessage(uint command, int parameter)
	{
		if (!INativeImportCondition.Evaluate<IsAndroid>())
		{
			failPlatformNotSupported();
		}

		return SDL_SendAndroidMessage(command, parameter);
		
		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TrySendAndroidMessage)} is only supported on Android.");
	}

	private static GCHandle mIOSAnimationCallback = default;

	/// <summary>
	/// Tries to set the animation callback on Apple iOS
	/// </summary>
	/// <param name="window">The <see cref="Window"/> for which the animation callback should be set</param>
	/// <param name="interval">The number of frames after which <paramref name="callback"/> will be invoked</param>
	/// <param name="callback">The <see cref="Utilities.IOSAnimationCallback"/> to invoke for every frame</param>
	/// <returns><c><see langword="true"/></c> if animation callback was succesfully set; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// Note that if you use an <see cref="App"/>'s execution model, especially <see cref="App.OnIterate()"/>, you don't have to use this API, as SDL will manage this for you.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Apple iOS</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks> 
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Apple iOS</em></exception>
	/// <exception cref="ArgumentNullException"><paramref name="callback"/> is <c><see langword="null"/></c></exception>
	[SupportedOSPlatform("iOS")]
	public static bool TrySetIOSAnimationCallback(Window window, int interval, IOSAnimationCallback callback)
	{
		if (!INativeImportCondition.Evaluate<IsIOS>())
		{
			failPlatformNotSupported();
		}

		unsafe
		{
			if (callback is null)
			{
				failCallbackArgumentNull();
			}

			if (mIOSAnimationCallback is { IsAllocated: true, Target: not null })
			{
				mIOSAnimationCallback.Free();

				mIOSAnimationCallback = default;
			}

			mIOSAnimationCallback = GCHandle.Alloc(callback, GCHandleType.Normal);

			var result = SDL_SetiOSAnimationCallback(window is not null ? window.Pointer : null, interval, &IOSAnimationCallback, unchecked((void*)GCHandle.ToIntPtr(mIOSAnimationCallback)));

			if (!result.AsClrBool)
			{
				mIOSAnimationCallback.Free();

				mIOSAnimationCallback = default;

				return false;
			}

			return true;
		}

		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TrySetIOSAnimationCallback)} is only supported on iOS.");

		[DoesNotReturn]
		static void failCallbackArgumentNull() => throw new ArgumentNullException(nameof(callback));
	}

	/// <summary>
	/// Tires to set the <see href="https://en.wikipedia.org/wiki/Nice_(Unix)">UNIX nice value</see> for a thread
	/// </summary>
	/// <param name="threadId">The UNIX thread id of the thread to change it's priority of</param>
	/// <param name="niceValue">The new <see href="https://en.wikipedia.org/wiki/Nice_(Unix)">UNIX nice value</see> to change the thread priority to</param>
	/// <returns><c><see langword="true"/></c> if the priority of the thread with id <paramref name="threadId"/> was succesfully changed to <paramref name="niceValue"/>; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
	/// <remarks>
	/// <para>
	/// This uses <c>setpriority()</c> if possible, and RealtimeKit if available.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Linux</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Linux</em></exception>
	[SupportedOSPlatform("Linux")]
	public static bool TrySetLinuxThreadPriority(long threadId, int niceValue)
	{
		if (!INativeImportCondition.Evaluate<IsLinux>())
		{
			failPlatformNotSupported();
		}

		return SDL_SetLinuxThreadPriority(threadId, niceValue);
		
		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TrySetIOSAnimationCallback)} is only supported on Linux.");
	}

	/// <summary>
	/// Tries to set the <see cref="ThreadPriority">priority</see> and scheduling policy for a thread
	/// </summary>
	/// <param name="threadId">The UNIX thread id of the thread to change it's priority and scheduling policy of</param>
	/// <param name="priority">The new <see cref="ThreadPriority">priority</see> (not a UNIX nice value!) to change the thread priority to</param>
	/// <param name="schedulingPolicy">The new scheduling policy to change the thread's scheduling policy to (<c>SCHED_FIFO</c>, <c>SCHED_RR</c>, <c>SCHED_OTHER</c>, etc...)</param>
	/// <returns>
	/// <c><see langword="true"/></c> if the <see cref="ThreadPriority">priority</see> and scheduling policy of the thread with id <paramref name="threadId"/> was succesfully changed to <paramref name="priority"/> and <paramref name="schedulingPolicy"/> respectively;
	/// otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </returns>
	/// <remarks>
	/// <para>
	/// This uses <c>setpriority()</c> if possible, and RealtimeKit if available.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Linux</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Linux</em></exception>
	[SupportedOSPlatform("Linux")]
	public static bool TrySetLinuxThreadPriority(long threadId, ThreadPriority priority, int schedulingPolicy)
	{
		if (!INativeImportCondition.Evaluate<IsLinux>())
		{
			failPlatformNotSupported();
		}

		return SDL_SetLinuxThreadPriorityAndPolicy(threadId, priority, schedulingPolicy);
		
		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TrySetIOSAnimationCallback)} is only supported on Linux.");
	}

	/// <summary>
	/// Shows an Android toast notification
	/// </summary>
	/// <param name="message">The text message to be shown</param>
	/// <param name="duration">
	/// The duration for which the toast notification shall appear on screen.
	/// Set this to <c>0</c> for a <em>short</em> duration, or to <c>1</c> for a <em>long</em> duration.
	/// </param>
	/// <param name="gravity">
	/// The positioning for where the toast notification shall appear on screen.
	/// Should be one of those values: <see href="https://developer.android.com/reference/android/view/Gravity"/>.
	/// You can also set this to <c>-1</c> if you don't have a preference.
	/// </param>
	/// <param name="xOffset">
	/// The horizontal offset in relation to the choosen <paramref name="gravity"/>.
	/// Must be only set if <paramref name="gravity"/> is ≥ <c>0</c>!
	/// </param>
	/// <param name="yOffset">
	/// The vertical offset in relation to the choosen <paramref name="gravity"/>.
	/// Must be only set if <paramref name="gravity"/> is ≥ <c>0</c>!
	/// </param>
	/// <returns><c><see langword="true"/></c> if the Android toast notification was succesfully shown; otherwise, <c><see langword="false"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>	
	/// <remarks>
	/// <para>
	/// Toasts are a sort of lightweight notification that are unique to Android. For more information see: <see href="https://developer.android.com/guide/topics/ui/notifiers/toasts"/>.
	/// </para>
	/// <para>
	/// Android toast notifications are shown in the UI thread.
	/// </para>
	/// <para>
	/// Note: This method is only available on <em>Android</em> and will throw a <see cref="PlatformNotSupportedException"/> on all other platforms!
	/// </para>
	/// </remarks>
	/// <exception cref="PlatformNotSupportedException">The executing platform is not <em>Android</em></exception>
	[SupportedOSPlatform("Android")]
	public static bool TryShowAndroidToast(string message, int duration = 0, int gravity = -1, int xOffset = default, int yOffset = default)
	{
		if (!INativeImportCondition.Evaluate<IsAndroid>())
		{
			failPlatformNotSupported();
		}

		unsafe
		{
			var messageUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(message);

			try
			{
				return SDL_ShowAndroidToast(messageUtf8, duration, gravity, xOffset, yOffset);
			}
			finally
			{
				Utf8StringMarshaller.Free(messageUtf8);
			}
		}
		
		[DoesNotReturn]
		static void failPlatformNotSupported() => throw new PlatformNotSupportedException($"{nameof(TryShowAndroidToast)} is only supported on Android.");
	}
}
