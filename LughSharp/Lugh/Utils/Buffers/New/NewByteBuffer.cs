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

namespace LughSharp.Lugh.Utils.Buffers.New;

[PublicAPI]
public class NewByteBuffer : IDisposable
{
    public const bool READ_ONLY  = true;
    public const bool READ_WRITE = false;
    public const bool DIRECT     = true;
    public const bool NOT_DIRECT = false;

    // ========================================================================

    protected const bool IS_READ_ONLY_DEFAULT = false;
    protected const bool IS_DIRECT_DEFAULT    = false;

    // ========================================================================

    public bool           IsReadOnly { get; set; }
    public bool           IsDirect   { get; set; }
    public int            Length     { get; set; }
    public Memory< byte > Memory     { get; set; }

    // ========================================================================

    private byte[] _byteBuffer;
    private int    _position;
    private int    _limit;

    // ========================================================================

    /// <summary>
    /// Creates a new ByteBuffer with the specified capacity.
    /// </summary>
    /// <param name="capacityInBytes"> The number of bytes to be made available in the buffer. </param>
    public NewByteBuffer( int capacityInBytes )
    {
        _byteBuffer = new byte[ capacityInBytes ];

        Memory      = _byteBuffer.AsMemory();
        IsBigEndian = !BitConverter.IsLittleEndian;
        IsReadOnly  = IS_READ_ONLY_DEFAULT;
        IsDirect    = IS_DIRECT_DEFAULT;

        Capacity = capacityInBytes;
        Limit    = capacityInBytes;
        Length   = 0;
        Position = 0;

        SetBufferStatus( READ_WRITE, NOT_DIRECT );
    }

    // ========================================================================

    /// <summary>
    /// Gets the Byte from the backing array at the current <see cref="Position"/>.
    /// </summary>
    public byte GetByte()
    {
        if ( ( Position >= Limit ) || ( Position < 0 ) ) // Check against Limit now for sequential read
        {
            throw new IndexOutOfRangeException( "ByteBuffer position out of range or beyond limit." );
        }

        var value = Memory.Span[ Position ];

        Position++;
        
        return value;
    }

    /// <summary>
    /// Gets the Byte from the backing array at the specified index.
    /// </summary>
    /// <param name="index"> The buffer position from which to return the byte value. </param>
    public byte GetByte( int index )
    {
        if ( ( index < 0 ) || ( index > Limit ) )
        {
            throw new IndexOutOfRangeException( "Buffer position out of range." );
        }

        return Memory.Span[ index ];
    }

    /// <summary>
    /// Puts the provided Byte into the backing array at the current <see cref="Position"/>.
    /// Position is then updated.
    /// </summary>
    /// <param name="value"> The value to put. </param>
    public void PutByte( byte value )
    {
        if ( Position >= Capacity )
        {
            throw new BufferOverflowException();
        }

        Memory.Span[ Position ] = value;
        Position++; // Increment position AFTER write
    }

    /// <summary>
    /// Puts the provided Byte into the backing array at the specified index.
    /// </summary>
    /// <param name="index"> The position in the buffer at which to put the byte value. </param>
    /// <param name="value"> The value to put. </param>
    public void PutByte( int index, byte value )
    {
        if ( ( index < 0 ) || ( index >= Capacity ) )
        {
            throw new IndexOutOfRangeException( "Index out of range." );
        }

        Memory.Span[ index ] = value;
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Gets a Short value from the backing array at the current <see cref="Position"/>.
    /// </summary>
    public short GetShort()
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadInt16BigEndian( Memory.Span.Slice( Position ) )
            : BinaryPrimitives.ReadInt16LittleEndian( Memory.Span.Slice( Position ) );
    }

