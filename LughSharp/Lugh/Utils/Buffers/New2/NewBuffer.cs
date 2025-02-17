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

namespace LughSharp.Lugh.Utils.Buffers.New2;

[PublicAPI]
public class NewBuffer : IDisposable
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
    public Memory< byte > Memory     { get; set; }

    // ========================================================================

    public NewBuffer()
    {
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

    // ========================================================================
    
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
        }
    }
}
