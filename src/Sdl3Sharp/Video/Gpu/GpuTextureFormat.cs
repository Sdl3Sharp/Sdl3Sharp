namespace Sdl3Sharp.Video.Gpu;

/// <summary>
/// Represents the pixel format of a texture
/// </summary>
public enum GpuTextureFormat
{
	/// <summary>Represents an invalid texture format</summary>
	Invalid,

	#region Unsiged normalized float color formats

	/// <summary>Channels: Alpha: 8 bits; Channel values: Unsigned normalized floats</summary>
	A8UNorm,

	/// <summary>Channels: Red: 8 bits; Channel values: Unsigned normalized floats</summary>
	R8UNorm,

	/// <summary>Channels: Red: 8 bits, Green: 8 bits; Channel values: Unsigned normalized floats</summary>
	R8G8UNorm,

	/// <summary>Channels: Red: 8 bits, Green: 8 bits, Blue: 8 bits, Alpha: 8 bits; Channel values: Unsigned normalized floats</summary>
	R8G8B8A8UNorm,

	/// <summary>Channels: Red: 16 bits; Channel values: Unsigned normalized floats</summary>
	R16UNorm,

	/// <summary>Channels: Red: 16 bits, Green: 16 bits; Channel values: Unsigned normalized floats</summary>
	R16G16UNorm,

	/// <summary>Channels: Red: 16 bits, Green: 16 bits, Blue: 16 bits, Alpha: 16 bits; Channel values: Unsigned normalized floats</summary>
	R16G16B16A16UNorm,

	/// <summary>Channels: Red: 10 bits, Green: 10 bits, Blue: 10 bits, Alpha: 2 bits; Channel values: Unsigned normalized floats</summary>
	R10G10B10A2UNorm,

	/// <summary>Channels: Blue: 5 bits, Green: 6 bits, Red: 5 bits; Channel values: Unsigned normalized floats</summary>
	B5G6R5UNorm,

	/// <summary>Channels: Blue: 5 bits, Green: 5 bits, Red: 5 bits, Alpha: 1 bit; Channel values: Unsigned normalized floats</summary>
	B5G5R5A1UNorm,

	/// <summary>Channels: Blue: 4 bits, Green: 4 bits, Red: 4 bits, Alpha: 4 bits; Channel values: Unsigned normalized floats</summary>
	B4G4R4A4UNorm,

	/// <summary>Channels: Blue: 8 bits, Green: 8 bits, Red: 8 bits, Alpha: 8 bits; Channel values: Unsigned normalized floats</summary>
	/// <remarks>
	/// <para>
	/// The color channels are in reverse order compared to <see cref="R8G8B8A8UNorm"/>.
	/// </para>
	/// </remarks>
	B8G8R8A8UNorm,

	#endregion

	#region Compressed unsigned normalized float color formats

	/// <summary>Block Compression 1: 4x4 blocks; Channels: RGBA with 1-bit alpha (4 bpp); Channel values: Unsigned normalized floats</summary>
	Bc1RgbaUNorm,

	/// <summary>Block Compression 2: 4x4 blocks; Channels: RGBA with explicit (uncompressed) alpha (8 bpp); Channel values: Unsigned normalized floats</summary>
	Bc2RgbaUNorm,
	
	/// <summary>Block Compression 3: 4x4 blocks; Channels: RGBA with interpolated alpha (8 bpp); Channel values: Unsigned normalized floats</summary>
	Bc3RgbaUNorm,

	/// <summary>Block Compression 4: 4x4 blocks; Channels: Red (4 bpp); Channel values: Unsigned normalized floats</summary>
	Bc4RUnorm,

	/// <summary>Block Compression 5: 4x4 blocks; Channels: Red and Green (8 bpp); Channel values: Unsigned normalized floats</summary>
	Bc5RgUNorm,

	/// <summary>Block Compression 7: 4x4 blocks; Channels: High-quality RGBA (8 bpp); Channel values: Unsigned normalized floats</summary>
	Bc7RgbaUNorm,

	#endregion

	#region Compressed signed float color formats

	/// <summary>Block Compression 6: 4x4 blocks; Channels: RGB with HDR color data (8 bpp); Channel values: Signed floats</summary>
	Bc6hRgbFloat,

	#endregion

	#region Compressed unsigned float color formats

	/// <summary>Block Compression 6: 4x4 blocks; Channels: RGB with HDR color data (8 bpp); Channel values: Unsigned floats</summary>
	Bc6hRgbUFloat,

