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

using System.Runtime.Versioning;
using JetBrains.Annotations;
using LughSharp.Core.Files;
using LughSharp.Core.Graphics.Atlases;
using LughUtils.source.Exceptions;
using LughUtils.source.Maths;
using Bitmap = System.Drawing.Bitmap;

namespace Extensions.Source.Tools.TexturePacker;

/// <summary>
/// TexturePacker can pack all images for an application in one shot. Given a directory, it
/// recursively scans for image files. For each directory of images TexturePacker encounters,
/// it packs the images on to a larger texture, called a page. If the images in a directory
/// don’t fit on the max size of a single page, multiple pages will be used.
/// <para>
/// Images in the same directory go on the same set of pages. If all images fit on a single page,
/// no subdirectories should be used because with one page the app will only ever perform one
/// texture bind. Otherwise, subdirectories can be used to segregate related images to minimize
/// texture binds.
/// </para>
/// <para>
/// E.g., an application may want to place all the “game” images in a separate directory from the
/// “pause menu” images, since these two sets of images are drawn serially: all the game images
/// are drawn (one bind), then the pause menu is drawn on top (another bind). If the images were
/// in a single directory that resulted in more than one page, each page could contain a mix of
/// game and pause menu images. This would cause multiple texture binds to render the game and
/// pause menu instead of just one each.
/// </para>
/// <para>
/// Subdirectories are also useful to group images with related texture settings. Settings like
/// runtime memory format (RGBA, RGB, etc.) and filtering (nearest, linear, etc.) are per texture.
/// Images that need different per-texture settings need to go on separate pages, so should be
/// placed in separate subdirectories.
/// </para>
/// <para>
/// To use subdirectories for organising without TexturePacker outputting a set of pages for
/// each subdirectory, see the combineSubdirectories setting.
/// To avoid subdirectory paths being used in image names in the atlas file, see the flattenPaths
/// setting.
/// </para>
/// <para>
/// Each directory may contain a “pack.json” file, which is a JSON representation of the
/// <see cref="TexturePackerSettings"/> class. Each subdirectory inherits all the settings from
/// its parent directory. Any settings set in the subdirectory override those set in the parent
/// directory.
/// </para>
/// <para>
/// Below is a JSON example with every available setting and the default value for each. All settings
/// do not need to be specified, any or all may be omitted. If a setting is not specified for a
/// directory or any parent directory, the default value is used.
/// <code>
/// {
///     "MultipleOfFour": true,
///     "Rotation": false,
///     "PowerOfTwo": true,
///     "PaddingX": 2,
///     "PaddingY": 2,
///     "EdgePadding": true,
///     "DuplicatePadding": false,
///     "MinWidth": 16,
///     "MinHeight": 16,
///     "MaxWidth": 1024,
///     "MaxHeight": 1024,
///     "Square": false,
///     "StripWhitespaceX": false,
///     "StripWhitespaceY": false,
///     "AlphaThreshold": 0,
///     "FilterMin": "Nearest",
///     "FilterMag": "Nearest",
///     "WrapX": "ClampToEdge",
///     "WrapY": "ClampToEdge",
///     "Format": "RGBA8888",
///     "Alias": true,
///     "OutputFormat": "png",
///     "JpegQuality": 0.9,
///     "IgnoreBlankImages": true,
///     "Fast": false,
///     "Debug": false,
///     "Silent": false,
///     "CombineSubdirectories": false,
///     "Ignore": true,
///     "FlattenPaths": false,
///     "PremultiplyAlpha": false,
///     "UseIndexes": true,
///     "Bleed": true,
///     "BleedIterations": 2,
///     "LimitMemory": true,
///     "Grid": false,
///     "Scale": [ 1 ],
///     "ScaleSuffix": [ "" ],
///     "ScaleResampling": [ "Bicubic" ],
///     "AtlasExtension": ".atlas",
///     "PrettyPrint": true,
///     "LegacyOutput": true
/// }
/// </code>
/// </para>
/// </summary>
[PublicAPI]
[SupportedOSPlatform( "windows" )]
public partial class TexturePacker
{
    public string?                        RootPath         { get; set; }
    public IPacker                        Packer           { get; set; }
    public TexturePackerProgressListener? ProgressListener { get; set; }

    // ========================================================================

    private TexturePackerSettings _settings;
    private ImageProcessor        _imageProcessor;
    private List< InputImage >    _inputImages;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Default Constructor
    /// </summary>
    public TexturePacker()
    {
        Packer          = null!;
        _settings       = null!;
        _imageProcessor = null!;
        _inputImages    = [ ];
    }

