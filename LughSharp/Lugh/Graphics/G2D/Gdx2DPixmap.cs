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

namespace LughSharp.Lugh.Graphics.G2D;

/// <summary>
/// </summary>
[PublicAPI]
public partial class Gdx2DPixmap : Image, IDisposable
{
    [PublicAPI]
    public enum Gdx2dPixelFormat : int
    {
        GrayScale      = 0,
        TrueColor      = 2,
        IndexedColor   = 3,
        GrayScaleAlpha = 4,
        TrueColorAlpha = 6,
        
        // ----------------------------
        
        Default = TrueColorAlpha,
    };

//    public const byte GDX_2D_FORMAT_ALPHA           = 1;
//    public const byte GDX_2D_FORMAT_LUMINANCE_ALPHA = 2;
//    public const byte GDX_2D_FORMAT_RGB888          = 3;
//    public const byte GDX_2D_FORMAT_RGBA8888        = 4;
//    public const byte GDX_2D_FORMAT_RGB565          = 5;
//    public const byte GDX_2D_FORMAT_RGBA4444        = 6;
//    public const byte GDX_2D_FORMAT_DEFAULT         = 4;

    // ========================================================================

    public const int GDX_2D_SCALE_NEAREST  = 0;
    public const int GDX_2D_SCALE_LINEAR   = 1;
    public const int GDX_2D_SCALE_BILINEAR = 1;

    // ========================================================================

    public const int GDX_2D_BLEND_NONE     = 0;
    public const int GDX_2D_BLEND_SRC_OVER = 1;

    // ========================================================================

    public Buffer< byte > PixmapBuffer  { get; set; }
    public int            ColorType     { get; set; }
    public uint           Blend         { get; set; }
    public uint           Scale         { get; set; }
    public long           TotalIDATSize { get; set; }

    // ========================================================================

    private PixmapDataType _pixmapDataType;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new Gdx2DPixmap instance using data from the supplied buffer.
    /// <paramref name="len" /> bytes are copied from <paramref name="buffer" />, starting
    /// at position specified by <paramref name="offset" />.
    /// </summary>
    /// <param name="buffer"> The source byte buffer. </param>
    /// <param name="offset"> The position in buffer to start copying data from. </param>
    /// <param name="len"> The number of bytes to copy from buffer. </param>
    /// <param name="requestedFormat"> The desired color format. </param>
    /// <exception cref="IOException"></exception>
    public Gdx2DPixmap( byte[] buffer, int offset, int len, int requestedFormat )
    {
        Logger.Checkpoint();

        ( PixmapBuffer, _pixmapDataType ) = LoadPixmapDataType( buffer, offset, len );

        if ( ( requestedFormat != 0 ) && ( requestedFormat != _pixmapDataType.ColorType ) )
        {
            ConvertPixelFormatTo( requestedFormat );
        }

        if ( PixmapBuffer == null )
        {
            throw new GdxRuntimeException( "Failed to create PixmapDef object." );
        }

        ColorType = _pixmapDataType.ColorType;
        Blend     = _pixmapDataType.Blend;
        Scale     = _pixmapDataType.Scale;
        BitDepth  = _pixmapDataType.BitDepth;

        SafeConstructorInit( _pixmapDataType.Width, _pixmapDataType.Height );
    }

    /// <summary>
    /// Provides functionality for managing and manipulating 2D pixel maps with various
    /// color formats, blending modes, and scaling options.
    /// Gdx2DPixmap is designed to handle image processing and manipulation at the
    /// pixel level. It supports multiple color formats, allows drawing operations,
    /// and facilitates scaling and format conversion for efficient rendering in 2D
    /// graphics. The class also allows management of associated buffer data and
    /// provides constructors for different initialization scenarios.
    /// </summary>
    public Gdx2DPixmap( StreamReader inStream, int requestedFormat )
    {
        Logger.Checkpoint();

        MemoryStream memoryStream = new( 1024 );
        StreamWriter writer       = new( memoryStream );

        int bytesRead;

        while ( ( bytesRead = inStream.Read() ) != -1 )
        {
            writer.Write( bytesRead );
        }

        var buffer = memoryStream.ToArray();

        ( PixmapBuffer, _pixmapDataType ) = LoadPixmapDataType( buffer, 0, buffer.Length );

        if ( ( requestedFormat != 0 ) && ( requestedFormat != ColorType ) )
        {
            ConvertPixelFormatTo( requestedFormat );
        }

        if ( PixmapBuffer == null )
        {
            throw new GdxRuntimeException( "Failed to create PixmapDef object." );
        }

        ColorType = _pixmapDataType.ColorType;
        Blend     = _pixmapDataType.Blend;
        Scale     = _pixmapDataType.Scale;
        BitDepth  = _pixmapDataType.BitDepth;

        SafeConstructorInit( _pixmapDataType.Width, _pixmapDataType.Height );
    }

