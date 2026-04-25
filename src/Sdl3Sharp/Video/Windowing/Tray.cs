using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#if SDL3_6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using System.Runtime.InteropServices.Marshalling;
#if SDL3_6_0_OR_GREATER
using unsafe SDL_TrayClickCallback = delegate* unmanaged[Cdecl]<void*, Sdl3Sharp.Video.Windowing.Tray.SDL_Tray*, Sdl3Sharp.Internal.Interop.CBool>;
#endif

namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents a tray icon added to the "system tray" or "notification area" of the user's desktop environment
/// </summary>
/// <remarks>
/// <para>
/// Not all platforms support tray icons.
/// On platforms that support this concept, the <see cref="Tray"/> type offers a way to add a conceptually equivalent of a tray icon to the "system tray" or "notification area",
/// that the user can interact with and that provides a way to show a tray menu with subsequent tray entries.
/// </para>
/// <para>
/// Many platforms advise not using a system tray unless persistence is a necessary feature. Avoid needlessly creating a tray icon, as the user may feel like it clutters their interface.
/// </para>
/// <para>
/// Using tray icons require the video subsystem.
/// </para>
/// </remarks>
public sealed partial class Tray : IDisposable
{
	private static readonly ConcurrentDictionary<IntPtr, WeakReference<Tray>> mKnownInstances = [];

	private unsafe SDL_Tray* mTray;
#if SDL3_6_0_OR_GREATER
	private GCHandle mSelfHandle;
#endif
	private TrayMenu? mMenu;
#if SDL3_6_0_OR_GREATER
	private unsafe readonly void* mUserdata;
	private unsafe readonly SDL_TrayClickCallback mLeftClickCallback, mRightClickCallback, mMiddleClickCallback;
#endif

	/// <summary>
	/// Creates a new tray with the specified icon and tooltip
	/// </summary>
	/// <param name="icon">The icon of the tray, or <c><see langword="null"/></c> to create a tray without an icon</param>
	/// <param name="tooltip">The tooltip of the tray, or <c><see langword="null"/></c> to create a tray without a tooltip</param>
	/// <remarks>
	/// <para>
	/// You can set the tray icon and tooltip after the tray is created by using the <see cref="SetIcon"/> and <see cref="SetTooltip"/> methods respectively.
	/// </para>
	/// <para>
	/// You can add tray entries to the tray after it's created by using the <see cref="Menu"/> property.
	/// </para>
	/// <para>
	/// This constructor should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Could not create the tray (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	public Tray(Surface? icon = default, string? tooltip = default)
#if SDL3_6_0_OR_GREATER
		: this(icon, tooltip, properties: null)
	{ }
#else
	{
		unsafe
		{
			var tooltipUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(tooltip);
			try
			{
				mTray = SDL_CreateTray(icon is not null ? icon.Pointer : null, tooltipUtf8);

				if (mTray is null)
				{
					failCouldNotCreateTray();
				}

				mMenu = null;

				mKnownInstances.AddOrUpdate(unchecked((IntPtr)mTray), addRef, updateRef, this);

				static WeakReference<Tray> addRef(IntPtr tray, Tray newTray) => new(newTray);

				static WeakReference<Tray> updateRef(IntPtr tray, WeakReference<Tray> previousTrayRef, Tray newTray)
				{
					if (previousTrayRef.TryGetTarget(out var previousTray))
					{
#pragma warning disable IDE0079
#pragma warning disable CA1816
						GC.SuppressFinalize(previousTray);
#pragma warning restore CA1816
#pragma warning restore IDE0079

						previousTray.Dispose(forget: false);
					}

					previousTrayRef.SetTarget(newTray);

					return previousTrayRef;
				}
			}
			finally
			{
				Utf8StringMarshaller.Free(tooltipUtf8);
			}
		}

		[DoesNotReturn]
		static void failCouldNotCreateTray() => throw new SdlException($"Could not create the {nameof(Tray)}");
	}
#endif

#if SDL3_6_0_OR_GREATER

