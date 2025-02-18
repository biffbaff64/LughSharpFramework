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

using System.Buffers.Binary;

using LughSharp.Lugh.Utils.Exceptions;

using IndexOutOfRangeException = System.IndexOutOfRangeException;

namespace LughSharp.Lugh.Utils.Buffers.NewBuffers;

[PublicAPI]
public class ByteBuffer : Buffer, IDisposable
{
    private byte[]         _backingArray;
    private Memory< byte > _memory;

    // ========================================================================

    /// <summary>
    /// Creates a new ByteBuffer with the specified capacity.
    /// </summary>
    /// <param name="capacityInBytes"> The number of bytes to be made available in the buffer. </param>
    public ByteBuffer( int capacityInBytes ) : base( capacityInBytes )
    {
        _backingArray = new byte[ capacityInBytes ];
        _memory       = _backingArray.AsMemory();
    }

    // ========================================================================
    // Byte Type handling methods
    // ========================================================================

    /// <summary>
    /// Gets the Byte from the backing array at the current <see cref="Buffer.Position"/>.
    /// </summary>
    public override byte GetByte()
    {
        if ( ( Position >= Limit ) || ( Position < 0 ) ) // Check against Limit now for sequential read
        {
            throw new IndexOutOfRangeException( "ByteBuffer position out of range or beyond limit." );
        }

        var value = _memory.Span[ Position ];

        Position++;

        return value;
    }

    /// <summary>
    /// Gets the Byte from the backing array at the specified index.
    /// </summary>
    /// <param name="index"> The buffer position from which to return the byte value. </param>
    public override byte GetByte( int index )
    {
        if ( ( index < 0 ) || ( index > Limit ) )
        {
            throw new IndexOutOfRangeException( "Buffer position out of range." );
        }

        return _memory.Span[ index ];
    }

    /// <summary>
    /// Puts the provided Byte into the backing array at the current <see cref="Buffer.Position"/>.
    /// Position is then updated.
    /// </summary>
    /// <param name="value"> The value to put. </param>
    public override void PutByte( byte value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        if ( Position >= Capacity )
        {
            throw new BufferOverflowException();
        }

        _memory.Span[ Position ] = value;
        Position++; // Increment position AFTER write
        Length = Position;
    }

    /// <summary>
    /// Puts the provided Byte into the backing array at the specified index.
    /// </summary>
    /// <param name="index"> The position in the buffer at which to put the byte value. </param>
    /// <param name="value"> The value to put. </param>
    public override void PutByte( int index, byte value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        if ( ( index < 0 ) || ( index >= Capacity ) )
        {
            throw new IndexOutOfRangeException( "Index out of range." );
        }

        _memory.Span[ index ] = value;

        if ( index > Length )
        {
            Length = index + 1;
        }
    }

    // ----- Bulk Get/Put operations -----

    /// <summary>
    /// Gets bytes from this buffer, starting at the current <see cref="Buffer.Position"/>,
    /// and puts them into the provided destination byte array 'dst'.
    /// Updates the <see cref="Buffer.Position"/> by the number of bytes read.
    /// </summary>
    /// <param name="dst">
    /// The destination array to receive the bytes. Must be large enough to hold 'dstOffset + length' bytes.
    /// </param>
    /// <param name="dstOffset"> The starting offset within the destination array to write to. </param>
    /// <param name="length"> The number of bytes to get. </param>
    /// <exception cref="IndexOutOfRangeException">
    /// If there are not enough remaining bytes in the buffer or if dstOffset and length cause overflow in dst.
    /// </exception>
    /// <exception cref="ArgumentNullException"> If <paramref name="dst"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="dstOffset"/> or <paramref name="length"/> is negative.
    /// </exception>
    public override void GetBytes( byte[] dst, int dstOffset, int length )
    {
        ArgumentNullException.ThrowIfNull( dst );

        if ( dstOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Destination offset cannot be negative." );
        }

        if ( length < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( length ), "Length cannot be negative." );
        }

        // Check if dstOffset + length exceeds dst array bounds
        if ( ( dstOffset + length ) > dst.Length )
        {
            throw new ArgumentOutOfRangeException( nameof( length ), "Length and destination offset exceed destination array bounds." );
        }

        // Check if enough bytes remaining in buffer
        if ( ( Position + length ) > Limit )
        {
            throw new IndexOutOfRangeException( "Not enough remaining bytes in buffer to read the requested length." );
        }

        // Efficient copy to dst array
        _memory.Span.Slice( Position, length ).CopyTo( dst.AsSpan( dstOffset, length ) );

