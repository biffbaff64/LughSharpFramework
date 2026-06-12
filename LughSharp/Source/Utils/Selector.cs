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
/// This class is for selecting a ranked element (kth ordered statistic) from an
/// unordered list in faster time than sorting the whole array. Typical applications
/// include finding the nearest enemy unit(s), and other operations which are likely
/// to run as often as every x frames. Certain values of k will result in a partial
/// sorting of the Array.
/// The lowest ranking element starts at 1, not 0. 1 = first, 2 = second, 3 = third,
/// etc. Calling with a value of zero will result in a RuntimeException
/// This class uses very minimal extra memory, as it makes no copies of the array.
/// The underlying algorithms used are a naive single-pass for k = min and k =max, and
/// Hoare's quickselect for values in between.
/// </summary>
[PublicAPI]
public class Selector< T >
{
    public static Selector< T > Instance { get; } = new();

    // ========================================================================

    private QuickSelect< T >? _quickSelect;

    // ========================================================================

    /// <summary>
    /// Selects the element in the array that corresponds to the k-th lowest value
    /// based on the provided comparer.
    /// </summary>
    /// <param name="items">The array containing the elements to be evaluated.</param>
    /// <param name="comp">The comparer used to determine the relative order of the elements.</param>
    /// <param name="kthLowest">The rank of the element to select (1-based index).</param>
    /// <param name="size">The number of elements in the array to consider. Must be greater than 0.</param>
    /// <returns>The element in the array corresponding to the k-th lowest value.</returns>
    /// <exception cref="RuntimeException">
    /// Thrown if the size is less than 1 or if the k-th rank exceeds the number of elements considered.
    /// </exception>
    public T Select( T[] items, IComparer< T > comp, int kthLowest, int size )
    {
        int idx = SelectIndex( items, comp, kthLowest, size );

        return items[ idx ];
    }

    /// <summary>
    /// Selects the index of the element in the array that corresponds to the k-th lowest value
    /// based on the provided comparer.
    /// </summary>
    /// <param name="items">The array containing the elements to be evaluated.</param>
    /// <param name="comp">The comparer used to determine the relative order of the elements.</param>
    /// <param name="kthLowest">The rank of the element to select (1-based index).</param>
    /// <param name="size">The number of elements in the array to consider. Must be greater than 0.</param>
    /// <returns>The index of the element in the array corresponding to the k-th lowest value.</returns>
    /// <exception cref="RuntimeException">
    /// Thrown if the size is less than 1 or if the k-th rank exceeds the number of elements considered.
    /// </exception>
    public int SelectIndex( T[] items, IComparer< T > comp, int kthLowest, int size )
    {
        if ( size < 1 )
        {
            throw new RuntimeException( "cannot select from empty array (size < 1)" );
        }

        if ( kthLowest > size )
        {
            throw new RuntimeException
                ( $"Kth rank is larger than size. k: {kthLowest}, size: {size}" );
        }

        int idx;

        // naive partial selection sort almost certain to outperform
        // quickselect where n is min or max
        if ( kthLowest == 1 )
        {
            // find min
            idx = FastMin( items, comp, size );
        }
        else if ( kthLowest == size )
        {
            // find max
            idx = FastMax( items, comp, size );
        }
        else
        {
            // quickselect a better choice for cases of k between min and max
            _quickSelect ??= new QuickSelect< T >();

            idx = _quickSelect.Select( items, comp, kthLowest, size );
        }

        return idx;
    }

    /// <summary>
    /// Finds the index of the minimum element in the given array based on the provided comparer.
    /// </summary>
    /// <param name="items">The array of items to search for the minimum element.</param>
    /// <param name="comp">The comparer used to determine the relative order of the elements in the array.</param>
    /// <param name="size">The number of elements in the array to consider. Must be greater than 0.</param>
    /// <returns>The index of the minimum element in the array as determined by the comparer.</returns>
    /// <remarks> Faster than <see cref="QuickSelect{T}"/> for n = min. </remarks>
    private static int FastMin( T[] items, IComparer< T > comp, int size )
    {
        if ( size < 1 )
        {
            throw new RuntimeException( "cannot select from empty array (size < 1)" );
        }

        var lowestIdx = 0;

        for ( var i = 1; i < size; i++ )
        {
            int comparison = comp.Compare( items[ i ], items[ lowestIdx ] );

            if ( comparison < 0 )
            {
                lowestIdx = i;
            }
        }

        return lowestIdx;
    }

    /// <summary>
    /// Finds the index of the maximum element in the given array based on the provided comparer.
    /// </summary>
    /// <param name="items">The array of items to search for the maximum element.</param>
    /// <param name="comp">The comparer used to determine the relative order of the elements in the array.</param>
    /// <param name="size">The number of elements in the array to consider. Must be greater than 0.</param>
    /// <returns>The index of the maximum element in the array as determined by the comparer.</returns>
    private static int FastMax( T[] items, IComparer< T > comp, int size )
    {
        if ( size < 1 )
        {
            throw new RuntimeException( "cannot select from empty array (size < 1)" );
        }
        
        var highestIdx = 0;

        for ( var i = 1; i < size; i++ )
        {
            int comparison = comp.Compare( items[ i ], items[ highestIdx ] );

            if ( comparison > 0 )
            {
                highestIdx = i;
            }
        }

        return highestIdx;
    }
}

// ============================================================================
// ============================================================================