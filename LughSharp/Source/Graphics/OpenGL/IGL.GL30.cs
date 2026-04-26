// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

using System.Diagnostics.CodeAnalysis;

namespace LughSharp.Source.Graphics.OpenGL;

/// <summary>
/// OpenGL 3.0 functions.
/// </summary>
[SuppressMessage( "ReSharper", "InconsistentNaming" )]
partial interface IGL
{
    const int GLReadBuffer                   = 0x0C02;
    const int GLUnpackRowLength              = 0x0CF2;
    const int GLUnpackSkipRows               = 0x0CF3;
    const int GLUnpackSkipPixels             = 0x0CF4;
    const int GLPackRowLength                = 0x0D02;
    const int GLPackSkipRows                 = 0x0D03;
    const int GLPackSkipPixels               = 0x0D04;
    const int GLColor                        = 0x1800;
    const int GLDepth                        = 0x1801;
    const int GLStencil                      = 0x1802;
    const int GLRed                          = 0x1903;
    const int GLRGB8                         = 0x8051;
    const int GLRGBA8                        = 0x8058;
    const int GLRGB10A2                      = 0x8059;
    const int GLTextureBinding3D             = 0x806A;
    const int GLUnpackSkipImages             = 0x806D;
    const int GLUnpackImageHeight            = 0x806E;
    const int GLTexture3D                    = 0x806F;
    const int GLTextureWrapR                 = 0x8072;
    const int GLMax3DTextureSize             = 0x8073;
    const int GLUnsignedInt2101010Rev        = 0x8368;
    const int GLMaxElementsVertices          = 0x80E8;
    const int GLMaxElementsIndices           = 0x80E9;
    const int GLTextureMinLod                = 0x813A;
    const int GLTextureMaxLod                = 0x813B;
    const int GLTextureBaseLevel             = 0x813C;
    const int GLTextureMaxLevel              = 0x813D;
    const int GLMin                          = 0x8007;
    const int GLMax                          = 0x8008;
    const int GLDepthComponent24             = 0x81A6;
    const int GLMaxTextureLodBias            = 0x84FD;
    const int GLTextureCompareMode           = 0x884C;
    const int GLTextureCompareFunc           = 0x884D;
    const int GLCurrentQuery                 = 0x8865;
    const int GLQueryResult                  = 0x8866;
    const int GLQueryResultAvailable         = 0x8867;
    const int GLBufferMapped                 = 0x88BC;
    const int GLBufferMapPointer             = 0x88BD;
    const int GLStreamRead                   = 0x88E1;
    const int GLStreamCopy                   = 0x88E2;
    const int GLStaticRead                   = 0x88E5;
    const int GLStaticCopy                   = 0x88E6;
    const int GLDynamicRead                  = 0x88E9;
    const int GLDynamicCopy                  = 0x88EA;
    const int GLMaxDrawBuffers               = 0x8824;
    const int GLDrawBuffer0                  = 0x8825;
    const int GLDrawBuffer1                  = 0x8826;
    const int GLDrawBuffer2                  = 0x8827;
    const int GLDrawBuffer3                  = 0x8828;
    const int GLDrawBuffer4                  = 0x8829;
    const int GLDrawBuffer5                  = 0x882A;
    const int GLDrawBuffer6                  = 0x882B;
    const int GLDrawBuffer7                  = 0x882C;
    const int GLDrawBuffer8                  = 0x882D;
    const int GLDrawBuffer9                  = 0x882E;
    const int GLDrawBuffer10                 = 0x882F;
    const int GLDrawBuffer11                 = 0x8830;
    const int GLDrawBuffer12                 = 0x8831;
    const int GLDrawBuffer13                 = 0x8832;
    const int GLDrawBuffer14                 = 0x8833;
    const int GLDrawBuffer15                 = 0x8834;
    const int GLMaxFragmentUniformComponents = 0x8B49;
    const int GLMaxVertexUniformComponents   = 0x8B4A;
    const int GLSampler3D                    = 0x8B5F;
    const int GLSampler2DShadow              = 0x8B62;
    const int GLFragmentShaderDerivativeHint = 0x8B8B;
    const int GLPixelPackBuffer              = 0x88EB;
    const int GLPixelUnpackBuffer            = 0x88EC;
    const int GLPixelPackBufferBinding       = 0x88ED;

