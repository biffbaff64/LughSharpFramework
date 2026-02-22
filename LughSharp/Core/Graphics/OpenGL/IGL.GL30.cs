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

namespace LughSharp.Core.Graphics.OpenGL;

/// <summary>
/// OpenGL 3.0 functions.
/// </summary>
partial interface IGL
{
    const int GL_READ_BUFFER                                   = 0x0C02;
    const int GL_UNPACK_ROW_LENGTH                             = 0x0CF2;
    const int GL_UNPACK_SKIP_ROWS                              = 0x0CF3;
    const int GL_UNPACK_SKIP_PIXELS                            = 0x0CF4;
    const int GL_PACK_ROW_LENGTH                               = 0x0D02;
    const int GL_PACK_SKIP_ROWS                                = 0x0D03;
    const int GL_PACK_SKIP_PIXELS                              = 0x0D04;
    const int GL_COLOR                                         = 0x1800;
    const int GL_DEPTH                                         = 0x1801;
    const int GL_STENCIL                                       = 0x1802;
    const int GL_RED                                           = 0x1903;
    const int GL_RGB8                                          = 0x8051;
    const int GL_RGBA8                                         = 0x8058;
    const int GL_RGB10_A2                                      = 0x8059;
    const int GL_TEXTURE_BINDING_3D                            = 0x806A;
    const int GL_UNPACK_SKIP_IMAGES                            = 0x806D;
    const int GL_UNPACK_IMAGE_HEIGHT                           = 0x806E;
    const int GL_TEXTURE_3D                                    = 0x806F;
    const int GL_TEXTURE_WRAP_R                                = 0x8072;
    const int GL_MAX_3D_TEXTURE_SIZE                           = 0x8073;
    const int GL_UNSIGNED_INT_2_10_10_10_REV                   = 0x8368;
    const int GL_MAX_ELEMENTS_VERTICES                         = 0x80E8;
    const int GL_MAX_ELEMENTS_INDICES                          = 0x80E9;
    const int GL_TEXTURE_MIN_LOD                               = 0x813A;
    const int GL_TEXTURE_MAX_LOD                               = 0x813B;
    const int GL_TEXTURE_BASE_LEVEL                            = 0x813C;
    const int GL_TEXTURE_MAX_LEVEL                             = 0x813D;
    const int GL_MIN                                           = 0x8007;
    const int GL_MAX                                           = 0x8008;
    const int GL_DEPTH_COMPONENT24                             = 0x81A6;
    const int GL_MAX_TEXTURE_LOD_BIAS                          = 0x84FD;
    const int GL_TEXTURE_COMPARE_MODE                          = 0x884C;
    const int GL_TEXTURE_COMPARE_FUNC                          = 0x884D;
    const int GL_CURRENT_QUERY                                 = 0x8865;
    const int GL_QUERY_RESULT                                  = 0x8866;
    const int GL_QUERY_RESULT_AVAILABLE                        = 0x8867;
    const int GL_BUFFER_MAPPED                                 = 0x88BC;
    const int GL_BUFFER_MAP_POINTER                            = 0x88BD;
    const int GL_STREAM_READ                                   = 0x88E1;
    const int GL_STREAM_COPY                                   = 0x88E2;
    const int GL_STATIC_READ                                   = 0x88E5;
    const int GL_STATIC_COPY                                   = 0x88E6;
    const int GL_DYNAMIC_READ                                  = 0x88E9;
    const int GL_DYNAMIC_COPY                                  = 0x88EA;
    const int GL_MAX_DRAW_BUFFERS                              = 0x8824;
    const int GL_DRAW_BUFFER0                                  = 0x8825;
    const int GL_DRAW_BUFFER1                                  = 0x8826;
    const int GL_DRAW_BUFFER2                                  = 0x8827;
    const int GL_DRAW_BUFFER3                                  = 0x8828;
    const int GL_DRAW_BUFFER4                                  = 0x8829;
    const int GL_DRAW_BUFFER5                                  = 0x882A;
    const int GL_DRAW_BUFFER6                                  = 0x882B;
    const int GL_DRAW_BUFFER7                                  = 0x882C;
    const int GL_DRAW_BUFFER8                                  = 0x882D;
    const int GL_DRAW_BUFFER9                                  = 0x882E;
    const int GL_DRAW_BUFFER10                                 = 0x882F;
    const int GL_DRAW_BUFFER11                                 = 0x8830;
    const int GL_DRAW_BUFFER12                                 = 0x8831;
    const int GL_DRAW_BUFFER13                                 = 0x8832;
    const int GL_DRAW_BUFFER14                                 = 0x8833;
    const int GL_DRAW_BUFFER15                                 = 0x8834;
    const int GL_MAX_FRAGMENT_UNIFORM_COMPONENTS               = 0x8B49;
    const int GL_MAX_VERTEX_UNIFORM_COMPONENTS                 = 0x8B4A;
    const int GL_SAMPLER_3D                                    = 0x8B5F;
    const int GL_SAMPLER_2D_SHADOW                             = 0x8B62;
    const int GL_FRAGMENT_SHADER_DERIVATIVE_HINT               = 0x8B8B;
    const int GL_PIXEL_PACK_BUFFER                             = 0x88EB;
    const int GL_PIXEL_UNPACK_BUFFER                           = 0x88EC;
    const int GL_PIXEL_PACK_BUFFER_BINDING                     = 0x88ED;
    const int GL_PIXEL_UNPACK_BUFFER_BINDING                   = 0x88EF;
    const int GL_FLOAT_MAT2x3                                  = 0x8B65;
    const int GL_FLOAT_MAT2x4                                  = 0x8B66;
    const int GL_FLOAT_MAT3x2                                  = 0x8B67;
    const int GL_FLOAT_MAT3x4                                  = 0x8B68;
    const int GL_FLOAT_MAT4x2                                  = 0x8B69;
    const int GL_FLOAT_MAT4x3                                  = 0x8B6A;
    const int GL_SRGB                                          = 0x8C40;
    const int GL_SRGB8                                         = 0x8C41;
    const int GL_SRGB8_ALPHA8                                  = 0x8C43;
    const int GL_COMPARE_REF_TO_TEXTURE                        = 0x884E;
    const int GL_MAJOR_VERSION                                 = 0x821B;
    const int GL_MINOR_VERSION                                 = 0x821C;
    const int GL_NUM_EXTENSIONS                                = 0x821D;
    const int GL_RGBA32F                                       = 0x8814;
    const int GL_RGB32F                                        = 0x8815;
    const int GL_RGBA16F                                       = 0x881A;
    const int GL_RGB16F                                        = 0x881B;
    const int GL_VERTEX_ATTRIB_ARRAY_INTEGER                   = 0x88FD;
    const int GL_MAX_ARRAY_TEXTURE_LAYERS                      = 0x88FF;
    const int GL_MIN_PROGRAM_TEXEL_OFFSET                      = 0x8904;
    const int GL_MAX_PROGRAM_TEXEL_OFFSET                      = 0x8905;
    const int GL_MAX_VARYING_COMPONENTS                        = 0x8B4B;
    const int GL_TEXTURE_2D_ARRAY                              = 0x8C1A;
    const int GL_TEXTURE_BINDING_2D_ARRAY                      = 0x8C1D;
    const int GL_R11F_G11F_B10F                                = 0x8C3A;
    const int GL_UNSIGNED_INT_10F_11F_11F_REV                  = 0x8C3B;
    const int GL_RGB9_E5                                       = 0x8C3D;
    const int GL_UNSIGNED_INT_5_9_9_9_REV                      = 0x8C3E;
    const int GL_TRANSFORM_FEEDBACK_VARYING_MAX_LENGTH         = 0x8C76;
    const int GL_TRANSFORM_FEEDBACK_BUFFER_MODE                = 0x8C7F;
    const int GL_MAX_TRANSFORM_FEEDBACK_SEPARATE_COMPONENTS    = 0x8C80;
    const int GL_TRANSFORM_FEEDBACK_VARYINGS                   = 0x8C83;
    const int GL_TRANSFORM_FEEDBACK_BUFFER_START               = 0x8C84;
    const int GL_TRANSFORM_FEEDBACK_BUFFER_SIZE                = 0x8C85;
    const int GL_TRANSFORM_FEEDBACK_PRIMITIVES_WRITTEN         = 0x8C88;
    const int GL_RASTERIZER_DISCARD                            = 0x8C89;
    const int GL_MAX_TRANSFORM_FEEDBACK_INTERLEAVED_COMPONENTS = 0x8C8A;
    const int GL_MAX_TRANSFORM_FEEDBACK_SEPARATE_ATTRIBS       = 0x8C8B;
    const int GL_INTERLEAVED_ATTRIBS                           = 0x8C8C;
    const int GL_SEPARATE_ATTRIBS                              = 0x8C8D;
    const int GL_TRANSFORM_FEEDBACK_BUFFER                     = 0x8C8E;
    const int GL_TRANSFORM_FEEDBACK_BUFFER_BINDING             = 0x8C8F;
    const int GL_RGBA32UI                                      = 0x8D70;
    const int GL_RGB32UI                                       = 0x8D71;
    const int GL_RGBA16UI                                      = 0x8D76;
    const int GL_RGB16UI                                       = 0x8D77;
    const int GL_RGBA8UI                                       = 0x8D7C;
    const int GL_RGB8UI                                        = 0x8D7D;
    const int GL_RGBA32I                                       = 0x8D82;
    const int GL_RGB32I                                        = 0x8D83;
    const int GL_RGBA16I                                       = 0x8D88;
    const int GL_RGB16I                                        = 0x8D89;
    const int GL_RGBA8I                                        = 0x8D8E;
    const int GL_RGB8I                                         = 0x8D8F;
    const int GL_RED_INTEGER                                   = 0x8D94;
    const int GL_RGB_INTEGER                                   = 0x8D98;
    const int GL_RGBA_INTEGER                                  = 0x8D99;
    const int GL_SAMPLER_2D_ARRAY                              = 0x8DC1;
    const int GL_SAMPLER_2D_ARRAY_SHADOW                       = 0x8DC4;
    const int GL_SAMPLER_CUBE_SHADOW                           = 0x8DC5;
    const int GL_UNSIGNED_INT_VEC2                             = 0x8DC6;
    const int GL_UNSIGNED_INT_VEC3                             = 0x8DC7;
    const int GL_UNSIGNED_INT_VEC4                             = 0x8DC8;
    const int GL_INT_SAMPLER_2D                                = 0x8DCA;
    const int GL_INT_SAMPLER_3D                                = 0x8DCB;
    const int GL_INT_SAMPLER_CUBE                              = 0x8DCC;
    const int GL_INT_SAMPLER_2D_ARRAY                          = 0x8DCF;
    const int GL_UNSIGNED_INT_SAMPLER_2D                       = 0x8DD2;
    const int GL_UNSIGNED_INT_SAMPLER_3D                       = 0x8DD3;
    const int GL_UNSIGNED_INT_SAMPLER_CUBE                     = 0x8DD4;
    const int GL_UNSIGNED_INT_SAMPLER_2D_ARRAY                 = 0x8DD7;
    const int GL_BUFFER_ACCESS_FLAGS                           = 0x911F;
    const int GL_BUFFER_MAP_LENGTH                             = 0x9120;
    const int GL_BUFFER_MAP_OFFSET                             = 0x9121;
    const int GL_DEPTH_COMPONENT32F                            = 0x8CAC;
    const int GL_DEPTH32F_STENCIL8                             = 0x8CAD;
    const int GL_FLOAT_32_UNSIGNED_INT_24_8_REV                = 0x8DAD;
    const int GL_FRAMEBUFFER_ATTACHMENT_COLOR_ENCODING         = 0x8210;
    const int GL_FRAMEBUFFER_ATTACHMENT_COMPONENT_TYPE         = 0x8211;
    const int GL_FRAMEBUFFER_ATTACHMENT_RED_SIZE               = 0x8212;
    const int GL_FRAMEBUFFER_ATTACHMENT_GREEN_SIZE             = 0x8213;
    const int GL_FRAMEBUFFER_ATTACHMENT_BLUE_SIZE              = 0x8214;
    const int GL_FRAMEBUFFER_ATTACHMENT_ALPHA_SIZE             = 0x8215;
    const int GL_FRAMEBUFFER_ATTACHMENT_DEPTH_SIZE             = 0x8216;
    const int GL_FRAMEBUFFER_ATTACHMENT_STENCIL_SIZE           = 0x8217;
    const int GL_FRAMEBUFFER_DEFAULT                           = 0x8218;
    const int GL_FRAMEBUFFER_UNDEFINED                         = 0x8219;
    const int GL_DEPTH_STENCIL_ATTACHMENT                      = 0x821A;
    const int GL_DEPTH_STENCIL                                 = 0x84F9;
    const int GL_UNSIGNED_INT_24_8                             = 0x84FA;
    const int GL_DEPTH24_STENCIL8                              = 0x88F0;
    const int GL_UNSIGNED_NORMALIZED                           = 0x8C17;
    const int GL_DRAW_FRAMEBUFFER_BINDING                      = GL_FRAMEBUFFER_BINDING;
    const int GL_READ_FRAMEBUFFER                              = 0x8CA8;
    const int GL_DRAW_FRAMEBUFFER                              = 0x8CA9;
    const int GL_READ_FRAMEBUFFER_BINDING                      = 0x8CAA;
    const int GL_RENDERBUFFER_SAMPLES                          = 0x8CAB;
    const int GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_LAYER          = 0x8CD4;
    const int GL_MAX_COLOR_ATTACHMENTS                         = 0x8CDF;
    const int GL_COLOR_ATTACHMENT1                             = 0x8CE1;
    const int GL_COLOR_ATTACHMENT2                             = 0x8CE2;
    const int GL_COLOR_ATTACHMENT3                             = 0x8CE3;
    const int GL_COLOR_ATTACHMENT4                             = 0x8CE4;
    const int GL_COLOR_ATTACHMENT5                             = 0x8CE5;
    const int GL_COLOR_ATTACHMENT6                             = 0x8CE6;
    const int GL_COLOR_ATTACHMENT7                             = 0x8CE7;
    const int GL_COLOR_ATTACHMENT8                             = 0x8CE8;
    const int GL_COLOR_ATTACHMENT9                             = 0x8CE9;
    const int GL_COLOR_ATTACHMENT10                            = 0x8CEA;
    const int GL_COLOR_ATTACHMENT11                            = 0x8CEB;
    const int GL_COLOR_ATTACHMENT12                            = 0x8CEC;
    const int GL_COLOR_ATTACHMENT13                            = 0x8CED;
    const int GL_COLOR_ATTACHMENT14                            = 0x8CEE;
    const int GL_COLOR_ATTACHMENT15                            = 0x8CEF;
    const int GL_FRAMEBUFFER_INCOMPLETE_MULTISAMPLE            = 0x8D56;
    const int GL_MAX_SAMPLES                                   = 0x8D57;
    const int GL_HALF_FLOAT                                    = 0x140B;
    const int GL_MAP_READ_BIT                                  = 0x0001;
    const int GL_MAP_WRITE_BIT                                 = 0x0002;
    const int GL_MAP_INVALIDATE_RANGE_BIT                      = 0x0004;
    const int GL_MAP_INVALIDATE_BUFFER_BIT                     = 0x0008;
    const int GL_MAP_FLUSH_EXPLICIT_BIT                        = 0x0010;
    const int GL_MAP_UNSYNCHRONIZED_BIT                        = 0x0020;
    const int GL_RG                                            = 0x8227;
    const int GL_RG_INTEGER                                    = 0x8228;
    const int GL_R8                                            = 0x8229;
    const int GL_RG8                                           = 0x822B;
    const int GL_R16F                                          = 0x822D;
    const int GL_R32F                                          = 0x822E;
    const int GL_RG16F                                         = 0x822F;
    const int GL_RG32F                                         = 0x8230;
    const int GL_R8I                                           = 0x8231;
    const int GL_R8UI                                          = 0x8232;
    const int GL_R16I                                          = 0x8233;
    const int GL_R16UI                                         = 0x8234;
    const int GL_R32I                                          = 0x8235;
    const int GL_R32UI                                         = 0x8236;
    const int GL_RG8I                                          = 0x8237;
    const int GL_RG8UI                                         = 0x8238;
    const int GL_RG16I                                         = 0x8239;
    const int GL_RG16UI                                        = 0x823A;
    const int GL_RG32I                                         = 0x823B;
    const int GL_RG32UI                                        = 0x823C;
    const int GL_VERTEX_ARRAY_BINDING                          = 0x85B5;
    const int GL_R8_SNORM                                      = 0x8F94;
    const int GL_RG8_SNORM                                     = 0x8F95;
    const int GL_RGB8_SNORM                                    = 0x8F96;
    const int GL_RGBA8_SNORM                                   = 0x8F97;
    const int GL_SIGNED_NORMALIZED                             = 0x8F9C;
    const int GL_PRIMITIVE_RESTART_FIXED_INDEX                 = 0x8D69;
    const int GL_COPY_READ_BUFFER                              = 0x8F36;
    const int GL_COPY_WRITE_BUFFER                             = 0x8F37;
    const int GL_COPY_READ_BUFFER_BINDING                      = GL_COPY_READ_BUFFER;
    const int GL_COPY_WRITE_BUFFER_BINDING                     = GL_COPY_WRITE_BUFFER;
    const int GL_UNIFORM_BUFFER                                = 0x8A11;
    const int GL_UNIFORM_BUFFER_BINDING                        = 0x8A28;
    const int GL_UNIFORM_BUFFER_START                          = 0x8A29;
    const int GL_UNIFORM_BUFFER_SIZE                           = 0x8A2A;
    const int GL_MAX_VERTEX_UNIFORM_BLOCKS                     = 0x8A2B;
    const int GL_MAX_FRAGMENT_UNIFORM_BLOCKS                   = 0x8A2D;
    const int GL_MAX_COMBINED_UNIFORM_BLOCKS                   = 0x8A2E;
    const int GL_MAX_UNIFORM_BUFFER_BINDINGS                   = 0x8A2F;
    const int GL_MAX_UNIFORM_BLOCK_SIZE                        = 0x8A30;
    const int GL_MAX_COMBINED_VERTEX_UNIFORM_COMPONENTS        = 0x8A31;
    const int GL_MAX_COMBINED_FRAGMENT_UNIFORM_COMPONENTS      = 0x8A33;
    const int GL_UNIFORM_BUFFER_OFFSET_ALIGNMENT               = 0x8A34;
    const int GL_ACTIVE_UNIFORM_BLOCK_MAX_NAME_LENGTH          = 0x8A35;
    const int GL_ACTIVE_UNIFORM_BLOCKS                         = 0x8A36;
    const int GL_UNIFORM_TYPE                                  = 0x8A37;
    const int GL_UNIFORM_SIZE                                  = 0x8A38;
    const int GL_UNIFORM_NAME_LENGTH                           = 0x8A39;
    const int GL_UNIFORM_BLOCK_INDEX                           = 0x8A3A;
    const int GL_UNIFORM_OFFSET                                = 0x8A3B;
    const int GL_UNIFORM_ARRAY_STRIDE                          = 0x8A3C;
    const int GL_UNIFORM_MATRIX_STRIDE                         = 0x8A3D;
    const int GL_UNIFORM_IS_ROW_MAJOR                          = 0x8A3E;
    const int GL_UNIFORM_BLOCK_BINDING                         = 0x8A3F;
    const int GL_UNIFORM_BLOCK_DATA_SIZE                       = 0x8A40;
    const int GL_UNIFORM_BLOCK_NAME_LENGTH                     = 0x8A41;
    const int GL_UNIFORM_BLOCK_ACTIVE_UNIFORMS                 = 0x8A42;
    const int GL_UNIFORM_BLOCK_ACTIVE_UNIFORM_INDICES          = 0x8A43;
    const int GL_UNIFORM_BLOCK_REFERENCED_BY_VERTEX_SHADER     = 0x8A44;

