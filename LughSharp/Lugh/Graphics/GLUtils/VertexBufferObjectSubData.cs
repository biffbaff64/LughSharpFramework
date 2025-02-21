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

using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Utils.Buffers.NewBuffers
;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.GLUtils;

/// <summary>
/// A <see cref="IVertexData"/> implementation based on OpenGL vertex buffer objects.
/// If the OpenGL ES context was lost you can call <see cref="Invalidate()"/> to
/// recreate a new OpenGL vertex buffer object.
/// <para>
/// The data is bound via GLVertexAttribPointer() according to the attribute aliases
/// specified via <see cref="VertexAttributes"/> in the constructor. VertexBufferObjects
/// must be disposed via the <see cref="Dispose()"/> method when no longer needed.
/// </para>
/// </summary>
[PublicAPI]
public class VertexBufferObjectSubData : IVertexData
{
    public ByteBuffer       ByteBuffer { get; set; }
    public VertexAttributes Attributes { get; set; }

    private readonly FloatBuffer _buffer;
    private readonly bool        _isDirect;
    private readonly int         _usage;

    private int  _bufferHandle;
    private bool _isBound;
    private bool _isDirty;
    private bool _isStatic;

    // ========================================================================

    /// <summary>
    /// Constructs a new interleaved VertexBufferObject.
    /// </summary>
    /// <param name="isStatic"> whether the vertex data is static. </param>
    /// <param name="numVertices"> the maximum number of vertices </param>
    /// <param name="attributes"> the <see cref="VertexAttributes"/>.  </param>
    public VertexBufferObjectSubData( bool isStatic, int numVertices, params VertexAttribute[] attributes )
        : this( isStatic, numVertices, new VertexAttributes( attributes ) )
    {
    }

    /// <summary>
    /// Constructs a new interleaved VertexBufferObject.
    /// </summary>
    /// <param name="isStatic"> whether the vertex data is static. </param>
    /// <param name="numVertices"> the maximum number of vertices </param>
    /// <param name="attributes"> the <see cref="VertexAttributes"/>. </param>
    public VertexBufferObjectSubData( bool isStatic, int numVertices, VertexAttributes attributes )
    {
        _isStatic  = isStatic;
        Attributes = attributes;

        ByteBuffer = new ByteBuffer( Attributes.VertexSize * numVertices );
        _isDirect  = true;

        _usage  = isStatic ? IGL.GL_STATIC_DRAW : IGL.GL_DYNAMIC_DRAW;
        _buffer = ByteBuffer.AsFloatBuffer();

        _bufferHandle = CreateBufferObject();

        _buffer.Flip();
        ByteBuffer.Flip();
    }

    /// <summary>
    /// Returns the number of vertices this VertexData stores.
    /// </summary>
    public int NumVertices => ( _buffer.Limit * 4 ) / Attributes.VertexSize;

    /// <summary>
    /// Returns the maximum number of vertices this VertedData can store.
    /// </summary>
    public int NumMaxVertices => ByteBuffer.Capacity / Attributes.VertexSize;

    /// <summary>
    /// Sets the vertices of this VertexData, discarding the old vertex data. The
    /// count must equal the number of floats per vertex times the number of vertices
    /// to be copied to this VertexData. The order of the vertex attributes must be
    /// the same as specified at construction time via <see cref="VertexAttributes"/>.
    /// <para>
    /// This can be called in between calls to bind and unbind. The vertex data will
    /// be updated instantly.
    /// </para>
    /// </summary>
    /// <param name="vertices"> the vertex data </param>
    /// <param name="offset"> the offset to start copying the data from </param>
    /// <param name="count"> the number of floats to copy  </param>
    public void SetVertices( float[] vertices, int offset, int count )
    {
        _isDirty = true;

        if ( _isDirect )
        {
            //TODO: Check against LibGDX
            this.ByteBuffer.PutFloats( vertices, offset, count );

            _buffer.Position = 0;
            _buffer.Limit    = count;
        }
        else
        {
            _buffer.Clear();
            _buffer.PutFloats( vertices, offset, count );

            _buffer.Flip();
            ByteBuffer.Position = 0;
            ByteBuffer.Limit    = _buffer.Limit << 2;
        }

        BufferChanged();
    }

    /// <summary>
    /// Update (a portion of) the vertices. Does not resize the backing buffer.
    /// </summary>
    /// <param name="targetOffset"> the offset to copy the data to. </param>
    /// <param name="vertices"> the vertex data </param>
    /// <param name="sourceOffset"> the offset to start copying the data from </param>
    /// <param name="count"> the number of floats to copy  </param>
    public void UpdateVertices( int targetOffset, float[] vertices, int sourceOffset, int count )
    {
        _isDirty = true;

        if ( _isDirect )
        {
            var pos = ByteBuffer.Position;

            ByteBuffer.Position = targetOffset * 4;
            BufferUtils.Copy( vertices, sourceOffset, count, ByteBuffer );
            ByteBuffer.Position = pos;
        }
        else
        {
            throw new GdxRuntimeException( "Buffer must be allocated direct." );
        }

        BufferChanged();
    }

