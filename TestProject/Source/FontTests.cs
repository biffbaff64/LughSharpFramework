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

using System.IO;

using Extensions.Source;
using Extensions.Source.Freetype;

using JetBrains.Annotations;

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.BitmapFonts;
using LughSharp.Core.Graphics.Text;

namespace TestProject.Source;

[PublicAPI]
public class FontTests
{
    public BitmapFont CreateBitmapFont()
    {
        var font = new BitmapFont( new FileInfo( Assets.ArialFont ) );
        font.Cache?.Clear();
        font.Cache?.AddText( "THIS TEXT SHOULD BE RED!", 100, 100 );
        font.Cache?.Tint( Color.Red );
        
        return font;
    }
    
    public BitmapFont CreateFreeTypeFont()
    {
//        var generator = new FreeTypeFontGenerator( Engine.Files.Internal( Assets.AmbleRegular26Font ) );
//        var parameter = new FreeTypeFontGenerator.FreeTypeFontParameter
//        {
//            Size = 40
//        };

//        BitmapFont font = generator.GenerateFont( parameter );
//        font.SetColor( Color.White );
        
//        return font;

        return new BitmapFont( new FileInfo( Assets.AmbleRegular26Font ) );
    }
}

// ============================================================================
// ============================================================================
