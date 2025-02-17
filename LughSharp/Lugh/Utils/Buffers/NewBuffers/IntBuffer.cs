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

namespace LughSharp.Lugh.Utils.Buffers.NewBuffers;

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

    // ========================================================================

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

    public int GetInt( int index )
    {
        var byteOffset = index * sizeof( int );

        return _byteBufferDelegate.GetInt( byteOffset );
    }

    public void PutInt( int value )
    {
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

    public void PutInt( int index, int value )
    {
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

    // ========================================================================

    /// <inheritdoc cref="ByteBuffer.Resize(int)"/>
    public override void Resize( int extraCapacityInBytes )
    {
        _byteBufferDelegate.Resize( extraCapacityInBytes ); // **1. Delegate Resize to ByteBuffer**

        // **2. Recalculate IntBuffer Capacity in *ints* based on the resized ByteBuffer's byte capacity**
        Capacity = ( int )Math.Ceiling( ( double )_byteBufferDelegate.Capacity / sizeof( int ) );

        // **3. Adjust Limit if it was originally at Capacity** (Optional, but good practice to maintain Limit if it was at full capacity before resize)
        if ( Limit == Capacity -
            ( int )Math.Ceiling( ( double )extraCapacityInBytes / sizeof( int ) ) ) // Check if Limit was at old Capacity
        {
            Limit = Capacity; // Reset Limit to the new Capacity if it was at the old Capacity
        }
    }

    /// <inheritdoc cref="ByteBuffer.Clear()"/>
    public override void Clear()
    {
        _byteBufferDelegate.Clear(); // Delegate to ByteBuffer's Clear implementation
        Length   = 0;                // Reset IntBuffer Length
        Position = 0;                // Reset IntBuffer Position
        Limit    = Capacity;         // Reset IntBuffer Limit
    }

    // ========================================================================

    /// <inheritdoc />
    public override byte GetByte() => _byteBufferDelegate.GetByte();

    /// <inheritdoc />
    public override byte GetByte( int index ) => _byteBufferDelegate.GetByte( index );

    /// <inheritdoc />
    public override void PutByte( byte value ) => _byteBufferDelegate.PutByte( value );

    /// <inheritdoc />
    public override void PutByte( int index, byte value ) => _byteBufferDelegate.PutByte( index, value );

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