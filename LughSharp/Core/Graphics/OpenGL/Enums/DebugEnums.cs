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

using JetBrains.Annotations;

namespace LughSharp.Core.Graphics.OpenGL.Enums;

[PublicAPI]
public enum DebugSeverityControl
{
    High         = IGL.GL_DEBUG_SEVERITY_HIGH,
    Medium       = IGL.GL_DEBUG_SEVERITY_MEDIUM,
    Low          = IGL.GL_DEBUG_SEVERITY_LOW,
    Notification = IGL.GL_DEBUG_SEVERITY_NOTIFICATION,
    DontCare     = IGL.GL_DONT_CARE,
}

[PublicAPI]
public enum DebugSourceControl
{
    Api            = IGL.GL_DEBUG_SOURCE_API,
    WindowSystem   = IGL.GL_DEBUG_SOURCE_WINDOW_SYSTEM,
    ShaderCompiler = IGL.GL_DEBUG_SOURCE_SHADER_COMPILER,
    ThirdParty     = IGL.GL_DEBUG_SOURCE_THIRD_PARTY,
    Application    = IGL.GL_DEBUG_SOURCE_APPLICATION,
    Other          = IGL.GL_DEBUG_SOURCE_OTHER,
    DontCare       = IGL.GL_DONT_CARE,
}

[PublicAPI]
public enum DebugTypeControl
{
    Error              = IGL.GL_DEBUG_TYPE_ERROR,
    DeprecatedBehavior = IGL.GL_DEBUG_TYPE_DEPRECATED_BEHAVIOR,
    UndefinedBehavior  = IGL.GL_DEBUG_TYPE_UNDEFINED_BEHAVIOR,
    Portability        = IGL.GL_DEBUG_TYPE_PORTABILITY,
    Performance        = IGL.GL_DEBUG_TYPE_PERFORMANCE,
    Marker             = IGL.GL_DEBUG_TYPE_MARKER,
    Other              = IGL.GL_DEBUG_TYPE_OTHER,
    DontCare           = IGL.GL_DONT_CARE,
}

// ========================================================================
// ========================================================================