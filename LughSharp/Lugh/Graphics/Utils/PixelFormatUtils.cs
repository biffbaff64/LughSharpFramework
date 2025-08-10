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

using LughSharp.Lugh.Graphics.G2D;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public class PixelFormatUtils
{
    // ----------------------------------------------------
    #region Methods handling pixel format in memory.
    // ----------------------------------------------------

    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static Gdx2DPixmap.Gdx2DPixmapFormat GdxFormatToPixelTypeFormat( int format )
    {
        return format switch
        {
            1 => Gdx2DPixmap.Gdx2DPixmapFormat.Alpha,
            2 => Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha,
            3 => Gdx2DPixmap.Gdx2DPixmapFormat.RGB888,
            4 => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888,
            5 => Gdx2DPixmap.Gdx2DPixmapFormat.RGB565,
            6 => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// Returns the pixel format from a valid named string.
    /// </summary>
    public static Gdx2DPixmap.Gdx2DPixmapFormat GetFormatFromString( string str )
    {
        str = str.ToLower();

        return str switch
        {
            "Alpha"          => Gdx2DPixmap.Gdx2DPixmapFormat.Alpha,
            "Luminancealpha" => Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha,
            "RGB565"         => Gdx2DPixmap.Gdx2DPixmapFormat.RGB565,
            "RGBA4444"       => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444,
            "RGB888"         => Gdx2DPixmap.Gdx2DPixmapFormat.RGB888,
            "RGBA8888"       => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888,

            var _ => throw new GdxRuntimeException( $"Unknown Format: {str}" ),
        };
    }

    public static Gdx2DPixmap.Gdx2DPixmapFormat ToGdx2DPixmapFormat( int format )
    {
        return format switch
        {
            1 => Gdx2DPixmap.Gdx2DPixmapFormat.Alpha,
            2 => Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha,
            3 => Gdx2DPixmap.Gdx2DPixmapFormat.RGB888,
            4 => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888,
            5 => Gdx2DPixmap.Gdx2DPixmapFormat.RGB565,
            6 => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// Gets the bit depth corresponding to the provided Gdx2DPixmapFormat.
    /// </summary>
    /// <param name="format">The Gdx2DPixmapFormat whose bit depth is to be retrieved.</param>
    /// <returns>The bit depth associated with the specified format.</returns>
    /// <exception cref="ArgumentException">Thrown if the provided format is unknown or unsupported.</exception>
    public static int GetBitDepth( Gdx2DPixmap.Gdx2DPixmapFormat format )
    {
        // Get proper bit depth based on format
        return format switch
        {
            Gdx2DPixmap.Gdx2DPixmapFormat.Alpha          => 8,
            Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha => 16,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB888         => 24,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888       => 32,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB565         => 16,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444       => 16,

            // ----------------------------------

            var _ => throw new ArgumentException( $"Unknown format: {format}" ),
        };
    }

    /// <summary>
    /// Gets the number of bytes required for 1 pixel of the specified format.
    /// </summary>
    public static int Gdx2dBytesPerPixel( Gdx2DPixmap.Gdx2DPixmapFormat format )
    {
        return format switch
        {
            Gdx2DPixmap.Gdx2DPixmapFormat.Alpha          => 1,
            Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha => 2,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB565         => 2,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444       => 2,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB888         => 3,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888       => 4,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// Converts a PNG color type to the corresponding Pixmap pixel format.
    /// </summary>
    /// <param name="format">The PNG color type represented as an integer.</param>
    /// <returns>
    /// The corresponding Pixmap pixel format as a <see cref="Gdx2DPixmap.Gdx2DPixmapFormat"/>.
    /// </returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the format is unknown or if the format is an unsupported indexed color.
    /// </exception>
    public static Gdx2DPixmap.Gdx2DPixmapFormat PNGColorTypeToPixmapFormat( int format )
    {
        return format switch
        {
            0 => Gdx2DPixmap.Gdx2DPixmapFormat.RGB888,
            2 => Gdx2DPixmap.Gdx2DPixmapFormat.RGB888,

            // ----------------------------------

            3 => throw new GdxRuntimeException( "Indexed color not supported yet." ),

            // ----------------------------------

            4 => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888,
            6 => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"unknown format: {format}" ),
        };
    }

    #endregion Methods handling pixel format in memory.
    
    // ----------------------------------------------------
    #region Methods handling pixel format OpenGL expects
    // ----------------------------------------------------

    /// <summary>
    /// Converts a given Gdx2DPixmap format to its corresponding OpenGL pixel format.
    /// </summary>
    /// <param name="format">The Gdx2DPixmap format to be converted.</param>
    /// <returns>The integer value representing the corresponding OpenGL pixel format.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown when the provided format is invalid or unrecognized.
    /// </exception>
    public static int ToGLPixelFormat( Gdx2DPixmap.Gdx2DPixmapFormat format )
    {
        return format switch
        {
            Gdx2DPixmap.Gdx2DPixmapFormat.Alpha          => IGL.GL_ALPHA,
            Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha => IGL.GL_LUMINANCE_ALPHA,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB888         => IGL.GL_RGB,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888       => IGL.GL_RGBA,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB565         => IGL.GL_RGB,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444       => IGL.GL_RGBA,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// Converts a Gdx2DPixmap format to the corresponding OpenGL data type.
    /// </summary>
    /// <param name="format">The Gdx2DPixmap format that needs to be converted.</param>
    /// <returns>The corresponding OpenGL data type as an integer constant.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the specified format is not valid or recognized.
    /// </exception>
    public static int ToGLDataType( Gdx2DPixmap.Gdx2DPixmapFormat format )
    {
        return format switch
        {
            Gdx2DPixmap.Gdx2DPixmapFormat.Alpha          => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB888         => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB565         => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888       => IGL.GL_UNSIGNED_SHORT_5_6_5,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444       => IGL.GL_UNSIGNED_SHORT_4_4_4_4,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static int GetGLInternalFormat( Gdx2DPixmap.Gdx2DPixmapFormat format )
    {
        return format switch
        {
            Gdx2DPixmap.Gdx2DPixmapFormat.Alpha          => IGL.GL_ALPHA,
            Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha => IGL.GL_LUMINANCE_ALPHA,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB888         => IGL.GL_RGB8,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888       => IGL.GL_RGBA8,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB565         => IGL.GL_RGB565,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444       => IGL.GL_RGBA4,
            var _                                        => throw new ArgumentException( $"Unsupported format: {format}" )
        };
    }

    /// <summary>
    /// Gets the number of bytes required for 1 pixel of the specified format.
    /// </summary>
    public static int GLBytesPerPixel( int format )
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

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    #endregion Methods handling pixel format OpenGL expects
    
    // ----------------------------------------------------
    #region Internal pixel format
    // Methods handling the internal pixel format that
    // OpenGL uses to store the pixel data..
    // ----------------------------------------------------

    #endregion Internal pixel format
    
    // ----------------------------------------------------
    #region Miscellaneous
    // ----------------------------------------------------

    /// <summary>
    /// Converts a color to the specified pixel format
    /// </summary>
    public static uint ToPixelFormat( Gdx2DPixmap.Gdx2DPixmapFormat format, uint color )
    {
        uint r, g, b, a;

        switch ( format )
        {
            case Gdx2DPixmap.Gdx2DPixmapFormat.Alpha:
                return color & 0xff;

            case Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha:
                r = ( color & 0xff000000 ) >> 24;
                g = ( color & 0xff0000 ) >> 16;
                b = ( color & 0xff00 ) >> 8;
                a = color & 0xff;
                var l = ( ( uint )( ( 0.2126f * r ) + ( 0.7152 * g ) + ( 0.0722 * b ) ) & 0xff ) << 8;

                return ( l & 0xffffff00 ) | a;

            case Gdx2DPixmap.Gdx2DPixmapFormat.RGB888:
                return color >> 8;

            case Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888:
                return color;

            case Gdx2DPixmap.Gdx2DPixmapFormat.RGB565:
                r = ( ( ( color & 0xff000000 ) >> 27 ) << 11 ) & 0xf800;
                g = ( ( ( color & 0xff0000 ) >> 18 ) << 5 ) & 0x7e0;
                b = ( ( color & 0xff00 ) >> 11 ) & 0x1f;

                return r | g | b;

            case Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444:
                r = ( ( ( color & 0xff000000 ) >> 28 ) << 12 ) & 0xf000;
                g = ( ( ( color & 0xff0000 ) >> 20 ) << 8 ) & 0xf00;
                b = ( ( ( color & 0xff00 ) >> 12 ) << 4 ) & 0xf0;
                a = ( ( color & 0xff ) >> 4 ) & 0xf;

                return r | g | b | a;

            default:
                return 0;
        }
    }

    #endregion Miscellaneous
    
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Get String methods
    
    /// <summary>
    /// Converts a <see cref="Gdx2DPixmap.Gdx2DPixmapFormat"/> to its string representation.
    /// </summary>
    /// <param name="format">The pixmap format to convert.</param>
    /// <returns>A string representing the given pixmap format.</returns>
    public static string GetFormatString( Gdx2DPixmap.Gdx2DPixmapFormat format )
    {
        return format switch
        {
            Gdx2DPixmap.Gdx2DPixmapFormat.Alpha          => "Alpha",
            Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha => "LuminanceAlpha",
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB888         => "RGB888",
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888       => "RGBA8888",
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB565         => "RGB565",
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444       => "RGBA4444",

            // ----------------------------------

            var _ => $"Invalid format: {format}",
        };
    }

    /// <summary>
    /// Retrieves the name of the OpenGL pixel format based on the provided format identifier.
    /// </summary>
    /// <param name="format">The integer identifier of the OpenGL pixel format.</param>
    /// <returns>A string representing the name of the corresponding OpenGL pixel format.</returns>
    public static string GetGLPixelFormatName( int format )
    {
        return format switch
        {
            IGL.GL_ALPHA           => "IGL.GL_ALPHA",
            IGL.GL_LUMINANCE_ALPHA => "IGL.GL_LUMINANCE_ALPHA",
            IGL.GL_RGB             => "IGL.GL_RGB",
            IGL.GL_RGBA            => "IGL.GL_RGBA",
            IGL.GL_RGB8            => "IGL.GL_RGB8",
            IGL.GL_RGBA8           => "IGL.GL_RGBA8",
            IGL.GL_RGB565          => "IGL.GL_RGB565",
            IGL.GL_RGBA4           => "IGL.GL_RGBA4",
            IGL.GL_RGB10           => "IGL.GL_RGB10",
            IGL.GL_RGB12           => "IGL.GL_RGB12",
            IGL.GL_RGBA16          => "IGL.GL_RGBA16",

            // ----------------------------------

            var _ => $"Invalid format: {format}",
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetGLInternalFormatName( int format )
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

            // ----------------------------------

            var _ => $"Invalid format: {format}",
        };
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
    
    #endregion Get String methods
}

// ========================================================================
// ========================================================================