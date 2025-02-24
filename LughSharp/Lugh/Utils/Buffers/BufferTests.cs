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

namespace LughSharp.Lugh.Utils.Buffers;

[PublicAPI]
public class BufferTests
{
    public BufferTests()
    {
        Logger.Checkpoint();

        TestByteBuffer();
        TestFloatBuffer();
        TestIntBuffer();
        TestShortBuffer();

        Logger.Debug( "Done" );
    }

    private static void TestByteBuffer()
    {
        Logger.Checkpoint();

        var byteBuffer = new ByteBuffer( 20 );

        Logger.Debug( "BEFORE:" );
        Logger.Debug( $"Position: {byteBuffer.Position}" );
        Logger.Debug( $"Length  : {byteBuffer.Length}" );
        Logger.Debug( $"Capacity: {byteBuffer.Capacity}" );
        Logger.Debug( $"Limit   : {byteBuffer.Limit}" );

        byteBuffer.PutByte( 5 );
        byteBuffer.PutFloat( 16.0f );
        byteBuffer.PutInt( 768 );
        byteBuffer.PutShort( ( short )23 );

        Logger.Debug( "AFTER:" );
        Logger.Debug( $"Position: {byteBuffer.Position}" );
        Logger.Debug( $"Length  : {byteBuffer.Length}" );
        Logger.Debug( $"Capacity: {byteBuffer.Capacity}" );
        Logger.Debug( $"Limit   : {byteBuffer.Limit}" );

        for ( var i = 0; i < byteBuffer.Length; i++ )
        {
            Logger.Debug( $"{byteBuffer.GetByte( i )}, " );
        }

        byteBuffer.Flip();
        byteBuffer.PutFloat( 32.0f );

        Logger.Debug( "AFTER FLIP:" );
        Logger.Debug( $"Length  : {byteBuffer.Length}" );
        Logger.Debug( $"Capacity: {byteBuffer.Capacity}" );

        for ( var i = 0; i < byteBuffer.Length; i++ )
        {
            Logger.Debug( $"{byteBuffer.GetFloat( i )}, " );
        }

        byteBuffer.Resize( 20 );

        Logger.Debug( "AFTER RESIZE:" );
        Logger.Debug( $"Length  : {byteBuffer.Length}" );
        Logger.Debug( $"Capacity: {byteBuffer.Capacity}" );

        for ( var i = 0; i < byteBuffer.Length; i++ )
        {
            Logger.Debug( $"{byteBuffer.GetFloat( i )}, " );
        }

        byteBuffer.Clear();

        Logger.Debug( "AFTER CLEAR:" );
        Logger.Debug( $"Length  : {byteBuffer.Length}" );
        Logger.Debug( $"Capacity: {byteBuffer.Capacity}" );

        for ( var i = 0; i < byteBuffer.Length; i++ )
        {
            Logger.Debug( $"{byteBuffer.GetFloat( i )}, " );
        }

        Logger.Debug( "Done" );
    }

    private static void TestFloatBuffer()
    {
        Logger.Checkpoint();

        var floatBuffer = new FloatBuffer( 20 );

        Logger.Debug( "BEFORE:" );
        Logger.Debug( $"Length  : {floatBuffer.Length}" );
        Logger.Debug( $"Capacity: {floatBuffer.Capacity}" );

        floatBuffer.PutFloat( 16.0f );
        floatBuffer.PutFloat( 16.0f );
        floatBuffer.PutFloat( 24.0f );

        Logger.Debug( "AFTER:" );
        Logger.Debug( $"Length  : {floatBuffer.Length}" );
        Logger.Debug( $"Capacity: {floatBuffer.Capacity}" );

        for ( var i = 0; i < floatBuffer.Length; i++ )
        {
            Logger.Debug( $"{floatBuffer.GetFloat( i )}, " );
        }

        floatBuffer.Flip();
        floatBuffer.PutFloat( 32.0f );

        Logger.Debug( "AFTER FLIP:" );
        Logger.Debug( $"Length  : {floatBuffer.Length}" );
        Logger.Debug( $"Capacity: {floatBuffer.Capacity}" );

        for ( var i = 0; i < floatBuffer.Length; i++ )
        {
            Logger.Debug( $"{floatBuffer.GetFloat( i )}, " );
        }

        floatBuffer.Resize( 20 );

        Logger.Debug( "AFTER RESIZE:" );
        Logger.Debug( $"Length  : {floatBuffer.Length}" );
        Logger.Debug( $"Capacity: {floatBuffer.Capacity}" );

        for ( var i = 0; i < floatBuffer.Length; i++ )
        {
            Logger.Debug( $"{floatBuffer.GetFloat( i )}, " );
        }

        floatBuffer.Clear();

        Logger.Debug( "AFTER CLEAR:" );
        Logger.Debug( $"Length  : {floatBuffer.Length}" );
        Logger.Debug( $"Capacity: {floatBuffer.Capacity}" );

        for ( var i = 0; i < floatBuffer.Length; i++ )
        {
            Logger.Debug( $"{floatBuffer.GetFloat( i )}, " );
        }

        Logger.Debug( "Done" );
    }

    private static void TestIntBuffer()
    {
        Logger.Checkpoint();
    }

    private static void TestShortBuffer()
    {
        Logger.Checkpoint();
    }
}