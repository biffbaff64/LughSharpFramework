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

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public class Etc1TextureData : ITextureData
{
    // ========================================================================

    private readonly FileInfo?      _file;
    private          ETC1.ETC1Data? _data;
    private          ETC1           _etc1;

    // ========================================================================

    public Etc1TextureData( FileInfo file, bool useMipMaps = false )
    {
        _file      = file;
        _etc1      = new ETC1();
        UseMipMaps = useMipMaps;
    }

    public Etc1TextureData( ETC1.ETC1Data encodedImage, bool useMipMaps )
    {
        _data      = encodedImage;
        _etc1      = new ETC1();
        UseMipMaps = useMipMaps;
    }

    /// <inheritdoc />
    public int Width { get; set; }

    /// <inheritdoc />
    public int Height { get; set; }

    /// <inheritdoc />
    public bool UseMipMaps { get; set; } = false;

    /// <inheritdoc />
    public bool IsPrepared { get; set; }

    /// <inheritdoc />
    public bool IsOwned { get; set; }

    /// <inheritdoc />
    public int PixelFormat { get; set; } = Gdx2DPixmap.GDX_2D_FORMAT_ALPHA;

    /// <inheritdoc />
    public ITextureData.TextureType TextureDataType => ITextureData.TextureType.Custom;

    /// <inheritdoc />
    bool IManaged.IsManaged => false;

    /// <inheritdoc />
    public void Prepare()
    {
        if ( IsPrepared )
        {
            throw new GdxRuntimeException( "Already prepared" );
        }

        if ( ( _file == null ) && ( _data == null ) )
        {
            throw new GdxRuntimeException( "Can only load once from ETC1Data" );
        }

        if ( _file != null )
        {
            _data = new ETC1.ETC1Data( _file, _etc1 );
        }

        if ( _data == null )
        {
            throw new GdxRuntimeException( "No data to prepare!" );
        }

        Width      = _data.Width;
        Height     = _data.Height;
        IsPrepared = true;
    }

    /// <inheritdoc />
    public Pixmap ConsumePixmap()
    {
        throw new GdxRuntimeException( "This TextureData implementation does not return a Pixmap" );
    }

    /// <inheritdoc />
    public bool ShouldDisposePixmap()
    {
        throw new GdxRuntimeException( "This TextureData implementation does not return a Pixmap" );
    }

    /// <inheritdoc />
    public unsafe void ConsumeCustomData( int target )
    {
        if ( !IsPrepared )
        {
            throw new GdxRuntimeException( "Call prepare() before calling consumeCompressedData()" );
        }

        if ( _data is null )
        {
            throw new GdxRuntimeException( "No data to consume!" );
        }

        if ( !Api.Graphics.SupportsExtension( "GL_OES_compressed_ETC1_RGB8_texture" ) )
        {
            var pixmap = _etc1.DecodeImage( _data, Gdx2DPixmap.GDX_2D_FORMAT_RGB565 );

            fixed ( void* ptr = &pixmap.PixelData[ 0 ] )
            {
                GL.TexImage2D( target,
                               0,
                               pixmap.GLInternalPixelFormat,
                               pixmap.Width,
                               pixmap.Height,
                               0,
                               pixmap.GLPixelFormat,
                               pixmap.GLDataType,
                               ( IntPtr )ptr );
            }

            if ( UseMipMaps )
            {
                MipMapGenerator.GenerateMipMap( target, pixmap, pixmap.Width, pixmap.Height );
            }

            pixmap.Dispose();
            UseMipMaps = false;
        }
        else
        {
            fixed ( void* ptr = &_data.CompressedData.BackingArray()[ 0 ] )
            {
                GL.CompressedTexImage2D( target,
                                         0,
                                         ETC1.ETC1_RGB8_OES,
                                         Width,
                                         Height,
                                         0,
                                         _data.CompressedData.Capacity - _data.DataOffset,
                                         ( IntPtr )ptr );
            }

            if ( UseMipMaps )
            {
                GL.GenerateMipmap( IGL.GL_TEXTURE_2D );
            }
        }

        _data.Dispose();

        _data      = null;
        IsPrepared = false;
    }

    /// <inheritdoc />
    public void DebugPrint()
    {
    }
}

// ============================================================================
// ============================================================================