    /// <summary>
    /// Creates a new TexturePacker object, using the supplied settings.
    /// </summary>
    /// <param name="settings"> The TexturePackerSettings instance to use. </param>
    public TexturePacker( TexturePackerSettings settings )
        : this( null, settings )
    {
    }

    /// <summary>
    /// Creates a new TexturePacker object.
    /// </summary>
    /// <param name="rootDir"> The root folder of the source textures. </param>
    /// <param name="settings"> The <see cref="TexturePackerSettings"/> to use when packing. </param>
    /// <exception cref="GdxRuntimeException"></exception>
    public TexturePacker( DirectoryInfo? rootDir, TexturePackerSettings settings )
    {
        _inputImages = [ ];
        _settings    = settings;

        if ( settings.PowerOfTwo )
        {
            if ( settings.MaxWidth != MathUtils.NextPowerOfTwo( settings.MaxWidth ) )
            {
                throw new GdxRuntimeException( $"If pot is true, maxWidth must be a power "
                                               + $"of two: {settings.MaxWidth}" );
            }

            if ( settings.MaxHeight != MathUtils.NextPowerOfTwo( settings.MaxHeight ) )
            {
                throw new GdxRuntimeException( $"If pot is true, maxHeight must be a power "
                                               + $"of two: {settings.MaxHeight}" );
            }
        }

        if ( settings.MultipleOfFour )
        {
            if ( ( settings.MaxWidth % 4 ) != 0 )
            {
                throw new GdxRuntimeException( $"If MultipleOfFour is true, maxWidth must be evenly "
                                               + $"divisible by 4: {settings.MaxWidth}" );
            }

            if ( ( settings.MaxHeight % 4 ) != 0 )
            {
                throw new GdxRuntimeException( $"If MultipleOfFour is true, maxHeight must be evenly "
                                               + $"divisible by 4: {settings.MaxHeight}" );
            }
        }

        Packer = settings.Grid ? new GridPacker( settings ) : new MaxRectsPacker( settings );
        
        _imageProcessor = new ImageProcessor( settings );

        SetRootDir( rootDir );
    }

    // ========================================================================

    #region process methods

    /// <summary>
    /// Packs the images in the supplied input folder into a texture atlas.
    /// This method does not perform any modifications to the provided paths. It is
    /// essential to provide the correct paths, optherwise processing will fail.
    /// </summary>
    /// <param name="input"> Directory holding the images to be packed. </param>
    /// <param name="output">
    /// Directory where the pack file and page images will be written. This folder will
    /// be cleared before processing.
    /// </param>
    /// <param name="packFileName"> The name of the pack file. Also used to name the page images. </param>
    public static void Process( string input, string output, string packFileName )
    {
        Process( input, output, packFileName, new TexturePackerSettings() );
    }

    /// <summary>
    /// Packs the images in the supplied input folder into a texture atlas.
    /// This method does not perform any modifications to the provided paths. It is
    /// essential to provide the correct paths, optherwise processing will fail.
    /// </summary>
    /// <param name="settings"> The <see cref="TexturePackerSettings"/> to use. </param>
    /// <param name="input"> Directory holding the images to be packed. </param>
    /// <param name="output">
    /// Directory where the pack file and page images will be written. This folder will
    /// be cleared before processing.
    /// </param>
    /// <param name="packFileName"> The name of the pack file. Also used to name the page images. </param>
    public static void Process( TexturePackerSettings settings, string input, string output, string packFileName )
    {
        Process( input, output, packFileName, settings );
    }

    /// <summary>
    /// Packs the images in the supplied input folder into a texture atlas.
    /// This method does not perform any modifications to the provided paths. It is
    /// essential to provide the correct paths, otherwise processing will fail.
    /// </summary>
    /// <param name="inputFolder"> Directory containing individual images to be packed. </param>
    /// <param name="outputFolder">
    /// Directory where the pack file and page images will be written. This folder will
    /// be cleared before processing.
    /// </param>
    /// <param name="packFileName"> The name of the pack file. Also used to name the page images. </param>
    /// <param name="settings"> The <see cref="TexturePackerSettings"/> to use. </param>
    /// <param name="progressListener"> Could be null. </param>
    public static void Process( string inputFolder,
                                string outputFolder,
                                string packFileName,
                                TexturePackerSettings settings,
                                TexturePackerProgressListener? progressListener = null )
    {
        // All other Process() methods call this one, so this is the best place to do the
        // conversion of the input and output paths ao that they are guaranteed to be
        // pointing to the correct assets folder.
        inputFolder = IOUtils.AssetPath( inputFolder );
        outputFolder = IOUtils.AssetPath( outputFolder );
        
        try
        {
            var processor = new TexturePackerFileProcessor( settings, packFileName, progressListener );
            _ = processor.Process( new DirectoryInfo( inputFolder ), new DirectoryInfo( outputFolder ) );
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( "Error packing images.", ex );
        }
    }

