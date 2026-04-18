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

using System;
using System.IO;

using JetBrains.Annotations;

using LughSharp.Core.Files;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Images;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Scene2D.UI;
using LughSharp.Core.Utils.Logging;

using NUnit.Framework;

using Color = LughSharp.Core.Graphics.Color;

namespace LughSharp.Tests.Source;

[TestFixture]
[PublicAPI]
public class SkinLoadingTests
{
    public void Test_Skin_Load_From_Json()
    {
        // Setup a mock JSON structure
        const string JsonContent =
            """
                    {
                        "LughSharp.Core.Graphics.Color": {
                            "green": { "r": 0, "g": 1, "b": 0, "a": 1 },
                            "custom_hex": { "hex": "ff00ff" }
                        },
                        "BitmapFont": {
                            "default-font": { "file": "Assets\\Fonts\\arial-15.fnt", "markupEnabled": true }
                        },
                        "TintedDrawable": {
                            "star": { "name": "complete_star", "color": "green" }
                        }
                    }
            """;

        // Setup file environment (Mocking the .fnt file location)
        var tempFile = new FileInfo( Path.GetTempFileName() );
        File.WriteAllText( tempFile.FullName, JsonContent );

        // Initialize Skin
        using var skin = new Skin();

        // Add a base region so the TintedDrawable has something to reference
        var mockRegion = new TextureRegion( new Texture2D( @"Assets\complete_star.png" ) );
        skin.Add( "complete_star", mockRegion );

        // Load the JSON
        skin.Load( tempFile );

        // Verify Colors
        try
        {
            _ = skin.Get< Color >( "green" );
            
            Logger.Debug( "green found." );
        }
        catch ( Exception )
        {
            Logger.Error( "green not found." );
        }

        // Verify Font Properties
        try
        {
            _ = skin.GetFont( "default-font" );
            
            Logger.Debug( "default-font found." );
        }
        catch ( Exception )
        {
            Logger.Error( "default-font not found." );
        }

        // Verify TintedDrawable logic
        try
        {
            _ = skin.GetDrawable( "star" );
            
            Logger.Debug( "TintedDrawable found." );
        }
        catch ( Exception )
        {
            Logger.Error( "TintedDrawable not found." );
        }

        // Cleanup
        tempFile.Delete();
    }
}

// ============================================================================
// ============================================================================