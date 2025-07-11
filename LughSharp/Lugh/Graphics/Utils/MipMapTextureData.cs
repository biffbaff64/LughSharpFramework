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

using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Utils;

/// <summary>
/// This class will load each contained TextureData to the chosen
/// mipmap level. All the mipmap levels must be defined and cannot be null.
/// </summary>
[PublicAPI]
public class MipMapTextureData : ITextureData
{
    public bool IsPrepared { get; set; }
    public bool UseMipMaps { get; set; }
    public int  Width      { get; set; }
    public int  Height     { get; set; }

    public Gdx2DPixmap.Gdx2DPixmapFormat PixelFormat { get; set; } = Gdx2DPixmap.Gdx2DPixmapFormat.Alpha;

    // ========================================================================

    private readonly ITextureData[] _mips;

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="mipMapData"> must be != null and its length must be >= 1 </param>
    public MipMapTextureData( params ITextureData[] mipMapData )
    {
        _mips = new ITextureData[ mipMapData.Length ];

        Array.Copy( mipMapData, 0, _mips, 0, mipMapData.Length );
    }

    public ITextureData.TextureType TextureDataType => ITextureData.TextureType.Custom;
    public bool                     IsManaged       => false;

    public bool ShouldDisposePixmap()
    {
        return false;
    }

    /// <summary>
    /// Prepares the TextureData for a call to <see cref="ITextureData.ConsumePixmap" /> or
    /// <see cref="ITextureData.ConsumeCustomData" />. This method can be called from a non
    /// OpenGL thread and should thus not interact with OpenGL.
    /// </summary>
    public void Prepare()
    {
    }

    /// <summary>
    /// Returns the <see cref="Pixmap" /> for upload by Texture.
    /// <para>
    /// A call to <see cref="ITextureData.Prepare" /> must precede a call to this method. Any
    /// internal data structures created in <see cref="ITextureData.Prepare" /> should be
    /// disposed of here.
    /// </para>
    /// </summary>
    /// <returns> the pixmap.</returns>
    public Pixmap ConsumePixmap()
    {
        throw new GdxRuntimeException( "This Texture is compressed, use the compress method." );
    }

    /// <summary>
    /// Uploads the pixel data to the OpenGL ES texture. The caller must bind an
    /// OpenGL ES texture. A call to <see cref="ITextureData.Prepare" /> must preceed
    /// a call to this method.
    /// <para>
    /// Any internal data structures created in <see cref="ITextureData.Prepare" />
    /// should be disposed of here.
    /// </para>
    /// </summary>
    public void ConsumeCustomData( int target )
    {
        var derivedGLTexture = new DerivedGLTexture();

        for ( var i = 0; i < _mips.Length; ++i )
        {
            derivedGLTexture.UploadImageData( target, _mips[ i ], i );
        }
    }

    //TODO: This is a hack to get around the fact that GLTexture is not abstract. This should be fixed.
    private class DerivedGLTexture : GLTexture
    {
        /// <inheritdoc />
        public override void Reload()
        {
        }

        /// <inheritdoc />
        public override void ClearWithColor( Color color )
        {
        }

        /// <inheritdoc />
        public override int GetPixel( int x, int y )
        {
            return 0;
        }

        /// <inheritdoc />
        public override void SetPixel( int x, int y, Color color )
        {
        }

        /// <inheritdoc />
        public override void SetPixel( int x, int y, int color )
        {
        }
    }
}