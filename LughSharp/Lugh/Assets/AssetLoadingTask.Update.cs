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

using LughSharp.Lugh.Assets.Loaders;

namespace LughSharp.Lugh.Assets;

public partial class AssetLoadingTask : IAsyncTask< object >
{
    /// <summary>
    /// Updates the loading of the asset.
    /// <para>
    /// In case the asset is loaded with an <see cref="AsynchronousAssetLoader"/>, the
    /// loaders <see cref="AsynchronousAssetLoader.LoadAsync"/> method is first called
    /// on a worker thread. Once this method returns, the rest of the asset is loaded on
    /// the rendering thread via <see cref="AsynchronousAssetLoader.LoadSync"/>.
    /// </para>
    /// </summary>
    /// <returns> true in case the asset was fully loaded, false otherwise. </returns>
    public bool Update()
    {
        switch ( _loader )
        {
            case AsynchronousAssetLoader asyncLoader:
                HandleAsyncLoader( asyncLoader );

                break;

            case SynchronousAssetLoader< Type, AssetLoaderParameters > syncLoader:
                HandleSyncLoader( syncLoader );

                break;

            default:
                break;
        }

        return Asset != null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="asyncLoader"></param>
    /// <exception cref="GdxRuntimeException"></exception>
    private void HandleAsyncLoader( AsynchronousAssetLoader asyncLoader )
    {
        if ( !DependenciesLoaded )
        {
            if ( DepsFuture == null )
            {
                DepsFuture = _executor.Submit< object >( this );
            }
            else if ( DepsFuture.IsDone() )
            {
                try
                {
                    DepsFuture.Get();
                }
                catch ( Exception e )
                {
                    throw new GdxRuntimeException( $"Couldn't load dependencies of asset: " +
                                                   $"{AssetDesc.AssetName}" );
                }

                DependenciesLoaded = true;

                if ( _asyncDone )
                {
                    Asset = asyncLoader.LoadSync( _manager,
                                                  Resolve( _loader, AssetDesc )!,
                                                  AssetDesc.Parameters );
                }
            }
        }
        else if ( LoadFuture == null && !_asyncDone )
        {
            LoadFuture = _executor.Submit< object >( this );
        }
        else if ( _asyncDone )
        {
            Asset = asyncLoader.LoadSync( _manager,
                                          Resolve( _loader, AssetDesc )!,
                                          AssetDesc.Parameters );
        }
        else if ( LoadFuture!.IsDone() )
        {
            try
            {
                LoadFuture.Get();
            }
            catch ( Exception e )
            {
                throw new GdxRuntimeException( $"Couldn't load asset: {AssetDesc.AssetName}" );
            }

            Asset = asyncLoader.LoadSync( _manager,
                                          Resolve( _loader, AssetDesc )!,
                                          AssetDesc.Parameters );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="syncLoader"></param>
    private void HandleSyncLoader( SynchronousAssetLoader< Type, AssetLoaderParameters > syncLoader )
    {
        //TODO: Remove the need for null suppression here.

        if ( !DependenciesLoaded )
        {
            DependenciesLoaded = true;
            Dependencies = syncLoader.GetDependencies( AssetDesc.AssetName,
                                                       Resolve( _loader, AssetDesc )!,
                                                       AssetDesc.Parameters );

            if ( Dependencies == null )
            {
                Asset = syncLoader.Load( _manager,
                                         Resolve( _loader, AssetDesc )!,
                                         AssetDesc.Parameters! );

                return;
            }

            RemoveDuplicates( Dependencies );
            _manager.InjectDependencies( AssetDesc.AssetName, Dependencies );
        }
        else
        {
            Asset = syncLoader.Load( _manager,
                                     Resolve( _loader, AssetDesc )!,
                                     AssetDesc.Parameters! );
        }
    }
}

// ========================================================================
// ========================================================================