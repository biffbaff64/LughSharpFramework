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

using System.Runtime.Versioning;

namespace LughSharp.Lugh.Graphics;

[PublicAPI]
public class PixelFormat
{
    /// <summary>
    /// Converts a GL format to a LughFormat value.
    /// </summary>
    public static int GLFormatToLughFormat( int format )
    {
        return format switch
               {
                   IGL.GL_ALPHA           => LughFormat.ALPHA,
                   IGL.GL_LUMINANCE_ALPHA => LughFormat.LUMINANCE_ALPHA,
                   IGL.GL_RGB565          => LughFormat.RGB565,
                   IGL.GL_RGBA4           => LughFormat.RGBA4444,
                   IGL.GL_RGB             => LughFormat.RGB888,
                   IGL.GL_RGBA            => LughFormat.RGBA8888,
                   IGL.GL_COLOR_INDEX     => LughFormat.INDEXED_COLOR,

                   // ----------------------------------

                   var _ => throw new GdxRuntimeException( "Unknown Gdx2DPixmap Format: " + format ),
               };
    }

    /// <summary>
    /// Converts a PixelFormat value to a GL format.
    /// </summary>
    public static int LughFormatToGLFormat( int format )
    {
        return format switch
               {
                   LughFormat.ALPHA           => IGL.GL_ALPHA,
                   LughFormat.LUMINANCE_ALPHA => IGL.GL_LUMINANCE_ALPHA,
                   LughFormat.RGB888          => IGL.GL_RGB,
                   LughFormat.RGBA8888        => IGL.GL_RGBA,
                   LughFormat.RGB565          => IGL.GL_RGB565,
                   LughFormat.RGBA4444        => IGL.GL_RGBA4,
                   LughFormat.INDEXED_COLOR   => IGL.GL_COLOR_INDEX,

                   // ----------------------------------

                   var _ => throw new GdxRuntimeException( $"Unsupported format: {format}" ),
               };
    }

    /// <summary>
    /// Converts a PixelFormat value to a GL format.
    /// </summary>
    public static int LughFormatToGLInternalFormat( int format )
    {
        return format switch
               {
                   LughFormat.ALPHA           => IGL.GL_ALPHA,
                   LughFormat.LUMINANCE_ALPHA => IGL.GL_LUMINANCE_ALPHA,
                   LughFormat.RGB888          => IGL.GL_RGB,
                   LughFormat.RGBA8888        => IGL.GL_RGBA,
                   LughFormat.RGB565          => IGL.GL_RGB565,
                   LughFormat.RGBA4444        => IGL.GL_RGBA4,
                   LughFormat.INDEXED_COLOR   => IGL.GL_COLOR_INDEX,

                   // ----------------------------------

                   var _ => throw new GdxRuntimeException( $"Unsupported format: {format}" ),
               };
    }

    /// <summary>
    /// Converts a PixelFormat value to a PNG Color Type value.
    /// </summary>
    public static byte LughFormatToPNGColorType( int format )
    {
        return format switch
               {
                   LughFormat.ALPHA           => 0,
                   LughFormat.LUMINANCE_ALPHA => 4,
                   LughFormat.RGB565          => 2,
                   LughFormat.RGBA4444        => 2,
                   LughFormat.RGB888          => 2,
                   LughFormat.RGBA8888        => 6,
                   LughFormat.INDEXED_COLOR   => 3,

                   // ----------------------------------

                   var _ => throw new GdxRuntimeException( $"unknown format: {format}" ),
               };
    }

