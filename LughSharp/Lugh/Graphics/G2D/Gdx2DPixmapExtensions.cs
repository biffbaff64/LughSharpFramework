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

using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.G2D;

/// <summary>
/// Contains native method extensions for Gdx2DPixmap
/// </summary>
public partial class Gdx2DPixmap
{
    /// <summary>
    /// Clears the pixmap with the specified color
    /// </summary>
    public override void ClearWithColor( Color color )
    {
        var size = ( uint )( _pixmapDataType.Width
                             * _pixmapDataType.Height
                             * PixelFormatUtils.Gdx2dBytesPerPixel( _pixmapDataType.ColorType ) );

        switch ( _pixmapDataType.ColorType )
        {
            case Gdx2DPixmapFormat.Alpha:
                ClearAlpha( _pixmapDataType, color, size );

                break;

            case Gdx2DPixmapFormat.LuminanceAlpha:
                ClearLuminanceAlpha( _pixmapDataType, color, size );

                break;

            case Gdx2DPixmapFormat.RGB888:
                ClearRGB888( _pixmapDataType, color, size );

                break;

            case Gdx2DPixmapFormat.RGBA8888:
                ClearRGBA8888( _pixmapDataType, color, size );

                break;

            case Gdx2DPixmapFormat.RGB565:
                ClearRGB565( _pixmapDataType, color, size );

                break;

            case Gdx2DPixmapFormat.RGBA4444:
                ClearRGBA4444( _pixmapDataType, color, size );

                break;

            default:
                throw new GdxRuntimeException( "Unknown color type" );
        }

        Array.Copy( _pixmapDataType.Pixels, PixmapBuffer.BackingArray(), _pixmapDataType.Pixels.Length );
    }

    // ========================================================================
    
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

    /// <summary>
    /// Clears the pixmap using a luminance and alpha combination derived from the specified color.
    /// </summary>
    /// <param name="pd">The pixmap data containing pixel information and dimensions to be modified.</param>
    /// <param name="color">The color used to calculate luminance and alpha values.</param>
    /// <param name="size">The total number of bytes to be processed in the pixel data.</param>
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

    /// <summary>
    /// Clears the pixmap data in the RGB888 format with the specified color.
    /// </summary>
    /// <param name="pd">The pixmap data type containing the pixel data and metadata.</param>
    /// <param name="color">The color used to clear the pixmap, specified in the ARGB color format.</param>
    /// <param name="size">The size of the pixmap data in bytes.</param>
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

    /// <summary>
    /// Clears the pixmap data using the RGBA8888 format with the specified color.
    /// </summary>
    /// <param name="pd">The pixmap data type containing the pixel data and metadata for the pixmap.</param>
    /// <param name="color">The color to clear the pixmap with as an instance of the <see cref="Color"/> class.</param>
    /// <param name="size">The size of the pixmap data in bytes, representing the total pixel data.</param>
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

    /// <summary>
    /// Clears the pixmap data of RGB565 format with the specified color.
    /// </summary>
    /// <param name="pd">The pixmap data to be modified.</param>
    /// <param name="color">The color used to clear the pixmap.</param>
    /// <param name="size">The total size of the pixmap data to be cleared.</param>
    private static void ClearRGB565( PixmapDataType pd, Color color, uint size )
    {
        var col = Color.RGB565( color );

        for ( var i = 0; i < size; i += 2 )
        {
            pd.Pixels[ i ]     = ( byte )( col & 0xFF );
            pd.Pixels[ i + 1 ] = ( byte )( ( col >> 8 ) & 0xFF );
        }
    }

    /// <summary>
    /// Clears the pixmap with a specified color in RGBA4444 format.
    /// </summary>
    /// <param name="pd">The pixmap data type containing pixel data and format information.</param>
    /// <param name="color">The color to fill the pixmap with, in RGBA4444 format.</param>
    /// <param name="size">The size of the pixmap data in bytes.</param>
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
    public static uint ToPixelFormat( Gdx2DPixmapFormat format, uint color )
    {
        uint r, g, b, a;

        switch ( format )
        {
            case Gdx2DPixmapFormat.Alpha:
                return color & 0xff;

            case Gdx2DPixmapFormat.LuminanceAlpha:
                r = ( color & 0xff000000 ) >> 24;
                g = ( color & 0xff0000 ) >> 16;
                b = ( color & 0xff00 ) >> 8;
                a = color & 0xff;
                var l = ( ( uint )( ( 0.2126f * r ) + ( 0.7152 * g ) + ( 0.0722 * b ) ) & 0xff ) << 8;

                return ( l & 0xffffff00 ) | a;

            case Gdx2DPixmapFormat.RGB888:
                return color >> 8;

            case Gdx2DPixmapFormat.RGBA8888:
                return color;

            case Gdx2DPixmapFormat.RGB565:
                r = ( ( ( color & 0xff000000 ) >> 27 ) << 11 ) & 0xf800;
                g = ( ( ( color & 0xff0000 ) >> 18 ) << 5 ) & 0x7e0;
                b = ( ( color & 0xff00 ) >> 11 ) & 0x1f;

                return r | g | b;

            case Gdx2DPixmapFormat.RGBA4444:
                r = ( ( ( color & 0xff000000 ) >> 28 ) << 12 ) & 0xf000;
                g = ( ( ( color & 0xff0000 ) >> 20 ) << 8 ) & 0xf00;
                b = ( ( ( color & 0xff00 ) >> 12 ) << 4 ) & 0xf0;
                a = ( ( color & 0xff ) >> 4 ) & 0xf;

                return r | g | b | a;

            default:
                return 0;
        }
    }

