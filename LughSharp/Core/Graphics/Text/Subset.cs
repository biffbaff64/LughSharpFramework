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

using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Graphics.Text;

/// <summary>
/// Instances of this class represent particular subsets of the Unicode character set.
/// The only family of subsets defined in the <c>Character</c> class is <see cref="UnicodeBlock"/>.
/// Other portions of the Java API may define other subsets for their own purposes.
/// </summary>
public class Subset
{
    private readonly string _name;

    // ========================================================================

    /// <summary>
    /// Constructs a new <c>Subset</c> instance.
    /// </summary>
    /// <param name="name"> The name of this subset </param>
    /// <exception cref="NullReferenceException"> if name is <c>null</c>. </exception>
    protected Subset( string name )
    {
        Guard.Against.Null( name );

        _name = name;
    }

    /// <summary>
    /// Returns the name of this subset.
    /// </summary>
    public override string ToString()
    {
        return _name;
    }
}

// ============================================================================
// ============================================================================