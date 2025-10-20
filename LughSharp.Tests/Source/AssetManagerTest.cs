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

        var files = new List< FileInfo >
        {
            Engine.Api.Files.Assets( "PackedImages/Objects/red7logo.png" ),
//            Engine.Api.Files.Assets( "PackedImages/Objects/rover_wheel.png" ),
//            Engine.Api.Files.Assets( "PackedImages/Objects/libgdx.png" ),
//            Engine.Api.Files.Assets( "title_background.png" ),
        };

        foreach ( var file in files )
        {
            _assetManager.Load< Texture >( file.FullName );
        }
        _assetManager.FinishLoading();

        foreach ( var file in files )
        {
            CheckAsset< Texture >( file );
        }

        Logger.Debug( "Finished!", true );
    }

    [TearDown]
    public void TearDown()
    {
    }

    private void CheckAsset< T >( FileInfo asset )
    {
        if ( _assetManager == null )
        {
            return;
        }

        Logger.Divider();
        Logger.Divider();
        Logger.Divider();
        Logger.Debug( $"########## Checking asset {asset.Name}... ##########" );

        if ( _assetManager.Contains( asset.FullName ) )
        {
            Logger.Debug( $"AssetManager contains {asset.Name}" );

            var texture = _assetManager.Get< Texture >( asset.FullName );

            if ( texture != null )
            {
                Logger.Debug( $"Texture loaded and retrieved: {asset.Name}" );
            }
            else
            {
                Logger.Debug( $"Unable to retrieve {asset.Name} from AssetManager" );
            }
        }
        else
        {
            Logger.Debug( $"AssetManager does not contain {asset.Name}" );
        }

        Logger.Debug( $"########## Completed {asset.Name}... ##########" );
        Logger.Divider();
        Logger.Divider();
        Logger.Divider();
    }
}

// ============================================================================
// ============================================================================