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

using LughSharp.Lugh.Graphics.G2D;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Images;

[PublicAPI]
public class PixmapFormat
{
    /// <summary>
    /// 
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

            var _ => throw new GdxRuntimeException( $"Illegal PixelFormat specified: {format}" )
        };
    }

    /// <summary>
    /// 
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

            var _ => throw new GdxRuntimeException( $"Illegal PixelFormat specified: {format}" )
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static PixelType.Format PNGColorTypeToPixmapColorFormat( int format )
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
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetGLFormatName( int format )
    {
        return format switch
        {
            IGL.GL_ALPHA           => "IGL.GL_ALPHA",
            IGL.GL_LUMINANCE_ALPHA => "IGL.GL_LUMINANCE_ALPHA",
            IGL.GL_RGB             => "IGL.GL_RGB",
            IGL.GL_RGBA            => "IGL.GL_RGBA",

            var _ => $"Unknown format: {format}",
        };
    }

    /// <summary>
    /// 
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

            var _ => $"Unknown format: {format}",
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static int ToGdx2DFormat( PixelType.Format format )
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

            var _ => throw new GdxRuntimeException( $"unknown format: {format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static PixelType.Format ToPixmapColorFormat( int format )
    {
        return format switch
        {
            Gdx2DPixmap.GDX_2D_FORMAT_ALPHA           => PixelType.Format.Alpha,
            Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA => PixelType.Format.LuminanceAlpha,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB888          => PixelType.Format.RGB888,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888        => PixelType.Format.RGBA8888,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB565          => PixelType.Format.RGB565,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444        => PixelType.Format.RGBA4444,

            var _ => throw new GdxRuntimeException( $"unknown format: {format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static int ToGLFormat( PixelType.Format format )
    {
        var cformat = ToGdx2DFormat( format );

        return cformat switch
        {
            Gdx2DPixmap.GDX_2D_FORMAT_ALPHA           => IGL.GL_ALPHA,
            Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA => IGL.GL_LUMINANCE_ALPHA,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB888          => IGL.GL_RGB,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB565          => IGL.GL_RGB,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888        => IGL.GL_RGBA,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444        => IGL.GL_RGBA,

            var _ => throw new GdxRuntimeException( $"unknown format: {format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static int ToGLType( PixelType.Format format )
    {
        var cformat = ToGdx2DFormat( format );

        return cformat switch
        {
            Gdx2DPixmap.GDX_2D_FORMAT_ALPHA           => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB888          => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888        => IGL.GL_UNSIGNED_BYTE,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB565          => IGL.GL_UNSIGNED_SHORT_5_6_5,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444        => IGL.GL_UNSIGNED_SHORT_4_4_4_4,

            var _ => throw new GdxRuntimeException( $"unknown format: {format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    public static string GetFormatString( int format )
    {
        var cformat = OglToPngColor( format );

        return cformat switch
        {
            PixelType.Format.Alpha          => "GDX_2D_FORMAT_ALPHA",
            PixelType.Format.LuminanceAlpha => "GDX_2D_FORMAT_LUMINANCE_ALPHA",
            PixelType.Format.RGB888         => "GDX_2D_FORMAT_RGB888",
            PixelType.Format.RGBA8888       => "GDX_2D_FORMAT_RGBA8888",
            PixelType.Format.RGB565         => "GDX_2D_FORMAT_RGB565",
            PixelType.Format.RGBA4444       => "GDX_2D_FORMAT_RGBA4444",

            var _ => $"Unknown format: {format}",
        };
    }
}