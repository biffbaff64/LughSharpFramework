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
public enum BlendMode
{
    Zero                  = IGL.GL_ZERO,
    One                   = IGL.GL_ONE,
    SrcColor              = IGL.GL_SRC_COLOR,
    OneMinusSrcColor      = IGL.GL_ONE_MINUS_SRC_COLOR,
    DstColor              = IGL.GL_DST_COLOR,
    OneMinusDstColor      = IGL.GL_ONE_MINUS_DST_COLOR,
    SrcAlpha              = IGL.GL_SRC_ALPHA,
    OneMinusSrcAlpha      = IGL.GL_ONE_MINUS_SRC_ALPHA,
    DstAlpha              = IGL.GL_DST_ALPHA,
    OneMinusDstAlpha      = IGL.GL_ONE_MINUS_DST_ALPHA,
    ConstantColor         = IGL.GL_CONSTANT_COLOR,
    OneMinusConstantColor = IGL.GL_ONE_MINUS_CONSTANT_COLOR,
    ConstantAlpha         = IGL.GL_CONSTANT_ALPHA,
    OneMinusConstantAlpha = IGL.GL_ONE_MINUS_CONSTANT_ALPHA,
    SrcAlphaSaturate      = IGL.GL_SRC_ALPHA_SATURATE,
}

// ========================================================================
// ========================================================================

