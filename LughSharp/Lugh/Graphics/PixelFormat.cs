// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

namespace LughSharp.Lugh.Graphics;

[PublicAPI]
public class PixelFormat
{
    /// <summary>
    /// Converts a GL format to a Pixmap.Format value.
    /// </summary>
    public static Pixmap.Format GLFormatToPixmapFormat( int format )
    {
        return format switch
        {
            IGL.GL_ALPHA           => Pixmap.Format.Alpha,
            IGL.GL_LUMINANCE_ALPHA => Pixmap.Format.LuminanceAlpha,
            IGL.GL_RGB565          => Pixmap.Format.RGB565,
            IGL.GL_RGBA4           => Pixmap.Format.RGBA4444,
            IGL.GL_RGB             => Pixmap.Format.RGB888,
            IGL.GL_RGBA            => Pixmap.Format.RGBA8888,
            IGL.GL_COLOR_INDEX     => Pixmap.Format.IndexedColor,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( "Unknown Gdx2DPixmap Format: " + format ),
        };
    }

    /// <summary>
    /// Converts a Pixmap.Format value to a GL format.
    /// </summary>
    public static int PixmapFormatToGLFormat( Pixmap.Format format )
    {
        return format switch
        {
            Pixmap.Format.Alpha          => IGL.GL_ALPHA,
            Pixmap.Format.LuminanceAlpha => IGL.GL_LUMINANCE_ALPHA,
            Pixmap.Format.RGB888         => IGL.GL_RGB,
            Pixmap.Format.RGBA8888       => IGL.GL_RGBA,
            Pixmap.Format.RGB565         => IGL.GL_RGB565,
            Pixmap.Format.RGBA4444       => IGL.GL_RGBA4,
            Pixmap.Format.IndexedColor   => IGL.GL_COLOR_INDEX,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Unsupported format: {format}" ),
        };
    }

    /// <summary>
    /// Converts a Pixmap.Format value to a GL format.
    /// </summary>
    public static int PixmapFormatToGLInternalFormat( Pixmap.Format format )
    {
        return format switch
        {
            Pixmap.Format.Alpha          => IGL.GL_ALPHA,
            Pixmap.Format.LuminanceAlpha => IGL.GL_LUMINANCE_ALPHA,
            Pixmap.Format.RGB888         => IGL.GL_RGB,
            Pixmap.Format.RGBA8888       => IGL.GL_RGBA,
            Pixmap.Format.RGB565         => IGL.GL_RGB565,
            Pixmap.Format.RGBA4444       => IGL.GL_RGBA4,
            Pixmap.Format.IndexedColor   => IGL.GL_COLOR_INDEX,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Unsupported format: {format}" ),
        };
    }

