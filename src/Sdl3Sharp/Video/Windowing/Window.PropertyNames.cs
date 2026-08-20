using Sdl3Sharp.Events;
using Sdl3Sharp.Input;
using Sdl3Sharp.Video.Coloring;

namespace Sdl3Sharp.Video.Windowing;

partial class Window
{
	/// <summary>
	/// Provides property names for <see cref="Window"/> <see cref="Properties">properties</see>
	/// </summary>
	public abstract class PropertyNames
	{
		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should always be above other windows
		/// </summary>
		public const string CreateAlwaysOnTopBoolean = "SDL.window.create.always_on_top";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should be created without a border (without decorations)
		/// </summary>
		public const string CreateBorderlessBoolean = "SDL.window.create.borderless";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether a "tooltip" or "menu" popup window should be automatically constrained to the bounds of the display
		/// </summary>
		/// <remarks>
		/// <para>
		/// The default value for the associated property, if not specified otherwise when creating a window, is <c><see langword="true"/></c>.
		/// </para>
		/// </remarks>
		public const string CreateConstrainPopupBoolean = "SDL.window.create.constrain_popup";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should accept input focus
		/// </summary>
		/// <remarks>
		/// <para>
		/// The default value for the associated property, if not specified otherwise when creating a window, is <c><see langword="true"/></c>.
		/// </para>
		/// </remarks>
		public const string CreateFocusableBoolean = "SDL.window.create.focusable";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window will be used with an externally managed graphics context
		/// </summary>
		public const string CreateExternalGraphicsContextBoolean = "SDL.window.create.external_graphics_context";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds the <see cref="WindowFlags"/> to be used when creating the window
		/// </summary>
		public const string CreateFlagsNumber = "SDL.window.create.flags";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should be created initially in fullscreen mode at desktop resolution
		/// </summary>
		public const string CreateFullscreenBoolean = "SDL.window.create.fullscreen";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds the height of the window
		/// </summary>
		public const string CreateHeightNumber = "SDL.window.create.height";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should be created initially hidden
		/// </summary>
		public const string CreateHiddenBoolean = "SDL.window.create.hidden";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should be created with a high pixel density buffer, if possible
		/// </summary>
		public const string CreateHighPixelDensityBoolean = "SDL.window.create.high_density";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should be created initially maximized
		/// </summary>
		public const string CreateMaximizedBoolean = "SDL.window.create.maximized";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value determining whether the window should be created as a "menu" popup window
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the value of the associated property is set to <c><see langword="true"/></c>,
		/// you must specify a <see cref="CreateParentPointer">parent window</see> for the window to be created.
		/// </para>
		/// </remarks>
		public const string CreateMenuBoolean = "SDL.window.create.menu";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window will be used with Metal rendering
		/// </summary>
		public const string CreateMetalBoolean = "SDL.window.create.metal";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should be created initially minimized
		/// </summary>
		public const string CreateMinimizedBoolean = "SDL.window.create.minimized";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should be created as modal to its parent window
		/// </summary>
		/// <remarks>
		/// <para>
		/// If the value of the associated property is set to <c><see langword="true"/></c>,
		/// you must specify a <see cref="CreateParentPointer">parent window</see> for the window to be created.
		/// </para>
		/// </remarks>
		public const string CreateModalBoolean = "SDL.window.create.modal";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should be created initially with the mouse grabbed
		/// </summary>
		public const string CreateMouseGrabbedBoolean = "SDL.window.create.mouse_grabbed";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window will be used with OpenGL rendering
		/// </summary>
		public const string CreateOpenGLBoolean = "SDL.window.create.opengl";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a pointer to the parent <see cref="Window"/> of the window to be created
		/// </summary>
		/// <remarks>
		/// <para>
		/// You must specify a valid pointer as the value for the associated property when you want to create a "tooltip" or "menu" popup window or a window that should be modal to its parent window.
		/// </para>
		/// </remarks>
		public const string CreateParentPointer = "SDL.window.create.parent";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should be resizable by the user
		/// </summary>
		public const string CreateResizableBoolean = "SDL.window.create.resizable";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds the title of the window
		/// </summary>
		/// <remarks>
		/// <para>
		/// Depending on the platform and windowing backend, Unicode characters may be allowed in the window title.
		/// </para>
		/// </remarks>
		public const string CreateTitleString = "SDL.window.create.title";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should be created with a transparent buffer
		/// </summary>
		/// <remarks>
		/// <para>
		/// The associated property will determine whether the window will be shown transparent in areas where the alpha channel value of the window's buffer is equal to <c>0</c>.
		/// </para>
		/// </remarks>
		public const string CreateTransparentBoolean = "SDL.window.create.transparent";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should be created as a "tooltip" popup window
		/// </summary>
		public const string CreateTooltipBoolean = "SDL.window.create.tooltip";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window should be created as a utility window (e.g. not showing in the task bar and window list)
		/// </summary>
		public const string CreateUtilityBoolean = "SDL.window.create.utility";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds a value indicating whether the window will be used with Vulkan rendering
		/// </summary>
		public const string CreateVulkanBoolean = "SDL.window.create.vulkan";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds the width of the window
		/// </summary>
		public const string CreateWidthNumber = "SDL.window.create.width";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds the X coordinate of the window, <see cref="WindowPosition.Centered"/>, <see cref="WindowPosition.Undefined"/>, or a value obtained from using the <see cref="WindowPosition.CenteredOn(Display)"/> or <see cref="WindowPosition.UndefinedOn(Display)"/> methods
		/// </summary>
		/// <remarks>
		/// <para>
		/// You can either specify definitive coordinates as the value for the associated property,
		/// or you can use <see cref="WindowPosition.Centered"/>, <see cref="WindowPosition.Undefined"/>, <see cref="WindowPosition.CenteredOn(Display)"/>, or <see cref="WindowPosition.UndefinedOn(Display)"/>
		/// to specify some special window positions to be determined in relation to the primary display or a specific display.
		/// </para>
		/// </remarks>
		public const string CreateXNumber = "SDL.window.create.x";

