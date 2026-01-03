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

using LughSharp.Core.Utils.Logging;
using GLenum = int;
using GLfloat = float;
using GLint = int;
using GLsizei = int;
using GLdouble = double;
using GLuint = uint;
using GLboolean = bool;
using GLubyte = byte;
using GLshort = short;
using GLbyte = sbyte;
using GLushort = ushort;
using GLchar = byte;

// ============================================================================

namespace LughSharp.Core.Graphics.OpenGL.Bindings;

public unsafe partial class GLBindings
{
    /// <inheritdoc />
    public void AttachShader( GLint program, GLint shader )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        if ( !GL.IsShader( shader ) || ( shader == INVALID_SHADER ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLATTACHSHADERPROC >( "glAttachShader", out _glAttachShader );

        _glAttachShader( ( GLuint )program, ( GLuint )shader );
    }

    // ========================================================================

    /// <inheritdoc />
    public void BindAttribLocation( GLint program, GLuint index, byte* name )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLBINDATTRIBLOCATIONPROC >( "glBindAttribLocation", out _glBindAttribLocation );

        _glBindAttribLocation( ( GLuint )program, index, name );
    }

    /// <inheritdoc />
    public void BindAttribLocation( int program, GLuint index, string name )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        var utf8 = Encoding.UTF8.GetBytes( name );

        GetDelegateForFunction< PFNGLBINDATTRIBLOCATIONPROC >( "glBindAttribLocation", out _glBindAttribLocation );

