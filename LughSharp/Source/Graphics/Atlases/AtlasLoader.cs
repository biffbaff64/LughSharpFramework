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

using LughSharp.Source.IO;

namespace LughSharp.Source.Graphics.Atlases;

[PublicAPI]
public class AtlasLoader
{
    private readonly AssetManager   _assetManager;
    private readonly AssetUtils     _assetUtils;
    private readonly List< string > _atlases = [ ];

    // ========================================================================

    /// <summary>
    /// Creates a new AtlasLoader.
    /// </summary>
    /// <param name="assetManager"> The <see cref="AssetManager"/> to use. </param>
    public AtlasLoader( AssetManager assetManager )
    {
        _assetManager = assetManager;
        _assetUtils   = new AssetUtils( assetManager );
    }

    /// <summary>
    /// Loads all registered atlases into the AssetManager.
    /// </summary>
    public void Load()
    {
        if ( _atlases.Count == 0 )
        {
            return;
        }
        
        foreach ( string atlasPath in _atlases )
        {
            var atlas = _assetUtils.LoadSingleAsset< TextureAtlas >( atlasPath );
            
            atlas?.AtlasName = atlasPath;
        }
    }

    /// <summary>
    /// Registers an atlas to be loaded when <see cref="Load"/> is called. The atlas
    /// will not be registered if it has already been registered.
    /// </summary>
    /// <param name="path">
    /// The path to the atlas to register. This path should be relative to the library's
    /// content root, which will be added to the provided path when registering the atlas.
    /// An example would be:
    /// <code>
    ///     @"\PackedImages\output\animations.atlas"
    /// </code>
    /// </param>
    /// <exception cref="FileNotFoundException"> Thrown if the atlas cannot be found. </exception>
    /// <exception cref="ArgumentException"> Thrown if if the path does not end with .atlas. </exception>
    public AtlasLoader RegisterAtlas( string path )
    {
        if ( !path.EndsWith( ".atlas" ) )
        {
            throw new ArgumentException( "The atlas path must end with .atlas" );
        }

        path = $"{Files.ContentRoot}{path}";
        
        if ( !File.Exists( path ) )
        {
            throw new FileNotFoundException( $"Atlas file not found: {path}" );
        }
        
        if ( !_atlases.Contains( path ) )
        {
            _atlases.Add( path );
        }

        return this;
    }
}

// ============================================================================
// ============================================================================