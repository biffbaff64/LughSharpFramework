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

/// <summary>
/// This is 'GetPName' in OpenTK.
/// </summary>
[PublicAPI]
public enum GLParameter
{
    LineWidth                     = IGL.GLLineWidth,
    CullFace                      = IGL.GLCullFace,
    CullFaceMode                  = IGL.GLCullFaceMode,
    FrontFace                     = IGL.GLFrontFace,
    DepthRange                    = IGL.GLDepthRange,
    DepthTest                     = IGL.GLDepthTest,
    DepthWritemask                = IGL.GLDepthWritemask,
    DepthClearValue               = IGL.GLDepthClearValue,
    DepthFunc                     = IGL.GLDepthFunc,
    StencilTest                   = IGL.GLStencilTest,
    StencilClearValue             = IGL.GLStencilClearValue,
    StencilFunc                   = IGL.GLStencilFunc,
    StencilValueMask              = IGL.GLStencilValueMask,
    StencilFail                   = IGL.GLStencilFail,
    StencilPassDepthFail          = IGL.GLStencilPassDepthFail,
    StencilPassDepthPass          = IGL.GLStencilPassDepthPass,
    StencilRef                    = IGL.GLStencilRef,
    StencilWritemask              = IGL.GLStencilWritemask,
    Viewport                      = IGL.GLViewport,
    Dither                        = IGL.GLDither,
    Blend                         = IGL.GLBlend,
    ScissorBox                    = IGL.GLScissorBox,
    ScissorTest                   = IGL.GLScissorTest,
    ColorClearValue               = IGL.GLColorClearValue,
    ColorWritemask                = IGL.GLColorWritemask,
    UnpackAlignment               = IGL.GLUnpackAlignment,
    PackAlignment                 = IGL.GLPackAlignment,
    MaxTextureSize                = IGL.GLMaxTextureSize,
    MaxViewportDims               = IGL.GLMaxViewportDims,
    SubpixelBits                  = IGL.GLSubpixelBits,
    RedBits                       = IGL.GLRedBits,
    GreenBits                     = IGL.GLGreenBits,
    BlueBits                      = IGL.GLBlueBits,
    AlphaBits                     = IGL.GLAlphaBits,
    DepthBits                     = IGL.GLDepthBits,
    StencilBits                   = IGL.GLStencilBits,
    Texture2D                     = IGL.GLTexture2D,
    PolygonOffsetUnits            = IGL.GLPolygonOffsetUnits,
    BlendColor                    = IGL.GLBlendColor,
    BlendEquation                 = IGL.GLBlendEquation,
    BlendEquationRgb              = IGL.GLBlendEquationRGB,
    PolygonOffsetFill             = IGL.GLPolygonOffsetFill,
    PolygonOffsetFactor           = IGL.GLPolygonOffsetFactor,
    TextureBinding2D              = IGL.GLTextureBinding2D,
    SampleAlphaToCoverage         = IGL.GLSampleAlphaToCoverage,
    SampleCoverage                = IGL.GLSampleCoverage,
    SampleBuffers                 = IGL.GLSampleBuffers,
    Samples                       = IGL.GLSamples,
    SampleCoverageValue           = IGL.GLSampleCoverageValue,
    SampleCoverageInvert          = IGL.GLSampleCoverageInvert,
    BlendDstRgb                   = IGL.GLBlendDstRGB,
    BlendSrcRgb                   = IGL.GLBlendSrcRGB,
    BlendDstAlpha                 = IGL.GLBlendDstAlpha,
    BlendSrcAlpha                 = IGL.GLBlendSrcAlpha,
    GenerateMipmapHint            = IGL.GLGenerateMipmapHint,
    AliasedPointSizeRange         = IGL.GLAliasedPointSizeRange,
    AliasedLineWidthRange         = IGL.GLAliasedLineWidthRange,
    ActiveTexture                 = IGL.GLActiveTexture,
    MaxRenderbufferSize           = IGL.GLMaxRenderbufferSize,
    TextureBindingCubeMap         = IGL.GLTextureBindingCubeMap,
    MaxCubeMapTextureSize         = IGL.GLMaxCubeMapTextureSize,
    NumCompressedTextureFormats   = IGL.GLNumCompressedTextureFormats,
    CompressedTextureFormats      = IGL.GLCompressedTextureFormats,
    StencilBackFunc               = IGL.GLStencilBackFunc,
    StencilBackFail               = IGL.GLStencilBackFail,
    StencilBackPassDepthFail      = IGL.GLStencilBackPassDepthFail,
    StencilBackPassDepthPass      = IGL.GLStencilBackPassDepthPass,
    BlendEquationAlpha            = IGL.GLBlendEquationAlpha,
    MaxVertexAttribs              = IGL.GLMaxVertexAttribs,
    MaxTextureImageUnits          = IGL.GLMaxTextureImageUnits,
    ArrayBufferBinding            = IGL.GLArrayBufferBinding,
    ElementArrayBufferBinding     = IGL.GLElementArrayBufferBinding,
    MaxVertexTextureImageUnits    = IGL.GLMaxVertexTextureImageUnits,
    MaxCombinedTextureImageUnits  = IGL.GLMaxCombinedTextureImageUnits,
    CurrentProgram                = IGL.GLCurrentProgram,
    ImplementationColorReadType   = IGL.GLImplementationColorReadType,
    ImplementationColorReadFormat = IGL.GLImplementationColorReadFormat,
    StencilBackRef                = IGL.GLStencilBackRef,
    StencilBackValueMask          = IGL.GLStencilBackValueMask,
    StencilBackWritemask          = IGL.GLStencilBackWritemask,
    FramebufferBinding            = IGL.GLFramebufferBinding,
    RenderbufferBinding           = IGL.GLRenderbufferBinding,
    ShaderBinaryFormats           = IGL.GLShaderBinaryFormats,
    NumShaderBinaryFormats        = IGL.GLNumShaderBinaryFormats,
    ShaderCompiler                = IGL.GLShaderCompiler,
    MaxVertexUniformVectors       = IGL.GLMaxVertexUniformVectors,
    MaxVaryingVectors             = IGL.GLMaxVaryingVectors,
    MaxFragmentUniformVectors     = IGL.GLMaxFragmentUniformVectors,
    VertexArrayBinding            = IGL.GLVertexArrayBinding
}

// ========================================================================
// ========================================================================