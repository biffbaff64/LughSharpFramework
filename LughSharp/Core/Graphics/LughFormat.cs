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

namespace LughSharp.Core.Graphics;

[PublicAPI]
public class LughFormat
{
    public const int ALPHA           = 1; // Was GDX_2D_FORMAT_ALPHA
    public const int LUMINANCE_ALPHA = 2; // Was GDX_2D_FORMAT_LUMINANCE_ALPHA
    public const int RGB888          = 3; // Was GDX_2D_FORMAT_RGB888
    public const int RGBA8888        = 4; // Was GDX_2D_FORMAT_RGBA8888
    public const int RGB565          = 5; // Was GDX_2D_FORMAT_RGB565
    public const int RGBA4444        = 6; // Was GDX_2D_FORMAT_RGBA4444
    public const int INDEXED_COLOR   = 7; // Was GDX_2D_FORMAT_COLOR_INDEX

    // ------------------------------------------
    
    public const int DEFAULT         = RGBA8888; // Was GDX_2D_FORMAT_DEFAULT
    public const int INVALID         = -1;       // Was GDX_2D_FORMAT_INVALID
    public const int UNDEFINED       = 0;
}

// ============================================================================
// ============================================================================
