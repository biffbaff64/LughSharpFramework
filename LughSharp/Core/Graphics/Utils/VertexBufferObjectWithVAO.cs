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

using JetBrains.Annotations; namespace LughSharp.Core.Graphics.Utils;

/// <summary>
/// </summary>
[PublicAPI]
public class VertexBufferObjectWithVAO : IVertexData
{
    public VertexAttributes Attributes { get; set; }

    // ========================================================================

    private static readonly Buffer< int > _tmpHandle = new( 1 );

    private readonly Buffer< float > _buffer;
    private readonly Buffer< byte >  _byteBuffer;
    private readonly List< int >     _cachedLocations = [ ];
    private readonly bool            _ownsBuffer;
    private readonly int             _usage;

    private int  _bufferHandle;
    private bool _isBound = false;
    private bool _isDirty = false;
    private bool _isStatic;
    private int  _vaoHandle = -1;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Constructs a new interleaved VertexBufferObjectWithVAO.
    /// </summary>
    /// <param name="isStatic"> whether the vertex data is static. </param>
    /// <param name="numVertices"> the maximum number of vertices </param>
    /// <param name="attributes"> the <see cref="VertexAttribute"/>s. </param>
    public VertexBufferObjectWithVAO( bool isStatic, int numVertices, params VertexAttribute[] attributes )
        : this( isStatic, numVertices, new VertexAttributes( attributes ) )
    {
    }

    /// <summary>
    /// Constructs a new interleaved VertexBufferObjectWithVAO.
    /// </summary>
    /// <param name="isStatic"> whether the vertex data is static. </param>
    /// <param name="numVertices"> the maximum number of vertices </param>
    /// <param name="attributes"> the <see cref="VertexAttributes"/>. </param>
    public VertexBufferObjectWithVAO( bool isStatic, int numVertices, VertexAttributes attributes )
    {
        _isStatic   = isStatic;
        Attributes  = attributes;
        _byteBuffer = new Buffer< byte >( Attributes.VertexSize * numVertices );

        _buffer     = _byteBuffer.AsFloatBuffer();
        _ownsBuffer = true;

        _buffer.Flip();
        _byteBuffer.Flip();

        _bufferHandle = ( int )GL.GenBuffer();
        _usage        = isStatic ? IGL.GL_STATIC_DRAW : IGL.GL_DYNAMIC_DRAW;

        CreateVAO();
    }

    /// <summary>
    /// Constructs a new interleaved VertexBufferObjectWithVAO.
    /// </summary>
    /// <param name="isStatic">Indicates whether the vertex data is static.</param>
    /// <param name="unmanagedBuffer">The unmanaged byte buffer to store vertex data.</param>
    /// <param name="attributes">The vertex attributes associated with this buffer.</param>
    public VertexBufferObjectWithVAO( bool isStatic, Buffer< byte > unmanagedBuffer, VertexAttributes attributes )
    {
        _isStatic   = isStatic;
        Attributes  = attributes;
        _byteBuffer = unmanagedBuffer;
        _ownsBuffer = false;
        _buffer     = _byteBuffer.AsFloatBuffer();

        _buffer.Flip();
        _byteBuffer.Flip();

        _bufferHandle = ( int )GL.GenBuffer();
        _usage        = isStatic ? IGL.GL_STATIC_DRAW : IGL.GL_DYNAMIC_DRAW;

        CreateVAO();
    }

    /// <summary>
    /// Gets the number of vertices contained in the vertex buffer.
    /// If the vertex attributes have no associated size, a warning is logged, and the value will be 0.
    /// Otherwise, the number of vertices is calculated based on the buffer's limit
    /// and the size of each vertex.
    /// </summary>
    public int NumVertices
    {
        get
        {
            if ( Attributes.VertexSize == 0 )
            {
                Logger.Error( "WARNING: VertexData has no attributes!" );

                return 0;
            }

            return ( _buffer.Limit * 4 ) / Attributes.VertexSize;
        }
    }

    /// <summary>
    /// Gets the maximum number of vertices that can be stored in the vertex buffer.
    /// If the vertex attributes have no defined size, a warning is logged, and the returned value will be 0.
    /// Otherwise, the maximum number of vertices is determined by dividing the buffer's capacity
    /// by the size of each vertex.
    /// </summary>
    public int NumMaxVertices
    {
        get
        {
            if ( Attributes.VertexSize == 0 )
            {
                Logger.Error( "WARNING: VertexData has no attributes!" );

                return 0;
            }

            return _byteBuffer.Capacity / Attributes.VertexSize;
        }
    }

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

        _byteBuffer.Limit = Math.Min( _byteBuffer.Capacity, count << 2 );
        _byteBuffer.PutFloats( vertices, offset, count );

        _buffer.Position = 0;
        _buffer.Limit    = count;

