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

using LughSharp.Lugh.Assets.Loaders;
using LughSharp.Lugh.Assets.Loaders.Resolvers;
using LughSharp.Lugh.Audio;
using LughSharp.Lugh.Files;
using LughSharp.Lugh.Scenes.Scene2D.UI;

using LughUtils.source.Collections;

namespace LughSharp.Lugh.Assets;

/// <summary>
/// Loads and stores assets like textures, bitmapfonts, tile maps, sounds, music and so
/// on. Assets are reference counted. If two assets A and B both depend on another asset C,
/// C won’t be disposed until A and B have been disposed.
/// </summary>
[PublicAPI]
public partial class AssetManager
{
    //TODO: implement this
    [PublicAPI]
    public class AssetData
    {
        public string?      Name     { get; set; } // Short name (no path)
        public string?      FullName { get; set; } // Full name (with path)
        public Type?        Type     { get; set; } // Asset type (e.g., Texture, Sound, etc.)
        public AssetLoader? Loader   { get; set; } // Loader used to load the asset
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
    /// A ObjectMap holding the loaders for each asset type.
    /// The loaders are stored in a ObjectMap with the assetloader type as key
    /// and the a second ObjectMap with the asset suffix as key and the asset loader
    /// as value.
    /// </summary>
    private readonly ObjectMap< Type, ObjectMap< string, AssetLoader > >? _loaders = [ ];

    /// <summary>
    /// A ObjectMap holding the assets for each asset type. The assets are stored in a
    /// ObjectMap with the asset name as key and the asset container as value. The asset
    /// container is a ObjectMap holding the asset name as key and a reference counter
    /// as the value. 
    /// </summary>
    private readonly ObjectMap< Type, ObjectMap< string, IRefCountedContainer >? > _assets = [ ];

    /// <summary>
    /// A ObjectMap holding the dependencies for each asset. The dependencies are stored in a
    /// ObjectMap with the asset name as key and a list of dependencies as value.
    /// </summary>
    private readonly ObjectMap< string, List< string > > _assetDependencies = [ ];

//NOTE: I have removed this because I feel it is unnecessary duplication of data, and
//      it is also defined the wrong way around. I think the key should be the type,
//      and the value should be the asset name. The data in this ObjectMap can be
//      reconstructed from the _assets ObjectMap if needed.

//    /// <summary>
//    /// Asset filename lookup table. This ObjectMap holds the asset filename as the
//    /// key, and the asset type as the value.
//    /// </summary>
    private readonly ObjectMap< string, Type > _assetTypes = [ ];

    private readonly List< string >            _injected    = [ ];
    private readonly List< AssetDescriptor >   _loadQueue   = [ ];
    private readonly Queue< AssetLoadingTask > _tasks       = [ ];
    private readonly ReaderWriterLockSlim      _loadersLock = new();

    private IAssetErrorListener? _listener;
    private AsyncExecutor        _executor;

    private int _loaded;
    private int _peakTasks;
    private int _toLoad;

    // ========================================================================

    public void TestSetup()
    {
        var value1 = new ObjectMap< string, IRefCountedContainer >();

//        var value2 = new ObjectMap< string, IRefCountedContainer >();
//        var value3 = new ObjectMap< string, IRefCountedContainer >();
//        var value4 = new ObjectMap< string, IRefCountedContainer >();

        value1.AddPair( "test1", new RefCountedContainer( new object() ) );
        value1.AddPair( "test2", new RefCountedContainer( new object() ) );
        value1.AddPair( "test3", new RefCountedContainer( new object() ) );
        value1.AddPair( "test4", new RefCountedContainer( new object() ) );

        _assets.AddPair( typeof( Texture ), value1 );

//        _assets.AddPair( typeof( Texture ), value2 );
//        _assets.AddPair( typeof( Texture ), value3 );
//        _assets.AddPair( typeof( Texture ), value4 );
    }