    const int GLPixelUnpackBufferBinding = 0x88EF;

//    const int GLFloatMat2X3                                  = 0x8B65;
//    const int GLFloatMat2X4                                  = 0x8B66;
//    const int GLFloatMat3X2                                  = 0x8B67;
//    const int GLFloatMat3X4                                  = 0x8B68;
//    const int GLFloatMat4X2                                  = 0x8B69;
//    const int GLFloatMat4X3                                  = 0x8B6A;
    const int GLSrgb                = 0x8C40;
    const int GLSrgb8               = 0x8C41;
    const int GLSrgb8Alpha8         = 0x8C43;
    const int GLCompareRefToTexture = 0x884E;
    const int GLMajorVersion        = 0x821B;
    const int GLMinorVersion        = 0x821C;
    const int GLNumExtensions       = 0x821D;

//    const int GLRGBA32F                                       = 0x8814;
//    const int GLRGB32F                                        = 0x8815;
//    const int GLRGBA16F                                       = 0x881A;
//    const int GLRGB16F                                        = 0x881B;
    const int GLVertexAttribArrayInteger = 0x88FD;
    const int GLMaxArrayTextureLayers    = 0x88FF;
    const int GLMinProgramTexelOffset    = 0x8904;
    const int GLMaxProgramTexelOffset    = 0x8905;
    const int GLMaxVaryingComponents     = 0x8B4B;
    const int GLTexture2DArray           = 0x8C1A;
    const int GLTextureBinding2DArray    = 0x8C1D;

//    const int GLR11FG11FB10F                                = 0x8C3A;
//    const int GLUnsignedInt10F11F11FRev                  = 0x8C3B;
    const int GLRGB9E5                                    = 0x8C3D;
    const int GLUnsignedInt5999Rev                        = 0x8C3E;
    const int GLTransformFeedbackVaryingMaxLength         = 0x8C76;
    const int GLTransformFeedbackBufferMode               = 0x8C7F;
    const int GLMaxTransformFeedbackSeparateComponents    = 0x8C80;
    const int GLTransformFeedbackVaryings                 = 0x8C83;
    const int GLTransformFeedbackBufferStart              = 0x8C84;
    const int GLTransformFeedbackBufferSize               = 0x8C85;
    const int GLTransformFeedbackPrimitivesWritten        = 0x8C88;
    const int GLRasterizerDiscard                         = 0x8C89;
    const int GLMaxTransformFeedbackInterleavedComponents = 0x8C8A;
    const int GLMaxTransformFeedbackSeparateAttribs       = 0x8C8B;
    const int GLInterleavedAttribs                        = 0x8C8C;
    const int GLSeparateAttribs                           = 0x8C8D;
    const int GLTransformFeedbackBuffer                   = 0x8C8E;
    const int GLTransformFeedbackBufferBinding            = 0x8C8F;

//    const int GLRGBA32UI                                      = 0x8D70;
//    const int GLRGB32UI                                       = 0x8D71;
//    const int GLRGBA16UI                                      = 0x8D76;
//    const int GLRGB16UI                                       = 0x8D77;
//    const int GLRGBA8UI                                       = 0x8D7C;
//    const int GLRGB8UI                                        = 0x8D7D;
//    const int GLRGBA32I                                       = 0x8D82;
//    const int GLRGB32I                                        = 0x8D83;
//    const int GLRGBA16I                                       = 0x8D88;
//    const int GLRGB16I                                        = 0x8D89;
//    const int GLRGBA8I                                        = 0x8D8E;
//    const int GLRGB8I                                         = 0x8D8F;
    const int GLRedInteger                = 0x8D94;
    const int GLRGBInteger                = 0x8D98;
    const int GLRGBAInteger               = 0x8D99;
    const int GLSampler2DArray            = 0x8DC1;
    const int GLSampler2DArrayShadow      = 0x8DC4;
    const int GLSamplerCubeShadow         = 0x8DC5;
    const int GLUnsignedIntVec2           = 0x8DC6;
    const int GLUnsignedIntVec3           = 0x8DC7;
    const int GLUnsignedIntVec4           = 0x8DC8;
    const int GLIntSampler2D              = 0x8DCA;
    const int GLIntSampler3D              = 0x8DCB;
    const int GLIntSamplerCube            = 0x8DCC;
    const int GLIntSampler2DArray         = 0x8DCF;
    const int GLUnsignedIntSampler2D      = 0x8DD2;
    const int GLUnsignedIntSampler3D      = 0x8DD3;
    const int GLUnsignedIntSamplerCube    = 0x8DD4;
    const int GLUnsignedIntSampler2DArray = 0x8DD7;
    const int GLBufferAccessFlags         = 0x911F;
    const int GLBufferMapLength           = 0x9120;
    const int GLBufferMapOffset           = 0x9121;

//    const int GLDepthComponent32F                            = 0x8CAC;
//    const int GLDepth32FStencil8                             = 0x8CAD;
    const int GLFloat32UnsignedInt248Rev           = 0x8DAD;
    const int GLFramebufferAttachmentColorEncoding = 0x8210;
    const int GLFramebufferAttachmentComponentType = 0x8211;
    const int GLFramebufferAttachmentRedSize       = 0x8212;
    const int GLFramebufferAttachmentGreenSize     = 0x8213;
    const int GLFramebufferAttachmentBlueSize      = 0x8214;
    const int GLFramebufferAttachmentAlphaSize     = 0x8215;
    const int GLFramebufferAttachmentDepthSize     = 0x8216;
    const int GLFramebufferAttachmentStencilSize   = 0x8217;
    const int GLFramebufferDefault                 = 0x8218;
    const int GLFramebufferUndefined               = 0x8219;
    const int GLDepthStencilAttachment             = 0x821A;
    const int GLDepthStencil                       = 0x84F9;
    const int GLUnsignedInt248                     = 0x84FA;
    const int GLDepth24Stencil8                    = 0x88F0;
    const int GLUnsignedNormalized                 = 0x8C17;
    const int GLDrawFramebufferBinding             = GLFramebufferBinding;
    const int GLReadFramebuffer                    = 0x8CA8;
    const int GLDrawFramebuffer                    = 0x8CA9;
    const int GLReadFramebufferBinding             = 0x8CAA;
    const int GLRenderbufferSamples                = 0x8CAB;
    const int GLFramebufferAttachmentTextureLayer  = 0x8CD4;
    const int GLMaxColorAttachments                = 0x8CDF;
    const int GLColorAttachment1                   = 0x8CE1;
    const int GLColorAttachment2                   = 0x8CE2;
    const int GLColorAttachment3                   = 0x8CE3;
    const int GLColorAttachment4                   = 0x8CE4;
    const int GLColorAttachment5                   = 0x8CE5;
    const int GLColorAttachment6                   = 0x8CE6;
    const int GLColorAttachment7                   = 0x8CE7;
    const int GLColorAttachment8                   = 0x8CE8;
    const int GLColorAttachment9                   = 0x8CE9;
    const int GLColorAttachment10                  = 0x8CEA;
    const int GLColorAttachment11                  = 0x8CEB;
    const int GLColorAttachment12                  = 0x8CEC;
    const int GLColorAttachment13                  = 0x8CED;
    const int GLColorAttachment14                  = 0x8CEE;
    const int GLColorAttachment15                  = 0x8CEF;
    const int GLFramebufferIncompleteMultisample   = 0x8D56;
    const int GLMaxSamples                         = 0x8D57;
    const int GLHalfFloat                          = 0x140B;
    const int GLMapReadBit                         = 0x0001;
    const int GLMapWriteBit                        = 0x0002;
    const int GLMapInvalidateRangeBit              = 0x0004;
    const int GLMapInvalidateBufferBit             = 0x0008;
    const int GLMapFlushExplicitBit                = 0x0010;
    const int GLMapUnsynchronizedBit               = 0x0020;
    const int GLRg                                 = 0x8227;
    const int GLRgInteger                          = 0x8228;
    const int GLR8                                 = 0x8229;
    const int GLRg8                                = 0x822B;

//    const int GLR16F                                          = 0x822D;
//    const int GLR32F                                          = 0x822E;
//    const int GLRg16F                                         = 0x822F;
//    const int GLRg32F                                         = 0x8230;
//    const int GLR8I                                           = 0x8231;
//    const int GLR8UI                                          = 0x8232;
//    const int GLR16I                                          = 0x8233;
//    const int GLR16UI                                         = 0x8234;
//    const int GLR32I                                          = 0x8235;
//    const int GLR32UI                                         = 0x8236;
//    const int GLRg8I                                          = 0x8237;
//    const int GLRg8UI                                         = 0x8238;
//    const int GLRg16I                                         = 0x8239;
//    const int GLRg16UI                                        = 0x823A;
//    const int GLRg32I                                         = 0x823B;
//    const int GLRg32UI                                        = 0x823C;
    const int GLVertexArrayBinding                   = 0x85B5;
    const int GLR8Snorm                              = 0x8F94;
    const int GLRg8Snorm                             = 0x8F95;
    const int GLRGB8Snorm                            = 0x8F96;
    const int GLRGBA8Snorm                           = 0x8F97;
    const int GLSignedNormalized                     = 0x8F9C;
    const int GLPrimitiveRestartFixedIndex           = 0x8D69;
    const int GLCopyReadBuffer                       = 0x8F36;
    const int GLCopyWriteBuffer                      = 0x8F37;
    const int GLCopyReadBufferBinding                = GLCopyReadBuffer;
    const int GLCopyWriteBufferBinding               = GLCopyWriteBuffer;
    const int GLUniformBuffer                        = 0x8A11;
    const int GLUniformBufferBinding                 = 0x8A28;
    const int GLUniformBufferStart                   = 0x8A29;
    const int GLUniformBufferSize                    = 0x8A2A;
    const int GLMaxVertexUniformBlocks               = 0x8A2B;
    const int GLMaxFragmentUniformBlocks             = 0x8A2D;
    const int GLMaxCombinedUniformBlocks             = 0x8A2E;
    const int GLMaxUniformBufferBindings             = 0x8A2F;
    const int GLMaxUniformBlockSize                  = 0x8A30;
    const int GLMaxCombinedVertexUniformComponents   = 0x8A31;
    const int GLMaxCombinedFragmentUniformComponents = 0x8A33;
    const int GLUniformBufferOffsetAlignment         = 0x8A34;
    const int GLActiveUniformBlockMaxNameLength      = 0x8A35;
    const int GLActiveUniformBlocks                  = 0x8A36;
    const int GLUniformType                          = 0x8A37;
    const int GLUniformSize                          = 0x8A38;
    const int GLUniformNameLength                    = 0x8A39;
    const int GLUniformBlockIndex                    = 0x8A3A;
    const int GLUniformOffset                        = 0x8A3B;
    const int GLUniformArrayStride                   = 0x8A3C;
    const int GLUniformMatrixStride                  = 0x8A3D;
    const int GLUniformIsRowMajor                    = 0x8A3E;
    const int GLUniformBlockBinding                  = 0x8A3F;
    const int GLUniformBlockDataSize                 = 0x8A40;
    const int GLUniformBlockNameLength               = 0x8A41;
    const int GLUniformBlockActiveUniforms           = 0x8A42;
    const int GLUniformBlockActiveUniformIndices     = 0x8A43;
    const int GLUniformBlockReferencedByVertexShader = 0x8A44;

