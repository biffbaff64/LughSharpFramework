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

using LughSharp.Source.Assets.Loaders.Resolvers;

namespace LughSharp.Source.Assets.Loaders;

/// <summary>
/// <see cref="AssetLoader"/> for <see cref="I18NBundle"/> instances. The asset name
/// is the root bundle file, e.g. <c>i18n/strings.properties</c>; culture variants
/// are found from it as described in <see cref="I18NBundle"/>.
/// The bundle is loaded asynchronously.
/// </summary>
[PublicAPI]
public class I18NBundleLoader : AsynchronousAssetLoader
{
    private I18NBundle? _bundle;

    /// <summary>
    /// Creates a new I18NBundleLoader using the provided <see cref="IFileHandleResolver"/>
    /// </summary>
    /// <param name="resolver"> The resolver to use. </param>
    public I18NBundleLoader( IFileHandleResolver resolver ) : base( resolver )
    {
    }

    /// <inheritdoc/>
    public override List< AssetDescriptor > GetDependencies< TP >( string filename,
                                                                   FileInfo file,
                                                                   TP? p ) where TP : class
    {
        return null!;
    }

    /// <summary>
    /// Reads and parses the bundle files. No OpenGL work is needed, so this
    /// does all of the loading.
    /// </summary>
    /// <param name="manager">The asset manager responsible for loading the asset.</param>
    /// <param name="filename"> The name of the asset to load. </param>
    /// <param name="file">The root bundle file.</param>
    /// <param name="parameter">
    /// An optional <see cref="I18NBundleParameter"/>. If not supplied, the bundle is loaded
    /// for <see cref="CultureInfo.CurrentUICulture"/> using UTF-8 encoding.
    /// </param>
    public override void LoadAsync< TP >( AssetManager manager,
                                          string filename,
                                          FileInfo? file,
                                          TP? parameter ) where TP : class
    {
        Guard.Against.Null( file );

        var p = parameter as I18NBundleParameter;

        _bundle = I18NBundle.CreateBundle( file,
                                           p?.Culture ?? CultureInfo.CurrentUICulture,
                                           p?.Encoding );
    }

    /// <inheritdoc/>
    public override object? LoadSync< TP >( AssetManager manager,
                                            FileInfo file,
                                            TP? parameter ) where TP : class
    {
        I18NBundle? bundle = _bundle;

        _bundle = null;

        return bundle;
    }

    /// <inheritdoc/>
    public override void UnloadAsync< TP >( AssetManager manager,
                                            string filename,
                                            FileInfo file,
                                            TP? parameter ) where TP : class
    {
        _bundle = null;
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Parameters for loading <see cref="I18NBundle"/> assets.
    /// </summary>
    [PublicAPI]
    public class I18NBundleParameter : AssetLoaderParameters
    {
        /// <summary>
        /// The culture to load strings for. If null, <see cref="CultureInfo.CurrentUICulture"/>
        /// is used.
        /// </summary>
        public CultureInfo? Culture { get; set; }

        /// <summary>
        /// The encoding of the bundle files. If null, UTF-8 is used.
        /// </summary>
        public Encoding? Encoding { get; set; }
    }
}

// ============================================================================
// ============================================================================