    public void TestRun()
    {
        var numAssetTypes = 0;
        var numAssets     = 0;
        var index         = 0;

        foreach ( var assetType in _assets.Keys )
        {
            if ( assetType == null )
            {
                Logger.Debug( "Asset Type is null" );

                continue;
            }

            numAssetTypes++;

            foreach ( var assetContainer in _assets![ assetType ]! )
            {
                Logger.Debug( $"[Index: {index:000}]" +
                              $"Asset Type: {assetType}\n" +
                              $"Index of Asset Type: {_assets?.LocateKey( assetType )}\n" +
                              $"Asset Name: {assetContainer.Key}\n" +
                              $"Asset RefCount: {assetContainer.Value.RefCount}\n" +
                              $"Asset: {assetContainer.Value.Asset}" );
                index++;
                numAssets++;
            }
        }

        Logger.Debug( $"No. of Asset TYPES: {numAssetTypes}" );
        Logger.Debug( $"No. of Assets     : {numAssets}" );

        Logger.Checkpoint();

        var names = GetAssetNames();

        Logger.Checkpoint();

        if ( names.Count == 0 )
        {
            Logger.Debug( "No Names retrieved!" );

            return;
        }

        Logger.Checkpoint();

        foreach ( var name in names )
        {
            Logger.Debug( $"Asset Name: {name}" );
        }
    }

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
            Logger.Debug( "Setting Default Asset Loaders..." );

            SetLoader( typeof( BitmapFont ), new BitmapFontLoader( resolver ) );
            SetLoader( typeof( IMusic ), new MusicLoader( resolver ) );
            SetLoader( typeof( Pixmap ), new PixmapLoader( resolver ) );
            SetLoader( typeof( ISound ), new SoundLoader( resolver ) );
            SetLoader( typeof( TextureAtlas ), new TextureAtlasLoader( resolver ) );
            SetLoader( typeof( Texture ), new TextureLoader( resolver ) );
            SetLoader( typeof( Skin ), new SkinLoader( resolver ) );
            SetLoader( typeof( ParticleEffect ), new ParticleEffectLoader( resolver ) );
            SetLoader( typeof( PolygonRegion ), new PolygonRegionLoader( resolver ) );
            SetLoader( typeof( ShaderProgram ), new ShaderProgramLoader( resolver ) );
            SetLoader( typeof( Cubemap ), new CubemapLoader( resolver ) );

            //TODO:
            // 3D Particle effect loader here...
            // .g3dj loader here...
            // .g3db loader here...
            // .obj loader here...
        }

