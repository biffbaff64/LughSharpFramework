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

using LughSharp.Lugh.Assets;
using LughSharp.Lugh.Core;
using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics;
using LughSharp.Lugh.Graphics.Utils;
using LughUtils.source.Logging;

using NUnit.Framework;

namespace LughSharp.Tests.Source;

[TestFixture]
public class AssetManagerTest
{
    private AssetManager? _assetManager;

    [SetUp]
    public void Setup()
    {
        _assetManager = new AssetManager();
    }

    [Test]
    public void Run()
    {
        Logger.Divider();
        Logger.Debug( "Loading assets...", true );
        Logger.Divider();

        if ( _assetManager == null )
        {
            return;
        }

        var file1 = Engine.Api.Files.Assets( "PackedImages/Objects/red7logo_small.png" );
        var file2 = Engine.Api.Files.Assets( "PackedImages/Objects/rover_wheel.png" );
        var file3 = Engine.Api.Files.Assets( "PackedImages/Objects/libgdx.png" );
        var file4 = Engine.Api.Files.Assets( "title_background.png" );

        _assetManager.Load< Texture >( file1.FullName );
        _assetManager.Load< Texture >( file2.FullName );
        _assetManager.Load< Texture >( file3.FullName );
        _assetManager.Load< Texture >( file4.FullName );
        _assetManager.FinishLoading();
        
//        if ( !_assetManager.Contains( file1.FullName ) )
//        {
//            Logger.Error( $"AssetManager does not contain {file1}" );

//            return;
//        }

//        var data1 = _assetManager.Get< Texture >( file1.FullName )?.GetImageData();

//        if ( data1 == null )
//        {
//            Logger.Error( $"Failed to load image data: {file1}" );
//        }
//        else
//        {
//            PNGDecoder.AnalysePNG( data1, true );
//        }

//        _assetManager.DebugPrint();
        _assetManager.DisplayMetrics();
        
        Logger.Debug( "Finished!", true );

//        PNGDecoder.AnalysePNG( File.ReadAllBytes( file1.FullName ), true );
//        PNGDecoder.AnalysePNG( File.ReadAllBytes( file2.FullName ), true );
//        PNGDecoder.AnalysePNG( File.ReadAllBytes( file3.FullName ), true );
//        PNGDecoder.AnalysePNG( File.ReadAllBytes( file4.FullName ), true );
    }

    [TearDown]
    public void TearDown()
    {
    }
}