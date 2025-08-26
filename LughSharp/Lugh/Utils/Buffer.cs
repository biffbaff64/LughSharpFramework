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

namespace LughSharp.Lugh.Utils;

//[PublicAPI]
//public static class Buffers
//{
//    public static Buffer< byte > Buffer< byte >( int count ) => new( count );
//    public static Buffer< int > Buffer< int >( int count ) => new( count );
//    public static Buffer< short > Buffer< short >( int count ) => new( count );
//    public static Buffer< float > Buffer< float >( int count ) => new( count );
//}

[PublicAPI]
public class Buffer< T > : IDisposable where T : unmanaged
{
    // All capacity/position/limit values are in Bytes
    public int  Capacity    { get; private set; }
    public int  Position    { get; set; }
    public int  Limit       { get; set; }
    public int  Length      { get; private set; }
    public bool IsBigEndian { get; set; }
    public bool IsReadOnly  { get; set; }
    public bool IsDirect    { get; set; }

    // ========================================================================

    // Convenience properties for element-based operations
    public int ElementCapacity => Capacity / _elementSize;
    public int ElementPosition => Position / _elementSize;
    public int ElementLength   => Length / _elementSize;

    // ========================================================================

    protected const bool IS_READ_ONLY_DEFAULT = false;
    protected const bool IS_DIRECT_DEFAULT    = false;

    // ========================================================================

    private          byte[]         _backingArray;
    private          Memory< byte > _memory;
    private readonly int            _elementSize;

    // ========================================================================

    public Buffer( int elementCount )
    {
        IsBigEndian = !BitConverter.IsLittleEndian;
        IsReadOnly  = IS_READ_ONLY_DEFAULT;
        IsDirect    = IS_DIRECT_DEFAULT;

        _elementSize = Unsafe.SizeOf< T >();

        Capacity = elementCount * _elementSize; // Always store as bytes

        _backingArray = new byte[ Capacity ];
        _memory       = _backingArray.AsMemory();
        Limit         = Capacity;
    }

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
        Capacity      = memory.Length; // Capacity is now derived from the provided Memory<byte>.Length
        Limit         = Capacity;      // Initially Limit is Capacity for a new view
        Length        = 0;             // Initially Length is 0 for a new view
        Position      = 0;             // Initially Position is 0 for a new view
    }

