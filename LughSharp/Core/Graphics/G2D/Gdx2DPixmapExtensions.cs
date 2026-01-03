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

using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics.G2D;

/// <summary>
/// Contains native method extensions for Gdx2DPixmap
/// </summary>
public partial class Gdx2DPixmap
{
    /// <summary>
    /// Clears the pixmap with the specified color
    /// </summary>
    public void ClearWithColor( Color color )
    {
        var size = ( uint )( Width * Height * PixelFormat.BytesPerPixel( ColorFormat ) );

        switch ( ColorFormat )
        {
            case LughFormat.ALPHA:
                ClearAlpha( color, size );

                break;

            case LughFormat.LUMINANCE_ALPHA:
                ClearLuminanceAlpha( color, size );

                break;

            case LughFormat.RGB888:
                ClearRGB888( color, size );

                break;

            case LughFormat.RGBA8888:
                ClearRGBA8888( color, size );

                break;

            case LughFormat.RGB565:
                ClearRGB565( color, size );

                break;

            case LughFormat.RGBA4444:
                ClearRGBA4444( color, size );

                break;

            case LughFormat.INDEXED_COLOR:
                ClearIndexedColor( color, size );

                break;

            case LughFormat.INVALID:
            default:
                throw new GdxRuntimeException( "Unknown color type" );
        }

        Array.Copy( Pixels, PixmapBuffer.BackingArray(), Pixels.Length );
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="color"></param>
    public static void Clear( int color )
    {
    }

    /// <summary>
    /// Clears the pixmap by setting the alpha channel to the specified color's alpha.
    /// </summary>
    /// <param name="color">The color whose alpha channel will be used in the clearing process.</param>
    /// <param name="size">The total size of the pixmap in bytes.</param>
    private void ClearAlpha( Color color, uint size )
    {
        Array.Fill( Pixels, ( byte )( color.A * 255 ), 0, ( int )( Width * Height ) );
    }

    /// <summary>
    /// Clears the pixmap using a luminance and alpha combination derived from the specified color.
    /// </summary>
    /// <param name="color">The color used to calculate luminance and alpha values.</param>
    /// <param name="size">The total number of bytes to be processed in the pixel data.</param>
    private void ClearLuminanceAlpha( Color color, uint size )
    {
        var luminance = ( byte )( ( ( 0.2126f * color.R )
                                    + ( 0.7152f * color.G )
                                    + ( 0.0722f * color.B ) ) * 255 );
        var alpha = ( byte )( color.A * 255 );

        for ( var i = 0; i < size; i += 2 )
        {
            Pixels[ i ]     = luminance;
            Pixels[ i + 1 ] = alpha;
        }
    }

    /// <summary>
    /// Clears the pixmap data in the GDX_2D_FORMAT_RGB888 format with the specified color.
    /// </summary>
    /// <param name="color">The color used to clear the pixmap, specified in the ARGB color format.</param>
    /// <param name="size">The size of the pixmap data in bytes.</param>
    private void ClearRGB888( Color color, uint size )
    {
        var col = Color.RGB888( color );
        var b   = ( byte )( ( col & 0x0000ff00 ) >> 8 );
        var g   = ( byte )( ( col & 0x00ff0000 ) >> 16 );
        var r   = ( byte )( ( col & 0xff000000 ) >> 24 );

        for ( var pixel = 0; pixel < size; )
        {
            Pixels[ pixel++ ] = b;
            Pixels[ pixel++ ] = g;
            Pixels[ pixel++ ] = r;
        }
    }

    /// <summary>
    /// Clears the pixmap data using the RGBA8888 format with the specified color.
    /// </summary>
    /// <param name="color">The color to clear the pixmap with as an instance of the <see cref="Color"/> class.</param>
    /// <param name="size">The size of the pixmap data in bytes, representing the total pixel data.</param>
    private void ClearRGBA8888( Color color, uint size )
    {
        if ( ( size % 4 ) != 0 )
        {
            throw new GdxRuntimeException( "Invalid size for RGBA8888 format" );
        }
        
        var col = Color.RGBA8888( color );
        var a   = ( byte )( col & 0x000000ff );
        var b   = ( byte )( ( col & 0x0000ff00 ) >> 8 );
        var g   = ( byte )( ( col & 0x00ff0000 ) >> 16 );
        var r   = ( byte )( ( col & 0xff000000 ) >> 24 );

        for ( var pixel = 0; pixel < size; )
        {
            Pixels[ pixel++ ] = a;
            Pixels[ pixel++ ] = b;
            Pixels[ pixel++ ] = g;
            Pixels[ pixel++ ] = r;
        }
    }

    /// <summary>
    /// Clears the pixmap data of RGB565 format with the specified color.
    /// </summary>
    /// <param name="color">The color used to clear the pixmap.</param>
    /// <param name="size">The total size of the pixmap data to be cleared.</param>
    private void ClearRGB565( Color color, uint size )
    {
        var col = Color.RGB565( color );

        for ( var i = 0; i < size; i += 2 )
        {
            Pixels[ i ]     = ( byte )( col & 0xFF );
            Pixels[ i + 1 ] = ( byte )( ( col >> 8 ) & 0xFF );
        }
    }

    /// <summary>
    /// Clears the pixmap with a specified color in RGBA4444 format.
    /// </summary>
    /// <param name="color">The color to fill the pixmap with, in RGBA4444 format.</param>
    /// <param name="size">The size of the pixmap data in bytes.</param>
    private void ClearRGBA4444( Color color, uint size )
    {
        var col = Color.RGBA4444( color );

        for ( var i = 0; i < size; i += 2 )
        {
            Pixels[ i ]     = ( byte )( col & 0xFF );
            Pixels[ i + 1 ] = ( byte )( ( col >> 8 ) & 0xFF );
        }
    }

    /// <summary>
    /// Clears the pixmap data using indexed color format with the specified color.
    /// </summary>
    /// <param name="color">The color to fill the pixmap with, in IndexedColor format.</param>
    /// <param name="size">The size of the pixmap data in bytes.</param>
    private void ClearIndexedColor( Color color, uint size )
    {
        //TODO:
    }

    /// <summary>
    /// Clears the pixmap data using Intensity color format with the specified color.
    /// </summary>
    /// <param name="color">The color to fill the pixmap with, in Intensity format.</param>
    /// <param name="size">The size of the pixmap data in bytes.</param>
    private void ClearIntensity( Color color, uint size )
    {
        //TODO:
    }

    // ========================================================================

    /// <summary>
    /// Converts a color to the specified pixel format
    /// </summary>
    public static uint ToPixelFormat( int format, uint color )
    {
        uint r, g, b, a;

        switch ( format )
        {
            case LughFormat.ALPHA:
                return color & 0xff;

            case LughFormat.LUMINANCE_ALPHA:
                r = ( color & 0xff000000 ) >> 24;
                g = ( color & 0xff0000 ) >> 16;
                b = ( color & 0xff00 ) >> 8;
                a = color & 0xff;
                var l = ( ( uint )( ( 0.2126f * r ) + ( 0.7152 * g ) + ( 0.0722 * b ) ) & 0xff ) << 8;

                return ( l & 0xffffff00 ) | a;

            case LughFormat.RGB888:
                return color >> 8;

            case LughFormat.RGB565:
                r = ( ( ( color & 0xff000000 ) >> 27 ) << 11 ) & 0xf800;
                g = ( ( ( color & 0xff0000 ) >> 18 ) << 5 ) & 0x7e0;
                b = ( ( color & 0xff00 ) >> 11 ) & 0x1f;

                return r | g | b;

            case LughFormat.RGBA4444:
                r = ( ( ( color & 0xff000000 ) >> 28 ) << 12 ) & 0xf000;
                g = ( ( ( color & 0xff0000 ) >> 20 ) << 8 ) & 0xf00;
                b = ( ( ( color & 0xff00 ) >> 12 ) << 4 ) & 0xf0;
                a = ( ( color & 0xff ) >> 4 ) & 0xf;

                return r | g | b | a;

//            case PixelFormat.INTENSITY:
            //TODO:
            case LughFormat.INDEXED_COLOR:
            //TODO:
            case LughFormat.RGBA8888:
                return color;

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
        return gdx2d_get_pixel( x, y );
    }

    /// <summary>
    /// Sets the pixel color at the specified coordinates
    /// </summary>
    /// <param name="x">The x coordinate</param>
    /// <param name="y">The y coordinate</param>
    /// <param name="color">The color to set</param>
    public void SetPixelNative( int x, int y, Color color )
    {
        gdx2d_set_pixel( x, y, color.PackedColorABGR() );
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
        gdx2d_draw_line( x, y, x2, y2, color.PackedColorABGR() );
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
        gdx2d_draw_rect( x, y, width, height, color.PackedColorABGR() );
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
        gdx2d_draw_circle( x, y, radius, color.PackedColorABGR() );
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
        gdx2d_fill_rect( x, y, width, height, color.PackedColorABGR() );
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
        gdx2d_fill_circle( x, y, radius, color.PackedColorABGR() );
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
        gdx2d_fill_triangle( new NativePixmapStruct(), x1, y1, x2, y2, x3, y3, color.PackedColorABGR() );
    }

    /// <summary>
    /// Draws an area of the source pixmap onto this pixmap.
    /// </summary>
    /// <param name="src"> The source pixmap. </param>
    /// <param name="srcX"> The source X coordinate. </param>
    /// <param name="srcY"> The source Y coordinate. </param>
    /// <param name="dstX"> The destination X coordinate. </param>
    /// <param name="dstY"> The destination Y coordinate. </param>
    /// <param name="width"> The width of the area. </param>
    /// <param name="height"> The height of the area. </param>
    public void DrawPixmapNative( Gdx2DPixmap src, int srcX, int srcY, int dstX, int dstY, int width, int height )
    {
        gdx2d_draw_pixmap( new NativePixmapStruct(),
                           new NativePixmapStruct(),
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
        gdx2d_draw_pixmap( new NativePixmapStruct(),
                           new NativePixmapStruct(),
                           srcX, srcY, srcWidth, srcHeight,
                           dstX, dstY, dstWidth, dstHeight );
    }

    // ========================================================================
    // ========================================================================

    private const string DLL_PATH = "lib/net8.0/gdx2d.dll";

    // ------------------------------------------

    [DllImport( DLL_PATH, EntryPoint = "gdx2d_load", CallingConvention = CallingConvention.Cdecl )]
    private static extern IntPtr gdx2d_load( IntPtr nativeData, int len );

    [DllImport( DLL_PATH, EntryPoint = "gdx2d_new", CallingConvention = CallingConvention.Cdecl )]
    private static extern IntPtr gdx2d_new( [In] [Out] long[] nativeData, int width, int height, int format );

    [DllImport( DLL_PATH, EntryPoint = "gdx2d_get_pixel", CallingConvention = CallingConvention.Cdecl )]
    private static extern int gdx2d_get_pixel( int x, int y );

    [DllImport( DLL_PATH, EntryPoint = "gdx2d_set_pixel", CallingConvention = CallingConvention.Cdecl )]
    private static extern void gdx2d_set_pixel( int x, int y, uint color );

    [DllImport( DLL_PATH, EntryPoint = "gdx2d_draw_line", CallingConvention = CallingConvention.Cdecl )]
    private static extern void gdx2d_draw_line( int x1, int y1, int x2, int y2, uint color );

    [DllImport( DLL_PATH, EntryPoint = "gdx2d_draw_rect", CallingConvention = CallingConvention.Cdecl )]
    private static extern void gdx2d_draw_rect( int x, int y, uint width, uint height, uint color );

    [DllImport( DLL_PATH, EntryPoint = "gdx2d_draw_circle", CallingConvention = CallingConvention.Cdecl )]
    private static extern void gdx2d_draw_circle( int x, int y, uint radius, uint color );

    [DllImport( DLL_PATH, EntryPoint = "gdx2d_fill_rect", CallingConvention = CallingConvention.Cdecl )]
    private static extern void gdx2d_fill_rect( int x, int y, uint width, uint height, uint color );

    [DllImport( DLL_PATH, EntryPoint = "gdx2d_fill_circle", CallingConvention = CallingConvention.Cdecl )]
    private static extern void gdx2d_fill_circle( int x, int y, uint radius, uint color );

    [DllImport( DLL_PATH, EntryPoint = "gdx2d_fill_triangle", CallingConvention = CallingConvention.Cdecl )]
    private static extern void gdx2d_fill_triangle( NativePixmapStruct pd,
                                                    int x1, int y1,
                                                    int x2, int y2,
                                                    int x3, int y3,
                                                    uint color );

    [DllImport( DLL_PATH, EntryPoint = "gdx2d_draw_pixmap", CallingConvention = CallingConvention.Cdecl )]
    private static extern void gdx2d_draw_pixmap( NativePixmapStruct pd,
                                                  NativePixmapStruct dpd,
                                                  int srcX,
                                                  int srcY,
                                                  int srcWidth,
                                                  int srcHeight,
                                                  int dstX,
                                                  int dstY,
                                                  int dstWidth,
                                                  int dstHeight );

    [DllImport( DLL_PATH, EntryPoint = "gdx2d_clear", CallingConvention = CallingConvention.Cdecl )]
    private static extern void gdx2d_clear( uint color );
}

// ========================================================================
// ========================================================================