﻿// ///////////////////////////////////////////////////////////////////////////////
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

using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Buffers;
using LughSharp.Lugh.Utils.Exceptions;

using Exception = System.Exception;

namespace LughSharp.Lugh.Graphics.Images;

/// <summary>
/// A Pixmap represents an image in memory. It has a width and height expressed
/// in pixels as well as a <see cref="PixelType.Format" /> specifying the number and order
/// of color components per pixel.
/// <para>
/// Coordinates of pixels are specified with respect to the top left corner of
/// the image, with the x-axis pointing to the right and the y-axis pointing
/// downwards.
/// </para>
/// <para>
/// By default all methods use blending. You can disable blending by setting it
/// to <see cref="BlendTypes.None" />, which may reduce blitting time by ~30%.
/// </para>
/// <para>
/// The <see cref="DrawPixmap(Pixmap, int, int, int, int, int, int, int, int)" /> method
/// will scale and stretch the source image to a target image. In this case either nearest
/// neighbour or bilinear filtering can be used.
/// </para>
/// </summary>
[PublicAPI]
public class Pixmap : IDisposable
{
    // ========================================================================

    private Filter _filter = Filter.BiLinear;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new Pixmap instance with the given width, height and format.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="format">The <see cref="PixelType.Format" /></param>
    public Pixmap( int width, int height, PixelType.Format? format )
    {
        if ( format == null )
        {
            format = PixelType.Format.Default;

            Logger.Warning( "Supplied Pixel Format is null. Default format applied." );
        }

        Gdx2DPixmap = new Gdx2DPixmap( width, height, PixmapFormat.ToGdx2DPixelFormat( format ) );

        SetColor( Color.Red );
        FillWithCurrentColor();

        Debug();
    }

    /// <summary>
    /// Creates a new Pixmap instance from the given encoded image data. The image can be encoded
    /// as JPEG, PNG or BMP. The size of data used is <b>len</b>, starting from <b>offset</b>.
    /// </summary>
    public Pixmap( byte[] encodedData, int offset, int len )
    {
        ArgumentException.ThrowIfNullOrEmpty( nameof( encodedData ) );

        try
        {
            Gdx2DPixmap = new Gdx2DPixmap( encodedData, offset, len, 0 );
        }
        catch ( IOException e )
        {
            throw new GdxRuntimeException( "Couldn't load pixmap from image data", e );
        }
    }

    /// <summary>
    /// Creates a new Pixmap instance from the given encoded image data. The image can be encoded
    /// as JPEG, PNG or BMP. The size of data used is <b>len</b>, starting from <b>offset</b>.
    /// </summary>
    /// <param name="buffer"> A ByteBuffer holding the encoded data. </param>
    /// <param name="offset"> The position in the data to start copying from. </param>
    /// <param name="len"> The size of data to copy. </param>
    /// <exception cref="GdxRuntimeException"></exception>
    public Pixmap( ByteBuffer buffer, int offset, int len )
    {
        ArgumentNullException.ThrowIfNull( buffer );

        if ( !buffer.IsDirect )
        {
            throw new GdxRuntimeException( "Couldn't load pixmap from non-direct ByteBuffer" );
        }

        try
        {
            Gdx2DPixmap = new Gdx2DPixmap( buffer, offset, len, 0 );
        }
        catch ( IOException e )
        {
            throw new GdxRuntimeException( "Couldn't load pixmap from image data", e );
        }
    }

    /// <summary>
    /// Creates a new Pixmap from the supplied encoded data.
    /// </summary>
    /// <param name="buffer"> A ByteBuffer holding the encoded data. </param>
    public Pixmap( ByteBuffer buffer )
        : this( buffer, buffer.Position, buffer.Remaining() )
    {
    }

    /// <summary>
    /// Creates a new Pixmap instance from the given file. The file must be a Png,
    /// Jpeg or Bitmap. Paletted formats are not supported.
    /// </summary>
    /// <param name="file"> The file. </param>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if there were errors loading or reading the file.
    /// </exception>
    public Pixmap( FileInfo file )
    {
        ArgumentNullException.ThrowIfNull( file );

        try
        {
            var data = File.ReadAllBytes( file.FullName );
            Gdx2DPixmap = new Gdx2DPixmap( data, 0, data.Length, 0 );
        }
        catch ( Exception e )
        {
            throw new GdxRuntimeException( $"Couldn't load file:  {file.FullName}", e );
        }
    }

