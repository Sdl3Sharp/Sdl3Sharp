using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents a submenu tray entry that's associated with a <see cref="TrayMenu"/> that can contain other tray entries
/// </summary>
public sealed class SubmenuTrayEntry : TrayEntry
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
	/// Creates a new <see cref="SubmenuTrayEntry"/> with the specified label
	/// </summary>
	/// <param name="label">The label of the submenu tray entry. Must be a non-<c><see langword="null"/></c> string.</param>
	/// <param name="isEnabled">An optional value indicating whether the tray entry should be initially <see cref="IsEnabled">enabled</see>. Defaults to <c><see langword="true"/></c>.</param>
	/// <exception cref="ArgumentNullException"><paramref name="label"/> is <c><see langword="null"/></c></exception>
	public SubmenuTrayEntry(string label, bool isEnabled = true) :
		base(ValidateLabel(label), TrayEntryFlags.Submenu | (isEnabled ? 0 : TrayEntryFlags.Disabled))
	{ }

	internal unsafe SubmenuTrayEntry(SDL_TrayEntry* entry) :
		base(entry)
	{ }

	/// <inheritdoc/>
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
	/// Note that <see cref="SubmenuTrayEntry"/> always require a non-<c><see langword="null"/></c> label,
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
	/// Gets the submenu associated with the tray entry
	/// </summary>
	/// <value>
	/// The submenu associated with the tray entry
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the thread that created the <see cref="Tray">tray</see>.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">The submenu could not be created</exception>
	public new TrayMenu Menu
	{
		get
		{
			var menu = base.Menu;

			if (menu is null)
			{
				if (!TryCreateMenu(out menu))
				{
					failCouldNotCreateMenu();
				}
			}

			return menu;

			[DoesNotReturn]
			static void failCouldNotCreateMenu() => throw new SdlException($"Could not create the {nameof(TrayMenu)} for this {nameof(SubmenuTrayEntry)}");
		}
	}
}