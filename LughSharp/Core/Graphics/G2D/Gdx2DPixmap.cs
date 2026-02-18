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

using System.Runtime.InteropServices;
using JetBrains.Annotations;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics.G2D;

/// <summary>
/// Provides functionality for managing and manipulating 2D pixel maps with various
/// color formats, blending modes, and scaling options.  Gdx2DPixmap is designed to
/// handle image processing and manipulation at the pixel level. It supports multiple
/// color formats, allows drawing operations, and facilitates scaling and format
/// conversion for efficient rendering in 2D graphics. The class also allows management
/// of associated buffer data and provides constructors for different initialization scenarios.
/// </summary>
[PublicAPI]
public class Gdx2DPixmap : IDisposable
{
    /// <summary>
    /// Struct holding data for a pixmap which is compatible with the native gdx2d library.
    /// <br></br>
    /// <para>
    /// <b> IMPORTANT: </b> This struct is used for marshalling data between managed and
    /// unmanaged code. It is vital that the layout of this struct is identical to the
    /// equivalent struct in the native library.
    /// <para>
    /// <b>DO NOT MODIFY THIS STRUCT WITHOUT ALSO UPDATING THE NATIVE STRUCT DEFINITION.</b>
    /// </para>
    /// </para>
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    private struct NativePixmapStruct
    {
        public uint Width;
        public uint Height;
        public uint ColorFormat;
        public uint Blend;
        public uint Scale;
        public IntPtr Pixels;
    }

    // ========================================================================

    public const int GDX_2D_IGNORE = 0;

    // ========================================================================

    public const int GDX_2D_SCALE_NEAREST  = 0;
    public const int GDX_2D_SCALE_LINEAR   = 1;
    public const int GDX_2D_SCALE_BILINEAR = 1;

    // ========================================================================

    public const int GDX_2D_BLEND_NONE     = 0;
    public const int GDX_2D_BLEND_SRC_OVER = 1;

    // ========================================================================

    public Buffer< byte > PixmapBuffer  { get; set; }
    public long           TotalIDATSize { get; set; }

