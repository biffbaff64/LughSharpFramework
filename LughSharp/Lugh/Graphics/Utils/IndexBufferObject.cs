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

namespace LughSharp.Lugh.Graphics.Utils;

/// <summary>
/// An IndexBufferObject wraps OpenGL's index buffer functionality to be used in conjunction
/// with VBOs.
/// <para>
/// You can also use this to store indices for vertex arrays. Do not call <see cref="Bind()" />
/// or <see cref="Unbind()" /> in this case but rather use <see cref="GetBuffer(bool)" /> to use the
/// buffer directly with glDrawElements. You must also create the IndexBufferObject with the
/// second constructor and specify isDirect as true as glDrawElements in conjunction with vertex arrays needs
/// direct buffers.
/// </para>
/// <para>
/// VertexBufferObjects must be disposed via the {@link #dispose()} method when no longer needed
/// </para>
/// </summary>
[PublicAPI]
public class IndexBufferObject : IIndexData
{
    public int BufferID { get; private set; }

    // ========================================================================

    private readonly Buffer< short > _buffer;
    private readonly Buffer< byte >  _byteBuffer;
    private readonly bool            _empty;
    private readonly bool            _ownsBuffer;
    private readonly int             _usage;
    private          bool            _isBound = false;
    private          bool            _isDirty = true;

    // ========================================================================

    /// <summary>
    /// Constructs a new IndexBufferObject, setting <see cref="_usage" /> to
    /// <see cref="IGL.GL_STATIC_DRAW" /> and <see cref="maxIndices" /> to the
    /// given value.
    /// </summary>
    public IndexBufferObject( int maxIndices )
        : this( true, maxIndices )
    {
    }

    /// <summary>
    /// Constructs a new IndexBufferObject.
    /// </summary>
    /// <param name="isStatic">
    /// If true, the buffer will be created with static draw usage, otherwise
    /// with dynamic draw usage.
    /// </param>
    /// <param name="maxIndices">The maximum number of indices that this buffer can hold.</param>
    public IndexBufferObject( bool isStatic, int maxIndices )
    {
        // Determine if the buffer is empty based on the maxIndices parameter.
        _empty = maxIndices == 0;

        // If the buffer is empty, set maxIndices to 1 to avoid creating a zero-sized buffer.
        if ( _empty )
        {
            maxIndices = 1;
        }

        // Create a new byte buffer to hold the indices. Each index is a short (2 bytes).
        _byteBuffer = new Buffer< byte >( maxIndices * 2 );

        // Create a view of the byte buffer as a short buffer.
        _buffer = _byteBuffer.AsShortBuffer();

        // Set the ownership flag to true, indicating that this object owns the buffer.
        _ownsBuffer = true;

        // Flip the buffers to prepare them for reading.
        _buffer.Flip();
        _byteBuffer.Flip();

        // Generate a new OpenGL buffer handle.
        BufferID = ( int )GL.GenBuffer();

        // Set the usage flag for the buffer based on whether it is static or dynamic.
        _usage = isStatic ? IGL.GL_STATIC_DRAW : IGL.GL_DYNAMIC_DRAW;
    }

    /// <inheritdoc />
    public int NumIndices => _empty ? 0 : _buffer.Limit;

    /// <inheritdoc />
    public int NumMaxIndices => _empty ? 0 : _buffer.Capacity;

    /// <inheritdoc />
    public unsafe void SetIndices( short[] indices, int offset, int count )
    {
        _isDirty = true;

        _buffer.Clear();
        _buffer.PutShorts( indices, offset, count );
        _buffer.Flip();

        _byteBuffer.Position = 0;
        _byteBuffer.Limit    = count << 1;

        if ( _isBound )
        {
            fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
            {
                GL.BufferData( IGL.GL_ELEMENT_ARRAY_BUFFER, _byteBuffer.Limit, ( IntPtr )ptr, _usage );
            }

            _isDirty = false;
        }
    }

    /// <inheritdoc />
    public unsafe void SetIndices( Buffer< short > indices )
    {
        _isDirty = true;

        var pos = indices.Position;

        _buffer.Clear();
        _buffer.PutShorts( indices.ToArray() );
        _buffer.Flip();

        indices.Position = pos;

        _byteBuffer.Position = 0;
        _byteBuffer.Limit    = _buffer.Limit << 1;

        if ( _isBound )
        {
            fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
            {
                GL.BufferData( IGL.GL_ELEMENT_ARRAY_BUFFER, _byteBuffer.Limit, ( IntPtr )ptr, _usage );
            }

            _isDirty = false;
        }
    }

    /// <inheritdoc />
    public unsafe void UpdateIndices( int targetOffset, short[] indices, int offset, int count )
    {
        _isDirty = true;

        var pos = _byteBuffer.Position;

        _byteBuffer.Position = targetOffset * 2;

        BufferUtils.Copy( indices, offset, count, _byteBuffer );

        _byteBuffer.Position = pos;
        _buffer.Position     = 0;

        if ( _isBound )
        {
            fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
            {
                GL.BufferData( IGL.GL_ELEMENT_ARRAY_BUFFER, _byteBuffer.Limit, ( IntPtr )ptr, _usage );
            }

            _isDirty = false;
        }
    }

    /// <inheritdoc />
    public Buffer< short > GetBuffer( bool forWriting )
    {
        _isDirty = forWriting;

        return _buffer;
    }

    /// <inheritdoc />
    public void Bind()
    {
        if ( BufferID == 0 )
        {
            throw new GdxRuntimeException( "No buffer allocated!" );
        }

        GL.BindBuffer( IGL.GL_ELEMENT_ARRAY_BUFFER, ( uint )BufferID );

        if ( _isDirty )
        {
            _byteBuffer.Limit = _buffer.Limit * 2;

            unsafe
            {
                fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
                {
                    GL.BufferData( IGL.GL_ELEMENT_ARRAY_BUFFER, _byteBuffer.Limit, ( IntPtr )ptr, _usage );
                }
            }

            _isDirty = false;
        }

        _isBound = true;
    }

    /// <inheritdoc />
    public void Unbind()
    {
        GL.BindBuffer( IGL.GL_ELEMENT_ARRAY_BUFFER, 0 );
        _isBound = false;
    }

    /// <inheritdoc />
    public void Invalidate()
    {
        BufferID = ( int )GL.GenBuffer(); //TODO: ???
        _isDirty = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GL.BindBuffer( IGL.GL_ELEMENT_ARRAY_BUFFER, 0 );
        GL.DeleteBuffers( ( uint )BufferID );

        BufferID = 0;

        if ( _ownsBuffer )
        {
            _byteBuffer.Dispose();
        }

        GC.SuppressFinalize( this );
    }
}