    /// <summary>
    /// Creates a new Pixmap object from the supplied <see cref="Gdx2DPixmap" />.
    /// </summary>
    public Pixmap( Gdx2DPixmap gdx2DPixmap )
    {
        ArgumentNullException.ThrowIfNull( gdx2DPixmap );

        Gdx2DPixmap = gdx2DPixmap;
    }

    // ========================================================================

    /// <summary>
    /// Returns the OpenGL pixel format of this Pixmap.
    /// </summary>
    /// <returns> one of GL_ALPHA, GL_RGB, GL_RGBA, GL_LUMINANCE, or GL_LUMINANCE_ALPHA.</returns>
    public int GLPixelFormat => PixmapFormat.ToGLPixelFormat( ( int )Gdx2DPixmap.ColorType );

    /// <summary>
    /// Returns the OpenGL ES internal pixel format of this Pixmap.
    /// </summary>
    /// <returns> one of GL_RG, GL_RGB, GL_RGBA, GL_RED, GL_DEPTH_COMPONENT, or GL_DEPTH_STENCIL.</returns>
    public int GLInternalPixelFormat
    {
        get
        {
            var format = PixmapFormat.ToGLPixelFormat( ( int )Gdx2DPixmap.ColorType );

            if ( ( format != IGL.GL_RG )
                 && ( format != IGL.GL_RGB )
                 && ( format != IGL.GL_RGBA )
                 && ( format != IGL.GL_DEPTH_COMPONENT )
                 && ( format != IGL.GL_DEPTH_STENCIL ) )
            {
                throw new GdxRuntimeException( "Unsupported GLInternalFormat" );
            }

            return format;
        }
    }

    /// <summary>
    /// Returns the OpenGL Data Type of this Pixmap.
    /// </summary>
    /// <returns> one of GL_UNSIGNED_BYTE, GL_UNSIGNED_SHORT </returns>
    public int GLDataType
    {
        get
        {
            // Determine OpenGL data type based on bit depth and color type
            return Gdx2DPixmap switch
            {
                { ColorType: 0 } or { ColorType: 4 } =>
                    Gdx2DPixmap.BitDepth switch
                    {
                        8  => IGL.GL_UNSIGNED_BYTE,
                        16 => IGL.GL_UNSIGNED_SHORT,

                        var _ => throw new Exception( "Unsupported bit depth for grayscale." ),
                    },
                { ColorType: 2 } or { ColorType: 6 } =>
                    Gdx2DPixmap.BitDepth switch
                    {
                        8  => IGL.GL_UNSIGNED_BYTE,
                        16 => IGL.GL_UNSIGNED_SHORT,

                        var _ => throw new Exception( "Unsupported bit depth for truecolor." ),
                    },
                { ColorType: 3 } =>
                    Gdx2DPixmap.BitDepth switch
                    {
                        1 or 2 or 4 or 8 => IGL.GL_UNSIGNED_BYTE,

                        var _ => throw new Exception( "Unsupported bit depth for indexed colour." ),
                    },
                var _ => throw new Exception( "Unknown color type." ),
            };
        }
    }

    /// <summary>
    /// Returns the byte[] array holding the pixel data. For the format Alpha each
    /// value is encoded as a byte.
    /// <para>
    /// For the format LuminanceAlpha the luminance is the first byte and the alpha is
    /// the second byte of the pixel.
    /// </para>
    /// <para>
    /// For the formats RGB888 and RGBA8888 the color components are stored in a single
    /// byte each in the order red, green, blue (alpha).
    /// </para>
    /// <para>
    /// For the formats RGB565 and RGBA4444 the pixel colors are stored in shorts in
    /// machine dependent order.
    /// </para>
    /// </summary>
    public ByteBuffer ByteBuffer
    {
        get
        {
            if ( IsDisposed )
            {
                throw new GdxRuntimeException( "Pixmap already disposed" );
            }

            return Gdx2DPixmap.PixmapBuffer ?? throw new GdxRuntimeException( "Pixmap buffer is null" );
        }

        set => Gdx2DPixmap.PixmapBuffer = value;
    }

