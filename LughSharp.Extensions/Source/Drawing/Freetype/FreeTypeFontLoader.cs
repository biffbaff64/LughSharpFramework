// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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

using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using LughSharp.Core.Assets;
using LughSharp.Core.Assets.Loaders;
using LughSharp.Core.Assets.Loaders.Resolvers;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Utils.Exceptions;

namespace Extensions.Source.Drawing.Freetype;

/// <summary>
/// Creates <see cref="BitmapFont"/> instances from FreeType font files.
/// Requires a <see cref="FreeTypeFontLoaderParameter"/> to be passed to
/// <see cref="AssetManager.Load(string?, Type?, AssetLoaderParameters?)"/>
/// which specifies the name of the TTF file as well the parameters
/// used to generate the BitmapFont (size, characters, etc.)
/// </summary>
[PublicAPI]
public class FreetypeFontLoader : AsynchronousAssetLoader
{
    public FreetypeFontLoader( IFileHandleResolver resolver )
        : base( resolver )
    {
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
        if ( parameter == null )
        {
            throw new RuntimeException( "FreetypeFontParameter must be set in " +
                                        "AssetManager#load to point at a TTF file!" );
        }
    }

    /// <summary>
    /// Loads the OpenGL part of the asset.
    /// </summary>
    /// <param name="manager"></param>
    /// <param name="file"> the resolved file to load </param>
    /// <param name="p"></param>
    public override object? LoadSync< TP >( AssetManager manager, FileInfo file, TP? p ) where TP : class
    {
        if ( p == null )
        {
            throw new RuntimeException( "FreetypeFontParameter must be set in AssetManager.load" +
                                        " to point at a TTF file!" );
        }

        var parameter = p as FreeTypeFontLoaderParameter;

        var generator = manager.Get< FreeTypeFontGenerator >( parameter?.FontFileName + ".gen",
                                                              typeof( FreeTypeFontGenerator ),
                                                              true );

        var font = generator?.GenerateFont( parameter!.FontParameters );

        return font;
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
        var parameter = p as FreeTypeFontLoaderParameter;

        var deps = new List< AssetDescriptor >
        {
            new( parameter?.FontFileName + ".gen", typeof( FreeTypeFontGenerator ), parameter )
        };

        return deps;
    }

    // ========================================================================
    
    public class FreeTypeFontLoaderParameter : BitmapFontParameter
    {
        /// the name of the TTF file to be used to load the font
        public string FontFileName { get; set; } = "";

        /// the parameters used to generate the font, e.g. size, characters, etc.
        public readonly FreeTypeFontGenerator.FreeTypeFontParameter FontParameters = new();
    }
}

// ============================================================================
// ============================================================================