    #endregion process methods

    /// <summary>
    /// Packs processed images into a <see cref="TextureAtlas"/> with the
    /// provided filename. The atlas will be stored in the destination directory.
    /// </summary>
    /// <param name="outputDir"> The destination directory. </param>
    /// <param name="packFileName"> The name for the resulting TextureAtlas. </param>
    public void Pack( DirectoryInfo outputDir, string packFileName )
    {
        if ( packFileName.EndsWith( _settings.AtlasExtension ) )
        {
            packFileName = Path.GetFileNameWithoutExtension( packFileName );
        }

        // Initialise the progress listener, which can be used to report progress
        // in the form of a progress bar or console output.
        ProgressListener ??= new TexturePackerProgressListener();
        ProgressListener.Start( 1 );

        // Performs the process once for each scale factor.
        var n = _settings.Scale.Length;

        for ( var i = 0; i < n; i++ )
        {
            ProgressListener.Start( 1f / n );

            _imageProcessor.Scale = _settings.Scale[ i ];

            if ( ( _settings.ScaleResampling != null )
                 && ( _settings.ScaleResampling.Count > i )
                 && ( _settings.ScaleResampling[ i ] != Resampling.None ) )
            {
                _imageProcessor.Resampling = _settings.ScaleResampling[ i ];
            }

            ProgressListener.Start( 0.35f );
            ProgressListener.Count = 0;
            ProgressListener.Total = _inputImages.Count;

            for ( int ii = 0, nn = _inputImages.Count; ii < nn; ii++, ProgressListener.Count++ )
            {
                var inputImage = _inputImages[ ii ];

                if ( inputImage.FileInfo != null )
                {
                    _imageProcessor.AddImage( inputImage.FileInfo, inputImage.RootPath );
                }
                else
                {
                    _imageProcessor.AddImage( inputImage.Image, inputImage.Name );
                }

                if ( ProgressListener.Update( ii + 1, nn ) )
                {
                    return;
                }
            }

            ProgressListener.End();

            ProgressListener.Start( 0.35f );
            ProgressListener.Count = 0;
            ProgressListener.Total = _imageProcessor.ImageRects.Count;

            var pages = Packer.Pack( ProgressListener, _imageProcessor.ImageRects );

            ProgressListener.End();

            // ---------- Handle writing of the texture atlas ----------

            var scaledPackFileName = _settings.GetScaledPackFileName( packFileName, i );

            ProgressListener.Start( 0.29f );
            ProgressListener.Count = 0;
            ProgressListener.Total = pages.Count;

            WriteImages( outputDir.FullName, scaledPackFileName, pages );

            ProgressListener.End();

            // ---------- End of writing of the texture atlas ----------

            // ---------- Write the .atlas pack file ----------

            ProgressListener.Start( 0.01f );

            try
            {
                WritePackFile( outputDir, scaledPackFileName, pages );
            }
            catch ( IOException ex )
            {
                throw new GdxRuntimeException( "Error writing pack file.", ex );
            }

            _imageProcessor.Clear();
            ProgressListener.End();

            if ( ProgressListener.Update( i + 1, n ) )
            {
                return;
            }

            // ---------- End of writing of the .atlas pack file ----------
        }

        ProgressListener.End();
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="rootDir"></param>
    public void SetRootDir( DirectoryInfo? rootDir )
    {
        if ( rootDir == null )
        {
            RootPath = null;

            return;
        }

        RootPath = Path.GetFullPath( rootDir.FullName );

        if ( !RootPath!.EndsWith( '\\' ) )
        {
            RootPath += "\\";
        }
    }

    /// <summary>
    /// Adds an image to the list of Input Images.
    /// </summary>
    /// <param name="file"></param>
    public void AddImage( FileInfo file )
    {
        var inputImage = new InputImage
        {
            FileInfo = file,
            RootPath = RootPath,
            Name     = file.Name,
        };

        _inputImages.Add( inputImage );
    }

    /// <summary>
    /// Adds an image to the list of Input Images.
    /// </summary>
    /// <param name="image"> The <see cref="Bitmap"/> image.</param>
    /// <param name="name"> The name for this image. </param>
    public void AddImage( Bitmap image, string name )
    {
        var inputImage = new InputImage
        {
            Image = image,
            Name  = name,
        };

        _inputImages.Add( inputImage );
    }
}

// ============================================================================
// ============================================================================