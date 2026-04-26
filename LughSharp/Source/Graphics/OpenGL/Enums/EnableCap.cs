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
public enum EnableCap
{
    // --------------------------------
    ColorArray    = IGL.GLColorArray,
    ColorLogicOp  = IGL.GLColorLogicOp,
    ColorMaterial = IGL.GLColorMaterial,

    // --------------------------------
    CullFace = IGL.GLCullFace,
    Fog      = IGL.GLFog,
    Lighting = IGL.GLLighting,

    // --------------------------------
    DebugOutput            = IGL.GL_DEBUG_OUTPUT,
    DebugOutputSynchronous = IGL.GL_DEBUG_OUTPUT_SYNCHRONOUS,

    // --------------------------------
    TextureCubemapSeamless = IGL.GLTextureCubeMapSeamless,
    Texture1D              = IGL.GLTexture1D,
    Texture2D              = IGL.GLTexture2D,
    TextureCoordArray      = IGL.GLTextureCoordArray,

    // --------------------------------
    Blend  = IGL.GLBlend,
    Dither = IGL.GLDither,

    // --------------------------------
    ScissorTest = IGL.GLScissorTest,
    StencilTest = IGL.GLStencilTest,
    DepthTest   = IGL.GLDepthTest,

    // --------------------------------
    FramebufferSrgb = IGL.GLFramebufferSrgb,

    // --------------------------------
    NormalArray = IGL.GLNormalArray,
    Normalize   = IGL.GLNormalize,

    // --------------------------------
    VertexArray = IGL.GL_VERTEX_ARRAY,

    // --------------------------------
    SampleAlphaToCoverage = IGL.GLSampleAlphaToCoverage,
    SampleAlphaToOne      = IGL.GLSampleAlphaToOne,
    SampleCoverage        = IGL.GLSampleCoverage
}