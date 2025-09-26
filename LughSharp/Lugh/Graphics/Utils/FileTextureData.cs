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

namespace LughSharp.Lugh.Graphics.Utils;

/// <summary>
/// Represents a texture data source loaded from a file. It implements the
/// <see cref="ITextureData"/> interface, allowing it to be used as a source
/// for texture data in OpenGL-based graphics applications.
/// </summary>
[PublicAPI]
public class FileTextureData : ITextureData
{
    public FileInfo File { get; set; }

    /// <inheritdoc/>
    public int Width { get; set; } = 0;

    /// <inheritdoc/>
    public int Height { get; set; } = 0;

    /// <inheritdoc/>
    public bool IsPrepared { get; set; }

    /// <inheritdoc />
    public bool IsOwned { get; set; }

    /// <inheritdoc/>
    public bool UseMipMaps { get; set; }

    /// <inheritdoc/>
    public int PixelFormat { get; set; }

    // ========================================================================

    private Pixmap? _pixmap;

    // ========================================================================
    // ========================================================================

    public FileTextureData( FileInfo file, Pixmap preloadedPixmap, bool useMipMaps )
        : this( file, preloadedPixmap, preloadedPixmap.GetColorFormat(), useMipMaps )
    {
    }

    public FileTextureData( FileInfo file, Pixmap preloadedPixmap, int desiredFormat, bool useMipMaps )
    {
        Logger.Checkpoint();
        
        File        = file;
        _pixmap     = preloadedPixmap;
        PixelFormat = desiredFormat;
        UseMipMaps  = useMipMaps;
        Width       = _pixmap.Width;
        Height      = _pixmap.Height;

        // If the pixmap format doesn't match our desired format, convert it
        if ( _pixmap.GetColorFormat() != desiredFormat )
        {
            _pixmap.Gdx2DPixmap?.ConvertPixelFormatTo( desiredFormat );
        }

        IsOwned = false;
    }

    /// <inheritdoc />
    public virtual bool IsManaged => true;

    /// <returns> the <see cref="ITextureData.TextureDataType"/></returns>
    public ITextureData.TextureType TextureDataType => ITextureData.TextureType.Pixmap;

    /// <summary>
    /// Prepares the TextureData for a call to <see cref="ITextureData.FetchPixmap"/> or
    /// <see cref="ITextureData.UploadCustomData"/>. This method can be called from a non
    /// OpenGL thread and should thus not interact with OpenGL.
    /// </summary>
    public void Prepare()
    {
        if ( IsPrepared )
        {
            throw new InvalidOperationException( "Texture is already prepared." );
        }

        if ( _pixmap == null )
        {
            var ext   = File.Extension;
            var isCim = ext.Equals( "cim", StringComparison.OrdinalIgnoreCase );

            _pixmap = isCim ? PixmapIO.ReadCIM( File ) : new Pixmap( File );
            IsOwned = true;
        }

        // Resolve basic metadata from the pixmap
        Width  = _pixmap.Width;
        Height = _pixmap.Height;

        if ( ( Width <= 0 ) || ( Height <= 0 ) )
        {
            throw new InvalidOperationException( "Pixmap has invalid dimensions." );
        }

        // Resolve/validate pixel format
        if ( PixelFormat == Gdx2DPixmap.GDX_2D_FORMAT_DEFAULT )
        {
            PixelFormat = _pixmap.GetColorFormat();
        }
        else if ( PixelFormat != _pixmap.GetColorFormat() )
        {
            _pixmap.Gdx2DPixmap?.ConvertPixelFormatTo( PixelFormat );
        }

        IsPrepared = true;
    }

    /// <summary>
    /// Returns the <see cref="Pixmap"/> for upload by Texture.
    /// <para>
    /// A call to <see cref="ITextureData.Prepare"/> must precede a call to this method.
    /// Any internal data structures created in <see cref="ITextureData.Prepare"/>
    /// should be disposed of here.
    /// </para>
    /// </summary>
    /// <returns> the pixmap.</returns>
    public virtual Pixmap FetchPixmap()
    {
        Logger.Checkpoint();
        
        if ( !IsPrepared )
        {
            throw new GdxRuntimeException( "Call prepare() before calling FetchPixmap()" );
        }

        var pixmap = _pixmap;

        if ( pixmap == null )
        {
            throw new GdxRuntimeException( "FetchPixmap() resulted in a null Pixmap!" );
        }

        // Transfer ownership to caller and clear internal reference
        _pixmap    = null;
        IsPrepared = false;

        return pixmap;
    }

    /// <returns>
    /// whether the caller of <see cref="ITextureData.FetchPixmap"/> should dispose the
    /// Pixmap returned by <see cref="ITextureData.FetchPixmap"/>
    /// </returns>
    public virtual bool ShouldDisposePixmap()
    {
        return IsOwned;
    }

    /// <summary>
    /// Uploads the pixel data to the OpenGL ES texture. The caller must bind an
    /// OpenGL ES texture. A call to <see cref="ITextureData.Prepare"/> must preceed a call
    /// to this method.
    /// <para>
    /// Any internal data struct.ures created in <see cref="ITextureData.Prepare"/> should be
    /// disposed of here.
    /// </para>
    /// </summary>
    public virtual void UploadCustomData( int target )
    {
        throw new GdxRuntimeException( "This TextureData implementation does not upload data itself" );
    }

    /// <inheritdoc />
    public void DebugPrint()
    {
        Logger.Debug( $"FileTextureData: {File.Name}" );
        Logger.Debug( $"Size           : {Width} x {Height}" );
        Logger.Debug( $"IsPrepared     : {IsPrepared}" );
        Logger.Debug( $"IsOwned        : {IsOwned}" );
        Logger.Debug( $"UseMipMaps     : {UseMipMaps}" );
        Logger.Debug( $"PixelFormat    : {PixelFormatUtils.GetFormatString( PixelFormat )}" );
    }
}

// ============================================================================
// ============================================================================
