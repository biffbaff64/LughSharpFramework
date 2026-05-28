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

namespace LughSharp.Source.Utils;

/// <summary>
/// Implementation of Tony Hoare's quickselect algorithm. Running time is generally O(n),
/// but the worst case is O(n^2) Pivot choice is median of three method, providing better
/// performance than a random pivot for partially sorted data.
/// <para>
/// See http://en.wikipedia.org/wiki/Quickselect.
/// </para>
/// </summary>
[PublicAPI]
public class QuickSelect< T >
{
    private T[]            _array = null!;
    private IComparer< T > _comp  = null!;

    // ========================================================================

    /// <summary>
    /// Selects the index of the element that is the kth smallest in the given array based on
    /// the specified comparer.
    /// </summary>
    /// <param name="items">The array of elements to search within.</param>
    /// <param name="comp">The comparer used to define the ordering of the elements.</param>
    /// <param name="n">The positional rank (1-based) of the desired element in the sorted array.</param>
    /// <param name="size">The number of elements in the array to consider.</param>
    /// <returns>The index of the kth smallest element in the array.</returns>
    public int Select( T[] items, IComparer< T > comp, int n, int size )
    {
        _array = items;
        _comp  = comp;

        return RecursiveSelect( 0, size - 1, n );
    }

    /// <summary>
    /// Partitions the elements in the array within the specified range, rearranging them
    /// so that all elements less than the pivot are moved to the left of the pivot and
    /// all elements greater than or equal to the pivot are moved to the right.
    /// </summary>
    /// <param name="left">The starting index of the range to partition.</param>
    /// <param name="right">The ending index of the range to partition.</param>
    /// <param name="pivot">The index of the pivot element.</param>
    /// <returns>The new index of the pivot element after partitioning.</returns>
    private int Partition( int left, int right, int pivot )
    {
        T pivotValue = _array[ pivot ];

        Swap( right, pivot );

        int storage = left;

        for ( int i = left; i < right; i++ )
        {
            if ( _comp.Compare( _array[ i ], pivotValue ) < 0 )
            {
                Swap( storage, i );
                storage++;
            }
        }

        Swap( right, storage );

        return storage;
    }

    /// <summary>
    /// Recursively selects the index of the element that is the kth smallest in the subarray
    /// defined by the specified range [left, right].
    /// </summary>
    /// <param name="left">The starting index of the subarray.</param>
    /// <param name="right">The ending index of the subarray.</param>
    /// <param name="k">The positional rank (1-based) of the desired element in the subarray.</param>
    /// <returns>The index of the kth smallest element within the specified range.</returns>
    private int RecursiveSelect( int left, int right, int k )
    {
        if ( left == right )
        {
            return left;
        }

        int pivotIndex    = MedianOfThreePivot( left, right );
        int pivotNewIndex = Partition( left, right, pivotIndex );
        int pivotDist     = pivotNewIndex - left + 1;

        int result;

        if ( pivotDist == k )
        {
            result = pivotNewIndex;
        }
        else if ( k < pivotDist )
        {
            result = RecursiveSelect( left, pivotNewIndex - 1, k );
        }
        else
        {
            result = RecursiveSelect( pivotNewIndex + 1, right, k - pivotDist );
        }

        return result;
    }

    /// <summary>
    /// Median of Three has the potential to outperform a random pivot, especially
    /// for partially sorted arrays
    /// </summary>
    private int MedianOfThreePivot( int leftIdx, int rightIdx )
    {
        T   left   = _array[ leftIdx ];
        int midIdx = ( leftIdx + rightIdx ) / 2;
        T   mid    = _array[ midIdx ];
        T   right  = _array[ rightIdx ];

        // spaghetti median of three algorithm
        // does at most 3 comparisons
        if ( _comp.Compare( left, mid ) > 0 )
        {
            if ( _comp.Compare( mid, right ) > 0 )
            {
                return midIdx;
            }

            return _comp.Compare( left, right ) > 0 ? rightIdx : leftIdx;
        }

        if ( _comp.Compare( left, right ) > 0 )
        {
            return leftIdx;
        }

        return _comp.Compare( mid, right ) > 0 ? rightIdx : midIdx;
    }

    /// <summary>
    /// Swap two elements in the array.
    /// </summary>
    /// <param name="left"> First element. </param>
    /// <param name="right"> Second element. </param>
    private void Swap( int left, int right )
    {
        ( _array[ left ], _array[ right ] ) = ( _array[ right ], _array[ left ] );
    }
}

// ============================================================================
// ============================================================================