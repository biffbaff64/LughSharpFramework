// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Images;

[PublicAPI]
public static class PixmapFormatExtensions
{
    public static int ToGLType( this Pixmap.PixelFormat format )
    {
        return Gdx2DPixmap.ToGLType( ToGdx2DPixmapFormat( format ) );
    }

    public static int ToGLFormat( this Pixmap.PixelFormat format )
    {
        return Gdx2DPixmap.ToGLFormat( ToGdx2DPixmapFormat( format ) );
    }

    public static int ToGdx2DPixmapFormat( this Pixmap.PixelFormat format )
    {
        return format switch
        {
            Pixmap.PixelFormat.Alpha          => Gdx2DPixmap.GDX_2D_FORMAT_ALPHA,
            Pixmap.PixelFormat.Intensity      => Gdx2DPixmap.GDX_2D_FORMAT_ALPHA,
            Pixmap.PixelFormat.LuminanceAlpha => Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA,
            Pixmap.PixelFormat.RGB565         => Gdx2DPixmap.GDX_2D_FORMAT_RGB565,
            Pixmap.PixelFormat.RGBA4444       => Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444,
            Pixmap.PixelFormat.RGB888         => Gdx2DPixmap.GDX_2D_FORMAT_RGB888,
            Pixmap.PixelFormat.RGBA8888       => Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888,

            var _ => throw new GdxRuntimeException( $"Unknown format: {format}" ),
        };
    }
}