		/// <summary>
		/// The name of a <see cref="TryCreate(out Window?, bool?, bool?, bool?, bool?, WindowFlags?, bool?, bool?, int?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, bool?, Window?, bool?, string?, bool?, bool?, bool?, bool?, int?, WindowPosition?, WindowPosition?, Properties?)">property used when creating a <see cref="Window"/></see>
		/// that holds the Y coordinate of the window, <see cref="WindowPosition.Centered"/>, <see cref="WindowPosition.Undefined"/>, or a value obtained from using the <see cref="WindowPosition.CenteredOn(Display)"/> or <see cref="WindowPosition.UndefinedOn(Display)"/> methods
		/// </summary>
		/// <remarks>
		/// <para>
		/// You can either specify definitive coordinates as the value for the associated property,
		/// or you can use <see cref="WindowPosition.Centered"/>, <see cref="WindowPosition.Undefined"/>, <see cref="WindowPosition.CenteredOn(Display)"/>, or <see cref="WindowPosition.UndefinedOn(Display)"/>
		/// to specify some special window positions to be determined in relation to the primary display or a specific display.
		/// </para>
		/// </remarks>
		public const string CreateYNumber = "SDL.window.create.y";

		/// <summary>
		/// The name of a <em>read-only</em> <see cref="Properties">property</see> that holds a pointer to the shape that's associated with the window
		/// </summary>
		/// <remarks>
		/// <para>
		/// See <see cref="Window.Shape"/> for more information about window shapes.
		/// </para>
		/// </remarks>
		public const string ShapePointer = "SDL.window.shape";

		/// <summary>
		/// The name of a <em>read-only</em> <see cref="Properties">property</see> that holds a value indicating whether the window has <see cref="HdrHeadroomFloat">HDR headroom</see> above the <see cref="SdrWhiteLevelFloat">SDR white level</see>
		/// </summary>
		/// <remarks>
		/// <para>
		/// The value of the associated property can change dynamically at runtime when a <see cref="EventType.WindowHdrStateChanged"/> event is sent.
		/// </para>
		/// </remarks>
		public const string HdrEnabledBoolean = "SDL.window.HDR_enabled";

		/// <summary>
		/// The name of a <em>read-only</em> <see cref="Properties">property</see> that holds the value of the SDR white in the <see cref="ColorSpace.Srgb"/> color space for the window
		/// </summary>
		/// <remarks>
		/// <para>
		/// On Windows the value of the associated property corresponds to the SDR white level in scRGB color space, and on Apple platform this is always <c>1.0</c> for EDR content.
		/// This property can change dynamically at runtime when a <see cref="EventType.WindowHdrStateChanged"/> event (<see cref="WindowEvent"/>) is sent.
		/// </para>
		/// </remarks>
		public const string SdrWhiteLevelFloat = "SDL.window.SDR_white_level";

		/// <summary>
		/// The name of a <em>read-only</em> <see cref="Properties">property</see> that holds the additional high dynamic range that can be displayed, in terms of the <see cref="SdrWhiteLevelFloat">SDR white level</see>
		/// </summary>
		/// <remarks>
		/// <para>
		/// The value of the associated property will be <c>1.0</c> when HDR is not enabled.
		/// This property can change dynamically at runtime when a <see cref="EventType.WindowHdrStateChanged"/> event (<see cref="WindowEvent"/>) is sent.
		/// </para>
		/// </remarks>
		public const string HdrHeadroomFloat = "SDL.window.HDR_headroom";

