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
/// Provides a type-safe view of an underlying ByteBuffer, specialized for short values.
/// This buffer holds a reference to a ByteBuffer instance (_byteBuffer), and does
/// not have its own backing arrays.
/// Properties <see cref="ByteBuffer.Limit" /> and <see cref="ByteBuffer.Capacity" /> are
/// delegated to the <see cref="ByteBuffer" /> class, as it is that cass which handles
/// the underlying byte buffer.
/// </summary>
[PublicAPI]
public class ShortBuffer : Buffer, IDisposable
{
    private ByteBuffer _byteBufferDelegate;

    // ========================================================================

    /// <summary>
    /// Creates a new ShortBuffer with the specified capacity.
    /// </summary>
    /// <param name="capacityInShorts">
    /// The number of shorts to be made available in the buffer. As the backing buffer is a
    /// ByteBuffer, this capacity will need to be translated into bytes from shorts.
    /// </param>
    public ShortBuffer( int capacityInShorts ) : base( capacityInShorts )
    {
        _byteBufferDelegate = new ByteBuffer( capacityInShorts * sizeof( short ) );
        Capacity            = capacityInShorts;
        Length              = 0;
        Limit               = capacityInShorts;
    }

    /// <summary>
    /// Creates a new IntBuffer that is a view of the given byte array.
    /// This constructor is intended for creating buffer views (e.g., using ByteBuffer.AsIntBuffer()).
    /// It shares the provided byte array; data is NOT copied.
    /// </summary>
    /// <param name="backingArray">The byte array to use as the backing store.</param>
    /// <param name="offset">The starting offset within the byte array (in bytes).</param>
    /// <param name="capacityInShorts">The capacity of the IntBuffer in ints.</param>
    /// <param name="isBigEndian">True if big-endian byte order, false for little-endian.</param>
    internal ShortBuffer( byte[] backingArray, int offset, int capacityInShorts, bool isBigEndian )
    {
        ArgumentNullException.ThrowIfNull( backingArray );

        if ( ( offset < 0 ) || ( capacityInShorts < 0 ) )
        {
            throw new GdxRuntimeException( "Offset and capacity must be non-negative." );
        }

        if ( ( offset + ( capacityInShorts * sizeof( short ) ) ) > backingArray.Length )
        {
            throw new GdxRuntimeException( "Capacity and offset exceed backing array bounds." );
        }

        // Create ByteBuffer delegate with Memory<byte> slice
        _byteBufferDelegate = new ByteBuffer( backingArray.AsMemory( offset, capacityInShorts * sizeof( short ) ),
                                              isBigEndian );

        Capacity    = capacityInShorts;
        Length      = 0;
        Limit       = capacityInShorts;
        IsBigEndian = isBigEndian;
    }

    // ========================================================================
    // Get methods
    // ========================================================================
    
    /// <summary>
    /// Reads a 16-bit integer (short) from the current position of the buffer and advances
    /// the position by the size of a short.
    /// </summary>
    /// <returns>The 16-bit integer value read from the buffer.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if the current position of the buffer is out of range or if there is not enough
    /// remaining capacity to read a short.
    /// </exception>
    public short GetShort()
    {
        var byteOffset = Position;

        if ( ( ( byteOffset + sizeof( short ) ) > Limit ) || ( byteOffset < 0 ) )
        {
            throw new IndexOutOfRangeException( "IntBuffer position out of range" );
        }

        var value = _byteBufferDelegate.GetShort( byteOffset );

        Position += sizeof( short );

        return value;
    }

    /// <summary>
    /// Retrieves a short value from the buffer at the specified index.
    /// </summary>
    /// <param name="index">
    /// The index position within the buffer from which to retrieve the short value.
    /// </param>
    /// <returns>
    /// The short value at the specified index in the buffer.
    /// </returns>
    public short GetShort( int index )
    {
        var byteOffset = index * sizeof( short );

        return _byteBufferDelegate.GetShort( byteOffset );
    }

    /// <summary>
    /// Reads a sequence of shorts from the buffer into the specified array.
    /// </summary>
    /// <param name="shortArray">
    /// The array into which the shorts will be written. The number of shorts read
    /// will be equal to the length of the array.
    /// </param>
    public void GetShorts( short[] shortArray )
    {
        _byteBufferDelegate.GetShorts( shortArray );
    }

    /// <summary>
    /// Reads a sequence of short values from this buffer and transfers them into the specified
    /// array starting at the given offset.
    /// </summary>
    /// <param name="dst">
    /// The destination array into which the short values from the buffer are copied.
    /// </param>
    /// <param name="dstOffset">
    /// The starting offset in the destination array where short values will be written.
    /// </param>
    /// <param name="length">
    /// The number of short values to transfer from the buffer to the destination array.
    /// </param>
    public void GetShorts( short[] dst, int dstOffset, int length )
    {
        _byteBufferDelegate.GetShorts( dst, dstOffset, length );
    }

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
    public override void GetBytes( byte[] result, int offset, int length )
    {
        _byteBufferDelegate.GetBytes( result, offset, length );
    }