    /// <summary>
    /// Creates a new Gdx2DPixmap object with the given width, height, and pixel format.
    /// </summary>
    /// <param name="width"> Width in pixels. </param>
    /// <param name="height"> Height in pixels. </param>
    /// <param name="format"> The requested GDX_2D_FORMAT_xxx color format. </param>
    /// <exception cref="GdxRuntimeException"></exception>
    public Gdx2DPixmap( int width, int height, int format )
    {
        Logger.Checkpoint();

        ColorType = format;
        Blend     = ( uint )Pixmap.BlendTypes.Default;
        Scale     = ( uint )Pixmap.ScaleType.Default;

        var length = width * height * PixelFormatUtils.Gdx2dBytesPerPixel( format );

        PixmapBuffer = new Buffer< byte >( length );
        PixmapBuffer.PutBytes( PNGDecoder.CreatePNGFromRawRGBA( PixmapBuffer.BackingArray(),
                                                                width,
                                                                height,
                                                                format ) );

        PNGDecoder.AnalysePNG( PixmapBuffer.BackingArray(), true );

        BitDepth = PNGDecoder.BitDepth;

        SafeConstructorInit( width, height );
        SafeInitPixmapDataType( width, height, format );

        Logger.Debug( $"Width: {width}" );
        Logger.Debug( $"Height: {height}" );
        Logger.Debug( $"Format: {PixelFormatUtils.GetFormatString( format )}" );
        Logger.Debug( $"BitDepth: {BitDepth}" );
        Logger.Debug( $"Pixels data length: {_pixmapDataType.Pixels.Length}" );

        PixmapBuffer.Put( _pixmapDataType.Pixels );
    }

    /// <summary>
    /// Initializes the properties of the Gdx2DPixmap with the specified width,
    /// height, and bit depth.
    /// <para>
    /// Used by constructors that need to reference virtual members, which should not be
    /// done directly from constructors.
    /// </para>
    /// </summary>
    /// <param name="width">The width of the pixmap in pixels.</param>
    /// <param name="height">The height of the pixmap in pixels.</param>
    private void SafeConstructorInit( int width, int height )
    {
        Width  = width;
        Height = height;
    }

    /// <summary>
    /// Initializes and configures a new instance of <see cref="PixmapDataType"/> with the specified
    /// dimensions, format, and associated properties for the pixmap.
    /// <para>
    /// Used by constructors that need to reference virtual members, which should not be
    /// done directly from constructors.
    /// </para>
    /// </summary>
    /// <param name="width">The width of the pixmap in pixels.</param>
    /// <param name="height">The height of the pixmap in pixels.</param>
    /// <param name="format">The pixel format of the pixmap, defining the color encoding and storage.</param>
    private void SafeInitPixmapDataType( int width, int height, int format )
    {
        var length = width * height * PixelFormatUtils.Gdx2dBytesPerPixel( format );

        _pixmapDataType = new PixmapDataType
        {
            Width         = Width,
            Height        = Height,
            ColorType     = ColorType,
            BitDepth      = BitDepth,
            Blend         = Blend,
            Scale         = Scale,
            TotalIDATSize = TotalIDATSize,
            Pixels        = new byte[ length ],
        };
    }

    // ========================================================================

