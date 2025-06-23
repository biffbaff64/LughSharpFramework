// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

using System.Runtime.Versioning;

using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Pixels;

[PublicAPI]
public class PixmapFormat
{
    /// <summary>
    /// Converts a PNG color type to the corresponding Pixmap pixel format.
    /// </summary>
    /// <param name="format">The PNG color type represented as an integer.</param>
    /// <returns>The corresponding Pixmap pixel format as a <see cref="PixelType.Format"/>.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the format is unknown or if the format is an unsupported indexed color.
    /// </exception>
    public static PixelType.Format PNGColorTypeToPixmapPixelFormat( int format )
    {
        return format switch
        {
            0 => PixelType.Format.RGB888,
            2 => PixelType.Format.RGB888,

            // ----------------------------------

            3 => throw new GdxRuntimeException( "Indexed color not supported yet." ),

            // ----------------------------------

            4 => PixelType.Format.RGBA8888,
            6 => PixelType.Format.RGBA8888,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"unknown format: {format}" ),
        };
    }

    /// <summary>
    /// Gets the number of bytes required for 1 pixel of the specified format.
    /// </summary>
    public static int Gdx2dBytesPerPixel( PixelType.Format format )
    {
        return format switch
        {
            PixelType.Format.Alpha          => 1,
            PixelType.Format.Intensity      => 1,
            PixelType.Format.LuminanceAlpha => 2,
            PixelType.Format.RGB565         => 2,
            PixelType.Format.RGBA4444       => 2,
            PixelType.Format.RGB888         => 3,
            PixelType.Format.RGBA8888       => 4,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
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
    /// Gets the number of bytes required for 1 pixel of the specified format.
    /// </summary>
    public static int Gdx2dBytesPerPixel( int format )
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
            IGL.GL_TEXTURE_1D                   => "IGL.GL_TEXTURE_1D",
            IGL.GL_TEXTURE_2D                   => "IGL.GL_TEXTURE_2D",
            IGL.GL_TEXTURE_3D                   => "IGL.GL_TEXTURE_3D",
            IGL.GL_TEXTURE_CUBE_MAP             => "IGL.GL_TEXTURE_CUBE_MAP",
            IGL.GL_TEXTURE_RECTANGLE            => "IGL.GL_TEXTURE_RECTANGLE",
            IGL.GL_TEXTURE_BUFFER               => "IGL.GL_TEXTURE_BUFFER",
            IGL.GL_TEXTURE_2D_ARRAY             => "IGL.GL_TEXTURE_2D_ARRAY",
            IGL.GL_TEXTURE_CUBE_MAP_ARRAY       => "IGL.GL_TEXTURE_CUBE_MAP_ARRAY",
            IGL.GL_TEXTURE_2D_MULTISAMPLE       => "IGL.GL_TEXTURE_2D_MULTISAMPLE",
            IGL.GL_TEXTURE_2D_MULTISAMPLE_ARRAY => "IGL.GL_TEXTURE_2D_MULTISAMPLE_ARRAY",

            // ----------------------------------

            var _ => $"UNKNOWN TARGET: {target}",
        };
    }

    /// <summary>
    /// Converts a Pixmap pixel format to the corresponding Gdx2DPixmap format.
    /// </summary>
    /// <param name="format">The Pixmap pixel format represented as a <see cref="PixelType.Format"/>.</param>
    /// <returns>The corresponding Gdx2DPixmap format as a <see cref="Gdx2DPixmap.Gdx2DPixmapFormat"/>.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the pixel format is invalid or not recognized.
    /// </exception>
    public static Gdx2DPixmap.Gdx2DPixmapFormat ToGdx2DPixelFormat( PixelType.Format? format )
    {
        return format switch
        {
            PixelType.Format.Alpha          => Gdx2DPixmap.Gdx2DPixmapFormat.Alpha,
            PixelType.Format.Intensity      => Gdx2DPixmap.Gdx2DPixmapFormat.Alpha,
            PixelType.Format.LuminanceAlpha => Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha,
            PixelType.Format.RGB565         => Gdx2DPixmap.Gdx2DPixmapFormat.RGB565,
            PixelType.Format.RGBA4444       => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444,
            PixelType.Format.RGB888         => Gdx2DPixmap.Gdx2DPixmapFormat.RGB888,
            PixelType.Format.RGBA8888       => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// Converts a Gdx2DPixmap format to the corresponding PixelType format.
    /// </summary>
    /// <param name="format">The Gdx2DPixmap format to be converted, specified as a <see cref="Gdx2DPixmap.Gdx2DPixmapFormat"/>.</param>
    /// <returns>The corresponding PixelType format as a <see cref="PixelType.Format"/>.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the provided format is invalid or unsupported.
    /// </exception>
    public static PixelType.Format GdxFormatToPixelTypeFormat( Gdx2DPixmap.Gdx2DPixmapFormat format )
    {
        return format switch
        {
            Gdx2DPixmap.Gdx2DPixmapFormat.Alpha          => PixelType.Format.Alpha,
            Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha => PixelType.Format.LuminanceAlpha,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB565         => PixelType.Format.RGB888,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444       => PixelType.Format.RGBA8888,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB888         => PixelType.Format.RGB565,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888       => PixelType.Format.RGBA4444,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    public static PixelType.Format GdxFormatToPixelTypeFormat( int format )
    {
        return format switch
        {
            1 => PixelType.Format.Alpha,
            2 => PixelType.Format.LuminanceAlpha,
            3 => PixelType.Format.RGB888,
            4 => PixelType.Format.RGBA8888,
            5 => PixelType.Format.RGB565,
            6 => PixelType.Format.RGBA4444,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// Converts a pixel format from the PixelType.Format enumeration to its corresponding
    /// OpenGL pixel format.
    /// </summary>
    /// <param name="format">
    /// The pixel format from the PixelType.Format enumeration to be converted.
    /// </param>
    /// <returns>The integer value representing the corresponding OpenGL pixel format.</returns>
    public static int ToGLPixelFormat( PixelType.Format format )
    {
        return ToGLPixelFormat( ToGdx2DPixelFormat( format ) );
    }

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
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB565         => IGL.GL_RGB,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888       => IGL.GL_RGBA,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444       => IGL.GL_RGBA,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// Converts a pixel format to the equivalent GL data type.
    /// </summary>
    /// <param name="format">The pixel format to be converted, specified as a <see cref="PixelType.Format"/>.</param>
    /// <returns>The corresponding GL data type as an integer.</returns>
    public static int ToGLDataType( PixelType.Format format )
    {
        return ToGLDataType( ToGdx2DPixelFormat( format ) );
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
    /// Converts a <see cref="Gdx2DPixmap.Gdx2DPixmapFormat"/> to its string representation.
    /// </summary>
    /// <param name="format">The pixmap format to convert.</param>
    /// <returns>A string representing the given pixmap format.</returns>
    public static string GetFormatString( Gdx2DPixmap.Gdx2DPixmapFormat format )
    {
        return format switch
        {
            Gdx2DPixmap.Gdx2DPixmapFormat.Alpha          => "ALPHA",
            Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha => "LUMINANCE_ALPHA",
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB888         => "RGB888",
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888       => "RGBA8888",
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB565         => "RGB565",
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444       => "RGBA4444",

            // ----------------------------------

            var _ => $"Invalid format: {format}",
        };
    }

    // ========================================================================

    /// <summary>
    /// Converts a Pixmap pixel format to the corresponding PixelType format.
    /// </summary>
    /// <param name="format">The Pixmap pixel format to be converted.</param>
    /// <returns>The corresponding <see cref="PixelType.Format"/> for the given Pixmap pixel format.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the provided Pixmap pixel format is invalid or not supported.
    /// </exception>
    [SupportedOSPlatform( "windows" )]
    public static PixelType.Format PixelFormatToPixelTypeFormat( PixelFormat format )
    {
        return format switch
        {
            PixelFormat.Alpha                => PixelType.Format.Alpha,
            PixelFormat.Format24bppRgb       => PixelType.Format.RGB888,
            PixelFormat.Format32bppArgb      => PixelType.Format.RGBA8888,
            PixelFormat.Format16bppRgb565    => PixelType.Format.RGB565,
            PixelFormat.Format16bppGrayScale => PixelType.Format.RGBA4444,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// Converts a <see cref="PixelType.Format"/> to the corresponding System.Drawing.Imaging.PixelFormat.
    /// </summary>
    /// <param name="format">The pixel format of type <see cref="PixelType.Format"/>.</param>
    /// <returns>The corresponding <see cref="System.Drawing.Imaging.PixelFormat"/>.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the provided format is invalid or unsupported.
    /// </exception>
    [SupportedOSPlatform( "windows" )]
    public static System.Drawing.Imaging.PixelFormat ToPixelFormat( PixelType.Format format )
    {
        var cformat = ToGdx2DPixelFormat( format );

        return cformat switch
        {
            Gdx2DPixmap.Gdx2DPixmapFormat.Alpha          => PixelFormat.Alpha,
            Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha => PixelFormat.Alpha,             // IGL.GL_LUMINANCE_ALPHA,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB888         => PixelFormat.Format32bppRgb,    // IGL.GL_RGB,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB565         => PixelFormat.Format16bppRgb565, // IGL.GL_RGB,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888       => PixelFormat.Format32bppArgb,   // IGL.GL_RGBA,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444       => PixelFormat.Format32bppArgb,   // IGL.GL_RGBA,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    // ========================================================================

    /// <summary>
    /// Converts a given Pixmap to the specified pixel format.
    /// </summary>
    /// <param name="pixmap">The source Pixmap to be converted.</param>
    /// <param name="targetFormat">The target pixel format for the conversion.</param>
    /// <returns>A new Pixmap instance converted to the specified format.</returns>
    public static Pixmap ConvertToFormat( Pixmap pixmap, PixelType.Format targetFormat )
    {
        var convertedPixmap = new Pixmap( pixmap.Width, pixmap.Height, targetFormat );

        // Convert RGBA4444 to RGBA8888
        var srcData = pixmap.PixelData;
        var dstData = convertedPixmap.PixelData;

        for ( var i = 0; i < ( pixmap.Width * pixmap.Height ); i++ )
        {
            // For RGBA4444, each pixel is 2 bytes
            var srcIndex = i * 2;
            var dstIndex = i * 4;

            if ( ( srcIndex + 1 ) >= srcData.Length )
            {
                break;
            }

            // Read 2 bytes for RGBA4444
            var pixel = ( ushort )( ( srcData[ srcIndex ] << 8 ) | srcData[ srcIndex + 1 ] );

            // Extract 4-bit components and convert to 8-bit
            var r = ( byte )( ( pixel >> 12 ) & 0xF );
            var g = ( byte )( ( pixel >> 8 ) & 0xF );
            var b = ( byte )( ( pixel >> 4 ) & 0xF );
            var a = ( byte )( pixel & 0xF );

            // Convert 4-bit to 8-bit by replicating bits
            dstData[ dstIndex ]     = ( byte )( ( r << 4 ) | r ); // R
            dstData[ dstIndex + 1 ] = ( byte )( ( g << 4 ) | g ); // G
            dstData[ dstIndex + 2 ] = ( byte )( ( b << 4 ) | b ); // B
            dstData[ dstIndex + 3 ] = ( byte )( ( a << 4 ) | a ); // A
        }

        return convertedPixmap;
    }
}