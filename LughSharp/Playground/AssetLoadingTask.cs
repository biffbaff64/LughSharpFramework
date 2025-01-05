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

using LughSharp.Lugh.Assets;
using LughSharp.Lugh.Assets.Loaders;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Playground;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[PublicAPI]
public class AssetLoadingTask
{
    public bool                     DependenciesLoaded { get; private set; }
    public List< AssetDescriptor >? Dependencies       { get; private set; }
    public AssetDescriptor          AssetDesc          { get; }
    public bool                     Cancel             { get; set; }
    public object?                  Asset              { get; set; }
    public bool                     IsAsyncLoader      { get; }

    private readonly AssetLoader  _loader;
    private readonly AssetManager _manager;

    public AssetLoadingTask( AssetManager manager, AssetDescriptor assetDesc, AssetLoader loader )
    {
        AssetDesc     = assetDesc;
        _manager      = manager;
        _loader       = loader;
        IsAsyncLoader = _loader is AsynchronousAssetLoader;
    }

    public async Task LoadAsync()
    {
        if ( Cancel ) return;

        try
        {
            if ( _loader is AsynchronousAssetLoader asyncLoader )
            {
                await LoadDependenciesAsync( asyncLoader );

                if ( Cancel ) return; // Check for cancellation after dependencies are loaded.

                Asset = await LoadAssetAsync( asyncLoader );
            }
            else if ( _loader is SynchronousAssetLoader< Type, AssetLoaderParameters > syncLoader )
            {
                LoadDependenciesSync( syncLoader );

                if ( Cancel ) return; // Check for cancellation after dependencies are loaded.

                Asset = LoadSynchronously( syncLoader );
            }
            else
            {
                throw new GdxRuntimeException( "Unknown asset loader type." );
            }
        }
        catch ( Exception ex )
        {
//            _manager.HandleTaskError( ex );
        }
    }

    private async Task LoadDependenciesAsync( AsynchronousAssetLoader loader )
    {
        if ( DependenciesLoaded ) return;

        Dependencies = loader.GetDependencies( AssetDesc.AssetName, Resolve( loader, AssetDesc ), AssetDesc.Parameters );

        if ( Dependencies != null )
        {
            RemoveDuplicates( Dependencies );
//            await _manager.InjectDependenciesAsync( AssetDesc.AssetName, Dependencies );
        }

        DependenciesLoaded = true;
    }

    private void LoadDependenciesSync( SynchronousAssetLoader< Type, AssetLoaderParameters > loader )
    {
        if ( DependenciesLoaded ) return;

        Dependencies = loader.GetDependencies( AssetDesc.AssetName, Resolve( loader, AssetDesc ), AssetDesc.Parameters );

        if ( Dependencies != null )
        {
            RemoveDuplicates( Dependencies );
//            _manager.InjectDependencies( AssetDesc.AssetName, Dependencies );
        }

        DependenciesLoaded = true;
    }

    private async Task< object > LoadAssetAsync( AsynchronousAssetLoader loader )
    {
        return null!;

//        return await loader.LoadAsync( _manager, Resolve( loader, AssetDesc ), AssetDesc.Parameters! );
    }

    private object LoadSynchronously( SynchronousAssetLoader< Type, AssetLoaderParameters > loader )
    {
        return null!;

//        return loader.Load( _manager, Resolve( loader, AssetDesc ), AssetDesc.Parameters! );
    }

    public void Unload()
    {
//        if ( _loader is AsynchronousAssetLoader asyncLoader && Asset != null )
//        {
//            asyncLoader.UnloadAsync( _manager, Resolve( _loader, AssetDesc ), AssetDesc.Parameters! );
//        }
    }

    private static FileInfo? Resolve( AssetLoader? loader, AssetDescriptor? assetDesc )
    {
        if ( assetDesc?.File == null && loader != null )
        {
            assetDesc!.File = loader.Resolve( assetDesc.AssetName );
        }

        return assetDesc?.File;
    }

    private static void RemoveDuplicates( List< AssetDescriptor > array )
    {
        var uniqueAssets = array.GroupBy( a => new { a.AssetName, a.AssetType } ).Select( g => g.First() ).ToList();
        array.Clear();
        array.AddRange( uniqueAssets );
    }
}