// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

using System;
using System.Collections.Generic;
using System.IO;

using JetBrains.Annotations;

using LughSharp.Core.Assets.Loaders.Resolvers;
using LughSharp.Core.Audio;
using LughSharp.Core.Main;

namespace LughSharp.Core.Assets.Loaders;

/// <summary>
/// <see cref="AssetLoader"/> to load <see cref="ISound"/> instances.
/// </summary>
[PublicAPI]
public class SoundLoader : AsynchronousAssetLoader, IDisposable
{
    /// <summary>
    /// The <see cref="ISound"/> instance currently loaded by this <see cref="SoundLoader"/>.
    /// </summary>
    public ISound? LoadedSound { get; set; }

    /// <summary>
    /// Creates a new SoundLoader using the provided <see cref="IFileHandleResolver"/>
    /// </summary>
    /// <param name="resolver"></param>
    public SoundLoader( IFileHandleResolver resolver ) : base( resolver )
    {
        LoadedSound = null!;
    }

    /// <inheritdoc />
    public override List< AssetDescriptor > GetDependencies< TP >( string filename,
                                                                   FileInfo file,
                                                                   TP? p ) where TP : class
    {
        return null!;
    }

    /// <summary>
    /// Loads the non-OpenGL part of the asset and injects any dependencies of
    /// the asset into the AssetManager.
    /// </summary>
    /// <param name="manager"> The <see cref="AssetManager"/> to use. </param>
    /// <param name="filename"></param>
    /// <param name="file"> A <see cref="FileInfo"/> object holding file information. </param>
    /// <param name="parameter"> <see cref="SoundLoaderParameters"/> to use. </param>
    public override void LoadAsync< TP >( AssetManager manager,
                                          string filename,
                                          FileInfo? file,
                                          TP? parameter ) where TP : class
    {
        LoadedSound = Engine.Api.Audio.NewSound( file );
    }

    /// <summary>
    /// Loads the sound asset synchronously.
    /// </summary>
    /// <param name="manager">The asset manager responsible for loading assets.</param>
    /// <param name="file">The file information of the sound asset.</param>
    /// <param name="parameter">The parameters for loading the sound asset (ignored).</param>
    /// <returns>The loaded sound asset, or null if no sound is loaded.</returns>
    public override ISound? LoadSync< TP >( AssetManager manager, FileInfo file, TP? parameter ) where TP : class
    {
        ISound? sound = LoadedSound;

        LoadedSound = null;

        return sound;
    }

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
    protected void Dispose( bool disposing )
    {
        if ( disposing )
        {
            LoadedSound = null!;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Parameters for loading sound assets. The default class provides no extra
    /// parameters and acts as a placeholder for possible future extensions.
    /// </summary>
    [PublicAPI]
    public class SoundLoaderParameters : AssetLoaderParameters
    {
    }
}

// ============================================================================
// ============================================================================