﻿// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin.
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

namespace LughSharp.Lugh.Graphics.OpenGL;

/// <summary>
/// OpenGL 2.0 functions.
/// </summary>
partial interface IGL
{
    const int GL_ES_VERSION_2_0                               = 1;
    const int GL_DEPTH_BUFFER_BIT                             = 0x00000100;
    const int GL_STENCIL_BUFFER_BIT                           = 0x00000400;
    const int GL_COLOR_BUFFER_BIT                             = 0x00004000;
    const int GL_FALSE                                        = 0;
    const int GL_TRUE                                         = 1;
    const int GL_UNRESTRICTED                                 = -1;
    const int GL_POINTS                                       = 0x0000;
    const int GL_LINES                                        = 0x0001;
    const int GL_LINE_LOOP                                    = 0x0002;
    const int GL_LINE_STRIP                                   = 0x0003;
    const int GL_TRIANGLES                                    = 0x0004;
    const int GL_TRIANGLE_STRIP                               = 0x0005;
    const int GL_TRIANGLE_FAN                                 = 0x0006;
    const int GL_ZERO                                         = 0;
    const int GL_ONE                                          = 1;
    const int GL_SRC_COLOR                                    = 0x0300;
    const int GL_ONE_MINUS_SRC_COLOR                          = 0x0301;
    const int GL_SRC_ALPHA                                    = 0x0302;
    const int GL_ONE_MINUS_SRC_ALPHA                          = 0x0303;
    const int GL_DST_ALPHA                                    = 0x0304;
    const int GL_ONE_MINUS_DST_ALPHA                          = 0x0305;
    const int GL_DST_COLOR                                    = 0x0306;
    const int GL_ONE_MINUS_DST_COLOR                          = 0x0307;
    const int GL_SRC_ALPHA_SATURATE                           = 0x0308;
    const int GL_FUNC_ADD                                     = 0x8006;
    const int GL_BLEND_EQUATION                               = 0x8009;
    const int GL_BLEND_EQUATION_RGB                           = 0x8009;
    const int GL_BLEND_EQUATION_ALPHA                         = 0x883D;
    const int GL_FUNC_SUBTRACT                                = 0x800A;
    const int GL_FUNC_REVERSE_SUBTRACT                        = 0x800B;
    const int GL_BLEND_DST_RGB                                = 0x80C8;
    const int GL_BLEND_SRC_RGB                                = 0x80C9;
    const int GL_BLEND_DST_ALPHA                              = 0x80CA;
    const int GL_BLEND_SRC_ALPHA                              = 0x80CB;
    const int GL_CONSTANT_COLOR                               = 0x8001;
    const int GL_ONE_MINUS_CONSTANT_COLOR                     = 0x8002;
    const int GL_CONSTANT_ALPHA                               = 0x8003;
    const int GL_ONE_MINUS_CONSTANT_ALPHA                     = 0x8004;
    const int GL_BLEND_COLOR                                  = 0x8005;
    const int GL_ARRAY_BUFFER                                 = 0x8892;
    const int GL_ELEMENT_ARRAY_BUFFER                         = 0x8893;
    const int GL_ARRAY_BUFFER_BINDING                         = 0x8894;
    const int GL_ELEMENT_ARRAY_BUFFER_BINDING                 = 0x8895;
    const int GL_STREAM_DRAW                                  = 0x88E0;
    const int GL_STATIC_DRAW                                  = 0x88E4;
    const int GL_DYNAMIC_DRAW                                 = 0x88E8;
    const int GL_BUFFER_SIZE                                  = 0x8764;
    const int GL_BUFFER_USAGE                                 = 0x8765;
    const int GL_CURRENT_VERTEX_ATTRIB                        = 0x8626;
    const int GL_FRONT                                        = 0x0404;
    const int GL_BACK                                         = 0x0405;
    const int GL_FRONT_AND_BACK                               = 0x0408;
    const int GL_TEXTURE_2D                                   = 0x0DE1;
    const int GL_CULL_FACE                                    = 0x0B44;
    const int GL_BLEND                                        = 0x0BE2;
    const int GL_DITHER                                       = 0x0BD0;
    const int GL_STENCIL_TEST                                 = 0x0B90;
    const int GL_DEPTH_TEST                                   = 0x0B71;
    const int GL_SCISSOR_TEST                                 = 0x0C11;
    const int GL_POLYGON_OFFSET_FILL                          = 0x8037;
    const int GL_SAMPLE_ALPHA_TO_COVERAGE                     = 0x809E;
    const int GL_SAMPLE_COVERAGE                              = 0x80A0;
    const int GL_NO_ERROR                                     = 0;
    const int GL_INVALID_ENUM                                 = 0x0500;
    const int GL_INVALID_VALUE                                = 0x0501;
    const int GL_INVALID_OPERATION                            = 0x0502;
    const int GL_OUT_OF_MEMORY                                = 0x0505;
    const int GL_CW                                           = 0x0900;
    const int GL_CCW                                          = 0x0901;
    const int GL_LINE_WIDTH                                   = 0x0B21;
    const int GL_ALIASED_POINT_SIZE_RANGE                     = 0x846D;
    const int GL_ALIASED_LINE_WIDTH_RANGE                     = 0x846E;
    const int GL_CULL_FACE_MODE                               = 0x0B45;
    const int GL_FRONT_FACE                                   = 0x0B46;
    const int GL_DEPTH_RANGE                                  = 0x0B70;
    const int GL_DEPTH_WRITEMASK                              = 0x0B72;
    const int GL_DEPTH_CLEAR_VALUE                            = 0x0B73;
    const int GL_DEPTH_FUNC                                   = 0x0B74;
    const int GL_STENCIL_CLEAR_VALUE                          = 0x0B91;
    const int GL_STENCIL_FUNC                                 = 0x0B92;
    const int GL_STENCIL_FAIL                                 = 0x0B94;
    const int GL_STENCIL_PASS_DEPTH_FAIL                      = 0x0B95;
    const int GL_STENCIL_PASS_DEPTH_PASS                      = 0x0B96;
    const int GL_STENCIL_REF                                  = 0x0B97;
    const int GL_STENCIL_VALUE_MASK                           = 0x0B93;
    const int GL_STENCIL_WRITEMASK                            = 0x0B98;
    const int GL_STENCIL_BACK_FUNC                            = 0x8800;
    const int GL_STENCIL_BACK_FAIL                            = 0x8801;
    const int GL_STENCIL_BACK_PASS_DEPTH_FAIL                 = 0x8802;
    const int GL_STENCIL_BACK_PASS_DEPTH_PASS                 = 0x8803;
    const int GL_STENCIL_BACK_REF                             = 0x8CA3;
    const int GL_STENCIL_BACK_VALUE_MASK                      = 0x8CA4;
    const int GL_STENCIL_BACK_WRITEMASK                       = 0x8CA5;
    const int GL_VIEWPORT                                     = 0x0BA2;
    const int GL_SCISSOR_BOX                                  = 0x0C10;
    const int GL_COLOR_CLEAR_VALUE                            = 0x0C22;
    const int GL_COLOR_WRITEMASK                              = 0x0C23;
    const int GL_UNPACK_ALIGNMENT                             = 0x0CF5;
    const int GL_PACK_ALIGNMENT                               = 0x0D05;
    const int GL_MAX_TEXTURE_SIZE                             = 0x0D33;
    const int GL_MAX_TEXTURE_UNITS                            = 0x84E2;
    const int GL_MAX_VIEWPORT_DIMS                            = 0x0D3A;
    const int GL_SUBPIXEL_BITS                                = 0x0D50;
    const int GL_RED_BITS                                     = 0x0D52;
    const int GL_GREEN_BITS                                   = 0x0D53;
    const int GL_BLUE_BITS                                    = 0x0D54;
    const int GL_ALPHA_BITS                                   = 0x0D55;
    const int GL_DEPTH_BITS                                   = 0x0D56;
    const int GL_STENCIL_BITS                                 = 0x0D57;
    const int GL_POLYGON_OFFSET_UNITS                         = 0x2A00;
    const int GL_POLYGON_OFFSET_FACTOR                        = 0x8038;
    const int GL_TEXTURE_BINDING_2D                           = 0x8069;
    const int GL_SAMPLE_BUFFERS                               = 0x80A8;
    const int GL_SAMPLES                                      = 0x80A9;
    const int GL_SAMPLE_COVERAGE_VALUE                        = 0x80AA;
    const int GL_SAMPLE_COVERAGE_INVERT                       = 0x80AB;
    const int GL_NUM_COMPRESSED_TEXTURE_FORMATS               = 0x86A2;
    const int GL_COMPRESSED_TEXTURE_FORMATS                   = 0x86A3;
    const int GL_DONT_CARE                                    = 0x1100;
    const int GL_FASTEST                                      = 0x1101;
    const int GL_NICEST                                       = 0x1102;
    const int GL_GENERATE_MIPMAP                              = 0x8191;
    const int GL_GENERATE_MIPMAP_HINT                         = 0x8192;
    const int GL_BYTE                                         = 0x1400;
    const int GL_UNSIGNED_BYTE                                = 0x1401;
    const int GL_SHORT                                        = 0x1402;
    const int GL_UNSIGNED_SHORT                               = 0x1403;
    const int GL_INT                                          = 0x1404;
    const int GL_UNSIGNED_INT                                 = 0x1405;
    const int GL_FLOAT                                        = 0x1406;
    const int GL_FIXED                                        = 0x140C;
    const int GL_COLOR_INDEX                                  = 0x1900;
    const int GL_DEPTH_COMPONENT                              = 0x1902;
    const int GL_ALPHA                                        = 0x1906;
    const int GL_RGB                                          = 0x1907;
    const int GL_RGBA                                         = 0x1908;
    const int GL_LUMINANCE                                    = 0x1909;
    const int GL_LUMINANCE_ALPHA                              = 0x190A;
    const int GL_UNSIGNED_SHORT_4_4_4_4                       = 0x8033;
    const int GL_UNSIGNED_SHORT_5_5_5_1                       = 0x8034;
    const int GL_UNSIGNED_SHORT_5_6_5                         = 0x8363;
    const int GL_FRAGMENT_SHADER                              = 0x8B30;
    const int GL_VERTEX_SHADER                                = 0x8B31;
    const int GL_MAX_VERTEX_ATTRIBS                           = 0x8869;
    const int GL_MAX_VERTEX_UNIFORM_VECTORS                   = 0x8DFB;
    const int GL_MAX_VARYING_VECTORS                          = 0x8DFC;
    const int GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS             = 0x8B4D;
    const int GL_MAX_VERTEX_TEXTURE_IMAGE_UNITS               = 0x8B4C;
    const int GL_MAX_TEXTURE_IMAGE_UNITS                      = 0x8872;
    const int GL_MAX_FRAGMENT_UNIFORM_VECTORS                 = 0x8DFD;
    const int GL_SHADER_TYPE                                  = 0x8B4F;
    const int GL_DELETE_STATUS                                = 0x8B80;
    const int GL_LINK_STATUS                                  = 0x8B82;
    const int GL_VALIDATE_STATUS                              = 0x8B83;
    const int GL_ATTACHED_SHADERS                             = 0x8B85;
    const int GL_ACTIVE_UNIFORMS                              = 0x8B86;
    const int GL_ACTIVE_UNIFORM_MAX_LENGTH                    = 0x8B87;
    const int GL_ACTIVE_ATTRIBUTES                            = 0x8B89;
    const int GL_ACTIVE_ATTRIBUTE_MAX_LENGTH                  = 0x8B8A;
    const int GL_SHADING_LANGUAGE_VERSION                     = 0x8B8C;
    const int GL_CURRENT_PROGRAM                              = 0x8B8D;
    const int GL_NEVER                                        = 0x0200;
    const int GL_LESS                                         = 0x0201;
    const int GL_EQUAL                                        = 0x0202;
    const int GL_LEQUAL                                       = 0x0203;
    const int GL_GREATER                                      = 0x0204;
    const int GL_NOTEQUAL                                     = 0x0205;
    const int GL_GEQUAL                                       = 0x0206;
    const int GL_ALWAYS                                       = 0x0207;
    const int GL_KEEP                                         = 0x1E00;
    const int GL_REPLACE                                      = 0x1E01;
    const int GL_INCR                                         = 0x1E02;
    const int GL_DECR                                         = 0x1E03;
    const int GL_INVERT                                       = 0x150A;
    const int GL_INCR_WRAP                                    = 0x8507;
    const int GL_DECR_WRAP                                    = 0x8508;
    const int GL_VENDOR                                       = 0x1F00;
    const int GL_RENDERER                                     = 0x1F01;
    const int GL_VERSION                                      = 0x1F02;
    const int GL_EXTENSIONS                                   = 0x1F03;
    const int GL_NEAREST                                      = 0x2600;
    const int GL_LINEAR                                       = 0x2601;
    const int GL_NEAREST_MIPMAP_NEAREST                       = 0x2700;
    const int GL_LINEAR_MIPMAP_NEAREST                        = 0x2701;
    const int GL_NEAREST_MIPMAP_LINEAR                        = 0x2702;
    const int GL_LINEAR_MIPMAP_LINEAR                         = 0x2703;
    const int GL_TEXTURE_MAG_FILTER                           = 0x2800;
    const int GL_TEXTURE_MIN_FILTER                           = 0x2801;
    const int GL_TEXTURE_WRAP_S                               = 0x2802;
    const int GL_TEXTURE_WRAP_T                               = 0x2803;
    const int GL_TEXTURE                                      = 0x1702;
    const int GL_TEXTURE_CUBE_MAP                             = 0x8513;
    const int GL_TEXTURE_BINDING_CUBE_MAP                     = 0x8514;
    const int GL_TEXTURE_CUBE_MAP_POSITIVE_X                  = 0x8515;
    const int GL_TEXTURE_CUBE_MAP_NEGATIVE_X                  = 0x8516;
    const int GL_TEXTURE_CUBE_MAP_POSITIVE_Y                  = 0x8517;
    const int GL_TEXTURE_CUBE_MAP_NEGATIVE_Y                  = 0x8518;
    const int GL_TEXTURE_CUBE_MAP_POSITIVE_Z                  = 0x8519;
    const int GL_TEXTURE_CUBE_MAP_NEGATIVE_Z                  = 0x851A;
    const int GL_MAX_CUBE_MAP_TEXTURE_SIZE                    = 0x851C;
    const int GL_TEXTURE0                                     = 0x84C0;
    const int GL_TEXTURE1                                     = 0x84C1;
    const int GL_TEXTURE2                                     = 0x84C2;
    const int GL_TEXTURE3                                     = 0x84C3;
    const int GL_TEXTURE4                                     = 0x84C4;
    const int GL_TEXTURE5                                     = 0x84C5;
    const int GL_TEXTURE6                                     = 0x84C6;
    const int GL_TEXTURE7                                     = 0x84C7;
    const int GL_TEXTURE8                                     = 0x84C8;
    const int GL_TEXTURE9                                     = 0x84C9;
    const int GL_TEXTURE10                                    = 0x84CA;
    const int GL_TEXTURE11                                    = 0x84CB;
    const int GL_TEXTURE12                                    = 0x84CC;
    const int GL_TEXTURE13                                    = 0x84CD;
    const int GL_TEXTURE14                                    = 0x84CE;
    const int GL_TEXTURE15                                    = 0x84CF;
    const int GL_TEXTURE16                                    = 0x84D0;
    const int GL_TEXTURE17                                    = 0x84D1;
    const int GL_TEXTURE18                                    = 0x84D2;
    const int GL_TEXTURE19                                    = 0x84D3;
    const int GL_TEXTURE20                                    = 0x84D4;
    const int GL_TEXTURE21                                    = 0x84D5;
    const int GL_TEXTURE22                                    = 0x84D6;
    const int GL_TEXTURE23                                    = 0x84D7;
    const int GL_TEXTURE24                                    = 0x84D8;
    const int GL_TEXTURE25                                    = 0x84D9;
    const int GL_TEXTURE26                                    = 0x84DA;
    const int GL_TEXTURE27                                    = 0x84DB;
    const int GL_TEXTURE28                                    = 0x84DC;
    const int GL_TEXTURE29                                    = 0x84DD;
    const int GL_TEXTURE30                                    = 0x84DE;
    const int GL_TEXTURE31                                    = 0x84DF;
    const int GL_ACTIVE_TEXTURE                               = 0x84E0;
    const int GL_REPEAT                                       = 0x2901;
    const int GL_CLAMP_TO_EDGE                                = 0x812F;
    const int GL_MIRRORED_REPEAT                              = 0x8370;
    const int GL_FLOAT_VEC2                                   = 0x8B50;
    const int GL_FLOAT_VEC3                                   = 0x8B51;
    const int GL_FLOAT_VEC4                                   = 0x8B52;
    const int GL_INT_VEC2                                     = 0x8B53;
    const int GL_INT_VEC3                                     = 0x8B54;
    const int GL_INT_VEC4                                     = 0x8B55;
    const int GL_BOOL                                         = 0x8B56;
    const int GL_BOOL_VEC2                                    = 0x8B57;
    const int GL_BOOL_VEC3                                    = 0x8B58;
    const int GL_BOOL_VEC4                                    = 0x8B59;
    const int GL_FLOAT_MAT2                                   = 0x8B5A;
    const int GL_FLOAT_MAT3                                   = 0x8B5B;
    const int GL_FLOAT_MAT4                                   = 0x8B5C;
    const int GL_SAMPLER_2D                                   = 0x8B5E;
    const int GL_SAMPLER_CUBE                                 = 0x8B60;
    const int GL_VERTEX_ATTRIB_ARRAY_ENABLED                  = 0x8622;
    const int GL_VERTEX_ATTRIB_ARRAY_SIZE                     = 0x8623;
    const int GL_VERTEX_ATTRIB_ARRAY_STRIDE                   = 0x8624;
    const int GL_VERTEX_ATTRIB_ARRAY_TYPE                     = 0x8625;
    const int GL_VERTEX_ATTRIB_ARRAY_NORMALIZED               = 0x886A;
    const int GL_VERTEX_ATTRIB_ARRAY_POINTER                  = 0x8645;
    const int GL_VERTEX_ATTRIB_ARRAY_BUFFER_BINDING           = 0x889F;
    const int GL_IMPLEMENTATION_COLOR_READ_TYPE               = 0x8B9A;
    const int GL_IMPLEMENTATION_COLOR_READ_FORMAT             = 0x8B9B;
    const int GL_COMPILE_STATUS                               = 0x8B81;
    const int GL_INFO_LOG_LENGTH                              = 0x8B84;
    const int GL_SHADER_SOURCE_LENGTH                         = 0x8B88;
    const int GL_SHADER_COMPILER                              = 0x8DFA;
    const int GL_SHADER_BINARY_FORMATS                        = 0x8DF8;
    const int GL_NUM_SHADER_BINARY_FORMATS                    = 0x8DF9;
    const int GL_LOW_FLOAT                                    = 0x8DF0;
    const int GL_MEDIUM_FLOAT                                 = 0x8DF1;
    const int GL_HIGH_FLOAT                                   = 0x8DF2;
    const int GL_LOW_INT                                      = 0x8DF3;
    const int GL_MEDIUM_INT                                   = 0x8DF4;
    const int GL_HIGH_INT                                     = 0x8DF5;
    const int GL_FRAMEBUFFER                                  = 0x8D40;
    const int GL_RENDERBUFFER                                 = 0x8D41;
    const int GL_RGBA4                                        = 0x8056;
    const int GL_RGB5_A1                                      = 0x8057;
    const int GL_RGB565                                       = 0x8D62;
    const int GL_DEPTH_COMPONENT16                            = 0x81A5;
    const int GL_STENCIL_INDEX                                = 0x1901;
    const int GL_STENCIL_INDEX8                               = 0x8D48;
    const int GL_RENDERBUFFER_WIDTH                           = 0x8D42;
    const int GL_RENDERBUFFER_HEIGHT                          = 0x8D43;
    const int GL_RENDERBUFFER_INTERNAL_FORMAT                 = 0x8D44;
    const int GL_RENDERBUFFER_RED_SIZE                        = 0x8D50;
    const int GL_RENDERBUFFER_GREEN_SIZE                      = 0x8D51;
    const int GL_RENDERBUFFER_BLUE_SIZE                       = 0x8D52;
    const int GL_RENDERBUFFER_ALPHA_SIZE                      = 0x8D53;
    const int GL_RENDERBUFFER_DEPTH_SIZE                      = 0x8D54;
    const int GL_RENDERBUFFER_STENCIL_SIZE                    = 0x8D55;
    const int GL_FRAMEBUFFER_ATTACHMENT_OBJECT_TYPE           = 0x8CD0;
    const int GL_FRAMEBUFFER_ATTACHMENT_OBJECT_NAME           = 0x8CD1;
    const int GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_LEVEL         = 0x8CD2;
    const int GL_FRAMEBUFFER_ATTACHMENT_TEXTURE_CUBE_MAP_FACE = 0x8CD3;
    const int GL_COLOR_ATTACHMENT0                            = 0x8CE0;
    const int GL_DEPTH_ATTACHMENT                             = 0x8D00;
    const int GL_STENCIL_ATTACHMENT                           = 0x8D20;
    const int GL_NONE                                         = 0;
    const int GL_FRAMEBUFFER_COMPLETE                         = 0x8CD5;
    const int GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT            = 0x8CD6;
    const int GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT    = 0x8CD7;
    const int GL_FRAMEBUFFER_INCOMPLETE_DIMENSIONS            = 0x8CD9;
    const int GL_FRAMEBUFFER_UNSUPPORTED                      = 0x8CDD;
    const int GL_FRAMEBUFFER_BINDING                          = 0x8CA6;
    const int GL_RENDERBUFFER_BINDING                         = 0x8CA7;
    const int GL_MAX_RENDERBUFFER_SIZE                        = 0x84E8;
    const int GL_INVALID_FRAMEBUFFER_OPERATION                = 0x0506;
    const int GL_VERTEX_PROGRAM_POINT_SIZE                    = 0x8642;

    // Extensions
    const int GL_COVERAGE_BUFFER_BIT_NV         = 0x8000;
    const int GL_TEXTURE_MAX_ANISOTROPY_EXT     = 0x84FE;
    const int GL_MAX_TEXTURE_MAX_ANISOTROPY_EXT = 0x84FF;
}

// ========================================================================
// ========================================================================