    /// <summary>
    /// Gets the pixel color at the specified coordinates
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <returns>The pixel color in the format of the pixmap</returns>
    public int GetPixelNative( int x, int y )
    {
        return NativeMethods.gdx2d_get_pixel( _pixmapDataType, x, y );
    }

    /// <summary>
    /// Sets the pixel color at the specified coordinates
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <param name="color">The color to set</param>
    public void SetPixelNative( int x, int y, Color color )
    {
        NativeMethods.gdx2d_set_pixel( _pixmapDataType, x, y, color.PackedColorABGR() );
    }

    /// <summary>
    /// Draws a line between two points
    /// </summary>
    /// <param name="x">Starting x coordinate</param>
    /// <param name="y">Starting y coordinate</param>
    /// <param name="x2">Ending x coordinate</param>
    /// <param name="y2">Ending y coordinate</param>
    /// <param name="color">Line color</param>
    public void DrawLineNative( int x, int y, int x2, int y2, Color color )
    {
        NativeMethods.gdx2d_draw_line( _pixmapDataType, x, y, x2, y2, color.PackedColorABGR() );
    }

    /// <summary>
    /// Draws a rectangle outline
    /// </summary>
    /// <param name="x">The x coordinate of the top-left corner</param>
    /// <param name="y">The y coordinate of the top-left corner</param>
    /// <param name="width">The width of the rectangle</param>
    /// <param name="height">The height of the rectangle</param>
    /// <param name="color">The outline color</param>
    public void DrawRectNative( int x, int y, uint width, uint height, Color color )
    {
        NativeMethods.gdx2d_draw_rect( _pixmapDataType, x, y, width, height, color.PackedColorABGR() );
    }

    /// <summary>
    /// Draws a circle outline
    /// </summary>
    /// <param name="x">The x coordinate of the center</param>
    /// <param name="y">The y coordinate of the center</param>
    /// <param name="radius">The radius of the circle</param>
    /// <param name="color">The outline color</param>
    public void DrawCircleNative( int x, int y, uint radius, Color color )
    {
        NativeMethods.gdx2d_draw_circle( _pixmapDataType, x, y, radius, color.PackedColorABGR() );
    }

    /// <summary>
    /// Fills a rectangle with a solid color
    /// </summary>
    /// <param name="x">The x coordinate of the top-left corner</param>
    /// <param name="y">The y coordinate of the top-left corner</param>
    /// <param name="width">The width of the rectangle</param>
    /// <param name="height">The height of the rectangle</param>
    /// <param name="color">The fill color</param>
    public void FillRectNative( int x, int y, uint width, uint height, Color color )
    {
        NativeMethods.gdx2d_fill_rect( _pixmapDataType, x, y, width, height, color.PackedColorABGR() );
    }

    /// <summary>
    /// Fills a circle with a solid color
    /// </summary>
    /// <param name="x">The x coordinate of the center</param>
    /// <param name="y">The y coordinate of the center</param>
    /// <param name="radius">The radius of the circle</param>
    /// <param name="color">The fill color</param>
    public void FillCircleNative( int x, int y, uint radius, Color color )
    {
        NativeMethods.gdx2d_fill_circle( _pixmapDataType, x, y, radius, color.PackedColorABGR() );
    }

    /// <summary>
    /// Fills a triangle with a solid color
    /// </summary>
    /// <param name="x1">The x coordinate of the first vertex</param>
    /// <param name="y1">The y coordinate of the first vertex</param>
    /// <param name="x2">The x coordinate of the second vertex</param>
    /// <param name="y2">The y coordinate of the second vertex</param>
    /// <param name="x3">The x coordinate of the third vertex</param>
    /// <param name="y3">The y coordinate of the third vertex</param>
    /// <param name="color">The fill color</param>
    public void FillTriangleNative( int x1, int y1, int x2, int y2, int x3, int y3, Color color )
    {
        NativeMethods.gdx2d_fill_triangle( _pixmapDataType, x1, y1, x2, y2, x3, y3, color.PackedColorABGR() );
    }

