﻿// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
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

using System.Text;

using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Utils.Buffers;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;

using ByteOrder = LughSharp.Lugh.Utils.ByteOrder;
using Platform = LughSharp.Lugh.Core.Platform;

namespace LughSharp.Lugh.Graphics.FrameBuffers;

/// <summary>
/// Encapsulates OpenGL ES 2.0 frame buffer objects. This is a simple helper
/// class which should cover most FBO uses. It will automatically create a
/// gltexture for the color attachment and a renderbuffer for the depth buffer.
/// You can get a hold of the gltexture by <see cref="GLFrameBuffer{T}.GetColorBufferTexture()" />.
/// This class will only work with OpenGL ES 2.0.
/// <para>
/// FrameBuffers are managed. In case of an OpenGL context loss, which only
/// happens on Android when a user switches to another application or receives
/// an incoming call, the framebuffer will be automatically recreated.
/// </para>
/// <para>
/// A FrameBuffer must be disposed if it is no longer needed
/// </para>
/// </summary>
/// <typeparam name="T">
/// Types which derive from GLTexture, such as Texture, Cubemap, TextureArray.
/// </typeparam>
[PublicAPI]
public class GLFrameBuffer< T > : IDisposable where T : GLTexture
{
    public const int GL_DEPTH24_STENCIL8_OES = 0x88F0;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new GLFrameBuffer. No <see cref="GLFrameBufferBuilder{TU}" /> specifications
    /// are provided for construction so information will need to be provided.
    /// </summary>
    protected GLFrameBuffer()
    {
        BufferBuilder = null!;
    }

    /// <summary>
    /// Creates a GLFrameBuffer from the specifications provided
    /// by bufferBuilder.
    /// </summary>
    protected GLFrameBuffer( GLFrameBufferBuilder< GLFrameBuffer< GLTexture > > bufferBuilder )
    {
        BufferBuilder = bufferBuilder;

        BuildBuffer();
    }

    public int  FramebufferHandle              { get; set; }
    public int  DepthbufferHandle              { get; set; }
    public int  StencilbufferHandle            { get; set; }
    public int  DepthStencilPackedBufferHandle { get; set; }
    public bool HasDepthStencilPackedBuffer    { get; set; }
    public bool HasMultipleTexturesPresent     { get; set; }

    // the frame buffers
    public Dictionary< IApplication, List< GLFrameBuffer< T > >? >? Buffers { get; set; } = new();

    // the color buffer texture
    public List< T > TextureAttachments { get; set; } = new();

    // the default framebuffer handle, a.k.a screen.
    public int DefaultFramebufferHandle { get; set; }

    // true if we have polled for the default handle already.
    public bool DefaultFramebufferHandleInitialized { get; set; }

    protected GLFrameBufferBuilder< GLFrameBuffer< GLTexture > > BufferBuilder { get; set; }

    // ========================================================================

    /// <summary>
    /// Releases all resources associated with the FrameBuffer.
    /// </summary>
    public void Dispose()
    {
        foreach ( var texture in TextureAttachments )
        {
            DisposeColorTexture( texture );
        }

        if ( HasDepthStencilPackedBuffer )
        {
            GL.DeleteRenderbuffers( ( uint )DepthStencilPackedBufferHandle );
        }
        else
        {
            if ( BufferBuilder.HasDepthRenderBuffer )
            {
                GL.DeleteRenderbuffers( ( uint )DepthbufferHandle );
            }

            if ( BufferBuilder.HasStencilRenderBuffer )
            {
                GL.DeleteRenderbuffers( ( uint )StencilbufferHandle );
            }
        }

        GL.DeleteFramebuffers( ( uint )FramebufferHandle );

        if ( Buffers?[ Api.App ] != null )
        {
            Buffers[ Api.App ]?.Remove( this );
        }
    }

    // ========================================================================

    /// <summary>
    /// Wrapper to allow calling of virtual method <see cref="Build" />
    /// from constructors.
    /// </summary>
    protected void BuildBuffer()
    {
        Build();
    }

    /// <summary>
    /// Override this method in a derived class to set up the
    /// backing texture as you like.
    /// </summary>
    protected virtual T CreateTexture( FrameBufferTextureAttachmentSpec attachmentSpec )
    {
        throw new GdxRuntimeException( "This method must be overriden by derived class(es)" );
    }

