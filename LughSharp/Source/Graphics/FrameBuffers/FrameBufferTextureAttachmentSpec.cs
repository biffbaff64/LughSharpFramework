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
/// A specification for a texture attachment to a framebuffer. This class encapsulates the internal
/// format, format, and type of the texture, as well as flags indicating whether the texture is a 
/// float texture, GPU-only, depth texture, or stencil texture.
/// <para>
/// This information is crucial for correctly configuring framebuffer attachments in OpenGL and 
/// ensuring that the textures are created with the appropriate properties for rendering operations.
/// </para>
/// </summary>
/// <param name="internalFormat">The internal format of the texture.</param>
/// <param name="format">The format of the texture.</param>
/// <param name="type">The type of the texture.</param>
[PublicAPI]
public class FrameBufferTextureAttachmentSpec( int internalFormat, int format, int type )
{
    /// <summary>
    /// Gets the internal format identifier used by the resource.
    /// </summary>
    public int InternalFormat { get; } = internalFormat;

    /// <summary>
    /// Gets the format code associated with the current instance.
    /// </summary>
    public int Format { get; } = format;

    /// <summary>
    /// Gets the type identifier associated with the current instance.
    /// </summary>
    public int Type { get; } = type;

    /// <summary>
    /// Gets a value indicating whether the value is represented as a floating-point number.
    /// </summary>
    public bool IsFloat { get; init; }

    /// <summary>
    /// Gets a value indicating whether the resource is restricted to GPU usage only.
    /// </summary>
    public bool IsGpuOnly { get; init; }

    /// <summary>
    /// Gets a value indicating whether the current instance represents a depth measurement.
    /// </summary>
    public bool IsDepth { get; init; }

    /// <summary>
    /// Gets a value indicating whether the object is used as a stencil.
    /// </summary>
    public bool IsStencil { get; init; }

    /// <summary>
    /// Gets a value indicating whether the texture represents a color texture.
    /// </summary>
    public bool IsColorTexture => !IsDepth && !IsStencil;
}

// ============================================================================
// ============================================================================