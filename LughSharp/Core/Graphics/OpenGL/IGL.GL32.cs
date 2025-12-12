// /////////////////////////////////////////////////////////////////////////////
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

using JetBrains.Annotations; namespace LughSharp.Core.Graphics.OpenGL;

/// <summary>
/// OpenGL 3.2 functions.
/// </summary>
[SuppressMessage( "ReSharper", "InconsistentNaming" )]
partial interface IGL
{
    const int GL_CONTEXT_FLAG_DEBUG_BIT                          = 0x00000002;
    const int GL_CONTEXT_FLAG_ROBUST_ACCESS_BIT                  = 0x00000004;
    const int GL_GEOMETRY_SHADER_BIT                             = 0x00000004;
    const int GL_TESS_CONTROL_SHADER_BIT                         = 0x00000008;
    const int GL_TESS_EVALUATION_SHADER_BIT                      = 0x00000010;
    const int GL_QUADS                                           = 0x0007;
    const int GL_LINES_ADJACENCY                                 = 0x000A;
    const int GL_LINE_STRIP_ADJACENCY                            = 0x000B;
    const int GL_TRIANGLES_ADJACENCY                             = 0x000C;
    const int GL_TRIANGLE_STRIP_ADJACENCY                        = 0x000D;
    const int GL_PATCHES                                         = 0x000E;
    const int GL_STACK_OVERFLOW                                  = 0x0503;
    const int GL_STACK_UNDERFLOW                                 = 0x0504;
    const int GL_CONTEXT_LOST                                    = 0x0507;
    const int GL_TEXTURE_BORDER_COLOR                            = 0x1004;
    const int GL_VERTEX_ARRAY                                    = 0x8074;
    const int GL_CLAMP_TO_BORDER                                 = 0x812D;
    const int GL_CONTEXT_FLAGS                                   = 0x821E;
    const int GL_PRIMITIVE_RESTART_FOR_PATCHES_SUPPORTED         = 0x8221;
    const int GL_DEBUG_OUTPUT_SYNCHRONOUS                        = 0x8242;
    const int GL_DEBUG_NEXT_LOGGED_MESSAGE_LENGTH                = 0x8243;
    const int GL_DEBUG_CALLBACK_FUNCTION                         = 0x8244;
    const int GL_DEBUG_CALLBACK_USER_PARAM                       = 0x8245;
    const int GL_DEBUG_SOURCE_API                                = 0x8246;
    const int GL_DEBUG_SOURCE_WINDOW_SYSTEM                      = 0x8247;
    const int GL_DEBUG_SOURCE_SHADER_COMPILER                    = 0x8248;
    const int GL_DEBUG_SOURCE_THIRD_PARTY                        = 0x8249;
    const int GL_DEBUG_SOURCE_APPLICATION                        = 0x824A;
    const int GL_DEBUG_SOURCE_OTHER                              = 0x824B;
    const int GL_DEBUG_TYPE_ERROR                                = 0x824C;
    const int GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR                  = 0x824D;
    const int GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR                   = 0x824E;
    const int GL_DEBUG_TYPE_PORTABILITY                          = 0x824F;
    const int GL_DEBUG_TYPE_PERFORMANCE                          = 0x8250;
    const int GL_DEBUG_TYPE_OTHER                                = 0x8251;
    const int GL_LOSE_CONTEXT_ON_RESET                           = 0x8252;
    const int GL_GUILTY_CONTEXT_RESET                            = 0x8253;
    const int GL_INNOCENT_CONTEXT_RESET                          = 0x8254;
    const int GL_UNKNOWN_CONTEXT_RESET                           = 0x8255;
    const int GL_RESET_NOTIFICATION_STRATEGY                     = 0x8256;
    const int GL_LAYER_PROVOKING_VERTEX                          = 0x825E;
    const int GL_UNDEFINED_VERTEX                                = 0x8260;
    const int GL_NO_RESET_NOTIFICATION                           = 0x8261;
    const int GL_DEBUG_TYPE_MARKER                               = 0x8268;
    const int GL_DEBUG_TYPE_PUSH_GROUP                           = 0x8269;
    const int GL_DEBUG_TYPE_POP_GROUP                            = 0x826A;
    const int GL_DEBUG_SEVERITY_NOTIFICATION                     = 0x826B;
    const int GL_MAX_DEBUG_GROUP_STACK_DEPTH                     = 0x826C;
    const int GL_DEBUG_GROUP_STACK_DEPTH                         = 0x826D;
    const int GL_BUFFER                                          = 0x82E0;
    const int GL_SHADER                                          = 0x82E1;
    const int GL_PROGRAM                                         = 0x82E2;
    const int GL_QUERY                                           = 0x82E3;
    const int GL_PROGRAM_PIPELINE                                = 0x82E4;
    const int GL_SAMPLER                                         = 0x82E6;
    const int GL_MAX_LABEL_LENGTH                                = 0x82E8;
    const int GL_MAX_TESS_CONTROL_INPUT_COMPONENTS               = 0x886C;
    const int GL_MAX_TESS_EVALUATION_INPUT_COMPONENTS            = 0x886D;
    const int GL_GEOMETRY_SHADER_INVOCATIONS                     = 0x887F;
    const int GL_GEOMETRY_VERTICES_OUT                           = 0x8916;
    const int GL_GEOMETRY_INPUT_TYPE                             = 0x8917;
    const int GL_GEOMETRY_OUTPUT_TYPE                            = 0x8918;
    const int GL_MAX_GEOMETRY_UNIFORM_BLOCKS                     = 0x8A2C;
    const int GL_MAX_COMBINED_GEOMETRY_UNIFORM_COMPONENTS        = 0x8A32;
    const int GL_MAX_GEOMETRY_TEXTURE_IMAGE_UNITS                = 0x8C29;
    const int GL_TEXTURE_BUFFER                                  = 0x8C2A;
    const int GL_TEXTURE_BUFFER_BINDING                          = 0x8C2A;
    const int GL_MAX_TEXTURE_BUFFER_SIZE                         = 0x8C2B;
    const int GL_TEXTURE_BINDING_BUFFER                          = 0x8C2C;
    const int GL_TEXTURE_BUFFER_DATA_STORE_BINDING               = 0x8C2D;
    const int GL_SAMPLE_SHADING                                  = 0x8C36;
    const int GL_MIN_SAMPLE_SHADING_VALUE                        = 0x8C37;
    const int GL_PRIMITIVES_GENERATED                            = 0x8C87;
    const int GL_FRAMEBUFFER_ATTACHMENT_LAYERED                  = 0x8DA7;
    const int GL_FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS            = 0x8DA8;
    const int GL_SAMPLER_BUFFER                                  = 0x8DC2;
    const int GL_INT_SAMPLER_BUFFER                              = 0x8DD0;
    const int GL_UNSIGNED_INT_SAMPLER_BUFFER                     = 0x8DD8;
    const int GL_GEOMETRY_SHADER                                 = 0x8DD9;
    const int GL_MAX_GEOMETRY_UNIFORM_COMPONENTS                 = 0x8DDF;
    const int GL_MAX_GEOMETRY_OUTPUT_VERTICES                    = 0x8DE0;
    const int GL_MAX_GEOMETRY_TOTAL_OUTPUT_COMPONENTS            = 0x8DE1;
    const int GL_MAX_COMBINED_TESS_CONTROL_UNIFORM_COMPONENTS    = 0x8E1E;
    const int GL_MAX_COMBINED_TESS_EVALUATION_UNIFORM_COMPONENTS = 0x8E1F;
    const int GL_FIRST_VERTEX_CONVENTION                         = 0x8E4D;
    const int GL_LAST_VERTEX_CONVENTION                          = 0x8E4E;
    const int GL_MAX_GEOMETRY_SHADER_INVOCATIONS                 = 0x8E5A;
    const int GL_MIN_FRAGMENT_INTERPOLATION_OFFSET               = 0x8E5B;
    const int GL_MAX_FRAGMENT_INTERPOLATION_OFFSET               = 0x8E5C;
    const int GL_FRAGMENT_INTERPOLATION_OFFSET_BITS              = 0x8E5D;
    const int GL_PATCH_VERTICES                                  = 0x8E72;
    const int GL_TESS_CONTROL_OUTPUT_VERTICES                    = 0x8E75;
    const int GL_TESS_GEN_MODE                                   = 0x8E76;
    const int GL_TESS_GEN_SPACING                                = 0x8E77;
    const int GL_TESS_GEN_VERTEX_ORDER                           = 0x8E78;
    const int GL_TESS_GEN_POINT_MODE                             = 0x8E79;
    const int GL_ISOLINES                                        = 0x8E7A;
    const int GL_FRACTIONAL_ODD                                  = 0x8E7B;
    const int GL_FRACTIONAL_EVEN                                 = 0x8E7C;
    const int GL_MAX_PATCH_VERTICES                              = 0x8E7D;
    const int GL_MAX_TESS_GEN_LEVEL                              = 0x8E7E;
    const int GL_MAX_TESS_CONTROL_UNIFORM_COMPONENTS             = 0x8E7F;
    const int GL_MAX_TESS_EVALUATION_UNIFORM_COMPONENTS          = 0x8E80;
    const int GL_MAX_TESS_CONTROL_TEXTURE_IMAGE_UNITS            = 0x8E81;
    const int GL_MAX_TESS_EVALUATION_TEXTURE_IMAGE_UNITS         = 0x8E82;
    const int GL_MAX_TESS_CONTROL_OUTPUT_COMPONENTS              = 0x8E83;
    const int GL_MAX_TESS_PATCH_COMPONENTS                       = 0x8E84;
    const int GL_MAX_TESS_CONTROL_TOTAL_OUTPUT_COMPONENTS        = 0x8E85;
    const int GL_MAX_TESS_EVALUATION_OUTPUT_COMPONENTS           = 0x8E86;
    const int GL_TESS_EVALUATION_SHADER                          = 0x8E87;
    const int GL_TESS_CONTROL_SHADER                             = 0x8E88;
    const int GL_MAX_TESS_CONTROL_UNIFORM_BLOCKS                 = 0x8E89;
    const int GL_MAX_TESS_EVALUATION_UNIFORM_BLOCKS              = 0x8E8A;
    const int GL_TEXTURE_CUBE_MAP_ARRAY                          = 0x9009;
    const int GL_TEXTURE_BINDING_CUBE_MAP_ARRAY                  = 0x900A;
    const int GL_SAMPLER_CUBE_MAP_ARRAY                          = 0x900C;
    const int GL_SAMPLER_CUBE_MAP_ARRAY_SHADOW                   = 0x900D;
    const int GL_INT_SAMPLER_CUBE_MAP_ARRAY                      = 0x900E;
    const int GL_UNSIGNED_INT_SAMPLER_CUBE_MAP_ARRAY             = 0x900F;
    const int GL_IMAGE_BUFFER                                    = 0x9051;
    const int GL_IMAGE_CUBE_MAP_ARRAY                            = 0x9054;
    const int GL_INT_IMAGE_BUFFER                                = 0x905C;
    const int GL_INT_IMAGE_CUBE_MAP_ARRAY                        = 0x905F;
    const int GL_UNSIGNED_INT_IMAGE_BUFFER                       = 0x9067;
    const int GL_UNSIGNED_INT_IMAGE_CUBE_MAP_ARRAY               = 0x906A;
    const int GL_MAX_TESS_CONTROL_IMAGE_UNIFORMS                 = 0x90CB;
    const int GL_MAX_TESS_EVALUATION_IMAGE_UNIFORMS              = 0x90CC;
    const int GL_MAX_GEOMETRY_IMAGE_UNIFORMS                     = 0x90CD;
    const int GL_MAX_GEOMETRY_SHADER_STORAGE_BLOCKS              = 0x90D7;
    const int GL_MAX_TESS_CONTROL_SHADER_STORAGE_BLOCKS          = 0x90D8;
    const int GL_MAX_TESS_EVALUATION_SHADER_STORAGE_BLOCKS       = 0x90D9;
    const int GL_TEXTURE_2D_MULTISAMPLE_ARRAY                    = 0x9102;
    const int GL_TEXTURE_BINDING_2D_MULTISAMPLE_ARRAY            = 0x9105;
    const int GL_SAMPLER_2D_MULTISAMPLE_ARRAY                    = 0x910B;
    const int GL_INT_SAMPLER_2D_MULTISAMPLE_ARRAY                = 0x910C;
    const int GL_UNSIGNED_INT_SAMPLER_2D_MULTISAMPLE_ARRAY       = 0x910D;
    const int GL_MAX_GEOMETRY_INPUT_COMPONENTS                   = 0x9123;
    const int GL_MAX_GEOMETRY_OUTPUT_COMPONENTS                  = 0x9124;
    const int GL_MAX_DEBUG_MESSAGE_LENGTH                        = 0x9143;
    const int GL_MAX_DEBUG_LOGGED_MESSAGES                       = 0x9144;
    const int GL_DEBUG_LOGGED_MESSAGES                           = 0x9145;
    const int GL_DEBUG_SEVERITY_HIGH                             = 0x9146;
    const int GL_DEBUG_SEVERITY_MEDIUM                           = 0x9147;
    const int GL_DEBUG_SEVERITY_LOW                              = 0x9148;
    const int GL_TEXTURE_BUFFER_OFFSET                           = 0x919D;
    const int GL_TEXTURE_BUFFER_SIZE                             = 0x919E;
    const int GL_TEXTURE_BUFFER_OFFSET_ALIGNMENT                 = 0x919F;
    const int GL_MULTIPLY                                        = 0x9294;
    const int GL_SCREEN                                          = 0x9295;
    const int GL_OVERLAY                                         = 0x9296;
    const int GL_DARKEN                                          = 0x9297;
    const int GL_LIGHTEN                                         = 0x9298;
    const int GL_COLORDODGE                                      = 0x9299;
    const int GL_COLORBURN                                       = 0x929A;
    const int GL_HARDLIGHT                                       = 0x929B;
    const int GL_SOFTLIGHT                                       = 0x929C;
    const int GL_DIFFERENCE                                      = 0x929E;
    const int GL_EXCLUSION                                       = 0x92A0;
    const int GL_HSL_HUE                                         = 0x92AD;
    const int GL_HSL_SATURATION                                  = 0x92AE;
    const int GL_HSL_COLOR                                       = 0x92AF;
    const int GL_HSL_LUMINOSITY                                  = 0x92B0;
    const int GL_PRIMITIVE_BOUNDING_BOX                          = 0x92BE;
    const int GL_MAX_TESS_CONTROL_ATOMIC_COUNTER_BUFFERS         = 0x92CD;
    const int GL_MAX_TESS_EVALUATION_ATOMIC_COUNTER_BUFFERS      = 0x92CE;
    const int GL_MAX_GEOMETRY_ATOMIC_COUNTER_BUFFERS             = 0x92CF;
    const int GL_MAX_TESS_CONTROL_ATOMIC_COUNTERS                = 0x92D3;
    const int GL_MAX_TESS_EVALUATION_ATOMIC_COUNTERS             = 0x92D4;
    const int GL_MAX_GEOMETRY_ATOMIC_COUNTERS                    = 0x92D5;
    const int GL_DEBUG_OUTPUT                                    = 0x92E0;
    const int GL_IS_PER_PATCH                                    = 0x92E7;
    const int GL_REFERENCED_BY_TESS_CONTROL_SHADER               = 0x9307;
    const int GL_REFERENCED_BY_TESS_EVALUATION_SHADER            = 0x9308;
    const int GL_REFERENCED_BY_GEOMETRY_SHADER                   = 0x9309;
    const int GL_FRAMEBUFFER_DEFAULT_LAYERS                      = 0x9312;
    const int GL_MAX_FRAMEBUFFER_LAYERS                          = 0x9317;
    const int GL_MULTISAMPLE_LINE_WIDTH_RANGE                    = 0x9381;
    const int GL_MULTISAMPLE_LINE_WIDTH_GRANULARITY              = 0x9382;
    const int GL_COMPRESSED_RGBA_ASTC_4X4                        = 0x93B0;
    const int GL_COMPRESSED_RGBA_ASTC_5X4                        = 0x93B1;
    const int GL_COMPRESSED_RGBA_ASTC_5X5                        = 0x93B2;
    const int GL_COMPRESSED_RGBA_ASTC_6X5                        = 0x93B3;
    const int GL_COMPRESSED_RGBA_ASTC_6X6                        = 0x93B4;
    const int GL_COMPRESSED_RGBA_ASTC_8X5                        = 0x93B5;
    const int GL_COMPRESSED_RGBA_ASTC_8X6                        = 0x93B6;
    const int GL_COMPRESSED_RGBA_ASTC_8X8                        = 0x93B7;
    const int GL_COMPRESSED_RGBA_ASTC_10X5                       = 0x93B8;
    const int GL_COMPRESSED_RGBA_ASTC_10X6                       = 0x93B9;
    const int GL_COMPRESSED_RGBA_ASTC_10X8                       = 0x93BA;
    const int GL_COMPRESSED_RGBA_ASTC_10X10                      = 0x93BB;
    const int GL_COMPRESSED_RGBA_ASTC_12X10                      = 0x93BC;
    const int GL_COMPRESSED_RGBA_ASTC_12X12                      = 0x93BD;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_4X4                = 0x93D0;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_5X4                = 0x93D1;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_5X5                = 0x93D2;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_6X5                = 0x93D3;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_6X6                = 0x93D4;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8X5                = 0x93D5;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8X6                = 0x93D6;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_8X8                = 0x93D7;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10X5               = 0x93D8;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10X6               = 0x93D9;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10X8               = 0x93DA;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_10X10              = 0x93DB;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_12X10              = 0x93DC;
    const int GL_COMPRESSED_SRGB8_ALPHA8_ASTC_12X12              = 0x93DD;
}

// ========================================================================
// ========================================================================