    /// <summary>
    /// Converts a Pixmap.Format value to a PNG Color Type value.
    /// </summary>
    public static byte PixmapFormatToPNGColorType( Pixmap.Format format )
    {
        return format switch
        {
            Pixmap.Format.Alpha          => 0,
            Pixmap.Format.LuminanceAlpha => 4,
            Pixmap.Format.RGB565         => 2,
            Pixmap.Format.RGBA4444       => 2,
            Pixmap.Format.RGB888         => 2,
            Pixmap.Format.RGBA8888       => 6,
            Pixmap.Format.IndexedColor   => 3,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"unknown format: {format}" ),
        };
    }

    /// <summary>
    /// Converts a PNG Color Type value to a Pixmap.Format value.
    /// </summary>
    public static Pixmap.Format PNGColorToPixmapFormat( int colorType )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Converts a PNG color type and bit depth to a Pixmap.Format value.
    /// </summary>
    public static Pixmap.Format FromPNGColorAndBitDepth( byte colorType, byte bitDepth )
    {
        // Map PNG color type and bit depth to the format
        var format = ( colorType, bitDepth ) switch
        {
            (6, 8) => Pixmap.Format.RGBA8888,       // Truecolor with alpha, 8 bits
            (6, 4) => Pixmap.Format.RGBA4444,       // Truecolor with alpha, 4 bits
            (2, 8) => Pixmap.Format.RGB888,         // Truecolor, 8 bits
            (0, 8) => Pixmap.Format.Alpha,          // Grayscale, 8 bits
            (4, 8) => Pixmap.Format.LuminanceAlpha, // Grayscale with alpha, 8 bits
            (2, 5) => Pixmap.Format.RGB565,         // Truecolor, 5 bits
            (3, 8) => Pixmap.Format.IndexedColor,   // Indexed color, 8 bits

            // Add more mappings as needed
            var _ => throw new Exception( $"Unsupported PNG colorType {colorType} and bitDepth {bitDepth}" ),
        };

        return format;
    }

    /// <summary>
    /// Retrieves the OpenGL data type name corresponding to the given format.
    /// </summary>
    /// <param name="format">The OpenGL format represented as an integer.</param>
    /// <returns>
    /// A string representing the OpenGL data type name. Returns "Invalid format: {format}"
    /// if the format is unrecognized.
    /// </returns>
    public static string GetGLTypeName( int format )
    {
        return format switch
        {
            IGL.GL_UNSIGNED_BYTE          => "IGL.GL_UNSIGNED_BYTE",
            IGL.GL_UNSIGNED_SHORT_5_6_5   => "IGL.GL_UNSIGNED_SHORT_5_6_5",
            IGL.GL_UNSIGNED_SHORT_4_4_4_4 => "IGL.GL_UNSIGNED_SHORT_4_4_4_4",

            // ----------------------------------

            var _ => $"Invalid format: {format}",
        };
    }

    /// <summary>
    /// Retrieves the OpenGL target name corresponding to the given target integer value.
    /// </summary>
    /// <param name="target">The integer value representing the OpenGL target.</param>
    /// <returns>
    /// A string representing the name of the OpenGL target, or "UNKNOWN TARGET: {target}" if not recognized.
    /// </returns>
    public static string GetGLTargetName( int target )
    {
        return target switch
        {
            IGL.GL_TEXTURE_1D                   => "GL_TEXTURE_1D",
            IGL.GL_TEXTURE_2D                   => "GL_TEXTURE_2D",
            IGL.GL_TEXTURE_3D                   => "GL_TEXTURE_3D",
            IGL.GL_TEXTURE_CUBE_MAP             => "GL_TEXTURE_CUBE_MAP",
            IGL.GL_TEXTURE_RECTANGLE            => "GL_TEXTURE_RECTANGLE",
            IGL.GL_TEXTURE_BUFFER               => "GL_TEXTURE_BUFFER",
            IGL.GL_TEXTURE_2D_ARRAY             => "GL_TEXTURE_2D_ARRAY",
            IGL.GL_TEXTURE_CUBE_MAP_ARRAY       => "GL_TEXTURE_CUBE_MAP_ARRAY",
            IGL.GL_TEXTURE_2D_MULTISAMPLE       => "GL_TEXTURE_2D_MULTISAMPLE",
            IGL.GL_TEXTURE_2D_MULTISAMPLE_ARRAY => "GL_TEXTURE_2D_MULTISAMPLE_ARRAY",

            // ----------------------------------

            var _ => $"UNKNOWN TARGET: {target}",
        };
    }

    /// <summary>
    /// Retrieves the name of the Pixmap.Format format based on the provided format identifier.
    /// </summary>
    public static string GetFormatString( Pixmap.Format format )
    {
        return format switch
        {
            Pixmap.Format.Alpha          => "Alpha",
            Pixmap.Format.Intensity      => "Intensity",
            Pixmap.Format.LuminanceAlpha => "LuminanceAlpha",
            Pixmap.Format.RGB565         => "RGB565",
            Pixmap.Format.RGBA4444       => "RGBA4444",
            Pixmap.Format.RGB888         => "RGB888",
            Pixmap.Format.RGBA8888       => "RGBA8888",
            Pixmap.Format.IndexedColor   => "IndexedColor",

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Unknown Format: {format}" ),
        };
    }

    /// <summary>
    /// Retrieves the name of the OpenGL pixel format based on the provided format identifier.
    /// </summary>
    /// <param name="format">The integer identifier of the OpenGL pixel format.</param>
    /// <returns>A string representing the name of the corresponding OpenGL pixel format.</returns>
    public static string GLFormatAsString( int format )
    {
        return format switch
        {
            // Unsigned normalized ( filterable, colour )
            IGL.GL_R8           => "IGL.GL_R8",
            IGL.GL_RG8          => "IGL.GL_RG8",
            IGL.GL_RGB8         => "IGL.GL_RGB8",
            IGL.GL_RGBA8        => "IGL.GL_RGBA8",
            IGL.GL_SRGB8        => "IGL.GL_SRGB8",
            IGL.GL_SRGB8_ALPHA8 => "IGL.GL_SRGB8_ALPHA8",

            // Signed normalized
            IGL.GL_R8_SNORM     => "IGL.GL_R8_SNORM",
            IGL.GL_RG8_SNORM    => "IGL.GL_RG8_SNORM",
            IGL.GL_RGB8_SNORM   => "IGL.GL_RGB8_SNORM",
            IGL.GL_RGBA8_SNORM  => "GL_RGBA8_SNORM",
            IGL.GL_R16_SNORM    => "IGL.GL_R16_SNORM",
            IGL.GL_RG16_SNORM   => "IGL.GL_RG16_SNORM",
            IGL.GL_RGB16_SNORM  => "IGL.GL_RGB16_SNORM",
            IGL.GL_RGBA16_SNORM => "GL_RGBA16_SNORM",

            IGL.GL_R16_F               => "IGL.GL_R16F",
            IGL.GL_RG16_F              => "IGL.GL_RG16F",
            IGL.GL_RGBA16_F            => "IGL.GL_RGBA16F",
            IGL.GL_R32_F               => "IGL.GL_R32F",
            IGL.GL_RG32_F              => "IGL.GL_RG32F",
            IGL.GL_RGBA32_F            => "IGL.GL_RGBA32F",
            IGL.GL_R11_F_G11_F_B10_F   => "IGL.GL_R11F_G11F_B10F",
            IGL.GL_RGB9_E5             => "IGL.GL_RGB9_E5 ",
            IGL.GL_R8_I                => "IGL.GL_R8I",
            IGL.GL_R8_UI               => "IGL.GL_R8UI",
            IGL.GL_RG8_I               => "IGL.GL_RG8I",
            IGL.GL_RG8_UI              => "IGL.GL_RG8UI",
            IGL.GL_RGBA8_I             => "IGL.GL_RGBA8I",
            IGL.GL_RGBA8_UI            => "IGL.GL_RGBA8UI",
            IGL.GL_R16_I               => "IGL.GL_R16I",
            IGL.GL_R16_UI              => "IGL.GL_R16UI",
            IGL.GL_RG16_I              => "IGL.GL_RG16I",
            IGL.GL_RG16_UI             => "IGL.GL_RG16UI",
            IGL.GL_RGBA16_I            => "IGL.GL_RGBA16I",
            IGL.GL_RGBA16_UI           => "IGL.GL_RGBA16UI",
            IGL.GL_R32_I               => "IGL.GL_R32I",
            IGL.GL_R32_UI              => "IGL.GL_R32UI",
            IGL.GL_RG32_I              => "IGL.GL_RG32I",
            IGL.GL_RG32_UI             => "IGL.GL_RG32UI",
            IGL.GL_RGBA32_I            => "IGL.GL_RGBA32I",
            IGL.GL_RGBA32_UI           => "IGL.GL_RGBA32UI",
            IGL.GL_DEPTH_COMPONENT24   => "IGL.GL_DEPTH_COMPONENT24 ",
            IGL.GL_DEPTH_COMPONENT32_F => "IGL.GL_DEPTH_COMPONENT32F",
            IGL.GL_STENCIL_INDEX8      => "IGL.GL_STENCIL_INDEX8",
            IGL.GL_DEPTH24_STENCIL8    => "IGL.GL_DEPTH24_STENCIL8",
            IGL.GL_DEPTH32_F_STENCIL8  => "IGL.GL_DEPTH32F_STENCIL8",
            IGL.GL_COLOR_INDEX         => "IGL.GL_COLOR_INDEX",

            // ----------------------------------

            var _ => $"Invalid format: {format}",
        };
    }

    public static int PixmapFormatToGLDataType( Pixmap.Format format )
    {
        return format switch
        {
            Pixmap.Format.Alpha          => IGL.GL_ALPHA,
            Pixmap.Format.LuminanceAlpha => IGL.GL_LUMINANCE_ALPHA,
            Pixmap.Format.RGB888         => IGL.GL_RGB,
            Pixmap.Format.RGBA8888       => IGL.GL_RGBA,
            Pixmap.Format.RGB565         => IGL.GL_RGB,
            Pixmap.Format.RGBA4444       => IGL.GL_RGBA,
            Pixmap.Format.IndexedColor   => IGL.GL_COLOR_INDEX,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// Returns the pixel format from a valid named string.
    /// </summary>
    public static Pixmap.Format GetFormatFromString( string str )
    {
        str = str.ToLower();

        return str switch
        {
            "alpha"          => Pixmap.Format.Alpha,
            "luminancealpha" => Pixmap.Format.LuminanceAlpha,
            "rgb565"         => Pixmap.Format.RGB565,
            "rgba4444"       => Pixmap.Format.RGBA4444,
            "rgb888"         => Pixmap.Format.RGB888,
            "rgba8888"       => Pixmap.Format.RGBA8888,

            // ----------------------------------
            
            var _ => throw new GdxRuntimeException( $"Unknown Format: {str}" ),
        };
    }

    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Bytes Per Pixel Methods

    /// <summary>
    /// Gets the number of bytes required for 1 pixel of the specified format.
    /// </summary>
    public static int BytesPerPixel( int format )
    {
        return format switch
        {
            IGL.GL_ALPHA           => 1,
            IGL.GL_LUMINANCE       => 1,
            IGL.GL_LUMINANCE_ALPHA => 2,
            IGL.GL_RGB565          => 2,
            IGL.GL_RGBA4           => 2,
            IGL.GL_RGB             => 3,
            IGL.GL_RGBA            => 4,
            IGL.GL_COLOR_INDEX     => 1,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// Gets the number of bytes required for 1 pixel of the specified format.
    /// </summary>
    public static int BytesPerPixel( Pixmap.Format format )
    {
        return format switch
        {
            Pixmap.Format.Alpha          => 1,
            Pixmap.Format.LuminanceAlpha => 2,
            Pixmap.Format.RGB565         => 2,
            Pixmap.Format.RGBA4444       => 2,
            Pixmap.Format.RGB888         => 3,
            Pixmap.Format.RGBA8888       => 4,
            Pixmap.Format.IndexedColor   => 1,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    #endregion Bytes Per Pixel Methods

    #region Miscellaneous Methods

    /// <summary>
    /// Computes the alignment for a given row stride in bytes.
    /// </summary>
    /// <param name="rowStrideBytes">The stride of a row in bytes.</param>
    /// <returns>
    /// The computed alignment value, which can be 8, 4, 2, or 1 depending on the input.
    /// </returns>
    public static int ComputeAlignment( int rowStrideBytes )
    {
        if ( ( rowStrideBytes % 8 ) == 0 )
        {
            return 8;
        }

        if ( ( rowStrideBytes % 4 ) == 0 )
        {
            return 4;
        }

        return ( rowStrideBytes % 2 ) == 0 ? 2 : 1;
    }

    /// <summary>
    /// Calculates the pixel data alignment value for OpenGL based on the given pixmap.
    /// </summary>
    /// <param name="pixmap">The pixmap containing information about the pixel format. May be null.</param>
    /// <returns>The alignment value required for OpenGL pixel data operations.</returns>
    public static int GetAlignment( Pixmap? pixmap )
    {
        if ( pixmap == null )
        {
            return 1;
        }

        return pixmap.GLPixelFormat switch
        {
            IGL.GL_RGB         => 1, // 3 Bpp => use 1 unless you know rowStride % 4 == 0
            IGL.GL_RGBA        => 4, // 4 Bpp (for UNSIGNED_BYTE); otherwise compute from the actual type
            IGL.GL_RGBA4       => 2, // 16-bit packed
            IGL.GL_RGB565      => 2, // 16-bit packed
            IGL.GL_ALPHA       => 1,
            IGL.GL_LUMINANCE   => 1,
            IGL.GL_COLOR_INDEX => 1,

            // ----------------------------------

            var _ => 1,
        };
    }

    #endregion Miscellaneous Methods
}

// ========================================================================
// ========================================================================