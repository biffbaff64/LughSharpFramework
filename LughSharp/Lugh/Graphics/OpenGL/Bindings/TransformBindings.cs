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
using GLenum = int;
using GLint = int;
using GLsizei = int;
using GLuint = uint;
using GLboolean = bool;
using GLsizeiptr = int;
using GLintptr = int;
using GLbyte = sbyte;
using GLchar = byte;
using GLint64 = long;

// ============================================================================

namespace LughSharp.Lugh.Graphics.OpenGL.Bindings;

public unsafe partial class GLBindings
{
    /// <inheritdoc />
    public void BeginTransformFeedback( GLenum primitiveMode )
    {
        GetDelegateForFunction< PFNGLBEGINTRANSFORMFEEDBACKPROC >( "glBeginTransformFeedback", out _glBeginTransformFeedback );

        _glBeginTransformFeedback( primitiveMode );
    }

    // ========================================================================

    /// <inheritdoc />
    public void EndTransformFeedback()
    {
        GetDelegateForFunction< PFNGLENDTRANSFORMFEEDBACKPROC >( "glEndTransformFeedback", out _glEndTransformFeedback );

        _glEndTransformFeedback();
    }

    // ========================================================================

    public void BindTransformFeedback( GLenum target, GLuint id )
    {
        GetDelegateForFunction< PFNGLBINDTRANSFORMFEEDBACKPROC >( "glBindTransformFeedback", out _glBindTransformFeedback );

        _glBindTransformFeedback( target, id );
    }

    // ========================================================================

    public void DeleteTransformFeedbacks( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLDELETETRANSFORMFEEDBACKSPROC >( "glDeleteTransformFeedbacks", out _glDeleteTransformFeedbacks );

        _glDeleteTransformFeedbacks( n, ids );
    }

    public void DeleteTransformFeedbacks( params GLuint[] ids )
    {
        GetDelegateForFunction< PFNGLDELETETRANSFORMFEEDBACKSPROC >( "glDeleteTransformFeedbacks", out _glDeleteTransformFeedbacks );

        fixed ( GLuint* p = &ids[ 0 ] )
        {
            _glDeleteTransformFeedbacks( ids.Length, p );
        }
    }

    // ========================================================================

    public void GenTransformFeedbacks( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLGENTRANSFORMFEEDBACKSPROC >( "glGenTransformFeedbacks", out _glGenTransformFeedbacks );

        _glGenTransformFeedbacks( n, ids );
    }

    public GLuint[] GenTransformFeedbacks( GLsizei n )
    {
        var r = new GLuint[ n ];

        GetDelegateForFunction< PFNGLGENTRANSFORMFEEDBACKSPROC >( "glGenTransformFeedbacks", out _glGenTransformFeedbacks );

        fixed ( GLuint* p = &r[ 0 ] )
        {
            _glGenTransformFeedbacks( n, p );
        }

        return r;
    }

    public GLuint GenTransformFeedback()
    {
        return GenTransformFeedbacks( 1 )[ 0 ];
    }

    // ========================================================================

    public GLboolean IsTransformFeedback( GLuint id )
    {
        GetDelegateForFunction< PFNGLISTRANSFORMFEEDBACKPROC >( "glIsTransformFeedback", out _glIsTransformFeedback );

        return _glIsTransformFeedback( id );
    }

    // ========================================================================

    public void PauseTransformFeedback()
    {
        GetDelegateForFunction< PFNGLPAUSETRANSFORMFEEDBACKPROC >( "glPauseTransformFeedback", out _glPauseTransformFeedback );

        _glPauseTransformFeedback();
    }

    // ========================================================================

    public void ResumeTransformFeedback()
    {
        GetDelegateForFunction< PFNGLRESUMETRANSFORMFEEDBACKPROC >( "glResumeTransformFeedback", out _glResumeTransformFeedback );

        _glResumeTransformFeedback();
    }

    // ========================================================================

    public void DrawTransformFeedback( GLenum mode, GLuint id )
    {
        GetDelegateForFunction< PFNGLDRAWTRANSFORMFEEDBACKPROC >( "glDrawTransformFeedback", out _glDrawTransformFeedback );

        _glDrawTransformFeedback( mode, id );
    }

