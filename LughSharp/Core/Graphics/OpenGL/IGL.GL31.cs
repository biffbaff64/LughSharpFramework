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
/// OpenGL 3.1 functions.
/// </summary>
[SuppressMessage( "ReSharper", "InconsistentNaming" )]
partial interface IGL
{
    const int GLVertexShaderBit                        = 0x00000001;
    const int GLFragmentShaderBit                      = 0x00000002;
    const int GLComputeShaderBit                       = 0x00000020;
    const int GLAllShaderBits                          = -1; // 0xFFFFFFFF
    const int GLVertexAttribArrayBarrierBit            = 0x00000001;
    const int GLElementArrayBarrierBit                 = 0x00000002;
    const int GLUniformBarrierBit                      = 0x00000004;
    const int GLTextureFetchBarrierBit                 = 0x00000008;
    const int GLShaderImageAccessBarrierBit            = 0x00000020;
    const int GLCommandBarrierBit                      = 0x00000040;
    const int GLPixelBufferBarrierBit                  = 0x00000080;
    const int GLTextureUpdateBarrierBit                = 0x00000100;
    const int GLBufferUpdateBarrierBit                 = 0x00000200;
    const int GLFramebufferBarrierBit                  = 0x00000400;
    const int GLTransformFeedbackBarrierBit            = 0x00000800;
    const int GLAtomicCounterBarrierBit                = 0x00001000;
    const int GLShaderStorageBarrierBit                = 0x00002000;
    const int GLAllBarrierBits                         = -1; // 0xFFFFFFFF
    const int GLTextureWidth                           = 0x1000;
    const int GLTextureHeight                          = 0x1001;
    const int GLTextureInternalFormat                  = 0x1003;
    const int GLTextureRedSize                         = 0x805C;
    const int GLTextureGreenSize                       = 0x805D;
    const int GLTextureBlueSize                        = 0x805E;
    const int GLTextureAlphaSize                       = 0x805F;
    const int GLTextureDepth                           = 0x8071;
    const int GLProgramSeparable                       = 0x8258;
    const int GLActiveProgram                          = 0x8259;
    const int GLProgramPipelineBinding                 = 0x825A;
    const int GLMaxComputeSharedMemorySize             = 0x8262;
    const int GLMaxComputeUniformComponents            = 0x8263;
    const int GLMaxComputeAtomicCounterBuffers         = 0x8264;
    const int GLMaxComputeAtomicCounters               = 0x8265;
    const int GLMaxCombinedComputeUniformComponents    = 0x8266;
    const int GLComputeWorkGroupSize                   = 0x8267;
    const int GLMaxUniformLocations                    = 0x826E;
    const int GLVertexAttribBinding                    = 0x82D4;
    const int GLVertexAttribRelativeOffset             = 0x82D5;
    const int GLVertexBindingDivisor                   = 0x82D6;
    const int GLVertexBindingOffset                    = 0x82D7;
    const int GLVertexBindingStride                    = 0x82D8;
    const int GLMaxVertexAttribRelativeOffset          = 0x82D9;
    const int GLMaxVertexAttribBindings                = 0x82DA;
    const int GLMaxVertexAttribStride                  = 0x82E5;
    const int GLTextureCompressed                      = 0x86A1;
    const int GLTextureDepthSize                       = 0x884A;
    const int GLReadOnly                               = 0x88B8;
    const int GLWriteOnly                              = 0x88B9;
    const int GLReadWrite                              = 0x88BA;
    const int GLTextureStencilSize                     = 0x88F1;
    const int GLTextureRedType                         = 0x8C10;
    const int GLTextureGreenType                       = 0x8C11;
    const int GLTextureBlueType                        = 0x8C12;
    const int GLTextureAlphaType                       = 0x8C13;
    const int GLTextureDepthType                       = 0x8C16;
    const int GLTextureSharedSize                      = 0x8C3F;
    const int GLSamplePosition                         = 0x8E50;
    const int GLSampleMask                             = 0x8E51;
    const int GLSampleMaskValue                        = 0x8E52;
    const int GLMaxSampleMaskWords                     = 0x8E59;
    const int GLMinProgramTextureGatherOffset          = 0x8E5E;
    const int GLMaxProgramTextureGatherOffset          = 0x8E5F;
    const int GLMaxImageUnits                          = 0x8F38;
    const int GLMaxCombinedShaderOutputResources       = 0x8F39;
    const int GLImageBindingName                       = 0x8F3A;
    const int GLImageBindingLevel                      = 0x8F3B;
    const int GLImageBindingLayered                    = 0x8F3C;
    const int GLImageBindingLayer                      = 0x8F3D;
    const int GLImageBindingAccess                     = 0x8F3E;
    const int GLDrawIndirectBuffer                     = 0x8F3F;
    const int GLDrawIndirectBufferBinding              = 0x8F43;
    const int GLVertexBindingBuffer                    = 0x8F4F;
    const int GLImage2D                                = 0x904D;
    const int GLImage3D                                = 0x904E;
    const int GLImageCube                              = 0x9050;
    const int GLImage2DArray                           = 0x9053;
    const int GLIntImage2D                             = 0x9058;
    const int GLIntImage3D                             = 0x9059;
    const int GLIntImageCube                           = 0x905B;
    const int GLIntImage2DArray                        = 0x905E;
    const int GLUnsignedIntImage2D                     = 0x9063;
    const int GLUnsignedIntImage3D                     = 0x9064;
    const int GLUnsignedIntImageCube                   = 0x9066;
    const int GLUnsignedIntImage2DArray                = 0x9069;
    const int GLImageBindingFormat                     = 0x906E;
    const int GLImageFormatCompatibilityType           = 0x90C7;
    const int GLImageFormatCompatibilityBySize         = 0x90C8;
    const int GLImageFormatCompatibilityByClass        = 0x90C9;
    const int GLMaxVertexImageUniforms                 = 0x90CA;
    const int GLMaxFragmentImageUniforms               = 0x90CE;
    const int GLMaxCombinedImageUniforms               = 0x90CF;
    const int GLShaderStorageBuffer                    = 0x90D2;
    const int GLShaderStorageBufferBinding             = 0x90D3;
    const int GLShaderStorageBufferStart               = 0x90D4;
    const int GLShaderStorageBufferSize                = 0x90D5;
    const int GLMaxVertexShaderStorageBlocks           = 0x90D6;
    const int GLMaxFragmentShaderStorageBlocks         = 0x90DA;
    const int GLMaxComputeShaderStorageBlocks          = 0x90DB;
    const int GLMaxCombinedShaderStorageBlocks         = 0x90DC;
    const int GLMaxShaderStorageBufferBindings         = 0x90DD;
    const int GLMaxShaderStorageBlockSize              = 0x90DE;
    const int GLShaderStorageBufferOffsetAlignment     = 0x90DF;
    const int GLDepthStencilTextureMode                = 0x90EA;
    const int GLMaxComputeWorkGroupInvocations         = 0x90EB;
    const int GLDispatchIndirectBuffer                 = 0x90EE;
    const int GLDispatchIndirectBufferBinding          = 0x90EF;
    const int GLTexture2DMultisample                   = 0x9100;
    const int GLTextureBinding2DMultisample            = 0x9104;
    const int GLTextureSamples                         = 0x9106;
    const int GLTextureFixedSampleLocations            = 0x9107;
    const int GLSampler2DMultisample                   = 0x9108;
    const int GLIntSampler2DMultisample                = 0x9109;
    const int GLUnsignedIntSampler2DMultisample        = 0x910A;
    const int GLMaxColorTextureSamples                 = 0x910E;
    const int GLMaxDepthTextureSamples                 = 0x910F;
    const int GLMaxIntegerSamples                      = 0x9110;
    const int GLComputeShader                          = 0x91B9;
    const int GLMaxComputeUniformBlocks                = 0x91BB;
    const int GLMaxComputeTextureImageUnits            = 0x91BC;
    const int GLMaxComputeImageUniforms                = 0x91BD;
    const int GLMaxComputeWorkGroupCount               = 0x91BE;
    const int GLMaxComputeWorkGroupSize                = 0x91BF;
    const int GLAtomicCounterBuffer                    = 0x92C0;
    const int GLAtomicCounterBufferBinding             = 0x92C1;
    const int GLAtomicCounterBufferStart               = 0x92C2;
    const int GLAtomicCounterBufferSize                = 0x92C3;
    const int GLMaxVertexAtomicCounterBuffers          = 0x92CC;
    const int GLMaxFragmentAtomicCounterBuffers        = 0x92D0;
    const int GLMaxCombinedAtomicCounterBuffers        = 0x92D1;
    const int GLMaxVertexAtomicCounters                = 0x92D2;
    const int GLMaxFragmentAtomicCounters              = 0x92D6;
    const int GLMaxCombinedAtomicCounters              = 0x92D7;
    const int GLMaxAtomicCounterBufferSize             = 0x92D8;
    const int GLActiveAtomicCounterBuffers             = 0x92D9;
    const int GLUnsignedIntAtomicCounter               = 0x92DB;
    const int GLMaxAtomicCounterBufferBindings         = 0x92DC;
    const int GLUniform                                = 0x92E1;
    const int GLUniformBlock                           = 0x92E2;
    const int GLProgramInput                           = 0x92E3;
    const int GLProgramOutput                          = 0x92E4;
    const int GLBufferVariable                         = 0x92E5;
    const int GLShaderStorageBlock                     = 0x92E6;
    const int GLTransformFeedbackVarying               = 0x92F4;
    const int GLActiveResources                        = 0x92F5;
    const int GLMaxNameLength                          = 0x92F6;
    const int GLMaxNumActiveVariables                  = 0x92F7;
    const int GLNameLength                             = 0x92F9;
    const int GLType                                   = 0x92FA;
    const int GLArraySize                              = 0x92FB;
    const int GLOffset                                 = 0x92FC;
    const int GLBlockIndex                             = 0x92FD;
    const int GLArrayStride                            = 0x92FE;
    const int GLMatrixStride                           = 0x92FF;
    const int GLIsRowMajor                             = 0x9300;
    const int GLAtomicCounterBufferIndex               = 0x9301;
    const int GLBufferBinding                          = 0x9302;
    const int GLBufferDataSize                         = 0x9303;
    const int GLNumActiveVariables                     = 0x9304;
    const int GLActiveVariables                        = 0x9305;
    const int GLReferencedByVertexShader               = 0x9306;
    const int GLReferencedByFragmentShader             = 0x930A;
    const int GLReferencedByComputeShader              = 0x930B;
    const int GLTopLevelArraySize                      = 0x930C;
    const int GLTopLevelArrayStride                    = 0x930D;
    const int GLLocation                               = 0x930E;
    const int GLFramebufferDefaultWidth                = 0x9310;
    const int GLFramebufferDefaultHeight               = 0x9311;
    const int GLFramebufferDefaultSamples              = 0x9313;
    const int GLFramebufferDefaultFixedSampleLocations = 0x9314;
    const int GLMaxFramebufferWidth                    = 0x9315;
    const int GLMaxFramebufferHeight                   = 0x9316;
    const int GLMaxFramebufferSamples                  = 0x9318;
}

// ========================================================================
// ========================================================================