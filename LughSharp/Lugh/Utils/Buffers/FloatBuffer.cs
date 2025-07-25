﻿// /////////////////////////////////////////////////////////////////////////////
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

namespace LughSharp.Lugh.Utils.Buffers;

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

    /// <summary>
    /// Creates a new IntBuffer that is a view of the given byte array.
    /// This constructor is intended for creating buffer views (e.g., using ByteBuffer.AsIntBuffer()).
    /// It shares the provided byte array; data is NOT copied.
    /// </summary>
    /// <param name="backingArray">The byte array to use as the backing store.</param>
    /// <param name="offset">The starting offset within the byte array (in bytes).</param>
    /// <param name="capacityInFloats">The capacity of the IntBuffer in ints.</param>
    /// <param name="isBigEndian">True if big-endian byte order, false for little-endian.</param>
    internal FloatBuffer( byte[] backingArray, int offset, int capacityInFloats, bool isBigEndian )
    {
        ArgumentNullException.ThrowIfNull( backingArray );

        if ( ( offset < 0 ) || ( capacityInFloats < 0 ) )
        {
            throw new GdxRuntimeException( "Offset and capacity must be non-negative." );
        }

        if ( ( offset + ( capacityInFloats * sizeof( float ) ) ) > backingArray.Length )
        {
            throw new GdxRuntimeException( "Capacity and offset exceed backing array bounds." );
        }

        // Create ByteBuffer delegate with Memory<byte> slice
        _byteBufferDelegate = new ByteBuffer( backingArray.AsMemory( offset, capacityInFloats * sizeof( float ) ),
                                              isBigEndian );

        Capacity    = capacityInFloats;
        Length      = 0;
        Limit       = capacityInFloats;
        IsBigEndian = isBigEndian;
    }

    // ========================================================================

    /// <inheritdoc cref="ByteBuffer.GetFloat()" />
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

    /// <inheritdoc cref="ByteBuffer.GetFloat(int)" />
    public float GetFloat( int index )
    {
        var byteOffset = index * sizeof( float );

        return _byteBufferDelegate.GetFloat( byteOffset );
    }

    /// <inheritdoc cref="ByteBuffer.PutFloat(float)" />
    public void PutFloat( float value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( Position + sizeof( float ) );

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

    /// <inheritdoc cref="ByteBuffer.PutFloat(int,float)" />
    public void PutFloat( int index, float value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( index + sizeof( float ) );

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

    // ----- Bulk Get/Put operations -----

    /// <summary>
    /// Fills the provided array with float values from the buffer.
    /// </summary>
    /// <param name="floatArray">
    /// The float array to be filled with values from the buffer. The array length should be
    /// less than or equal to the remaining number of floats in the buffer to avoid buffer
    /// underflow.
    /// </param>
    public void GetFloats( float[] floatArray )
    {
        _byteBufferDelegate.GetFloats( floatArray );
    }

    /// <summary>
    /// Retrieves a sequence of float values from the buffer and stores them into the specified array.
    /// </summary>
    /// <param name="dst">
    /// The destination array where the float values will be stored.
    /// </param>
    /// <param name="dstOffset">
    /// The offset within the destination array at which to begin storing the float values.
    /// </param>
    /// <param name="length">
    /// The number of float values to retrieve and store in the destination array.
    /// </param>
    public void GetFloats( float[] dst, int dstOffset, int length )
    {
        _byteBufferDelegate.GetFloats( dst, dstOffset, length );
    }

    /// <summary>
    /// Adds the contents of the provided float array to this buffer, staring at
    /// index <see cref="Buffer.Position" />
    /// </summary>
    public void PutFloats( float[] floatArray )
    {
        _byteBufferDelegate.PutFloats( floatArray );
    }

    /// <summary>
    /// Copies a sequence of floats from the specified source array into the buffer at
    /// the current position.
    /// </summary>
    /// <param name="src">
    /// The source array containing the floats to be copied.
    /// </param>
    /// <param name="srcOffset">
    /// The starting position in the source array from which floats will be copied.
    /// </param>
    /// <param name="length">
    /// The number of floats to copy from the source array.
    /// </param>
    public void PutFloats( float[] src, int srcOffset, int length )
    {
        _byteBufferDelegate.PutFloats( src, srcOffset, length );
    }

    // ========================================================================

    /// <summary>
    /// Returns the backing array as a byte[].
    /// </summary>
    /// <returns></returns>
    public new float[] ToArray()
    {
        var tmpArray = new float[ Length ];

        _byteBufferDelegate.GetFloats( tmpArray );

        return tmpArray;
    }

    // ========================================================================

    /// <inheritdoc cref="ByteBuffer.Resize(int)" />
    public override void Resize( int extraCapacityInBytes )
    {
        _byteBufferDelegate.Resize( extraCapacityInBytes );
        Capacity = ( int )Math.Ceiling( ( double )_byteBufferDelegate.Capacity / sizeof( float ) );
    }

    /// <inheritdoc cref="ByteBuffer.Clear()" />
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

    /// <inheritdoc />
    public override void Compact()
    {
        _byteBufferDelegate.Compact();
    }

    // ========================================================================

    /// <inheritdoc />
    public override byte GetByte()
    {
        return _byteBufferDelegate.GetByte();
    }

    /// <inheritdoc />
    public override byte GetByte( int index )
    {
        return _byteBufferDelegate.GetByte( index );
    }

    /// <inheritdoc />
    public override void PutByte( byte value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        _byteBufferDelegate.PutByte( value );
    }

    /// <inheritdoc />
    public override void PutByte( int index, byte value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

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