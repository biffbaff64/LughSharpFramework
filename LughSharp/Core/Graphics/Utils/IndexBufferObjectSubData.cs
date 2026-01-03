// ///////////////////////////////////////////////////////////////////////////////
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

using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics.Utils;

/// <summary>
/// IndexBufferObject wraps OpenGL's index buffer functionality to be used in conjunction with VBOs.
/// <para>
/// You can also use this to store indices for vertex arrays. Do not call <see cref="Bind()"/> or
/// <see cref="Unbind()"/> in this case but rather use <see cref="GetBuffer(bool)"/> to use the
/// buffer directly with GLDrawElements. You must also create the IndexBufferObject with the second
/// constructor and specify isDirect as true as glDrawElements in conjunction with vertex arrays
/// needs direct buffers.
/// </para>
/// <para>
/// IndexBufferObjects must be disposed via the <see cref="Dispose()"/> method when no longer needed.
/// </para>
/// </summary>
[PublicAPI]
public class IndexBufferObjectSubData : IIndexData
{
    private readonly Buffer< int >   _buffer;
    private readonly Buffer< byte >  _byteBuffer;
    private readonly BufferUsageHint _usage;
    private          int             _bufferHandle;
    private          bool            _isBound = false;
    private          bool            _isDirty = true;

    // ========================================================================

    /// <summary>
    /// Creates a new IndexBufferObject.
    /// </summary>
    /// <param name="isStatic"> whether the index buffer is static </param>
    /// <param name="maxIndices"> the maximum number of indices this buffer can hold </param>
    public IndexBufferObjectSubData( bool isStatic, int maxIndices )
    {
        _byteBuffer = new Buffer< byte >( maxIndices * sizeof( int ) );
        _usage      = isStatic ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw;
        _buffer     = _byteBuffer.AsIntBuffer();

        _buffer.Flip();
        _byteBuffer.Flip();

        _bufferHandle = CreateBufferObject();
    }

    /// <summary>
    /// Creates a new IndexBufferObject to be used with vertex arrays.
    /// </summary>
    /// <param name="maxIndices"> the maximum number of indices this buffer can hold </param>
    public IndexBufferObjectSubData( int maxIndices )
    {
        _byteBuffer = new Buffer< byte >( maxIndices * sizeof( int ) );
        _usage      = BufferUsageHint.StaticDraw;
        _buffer     = _byteBuffer.AsIntBuffer();

        _buffer.Flip();
        _byteBuffer.Flip();

        _bufferHandle = CreateBufferObject();
    }

    // ========================================================================

    /// <inheritdoc />
    public int NumIndices => _buffer.Limit;

    /// <inheritdoc />
    public int NumMaxIndices => _buffer.Capacity;

    /// <inheritdoc />
    public unsafe void SetIndices( int[] indices, int offset, int count )
    {
        _isDirty = true;

        _buffer.Clear();
        _buffer.PutInts( indices, offset, count );
        _buffer.Flip();

        _byteBuffer.Position = 0;
        _byteBuffer.Limit    = count << 1;

        if ( _isBound )
        {
            fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
            {
                GL.BufferSubData( IGL.GL_ELEMENT_ARRAY_BUFFER, 0, _byteBuffer.Limit, ( IntPtr )ptr );
            }

            _isDirty = false;
        }
    }

    /// <inheritdoc />
    public unsafe void SetIndices( Buffer< int > indices )
    {
        var pos = indices.Position;

        _isDirty = true;

        _buffer.Clear();
        _buffer.PutInts( indices.ToArray() );
        _buffer.Flip();

        indices.Position = pos;

        _byteBuffer.Position = 0;
        _byteBuffer.Limit    = _buffer.Limit << 1;

        if ( _isBound )
        {
            fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
            {
                GL.BufferSubData( IGL.GL_ELEMENT_ARRAY_BUFFER, 0, _byteBuffer.Limit, ( IntPtr )ptr );
            }

            _isDirty = false;
        }
    }

    /// <inheritdoc />
    public unsafe void UpdateIndices( int targetOffset, int[] indices, int offset, int count )
    {
        _isDirty = true;

        var pos = _byteBuffer.Position;

        _byteBuffer.Position = targetOffset * sizeof( int );

        BufferUtils.Copy( indices, offset, count, _byteBuffer );

        _byteBuffer.Position = pos;
        _buffer.Position     = 0;

        if ( _isBound )
        {
            fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
            {
                GL.BufferSubData( IGL.GL_ELEMENT_ARRAY_BUFFER, 0, _byteBuffer.Limit, ( IntPtr )ptr );
            }

            _isDirty = false;
        }
    }

    /// <inheritdoc />
    public Buffer< int > GetBuffer( bool forWriting )
    {
        _isDirty |= forWriting;

        return _buffer;
    }

    /// <inheritdoc />
    public unsafe void Bind()
    {
        if ( _bufferHandle == 0 )
        {
            throw new GdxRuntimeException( "IndexBufferObject cannot be used after it has been disposed." );
        }

        GL.BindBuffer( BufferTarget.ElementArrayBuffer, ( uint )_bufferHandle );

        if ( _isDirty )
        {
            _byteBuffer.Limit = _buffer.Limit * sizeof( int );

            fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
            {
                GL.BufferSubData( IGL.GL_ELEMENT_ARRAY_BUFFER, 0, _byteBuffer.Limit, ( IntPtr )ptr );
            }

            _isDirty = false;
        }

        _isBound = true;
    }

    /// <inheritdoc />
    public void Unbind()
    {
        GL.BindBuffer( BufferTarget.ElementArrayBuffer, 0 );
        _isBound = false;
    }

    /// <inheritdoc />
    public void Invalidate()
    {
        _bufferHandle = CreateBufferObject();
        _isDirty      = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GL.BindBuffer( BufferTarget.ElementArrayBuffer, 0 );
        GL.DeleteBuffers( ( uint )_bufferHandle );

        _bufferHandle = 0;
    }

    private int CreateBufferObject()
    {
        var result = GL.GenBuffer();

        GL.BindBuffer( BufferTarget.ElementArrayBuffer, result );
        GL.BufferData( BufferTarget.ElementArrayBuffer, _byteBuffer.Capacity, 0, _usage );
        GL.BindBuffer( BufferTarget.ElementArrayBuffer, 0 );

        return ( int )result;
    }
}