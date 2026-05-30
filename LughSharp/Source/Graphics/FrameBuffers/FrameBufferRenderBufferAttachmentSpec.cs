// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

namespace LughSharp.Source.Graphics.FrameBuffers;

/// <summary>
/// Represents the specification for a renderbuffer attachment in a framebuffer, including
/// its internal format.
/// </summary>
/// <remarks>
/// Use this class to define the internal format of a renderbuffer when attaching it to a
/// framebuffer. The internal format determines how pixel data is stored in the renderbuffer,
/// which affects rendering and compatibility with framebuffer operations.
/// </remarks>
[PublicAPI]
public class FrameBufferRenderBufferAttachmentSpec
{
    /// <summary>
    /// Gets or sets the internal format identifier used for data processing or storage.
    /// </summary>
    public int InternalFormat { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the FrameBufferRenderBufferAttachmentSpec class with
    /// the specified internal format.
    /// </summary>
    /// <param name="internalFormat">
    /// The internal format to use for the render buffer attachment. The value typically 
    /// specifies the color, depth, or stencil format as required by the graphics API.
    /// </param>
    public FrameBufferRenderBufferAttachmentSpec( int internalFormat )
    {
        InternalFormat = internalFormat;
    }
}

// ============================================================================
// ============================================================================

