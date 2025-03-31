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
    public void DebugMessageControl( GLenum source, GLenum type, GLenum severity, GLsizei count, GLuint* ids, GLboolean enabled )
    {
        GetDelegateForFunction< PFNGLDEBUGMESSAGECONTROLPROC >( "glDebugMessageControl", out _glDebugMessageControl );

        _glDebugMessageControl( source, type, severity, count, ids, enabled );
    }

    public void DebugMessageControl( GLenum source, GLenum type, GLenum severity, GLuint[] ids, GLboolean enabled )
    {
        GetDelegateForFunction< PFNGLDEBUGMESSAGECONTROLPROC >( "glDebugMessageControl", out _glDebugMessageControl );

        fixed ( GLuint* pIds = ids )
        {
            _glDebugMessageControl( source, type, severity, ids.Length, pIds, enabled );
        }
    }

    // ========================================================================

    public void DebugMessageInsert( GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, GLchar* buf )
    {
        GetDelegateForFunction< PFNGLDEBUGMESSAGEINSERTPROC >( "glDebugMessageInsert", out _glDebugMessageInsert );

        _glDebugMessageInsert( source, type, id, severity, length, buf );
    }

    public void DebugMessageInsert( GLenum source, GLenum type, GLuint id, GLenum severity, string buf )
    {
        var bufBytes = Encoding.UTF8.GetBytes( buf );

        GetDelegateForFunction< PFNGLDEBUGMESSAGEINSERTPROC >( "glDebugMessageInsert", out _glDebugMessageInsert );

        fixed ( GLchar* pBufBytes = bufBytes )
        {
            _glDebugMessageInsert( source, type, id, severity, bufBytes.Length, pBufBytes );
        }
    }

    // ========================================================================

    public void DebugMessageCallback( GLDEBUGPROC callback, void* userParam )
    {
        GetDelegateForFunction< PFNGLDEBUGMESSAGECALLBACKPROC >( "glDebugMessageCallback", out _glDebugMessageCallback );

        _glDebugMessageCallback( callback, ( IntPtr )userParam );
    }

    public void DebugMessageCallback( GLDEBUGPROCSAFE callback, void* userParam )
    {
        GetDelegateForFunction< PFNGLDEBUGMESSAGECALLBACKPROC >( "glDebugMessageCallback", out _glDebugMessageCallback );

        _glDebugMessageCallback( CallbackUnsafe, ( IntPtr )userParam );

        return;

        void CallbackUnsafe( GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, GLchar* message, IntPtr param )
        {
            var messageString = new string( ( GLbyte* )message, 0, length, Encoding.UTF8 );

            callback( source, type, id, severity, messageString, param );
        }
    }

    // ========================================================================

    public GLuint GetDebugMessageLog( GLuint count,
                                      GLsizei bufsize,
                                      GLenum* sources,
                                      GLenum* types,
                                      GLuint* ids,
                                      GLenum* severities,
                                      GLsizei* lengths,
                                      GLchar* messageLog )
    {
        GetDelegateForFunction< PFNGLGETDEBUGMESSAGELOGPROC >( "glGetDebugMessageLog", out _glGetDebugMessageLog );

        return _glGetDebugMessageLog( count, bufsize, sources, types, ids, severities, lengths, messageLog );
    }

    public GLuint GetDebugMessageLog( GLuint count,
                                      GLsizei bufSize,
                                      out GLenum[] sources,
                                      out GLenum[] types,
                                      out GLuint[] ids,
                                      out GLenum[] severities,
                                      out string[] messageLog )
    {
        sources    = new GLenum[ count ];
        types      = new GLenum[ count ];
        ids        = new GLuint[ count ];
        severities = new GLenum[ count ];

        var messageLogBytes = new GLchar[ bufSize ];
        var lengths         = new GLsizei[ count ];

        GetDelegateForFunction< PFNGLGETDEBUGMESSAGELOGPROC >( "glGetDebugMessageLog", out _glGetDebugMessageLog );

        fixed ( GLenum* pSources = &sources[ 0 ] )
        fixed ( GLenum* pTypes = &types[ 0 ] )
        fixed ( GLuint* pIds = &ids[ 0 ] )
        fixed ( GLenum* pSeverities = &severities[ 0 ] )
        fixed ( GLsizei* pLengths = &lengths[ 0 ] )
        fixed ( GLchar* pMessageLogBytes = &messageLogBytes[ 0 ] )
        {
            var pstart = pMessageLogBytes;
            var ret    = _glGetDebugMessageLog( count, bufSize, pSources, pTypes, pIds, pSeverities, pLengths, pMessageLogBytes );

            messageLog = new string[ count ];

            for ( var i = 0; i < count; i++ )
            {
                messageLog[ i ] =  new string( ( GLbyte* )pstart, 0, lengths[ i ], Encoding.UTF8 );
                pstart          += lengths[ i ];
            }

            Array.Resize( ref sources, ( int )ret );
            Array.Resize( ref types, ( int )ret );
            Array.Resize( ref ids, ( int )ret );
            Array.Resize( ref severities, ( int )ret );
            Array.Resize( ref messageLog, ( int )ret );

            return ret;
        }
    }

    // ========================================================================

    public void PushDebugGroup( GLenum source, GLuint id, GLsizei length, GLchar* message )
    {
        GetDelegateForFunction< PFNGLPUSHDEBUGGROUPPROC >( "glPushDebugGroup", out _glPushDebugGroup );

        _glPushDebugGroup( source, id, length, message );
    }

    public void PushDebugGroup( GLenum source, GLuint id, string message )
    {
        var messageBytes = Encoding.UTF8.GetBytes( message );

        GetDelegateForFunction< PFNGLPUSHDEBUGGROUPPROC >( "glPushDebugGroup", out _glPushDebugGroup );

        fixed ( GLchar* pMessageBytes = messageBytes )
        {
            _glPushDebugGroup( source, id, messageBytes.Length, pMessageBytes );
        }
    }

    // ========================================================================

    public void PopDebugGroup()
    {
        GetDelegateForFunction< PFNGLPOPDEBUGGROUPPROC >( "glPopDebugGroup", out _glPopDebugGroup );

        _glPopDebugGroup();
    }

    // ========================================================================
}