// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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
public enum BufferTarget
{
    ArrayBuffer             = IGL.GLArrayBuffer,
    ElementArrayBuffer      = IGL.GLElementArrayBuffer,
    PixelPackBuffer         = IGL.GLPixelPackBuffer,
    PixelUnpackBuffer       = IGL.GLPixelUnpackBuffer,
    TransformFeedbackBuffer = IGL.GLTransformFeedbackBuffer,
    CopyReadBuffer          = IGL.GLCopyReadBuffer,
    CopyWriteBuffer         = IGL.GLCopyWriteBuffer,
    TextureBuffer           = IGL.GL_TEXTURE_BUFFER,
    UniformBuffer           = IGL.GLUniformBuffer
}

[PublicAPI]
public enum BufferAccess
{
    ReadOnly  = IGL.GLReadOnly,
    ReadWrite = IGL.GLReadWrite,
    WriteOnly = IGL.GLWriteOnly
}

[PublicAPI]
public enum BufferUsageHint
{
    StaticDraw  = IGL.GLStaticDraw,
    StaticCopy  = IGL.GLStaticCopy,
    StaticRead  = IGL.GLStaticRead,
    DynamicDraw = IGL.GLDynamicDraw,
    DynamicCopy = IGL.GLDynamicCopy,
    DynamicRead = IGL.GLDynamicRead,
    StreamDraw  = IGL.GLStreamDraw,
    StreamCopy  = IGL.GLStreamCopy,
    StreamRead  = IGL.GLStreamRead
}

[PublicAPI]
public enum BufferBinding
{
    ArrayBuffer                    = IGL.GLArrayBuffer,
    ArrayBufferBinding             = IGL.GLArrayBufferBinding,
    BufferSize                     = IGL.GLBufferSize,
    BufferUsage                    = IGL.GLBufferUsage,
    ColorArrayBufferBinding        = IGL.GLColorArrayBufferBinding,
    DynamicDraw                    = IGL.GLDynamicDraw,
    ElementArrayBuffer             = IGL.GLElementArrayBuffer,
    ElementArrayBufferBinding      = IGL.GLElementArrayBufferBinding,
    FramebufferBinding             = IGL.GLFramebufferBinding,
    NormalArrayBufferBinding       = IGL.GLNormalArrayBufferBinding,
    StaticDraw                     = IGL.GLStaticDraw,
    TextureCoordArrayBufferBinding = IGL.GLTextureCoordArrayBufferBinding,
    VertexArrayBinding             = IGL.GLVertexArrayBinding,
    VertexArrayBufferBinding       = IGL.GLVertexArrayBufferBinding
}

// ============================================================================
// ============================================================================