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

using System.Runtime.Serialization;

using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Pixels;

[PublicAPI]
public class PixelType
{
    [PublicAPI]
    public enum Format : int
    {
        Dummy = 0,

        // ----------
        Alpha          = 1,
        LuminanceAlpha = 2,
        RGB888         = 3,
        RGBA8888       = 4,
        RGB565         = 5,
        RGBA4444       = 6,
        Intensity      = 7,

        // ----------------------------------

        Default = RGBA8888,
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static Gdx2DPixmap.Gdx2DPixmapFormat ToGdx2DPixmapPixelFormat( Format format )
    {
        return format switch
        {
            Format.Alpha          => Gdx2DPixmap.Gdx2DPixmapFormat.Alpha,
            Format.Intensity      => Gdx2DPixmap.Gdx2DPixmapFormat.Alpha,
            Format.LuminanceAlpha => Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha,
            Format.RGB565         => Gdx2DPixmap.Gdx2DPixmapFormat.RGB565,
            Format.RGBA4444       => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444,
            Format.RGB888         => Gdx2DPixmap.Gdx2DPixmapFormat.RGB888,
            Format.RGBA8888       => Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888,

            // ----------------------------------
            
            var _ => throw new GdxRuntimeException( "Unknown Format: " + format ),
        };
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static Format FromGdx2DPixmapPixelFormat( Gdx2DPixmap.Gdx2DPixmapFormat format )
    {
        return format switch
        {
            Gdx2DPixmap.Gdx2DPixmapFormat.Alpha          => Format.Alpha,
            Gdx2DPixmap.Gdx2DPixmapFormat.LuminanceAlpha => Format.LuminanceAlpha,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB565         => Format.RGB565,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA4444       => Format.RGBA4444,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGB888         => Format.RGB888,
            Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888       => Format.RGBA8888,

            // ----------------------------------
            
            var _ => throw new GdxRuntimeException( "Unknown Gdx2DPixmap Format: " + format ),
        };
    }

    // ========================================================================

    /// <summary>
    /// Calculates and returns the number of bytes per pixel for the given pixel format.
    /// </summary>
    /// <param name="format">The pixel format for which to calculate the bytes per pixel.</param>
    /// <returns>The number of bytes per pixel corresponding to the specified format.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown when the pixel format is unknown or invalid.
    /// </exception>
    public static int BytesPerPixel( Format format )
    {
        return format switch
        {
            Format.Alpha          => 1,
            Format.Intensity      => 1,
            Format.LuminanceAlpha => 2,
            Format.RGB565         => 2,
            Format.RGBA4444       => 2,
            Format.RGB888         => 3,
            Format.RGBA8888       => 4,

            // ----------------------------------

            var _ => throw new GdxRuntimeException( "Unknown Format: " + format ),
        };
    }

    // ========================================================================

    /// <inheritdoc cref="PixmapFormat.ToGLPixelFormat(PixelType.Format)" />
    public static int ToGLPixelFormat( Format format )
    {
        return PixmapFormat.ToGLPixelFormat( ToGdx2DPixmapPixelFormat( format ) );
    }

    // ========================================================================

    /// <inheritdoc cref="PixmapFormat.ToGLDataType(Format)" />
    public static int ToGLDataType( Format format )
    {
        return PixmapFormat.ToGLDataType( ToGdx2DPixmapPixelFormat( format ) );
    }

    // ========================================================================

    public static Format FromRgba( int r, int g, int b, int a )
    {
        return Format.RGBA8888; //TODO:
    }
}