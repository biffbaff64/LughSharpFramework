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

namespace LughSharp.Lugh.Graphics.OpenGL.Enums;

[PublicAPI]
public enum FramebufferTarget
{
    Framebuffer     = IGL.GL_FRAMEBUFFER,
    ReadFramebuffer = IGL.GL_READ_FRAMEBUFFER,
    DrawFramebuffer = IGL.GL_DRAW_FRAMEBUFFER,
}

[PublicAPI]
public enum FramebufferAttachment
{
    ColorAttachment0       = IGL.GL_COLOR_ATTACHMENT0,
    DepthAttachment        = IGL.GL_DEPTH_ATTACHMENT,
    StencilAttachment      = IGL.GL_STENCIL_ATTACHMENT,
    DepthStencilAttachment = IGL.GL_DEPTH_STENCIL_ATTACHMENT,
}

[PublicAPI]
public enum FramebufferStatus
{
    Complete                    = IGL.GL_FRAMEBUFFER_COMPLETE,
    Undefined                   = IGL.GL_FRAMEBUFFER_UNDEFINED,
    IncompleteAttachment        = IGL.GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT,
    IncompleteMissingAttachment = IGL.GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT,
    IncompleteDrawBuffer        = IGL.GL_FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER,
    IncompleteReadBuffer        = IGL.GL_FRAMEBUFFER_INCOMPLETE_READ_BUFFER,
    Unsupported                 = IGL.GL_FRAMEBUFFER_UNSUPPORTED,
}

[PublicAPI]
public enum FramebufferAttachmentPoint
{
    ColorAttachment0  = IGL.GL_COLOR_ATTACHMENT0,
    ColorAttachment1  = IGL.GL_COLOR_ATTACHMENT1,
    ColorAttachment2  = IGL.GL_COLOR_ATTACHMENT2,
    DepthAttachment   = IGL.GL_DEPTH_ATTACHMENT,
    StencilAttachment = IGL.GL_STENCIL_ATTACHMENT,
}

// ========================================================================
// ========================================================================