		/// <summary>
		/// The name of a <see cref="TryStartTextInput(Input.TextInputType?, Input.Capitalization?, bool?, bool?, string?, string?, string?, int?, Properties?)">property used when starting text input</see>
		/// that holds a <see cref="TextInputType"/> describing the type of text to be inputted
		/// </summary>
		/// <remarks>
		/// <para>
		/// If not specified, the type of text to be inputted will default to <see cref="TextInputType.Text"/>.
		/// </para>
		/// </remarks>
		public const string TextInputTypeNumber = "SDL.textinput.type";

		/// <summary>
		/// The name of a <see cref="TryStartTextInput(Input.TextInputType?, Input.Capitalization?, bool?, bool?, string?, string?, string?, int?, Properties?)">property used when starting text input</see>
		/// that holds a <see cref="Capitalization"/> describing how the text to be inputted should be capitalized
		/// </summary>
		/// <remarks>
		/// <para>
		/// If not specified, the capitalization of the text to be inputted will default to
		/// <see cref="Capitalization.Sentences"/> if the type of text is <see cref="TextInputType.Text"/>,
		/// <see cref="Capitalization.Words"/> if the type of text is <see cref="TextInputType.TextName"/>,
		/// or <see cref="Capitalization.None"/> for any other type of text.
		/// </para>
		/// </remarks>
		public const string TextInputCapitalizationNumber = "SDL.textinput.capitalization";

		/// <summary>
		/// The name of a <see cref="TryStartTextInput(Input.TextInputType?, Input.Capitalization?, bool?, bool?, string?, string?, string?, int?, Properties?)">property used when starting text input</see>
		/// that holds a value indicating whether the text input should use auto-completion and auto-correction features
		/// </summary>
		/// <remarks>
		/// <para>
		/// If not specified, the text input will default to using auto-completion and auto-correction features.
		/// </para>
		/// </remarks>
		public const string TextInputAutocorrectBoolean = "SDL.textinput.autocorrect";

		/// <summary>
		/// The name of a <see cref="TryStartTextInput(Input.TextInputType?, Input.Capitalization?, bool?, bool?, string?, string?, string?, int?, Properties?)">property used when starting text input</see>
		/// that holds a value indicating whether the text input should allow multiple lines of text to be inputted
		/// </summary>
		/// <remarks>
		/// <para>
		/// If not specified, the text input's default will depend on the value of the <see cref="Hint.ReturnKeyHidesIme"/> hint:
		/// <list type="bullet">
		///		<item>
		///			<term>
		///				<see cref="Hint.ReturnKeyHidesIme"/> set to <c>"0"</c>
		///			</term>
		///			<description>
		///				Multiline text input is allowed
		///			</description>
		///		</item>
		///		<item>
		///			<term>
		///				<see cref="Hint.ReturnKeyHidesIme"/> set to <c>"1"</c>
		///			</term>
		///			<description>
		///				Multiline text input is not allowed
		///			</description>
		///		</item>
		///		<item>
		///			<term>
		///				<see cref="Hint.ReturnKeyHidesIme"/> is not set
		///			</term>
		///			<description>
		///				Multiline text input is allowed (the same behavior as if <see cref="Hint.ReturnKeyHidesIme"/> was set to <c>"0"</c>)
		///			</description>
		///		</item>
		///	</list>
		/// </para>
		/// </remarks>
		public const string TextInputMultilineBoolean = "SDL.textinput.multiline";

		/// <summary>
		/// The name of a <see cref="TryStartTextInput(Input.TextInputType?, Input.Capitalization?, bool?, bool?, string?, string?, string?, int?, Properties?)">property used when starting text input</see>
		/// that holds a title to be used at the top of the on-screen keyboard, if applicable
		/// </summary>
		public const string TextInputTitleString = "SDL.textinput.title";

		/// <summary>
		/// The name of a <see cref="TryStartTextInput(Input.TextInputType?, Input.Capitalization?, bool?, bool?, string?, string?, string?, int?, Properties?)">property used when starting text input</see>
		/// that holds a placeholder text to be shown in the text input when it's empty
		/// </summary>
		public const string TextInputPlaceholderString = "SDL.textinput.placeholder";

		/// <summary>
		/// The name of a <see cref="TryStartTextInput(Input.TextInputType?, Input.Capitalization?, bool?, bool?, string?, string?, string?, int?, Properties?)">property used when starting text input</see>
		/// that holds a text to be pre-filled in the text input when it starts
		/// </summary>
		public const string TextInputDefaultTextString = "SDL.textinput.default_text";

		/// <summary>
		/// The name of a <see cref="TryStartTextInput(Input.TextInputType?, Input.Capitalization?, bool?, bool?, string?, string?, string?, int?, Properties?)">property used when starting text input</see>
		/// that holds the maximum number of characters that can be inputted into the text input
		/// </summary>
		/// <remarks>
		/// <para>
		/// Note that the value of the associated property is measured in characters, not bytes.
		/// </para>
		/// </remarks>
		public const string TextInputMaxLengthNumber = "SDL.textinput.max_length";

		private protected PropertyNames() { }
	}
}
