namespace Sdl3Sharp.Input;

/// <summary>
/// Represents system cursor types
/// </summary>
public enum SystemCursor
{
	/// <summary>Default cursor</summary>
	/// <remarks>
	/// <para>
	/// Usually an arrow.
	/// </para>
	/// </remarks>
	Default,

	/// <summary>Text selection cursor</summary>
	/// <remarks>
	/// <para>
	/// Usually an I-beam.
	/// </para>
	/// </remarks>
	Text,

	/// <summary>Wait cursor</summary>
	/// <remarks>
	/// <para>
	/// Usually an hourglass or watch or spinning ball.
	/// </para>
	/// </remarks>
	Wait,

	/// <summary>Crosshair cursor</summary>
	Crosshair,

	/// <summary>Busy cursor</summary>
	/// <remarks>
	/// <para>
	/// Indicates that the program is busy but still interactive.
	/// </para>
	/// <para>
	/// Usually it's like <see cref="Wait"/> with an arrow.
	/// </para>
	/// </remarks>
	Progress,

	/// <summary>Double arrow cursor pointing north-west and south-east</summary>
	NorthWestSouthEastResize,

	/// <summary>Double arrow cursor pointing north-east and south-west</summary>
	NorthEastSouthWestResize,

	/// <summary>Double arrow cursor pointing west and east</summary>
	EastWestResize,

	/// <summary>Double arrow cursor pointing north and south</summary>
	NorthSouthResize,

	/// <summary>Four arrow cursor pointing north, south, east, and west</summary>
	Move,

	/// <summary>Not permitted cursor</summary>
	/// <remarks>
	/// <para>
	/// Usually a slashed circle or crossbones.
	/// </para>
	/// </remarks>
	NotAllowed,

	/// <summary>Pointer cursor indicating a link</summary>
	/// <remarks>
	/// <para>
	/// Usually a pointing hand.
	/// </para>
	/// </remarks>
	Pointer,

	/// <summary>Resize top-left cursor</summary>
	/// <remarks>
	/// <para>
	/// This may be a single arrow or a double arrow like <see cref="NorthWestSouthEastResize"/>.
	/// </para>
	/// </remarks>
	NorthWestResize,

	/// <summary>Resize top cursor</summary>
	/// <remarks>
	/// <para>
	/// This may be a single arrow or a double arrow like <see cref="NorthSouthResize"/>.
	/// </para>
	/// </remarks>
	NorthResize,

	/// <summary>Resize top-right cursor</summary>
	/// <remarks>
	/// <para>
	/// This may be a single arrow or a double arrow like <see cref="NorthEastSouthWestResize"/>.
	/// </para>
	/// </remarks>
	NorthEastResize,

	/// <summary>Resize right cursor</summary>
	/// <remarks>
	/// <para>
	/// This may be a single arrow or a double arrow like <see cref="EastWestResize"/>.
	/// </para>
	/// </remarks>
	EastResize,

	/// <summary>Resize bottom-right cursor</summary>
	/// <remarks>
	/// <para>
	/// This may be a single arrow or a double arrow like <see cref="NorthWestSouthEastResize"/>.
	/// </para>
	/// </remarks>
	SouthEastResize,

	/// <summary>Resize bottom cursor</summary>
	/// <remarks>
	/// <para>
	/// This may be a single arrow or a double arrow like <see cref="NorthSouthResize"/>.
	/// </para>
	/// </remarks>
	SouthResize,

	/// <summary>Resize bottom-left cursor</summary>
	/// <remarks>
	/// <para>
	/// This may be a single arrow or a double arrow like <see cref="NorthWestSouthEastResize"/>.
	/// </para>
	/// </remarks>
	SouthWestResize,

	/// <summary>Resize left cursor</summary>
	/// <remarks>
	/// <para>
	/// This may be a single arrow or a double arrow like <see cref="EastWestResize"/>.
	/// </para>
	/// </remarks>
	WestResize,

	/// <summary>Context menu cursor</summary>
	/// <remarks>
	/// <para>
	/// Indicates that a context menu is available for the object under the cursor.
	/// </para>
	/// </remarks>
	ContextMenu,

	/// <summary>Help cursor</summary>
	/// <remarks>
	/// <para>
	/// Indicates that help is available for the object under the cursor.
	/// </para>
	/// </remarks>
	Help,

	/// <summary>Cell selection cursor</summary>
	Cell,

	/// <summary>Vertical text selection cursor</summary>
	/// <remarks>
	/// <para>
	/// May be the same as <see cref="Text"/>.
	/// </para>
	/// </remarks>
	VerticalText,

	/// <summary>Shortcut creation cursor</summary>
	/// <remarks>
	/// <para>
	/// Indicates that a shortcut can be created or is being created.
	/// </para>
	/// </remarks>
	Alias,

	/// <summary>Copy cursor</summary>
	/// <remarks>
	/// <para>
	/// Indicates that something is to be copied.
	/// </para>
	/// </remarks>
	Copy,

	/// <summary>Drop not permitted cursor</summary>
	/// <remarks>
	/// <para>
	/// Indicates that an item dragged cannot be dropped at the current location.
	/// </para>
	/// <para>
	/// May be the same as <see cref="NotAllowed"/>.
	/// </para>
	/// </remarks>
	NoDrop,

	/// <summary>Grab cursor</summary>
	/// <remarks>
	/// <para>
	/// Indicates that the object under the cursor can be grabbed.
	/// </para>
	/// </remarks>
	Grab,

	/// <summary>Grabbing cursor</summary>
	/// <remarks>
	/// <para>
	/// Indicates that an object is currently being grabbed.
	/// </para>
	/// </remarks>
	Grabbing,

	/// <summary>Column resize cursor</summary>
	/// <remarks>
	/// <para>
	/// May be the same as <see cref="EastWestResize"/>.
	/// </para>
	/// </remarks>
	ColumnResize,

	/// <summary>Row resize cursor</summary>
	/// <remarks>
	/// <para>
	/// May be the same as <see cref="NorthSouthResize"/>.
	/// </para>
	/// </remarks>
	RowResize,

	/// <summary>Four arrow cursor pointing north, south, east, and west</summary>
	AllScroll,

	/// <summary>Zoom in cursor</summary>
	ZoomIn,

	/// <summary>Zoom out cursor</summary>
	ZoomOut
}
