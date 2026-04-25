namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents a method that handles the <see cref="ITrayEntry.Selected"/> event
/// </summary>
/// <typeparam name="TTrayEntry">The type of the tray entry that was selected</typeparam>
/// <param name="entry">The tray entry that was selected</param>
public delegate void TrayEntrySelectedEventHandler<in TTrayEntry>(TTrayEntry entry) where TTrayEntry : notnull, ITrayEntry;