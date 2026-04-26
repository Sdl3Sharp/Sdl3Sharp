namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the intended usage of a GPU transfer buffer
/// </summary>
/// <remarks>
/// <para>
/// Note that mapping and copying <em>from</em> an <see cref="Upload"/> buffer or <em>to</em> a <see cref="Download"/> buffer is undefined behavior.
/// </para>
/// </remarks>
public enum TransferBufferUsage
{
	/// <summary>The buffer is used for uploading data to the GPU</summary>
	Upload,

	/// <summary>The buffer is used for downloading data from the GPU</summary>
	Download
}
