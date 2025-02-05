﻿// /////////////////////////////////////////////////////////////////////////////
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

namespace LughSharp.Lugh.Graphics.OpenGL.Enums;

[PublicAPI]
public enum BufferTarget : int
{
    ArrayBuffer        = IGL.GL_ARRAY_BUFFER,
    ElementArrayBuffer = IGL.GL_ELEMENT_ARRAY_BUFFER,

    // ... other buffer targets
}

[PublicAPI]
public enum BufferUsageHint : int
{
    StaticDraw  = IGL.GL_STATIC_DRAW,
    DynamicDraw = IGL.GL_DYNAMIC_DRAW,
    StreamDraw  = IGL.GL_STREAM_DRAW,

    // ... other usage hints
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
    NormalArrayBufferBinding       = IGL.GL_NORMAL_ARRAY_BUFFER_BINDING,
    StaticDraw                     = IGL.GL_STATIC_DRAW,
    TextureCoordArrayBufferBinding = IGL.GL_TEXTURE_COORD_ARRAY_BUFFER_BINDING,
    VertexArrayBinding             = IGL.GL_VERTEX_ARRAY_BINDING,
    VertexArrayBufferBinding       = IGL.GL_VERTEX_ARRAY_BUFFER_BINDING,
}