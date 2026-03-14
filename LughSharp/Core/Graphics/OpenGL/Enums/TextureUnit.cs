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
public enum TextureUnit
{
    None          = 0,
    ActiveTexture = IGL.GLActiveTexture,
    Texture0      = IGL.GLTexture0,
    Texture1      = IGL.GLTexture1,
    Texture2      = IGL.GLTexture2,
    Texture3      = IGL.GLTexture3,
    Texture4      = IGL.GLTexture4,
    Texture5      = IGL.GLTexture5,
    Texture6      = IGL.GLTexture6,
    Texture7      = IGL.GLTexture7,
    Texture8      = IGL.GLTexture8,
    Texture9      = IGL.GLTexture9,
    Texture10     = IGL.GLTexture10,
    Texture11     = IGL.GLTexture11,
    Texture12     = IGL.GLTexture12,
    Texture13     = IGL.GLTexture13,
    Texture14     = IGL.GLTexture14,
    Texture15     = IGL.GLTexture15,
    Texture16     = IGL.GLTexture16,
    Texture17     = IGL.GLTexture17,
    Texture18     = IGL.GLTexture18,
    Texture19     = IGL.GLTexture19
}