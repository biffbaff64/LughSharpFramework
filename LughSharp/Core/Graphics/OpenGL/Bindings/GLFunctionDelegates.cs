// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using GLenum = int;
using GLfloat = float;
using GLint = int;
using GLsizei = int;
using GLbitfield = uint;
using GLdouble = double;
using GLuint = uint;
using GLboolean = bool;
using GLubyte = byte;
using GLsizeiptr = int;
using GLintptr = int;
using GLshort = short;
using GLbyte = sbyte;
using GLushort = ushort;
using GLchar = byte;
using GLuint64 = ulong;
using GLint64 = long;

// ============================================================================

namespace LughSharp.Core.Graphics.OpenGL.Bindings;

[SuppressMessage( "ReSharper", "InconsistentNaming" )]
public partial class GLBindings
{
    private PFNGLACTIVESHADERPROGRAMPROC?                         _glActiveShaderProgram;
    private PFNGLACTIVETEXTUREPROC?                               _glActiveTexture;
    private PFNGLATTACHSHADERPROC?                                _glAttachShader;
    private PFNGLBEGINCONDITIONALRENDERPROC?                      _glBeginConditionalRender;
    private PFNGLBEGINQUERYPROC?                                  _glBeginQuery;
    private PFNGLBEGINQUERYINDEXEDPROC?                           _glBeginQueryIndexed;
    private PFNGLBEGINTRANSFORMFEEDBACKPROC?                      _glBeginTransformFeedback;
    private PFNGLBINDATTRIBLOCATIONPROC?                          _glBindAttribLocation;
    private PFNGLBINDBUFFERPROC?                                  _glBindBuffer;
    private PFNGLBINDBUFFERBASEPROC?                              _glBindBufferBase;
    private PFNGLBINDBUFFERRANGEPROC?                             _glBindBufferRange;
    private PFNGLBINDBUFFERSBASEPROC?                             _glBindBuffersBase;
    private PFNGLBINDBUFFERSRANGEPROC?                            _glBindBuffersRange;
    private PFNGLBINDFRAGDATALOCATIONPROC?                        _glBindFragDataLocation;
    private PFNGLBINDFRAGDATALOCATIONINDEXEDPROC?                 _glBindFragDataLocationIndexed;
    private PFNGLBINDFRAMEBUFFERPROC?                             _glBindFramebuffer;
    private PFNGLBINDIMAGETEXTUREPROC?                            _glBindImageTexture;
    private PFNGLBINDIMAGETEXTURESPROC?                           _glBindImageTextures;
    private PFNGLBINDPROGRAMPIPELINEPROC?                         _glBindProgramPipeline;
    private PFNGLBINDRENDERBUFFERPROC?                            _glBindRenderbuffer;
    private PFNGLBINDSAMPLERPROC?                                 _glBindSampler;
    private PFNGLBINDSAMPLERSPROC?                                _glBindSamplers;
    private PFNGLBINDTEXTUREPROC?                                 _glBindTexture;
    private PFNGLBINDTEXTURESPROC?                                _glBindTextures;
    private PFNGLBINDTEXTUREUNITPROC?                             _glBindTextureUnit;
    private PFNGLBINDTRANSFORMFEEDBACKPROC?                       _glBindTransformFeedback;
    private PFNGLBINDVERTEXARRAYPROC?                             _glBindVertexArray;
    private PFNGLBINDVERTEXBUFFERPROC?                            _glBindVertexBuffer;
    private PFNGLBINDVERTEXBUFFERSPROC?                           _glBindVertexBuffers;
    private PFNGLBLENDCOLORPROC?                                  _glBlendColor;
    private PFNGLBLENDEQUATIONPROC?                               _glBlendEquation;
    private PFNGLBLENDEQUATIONIPROC?                              _glBlendEquationi;
    private PFNGLBLENDEQUATIONSEPARATEPROC?                       _glBlendEquationSeparate;
    private PFNGLBLENDEQUATIONSEPARATEIPROC?                      _glBlendEquationSeparatei;
    private PFNGLBLENDFUNCPROC?                                   _glBlendFunc;
    private PFNGLBLENDFUNCIPROC?                                  _glBlendFunci;
    private PFNGLBLENDFUNCSEPARATEPROC?                           _glBlendFuncSeparate;
    private PFNGLBLENDFUNCSEPARATEIPROC?                          _glBlendFuncSeparatei;
    private PFNGLBLITFRAMEBUFFERPROC?                             _glBlitFramebuffer;
    private PFNGLBLITNAMEDFRAMEBUFFERPROC?                        _glBlitNamedFramebuffer;
    private PFNGLBUFFERDATAPROC?                                  _glBufferData;
    private PFNGLBUFFERSTORAGEPROC?                               _glBufferStorage;
    private PFNGLBUFFERSUBDATAPROC?                               _glBufferSubData;
    private PFNGLCHECKFRAMEBUFFERSTATUSPROC?                      _glCheckFramebufferStatus;
    private PFNGLCHECKNAMEDFRAMEBUFFERSTATUSPROC?                 _glCheckNamedFramebufferStatus;
    private PFNGLCLAMPCOLORPROC?                                  _glClampColor;
    private PFNGLCLEARPROC?                                       _glClear;
    private PFNGLCLEARBUFFERDATAPROC?                             _glClearBufferData;
    private PFNGLCLEARBUFFERFIPROC?                               _glClearBufferfi;
    private PFNGLCLEARBUFFERFVPROC?                               _glClearBufferfv;
    private PFNGLCLEARBUFFERIVPROC?                               _glClearBufferiv;
    private PFNGLCLEARBUFFERSUBDATAPROC?                          _glClearBufferSubData;
    private PFNGLCLEARBUFFERUIVPROC?                              _glClearBufferuiv;
    private PFNGLCLEARCOLORPROC?                                  _glClearColor;
    private PFNGLCLEARDEPTHPROC?                                  _glClearDepth;
    private PFNGLCLEARDEPTHFPROC?                                 _glClearDepthf;
    private PFNGLCLEARNAMEDBUFFERDATAPROC?                        _glClearNamedBufferData;
    private PFNGLCLEARNAMEDBUFFERSUBDATAPROC?                     _glClearNamedBufferSubData;
    private PFNGLCLEARNAMEDFRAMEBUFFERFIPROC?                     _glClearNamedFramebufferfi;
    private PFNGLCLEARNAMEDFRAMEBUFFERFVPROC?                     _glClearNamedFramebufferfv;
    private PFNGLCLEARNAMEDFRAMEBUFFERIVPROC?                     _glClearNamedFramebufferiv;
    private PFNGLCLEARNAMEDFRAMEBUFFERUIVPROC?                    _glClearNamedFramebufferuiv;
    private PFNGLCLEARSTENCILPROC?                                _glClearStencil;
    private PFNGLCLEARTEXIMAGEPROC?                               _glClearTexImage;
    private PFNGLCLEARTEXSUBIMAGEPROC?                            _glClearTexSubImage;
    private PFNGLCLIENTWAITSYNCPROC?                              _glClientWaitSync;
    private PFNGLCLIPCONTROLPROC?                                 _glClipControl;
    private PFNGLCOLORMASKPROC?                                   _glColorMask;
    private PFNGLCOLORMASKIPROC?                                  _glColorMaski;
    private PFNGLCOMPILESHADERPROC?                               _glCompileShader;
    private PFNGLCOMPRESSEDTEXIMAGE1DPROC?                        _glCompressedTexImage1D;
    private PFNGLCOMPRESSEDTEXIMAGE2DPROC?                        _glCompressedTexImage2D;
    private PFNGLCOMPRESSEDTEXIMAGE3DPROC?                        _glCompressedTexImage3D;
    private PFNGLCOMPRESSEDTEXSUBIMAGE1DPROC?                     _glCompressedTexSubImage1D;
    private PFNGLCOMPRESSEDTEXSUBIMAGE2DPROC?                     _glCompressedTexSubImage2D;
    private PFNGLCOMPRESSEDTEXSUBIMAGE3DPROC?                     _glCompressedTexSubImage3D;
    private PFNGLCOMPRESSEDTEXTURESUBIMAGE1DPROC?                 _glCompressedTextureSubImage1D;
    private PFNGLCOMPRESSEDTEXTURESUBIMAGE2DPROC?                 _glCompressedTextureSubImage2D;
    private PFNGLCOMPRESSEDTEXTURESUBIMAGE3DPROC?                 _glCompressedTextureSubImage3D;
    private PFNGLCOPYBUFFERSUBDATAPROC?                           _glCopyBufferSubData;
    private PFNGLCOPYIMAGESUBDATAPROC?                            _glCopyImageSubData;
    private PFNGLCOPYNAMEDBUFFERSUBDATAPROC?                      _glCopyNamedBufferSubData;
    private PFNGLCOPYTEXIMAGE1DPROC?                              _glCopyTexImage1D;
    private PFNGLCOPYTEXIMAGE2DPROC?                              _glCopyTexImage2D;
    private PFNGLCOPYTEXSUBIMAGE1DPROC?                           _glCopyTexSubImage1D;
    private PFNGLCOPYTEXSUBIMAGE2DPROC?                           _glCopyTexSubImage2D;
    private PFNGLCOPYTEXSUBIMAGE3DPROC?                           _glCopyTexSubImage3D;
    private PFNGLCOPYTEXTURESUBIMAGE1DPROC?                       _glCopyTextureSubImage1D;
    private PFNGLCOPYTEXTURESUBIMAGE2DPROC?                       _glCopyTextureSubImage2D;
    private PFNGLCOPYTEXTURESUBIMAGE3DPROC?                       _glCopyTextureSubImage3D;
    private PFNGLCREATEBUFFERSPROC?                               _glCreateBuffers;
    private PFNGLCREATEFRAMEBUFFERSPROC?                          _glCreateFramebuffers;
    private PFNGLCREATEPROGRAMPROC?                               _glCreateProgram;
    private PFNGLCREATEPROGRAMPIPELINESPROC?                      _glCreateProgramPipelines;
    private PFNGLCREATEQUERIESPROC?                               _glCreateQueries;
    private PFNGLCREATERENDERBUFFERSPROC?                         _glCreateRenderbuffers;
    private PFNGLCREATESAMPLERSPROC?                              _glCreateSamplers;
    private PFNGLCREATESHADERPROC?                                _glCreateShader;
    private PFNGLCREATESHADERPROGRAMVPROC?                        _glCreateShaderProgramv;
    private PFNGLCREATETEXTURESPROC?                              _glCreateTextures;
    private PFNGLCREATETRANSFORMFEEDBACKSPROC?                    _glCreateTransformFeedbacks;
    private PFNGLCREATEVERTEXARRAYSPROC?                          _glCreateVertexArrays;
    private PFNGLCULLFACEPROC?                                    _glCullFace;
    private PFNGLDEBUGMESSAGECALLBACKPROC?                        _glDebugMessageCallback;
    private PFNGLDEBUGMESSAGECONTROLPROC?                         _glDebugMessageControl;
    private PFNGLDEBUGMESSAGEINSERTPROC?                          _glDebugMessageInsert;
    private PFNGLDELETEBUFFERSPROC?                               _glDeleteBuffers;
    private PFNGLDELETEFRAMEBUFFERSPROC?                          _glDeleteFramebuffers;
    private PFNGLDELETEPROGRAMPROC?                               _glDeleteProgram;
    private PFNGLDELETEPROGRAMPIPELINESPROC?                      _glDeleteProgramPipelines;
    private PFNGLDELETEQUERIESPROC?                               _glDeleteQueries;
    private PFNGLDELETERENDERBUFFERSPROC?                         _glDeleteRenderbuffers;
    private PFNGLDELETESAMPLERSPROC?                              _glDeleteSamplers;
    private PFNGLDELETESHADERPROC?                                _glDeleteShader;
    private PFNGLDELETESYNCPROC?                                  _glDeleteSync;
    private PFNGLDELETETEXTURESPROC?                              _glDeleteTextures;
    private PFNGLDELETETRANSFORMFEEDBACKSPROC?                    _glDeleteTransformFeedbacks;
    private PFNGLDELETEVERTEXARRAYSPROC?                          _glDeleteVertexArrays;
    private PFNGLDEPTHFUNCPROC?                                   _glDepthFunc;
    private PFNGLDEPTHMASKPROC?                                   _glDepthMask;
    private PFNGLDEPTHRANGEPROC?                                  _glDepthRange;
    private PFNGLDEPTHRANGEARRAYVPROC?                            _glDepthRangeArrayv;
    private PFNGLDEPTHRANGEFPROC?                                 _glDepthRangef;
    private PFNGLDEPTHRANGEINDEXEDPROC?                           _glDepthRangeIndexed;
    private PFNGLDETACHSHADERPROC?                                _glDetachShader;
    private PFNGLDISABLEPROC?                                     _glDisable;
    private PFNGLDISABLEIPROC?                                    _glDisablei;
    private PFNGLDISABLEVERTEXARRAYATTRIBPROC?                    _glDisableVertexArrayAttrib;
    private PFNGLDISABLEVERTEXATTRIBARRAYPROC?                    _glDisableVertexAttribArray;
    private PFNGLDISPATCHCOMPUTEPROC?                             _glDispatchCompute;
    private PFNGLDISPATCHCOMPUTEINDIRECTPROC?                     _glDispatchComputeIndirect;
    private PFNGLDRAWARRAYSPROC?                                  _glDrawArrays;
    private PFNGLDRAWARRAYSINDIRECTPROC?                          _glDrawArraysIndirect;
    private PFNGLDRAWARRAYSINSTANCEDPROC?                         _glDrawArraysInstanced;
    private PFNGLDRAWARRAYSINSTANCEDBASEINSTANCEPROC?             _glDrawArraysInstancedBaseInstance;
    private PFNGLDRAWBUFFERPROC?                                  _glDrawBuffer;
    private PFNGLDRAWBUFFERSPROC?                                 _glDrawBuffers;
    private PFNGLDRAWELEMENTSPROC?                                _glDrawElements;
    private PFNGLDRAWELEMENTSBASEVERTEXPROC?                      _glDrawElementsBaseVertex;
    private PFNGLDRAWELEMENTSINDIRECTPROC?                        _glDrawElementsIndirect;
    private PFNGLDRAWELEMENTSINSTANCEDPROC?                       _glDrawElementsInstanced;
    private PFNGLDRAWELEMENTSINSTANCEDBASEINSTANCEPROC?           _glDrawElementsInstancedBaseInstance;
    private PFNGLDRAWELEMENTSINSTANCEDBASEVERTEXPROC?             _glDrawElementsInstancedBaseVertex;
    private PFNGLDRAWELEMENTSINSTANCEDBASEVERTEXBASEINSTANCEPROC? _glDrawElementsInstancedBaseVertexBaseInstance;
    private PFNGLDRAWRANGEELEMENTSPROC?                           _glDrawRangeElements;
    private PFNGLDRAWRANGEELEMENTSBASEVERTEXPROC?                 _glDrawRangeElementsBaseVertex;
    private PFNGLDRAWTRANSFORMFEEDBACKPROC?                       _glDrawTransformFeedback;
    private PFNGLDRAWTRANSFORMFEEDBACKINSTANCEDPROC?              _glDrawTransformFeedbackInstanced;
    private PFNGLDRAWTRANSFORMFEEDBACKSTREAMPROC?                 _glDrawTransformFeedbackStream;
    private PFNGLDRAWTRANSFORMFEEDBACKSTREAMINSTANCEDPROC?        _glDrawTransformFeedbackStreamInstanced;
    private PFNGLENABLEPROC?                                      _glEnable;
    private PFNGLENABLEIPROC?                                     _glEnablei;
    private PFNGLENABLEVERTEXARRAYATTRIBPROC?                     _glEnableVertexArrayAttrib;
    private PFNGLENABLEVERTEXATTRIBARRAYPROC?                     _glEnableVertexAttribArray;
    private PFNGLENDCONDITIONALRENDERPROC?                        _glEndConditionalRender;
    private PFNGLENDQUERYPROC?                                    _glEndQuery;
    private PFNGLENDQUERYINDEXEDPROC?                             _glEndQueryIndexed;
    private PFNGLENDTRANSFORMFEEDBACKPROC?                        _glEndTransformFeedback;
    private PFNGLFENCESYNCPROC?                                   _glFenceSync;
    private PFNGLFINISHPROC?                                      _glFinish;
    private PFNGLFLUSHPROC?                                       _glFlush;
    private PFNGLFLUSHMAPPEDBUFFERRANGEPROC?                      _glFlushMappedBufferRange;
    private PFNGLFLUSHMAPPEDNAMEDBUFFERRANGEPROC?                 _glFlushMappedNamedBufferRange;
    private PFNGLFRAMEBUFFERPARAMETERIPROC?                       _glFramebufferParameteri;
    private PFNGLFRAMEBUFFERRENDERBUFFERPROC?                     _glFramebufferRenderbuffer;
    private PFNGLFRAMEBUFFERTEXTUREPROC?                          _glFramebufferTexture;
    private PFNGLFRAMEBUFFERTEXTURE1DPROC?                        _glFramebufferTexture1D;
    private PFNGLFRAMEBUFFERTEXTURE2DPROC?                        _glFramebufferTexture2D;
    private PFNGLFRAMEBUFFERTEXTURE3DPROC?                        _glFramebufferTexture3D;
    private PFNGLFRAMEBUFFERTEXTURELAYERPROC?                     _glFramebufferTextureLayer;
    private PFNGLFRONTFACEPROC?                                   _glFrontFace;
    private PFNGLGENBUFFERSPROC?                                  _glGenBuffers;
    private PFNGLGENERATEMIPMAPPROC?                              _glGenerateMipmap;
    private PFNGLGENERATETEXTUREMIPMAPPROC?                       _glGenerateTextureMipmap;
    private PFNGLGENFRAMEBUFFERSPROC?                             _glGenFramebuffers;
    private PFNGLGENPROGRAMPIPELINESPROC?                         _glGenProgramPipelines;
    private PFNGLGENQUERIESPROC?                                  _glGenQueries;
    private PFNGLGENRENDERBUFFERSPROC?                            _glGenRenderbuffers;
    private PFNGLGENSAMPLERSPROC?                                 _glGenSamplers;
    private PFNGLGENTEXTURESPROC?                                 _glGenTextures;
    private PFNGLGENTRANSFORMFEEDBACKSPROC?                       _glGenTransformFeedbacks;
    private PFNGLGENVERTEXARRAYSPROC?                             _glGenVertexArrays;
    private PFNGLGETACTIVEATOMICCOUNTERBUFFERIVPROC?              _glGetActiveAtomicCounterBufferiv;
    private PFNGLGETACTIVEATTRIBPROC?                             _glGetActiveAttrib;
    private PFNGLGETACTIVESUBROUTINENAMEPROC?                     _glGetActiveSubroutineName;
    private PFNGLGETACTIVESUBROUTINEUNIFORMIVPROC?                _glGetActiveSubroutineUniformiv;
    private PFNGLGETACTIVESUBROUTINEUNIFORMNAMEPROC?              _glGetActiveSubroutineUniformName;
    private PFNGLGETACTIVEUNIFORMPROC?                            _glGetActiveUniform;
    private PFNGLGETACTIVEUNIFORMBLOCKIVPROC?                     _glGetActiveUniformBlockiv;
    private PFNGLGETACTIVEUNIFORMBLOCKNAMEPROC?                   _glGetActiveUniformBlockName;
    private PFNGLGETACTIVEUNIFORMNAMEPROC?                        _glGetActiveUniformName;
    private PFNGLGETACTIVEUNIFORMSIVPROC?                         _glGetActiveUniformsiv;
    private PFNGLGETATTACHEDSHADERSPROC?                          _glGetAttachedShaders;
    private PFNGLGETATTRIBLOCATIONPROC?                           _glGetAttribLocation;
    private PFNGLGETBOOLEANI_VPROC?                               _glGetBooleani_v;
    private PFNGLGETBOOLEANVPROC?                                 _glGetBooleanv;
    private PFNGLGETBUFFERPARAMETERI64VPROC?                      _glGetBufferParameteri64v;
    private PFNGLGETBUFFERPARAMETERIVPROC?                        _glGetBufferParameteriv;
    private PFNGLGETBUFFERPOINTERVPROC?                           _glGetBufferPointerv;
    private PFNGLGETBUFFERSUBDATAPROC?                            _glGetBufferSubData;
    private PFNGLGETCOMPRESSEDTEXIMAGEPROC?                       _glGetCompressedTexImage;
    private PFNGLGETCOMPRESSEDTEXTUREIMAGEPROC?                   _glGetCompressedTextureImage;
    private PFNGLGETCOMPRESSEDTEXTURESUBIMAGEPROC?                _glGetCompressedTextureSubImage;
    private PFNGLGETDEBUGMESSAGELOGPROC?                          _glGetDebugMessageLog;
    private PFNGLGETDOUBLEI_VPROC?                                _glGetDoublei_v;
    private PFNGLGETDOUBLEVPROC?                                  _glGetDoublev;
    private PFNGLGETERRORPROC?                                    _glGetError;
    private PFNGLGETFLOATI_VPROC?                                 _glGetFloati_v;
    private PFNGLGETFLOATVPROC?                                   _glGetFloatv;
    private PFNGLGETFRAGDATAINDEXPROC?                            _glGetFragDataIndex;
    private PFNGLGETFRAGDATALOCATIONPROC?                         _glGetFragDataLocation;
    private PFNGLGETFRAMEBUFFERATTACHMENTPARAMETERIVPROC?         _glGetFramebufferAttachmentParameteriv;
    private PFNGLGETFRAMEBUFFERPARAMETERIVPROC?                   _glGetFramebufferParameteriv;
    private PFNGLGETGRAPHICSRESETSTATUSPROC?                      _glGetGraphicsResetStatus;
    private PFNGLGETINTEGER64I_VPROC?                             _glGetInteger64i_v;
    private PFNGLGETINTEGER64VPROC?                               _glGetInteger64v;
    private PFNGLGETINTEGERI_VPROC?                               _glGetIntegeri_v;
    private PFNGLGETINTEGERVPROC?                                 _glGetIntegerv;
    private PFNGLGETINTERNALFORMATI64VPROC?                       _glGetInternalformati64v;
    private PFNGLGETINTERNALFORMATIVPROC?                         _glGetInternalformativ;
    private PFNGLGETMULTISAMPLEFVPROC?                            _glGetMultisamplefv;
    private PFNGLGETNAMEDBUFFERPARAMETERI64VPROC?                 _glGetNamedBufferParameteri64v;
    private PFNGLGETNAMEDBUFFERPARAMETERIVPROC?                   _glGetNamedBufferParameteriv;
    private PFNGLGETNAMEDBUFFERPOINTERVPROC?                      _glGetNamedBufferPointerv;
    private PFNGLGETNAMEDBUFFERSUBDATAPROC?                       _glGetNamedBufferSubData;
    private PFNGLGETNAMEDFRAMEBUFFERATTACHMENTPARAMETERIVPROC?    _glGetNamedFramebufferAttachmentParameteriv;
    private PFNGLGETNAMEDFRAMEBUFFERPARAMETERIVPROC?              _glGetNamedFramebufferParameteriv;
    private PFNGLGETNAMEDRENDERBUFFERPARAMETERIVPROC?             _glGetNamedRenderbufferParameteriv;
    private PFNGLGETNCOMPRESSEDTEXIMAGEPROC?                      _glGetnCompressedTexImage;
    private PFNGLGETNTEXIMAGEPROC?                                _glGetnTexImage;
    private PFNGLGETNUNIFORMDVPROC?                               _glGetnUniformdv;
    private PFNGLGETNUNIFORMFVPROC?                               _glGetnUniformfv;
    private PFNGLGETNUNIFORMIVPROC?                               _glGetnUniformiv;
    private PFNGLGETNUNIFORMUIVPROC?                              _glGetnUniformuiv;
    private PFNGLGETOBJECTLABELPROC?                              _glGetObjectLabel;
    private PFNGLGETOBJECTPTRLABELPROC?                           _glGetObjectPtrLabel;
    private PFNGLGETPOINTERVPROC?                                 _glGetPointerv;
    private PFNGLGETPROGRAMBINARYPROC?                            _glGetProgramBinary;
    private PFNGLGETPROGRAMINFOLOGPROC?                           _glGetProgramInfoLog;
    private PFNGLGETPROGRAMINTERFACEIVPROC?                       _glGetProgramInterfaceiv;
    private PFNGLGETPROGRAMIVPROC?                                _glGetProgramiv;
    private PFNGLGETPROGRAMPIPELINEINFOLOGPROC?                   _glGetProgramPipelineInfoLog;
    private PFNGLGETPROGRAMPIPELINEIVPROC?                        _glGetProgramPipelineiv;
    private PFNGLGETPROGRAMRESOURCEINDEXPROC?                     _glGetProgramResourceIndex;
    private PFNGLGETPROGRAMRESOURCEIVPROC?                        _glGetProgramResourceiv;
    private PFNGLGETPROGRAMRESOURCELOCATIONPROC?                  _glGetProgramResourceLocation;
    private PFNGLGETPROGRAMRESOURCELOCATIONINDEXPROC?             _glGetProgramResourceLocationIndex;
    private PFNGLGETPROGRAMRESOURCENAMEPROC?                      _glGetProgramResourceName;
    private PFNGLGETPROGRAMSTAGEIVPROC?                           _glGetProgramStageiv;
    private PFNGLGETQUERYBUFFEROBJECTI64VPROC?                    _glGetQueryBufferObjecti64v;
    private PFNGLGETQUERYBUFFEROBJECTIVPROC?                      _glGetQueryBufferObjectiv;
    private PFNGLGETQUERYBUFFEROBJECTUI64VPROC?                   _glGetQueryBufferObjectui64v;
    private PFNGLGETQUERYBUFFEROBJECTUIVPROC?                     _glGetQueryBufferObjectuiv;
    private PFNGLGETQUERYINDEXEDIVPROC?                           _glGetQueryIndexediv;
    private PFNGLGETQUERYIVPROC?                                  _glGetQueryiv;
    private PFNGLGETQUERYOBJECTI64VPROC?                          _glGetQueryObjecti64v;
    private PFNGLGETQUERYOBJECTIVPROC?                            _glGetQueryObjectiv;
    private PFNGLGETQUERYOBJECTUI64VPROC?                         _glGetQueryObjectui64v;
    private PFNGLGETQUERYOBJECTUIVPROC?                           _glGetQueryObjectuiv;
    private PFNGLGETRENDERBUFFERPARAMETERIVPROC?                  _glGetRenderbufferParameteriv;
    private PFNGLGETSAMPLERPARAMETERFVPROC?                       _glGetSamplerParameterfv;
    private PFNGLGETSAMPLERPARAMETERIIVPROC?                      _glGetSamplerParameterIiv;
    private PFNGLGETSAMPLERPARAMETERIUIVPROC?                     _glGetSamplerParameterIuiv;
    private PFNGLGETSAMPLERPARAMETERIVPROC?                       _glGetSamplerParameteriv;
    private PFNGLGETSHADERINFOLOGPROC?                            _glGetShaderInfoLog;
    private PFNGLGETSHADERIVPROC?                                 _glGetShaderiv;
    private PFNGLGETSHADERPRECISIONFORMATPROC?                    _glGetShaderPrecisionFormat;
    private PFNGLGETSHADERSOURCEPROC?                             _glGetShaderSource;
    private PFNGLGETSTRINGPROC?                                   _glGetString;
    private PFNGLGETSTRINGIPROC?                                  _glGetStringi;
    private PFNGLGETSUBROUTINEINDEXPROC?                          _glGetSubroutineIndex;
    private PFNGLGETSUBROUTINEUNIFORMLOCATIONPROC?                _glGetSubroutineUniformLocation;
    private PFNGLGETSYNCIVPROC?                                   _glGetSynciv;
    private PFNGLGETTEXIMAGEPROC?                                 _glGetTexImage;
    private PFNGLGETTEXLEVELPARAMETERFVPROC?                      _glGetTexLevelParameterfv;
    private PFNGLGETTEXLEVELPARAMETERIVPROC?                      _glGetTexLevelParameteriv;
    private PFNGLGETTEXPARAMETERFVPROC?                           _glGetTexParameterfv;
    private PFNGLGETTEXPARAMETERIIVPROC?                          _glGetTexParameterIiv;
    private PFNGLGETTEXPARAMETERIUIVPROC?                         _glGetTexParameterIuiv;
    private PFNGLGETTEXPARAMETERIVPROC?                           _glGetTexParameteriv;
    private PFNGLGETTEXTUREIMAGEPROC?                             _glGetTextureImage;
    private PFNGLGETTEXTURELEVELPARAMETERFVPROC?                  _glGetTextureLevelParameterfv;
    private PFNGLGETTEXTURELEVELPARAMETERIVPROC?                  _glGetTextureLevelParameteriv;
    private PFNGLGETTEXTUREPARAMETERFVPROC?                       _glGetTextureParameterfv;
    private PFNGLGETTEXTUREPARAMETERIIVPROC?                      _glGetTextureParameterIiv;
    private PFNGLGETTEXTUREPARAMETERIUIVPROC?                     _glGetTextureParameterIuiv;
    private PFNGLGETTEXTUREPARAMETERIVPROC?                       _glGetTextureParameteriv;
    private PFNGLGETTEXTURESUBIMAGEPROC?                          _glGetTextureSubImage;
    private PFNGLGETTRANSFORMFEEDBACKI_VPROC?                     _glGetTransformFeedbacki_v;
    private PFNGLGETTRANSFORMFEEDBACKI64_VPROC?                   _glGetTransformFeedbacki64_v;
    private PFNGLGETTRANSFORMFEEDBACKIVPROC?                      _glGetTransformFeedbackiv;
    private PFNGLGETTRANSFORMFEEDBACKVARYINGPROC?                 _glGetTransformFeedbackVarying;
    private PFNGLGETUNIFORMBLOCKINDEXPROC?                        _glGetUniformBlockIndex;
    private PFNGLGETUNIFORMDVPROC?                                _glGetUniformdv;
    private PFNGLGETUNIFORMFVPROC?                                _glGetUniformfv;
    private PFNGLGETUNIFORMINDICESPROC?                           _glGetUniformIndices;
    private PFNGLGETUNIFORMIVPROC?                                _glGetUniformiv;
    private PFNGLGETUNIFORMLOCATIONPROC?                          _glGetUniformLocation;
    private PFNGLGETUNIFORMSUBROUTINEUIVPROC?                     _glGetUniformSubroutineuiv;
    private PFNGLGETUNIFORMUIVPROC?                               _glGetUniformuiv;
    private PFNGLGETVERTEXARRAYINDEXED64IVPROC?                   _glGetVertexArrayIndexed64iv;
    private PFNGLGETVERTEXARRAYINDEXEDIVPROC?                     _glGetVertexArrayIndexediv;
    private PFNGLGETVERTEXARRAYIVPROC?                            _glGetVertexArrayiv;
    private PFNGLGETVERTEXATTRIBDVPROC?                           _glGetVertexAttribdv;
    private PFNGLGETVERTEXATTRIBFVPROC?                           _glGetVertexAttribfv;
    private PFNGLGETVERTEXATTRIBIIVPROC?                          _glGetVertexAttribIiv;
    private PFNGLGETVERTEXATTRIBIUIVPROC?                         _glGetVertexAttribIuiv;
    private PFNGLGETVERTEXATTRIBIVPROC?                           _glGetVertexAttribiv;
    private PFNGLGETVERTEXATTRIBLDVPROC?                          _glGetVertexAttribLdv;
    private PFNGLGETVERTEXATTRIBPOINTERVPROC?                     _glGetVertexAttribPointerv;
    private PFNGLHINTPROC?                                        _glHint;
    private PFNGLINVALIDATEBUFFERDATAPROC?                        _glInvalidateBufferData;
    private PFNGLINVALIDATEBUFFERSUBDATAPROC?                     _glInvalidateBufferSubData;
    private PFNGLINVALIDATEFRAMEBUFFERPROC?                       _glInvalidateFramebuffer;
    private PFNGLINVALIDATENAMEDFRAMEBUFFERDATAPROC?              _glInvalidateNamedFramebufferData;
    private PFNGLINVALIDATENAMEDFRAMEBUFFERSUBDATAPROC?           _glInvalidateNamedFramebufferSubData;
    private PFNGLINVALIDATESUBFRAMEBUFFERPROC?                    _glInvalidateSubFramebuffer;
    private PFNGLINVALIDATETEXIMAGEPROC?                          _glInvalidateTexImage;
    private PFNGLINVALIDATETEXSUBIMAGEPROC?                       _glInvalidateTexSubImage;
    private PFNGLISBUFFERPROC?                                    _glIsBuffer;
    private PFNGLISENABLEDPROC?                                   _glIsEnabled;
    private PFNGLISENABLEDIPROC?                                  _glIsEnabledi;
    private PFNGLISFRAMEBUFFERPROC?                               _glIsFramebuffer;
    private PFNGLISPROGRAMPROC?                                   _glIsProgram;
    private PFNGLISPROGRAMPIPELINEPROC?                           _glIsProgramPipeline;
    private PFNGLISQUERYPROC?                                     _glIsQuery;
    private PFNGLISRENDERBUFFERPROC?                              _glIsRenderbuffer;
    private PFNGLISSAMPLERPROC?                                   _glIsSampler;
    private PFNGLISSHADERPROC?                                    _glIsShader;
    private PFNGLISSYNCPROC?                                      _glIsSync;
    private PFNGLISTEXTUREPROC?                                   _glIsTexture;
    private PFNGLISTRANSFORMFEEDBACKPROC?                         _glIsTransformFeedback;
    private PFNGLISVERTEXARRAYPROC?                               _glIsVertexArray;
    private PFNGLLINEWIDTHPROC?                                   _glLineWidth;
    private PFNGLLINKPROGRAMPROC?                                 _glLinkProgram;
    private PFNGLLOGICOPPROC?                                     _glLogicOp;
    private PFNGLMAPBUFFERPROC?                                   _glMapBuffer;
    private PFNGLMAPBUFFERRANGEPROC?                              _glMapBufferRange;
    private PFNGLMAPNAMEDBUFFERPROC?                              _glMapNamedBuffer;
    private PFNGLMAPNAMEDBUFFERRANGEPROC?                         _glMapNamedBufferRange;
    private PFNGLMEMORYBARRIERPROC?                               _glMemoryBarrier;
    private PFNGLMEMORYBARRIERBYREGIONPROC?                       _glMemoryBarrierByRegion;
    private PFNGLMINSAMPLESHADINGPROC?                            _glMinSampleShading;
    private PFNGLMULTIDRAWARRAYSPROC?                             _glMultiDrawArrays;
    private PFNGLMULTIDRAWARRAYSINDIRECTPROC?                     _glMultiDrawArraysIndirect;
    private PFNGLMULTIDRAWARRAYSINDIRECTCOUNTPROC?                _glMultiDrawArraysIndirectCount;
    private PFNGLMULTIDRAWELEMENTSPROC?                           _glMultiDrawElements;
    private PFNGLMULTIDRAWELEMENTSBASEVERTEXPROC?                 _glMultiDrawElementsBaseVertex;
    private PFNGLMULTIDRAWELEMENTSINDIRECTPROC?                   _glMultiDrawElementsIndirect;
    private PFNGLMULTIDRAWELEMENTSINDIRECTCOUNTPROC?              _glMultiDrawElementsIndirectCount;
    private PFNGLNAMEDBUFFERDATAPROC?                             _glNamedBufferData;
    private PFNGLNAMEDBUFFERSTORAGEPROC?                          _glNamedBufferStorage;
    private PFNGLNAMEDBUFFERSUBDATAPROC?                          _glNamedBufferSubData;
    private PFNGLNAMEDFRAMEBUFFERDRAWBUFFERPROC?                  _glNamedFramebufferDrawBuffer;
    private PFNGLNAMEDFRAMEBUFFERDRAWBUFFERSPROC?                 _glNamedFramebufferDrawBuffers;
    private PFNGLNAMEDFRAMEBUFFERPARAMETERIPROC?                  _glNamedFramebufferParameteri;
    private PFNGLNAMEDFRAMEBUFFERREADBUFFERPROC?                  _glNamedFramebufferReadBuffer;
    private PFNGLNAMEDFRAMEBUFFERRENDERBUFFERPROC?                _glNamedFramebufferRenderbuffer;
    private PFNGLNAMEDFRAMEBUFFERTEXTUREPROC?                     _glNamedFramebufferTexture;
    private PFNGLNAMEDFRAMEBUFFERTEXTURELAYERPROC?                _glNamedFramebufferTextureLayer;
    private PFNGLNAMEDRENDERBUFFERSTORAGEPROC?                    _glNamedRenderbufferStorage;
    private PFNGLNAMEDRENDERBUFFERSTORAGEMULTISAMPLEPROC?         _glNamedRenderbufferStorageMultisample;
    private PFNGLOBJECTLABELPROC?                                 _glObjectLabel;
    private PFNGLOBJECTPTRLABELPROC?                              _glObjectPtrLabel;
    private PFNGLPATCHPARAMETERFVPROC?                            _glPatchParameterfv;
    private PFNGLPATCHPARAMETERIPROC?                             _glPatchParameteri;
    private PFNGLPAUSETRANSFORMFEEDBACKPROC?                      _glPauseTransformFeedback;
    private PFNGLPIXELSTOREFPROC?                                 _glPixelStoref;
    private PFNGLPIXELSTOREIPROC?                                 _glPixelStorei;
    private PFNGLPOINTPARAMETERFPROC?                             _glPointParameterf;
    private PFNGLPOINTPARAMETERFVPROC?                            _glPointParameterfv;
    private PFNGLPOINTPARAMETERIPROC?                             _glPointParameteri;
    private PFNGLPOINTPARAMETERIVPROC?                            _glPointParameteriv;
    private PFNGLPOINTSIZEPROC?                                   _glPointSize;
    private PFNGLPOLYGONMODEPROC?                                 _glPolygonMode;
    private PFNGLPOLYGONOFFSETPROC?                               _glPolygonOffset;
    private PFNGLPOLYGONOFFSETCLAMPPROC?                          _glPolygonOffsetClamp;
    private PFNGLPOPDEBUGGROUPPROC?                               _glPopDebugGroup;
    private PFNGLPRIMITIVERESTARTINDEXPROC?                       _glPrimitiveRestartIndex;
    private PFNGLPROGRAMBINARYPROC?                               _glProgramBinary;
    private PFNGLPROGRAMPARAMETERIPROC?                           _glProgramParameteri;
    private PFNGLPROGRAMUNIFORM1DPROC?                            _glProgramUniform1d;
    private PFNGLPROGRAMUNIFORM1DVPROC?                           _glProgramUniform1dv;
    private PFNGLPROGRAMUNIFORM1FPROC?                            _glProgramUniform1f;
    private PFNGLPROGRAMUNIFORM1FVPROC?                           _glProgramUniform1fv;
    private PFNGLPROGRAMUNIFORM1IPROC?                            _glProgramUniform1i;
    private PFNGLPROGRAMUNIFORM1IVPROC?                           _glProgramUniform1iv;
    private PFNGLPROGRAMUNIFORM1UIPROC?                           _glProgramUniform1ui;
    private PFNGLPROGRAMUNIFORM1UIVPROC?                          _glProgramUniform1uiv;
    private PFNGLPROGRAMUNIFORM2DPROC?                            _glProgramUniform2d;
    private PFNGLPROGRAMUNIFORM2DVPROC?                           _glProgramUniform2dv;
    private PFNGLPROGRAMUNIFORM2FPROC?                            _glProgramUniform2f;
    private PFNGLPROGRAMUNIFORM2FVPROC?                           _glProgramUniform2fv;
    private PFNGLPROGRAMUNIFORM2IPROC?                            _glProgramUniform2i;
    private PFNGLPROGRAMUNIFORM2IVPROC?                           _glProgramUniform2iv;
    private PFNGLPROGRAMUNIFORM2UIPROC?                           _glProgramUniform2ui;
    private PFNGLPROGRAMUNIFORM2UIVPROC?                          _glProgramUniform2uiv;
    private PFNGLPROGRAMUNIFORM3DPROC?                            _glProgramUniform3d;
    private PFNGLPROGRAMUNIFORM3DVPROC?                           _glProgramUniform3dv;
    private PFNGLPROGRAMUNIFORM3FPROC?                            _glProgramUniform3f;
    private PFNGLPROGRAMUNIFORM3FVPROC?                           _glProgramUniform3fv;
    private PFNGLPROGRAMUNIFORM3IPROC?                            _glProgramUniform3i;
    private PFNGLPROGRAMUNIFORM3IVPROC?                           _glProgramUniform3iv;
    private PFNGLPROGRAMUNIFORM3UIPROC?                           _glProgramUniform3ui;
    private PFNGLPROGRAMUNIFORM3UIVPROC?                          _glProgramUniform3uiv;
    private PFNGLPROGRAMUNIFORM4DPROC?                            _glProgramUniform4d;
    private PFNGLPROGRAMUNIFORM4DVPROC?                           _glProgramUniform4dv;
    private PFNGLPROGRAMUNIFORM4FPROC?                            _glProgramUniform4f;
    private PFNGLPROGRAMUNIFORM4FVPROC?                           _glProgramUniform4fv;
    private PFNGLPROGRAMUNIFORM4IPROC?                            _glProgramUniform4i;
    private PFNGLPROGRAMUNIFORM4IVPROC?                           _glProgramUniform4iv;
    private PFNGLPROGRAMUNIFORM4UIPROC?                           _glProgramUniform4ui;
    private PFNGLPROGRAMUNIFORM4UIVPROC?                          _glProgramUniform4uiv;
    private PFNGLPROGRAMUNIFORMMATRIX2DVPROC?                     _glProgramUniformMatrix2dv;
    private PFNGLPROGRAMUNIFORMMATRIX2FVPROC?                     _glProgramUniformMatrix2fv;
    private PFNGLPROGRAMUNIFORMMATRIX2X3DVPROC?                   _glProgramUniformMatrix2x3dv;
    private PFNGLPROGRAMUNIFORMMATRIX2X3FVPROC?                   _glProgramUniformMatrix2x3fv;
    private PFNGLPROGRAMUNIFORMMATRIX2X4DVPROC?                   _glProgramUniformMatrix2x4dv;
    private PFNGLPROGRAMUNIFORMMATRIX2X4FVPROC?                   _glProgramUniformMatrix2x4fv;
    private PFNGLPROGRAMUNIFORMMATRIX3DVPROC?                     _glProgramUniformMatrix3dv;
    private PFNGLPROGRAMUNIFORMMATRIX3FVPROC?                     _glProgramUniformMatrix3fv;
    private PFNGLPROGRAMUNIFORMMATRIX3X2DVPROC?                   _glProgramUniformMatrix3x2dv;
    private PFNGLPROGRAMUNIFORMMATRIX3X2FVPROC?                   _glProgramUniformMatrix3x2fv;
    private PFNGLPROGRAMUNIFORMMATRIX3X4DVPROC?                   _glProgramUniformMatrix3x4dv;
    private PFNGLPROGRAMUNIFORMMATRIX3X4FVPROC?                   _glProgramUniformMatrix3x4fv;
    private PFNGLPROGRAMUNIFORMMATRIX4DVPROC?                     _glProgramUniformMatrix4dv;
    private PFNGLPROGRAMUNIFORMMATRIX4FVPROC?                     _glProgramUniformMatrix4fv;
    private PFNGLPROGRAMUNIFORMMATRIX4X2DVPROC?                   _glProgramUniformMatrix4x2dv;
    private PFNGLPROGRAMUNIFORMMATRIX4X2FVPROC?                   _glProgramUniformMatrix4x2fv;
    private PFNGLPROGRAMUNIFORMMATRIX4X3DVPROC?                   _glProgramUniformMatrix4x3dv;
    private PFNGLPROGRAMUNIFORMMATRIX4X3FVPROC?                   _glProgramUniformMatrix4x3fv;
    private PFNGLPROVOKINGVERTEXPROC?                             _glProvokingVertex;
    private PFNGLPUSHDEBUGGROUPPROC?                              _glPushDebugGroup;
    private PFNGLQUERYCOUNTERPROC?                                _glQueryCounter;
    private PFNGLREADBUFFERPROC?                                  _glReadBuffer;
    private PFNGLREADNPIXELSPROC?                                 _glReadnPixels;
    private PFNGLREADPIXELSPROC?                                  _glReadPixels;
    private PFNGLRELEASESHADERCOMPILERPROC?                       _glReleaseShaderCompiler;
    private PFNGLRENDERBUFFERSTORAGEPROC?                         _glRenderbufferStorage;
    private PFNGLRENDERBUFFERSTORAGEMULTISAMPLEPROC?              _glRenderbufferStorageMultisample;
    private PFNGLRESUMETRANSFORMFEEDBACKPROC?                     _glResumeTransformFeedback;
    private PFNGLSAMPLECOVERAGEPROC?                              _glSampleCoverage;
    private PFNGLSAMPLEMASKIPROC?                                 _glSampleMaski;
    private PFNGLSAMPLERPARAMETERFPROC?                           _glSamplerParameterf;
    private PFNGLSAMPLERPARAMETERFVPROC?                          _glSamplerParameterfv;
    private PFNGLSAMPLERPARAMETERIPROC?                           _glSamplerParameteri;
    private PFNGLSAMPLERPARAMETERIIVPROC?                         _glSamplerParameterIiv;
    private PFNGLSAMPLERPARAMETERIUIVPROC?                        _glSamplerParameterIuiv;
    private PFNGLSAMPLERPARAMETERIVPROC?                          _glSamplerParameteriv;
    private PFNGLSCISSORPROC?                                     _glScissor;
    private PFNGLSCISSORARRAYVPROC?                               _glScissorArrayv;
    private PFNGLSCISSORINDEXEDPROC?                              _glScissorIndexed;
    private PFNGLSCISSORINDEXEDVPROC?                             _glScissorIndexedv;
    private PFNGLSHADERBINARYPROC?                                _glShaderBinary;
    private PFNGLSHADERSOURCEPROC?                                _glShaderSource;
    private PFNGLSHADERSTORAGEBLOCKBINDINGPROC?                   _glShaderStorageBlockBinding;
    private PFNGLSPECIALIZESHADERPROC?                            _glSpecializeShader;
    private PFNGLSTENCILFUNCPROC?                                 _glStencilFunc;
    private PFNGLSTENCILFUNCSEPARATEPROC?                         _glStencilFuncSeparate;
    private PFNGLSTENCILMASKPROC?                                 _glStencilMask;
    private PFNGLSTENCILMASKSEPARATEPROC?                         _glStencilMaskSeparate;
    private PFNGLSTENCILOPPROC?                                   _glStencilOp;
    private PFNGLSTENCILOPSEPARATEPROC?                           _glStencilOpSeparate;
    private PFNGLTEXBUFFERPROC?                                   _glTexBuffer;
    private PFNGLTEXBUFFERRANGEPROC?                              _glTexBufferRange;
    private PFNGLTEXIMAGE1DPROC?                                  _glTexImage1D;
    private PFNGLTEXIMAGE2DPROC?                                  _glTexImage2D;
    private PFNGLTEXIMAGE2DMULTISAMPLEPROC?                       _glTexImage2DMultisample;
    private PFNGLTEXIMAGE3DPROC?                                  _glTexImage3D;
    private PFNGLTEXIMAGE3DMULTISAMPLEPROC?                       _glTexImage3DMultisample;
    private PFNGLTEXPARAMETERFPROC?                               _glTexParameterf;
    private PFNGLTEXPARAMETERFVPROC?                              _glTexParameterfv;
    private PFNGLTEXPARAMETERIPROC?                               _glTexParameteri;
    private PFNGLTEXPARAMETERIIVPROC?                             _glTexParameterIiv;
    private PFNGLTEXPARAMETERIUIVPROC?                            _glTexParameterIuiv;
    private PFNGLTEXPARAMETERIVPROC?                              _glTexParameteriv;
    private PFNGLTEXSTORAGE1DPROC?                                _glTexStorage1D;
    private PFNGLTEXSTORAGE2DPROC?                                _glTexStorage2D;
    private PFNGLTEXSTORAGE2DMULTISAMPLEPROC?                     _glTexStorage2DMultisample;
    private PFNGLTEXSTORAGE3DPROC?                                _glTexStorage3D;
    private PFNGLTEXSTORAGE3DMULTISAMPLEPROC?                     _glTexStorage3DMultisample;
    private PFNGLTEXSUBIMAGE1DPROC?                               _glTexSubImage1D;
    private PFNGLTEXSUBIMAGE2DPROC?                               _glTexSubImage2D;
    private PFNGLTEXSUBIMAGE3DPROC?                               _glTexSubImage3D;
    private PFNGLTEXTUREBARRIERPROC?                              _glTextureBarrier;
    private PFNGLTEXTUREBUFFERPROC?                               _glTextureBuffer;
    private PFNGLTEXTUREBUFFERRANGEPROC?                          _glTextureBufferRange;
    private PFNGLTEXTUREPARAMETERFPROC?                           _glTextureParameterf;
    private PFNGLTEXTUREPARAMETERFVPROC?                          _glTextureParameterfv;
    private PFNGLTEXTUREPARAMETERIPROC?                           _glTextureParameteri;
    private PFNGLTEXTUREPARAMETERIIVPROC?                         _glTextureParameterIiv;
    private PFNGLTEXTUREPARAMETERIUIVPROC?                        _glTextureParameterIuiv;
    private PFNGLTEXTUREPARAMETERIVPROC?                          _glTextureParameteriv;
    private PFNGLTEXTURESTORAGE1DPROC?                            _glTextureStorage1D;
    private PFNGLTEXTURESTORAGE2DPROC?                            _glTextureStorage2D;
    private PFNGLTEXTURESTORAGE2DMULTISAMPLEPROC?                 _glTextureStorage2DMultisample;
    private PFNGLTEXTURESTORAGE3DPROC?                            _glTextureStorage3D;
    private PFNGLTEXTURESTORAGE3DMULTISAMPLEPROC?                 _glTextureStorage3DMultisample;
    private PFNGLTEXTURESUBIMAGE1DPROC?                           _glTextureSubImage1D;
    private PFNGLTEXTURESUBIMAGE2DPROC?                           _glTextureSubImage2D;
    private PFNGLTEXTURESUBIMAGE3DPROC?                           _glTextureSubImage3D;
    private PFNGLTEXTUREVIEWPROC?                                 _glTextureView;
    private PFNGLTRANSFORMFEEDBACKBUFFERBASEPROC?                 _glTransformFeedbackBufferBase;
    private PFNGLTRANSFORMFEEDBACKBUFFERRANGEPROC?                _glTransformFeedbackBufferRange;
    private PFNGLTRANSFORMFEEDBACKVARYINGSPROC?                   _glTransformFeedbackVaryings;
    private PFNGLUNIFORM1DPROC?                                   _glUniform1d;
    private PFNGLUNIFORM1DVPROC?                                  _glUniform1dv;
    private PFNGLUNIFORM1FPROC?                                   _glUniform1f;
    private PFNGLUNIFORM1FVPROC?                                  _glUniform1fv;
    private PFNGLUNIFORM1IPROC?                                   _glUniform1i;
    private PFNGLUNIFORM1IVPROC?                                  _glUniform1iv;
    private PFNGLUNIFORM1UIPROC?                                  _glUniform1ui;
    private PFNGLUNIFORM1UIVPROC?                                 _glUniform1uiv;
    private PFNGLUNIFORM2DPROC?                                   _glUniform2d;
    private PFNGLUNIFORM2DVPROC?                                  _glUniform2dv;
    private PFNGLUNIFORM2FPROC?                                   _glUniform2f;
    private PFNGLUNIFORM2FVPROC?                                  _glUniform2fv;
    private PFNGLUNIFORM2IPROC?                                   _glUniform2i;
    private PFNGLUNIFORM2IVPROC?                                  _glUniform2iv;
    private PFNGLUNIFORM2UIPROC?                                  _glUniform2ui;
    private PFNGLUNIFORM2UIVPROC?                                 _glUniform2uiv;
    private PFNGLUNIFORM3DPROC?                                   _glUniform3d;
    private PFNGLUNIFORM3DVPROC?                                  _glUniform3dv;
    private PFNGLUNIFORM3FPROC?                                   _glUniform3f;
    private PFNGLUNIFORM3FVPROC?                                  _glUniform3fv;
    private PFNGLUNIFORM3IPROC?                                   _glUniform3i;
    private PFNGLUNIFORM3IVPROC?                                  _glUniform3iv;
    private PFNGLUNIFORM3UIPROC?                                  _glUniform3ui;
    private PFNGLUNIFORM3UIVPROC?                                 _glUniform3uiv;
    private PFNGLUNIFORM4DPROC?                                   _glUniform4d;
    private PFNGLUNIFORM4DVPROC?                                  _glUniform4dv;
    private PFNGLUNIFORM4FPROC?                                   _glUniform4f;
    private PFNGLUNIFORM4FVPROC?                                  _glUniform4fv;
    private PFNGLUNIFORM4IPROC?                                   _glUniform4i;
    private PFNGLUNIFORM4IVPROC?                                  _glUniform4iv;
    private PFNGLUNIFORM4UIPROC?                                  _glUniform4ui;
    private PFNGLUNIFORM4UIVPROC?                                 _glUniform4uiv;
    private PFNGLUNIFORMBLOCKBINDINGPROC?                         _glUniformBlockBinding;
    private PFNGLUNIFORMMATRIX2DVPROC?                            _glUniformMatrix2dv;
    private PFNGLUNIFORMMATRIX2FVPROC?                            _glUniformMatrix2fv;
    private PFNGLUNIFORMMATRIX2X3DVPROC?                          _glUniformMatrix2x3dv;
    private PFNGLUNIFORMMATRIX2X3FVPROC?                          _glUniformMatrix2x3fv;
    private PFNGLUNIFORMMATRIX2X4DVPROC?                          _glUniformMatrix2x4dv;
    private PFNGLUNIFORMMATRIX2X4FVPROC?                          _glUniformMatrix2x4fv;
    private PFNGLUNIFORMMATRIX3DVPROC?                            _glUniformMatrix3dv;
    private PFNGLUNIFORMMATRIX3FVPROC?                            _glUniformMatrix3fv;
    private PFNGLUNIFORMMATRIX3X2DVPROC?                          _glUniformMatrix3x2dv;
    private PFNGLUNIFORMMATRIX3X2FVPROC?                          _glUniformMatrix3x2fv;
    private PFNGLUNIFORMMATRIX3X4DVPROC?                          _glUniformMatrix3x4dv;
    private PFNGLUNIFORMMATRIX3X4FVPROC?                          _glUniformMatrix3x4fv;
    private PFNGLUNIFORMMATRIX4DVPROC?                            _glUniformMatrix4dv;
    private PFNGLUNIFORMMATRIX4FVPROC?                            _glUniformMatrix4fv;
    private PFNGLUNIFORMMATRIX4X2DVPROC?                          _glUniformMatrix4x2dv;
    private PFNGLUNIFORMMATRIX4X2FVPROC?                          _glUniformMatrix4x2fv;
    private PFNGLUNIFORMMATRIX4X3DVPROC?                          _glUniformMatrix4x3dv;
    private PFNGLUNIFORMMATRIX4X3FVPROC?                          _glUniformMatrix4x3fv;
    private PFNGLUNIFORMSUBROUTINESUIVPROC?                       _glUniformSubroutinesuiv;
    private PFNGLUNMAPBUFFERPROC?                                 _glUnmapBuffer;
    private PFNGLUNMAPNAMEDBUFFERPROC?                            _glUnmapNamedBuffer;
    private PFNGLUSEPROGRAMPROC?                                  _glUseProgram;
    private PFNGLUSEPROGRAMSTAGESPROC?                            _glUseProgramStages;
    private PFNGLVALIDATEPROGRAMPROC?                             _glValidateProgram;
    private PFNGLVALIDATEPROGRAMPIPELINEPROC?                     _glValidateProgramPipeline;
    private PFNGLVERTEXARRAYATTRIBBINDINGPROC?                    _glVertexArrayAttribBinding;
    private PFNGLVERTEXARRAYATTRIBFORMATPROC?                     _glVertexArrayAttribFormat;
    private PFNGLVERTEXARRAYATTRIBIFORMATPROC?                    _glVertexArrayAttribIFormat;
    private PFNGLVERTEXARRAYATTRIBLFORMATPROC?                    _glVertexArrayAttribLFormat;
    private PFNGLVERTEXARRAYBINDINGDIVISORPROC?                   _glVertexArrayBindingDivisor;
    private PFNGLVERTEXARRAYELEMENTBUFFERPROC?                    _glVertexArrayElementBuffer;
    private PFNGLVERTEXARRAYVERTEXBUFFERPROC?                     _glVertexArrayVertexBuffer;
    private PFNGLVERTEXARRAYVERTEXBUFFERSPROC?                    _glVertexArrayVertexBuffers;
    private PFNGLVERTEXATTRIB1DPROC?                              _glVertexAttrib1d;
    private PFNGLVERTEXATTRIB1DVPROC?                             _glVertexAttrib1dv;
    private PFNGLVERTEXATTRIB1FPROC?                              _glVertexAttrib1f;
    private PFNGLVERTEXATTRIB1FVPROC?                             _glVertexAttrib1fv;
    private PFNGLVERTEXATTRIB1SPROC?                              _glVertexAttrib1s;
    private PFNGLVERTEXATTRIB1SVPROC?                             _glVertexAttrib1sv;
    private PFNGLVERTEXATTRIB2DPROC?                              _glVertexAttrib2d;
    private PFNGLVERTEXATTRIB2DVPROC?                             _glVertexAttrib2dv;
    private PFNGLVERTEXATTRIB2FPROC?                              _glVertexAttrib2f;
    private PFNGLVERTEXATTRIB2FVPROC?                             _glVertexAttrib2fv;
    private PFNGLVERTEXATTRIB2SPROC?                              _glVertexAttrib2s;
    private PFNGLVERTEXATTRIB2SVPROC?                             _glVertexAttrib2sv;
    private PFNGLVERTEXATTRIB3DPROC?                              _glVertexAttrib3d;
    private PFNGLVERTEXATTRIB3DVPROC?                             _glVertexAttrib3dv;
    private PFNGLVERTEXATTRIB3FPROC?                              _glVertexAttrib3f;
    private PFNGLVERTEXATTRIB3FVPROC?                             _glVertexAttrib3fv;
    private PFNGLVERTEXATTRIB3SPROC?                              _glVertexAttrib3s;
    private PFNGLVERTEXATTRIB3SVPROC?                             _glVertexAttrib3sv;
    private PFNGLVERTEXATTRIB4BVPROC?                             _glVertexAttrib4bv;
    private PFNGLVERTEXATTRIB4DPROC?                              _glVertexAttrib4d;
    private PFNGLVERTEXATTRIB4DVPROC?                             _glVertexAttrib4dv;
    private PFNGLVERTEXATTRIB4FPROC?                              _glVertexAttrib4f;
    private PFNGLVERTEXATTRIB4FVPROC?                             _glVertexAttrib4fv;
    private PFNGLVERTEXATTRIB4IVPROC?                             _glVertexAttrib4iv;
    private PFNGLVERTEXATTRIB4NBVPROC?                            _glVertexAttrib4Nbv;
    private PFNGLVERTEXATTRIB4NIVPROC?                            _glVertexAttrib4Niv;
    private PFNGLVERTEXATTRIB4NSVPROC?                            _glVertexAttrib4Nsv;
    private PFNGLVERTEXATTRIB4NUBPROC?                            _glVertexAttrib4Nub;
    private PFNGLVERTEXATTRIB4NUBVPROC?                           _glVertexAttrib4Nubv;
    private PFNGLVERTEXATTRIB4NUIVPROC?                           _glVertexAttrib4Nuiv;
    private PFNGLVERTEXATTRIB4NUSVPROC?                           _glVertexAttrib4Nusv;
    private PFNGLVERTEXATTRIB4SPROC?                              _glVertexAttrib4s;
    private PFNGLVERTEXATTRIB4SVPROC?                             _glVertexAttrib4sv;
    private PFNGLVERTEXATTRIB4UBVPROC?                            _glVertexAttrib4ubv;
    private PFNGLVERTEXATTRIB4UIVPROC?                            _glVertexAttrib4uiv;
    private PFNGLVERTEXATTRIB4USVPROC?                            _glVertexAttrib4usv;
    private PFNGLVERTEXATTRIBBINDINGPROC?                         _glVertexAttribBinding;
    private PFNGLVERTEXATTRIBDIVISORPROC?                         _glVertexAttribDivisor;
    private PFNGLVERTEXATTRIBFORMATPROC?                          _glVertexAttribFormat;
    private PFNGLVERTEXATTRIBI1IPROC?                             _glVertexAttribI1i;
    private PFNGLVERTEXATTRIBI1IVPROC?                            _glVertexAttribI1iv;
    private PFNGLVERTEXATTRIBI1UIPROC?                            _glVertexAttribI1ui;
    private PFNGLVERTEXATTRIBI1UIVPROC?                           _glVertexAttribI1uiv;
    private PFNGLVERTEXATTRIBI2IPROC?                             _glVertexAttribI2i;
    private PFNGLVERTEXATTRIBI2IVPROC?                            _glVertexAttribI2iv;
    private PFNGLVERTEXATTRIBI2UIPROC?                            _glVertexAttribI2ui;
    private PFNGLVERTEXATTRIBI2UIVPROC?                           _glVertexAttribI2uiv;
    private PFNGLVERTEXATTRIBI3IPROC?                             _glVertexAttribI3i;
    private PFNGLVERTEXATTRIBI3IVPROC?                            _glVertexAttribI3iv;
    private PFNGLVERTEXATTRIBI3UIPROC?                            _glVertexAttribI3ui;
    private PFNGLVERTEXATTRIBI3UIVPROC?                           _glVertexAttribI3uiv;
    private PFNGLVERTEXATTRIBI4BVPROC?                            _glVertexAttribI4bv;
    private PFNGLVERTEXATTRIBI4IPROC?                             _glVertexAttribI4i;
    private PFNGLVERTEXATTRIBI4IVPROC?                            _glVertexAttribI4iv;
    private PFNGLVERTEXATTRIBI4SVPROC?                            _glVertexAttribI4sv;
    private PFNGLVERTEXATTRIBI4UBVPROC?                           _glVertexAttribI4ubv;
    private PFNGLVERTEXATTRIBI4UIPROC?                            _glVertexAttribI4ui;
    private PFNGLVERTEXATTRIBI4UIVPROC?                           _glVertexAttribI4uiv;
    private PFNGLVERTEXATTRIBI4USVPROC?                           _glVertexAttribI4usv;
    private PFNGLVERTEXATTRIBIFORMATPROC?                         _glVertexAttribIFormat;
    private PFNGLVERTEXATTRIBIPOINTERPROC?                        _glVertexAttribIPointer;
    private PFNGLVERTEXATTRIBL1DPROC?                             _glVertexAttribL1d;
    private PFNGLVERTEXATTRIBL1DVPROC?                            _glVertexAttribL1dv;
    private PFNGLVERTEXATTRIBL2DPROC?                             _glVertexAttribL2d;
    private PFNGLVERTEXATTRIBL2DVPROC?                            _glVertexAttribL2dv;
    private PFNGLVERTEXATTRIBL3DPROC?                             _glVertexAttribL3d;
    private PFNGLVERTEXATTRIBL3DVPROC?                            _glVertexAttribL3dv;
    private PFNGLVERTEXATTRIBL4DPROC?                             _glVertexAttribL4d;
    private PFNGLVERTEXATTRIBL4DVPROC?                            _glVertexAttribL4dv;
    private PFNGLVERTEXATTRIBLFORMATPROC?                         _glVertexAttribLFormat;
    private PFNGLVERTEXATTRIBLPOINTERPROC?                        _glVertexAttribLPointer;
    private PFNGLVERTEXATTRIBP1UIPROC?                            _glVertexAttribP1ui;
    private PFNGLVERTEXATTRIBP1UIVPROC?                           _glVertexAttribP1uiv;
    private PFNGLVERTEXATTRIBP2UIPROC?                            _glVertexAttribP2ui;
    private PFNGLVERTEXATTRIBP2UIVPROC?                           _glVertexAttribP2uiv;
    private PFNGLVERTEXATTRIBP3UIPROC?                            _glVertexAttribP3ui;
    private PFNGLVERTEXATTRIBP3UIVPROC?                           _glVertexAttribP3uiv;
    private PFNGLVERTEXATTRIBP4UIPROC?                            _glVertexAttribP4ui;
    private PFNGLVERTEXATTRIBP4UIVPROC?                           _glVertexAttribP4uiv;
    private PFNGLVERTEXATTRIBPOINTERPROC?                         _glVertexAttribPointer;
    private PFNGLVERTEXBINDINGDIVISORPROC?                        _glVertexBindingDivisor;
    private PFNGLVIEWPORTPROC?                                    _glViewport;
    private PFNGLVIEWPORTARRAYVPROC?                              _glViewportArrayv;
    private PFNGLVIEWPORTINDEXEDFPROC?                            _glViewportIndexedf;
    private PFNGLVIEWPORTINDEXEDFVPROC?                           _glViewportIndexedfv;
    private PFNGLWAITSYNCPROC?                                    _glWaitSync;

