using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents a toggleable checkbox tray entry
/// </summary>
public sealed class CheckboxTrayEntry : TrayEntry
{
	private static string ValidateLabel(string label, [CallerArgumentExpression(nameof(label))] string? parameterName = default)
	{
		if (label is null)
		{
			failLabelArgumentNull(parameterName);
		}

		return label;

		[DoesNotReturn]
		static void failLabelArgumentNull(string? parameterName) => throw new ArgumentNullException(parameterName);
	}

	/// <summary>
	/// Creates a new <see cref="CheckboxTrayEntry"/> with the specified label
	/// </summary>
	/// <param name="label">The label of the checkbox tray entry. Must be a non-<c><see langword="null"/></c> string.</param>
	/// <param name="isChecked">An optional value indicating whether the tray entry should be initially <see cref="IsChecked">checked</see>. Defaults to <c><see langword="false"/></c>.</param>
	/// <param name="isEnabled">An optional value indicating whether the tray entry should be initially <see cref="IsEnabled">enabled</see>. Defaults to <c><see langword="true"/></c>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="label"/> is <c><see langword="null"/></c></exception>
	public CheckboxTrayEntry(string label, bool isChecked = false, bool isEnabled = true) :
		base(ValidateLabel(label), TrayEntryFlags.Checkbox | (isChecked ? TrayEntryFlags.Checked : 0) | (isEnabled ? 0 : TrayEntryFlags.Disabled))
	{ }

	internal unsafe CheckboxTrayEntry(SDL_TrayEntry* entry) :
		base(entry)
	{ }

	/// <summary>
	/// Gets or sets a value indicating whether the tray entry is checked
	/// </summary>
	/// <value>
	/// A value indicating whether the tray entry is checked
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public new bool IsChecked
	{
		get => base.IsChecked;
		set => base.IsChecked = value;
	}

	/// <inheritdoc cref="TrayEntry.IsEnabled"/>
	public new bool IsEnabled
	{
		get => base.IsEnabled;
		set => base.IsEnabled = value;
	}

	/// <summary>
	/// Gets or sets the label of the tray entry
	/// </summary>
	/// <value>
	/// The label of the tray entry
	/// </value>
	/// <remarks>
	/// <para>
	/// Note that <see cref="CheckboxTrayEntry"/> always require a non-<c><see langword="null"/></c> label,
	/// and trying to set the value of this property to <c><see langword="null"/></c> will result in an <see cref="ArgumentNullException"/> being thrown.
	/// </para>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentNullException">When setting the value of this property to <c><see langword="null"/></c></exception>
	public new string Label
	{
		get => base.Label!;
		set => base.Label = ValidateLabel(value);
	}

	/// <summary>
	/// An event that is raised when the tray entry is <see cref="IsChecked">toggled</see> while it is <see cref="IsEnabled">enabled</see>
	/// </summary>
	/// <remarks>
	/// <para>
	/// The event might still be raised for <see cref="IsEnabled">disabled</see> checkbox tray entries as the result of calling the <see cref="Click"/> method.
	/// </para>
	/// </remarks>
	public event TrayEntrySelectedEventHandler<CheckboxTrayEntry>? Toggled;

	/// <summary>
	/// Simulates a click on the tray entry
	/// </summary>
	/// <remarks>
	/// <para>
	/// Calling this method might toggle the value of the <see cref="IsChecked"/> property and raise the <see cref="Toggled"/> event.
	/// </para>
	/// <para>
	/// This method might even raise the <see cref="Toggled"/> event when the tray entry is <see cref="IsEnabled">disabled</see>.
	/// </para>
	/// <para>
	/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	public new void Click() => base.Click();

	/// <inheritdoc/>
	protected sealed override void OnSelected()
	{		
		base.OnSelected();
		Toggled?.Invoke(this);
	}
}
