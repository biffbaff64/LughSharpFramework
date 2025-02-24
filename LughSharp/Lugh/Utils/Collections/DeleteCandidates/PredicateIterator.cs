﻿// ///////////////////////////////////////////////////////////////////////////////
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

using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Utils.Collections.DeleteCandidates;

[PublicAPI]
[Obsolete( "Obsolete" )]
public class PredicateIterator< T > : IEnumerator< T >, IDisposable
{
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

    public IEnumerator< T? > Enumerator { get; set; }
    public IPredicate< T >   Predicate  { get; set; }
    public bool              End        { get; set; }
    public bool              Peeked     { get; set; }
    public T?                NextItem   { get; set; }
    public T                 Current    { get; }

    // ========================================================================

    /// <inheritdoc />
    object? IEnumerator.Current => Current;

    /// <summary>
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual bool MoveNext()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public virtual void Reset()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Remove();

        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// </summary>
    /// <param name="enumerable"></param>
    /// <param name="predicate"></param>
    public void Set( IEnumerable< T? > enumerable, IPredicate< T > predicate )
    {
        Set( enumerable.GetEnumerator(), predicate );
    }

    /// <summary>
    /// </summary>
    /// <param name="iterator"></param>
    /// <param name="predicate"></param>
    public void Set( IEnumerator< T? > iterator, IPredicate< T > predicate )
    {
        Enumerator = iterator;
        Predicate  = predicate;
        End        = false;
        Peeked     = false;
        NextItem   = default( T? );
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
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
    /// </summary>
    /// <returns></returns>
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
    /// </summary>
    /// <exception cref="GdxRuntimeException"></exception>
    public void Remove()
    {
        if ( Peeked )
        {
            throw new GdxRuntimeException( "Cannot remove between a call to hasNext() and next()." );
        }

        Enumerator.Dispose();
    }
}