    const int GLUniformBlockReferencedByFragmentShader = 0x8A46;

    // GL_INVALID_INDEX is defined as 0xFFFFFFFFu in C.
    const int GLInvalidIndex               = -1;
    const int GLMaxVertexOutputComponents  = 0x9122;
    const int GLMaxFragmentInputComponents = 0x9125;
    const int GLMaxServerWaitTimeout       = 0x9111;
    const int GLObjectType                 = 0x9112;
    const int GLSyncCondition              = 0x9113;
    const int GLSyncStatus                 = 0x9114;
    const int GLSyncFlags                  = 0x9115;
    const int GLSyncFence                  = 0x9116;
    const int GLSyncGpuCommandsComplete    = 0x9117;
    const int GLUnsignaled                 = 0x9118;
    const int GLSignaled                   = 0x9119;
    const int GLAlreadySignaled            = 0x911A;
    const int GLTimeoutExpired             = 0x911B;
    const int GLConditionSatisfied         = 0x911C;
    const int GLWaitFailed                 = 0x911D;

    const int GLSyncFlushCommandsBit = 0x00000001;

    // GL_TIMEOUT_IGNORED is defined as 0xFFFFFFFFFFFFFFFFull in C.
    const long GLTimeoutIgnored               = -1;
    const int  GLVertexAttribArrayDivisor     = 0x88FE;
    const int  GLAnySamplesPassed             = 0x8C2F;
    const int  GLAnySamplesPassedConservative = 0x8D6A;

