using Sdl3Sharp.Internal;
using Sdl3Sharp.Utilities;
using Sdl3Sharp.Video;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Sdl3Sharp.Input;

/// <summary>
/// Represents a mouse cursor
/// </summary>
/// <remarks>
/// <para>
/// For the most part, <see cref="Cursor"/> is not thread-safe, and most of its properties and methods should only be accessed from the main thread.
/// </para>
/// </remarks>
public sealed partial class Cursor : IDisposable
{
	private interface IUnsafeConstructorDispatch;

	private static readonly ConcurrentDictionary<IntPtr, WeakReference<Cursor>> mKnownInstances = [];

	private unsafe SDL_Cursor* mCursor;

	private unsafe Cursor(SDL_Cursor* cursor, bool register)
	{
		mCursor = cursor;

		if (register && cursor is not null)
		{
			mKnownInstances.AddOrUpdate(unchecked((IntPtr)cursor), addRef, updateRef, this);

			static WeakReference<Cursor> addRef(IntPtr cursor, Cursor newCursor) => new(newCursor);

			static WeakReference<Cursor> updateRef(IntPtr cursor, WeakReference<Cursor> existingCursorRef, Cursor newCursor)
			{
				if (existingCursorRef.TryGetTarget(out Cursor? existingCursor))
				{
#pragma warning disable IDE0079
#pragma warning disable CA1816
					GC.SuppressFinalize(existingCursor);
#pragma warning restore CA1816
#pragma warning restore IDE0079

					existingCursor.Dispose(forget: false);
				}

				existingCursorRef.SetTarget(newCursor);

				return existingCursorRef;
			}
		}
	}

	/// <exception cref="SdlException">The <see cref="Cursor"/> could not be created (check <see cref="Error.TryGet(out string?)"/> for more information)</exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	[return: NotNull]
	private unsafe static SDL_Cursor* ValidateCursor([NotNull] SDL_Cursor* cursor)
	{
		if (cursor is null)
		{
			[DoesNotReturn]
			static void failCouldNotCreateCursor() => throw new SdlException($"Could not create {nameof(Cursor)}");

			failCouldNotCreateCursor();
		}

		return cursor;
	}

#if SDL3_4_0_OR_GREATER

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private unsafe static SDL_Cursor* CreateAnimated(ReadOnlySpan<CursorFrameInfo> frames, int hotSpotX, int hotSpotY)
	{
		fixed (CursorFrameInfo* framesPtr = frames)
		{
			return SDL_CreateAnimatedCursor(framesPtr, frames.Length, hotSpotX, hotSpotY);
		}
	}

	/// <inheritdoc cref="ValidateCursor(SDL_Cursor*)"/>
	private unsafe Cursor(ReadOnlySpan<CursorFrameInfo> frames, int hotSpotX, int hotSpotY, IUnsafeConstructorDispatch? _ = default) :
		this(ValidateCursor(CreateAnimated(frames, hotSpotX, hotSpotY)), register: true)
	{ }

	/// <summary>
	/// Creates a new animated <see cref="Cursor"/> from a sequence of frames
	/// </summary>
	/// <param name="frames">The sequence of frames composing the animation</param>
	/// <param name="hotSpotX">The horizontal coordinate within the cursor image for the cursor's hot spot</param>
	/// <param name="hotSpotY">The vertical coordinate within the cursor image for the cursor's hot spot</param>
	/// <remarks>
	/// <para>
	/// Animated cursors are composed of a sequence of frames, specified as surfaces and durations as <see cref="CursorFrameInfo"/>.
	/// The hot spot coordinates are universal to all frames, and all frames must have the same dimensions.
	/// </para>
	/// <para>
	/// Frame durations are specified in milliseconds.
	/// A duration of <c>0</c> implies an infinite frame time, and the animation will stop on that frame.
	/// To create a one-shot animation, set the duration of the last frame in the sequence to <c>0</c>.
	/// </para>
	/// <para>
	/// If any of the given <paramref name="frames"/> has <see cref="Surface.Images">alternate representations</see> <see cref="Surface.TryAddAlternateImage(Surface)">added</see> to their <see cref="CursorFrameInfo.Surface">surfaces</see>,
	/// such a surface will be interpreted as the content to be used for 100% display scale, and the alternate representations will be used for high DPI situations if the hint <see cref="Hint.Mouse.DpiScaleCursors"/> is enabled.
	/// For example, if the original surface is 32⨯32 , then on a 2x macOS display or 200% display scale on Windows, a 64⨯64 version of the image will be used, if available.
	/// If a matching version of the image isn't available, the closest larger size image will be downscaled to the appropriate size and be used instead, if available.
	/// Otherwise, the closest smaller image will be upscaled and be used instead.
	/// </para>
	/// <para>
	/// If the underlying platform does not support animated cursors, this constructor will fall back to creating a static cursor using the first frame in the sequence.
	/// </para>
	/// <para>
	/// This constructor should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="Cursor(ReadOnlySpan{CursorFrameInfo}, int, int, IUnsafeConstructorDispatch?)"/>
	public Cursor(ReadOnlySpan<CursorFrameInfo> frames, int hotSpotX, int hotSpotY) :
#pragma warning disable IDE0034
		this(frames, hotSpotX, hotSpotY, default(IUnsafeConstructorDispatch?))
#pragma warning restore IDE0034
	{ }

#endif

