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
using LughSharp.Core.Assets.Loaders.Resolvers;
using LughSharp.Core.Audio;
using LughSharp.Core.Files;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Graphics.Utils;
using LughSharp.Core.Scenes.Scene2D.UI;
using LughSharp.Core.Utils;

namespace LughSharp.Core.Assets;

/// <summary>
/// Loads and stores assets like textures, bitmapfonts, tile maps, sounds, music and so
/// on. Assets are reference counted. If two assets A and B both depend on another asset C,
/// C won’t be disposed until A and B have been disposed.
/// </summary>
[PublicAPI]
public partial class AssetManager : IDisposable
{
    //TODO: implement this
    [PublicAPI]
    public class AssetData
    {
        public string?              Name                { get; set; } // Short name (no path)
        public string?              FullName            { get; set; } // Full name (with path)
        public Type?                Type                { get; set; } // Asset type (e.g., Texture, Sound, etc.)
        public AssetLoader?         Loader              { get; set; } // Loader used to load the asset
        public RefCountedContainer? RefCountedContainer { get; set; } // Reference counted container holding the asset
        public List< string >?      Dependencies        { get; set; } // List of dependencies
    }

    // ========================================================================

    /// <summary>
    /// Returns the <see cref="IFileHandleResolver"/> which this
    /// AssetManager was loaded with.
    /// </summary>
    /// <returns>the file handle resolver which this AssetManager uses.</returns>
    public IFileHandleResolver FileHandleResolver { get; set; }

    // ========================================================================

    //TODO: implement this
    /// <summary>
    /// A list of all asset data currently managed by the AssetManager.
    /// </summary>
    public readonly List< AssetData > AssetDataList = [ ];

    /// <summary>
    /// A Dictionary holding the loaders for each asset type.
    /// The loaders are stored in a Dictionary with the assetloader type as key
    /// and the a second Dictionary with the asset suffix as key and the asset loader
    /// as value.
    /// </summary>
    private readonly Dictionary< Type, Dictionary< string, AssetLoader > > _loaders = [ ];

    /// <summary>
    /// A Dictionary holding the assets for each asset type. The assets are stored in a
    /// Dictionary with the asset name as key and the asset container as value. The asset
    /// container is a Dictionary holding the asset name as key and a reference counter
    /// as the value. 
    /// </summary>
    private readonly Dictionary< Type, Dictionary< string, IRefCountedContainer >? > _assets = [ ];

    /// <summary>
    /// A Dictionary holding the dependencies for each asset. The dependencies are stored in a
    /// Dictionary with the asset name as key and a list of dependencies as value.
    /// </summary>
    private readonly Dictionary< string, List< string > > _assetDependencies = [ ];

    /// <summary>
    /// Asset filename lookup table. This Dictionary holds the asset filename as the
    /// key, and the asset type as the value.
    /// </summary>
    private readonly Dictionary< string, Type > _assetTypes = [ ];

    private readonly List< string >            _injected    = [ ];
    private readonly List< AssetDescriptor >   _loadQueue   = [ ];
    private readonly Stack< AssetLoadingTask > _tasks       = [ ];
    private readonly ReaderWriterLockSlim      _loadersLock = new();

    private IAssetErrorListener? _listener;
    private AsyncExecutor        _executor;
    private int                  _loaded;
    private int                  _toLoad;
    private int                  _peakTasks;
    private bool                 _disposed = false;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new AssetManager with all default loaders.
    /// </summary>
    public AssetManager() : this( new InternalFileHandleResolver() )
    {
    }

    /// <summary>
    /// Creates a new AssetManager with optionally all default loaders. If you don't add the
    /// default loaders then you have to manually add the loaders you need, including any
    /// loaders they might depend on.
    /// </summary>
    /// <param name="resolver">The dedicated resolver to use.</param>
    /// <param name="defaultLoaders">Whether to add the default loaders (default is true).</param>
    public AssetManager( IFileHandleResolver resolver, bool defaultLoaders = true )
    {
        if ( defaultLoaders )
        {
            #if DEBUG
            Logger.Debug( "Setting Default Asset Loaders..." );
            #endif

            //@formatter:off
            SetLoader( typeof( BitmapFont ),        new BitmapFontLoader( resolver ) );
            SetLoader( typeof( IMusic ),            new MusicLoader( resolver ) );
            SetLoader( typeof( Pixmap ),            new PixmapLoader( resolver ) );
            SetLoader( typeof( ISound ),            new SoundLoader( resolver ) );
            SetLoader( typeof( TextureAtlas ),      new TextureAtlasLoader( resolver ) );
            SetLoader( typeof( Texture ),           new TextureLoader( resolver ) );
            SetLoader( typeof( Skin ),              new SkinLoader( resolver ) );
            SetLoader( typeof( ParticleEffect ),    new ParticleEffectLoader( resolver ) );
            SetLoader( typeof( PolygonRegion ),     new PolygonRegionLoader( resolver ) );
            SetLoader( typeof( ShaderProgram ),     new ShaderProgramLoader( resolver ) );
            SetLoader( typeof( Cubemap ),           new CubemapLoader( resolver ) );
            //@formatter:on

            //TODO:
            // 3D Particle effect loader here...
            // .g3dj loader here...
            // .g3db loader here...
            // .obj loader here...
            // I18NBundle loader here...
        }

        _executor          = new AsyncExecutor( 1, "AssetManager" );
        FileHandleResolver = resolver;
    }

