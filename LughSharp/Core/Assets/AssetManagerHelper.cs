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

using LughSharp.Core.Audio;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Graphics.Utils;
using LughSharp.Core.Scenes.Scene2D.UI;

namespace LughSharp.Core.Assets;

/// <summary>
/// Debug methods for the AssetManager.
/// </summary>
public partial class AssetManager
{
    protected readonly HashSet< Type > TypeList =
    [
        typeof( BitmapFont ),
        typeof( Cubemap ),
        typeof( IMusic ),
        typeof( ISound ),
        typeof( ParticleEffect ),
        typeof( Pixmap ),
        typeof( PolygonRegion ),
        typeof( ShaderProgram ),
        typeof( Skin ),
        typeof( Texture ),
        typeof( TextureAtlas ),
    ];

    // ========================================================================

    [Conditional( "DEBUG" )]
    public void DisplayMetrics()
    {
        lock ( this )
        {
            Logger.Divider();
            Logger.Debug( $"_assetTypes[].Count: {_assetTypes.Count}" );

            foreach ( var key in _assetTypes.Keys )
            {
                Logger.Debug( $"Key: {key}: " );

                foreach ( var value in _assetTypes.Values )
                {
                    Logger.Debug( $"Value: {value}" );
                }
            }

            Logger.Debug( $"_assets Count       : {_assets.Count}" );
            Logger.Debug( $"_loaders Count      : {_loaders?.Count}" );
            Logger.Debug( $"_loaded             : {_loaded}" );
            Logger.Debug( $"_toLoad             : {_toLoad}" );
            Logger.Debug( $"_loadQueue Count    : {_loadQueue.Count}" );
            Logger.Debug( $"_tasks Count        : {_tasks.Count}" );
            Logger.Divider();

            var names = GetAssetNames();

            if ( names.Count == 0 )
            {
                Logger.Debug( "No assets loaded." );

                return;
            }

            foreach ( var name in names )
            {
                if ( name != null )
                {
                    var type  = GetAssetType( name );
                    var asset = Get( name );

                    Logger.Debug( $"Asset: {name}, Type: {type.Name}, " +
                                  $"Asset: {( asset != null ? "Loaded" : "NULL" )}" );
                }
            }
        }
    }

    public void DebugPrint()
    {
        lock ( this )
        {
            Guard.ThrowIfNull( _assets );
            Guard.ThrowIfNull( _loaders );
            Guard.ThrowIfNull( _assetTypes );

            Logger.Debug( $"Number of Assets: {_assets.Count}" );
            Logger.Debug( $"Number of Types: {_assetTypes.Count}" );

            Logger.Debug( $"\n--- Asset Loader Debug Dump ({_loaders.Count} Asset Types Registered) ---" );

            // Iterate over the outer Dictionary (Key: Asset Type)
            foreach ( var outerEntry in _loaders )
            {
                var assetType    = outerEntry.Key;
                var innerLoaders = outerEntry.Value;

                Logger.Debug( $"\n[ASSET TYPE]: {assetType.Name} (Total Loaders: {innerLoaders.Count})" );
                Logger.Debug( "--------------------------------------------------" );

                // Iterate over the inner Dictionary (Key: Asset Name, Value: Concrete Loader)
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

//    public string GetDiagnostics()
//    {
//        lock ( this )
//        {
//            var sb = new StringBuilder();
//
//            sb.Append( $"_assets.Length    : {_assets.Count}\n" );
//            sb.Append( $"_assetTypes.Length: {_assetTypes.Count}\n" );
//            sb.Append( $"_loaders.Length   : {_loaders?.Count}\n" );
//            sb.Append( $"_tasks.Length     : {_tasks.Count}\n" );
//            sb.Append( $"_loadQueue.Length : {_loadQueue.Count}\n" );
//            sb.Append( $"_injected.Length  : {_injected.Count}\n" );
//
//            foreach ( var filename in _assetTypes.Keys )
//            {
//                if ( sb.Length > 0 )
//                {
//                    sb.Append( '\n' );
//                }
//
//                sb.Append( filename ).Append( ", " );
//
//                var type = _assetTypes[ filename ];
//
//                if ( _assets == null )
//                {
//                    sb.Append( "NULL assets List!" );
//                }
//                else
//                {
//                    var dependencies = _assetDependencies?[ filename ];
//
//                    if ( dependencies != null )
//                    {
//                        sb.Append( type.Name );
//                        sb.Append( ", refs: " ).Append( _assets[ type ]?[ filename ].RefCount );
//                        sb.Append( ", deps: [" );
//
//                        foreach ( var dep in dependencies )
//                        {
//                            sb.Append( dep );
//                            sb.Append( ',' );
//                        }
//
//                        sb.Append( ']' );
//                    }
//                }
//            }
//
//            return sb.ToString();
//        }
//    }

    [Conditional( "DEBUG" )]
    protected void DebugAssetLoaders()
    {
        lock ( this )
        {
            foreach ( var loader in _loaders! )
            {
                Logger.Debug( $"Type: {loader.Key.Name}" );

                foreach ( var entry in loader.Value )
                {
                    var suffix = string.IsNullOrEmpty( entry.Key ) ? "(default)" : entry.Key;

                    Logger.Debug( $"  Suffix: '{suffix}' => Loader class: {entry.Value.GetType().Name}" );
                }
            }
        }
    }
}

// ============================================================================
// ============================================================================