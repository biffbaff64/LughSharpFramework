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

using LughSharp.Source.Graphics.Images;
using LughSharp.Source.Graphics.OpenGL;

namespace LughSharp.Source.Graphics.FrameBuffers;

/// <summary>
/// Fluent builder for constructing <see cref="GLFrameBuffer{TU}"/> instances with
/// configurable texture and render-buffer attachments.
/// <para>
/// Call the various <c>Add*</c> methods to describe the desired attachments, then
/// call <see cref="Build"/> (overridden in a concrete subclass) to produce the
/// frame-buffer object.
/// </para>
/// </summary>
/// <typeparam name="TU">The concrete <see cref="GLFrameBuffer{GLTexture}"/> type to build.</typeparam>
/// <param name="width">Width of the frame buffer in pixels.</param>
/// <param name="height">Height of the frame buffer in pixels.</param>
[PublicAPI]
public class GLFrameBufferBuilder< TU >( int width, int height )
    where TU : GLFrameBuffer< GLTexture >
{
    /// <summary>
    /// Width of the frame buffer in pixels.
    /// </summary>
    public int Width { get; } = width;

    /// <summary>
    /// Height of the frame buffer in pixels.
    /// </summary>
    public int Height { get; } = height;

    /// <summary>
    /// Ordered list of texture attachment specifications added to this builder.
    /// </summary>
    public List< FrameBufferTextureAttachmentSpec > TextureAttachmentSpecs { get; } = new();

    /// <summary>
    /// Specification for the stencil render-buffer attachment, or <c>null</c> if none was added.
    /// </summary>
    public FrameBufferRenderBufferAttachmentSpec? StencilRenderBufferSpec { get; set; }

    /// <summary>
    /// Specification for the depth render-buffer attachment, or <c>null</c> if none was added.
    /// </summary>
    public FrameBufferRenderBufferAttachmentSpec? DepthRenderBufferSpec { get; set; }

    /// <summary>
    /// Specification for the packed depth/stencil render-buffer attachment, or <c>null</c> if none was added.
    /// </summary>
    public FrameBufferRenderBufferAttachmentSpec? PackedStencilDepthRenderBufferSpec { get; set; }

    /// <summary>
    /// <c>true</c> when a stencil render-buffer attachment has been configured.
    /// </summary>
    public bool HasStencilRenderBuffer { get; set; }

    /// <summary>
    /// <c>true</c> when a depth render-buffer attachment has been configured.
    /// </summary>
    public bool HasDepthRenderBuffer { get; set; }

    /// <summary>
    /// <c>true</c> when a packed depth/stencil render-buffer attachment has been configured.
    /// </summary>
    public bool HasPackedStencilDepthRenderBuffer { get; set; }

    // ========================================================================

    /// <summary>
    /// Adds a color texture attachment with explicit GL format parameters.
    /// </summary>
    /// <param name="internalFormat">GL internal format (e.g. <c>GL_RGBA8</c>).</param>
    /// <param name="format">GL base format (e.g. <c>GL_RGBA</c>).</param>
    /// <param name="type">GL data type (e.g. <c>GL_UNSIGNED_BYTE</c>).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GLFrameBufferBuilder< TU > AddColorTextureAttachment( int internalFormat,
                                                                 int format,
                                                                 int type )
    {
        TextureAttachmentSpecs.Add( new FrameBufferTextureAttachmentSpec( internalFormat, format, type ) );

        return this;
    }

    /// <summary>
    /// Adds a color texture attachment by deriving the GL format and data type from a
    /// <see cref="PixFormat"/> value. This is a convenience wrapper around
    /// <see cref="AddColorTextureAttachment"/>.
    /// </summary>
    /// <param name="format">A <see cref="PixFormat"/> constant used to resolve the GL format and type.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GLFrameBufferBuilder< TU > AddBasicColorTextureAttachment( int format )
    {
        int glFormat = PixFormat.ToGLFormat( format );
        int glType   = PixFormat.ToGLDataType( format );

        return AddColorTextureAttachment( glFormat, glFormat, glType );
    }

    /// <summary>
    /// Adds a floating-point texture attachment. Floating-point attachments are used for
    /// HDR rendering or other scenarios that require values outside the [0, 1] range.
    /// </summary>
    /// <param name="internalFormat">GL internal format (e.g. <c>GL_RGBA16F</c> or <c>GL_RGBA32F</c>).</param>
    /// <param name="format">GL base format (e.g. <c>GL_RGBA</c>).</param>
    /// <param name="type">GL data type (e.g. <c>GL_FLOAT</c>).</param>
    /// <param name="gpuOnly">
    /// When <c>true</c> the texture data lives exclusively on the GPU and cannot be read back to the CPU.
    /// </param>
    /// <returns>This builder instance for method chaining.</returns>
    public GLFrameBufferBuilder< TU > AddFloatAttachment( int internalFormat,
                                                          int format,
                                                          int type,
                                                          bool gpuOnly )
    {
        var spec = new FrameBufferTextureAttachmentSpec( internalFormat, format, type )
        {
            IsFloat   = true,
            IsGpuOnly = gpuOnly
        };

        TextureAttachmentSpecs.Add( spec );

        return this;
    }

    /// <summary>
    /// Adds a depth texture attachment. The attachment uses <c>GL_DEPTH_COMPONENT</c> as its
    /// base format and is flagged as a depth attachment.
    /// </summary>
    /// <param name="internalFormat">GL internal depth format (e.g. <c>GL_DEPTH_COMPONENT16</c>).</param>
    /// <param name="type">GL data type for the depth values (e.g. <c>GL_UNSIGNED_SHORT</c>).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GLFrameBufferBuilder< TU > AddDepthTextureAttachment( int internalFormat, int type )
    {
        var spec = new FrameBufferTextureAttachmentSpec( internalFormat, IGL.GLDepthComponent, type )
        {
            IsDepth = true
        };

        TextureAttachmentSpecs.Add( spec );

        return this;
    }

    /// <summary>
    /// Adds a stencil texture attachment. The attachment uses <c>GL_STENCIL_ATTACHMENT</c> as
    /// its base format and is flagged as a stencil attachment.
    /// </summary>
    /// <param name="internalFormat">GL internal stencil format (e.g. <c>GL_STENCIL_INDEX8</c>).</param>
    /// <param name="type">GL data type for the stencil values.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GLFrameBufferBuilder< TU > AddStencilTextureAttachment( int internalFormat, int type )
    {
        var spec = new FrameBufferTextureAttachmentSpec( internalFormat, IGL.GLStencilAttachment, type )
        {
            IsStencil = true
        };

        TextureAttachmentSpecs.Add( spec );

        return this;
    }

    /// <summary>
    /// Adds a depth render-buffer attachment with the specified internal format.
    /// Sets <see cref="HasDepthRenderBuffer"/> to <c>true</c>.
    /// </summary>
    /// <param name="internalFormat">GL internal depth format (e.g. <c>GL_DEPTH_COMPONENT16</c>).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GLFrameBufferBuilder< TU > AddDepthRenderBuffer( int internalFormat )
    {
        DepthRenderBufferSpec = new FrameBufferRenderBufferAttachmentSpec( internalFormat );
        HasDepthRenderBuffer  = true;

        return this;
    }

    /// <summary>
    /// Adds a stencil render-buffer attachment with the specified internal format.
    /// Sets <see cref="HasStencilRenderBuffer"/> to <c>true</c>.
    /// </summary>
    /// <param name="internalFormat">GL internal stencil format (e.g. <c>GL_STENCIL_INDEX8</c>).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GLFrameBufferBuilder< TU > AddStencilRenderBuffer( int internalFormat )
    {
        StencilRenderBufferSpec = new FrameBufferRenderBufferAttachmentSpec( internalFormat );
        HasStencilRenderBuffer  = true;

        return this;
    }

    /// <summary>
    /// Adds a packed depth/stencil render-buffer attachment with the specified internal format.
    /// Sets <see cref="HasPackedStencilDepthRenderBuffer"/> to <c>true</c>.
    /// </summary>
    /// <param name="internalFormat">GL internal packed format (e.g. <c>GL_DEPTH24_STENCIL8</c>).</param>
    /// <returns>This builder instance for method chaining.</returns>
    public GLFrameBufferBuilder< TU > AddStencilDepthPackedRenderBuffer( int internalFormat )
    {
        PackedStencilDepthRenderBufferSpec = new FrameBufferRenderBufferAttachmentSpec( internalFormat );
        HasPackedStencilDepthRenderBuffer  = true;

        return this;
    }

    /// <summary>
    /// Adds a depth render-buffer using the default <c>GL_DEPTH_COMPONENT16</c> internal format.
    /// Convenience wrapper for <see cref="AddDepthRenderBuffer"/>.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    public GLFrameBufferBuilder< TU > AddBasicDepthRenderBuffer()
    {
        return AddDepthRenderBuffer( IGL.GLDepthComponent16 );
    }

    /// <summary>
    /// Adds a stencil render-buffer using the default <c>GL_STENCIL_INDEX8</c> internal format.
    /// Convenience wrapper for <see cref="AddStencilRenderBuffer"/>.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    public GLFrameBufferBuilder< TU > AddBasicStencilRenderBuffer()
    {
        return AddStencilRenderBuffer( IGL.GLStencilIndex8 );
    }

    /// <summary>
    /// Adds a packed depth/stencil render-buffer using the default <c>GL_DEPTH24_STENCIL8</c>
    /// internal format. Convenience wrapper for <see cref="AddStencilDepthPackedRenderBuffer"/>.
    /// </summary>
    /// <returns>This builder instance for method chaining.</returns>
    public GLFrameBufferBuilder< TU > AddBasicStencilDepthPackedRenderBuffer()
    {
        return AddStencilDepthPackedRenderBuffer( IGL.GLDepth24Stencil8 );
    }

    /// <summary>
    /// Constructs and returns the configured frame-buffer object.
    /// Must be overridden by every concrete subclass.
    /// </summary>
    /// <returns>The newly created frame-buffer instance.</returns>
    /// <exception cref="RuntimeException">Always thrown from this base implementation.</exception>
    public virtual object Build()
    {
        throw new RuntimeException( "This method must be overriden by derived class(es)" );
    }
}

// ============================================================================
// ============================================================================