    const int GL_UNIFORM_BLOCK_REFERENCED_BY_FRAGMENT_SHADER = 0x8A46;

    // GL_INVALID_INDEX is defined as 0xFFFFFFFFu in C.
    const int GL_INVALID_INDEX                 = -1;
    const int GL_MAX_VERTEX_OUTPUT_COMPONENTS  = 0x9122;
    const int GL_MAX_FRAGMENT_INPUT_COMPONENTS = 0x9125;
    const int GL_MAX_SERVER_WAIT_TIMEOUT       = 0x9111;
    const int GL_OBJECT_TYPE                   = 0x9112;
    const int GL_SYNC_CONDITION                = 0x9113;
    const int GL_SYNC_STATUS                   = 0x9114;
    const int GL_SYNC_FLAGS                    = 0x9115;
    const int GL_SYNC_FENCE                    = 0x9116;
    const int GL_SYNC_GPU_COMMANDS_COMPLETE    = 0x9117;
    const int GL_UNSIGNALED                    = 0x9118;
    const int GL_SIGNALED                      = 0x9119;
    const int GL_ALREADY_SIGNALED              = 0x911A;
    const int GL_TIMEOUT_EXPIRED               = 0x911B;
    const int GL_CONDITION_SATISFIED           = 0x911C;
    const int GL_WAIT_FAILED                   = 0x911D;

