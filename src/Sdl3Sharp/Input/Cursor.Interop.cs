using Sdl3Sharp.Internal.Interop;
using Sdl3Sharp.SourceGeneration;
using Sdl3Sharp.Video;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sdl3Sharp.Input;

partial class Cursor
{
	// used for opaque pointers
	[StructLayout(LayoutKind.Sequential, Size = 0)]
	internal readonly struct SDL_Cursor;

#if SDL3_4_0_OR_GREATER

	/// <summary>
	/// Creates an animated color cursor
	/// </summary>
	/// <param name="frames">An array of cursor images composing the animation</param>
	/// <param name="frame_count">The number of frames in the sequence</param>
	/// <param name="hot_x">The x position of the cursor hot spot.</param>
	/// <param name="hot_y">The y position of the cursor hot spot</param>
	/// <returns>Returns the new cursor on success or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// Animated cursors are composed of a sequential array of frames, specified as surfaces and durations in an array of <see href="https://wiki.libsdl.org/SDL3/SDL_CursorFrameInfo">SDL_CursorFrameInfo</see> structs.
	/// The hot spot coordinates are universal to all frames, and all frames must have the same dimensions.
	/// </para>
	/// <para>
	/// Frame durations are specified in milliseconds.
	/// A duration of 0 implies an infinite frame time, and the animation will stop on that frame.
	/// To create a one-shot animation, set the duration of the last frame in the sequence to 0.
	/// </para>
	/// <para>
	/// If this function is passed surfaces with alternate representations added with <see href="https://wiki.libsdl.org/SDL3/SDL_AddSurfaceAlternateImage">SDL_AddSurfaceAlternateImage</see>(), the surfaces will be interpreted as the content to be used for 100% display scale, and the alternate representations will be used for high DPI situations.
	/// For example, if the original surfaces are 32x32, then on a 2x macOS display or 200% display scale on Windows, a 64x64 version of the image will be used, if available.
	/// If a matching version of the image isn't available, the closest larger size image will be downscaled to the appropriate size and be used instead, if available.
	/// Otherwise, the closest smaller image will be upscaled and be used instead.
	/// </para>
	/// <para>
	/// If the underlying platform does not support animated cursors, this function will fall back to creating a static color cursor using the first frame in the sequence.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateAnimatedCursor">SDL_CreateAnimatedCursor</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Cursor* SDL_CreateAnimatedCursor(CursorFrameInfo* frames, int frame_count, int hot_x, int hot_y);

#endif

	/// <summary>
	/// Creates a color cursor
	/// </summary>
	/// <param name="surface">An <see href="https://wiki.libsdl.org/SDL3/SDL_Surface">SDL_Surface</see> structure representing the cursor image</param>
	/// <param name="hot_x">The x position of the cursor hot spot</param>
	/// <param name="hot_y">The y position of the cursor hot spot</param>
	/// <returns>Returns the new cursor on success or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// If this function is passed a surface with alternate representations added with <see href="https://wiki.libsdl.org/SDL3/SDL_AddSurfaceAlternateImage">SDL_AddSurfaceAlternateImage</see>(), the surface will be interpreted as the content to be used for 100% display scale,
	/// and the alternate representations will be used for high DPI situations if <see href="https://wiki.libsdl.org/SDL3/SDL_HINT_MOUSE_DPI_SCALE_CURSORS">SDL_HINT_MOUSE_DPI_SCALE_CURSORS</see> is enabled.
	/// For example, if the original surface is 32x32, then on a 2x macOS display or 200% display scale on Windows, a 64x64 version of the image will be used, if available.
	/// If a matching version of the image isn't available, the closest larger size image will be downscaled to the appropriate size and be used instead, if available.
	/// Otherwise, the closest smaller image will be upscaled and be used instead.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateColorCursor">SDL_CreateColorCursor</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Cursor* SDL_CreateColorCursor(Surface.SDL_Surface* surface, int hot_x, int hot_y);

