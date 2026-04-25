namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Provides extension methods for <see cref="TrayEntry"/> and tray entry types that derive from it
/// </summary>
public static class TrayEntryExtensions
{
	extension(ButtonTrayEntry entry)
	{
		/// <summary>
		/// Adds a <see cref="ButtonTrayEntry.Clicked"/> event handler to the <see cref="ButtonTrayEntry"/>
		/// </summary>
		/// <param name="handler">The event handler to add to the <see cref="ButtonTrayEntry.Clicked"/> event</param>
		/// <returns>The same <see cref="ButtonTrayEntry"/> instance that the event handler was added to, allowing for method chaining</returns>
		public ButtonTrayEntry AddClickHandler(TrayEntrySelectedEventHandler<ButtonTrayEntry> handler)
		{
			entry.Clicked += handler;
			return entry;
		}

		/// <summary>
		/// Removes a <see cref="ButtonTrayEntry.Clicked"/> event handler from the <see cref="ButtonTrayEntry"/>
		/// </summary>
		/// <param name="handler">The event handler to remove from the <see cref="ButtonTrayEntry.Clicked"/> event</param>
		/// <returns>The same <see cref="ButtonTrayEntry"/> instance that the event handler was removed from, allowing for method chaining</returns>
		public ButtonTrayEntry RemoveClickHandler(TrayEntrySelectedEventHandler<ButtonTrayEntry> handler)
		{
			entry.Clicked -= handler;
			return entry;
		}
	}

	extension(CheckboxTrayEntry entry)
	{
		/// <summary>
		/// Adds a <see cref="CheckboxTrayEntry.Toggled"/> event handler to the <see cref="CheckboxTrayEntry"/>
		/// </summary>
		/// <param name="handler">The event handler to add to the <see cref="CheckboxTrayEntry.Toggled"/> event</param>
		/// <returns>The same <see cref="CheckboxTrayEntry"/> instance that the event handler was added to, allowing for method chaining</returns>
		public CheckboxTrayEntry AddToggledHandler(TrayEntrySelectedEventHandler<CheckboxTrayEntry> handler)
		{
			entry.Toggled += handler;
			return entry;
		}

		/// <summary>
		/// Removes a <see cref="CheckboxTrayEntry.Toggled"/> event handler from the <see cref="CheckboxTrayEntry"/>
		/// </summary>
		/// <param name="handler">The event handler to remove from the <see cref="CheckboxTrayEntry.Toggled"/> event</param>
		/// <returns>The same <see cref="CheckboxTrayEntry"/> instance that the event handler was removed from, allowing for method chaining</returns>
		public CheckboxTrayEntry RemoveToggledHandler(TrayEntrySelectedEventHandler<CheckboxTrayEntry> handler)
		{
			entry.Toggled -= handler;
			return entry;
		}
	}
}
