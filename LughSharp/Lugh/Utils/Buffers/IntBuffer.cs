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

namespace LughSharp.Lugh.Utils.Buffers;

/// <summary>
/// Provides a type-safe view of an underlying ByteBuffer, specialized for Int32 values.
/// This buffer holds a reference to a ByteBuffer instance (_byteBuffer), and does
/// not have its own backing arrays.
/// </summary>
[PublicAPI]
public class IntBuffer : Buffer, IDisposable
{
    private ByteBuffer _byteBufferDelegate;

    // ========================================================================

    /// <summary>
    /// Creates a new IntBuffer with the specified capacity.
    /// </summary>
    /// <param name="capacityInInts">
    /// The number of ints to be made available in the buffer. As the backing buffer is a
    /// ByteBuffer, this capacity will need to be translated into bytes from ints.
    /// </param>
    public IntBuffer( int capacityInInts ) : base( capacityInInts )
    {
        _byteBufferDelegate = new ByteBuffer( capacityInInts * sizeof( int ) );
        Capacity            = capacityInInts;
        Length              = 0;
        Limit               = capacityInInts;
    }

    /// <summary>
    /// Creates a new IntBuffer that is a view of the given byte array.
    /// This constructor is intended for creating buffer views (e.g., using ByteBuffer.AsIntBuffer()).
    /// It shares the provided byte array; data is NOT copied.
    /// </summary>
    /// <param name="backingArray">The byte array to use as the backing store.</param>
    /// <param name="offset">The starting offset within the byte array (in bytes).</param>
    /// <param name="capacityInInts">The capacity of the IntBuffer in ints.</param>
    /// <param name="isBigEndian">True if big-endian byte order, false for little-endian.</param>
    internal IntBuffer( byte[] backingArray, int offset, int capacityInInts, bool isBigEndian )
    {
        ArgumentNullException.ThrowIfNull( backingArray );

        if ( ( offset < 0 ) || ( capacityInInts < 0 ) )
        {
            throw new GdxRuntimeException( "Offset and capacity must be non-negative." );
        }

        if ( ( offset + ( capacityInInts * sizeof( int ) ) ) > backingArray.Length )
        {
            throw new GdxRuntimeException( "Capacity and offset exceed backing array bounds." );
        }

        // Create ByteBuffer delegate with Memory<byte> slice
        _byteBufferDelegate = new ByteBuffer( backingArray.AsMemory( offset, capacityInInts * sizeof( int ) ),
                                              isBigEndian );

        Capacity    = capacityInInts;
        Length      = 0;
        Limit       = capacityInInts;
        IsBigEndian = isBigEndian;
    }

    // ========================================================================

    /// <summary>
    /// Retrieves the next Int32 value from the current position of the buffer and
    /// advances the position by the size of an Int32.
    /// </summary>
    /// <returns>
    /// The Int32 value read from the buffer at the current position.
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when the current position, adjusted by the size of an Int32, exceeds the buffer's limit
    /// or is less than zero.
    /// </exception>
    public int GetInt()
    {
        var byteOffset = Position;

        if ( ( ( byteOffset + sizeof( int ) ) > Limit ) || ( byteOffset < 0 ) )
        {
            throw new IndexOutOfRangeException( "IntBuffer position out of range" );
        }

        var value = _byteBufferDelegate.GetInt( byteOffset );

        Position += sizeof( int );

        return value;
    }

    /// <summary>
    /// Retrieves the integer value at the specified index within the buffer.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the integer to retrieve. The index is translated into the
    /// corresponding byte offset within the backing ByteBuffer.
    /// </param>
    /// <returns>
    /// The integer value located at the specified index in the buffer.
    /// </returns>
    public int GetInt( int index )
    {
        var byteOffset = index * sizeof( int );

        return _byteBufferDelegate.GetInt( byteOffset );
    }

    /// <summary>
    /// Inserts an integer value into the buffer at the current position and advances the
    /// position by the size of an integer.
    /// </summary>
    /// <param name="value">
    /// The integer value to be inserted into the buffer.
    /// </param>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the buffer is in a read-only mode and modification is attempted.
    /// </exception>
    /// <exception cref="BufferOverflowException">
    /// Thrown if the buffer does not have enough capacity to accommodate the integer value.
    /// </exception>
    public void PutInt( int value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( Position + sizeof( int ) );

        var intOffset = Position;

        if ( ( intOffset + sizeof( int ) ) > Capacity )
        {
            throw new BufferOverflowException( "IntBuffer overflow (ByteBuffer capacity reached)" );
        }

        _byteBufferDelegate.PutInt( intOffset, value );
        Position += sizeof( int );

        var intIndex = Position / sizeof( int );

        if ( intIndex > Length )
        {
            Length = intIndex;
        }
    }

