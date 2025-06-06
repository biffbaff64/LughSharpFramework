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

namespace LughSharp.Lugh.Graphics.Images;

public partial class PixmapData
{
    /// <summary>
    /// Clears the pixmap with the specified color
    /// </summary>
    public override void Clear( Color color )
    {
        var size = ( uint )( _pixmapDataType.Width
                             * _pixmapDataType.Height
                             * PixmapFormat.Gdx2dBytesPerPixel( ( int )_pixmapDataType.ColorType ) );

        switch ( _pixmapDataType.ColorType )
        {
            case GDX_2D_FORMAT_ALPHA:
                ClearAlpha( _pixmapDataType, color, size );

                break;

            case GDX_2D_FORMAT_LUMINANCE_ALPHA:
                ClearLuminanceAlpha( _pixmapDataType, color, size );

                break;

            case GDX_2D_FORMAT_RGB888:
                ClearRGB888( _pixmapDataType, color, size );

                break;

            case GDX_2D_FORMAT_RGBA8888:
                ClearRGBA8888( _pixmapDataType, color, size );

                break;

            case GDX_2D_FORMAT_RGB565:
                ClearRGB565( _pixmapDataType, color, size );

                break;

            case GDX_2D_FORMAT_RGBA4444:
                ClearRGBA4444( _pixmapDataType, color, size );

                break;
        }

        Array.Copy( _pixmapDataType.Pixels, PixmapBuffer.BackingArray(), _pixmapDataType.Pixels.Length );
    }

    /// <summary>
    /// Clears the pixmap by setting the alpha channel to the specified color's alpha.
    /// </summary>
    /// <param name="pd">The pixmap data to be cleared.</param>
    /// <param name="color">The color whose alpha channel will be used in the clearing process.</param>
    /// <param name="size">The total size of the pixmap in bytes.</param>
    private static void ClearAlpha( PixmapDataType pd, Color color, uint size )
    {
        var alpha = ( byte )( color.A * 255 );
        Array.Fill( pd.Pixels, alpha, 0, ( int )( pd.Width * pd.Height ) );
    }

    private static void ClearLuminanceAlpha( PixmapDataType pd, Color color, uint size )
    {
        var luminance = ( byte )( ( ( 0.2126f * color.R )
                                    + ( 0.7152f * color.G )
                                    + ( 0.0722f * color.B ) ) * 255 );
        var alpha = ( byte )( color.A * 255 );

        for ( var i = 0; i < size; i += 2 )
        {
            pd.Pixels[ i ]     = luminance;
            pd.Pixels[ i + 1 ] = alpha;
        }
    }

    private static void ClearRGB888( PixmapDataType pd, Color color, uint size )
    {
        var col = Color.RGB888( color );
        var b   = ( byte )( ( col & 0x0000ff00 ) >> 8 );
        var g   = ( byte )( ( col & 0x00ff0000 ) >> 16 );
        var r   = ( byte )( ( col & 0xff000000 ) >> 24 );

        for ( var pixel = 0; pixel < size; )
        {
            pd.Pixels[ pixel++ ] = b;
            pd.Pixels[ pixel++ ] = g;
            pd.Pixels[ pixel++ ] = r;
        }
    }

    private static void ClearRGBA8888( PixmapDataType pd, Color color, uint size )
    {
        var col = Color.RGBA8888( color );
        var a   = ( byte )( col & 0x000000ff );
        var b   = ( byte )( ( col & 0x0000ff00 ) >> 8 );
        var g   = ( byte )( ( col & 0x00ff0000 ) >> 16 );
        var r   = ( byte )( ( col & 0xff000000 ) >> 24 );

        for ( var pixel = 0; pixel < size; )
        {
            pd.Pixels[ pixel++ ] = a;
            pd.Pixels[ pixel++ ] = b;
            pd.Pixels[ pixel++ ] = g;
            pd.Pixels[ pixel++ ] = r;
        }
    }

    private static void ClearRGB565( PixmapDataType pd, Color color, uint size )
    {
        var col = Color.RGB565( color );

        for ( var i = 0; i < size; i += 2 )
        {
            pd.Pixels[ i ]     = ( byte )( col & 0xFF );
            pd.Pixels[ i + 1 ] = ( byte )( ( col >> 8 ) & 0xFF );
        }
    }

    private static void ClearRGBA4444( PixmapDataType pd, Color color, uint size )
    {
        var col = Color.RGBA4444( color );

        for ( var i = 0; i < size; i += 2 )
        {
            pd.Pixels[ i ]     = ( byte )( col & 0xFF );
            pd.Pixels[ i + 1 ] = ( byte )( ( col >> 8 ) & 0xFF );
        }
    }

    /// <summary>
    /// Converts a color to the specified pixel format
    /// </summary>
    public static uint ToPixelFormat( uint format, uint color )
    {
        uint r, g, b, a;

        switch ( format )
        {
            case GDX_2D_FORMAT_ALPHA:
                return color & 0xff;

            case GDX_2D_FORMAT_LUMINANCE_ALPHA:
                r = ( color & 0xff000000 ) >> 24;
                g = ( color & 0xff0000 ) >> 16;
                b = ( color & 0xff00 ) >> 8;
                a = color & 0xff;
                var l = ( ( uint )( ( 0.2126f * r ) + ( 0.7152 * g ) + ( 0.0722 * b ) ) & 0xff ) << 8;

                return ( l & 0xffffff00 ) | a;

            case GDX_2D_FORMAT_RGB888:
                return color >> 8;

            case GDX_2D_FORMAT_RGBA8888:
                return color;

            case GDX_2D_FORMAT_RGB565:
                r = ( ( ( color & 0xff000000 ) >> 27 ) << 11 ) & 0xf800;
                g = ( ( ( color & 0xff0000 ) >> 18 ) << 5 ) & 0x7e0;
                b = ( ( color & 0xff00 ) >> 11 ) & 0x1f;

                return r | g | b;

            case GDX_2D_FORMAT_RGBA4444:
                r = ( ( ( color & 0xff000000 ) >> 28 ) << 12 ) & 0xf000;
                g = ( ( ( color & 0xff0000 ) >> 20 ) << 8 ) & 0xf00;
                b = ( ( ( color & 0xff00 ) >> 12 ) << 4 ) & 0xf0;
                a = ( ( color & 0xff ) >> 4 ) & 0xf;

                return r | g | b | a;

            default:
                return 0;
        }
    }
}