    public int    Width         { get; set; }
    public int    Height        { get; set; }
    public int    ColorFormat   { get; set; }
    public byte   BitDepth      { get; set; }
    public int    BytesPerPixel { get; set; }
    public uint   Blend         { get; set; }
    public uint   Scale         { get; set; }
    public byte[] Pixels        { get; set; } = [ ];

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new Gdx2DPixmap instance using data from the supplied buffer.
    /// <paramref name="len"/> bytes are copied from <paramref name="encodedData"/>,
    /// starting at position specified by <paramref name="offset"/>.
    /// </summary>
    /// <param name="encodedData"> The source byte buffer. </param>
    /// <param name="offset"> The position in buffer to start copying data from. </param>
    /// <param name="len"> The number of bytes to copy from buffer. </param>
    /// <param name="requestedFormat"> The desired color format. </param>
    /// <exception cref="IOException"></exception>
    public Gdx2DPixmap( byte[] encodedData, int offset, int len, int requestedFormat )
    {
        PixmapBuffer = InitializeFromBuffer( encodedData, offset, len );
        
        if ( ( requestedFormat != 0 ) && ( requestedFormat != ColorFormat ) )
        {
            ConvertPixelFormatTo( requestedFormat );
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Gdx2DPixmap"/> class by loading
    /// image data from a stream.
    /// <param>
    /// This constructor reads all data from <paramref name="inStream"/> into an in-memory
    /// buffer, determines the image type, and then loads the pixmap.
    /// If <paramref name="requestedFormat"/> is specified and differs from the image's
    /// original format, the pixmap's pixel format will be converted.
    /// </param>
    /// </summary>
    /// <param name="inStream">
    /// The <see cref="StreamReader"/> containing the raw image data.
    /// </param>
    /// <param name="requestedFormat">
    /// The desired pixel format for the pixmap. Use 0 to keep the original format.
    /// </param>
    public Gdx2DPixmap( StreamReader inStream, int requestedFormat )
    {
        MemoryStream memoryStream = new( 1024 );
        StreamWriter writer       = new( memoryStream );

        int bytesRead;

        while ( ( bytesRead = inStream.Read() ) != -1 )
        {
            writer.Write( bytesRead );
        }

        var buffer = memoryStream.ToArray();

        PixmapBuffer = InitializeFromBuffer( buffer, 0, buffer.Length );

        if ( ( requestedFormat != 0 ) && ( requestedFormat != ColorFormat ) )
        {
            ConvertPixelFormatTo( requestedFormat );
        }
    }

    /// <summary>
    /// Creates a new Gdx2DPixmap instance using data from the supplied buffer.
    /// </summary>
    /// <param name="buffer"></param>
    /// <exception cref="RuntimeException"></exception>
    public Gdx2DPixmap( byte[] buffer )
    {
        PixmapBuffer = InitializeFromBuffer( buffer, 0, buffer.Length );
    }

    /// <summary>
    /// Allocates memory for a new pixmap with the specified width and height, using
    /// the specified color format. This can be used to draw an existing Pixmap into
    /// this newly created EMPTY buffer.
    /// </summary>
    /// <param name="width"> Width in pixels. </param>
    /// <param name="height"> Height in pixels. </param>
    /// <param name="format"> The requested Pixmap.Format color format. </param>
    /// <exception cref="RuntimeException"></exception>
    /// <remarks> After buffer creation, the Pixmap BitDepth is undefined. </remarks>
    public Gdx2DPixmap( int width, int height, int format )
    {
        Width       = width;
        Height      = height;
        ColorFormat = format;
        Blend       = ( uint )Pixmap.BlendType.Default;
        Scale       = ( uint )Pixmap.ScaleType.Default;

        var length = width * height * PixelFormat.BytesPerPixel( format );
        
        Pixels = new byte[ length ];

        PixmapBuffer = new Buffer< byte >( length );
        PixmapBuffer.Put( Pixels );
    }

    /// <summary>
    /// Initializes the internal buffer of the Gdx2DPixmap from the provided byte array.
    /// The buffer data is processed starting from the specified offset and continues
    /// up to the given length.
    /// </summary>
    /// <param name="buffer">The byte array containing the encoded pixmap data.</param>
    /// <param name="offset">The starting position in the byte array from which the data is read.</param>
    /// <param name="len">The length of data to read from the byte array, starting at the offset.</param>
    /// <returns>A byte buffer wrapping the initialized pixmap data.</returns>
    private Buffer< byte > InitializeFromBuffer( byte[] buffer, int offset, int len )
    {
        var bufferHandle  = GCHandle.Alloc( buffer, GCHandleType.Pinned );
        var pixmapPtr     = load( bufferHandle.AddrOfPinnedObject() + offset, len );
        var pixmap        = Marshal.PtrToStructure< NativePixmapStruct >( pixmapPtr );
        var dataBlockSize = ( int )( pixmap.Width * pixmap.Height * bytes_per_pixel( pixmap.ColorFormat ) );
        
        bufferHandle.Free();

        Width         = ( int )pixmap.Width;
        Height        = ( int )pixmap.Height;
        ColorFormat   = ( int )pixmap.ColorFormat;
        BytesPerPixel = ( int )bytes_per_pixel( pixmap.ColorFormat );
        Pixels        = new byte[ dataBlockSize ];
        
        Marshal.Copy( pixmap.Pixels, Pixels, 0, dataBlockSize );

        var byteBuffer = Buffer< byte >.Wrap( Pixels );

        return byteBuffer;

        // --------------------------------------

        [DllImport( "lib/net8.0/gdx2d.dll", EntryPoint = "gdx2d_load" )]
        static extern IntPtr load( IntPtr nativeData, int len );

        [DllImport( "lib/net8.0/gdx2d.dll", EntryPoint = "gdx2d_bytes_per_pixel" )]
        static extern uint bytes_per_pixel( uint format );
    }

    // ========================================================================

    /// <summary>
    /// Converts this Pixmaps <see cref="ColorFormat"/> to the requested format.
    /// </summary>
    /// <param name="requestedFormat"> The new Format. </param>
    public void ConvertPixelFormatTo( int requestedFormat )
    {
        // Double-check conditions
        if ( ( requestedFormat != 0 ) && ( requestedFormat != ColorFormat ) )
        {
            // Create an empty pixmap of the requested format and size
            var pixmap = new Gdx2DPixmap( Width, Height, requestedFormat );

            pixmap.Blend = GDX_2D_BLEND_NONE;
            pixmap.DrawPixmap( this, 0, 0, 0, 0, Width, Height );

            Width        = pixmap.Width;
            Height       = pixmap.Height;
            ColorFormat  = pixmap.ColorFormat;
            BitDepth     = pixmap.BitDepth;
            Blend        = pixmap.Blend;
            Scale        = pixmap.Scale;
            PixmapBuffer = pixmap.PixmapBuffer;

            pixmap.Dispose();
        }
    }

    // ========================================================================

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
    public void DrawPixmap( Gdx2DPixmap src, int srcX, int srcY, int dstX, int dstY, int width, int height )
    {
        DrawPixmapNative( src, srcX, srcY, dstX, dstY, width, height );
    }

    /// <summary>
    /// Returns the pixel from this pixmap at the specified coordinates.
    /// </summary>
    public int GetPixel( int x, int y ) => GetPixelNative( x, y );

    /// <summary>
    /// Sets the pixel at the specified coordinates to the specified color.
    /// </summary>
    public void SetPixel( int x, int y, Color color ) => SetPixelNative( x, y, color );

    /// <summary>
    /// Sets the color of a specific pixel in the pixmap. This method handles color values
    /// that are RGBA8888 encoded in an integer.
    /// </summary>
    /// <param name="x">The x-coordinate of the pixel.</param>
    /// <param name="y">The y-coordinate of the pixel.</param>
    /// <param name="color">The color to set the pixel to, represented as an integer.</param>
    public void SetPixel( int x, int y, int color )
    {
        var rgba = new Color();

        Color.Rgba8888ToColor( ref rgba, ( uint )color );

        SetPixelNative( x, y, rgba );
    }

    // ========================================================================

    /// <summary>
    /// Performs application-defined tasks associated with freeing,
    /// releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Centralise all logic related to releasing unmanaged resources.
    /// </summary>
    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            PixmapBuffer.Dispose();
            PixmapBuffer = null!;
            Pixels       = null!;
        }
    }
    