    /// <summary>
    /// Loads the data in the supplied byte array into a <see cref="PixmapDataType" />
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    private static ( Buffer< byte >, PixmapDataType ) LoadPixmapDataType( byte[] buffer, int offset, int len )
    {
        // Analyse the PNG file the get the properties. Do this with
        // verbose set to false, as we don't need the full analysis report.
        PNGDecoder.AnalysePNG( buffer, false );

        var pixmapDef = new PixmapDataType
        {
            Width         = ( int )PNGDecoder.IHDRchunk.Width,
            Height        = ( int )PNGDecoder.IHDRchunk.Height,
            BitDepth      = PNGDecoder.IHDRchunk.BitDepth,
            ColorType     = PNGDecoder.IHDRchunk.ColorType,
            Blend         = 0,
            Scale         = 0,
            TotalIDATSize = PNGDecoder.TotalIDATSize,
            Pixels        = new byte[ PNGDecoder.IDATchunk.ChunkSize ],
        };

        Array.Copy( buffer, PNGDecoder.IDAT_DATA_OFFSET,
                    pixmapDef.Pixels, 0, PNGDecoder.IDATchunk.ChunkSize );

        var byteBuffer = new Buffer< byte >( pixmapDef.Pixels.Length );
        byteBuffer.Put( pixmapDef.Pixels );

        return ( byteBuffer, pixmapDef );
    }

    // ========================================================================

    /// <summary>
    /// Converts this Pixmaps <see cref="ColorType" /> to the requested format.
    /// </summary>
    /// <param name="requestedFormat"> The new Format. </param>
    public void ConvertPixelFormatTo( int requestedFormat )
    {
        Logger.Checkpoint();

        // Double-check conditions
        if ( ( requestedFormat != 0 ) && ( requestedFormat != ColorType ) )
        {
            Logger.Debug( $"Requested pixel format {requestedFormat} differs from pixmap format {ColorType}." );
            Logger.Debug( $"Converting pixmap format from {ColorType} to {requestedFormat}." );

            var pixmap = new Gdx2DPixmap( ( int )Width, ( int )Height, requestedFormat );

            pixmap.Blend = GDX_2D_BLEND_NONE;
            pixmap.DrawPixmap( this, 0, 0, 0, 0, ( int )Width, ( int )Height );

            Dispose();

            Width         = pixmap.Width;
            Height        = pixmap.Height;
            ColorType     = pixmap.ColorType;
            BitDepth      = pixmap.BitDepth;
            Blend         = pixmap.Blend;
            Scale         = pixmap.Scale;
            TotalIDATSize = pixmap.TotalIDATSize;
            PixmapBuffer  = pixmap.PixmapBuffer;
        }
    }

    // ========================================================================
    // ========================================================================

    public void DrawPixmap( Gdx2DPixmap src, int srcX, int srcY, int dstX, int dstY, int width, int height )
    {
    }

    public override int GetPixel( int x, int y )
    {
        return 0;
    }

    public override void SetPixel( int x, int y, Color color )
    {
    }

    public override void SetPixel( int x, int y, int color )
    {
    }

