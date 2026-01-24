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
public enum TextureUnit : int
{
    None          = 0,
    ActiveTexture = IGL.GL_ACTIVE_TEXTURE,
    Texture0      = IGL.GL_TEXTURE0,
    Texture1      = IGL.GL_TEXTURE1,
    Texture2      = IGL.GL_TEXTURE2,
    Texture3      = IGL.GL_TEXTURE3,
    Texture4      = IGL.GL_TEXTURE4,
    Texture5      = IGL.GL_TEXTURE5,
    Texture6      = IGL.GL_TEXTURE6,
    Texture7      = IGL.GL_TEXTURE7,
    Texture8      = IGL.GL_TEXTURE8,
    Texture9      = IGL.GL_TEXTURE9,
    Texture10     = IGL.GL_TEXTURE10,
    Texture11     = IGL.GL_TEXTURE11,
    Texture12     = IGL.GL_TEXTURE12,
    Texture13     = IGL.GL_TEXTURE13,
    Texture14     = IGL.GL_TEXTURE14,
    Texture15     = IGL.GL_TEXTURE15,
    Texture16     = IGL.GL_TEXTURE16,
    Texture17     = IGL.GL_TEXTURE17,
    Texture18     = IGL.GL_TEXTURE18,
    Texture19     = IGL.GL_TEXTURE19,
}