	/// <inheritdoc cref="ValidateCursor(SDL_Cursor*)"/>
	private unsafe Cursor(Surface surface, int hotSpotX, int hotSpotY, IUnsafeConstructorDispatch? _ = default) :
		this(ValidateCursor(SDL_CreateColorCursor(surface is not null ? surface.Pointer : null, hotSpotX, hotSpotY)), register: true)
	{ }

	/// <summary>
	/// Creates a new <see cref="Cursor"/> from a <see cref="Surface"/>
	/// </summary>
	/// <param name="surface">The <see cref="Surface"/> containing the cursor image</param>
	/// <param name="hotSpotX">The horizontal coordinate within the given <paramref name="surface"/> for the cursor's hot spot</param>
	/// <param name="hotSpotY">The vertical coordinate within the given <paramref name="surface"/> for the cursor's hot spot</param>
	/// <remarks>
	/// <para>
	/// If the given <paramref name="surface"/> has <see cref="Surface.Images">alternate representations</see> <see cref="Surface.TryAddAlternateImage(Surface)">added</see>,
	/// the surface will be interpreted as the content to be used for 100% display scale, and the alternate representations will be used for high DPI situations if the hint <see cref="Hint.Mouse.DpiScaleCursors"/> is enabled.
	/// For example, if the original surface is 32⨯32 , then on a 2x macOS display or 200% display scale on Windows, a 64⨯64 version of the image will be used, if available.
	/// If a matching version of the image isn't available, the closest larger size image will be downscaled to the appropriate size and be used instead, if available.
	/// Otherwise, the closest smaller image will be upscaled and be used instead.
	/// </para>
	/// <para>
	/// This constructor should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="Cursor(Surface, int, int, IUnsafeConstructorDispatch?)"/>
	public Cursor(Surface surface, int hotSpotX, int hotSpotY) :
#pragma warning disable IDE0034
		this(surface, hotSpotX, hotSpotY, default(IUnsafeConstructorDispatch?))
#pragma warning restore IDE0034
	{ }

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
#pragma warning disable IDE0060
	private static void ValidateDataAndMaskLength<T>(int width, int height, ulong dataLength, ulong maskLength, T data, T mask, [CallerArgumentExpression(nameof(data))] string? dataArgumentExpression = default, [CallerArgumentExpression(nameof(mask))] string? maskArgumentExpression = default)
#pragma warning restore IDE0060
		where T : allows ref struct
	{
		if (width is < 0 || (width % 8) is not 0)
		{
			[DoesNotReturn]
			static void failInvalidWidth() => throw new ArgumentException($"{nameof(width)} must be a non-negative multiple of 8", nameof(width));

			failInvalidWidth();
		}

		if (height is < 0)
		{
			static void failInvalidHeight() => throw new ArgumentException($"{nameof(height)} must be non-negative", nameof(height));

			failInvalidHeight();
		}

		var requiredLength = unchecked((ulong)width * (ulong)height) / 8;

		if (dataLength < requiredLength)
		{
			[DoesNotReturn]
			static void failDataTooShort(string? dataArgumentExpression) => throw new ArgumentException($"{dataArgumentExpression} is too short", nameof(data));

			failDataTooShort(dataArgumentExpression);
		}

		if (maskLength < requiredLength)
		{
			[DoesNotReturn]
			static void failMaskTooShort(string? maskArgumentExpression) => throw new ArgumentException($"{maskArgumentExpression} is too short", nameof(mask));

			failMaskTooShort(maskArgumentExpression);
		}
	}