    // ========================================================================
    // ========================================================================

    #region Non-Generic Methods

    /// <summary>
    /// Retrieves an asset of the specified type by its name.
    /// </summary>
    /// <param name="name">The name of the asset to retrieve.</param>
    /// <returns>The asset of the specified type.</returns>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is not loaded.</exception>
    public object? Get( string name )
    {
        lock ( this )
        {
            var type = GetAssetType( name );

            _assets.TryGetValue( type, out var assetsByType );
            assetsByType!.TryGetValue( name, out var assetContainer );

            return assetContainer != null
                ? assetContainer.Asset
                : throw new GdxRuntimeException( $"Asset not loaded: {name}" );
        }
    }

    /// <summary>
    /// Returns true if an asset with the specified name is loading,
    /// queued to be loaded, or has been loaded.
    /// </summary>
    public bool Contains( string? filename )
    {
        lock ( this )
        {
            if ( filename == null )
            {
                return false;
            }

            if ( ( _tasks.Count > 0 )
              && _tasks.First().AssetDesc is { } assetDescriptor
              && assetDescriptor.AssetName.Equals( filename ) )
            {
                return true;
            }

            foreach ( var lq in _loadQueue )
            {
                if ( lq.AssetName.Equals( filename ) )
                {
                    return true;
                }
            }

            return IsLoaded( filename );
        }
    }

    /// <summary>
    /// Returns TRUE if the asset identified by filename is loaded.
    /// </summary>
    public bool IsLoaded( string? filename )
    {
        lock ( this )
        {
            return ( filename != null ) && _assetTypes.ContainsKey( filename );
        }
    }

    /// <summary>
    /// Returns TRUE if the asset identified by filename and Type is loaded.
    /// </summary>
    public bool IsLoaded( string filename, Type? type )
    {
        lock ( this )
        {
            Guard.Against.Null( type );
            Guard.Against.Null( _assets );

            // Retrieve all assets of the required type
            _assets.TryGetValue( type, out var assetsByType );

            return assetsByType?[ filename ] != null;
        }
    }

    /// <summary>
    /// Adds the given asset to the loading queue of the AssetManager.
    /// </summary>
    /// <param name="desc">the <see cref="AssetDescriptor"/></param>
    public void Load( AssetDescriptor desc )
    {
        Guard.Against.Null( desc );

        lock ( this )
        {
            Load( desc.AssetName, desc.AssetType, desc.Parameters );
        }
    }

    /// <summary>
    /// Adds the given asset to the loading queue of the AssetManager.
    /// </summary>
    /// <param name="filename">
    /// the file name (interpretation depends on <see cref="AssetLoader"/>)
    /// </param>
    /// <param name="type">the type of the asset.</param>
    /// <param name="parameters"></param>
    public void Load( string? filename, Type? type, AssetLoaderParameters? parameters )
    {
        Guard.Against.Null( filename );
        Guard.Against.Null( type );

        lock ( this )
        {
            // Confirm availability of the loader for the supplied asset.
            if ( GetLoader( type, filename ) == null )
            {
                throw new GdxRuntimeException( $"No loader for type: {type.Name}" );
            }

            if ( _loadQueue.Count == 0 )
            {
                // Nothing in the load queue, reset stats
                _loaded    = 0;
                _toLoad    = 0;
                _peakTasks = 0;
            }

            ValidatePreloadQueue( filename, type );

            _toLoad++;

            var descriptor = new AssetDescriptor( filename, type, parameters );

            // Add this asset to the load queue
            _loadQueue.Add( descriptor );
        }
    }

    /// <summary>
    /// Asynchronously unloads an asset from the AssetManager.
    /// </summary>
    /// <param name="filename">The name of the asset to be unloaded.</param>
    /// <returns>A task representing the asynchronous unload operation.</returns>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is not loaded or cannot be found.</exception>
    public void Unload( string filename )
    {
        filename = IOUtils.NormalizePath( filename );

        // Check if it's currently processed (and the first element in the queue,
        // thus not a dependency) and cancel if necessary
        if ( TryCancelCurrentTask( filename ) )
        {
            return;
        }

        // Get the type of the asset
        if ( !_assetTypes.TryGetValue( filename, out var type ) )
        {
            throw new GdxRuntimeException( $"Asset not loaded: {filename}" );
        }

        // Check if the asset is in the load queue
        if ( TryRemoveFromQueue( filename, type ) )
        {
            return;
        }

        // Handle unloading the asset and its dependencies
        UnloadAsset( filename, type );
    }

    /// <summary>
    /// Returns true when all assets are loaded. Can be called from any thread but
    /// note <see cref="Update()"/> or related methods must be called to process tasks.
    /// </summary>
    public bool IsFinished()
    {
        lock ( this )
        {
            return ( _loadQueue.Count == 0 ) && ( _tasks.Count == 0 );
        }
    }

    /// <summary>
    /// Asynchronously waits for all loading tasks in the AssetManager to complete.
    /// </summary>
    public void FinishLoading()
    {
        while ( !Update() )
        {
            Task.Yield();
        }
    }

    /// <summary>
    /// Blocks until the specified asset is loaded.
    /// </summary>
    /// <param name="assetDesc"> the AssetDescriptor of the asset </param>
    public T FinishLoadingAsset< T >( AssetDescriptor assetDesc )
    {
        return FinishLoadingAsset< T >( assetDesc.AssetName );
    }

