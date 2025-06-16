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

using System.Drawing.Imaging;
using System.Runtime.Versioning;

using LughSharp.Lugh.Graphics.Atlases;
using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;

using Bitmap = System.Drawing.Bitmap;
using Pen = System.Drawing.Pen;
using Image = System.Drawing.Image;

namespace Extensions.Source.Tools.ImagePacker;

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
/// <see cref="TexturePacker.Settings"/> class. Each subdirectory inherits all the settings from
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
    public string?                   RootPath         { get; set; }
    public IPacker                   Packer           { get; set; }
    public AbstractProgressListener? ProgressListener { get; set; }

    // ========================================================================

    private Settings           _settings;
    private ImageProcessor     _imageProcessor;
    private List< InputImage > _inputImages = [ ];

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
    }

    /// <summary>
    /// Creates a new TexturePacker object.
    /// </summary>
    /// <param name="rootDir"> The root folder of the source textures. </param>
    /// <param name="settings"> The <see cref="Settings"/> to use when packing. </param>
    /// <exception cref="GdxRuntimeException"></exception>
    public TexturePacker( DirectoryInfo? rootDir, Settings settings )
    {
        _settings = settings;

        if ( settings.PowerOfTwo )
        {
            if ( settings.MaxWidth != MathUtils.NextPowerOfTwo( settings.MaxWidth ) )
            {
                throw new GdxRuntimeException( $"If pot is true, maxWidth must be a power " +
                                               $"of two: {settings.MaxWidth}" );
            }

            if ( settings.MaxHeight != MathUtils.NextPowerOfTwo( settings.MaxHeight ) )
            {
                throw new GdxRuntimeException( $"If pot is true, maxHeight must be a power " +
                                               $"of two: {settings.MaxHeight}" );
            }
        }

        if ( settings.MultipleOfFour )
        {
            if ( ( settings.MaxWidth % 4 ) != 0 )
            {
                throw new GdxRuntimeException( $"If mod4 is true, maxWidth must be evenly " +
                                               $"divisible by 4: {settings.MaxWidth}" );
            }

            if ( ( settings.MaxHeight % 4 ) != 0 )
            {
                throw new GdxRuntimeException( $"If mod4 is true, maxHeight must be evenly " +
                                               $"divisible by 4: {settings.MaxHeight}" );
            }
        }

        Packer = settings.Grid ? new GridPacker( settings ) : new MaxRectsPacker( settings );

        _imageProcessor = new ImageProcessor( settings );

        SetRootDir( rootDir );
    }

    /// <summary>
    /// </summary>
    /// <param name="settings"></param>
    public TexturePacker( Settings settings )
        : this( null, settings )
    {
    }

    // ========================================================================

    /// <summary>
    /// Packs using defaults settings.
    /// </summary>
    public void Process( string input, string output, string packFileName )
    {
        Process( input, output, packFileName, new Settings() );
    }

    /// <summary>
    /// Packs the images in the supplied input folder into a texture atlas.
    /// This method does not perform any modifications to the provided paths. It is
    /// essential to provide the correct paths, optherwise processing will fail.
    /// </summary>
    /// <param name="inputFolder"> Directory containing individual images to be packed. </param>
    /// <param name="outputFolder"> Directory where the pack file and page images will be written. </param>
    /// <param name="packFileName"> The name of the pack file. Also used to name the page images. </param>
    /// <param name="settings"> The <see cref="TexturePacker.Settings"/> to use. </param>
    /// <param name="progressListener"> Could be null. </param>
    public void Process( string inputFolder,
                         string outputFolder,
                         string packFileName,
                         Settings settings,
                         AbstractProgressListener? progressListener = null )
    {
        Logger.Debug( $"inputFolder: {inputFolder}" );
        Logger.Debug( $"outputFolder: {outputFolder}" );
        Logger.Debug( $"packFileName: {packFileName}" );
        
        try
        {
            var processor = new TexturePackerFileProcessor( settings, packFileName, progressListener );
            _ = processor.Process( new DirectoryInfo( inputFolder ),
                                   new DirectoryInfo( outputFolder ) );
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( "Error packing images.", ex );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="input"></param>
    /// <param name="output"></param>
    /// <param name="packFileName"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public bool ProcessIfModified( string input,
                                   string output,
                                   string packFileName,
                                   Settings? settings = null )
    {
        if ( settings == null )
        {
            settings = new Settings();
        }

        if ( IsModified( input, output, packFileName, settings ) )
        {
            Process( input, output, packFileName, settings );

            return true;
        }

        return false;
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
            RootPath = this.RootPath,
        };

        _inputImages.Add( inputImage );
    }

    /// <summary>
    /// Adds an image to the list of Input Images.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="name"></param>
    public void AddImage( Bitmap image, string name )
    {
        var inputImage = new InputImage
        {
            Image = image,
            Name  = name,
        };

        _inputImages.Add( inputImage );
    }

    /// <summary>
    /// Packs processed images into the a <see cref="TextureAtlas"/> with the
    /// specified filename. The atlas will be stored in the specified directory.
    /// </summary>
    /// <param name="outputDir"> The destination directory. </param>
    /// <param name="packFileName"> The name for the resulting TextureAtlas. </param>
    public void Pack( DirectoryInfo outputDir, string packFileName )
    {
        Logger.Debug( "Packing..." );
        Logger.Debug( $"outputDir: {outputDir.FullName}" );
        Logger.Debug( $"packFileName: {packFileName}" );

        if ( packFileName.EndsWith( _settings.AtlasExtension ) )
        {
            packFileName = packFileName.Substring( 0, packFileName.Length - _settings.AtlasExtension.Length );
        }

        Directory.CreateDirectory( outputDir.FullName );

        ProgressListener ??= new AbstractProgressListenerImpl();
        ProgressListener.Start( 1 );

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
                    if ( inputImage.Image == null )
                    {
                        continue;
                    }

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
            ProgressListener.Start( 0.29f );
            ProgressListener.Count = 0;
            ProgressListener.Total = pages.Count;

            var scaledPackFileName = _settings.GetScaledPackFileName( packFileName, i );

            WriteImages( outputDir.FullName, scaledPackFileName, pages );

            ProgressListener.End();
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
        }

        ProgressListener.End();
    }

    /// <summary>
    /// </summary>
    /// <param name="dst"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="argb"></param>
    private static void Plot( Bitmap dst, int x, int y, System.Drawing.Color argb )
    {
        if ( ( 0 <= x ) && ( x < dst.Width ) && ( 0 <= y ) && ( y < dst.Height ) )
        {
            dst.SetPixel( x, y, argb );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="src"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="dst"></param>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    /// <param name="rotated"></param>
    private static void Copy( Bitmap src, int x, int y, int w, int h, Bitmap dst, int dx, int dy, bool rotated )
    {
        for ( var i = 0; i < w; i++ )
        {
            for ( var j = 0; j < h; j++ )
            {
                if ( rotated )
                {
                    Plot( dst, dx + j, ( dy + w ) - i - 1, src.GetPixel( x + i, y + j ) );
                }
                else
                {
                    Plot( dst, dx + i, dy + j, src.GetPixel( x + i, y + j ) );
                }
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="outputDir"></param>
    /// <param name="scaledPackFileName"></param>
    /// <param name="pages"></param>
    /// <exception cref="GdxRuntimeException"></exception>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="Exception"></exception>
    private void WriteImages( string outputDir, string scaledPackFileName, List< Page > pages )
    {
        Logger.Debug( "Writing images..." );
        Logger.Debug( $"outputDir: {outputDir}" );
        Logger.Debug( $"scaledPackFileName: {scaledPackFileName}" );
        Logger.Debug( $"pages.Count: {pages.Count}" );

        var packFileNoExt = Path.Combine( outputDir, scaledPackFileName );
        var packDir       = Path.GetDirectoryName( packFileNoExt );
        var imageName     = Path.GetFileName( packFileNoExt );

        Logger.Debug( $"packFileNoExt: {packFileNoExt}" );
        Logger.Debug( $"packDir: {packDir}" );
        Logger.Debug( $"imageName: {imageName}" );

        if ( packDir == null )
        {
            throw new GdxRuntimeException( "Error creating pack directory." );
        }

        var fileIndex = 1;

        for ( var p = 0; p < pages.Count; p++ )
        {
            var page = pages[ p ];

            var width  = page.Width;
            var height = page.Height;

            if ( _settings.EdgePadding )
            {
                var edgePadX = _settings.PaddingX;
                var edgePadY = _settings.PaddingY;

                if ( _settings.DuplicatePadding )
                {
                    edgePadX /= 2;
                    edgePadY /= 2;
                }

                page.X =  edgePadX;
                page.Y =  edgePadY;
                width  += edgePadX * 2;
                height += edgePadY * 2;
            }

            if ( _settings.PowerOfTwo )
            {
                width  = MathUtils.NextPowerOfTwo( width );
                height = MathUtils.NextPowerOfTwo( height );
            }

            if ( _settings.MultipleOfFour )
            {
                width  = ( width % 4 ) == 0 ? width : ( width + 4 ) - ( width % 4 );
                height = ( height % 4 ) == 0 ? height : ( height + 4 ) - ( height % 4 );
            }

            width            = Math.Max( _settings.MinWidth, width );
            height           = Math.Max( _settings.MinHeight, height );
            page.ImageWidth  = width;
            page.ImageHeight = height;

            string outputFile;

            Logger.Debug( $"width: {width}" );
            Logger.Debug( $"height: {height}" );

            while ( true )
            {
                var name = imageName;

                if ( fileIndex > 1 )
                {
                    var last = name[ name.Length - 1 ];

                    if ( char.IsDigit( last )
                         || ( ( name.Length > 3 )
                              && ( last == 'x' )
                              && char.IsDigit( name[ name.Length - 2 ] ) ) )
                    {
                        name += "-";
                    }

                    name += fileIndex;
                }

                fileIndex++;

                outputFile = Path.Combine( packDir, name + "." + _settings.OutputFormat );

                Logger.Debug( $"outputFile: {outputFile}" );

                if ( !File.Exists( outputFile ) )
                {
                    break;
                }
            }

            // Create output directories
            Directory.CreateDirectory( Path.GetDirectoryName( outputFile )
                                       ?? throw new GdxRuntimeException( "Error creating output directory" ) );

            page.ImageName = Path.GetFileName( outputFile );

            var canvas = new Bitmap( width, height, PixmapFormat.ToPixelFormat( _settings.Format ) );
            var g      = System.Drawing.Graphics.FromImage( canvas );

            if ( !_settings.Silent )
            {
                Logger.Debug( $"Writing {canvas.Width}x{canvas.Height}: {outputFile}" );
            }

            if ( page.OutputRects == null )
            {
                throw new NullReferenceException( "OutputRects for page is null" );
            }

            ProgressListener?.Start( 1f / pages.Count );

            for ( var r = 0; r < page.OutputRects.Count; r++ )
            {
                var rect = page.OutputRects[ r ];

                using ( var image = rect.GetImage( _imageProcessor ) )
                {
                    var iw    = image.Width;
                    var ih    = image.Height;
                    var rectX = page.X + rect.X;
                    var rectY = ( page.Y + page.Height ) - rect.Y - ( rect.Height - _settings.PaddingY );

                    if ( _settings.DuplicatePadding )
                    {
                        var amountX = _settings.PaddingX / 2;
                        var amountY = _settings.PaddingY / 2;

                        if ( rect.Rotated )
                        {
                            // Copy corner pixels to fill the corners of the padding.
                            for ( var i = 1; i <= amountX; i++ )
                            {
                                for ( var j = 1; j <= amountY; j++ )
                                {
                                    Plot( canvas, rectX - j, ( ( rectY + iw ) - 1 ) + i, image.GetPixel( 0, 0 ) );
                                    Plot( canvas, ( ( rectX + ih ) - 1 ) + j, ( ( rectY + iw ) - 1 ) + i, image.GetPixel( 0, ih - 1 ) );
                                    Plot( canvas, rectX - j, rectY - i, image.GetPixel( iw - 1, 0 ) );
                                    Plot( canvas, ( ( rectX + ih ) - 1 ) + j, rectY - i, image.GetPixel( iw - 1, ih - 1 ) );
                                }
                            }

                            // Copy edge pixels into padding.
                            for ( var i = 1; i <= amountY; i++ )
                            {
                                for ( var j = 0; j < iw; j++ )
                                {
                                    Plot( canvas, rectX - i, ( rectY + iw ) - 1 - j, image.GetPixel( j, 0 ) );
                                    Plot( canvas, ( ( rectX + ih ) - 1 ) + i, ( rectY + iw ) - 1 - j, image.GetPixel( j, ih - 1 ) );
                                }
                            }

                            for ( var i = 1; i <= amountX; i++ )
                            {
                                for ( var j = 0; j < ih; j++ )
                                {
                                    Plot( canvas, rectX + j, rectY - i, image.GetPixel( iw - 1, j ) );
                                    Plot( canvas, rectX + j, ( ( rectY + iw ) - 1 ) + i, image.GetPixel( 0, j ) );
                                }
                            }
                        }
                        else
                        {
                            // Copy corner pixels to fill the corners of the padding.
                            for ( var i = 1; i <= amountX; i++ )
                            {
                                for ( var j = 1; j <= amountY; j++ )
                                {
                                    Plot( canvas, rectX - i, rectY - j, image.GetPixel( 0, 0 ) );
                                    Plot( canvas, rectX - i, ( ( rectY + ih ) - 1 ) + j, image.GetPixel( 0, ih - 1 ) );
                                    Plot( canvas, ( ( rectX + iw ) - 1 ) + i, rectY - j, image.GetPixel( iw - 1, 0 ) );
                                    Plot( canvas, ( ( rectX + iw ) - 1 ) + i, ( ( rectY + ih ) - 1 ) + j,
                                          image.GetPixel( iw - 1, ih - 1 ) );
                                }
                            }

                            // Copy edge pixels into padding.
                            for ( var i = 1; i <= amountY; i++ )
                            {
                                Copy( image, 0, 0, iw, 1, canvas, rectX, rectY - i, rect.Rotated );
                                Copy( image, 0, ih - 1, iw, 1, canvas, rectX, ( ( rectY + ih ) - 1 ) + i, rect.Rotated );
                            }

                            for ( var i = 1; i <= amountX; i++ )
                            {
                                Copy( image, 0, 0, 1, ih, canvas, rectX - i, rectY, rect.Rotated );
                                Copy( image, iw - 1, 0, 1, ih, canvas, ( ( rectX + iw ) - 1 ) + i, rectY, rect.Rotated );
                            }
                        }
                    }

                    Copy( image, 0, 0, iw, ih, canvas, rectX, rectY, rect.Rotated );

                    if ( _settings.Debug )
                    {
                        using ( var pen = new Pen( System.Drawing.Color.Magenta ) )
                        {
                            g.DrawRectangle( pen, rectX, rectY, rect.Width - _settings.PaddingX - 1,
                                             rect.Height - _settings.PaddingY - 1 );
                        }
                    }

                    if ( ProgressListener!.Update( r + 1, page.OutputRects.Count ) )
                    {
                        return;
                    }
                }
            }

            ProgressListener?.End();

            if ( _settings is { Bleed: true, PremultiplyAlpha: false }
                 && !( _settings.OutputFormat.Equals( "jpg", StringComparison.OrdinalIgnoreCase ) ||
                       _settings.OutputFormat.Equals( "jpeg", StringComparison.OrdinalIgnoreCase ) ) )
            {
                canvas = new ColorBleedEffect().ProcessImage( canvas, _settings.BleedIterations );

                g.Dispose(); // Dispose of previous Graphics object
                g = System.Drawing.Graphics.FromImage( canvas );
            }

            if ( _settings.Debug )
            {
                using ( var pen = new Pen( System.Drawing.Color.Magenta ) )
                {
                    g.DrawRectangle( pen, 0, 0, width - 1, height - 1 );
                }
            }

            try
            {
                if ( _settings.OutputFormat.Equals( "jpg", StringComparison.OrdinalIgnoreCase ) ||
                     _settings.OutputFormat.Equals( "jpeg", StringComparison.OrdinalIgnoreCase ) )
                {
                    using ( var newImage = new Bitmap( canvas.Width, canvas.Height, PixelFormat.Format24bppRgb ) )
                    using ( var newGraphics = System.Drawing.Graphics.FromImage( newImage ) )
                    {
                        newGraphics.DrawImage( canvas, 0, 0 );

                        var jpgEncoder          = GetEncoder( ImageFormat.Jpeg );
                        var myEncoder           = Encoder.Quality;
                        var myEncoderParameters = new EncoderParameters( 1 );
                        var myEncoderParameter  = new EncoderParameter( myEncoder, ( long )( _settings.JpegQuality * 100 ) );

                        myEncoderParameters.Param[ 0 ] = myEncoderParameter;
                        newImage.Save( outputFile, jpgEncoder, myEncoderParameters );
                    }
                }
                else
                {
                    if ( _settings.PremultiplyAlpha )
                    {
                        // Premultiply alpha (if needed)
                        for ( var y = 0; y < canvas.Height; y++ )
                        {
                            for ( var x = 0; x < canvas.Width; x++ )
                            {
                                var color = canvas.GetPixel( x, y );
                                var alpha = color.A / 255f;
                                var red   = ( int )( color.R * alpha );
                                var green = ( int )( color.G * alpha );
                                var blue  = ( int )( color.B * alpha );

                                canvas.SetPixel( x, y, System.Drawing.Color.FromArgb( color.A, red, green, blue ) );
                            }
                        }
                    }

                    canvas.Save( outputFile, ImageFormat.Png );
                }
            }
            catch ( IOException ex )
            {
                throw new Exception( "Error writing file: " + outputFile, ex );
            }

            if ( ProgressListener!.Update( p + 1, pages.Count ) )
            {
                return;
            }

            ProgressListener.Count++;
        }
    }

    private void WritePackFile( DirectoryInfo outputDir, string scaledPackFileName, List< Page > pages )
    {
        var packFile = new FileInfo( Path.Combine( outputDir.FullName, scaledPackFileName + _settings.AtlasExtension ) );
        var packDir  = packFile.Directory;

        packDir?.Create();

        if ( packFile.Exists )
        {
            // Make sure there aren't duplicate names.
            var textureAtlasData = new TextureAtlasData( packFile, flip: _settings.FlattenPaths );

            foreach ( var page in pages )
            {
                ArgumentNullException.ThrowIfNull( page.OutputRects );

                foreach ( var rect in page.OutputRects )
                {
                    var rectName = Rect.GetAtlasName( rect.Name, _settings.FlattenPaths );

                    foreach ( var region in textureAtlasData.Regions )
                    {
                        ArgumentNullException.ThrowIfNull( region.Name );

                        if ( region.Name.Equals( rectName ) )
                        {
                            throw new GdxRuntimeException( $"A region with the name \"{rectName}\" has already been packed: {rect.Name}" );
                        }
                    }
                }
            }
        }

//        var tab   = "";
//        var colon = ":";
//        var comma = ",";

//        if ( _settings.PrettyPrint )
//        {
//            tab   = "\t";
//            colon = ": ";
//            comma = ", ";
//        }

        var appending = packFile.Exists;

        using ( var writer = new StreamWriter( packFile.FullName, appending, System.Text.Encoding.UTF8 ) )
        {
            for ( var i = 0; i < pages.Count; i++ )
            {
                var page = pages[ i ];

                if ( page.OutputRects == null )
                {
                    throw new NullReferenceException();
                }

                if ( _settings.LegacyOutput )
                {
                    WritePageLegacy( writer, page );
                }
                else
                {
                    if ( ( i != 0 ) || appending )
                    {
                        writer.WriteLine();
                    }

                    WritePage( writer, appending, page );
                }

                page.OutputRects.Sort();

                foreach ( var rect in page.OutputRects )
                {
                    if ( ( rect.Name == null ) || ( rect.Name.Length == 0 ) )
                    {
                        throw new GdxRuntimeException( "rect.Name must not be null or empty" );
                    }

                    if ( _settings.LegacyOutput )
                    {
                        WriteRectLegacy( writer, page, rect, rect.Name );
                    }
                    else
                    {
                        WriteRect( writer, page, rect, rect.Name );
                    }

                    var aliases = new List< Alias >( rect.Aliases );

                    aliases.Sort();

                    foreach ( var alias in aliases )
                    {
                        if ( ( alias.Name == null ) || ( alias.Name.Length == 0 ) )
                        {
                            throw new GdxRuntimeException( "alias.Name must not be null or empty" );
                        }

                        var aliasRect = new Rect();
                        aliasRect.Set( rect );
                        alias.Apply( aliasRect );

                        if ( _settings.LegacyOutput )
                        {
                            WriteRectLegacy( writer, page, aliasRect, alias.Name );
                        }
                        else
                        {
                            WriteRect( writer, page, aliasRect, alias.Name );
                        }
                    }
                }
            }
        }
    }

    private void WritePage( TextWriter writer, bool appending, Page page )
    {
        var tab   = "";
        var colon = ":";
        var comma = ",";

        if ( _settings.PrettyPrint )
        {
            tab   = "\t";
            colon = ": ";
            comma = ", ";
        }

        writer.WriteLine( page.ImageName );
        writer.WriteLine( $"{tab}size{colon}{page.ImageWidth}{comma}{page.ImageHeight}" );

        if ( PixmapFormat.ToPixelFormat( _settings.Format ) != PixelFormat.Format32bppArgb )
        {
            writer.WriteLine( $"{tab}format{colon}{_settings.Format}" );
        }

        if ( ( _settings.FilterMin != Texture.TextureFilter.Nearest ) || ( _settings.FilterMag != Texture.TextureFilter.Nearest ) )
        {
            writer.WriteLine( $"{tab}filter{colon}{_settings.FilterMin}{comma}{_settings.FilterMag}" );
        }

        var repeatValue = GetRepeatValue(); // Pass settings to GetRepeatValue

        if ( repeatValue != null )
        {
            writer.WriteLine( $"{tab}repeat{colon}{repeatValue}" );
        }

        if ( _settings.PremultiplyAlpha )
        {
            writer.WriteLine( $"{tab}pma{colon}true" );
        }
    }

    private void WriteRect( TextWriter writer, Page page, Rect rect, string name )
    {
        var tab   = "";
        var colon = ":";
        var comma = ",";

        if ( _settings.PrettyPrint )
        {
            tab   = "\t";
            colon = ": ";
            comma = ", ";
        }

        writer.WriteLine( Rect.GetAtlasName( name, _settings.FlattenPaths ) );

        if ( rect.Index != -1 )
        {
            writer.WriteLine( $"{tab}index{colon}{rect.Index}" );
        }

        writer.WriteLine( $"{tab}bounds{colon}{page.X + rect.X}{comma}{( page.Y + page.Height ) - rect.Y - ( rect.Height - _settings.PaddingY )}{comma}{rect.RegionWidth}{comma}{rect.RegionHeight}" );

        var offsetY = rect.OriginalHeight - rect.RegionHeight - rect.OffsetY;

        if ( ( rect.OffsetX != 0 ) || ( offsetY != 0 ) || ( rect.OriginalWidth != rect.RegionWidth ) ||
             ( rect.OriginalHeight != rect.RegionHeight ) )
        {
            writer.WriteLine( $"{tab}offsets{colon}{rect.OffsetX}{comma}{offsetY}{comma}{rect.OriginalWidth}{comma}{rect.OriginalHeight}" );
        }

        if ( rect.Rotated )
        {
            writer.WriteLine( $"{tab}rotate{colon}{rect.Rotated}" );
        }

        if ( rect.Splits != null )
        {
            writer.WriteLine( $"{tab}split{colon}{rect.Splits[ 0 ]}{comma}{rect.Splits[ 1 ]}{comma}{rect.Splits[ 2 ]}{comma}{rect.Splits[ 3 ]}" );
        }

        if ( rect.Pads != null )
        {
            if ( rect.Splits == null )
            {
                writer.WriteLine( $"{tab}split{colon}0{comma}0{comma}0{comma}0" );
            }

            writer.WriteLine( $"{tab}pad{colon}{rect.Pads[ 0 ]}{comma}{rect.Pads[ 1 ]}{comma}{rect.Pads[ 2 ]}{comma}{rect.Pads[ 3 ]}" );
        }
    }

    private void WritePageLegacy( TextWriter writer, Page page )
    {
        writer.WriteLine();
        writer.WriteLine( page.ImageName );
        writer.WriteLine( $"size: {page.ImageWidth}, {page.ImageHeight}" );
        writer.WriteLine( $"format: {_settings.Format}" );
        writer.WriteLine( $"filter: {_settings.FilterMin}, {_settings.FilterMag}" );

        var repeatValue = GetRepeatValue();
        writer.WriteLine( $"repeat: {repeatValue ?? "none"}" );
    }

    private void WriteRectLegacy( TextWriter writer, Page page, Rect rect, string name )
    {
        writer.WriteLine( Rect.GetAtlasName( name, _settings.FlattenPaths ) );
        writer.WriteLine( $"  rotate: {rect.Rotated}" );
        writer.WriteLine( $"  xy: {page.X + rect.X}, {( page.Y + page.Height ) - rect.Y - ( rect.Height - _settings.PaddingY )}" );
        writer.WriteLine( $"  size: {rect.RegionWidth}, {rect.RegionHeight}" );

        if ( rect.Splits != null )
        {
            writer.WriteLine( $"  split: {rect.Splits[ 0 ]}, {rect.Splits[ 1 ]}, {rect.Splits[ 2 ]}, {rect.Splits[ 3 ]}" );
        }

        if ( rect.Pads != null )
        {
            if ( rect.Splits == null )
            {
                writer.WriteLine( "  split: 0, 0, 0, 0" );
            }

            writer.WriteLine( $"  pad: {rect.Pads[ 0 ]}, {rect.Pads[ 1 ]}, {rect.Pads[ 2 ]}, {rect.Pads[ 3 ]}" );
        }

        writer.WriteLine( $"  orig: {rect.OriginalWidth}, {rect.OriginalHeight}" );
        writer.WriteLine( $"  offset: {rect.OffsetX}, {rect.OriginalHeight - rect.RegionHeight - rect.OffsetY}" );
        writer.WriteLine( $"  index: {rect.Index}" );
    }

    private string? GetRepeatValue()
    {
        return _settings switch
        {
            { WrapX: Texture.TextureWrap.Repeat, WrapY     : Texture.TextureWrap.Repeat }      => "xy",
            { WrapX: Texture.TextureWrap.Repeat, WrapY     : Texture.TextureWrap.ClampToEdge } => "x",
            { WrapX: Texture.TextureWrap.ClampToEdge, WrapY: Texture.TextureWrap.Repeat }      => "y",

            var _ => null,
        };
    }

    private PixelFormat GetPixelFormat( PixelType.Format format )
    {
        return format switch
        {
            PixelType.Format.RGBA8888
                or PixelType.Format.RGBA4444 => PixelFormat.Format32bppArgb,

            PixelType.Format.RGB565
                or PixelType.Format.RGB888 => PixelFormat.Format32bppRgb,

            PixelType.Format.Alpha => PixelFormat.Alpha,

            var _ => throw new GdxRuntimeException( $"Unsupported format: {_settings.Format}" ),
        };
    }

    /// <summary>
    /// </summary>
    /// <param name="format"></param>
    /// <returns></returns>
    /// <exception cref="GdxRuntimeException"></exception>
    private static ImageCodecInfo GetEncoder( ImageFormat format )
    {
        var codecs = ImageCodecInfo.GetImageEncoders();

        foreach ( var codec in codecs )
        {
            if ( codec.FormatID == format.Guid )
            {
                return codec;
            }
        }

        throw new GdxRuntimeException( $"Decode for ImageFormat {format} not found" );
    }

    /// <summary>
    /// Returns true if the output file does not yet exist or its last modification date
    /// is before the last modification date of the input file
    /// </summary>
    public static bool IsModified( string input, string output, string packFileName, Settings settings )
    {
        var packFullFileName = output;

        if ( !packFullFileName.EndsWith( '/' ) )
        {
            packFullFileName += "/";
        }

        packFullFileName += packFileName;
        packFullFileName += settings.AtlasExtension;

        // Check against the only file we know for sure will exist and will
        // be changed if any asset changes the atlas file.
        var outputFile = new FileInfo( packFullFileName );

        if ( !File.Exists( outputFile.FullName ) )
        {
            return true;
        }

        var inputFile = new FileInfo( input );

        if ( !File.Exists( inputFile.FullName ) )
        {
            throw new ArgumentException( $"TexturePacker#IsModified: Input file does not exist: {inputFile.Name}" );
        }

        return IsModified( inputFile.FullName, outputFile.LastWriteTimeUtc.Ticks / 10000 );
    }

    /// <summary>
    /// Returns true if the output file does not yet exist or its last modification date
    /// is before the last modification date of the input file
    /// </summary>
    /// <param name="filePath"> Output file path. </param>
    /// <param name="lastModified"></param>
    /// <returns></returns>
    public static bool IsModified( string filePath, long lastModified )
    {
        try
        {
            var fileInfo = new FileInfo( filePath );

            if ( fileInfo.LastWriteTimeUtc.Ticks > lastModified )
            {
                return true;
            }

            if ( fileInfo.Attributes.HasFlag( FileAttributes.Directory ) )
            {
                var children = Directory.GetFiles( filePath, "*", SearchOption.AllDirectories );

                if ( children is { Length: > 0 } )
                {
                    foreach ( var child in children )
                    {
                        if ( IsModified( child, lastModified ) )
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( $"Error checking modification: {ex.Message}" );
        }
    }

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
        RootPath = RootPath!.Replace( '\\', '/' );

        if ( !RootPath!.EndsWith( '/' ) )
        {
            RootPath += "/";
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Page
    {
        public string?       ImageName      { get; set; } = "";
        public List< Rect >? OutputRects    { get; set; } = [ ];
        public List< Rect >? RemainingRects { get; set; } = [ ];
        public float         Occupancy      { get; set; } = 0;
        public int           X              { get; set; } = 0;
        public int           Y              { get; set; } = 0;
        public int           Width          { get; set; } = 0;
        public int           Height         { get; set; } = 0;
        public int           ImageWidth     { get; set; } = 0;
        public int           ImageHeight    { get; set; } = 0;

        public void Debug()
        {
            Logger.Debug( $"ImageName: {ImageName}" );
            Logger.Debug( $"OutputRects: {OutputRects?.Count}" );
            Logger.Debug( $"RemainingRects: {RemainingRects?.Count}" );
            Logger.Debug( $"Occupancy: {Occupancy}" );
            Logger.Debug( $"X: {X}" );
            Logger.Debug( $"Y: {Y}" );
            Logger.Debug( $"Width: {Width}" );
            Logger.Debug( $"Height: {Height}" );
            Logger.Debug( $"ImageWidth: {ImageWidth}" );
            Logger.Debug( $"ImageHeight: {ImageHeight}" );
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Alias : IComparable< Alias >
    {
        public int     Index          = 0;
        public int     OffsetX        = 0;
        public int     OffsetY        = 0;
        public int     OriginalHeight = 0;
        public int     OriginalWidth  = 0;
        public string? Name;
        public int[]?  Pads;
        public int[]?  Splits;

        public Alias( Rect rect )
        {
            Index          = rect.Index;
            Name           = rect.Name;
            OffsetX        = rect.OffsetX;
            OffsetY        = rect.OffsetY;
            OriginalHeight = rect.OriginalHeight;
            OriginalWidth  = rect.OriginalWidth;
            Pads           = rect.Pads;
            Splits         = rect.Splits;
        }

        public void Apply( Rect rect )
        {
            rect.Name           = Name;
            rect.Index          = Index;
            rect.Splits         = Splits;
            rect.Pads           = Pads;
            rect.OffsetX        = OffsetX;
            rect.OffsetY        = OffsetY;
            rect.OriginalWidth  = OriginalWidth;
            rect.OriginalHeight = OriginalHeight;
        }

        public int CompareTo( Alias? o )
        {
            return string.Compare( Name, o?.Name, StringComparison.Ordinal );
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Rect : IComparable< Rect >
    {
        public int  Score1         = 0;
        public int  Score2         = 0;
        public bool Rotated        = false;
        public bool CanRotate      = true;
        public int  X              = 0;
        public int  Y              = 0;
        public int  Width          = 0; // Portion of page taken by this region, including padding.
        public int  Height         = 0; // Portion of page taken by this region, including padding.
        public int  Index          = 0;
        public int  OffsetX        = 0;
        public int  OffsetY        = 0;
        public int  OriginalHeight = 0;
        public int  OriginalWidth  = 0;
        public int  RegionHeight   = 0;
        public int  RegionWidth    = 0;

        public string?       Name    = string.Empty;
        public List< Alias > Aliases = [ ];
        public int[]?        Splits  = null;
        public int[]?        Pads    = null;

        // ====================================================================

        private Bitmap?  _bufferedImage = null;
        private FileInfo _file          = null!;
        private bool     _isPatch       = false;

        // ====================================================================

        public Rect()
        {
        }

        public Rect( Rect rect )
        {
            X      = rect.X;
            Y      = rect.Y;
            Width  = rect.Width;
            Height = rect.Height;
        }

        public int CompareTo( Rect? o )
        {
            return string.Compare( Name, o?.Name, StringComparison.Ordinal );
        }

        public Rect( Bitmap source, int left, int top, int newWidth, int newHeight, bool isPatch )
        {
            _bufferedImage = new Bitmap( newWidth, newHeight, source.PixelFormat );
            _isPatch       = isPatch;

            OffsetX        = left;
            OffsetY        = top;
            RegionWidth    = newWidth;
            RegionHeight   = newHeight;
            OriginalWidth  = source.Width;
            OriginalHeight = source.Height;
            Width          = newWidth;
            Height         = newHeight;
        }

        public void UnloadImage( FileInfo fileInfo )
        {
            _file = fileInfo;

            _bufferedImage = null;
        }

        public Bitmap GetImage( ImageProcessor? imageProcessor )
        {
            ArgumentNullException.ThrowIfNull( imageProcessor );

            if ( _bufferedImage != null )
            {
                return _bufferedImage;
            }

            Bitmap image;

            try
            {
                image = ( Bitmap )Image.FromFile( _file.Name );
            }
            catch ( IOException ex )
            {
                throw new GdxRuntimeException( $"Error reading image: {_file}", ex );
            }

            if ( image == null )
            {
                throw new GdxRuntimeException( $"Unable to read image: {_file}" );
            }

            var name = Name;

            if ( _isPatch )
            {
                name += ".9";
            }

            var rect = imageProcessor.ProcessImage( image, name );

            if ( rect == null )
            {
                throw new GdxRuntimeException( "ProcessImage returned null" );
            }

            return rect.GetImage( null );
        }

        public void Set( Rect rect )
        {
            Name           = rect.Name;
            _bufferedImage = rect._bufferedImage;
            OffsetX        = rect.OffsetX;
            OffsetY        = rect.OffsetY;
            RegionWidth    = rect.RegionWidth;
            RegionHeight   = rect.RegionHeight;
            OriginalWidth  = rect.OriginalWidth;
            OriginalHeight = rect.OriginalHeight;
            X              = rect.X;
            Y              = rect.Y;
            Width          = rect.Width;
            Height         = rect.Height;
            Index          = rect.Index;
            Rotated        = rect.Rotated;
            Aliases        = rect.Aliases;
            Splits         = rect.Splits;
            Pads           = rect.Pads;
            CanRotate      = rect.CanRotate;
            Score1         = rect.Score1;
            Score2         = rect.Score2;
            _file          = rect._file;
            _isPatch       = rect._isPatch;
        }

        public static string GetAtlasName( string? name, bool flattenPaths )
        {
            ArgumentNullException.ThrowIfNull( name );

            return flattenPaths ? new FileInfo( name ).Name : name;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            const int PRIME = 31;

            var result = PRIME + NumberUtils.FloatToRawIntBits( 10f );
            result = ( PRIME * result ) + NumberUtils.FloatToRawIntBits( 20f );
            result = ( PRIME * result ) + NumberUtils.FloatToRawIntBits( 30f );
            result = ( PRIME * result ) + NumberUtils.FloatToRawIntBits( 40f );

            return result;
        }

        /// <inheritdoc />
        public override bool Equals( object? obj )
        {
            if ( this == obj )
            {
                return true;
            }

            if ( obj == null )
            {
                return false;
            }

            if ( GetType() != obj.GetType() )
            {
                return false;
            }

            var other = ( Rect )obj;

            if ( Name == null )
            {
                if ( other.Name != null )
                {
                    return false;
                }
            }
            else
            {
                if ( !Name.Equals( other.Name ) )
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Name}{( Index != -1 ? $"_{Index}" : "" )}[{X},{Y} {Width}x{Height}]";
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public interface IPacker
    {
        public List< Page > Pack( List< Rect > inputRects );
        public List< Page > Pack( AbstractProgressListener progressListener, List< Rect > inputRects );
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class InputImage
    {
        public FileInfo? FileInfo { get; set; } = null;
        public string?   RootPath { get; set; } = null;
        public string?   Name     { get; set; } = null;
        public Bitmap?   Image    { get; set; } = null;
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class AbstractProgressListenerImpl : AbstractProgressListener
    {
        /// <inheritdoc />
        protected override void Progress( float progress )
        {
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public abstract class AbstractProgressListener
    {
        public int  Count    { get; set; }
        public int  Total    { get; set; }
        public bool Canceled { get; set; }

        // ====================================================================

        private          float         _scale = 1;
        private          float         _lastUpdate;
        private readonly List< float > _portions = new( 8 );
        private          string        _message  = "";

        // ====================================================================

        public void Start( float portion )
        {
            if ( portion == 0 )
            {
                throw new ArgumentException( "portion cannot be 0." );
            }

            _portions.Add( _lastUpdate );
            _portions.Add( _scale * portion );
            _portions.Add( _scale );

            _scale *= portion;
        }

        public bool Update( int count, int total )
        {
            Update( total == 0 ? 0 : count / ( float )total );

            return Canceled;
        }

        public void Update( float percent )
        {
            _lastUpdate = _portions[ _portions.Count - 3 ] + ( _portions[ _portions.Count - 2 ] * percent );

            Progress( _lastUpdate );
        }

        public void End()
        {
            _scale = _portions.Pop();

            var portion = _portions.Pop();

            _lastUpdate = _portions.Pop() + portion;

            Progress( _lastUpdate );
        }

        public void Reset()
        {
            _scale  = 1;
            Message = "";
            Count   = 0;
            Total   = 0;

            Progress( 0 );
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;

                Progress( _lastUpdate );
            }
        }

        protected abstract void Progress( float progress );
    }

    // ========================================================================
    // ========================================================================
}