    // ========================================================================
    // ========================================================================

//    #region updates june012025
//
//    /// <summary>
//    /// Draws a portion of the source pixmap onto this pixmap at the specified destination location.
//    /// </summary>
//    /// <param name="src">The source pixmap to draw from.</param>
//    /// <param name="srcX">The x-coordinate of the top-left corner of the source region.</param>
//    /// <param name="srcY">The y-coordinate of the top-left corner of the source region.</param>
//    /// <param name="dstX">The x-coordinate of the destination location on this pixmap.</param>
//    /// <param name="dstY">The y-coordinate of the destination location on this pixmap.</param>
//    /// <param name="width">The width of the region to be drawn from the source pixmap.</param>
//    /// <param name="height">The height of the region to be drawn from the source pixmap.</param>
//    public void DrawPixmap( Gdx2DPixmap src, int srcX, int srcY, int dstX, int dstY, int width, int height )
//    {
//        if ( Blend == GDX_2D_BLEND_NONE )
//        {
//            BlitPixmap( src, srcX, srcY, dstX, dstY, width, height );
//        }
//        else
//        {
//            BlendPixmap( src, srcX, srcY, dstX, dstY, width, height );
//        }
//    }
//
//    /// <summary>
//    /// Copies a rectangular region from a source pixmap to the current pixmap at the
//    /// specified coordinates. The dimensions of the region and the source/destination
//    /// positions are defined by the parameters.
//    /// </summary>
//    /// <param name="src">The source pixmap from which the region will be copied.</param>
//    /// <param name="srcX">The x-coordinate of the upper-left corner of the source region.</param>
//    /// <param name="srcY">The y-coordinate of the upper-left corner of the source region.</param>
//    /// <param name="dstX">The x-coordinate where the region will be placed in the destination pixmap.</param>
//    /// <param name="dstY">The y-coordinate where the region will be placed in the destination pixmap.</param>
//    /// <param name="width">The width of the region to copy, in pixels.</param>
//    /// <param name="height">The height of the region to copy, in pixels.</param>
//    private void BlitPixmap( Gdx2DPixmap src, int srcX, int srcY, int dstX, int dstY, int width, int height )
//    {
//        var bytesPerPixel = PixelFormatUtils.Gdx2dBytesPerPixel( ColorType );
//
//        for ( var y = 0; y < height; y++ )
//        {
//            var srcOffset = ( int )( ( ( ( srcY + y ) * src.Width ) + srcX ) * bytesPerPixel );
//            var dstOffset = ( int )( ( ( ( dstY + y ) * Width ) + dstX ) * bytesPerPixel );
//            var rowSize   = width * bytesPerPixel;
//
//            System.Buffer.BlockCopy( src.PixmapBuffer.BackingArray(), srcOffset,
//                                     PixmapBuffer.BackingArray(), dstOffset, rowSize );
//        }
//    }
//
//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="src"></param>
//    /// <param name="srcX"></param>
//    /// <param name="srcY"></param>
//    /// <param name="dstX"></param>
//    /// <param name="dstY"></param>
//    /// <param name="width"></param>
//    /// <param name="height"></param>
//    private void BlendPixmap( Gdx2DPixmap src, int srcX, int srcY, int dstX, int dstY, int width, int height )
//    {
//        for ( var y = 0; y < height; y++ )
//        {
//            for ( var x = 0; x < width; x++ )
//            {
//                var srcColor     = src.GetPixel( srcX + x, srcY + y );
//                var dstColor     = GetPixel( dstX + x, dstY + y );
//                var blendedColor = BlendColors( srcColor, dstColor );
//                SetPixel( dstX + x, dstY + y, blendedColor );
//            }
//        }
//    }
//
//    public override void SetPixel( int x, int y, Color color )
//    {
//        SetPixelNative( x, y, color );
//    }
//
//    public override void SetPixel( int x, int y, int color )
//    {
//        if ( ( x < 0 ) || ( x >= Width ) || ( y < 0 ) || ( y >= Height ) )
//        {
//            return;
//        }
//
//        var bytesPerPixel = PixelFormatUtils.Gdx2dBytesPerPixel( ColorType );
//        var offset        = ( int )( ( ( y * Width ) + x ) * bytesPerPixel );
//
//        WritePixel( offset, color );
//    }
//
//    public override int GetPixel( int x, int y )
//    {
//        return GetPixelNative( x, y );
//    }
//
//    private static int BlendColors( int src, int dst )
//    {
//        var srcA = ( src >> 24 ) & 0xff;
//        var srcR = ( src >> 16 ) & 0xff;
//        var srcG = ( src >> 8 ) & 0xff;
//        var srcB = src & 0xff;
//
//        var dstA = ( dst >> 24 ) & 0xff;
//        var dstR = ( dst >> 16 ) & 0xff;
//        var dstG = ( dst >> 8 ) & 0xff;
//        var dstB = dst & 0xff;
//
//        var outA = srcA + ( ( dstA * ( 255 - srcA ) ) >> 8 );
//        var outR = ( ( srcR * srcA ) + ( dstR * ( 255 - srcA ) ) ) >> 8;
//        var outG = ( ( srcG * srcA ) + ( dstG * ( 255 - srcA ) ) ) >> 8;
//        var outB = ( ( srcB * srcA ) + ( dstB * ( 255 - srcA ) ) ) >> 8;
//
//        return ( outA << 24 ) | ( outR << 16 ) | ( outG << 8 ) | outB;
//    }
//
//    private void WritePixel( int offset, int color )
//    {
//        switch ( ColorType )
//        {
//            case Gdx2DPixmapFormat.Alpha:
//                PixmapBuffer.PutByte( offset, ( byte )( color & 0xff ) );
//
//                break;
//
//            case Gdx2DPixmapFormat.LuminanceAlpha:
//                PixmapBuffer.PutByte( offset, ( byte )( color & 0xff ) );
//                PixmapBuffer.PutByte( offset + 1, ( byte )( ( color >> 8 ) & 0xff ) );
//
//                break;
//
//            case Gdx2DPixmapFormat.RGB888:
//                PixmapBuffer.PutByte( offset, ( byte )( ( color >> 16 ) & 0xff ) );
//                PixmapBuffer.PutByte( offset + 1, ( byte )( ( color >> 8 ) & 0xff ) );
//                PixmapBuffer.PutByte( offset + 2, ( byte )( color & 0xff ) );
//
//                break;
//
//            case Gdx2DPixmapFormat.RGBA8888:
//                PixmapBuffer.PutByte( offset, ( byte )( ( color >> 24 ) & 0xff ) );
//                PixmapBuffer.PutByte( offset + 1, ( byte )( ( color >> 16 ) & 0xff ) );
//                PixmapBuffer.PutByte( offset + 2, ( byte )( ( color >> 8 ) & 0xff ) );
//                PixmapBuffer.PutByte( offset + 3, ( byte )( color & 0xff ) );
//
//                break;
//
//            case Gdx2DPixmapFormat.RGB565:
//                var value = ( ( ( ( color >> 16 ) & 0xff ) >> 3 ) << 11 )
//                            | ( ( ( ( color >> 8 ) & 0xff ) >> 2 ) << 5 )
//                            | ( ( color & 0xff ) >> 3 );
//                PixmapBuffer.PutShort( offset, ( short )value );
//
//                break;
//
//            case Gdx2DPixmapFormat.RGBA4444:
//                value = ( ( ( ( color >> 24 ) & 0xff ) >> 4 ) << 12 )
//                        | ( ( ( ( color >> 16 ) & 0xff ) >> 4 ) << 8 )
//                        | ( ( ( ( color >> 8 ) & 0xff ) >> 4 ) << 4 )
//                        | ( ( color & 0xff ) >> 4 );
//                PixmapBuffer.PutShort( offset, ( short )value );
//
//                break;
//        }
//    }
//
//    private int ReadPixel( int offset )
//    {
//        switch ( ColorType )
//        {
//            case Gdx2DPixmapFormat.Alpha:
//                return PixmapBuffer.GetByte( offset ) & 0xff;
//
//            case Gdx2DPixmapFormat.LuminanceAlpha:
//                return ( ( PixmapBuffer.GetByte( offset ) & 0xff ) << 24 )
//                       | ( ( PixmapBuffer.GetByte( offset + 1 ) & 0xff ) << 16 )
//                       | ( ( PixmapBuffer.GetByte( offset + 1 ) & 0xff ) << 8 )
//                       | ( PixmapBuffer.GetByte( offset + 1 ) & 0xff );
//
//            case Gdx2DPixmapFormat.RGB888:
//                return ( int )( ( ( PixmapBuffer.GetByte( offset ) & 0xff ) << 16 )
//                                | ( ( PixmapBuffer.GetByte( offset + 1 ) & 0xff ) << 8 )
//                                | ( PixmapBuffer.GetByte( offset + 2 ) & 0xff )
//                                | 0xff000000 );
//
//            case Gdx2DPixmapFormat.RGBA8888:
//                return ( ( PixmapBuffer.GetByte( offset ) & 0xff ) << 24 )
//                       | ( ( PixmapBuffer.GetByte( offset + 1 ) & 0xff ) << 16 )
//                       | ( ( PixmapBuffer.GetByte( offset + 2 ) & 0xff ) << 8 )
//                       | ( PixmapBuffer.GetByte( offset + 3 ) & 0xff );
//
//            case Gdx2DPixmapFormat.RGB565:
//                var value = PixmapBuffer.GetShort( offset );
//                var r     = ( ( value & 0xf800 ) >> 11 ) << 3;
//                var g     = ( ( value & 0x07e0 ) >> 5 ) << 2;
//                var b     = ( value & 0x001f ) << 3;
//
//                return ( 0xff << 24 ) | ( r << 16 ) | ( g << 8 ) | b;
//
//            case Gdx2DPixmapFormat.RGBA4444:
//                value = PixmapBuffer.GetShort( offset );
//                var ra = ( ( value & 0xf000 ) >> 12 ) << 28;
//                r = ( ( value & 0x0f00 ) >> 8 ) << 20;
//                g = ( ( value & 0x00f0 ) >> 4 ) << 12;
//                b = ( value & 0x000f ) << 4;
//
//                return ra | r | g | b;
//
//            default:
//                throw new ArgumentException( $"Unknown format: {ColorType}" );
//        }
//    }
//
//    public void ScalePixmap( int srcX, int srcY, int srcWidth, int srcHeight,
//                             int dstX, int dstY, int dstWidth, int dstHeight )
//    {
//        if ( Scale == GDX_2D_SCALE_NEAREST )
//        {
//            ScaleNearest( srcX, srcY, srcWidth, srcHeight, dstX, dstY, dstWidth, dstHeight );
//        }
//        else
//        {
//            ScaleBilinear( srcX, srcY, srcWidth, srcHeight, dstX, dstY, dstWidth, dstHeight );
//        }
//    }
//
//    private void ScaleNearest( int srcX, int srcY, int srcWidth, int srcHeight,
//                               int dstX, int dstY, int dstWidth, int dstHeight )
//    {
//        var xRatio = ( float )srcWidth / dstWidth;
//        var yRatio = ( float )srcHeight / dstHeight;
//
//        for ( var y = 0; y < dstHeight; y++ )
//        {
//            var sy = ( int )( y * yRatio );
//
//            for ( var x = 0; x < dstWidth; x++ )
//            {
//                var sx    = ( int )( x * xRatio );
//                var pixel = GetPixel( srcX + sx, srcY + sy );
//                SetPixel( dstX + x, dstY + y, pixel );
//            }
//        }
//    }
//
//    private void ScaleBilinear( int srcX, int srcY, int srcWidth, int srcHeight,
//                                int dstX, int dstY, int dstWidth, int dstHeight )
//    {
//        var xRatio = ( float )( srcWidth - 1 ) / dstWidth;
//        var yRatio = ( float )( srcHeight - 1 ) / dstHeight;
//
//        for ( var y = 0; y < dstHeight; y++ )
//        {
//            var sy  = y * yRatio;
//            var sy1 = ( int )sy;
//            var sy2 = Math.Min( sy1 + 1, srcHeight - 1 );
//            var yw  = sy - sy1;
//
//            for ( var x = 0; x < dstWidth; x++ )
//            {
//                var sx  = x * xRatio;
//                var sx1 = ( int )sx;
//                var sx2 = Math.Min( sx1 + 1, srcWidth - 1 );
//                var xw  = sx - sx1;
//
//                var p1 = GetPixel( srcX + sx1, srcY + sy1 );
//                var p2 = GetPixel( srcX + sx2, srcY + sy1 );
//                var p3 = GetPixel( srcX + sx1, srcY + sy2 );
//                var p4 = GetPixel( srcX + sx2, srcY + sy2 );
//
//                var pixel = BilinearInterpolate( p1, p2, p3, p4, xw, yw );
//                SetPixel( dstX + x, dstY + y, pixel );
//            }
//        }
//    }
//
//    private static int BilinearInterpolate( int c1, int c2, int c3, int c4, float xw, float yw )
//    {
//        var a = InterpolateChannel( ( c1 >> 24 ) & 0xff, ( c2 >> 24 ) & 0xff,
//                                    ( c3 >> 24 ) & 0xff, ( c4 >> 24 ) & 0xff, xw, yw );
//        var r = InterpolateChannel( ( c1 >> 16 ) & 0xff, ( c2 >> 16 ) & 0xff,
//                                    ( c3 >> 16 ) & 0xff, ( c4 >> 16 ) & 0xff, xw, yw );
//        var g = InterpolateChannel( ( c1 >> 8 ) & 0xff, ( c2 >> 8 ) & 0xff,
//                                    ( c3 >> 8 ) & 0xff, ( c4 >> 8 ) & 0xff, xw, yw );
//        var b = InterpolateChannel( c1 & 0xff, c2 & 0xff, c3 & 0xff, c4 & 0xff, xw, yw );
//
//        return ( a << 24 ) | ( r << 16 ) | ( g << 8 ) | b;
//    }
//
//    private static int InterpolateChannel( int c1, int c2, int c3, int c4, float xw, float yw )
//    {
//        var top    = ( c1 * ( 1 - xw ) ) + ( c2 * xw );
//        var bottom = ( c3 * ( 1 - xw ) ) + ( c4 * xw );
//
//        return ( int )( ( top * ( 1 - yw ) ) + ( bottom * yw ) );
//    }
//
//    #endregion updates june012025

    // ========================================================================
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
        }
    }

    // ========================================================================
    // ========================================================================
}