    /// <summary>
    /// Inserts the specified 32-bit integer value into the buffer at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which the integer value will be placed. Must be within
    /// the current buffer capacity.
    /// </param>
    /// <param name="value">
    /// The 32-bit integer value to insert into the buffer.
    /// </param>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the buffer is read-only and a write operation is attempted.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if the specified index is outside the bounds of the buffer's current capacity.
    /// </exception>
    public void PutInt( int index, int value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( index + sizeof( int ) );

        var byteOffset = index * sizeof( int );

        if ( ( index < 0 ) || ( index >= Capacity ) )
        {
            throw new IndexOutOfRangeException();
        }

        _byteBufferDelegate.PutInt( byteOffset, value );

        if ( index > Length )
        {
            Length = index + 1;
        }
    }

    // ----- Bulk Get/Put operations -----

    /// <summary>
    /// Populates the provided array with integers from the buffer.
    /// </summary>
    /// <param name="intArray">
    /// Array that will be filled with integer values read from the buffer. The size of the array
    /// determines the number of integers retrieved.
    /// </param>
    public void GetInts( int[] intArray )
    {
        _byteBufferDelegate.GetInts( intArray );
    }

    /// <summary>
    /// Reads a sequence of integers from the buffer into the specified array starting at the given offset.
    /// </summary>
    /// <param name="dst">
    /// The destination array where the integers will be stored. This array must have sufficient
    /// capacity to accommodate the number of integers specified by the length parameter, starting from the
    /// specified offset.
    /// </param>
    /// <param name="dstOffset">
    /// The starting offset in the destination array where the integers will begin to be written.
    /// This value must be equal to or greater than zero.
    /// </param>
    /// <param name="length">
    /// The number of integers to read from the buffer. This value must be equal to or greater than zero
    /// and must not exceed the remaining capacity of the buffer.
    /// </param>
    public void GetInts( int[] dst, int dstOffset, int length )
    {
        _byteBufferDelegate.GetInts( dst, dstOffset, length );
    }

    /// <summary>
    /// Adds the contents of the provided int array to this buffer, staring at
    /// index <see cref="Buffer.Position" />
    /// </summary>
    /// <param name="intArray"></param>
    public void PutInts( int[] intArray )
    {
        _byteBufferDelegate.PutInts( intArray );
    }

    /// <summary>
    /// Writes a sequence of integers from the specified source array into the buffer,
    /// starting at the given source offset and spanning the specified number of integers.
    /// </summary>
    /// <param name="src">
    /// The source array containing the integers to be written to the buffer.
    /// </param>
    /// <param name="srcOffset">
    /// The starting position in the source array from which integers will be read.
    /// </param>
    /// <param name="length">
    /// The number of integers to transfer from the source array to the buffer.
    /// </param>
    public void PutInts( int[] src, int srcOffset, int length )
    {
        _byteBufferDelegate.PutInts( src, srcOffset, length );
    }

    // ========================================================================

    /// <summary>
    /// Returns the backing array as a int[].
    /// </summary>
    /// <returns></returns>
    public new int[] ToArray()
    {
        var tmpArray = new int[ Length ];

        _byteBufferDelegate.GetInts( tmpArray );

        return tmpArray;
    }

    // ========================================================================

    /// <inheritdoc cref="ByteBuffer.Resize(int)" />
    public override void Resize( int extraCapacityInBytes )
    {
        // **1. Delegate Resize to ByteBuffer**
        _byteBufferDelegate.Resize( extraCapacityInBytes );

        // **2. Recalculate IntBuffer Capacity in *ints* based on the resized ByteBuffer's byte capacity**
        Capacity = ( int )Math.Ceiling( ( double )_byteBufferDelegate.Capacity / sizeof( int ) );

        // **3. Adjust Limit if it was originally at Capacity** (Optional, but good practice to maintain Limit if it was at full capacity before resize)
        if ( Limit == ( Capacity -
                        ( int )Math.Ceiling( ( double )extraCapacityInBytes / sizeof( int ) ) ) ) // Check if Limit was at old Capacity
        {
            Limit = Capacity; // Reset Limit to the new Capacity if it was at the old Capacity
        }
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
        return ( Limit - Position ) / sizeof( int );
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