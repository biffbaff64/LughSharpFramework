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

using JetBrains.Annotations;

using LughSharp.Core.Graphics.FrameBuffers;
using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Main;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics.Utils;

/// <summary>
/// A <see cref="ITextureData"/> implementation which should be used to create
/// GL only textures.
/// This TextureData fits perfectly for <see cref="FrameBuffer"/>s.
/// The data is not managed.
/// </summary>
[PublicAPI]
public class GLOnlyTextureData : ITextureData
{
    public int  InternalFormat { get; set; }
    public int  MipLevel       { get; set; }
    public int  Type           { get; set; }
    public int  Width          { get; set; }
    public int  Height         { get; set; }
    public int  BitDepth       { get; set; }
    public int  BytesPerPixel  { get; set; }
    public bool IsPrepared     { get; set; }
    public bool UseMipMaps     { get; set; }

    /// <inheritdoc />
    public bool IsOwned { get; set; }

    /// <summary>
    /// GLOnlyTextureData objects are not Managed.
    /// </summary>
    public bool IsManaged => false;

    // ========================================================================

    private int _pixelFormat;

    // ========================================================================

    /// <summary>
    /// See <a href="https://www.khronos.org/opengles/sdk/docs/man/xhtml/glTexImage2D.xml">glTexImage2D</a>
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="mipMapLevel"></param>
    /// <param name="internalFormat">
    /// Specifies the internal format of the texture. Must be one of the following symbolic constants:
    /// <see cref="IGL.GL_ALPHA"/>, <see cref="IGL.GL_LUMINANCE"/>, <see cref="IGL.GL_LUMINANCE_ALPHA"/>,
    /// <see cref="IGL.GL_RGB"/>, <see cref="IGL.GL_RGBA"/>.
    /// </param>
    /// <param name="format">
    /// Specifies the format of the texel data. Must match internalFormat.
    /// The following symbolic values are accepted:
    /// <see cref="IGL.GL_ALPHA"/>, <see cref="IGL.GL_RGB"/>, <see cref="IGL.GL_RGBA"/>,
    /// <see cref="IGL.GL_LUMINANCE"/>, and <see cref="IGL.GL_LUMINANCE_ALPHA"/>.
    /// </param>
    /// <param name="type">
    /// Specifies the data type of the texel data. The following symbolic values are accepted:
    /// <see cref="IGL.GL_UNSIGNED_BYTE"/>, <see cref="IGL.GL_UNSIGNED_SHORT_5_6_5"/>,
    /// <see cref="IGL.GL_UNSIGNED_SHORT_4_4_4_4"/>, and <see cref="IGL.GL_UNSIGNED_SHORT_5_5_5_1"/>.
    /// </param>
    public GLOnlyTextureData( int width,
                              int height,
                              int mipMapLevel,
                              int internalFormat,
                              int format,
                              int type )
    {
        Width          = width;
        Height         = height;
        MipLevel       = mipMapLevel;
        InternalFormat = internalFormat;
        _pixelFormat   = format;
        Type           = type;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="RuntimeException"></exception>
    public void Prepare()
    {
        if ( IsPrepared )
        {
            throw new RuntimeException( "Already prepared" );
        }

        IsPrepared = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    public void ConsumeCustomData( int target )
    {
        Engine.GL.TexImage2D( target,
                              MipLevel,
                              InternalFormat,
                              Width,
                              Height,
                              0,
                              _pixelFormat,
                              Type,
                              IntPtr.Zero );
    }

    /// <inheritdoc />
    public void DebugPrint()
    {
        // TODO: Implement DebugPrint
    }

    /// <summary>
    /// Returns the <see cref="ITextureData.TextureType"/> for this Texture Data.
    /// </summary>
    public ITextureData.TextureType TextureDataType => ITextureData.TextureType.Custom;

    public int GetPixelFormat()
    {
        return LughFormat.RGBA8888;
    }

    public Pixmap ConsumePixmap()
    {
        throw new RuntimeException( "This TextureData implementation does not return a Pixmap" );
    }

    public bool ShouldDisposePixmap()
    {
        throw new RuntimeException( "This TextureData implementation does not return a Pixmap" );
    }
}

// ========================================================================
// ========================================================================