    /// <summary>
    /// Override this method in a derived class to dispose the
    /// backing texture as you like.
    /// </summary>
    protected virtual void DisposeColorTexture( T colorTexture )
    {
        throw new GdxRuntimeException( "This method must be overriden by derived class(es)" );
    }

    /// <summary>
    /// Override this method in a derived class to attach the backing
    /// texture to the GL framebuffer object.
    /// </summary>
    protected virtual void AttachFrameBufferColorTexture( T texture )
    {
        throw new GdxRuntimeException( "This method must be overriden by derived class(es)" );
    }

    /// <summary>
    /// Convenience method to return the first Texture attachment present in the fbo.
    /// </summary>
    public virtual T GetColorBufferTexture()
    {
        return TextureAttachments.First();
    }

    /// <summary>
    /// </summary>
    public virtual void Build()
    {
        InitialiseFrameBufferHandle();

        var width  = BufferBuilder.Width;
        var height = BufferBuilder.Height;

        SetupDepthStencilRenderBuffers( width, height );

        HasMultipleTexturesPresent = BufferBuilder.TextureAttachmentSpecs.Count > 1;

        AttachTextureAttachments();

        if ( BufferBuilder.HasDepthRenderBuffer )
        {
            GL.FramebufferRenderbuffer( IGL.GL_FRAMEBUFFER,
                                        IGL.GL_DEPTH_ATTACHMENT,
                                        IGL.GL_RENDERBUFFER,
                                        ( uint )DepthbufferHandle );
        }

        if ( BufferBuilder.HasStencilRenderBuffer )
        {
            GL.FramebufferRenderbuffer( IGL.GL_FRAMEBUFFER,
                                        IGL.GL_STENCIL_ATTACHMENT,
                                        IGL.GL_RENDERBUFFER,
                                        ( uint )StencilbufferHandle );
        }

        if ( BufferBuilder.HasPackedStencilDepthRenderBuffer )
        {
            GL.FramebufferRenderbuffer( IGL.GL_FRAMEBUFFER,
                                        IGL.GL_DEPTH_STENCIL_ATTACHMENT,
                                        IGL.GL_RENDERBUFFER,
                                        ( uint )DepthStencilPackedBufferHandle );
        }

        GL.BindRenderbuffer( IGL.GL_RENDERBUFFER, 0 );

        foreach ( var texture in TextureAttachments )
        {
            GL.BindTexture( texture.GLTarget, 0 );
        }

        var result = GL.CheckFramebufferStatus( IGL.GL_FRAMEBUFFER );

        result = HandleUnsupportedFrameBuffer( result, width, height );

        GL.BindFramebuffer( IGL.GL_FRAMEBUFFER, ( uint )DefaultFramebufferHandle );

        HandleIncompleteFrameBuffer( result );

        AddManagedFrameBuffer( Api.App, this );
    }

    /// <summary>
    /// Sets up depth and stencil render buffers based on the provided width and height.
    /// </summary>
    /// <param name="width">The width of the render buffers.</param>
    /// <param name="height">The height of the render buffers.</param>
    private void SetupDepthStencilRenderBuffers( int width, int height )
    {
        if ( BufferBuilder.HasDepthRenderBuffer )
        {
            //TODO: uint -> int -> uint ????
            DepthbufferHandle = ( int )GL.GenRenderbuffer();
            GL.BindRenderbuffer( IGL.GL_RENDERBUFFER, ( uint )DepthbufferHandle );

            if ( BufferBuilder.DepthRenderBufferSpec != null )
            {
                GL.RenderbufferStorage( IGL.GL_RENDERBUFFER,
                                        BufferBuilder.DepthRenderBufferSpec.InternalFormat,
                                        width,
                                        height );
            }
        }

        if ( BufferBuilder.HasStencilRenderBuffer )
        {
            StencilbufferHandle = ( int )GL.GenRenderbuffer();
            GL.BindRenderbuffer( IGL.GL_RENDERBUFFER, ( uint )StencilbufferHandle );

            if ( BufferBuilder.StencilRenderBufferSpec != null )
            {
                GL.RenderbufferStorage( IGL.GL_RENDERBUFFER,
                                        BufferBuilder.StencilRenderBufferSpec.InternalFormat,
                                        width,
                                        height );
            }
        }

        if ( BufferBuilder.HasPackedStencilDepthRenderBuffer )
        {
            DepthStencilPackedBufferHandle = ( int )GL.GenRenderbuffer();
            GL.BindRenderbuffer( IGL.GL_RENDERBUFFER, ( uint )DepthStencilPackedBufferHandle );

            if ( BufferBuilder.PackedStencilDepthRenderBufferSpec != null )
            {
                GL.RenderbufferStorage( IGL.GL_RENDERBUFFER,
                                        BufferBuilder.PackedStencilDepthRenderBufferSpec.InternalFormat,
                                        width,
                                        height );
            }
        }
    }

