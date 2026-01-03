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

using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Utils.Collections;

[PublicAPI]
public static class ListExtensions
{
    /// <param name="ts"> This list </param>
    /// <typeparam name="T"> This list type </typeparam>
    extension< T >( List< T > ts )
    {
        public T[] Resize( int newSize )
        {
            var newItems = new T[ newSize ];

            Array.Copy( ts.ToArray(), newItems, newSize );

            return newItems;
        }

        /// <summary>
        /// Returns the element found at a random position within the list.
        /// </summary>
        /// <returns></returns>
        public T Random()
        {
            return ts[ MathUtils.Random( ts.Count - 1 ) ];
        }
    }

    /// <summary>
    /// Returns a new List of the required type.
    /// </summary>
    public static List< T > New< T >( T t )
    {
        return new List< T >();
    }

    /// <param name="target"></param>
    /// <typeparam name="T"></typeparam>
    extension< T >( List< T > target )
    {
        /// <summary>
        /// Adds <paramref name="count" /> elements in the source Array, starting at position
        /// <paramref name="start" /> to the target List.
        /// </summary>
        public void AddAll( T[] source, int start, int count )
        {
            for ( var i = start; i < count; i++ )
            {
                target.Add( source[ i ] );
            }
        }

        /// <summary>
        /// Adds <paramref name="count" /> elements in the source List, starting at position
        /// <paramref name="start" /> to the target List.
        /// </summary>
        public void AddAll( List< T > source, int start, int count )
        {
            for ( var i = start; i < count; i++ )
            {
                target.Add( source[ i ] );
            }
        }

        /// <summary>
        /// Adds all elements in the source List to the target List.
        /// </summary>
        public void AddAll( List< T > source )
        {
            foreach ( var tex in source )
            {
                target.Add( tex );
            }
        }

        /// <summary>
        /// Adds all elements in the array 'items' to the target List.
        /// </summary>
        public void AddAll( params T[] items )
        {
            foreach ( var item in items )
            {
                target.Add( item );
            }
        }

        /// <summary>
        /// Shuffles the element order of the specified list.
        /// </summary>
        public void Shuffle()
        {
            var count = target.Count;
            var last  = count - 1;

            var random = new Random();

            for ( var i = 0; i < last; ++i )
            {
                var r = random.Next( i, count );

                ( target[ i ], target[ r ] ) = ( target[ r ], target[ i ] );
            }
        }

        /// <summary>
        /// Reduces the size of the array to the specified size. If the array is
        /// already smaller than the specified size, no action is taken.
        /// </summary>
        public void Truncate( int newSize )
        {
            if ( target.Count > newSize )
            {
                target.RemoveRange( newSize, target.Count - newSize );
            }
        }

        /// <summary>
        /// Removes and returns the last item in the list.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="GdxRuntimeException"></exception>
        public T Pop()
        {
            if ( target.Count == 0 )
            {
                throw new GdxRuntimeException( "List is empty." );
            }

            var item = target[ ^1 ];

            target.RemoveAt( target.Count - 1 );

            return item;
        }

        /// <summary>
        /// Returns the last item in a list.
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            return target[ ^1 ];
        }

        /// <summary>
        /// Removes and returns the item at the specified index.
        /// </summary>
        /// <param name="index"> The index at which to get the element. </param>
        /// <returns></returns>
        public T RemoveIndex( int index )
        {
            if ( index >= target.Count )
            {
                throw new IndexOutOfRangeException( $"index can't be >= size: {index} >= {target.Count}" );
            }

            var value = target[ index ];

            target.RemoveAt( index );

            return value;
        }

        /// <summary>
        /// Logs each string entry in the list using debug-level logging.
        /// </summary>
        public void DebugPrint()
        {
            Logger.Debug( $"{target.Count} items in list {nameof( target )}" );

            Logger.IsMinimal = true;
            foreach( var entry in target )
            {
                Logger.Debug( $"{entry}" );
            }
            Logger.IsMinimal = false;
        }
    }
}

// ============================================================================
// ============================================================================