        BufferChanged();
    }

    /// <summary>
    /// Update (a portion of) the vertices. Does not resize the backing buffer.
    /// </summary>
    /// <param name="targetOffset"></param>
    /// <param name="vertices"> the vertex data </param>
    /// <param name="sourceOffset"> the offset to start copying the data from </param>
    /// <param name="count"> the number of floats to copy  </param>
    public void UpdateVertices( int targetOffset, float[] vertices, int sourceOffset, int count )
    {
        _isDirty = true;
        var pos = _byteBuffer.Position;

        _byteBuffer.Position = targetOffset * 4;

        BufferUtils.Copy( vertices, sourceOffset, count, _byteBuffer );

        _byteBuffer.Position = pos;
        _buffer.Position     = 0;

        BufferChanged();
    }

    /// <summary>
    /// <para>
    /// Returns the underlying Buffer and marks it as dirty, causing the buffer contents to be
    /// uploaded on the next call to <see cref="Bind"/>.
    /// </para>
    /// <para>
    /// If you need immediate uploading use <see cref="IVertexData.SetVertices"/>.
    /// </para>
    /// <para>
    /// Any modifications made to the Buffer after the call to <see cref="Bind"/> will not automatically
    /// be uploaded.
    /// </para>
    /// </summary>
    /// <returns> the underlying Buffer holding the vertex data.  </returns>
    public Buffer< float > GetBuffer( bool forWriting )
    {
        _isDirty |= forWriting;

        return _buffer;
    }

    /// <summary>
    /// Binds this VertexData for rendering via glDrawArrays or glDrawElements.
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="locations"> array containing the attribute locations. </param>
    public void Bind( ShaderProgram shader, int[]? locations = null )
    {
        GL.BindVertexArray( ( uint )_vaoHandle );

        BindAttributes( shader, locations );
        BindData();

        _isBound = true;
    }

    /// <summary>
    /// Unbinds this VertexData.
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="locations"> array containing the attribute locations.  </param>
    public void Unbind( ShaderProgram? shader, int[]? locations = null )
    {
        GL.BindVertexArray( 0 );
        _isBound = false;
    }

    /// <summary>
    /// Invalidates the VertexData if applicable. Use this in case of a context loss.
    /// </summary>
    public void Invalidate()
    {
        _bufferHandle = ( int )GL.GenBuffer(); //TODO: ???

        CreateVAO();

        _isDirty = true;
    }

    private unsafe void BufferChanged()
    {
        // Bind the buffer regardless of _isBound to ensure the upload happens
        GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, ( uint )_bufferHandle );

        fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
        {
            // Use BufferSubData if you already allocated memory in Initialise, 
            // otherwise BufferData is fine for now.
            GL.BufferData( ( int )BufferTarget.ArrayBuffer, _byteBuffer.Limit, ( IntPtr )ptr, _usage );
        }

        _isDirty = false;
    }

    private void BindAttributes( ShaderProgram shader, int[]? locations )
    {
        var stillValid    = _cachedLocations.Count != 0;
        var numAttributes = Attributes.Size;

        if ( stillValid )
        {
            if ( locations == null )
            {
                for ( var i = 0; stillValid && ( i < numAttributes ); i++ )
                {
                    var attribute = Attributes.Get( i );
                    var location  = shader.GetAttributeLocation( attribute.Alias );

                    stillValid = location == _cachedLocations[ i ];
                }
            }
            else
            {
                stillValid = locations.Length == _cachedLocations.Count;

                for ( var i = 0; stillValid && ( i < numAttributes ); i++ )
                {
                    stillValid = locations[ i ] == _cachedLocations[ i ];
                }
            }
        }

        if ( !stillValid )
        {
            GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, ( uint )_bufferHandle );

            UnbindAttributes( shader );

            _cachedLocations.Clear();

            for ( var i = 0; i < numAttributes; i++ )
            {
                var attribute = Attributes.Get( i );

                _cachedLocations.Add( locations == null
                                          ? shader.GetAttributeLocation( attribute.Alias )
                                          : locations[ i ] );

                var location = _cachedLocations[ i ];

                if ( location < 0 )
                {
                    continue;
                }

                shader.EnableVertexAttribute( location );

                shader.SetVertexAttribute( location,
                                           attribute.NumComponents,
                                           attribute.ComponentType,
                                           attribute.Normalized,
                                           Attributes.VertexSize,
                                           attribute.Offset );
            }
        }
    }

    private void UnbindAttributes( ShaderProgram shaderProgram )
    {
        if ( _cachedLocations.Count == 0 )
        {
            return;
        }

        var numAttributes = Attributes.Size;

        for ( var i = 0; i < numAttributes; i++ )
        {
            var location = _cachedLocations[ i ];

            if ( location < 0 )
            {
                continue;
            }

            shaderProgram.DisableVertexAttribute( location );
        }
    }

    private unsafe void BindData()
    {
        if ( _isDirty )
        {
            GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, ( uint )_bufferHandle );

            _byteBuffer.Limit = _buffer.Limit * 4;

            fixed ( void* ptr = &_byteBuffer.BackingArray()[ 0 ] )
            {
                GL.BufferData( ( int )BufferTarget.ArrayBuffer, _byteBuffer.Limit, ( IntPtr )ptr, _usage );
            }

            _isDirty = false;
        }
    }

    private void CreateVAO()
    {
        _vaoHandle = ( int )GL.GenVertexArray();
    }

    private unsafe void DeleteVAO()
    {
        if ( _vaoHandle != -1 )
        {
            _tmpHandle.Clear();
            _tmpHandle.PutInt( _vaoHandle );
            _tmpHandle.Flip();

            fixed ( int* intptr = &_tmpHandle.ToArray()[ 0 ] )
            {
                GL.DeleteVertexArrays( 1, ( uint* )intptr );
            }

            _vaoHandle = -1;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            GL.BindBuffer( ( int )BufferTarget.ArrayBuffer, 0 );
            GL.DeleteBuffers( ( uint )_bufferHandle );

            _bufferHandle = 0;

            if ( _ownsBuffer )
            {
                _byteBuffer.Dispose();
            }

            DeleteVAO();
        }
    }
}