    /// <summary>
    /// Draws a pixmap at the specified location
    /// </summary>
    public void DrawPixmapNative( Gdx2DPixmap src, int srcX, int srcY, int dstX, int dstY, int width, int height )
    {
        NativeMethods.gdx2d_draw_pixmap( src._pixmapDataType, _pixmapDataType,
                                         srcX, srcY, width, height,
                                         dstX, dstY, width, height );
    }

    /// <summary>
    /// Draws a pixmap with scaling
    /// </summary>
    public void DrawPixmapNative( Gdx2DPixmap src,
                                  int srcX, int srcY,
                                  int dstX, int srcWidth, int srcHeight,
                                  int dstY, int dstWidth, int dstHeight )
    {
        NativeMethods.gdx2d_draw_pixmap( src._pixmapDataType, _pixmapDataType,
                                         srcX, srcY, srcWidth, srcHeight,
                                         dstX, dstY, dstWidth, dstHeight );
    }
}

// ============================================================================
// ============================================================================

/// <summary>
/// Native method imports for Gdx2DPixmap
/// </summary>
internal static class NativeMethods
{
    private const string DLL_PATH = "lib/gdx2d.dll";

    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
    internal static extern int gdx2d_get_pixel( PixmapDataType pd, int x, int y );

    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
    internal static extern void gdx2d_set_pixel( PixmapDataType pd, int x, int y, uint color );

    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
    internal static extern void gdx2d_draw_line( PixmapDataType pd, int x1, int y1, int x2, int y2, uint color );

    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
    internal static extern void gdx2d_draw_rect( PixmapDataType pd, int x, int y, uint width, uint height, uint color );

    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
    internal static extern void gdx2d_draw_circle( PixmapDataType pd, int x, int y, uint radius, uint color );

    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
    internal static extern void gdx2d_fill_rect( PixmapDataType pd, int x, int y, uint width, uint height, uint color );

    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
    internal static extern void gdx2d_fill_circle( PixmapDataType pd, int x, int y, uint radius, uint color );

    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
    internal static extern void gdx2d_fill_triangle( PixmapDataType pd,
                                                     int x1, int y1,
                                                     int x2, int y2,
                                                     int x3, int y3,
                                                     uint color );

    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
    internal static extern void gdx2d_draw_pixmap( PixmapDataType pd,
                                                   PixmapDataType dpd,
                                                   int srcX,
                                                   int srcY,
                                                   int srcWidth,
                                                   int srcHeight,
                                                   int dstX,
                                                   int dstY,
                                                   int dstWidth,
                                                   int dstHeight );

    // ========================================================================
    // ========================================================================

//TODO: Convert all of these to C#

//    [DllImport( DLL_PATH, SetLastError = true )]
//    private static extern Gdx2dPixmapStruct gdx2d_load( byte[] buffer, int len );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern Gdx2dPixmapStruct gdx2d_new( int width, int height, int format );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern string gdx2d_get_failure_reason();
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern void gdx2d_clear( Gdx2dPixmapStruct pd, uint color );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern int gdx2d_get_pixel( Gdx2dPixmapStruct pd, int x, int y );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern void gdx2d_set_pixel( Gdx2dPixmapStruct pd, int x, int y, uint color );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern void gdx2d_set_blend( Gdx2dPixmapStruct src, int blend );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern void gdx2d_set_scale( Gdx2dPixmapStruct src, int scale );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern uint gdx2d_bytes_per_pixel( uint format );
//
//    [DllImport( DLL_PATH, EntryPoint = "gdx2d_free", CallingConvention = CallingConvention.Cdecl )]
//    private static extern void free( Gdx2dPixmapStruct pixmap );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern void gdx2d_draw_line( Gdx2dPixmapStruct pd, int x0, int y0, int x1, int y1, uint col );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern void gdx2d_draw_rect( Gdx2dPixmapStruct pd, int x, int y, uint width, uint height, uint col );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern void gdx2d_draw_circle( Gdx2dPixmapStruct pd, int x, int y, uint radius, uint col );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern void gdx2d_fill_rect( Gdx2dPixmapStruct pd, int x, int y, uint width, uint height, uint col );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern void gdx2d_fill_circle( Gdx2dPixmapStruct pd, int x0, int y0, uint radius, uint col );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern void gdx2d_fill_triangle( Gdx2dPixmapStruct pd, int x1, int y1, int x2, int y2, int x3, int y3, uint col );
//
//    [DllImport( DLL_PATH, CallingConvention = CallingConvention.Cdecl )]
//    private static extern void gdx2d_draw_pixmap( Gdx2dPixmapStruct src,
//                                                  Gdx2dPixmapStruct dst,
//                                                  int srcX,
//                                                  int srcY,
//                                                  int srcWidth,
//                                                  int srcHeight,
//                                                  int dstX,
//                                                  int dstY,
//                                                  int dstWidth,
//                                                  int dstHeight );
}

// ========================================================================
// ========================================================================