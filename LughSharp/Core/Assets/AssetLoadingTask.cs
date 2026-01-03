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

using LughSharp.Core.Assets.Loaders;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Assets;

/// <summary>
/// Responsible for loading an asset through an <see cref="AssetLoader"/> based
/// on an <see cref="AssetDescriptor"/>.
/// </summary>
[PublicAPI]
public class AssetLoadingTask : IAssetTask
{
    public AssetManager    Manager   { get; set; }
    public AssetDescriptor AssetDesc { get; }
    public AssetLoader     Loader    { get; set; }
    public AsyncExecutor   Executor  { get; set; }
    public long            StartTime { get; set; }

    // ========================================================================

    public volatile bool                     AsyncDone;
    public volatile bool                     DependenciesLoaded;
    public volatile List< AssetDescriptor >? Dependencies;
    public volatile Task?                    DepsFuture;
    public volatile Task?                    LoadFuture;
    public volatile object?                  Asset;
    public volatile bool                     Cancel;

    // ========================================================================

    public bool IsAsyncLoader { get; private set; }

    // ========================================================================

    private readonly object _lock = new();

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Represents a task for loading an asset, including managing dependencies and handling cancellation.
    /// </summary>
    public AssetLoadingTask( AssetManager manager,
                             AssetDescriptor assetDesc,
                             AssetLoader loader,
                             AsyncExecutor threadPool )
    {
        Logger.Checkpoint();

        AssetDesc = assetDesc;
        Manager   = manager;
        Loader    = loader;
        Executor  = threadPool;

        IsAsyncLoader = Loader is AsynchronousAssetLoader;
        StartTime     = Logger.TraceLevel.Equals( Logger.LOG_DEBUG ) ? TimeUtils.NanoTime() : 0;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Call()
    {
        if ( Cancel )
        {
            return;
        }

        var asyncLoader = ( AsynchronousAssetLoader )Loader;
        
        if ( !DependenciesLoaded )
        {
            Dependencies = asyncLoader.GetDependencies( AssetDesc.AssetName,
                                                        Resolve( Loader, AssetDesc )!,
                                                        AssetDesc.Parameters );

            if ( Dependencies != null )
            {
                RemoveDuplicates( Dependencies );
                Manager.InjectDependencies( AssetDesc.AssetName, Dependencies );
            }
            else
            {
                // if we have no dependencies, we load the async part of the task immediately.
                asyncLoader.LoadAsync( Manager,
                                       AssetDesc.AssetName,
                                       Resolve( Loader, AssetDesc ),
                                       AssetDesc.Parameters );
                AsyncDone = true;
            }
        }
        else
        {
            asyncLoader.LoadAsync( Manager,
                                   AssetDesc.AssetName,
                                   Resolve( Loader, AssetDesc ),
                                   AssetDesc.Parameters );
            AsyncDone = true;
        }
    }

    /// <summary>
    /// Updates the loading of the asset.
    /// <para>
    /// In case the asset is loaded with an <see cref="AsynchronousAssetLoader"/>, the
    /// loaders <see cref="AsynchronousAssetLoader.LoadAsync"/> method is first called
    /// on a worker thread. Once this method returns, the rest of the asset is loaded on
    /// the rendering thread via <see cref="AsynchronousAssetLoader.LoadSync"/>.
    /// </para>
    /// </summary>
    /// <returns> true if the asset was fully loaded, false otherwise. </returns>
    public bool Update()
    {
        if ( Loader is AsynchronousAssetLoader )
        {
            HandleAsyncLoader();
        }
        else
        {
            HandleSyncLoader();
        }

        return Asset != null;
    }

    /// <summary>
    /// 
    /// </summary>
    private void HandleAsyncLoader()
    {
        var asyncLoader = ( AsynchronousAssetLoader )Loader;

        lock ( _lock )
        {
            if ( !DependenciesLoaded )
            {
                if ( DepsFuture == null )
                {
                    DepsFuture = Executor.Submit( this );
                }
                else if ( DepsFuture.IsCompleted )
                {
                    try
                    {
                        // Task.Wait() blocks and re-throws the exception from the background thread, 
                        // analogous to future.get() for a Void/non-generic type.
                        DepsFuture.Wait();
                    }
                    catch ( Exception e )
                    {
                        // C# interpolated string and retaining GdxRuntimeException
                        // Get the first inner exception, which is usually the original exception.
                        var inner = e.InnerException ?? e;

                        throw new GdxRuntimeException( $"Couldn't load asset: {AssetDesc.AssetName}", inner );
                    }

                    DependenciesLoaded = true;

                    if ( AsyncDone )
                    {
                        Asset = asyncLoader.LoadSync( Manager,
                                                      Resolve( Loader, AssetDesc )!,
                                                      AssetDesc.Parameters );
                    }
                }
            }
            else if ( LoadFuture == null && !AsyncDone )
            {
                LoadFuture = Executor.Submit( this );
            }
            else if ( AsyncDone )
            {
                Asset = asyncLoader.LoadSync( Manager, Resolve( Loader, AssetDesc )!, AssetDesc.Parameters );
            }
            else if ( LoadFuture!.IsCompleted )
            {
                try
                {
                    LoadFuture.Wait();
                }
                catch ( Exception e )
                {
                    throw new GdxRuntimeException( $"Couldn't load asset: {AssetDesc.AssetName}", e );
                }

                Asset = asyncLoader.LoadSync( Manager, Resolve( Loader, AssetDesc )!, AssetDesc.Parameters );
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void HandleSyncLoader()
    {
        var syncLoader = Loader as SynchronousAssetLoader< Type >;

        if ( !DependenciesLoaded )
        {
            DependenciesLoaded = true;
            Dependencies = syncLoader?.GetDependencies( AssetDesc.AssetName,
                                                       Resolve( Loader, AssetDesc )!,
                                                       AssetDesc.Parameters );

            if ( Dependencies == null )
            {
                Asset = syncLoader?.Load( Manager, Resolve( Loader, AssetDesc )!, AssetDesc.Parameters! );

                return;
            }

            RemoveDuplicates( Dependencies );
            Manager.InjectDependencies( AssetDesc.AssetName, Dependencies );
        }
        else
        {
            Asset = syncLoader?.Load( Manager, Resolve( Loader, AssetDesc )!, AssetDesc.Parameters! );
        }
    }

    /// <summary>
    /// Unloads the current asset using the associated asynchronous asset loader,
    /// if applicable.
    /// </summary>
    public void Unload()
    {
        if ( Loader is AsynchronousAssetLoader asyncLoader )
        {
            asyncLoader.UnloadAsync( Manager,
                                     AssetDesc.AssetName,
                                     Resolve( Loader, AssetDesc )!,
                                     AssetDesc.Parameters! );
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
        if ( ( assetDesc?.File == null ) && ( loader != null ) )
        {
            assetDesc!.File = loader.Resolve( assetDesc.AssetName );
        }

        return assetDesc?.File;
    }

    /// <summary>
    /// Removes duplicate asset descriptors from the given list. Duplicates are identified
    /// based on the asset name and asset type.
    /// </summary>
    /// <param name="array">The list of asset descriptors to process for duplicates.</param>
    private static void RemoveDuplicates( List< AssetDescriptor > array )
    {
        for ( var i = 0; i < array.Count; ++i )
        {
            var fn   = array[ i ].AssetName;
            var type = array[ i ].AssetType;

            for ( var j = array.Count - 1; j > i; --j )
            {
                if ( type == array[ j ].AssetType && fn.Equals( array[ j ].AssetName ) )
                {
                    array.RemoveAt( j );
                }
            }
        }
    }
}

// ============================================================================
// ============================================================================