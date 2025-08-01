﻿// /////////////////////////////////////////////////////////////////////////////
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

using System.Text;

using LughSharp.Lugh.Audio;
using LughSharp.Lugh.Graphics;
using LughSharp.Lugh.Graphics.Atlases;
using LughSharp.Lugh.Graphics.G2D;
using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Graphics.Text;
using LughSharp.Lugh.Graphics.Utils;
using LughSharp.Lugh.Scenes.Scene2D.UI;
using LughSharp.Lugh.Utils;

namespace LughSharp.Lugh.Assets;

public partial class AssetManager
{
    private readonly HashSet< Type > _typeList =
    [
        typeof( Texture ),
        typeof( BitmapFont ),
        typeof( Pixmap ),
        typeof( TextureAtlas ),
        typeof( Skin ),
        typeof( ParticleEffect ),
        typeof( PolygonRegion ),
        typeof( ShaderProgram ),
        typeof( Cubemap ),
        typeof( ISound ),
        typeof( IMusic ),
    ];

    /// <summary>
    /// Retrieves an asset of the specified type by its name, if it exists within
    /// the loaded assets.
    /// </summary>
    /// <typeparam name="T"> The type of the asset to retrieve. </typeparam>
    /// <param name="name"> The name of the asset to retrieve. </param>
    /// <returns>
    /// The asset cast to the specified type, or null if not found or if the type
    /// does not match.
    /// </returns>
    public T? GetAs< T >( string name ) where T : class
    {
        if ( _typeList.TryGetValue( typeof( T ), out var _ ) )
        {
            return Get( name ) as T;
        }

        return null;
    }

    // ========================================================================

    /// <summary>
    /// Output Assetmanager metrics via the <see cref="Logger" /> class.
    /// </summary>
    public void DisplayMetrics()
    {
        lock ( this )
        {
            Logger.Divider();
            Logger.Debug( $"_assetTypes[].Count: {_assetTypes.Count}" );

            foreach ( var key in _assetTypes.Keys )
            {
                Logger.Debug( $"{key}: " );

                foreach ( var value in _assetTypes.Values )
                {
                    Logger.Debug( $"{value}" );
                }
            }

            Logger.Debug( $"_assets Count       : {_assets.Count}" );
            Logger.Debug( $"_loaders Count      : {_loaders?.Count}" );
            Logger.Debug( $"_loaded             : {_loaded}" );
            Logger.Debug( $"_toLoad             : {_toLoad}" );
            Logger.Debug( $"_loadQueue Count    : {_loadQueue.Count}" );
            Logger.Debug( $"_tasks Count        : {_tasks.Count}" );
            Logger.Divider();
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
            sb.Append( $"_loaders.Length   : {_loaders?.Count}\n" );
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

            return sb.ToString();
        }
    }

    // ========================================================================
    // ========================================================================
}