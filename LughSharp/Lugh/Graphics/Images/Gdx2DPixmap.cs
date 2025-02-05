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

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Buffers;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Images;

// ============================================================================
// ============================================================================
// ============================================================================

/// <summary>
/// Simple pixmap struct holding the pixel data, the dimensions and the
/// format of the pixmap.
/// The <see cref="ColorType"/> is one of the GDX_2D_FORMAT_XXX constants.
/// </summary>
[PublicAPI, StructLayout( LayoutKind.Sequential )]
public struct PixmapDataType()
{
    public uint   Width         { get; set; } = 0;
    public uint   Height        { get; set; } = 0;
    public uint   BitDepth      { get; set; } = 0;
    public uint   ColorType     { get; set; } = 0;
    public uint   Blend         { get; set; } = 0;
    public uint   Scale         { get; set; } = 0;
    public byte[] Pixels        { get; set; } = [ ];
    public long   TotalIDATSize { get; set; } = 0;
}

// ========================================================================
// ========================================================================

/// <summary>
/// 
/// </summary>
[PublicAPI]
public partial class Gdx2DPixmap : IDisposable
{
    public const int GDX_2D_FORMAT_ALPHA           = 1;
    public const int GDX_2D_FORMAT_LUMINANCE_ALPHA = 2;
    public const int GDX_2D_FORMAT_RGB888          = 3;
    public const int GDX_2D_FORMAT_RGBA8888        = 4;
    public const int GDX_2D_FORMAT_RGB565          = 5;
    public const int GDX_2D_FORMAT_RGBA4444        = 6;

    public const int GDX_2D_SCALE_NEAREST  = 0;
    public const int GDX_2D_SCALE_LINEAR   = 1;
    public const int GDX_2D_SCALE_BILINEAR = 1;

    public const int GDX_2D_BLEND_NONE     = 0;
    public const int GDX_2D_BLEND_SRC_OVER = 1;

    public const int DEFAULT_FORMAT = GDX_2D_FORMAT_RGBA8888;
    public const int DEFAULT_BLEND  = GDX_2D_BLEND_SRC_OVER;
    public const int DEFAULT_SCALE  = GDX_2D_SCALE_BILINEAR;

    // ========================================================================
    // ========================================================================

    public ByteBuffer PixmapBuffer  { get; set; }
    public uint       Width         { get; set; }
    public uint       Height        { get; set; }
    public uint       ColorType     { get; set; }
    public uint       BitDepth      { get; set; }
    public uint       Blend         { get; set; }
    public uint       Scale         { get; set; }
    public long       TotalIDATSize { get; set; }

    // ========================================================================

    private PixmapDataType _pixmapDataType;

    // ========================================================================
    // ========================================================================

    #region constructors

    /// <summary>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="len"></param>
    /// <param name="requestedFormat"></param>
    public Gdx2DPixmap( ByteBuffer buffer, int offset, int len, int requestedFormat )
        : this( buffer.BackingArray(), offset, len, requestedFormat )
    {
    }

    /// <summary>
    /// Creates a new Gdx2DPixmap instance using data from the supplied buffer.
    /// <paramref name="len"/> bytes are copied from <paramref name="buffer"/>, starting
    /// at position specified by <paramref name="offset"/>.
    /// </summary>
    /// <param name="buffer"> The source byte buffer. </param>
    /// <param name="offset"> The position in buffer to start copying data from. </param>
    /// <param name="len"> The number of bytes to copy from buffer. </param>
    /// <param name="requestedFormat"> The desired color format. </param>
    /// <exception cref="IOException"></exception>
    public Gdx2DPixmap( byte[] buffer, int offset, int len, int requestedFormat )
    {
        PNGUtils.AnalysePNG( buffer, PNGUtils.NO_OUTPUT );

        ( PixmapBuffer, _pixmapDataType ) = LoadPixmapDataType( buffer, offset, len );

        if ( ( requestedFormat != 0 ) && ( requestedFormat != ColorType ) )
        {
            ConvertPixelFormatTo( requestedFormat );
        }

        if ( PixmapBuffer == null )
        {
            throw new GdxRuntimeException( "Failed to create PixmapDef object." );
        }

        this.Width     = _pixmapDataType.Width;
        this.Height    = _pixmapDataType.Height;
        this.ColorType = _pixmapDataType.ColorType;
        this.BitDepth  = _pixmapDataType.BitDepth;
        this.Blend     = _pixmapDataType.Blend;
        this.Scale     = _pixmapDataType.Scale;
    }

    /// <summary>
    /// </summary>
    /// <param name="inStream"></param>
    /// <param name="requestedFormat"></param>
    /// <exception cref="IOException"></exception>
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