    // ========================================================================

    public void DrawTransformFeedbackStream( GLenum mode, GLuint id, GLuint stream )
    {
        GetDelegateForFunction< PFNGLDRAWTRANSFORMFEEDBACKSTREAMPROC >( "glDrawTransformFeedbackStream",
                                                                                                     out _glDrawTransformFeedbackStream );

        _glDrawTransformFeedbackStream( mode, id, stream );
    }

    // ========================================================================

    public void DrawTransformFeedbackInstanced( GLenum mode, GLuint id, GLsizei instancecount )
    {
        GetDelegateForFunction< PFNGLDRAWTRANSFORMFEEDBACKINSTANCEDPROC >( "glDrawTransformFeedbackInstanced",
                                                                                                        out _glDrawTransformFeedbackInstanced );

        _glDrawTransformFeedbackInstanced( mode, id, instancecount );
    }

    // ========================================================================

    public void DrawTransformFeedbackStreamInstanced( GLenum mode, GLuint id, GLuint stream, GLsizei instancecount )
    {
        GetDelegateForFunction< PFNGLDRAWTRANSFORMFEEDBACKSTREAMINSTANCEDPROC >( "glDrawTransformFeedbackStreamInstanced",
                                                                                                              out _glDrawTransformFeedbackStreamInstanced );

        _glDrawTransformFeedbackStreamInstanced( mode, id, stream, instancecount );
    }

    // ========================================================================

    public void CreateTransformFeedbacks( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLCREATETRANSFORMFEEDBACKSPROC >( "glCreateTransformFeedbacks", out _glCreateTransformFeedbacks );

        _glCreateTransformFeedbacks( n, ids );
    }

    public GLuint[] CreateTransformFeedbacks( GLsizei n )
    {
        var ids = new GLuint[ n ];

        GetDelegateForFunction< PFNGLCREATETRANSFORMFEEDBACKSPROC >( "glCreateTransformFeedbacks", out _glCreateTransformFeedbacks );

        fixed ( GLuint* pIds = &ids[ 0 ] )
        {
            _glCreateTransformFeedbacks( n, pIds );
        }

        return ids;
    }

    public GLuint CreateTransformFeedbacks()
    {
        return CreateTransformFeedbacks( 1 )[ 0 ];
    }

    // ========================================================================

    public void TransformFeedbackBufferBase( GLuint xfb, GLuint index, GLuint buffer )
    {
        GetDelegateForFunction< PFNGLTRANSFORMFEEDBACKBUFFERBASEPROC >( "glTransformFeedbackBufferBase",
                                                                                                     out _glTransformFeedbackBufferBase );

        _glTransformFeedbackBufferBase( xfb, index, buffer );
    }

    // ========================================================================

    public void TransformFeedbackBufferRange( GLuint xfb, GLuint index, GLuint buffer, GLintptr offset, GLsizeiptr size )
    {
        GetDelegateForFunction< PFNGLTRANSFORMFEEDBACKBUFFERRANGEPROC >( "glTransformFeedbackBufferRange",
                                                                                                      out _glTransformFeedbackBufferRange );

        _glTransformFeedbackBufferRange( xfb, index, buffer, offset, size );
    }

    // ========================================================================

    public void GetTransformFeedbackiv( GLuint xfb, GLenum pname, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKIVPROC >( "glGetTransformFeedbackiv", out _glGetTransformFeedbackiv );

        _glGetTransformFeedbackiv( xfb, pname, param );
    }

    public void GetTransformFeedbackiv( GLuint xfb, GLenum pname, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKIVPROC >( "glGetTransformFeedbackiv", out _glGetTransformFeedbackiv );

        fixed ( GLint* pParam = &param[ 0 ] )
        {
            _glGetTransformFeedbackiv( xfb, pname, pParam );
        }
    }

    // ========================================================================

    public void GetTransformFeedbacki_v( GLuint xfb, GLenum pname, GLuint index, GLint* param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKI_VPROC >( "glGetTransformFeedbacki_v", out _glGetTransformFeedbacki_v );

        _glGetTransformFeedbacki_v( xfb, pname, index, param );
    }

