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
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Main;
using LughSharp.Core.Utils.Logging;
using NUnit.Framework;

namespace LughSharp.Tests.Source;

[TestFixture]
public class AssetManagerTest
{
    private readonly AssetManager? _assetManager;

    // ========================================================================
    
    public AssetManagerTest( AssetManager? assetManager )
    {
        _assetManager = assetManager;
    }

    [SetUp]
    public void Setup()
    {
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
            Api.Files.Assets( "PackedImages/input/button_a.png" ),
            Api.Files.Assets( "PackedImages/input/button_b.png" ),
            Api.Files.Assets( "PackedImages/input/button_x.png" ),
            Api.Files.Assets( "PackedImages/input/button_y.png" ),
            Api.Files.Assets( "title_background.png" ),
        };

        _assetManager.Load< Texture >( files );
        _assetManager.FinishLoading();

        Logger.Debug( "Checking loaded assets...", true );
        
        foreach ( var file in files )
        {
            CheckAsset( file );
        }

        Logger.Debug( "Checking TextureAtlas assets...", true );

        // ====================================================================
        
        const string OBJECTS_ATLAS = @"Assets\PackedImages\output\objects.atlas";
        
        Logger.Debug( $"Loading atlas: {Engine.Api.Files.Internal( OBJECTS_ATLAS )}" );
        
        var objectsAtlas = new TextureAtlas( Engine.Api.Files.Internal( OBJECTS_ATLAS ) );
        
        Logger.Debug( "Fetching atlas region..." );

        var region = objectsAtlas.FindRegion( "bar9" );
        
        if ( region == null )
        {
            Logger.Error( "Unable to find requested region in objects.atlas" );
        }
        
        Logger.Debug( $"region size: {region?.GetRegionWidth()}, {region?.GetRegionHeight()}" );
        
        Logger.Debug( "Finished!", true );
    }

    [TearDown]
    public void TearDown()
    {
        _assetManager?.Dispose();
    }

    private void CheckAsset( FileInfo asset )
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

            Logger.Debug( texture != null
                              ? $"Texture loaded and retrieved: {asset.Name}"
                              : $"Unable to retrieve {asset.Name} from AssetManager" );
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