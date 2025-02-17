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
public class NewBufferTests
{
    public NewBufferTests()
    {
        Logger.Checkpoint();

        TestNewByteBuffer();
        TestNewFloatBuffer();
        TestNewIntBuffer();
        TestNewShortBuffer();

        Logger.Debug( "Done" );
    }

    private static void TestNewByteBuffer()
    {
        Logger.Checkpoint();
        
        var newByteBuffer = new ByteBuffer( 20 );

        Logger.Debug( "BEFORE:" );
        Logger.Debug( $"Position: {newByteBuffer.Position}" );
        Logger.Debug( $"Length  : {newByteBuffer.Length}" );
        Logger.Debug( $"Capacity: {newByteBuffer.Capacity}" );
        Logger.Debug( $"Limit   : {newByteBuffer.Limit}" );

        newByteBuffer.PutByte( 5 );
        newByteBuffer.PutFloat( 16.0f );
        newByteBuffer.PutInt( 768 );
        newByteBuffer.PutShort( ( short )23 );

        Logger.Debug( "AFTER:" );
        Logger.Debug( $"Position: {newByteBuffer.Position}" );
        Logger.Debug( $"Length  : {newByteBuffer.Length}" );
        Logger.Debug( $"Capacity: {newByteBuffer.Capacity}" );
        Logger.Debug( $"Limit   : {newByteBuffer.Limit}" );

        for ( var i = 0; i < newByteBuffer.Length; i++ )
        {
            Logger.Debug( $"{newByteBuffer.GetByte( i )}, " );
        }

        Logger.Debug( "Done" );
    }

    private static void TestNewFloatBuffer()
    {
        Logger.Checkpoint();

        var newFloatBuffer = new FloatBuffer( 20 );

        Logger.Debug( "BEFORE:" );
        Logger.Debug( $"Length  : {newFloatBuffer.Length}" );
        Logger.Debug( $"Capacity: {newFloatBuffer.Capacity}" );

        newFloatBuffer.PutFloat( 16.0f );
        newFloatBuffer.PutFloat( 16.0f );
        newFloatBuffer.PutFloat( 24.0f );
        
        Logger.Debug( "AFTER:" );
        Logger.Debug( $"Length  : {newFloatBuffer.Length}" );
        Logger.Debug( $"Capacity: {newFloatBuffer.Capacity}" );

        for ( var i = 0; i < newFloatBuffer.Length; i++ )
        {
            Logger.Debug( $"{newFloatBuffer.GetFloat( i )}, " );
        }
        
        newFloatBuffer.Flip();
        newFloatBuffer.PutFloat( 32.0f );
        
        Logger.Debug( "AFTER FLIP:" );
        Logger.Debug( $"Length  : {newFloatBuffer.Length}" );
        Logger.Debug( $"Capacity: {newFloatBuffer.Capacity}" );

        for ( var i = 0; i < newFloatBuffer.Length; i++ )
        {
            Logger.Debug( $"{newFloatBuffer.GetFloat( i )}, " );
        }

        newFloatBuffer.Resize( 20 );
        
        Logger.Debug( "AFTER RESIZE:" );
        Logger.Debug( $"Length  : {newFloatBuffer.Length}" );
        Logger.Debug( $"Capacity: {newFloatBuffer.Capacity}" );

        for ( var i = 0; i < newFloatBuffer.Length; i++ )
        {
            Logger.Debug( $"{newFloatBuffer.GetFloat( i )}, " );
        }
        
        newFloatBuffer.Clear();

        Logger.Debug( "AFTER CLEAR:" );
        Logger.Debug( $"Length  : {newFloatBuffer.Length}" );
        Logger.Debug( $"Capacity: {newFloatBuffer.Capacity}" );

        for ( var i = 0; i < newFloatBuffer.Length; i++ )
        {
            Logger.Debug( $"{newFloatBuffer.GetFloat( i )}, " );
        }
        
        Logger.Debug( "Done" );
    }

    private static void TestNewIntBuffer()
    {
        Logger.Checkpoint();
    }

    private static void TestNewShortBuffer()
    {
        Logger.Checkpoint();
    }
}