    /// <summary>
    /// Attaches the texture attachments to the framebuffer.
    /// </summary>
    private unsafe void AttachTextureAttachments()
    {
        var colorTextureCounter = 0;

        // If there are multiple textures present, attach each one to the framebuffer
        if ( HasMultipleTexturesPresent )
        {
            foreach ( var attachmentSpec in BufferBuilder.TextureAttachmentSpecs )
            {
                var texture = CreateTexture( attachmentSpec );
                TextureAttachments.Add( texture );

                var tempHandle = ( uint )texture.GLTextureHandle;

                // Attach color texture to the specified color attachment point
                if ( attachmentSpec.IsColorTexture )
                {
                    GL.FramebufferTexture2D( IGL.GL_FRAMEBUFFER,
                                             IGL.GL_COLOR_ATTACHMENT0 + colorTextureCounter,
                                             IGL.GL_TEXTURE_2D,
                                             tempHandle,
                                             0 );

                    colorTextureCounter++;
                }

                // Attach depth texture to the depth attachment point
                else if ( attachmentSpec.IsDepth )
                {
                    GL.FramebufferTexture2D( IGL.GL_FRAMEBUFFER,
                                             IGL.GL_DEPTH_ATTACHMENT,
                                             IGL.GL_TEXTURE_2D,
                                             tempHandle,
                                             0 );
                }

                // Attach stencil texture to the stencil attachment point
                else if ( attachmentSpec.IsStencil )
                {
                    GL.FramebufferTexture2D( IGL.GL_FRAMEBUFFER,
                                             IGL.GL_STENCIL_ATTACHMENT,
                                             IGL.GL_TEXTURE_2D,
                                             tempHandle,
                                             0 );
                }
            }
        }

        // If there's only one texture present, attach it to the framebuffer
        else
        {
            var texture = CreateTexture( BufferBuilder.TextureAttachmentSpecs.First() );
            TextureAttachments.Add( texture );

            GL.BindTexture( texture.GLTarget, ( uint )texture.GLTextureHandle );
        }

        // Set the draw buffers for multiple color attachments
        if ( HasMultipleTexturesPresent )
        {
            var buffer = new IntBuffer( colorTextureCounter );

            for ( var i = 0; i < colorTextureCounter; i++ )
            {
                buffer.PutInt( IGL.GL_COLOR_ATTACHMENT0 + i );
            }

            buffer.Position = 0;

            fixed ( int* ptr = &buffer.ToArray()[ 0 ] )
            {
                GL.DrawBuffers( colorTextureCounter, ptr );
            }
        }
        else
        {
            // Attach the framebuffer color texture for single texture attachments
            AttachFrameBufferColorTexture( TextureAttachments.First() );
        }
    }