    // ========================================================================
    // ========================================================================

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    public unsafe delegate void GLDEBUGPROC( GLenum source,
                                             GLenum type,
                                             GLuint id,
                                             GLenum severity,
                                             GLsizei length,
                                             GLchar* message,
                                             IntPtr userParam );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    public delegate void GLDEBUGPROCSAFE( GLenum source,
                                          GLenum type,
                                          GLuint id,
                                          GLenum severity,
                                          string message,
                                          IntPtr userParam );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCULLFACEPROC( GLenum mode );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLFRONTFACEPROC( GLenum mode );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLHINTPROC( GLenum target, GLenum mode );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLLINEWIDTHPROC( GLfloat width );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPOINTSIZEPROC( GLfloat size );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPOLYGONMODEPROC( GLenum face, GLenum mode );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSCISSORPROC( GLint x, GLint y, GLsizei width, GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXPARAMETERFPROC( GLenum target, GLenum pname, GLfloat param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLTEXPARAMETERFVPROC( GLenum target, GLenum pname, GLfloat* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXPARAMETERIPROC( GLenum target, GLenum pname, GLint param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLTEXPARAMETERIVPROC( GLenum target, GLenum pname, GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXIMAGE1DPROC( GLenum target,
                                               GLint level,
                                               GLenum internalFormat,
                                               GLsizei width,
                                               GLint border,
                                               GLenum format,
                                               GLenum type,
                                               IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXIMAGE2DPROC( GLenum target,
                                               GLint level,
                                               GLenum internalFormat,
                                               GLsizei width,
                                               GLsizei height,
                                               GLint border,
                                               GLenum format,
                                               GLenum type,
                                               IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWBUFFERPROC( GLenum buf );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLEARPROC( GLbitfield mask );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLEARCOLORPROC( GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLEARSTENCILPROC( GLint s );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLEARDEPTHPROC( GLdouble depth );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSTENCILMASKPROC( GLuint mask );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOLORMASKPROC( GLboolean red, GLboolean green, GLboolean blue, GLboolean alpha );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDEPTHMASKPROC( GLboolean flag );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDISABLEPROC( GLenum cap );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLENABLEPROC( GLenum cap );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLFINISHPROC();

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLFLUSHPROC();

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBLENDFUNCPROC( GLenum sfactor, GLenum dfactor );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLLOGICOPPROC( GLenum opcode );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSTENCILFUNCPROC( GLenum func, GLint @ref, GLuint mask );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSTENCILOPPROC( GLenum fail, GLenum zfail, GLenum zpass );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDEPTHFUNCPROC( GLenum func );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPIXELSTOREFPROC( GLenum pname, GLfloat param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPIXELSTOREIPROC( GLenum pname, GLint param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLREADBUFFERPROC( GLenum src );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLREADPIXELSPROC( GLint x, GLint y, GLsizei width, GLsizei height, GLenum format,
                                               GLenum type, IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETBOOLEANVPROC( GLenum pname, GLboolean* data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETDOUBLEVPROC( GLenum pname, GLdouble* data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLenum PFNGLGETERRORPROC();

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETFLOATVPROC( GLenum pname, GLfloat* data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETINTEGERVPROC( GLenum pname, GLint* data );

    [UnmanagedFunctionPointer( CallingConvention.StdCall )]
    private unsafe delegate GLubyte* PFNGLGETSTRINGPROC( GLenum name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETTEXIMAGEPROC( GLenum target, GLint level, GLenum format, GLenum type, IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTEXPARAMETERFVPROC( GLenum target, GLenum pname, GLfloat* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTEXPARAMETERIVPROC( GLenum target, GLenum pname, GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTEXLEVELPARAMETERFVPROC( GLenum target, GLint level, GLenum pname,
                                                                  GLfloat* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTEXLEVELPARAMETERIVPROC( GLenum target, GLint level, GLenum pname,
                                                                  GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISENABLEDPROC( GLenum cap );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDEPTHRANGEPROC( GLdouble near, GLdouble far );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVIEWPORTPROC( GLint x, GLint y, GLsizei width, GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWARRAYSPROC( GLenum mode, GLint first, GLsizei count );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWELEMENTSPROC( GLenum mode, GLsizei count, GLenum type, IntPtr indices );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPOLYGONOFFSETPROC( GLfloat factor, GLfloat units );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOPYTEXIMAGE1DPROC( GLenum target, GLint level, GLenum internalFormat, GLint x, GLint y,
                                                   GLsizei width,
                                                   GLint border );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOPYTEXIMAGE2DPROC( GLenum target,
                                                   GLint level,
                                                   GLenum internalFormat,
                                                   GLint x,
                                                   GLint y,
                                                   GLsizei width,
                                                   GLsizei height,
                                                   GLint border );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOPYTEXSUBIMAGE1DPROC( GLenum target, GLint level, GLint xoffset, GLint x, GLint y,
                                                      GLsizei width );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOPYTEXSUBIMAGE2DPROC( GLenum target,
                                                      GLint level,
                                                      GLint xoffset,
                                                      GLint yoffset,
                                                      GLint x,
                                                      GLint y,
                                                      GLsizei width,
                                                      GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXSUBIMAGE1DPROC( GLenum target, GLint level, GLint xoffset, GLsizei width,
                                                  GLenum format, GLenum type,
                                                  IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXSUBIMAGE2DPROC( GLenum target,
                                                  GLint level,
                                                  GLint xoffset,
                                                  GLint yoffset,
                                                  GLsizei width,
                                                  GLsizei height,
                                                  GLenum format,
                                                  GLenum type,
                                                  IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDTEXTUREPROC( GLenum target, GLuint texture );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDELETETEXTURESPROC( GLsizei n, GLuint* textures );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGENTEXTURESPROC( GLsizei n, GLuint* textures );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISTEXTUREPROC( GLuint texture );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWRANGEELEMENTSPROC( GLenum mode, GLuint start, GLuint end, GLsizei count, GLenum type,
                                                      IntPtr indices );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXIMAGE3DPROC( GLenum target,
                                               GLint level,
                                               GLint internalFormat,
                                               GLsizei width,
                                               GLsizei height,
                                               GLsizei depth,
                                               GLint border,
                                               GLenum format,
                                               GLenum type,
                                               IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXSUBIMAGE3DPROC( GLenum target,
                                                  GLint level,
                                                  GLint xoffset,
                                                  GLint yoffset,
                                                  GLint zoffset,
                                                  GLsizei width,
                                                  GLsizei height,
                                                  GLsizei depth,
                                                  GLenum format,
                                                  GLenum type,
                                                  IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOPYTEXSUBIMAGE3DPROC( GLenum target,
                                                      GLint level,
                                                      GLint xoffset,
                                                      GLint yoffset,
                                                      GLint zoffset,
                                                      GLint x,
                                                      GLint y,
                                                      GLsizei width,
                                                      GLsizei height );

    // ========================================================================

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLACTIVETEXTUREPROC( GLenum texture );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSAMPLECOVERAGEPROC( GLfloat value, GLboolean invert );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOMPRESSEDTEXIMAGE3DPROC( GLenum target,
                                                         GLint level,
                                                         GLenum internalFormat,
                                                         GLsizei width,
                                                         GLsizei height,
                                                         GLsizei depth,
                                                         GLint border,
                                                         GLsizei imageSize,
                                                         IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOMPRESSEDTEXIMAGE2DPROC( GLenum target,
                                                         GLint level,
                                                         GLenum internalFormat,
                                                         GLsizei width,
                                                         GLsizei height,
                                                         GLint border,
                                                         GLsizei imageSize,
                                                         IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOMPRESSEDTEXIMAGE1DPROC( GLenum target,
                                                         GLint level,
                                                         GLenum internalFormat,
                                                         GLsizei width,
                                                         GLint border,
                                                         GLsizei imageSize,
                                                         IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOMPRESSEDTEXSUBIMAGE3DPROC( GLenum target,
                                                            GLint level,
                                                            GLint xoffset,
                                                            GLint yoffset,
                                                            GLint zoffset,
                                                            GLsizei width,
                                                            GLsizei height,
                                                            GLsizei depth,
                                                            GLenum format,
                                                            GLsizei imageSize,
                                                            IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOMPRESSEDTEXSUBIMAGE2DPROC( GLenum target,
                                                            GLint level,
                                                            GLint xoffset,
                                                            GLint yoffset,
                                                            GLsizei width,
                                                            GLsizei height,
                                                            GLenum format,
                                                            GLsizei imageSize,
                                                            IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOMPRESSEDTEXSUBIMAGE1DPROC( GLenum target,
                                                            GLint level,
                                                            GLint xoffset,
                                                            GLsizei width,
                                                            GLenum format,
                                                            GLsizei imageSize,
                                                            IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETCOMPRESSEDTEXIMAGEPROC( GLenum target, GLint level, IntPtr img );

    // ========================================================================

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBLENDFUNCSEPARATEPROC( GLenum sfactorRGB, GLenum dfactorRGB, GLenum sfactorAlpha,
                                                      GLenum dfactorAlpha );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLMULTIDRAWARRAYSPROC( GLenum mode, GLint* first, GLsizei* count,
                                                           GLsizei drawcount );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLMULTIDRAWELEMENTSPROC( GLenum mode, GLsizei* count, GLenum type, IntPtr* indices,
                                                             GLsizei drawcount );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPOINTPARAMETERFPROC( GLenum pname, GLfloat param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPOINTPARAMETERFVPROC( GLenum pname, GLfloat* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPOINTPARAMETERIPROC( GLenum pname, GLint param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPOINTPARAMETERIVPROC( GLenum pname, GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBLENDCOLORPROC( GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBLENDEQUATIONPROC( GLenum mode );

    // ========================================================================

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGENQUERIESPROC( GLsizei n, GLuint* ids );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDELETEQUERIESPROC( GLsizei n, GLuint* ids );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISQUERYPROC( GLuint id );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBEGINQUERYPROC( GLenum target, GLuint id );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLENDQUERYPROC( GLenum target );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETQUERYIVPROC( GLenum target, GLenum pname, GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETQUERYOBJECTIVPROC( GLuint id, GLenum pname, GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETQUERYOBJECTUIVPROC( GLuint id, GLenum pname, GLuint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDBUFFERPROC( GLenum target, GLuint buffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDELETEBUFFERSPROC( GLsizei n, GLuint* buffers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGENBUFFERSPROC( GLsizei n, GLuint* buffers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISBUFFERPROC( GLuint buffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBUFFERDATAPROC( GLenum target, GLsizeiptr size, IntPtr data, GLenum usage );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBUFFERSUBDATAPROC( GLenum target, GLintptr offset, GLsizeiptr size, IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETBUFFERSUBDATAPROC( GLenum target, GLintptr offset, GLsizeiptr size, IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate IntPtr PFNGLMAPBUFFERPROC( GLenum target, GLenum access );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLUNMAPBUFFERPROC( GLenum target );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETBUFFERPARAMETERIVPROC( GLenum target, GLenum pname, GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETBUFFERPOINTERVPROC( GLenum target, GLenum pname, IntPtr* @params );

    // ========================================================================

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBLENDEQUATIONSEPARATEPROC( GLenum modeRGB, GLenum modeAlpha );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDRAWBUFFERSPROC( GLsizei n, GLenum* bufs );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSTENCILOPSEPARATEPROC( GLenum face, GLenum sfail, GLenum dpfail, GLenum dppass );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSTENCILFUNCSEPARATEPROC( GLenum face, GLenum func, GLint @ref, GLuint mask );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSTENCILMASKSEPARATEPROC( GLenum face, GLuint mask );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLATTACHSHADERPROC( GLuint program, GLuint shader );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLBINDATTRIBLOCATIONPROC( GLuint program, GLuint index, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOMPILESHADERPROC( GLuint shader );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLuint PFNGLCREATEPROGRAMPROC();

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLuint PFNGLCREATESHADERPROC( GLenum type );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDELETEPROGRAMPROC( GLuint program );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDELETESHADERPROC( GLuint shader );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDETACHSHADERPROC( GLuint program, GLuint shader );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDISABLEVERTEXATTRIBARRAYPROC( GLuint index );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLENABLEVERTEXATTRIBARRAYPROC( GLuint index );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETACTIVEATTRIBPROC( GLuint program, GLuint index, GLsizei bufSize,
                                                           GLsizei* length, GLint* size,
                                                           GLenum* type, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETACTIVEUNIFORMPROC( GLuint program, GLuint index, GLsizei bufSize,
                                                            GLsizei* length, GLint* size,
                                                            GLenum* type, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETATTACHEDSHADERSPROC( GLuint program, GLsizei maxCount, GLsizei* count,
                                                              GLuint* shaders );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLint PFNGLGETATTRIBLOCATIONPROC( GLuint program, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETPROGRAMIVPROC( GLuint program, GLenum pname, GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETPROGRAMINFOLOGPROC( GLuint program, GLsizei bufSize, GLsizei* length,
                                                             GLchar* infoLog );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETSHADERIVPROC( GLuint shader, GLenum pname, GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETSHADERINFOLOGPROC( GLuint shader, GLsizei bufSize, GLsizei* length,
                                                            GLchar* infoLog );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETSHADERSOURCEPROC( GLuint shader, GLsizei bufSize, GLsizei* length,
                                                           GLchar* source );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLint PFNGLGETUNIFORMLOCATIONPROC( GLuint program, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETUNIFORMFVPROC( GLuint program, GLint location, GLfloat* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETUNIFORMIVPROC( GLuint program, GLint location, GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETVERTEXATTRIBDVPROC( GLuint index, GLenum pname, GLdouble* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETVERTEXATTRIBFVPROC( GLuint index, GLenum pname, GLfloat* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETVERTEXATTRIBIVPROC( GLuint index, GLenum pname, GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETVERTEXATTRIBPOINTERVPROC( GLuint index, GLenum pname, IntPtr* pointer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISPROGRAMPROC( GLuint program );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISSHADERPROC( GLuint shader );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLLINKPROGRAMPROC( GLuint program );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLSHADERSOURCEPROC( GLuint shader, GLsizei count, GLchar** @string, GLint* length );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUSEPROGRAMPROC( GLuint program );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM1FPROC( GLint location, GLfloat v0 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM2FPROC( GLint location, GLfloat v0, GLfloat v1 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM3FPROC( GLint location, GLfloat v0, GLfloat v1, GLfloat v2 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM4FPROC( GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM1IPROC( GLint location, GLint v0 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM2IPROC( GLint location, GLint v0, GLint v1 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM3IPROC( GLint location, GLint v0, GLint v1, GLint v2 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM4IPROC( GLint location, GLint v0, GLint v1, GLint v2, GLint v3 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM1FVPROC( GLint location, GLsizei count, GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM2FVPROC( GLint location, GLsizei count, GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM3FVPROC( GLint location, GLsizei count, GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM4FVPROC( GLint location, GLsizei count, GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM1IVPROC( GLint location, GLsizei count, GLint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM2IVPROC( GLint location, GLsizei count, GLint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM3IVPROC( GLint location, GLsizei count, GLint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM4IVPROC( GLint location, GLsizei count, GLint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX2FVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                            GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX3FVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                            GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX4FVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                            GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLVALIDATEPROGRAMPROC( GLuint program );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB1DPROC( GLuint index, GLdouble x );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB1DVPROC( GLuint index, GLdouble* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB1FPROC( GLuint index, GLfloat x );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB1FVPROC( GLuint index, GLfloat* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB1SPROC( GLuint index, GLshort x );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB1SVPROC( GLuint index, GLshort* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB2DPROC( GLuint index, GLdouble x, GLdouble y );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB2DVPROC( GLuint index, GLdouble* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB2FPROC( GLuint index, GLfloat x, GLfloat y );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB2FVPROC( GLuint index, GLfloat* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB2SPROC( GLuint index, GLshort x, GLshort y );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB2SVPROC( GLuint index, GLshort* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB3DPROC( GLuint index, GLdouble x, GLdouble y, GLdouble z );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB3DVPROC( GLuint index, GLdouble* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB3FPROC( GLuint index, GLfloat x, GLfloat y, GLfloat z );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB3FVPROC( GLuint index, GLfloat* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB3SPROC( GLuint index, GLshort x, GLshort y, GLshort z );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB3SVPROC( GLuint index, GLshort* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4NBVPROC( GLuint index, GLbyte* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4NIVPROC( GLuint index, GLint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4NSVPROC( GLuint index, GLshort* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB4NUBPROC( GLuint index, GLubyte x, GLubyte y, GLubyte z, GLubyte w );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4NUBVPROC( GLuint index, GLubyte* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4NUIVPROC( GLuint index, GLuint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4NUSVPROC( GLuint index, GLushort* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4BVPROC( GLuint index, GLbyte* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB4DPROC( GLuint index, GLdouble x, GLdouble y, GLdouble z, GLdouble w );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4DVPROC( GLuint index, GLdouble* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB4FPROC( GLuint index, GLfloat x, GLfloat y, GLfloat z, GLfloat w );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4FVPROC( GLuint index, GLfloat* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4IVPROC( GLuint index, GLint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIB4SPROC( GLuint index, GLshort x, GLshort y, GLshort z, GLshort w );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4SVPROC( GLuint index, GLshort* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4UBVPROC( GLuint index, GLubyte* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4UIVPROC( GLuint index, GLuint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIB4USVPROC( GLuint index, GLushort* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBPOINTERPROC( GLuint index, GLint size, GLenum type, GLboolean normalized,
                                                        GLsizei stride,
                                                        IntPtr pointer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX2X3FVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                              GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX3X2FVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                              GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX2X4FVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                              GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX4X2FVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                              GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX3X4FVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                              GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX4X3FVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                              GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOLORMASKIPROC( GLuint index, GLboolean r, GLboolean g, GLboolean b, GLboolean a );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETBOOLEANI_VPROC( GLenum target, GLuint index, GLboolean* data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETINTEGERI_VPROC( GLenum target, GLuint index, GLint* data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLENABLEIPROC( GLenum target, GLuint index );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDISABLEIPROC( GLenum target, GLuint index );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISENABLEDIPROC( GLenum target, GLuint index );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBEGINTRANSFORMFEEDBACKPROC( GLenum primitiveMode );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLENDTRANSFORMFEEDBACKPROC();

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDBUFFERRANGEPROC( GLenum target, GLuint index, GLuint buffer, GLintptr offset,
                                                    GLsizeiptr size );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDBUFFERBASEPROC( GLenum target, GLuint index, GLuint buffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLTRANSFORMFEEDBACKVARYINGSPROC( GLuint program, GLsizei count, GLchar** varyings,
                                                                     GLenum bufferMode );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTRANSFORMFEEDBACKVARYINGPROC( GLuint program,
                                                                       GLuint index,
                                                                       GLsizei bufSize,
                                                                       GLsizei* length,
                                                                       GLsizei* size,
                                                                       GLenum* type,
                                                                       GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLAMPCOLORPROC( GLenum target, GLenum clamp );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBEGINCONDITIONALRENDERPROC( GLuint id, GLenum mode );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLENDCONDITIONALRENDERPROC();

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBIPOINTERPROC( GLuint index, GLint size, GLenum type, GLsizei stride,
                                                         IntPtr pointer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETVERTEXATTRIBIIVPROC( GLuint index, GLenum pname, GLint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETVERTEXATTRIBIUIVPROC( GLuint index, GLenum pname, GLuint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBI1IPROC( GLuint index, GLint x );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBI2IPROC( GLuint index, GLint x, GLint y );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBI3IPROC( GLuint index, GLint x, GLint y, GLint z );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBI4IPROC( GLuint index, GLint x, GLint y, GLint z, GLint w );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBI1UIPROC( GLuint index, GLuint x );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBI2UIPROC( GLuint index, GLuint x, GLuint y );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBI3UIPROC( GLuint index, GLuint x, GLuint y, GLuint z );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBI4UIPROC( GLuint index, GLuint x, GLuint y, GLuint z, GLuint w );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBI1IVPROC( GLuint index, GLint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBI2IVPROC( GLuint index, GLint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBI3IVPROC( GLuint index, GLint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBI4IVPROC( GLuint index, GLint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBI1UIVPROC( GLuint index, GLuint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBI2UIVPROC( GLuint index, GLuint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBI3UIVPROC( GLuint index, GLuint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBI4UIVPROC( GLuint index, GLuint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBI4BVPROC( GLuint index, GLbyte* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBI4SVPROC( GLuint index, GLshort* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBI4UBVPROC( GLuint index, GLubyte* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBI4USVPROC( GLuint index, GLushort* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETUNIFORMUIVPROC( GLuint program, GLint location, GLuint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLBINDFRAGDATALOCATIONPROC( GLuint program, GLuint color, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLint PFNGLGETFRAGDATALOCATIONPROC( GLuint program, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM1UIPROC( GLint location, GLuint v0 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM2UIPROC( GLint location, GLuint v0, GLuint v1 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM3UIPROC( GLint location, GLuint v0, GLuint v1, GLuint v2 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM4UIPROC( GLint location, GLuint v0, GLuint v1, GLuint v2, GLuint v3 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM1UIVPROC( GLint location, GLsizei count, GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM2UIVPROC( GLint location, GLsizei count, GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM3UIVPROC( GLint location, GLsizei count, GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM4UIVPROC( GLint location, GLsizei count, GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLTEXPARAMETERIIVPROC( GLenum target, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLTEXPARAMETERIUIVPROC( GLenum target, GLenum pname, GLuint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTEXPARAMETERIIVPROC( GLenum target, GLenum pname, GLint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTEXPARAMETERIUIVPROC( GLenum target, GLenum pname, GLuint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCLEARBUFFERIVPROC( GLenum buffer, GLint drawbuffer, GLint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCLEARBUFFERUIVPROC( GLenum buffer, GLint drawbuffer, GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCLEARBUFFERFVPROC( GLenum buffer, GLint drawbuffer, GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLEARBUFFERFIPROC( GLenum buffer, GLint drawbuffer, GLfloat depth, GLint stencil );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLubyte* PFNGLGETSTRINGIPROC( GLenum name, GLuint index );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISRENDERBUFFERPROC( GLuint renderbuffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDRENDERBUFFERPROC( GLenum target, GLuint renderbuffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDELETERENDERBUFFERSPROC( GLsizei n, GLuint* renderbuffers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGENRENDERBUFFERSPROC( GLsizei n, GLuint* renderbuffers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLRENDERBUFFERSTORAGEPROC( GLenum target, GLenum internalFormat, GLsizei width,
                                                        GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETRENDERBUFFERPARAMETERIVPROC( GLenum target, GLenum pname, GLint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISFRAMEBUFFERPROC( GLuint framebuffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDFRAMEBUFFERPROC( GLenum target, GLuint framebuffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDELETEFRAMEBUFFERSPROC( GLsizei n, GLuint* framebuffers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGENFRAMEBUFFERSPROC( GLsizei n, GLuint* framebuffers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLenum PFNGLCHECKFRAMEBUFFERSTATUSPROC( GLenum target );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLFRAMEBUFFERTEXTURE1DPROC( GLenum target, GLenum attachment, GLenum textarget,
                                                         GLuint texture, GLint level );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLFRAMEBUFFERTEXTURE2DPROC( GLenum target, GLenum attachment, GLenum textarget,
                                                         GLuint texture, GLint level );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLFRAMEBUFFERTEXTURE3DPROC( GLenum target, GLenum attachment, GLenum textarget,
                                                         GLuint texture, GLint level,
                                                         GLint zoffset );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLFRAMEBUFFERRENDERBUFFERPROC( GLenum target, GLenum attachment, GLenum renderbuffertarget,
                                                            GLuint renderbuffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETFRAMEBUFFERATTACHMENTPARAMETERIVPROC(
        GLenum target, GLenum attachment, GLenum pname,
        GLint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGENERATEMIPMAPPROC( GLenum target );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBLITFRAMEBUFFERPROC( GLint srcX0,
                                                    GLint srcY0,
                                                    GLint srcX1,
                                                    GLint srcY1,
                                                    GLint dstX0,
                                                    GLint dstY0,
                                                    GLint dstX1,
                                                    GLint dstY1,
                                                    GLbitfield mask,
                                                    GLenum filter );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLRENDERBUFFERSTORAGEMULTISAMPLEPROC( GLenum target, GLsizei samples,
                                                                   GLenum internalFormat, GLsizei width,
                                                                   GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLFRAMEBUFFERTEXTURELAYERPROC( GLenum target, GLenum attachment, GLuint texture,
                                                            GLint level, GLint layer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate IntPtr PFNGLMAPBUFFERRANGEPROC( GLenum target, GLintptr offset, GLsizeiptr length,
                                                     GLbitfield access );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLFLUSHMAPPEDBUFFERRANGEPROC( GLenum target, GLintptr offset, GLsizeiptr length );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDVERTEXARRAYPROC( GLuint array );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDELETEVERTEXARRAYSPROC( GLsizei n, GLuint* arrays );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGENVERTEXARRAYSPROC( GLsizei n, GLuint* arrays );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISVERTEXARRAYPROC( GLuint array );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void
        PFNGLDRAWARRAYSINSTANCEDPROC( GLenum mode, GLint first, GLsizei count, GLsizei instancecount );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWELEMENTSINSTANCEDPROC( GLenum mode, GLsizei count, GLenum type, IntPtr indices,
                                                          GLsizei instancecount );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXBUFFERPROC( GLenum target, GLenum internalFormat, GLuint buffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPRIMITIVERESTARTINDEXPROC( GLuint index );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOPYBUFFERSUBDATAPROC( GLenum readTarget, GLenum writeTarget, GLintptr readOffset,
                                                      GLintptr writeOffset,
                                                      GLsizeiptr size );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETUNIFORMINDICESPROC( GLuint program, GLsizei uniformCount,
                                                             GLchar** uniformNames,
                                                             GLuint* uniformIndices );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETACTIVEUNIFORMSIVPROC( GLuint program, GLsizei uniformCount,
                                                               GLuint* uniformIndices, GLenum pname,
                                                               GLint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETACTIVEUNIFORMNAMEPROC( GLuint program, GLuint uniformIndex, GLsizei bufSize,
                                                                GLsizei* length,
                                                                GLchar* uniformName );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLuint PFNGLGETUNIFORMBLOCKINDEXPROC( GLuint program, GLchar* uniformBlockName );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETACTIVEUNIFORMBLOCKIVPROC( GLuint program, GLuint uniformBlockIndex,
                                                                   GLenum pname,
                                                                   GLint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETACTIVEUNIFORMBLOCKNAMEPROC( GLuint program,
                                                                     GLuint uniformBlockIndex,
                                                                     GLsizei bufSize,
                                                                     GLsizei* length,
                                                                     GLchar* uniformBlockName );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORMBLOCKBINDINGPROC( GLuint program, GLuint uniformBlockIndex,
                                                        GLuint uniformBlockBinding );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWELEMENTSBASEVERTEXPROC( GLenum mode, GLsizei count, GLenum type, IntPtr indices,
                                                           GLint basevertex );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWRANGEELEMENTSBASEVERTEXPROC( GLenum mode,
                                                                GLuint start,
                                                                GLuint end,
                                                                GLsizei count,
                                                                GLenum type,
                                                                IntPtr indices,
                                                                GLint basevertex );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWELEMENTSINSTANCEDBASEVERTEXPROC( GLenum mode,
                                                                    GLsizei count,
                                                                    GLenum type,
                                                                    IntPtr indices,
                                                                    GLsizei instancecount,
                                                                    GLint basevertex );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLMULTIDRAWELEMENTSBASEVERTEXPROC( GLenum mode,
                                                                       GLsizei* count,
                                                                       GLenum type,
                                                                       IntPtr* indices,
                                                                       GLsizei drawcount,
                                                                       GLint* basevertex );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROVOKINGVERTEXPROC( GLenum mode );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate IntPtr PFNGLFENCESYNCPROC( GLenum condition, GLbitfield flags );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISSYNCPROC( IntPtr sync );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDELETESYNCPROC( IntPtr sync );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLenum PFNGLCLIENTWAITSYNCPROC( IntPtr sync, GLbitfield flags, GLuint64 timeout );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLWAITSYNCPROC( IntPtr sync, GLbitfield flags, GLuint64 timeout );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETINTEGER64VPROC( GLenum pname, GLint64* data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETSYNCIVPROC( IntPtr sync, GLenum pname, GLsizei bufSize, GLsizei* length,
                                                     GLint* values );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETINTEGER64I_VPROC( GLenum target, GLuint index, GLint64* data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETBUFFERPARAMETERI64VPROC( GLenum target, GLenum pname, GLint64* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLFRAMEBUFFERTEXTUREPROC( GLenum target, GLenum attachment, GLuint texture, GLint level );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXIMAGE2DMULTISAMPLEPROC( GLenum target,
                                                          GLsizei samples,
                                                          GLenum internalFormat,
                                                          GLsizei width,
                                                          GLsizei height,
                                                          GLboolean fixedsamplelocations );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXIMAGE3DMULTISAMPLEPROC( GLenum target,
                                                          GLsizei samples,
                                                          GLenum internalFormat,
                                                          GLsizei width,
                                                          GLsizei height,
                                                          GLsizei depth,
                                                          GLboolean fixedsamplelocations );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETMULTISAMPLEFVPROC( GLenum pname, GLuint index, GLfloat* val );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSAMPLEMASKIPROC( GLuint maskNumber, GLbitfield mask );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLBINDFRAGDATALOCATIONINDEXEDPROC( GLuint program, GLuint colorNumber, GLuint index,
                                                                       GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLint PFNGLGETFRAGDATAINDEXPROC( GLuint program, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGENSAMPLERSPROC( GLsizei count, GLuint* samplers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDELETESAMPLERSPROC( GLsizei count, GLuint* samplers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISSAMPLERPROC( GLuint sampler );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDSAMPLERPROC( GLuint unit, GLuint sampler );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSAMPLERPARAMETERIPROC( GLuint sampler, GLenum pname, GLint param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLSAMPLERPARAMETERIVPROC( GLuint sampler, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSAMPLERPARAMETERFPROC( GLuint sampler, GLenum pname, GLfloat param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLSAMPLERPARAMETERFVPROC( GLuint sampler, GLenum pname, GLfloat* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLSAMPLERPARAMETERIIVPROC( GLuint sampler, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLSAMPLERPARAMETERIUIVPROC( GLuint sampler, GLenum pname, GLuint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETSAMPLERPARAMETERIVPROC( GLuint sampler, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETSAMPLERPARAMETERIIVPROC( GLuint sampler, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETSAMPLERPARAMETERFVPROC( GLuint sampler, GLenum pname, GLfloat* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETSAMPLERPARAMETERIUIVPROC( GLuint sampler, GLenum pname, GLuint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLQUERYCOUNTERPROC( GLuint id, GLenum target );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETQUERYOBJECTI64VPROC( GLuint id, GLenum pname, GLint64* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETQUERYOBJECTUI64VPROC( GLuint id, GLenum pname, GLuint64* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBDIVISORPROC( GLuint index, GLuint divisor );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBP1UIPROC( GLuint index, GLenum type, GLboolean normalized, GLuint value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBP1UIVPROC( GLuint index, GLenum type, GLboolean normalized,
                                                             GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBP2UIPROC( GLuint index, GLenum type, GLboolean normalized, GLuint value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBP2UIVPROC( GLuint index, GLenum type, GLboolean normalized,
                                                             GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBP3UIPROC( GLuint index, GLenum type, GLboolean normalized, GLuint value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBP3UIVPROC( GLuint index, GLenum type, GLboolean normalized,
                                                             GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBP4UIPROC( GLuint index, GLenum type, GLboolean normalized, GLuint value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBP4UIVPROC( GLuint index, GLenum type, GLboolean normalized,
                                                             GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLMINSAMPLESHADINGPROC( GLfloat value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBLENDEQUATIONIPROC( GLuint buf, GLenum mode );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBLENDEQUATIONSEPARATEIPROC( GLuint buf, GLenum modeRGB, GLenum modeAlpha );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBLENDFUNCIPROC( GLuint buf, GLenum src, GLenum dst );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBLENDFUNCSEPARATEIPROC( GLuint buf, GLenum srcRGB, GLenum dstRGB, GLenum srcAlpha,
                                                       GLenum dstAlpha );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWARRAYSINDIRECTPROC( GLenum mode, IntPtr indirect );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWELEMENTSINDIRECTPROC( GLenum mode, GLenum type, IntPtr indirect );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM1DPROC( GLint location, GLdouble x );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM2DPROC( GLint location, GLdouble x, GLdouble y );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM3DPROC( GLint location, GLdouble x, GLdouble y, GLdouble z );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUNIFORM4DPROC( GLint location, GLdouble x, GLdouble y, GLdouble z, GLdouble w );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM1DVPROC( GLint location, GLsizei count, GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM2DVPROC( GLint location, GLsizei count, GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM3DVPROC( GLint location, GLsizei count, GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORM4DVPROC( GLint location, GLsizei count, GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX2DVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                            GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX3DVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                            GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX4DVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                            GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX2X3DVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                              GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX2X4DVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                              GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX3X2DVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                              GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX3X4DVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                              GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX4X2DVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                              GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMMATRIX4X3DVPROC( GLint location, GLsizei count, GLboolean transpose,
                                                              GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETUNIFORMDVPROC( GLuint program, GLint location, GLdouble* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLint PFNGLGETSUBROUTINEUNIFORMLOCATIONPROC(
        GLuint program, GLenum shadertype, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLuint PFNGLGETSUBROUTINEINDEXPROC( GLuint program, GLenum shadertype, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETACTIVESUBROUTINEUNIFORMIVPROC( GLuint program, GLenum shadertype, GLuint index,
                                                                        GLenum pname,
                                                                        GLint* values );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETACTIVESUBROUTINEUNIFORMNAMEPROC(
        GLuint program, GLenum shadertype, GLuint index, GLsizei bufsize,
        GLsizei* length, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETACTIVESUBROUTINENAMEPROC( GLuint program, GLenum shadertype, GLuint index,
                                                                   GLsizei bufsize,
                                                                   GLsizei* length, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLUNIFORMSUBROUTINESUIVPROC( GLenum shadertype, GLsizei count, GLuint* indices );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETUNIFORMSUBROUTINEUIVPROC( GLenum shadertype, GLint location, GLuint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETPROGRAMSTAGEIVPROC( GLuint program, GLenum shadertype, GLenum pname,
                                                             GLint* values );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPATCHPARAMETERIPROC( GLenum pname, GLint value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPATCHPARAMETERFVPROC( GLenum pname, GLfloat* values );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDTRANSFORMFEEDBACKPROC( GLenum target, GLuint id );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDELETETRANSFORMFEEDBACKSPROC( GLsizei n, GLuint* ids );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGENTRANSFORMFEEDBACKSPROC( GLsizei n, GLuint* ids );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISTRANSFORMFEEDBACKPROC( GLuint id );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPAUSETRANSFORMFEEDBACKPROC();

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLRESUMETRANSFORMFEEDBACKPROC();

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWTRANSFORMFEEDBACKPROC( GLenum mode, GLuint id );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWTRANSFORMFEEDBACKSTREAMPROC( GLenum mode, GLuint id, GLuint stream );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBEGINQUERYINDEXEDPROC( GLenum target, GLuint index, GLuint id );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLENDQUERYINDEXEDPROC( GLenum target, GLuint index );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETQUERYINDEXEDIVPROC( GLenum target, GLuint index, GLenum pname,
                                                             GLint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLRELEASESHADERCOMPILERPROC();

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void
        PFNGLSHADERBINARYPROC( GLsizei count, GLuint* shaders, GLenum binaryformat, IntPtr binary, GLsizei length );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETSHADERPRECISIONFORMATPROC( GLenum shadertype, GLenum precisiontype,
                                                                    GLint* range,
                                                                    GLint* precision );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDEPTHRANGEFPROC( GLfloat n, GLfloat f );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLEARDEPTHFPROC( GLfloat d );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETPROGRAMBINARYPROC( GLuint program, GLsizei bufSize, GLsizei* length,
                                                            GLenum* binaryFormat,
                                                            IntPtr binary );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMBINARYPROC( GLuint program, GLenum binaryFormat, IntPtr binary, GLsizei length );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMPARAMETERIPROC( GLuint program, GLenum pname, GLint value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLUSEPROGRAMSTAGESPROC( GLuint pipeline, GLbitfield stages, GLuint program );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLACTIVESHADERPROGRAMPROC( GLuint pipeline, GLuint program );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLuint PFNGLCREATESHADERPROGRAMVPROC( GLenum type, GLsizei count, GLchar** strings );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDPROGRAMPIPELINEPROC( GLuint pipeline );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDELETEPROGRAMPIPELINESPROC( GLsizei n, GLuint* pipelines );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGENPROGRAMPIPELINESPROC( GLsizei n, GLuint* pipelines );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLISPROGRAMPIPELINEPROC( GLuint pipeline );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETPROGRAMPIPELINEIVPROC( GLuint pipeline, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM1IPROC( GLuint program, GLint location, GLint v0 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM1IVPROC( GLuint program, GLint location, GLsizei count,
                                                             GLint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM1FPROC( GLuint program, GLint location, GLfloat v0 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM1FVPROC( GLuint program, GLint location, GLsizei count,
                                                             GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM1DPROC( GLuint program, GLint location, GLdouble v0 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM1DVPROC( GLuint program, GLint location, GLsizei count,
                                                             GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM1UIPROC( GLuint program, GLint location, GLuint v0 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM1UIVPROC( GLuint program, GLint location, GLsizei count,
                                                              GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM2IPROC( GLuint program, GLint location, GLint v0, GLint v1 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM2IVPROC( GLuint program, GLint location, GLsizei count,
                                                             GLint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM2FPROC( GLuint program, GLint location, GLfloat v0, GLfloat v1 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM2FVPROC( GLuint program, GLint location, GLsizei count,
                                                             GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM2DPROC( GLuint program, GLint location, GLdouble v0, GLdouble v1 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM2DVPROC( GLuint program, GLint location, GLsizei count,
                                                             GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM2UIPROC( GLuint program, GLint location, GLuint v0, GLuint v1 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM2UIVPROC( GLuint program, GLint location, GLsizei count,
                                                              GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM3IPROC( GLuint program, GLint location, GLint v0, GLint v1, GLint v2 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM3IVPROC( GLuint program, GLint location, GLsizei count,
                                                             GLint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM3FPROC( GLuint program, GLint location, GLfloat v0, GLfloat v1,
                                                     GLfloat v2 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM3FVPROC( GLuint program, GLint location, GLsizei count,
                                                             GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM3DPROC( GLuint program, GLint location, GLdouble v0, GLdouble v1,
                                                     GLdouble v2 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM3DVPROC( GLuint program, GLint location, GLsizei count,
                                                             GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM3UIPROC( GLuint program, GLint location, GLuint v0, GLuint v1, GLuint v2 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM3UIVPROC( GLuint program, GLint location, GLsizei count,
                                                              GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM4IPROC( GLuint program, GLint location, GLint v0, GLint v1, GLint v2,
                                                     GLint v3 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM4IVPROC( GLuint program, GLint location, GLsizei count,
                                                             GLint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM4FPROC( GLuint program, GLint location, GLfloat v0, GLfloat v1, GLfloat v2,
                                                     GLfloat v3 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM4FVPROC( GLuint program, GLint location, GLsizei count,
                                                             GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM4DPROC( GLuint program, GLint location, GLdouble v0, GLdouble v1,
                                                     GLdouble v2, GLdouble v3 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM4DVPROC( GLuint program, GLint location, GLsizei count,
                                                             GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPROGRAMUNIFORM4UIPROC( GLuint program, GLint location, GLuint v0, GLuint v1, GLuint v2,
                                                      GLuint v3 );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORM4UIVPROC( GLuint program, GLint location, GLsizei count,
                                                              GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX2FVPROC( GLuint program, GLint location, GLsizei count,
                                                                   GLboolean transpose,
                                                                   GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX3FVPROC( GLuint program, GLint location, GLsizei count,
                                                                   GLboolean transpose,
                                                                   GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX4FVPROC( GLuint program, GLint location, GLsizei count,
                                                                   GLboolean transpose,
                                                                   GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX2DVPROC( GLuint program, GLint location, GLsizei count,
                                                                   GLboolean transpose,
                                                                   GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX3DVPROC( GLuint program, GLint location, GLsizei count,
                                                                   GLboolean transpose,
                                                                   GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX4DVPROC( GLuint program, GLint location, GLsizei count,
                                                                   GLboolean transpose,
                                                                   GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX2X3FVPROC( GLuint program, GLint location, GLsizei count,
                                                                     GLboolean transpose,
                                                                     GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX3X2FVPROC( GLuint program, GLint location, GLsizei count,
                                                                     GLboolean transpose,
                                                                     GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX2X4FVPROC( GLuint program, GLint location, GLsizei count,
                                                                     GLboolean transpose,
                                                                     GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX4X2FVPROC( GLuint program, GLint location, GLsizei count,
                                                                     GLboolean transpose,
                                                                     GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX3X4FVPROC( GLuint program, GLint location, GLsizei count,
                                                                     GLboolean transpose,
                                                                     GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX4X3FVPROC( GLuint program, GLint location, GLsizei count,
                                                                     GLboolean transpose,
                                                                     GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX2X3DVPROC( GLuint program, GLint location, GLsizei count,
                                                                     GLboolean transpose,
                                                                     GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX3X2DVPROC( GLuint program, GLint location, GLsizei count,
                                                                     GLboolean transpose,
                                                                     GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX2X4DVPROC( GLuint program, GLint location, GLsizei count,
                                                                     GLboolean transpose,
                                                                     GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX4X2DVPROC( GLuint program, GLint location, GLsizei count,
                                                                     GLboolean transpose,
                                                                     GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX3X4DVPROC( GLuint program, GLint location, GLsizei count,
                                                                     GLboolean transpose,
                                                                     GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPROGRAMUNIFORMMATRIX4X3DVPROC( GLuint program, GLint location, GLsizei count,
                                                                     GLboolean transpose,
                                                                     GLdouble* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVALIDATEPROGRAMPIPELINEPROC( GLuint pipeline );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETPROGRAMPIPELINEINFOLOGPROC( GLuint pipeline, GLsizei bufSize, GLsizei* length,
                                                                     GLchar* infoLog );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBL1DPROC( GLuint index, GLdouble x );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBL2DPROC( GLuint index, GLdouble x, GLdouble y );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBL3DPROC( GLuint index, GLdouble x, GLdouble y, GLdouble z );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBL4DPROC( GLuint index, GLdouble x, GLdouble y, GLdouble z, GLdouble w );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBL1DVPROC( GLuint index, GLdouble* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBL2DVPROC( GLuint index, GLdouble* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBL3DVPROC( GLuint index, GLdouble* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXATTRIBL4DVPROC( GLuint index, GLdouble* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBLPOINTERPROC( GLuint index, GLint size, GLenum type, GLsizei stride,
                                                         IntPtr pointer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETVERTEXATTRIBLDVPROC( GLuint index, GLenum pname, GLdouble* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVIEWPORTARRAYVPROC( GLuint first, GLsizei count, GLfloat* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVIEWPORTINDEXEDFPROC( GLuint index, GLfloat x, GLfloat y, GLfloat w, GLfloat h );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVIEWPORTINDEXEDFVPROC( GLuint index, GLfloat* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLSCISSORARRAYVPROC( GLuint first, GLsizei count, GLint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSCISSORINDEXEDPROC( GLuint index, GLint left, GLint bottom, GLsizei width,
                                                   GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLSCISSORINDEXEDVPROC( GLuint index, GLint* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDEPTHRANGEARRAYVPROC( GLuint first, GLsizei count, GLdouble* v );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDEPTHRANGEINDEXEDPROC( GLuint index, GLdouble n, GLdouble f );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETFLOATI_VPROC( GLenum target, GLuint index, GLfloat* data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETDOUBLEI_VPROC( GLenum target, GLuint index, GLdouble* data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWARRAYSINSTANCEDBASEINSTANCEPROC( GLenum mode, GLint first, GLsizei count,
                                                                    GLsizei instancecount,
                                                                    GLuint baseinstance );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWELEMENTSINSTANCEDBASEINSTANCEPROC( GLenum mode, GLsizei count, GLenum type,
                                                                      IntPtr indices,
                                                                      GLsizei instancecount, GLuint baseinstance );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWELEMENTSINSTANCEDBASEVERTEXBASEINSTANCEPROC(
        GLenum mode, GLsizei count, GLenum type, IntPtr indices,
        GLsizei instancecount, GLint basevertex,
        GLuint baseinstance );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETINTERNALFORMATIVPROC( GLenum target, GLenum internalFormat, GLenum pname,
                                                               GLsizei bufSize,
                                                               GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void
        PFNGLGETACTIVEATOMICCOUNTERBUFFERIVPROC( GLuint program, GLuint bufferIndex, GLenum pname, GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDIMAGETEXTUREPROC( GLuint unit, GLuint texture, GLint level, GLboolean layered,
                                                     GLint layer,
                                                     GLenum access, GLenum format );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLMEMORYBARRIERPROC( GLbitfield barriers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXSTORAGE1DPROC( GLenum target, GLsizei levels, GLenum internalFormat, GLsizei width );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXSTORAGE2DPROC( GLenum target, GLsizei levels, GLenum internalFormat, GLsizei width,
                                                 GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXSTORAGE3DPROC( GLenum target, GLsizei levels, GLenum internalFormat, GLsizei width,
                                                 GLsizei height,
                                                 GLsizei depth );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWTRANSFORMFEEDBACKINSTANCEDPROC( GLenum mode, GLuint id, GLsizei instancecount );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDRAWTRANSFORMFEEDBACKSTREAMINSTANCEDPROC(
        GLenum mode, GLuint id, GLuint stream, GLsizei instancecount );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETPOINTERVPROC( GLenum pname, IntPtr* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLEARBUFFERDATAPROC( GLenum target, GLenum internalFormat, GLenum format, GLenum type,
                                                    IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLEARBUFFERSUBDATAPROC( GLenum target, GLenum internalFormat, GLintptr offset,
                                                       GLsizeiptr size,
                                                       GLenum format, GLenum type, IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDISPATCHCOMPUTEPROC( GLuint num_groups_x, GLuint num_groups_y, GLuint num_groups_z );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDISPATCHCOMPUTEINDIRECTPROC( IntPtr indirect );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOPYIMAGESUBDATAPROC( GLuint srcName, GLenum srcTarget, GLint srcLevel, GLint srcX,
                                                     GLint srcY, GLint srcZ,
                                                     GLuint dstName, GLenum dstTarget, GLint dstLevel, GLint dstX,
                                                     GLint dstY, GLint dstZ,
                                                     GLsizei srcWidth, GLsizei srcHeight, GLsizei srcDepth );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLFRAMEBUFFERPARAMETERIPROC( GLenum target, GLenum pname, GLint param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETFRAMEBUFFERPARAMETERIVPROC( GLenum target, GLenum pname, GLint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETINTERNALFORMATI64VPROC( GLenum target, GLenum internalFormat, GLenum pname,
                                                                 GLsizei count,
                                                                 GLint64* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLINVALIDATETEXSUBIMAGEPROC( GLuint texture, GLint level, GLint xoffset, GLint yoffset,
                                                          GLint zoffset,
                                                          GLsizei width, GLsizei height, GLsizei depth );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLINVALIDATETEXIMAGEPROC( GLuint texture, GLint level );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLINVALIDATEBUFFERSUBDATAPROC( GLuint buffer, GLintptr offset, GLsizeiptr length );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLINVALIDATEBUFFERDATAPROC( GLuint buffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLINVALIDATEFRAMEBUFFERPROC( GLenum target, GLsizei numAttachments,
                                                                 GLenum* attachments );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLINVALIDATESUBFRAMEBUFFERPROC( GLenum target, GLsizei numAttachments,
                                                                    GLenum* attachments, GLint x,
                                                                    GLint y, GLsizei width, GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLMULTIDRAWARRAYSINDIRECTPROC( GLenum mode, IntPtr indirect, GLsizei drawcount,
                                                            GLsizei stride );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void
        PFNGLMULTIDRAWELEMENTSINDIRECTPROC( GLenum mode, GLenum type, IntPtr indirect, GLsizei drawcount,
                                            GLsizei stride );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETPROGRAMINTERFACEIVPROC( GLuint program, GLenum programInterface, GLenum pname,
                                                                 GLint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLuint PFNGLGETPROGRAMRESOURCEINDEXPROC( GLuint program, GLenum programInterface,
                                                                     GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETPROGRAMRESOURCENAMEPROC( GLuint program, GLenum programInterface, GLuint index,
                                                                  GLsizei bufSize,
                                                                  GLsizei* length, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETPROGRAMRESOURCEIVPROC( GLuint program, GLenum programInterface, GLuint index,
                                                                GLsizei propCount,
                                                                GLenum* props, GLsizei bufSize, GLsizei* length,
                                                                GLint* @params );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLint PFNGLGETPROGRAMRESOURCELOCATIONPROC( GLuint program, GLenum programInterface,
                                                                       GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLint PFNGLGETPROGRAMRESOURCELOCATIONINDEXPROC(
        GLuint program, GLenum programInterface, GLchar* name );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLSHADERSTORAGEBLOCKBINDINGPROC( GLuint program, GLuint storageBlockIndex,
                                                              GLuint storageBlockBinding );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXBUFFERRANGEPROC( GLenum target, GLenum internalFormat, GLuint buffer, GLintptr offset,
                                                   GLsizeiptr size );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXSTORAGE2DMULTISAMPLEPROC( GLenum target, GLsizei samples, GLenum internalFormat,
                                                            GLsizei width,
                                                            GLsizei height, GLboolean fixedsamplelocations );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXSTORAGE3DMULTISAMPLEPROC( GLenum target, GLsizei samples, GLenum internalFormat,
                                                            GLsizei width,
                                                            GLsizei height, GLsizei depth,
                                                            GLboolean fixedsamplelocations );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTUREVIEWPROC( GLuint texture, GLenum target, GLuint origtexture,
                                                GLenum internalFormat, GLuint minlevel,
                                                GLuint numlevels, GLuint minlayer, GLuint numlayers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDVERTEXBUFFERPROC( GLuint bindingindex, GLuint buffer, GLintptr offset,
                                                     GLsizei stride );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBFORMATPROC( GLuint attribindex, GLint size, GLenum type,
                                                       GLboolean normalized,
                                                       GLuint relativeoffset );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBIFORMATPROC( GLuint attribindex, GLint size, GLenum type,
                                                        GLuint relativeoffset );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBLFORMATPROC( GLuint attribindex, GLint size, GLenum type,
                                                        GLuint relativeoffset );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXATTRIBBINDINGPROC( GLuint attribindex, GLuint bindingindex );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXBINDINGDIVISORPROC( GLuint bindingindex, GLuint divisor );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDEBUGMESSAGECONTROLPROC( GLenum source, GLenum type, GLenum severity,
                                                               GLsizei count, GLuint* ids,
                                                               GLboolean enabled );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLDEBUGMESSAGEINSERTPROC( GLenum source, GLenum type, GLuint id, GLenum severity,
                                                              GLsizei length,
                                                              GLchar* buf );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDEBUGMESSAGECALLBACKPROC( GLDEBUGPROC callback, IntPtr userParam );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate GLuint PFNGLGETDEBUGMESSAGELOGPROC( GLuint count, GLsizei bufsize, GLenum* sources,
                                                                GLenum* types, GLuint* ids,
                                                                GLenum* severities, GLsizei* lengths,
                                                                GLchar* messageLog );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLPUSHDEBUGGROUPPROC( GLenum source, GLuint id, GLsizei length, GLchar* message );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPOPDEBUGGROUPPROC();

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLOBJECTLABELPROC( GLenum identifier, GLuint name, GLsizei length, GLchar* label );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETOBJECTLABELPROC( GLenum identifier, GLuint name, GLsizei bufSize,
                                                          GLsizei* length, GLchar* label );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLOBJECTPTRLABELPROC( IntPtr ptr, GLsizei length, GLchar* label );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETOBJECTPTRLABELPROC( IntPtr ptr, GLsizei bufSize, GLsizei* length,
                                                             GLchar* label );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBUFFERSTORAGEPROC( GLenum target, GLsizeiptr size, IntPtr data, GLbitfield flags );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void
        PFNGLCLEARTEXIMAGEPROC( GLuint texture, GLint level, GLenum format, GLenum type, IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLEARTEXSUBIMAGEPROC( GLuint texture, GLint level, GLint xOffset, GLint yOffset,
                                                     GLint zOffset,
                                                     GLsizei width, GLsizei height, GLsizei depth, GLenum format,
                                                     GLenum type,
                                                     IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void
        PFNGLBINDBUFFERSBASEPROC( GLenum target, GLuint first, GLsizei count, GLuint* buffers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLBINDBUFFERSRANGEPROC( GLenum target, GLuint first, GLsizei count, GLuint* buffers,
                                                            GLintptr* offsets,
                                                            GLsizeiptr* sizes );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLBINDTEXTURESPROC( GLuint first, GLsizei count, GLuint* textures );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLBINDSAMPLERSPROC( GLuint first, GLsizei count, GLuint* samplers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLBINDIMAGETEXTURESPROC( GLuint first, GLsizei count, GLuint* textures );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLBINDVERTEXBUFFERSPROC( GLuint first, GLsizei count, GLuint* buffers,
                                                             GLintptr* offsets,
                                                             GLsizei* strides );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLIPCONTROLPROC( GLenum origin, GLenum depth );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCREATETRANSFORMFEEDBACKSPROC( GLsizei n, GLuint* ids );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTRANSFORMFEEDBACKBUFFERBASEPROC( GLuint xfb, GLuint index, GLuint buffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void
        PFNGLTRANSFORMFEEDBACKBUFFERRANGEPROC( GLuint xfb, GLuint index, GLuint buffer, GLintptr offset,
                                               GLsizeiptr size );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTRANSFORMFEEDBACKIVPROC( GLuint xfb, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTRANSFORMFEEDBACKI_VPROC( GLuint xfb, GLenum pname, GLuint index,
                                                                   GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTRANSFORMFEEDBACKI64_VPROC( GLuint xfb, GLenum pname, GLuint index,
                                                                     GLint64* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCREATEBUFFERSPROC( GLsizei n, GLuint* buffers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLNAMEDBUFFERSTORAGEPROC( GLuint buffer, GLsizeiptr size, IntPtr data, GLbitfield flags );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLNAMEDBUFFERDATAPROC( GLuint buffer, GLsizeiptr size, IntPtr data, GLenum usage );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLNAMEDBUFFERSUBDATAPROC( GLuint buffer, GLintptr offset, GLsizeiptr size, IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOPYNAMEDBUFFERSUBDATAPROC( GLuint readBuffer, GLuint writeBuffer, GLintptr readOffset,
                                                           GLintptr writeOffset,
                                                           GLsizeiptr size );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLEARNAMEDBUFFERDATAPROC( GLuint buffer, GLenum internalFormat, GLenum format,
                                                         GLenum type, IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLEARNAMEDBUFFERSUBDATAPROC( GLuint buffer, GLenum internalFormat, GLintptr offset,
                                                            GLsizeiptr size,
                                                            GLenum format, GLenum type, IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate IntPtr PFNGLMAPNAMEDBUFFERPROC( GLuint buffer, GLenum access );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate IntPtr PFNGLMAPNAMEDBUFFERRANGEPROC( GLuint buffer, GLintptr offset, GLsizeiptr length,
                                                          GLbitfield access );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLboolean PFNGLUNMAPNAMEDBUFFERPROC( GLuint buffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLFLUSHMAPPEDNAMEDBUFFERRANGEPROC( GLuint buffer, GLintptr offset, GLsizeiptr length );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETNAMEDBUFFERPARAMETERIVPROC( GLuint buffer, GLenum pname, GLint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETNAMEDBUFFERPARAMETERI64VPROC( GLuint buffer, GLenum pname,
                                                                       GLint64* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETNAMEDBUFFERPOINTERVPROC( GLuint buffer, GLenum pname, IntPtr* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void
        PFNGLGETNAMEDBUFFERSUBDATAPROC( GLuint buffer, GLintptr offset, GLsizeiptr size, IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCREATEFRAMEBUFFERSPROC( GLsizei n, GLuint* framebuffers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLNAMEDFRAMEBUFFERRENDERBUFFERPROC( GLuint framebuffer, GLenum attachment,
                                                                 GLenum renderbuffertarget,
                                                                 GLuint renderbuffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLNAMEDFRAMEBUFFERPARAMETERIPROC( GLuint framebuffer, GLenum pname, GLint param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLNAMEDFRAMEBUFFERTEXTUREPROC( GLuint framebuffer, GLenum attachment, GLuint texture,
                                                            GLint level );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLNAMEDFRAMEBUFFERTEXTURELAYERPROC( GLuint framebuffer, GLenum attachment, GLuint texture,
                                                                 GLint level,
                                                                 GLint layer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLNAMEDFRAMEBUFFERDRAWBUFFERPROC( GLuint framebuffer, GLenum buf );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLNAMEDFRAMEBUFFERDRAWBUFFERSPROC( GLuint framebuffer, GLsizei n, GLenum* bufs );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLNAMEDFRAMEBUFFERREADBUFFERPROC( GLuint framebuffer, GLenum src );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLINVALIDATENAMEDFRAMEBUFFERDATAPROC(
        GLuint framebuffer, GLsizei numAttachments, GLenum* attachments );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLINVALIDATENAMEDFRAMEBUFFERSUBDATAPROC( GLuint framebuffer, GLsizei numAttachments,
                                                                             GLenum* attachments, GLint x, GLint y,
                                                                             GLsizei width,
                                                                             GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCLEARNAMEDFRAMEBUFFERIVPROC( GLuint framebuffer, GLenum buffer, GLint drawbuffer,
                                                                   GLint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCLEARNAMEDFRAMEBUFFERUIVPROC( GLuint framebuffer, GLenum buffer, GLint drawbuffer,
                                                                    GLuint* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCLEARNAMEDFRAMEBUFFERFVPROC( GLuint framebuffer, GLenum buffer, GLint drawbuffer,
                                                                   GLfloat* value );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCLEARNAMEDFRAMEBUFFERFIPROC( GLuint framebuffer, GLenum buffer, GLfloat depth,
                                                            GLint stencil );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBLITNAMEDFRAMEBUFFERPROC( GLuint readFramebuffer, GLuint drawFramebuffer, GLint srcX0,
                                                         GLint srcY0,
                                                         GLint srcX1, GLint srcY1, GLint dstX0, GLint dstY0,
                                                         GLint dstX1, GLint dstY1,
                                                         GLbitfield mask, GLenum filter );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLenum PFNGLCHECKNAMEDFRAMEBUFFERSTATUSPROC( GLuint framebuffer, GLenum target );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETNAMEDFRAMEBUFFERPARAMETERIVPROC(
        GLuint framebuffer, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETNAMEDFRAMEBUFFERATTACHMENTPARAMETERIVPROC(
        GLuint framebuffer, GLenum attachment, GLenum pname,
        GLint* params_ );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCREATERENDERBUFFERSPROC( GLsizei n, GLuint* renderbuffers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLNAMEDRENDERBUFFERSTORAGEPROC( GLuint renderbuffer, GLenum internalFormat, GLsizei width,
                                                             GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLNAMEDRENDERBUFFERSTORAGEMULTISAMPLEPROC( GLuint renderbuffer, GLsizei samples,
                                                                        GLenum internalFormat,
                                                                        GLsizei width, GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETNAMEDRENDERBUFFERPARAMETERIVPROC(
        GLuint renderbuffer, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCREATETEXTURESPROC( GLenum target, GLsizei n, GLuint* textures );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTUREBUFFERPROC( GLuint texture, GLenum internalFormat, GLuint buffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTUREBUFFERRANGEPROC( GLuint texture, GLenum internalFormat, GLuint buffer,
                                                       GLintptr offset,
                                                       GLsizeiptr size );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTURESTORAGE1DPROC( GLuint texture, GLsizei levels, GLenum internalFormat,
                                                     GLsizei width );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTURESTORAGE2DPROC( GLuint texture, GLsizei levels, GLenum internalFormat,
                                                     GLsizei width, GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTURESTORAGE3DPROC( GLuint texture, GLsizei levels, GLenum internalFormat,
                                                     GLsizei width, GLsizei height,
                                                     GLsizei depth );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTURESTORAGE2DMULTISAMPLEPROC( GLuint texture, GLsizei samples, GLenum internalFormat,
                                                                GLsizei width,
                                                                GLsizei height, GLboolean fixedsamplelocations );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTURESTORAGE3DMULTISAMPLEPROC( GLuint texture, GLsizei samples, GLenum internalFormat,
                                                                GLsizei width,
                                                                GLsizei height, GLsizei depth,
                                                                GLboolean fixedsamplelocations );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTURESUBIMAGE1DPROC( GLuint texture, GLint level, GLint xoffset, GLsizei width,
                                                      GLenum format, GLenum type,
                                                      IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTURESUBIMAGE2DPROC( GLuint texture, GLint level, GLint xoffset, GLint yoffset,
                                                      GLsizei width,
                                                      GLsizei height, GLenum format, GLenum type, IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTURESUBIMAGE3DPROC( GLuint texture, GLint level, GLint xoffset, GLint yoffset,
                                                      GLint zoffset,
                                                      GLsizei width, GLsizei height, GLsizei depth, GLenum format,
                                                      GLenum type,
                                                      IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOMPRESSEDTEXTURESUBIMAGE1DPROC( GLuint texture, GLint level, GLint xoffset,
                                                                GLsizei width, GLenum format,
                                                                GLsizei imageSize, IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOMPRESSEDTEXTURESUBIMAGE2DPROC( GLuint texture, GLint level, GLint xoffset,
                                                                GLint yoffset, GLsizei width,
                                                                GLsizei height, GLenum format, GLsizei imageSize,
                                                                IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOMPRESSEDTEXTURESUBIMAGE3DPROC( GLuint texture, GLint level, GLint xoffset,
                                                                GLint yoffset, GLint zoffset,
                                                                GLsizei width, GLsizei height, GLsizei depth,
                                                                GLenum format,
                                                                GLsizei imageSize, IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOPYTEXTURESUBIMAGE1DPROC( GLuint texture, GLint level, GLint xoffset, GLint x, GLint y,
                                                          GLsizei width );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOPYTEXTURESUBIMAGE2DPROC( GLuint texture, GLint level, GLint xoffset, GLint yoffset,
                                                          GLint x, GLint y,
                                                          GLsizei width, GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLCOPYTEXTURESUBIMAGE3DPROC( GLuint texture, GLint level, GLint xoffset, GLint yoffset,
                                                          GLint zoffset, GLint x,
                                                          GLint y, GLsizei width, GLsizei height );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTUREPARAMETERFPROC( GLuint texture, GLenum pname, GLfloat param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLTEXTUREPARAMETERFVPROC( GLuint texture, GLenum pname, GLfloat* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTUREPARAMETERIPROC( GLuint texture, GLenum pname, GLint param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLTEXTUREPARAMETERIIVPROC( GLuint texture, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLTEXTUREPARAMETERIUIVPROC( GLuint texture, GLenum pname, GLuint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLTEXTUREPARAMETERIVPROC( GLuint texture, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGENERATETEXTUREMIPMAPPROC( GLuint texture );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLBINDTEXTUREUNITPROC( GLuint unit, GLuint texture );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETTEXTUREIMAGEPROC( GLuint texture, GLint level, GLenum format, GLenum type,
                                                    GLsizei bufSize,
                                                    IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETCOMPRESSEDTEXTUREIMAGEPROC( GLuint texture, GLint level, GLsizei bufSize,
                                                              IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTEXTURELEVELPARAMETERFVPROC( GLuint texture, GLint level, GLenum pname,
                                                                      GLfloat* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTEXTURELEVELPARAMETERIVPROC( GLuint texture, GLint level, GLenum pname,
                                                                      GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTEXTUREPARAMETERFVPROC( GLuint texture, GLenum pname, GLfloat* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTEXTUREPARAMETERIIVPROC( GLuint texture, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTEXTUREPARAMETERIUIVPROC( GLuint texture, GLenum pname, GLuint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETTEXTUREPARAMETERIVPROC( GLuint texture, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCREATEVERTEXARRAYSPROC( GLsizei n, GLuint* arrays );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLDISABLEVERTEXARRAYATTRIBPROC( GLuint vaobj, GLuint index );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLENABLEVERTEXARRAYATTRIBPROC( GLuint vaobj, GLuint index );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXARRAYELEMENTBUFFERPROC( GLuint vaobj, GLuint buffer );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXARRAYVERTEXBUFFERPROC( GLuint vaobj, GLuint bindingindex, GLuint buffer,
                                                            GLintptr offset,
                                                            GLsizei stride );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLVERTEXARRAYVERTEXBUFFERSPROC( GLuint vaobj, GLuint first, GLsizei count,
                                                                    GLuint* buffers,
                                                                    GLintptr* offsets, GLsizei* strides );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXARRAYATTRIBBINDINGPROC( GLuint vaobj, GLuint attribindex, GLuint bindingindex );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXARRAYATTRIBFORMATPROC( GLuint vaobj, GLuint attribindex, GLint size, GLenum type,
                                                            GLboolean normalized,
                                                            GLuint relativeoffset );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXARRAYATTRIBIFORMATPROC( GLuint vaobj, GLuint attribindex, GLint size, GLenum type,
                                                             GLuint relativeoffset );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXARRAYATTRIBLFORMATPROC( GLuint vaobj, GLuint attribindex, GLint size, GLenum type,
                                                             GLuint relativeoffset );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLVERTEXARRAYBINDINGDIVISORPROC( GLuint vaobj, GLuint bindingindex, GLuint divisor );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETVERTEXARRAYIVPROC( GLuint vaobj, GLenum pname, GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETVERTEXARRAYINDEXEDIVPROC( GLuint vaobj, GLuint index, GLenum pname,
                                                                   GLint* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETVERTEXARRAYINDEXED64IVPROC( GLuint vaobj, GLuint index, GLenum pname,
                                                                     GLint64* param );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCREATESAMPLERSPROC( GLsizei n, GLuint* samplers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCREATEPROGRAMPIPELINESPROC( GLsizei n, GLuint* pipelines );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLCREATEQUERIESPROC( GLenum target, GLsizei n, GLuint* ids );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETQUERYBUFFEROBJECTI64VPROC( GLuint id, GLuint buffer, GLenum pname, GLintptr offset );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETQUERYBUFFEROBJECTIVPROC( GLuint id, GLuint buffer, GLenum pname, GLintptr offset );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETQUERYBUFFEROBJECTUI64VPROC( GLuint id, GLuint buffer, GLenum pname, GLintptr offset );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETQUERYBUFFEROBJECTUIVPROC( GLuint id, GLuint buffer, GLenum pname, GLintptr offset );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLMEMORYBARRIERBYREGIONPROC( GLbitfield barriers );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETTEXTURESUBIMAGEPROC( GLuint texture, GLint level, GLint xoffset, GLint yoffset,
                                                       GLint zoffset,
                                                       GLsizei width, GLsizei height, GLsizei depth, GLenum format,
                                                       GLenum type,
                                                       GLsizei bufSize, IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETCOMPRESSEDTEXTURESUBIMAGEPROC( GLuint texture, GLint level, GLint xoffset,
                                                                 GLint yoffset, GLint zoffset,
                                                                 GLsizei width, GLsizei height, GLsizei depth,
                                                                 GLsizei bufSize,
                                                                 IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate GLenum PFNGLGETGRAPHICSRESETSTATUSPROC();

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETNCOMPRESSEDTEXIMAGEPROC( GLenum target, GLint lod, GLsizei bufSize, IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLGETNTEXIMAGEPROC( GLenum target, GLint level, GLenum format, GLenum type,
                                                 GLsizei bufSize, IntPtr pixels );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETNUNIFORMDVPROC( GLuint program, GLint location, GLsizei bufSize,
                                                         GLdouble* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETNUNIFORMFVPROC( GLuint program, GLint location, GLsizei bufSize,
                                                         GLfloat* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETNUNIFORMIVPROC( GLuint program, GLint location, GLsizei bufSize,
                                                         GLint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLGETNUNIFORMUIVPROC( GLuint program, GLint location, GLsizei bufSize,
                                                          GLuint* parameters );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLREADNPIXELSPROC( GLint x, GLint y, GLsizei width, GLsizei height, GLenum format,
                                                GLenum type,
                                                GLsizei bufSize, IntPtr data );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLTEXTUREBARRIERPROC();

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private unsafe delegate void PFNGLSPECIALIZESHADERPROC( GLuint shader, GLchar* pEntryPoint,
                                                            GLuint numSpecializationConstants,
                                                            GLuint* pConstantIndex, GLuint* pConstantValue );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLMULTIDRAWARRAYSINDIRECTCOUNTPROC( GLenum mode, IntPtr indirect, GLintptr drawcount,
                                                                 GLsizei maxdrawcount,
                                                                 GLsizei stride );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLMULTIDRAWELEMENTSINDIRECTCOUNTPROC( GLenum mode, GLenum type, IntPtr indirect,
                                                                   GLintptr drawcount,
                                                                   GLsizei maxdrawcount, GLsizei stride );

    [UnmanagedFunctionPointer( CallingConvention.Cdecl )]
    private delegate void PFNGLPOLYGONOFFSETCLAMPPROC( GLfloat factor, GLfloat units, GLfloat clamp );
}

// ============================================================================
// ============================================================================