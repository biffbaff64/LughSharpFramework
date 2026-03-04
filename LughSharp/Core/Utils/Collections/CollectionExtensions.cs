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

using JetBrains.Annotations;

namespace LughSharp.Core.Utils.Collections;

[PublicAPI]
public static class CollectionExtensions
{
    public static bool DeepEquals( object? obj1, object? obj2 )
    {
        // 1. Easy wins: same reference or both null
        if ( ReferenceEquals( obj1, obj2 ) )
        {
            return true;
        }

        if ( obj1 == null || obj2 == null )
        {
            return false;
        }

        // 2. If they aren't collections (e.g., int, string, Color), use standard Equals
        if ( obj1 is not IEnumerable list1 || obj2 is not IEnumerable list2 )
        {
            return obj1.Equals( obj2 );
        }

        // 3. Compare as collections
        IEnumerator enumerator1 = list1.GetEnumerator();
        IEnumerator enumerator2 = list2.GetEnumerator();

        while ( true )
        {
            bool hasNext1 = enumerator1.MoveNext();
            bool hasNext2 = enumerator2.MoveNext();

            // If lengths differ, they aren't equal
            if ( hasNext1 != hasNext2 )
            {
                return false;
            }

            // If we reached the end of both, they are equal
            if ( !hasNext1 )
            {
                return true;
            }

            // Recursively check the current elements
            if ( !DeepEquals( enumerator1.Current, enumerator2.Current ) )
            {
                return false;
            }
        }
    }
}

// ============================================================================
// ============================================================================