    const int GL_SYNC_FLUSH_COMMANDS_BIT = 0x00000001;

    // GL_TIMEOUT_IGNORED is defined as 0xFFFFFFFFFFFFFFFFull in C.
    const long GL_TIMEOUT_IGNORED                           = -1;
    const int  GL_VERTEX_ATTRIB_ARRAY_DIVISOR               = 0x88FE;
    const int  GL_ANY_SAMPLES_PASSED                        = 0x8C2F;
    const int  GL_ANY_SAMPLES_PASSED_CONSERVATIVE           = 0x8D6A;
    const int  GL_SAMPLER_BINDING                           = 0x8919;
    const int  GL_RGB10_A2UI                                = 0x906F;
    const int  GL_TEXTURE_SWIZZLE_R                         = 0x8E42;
    const int  GL_TEXTURE_SWIZZLE_G                         = 0x8E43;
    const int  GL_TEXTURE_SWIZZLE_B                         = 0x8E44;
    const int  GL_TEXTURE_SWIZZLE_A                         = 0x8E45;
    const int  GL_GREEN                                     = 0x1904;
    const int  GL_BLUE                                      = 0x1905;
    const int  GL_INT_2_10_10_10_REV                        = 0x8D9F;
    const int  GL_TRANSFORM_FEEDBACK                        = 0x8E22;
    const int  GL_TRANSFORM_FEEDBACK_PAUSED                 = 0x8E23;
    const int  GL_TRANSFORM_FEEDBACK_ACTIVE                 = 0x8E24;
    const int  GL_TRANSFORM_FEEDBACK_BINDING                = 0x8E25;
    const int  GL_PROGRAM_BINARY_RETRIEVABLE_HINT           = 0x8257;
    const int  GL_PROGRAM_BINARY_LENGTH                     = 0x8741;
    const int  GL_NUM_PROGRAM_BINARY_FORMATS                = 0x87FE;
    const int  GL_PROGRAM_BINARY_FORMATS                    = 0x87FF;
    const int  GL_COMPRESSED_R11_EAC                        = 0x9270;
    const int  GL_COMPRESSED_SIGNED_R11_EAC                 = 0x9271;
    const int  GL_COMPRESSED_RG11_EAC                       = 0x9272;
    const int  GL_COMPRESSED_SIGNED_RG11_EAC                = 0x9273;
    const int  GL_COMPRESSED_RGB8_ETC2                      = 0x9274;
    const int  GL_COMPRESSED_SRGB8_ETC2                     = 0x9275;
    const int  GL_COMPRESSED_RGB8_PUNCHTHROUGH_ALPHA1_ETC2  = 0x9276;
    const int  GL_COMPRESSED_SRGB8_PUNCHTHROUGH_ALPHA1_ETC2 = 0x9277;
    const int  GL_COMPRESSED_RGBA8_ETC2_EAC                 = 0x9278;
    const int  GL_COMPRESSED_SRGB8_ALPHA8_ETC2_EAC          = 0x9279;
    const int  GL_TEXTURE_IMMUTABLE_FORMAT                  = 0x912F;
    const int  GL_MAX_ELEMENT_INDEX                         = 0x8D6B;
    const int  GL_NUM_SAMPLE_COUNTS                         = 0x9380;
    const int  GL_TEXTURE_IMMUTABLE_LEVELS                  = 0x82DF;
}

// ========================================================================
// ========================================================================
