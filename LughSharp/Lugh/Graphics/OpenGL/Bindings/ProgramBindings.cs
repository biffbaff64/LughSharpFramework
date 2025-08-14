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

// ============================================================================

using System.Text;

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

// ============================================================================

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

namespace LughSharp.Lugh.Graphics.OpenGL.Bindings;

public unsafe partial class GLBindings : IGLBindings
{
    /// <inheritdoc />
    public GLuint CreateProgram()
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLCREATEPROGRAMPROC >( "glCreateProgram", out _glCreateProgram );

        //TODO: Error checking here

        return _glCreateProgram();
    }

    // ========================================================================

    /// <inheritdoc />
    public GLboolean IsProgram( GLint program )
    {
        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLISPROGRAMPROC >( "glIsProgram", out _glIsProgram );

        return _glIsProgram( ( uint )program );
    }

    // ========================================================================

    /// <inheritdoc />
    public void LinkProgram( GLint program )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLLINKPROGRAMPROC >( "glLinkProgram", out _glLinkProgram );

        _glLinkProgram( ( uint )program );
    }

    // ========================================================================

    /// <inheritdoc />
    public void UseProgram( GLint program )
    {
        if ( program == INVALID_SHADER_PROGRAM )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLUSEPROGRAMPROC >( "glUseProgram", out _glUseProgram );

        _glUseProgram( ( uint )program );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetProgramiv( GLint program, GLenum pname, GLint* parameters )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMIVPROC >( "glGetProgramiv", out _glGetProgramiv );

        _glGetProgramiv( ( uint )program, pname, parameters );
    }

    /// <inheritdoc />
    public void GetProgramiv( GLint program, GLenum pname, ref GLint[] parameters )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMIVPROC >( "glGetProgramiv", out _glGetProgramiv );

        fixed ( GLint* pparams = &parameters[ 0 ] )
        {
            _glGetProgramiv( ( uint )program, pname, pparams );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetProgramInfoLog( GLint program, GLsizei bufSize, GLsizei* length, GLchar* infoLog )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMINFOLOGPROC >( "glGetProgramInfoLog", out _glGetProgramInfoLog );

        _glGetProgramInfoLog( ( uint )program, bufSize, length, infoLog );
    }

    /// <inheritdoc />
    public string GetProgramInfoLog( GLint program, GLsizei bufSize )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        var     infoLog = stackalloc GLchar[ bufSize ];
        GLsizei len;

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMINFOLOGPROC >( "glGetProgramInfoLog", out _glGetProgramInfoLog );

        _glGetProgramInfoLog( ( uint )program, bufSize, &len, infoLog );

        return new string( ( GLbyte* )infoLog, 0, len, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public bool ValidateProgram( int program )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLVALIDATEPROGRAMPROC >( "glValidateProgram", out _glValidateProgram );

        return _glValidateProgram( ( uint )program );
    }

    // ========================================================================

    public void GetProgramStageiv( GLuint program, GLenum shadertype, GLenum pname, GLint* values )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMSTAGEIVPROC >( "glGetProgramStageiv", out _glGetProgramStageiv );

        _glGetProgramStageiv( ( uint )program, shadertype, pname, values );
    }

    public void GetProgramStageiv( GLuint program, GLenum shadertype, GLenum pname, ref GLint[] values )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMSTAGEIVPROC >( "glGetProgramStageiv", out _glGetProgramStageiv );

        fixed ( GLint* p = &values[ 0 ] )
        {
            _glGetProgramStageiv( ( uint )program, shadertype, pname, p );
        }
    }

    // ========================================================================

    public void PatchParameteri( GLenum pname, GLint value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPATCHPARAMETERIPROC >( "glPatchParameteri", out _glPatchParameteri );

        _glPatchParameteri( pname, value );
    }

    // ========================================================================

    public void GetProgramBinary( GLuint program, GLsizei bufSize, GLsizei* length, GLenum* binaryFormat, IntPtr binary )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMBINARYPROC >( "glGetProgramBinary", out _glGetProgramBinary );

        _glGetProgramBinary( ( uint )program, bufSize, length, binaryFormat, binary );
    }

    public byte[] GetProgramBinary( GLuint program, GLsizei bufSize, out GLenum binaryFormat )
    {
        var     binary = new byte[ bufSize ];
        GLsizei length;

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMBINARYPROC >( "glGetProgramBinary", out _glGetProgramBinary );

        fixed ( byte* pBinary = &binary[ 0 ] )
        {
            fixed ( GLenum* pBinaryFormat = &binaryFormat )
            {
                _glGetProgramBinary( ( uint )program, bufSize, &length, pBinaryFormat, ( IntPtr )pBinary );
            }
        }

        Array.Resize( ref binary, length );

        return binary;
    }

    // ========================================================================

    public void ProgramBinary( GLuint program, GLenum binaryFormat, IntPtr binary, GLsizei length )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMBINARYPROC >( "glProgramBinary", out _glProgramBinary );

        _glProgramBinary( ( uint )program, binaryFormat, binary, length );
    }

    public void ProgramBinary( GLuint program, GLenum binaryFormat, byte[] binary )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMBINARYPROC >( "glProgramBinary", out _glProgramBinary );

        fixed ( byte* pBinary = &binary[ 0 ] )
        {
            _glProgramBinary( ( uint )program, binaryFormat, ( IntPtr )pBinary, binary.Length );
        }
    }

    // ========================================================================

    public void ProgramParameteri( GLuint program, GLenum pname, GLint value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMPARAMETERIPROC >( "glProgramParameteri", out _glProgramParameteri );

        _glProgramParameteri( ( uint )program, pname, value );
    }

    // ========================================================================

    public void UseProgramStages( GLuint pipeline, GLbitfield stages, GLuint program )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLUSEPROGRAMSTAGESPROC >( "glUseProgramStages", out _glUseProgramStages );

        _glUseProgramStages( pipeline, stages, ( uint )program );
    }

    // ========================================================================

    public void ActiveShaderProgram( GLuint pipeline, GLuint program )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLACTIVESHADERPROGRAMPROC >( "glActiveShaderProgram", out _glActiveShaderProgram );

        _glActiveShaderProgram( pipeline, ( uint )program );
    }

    // ========================================================================

    public GLuint CreateShaderProgramv( GLenum type, GLsizei count, GLchar** strings )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLCREATESHADERPROGRAMVPROC >( "glCreateShaderProgramv", out _glCreateShaderProgramv );

        return _glCreateShaderProgramv( type, count, strings );
    }

    public GLuint CreateShaderProgramv( GLenum type, string[] strings )
    {
        var stringsBytes = new GLchar[ strings.Length ][];

        for ( var i = 0; i < strings.Length; i++ )
        {
            stringsBytes[ i ] = Encoding.UTF8.GetBytes( strings[ i ] );
        }

        var stringsPtrs = new GLchar*[ strings.Length ];

        for ( var i = 0; i < strings.Length; i++ )
        {
            fixed ( GLchar* pString = &stringsBytes[ i ][ 0 ] )
            {
                stringsPtrs[ i ] = pString;
            }
        }

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLCREATESHADERPROGRAMVPROC >( "glCreateShaderProgramv", out _glCreateShaderProgramv );

        fixed ( GLchar** pStrings = &stringsPtrs[ 0 ] )
        {
            return _glCreateShaderProgramv( type, strings.Length, pStrings );
        }
    }

    // ========================================================================

    public void BindProgramPipeline( GLuint pipeline )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLBINDPROGRAMPIPELINEPROC >( "glBindProgramPipeline", out _glBindProgramPipeline );

        _glBindProgramPipeline( pipeline );
    }

    // ========================================================================

    public void DeleteProgramPipelines( GLsizei n, GLuint* pipelines )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLDELETEPROGRAMPIPELINESPROC >( "glDeleteProgramPipelines", out _glDeleteProgramPipelines );

        _glDeleteProgramPipelines( n, pipelines );
    }

    public void DeleteProgramPipelines( params GLuint[] pipelines )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLDELETEPROGRAMPIPELINESPROC >( "glDeleteProgramPipelines", out _glDeleteProgramPipelines );

        fixed ( GLuint* pPipelines = &pipelines[ 0 ] )
        {
            _glDeleteProgramPipelines( pipelines.Length, pPipelines );
        }
    }

    // ========================================================================

    public void GenProgramPipelines( GLsizei n, GLuint* pipelines )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGENPROGRAMPIPELINESPROC >( "glGenProgramPipelines", out _glGenProgramPipelines );

        _glGenProgramPipelines( n, pipelines );
    }

    public GLuint[] GenProgramPipelines( GLsizei n )
    {
        var pipelines = new GLuint[ n ];

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGENPROGRAMPIPELINESPROC >( "glGenProgramPipelines", out _glGenProgramPipelines );

        fixed ( GLuint* pPipelines = &pipelines[ 0 ] )
        {
            _glGenProgramPipelines( n, pPipelines );
        }

        return pipelines;
    }

    public GLuint GenProgramPipeline()
    {
        return GenProgramPipelines( 1 )[ 0 ];
    }

    // ========================================================================

    public GLboolean IsProgramPipeline( GLuint pipeline )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLISPROGRAMPIPELINEPROC >( "glIsProgramPipeline", out _glIsProgramPipeline );

        return _glIsProgramPipeline( pipeline );
    }

    // ========================================================================

    public void GetProgramPipelineiv( GLuint pipeline, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMPIPELINEIVPROC >( "glGetProgramPipelineiv", out _glGetProgramPipelineiv );

        _glGetProgramPipelineiv( pipeline, pname, param );
    }

    public void GetProgramPipelineiv( GLuint pipeline, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMPIPELINEIVPROC >( "glGetProgramPipelineiv", out _glGetProgramPipelineiv );

        fixed ( GLint* pParam = &param[ 0 ] )
        {
            _glGetProgramPipelineiv( pipeline, pname, pParam );
        }
    }

    // ========================================================================

    public void ProgramUniform1i( GLuint program, GLint location, GLint v0 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM1IPROC >( "glProgramUniform1i", out _glProgramUniform1i );

        _glProgramUniform1i( ( uint )program, location, v0 );
    }

    // ========================================================================

    public void ProgramUniform1iv( GLuint program, GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM1IVPROC >( "glProgramUniform1iv", out _glProgramUniform1iv );

        _glProgramUniform1iv( ( uint )program, location, count, value );
    }

    public void ProgramUniform1iv( GLuint program, GLint location, GLint[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM1IVPROC >( "glProgramUniform1iv", out _glProgramUniform1iv );

        fixed ( GLint* pValue = &value[ 0 ] )
        {
            _glProgramUniform1iv( ( uint )program, location, value.Length, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform1f( GLuint program, GLint location, GLfloat v0 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM1FPROC >( "glProgramUniform1f", out _glProgramUniform1f );

        _glProgramUniform1f( ( uint )program, location, v0 );
    }

    // ========================================================================

    public void ProgramUniform1fv( GLuint program, GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM1FVPROC >( "glProgramUniform1fv", out _glProgramUniform1fv );

        _glProgramUniform1fv( ( uint )program, location, count, value );
    }

    public void ProgramUniform1fv( GLuint program, GLint location, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM1FVPROC >( "glProgramUniform1fv", out _glProgramUniform1fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniform1fv( ( uint )program, location, value.Length, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform1d( GLuint program, GLint location, GLdouble v0 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM1DPROC >( "glProgramUniform1d", out _glProgramUniform1d );

        _glProgramUniform1d( ( uint )program, location, v0 );
    }

    // ========================================================================

    public void ProgramUniform1dv( GLuint program, GLint location, GLsizei count, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM1DVPROC >( "glProgramUniform1dv", out _glProgramUniform1dv );

        _glProgramUniform1dv( ( uint )program, location, count, value );
    }

    public void ProgramUniform1dv( GLuint program, GLint location, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM1DVPROC >( "glProgramUniform1dv", out _glProgramUniform1dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniform1dv( ( uint )program, location, value.Length, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform1ui( GLuint program, GLint location, GLuint v0 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM1UIPROC >( "glProgramUniform1ui", out _glProgramUniform1ui );

        _glProgramUniform1ui( ( uint )program, location, v0 );
    }

    // ========================================================================

    public void ProgramUniform1uiv( GLuint program, GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM1UIVPROC >( "glProgramUniform1uiv", out _glProgramUniform1uiv );

        _glProgramUniform1uiv( ( uint )program, location, count, value );
    }

    public void ProgramUniform1uiv( GLuint program, GLint location, GLuint[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM1UIVPROC >( "glProgramUniform1uiv", out _glProgramUniform1uiv );

        fixed ( GLuint* pValue = &value[ 0 ] )
        {
            _glProgramUniform1uiv( ( uint )program, location, value.Length, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform2i( GLuint program, GLint location, GLint v0, GLint v1 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM2IPROC >( "glProgramUniform2i", out _glProgramUniform2i );

        _glProgramUniform2i( ( uint )program, location, v0, v1 );
    }

    // ========================================================================

    public void ProgramUniform2iv( GLuint program, GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM2IVPROC >( "glProgramUniform2iv", out _glProgramUniform2iv );

        _glProgramUniform2iv( ( uint )program, location, count, value );
    }

    public void ProgramUniform2iv( GLuint program, GLint location, GLint[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM2IVPROC >( "glProgramUniform2iv", out _glProgramUniform2iv );

        fixed ( GLint* pValue = &value[ 0 ] )
        {
            _glProgramUniform2iv( ( uint )program, location, value.Length / 2, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform2f( GLuint program, GLint location, GLfloat v0, GLfloat v1 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM2FPROC >( "glProgramUniform2f", out _glProgramUniform2f );

        _glProgramUniform2f( ( uint )program, location, v0, v1 );
    }

    // ========================================================================

    public void ProgramUniform2fv( GLuint program, GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM2FVPROC >( "glProgramUniform2fv", out _glProgramUniform2fv );

        _glProgramUniform2fv( ( uint )program, location, count, value );
    }

    public void ProgramUniform2fv( GLuint program, GLint location, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM2FVPROC >( "glProgramUniform2fv", out _glProgramUniform2fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniform2fv( ( uint )program, location, value.Length / 2, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform2d( GLuint program, GLint location, GLdouble v0, GLdouble v1 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM2DPROC >( "glProgramUniform2d", out _glProgramUniform2d );

        _glProgramUniform2d( ( uint )program, location, v0, v1 );
    }

    // ========================================================================

    public void ProgramUniform2dv( GLuint program, GLint location, GLsizei count, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM2DVPROC >( "glProgramUniform2dv", out _glProgramUniform2dv );

        _glProgramUniform2dv( ( uint )program, location, count, value );
    }

    public void ProgramUniform2dv( GLuint program, GLint location, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM2DVPROC >( "glProgramUniform2dv", out _glProgramUniform2dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniform2dv( ( uint )program, location, value.Length / 2, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform2ui( GLuint program, GLint location, GLuint v0, GLuint v1 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM2UIPROC >( "glProgramUniform2ui", out _glProgramUniform2ui );

        _glProgramUniform2ui( ( uint )program, location, v0, v1 );
    }

    // ========================================================================

    public void ProgramUniform2uiv( GLuint program, GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM2UIVPROC >( "glProgramUniform2uiv", out _glProgramUniform2uiv );

        _glProgramUniform2uiv( ( uint )program, location, count, value );
    }

    public void ProgramUniform2uiv( GLuint program, GLint location, GLuint[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM2UIVPROC >( "glProgramUniform2uiv", out _glProgramUniform2uiv );

        fixed ( GLuint* pValue = &value[ 0 ] )
        {
            _glProgramUniform2uiv( ( uint )program, location, value.Length / 2, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform3i( GLuint program, GLint location, GLint v0, GLint v1, GLint v2 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM3IPROC >( "glProgramUniform3i", out _glProgramUniform3i );

        _glProgramUniform3i( ( uint )program, location, v0, v1, v2 );
    }

    // ========================================================================

    public void ProgramUniform3iv( GLuint program, GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM3IVPROC >( "glProgramUniform3iv", out _glProgramUniform3iv );

        _glProgramUniform3iv( ( uint )program, location, count, value );
    }

    public void ProgramUniform3iv( GLuint program, GLint location, GLint[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM3IVPROC >( "glProgramUniform3iv", out _glProgramUniform3iv );

        fixed ( GLint* pValue = &value[ 0 ] )
        {
            _glProgramUniform3iv( ( uint )program, location, value.Length / 3, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform3f( GLuint program, GLint location, GLfloat v0, GLfloat v1, GLfloat v2 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM3FPROC >( "glProgramUniform3f", out _glProgramUniform3f );

        _glProgramUniform3f( ( uint )program, location, v0, v1, v2 );
    }

    // ========================================================================

    public void ProgramUniform3fv( GLuint program, GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM3FVPROC >( "glProgramUniform3fv", out _glProgramUniform3fv );

        _glProgramUniform3fv( ( uint )program, location, count, value );
    }

    public void ProgramUniform3fv( GLuint program, GLint location, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM3FVPROC >( "glProgramUniform3fv", out _glProgramUniform3fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniform3fv( ( uint )program, location, value.Length / 3, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform3d( GLuint program, GLint location, GLdouble v0, GLdouble v1, GLdouble v2 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM3DPROC >( "glProgramUniform3d", out _glProgramUniform3d );

        _glProgramUniform3d( ( uint )program, location, v0, v1, v2 );
    }

    // ========================================================================

    public void ProgramUniform3dv( GLuint program, GLint location, GLsizei count, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM3DVPROC >( "glProgramUniform3dv", out _glProgramUniform3dv );

        _glProgramUniform3dv( ( uint )program, location, count, value );
    }

    public void ProgramUniform3dv( GLuint program, GLint location, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM3DVPROC >( "glProgramUniform3dv", out _glProgramUniform3dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniform3dv( ( uint )program, location, value.Length / 3, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform3ui( GLuint program, GLint location, GLuint v0, GLuint v1, GLuint v2 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM3UIPROC >( "glProgramUniform3ui", out _glProgramUniform3ui );

        _glProgramUniform3ui( ( uint )program, location, v0, v1, v2 );
    }

    // ========================================================================

    public void ProgramUniform3uiv( GLuint program, GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM3UIVPROC >( "glProgramUniform3uiv", out _glProgramUniform3uiv );

        _glProgramUniform3uiv( ( uint )program, location, count, value );
    }

    public void ProgramUniform3uiv( GLuint program, GLint location, GLuint[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM3UIVPROC >( "glProgramUniform3uiv", out _glProgramUniform3uiv );

        fixed ( GLuint* pValue = &value[ 0 ] )
        {
            _glProgramUniform3uiv( ( uint )program, location, value.Length / 3, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform4i( GLuint program, GLint location, GLint v0, GLint v1, GLint v2, GLint v3 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM4IPROC >( "glProgramUniform4i", out _glProgramUniform4i );

        _glProgramUniform4i( ( uint )program, location, v0, v1, v2, v3 );
    }

    // ========================================================================

    public void ProgramUniform4iv( GLuint program, GLint location, GLsizei count, GLint* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM4IVPROC >( "glProgramUniform4iv", out _glProgramUniform4iv );

        _glProgramUniform4iv( ( uint )program, location, count, value );
    }

    public void ProgramUniform4iv( GLuint program, GLint location, GLint[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM4IVPROC >( "glProgramUniform4iv", out _glProgramUniform4iv );

        fixed ( GLint* pValue = &value[ 0 ] )
        {
            _glProgramUniform4iv( ( uint )program, location, value.Length / 4, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform4f( GLuint program, GLint location, GLfloat v0, GLfloat v1, GLfloat v2, GLfloat v3 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM4FPROC >( "glProgramUniform4f", out _glProgramUniform4f );

        _glProgramUniform4f( ( uint )program, location, v0, v1, v2, v3 );
    }

    // ========================================================================

    public void ProgramUniform4fv( GLuint program, GLint location, GLsizei count, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM4FVPROC >( "glProgramUniform4fv", out _glProgramUniform4fv );

        _glProgramUniform4fv( ( uint )program, location, count, value );
    }

    public void ProgramUniform4fv( GLuint program, GLint location, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM4FVPROC >( "glProgramUniform4fv", out _glProgramUniform4fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniform4fv( ( uint )program, location, value.Length / 4, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform4d( GLuint program, GLint location, GLdouble v0, GLdouble v1, GLdouble v2, GLdouble v3 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM4DPROC >( "glProgramUniform4d", out _glProgramUniform4d );

        _glProgramUniform4d( ( uint )program, location, v0, v1, v2, v3 );
    }

    // ========================================================================

    public void ProgramUniform4dv( GLuint program, GLint location, GLsizei count, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM4DVPROC >( "glProgramUniform4dv", out _glProgramUniform4dv );

        _glProgramUniform4dv( ( uint )program, location, count, value );
    }

    public void ProgramUniform4dv( GLuint program, GLint location, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM4DVPROC >( "glProgramUniform4dv", out _glProgramUniform4dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniform4dv( ( uint )program, location, value.Length / 4, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniform4ui( GLuint program, GLint location, GLuint v0, GLuint v1, GLuint v2, GLuint v3 )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM4UIPROC >( "glProgramUniform4ui", out _glProgramUniform4ui );

        _glProgramUniform4ui( ( uint )program, location, v0, v1, v2, v3 );
    }

    // ========================================================================

    public void ProgramUniform4uiv( GLuint program, GLint location, GLsizei count, GLuint* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM4UIVPROC >( "glProgramUniform4uiv", out _glProgramUniform4uiv );

        _glProgramUniform4uiv( ( uint )program, location, count, value );
    }

    public void ProgramUniform4uiv( GLuint program, GLint location, GLuint[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORM4UIVPROC >( "glProgramUniform4uiv", out _glProgramUniform4uiv );

        fixed ( GLuint* pValue = &value[ 0 ] )
        {
            _glProgramUniform4uiv( ( uint )program, location, value.Length / 4, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix2fv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX2FVPROC >( "glProgramUniformMatrix2fv", out _glProgramUniformMatrix2fv );

        _glProgramUniformMatrix2fv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix2fv( GLuint program, GLint location, GLboolean transpose, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX2FVPROC >( "glProgramUniformMatrix2fv", out _glProgramUniformMatrix2fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix2fv( ( uint )program, location, value.Length / 4, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix3fv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX3FVPROC >( "glProgramUniformMatrix3fv", out _glProgramUniformMatrix3fv );

        _glProgramUniformMatrix3fv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix3fv( GLuint program, GLint location, GLboolean transpose, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX3FVPROC >( "glProgramUniformMatrix3fv", out _glProgramUniformMatrix3fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix3fv( ( uint )program, location, value.Length / 9, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix4fv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX4FVPROC >( "glProgramUniformMatrix4fv", out _glProgramUniformMatrix4fv );

        _glProgramUniformMatrix4fv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix4fv( GLuint program, GLint location, GLboolean transpose, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX4FVPROC >( "glProgramUniformMatrix4fv", out _glProgramUniformMatrix4fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix4fv( ( uint )program, location, value.Length / 16, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix2dv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX2DVPROC >( "glProgramUniformMatrix2dv", out _glProgramUniformMatrix2dv );

        _glProgramUniformMatrix2dv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix2dv( GLuint program, GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX2DVPROC >( "glProgramUniformMatrix2dv", out _glProgramUniformMatrix2dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix2dv( ( uint )program, location, value.Length / 4, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix3dv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX3DVPROC >( "glProgramUniformMatrix3dv", out _glProgramUniformMatrix3dv );

        _glProgramUniformMatrix3dv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix3dv( GLuint program, GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX3DVPROC >( "glProgramUniformMatrix3dv", out _glProgramUniformMatrix3dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix3dv( ( uint )program, location, value.Length / 9, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix4dv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX4DVPROC >( "glProgramUniformMatrix4dv", out _glProgramUniformMatrix4dv );

        _glProgramUniformMatrix4dv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix4dv( GLuint program, GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX4DVPROC >( "glProgramUniformMatrix4dv", out _glProgramUniformMatrix4dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix4dv( ( uint )program, location, value.Length / 16, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix2x3fv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX2X3FVPROC >( "glProgramUniformMatrix2x3fv", out _glProgramUniformMatrix2x3fv );

        _glProgramUniformMatrix2x3fv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix2x3fv( GLuint program, GLint location, GLboolean transpose, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX2X3FVPROC >( "glProgramUniformMatrix2x3fv", out _glProgramUniformMatrix2x3fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix2x3fv( ( uint )program, location, value.Length / 6, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix3x2fv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX3X2FVPROC >( "glProgramUniformMatrix3x2fv", out _glProgramUniformMatrix3x2fv );

        _glProgramUniformMatrix3x2fv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix3x2fv( GLuint program, GLint location, GLboolean transpose, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX3X2FVPROC >( "glProgramUniformMatrix3x2fv", out _glProgramUniformMatrix3x2fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix3x2fv( ( uint )program, location, value.Length / 6, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix2x4fv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX2X4FVPROC >( "glProgramUniformMatrix2x4fv", out _glProgramUniformMatrix2x4fv );

        _glProgramUniformMatrix2x4fv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix2x4fv( GLuint program, GLint location, GLboolean transpose, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX2X4FVPROC >( "glProgramUniformMatrix2x4fv", out _glProgramUniformMatrix2x4fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix2x4fv( ( uint )program, location, value.Length / 8, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix4x2fv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX4X2FVPROC >( "glProgramUniformMatrix4x2fv", out _glProgramUniformMatrix4x2fv );

        _glProgramUniformMatrix4x2fv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix4x2fv( GLuint program, GLint location, GLboolean transpose, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX4X2FVPROC >( "glProgramUniformMatrix4x2fv", out _glProgramUniformMatrix4x2fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix4x2fv( ( uint )program, location, value.Length / 8, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix3x4fv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX3X4FVPROC >( "glProgramUniformMatrix3x4fv", out _glProgramUniformMatrix3x4fv );

        _glProgramUniformMatrix3x4fv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix3x4fv( GLuint program, GLint location, GLboolean transpose, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX3X4FVPROC >( "glProgramUniformMatrix3x4fv", out _glProgramUniformMatrix3x4fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix3x4fv( ( uint )program, location, value.Length / 12, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix4x3fv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX4X3FVPROC >( "glProgramUniformMatrix4x3fv", out _glProgramUniformMatrix4x3fv );

        _glProgramUniformMatrix4x3fv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix4x3fv( GLuint program, GLint location, GLboolean transpose, GLfloat[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX4X3FVPROC >( "glProgramUniformMatrix4x3fv", out _glProgramUniformMatrix4x3fv );

        fixed ( GLfloat* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix4x3fv( ( uint )program, location, value.Length / 12, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix2x3dv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX2X3DVPROC >( "glProgramUniformMatrix2x3dv", out _glProgramUniformMatrix2x3dv );

        _glProgramUniformMatrix2x3dv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix2x3dv( GLuint program, GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX2X3DVPROC >( "glProgramUniformMatrix2x3dv", out _glProgramUniformMatrix2x3dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix2x3dv( ( uint )program, location, value.Length / 6, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix3x2dv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX3X2DVPROC >( "glProgramUniformMatrix3x2dv", out _glProgramUniformMatrix3x2dv );

        _glProgramUniformMatrix3x2dv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix3x2dv( GLuint program, GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX3X2DVPROC >( "glProgramUniformMatrix3x2dv", out _glProgramUniformMatrix3x2dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix3x2dv( ( uint )program, location, value.Length / 6, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix2x4dv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX2X4DVPROC >( "glProgramUniformMatrix2x4dv", out _glProgramUniformMatrix2x4dv );

        _glProgramUniformMatrix2x4dv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix2x4dv( GLuint program, GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX2X4DVPROC >( "glProgramUniformMatrix2x4dv", out _glProgramUniformMatrix2x4dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix2x4dv( ( uint )program, location, value.Length / 8, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix4x2dv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX4X2DVPROC >( "glProgramUniformMatrix4x2dv", out _glProgramUniformMatrix4x2dv );

        _glProgramUniformMatrix4x2dv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix4x2dv( GLuint program, GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX4X2DVPROC >( "glProgramUniformMatrix4x2dv", out _glProgramUniformMatrix4x2dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix4x2dv( ( uint )program, location, value.Length / 8, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix3x4dv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX3X4DVPROC >( "glProgramUniformMatrix3x4dv", out _glProgramUniformMatrix3x4dv );

        _glProgramUniformMatrix3x4dv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix3x4dv( GLuint program, GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX3X4DVPROC >( "glProgramUniformMatrix3x4dv", out _glProgramUniformMatrix3x4dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix3x4dv( ( uint )program, location, value.Length / 12, transpose, pValue );
        }
    }

    // ========================================================================

    public void ProgramUniformMatrix4x3dv( GLuint program, GLint location, GLsizei count, GLboolean transpose, GLdouble* value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX4X3DVPROC >( "glProgramUniformMatrix4x3dv", out _glProgramUniformMatrix4x3dv );

        _glProgramUniformMatrix4x3dv( ( uint )program, location, count, transpose, value );
    }

    public void ProgramUniformMatrix4x3dv( GLuint program, GLint location, GLboolean transpose, GLdouble[] value )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLPROGRAMUNIFORMMATRIX4X3DVPROC >( "glProgramUniformMatrix4x3dv", out _glProgramUniformMatrix4x3dv );

        fixed ( GLdouble* pValue = &value[ 0 ] )
        {
            _glProgramUniformMatrix4x3dv( ( uint )program, location, value.Length / 12, transpose, pValue );
        }
    }

    // ========================================================================

    public void ValidateProgramPipeline( GLuint pipeline )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLVALIDATEPROGRAMPIPELINEPROC >( "glValidateProgramPipeline", out _glValidateProgramPipeline );

        _glValidateProgramPipeline( pipeline );
    }

    // ========================================================================

    public void GetProgramPipelineInfoLog( GLuint pipeline, GLsizei bufSize, GLsizei* length, GLchar* infoLog )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMPIPELINEINFOLOGPROC >( "glGetProgramPipelineInfoLog", out _glGetProgramPipelineInfoLog );

        _glGetProgramPipelineInfoLog( pipeline, bufSize, length, infoLog );
    }

    public string GetProgramPipelineInfoLog( GLuint pipeline, GLsizei bufSize )
    {
        var infoLog = new GLchar[ bufSize ];

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMPIPELINEINFOLOGPROC >( "glGetProgramPipelineInfoLog", out _glGetProgramPipelineInfoLog );

        fixed ( GLchar* pInfoLog = &infoLog[ 0 ] )
        {
            GLsizei length;

            _glGetProgramPipelineInfoLog( pipeline, bufSize, &length, pInfoLog );

            return new string( ( GLbyte* )pInfoLog, 0, length, Encoding.UTF8 );
        }
    }

    // ========================================================================

    public void GetProgramInterfaceiv( GLint program, GLenum programInterface, GLenum pname, GLint* parameters )
    {
        Logger.Checkpoint();

        if ( !IsProgram( program ) )
        {
            throw new GdxRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMINTERFACEIVPROC >( "glGetProgramInterfaceiv", out _glGetProgramInterfaceiv );

        _glGetProgramInterfaceiv( ( uint )program, programInterface, pname, parameters );
    }

    public void GetProgramInterfaceiv( GLint program, GLenum programInterface, GLenum pname, ref GLint[] parameters )
    {
        Logger.Checkpoint();

        if ( !IsProgram( program ) )
        {
            throw new GdxRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMINTERFACEIVPROC >( "glGetProgramInterfaceiv", out _glGetProgramInterfaceiv );

        fixed ( GLint* pParameters = &parameters[ 0 ] )
        {
            _glGetProgramInterfaceiv( ( uint )program, programInterface, pname, pParameters );
        }
    }

    // ========================================================================

    public GLuint GetProgramResourceIndex( GLint program, GLenum programInterface, GLchar* name )
    {
        if ( !IsProgram( program ) )
        {
            throw new GdxRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMRESOURCEINDEXPROC >( "glGetProgramResourceIndex", out _glGetProgramResourceIndex );

        return _glGetProgramResourceIndex( ( uint )program, programInterface, name );
    }

    public GLuint GetProgramResourceIndex( GLint program, GLenum programInterface, string name )
    {
        if ( !IsProgram( program ) )
        {
            throw new GdxRuntimeException( $"Invalid program ID: {program}" );
        }

        var nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMRESOURCEINDEXPROC >( "glGetProgramResourceIndex", out _glGetProgramResourceIndex );

        fixed ( GLchar* pName = &nameBytes[ 0 ] )
        {
            return _glGetProgramResourceIndex( ( uint )program, programInterface, pName );
        }
    }

    // ========================================================================

    public void GetProgramResourceName( GLint program, GLenum programInterface, GLuint index, GLsizei bufSize, GLsizei* length,
                                        GLchar* name )
    {
        if ( !IsProgram( program ) )
        {
            throw new GdxRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMRESOURCENAMEPROC >( "glGetProgramResourceName", out _glGetProgramResourceName );

        _glGetProgramResourceName( ( uint )program, programInterface, index, bufSize, length, name );
    }

    public string GetProgramResourceName( GLint program, GLenum programInterface, GLuint index, GLsizei bufSize )
    {
        if ( !IsProgram( program ) )
        {
            throw new GdxRuntimeException( $"Invalid program ID: {program}" );
        }

        var name = new GLchar[ bufSize ];

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMRESOURCENAMEPROC >( "glGetProgramResourceName", out _glGetProgramResourceName );

        fixed ( GLchar* pName = &name[ 0 ] )
        {
            GLsizei length;

            _glGetProgramResourceName( ( uint )program, programInterface, index, bufSize, &length, pName );

            return new string( ( GLbyte* )pName, 0, length, Encoding.UTF8 );
        }
    }

    // ========================================================================

    public void GetProgramResourceiv( GLint program, GLenum programInterface, GLuint index, GLsizei propCount,
                                      GLenum* props, GLsizei bufSize, GLsizei* length, GLint* parameters )
    {
        if ( !IsProgram( program ) )
        {
            throw new GdxRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMRESOURCEIVPROC >( "glGetProgramResourceiv", out _glGetProgramResourceiv );

        _glGetProgramResourceiv( ( uint )program, programInterface, index, propCount, props, bufSize, length, parameters );
    }

    public void GetProgramResourceiv( GLint program, GLenum programInterface, GLuint index, GLenum[] props, GLsizei bufSize,
                                      ref GLint[] parameters )
    {
        if ( !IsProgram( program ) )
        {
            throw new GdxRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMRESOURCEIVPROC >( "glGetProgramResourceiv", out _glGetProgramResourceiv );

        fixed ( GLenum* pProps = &props[ 0 ] )
        {
            fixed ( GLint* pParams = &parameters[ 0 ] )
            {
                GLsizei length;
                _glGetProgramResourceiv( ( uint )program, programInterface, index, props.Length, pProps, bufSize, &length, pParams );
            }
        }
    }

    // ========================================================================

    public GLint GetProgramResourceLocation( GLint program, GLenum programInterface, GLchar* name )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMRESOURCELOCATIONPROC >( "glGetProgramResourceLocation", out _glGetProgramResourceLocation );

        return _glGetProgramResourceLocation( ( uint )program, programInterface, name );
    }

    public GLint GetProgramResourceLocation( GLint program, GLenum programInterface, string name )
    {
        if ( !IsProgram( program ) )
        {
            throw new GdxRuntimeException( $"Invalid program ID: {program}" );
        }

        var nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMRESOURCELOCATIONPROC >( "glGetProgramResourceLocation", out _glGetProgramResourceLocation );

        fixed ( GLchar* pName = &nameBytes[ 0 ] )
        {
            return _glGetProgramResourceLocation( ( uint )program, programInterface, pName );
        }
    }

    // ========================================================================

    public GLint GetProgramResourceLocationIndex( GLint program, GLenum programInterface, GLchar* name )
    {
        if ( !IsProgram( program ) )
        {
            throw new GdxRuntimeException( $"Invalid program ID: {program}" );
        }

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMRESOURCELOCATIONINDEXPROC >( "glGetProgramResourceLocationIndex",
                                                                                                out _glGetProgramResourceLocationIndex );

        return _glGetProgramResourceLocationIndex( ( uint )program, programInterface, name );
    }

    public GLint GetProgramResourceLocationIndex( GLint program, GLenum programInterface, string name )
    {
        if ( !IsProgram( program ) )
        {
            throw new GdxRuntimeException( $"Invalid program ID: {program}" );
        }

        var nameBytes = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLGETPROGRAMRESOURCELOCATIONINDEXPROC >( "glGetProgramResourceLocationIndex",
                                                                                                out _glGetProgramResourceLocationIndex );

        fixed ( GLchar* pName = &nameBytes[ 0 ] )
        {
            return _glGetProgramResourceLocationIndex( ( uint )program, programInterface, pName );
        }
    }

    // ========================================================================

    public void CreateProgramPipelines( GLsizei n, GLuint* pipelines )
    {
        GetDelegateForFunction< OpenGL.GLBindings.PFNGLCREATEPROGRAMPIPELINESPROC >( "glCreateProgramPipelines", out _glCreateProgramPipelines );

        _glCreateProgramPipelines( n, pipelines );
    }

    public GLuint[] CreateProgramPipelines( GLsizei n )
    {
        var pipelines = new GLuint[ n ];

        GetDelegateForFunction< OpenGL.GLBindings.PFNGLCREATEPROGRAMPIPELINESPROC >( "glCreateProgramPipelines", out _glCreateProgramPipelines );

        fixed ( GLuint* ptrPipelines = &pipelines[ 0 ] )
        {
            _glCreateProgramPipelines( n, ptrPipelines );
        }

        return pipelines;
    }

    public GLuint CreateProgramPipeline()
    {
        return CreateProgramPipelines( 1 )[ 0 ];
    }

    // ========================================================================
}