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
public class NewBufferTests
{
    private NewByteBuffer?  _newByteBuffer;
    private NewFloatBuffer? _newFloatBuffer;
    private NewIntBuffer?   _newIntBuffer;
    private NewShortBuffer? _newShortBuffer;

    public NewBufferTests()
    {
        Logger.Checkpoint();

        TestNewByteBuffer();
        TestNewFloatBuffer();
        TestNewIntBuffer();
        TestNewShortBuffer();

        Logger.Debug( "Done" );
    }

    public void TestNewByteBuffer()
    {
        _newByteBuffer = new NewByteBuffer( 20 );

        Logger.Debug( "BEFORE:" );
        Logger.Debug( $"Position: {_newByteBuffer.Position}" );
        Logger.Debug( $"Length  : {_newByteBuffer.Length}" );
        Logger.Debug( $"Capacity: {_newByteBuffer.Capacity}" );
        Logger.Debug( $"Limit   : {_newByteBuffer.Limit}" );

        _newByteBuffer.PutByte( 5 );
        _newByteBuffer.PutFloat( 16.0f );
        _newByteBuffer.PutInt( 768 );
        _newByteBuffer.PutShort( ( short )23 );

        Logger.Debug( "AFTER:" );
        Logger.Debug( $"Position: {_newByteBuffer.Position}" );
        Logger.Debug( $"Length  : {_newByteBuffer.Length}" );
        Logger.Debug( $"Capacity: {_newByteBuffer.Capacity}" );
        Logger.Debug( $"Limit   : {_newByteBuffer.Limit}" );

        for ( var i = 0; i < _newByteBuffer.Capacity; i++ )
        {
            Logger.Debug( $"{_newByteBuffer.GetByte( i )}, " );
        }

        Logger.Debug( "Done" );
    }

    public void TestNewFloatBuffer()
    {
    }

    public void TestNewIntBuffer()
    {
    }

    public void TestNewShortBuffer()
    {
    }
}