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
public class NewShortBuffer : NewBuffer
{
    private readonly NewByteBuffer _byteBuffer;

    public NewShortBuffer( NewByteBuffer byteBuffer )
    {
        _byteBuffer = byteBuffer;
        
        SetBufferStatus( READ_WRITE, NOT_DIRECT );
    }
    
    // ========================================================================

    public short GetShort( int index )
    {
        var byteOffset = index * sizeof( short );
        var byteSpan   = _byteBuffer.Memory.Span;

        if ( ( ( byteOffset + sizeof( short ) ) > byteSpan.Length ) || ( byteOffset < 0 ) )
        {
            throw new IndexOutOfRangeException( "ShortBuffer index out of range." );
        }

        return _byteBuffer.IsBigEndian
            ? BinaryPrimitives.ReadInt16BigEndian( byteSpan.Slice( byteOffset ) )
            : BinaryPrimitives.ReadInt16LittleEndian( byteSpan.Slice( byteOffset ) );
    }

    public void PutShort( int index, short value )
    {
        var byteOffset = index * sizeof( short );
        var byteSpan   = _byteBuffer.Memory.Span;

        if ( ( ( byteOffset + sizeof( short ) ) > byteSpan.Length ) || ( byteOffset < 0 ) )
        {
            throw new IndexOutOfRangeException( "ShortBuffer index out of range." );
        }

        if ( _byteBuffer.IsBigEndian )
        {
            BinaryPrimitives.WriteInt16BigEndian( byteSpan.Slice( byteOffset ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt16LittleEndian( byteSpan.Slice( byteOffset ), value );
        }
    }

    public int GetInt( int index )
    {
        var byteOffset = index * sizeof( short );
        var byteSpan   = _byteBuffer.Memory.Span;

        if ( ( ( byteOffset + sizeof( int ) ) > byteSpan.Length ) || ( byteOffset < 0 ) )
        {
            throw new IndexOutOfRangeException( "ShortBuffer index out of range." );
        }

        return _byteBuffer.IsBigEndian
            ? BinaryPrimitives.ReadInt32BigEndian( byteSpan.Slice( byteOffset ) )
            : BinaryPrimitives.ReadInt32LittleEndian( byteSpan.Slice( byteOffset ) );
    }

    public void PutInt( int index, int value )
    {
        var byteOffset = index * sizeof( short );
        var byteSpan   = _byteBuffer.Memory.Span;

        if ( ( ( byteOffset + sizeof( int ) ) > byteSpan.Length ) || ( byteOffset < 0 ) )
        {
            throw new IndexOutOfRangeException( "ShortBuffer index out of range." );
        }

        if ( _byteBuffer.IsBigEndian )
        {
            BinaryPrimitives.WriteInt32BigEndian( byteSpan.Slice( byteOffset ), value );
        }
        else
        {
            BinaryPrimitives.WriteInt32LittleEndian( byteSpan.Slice( byteOffset ), value );
        }
    }

    public float GetFloat( int index )
    {
        var byteOffset = index * sizeof( short );
        var byteSpan   = _byteBuffer.Memory.Span;

        if ( ( ( byteOffset + sizeof( float ) ) > byteSpan.Length ) || ( byteOffset < 0 ) )
        {
            throw new IndexOutOfRangeException( "ShortBuffer index out of range." );
        }

        return _byteBuffer.IsBigEndian
            ? BinaryPrimitives.ReadSingleBigEndian( byteSpan.Slice( byteOffset ) )
            : BinaryPrimitives.ReadSingleLittleEndian( byteSpan.Slice( byteOffset ) );
    }

    public void PutFloat( int index, float value )
    {
        var byteOffset = index * sizeof( short );
        var byteSpan   = _byteBuffer.Memory.Span;

        if ( ( ( byteOffset + sizeof( float ) ) > byteSpan.Length ) || ( byteOffset < 0 ) )
        {
            throw new IndexOutOfRangeException( "ShortBuffer index out of range." );
        }

        if ( _byteBuffer.IsBigEndian )
        {
            BinaryPrimitives.WriteSingleBigEndian( byteSpan.Slice( byteOffset ), value );
        }
        else
        {
            BinaryPrimitives.WriteSingleLittleEndian( byteSpan.Slice( byteOffset ), value );
        }
    }
}