    const int GLSamplerBinding = 0x8919;

//    const int  GLRGB10A2UI                                = 0x906F;
    const int GLTextureSwizzleR                       = 0x8E42;
    const int GLTextureSwizzleG                       = 0x8E43;
    const int GLTextureSwizzleB                       = 0x8E44;
    const int GLTextureSwizzleA                       = 0x8E45;
    const int GLGreen                                 = 0x1904;
    const int GLBlue                                  = 0x1905;
    const int GLInt2101010Rev                         = 0x8D9F;
    const int GLTransformFeedback                     = 0x8E22;
    const int GLTransformFeedbackPaused               = 0x8E23;
    const int GLTransformFeedbackActive               = 0x8E24;
    const int GLTransformFeedbackBinding              = 0x8E25;
    const int GLProgramBinaryRetrievableHint          = 0x8257;
    const int GLProgramBinaryLength                   = 0x8741;
    const int GLNumProgramBinaryFormats               = 0x87FE;
    const int GLProgramBinaryFormats                  = 0x87FF;
    const int GLCompressedR11Eac                      = 0x9270;
    const int GLCompressedSignedR11Eac                = 0x9271;
    const int GLCompressedRg11Eac                     = 0x9272;
    const int GLCompressedSignedRg11Eac               = 0x9273;
    const int GLCompressedRGB8ETC2                    = 0x9274;
    const int GLCompressedSrgb8ETC2                   = 0x9275;
    const int GLCompressedRGB8PunchthroughAlpha1ETC2  = 0x9276;
    const int GLCompressedSrgb8PunchthroughAlpha1ETC2 = 0x9277;
    const int GLCompressedRGBA8ETC2Eac                = 0x9278;
    const int GLCompressedSrgb8Alpha8ETC2Eac          = 0x9279;
    const int GLTextureImmutableFormat                = 0x912F;
    const int GLMaxElementIndex                       = 0x8D6B;
    const int GLNumSampleCounts                       = 0x9380;
    const int GLTextureImmutableLevels                = 0x82DF;
}

// ========================================================================
// ========================================================================