// // /////////////////////////////////////////////////////////////////////////////
// //  MIT License
// //
// //  Copyright (c) 2024 Richard Ikin
// //
// //  Permission is hereby granted, free of charge, to any person obtaining a copy
// //  of this software and associated documentation files (the "Software"), to deal
// //  in the Software without restriction, including without limitation the rights
// //  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// //  copies of the Software, and to permit persons to whom the Software is
// //  furnished to do so, subject to the following conditions:
// //
// //  The above copyright notice and this permission notice shall be included in all
// //  copies or substantial portions of the Software.
// //
// //  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// //  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// //  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// //  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// //  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// //  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// //  SOFTWARE.
// // /////////////////////////////////////////////////////////////////////////////

using System.Collections.Concurrent;

namespace LughSharp.Source.Utils.Pooling;

[PublicAPI]
public class ConcurrentPool< T >( Func< T > supplier ) : IPool< T > where T : class
{
    private readonly ConcurrentBag< T > _freeObjects = new();

    private readonly Func< T > _supplier = supplier
                                        ?? throw new ArgumentNullException( nameof( supplier ) );

    // ========================================================================

    /// <summary>
    /// Try to pop an existing object from the bag; if empty, manufacture a new one.
    /// </summary>
    /// <returns></returns>
    public T Obtain()
    {
        return _freeObjects.TryTake( out T? item ) ? item : _supplier();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void Free( T obj )
    {
        if ( obj == null )
        {
            throw new ArgumentNullException( nameof( obj ) );
        }

        // Reset object state if it implements a reset interface
        if ( obj is IPoolable poolable )
        {
            poolable.Reset();
        }

        _freeObjects.Add( obj );
    }

    /// <summary>
    /// Clears the pool of all objects.
    /// </summary>
    public void Clear()
    {
        _freeObjects.Clear();
    }
}

// ============================================================================
// ============================================================================
