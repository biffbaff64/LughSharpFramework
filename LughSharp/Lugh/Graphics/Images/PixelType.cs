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

using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Images;

[PublicAPI]
public class PixelType
{
    [PublicAPI]
    public enum Format : int
    {
        [EnumMember( Value = "Dummy" )]
        Dummy = 0,

        // ----------
        [EnumMember( Value = "Alpha" )]
        Alpha          = 1,
        
        [EnumMember( Value = "LuminanceAlpha" )]
        LuminanceAlpha = 2,
        
        [EnumMember( Value = "RGB888" )]
        RGB888         = 3,
        
        [EnumMember( Value = "RGBA8888" )]
        RGBA8888       = 4,

        [EnumMember( Value = "RGB565" )]
        RGB565         = 5,
        
        [EnumMember( Value = "RBGA4444" )]
        RGBA4444       = 6,
        
        [EnumMember( Value = "Intensity" )]
        Intensity      = 7,

        // ----------=
        [EnumMember( Value = "Default" )]
        Default = RGBA8888,
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static int ToGdx2DPixmapPixelFormat( Format format )
    {
        return format switch
        {
            Format.Alpha          => Gdx2DPixmap.GDX_2D_FORMAT_ALPHA,
            Format.Intensity      => Gdx2DPixmap.GDX_2D_FORMAT_ALPHA,
            Format.LuminanceAlpha => Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA,
            Format.RGB565         => Gdx2DPixmap.GDX_2D_FORMAT_RGB565,
            Format.RGBA4444       => Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444,
            Format.RGB888         => Gdx2DPixmap.GDX_2D_FORMAT_RGB888,
            Format.RGBA8888       => Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888,

            var _ => throw new GdxRuntimeException( "Unknown Format: " + format ),
        };
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    public static Format FromGdx2DPixmapPixelFormat( int format )
    {
        return format switch
        {
            Gdx2DPixmap.GDX_2D_FORMAT_ALPHA           => Format.Alpha,
            Gdx2DPixmap.GDX_2D_FORMAT_LUMINANCE_ALPHA => Format.LuminanceAlpha,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB565          => Format.RGB565,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA4444        => Format.RGBA4444,
            Gdx2DPixmap.GDX_2D_FORMAT_RGB888          => Format.RGB888,
            Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888        => Format.RGBA8888,

            var _ => throw new GdxRuntimeException( "Unknown Gdx2DPixmap Format: " + format ),
        };
    }

    // ========================================================================

    /// <inheritdoc cref="PixmapFormat.ToGLPixelFormat(LughSharp.Lugh.Graphics.Images.PixelType.Format)" />
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
}