	#endregion

	#region Signed normalized float color formats

	/// <summary>Channels: Red: 8 bits; Channel values: Signed normalized floats</summary>
	R8SNorm,

	/// <summary>Channels: Red: 8 bits, Green: 8 bits; Channel values: Signed normalized floats</summary>
	R8G8SNorm,

	/// <summary>Channels: Red: 8 bits, Green: 8 bits, Blue: 8 bits, Alpha: 8 bits; Channel values: Signed normalized floats</summary>
	R8G8B8A8SNorm,

	/// <summary>Channels: Red: 16 bits; Channel values: Signed normalized floats</summary>
	R16SNorm,

	/// <summary>Channels: Red: 16 bits, Green: 16 bits; Channel values: Signed normalized floats</summary>
	R16G16SNorm,

	/// <summary>Channels: Red: 16 bits, Green: 16 bits, Blue: 16 bits, Alpha: 16 bits; Channel values: Signed normalized floats</summary>
	R16G16B16A16SNorm,

	#endregion

	#region Signed float color formats

	/// <summary>Channels: Red: 16 bits; Channel values: Signed floats</summary>
	R16Float,

	/// <summary>Channels: Red: 16 bits, Green: 16 bits; Channel values: Signed floats</summary>
	R16G16Float,

	/// <summary>Channels: Red: 16 bits, Green: 16 bits, Blue: 16 bits, Alpha: 16 bits; Channel values: Signed floats</summary>
	R16G16B16A16Float,

	/// <summary>Channels: Red: 32 bits; Channel values: Signed floats</summary>
	R32Float,

	/// <summary>Channels: Red: 32 bits, Green: 32 bits; Channel values: Signed floats</summary>
	R32G32Float,

	/// <summary>Channels: Red: 32 bits, Green: 32 bits, Blue: 32 bits, Alpha: 32 bits; Channel values: Signed floats</summary>
	R32G32B32A32Float,

	#endregion

	#region Unsigned float color formats

	/// <summary>Channels: Red: 16 bits; Channel values: Unsigned floats</summary>
	R11G11B10UFloat,

	#endregion

	#region Unsigned integer color formats

	/// <summary>Channels: Red: 8 bits; Channel values: Unsigned integers</summary>
	R8UInt,

	/// <summary>Channels: Red: 8 bits, Green: 8 bits; Channel values: Unsigned integers</summary>
	R8G8UInt,

	/// <summary>Channels: Red: 8 bits, Green: 8 bits, Blue: 8 bits, Alpha: 8 bits; Channel values: Unsigned integers</summary>
	R8G8B8A8UInt,

	/// <summary>Channels: Red: 16 bits; Channel values: Unsigned integers</summary>
	R16UInt,

	/// <summary>Channels: Red: 16 bits, Green: 16 bits; Channel values: Unsigned integers</summary>
	R16G16UInt,

	/// <summary>Channels: Red: 16 bits, Green: 16 bits, Blue: 16 bits, Alpha: 16 bits; Channel values: Unsigned integers</summary>
	R16G16B16A16UInt,

	/// <summary>Channels: Red: 32 bits; Channel values: Unsigned integers</summary>
	R32UInt,

	/// <summary>Channels: Red: 32 bits, Green: 32 bits; Channel values: Unsigned integers</summary>
	R32G32UInt,

	/// <summary>Channels: Red: 32 bits, Green: 32 bits, Blue: 32 bits, Alpha: 32 bits; Channel values: Unsigned integers</summary>
	R32G32B32A32UInt,

	#endregion

	#region Signed integer color formats

	/// <summary>Channels: Red: 8 bits; Channel values: Signed integers</summary>
	R8Int,

	/// <summary>Channels: Red: 8 bits, Green: 8 bits; Channel values: Signed integers</summary>
	R8G8Int,

	/// <summary>Channels: Red: 8 bits, Green: 8 bits, Blue: 8 bits, Alpha: 8 bits; Channel values: Signed integers</summary>
	R8G8B8A8Int,

	/// <summary>Channels: Red: 16 bits; Channel values: Signed integers</summary>
	R16Int,

	/// <summary>Channels: Red: 16 bits, Green: 16 bits; Channel values: Signed integers</summary>
	R16G16Int,

	/// <summary>Channels: Red: 16 bits, Green: 16 bits, Blue: 16 bits, Alpha: 16 bits; Channel values: Signed integers</summary>
	R16G16B16A16Int,

