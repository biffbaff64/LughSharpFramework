// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024 Richard Ikin.
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

namespace LughSharp.Core.Utils.Collections;

[PublicAPI]
public class ResettableStack< T > : IDisposable
{
    private       T[]? _buffer;
    private       int  _count;
    private const int  DefaultCapacity = 16;

    public int Count => _count;

    public ResettableStack( int initialCapacity = DefaultCapacity )
    {
        // Rent the initial buffer
        _buffer = ArrayPool< T >.Shared.Rent( initialCapacity );
        _count  = 0;
    }

    public void Push( T item )
    {
        Guard.Against.Null( _buffer );
        
        if ( _count == _buffer.Length )
        {
            // Grow the array
            int newSize   = _buffer.Length * 2;
            T[] newBuffer = ArrayPool< T >.Shared.Rent( newSize );

            Array.Copy( _buffer, 0, newBuffer, 0, _count );

            // Return old, rent new
            ArrayPool< T >.Shared.Return( _buffer );
            _buffer = newBuffer;
        }

        _buffer[ _count++ ] = item;
    }

    public T? Pop()
    {
        Guard.Against.Null( _buffer );

        if ( _count == 0 )
        {
            return default;
        }

        T item = _buffer[ --_count ];
        _buffer[ _count ] = default!; // Clear the slot to avoid memory leaks

        return item;
    }

    public void Clear()
    {
        // Reset count without returning the buffer, allowing reuse
        if ( RuntimeHelpers.IsReferenceOrContainsReferences< T >() )
        {
            if ( _buffer != null )
            {
                Array.Clear( _buffer, 0, _count );
            }
        }

        _count = 0;
    }

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