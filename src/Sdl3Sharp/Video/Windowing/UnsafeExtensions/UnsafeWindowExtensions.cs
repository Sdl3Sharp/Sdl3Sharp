namespace Sdl3Sharp.Video.Windowing.UnsafeExtensions;

/// <summary>
/// Provides unsafe extension methods and properties for <see cref="Window"/>
/// </summary>
public static class UnsafeWindowExtensions
{
	extension<TWindow>(TWindow window)
		where TWindow : Window
	{
		/// <summary>
		/// Gets the pointer to the underlying native <c>SDL_Window</c> instance.
		/// </summary>
		/// <value>
		/// The pointer to the underlying native <c>SDL_Window</c> instance.
		/// </value>
		/// <remarks>
		/// <para>
		/// <em>Be very cautious</em> about what you do with the native instance.
		/// Improper use can result in desynchronization between the managed <see cref="Window"/> object and the underlying native instance,
		/// undefined behavior, memory leaks, crashes, and other severe issues!
		/// Do never, under no circumstances, call <c>SDL_DestroyWindow</c> on the native instance or free its memory!
		/// Always call the managed <see cref="Window.Dispose()"/> method to properly dispose of the window and free its resources!
		/// </para>
		/// <para>
		/// If you use this property, we can't guarantee any safety or stability. The responsibility is entirely on you!
		/// </para>
		/// </remarks>
		public unsafe void* UnsafeNativePointer => window.Pointer;
	}
}
