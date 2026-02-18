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

using System;

using JetBrains.Annotations;
using LughSharp.Core.Files;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;

namespace LughSharp.Core.Assets;

/// <summary>
/// Helper class for loading assets.
/// </summary>
[PublicAPI]
public class AssetHelper
{
    private AssetManager _assetManager;

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="assetManager"></param>
    public AssetHelper( AssetManager? assetManager )
    {
        _assetManager = assetManager ?? throw new ArgumentNullException( nameof( assetManager ) );
    }

    /// <summary>
    /// Load single asset, and ensures that it is loaded. It then returns an object
    /// of the specified type.
    /// </summary>
    /// <typeparam name="T"> The Type of the asset to load. </typeparam>
    /// <param name="asset"> the asset to load. </param>
    /// <returns>
    /// An object of the specified type, or null if the asset could not be loaded.
    /// </returns>
    public T? LoadSingleAsset< T >( string asset ) where T : class
    {
        if ( !_assetManager.IsLoaded( asset ) )
        {
            _assetManager.Load< T >( asset );
            _assetManager.FinishLoadingAsset< T >( asset );
        }

        return _assetManager.Get< T >( asset );
    }

    /// <summary>
    /// Unload the specified object
    /// </summary>
    /// <param name="asset"> the filename of the object to unload. </param>
    public void UnloadAsset< T >( string asset ) where T : class
    {
        if ( _assetManager.IsLoaded< T >( asset ) )
        {
            _assetManager.Unload( asset );
        }
    }

    /// <summary>
    /// Loads a texture atlas from the specified atlas name.
    /// </summary>
    /// <param name="atlasName"> The full name of the specified atlas </param>
    /// <returns>A <see cref="TextureAtlas"/> object representing the loaded atlas.</returns>
    public TextureAtlas LoadAtlas( string atlasName )
    {
        // Ensure that the path contains the path to the assets folder.
        atlasName = IOUtils.AssetPath( atlasName );

        return new TextureAtlas( atlasName );
    }

    /// <summary>
    /// Retrieves a texture region from the specified texture atlas using the provided asset name.
    /// </summary>
    /// <param name="atlas">
    /// The texture atlas to search for the region. If null, no region will be found.
    /// </param>
    /// <param name="assetName">The name of the asset to locate within the atlas.</param>
    /// <returns>
    /// An instance of <see cref="TextureRegion"/> corresponding to the specified asset name
    /// if found, or null if the asset does not exist in the atlas.
    /// </returns>
    public TextureRegion? GetAtlasRegion( TextureAtlas? atlas, string assetName )
    {
        return atlas?.FindRegion( assetName );
    }
}

// ============================================================================
// ============================================================================