    /// <summary>
    /// Asynchronously waits for the specified asset to be loaded and returns it upon completion.
    /// </summary>
    /// <param name="filename">The name of the file of the asset to finish loading.</param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the loaded asset.
    /// </returns>
    public T FinishLoadingAsset< T >( string filename )
    {
        Guard.Against.Null( filename );

        while ( true )
        {
            lock ( this )
            {
                var type = _assetTypes.Get( filename );

                // If type is null, asset has not finished loading, and has
                // not been added to assetTypes.
                if ( type != null )
                {
                    _assets.TryGetValue( type, out var assetsByType );

                    assetsByType!.TryGetValue( filename, out var assetContainer );

                    var asset = assetContainer?.Asset;

                    if ( asset != null )
                    {
                        return ( T )asset;
                    }
                }

                Update();
            }

            Thread.Yield();
        }
    }

    #endregion Non-Generic Methods

    // ========================================================================
    // ========================================================================

    #region Generic Methods

    /// <summary>
    /// Retrieves an asset of the specified type by its name.
    /// </summary>
    /// <param name="name">The name of the asset to retrieve.</param>
    /// <returns>The asset of the specified type.</returns>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is not loaded.</exception>
    public T? Get< T >( string name ) where T : class
    {
        lock ( this )
        {
            return Get< T >( name, typeof( T ), true );
        }
    }

    /// <summary>
    /// Retrieves an asset by name, throwing an exception if the asset is not found and required.
    /// </summary>
    /// <param name="name">The name of the asset to retrieve.</param>
    /// <param name="required">
    /// Determines if an exception should be thrown if the asset is not found.
    /// </param>
    /// <returns>The requested asset if found, otherwise null.</returns>
    public T? Get< T >( string name, bool required ) where T : class
    {
        lock ( this )
        {
            var type = GetAssetType( name );

            _assets.TryGetValue( type, out var assetsByType );
            assetsByType!.TryGetValue( name, out var assetContainer );

            if ( assetContainer != null )
            {
                return assetContainer.Asset as T;
            }

            return required
                ? throw new GdxRuntimeException( $"Asset not loaded: {name}" )
                : null;
        }
    }

    /// <summary>
    /// Retrieves an asset of the specified type using the given asset descriptor.
    /// </summary>
    /// <param name="assetDescriptor">The descriptor containing the filepath and type of the asset.</param>
    /// <returns>The asset of the specified type.</returns>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if the asset descriptor is null or if the filepath is null.
    /// </exception>
    public T? Get< T >( AssetDescriptor assetDescriptor ) where T : class
    {
        lock ( this )
        {
            return Get< T >( assetDescriptor.AssetName, assetDescriptor.AssetType, true );
        }
    }

    /// <summary>
    /// Retrieves an asset from the manager.
    /// </summary>
    /// <param name="name">The name of the asset to retrieve.</param>
    /// <param name="type">The type of the asset to retrieve.</param>
    /// <param name="required">Indicates whether to throw an exception if the asset is not found.</param>
    /// <returns>The requested asset if found; otherwise, null if not required.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the name or type is null.</exception>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is not found and it is required.</exception>
    public T? Get< T >( string name, Type? type, bool required ) where T : class
    {
        Guard.Against.Null( type );

        lock ( this )
        {
            name = IOUtils.NormalizePath( name );

            _assets.TryGetValue( type, out var assetsByType );
            assetsByType!.TryGetValue( name, out var assetContainer );

            if ( required && ( assetContainer == null ) )
            {
                throw new GdxRuntimeException( $"Asset not loaded: {name}" );
            }

            return assetContainer?.Asset as T;
        }
    }

    /// <summary>
    /// Retrieves all assets of the specified type and adds them to the provided list.
    /// </summary>
    /// <typeparam name="T">The type of assets to retrieve.</typeparam>
    /// <param name="type">The type of the assets to retrieve.</param>
    /// <param name="outArray">The list to which the retrieved assets will be added.</param>
    /// <returns>The list containing all assets of the specified type.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="type"/> or <paramref name="outArray"/> is null.
    /// </exception>
    /// <exception cref="GdxRuntimeException">Thrown if no assets of the specified type are found.</exception>
    public List< T > GetAll< T >( Type? type, List< T > outArray )
    {
        Guard.Against.Null( type );
        Guard.Against.Null( outArray );

        lock ( this )
        {
            lock ( this )
            {
                if ( !_assets.TryGetValue( type, out var assetsByType ) || ( assetsByType == null ) )
                {
                    throw new GdxRuntimeException( $"No assets loaded for type {type.FullName}" );
                }

                foreach ( var assetContainer in assetsByType.Values )
                {
                    if ( assetContainer.Asset is T asset )
                    {
                        outArray.Add( asset );
                    }
                }
            }

            return outArray;
        }
    }

    /// <summary>
    /// Returns true if an asset with the specified name and type is loading,
    /// queued to be loaded, or has been loaded.
    /// </summary>
    public bool Contains< T >( string? filename )
    {
        if ( filename == null )
        {
            return false;
        }

        lock ( this )
        {
            filename = IOUtils.NormalizePath( filename );

            if ( _tasks.Count > 0 )
            {
                if ( _tasks.First().AssetDesc is { } assetDesc
                  && ( assetDesc.AssetType == typeof( T ) )
                  && assetDesc.AssetName.Equals( filename ) )
                {
                    return true;
                }
            }

            foreach ( var assetDesc in _loadQueue )
            {
                if ( ( assetDesc.AssetType == typeof( T ) )
                  && assetDesc.AssetName.Equals( filename ) )
                {
                    return true;
                }
            }

            return IsLoaded< T >( filename );
        }
    }

