using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Video.Gpu;

public sealed partial class Shader
{
	private unsafe SDL_GPUShader* mShader;

	internal unsafe SDL_GPUShader* Pointer { [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)] get => mShader; }

	// TODO: IMPLEMENT!
	internal unsafe static bool TryGetOrCreate(SDL_GPUShader* shader, [NotNullWhen(true)] out Shader? result)
	{
		result = null;
		return false;
	}
}
