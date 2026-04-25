using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Provides extension methods for <see cref="TrayMenu"/>
/// </summary>
public static class TrayMenuExtensions
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

	extension(TrayMenu menu)
	{
		/// <summary>
		/// Creates a new <see cref="ButtonTrayEntry"/>, and adds it at the end of the tray menu
		/// </summary>
		/// <param name="label">The label of the button tray entry. Must be a non-<c><see langword="null"/></c> string.</param>
		/// <param name="isEnabled">An optional value indicating whether the tray entry should be initially <see cref="ButtonTrayEntry.IsEnabled">enabled</see>. Defaults to <c><see langword="true"/></c>.</param>
		/// <returns>The newly created button tray entry, if it was successfully created and added at the end of the tray menu; otherwise, <c><see langword="null"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
		/// <remarks>
		/// <para>
		/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="label"/> is <c><see langword="null"/></c></exception>
		public ButtonTrayEntry? AddButton(string label, bool isEnabled = true)
			=> menu.InsertButton(position: -1, label, isEnabled);

		/// <summary>
		/// Creates a new <see cref="CheckboxTrayEntry"/>, and adds it at the end of the tray menu
		/// </summary>
		/// <param name="label">The label of the checkbox tray entry. Must be a non-<c><see langword="null"/></c> string.</param>
		/// <param name="isChecked">An optional value indicating whether the tray entry should be initially <see cref="CheckboxTrayEntry.IsChecked">checked</see>. Defaults to <c><see langword="false"/></c>.</param>
		/// <param name="isEnabled">An optional value indicating whether the tray entry should be initially <see cref="CheckboxTrayEntry.IsEnabled">enabled</see>. Defaults to <c><see langword="true"/></c>.</param>
		/// <returns>The newly created checkbox tray entry, if it was successfully created and added at the end of the tray menu; otherwise, <c><see langword="null"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
		/// <remarks>
		/// <para>
		/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="label"/> is <c><see langword="null"/></c></exception>
		public CheckboxTrayEntry? AddCheckbox(string label, bool isChecked = false, bool isEnabled = true)
			=> menu.InsertCheckbox(position: -1, label, isChecked, isEnabled);

		/// <summary>
		/// Creates a new <see cref="SubmenuTrayEntry"/>, and adds it at the end of the tray menu
		/// </summary>
		/// <param name="label">The label of the submenu tray entry. Must be a non-<c><see langword="null"/></c> string.</param>
		/// <param name="isEnabled">An optional value indicating whether the tray entry should be initially <see cref="SubmenuTrayEntry.IsEnabled">enabled</see>. Defaults to <c><see langword="true"/></c>.</param>
		/// <returns>The newly created submenu tray entry, if it was successfully created and added at the end of the tray menu; otherwise, <c><see langword="null"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
		/// <remarks>
		/// <para>
		/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="label"/> is <c><see langword="null"/></c></exception>
		public SubmenuTrayEntry? AddSubmenu(string label, bool isEnabled = true)
			=> menu.InsertSubmenu(position: -1, label, isEnabled);

		/// <summary>
		/// Creates a new <see cref="SeparatorTrayEntry"/>, and adds it at the end of the tray menu
		/// </summary>
		/// <returns>The newly created separator tray entry, if it was successfully created and added at the end of the tray menu; otherwise, <c><see langword="null"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
		/// <remarks>
		/// <para>
		/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
		/// </para>
		/// </remarks>
		public SeparatorTrayEntry? AddSeparator()
			=> menu.InsertSeparator(position: -1);

		/// <summary>
		/// Creates a new <see cref="ButtonTrayEntry"/>, and inserts it at the specified position in the tray menu
		/// </summary>
		/// <param name="position">The zero-based position at which to insert the button tray entry in the tray menu, or <c>-1</c> to insert the tray entry at the end of the tray menu</param>
		/// <param name="label">The label of the button tray entry. Must be a non-<c><see langword="null"/></c> string.</param>
		/// <param name="isEnabled">An optional value indicating whether the tray entry should be initially <see cref="ButtonTrayEntry.IsEnabled">enabled</see>. Defaults to <c><see langword="true"/></c>.</param>
		/// <returns>The newly created button tray entry, if it was successfully created and inserted at the specified <paramref name="position"/> in the tray menu; otherwise, <c><see langword="null"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
		/// <remarks>
		/// <para>
		/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="label"/> is <c><see langword="null"/></c></exception>
		public ButtonTrayEntry? InsertButton(int position, string label, bool isEnabled = true)
		{
			switch (menu.InsertEntry(position, ValidateLabel(label), TrayEntryFlags.Button | (isEnabled ? 0 : TrayEntryFlags.Disabled)))
			{
				case null: return null;
				case ButtonTrayEntry entry: return entry;
				case var entry: menu.Remove(entry); return null;
			}
		}

		/// <summary>
		/// Creates a new <see cref="CheckboxTrayEntry"/>, and inserts it at the specified position in the tray menu
		/// </summary>
		/// <param name="position">The zero-based position at which to insert the checkbox tray entry in the tray menu, or <c>-1</c> to insert the tray entry at the end of the tray menu</param>
		/// <param name="label">The label of the checkbox tray entry. Must be a non-<c><see langword="null"/></c> string.</param>
		/// <param name="isChecked">An optional value indicating whether the tray entry should be initially <see cref="CheckboxTrayEntry.IsChecked">checked</see>. Defaults to <c><see langword="false"/></c>.</param>
		/// <param name="isEnabled">An optional value indicating whether the tray entry should be initially <see cref="CheckboxTrayEntry.IsEnabled">enabled</see>. Defaults to <c><see langword="true"/></c>.</param>
		/// <returns>The newly created checkbox tray entry, if it was successfully created and inserted at the specified <paramref name="position"/> in the tray menu; otherwise, <c><see langword="null"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
		/// <remarks>
		/// <para>
		/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="label"/> is <c><see langword="null"/></c></exception>
		public CheckboxTrayEntry? InsertCheckbox(int position, string label, bool isChecked = false, bool isEnabled = true)
		{
			switch (menu.InsertEntry(position, ValidateLabel(label), TrayEntryFlags.Checkbox | (isChecked ? TrayEntryFlags.Checked : 0) | (isEnabled ? 0 : TrayEntryFlags.Disabled)))
			{
				case null: return null;
				case CheckboxTrayEntry entry: return entry;
				case var entry: menu.Remove(entry); return null;
			}
		}

		/// <summary>
		/// Creates a new <see cref="SubmenuTrayEntry"/>, and inserts it at the specified position in the tray menu
		/// </summary>
		/// <param name="position">The zero-based position at which to insert the submenu tray entry in the tray menu, or <c>-1</c> to insert the tray entry at the end of the tray menu</param>
		/// <param name="label">The label of the submenu tray entry. Must be a non-<c><see langword="null"/></c> string.</param>
		/// <param name="isEnabled">An optional value indicating whether the tray entry should be initially <see cref="SubmenuTrayEntry.IsEnabled">enabled</see>. Defaults to <c><see langword="true"/></c>.</param>
		/// <returns>The newly created submenu tray entry, if it was successfully created and inserted at the specified <paramref name="position"/> in the tray menu; otherwise, <c><see langword="null"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
		/// <remarks>
		/// <para>
		/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
		/// </para>
		/// </remarks>
		/// <exception cref="ArgumentNullException"><paramref name="label"/> is <c><see langword="null"/></c></exception>
		public SubmenuTrayEntry? InsertSubmenu(int position, string label, bool isEnabled = true)
		{
			switch (menu.InsertEntry(position, ValidateLabel(label), TrayEntryFlags.Submenu | (isEnabled ? 0 : TrayEntryFlags.Disabled)))
			{
				case null: return null;
				case SubmenuTrayEntry entry: return entry;
				case var entry: menu.Remove(entry); return null;
			}
		}

		/// <summary>
		/// Creates a new <see cref="SeparatorTrayEntry"/>, and inserts it at the specified position in the tray menu
		/// </summary>
		/// <param name="position">The zero-based position at which to insert the separator tray entry in the tray menu, or <c>-1</c> to insert the tray entry at the end of the tray menu</param>
		/// <returns>The newly created separator tray entry, if it was successfully created and inserted at the specified <paramref name="position"/> in the tray menu; otherwise, <c><see langword="null"/></c> (check <see cref="Error.TryGet(out string?)"/> for more information)</returns>
		/// <remarks>
		/// <para>
		/// This method should only be called from the thread that created the <see cref="Tray">tray</see>.
		/// </para>
		/// </remarks>
		public SeparatorTrayEntry? InsertSeparator(int position)
		{
			switch (menu.InsertEntry(position, label: null, flags: 0))
			{
				case null: return null;
				case SeparatorTrayEntry entry: return entry;
				case var entry: menu.Remove(entry); return null;
			}
		}
	}
}
