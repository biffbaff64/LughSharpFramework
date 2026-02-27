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

using JetBrains.Annotations;

using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Graphics.Shaders;
using LughSharp.Core.Main;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics.Utils;

/// <summary>
/// Modification of the <see cref="VertexBufferObjectSubData"/> class.
/// Sets the glVertexAttribDivisor for every <see cref="VertexAttribute"/>
/// automatically.
/// </summary>
[PublicAPI]
public class InstanceBufferObjectSubData : IInstanceData
{
    public int              BufferHandle { get; set; }
    public VertexAttributes Attributes   { get; set; }

    // ========================================================================

    private readonly Buffer< float > _buffer;
    private readonly Buffer< byte >  _byteBuffer;
    private readonly bool            _isDirect;
    private readonly BufferUsageHint _usage;

    private bool _isBound;
    private bool _isDirty;
    private bool _isStatic;

    // ========================================================================

    /// <summary>
    /// Constructs a new interleaved InstanceBufferObject.
    /// </summary>
    /// <param name="isStatic"> whether the vertex data is static. </param>
    /// <param name="numInstances"> the maximum number of vertices. </param>
    /// <param name="instanceAttributes"> the <see cref="VertexAttributes"/>. </param>
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
    /// <param name="instanceAttributes"> the <see cref="VertexAttributes"/>. </param>
    public InstanceBufferObjectSubData( bool isStatic, int numInstances, VertexAttributes instanceAttributes )
    {
        _isStatic   = isStatic;
        Attributes  = instanceAttributes;
        _byteBuffer = new Buffer< byte >( Attributes.VertexSize * numInstances );
        _isDirect   = true;

        _usage       = isStatic ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw;
        _buffer      = _byteBuffer.AsFloatBuffer();
        BufferHandle = CreateBufferObject();

        _buffer.Flip();
        _byteBuffer.Flip();
    }

    /// <summary>
    /// Returns the number of instances in this buffer.
    /// </summary>
    public int NumInstances => _buffer.Limit * 4 / Attributes.VertexSize;

    /// <summary>
    /// Returns the max number of instances in this buffer.
    /// </summary>
    public int NumMaxInstances => _byteBuffer.Capacity / Attributes.VertexSize;

    public Buffer< float > GetBuffer( bool forWriting )
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

    public void SetInstanceData( Buffer< float > data, int count )
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
            int pos = _byteBuffer.Position;

            _byteBuffer.Position = targetOffset * 4;

            BufferUtils.Copy( data, sourceOffset, count, _byteBuffer );

            _byteBuffer.Position = pos;
        }
        else
        {
            throw new RuntimeException( "Buffer must be allocated direct." ); // Should never happen
        }

        BufferChanged();
    }

    public void UpdateInstanceData( int targetOffset, Buffer< float > data, int sourceOffset, int count )
    {
        _isDirty = true;

        if ( _isDirect )
        {
            int pos = _byteBuffer.Position;

            _byteBuffer.Position = targetOffset * 4;
            data.Position        = sourceOffset * 4;

            BufferUtils.Copy( data.ToArray(), 0, count, _byteBuffer );

            _byteBuffer.Position = pos;
        }
        else
        {
            throw new RuntimeException( "Buffer must be allocated direct." ); // Should never happen
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
        Engine.GL.BindBuffer( BufferTarget.ArrayBuffer, ( uint )BufferHandle );

        if ( _isDirty )
        {
            unsafe
            {
                fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
                {
                    _byteBuffer.Limit = _buffer.Limit * 4;
                    Engine.GL.BufferData( BufferTarget.ArrayBuffer, _byteBuffer.Limit, ( IntPtr )ptr, _usage );
                    _isDirty = false;
                }
            }
        }

        int numAttributes = Attributes.Size;

        if ( locations == null )
        {
            for ( var i = 0; i < numAttributes; i++ )
            {
                VertexAttribute attribute = Attributes.Get( i );
                int             location  = shader.GetAttributeLocation( attribute.Alias );

                if ( location < 0 )
                {
                    continue;
                }

                int unitOffset = +attribute.Unit;
                shader.EnableVertexAttribute( location + unitOffset );

                shader.SetVertexAttribute( location + unitOffset,
                                           attribute.NumComponents,
                                           attribute.ComponentType,
                                           attribute.Normalized,
                                           Attributes.VertexSize,
                                           attribute.Offset );

                Engine.GL.VertexAttribDivisor( ( uint )( location + unitOffset ), 1 );
            }
        }
        else
        {
            for ( var i = 0; i < numAttributes; i++ )
            {
                VertexAttribute attribute = Attributes.Get( i );
                int             location  = locations[ i ];

                if ( location < 0 )
                {
                    continue;
                }

                int unitOffset = +attribute.Unit;
                shader.EnableVertexAttribute( location + unitOffset );

                shader.SetVertexAttribute( location + unitOffset,
                                           attribute.NumComponents,
                                           attribute.ComponentType,
                                           attribute.Normalized,
                                           Attributes.VertexSize,
                                           attribute.Offset );

                Engine.GL.VertexAttribDivisor( ( uint )( location + unitOffset ), 1 );
            }
        }

        _isBound = true;
    }

    /// <summary>
    /// Unbinds this InstanceBufferObject.
    /// </summary>
    public void Unbind( ShaderProgram shader, int[]? locations = null )
    {
        int numAttributes = Attributes.Size;

        if ( locations == null )
        {
            for ( var i = 0; i < numAttributes; i++ )
            {
                VertexAttribute attribute = Attributes.Get( i );
                int             location  = shader.GetAttributeLocation( attribute.Alias );

                if ( location < 0 )
                {
                    continue;
                }

                int unitOffset = +attribute.Unit;

                shader.DisableVertexAttribute( location + unitOffset );
            }
        }
        else
        {
            for ( var i = 0; i < numAttributes; i++ )
            {
                VertexAttribute attribute = Attributes.Get( i );
                int             location  = locations[ i ];

                if ( location < 0 )
                {
                    continue;
                }

                int unitOffset = +attribute.Unit;

                shader.EnableVertexAttribute( location + unitOffset );
            }
        }

        Engine.GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
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
        Engine.GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
        Engine.GL.DeleteBuffers( ( uint )BufferHandle );
        BufferHandle = 0;
    }

    private int CreateBufferObject()
    {
        uint result = Engine.GL.GenBuffer();

        Engine.GL.BindBuffer( BufferTarget.ArrayBuffer, result );
        Engine.GL.BufferData( BufferTarget.ArrayBuffer, _byteBuffer.Capacity, 0, _usage );
        Engine.GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );

        return ( int )result;
    }

    private unsafe void BufferChanged()
    {
        if ( _isBound )
        {
            fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
            {
                Engine.GL.BufferData( BufferTarget.ArrayBuffer, _byteBuffer.Limit, 0, _usage );
                Engine.GL.BufferSubData( ( int )BufferTarget.ArrayBuffer, 0, _byteBuffer.Limit, ( IntPtr )ptr );
                _isDirty = false;
            }
        }
    }
}

// ============================================================================
// ============================================================================