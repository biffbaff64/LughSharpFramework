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

using LughSharp.Lugh.Utils;

namespace LughSharp.Lugh.Assets;

public partial class AssetManager
{
    /// <summary>
    /// Output Assetmanager metrics via the <see cref="Logger"/> class.
    /// </summary>
    public void DisplayMetrics()
    {
        lock ( this )
        {
            Logger.Divider();
            Logger.Debug( $"_assetTypes[].Count: {_assetTypes.Count}" );

            for ( var i = 0; i < _assetTypes.Keys.Count; i++ )
            {
                Logger.Debug( $"Key: {i}" );
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