    /// <summary>
    /// Handles the case where the framebuffer is unsupported, attempting to
    /// resolve it by creating a packed depth-stencil buffer if supported.
    /// </summary>
    /// <param name="result">The result code indicating the framebuffer status.</param>
    /// <param name="width">The width of the framebuffer.</param>
    /// <param name="height">The height of the framebuffer.</param>
    /// <returns>The result code after attempting to resolve the unsupported framebuffer issue.</returns>
    private int HandleUnsupportedFrameBuffer( int result, int width, int height )
    {
        if ( ( result == IGL.GL_FRAMEBUFFER_UNSUPPORTED )
             && BufferBuilder is { HasDepthRenderBuffer: true, HasStencilRenderBuffer: true }
             && ( Api.Graphics.SupportsExtension( "GL_OES_packed_depth_stencil" )
                  || Api.Graphics.SupportsExtension( "GL_EXT_packed_depth_stencil" ) ) )
        {
            // Delete existing render buffers
            if ( BufferBuilder.HasDepthRenderBuffer )
            {
                GL.DeleteRenderbuffers( ( uint )DepthbufferHandle );
                DepthbufferHandle = 0;
            }

            if ( BufferBuilder.HasStencilRenderBuffer )
            {
                GL.DeleteRenderbuffers( ( uint )StencilbufferHandle );
                StencilbufferHandle = 0;
            }

            if ( BufferBuilder.HasPackedStencilDepthRenderBuffer )
            {
                GL.DeleteRenderbuffers( ( uint )DepthStencilPackedBufferHandle );
                DepthStencilPackedBufferHandle = 0;
            }

            // Create a new packed depth-stencil buffer
            DepthStencilPackedBufferHandle = ( int )GL.GenRenderbuffer();
            HasDepthStencilPackedBuffer    = true;
            GL.BindRenderbuffer( IGL.GL_RENDERBUFFER, ( uint )DepthStencilPackedBufferHandle );
            GL.RenderbufferStorage( IGL.GL_RENDERBUFFER, GL_DEPTH24_STENCIL8_OES, width, height );
            GL.BindRenderbuffer( IGL.GL_RENDERBUFFER, 0 );

            // Attach the new buffer to the framebuffer
            GL.FramebufferRenderbuffer( IGL.GL_FRAMEBUFFER, IGL.GL_DEPTH_ATTACHMENT, IGL.GL_RENDERBUFFER,
                                        ( uint )DepthStencilPackedBufferHandle );
            GL.FramebufferRenderbuffer( IGL.GL_FRAMEBUFFER, IGL.GL_STENCIL_ATTACHMENT, IGL.GL_RENDERBUFFER,
                                        ( uint )DepthStencilPackedBufferHandle );

            // Re-check the framebuffer status
            result = GL.CheckFramebufferStatus( IGL.GL_FRAMEBUFFER );
        }

        return result;
    }

    /// <summary>
    /// Handles the case where the framebuffer is incomplete, disposing
    /// resources and throwing exceptions accordingly.
    /// </summary>
    /// <param name="result">The result code indicating the framebuffer status.</param>
    public virtual void HandleIncompleteFrameBuffer( int result )
    {
        if ( result != IGL.GL_FRAMEBUFFER_COMPLETE )
        {
            // Dispose color textures
            foreach ( var texture in TextureAttachments )
            {
                DisposeColorTexture( texture );
            }

            // Delete depth-stencil buffer or separate depth and stencil renderbuffers
            if ( HasDepthStencilPackedBuffer )
            {
                GL.DeleteBuffers( ( uint )DepthStencilPackedBufferHandle );
            }
            else
            {
                if ( BufferBuilder.HasDepthRenderBuffer )
                {
                    GL.DeleteRenderbuffers( ( uint )DepthbufferHandle );
                }

                if ( BufferBuilder.HasStencilRenderBuffer )
                {
                    GL.DeleteRenderbuffers( ( uint )StencilbufferHandle );
                }
            }

            // Delete the framebuffer
            GL.DeleteFramebuffers( ( uint )FramebufferHandle );

            // Handle specific incomplete framebuffer scenarios
            if ( result == IGL.GL_FRAMEBUFFER_INCOMPLETE_ATTACHMENT )
            {
                throw new GdxRuntimeException( "Frame buffer couldn't be constructed: incomplete attachment" );
            }

            if ( result == IGL.GL_FRAMEBUFFER_INCOMPLETE_DIMENSIONS )
            {
                throw new GdxRuntimeException( "Frame buffer couldn't be constructed: incomplete dimensions" );
            }

            if ( result == IGL.GL_FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT )
            {
                throw new GdxRuntimeException( "Frame buffer couldn't be constructed: missing attachment" );
            }

            if ( result == IGL.GL_FRAMEBUFFER_UNSUPPORTED )
            {
                throw new GdxRuntimeException( "Frame buffer couldn't be constructed: unsupported combination of formats" );
            }

            throw new GdxRuntimeException( "Frame buffer couldn't be constructed: unknown error " + result );
        }
    }

