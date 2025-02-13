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
public class NewFloatBuffer
{
    private NewByteBuffer _byteBuffer;

    public NewFloatBuffer( int capacityInFloats )
    {
        var byteCapacity = capacityInFloats * sizeof( float );
        
        _byteBuffer = new NewByteBuffer( byteCapacity );

        _byteBuffer.Length   = 0;
        _byteBuffer.Position = 0;
        
        _byteBuffer.SetBufferStatus( NewByteBuffer.READ_WRITE, NewByteBuffer.NOT_DIRECT );
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public float GetFloat( int index )
    {
        var floatOffset = index  * sizeof( float );
        
        return _byteBuffer.GetFloat( floatOffset );
    }

    public void PutFloat( int index, float value )
    {
        var floatOffset = index * sizeof( float );
        
        _byteBuffer.PutFloat( floatOffset, value );
    }
}