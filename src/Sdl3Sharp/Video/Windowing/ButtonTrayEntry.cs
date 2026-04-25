using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents a clickable button tray entry
/// </summary>
public sealed class ButtonTrayEntry : TrayEntry
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
	/// Creates a new <see cref="ButtonTrayEntry"/> with the specified label
	/// </summary>
	/// <param name="label">The label of the button tray entry. Must be a non-<c><see langword="null"/></c> string.</param>
	/// <param name="isEnabled">An optional value indicating whether the tray entry should be initially <see cref="IsEnabled">enabled</see>. Defaults to <c><see langword="true"/></c>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="label"/> is <c><see langword="null"/></c></exception>
	public ButtonTrayEntry(string label, bool isEnabled = true) :
		base(ValidateLabel(label), TrayEntryFlags.Button | (isEnabled ? 0 : TrayEntryFlags.Disabled))
	{ }

	internal unsafe ButtonTrayEntry(SDL_TrayEntry* entry) :
		base(entry)
	{ }

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
	/// Note that <see cref="ButtonTrayEntry"/> always require a non-<c><see langword="null"/></c> label,
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
	/// An event that is raised when the tray entry is clicked while it is <see cref="IsEnabled">enabled</see>
	/// </summary>
	/// <remarks>
	/// <para>
	/// The event might still be raised for <see cref="IsEnabled">disabled</see> button tray entries as the result of calling the <see cref="Click"/> method.
	/// </para>
	/// </remarks>
	public event TrayEntrySelectedEventHandler<ButtonTrayEntry>? Clicked;

	/// <summary>
	/// Simulates a click on the tray entry
	/// </summary>
	/// <remarks>
	/// <para>
	/// Calling this method might raise the <see cref="Clicked"/> event.
	/// </para>
	/// <para>
	/// This method might even raise the <see cref="Clicked"/> event when the tray entry is <see cref="IsEnabled">disabled</see>.
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
		Clicked?.Invoke(this);
	}
}
