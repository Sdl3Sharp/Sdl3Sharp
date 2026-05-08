// DISCLAIMER: This source file was created 100% by AI (GitHub Copilot using GPT-5.4).

using System;
using Sdl3Sharp.Threading;
using Sdl3Sharp.Video.Windowing;
using Xunit.Sdk;

namespace Sdl3Sharp.Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class TrayLifetimeTestCollectionDefinition
{
	public const string Name = nameof(TrayLifetimeTestCollectionDefinition);
}

[Collection(TrayLifetimeTestCollectionDefinition.Name)]
public sealed class TrayLifetimeTests
{
	[Fact]
	public void MenuProperty_ReturnsStableMenuInstance_BoundToTheOwningTray()
	{
		using var scope = TrayTestScope.Create();

		var firstMenu = scope.Tray.Menu;
		var secondMenu = scope.Tray.Menu;

		Assert.Same(firstMenu, secondMenu);
		Assert.Same(scope.Tray, firstMenu.ParentTray);
		Assert.Null(firstMenu.ParentTrayEntry);
		Assert.Empty(firstMenu);
	}

	[Fact]
	public void ButtonEntry_CanRoundTripBetweenManagedAndNativeStates_WithoutLosingIdentityOrState()
	{
		using var scope = TrayTestScope.Create();

		var menu = scope.Tray.Menu;
		var entry = new ButtonTrayEntry("Open", isEnabled: true);

		menu.Add(entry);

		Assert.Same(entry, menu[0]);
		Assert.Same(menu, entry.Parent);
		Assert.Contains(entry, menu);
		Assert.Equal(0, menu.IndexOf(entry));

		entry.Label = "Open Later";
		entry.IsEnabled = false;

		Assert.True(menu.Remove(entry));
		Assert.Empty(menu);
		Assert.DoesNotContain(entry, menu);
		Assert.Null(entry.Parent);
		Assert.Equal("Open Later", entry.Label);
		Assert.False(entry.IsEnabled);

		var clickedCount = 0;
		entry.Clicked += _ => clickedCount++;
		entry.Click();

		Assert.Equal(1, clickedCount);

		menu.Add(entry);

		Assert.Same(entry, menu[0]);
		Assert.Same(menu, entry.Parent);
		Assert.Equal("Open Later", entry.Label);
		Assert.False(entry.IsEnabled);
	}

	[Fact]
	public void SubmenuTree_CanRoundTripBetweenManagedAndNativeStates_WithoutReplacingInstances()
	{
		using var scope = TrayTestScope.Create();

		var topMenu = scope.Tray.Menu;
		var submenuEntry = new SubmenuTrayEntry("Options", isEnabled: false);
		var submenuMenu = submenuEntry.Menu;
		var childCheckbox = new CheckboxTrayEntry("Enabled", isChecked: true, isEnabled: false);
		var childButton = new ButtonTrayEntry("Reset");

		submenuMenu.Add(childCheckbox);
		submenuMenu.Add(childButton);

		Assert.Same(submenuMenu, submenuEntry.Menu);
		Assert.Same(submenuEntry, submenuMenu.ParentTrayEntry);
		Assert.Null(submenuMenu.ParentTray);

		topMenu.Add(submenuEntry);

		Assert.Same(submenuEntry, topMenu[0]);
		Assert.Same(topMenu, submenuEntry.Parent);
		Assert.Same(submenuMenu, submenuEntry.Menu);
		Assert.Same(submenuEntry, submenuMenu.ParentTrayEntry);
		Assert.Same(childCheckbox, submenuMenu[0]);
		Assert.Same(childButton, submenuMenu[1]);

		childCheckbox.IsChecked = false;
		childButton.Label = "Reset Everything";

		Assert.True(topMenu.Remove(submenuEntry));
		Assert.Null(submenuEntry.Parent);
		Assert.Same(submenuMenu, submenuEntry.Menu);
		Assert.Same(submenuEntry, submenuMenu.ParentTrayEntry);
		Assert.Same(childCheckbox, submenuMenu[0]);
		Assert.Same(childButton, submenuMenu[1]);
		Assert.False(childCheckbox.IsChecked);
		Assert.False(childCheckbox.IsEnabled);
		Assert.Equal("Reset Everything", childButton.Label);

		topMenu.Add(submenuEntry);

		Assert.Same(submenuEntry, topMenu[0]);
		Assert.Same(submenuMenu, submenuEntry.Menu);
		Assert.Same(childCheckbox, submenuEntry.Menu[0]);
		Assert.Same(childButton, submenuEntry.Menu[1]);
	}

