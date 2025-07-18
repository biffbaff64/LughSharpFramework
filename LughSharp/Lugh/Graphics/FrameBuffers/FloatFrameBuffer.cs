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
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Graphics.Utils;
using LughSharp.Lugh.Utils.Exceptions;

using Platform = LughSharp.Lugh.Core.Platform;

namespace LughSharp.Lugh.Graphics.FrameBuffers;

/// <summary>
/// This is a <see cref="FrameBuffer" /> variant backed by a float texture.
/// </summary>
[PublicAPI]
public class FloatFrameBuffer : FrameBuffer
{
    /// <summary>
    /// Default constructor which creates an empty FloatFrameBuffer.
    /// </summary>
    public FloatFrameBuffer()
    {
    }

    /// <summary>
    /// Creates a GLFrameBuffer from the specifications provided by the
    /// given <see cref="GLFrameBufferBuilder{TU}" />.
    /// </summary>
    /// <param name="bufferBuilder"></param>
    public FloatFrameBuffer( GLFrameBufferBuilder< GLFrameBuffer< GLTexture > > bufferBuilder )
        : base( bufferBuilder )
    {
    }

    /// <summary>
    /// Creates a new FrameBuffer with a float backing texture, having the given dimensions
    /// and potentially a depth buffer attached.
    /// </summary>
    /// <param name="width"> the width of the framebuffer in pixels </param>
    /// <param name="height"> the height of the framebuffer in pixels </param>
    /// <param name="hasDepth"> whether to attach a depth buffer </param>
    /// <exception cref="GdxRuntimeException"> in case the FrameBuffer could not be created  </exception>
    public FloatFrameBuffer( int width, int height, bool hasDepth )
    {
        var bufferBuilder = new FrameBufferBuilder( width, height );

        bufferBuilder.AddFloatAttachment( IGL.GL_RGBA32_F, IGL.GL_RGBA, IGL.GL_FLOAT, false );

        if ( hasDepth )
        {
            bufferBuilder.AddBasicDepthRenderBuffer();
        }

        BufferBuilder = bufferBuilder;

        BuildBuffer();
    }

    /// <summary>
    /// Creates the backing <see cref="Texture" /> for this buffer, using the provided
    /// <see cref="FrameBufferTextureAttachmentSpec" /> which contains config
    /// data for said texture.
    /// </summary>
    /// <returns> The texture</returns>
    protected override Texture CreateTexture( FrameBufferTextureAttachmentSpec attachmentSpec )
    {
        var data = new FloatTextureData( BufferBuilder.Width,
                                         BufferBuilder.Height,
                                         attachmentSpec.InternalFormat,
                                         attachmentSpec.Format,
                                         attachmentSpec.Type,
                                         attachmentSpec.IsGpuOnly );

        var result = new Texture( data );

        if ( Api.App.AppType == Platform.ApplicationType.WindowsGL )
        {
            result.SetFilter( TextureFilterMode.Linear, TextureFilterMode.Linear );
        }
        else
        {
            // no filtering for float textures in OpenGL ES
            result.SetFilter( TextureFilterMode.Nearest, TextureFilterMode.Nearest );
        }

        result.SetWrap( TextureWrapMode.ClampToEdge, TextureWrapMode.ClampToEdge );

        return result;
    }
}