// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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
using System.Runtime.CompilerServices;

namespace LughSharp.Source.Utils;

/// <summary>
/// Represents a generic, resizable buffer for unmanaged types, supporting
/// byte-level and element-level operations, endianness, and direct/bulk access.
/// </summary>
/// <typeparam name="T">The element type of the buffer (must be unmanaged).</typeparam>
[PublicAPI]
public class Buffer< T > : IDisposable where T : unmanaged
{
    /// <summary>
    /// Gets the total capacity of the buffer in bytes.
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    /// Gets or sets the current position in the buffer (in bytes).
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// Gets or sets the limit of the buffer (in bytes).
    /// </summary>
    public int Limit { get; set; }

    /// <summary>
    /// Gets the current length of valid data in the buffer (in bytes).
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// Gets or sets a value indicating whether the buffer uses big-endian byte order.
    /// </summary>
    public bool IsBigEndian { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the buffer is read-only.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the buffer is direct (not backed by managed memory).
    /// </summary>
    public bool IsDirect { get; set; }

    /// <summary>
    /// Gets the capacity of the buffer in elements of type <typeparamref name="T"/>.
    /// </summary>
    public int ElementCapacity => Capacity / _elementSize;

    /// <summary>
    /// Gets the current position in the buffer in elements of type <typeparamref name="T"/>.
    /// </summary>
    public int ElementPosition => Position / _elementSize;

    /// <summary>
    /// Gets the current length of valid data in the buffer in elements
    /// of type <typeparamref name="T"/>.
    /// </summary>
    public int ElementLength => Length / _elementSize;

    // ========================================================================

    protected const bool IsReadOnlyDefault = false;
    protected const bool IsDirectDefault   = false;

    // ========================================================================

    private byte[]         _backingArray;
    private Memory< byte > _memory;
    private int            _elementSize;

    // ========================================================================

    /// <summary>
    /// Creates a new Buffer with the specified initial capacity.
    /// </summary>
    public Buffer( int elementCount )
    {
        IsBigEndian   = !BitConverter.IsLittleEndian;
        IsReadOnly    = IsReadOnlyDefault;
        IsDirect      = IsDirectDefault;
        _elementSize  = Unsafe.SizeOf< T >();
        Capacity      = elementCount * _elementSize; // Always store as bytes
        _backingArray = new byte[ Capacity ];
        _memory       = _backingArray.AsMemory();
        Limit         = Capacity;
    }

    /// <summary>
    /// Creates a new Buffer that wraps the provided Memory{byte}, and with the specified
    /// byte order..
    /// </summary>
    public Buffer( Memory< byte > memory, bool isBigEndian )
    {
        if ( memory.IsEmpty ) // Check if Memory<byte> is valid (not empty)
        {
            throw new ArgumentException( "Memory<byte> cannot be empty.",
                                         nameof( memory ) );
        }

        _memory       = memory;
        _backingArray = memory.ToArray();
        IsBigEndian   = isBigEndian;
        Capacity      = memory.Length; // Capacity is derived from the provided Memory<byte>.Length
        Limit         = Capacity;      // Initially Limit is Capacity for a new view
        Length        = 0;             // Initially Length is 0 for a new view
        Position      = 0;             // Initially Position is 0 for a new view
    }

    /// <summary>
    /// Creates a Buffer that shares the same underlying memory as this buffer.
    /// Changes to one buffer will be reflected in the other.
    /// </summary>
    /// <typeparam name="TView">The target element type (must be unmanaged)</typeparam>
    /// <returns>A new buffer view with the specified element type</returns>
    public Buffer< TView > AsBuffer< TView >() where TView : unmanaged
    {
        int viewElementSize = Unsafe.SizeOf< TView >();

        // Ensure the conversion makes sense (capacity should be divisible by target element size)
        if ( ( Length % viewElementSize ) != 0 )
        {
            throw new InvalidOperationException( $"Cannot create Buffer<{typeof( TView ).Name}> " +
                                                 $"view: current length ({Length} bytes) is not " +
                                                 $"divisible by target element size ({viewElementSize} bytes)" );
        }

        var viewBuffer = new Buffer< TView >( _memory, IsBigEndian )
        {
            Position   = Position,
            Limit      = Limit,
            Length     = Length,
            IsReadOnly = IsReadOnly,
            IsDirect   = IsDirect
        };

        return viewBuffer;
    }

    // ========================================================================
    // Convenience methods for common conversions
    // ========================================================================

    /// <summary>
    /// Creates a Buffer&lt; float &gt; that shares the same underlying memory
    /// as this buffer. Changes to one buffer will be reflected in the other.
    /// </summary>
    public Buffer< float > AsFloatBuffer()
    {
        return AsBuffer< float >();
    }

    /// <summary>
    /// Creates a Buffer&lt; int &gt; that shares the same underlying memory
    /// as this buffer. Changes to one buffer will be reflected in the other.
    /// </summary>
    public Buffer< int > AsIntBuffer()
    {
        return AsBuffer< int >();
    }

    /// <summary>
    /// Creates a Buffer&lt; short &gt; that shares the same underlying memory
    /// as this buffer. Changes to one buffer will be reflected in the other.
    /// </summary>
    public Buffer< short > AsShortBuffer()
    {
        return AsBuffer< short >();
    }

    /// <summary>
    /// Creates a Buffer&lt; byte &gt; that shares the same underlying memory
    /// as this buffer. Changes to one buffer will be reflected in the other.
    /// </summary>
    public Buffer< byte > AsByteBuffer()
    {
        return AsBuffer< byte >();
    }

    // ========================================================================

    /// <summary>
    /// Reads the next element of type <typeparamref name="T"/> from the buffer at the current
    /// position and advances the position by the size of the element.
    /// </summary>
    /// <returns>The next element of type <typeparamref name="T"/>.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if the next element is beyond the buffer's limit.
    /// </exception>
    public T Get()
    {
        if ( ( Position + _elementSize ) > Limit )
        {
            throw new IndexOutOfRangeException();
        }

        var value = MemoryMarshal.Read< T >( _memory.Span.Slice( Position ) );
        Position += _elementSize;

        return value;
    }

    /// <summary>
    /// Reads the element of type <typeparamref name="T"/> at the specified index from the buffer.
    /// </summary>
    /// <param name="elementIndex">The index of the element to read.</param>
    /// <returns>The element of type <typeparamref name="T"/> at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the specified index is out of range.</exception>
    public T Get( int elementIndex )
    {
        int byteIndex = elementIndex * _elementSize;

        if ( ( byteIndex < 0 ) || ( byteIndex > Limit ) )
        {
            throw new IndexOutOfRangeException();
        }

        return ( byteIndex + _elementSize ) > Capacity
                   ? throw new IndexOutOfRangeException()
                   : MemoryMarshal.Read< T >( _memory.Span.Slice( byteIndex ) );
    }

    /// <summary>
    /// Reads <c>count</c> elements from the buffer into the specified array, starting at the given offset.
    /// </summary>
    /// <param name="array">The array to read elements into.</param>
    /// <param name="offset">The starting index in the array to begin writing elements.</param>
    /// <param name="count">The number of elements to read from the buffer.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if the specified range is out of the buffer's bounds.</exception>
    public void Get( T[] array, int offset, int count )
    {
        int bytesToRead = count * _elementSize;

        if ( ( Position + bytesToRead ) > Limit )
        {
            throw new IndexOutOfRangeException();
        }

        MemoryMarshal.Cast< byte, T >( _memory.Span.Slice( Position, bytesToRead ) )
                     .CopyTo( array.AsSpan( offset, count ) );

        Position += bytesToRead;
    }

    /// <summary>
    /// Writes the specified element of type <typeparamref name="T"/> to the buffer at
    /// the current position and advances the position by the size of the element.
    /// </summary>
    /// <param name="value">The element to write to the buffer.</param>
    public void Put( T value )
    {
        EnsureCapacity( Position + _elementSize );

        MemoryMarshal.Write( _memory.Span.Slice( Position ), in value );
        Position += _elementSize;

        if ( Position > Length )
        {
            Length = Position;
        }
    }

    /// <summary>
    /// Writes the specified element of type <typeparamref name="T"/> to the buffer at the given index.
    /// </summary>
    /// <param name="elementIndex">The index at which to write the element.</param>
    /// <param name="value">The element to write to the buffer.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown if the specified index is out of range.</exception>
    public void Put( int elementIndex, T value )
    {
        if ( ( elementIndex < 0 ) || ( elementIndex >= ElementCapacity ) )
        {
            throw new IndexOutOfRangeException( $"elementIndex out of range: {elementIndex}" );
        }

        int byteIndex = elementIndex * _elementSize;

        EnsureCapacity( byteIndex + _elementSize );

        MemoryMarshal.Write( _memory.Span.Slice( byteIndex ), in value );

        if ( ( byteIndex + _elementSize ) > Length )
        {
            Length = byteIndex + _elementSize;
        }
    }

    /// <summary>
    /// Writes the elements of the specified array to the buffer at the current position
    /// and advances the position by the number of elements written.
    /// </summary>
    /// <param name="array">The array of elements to write to the buffer.</param>
    public void Put( T[] array )
    {
        Put( array, 0, array.Length );
    }

    /// <summary>
    /// Writes the elements of the specified array to the buffer at the given offset and
    /// advances the position by the number of elements written.
    /// </summary>
    /// <param name="array">The array of elements to write to the buffer.</param>
    /// <param name="offset">The starting index in the array to begin reading elements.</param>
    /// <param name="count">The number of elements to write to the buffer.</param>
    public void Put( T[] array, int offset, int count )
    {
        Put( array, offset, Position, count );
    }

    /// <summary>
    /// Writes the elements of the specified array to the buffer at the given source and destination offsets.
    /// </summary>
    /// <param name="array">The array of elements to write to the buffer.</param>
    /// <param name="srcOffset">The starting index in the array to begin reading elements.</param>
    /// <param name="dstOffset">The starting index in the buffer to begin writing elements.</param>
    /// <param name="count">The number of elements to write to the buffer.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Put( T[] array, int srcOffset, int dstOffset, int count )
    {
        if ( ( srcOffset < 0 ) || ( ( srcOffset + count ) > array.Length ) )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ),
                                                   "Source offset is out of range." );
        }

        if ( dstOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ),
                                                   "Destination offset cannot be negative." );
        }

        int dstByteOffset = dstOffset * _elementSize;
        int bytesToWrite  = count * _elementSize;

        EnsureCapacity( dstByteOffset + bytesToWrite );

        array.AsSpan( srcOffset, count )
             .CopyTo( MemoryMarshal.Cast< byte, T >( _memory.Span.Slice( dstByteOffset ) ) );

        if ( ( dstByteOffset + bytesToWrite ) > Length )
        {
            Length = dstByteOffset + bytesToWrite;
        }
    }

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Get / Put BYTE operations

    /// <summary>
    /// Reads the next byte from the buffer at the current position and advances
    /// the position by one byte.
    /// </summary>
    /// <returns>The byte read from the buffer.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the current position is out of range.</exception>
    public byte GetByte()
    {
        if ( ( Position + 1 ) > Limit )
        {
            throw new IndexOutOfRangeException();
        }

        byte value = _memory.Span[ Position ];
        Position += 1;

        return value;
    }

    /// <summary>
    /// Reads the byte at the specified index from the buffer without changing the position.
    /// </summary>
    /// <param name="byteIndex">The index of the byte to read.</param>
    /// <returns>The byte read from the buffer.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the specified index is out of range.</exception>
    public byte GetByte( int byteIndex )
    {
        if ( ( byteIndex < 0 ) || ( byteIndex >= Capacity ) )
        {
            throw new IndexOutOfRangeException();
        }

        return _memory.Span[ byteIndex ];
    }

    /// <summary>
    /// Reads the specified number of bytes from the buffer into a new byte array,
    /// starting at position 0.
    /// </summary>
    /// <param name="numBytes"> The number of bytes to read. </param>
    /// <returns></returns>
    public byte[] GetBytes( int numBytes )
    {
        return GetBytes( 0, numBytes );
    }

    /// <summary>
    /// Reads a specified number of bytes from the buffer starting at the given offset.
    /// </summary>
    /// <param name="offset">The zero-based position within the buffer to start reading from.</param>
    /// <param name="length">The number of bytes to read from the buffer.</param>
    /// <returns>A byte array containing the requested number of bytes.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when the buffer does not contain enough remaining bytes to fulfill the request.
    /// </exception>
    public byte[] GetBytes( int offset, int length )
    {
        Guard.Against.Negative( offset );
        Guard.Against.NegativeOrZero( length );

        // Check if enough bytes remaining in buffer
        if ( ( Position + length ) > Limit )
        {
            throw new IndexOutOfRangeException( "Not enough remaining bytes in buffer to read the requested length." );
        }

        var dst = new byte[ length ];

        // Efficient copy to dst array
        _memory.Span.Slice( Position, length ).CopyTo( dst.AsSpan( offset, length ) );

        // Update Position by the number of bytes read
        Position += length;

        return dst;
    }

    /// <summary>
    /// Writes the specified byte to the buffer at the current position and advances
    /// the position by one byte.
    /// </summary>
    /// <param name="value">The byte value to write to the buffer.</param>
    public void PutByte( byte value )
    {
        EnsureCapacity( Position + 1 );

        _memory.Span[ Position ] =  value;
        Position                 += 1;

        if ( Position > Length )
        {
            Length = Position;
        }
    }

    /// <summary>
    /// Writes the specified byte to the buffer at the given index without changing the position.
    /// </summary>
    /// <param name="byteIndex">The index at which to write the byte.</param>
    /// <param name="value">The byte value to write to the buffer.</param>
    public void PutByte( int byteIndex, byte value )
    {
        if ( ( byteIndex < 0 ) || ( byteIndex >= Capacity ) )
        {
            EnsureCapacity( byteIndex + 1 );
        }

        _memory.Span[ byteIndex ] = value;

        if ( ( byteIndex + 1 ) > Length )
        {
            Length = byteIndex + 1;
        }
    }

    /// <summary>
    /// Writes the specified byte array to the buffer, starting at position 0.
    /// </summary>
    /// <param name="src">The source byte array.</param>
    public void PutBytes( byte[] src )
    {
        PutBytes( src, 0, 0, src.Length );
    }

    /// <summary>
    /// Writes the specified number of bytes from the source array to the buffer.
    /// </summary>
    /// <param name="src">The source byte array.</param>
    /// <param name="srcOffset">The offset in the source array from which to start reading.</param>
    /// <param name="numBytes">The number of bytes to write.</param>
    public void PutBytes( byte[] src, int srcOffset, int numBytes )
    {
        PutBytes( src, srcOffset, Position, numBytes );
    }

    /// <summary>
    /// Writes the specified number of bytes from the source array to the buffer, starting
    /// at the given source offset and the given destination offset in the buffer. The position
    /// in the buffer will be updated if the write extends beyond the current position.
    /// </summary>
    /// <param name="src">The source byte array.</param>
    /// <param name="srcOffset">The offset in the source array from which to start reading.</param>
    /// <param name="dstOffset">The offset in the buffer at which to start writing.</param>
    /// <param name="numBytes">The number of bytes to write.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public void PutBytes( byte[] src, int srcOffset, int dstOffset, int numBytes )
    {
        ArgumentNullException.ThrowIfNull( src );

        EnsureCapacity( dstOffset + numBytes );

        if ( srcOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ), "Source offset cannot be negative." );
        }

        if ( dstOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Destination offset cannot be negative." );
        }

        if ( numBytes < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( numBytes ), "numBytes cannot be negative." );
        }

        if ( ( srcOffset + numBytes ) > src.Length )
        {
            throw new ArgumentOutOfRangeException( nameof( numBytes ),
                                                   "Length and source offset exceed source array bounds." );
        }

        // Check for space in destination buffer using dstOffset
        if ( ( dstOffset + numBytes ) > Capacity )
        {
            throw new IndexOutOfRangeException( "Not enough space in buffer to put the "
                                              + "requested length at the given destination offset." );
        }

        // Copy from src to buffer at dstOffset
        src.AsSpan( srcOffset, numBytes ).CopyTo( _memory.Span.Slice( dstOffset, numBytes ) );

        if ( ( dstOffset + numBytes ) > Position )
        {
            Position = dstOffset + numBytes;
        }

        if ( Position > Length )
        {
            Length = Position;
        }
    }

    #endregion Get / Put BYTE operations

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Get / Put SHORT operations

    /// <summary>
    /// Reads the next short (2 bytes) from the buffer at the current position, taking
    /// into account the endianness, and advances the position by 2 bytes.
    /// </summary>
    /// <returns>The short value read from the buffer.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the current position is out of range.</exception>
    public short GetShort()
    {
        if ( ( Position + 2 ) > Limit )
        {
            throw new IndexOutOfRangeException();
        }

        short value = IsBigEndian
                          ? BinaryPrimitives.ReadInt16BigEndian( _memory.Span.Slice( Position ) )
                          : BinaryPrimitives.ReadInt16LittleEndian( _memory.Span.Slice( Position ) );

        Position += 2;

        return value;
    }

    /// <summary>
    /// Reads the short (2 bytes) at the specified index from the buffer, taking into
    /// account the endianness, without changing the position.
    /// </summary>
    /// <param name="byteIndex">The index of the byte to read.</param>
    /// <returns>The short value read from the buffer.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the specified index is out of range.</exception>
    public short GetShort( int byteIndex )
    {
        if ( ( byteIndex < 0 ) || ( ( byteIndex + 2 ) > Capacity ) )
        {
            throw new IndexOutOfRangeException();
        }

        return IsBigEndian
                   ? BinaryPrimitives.ReadInt16BigEndian( _memory.Span.Slice( byteIndex ) )
                   : BinaryPrimitives.ReadInt16LittleEndian( _memory.Span.Slice( byteIndex ) );
    }

    /// <summary>
    /// Reads the specified number of shorts from the buffer into a new short array,
    /// starting at position 0.
    /// </summary>
    /// <param name="numShorts"> The number of short to read. </param>
    /// <returns></returns>
    public short[] GetShorts( int numShorts )
    {
        return GetShorts( 0, numShorts );
    }

    /// <summary>
    /// Reads a specified number of 16-bit short values from the buffer, starting at the specified offset.
    /// </summary>
    /// <param name="offset">The starting index in the destination array where the values will be stored.</param>
    /// <param name="length">The number of short values to read from the buffer.</param>
    /// <returns>An array of 16-bit short values read from the buffer.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when the buffer does not have enough remaining bytes to read the requested length.
    /// </exception>
    public short[] GetShorts( int offset, int length )
    {
        Guard.Against.Negative( offset );
        Guard.Against.NegativeOrZero( length );

        int byteCount = length * 2;

        if ( ( Position + byteCount ) > Limit )
        {
            throw new IndexOutOfRangeException( "Not enough remaining bytes in buffer to read the requested length." );
        }

        var          dst = new short[ length ];
        Span< byte > src = _memory.Span.Slice( Position, byteCount );

        if ( IsBigEndian )
        {
            for ( var i = 0; i < length; i++ )
            {
                dst[ offset + i ] = BinaryPrimitives.ReadInt16BigEndian( src.Slice( i * 2 ) );
            }
        }
        else
        {
            for ( var i = 0; i < length; i++ )
            {
                dst[ offset + i ] = BinaryPrimitives.ReadInt16LittleEndian( src.Slice( i * 2 ) );
            }
        }

        Position += byteCount;

        return dst;
    }

    /// <summary>
    /// Writes the specified short (2 bytes) to the buffer at the current position, taking
    /// into account the endianness, and advances the position by 2 bytes.
    /// </summary>
    /// <param name="value">The short value to write to the buffer.</param>
    public void PutShort( short value )
    {
        EnsureCapacity( Position + 2 );

        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteInt16BigEndian( _memory.Span.Slice( Position ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian( _memory.Span.Slice( Position ), value );
        }

        Position += 2;

        if ( Position > Length )
        {
            Length = Position;
        }
    }

    /// <summary>
    /// Writes the specified short (2 bytes) to the buffer at the given index, taking into
    /// account the endianness, without changing the position.
    /// </summary>
    /// <param name="byteIndex">The index at which to write the short.</param>
    /// <param name="value">The short value to write to the buffer.</param>
    public void PutShort( int byteIndex, short value )
    {
        EnsureCapacity( byteIndex + 2 );

        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteInt16BigEndian( _memory.Span.Slice( byteIndex ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian( _memory.Span.Slice( byteIndex ), value );
        }

        if ( ( byteIndex + 2 ) > Length )
        {
            Length = byteIndex + 2;
        }
    }

    /// <summary>
    /// Writes the specified short array to the buffer, starting at position 0.
    /// 2 bytes per short.
    /// </summary>
    /// <param name="src"> The source array. </param>
    public void PutShorts( short[] src )
    {
        PutShorts( src, 0, 0, src.Length );
    }

    /// <summary>
    /// Writes the specified number of shorts from the source array to the buffer.
    /// </summary>
    /// <param name="src"> The source array. </param>
    /// <param name="srcOffset"> The offset in the source array. </param>
    /// <param name="numBytes"> The number of bytes to write. </param>
    public void PutShorts( short[] src, int srcOffset, int numBytes )
    {
        PutShorts( src, srcOffset, Position, numBytes );
    }

    /// <summary>
    /// Writes the specified number of shorts from the source array to the buffer, starting
    /// at the given source offset and the given destination offset in the buffer. The position
    /// in the buffer will be updated if the write extends beyond the current position.
    /// </summary>
    /// <param name="src"> The source array. </param>
    /// <param name="srcOffset"> The offset in the source array. </param>
    /// <param name="dstOffset"> The offset in the buffer. </param>
    /// <param name="numBytes"> The number of bytes to write. </param>
    /// <exception cref="ArgumentNullException"> THrown if the source array is null. </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the source offset is out of range or the destination offset is negative.
    /// </exception>
    public void PutShorts( short[] src, int srcOffset, int dstOffset, int numBytes )
    {
        ArgumentNullException.ThrowIfNull( src );

        if ( ( srcOffset < 0 ) || ( ( srcOffset + numBytes ) > src.Length ) )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ), "Source offset is out of range." );
        }

        if ( dstOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Destination offset cannot be negative." );
        }

        int byteCount = numBytes * 2;

        EnsureCapacity( dstOffset + byteCount );

        Span< byte > dstSpan = _memory.Span.Slice( dstOffset, byteCount );

        if ( IsBigEndian )
        {
            for ( var i = 0; i < numBytes; i++ )
            {
                BinaryPrimitives.WriteInt16BigEndian( dstSpan.Slice( i * 2 ), src[ srcOffset + i ] );
            }
        }
        else
        {
            for ( var i = 0; i < numBytes; i++ )
            {
                BinaryPrimitives.WriteInt16LittleEndian( dstSpan.Slice( i * 2 ), src[ srcOffset + i ] );
            }
        }

        if ( ( dstOffset + byteCount ) > Position )
        {
            Position = dstOffset + byteCount;
        }

        if ( Position > Length )
        {
            Length = Position;
        }
    }

    #endregion Get / Put SHORT operations

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Get / Put INT operations

    /// <summary>
    /// Reads the next int (4 bytes) from the buffer at the current position, taking
    /// into account the endianness, and advances the position by 4 bytes.
    /// </summary>
    /// <returns>The int value read from the buffer.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the current position is out of range.</exception>
    public int GetInt()
    {
        if ( ( Position + 4 ) > Limit )
        {
            throw new IndexOutOfRangeException();
        }

        int value = IsBigEndian
                        ? BinaryPrimitives.ReadInt32BigEndian( _memory.Span.Slice( Position ) )
                        : BinaryPrimitives.ReadInt32LittleEndian( _memory.Span.Slice( Position ) );

        Position += 4;

        return value;
    }

    /// <summary>
    /// Reads the int (4 bytes) at the specified index from the buffer, taking into
    /// account the endianness, without changing the position.
    /// </summary>
    /// <param name="byteIndex">The index of the byte to read.</param>
    /// <returns>The int value read from the buffer.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the specified index is out of range.</exception>
    public int GetInt( int byteIndex )
    {
        if ( ( byteIndex < 0 ) || ( ( byteIndex + 4 ) > Capacity ) )
        {
            throw new IndexOutOfRangeException();
        }

        return IsBigEndian
                   ? BinaryPrimitives.ReadInt32BigEndian( _memory.Span.Slice( byteIndex ) )
                   : BinaryPrimitives.ReadInt32LittleEndian( _memory.Span.Slice( byteIndex ) );
    }

    /// <summary>
    /// Reads the specified number of ints from the buffer into a new int array,
    /// starting at position 0.
    /// </summary>
    /// <param name="numInts">The number of ints to read.</param>
    public int[] GetInts( int numInts )
    {
        return GetInts( 0, numInts );
    }

    /// <summary>
    /// Reads a specified number of ints from the buffer starting at the given offset.
    /// </summary>
    /// <param name="offset">The zero-based position within the destination array to start writing.</param>
    /// <param name="length">The number of ints to read from the buffer.</param>
    /// <returns>An int array containing the requested number of ints.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the source offset is out of range or the destination offset is negative.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when the buffer does not contain enough remaining bytes to fulfill the request.
    /// </exception>
    public int[] GetInts( int offset, int length )
    {
        Guard.Against.Negative( offset );

        int byteCount = length * 4;
        var dst       = new int[ length ];

        if ( ( offset < 0 ) || ( ( offset + length ) > dst.Length ) )
        {
            throw new ArgumentOutOfRangeException( nameof( offset ),
                                                   "Destination offset and length exceed array bounds." );
        }

        if ( ( Position + byteCount ) > Limit )
        {
            throw new IndexOutOfRangeException( "Not enough remaining bytes in buffer to read the requested length." );
        }

        Span< byte > src = _memory.Span.Slice( Position, byteCount );

        if ( IsBigEndian )
        {
            for ( var i = 0; i < length; i++ )
            {
                dst[ offset + i ] = BinaryPrimitives.ReadInt32BigEndian( src.Slice( i * 4 ) );
            }
        }
        else
        {
            for ( var i = 0; i < length; i++ )
            {
                dst[ offset + i ] = BinaryPrimitives.ReadInt32LittleEndian( src.Slice( i * 4 ) );
            }
        }

        Position += byteCount;

        return dst;
    }

    /// <summary>
    /// Writes the specified int (4 bytes) to the buffer at the current position, taking
    /// into account the endianness, and advances the position by 4 bytes.
    /// </summary>
    /// <param name="value">The int value to write to the buffer.</param>
    public void PutInt( int value )
    {
        EnsureCapacity( Position + 4 );

        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteInt32BigEndian( _memory.Span.Slice( Position ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian( _memory.Span.Slice( Position ), value );
        }

        Position += 4;

        if ( Position > Length )
        {
            Length = Position;
        }
    }

    /// <summary>
    /// Writes the specified int (4 bytes) to the buffer at the given index, taking into
    /// account the endianness, without changing the position.
    /// </summary>
    /// <param name="byteIndex">The index at which to write the int.</param>
    /// <param name="value">The int value to write to the buffer.</param>
    public void PutInt( int byteIndex, int value )
    {
        EnsureCapacity( byteIndex + 4 );

        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteInt32BigEndian( _memory.Span.Slice( byteIndex ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian( _memory.Span.Slice( byteIndex ), value );
        }

        if ( ( byteIndex + 4 ) > Length )
        {
            Length = byteIndex + 4;
        }
    }

    /// <summary>
    /// Writes the specified int array to the buffer, starting at position 0.
    /// </summary>
    /// <param name="src"> The source array. </param>
    public void PutInts( int[] src )
    {
        PutInts( src, 0, 0, src.Length );
    }

    /// <summary>
    /// Writes the specified number of ints from the source array to the buffer.
    /// </summary>
    /// <param name="src"> The source array. </param>
    /// <param name="srcOffset"> The offset in the source array. </param>
    /// <param name="numInts"> The number of ints to write. </param>
    public void PutInts( int[] src, int srcOffset, int numInts )
    {
        PutInts( src, srcOffset, Position, numInts );
    }

    /// <summary>
    /// Writes the specified number of ints from the source array to the buffer, starting
    /// at the given source offset and the given destination offset in the buffer. The position
    /// in the buffer will be updated if the write extends beyond the current position.
    /// </summary>
    /// <param name="src"> The source array. </param>
    /// <param name="srcOffset"> The offset in the source array. </param>
    /// <param name="dstOffset"> The offset in the buffer. </param>
    /// <param name="numInts"> The number of ints to write. </param>
    /// <exception cref="ArgumentNullException"> THrown if the source array is null. </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the source offset is out of range or the destination offset is negative.
    /// </exception>
    public void PutInts( int[] src, int srcOffset, int dstOffset, int numInts )
    {
        ArgumentNullException.ThrowIfNull( src );

        if ( ( srcOffset < 0 ) || ( ( srcOffset + numInts ) > src.Length ) )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ), "Source offset is out of range." );
        }

        if ( dstOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Destination offset cannot be negative." );
        }

        int byteCount = numInts * 4;

        EnsureCapacity( dstOffset + byteCount );

        Span< byte > dstSpan = _memory.Span.Slice( dstOffset, byteCount );

        if ( IsBigEndian )
        {
            for ( var i = 0; i < numInts; i++ )
            {
                BinaryPrimitives.WriteInt32BigEndian( dstSpan.Slice( i * 4 ), src[ srcOffset + i ] );
            }
        }
        else
        {
            for ( var i = 0; i < numInts; i++ )
            {
                BinaryPrimitives.WriteInt32LittleEndian( dstSpan.Slice( i * 4 ), src[ srcOffset + i ] );
            }
        }

        if ( ( dstOffset + byteCount ) > Position )
        {
            Position = dstOffset + byteCount;
        }

        if ( Position > Length )
        {
            Length = Position;
        }
    }

    #endregion Get / Put INT operations

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

    #region Get / Put FLOAT operations

    /// <summary>
    /// Reads the next float (4 bytes) from the buffer at the current position, taking
    /// into account the endianness, and advances the position by 4 bytes.
    /// </summary>
    /// <returns>The float value read from the buffer.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the current position is out of range.</exception>
    public float GetFloat()
    {
        if ( ( Position + 4 ) > Limit )
        {
            throw new IndexOutOfRangeException();
        }

        float value = IsBigEndian
                          ? BinaryPrimitives.ReadSingleBigEndian( _memory.Span.Slice( Position ) )
                          : BinaryPrimitives.ReadSingleLittleEndian( _memory.Span.Slice( Position ) );

        Position += 4;

        return value;
    }

    /// <summary>
    /// Reads the float (4 bytes) at the specified index from the buffer, taking into
    /// account the endianness, without changing the position.
    /// </summary>
    /// <param name="byteIndex">The index of the byte to read.</param>
    /// <returns>The float value read from the buffer.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if the specified index is out of range.</exception>
    public float GetFloat( int byteIndex )
    {
        if ( ( byteIndex < 0 ) || ( ( byteIndex + 4 ) > Capacity ) )
        {
            throw new IndexOutOfRangeException();
        }

        return IsBigEndian
                   ? BinaryPrimitives.ReadSingleBigEndian( _memory.Span.Slice( byteIndex ) )
                   : BinaryPrimitives.ReadSingleLittleEndian( _memory.Span.Slice( byteIndex ) );
    }

    /// <summary>
    /// Reads the specified number of floats from the buffer into a new float array,
    /// starting at position 0.
    /// </summary>
    /// <param name="numFloats">The number of floats to read.</param>
    public float[] GetFloats( int numFloats )
    {
        return GetFloats( 0, numFloats );
    }

    /// <summary>
    /// Reads a specified number of floats from the buffer starting at the given offset.
    /// </summary>
    /// <param name="offset">The zero-based position within the destination array to start writing.</param>
    /// <param name="length">The number of floats to read from the buffer.</param>
    /// <returns>A float array containing the requested number of floats.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the source offset is out of range or the destination offset is negative.
    /// </exception>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when the buffer does not contain enough remaining bytes to fulfill the request.
    /// </exception>
    public float[] GetFloats( int offset, int length )
    {
        Guard.Against.Negative( offset );

        int byteCount = length * 4;
        var dst       = new float[ length ];

        if ( ( offset < 0 ) || ( ( offset + length ) > dst.Length ) )
        {
            throw new ArgumentOutOfRangeException( nameof( offset ),
                                                   "Destination offset and length exceed array bounds." );
        }

        if ( ( Position + byteCount ) > Limit )
        {
            throw new IndexOutOfRangeException( "Not enough remaining bytes in buffer to read the requested length." );
        }

        Span< byte > src = _memory.Span.Slice( Position, byteCount );

        if ( IsBigEndian )
        {
            for ( var i = 0; i < length; i++ )
            {
                dst[ offset + i ] = BinaryPrimitives.ReadSingleBigEndian( src.Slice( i * 4 ) );
            }
        }
        else
        {
            for ( var i = 0; i < length; i++ )
            {
                dst[ offset + i ] = BinaryPrimitives.ReadSingleLittleEndian( src.Slice( i * 4 ) );
            }
        }

        Position += byteCount;

        return dst;
    }

    /// <summary>
    /// Writes the specified float (4 bytes) to the buffer at the current position, taking
    /// into account the endianness, and advances the position by 4 bytes.
    /// </summary>
    /// <param name="value">The float value to write to the buffer.</param>
    public void PutFloat( float value )
    {
        EnsureCapacity( Position + 4 );

        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteSingleBigEndian( _memory.Span.Slice( Position ), value );
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian( _memory.Span.Slice( Position ), value );
        }

        Position += 4;

        if ( Position > Length )
        {
            Length = Position;
        }
    }

    /// <summary>
    /// Writes the specified float (4 bytes) to the buffer at the given index, taking into
    /// account the endianness, without changing the position.
    /// </summary>
    /// <param name="byteIndex">The index at which to write the float.</param>
    /// <param name="value">The float value to write to the buffer.</param>
    public void PutFloat( int byteIndex, float value )
    {
        EnsureCapacity( byteIndex + 4 );

        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteSingleBigEndian( _memory.Span.Slice( byteIndex ), value );
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian( _memory.Span.Slice( byteIndex ), value );
        }

        if ( ( byteIndex + 4 ) > Length )
        {
            Length = byteIndex + 4;
        }
    }

    /// <summary>
    /// Writes all floats from the source array to the buffer.
    /// </summary>
    /// <param name="src">The source float array.</param>
    public void PutFloats( float[] src )
    {
        PutFloats( src, 0, 0, src.Length );
    }

    /// <summary>
    /// Writes the specified number of bytes from the source float array to the buffer, starting
    /// at the given source offset and the current position in the buffer. The position in the 
    /// buffer will be advanced by the number of bytes written.
    /// </summary>
    /// <param name="src">The source float array.</param>
    /// <param name="srcOffset">The offset in the source array from which to start reading.</param>
    /// <param name="numBytes">The number of bytes to write to the buffer.</param>
    public void PutFloats( float[] src, int srcOffset, int numBytes )
    {
        PutFloats( src, srcOffset, Position, numBytes );
    }
    
    /// <summary>
    /// Writes the specified number of ints from the source array to the buffer, starting
    /// at the given source offset and the given destination offset in the buffer. The position
    /// in the buffer will be updated if the write extends beyond the current position.
    /// </summary>
    /// <param name="src"> The source array. </param>
    /// <param name="srcOffset"> The offset in the source array. </param>
    /// <param name="dstOffset"> The offset in the buffer. </param>
    /// <param name="numFloats"> The number of ints to write. </param>
    /// <exception cref="ArgumentNullException"> THrown if the source array is null. </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the source offset is out of range or the destination offset is negative.
    /// </exception>
    public void PutFloats( float[] src, int srcOffset, int dstOffset, int numFloats )
    {
        ArgumentNullException.ThrowIfNull( src );

        if ( ( srcOffset < 0 ) || ( ( srcOffset + numFloats ) > src.Length ) )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ), "Source offset is out of range." );
        }

        if ( dstOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Destination offset cannot be negative." );
        }

        int byteCount = numFloats * 4;

        EnsureCapacity( dstOffset + byteCount );

        Span< byte > dstSpan = _memory.Span.Slice( dstOffset, byteCount );

        if ( IsBigEndian )
        {
            for ( var i = 0; i < numFloats; i++ )
            {
                BinaryPrimitives.WriteSingleBigEndian( dstSpan.Slice( i * 4 ), src[ srcOffset + i ] );
            }
        }
        else
        {
            for ( var i = 0; i < numFloats; i++ )
            {
                BinaryPrimitives.WriteSingleLittleEndian( dstSpan.Slice( i * 4 ), src[ srcOffset + i ] );
            }
        }

        if ( ( dstOffset + byteCount ) > Position )
        {
            Position = dstOffset + byteCount;
        }

        if ( Position > Length )
        {
            Length = Position;
        }
    }

    #endregion Get / Put FLOAT operations

    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================
    // ========================================================================

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
    public static Buffer< byte > Wrap( byte[] array, int offset, int length )
    {
        try
        {
            ArgumentNullException.ThrowIfNull( array );

            var buffer = new Buffer< byte >( length );
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
    public static Buffer< byte > Wrap( byte[] array )
    {
        ArgumentNullException.ThrowIfNull( array );

        return Wrap( array, 0, array.Length );
    }

    // ========================================================================

    /// <summary>
    /// Allocates a new ByteBuffer with the specified capacity (in bytes).
    /// </summary>
    /// <param name="capacityInBytes">The desired capacity of the ByteBuffer in bytes.</param>
    /// <returns>A new ByteBuffer instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="capacityInBytes" /> is negative.</exception>
    public static Buffer< byte > Allocate( int capacityInBytes )
    {
        if ( capacityInBytes < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( capacityInBytes ), "Capacity cannot be negative." );
        }

        return new Buffer< byte >( capacityInBytes );
    }

    // ========================================================================

    /// <summary>
    /// Resizes the buffer by increasing its capacity by the specified number of bytes. The
    /// new capacity will be the current capacity plus the extra capacity. If the extra capacity
    /// is zero, no resizing will occur.
    /// <para>
    /// If the extra capacity is negative, an <see cref="ArgumentOutOfRangeException"/> will be thrown.
    /// </para>
    /// </summary>
    /// <param name="extraCapacityInBytes">The number of bytes to increase the buffer's capacity by.</param>
    /// <exception cref="ArgumentOutOfRangeException">If <paramref name="extraCapacityInBytes" /> is negative.</exception>
    public void Resize( int extraCapacityInBytes )
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

        int newCapacity = Capacity + extraCapacityInBytes;
        Array.Resize( ref _backingArray, newCapacity );

        _memory  = _backingArray.AsMemory();
        Capacity = newCapacity;
    }

    /// <summary>
    /// Ensures that the buffer has at least the specified capacity in bytes. If the current
    /// capacity is less than the required capacity, the buffer is resized to accommodate
    /// the required capacity.
    /// </summary>
    /// <param name="requiredBytes">The minimum required capacity in bytes.</param>
    public void EnsureCapacity( int requiredBytes )
    {
        if ( Capacity < requiredBytes )
        {
            Resize( requiredBytes - Capacity );
        }
    }

    /// <summary>
    /// Returns the underlying byte array of the buffer.
    /// </summary>
    /// <returns>The byte array that backs the buffer.</returns>
    public byte[] BackingArray()
    {
        return _backingArray;
    }

    /// <summary>
    /// Returns a memory representation of the buffer's underlying byte array.
    /// </summary>
    /// <returns>A <see cref="Memory{Byte}"/> representing the buffer's data.</returns>
    public Memory< byte > Memory()
    {
        return _memory;
    }

    /// <summary>
    /// Converts the buffer's contents to a new array of type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>A new array containing the buffer's data.</returns>
    public T[] ToArray()
    {
        int elementCount = Length / _elementSize;
        var result       = new T[ elementCount ];
        MemoryMarshal.Cast< byte, T >( _memory.Span.Slice( 0, Length ) ).CopyTo( result );

        return result;
    }

    /// <summary>
    /// Clears the buffer by resetting the position and length to zero, and clearing the
    /// underlying byte array.
    /// </summary>
    public void Clear()
    {
        Array.Clear( _backingArray, 0, Capacity );

        Position = 0;
        Length   = 0;
    }

    /// <summary>
    /// Prepares the buffer for reading by setting the limit to the current position and
    /// resetting the position to zero.
    /// <para>
    /// After calling this method, the buffer is ready to be read from the beginning up to the
    /// previous position. This is typically used after writing data to the buffer and before
    /// reading it.
    /// </para>
    /// </summary>
    public void Flip()
    {
        Limit    = Position;
        Position = 0;
    }

    /// <summary>
    /// Sets the current position to the specified value, effectively rewinding to an
    /// earlier point.
    /// </summary>
    /// <param name="toPosition">
    /// The position to rewind to. Must be greater than or equal to 0. Defaults to 0.
    /// </param>
    public void Rewind( int toPosition = 0 )
    {
        Position = toPosition;
    }

    /// <summary>
    /// Reduces the capacity of the underlying storage to match the current length,
    /// releasing any unused memory.
    /// <para>
    /// Use this method to minimize memory usage after removing items or shrinking the
    /// collection. After calling this method, the capacity will be equal to the number
    /// of elements currently stored.
    /// </para>
    /// </summary>
    public void Compact()
    {
        int newCapacity = Length;

        Array.Resize( ref _backingArray, newCapacity );

        _memory  = _backingArray.AsMemory();
        Capacity = newCapacity;
    }

    /// <summary>
    /// Creates a new buffer that represents a subsequence of the current buffer's
    /// remaining elements, starting from the current position up to the limit. The
    /// new buffer shares the same underlying memory as the original buffer.
    /// </summary>
    /// <returns>
    /// A new buffer instance representing the subsequence of the current buffer from
    /// the current position to the limit.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if there are no remaining elements in the buffer to create a slice.
    /// </exception>
    public Buffer< T > Slice()
    {
        int remainingElements = ( Limit - Position ) / _elementSize;

        if ( remainingElements <= 0 )
        {
            throw new InvalidOperationException( "Cannot create slice: no remaining data in buffer" );
        }

        Memory< byte > sliceMemory = _memory.Slice( Position, Limit - Position );

        var slicedBuffer = new Buffer< T >( remainingElements );
        slicedBuffer._memory       = sliceMemory;
        slicedBuffer._backingArray = sliceMemory.ToArray();            // For direct array access
        slicedBuffer.Limit         = remainingElements * _elementSize; // In bytes for the slice
        slicedBuffer.IsBigEndian   = IsBigEndian;
        slicedBuffer.IsReadOnly    = IsReadOnly;

        return slicedBuffer;
    }

    /// <summary>
    /// Creates a new buffer that shares a subsequence of this buffer's elements between
    /// the specified indices. Modifications in the slice affect the backing data of the
    /// original buffer.
    /// </summary>
    /// <param name="fromElementIndex">The start index of the slice, inclusive.</param>
    /// <param name="toElementIndex">The end index of the slice, inclusive.</param>
    /// <returns>
    /// A new buffer instance representing the specified range of elements from the original
    /// buffer.
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown if <paramref name="fromElementIndex"/> or <paramref name="toElementIndex"/>
    /// are out of bounds.
    /// </exception>
    public Buffer< T > Slice( int fromElementIndex, int toElementIndex )
    {
        if ( ( fromElementIndex < 0 ) || ( fromElementIndex >= ElementCapacity ) )
        {
            throw new IndexOutOfRangeException( $"fromElementIndex out of range: {fromElementIndex}" );
        }

        if ( ( toElementIndex < 0 ) || ( toElementIndex >= ElementCapacity ) )
        {
            throw new IndexOutOfRangeException( $"toElementIndex out of range: {toElementIndex}" );
        }

        Memory< byte > sliceMemory = _memory.Slice( fromElementIndex * _elementSize,
                                                    ( toElementIndex - fromElementIndex + 1 ) * _elementSize );
        var sliceBuffer = new Buffer< T >( sliceMemory, IsBigEndian );

        if ( IsReadOnly )
        {
            sliceBuffer.IsReadOnly = true;
        }

        return sliceBuffer;
    }

    /// <summary>
    /// Reduces the capacity of the buffer to its current limit, effectively shrinking its size.
    /// Excess memory beyond the limit is discarded, and the backing array is resized if necessary.
    /// Adjusts the position, limit, and length if they exceed the new capacity.
    /// </summary>
    public void Shrink()
    {
        Guard.Against.Null( _backingArray );

        int newCapacityInBytes = Limit;

        if ( Capacity > newCapacityInBytes )
        {
            var newArray = new byte[ newCapacityInBytes ];
            Array.Copy( _backingArray, 0, newArray, 0, Math.Min( Length, newCapacityInBytes ) );

            _backingArray = newArray;
            _memory       = _backingArray.AsMemory();
            Capacity      = newCapacityInBytes;

            // Adjust Position and Limit if they exceed new capacity
            if ( Position > newCapacityInBytes )
            {
                Position = newCapacityInBytes;
            }

            if ( Limit > newCapacityInBytes )
            {
                Limit = newCapacityInBytes;
            }

            if ( Length > newCapacityInBytes )
            {
                Length = newCapacityInBytes;
            }
        }
    }

    /// <summary>
    /// Determines whether there are remaining elements between the current position
    /// and the limit in the buffer.
    /// </summary>
    /// <returns>True if there are elements remaining; otherwise, false.</returns>
    public bool HasRemaining()
    {
        return Position < Limit;
    }

    /// <summary>
    /// Calculates the number of elements remaining between the current position and
    /// the limit in the buffer.
    /// </summary>
    /// <returns>The number of elements available to be read or written in the buffer.</returns>
    public int Remaining()
    {
        return ( Limit - Position ) / _elementSize;
    }

    /// <summary>
    /// Retrieves the byte order of the buffer based on its current configuration.
    /// </summary>
    /// <returns>The byte order of the buffer, either big-endian or little-endian.</returns>
    public ByteOrder GetOrder()
    {
        return IsBigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
    }

    /// <summary>
    /// Sets the byte order of the buffer.
    /// </summary>
    /// <param name="order">The byte order to apply to the buffer.</param>
    /// <returns>The buffer instance with the updated byte order.</returns>
    public void SetOrder( ByteOrder order )
    {
        IsBigEndian = order == ByteOrder.BigEndian;
    }

    /// <summary>
    /// Indexer. Gets or sets the byte at the specified index in the Buffer.
    /// </summary>
    /// <param name="index">The zero-based index of the byte to get or set.</param>
    /// <returns>The byte at the specified index.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// Thrown when the index is less than zero or greater than or equal to the <see cref="Capacity"/>.
    /// </exception>
    public byte this[ int index ]
    {
        get
        {
            if ( ( index < 0 ) || ( index >= Capacity ) )
            {
                throw new IndexOutOfRangeException( $"Index '{index}' is out of range. "
                                                  + $"Valid range is 0 to {Capacity - 1}." );
            }

            Guard.Against.Null( _backingArray );

            return _backingArray[ index ];
        }
        set
        {
            if ( ( index < 0 ) || ( index >= Capacity ) )
            {
                throw new IndexOutOfRangeException( $"Index '{index}' is out of range. "
                                                  + $"Valid range is 0 to {Capacity - 1}." );
            }

            Guard.Against.Null( _backingArray );

            _backingArray[ index ] = value;
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Releases all resources used by the buffer. This method clears the underlying byte array
    /// and resets the buffer's properties to their default values.
    /// </summary>
    /// <param name="disposing">Indicates whether the method is being called from the Dispose method.</param>
    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            // Clear sensitive data
            Array.Clear( _backingArray, 0, _backingArray.Length );
            _backingArray = null!;

            _memory  = Memory< byte >.Empty;
            Capacity = 0;
            Position = 0;
            Limit    = 0;
            Length   = 0;
        }
    }
}

// ========================================================================
// ========================================================================