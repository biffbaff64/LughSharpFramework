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

using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Images;

[PublicAPI]
public class PixmapFormat
{
    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static int PngToOglColor( PixelType.Format format )
    {
        return format switch
        {
            PixelType.Format.Alpha          => IGL.GL_ALPHA,
            PixelType.Format.LuminanceAlpha => IGL.GL_LUMINANCE_ALPHA,
            PixelType.Format.RGB888         => IGL.GL_RGB,
            PixelType.Format.RGBA8888       => IGL.GL_RGBA,
            PixelType.Format.RGB565         => IGL.GL_RGB565,
            PixelType.Format.RGBA4444       => IGL.GL_RGBA4,
            PixelType.Format.Intensity      => IGL.GL_LUMINANCE,

            var _ => throw new GdxRuntimeException( $"Illegal PixelFormat specified: {format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static PixelType.Format OglToPngColor( int format )
    {
        return format switch
        {
            IGL.GL_ALPHA           => PixelType.Format.Alpha,
            IGL.GL_LUMINANCE_ALPHA => PixelType.Format.LuminanceAlpha,
            IGL.GL_RGB             => PixelType.Format.RGB888,
            IGL.GL_RGBA            => PixelType.Format.RGBA8888,
            IGL.GL_RGB565          => PixelType.Format.RGB565,
            IGL.GL_RGBA4           => PixelType.Format.RGBA4444,
            IGL.GL_LUMINANCE       => PixelType.Format.Intensity,

            var _ => throw new GdxRuntimeException( $"Illegal PixelFormat specified: {format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static PixelType.Format PNGColorTypeToPixmapPixelFormat( int format )
    {
        return format switch
        {
            0 => PixelType.Format.RGB888,
            2 => PixelType.Format.RGB888,

            3 => throw new GdxRuntimeException( "Indexed color not supported yet." ),

            4 => PixelType.Format.RGBA8888,
            6 => PixelType.Format.RGBA8888,

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

            var _ => 4,
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

            var _ => 4,
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetGLPixelFormatName( int format )
    {
        return format switch
        {
            IGL.GL_ALPHA           => "IGL.GL_ALPHA",
            IGL.GL_LUMINANCE_ALPHA => "IGL.GL_LUMINANCE_ALPHA",
            IGL.GL_RGB             => "IGL.GL_RGB",
            IGL.GL_RGBA            => "IGL.GL_RGBA",

            var _ => $"Invalid format: {format}",
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetGLTypeName( int format )
    {
        return format switch
        {
            IGL.GL_UNSIGNED_BYTE          => "IGL.GL_UNSIGNED_BYTE",
            IGL.GL_UNSIGNED_SHORT_5_6_5   => "IGL.GL_UNSIGNED_SHORT_5_6_5",
            IGL.GL_UNSIGNED_SHORT_4_4_4_4 => "IGL.GL_UNSIGNED_SHORT_4_4_4_4",

            var _ => $"Invalid format: {format}",
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static int ToGdx2DPixelFormat( PixelType.Format? format )
    {
        return format switch
        {
            PixelType.Format.Alpha          => Gdx2DPixmap.GDX_2D_FORMAT_ALPHA,
            PixelType.Format.Intensity      => Gdx2DPixmap.GDX_2D_FORMAT_ALPHA,
            PixelType.Format.LuminanceAlpha => Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA,
            PixelType.Format.RGB565         => Gdx2DPixmap.GDX_2D_FORMAT_RGB565,
            PixelType.Format.RGBA4444       => Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444,
            PixelType.Format.RGB888         => Gdx2DPixmap.GDX_2D_FORMAT_RGB888,
            PixelType.Format.RGBA8888       => Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888,

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static PixelType.Format ToPixmapPixelFormat( int format )
    {
        return format switch
        {
            Gdx2DPixmap.GDX_2D_FORMAT_ALPHA           => PixelType.Format.Alpha,
            Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA => PixelType.Format.LuminanceAlpha,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB888          => PixelType.Format.RGB888,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888        => PixelType.Format.RGBA8888,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB565          => PixelType.Format.RGB565,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444        => PixelType.Format.RGBA4444,

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static int ToGLPixelFormat( PixelType.Format format )
    {
        var cformat = ToGdx2DPixelFormat( format );

        return cformat switch
        {
            Gdx2DPixmap.GDX_2D_FORMAT_ALPHA           => IGL.GL_ALPHA,
            Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA => IGL.GL_LUMINANCE_ALPHA,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB888          => IGL.GL_RGB,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB565          => IGL.GL_RGB,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888        => IGL.GL_RGBA,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444        => IGL.GL_RGBA,

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static int ToGLPixelFormat( int format )
    {
        return format switch
        {
            Gdx2DPixmap.GDX_2D_FORMAT_ALPHA           => IGL.GL_ALPHA,
            Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA => IGL.GL_LUMINANCE_ALPHA,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB888          => IGL.GL_RGB,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB565          => IGL.GL_RGB,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888        => IGL.GL_RGBA,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444        => IGL.GL_RGBA,

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    public static System.Drawing.Imaging.PixelFormat ToPixelFormat( PixelType.Format format )
    {
        var cformat = ToGdx2DPixelFormat( format );

        return cformat switch
        {
            Gdx2DPixmap.GDX_2D_FORMAT_ALPHA           => PixelFormat.Alpha,
            Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA => PixelFormat.Alpha,             // IGL.GL_LUMINANCE_ALPHA,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB888          => PixelFormat.Format32bppRgb,    // IGL.GL_RGB,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB565          => PixelFormat.Format16bppRgb565, // IGL.GL_RGB,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888        => PixelFormat.Format32bppArgb,   // IGL.GL_RGBA,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444        => PixelFormat.Format32bppArgb,   // IGL.GL_RGBA,

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static int ToGLDataType( PixelType.Format format )
    {
        var cformat = ToGdx2DPixelFormat( format );

        return cformat switch
        {
            Gdx2DPixmap.GDX_2D_FORMAT_ALPHA           => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB888          => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888        => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB565          => IGL.GL_UNSIGNED_SHORT_5_6_5,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444        => IGL.GL_UNSIGNED_SHORT_4_4_4_4,

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static int ToGLDataType( int format )
    {
        return format switch
        {
            Gdx2DPixmap.GDX_2D_FORMAT_ALPHA           => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB888          => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888        => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB565          => IGL.GL_UNSIGNED_SHORT_5_6_5,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444        => IGL.GL_UNSIGNED_SHORT_4_4_4_4,

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetFormatString( int format )
    {
        return format switch
        {
            Gdx2DPixmap.GDX_2D_FORMAT_ALPHA           => "GDX_2D_FORMAT_ALPHA",
            Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA => "GDX_2D_FORMAT_LUMINANCE_ALPHA",
            Gdx2DPixmap.GDX_2D_FORMAT_RGB888          => "GDX_2D_FORMAT_RGB888",
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888        => "GDX_2D_FORMAT_RGBA8888",
            Gdx2DPixmap.GDX_2D_FORMAT_RGB565          => "GDX_2D_FORMAT_RGB565",
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444        => "GDX_2D_FORMAT_RGBA4444",

            var _ => $"Invalid format: {format}",
        };
    }
    
    public static Pixmap ConvertToFormat( Pixmap pixmap, PixelType.Format targetFormat )
    {
        var convertedPixmap = new Pixmap( pixmap.Width, pixmap.Height, targetFormat );

//        Logger.Debug( "Original pixmap - 'pixmap'" );
//        Logger.Debug( $"Width: {pixmap.Width}, Height: {pixmap.Height}" );
//        Logger.Debug( $"GLDataType: {pixmap.GLDataType}" );
//        Logger.Debug( $"GLFormat: {PixmapFormat.GetGLPixelFormatName( pixmap.GLPixelFormat )}" );
//        Logger.Debug( $"Backing array length: {pixmap.PixelData.Length}" );

//        Logger.Debug( $"New pixmap - '{nameof( convertedPixmap )}'" );
//        Logger.Debug( $"Width: {convertedPixmap.Width}, Height: {convertedPixmap.Height}" );
//        Logger.Debug( $"GLDataType: {convertedPixmap.GLDataType}" );
//        Logger.Debug( $"GLFormat: {PixmapFormat.GetGLPixelFormatName( convertedPixmap.GLPixelFormat )}" );
//        Logger.Debug( $"Backing array length: {convertedPixmap.PixelData.Length}" );

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