	/// <summary>
	/// Creates a cursor using the specified bitmap data and mask (in MSB format)
	/// </summary>
	/// <param name="data">The color value for each pixel of the cursor</param>
	/// <param name="mask">The mask value for each pixel of the cursor</param>
	/// <param name="w">The width of the cursor</param>
	/// <param name="h">The height of the cursor</param>
	/// <param name="hot_x">The x-axis offset from the left of the cursor image to the mouse x position, in the range of 0 to <c><paramref name="w"/></c> - 1</param>
	/// <param name="hot_y">The y-axis offset from the top of the cursor image to the mouse y position, in the range of 0 to <c><paramref name="h"/></c> - 1</param>
	/// <returns>Returns a new cursor with the specified parameters on success or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information.</returns>
	/// <remarks>
	/// <para>
	/// <c><paramref name="mask"/></c> has to be in MSB (Most Significant Bit) format.
	/// </para>
	/// <para>
	/// The cursor width (<c><paramref name="w"/></c>) must be a multiple of 8 bits.
	/// </para>
	/// <para>
	/// The cursor is created in black and white according to the following:
	/// <list type="bullet">
	///		<item>
	///			<term>data=0, mask=1</term>
	///			<description>White</description>
	///		</item>
	///		<item>
	///			<term>data=1, mask=1</term>
	///			<description>Black</description>
	///		</item>
	///		<item>
	///			<term>data=0, mask=0</term>
	///			<description>Transparent</description>
	///		</item>
	///		<item>
	///			<term>data=1, mask=0</term>
	///			<description>Inverted color if possible, black if not</description>
	///		</item>
	/// </list>
	/// </para>
	/// <para>
	/// Cursors created with this function must be freed with <see href="https://wiki.libsdl.org/SDL3/SDL_DestroyCursor">SDL_DestroyCursor</see>().
	/// </para>
	/// <para>
	/// If you want to have a color cursor, or create your cursor from an <see href="https://wiki.libsdl.org/SDL3/SDL_Surface">SDL_Surface</see>, you should use <see href="https://wiki.libsdl.org/SDL3/SDL_CreateColorCursor">SDL_CreateColorCursor</see>().
	/// Alternately, you can hide the cursor and draw your own as part of your game's rendering, but it will be bound to the framerate.
	/// </para>
	/// <para>
	/// Also, <see href="https://wiki.libsdl.org/SDL3/SDL_CreateSystemCursor">SDL_CreateSystemCursor</see>() is available, which provides several readily-available system cursors to pick from.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateCursor">SDL_CreateCursor</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Cursor* SDL_CreateCursor(byte* data, byte* mask, int w, int h, int hot_x, int hot_y);

	/// <summary>
	/// Creates a system cursor
	/// </summary>
	/// <param name="id">An <see href="https://wiki.libsdl.org/SDL3/SDL_SystemCursor">SDL_SystemCursor</see> enum value</param>
	/// <returns>Returns a cursor on success or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CreateSystemCursor">SDL_CreateSystemCursor</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Cursor* SDL_CreateSystemCursor(SystemCursor id);

	/// <summary>
	/// Returns whether the cursor is currently being shown
	/// </summary>
	/// <returns>Returns <c>true</c> if the cursor is being shown, or <c>false</c> if the cursor is hidden</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_CursorVisible">SDL_CursorVisible</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial CBool SDL_CursorVisible();

	/// <summary>
	/// Frees a previously-created cursor
	/// </summary>
	/// <param name="cursor">The cursor to free</param>
	/// <remarks>
	/// <para>
	/// Use this function to free cursor resources created with <see href="https://wiki.libsdl.org/SDL3/SDL_CreateCursor">SDL_CreateCursor</see>, <see href="https://wiki.libsdl.org/SDL3/SDL_CreateColorCursor">SDL_CreateColorCursor</see> or <see href="https://wiki.libsdl.org/SDL3/SDL_CreateSystemCursor">SDL_CreateSystemCursor</see>.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_DestroyCursor">SDL_DestroyCursor</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial void SDL_DestroyCursor(SDL_Cursor* cursor);

	/// <summary>
	/// Gets the active cursor
	/// </summary>
	/// <returns>Returns the active cursor or NULL if there is no mouse</returns>
	/// <remarks>
	/// <para>
	/// This function returns a pointer to the current cursor which is owned by the library.
	/// It is not necessary to free the cursor with <see href="https://wiki.libsdl.org/SDL3/SDL_DestroyCursor">SDL_DestroyCursor</see>().
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetCursor">SDL_GetCursor</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Cursor* SDL_GetCursor();

	/// <summary>
	/// Gets the default cursor
	/// </summary>
	/// <returns>Returns the default cursor on success or NULL on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// You do not have to call <see href="https://wiki.libsdl.org/SDL3/SDL_DestroyCursor">SDL_DestroyCursor</see>() on the return value, but it is safe to do so.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_GetDefaultCursor">SDL_GetDefaultCursor</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial SDL_Cursor* SDL_GetDefaultCursor();

	/// <summary>
	/// Hides the cursor.
	/// </summary>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_HideCursor">SDL_HideCursor</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal static partial CBool SDL_HideCursor();

	/// <summary>
	/// Sets the active cursor
	/// </summary>
	/// <param name="cursor">A cursor to make active</param>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>>
	/// This function sets the currently active cursor to the specified one. If the cursor is currently visible, the change will be immediately represented on the display.
	/// <see href="https://wiki.libsdl.org/SDL3/SDL_SetCursor">SDL_SetCursor</see>(NULL) can be used to force cursor redraw, if this is desired for any reason.
	/// </para>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_SetCursor">SDL_SetCursor</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_SetCursor(SDL_Cursor* cursor);

	/// <summary>
	/// Shows the cursor
	/// </summary>
	/// <returns>Returns true on success or false on failure; call <see href="https://wiki.libsdl.org/SDL3/SDL_GetError">SDL_GetError</see>() for more information</returns>
	/// <remarks>
	/// <para>
	/// This function should only be called on the main thread.
	/// </para>
	/// </remarks>
	/// <seealso href="https://wiki.libsdl.org/SDL3/SDL_ShowCursor">SDL_ShowCursor</seealso>
	[NativeImportFunction<Library>(CallConvs = [typeof(CallConvCdecl)])]
	internal unsafe static partial CBool SDL_ShowCursor();
}
