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

using Extensions.Source.Drawing.Freetype;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Main;
using LughSharp.Core.Utils.Logging;
using Color = LughSharp.Core.Graphics.Color;

namespace Extensions.Source.Drawing;

[PublicAPI]
public class FontUtils
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fontFile"></param>
    /// <param name="size"></param>
    /// <param name="color"></param>
    /// <returns></returns>
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
    /// 
    /// </summary>
    /// <param name="fontFile"></param>
    /// <param name="size"></param>
    /// <returns></returns>
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