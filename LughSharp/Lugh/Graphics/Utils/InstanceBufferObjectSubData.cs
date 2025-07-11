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
using LughSharp.Lugh.Utils.Buffers;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Utils;

/// <summary>
/// Modification of the <see cref="VertexBufferObjectSubData" /> class.
/// Sets the glVertexAttribDivisor for every <see cref="VertexAttribute" />
/// automatically.
/// </summary>
[PublicAPI]
public class InstanceBufferObjectSubData : IInstanceData
{
    private readonly FloatBuffer _buffer;
    private readonly ByteBuffer  _byteBuffer;
    private readonly bool        _isDirect;

    private readonly int  _usage;
    private          bool _isBound = false;
    private          bool _isDirty = false;
    private          bool _isStatic;

    // ========================================================================

    /// <summary>
    /// Constructs a new interleaved InstanceBufferObject.
    /// </summary>
    /// <param name="isStatic"> whether the vertex data is static. </param>
    /// <param name="numInstances"> the maximum number of vertices. </param>
    /// <param name="instanceAttributes"> the <see cref="VertexAttributes" />". </param>
    public InstanceBufferObjectSubData( bool isStatic,
                                        int numInstances,
                                        params VertexAttribute[] instanceAttributes )
        : this( isStatic, numInstances, new VertexAttributes( instanceAttributes ) )
    {
    }

    /// <summary>
    /// Constructs a new interleaved InstanceBufferObject.
    /// </summary>
    /// <param name="isStatic"> whether the vertex data is static. </param>
    /// <param name="numInstances"> the maximum number of vertices. </param>
    /// <param name="instanceAttributes"> the <see cref="VertexAttributes" />". </param>
    public InstanceBufferObjectSubData( bool isStatic, int numInstances, VertexAttributes instanceAttributes )
    {
        _isStatic   = isStatic;
        Attributes  = instanceAttributes;
        _byteBuffer = new ByteBuffer( Attributes.VertexSize * numInstances );
        _isDirect   = true;

        _usage       = isStatic ? IGL.GL_STATIC_DRAW : IGL.GL_DYNAMIC_DRAW;
        _buffer      = _byteBuffer.AsFloatBuffer();
        BufferHandle = CreateBufferObject();

        _buffer.Flip();
        _byteBuffer.Flip();
    }

    public int              BufferHandle { get; set; }
    public VertexAttributes Attributes   { get; set; }

    /// <summary>
    /// Returns the number of instances in this buffer.
    /// </summary>
    public int NumInstances => ( _buffer.Limit * 4 ) / Attributes.VertexSize;

    /// <summary>
    /// Returns the max number of instances in this buffer.
    /// </summary>
    public int NumMaxInstances => _byteBuffer.Capacity / Attributes.VertexSize;

    public FloatBuffer GetBuffer( bool forWriting )
    {
        _isDirty |= forWriting; //TODO: ???

        return _buffer;
    }

    public void SetInstanceData( float[] data, int offset, int count )
    {
        _isDirty = true;

        if ( _isDirect )
        {
            _byteBuffer.PutFloats( data, offset, count );

            _buffer.Position = 0;
            _buffer.Limit    = count;
        }
        else
        {
            _buffer.Clear();

            _buffer.PutFloats( data, offset, count );

            _buffer.Flip();
            _byteBuffer.Position = 0;
            _byteBuffer.Limit    = _buffer.Limit << 2;
        }

        BufferChanged();
    }

    public void SetInstanceData( FloatBuffer data, int count )
    {
        _isDirty = true;

        if ( _isDirect )
        {
            BufferUtils.Copy( data.ToArray(), 0, count, _byteBuffer );

            _buffer.Position = 0;
            _buffer.Limit    = count;
        }
        else
        {
            _buffer.Clear();

            _buffer.PutFloats( data.ToArray() );

            _buffer.Flip();
            _byteBuffer.Position = 0;
            _byteBuffer.Limit    = _buffer.Limit << 2;
        }

        BufferChanged();
    }

