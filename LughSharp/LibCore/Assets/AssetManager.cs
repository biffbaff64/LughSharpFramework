﻿// ///////////////////////////////////////////////////////////////////////////////
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

using LughSharp.LibCore.Scenes.Scene2D.UI;

namespace LughSharp.LibCore.Assets;

/// <summary>
/// Loads and stores assets like textures, bitmapfonts, tile maps, sounds, music and so
/// on. Assets are reference counted. If two assets A and B both depend on another asset C,
/// C won’t be disposed until A and B have been disposed.
/// </summary>
[PublicAPI]
public class AssetManager
{
    /// <summary>
    /// Returns the <see cref="IFileHandleResolver"/> which this
    /// AssetManager was loaded with.
    /// </summary>
    /// <returns>the file handle resolver which this AssetManager uses.</returns>
    public IFileHandleResolver FileHandleResolver { get; set; }

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    private readonly Dictionary< string, List< string >? >                          _assetDependencies = new();
    private readonly Dictionary< Type, Dictionary< string, IRefCountedContainer > > _assets            = new();
    private readonly Dictionary< string, Type? >                                    _assetTypes        = new();
    private readonly Dictionary< Type, Dictionary< string, AssetLoader > >          _loaders           = new();

    private readonly List< string >          _injected  = [ ];
    private readonly List< AssetDescriptor > _loadQueue = [ ];

    private readonly Queue< AssetLoadingTask > _tasks = new();

    private IAssetErrorListener? _listener;
    private int                  _toLoad;
    private int                  _loaded;
    private int                  _peakTasks;

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    /// <summary>
    /// Creates a new AssetManager with all default loaders.
    /// </summary>
    public AssetManager() : this( new InternalFileHandleResolver() )
    {
        Logger.Checkpoint();
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
        Logger.Checkpoint();

        if ( defaultLoaders )
        {
            Logger.Debug( "Setting default loaders..." );
            
            //@formatter:off
            SetLoader( typeof( BitmapFont ),       new BitmapFontLoader( resolver ) );
            SetLoader( typeof( Texture ),          new TextureLoader( resolver ) );
            SetLoader( typeof( TextureAtlas ),     new TextureAtlasLoader( resolver ) );
            SetLoader( typeof( Pixmap ),           new PixmapLoader( resolver ) );
            SetLoader( typeof( Skin ),             new SkinLoader( resolver ) );
            SetLoader( typeof( IMusic ),           new MusicLoader( resolver ) );
            SetLoader( typeof( ISound ),           new SoundLoader( resolver ) );
            SetLoader( typeof( Cubemap ),          new CubemapLoader( resolver ) );
            SetLoader( typeof( ParticleEffect ),   new ParticleEffectLoader( resolver ) );
            SetLoader( typeof( ShaderProgram ),    new ShaderProgramLoader( resolver ) );
            SetLoader( typeof( PolygonRegion ),    new PolygonRegionLoader( resolver ) );
            //@formatter:on
        }

        FileHandleResolver = resolver;
    }

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    /// <summary>
    /// Returns true if an asset with the specified name is loading,
    /// queued to be loaded, or has been loaded.
    /// </summary>
    public bool Contains( string? fileName )
    {
        lock ( this )
        {
            if ( fileName == null ) return false;

            if ( ( _tasks.Count > 0 )
                 && _tasks.First().AssetDesc is { } assetDescriptor
                 && assetDescriptor.AssetName.Equals( fileName ) )
            {
                return true;
            }

            foreach ( var t in _loadQueue )
            {
                if ( t.AssetName.Equals( fileName ) )
                {
                    return true;
                }
            }

            return IsLoaded( fileName );
        }
    }

    /// <summary>
    /// Returns true if an asset with the specified name and type is loading,
    /// queued to be loaded, or has been loaded.
    /// </summary>
    public bool Contains( string? fileName, Type? type )
    {
        lock ( this )
        {
            if ( ( fileName == null ) || ( type == null ) ) return false;

            if ( _tasks.Count > 0 )
            {
                if ( _tasks.First().AssetDesc is { } assetDesc
                     && ( assetDesc.AssetType == type )
                     && assetDesc.AssetName.Equals( fileName ) )
                {
                    return true;
                }
            }

            foreach ( var assetDesc in _loadQueue )
            {
                if ( ( assetDesc.AssetType == type )
                     && assetDesc.AssetName.Equals( fileName ) )
                {
                    return true;
                }
            }

            return IsLoaded( fileName, type );
        }
    }