        ( PixmapBuffer, _pixmapDataType ) = LoadPixmapDataType( buffer, 0, buffer.Length );

        if ( ( requestedFormat != 0 ) && ( requestedFormat != ColorType ) )
        {
            ConvertPixelFormatTo( requestedFormat );
        }

        if ( PixmapBuffer == null )
        {
            throw new GdxRuntimeException( "Failed to create PixmapDef object." );
        }

        this.Width     = _pixmapDataType.Width;
        this.Height    = _pixmapDataType.Height;
        this.ColorType = _pixmapDataType.ColorType;
        this.BitDepth  = _pixmapDataType.BitDepth;
        this.Blend     = _pixmapDataType.Blend;
        this.Scale     = _pixmapDataType.Scale;
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
        this.Width     = ( uint )width;
        this.Height    = ( uint )height;
        this.ColorType = ( uint )format;
        this.BitDepth  = 0; //TODO:
        this.Blend     = ( uint )Pixmap.BlendTypes.Default;
        this.Scale     = ( uint )Pixmap.ScaleType.Default;

        var length = width * height * PixmapFormat.Gdx2dBytesPerPixel( format );

        _pixmapDataType = new PixmapDataType
        {
            Width         = this.Width,
            Height        = this.Height,
            ColorType     = this.ColorType,
            BitDepth      = this.BitDepth,
            Blend         = this.Blend,
            Scale         = this.Scale,
            TotalIDATSize = this.TotalIDATSize,
            Pixels        = new byte[ length ],
        };

        PixmapBuffer = new HeapByteBuffer( _pixmapDataType.Pixels, 0, length );

        if ( PixmapBuffer == null )
        {
            throw new GdxRuntimeException( $"Unable to allocate memory for pixmap: "
                                           + $"{width} x {height}: {PixmapFormat.GetFormatString( format )}" );
        }
    }

    #endregion constructors

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Loads the data in the supplied byte array into a <see cref="PixmapDataType"/>
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="len"></param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    private static ( ByteBuffer, PixmapDataType ) LoadPixmapDataType( byte[] buffer, int offset, int len )
    {
        PNGUtils.AnalysePNG( buffer, PNGUtils.NO_OUTPUT );
        
        var pixmapDef = new PixmapDataType
        {
            Width         = ( uint )PNGUtils.IHDRchunk.Width,
            Height        = ( uint )PNGUtils.IHDRchunk.Height,
            BitDepth      = ( uint )PNGUtils.IHDRchunk.BitDepth,
            ColorType     = ( uint )PNGUtils.IHDRchunk.ColorType,
            Blend         = 0,
            Scale         = 0,
            TotalIDATSize = PNGUtils.TotalIDATSize,
            Pixels        = new byte[ PNGUtils.IDATchunk.ChunkSize ],
        };

        Array.Copy( buffer, PNGUtils.IDAT_DATA_OFFSET, pixmapDef.Pixels, 0, PNGUtils.IDATchunk.ChunkSize );

        return ( new HeapByteBuffer( pixmapDef.Pixels, 0, pixmapDef.Pixels.Length ), pixmapDef );
    }

    /// <summary>
    /// Converts this Pixmaps <see cref="ColorType"/> to the requested format.
    /// </summary>
    /// <param name="requestedFormat"> The new Format. </param>
    private void ConvertPixelFormatTo( int requestedFormat )
    {
        // Double-check conditions
        if ( ( requestedFormat != 0 ) && ( requestedFormat != ColorType ) )
        {
            var pixmap = new Gdx2DPixmap( ( int )Width, ( int )Height, requestedFormat );

            pixmap.SetBlend( GDX_2D_BLEND_NONE );
            pixmap.DrawPixmap( this, 0, 0, 0, 0, ( int )Width, ( int )Height );

            Dispose();

            this.Width         = pixmap.Width;
            this.Height        = pixmap.Height;
            this.ColorType     = pixmap.ColorType;
            this.BitDepth      = pixmap.BitDepth;
            this.Blend         = pixmap.Blend;
            this.Scale         = pixmap.Scale;
            this.TotalIDATSize = pixmap.TotalIDATSize;
            this.PixmapBuffer  = pixmap.PixmapBuffer;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Sets this pixmaps blending value.
    /// </summary>
    /// <param name="blend"></param>
    public void SetBlend( int blend ) => this.Blend = ( uint )blend;

    /// <summary>
    /// Sets this pixmaps scaling value.
    /// </summary>
    /// <param name="scale"></param>
    public void SetScale( int scale ) => this.Scale = ( uint )scale;

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