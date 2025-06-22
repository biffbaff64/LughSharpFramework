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

namespace LughSharp.Lugh.Graphics.Images;

/// <summary>
/// Base class for all Image types:-
/// <li>Common interface for all image types.</li>
/// <li>Shared basic functionality.</li>
/// <li>Clear separation between system memory and GPU memory implementations.</li>
/// <li>Easier to add new image types later.</li>
/// <li>Potential for shared utility methods.</li>
/// </summary>
[PublicAPI]
public abstract class ImageBase
{
    // Common properties
    public virtual int         Width    { get; protected set; }
    public virtual int         Height   { get; protected set; }
    public virtual int         BitDepth { get; protected set; }
    public virtual PixelFormat Format   { get; protected set; }

    // ========================================================================

    // Common methods for basic image operations
    public abstract void ClearWithColor( Color color );
    public abstract int GetPixel( int x, int y );
    public abstract void SetPixel( int x, int y, Color color );
    public abstract void SetPixel( int x, int y, int color );

    // ========================================================================

    // Common utility methods
    public bool IsInBounds( int x, int y )
        => ( x >= 0 ) && ( x < Width ) && ( y >= 0 ) && ( y < Height );
}

// ========================================================================
// ========================================================================