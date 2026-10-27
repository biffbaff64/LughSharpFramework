// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

namespace LughSharp.Source.Graphics.Images.TextureData;

/// <summary>
/// This class will load each contained TextureData to the chosen
/// mipmap level. All the mipmap levels must be defined and cannot be null.
/// </summary>
[PublicAPI]
public class MipMapTextureData : TextureDataBase
{
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

    /// <inheritdoc/>
    public override ITextureData.TextureType TextureDataType => ITextureData.TextureType.Custom;

    /// <inheritdoc/>
    public override bool IsManaged => false;

    /// <summary>
    /// Returns whether the caller of <see cref="ITextureData.ConsumePixmap"/> should
    /// dispose the Pixmap returned by <see cref="ITextureData.ConsumePixmap"/>.
    /// </summary>
    public override bool ShouldDisposePixmap()
    {
        return false;
    }

    /// <summary>
    /// Prepares the TextureData for a call to <see cref="ITextureData.ConsumePixmap"/> or
    /// <see cref="ITextureData.ConsumeCustomData"/>. This method can be called from a non
    /// OpenGL thread and should thus not interact with OpenGL.
    /// </summary>
    public override void Prepare()
    {
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
    public override Pixmap ConsumePixmap()
    {
        throw new LughRuntimeException( "This Texture is compressed, use the compress method." );
    }

    /// <summary>
    /// Uploads the pixel data to the OpenGL ES texture. The caller must bind an
    /// OpenGL ES texture. A call to <see cref="ITextureData.Prepare"/> must preceed
    /// a call to this method.
    /// <para>
    /// Any internal data structures created in <see cref="ITextureData.Prepare"/>
    /// should be disposed of here.
    /// </para>
    /// </summary>
    public override void ConsumeCustomData( int target )
    {
        for ( int i = 0; i < _mips.Length; ++i )
        {
            GLTexture.UploadImageData( target, _mips[ i ], i );
        }
    }

    /// <inheritdoc/>
    public override int GetPixelFormat()
    {
        return _mips[ 0 ].GetPixelFormat();
    }
}

// ============================================================================
// ============================================================================
