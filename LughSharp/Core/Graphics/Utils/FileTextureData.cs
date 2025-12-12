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

using JetBrains.Annotations; namespace LughSharp.Core.Graphics.Utils;

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

    /// <inheritdoc />
    public int BitDepth { get; set; }

    /// <inheritdoc />
    public int BytesPerPixel { get; set; }

    /// <inheritdoc/>
    public bool IsPrepared { get; set; }

    /// <inheritdoc />
    public bool IsOwned { get; set; }

    /// <inheritdoc/>
    public bool UseMipMaps { get; set; }

    // ========================================================================

    private int     _pixelFormat;
    private Pixmap? _pixmap;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <param name="preloadedPixmap"></param>
    /// <param name="format"></param>
    /// <param name="useMipMaps"></param>
    public FileTextureData( FileInfo file, Pixmap preloadedPixmap, int format, bool useMipMaps )
    {
        Guard.Against.Null( preloadedPixmap );

        File         = file;
        _pixmap      = preloadedPixmap;
        _pixelFormat = format;
        UseMipMaps   = useMipMaps;

        if ( _pixmap != null )
        {
            Width  = _pixmap.Width;
            Height = _pixmap.Height;

            // If _pixelFormat is not set, use the pixmap's format.
            if ( _pixelFormat <= 0 )
            {
                _pixelFormat = _pixmap.GetColorFormat();
            }

            BytesPerPixel = PixelFormat.BytesPerPixel( _pixmap.GetColorFormat() );
        }

        IsOwned = false;
    }

    /// <inheritdoc />
    public virtual bool IsManaged => true;

    /// <returns> the <see cref="ITextureData.TextureDataType"/></returns>
    public ITextureData.TextureType TextureDataType => ITextureData.TextureType.Pixmap;

    /// <summary>
    /// Prepares the TextureData for a call to <see cref="ITextureData.ConsumePixmap"/> or
    /// <see cref="ITextureData.ConsumeCustomData"/>. This method can be called from a non
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

        _pixelFormat = _pixmap.GetColorFormat();

        IsPrepared = true;
    }

    /// <summary>
    /// Returns the <c>Pixmap.Format.XXX</c> of the pixel data.
    /// </summary>
    public int GetPixelFormat()
    {
        return _pixmap?.GetColorFormat() ?? LughFormat.RGBA8888;
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
    public virtual Pixmap ConsumePixmap()
    {
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
    /// whether the caller of <see cref="ITextureData.ConsumePixmap"/> should dispose the
    /// Pixmap returned by <see cref="ITextureData.ConsumePixmap"/>
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
    public virtual void ConsumeCustomData( int target )
    {
        throw new GdxRuntimeException( "This TextureData implementation does not upload data itself" );
    }

    /// <summary>
    /// Dumps the internal state of the TextureData to the log.
    /// </summary>
    public void DebugPrint()
    {
        Logger.Debug( $"FileTextureData: {File.Name}" );
        Logger.Debug( $"Size           : {Width} x {Height}" );
        Logger.Debug( $"IsPrepared     : {IsPrepared}" );
        Logger.Debug( $"IsOwned        : {IsOwned}" );
        Logger.Debug( $"UseMipMaps     : {UseMipMaps}" );
        Logger.Debug( $"PixelFormat    : {_pixelFormat}" );
    }
}

// ============================================================================
// ============================================================================