        // Update Position by the number of bytes read
        Position += length;
    }

    /// <summary>
    /// Puts bytes from the source byte array 'src' into this buffer, starting at the current
    /// <see cref="Buffer.Position"/> and writing 'length' bytes. The writing starts at a
    /// destination offset within the buffer, specified by 'offset'. Also updates the
    /// <see cref="Buffer.Position"/> by the number of bytes written.
    /// </summary>
    /// <param name="src"> The source byte array to get bytes from. </param>
    /// <param name="srcOffset"> The starting offset within the source array to read from. </param>
    /// <param name="dstOffset"> The starting offset within *this buffer* (the destination) to write to. </param>
    /// <param name="length"> The number of bytes to put. </param>
    /// <exception cref="IndexOutOfRangeException">
    /// If there is not enough space remaining in the buffer or if srcOffset and length cause overflow in src.
    /// </exception>
    /// <exception cref="ArgumentNullException"> If <paramref name="src"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="srcOffset"/>, <paramref name="dstOffset"/> or <paramref name="length"/> is negative.
    /// </exception>
    public override void PutBytes( byte[] src, int srcOffset, int dstOffset, int length )
    {
        ArgumentNullException.ThrowIfNull( src );

        if ( srcOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ), "Source offset cannot be negative." );
        }

        if ( dstOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Destination offset cannot be negative." );
        }

        if ( length < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( length ), "Length cannot be negative." );
        }

        if ( ( srcOffset + length ) > src.Length )
        {
            throw new ArgumentOutOfRangeException( nameof( length ), "Length and source offset exceed source array bounds." );
        }

        // Check for space in destination buffer using dstOffset
        if ( ( dstOffset + length ) > Capacity )
        {
            throw new IndexOutOfRangeException( "Not enough space in buffer to put the requested length at the given destination offset." );
        }

        // Copy from src to buffer at dstOffset
        src.AsSpan( srcOffset, length ).CopyTo( _memory.Span.Slice( dstOffset, length ) );

        if ( ( dstOffset + length ) > Position )
        {
            Position = dstOffset + length;
        }
    }
    
    /// <summary>
    /// Transfers the entire contents of this buffer into the provided destination
    /// array. An invocation of this method with the destination byte array
    /// <paramref name="dst"/> behaves in exactly the same way as invoking
    /// <c>dst.GetBytes(src, 0, src.Length)</c>.
    /// </summary>
    /// <param name="dst"> The destination byte array. </param>
    /// <exception cref="ArgumentNullException"> If <paramref name="dst"/> is null.</exception>
    public override void GetBytes( byte[] dst )
    {
        GetBytes( dst, 0, this.Length );
    }

    /// <summary>
    /// Transfers the entire content of the given source byte array into this buffer.
    /// An invocation of this method with the source byte array <paramref name="src"/>
    /// behaves in exactly the same way as invoking <c>dst.PutBytes(src, 0, src.Length)</c>.
    /// </summary>
    /// <param name="src">The source byte array.</param>
    /// <exception cref="ArgumentNullException"> If <paramref name="src"/> is null.</exception>
    public override void PutBytes( byte[] src )
    {
        PutBytes( src, 0, 0, src.Length );
    }

    // ========================================================================
    // Short Type handling methods
    // ========================================================================

    /// <summary>
    /// Gets a Short value from the backing array at the current <see cref="Buffer.Position"/>.
    /// </summary>
    public short GetShort()
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadInt16BigEndian( _memory.Span.Slice( Position ) )
            : BinaryPrimitives.ReadInt16LittleEndian( _memory.Span.Slice( Position ) );
    }

    /// <summary>
    /// Gets a Short value from the backing array at the specified index.
    /// </summary>
    public short GetShort( int index )
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadInt16BigEndian( _memory.Span.Slice( index ) )
            : BinaryPrimitives.ReadInt16LittleEndian( _memory.Span.Slice( index ) );
    }

    /// <summary>
    /// Puts the provided Short value into the backing array at the current <see cref="Buffer.Position"/>.
    /// </summary>
    /// <param name="value"> The value to put. </param>
    public void PutShort( short value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteInt16BigEndian( _memory.Span.Slice( Position ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian( _memory.Span.Slice( Position ), value );
        }

        Position += sizeof( short );
        Length   =  Position;
    }

    /// <summary>
    /// Puts the provided Short value into the backing array at the specified index.
    /// </summary>
    /// <param name="index"> The index. </param>
    /// <param name="value"> The value to put. </param>
    public void PutShort( int index, short value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteInt16BigEndian( _memory.Span.Slice( index ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian( _memory.Span.Slice( index ), value );
        }

        if ( index >= Position )
        {
            Length = index + 1;
        }
    }

    // ----- Bulk Get/Put operations -----

    /// <summary>
    /// 
    /// </summary>
    public void GetShorts( out short[] shortArray )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    public void PutShorts( short[] shortArray )
    {
        throw new NotImplementedException();
    }

    // ========================================================================
    // Int Type handling methods
    // ========================================================================

    /// <summary>
    /// Gets an Int32 value from the backing array at the current <see cref="Buffer.Position"/>.
    /// </summary>
    public int GetInt()
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadInt32BigEndian( _memory.Span.Slice( Position ) )
            : BinaryPrimitives.ReadInt32LittleEndian( _memory.Span.Slice( Position ) );
    }

    /// <summary>
    /// Gets an Int32 value from the backing array at the specified index.
    /// </summary>
    public int GetInt( int index )
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadInt32BigEndian( _memory.Span.Slice( index ) )
            : BinaryPrimitives.ReadInt32LittleEndian( _memory.Span.Slice( index ) );
    }

    /// <summary>
    /// Puts the provided Int32 value into the backing array at the current <see cref="Buffer.Position"/>.
    /// </summary>
    /// <param name="value"> The value to put. </param>
    public void PutInt( int value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteInt32BigEndian( _memory.Span.Slice( Position ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian( _memory.Span.Slice( Position ), value );
        }

        Position += sizeof( int );
        Length   =  Position;
    }

    /// <summary>
    /// Puts the provided Int32 into the backing array at the specified index.
    /// </summary>
    /// <param name="index"> The index. </param>
    /// <param name="value"> The value to put. </param>
    public void PutInt( int index, int value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteInt32BigEndian( _memory.Span.Slice( index ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian( _memory.Span.Slice( index ), value );
        }

        if ( index >= Position )
        {
            Length = index + 1;
        }
    }

    // ----- Bulk Get/Put operations -----

    /// <summary>
    /// 
    /// </summary>
    public void GetInts( out int[] intArray )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    public void PutInts( int[] intArray )
    {
        throw new NotImplementedException();
    }

    // ========================================================================
    // Float Type handling methods
    // ========================================================================

    /// <summary>
    /// Gets a Float value from the backing array at the current <see cref="Buffer.Position"/>.
    /// </summary>
    public float GetFloat()
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadSingleBigEndian( _memory.Span.Slice( Position ) )
            : BinaryPrimitives.ReadSingleLittleEndian( _memory.Span.Slice( Position ) );
    }

    /// <summary>
    /// Gets a Float value from the backing array at the specified index.
    /// </summary>
    public float GetFloat( int index )
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadSingleBigEndian( _memory.Span.Slice( index ) )
            : BinaryPrimitives.ReadSingleLittleEndian( _memory.Span.Slice( index ) );
    }

    /// <summary>
    /// Puts the provided Float into the backing array at the current <see cref="Buffer.Position"/>.
    /// </summary>
    /// <param name="value"> The value to put. </param>
    public void PutFloat( float value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteSingleBigEndian( _memory.Span.Slice( Position ), value );
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian( _memory.Span.Slice( Position ), value );
        }

        Position += sizeof( float );
        Length   =  Position;
    }

    /// <summary>
    /// Puts the provided Float into the backing array at the specified index.
    /// </summary>
    /// <param name="index"> The index. </param>
    /// <param name="value"> The value to put. </param>
    public void PutFloat( int index, float value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteSingleBigEndian( _memory.Span.Slice( index ), value );
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian( _memory.Span.Slice( index ), value );
        }

        if ( index >= Position )
        {
            Length = index + 1;
        }
    }

    // ----- Bulk Get/Put operations -----

    /// <summary>
    /// 
    /// </summary>
    public void GetFloats( out float[] floatArray )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    public void PutFloats( float[] floatArray )
    {
        throw new NotImplementedException();
    }

    // ========================================================================

    /// <inheritdoc />
    public override void Resize( int extraCapacityInBytes )
    {
        switch ( extraCapacityInBytes )
        {
            case < 0:
                throw new ArgumentOutOfRangeException( nameof( extraCapacityInBytes ), "Extra capacity must be non-negative." );

            case 0:
                // No resize needed if extraCapacity is 0
                return;
        }

        var newCapacity = Capacity + extraCapacityInBytes;

        if ( newCapacity < 0 ) // Check for integer overflow (if capacity is very large)
        {
            throw new ArgumentOutOfRangeException( nameof( extraCapacityInBytes ), "Resize would result in capacity overflow." );
        }

        var newBackingArray = new byte[ newCapacity ];

        // Efficiently copy existing data using Array.Copy
        Array.Copy( _backingArray, newBackingArray, _backingArray.Length );

        _backingArray = newBackingArray;
        _memory       = _backingArray.AsMemory();
        Capacity      = newCapacity;

        // **Position Handling:** Keep position within the new bounds.
        if ( Position > Capacity )
        {
            Position = Capacity; // Clamp position to the new capacity if it was beyond
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        Array.Clear( _backingArray, 0, _backingArray.Length );

        Position = 0;
        Length   = 0;
    }

    /// <inheritdoc />
    public override int Remaining()
    {
        return ( Limit - Position ) / sizeof( byte );
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