	/// <exception cref="ArgumentException">
	/// <paramref name="data"/> is not <see cref="ReadOnlyNativeMemory{T}.IsValid">valid</see>
	/// - or -
	/// <paramref name="mask"/> is not <see cref="ReadOnlyNativeMemory{T}.IsValid">valid</see>
	/// - or -
	/// <paramref name="width"/> is negative or not a multiple of <c>8</c>
	/// - or -
	/// <paramref name="height"/> is negative
	/// - or -
	/// <paramref name="data"/> is too short for the given <paramref name="width"/> and <paramref name="height"/>
	/// - or -
	/// <paramref name="mask"/> is too short for the given <paramref name="width"/> and <paramref name="height"/>
	/// </exception>
	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
	private unsafe static SDL_Cursor* Create(ReadOnlyNativeMemory<byte> data, ReadOnlyNativeMemory<byte> mask, int width, int height, int hotSpotX, int hotSpotY)
	{
		if (!data.IsValid)
		{
			[DoesNotReturn]
			static void failInvalidData() => throw new ArgumentException($"{nameof(data)} is not valid", nameof(data));

			failInvalidData();
		}

		if (!mask.IsValid)
		{
			[DoesNotReturn]
			static void failInvalidMask() => throw new ArgumentException($"{nameof(mask)} is not valid", nameof(mask));

			failInvalidMask();
		}

		ValidateDataAndMaskLength(width, height, data.Length, mask.Length, data, mask);

		return SDL_CreateCursor(data.RawPointer, mask.RawPointer, width, height, hotSpotX, hotSpotY);
	}

	/// <inheritdoc cref="Create(ReadOnlyNativeMemory{byte}, ReadOnlyNativeMemory{byte}, int, int, int, int)"/>
	/// <inheritdoc cref="ValidateCursor(SDL_Cursor*)"/>
	private unsafe Cursor(ReadOnlyNativeMemory<byte> data, ReadOnlyNativeMemory<byte> mask, int width, int height, int hotSpotX, int hotSpotY, IUnsafeConstructorDispatch? _ = default) :
		this(ValidateCursor(Create(data, mask, width, height, hotSpotX, hotSpotY)), register: true)
	{ }

	/// <summary>
	/// Creates a new <see cref="Cursor"/> from a data and mask bitmap
	/// </summary>
	/// <param name="data">The color values for each pixel of the cursor, in MSB format</param>
	/// <param name="mask">The mask values for each pixel of the cursor, in MSB format</param>
	/// <param name="width">The width of the cursor in pixels</param>
	/// <param name="height">The height of the cursor in pixels</param>
	/// <param name="hotSpotX">The horizontal coordinate within the cursor's bitmaps of the cursor's hot spot; must be between <c>0</c> and <c><paramref name="width"/> - 1</c></param>
	/// <param name="hotSpotY">The vertical coordinate within the cursor's bitmaps of the cursor's hot spot; must be between <c>0</c> and <c><paramref name="height"/> - 1</c></param>
	/// <remarks>
	/// <para>
	/// The given bitmaps, <paramref name="data"/> and <paramref name="mask"/>, must both be in MSB format, and each pixel is represented by a single bit from both bitmaps.
	/// </para>
	/// <para>
	/// The cursor's <paramref name="width"/> must be a multiple of <c>8</c>.
	/// </para>
	/// <para>
	/// The resulting pixel data of the cursor will be determined by the following rules:
	/// <list type="bullet">
	/// <item>
	/// <term><paramref name="data"/> bit is <c>0</c> and <paramref name="mask"/> bit is <c>0</c></term>
	/// <description>transparent pixel</description>
	/// </item>
	/// <item>
	/// <term><paramref name="data"/> bit is <c>0</c> and <paramref name="mask"/> bit is <c>1</c></term>
	/// <description>white pixel</description>
	/// </item>
	/// <item>
	/// <term><paramref name="data"/> bit is <c>1</c> and <paramref name="mask"/> bit is <c>0</c></term>
	/// <description>invert color, if possible; otherwise, black pixel</description>
	/// </item>
	/// <item>
	/// <term><paramref name="data"/> bit is <c>1</c> and <paramref name="mask"/> bit is <c>1</c></term>
	/// <description>black pixel</description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// Alternatively, you can use the <see cref="Cursor(Surface, int, int)"/> constructor to create a cursor from a colored <see cref="Surface"/>,
	/// or <see cref="Cursor(SystemCursor)"/> to create a cursor from a system cursor.
	/// </para>
	/// <para>
	/// You can also try to <see cref="IsVisible">hide</see> the cursor and draw your own cursor using regular rendering methods, but that will be bound to the framerate of your application.
	/// </para>
	/// <para>
	/// This constructor should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="Cursor(ReadOnlyNativeMemory{byte}, ReadOnlyNativeMemory{byte}, int, int, int, int, IUnsafeConstructorDispatch?)"/>
	public Cursor(ReadOnlyNativeMemory<byte> data, ReadOnlyNativeMemory<byte> mask, int width, int height, int hotSpotX, int hotSpotY) :
#pragma warning disable IDE0034
		this(data, mask, width, height, hotSpotX, hotSpotY, default(IUnsafeConstructorDispatch?))
#pragma warning restore IDE0034
	{ }