    /// <summary>
    /// </summary>
    public byte[] PixelData => Gdx2DPixmap.PixmapBuffer.BackingArray();

    /// <summary>
    /// Sets the type of interpolation <see cref="BlendTypes" /> to be used in
    /// conjunction with <see cref="DrawPixmap(Pixmap, int, int, int, int, int, int, int, int)" />.
    /// </summary>
    public Filter FilterValue
    {
        get => _filter;
        set
        {
            _filter = value;

            Scale = _filter == Filter.NearestNeighbour
                ? Gdx2DPixmap.GDX_2D_SCALE_NEAREST
                : Gdx2DPixmap.GDX_2D_SCALE_LINEAR;
        }
    }

    /// <summary>
    /// Downloads an image from http(s) url and passes it as a Pixmap to the
    /// specified <see cref="IDownloadPixmapResponseListener" />.
    /// </summary>
    /// <param name="url">http url to download the image from.</param>
    /// <param name="responseListener"> The listener to call once the image is available as a Pixmap</param>
    /// <remarks> NOT YET IMPLEMENTED. </remarks>
    public static void DownloadFromUrl( string url, IDownloadPixmapResponseListener responseListener )
    {
        //TODO:
        throw new NotImplementedException( "Pixmap#DownloadFromUrl is not currently supported." );
    }

    /// <summary>
    /// Sets the color for drawing operations.
    /// </summary>
    /// <param name="color"> The color, encoded as RGBA8888. </param>
    public void SetColor( Color color )
    {
        Color = color;
    }

    /// <summary>
    /// Sets the color for drawing operations.
    /// </summary>
    /// <param name="r"> The red component. </param>
    /// <param name="g"> The green component. </param>
    /// <param name="b"> The blue component. </param>
    /// <param name="a"> The alpha component.  </param>
    public void SetColor( float r, float g, float b, float a )
    {
        Color = new Color( r, g, b, a );
    }

    /// <summary>
    /// Fills the complete bitmap with the currently set color.
    /// </summary>
    public void FillWithCurrentColor()
    {
        Gdx2DPixmap.Clear( Color );
    }

    /// <summary>
    /// Returns the <see cref="PixelType.Format" /> of this Pixmap.
    /// </summary>
    public PixelType.Format GetColorFormat()
    {
        return PixmapFormat.ToPixmapPixelFormat( ( int )Gdx2DPixmap.ColorType );
    }

    /// <summary>
    /// Gets the number of bits per pixel.
    /// </summary>
    public int GetBitDepth()
    {
        return ( int )Gdx2DPixmap.BitDepth;
    }

    /// <summary>
    /// Draws a line between the given coordinates using the currently set color.
    /// </summary>
    /// <param name="x"> The x-coodinate of the first point </param>
    /// <param name="y"> The y-coordinate of the first point </param>
    /// <param name="x2"> The x-coordinate of the second point </param>
    /// <param name="y2"> The y-coordinate of the second point  </param>
    public void DrawLine( int x, int y, int x2, int y2 )
    {
        Gdx2DPixmap.DrawLine( x, y, x2, y2, Color );
    }

    /// <summary>
    /// Draws a rectangle outline starting at x, y extending by width to the right
    /// and by height downwards (y-axis points downwards) using the current color.
    /// </summary>
    /// <param name="x"> The x coordinate </param>
    /// <param name="y"> The y coordinate </param>
    /// <param name="width"> The width in pixels </param>
    /// <param name="height"> The height in pixels  </param>
    public void DrawRectangle( int x, int y, uint width, uint height )
    {
        Gdx2DPixmap.DrawRect( x, y, width, height, Color );
    }

    /// <summary>
    /// Draws an area from another Pixmap to this Pixmap.
    /// </summary>
    /// <param name="pixmap"> The other Pixmap </param>
    /// <param name="x"> The target x-coordinate (top left corner) </param>
    /// <param name="y"> The target y-coordinate (top left corner)  </param>
    public void DrawPixmap( Pixmap pixmap, int x, int y )
    {
        DrawPixmap( pixmap, x, y, 0, 0, ( int )pixmap.Width, ( int )pixmap.Height );
    }

