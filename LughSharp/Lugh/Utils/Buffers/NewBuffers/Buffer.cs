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

    public bool IsReadOnly  { get; protected set; } // Protected setter
    public bool IsDirect    { get; protected set; } // Protected setter
    public int  Capacity    { get; protected set; } // Protected setter
    public int  Position    { get; set; }
    public int  Limit       { get; set; }
    public int  Length      { get; protected set; } // Protected setter
    public bool IsBigEndian { get; set; }

    // ========================================================================

    protected Buffer( int capacityInBytes )
    {
        Logger.Checkpoint();

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
    // Abstract methods - to be implemented in derived classes for type-specific operations
    //
    public abstract byte GetByte();
    public abstract byte GetByte( int index );
    public abstract void PutByte( byte value );
    public abstract void PutByte( int index, byte value );

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

    // ========================================================================

    /// <summary>
    /// Retrieves this buffer's byte order.
    /// </summary>
    public ByteOrder Order() => IsBigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;

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

