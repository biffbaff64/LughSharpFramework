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

namespace LughSharp.LibCore.Utils.Buffers.DirectBuffers;

[PublicAPI]
public class DirectByteBuffer : MappedByteBuffer
{
    public DirectByteBuffer( int capacity )
        : base( -1, 0, capacity, capacity, null )
    {
    }

    /// <inheritdoc />
    public override bool IsDirect()
    {
        return false;
    }

    /// <inheritdoc />
    public override ByteBuffer Compact()
    {
        return null;
    }

    /// <inheritdoc />
    protected override int Ix( int i )
    {
        return 0;
    }

    /// <inheritdoc />
    public override ByteBuffer Slice()
    {
        return null;
    }

    /// <inheritdoc />
    public override ByteBuffer Duplicate()
    {
        return null;
    }

    /// <inheritdoc />
    public override ByteBuffer AsReadOnlyBuffer()
    {
        return null;
    }

    /// <inheritdoc />
    public override byte Get()
    {
        return 0;
    }

    /// <inheritdoc />
    public override ByteBuffer Put( byte b )
    {
        return null;
    }

    /// <inheritdoc />
    public override byte Get( int index )
    {
        return 0;
    }

    /// <inheritdoc />
    public override ByteBuffer Put( int index, byte b )
    {
        return null;
    }

    /// <inheritdoc />
    public override char GetChar()
    {
        return '\0';
    }

    /// <inheritdoc />
    public override ByteBuffer PutChar( char value )
    {
        return null;
    }

    /// <inheritdoc />
    public override char GetChar( int index )
    {
        return '\0';
    }

    /// <inheritdoc />
    public override ByteBuffer PutChar( int index, char value )
    {
        return null;
    }

    /// <inheritdoc />
    public override CharBuffer AsCharBuffer()
    {
        return null;
    }

    /// <inheritdoc />
    public override short GetShort()
    {
        return 0;
    }

    /// <inheritdoc />
    public override ByteBuffer PutShort( short value )
    {
        return null;
    }

    /// <inheritdoc />
    public override short GetShort( int index )
    {
        return 0;
    }

    /// <inheritdoc />
    public override ByteBuffer PutShort( int index, short value )
    {
        return null;
    }

    /// <inheritdoc />
    public override ShortBuffer AsShortBuffer()
    {
        return null;
    }

    /// <inheritdoc />
    public override int GetInt()
    {
        return 0;
    }

    /// <inheritdoc />
    public override ByteBuffer PutInt( int value )
    {
        return null;
    }

    /// <inheritdoc />
    public override int GetInt( int index )
    {
        return 0;
    }

    /// <inheritdoc />
    public override ByteBuffer PutInt( int index, int value )
    {
        return null;
    }

    /// <inheritdoc />
    public override IntBuffer AsIntBuffer()
    {
        return null;
    }

    /// <inheritdoc />
    public override long GetLong()
    {
        return 0;
    }

    /// <inheritdoc />
    public override ByteBuffer PutLong( long value )
    {
        return null;
    }

    /// <inheritdoc />
    public override long GetLong( int index )
    {
        return 0;
    }

    /// <inheritdoc />
    public override ByteBuffer PutLong( int index, long value )
    {
        return null;
    }

    /// <inheritdoc />
    public override LongBuffer AsLongBuffer()
    {
        return null;
    }

    /// <inheritdoc />
    public override float GetFloat()
    {
        return 0;
    }

    /// <inheritdoc />
    public override ByteBuffer PutFloat( float value )
    {
        return null;
    }

    /// <inheritdoc />
    public override float GetFloat( int index )
    {
        return 0;
    }

    /// <inheritdoc />
    public override ByteBuffer PutFloat( int index, float value )
    {
        return null;
    }

    /// <inheritdoc />
    public override FloatBuffer AsFloatBuffer()
    {
        return null;
    }

    /// <inheritdoc />
    public override double GetDouble()
    {
        return 0;
    }

    /// <inheritdoc />
    public override double GetDouble( int index )
    {
        return 0;
    }

    /// <inheritdoc />
    public override ByteBuffer PutDouble( double value )
    {
        return null;
    }

    /// <inheritdoc />
    public override ByteBuffer PutDouble( int index, double value )
    {
        return null;
    }

    /// <inheritdoc />
    public override DoubleBuffer AsDoubleBuffer()
    {
        return null;
    }
}