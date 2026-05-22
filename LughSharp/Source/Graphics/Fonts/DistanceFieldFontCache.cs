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

using LughSharp.Source.Graphics.G2D;

namespace LughSharp.Source.Graphics.Fonts;

/// <summary>
/// Provides a font cache that uses distance field shader for rendering fonts.
/// Attention: breaks batching because uniform is needed for smoothing factor,
/// so a flush is performed before and after every font rendering.
/// </summary>
[PublicAPI]
public class DistanceFieldFontCache : BitmapFontCache
{
    public DistanceFieldFontCache( DistanceFieldFont font )
        : base( font, font.GetUseIntegerPositions() )
    {
    }

    public DistanceFieldFontCache( DistanceFieldFont font, bool integer )
        : base( font, integer )
    {
    }

    public override void Draw( IBatch spriteBatch )
    {
        SetSmoothingUniform( spriteBatch, GetSmoothingFactor() );
        base.Draw( spriteBatch );
        SetSmoothingUniform( spriteBatch, 0 );
    }

    public override void Draw( IBatch spriteBatch, int start, int end )
    {
        SetSmoothingUniform( spriteBatch, GetSmoothingFactor() );
        base.Draw( spriteBatch, start, end );
        SetSmoothingUniform( spriteBatch, 0 );
    }

    public void SetSmoothingUniform( IBatch spriteBatch, float smoothing )
    {
        spriteBatch.Flush();
        spriteBatch.Shader?.SetUniformf( "u_smoothing", smoothing );
    }

    public float GetSmoothingFactor()
    {
        var font = ( DistanceFieldFont )Font;

        return font.DistanceFieldSmoothing * font.GetScaleX();
    }
}

// ============================================================================
// ============================================================================