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

using LughSharp.Lugh.Files;

namespace LughSharp.Lugh.Graphics.Images;

[PublicAPI]
public class Pixmap2D : IDisposable
{
//    public const int GDX_2D_IGNORE = 0;
//
//    // ========================================================================
//
//    public const int GDX_2D_SCALE_NEAREST  = 0;
//    public const int GDX_2D_SCALE_LINEAR   = 1;
//    public const int GDX_2D_SCALE_BILINEAR = 1;
//
//    // ========================================================================
//
//    public const int GDX_2D_BLEND_NONE     = 0;
//    public const int GDX_2D_BLEND_SRC_OVER = 1;
//
//    // ========================================================================
//
//    public int Width       { get; private set; }
//    public int Height      { get; private set; }
//    public int ColorFormat { get; private set; }
//
//    // ========================================================================
//
//    /// <summary>
//    /// Enum to clearly map the contents of the 'nativeData' array, which holds
//    /// metadata from the extern glue code.
//    /// </summary>
//    private enum NativeDataIndex
//    {
//        BasePtr = 0, // The pointer to the extern gdx2d_pixmap struct (gdx2d_pixmap*)
//        Width   = 1,
//        Height  = 2,
//        Format  = 3
//    }
//
//    // ========================================================================
//
//    private IntPtr         _basePtr;
//    private Buffer< byte > _pixelPtr;
//    private long[]         _nativeData = new long[ 4 ];
//
//    // ========================================================================
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="encodedData"></param>
//    /// <param name="offset"></param>
//    /// <param name="len"></param>
//    /// <param name="requestedFormat"></param>
//    /// <exception cref="IOException"></exception>
//    public Pixmap2D( byte[] encodedData, int offset, int len, int requestedFormat )
//    {
//        _pixelPtr = Load( _nativeData, encodedData, offset, len );
//
//        if ( _pixelPtr == null )
//        {
//            throw new IOException( $"Error loading pixmap: {GetFailureReason()}" );
//        }
//
//        checked
//        {
//            this._basePtr    = ( IntPtr )_nativeData[ ( int )NativeDataIndex.BasePtr ];
//            this.Width       = ( int )_nativeData[ ( int )NativeDataIndex.Width ];
//            this.Height      = ( int )_nativeData[ ( int )NativeDataIndex.Height ];
//            this.ColorFormat = ( int )_nativeData[ ( int )NativeDataIndex.Format ];
//        }
//
//        if ( ( requestedFormat != 0 ) && ( requestedFormat != ColorFormat ) )
//        {
//            Convert( requestedFormat );
//        }
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="encodedData"></param>
//    /// <param name="offset"></param>
//    /// <param name="len"></param>
//    /// <param name="requestedFormat"></param>
//    /// <exception cref="IOException"></exception>
//    public Pixmap2D( Buffer< byte > encodedData, int offset, int len, int requestedFormat )
//    {
//        if ( encodedData == null )
//        {
//            throw new IOException( "Couldn't load pixmap from null pointer" );
//        }
//
//        _pixelPtr = LoadByteBuffer( _nativeData, encodedData, offset, len );
//
//        if ( _pixelPtr == null )
//        {
//            throw new IOException( $"Error loading pixmap: {GetFailureReason()}" );
//        }
//
//        checked
//        {
//            this._basePtr    = ( IntPtr )_nativeData[ ( int )NativeDataIndex.BasePtr ];
//            this.Width       = ( int )_nativeData[ ( int )NativeDataIndex.Width ];
//            this.Height      = ( int )_nativeData[ ( int )NativeDataIndex.Height ];
//            this.ColorFormat = ( int )_nativeData[ ( int )NativeDataIndex.Format ];
//        }
//
//        if ( ( requestedFormat != 0 ) && ( requestedFormat != ColorFormat ) )
//        {
//            Convert( requestedFormat );
//        }
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="inStream"></param>
//    /// <param name="requestedFormat"></param>
//    /// <exception cref="IOException"></exception>
//    public Pixmap2D( Stream inStream, int requestedFormat )
//    {
//        // Equivalent to reading all bytes from InputStream in Java
//        using ( var ms = new MemoryStream() )
//        {
//            inStream.CopyTo( ms );
//            var buffer = ms.ToArray();
//
//            _pixelPtr = Load( _nativeData, buffer, 0, buffer.Length );
//
//            if ( _pixelPtr == null )
//            {
//                throw new IOException( $"Error loading pixmap: {GetFailureReason()}" );
//            }
//
//            checked
//            {
//                this._basePtr    = ( IntPtr )_nativeData[ ( int )NativeDataIndex.BasePtr ];
//                this.Width       = ( int )_nativeData[ ( int )NativeDataIndex.Width ];
//                this.Height      = ( int )_nativeData[ ( int )NativeDataIndex.Height ];
//                this.ColorFormat = ( int )_nativeData[ ( int )NativeDataIndex.Format ];
//            }
//
//            if ( ( requestedFormat != 0 ) && ( requestedFormat != ColorFormat ) )
//            {
//                Convert( requestedFormat );
//            }
//        }
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="width"></param>
//    /// <param name="height"></param>
//    /// <param name="format"></param>
//    /// <exception cref="Exception"></exception>
//    public Pixmap2D( int width, int height, int format )
//    {
//        _pixelPtr = NewPixmap( _nativeData, width, height, format );
//
//        if ( _pixelPtr == null )
//        {
//            throw new Exception( $"Unable to allocate memory for pixmap: " +
//                                 $"{width}x{height}, {GetFormatString( format )}" );
//        }
//
//        checked
//        {
//            this._basePtr    = ( IntPtr )_nativeData[ ( int )NativeDataIndex.BasePtr ];
//            this.Width       = ( int )_nativeData[ ( int )NativeDataIndex.Width ];
//            this.Height      = ( int )_nativeData[ ( int )NativeDataIndex.Height ];
//            this.ColorFormat = ( int )_nativeData[ ( int )NativeDataIndex.Format ];
//        }
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="pixelPtr"></param>
//    /// <param name="nativeData"></param>
//    public Pixmap2D( Buffer< byte > pixelPtr, long[] nativeData )
//    {
//        checked
//        {
//            this._pixelPtr   = pixelPtr;
//            this._basePtr    = ( IntPtr )nativeData[ ( int )NativeDataIndex.BasePtr ];
//            this.Width       = ( int )nativeData[ ( int )NativeDataIndex.Width ];
//            this.Height      = ( int )nativeData[ ( int )NativeDataIndex.Height ];
//            this.ColorFormat = ( int )nativeData[ ( int )NativeDataIndex.Format ];
//        }
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="requestedFormat"></param>
//    private void Convert( int requestedFormat )
//    {
//        var pixmap2D = new Pixmap2D( Width, Height, requestedFormat );
//
//        pixmap2D.SetBlend( GDX_2D_BLEND_NONE );
//        pixmap2D.DrawPixmap( this, 0, 0, 0, 0, Width, Height );
//
//        Dispose();
//
//        this._nativeData = pixmap2D._nativeData;
//        this._pixelPtr   = pixmap2D._pixelPtr;
//        this._basePtr    = pixmap2D._basePtr;
//        this.Width       = pixmap2D.Width;
//        this.Height      = pixmap2D.Height;
//        this.ColorFormat = pixmap2D.ColorFormat;
//    }

