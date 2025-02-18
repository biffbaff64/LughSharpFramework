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

namespace LughSharp.Lugh.Utils.Buffers.NewBuffers;

/// <summary>
/// The base class for buffers. This holds all the essential shared data for IntBuffers, ShortBuffers,
/// ByteBuffers, FloatBuffers, and any other buffer types.
/// This class DOES NOT contain any buffer arrays, merely propereties for defining capacity,
/// length, limit, etc. Thiose buffer arrays must be defined in extending classes.
/// </summary>
[PublicAPI]
public abstract class Buffer : IDisposable
{
    public const bool READ_ONLY  = true;
    public const bool READ_WRITE = false;
    public const bool DIRECT     = true;
    public const bool NOT_DIRECT = false;

    // ========================================================================

    protected const bool IS_READ_ONLY_DEFAULT = false;
    protected const bool IS_DIRECT_DEFAULT    = false;

    // ========================================================================

    public bool IsReadOnly  { get; protected set; }
    public bool IsDirect    { get; protected set; }
    public int  Capacity    { get; protected set; }
    public int  Position    { get; set; }
    public int  Limit       { get; set; }
    public int  Length      { get; protected set; }
    public bool IsBigEndian { get; set; }

    private int _markPosition = -1;

    // ========================================================================

    /// <summary>
    /// Creates a new Buffer object 
    /// </summary>
    /// <param name="capacityInBytes"></param>
    protected Buffer( int capacityInBytes )
    {
        Capacity = capacityInBytes;
        Limit    = capacityInBytes;
        Length   = 0;
        Position = 0;

        IsBigEndian = !BitConverter.IsLittleEndian;
        IsReadOnly  = IS_READ_ONLY_DEFAULT;
        IsDirect    = IS_DIRECT_DEFAULT;

        SetBufferStatus( READ_WRITE, NOT_DIRECT ); // Set default status
    }

    // ========================================================================
    // Byte type abstract methods - to be implemented in derived classes for type-specific operations
    //
    public abstract byte GetByte();
    public abstract byte GetByte( int index );
    public abstract void PutByte( byte value );
    public abstract void PutByte( int index, byte value );
    public abstract void GetBytes( byte[] result, int offset, int length );
    public abstract void PutBytes( byte[] src, int srcOffset, int dstOffset, int length );
    public abstract void GetBytes( byte[] byteArray );
    public abstract void PutBytes( byte[] byteArray );

    // ========================================================================

