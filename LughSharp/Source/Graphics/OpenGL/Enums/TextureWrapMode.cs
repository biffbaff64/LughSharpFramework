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

namespace LughSharp.Source.Graphics.OpenGL.Enums;

[PublicAPI]
public enum TextureWrapMode
{
    ClampToBorder = IGL.GL_CLAMP_TO_BORDER,

    /// <summary>
    /// Repeats the texture, mirroring it at every integer boundary. This
    /// creates a seamless mirrored effect at the edges.
    /// </summary>
    MirroredRepeat = IGL.GLMirroredRepeat,

    /// <summary>
    /// Clamps texture coordinates to the edges of the texture, ensuring
    /// that texture sampling outside the bounds of the texture fetches
    /// the color from the nearest edge texel.
    /// </summary>
    ClampToEdge = IGL.GLClampToEdge,

    /// <summary>
    /// Wraps texture coordinates, causing the texture to repeat when
    /// coordinates exceed the range [0.0, 1.0].
    /// </summary>
    Repeat = IGL.GLRepeat
}