    /// <summary>
    /// Draws an area from another Pixmap to this Pixmap.
    /// </summary>
    /// <param name="pixmap"> The other Pixmap </param>
    /// <param name="x"> The target x-coordinate (top left corner) </param>
    /// <param name="y"> The target y-coordinate (top left corner) </param>
    /// <param name="srcx"> The source x-coordinate (top left corner) </param>
    /// <param name="srcy"> The source y-coordinate (top left corner); </param>
    /// <param name="srcWidth"> The width of the area from the other Pixmap in pixels </param>
    /// <param name="srcHeight"> The height of the area from the other Pixmap in pixels  </param>
    public void DrawPixmap( Pixmap pixmap, int x, int y, int srcx, int srcy, int srcWidth, int srcHeight )
    {
        ArgumentNullException.ThrowIfNull( nameof( pixmap ), "Source Pixmap cannot be null." );

        try
        {
            Gdx2DPixmap.DrawPixmap( pixmap.Gdx2DPixmap, srcx, srcy, x, y, srcWidth, srcHeight );
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( "Error occurred while drawing the pixmap.", ex );
        }
    }

    /// <summary>
    /// Draws an area from another Pixmap to this Pixmap. This will automatically
    /// scale and stretch the source image to the specified target rectangle.
    /// <para>
    /// Use <see cref="Pixmap.Filter" /> property to specify the type of filtering to
    /// be used (NearestNeighbour or Bilinear).
    /// </para>
    /// </summary>
    /// <param name="pixmap"> The other Pixmap </param>
    /// <param name="srcx"> The source x-coordinate (top left corner) </param>
    /// <param name="srcy"> The source y-coordinate (top left corner); </param>
    /// <param name="srcWidth"> The width of the area from the other Pixmap in pixels </param>
    /// <param name="srcHeight"> The height of the area from the other Pixmap in pixels </param>
    /// <param name="dstx"> The target x-coordinate (top left corner) </param>
    /// <param name="dsty"> The target y-coordinate (top left corner) </param>
    /// <param name="dstWidth"> The target width </param>
    /// <param name="dstHeight"> The target height  </param>
    public void DrawPixmap( Pixmap pixmap, int srcx, int srcy, int srcWidth, int srcHeight, int dstx, int dsty, int dstWidth,
                            int dstHeight )
    {
        ArgumentNullException.ThrowIfNull( nameof( pixmap ), "Source Pixmap cannot be null." );

        try
        {
            Gdx2DPixmap.DrawPixmap( pixmap.Gdx2DPixmap, srcx, srcy, srcWidth, srcHeight, dstx, dsty, dstWidth, dstHeight );
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( "Error occurred while drawing the pixmap.", ex );
        }
    }

    /// <summary>
    /// Fills a rectangle starting at x, y extending by width to the right and by
    /// height downwards (y-axis points downwards) using the current color.
    /// </summary>
    /// <param name="x"> The x coordinate </param>
    /// <param name="y"> The y coordinate </param>
    /// <param name="width"> The width in pixels </param>
    /// <param name="height"> The height in pixels  </param>
    public void FillRectangle( int x, int y, uint width, uint height )
    {
        Gdx2DPixmap.FillRect( x, y, width, height, Color );
    }

    /// <summary>
    /// Draws a circle outline with the center at x,y and a radius using the
    /// current color and stroke width.
    /// </summary>
    /// <param name="x"> The x-coordinate of the center </param>
    /// <param name="y"> The y-coordinate of the center </param>
    /// <param name="radius"> The radius in pixels  </param>
    public void DrawCircle( int x, int y, uint radius )
    {
        Gdx2DPixmap.DrawCircle( x, y, radius, Color );
    }

    /// <summary>
    /// Fills a circle with the center at x,y and a radius using the current color.
    /// </summary>
    /// <param name="x"> The x-coordinate of the center </param>
    /// <param name="y"> The y-coordinate of the center </param>
    /// <param name="radius"> The radius in pixels </param>
    public void FillCircle( int x, int y, uint radius )
    {
        Gdx2DPixmap.FillCircle( x, y, radius, Color );
    }

