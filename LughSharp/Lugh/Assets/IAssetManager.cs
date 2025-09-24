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
using LughSharp.Lugh.Assets.Loaders.Resolvers;

namespace LughSharp.Lugh.Assets;

//TODO: Is this interface REALLY necessary?

[PublicAPI]
public interface IAssetManager
{
    /// <summary>
    /// Returns the <see cref="IFileHandleResolver" /> which this
    /// AssetManager was loaded with.
    /// </summary>
    /// <returns>the file handle resolver which this AssetManager uses.</returns>
    IFileHandleResolver FileHandleResolver { get; set; }

    /// <summary>
    /// Sets an <see cref="IAssetErrorListener" /> to be invoked in case loading an asset failed.
    /// </summary>
    IAssetErrorListener? Listener { get; set; }

    /// <summary>
    /// Updates the asset manager, loading new assets and processing asset loading tasks.
    /// Returns true if all assets are loaded, otherwise false.
    /// </summary>
    /// <returns> A boolean value indicating whether all assets are loaded. </returns>
    bool Update();

    /// <summary>
    /// Asynchronously updates the AssetManager by processing queued asset loading
    /// tasks for a specified amount of time.
    /// </summary>
    /// <param name="millis">The number of milliseconds to perform updates.</param>
    /// <returns>A task that represents whether the asset manager finished processing all tasks.</returns>
    /// <exception cref="GdxRuntimeException">Thrown when an error occurs during asset loading.</exception>
    bool Update( int millis );

    /// <summary>
    /// Returns true if an asset with the specified name is loading, queued to be loaded,
    /// or has been loaded.
    /// </summary>
    bool Contains( string? fileName );

    /// <summary>
    /// Returns true if an asset with the specified name and type is loading,
    /// queued to be loaded, or has been loaded.
    /// </summary>
    bool Contains( string? fileName, Type? type );

    /// <summary>
    /// Returns whether the specified asset is contained in this manager.
    /// </summary>
    bool ContainsAsset< T >( T asset );

    /// <summary>
    /// Injects dependencies for a given parent asset by adding the specified
    /// dependent asset descriptors.
    /// </summary>
    /// <param name="parentAssetFilename">The file name of the parent asset.</param>
    /// <param name="dependendAssetDescs">The list of dependent asset descriptors to inject.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parentAssetFilename" /> is null.</exception>
    void InjectDependencies( string parentAssetFilename, List< AssetDescriptor > dependendAssetDescs );

    /// <summary>
    /// Injects dependencies asynchronously for a given parent asset by
    /// adding the specified dependent asset descriptors.
    /// </summary>
    /// <param name="parentAssetFilename">The file name of the parent asset.</param>
    /// <param name="dependendAssetDescs">The list of dependent asset descriptors to inject.</param>
    /// <returns></returns>
    Task InjectDependenciesAsync( string parentAssetFilename, List< AssetDescriptor > dependendAssetDescs );

    /// <summary>
    /// Injects a single dependency for a given parent asset.
    /// </summary>
    /// <param name="parentAssetFilename">The file name of the parent asset.</param>
    /// <param name="dependendAssetDesc">The descriptor of the dependent asset to inject.</param>
    /// <exception cref="GdxRuntimeException">Thrown if the type of the dependent asset is null.</exception>
    void InjectDependency( string parentAssetFilename, AssetDescriptor dependendAssetDesc );

    /// <summary>
    /// Increments the reference count of the dependencies for a given parent asset.
    /// If the parent asset has dependencies, their reference counts will be incremented recursively.
    /// </summary>
    /// <param name="parent">
    /// The file name of the parent asset whose dependencies' reference counts are to be incremented.
    /// </param>
    /// <exception cref="GdxRuntimeException">Thrown if the type of a dependency is null.</exception>
    void IncrementRefCountedDependencies( string parent );

