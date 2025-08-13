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

using System.Diagnostics;

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.OpenGL;

[PublicAPI]
public static class GLUtils
{
    public const int           DEFAULT_GL_MAJOR             = 4;
    public const int           DEFAULT_GL_MINOR             = 5;
    public const ClientAPI     DEFAULT_CLIENT_API           = ClientAPI.OpenGLAPI;
    public const OpenGLProfile DEFAULT_OPENGL_PROFILE       = OpenGLProfile.CoreProfile;
    public const bool          DEFAULT_OPENGL_FORWARDCOMPAT = true;

    // ========================================================================

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Converts a specified OpenGL error code into a corresponding descriptive error string.
    /// </summary>
    /// <param name="errorCode">
    /// The error code to be translated, typically returned from OpenGL functions.
    /// </param>
    /// <returns>
    /// A string that describes the given OpenGL error code. If the error code is unrecognized,
    /// a generic message including the code is returned.
    /// </returns>
    public static string GetErrorString( int errorCode )
    {
        return errorCode switch
        {
            IGL.GL_INVALID_ENUM                  => "Invalid Enum",
            IGL.GL_INVALID_OPERATION             => "Invalid Operation",
            IGL.GL_INVALID_VALUE                 => "Invalid Value",
            IGL.GL_OUT_OF_MEMORY                 => "Out of memory",
            IGL.GL_INVALID_FRAMEBUFFER_OPERATION => "Invalid Framebuffer operation",
            IGL.GL_STACK_OVERFLOW                => "Stack Overflow",
            IGL.GL_STACK_UNDERFLOW               => "Stack Underflow",
            IGL.GL_CONTEXT_LOST                  => "Context Lost",

            // ----------------------------------

            IGL.GL_NO_ERROR => "No Error",

            // ----------------------------------

            var _ => $"Unknown GL Error Code: {errorCode}",
        };
    }

    /// <summary>
    /// </summary>
    public static void CreateCapabilities()
    {
    }

    /// <summary>
    /// Checks if there is a current OpenGL context bound to the calling thread.
    /// Throws a <see cref="GdxRuntimeException" /> if no OpenGL context is active.
    /// </summary>
    /// <exception cref="GdxRuntimeException">
    /// Thrown when no OpenGL context is currently bound to the thread.
    /// </exception>
    public static bool CheckOpenGLContext()
    {
        if ( Glfw.GetCurrentContext() == null )
        {
            throw new GdxRuntimeException( "No OpenGL context is current on this thread!" );
        }

        return true;
    }

    /// <summary>
    /// Verifies whether the currently bound buffer matches the expected buffer ID.
    /// Logs an error if there is a mismatch.
    /// </summary>
    /// <param name="expectedBuffer">The identifier of the buffer that is expected to be currently bound.</param>
    public static unsafe void CheckBufferBinding( uint expectedBuffer )
    {
        var currentBuffer = new int[ 1 ];

        fixed ( int* ptr = &currentBuffer[ 0 ] )
        {
            // Fetch the currently bound buffer
            GL.GetIntegerv( ( int )BufferBindings.ArrayBufferBinding, ptr );
        }

        if ( currentBuffer[ 0 ] != expectedBuffer )
        {
            Logger.Warning( $"Buffer not bound correctly! Expected {expectedBuffer}, got {currentBuffer[ 0 ]}" );
        }
    }

    /// <summary>
    /// Checks for OpenGL errors after a given stage of rendering or processing.
    /// Throws an <see cref="InvalidOperationException" /> if an OpenGL error is detected.
    /// </summary>
    /// <param name="stage">The description of the stage at which the error occurred.</param>
    /// <exception cref="InvalidOperationException">Thrown when an OpenGL error is detected.</exception>
    public static void CheckGLError( string stage )
    {
        var error = GL.GetError();

        if ( error != ( int )ErrorCode.NoError )
        {
            throw new InvalidOperationException( $"OpenGL error at {stage}: {error}" );
        }
    }

