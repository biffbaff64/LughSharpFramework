// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024, 2025, 2026 Circa64 Software Projects / Richard Ikin.
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

using LughSharp.Source.Graphics.Fonts;

namespace LughSharp.Source.Scene2D.RegistryStyles;

[PublicAPI]
public record TextButtonStyleRecord
{
    public BitmapFont Font                    { get; set; } = new ();
    public Color      FontColor               { get; set; } = new ( 1, 1, 1, 1 );
    public Color      DownFontColor           { get; set; } = new ( 1, 1, 1, 1 );
    public Color      OverFontColor           { get; set; } = new ( 1, 1, 1, 1 );
    public Color      FocusedFontColor        { get; set; } = new ( 1, 1, 1, 1 );
    public Color      DisabledFontColor       { get; set; } = new ( 0.7f, 0.7f, 0.7f, 1 );
    public Color      CheckedFontColor        { get; set; } = new ( 1, 1, 1, 1 );
    public Color      CheckedDownFontColor    { get; set; } = new ( 1, 1, 1, 1 );
    public Color      CheckedOverFontColor    { get; set; } = new ( 1, 1, 1, 1 );
    public Color      CheckedFocusedFontColor { get; set; } = new ( 1, 1, 1, 1 );
}

// ============================================================================
// ============================================================================