    public void GetTransformFeedbacki_v( GLuint xfb, GLenum pname, GLuint index, ref GLint[] param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKI_VPROC >( "glGetTransformFeedbacki_v", out _glGetTransformFeedbacki_v );

        fixed ( GLint* pParam = &param[ 0 ] )
        {
            _glGetTransformFeedbacki_v( xfb, pname, index, pParam );
        }
    }

    // ========================================================================

    public void GetTransformFeedbacki64_v( GLuint xfb, GLenum pname, GLuint index, GLint64* param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKI64_VPROC >( "glGetTransformFeedbacki64_v", out _glGetTransformFeedbacki64_v );

        _glGetTransformFeedbacki64_v( xfb, pname, index, param );
    }

    public void GetTransformFeedbacki64_v( GLuint xfb, GLenum pname, GLuint index, ref GLint64[] param )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKI64_VPROC >( "glGetTransformFeedbacki64_v", out _glGetTransformFeedbacki64_v );

        fixed ( GLint64* pParam = &param[ 0 ] )
        {
            _glGetTransformFeedbacki64_v( xfb, pname, index, pParam );
        }
    }

    // ========================================================================

    public void TransformFeedbackVaryings( GLuint program, GLsizei count, GLchar** varyings, GLenum bufferMode )
    {
        GetDelegateForFunction< PFNGLTRANSFORMFEEDBACKVARYINGSPROC >( "glTransformFeedbackVaryings", out _glTransformFeedbackVaryings );

        _glTransformFeedbackVaryings( ( uint )program, count, varyings, bufferMode );
    }

    // ========================================================================

    public void TransformFeedbackVaryings( GLuint program, string[] varyings, GLenum bufferMode )
    {
        var varyingsBytes = new GLchar[ varyings.Length ][];

        for ( var i = 0; i < varyings.Length; i++ )
        {
            varyingsBytes[ i ] = Encoding.UTF8.GetBytes( varyings[ i ] );
        }

        var varyingsPtrs = new GLchar*[ varyings.Length ];

        for ( var i = 0; i < varyings.Length; i++ )
        {
            fixed ( GLchar* p = &varyingsBytes[ i ][ 0 ] )
            {
                varyingsPtrs[ i ] = p;
            }
        }

        GetDelegateForFunction< PFNGLTRANSFORMFEEDBACKVARYINGSPROC >( "glTransformFeedbackVaryings",
                                                                                                   out _glTransformFeedbackVaryings );

        fixed ( GLchar** p = &varyingsPtrs[ 0 ] )
        {
            _glTransformFeedbackVaryings( ( uint )program, varyings.Length, p, bufferMode );
        }
    }

    // ========================================================================

    public void GetTransformFeedbackVarying( GLuint program,
                                             GLuint index,
                                             GLsizei bufSize,
                                             GLsizei* length,
                                             GLsizei* size,
                                             GLenum* type,
                                             GLchar* name )
    {
        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKVARYINGPROC >( "glGetTransformFeedbackVarying",
                                                                                                     out _glGetTransformFeedbackVarying );

        _glGetTransformFeedbackVarying( ( uint )program, index, bufSize, length, size, type, name );
    }

    // ========================================================================

    public string GetTransformFeedbackVarying( GLuint program, GLuint index, GLsizei bufSize, out GLsizei size, out GLenum type )
    {
        var name = new GLchar[ bufSize ];

        GetDelegateForFunction< PFNGLGETTRANSFORMFEEDBACKVARYINGPROC >( "glGetTransformFeedbackVarying",
                                                                                                     out _glGetTransformFeedbackVarying );

        fixed ( GLsizei* pSize = &size )
        {
            fixed ( GLenum* pType = &type )
            {
                fixed ( GLchar* p = &name[ 0 ] )
                {
                    GLsizei length;

                    _glGetTransformFeedbackVarying( ( uint )program, index, bufSize, &length, pSize, pType, p );

                    return new string( ( GLbyte* )p, 0, length, Encoding.UTF8 );
                }
            }
        }
    }
}

// ========================================================================
// ========================================================================