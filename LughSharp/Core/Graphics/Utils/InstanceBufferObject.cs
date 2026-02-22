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

using System.Diagnostics;
using JetBrains.Annotations;
using LughSharp.Core.Graphics.OpenGL;
using LughSharp.Core.Graphics.OpenGL.Bindings;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Graphics.Shaders;
using LughSharp.Core.Main;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics.Utils;

[PublicAPI]
public class InstanceBufferObject : IInstanceData
{
    // ========================================================================

    private Buffer< float > _buffer = null!;
    private Buffer< byte >? _byteBuffer;

    private int  _bufferHandle;
    private bool _isBound;
    private bool _isDirty;
    private bool _ownsBuffer;

    // ========================================================================

    public InstanceBufferObject( bool isStatic, int numVertices, params VertexAttribute[] attributes )
        : this( isStatic, numVertices, new VertexAttributes( attributes ) )
    {
    }

    public InstanceBufferObject( bool isStatic, int numVertices, VertexAttributes instanceAttributes )
    {
        _bufferHandle = ( int )Engine.GL.GenBuffer();

        var data = new Buffer< byte >( instanceAttributes.VertexSize * numVertices )
        {
            Limit = 0,
        };

        SetBuffer( data, true, instanceAttributes );

        Usage = isStatic ? BufferUsageHint.StaticDraw : BufferUsageHint.DynamicDraw;
    }

    /// <summary>
    /// The GL enum used in the call to <see cref="GLBindings.BufferData"/>,
    /// e.g. GL_STATIC_DRAW or GL_DYNAMIC_DRAW. It can only be called when the VBO is not bound.
    /// </summary>
    public BufferUsageHint Usage
    {
        get;
        set
        {
            if ( _isBound )
            {
                throw new RuntimeException( "Cannot change _usage while VBO is bound" );
            }

            field = value;
        }
    }

    public VertexAttributes Attributes { get; set; } = null!;

    public int NumInstances    => ( _buffer.Limit * 4 ) / Attributes.VertexSize;
    public int NumMaxInstances => _byteBuffer!.Capacity / Attributes.VertexSize;

    public Buffer< float > GetBuffer( bool forWriting = true )
    {
        _isDirty |= forWriting;

        return _buffer;
    }

    public void SetInstanceData( float[] data, int offset, int count )
    {
        Debug.Assert( _byteBuffer != null, "SetInstanceData(float[], int, int) fail: _byteBuffer is NULL" );

        _isDirty = true;

        _byteBuffer.PutFloats( data, offset, count );

        _buffer.Position = 0;
        _buffer.Limit    = count;

        BufferChanged();
    }

    public void SetInstanceData( Buffer< float > data, int count )
    {
        Debug.Assert( _byteBuffer != null, "SetInstanceData(Buffer< float >, int) fail: _byteBuffer is NULL" );

        _isDirty = true;

        BufferUtils.Copy( data.ToArray(), 0, count, _byteBuffer );

        _buffer.Position = 0;
        _buffer.Limit    = count;

        BufferChanged();
    }

    public void UpdateInstanceData( int targetOffset, float[] data, int sourceOffset, int count )
    {
        if ( _byteBuffer == null )
        {
            throw new RuntimeException( "_byteBuffer cannot be null" );
        }

        _isDirty = true;

        var pos = _byteBuffer.Position;

        _byteBuffer.Position = targetOffset * 4;

        BufferUtils.Copy( data, sourceOffset, count, _byteBuffer );

        _byteBuffer.Position = pos;
        _buffer.Position     = 0;

        BufferChanged();
    }

    public void UpdateInstanceData( int targetOffset, Buffer< float > data, int sourceOffset, int count )
    {
        Guard.Against.Null( _byteBuffer );

        _isDirty = true;

        var pos = _byteBuffer.Position;

        _byteBuffer.Position = targetOffset * 4;
        data.Position        = sourceOffset * 4;

        BufferUtils.Copy( data.ToArray(), 0, count, _byteBuffer );

        _byteBuffer.Position = pos;
        _buffer.Position     = 0;

        BufferChanged();
    }

    /// <summary>
    /// Binds this InstanceBufferObject for rendering via GLDrawArraysInstanced
    /// or GLDrawElementsInstanced
    /// </summary>
    public unsafe void Bind( ShaderProgram shader, int[]? locations = null )
    {
        Debug.Assert( _byteBuffer != null, "Bind(ShaderProgram, int[]) fail: _byteBuffer is NULL" );

        Engine.GL.BindBuffer( BufferTarget.ArrayBuffer, ( uint )_bufferHandle );

        if ( _isDirty )
        {
            _byteBuffer.Limit = _buffer.Limit * 4;

            fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
            {
                Engine.GL.BufferData( BufferTarget.ArrayBuffer, _byteBuffer.Limit, ( IntPtr )ptr, Usage );
            }

            _isDirty = false;
        }

        var numAttributes = Attributes.Size;

        if ( locations == null )
        {
            for ( var i = 0; i < numAttributes; i++ )
            {
                var attribute = Attributes.Get( i );

                var location = shader.GetAttributeLocation( attribute.Alias );

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

                Engine.GL.VertexAttribDivisor( ( uint )( location + unitOffset ), 1 );
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
                shader.DisableVertexAttribute( location + unitOffset );
            }
        }

        Engine.GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
        _isBound = false;
    }

    /// <summary>
    /// Invalidates the InstanceBufferObject so a new OpenGL _buffer handle
    /// is created. Use this in case of a context loss.
    /// </summary>
    public void Invalidate()
    {
        _bufferHandle = ( int )Engine.GL.GenBuffer(); //TODO: ???
        _isDirty      = true;
    }

    /// <summary>
    /// Disposes of all resources this InstanceBufferObject uses.
    /// </summary>
    public void Dispose()
    {
        Engine.GL.BindBuffer( BufferTarget.ArrayBuffer, 0 );
        Engine.GL.DeleteBuffers( ( uint )_bufferHandle );

        _bufferHandle = 0;

        if ( _ownsBuffer && ( _byteBuffer != null ) )
        {
            _byteBuffer.Dispose();
        }
    }

    /// <summary>
    /// Low level method to reset the _buffer and _attributes to
    /// the specified values. Use with care!
    /// </summary>
    protected void SetBuffer( Buffer< byte >? data, bool ownsBuffer, VertexAttributes value )
    {
        if ( _isBound )
        {
            throw new RuntimeException( "Cannot change _attributes while VBO is bound" );
        }

        if ( _ownsBuffer && ( _byteBuffer != null ) )
        {
            _byteBuffer.Dispose();
        }

        Attributes = value;

        if ( data != null )
        {
            _byteBuffer = data;
        }
        else
        {
            throw new RuntimeException( "Only Buffer< byte > is currently supported" );
        }

        _ownsBuffer = ownsBuffer;

        var lim = _byteBuffer.Limit;

        _byteBuffer.Limit = _byteBuffer.Capacity;
        _buffer           = _byteBuffer.AsFloatBuffer();
        _byteBuffer.Limit = lim;
        _buffer.Limit     = lim / 4;
    }

    private unsafe void BufferChanged()
    {
        if ( _byteBuffer == null )
        {
            throw new RuntimeException( "NULL _byteBuffer not allowed" );
        }

        if ( _isBound )
        {
            fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
            {
                Engine.GL.BufferData( BufferTarget.ArrayBuffer, _byteBuffer.Limit, 0, Usage );
                Engine.GL.BufferData( BufferTarget.ArrayBuffer, _byteBuffer.Limit, ( IntPtr )ptr, Usage );
                _isDirty = false;
            }
        }
    }
}