    /// <summary>
    /// </summary>
    private unsafe void InitialiseFrameBufferHandle()
    {
        if ( !DefaultFramebufferHandleInitialized )
        {
            DefaultFramebufferHandleInitialized = true;

            if ( Api.App.AppType == Platform.ApplicationType.IOS )
            {
                var intbuf = ByteBuffer.Allocate
                    ( ( 16 * sizeof( int ) ) / 8 ).Order( ByteOrder.NativeOrder ).AsIntBuffer();

                fixed ( int* ptr = &intbuf.ToArray()[ 0 ] )
                {
                    GL.GetIntegerv( IGL.GL_FRAMEBUFFER_BINDING, ptr );
                }

                DefaultFramebufferHandle = intbuf.GetInt( 0 );
            }
            else
            {
                DefaultFramebufferHandle = 0;
            }
        }

        FramebufferHandle = ( int )GL.GenFramebuffer();

        GL.BindFramebuffer( IGL.GL_FRAMEBUFFER, ( uint )FramebufferHandle );
    }

    /// <summary>
    /// Makes the frame buffer current so everything gets drawn to it.
    /// </summary>
    protected virtual void Bind()
    {
        GL.BindFramebuffer( IGL.GL_FRAMEBUFFER, ( uint )FramebufferHandle );
    }

    /// <summary>
    /// Unbinds the framebuffer, all drawing will be performed to the
    /// normal framebuffer from here on.
    /// </summary>
    public virtual void Unbind()
    {
        GL.BindFramebuffer( IGL.GL_FRAMEBUFFER, ( uint )DefaultFramebufferHandle );
    }

    /// <summary>
    /// Binds the frame buffer and sets the viewport accordingly,
    /// so everything gets drawn to it.
    /// </summary>
    public virtual void Begin()
    {
        Bind();
        SetFrameBufferViewport();
    }

    /// <summary>
    /// Sets viewport to the dimensions of framebuffer.
    /// Called by <see cref="Begin()" />.
    /// </summary>
    public void SetFrameBufferViewport()
    {
        GL.Viewport( 0, 0, BufferBuilder.Width, BufferBuilder.Height );
    }

    /// <summary>
    /// Unbinds the framebuffer, all drawing will be performed to the
    /// normal framebuffer from here on.
    /// </summary>
    public virtual void End()
    {
        End( 0, 0, Api.Graphics.BackBufferWidth, Api.Graphics.BackBufferHeight );
    }

    /// <summary>
    /// Unbinds the framebuffer and sets viewport sizes, all drawing will be
    /// performed to the normal framebuffer from here on.
    /// </summary>
    /// <param name="x"> the x-axis position of the viewport in pixels </param>
    /// <param name="y"> the y-asis position of the viewport in pixels </param>
    /// <param name="width"> the width of the viewport in pixels </param>
    /// <param name="height"> the height of the viewport in pixels  </param>
    public virtual void End( int x, int y, int width, int height )
    {
        Unbind();
        GL.Viewport( x, y, width, height );
    }

    /// <summary>
    /// </summary>
    /// <param name="app"></param>
    /// <param name="frameBuffer"></param>
    private void AddManagedFrameBuffer( IApplication app, GLFrameBuffer< T > frameBuffer )
    {
        if ( Buffers == null )
        {
            throw new GdxRuntimeException( "Buffers is NULL!" );
        }

        var managedResources = Buffers[ app ] ?? [ ];

        managedResources.Add( frameBuffer );
        Buffers.Put( app, managedResources );
    }

    /// <summary>
    /// Invalidates all frame buffers. This can be used when the OpenGL context is
    /// lost to rebuild all managed frame buffers. This assumes that the texture
    /// attached to this buffer has already been rebuild! Use with care.
    /// </summary>
    public void InvalidateAllFrameBuffers( IApplication app )
    {
        ArgumentNullException.ThrowIfNull( app );

        if ( Buffers?[ app ] == null )
        {
            return;
        }

        for ( var i = 0; i < Buffers[ app ]?.Count; i++ )
        {
            Buffers[ app ]?[ i ].Build();
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="app"></param>
    public void ClearAllFrameBuffers( IApplication app )
    {
        Buffers?.Remove( app );
    }

    /// <summary>
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public StringBuilder GetManagedStatus( in StringBuilder builder )
    {
        builder.Append( "Managed buffers/app: { " );

        if ( Buffers == null )
        {
            builder.Append( "null" );
        }
        else
        {
            if ( Buffers?.Keys != null )
            {
                foreach ( var app in Buffers.Keys )
                {
                    builder.Append( Buffers[ app ]?.Count );
                    builder.Append( ' ' );
                }
            }
        }

        builder.Append( '}' );

        return builder;
    }
}