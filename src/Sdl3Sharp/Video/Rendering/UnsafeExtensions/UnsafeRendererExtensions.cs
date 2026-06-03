namespace Sdl3Sharp.Video.Rendering.UnsafeExtensions;

/// <summary>
/// Provides unsafe extension methods and properties for <see cref="Renderer"/>
/// </summary>
public static class UnsafeRendererExtensions
{
	extension<TRenderer>(TRenderer renderer)
		where TRenderer : Renderer
	{
		/// <summary>
		/// Gets the pointer to the underlying native <c>SDL_Renderer</c> instance.
		/// </summary>
		/// <value>
		/// The pointer to the underlying native <c>SDL_Renderer</c> instance.
		/// </value>
		/// <remarks>
		/// <para>
		/// <em>Be very cautious</em> about what you do with the native instance.
		/// Improper use can result in desynchronization between the managed <see cref="Renderer"/> object and the underlying native instance,
		/// undefined behavior, memory leaks, crashes, and other severe issues!
		/// Do never, under no circumstances, call <c>SDL_DestroyRenderer</c> on the native instance or free its memory!
		/// Always call the managed <see cref="Renderer.Dispose()"/> method to properly dispose of the renderer and free its resources!
		/// </para>
		/// <para>
		/// If you use this property, we can't guarantee any safety or stability. The responsibility is entirely on you!
		/// </para>
		/// </remarks>
		public unsafe void* UnsafeNativePointer => renderer.Pointer;
	}
}