    /// <summary>
    /// Sets up OpenGL's debug message callback and enables debug output. This helps capture and log
    /// OpenGL debug messages during runtime, providing details about issues such as errors, warnings,
    /// or performance bottlenecks in OpenGL operations.
    /// </summary>
    public static void GLDebug()
    {
        Logger.Debug( "********** SETTING UP GL DEBUG **********" );

        unsafe
        {
            Glfw.WindowHint( WindowHint.OpenGLDebugContext, true );

            var debugProc = new GLBindings.GLDEBUGPROC( ( source, type, id, severity, length, message, userParam ) =>
            {
                var msg = Marshal.PtrToStringAnsi( ( IntPtr )message, length );

                MessageCallback( ( uint )source, ( uint )type, id, ( uint )severity, length, msg, userParam );

                if ( severity == DebugSeverity.DEBUG_SEVERITY_HIGH )
                {
                    Debugger.Break();
                }
            } );

            GL.DebugMessageCallback( debugProc, null );
            GL.Enable( IGL.GL_DEBUG_OUTPUT );
            GL.Enable( IGL.GL_DEBUG_OUTPUT_SYNCHRONOUS );
        }
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
    /// <param name="message">
    /// A string describing the debug message. This provides detailed information regarding
    /// the event or error reported by OpenGL.
    /// </param>
    /// <param name="userParam">
    /// An optional user-defined pointer passed to the callback. This parameter allows for
    /// additional context or state to be associated with the debug message.
    /// </param>
    public static void MessageCallback( uint source,
                                        uint type,
                                        uint id,
                                        uint severity,
                                        int length,
                                        string message,
                                        IntPtr userParam )
    {
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

        Logger.Warning( $"{srcStr}, {typeStr}, {severityStr}, {id}: {message}" );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vbo"></param>
    /// <param name="sizeInBytes"></param>
    /// <param name="vertexSizeInFloats"></param>
    /// <returns></returns>
    public static float[]? GetVboData( uint vbo, int sizeInBytes, int vertexSizeInFloats )
    {
        GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, vbo );

        var dataPtr = GL.MapBuffer( ( int )BufferTarget.ArrayBuffer, ( int )BufferAccess.ReadOnly );

        if ( dataPtr == IntPtr.Zero )
        {
            Logger.Debug( "Error: Failed to map VBO." );

            return null;
        }

        var data = new float[ sizeInBytes / sizeof( float ) ];
        Marshal.Copy( dataPtr, data, 0, data.Length );

        GL.UnmapBuffer( ( int )BufferTarget.ArrayBuffer );
        GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 );

        return data;
    }

    /// <summary>
    /// Logs the data contained in a specified Vertex Buffer Object (VBO) for debugging purposes.
    /// </summary>
    /// <param name="vbo">
    /// The ID of the Vertex Buffer Object to retrieve and print data from.
    /// </param>
    /// <param name="sizeInBytes"> The size of the data in the VBO, in bytes. </param>
    /// <param name="vertexSizeInFloats">
    /// The size of each vertex in the VBO, in terms of the number of floats it contains.
    /// </param>
    public static void PrintVboData( uint vbo, int sizeInBytes, int vertexSizeInFloats )
    {
        var data = GetVboData( vbo, sizeInBytes, vertexSizeInFloats );

        if ( data == null )
        {
            return;
        }

        Logger.Debug( $"sizeInBytes: {sizeInBytes}" );
        Logger.Debug( $"vertexSizeInFloats: {vertexSizeInFloats}" );
        Logger.Debug( $"data.Length: {data.Length}" );

        for ( var i = 0; i < data.Length; i += vertexSizeInFloats )
        {
            Logger.Debug( $"Vertex {i / vertexSizeInFloats}: " );

            for ( var j = 0; j < vertexSizeInFloats; j++ )
            {
                Logger.Debug( $"{data[ i + j ]} " );
            }
        }
    }
}

// ============================================================================
// ============================================================================