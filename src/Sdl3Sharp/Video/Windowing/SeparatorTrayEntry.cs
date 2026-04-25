namespace Sdl3Sharp.Video.Windowing;

/// <summary>
/// Represents a non-interactive separator tray entry
/// </summary>
public sealed class SeparatorTrayEntry : TrayEntry
{
	/// <summary>
	/// Creates a new <see cref="SeparatorTrayEntry"/>
	/// </summary>
	public SeparatorTrayEntry() :
		base(label: null, flags: 0)
	{ }

	internal unsafe SeparatorTrayEntry(SDL_TrayEntry* entry) :
		base(entry)
	{ }
}
