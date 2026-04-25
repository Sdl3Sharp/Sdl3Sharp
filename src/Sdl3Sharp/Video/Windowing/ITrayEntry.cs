namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents a common abstract interface for all tray entries
/// </summary>
/// <remarks>
/// <para>
/// This interface provides properties and methods that are commonly shared by all types of tray entries.
/// Note that some properties and methods may not be meaningful for certain types of tray entries.
/// </para>
/// </remarks>
public interface ITrayEntry
{
	/// <summary>
	/// Gets or sets a value indicating whether the tray entry is checked
	/// </summary>
	/// <value>
	/// A value indicating whether the tray entry is checked
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is only meaningful for <see cref="CheckboxTrayEntry">checkbox tray entries</see>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	bool IsChecked { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the tray entry is enabled
	/// </summary>
	/// <value>
	/// A value indicating whether the tray entry is enabled
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	bool IsEnabled { get; set; }

	/// <summary>
	/// Gets or sets the label of the tray entry
	/// </summary>
	/// <value>
	/// The label of the tray entry, or <c><see langword="null"/></c> if the entry is a <see cref="SeparatorTrayEntry">separator entry</see>
	/// </value>
	/// <remarks>
	/// <para>
	/// Regular tray entries always have a non-<c><see langword="null"/></c> label, while <see cref="SeparatorTrayEntry">separator tray entries</see> always have a <c><see langword="null"/></c> label.
	/// </para>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	string? Label { get; set; }

	/// <summary>
	/// Gets the submenu associated with the tray entry
	/// </summary>
	/// <value>
	/// The submenu associated with the tray entry, or <c><see langword="null"/></c> if the entry is not a <see cref="SubmenuTrayEntry">submenu entry</see>
	/// </value>
	/// <remarks>
	/// <para>
	/// This property is only meaningful for <see cref="SubmenuTrayEntry">submenu tray entries</see>.
	/// </para>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	TrayMenu? Menu { get; }

	/// <summary>
	/// Gets the tray menu that contains the tray entry
	/// </summary>
	/// <value>
	/// The tray menu that contains the tray entry, if any
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	TrayMenu? Parent { get; }

	/// <summary>
	/// An event that is raised when the tray entry is "selected"
	/// </summary>
	/// <remarks>
	/// <para>
	/// "Selecting" a tray entry has different meanings depending on the type and state of the entry:
	/// <list type="bullet">
	///		<item>
	///			<term><see cref="ButtonTrayEntry">Button tray entries</see></term>
	///			<description>The event is raised when the <em><see cref="IsEnabled">enabled</see></em> entry is <see cref="ButtonTrayEntry.Clicked">clicked</see></description>
	///		</item>
	///		<item>
	///			<term><see cref="CheckboxTrayEntry">Checkbox tray entries</see></term>
	///			<description>The event is raised after the <em><see cref="IsEnabled">enabled</see></em> entry is <see cref="CheckboxTrayEntry.Toggled">toggled</see></description>
	///		</item>
	///		<item>
	///			<term>Other types of tray entries, or <see cref="IsEnabled">disabled</see> entries of any type</term>
	///			<description>The event is <em>never</em> raised as the result of user interaction. The event might still be raised as the result of calling the <see cref="Click"/> method.</description>
	///		</item>
	/// </list>
	/// </para>
	/// </remarks>
	event TrayEntrySelectedEventHandler<ITrayEntry>? Selected;

	/// <summary>
	/// Simulates a click on the tray entry
	/// </summary>
	/// <remarks>
	/// <para>
	/// See then <see cref="Selected"/> event for more information on when events are raised as a result of "selecting" a tray entry.
	/// </para>
	/// <para>
	/// This method might even raise the <see cref="Selected"/> event when the tray entry is <see cref="IsEnabled">disabled</see>.
	/// </para>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	void Click();
}
