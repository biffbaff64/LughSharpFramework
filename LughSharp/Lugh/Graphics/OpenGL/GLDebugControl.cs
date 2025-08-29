﻿// /////////////////////////////////////////////////////////////////////////////
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

using LughSharp.Lugh.Utils;

namespace LughSharp.Lugh.Graphics.OpenGL;

[PublicAPI]
public class GLDebugControl
{
    /// <summary>
    /// Sets up OpenGL's debug message callback and enables debug output. This helps capture and log
    /// OpenGL debug messages during runtime, providing details about issues such as errors, warnings,
    /// or performance bottlenecks in OpenGL operations.
    /// </summary>
    public static unsafe void EnableGLDebugOutput()
    {
        Logger.Debug( "********** SETTING UP GL DEBUG **********" );

        Glfw.WindowHint( WindowHint.OpenGLDebugContext, true );

        GL.Enable( ( int )EnableCap.DebugOutput );
        GL.Enable( ( int )EnableCap.DebugOutputSynchronous );
        GL.DebugMessageCallback( GL.MessageCallback, null );

        var array = new uint[ 1 ];

        fixed ( uint* ptr = &array[ 0 ] )
        {
            GL.DebugMessageControl( ( int )DebugSourceControl.DontCare,
                                    ( int )DebugTypeControl.DontCare,
                                    ( int )DebugSeverityControl.DontCare, 0,
                                    ptr,
                                    true );
        }
    }
}

// ========================================================================
// ========================================================================