    /// <summary>
    /// Gets a Short value from the backing array at the specified index.
    /// </summary>
    public short GetShort( int index )
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadInt16BigEndian( Memory.Span.Slice( index ) )
            : BinaryPrimitives.ReadInt16LittleEndian( Memory.Span.Slice( index ) );
    }

    /// <summary>
    /// Puts the provided Short value into the backing array at the current <see cref="Position"/>.
    /// </summary>
    /// <param name="value"> The value to put. </param>
    public void PutShort( short value )
    {
        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteInt16BigEndian( Memory.Span.Slice( Position ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian( Memory.Span.Slice( Position ), value );
        }
    }

    /// <summary>
    /// Puts the provided Short value into the backing array at the specified index.
    /// </summary>
    /// <param name="index"> The index. </param>
    /// <param name="value"> The value to put. </param>
    public void PutShort( int index, short value )
    {
        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteInt16BigEndian( Memory.Span.Slice( index ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian( Memory.Span.Slice( index ), value );
        }

        if ( index >= Position )
        {
            Length = index + 1;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Gets an Int32 value from the backing array at the current <see cref="Position"/>.
    /// </summary>
    public int GetInt()
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadInt32BigEndian( Memory.Span.Slice( Position ) )
            : BinaryPrimitives.ReadInt32LittleEndian( Memory.Span.Slice( Position ) );
    }

    /// <summary>
    /// Gets an Int32 value from the backing array at the specified index.
    /// </summary>
    public int GetInt( int index )
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadInt32BigEndian( Memory.Span.Slice( index ) )
            : BinaryPrimitives.ReadInt32LittleEndian( Memory.Span.Slice( index ) );
    }

    /// <summary>
    /// Puts the provided Int32 value into the backing array at the current <see cref="Position"/>.
    /// </summary>
    /// <param name="value"> The value to put. </param>
    public void PutInt( int value )
    {
        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteInt32BigEndian( Memory.Span.Slice( Position ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian( Memory.Span.Slice( Position ), value );
        }
    }

    /// <summary>
    /// Puts the provided Int32 into the backing array at the specified index.
    /// </summary>
    /// <param name="index"> The index. </param>
    /// <param name="value"> The value to put. </param>
    public void PutInt( int index, int value )
    {
        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteInt32BigEndian( Memory.Span.Slice( index ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian( Memory.Span.Slice( index ), value );
        }

        if ( index >= Position )
        {
            Length = index + 1;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Gets a Float value from the backing array at the current <see cref="Position"/>.
    /// </summary>
    public float GetFloat()
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadSingleBigEndian( Memory.Span.Slice( Position ) )
            : BinaryPrimitives.ReadSingleLittleEndian( Memory.Span.Slice( Position ) );
    }

    /// <summary>
    /// Gets a Float value from the backing array at the specified index.
    /// </summary>
    public float GetFloat( int index )
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadSingleBigEndian( Memory.Span.Slice( index ) )
            : BinaryPrimitives.ReadSingleLittleEndian( Memory.Span.Slice( index ) );
    }

    /// <summary>
    /// Puts the provided Float into the backing array at the current <see cref="Position"/>.
    /// </summary>
    /// <param name="value"> The value to put. </param>
    public void PutFloat( float value )
    {
        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteSingleBigEndian( Memory.Span.Slice( Position ), value );
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian( Memory.Span.Slice( Position ), value );
        }
    }

    /// <summary>
    /// Puts the provided Float into the backing array at the specified index.
    /// </summary>
    /// <param name="index"> The index. </param>
    /// <param name="value"> The value to put. </param>
    public void PutFloat( int index, float value )
    {
        if ( IsBigEndian )
        {
            BinaryPrimitives.WriteSingleBigEndian( Memory.Span.Slice( index ), value );
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian( Memory.Span.Slice( index ), value );
        }

        if ( index >= Position )
        {
            Length = index + 1;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Resize the backing array by taking the existing capacity and adding the requested
    /// extra capacity.
    /// </summary>
    /// <param name="extraCapacityInBytes">
    /// The number of extra BYTES to increase Capacity by. The backing array for all buffers is
    /// always a <see cref="NewByteBuffer"/> so make sure to translate the extra capacity
    /// requested into the correct number of bytes to add before calling Resize.
    /// </param>
    public void Resize( int extraCapacityInBytes )
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
        Array.Copy( _byteBuffer, newBackingArray, _byteBuffer.Length );

        _byteBuffer = newBackingArray;
        Memory      = _byteBuffer.AsMemory();
        Capacity    = newCapacity;

        // **Position Handling:** Keep position within the new bounds.
        if ( Position > Capacity )
        {
            Position = Capacity; // Clamp position to the new capacity if it was beyond
        }
    }

    /// <summary>
    /// Clear the buffer.
    /// </summary>
    public void Clear()
    {
        Array.Clear( _byteBuffer, 0, _byteBuffer.Length );

        Position = 0;
        Length   = 0;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="extraLength"></param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public virtual void UpdateLength( int extraLength )
    {
        // If writing at or beyond current Length, update Length
        if ( extraLength >= Length )
        {
            // Length becomes index + 1 (since index is 0-based)
            Length = extraLength + 1;
        }
        else if ( extraLength >= Capacity )
        {
            throw new IndexOutOfRangeException( "Buffer index out of range." );
        }
    }

    /// <summary>
    /// Sets the buffer status, determining whether the buffer is in read/write mode
    /// and whether it is a direct buffer.
    /// </summary>
    /// <param name="readWrite">Indicates if the buffer is in read/write mode.</param>
    /// <param name="direct">Indicates if the buffer is a direct buffer.</param>
    protected internal void SetBufferStatus( bool readWrite, bool direct )
    {
        IsReadOnly = readWrite;
        IsDirect   = direct;
    }

    /// <summary>
    /// This sets the <see cref="Limit"/> to the current value of Position. At this point,
    /// Position typically indicates the position after the last byte written. So, setting
    /// Limit to Position effectively marks the extent of the valid data that has been written
    /// into the buffer. <see cref="Position"/> is then reset to 0. Now, when you start using
    /// GetByte() or other read methods on the ByteBuffer, you will begin reading from the
    /// very beginning of the data (from index 0) up to the Limit.
    /// </summary>
    public virtual void Flip()
    {
        Limit    = Position; // Set Limit to current Position
        Position = 0;        // Reset Position to 0
    }

    // ========================================================================

    /// <summary>
    /// Flag to indicate byte order (Big-Endian or Little-Endian).
    /// </summary>
    public bool IsBigEndian { get; set; }

    /// <summary>
    /// Retrieves this buffer's byte order.
    /// </summary>
    public ByteOrder Order() => IsBigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;

    /// <summary>
    /// Total allocated byte size (read-only after constructor). Can be modified by <see cref="Resize(int)"/>.
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    /// Current position within the buffer. Any new values will be added at this position.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the new position is no longer within the buffer bounds.
    /// </exception>
    public int Position
    {
        get => _position;
        set
        {
            if ( ( value < 0 ) || ( value > Capacity ) ) // Ensure position is within bounds
            {
                throw new ArgumentOutOfRangeException( nameof( Position ), "Position must be within buffer capacity." );
            }

            _position = value;
        }
    }

    /// <summary>
    /// Boundary for read operations (read-write). Set to Capacity initially (or 0), set to
    /// the value held in <see cref="Position"/> by <see cref="Flip()"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"> If Limit exceeds the buffer capacity. </exception>
    public int Limit
    {
        get => _limit;
        set
        {
            if ( ( value < 0 ) || ( value > Capacity ) )
            {
                throw new ArgumentOutOfRangeException( nameof( Limit ), "Limit must be within buffer capacity." );
            }

            _limit = value;
        }
    }

    // ========================================================================

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or
    /// resetting unmanaged resources.
    /// </summary>
    public void Dispose( bool disposing )
    {
        if ( disposing )
        {
            Array.Clear( _byteBuffer, 0, _byteBuffer.Length );

            this._byteBuffer = null!;
            this.Memory      = null;
        }
    }
}