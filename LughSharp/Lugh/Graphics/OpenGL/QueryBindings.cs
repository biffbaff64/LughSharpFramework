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

namespace LughSharp.Lugh.Graphics.OpenGL;

public unsafe partial class GLBindings
{
    /// <inheritdoc />
    public void GenQueries( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLGENQUERIESPROC >( "glGenQueries", out _glGenQueries );

        _glGenQueries( n, ids );
    }

    /// <inheritdoc />
    public GLuint[] GenQueries( GLsizei n )
    {
        GetDelegateForFunction< PFNGLGENQUERIESPROC >( "glGenQueries", out _glGenQueries );

        var ret = new GLuint[ n ];

        {
            fixed ( GLuint* p = &ret[ 0 ] )
            {
                _glGenQueries( n, p );
            }
        }

        return ret;
    }

    /// <inheritdoc />
    public GLuint GenQuery()
    {
        return GenQueries( 1 )[ 0 ];
    }

    // ========================================================================

    /// <inheritdoc />
    public void DeleteQueries( GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLDELETEQUERIESPROC >( "glDeleteQueries", out _glDeleteQueries );

        _glDeleteQueries( n, ids );
    }

    /// <inheritdoc />
    public void DeleteQueries( params GLuint[] ids )
    {
        GetDelegateForFunction< PFNGLDELETEQUERIESPROC >( "glDeleteQueries", out _glDeleteQueries );

        {
            fixed ( GLuint* p = &ids[ 0 ] )
            {
                _glDeleteQueries( ids.Length, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public GLboolean IsQuery( GLuint id )
    {
        GetDelegateForFunction< PFNGLISQUERYPROC >( "glIsQuery", out _glIsQuery );

        return _glIsQuery( id );
    }

    // ========================================================================

    /// <inheritdoc />
    public void BeginQuery( GLenum target, GLuint id )
    {
        GetDelegateForFunction< PFNGLBEGINQUERYPROC >( "glBeginQuery", out _glBeginQuery );

        _glBeginQuery( target, id );
    }

    // ========================================================================

    /// <inheritdoc />
    public void EndQuery( GLenum target )
    {
        GetDelegateForFunction< PFNGLENDQUERYPROC >( "glEndQuery", out _glEndQuery );

        _glEndQuery( target );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetQueryiv( GLenum target, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYIVPROC >( "glGetQueryiv", out _glGetQueryiv );

        _glGetQueryiv( target, pname, parameters );
    }

    /// <inheritdoc />
    public void GetQueryiv( GLenum target, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYIVPROC >( "glGetQueryiv", out _glGetQueryiv );

        {
            fixed ( GLint* p = &parameters[ 0 ] )
            {
                _glGetQueryiv( target, pname, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetQueryObjectiv( GLuint id, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTIVPROC >( "glGetQueryObjectiv", out _glGetQueryObjectiv );

        _glGetQueryObjectiv( id, pname, parameters );
    }

    /// <inheritdoc />
    public void GetQueryObjectiv( GLuint id, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTIVPROC >( "glGetQueryObjectiv", out _glGetQueryObjectiv );

        {
            fixed ( GLint* p = &parameters[ 0 ] )
            {
                _glGetQueryObjectiv( id, pname, p );
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetQueryObjectuiv( GLuint id, GLenum pname, GLuint* parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTUIVPROC >( "glGetQueryObjectuiv", out _glGetQueryObjectuiv );

        _glGetQueryObjectuiv( id, pname, parameters );
    }

    /// <inheritdoc />
    public void GetQueryObjectuiv( GLuint id, GLenum pname, ref GLuint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTUIVPROC >( "glGetQueryObjectuiv", out _glGetQueryObjectuiv );

        {
            fixed ( GLuint* p = &parameters[ 0 ] )
            {
                _glGetQueryObjectuiv( id, pname, p );
            }
        }
    }

    // ========================================================================
    
    public void CreateQueries( GLenum target, GLsizei n, GLuint* ids )
    {
        GetDelegateForFunction< PFNGLCREATEQUERIESPROC >( "glCreateQueries", out _glCreateQueries );

        _glCreateQueries( target, n, ids );
    }

    public GLuint[] CreateQueries( GLenum target, GLsizei n )
    {
        var ids = new GLuint[ n ];

        GetDelegateForFunction< PFNGLCREATEQUERIESPROC >( "glCreateQueries", out _glCreateQueries );

        fixed ( GLuint* ptrIds = &ids[ 0 ] )
        {
            _glCreateQueries( target, n, ptrIds );
        }

        return ids;
    }

    // ========================================================================

    public GLuint CreateQuery( GLenum target )
    {
        return CreateQueries( target, 1 )[ 0 ];
    }

    // ========================================================================

    public void GetQueryBufferObjecti64v( GLuint id, GLuint buffer, GLenum pname, GLintptr offset )
    {
        GetDelegateForFunction< PFNGLGETQUERYBUFFEROBJECTI64VPROC >( "glGetQueryBufferObjecti64v", out _glGetQueryBufferObjecti64v );

        _glGetQueryBufferObjecti64v( id, buffer, pname, offset );
    }

    // ========================================================================

    public void GetQueryBufferObjectiv( GLuint id, GLuint buffer, GLenum pname, GLintptr offset )
    {
        GetDelegateForFunction< PFNGLGETQUERYBUFFEROBJECTIVPROC >( "glGetQueryBufferObjectiv", out _glGetQueryBufferObjectiv );

        _glGetQueryBufferObjectiv( id, buffer, pname, offset );
    }

    // ========================================================================

    public void GetQueryBufferObjectui64v( GLuint id, GLuint buffer, GLenum pname, GLintptr offset )
    {
        GetDelegateForFunction< PFNGLGETQUERYBUFFEROBJECTUI64VPROC >( "glGetQueryBufferObjectui64v", out _glGetQueryBufferObjectui64v );

        _glGetQueryBufferObjectui64v( id, buffer, pname, offset );
    }

    // ========================================================================

    public void GetQueryBufferObjectuiv( GLuint id, GLuint buffer, GLenum pname, GLintptr offset )
    {
        GetDelegateForFunction< PFNGLGETQUERYBUFFEROBJECTUIVPROC >( "glGetQueryBufferObjectuiv", out _glGetQueryBufferObjectuiv );

        _glGetQueryBufferObjectuiv( id, buffer, pname, offset );
    }

    // ========================================================================
    
    /// <inheritdoc />
    /// <inheritdoc />
    public void QueryCounter( GLuint id, GLenum target )
    {
        GetDelegateForFunction< PFNGLQUERYCOUNTERPROC >( "glQueryCounter", out _glQueryCounter );

        _glQueryCounter( id, target );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetQueryObjecti64v( GLuint id, GLenum pname, GLint64* param )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTI64VPROC >( "glGetQueryObjecti64v", out _glGetQueryObjecti64v );

        _glGetQueryObjecti64v( id, pname, param );
    }

    /// <inheritdoc />
    public void GetQueryObjecti64v( GLuint id, GLenum pname, ref GLint64[] param )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTI64VPROC >( "glGetQueryObjecti64v", out _glGetQueryObjecti64v );

        fixed ( GLint64* dp = &param[ 0 ] )
        {
            _glGetQueryObjecti64v( id, pname, dp );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetQueryObjectui64v( GLuint id, GLenum pname, GLuint64* param )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTUI64VPROC >( "glGetQueryObjectui64v", out _glGetQueryObjectui64v );

        _glGetQueryObjectui64v( id, pname, param );
    }

    /// <inheritdoc />
    public void GetQueryObjectui64v( GLuint id, GLenum pname, ref GLuint64[] param )
    {
        GetDelegateForFunction< PFNGLGETQUERYOBJECTUI64VPROC >( "glGetQueryObjectui64v", out _glGetQueryObjectui64v );

        fixed ( GLuint64* dp = &param[ 0 ] )
        {
            _glGetQueryObjectui64v( id, pname, dp );
        }
    }

    // ========================================================================

}