    /// <summary>
    /// Resize the backing array by taking the existing capacity and adding the requested
    /// extra capacity.
    /// </summary>
    /// <param name="extraCapacityInBytes">
    /// The number of extra BYTES to increase Capacity by. The backing array for all buffers is
    /// always a <see cref="ByteBuffer"/> so make sure to translate the extra capacity
    /// requested into the correct number of bytes to add before calling Resize.
    /// </param>
    public virtual void Resize( int extraCapacityInBytes )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Clear the buffer.
    /// </summary>
    public virtual void Clear()
    {
        throw new NotImplementedException();
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

    /// <summary>
    /// Rewinds this buffer. The position is set to zero and the mark is discarded.
    /// Invoke this method before a sequence of <c>Put</c> or <c>Get</c> operations, assuming
    /// that the limit has already been set appropriately.
    /// <br/>
    /// <para>
    /// <b>Usefulness:</b>
    /// <li>
    /// Re-reading Data: Allows you to reread the same data from the buffer multiple times without
    /// needing to create a new buffer or copy data. This is very efficient when you need to process
    /// the same data in different ways or multiple passes.
    /// </li>
    /// <li>
    /// Reprocessing: Useful in parsing, data analysis, or network protocols where you might need to
    /// re-examine the beginning of a buffer after some initial processing.
    /// </li>
    /// </para>
    /// </summary>
    public virtual void Rewind()
    {
        Position = 0;
    }

    /// <summary>
    /// Concept: Remaining() returns the number of elements (or bytes in ByteBuffer) remaining in the
    /// buffer between the current Position and the Limit. This tells you how much data is available
    /// to be read from the current position onward.
    /// <br/>
    /// <para>
    /// <b>Usefulness:</b>
    /// <li>
    /// Read Boundary Check: Essential for preventing IndexOutOfRangeException errors when reading
    /// sequentially. Before a Get...() operation, you can call Remaining() to check if there are enough
    /// elements/bytes left to read.
    /// </li>
    /// <li>
    /// Loop Control: Useful for loops that process data from the buffer sequentially. The loop can
    /// continue as long as Remaining() is greater than zero (or greater than the size of the element
    /// being read).
    /// </li>
    /// <li>
    /// Size Indication: Provides a dynamic indication of the amount of unread data within the buffer's
    /// current read window (between Position and Limit).
    /// </li>
    /// </para>
    /// </summary>
    public abstract int Remaining();

    /// <summary>
    /// Returns <c>true</c> if, and only if, there is at least one element remaining in this buffer.
    /// </summary>
    public virtual bool HasRemaining()
    {
        return Position < Limit;
    }

    /// <summary>
    /// Retrieves this buffer's byte order.
    /// </summary>
    public ByteOrder Order() => IsBigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;

    /// <summary>
    /// Concept: Shrink() (or sometimes called Compact() in other buffer APIs) is about reducing the
    /// buffer's Capacity to be closer to the amount of data it actually holds (up to the Limit). It's
    /// often used to reclaim memory if a buffer was allocated with a large capacity but is now holding
    /// a smaller amount of data.
    /// The data currently between position 0 and Limit should be preserved. The Position and Limit might
    /// need to be adjusted after shrinking to remain valid within the new, smaller capacity.
    /// <br/>
    /// <para>
    /// <b>Usefulness:</b>
    /// <li>
    /// Memory Optimization: Important in memory-constrained environments or when dealing with long-lived
    /// buffers that might hold varying amounts of data over time. Reduces memory footprint by releasing
    /// unused capacity.
    /// </li>
    /// </para>
    /// </summary>
    public virtual void Shrink()
    {
        // Default implementation - potentially shrink to Limit?
        // Or leave empty and make subclasses override?

        var newCapacityInBytes = Limit; // Shrink to Limit (in bytes - for ByteBuffer)

        if ( Capacity > newCapacityInBytes )
        {
            Resize( newCapacityInBytes - Capacity ); // Resize by the *difference* to shrink

            Limit = newCapacityInBytes;

            Rewind(); // Rewind after shrink for typical shrink behavior
        }
    }

    /// <summary>
    /// Saves the current <see cref="Position"/> of the buffer as a "mark."
    /// </summary>
    public virtual void Mark()
    {
        _markPosition = Position;
    }

    /// <summary>
    /// Discards any previously set mark position by setting its value to -1;
    /// </summary>
    public virtual void DiscardMark()
    {
        _markPosition = -1;
    }
    
    /// <summary>
    /// Sets the Position back to the last saved "mark." The mark is typically discarded after
    /// Reset() is called. If Mark() has not been called, Reset() might have undefined behavior.
    /// <br/>
    /// <para>
    /// <b>Usefulness:</b>
    /// <li>
    /// Backtracking/Navigation: Allows you to temporarily explore data in the buffer (read ahead),
    /// and then easily return to a previously saved position to continue processing from there.
    /// Useful in parsing scenarios where you might need to "peek ahead" but then backtrack if a
    /// certain condition is not met.
    /// </li>
    /// <li>
    /// State Management: Provides a way to save and restore the read/write position within the buffer.
    /// </li>
    /// </para>
    /// </summary>
    public virtual void Reset()
    {
        if ( _markPosition == -1 )
        {
            throw new InvalidOperationException( "Mark not set." ); // Or decide on different behavior if no mark
        }

        Position      = _markPosition; // Restore Position to the marked position
        _markPosition = -1;            // Clear the mark after reset (one-time use mark)
    }

    // ========================================================================

    /// <summary>
    /// Sets the buffer status, determining whether the buffer is in read/write mode
    /// and whether it is a direct buffer.
    /// </summary>
    /// <param name="readWrite">Indicates if the buffer is in read/write mode.</param>
    /// <param name="direct">Indicates if the buffer is a direct buffer.</param>
    protected void SetBufferStatus( bool readWrite, bool direct )
    {
        IsReadOnly = readWrite;
        IsDirect   = direct;
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
    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
        }
    }
}