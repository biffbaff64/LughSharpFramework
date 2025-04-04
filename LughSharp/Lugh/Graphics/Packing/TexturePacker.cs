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
using System.Text.Json;

using LughSharp.Lugh.Graphics.Atlases;
using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Guarding;

using Bitmap = System.Drawing.Bitmap;
using Image = System.Drawing.Image;
using Pen = System.Drawing.Pen;

namespace LughSharp.Lugh.Graphics.Packing;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class TexturePacker
{
    private Settings           _settings;
    private IPacker            _packer;
    private ImageProcessor     _imageProcessor;
    private List< InputImage > _inputImages = [ ];
    private ProgressListener?  _progressListener;
    private string?            _rootPath;

    // ========================================================================
    // ========================================================================

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
                throw new GdxRuntimeException( $"If pot is true, maxWidth must be a power of two: {settings.MaxWidth}" );
            }

            if ( settings.MaxHeight != MathUtils.NextPowerOfTwo( settings.MaxHeight ) )
            {
                throw new GdxRuntimeException( $"If pot is true, maxHeight must be a power of two: {settings.MaxHeight}" );
            }
        }

        if ( settings.MultipleOfFour )
        {
            if ( ( settings.MaxWidth % 4 ) != 0 )
            {
                throw new GdxRuntimeException( $"If mod4 is true, maxWidth must be evenly divisible by 4: {settings.MaxWidth}" );
            }

            if ( ( settings.MaxHeight % 4 ) != 0 )
            {
                throw new GdxRuntimeException( $"If mod4 is true, maxHeight must be evenly divisible by 4: {settings.MaxHeight}" );
            }
        }

        _packer         = settings.Grid ? new GridPacker( settings ) : new MaxRectsPacker( settings );
        _imageProcessor = NewImageProcessor( settings );

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
    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    protected static ImageProcessor NewImageProcessor( Settings settings )
    {
        return new ImageProcessor( settings );
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public string? GetRootPath() => _rootPath;

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    private void SetRootPath( string? value ) => _rootPath = value;

    /// <summary>
    /// </summary>
    /// <param name="rootDir"></param>
    public void SetRootDir( DirectoryInfo? rootDir )
    {
        if ( rootDir == null )
        {
            SetRootPath( null );

            return;
        }

        Guard.ThrowIfNull( GetRootPath() );
        
        SetRootPath( Path.GetFullPath( rootDir.FullName ) );
        SetRootPath( GetRootPath()!.Replace( '\\', '/' ) );

        if ( !GetRootPath()!.EndsWith( '/' ) )
        {
            SetRootPath( GetRootPath() + "/" );
        }
    }

    public void AddImage( FileInfo file )
    {
        var inputImage = new InputImage
        {
            FileInfo = file,
            RootPath = GetRootPath(),
        };

        _inputImages.Add( inputImage );
    }

    public void AddImage( Bitmap image, string name )
    {
        var inputImage = new InputImage
        {
            Image = image,
            Name  = name,
        };

        _inputImages.Add( inputImage );
    }

    public void SetPacker( IPacker packer )
    {
        _packer = packer;
    }

    public void SetProgressListener( ProgressListener? listener )
    {
        _progressListener = listener;
    }

    /// <summary>
    /// Packs processed images into the a <see cref="Graphics.Atlases.TextureAtlas"/> with the
    /// specified filename. The atlas will be stored in the specified directory.
    /// </summary>
    /// <param name="outputDir"> The destination directory. </param>
    /// <param name="packFileName"> The name for the resulting TextureAtlas. </param>
    /// <exception cref="GdxRuntimeException"></exception>
    public void Pack( DirectoryInfo outputDir, string packFileName )
    {
        ArgumentNullException.ThrowIfNull( outputDir );

        if ( packFileName.EndsWith( _settings.AtlasExtension ) )
        {
            packFileName = packFileName.Substring( 0, packFileName.Length - _settings.AtlasExtension.Length );
        }

        Directory.CreateDirectory( outputDir.FullName );

        _progressListener ??= new ProgressListenerImpl();
        _progressListener.Start( 1 );

        var n = _settings.Scale.Length;

        for ( var i = 0; i < n; i++ )
        {
            _progressListener.Start( 1f / n );

            _imageProcessor.Scale = _settings.Scale[ i ];

            if ( ( _settings.ScaleResampling != null )
                 && ( _settings.ScaleResampling.Length > i )
                 && ( _settings.ScaleResampling[ i ] != Resampling.None ) )
            {
                _imageProcessor.SetResampling( _settings.ScaleResampling[ i ] );
            }

            _progressListener.Start( 0.35f );
            _progressListener.Count = 0;
            _progressListener.Total = _inputImages.Count;

            for ( int ii = 0, nn = _inputImages.Count; ii < nn; ii++, _progressListener.Count++ )
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

                if ( _progressListener.Update( ii + 1, nn ) )
                {
                    return;
                }
            }

            _progressListener.End();
            _progressListener.Start( 0.35f );
            _progressListener.Count = 0;
            _progressListener.Total = _imageProcessor.GetImages().Count;

            var pages = _packer.Pack( _progressListener, _imageProcessor.GetImages() );

            _progressListener.End();
            _progressListener.Start( 0.29f );
            _progressListener.Count = 0;
            _progressListener.Total = pages.Count;

            var scaledPackFileName = _settings.GetScaledPackFileName( packFileName, i );

            WriteImages( outputDir.FullName, scaledPackFileName, pages );

            _progressListener.End();
            _progressListener.Start( 0.01f );

            try
            {
                WritePackFile( outputDir, scaledPackFileName, pages );
            }
            catch ( IOException ex )
            {
                throw new GdxRuntimeException( "Error writing pack file.", ex );
            }

            _imageProcessor.Clear();
            _progressListener.End();

            if ( _progressListener.Update( i + 1, n ) )
            {
                return;
            }
        }

        _progressListener.End();
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
        ArgumentNullException.ThrowIfNull( outputDir );
        ArgumentNullException.ThrowIfNull( _progressListener );

        var packFileNoExt = Path.Combine( outputDir, scaledPackFileName );
        var packDir       = Path.GetDirectoryName( packFileNoExt );
        var imageName     = Path.GetFileName( packFileNoExt );

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

                if ( !File.Exists( outputFile ) )
                {
                    break;
                }
            }

            // Create output directories
            Directory.CreateDirectory( Path.GetDirectoryName( outputFile )
                                       ?? throw new GdxRuntimeException( "Error creating output directory" ) );

            page.ImageName = Path.GetFileName( outputFile );

            var canvas = new Bitmap( width, height, _settings.Format );
            var g      = System.Drawing.Graphics.FromImage( canvas );

            if ( !_settings.Silent )
            {
                Logger.Debug( $"Writing {canvas.Width}x{canvas.Height}: {outputFile}" );
            }

            if ( page.OutputRects == null )
            {
                throw new NullReferenceException( "OutputRects for page is null" );
            }

            _progressListener.Start( 1f / pages.Count );

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
                            // Copy corner pixels to fill corners of the padding.
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
                            // Copy corner pixels to fill corners of the padding.
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

                    if ( _progressListener.Update( r + 1, page.OutputRects.Count ) )
                    {
                        return;
                    }
                }
            }

            _progressListener.End();

            if ( _settings is { Bleed: true, PremultiplyAlpha: false }
                 && !( _settings.OutputFormat.Equals( "jpg", StringComparison.OrdinalIgnoreCase ) ||
                       _settings.OutputFormat.Equals( "jpeg", StringComparison.OrdinalIgnoreCase ) ) )
            {
                canvas = new ColorBleedEffect().ProcessImage( canvas, _settings.BleedIterations );

                g.Dispose(); // Dispose previous Graphics object
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

            if ( _progressListener.Update( p + 1, pages.Count ) )
            {
                return;
            }

            _progressListener.Count++;
        }
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

        if ( _settings.Format != PixelFormat.Format32bppArgb )
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
        if ( _settings is { WrapX: Texture.TextureWrap.Repeat, WrapY: Texture.TextureWrap.Repeat } )
        {
            return "xy";
        }

        if ( _settings is { WrapX: Texture.TextureWrap.Repeat, WrapY: Texture.TextureWrap.ClampToEdge } )
        {
            return "x";
        }

        if ( _settings is { WrapX: Texture.TextureWrap.ClampToEdge, WrapY: Texture.TextureWrap.Repeat } )
        {
            return "y";
        }

        return null;
    }

    private PixelFormat GetPixelFormat( PixelType.Format format )
    {
        switch ( format )
        {
            case PixelType.Format.RGBA8888:
            case PixelType.Format.RGBA4444:
                return PixelFormat.Format32bppArgb;

            case PixelType.Format.RGB565:
            case PixelType.Format.RGB888:
                return PixelFormat.Format32bppRgb;

            case PixelType.Format.Alpha:
                return PixelFormat.Alpha;

            default:
                throw new GdxRuntimeException( $"Unsupported format: {_settings.Format}" );
        }
    }

    /// <summary>
    /// Packs using defaults settings.
    /// </summary>
    public static void Process( string input, string output, string packFileName )
    {
        Process( new Settings(), input, output, packFileName );
    }

    /// <summary>
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="input"> Directory containing individual images to be packed. </param>
    /// <param name="output"> Directory where the pack file and page images will be written. </param>
    /// <param name="packFileName"> The name of the pack file. Also used to name the page images. </param>
    /// <param name="progress"> May be null. </param>
    public static void Process( Settings settings,
                                string input,
                                string output,
                                string packFileName,
                                ProgressListener? progress = null )
    {
        try
        {
            var processor = new TexturePackerFileProcessor( settings, packFileName, progress );
            processor.Process( input, output );
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( "Error packing images.", ex );
        }
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
            throw new ArgumentException( "Input file does not exist: " + inputFile.Name );
        }

        return IsModified( inputFile.FullName, outputFile.LastWriteTimeUtc.Ticks / 10000 );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
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
                        //TODO:
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

    public static bool ProcessIfModified( string input, string output, string packFileName )
    {
        var settings = new Settings();

        if ( IsModified( input, output, packFileName, settings ) )
        {
            Process( settings, input, output, packFileName );

            return true;
        }

        return false;
    }

    public static bool ProcessIfModified( Settings settings, string input, string output, string packFileName )
    {
        if ( IsModified( input, output, packFileName, settings ) )
        {
            Process( settings, input, output, packFileName );

            return true;
        }

        return false;
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Page
    {
        public string?       ImageName      { get; set; }
        public List< Rect >? OutputRects    { get; set; }
        public List< Rect >? RemainingRects { get; set; }
        public float         Occupancy      { get; set; }
        public int           X              { get; set; }
        public int           Y              { get; set; }
        public int           Width          { get; set; }
        public int           Height         { get; set; }
        public int           ImageWidth     { get; set; }
        public int           ImageHeight    { get; set; }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    [SupportedOSPlatform( "windows" )]
    public class Alias : IComparable< Alias >
    {
        public int     Index;
        public string? Name;
        public int     OffsetX;
        public int     OffsetY;
        public int     OriginalHeight;
        public int     OriginalWidth;
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
    [SupportedOSPlatform( "windows" )]
    public class Rect : IComparable< Rect >
    {
        public int           Score1;
        public int           Score2;
        public bool          CanRotate = true;
        public int           Height; // Portion of page taken by this region, including padding.
        public int           Index;
        public string?       Name = string.Empty;
        public int           OffsetX;
        public int           OffsetY;
        public int           OriginalHeight;
        public int           OriginalWidth;
        public int[]?        Pads = null;
        public int           RegionHeight;
        public int           RegionWidth;
        public bool          Rotated;
        public List< Alias > Aliases = [ ];
        public int[]?        Splits  = null;
        public int           Width; // Portion of page taken by this region, including padding.
        public int           X;
        public int           Y;

        private Bitmap?  _bufferedImage;
        private FileInfo _file = null!;
        private bool     _isPatch;

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

        [SupportedOSPlatform( "windows" )]
        public Rect( Bitmap source, int left, int top, int newWidth, int newHeight, bool isPatch )
        {
            _bufferedImage = new Bitmap( newWidth, newHeight, source.PixelFormat );

            OffsetX        = left;
            OffsetY        = top;
            RegionWidth    = newWidth;
            RegionHeight   = newHeight;
            OriginalWidth  = source.Width;
            OriginalHeight = source.Height;
            Width          = newWidth;
            Height         = newHeight;
            _isPatch       = isPatch;
        }

        public void UnloadImage( FileInfo fileInfo )
        {
            _file = fileInfo;

            _bufferedImage = null;
        }

        [SupportedOSPlatform( "windows" )]
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
        public List< Page > Pack( ProgressListener progressListener, List< Rect > inputRects );
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class InputImage
    {
        public FileInfo? FileInfo { get; set; }
        public string?   RootPath { get; set; }
        public string?   Name     { get; set; }
        public Bitmap?   Image    { get; set; }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class ProgressListenerImpl : ProgressListener
    {
        /// <inheritdoc />
        protected override void Progress( float progress )
        {
        }
    }

    [PublicAPI]
    public abstract class ProgressListener
    {
        public int  Count    { get; set; }
        public int  Total    { get; set; }
        public bool Canceled { get; set; }

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

    [PublicAPI]
    [SupportedOSPlatform( "windows" )]
    public class Settings
    {
        public bool                  MultipleOfFour        { get; set; }
        public bool                  Rotation              { get; set; }
        public bool                  PowerOfTwo            { get; set; } = true;
        public int                   PaddingX              { get; set; } = 2;
        public int                   PaddingY              { get; set; } = 2;
        public bool                  EdgePadding           { get; set; } = true;
        public bool                  DuplicatePadding      { get; set; } = false;
        public int                   MinWidth              { get; set; } = 16;
        public int                   MinHeight             { get; set; } = 16;
        public int                   MaxWidth              { get; set; } = 1024;
        public int                   MaxHeight             { get; set; } = 1024;
        public bool                  Square                { get; set; } = false;
        public bool                  StripWhitespaceX      { get; set; }
        public bool                  StripWhitespaceY      { get; set; }
        public int                   AlphaThreshold        { get; set; }
        public Texture.TextureFilter FilterMin             { get; set; } = Texture.TextureFilter.Nearest;
        public Texture.TextureFilter FilterMag             { get; set; } = Texture.TextureFilter.Nearest;
        public Texture.TextureWrap   WrapX                 { get; set; } = Texture.TextureWrap.ClampToEdge;
        public Texture.TextureWrap   WrapY                 { get; set; } = Texture.TextureWrap.ClampToEdge;
        public PixelFormat           Format                { get; set; } = PixelFormat.Format32bppArgb;
        public bool                  IsAlias               { get; set; } = true;
        public string                OutputFormat          { get; set; } = "png";
        public float                 JpegQuality           { get; set; } = 0.9f;
        public bool                  IgnoreBlankImages     { get; set; } = true;
        public bool                  Fast                  { get; set; }
        public bool                  Debug                 { get; set; }
        public bool                  Silent                { get; set; }
        public bool                  CombineSubdirectories { get; set; }
        public bool                  Ignore                { get; set; }
        public bool                  FlattenPaths          { get; set; }
        public bool                  PremultiplyAlpha      { get; set; }
        public bool                  UseIndexes            { get; set; } = true;
        public bool                  Bleed                 { get; set; } = true;
        public int                   BleedIterations       { get; set; } = 2;
        public bool                  LimitMemory           { get; set; } = true;
        public bool                  Grid                  { get; set; }
        public float[]               Scale                 { get; set; } = [ 1 ];
        public string[]              ScaleSuffix           { get; set; } = [ "" ];
        public Resampling[]          ScaleResampling       { get; set; } = [ Resampling.Bicubic ];
        public string                AtlasExtension        { get; set; } = ".atlas";
        public bool                  PrettyPrint           { get; set; } = true;
        public bool                  LegacyOutput          { get; set; } = true;

        // ====================================================================

        public Settings()
        {
        }

        public Settings( Settings settings )
        {
            Set( settings );
        }

        // ====================================================================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public void Set( Settings settings )
        {
            MinWidth  = settings.MinWidth;
            MinHeight = settings.MinHeight;
            MaxWidth  = settings.MaxWidth;
            MaxHeight = settings.MaxHeight;

            Fast                  = settings.Fast;
            Rotation              = settings.Rotation;
            PowerOfTwo            = settings.PowerOfTwo;
            MultipleOfFour        = settings.MultipleOfFour;
            PaddingX              = settings.PaddingX;
            PaddingY              = settings.PaddingY;
            EdgePadding           = settings.EdgePadding;
            DuplicatePadding      = settings.DuplicatePadding;
            AlphaThreshold        = settings.AlphaThreshold;
            IgnoreBlankImages     = settings.IgnoreBlankImages;
            StripWhitespaceX      = settings.StripWhitespaceX;
            StripWhitespaceY      = settings.StripWhitespaceY;
            IsAlias               = settings.IsAlias;
            Format                = settings.Format;
            JpegQuality           = settings.JpegQuality;
            OutputFormat          = settings.OutputFormat;
            FilterMin             = settings.FilterMin;
            FilterMag             = settings.FilterMag;
            WrapX                 = settings.WrapX;
            WrapY                 = settings.WrapY;
            Debug                 = settings.Debug;
            Silent                = settings.Silent;
            CombineSubdirectories = settings.CombineSubdirectories;
            Ignore                = settings.Ignore;
            FlattenPaths          = settings.FlattenPaths;
            PremultiplyAlpha      = settings.PremultiplyAlpha;
            Square                = settings.Square;
            UseIndexes            = settings.UseIndexes;
            Bleed                 = settings.Bleed;
            BleedIterations       = settings.BleedIterations;
            LimitMemory           = settings.LimitMemory;
            Grid                  = settings.Grid;
            AtlasExtension        = settings.AtlasExtension;
            PrettyPrint           = settings.PrettyPrint;
            LegacyOutput          = settings.LegacyOutput;

            settings.Scale.CopyTo( Scale, 0 );
            settings.ScaleSuffix.CopyTo( ScaleSuffix, 0 );
            settings.ScaleResampling.CopyTo( ScaleResampling, 0 );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packFileName"></param>
        /// <param name="scaleIndex"></param>
        /// <returns></returns>
        public string GetScaledPackFileName( string packFileName, int scaleIndex )
        {
            // Use suffix if not empty string.
            if ( ScaleSuffix[ scaleIndex ].Length > 0 )
            {
                packFileName += ScaleSuffix[ scaleIndex ];
            }
            else
            {
                // Otherwise if scale != 1 or multiple scales, use subdirectory.
                var scaleValue = Scale[ scaleIndex ];

                if ( Scale.Length != 1 )
                {
                    packFileName = ( ( scaleValue % 1 ) == 0f ? $"{( int )scaleValue}" : $"{scaleValue}" )
                                   + "/"
                                   + packFileName;
                }
            }

            return packFileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        public void ReadFromJson( JsonElement root )
        {
        }
    }
}