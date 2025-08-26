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

using LughSharp.Lugh.Utils;

namespace LughSharp.Lugh.Graphics.Utils;

[PublicAPI]
public class VertexArray : IVertexData
{
    /// <summary>
    /// Returns the <see cref="VertexAttributes" /> as specified during construction.
    /// </summary>
    public VertexAttributes Attributes { get; set; }

    // ========================================================================

    private readonly Buffer< float > _buffer;
    private readonly Buffer< byte >  _byteBuffer;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Constructs a new interleaved VertexArray
    /// </summary>
    /// <param name="numVertices"> the maximum number of vertices </param>
    /// <param name="attributes"> the <see cref="VertexAttribute" />s  </param>
    public VertexArray( int numVertices, params VertexAttribute[] attributes )
        : this( numVertices, new VertexAttributes( attributes ) )
    {
    }

    /// <summary>
    /// Constructs a new interleaved VertexArray
    /// </summary>
    /// <param name="numVertices"> the maximum number of vertices </param>
    /// <param name="attributes"> the <see cref="VertexAttributes" /> </param>
    public VertexArray( int numVertices, VertexAttributes attributes )
    {
        Attributes  = attributes;
        _byteBuffer = new Buffer< byte >( Attributes.VertexSize * numVertices );
        _buffer     = _byteBuffer.AsFloatBuffer();
    }

    /// <summary>
    /// Returns the number of vertices this VertexData stores.
    /// </summary>
    public int NumVertices => _byteBuffer.Position / Attributes.VertexSize;

    /// <summary>
    /// Returns the maximum number of vertices this VertexData can store.
    /// </summary>
    public int NumMaxVertices => _byteBuffer.Capacity / Attributes.VertexSize;

    /// <summary>
    /// Returns the underlying Buffer and marks it as dirty, causing the buffer
    /// contents to be uploaded on the next call to bind. If you need immediate
    /// uploading use <see cref="IVertexData.SetVertices" />; Any modifications made to the Buffer
    /// after* the call to bind will not automatically be uploaded.
    /// </summary>
    /// <returns> the underlying Buffer holding the vertex data.</returns>
    public Buffer< float > GetBuffer( bool forWriting )
    {
        return _buffer;
    }

    /// <summary>
    /// Sets the vertices of this VertexData, discarding the old vertex data. The
    /// count must equal the number of floats per vertex times the number of vertices
    /// to be copied to this VertexData. The order of the vertex attributes must be
    /// the same as specified at construction time via <see cref="VertexAttributes" />.
    /// <para>
    /// This can be called in between calls to bind and unbind. The vertex data will
    /// be updated instantly.
    /// </para>
    /// </summary>
    /// <param name="vertices"> the vertex data </param>
    /// <param name="offset"> the offset to start copying the data from </param>
    /// <param name="count"> the number of floats to copy  </param>
    public unsafe void SetVertices( float[] vertices, int offset, int count )
    {
        if ( _byteBuffer.Capacity < ( count * 4 ) )
        {
            throw new ArgumentException( "Buffer capacity is too small." );
        }

        fixed ( float* sourcePtr = &vertices[ offset ] )
        fixed ( byte* destPtr = _byteBuffer.BackingArray() )
        {
            Unsafe.CopyBlock( destPtr + _byteBuffer.Position, sourcePtr, ( uint )( count * sizeof( float ) ) );
            _byteBuffer.Position += count * 4;
            _buffer.Position     =  0;
            _buffer.Limit        =  count;
        }
    }

    /// <summary>
    /// Update (a portion of) the vertices. Does not resize the backing buffer.
    /// </summary>
    /// <param name="targetOffset"></param>
    /// <param name="vertices"> the vertex data </param>
    /// <param name="sourceOffset"> the offset to start copying the data from </param>
    /// <param name="count"> the number of floats to copy  </param>
    public unsafe void UpdateVertices( int targetOffset, float[] vertices, int sourceOffset, int count )
    {
        if ( _byteBuffer.Capacity < ( ( targetOffset + count ) * 4 ) )
        {
            throw new ArgumentException( "Buffer capacity is too small." );
        }

        fixed ( float* sourcePtr = &vertices[ sourceOffset ] )
        fixed ( byte* destPtr = _byteBuffer.BackingArray() )
        {
            Unsafe.CopyBlock( destPtr + ( targetOffset * sizeof( float ) ), sourcePtr, ( uint )( count * sizeof( float ) ) );
        }
    }

    /// <summary>
    /// Binds this VertexData for rendering via glDrawArrays or glDrawElements.
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="locations"> array containing the attribute locations.</param>
    public void Bind( ShaderProgram shader, int[]? locations = null )
    {
        _byteBuffer.Position = 0;
        _buffer.Position     = 0;

        var numAttributes = Attributes.Size;

        for ( var i = 0; i < numAttributes; i++ )
        {
            var attribute = Attributes.Get( i );
            var location  = locations?[ i ] ?? shader.GetAttributeLocation( attribute.Alias );

            if ( location < 0 )
            {
                continue;
            }

            GL.EnableVertexAttribArray( ( uint )location );

            var byteOffset    = attribute.Offset;
            var type          = attribute.ComponentType;
            var numComponents = attribute.NumComponents;
            var normalized    = attribute.Normalized;
            var stride        = Attributes.VertexSize;

            GL.VertexAttribPointer( ( uint )location,
                                    numComponents,
                                    type,
                                    normalized,
                                    stride,
                                    ( uint )byteOffset );
        }
    }

    /// <summary>
    /// Unbinds this VertexData.
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="locations"> array containing the attribute locations.</param>
    public void Unbind( ShaderProgram? shader, int[]? locations = null )
    {
        var numAttributes = Attributes.Size;

        for ( var i = 0; i < numAttributes; i++ )
        {
            var location = locations?[ i ] ?? shader?.GetAttributeLocation( Attributes.Get( i ).Alias ) ?? -1;

            if ( location >= 0 )
            {
                GL.DisableVertexAttribArray( ( uint )location );
            }
        }
    }

    /// <summary>
    /// Invalidates the VertexData if applicable. Use this in case of a context loss.
    /// </summary>
    public virtual void Invalidate()
    {
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing,
    /// or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize( this );
    }
}