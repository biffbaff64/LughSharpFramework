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

namespace LughSharp.Lugh.Utils.Buffers.New;

[PublicAPI]
public class NewByteBuffer : NewBuffer
{
    public Memory< byte > Memory { get; set; }

    // ========================================================================

    private byte[] _backingArray;

    // ========================================================================
    // ========================================================================

    public NewByteBuffer( int capacity )
    {
        _backingArray = new byte[ capacity ];
        Memory        = _backingArray.AsMemory();
        
        SetBufferStatus( READ_WRITE, NOT_DIRECT );
    }

    // ========================================================================

    public byte GetByte( int index )
    {
        return Memory.Span[ index ];
    }

    public void PutByte( int index, byte value )
    {
        Memory.Span[ index ] = value;
    }

    public short GetShort( int index )
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadInt16BigEndian( Memory.Span.Slice( index ) )
            : BinaryPrimitives.ReadInt16LittleEndian( Memory.Span.Slice( index ) );
    }

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
    }

    public int GetInt( int index )
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadInt32BigEndian( Memory.Span.Slice( index ) )
            : BinaryPrimitives.ReadInt32LittleEndian( Memory.Span.Slice( index ) );
    }

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
    }

    public float GetFloat( int index )
    {
        return IsBigEndian
            ? BinaryPrimitives.ReadSingleBigEndian( Memory.Span.Slice( index ) )
            : BinaryPrimitives.ReadSingleLittleEndian( Memory.Span.Slice( index ) );
    }

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
    }

    // ========================================================================

    public override int Capacity => _backingArray.Length;

    // ========================================================================
}