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
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Assets.Loaders;

/// <summary>
/// <see cref="AssetLoader"/> for <see cref="Pixmap"/> instances.
/// The Pixmap is loaded asynchronously.
/// </summary>
[PublicAPI]
public class PixmapLoader : AsynchronousAssetLoader, IDisposable
{
    private Pixmap? _pixmap;

    /// <summary>
    /// Creates a new PixmapLoader using the provided <see cref="IFileHandleResolver"/>
    /// </summary>
    /// <param name="resolver"> The resolver to use. </param>
    public PixmapLoader( IFileHandleResolver resolver ) : base( resolver )
    {
        _pixmap = null!;
    }

    /// <summary>
    /// Returns the assets this asset requires to be loaded first. This method may be
    /// called on a thread other than the GL thread.
    /// </summary>
    /// <param name="filename">name of the asset to load</param>
    /// <param name="file">the resolved file to load</param>
    /// <param name="p">parameters for loading the asset</param>
    public override List< AssetDescriptor > GetDependencies< TP >( string filename,
                                                                   FileInfo file,
                                                                   TP? p ) where TP : class
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
        Guard.Against.Null( file );

        _pixmap = new Pixmap( file );
    }

    /// <summary>
    /// Loads the OpenGL part of the asset.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="file"> the resolved file to load </param>
    /// <param name="parameter"></param>
    public override object? LoadSync< TP >( AssetManager manager,
                                            FileInfo file,
                                            TP? parameter ) where TP : class
    {
        var pixmap = _pixmap;

        _pixmap = null;

        return pixmap;
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
            _pixmap?.Dispose();
            _pixmap = null!;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Parameters for loading Pixmap assets. The default class provides no extra
    /// parameters and acts as a placeholder for possible future extensions.
    /// </summary>
    [PublicAPI]
    public class PixmapLoaderParameter : AssetLoaderParameters
    {
    }
}

// ============================================================================
// ============================================================================
