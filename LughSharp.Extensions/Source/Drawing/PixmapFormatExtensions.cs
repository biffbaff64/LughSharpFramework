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
using JetBrains.Annotations;
using LughSharp.Core.Graphics;
using LughSharp.Core.Utils.Exceptions;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Extensions.Source.Drawing;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public static class PixmapFormatExtensions
{
    /// <summary>
    /// Converts a system pixel format to the corresponding <see cref="PixelFormat"/> format.
    /// </summary>
    /// <param name="format">The system pixel format to be converted.</param>
    /// <returns>
    /// The corresponding <see cref="PixelFormat"/> value for the given system pixel format.
    /// </returns>
    /// <exception cref="RuntimeException">
    /// Thrown if the provided system pixel format is invalid or not supported.
    /// </exception>
    public static int SystemPixelFormatToPixelFormat( PixelFormat format )
    {
        return format switch
        {
            PixelFormat.Alpha                => LughFormat.ALPHA,
            PixelFormat.Format16bppRgb565    => LughFormat.RGB565,
            PixelFormat.Format16bppGrayScale => LughFormat.RGBA4444,
            PixelFormat.Format24bppRgb       => LughFormat.RGB888,
            PixelFormat.Format32bppArgb      => LughFormat.RGBA8888,

            // ----------------------------------

            var _ => throw new RuntimeException( $"Invalid format: {format}" ),
        };
    }

    /// <summary>
    /// Converts a <c>Gdx2DPixmap.GDX_2D_FORMAT_XXX</c> to the corresponding <see cref="PixelFormat"/>.
    /// </summary>
    /// <param name="format">The pixel format of type <see cref="LughFormat"/>.</param>
    /// <returns>The corresponding <see cref="PixelFormat"/>.</returns>
    /// <exception cref="RuntimeException">
    /// Thrown if the provided format is invalid or unsupported.
    /// </exception>
    public static System.Drawing.Imaging.PixelFormat ToSystemPixelFormat( int format )
    {
        return format switch
        {
            LughFormat.ALPHA           => PixelFormat.Alpha,
            LughFormat.LUMINANCE_ALPHA => PixelFormat.Alpha,             // IGL.GL_LUMINANCE_ALPHA,
            LughFormat.RGB565          => PixelFormat.Format16bppRgb565, // IGL.GL_RGB,
            LughFormat.RGB888          => PixelFormat.Format32bppRgb,    // IGL.GL_RGB,
            LughFormat.RGBA8888        => PixelFormat.Format32bppArgb,   // IGL.GL_RGBA,
            LughFormat.RGBA4444        => PixelFormat.Format32bppArgb,   // IGL.GL_RGBA,

            // ----------------------------------

            var _ => throw new RuntimeException( $"Invalid format: {format}" ),
        };
    }
}

// ============================================================================
// ============================================================================