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


namespace LughSharp.LibCore.Assets.Loaders;

/// <summary>
/// <see cref="AssetLoader"/> for <see cref="Texture"/> instances. The pixel data
/// is loaded asynchronously. The texture is then created on the rendering thread,
/// synchronously.
/// Passing a <see cref="TextureLoaderParameters"/> to <see cref="AssetManager"/>.Load()
/// allows one to specify parameters as can be passed to the various Texture constructors,
/// e.g. filtering, whether to generate mipmaps and so on.
/// </summary>
[PublicAPI]
public class TextureLoader
    : AsynchronousAssetLoader< Texture, TextureLoader.TextureLoaderParameters >, IDisposable
{
    private TextureLoaderInfo _loaderInfo;

    /// <summary>
    /// Creates a new TextureLoader using the specified <see cref="IFileHandleResolver"/>.
    /// A new reference to <see cref="TextureLoaderInfo"/> is created to help with loading.
    /// </summary>
    /// <param name="resolver"> The <see cref="IFileHandleResolver"/> to use. </param>
    public TextureLoader( IFileHandleResolver resolver ) : base( resolver )
    {
        _loaderInfo = new TextureLoaderInfo();
    }

    /// <inheritdoc />
    public override List< AssetDescriptor > GetDependencies( string? filename, FileInfo? file, AssetLoaderParameters? p )
    {
        return null!;
    }

    /// <inheritdoc />
    public override void LoadAsync( AssetManager? manager, FileInfo? file, TextureLoaderParameters? parameter )
    {
        _loaderInfo.Filename = file?.Name;

        if ( parameter?.TextureData == null )
        {
            var format     = Pixmap.ColorFormat.Default;
            var genMipMaps = false;

            _loaderInfo.Texture = null;

            if ( parameter != null )
            {
                format              = parameter.Format;
                genMipMaps          = parameter.GenMipMaps;
                _loaderInfo.Texture = parameter.Texture;
            }

            _loaderInfo.Data = TextureDataFactory.LoadFromFile( file!, format, genMipMaps );
        }
        else
        {
            _loaderInfo.Data    = parameter.TextureData;
            _loaderInfo.Texture = parameter.Texture;
        }

        if ( _loaderInfo.Data is { IsPrepared: false } )
        {
            _loaderInfo.Data.Prepare();
        }
    }

    /// <inheritdoc />
    public override Texture LoadSync( AssetManager manager, FileInfo? file, TextureLoaderParameters? parameter )
    {
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
            texture.SetFilter( parameter.MinFilter, parameter.MagFilter );
            texture.SetWrap( parameter.WrapU, parameter.WrapV );
        }

        return texture;
    }

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    /// <summary>
    /// Contains information about a texture being loaded.
    /// </summary>
    [PublicAPI]
    public class TextureLoaderInfo
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
    }

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    /// <summary>
    /// Parameters for loading a texture asset.
    /// </summary>
    [PublicAPI]
    public class TextureLoaderParameters : AssetLoaderParameters
    {
        /// <summary>
        /// Gets or sets the minification filter for the texture.
        /// </summary>
        public TextureFilter MinFilter { get; set; } = TextureFilter.Nearest;

        /// <summary>
        /// Gets or sets the magnification filter for the texture.
        /// </summary>
        public TextureFilter MagFilter { get; set; } = TextureFilter.Nearest;

        /// <summary>
        /// Gets or sets the wrapping mode for the texture in the horizontal direction.
        /// </summary>
        public TextureWrap WrapU { get; set; } = TextureWrap.ClampToEdge;

        /// <summary>
        /// Gets or sets the wrapping mode for the texture in the vertical direction.
        /// </summary>
        public TextureWrap WrapV { get; set; } = TextureWrap.ClampToEdge;

        /// <summary>
        /// Gets or sets the format of the final texture. Uses the source image's format if null.
        /// </summary>
        public Pixmap.ColorFormat Format { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to generate mipmaps for the texture.
        /// </summary>
        public bool GenMipMaps { get; set; } = false;

        /// <summary>
        /// Gets or sets the texture object to put the <see cref="TextureData"/> in (optional).
        /// </summary>
        public Texture? Texture { get; set; } = null;

        /// <summary>
        /// Gets or sets the <see cref="ITextureData"/> for textures created on the fly (optional).
        /// When set, all format and genMipMaps are ignored.
        /// </summary>
        public ITextureData? TextureData { get; set; } = null;
    }

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    
    #region dispose pattern

    /// <summary>
    /// Performs application-defined tasks associated with freeing,
    /// releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose( true );
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

    #endregion dispose pattern
}

// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------
// ----------------------------------------------------------------------------


