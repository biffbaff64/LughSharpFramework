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

namespace LughSharp.Lugh.Utils.Buffers.New;

[PublicAPI]
public class NewBuffer : IDisposable
{
    public const bool READ_ONLY  = true;
    public const bool READ_WRITE = false;
    public const bool DIRECT     = true;
    public const bool NOT_DIRECT = false;

    // ========================================================================
    
    public bool IsBigEndian { get; set; }
    public bool IsReadOnly  { get; set; }
    public bool IsDirect    { get; set; }
    public int  Offset      { get; set; }
    public int  Mark        { get; set; }
    public int  Position    { get; set; }
    public int  Limit       { get; set; }
    
    // ========================================================================

    public NewBuffer()
    {
    }
    
    /// <summary>
    /// Creates a new Buffer object, using the supplied parameters.
    /// By default, Buffers are <see cref="ByteOrder.LittleEndian"/>, Read/Write and
    /// non-direct.
    /// </summary>
    /// <param name="isBigEndian"></param>
    /// <param name="isReadOnly"></param>
    /// <param name="isDirect"></param>
    public NewBuffer( bool isBigEndian = false, bool isReadOnly = false, bool isDirect = false )
    {
        SetBufferStatus( isReadOnly, isDirect );
    }

    /// <summary>
    /// Retrieves this buffer's byte order.
    /// </summary>
    public ByteOrder Order() => IsBigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;

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
    /// Returns a string summarizing the state of this buffer.
    /// </summary>
    /// <returns> A summary string </returns>
    public override string ToString()
    {
        return $"{GetType().Name} [pos={Position} lim={Limit} cap={Capacity}]";
    }
    
    // ========================================================================

    public virtual int Capacity => 0;
    
    // ========================================================================

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    public static void Dispose( bool disposing )
    {
        if ( disposing )
        {
        }
    }
}