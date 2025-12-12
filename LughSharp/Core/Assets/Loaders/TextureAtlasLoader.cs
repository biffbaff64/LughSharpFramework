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

using JetBrains.Annotations;
using LughSharp.Core.Assets.Loaders.Resolvers;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;

namespace LughSharp.Core.Assets.Loaders;

/// <summary>
/// AssetLoader to load TextureAtlas instances.
/// </summary>
[PublicAPI]
public class TextureAtlasLoader : SynchronousAssetLoader< TextureAtlas >, IDisposable
{
    private TextureAtlasData? _data;

    /// <summary>
    /// Creates a new TextureAtlasLoader using the supplied resolver.
    /// </summary>
    public TextureAtlasLoader( IFileHandleResolver resolver )
        : base( resolver )
    {
    }

    /// <summary>
    /// Loads the texture atlas using the provided asset manager, file, and parameters.
    /// </summary>
    /// <param name="assetManager">The asset manager responsible for loading the assets.</param>
    /// <param name="file">The file information of the texture atlas.</param>
    /// <param name="parameter">The parameters for loading the texture atlas.</param>
    /// <returns>The loaded texture atlas.</returns>
    /// <exception cref="GdxRuntimeException">Thrown if the texture atlas data is null.</exception>
    public override TextureAtlas Load( AssetManager assetManager, FileInfo? file, AssetLoaderParameters? parameter )
    {
        Logger.Checkpoint();
        
        if ( _data == null )
        {
            throw new GdxRuntimeException( "TextureAtlasData cannot be null!" );
        }

        foreach ( var page in _data.Pages )
        {
            if ( page.TextureFile != null )
            {
                var texture = assetManager.Get< Texture >( page.TextureFile.FullName );
                page.Texture = texture;
            }
            else
            {
                Logger.Debug( "page texture file is null!" );
            }
        }

        var atlas = new TextureAtlas( _data );

        _data = null;

        return atlas;
    }

    /// <inheritdoc />
    public override List< AssetDescriptor > GetDependencies< TP >( string filename,
                                                                   FileInfo atlasFile,
                                                                   TP? parameter ) where TP : class
    {
        Guard.Against.Null( atlasFile );

        var p      = parameter as TextureAtlasParameter;
        var imgDir = atlasFile.Directory;

        Guard.Against.Null( imgDir );
        
        _data = p != null
            ? new TextureAtlasData( atlasFile, imgDir, p.FlipVertically )
            : new TextureAtlasData( atlasFile, imgDir );

        var dependencies = new List< AssetDescriptor >();

        foreach ( var page in _data.Pages )
        {
            var tparams = new TextureLoader.TextureLoaderParameters
            {
                Format     = page.Format,
                GenMipMaps = page.UseMipMaps,
                MinFilter  = page.MinFilter,
                MagFilter  = page.MagFilter,
            };

            if ( page.TextureFile != null )
            {
                dependencies.Add( new AssetDescriptor( page.TextureFile, typeof( Texture ), tparams ) );
            }
        }

        return dependencies;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing,
    /// releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    /// <summary>
    /// Releases the unmanaged resources used by the texture loader.
    /// </summary>
    /// <param name="disposing">
    /// True to release both managed and unmanaged resources; false to release only unmanaged resources.
    /// </param>
    protected void Dispose( bool disposing )
    {
        if ( disposing )
        {
            _data = null;
        }
    }
}

// ========================================================================
// ========================================================================

/// <summary>
/// Parameters for loading a <see cref="TextureAtlas"/>
/// </summary>
[PublicAPI]
public class TextureAtlasParameter( bool flip ) : AssetLoaderParameters
{
    public TextureAtlasParameter() : this( false )
    {
    }

    public bool FlipVertically { get; } = flip;
}

// ============================================================================
// ============================================================================
