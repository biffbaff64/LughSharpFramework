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

namespace LughSharp.Lugh.Utils.Buffers.New;

/// <summary>
/// Provides a type-safe view of an underlying ByteBuffer, specialized for Int32 values.
/// This buffer holds a reference to a ByteBuffer instance (_byteBuffer), and does
/// not have its own backing arrays.
/// </summary>
[PublicAPI]
public class NewIntBuffer
{
    private NewByteBuffer _byteBuffer;

    private int _length;

    // ========================================================================

    /// <summary>
    /// Creates a new IntBuffer with the specified capacity.
    /// </summary>
    /// <param name="capacityInInts">
    /// The number of ints to be made available in the buffer. As the backing buffer is a
    /// ByteBuffer, this capacity will need to be translated into bytes from ints.
    /// </param>
    public NewIntBuffer( int capacityInInts )
    {
        var byteCapacity = capacityInInts * sizeof( int );

        _byteBuffer = new NewByteBuffer( byteCapacity );
        _byteBuffer.Position = 0;

        Length   = 0;
        Capacity = capacityInInts;
        
        _byteBuffer.SetBufferStatus( NewByteBuffer.READ_WRITE, NewByteBuffer.NOT_DIRECT );
    }

    // ========================================================================

    /// <inheritdoc cref="NewByteBuffer.GetInt()"/>
    public int GetInt()
    {
        var byteOffset = _byteBuffer.Position;

        if ( ( ( byteOffset + sizeof( int ) ) > _byteBuffer.Limit ) || ( byteOffset < 0 ) )
        {
            throw new IndexOutOfRangeException( "IntBuffer position out of range" );
        }

        var value = _byteBuffer.GetInt( byteOffset );

        _byteBuffer.Position += sizeof( int );

        return value;
    }

    /// <inheritdoc cref="NewByteBuffer.GetInt(int)"/>
    public int GetInt( int index )
    {
        var byteOffset = index  * sizeof( int );
        
        return _byteBuffer.GetInt( byteOffset );
    }

    /// <inheritdoc cref="NewByteBuffer.PutInt(int)"/>
    public void PutInt( int value )
    {
        var intOffset = _byteBuffer.Position;
        
        if ( ( intOffset + sizeof( int ) ) > _byteBuffer.Capacity )
        {
            throw new BufferOverflowException( "IntBuffer overflow (ByteBuffer capacity reached)" );
        }

        _byteBuffer.PutInt( intOffset, value );
        _byteBuffer.Position += sizeof( int );

        var intIndex = _byteBuffer.Position / sizeof( int );

        if ( intIndex > Length )
        {
            Length = intIndex;
        }
    }
    
    /// <inheritdoc cref="NewByteBuffer.PutInt(int,int)"/>
    public void PutInt( int index, int value )
    {
        var byteOffset = index * sizeof( int );
        
        if (( index < 0 ) || ( index >= Capacity ))
        {
            throw new IndexOutOfRangeException();
        }

        _byteBuffer.PutInt( byteOffset, value );

        if ( index > Length )
        {
            Length = index + 1;
        }
    }

    // ========================================================================
    
    /// <summary>
    /// Clear the buffer.
    /// </summary>
    public void Clear()
    {
        Length = 0;
        _byteBuffer.Clear();
    }

    /// <summary>
    /// This sets the <see cref="NewByteBuffer.Limit"/> to the current value of Position. At this
    /// point, Position typically indicates the position after the last byte written. So, setting
    /// Limit to Position effectively marks the extent of the valid data that has been written into
    /// the buffer. <see cref="NewByteBuffer.Position"/> is then reset to 0. Now, when you start using
    /// GetByte() or other read methods on the ByteBuffer, you will begin reading from the very
    /// beginning of the data (from index 0) up to the Limit.
    /// </summary>
    public void Flip()
    {
        _byteBuffer.Flip();
    }

    /// <inheritdoc cref="NewByteBuffer.IsBigEndian"/>
    public bool IsBigEndian => _byteBuffer.IsBigEndian;
    
    /// <summary>
    /// Boundary for read operations (read-write). Set to Capacity initially (or 0), set to
    /// the value held in <see cref="NewByteBuffer.Position"/> by <see cref="Flip()"/>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"> If Limit exceeds the buffer capacity. </exception>
    public int Length
    {
        get => _length;
        private set
        {
            _length = value;

            if ( _length < 0 )
            {
                throw new IndexOutOfRangeException( "Length cannot be < 0" );
            }

            if ( _length > Capacity )
            {
                throw new IndexOutOfRangeException( "Length cannot be > Capacity" );
            }
        }
    }

    /// <summary>
    /// Total capacity in units of <c>Int32</c> type (read-only, calculated from ByteBuffer.Capacity).
    /// </summary>
    public int Capacity { get; private set; }
}