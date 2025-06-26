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

using System.Collections;

namespace LughSharp.Lugh.Utils.Collections;

[PublicAPI]
public class ArrayListEnumerator< T > : IEnumerator< T >
{
    private int _position = -1;

    private readonly ArrayList< T > _list;

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list">The list to enumerate.</param>
    public ArrayListEnumerator( ArrayList< T > list )
    {
        _list = list;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool MoveNext()
    {
        _position++;

        return _position < _list.Count;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reset()
    {
        _position = -1;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="Exception"></exception>
    public T Current => _list[ _position ];

    /// <summary>
    /// 
    /// </summary>
    object? IEnumerator.Current => Current;

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
        _list.Clear();

        GC.SuppressFinalize( this );
    }
}