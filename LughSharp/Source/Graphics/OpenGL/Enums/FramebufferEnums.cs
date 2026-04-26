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

namespace LughSharp.Source.Graphics.OpenGL.Enums;

[PublicAPI]
public enum FramebufferTarget
{
    Framebuffer     = IGL.GLFramebuffer,
    ReadFramebuffer = IGL.GLReadFramebuffer,
    DrawFramebuffer = IGL.GLDrawFramebuffer
}

[PublicAPI]
public enum FramebufferAttachment
{
    ColorAttachment0       = IGL.GLColorAttachment0,
    DepthAttachment        = IGL.GLDepthAttachment,
    StencilAttachment      = IGL.GLStencilAttachment,
    DepthStencilAttachment = IGL.GLDepthStencilAttachment
}

[PublicAPI]
public enum FramebufferStatus
{
    Complete                    = IGL.GLFramebufferComplete,
    Undefined                   = IGL.GLFramebufferUndefined,
    IncompleteAttachment        = IGL.GLFramebufferIncompleteAttachment,
    IncompleteMissingAttachment = IGL.GLFramebufferIncompleteMissingAttachment,
    IncompleteDrawBuffer        = IGL.GLFramebufferIncompleteDrawBuffer,
    IncompleteReadBuffer        = IGL.GLFramebufferIncompleteReadBuffer,
    Unsupported                 = IGL.GLFramebufferUnsupported
}

[PublicAPI]
public enum FramebufferAttachmentPoint
{
    ColorAttachment0  = IGL.GLColorAttachment0,
    ColorAttachment1  = IGL.GLColorAttachment1,
    ColorAttachment2  = IGL.GLColorAttachment2,
    DepthAttachment   = IGL.GLDepthAttachment,
    StencilAttachment = IGL.GLStencilAttachment
}

// ========================================================================
// ========================================================================