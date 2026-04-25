#if SDL3_6_0_OR_GREATER

namespace Sdl3Sharp.Video.Windowing;

partial class Tray
{
	/// <summary>
	/// Provides property names for <see cref="Tray(Surface?, string?, Properties?)">properties used when creating a <see cref="Tray"/></see>
	/// </summary>
	public static class PropertyNames
	{
		/// <summary>
		/// The name of a <see cref="Tray(Surface?, string?, Properties?)">property used when creating a <see cref="Tray"/></see>,
		/// that holds the surface used as the icon for the tray icon
		/// </summary>
		/// <remarks>
		/// <para>
		/// Specifying this property is optional and its value may be <c><see langword="null"/></c>.
		/// </para>
		/// </remarks>
		public const string CreateIconPointer = "SDL.tray.create.icon";

		/// <summary>
		/// The name of a <see cref="Tray(Surface?, string?, Properties?)">property used when creating a <see cref="Tray"/></see>,
		/// that holds the tooltip text to be displayed when the mouse hovers the tray icon
		/// </summary>
		/// <remarks>
		/// <para>
		/// Not all platforms support tooltips for tray icons, so this property might be ignored on some platforms.
		/// </para>
		/// <para>
		/// Specifying this property is optional and its value may be <c><see langword="null"/></c>.
		/// </para>
		/// </remarks>
		public const string CreateTooltipString = "SDL.tray.create.tooltip";

		/// <summary>
		/// The name of a <see cref="Tray(Surface?, string?, Properties?)">property used when creating a <see cref="Tray"/></see>,
		/// that holds a pointer to the user data to be passed to the "click" callbacks of the tray icon
		/// </summary>
		/// <remarks>
		/// <para>
		/// Specifying this property is optional and its value may be <c><see langword="null"/></c>.
		/// </para>
		/// </remarks>
		public const string CreateUserdataPointer = "SDL.tray.create.userdata";

		/// <summary>
		/// The name of a <see cref="Tray(Surface?, string?, Properties?)">property used when creating a <see cref="Tray"/></see>,
		/// that holds a pointer to a callback function to be called when the tray icon is left-clicked
		/// </summary>
		/// <remarks>
		/// <para>
		/// Not all platforms support left-click callbacks for tray icons, so this property might be ignored on some platforms.
		/// </para>
		/// <para>
		/// Specifying this property is optional and its value may be <c><see langword="null"/></c>.
		/// </para>
		/// </remarks>
		public const string CreateLeftClickCallbackPointer = "SDL.tray.create.leftclick_callback";

		/// <summary>
		/// The name of a <see cref="Tray(Surface?, string?, Properties?)">property used when creating a <see cref="Tray"/></see>,
		/// that holds a pointer to a callback function to be called when the tray icon is right-clicked
		/// </summary>
		/// <remarks>
		/// <para>
		/// Not all platforms support right-click callbacks for tray icons, so this property might be ignored on some platforms.
		/// </para>
		/// <para>
		/// Specifying this property is optional and its value may be <c><see langword="null"/></c>.
		/// </para>
		/// </remarks>
		public const string CreateRightClickCallbackPointer = "SDL.tray.create.rightclick_callback";

		/// <summary>
		/// The name of a <see cref="Tray(Surface?, string?, Properties?)">property used when creating a <see cref="Tray"/></see>,
		/// that holds a pointer to a callback function to be called when the tray icon is middle-clicked
		/// </summary>
		/// <remarks>
		/// <para>
		/// Not all platforms support middle-click callbacks for tray icons, so this property might be ignored on some platforms.
		/// </para>
		/// <para>
		/// Specifying this property is optional and its value may be <c><see langword="null"/></c>.
		/// </para>
		/// </remarks>
		public const string CreateMiddleClickCallbackPointer = "SDL.tray.create.middleclick_callback";
	}
}

#endif
