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

namespace LughSharp.Core.Graphics.OpenGL.Enums;

[PublicAPI]
public enum TextureParameter : int
{
    MinFilter             = IGL.GL_TEXTURE_MIN_FILTER,
    MagFilter             = IGL.GL_TEXTURE_MAG_FILTER,
    WrapS                 = IGL.GL_TEXTURE_WRAP_S,
    WrapT                 = IGL.GL_TEXTURE_WRAP_T,
    BaseLevel             = IGL.GL_TEXTURE_BASE_LEVEL,
    MaxLevel              = IGL.GL_TEXTURE_MAX_LEVEL,
    CompareMode           = IGL.GL_TEXTURE_COMPARE_MODE,
    CompareFunc           = IGL.GL_TEXTURE_COMPARE_FUNC,
    MinLod                = IGL.GL_TEXTURE_MIN_LOD,
    MaxLod                = IGL.GL_TEXTURE_MAX_LOD,
    BorderColor           = IGL.GL_TEXTURE_BORDER_COLOR,
    TextureSwizzleR       = IGL.GL_TEXTURE_SWIZZLE_R,
    TextureSwizzleG       = IGL.GL_TEXTURE_SWIZZLE_G,
    TextureSwizzleB       = IGL.GL_TEXTURE_SWIZZLE_B,
    TextureSwizzleA       = IGL.GL_TEXTURE_SWIZZLE_A,
    TextureWidth          = IGL.GL_TEXTURE_WIDTH,
    TextureHeight         = IGL.GL_TEXTURE_HEIGHT,
    TextureDepth          = IGL.GL_TEXTURE_DEPTH,
    TextureInternalFormat = IGL.GL_TEXTURE_INTERNAL_FORMAT,
    TextureRedSize        = IGL.GL_TEXTURE_RED_SIZE,
    TextureGreenSize      = IGL.GL_TEXTURE_GREEN_SIZE,
    TextureMaxLevel       = IGL.GL_TEXTURE_MAX_LEVEL,
}

// ========================================================================
// ========================================================================