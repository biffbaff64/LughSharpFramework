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
using System.Runtime.InteropServices;

using LughSharp.Lugh.Graphics.OpenGL.Enums;
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
            var _                                => $"Unknown GL Error Code: {errorCode}",
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
            GdxApi.Bindings.GetIntegerv( ( int )BufferBindings.ArrayBufferBinding, ptr );
        }

        if ( currentBuffer[ 0 ] != expectedBuffer )
        {
            Logger.Error( $"Buffer not bound correctly! Expected {expectedBuffer}, got {currentBuffer[ 0 ]}" );
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
        var error = GdxApi.Bindings.GetError();

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
        Logger.Debug( "Setting up GL Debug" );

        unsafe
        {
            Glfw.WindowHint( WindowHint.OpenGLDebugContext, true );

            var debugProc = new GLBindings.GLDEBUGPROC( ( source, type, id, severity, length, message, userParam ) =>
            {
                var msg = Marshal.PtrToStringAnsi( ( IntPtr )message, length );

                Logger.Debug( $"\nOpenGL Debug Message:\n" +
                              $"Source  : 0x{source:X}\n" +
                              $"Type    : 0x{type:X}\n" +
                              $"ID      : 0x{id:X}\n" +
                              $"Severity: 0x{severity:X}\n" +
                              $"Message : {msg}" );

                if ( severity == DebugSeverity.DEBUG_SEVERITY_HIGH )
                {
                    Debugger.Break();
                }
            } );

            GdxApi.Bindings.DebugMessageCallback( debugProc, null );
            GdxApi.Bindings.Enable( IGL.GL_DEBUG_OUTPUT );
            GdxApi.Bindings.Enable( IGL.GL_DEBUG_OUTPUT_SYNCHRONOUS );
        }
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
        GdxApi.Bindings.BindBuffer( ( int )BufferTarget.ArrayBuffer, vbo );

        var dataPtr = GdxApi.Bindings.MapBuffer( ( int )BufferTarget.ArrayBuffer, ( int )BufferAccess.ReadOnly );

        if ( dataPtr == IntPtr.Zero )
        {
            Logger.Debug( "Error: Failed to map VBO." );

            return null;
        }

        var data = new float[ sizeInBytes / sizeof( float ) ];
        Marshal.Copy( dataPtr, data, 0, data.Length );

        GdxApi.Bindings.UnmapBuffer( ( int )BufferTarget.ArrayBuffer );
        GdxApi.Bindings.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 );

        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vbo"></param>
    /// <param name="sizeInBytes"></param>
    /// <param name="vertexSizeInFloats"></param>
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