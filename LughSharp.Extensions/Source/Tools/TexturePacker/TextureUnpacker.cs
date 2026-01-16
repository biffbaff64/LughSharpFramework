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

using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.Versioning;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;
using Rectangle = System.Drawing.Rectangle;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class TextureUnpacker
{
    /// <summary>
    /// If true, enables informational messages during execution.
    /// </summary>
    public bool Quiet { get; set; }

    // ========================================================================

    private const string DEFAULT_OUTPUT_PATH  = "output";
    private const string DEFAULT_OUTPUT_TYPE  = "png";
    private const string HELP                 = "Usage: atlasFile [imageDir] [outputDir]";
    private const string ATLAS_FILE_EXTENSION = ".atlas";
    private const int    NINEPATCH_PADDING    = 1;

    // ========================================================================

    /// <summary>
    /// Splits an atlas into seperate image and ninepatch files.
    /// </summary>
    public void SplitAtlas( TextureAtlasData atlas, string outputDir )
    {
        // create the output directory if it did not exist yet
        var outputDirInfo = new DirectoryInfo( outputDir );

        if ( !outputDirInfo.Exists )
        {
            outputDirInfo.Create();

            if ( !Quiet )
            {
                Logger.Debug( $"Creating directory: {outputDirInfo.FullName}" );
            }
        }

        foreach ( var page in atlas.Pages )
        {
            // load the image file belonging to this page as a Bitmap
            var fileInfo = new FileInfo( page.TextureFile!.FullName );

            if ( !fileInfo.Exists )
            {
                throw new Exception( $"Unable to find atlas image: {fileInfo.FullName}" );
            }

            Bitmap img = null!;

            try
            {
                img = new Bitmap( fileInfo.FullName );
            }
            catch ( Exception e )
            {
                PrintExceptionAndExit( e );
            }

            foreach ( var region in atlas.Regions )
            {
                if ( !Quiet )
                {
                    Console.WriteLine( $"Processing image for {region.Name}: x[{region.Left}] " +
                                       $"y[{region.Top}] w[{region.Width}] h[{region.Height}], " +
                                       $"rotate[{region.Rotate}]" );
                }

                // check if the page this region is in is currently loaded in a Bitmap
                if ( region.Page == page )
                {
                    Bitmap splitImage;
                    string extension;

                    // check if the region is a ninepatch or a normal image and delegate accordingly
                    if ( region.FindValue( "split" ) == null )
                    {
                        splitImage = ExtractImage( img, region, outputDirInfo, 0 );

                        if ( ( region.Width != region.OriginalWidth ) || ( region.Height != region.OriginalHeight ) )
                        {
                            var originalImg =
                                new Bitmap( region.OriginalWidth, region.OriginalHeight, img.PixelFormat );

                            using ( var g = Graphics.FromImage( originalImg ) )
                            {
                                g.InterpolationMode = InterpolationMode.Bilinear;
                                g.DrawImage( splitImage,
                                             ( int )region.OffsetX,
                                             ( int )( region.OriginalHeight - region.Height - region.OffsetY ) );
                            }

                            splitImage = originalImg;
                        }

                        extension = DEFAULT_OUTPUT_TYPE;
                    }
                    else
                    {
                        splitImage = ExtractNinePatch( img, region, outputDirInfo );
                        extension  = $"9.{DEFAULT_OUTPUT_TYPE}";
                    }

                    // check if the parent directories of this image file exist and create them if not
                    var imgOutputFilePath = Path.Combine( outputDirInfo.FullName,
                                                          $"{region.Index switch
                                                             { -1 => region.Name,
                                                                 var _ => $"{region.Name}_{region.Index}" }}.{extension}" );
                    var imgDirInfo = new FileInfo( imgOutputFilePath ).Directory;

                    if ( imgDirInfo is { Exists: false } )
                    {
                        if ( !Quiet )
                        {
                            Console.WriteLine( $"Creating directory: {imgDirInfo.FullName}" );
                        }

                        imgDirInfo.Create();
                    }

                    // save the image
                    try
                    {
                        splitImage.Save( imgOutputFilePath, GetImageFormat( DEFAULT_OUTPUT_TYPE ) );
                    }
                    catch ( Exception e )
                    {
                        PrintExceptionAndExit( e );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Entry point for the TextureUnpacker application.
    /// Parses command-line arguments and performs texture atlas unpacking.
    /// </summary>
    /// <param name="args">
    /// Command-line arguments including atlas file path, optional image directory,
    /// and optional output directory.
    /// </param>
    /// <exception cref="RuntimeException">Thrown if the specified atlas file is not found.</exception>
    public static void Entry( string[] args )
    {
        var unpacker = new TextureUnpacker();

        string? atlasFile = null;
        string? imageDir  = null;
        string? outputDir = null;

        // parse the arguments and display the help text if there
        // is a problem with the command line arguments
        if ( unpacker.ParseArguments( args ) == 0 )
        {
            Logger.Debug( HELP );

            return;
        }

        if ( unpacker.ParseArguments( args ) == 1 )
        {
            atlasFile = args[ 0 ];
        }
        else if ( unpacker.ParseArguments( args ) == 2 )
        {
            atlasFile = args[ 0 ];
            imageDir  = args[ 1 ];
        }
        else if ( unpacker.ParseArguments( args ) == 3 )
        {
            atlasFile = args[ 0 ];
            imageDir  = args[ 1 ];
            outputDir = args[ 2 ];
        }

        if ( atlasFile == null )
        {
            return;
        }

        var atlasFileHandle = new FileInfo( atlasFile ).FullName;

        if ( !File.Exists( atlasFileHandle ) )
        {
            throw new RuntimeException( $"Atlas file not found: {atlasFileHandle}" );
        }

        var atlasParentPath = Path.GetPathRoot( atlasFileHandle );

        if ( atlasParentPath == null )
        {
            return;
        }

        // Set the directory variables to a default when they weren't given in the variables
        imageDir  ??= atlasParentPath;
        outputDir ??= string.Join( atlasParentPath, DEFAULT_OUTPUT_PATH );

        // Opens the atlas file from the specified filename
        var atlas = new TextureAtlasData( new FileInfo( atlasFile ), new DirectoryInfo( imageDir ) );

        unpacker.SplitAtlas( atlas, outputDir );
    }

    /// <summary>
    /// Extract an image from a texture atlas.
    /// </summary>
    /// <param name="page"> The image file related to the page the region is in </param>
    /// <param name="region"> The region to extract </param>
    /// <param name="outputDir"> The output directory </param>
    /// <param name="padding"> padding (in pixels) to apply to the image </param>
    /// <returns> The extracted image </returns>
    public static Bitmap ExtractImage( Bitmap page, TextureAtlasData.Region region, DirectoryInfo outputDir,
                                       int padding )
    {
        Bitmap splitImage;

        // get the needed part of the page and rotate if needed
        if ( region.Rotate )
        {
            var srcRect  = new Rectangle( region.Left, region.Top, region.Height, region.Width );
            var srcImage = page.Clone( srcRect, page.PixelFormat );

            splitImage = new Bitmap( region.Width, region.Height, page.PixelFormat );

            using var g = Graphics.FromImage( splitImage );

            g.InterpolationMode = InterpolationMode.Bilinear;
            g.TranslateTransform( 0, -region.Width );
            g.RotateTransform( 90 );
            g.DrawImage( srcImage, 0, 0 );
        }
        else
        {
            var splitRect = new Rectangle( region.Left, region.Top, region.Width, region.Height );

            splitImage = page.Clone( splitRect, page.PixelFormat );
        }

        // draw the image to a bigger one if padding is needed
        if ( padding > 0 )
        {
            var paddedImage = new Bitmap( splitImage.Width + ( padding * 2 ),
                                          splitImage.Height + ( padding * 2 ),
                                          page.PixelFormat );

            using var g = Graphics.FromImage( paddedImage );
            g.DrawImage( splitImage, padding, padding );

            return paddedImage;
        }

        return splitImage;
    }

    /// <summary>
    /// Extract a ninepatch from a texture atlas.
    /// </summary>
    /// <param name="page"> The image file related to the page the region is in </param>
    /// <param name="region"> The region to extract </param>
    /// <param name="outputDir"></param>
    public static Bitmap ExtractNinePatch( Bitmap page, TextureAtlasData.Region region, DirectoryInfo outputDir )
    {
        var splitImage = ExtractImage( page, region, outputDir, NINEPATCH_PADDING );

        using var g        = Graphics.FromImage( splitImage );
        using var blackPen = new Pen( Color.Black );

        // Draw the four lines to save the ninepatch's padding and splits
        var splits = region.FindValue( "split" );

        if ( splits is { Length: 4 } )
        {
            var startX = splits[ 0 ] + NINEPATCH_PADDING;
            var endX   = ( ( region.Width - splits[ 1 ] ) + NINEPATCH_PADDING ) - 1;
            var startY = splits[ 2 ] + NINEPATCH_PADDING;
            var endY   = ( ( region.Height - splits[ 3 ] ) + NINEPATCH_PADDING ) - 1;

            if ( endX >= startX )
            {
                g.DrawLine( blackPen, startX, 0, endX, 0 );
            }

            if ( endY >= startY )
            {
                g.DrawLine( blackPen, 0, startY, 0, endY );
            }
        }

        var pads = region.FindValue( "pad" );

        if ( pads is { Length: 4 } )
        {
            var padStartX = pads[ 0 ] + NINEPATCH_PADDING;
            var padEndX   = ( ( region.Width - pads[ 1 ] ) + NINEPATCH_PADDING ) - 1;
            var padStartY = pads[ 2 ] + NINEPATCH_PADDING;
            var padEndY   = ( ( region.Height - pads[ 3 ] ) + NINEPATCH_PADDING ) - 1;

            g.DrawLine( blackPen, padStartX, splitImage.Height - 1, padEndX, splitImage.Height - 1 );
            g.DrawLine( blackPen, splitImage.Width - 1, padStartY, splitImage.Width - 1, padEndY );
        }

        return splitImage;
    }

    /// <summary>
    /// Checks the command line arguments for correctness.
    /// </summary>
    /// <returns> 0 If arguments are invalid, Number of arguments otherwise. </returns>
    private int ParseArguments( string[] args )
    {
        var numArgs = args.Length;

        // check if number of args is right
        if ( numArgs < 1 )
        {
            return 0;
        }

        // check if the input file's extension is right
        var extension = args[ 0 ].Substring( args[ 0 ].Length - ATLAS_FILE_EXTENSION.Length )
                                 .Equals( ATLAS_FILE_EXTENSION );

        // check if the directory names are valid
        var directory = true;

        if ( numArgs >= 2 )
        {
            directory &= CheckDirectoryValidity( args[ 1 ] );
        }

        if ( numArgs == 3 )
        {
            directory &= CheckDirectoryValidity( args[ 2 ] );
        }

        return extension && directory ? numArgs : 0;
    }

    /// <summary>
    /// Verifies the validity of a directory path.
    /// </summary>
    /// <param name="directory">The directory path to validate.</param>
    /// <returns>True if the directory path is valid; otherwise, false.</returns>
    private bool CheckDirectoryValidity( string directory )
    {
        var checkFile = new FileInfo( directory );
        var path      = true;

        // try to get the canonical path, if this fails the path is not valid
        try
        {
            _ = Path.GetFullPath( checkFile.Name );
        }
        catch ( Exception )
        {
            path = false;
        }

        return path;
    }

    /// <summary>
    /// Determines the appropriate image format based on the given file extension.
    /// </summary>
    /// <param name="extension">
    /// The file extension (e.g., "png", "jpg", "bmp") used to identify the image format.
    /// </param>
    /// <returns>
    /// The corresponding <see cref="ImageFormat"/> based on the provided file extension.
    /// Defaults to PNG if the format is unrecognized.
    /// </returns>
    private static ImageFormat GetImageFormat( string extension )
    {
        return extension.ToLower() switch
               {
                   "png"           => ImageFormat.Png,
                   "jpg" or "jpeg" => ImageFormat.Jpeg,
                   "bmp"           => ImageFormat.Bmp,
                   "gif"           => ImageFormat.Gif,
                   "tiff"          => ImageFormat.Tiff,
                   _               => ImageFormat.Png
               };
    }

    /// <summary>
    /// Logs the details of an exception and terminates the application with an exit code of 1.
    /// </summary>
    /// <param name="e">The exception to be logged before exiting the application.</param>
    private static void PrintExceptionAndExit( Exception e )
    {
        Logger.Error( e.StackTrace ?? "No Stacktrace available" );

        Environment.Exit( 1 );
    }
}

// ============================================================================
// ============================================================================