    /// <summary>
    /// Fills a triangle with vertices at x1,y1 and x2,y2 and x3,y3 using the current color.
    /// </summary>
    /// <param name="x1"> The x-coordinate of vertex 1 </param>
    /// <param name="y1"> The y-coordinate of vertex 1 </param>
    /// <param name="x2"> The x-coordinate of vertex 2 </param>
    /// <param name="y2"> The y-coordinate of vertex 2 </param>
    /// <param name="x3"> The x-coordinate of vertex 3 </param>
    /// <param name="y3"> The y-coordinate of vertex 3  </param>
    public void FillTriangle( int x1, int y1, int x2, int y2, int x3, int y3 )
    {
        Gdx2DPixmap.FillTriangle( x1, y1, x2, y2, x3, y3, Color );
    }

    /// <summary>
    /// Returns the 32-bit RGBA8888 value of the pixel at x, y.
    /// For Alpha formats the RGB components will be one.
    /// </summary>
    /// <param name="x"> The x-coordinate </param>
    /// <param name="y"> The y-coordinate </param>
    /// <returns> The pixel color in RGBA8888 format.  </returns>
    public int GetPixel( int x, int y )
    {
        return Gdx2DPixmap.GetPixel( x, y );
    }

    /// <summary>
    /// Draws a pixel at the given location with the current color.
    /// </summary>
    /// <param name="x"> the x-coordinate </param>
    /// <param name="y"> the y-coordinate </param>
    public void DrawPixel( int x, int y )
    {
        DrawPixel( x, y, Color );
    }

    /// <summary>
    /// Draws a pixel at the given location with the given color.
    /// </summary>
    /// <param name="x"> The x-coordinate </param>
    /// <param name="y"> The y-coordinate </param>
    /// <param name="color"> The color in RGBA8888 format. </param>
    public void DrawPixel( int x, int y, Color color )
    {
        Gdx2DPixmap.SetPixel( x, y, color );
    }

    /// <summary>
    /// Creates a Pixmap from a part of the current framebuffer.
    /// </summary>
    /// <param name="x"> Framebuffer region x </param>
    /// <param name="y"> Framebuffer region y </param>
    /// <param name="width"> Framebuffer region width </param>
    /// <param name="height"> Framebuffer region height </param>
    /// <returns> The new Pixmap. </returns>
    public static unsafe Pixmap CreateFromFrameBuffer( int x, int y, int width, int height )
    {
        GdxApi.Bindings.PixelStorei( IGL.GL_PACK_ALIGNMENT, 1 );

        Pixmap pixmap = new( width, height, PixelType.Format.RGBA8888 );

        fixed ( void* ptr = &pixmap.PixelData[ 0 ] )
        {
            GdxApi.Bindings.ReadPixels( x, y, width, height, IGL.GL_RGBA, IGL.GL_UNSIGNED_BYTE, ( IntPtr )ptr );
        }

        return pixmap;
    }

    /// <summary>
    /// </summary>
    /// <param name="file"></param>
    /// <param name="pixmap"></param>
    /// <exception cref="GdxRuntimeException"></exception>
    public static void SaveToFile( FileInfo file, Pixmap pixmap )
    {
        ArgumentNullException.ThrowIfNull( file );
        ArgumentNullException.ThrowIfNull( pixmap );

        try
        {
            Logger.Debug( $"Saving pixmap to file: {file}" );

            if ( !File.Exists( file.FullName ) )
            {
                File.OpenWrite( file.FullName );
            }

            PixmapIO.WritePNG( file, pixmap );

            Logger.Debug( "Pixmap saved successfully." );
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( "Error occurred while saving the pixmap.", ex );
        }
    }

    /// <summary>
    /// Returns the pixel format from a valid named string.
    /// </summary>
    public static PixelType.Format FormatFromString( string str )
    {
        str = str.ToLower();

        return str switch
        {
            "alpha"          => PixelType.Format.Alpha,
            "intensity"      => PixelType.Format.Intensity,
            "luminancealpha" => PixelType.Format.LuminanceAlpha,
            "rgb565"         => PixelType.Format.RGB565,
            "rgba4444"       => PixelType.Format.RGBA4444,
            "rgb888"         => PixelType.Format.RGB888,
            "rgba8888"       => PixelType.Format.RGBA8888,

            var _ => throw new GdxRuntimeException( $"Unknown Format: {str}" ),
        };
    }

