﻿// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin / LughSharp Team.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

using Corelib.Lugh.Graphics.OpenGL;
using Corelib.Lugh.Maths;
using Corelib.Lugh.Utils;

namespace Corelib.Lugh.Graphics.Profiling;

[PublicAPI]
public abstract class BaseGLInterceptor
{
    public int          Calls           { get; set; }
    public int          TextureBindings { get; set; }
    public int          DrawCalls       { get; set; }
    public int          ShaderSwitches  { get; set; }
    public FloatCounter VertexCount     { get; set; } = new( 0 );

    protected readonly GLProfiler GLProfiler;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Constructs a new BaseGLInterceptor instance, setting the <see cref="GLProfiler"/>
    /// to the supplied instance.
    /// </summary>
    /// <param name="profiler"></param>
    protected BaseGLInterceptor( GLProfiler profiler )
    {
        GLProfiler = profiler;
    }

    /// <summary>
    /// Returns a string representation of the supplied GL Error number.
    /// </summary>
    /// <param name="error">
    /// One of <see cref="IGL.GL_INVALID_VALUE"/>, <see cref="IGL.GL_INVALID_OPERATION"/>,
    /// <see cref="IGL.GL_INVALID_FRAMEBUFFER_OPERATION"/>, <see cref="IGL.GL_INVALID_ENUM"/>,
    /// <see cref="IGL.GL_OUT_OF_MEMORY"/> or <see cref="IGL.GL_NO_ERROR"/>.
    /// </param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if an invalid error number is passed.
    /// </exception>
    public static string ResolveErrorNumber( int error )
    {
        return error switch
        {
            IGL.GL_INVALID_VALUE                 => "InvalidValue",
            IGL.GL_INVALID_OPERATION             => "InvalidOperation",
            IGL.GL_INVALID_FRAMEBUFFER_OPERATION => "InvalidFramebufferOperation",
            IGL.GL_INVALID_ENUM                  => "InvalidEnum",
            IGL.GL_OUT_OF_MEMORY                 => "OutOfMemory",
            IGL.GL_NO_ERROR                      => "NoError",
            var _                                => throw new ArgumentOutOfRangeException( nameof( error ), error, null ),
        };
    }

    /// <summary>
    /// Resets all profiling data.
    /// </summary>
    public void Reset()
    {
        Calls           = 0;
        TextureBindings = 0;
        DrawCalls       = 0;
        ShaderSwitches  = 0;
        VertexCount.Reset();
    }
    
    /// <summary>
    /// Handles any GL Errors generated by profiling methods.
    /// </summary>
    protected void CheckErrors()
    {
        var error = GdxApi.Bindings.GetError();

        while ( error != IGL.GL_NO_ERROR )
        {
            Logger.Checkpoint();

            GLProfiler?.Listener.OnError( error );

            error = GdxApi.Bindings.GetError();
        }
    }
}
