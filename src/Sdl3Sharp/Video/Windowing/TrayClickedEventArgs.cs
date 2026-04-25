#if SDL3_6_0_OR_GREATER

namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Provides mutable event data for the <see cref="Tray.LeftClicked"/>, <see cref="Tray.RightClicked"/>, and <see cref="Tray.MiddleClicked"/> events
/// </summary>
public sealed class TrayClickedEventArgs
{
	/// <summary>Gets or sets a value indicating whether the tray menu should be shown after this event is handled</summary>
	/// <remarks>
	/// <para>
	/// The value of this property defaults to <c><see langword="true"/></c> when the event is raised.
	/// You can set the value to <c><see langword="false"/></c> to prevent the tray menu from being shown after this event is handled.
	/// </para>
	/// <para>
	/// Note that other event handlers might have already manipulated this property before or might manipulate it after your event handler.
	/// Generally, the order in which event handlers are called is not guaranteed, so you should at least set the value of property to your preferred value in your event handler and don't rely on its default value.
	/// </para>
	/// </remarks>
	public bool ShowMenu { get; set; }
}

#endif
