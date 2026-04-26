namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents how the contents of a texture attached to a render pass will be treated at the end of the render pass
/// </summary>
public enum StoreOperation
{
	/// <summary>The contents generated during the render pass will be stored in memory</summary>
	Store,

	/// <summary>The contents generated during the render pass are not needed after the render pass, and may be discarded</summary>
	DontCare,

	/// <summary>The multisample contents generated during the render pass will be resolved to a non-multisample texture, and the contents of the multisample texture may then be discarded and will be undefined</summary>
	Resolve,

	/// <summary>The multisample contents generated during the render pass will be resolved to a non-multisample texture, and the contents of the multisample texture will be stored in memory</summary>
	ResolveAndStore,
}
