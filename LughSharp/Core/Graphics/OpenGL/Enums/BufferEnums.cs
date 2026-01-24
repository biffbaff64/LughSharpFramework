// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using JetBrains.Annotations;

namespace LughSharp.Core.Graphics.OpenGL.Enums;

[PublicAPI]
public enum BufferTarget : int
{
    ArrayBuffer             = IGL.GL_ARRAY_BUFFER,
    ElementArrayBuffer      = IGL.GL_ELEMENT_ARRAY_BUFFER,
    PixelPackBuffer         = IGL.GL_PIXEL_PACK_BUFFER,
    PixelUnpackBuffer       = IGL.GL_PIXEL_UNPACK_BUFFER,
    TransformFeedbackBuffer = IGL.GL_TRANSFORM_FEEDBACK_BUFFER,
    CopyReadBuffer          = IGL.GL_COPY_READ_BUFFER,
    CopyWriteBuffer         = IGL.GL_COPY_WRITE_BUFFER,
    TextureBuffer           = IGL.GL_TEXTURE_BUFFER,
    UniformBuffer           = IGL.GL_UNIFORM_BUFFER,
}

[PublicAPI]
public enum BufferAccess : int
{
    ReadOnly  = IGL.GL_READ_ONLY,
    ReadWrite = IGL.GL_READ_WRITE,
    WriteOnly = IGL.GL_WRITE_ONLY,
}

[PublicAPI]
public enum BufferUsageHint : int
{
    StaticDraw  = IGL.GL_STATIC_DRAW,
    StaticCopy  = IGL.GL_STATIC_COPY,
    StaticRead  = IGL.GL_STATIC_READ,
    DynamicDraw = IGL.GL_DYNAMIC_DRAW,
    DynamicCopy = IGL.GL_DYNAMIC_COPY,
    DynamicRead = IGL.GL_DYNAMIC_READ,
    StreamDraw  = IGL.GL_STREAM_DRAW,
    StreamCopy  = IGL.GL_STREAM_COPY,
    StreamRead  = IGL.GL_STREAM_READ,
}

[PublicAPI]
public enum BufferBindings : int
{
    ArrayBuffer                    = IGL.GL_ARRAY_BUFFER,
    ArrayBufferBinding             = IGL.GL_ARRAY_BUFFER_BINDING,
    BufferSize                     = IGL.GL_BUFFER_SIZE,
    BufferUsage                    = IGL.GL_BUFFER_USAGE,
    ColorArrayBufferBinding        = IGL.GL_COLOR_ARRAY_BUFFER_BINDING,
    DynamicDraw                    = IGL.GL_DYNAMIC_DRAW,
    ElementArrayBuffer             = IGL.GL_ELEMENT_ARRAY_BUFFER,
    ElementArrayBufferBinding      = IGL.GL_ELEMENT_ARRAY_BUFFER_BINDING,
    FramebufferBinding             = IGL.GL_FRAMEBUFFER_BINDING,
    NormalArrayBufferBinding       = IGL.GL_NORMAL_ARRAY_BUFFER_BINDING,
    StaticDraw                     = IGL.GL_STATIC_DRAW,
    TextureCoordArrayBufferBinding = IGL.GL_TEXTURE_COORD_ARRAY_BUFFER_BINDING,
    VertexArrayBinding             = IGL.GL_VERTEX_ARRAY_BINDING,
    VertexArrayBufferBinding       = IGL.GL_VERTEX_ARRAY_BUFFER_BINDING,
}

// ============================================================================
// ============================================================================