    /// <summary>
    /// Converts a PNG Color Type value to a PixelFormat value.
    /// </summary>
    public static int PNGColorToLughFormat( int colorType )
    {
        return colorType switch
               {
                   1 => LughFormat.ALPHA,
                   2 => LughFormat.LUMINANCE_ALPHA,
                   3 => LughFormat.RGB888,
                   4 => LughFormat.RGBA8888,
                   5 => LughFormat.RGB565,
                   6 => LughFormat.RGBA4444,

                   // ----------------------------------

                   var _ => throw new GdxRuntimeException( $"Unknown PNG Color Type: {colorType}" ),
               };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static int GLFormatToPNGFormat( int format )
    {
        return format switch
               {
                   IGL.GL_ALPHA           => 0, // ALPHA,
                   IGL.GL_LUMINANCE_ALPHA => 4, // LUMINANCE_ALPHA,
                   IGL.GL_RGB565          => 2, // RGB565,
                   IGL.GL_RGBA4           => 2, // RGBA4444,
                   IGL.GL_RGB             => 2, // RGB888,
                   IGL.GL_RGBA            => 6, // RGBA8888,
                   IGL.GL_COLOR_INDEX     => 3, // INDEXED_COLOR,

                   // ----------------------------------

                   var _ => throw new GdxRuntimeException( $"Unknown GL Format: {format}" ),
               };
    }

    /// <summary>
    /// Converts a PNG color type and bit depth to a PixelFormat value.
    /// </summary>
    public static int FromPNGColorAndBitDepth( byte colorType, byte bitDepth )
    {
        // Map PNG color type and bit depth to the format
        var format = ( colorType, bitDepth ) switch
                     {
                         (6, 8) => LughFormat.RGBA8888,        // Truecolor with alpha, 8 bits
                         (6, 4) => LughFormat.RGBA4444,        // Truecolor with alpha, 4 bits
                         (2, 8) => LughFormat.RGB888,          // Truecolor, 8 bits
                         (0, 8) => LughFormat.ALPHA,           // Grayscale, 8 bits
                         (4, 8) => LughFormat.LUMINANCE_ALPHA, // Grayscale with alpha, 8 bits
                         (2, 5) => LughFormat.RGB565,          // Truecolor, 5 bits
                         (3, 8) => LughFormat.INDEXED_COLOR,   // Indexed color, 8 bits

                         // ----------------------------------

                         var _ => LughFormat.INVALID, // Invalid format, handled by caller
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
    /// Retrieves the name of the PixelFormat format based on the provided format identifier.
    /// </summary>
    public static string GetFormatString( int format )
    {
        return format switch
               {
                   LughFormat.ALPHA           => "Alpha",
                   LughFormat.LUMINANCE_ALPHA => "LuminanceAlpha",
                   LughFormat.RGB565          => "RGB565",
                   LughFormat.RGBA4444        => "RGBA4444",
                   LughFormat.RGB888          => "RGB888",
                   LughFormat.RGBA8888        => "RGBA8888 ( DEFAULT )",
                   LughFormat.INDEXED_COLOR   => "IndexedColor",

                   // ----------------------------------

                   LughFormat.INVALID   => "Invalid",
                   LughFormat.UNDEFINED => "Undefined",

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

    /// <summary>
    /// Converts a color to the specified pixel format
    /// </summary>
    public static uint RGBAToLughFormat( int requestedFormat, uint color )
    {
        uint r, g, b, a;

        switch ( requestedFormat )
        {
            case LughFormat.ALPHA:
                return color & 0xff;

            case LughFormat.LUMINANCE_ALPHA:
                r = ( color & 0xff000000 ) >> 24;
                g = ( color & 0xff0000 ) >> 16;
                b = ( color & 0xff00 ) >> 8;
                a = color & 0xff;
                var l = ( ( uint )( ( 0.2126f * r ) + ( 0.7152 * g ) + ( 0.0722 * b ) ) & 0xff ) << 8;

                return ( l & 0xffffff00 ) | a;

            case LughFormat.RGB888:
                return color >> 8;

            case LughFormat.RGBA8888:
                return color;

            case LughFormat.RGB565:
                r = ( ( ( color & 0xff000000 ) >> 27 ) << 11 ) & 0xf800;
                g = ( ( ( color & 0xff0000 ) >> 18 ) << 5 ) & 0x7e0;
                b = ( ( color & 0xff00 ) >> 11 ) & 0x1f;

                return r | g | b;

            case LughFormat.RGBA4444:
                r = ( ( ( color & 0xff000000 ) >> 28 ) << 12 ) & 0xf000;
                g = ( ( ( color & 0xff0000 ) >> 20 ) << 8 ) & 0xf00;
                b = ( ( ( color & 0xff00 ) >> 12 ) << 4 ) & 0xf0;
                a = ( ( color & 0xff ) >> 4 ) & 0xf;

                return r | g | b | a;

            case LughFormat.INDEXED_COLOR:
                return color;

            default:
                return 0;
        }
    }

    public static int LughFormatToGLDataType( int format )
    {
        return format switch
               {
                   LughFormat.ALPHA           => IGL.GL_ALPHA,
                   LughFormat.LUMINANCE_ALPHA => IGL.GL_LUMINANCE_ALPHA,
                   LughFormat.RGB888          => IGL.GL_RGB,
                   LughFormat.RGBA8888        => IGL.GL_RGBA,
                   LughFormat.RGB565          => IGL.GL_RGB,
                   LughFormat.RGBA4444        => IGL.GL_RGBA,
                   LughFormat.INDEXED_COLOR   => IGL.GL_COLOR_INDEX,

                   // ----------------------------------

                   var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
               };
    }

    /// <summary>
    /// Converts a <c>Gdx2DPixmap.GDX_2D_FORMAT_XXX</c> to the corresponding System.Drawing.Imaging.PixelFormat.
    /// </summary>
    /// <param name="format">The pixel format of type <c>Gdx2DPixmap.GDX_2D_FORMAT_XXX</c>.</param>
    /// <returns>The corresponding <see cref="System.Drawing.Imaging.PixelFormat"/>.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the provided format is invalid or unsupported.
    /// </exception>
    [SupportedOSPlatform( "windows" )]
    public static System.Drawing.Imaging.PixelFormat ToSystemPixelFormat( int format )
    {
        return format switch
               {
                   LughFormat.ALPHA           => System.Drawing.Imaging.PixelFormat.Alpha,
                   LughFormat.LUMINANCE_ALPHA => System.Drawing.Imaging.PixelFormat.Alpha,
                   LughFormat.RGB565          => System.Drawing.Imaging.PixelFormat.Format16bppRgb565,
                   LughFormat.RGB888          => System.Drawing.Imaging.PixelFormat.Format32bppRgb,
                   LughFormat.RGBA8888        => System.Drawing.Imaging.PixelFormat.Format32bppArgb,
                   LughFormat.RGBA4444        => System.Drawing.Imaging.PixelFormat.Format32bppArgb,

                   // ----------------------------------

                   var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
               };
    }

    /// <summary>
    /// Returns the pixel format from a valid named string.
    /// </summary>
    public static int GetFormatFromString( string str )
    {
        str = str.ToLower();

        return str switch
               {
                   "alpha"          => LughFormat.ALPHA,
                   "luminancealpha" => LughFormat.LUMINANCE_ALPHA,
                   "rgb565"         => LughFormat.RGB565,
                   "rgba4444"       => LughFormat.RGBA4444,
                   "rgb888"         => LughFormat.RGB888,
                   "rgba8888"       => LughFormat.RGBA8888,
                   "indexedcolor"   => LughFormat.INDEXED_COLOR,

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
        //TODO: I might have to revisit this, it doesn't feel right
        //      but I'm not sure how else I want to do it just yet.
        return format switch
               {
                   // GL Formats
                   IGL.GL_ALPHA           => 1,
                   IGL.GL_LUMINANCE       => 1,
                   IGL.GL_LUMINANCE_ALPHA => 2,
                   IGL.GL_RGB565          => 2,
                   IGL.GL_RGBA4           => 2,
                   IGL.GL_RGB             => 3,
                   IGL.GL_RGBA            => 4,
                   IGL.GL_COLOR_INDEX     => 1,

                   // ----------------------------------

                   LughFormat.ALPHA           => 1,
                   LughFormat.LUMINANCE_ALPHA => 2,
                   LughFormat.RGB565          => 2,
                   LughFormat.RGBA4444        => 2,
                   LughFormat.RGB888          => 3,
                   LughFormat.RGBA8888        => 4,
                   LughFormat.INDEXED_COLOR   => 1,

                   // ----------------------------------

                   var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
               };
    }

    #endregion Bytes Per Pixel Methods

    // ========================================================================

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