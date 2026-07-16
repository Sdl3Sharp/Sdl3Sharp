using Sdl3Sharp.Internal;
using Sdl3Sharp.IO;
using Sdl3Sharp.Utilities;
using Sdl3Sharp.Video.Windowing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;

namespace Sdl3Sharp;

/// <summary>
/// Represents the lifetime of SDL
/// </summary>
/// <remarks>
/// <para>
/// You must create an instance of <see cref="Sdl"/> in order to use most of the API.
/// Creating the very first instance of <see cref="Sdl"/> initializes SDL, while <see cref="Dispose()">disposing</see> the very last instance shuts down SDL.
/// </para>
/// <para>
/// You can create multiple instances of <see cref="Sdl"/> at the same time, only the very first instance initializes SDL, and only the very last instance to be <see cref="Dispose()">disposed</see> shuts it down.
/// Other instances can be used to initialize additional subsystems, and will automatically deinitialize them when they are disposed, if no other instance of <see cref="Sdl"/> is using them anymore.
/// </para>
/// </remarks>
public sealed partial class Sdl : IDisposable
{
	private static readonly ConcurrentDictionary<WeakReference<Sdl>, byte> mKnownInstances = new(WeakReferenceEqualityComparer<Sdl>.Instance);
	private static uint mInstanceCounter = 0;

