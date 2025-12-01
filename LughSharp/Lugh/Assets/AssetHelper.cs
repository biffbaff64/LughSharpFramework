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

namespace LughSharp.Lugh.Assets;

[PublicAPI]
public class AssetHelper
{
    public AssetManager AssetManager = new();

    // ========================================================================

    /// <summary>
    /// Load single asset, and ensures that it is loaded.
    /// It then returns an object of the specified type.
    /// </summary>
    /// <typeparam name="T"> The Type of the asset to load. </typeparam>
    /// <param name="asset"> the asset to load. </param>
    /// <returns>
    /// An object of the specified type, or null if the asset could not be loaded.
    /// </returns>
    public T? LoadSingleAsset< T >( string asset )
    {
        if ( !AssetManager.IsLoaded( asset ) )
        {
            AssetManager.Load< T >( asset );
            AssetManager.FinishLoadingAsset( asset );
        }

        return AssetManager.Get< T >( asset );
    }

    /// <summary>
    /// Load TextureAtlas asset.
    /// </summary>
    /// <param name="atlasName"> the full name of the specified atlas. </param>
    public void LoadAtlas( string atlasName )
    {
        LoadSingleAsset< TextureAtlas >( atlasName );
    }

    /// <summary>
    /// Unload the specified object
    /// </summary>
    /// <param name="asset"> the filename of the object to unload. </param>
    public void UnloadAsset< T >( string asset )
    {
        if ( AssetManager.IsLoaded< T >( asset ) )
        {
            AssetManager.Unload( asset );
        }
    }
}

// ============================================================================
// ============================================================================
