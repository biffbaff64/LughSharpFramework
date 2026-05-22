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

// ============================================================================

using LughSharp.Source.Graphics.OpenGL.Enums;

using GLenum = int;
using GLfloat = float;
using GLint = int;
using GLsizei = int;
using GLuint = uint;
using GLboolean = bool;
using GLsizeiptr = int;
using GLintptr = int;
using GLint64 = long;

// ============================================================================

namespace LughSharp.Source.Graphics.OpenGL.Bindings;

public unsafe partial class GLBindings
{
    /// <summary>
    /// Bind a named buffer object.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object of the binding point to be modified. This buffer is
    /// one of those named in the <see cref="BufferTarget"/> enum.
    /// </param>
    /// <param name="buffer">Specifies the name of a buffer object.</param>
    public void BindBuffer( BufferTarget target, GLuint buffer )
    {
        GetDelegateForFunction< PFNGLBINDBUFFERPROC >( "glBindBuffer", out _glBindBuffer );

        _glBindBuffer( ( GLenum )target, buffer );
    }

    // ========================================================================

    /// <summary>
    /// Delete named buffer objects.
    /// </summary>
    /// <param name="n">Specifies the number of buffer objects to be deleted.</param>
    /// <param name="buffers">A pointer to an array of buffer objects to be deleted.</param>
    public void DeleteBuffers( GLsizei n, GLuint* buffers )
    {
        GetDelegateForFunction< PFNGLDELETEBUFFERSPROC >( "glDeleteBuffers", out _glDeleteBuffers );

        _glDeleteBuffers( n, buffers );
    }

    /// <summary>
    /// Delete named buffer objects.
    /// </summary>
    /// <param name="buffers">An array of buffer objects to be deleted.</param>
    public void DeleteBuffers( params GLuint[] buffers )
    {
        GetDelegateForFunction< PFNGLDELETEBUFFERSPROC >( "glDeleteBuffers", out _glDeleteBuffers );

        fixed ( GLuint* p = &buffers[ 0 ] )
        {
            _glDeleteBuffers( buffers.Length, p );
        }
    }

    // ========================================================================

    /// <summary>
    /// Generate buffer object names.
    /// </summary>
    /// <param name="n">Specifies the number of buffer object names to generate.</param>
    /// <param name="buffers">
    /// A pointer to an array in which the generated buffer object names are to be stored.
    /// </param>
    public void GenBuffers( GLsizei n, GLuint* buffers )
    {
        GetDelegateForFunction< PFNGLGENBUFFERSPROC >( "glGenBuffers", out _glGenBuffers );

        _glGenBuffers( n, buffers );
    }

    /// <summary>
    /// Generate buffer object names.
    /// </summary>
    /// <param name="n">Specifies the number of buffer object names to generate.</param>
    /// <returns>An array of generated buffer object names.</returns>
    public GLuint[] GenBuffers( GLsizei n )
    {
        GetDelegateForFunction< PFNGLGENBUFFERSPROC >( "glGenBuffers", out _glGenBuffers );

        var ret = new GLuint[ n ];

        fixed ( GLuint* p = &ret[ 0 ] )
        {
            _glGenBuffers( n, p );
        }

        return ret;
    }

    /// <summary>
    /// Generate a single buffer object name.
    /// </summary>
    /// <returns>The generated buffer object name.</returns>
    public GLuint GenBuffer()
    {
        return GenBuffers( 1 )[ 0 ];
    }

    // ========================================================================

