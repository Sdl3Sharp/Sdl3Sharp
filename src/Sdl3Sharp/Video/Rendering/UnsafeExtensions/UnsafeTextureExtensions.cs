namespace Sdl3Sharp.Video.Rendering.UnsafeExtensions;

/// <summary>
/// Provides unsafe extension methods and properties for <see cref="Texture"/>
/// </summary>
public static class UnsafeTextureExtensions
{
	extension<TTexture>(TTexture texture)
		where TTexture : Texture
	{
		/// <summary>
		/// Gets the pointer to the underlying native <c>SDL_Texture</c> instance.
		/// </summary>
		/// <value>
		/// The pointer to the underlying native <c>SDL_Texture</c> instance.
		/// </value>
		/// <remarks>
		/// <para>
		/// <em>Be very cautious</em> about what you do with the native instance.
		/// Improper use can result in desynchronization between the managed <see cref="Texture"/> object and the underlying native instance,
		/// undefined behavior, memory leaks, crashes, and other severe issues!
		/// Do never, under no circumstances, call <c>SDL_DestroyTexture</c> on the native instance or free its memory!
		/// Always call the managed <see cref="Texture.Dispose()"/> method to properly dispose of the texture and free its resources!
		/// </para>
		/// <para>
		/// If you use this property, we can't guarantee any safety or stability. The responsibility is entirely on you!
		/// </para>
		/// </remarks>
		public unsafe void* UnsafeNativePointer => texture.Pointer;
	}
}
