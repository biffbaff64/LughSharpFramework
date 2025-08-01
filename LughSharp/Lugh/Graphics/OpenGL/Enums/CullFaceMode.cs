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
public enum CullFaceMode
{
    FrontLeft    = IGL.GL_FRONT_LEFT,
    FrontRight   = IGL.GL_FRONT_RIGHT,
    BackLeft     = IGL.GL_BACK_LEFT,
    BackRight    = IGL.GL_BACK_RIGHT,
    Front        = IGL.GL_FRONT,
    Back         = IGL.GL_BACK,
    Left         = IGL.GL_LEFT,
    Right        = IGL.GL_RIGHT,
    FrontAndBack = IGL.GL_FRONT_AND_BACK,
}

[PublicAPI]
public enum FrontFaceDirection
{
    Clockwise        = IGL.GL_CW,
    CounterClockwise = IGL.GL_CCW,
}

// ========================================================================
// ========================================================================