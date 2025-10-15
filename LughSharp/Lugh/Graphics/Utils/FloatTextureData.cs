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

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

using Platform = LughSharp.Lugh.Core.Platform;

namespace LughSharp.Lugh.Graphics.Utils;

/// <summary>
/// A <see cref="ITextureData"/> implementation which should be used
/// to create float textures.
/// </summary>
[PublicAPI]
public class FloatTextureData : ITextureData
{
    public Buffer< float > Buffer        { get; private set; } = null!;
    public int             Width         { get; set; }         = 0;
    public int             Height        { get; set; }         = 0;
    public int             BitDepth      { get; set; }
    public int             BytesPerPixel { get; set; }
    public bool            IsPrepared    { get; set; } = false;
    public bool            UseMipMaps    { get; set; }

    /// <inheritdoc />
    public bool IsOwned { get; set; }

    /// <summary>
    /// FloatTextureData objects are Managed.
    /// </summary>
    public bool IsManaged => true;

    // ========================================================================

    private readonly int  _format;
    private readonly int  _internalFormat;
    private readonly bool _isGpuOnly;
    private readonly int  _type;

    // ========================================================================

    public FloatTextureData( int w, int h, int internalFormat, int format, int type, bool isGpuOnly )
    {
        Width           = w;
        Height          = h;
        _internalFormat = internalFormat;
        _format         = format;
        _type           = type;
        _isGpuOnly      = isGpuOnly;
    }

    public void Prepare()
    {
        if ( IsPrepared )
        {
            throw new GdxRuntimeException( "Already prepared" );
        }

        if ( !_isGpuOnly )
        {
            var amountOfFloats = 4;

            if ( Api.Graphics.GLVersion!.BackendType.Equals( GraphicsBackend.BackendType.OpenGL ) )
            {
                if ( _internalFormat is IGL.GL_RGBA16_F or IGL.GL_RGBA32_F )
                {
                    amountOfFloats = 4;
                }

                if ( _internalFormat is IGL.GL_RGB16_F or IGL.GL_RGB32_F )
                {
                    amountOfFloats = 3;
                }

                if ( _internalFormat is IGL.GL_RG16_F or IGL.GL_RG32_F )
                {
                    amountOfFloats = 2;
                }

                if ( _internalFormat is IGL.GL_R16_F or IGL.GL_R32_F )
                {
                    amountOfFloats = 1;
                }
            }

            Buffer = new Buffer< float >( Width * Height * amountOfFloats );
        }

        IsPrepared = true;
    }

    public void UploadCustomData( int target )
    {
        if ( ( Api.App.AppType == Platform.ApplicationType.Android )
             || ( Api.App.AppType == Platform.ApplicationType.IOS )
             || ( Api.App.AppType == Platform.ApplicationType.WebGL ) )
        {
            if ( !Api.Graphics.SupportsExtension( "OES_texture_float" ) )
            {
                throw new GdxRuntimeException( "Extension OES_texture_float not supported!" );
            }

            // GLES and WebGL defines texture format by 3rd and 8th argument,
            // so to get a float texture one needs to supply GL_RGBA and GL_FLOAT there.
            unsafe
            {
                fixed ( void* ptr = &Buffer.ToArray()[ 0 ] )
                {
                    GL.TexImage2D( target,
                                   0,
                                   IGL.GL_RGBA,
                                   Width,
                                   Height,
                                   0,
                                   IGL.GL_RGBA,
                                   IGL.GL_FLOAT,
                                   ( IntPtr )ptr );
                }
            }
        }
        else
        {
            if ( !Api.Graphics.SupportsExtension( "GL_ARB_texture_float" ) )
            {
                throw new GdxRuntimeException( "Extension GL_ARB_texture_float not supported!" );
            }

            // in desktop OpenGL the texture format is defined only by the third argument,
            // hence we need to use GL_RGBA32F there (this constant is unavailable in GLES/WebGL)
            unsafe
            {
                fixed ( void* ptr = &Buffer.ToArray()[ 0 ] )
                {
                    GL.TexImage2D( target,
                                   0,
                                   _internalFormat,
                                   Width,
                                   Height,
                                   0,
                                   _format,
                                   IGL.GL_FLOAT,
                                   ( IntPtr )ptr );
                }
            }
        }
    }

    /// <inheritdoc />
    public void DebugPrint()
    {
    }

    public ITextureData.TextureType TextureDataType => ITextureData.TextureType.Custom;

    /// <inheritdoc />
    public Pixmap.Format GetPixelFormat()
    {
        return Pixmap.Format.RGBA8888;
    }

    // ========================================================================
    // ========================================================================

    public Pixmap.Format PixelFormat
    {
        get => throw new GdxRuntimeException( "This TextureData implementation does not return a Pixmap" );
        set => throw new GdxRuntimeException( "This TextureData implementation does not return a Pixmap" );
    }

    public Pixmap FetchPixmap()
    {
        throw new GdxRuntimeException( "This TextureData implementation does not return a Pixmap" );
    }

    public bool ShouldDisposePixmap()
    {
        throw new GdxRuntimeException( "This TextureData implementation does not return a Pixmap" );
    }
}

// ============================================================================
// ============================================================================