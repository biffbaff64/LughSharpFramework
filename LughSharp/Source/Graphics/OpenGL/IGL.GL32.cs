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

namespace LughSharp.Source.Graphics.OpenGL;

/// <summary>
/// OpenGL 3.2 functions.
/// </summary>
//[SuppressMessage( "ReSharper", "InconsistentNaming" )]
partial interface IGL
{
    const int GLContextFlagDebugBit                        = 0x00000002;
    const int GLContextFlagRobustAccessBit                 = 0x00000004;
    const int GLGeometryShaderBit                          = 0x00000004;
    const int GLTessControlShaderBit                       = 0x00000008;
    const int GLTessEvaluationShaderBit                    = 0x00000010;
    const int GLQuads                                      = 0x0007;
    const int GLQuadStrip                                  = 0x0008;
    const int GLPolygon                                    = 0x0009;
    const int GLLinesAdjacency                             = 0x000A;
    const int GLLineStripAdjacency                         = 0x000B;
    const int GLTrianglesAdjacency                         = 0x000C;
    const int GLTriangleStripAdjacency                     = 0x000D;
    const int GLPatches                                    = 0x000E;
    const int GLStackOverflow                              = 0x0503;
    const int GLStackUnderflow                             = 0x0504;
    const int GLContextLost                                = 0x0507;
    const int GLTextureBorderColor                         = 0x1004;
    const int GLVertexArray                                = 0x8074;
    const int GLClampToBorder                              = 0x812D;
    const int GLContextFlags                               = 0x821E;
    const int GLPrimitiveRestartForPatchesSupported        = 0x8221;
    const int GLDebugOutputSynchronous                     = 0x8242;
    const int GLDebugNextLoggedMessageLength               = 0x8243;
    const int GLDebugCallbackFunction                      = 0x8244;
    const int GLDebugCallbackUserParam                     = 0x8245;
    const int GLDebugSourceApi                             = 0x8246;
    const int GLDebugSourceWindowSystem                    = 0x8247;
    const int GLDebugSourceShaderCompiler                  = 0x8248;
    const int GLDebugSourceThirdParty                      = 0x8249;
    const int GLDebugSourceApplication                     = 0x824A;
    const int GLDebugSourceOther                           = 0x824B;
    const int GLDebugTypeError                             = 0x824C;
    const int GLDebugTypeDeprecatedBehavior                = 0x824D;
    const int GLDebugTypeUndefinedBehavior                 = 0x824E;
    const int GLDebugTypePortability                       = 0x824F;
    const int GLDebugTypePerformance                       = 0x8250;
    const int GLDebugTypeOther                             = 0x8251;
    const int GLLoseContextOnReset                         = 0x8252;
    const int GLGuiltyContextReset                         = 0x8253;
    const int GLInnocentContextReset                       = 0x8254;
    const int GLUnknownContextReset                        = 0x8255;
    const int GLResetNotificationStrategy                  = 0x8256;
    const int GLLayerProvokingVertex                       = 0x825E;
    const int GLUndefinedVertex                            = 0x8260;
    const int GLNoResetNotification                        = 0x8261;
    const int GLDebugTypeMarker                            = 0x8268;
    const int GLDebugTypePushGroup                         = 0x8269;
    const int GLDebugTypePopGroup                          = 0x826A;
    const int GLDebugSeverityNotification                  = 0x826B;
    const int GLMaxDebugGroupStackDepth                    = 0x826C;
    const int GLDebugGroupStackDepth                       = 0x826D;
    const int GLBuffer                                     = 0x82E0;
    const int GLShader                                     = 0x82E1;
    const int GLProgram                                    = 0x82E2;
    const int GLQuery                                      = 0x82E3;
    const int GLProgramPipeline                            = 0x82E4;
    const int GLSampler                                    = 0x82E6;
    const int GLMaxLabelLength                             = 0x82E8;
    const int GLMaxTessControlInputComponents              = 0x886C;
    const int GLMaxTessEvaluationInputComponents           = 0x886D;
    const int GLGeometryShaderInvocations                  = 0x887F;
    const int GLGeometryVerticesOut                        = 0x8916;
    const int GLGeometryInputType                          = 0x8917;
    const int GLGeometryOutputType                         = 0x8918;
    const int GLMaxGeometryUniformBlocks                   = 0x8A2C;
    const int GLMaxCombinedGeometryUniformComponents       = 0x8A32;
    const int GLMaxGeometryTextureImageUnits               = 0x8C29;
    const int GLTextureBuffer                              = 0x8C2A;
    const int GLTextureBufferBinding                       = 0x8C2A;
    const int GLMaxTextureBufferSize                       = 0x8C2B;
    const int GLTextureBindingBuffer                       = 0x8C2C;
    const int GLTextureBufferDataStoreBinding              = 0x8C2D;
    const int GLSampleShading                              = 0x8C36;
    const int GLMinSampleShadingValue                      = 0x8C37;
    const int GLPrimitivesGenerated                        = 0x8C87;
    const int GLFramebufferAttachmentLayered               = 0x8DA7;
    const int GLFramebufferIncompleteLayerTargets          = 0x8DA8;
    const int GLSamplerBuffer                              = 0x8DC2;
    const int GLIntSamplerBuffer                           = 0x8DD0;
    const int GLUnsignedIntSamplerBuffer                   = 0x8DD8;
    const int GLGeometryShader                             = 0x8DD9;
    const int GLMaxGeometryUniformComponents               = 0x8DDF;
    const int GLMaxGeometryOutputVertices                  = 0x8DE0;
    const int GLMaxGeometryTotalOutputComponents           = 0x8DE1;
    const int GLMaxCombinedTessControlUniformComponents    = 0x8E1E;
    const int GLMaxCombinedTessEvaluationUniformComponents = 0x8E1F;
    const int GLFirstVertexConvention                      = 0x8E4D;
    const int GLLastVertexConvention                       = 0x8E4E;
    const int GLMaxGeometryShaderInvocations               = 0x8E5A;
    const int GLMinFragmentInterpolationOffset             = 0x8E5B;
    const int GLMaxFragmentInterpolationOffset             = 0x8E5C;
    const int GLFragmentInterpolationOffsetBits            = 0x8E5D;
    const int GLPatchVertices                              = 0x8E72;
    const int GLTessControlOutputVertices                  = 0x8E75;
    const int GLTessGenMode                                = 0x8E76;
    const int GLTessGenSpacing                             = 0x8E77;
    const int GLTessGenVertexOrder                         = 0x8E78;
    const int GLTessGenPointMode                           = 0x8E79;
    const int GLIsolines                                   = 0x8E7A;
    const int GLFractionalOdd                              = 0x8E7B;
    const int GLFractionalEven                             = 0x8E7C;
    const int GLMaxPatchVertices                           = 0x8E7D;
    const int GLMaxTessGenLevel                            = 0x8E7E;
    const int GLMaxTessControlUniformComponents            = 0x8E7F;
    const int GLMaxTessEvaluationUniformComponents         = 0x8E80;
    const int GLMaxTessControlTextureImageUnits            = 0x8E81;
    const int GLMaxTessEvaluationTextureImageUnits         = 0x8E82;
    const int GLMaxTessControlOutputComponents             = 0x8E83;
    const int GLMaxTessPatchComponents                     = 0x8E84;
    const int GLMaxTessControlTotalOutputComponents        = 0x8E85;
    const int GLMaxTessEvaluationOutputComponents          = 0x8E86;
    const int GLTessEvaluationShader                       = 0x8E87;
    const int GLTessControlShader                          = 0x8E88;
    const int GLMaxTessControlUniformBlocks                = 0x8E89;
    const int GLMaxTessEvaluationUniformBlocks             = 0x8E8A;
    const int GLTextureCubeMapArray                        = 0x9009;
    const int GLTextureBindingCubeMapArray                 = 0x900A;
    const int GLSamplerCubeMapArray                        = 0x900C;
    const int GLSamplerCubeMapArrayShadow                  = 0x900D;
    const int GLIntSamplerCubeMapArray                     = 0x900E;
    const int GLUnsignedIntSamplerCubeMapArray             = 0x900F;
    const int GLImageBuffer                                = 0x9051;
    const int GLImageCubeMapArray                          = 0x9054;
    const int GLIntImageBuffer                             = 0x905C;
    const int GLIntImageCubeMapArray                       = 0x905F;
    const int GLUnsignedIntImageBuffer                     = 0x9067;
    const int GLUnsignedIntImageCubeMapArray               = 0x906A;
    const int GLMaxTessControlImageUniforms                = 0x90CB;
    const int GLMaxTessEvaluationImageUniforms             = 0x90CC;
    const int GLMaxGeometryImageUniforms                   = 0x90CD;
    const int GLMaxGeometryShaderStorageBlocks             = 0x90D7;
    const int GLMaxTessControlShaderStorageBlocks          = 0x90D8;
    const int GLMaxTessEvaluationShaderStorageBlocks       = 0x90D9;
    const int GLTexture2DMultisampleArray                  = 0x9102;
    const int GLTextureBinding2DMultisampleArray           = 0x9105;
    const int GLSampler2DMultisampleArray                  = 0x910B;
    const int GLIntSampler2DMultisampleArray               = 0x910C;
    const int GLUnsignedIntSampler2DMultisampleArray       = 0x910D;
    const int GLMaxGeometryInputComponents                 = 0x9123;
    const int GLMaxGeometryOutputComponents                = 0x9124;
    const int GLMaxDebugMessageLength                      = 0x9143;
    const int GLMaxDebugLoggedMessages                     = 0x9144;
    const int GLDebugLoggedMessages                        = 0x9145;
    const int GLDebugSeverityHigh                          = 0x9146;
    const int GLDebugSeverityMedium                        = 0x9147;
    const int GLDebugSeverityLow                           = 0x9148;
    const int GLTextureBufferOffset                        = 0x919D;
    const int GLTextureBufferSize                          = 0x919E;
    const int GLTextureBufferOffsetAlignment               = 0x919F;
    const int GLMultiply                                   = 0x9294;
    const int GLScreen                                     = 0x9295;
    const int GLOverlay                                    = 0x9296;
    const int GLDarken                                     = 0x9297;
    const int GLLighten                                    = 0x9298;
    const int GLColordodge                                 = 0x9299;
    const int GLColorburn                                  = 0x929A;
    const int GLHardlight                                  = 0x929B;
    const int GLSoftlight                                  = 0x929C;
    const int GLDifference                                 = 0x929E;
    const int GLExclusion                                  = 0x92A0;
    const int GLHslHue                                     = 0x92AD;
    const int GLHslSaturation                              = 0x92AE;
    const int GLHslColor                                   = 0x92AF;
    const int GLHslLuminosity                              = 0x92B0;
    const int GLPrimitiveBoundingBox                       = 0x92BE;
    const int GLMaxTessControlAtomicCounterBuffers         = 0x92CD;
    const int GLMaxTessEvaluationAtomicCounterBuffers      = 0x92CE;
    const int GLMaxGeometryAtomicCounterBuffers            = 0x92CF;
    const int GLMaxTessControlAtomicCounters               = 0x92D3;
    const int GLMaxTessEvaluationAtomicCounters            = 0x92D4;
    const int GLMaxGeometryAtomicCounters                  = 0x92D5;
    const int GLDebugOutput                                = 0x92E0;
    const int GLIsPerPatch                                 = 0x92E7;
    const int GLReferencedByTessControlShader              = 0x9307;
    const int GLReferencedByTessEvaluationShader           = 0x9308;
    const int GLReferencedByGeometryShader                 = 0x9309;
    const int GLFramebufferDefaultLayers                   = 0x9312;
    const int GLMaxFramebufferLayers                       = 0x9317;
    const int GLMultisampleLineWidthRange                  = 0x9381;
    const int GLMultisampleLineWidthGranularity            = 0x9382;
    const int GLCompressedRGBAAstc4X4                      = 0x93B0;
    const int GLCompressedRGBAAstc5X4                      = 0x93B1;
    const int GLCompressedRGBAAstc5X5                      = 0x93B2;
    const int GLCompressedRGBAAstc6X5                      = 0x93B3;
    const int GLCompressedRGBAAstc6X6                      = 0x93B4;
    const int GLCompressedRGBAAstc8X5                      = 0x93B5;
    const int GLCompressedRGBAAstc8X6                      = 0x93B6;
    const int GLCompressedRGBAAstc8X8                      = 0x93B7;
    const int GLCompressedRGBAAstc10X5                     = 0x93B8;
    const int GLCompressedRGBAAstc10X6                     = 0x93B9;
    const int GLCompressedRGBAAstc10X8                     = 0x93BA;
    const int GLCompressedRGBAAstc10X10                    = 0x93BB;
    const int GLCompressedRGBAAstc12X10                    = 0x93BC;
    const int GLCompressedRGBAAstc12X12                    = 0x93BD;
    const int GLCompressedSrgb8Alpha8Astc4X4               = 0x93D0;
    const int GLCompressedSrgb8Alpha8Astc5X4               = 0x93D1;
    const int GLCompressedSrgb8Alpha8Astc5X5               = 0x93D2;
    const int GLCompressedSrgb8Alpha8Astc6X5               = 0x93D3;
    const int GLCompressedSrgb8Alpha8Astc6X6               = 0x93D4;
    const int GLCompressedSrgb8Alpha8Astc8X5               = 0x93D5;
    const int GLCompressedSrgb8Alpha8Astc8X6               = 0x93D6;
    const int GLCompressedSrgb8Alpha8Astc8X8               = 0x93D7;
    const int GLCompressedSrgb8Alpha8Astc10X5              = 0x93D8;
    const int GLCompressedSrgb8Alpha8Astc10X6              = 0x93D9;
    const int GLCompressedSrgb8Alpha8Astc10X8              = 0x93DA;
    const int GLCompressedSrgb8Alpha8Astc10X10             = 0x93DB;
    const int GLCompressedSrgb8Alpha8Astc12X10             = 0x93DC;
    const int GLCompressedSrgb8Alpha8Astc12X12             = 0x93DD;
}

// ========================================================================
// ========================================================================