    /// <summary>
    /// Sets the reference count of an asset.
    /// </summary>
    /// <param name="fileName"> The asset name. </param>
    /// <param name="refCount"> The new reference count. </param>
    void SetReferenceCount( string fileName, int refCount );

    /// <summary>
    /// Returns the reference count of an asset.
    /// </summary>
    /// <param name="fileName"> The asset name. </param>
    int GetReferenceCount( string fileName );

    /// <summary>
    /// Returns the number of currently queued assets.
    /// </summary>
    int GetQueuedAssets();

    /// <summary>
    /// Returns the progress in percent of completion.
    /// </summary>
    float GetProgress();

    /// <summary>
    /// Returns a list of all asset names.
    /// </summary>
    List< string > GetAssetNames();

    /// <summary>
    /// Returns a list of dependencies for the named asset.
    /// </summary>
    /// <param name="name"> Asset name. </param>
    /// <returns> Dependencies list. </returns>
    IEnumerable< string > GetDependencies( string name );

    /// <summary>
    /// Returns the asset type for the given asset name.
    /// </summary>
    /// <param name="name"> String holding the asset name. </param>
    /// <returns> The asset type. </returns>
    Type GetAssetType( string name );

    /// <summary>
    /// Retrieves an asset of the specified type by its name.
    /// </summary>
    /// <param name="name">The name of the asset to retrieve.</param>
    /// <returns>The asset of the specified type.</returns>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is not loaded.</exception>
    object? Get( string name );

    /// <summary>
    /// Retrieves an asset by name, throwing an exception if the asset is not found and required.
    /// </summary>
    /// <param name="name">The name of the asset to retrieve.</param>
    /// <param name="required">Determines if an exception should be thrown if the asset is not found.</param>
    /// <returns>The requested asset if found, otherwise null.</returns>
    object? Get( string name, bool required );

    /// <summary>
    /// Retrieves an asset of the specified type using the given asset descriptor.
    /// </summary>
    /// <param name="assetDescriptor">The descriptor containing the filepath and type of the asset.</param>
    /// <returns>The asset of the specified type.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the asset descriptor is null or if the filepath is null.
    /// </exception>
    object? Get( AssetDescriptor assetDescriptor );

    /// <summary>
    /// Retrieves an asset from the manager.
    /// </summary>
    /// <param name="name">The name of the asset to retrieve.</param>
    /// <param name="type">The type of the asset to retrieve.</param>
    /// <param name="required">Indicates whether to throw an exception if the asset is not found.</param>
    /// <returns>The requested asset if found; otherwise, null if not required.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the name or type is null.</exception>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is not found and it is required.</exception>
    object? Get( string name, Type? type, bool required );

    /// <summary>
    /// Retrieves all assets of the specified type and adds them to the provided list.
    /// </summary>
    /// <typeparam name="T">The type of assets to retrieve.</typeparam>
    /// <param name="type">The type of the assets to retrieve.</param>
    /// <param name="outArray">The list to which the retrieved assets will be added.</param>
    /// <returns>The list containing all assets of the specified type.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="type" /> or <paramref name="outArray" /> is null.
    /// </exception>
    /// <exception cref="GdxRuntimeException">Thrown if no assets of the specified type are found.</exception>
    List< T > GetAll< T >( Type? type, List< T > outArray );

    /// <summary>
    /// Gets the filename for the specified asset type.
    /// Will return Null if the asset is not contained in this manager.
    /// </summary>
    string? GetAssetFileName< T >( T asset );

    /// <summary>
    /// Returns the loader for the given type and the specified filename.
    /// </summary>
    /// <param name="type">The type of the loader to get</param>
    /// <param name="fileName">
    /// The filename of the asset to get a loader for, or null to get the default loader.
    /// </param>
    /// <param name="result"></param>
    /// <returns>
    /// The loader capable of loading the type and filename, or null if none exists.
    /// </returns>
    /// <exception cref="GdxRuntimeException"> If no loader was found. </exception>
    void GetLoader( Type? type, out AssetLoader? result, string? fileName = null );

