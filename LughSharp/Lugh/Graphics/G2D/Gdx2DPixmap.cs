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

using System;
using System.IO;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using LughUtils.source;
using LughUtils.source.Exceptions;

namespace LughSharp.Lugh.Graphics.G2D;

/// <summary>
/// Provides functionality for managing and manipulating 2D pixel maps with various
/// color formats, blending modes, and scaling options.  Gdx2DPixmap is designed to
/// handle image processing and manipulation at the pixel level. It supports multiple
/// color formats, allows drawing operations, and facilitates scaling and format
/// conversion for efficient rendering in 2D graphics. The class also allows management
/// of associated buffer data and provides constructors for different initialization scenarios.
/// </summary>
[PublicAPI]
public partial class Gdx2DPixmap : IDisposable
{
    /// <summary>
    /// Struct holding data for a pixmap which is compatible with the native gdx2d library.
    /// <para>
    /// <b> IMPORTANT: </b> This struct is used for marshalling data between managed and
    /// unmanaged code. It is vital that the layout of this struct is identical to the
    /// equivalent struct in the native library. Therefore, <b>DO NOT modify this struct
    /// without also updating the native struct definition.</b>
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
    /// <exception cref="GdxRuntimeException"></exception>
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
    /// <exception cref="GdxRuntimeException"></exception>
    /// <remarks> After buffer creation, the Pixmap BitDepth is undefined. </remarks>
    public Gdx2DPixmap( int width, int height, int format )
    {
        Width       = width;
        Height      = height;
        ColorFormat = format;
        Blend       = ( uint )Pixmap.BlendTypes.Default;
        Scale       = ( uint )Pixmap.ScaleType.Default;

        var length = width * height * PixelFormat.BytesPerPixel( format );
        
        Pixels = new byte[ length ];

        PixmapBuffer = new Buffer< byte >( length );
        PixmapBuffer.Put( Pixels );
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
            var pixmap = new Gdx2DPixmap( ( int )Width, ( int )Height, requestedFormat );

            pixmap.Blend = GDX_2D_BLEND_NONE;
            pixmap.DrawPixmap( this, 0, 0, 0, 0, ( int )Width, ( int )Height );

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
    public int GetPixel( int x, int y )
    {
        return GetPixelNative( x, y );
    }

    /// <summary>
    /// Sets the pixel at the specified coordinates to the specified color.
    /// </summary>
    public void SetPixel( int x, int y, Color color )
    {
        SetPixelNative( x, y, color );
    }

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

        Color.RGBA8888ToColor( ref rgba, ( uint )color );

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
}

// ============================================================================
// ============================================================================