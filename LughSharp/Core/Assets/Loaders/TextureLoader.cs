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

using LughSharp.Core.Assets.Loaders.Resolvers;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Assets.Loaders;

/// <summary>
/// <see cref="AssetLoader"/> for <see cref="Texture"/> instances. The pixel data
/// is loaded asynchronously. The texture is then created on the rendering thread,
/// synchronously.
/// Passing a <see cref="TextureLoaderParameters"/> to <see cref="AssetManager"/>.Load()
/// allows one to specify parameters as can be passed to the various Texture constructors,
/// e.g. filtering, whether to generate mipmaps and so on.
/// </summary>
[PublicAPI]
public class TextureLoader : AsynchronousAssetLoader, IDisposable
{
    private TextureLoaderInfo _loaderInfo;

    // ========================================================================

    /// <summary>
    /// Creates a new TextureLoader using the specified <see cref="IFileHandleResolver"/>.
    /// A new reference to <see cref="TextureLoaderInfo"/> is created to help with loading.
    /// </summary>
    /// <param name="resolver"> The <see cref="IFileHandleResolver"/> to use. </param>
    public TextureLoader( IFileHandleResolver resolver ) : base( resolver )
    {
        _loaderInfo = new TextureLoaderInfo();
    }

    // ========================================================================

    /// <summary>
    /// Returns the assets this asset requires to be loaded first. This method may be
    /// called on a thread other than the GL thread.
    /// </summary>
    /// <param name="filename"> name of the asset to load. </param>
    /// <param name="file"> the resolved file to load. </param>
    /// <param name="parameters"> parameters for loading the asset. </param>
    public override List< AssetDescriptor > GetDependencies< TP >( string filename,
                                                                   FileInfo file,
                                                                   TP? parameters ) where TP : class
    {
        return null!;
    }

    /// <summary>
    /// Loads the non-OpenGL part of the asset and injects any dependencies of
    /// the asset into the <paramref name="manager"/>.
    /// </summary>
    /// <param name="manager">The asset manager responsible for loading the asset.</param>
    /// <param name="filename"> The name of the asset to load. </param>
    /// <param name="file">The file information of the asset to load.</param>
    /// <param name="parameter">The parameters for loading the asset.</param>
    public override void LoadAsync< TP >( AssetManager manager,
                                          string filename,
                                          FileInfo? file,
                                          TP? parameter ) where TP : class
    {
        if ( manager == null )
        {
            throw new RuntimeException( "NULL AssetManager passed to TextureLoader.loadAsync" );
        }

        if ( string.IsNullOrEmpty( filename ) )
        {
            throw new RuntimeException( "filename cannot be null or empty" );
        }

        var p = parameter as TextureLoaderParameters;

        _loaderInfo.Filename = file?.Name;

        if ( p?.TextureData == null )
        {
            var format     = LughFormat.RGBA8888;
            var genMipMaps = false;

            _loaderInfo.Texture = null;

            if ( parameter != null )
            {
                format              = p!.Format;
                genMipMaps          = p.GenMipMaps;
                _loaderInfo.Texture = p.Texture;
            }

            _loaderInfo.Data = TextureDataFactory.LoadFromFile( file!, format, genMipMaps );
        }
        else
        {
            _loaderInfo.Data    = p.TextureData;
            _loaderInfo.Texture = p.Texture;
        }

        if ( _loaderInfo.Data is { IsPrepared: false } )
        {
            _loaderInfo.Data.Prepare();
        }
    }

    /// <summary>
    /// Loads the OpenGL part of the asset.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="file"> the resolved file to load </param>
    /// <param name="parameter"></param>
    public override Texture LoadSync< TP >( AssetManager manager,
                                            FileInfo file,
                                            TP? parameter ) where TP : class
    {
        var p       = parameter as TextureLoaderParameters;
        var texture = _loaderInfo.Texture;

        if ( texture != null )
        {
            texture.Load( _loaderInfo.Data! );
        }
        else
        {
            texture = new Texture( _loaderInfo.Data! );
        }

        if ( parameter != null )
        {
            texture.SetFilter( p!.MinFilter, p.MagFilter );
            texture.SetWrap( p.WrapU, p.WrapV );
        }

        return texture;
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
    protected virtual void Dispose( bool disposing )
    {
        if ( disposing )
        {
            // Dispose managed resources
            _loaderInfo = null!;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Contains information about a texture being loaded.
    /// </summary>
    [PublicAPI]
    public class TextureLoaderInfo : IDisposable
    {
        /// <summary>
        /// Gets or sets the filename of the texture.
        /// </summary>
        public string? Filename { get; set; }

        /// <summary>
        /// Gets or sets the texture data.
        /// </summary>
        public ITextureData? Data { get; set; }

        /// <summary>
        /// Gets or sets the loaded texture object.
        /// </summary>
        public Texture? Texture { get; set; }

        // ====================================================================

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose( true );

            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Releases the unmanaged resources used by the texture loader info.
        /// </summary>
        /// <param name="disposing">
        /// True to release both managed and unmanaged resources; false to release only
        /// unmanaged resources.
        /// </param>
        protected virtual void Dispose( bool disposing )
        {
            if ( disposing )
            {
                // Dispose managed resources
                Data    = null;
                Texture = null;
            }
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Parameters for loading a texture asset.
    /// </summary>
    [PublicAPI]
    public class TextureLoaderParameters : AssetLoaderParameters
    {
        /// <summary>
        /// Gets or sets the minification filter for the texture.
        /// </summary>
        public TextureFilterMode MinFilter { get; set; } = TextureFilterMode.Nearest;

        /// <summary>
        /// Gets or sets the magnification filter for the texture.
        /// </summary>
        public TextureFilterMode MagFilter { get; set; } = TextureFilterMode.Nearest;

        /// <summary>
        /// Gets or sets the wrapping mode for the texture in the horizontal direction.
        /// </summary>
        public TextureWrapMode WrapU { get; set; } = TextureWrapMode.ClampToEdge;

        /// <summary>
        /// Gets or sets the wrapping mode for the texture in the vertical direction.
        /// </summary>
        public TextureWrapMode WrapV { get; set; } = TextureWrapMode.ClampToEdge;

        /// <summary>
        /// Gets or sets the format of the final texture. Uses the source image's format if null.
        /// </summary>
        public int Format { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate mipmaps for the texture.
        /// </summary>
        public bool GenMipMaps { get; set; } = false;

        /// <summary>
        /// Gets or sets the texture object to put the <see cref="TextureData"/> in (optional).
        /// </summary>
        public Texture? Texture { get; set; } = null;

        /// <summary>
        /// Gets or sets the <see cref="ITextureData"/> for textures created on the fly
        /// (optional). When set, all format and genMipMaps are ignored.
        /// </summary>
        public ITextureData? TextureData { get; set; } = null;
    }
}

// ====================================================================--------
// ====================================================================--------