    /// <summary>
    /// Returns the underlying FloatBuffer and marks it as dirty, causing the buffer
    /// contents to be uploaded on the next call to bind. If you need immediate
    /// uploading use <see cref="IVertexData.SetVertices"/>; Any modifications made
    /// to the Buffer after the call to bind will not automatically be uploaded.
    /// </summary>
    /// <returns> the underlying <see cref="FloatBuffer"/> holding the vertex data. </returns>
    public FloatBuffer GetBuffer( bool forWriting )
    {
        _isDirty |= forWriting;

        return _buffer;
    }

    /// <summary>
    /// Binds this VertexData for rendering via glDrawArrays or glDrawElements.
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="locations"> array containing the attribute locations.</param>
    public unsafe void Bind( ShaderProgram shader, int[]? locations = null )
    {
        GdxApi.Bindings.BindBuffer( IGL.GL_ARRAY_BUFFER, ( uint )_bufferHandle );

        if ( _isDirty )
        {
            ByteBuffer.Limit = _buffer.Limit * 4;

            fixed ( void* ptr = &ByteBuffer.ToArray()[ 0 ] )
            {
                GdxApi.Bindings.BufferData( IGL.GL_ARRAY_BUFFER, ByteBuffer.Limit, ptr, _usage );
            }

            _isDirty = false;
        }

        var numAttributes = Attributes.Size;

        if ( locations == null )
        {
            for ( var i = 0; i < numAttributes; i++ )
            {
                var attribute = Attributes.Get( i );
                var location  = shader.GetAttributeLocation( attribute.Alias );

                if ( location < 0 )
                {
                    continue;
                }

                shader.EnableVertexAttribute( location );

                shader.SetVertexAttribute( location,
                                           attribute.NumComponents,
                                           attribute.Type,
                                           attribute.Normalized,
                                           Attributes.VertexSize,
                                           attribute.Offset );
            }
        }
        else
        {
            for ( var i = 0; i < numAttributes; i++ )
            {
                var attribute = Attributes.Get( i );
                var location  = locations[ i ];

                if ( location < 0 )
                {
                    continue;
                }

                shader.EnableVertexAttribute( location );

                shader.SetVertexAttribute( location,
                                           attribute.NumComponents,
                                           attribute.Type,
                                           attribute.Normalized,
                                           Attributes.VertexSize,
                                           attribute.Offset );
            }
        }

        _isBound = true;
    }

    /// <summary>
    /// Unbinds this VertexBufferObject.
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="locations"> array containing the attribute locations.</param>
    public void Unbind( ShaderProgram shader, int[]? locations = null )
    {
        Debug.Assert( Attributes != null, "Attributes != null" );

        var numAttributes = Attributes.Size;

        if ( locations == null )
        {
            for ( var i = 0; i < numAttributes; i++ )
            {
                shader.DisableVertexAttribute( Attributes.Get( i ).Alias );
            }
        }
        else
        {
            for ( var i = 0; i < numAttributes; i++ )
            {
                var location = locations[ i ];

                if ( location >= 0 )
                {
                    shader.DisableVertexAttribute( location );
                }
            }
        }

        GdxApi.Bindings.BindBuffer( IGL.GL_ARRAY_BUFFER, 0 );
        _isBound = false;
    }

    /// <summary>
    /// Generates a new Buffer Object.
    /// </summary>
    private unsafe int CreateBufferObject()
    {
        var result = GdxApi.Bindings.GenBuffer();

        GdxApi.Bindings.BindBuffer( IGL.GL_ARRAY_BUFFER, result );
        GdxApi.Bindings.BufferData( IGL.GL_ARRAY_BUFFER, ByteBuffer.Capacity, null!, _usage );
        GdxApi.Bindings.BindBuffer( IGL.GL_ARRAY_BUFFER, 0 );

        return ( int )result;
    }

    private unsafe void BufferChanged()
    {
        if ( _isBound )
        {
            fixed ( void* ptr = &ByteBuffer.ToArray()[ 0 ] )
            {
                GdxApi.Bindings.BufferSubData( IGL.GL_ARRAY_BUFFER, 0, ByteBuffer.Limit, ptr );
            }

            _isDirty = false;
        }
    }

    /// <summary>
    /// Invalidates the VertexData if applicable. Use this in case of a context loss.
    /// </summary>
    public void Invalidate()
    {
        _bufferHandle = CreateBufferObject();
        _isDirty      = true;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing,
    /// or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        GdxApi.Bindings.BindBuffer( IGL.GL_ARRAY_BUFFER, 0 );
        GdxApi.Bindings.DeleteBuffers( ( uint )_bufferHandle );
        _bufferHandle = 0;
    }
}