	/// <summary>Channels: Red: 32 bits; Channel values: Signed integers</summary>
	R32Int,

	/// <summary>Channels: Red: 32 bits, Green: 32 bits; Channel values: Signed integers</summary>
	R32G32Int,

	/// <summary>Channels: Red: 32 bits, Green: 32 bits, Blue: 32 bits, Alpha: 32 bits; Channel values: Signed integers</summary>
	R32G32B32A32Int,

	#endregion

	#region sRGB unsigned normalized color formats

	/// <summary>Channels: Red: 8 bits, Green: 8 bits, Blue: 8 bits, Alpha: 8 bits; Channel values: Unsigned normalized floats in sRGB color space</summary>
	R8G8B8A8UNormSrgb,

	/// <summary>Channels: Blue: 8 bits, Green: 8 bits, Red: 8 bits, Alpha: 8 bits; Channel values: Unsigned normalized floats in sRGB color space</summary>
	/// <remarks>
	/// <para>
	/// The color channels are in reverse order compared to <see cref="R8G8B8A8UNormSrgb"/>.
	/// </para>
	/// </remarks>
	B8G8R8A8UNormSrgb,

	#endregion

	#region Compressed sRGB unsigned normalized color formats

	/// <summary>Block Compression 1: 4x4 blocks; Channels: RGBA with 1-bit alpha (4 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Bc1RgbaUNormSrgb,

	/// <summary>Block Compression 2: 4x4 blocks; Channels: RGBA with explicit (uncompressed) alpha (8 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Bc2RgbaUNormSrgb,

	/// <summary>Block Compression 3: 4x4 blocks; Channels: RGBA with interpolated alpha (8 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Bc3RgbaUNormSrgb,

	/// <summary>Block Compression 4: 4x4 blocks; Channels: Red (4 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Bc7RgbaUNormSrgb,

	#endregion

	#region Depth and stencil formats

	/// <summary>Channels: Depth: 16 bits; Channel values: Unsigned normalized floats</summary>
	D16UNorm,

	/// <summary>Channels: Depth: 24 bits; Channel values: Unsigned normalized floats</summary>
	D24UNorm,

	/// <summary>Channels: Depth: 32 bits; Channel values: Signed floats</summary>
	D32Float,

	/// <summary>Channels: Depth: 24 bits, Stencil: 8 bits; Channel values: Unsigned normalized floats for depth and unsigned integers for stencil</summary>
	D24UNormS8UInt,

	/// <summary>Channels: Depth: 32 bits, Stencil: 8 bits; Channel values: Signed floats for depth and unsigned integers for stencil</summary>
	D32FloatS8UInt,

	#endregion

	#region Compressed ASTC normalized float color formats

	/// <summary>Adaptive Scalable Texture Compression: 4x4 blocks; Channels: RGBA (8 bpp); Channel values: Unsigned normalized floats</summary>
	Astc4x4UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 5x4 blocks; Channels: RGBA (6.4 bpp); Channel values: Unsigned normalized floats</summary>
	Astc5x4UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 5x5 blocks; Channels: RGBA (5.12 bpp); Channel values: Unsigned normalized floats</summary>
	Astc5x5UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 6x5 blocks; Channels: RGBA (4.27 bpp); Channel values: Unsigned normalized floats</summary>
	Astc6x5UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 6x6 blocks; Channels: RGBA (3.56 bpp); Channel values: Unsigned normalized floats</summary>
	Astc6x6UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 8x5 blocks; Channels: RGBA (3.2 bpp); Channel values: Unsigned normalized floats</summary>
	Astc8x5UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 8x6 blocks; Channels: RGBA (2.67 bpp); Channel values: Unsigned normalized floats</summary>
	Astc8x6UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 8x8 blocks; Channels: RGBA (2 bpp); Channel values: Unsigned normalized floats</summary>
	Astc8x8UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 10x5 blocks; Channels: RGBA (2.56 bpp); Channel values: Unsigned normalized floats</summary>
	Astc10x5UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 10x6 blocks; Channels: RGBA (2.13 bpp); Channel values: Unsigned normalized floats</summary>
	Astc10x6UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 10x8 blocks; Channels: RGBA (1.6 bpp); Channel values: Unsigned normalized floats</summary>
	Astc10x8UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 10x10 blocks; Channels: RGBA (1.28 bpp); Channel values: Unsigned normalized floats</summary>
	Astc10x10UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 12x10 blocks; Channels: RGBA (1.07 bpp); Channel values: Unsigned normalized floats</summary>
	Astc12x10UNorm,

