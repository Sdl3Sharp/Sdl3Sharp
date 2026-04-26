using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Gpu;

// TODO: make 'abstract' if we want to introduce GpuTexture<TDriver> inheriting from this,
//       or make 'sealed' otherwise
public partial class GpuTexture
{
	private unsafe SDL_GPUTexture* mTexture;

	internal unsafe SDL_GPUTexture* Pointer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mTexture; }

	// TODO: IMPLEMENT!
	internal unsafe static bool TryGetOrCreate(SDL_GPUTexture* texture, [NotNullWhen(true)] out GpuTexture? result)
	{
		result = null;
		return false;
	}
}
