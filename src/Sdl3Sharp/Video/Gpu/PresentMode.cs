namespace Sdl3Sharp.Video.Gpu;

// TODO: fix docs
/// <summary>
/// Represents the timing that will be used to present swapchain textures to the operating system
/// </summary>
/// <remarks>
/// <para>
/// <see cref="VSync"/> will always be supported. <see cref="Immediate"/> and <see cref="Mailbox"/> may not be supported on certain systems.
/// </para>
/// <para>
/// It is recommended to check <see cref="SDL_WindowSupportsGPUPresentMode"/> after claiming the window if you wish to change the present mode to <see cref="Immediate"/> or <see cref="Mailbox"/>.
/// </para>
/// </remarks>
public enum PresentMode
{
	/// <summary>
	/// The presentation waits for the vertical blank
	/// </summary>
	/// <remarks>
	/// <para>
	/// No tearing is possible.
	/// If there is a pending image to present, the new image is enqueued for presentation.
	/// This disallows tearing at the cost of visual latency.
	/// </para>
	/// </remarks>
	VSync,

	/// <summary>
	/// The presentation will be immediate
	/// </summary>
	/// <remarks>
	/// <para>
	/// Lowest latency option, but tearing may occur.
	/// </para>
	/// </remarks>
	Immediate,

	/// <summary>
	/// The presentation waits for the vertical blank, but with reduced visual latency compared to <see cref="VSync"/>
	/// </summary>
	/// <remarks>
	/// <para>
	/// No tearing is possible.
	/// If there is a pending image to present, the pending image is replaced by the new image.
	/// Thais is similar to <see cref="VSync"/>, but with reduced visual latency.
	/// </para>
	/// </remarks>
	Mailbox
}