    /// <summary>
    /// Determine if a name corresponds to a buffer object.
    /// </summary>
    /// <param name="buffer">A value that may be the name of a buffer object.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="buffer"/> is a buffer object name. <see langword="false"/>
    /// otherwise.
    /// </returns>
    public GLboolean IsBuffer( GLuint buffer )
    {
        GetDelegateForFunction< PFNGLISBUFFERPROC >( "glIsBuffer", out _glIsBuffer );

        return _glIsBuffer( buffer );
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="size"></param>
    /// <param name="data"></param>
    /// <param name="usage"></param>
    public void BufferData( BufferTarget target, int size, IntPtr data, BufferUsageHint usage )
    {
        GetDelegateForFunction< PFNGLBUFFERDATAPROC >( "glBufferData", out _glBufferData );

        _glBufferData( ( GLenum )target, size, data, ( GLenum )usage );

        CheckErrors();
    }

    /// <summary>
    /// Create and initialize a buffer object's data store.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="size">Specifies the size in bytes of the buffer object's new data store.</param>
    /// <param name="data">
    /// Specifies a pointer to data that will be copied into the data store for initialization, or
    /// <see cref="GLBindings.Null"/> if no data is to be copied.
    /// </param>
    /// <param name="usage">
    /// Specifies the expected usage pattern of the data store. The symbolic constant must be
    /// <see cref="IGL.GLStreamDraw"/>, <see cref="IGL.GLStreamRead"/>, <see cref="IGL.GLStreamCopy"/>,
    /// <see cref="IGL.GLStaticDraw"/>, <see cref="IGL.GLStaticRead"/>, <see cref="IGL.GLStaticCopy"/>,
    /// <see cref="IGL.GLDynamicDraw"/>, <see cref="IGL.GLDynamicRead"/> or <see cref="IGL.GLDynamicCopy"/>.
    /// </param>
    public void BufferData( GLenum target, int size, IntPtr data, GLenum usage )
    {
        GetDelegateForFunction< PFNGLBUFFERDATAPROC >( "glBufferData", out _glBufferData );

        _glBufferData( target, size, data, usage );

        CheckErrors();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="data"></param>
    /// <param name="usage"></param>
    /// <typeparam name="T"></typeparam>
    public void BufferData< T >( BufferTarget target, T[] data, BufferUsageHint usage ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLBUFFERDATAPROC >( "glBufferData", out _glBufferData );

        fixed ( T* p = &data[ 0 ] )
        {
            _glBufferData( ( GLenum )target, sizeof( T ) * data.Length, ( IntPtr )p, ( GLenum )usage );
        }
    }

    /// <summary>
    /// Create and initialize a buffer object's data store.
    /// </summary>
    /// <typeparam name="T">The type of the data to be copied.</typeparam>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="data">
    /// An array of <typeparamref name="T"/>s that will be copied into the data store for initialization.
    /// </param>
    /// <param name="usage">
    /// Specifies the expected usage pattern of the data store. The symbolic constant must be
    /// <see cref="IGL.GLStreamDraw"/>, <see cref="IGL.GLStreamRead"/>, <see cref="IGL.GLStreamCopy"/>,
    /// <see cref="IGL.GLStaticDraw"/>, <see cref="IGL.GLStaticRead"/>, <see cref="IGL.GLStaticCopy"/>,
    /// <see cref="IGL.GLDynamicDraw"/>, <see cref="IGL.GLDynamicRead"/> or <see cref="IGL.GLDynamicCopy"/>.
    /// </param>
    public void BufferData< T >( GLenum target, T[] data, GLenum usage ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLBUFFERDATAPROC >( "glBufferData", out _glBufferData );

        fixed ( T* p = &data[ 0 ] )
        {
            _glBufferData( target, sizeof( T ) * data.Length, ( IntPtr )p, usage );
        }
    }

    // ========================================================================

    /// <summary>
    /// Update a subset of a buffer object's data store.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="offset">
    /// Specifies the offset into the buffer object's data store where data replacement will begin,
    /// measured in bytes.
    /// </param>
    /// <param name="size">Specifies the size in bytes of the data store region being replaced.</param>
    /// <param name="data">Specifies a pointer to the new data that will be copied into the data store.</param>
    public void BufferSubData( GLenum target, GLintptr offset, GLsizeiptr size, IntPtr data )
    {
        GetDelegateForFunction< PFNGLBUFFERSUBDATAPROC >( "glBufferSubData", out _glBufferSubData );

        _glBufferSubData( target, offset, size, data );
    }

    /// <summary>
    /// Update a subset of a buffer object's data store.
    /// </summary>
    /// <typeparam name="T">The type of the data to be copied.</typeparam>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="offsetCount">Specifies the offset into the buffer object's data store where data replacement will begin.</param>
    /// <param name="data">An array of <typeparamref name="T"/>s that will be copied into the data store for replacement.</param>
    public void BufferSubData< T >( GLenum target, GLintptr offsetCount, T[] data ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLBUFFERSUBDATAPROC >( "glBufferSubData", out _glBufferSubData );

        fixed ( T* p = &data[ 0 ] )
        {
            _glBufferSubData( target, offsetCount * sizeof( T ), sizeof( T ) * data.Length, ( IntPtr )p );
        }
    }

    // ========================================================================

    /// <summary>
    /// Return a subset of a buffer object's data store.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="offset">
    /// Specifies the offset into the buffer object's data store from which data will be returned,
    /// measured in bytes.
    /// </param>
    /// <param name="size">Specifies the size in bytes of the data store region being returned.</param>
    /// <param name="data">Specifies a pointer to the location where buffer object data is returned.</param>
    public void GetBufferSubData( GLenum target, GLintptr offset, GLsizeiptr size, IntPtr data )
    {
        GetDelegateForFunction< PFNGLGETBUFFERSUBDATAPROC >( "glGetBufferSubData", out _glGetBufferSubData );

        _glGetBufferSubData( target, offset, size, data );
    }

    /// <summary>
    /// Return a subset of a buffer object's data store.
    /// </summary>
    /// <typeparam name="T">The type of the data to be returned.</typeparam>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="offsetCount">Specifies the offset into the buffer object's data store from which data will be returned.</param>
    /// <param name="count">Specifies the number of <typeparamref name="T"/>s to be returned.</param>
    /// <param name="data">An array of <typeparamref name="T"/>s that will be filled with the data from the buffer object.</param>
    public void GetBufferSubData< T >( GLenum target, GLintptr offsetCount, GLsizei count, ref T[] data )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLGETBUFFERSUBDATAPROC >( "glGetBufferSubData", out _glGetBufferSubData );

        fixed ( T* p = &data[ 0 ] )
        {
            _glGetBufferSubData( target, offsetCount * sizeof( T ), sizeof( T ) * count, ( IntPtr )p );
        }
    }

    // ========================================================================

    /// <summary>
    /// Map a buffer object's data store into the client's address space.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="access">
    /// Specifies a combination of access flags indicating the desired access to the range of the buffer
    /// object's data store. One of <see cref="IGL.GLReadOnly"/>, <see cref="IGL.GLWriteOnly"/> or
    /// <see cref="IGL.GLReadWrite"/>.
    /// </param>
    /// <returns>Returns a pointer to the beginning of the mapped range.</returns>
    public IntPtr MapBuffer( GLenum target, GLenum access )
    {
        GetDelegateForFunction< PFNGLMAPBUFFERPROC >( "glMapBuffer", out _glMapBuffer );

        return _glMapBuffer( target, access );
    }

    /// <summary>
    /// Map a buffer object's data store into the client's address space.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object. The symbolic constant must be <see cref="IGL.GLArrayBuffer"/>,
    /// <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>, <see cref="IGL.GLCopyWriteBuffer"/>,
    /// <see cref="IGL.GLDispatchIndirectBuffer"/>, <see cref="IGL.GLDrawIndirectBuffer"/>,
    /// <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>, <see cref="IGL.GLPixelUnpackBuffer"/>,
    /// <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>, <see cref="IGL.GL_TEXTURE_BUFFER"/>,
    /// <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <param name="access">
    /// Specifies a combination of access flags indicating the desired access to the range of the buffer
    /// object's data store. One of <see cref="IGL.GLReadOnly"/>, <see cref="IGL.GLWriteOnly"/> or
    /// <see cref="IGL.GLReadWrite"/>.
    /// </param>
    /// <returns>Returns a type-safe and memory-safe <see cref="System.Span{T}"/> of the buffers data.</returns>
    public Span< T > MapBuffer< T >( GLenum target, GLenum access ) where T : unmanaged
    {
        GLint size;

        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERIVPROC >( "glGetBufferParameteriv",
                                                                 out _glGetBufferParameteriv );

        _glGetBufferParameteriv( target, IGL.GLBufferSize, &size );

        GetDelegateForFunction< PFNGLMAPBUFFERPROC >( "glMapBuffer", out _glMapBuffer );

        IntPtr ret = _glMapBuffer( target, access );

        return new Span< T >( ( void* )ret, size / sizeof( T ) );
    }

    // ========================================================================

    /// <summary>
    /// Release a mapped buffer object's data store.
    /// </summary>
    /// <param name="target">
    /// Specifies the target buffer object being unmapped. The symbolic constant must be
    /// <see cref="IGL.GLArrayBuffer"/>, <see cref="IGL.GLAtomicCounterBuffer"/>, <see cref="IGL.GLCopyReadBuffer"/>,
    /// <see cref="IGL.GLCopyWriteBuffer"/>, <see cref="IGL.GLDispatchIndirectBuffer"/>,
    /// <see cref="IGL.GLDrawIndirectBuffer"/>, <see cref="IGL.GLElementArrayBuffer"/>, <see cref="IGL.GLPixelPackBuffer"/>
    /// , <see cref="IGL.GLPixelUnpackBuffer"/>, <see cref="IGL.GLQueryBuffer"/>, <see cref="IGL.GLShaderStorageBuffer"/>,
    /// <see cref="IGL.GL_TEXTURE_BUFFER"/>, <see cref="IGL.GLTransformFeedbackBuffer"/> or <see cref="IGL.GLUniformBuffer"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> unless the data store contents have become corrupt during the time the data store was
    /// mapped. This can occur for system-specific reasons that affect the availability of graphics memory, such as screen
    /// mode changes. In such situations, <see cref="IGLBindings.UnmapBuffer"/> may return <see langword="false"/> to
    /// indicate that
    /// the contents of the buffer have become corrupt and should be considered undefined. An application must detect this
    /// rare condition and reinitialize the data store.
    /// </returns>
    public GLboolean UnmapBuffer( GLenum target )
    {
        GetDelegateForFunction< PFNGLUNMAPBUFFERPROC >( "glUnmapBuffer", out _glUnmapBuffer );

        return _glUnmapBuffer( target );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetBufferParameteriv( GLenum target, GLenum pname, GLint* parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERIVPROC >( "glGetBufferParameteriv",
                                                                 out _glGetBufferParameteriv );

        _glGetBufferParameteriv( target, pname, parameters );
    }

    /// <inheritdoc />
    public void GetBufferParameteriv( GLenum target, GLenum pname, ref GLint[] parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERIVPROC >( "glGetBufferParameteriv",
                                                                 out _glGetBufferParameteriv );

        fixed ( GLint* p = &parameters[ 0 ] )
        {
            _glGetBufferParameteriv( target, pname, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetBufferPointerv( GLenum target, GLenum pname, IntPtr* parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPOINTERVPROC >( "glGetBufferPointerv", out _glGetBufferPointerv );

        _glGetBufferPointerv( target, pname, parameters );
    }

    /// <inheritdoc />
    public void GetBufferPointerv( GLenum target, GLenum pname, ref IntPtr[] parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPOINTERVPROC >( "glGetBufferPointerv", out _glGetBufferPointerv );

        fixed ( IntPtr* p = &parameters[ 0 ] )
        {
            _glGetBufferPointerv( target, pname, p );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void DrawBuffers( GLsizei n, GLenum* bufs )
    {
        GetDelegateForFunction< PFNGLDRAWBUFFERSPROC >( "glDrawBuffers", out _glDrawBuffers );

        _glDrawBuffers( n, bufs );
    }

    /// <inheritdoc />
    public void DrawBuffers( params GLenum[] bufs )
    {
        GetDelegateForFunction< PFNGLDRAWBUFFERSPROC >( "glDrawBuffers", out _glDrawBuffers );

        fixed ( GLenum* pbufs = &bufs[ 0 ] )
        {
            _glDrawBuffers( bufs.Length, pbufs );
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void BindBufferRange( GLenum target, GLuint index, GLuint buffer, GLintptr offset, GLsizeiptr size )
    {
        GetDelegateForFunction< PFNGLBINDBUFFERRANGEPROC >( "glBindBufferRange", out _glBindBufferRange );

        _glBindBufferRange( target, index, buffer, offset, size );
    }

    // ========================================================================

    /// <inheritdoc />
    public void BindBufferBase( GLenum target, GLuint index, GLuint buffer )
    {
        GetDelegateForFunction< PFNGLBINDBUFFERBASEPROC >( "glBindBufferBase", out _glBindBufferBase );

        _glBindBufferBase( target, index, buffer );
    }

    // ========================================================================

    /// <inheritdoc />
    public void GetBufferParameteri64V( GLenum target, GLenum pname, GLint64* parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERI64VPROC >( "glGetBufferParameteri64v",
                                                                   out _glGetBufferParameteri64v );

        _glGetBufferParameteri64v( target, pname, parameters );
    }

    /// <inheritdoc />
    public void GetBufferParameteri64V( GLenum target, GLenum pname, ref GLint64[] parameters )
    {
        GetDelegateForFunction< PFNGLGETBUFFERPARAMETERI64VPROC >( "glGetBufferParameteri64v",
                                                                   out _glGetBufferParameteri64v );

        {
            fixed ( GLint64* dp = &parameters[ 0 ] )
            {
                _glGetBufferParameteri64v( target, pname, dp );
            }
        }
    }

    // ========================================================================

    public void ClearBufferData( GLenum target, GLenum internalFormat, GLenum format, GLenum type, IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERDATAPROC >( "glClearBufferData", out _glClearBufferData );

        _glClearBufferData( target, internalFormat, format, type, data );
    }

    public void ClearBufferData< T >( GLenum target, GLenum internalFormat, GLenum format, GLenum type, T[] data )
        where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERDATAPROC >( "glClearBufferData", out _glClearBufferData );

        fixed ( T* t = &data[ 0 ] )
        {
            _glClearBufferData( target, internalFormat, format, type, ( IntPtr )t );
        }
    }

    // ========================================================================

    public void ClearBufferSubData( GLenum target, GLenum internalFormat, GLintptr offset, GLsizeiptr size,
                                    GLenum format, GLenum type,
                                    IntPtr data )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERSUBDATAPROC >( "glClearBufferSubData", out _glClearBufferSubData );

        _glClearBufferSubData( target, internalFormat, offset, size, format, type, data );
    }

    public void ClearBufferSubData< T >( GLenum target, GLenum internalFormat, GLintptr offset, GLsizeiptr size,
                                         GLenum format, GLenum type,
                                         T[] data ) where T : unmanaged
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERSUBDATAPROC >( "glClearBufferSubData", out _glClearBufferSubData );

        fixed ( T* t = &data[ 0 ] )
        {
            _glClearBufferSubData( target, internalFormat, offset, size, format, type, ( IntPtr )t );
        }
    }

    // ========================================================================

    public void InvalidateBufferSubData( GLuint buffer, GLintptr offset, GLsizeiptr length )
    {
        GetDelegateForFunction< PFNGLINVALIDATEBUFFERSUBDATAPROC >( "glInvalidateBufferSubData",
                                                                    out _glInvalidateBufferSubData );

        _glInvalidateBufferSubData( buffer, offset, length );
    }

    // ========================================================================

    public void InvalidateBufferData( GLuint buffer )
    {
        GetDelegateForFunction< PFNGLINVALIDATEBUFFERDATAPROC >( "glInvalidateBufferData",
                                                                 out _glInvalidateBufferData );

        _glInvalidateBufferData( buffer );
    }

    // ========================================================================

    public void ClearBufferiv( GLenum buffer, GLint drawbuffer, GLint* value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERIVPROC >( "glClearBufferiv", out _glClearBufferiv );

        _glClearBufferiv( buffer, drawbuffer, value );
    }

    public void ClearBufferiv( GLenum buffer, GLint drawbuffer, GLint[] value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERIVPROC >( "glClearBufferiv", out _glClearBufferiv );

        fixed ( GLint* p = &value[ 0 ] )
        {
            _glClearBufferiv( buffer, drawbuffer, p );
        }
    }

    // ========================================================================

    /// <summary>
    /// Clear a buffer to an unsigned integer value. Refer to <see href="https://docs.gl/gl4/glClearBuffer"/> for details
    /// on how to use this function.
    /// </summary>
    /// <param name="buffer">
    /// Specifies the buffer to clear. <paramref name="buffer"/> must be one of <see cref="IGL.GLColor"/>,
    /// <see cref="IGL.GLDepth"/>, <see cref="IGL.GLStencil"/>.
    /// </param>
    /// <param name="drawbuffer">Specify a partical draw buffer to clear.</param>
    /// <param name="value">Specifies the value to clear the buffer to.</param>
    public void ClearBufferuiv( GLenum buffer, GLint drawbuffer, GLuint* value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERUIVPROC >( "glClearBufferuiv", out _glClearBufferuiv );

        _glClearBufferuiv( buffer, drawbuffer, value );
    }

    /// <summary>
    /// Clear a buffer to an unsigned integer value. Refer to <see href="https://docs.gl/gl4/glClearBuffer"/> for details
    /// on how to use this function.
    /// </summary>
    /// <param name="buffer">
    /// Specifies the buffer to clear. <paramref name="buffer"/> must be one of <see cref="IGL.GLColor"/>,
    /// <see cref="IGL.GLDepth"/>, <see cref="IGL.GLStencil"/>.
    /// </param>
    /// <param name="drawbuffer">Specify a partical draw buffer to clear.</param>
    /// <param name="value">Specifies the value to clear the buffer to.</param>
    public void ClearBufferuiv( GLenum buffer, GLint drawbuffer, GLuint[] value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERUIVPROC >( "glClearBufferuiv", out _glClearBufferuiv );

        fixed ( GLuint* p = &value[ 0 ] )
        {
            _glClearBufferuiv( buffer, drawbuffer, p );
        }
    }

    // ========================================================================

    /// <summary>
    /// Clear a buffer to a floating point value. Refer to <see href="https://docs.gl/gl4/glClearBuffer"/> for details on
    /// how to use this function.
    /// </summary>
    /// <param name="buffer">
    /// Specifies the buffer to clear. <paramref name="buffer"/> must be one of <see cref="IGL.GLColor"/>,
    /// <see cref="IGL.GLDepth"/>, <see cref="IGL.GLStencil"/>.
    /// </param>
    /// <param name="drawbuffer">Specify a partical draw buffer to clear.</param>
    /// <param name="value">Specifies the value to clear the buffer to.</param>
    public void ClearBufferfv( GLenum buffer, GLint drawbuffer, GLfloat* value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERFVPROC >( "glClearBufferfv", out _glClearBufferfv );

        _glClearBufferfv( buffer, drawbuffer, value );
    }

    /// <summary>
    /// Clear a buffer to a floating point value. Refer to <see href="https://docs.gl/gl4/glClearBuffer"/> for details on
    /// how to use this function.
    /// </summary>
    /// <param name="buffer">
    /// Specifies the buffer to clear. <paramref name="buffer"/> must be one of <see cref="IGL.GLColor"/>,
    /// <see cref="IGL.GLDepth"/>, <see cref="IGL.GLStencil"/>.
    /// </param>
    /// <param name="drawbuffer">Specify a partical draw buffer to clear.</param>
    /// <param name="value">Specifies the value to clear the buffer to.</param>
    public void ClearBufferfv( GLenum buffer, GLint drawbuffer, GLfloat[] value )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERFVPROC >( "glClearBufferfv", out _glClearBufferfv );

        fixed ( GLfloat* p = &value[ 0 ] )
        {
            _glClearBufferfv( buffer, drawbuffer, p );
        }
    }

    // ========================================================================

    /// <summary>
    /// Clear a buffer to a floating point value. Refer to <see href="https://docs.gl/gl4/glClearBuffer"/> for details on
    /// how to use this function.
    /// </summary>
    /// <param name="buffer">
    /// Specifies the buffer to clear. <paramref name="buffer"/> must be one of
    /// <see cref="IGL.GLDepthStencil"/>.
    /// </param>
    /// <param name="drawbuffer">Must be zero.</param>
    /// <param name="depth">Specifies the value to clear the depth buffer to.</param>
    /// <param name="stencil">Specifies the value to clear the stencil buffer to.</param>
    public void ClearBufferfi( GLenum buffer, GLint drawbuffer, GLfloat depth, GLint stencil )
    {
        GetDelegateForFunction< PFNGLCLEARBUFFERFIPROC >( "glClearBufferfi", out _glClearBufferfi );

        _glClearBufferfi( buffer, drawbuffer, depth, stencil );
    }

    // ========================================================================
}

// ============================================================================
// ============================================================================