    // ========================================================================
    // ========================================================================
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="color"></param>
    public void Clear( int color )
    {
        var colorObj = new Color();
        
        Color.Rgba8888ToColor( ref colorObj, ( uint )color );

        ClearWithColor( colorObj );
    }

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
                throw new RuntimeException( "Unknown color type" );
        }

        Array.Copy( Pixels, PixmapBuffer.BackingArray(), Pixels.Length );
    }

    /// <summary>
    /// Clears the pixmap by setting the alpha channel to the specified color's alpha.
    /// </summary>
    /// <param name="color">The color whose alpha channel will be used in the clearing process.</param>
    /// <param name="size">The total size of the pixmap in bytes.</param>
    private void ClearAlpha( Color color, uint size )
    {
        Array.Fill( Pixels, ( byte )( color.A * 255 ), 0, ( Width * Height ) );
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
        var col = Color.Rgb888( color );
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
            throw new RuntimeException( "Invalid size for RGBA8888 format" );
        }

        var col = Color.ToRgba8888( color );
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
    /// <param name="color">The color used to clear the </param>
    /// <param name="size">The total size of the pixmap data to be cleared.</param>
    private void ClearRGB565( Color color, uint size )
    {
        var col = Color.Rgb565( color );

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
        var col = Color.Rgba4444( color );

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

            case LughFormat.INDEXED_COLOR:
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
        gdx2d_set_pixel( x, y, color.PackedColorAbgr() );
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
        gdx2d_draw_line( x, y, x2, y2, color.PackedColorAbgr() );
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
        gdx2d_draw_rect( x, y, width, height, color.PackedColorAbgr() );
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
        gdx2d_draw_circle( x, y, radius, color.PackedColorAbgr() );
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
        gdx2d_fill_rect( x, y, width, height, color.PackedColorAbgr() );
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
        gdx2d_fill_circle( x, y, radius, color.PackedColorAbgr() );
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
        gdx2d_fill_triangle( new NativePixmapStruct(), x1, y1, x2, y2, x3, y3, color.PackedColorAbgr() );
    }

    /// <summary>
    /// Draws an area of the source pixmap onto this 
    /// </summary>
    /// <param name="src"> The source  </param>
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

// ============================================================================
// ============================================================================