//    // ========================================================================
//    // ========================================================================
//    
//    /// <summary>
//    /// Creates a new IntBuffer whose content is a shared subsequence of this
//    /// ByteBuffer's content. Changes to this ByteBuffer's content will be visible in
//    /// the returned IntBuffer, and vice-versa. The position, limit, and mark of the
//    /// new buffer will be independent of this buffer.
//    /// </summary>
//    /// <returns>A new IntBuffer instance.</returns>
//    public Buffer< int > AsIntBuffer()
//    {
//        // Calculate capacity in ints based on ByteBuffer's byte capacity
//        // Floor to avoid partial ints at the end
//        var capacityInInts = ( int )Math.Floor( ( double )Capacity / sizeof( int ) );
//
//        // Create a new IntBuffer, sharing the _byteBuffer array
//        return new Buffer< int >( _backingArray, 0, capacityInInts, IsBigEndian );
//    }
//
//    /// <summary>
//    /// Creates a new ShortBuffer whose content is a shared subsequence of this
//    /// ByteBuffer's content. Changes to this ByteBuffer's content will be visible in
//    /// the returned ShortBuffer, and vice-versa. The position, limit, and mark of the
//    /// new buffer will be independent of this buffer.
//    /// </summary>
//    /// <returns>A new ShortBuffer instance.</returns>
//    public Buffer< short > AsShortBuffer()
//    {
//        // Calculate capacity in shorts based on ByteBuffer's byte capacity
//        // Floor to avoid partial shorts at the end
//        var capacityInShorts = ( int )Math.Floor( ( double )Capacity / sizeof( short ) );
//
//        // Create a new ShortBuffer, sharing the _byteBuffer array
//        return new Buffer< short >( _backingArray, 0, capacityInShorts, IsBigEndian );
//    }
//
//    /// <summary>
//    /// Creates a new FloatBuffer whose content is a shared subsequence of this
//    /// ByteBuffer's content. Changes to this ByteBuffer's content will be visible in
//    /// the returned FloatBuffer, and vice-versa. The position, limit, and mark of the
//    /// new buffer will be independent of this buffer.
//    /// </summary>
//    /// <returns>A new FloatBuffer instance.</returns>
//    public Buffer< float > AsFloatBuffer()
//    {
//        // Calculate capacity in floats based on ByteBuffer's byte capacity
//        // Floor to avoid partial floats at the end
//        var capacityInFloats = ( int )Math.Floor( ( double )Capacity / sizeof( float ) );
//
//        // Create a new FloatBuffer, sharing the _byteBuffer array
//        return new Buffer< float >( _backingArray, 0, capacityInFloats, IsBigEndian );
//    }

    // ========================================================================

    /// <summary>
    /// Creates a Buffer that shares the same underlying memory as this buffer.
    /// Changes to one buffer will be reflected in the other.
    /// </summary>
    /// <typeparam name="Tview">The target element type (must be unmanaged)</typeparam>
    /// <returns>A new buffer view with the specified element type</returns>
    public Buffer< Tview > AsBuffer< Tview >() where Tview : unmanaged
    {
        var viewElementSize = Unsafe.SizeOf< Tview >();

        // Ensure the conversion makes sense (capacity should be divisible by target element size)
        if ( ( Length % viewElementSize ) != 0 )
        {
            throw new InvalidOperationException( $"Cannot create Buffer<{typeof( Tview ).Name}> " +
                                                 $"view: current length ({Length} bytes) is not " +
                                                 $"divisible by target element size ({viewElementSize} bytes)" );
        }

        var viewBuffer = new Buffer< Tview >( _memory, IsBigEndian )
        {
            Position   = Position,
            Limit      = Limit,
            Length     = Length,
            IsReadOnly = IsReadOnly,
            IsDirect   = IsDirect,
        };

        return viewBuffer;
    }

    // Convenience methods for common conversions
    public Buffer< float > AsFloatBuffer()
    {
        return AsBuffer< float >();
    }

    public Buffer< int > AsIntBuffer()
    {
        return AsBuffer< int >();
    }

    public Buffer< short > AsShortBuffer()
    {
        return AsBuffer< short >();
    }

    public Buffer< byte > AsByteBuffer()
    {
        return AsBuffer< byte >();
    }

    // ========================================================================

    #region Get Methods

    // ========================================================================

    // Generic Get/Put methods
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

    public T Get( int elementIndex )
    {
        var byteIndex = elementIndex * _elementSize;

        if ( ( byteIndex < 0 ) || ( byteIndex > Limit ) )
        {
            throw new IndexOutOfRangeException();
        }

        return ( byteIndex + _elementSize ) > Capacity
            ? throw new IndexOutOfRangeException()
            : MemoryMarshal.Read< T >( _memory.Span.Slice( byteIndex ) );
    }

    public void Get( T[] array )
    {
        Get( array, 0, array.Length );
    }

    public void Get( T[] array, int offset, int count )
    {
        var bytesToRead = count * _elementSize;

        if ( ( Position + bytesToRead ) > Limit )
        {
            throw new IndexOutOfRangeException();
        }

        MemoryMarshal.Cast< byte, T >( _memory.Span.Slice( Position, bytesToRead ) )
                     .CopyTo( array.AsSpan( offset, count ) );

        Position += bytesToRead;
    }

    #endregion Get Methods

    // ========================================================================

    #region Put Methods

    // ========================================================================

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

    public void Put( int elementIndex, T value )
    {
        if ( ( elementIndex < 0 ) || ( elementIndex >= ElementCapacity ) )
        {
            throw new IndexOutOfRangeException( $"elementIndex out of range: {elementIndex}" );
        }

        var byteIndex = elementIndex * _elementSize;
        EnsureCapacity( byteIndex + _elementSize );

        MemoryMarshal.Write( _memory.Span.Slice( byteIndex ), in value );

        if ( ( byteIndex + _elementSize ) > Length )
        {
            Length = byteIndex + _elementSize;
        }
    }

    public void Put( T[] array )
    {
        Put( array, 0, array.Length );
    }

    public void Put( T[] array, int offset, int count )
    {
        Put( array, offset, Position, count );
    }

    public void Put( T[] array, int srcOffset, int dstOffset, int count )
    {
        Guard.ThrowIfNull( array );

        if ( ( srcOffset < 0 ) || ( ( srcOffset + count ) > array.Length ) )
        {
            throw new ArgumentOutOfRangeException( nameof( srcOffset ), "Source offset is out of range." );
        }

        if ( dstOffset < 0 )
        {
            throw new ArgumentOutOfRangeException( nameof( dstOffset ), "Destination offset cannot be negative." );
        }

        // Checks that count is positive
        Guard.ValidPositiveInteger( count );

        var dstByteOffset = dstOffset * _elementSize;
        var bytesToWrite  = count * _elementSize;
        EnsureCapacity( dstByteOffset + bytesToWrite );

        array.AsSpan( srcOffset, count )
             .CopyTo( MemoryMarshal.Cast< byte, T >( _memory.Span.Slice( dstByteOffset ) ) );

        if ( ( dstByteOffset + bytesToWrite ) > Length )
        {
            Length = dstByteOffset + bytesToWrite;
        }
    }

    #endregion Put Methods

    // ========================================================================

    #region Typed Get Methods

    // ========================================================================

    public byte GetByte()
    {
        if ( ( Position + 1 ) > Limit )
        {
            throw new IndexOutOfRangeException();
        }

        var value = _memory.Span[ Position ];
        Position += 1;

        return value;
    }

    public byte GetByte( int byteIndex )
    {
        if ( ( byteIndex < 0 ) || ( byteIndex >= Capacity ) )
        {
            throw new IndexOutOfRangeException();
        }

        return _memory.Span[ byteIndex ];
    }

    public short GetShort()
    {
        if ( ( Position + 2 ) > Limit )
        {
            throw new IndexOutOfRangeException();
        }

        var value = IsBigEndian
            ? BinaryPrimitives.ReadInt16BigEndian( _memory.Span.Slice( Position ) )
            : BinaryPrimitives.ReadInt16LittleEndian( _memory.Span.Slice( Position ) );

        Position += 2;

        return value;
    }

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

    public int GetInt()
    {
        if ( ( Position + 4 ) > Limit )
        {
            throw new IndexOutOfRangeException();
        }

        var value = IsBigEndian
            ? BinaryPrimitives.ReadInt32BigEndian( _memory.Span.Slice( Position ) )
            : BinaryPrimitives.ReadInt32LittleEndian( _memory.Span.Slice( Position ) );

        Position += 4;

        return value;
    }

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

    public float GetFloat()
    {
        if ( ( Position + 4 ) > Limit )
        {
            throw new IndexOutOfRangeException();
        }

        var value = IsBigEndian
            ? BinaryPrimitives.ReadSingleBigEndian( _memory.Span.Slice( Position ) )
            : BinaryPrimitives.ReadSingleLittleEndian( _memory.Span.Slice( Position ) );

        Position += 4;

        return value;
    }

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

    #endregion Typed Get Methods

    // ========================================================================

    #region Typed Put Methods

    // ========================================================================

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

    #endregion Typed Put Methods

    // ========================================================================

    #region Bulk Get Methods

    // ========================================================================

    public void GetBytes( byte[] dst )
    {
        GetBytes( dst, 0, dst.Length );
    }

    public void GetInts( int[] dst )
    {
        GetInts( dst, 0, dst.Length );
    }

    public void GetShorts( short[] dst )
    {
        GetShorts( dst, 0, dst.Length );
    }

    public void GetFloats( float[] dst )
    {
        GetFloats( dst, 0, dst.Length );
    }

    public void GetBytes( byte[] dst, int dstOffset, int length )
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

    public void GetInts( int[] dst, int dstOffset, int length )
    {
        //TODO:
    }

    public void GetShorts( short[] dst, int dstOffset, int length )
    {
        //TODO:
    }

    public void GetFloats( float[] dst, int dstOffset, int length )
    {
        //TODO:
    }

    #endregion Bulk Get Methods

    // ========================================================================

    #region Bulk Put Methods

    // ========================================================================

    public void PutBytes( byte[] src )
    {
        PutBytes( src, 0, 0, src.Length );
    }

    public void PutBytes( byte[] src, int srcOffset, int numBytes )
    {
        PutBytes( src, srcOffset, Position, numBytes );
    }

    public void PutBytes( byte[] src, int srcOffset, int dstOffset, int numBytes )
    {
        ArgumentNullException.ThrowIfNull( src );

        EnsureCapacity( dstOffset + 1 + numBytes );

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
            throw new ArgumentOutOfRangeException( nameof( numBytes ), "Length and source offset exceed source array bounds." );
        }

        // Check for space in destination buffer using dstOffset
        if ( ( dstOffset + numBytes ) > Capacity )
        {
            throw new IndexOutOfRangeException( "Not enough space in buffer to put the requested length at the given destination offset." );
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

    public void PutInts( int[] src )
    {
        PutInts( src, 0, 0, src.Length );
    }

    public void PutInts( int[] src, int srcOffset, int numBytes )
    {
        PutInts( src, srcOffset, Position, numBytes );
    }

    public void PutInts( int[] src, int srcOffset, int dstOffset, int numBytes )
    {
        //TODO:
    }

    public void PutShorts( short[] src )
    {
        PutShorts( src, 0, 0, src.Length );
    }

    public void PutShorts( short[] src, int srcOffset, int numBytes )
    {
        PutShorts( src, srcOffset, Position, numBytes );
    }

    public void PutShorts( short[] src, int srcOffset, int dstOffset, int numBytes )
    {
        //TODO:
    }

    public void PutFloats( float[] src )
    {
        PutFloats( src, 0, 0, src.Length );
    }

    public void PutFloats( float[] src, int srcOffset, int numBytes )
    {
        PutFloats( src, srcOffset, Position, numBytes );
    }

    public void PutFloats( float[] src, int srcOffset, int dstOffset, int numBytes )
    {
        //TODO:
    }

    #endregion Bulk Put Methods

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

        var newCapacity = Capacity + extraCapacityInBytes;
        Array.Resize( ref _backingArray, newCapacity );

        _memory  = _backingArray.AsMemory();
        Capacity = newCapacity;
    }

    public void EnsureCapacity( int requiredBytes )
    {
        if ( Capacity < requiredBytes )
        {
            Resize( requiredBytes - Capacity );
        }
    }

    public byte[] BackingArray()
    {
        return _backingArray;
    }

    public Memory< byte > Memory()
    {
        return _memory;
    }

    public T[] ToArray()
    {
        var elementCount = Length / _elementSize;
        var result       = new T[ elementCount ];
        MemoryMarshal.Cast< byte, T >( _memory.Span.Slice( 0, Length ) ).CopyTo( result );

        return result;
    }

    public void Clear()
    {
        Array.Clear( _backingArray, 0, Capacity );

        Position = 0;
        Length   = 0;
    }

    public void Flip()
    {
        Limit    = Position;
        Position = 0;
    }

    public void Rewind( int toPosition = 0 )
    {
        Position = toPosition;
    }

    public void Compact()
    {
        var newCapacity = Length;

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
        var remainingElements = ( Limit - Position ) / _elementSize;

        if ( remainingElements <= 0 )
        {
            throw new InvalidOperationException( "Cannot create slice: no remaining data in buffer" );
        }

        var sliceMemory = _memory.Slice( Position, Limit - Position );

        var slicedBuffer = new Buffer< T >( remainingElements );
        slicedBuffer._memory       = sliceMemory;
        slicedBuffer._backingArray = sliceMemory.ToArray();            // Only if you need direct array access
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

        var sliceMemory = _memory.Slice( fromElementIndex * _elementSize, ( ( toElementIndex - fromElementIndex ) + 1 ) * _elementSize );
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
        Guard.ThrowIfNull( _backingArray );

        var newCapacityInBytes = Limit;

        if ( Capacity > newCapacityInBytes )
        {
//            var reductionAmount = Capacity - newCapacityInBytes;
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
                throw new IndexOutOfRangeException( $"Index '{index}' is out of range. Valid range is 0 to {Capacity - 1}." );
            }

            Guard.ThrowIfNull( _backingArray );

            return _backingArray[ index ];
        }
        set
        {
            if ( ( index < 0 ) || ( index >= Capacity ) )
            {
                throw new IndexOutOfRangeException( $"Index '{index}' is out of range. Valid range is 0 to {Capacity - 1}." );
            }

            Guard.ThrowIfNull( _backingArray );

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

    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            // Clear sensitive data
            Array.Clear( _backingArray, 0, _backingArray.Length );
            _backingArray = null!;

            _memory = default;
        }
    }
}

// ========================================================================
// ========================================================================