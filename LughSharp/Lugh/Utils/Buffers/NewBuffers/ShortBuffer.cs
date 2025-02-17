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
/// Provides a type-safe view of an underlying ByteBuffer, specialized for short values.
/// This buffer holds a reference to a ByteBuffer instance (_byteBuffer), and does
/// not have its own backing arrays.
/// Properties <see cref="Buffers.ByteBuffer.Limit"/> and <see cref="Buffers.ByteBuffer.Capacity"/> are
/// delegated to the <see cref="Buffers.ByteBuffer"/> class, as it is that cass which handles
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

    // ========================================================================

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

    public short GetShort( int index )
    {
        var byteOffset = index * sizeof( short );

        return _byteBufferDelegate.GetShort( byteOffset );
    }

    public void PutShort( short value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

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

    public void PutShort( int index, short value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

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

    // ========================================================================

    /// <inheritdoc cref="ByteBuffer.Resize(int)"/>
    public override void Resize( int extraCapacityInBytes )
    {
        _byteBufferDelegate.Resize(extraCapacityInBytes); // **1. Delegate Resize to ByteBuffer**

        // **2. Recalculate ShortBuffer Capacity in *shorts* based on the resized ByteBuffer's byte capacity**
        Capacity = (int)Math.Ceiling((double)_byteBufferDelegate.Capacity / sizeof(short));

        // **3. Adjust Limit if it was originally at Capacity** (Optional, but good practice to maintain Limit if it was at full capacity before resize)
        if (Limit == Capacity - (int)Math.Ceiling((double)extraCapacityInBytes / sizeof(short))) // Check if Limit was at old Capacity
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

    /// <inheritdoc />
    public override int Remaining()
    {
        return ( Limit - Position ) / sizeof( short );
    }

    // ========================================================================

    /// <inheritdoc />
    public override byte GetByte() => _byteBufferDelegate.GetByte();

    /// <inheritdoc />
    public override byte GetByte( int index ) => _byteBufferDelegate.GetByte( index );

    /// <inheritdoc />
    public override void PutByte( byte value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        _byteBufferDelegate.PutByte( value );
    }

    /// <inheritdoc />
    public override void PutByte( int index, byte value )
    {
        if ( IsReadOnly ) throw new GdxRuntimeException( "Cannot write to a read-only buffer." );

        _byteBufferDelegate.PutByte( index, value );
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