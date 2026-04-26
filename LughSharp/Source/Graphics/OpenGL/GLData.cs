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

using JetBrains.Annotations;

namespace LughSharp.Source.Graphics.OpenGL;

[PublicAPI]
public class GLData
{
    public const int                   DefaultGLMajor             = 3;
    public const int                   DefaultGLMinor             = 2;
    public const DotGLFW.ClientAPI     DefaultClientApi           = DotGLFW.ClientAPI.OpenGLAPI;
    public const DotGLFW.OpenGLProfile DefaultOpenglProfile       = DotGLFW.OpenGLProfile.CoreProfile;
    public const bool                  DefaultOpenglForwardcompat = true;

    public const int Texture2D     = 0x0DE1;
    public const int Extensions    = 0x1F03;
    public const int MajorVersion  = 0x821B;
    public const int MinorVersion  = 0x821C;
    public const int NumExtensions = 0x821D;

    // ========================================================================

    /// <summary>
    /// As named, this is the currently bound FBO, used to keep track.
    /// </summary>
    public static uint CurrentBoundFbo { get; set; }
}

// ========================================================================
// ========================================================================