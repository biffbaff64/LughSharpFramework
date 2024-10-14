// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin / Red 7 Projects and Contributors.
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

using LughSharp.LibCore.Utils.Async;

namespace LughSharp.LibCore.Assets;

/// <summary>
/// Represents a task for loading an asset, including managing dependencies and handling cancellation.
/// </summary>
[PublicAPI]
public class AssetLoadingTask
{
    public bool                     DependenciesLoaded { get; set; }
    public List< AssetDescriptor >? Dependencies       { get; set; }
    public AssetDescriptor          AssetDesc          { get; }
    public bool                     Cancel             { get; set; }
    public object?                  Asset              { get; set; }

    private readonly AsyncExecutor _executor;
    private readonly AssetLoader   _loader;
    private readonly AssetManager  _manager;
    private readonly long          _startTime;

    private Task< object? >? _loadTask;

    private bool IsAsyncLoader => _loader is AsynchronousAssetLoader< Type, AssetLoaderParameters >;

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    /// <summary>
    /// Represents a task for loading an asset, including managing dependencies and handling cancellation.
    /// </summary>
    public AssetLoadingTask( AssetManager manager, AssetDescriptor assetDesc, AssetLoader loader, AsyncExecutor threadPool )
    {
        Logger.Checkpoint();

        _manager   = manager;
        _executor  = threadPool;
        AssetDesc  = assetDesc;
        _loader    = loader;
        _startTime = Logger.TraceLevel == Logger.LOG_DEBUG ? TimeUtils.NanoTime() : 0;
    }

    /// <summary>
    /// Asynchronously loads an asset along with its dependencies based on the type of asset loader provided.
    /// Can handle both asynchronous and synchronous asset loaders, ensuring dependencies are resolved
    /// and the asset is either loaded asynchronously or synchronously as required.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous operation. The task contains the loaded asset object or null
    /// if the loading task was cancelled or the loader was not recognized.
    /// </returns>
    public async Task< object? > LoadAsync()
    {
        if ( Cancel )
        {
            return null;
        }

        if ( _loader is AsynchronousAssetLoader< Type, AssetLoaderParameters > asyncLoader )
        {
            await LoadDependenciesAsync( asyncLoader );
            return await LoadAssetAsync( asyncLoader );
        }
        
        if ( _loader is SynchronousAssetLoader< Type, AssetLoaderParameters > syncLoader )
        {
            return await Task.Run( () => LoadSynchronously( syncLoader ) );
        }

        return null;
    }

    /// <summary>
    /// Asynchronously loads the dependencies required for an asset using the
    /// provided asynchronous asset loader. Ensures all dependencies are resolved
    /// and loaded before continuing to load the actual asset.
    /// </summary>
    /// <param name="loader">
    /// The asynchronous asset loader responsible for fetching the dependencies
    /// required by the asset.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task contains the resolved
    /// dependencies or completes when dependencies are loaded.
    /// </returns>
    private async Task LoadDependenciesAsync( AsynchronousAssetLoader< Type, AssetLoaderParameters > loader )
    {
        if ( !DependenciesLoaded )
        {
            Dependencies = await Task.Run( () => loader.GetDependencies( AssetDesc.AssetName,
                Resolve( loader, AssetDesc ), AssetDesc.Parameters ) );

            if ( Dependencies != null )
            {
                RemoveDuplicates( Dependencies );
                _manager.InjectDependencies( AssetDesc.AssetName, Dependencies );
            }
            else
            {
                // If we have no dependencies, we load the async part of the task immediately.
                await Task.Run( () => loader.LoadAsync( _manager, Resolve( loader, AssetDesc ), AssetDesc.Parameters! ) );
            }

            DependenciesLoaded = true;
        }
    }

    /// <summary>
    /// Asynchronously loads an asset using the provided asynchronous asset loader.
    /// Ensures that dependencies are loaded prior to loading the asset itself.
    /// </summary>
    /// <param name="loader">The asynchronous asset loader to be used for loading the asset.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the loaded
    /// asset object or null if loading fails.
    /// </returns>
    private async Task< object? > LoadAssetAsync( AsynchronousAssetLoader< Type, AssetLoaderParameters > loader )
    {
        if ( !DependenciesLoaded )
        {
            await LoadDependenciesAsync( loader );
        }

        await Task.Run( () => loader.LoadAsync( _manager, Resolve( loader, AssetDesc ), AssetDesc.Parameters! ) );
        return loader.LoadSync( _manager, Resolve( loader, AssetDesc ), AssetDesc.Parameters! );
    }

    /// <summary>
    /// Loads an asset synchronously using the provided loader.
    /// This method handles dependency resolution before loading the asset itself.
    /// </summary>
    /// <param name="loader">The synchronous asset loader to be used for loading the asset.</param>
    /// <returns>The loaded asset object or null if loading fails.</returns>
    private object? LoadSynchronously( SynchronousAssetLoader< Type, AssetLoaderParameters > loader )
    {
        if ( !DependenciesLoaded )
        {
            Dependencies = loader.GetDependencies( AssetDesc.AssetName, Resolve( loader, AssetDesc ), AssetDesc.Parameters );
            if ( Dependencies != null )
            {
                RemoveDuplicates( Dependencies );
                _manager.InjectDependencies( AssetDesc.AssetName, Dependencies );
            }

            DependenciesLoaded = true;
        }

        return loader.Load( _manager, Resolve( loader, AssetDesc )!, AssetDesc.Parameters! );
    }

    /// <summary>
    /// Unloads the current asset using the associated asynchronous asset loader,
    /// if applicable.
    /// </summary>
    public void Unload()
    {
        if ( _loader is AsynchronousAssetLoader< Type, AssetLoaderParameters > asyncLoader )
        {
            asyncLoader.UnloadAsync( _manager, Resolve( _loader, AssetDesc ), AssetDesc.Parameters! );
        }
    }

    /// <summary>
    /// Resolves the file information for the given asset descriptor using the
    /// provided asset loader, if needed.
    /// </summary>
    /// <param name="loader">The asset loader to use for resolving the file information.</param>
    /// <param name="assetDesc">The asset descriptor containing the asset details to resolve.</param>
    /// <returns>The resolved file information, or null if it cannot be resolved.</returns>
    private static FileInfo? Resolve( AssetLoader? loader, AssetDescriptor? assetDesc )
    {
        if ( assetDesc?.File == null && loader != null )
        {
            assetDesc!.File = loader.Resolve( assetDesc.AssetName );
        }

        return assetDesc?.File;
    }

    /// <summary>
    /// Removes duplicate asset descriptors from the given list. Duplicates are identified
    /// based on the asset00000000000000000000000000000000000000000000000000000 name and asset type.
    /// </summary>
    /// <param name="array">The list of asset descriptors to process for duplicates.</param>
    private static void RemoveDuplicates( List< AssetDescriptor > array )
    {
        var uniqueAssets = array
                           .GroupBy( a => new { a.AssetName, a.AssetType } )
                           .Select( g => g.First() )
                           .ToList();

        array.Clear();
        array.AddRange( uniqueAssets );
    }
}