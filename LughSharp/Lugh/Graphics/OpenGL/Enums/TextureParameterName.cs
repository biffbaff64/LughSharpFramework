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

namespace LughSharp.Lugh.Graphics.OpenGL.Enums;

[PublicAPI]
public enum TextureParameterName : int
{
    TextureMinFilter   = IGL.GL_TEXTURE_MIN_FILTER,
    TextureMagFilter   = IGL.GL_TEXTURE_MAG_FILTER,
    TextureWrapS       = IGL.GL_TEXTURE_WRAP_S,
    TextureWrapT       = IGL.GL_TEXTURE_WRAP_T,
    TextureBaseLevel   = IGL.GL_TEXTURE_BASE_LEVEL,
    TextureMaxLevel    = IGL.GL_TEXTURE_MAX_LEVEL,
    TextureCompareMode = IGL.GL_TEXTURE_COMPARE_MODE,
    TextureCompareFunc = IGL.GL_TEXTURE_COMPARE_FUNC,
    TextureMinLod      = IGL.GL_TEXTURE_MIN_LOD,
    TextureMaxLod      = IGL.GL_TEXTURE_MAX_LOD,
}

[PublicAPI]
public enum TextureMinFilter : int
{
    NearestMipmapNearest = IGL.GL_NEAREST_MIPMAP_NEAREST,
    LinearMipmapNearest  = IGL.GL_LINEAR_MIPMAP_NEAREST,
    NearestMipmapLinear  = IGL.GL_NEAREST_MIPMAP_LINEAR,
    LinearMipmapLinear   = IGL.GL_LINEAR_MIPMAP_LINEAR,
    Nearest              = IGL.GL_NEAREST,
    Linear               = IGL.GL_LINEAR,
}

[PublicAPI]
public enum TextureMagFilter : int
{
    Nearest = IGL.GL_NEAREST,
    Linear  = IGL.GL_LINEAR,
}
// ========================================================================
// ========================================================================