    /// <summary>
    /// Called when a task throws an exception during loading. The default implementation
    /// rethrows the exception and does not use the <tt>assetDesc</tt> parameter.
    /// A subclass may supress the default implementation when loading assets where loading
    /// failure is recoverable.
    /// </summary>
    void TaskFailed( AssetDescriptor assetDesc, Exception ex );

    /// <summary>
    /// Handles a runtime/loading error in <see cref="AssetManager.Update()" /> by optionally
    /// invoking the <see cref="IAssetErrorListener" />.
    /// </summary>
    /// <param name="t"></param>
    void HandleTaskError( Exception? t );

    /// <summary>
    /// Returns TRUE if the asset identified by fileName is loaded.
    /// </summary>
    bool IsLoaded( string? fileName );

    /// <summary>
    /// Returns TRUE if the asset identified by fileName and Type is loaded.
    /// </summary>
    bool IsLoaded( string fileName, Type? type );

    /// <summary>
    /// Adds the given asset to the loading queue of the AssetManager.
    /// </summary>
    /// <param name="desc">the <see cref="AssetDescriptor" /></param>
    void AddToLoadqueue( AssetDescriptor desc );

    /// <summary>
    /// Adds the given asset to the loading queue of the AssetManager.
    /// </summary>
    /// <param name="fileName">
    /// the file name (interpretation depends on <see cref="AssetLoader" />)
    /// </param>
    /// <param name="type">the type of the asset.</param>
    /// <param name="parameters"></param>
    /// <remarks> Renamed from Load() for clarity during development. </remarks>
    void AddToLoadqueue( string? fileName, Type? type, AssetLoaderParameters? parameters );

    /// <summary>
    /// Adds an asset to the asset manager with the specified file name and type.
    /// The asset will be contained within a new RefCountedContainer instance.
    /// </summary>
    /// <param name="fileName">The file name associated with the asset.</param>
    /// <param name="type">The type of the asset.</param>
    /// <param name="asset">The asset to add.</param>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is null.</exception>
    void AddAsset( string fileName, Type? type, object? asset );

    /// <summary>
    /// Returns true when all assets are loaded. Can be called from any thread but
    /// note <see cref="AssetManager.Update()" /> or related methods must be called to process tasks.
    /// </summary>
    bool IsFinished();

    /// <summary>
    /// Asynchronously waits for all loading tasks in the AssetManager to complete.
    /// </summary>
    /// <returns></returns>
    void FinishLoading();

    /// <summary>
    /// Blocks until the specified asset is loaded.
    /// </summary>
    /// <param name="assetDesc">the AssetDescriptor of the asset</param>
    object FinishLoadingAsset( AssetDescriptor assetDesc );

    /// <summary>
    /// Asynchronously waits for the specified asset to be loaded and returns it upon completion.
    /// </summary>
    /// <param name="fileName">The name of the file of the asset to finish loading.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the loaded asset.</returns>
    object FinishLoadingAsset( string fileName );

    /// <summary>
    /// Sets a new <see cref="AssetLoader" /> for the given type.
    /// </summary>
    /// <param name="type"> the type of the asset </param>
    /// <param name="loader"> the loader</param>
    /// <param name="suffix">
    /// the suffix the filename must have for this loader to be used or null
    /// to specify the default loader.
    /// </param>
    void SetLoader( Type? type, AssetLoader loader, string suffix = "" );

    /// <summary>
    /// Asynchronously unloads an asset from the AssetManager.
    /// </summary>
    /// <param name="fileName">The name of the asset to be unloaded.</param>
    /// <returns>A task representing the asynchronous unload operation.</returns>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is not loaded or cannot be found.</exception>
    void Unload( string fileName );

    /// <summary>
    /// Clears and disposes all assets and the preloading queue.
    /// </summary>
    void ClearAsync();

    /// <summary>
    /// Disposes all assets in the manager and stops all asynchronous loading.
    /// </summary>
    void Dispose();
}