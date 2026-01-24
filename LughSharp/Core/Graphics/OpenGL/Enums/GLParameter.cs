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
    LineWidth                     = IGL.GL_LINE_WIDTH,
    CullFace                      = IGL.GL_CULL_FACE,
    CullFaceMode                  = IGL.GL_CULL_FACE_MODE,
    FrontFace                     = IGL.GL_FRONT_FACE,
    DepthRange                    = IGL.GL_DEPTH_RANGE,
    DepthTest                     = IGL.GL_DEPTH_TEST,
    DepthWritemask                = IGL.GL_DEPTH_WRITEMASK,
    DepthClearValue               = IGL.GL_DEPTH_CLEAR_VALUE,
    DepthFunc                     = IGL.GL_DEPTH_FUNC,
    StencilTest                   = IGL.GL_STENCIL_TEST,
    StencilClearValue             = IGL.GL_STENCIL_CLEAR_VALUE,
    StencilFunc                   = IGL.GL_STENCIL_FUNC,
    StencilValueMask              = IGL.GL_STENCIL_VALUE_MASK,
    StencilFail                   = IGL.GL_STENCIL_FAIL,
    StencilPassDepthFail          = IGL.GL_STENCIL_PASS_DEPTH_FAIL,
    StencilPassDepthPass          = IGL.GL_STENCIL_PASS_DEPTH_PASS,
    StencilRef                    = IGL.GL_STENCIL_REF,
    StencilWritemask              = IGL.GL_STENCIL_WRITEMASK,
    Viewport                      = IGL.GL_VIEWPORT,
    Dither                        = IGL.GL_DITHER,
    Blend                         = IGL.GL_BLEND,
    ScissorBox                    = IGL.GL_SCISSOR_BOX,
    ScissorTest                   = IGL.GL_SCISSOR_TEST,
    ColorClearValue               = IGL.GL_COLOR_CLEAR_VALUE,
    ColorWritemask                = IGL.GL_COLOR_WRITEMASK,
    UnpackAlignment               = IGL.GL_UNPACK_ALIGNMENT,
    PackAlignment                 = IGL.GL_PACK_ALIGNMENT,
    MaxTextureSize                = IGL.GL_MAX_TEXTURE_SIZE,
    MaxViewportDims               = IGL.GL_MAX_VIEWPORT_DIMS,
    SubpixelBits                  = IGL.GL_SUBPIXEL_BITS,
    RedBits                       = IGL.GL_RED_BITS,
    GreenBits                     = IGL.GL_GREEN_BITS,
    BlueBits                      = IGL.GL_BLUE_BITS,
    AlphaBits                     = IGL.GL_ALPHA_BITS,
    DepthBits                     = IGL.GL_DEPTH_BITS,
    StencilBits                   = IGL.GL_STENCIL_BITS,
    Texture2D                     = IGL.GL_TEXTURE_2D,
    PolygonOffsetUnits            = IGL.GL_POLYGON_OFFSET_UNITS,
    BlendColor                    = IGL.GL_BLEND_COLOR,
    BlendEquation                 = IGL.GL_BLEND_EQUATION,
    BlendEquationRgb              = IGL.GL_BLEND_EQUATION_RGB,
    PolygonOffsetFill             = IGL.GL_POLYGON_OFFSET_FILL,
    PolygonOffsetFactor           = IGL.GL_POLYGON_OFFSET_FACTOR,
    TextureBinding2D              = IGL.GL_TEXTURE_BINDING_2D,
    SampleAlphaToCoverage         = IGL.GL_SAMPLE_ALPHA_TO_COVERAGE,
    SampleCoverage                = IGL.GL_SAMPLE_COVERAGE,
    SampleBuffers                 = IGL.GL_SAMPLE_BUFFERS,
    Samples                       = IGL.GL_SAMPLES,
    SampleCoverageValue           = IGL.GL_SAMPLE_COVERAGE_VALUE,
    SampleCoverageInvert          = IGL.GL_SAMPLE_COVERAGE_INVERT,
    BlendDstRgb                   = IGL.GL_BLEND_DST_RGB,
    BlendSrcRgb                   = IGL.GL_BLEND_SRC_RGB,
    BlendDstAlpha                 = IGL.GL_BLEND_DST_ALPHA,
    BlendSrcAlpha                 = IGL.GL_BLEND_SRC_ALPHA,
    GenerateMipmapHint            = IGL.GL_GENERATE_MIPMAP_HINT,
    AliasedPointSizeRange         = IGL.GL_ALIASED_POINT_SIZE_RANGE,
    AliasedLineWidthRange         = IGL.GL_ALIASED_LINE_WIDTH_RANGE,
    ActiveTexture                 = IGL.GL_ACTIVE_TEXTURE,
    MaxRenderbufferSize           = IGL.GL_MAX_RENDERBUFFER_SIZE,
    TextureBindingCubeMap         = IGL.GL_TEXTURE_BINDING_CUBE_MAP,
    MaxCubeMapTextureSize         = IGL.GL_MAX_CUBE_MAP_TEXTURE_SIZE,
    NumCompressedTextureFormats   = IGL.GL_NUM_COMPRESSED_TEXTURE_FORMATS,
    CompressedTextureFormats      = IGL.GL_COMPRESSED_TEXTURE_FORMATS,
    StencilBackFunc               = IGL.GL_STENCIL_BACK_FUNC,
    StencilBackFail               = IGL.GL_STENCIL_BACK_FAIL,
    StencilBackPassDepthFail      = IGL.GL_STENCIL_BACK_PASS_DEPTH_FAIL,
    StencilBackPassDepthPass      = IGL.GL_STENCIL_BACK_PASS_DEPTH_PASS,
    BlendEquationAlpha            = IGL.GL_BLEND_EQUATION_ALPHA,
    MaxVertexAttribs              = IGL.GL_MAX_VERTEX_ATTRIBS,
    MaxTextureImageUnits          = IGL.GL_MAX_TEXTURE_IMAGE_UNITS,
    ArrayBufferBinding            = IGL.GL_ARRAY_BUFFER_BINDING,
    ElementArrayBufferBinding     = IGL.GL_ELEMENT_ARRAY_BUFFER_BINDING,
    MaxVertexTextureImageUnits    = IGL.GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS,
    MaxCombinedTextureImageUnits  = IGL.GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS,
    CurrentProgram                = IGL.GL_CURRENT_PROGRAM,
    ImplementationColorReadType   = IGL.GL_IMPLEMENTATION_COLOR_READ_TYPE,
    ImplementationColorReadFormat = IGL.GL_IMPLEMENTATION_COLOR_READ_FORMAT,
    StencilBackRef                = IGL.GL_STENCIL_BACK_REF,
    StencilBackValueMask          = IGL.GL_STENCIL_BACK_VALUE_MASK,
    StencilBackWritemask          = IGL.GL_STENCIL_BACK_WRITEMASK,
    FramebufferBinding            = IGL.GL_FRAMEBUFFER_BINDING,
    RenderbufferBinding           = IGL.GL_RENDERBUFFER_BINDING,
    ShaderBinaryFormats           = IGL.GL_SHADER_BINARY_FORMATS,
    NumShaderBinaryFormats        = IGL.GL_NUM_SHADER_BINARY_FORMATS,
    ShaderCompiler                = IGL.GL_SHADER_COMPILER,
    MaxVertexUniformVectors       = IGL.GL_MAX_VERTEX_UNIFORM_VECTORS,
    MaxVaryingVectors             = IGL.GL_MAX_VARYING_VECTORS,
    MaxFragmentUniformVectors     = IGL.GL_MAX_FRAGMENT_UNIFORM_VECTORS,
    VertexArrayBinding            = IGL.GL_VERTEX_ARRAY_BINDING,
}

// ========================================================================
// ========================================================================