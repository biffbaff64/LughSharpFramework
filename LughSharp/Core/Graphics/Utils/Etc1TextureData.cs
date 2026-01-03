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

using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Main;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics.Utils;

[PublicAPI]
public class Etc1TextureData : ITextureData
{
    /// <inheritdoc />
    public int Width { get; set; }

    /// <inheritdoc />
    public int Height { get; set; }

    /// <inheritdoc />
    public int BitDepth { get; set; }

    /// <inheritdoc />
    public int BytesPerPixel { get; set; }

    /// <inheritdoc />
    public bool UseMipMaps { get; set; } = false;

    /// <inheritdoc />
    public bool IsPrepared { get; set; }

    /// <inheritdoc />
    public bool IsOwned { get; set; }

    /// <inheritdoc />
    public ITextureData.TextureType TextureDataType => ITextureData.TextureType.Custom;

    /// <inheritdoc />
    bool IManaged.IsManaged => false;

    // ========================================================================

    private readonly FileInfo?      _file;
    private          ETC1.ETC1Data? _data;
    private          ETC1           _etc1;

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <param name="useMipMaps"></param>
    public Etc1TextureData( FileInfo file, bool useMipMaps = false )
    {
        _file      = file;
        _etc1      = new ETC1();
        UseMipMaps = useMipMaps;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="encodedImage"></param>
    /// <param name="useMipMaps"></param>
    public Etc1TextureData( ETC1.ETC1Data encodedImage, bool useMipMaps )
    {
        _data      = encodedImage;
        _etc1      = new ETC1();
        UseMipMaps = useMipMaps;
    }

    /// <summary>
    /// Prepares the TextureData for a call to <see cref="ITextureData.ConsumePixmap"/> or
    /// <see cref="ITextureData.ConsumeCustomData"/>. This method can be called from a non
    /// OpenGL thread and should not interact with OpenGL.
    /// </summary>
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

    /// <summary>
    /// Uploads the pixel data to the OpenGL ES texture. The caller must bind an
    /// OpenGL ES texture. A call to <see cref="ITextureData.Prepare"/> must preceed a call
    /// to this method.
    /// <para>
    /// Any internal data structures created in <see cref="ITextureData.Prepare"/> should be
    /// disposed of here.
    /// </para>
    /// </summary>
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

        if ( !Engine.Api.Graphics.SupportsExtension( "GL_OES_compressed_ETC1_RGB8_texture" ) )
        {
            var pixmap = _etc1.DecodeImage( _data, LughFormat.RGB565 );

            Engine.GL.TexImage2D( target,
                           0,
                           pixmap.GLInternalPixelFormat,
                           pixmap.Width,
                           pixmap.Height,
                           0,
                           pixmap.GLPixelFormat,
                           pixmap.GLDataType,
                           pixmap.PixelData );

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
                Engine.GL.CompressedTexImage2D( target,
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
                Engine.GL.GenerateMipmap( IGL.GL_TEXTURE_2D );
            }
        }

        _data.Dispose();

        _data      = null;
        IsPrepared = false;
    }

    /// <summary>
    /// Returns the <c>Pixmap.Format.XXX</c> of the pixel data.
    /// </summary>
    public int GetPixelFormat()
    {
        return LughFormat.RGB565;
    }

    /// <summary>
    /// Returns the <see cref="Pixmap"/> for upload by Texture.
    /// <para>
    /// A call to <see cref="ITextureData.Prepare"/> must precede a call to this method. Any
    /// internal data structures created in <see cref="ITextureData.Prepare"/> should be
    /// disposed of here.
    /// </para>
    /// </summary>
    /// <returns> the pixmap.</returns>
    public Pixmap ConsumePixmap()
    {
        throw new GdxRuntimeException( "This TextureData implementation does not return a Pixmap" );
    }

    /// <summary>
    /// Returns whether the caller of <see cref="ITextureData.ConsumePixmap"/> should
    /// dispose the Pixmap returned by <see cref="ITextureData.ConsumePixmap"/>.
    /// </summary>
    public bool ShouldDisposePixmap()
    {
        throw new GdxRuntimeException( "This TextureData implementation does not return a Pixmap" );
    }

    /// <summary>
    /// Dumps the internal state of the TextureData to the log.
    /// </summary>
    public void DebugPrint()
    {
    }
}

// ============================================================================
// ============================================================================