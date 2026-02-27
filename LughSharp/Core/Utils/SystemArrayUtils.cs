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

using System;
using System.Linq;

using JetBrains.Annotations;

namespace LughSharp.Core.Utils;

using Array = Array;

[PublicAPI]
public static class SystemArrayUtils
{
    /// <summary>
    /// Compares two single-dimensional arrays of floats for equality within a
    /// specified tolerance.
    /// </summary>
    /// <param name="array1">The first array to compare.</param>
    /// <param name="array2">The second array to compare.</param>
    /// <param name="epsilon">The maximum allowable difference between corresponding elements for them to be considered equal. Default is 0.0001.</param>
    /// <returns>True if the arrays have the same length and all corresponding elements differ by less than or equal to the specified tolerance; otherwise, false.</returns>
    public static bool AreEqual( float[] array1, float[] array2, float epsilon = 0.0001f )
    {
        return ( array1.Length == array2.Length )
            && array1.Zip( array2, ( a, b ) => Math.Abs( a - b ) <= epsilon ).All( x => x );
    }

    /// <summary>
    /// </summary>
    /// <param name="array1"></param>
    /// <param name="array2"></param>
    /// <returns></returns>
    public static bool DeepEquals( Array? array1, Array? array2 )
    {
        if ( ( array1 == null ) && ( array2 == null ) )
        {
            return true;
        }

        if ( ( array1 == null ) || ( array2 == null ) || ( array1.Rank != array2.Rank )
          || ( array1.Length != array2.Length ) )
        {
            return false;
        }

        var indices = new int[ array1.Rank ];

        return DeepEqualsRecursive( array1, array2, indices, 0 );
    }

    /// <summary>
    /// </summary>
    /// <param name="array1"></param>
    /// <param name="array2"></param>
    /// <param name="indices"></param>
    /// <param name="dimension"></param>
    /// <returns></returns>
    private static bool DeepEqualsRecursive( Array array1, Array array2, int[] indices, int dimension )
    {
        if ( dimension == array1.Rank )
        {
            object? element1 = array1.GetValue( indices );
            object? element2 = array2.GetValue( indices );

            if ( element1 is Array nestedArray1 && element2 is Array nestedArray2 )
            {
                return DeepEquals( nestedArray1, nestedArray2 );
            }
            else
            {
                return Equals( element1, element2 );
            }
        }

        for ( int i = array1.GetLowerBound( dimension ); i <= array1.GetUpperBound( dimension ); i++ )
        {
            indices[ dimension ] = i;

            if ( !DeepEqualsRecursive( array1, array2, indices, dimension + 1 ) )
            {
                return false;
            }
        }

        return true;
    }
}