	[Fact]
	public void Clear_DetachesNativeEntries_AndPreservesTheirLatestManagedState()
	{
		using var scope = TrayTestScope.Create();

		var menu = scope.Tray.Menu;
		var button = new ButtonTrayEntry("Play");
		var checkbox = new CheckboxTrayEntry("Music", isChecked: true);

		menu.Add(button);
		menu.Add(checkbox);

		button.Label = "Play Now";
		checkbox.IsChecked = false;
		checkbox.IsEnabled = false;

		menu.Clear();

		Assert.Empty(menu);
		Assert.Null(button.Parent);
		Assert.Null(checkbox.Parent);
		Assert.Equal("Play Now", button.Label);
		Assert.False(checkbox.IsChecked);
		Assert.False(checkbox.IsEnabled);

		menu.Add(checkbox);
		menu.Insert(0, button);

		Assert.Same(button, menu[0]);
		Assert.Same(checkbox, menu[1]);
		Assert.Same(menu, button.Parent);
		Assert.Same(menu, checkbox.Parent);
		Assert.Equal("Play Now", button.Label);
		Assert.False(checkbox.IsChecked);
		Assert.False(checkbox.IsEnabled);
	}

	[Fact]
	public void FactoryMethods_CreateExpectedEntryTypes_AndPreserveMenuOrder()
	{
		using var scope = TrayTestScope.Create();

		var menu = scope.Tray.Menu;
		var button = Assert.IsType<ButtonTrayEntry>(menu.AddButton("Open", isEnabled: false));
		var checkbox = Assert.IsType<CheckboxTrayEntry>(menu.InsertCheckbox(0, "Pinned", isChecked: true, isEnabled: false));
		var separator = Assert.IsType<SeparatorTrayEntry>(menu.InsertSeparator(2));
		var submenu = Assert.IsType<SubmenuTrayEntry>(menu.AddSubmenu("Recent"));

		Assert.Collection(
			menu,
			entry => Assert.Same(checkbox, entry),
			entry => Assert.Same(button, entry),
			entry => Assert.Same(separator, entry),
			entry => Assert.Same(submenu, entry));

		Assert.True(checkbox.IsChecked);
		Assert.False(checkbox.IsEnabled);
		Assert.False(button.IsEnabled);
		Assert.Same(menu, checkbox.Parent);
		Assert.Same(menu, button.Parent);
		Assert.Same(menu, separator.Parent);
		Assert.Same(menu, submenu.Parent);
		Assert.Same(submenu, submenu.Menu.ParentTrayEntry);
	}

	[Fact]
	public void CopyToAndEnumeration_ReturnStableManagedInstances_ForNativeBackedMenus()
	{
		using var scope = TrayTestScope.Create();

		var menu = scope.Tray.Menu;
		var first = Assert.IsType<ButtonTrayEntry>(menu.AddButton("First"));
		var second = Assert.IsType<CheckboxTrayEntry>(menu.AddCheckbox("Second", isChecked: true));
		var third = Assert.IsType<SubmenuTrayEntry>(menu.AddSubmenu("Third"));

		var arrayDestination = new TrayEntry[4];
		var copiedToArray = menu.CopyTo(arrayDestination, arrayIndex: 1);
		var spanDestination = new TrayEntry[3];
		var copiedToSpan = menu.CopyTo(spanDestination);

		Assert.Equal(3, copiedToArray);
		Assert.Equal(3, copiedToSpan);
		Assert.Same(first, arrayDestination[1]);
		Assert.Same(second, arrayDestination[2]);
		Assert.Same(third, arrayDestination[3]);
		Assert.Same(first, spanDestination[0]);
		Assert.Same(second, spanDestination[1]);
		Assert.Same(third, spanDestination[2]);

		Assert.Collection(
			menu,
			entry => Assert.Same(first, entry),
			entry => Assert.Same(second, entry),
			entry => Assert.Same(third, entry));
	}