    /// <summary>
    /// Returns whether the specified asset is contained in this manager.
    /// </summary>
    /// <param name="asset"> The asset to check. </param>
    /// <typeparam name="T"> The asset type ( Texture, Cubemap etc. ) </typeparam>
    /// <returns> True if the asset is contained in this manager. </returns>
    public bool ContainsAsset< T >( T asset )
    {
        lock ( this )
        {
            if ( asset == null )
            {
                return false;
            }

            _assets.TryGetValue( asset.GetType(), out var assetsByType );

            if ( assetsByType == null )
            {
                return false;
            }

            foreach ( var assetRef in assetsByType.Values )
            {
                if ( assetRef.Asset != null )
                {
                    if ( assetRef.Asset.Equals( asset )
                      || asset.Equals( assetRef.Asset ) )
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Gets the filename for the specified asset type.
    /// Will return Null if the asset is not contained in this manager.
    /// </summary>
    /// <param name="asset"> The asset to check. </param>
    /// <typeparam name="T"> The asset type ( Texture, Cubemap etc. ) </typeparam>
    public string? GetAssetFileName< T >( T asset )
    {
        lock ( this )
        {
            GdxRuntimeException.ThrowIfNull( _assets );

            if ( asset == null )
            {
                return null;
            }

            foreach ( var assetType in _assets.Keys )
            {
                _assets.TryGetValue( assetType, out var assetsByType );

                if ( assetsByType == null )
                {
                    throw new GdxRuntimeException( $"Failed to get assets by type: {assetType}" );
                }

                foreach ( var entry in assetsByType )
                {
                    var otherAsset = entry.Value.Asset;

                    if ( ( otherAsset?.GetType() == asset.GetType() )
                      || asset.Equals( otherAsset ) )
                    {
                        return entry.Key;
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Returns TRUE if the asset identified by filename and Type is loaded.
    /// </summary>
    public bool IsLoaded< T >( string? filename )
    {
        lock ( this )
        {
            Guard.Against.Null( _assets );
            Guard.Against.Null( filename );

            _assets.TryGetValue( typeof( T ), out var assetsByType );

            return assetsByType?[ filename ] != null;
        }
    }

    /// <summary>
    /// Adds the given asset to the loading queue of the AssetManager.
    /// </summary>
    /// <param name="filename">
    /// the file name (interpretation depends on <see cref="AssetLoader"/>)
    /// </param>
    /// <typeparam name="T">the type of the asset.</typeparam>
    /// <param name="parameters"></param>
    public void Load< T >( string? filename, AssetLoaderParameters? parameters = null )
    {
        Load( filename, typeof( T ), parameters );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="files"></param>
    /// <typeparam name="T"></typeparam>
    public void Load< T >( List< FileInfo > files )
    {
        foreach ( var asset in files )
        {
            Load< T >( asset.FullName );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="files"></param>
    /// <typeparam name="T"></typeparam>
    public void Load< T >( List< string > files )
    {
        foreach ( var asset in files )
        {
            Load< T >( asset );
        }
    }

    #endregion Generic Methods

    // ========================================================================
    // ========================================================================

    #region loading task methods

    /// <summary>
    /// Loads the next asset task from the load queue. If the asset is already loaded,
    /// increment the loaded count for use in <see cref="GetProgress"/>.
    /// </summary>
    private bool LoadNextTask()
    {
        lock ( this )
        {
            var assetDesc = _loadQueue.RemoveIndex( 0 );
            var isLoaded  = false;

            if ( IsLoaded( assetDesc.AssetName ) )
            {
                var type = GetAssetType( assetDesc.AssetName );
                var assetRef = _assets[ type ]![ assetDesc.AssetName ];

                assetRef.RefCount++;
                
                IncrementRefCountedDependencies( assetDesc.AssetName );

                if ( assetDesc.Parameters is { LoadedCallback: not null } )
                {
                    assetDesc.Parameters.LoadedCallback.FinishedLoading( this, assetDesc.AssetName, type );
                }
                
                _loaded++;
            }
            else
            {
                AddTask( assetDesc );

                isLoaded = true;
            }

            return isLoaded;
        }
    }

    /// <summary>
    /// Called when a task throws an exception during loading. The default implementation
    /// rethrows the exception and does not use the <tt>assetDesc</tt> parameter.
    /// A subclass may supress the default implementation when loading assets where loading
    /// failure is recoverable.
    /// </summary>
    public virtual void TaskFailed( AssetDescriptor assetDesc, Exception ex )
    {
        throw ex;
    }

    /// <summary>
    /// Adds a new asset loading task to the queue using the given asset descriptor.
    /// </summary>
    /// <param name="assetDesc">Descriptor containing information about the asset to be loaded.</param>
    /// <exception cref="GdxRuntimeException">
    /// Thrown if no loader is available for the asset type specified in the asset
    /// descriptor.
    /// </exception>
    private void AddTask( AssetDescriptor assetDesc )
    {
        var loader = GetLoader( assetDesc.AssetType, assetDesc.AssetName );

        if ( loader == null )
        {
            throw new GdxRuntimeException( $"No loader for type: {assetDesc.AssetType}" );
        }

        _tasks.Push( new AssetLoadingTask( this, assetDesc, loader, _executor ) );

        _peakTasks++;
    }

    /// <summary>
    /// Attempts to cancel the currently processing task if it matches the given file name.
    /// </summary>
    /// <param name="filename">The name of the file to check for cancellation.</param>
    /// <returns>True if the current task was successfully canceled; otherwise, false.</returns>
    private bool TryCancelCurrentTask( string filename )
    {
        if ( ( _tasks.Count > 0 ) && ( _tasks.Peek().AssetDesc.AssetName == filename ) )
        {
            var task = _tasks.Pop();
            task.Cancel = true;
            task.Unload();

            return true;
        }

        return false;
    }

    /// <summary>
    /// Updates the current task on the top of the task stack.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the asset is loaded or the task was cancelled.
    /// </returns>
    private bool UpdateTask()
    {
        if ( _tasks.TryPeek( out var task ) )
        {
            var complete = true;

            try
            {
                complete = task.Cancel || task.Update();
            }
            catch ( Exception e )
            {
                task.Cancel = true;
                TaskFailed( task.AssetDesc, e );
            }

            // if the task has been cancelled or has finished loading
            if ( complete )
            {
                // increase the number of loaded assets and pop the task from the stack
                if ( _tasks.Count == 1 )
                {
                    _loaded++;
                    _peakTasks = 0;
                }

                _ = _tasks.Pop();

                if ( task.Cancel )
                {
                    return true;
                }

                AddAsset( task.AssetDesc.AssetName, task.AssetDesc.AssetType, task.Asset );

                if ( task.AssetDesc.Parameters is { LoadedCallback: not null } )
                {
                    task.AssetDesc.Parameters.LoadedCallback.FinishedLoading( this, task.AssetDesc.AssetName,
                                                                              task.AssetDesc.AssetType );
                }

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Handles a runtime/loading error in <see cref="Update()"/> by optionally
    /// invoking the <see cref="IAssetErrorListener"/>.
    /// </summary>
    /// <param name="t"></param>
    public void HandleTaskError( Exception t )
    {
        if ( _tasks.Count == 0 )
        {
            throw new GdxRuntimeException( t );
        }

        // pop the faulty task from the stack
        var task      = _tasks.Pop();
        var assetDesc = task.AssetDesc;

        // remove all dependencies if dependences are loaded and
        // those dependencies actually exist...
        if ( task is { DependenciesLoaded: true, Dependencies: not null } )
        {
            foreach ( var desc in task.Dependencies )
            {
                Unload( desc.AssetName );
            }
        }

        // clear the rest of the stack
        _tasks.Clear();

        // inform the listener that something bad happened
        if ( _listener != null )
        {
            _listener.Error( assetDesc, t );
        }
        else
        {
            throw new GdxRuntimeException( t );
        }
    }

    #endregion loading task methods

    // ========================================================================
    // ========================================================================
    
    /// <summary>
    /// Updates the asset manager, loading new assets and processing asset loading tasks.
    /// Returns true if all assets are loaded, otherwise false.
    /// </summary>
    /// <returns> A boolean value indicating whether all assets are loaded.</returns>
    public bool Update()
    {
        lock ( this )
        {
            try
            {
                if ( _tasks.Count == 0 )
                {
                    // Load next task if there are no active tasks running.
                    while ( ( _loadQueue.Count != 0 ) && ( _tasks.Count == 0 ) )
                    {
                        LoadNextTask();
                    }

                    // If we still have no tasks, we're done
                    if ( _tasks.Count == 0 )
                    {
                        return true;
                    }
                }

                UpdateTask();

                return IsFinished();
            }
            catch ( Exception t )
            {
                HandleTaskError( t );

                return _loadQueue.Count == 0;
            }
        }
    }

    /// <summary>
    /// Asynchronously updates the AssetManager by processing queued asset loading
    /// tasks for a specified amount of time.
    /// </summary>
    /// <param name="millis">The number of milliseconds to perform updates.</param>
    /// <returns>A task that represents whether the asset manager finished processing all tasks.</returns>
    /// <exception cref="GdxRuntimeException">Thrown when an error occurs during asset loading.</exception>
    public bool Update( int millis )
    {
        try
        {
            var endTime = TimeUtils.Millis() + millis;

            while ( true )
            {
                var done = Update();

                if ( done || ( TimeUtils.Millis() >= endTime ) )
                {
                    return done;
                }

                Thread.Yield();
            }
        }
        catch ( Exception t )
        {
            HandleTaskError( t );

            return _loadQueue.Count == 0;
        }
    }

    /// <summary>
    /// Injects dependencies for a given parent asset by adding the specified
    /// dependent asset descriptors.
    /// </summary>
    /// <param name="parentAssetFilename">The file name of the parent asset.</param>
    /// <param name="dependendAssetDescs">The list of dependent asset descriptors to inject.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parentAssetFilename"/> is null.</exception>
    public void InjectDependencies( string parentAssetFilename, List< AssetDescriptor > dependendAssetDescs )
    {
        Guard.Against.Null( parentAssetFilename );

        lock ( this )
        {
            foreach ( var desc in dependendAssetDescs )
            {
                Guard.ThrowIfNull( desc.AssetName );

                if ( _injected.Contains( desc.AssetName ) )
                {
                    // Ignore subsequent dependencies if there are duplicates.
                    continue;
                }

                _injected.Add( desc.AssetName );

                InjectDependency( parentAssetFilename, desc );
            }

            _injected.Clear();
            _injected.EnsureCapacity( 32 );
        }
    }

    /// <summary>
    /// Injects a single dependency for a given parent asset.
    /// </summary>
    /// <param name="parentAssetFilename">The file name of the parent asset.</param>
    /// <param name="dependendAssetDesc">The descriptor of the dependent asset to inject.</param>
    /// <exception cref="GdxRuntimeException">Thrown if the type of the dependent asset is null.</exception>
    public void InjectDependency( string parentAssetFilename, AssetDescriptor dependendAssetDesc )
    {
        lock ( this )
        {
            // Ensure the parent asset has a dependency list initialized.
            if ( !_assetDependencies.TryGetValue( parentAssetFilename, out var dependencies ) )
            {
                dependencies = [ ];
                _assetDependencies.Add( parentAssetFilename, dependencies );
            }

            // Now safe to add to the retrieved list.
            dependencies.Add( dependendAssetDesc.AssetName );

            // If the asset is already loaded, increase its reference count.
            if ( IsLoaded( dependendAssetDesc.AssetName ) )
            {
                _assetTypes.TryGetValue( dependendAssetDesc.AssetName, out var type );

                GdxRuntimeException.ThrowIfNull( type );

                _assets.TryGetValue( type, out var asset );

                if ( asset != null )
                {
                    asset[ dependendAssetDesc.AssetName ].RefCount++;
                }

                IncrementRefCountedDependencies( dependendAssetDesc.AssetName );
            }
            else
            {
                // else add a new task for the asset.
                AddTask( dependendAssetDesc );
            }
        }
    }

    /// <summary>
    /// Increments the reference count of the dependencies for a given parent asset.
    /// If the parent asset has dependencies, their reference counts will be incremented recursively.
    /// </summary>
    /// <param name="parent">
    /// The file name of the parent asset whose dependencies' reference counts are to be incremented.
    /// </param>
    /// <exception cref="GdxRuntimeException">Thrown if the type of a dependency is null.</exception>
    public void IncrementRefCountedDependencies( string parent )
    {
        var stack = new Stack< string >();
        stack.Push( parent );

        while ( stack.Count > 0 )
        {
            var current = stack.Pop();

            if ( _assetDependencies.TryGetValue( current, out var assetDependency ) )
            {
                foreach ( var dependency in assetDependency )
                {
                    _assetTypes.TryGetValue( dependency, out var type );

                    if ( type == null )
                    {
                        throw new GdxRuntimeException( "type cannot be null!" );
                    }

                    _assets.TryGetValue( type, out var asset );

                    if ( asset == null )
                    {
                        throw new GdxRuntimeException( "asset cannot be null!" );
                    }

                    asset[ dependency ].RefCount++;

                    stack.Push( dependency );
                }
            }
        }
    }

    /// <summary>
    /// Returns the loader for the given type and the specified filename.
    /// </summary>
    /// <param name="type">The type of the loader to get</param>
    /// <param name="filename">
    /// The filename of the asset to get a loader for, or null to get the default loader.
    /// </param>
    /// <returns>
    /// The loader capable of loading the type and filename, or null if none exists.
    /// </returns>
    /// <exception cref="GdxRuntimeException"> If no loader was found. </exception>
    public AssetLoader? GetLoader( Type type, string? filename = null )
    {
        // Check if the type exists in _loaders before accessing it.
        if ( !_loaders.TryGetValue( type, out var typeLoaders )
          || ( typeLoaders.Count < 1 ) )
        {
            return null;
        }

        AssetLoader? result = null;

        if ( filename == null )
        {
            // Access the already retrieved dictionary
            typeLoaders.TryGetValue( "", out result );
        }
        else
        {
            var len = -1;

            // Iterate over the retrieved dictionary
            foreach ( var entry in typeLoaders )
            {
                if ( ( entry.Key.Length > len ) && filename.EndsWith( entry.Key ) )
                {
                    result = entry.Value;
                    len    = entry.Key.Length;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Sets a new <see cref="AssetLoader"/> for the given type.
    /// </summary>
    /// <param name="type"> the type of the asset </param>
    /// <param name="loader"> the loader </param>
    /// <param name="suffix">
    /// the suffix the filename must have for this loader to be used or null
    /// to specify the default loader.
    /// </param>
    public void SetLoader( Type type, AssetLoader loader, string? suffix = "" )
    {
        Guard.Against.Null( type );
        Guard.Against.Null( loader );
        
        // Normalize the suffix
        suffix = string.IsNullOrEmpty( suffix ) ? "" : suffix;

        _loadersLock.EnterWriteLock();

        try
        {
            if ( !_loaders.TryGetValue( type, out var typeLoaders ) )
            {
                typeLoaders      = new Dictionary< string, AssetLoader >();
                _loaders[ type ] = typeLoaders;
            }

            typeLoaders[ suffix ] = loader;
        }
        finally
        {
            _loadersLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Sets an <see cref="IAssetErrorListener"/> to be invoked in case loading an asset failed.
    /// </summary>
    public void SetErrorListener( IAssetErrorListener listener )
    {
        lock ( this )
        {
            _listener = listener;
        }
    }

    /// <summary>
    /// Returns the number of loaded assets
    /// </summary>
    public int GetLoadedAssetsCount()
    {
        lock ( this )
        {
            return _assetTypes.Count;
        }
    }

    /// <summary>
    /// Returns the number of currently queued assets.
    /// </summary>
    public int GetQueuedAssets()
    {
        lock ( this )
        {
            return _loadQueue.Count + _tasks.Count;
        }
    }

    /// <summary>
    /// Returns the progress in percent of completion.
    /// </summary>
    public float GetProgress()
    {
        lock ( this )
        {
            if ( _toLoad == 0 )
            {
                return 1f;
            }

            var fractionalLoaded = _loaded;

            if ( _peakTasks > 0 )
            {
                fractionalLoaded += ( ( _peakTasks - _tasks.Count ) / _peakTasks );
            }

            return Math.Min( 1f, fractionalLoaded / ( float )_toLoad );
        }
    }

    /// <summary>
    /// Returns a list of all asset names.
    /// </summary>
    public List< string? > GetAssetNames()
    {
        lock ( this )
        {
            var uniqueKeys = new HashSet< string? >();

            foreach ( var innerDictionary in _assets.Values )
            {
                if ( innerDictionary == null )
                {
                    continue;
                }

                foreach ( var key in innerDictionary.Keys )
                {
                    uniqueKeys.Add( key );
                }
            }

            return uniqueKeys.ToList();
        }
    }

    /// <summary>
    /// Returns a list of dependencies for the named asset.
    /// </summary>
    /// <param name="name"> Asset name. </param>
    /// <returns> Dependencies list. </returns>
    public IEnumerable< string > GetDependencies( string name )
    {
        lock ( this )
        {
            name = IOUtils.NormalizePath( name );

            _assetDependencies.TryGetValue( name, out var dependencies );

            return dependencies ?? [ ];
        }
    }

    /// <summary>
    /// Returns the asset type for the given asset name.
    /// </summary>
    /// <param name="name"> String holding the asset name. </param>
    /// <returns> The asset type. </returns>
    public Type GetAssetType( string name )
    {
        lock ( this )
        {
            name = IOUtils.NormalizePath( name );

            var result = _assetTypes.TryGetValue( name, out var assetType );

            if ( !result || ( assetType == null ) )
            {
                throw new GdxRuntimeException( $"Assets Container does not hold {name}" );
            }

            return assetType;
        }
    }

    /// <summary>
    /// Clears and disposes all assets and the preloading queue.
    /// </summary>
    public void ClearAsync()
    {
        lock ( this )
        {
            _loadQueue.Clear();
        }

        FinishLoading();

        var dependencyCount = new Dictionary< string, int >();

        while ( _assetTypes.Count > 0 )
        {
            // for each asset, figure out how often it was referenced
            dependencyCount.Clear();

            var assets = _assetTypes.Keys.ToList();

            foreach ( var asset in assets )
            {
                dependencyCount[ asset ] = 0;
            }

            foreach ( var asset in assets )
            {
                _assetDependencies.TryGetValue( asset, out var dependencies );

                if ( dependencies != null )
                {
                    foreach ( var dependency in dependencies )
                    {
                        dependencyCount.TryGetValue( dependency, out var count );
                        count++;
                        dependencyCount[ dependency ] = count;
                    }
                }
            }

            // only dispose of assets that are root assets (not referenced)
            foreach ( var asset in assets )
            {
                if ( !dependencyCount.TryGetValue( asset, out var _ ) )
                {
                    Unload( asset );
                }
            }
        }

        _loaded    = 0;
        _toLoad    = 0;
        _peakTasks = 0;

        _assets.Clear();
        _assetTypes.Clear();
        _assetDependencies.Clear();
        _loadQueue.Clear();
        _tasks.Clear();
    }

    /// <summary>
    /// Sets the reference count of an asset.
    /// </summary>
    /// <param name="filename"> The asset name. </param>
    /// <param name="refCount"> The new reference count. </param>
    public void SetReferenceCount( string filename, int refCount )
    {
        lock ( this )
        {
            _assetTypes.TryGetValue( filename, out var type );

            if ( type == null )
            {
                throw new GdxRuntimeException( $"Asset not loaded: {filename}" );
            }

            _assets[ type ]?[ filename ].RefCount = refCount;
        }
    }

    /// <summary>
    /// Returns the reference count of an asset.
    /// </summary>
    /// <param name="filename"> The asset name. </param>
    public int GetReferenceCount( string filename )
    {
        lock ( this )
        {
            _assetTypes.TryGetValue( filename, out var type );

            if ( type == null )
            {
                throw new GdxRuntimeException( $"Asset not loaded: {filename}" );
            }

            if ( _assets == null )
            {
                throw new GdxRuntimeException( "_assets list is null!" );
            }

            _assets.TryGetValue( type, out var asset );

            return asset == null
                ? throw new GdxRuntimeException( $"Asset not loaded: {filename}" )
                : asset[ filename ].RefCount;
        }
    }

    /// <summary>
    /// Adds an asset to the asset manager with the specified file name and type.
    /// The asset will be contained within a new RefCountedContainer instance.
    /// </summary>
    /// <param name="filename">The file name associated with the asset.</param>
    /// <param name="type">The type of the asset.</param>
    /// <param name="asset">The asset to add.</param>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is null.</exception>
    public void AddAsset( string filename, Type type, object? asset )
    {
        lock ( this )
        {
            _assetTypes[ filename ] = type;
            _assets.TryGetValue( type, out var typeToAssets );

            if ( typeToAssets == null )
            {
                typeToAssets    = new Dictionary< string, IRefCountedContainer >();
                _assets[ type ] = typeToAssets;
            }

            typeToAssets[ filename ] = new RefCountedContainer( asset );
        }
    }

    /// <summary>
    /// Check ths potential asset against assets currently in the preload queue and
    /// the loaded assets Dictionary, to make sure it has not already been loaded.
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="type"></param>
    private void ValidatePreloadQueue( string filename, Type? type )
    {
        lock ( this )
        {
            if ( !File.Exists( filename ) )
            {
                throw new GdxRuntimeException( $"File '{filename}' not found!" );
            }

            foreach ( var desc in _loadQueue )
            {
                if ( ( desc.AssetName == filename ) && ( desc.AssetType != type ) )
                {
                    throw new GdxRuntimeException
                        ( $"Asset with name '{filename}' already in preload queue, but has different " +
                          $"type (expected: {type?.Name}, found: {desc.AssetType.Name})" );
                }
            }

            // Try to find an asset in the preload queue with the same name as this
            // asset, but with a different asset type.
            for ( var i = 0; i < _tasks.Count; i++ )
            {
                var desc = _tasks.ElementAt( i ).AssetDesc;

                if ( ( desc.AssetName == filename ) && ( desc.AssetType != type ) )
                {
                    throw new GdxRuntimeException
                        ( $"Asset with name '{filename}' already in preload queue, but has different " +
                          $"type (expected: {type?.Name}, found: {desc.AssetType.Name})" );
                }
            }

            // Try to find an asset in the loaded assets Dictionary that has the same
            // name as this asset, but which has a different asset type.
            if ( _assetTypes.ContainsKey( filename ) )
            {
                var otherType = _assetTypes.Get( filename );

                if ( ( otherType != null ) && ( otherType != type ) )
                {
                    throw new GdxRuntimeException
                        ( $"Asset with name '{filename}' already loaded, but has different " +
                          $"type (expected: {type?.Name}, found: {otherType.Name})" );
                }
            }
        }
    }

    /// <summary>
    /// Attempts to remove an asset from the load queue based on its file name and type.
    /// </summary>
    /// <param name="filename">The name of the file corresponding to the asset to be removed.</param>
    /// <param name="type">The type of the asset to be removed.</param>
    /// <returns>
    /// true if the asset was successfully found and removed from the load queue; otherwise, false.
    /// </returns>
    private bool TryRemoveFromQueue( string filename, Type? type )
    {
        for ( var i = 0; i < _loadQueue.Count; i++ )
        {
            if ( _loadQueue[ i ].AssetName.Equals( filename ) )
            {
                _toLoad--;

                var desc = _loadQueue[ i ];
                _loadQueue.RemoveAt( i );

                // Notify callback if the asset was already loaded
                desc.Parameters?.LoadedCallback?.FinishedLoading( this, desc.AssetName, desc.AssetType );

                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Asynchronously unloads the specified asset to free up resources.
    /// </summary>
    /// <param name="filename">The name of the asset to unload.</param>
    /// <param name="type">The type of the asset to unload.</param>
    /// <returns>A task representing the asynchronous unload operation.</returns>
    private void UnloadAsset( string filename, Type? type )
    {
        if ( type == null )
        {
            throw new GdxRuntimeException( $"Asset not loaded: {filename}: Type not specified." );
        }

        GdxRuntimeException.ThrowIfNull( _assets );

        IRefCountedContainer? container = null;

        if ( !_assets.TryGetValue( type, out var assetRef )
          && ( ( assetRef != null ) && !assetRef.TryGetValue( filename, out container ) ) )
        {
            throw new GdxRuntimeException( $"Asset not loaded: {filename}" );
        }

        if ( container != null )
        {
            container.RefCount--;

            if ( container.RefCount <= 0 )
            {
                DisposeAsset( container );

                _assetTypes.Remove( filename );
                assetRef?.Remove( filename );

                // Remove dependencies if the asset is completely unloaded
                RemoveDependencies( filename );
            }
        }
    }

    /// <summary>
    /// Disposes of the assets contained within the provided IRefCountedContainer.
    /// </summary>
    /// <param name="container">The container that holds the asset to be disposed.</param>
    private static void DisposeAsset( IRefCountedContainer container )
    {
        if ( container.Asset is IDisposable disposable )
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Removes all dependencies related to the specified file asynchronously.
    /// </summary>
    /// <param name="filename">The name of the asset file whose dependencies are to be removed.</param>
    /// <returns>A task representing the asynchronous removal of dependencies.</returns>
    private void RemoveDependencies( string filename )
    {
        if ( !_assetDependencies.TryGetValue( filename, out var dependencies ) )
        {
            return;
        }

        foreach ( var dependency in dependencies )
        {
            if ( IsLoaded( dependency ) )
            {
                Unload( dependency );
            }
        }

        if ( _assets[ GetAssetType( filename ) ]?[ filename ].RefCount <= 0 )
        {
            _assetDependencies.Remove( filename );
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Disposes all assets in the manager and stops all asynchronous loading.
    /// </summary>
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Releases all resources used by the AssetManager.
    /// </summary>
    /// <param name="disposing">
    /// Indicates whether the method call comes from a Dispose method (true) or from
    /// a finalizer (false).
    /// </param>
    protected void Dispose( bool disposing )
    {
        if ( !_disposed )
        {
            if ( disposing )
            {
                ClearAsync();

                _executor.Dispose();
            }

            _disposed = true;
        }
    }
}

// ============================================================================
// ============================================================================