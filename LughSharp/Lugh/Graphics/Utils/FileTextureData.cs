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

using LughSharp.Lugh.Graphics.G2D;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

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

    /// <returns> the width of the pixel data </returns>
    public int Width { get; set; } = 0;

    /// <returns> the height of the pixel data </returns>
    public int Height { get; set; } = 0;

    /// <returns> whether the TextureData is prepared or not.</returns>
    public bool IsPrepared { get; set; }

    /// <returns> whether to generate mipmaps or not. </returns>
    public bool UseMipMaps { get; set; }

    /// <summary>
    /// Returns the <see cref="Gdx2DPixmap.Gdx2DPixmapFormat" /> of the pixel data.
    /// </summary>
    public Gdx2DPixmap.Gdx2DPixmapFormat PixelFormat { get; set; }

    // ========================================================================

    private Pixmap? _pixmap;

    // ========================================================================
    // ========================================================================

    public FileTextureData( FileInfo file, Pixmap preloadedPixmap, Gdx2DPixmap.Gdx2DPixmapFormat format, bool useMipMaps )
    {
        File        = file;
        _pixmap     = preloadedPixmap;
        PixelFormat = format;
        UseMipMaps  = useMipMaps;
        Width       = _pixmap.Width;
        Height      = _pixmap.Height;

        // If the pixmap format doesn't match our desired format, convert it
        if ( _pixmap.GetColorFormat() != format )
        {
            var newPixmap = new Pixmap( _pixmap.Width, _pixmap.Height, format );
            newPixmap.DrawPixmap( _pixmap, 0, 0 );
            _pixmap.Dispose();
            _pixmap = newPixmap;
        }
    }

    /// <summary>
    /// Prepares the TextureData for a call to <see cref="ITextureData.ConsumePixmap" /> or
    /// <see cref="ITextureData.ConsumeCustomData" />. This method can be called from a non
    /// OpenGL thread and should thus not interact with OpenGL.
    /// </summary>
    public void Prepare()
    {
        if ( IsPrepared )
        {
            Logger.Warning( "prepare() must not be called on a PixmapTextureData" +
                            " instance as it is already prepared." );

            return;
        }

        if ( _pixmap == null )
        {
            _pixmap = File.Extension.Equals( "cim" ) ? PixmapIO.ReadCIM( File ) : new Pixmap( File );

            Width  = _pixmap.Width;
            Height = _pixmap.Height;
        }

        IsPrepared = true;
    }

    /// <summary>
    /// Returns the <see cref="Pixmap" /> for upload by Texture.
    /// <para>
    /// A call to <see cref="ITextureData.Prepare" /> must precede a call to this method.
    /// Any internal data structures created in <see cref="ITextureData.Prepare" />
    /// should be disposed of here.
    /// </para>
    /// </summary>
    /// <returns> the pixmap.</returns>
    public virtual Pixmap? ConsumePixmap()
    {
        if ( !IsPrepared )
        {
            throw new GdxRuntimeException( "Call prepare() before calling ConsumePixmap()" );
        }

        IsPrepared = false;

        var pixmap = _pixmap;
        _pixmap = null;

        if ( pixmap == null )
        {
            throw new GdxRuntimeException( "ConsumePixmap() resulted in a null Pixmap!" );
        }

        return pixmap;
    }

    /// <returns>
    /// whether the caller of <see cref="ITextureData.ConsumePixmap" /> should dispose the
    /// Pixmap returned by <see cref="ITextureData.ConsumePixmap" />
    /// </returns>
    public virtual bool ShouldDisposePixmap()
    {
        return true;
    }

    /// <summary>
    /// Uploads the pixel data to the OpenGL ES texture. The caller must bind an
    /// OpenGL ES texture. A call to <see cref="ITextureData.Prepare" /> must preceed a call
    /// to this method.
    /// <para>
    /// Any internal data structures created in <see cref="ITextureData.Prepare" /> should be
    /// disposed of here.
    /// </para>
    /// </summary>
    public virtual void ConsumeCustomData( int target )
    {
        throw new GdxRuntimeException( "This TextureData implementation does not upload data itself" );
    }

    /// <inheritdoc />
    public virtual bool IsManaged => true;

    /// <returns> the <see cref="ITextureData.TextureDataType" /></returns>
    public ITextureData.TextureType TextureDataType => ITextureData.TextureType.Pixmap;
}