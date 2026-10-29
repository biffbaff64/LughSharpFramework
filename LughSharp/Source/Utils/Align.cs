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
/// Provides bit flag constants for alignment.
/// </summary>
[PublicAPI]
[Flags]
public enum Align
{
    None   = 0,
    Center = 1 << 0,
    Top    = 1 << 1,
    Bottom = 1 << 2,
    Left   = 1 << 3,
    Right  = 1 << 4,
    
    Special = 1 << 5,
    
    TopLeft      = Top | Left,
    TopCenter    = Top | Center,
    TopRight     = Top | Right,
    MiddleLeft   = Center | Left,
    MiddleCenter = Center,
    MiddleRight  = Center | Right,
    BottomLeft   = Bottom | Left,
    BottomCenter = Bottom | Center,
    BottomRight  = Bottom | Right,
}

// ============================================================================
// ============================================================================