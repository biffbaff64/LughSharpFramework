// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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

using System.Runtime.Versioning;
using Extensions.Source.Tools.TexturePacker;
using LughSharp.Core.Files;
using NUnit.Framework;

namespace LughSharp.Tests.Source;

[TestFixture]
[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class TexturePackerTest
{
    private const bool REMOVE_DUPLICATE_IMAGES = true;
    private const bool KEEP_DUPLICATE_IMAGES   = false;
    private const bool DRAW_DEBUG_LINES        = false;

    // ========================================================================

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Run()
    {
        var settings = new TexturePackerSettings
        {
            MaxWidth         = 2048,                  // Maximum Width of final atlas image
            MaxHeight        = 2048,                  // Maximum Height of final atlas image
            PowerOfTwo       = true,                  // 
            Debug            = DRAW_DEBUG_LINES,      // 
            IsAlias          = KEEP_DUPLICATE_IMAGES, // 
            Silent           = true,                 // 
            PaddingX         = 2,                     // Increase padding
            PaddingY         = 2,                     // Increase padding
            EdgePadding      = true,                  // Disable edge padding initially
            DuplicatePadding = false,                 // Disable duplicate padding
            MinWidth         = 16,                    // 
            MinHeight        = 16,                    // 
//            Grid             = true,                  // 
        };

        // Build the Atlases from the specified parameters :-
        // 1. source folder
        // 2. destination folder
        // 3. name of atlas, without extension (the extension '.atlas' will be added
        //    automatically). If an extension is specified, it will be removed.
        // 4. configuration settings
        var inputFolder  = IOUtils.AssetPath( @"Assets\PackedImages\objects" );
        var outputFolder = IOUtils.AssetPath( @"Assets\PackedImages\output" );

        // Make sure we have a settings file to use. Comment out if not needed,
        // or to test with default settings.
        var settingsFilePath = Path.Combine( inputFolder, "pack.json" );
        settings.WriteToJsonFile( settingsFilePath );

        // Calls to TexturePacker.Process() do not modify the paths with any IOUtils
        // mthods, as this is done by the TexturePacker class itself.

        TexturePacker.Process( @"Assets\PackedImages\animations", outputFolder, "animations", settings );
        TexturePacker.Process( @"Assets\PackedImages\input", outputFolder, "input", settings );
        TexturePacker.Process( @"Assets\PackedImages\objects", outputFolder, "objects", settings );
        TexturePacker.Process( @"Assets\PackedImages\text", outputFolder, "text", settings );
    }

    [TearDown]
    public void TearDown()
    {
    }
}

// ========================================================================
// ========================================================================