	[Fact]
	public void EntryEventHelpers_AddAndRemoveHandlers_WithoutChangingInstanceOrEventSemantics()
	{
		var button = new ButtonTrayEntry("Open");
		var checkbox = new CheckboxTrayEntry("Pinned", isChecked: true);
		var buttonEntry = (ITrayEntry)button;
		var checkboxEntry = (ITrayEntry)checkbox;
		var buttonSelectedCount = 0;
		var buttonClickedCount = 0;
		var checkboxSelectedCount = 0;
		var checkboxToggledCount = 0;
		var removedHandlerCount = 0;

		buttonEntry.Selected += _ => buttonSelectedCount++;
		checkboxEntry.Selected += _ => checkboxSelectedCount++;

		TrayEntrySelectedEventHandler<ButtonTrayEntry> buttonHandler = _ => buttonClickedCount++;
		TrayEntrySelectedEventHandler<ButtonTrayEntry> removableButtonHandler = _ => removedHandlerCount++;
		TrayEntrySelectedEventHandler<CheckboxTrayEntry> checkboxHandler = _ => checkboxToggledCount++;
		TrayEntrySelectedEventHandler<CheckboxTrayEntry> removableCheckboxHandler = _ => removedHandlerCount++;

		Assert.Same(button, button.AddClickHandler(buttonHandler));
		Assert.Same(button, button.AddClickHandler(removableButtonHandler));
		Assert.Same(button, button.RemoveClickHandler(removableButtonHandler));
		Assert.Same(checkbox, checkbox.AddToggledHandler(checkboxHandler));
		Assert.Same(checkbox, checkbox.AddToggledHandler(removableCheckboxHandler));
		Assert.Same(checkbox, checkbox.RemoveToggledHandler(removableCheckboxHandler));

		button.Click();
		checkbox.Click();

		Assert.Equal(1, buttonSelectedCount);
		Assert.Equal(1, buttonClickedCount);
		Assert.Equal(1, checkboxSelectedCount);
		Assert.Equal(1, checkboxToggledCount);
		Assert.Equal(0, removedHandlerCount);
	}

	private sealed class TrayTestScope(Sdl sdl, Tray tray) : IDisposable
	{
		public Sdl Sdl { get; } = sdl;

		public Tray Tray { get; } = tray;

		public static TrayTestScope Create()
		{
			if (!Thread.IsMainThread)
			{
				throw SkipException.ForSkip("Tray tests require running on SDL's main thread.");
			}

			var sdl = CreateVideoSdlOrSkip();

			try
			{
				return new TrayTestScope(sdl, CreateTrayOrSkip());
			}
			catch
			{
				sdl.Dispose();
				throw;
			}
		}

		public void Dispose()
		{
			try
			{
				Tray.Dispose();
			}
			finally
			{
				Sdl.Dispose();
			}
		}

		private static Sdl CreateVideoSdlOrSkip()
		{
			Error.Clear();

			try
			{
				return new(static builder => builder.InitializeSubSystems(SubSystems.Video));
			}
			catch (SdlException exception)
			{
				throw SkipException.ForSkip(BuildSkipReason("SDL video initialization is unavailable for tray tests", exception));
			}
		}

		private static Tray CreateTrayOrSkip()
		{
			Error.Clear();

			try
			{
				return new Tray(icon: null, tooltip: nameof(TrayLifetimeTests));
			}
			catch (SdlException exception)
			{
				throw SkipException.ForSkip(BuildSkipReason("Tray creation is unavailable in this environment", exception));
			}
		}

		private static string BuildSkipReason(string context, Exception exception)
			=> Error.TryGet(out var error) && !string.IsNullOrWhiteSpace(error)
				? $"{context}: {error}"
				: $"{context}: {exception.Message}";
	}
}