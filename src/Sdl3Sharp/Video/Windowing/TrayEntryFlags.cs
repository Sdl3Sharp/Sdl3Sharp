namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents flags that specify the type and initial state of a <see cref="TrayEntry"/> when creating it using <see cref="TrayMenu.AddEntry"/>, <see cref="TrayMenu.InsertEntry"/>, or <see cref="TrayEntry.TrayEntry(string?, Sdl3Sharp.Video.Windowing.TrayEntryFlags)"/>
/// </summary>
public enum TrayEntryFlags : uint
{
	/// <summary>The <see cref="TrayEntry"/> to be created should be a button</summary>
	/// <remarks>
	/// <para>
	/// You <em>cannot</em> combine this flag with <see cref="Checkbox"/> or <see cref="Submenu"/> when creating a new <see cref="TrayEntry"/> using <see cref="TrayMenu.AddEntry"/> or <see cref="TrayMenu.InsertEntry"/>.
	/// </para>
	/// </remarks>
	Button = 0x00000001u,

	/// <summary>The <see cref="TrayEntry"/> to be created should be a checkbox</summary>
	/// <remarks>
	/// <para>
	/// You <em>cannot</em> combine this flag with <see cref="Button"/> or <see cref="Submenu"/> when creating a new <see cref="TrayEntry"/> using <see cref="TrayMenu.AddEntry"/> or <see cref="TrayMenu.InsertEntry"/>.
	/// </para>
	/// </remarks>
	Checkbox = 0x00000002u,

	/// <summary>The <see cref="TrayEntry"/> to be created should be a submenu</summary>
	/// <remarks>
	/// <para>
	/// You <em>cannot</em> combine this flag with <see cref="Button"/> or <see cref="Checkbox"/> when creating a new <see cref="TrayEntry"/> using <see cref="TrayMenu.AddEntry"/> or <see cref="TrayMenu.InsertEntry"/>.
	/// </para>
	/// </remarks>
	Submenu  = 0x00000004u,

	/// <summary>The <see cref="TrayEntry"/> to be created should start as <see cref="TrayEntry.IsEnabled">disabled</see></summary>
	/// <remarks>
	/// <para>
	/// This flag is optional and can be combined with either <see cref="Button"/>, <see cref="Checkbox"/>, or <see cref="Submenu"/> when creating a new <see cref="TrayEntry"/> using <see cref="TrayMenu.AddEntry"/> or <see cref="TrayMenu.InsertEntry"/>.
	/// </para>
	/// <para>
	/// If this flag is not specified, the <see cref="TrayEntry"/> will start as <see cref="TrayEntry.IsEnabled">enabled</see> by default.
	/// </para>
	/// </remarks>
	Disabled = 0x80000000u,

	/// <summary>The <see cref="TrayEntry"/> to be created should start as <see cref="TrayEntry.IsChecked">checked</see></summary>
	/// <remarks>
	/// <para>
	/// This flag is optional and can <em>only</em> be combined with <see cref="Checkbox"/> when creating a new <see cref="TrayEntry"/> using <see cref="TrayMenu.AddEntry"/> or <see cref="TrayMenu.InsertEntry"/>.
	/// </para>
	/// <para>
	/// If this flag is not specified, the <see cref="TrayEntry"/> will start as <see cref="TrayEntry.IsChecked">unchecked</see> by default.
	/// </para>
	/// </remarks>
	Checked = 0x40000000u,
}
