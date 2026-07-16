using Sdl3Sharp.Events;
using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using unsafe SDL_AppEvent_func = delegate* unmanaged[Cdecl]<void*, Sdl3Sharp.Events.Event*, Sdl3Sharp.AppResult>;
using unsafe SDL_AppInit_func = delegate* unmanaged[Cdecl]<void**, int, byte**, Sdl3Sharp.AppResult>;
using unsafe SDL_AppIterate_func = delegate* unmanaged[Cdecl]<void*, Sdl3Sharp.AppResult>;
using unsafe SDL_AppQuit_func = delegate* unmanaged[Cdecl]<void*, Sdl3Sharp.AppResult, void>;
using unsafe SDL_main_func = delegate* unmanaged[Cdecl]<int, byte**, int>;

namespace Sdl3Sharp;

partial class App
{
	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private unsafe static int Main(int argc, byte** argv)
	{
		// enter the SDL main app loop with appinit and the other App* methods as callbacks.
		return SDL_EnterAppMainCallbacks(argc, argv, &Appinit, &Appiter, &Appevent, &Appquit);
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private unsafe static AppResult Appinit(void** appstate, int argc, byte** argv)
	{
		if (mRunningApp is not null and var app)
		{
			try
			{
				*appstate = null; // not needed, but it's good practice to clear it out, just in case

				// converting back the argvs into a string array
				Unsafe.SkipInit(out string[] args);
				if (argv is not null)
				{
					args = GC.AllocateUninitializedArray<string>(argc);

					foreach (ref var arg in args.AsSpan())
					{
						arg = Utf8StringMarshaller.ConvertToManaged(*argv++);
					}
				}
				else
				{
					args = [];
				}

				return app.OnInitialize(args);
			}
			catch (Exception exception)
			{
				return app.OnUnhandledException(ExceptionDispatchInfo.Capture(exception), AppUnhandledExceptionSource.OnInitialize);
			}
		}

		return AppResult.Failure;
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private unsafe static AppResult Appiter(void* appstate)
	{
		if (mRunningApp is not null and var app)
		{
			try
			{
				return app.OnIterate();
			}
			catch (Exception exception)
			{
				return app.OnUnhandledException(ExceptionDispatchInfo.Capture(exception), AppUnhandledExceptionSource.OnIterate);
			}
		}

		return AppResult.Failure;
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private unsafe static AppResult Appevent(void* appstate, Event* @event)
	{
		if (mRunningApp is not null and var app)
		{
			try
			{
				return app.OnEvent(ref Unsafe.AsRef<Event>(@event));
			}
			catch (Exception exception)
			{
				return app.OnUnhandledException(ExceptionDispatchInfo.Capture(exception), AppUnhandledExceptionSource.OnEvent);
			}
		}

		return AppResult.Failure;
	}

	[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
	private unsafe static void Appquit(void* appstate, AppResult result)
	{
		try
		{
			if (mRunningApp is not null and var app)
			{
				app.mOnQuitCalled = true; // set this to true before calling OnQuit, so that if OnQuit throws an exception, we don't try to call it again
				app.OnQuit(result);
			}
		}
		finally
		{
			// Sadly, we have to dispose of all alive Sdl instances here.
			// That's because SDL_EnterAppMainCallbacks essentially silently does the same,
			// and not only do we want to beat SDL to it and deinitialize SDL safely first,
			// but also because we want the managed side to stay in sync with the unmanaged side and reflect that SDL has been deinitialized (either way)
			// by disposing of any alive Sdl instances.
			Sdl.DisposeAll();
		}
	}

	/// <summary>
	/// An entry point for SDL's use in <see href="https://wiki.libsdl.org/SDL3/SDL_MAIN_USE_CALLBACKS">SDL_MAIN_USE_CALLBACKS</see>
	/// </summary>
	/// <param name="argc">standard Unix main argc</param>
	/// <param name="argv">standard Unix main argv</param>
	/// <param name="appinit">the application's <see href="https://wiki.libsdl.org/SDL3/SDL_AppInit">SDL_AppInit</see> function</param>
	/// <param name="appiter">the application's <see href="https://wiki.libsdl.org/SDL3/SDL_AppIterate">SDL_AppIterate</see> function</param>
	/// <param name="appevent">the application's <see href="https://wiki.libsdl.org/SDL3/SDL_AppEvent">SDL_AppEvent</see> function</param>
	/// <param name="appquit">the application's <see href="https://wiki.libsdl.org/SDL3/SDL_AppQuit">SDL_AppQuit</see> function</param>
	/// <returns>Returns standard Unix main return value</returns>
	/// <remarks>
	/// Generally, you should not call this function directly. This only exists to hand off work into SDL as soon as possible, where it has a lot more control and functionality available, and make the inline code in <see href="https://wiki.libsdl.org/SDL3/SDL_main">SDL_main</see>.h as small as possible.
	///
	/// Not all platforms use this, it's actual use is hidden in a magic header-only library, and you should not call this directly unless you <em>really</em> know what you're doing.
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_EnterAppMainCallbacks">SDL_EnterAppMainCallbacks</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial int SDL_EnterAppMainCallbacks(int argc, byte** argv, SDL_AppInit_func appinit, SDL_AppIterate_func appiter, SDL_AppEvent_func appevent, SDL_AppQuit_func appquit);

	/// <summary>
	/// Initializes and launches an SDL application, by doing platform-specific initialization before calling your mainFunction and cleanups after it returns, if that is needed for a specific platform, otherwise it just calls mainFunction
	/// </summary>
	/// <param name="argc">the argc parameter from the application's main() function, or 0 if the platform's main-equivalent has no argc</param>
	/// <param name="argv">the argv parameter from the application's main() function, or NULL if the platform's main-equivalent has no argv</param>
	/// <param name="mainFunction">your SDL app's C-style main(). NOT the function you're calling this from! Its name doesn't matter; it doesn't literally have to be <c>main</c>.</param>
	/// <param name="reserved">should be NULL (reserved for future use, will probably be platform-specific then)</param>
	/// <returns>Returns the return value from mainFunction: 0 on success, otherwise failure; <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() might have more information on the failure</returns>
	/// <remarks>
	/// You can use this if you want to use your own main() implementation without using <see href="https://wiki.libsdl.org/SDL3/SDL_main">SDL_main</see> (like when using <see href="https://wiki.libsdl.org/SDL3/SDL_MAIN_HANDLED">SDL_MAIN_HANDLED</see>). When using this, you do <em>not</em> need <see href="https://wiki.libsdl.org/SDL3/SDL_SetMainReady">SDL_SetMainReady</see>().
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_RunApp">SDL_RunApp</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial int SDL_RunApp(int argc, byte** argv, SDL_main_func mainFunction, void* reserved);
}