	/// <summary>Adaptive Scalable Texture Compression: 12x12 blocks; Channels: RGBA (0.89 bpp); Channel values: Unsigned normalized floats</summary>
	Astc12x12UNorm,

	#endregion

	#region Compressed ASTC sRGB normalized float color formats

	/// <summary>Adaptive Scalable Texture Compression: 4x4 blocks; Channels: RGBA (8 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc4x4UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 5x4 blocks; Channels: RGBA (6.4 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc5x4UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 5x5 blocks; Channels: RGBA (5.12 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc5x5UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 6x5 blocks; Channels: RGBA (4.27 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc6x5UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 6x6 blocks; Channels: RGBA (3.56 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc6x6UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 8x5 blocks; Channels: RGBA (3.2 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc8x5UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 8x6 blocks; Channels: RGBA (2.67 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc8x6UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 8x8 blocks; Channels: RGBA (2 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc8x8UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 10x5 blocks; Channels: RGBA (2.56 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc10x5UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 10x6 blocks; Channels: RGBA (2.13 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc10x6UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 10x8 blocks; Channels: RGBA (1.6 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc10x8UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 10x10 blocks; Channels: RGBA (1.28 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc10x10UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 12x10 blocks; Channels: RGBA (1.07 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc12x10UNormSrgb,

	/// <summary>Adaptive Scalable Texture Compression: 12x12 blocks; Channels: RGBA (0.89 bpp); Channel values: Unsigned normalized floats in sRGB color space</summary>
	Astc12x12UNormSrgb,

	#endregion

	#region Compressed ASTC signed float color formats

	/// <summary>Adaptive Scalable Texture Compression: 4x4 blocks; Channels: RGBA (8 bpp); Channel values: Signed floats</summary>
	Astc4x4Float,

	/// <summary>Adaptive Scalable Texture Compression: 5x4 blocks; Channels: RGBA (6.4 bpp); Channel values: Signed floats</summary>
	Astc5x4Float,

	/// <summary>Adaptive Scalable Texture Compression: 5x5 blocks; Channels: RGBA (5.12 bpp); Channel values: Signed floats</summary>
	Astc5x5Float,

	/// <summary>Adaptive Scalable Texture Compression: 6x5 blocks; Channels: RGBA (4.27 bpp); Channel values: Signed floats</summary>
	Astc6x5Float,

	/// <summary>Adaptive Scalable Texture Compression: 6x6 blocks; Channels: RGBA (3.56 bpp); Channel values: Signed floats</summary>
	Astc6x6Float,

	/// <summary>Adaptive Scalable Texture Compression: 8x5 blocks; Channels: RGBA (3.2 bpp); Channel values: Signed floats</summary>
	Astc8x5Float,

	/// <summary>Adaptive Scalable Texture Compression: 8x6 blocks; Channels: RGBA (2.67 bpp); Channel values: Signed floats</summary>
	Astc8x6Float,

	/// <summary>Adaptive Scalable Texture Compression: 8x8 blocks; Channels: RGBA (2 bpp); Channel values: Signed floats</summary>
	Astc8x8Float,

	/// <summary>Adaptive Scalable Texture Compression: 10x5 blocks; Channels: RGBA (2.56 bpp); Channel values: Signed floats</summary>
	Astc10x5Float,

	/// <summary>Adaptive Scalable Texture Compression: 10x6 blocks; Channels: RGBA (2.13 bpp); Channel values: Signed floats</summary>
	Astc10x6Float,

	/// <summary>Adaptive Scalable Texture Compression: 10x8 blocks; Channels: RGBA (1.6 bpp); Channel values: Signed floats</summary>
	Astc10x8Float,

	/// <summary>Adaptive Scalable Texture Compression: 10x10 blocks; Channels: RGBA (1.28 bpp); Channel values: Signed floats</summary>
	Astc10x10Float,

	/// <summary>Adaptive Scalable Texture Compression: 12x10 blocks; Channels: RGBA (1.07 bpp); Channel values: Signed floats</summary>
	Astc12x10Float,

	/// <summary>Adaptive Scalable Texture Compression: 12x12 blocks; Channels: RGBA (0.89 bpp); Channel values: Signed floats</summary>
	Astc12x12Float,

	#endregion
}