	private readonly SubSystems mSubSystems;
	private volatile bool mAlive;

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveInlining)]
	private static SubSystems InvokeBuildAction(BuildAction? buildAction)
	{
		var subSystems = SubSystems.None;

		buildAction?.Invoke(new(ref subSystems));

		return subSystems;
	}

	/// <summary>
	/// Creates a new <see cref="Sdl"/> instance and initializes SDL or additional subsystems
	/// </summary>
	/// <param name="buildAction">A <see cref="BuildAction"/> that is performed right before an <see cref="Sdl"/> instance is created. Use the provided <see cref="Builder"/> argument to perfom some preliminaries before an <see cref="Sdl"/> instance is created.</param>
	/// <remarks>
	/// <para>
	/// Creating the very first instance of <see cref="Sdl"/> initializes SDL, while <see cref="Dispose()">disposing</see> the very last instance shuts down SDL.
	/// </para>
	/// <para>
	/// You can create multiple instances of <see cref="Sdl"/> at the same time, only the very first instance initializes SDL, and only the very last instance shuts it down.
	/// Other instances can be used to initialize additional subsystems, and will automatically deinitialize them when they are disposed, if no other instance of <see cref="Sdl"/> is using them anymore.
	/// </para>
	/// <para>
	/// The file I/O (for example: <see cref="FileStream"/>) and threading (<see cref="SDL_CreateThread"/>) subsystems are initialized by default.
	/// Message boxes (<see cref="MessageBox.TryShowSimple(MessageBoxFlags, string, string, Window?)"/> and <see cref="MessageBox.TryShow(out int)"/>) also attempt to work without initializing the <see cref="SubSystems.Video">video sub system</see>,
	/// in hopes of being useful in showing an error dialog even before SDL initializes correclty.
	/// Logging (such as <see cref="Log.Info(string)"/>) works without initialization, too.
	/// </para>
	/// <para>
	/// You must specifically initialize other subsystems (by using <see cref="Builder.InitializeSubSystems(SubSystems)"/>), if you want to use them in your application.
	/// You can create new instances of <see cref="Sdl"/> to initialize additional subsystems while SDL is already initialized by another instance of <see cref="Sdl"/>.
	/// </para>
	/// <para>
	/// Consider reporting some basic metadata about your application inside the <paramref name="buildAction"/>, by using <see cref="Builder.TrySetMetadata(string, string?)"/> or <see cref="Builder.TrySetMetadata(string?, string?, string?)"/>.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="Sdl(SubSystems)"/>
	public Sdl(BuildAction? buildAction = default)
		: this(InvokeBuildAction(buildAction))
	{ }

	/// <summary>
	/// Creates a new <see cref="Sdl"/> instance and initializes SDL or additional subsystems
	/// </summary>
	/// <param name="subSystems">The <see cref="SubSystems">sub systems</see> to be initialized with the <see cref="Sdl"/> instance</param>
	/// <remarks>
	/// <para>
	/// Creating the very first instance of <see cref="Sdl"/> initializes SDL, while <see cref="Dispose()">disposing</see> the very last instance shuts down SDL.
	/// </para>
	/// <para>
	/// You can create multiple instances of <see cref="Sdl"/> at the same time, only the very first instance initializes SDL, and only the very last instance shuts it down.
	/// Other instances can be used to initialize additional subsystems, and will automatically deinitialize them when they are disposed, if no other instance of <see cref="Sdl"/> is using them anymore.
	/// </para>
	/// <para>
	/// The file I/O (for example: <see cref="FileStream"/>) and threading (<see cref="SDL_CreateThread"/>) subsystems are initialized by default.
	/// Message boxes (<see cref="MessageBox.TryShowSimple(MessageBoxFlags, string, string, Window?)"/> and <see cref="MessageBox.TryShow(out int)"/>) also attempt to work without initializing the <see cref="SubSystems.Video">video sub system</see>,
	/// in hopes of being useful in showing an error dialog even before SDL initializes correclty.
	/// Logging (such as <see cref="Log.Info(string)"/>) works without initialization, too.
	/// </para>
	/// <para>
	/// You must specifically initialize other subsystems (by using <see cref="Builder.InitializeSubSystems(SubSystems)"/>), if you want to use them in your application.
	/// You can create new instances of <see cref="Sdl"/> to initialize additional subsystems while SDL is already initialized by another instance of <see cref="Sdl"/>.
	/// </para>
	/// <para>
	/// This constructor is provided for convenience, when all you want to do is initialize additional subsystems, and you don't need to perform any other preliminaries before creating the new <see cref="Sdl"/> instance.
	/// If you want to create the very first instance of <see cref="Sdl"/> in order to initialize SDL, it is highly recommended to use the <see cref="Sdl(BuildAction?)"/> constructor instead,
	/// and report some basic metadata about your application by using the <see cref="Builder.TrySetMetadata(string, string?)"/> or <see cref="Builder.TrySetMetadata(string?, string?, string?)"/> methods.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Couldn't initialize SDL (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public Sdl(SubSystems subSystems)
	{
		var instanceCounter = Interlocked.Increment(ref mInstanceCounter);
		try
		{
			if (instanceCounter is not > 1)
			{
				// we're the very first instance of Sdl, so we're responsible for initializing SDL
				SdlErrorHelper.ThrowIfFailed(SDL_Init(subSystems));
			}
			else
			{
				// SDL's already initialized, so we just need to initialize the requested subsystems
				SdlErrorHelper.ThrowIfFailed(SDL_InitSubSystem(subSystems));
			}

			mKnownInstances.TryAdd(new(this), default); // this always succeeds, since we're a very new instance
		}
		catch
		{
			Interlocked.Decrement(ref mInstanceCounter); // decrement the instance counter, since we threw an exception and don't actually have a new valid instance
			throw;
		}

		mSubSystems = subSystems;
	}

	/// <inheritdoc/>
	/// <inheritdoc cref="Dispose(bool)"/>
	~Sdl() => Dispose(disposing: false);
	
	/// <summary>
	/// Gets the revision string of the currently loaded native SDL library
	/// </summary>
	/// <value>
	/// The revision string of the currently loaded native SDL library
	/// </value>
	/// <remarks>
	/// <para>
	/// The revision is arbitrary string (a hash value) uniquely identifying the exact revision of the SDL library in use, and is only useful in comparing against other revisions. It is <em>NOT</em> an incrementing number.
	/// </para>
	/// <para>
	/// If SDL wasn't built from a git repository with the appropriate tools, this will return an empty string.
	/// </para>
	/// <para>
	/// You shouldn't use this value for anything but logging it for debugging purposes. The string is not intended to be reliable in any way.
	/// </para>
	/// </remarks>
	public static string? Revision
	{
		get
		{
			unsafe
			{
				return Utf8StringMarshaller.ConvertToManaged(SDL_GetRevision());
			}
		}
	}

	/// <summary>
	/// Gets the version of the currently loaded native SDL library
	/// </summary>
	/// <value>
	/// The version of the currently loaded native SDL library
	/// </value>
	public static Version Version => SDL_GetVersion();

	/// <summary>
	/// Disposes the <see cref="Sdl"/> instance, deinitializes subsystems that are no longer in use, and potentially shuts down SDL if this is the very last instance of <see cref="Sdl"/> alive
	/// </summary>
	/// <inheritdoc cref="Dispose(bool)"/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(disposing: true);
	}

	/// <exception cref="AggregateException">One or more registered <see cref="IDisposeReceiver"/> threw an exception during the call to their <see cref="IDisposeReceiver.DisposeFromSdl(Sdl)"/></exception>
	private void Dispose(bool disposing)
	{
#pragma warning disable CS0420 // it's fine to cast away the volatile modifier of mAlive here, since we're using an Interlocked operation on a reference to it anyway
		if (!Unsafe.BitCast<byte, bool>(Interlocked.Exchange(ref Unsafe.As<bool, byte>(ref mAlive), Unsafe.BitCast<bool, byte>(false)))) // definitely unset the 'mAlive' flag, and at the same time check if it was already unset
#pragma warning restore CS0420
		{
			// the 'mAlive' flag was already unset, so we don't need to do anything here
			return;
		}

		// if we're here, this means we're responsible for deinitializing SDL or some of the currently initialized subsystems

		if (Interlocked.Decrement(ref mInstanceCounter) is not > 0)
		{
			// we're the very last instance of Sdl, so we're responsible for deinitializing SDL
			// we need to inform all registered <see cref="IDisposeReceiver"/>s that SDL is being deinitialized before we actually call SDL_Quit(), so that they can safely clean up their resources

			try
			{
				var exceptions = new Queue<Exception>();

				foreach ((var reference, _) in mRegisteredDisposeReceivers)
				{
					if (reference?.TryGetTarget(out var disposeReceiver) is true)
					{
						try
						{
							disposeReceiver.DisposeFromSdl(this);
						}
						catch (Exception exception)
						{
							exceptions.Enqueue(exception);
						}
					}
				}

				if (exceptions.Count is > 0
					&& !disposing // DO NOT throw an exception on the finalizer path!
				)
				{
					[DoesNotReturn]
					static void failDisposeReceiversAggregateException(Queue<Exception> exceptions) => throw new AggregateException(exceptions);

					failDisposeReceiversAggregateException(exceptions);
				}
			}
			finally
			{
				mRegisteredDisposeReceivers.Clear();

				SDL_Quit(); // now we can safely deinitialize SDL
			}
		}
		else
		{
			// there are still other instances of Sdl alive, so we just need to deinitialize the subsystems that this instance was initialized with

			SDL_QuitSubSystem(mSubSystems);
		}

		mKnownInstances.TryRemove(new(this), out _); // this could fail, especially if we're on the finalizer path, but we don't care about that, because in any case, the instance at hand is no longer referenced by the registry
	}

	// We need to this because running an App will inevitably call SDL_Quit and therefore invalidate all Sdl instances.
	// Therefore, App.Run will call this method to safely dispose all Sdl instances right before the call to SDL_Quit.
	// This is a workaround for how SDL, especially SDL_EnterAppMainCallbacks, is implemented, and is not ideal, but it works.
	internal static void DisposeAll()
	{
		// we're copying the registry before iterating over the copy, because Dispose calls on the instances will mutate the registry
		ReadOnlySpan<WeakReference<Sdl>> knownInstances = [..mKnownInstances.Keys];

		// this is also a good point to clear the registry, and we even get rid of any stale references (not that they would cause a problem in any case)
		mKnownInstances.Clear();

		foreach (var sdlRef in knownInstances)
		{
			if (sdlRef.TryGetTarget(out var sdl))
			{
				sdl.Dispose();
			}
		}
	}

	/// <summary>
	/// Gets the currently initialized <see cref="SubSystems"/>
	/// </summary>
	/// <param name="subSystems">The <see cref="SubSystems"/> to check for, or <see cref="SubSystems.None"/> to check all subsystems</param>
	/// <returns>
	/// A <see cref="SubSystems"/> mask with the currently initialized <see cref="SubSystems"/> from the ones specified in the given <paramref name="subSystems"/>, if <paramref name="subSystems"/> is not <see cref="SubSystems.None"/>;
	/// otherwise, a <see cref="SubSystems"/> representing all of the currently initialized <see cref="SubSystems"/>
	/// </returns>
	/// <remarks>
	/// <para>
	/// This method retrieves the initialization state of subsystems across all instances of <see cref="Sdl"/>.
	/// </para>
	/// <para>
	/// If you just want to check the initialization state <see cref="SubSystems"/>, you can use the <see cref="SubSystemsExtensions.get_IsInitialized(SubSystems)"/> extension property.
	/// </para>
	/// </remarks>
	public static SubSystems GetInitializedSubSystems(SubSystems subSystems = SubSystems.None) => SDL_WasInit(subSystems);

	/// <summary>
	/// Get metadata about your app
	/// </summary>
	/// <param name="name">The name of the metadata</param>
	/// <returns>The current value of the metadata, if it's set; otherwise, the default value for the metadata, or <c><see langword="null"/></c>, if it has no default value</returns>
	/// <remarks>
	/// <para>
	/// This returns metadata previously set using one of the <c>*SetMetadata</c> methods on the <see cref="Builder"/> that was used to create the current <see cref="Sdl"/> instance.
	/// </para>
	/// <para>
	/// See <see cref="Metadata"/> for a overview over the available metadata properties and their meanings.
	/// </para>
	/// </remarks>
#pragma warning disable IDE0079
#pragma warning disable CA1822 // this is intentionally an instance method
	public string? GetMetadata(string name)
#pragma warning restore CA1822
#pragma warning restore IDE0079
	{
		unsafe
		{
			var nameUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(name);

			try
			{
				return Utf8StringMarshaller.ConvertToManaged(SDL_GetAppMetadataProperty(nameUtf8));
			}
			finally
			{
				Utf8StringMarshaller.Free(nameUtf8);
			}
		}
	}
}