    public void UpdateInstanceData( int targetOffset, float[] data, int sourceOffset, int count )
    {
        _isDirty = true;

        if ( _isDirect )
        {
            var pos = _byteBuffer.Position;

            _byteBuffer.Position = targetOffset * 4;

            BufferUtils.Copy( data, sourceOffset, count, _byteBuffer );

            _byteBuffer.Position = pos;
        }
        else
        {
            throw new GdxRuntimeException( "Buffer must be allocated direct." ); // Should never happen
        }

        BufferChanged();
    }

    public void UpdateInstanceData( int targetOffset, FloatBuffer data, int sourceOffset, int count )
    {
        _isDirty = true;

        if ( _isDirect )
        {
            var pos = _byteBuffer.Position;

            _byteBuffer.Position = targetOffset * 4;
            data.Position        = sourceOffset * 4;

            BufferUtils.Copy( data.ToArray(), 0, count, _byteBuffer );

            _byteBuffer.Position = pos;
        }
        else
        {
            throw new GdxRuntimeException( "Buffer must be allocated direct." ); // Should never happen
        }

        BufferChanged();
    }

    /// <summary>
    /// Binds this InstanceBufferObject for rendering via glDrawArraysInstanced
    /// or glDrawElementsInstanced.
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="locations"></param>
    public void Bind( ShaderProgram shader, int[]? locations = null )
    {
        GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, ( uint )BufferHandle );

        if ( _isDirty )
        {
            unsafe
            {
                fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
                {
                    _byteBuffer.Limit = _buffer.Limit * 4;
                    GL.BufferData( ( int )BufferTarget.ArrayBuffer, _byteBuffer.Limit, ( IntPtr )ptr, _usage );
                    _isDirty = false;
                }
            }
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

                var unitOffset = +attribute.Unit;
                shader.EnableVertexAttribute( location + unitOffset );

                shader.SetVertexAttribute( location + unitOffset,
                                           attribute.NumComponents,
                                           attribute.ComponentType,
                                           attribute.Normalized,
                                           Attributes.VertexSize,
                                           attribute.Offset );

                GL.VertexAttribDivisor( ( uint )( location + unitOffset ), 1 );
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

                var unitOffset = +attribute.Unit;
                shader.EnableVertexAttribute( location + unitOffset );

                shader.SetVertexAttribute( location + unitOffset,
                                           attribute.NumComponents,
                                           attribute.ComponentType,
                                           attribute.Normalized,
                                           Attributes.VertexSize,
                                           attribute.Offset );

                GL.VertexAttribDivisor( ( uint )( location + unitOffset ), 1 );
            }
        }

        _isBound = true;
    }

    /// <summary>
    /// Unbinds this InstanceBufferObject.
    /// </summary>
    public void Unbind( ShaderProgram shader, int[]? locations = null )
    {
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

                var unitOffset = +attribute.Unit;

                shader.DisableVertexAttribute( location + unitOffset );
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

                var unitOffset = +attribute.Unit;

                shader.EnableVertexAttribute( location + unitOffset );
            }
        }

        GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 );
        _isBound = false;
    }

    /// <summary>
    /// Invalidates the InstanceBufferObject so a new OpenGL buffer handle is
    /// created. Use this in case of a context loss.
    /// </summary>
    public void Invalidate()
    {
        BufferHandle = CreateBufferObject();
        _isDirty     = true;
    }

    /// <summary>
    /// Disposes of all resources this InstanceBufferObject uses.
    /// </summary>
    public void Dispose()
    {
        GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 );
        GL.DeleteBuffers( ( uint )BufferHandle );
        BufferHandle = 0;
    }

    private int CreateBufferObject()
    {
        var result = GL.GenBuffer();

        GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, result );
        GL.BufferData( ( int )BufferTarget.ArrayBuffer, _byteBuffer.Capacity, 0, _usage );
        GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 );

        return ( int )result;
    }

    private unsafe void BufferChanged()
    {
        if ( _isBound )
        {
            fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
            {
                GL.BufferData( ( int )BufferTarget.ArrayBuffer, _byteBuffer.Limit, 0, _usage );
                GL.BufferSubData( ( int )BufferTarget.ArrayBuffer, 0, _byteBuffer.Limit, ( IntPtr )ptr );
                _isDirty = false;
            }
        }
    }
}