// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
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

namespace LughSharp.Playground;

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;

/*
 * Usage example:-
 * // Initialization:
 * // "Assets" is the root directory for your assets
 * AssetManager assetManager = new AssetManager("Assets");
 * 
 * // Synchronous loading:
 * byte[] imageData = assetManager.LoadAsset("textures/mytexture.png");
 * if (imageData != null)
 * {
 *     // Use imageData...
 * }
 * else
 * {
 *     Console.WriteLine("Failed to load asset");
 * }
 * 
 * // Asynchronous loading with progress:
 * IProgress<float> progress = new Progress<float>(percent =>
 * {
 *     Console.WriteLine($"Loading progress: {percent * 100:F0}%");
 * });
 * 
 * async Task LoadTextureAsync()
 * {
 *     byte[] asyncImageData = await assetManager.LoadAssetAsync("textures/anothertexture.png", progress);
 *     if (asyncImageData != null)
 *     {
 *         // Use asyncImageData...
 *     }
 * }
 * 
 * // Start the async loading:
 * LoadTextureAsync();
 */
[PublicAPI]
public class AssetManager
{
    private readonly string                                           _assetsRoot;
    private readonly ConcurrentDictionary< string, byte[]? >          _loadedAssets = new();
    private readonly ConcurrentDictionary< string, Task< byte[]? >? > _loadingTasks = new();

    public AssetManager( string assetsRoot )
    {
        _assetsRoot = assetsRoot;

        if ( !Directory.Exists( _assetsRoot ) )
        {
            Directory.CreateDirectory( _assetsRoot );
        }
    }

    public byte[]? LoadAsset( string assetPath )
    {
        if ( _loadedAssets.TryGetValue( assetPath, out var assetData ) )
        {
            return assetData; // Asset already loaded
        }

        var fullPath = Path.Combine( _assetsRoot, assetPath );

        if ( !File.Exists( fullPath ) )
        {
            return null; // Asset not found
        }

        assetData = File.ReadAllBytes( fullPath );
        _loadedAssets.TryAdd( assetPath, assetData );

        return assetData;
    }

    public async Task< byte[]? > LoadAssetAsync( string assetPath, IProgress< float >? progress = null )
    {
        if ( _loadedAssets.TryGetValue( assetPath, out var assetData ) )
        {
            return assetData; // Asset already loaded
        }

        if ( _loadingTasks.TryGetValue( assetPath, out var existingTask ) )
        {
            if ( existingTask != null ) return await existingTask; // Task already in progress
        }

        var loadingTask = LoadAssetAsyncInternal( assetPath, progress );
        _loadingTasks.TryAdd( assetPath, loadingTask );

        try
        {
            assetData = await loadingTask;
            _loadedAssets.TryAdd( assetPath, assetData );

            return assetData;
        }
        finally
        {
            // Remove task after completion/failure
            _loadingTasks.TryRemove( assetPath, out var _ );
        }
    }

    private async Task< byte[]? > LoadAssetAsyncInternal( string assetPath, IProgress< float >? progress )
    {
        var fullPath = Path.Combine( _assetsRoot, assetPath );

        if ( !File.Exists( fullPath ) )
        {
            throw new FileNotFoundException( "Asset not found.", assetPath );
        }

        await using var fileStream = new FileStream( fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true );

        var buffer         = new byte[ fileStream.Length ];
        var totalBytesRead = 0;
        int bytesRead;

        while ( ( bytesRead = await fileStream.ReadAsync(buffer.AsMemory(totalBytesRead, buffer.Length - totalBytesRead))) > 0 )
        {
            totalBytesRead += bytesRead;
            progress?.Report( ( float )totalBytesRead / fileStream.Length ); // Report progress
        }

        return buffer;
    }
}

