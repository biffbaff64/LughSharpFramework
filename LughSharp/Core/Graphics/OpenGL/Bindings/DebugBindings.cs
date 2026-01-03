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

using LughSharp.Core.Utils.Logging;
using GLenum = int;
using GLsizei = int;
using GLuint = uint;
using GLboolean = bool;
using GLbyte = sbyte;
using GLchar = byte;

// ============================================================================

namespace LughSharp.Core.Graphics.OpenGL.Bindings;

public unsafe partial class GLBindings
{
    public void DebugMessageControl( GLenum source, GLenum type, GLenum severity, GLsizei count, GLuint* ids, GLboolean enabled )
    {
        GetDelegateForFunction< PFNGLDEBUGMESSAGECONTROLPROC >( "glDebugMessageControl", out _glDebugMessageControl );

        _glDebugMessageControl( source, type, severity, count, ids, enabled );
    }

    // ========================================================================

    public void DebugMessageInsert( GLenum source, GLenum type, GLuint id, GLenum severity, GLsizei length, GLchar* buf )
    {
        GetDelegateForFunction< PFNGLDEBUGMESSAGEINSERTPROC >( "glDebugMessageInsert", out _glDebugMessageInsert );

        _glDebugMessageInsert( source, type, id, severity, length, buf );
    }

    // ========================================================================

    public void DebugMessageCallback( GLDEBUGPROC callback, void* userParam )
    {
        GetDelegateForFunction< PFNGLDEBUGMESSAGECALLBACKPROC >( "glDebugMessageCallback", out _glDebugMessageCallback );

        _glDebugMessageCallback( callback, ( IntPtr )userParam );
    }

    /// <summary>
    /// Processes an OpenGL debug message, providing details such as the source, type,
    /// severity, and description of the message.
    /// </summary>
    /// <param name="source">
    /// An identifier for the origin of the debug message. It determines where the message
    /// originated, such as API calls or shader compilers.
    /// </param>
    /// <param name="type">
    /// The type of the OpenGL debug message, indicating the nature of the event (e.g.,
    /// errors, performance warnings).
    /// </param>
    /// <param name="id">
    /// A numeric identifier for the debug message. This can be used to differentiate
    /// specific events.
    /// </param>
    /// <param name="severity">
    /// Specifies the level of severity of the event, identifying whether the message is
    /// informational, a warning, or critical.
    /// </param>
    /// <param name="length">
    /// The length of the debug message string, as provided by OpenGL. This parameter
    /// may be useful for handling the incoming message data.
    /// </param>
    /// <param name="msg">
    /// A string describing the debug message. This provides detailed information regarding
    /// the event or error reported by OpenGL.
    /// </param>
    /// <param name="userParam">
    /// An optional user-defined pointer passed to the callback. This parameter allows for
    /// additional context or state to be associated with the debug message.
    /// </param>
    public void MessageCallback( int source,
                                 int type,
                                 GLuint id,
                                 int severity,
                                 int length,
                                 GLchar* msg,
                                 IntPtr userParam )
    {
        var message = new string( ( GLbyte* )msg, 0, length, Encoding.UTF8 );

        var srcStr = source switch
        {
            IGL.GL_DEBUG_SOURCE_API             => "API",
            IGL.GL_DEBUG_SOURCE_WINDOW_SYSTEM   => "WINDOW SYSTEM",
            IGL.GL_DEBUG_SOURCE_SHADER_COMPILER => "SHADER COMPILER",
            IGL.GL_DEBUG_SOURCE_THIRD_PARTY     => "THIRD PARTY",
            IGL.GL_DEBUG_SOURCE_APPLICATION     => "APPLICATION",
            IGL.GL_DEBUG_SOURCE_OTHER           => "OTHER",
            var _                               => "UNKNOWN",
        };

        var typeStr = type switch
        {
            IGL.GL_DEBUG_TYPE_ERROR               => "ERROR",
            IGL.GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR => "DEPRECATED_BEHAVIOR",
            IGL.GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR  => "UNDEFINED_BEHAVIOR",
            IGL.GL_DEBUG_TYPE_PORTABILITY         => "PORTABILITY",
            IGL.GL_DEBUG_TYPE_PERFORMANCE         => "PERFORMANCE",
            IGL.GL_DEBUG_TYPE_MARKER              => "MARKER",
            IGL.GL_DEBUG_TYPE_OTHER               => "OTHER",
            var _                                 => "UNKNOWN",
        };

        var severityStr = severity switch
        {
            IGL.GL_DEBUG_SEVERITY_NOTIFICATION => "NOTIFICATION",
            IGL.GL_DEBUG_SEVERITY_LOW          => "LOW",
            IGL.GL_DEBUG_SEVERITY_MEDIUM       => "MEDIUM",
            IGL.GL_DEBUG_SEVERITY_HIGH         => "HIGH",
            var _                              => "UNKNOWN",
        };

        Logger.Error( $"{srcStr}, {typeStr}, {severityStr}, {id}: {message}" );
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