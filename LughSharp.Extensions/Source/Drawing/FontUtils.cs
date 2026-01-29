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
using Extensions.Source.Drawing.Freetype;
using JetBrains.Annotations;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Main;
using LughSharp.Core.Utils.Logging;
using Color = LughSharp.Core.Graphics.Color;

namespace Extensions.Source.Drawing;

[PublicAPI]
public class FontUtils
{
    /// <summary>
    /// Creates a <see cref="BitmapFont"/> from the specified font file, size, and color.
    /// Font creation is handled via <see cref="FreeTypeFontGenerator"/>.
    /// <para>
    /// This method handles exceptions and logs errors if font creation fails.
    /// </para>
    /// </summary>
    /// <param name="fontFile"> The fontfile path. </param>
    /// <param name="size"> The requested font size. </param>
    /// <param name="color"> The requested font colour. </param>
    /// <returns> The newly created BitmapFont. </returns>
    public BitmapFont CreateFont( string fontFile, int size, Color color )
    {
        BitmapFont font;

        try
        {
            font = CreateFont( fontFile, size );
            font.SetColor( color );
        }
        catch ( Exception e )
        {
            Logger.Error( e.Message );

            font = new BitmapFont();
        }

        return font;
    }

    /// <summary>
    /// Creates a <see cref="BitmapFont"/> from the specified font file and size.
    /// Font creation is handled via <see cref="FreeTypeFontGenerator"/>.
    /// <para>
    /// This method handles exceptions and logs errors if font creation fails.
    /// </para>
    /// </summary>
    /// <param name="fontFile"> The fontfile path. </param>
    /// <param name="size"> The requested font size. </param>
    /// <returns> The newly created BitmapFont. </returns>
    public BitmapFont CreateFont( string fontFile, int size )
    {
        BitmapFont font;

        try
        {
            var generator = new FreeTypeFontGenerator( Engine.Api.Files.Internal( fontFile ) );
            var parameter = new FreeTypeFontGenerator.FreeTypeFontParameter
            {
                Size = size,
            };

            font = generator.GenerateFont( parameter );
            font.SetColor( Color.White );
        }
        catch ( Exception e )
        {
            Logger.Debug( e.Message );

            font = new BitmapFont();
        }

        return font;
    }
}

// ============================================================================
// ============================================================================
