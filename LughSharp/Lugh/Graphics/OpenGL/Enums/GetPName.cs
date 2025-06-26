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
public enum GetPName
{
    AliasedLineWidthRange = IGL.GL_ALIASED_LINE_WIDTH_RANGE,
    AliasedPointSizeRange = IGL.GL_ALIASED_POINT_SIZE_RANGE,

    // ------------------------------------------------------------------------

    AlphaBits = IGL.GL_ALPHA_BITS,
    BlueBits  = IGL.GL_BLUE_BITS,
    GreenBits = IGL.GL_GREEN_BITS,
    RedBits   = IGL.GL_RED_BITS,

    // ------------------------------------------------------------------------

    ColorClearValue      = IGL.GL_COLOR_CLEAR_VALUE,
    ColorWriteMask       = IGL.GL_COLOR_WRITEMASK,
    CullFaceMode         = IGL.GL_CULL_FACE_MODE,
    DepthBits            = IGL.GL_DEPTH_BITS,
    DepthClearValue      = IGL.GL_DEPTH_CLEAR_VALUE,
    DepthFunc            = IGL.GL_DEPTH_FUNC,
    DepthRange           = IGL.GL_DEPTH_RANGE,
    DepthWriteMask       = IGL.GL_DEPTH_WRITEMASK,
    FrontFace            = IGL.GL_FRONT_FACE,
    LineWidth            = IGL.GL_LINE_WIDTH,
    MaxTextureSize       = IGL.GL_MAX_TEXTURE_SIZE,
    MaxViewportDims      = IGL.GL_MAX_VIEWPORT_DIMS,
    PackAlignment        = IGL.GL_PACK_ALIGNMENT,
    PolygonOffsetFactor  = IGL.GL_POLYGON_OFFSET_FACTOR,
    PolygonOffsetUnits   = IGL.GL_POLYGON_OFFSET_UNITS,
    SampleBuffers        = IGL.GL_SAMPLE_BUFFERS,
    SampleCoverageInvert = IGL.GL_SAMPLE_COVERAGE_INVERT,
    SampleCoverageValue  = IGL.GL_SAMPLE_COVERAGE_VALUE,
    Samples              = IGL.GL_SAMPLES,
    ScissorBox           = IGL.GL_SCISSOR_BOX,

    // ------------------------------------------------------------------------

    StencilBackFail          = IGL.GL_STENCIL_BACK_FAIL,
    StencilBackFunc          = IGL.GL_STENCIL_BACK_FUNC,
    StencilBackpassDepthFail = IGL.GL_STENCIL_BACK_PASS_DEPTH_FAIL,
    StencilBackpassDepthPass = IGL.GL_STENCIL_BACK_PASS_DEPTH_PASS,
    StencilBackRef           = IGL.GL_STENCIL_BACK_REF,
    StencilBackValueMask     = IGL.GL_STENCIL_BACK_VALUE_MASK,
    StencilBackWriteMask     = IGL.GL_STENCIL_BACK_WRITEMASK,
    StencilBits              = IGL.GL_STENCIL_BITS,
    StencilClearValue        = IGL.GL_STENCIL_CLEAR_VALUE,
    StencilFail              = IGL.GL_STENCIL_FAIL,
    StencilFunc              = IGL.GL_STENCIL_FUNC,
    StencilPassDepthFail     = IGL.GL_STENCIL_PASS_DEPTH_FAIL,
    StencilPassDepthPass     = IGL.GL_STENCIL_PASS_DEPTH_PASS,
    StencilRef               = IGL.GL_STENCIL_REF,
    StencilValueMask         = IGL.GL_STENCIL_VALUE_MASK,
    StencilWriteMask         = IGL.GL_STENCIL_WRITEMASK,

    // ------------------------------------------------------------------------

    SubpixelBits     = IGL.GL_SUBPIXEL_BITS,
    TextureBinding2D = IGL.GL_TEXTURE_BINDING_2D,
    UnpackAlignment  = IGL.GL_UNPACK_ALIGNMENT,
    Viewport         = IGL.GL_VIEWPORT,
}

// ========================================================================
// ========================================================================