	/// <exception cref="ArgumentException">
	/// <paramref name="width"/> is negative or not a multiple of <c>8</c>
	/// - or -
	/// <paramref name="height"/> is negative
	/// - or -
	/// <paramref name="data"/> is too short for the given <paramref name="width"/> and <paramref name="height"/>
	/// - or -
	/// <paramref name="mask"/> is too short for the given <paramref name="width"/> and <paramref name="height"/>
	/// </exception>
	private unsafe static SDL_Cursor* Create(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mask, int width, int height, int hotSpotX, int hotSpotY)
	{
		ValidateDataAndMaskLength(width, height, unchecked((ulong)data.Length), unchecked((ulong)mask.Length), data, mask);

		fixed (byte* dataPtr = data)
		fixed (byte* maskPtr = mask)
		{
			return SDL_CreateCursor(dataPtr, maskPtr, width, height, hotSpotX, hotSpotY);
		}
	}

	/// <inheritdoc cref="Create(ReadOnlySpan{byte}, ReadOnlySpan{byte}, int, int, int, int)"/>
	/// <inheritdoc cref="ValidateCursor(SDL_Cursor*)"/>
	private unsafe Cursor(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mask, int width, int height, int hotSpotX, int hotSpotY, IUnsafeConstructorDispatch? _ = default) :
		this(ValidateCursor(Create(data, mask, width, height, hotSpotX, hotSpotY)), register: true)
	{ }

	/// <summary>
	/// Creates a new <see cref="Cursor"/> from a data and mask bitmap
	/// </summary>
	/// <param name="data">The color values for each pixel of the cursor, in MSB format</param>
	/// <param name="mask">The mask values for each pixel of the cursor, in MSB format</param>
	/// <param name="width">The width of the cursor in pixels</param>
	/// <param name="height">The height of the cursor in pixels</param>
	/// <param name="hotSpotX">The horizontal coordinate within the cursor's bitmaps of the cursor's hot spot; must be between <c>0</c> and <c><paramref name="width"/> - 1</c></param>
	/// <param name="hotSpotY">The vertical coordinate within the cursor's bitmaps of the cursor's hot spot; must be between <c>0</c> and <c><paramref name="height"/> - 1</c></param>
	/// <remarks>
	/// <para>
	/// The given bitmaps, <paramref name="data"/> and <paramref name="mask"/>, must both be in MSB format, and each pixel is represented by a single bit from both bitmaps.
	/// </para>
	/// <para>
	/// The cursor's <paramref name="width"/> must be a multiple of <c>8</c>.
	/// </para>
	/// <para>
	/// The resulting pixel data of the cursor will be determined by the following rules:
	/// <list type="bullet">
	/// <item>
	/// <term><paramref name="data"/> bit is <c>0</c> and <paramref name="mask"/> bit is <c>0</c></term>
	/// <description>transparent pixel</description>
	/// </item>
	/// <item>
	/// <term><paramref name="data"/> bit is <c>0</c> and <paramref name="mask"/> bit is <c>1</c></term>
	/// <description>white pixel</description>
	/// </item>
	/// <item>
	/// <term><paramref name="data"/> bit is <c>1</c> and <paramref name="mask"/> bit is <c>0</c></term>
	/// <description>invert color, if possible; otherwise, black pixel</description>
	/// </item>
	/// <item>
	/// <term><paramref name="data"/> bit is <c>1</c> and <paramref name="mask"/> bit is <c>1</c></term>
	/// <description>black pixel</description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// Alternatively, you can use the <see cref="Cursor(Surface, int, int)"/> constructor to create a cursor from a colored <see cref="Surface"/>,
	/// or <see cref="Cursor(SystemCursor)"/> to create a cursor from a system cursor.
	/// </para>
	/// <para>
	/// You can also try to <see cref="IsVisible">hide</see> the cursor and draw your own cursor using regular rendering methods, but that will be bound to the framerate of your application.
	/// </para>
	/// <para>
	/// This constructor should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="Cursor(ReadOnlySpan{byte}, ReadOnlySpan{byte}, int, int, int, int, IUnsafeConstructorDispatch?)"/>
	public Cursor(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mask, int width, int height, int hotSpotX, int hotSpotY) :
#pragma warning disable IDE0034
		this(data, mask, width, height, hotSpotX, hotSpotY, default(IUnsafeConstructorDispatch?))
#pragma warning restore IDE0034
	{ }

