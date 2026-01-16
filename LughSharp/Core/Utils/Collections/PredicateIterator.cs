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

using System.Collections;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Utils.Collections;

/// <summary>
/// An iterator that enumerates elements of type <typeparamref name="T"/> from a collection,
/// returning only those that satisfy a specified predicate. Supports peeking at the next
/// valid element, resetting, and removal. Implements <see cref="IEnumerator{T}"/>.
/// </summary>
/// <typeparam name="T">The type of elements to iterate over.</typeparam>
[PublicAPI]
public class PredicateIterator<T> : IEnumerator<T>, IDisposable
{
    /// <summary>
    /// The underlying enumerator for the collection.
    /// </summary>
    public IEnumerator<T?> Enumerator { get; set; }

    /// <summary>
    /// The predicate used to filter elements.
    /// </summary>
    public IPredicate<T> Predicate { get; set; }

    /// <summary>
    /// Indicates whether the end of the collection has been reached.
    /// </summary>
    public bool End { get; set; }

    /// <summary>
    /// Indicates whether the next item has been peeked.
    /// </summary>
    public bool Peeked { get; set; }

    /// <summary>
    /// Stores the next item that matches the predicate.
    /// </summary>
    public T? NextItem { get; set; }

    /// <summary>
    /// The current item in the iteration.
    /// </summary>
    public T Current { get; }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="enumerable"></param>
    /// <param name="predicate"></param>
    public PredicateIterator( IEnumerable< T? > enumerable, IPredicate< T > predicate )
        : this( enumerable.GetEnumerator(), predicate )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="enumerator"></param>
    /// <param name="predicate"></param>
    public PredicateIterator( IEnumerator< T? > enumerator, IPredicate< T > predicate )
    {
        Enumerator = enumerator;
        Predicate  = predicate;
        End        = false;
        Peeked     = false;
        NextItem   = default( T? );
        Current    = default( T? )!;
    }

    // ========================================================================

    /// <inheritdoc />
    object? IEnumerator.Current => Current;

    /// <summary>
    /// Advances the iterator to the next element that satisfies the predicate.
    /// </summary>
    /// <returns>True if a valid element is found; otherwise, false.</returns>
    public virtual bool MoveNext()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Resets the iterator to its initial state.
    /// </summary>
    public virtual void Reset()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sets the iterator to use a new enumerable and predicate.
    /// </summary>
    /// <param name="enumerable"> The enumerable to iterate over. </param>
    /// <param name="predicate"> The predicate to filter elements. </param>
    public void Set( IEnumerable< T? > enumerable, IPredicate< T > predicate )
    {
        Set( enumerable.GetEnumerator(), predicate );
    }

    /// <summary>
    /// Sets the iterator to use a new enumerator and predicate.
    /// </summary>
    /// <param name="iterator"> The enumerator to iterate over. </param>
    /// <param name="predicate"> The predicate to filter elements. </param>
    public void Set( IEnumerator< T? > iterator, IPredicate< T > predicate )
    {
        Enumerator = iterator;
        Predicate  = predicate;
        End        = false;
        Peeked     = false;
        NextItem   = default( T? );
    }

    /// <summary>
    /// Determines whether there is a next element that satisfies the predicate.
    /// </summary>
    /// <returns> True if there is a next valid element; otherwise, false. </returns>
    public bool HasNext()
    {
        if ( End )
        {
            return false;
        }

        if ( NextItem != null )
        {
            return true;
        }

        Peeked = true;

        while ( Enumerator.MoveNext() )
        {
            var n = Enumerator.Current;

            if ( Predicate.Evaluate( n ) )
            {
                NextItem = n;

                return true;
            }
        }

        End = true;

        return false;
    }

    /// <summary>
    /// Returns the next element that satisfies the predicate.
    /// </summary>
    /// <returns> The next valid element, or default if none exists. </returns>
    public T? Next()
    {
        if ( ( NextItem == null ) && !HasNext() )
        {
            return default( T );
        }

        var result = NextItem;
        NextItem = default( T );
        Peeked   = false;

        return result;
    }

    /// <summary>
    /// Removes the current element from the underlying collection.
    /// </summary>
    /// <exception cref="RuntimeException"> Thrown if called between HasNext() and Next(). </exception>
    public void Remove()
    {
        if ( Peeked )
        {
            throw new RuntimeException( "Cannot remove between a call to HasNext() and Next()." );
        }

        Enumerator.Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Remove();

        GC.SuppressFinalize( this );
    }
}