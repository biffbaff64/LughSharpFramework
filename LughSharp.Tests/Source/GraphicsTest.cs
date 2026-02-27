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

using LughSharp.Core.Assets;
using LughSharp.Core.Graphics;
using LughSharp.Core.Main;

using NUnit.Framework;

namespace LughSharp.Tests.Source;

[TestFixture]
public class GraphicsTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Run()
    {
        var files = new List< FileInfo >
        {
            Api.Files.Assets( "PackedImages/Objects/red7logo.png" ),
            Api.Files.Assets( "PackedImages/Objects/rover_wheel.png" ),
            Api.Files.Assets( "PackedImages/Objects/libgdx.png" ),
            Api.Files.Assets( "title_background.png" )
        };

//        Pixmap pixmap;
        var assetManager = new AssetManager();

        assetManager.Load< Pixmap >( files[ 0 ].FullName );
        assetManager.Load< Pixmap >( files[ 1 ].FullName );
        assetManager.Load< Pixmap >( files[ 2 ].FullName );
        assetManager.Load< Pixmap >( files[ 3 ].FullName );
        assetManager.FinishLoading();

//        pixmap = new Pixmap( files[ 0 ] );
//        pixmap.DebugPrint();
//        pixmap = new Pixmap( files[ 1 ] );
//        pixmap.DebugPrint();
//        pixmap = new Pixmap( files[ 2 ] );
//        pixmap.DebugPrint();
//        pixmap = new Pixmap( files[ 3 ] );
//        pixmap.DebugPrint();
    }

    [TearDown]
    public void TearDown()
    {
    }
}

// ============================================================================
// ============================================================================