    /// <summary>
    /// </summary>
    public void Debug()
    {
        if ( GdxApi.DevMode )
        {
            Logger.Debug( $"Width : {Width}, Height: {Height}" );
            Logger.Debug( $"Format: {GetColorFormat()}, size : {Width * Height}" +
                          $"{PixmapFormat.ToPixmapPixelFormat( ( int )Gdx2DPixmap.ColorType )}:" +
                          $"{PixmapFormat.GetFormatString( PixmapFormat.ToGdx2DPixelFormat( GetColorFormat() ) )}" );
            Logger.Debug( $"Color : {Color.R}, {Color.G}, {Color.B}, {Color.A}" );

            var a = Gdx2DPixmap.PixmapBuffer.BackingArray();

            for ( var i = 0; i < 100; i += 10 )
            {
                Logger.Debug( $"{a[ i + 0 ]},{a[ i + 1 ]},{a[ i + 2 ]},{a[ i + 3 ]},"
                              + $"{a[ i + 4 ]},{a[ i + 5 ]},{a[ i + 6 ]},{a[ i + 7 ]},"
                              + $"{a[ i + 8 ]},{a[ i + 9 ]},{a[ i + 10 ]},{a[ i + 11 ]},"
                              + $"{a[ i + 12 ]},{a[ i + 13 ]},{a[ i + 14 ]},{a[ i + 15 ]}," );
            }
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Response listener for <see cref="Pixmap.DownloadFromUrl(String, IDownloadPixmapResponseListener)" />
    /// </summary>
    [PublicAPI]
    public interface IDownloadPixmapResponseListener
    {
        /// <summary>
        /// Called on the render thread when image was downloaded successfully.
        /// </summary>
        void DownloadComplete( Pixmap pixmap );

        /// <summary>
        /// Called when image download failed. This might get called on a background thread.
        /// </summary>
        void DownloadFailed( Exception e );
    }

    #region properties

    public bool        IsDisposed  { get; set; } = false;       // 
    public int         Scale       { get; set; } = 1;           // 
    public Color       Color       { get; set; } = Color.Clear; // 
    public Gdx2DPixmap Gdx2DPixmap { get; set; }                // 

    /// <summary>
    /// Returns the width of the Pixmap in pixels.
    /// </summary>
    public int Width => ( int )Gdx2DPixmap.Width;

    /// <summary>
    /// Returns the height of the Pixmap in pixels.
    /// </summary>
    public int Height => ( int )Gdx2DPixmap.Height;

    /// <summary>
    /// Sets the type of <see cref="BlendTypes" /> to be used for all operations.
    /// Default is <see cref="BlendTypes.SourceOver" />.
    /// </summary>
    public BlendTypes Blending { get; set; } = BlendTypes.SourceOver;

    #endregion properties

    // ========================================================================
    // ========================================================================

    #region dispose pattern

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing,
    /// or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose( !IsDisposed );
        GC.SuppressFinalize( this );
    }

    protected void Dispose( bool disposing )
    {
        if ( disposing )
        {
            //TODO:
            Gdx2DPixmap.Dispose();

            Color       = null!;
            Gdx2DPixmap = null!;

            IsDisposed = true;
        }
    }

    #endregion dispose pattern

    // ========================================================================

    #region PixmapEnums

    [PublicAPI]
    public enum ScaleType : int
    {
        Nearest  = 0,
        Linear   = 1,
        Bilinear = Linear,

        Default = Bilinear,
    }

    /// <summary>
    /// Blending functions to be set with <see cref="Pixmap.Blending" />.
    /// </summary>
    [PublicAPI]
    public enum BlendTypes : int
    {
        None       = 0,
        SourceOver = 1,

        Default = SourceOver,
    }

    /// <summary>
    /// Filters to be used with <see cref="DrawPixmap(Pixmap, int, int, int, int, int, int, int, int)" />.
    /// </summary>
    [PublicAPI]
    public enum Filter : int
    {
        NearestNeighbour,
        BiLinear,

        Default = BiLinear,
    }

    #endregion

    // ========================================================================
    // ========================================================================
}

// ========================================================================
// ========================================================================