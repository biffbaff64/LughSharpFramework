// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

namespace LughSharp.Source.Collections;

[PublicAPI]
public static class ListExtensions
{
    /// <param name="target"> This list </param>
    /// <typeparam name="T"> This list type </typeparam>
    extension< T >( List< T > target )
    {
        /// <summary>
        /// Returns the element found at a random position within the list.
        /// </summary>
        /// <returns></returns>
        public T Random()
        {
            return target[ MathUtils.Random( target.Count - 1 ) ];
        }

        /// <summary>
        /// Shuffles the element order of the specified list.
        /// </summary>
        public void Shuffle()
        {
            int count = target.Count;
            int last  = count - 1;

            var random = new Random();

            for ( var i = 0; i < last; ++i )
            {
                int r = random.Next( i, count );

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
        /// <exception cref="RuntimeException"></exception>
        public T Pop()
        {
            if ( target.Count == 0 )
            {
                throw new RuntimeException( "List is empty." );
            }

            T item = target[ target.Count - 1 ];

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

            T value = target[ index ];

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

            foreach ( T entry in target )
            {
                Logger.Debug( $"{entry}" );
            }

            Logger.IsMinimal = false;
        }
    }

    /// <param name="target"> This list </param>
    /// <typeparam name="T"> This list type </typeparam>
    extension< T >( List< T >? target )
    {
        public T? SafePop()
        {
            // 1. Check if we have anything to pop
            if ( target == null || target.Count == 0 )
            {
                return default;
            }

            // 2. Grab the last index
            int lastIndex = target.Count - 1;

            // 3. Store the item to return it
            T item = target[ lastIndex ];

            // 4. Correctly reduce the size by 1
            target.RemoveAt( lastIndex );

            return item;
        }
    }
}

// ============================================================================
// ============================================================================