        _executor          = new AsyncExecutor( 1, "AssetManager" );
        FileHandleResolver = resolver;
    }

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

                if ( done || TimeUtils.Millis() >= endTime )
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

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Returns true if an asset with the specified name is loading,
    /// queued to be loaded, or has been loaded.
    /// </summary>
    public bool Contains( string? fileName )
    {
        lock ( this )
        {
            if ( fileName == null )
            {
                return false;
            }

            fileName = IOUtils.NormalizePath( fileName );

            if ( ( _tasks.Count > 0 )
                 && _tasks.First().AssetDesc is { } assetDescriptor
                 && assetDescriptor.AssetName.Equals( fileName ) )
            {
                return true;
            }

            foreach ( var lq in _loadQueue )
            {
                if ( lq.AssetName.Equals( fileName ) )
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
        if ( ( fileName == null ) || ( type == null ) )
        {
            return false;
        }

        lock ( this )
        {
            fileName = IOUtils.NormalizePath( fileName );

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

            var assetsByType = _assets[ asset.GetType() ];

            if ( assetsByType == null )
            {
                return false;
            }

            foreach ( var assetRef in assetsByType.Values )
            {
                if ( assetRef?.Asset != null )
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
    /// Returns a list of all asset names.
    /// </summary>
    public List< string? > GetAssetNames()
    {
        lock ( this )
        {
//            return _assets.Values.SelectMany( innerDict => innerDict!.Keys ).ToList();

            // 1. Use a HashSet to efficiently store unique keys.
            // This prevents duplicate strings from being added.
            var uniqueKeys = new HashSet< string? >();

            // 2. Iterate through the OUTER Dictionary's VALUES (the inner dictionaries).
            foreach ( var innerDictionary in _assets.Values )
            {
                if ( innerDictionary == null )
                {
                    continue;
                }
                
                // 3. Iterate through the KEYS of the inner Dictionary.
                foreach ( var key in innerDictionary.Keys )
                {
                    if ( key == null )
                    {
                        continue;
                    }
                    
                    // 4. Add the key string to the HashSet.
                    // HashSet's Add method automatically handles uniqueness.
                    uniqueKeys.Add( key );
                }
            }

            return uniqueKeys.ToList();
        }
    }

    /// <summary>
    /// Returns a list of all asset names.
    /// </summary>
    public List< string? > GetAssetNamesDistinct()
    {
        lock ( this )
        {
            return _assets.Values
                          .SelectMany( innerDict => innerDict!.Keys )
                          .Distinct()
                          .ToList();
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
            name = IOUtils.NormalizePath( name );

            return _assetDependencies[ name ];
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
    /// Retrieves an asset of the specified type by its name.
    /// </summary>
    /// <param name="name">The name of the asset to retrieve.</param>
    /// <returns>The asset of the specified type.</returns>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is not loaded.</exception>
    public object? Get( string name )
    {
        lock ( this )
        {
            return Get( name, GetAssetType( name ), true );
        }
    }

    /// <summary>
    /// Retrieves an asset of the specified type by its name.
    /// </summary>
    /// <param name="name">The name of the asset to retrieve.</param>
    /// <returns>The asset of the specified type.</returns>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is not loaded.</exception>
    public T? Get< T >( string name )
    {
        lock ( this )
        {
            return ( T? )Get( name, typeof( T ), true );
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
    public object? Get( string name, bool required )
    {
        lock ( this )
        {
            name = IOUtils.NormalizePath( name );

            var type           = GetAssetType( name );
            var assetsByType   = _assets[ type ];
            var assetContainer = assetsByType?.Get( name );

            if ( assetContainer != null )
            {
                return assetContainer.Asset;
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
    public object? Get( AssetDescriptor assetDescriptor )
    {
        ArgumentNullException.ThrowIfNull( assetDescriptor );

        lock ( this )
        {
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
    public object? Get( string name, Type? type, bool required )
    {
        Logger.Checkpoint();

        Guard.ThrowIfNull( type );

        lock ( this )
        {
            name = IOUtils.NormalizePath( name );

            Logger.Debug( $"Getting asset: {name}, {type}" );

            try
            {
                var assetsByType   = _assets[ type ];
                var assetContainer = assetsByType?.Get( name );

                Logger.Debug( $"AssetContainer.Asset: {assetContainer?.Asset}" );

                return assetContainer?.Asset;
            }
            catch ( Exception )
            {
                throw new GdxRuntimeException( $"Asset not loaded: {name}" );
            }
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
        ArgumentNullException.ThrowIfNull( type );
        ArgumentNullException.ThrowIfNull( outArray );

        lock ( this )
        {
            lock ( this )
            {
                if ( !_assets.TryGetValue( type, out var assetsByType ) || assetsByType == null )
                {
                    throw new GdxRuntimeException( $"No assets loaded for type {type.FullName}" );
                }

                foreach ( var assetContainer in assetsByType.Values )
                {
                    if ( assetContainer?.Asset is T asset )
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

    // ========================================================================
    // ========================================================================

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
    /// Handles a runtime/loading error in <see cref="Update()"/> by optionally
    /// invoking the <see cref="IAssetErrorListener"/>.
    /// </summary>
    /// <param name="t"></param>
    public void HandleTaskError( Exception t )
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

    // ========================================================================
    // ========================================================================

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

        var dependencyCount = new ObjectMap< string, int >();

        while ( _assetTypes.Size > 0 )
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
                    Unload( asset );
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

    // ========================================================================

    public void DebugPrint()
    {
        lock ( this )
        {
            Guard.ThrowIfNull( _assets );
            Guard.ThrowIfNull( _loaders );
            Guard.ThrowIfNull( _assetTypes );

            Logger.Debug( $"Number of Assets: {_assets.Size}" );
            Logger.Debug( $"Number of Types: {_assetTypes.Size}" );

            Logger.Debug( $"\n--- Asset Loader Debug Dump ({_loaders.Size} Asset Types Registered) ---" );

            // Iterate over the outer ObjectMap (Key: Asset Type)
            foreach ( var outerEntry in _loaders )
            {
                var assetType    = outerEntry.Key;
                var innerLoaders = outerEntry.Value;

                Logger.Debug( $"\n[ASSET TYPE]: {assetType.Name} (Total Loaders: {innerLoaders.Size})" );
                Logger.Debug( "--------------------------------------------------" );

                // Iterate over the inner ObjectMap (Key: Asset Name, Value: Concrete Loader)
                foreach ( var innerEntry in innerLoaders )
                {
                    var assetName = innerEntry.Key;
                    var loader    = innerEntry.Value;

                    if ( assetName == string.Empty )
                    {
                        assetName = "(default)";
                    }

                    // Get the actual derived class name (e.g., "TextureLoader" or "SoundLoader")
                    var concreteType = loader.GetType();

                    Logger.Debug( $"  - Suffix        : '{assetName}'" );
                    Logger.Debug( $"    [Loader Class]: {concreteType.Name}" );
                }
            }

            Logger.Debug( "\n--- End of Debug Dump ---" );
        }
    }

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
    /// Indicates whether the method call comes from a Dispose
    /// method (true) or from a finalizer (false).
    /// </param>
    protected void Dispose( bool disposing )
    {
        if ( disposing )
        {
            ClearAsync();

            _executor.Dispose();
        }
    }

    // ========================================================================
    // ========================================================================

    #region dependency injection and ref counting

    /// <summary>
    /// Injects dependencies for a given parent asset by adding the specified
    /// dependent asset descriptors.
    /// </summary>
    /// <param name="parentAssetFilename">The file name of the parent asset.</param>
    /// <param name="dependendAssetDescs">The list of dependent asset descriptors to inject.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parentAssetFilename"/> is null.</exception>
    public void InjectDependencies( string parentAssetFilename, List< AssetDescriptor > dependendAssetDescs )
    {
        ArgumentNullException.ThrowIfNull( parentAssetFilename );

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
            _assetDependencies[ parentAssetFilename ].Add( dependendAssetDesc.AssetName );

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

            _assets[ type ]?[ fileName ].RefCount = refCount;
        }
    }

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

    #endregion dependency injection and ref counting

    // ========================================================================
    // ========================================================================

    #region loading methods

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
        if ( ( type == null ) || ( _loaders?[ type ].Size < 1 ) )
        {
            throw new GdxRuntimeException( $"No loader for type: {type?.Name}" );
        }

        AssetLoader? result = null;

        if ( fileName == null )
        {
            Logger.Debug( $"Fetching default loader for {type} : {fileName}" );

            result = _loaders?[ type ][ "" ];
        }
        else
        {
            fileName = IOUtils.NormalizePath( fileName );

            var len = -1;

            foreach ( var entry in _loaders![ type ] )
            {
                if ( ( entry.Key.Length > len ) && fileName.EndsWith( entry.Key ) )
                {
                    result = entry.Value;
                    len    = entry.Key.Length;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Adds the given asset to the loading queue of the AssetManager.
    /// </summary>
    /// <param name="fileName">
    /// The file name (interpretation depends on <see cref="AssetLoader"/>)
    /// </param>
    /// <typeparam name="T"> the type of the asset. </typeparam>
    public void Load< T >( string fileName )
    {
        lock ( this )
        {
            Load( fileName, typeof( T ), null );
        }
    }

    /// <summary>
    /// Adds the given asset to the loading queue of the AssetManager.
    /// </summary>
    /// <param name="fileName">
    /// the file name (interpretation depends on <see cref="AssetLoader"/>)
    /// </param>
    /// <typeparam name="T">the type of the asset.</typeparam>
    /// <param name="parameters"></param>
    public void Load< T >( string? fileName, AssetLoaderParameters? parameters )
    {
        Load( fileName, typeof( T ), parameters );
    }

    /// <summary>
    /// Adds the given asset to the loading queue of the AssetManager.
    /// </summary>
    /// <param name="desc">the <see cref="AssetDescriptor"/></param>
    public void Load( AssetDescriptor desc )
    {
        ArgumentNullException.ThrowIfNull( desc );

        lock ( this )
        {
            Load( desc.AssetName, desc.AssetType, desc.Parameters );
        }
    }

    /// <summary>
    /// Adds the given asset to the loading queue of the AssetManager.
    /// </summary>
    /// <param name="fileName">
    /// the file name (interpretation depends on <see cref="AssetLoader"/>)
    /// </param>
    /// <param name="type">the type of the asset.</param>
    /// <param name="parameters"></param>
    public void Load( string? fileName, Type? type, AssetLoaderParameters? parameters )
    {
        ArgumentNullException.ThrowIfNull( fileName );
        ArgumentNullException.ThrowIfNull( type );

        lock ( this )
        {
            Logger.Debug( $"Loading asset: {IOUtils.NormalizePath( fileName )}" );

            // The result of GetLoader is discarded here, but the call is made
            // to confirm availability of the loader for the supplied asset.
            _ = GetLoader( type, fileName );

            if ( _loadQueue.Count == 0 )
            {
                // Nothing in the load queue, reset stats
                _loaded    = 0;
                _toLoad    = 0;
                _peakTasks = 0;
            }

            ValidatePreloadQueue( fileName, type );

            _toLoad++;

            var descriptor = new AssetDescriptor( fileName, type, parameters );

            // Add this asset to the load queue
            _loadQueue.Add( descriptor );
        }
    }

    /// <summary>
    /// Adds an asset to the asset manager with the specified file name and type.
    /// The asset will be contained within a new RefCountedContainer instance.
    /// </summary>
    /// <param name="fileName">The file name associated with the asset.</param>
    /// <param name="type">The type of the asset.</param>
    /// <param name="asset">The asset to add.</param>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is null.</exception>
    public void AddAsset( string fileName, Type type, object? asset )
    {
        lock ( this )
        {
            Logger.Checkpoint();

            // Add the asset to the filename lookup
//            _assetTypes[ fileName ] = type;
            _assetTypes.Put( fileName, type );

            Logger.Checkpoint();

            var typeToAssets = _assets[ type ];

            Logger.Checkpoint();

            if ( typeToAssets == null )
            {
                Logger.Checkpoint();

                typeToAssets    = new ObjectMap< string, IRefCountedContainer >();
                _assets[ type ] = typeToAssets;

                Logger.Checkpoint();

                typeToAssets[ fileName ] = new RefCountedContainer( asset );

                Logger.Checkpoint();
            }
        }
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
    /// <returns></returns>
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
    /// <param name="assetDesc">the AssetDescriptor of the asset</param>
    public object FinishLoadingAsset( AssetDescriptor assetDesc )
    {
        return FinishLoadingAsset( assetDesc.AssetName );
    }

    /// <summary>
    /// Asynchronously waits for the specified asset to be loaded and returns it upon completion.
    /// </summary>
    /// <param name="fileName">The name of the file of the asset to finish loading.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the loaded asset.</returns>
    public object FinishLoadingAsset( string fileName )
    {
        ArgumentNullException.ThrowIfNull( fileName );

        fileName = IOUtils.NormalizePath( fileName );

        while ( true )
        {
            var type = _assetTypes.Get( fileName );

            if ( type != null )
            {
                var assetsByType   = _assets?.Get( type );
                var assetContainer = assetsByType?.Get( fileName );
                var asset          = assetContainer?.Asset;

                if ( asset != null )
                {
                    return asset;
                }
            }

            Update();
        }
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
        // Normalize the suffix
        suffix = string.IsNullOrEmpty( suffix ) ? "" : suffix;

        _loadersLock.EnterWriteLock();

        try
        {
            if ( !_loaders!.TryGetValue( type, out var typeLoaders ) )
            {
                typeLoaders      = new ObjectMap< string, AssetLoader >();
                _loaders[ type ] = typeLoaders;
            }

            if ( !typeLoaders.TryAdd( suffix, loader ) )
            {
                throw new ArgumentException( $"A loader for type {type.Name} " +
                                             $"with suffix '{suffix}' already exists." );
            }
        }
        finally
        {
            _loadersLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// Asynchronously unloads an asset from the AssetManager.
    /// </summary>
    /// <param name="fileName">The name of the asset to be unloaded.</param>
    /// <returns>A task representing the asynchronous unload operation.</returns>
    /// <exception cref="GdxRuntimeException">Thrown if the asset is not loaded or cannot be found.</exception>
    public void Unload( string fileName )
    {
        fileName = IOUtils.NormalizePath( fileName );

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
        if ( TryRemoveFromQueue( fileName, type ) )
        {
            return;
        }

        // Handle unloading the asset and its dependencies
        UnloadAsset( fileName, type );
    }

    /// <summary>
    /// Returns TRUE if the asset identified by fileName is loaded.
    /// </summary>
    public bool IsLoaded( string? fileName )
    {
        lock ( this )
        {
            return fileName != null && _assetTypes.ContainsKey( fileName );
        }
    }

    /// <summary>
    /// Returns TRUE if the asset identified by fileName and Type is loaded.
    /// </summary>
    public bool IsLoaded( string fileName, Type? type )
    {
        lock ( this )
        {
            ArgumentNullException.ThrowIfNull( type );
            GdxRuntimeException.ThrowIfNull( _assets );

            fileName = IOUtils.NormalizePath( fileName );

            // Retrieve all assets of the required type
            var assetsByType = _assets.Get( type );

            return assetsByType?[ fileName ] != null;
        }
    }

    #endregion loading methods

    // ========================================================================
    // ========================================================================

    #region private methods

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

        _tasks.Enqueue( new AssetLoadingTask( this, assetDesc, loader, _executor ) );

        _peakTasks++;
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

                _ = _tasks.Dequeue();

                if ( task.Cancel )
                {
                    return true;
                }

                AddAsset( task.AssetDesc.AssetName, task.AssetDesc.AssetType, task.Asset );

                if ( task.AssetDesc.Parameters is { LoadedCallback: not null } )
                {
                    task.AssetDesc.Parameters.LoadedCallback.FinishedLoading( this, task.AssetDesc.AssetName, task.AssetDesc.AssetType );
                }

                return true;
            }

            return false;
        }

        Logger.Debug( $"No task to load: {task?.AssetDesc.AssetName}" );

        return false;
    }

    /// <summary>
    /// Loads the next asset task from the load queue. If the asset is already loaded,
    /// increment the loaded count for use in <see cref="GetProgress"/>.
    /// </summary>
    private bool LoadNextTask()
    {
        var assetDesc = _loadQueue.RemoveIndex( 0 );
        var isLoaded  = false;

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

        return isLoaded;
    }

    /// <summary>
    /// Check ths potential asset against assets currently in the preload queue and
    /// the loaded assets ObjectMap, to make sure its has not already been loaded.
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

            // Try to find an asset in the preload queue with the same name as this
            // asset, but with a different asset type.
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

            // Try to find an asset in the loaded assets ObjectMap that has the same
            // name as this asset, but which has a different asset type.
            if ( _assetTypes.ContainsKey( fileName ) )
            {
                var otherType = _assetTypes.Get( fileName );

                if ( ( otherType != null ) && ( otherType != type ) )
                {
                    throw new GdxRuntimeException
                        ( $"Asset with name '{fileName}' already loaded, but has different " +
                          $"type (expected: {type?.Name}, found: {otherType.Name})" );
                }
            }
        }
    }

    /// <summary>
    /// Attempts to cancel the currently processing task if it matches the given file name.
    /// </summary>
    /// <param name="fileName">The name of the file to check for cancellation.</param>
    /// <returns>True if the current task was successfully canceled; otherwise, false.</returns>
    private bool TryCancelCurrentTask( string fileName )
    {
        if ( ( _tasks.Count > 0 ) && ( _tasks.Peek().AssetDesc.AssetName == fileName ) )
        {
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
    private bool TryRemoveFromQueue( string fileName, Type? type )
    {
        for ( var i = 0; i < _loadQueue.Count; i++ )
        {
            if ( _loadQueue[ i ].AssetName.Equals( fileName ) )
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
    /// <param name="fileName">The name of the asset to unload.</param>
    /// <param name="type">The type of the asset to unload.</param>
    /// <returns>A task representing the asynchronous unload operation.</returns>
    private void UnloadAsset( string fileName, Type? type )
    {
        if ( type == null )
        {
            throw new GdxRuntimeException( $"Asset not loaded: {fileName}: Type not specified." );
        }

        GdxRuntimeException.ThrowIfNull( _assets );

        IRefCountedContainer? container = null;

        if ( !_assets.TryGetValue( type, out var assetRef )
             && ( ( assetRef != null ) && !assetRef.TryGetValue( fileName, out container ) ) )
        {
            throw new GdxRuntimeException( $"Asset not loaded: {fileName}" );
        }

        if ( container != null )
        {
            container.RefCount--;

            if ( container.RefCount <= 0 )
            {
                DisposeAsset( container );

                _assetTypes.Remove( fileName );
                assetRef?.Remove( fileName );

                // Remove dependencies if the asset is completely unloaded
                RemoveDependencies( fileName );
            }
            else
            {
                Logger.Debug( $"Unload (decrement): {fileName}" );
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
    /// <param name="fileName">The name of the asset file whose dependencies are to be removed.</param>
    /// <returns>A task representing the asynchronous removal of dependencies.</returns>
    private void RemoveDependencies( string fileName )
    {
        if ( !_assetDependencies.TryGetValue( fileName, out var dependencies ) )
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

        if ( _assets[ _assetTypes[ fileName ] ]?[ fileName ].RefCount <= 0 )
        {
            _assetDependencies.Remove( fileName );
        }
    }

    #endregion private methods
}

// ============================================================================
// ============================================================================