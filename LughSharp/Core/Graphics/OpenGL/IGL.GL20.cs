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

namespace LughSharp.Core.Graphics.OpenGL;

/// <summary>
/// OpenGL 2.0 functions.
/// </summary>
[SuppressMessage( "ReSharper", "InconsistentNaming" )]
partial interface IGL
{
    const int GLEsVersion20                             = 1;
    const int GLDepthBufferBit                          = 0x00000100;
    const int GLStencilBufferBit                        = 0x00000400;
    const int GLColorBufferBit                          = 0x00004000;
    const int GLFalse                                   = 0;
    const int GLTrue                                    = 1;
    const int GLUnrestricted                            = -1;
    const int GLPoints                                  = 0x0000;
    const int GLLines                                   = 0x0001;
    const int GLLineLoop                                = 0x0002;
    const int GLLineStrip                               = 0x0003;
    const int GLTriangles                               = 0x0004;
    const int GLTriangleStrip                           = 0x0005;
    const int GLTriangleFan                             = 0x0006;
    const int GLZero                                    = 0;
    const int GLOne                                     = 1;
    const int GLSrcColor                                = 0x0300;
    const int GLOneMinusSrcColor                        = 0x0301;
    const int GLSrcAlpha                                = 0x0302;
    const int GLOneMinusSrcAlpha                        = 0x0303;
    const int GLDstAlpha                                = 0x0304;
    const int GLOneMinusDstAlpha                        = 0x0305;
    const int GLDstColor                                = 0x0306;
    const int GLOneMinusDstColor                        = 0x0307;
    const int GLSrcAlphaSaturate                        = 0x0308;
    const int GLFuncAdd                                 = 0x8006;
    const int GLBlendEquation                           = 0x8009;
    const int GLBlendEquationRGB                        = 0x8009;
    const int GLBlendEquationAlpha                      = 0x883D;
    const int GLFuncSubtract                            = 0x800A;
    const int GLFuncReverseSubtract                     = 0x800B;
    const int GLBlendDstRGB                             = 0x80C8;
    const int GLBlendSrcRGB                             = 0x80C9;
    const int GLBlendDstAlpha                           = 0x80CA;
    const int GLBlendSrcAlpha                           = 0x80CB;
    const int GLConstantColor                           = 0x8001;
    const int GLOneMinusConstantColor                   = 0x8002;
    const int GLConstantAlpha                           = 0x8003;
    const int GLOneMinusConstantAlpha                   = 0x8004;
    const int GLBlendColor                              = 0x8005;
    const int GLArrayBuffer                             = 0x8892;
    const int GLElementArrayBuffer                      = 0x8893;
    const int GLArrayBufferBinding                      = 0x8894;
    const int GLElementArrayBufferBinding               = 0x8895;
    const int GLStreamDraw                              = 0x88E0;
    const int GLStaticDraw                              = 0x88E4;
    const int GLDynamicDraw                             = 0x88E8;
    const int GLBufferSize                              = 0x8764;
    const int GLBufferUsage                             = 0x8765;
    const int GLCurrentVertexAttrib                     = 0x8626;
    const int GLFront                                   = 0x0404;
    const int GLBack                                    = 0x0405;
    const int GLFrontAndBack                            = 0x0408;
    const int GLTexture2D                               = 0x0DE1;
    const int GLCullFace                                = 0x0B44;
    const int GLBlend                                   = 0x0BE2;
    const int GLDither                                  = 0x0BD0;
    const int GLStencilTest                             = 0x0B90;
    const int GLDepthTest                               = 0x0B71;
    const int GLScissorTest                             = 0x0C11;
    const int GLPolygonOffsetFill                       = 0x8037;
    const int GLSampleAlphaToCoverage                   = 0x809E;
    const int GLSampleCoverage                          = 0x80A0;
    const int GLNoError                                 = 0;
    const int GLInvalidEnum                             = 0x0500;
    const int GLInvalidValue                            = 0x0501;
    const int GLInvalidOperation                        = 0x0502;
    const int GLOutOfMemory                             = 0x0505;
    const int GLCw                                      = 0x0900;
    const int GLCcw                                     = 0x0901;
    const int GLLineWidth                               = 0x0B21;
    const int GLAliasedPointSizeRange                   = 0x846D;
    const int GLAliasedLineWidthRange                   = 0x846E;
    const int GLCullFaceMode                            = 0x0B45;
    const int GLFrontFace                               = 0x0B46;
    const int GLDepthRange                              = 0x0B70;
    const int GLDepthWritemask                          = 0x0B72;
    const int GLDepthClearValue                         = 0x0B73;
    const int GLDepthFunc                               = 0x0B74;
    const int GLStencilClearValue                       = 0x0B91;
    const int GLStencilFunc                             = 0x0B92;
    const int GLStencilFail                             = 0x0B94;
    const int GLStencilPassDepthFail                    = 0x0B95;
    const int GLStencilPassDepthPass                    = 0x0B96;
    const int GLStencilRef                              = 0x0B97;
    const int GLStencilValueMask                        = 0x0B93;
    const int GLStencilWritemask                        = 0x0B98;
    const int GLStencilBackFunc                         = 0x8800;
    const int GLStencilBackFail                         = 0x8801;
    const int GLStencilBackPassDepthFail                = 0x8802;
    const int GLStencilBackPassDepthPass                = 0x8803;
    const int GLStencilBackRef                          = 0x8CA3;
    const int GLStencilBackValueMask                    = 0x8CA4;
    const int GLStencilBackWritemask                    = 0x8CA5;
    const int GLViewport                                = 0x0BA2;
    const int GLScissorBox                              = 0x0C10;
    const int GLColorClearValue                         = 0x0C22;
    const int GLColorWritemask                          = 0x0C23;
    const int GLUnpackAlignment                         = 0x0CF5;
    const int GLPackAlignment                           = 0x0D05;
    const int GLMaxTextureSize                          = 0x0D33;
    const int GLMaxTextureUnits                         = 0x84E2;
    const int GLMaxViewportDims                         = 0x0D3A;
    const int GLSubpixelBits                            = 0x0D50;
    const int GLRedBits                                 = 0x0D52;
    const int GLGreenBits                               = 0x0D53;
    const int GLBlueBits                                = 0x0D54;
    const int GLAlphaBits                               = 0x0D55;
    const int GLDepthBits                               = 0x0D56;
    const int GLStencilBits                             = 0x0D57;
    const int GLPolygonOffsetUnits                      = 0x2A00;
    const int GLPolygonOffsetFactor                     = 0x8038;
    const int GLTextureBinding2D                        = 0x8069;
    const int GLSampleBuffers                           = 0x80A8;
    const int GLSamples                                 = 0x80A9;
    const int GLSampleCoverageValue                     = 0x80AA;
    const int GLSampleCoverageInvert                    = 0x80AB;
    const int GLNumCompressedTextureFormats             = 0x86A2;
    const int GLCompressedTextureFormats                = 0x86A3;
    const int GLDontCare                                = 0x1100;
    const int GLFastest                                 = 0x1101;
    const int GLNicest                                  = 0x1102;
    const int GLGenerateMipmap                          = 0x8191;
    const int GLGenerateMipmapHint                      = 0x8192;
    const int GLByte                                    = 0x1400;
    const int GLUnsignedByte                            = 0x1401;
    const int GLShort                                   = 0x1402;
    const int GLUnsignedShort                           = 0x1403;
    const int GLInt                                     = 0x1404;
    const int GLUnsignedInt                             = 0x1405;
    const int GLFloat                                   = 0x1406;
    const int GLFixed                                   = 0x140C;
    const int GLColorIndex                              = 0x1900;
    const int GLDepthComponent                          = 0x1902;
    const int GLAlpha                                   = 0x1906;
    const int GLRGB                                     = 0x1907;
    const int GLRGBA                                    = 0x1908;
    const int GLLuminance                               = 0x1909;
    const int GLLuminanceAlpha                          = 0x190A;
    const int GLUnsignedShort4444                       = 0x8033;
    const int GLUnsignedShort5551                       = 0x8034;
    const int GLUnsignedShort565                        = 0x8363;
    const int GLFragmentShader                          = 0x8B30;
    const int GLVertexShader                            = 0x8B31;
    const int GLMaxVertexAttribs                        = 0x8869;
    const int GLMaxVertexUniformVectors                 = 0x8DFB;
    const int GLMaxVaryingVectors                       = 0x8DFC;
    const int GLMaxCombinedTextureImageUnits            = 0x8B4D;
    const int GLMaxVertexTextureImageUnits              = 0x8B4C;
    const int GLMaxTextureImageUnits                    = 0x8872;
    const int GLMaxFragmentUniformVectors               = 0x8DFD;
    const int GLShaderType                              = 0x8B4F;
    const int GLDeleteStatus                            = 0x8B80;
    const int GLLinkStatus                              = 0x8B82;
    const int GLValidateStatus                          = 0x8B83;
    const int GLAttachedShaders                         = 0x8B85;
    const int GLActiveUniforms                          = 0x8B86;
    const int GLActiveUniformMaxLength                  = 0x8B87;
    const int GLActiveAttributes                        = 0x8B89;
    const int GLActiveAttributeMaxLength                = 0x8B8A;
    const int GLShadingLanguageVersion                  = 0x8B8C;
    const int GLCurrentProgram                          = 0x8B8D;
    const int GLNever                                   = 0x0200;
    const int GLLess                                    = 0x0201;
    const int GLEqual                                   = 0x0202;
    const int GLLequal                                  = 0x0203;
    const int GLGreater                                 = 0x0204;
    const int GLNotequal                                = 0x0205;
    const int GLGequal                                  = 0x0206;
    const int GLAlways                                  = 0x0207;
    const int GLKeep                                    = 0x1E00;
    const int GLReplace                                 = 0x1E01;
    const int GLIncr                                    = 0x1E02;
    const int GLDecr                                    = 0x1E03;
    const int GLInvert                                  = 0x150A;
    const int GLIncrWrap                                = 0x8507;
    const int GLDecrWrap                                = 0x8508;
    const int GLVendor                                  = 0x1F00;
    const int GLRenderer                                = 0x1F01;
    const int GLVersion                                 = 0x1F02;
    const int GLExtensions                              = 0x1F03;
    const int GLNearest                                 = 0x2600;
    const int GLLinear                                  = 0x2601;
    const int GLNearestMipmapNearest                    = 0x2700;
    const int GLLinearMipmapNearest                     = 0x2701;
    const int GLNearestMipmapLinear                     = 0x2702;
    const int GLLinearMipmapLinear                      = 0x2703;
    const int GLTextureMagFilter                        = 0x2800;
    const int GLTextureMinFilter                        = 0x2801;
    const int GLTextureWrapS                            = 0x2802;
    const int GLTextureWrapT                            = 0x2803;
    const int GLTexture                                 = 0x1702;
    const int GLTextureCubeMap                          = 0x8513;
    const int GLTextureBindingCubeMap                   = 0x8514;
    const int GLTextureCubeMapPositiveX                 = 0x8515;
    const int GLTextureCubeMapNegativeX                 = 0x8516;
    const int GLTextureCubeMapPositiveY                 = 0x8517;
    const int GLTextureCubeMapNegativeY                 = 0x8518;
    const int GLTextureCubeMapPositiveZ                 = 0x8519;
    const int GLTextureCubeMapNegativeZ                 = 0x851A;
    const int GLMaxCubeMapTextureSize                   = 0x851C;
    const int GLTexture0                                = 0x84C0;
    const int GLTexture1                                = 0x84C1;
    const int GLTexture2                                = 0x84C2;
    const int GLTexture3                                = 0x84C3;
    const int GLTexture4                                = 0x84C4;
    const int GLTexture5                                = 0x84C5;
    const int GLTexture6                                = 0x84C6;
    const int GLTexture7                                = 0x84C7;
    const int GLTexture8                                = 0x84C8;
    const int GLTexture9                                = 0x84C9;
    const int GLTexture10                               = 0x84CA;
    const int GLTexture11                               = 0x84CB;
    const int GLTexture12                               = 0x84CC;
    const int GLTexture13                               = 0x84CD;
    const int GLTexture14                               = 0x84CE;
    const int GLTexture15                               = 0x84CF;
    const int GLTexture16                               = 0x84D0;
    const int GLTexture17                               = 0x84D1;
    const int GLTexture18                               = 0x84D2;
    const int GLTexture19                               = 0x84D3;
    const int GLTexture20                               = 0x84D4;
    const int GLTexture21                               = 0x84D5;
    const int GLTexture22                               = 0x84D6;
    const int GLTexture23                               = 0x84D7;
    const int GLTexture24                               = 0x84D8;
    const int GLTexture25                               = 0x84D9;
    const int GLTexture26                               = 0x84DA;
    const int GLTexture27                               = 0x84DB;
    const int GLTexture28                               = 0x84DC;
    const int GLTexture29                               = 0x84DD;
    const int GLTexture30                               = 0x84DE;
    const int GLTexture31                               = 0x84DF;
    const int GLActiveTexture                           = 0x84E0;
    const int GLRepeat                                  = 0x2901;
    const int GLClampToEdge                             = 0x812F;
    const int GLMirroredRepeat                          = 0x8370;
    const int GLFloatVec2                               = 0x8B50;
    const int GLFloatVec3                               = 0x8B51;
    const int GLFloatVec4                               = 0x8B52;
    const int GLIntVec2                                 = 0x8B53;
    const int GLIntVec3                                 = 0x8B54;
    const int GLIntVec4                                 = 0x8B55;
    const int GLBool                                    = 0x8B56;
    const int GLBoolVec2                                = 0x8B57;
    const int GLBoolVec3                                = 0x8B58;
    const int GLBoolVec4                                = 0x8B59;
    const int GLFloatMat2                               = 0x8B5A;
    const int GLFloatMat3                               = 0x8B5B;
    const int GLFloatMat4                               = 0x8B5C;
    const int GLSampler2D                               = 0x8B5E;
    const int GLSamplerCube                             = 0x8B60;
    const int GLVertexAttribArrayEnabled                = 0x8622;
    const int GLVertexAttribArraySize                   = 0x8623;
    const int GLVertexAttribArrayStride                 = 0x8624;
    const int GLVertexAttribArrayType                   = 0x8625;
    const int GLVertexAttribArrayNormalized             = 0x886A;
    const int GLVertexAttribArrayPointer                = 0x8645;
    const int GLVertexAttribArrayBufferBinding          = 0x889F;
    const int GLImplementationColorReadType             = 0x8B9A;
    const int GLImplementationColorReadFormat           = 0x8B9B;
    const int GLCompileStatus                           = 0x8B81;
    const int GLInfoLogLength                           = 0x8B84;
    const int GLShaderSourceLength                      = 0x8B88;
    const int GLShaderCompiler                          = 0x8DFA;
    const int GLShaderBinaryFormats                     = 0x8DF8;
    const int GLNumShaderBinaryFormats                  = 0x8DF9;
    const int GLLowFloat                                = 0x8DF0;
    const int GLMediumFloat                             = 0x8DF1;
    const int GLHighFloat                               = 0x8DF2;
    const int GLLowInt                                  = 0x8DF3;
    const int GLMediumInt                               = 0x8DF4;
    const int GLHighInt                                 = 0x8DF5;
    const int GLFramebuffer                             = 0x8D40;
    const int GLRenderbuffer                            = 0x8D41;
    const int GLRGBA4                                   = 0x8056;
    const int GLRGB5A1                                  = 0x8057;
    const int GLRGB565                                  = 0x8D62;
    const int GLDepthComponent16                        = 0x81A5;
    const int GLStencilIndex                            = 0x1901;
    const int GLStencilIndex8                           = 0x8D48;
    const int GLRenderbufferWidth                       = 0x8D42;
    const int GLRenderbufferHeight                      = 0x8D43;
    const int GLRenderbufferInternalFormat              = 0x8D44;
    const int GLRenderbufferRedSize                     = 0x8D50;
    const int GLRenderbufferGreenSize                   = 0x8D51;
    const int GLRenderbufferBlueSize                    = 0x8D52;
    const int GLRenderbufferAlphaSize                   = 0x8D53;
    const int GLRenderbufferDepthSize                   = 0x8D54;
    const int GLRenderbufferStencilSize                 = 0x8D55;
    const int GLFramebufferAttachmentObjectType         = 0x8CD0;
    const int GLFramebufferAttachmentObjectName         = 0x8CD1;
    const int GLFramebufferAttachmentTextureLevel       = 0x8CD2;
    const int GLFramebufferAttachmentTextureCubeMapFace = 0x8CD3;
    const int GLColorAttachment0                        = 0x8CE0;
    const int GLDepthAttachment                         = 0x8D00;
    const int GLStencilAttachment                       = 0x8D20;
    const int GLNone                                    = 0;
    const int GLFramebufferComplete                     = 0x8CD5;
    const int GLFramebufferIncompleteAttachment         = 0x8CD6;
    const int GLFramebufferIncompleteMissingAttachment  = 0x8CD7;
    const int GLFramebufferIncompleteDimensions         = 0x8CD9;
    const int GLFramebufferUnsupported                  = 0x8CDD;
    const int GLFramebufferBinding                      = 0x8CA6;
    const int GLRenderbufferBinding                     = 0x8CA7;
    const int GLMaxRenderbufferSize                     = 0x84E8;
    const int GLInvalidFramebufferOperation             = 0x0506;
    const int GLVertexProgramPointSize                  = 0x8642;

    // Extensions
    const int GLCoverageBufferBitNv        = 0x8000;
    const int GLTextureMaxAnisotropyExt    = 0x84FE;
    const int GLMaxTextureMaxAnisotropyExt = 0x84FF;
}

// ========================================================================
// ========================================================================