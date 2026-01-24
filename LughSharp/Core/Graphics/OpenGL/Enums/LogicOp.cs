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
public enum LogicOp : int
{
    And          = IGL.GL_AND,
    AndInverted  = IGL.GL_AND_INVERTED,
    AndReverse   = IGL.GL_AND_REVERSE,
    Clear        = IGL.GL_CLEAR,
    Copy         = IGL.GL_COPY,
    CopyInverted = IGL.GL_COPY_INVERTED,
    Equiv        = IGL.GL_EQUIV,
    Invert       = IGL.GL_INVERT,
    Nand         = IGL.GL_NAND,
    Noop         = IGL.GL_NOOP,
    Nor          = IGL.GL_NOR,
    Or           = IGL.GL_OR,
    OrInverted   = IGL.GL_OR_INVERTED,
    OrReversed   = IGL.GL_OR_REVERSE,
    Set          = IGL.GL_SET,
    Xor          = IGL.GL_XOR,
}