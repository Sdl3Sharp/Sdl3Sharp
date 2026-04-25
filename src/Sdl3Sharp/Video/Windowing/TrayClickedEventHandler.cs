#if SDL3_6_0_OR_GREATER

namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents a method that handles the <see cref="Tray.LeftClicked"/>, <see cref="Tray.RightClicked"/>, and <see cref="Tray.MiddleClicked"/> events
/// </summary>
/// <param name="tray">The <see cref="Tray"/> that was clicked</param>
/// <param name="args">A <see cref="TrayClickedEventArgs"/> that contains the mutable event data</param>
/// <remarks>
/// <para>
/// You can set the <see cref="TrayClickedEventArgs.ShowMenu"/> property of <paramref name="args"/> to <c><see langword="false"/></c> in your event handler to prevent the tray menu from being shown after this event is handled.
/// </para>
/// </remarks>
public delegate void TrayClickedEventHandler(Tray tray, TrayClickedEventArgs args);

#endif
