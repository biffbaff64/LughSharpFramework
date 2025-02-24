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

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

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
    public (int major, int minor) GetOpenGLVersion()
    {
        var version = GetString( IGL.GL_VERSION );

        if ( version == null )
        {
            throw new GdxRuntimeException( "NULL GL Version returned!" );
        }

        return ( version[ 0 ], version[ 2 ] );
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    ///     Handles OpenGL debug messages or errors by logging a callback. This can be used to retrieve
    ///     diagnostic messages from OpenGL during runtime.
    /// </summary>
    /// <param name="source">
    ///     Specifies the source of the debug callback, identifying which part of the OpenGL system the
    ///     message originates from.
    /// </param>
    /// <param name="type">Specifies the type of debug message, such as error, performance warning, or other messages.</param>
    /// <param name="id">Specifies the unique ID for the message to help identify specific issues.</param>
    /// <param name="severity">Specifies the severity level of the message (e.g., notification, warning, or error).</param>
    /// <param name="length">Specifies the length of the message string in bytes.</param>
    /// <param name="message">A pointer to the actual message string provided by OpenGL.</param>
    /// <param name="userParam">A pointer to optional user-defined data passed to the callback.</param>
    public void MessageCallback( int source,
                                 int type,
                                 uint id,
                                 int severity,
                                 int length,
                                 byte* message,
                                 IntPtr userParam )
    {
        Logger.Error( $"GL CALLBACK: " +
                      $"{( type == IGL.GL_DEBUG_TYPE_ERROR ? "** GL ERROR **" : "" )} " +
                      $"source = {source:X}, " +
                      $"type = {type:X}, " +
                      $"severity = {severity:X}, " +
                      $"message = {BytePointerToString.Convert( message )}\n" );
    }

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    public void Hint( GLenum target, GLenum mode )
    {
        GetDelegateForFunction< PFNGLHINTPROC >( "glHint", out _glHint );

        _glHint( target, mode );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Clear( GLbitfield mask )
    {
        GetDelegateForFunction< PFNGLCLEARPROC >( "glClear", out _glClear );

        _glClear( mask );
    }

    // ========================================================================

    /// <inheritdoc />
    public void ClearColor( GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha )
    {
        GetDelegateForFunction< PFNGLCLEARCOLORPROC >( "glClearColor", out _glClearColor );

        _glClearColor( red, green, blue, alpha );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Disable( GLenum cap )
    {
        GetDelegateForFunction< PFNGLDISABLEPROC >( "glDisable", out _glDisable );

        _glDisable( cap );
    }

    // ========================================================================

    /// <inheritdoc />
    public void Enable( GLenum cap )
    {
        GetDelegateForFunction< PFNGLENABLEPROC >( "glEnable", out _glEnable );

        _glEnable( cap );
    }

    // ========================================================================

    /// <inheritdoc />
    public void LogicOp( GLenum opcode )
    {
        GetDelegateForFunction< PFNGLLOGICOPPROC >( "glLogicOp", out _glLogicOp );

        _glLogicOp( opcode );
    }

    // ========================================================================

    /// <inheritdoc />
    public GLboolean IsEnabled( GLenum cap )
    {
        GetDelegateForFunction< PFNGLISENABLEDPROC >( "glIsEnabled", out _glIsEnabled );

        return _glIsEnabled( cap );
    }

    // ========================================================================
}