        fixed ( byte* putf8 = &utf8[ 0 ] )
        {
            _glBindAttribLocation( ( GLuint )program, index, putf8 );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void CompileShader( GLint shader )
    {
        if ( !GL.IsShader( shader ) || ( shader == INVALID_SHADER ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLCOMPILESHADERPROC >( "glCompileShader", out _glCompileShader );

        _glCompileShader( ( GLuint )shader );
    }

    // ========================================================================

    /// <inheritdoc />
    public GLuint CreateShader( GLenum type )
    {
        GetDelegateForFunction< PFNGLCREATESHADERPROC >( "glCreateShader", out _glCreateShader );

        return _glCreateShader( type );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DeleteProgram( GLint program )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        if ( GL.IsProgram( program ) && ( program != INVALID_SHADER_PROGRAM ) )
        {
            GetDelegateForFunction< PFNGLDELETEPROGRAMPROC >( "glDeleteProgram", out _glDeleteProgram );

            _glDeleteProgram( ( GLuint )program );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void DeleteShader( GLint shader )
    {
        if ( !GL.IsShader( shader ) || ( shader == INVALID_SHADER ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLDELETESHADERPROC >( "glDeleteShader", out _glDeleteShader );

        _glDeleteShader( ( GLuint )shader );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DetachShader( GLint program, GLint shader )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        if ( !GL.IsShader( shader ) || ( shader == INVALID_SHADER ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLDETACHSHADERPROC >( "glDetachShader", out _glDetachShader );

        _glDetachShader( ( GLuint )program, ( GLuint )shader );
    }

    // ========================================================================

    /// <inheritdoc />
    public void DisableVertexAttribArray( GLuint index )
    {
        GetDelegateForFunction< PFNGLDISABLEVERTEXATTRIBARRAYPROC >( "glDisableVertexAttribArray", out _glDisableVertexAttribArray );

        _glDisableVertexAttribArray( index );
    }

    // ========================================================================

    /// <inheritdoc />
    public void EnableVertexAttribArray( GLuint index )
    {
        GetDelegateForFunction< PFNGLENABLEVERTEXATTRIBARRAYPROC >( "glEnableVertexAttribArray", out _glEnableVertexAttribArray );

        _glEnableVertexAttribArray( index );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetActiveAttrib( GLint program, GLuint index, GLsizei bufSize, GLsizei* length, GLint* size, GLenum* type, GLchar* name )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETACTIVEATTRIBPROC >( "glGetActiveAttrib", out _glGetActiveAttrib );

        _glGetActiveAttrib( ( GLuint )program, index, bufSize, length, size, type, name );
    }

    /// <inheritdoc />
    public string GetActiveAttrib( GLint program, GLuint index, GLsizei bufSize, out GLint size, out GLenum type )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        var     name = stackalloc GLchar[ bufSize ];
        GLsizei len;

        GetDelegateForFunction< PFNGLGETACTIVEATTRIBPROC >( "glGetActiveAttrib", out _glGetActiveAttrib );

        fixed ( GLenum* ptype = &type )
        {
            fixed ( GLint* psize = &size )
            {
                _glGetActiveAttrib( ( GLuint )program, index, bufSize, &len, psize, ptype, name );
            }
        }

        return new string( ( GLbyte* )name, 0, len, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetActiveUniform( GLint program, GLuint index, GLsizei bufSize, GLsizei* length, GLint* size, GLenum* type, GLchar* name )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMPROC >( "glGetActiveUniform", out _glGetActiveUniform );

        _glGetActiveUniform( ( GLuint )program, index, bufSize, length, size, type, name );
    }

    /// <inheritdoc />
    public string GetActiveUniform( GLint program, GLuint index, GLsizei bufSize, out GLint size, out GLenum type )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        var     name = stackalloc GLchar[ bufSize ];
        GLsizei len;

        GetDelegateForFunction< PFNGLGETACTIVEUNIFORMPROC >( "glGetActiveUniform", out _glGetActiveUniform );

        fixed ( GLenum* ptype = &type )
        {
            fixed ( GLint* psize = &size )
            {
                _glGetActiveUniform( ( GLuint )program, index, bufSize, &len, psize, ptype, name );
            }
        }

        return new string( ( GLbyte* )name, 0, len, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetAttachedShaders( int program, int maxCount, int* count, GLuint* shaders )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETATTACHEDSHADERSPROC >( "glGetAttachedShaders", out _glGetAttachedShaders );

        _glGetAttachedShaders( ( GLuint )program, maxCount, count, shaders );
    }

    /// <inheritdoc />
    public GLuint[] GetAttachedShaders( int program, int maxCount )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        var     shaders = new GLuint[ maxCount ];
        GLsizei count;

        GetDelegateForFunction< PFNGLGETATTACHEDSHADERSPROC >( "glGetAttachedShaders", out _glGetAttachedShaders );

        fixed ( GLuint* pshaders = &shaders[ 0 ] )
        {
            _glGetAttachedShaders( ( GLuint )program, maxCount, &count, pshaders );
        }

        Array.Resize( ref shaders, count );

        return shaders;
    }

    // ========================================================================

    /// <inheritdoc />
    public GLint GetAttribLocation( GLint program, GLchar* name )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETATTRIBLOCATIONPROC >( "glGetAttribLocation", out _glGetAttribLocation );

        return _glGetAttribLocation( ( GLuint )program, name );
    }

    /// <inheritdoc />
    public GLint GetAttribLocation( GLint program, string name )
    {
        if ( !GL.IsProgram( program ) || ( program == INVALID_SHADER_PROGRAM ) )
        {
            Logger.Debug( $"***** Provided Program {program} is not a valid GLprogram *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETATTRIBLOCATIONPROC >( "glGetAttribLocation", out _glGetAttribLocation );

        fixed ( GLchar* pname = Encoding.UTF8.GetBytes( name ) )
        {
            return _glGetAttribLocation( ( GLuint )program, pname );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetShaderiv( GLint shader, GLenum pname, GLint* parameters )
    {
        if ( !GL.IsShader( shader ) || ( shader == INVALID_SHADER ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETSHADERIVPROC >( "glGetShaderiv", out _glGetShaderiv );

        _glGetShaderiv( ( GLuint )shader, pname, parameters );
    }

    /// <inheritdoc />
    public void GetShaderiv( GLint shader, GLenum pname, ref GLint[] parameters )
    {
        if ( !GL.IsShader( shader ) || ( shader == INVALID_SHADER ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETSHADERIVPROC >( "glGetShaderiv", out _glGetShaderiv );

        fixed ( GLint* pparams = &parameters[ 0 ] )
        {
            _glGetShaderiv( ( GLuint )shader, pname, pparams );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetShaderInfoLog( GLint shader, GLsizei bufSize, GLsizei* length, GLchar* infoLog )
    {
        if ( !GL.IsShader( shader ) || ( shader == INVALID_SHADER ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETSHADERINFOLOGPROC >( "glGetShaderInfoLog", out _glGetShaderInfoLog );

        _glGetShaderInfoLog( ( GLuint )shader, bufSize, length, infoLog );
    }

    /// <inheritdoc />
    public string GetShaderInfoLog( GLint shader, GLsizei bufSize )
    {
        if ( !GL.IsShader( shader ) || ( shader == INVALID_SHADER ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        var     infoLog = stackalloc GLchar[ bufSize ];
        GLsizei len;

        GetDelegateForFunction< PFNGLGETSHADERINFOLOGPROC >( "glGetShaderInfoLog", out _glGetShaderInfoLog );

        _glGetShaderInfoLog( ( GLuint )shader, bufSize, &len, infoLog );

        return new string( ( GLbyte* )infoLog, 0, len, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetShaderSource( GLint shader, GLsizei bufSize, GLsizei* length, GLchar* source )
    {
        if ( !GL.IsShader( shader ) || ( shader == INVALID_SHADER ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLGETSHADERSOURCEPROC >( "glGetShaderSource", out _glGetShaderSource );

        _glGetShaderSource( ( GLuint )shader, bufSize, length, source );
    }

    /// <inheritdoc />
    public string GetShaderSource( GLint shader, GLsizei bufSize = 4096 )
    {
        if ( !GL.IsShader( shader ) || ( shader == INVALID_SHADER ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        var     source = stackalloc GLchar[ bufSize ];
        GLsizei len;

        GetDelegateForFunction< PFNGLGETSHADERSOURCEPROC >( "glGetShaderSource", out _glGetShaderSource );

        _glGetShaderSource( ( GLuint )shader, bufSize, &len, source );

        return new string( ( GLbyte* )source, 0, len, Encoding.UTF8 );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetVertexAttribdv( GLuint index, GLenum pname, GLdouble* parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBDVPROC >( "glGetVertexAttribdv", out _glGetVertexAttribdv );

        _glGetVertexAttribdv( index, pname, parameters );
    }

    /// <inheritdoc />
    public void GetVertexAttribdv( GLuint index, GLenum pname, ref GLdouble[] parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBDVPROC >( "glGetVertexAttribdv", out _glGetVertexAttribdv );

        fixed ( GLdouble* pparams = &parameters[ 0 ] )
        {
            _glGetVertexAttribdv( index, pname, pparams );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetVertexAttribfv( GLuint index, GLenum pname, GLfloat* parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBFVPROC >( "glGetVertexAttribfv", out _glGetVertexAttribfv );

        _glGetVertexAttribfv( index, pname, parameters );
    }

    /// <inheritdoc />
    public void GetVertexAttribfv( GLuint index, GLenum pname, ref GLfloat[] parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBFVPROC >( "glGetVertexAttribfv", out _glGetVertexAttribfv );

        fixed ( GLfloat* pparams = &parameters[ 0 ] )
        {
            _glGetVertexAttribfv( index, pname, pparams );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetVertexAttribiv( GLuint index, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBIVPROC >( "glGetVertexAttribiv", out _glGetVertexAttribiv );

        _glGetVertexAttribiv( index, pname, parameters );
    }

    /// <inheritdoc />
    public void GetVertexAttribiv( GLuint index, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBIVPROC >( "glGetVertexAttribiv", out _glGetVertexAttribiv );

        fixed ( GLint* pparams = &parameters[ 0 ] )
        {
            _glGetVertexAttribiv( index, pname, pparams );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetVertexAttribPointerv( GLuint index, GLenum pname, IntPtr* pointer )
    {
        GetDelegateForFunction< PFNGLGETVERTEXATTRIBPOINTERVPROC >( "glGetVertexAttribPointerv", out _glGetVertexAttribPointerv );

        _glGetVertexAttribPointerv( index, pname, pointer );
    }

    /// <inheritdoc />
    public void GetVertexAttribPointerv( GLuint index, GLenum pname, ref GLuint[] pointer )
    {
        var ptr = new void*[ pointer.Length ];

        GetDelegateForFunction< PFNGLGETVERTEXATTRIBPOINTERVPROC >( "glGetVertexAttribPointerv", out _glGetVertexAttribPointerv );

        fixed ( void** p = &ptr[ 0 ] )
        {
            _glGetVertexAttribPointerv( index, pname, ( IntPtr* )p );
        }

        for ( var i = 0; i < pointer.Length; i++ )
        {
            pointer[ i ] = ( GLuint )ptr[ i ];
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public GLboolean IsShader( GLint shader )
    {
        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLISSHADERPROC >( "glIsShader", out _glIsShader );

        return _glIsShader( ( GLuint )shader );
    }

    // ========================================================================

    /// <inheritdoc />
    public void ShaderSource( GLint shader, GLsizei count, GLchar** str, GLint* length )
    {
        if ( !GL.IsShader( shader ) || ( shader == INVALID_SHADER ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        GetDelegateForFunction< PFNGLSHADERSOURCEPROC >( "glShaderSource", out _glShaderSource );

        _glShaderSource( ( GLuint )shader, count, str, length );
    }

    /// <inheritdoc />
    public void ShaderSource( GLint shader, params string[] stringParam )
    {
        if ( !GL.IsShader( shader ) || ( shader == INVALID_SHADER ) )
        {
            Logger.Debug( $"***** Provided Shader {shader} is not a valid GLshader *****" );
        }

        // Error checking is done internal to GetDelegateForFunction.
        var count   = stringParam.Length;
        var strings = new GLchar[ count ][];
        var lengths = new GLint[ count ];

        for ( var i = 0; i < count; i++ )
        {
            strings[ i ] = Encoding.UTF8.GetBytes( stringParam[ i ] );
            lengths[ i ] = stringParam[ i ].Length;
        }

        var pstring = stackalloc GLchar*[ count ];
        var length  = stackalloc GLint[ count ];

        for ( var i = 0; i < count; i++ )
        {
            fixed ( GLchar* p = &strings[ i ][ 0 ] )
            {
                pstring[ i ] = p;
            }

            length[ i ] = lengths[ i ];
        }

        GetDelegateForFunction< PFNGLSHADERSOURCEPROC >( "glShaderSource", out _glShaderSource );

        _glShaderSource( ( GLuint )shader, count, pstring, length );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib1d( GLuint index, GLdouble x )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1DPROC >( "glVertexAttrib1d", out _glVertexAttrib1d );

        _glVertexAttrib1d( index, x );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib1dv( GLuint index, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1DVPROC >( "glVertexAttrib1dv", out _glVertexAttrib1dv );

        _glVertexAttrib1dv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib1dv( GLuint index, params GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1DVPROC >( "glVertexAttrib1dv", out _glVertexAttrib1dv );

        fixed ( GLdouble* p = &v[ 0 ] )
        {
            _glVertexAttrib1dv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib1f( GLuint index, GLfloat x )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1FPROC >( "glVertexAttrib1f", out _glVertexAttrib1f );

        _glVertexAttrib1f( index, x );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib1fv( GLuint index, GLfloat* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1FVPROC >( "glVertexAttrib1fv", out _glVertexAttrib1fv );

        _glVertexAttrib1fv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib1fv( GLuint index, params GLfloat[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1FVPROC >( "glVertexAttrib1fv", out _glVertexAttrib1fv );

        fixed ( GLfloat* p = &v[ 0 ] )
        {
            _glVertexAttrib1fv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib1s( GLuint index, GLshort x )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1SPROC >( "glVertexAttrib1s", out _glVertexAttrib1s );

        _glVertexAttrib1s( index, x );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib1sv( GLuint index, GLshort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1SVPROC >( "glVertexAttrib1sv", out _glVertexAttrib1sv );

        _glVertexAttrib1sv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib1sv( GLuint index, params GLshort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB1SVPROC >( "glVertexAttrib1sv", out _glVertexAttrib1sv );

        fixed ( GLshort* p = &v[ 0 ] )
        {
            _glVertexAttrib1sv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib2d( GLuint index, GLdouble x, GLdouble y )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2DPROC >( "glVertexAttrib2d", out _glVertexAttrib2d );

        _glVertexAttrib2d( index, x, y );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib2dv( GLuint index, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2DVPROC >( "glVertexAttrib2dv", out _glVertexAttrib2dv );

        _glVertexAttrib2dv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib2dv( GLuint index, params GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2DVPROC >( "glVertexAttrib2dv", out _glVertexAttrib2dv );

        fixed ( GLdouble* p = &v[ 0 ] )
        {
            _glVertexAttrib2dv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib2f( GLuint index, GLfloat x, GLfloat y )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2FPROC >( "glVertexAttrib2f", out _glVertexAttrib2f );

        _glVertexAttrib2f( index, x, y );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib2fv( GLuint index, GLfloat* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2FVPROC >( "glVertexAttrib2fv", out _glVertexAttrib2fv );

        _glVertexAttrib2fv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib2fv( GLuint index, params GLfloat[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2FVPROC >( "glVertexAttrib2fv", out _glVertexAttrib2fv );

        fixed ( GLfloat* p = &v[ 0 ] )
        {
            _glVertexAttrib2fv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib2s( GLuint index, GLshort x, GLshort y )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2SPROC >( "glVertexAttrib2s", out _glVertexAttrib2s );

        _glVertexAttrib2s( index, x, y );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib2sv( GLuint index, GLshort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2SVPROC >( "glVertexAttrib2sv", out _glVertexAttrib2sv );

        _glVertexAttrib2sv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib2sv( GLuint index, params GLshort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB2SVPROC >( "glVertexAttrib2sv", out _glVertexAttrib2sv );

        fixed ( GLshort* p = &v[ 0 ] )
        {
            _glVertexAttrib2sv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib3d( GLuint index, GLdouble x, GLdouble y, GLdouble z )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3DPROC >( "glVertexAttrib3d", out _glVertexAttrib3d );

        _glVertexAttrib3d( index, x, y, z );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib3dv( GLuint index, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3DVPROC >( "glVertexAttrib3dv", out _glVertexAttrib3dv );

        _glVertexAttrib3dv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib3dv( GLuint index, params GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3DVPROC >( "glVertexAttrib3dv", out _glVertexAttrib3dv );

        fixed ( GLdouble* p = &v[ 0 ] )
        {
            _glVertexAttrib3dv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib3f( GLuint index, GLfloat x, GLfloat y, GLfloat z )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3FPROC >( "glVertexAttrib3f", out _glVertexAttrib3f );

        _glVertexAttrib3f( index, x, y, z );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib3fv( GLuint index, GLfloat* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3FVPROC >( "glVertexAttrib3fv", out _glVertexAttrib3fv );

        _glVertexAttrib3fv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib3fv( GLuint index, params GLfloat[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3FVPROC >( "glVertexAttrib3fv", out _glVertexAttrib3fv );

        fixed ( GLfloat* p = &v[ 0 ] )
        {
            _glVertexAttrib3fv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib3s( GLuint index, GLshort x, GLshort y, GLshort z )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3SPROC >( "glVertexAttrib3s", out _glVertexAttrib3s );

        _glVertexAttrib3s( index, x, y, z );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib3sv( GLuint index, GLshort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3SVPROC >( "glVertexAttrib3sv", out _glVertexAttrib3sv );

        _glVertexAttrib3sv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib3sv( GLuint index, params GLshort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB3SVPROC >( "glVertexAttrib3sv", out _glVertexAttrib3sv );

        fixed ( GLshort* p = &v[ 0 ] )
        {
            _glVertexAttrib3sv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Nbv( GLuint index, GLbyte* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NBVPROC >( "glVertexAttrib4Nbv", out _glVertexAttrib4Nbv );

        _glVertexAttrib4Nbv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Nbv( GLuint index, params GLbyte[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NBVPROC >( "glVertexAttrib4Nbv", out _glVertexAttrib4Nbv );

        fixed ( GLbyte* p = &v[ 0 ] )
        {
            _glVertexAttrib4Nbv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Niv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NIVPROC >( "glVertexAttrib4Niv", out _glVertexAttrib4Niv );

        _glVertexAttrib4Niv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Niv( GLuint index, params GLint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NIVPROC >( "glVertexAttrib4Niv", out _glVertexAttrib4Niv );

        fixed ( GLint* p = &v[ 0 ] )
        {
            _glVertexAttrib4Niv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Nsv( GLuint index, GLshort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NSVPROC >( "glVertexAttrib4Nsv", out _glVertexAttrib4Nsv );

        _glVertexAttrib4Nsv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Nsv( GLuint index, params GLshort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NSVPROC >( "glVertexAttrib4Nsv", out _glVertexAttrib4Nsv );

        fixed ( GLshort* p = &v[ 0 ] )
        {
            _glVertexAttrib4Nsv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Nub( GLuint index, GLubyte x, GLubyte y, GLubyte z, GLubyte w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUBPROC >( "glVertexAttrib4Nub", out _glVertexAttrib4Nub );

        _glVertexAttrib4Nub( index, x, y, z, w );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Nubv( GLuint index, GLubyte* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUBVPROC >( "glVertexAttrib4Nubv", out _glVertexAttrib4Nubv );

        _glVertexAttrib4Nubv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Nubv( GLuint index, params GLubyte[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUBVPROC >( "glVertexAttrib4Nubv", out _glVertexAttrib4Nubv );

        fixed ( GLubyte* p = &v[ 0 ] )
        {
            _glVertexAttrib4Nubv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Nuiv( GLuint index, GLuint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUIVPROC >( "glVertexAttrib4Nuiv", out _glVertexAttrib4Nuiv );

        _glVertexAttrib4Nuiv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Nuiv( GLuint index, params GLuint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUIVPROC >( "glVertexAttrib4Nuiv", out _glVertexAttrib4Nuiv );

        fixed ( GLuint* p = &v[ 0 ] )
        {
            _glVertexAttrib4Nuiv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4Nusv( GLuint index, GLushort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUSVPROC >( "glVertexAttrib4Nusv", out _glVertexAttrib4Nusv );

        _glVertexAttrib4Nusv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4Nusv( GLuint index, params GLushort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4NUSVPROC >( "glVertexAttrib4Nusv", out _glVertexAttrib4Nusv );

        fixed ( GLushort* p = &v[ 0 ] )
        {
            _glVertexAttrib4Nusv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4bv( GLuint index, GLbyte* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4BVPROC >( "glVertexAttrib4bv", out _glVertexAttrib4bv );

        _glVertexAttrib4bv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4bv( GLuint index, params GLbyte[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4BVPROC >( "glVertexAttrib4bv", out _glVertexAttrib4bv );

        fixed ( GLbyte* p = &v[ 0 ] )
        {
            _glVertexAttrib4bv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4d( GLuint index, GLdouble x, GLdouble y, GLdouble z, GLdouble w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4DPROC >( "glVertexAttrib4d", out _glVertexAttrib4d );

        _glVertexAttrib4d( index, x, y, z, w );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4dv( GLuint index, GLdouble* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4DVPROC >( "glVertexAttrib4dv", out _glVertexAttrib4dv );

        _glVertexAttrib4dv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4dv( GLuint index, params GLdouble[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4DVPROC >( "glVertexAttrib4dv", out _glVertexAttrib4dv );

        fixed ( GLdouble* p = &v[ 0 ] )
        {
            _glVertexAttrib4dv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4f( GLuint index, GLfloat x, GLfloat y, GLfloat z, GLfloat w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4FPROC >( "glVertexAttrib4f", out _glVertexAttrib4f );

        _glVertexAttrib4f( index, x, y, z, w );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4fv( GLuint index, GLfloat* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4FVPROC >( "glVertexAttrib4fv", out _glVertexAttrib4fv );

        _glVertexAttrib4fv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4fv( GLuint index, params GLfloat[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4FVPROC >( "glVertexAttrib4fv", out _glVertexAttrib4fv );

        fixed ( GLfloat* p = &v[ 0 ] )
        {
            _glVertexAttrib4fv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4iv( GLuint index, GLint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4IVPROC >( "glVertexAttrib4iv", out _glVertexAttrib4iv );

        _glVertexAttrib4iv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4iv( GLuint index, params GLint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4IVPROC >( "glVertexAttrib4iv", out _glVertexAttrib4iv );

        fixed ( GLint* p = &v[ 0 ] )
        {
            _glVertexAttrib4iv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4s( GLuint index, GLshort x, GLshort y, GLshort z, GLshort w )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4SPROC >( "glVertexAttrib4s", out _glVertexAttrib4s );

        _glVertexAttrib4s( index, x, y, z, w );
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4sv( GLuint index, GLshort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4SVPROC >( "glVertexAttrib4sv", out _glVertexAttrib4sv );

        _glVertexAttrib4sv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4sv( GLuint index, params GLshort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4SVPROC >( "glVertexAttrib4sv", out _glVertexAttrib4sv );

        fixed ( GLshort* p = &v[ 0 ] )
        {
            _glVertexAttrib4sv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4ubv( GLuint index, GLubyte* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4UBVPROC >( "glVertexAttrib4ubv", out _glVertexAttrib4ubv );

        _glVertexAttrib4ubv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4ubv( GLuint index, params GLubyte[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4UBVPROC >( "glVertexAttrib4ubv", out _glVertexAttrib4ubv );

        fixed ( GLubyte* p = &v[ 0 ] )
        {
            _glVertexAttrib4ubv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4uiv( GLuint index, GLuint* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4UIVPROC >( "glVertexAttrib4uiv", out _glVertexAttrib4uiv );

        _glVertexAttrib4uiv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4uiv( GLuint index, params GLuint[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4UIVPROC >( "glVertexAttrib4uiv", out _glVertexAttrib4uiv );

        fixed ( GLuint* p = &v[ 0 ] )
        {
            _glVertexAttrib4uiv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttrib4usv( GLuint index, GLushort* v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4USVPROC >( "glVertexAttrib4usv", out _glVertexAttrib4usv );

        _glVertexAttrib4usv( index, v );
    }

    /// <inheritdoc />
    public void VertexAttrib4usv( GLuint index, params GLushort[] v )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIB4USVPROC >( "glVertexAttrib4usv", out _glVertexAttrib4usv );

        fixed ( GLushort* p = &v[ 0 ] )
        {
            _glVertexAttrib4usv( index, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void VertexAttribPointer( GLuint index, GLint size, GLenum type, GLboolean normalized, GLsizei stride, GLuint pointer )
    {
        VertexAttribPointer( index, size, type, normalized, stride, ( IntPtr )pointer );
    }

    /// <inheritdoc />
    public void VertexAttribPointer( GLuint index, GLint size, GLenum type, GLboolean normalized, GLsizei stride, IntPtr pointer )
    {
        GetDelegateForFunction< PFNGLVERTEXATTRIBPOINTERPROC >( "glVertexAttribPointer", out _glVertexAttribPointer );

        _glVertexAttribPointer( index, size, type, normalized, stride, pointer );
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix2x3fv( GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X3FVPROC >( "glUniformMatrix2x3fv", out _glUniformMatrix2x3fv );

        _glUniformMatrix2x3fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix2x3fv( GLint location, GLboolean transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X3FVPROC >( "glUniformMatrix2x3fv", out _glUniformMatrix2x3fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix2x3fv( location, value.Length / 6, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix3x2fv( GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X2FVPROC >( "glUniformMatrix3x2fv", out _glUniformMatrix3x2fv );

        _glUniformMatrix3x2fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix3x2fv( GLint location, GLboolean transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X2FVPROC >( "glUniformMatrix3x2fv", out _glUniformMatrix3x2fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix3x2fv( location, value.Length / 6, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix2x4fv( GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X4FVPROC >( "_glUniformMatrix2x4fv", out _glUniformMatrix2x4fv );

        _glUniformMatrix2x4fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix2x4fv( GLint location, GLboolean transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX2X4FVPROC >( "_glUniformMatrix2x4fv", out _glUniformMatrix2x4fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix2x4fv( location, value.Length / 8, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix4x2fv( GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X2FVPROC >( "glUniformMatrix4x2fv", out _glUniformMatrix4x2fv );

        _glUniformMatrix4x2fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix4x2fv( GLint location, GLboolean transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X2FVPROC >( "glUniformMatrix4x2fv", out _glUniformMatrix4x2fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix4x2fv( location, value.Length / 8, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix3x4fv( GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X4FVPROC >( "glUniformMatrix3x4fv", out _glUniformMatrix3x4fv );

        _glUniformMatrix3x4fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix3x4fv( GLint location, GLboolean transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX3X4FVPROC >( "glUniformMatrix3x4fv", out _glUniformMatrix3x4fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix3x4fv( location, value.Length / 12, transpose, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void UniformMatrix4x3fv( GLint location, GLsizei count, GLboolean transpose, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X3FVPROC >( "glUniformMatrix4x3fv", out _glUniformMatrix4x3fv );

        _glUniformMatrix4x3fv( location, count, transpose, value );
    }

    /// <inheritdoc />
    public void UniformMatrix4x3fv( GLint location, GLboolean transpose, params GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLUNIFORMMATRIX4X3FVPROC >( "glUniformMatrix4x3fv", out _glUniformMatrix4x3fv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glUniformMatrix4x3fv( location, value.Length / 12, transpose, p );
        }
    }
}

// ============================================================================
// ============================================================================