    /// <inheritdoc />
    public override void GetBytes( byte[] byteArray )
    {
        _byteBufferDelegate.GetBytes( byteArray );
    }

    // ========================================================================
    // Put methods
    // ========================================================================
    
    /// <summary>
    /// Inserts a short value into the buffer at the current position, advancing the position
    /// by the size of a short.
    /// </summary>
    /// <param name="value">The short value to be inserted into the buffer.</param>
    /// <exception cref="GdxRuntimeException">Thrown if the buffer is read-only.</exception>
    /// <exception cref="BufferOverflowException">
    /// Thrown if there is not enough space remaining in the buffer to accommodate the short value.
    /// </exception>
    public void PutShort( short value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( Position + sizeof( short ) );

        var byteOffset = Position;

        if ( ( byteOffset + sizeof( short ) ) > Capacity )
        {
            throw new BufferOverflowException( "ShortBuffer overflow (ByteBuffer capacity reached)." );
        }

        _byteBufferDelegate.PutShort( byteOffset, value ); // Delegate to ByteBuffer's PutShort
        Position += sizeof( short );                       // Advance Position by size of short

        // Update ShortBuffer's Length (if write extends current Length)
        var shortIndex = Position / sizeof( short ); // Calculate short index based on new byte position

        if ( shortIndex > Length ) // Check if new short index exceeds current ShortBuffer Length
        {
            Length = shortIndex; // Update ShortBuffer Length
        }
    }

    /// <summary>
    /// Inserts a short value at the specified index in the buffer.
    /// </summary>
    /// <param name="index">
    /// The index at which the short value should be inserted. The index must be in the range
    /// of the buffer's capacity.
    /// </param>
    /// <param name="value">
    /// The short value to insert into the buffer at the specified index.
    /// </param>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the buffer is read-only.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if the specified index is outside the valid range of the buffer's capacity.
    /// </exception>
    public void PutShort( int index, short value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( ( index + 1 ) + sizeof( short ) );

        var byteOffset = index * sizeof( short );

        if ( ( index < 0 ) || ( index >= Capacity ) )
        {
            throw new IndexOutOfRangeException();
        }

        _byteBufferDelegate.PutShort( byteOffset, value );

        var shortIndex = Position / sizeof( short );

        if ( index > Length )
        {
            Length = shortIndex;
        }
    }

    /// <summary>
    /// Adds the contents of the provided short array to this buffer, staring at
    /// index <see cref="Buffer.Position" />
    /// </summary>
    public void PutShorts( short[] shortArray )
    {
        _byteBufferDelegate.PutShorts( shortArray );
    }

    /// <summary>
    /// Writes an array of short values into the buffer starting at the specified offset
    /// and for the given length.
    /// </summary>
    /// <param name="src">
    /// The source array containing the short values to be written into the buffer.
    /// </param>
    /// <param name="srcOffset">
    /// The offset in the source array at which to begin reading short values.
    /// </param>
    /// <param name="length">
    /// The number of short values to be written into the buffer.
    /// </param>
    public void PutShorts( short[] src, int srcOffset, int length )
    {
        _byteBufferDelegate.PutShorts( src, srcOffset, length );
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
    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Returns the backing array as a byte[].
    /// </summary>
    /// <returns></returns>
    public new short[] ToArray()
    {
        var tmpArray = new short[ Length ];

        _byteBufferDelegate.GetShorts( tmpArray );

        return tmpArray;
    }

    // ========================================================================

    /// <inheritdoc cref="ByteBuffer.Resize(int)" />
    public override void Resize( int extraCapacityInBytes )
    {
        Logger.Debug( $"Resize: {extraCapacityInBytes}" );

        // **1. Delegate Resize to ByteBuffer**
        _byteBufferDelegate.Resize( extraCapacityInBytes );

        // **2. Recalculate ShortBuffer Capacity in *shorts* based on the resized ByteBuffer's byte capacity**
        Capacity = ( int )Math.Ceiling( ( double )_byteBufferDelegate.Capacity / sizeof( short ) );

        // **3. Adjust Limit if it was originally at Capacity** (Optional, but good practice to maintain Limit if it was at full capacity before resize)
        if ( Limit == ( Capacity -
                        ( int )Math.Ceiling( ( double )extraCapacityInBytes / sizeof( short ) ) ) ) // Check if Limit was at old Capacity
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
        return ( Limit - Position ) / sizeof( short );
    }

    /// <inheritdoc />
    public override void Compact()
    {
        _byteBufferDelegate.Compact();
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