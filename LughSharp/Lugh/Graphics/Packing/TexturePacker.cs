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

using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Packing;

[PublicAPI]
public class TexturePacker
{
    public string? RootPath { get; private set; }

    // ========================================================================

    private Settings           _settings;
    private IPacker            _packer;
    private ImageProcessor     _imageProcessor;
    private List< InputImage > _inputImages = [ ];
    private ProgressListener?  _progressListener;

    // ========================================================================
    // ========================================================================

    public TexturePacker( FileInfo? rootDir, Settings settings )
    {
        this._settings = settings;

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
                throw new GdxRuntimeException( $"If mod4 is true, maxWidth must be evenly " +
                                               $"divisible by 4: {settings.MaxWidth}" );
            }

            if ( ( settings.MaxHeight % 4 ) != 0 )
            {
                throw new GdxRuntimeException( $"If mod4 is true, maxHeight must be evenly " +
                                               $"divisible by 4: {settings.MaxHeight}" );
            }
        }

        _packer         = settings.Grid ? new GridPacker( settings ) : new MaxRectsPacker( settings );
        _imageProcessor = NewImageProcessor( settings );

        SetRootDir( rootDir );
    }

    public TexturePacker( Settings settings )
        : this( null, settings )
    {
    }

    protected static ImageProcessor NewImageProcessor( Settings settings )
    {
        return new ImageProcessor( settings );
    }

    public void SetRootDir( FileInfo? rootDir )
    {
        if ( rootDir == null )
        {
            RootPath = null;

            return;
        }

        RootPath = Path.GetFullPath( rootDir.FullName );
        RootPath = RootPath.Replace( '\\', '/' );

        if ( !RootPath.EndsWith( '/' ) )
        {
            RootPath += "/";
        }
    }

    public void AddImage( FileInfo file )
    {
        var inputImage = new InputImage
        {
            FileInfo = file,
            RootPath = RootPath,
        };

        _inputImages.Add( inputImage );
    }

    public void AddImage( BufferedImage image, string name )
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
        this._packer = packer;
    }

    /// <summary>
    /// Packs processed images into the a <see cref="Graphics.Atlases.TextureAtlas"/> with the
    /// specified filename. The atlas will be stored in the specified directory.
    /// </summary>
    /// <param name="outputDir"> The destination directory. </param>
    /// <param name="packFileName"> The name for the resulting TextureAtlas. </param>
    /// <exception cref="GdxRuntimeException"></exception>
    public void Pack( FileInfo outputDir, string packFileName )
    {
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

            WriteImages( outputDir, scaledPackFileName, pages );

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

    private void WriteImages( FileInfo outputDir, string scaledPackFileName, List< Page > pages )
    {
        var fileName      = Path.Combine( outputDir.FullName, scaledPackFileName );
        var packFileNoExt = new FileInfo( fileName );
        var packDir       = Directory.GetParent( packFileNoExt.Name );
        var imageName     = packFileNoExt.Name;
        var fileIndex     = 1;

        for ( int p = 0, pn = pages.Count; p < pn; p++ )
        {
            var page   = pages[ p ];
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

            FileInfo outputFile;

            while ( true )
            {
                var name = imageName;

                if ( fileIndex > 1 )
                {
                    // Last character is a digit or a digit + 'x'.
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

                var fp = Path.Combine( packDir!.Name, $"{name}.{_settings.OutputFormat}" );
                outputFile = new FileInfo( fp );

                if ( !File.Exists( outputFile.FullName ) )
                {
                    break;
                }
            }

            Directory.CreateDirectory( Path.GetDirectoryName( outputFile.FullName )
                                       ?? throw new GdxRuntimeException( "Error creating directory for pack file." ) );

            page.ImageName = Path.GetFileName( outputFile.FullName );

            var canvas = new BufferedImage( width, height, _settings.Format );

            if ( !_settings.Silent )
            {
                Console.WriteLine( $"Writing {canvas.Width}x{canvas.Height}: {outputFile}" );
            }

            _progressListener?.Start( 1 / ( float )pn );

            GdxRuntimeException.ThrowIfNull( page.OutputRects );

            for ( int r = 0, rn = page.OutputRects.Count; r < rn; r++ )
            {
                var rect  = page.OutputRects[ r ];
                var image = rect.GetImage( _imageProcessor );
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
                                Plot( canvas, ( ( rectX + iw ) - 1 ) + i, ( ( rectY + ih ) - 1 ) + j, image.GetPixel( iw - 1, ih - 1 ) );
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

//                if ( _settings.debug )
//                {
//                    g.SetColor( Color.Magenta );
//                    g.DrawRectangle( rectX, rectY, rect.Width - _settings.paddingX - 1, rect.Height - _settings.paddingY - 1 );
//                }

                if ( _progressListener.Update( r + 1, rn ) )
                {
                    return;
                }
            }

            _progressListener.End();

//            if ( _settings is { bleed: true, premultiplyAlpha: false }
//                 && !( string.Equals( _settings.outputFormat, "jpg", StringComparison.Ordinal )
//                       || string.Equals( _settings.outputFormat, "jpeg", StringComparison.Ordinal ) )
//            {
//                canvas = new ColorBleedEffect().ProcessImage( canvas, _settings.bleedIterations );
//                g      = ( Graphics2D )canvas.getGraphics();
//            }

//            if ( _settings.debug )
//            {
//                g.setColor( Color.magenta );
//                g.drawRect( 0, 0, width - 1, height - 1 );
//            }

//            ImageOutputStream ios = null;

//            try
//            {
//                if ( _settings.outputFormat.equalsIgnoreCase( "jpg" ) || _settings.outputFormat.equalsIgnoreCase( "jpeg" ) )
//                {
//                    var newImage = new BufferedImage( canvas.getWidth(), canvas.getHeight(), BufferedImage.TYPE_3BYTE_BGR );
//                    newImage.getGraphics().drawImage( canvas, 0, 0, null );
//                    canvas = newImage;
//
//                    Iterator< ImageWriter > writers = ImageIO.getImageWritersByFormatName( "jpg" );
//                    ImageWriter             writer  = writers.next();
//                    ImageWriteParam         param   = writer.getDefaultWriteParam();
//                    param.setCompressionMode( ImageWriteParam.MODE_EXPLICIT );
//                    param.setCompressionQuality( _settings.jpegQuality );
//                    ios = ImageIO.createImageOutputStream( outputFile );
//                    writer.setOutput( ios );
//                    writer.write( null, new IIOImage( canvas, null, null ), param );
//                }
//                else
//                {
//                    if ( _settings.premultiplyAlpha ) canvas.getColorModel().coerceData( canvas.getRaster(), true );
//                    ImageIO.write( canvas, "png", outputFile );
//                }
//            }
//            catch ( IOException ex )
//            {
//                throw new GdxRuntimeException( "Error writing file: " + outputFile, ex );
//            }
//            finally
//            {
//                if ( ios != null )
//                {
//                    try
//                    {
//                        ios.close();
//                    }
//                    catch ( Exception ignored )
//                    {
//                    }
//                }
//            }

            if ( _progressListener.Update( p + 1, pn ) ) return;

            _progressListener.Count++;
        }
    }

    private static void Plot( BufferedImage dst, int x, int y, uint argb )
    {
        if ( ( 0 <= x ) && ( x < dst.Width ) && ( 0 <= y ) && ( y < dst.Height ) ) dst.SetPixel( x, y, argb );
    }

    private static void Copy( BufferedImage src, int x, int y, int w, int h, BufferedImage dst, int dx, int dy, bool rotated )
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

    private void WritePackFile( FileInfo outputDir, string scaledPackFileName, List< Page > pages )
    {
//        var packFile = new
//            FileInfo( outputDir, scaledPackFileName + _settings.atlasExtension );
//        FileInfo packDir = packFile.getParentFile();
//        packDir.mkdirs();
//
//        if ( packFile.exists() )
//        {
//            // Make sure there aren't duplicate names.
//            var textureAtlasData = new TextureAtlasData( new FileHandle( packFile ), new FileHandle( packFile ), false );
//            for ( Page page :
//            pages) {
//                for ( Rect rect :
//                page.outputRects)
//                {
//                    string rectName = Rect.getAtlasName( rect.name, _settings.flattenPaths );
//                    for ( Region region :
//                    textureAtlasData.getRegions()) {
//                        if ( region.name.equals( rectName ) )
//                        {
//                            throw new GdxRuntimeException( "A region with the name \"" + rectName + "\" has already been packed: " +
//                                                           rect.name );
//                        }
//                    }
//                }
//            }
//        }
//
//        string tab = "", colon = ":", comma = ",";
//
//        if ( _settings.prettyPrint )
//        {
//            tab   = "\t";
//            colon = ": ";
//            comma = ", ";
//        }
//
//        bool               appending = packFile.exists();
//        OutputStreamWriter writer    = new OutputStreamWriter( new FileOutputStream( packFile, true ), "UTF-8" );
//
//        for ( int i = 0, n = pages.size; i < n; i++ )
//        {
//            Page page = pages.get( i );
//
//            if ( _settings.legacyOutput )
//                WritePageLegacy( writer, page );
//            else
//            {
//                if ( ( i != 0 ) || appending ) writer.write( "\n" );
//                writePage( writer, appending, page );
//            }
//
//            page.outputRects.sort();
//            for ( Rect rect :
//            page.outputRects) {
//                if ( _settings.legacyOutput )
//                    WriteRectLegacy( writer, page, rect, rect.name );
//                else
//                    writeRect( writer, page, rect, rect.name );
//                List< Alias > aliases = new List( rect.aliases.toList() );
//                aliases.sort();
//                for ( Alias alias :
//                aliases) {
//                    var aliasRect = new Rect();
//                    aliasRect.set( rect );
//                    alias.apply( aliasRect );
//                    if ( _settings.legacyOutput )
//                        WriteRectLegacy( writer, page, aliasRect, alias.name );
//                    else
//                        writeRect( writer, page, aliasRect, alias.name );
//                }
//            }
//        }
//
//        writer.close();
    }

//    private void WritePage( OutputStreamWriter writer, bool appending, Page page )
//    {
//        string tab = "", colon = ":", comma = ",";
//
//        if ( _settings.prettyPrint )
//        {
//            tab   = "\t";
//            colon = ": ";
//            comma = ", ";
//        }
//
//        writer.write( page.imageName + "\n" );
//        writer.write( tab + "size" + colon + page.imageWidth + comma + page.imageHeight + "\n" );
//
//        if ( _settings.format != PixelType.Format.RGBA8888 ) writer.write( tab + "format" + colon + _settings.format + "\n" );
//
//        if ( ( _settings.filterMin != Texture.TextureFilter.Nearest ) || ( _settings.filterMag != Texture.TextureFilter.Nearest ) )
//            writer.write( tab + "filter" + colon + _settings.filterMin + comma + _settings.filterMag + "\n" );
//
//        var repeatValue = GetRepeatValue();
//        if ( repeatValue != null ) writer.write( tab + "repeat" + colon + repeatValue + "\n" );
//
//        if ( _settings.premultiplyAlpha ) writer.write( tab + "pma" + colon + "true\n" );
//    }

//    private void WriteRect( Writer writer, Page page, Rect rect, string name )
//    {
//        string tab = "", colon = ":", comma = ",";
//
//        if ( _settings.prettyPrint )
//        {
//            tab   = "\t";
//            colon = ": ";
//            comma = ", ";
//        }
//
//        writer.write( Rect.getAtlasName( name, _settings.flattenPaths ) + "\n" );
//        if ( rect.index != -1 ) writer.write( tab + "index" + colon + rect.index + "\n" );
//
//        writer.write( tab + "bounds" + colon //
//                      + ( page.x + rect.x ) + comma + ( ( page.y + page.height ) - rect.y - ( rect.height - _settings.paddingY ) ) +
//                      comma //
//                      + rect.regionWidth + comma + rect.regionHeight + "\n" );
//
//        int offsetY = rect.originalHeight - rect.regionHeight - rect.offsetY;
//
//        if ( ( rect.offsetX != 0 ) || ( offsetY != 0 ) //
//                                   || ( rect.originalWidth != rect.regionWidth ) || ( rect.originalHeight != rect.regionHeight ) )
//        {
//            writer.write( tab + "offsets" + colon                  //
//                          + rect.offsetX + comma + offsetY + comma //
//                          + rect.originalWidth + comma + rect.originalHeight + "\n" );
//        }
//
//        if ( rect.Rotated ) writer.write( tab + "rotate" + colon + rect.Rotated + "\n" );
//
//        if ( rect.splits != null )
//        {
//            writer.write( tab + "split" + colon                                 //
//                          + rect.splits[ 0 ] + comma + rect.splits[ 1 ] + comma //
//                          + rect.splits[ 2 ] + comma + rect.splits[ 3 ] + "\n" );
//        }
//
//        if ( rect.pads != null )
//        {
//            if ( rect.splits == null ) writer.write( tab + "split" + colon + "0" + comma + "0" + comma + "0" + comma + "0\n" );
//            writer.write(
//                         tab + "pad" + colon + rect.pads[ 0 ] + comma + rect.pads[ 1 ] + comma + rect.pads[ 2 ] + comma + rect.pads[ 3 ] +
//                         "\n" );
//        }
//    }

//    private void WritePageLegacy( OutputStreamWriter writer, Page page )
//    {
//        writer.write( "\n" + page.imageName + "\n" );
//        writer.write( "size: " + page.imageWidth + ", " + page.imageHeight + "\n" );
//        writer.write( "format: " + settings.format + "\n" );
//        writer.write( "filter: " + settings.filterMin + ", " + settings.filterMag + "\n" );
//        var repeatValue = GetRepeatValue();
//        writer.write( "repeat: " + ( repeatValue == null ? "none" : repeatValue ) + "\n" );
//    }

//    private void WriteRectLegacy( Writer writer, Page page, Rect rect, string name )
//    {
//        writer.write( Rect.getAtlasName( name, settings.flattenPaths ) + "\n" );
//        writer.write( "  rotate: " + rect.Rotated + "\n" );
//        writer
//            .write( "  xy: " + ( page.x + rect.x ) + ", " + ( ( page.y + page.height ) - rect.y - ( rect.height - settings.paddingY ) ) +
//                    "\n" );
//
//        writer.write( "  size: " + rect.regionWidth + ", " + rect.regionHeight + "\n" );
//
//        if ( rect.splits != null )
//        {
//            writer.write( "  split: " //
//                          + rect.splits[ 0 ] + ", " + rect.splits[ 1 ] + ", " + rect.splits[ 2 ] + ", " + rect.splits[ 3 ] + "\n" );
//        }
//
//        if ( rect.pads != null )
//        {
//            if ( rect.splits == null ) writer.write( "  split: 0, 0, 0, 0\n" );
//            writer.write( "  pad: " + rect.pads[ 0 ] + ", " + rect.pads[ 1 ] + ", " + rect.pads[ 2 ] + ", " + rect.pads[ 3 ] + "\n" );
//        }
//
//        writer.write( "  orig: " + rect.originalWidth + ", " + rect.originalHeight + "\n" );
//        writer.write( "  offset: " + rect.offsetX + ", " + ( rect.originalHeight - rect.regionHeight - rect.offsetY ) + "\n" );
//        writer.write( "  index: " + rect.index + "\n" );
//    }

    private string? GetRepeatValue()
    {
        if ( ( _settings.WrapX == Texture.TextureWrap.Repeat ) && ( _settings.WrapY == Texture.TextureWrap.Repeat ) )
        {
            return "xy";
        }

        if ( ( _settings.WrapX == Texture.TextureWrap.Repeat ) && ( _settings.WrapY == Texture.TextureWrap.ClampToEdge ) )
        {
            return "x";
        }

        if ( ( _settings.WrapX == Texture.TextureWrap.ClampToEdge ) && ( _settings.WrapY == Texture.TextureWrap.Repeat ) )
        {
            return "y";
        }

        return null;
    }

    //TODO:
    [SupportedOSPlatform( "windows" )]
    private PixelFormat GetBufferedImageType( PixelType.Format format )
    {
        switch ( _settings.Format )
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

    public void SetProgressListener( ProgressListener progress )
    {
        this._progressListener = progress;
    }

    /** Packs using defaults settings.
     * @see TexturePacker#process(Settings, string, string, string) */
    public static void Process( string input, string output, string packFileName )
    {
        Process( new Settings(), input, output, packFileName );
    }

    public static void Process( Settings settings, string input, string output, string packFileName )
    {
        Process( settings, input, output, packFileName, null );
    }

    /** @param input Directory containing individual images to be packed.
     * @param output Directory where the pack file and page images will be written.
     * @param packFileName The name of the pack file. Also used to name the page images.
     * @param progress May be null. */
    public static void Process( Settings settings, string input, string output, string packFileName,
                                ProgressListener? progress )
    {
        try
        {
            var processor = new TexturePackerFileProcessor( settings, packFileName, progress );
            processor.Process( new FileInfo( input ), new FileInfo( output ) );
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( "Error packing images.", ex );
        }
    }

    /** @return true if the output file does not yet exist or its last modification date is before the last modification date of
     *         the input file */
    public static bool IsModified( string input, string output, string packFileName, Settings settings )
    {
        var packFullFileName = output;

        if ( !packFullFileName.EndsWith( '/' ) )
        {
            packFullFileName += "/";
        }

        packFullFileName += packFileName;
        packFullFileName += settings.AtlasExtension;

        // Check against the only file we know for sure will exist and will be changed if any asset changes: the atlas file.
        var outputFile = new FileInfo( packFullFileName );

        if ( !File.Exists( outputFile.FullName ) ) return true;

        var inputFile = new FileInfo( input );

        if ( !File.Exists( inputFile.FullName ) )
        {
            throw new ArgumentException( "Input file does not exist: " + inputFile.Name );
        }

        return IsModified( inputFile.FullName, ( outputFile.LastWriteTimeUtc.Ticks / 10000 ) );
    }

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
            // Handle exceptions (e.g., file not found, access denied)
            Console.WriteLine( $"Error checking modification: {ex.Message}" );

            return false; // Or throw an exception, depending on your error handling strategy
        }
    }

    public static bool ProcessIfModified( string input, string output, string packFileName )
    {
        // Default settings (Needed to access the default atlas extension string)
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
    public class Alias : IComparable< Alias >
    {
        public int     Index;
        public string? Name;
        public int     OffsetX;
        public int     OffsetY;
        public int     OriginalHeight;
        public int     OriginalWidth;
        public int[]   Pads;
        public int[]   Splits;

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
        private BufferedImage? _bufferedImage;
        private FileInfo       _file = null!;

        private bool   _isPatch;
        public  int    Score1;
        public  int    Score2;
        public  bool   CanRotate = true;
        public  int    Height; // Portion of page taken by this region, including padding.
        public  int    Index;
        public  string Name = string.Empty;
        public  int    OffsetX;
        public  int    OffsetY;
        public  int    OriginalHeight;
        public  int    OriginalWidth;
        public  int[]  Pads = null!;
        public  int    RegionHeight;
        public  int    RegionWidth;
        public  bool   Rotated;

        public List< Alias > Aliases = [ ];
        public int[]         Splits  = null!;
        public int           Width; // Portion of page taken by this region, including padding.
        public int           X;
        public int           Y;

        public Rect()
        {
        }

        public Rect( Rect rect )
        {
            this.X      = rect.X;
            this.Y      = rect.Y;
            this.Width  = rect.Width;
            this.Height = rect.Height;
        }

        public int CompareTo( Rect? o )
        {
            return string.Compare( Name, o?.Name, StringComparison.Ordinal );
        }

        public Rect( BufferedImage source, int left, int top, int newWidth, int newHeight, bool isPatch )
        {
            _bufferedImage = new BufferedImage( source.GetColorModel(),
                                                source.GetRaster().CreateWritableChild( left, top, newWidth, newHeight, 0, 0, null ),
                                                source.GetColorModel().IsAlphaPremultiplied(), null );

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

        public BufferedImage GetImage( ImageProcessor imageProcessor )
        {
            ArgumentNullException.ThrowIfNull( imageProcessor );

            if ( _bufferedImage != null ) return _bufferedImage;

            BufferedImage image;

            try
            {
                image = BufferedImage.Read( _file );
            }
            catch ( IOException ex )
            {
                throw new GdxRuntimeException( $"Error reading image: {_file}", ex );
            }

            if ( image == null ) throw new GdxRuntimeException( $"Unable to read image: {_file}" );

            var name = this.Name;

            if ( _isPatch ) name += ".9";

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
            if ( this == obj ) return true;
            if ( obj == null ) return false;

            if ( GetType() != obj.GetType() ) return false;

            var other = ( Rect )obj;

            if ( Name == null )
            {
                if ( other.Name != null ) return false;
            }
            else
            {
                if ( !Name.Equals( other.Name ) ) return false;
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
        public FileInfo?      FileInfo { get; set; }
        public string?        RootPath { get; set; }
        public string?        Name     { get; set; }
        public BufferedImage? Image    { get; set; }
    }

    // ========================================================================
    // ========================================================================

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
            if ( portion == 0 ) throw new ArgumentException( "portion cannot be 0." );

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
        public PixelType.Format      Format                { get; set; } = PixelType.Format.RGBA8888;
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

        public void Set( Settings settings )
        {
            Fast                  = settings.Fast;
            Rotation              = settings.Rotation;
            PowerOfTwo            = settings.PowerOfTwo;
            MultipleOfFour        = settings.MultipleOfFour;
            MinWidth              = settings.MinWidth;
            MinHeight             = settings.MinHeight;
            MaxWidth              = settings.MaxWidth;
            MaxHeight             = settings.MaxHeight;
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
    }
}