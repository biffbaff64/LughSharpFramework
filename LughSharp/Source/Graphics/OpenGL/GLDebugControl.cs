// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

using LughSharp.Source.Graphics.OpenGL.Enums;
using LughSharp.Source.Graphics.OpenGL.Bindings;

namespace LughSharp.Source.Graphics.OpenGL;

[PublicAPI]
public class GLDebugControl
{
    public const int DebugOutput                  = 0x92E0;
    public const int DebugOutputSynchronous       = 0x8242;
    public const int ContextFlagDebugBit          = 0x0002;
    public const int MaxDebugMessageLength        = 0x9143;
    public const int MaxDebugLoggedMessages       = 0x9144;
    public const int DebugLoggedMessages          = 0x9145;
    public const int DebugNextLoggedMessageLength = 0x8243;
    public const int MaxDebugGroupStackDepth      = 0x826C;
    public const int DebugGroupStackDepth         = 0x826D;
    public const int MaxLabelLength               = 0x82E8;
    public const int DebugCallbackFunction        = 0x8244;
    public const int DebugCallbackUserParam       = 0x8245;
    public const int DebugSourceApi               = 0x8246;
    public const int DebugSourceWindowSystem      = 0x8247;
    public const int DebugSourceShaderCompiler    = 0x8248;
    public const int DebugSourceThirdParty        = 0x8249;
    public const int DebugSourceApplication       = 0x824A;
    public const int DebugSourceOther             = 0x824B;
    public const int DebugTypeError               = 0x824C;
    public const int DebugTypeDeprecatedBehavior  = 0x824D;
    public const int DebugTypeUndefinedBehavior   = 0x824E;
    public const int DebugTypePortability         = 0x824F;
    public const int DebugTypePerformance         = 0x8250;
    public const int DebugTypeOther               = 0x8251;
    public const int DebugTypeMarker              = 0x8268;
    public const int DebugTypePushGroup           = 0x8269;
    public const int DebugTypePopGroup            = 0x826A;
    public const int DebugSeverityHigh            = 0x9146;
    public const int DebugSeverityMedium          = 0x9147;
    public const int DebugSeverityLow             = 0x9148;
    public const int DebugSeverityNotification    = 0x826B;
    public const int Buffer                       = 0x82E0;
    public const int Shader                       = 0x82E1;
    public const int Program                      = 0x82E2;
    public const int Query                        = 0x82E3;
    public const int ProgramPipeline              = 0x82E4;
    public const int Sampler                      = 0x82E6;
    public const int DisplayList                  = 0x82E7;
    public const int DontCare                     = -0x1;

    // ========================================================================
    
    private static GLBindings.GLDEBUGPROC? _debugCallback;
    
    // ========================================================================

    /// <summary>
    /// Sets up OpenGL's debug message callback and enables debug output. This helps
    /// capture and log OpenGL debug messages during runtime, providing details about
    /// issues such as errors, warnings, or performance bottlenecks in OpenGL operations.
    /// </summary>
    public static unsafe void EnableGLDebugOutput()
    {
        DotGLFW.Glfw.WindowHint( DotGLFW.WindowHint.OpenGLDebugContext, true );

        _debugCallback = Engine.GL.MessageCallback;
        Engine.GL.Enable( EnableCap.DebugOutput );
        Engine.GL.Enable( EnableCap.DebugOutputSynchronous );
        Engine.GL.DebugMessageCallback( _debugCallback, null );

        var array = new uint[ 1 ];

        fixed ( uint* ptr = &array[ 0 ] )
        {
            Engine.GL.DebugMessageControl( ( int )DebugSourceControl.DontCare,
                                           ( int )DebugTypeControl.DontCare,
                                           ( int )DebugSeverityControl.DontCare,
                                           0,
                                           ptr,
                                           true );
        }

        Logger.Debug( "GL Debug output enabled" );
    }
}

// ========================================================================
// ========================================================================