	/// <summary>
	/// Creates a new <see cref="Cursor"/> from a data and mask bitmap
	/// </summary>
	/// <param name="data">A pointer to the color values for each pixel of the cursor, in MSB format</param>
	/// <param name="mask">A pointer to the mask values for each pixel of the cursor, in MSB format</param>
	/// <param name="width">The width of the cursor in pixels</param>
	/// <param name="height">The height of the cursor in pixels</param>
	/// <param name="hotSpotX">The horizontal coordinate within the cursor's bitmaps of the cursor's hot spot; must be between <c>0</c> and <c><paramref name="width"/> - 1</c></param>
	/// <param name="hotSpotY">The vertical coordinate within the cursor's bitmaps of the cursor's hot spot; must be between <c>0</c> and <c><paramref name="height"/> - 1</c></param>
	/// <remarks>
	/// <para>
	/// The given bitmaps, <paramref name="data"/> and <paramref name="mask"/>, must both be in MSB format, and each pixel is represented by a single bit from both bitmaps.
	/// </para>
	/// <para>
	/// The cursor's <paramref name="width"/> must be a multiple of <c>8</c>.
	/// </para>
	/// <para>
	/// The resulting pixel data of the cursor will be determined by the following rules:
	/// <list type="bullet">
	/// <item>
	/// <term><paramref name="data"/> bit is <c>0</c> and <paramref name="mask"/> bit is <c>0</c></term>
	/// <description>transparent pixel</description>
	/// </item>
	/// <item>
	/// <term><paramref name="data"/> bit is <c>0</c> and <paramref name="mask"/> bit is <c>1</c></term>
	/// <description>white pixel</description>
	/// </item>
	/// <item>
	/// <term><paramref name="data"/> bit is <c>1</c> and <paramref name="mask"/> bit is <c>0</c></term>
	/// <description>invert color, if possible; otherwise, black pixel</description>
	/// </item>
	/// <item>
	/// <term><paramref name="data"/> bit is <c>1</c> and <paramref name="mask"/> bit is <c>1</c></term>
	/// <description>black pixel</description>
	/// </item>
	/// </list>
	/// </para>
	/// <para>
	/// Alternatively, you can use the <see cref="Cursor(Surface, int, int)"/> constructor to create a cursor from a colored <see cref="Surface"/>,
	/// or <see cref="Cursor(SystemCursor)"/> to create a cursor from a system cursor.
	/// </para>
	/// <para>
	/// You can also try to <see cref="IsVisible">hide</see> the cursor and draw your own cursor using regular rendering methods, but that will be bound to the framerate of your application.
	/// </para>
	/// <para>
	/// This constructor should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="ValidateCursor(SDL_Cursor*)"/>
	public unsafe Cursor(byte* data, byte* mask, int width, int height, int hotSpotX, int hotSpotY) :
		this(ValidateCursor(SDL_CreateCursor(data, mask, width, height, hotSpotX, hotSpotY)), register: true)
	{ }

	/// <inheritdoc cref="ValidateCursor(SDL_Cursor*)"/>
	private unsafe Cursor(SystemCursor cursor, IUnsafeConstructorDispatch? _ = default) :
		this(ValidateCursor(SDL_CreateSystemCursor(cursor)), register: true)
	{ }

