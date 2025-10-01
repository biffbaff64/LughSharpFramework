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

using LughSharp.Lugh.Graphics;
using LughSharp.Lugh.Graphics.G2D;
using LughUtils.source.Exceptions;

namespace Extensions.Source.Drawing;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public static class PixmapFormatExtensions
{
    /// <summary>
    /// Converts a Pixmap pixel format to the corresponding PixelType format.
    /// </summary>
    /// <param name="format">The Pixmap pixel format to be converted.</param>
    /// <returns>The corresponding <c>Gdx2DPixmap.GDX_2D_FORMAT_XXX</c> for the given Pixmap pixel format.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the provided Pixmap pixel format is invalid or not supported.
    /// </exception>
    public static Pixmap.Format SystemPixelFormatToPixmapFormat( PixelFormat format )
    {
        return format switch
        {
            PixelFormat.Alpha                => Pixmap.Format.Alpha,
            PixelFormat.Format24bppRgb       => Pixmap.Format.RGB888,
            PixelFormat.Format32bppArgb      => Pixmap.Format.RGBA8888,
            PixelFormat.Format16bppRgb565    => Pixmap.Format.RGB565,
            PixelFormat.Format16bppGrayScale => Pixmap.Format.RGBA4444,

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
    public static PixelFormat ToSystemPixelFormat( Pixmap.Format format )
    {
        return format switch
        {
            Pixmap.Format.Alpha           => PixelFormat.Alpha,
            Pixmap.Format.LuminanceAlpha => PixelFormat.Alpha,             // IGL.GL_LUMINANCE_ALPHA,
            Pixmap.Format.RGB888          => PixelFormat.Format32bppRgb,    // IGL.GL_RGB,
            Pixmap.Format.RGB565          => PixelFormat.Format16bppRgb565, // IGL.GL_RGB,
            Pixmap.Format.RGBA8888        => PixelFormat.Format32bppArgb,   // IGL.GL_RGBA,
            Pixmap.Format.RGBA4444        => PixelFormat.Format32bppArgb,   // IGL.GL_RGBA,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( $"Invalid format: {format}" ),
        };
    }
}

// ============================================================================
// ============================================================================