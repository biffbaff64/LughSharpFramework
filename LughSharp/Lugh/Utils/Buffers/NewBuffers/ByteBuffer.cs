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
        _memory     = _backingArray.AsMemory();
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
        _memory     = _backingArray.AsMemory();
        Capacity    = newCapacity;

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