	/// <summary>
	/// Creates a new <see cref="Cursor"/> from a system cursor
	/// </summary>
	/// <param name="cursor">The system cursor to create the <see cref="Cursor"/> from</param>
	/// <remarks>
	/// <para>
	/// This constructor should only be called from the main thread.
	/// </para>
	/// </remarks>
	/// <inheritdoc cref="Cursor(SystemCursor, IUnsafeConstructorDispatch?)"/>
	public Cursor(SystemCursor cursor) :
#pragma warning disable IDE0034
		this(cursor, default(IUnsafeConstructorDispatch?))
#pragma warning restore IDE0034
	{ }

	/// <inheritdoc/>
	~Cursor() => Dispose(forget: true);

	/// <summary>
	/// Gets or sets the currently active cursor
	/// </summary>
	/// <value>
	/// The currently active cursor
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property is not required to be <see cref="Dispose()">disposed</see> as it's owned by SDL.
	/// </para>
	/// <para>
	/// If the value of this property is <c><see langword="null"/></c>, it usually means that there's no mouse connected to the system.
	/// </para>
	/// <para>
	/// You can set the value of this property to <c><see langword="null"/></c> to force a cursor redraw, if this desired for any reason.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting this property, the current cursor couldn't be set (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </exception>
	public static Cursor? Current
	{
		get
		{
			unsafe
			{
				TryGetOrCreate(SDL_GetCursor(), out var cursor);
				return cursor;
			}
		}

		set
		{
			unsafe
			{
				SdlErrorHelper.ThrowIfFailed(SDL_SetCursor(value is not null ? value.mCursor : null));
			}
		}
	}

	/// <summary>
	/// Gets the default cursor
	/// </summary>
	/// <value>
	/// The default cursor
	/// </value>
	/// <remarks>
	/// <para>
	/// The value of this property is not required to be <see cref="Dispose()">disposed</see> as it's owned by SDL.
	/// </para>
	/// <para>
	/// If the value of this property is <c><see langword="null"/></c>, it usually means that there's no mouse connected to the system.
	/// </para>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	public static Cursor? Default
	{
		get
		{
			unsafe
			{
				// Despite what the official SDL docs say, SDL_GetDefaultCursor has the same behavior as SDL_GetCursor,
				// returning null if there's no mouse - it returning null is not actually an error condition and SDL won't even set an error in that case.
				// So we just return null in that case (just like Current does) instead of throwing an exception to indicate that there's no mouse for the system.

				TryGetOrCreate(SDL_GetDefaultCursor(), out var cursor);
				return cursor;
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the cursor is visible
	/// </summary>
	/// <value>
	/// A value indicating whether the cursor is visible
	/// </value>
	/// <remarks>
	/// <para>
	/// This property should only be accessed from the main thread.
	/// </para>
	/// </remarks>
	/// <exception cref="SdlException">
	/// When setting this property, the cursor visibility couldn't be changed (check <see cref="Error.TryGet(out string?)"/> for more information)
	/// </exception>
	public static bool IsVisible
	{
		get => SDL_CursorVisible();		
		set => SdlErrorHelper.ThrowIfFailed(value ? SDL_ShowCursor() : SDL_HideCursor());
	}

	/// <inheritdoc/>
	public void Dispose()
	{
		GC.SuppressFinalize(this);
		Dispose(forget: true);
	}

	private void Dispose(bool forget)
	{
		unsafe
		{
			if (mCursor is not null)
			{
				if (forget)
				{
					mKnownInstances.TryRemove(unchecked((IntPtr)mCursor), out _);
				}

				SDL_DestroyCursor(mCursor);
				mCursor = null;
			}
		}
	}

	internal unsafe static bool TryGetOrCreate(SDL_Cursor* cursor, [NotNullWhen(true)] out Cursor? result)
	{
		if (cursor is null)
		{
			result = null;
			return false;
		}

		var surfaceRef = mKnownInstances.GetOrAdd(unchecked((IntPtr)cursor), createRef);

		if (!surfaceRef.TryGetTarget(out result))
		{
			surfaceRef.SetTarget(result = create(cursor));
		}

		return true;

		static WeakReference<Cursor> createRef(IntPtr cursor) => new(create(unchecked((SDL_Cursor*)cursor)));

		static Cursor create(SDL_Cursor* cursor) => new(cursor, register: false);
	}
}
