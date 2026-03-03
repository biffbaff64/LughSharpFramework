// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted; free of charge; to any person obtaining a copy
//  of this software and associated documentation files (the "Software"); to deal
//  in the Software without restriction; including without limitation the rights
//  to use; copy; modify; merge; publish; distribute; sublicense; and/or sell
//  copies of the Software; and to permit persons to whom the Software is
//  furnished to do so; subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS"; WITHOUT WARRANTY OF ANY KIND; EXPRESS OR
//  IMPLIED; INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY;
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM; DAMAGES OR OTHER
//  LIABILITY; WHETHER IN AN ACTION OF CONTRACT; TORT OR OTHERWISE; ARISING FROM;
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using JetBrains.Annotations;

namespace LughSharp.Core.Graphics.OpenGL;

[PublicAPI]
public class DebugSeverity
{
    public const int DebugOutput                     = 37600;
    public const int DebugOutputSynchronous         = 33346;
    public const int ContextFlagDebugBit           = 2;
    public const int MaxDebugMessageLength         = 37187;
    public const int MaxDebugLoggedMessages        = 37188;
    public const int DebugLoggedMessages            = 37189;
    public const int DebugNextLoggedMessageLength = 33347;
    public const int MaxDebugGroupStackDepth      = 33388;
    public const int DebugGroupStackDepth          = 33389;
    public const int MaxLabelLength                 = 33512;
    public const int DebugCallbackFunction          = 33348;
    public const int DebugCallbackUserParam        = 33349;
    public const int DebugSourceApi                 = 33350;
    public const int DebugSourceWindowSystem       = 33351;
    public const int DebugSourceShaderCompiler     = 33352;
    public const int DebugSourceThirdParty         = 33353;
    public const int DebugSourceApplication         = 33354;
    public const int DebugSourceOther               = 33355;
    public const int DebugTypeError                 = 33356;
    public const int DebugTypeDeprecatedBehavior   = 33357;
    public const int DebugTypeUndefinedBehavior    = 33358;
    public const int DebugTypePortability           = 33359;
    public const int DebugTypePerformance           = 33360;
    public const int DebugTypeOther                 = 33361;
    public const int DebugTypeMarker                = 33384;
    public const int DebugTypePushGroup            = 33385;
    public const int DebugTypePopGroup             = 33386;
    public const int DebugSeverityHigh              = 37190;
    public const int DebugSeverityMedium            = 37191;
    public const int DebugSeverityLow               = 37192;
    public const int DebugSeverityNotification      = 33387;
    public const int Buffer                           = 33504;
    public const int Shader                           = 33505;
    public const int Program                          = 33506;
    public const int Query                            = 33507;
    public const int ProgramPipeline                 = 33508;
    public const int Sampler                          = 33510;
    public const int DisplayList                     = 33511;
    public const int DontCare                        = -1;
}

// ============================================================================
// ============================================================================