	/// <summary>
	/// Creates a new tray with the specified icon and tooltip
	/// </summary>
	/// <param name="icon">The icon of the tray, or <c><see langword="null"/></c> to create a tray without an icon</param>
	/// <param name="tooltip">The tooltip of the tray, or <c><see langword="null"/></c> to create a tray without a tooltip</param>
	/// <param name="properties">Additional properties to use when creating the tray</param>
	/// <remarks>
	/// <para>
	/// You can set the tray icon and tooltip after the tray is created by using the <see cref="SetIcon"/> and <see cref="SetTooltip"/> methods respectively.
	/// </para>
	/// <para>
	/// You can add tray entries to the tray after it's created by using the <see cref="Menu"/> property.
	/// </para>
	/// <para>
	/// This constructor should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">Could not create the tray (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	[OverloadResolutionPriority(-1)]
	public Tray(Surface? icon = default, string? tooltip = default, Properties? properties = default)
	{
		unsafe
		{
			Properties propertiesUsed = [];
			Unsafe.SkipInit(out IntPtr? iconBackup);
			Unsafe.SkipInit(out string? tooltipBackup);
			Unsafe.SkipInit(out IntPtr? userdataBackup);
			Unsafe.SkipInit(out IntPtr? leftClickCallbackBackup);
			Unsafe.SkipInit(out IntPtr? rightClickCallbackBackup);
			Unsafe.SkipInit(out IntPtr? middleClickCallbackBackup);

			if (properties is null)
			{
				propertiesUsed = [];

				if (icon is { Pointer: var iconPtr })
				{
					propertiesUsed.TrySetPointerValue(PropertyNames.CreateIconPointer, unchecked((IntPtr)iconPtr));
				}

				if (tooltip is not null)
				{
					propertiesUsed.TrySetStringValue(PropertyNames.CreateTooltipString, tooltip);
				}

				mUserdata = null;
				mLeftClickCallback = null;
				mRightClickCallback = null;
				mMiddleClickCallback = null;
			}
			else
			{
				propertiesUsed = properties;

				if (icon is { Pointer: var iconPtr })
				{
					iconBackup = propertiesUsed.TryGetPointerValue(PropertyNames.CreateIconPointer, out var existingIconPtr)
						? existingIconPtr
						: null;

					propertiesUsed.TrySetPointerValue(PropertyNames.CreateIconPointer, unchecked((IntPtr)iconPtr));
				}

				if (tooltip is not null)
				{
					tooltipBackup = propertiesUsed.TryGetStringValue(PropertyNames.CreateTooltipString, out var existingTooltip)
						? existingTooltip
						: null;

					propertiesUsed.TrySetStringValue(PropertyNames.CreateTooltipString, tooltip);
				}

				if (propertiesUsed.TryGetPointerValue(PropertyNames.CreateUserdataPointer, out var userdataPtr))
				{
					userdataBackup = userdataPtr;
					mUserdata = unchecked((void*)userdataPtr);
				}
				else
				{
					userdataBackup = null;
					mUserdata = null;
				}

				if (propertiesUsed.TryGetPointerValue(PropertyNames.CreateLeftClickCallbackPointer, out var leftClickCallbackPtr))
				{
					leftClickCallbackBackup = leftClickCallbackPtr;
					mLeftClickCallback = unchecked((SDL_TrayClickCallback)leftClickCallbackPtr);
				}
				else
				{
					leftClickCallbackBackup = null;
					mLeftClickCallback = null;
				}

				if (propertiesUsed.TryGetPointerValue(PropertyNames.CreateRightClickCallbackPointer, out var rightClickCallbackPtr))
				{
					rightClickCallbackBackup = rightClickCallbackPtr;
					mRightClickCallback = unchecked((SDL_TrayClickCallback)rightClickCallbackPtr);
				}
				else
				{
					rightClickCallbackBackup = null;
					mRightClickCallback = null;
				}

				if (propertiesUsed.TryGetPointerValue(PropertyNames.CreateMiddleClickCallbackPointer, out var middleClickCallbackPtr))
				{
					middleClickCallbackBackup = middleClickCallbackPtr;
					mMiddleClickCallback = unchecked((SDL_TrayClickCallback)middleClickCallbackPtr);
				}
				else
				{
					middleClickCallbackBackup = null;
					mMiddleClickCallback = null;
				}
			}

			try
			{
				mSelfHandle = GCHandle.Alloc(this, GCHandleType.Weak);

				propertiesUsed.TrySetPointerValue(PropertyNames.CreateUserdataPointer, GCHandle.ToIntPtr(mSelfHandle));
				propertiesUsed.TrySetPointerValue(PropertyNames.CreateLeftClickCallbackPointer, unchecked((IntPtr)(SDL_TrayClickCallback)(&TrayLeftClickCallback)));
				propertiesUsed.TrySetPointerValue(PropertyNames.CreateRightClickCallbackPointer, unchecked((IntPtr)(SDL_TrayClickCallback)(&TrayRightClickCallback)));
				propertiesUsed.TrySetPointerValue(PropertyNames.CreateMiddleClickCallbackPointer, unchecked((IntPtr)(SDL_TrayClickCallback)(&TrayMiddleClickCallback)));

				mTray = SDL_CreateTrayWithProperties(propertiesUsed.Id);

				if (mTray is null)
				{
					mSelfHandle.Free();
					mSelfHandle = default;

					failCouldNotCreateTray();
				}

				mMenu = null;

				mKnownInstances.AddOrUpdate(unchecked((IntPtr)mTray), addRef, updateRef, this);

				static WeakReference<Tray> addRef(IntPtr tray, Tray newTray) => new(newTray);

				static WeakReference<Tray> updateRef(IntPtr tray, WeakReference<Tray> previousTrayRef, Tray newTray)
				{
					if (previousTrayRef.TryGetTarget(out var previousTray))
					{
#pragma warning disable IDE0079
#pragma warning disable CA1816
						GC.SuppressFinalize(previousTray);
#pragma warning restore CA1816
#pragma warning restore IDE0079

						previousTray.Dispose(forget: false);
					}

					previousTrayRef.SetTarget(newTray);

					return previousTrayRef;
				}
			}
			finally
			{
				if (properties is null)
				{
					// propertiesUsed was just a temporary instance we created for this call, so we need to dispose it now

					propertiesUsed.Dispose();
				}
				else
				{
					// we restored the original properties values from the given properties instance

					if (icon is not null)
					{
						if (iconBackup is IntPtr iconPtr)
						{
							propertiesUsed.TrySetPointerValue(PropertyNames.CreateIconPointer, iconPtr);

						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateIconPointer);
						}
					}

					if (tooltip is not null)
					{
						if (tooltipBackup is not null)
						{
							propertiesUsed.TrySetStringValue(PropertyNames.CreateTooltipString, tooltipBackup);
						}
						else
						{
							propertiesUsed.TryRemove(PropertyNames.CreateTooltipString);
						}
					}

					if (userdataBackup is IntPtr userdataPtr)
					{
						propertiesUsed.TrySetPointerValue(PropertyNames.CreateUserdataPointer, userdataPtr);
					}
					else
					{
						propertiesUsed.TryRemove(PropertyNames.CreateUserdataPointer);
					}

					if (leftClickCallbackBackup is IntPtr leftClickCallbackPtr)
					{
						propertiesUsed.TrySetPointerValue(PropertyNames.CreateLeftClickCallbackPointer, leftClickCallbackPtr);
					}
					else
					{
						propertiesUsed.TryRemove(PropertyNames.CreateLeftClickCallbackPointer);
					}

					if (rightClickCallbackBackup is IntPtr rightClickCallbackPtr)
					{
						propertiesUsed.TrySetPointerValue(PropertyNames.CreateRightClickCallbackPointer, rightClickCallbackPtr);
					}
					else
					{
						propertiesUsed.TryRemove(PropertyNames.CreateRightClickCallbackPointer);
					}

					if (middleClickCallbackBackup is IntPtr middleClickCallbackPtr)
					{
						propertiesUsed.TrySetPointerValue(PropertyNames.CreateMiddleClickCallbackPointer, middleClickCallbackPtr);
					}
					else
					{
						propertiesUsed.TryRemove(PropertyNames.CreateMiddleClickCallbackPointer);
					}
				}
			}
		}

