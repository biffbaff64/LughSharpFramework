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

namespace LughSharp.Lugh.Utils.Buffers;

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
    /// <param name="allowAutoResize">
    /// If true, the buffer will automatically resize if a buffer overflow is possible. This
    /// can be disabled, either in the constructor parameter or by setting <see cref="Buffer.AutoResizeEnabled" />
    /// to false.
    /// </param>
    /// <param name="maxCapacity">
    /// The value at which to set the maximum allowed buffer capacity. The default for this is 1GB, and
    /// can be changed to whatever the user requires.
    /// </param>
    public ByteBuffer( int capacityInBytes, bool allowAutoResize = true, int maxCapacity = DEFAULT_MAC_1_GB )
        : base( capacityInBytes )
    {
        _backingArray = new byte[ capacityInBytes ];
        _memory       = _backingArray.AsMemory();
    }

    /// <summary>
    /// Creates a new ByteBuffer that is backed by the provided Memory&lt;byte&gt;.
    /// This constructor is intended for creating buffer views and should not allocate new memory.
    /// </summary>
    /// <param name="memory">The Memory&lt;byte&gt; to use as the backing store.</param>
    /// <param name="isBigEndian">Indicates whether the buffer should use big-endian byte order.</param>
    internal ByteBuffer( Memory< byte > memory, bool isBigEndian )
    {
        if ( memory.IsEmpty ) // Check if Memory<byte> is valid (not empty)
        {
            throw new ArgumentException( "Memory<byte> cannot be empty.",
                                         nameof( memory ) ); // Or ArgumentNullException if null Memory<byte> is possible
        }

        _memory       = memory;
        _backingArray = memory.ToArray();
        IsBigEndian   = isBigEndian;
        Capacity      = memory.Length; // Capacity is now derived from the provided Memory<byte>.Length
        Length        = 0;             // Initially Length is 0 for a new view
        Limit         = Capacity;      // Initially Limit is Capacity for a new view
    }

    // ========================================================================
    // Byte Type handling methods
    // ========================================================================

    /// <summary>
    /// Gets the Byte from the backing array at the current <see cref="Buffer.Position" />.
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
    /// Puts the provided Byte into the backing array at the current <see cref="Buffer.Position" />.
    /// Position is then updated.
    /// </summary>
    /// <param name="value"> The value to put. </param>
    public override void PutByte( byte value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( Position + 1 );

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
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( index + 1 );

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
    /// Transfers the entire contents of this buffer into the provided destination
    /// array. An invocation of this method with the destination byte array
    /// <paramref name="dst" /> behaves in exactly the same way as invoking
    /// <c>dst.GetBytes(src, 0, src.Length)</c>.
    /// </summary>
    /// <param name="dst"> The destination byte array. </param>
    /// <exception cref="ArgumentNullException"> If <paramref name="dst" /> is null.</exception>
    public override void GetBytes( byte[] dst )
    {
        GetBytes( dst, 0, Length );
    }

    /// <summary>
    /// Gets bytes from this buffer, starting at the current <see cref="Buffer.Position" />,
    /// and puts them into the provided destination byte array 'dst'.
    /// Updates the <see cref="Buffer.Position" /> by the number of bytes read.
    /// </summary>
    /// <param name="dst">
    /// The destination array to receive the bytes. Must be large enough to hold 'dstOffset + length' bytes.
    /// </param>
    /// <param name="dstOffset"> The starting offset within the destination array to write to. </param>
    /// <param name="length"> The number of bytes to get. </param>
    /// <exception cref="IndexOutOfRangeException">
    /// If there are not enough remaining bytes in the buffer or if dstOffset and length cause overflow in dst.
    /// </exception>
    /// <exception cref="ArgumentNullException"> If <paramref name="dst" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="dstOffset" /> or <paramref name="length" /> is negative.
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
    /// Transfers the entire content of the given source byte array into this buffer.
    /// An invocation of this method with the source byte array <paramref name="src" />
    /// behaves in exactly the same way as invoking <c>dst.PutBytes(src, 0, src.Length)</c>.
    /// </summary>
    /// <param name="src">The source byte array.</param>
    /// <exception cref="ArgumentNullException"> If <paramref name="src" /> is null.</exception>
    public override void PutBytes( byte[] src )
    {
        // Does not need a call to EnsureCapacity as it will be called
        // from PutBytes(byte[],int,int,int).
        PutBytes( src, 0, 0, src.Length );
    }

    /// <summary>
    /// Puts bytes from the source byte array 'src' into this buffer, starting at the current
    /// <see cref="Buffer.Position" /> and writing 'length' bytes. The writing starts at a
    /// destination offset within the buffer, specified by 'offset'. Also updates the
    /// <see cref="Buffer.Position" /> by the number of bytes written.
    /// </summary>
    /// <param name="src"> The source byte array to get bytes from. </param>
    /// <param name="srcOffset"> The starting offset within the source array to read from. </param>
    /// <param name="dstOffset"> The starting offset within *this buffer* (the destination) to write to. </param>
    /// <param name="length"> The number of bytes to put. </param>
    /// <exception cref="IndexOutOfRangeException">
    /// If there is not enough space remaining in the buffer or if srcOffset and length cause overflow in src.
    /// </exception>
    /// <exception cref="ArgumentNullException"> If <paramref name="src" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="srcOffset" />, <paramref name="dstOffset" /> or <paramref name="length" /> is negative.
    /// </exception>
    public override void PutBytes( byte[] src, int srcOffset, int dstOffset, int length )
    {
        ArgumentNullException.ThrowIfNull( src );

        EnsureCapacity( dstOffset + length );

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

    // ========================================================================
    // Short Type handling methods
    // ========================================================================

    /// <summary>
    /// Gets a Short value from the backing array at the current <see cref="Buffer.Position" />.
    /// </summary>
    public short GetShort()
    {
        var source = _memory.Span.Slice( Position, sizeof( short ) );

        return IsBigEndian
            ? BinaryPrimitives.ReadInt16BigEndian( source )
            : BinaryPrimitives.ReadInt16LittleEndian( source );
    }

    /// <summary>
    /// Gets a Short value from the backing array at the specified index.
    /// </summary>
    public short GetShort( int index )
    {
        var source = _memory.Span.Slice( index, sizeof( short ) );

        Logger.Debug( $"source length: {source.Length}, index: {index}" );

        if ( index >= source.Length )
        {
            throw new IndexOutOfRangeException( $"Index {index} is out of range for source length {source.Length}" );
        }

        return IsBigEndian
            ? BinaryPrimitives.ReadInt16BigEndian( source )
            : BinaryPrimitives.ReadInt16LittleEndian( source );
    }

    /// <summary>
    /// Puts the provided Short value into the backing array at the current <see cref="Buffer.Position" />.
    /// </summary>
    /// <param name="value"> The value to put. </param>
    public void PutShort( short value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( Position + sizeof( short ) );

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
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( index + sizeof( short ) );

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
    /// Reads a sequence of short values from the buffer into the specified destination array.
    /// </summary>
    /// <param name="dst">The array into which the short values will be written.</param>
    public void GetShorts( short[] dst )
    {
        GetShorts( dst, 0, dst.Length );
    }

    /// <summary>
    /// Reads multiple short values from the buffer into the specified destination array.
    /// </summary>
    /// <param name="dst">The destination array to hold the short values read from the buffer.</param>
    /// <param name="dstOffset">The starting index in the destination array where the short values should be written.</param>
    /// <param name="length">The number of short values to read from the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the destination array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the destination offset or length is negative, or when the specified range exceeds the bounds of the destination array.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">Thrown when the buffer does not contain enough remaining bytes to satisfy the request.</exception>
    public void GetShorts( short[] dst, int dstOffset, int length )
    {
        ArgumentNullException.ThrowIfNull( dst, nameof( dst ) );

        if ( dstOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Destination offset cannot be negative." );
        }

        if ( length < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( length ), "Length cannot be negative." );
        }

        if ( ( dstOffset + length ) > dst.Length )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Length and destination offset exceed destination array bounds." );
        }

        var bytesToRead    = length * sizeof( short );
        var bytesRemaining = Remaining();

        if ( bytesToRead > bytesRemaining )
        {
            throw new IndexOutOfRangeException( $"Not enough remaining bytes in buffer to read {length} shorts. " +
                                                $"Remaining bytes: {bytesRemaining}" );
        }

        System.Buffer.BlockCopy( _backingArray,
                                 Position,
                                 dst,
                                 dstOffset * sizeof( short ), // Destination offset in bytes for short[]
                                 bytesToRead );

        Position += bytesToRead;
    }

    /// <summary>
    /// Writes an array of short values into the buffer.
    /// </summary>
    /// <param name="src">An array of short values to be written to the buffer.</param>
    public void PutShorts( short[] src )
    {
        PutShorts( src, 0, src.Length );
    }

    /// <summary>
    /// Writes a subset of shorts from the specified source array into the buffer.
    /// </summary>
    /// <param name="src">The source array containing the short values to be written.</param>
    /// <param name="srcOffset">The zero-based offset in the source array to start reading shorts from.</param>
    /// <param name="length">The number of shorts to write from the source array into the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the source array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the source offset or length is negative or when their sum exceeds the bounds of the source array.
    /// </exception>
    /// <exception cref="BufferOverflowException">
    /// Thrown when there is not enough remaining space in the buffer to accommodate the specified number of shorts.
    /// </exception>
    public void PutShorts( short[] src, int srcOffset, int length )
    {
        ArgumentNullException.ThrowIfNull( src, nameof( src ) );

        if ( srcOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ), "Source offset cannot be negative." );
        }

        if ( length < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( length ), "Length cannot be negative." );
        }

        if ( ( srcOffset + length ) > src.Length )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ), "Length and source offset exceed source array bounds." );
        }

        var bytesToWrite   = length * sizeof( short );
        var bytesRemaining = Capacity - Position; // Space remaining from Position to Capacity

        if ( bytesToWrite > bytesRemaining )
        {
            throw new BufferOverflowException( $"Not enough space remaining in buffer to write {length} shorts. " +
                                               $"Remaining space: {bytesRemaining}" );
        }

        System.Buffer.BlockCopy( src,
                                 srcOffset * sizeof( short ), // Source offset in bytes for short[]
                                 _backingArray,
                                 Position,
                                 bytesToWrite );

        Position += bytesToWrite;

        if ( Position > Length )
        {
            Length = Position;
        }
    }

    // ========================================================================
    // Int Type handling methods
    // ========================================================================

    /// <summary>
    /// Gets an Int32 value from the backing array at the current <see cref="Buffer.Position" />.
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
    /// Puts the provided Int32 value into the backing array at the current <see cref="Buffer.Position" />.
    /// </summary>
    /// <param name="value"> The value to put. </param>
    public void PutInt( int value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( Position + sizeof( int ) );

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
    /// Represents a signed 32-bit integer value.
    /// </summary>
    public void PutInt( int index, int value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( index + sizeof( int ) );

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
    /// Retrieves a sequence of integers from the buffer and stores them into the specified array.
    /// </summary>
    /// <param name="dst">
    /// The array into which integers will be transferred. The length of the array determines
    /// the number of integers to retrieve starting from the current buffer position.
    /// </param>
    public void GetInts( int[] dst )
    {
        GetInts( dst, 0, dst.Length );
    }

    /// <summary>
    /// Reads a sequence of integers from the buffer into the specified destination array.
    /// </summary>
    /// <param name="dst">The destination array where the integers will be copied.</param>
    /// <param name="dstOffset">The starting index in the destination array at which integers will begin to be written.</param>
    /// <param name="length">The number of integers to read and copy into the destination array.</param>
    /// <exception cref="ArgumentNullException">Thrown if the destination array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the destination offset is negative, if the length is negative, or
    /// if the sum of the destination offset and length exceeds the bounds of the destination array.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if there are not enough remaining bytes in the buffer to read the requested number of integers.
    /// </exception>
    public void GetInts( int[] dst, int dstOffset, int length )
    {
        ArgumentNullException.ThrowIfNull( dst, nameof( dst ) );

        if ( dstOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Destination offset cannot be negative." );
        }

        if ( length < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( length ), "Length cannot be negative." );
        }

        if ( ( dstOffset + length ) > dst.Length )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Length and destination offset exceed destination array bounds." );
        }

        var bytesToRead    = length * sizeof( int );
        var bytesRemaining = Remaining();

        if ( bytesToRead > bytesRemaining )
        {
            throw new IndexOutOfRangeException( $"Not enough remaining bytes in buffer to read {length} ints. " +
                                                $"Remaining bytes: {bytesRemaining}" );
        }

        System.Buffer.BlockCopy( _backingArray,
                                 Position,
                                 dst,
                                 dstOffset * sizeof( int ), // Destination offset in bytes for int[]
                                 bytesToRead );

        Position += bytesToRead;
    }

    /// <summary>
    /// Writes an array of integers into the buffer starting at the current position.
    /// </summary>
    /// <param name="src">
    /// The integer array containing the data to be written into the buffer.
    /// </param>
    public void PutInts( int[] src )
    {
        PutInts( src, 0, src.Length );
    }

    /// <summary>
    /// Writes the specified values from the integer array into the buffer.
    /// </summary>
    /// <param name="src">The source array of integers to write into the buffer.</param>
    /// <param name="srcOffset">
    /// The zero-based index in the source array at which to begin reading integers.
    /// Must be non-negative and within the bounds of the array.
    /// </param>
    /// <param name="length">
    /// The number of integers to write from the source array into the buffer.
    /// Must be non-negative and must not exceed the remaining capacity of the buffer.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="srcOffset"/> is negative, <paramref name="length"/> is negative,
    /// or if the combined values of <paramref name="srcOffset"/> and <paramref name="length"/> exceed
    /// the bounds of the source array.
    /// </exception>
    /// <exception cref="BufferOverflowException">
    /// Thrown if the buffer does not have sufficient capacity to accommodate the number of integers being written.
    /// </exception>
    public void PutInts( int[] src, int srcOffset, int length )
    {
        ArgumentNullException.ThrowIfNull( src, nameof( src ) );

        if ( srcOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ), "Source offset cannot be negative." );
        }

        if ( length < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( length ), "Length cannot be negative." );
        }

        if ( ( srcOffset + length ) > src.Length )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ), "Length and source offset exceed source array bounds." );
        }

        var bytesToWrite   = length * sizeof( int );
        var bytesRemaining = Capacity - Position; // Space remaining from Position to Capacity

        if ( bytesToWrite > bytesRemaining )
        {
            throw new BufferOverflowException( $"Not enough space remaining in buffer to write {length} ints. " +
                                               $"Remaining space: {bytesRemaining}" );
        }

        System.Buffer.BlockCopy( src,
                                 srcOffset * sizeof( int ), // Source offset in bytes for int[]
                                 _backingArray,
                                 Position,
                                 bytesToWrite );

        Position += bytesToWrite;

        if ( Position > Length )
        {
            Length = Position;
        }
    }

    // ========================================================================
    // Float Type handling methods
    // ========================================================================

    /// <summary>
    /// Gets a Float value from the backing array at the current <see cref="Buffer.Position" />.
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
    /// Puts the provided Float into the backing array at the current <see cref="Buffer.Position" />.
    /// </summary>
    /// <param name="value"> The value to put. </param>
    public void PutFloat( float value )
    {
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( Position + sizeof( float ) );

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
        if ( IsReadOnly )
        {
            throw new GdxRuntimeException( "Cannot write to a read-only buffer." );
        }

        EnsureCapacity( index + sizeof( float ) );

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
    /// Copies a sequence of float values from the buffer into the specified array.
    /// </summary>
    /// <param name="dst">
    /// The array into which the float values are copied. The size of the array must be sufficient to
    /// accommodate the values being copied, starting from the first element.
    /// </param>
    public void GetFloats( float[] dst )
    {
        GetFloats( dst, 0, dst.Length );
    }

    /// <summary>
    /// Reads a specified number of float values from the buffer into the destination array.
    /// </summary>
    /// <param name="dst">The destination array where the float values will be stored.</param>
    /// <param name="dstOffset">The starting offset in the destination array at which the float values will be written.</param>
    /// <param name="length">The number of float values to read from the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="dst"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if <paramref name="dstOffset"/> is negative,
    /// or if <paramref name="length"/> is negative,
    /// or if the combination of <paramref name="dstOffset"/> and <paramref name="length"/> exceeds the bounds of the destination array.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">Thrown if there are not enough remaining bytes in the buffer to read the specified number of floats.</exception>
    public void GetFloats( float[] dst, int dstOffset, int length )
    {
        ArgumentNullException.ThrowIfNull( dst, nameof( dst ) );

        if ( dstOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Destination offset cannot be negative." );
        }

        if ( length < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( length ), "Length cannot be negative." );
        }

        if ( ( dstOffset + length ) > dst.Length )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Length and destination offset exceed destination array bounds." );
        }

        var bytesToRead    = length * sizeof( float );
        var bytesRemaining = Remaining();

        if ( bytesToRead > bytesRemaining )
        {
            throw new IndexOutOfRangeException( $"Not enough remaining bytes in buffer to read {length} floats. " +
                                                $"Remaining bytes: {bytesRemaining}" );
        }

        System.Buffer.BlockCopy( _backingArray,
                                 Position,
                                 dst,
                                 dstOffset * sizeof( float ), // Destination offset in bytes for float[]
                                 bytesToRead );

        Position += bytesToRead;
    }

    /// <summary>
    /// Writes an array of floats into the buffer starting from the current position.
    /// </summary>
    /// <param name="src">The array of floats to be written into the buffer.</param>
    public void PutFloats( float[] src )
    {
        PutFloats( src, 0, src.Length );
    }

    /// <summary>
    /// Writes a subset of the specified float array into the buffer starting at the current position.
    /// </summary>
    /// <param name="src">The source array of floats to be written to the buffer.</param>
    /// <param name="srcOffset">The starting index in the source array from which data will be read.</param>
    /// <param name="length">The number of floats to write to the buffer.</param>
    /// <exception cref="ArgumentNullException">Thrown if the source array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the source offset or length is negative, or if the sum of the source offset and length exceeds the bounds of the source array.
    /// </exception>
    /// <exception cref="BufferOverflowException">Thrown if writing the data would exceed the buffer's capacity and auto-resizing is not allowed.</exception>
    public void PutFloats( float[] src, int srcOffset, int length )
    {
        ArgumentNullException.ThrowIfNull( src, nameof( src ) );

        if ( srcOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ), "Source offset cannot be negative." );
        }

        if ( length < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( length ), "Length cannot be negative." );
        }

        if ( ( srcOffset + length ) > src.Length )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ), "Length and source offset exceed source array bounds." );
        }

        var bytesToWrite = length * sizeof( float );

        EnsureCapacity( Position + bytesToWrite );

        var bytesRemaining = Capacity - Position; // Space remaining from Position to Capacity

        if ( bytesToWrite > bytesRemaining )
        {
            throw new BufferOverflowException( $"Not enough space remaining in buffer to write {length} floats. " +
                                               $"Remaining space: {bytesRemaining} bytes" );
        }

        System.Buffer.BlockCopy( src,
                                 srcOffset * sizeof( float ), // Source offset in bytes for float[]
                                 _backingArray,
                                 Position,
                                 bytesToWrite );

        Position += bytesToWrite;

        if ( Position > Length )
        {
            Length = Position;
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public override ByteBuffer Order( ByteOrder order )
    {
        base.Order( order );

        return this;
    }

    /// <summary>
    /// Retrieves the underlying array that backs this byte buffer.
    /// </summary>
    /// <returns>The internal byte array used to store the data for this buffer.</returns>
    public byte[] BackingArray()
    {
        return _backingArray;
    }

    /// <summary>
    /// Indexer. Gets or sets the byte at the specified index in the ByteBuffer.
    /// </summary>
    /// <param name="index">The zero-based index of the byte to get or set.</param>
    /// <returns>The byte at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when the index is less than zero or greater than or equal to the <see cref="Buffer.Capacity"/>.
    /// </exception>
    public byte this[ int index ]
    {
        get
        {
            if ( ( index < 0 ) || ( index >= Capacity ) )
            {
                throw new IndexOutOfRangeException( $"Index '{index}' is out of range. Valid range is 0 to {Capacity - 1}." );
            }

            return _backingArray[ index ];
        }
        set
        {
            if ( ( index < 0 ) || ( index >= Capacity ) )
            {
                throw new IndexOutOfRangeException( $"Index '{index}' is out of range. Valid range is 0 to {Capacity - 1}." );
            }

            _backingArray[ index ] = value;
            _backingArray[ index ] = value;
        }
    }

    /// <summary>
    /// Allocates a new ByteBuffer with the specified capacity (in bytes).
    /// </summary>
    /// <param name="capacityInBytes">The desired capacity of the ByteBuffer in bytes.</param>
    /// <returns>A new ByteBuffer instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="capacityInBytes" /> is negative.</exception>
    public static ByteBuffer Allocate( int capacityInBytes )
    {
        if ( capacityInBytes < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( capacityInBytes ), "Capacity cannot be negative." );
        }

        return new ByteBuffer( capacityInBytes );
    }

    /// <summary>
    /// Creates a new IntBuffer whose content is a shared subsequence of this ByteBuffer's content.
    /// Changes to this ByteBuffer's content will be visible in the returned IntBuffer, and vice-versa.
    /// The position, limit, and mark of the new buffer will be independent of this buffer.
    /// </summary>
    /// <returns>A new IntBuffer instance.</returns>
    public IntBuffer AsIntBuffer()
    {
        // Calculate capacity in ints based on ByteBuffer's byte capacity
        // Floor to avoid partial ints at the end
        var capacityInInts = ( int )Math.Floor( ( double )Capacity / sizeof( int ) );

        // Create a new IntBuffer, sharing the _byteBuffer array
        return new IntBuffer( _backingArray, 0, capacityInInts, IsBigEndian );
    }

    /// <summary>
    /// Creates a new ShortBuffer whose content is a shared subsequence of this ByteBuffer's content.
    /// Changes to this ByteBuffer's content will be visible in the returned ShortBuffer, and vice-versa.
    /// The position, limit, and mark of the new buffer will be independent of this buffer.
    /// </summary>
    /// <returns>A new ShortBuffer instance.</returns>
    public ShortBuffer AsShortBuffer()
    {
        // Calculate capacity in shorts based on ByteBuffer's byte capacity
        // Floor to avoid partial shorts at the end
        var capacityInShorts = ( int )Math.Floor( ( double )Capacity / sizeof( short ) );

        // Create a new ShortBuffer, sharing the _byteBuffer array
        return new ShortBuffer( _backingArray, 0, capacityInShorts, IsBigEndian );
    }

    /// <summary>
    /// Creates a new FloatBuffer whose content is a shared subsequence of this ByteBuffer's content.
    /// Changes to this ByteBuffer's content will be visible in the returned FloatBuffer, and vice-versa.
    /// The position, limit, and mark of the new buffer will be independent of this buffer.
    /// </summary>
    /// <returns>A new FloatBuffer instance.</returns>
    public FloatBuffer AsFloatBuffer()
    {
        // Calculate capacity in floats based on ByteBuffer's byte capacity
        // Floor to avoid partial floats at the end
        var capacityInFloats = ( int )Math.Floor( ( double )Capacity / sizeof( float ) );

        // Create a new FloatBuffer, sharing the _byteBuffer array
        return new FloatBuffer( _backingArray, 0, capacityInFloats, IsBigEndian );
    }

    // ========================================================================

    /// <summary>
    /// Copies a range of elements from this buffer to a destination array.
    /// </summary>
    /// <param name="srcOffset">The offset within this buffer to start copying from</param>
    /// <param name="dst">The destination array</param>
    /// <param name="dstOffset">The offset within the destination array to start copying to</param>
    /// <param name="count">The number of elements to copy</param>
    public void BlockCopy( int srcOffset, byte[] dst, int dstOffset, int count )
    {
        ArgumentNullException.ThrowIfNull( dst );

        if ( ( srcOffset < 0 ) || ( srcOffset >= Capacity ) )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ) );
        }

        if ( ( dstOffset < 0 ) || ( dstOffset >= dst.Length ) )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ) );
        }

        ArgumentOutOfRangeException.ThrowIfNegative( count );

        if ( ( ( srcOffset + count ) > Capacity ) || ( ( dstOffset + count ) > dst.Length ) )
        {
            throw new ArgumentException( "Copying would overflow buffer" );
        }

        // Get backing array and copy
        var src = BackingArray();

        System.Buffer.BlockCopy( src, srcOffset, dst, dstOffset, count );
    }

    /// <inheritdoc />
    public override void Resize( int extraCapacityInBytes )
    {
        switch ( extraCapacityInBytes )
        {
            case < 0:
                throw new ArgumentOutOfRangeException( nameof( extraCapacityInBytes ),
                                                       "Extra capacity must be non-negative." );

            case 0:
                // No resize needed if extraCapacity is 0
                return;
        }

        var newCapacity = Capacity + extraCapacityInBytes;

        if ( newCapacity < 0 ) // Check for integer overflow (if capacity is very large)
        {
            throw new ArgumentOutOfRangeException( nameof( extraCapacityInBytes ),
                                                   "Resize would result in capacity overflow." );
        }

        Logger.Debug( $"Capacity before Resize: {_backingArray.Length} bytes"  );
        Array.Resize< byte >( ref _backingArray, newCapacity );

        _memory       = _backingArray.AsMemory();
        Capacity      = newCapacity;

        // **Position Handling:** Keep position within the new bounds.
        if ( Position > Capacity )
        {
            Position = Capacity; // Clamp position to the new capacity if it was beyond
        }
        
        Logger.Debug( $"_backingArray: {_backingArray.Length}, Capacity: {Capacity} bytes"  );
        Logger.Debug( $"Position after Resize: {Position}"  );
    }

    /// <summary>
    /// Creates a new sliced buffer from the current buffer.
    /// </summary>
    /// <returns>
    /// A new <see cref="ByteBuffer"/> that represents a slice of the current buffer.
    /// </returns>
    public ByteBuffer Slice()
    {
        //TODO:

        throw new NotImplementedException();
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

    /// <inheritdoc />
    public override void Compact()
    {
        var remaining = Limit - Position;

        if ( remaining > 0 )
        {
            System.Buffer.BlockCopy( _backingArray, Position, _backingArray, 0, remaining );
        }

        Position = remaining;
        Limit    = Capacity;

        DiscardMark();
    }

    /// <summary>
    /// Wraps a byte array into a buffer.
    /// <para>
    /// The new buffer will be backed by the given byte array; that is, modifications
    /// to the buffer will cause the array to be modified and vice versa. The new buffer's
    /// capacity will be <c>array.length</c>, its position will be <c>offset</c>, its
    /// limit will be <c>offset + length</c>, and its mark will be undefined. Its
    /// backing array will be the given array.
    /// </para>
    /// </summary>
    /// <param name="array"> The array that will back the new buffer </param>
    /// <param name="offset">
    /// The offset of the subarray to be used; must be non-negative and no larger
    /// than <c>array.length</c>. The new buffer's position will be set to this value.
    /// </param>
    /// <param name="length">
    /// The length of the subarray to be used; must be non-negative and no larger
    /// than <c>array.length - offset</c>. The new buffer's limit will be set to
    /// <c>offset + length</c>.
    /// </param>
    /// <returns> The new byte buffer </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// If the preconditions on the <c>offset</c> and <c>length</c> parameters do not hold
    /// </exception>
    public static ByteBuffer Wrap( byte[] array, int offset, int length )
    {
        try
        {
            ArgumentNullException.ThrowIfNull( array );

            var buffer = new ByteBuffer( length );
            buffer.PutBytes( array, offset, 0, length );

            return buffer;
        }
        catch ( ArgumentException )
        {
            throw new IndexOutOfRangeException();
        }
    }

    /// <summary>
    /// Wraps a byte array into a buffer.
    /// <para>
    /// The new buffer will be backed by the given byte array; that is, modifications
    /// to the buffer will cause the array to be modified and vice versa. The new buffer's
    /// capacity will be <c>array.length</c>, its position will be <c>offset</c>, its
    /// limit will be <c>offset + length</c>, and its mark will be undefined. Its
    /// backing array will be the given array.
    /// </para>
    /// </summary>
    /// <param name="array"> The array that will back the new buffer </param>
    /// <returns> The new byte buffer </returns>
    public static ByteBuffer Wrap( byte[] array )
    {
        ArgumentNullException.ThrowIfNull( array );

        return Wrap( array, 0, array.Length );
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