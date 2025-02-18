// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
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

using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Utils.Buffers.NewBuffers;

/// <summary>
/// Provides a type-safe view of an underlying ByteBuffer, specialized float values.
/// This buffer holds a reference to a ByteBuffer instance (_byteBuffer), and does
/// not have its own backing arrays.
/// </summary>
[PublicAPI]
public class FloatBuffer : Buffer, IDisposable
{
    private ByteBuffer _byteBufferDelegate;

    // ========================================================================

    /// <summary>
    /// Creates a new FloatBuffer with the specified capacity.
    /// </summary>
    /// <param name="capacityInFloats">
    /// The number of floats to be made available in the buffer. As the backing buffer is a
    /// ByteBuffer, this capacity will need to be translated into bytes from floats.
    /// </param>
    public FloatBuffer( int capacityInFloats ) : base( capacityInFloats )
    {
        _byteBufferDelegate = new ByteBuffer( capacityInFloats * sizeof( float ) );
        Capacity            = capacityInFloats;
        Length              = 0;
        Limit               = capacityInFloats;
    }

    // ========================================================================

    /// <inheritdoc cref="ByteBuffer.GetFloat()"/>
    public float GetFloat()
    {
        var byteOffset = Position;

        if ( ( ( byteOffset + sizeof( float ) ) > Limit ) || ( byteOffset < 0 ) )
        {
            throw new IndexOutOfRangeException( "FloatBuffer position out of range" );
        }

        var value = _byteBufferDelegate.GetFloat( byteOffset );

        Position += sizeof( float );

        return value;
    }

    /// <inheritdoc cref="ByteBuffer.GetFloat(int)"/>
    public float GetFloat( int index )
    {
        var byteOffset = index * sizeof( float );

        return _byteBufferDelegate.GetFloat( byteOffset );
    }

    /// <inheritdoc cref="ByteBuffer.PutFloat(float)"/>
    public void PutFloat( float value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        var byteOffset = Position;

        if ( ( byteOffset + sizeof( float ) ) > Capacity )
        {
            throw new BufferOverflowException( "FloatBuffer overflow (ByteBuffer capacity reached)" );
        }

        _byteBufferDelegate.PutFloat( byteOffset, value );
        Position += sizeof( float );

        var floatIndex = Position / sizeof( float );

        if ( floatIndex > Length )
        {
            Length = floatIndex;
        }
    }

    /// <inheritdoc cref="ByteBuffer.PutFloat(int,float)"/>
    public void PutFloat( int index, float value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        var byteOffset = index * sizeof( float );

        if ( ( index < 0 ) || ( index >= Capacity ) )
        {
            throw new IndexOutOfRangeException();
        }

        _byteBufferDelegate.PutFloat( byteOffset, value );

        if ( index > Length )
        {
            Length = index + 1;
        }
    }

    // ========================================================================

    /// <inheritdoc cref="ByteBuffer.Resize(int)"/>
    public override void Resize( int extraCapacityInBytes )
    {
        _byteBufferDelegate.Resize( extraCapacityInBytes );
        Capacity = (int)Math.Ceiling( (double)_byteBufferDelegate.Capacity / sizeof(float) );
    }

    /// <inheritdoc cref="ByteBuffer.Clear()"/>
    public override void Clear()
    {
        _byteBufferDelegate.Clear(); // Delegate to ByteBuffer's Clear implementation
        Length   = 0;                // Reset IntBuffer Length
        Position = 0;                // Reset IntBuffer Position
        Limit    = Capacity;         // Reset IntBuffer Limit
    }

    /// <inheritdoc />
    public override int Remaining()
    {
        return ( Limit - Position ) / sizeof( float );
    }

    // ========================================================================

    /// <inheritdoc />
    public override byte GetByte() => _byteBufferDelegate.GetByte();

    /// <inheritdoc />
    public override byte GetByte( int index ) => _byteBufferDelegate.GetByte( index );

    /// <inheritdoc />
    public override void PutByte( byte value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        _byteBufferDelegate.PutByte( value );
    }

    /// <inheritdoc />
    public override void PutByte( int index, byte value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        _byteBufferDelegate.PutByte( index, value );
    }

    // ----- Bulk Get/Put operations -----

    /// <inheritdoc />
    public override void GetBytes( byte[] result, int offset, int length )
    {
        _byteBufferDelegate.GetBytes( result, offset, length );
    }

    /// <inheritdoc />
    public override void GetBytes( byte[] byteArray )
    {
        _byteBufferDelegate.GetBytes( byteArray );
    }

    /// <inheritdoc />
    public override void PutBytes( byte[] src, int srcOffset, int dstOffset, int length )
    {
        _byteBufferDelegate.PutBytes( src, srcOffset, dstOffset, length );
    }

    /// <inheritdoc />
    public override void PutBytes( byte[] byteArray )
    {
        _byteBufferDelegate.PutBytes( byteArray );
    }

    // ========================================================================

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or
    /// resetting unmanaged resources.
    /// </summary>
    protected override void Dispose( bool disposing )
    {
        if ( disposing )
        {
            base.Dispose( disposing );
        }
    }
}