		[DoesNotReturn]
		static void failCouldNotCreateTray() => throw new SdlException($"Could not create the {nameof(Tray)}");
	}

#endif

	/// <inheritdoc/>
	~Tray() => Dispose(forget: true); // Normally, we would require the finalizer to be run on the main thread.
									  // Sdl3Sharp.Threading.Thread.TryRunOnMainThread(() => Dispose(forget: true)) could be a solution to this,
									  // but it risk deadlocks, which are way worse than memory leaks in the context of a finalizer.
									  // So, we accept the current solution with its caveats, especially memory leaks,
									  // as a best opportunistic solution for careless users.

	/// <summary>
	/// Gets the tray menu associated with the tray
	/// </summary>
	/// <value>
	/// The tray menu associated with the tray
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public TrayMenu Menu
	{
		get
		{
			unsafe
			{
				if (mMenu is null)
				{
					var menu = SDL_GetTrayMenu(mTray);

					if (menu is null)
					{
						menu = SDL_CreateTrayMenu(mTray);
					}

					if (!TrayMenu.TryGetOrCreate(menu, out mMenu))
					{
						failCouldNotCreateMenu();
					}
				}

				return mMenu;
			}

			[DoesNotReturn]
			static void failCouldNotCreateMenu() => throw new SdlException($"Could not create the {nameof(TrayMenu)} for this {nameof(Tray)}");
		}
	}

	internal unsafe SDL_Tray* Pointer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mTray; }

