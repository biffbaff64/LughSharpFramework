// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024, 2025, 2026 Circa64 Software Projects / Richard Ikin.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ///////////////////////////////////////////////////////////////////////////////

using System.Buffers;
using System.Runtime.CompilerServices;

using JetBrains.Annotations;

using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Collections;

/// <summary>
/// A stack that can be reset to its initial state.
/// </summary>
/// <typeparam name="T"> The type of elements stored in the stack. </typeparam>
[PublicAPI]
public class ResettableStack< T > : IDisposable
{
    public int Count { get; private set; }

    // ========================================================================

    private const int DefaultCapacity = 16;

    private T[]? _buffer;

    // ========================================================================

    /// <summary>
    /// Creates a new ResettableStack object with the specified initial capacity.
    /// </summary>
    /// <param name="initialCapacity"> The capacity to set. Defaults to 16. </param>
    public ResettableStack( int initialCapacity = DefaultCapacity )
    {
        // Rent the initial buffer
        _buffer = ArrayPool< T >.Shared.Rent( initialCapacity );
        Count   = 0;
    }

    /// <summary>
    /// Pushes an item onto the stack.
    /// </summary>
    /// <param name="item"> The item to push. </param>
    public void Push( T item )
    {
        Guard.Against.Null( _buffer );

        if ( Count == _buffer.Length )
        {
            // Grow the array
            int newSize   = _buffer.Length * 2;
            T[] newBuffer = ArrayPool< T >.Shared.Rent( newSize );

            Array.Copy( _buffer, 0, newBuffer, 0, Count );

            // Return old, rent new
            ArrayPool< T >.Shared.Return( _buffer );
            _buffer = newBuffer.ToArray();
        }

        _buffer[ Count++ ] = item;
    }

    /// <summary>
    /// Pops an item off the stack.
    /// </summary>
    /// <returns> The popped item. </returns>
    public T? Pop()
    {
        Guard.Against.Null( _buffer );

        if ( Count == 0 )
        {
            return default;
        }

        // Clear the slot to avoid memory leaks
        T item = _buffer[ --Count ];
        _buffer[ Count ] = default!;

        return item;
    }

    /// <summary>
    /// Clears the stack. Reset count without returning the buffer, allowing re-use
    /// </summary>
    public void Clear()
    {
        if ( RuntimeHelpers.IsReferenceOrContainsReferences< T >() )
        {
            if ( _buffer != null )
            {
                Array.Clear( _buffer, 0, Count );
            }
        }

        Count = 0;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing,
    /// or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        if ( _buffer != null )
        {
            ArrayPool< T >.Shared.Return( _buffer );
            _buffer = null;
        }

        GC.SuppressFinalize( this );
    }
}

// ============================================================================
// ============================================================================