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

using JetBrains.Annotations; namespace LughSharp.Core.Graphics.OpenGL;

[PublicAPI]
public class GLData
{
    public const int                   DEFAULT_GL_MAJOR             = 3;
    public const int                   DEFAULT_GL_MINOR             = 2;
    public const DotGLFW.ClientAPI     DEFAULT_CLIENT_API           = DotGLFW.ClientAPI.OpenGLAPI;
    public const DotGLFW.OpenGLProfile DEFAULT_OPENGL_PROFILE       = DotGLFW.OpenGLProfile.CoreProfile;
    public const bool                  DEFAULT_OPENGL_FORWARDCOMPAT = true;

    public const int TEXTURE_2D     = 0x0DE1;
    public const int EXTENSIONS     = 0x1F03;
    public const int MAJOR_VERSION  = 0x821B;
    public const int MINOR_VERSION  = 0x821C;
    public const int NUM_EXTENSIONS = 0x821D;

    // ========================================================================
    
    /// <summary>
    /// As named, this is the currently bound FBO, used to keep track.
    /// </summary>
    public static uint CurrentBoundFBO { get; set; } = 0;
}

// ========================================================================
// ========================================================================