#if SDL3_6_0_OR_GREATER

	/// <summary>
	/// An event that is raised when the tray icon is left-clicked by the user
	/// </summary>
	/// <remarks>
	/// <para>
	/// You can set the <see cref="TrayClickedEventArgs.ShowMenu"/> property of the <see cref="TrayClickedEventArgs"/> passed to the event handler to control whether the tray menu should be shown after this event is handled.
	/// </para>
	/// </remarks>
	public event TrayClickedEventHandler? LeftClicked;

	/// <summary>
	/// An event that is raised when the tray icon is middle-clicked by the user
	/// </summary>
	/// <remarks>
	/// <para>
	/// You can set the <see cref="TrayClickedEventArgs.ShowMenu"/> property of the <see cref="TrayClickedEventArgs"/> passed to the event handler to control whether the tray menu should be shown after this event is handled.
	/// </para>
	/// </remarks>
	public event TrayClickedEventHandler? MiddleClicked;

	/// <summary>
	/// An event that is raised when the tray icon is right-clicked by the user
	/// </summary>
	/// <remarks>
	/// <para>
	/// You can set the <see cref="TrayClickedEventArgs.ShowMenu"/> property of the <see cref="TrayClickedEventArgs"/> passed to the event handler to control whether the tray menu should be shown after this event is handled.
	/// </para>
	/// </remarks>
	public event TrayClickedEventHandler? RightClicked;

#endif

	/// <summary>
	/// Dispose the tray
	/// </summary>
	/// <remarks>
	/// <para>
	/// Calling this method will make the tray icon immediately disappear for the user.
	/// </para>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(forget: true);
	}

	private void Dispose(bool forget)
	{
		unsafe
		{
#if SDL3_6_0_OR_GREATER

			MiddleClicked = null;
			RightClicked = null;
			LeftClicked = null;

			if (mSelfHandle.IsAllocated)
			{
				mSelfHandle.Free();
				mSelfHandle = default;
			}

#endif
			mMenu?.DisposeInternal();
			mMenu = null;

			if (mTray is not null)
			{
				if (forget)
				{
					mKnownInstances.TryRemove(unchecked((IntPtr)mTray), out _);
				}

				SDL_DestroyTray(mTray); // SDL_DestroyTray will also recursively destroy all subsequent submenus and entries, so we don't need to worry about that
			}

			mTray = null;
		}
	}

#if SDL3_6_0_OR_GREATER

	internal void OnLeftClicked(TrayClickedEventArgs args)
	{
		LeftClicked?.Invoke(this, args);
	}

	internal void OnMiddleClicked(TrayClickedEventArgs args)
	{
		MiddleClicked?.Invoke(this, args);
	}

	internal void OnRightClicked(TrayClickedEventArgs args)
	{
		RightClicked?.Invoke(this, args);
	}

#endif

	/// <summary>
	/// Sets the icon of the tray
	/// </summary>
	/// <param name="icon">The new icon of the tray, or <c><see langword="null"/></c> to remove the current icon</param>
	/// <remarks>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public void SetIcon(Surface? icon)
	{
		unsafe
		{
			SDL_SetTrayIcon(mTray, icon is not null ? icon.Pointer : null);
		}
	}

	/// <summary>
	/// Sets the tooltip of the tray
	/// </summary>
	/// <param name="tooltip">The new tooltip of the tray, or <c><see langword="null"/></c> to remove the current tooltip</param>
	/// <remarks>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public void SetTooltip(string? tooltip)
	{
		unsafe
		{
			var tooltipUtf8 = Utf8StringMarshaller.ConvertToUnmanaged(tooltip);
			try
			{
				SDL_SetTrayTooltip(mTray, tooltipUtf8);
			}
			finally
			{
				Utf8StringMarshaller.Free(tooltipUtf8);
			}
		}
	}

	internal unsafe static bool TryGet(SDL_Tray* tray, [NotNullWhen(true)] out Tray? result)
	{
		if (tray is null
			|| !mKnownInstances.TryGetValue(unchecked((IntPtr)tray), out var trayRef)
			|| !trayRef.TryGetTarget(out result))
		{
			result = null;
			return false;
		}

		return true;
	}

	/// <summary>
	/// Updates all trays and raises pending events for them
	/// </summary>
	/// <remarks>
	/// <para>
	/// Normally, you don't need to call this method yourself, as SDL will automatically call it while processing the event loop.
	/// However, you'll need to call this method yourself if you don't let SDL handle the event loop for you and you intend to handle event yourself.
	/// </para>
	/// <para>
	/// This method should only be called from the main thread.
	/// </para>
	/// </remarks>
	public static void UpdateAll() => SDL_UpdateTrays();
}
