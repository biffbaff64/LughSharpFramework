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

namespace LughSharp.Utils.source.Collections;

[PublicAPI]
public class DirectoryInfoComparer : IEqualityComparer< DirectoryInfo >
{
    /// <summary>
    /// Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
    /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
    /// <returns> true if the specified objects are equal; otherwise false. </returns>
    public bool Equals( DirectoryInfo? x, DirectoryInfo? y )
    {
        if ( x is null || y is null ) return x is null && y is null;

        // Compare based on the FullName string (the directory path)
        // We use StringComparer.OrdinalIgnoreCase for case-insensitive comparison, 
        // which is appropriate for most file systems (like Windows).
        return StringComparer.OrdinalIgnoreCase.Equals( x.FullName, y.FullName );
    }
    
    /// <summary>
    /// Returns a hash code for the specified object.
    /// The GetHashCode method must return the same hash code for objects 
    /// that are considered equal by the Equals method.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
    /// <exception cref="T:System.ArgumentNullException">
    /// The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null. </exception>
    /// <returns> A hash code for the specified object. </returns>
    public int GetHashCode( DirectoryInfo? obj )
    {
        if ( obj is null )
        {
            return 0;
        }

        // Return the hash code of the path string
        return StringComparer.OrdinalIgnoreCase.GetHashCode( obj.FullName );
    }
}

// ============================================================================
// ============================================================================
