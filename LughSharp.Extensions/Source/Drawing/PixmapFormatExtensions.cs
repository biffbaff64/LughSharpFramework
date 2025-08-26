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
using LughSharp.Lugh.Utils.Exceptions;

namespace Extensions.Source.Drawing;

[PublicAPI]
public static class PixmapFormatExtensions
{
    /// <summary>
    /// Converts a Pixmap pixel format to the corresponding PixelType format.
    /// </summary>
    /// <param name="format">The Pixmap pixel format to be converted.</param>
    /// <returns>The corresponding <see cref="Gdx2DPixmap.Gdx2DPixmapFormat"/> for the given Pixmap pixel format.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the provided Pixmap pixel format is invalid or not supported.
    /// </exception>
    public static Gdx2DPixmap.Gdx2DPixmapFormat SystemPixelFormatToGdx2DFormat( PixelFormat format )
    {
        return format switch
        {
            PixelFormat.Alpha                => Gdx2DPixmap.Gdx2DPixmapFormat.Alpha,
            PixelFormat.Format24bppRgb       => Gdx2DPixmap.Gdx2DPixmapFormat.RGB888,
            PixelFormat.Format32bppArgb      => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888,
            PixelFormat.Format16bppRgb565    => Gdx2DPixmap.Gdx2DPixmapFormat.RGB565,
            PixelFormat.Format16bppGrayScale => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// Converts a <see cref="Gdx2DPixmap.Gdx2DPixmapFormat"/> to the corresponding System.Drawing.Imaging.PixelFormat.
    /// </summary>
    /// <param name="format">The pixel format of type <see cref="Gdx2DPixmap.Gdx2DPixmapFormat"/>.</param>
    /// <returns>The corresponding <see cref="System.Drawing.Imaging.PixelFormat"/>.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the provided format is invalid or unsupported.
    /// </exception>
    public static PixelFormat ToSystemPixelFormat( Gdx2DPixmap.Gdx2DPixmapFormat format )
    {
        return format switch
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
}

// ============================================================================
// ============================================================================