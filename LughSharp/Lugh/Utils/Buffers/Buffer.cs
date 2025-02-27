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

namespace LughSharp.Lugh.Utils.Buffers;

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

    public const int DEFAULT_MAC_100MB = 100 * 1024 * 1024;
    public const int DEFAULT_MAC_500MB = 500 * 1024 * 1024;
    public const int DEFAULT_MAC_1GB   = 1000 * 1024 * 1024;
    public const int DEFAULT_MAC_2GB   = 2000 * 1024 * 1024;

    // ========================================================================

    protected const bool IS_READ_ONLY_DEFAULT = false;
    protected const bool IS_DIRECT_DEFAULT    = false;

    private int _markPosition       = -1;
    private int _maxAllowedCapacity = DEFAULT_MAC_1GB;

    // ========================================================================

    internal Buffer()
    {
    }

    /// <summary>
    /// Creates a new Buffer object
    /// </summary>
    /// <param name="capacityInBytes"></param>
    protected Buffer( int capacityInBytes )
    {
        IsBigEndian = !BitConverter.IsLittleEndian;
        IsReadOnly  = IS_READ_ONLY_DEFAULT;
        IsDirect    = IS_DIRECT_DEFAULT;

        Capacity = capacityInBytes;
        Limit    = capacityInBytes;
        Length   = 0;
        Position = 0;

        SetBufferStatus( READ_WRITE, NOT_DIRECT ); // Set default status
    }

    // ========================================================================

    public bool IsReadOnly        { get; protected set; }
    public bool IsDirect          { get; protected set; }
    public int  Capacity          { get; protected set; }
    public int  Position          { get; set; }
    public int  Limit             { get; set; }
    public int  Length            { get; protected set; }
    public bool IsBigEndian       { get; set; }
    public bool AutoResizeEnabled { get; set; } = true;

    // ========================================================================

    /// <summary>
    /// A safeguard against uncontrolled auto-resizing. Without a maximum, a buffer could
    /// theoretically grow to consume all available memory, potentially leading to out-of-
    /// memory errors or performance issues.  MaxAllowedCapacity sets a reasonable limit.
    /// <br></br>
    /// <para>
    ///     <b>Factors to Consider when Choosing a a value for MaxAllowedCapacity:</b>
    /// </para>
    /// <para>
    ///     <b>1. Typical Use Cases and Data Sizes:</b>
    ///     <li>
    ///         Small Buffers (Kilobytes to a few Megabytes): If your library is primarily intended
    ///         for handling relatively small data structures, network packets, or configuration data,
    ///         a smaller MaxAllowedCapacity might be appropriate.
    ///     </li>
    ///     <li>
    ///         Medium Buffers (Megabytes to hundreds of Megabytes): For applications dealing with moderate-
    ///         sized files, images, or intermediate data processing, a medium-sized limit might be suitable.
    ///     </li>
    ///     <li>
    ///         Large Buffers (Gigabytes or more): If your project is designed for high-performance scenarios,
    ///         large data sets, or memory-intensive operations (e.g., large file processing, in-memory databases,
    ///         scientific computing), a larger MaxAllowedCapacity or even no default limit (relying on system
    ///         memory limits) might be considered (with caution).
    ///     </li>
    ///     <br></br>
    ///     <b>2. Memory Constraints of Target Environments:</b>
    ///     <li>
    ///         Resource-Constrained Environments (Mobile, Embedded, IoT): In environments with limited RAM, a
    ///         smaller MaxAllowedCapacity is crucial to prevent out-of-memory errors and ensure efficient
    ///         resource utilization. Defaults in the range of 100MB to 500MB or even lower might be more
    ///         appropriate.
    ///     </li>
    ///     <li>
    ///         Desktop/Server Environments: On systems with more abundant RAM, you can afford to set a higher
    ///         value, but still want a reasonable limit to prevent runaway memory usage in case of programming
    ///         errors or unexpected data growth. Defaults in the range of 1GB to a few GB might be more suitable.
    ///     </li>
    ///     <br></br>
    ///     <b>3. Performance Implications of Resizing:</b>
    ///     <li>
    ///         Frequent resizing, especially of very large buffers, can have performance overhead due to memory
    ///         allocation and data copying. A MaxAllowedCapacity helps to put a bound on how large a buffer can
    ///         grow and potentially limits the worst-case resizing overhead.
    ///     </li>
    ///     <br></br>
    ///     <b>4. Safety and Preventing Runaway Memory Usage:</b>
    ///     <li>
    ///         The primary purpose of MaxAllowedCapacity is to act as a safety net and prevent uncontrolled
    ///         memory growth. A reasonable default should provide this safety without being overly restrictive
    ///         in typical use cases.
    ///     </li>
    /// </para>
    /// <para>
    ///     Note: When setting this value, properties <see cref="Capacity" /> and <see cref="Limit" />
    ///     will be truncated to the new max allowed capacity if they are currently greater.
    ///     If property <see cref="Position" /> is currently greater than the new Limit, it will be reset
    ///     to zero.
    /// </para>
    /// </summary>
    public int MaxAllowedCapacity
    {
        get => _maxAllowedCapacity;
        set
        {
            ArgumentOutOfRangeException.ThrowIfNegative( value );

            _maxAllowedCapacity = value;
            Capacity            = Math.Min( Capacity, _maxAllowedCapacity );
            Limit               = Math.Min( Limit, _maxAllowedCapacity );

            if ( Position > Limit )
            {
                Position = 0;
            }
        }
    }

    // ========================================================================

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
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
    /// Ensures that the buffer has at least the specified capacity (in bytes).
    /// Resizes the buffer if necessary.
    /// </summary>
    /// <param name="requiredCapacityInBytes">The minimum capacity (in bytes) required.</param>
    public virtual void EnsureCapacity( int requiredCapacityInBytes )
    {
        if ( AutoResizeEnabled )
        {
            if ( Capacity < requiredCapacityInBytes )
            {
                var extraCapacityNeeded = requiredCapacityInBytes - Capacity;

                Resize( extraCapacityNeeded ); // Use existing Resize method to increase capacity
            }
        }
        else
        {
            throw new BufferOverflowException( "Buffer Overflow!" );
        }
    }

    /// <summary>
    /// Resize the backing array by taking the existing capacity and adding the requested
    /// extra capacity.
    /// </summary>
    /// <param name="extraCapacityInBytes">
    /// The number of extra BYTES to increase Capacity by. The backing array for all buffers is
    /// always a <see cref="ByteBuffer" /> so make sure to translate the extra capacity
    /// requested into the correct number of bytes to add before calling Resize.
    /// </param>
    public virtual void Resize( int extraCapacityInBytes )
    {
        throw new GdxRuntimeException( "Resize() is abstract in base class, use extending class!" );
    }

    /// <summary>
    /// Clear the buffer.
    /// </summary>
    public virtual void Clear()
    {
        throw new GdxRuntimeException( "Clear() is abstract in base class, use extending class!" );
    }

    /// <summary>
    /// This sets the <see cref="Limit" /> to the current value of Position. At this point,
    /// Position typically indicates the position after the last byte written. So, setting
    /// Limit to Position effectively marks the extent of the valid data that has been written
    /// into the buffer. <see cref="Position" /> is then reset to 0. Now, when you start using
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
    /// <br />
    /// <para>
    ///     <b>Usefulness:</b>
    ///     <li>
    ///         Re-reading Data: Allows you to reread the same data from the buffer multiple times without
    ///         needing to create a new buffer or copy data. This is very efficient when you need to process
    ///         the same data in different ways or multiple passes.
    ///     </li>
    ///     <li>
    ///         Reprocessing: Useful in parsing, data analysis, or network protocols where you might need to
    ///         re-examine the beginning of a buffer after some initial processing.
    ///     </li>
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
    /// <br />
    /// <para>
    ///     <b>Usefulness:</b>
    ///     <li>
    ///         Read Boundary Check: Essential for preventing IndexOutOfRangeException errors when reading
    ///         sequentially. Before a Get...() operation, you can call Remaining() to check if there are enough
    ///         elements/bytes left to read.
    ///     </li>
    ///     <li>
    ///         Loop Control: Useful for loops that process data from the buffer sequentially. The loop can
    ///         continue as long as Remaining() is greater than zero (or greater than the size of the element
    ///         being read).
    ///     </li>
    ///     <li>
    ///         Size Indication: Provides a dynamic indication of the amount of unread data within the buffer's
    ///         current read window (between Position and Limit).
    ///     </li>
    /// </para>
    /// </summary>
    public abstract int Remaining();

    /// <summary>
    /// Prepares a buffer for subsequent writing after some data has been read from it. Primarily
    /// used in scenarios where you are processing data in chunks or cycles, and you need to retain
    /// any unprocessed data while making space for new data at the end of the buffer.
    /// </summary>
    public abstract void Compact();

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
    public ByteOrder Order()
    {
        return IsBigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
    }

    /// <summary>
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public virtual Buffer Order( ByteOrder order )
    {
        IsBigEndian = order == ByteOrder.BigEndian;

        return this;
    }

    /// <summary>
    /// Concept: Shrink() (or sometimes called Compact() in other buffer APIs) is about reducing the
    /// buffer's Capacity to be closer to the amount of data it actually holds (up to the Limit). It's
    /// often used to reclaim memory if a buffer was allocated with a large capacity but is now holding
    /// a smaller amount of data.
    /// The data currently between position 0 and Limit should be preserved. The Position and Limit might
    /// need to be adjusted after shrinking to remain valid within the new, smaller capacity.
    /// <br />
    /// <para>
    ///     <b>Usefulness:</b>
    ///     <li>
    ///         Memory Optimization: Important in memory-constrained environments or when dealing with long-lived
    ///         buffers that might hold varying amounts of data over time. Reduces memory footprint by releasing
    ///         unused capacity.
    ///     </li>
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
    /// Saves the current <see cref="Position" /> of the buffer as a "mark."
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
    /// <br />
    /// <para>
    ///     <b>Usefulness:</b>
    ///     <li>
    ///         Backtracking/Navigation: Allows you to temporarily explore data in the buffer (read ahead),
    ///         and then easily return to a previously saved position to continue processing from there.
    ///         Useful in parsing scenarios where you might need to "peek ahead" but then backtrack if a
    ///         certain condition is not met.
    ///     </li>
    ///     <li>
    ///         State Management: Provides a way to save and restore the read/write position within the buffer.
    ///     </li>
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

    /// <summary>
    /// In extendiong classes, this method returns the backing array.
    /// </summary>
    /// <returns></returns>
    public virtual object[] ToArray()
    {
        throw new NotImplementedException();
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