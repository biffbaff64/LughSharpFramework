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
    /// <param name="allowAutoResize">
    /// If true, the buffer will automatically resize if a buffer overflow is possible. This
    /// can be disabled, either in the constructor parameter or by setting <see cref="Buffer.AutoResizeEnabled"/>
    /// to false.
    /// </param>
    /// <param name="maxCapacity">
    /// The value at which to set the maximum allowed buffer capacity. The default for this is 1GB, and
    /// can be changed to whatever the user requires.
    /// </param>
    public ByteBuffer( int capacityInBytes, bool allowAutoResize = true, int maxCapacity = DEFAULT_MAC_1GB )
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
        if ( memory.IsEmpty ) // Check if Memory<byte> is valid (not empty) - you can add more checks if needed
        {
            throw new ArgumentException( "Memory<byte> cannot be empty.", nameof( memory ) ); // Or ArgumentNullException if null Memory<byte> is possible
        }

        _memory = memory;
        _backingArray = memory.ToArray();
        IsBigEndian = isBigEndian;
        Capacity    = memory.Length; // Capacity is now derived from the provided Memory<byte>.Length
        Length      = 0;             // Initially Length is 0 for a new view
        Limit       = Capacity;      // Initially Limit is Capacity for a new view
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
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

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
    /// Transfers the entire content of the given source byte array into this buffer.
    /// An invocation of this method with the source byte array <paramref name="src"/>
    /// behaves in exactly the same way as invoking <c>dst.PutBytes(src, 0, src.Length)</c>.
    /// </summary>
    /// <param name="src">The source byte array.</param>
    /// <exception cref="ArgumentNullException"> If <paramref name="src"/> is null.</exception>
    public override void PutBytes( byte[] src )
    {
        // Does not need a call to EnsureCapacity as it will be called
        // from PutBytes(byte[],int,int,int).
        PutBytes( src, 0, 0, src.Length );
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
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

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
    /// 
    /// </summary>
    /// <param name="dst"></param>
    public void GetShorts( short[] dst )
    {
        GetShorts( dst, 0, dst.Length );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dst"></param>
    /// <param name="dstOffset"></param>
    /// <param name="length"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="IndexOutOfRangeException"></exception>
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

        System.Buffer.BlockCopy( src: _backingArray,
                                 srcOffset: Position,
                                 dst: dst,
                                 dstOffset: dstOffset * sizeof( short ), // Destination offset in bytes for short[]
                                 count: bytesToRead );

        Position += bytesToRead;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="src"></param>
    public void PutShorts( short[] src )
    {
        PutShorts( src, 0, src.Length );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="src"></param>
    /// <param name="srcOffset"></param>
    /// <param name="length"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="BufferOverflowException"></exception>
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

        System.Buffer.BlockCopy( src: src,
                                 srcOffset: srcOffset * sizeof( short ), // Source offset in bytes for short[]
                                 dst: _backingArray,
                                 dstOffset: Position,
                                 count: bytesToWrite );

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
    /// Puts the provided Int32 into the backing array at the specified index.
    /// </summary>
    /// <param name="index"> The index. </param>
    /// <param name="value"> The value to put. </param>
    public void PutInt( int index, int value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

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
    /// 
    /// </summary>
    /// <param name="dst"></param>
    public void GetInts( int[] dst )
    {
        GetInts( dst, 0, dst.Length );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dst"></param>
    /// <param name="dstOffset"></param>
    /// <param name="length"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="IndexOutOfRangeException"></exception>
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

        System.Buffer.BlockCopy( src: _backingArray,
                                 srcOffset: Position,
                                 dst: dst,
                                 dstOffset: dstOffset * sizeof( int ), // Destination offset in bytes for int[]
                                 count: bytesToRead );

        Position += bytesToRead;
    }

    /// <summary>
    /// 
    /// </summary>
    public void PutInts( int[] src )
    {
        PutInts( src, 0, src.Length );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="src"></param>
    /// <param name="srcOffset"></param>
    /// <param name="length"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="BufferOverflowException"></exception>
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

        System.Buffer.BlockCopy( src: src,
                                 srcOffset: srcOffset * sizeof( int ), // Source offset in bytes for int[]
                                 dst: _backingArray,
                                 dstOffset: Position,
                                 count: bytesToWrite );

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
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

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
    /// 
    /// </summary>
    /// <param name="dst"></param>
    public void GetFloats( float[] dst )
    {
        GetFloats( dst, 0, dst.Length );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dst"></param>
    /// <param name="dstOffset"></param>
    /// <param name="length"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="IndexOutOfRangeException"></exception>
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

        System.Buffer.BlockCopy( src: _backingArray,
                                 srcOffset: Position,
                                 dst: dst,
                                 dstOffset: dstOffset * sizeof( float ), // Destination offset in bytes for float[]
                                 count: bytesToRead );

        Position += bytesToRead;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="src"></param>
    public void PutFloats( float[] src )
    {
        PutFloats( src, 0, src.Length );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="src"></param>
    /// <param name="srcOffset"></param>
    /// <param name="length"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="BufferOverflowException"></exception>
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

        var bytesToWrite   = length * sizeof( float );
        var bytesRemaining = Capacity - Position; // Space remaining from Position to Capacity

        if ( bytesToWrite > bytesRemaining )
        {
            throw new BufferOverflowException( $"Not enough space remaining in buffer to write {length} floats. " +
                                               $"Remaining space: {bytesRemaining}" );
        }

        System.Buffer.BlockCopy( src: src,
                                 srcOffset: srcOffset * sizeof( float ), // Source offset in bytes for float[]
                                 dst: _backingArray,
                                 dstOffset: Position,
                                 count: bytesToWrite );

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
    /// Returns the backing array as a byte[].
    /// </summary>
    /// <returns></returns>
    public new byte[] ToArray()
    {
        return _backingArray;
    }

    /// <summary>
    /// Allocates a new ByteBuffer with the specified capacity (in bytes).
    /// </summary>
    /// <param name="capacityInBytes">The desired capacity of the ByteBuffer in bytes.</param>
    /// <returns>A new ByteBuffer instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="capacityInBytes"/> is negative.</exception>
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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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