    // ========================================================================

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="disposing"></param>
    protected void Dispose( bool disposing )
    {
        if ( disposing )
        {
//            if ( _basePtr != IntPtr.Zero )
//            {
//                Free( _basePtr );
//                _basePtr = IntPtr.Zero; // Essential to prevent multiple frees
//            }
        }
    }

//    // ========================================================================
//    // ========================================================================
//
//    public void Clear( int color )
//    {
//        Clear( _basePtr, color );
//    }
//
//    public void SetPixel( int x, int y, int color )
//    {
//        SetPixel( _basePtr, x, y, color );
//    }
//
//    public int GetPixel( int x, int y )
//    {
//        return GetPixel( _basePtr, x, y );
//    }
//
//    public void DrawLine( int x, int y, int x2, int y2, int color )
//    {
//        DrawLine( _basePtr, x, y, x2, y2, color );
//    }
//
//    public void DrawRect( int x, int y, int width, int height, int color )
//    {
//        DrawRect( _basePtr, x, y, width, height, color );
//    }
//
//    public void DrawCircle( int x, int y, int radius, int color )
//    {
//        DrawCircle( _basePtr, x, y, radius, color );
//    }
//
//    public void FillRect( int x, int y, int width, int height, int color )
//    {
//        FillRect( _basePtr, x, y, width, height, color );
//    }
//
//    public void FillCircle( int x, int y, int radius, int color )
//    {
//        FillCircle( _basePtr, x, y, radius, color );
//    }
//
//    public void FillTriangle( int x1, int y1, int x2, int y2, int x3, int y3, int color )
//    {
//        FillTriangle( _basePtr, x1, y1, x2, y2, x3, y3, color );
//    }
//
//    public void DrawPixmap( Pixmap2D src, int srcX, int srcY, int dstX, int dstY, int width, int height )
//    {
//        DrawPixmap( src._basePtr, _basePtr, srcX, srcY, width, height, dstX, dstY, width, height );
//    }
//
//    public void DrawPixmap( Pixmap2D src, int srcX, int srcY, int srcWidth, int srcHeight, int dstX, int dstY,
//                            int dstWidth,
//                            int dstHeight )
//    {
//        DrawPixmap( src._basePtr, _basePtr, srcX, srcY, srcWidth, srcHeight, dstX, dstY, dstWidth, dstHeight );
//    }
//
//    public void SetBlend( int blend )
//    {
//        SetBlend( _basePtr, blend );
//    }
//
//    public void SetScale( int scale )
//    {
//        SetScale( _basePtr, scale );
//    }
//
//    public static Pixmap2D? NewPixmap( InputStream inStream, int requestedFormat )
//    {
//        try
//        {
//            return new Pixmap2D( inStream, requestedFormat );
//        }
//        catch ( IOException e )
//        {
//            return null;
//        }
//    }
//
//    public static Pixmap2D? NewPixmap( int width, int height, int format )
//    {
//        try
//        {
//            return new Pixmap2D( width, height, format );
//        }
//        catch ( ArgumentException e )
//        {
//            return null;
//        }
//    }
//
//    public Buffer< byte > GetPixels()
//    {
//        return _pixelPtr;
//    }
//
//    public int GetGLFormat() => GetGLInternalFormat();
//
//    public int GetGLInternalFormat() => ToGlFormat( ColorFormat );
//
//    public int GetGLType() => ToGlType( ColorFormat );
//
//    public int ToGlFormat( int format ) => 0;
//
//    public int ToGlType( int format ) => 0;
//
//    private static string GetFormatString( int format )
//    {
//        switch ( format )
//        {
////            case GDX2D_FORMAT_ALPHA:
////                return "alpha";
////
////            case GDX2D_FORMAT_LUMINANCE_ALPHA:
////                return "luminance alpha";
////
////            case GDX2D_FORMAT_RGB888:
////                return "rgb888";
////
////            case GDX2D_FORMAT_RGBA8888:
////                return "rgba8888";
////
////            case GDX2D_FORMAT_RGB565:
////                return "rgb565";
////
////            case GDX2D_FORMAT_RGBA4444:
////                return "rgba4444";
//
//            default:
//                return "unknown";
//        }
//    }
//
//    // ========================================================================
//
//    private static extern Buffer< byte > Load( long[] nativeData, byte[] buffer, int offset, int len ); /*MANUAL
//        const unsigned char* p_buffer = (const unsigned char*)env->GetPrimitiveArrayCritical(buffer, 0);
//        gdx2d_pixmap* pixmap = gdx2d_load(p_buffer + offset, len);
//        env->ReleasePrimitiveArrayCritical(buffer, (char*)p_buffer, 0);
//
//        if(pixmap==0)
//            return 0;
//
//        jobject pixel_buffer = env->NewDirectByteBuffer((void*)pixmap->pixels, pixmap->width * pixmap->height * gdx2d_bytes_per_pixel(pixmap->format));
//        jlong* p_native_data = (jlong*)env->GetPrimitiveArrayCritical(nativeData, 0);
//        p_native_data[0] = (jlong)pixmap;
//        p_native_data[1] = pixmap->width;
//        p_native_data[2] = pixmap->height;
//        p_native_data[3] = pixmap->format;
//        env->ReleasePrimitiveArrayCritical(nativeData, p_native_data, 0);
//
//        return pixel_buffer;
//     */
//
//    private static extern Buffer< byte > LoadByteBuffer( long[] nativeData, Buffer< byte > buffer, int offset, int len ); /*MANUAL
//        if(buffer==0)
//            return 0;
//
//        const unsigned char* p_buffer = (const unsigned char*)env->GetDirectBufferAddress(buffer);
//        gdx2d_pixmap* pixmap = gdx2d_load(p_buffer + offset, len);
//
//        if(pixmap==0)
//            return 0;
//
//        jobject pixel_buffer = env->NewDirectByteBuffer((void*)pixmap->pixels, pixmap->width * pixmap->height * gdx2d_bytes_per_pixel(pixmap->format));
//        jlong* p_native_data = (jlong*)env->GetPrimitiveArrayCritical(nativeData, 0);
//        p_native_data[0] = (jlong)pixmap;
//        p_native_data[1] = pixmap->width;
//        p_native_data[2] = pixmap->height;
//        p_native_data[3] = pixmap->format;
//        env->ReleasePrimitiveArrayCritical(nativeData, p_native_data, 0);
//
//        return pixel_buffer;
//     */
//
//    private static extern Buffer< byte > NewPixmap( long[] nativeData, int width, int height, int format ); /*MANUAL
//        gdx2d_pixmap* pixmap = gdx2d_new(width, height, format);
//        if(pixmap==0)
//            return 0;
//
//        jobject pixel_buffer = env->NewDirectByteBuffer((void*)pixmap->pixels, pixmap->width * pixmap->height * gdx2d_bytes_per_pixel(pixmap->format));
//        jlong* p_native_data = (jlong*)env->GetPrimitiveArrayCritical(nativeData, 0);
//        p_native_data[0] = (jlong)pixmap;
//        p_native_data[1] = pixmap->width;
//        p_native_data[2] = pixmap->height;
//        p_native_data[3] = pixmap->format;
//        env->ReleasePrimitiveArrayCritical(nativeData, p_native_data, 0);
//
//        return pixel_buffer;
//     */
//
//    private static extern void Free( long pixmap ); /*
//        gdx2d_free((gdx2d_pixmap*)pixmap);
//     */
//
//    private static extern void Clear( long pixmap, int color ); /*
//        gdx2d_clear((gdx2d_pixmap*)pixmap, color);
//     */
//
//    private static extern void SetPixel( long pixmap, int x, int y, int color ); /*
//        gdx2d_set_pixel((gdx2d_pixmap*)pixmap, x, y, color);
//     */
//
//    private static extern int GetPixel( long pixmap, int x, int y ); /*
//        return gdx2d_get_pixel((gdx2d_pixmap*)pixmap, x, y);
//     */
//
//    private static extern void DrawLine( long pixmap, int x, int y, int x2, int y2, int color ); /*
//        gdx2d_draw_line((gdx2d_pixmap*)pixmap, x, y, x2, y2, color);
//     */
//
//    private static extern void DrawRect( long pixmap, int x, int y, int width, int height, int color ); /*
//        gdx2d_draw_rect((gdx2d_pixmap*)pixmap, x, y, width, height, color);
//     */
//
//    private static extern void DrawCircle( long pixmap, int x, int y, int radius, int color ); /*
//        gdx2d_draw_circle((gdx2d_pixmap*)pixmap, x, y, radius, color);
//     */
//
//    private static extern void FillRect( long pixmap, int x, int y, int width, int height, int color ); /*
//        gdx2d_fill_rect((gdx2d_pixmap*)pixmap, x, y, width, height, color);
//     */
//
//    private static extern void FillCircle( long pixmap, int x, int y, int radius, int color ); /*
//        gdx2d_fill_circle((gdx2d_pixmap*)pixmap, x, y, radius, color);
//     */
//
//    private static extern void FillTriangle( long pixmap, int x1, int y1, int x2, int y2, int x3, int y3, int color ); /*
//        gdx2d_fill_triangle((gdx2d_pixmap*)pixmap, x1, y1, x2, y2, x3, y3, color);
//     */
//
//    private static extern void DrawPixmap( long src, long dst, int srcX, int srcY, int srcWidth, int srcHeight, int dstX,
//                                           int dstY, int dstWidth, int dstHeight ); /*
//                                           gdx2d_draw_pixmap((gdx2d_pixmap*)src, (gdx2d_pixmap*)dst, srcX, srcY, srcWidth, srcHeight, dstX, dstY, dstWidth, dstHeight);
//                                            */
//
//    private static extern void SetBlend( long src, int blend ); /*
//        gdx2d_set_blend((gdx2d_pixmap*)src, blend);
//     */
//
//    private static extern void SetScale( long src, int scale ); /*
//        gdx2d_set_scale((gdx2d_pixmap*)src, scale);
//     */
//
//    public static extern string GetFailureReason(); /*MANUAL
//        const char* reason = gdx2d_get_failure_reason();
//        if(reason==0)
//            return 0;
//        return env->NewStringUTF(reason);
//     */
}

// ============================================================================
// ============================================================================