    /// <summary>
    /// Returns whether the specified asset is contained in this manager.
    /// </summary>
    public bool ContainsAsset< T >( T asset )
    {
        lock ( this )
        {
            if ( asset == null )
            {
                return false;
            }

            var assetsByType = _assets[ asset.GetType() ] ?? throw new NullReferenceException();

            foreach ( var fileName in assetsByType.Keys )
            {
                var otherAsset = ( T? )assetsByType[ fileName ].Asset;

                if ( ( otherAsset?.GetType() == asset.GetType() ) || asset.Equals( otherAsset ) )
                {
                    return true;
                }
            }

            return false;
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
        lock ( this )
        {
            ArgumentNullException.ThrowIfNull( parentAssetFilename );

            foreach ( var desc in dependendAssetDescs )
            {
                Debug.Assert( desc.AssetName != null, "desc.FilePath is null" );

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
            // Add the asset as a dependency of the parent asset
            var dependencies = _assetDependencies[ parentAssetFilename ];

            if ( dependencies == null )
            {
                dependencies = [ ];
                _assetDependencies.Put( parentAssetFilename, dependencies );
            }

            _assetDependencies[ parentAssetFilename ]?.Add( dependendAssetDesc.AssetName );

            // If the asset is already loaded, increase its reference count.
            if ( IsLoaded( dependendAssetDesc.AssetName ) )
            {
                Logger.Debug( $"Dependency already loaded: {dependendAssetDesc}" );

                var type = _assetTypes[ dependendAssetDesc.AssetName ];

                GdxRuntimeException.ThrowIfNull( type );

                _assets[ type ][ dependendAssetDesc.AssetName ].RefCount++;

                IncrementRefCountedDependencies( dependendAssetDesc.AssetName );
            }
            else
            {
                // else add a new task for the asset.
                Logger.Debug( $"Loading dependency: {dependendAssetDesc}" );

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
        if ( _assetDependencies[ parent ] == null )
        {
            return;
        }

        var stack = new Stack< string >();
        stack.Push( parent );

        while ( stack.Count > 0 )
        {
            var current = stack.Pop();

            if ( _assetDependencies.TryGetValue( current, out var assetDependency ) )
            {
                if ( assetDependency != null )
                {
                    foreach ( var dependency in assetDependency )
                    {
                        var type = _assetTypes[ dependency ];

                        if ( type == null )
                        {
                            throw new GdxRuntimeException( "type cannot be null!" );
                        }

                        _assets[ type ][ dependency ].RefCount++;

                        stack.Push( dependency );
                    }
                }
            }
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
    /// Sets the reference count of an asset.
    /// </summary>
    /// <param name="fileName"> The asset name. </param>
    /// <param name="refCount"> The new reference count. </param>
    public void SetReferenceCount( string fileName, int refCount )
    {
        lock ( this )
        {
            var type = _assetTypes[ fileName ];

            if ( type == null )
            {
                throw new GdxRuntimeException( $"Asset not loaded: {fileName}" );
            }

            _assets[ type ][ fileName ].RefCount = refCount;
        }
    }

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    #region getters

    /// <summary>
    /// Returns the reference count of an asset.
    /// </summary>
    /// <param name="fileName"> The asset name. </param>
    public int GetReferenceCount( string fileName )
    {
        lock ( this )
        {
            var type = _assetTypes[ fileName ];

            if ( type == null )
            {
                throw new GdxRuntimeException( $"Asset not loaded: {fileName}" );
            }

            if ( _assets == null )
            {
                throw new GdxRuntimeException( "_assets list is null!" );
            }

            return _assets[ type ][ fileName ].RefCount;
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

            float fractionalLoaded = _loaded;

            if ( _peakTasks > 0 )
            {
                fractionalLoaded += ( _peakTasks - _tasks.Count ) / ( float )_peakTasks;
            }

            return Math.Min( 1f, fractionalLoaded / _toLoad );
        }
    }

    /// <summary>
    /// Returns a string containing ref count and dependency
    /// information for all assets.
    /// </summary>
    public string GetDiagnostics()
    {
        lock ( this )
        {
            var sb = new StringBuilder();

            sb.Append( $"_assets.Length    : {_assets.Count}\n" );
            sb.Append( $"_assetTypes.Length: {_assetTypes.Count}\n" );
            sb.Append( $"_loaders.Length   : {_loaders.Count}\n" );
            sb.Append( $"_tasks.Length     : {_tasks.Count}\n" );
            sb.Append( $"_loadQueue.Length : {_loadQueue.Count}\n" );
            sb.Append( $"_injected.Length  : {_injected.Count}\n" );

            foreach ( var fileName in _assetTypes.Keys )
            {
                if ( sb.Length > 0 )
                {
                    sb.Append( '\n' );
                }

                sb.Append( fileName ).Append( ", " );

                var type = _assetTypes[ fileName ];

                if ( type == null )
                {
                    sb.Append( "[ NULL type! ]" );
                }
                else
                {
                    if ( _assets == null )
                    {
                        sb.Append( "NULL assets List!" );
                    }
                    else
                    {
                        var dependencies = _assetDependencies?[ fileName ];

                        if ( dependencies != null )
                        {
                            sb.Append( type.Name );
                            sb.Append( ", refs: " ).Append( _assets[ type ][ fileName ].RefCount );
                            sb.Append( ", deps: [" );

                            foreach ( var dep in dependencies )
                            {
                                sb.Append( dep );
                                sb.Append( ',' );
                            }

                            sb.Append( ']' );
                        }
                    }
                }
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// Returns a list of all asset names.
    /// </summary>
    public List< string > GetAssetNames()
    {
        lock ( this )
        {
            return _assetTypes.Keys.ToList();
        }
    }

    /// <summary>
    /// Returns a list of dependencies for the named asset.
    /// </summary>
    /// <param name="name"> Asset name. </param>
    /// <returns> Dependencies list. </returns>
    public IEnumerable< string >? GetDependencies( string name )
    {
        lock ( this )
        {
            return _assetDependencies[ name ];
        }
    }

    /// <summary>
    /// Returns the asset type for the given asset name.
    /// </summary>
    /// <param name="name"> String holding the asset name. </param>
    /// <returns> The asset type. </returns>
    public Type? GetAssetType( string name )
    {
        lock ( this )
        {
            return _assetTypes[ name ];
        }
    }

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
            return Get( name, GetAssetType( name )!, true );
        }
    }

    /// <summary>
    /// Retrieves an asset by name, throwing an exception if the asset is not found and required.
    /// </summary>
    /// <param name="name">The name of the asset to retrieve.</param>
    /// <param name="required">Determines if an exception should be thrown if the asset is not found.</param>
    /// <returns>The requested asset if found, otherwise null.</returns>
    public object? Get( string name, bool required )
    {
        lock ( this )
        {
            var type = _assetTypes.Get( name );

            if ( type != null )
            {
                var assetsByType   = _assets[ type ];
                var assetContainer = assetsByType.Get( name );

                if ( assetContainer != null )
                {
                    return assetContainer.Asset;
                }
            }

            if ( required )
            {
                throw new GdxRuntimeException( $"Asset not loaded: {name}" );
            }

            return null;
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
    public object? Get( AssetDescriptor assetDescriptor )
    {
        lock ( this )
        {
            ArgumentNullException.ThrowIfNull( assetDescriptor );

            return Get( assetDescriptor.AssetName, assetDescriptor.AssetType, true );
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
    public object? Get( string name, Type type, bool required )
    {
        lock ( this )
        {
            var assetsByType   = _assets[ type ];
            var assetContainer = assetsByType.Get( name );

            if ( assetContainer != null )
            {
                return assetContainer.Asset;
            }

            if ( required )
            {
                throw new GdxRuntimeException( $"Asset not loaded: {name}" );
            }

            return null;
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
    public List< T > GetAll< T >( Type type, List< T > outArray )
    {
        lock ( this )
        {
            ArgumentNullException.ThrowIfNull( type );
            ArgumentNullException.ThrowIfNull( outArray );

            lock ( this )
            {
                if ( !_assets.TryGetValue( type, out var assetsByType ) )
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
    /// Gets the filename for the specified asset type.
    /// Will return Null if the asset is not contained in this manager.
    /// </summary>
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
                var assetsByType = _assets[ assetType ] ?? throw new NullReferenceException();

                foreach ( var fileName in assetsByType.Keys )
                {
                    var otherAsset = ( T? )assetsByType[ fileName ].Asset;

                    if ( ( otherAsset?.GetType() == asset.GetType() ) || asset.Equals( otherAsset ) )
                    {
                        return fileName;
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Returns the loader for the given type and the specified filename.
    /// </summary>
    /// <param name="type">The type of the loader to get</param>
    /// <param name="fileName">
    /// The filename of the asset to get a loader for, or null to get the default loader.
    /// </param>
    /// <returns>
    /// The loader capable of loading the type and filename, or null if none exists.
    /// </returns>
    /// <exception cref="GdxRuntimeException"> If no loader was found. </exception>
    public AssetLoader? GetLoader( Type? type, string? fileName = null )
    {
        if ( ( type == null ) || ( _loaders[ type ].Count < 1 ) )
        {
            throw new GdxRuntimeException( $"No loader for type: {type?.Name}" );
        }

        if ( fileName == null )
        {
            return _loaders[ type ][ "" ];
        }

        AssetLoader? result = null;

        var len = -1;

        foreach ( var entry in _loaders[ type ] )
        {
            if ( ( entry.Key.Length > len ) && fileName.EndsWith( entry.Key ) )
            {
                result = entry.Value;
            }

            len = entry.Key.Length;
        }

        return result;
    }

    #endregion getters

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    #region updates

    /// <summary>
    /// Asynchronously updates the asset manager, loading new assets and processing
    /// asset loading tasks. Returns true if all assets are loaded, otherwise false.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result
    /// contains a boolean value indicating whether all assets are loaded.</returns>
    public async Task< bool > UpdateAsync()
    {
        try
        {
            if ( _tasks.Count == 0 )
            {
                // Load next task if there are no active tasks running.
                while ( _loadQueue.Count != 0 && _tasks.Count == 0 )
                {
                    if ( LoadNextTask() )
                    {
                        Logger.Debug( $"Next task is loaded: {_tasks.Peek()}" );
                    }
                }

                // If we still have no tasks, we're done
                if ( _tasks.Count == 0 ) return true;
            }

            await ProcessNextTaskAsync();

            return IsFinished();
        }
        catch ( System.Exception t )
        {
            Logger.Error( $"Error loading asset: {t}" );

            if ( _tasks.Count == 0 )
            {
                throw new GdxRuntimeException( t );
            }

            var task = _tasks.Dequeue();
            task.Cancel = true;

            var assetDesc = task.AssetDesc;

            // Clear tasks and inform listener
            _tasks.Clear();

            if ( _listener != null )
            {
                _listener.Error( assetDesc, t );
            }
            else
            {
                throw new GdxRuntimeException( t );
            }

            return _loadQueue.Count == 0;
        }
    }

    /// <summary>
    /// Asynchronously updates the AssetManager by processing queued asset loading
    /// tasks for a specified amount of time.
    /// </summary>
    /// <param name="millis">The number of milliseconds to perform updates.</param>
    /// <returns>A task that represents whether the asset manager finished processing all tasks.</returns>
    /// <exception cref="GdxRuntimeException">Thrown when an error occurs during asset loading.</exception>
    public async Task< bool > UpdateAsync( int millis )
    {
        try
        {
            var endTime = TimeUtils.NanoTime() + TimeUtils.MillisToNanos( millis );

            while ( TimeUtils.NanoTime() < endTime )
            {
                if ( _tasks.Count == 0 )
                {
                    // Load next task if there are no active tasks
                    while ( _loadQueue.Count != 0 && _tasks.Count == 0 )
                    {
                        LoadNextTask();
                    }

                    // If we still have no tasks, we're done
                    if ( _tasks.Count == 0 )
                    {
                        return _loadQueue.Count == 0;
                    }
                }

                // Process the next task
                await ProcessNextTaskAsync();

                // If we're still in time, go for another round
                if ( TimeUtils.NanoTime() < endTime )
                {
                    continue;
                }

                // We ran out of time
                break;
            }

            return IsFinished();
        }
        catch ( System.Exception t )
        {
            Logger.Error( $"Error loading asset: {t}" );

            if ( _tasks.Count == 0 ) throw new GdxRuntimeException( t );

            var task = _tasks.Dequeue();
            task.Cancel = true;

            var assetDesc = task.AssetDesc;

            // Clear tasks and inform listener
            _tasks.Clear();

            if ( _listener != null )
            {
                _listener.Error( assetDesc, t );
            }
            else
            {
                throw new GdxRuntimeException( t );
            }

            return _loadQueue.Count == 0;
        }
    }

    #endregion updates

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    #region task management

    /// <summary>
    /// Adds a new asset loading task to the queue using the given asset descriptor.
    /// </summary>
    /// <param name="assetDesc">Descriptor containing information about the asset to be loaded.</param>
    /// <exception cref="GdxRuntimeException">Thrown if no loader is available for the asset type specified in the asset descriptor.</exception>
    private void AddTask( AssetDescriptor assetDesc )
    {
        Logger.Checkpoint();

        var loader = GetLoader( assetDesc.AssetType, assetDesc.AssetName );

        if ( loader == null )
        {
            throw new GdxRuntimeException( $"No loader for type: {assetDesc.AssetType}" );
        }

        Logger.Debug( $"loader: {loader}" );

        _tasks.Enqueue( new AssetLoadingTask( this, assetDesc, loader ) );

        _peakTasks++;
    }

    /// <summary>
    /// Processes the next asset loading task in the queue asynchronously.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation of processing the next asset loading task.
    /// </returns>
    private async Task ProcessNextTaskAsync()
    {
        if ( _tasks.TryPeek( out var task ) )
        {
            Logger.Checkpoint();

            try
            {
                var asset = await task.LoadAsync();

                Logger.Debug( $"task cancelled?: {task.Cancel}" );

                if ( !task.Cancel )
                {
                    AddAsset( task.AssetDesc.AssetName, task.AssetDesc.AssetType, asset );

                    task.AssetDesc.Parameters?
                        .LoadedCallback?
                        .FinishedLoading( this, task.AssetDesc.AssetName, task.AssetDesc.AssetType );
                }

                _loaded++;
            }
            catch ( System.Exception )
            {
                Logger.Error( $"Error loading asset: {task.AssetDesc.AssetName}" );
                task.Cancel = true;
            }
            finally
            {
                _tasks.Dequeue();
            }
        }
        else
        {
            Logger.Debug( $"No task to load: {task?.AssetDesc.AssetName}" );
        }
    }

    /// <summary>
    /// Loads the next asset task from the load queue.
    /// If the asset is already loaded, increment the loaded count for
    /// use in <see cref="GetProgress"/>.
    /// </summary>
    private bool LoadNextTask()
    {
        Logger.Checkpoint( true, true );

        Logger.Debug( $"_loaded size: {_loaded}" );
        Logger.Debug( $"_toLoad size: {_toLoad}" );
        Logger.Debug( $"LoadQueue size: {_loadQueue.Count}" );

        var assetDesc = _loadQueue.RemoveIndex( 0 );

        var isLoaded = false;

        if ( IsLoaded( assetDesc.AssetName ) )
        {
            Logger.Debug( $"Already loaded: {assetDesc.AssetName}" );

            _loaded++;
            _toLoad--;
        }
        else
        {
            Logger.Debug( $"Loading: {assetDesc.AssetName}" );

            AddTask( assetDesc );

            isLoaded = true;
        }

        Logger.Checkpoint( true, true );

        return isLoaded;
    }

    /// <summary>
    /// Called when a task throws an exception during loading. The default implementation
    /// rethrows the exception and does not use the <tt>assetDesc</tt> parameter.
    /// A subclass may supress the default implementation when loading assets where loading
    /// failure is recoverable.
    /// </summary>
    public virtual void TaskFailed( AssetDescriptor assetDesc, System.Exception ex )
    {
        throw ex;
    }

    /// <summary>
    /// Handles a runtime/loading error in <see cref="UpdateAsync()"/> by optionally
    /// invoking the <see cref="IAssetErrorListener"/>.
    /// </summary>
    /// <param name="t"></param>
    public async void HandleTaskErrorAsync( System.Exception t )
    {
        Logger.Error( $"Error loading asset: {t}" );

        if ( _tasks.Count == 0 )
        {
            throw new GdxRuntimeException( t );
        }

        // pop the faulty task from the stack
        var task      = _tasks.Dequeue();
        var assetDesc = task.AssetDesc;

        // remove all dependencies if dependences are loaded and
        // those dependencies actually exist...
        if ( task is { DependenciesLoaded: true, Dependencies: not null } )
        {
            foreach ( var desc in task.Dependencies )
            {
                await UnloadAsync( desc.AssetName );
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

        await Task.CompletedTask; // Placeholder for potential async operations
    }

    #endregion task management

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    #region asset loading

    /// <summary>
    /// Returns TRUE if the asset identified by fileName is loaded.
    /// </summary>
    public bool IsLoaded( string? fileName )
    {
        lock ( this )
        {
            var isLoaded = ( fileName != null ) && _assetTypes.ContainsKey( fileName );

            Logger.Debug( $"Asset: {fileName} is already loaded: {isLoaded}" );

            return isLoaded;
        }
    }

    /// <summary>
    /// Returns TRUE if the asset identified by fileName and Type is loaded.
    /// </summary>
    public bool IsLoaded( string fileName, Type type )
    {
        lock ( this )
        {
            ArgumentNullException.ThrowIfNull( type );
            GdxRuntimeException.ThrowIfNull( _assets );

            // Retrieve all assets of the required type
            var assetsByType = _assets.Get( type );
            var isLoaded = ( fileName.Length != 0 ) && ( assetsByType != null )
                                                    && assetsByType.ContainsKey( fileName );

            Logger.Debug( $"Asset: {fileName} is already loaded: {isLoaded}" );

            return isLoaded;
        }
    }

    /// <summary>
    /// Adds the given asset to the loading queue of the AssetManager.
    /// </summary>
    /// <param name="fileName">
    /// The file name (interpretation depends on <see cref="AssetLoader"/>)
    /// </param>
    /// <param name="type"> the type of the asset. </param>
    public void Load( string fileName, Type type )
    {
        lock ( this )
        {
            Logger.Divider();
            Logger.Debug( $"fileName: {fileName}, type: {type}" );

            Load( fileName, type, null );
        }
    }

    /// <summary>
    /// Adds the given asset to the loading queue of the AssetManager.
    /// </summary>
    /// <param name="fileName">
    /// the file name (interpretation depends on <see cref="AssetLoader"/>)
    /// </param>
    /// <param name="type">the type of the asset.</param>
    /// <param name="parameter"></param>
    public void Load( string? fileName, Type? type, AssetLoaderParameters? parameter )
    {
        lock ( this )
        {
            ArgumentNullException.ThrowIfNull( fileName, "Filename not specified!" );

            var loader = GetLoader( type, fileName );

            Logger.Debug( $"loader: {loader}" );

            if ( _loadQueue.Count == 0 )
            {
                // Nothing in the load queue, reset stats
                _loaded    = 0;
                _toLoad    = 0;
                _peakTasks = 0;
            }

            ValidatePreloadQueue( fileName, type );

            // check task list
            for ( var i = 0; i < _tasks.Count; i++ )
            {
                var desc = _tasks.ElementAt( i ).AssetDesc;

                if ( ( desc.AssetName == fileName ) && ( desc.AssetType != type ) )
                {
                    throw new GdxRuntimeException
                        ( $"Asset with name '{fileName}' already in preload queue, but has different " +
                          $"type (expected: {type?.Name}, found: {desc.AssetType.Name})" );
                }
            }

            // Try to find an asset in the dictionary that has the same name as this
            // asset, but which has a different asset type.
            // This situation should not occur.
            _assetTypes.TryGetValue( fileName, out var otherType );

            if ( ( otherType != null ) && ( otherType != type ) )
            {
                throw new GdxRuntimeException
                    ( $"Asset with name '{fileName}' already loaded, but has different " +
                      $"type (expected: {type?.Name}, found: {otherType.Name})" );
            }

            _toLoad++;

            Logger.Debug( $"Adding asset to Load Queue: Name:{fileName} Type: {type?.Name}" );
            
            _loadQueue.Add( new AssetDescriptor
            {
                AssetName  = fileName,
                AssetType  = type!,
                Parameters = parameter,
            } );
        }
    }

    /// <summary>
    /// Adds the given asset to the loading queue of the AssetManager.
    /// </summary>
    /// <param name="desc">the <see cref="AssetDescriptor"/></param>
    public void Load( AssetDescriptor desc )
    {
        lock ( this )
        {
            ArgumentNullException.ThrowIfNull( desc );

            Load( desc.AssetName, desc.AssetType, desc.Parameters );
        }
    }

    /// <summary>
    /// Adds an asset to the asset manager with the specified file name and type.
    /// </summary>
    /// <param name="fileName">The file name associated with the asset.</param>
    /// <param name="type">The type of the asset.</param>
    /// <param name="asset">The asset to add.</param>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is null.</exception>
    public void AddAsset( string fileName, Type type, object? asset )
    {
        Logger.Checkpoint();

        ArgumentNullException.ThrowIfNull( asset );

        if ( _assets == null )
        {
            throw new GdxRuntimeException( "_assets list is null" );
        }

        // Add the asset to the filename lookup
        _assetTypes[ fileName ]     = type;
        _assets[ type ][ fileName ] = new RefCountedContainer( asset );

        Logger.Debug( $"Asset added: {fileName}" );
    }

    /// <summary>
    /// Returns true when all assets are loaded. Can be called from any thread but
    /// note <see cref="UpdateAsync()"/> or related methods must be called to process tasks.
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
    /// <returns></returns>
    public async Task FinishLoadingAsync()
    {
        Logger.Debug( "Waiting for loading to complete..." );

        while ( !await UpdateAsync() )
        {
            await Task.Yield();
        }

        Logger.Debug( "Loading complete." );
    }

    /// <summary>
    /// Blocks until the specified asset is loaded.
    /// </summary>
    /// <param name="assetDesc">the AssetDescriptor of the asset</param>
    public object FinishLoadingAsset( AssetDescriptor assetDesc )
    {
        return FinishLoadingAssetAsync( assetDesc.AssetName );
    }

    /// <summary>
    /// Asynchronously waits for the specified asset to be loaded and returns it upon completion.
    /// </summary>
    /// <param name="fileName">The name of the file of the asset to finish loading.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the loaded asset.</returns>
    public async Task< object > FinishLoadingAssetAsync( string fileName )
    {
        ArgumentNullException.ThrowIfNull( fileName );

        Logger.Debug( $"Waiting for asset to be loaded: {fileName}" );

        while ( true )
        {
            var type = _assetTypes.Get( fileName );

            if ( type != null )
            {
                var assetsByType   = _assets?.Get( type );
                var assetContainer = assetsByType?.Get( fileName );

                var asset = assetContainer?.Asset;

                Logger.Debug( $"assetsByType: {assetsByType?.Count}" );
                Logger.Debug( $"asset       : {assetContainer?.Asset}" );
                Logger.Debug( $"refCount    : {assetContainer?.RefCount}" );

                if ( asset != null )
                {
                    Logger.Debug( $"Asset loaded: {fileName}" );
                    return asset;
                }
            }

            await UpdateAsync();
        }
    }

    /// <summary>
    /// Sets a new <see cref="AssetLoader"/> for the given type.
    /// </summary>
    /// <param name="type"> the type of the asset </param>
    /// <param name="loader"> the loader</param>
    /// <param name="suffix">
    /// the suffix the filename must have for this loader to be used or null
    /// to specify the default loader.
    /// </param>
    public void SetLoader( Type type, AssetLoader loader, string suffix = "" )
    {
        lock ( this )
        {
            ArgumentNullException.ThrowIfNull( type );
            ArgumentNullException.ThrowIfNull( loader );

            if ( !_loaders.TryGetValue( type, out var value ) )
            {
                var typeLoaders = new Dictionary< string, AssetLoader >();

                value            = typeLoaders;
                _loaders[ type ] = value;
            }

            value.Put( suffix, loader );
        }
    }

    /// <summary>
    /// Check if an asset with the same name but different type has already been added.
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="type"></param>
    private void ValidatePreloadQueue( string fileName, Type? type )
    {
        lock ( this )
        {
            if ( !File.Exists( fileName ) )
            {
                throw new GdxRuntimeException( $"File '{fileName}' not found!" );
            }
            
            foreach ( var desc in _loadQueue )
            {
                if ( ( desc.AssetName == fileName ) && ( desc.AssetType != type ) )
                {
                    throw new GdxRuntimeException
                        ( $"Asset with name '{fileName}' already in preload queue, but has different " +
                          $"type (expected: {type?.Name}, found: {desc.AssetType.Name})" );
                }
            }

            Logger.Debug( $"Asset {fileName}: Validated successfully." );
        }
    }

    #endregion asset loading

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    #region asset unloading

    /// <summary>
    /// Asynchronously unloads an asset from the AssetManager.
    /// </summary>
    /// <param name="fileName">The name of the asset to be unloaded.</param>
    /// <returns>A task representing the asynchronous unload operation.</returns>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is not loaded or cannot be found.</exception>
    public async Task UnloadAsync( string fileName )
    {
        // Convert all Windows path separators to Unix style
        fileName = fileName.Replace( '\\', '/' );

        // Check if it's currently processed (and the first element in the queue,
        // thus not a dependency) and cancel if necessary
        if ( TryCancelCurrentTask( fileName ) )
        {
            return;
        }

        // Get the type of the asset
        if ( !_assetTypes.TryGetValue( fileName, out var type ) )
        {
            throw new GdxRuntimeException( $"Asset not loaded: {fileName}" );
        }

        // Check if the asset is in the load queue
        if ( TryRemoveFromQueue( fileName, type! ) )
        {
            return;
        }

        // Handle unloading the asset and its dependencies
        await UnloadAssetAsync( fileName, type );
    }

    /// <summary>
    /// Attempts to cancel the currently processing task if it matches the given file name.
    /// </summary>
    /// <param name="fileName">The name of the file to check for cancellation.</param>
    /// <returns>True if the current task was successfully canceled; otherwise, false.</returns>
    private bool TryCancelCurrentTask( string fileName )
    {
        if ( _tasks.Count > 0 && _tasks.Peek().AssetDesc.AssetName == fileName )
        {
            Logger.Debug( $"Unload (from tasks): {fileName}" );

            var task = _tasks.Dequeue();
            task.Cancel = true;
            task.Unload();

            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to remove an asset from the load queue based on its file name and type.
    /// </summary>
    /// <param name="fileName">The name of the file corresponding to the asset to be removed.</param>
    /// <param name="type">The type of the asset to be removed.</param>
    /// <returns>
    /// true if the asset was successfully found and removed from the load queue; otherwise, false.
    /// </returns>
    private bool TryRemoveFromQueue( string fileName, Type type )
    {
        for ( var i = 0; i < _loadQueue.Count; i++ )
        {
            if ( _loadQueue[ i ].AssetName.Equals( fileName ) )
            {
                Logger.Debug( $"Unload (from queue): {fileName}" );
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
    /// <param name="fileName">The name of the asset to unload.</param>
    /// <param name="type">The type of the asset to unload.</param>
    /// <returns>A task representing the asynchronous unload operation.</returns>
    private async Task UnloadAssetAsync( string fileName, Type? type )
    {
        if ( type == null )
        {
            throw new GdxRuntimeException( $"Asset not loaded: {fileName}: Type not specified." );
        }

        GdxRuntimeException.ThrowIfNull( _assets );

        if ( !_assets.TryGetValue( type, out var assetRef )
             || !assetRef.TryGetValue( fileName, out var container ) )
        {
            throw new GdxRuntimeException( $"Asset not loaded: {fileName}" );
        }

        container.RefCount--;

        if ( container.RefCount <= 0 )
        {
            Logger.Debug( $"Unload (dispose): {fileName}" );

            await DisposeAssetAsync( container );

            _assetTypes.Remove( fileName );
            assetRef.Remove( fileName );

            // Remove dependencies if the asset is completely unloaded
            await RemoveDependenciesAsync( fileName );
        }
        else
        {
            Logger.Debug( $"Unload (decrement): {fileName}" );
        }
    }

    /// <summary>
    /// Asynchronously disposes of the assets contained within the provided IRefCountedContainer.
    /// </summary>
    /// <param name="container">The container that holds the asset to be disposed.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    private static async Task DisposeAssetAsync( IRefCountedContainer container )
    {
        switch ( container.Asset )
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync();
                break;

            case IDisposable disposable:
                disposable.Dispose();
                break;
        }
    }

    /// <summary>
    /// Removes all dependencies related to the specified file asynchronously.
    /// </summary>
    /// <param name="fileName">The name of the asset file whose dependencies are to be removed.</param>
    /// <returns>A task representing the asynchronous removal of dependencies.</returns>
    private async Task RemoveDependenciesAsync( string fileName )
    {
        if ( !_assetDependencies.TryGetValue( fileName, out var dependencies )
             || ( dependencies == null ) )
        {
            return;
        }

        foreach ( var dependency in dependencies )
        {
            if ( IsLoaded( dependency ) )
            {
                await UnloadAsync( dependency );
            }
        }

        if ( _assets[ _assetTypes[ fileName ]! ][ fileName ].RefCount <= 0 )
        {
            _assetDependencies.Remove( fileName );
        }
    }

    /// <summary>
    /// Disposes of the specified asset container if it implements <see cref="IDisposable"/>.
    /// </summary>
    /// <param name="container">The asset container to dispose.</param>
    private static void DisposeAsset( IRefCountedContainer container )
    {
        if ( container.Asset is IDisposable disposable )
        {
            disposable.Dispose();
        }
    }

    #endregion asset unloading

    // ------------------------------------------------------------------------

    /// <summary>
    /// Clears and disposes all assets and the preloading queue.
    /// </summary>
    public async Task ClearAsync()
    {
        lock ( this )
        {
            _loadQueue.Clear();
        }

        await FinishLoadingAsync();

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
                var dependencies = _assetDependencies?[ asset ];

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
                    await UnloadAsync( asset );
                }
            }
        }

        _loaded    = 0;
        _toLoad    = 0;
        _peakTasks = 0;

        _assets.Clear();
        _assetTypes.Clear();
        _assetDependencies?.Clear();
        _loadQueue.Clear();
        _tasks.Clear();
    }

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    #region dispose pattern

    /// <summary>
    /// Disposes all assets in the manager and stops all asynchronous loading.
    /// </summary>
    public void Dispose()
    {
        Dispose( true );
    }

    /// <summary>
    /// Releases all resources used by the AssetManager.
    /// </summary>
    /// <param name="disposing">Indicates whether the method call comes from a Dispose
    /// method (true) or from a finalizer (false).</param>
    protected async void Dispose( bool disposing )
    {
        if ( disposing )
        {
            await ClearAsync();
        }
    }

    #endregion dispose pattern

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    /// <summary>
    /// Output Assetmanager metrics via the <see cref="Logger"/> class.
    /// </summary>
    public void DisplayMetrics()
    {
        Logger.Divider();
        ReportAssetTypes();
        Logger.Debug( $"_assets Count     : {_assets.Count}" );
        Logger.Debug( $"_loaded           : {_loaded}" );
        Logger.Debug( $"_toLoad           : {_toLoad}" );
        Logger.Debug( $"_loadQueue Count  : {_loadQueue.Count}" );
        Logger.Debug( $"_tasks Count      : {_tasks.Count}" );
        Logger.Divider();
    }

    /// <summary>
    /// Logs the count and names of all asset types currently managed by the AssetManager.
    /// </summary>
    private void ReportAssetTypes()
    {
        Logger.Debug( $"_assetTypes[].Length: {_assetTypes.Count}" );

        var k = 1;

        if ( _assetTypes.Count > 0 )
        {
            foreach ( var key in _assetTypes.Keys )
            {
                Logger.Debug( $"Key #{k++}: {key}" );
            }
        }
    }
}