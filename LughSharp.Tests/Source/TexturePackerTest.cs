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

using Extensions.Source.Tools.ImagePacker;
using Extensions.Source.Tools.TexturePacker;

using JetBrains.Annotations;

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Utils;

using NUnit.Framework;

namespace LughSharp.Tests.Source;

[TestFixture]
[PublicAPI]
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
        Logger.Checkpoint();

        var settings = new TexturePacker.Settings
        {
            MaxWidth   = 2048, // Maximum Width of final atlas image
            MaxHeight  = 2048, // Maximum Height of final atlas image
            PowerOfTwo = true,
            Debug      = DRAW_DEBUG_LINES,
            IsAlias    = KEEP_DUPLICATE_IMAGES,
            Silent     = true,
            
            // Fix the padding issues
            PaddingX         = 4,     // Increase padding
            PaddingY         = 4,     // Increase padding
            EdgePadding      = false, // Disable edge padding initially
            DuplicatePadding = false, // Disable duplicate padding
            
            // Ensure minimum sizes are reasonable
            MinWidth  = 64,
            MinHeight = 64,
        };

        // Build the Atlases from the specified parameters :-
        // - configuration settings
        // - source folder
        // - destination folder
        // - name of atlas, without extension (the extension '.atlas' will be added automatically)
        var inputFolder      = IOUtils.NormalizeAssetPath( @"\Assets\PackedImages\objects" );
        var outputFolder     = IOUtils.NormalizeAssetPath( @"\Assets\PackedImages\output" );
        var settingsFilePath = Path.Combine( outputFolder, "TexturePackerTestSettings.json" );

        settings.WriteToJsonFile( settingsFilePath );

        var packer = new TexturePacker();
        packer.Process( inputFolder, outputFolder, "objects", settings );
    }

    [TearDown]
    public void TearDown()
    {
    }
}

// ========================================================================
// ========================================================================