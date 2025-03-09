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

using System.Drawing.Drawing2D;

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics.Atlases;
using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;
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

    public TexturePacker( FileInfo? rootDir, Settings settings )
    {
        this._settings = settings;

        if ( settings.pot )
        {
            if ( settings.maxWidth != MathUtils.NextPowerOfTwo( settings.maxWidth ) )
            {
                throw new GdxRuntimeException( $"If pot is true, maxWidth must be a power of two: {settings.maxWidth}" );
            }

            if ( settings.maxHeight != MathUtils.NextPowerOfTwo( settings.maxHeight ) )
            {
                throw new GdxRuntimeException( $"If pot is true, maxHeight must be a power of two: {settings.maxHeight}" );
            }
        }

        if ( settings.multipleOfFour )
        {
            if ( ( settings.maxWidth % 4 ) != 0 )
            {
                throw new GdxRuntimeException( $"If mod4 is true, maxWidth must be evenly divisible by 4: {settings.maxWidth}" );
            }

            if ( ( settings.maxHeight % 4 ) != 0 )
            {
                throw new GdxRuntimeException( $"If mod4 is true, maxHeight must be evenly divisible by 4: {settings.maxHeight}" );
            }
        }

        if ( settings.grid )
        {
            _packer = new GridPacker( settings );
        }
        else
        {
            _packer = new MaxRectsPacker( settings );
        }

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

        try
        {
            RootPath = rootDir.GetCanonicalPath();
        }
        catch ( IOException ex )
        {
            RootPath = rootDir.GetAbsolutePath();
        }

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

    public void Pack( FileInfo outputDir, string packFileName )
    {
        if ( packFileName.EndsWith( _settings.atlasExtension ) )
        {
            packFileName = packFileName.Substring( 0, packFileName.Length - _settings.atlasExtension.Length );
        }

        outputDir.mkdirs();

//        if ( _progressListener == null )
//        {
//            _progressListener = new ProgressListener()
//            {
//                public void Progress(float progress)
//                {
//                }
//            };
//        }

        _progressListener.Start( 1 );
        var n = _settings.scale.Length;

        for ( var i = 0; i < n; i++ )
        {
            _progressListener.Start( 1f / n );

            _imageProcessor.SetScale( _settings.scale[ i ] );

            if ( ( _settings.scaleResampling != null )
                 && ( _settings.scaleResampling.Length > i )
                 && ( _settings.scaleResampling[ i ] != null ) )
            {
                _imageProcessor.SetResampling( _settings.scaleResampling[ i ] );
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
            _progressListener.Total = _imageProcessor.Images.size;

            var pages = _packer.Pack( _progressListener, _imageProcessor.Images );

            _progressListener.End();

            _progressListener.Start( 0.29f );
            _progressListener.Count = 0;
            _progressListener.Total = pages.Count;

            var scaledPackFileName = _settings.GetScaledPackFileName( packFileName, i );

            writeImages( outputDir, scaledPackFileName, pages );

            _progressListener.End();

            _progressListener.Start( 0.01f );

            try
            {
                writePackFile( outputDir, scaledPackFileName, pages );
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

    private void writeImages( FileInfo outputDir, string scaledPackFileName, List< Page > pages )
    {
        var      packFileNoExt = new FileInfo( outputDir, scaledPackFileName );
        FileInfo packDir       = packFileNoExt.getParentFile();
        string   imageName     = packFileNoExt.getName();

        var fileIndex = 1;

        for ( int p = 0, pn = pages.Count; p < pn; p++ )
        {
            var page = pages[ p ];

            var width  = page.Width;
            var height = page.Height;

            if ( _settings.edgePadding )
            {
                var edgePadX = _settings.paddingX;
                var edgePadY = _settings.paddingY;

                if ( _settings.duplicatePadding )
                {
                    edgePadX /= 2;
                    edgePadY /= 2;
                }

                page.X =  edgePadX;
                page.Y =  edgePadY;
                width  += edgePadX * 2;
                height += edgePadY * 2;
            }

            if ( _settings.pot )
            {
                width  = MathUtils.NextPowerOfTwo( width );
                height = MathUtils.NextPowerOfTwo( height );
            }

            if ( _settings.multipleOfFour )
            {
                width  = ( width % 4 ) == 0 ? width : ( width + 4 ) - ( width % 4 );
                height = ( height % 4 ) == 0 ? height : ( height + 4 ) - ( height % 4 );
            }

            width            = Math.Max( _settings.minWidth, width );
            height           = Math.Max( _settings.minHeight, height );
            page.ImageWidth  = width;
            page.ImageHeight = height;

            FileInfo outputFile;

            while ( true )
            {
                var name = imageName;

                if ( fileIndex > 1 )
                {
                    // Last character is a digit or a digit + 'x'.
                    char last = name.charAt( name.Length - 1 );

                    if ( Character.isDigit( last )
                         || ( ( name.Length > 3 ) && ( last == 'x' ) && Character.isDigit( name.charAt( name.Length - 2 ) ) ) )
                    {
                        name += "-";
                    }

                    name += fileIndex;
                }

                fileIndex++;
                outputFile = new FileInfo( packDir, name + "." + _settings.outputFormat );

                if ( !outputFile.Exists() ) break;
            }

            new FileHandle( outputFile ).parent().mkdirs();
            page.imageName = outputFile.getName();

            var        canvas = new BufferedImage( width, height, getBufferedImageType( settings.format ) );
            Graphics2D g      = ( Graphics2D )canvas.getGraphics();

            if ( !settings.silent ) System.out.
            println( "Writing " + canvas.getWidth() + "x" + canvas.getHeight() + ": " + outputFile );

            _progressListener.start( 1 / ( float )pn );

            for ( int r = 0, rn = page.outputRects.size; r < rn; r++ )
            {
                Rect rect  = page.OutputRects[ r ];
                var  image = rect.getImage( _imageProcessor );
                int  iw    = image.Width;
                int  ih    = image.Height;
                int  rectX = page.X + rect.X;
                var  rectY = ( page.Y + page.Height ) - rect.Y - ( rect.Height - _settings.paddingY );

                if ( _settings.duplicatePadding )
                {
                    var amountX = _settings.paddingX / 2;
                    var amountY = _settings.paddingY / 2;

                    if ( rect.Rotated )
                    {
                        // Copy corner pixels to fill corners of the padding.
                        for ( var i = 1; i <= amountX; i++ )
                        {
                            for ( var j = 1; j <= amountY; j++ )
                            {
                                plot( canvas, rectX - j, ( ( rectY + iw ) - 1 ) + i, image.GetRGB( 0, 0 ) );
                                plot( canvas, ( ( rectX + ih ) - 1 ) + j, ( ( rectY + iw ) - 1 ) + i, image.GetRGB( 0, ih - 1 ) );
                                plot( canvas, rectX - j, rectY - i, image.GetRGB( iw - 1, 0 ) );
                                plot( canvas, ( ( rectX + ih ) - 1 ) + j, rectY - i, image.GetRGB( iw - 1, ih - 1 ) );
                            }
                        }

                        // Copy edge pixels into padding.
                        for ( var i = 1; i <= amountY; i++ )
                        {
                            for ( var j = 0; j < iw; j++ )
                            {
                                plot( canvas, rectX - i, ( rectY + iw ) - 1 - j, image.GetRGB( j, 0 ) );
                                plot( canvas, ( ( rectX + ih ) - 1 ) + i, ( rectY + iw ) - 1 - j, image.GetRGB( j, ih - 1 ) );
                            }
                        }

                        for ( var i = 1; i <= amountX; i++ )
                        {
                            for ( var j = 0; j < ih; j++ )
                            {
                                plot( canvas, rectX + j, rectY - i, image.GetRGB( iw - 1, j ) );
                                plot( canvas, rectX + j, ( ( rectY + iw ) - 1 ) + i, image.GetRGB( 0, j ) );
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
                                plot( canvas, rectX - i, rectY - j, image.GetRGB( 0, 0 ) );
                                plot( canvas, rectX - i, ( ( rectY + ih ) - 1 ) + j, image.GetRGB( 0, ih - 1 ) );
                                plot( canvas, ( ( rectX + iw ) - 1 ) + i, rectY - j, image.GetRGB( iw - 1, 0 ) );
                                plot( canvas, ( ( rectX + iw ) - 1 ) + i, ( ( rectY + ih ) - 1 ) + j, image.GetRGB( iw - 1, ih - 1 ) );
                            }
                        }

                        // Copy edge pixels into padding.
                        for ( var i = 1; i <= amountY; i++ )
                        {
                            copy( image, 0, 0, iw, 1, canvas, rectX, rectY - i, rect.Rotated );
                            copy( image, 0, ih - 1, iw, 1, canvas, rectX, ( ( rectY + ih ) - 1 ) + i, rect.Rotated );
                        }

                        for ( var i = 1; i <= amountX; i++ )
                        {
                            copy( image, 0, 0, 1, ih, canvas, rectX - i, rectY, rect.Rotated );
                            copy( image, iw - 1, 0, 1, ih, canvas, ( ( rectX + iw ) - 1 ) + i, rectY, rect.Rotated );
                        }
                    }
                }

                copy( image, 0, 0, iw, ih, canvas, rectX, rectY, rect.Rotated );

                if ( _settings.debug )
                {
                    g.setColor( Color.Magenta );
                    g.drawRect( rectX, rectY, rect.Width - _settings.paddingX - 1, rect.Height - _settings.paddingY - 1 );
                }

                if ( _progressListener.Update( r + 1, rn ) )
                {
                    return;
                }
            }

            _progressListener.End();

            if ( _settings is { bleed: true, premultiplyAlpha: false }
                 && !( string.Equals( _settings.outputFormat, "jpg", StringComparison.Ordinal )
                       || string.Equals( _settings.outputFormat, "jpeg", StringComparison.Ordinal ) )
            {
                canvas = new ColorBleedEffect().ProcessImage( canvas, _settings.bleedIterations );
                g      = ( Graphics2D )canvas.getGraphics();
            }

            if ( _settings.debug )
            {
                g.setColor( Color.magenta );
                g.drawRect( 0, 0, width - 1, height - 1 );
            }

            ImageOutputStream ios = null;

            try
            {
                if ( _settings.outputFormat.equalsIgnoreCase( "jpg" ) || _settings.outputFormat.equalsIgnoreCase( "jpeg" ) )
                {
                    var newImage = new BufferedImage( canvas.getWidth(), canvas.getHeight(), BufferedImage.TYPE_3BYTE_BGR );
                    newImage.getGraphics().drawImage( canvas, 0, 0, null );
                    canvas = newImage;

                    Iterator< ImageWriter > writers = ImageIO.getImageWritersByFormatName( "jpg" );
                    ImageWriter             writer  = writers.next();
                    ImageWriteParam         param   = writer.getDefaultWriteParam();
                    param.setCompressionMode( ImageWriteParam.MODE_EXPLICIT );
                    param.setCompressionQuality( _settings.jpegQuality );
                    ios = ImageIO.createImageOutputStream( outputFile );
                    writer.setOutput( ios );
                    writer.write( null, new IIOImage( canvas, null, null ), param );
                }
                else
                {
                    if ( _settings.premultiplyAlpha ) canvas.getColorModel().coerceData( canvas.getRaster(), true );
                    ImageIO.write( canvas, "png", outputFile );
                }
            }
            catch ( IOException ex )
            {
                throw new GdxRuntimeException( "Error writing file: " + outputFile, ex );
            }
            finally
            {
                if ( ios != null )
                {
                    try
                    {
                        ios.close();
                    }
                    catch ( Exception ignored )
                    {
                    }
                }
            }

            if ( _progressListener.Update( p + 1, pn ) ) return;

            _progressListener.count++;
        }
    }

    static private void plot( BufferedImage dst, int x, int y, int argb )
    {
        if ( ( 0 <= x ) && ( x < dst.getWidth() ) && ( 0 <= y ) && ( y < dst.getHeight() ) ) dst.setRGB( x, y, argb );
    }

    static private void copy( BufferedImage src, int x, int y, int w, int h, BufferedImage dst, int dx, int dy, bool Rotated )
    {
        if ( Rotated )
        {
            for ( var i = 0; i < w; i++ )
                for ( var j = 0; j < h; j++ )
                    plot( dst, dx + j, ( dy + w ) - i - 1, src.GetRGB( x + i, y + j ) );
        }
        else
        {
            for ( var i = 0; i < w; i++ )
                for ( var j = 0; j < h; j++ )
                    plot( dst, dx + i, dy + j, src.GetRGB( x + i, y + j ) );
        }
    }

    private void writePackFile( FileInfo outputDir, string scaledPackFileName, List< Page > pages )
    {
        var packFile = new
            FileInfo( outputDir, scaledPackFileName + _settings.atlasExtension );
        FileInfo packDir = packFile.getParentFile();
        packDir.mkdirs();

        if ( packFile.exists() )
        {
            // Make sure there aren't duplicate names.
            var textureAtlasData = new TextureAtlasData( new FileHandle( packFile ), new FileHandle( packFile ), false );
            for ( Page page :
            pages) {
                for ( Rect rect :
                page.outputRects)
                {
                    string rectName = Rect.getAtlasName( rect.name, _settings.flattenPaths );
                    for ( Region region :
                    textureAtlasData.getRegions()) {
                        if ( region.name.equals( rectName ) )
                        {
                            throw new GdxRuntimeException( "A region with the name \"" + rectName + "\" has already been packed: " +
                                                           rect.name );
                        }
                    }
                }
            }
        }

        string tab = "", colon = ":", comma = ",";

        if ( _settings.prettyPrint )
        {
            tab   = "\t";
            colon = ": ";
            comma = ", ";
        }

        bool               appending = packFile.exists();
        OutputStreamWriter writer    = new OutputStreamWriter( new FileOutputStream( packFile, true ), "UTF-8" );

        for ( int i = 0, n = pages.size; i < n; i++ )
        {
            Page page = pages.get( i );

            if ( _settings.legacyOutput )
                WritePageLegacy( writer, page );
            else
            {
                if ( ( i != 0 ) || appending ) writer.write( "\n" );
                writePage( writer, appending, page );
            }

            page.outputRects.sort();
            for ( Rect rect :
            page.outputRects) {
                if ( _settings.legacyOutput )
                    WriteRectLegacy( writer, page, rect, rect.name );
                else
                    writeRect( writer, page, rect, rect.name );
                List< Alias > aliases = new List( rect.aliases.toList() );
                aliases.sort();
                for ( Alias alias :
                aliases) {
                    var aliasRect = new Rect();
                    aliasRect.set( rect );
                    alias.apply( aliasRect );
                    if ( _settings.legacyOutput )
                        WriteRectLegacy( writer, page, aliasRect, alias.name );
                    else
                        writeRect( writer, page, aliasRect, alias.name );
                }
            }
        }

        writer.close();
    }

    private void writePage( OutputStreamWriter writer, bool appending, Page page )
    {
        string tab = "", colon = ":", comma = ",";

        if ( _settings.prettyPrint )
        {
            tab   = "\t";
            colon = ": ";
            comma = ", ";
        }

        writer.write( page.imageName + "\n" );
        writer.write( tab + "size" + colon + page.imageWidth + comma + page.imageHeight + "\n" );

        if ( _settings.format != PixelType.Format.RGBA8888 ) writer.write( tab + "format" + colon + _settings.format + "\n" );

        if ( ( _settings.filterMin != Texture.TextureFilter.Nearest ) || ( _settings.filterMag != Texture.TextureFilter.Nearest ) )
            writer.write( tab + "filter" + colon + _settings.filterMin + comma + _settings.filterMag + "\n" );

        var repeatValue = GetRepeatValue();
        if ( repeatValue != null ) writer.write( tab + "repeat" + colon + repeatValue + "\n" );

        if ( _settings.premultiplyAlpha ) writer.write( tab + "pma" + colon + "true\n" );
    }

    private void writeRect( Writer writer, Page page, Rect rect, string name )
    {
        string tab = "", colon = ":", comma = ",";

        if ( _settings.prettyPrint )
        {
            tab   = "\t";
            colon = ": ";
            comma = ", ";
        }

        writer.write( Rect.getAtlasName( name, _settings.flattenPaths ) + "\n" );
        if ( rect.index != -1 ) writer.write( tab + "index" + colon + rect.index + "\n" );

        writer.write( tab + "bounds" + colon //
                      + ( page.x + rect.x ) + comma + ( ( page.y + page.height ) - rect.y - ( rect.height - _settings.paddingY ) ) +
                      comma //
                      + rect.regionWidth + comma + rect.regionHeight + "\n" );

        int offsetY = rect.originalHeight - rect.regionHeight - rect.offsetY;

        if ( ( rect.offsetX != 0 ) || ( offsetY != 0 ) //
                                   || ( rect.originalWidth != rect.regionWidth ) || ( rect.originalHeight != rect.regionHeight ) )
        {
            writer.write( tab + "offsets" + colon                  //
                          + rect.offsetX + comma + offsetY + comma //
                          + rect.originalWidth + comma + rect.originalHeight + "\n" );
        }

        if ( rect.Rotated ) writer.write( tab + "rotate" + colon + rect.Rotated + "\n" );

        if ( rect.splits != null )
        {
            writer.write( tab + "split" + colon                                 //
                          + rect.splits[ 0 ] + comma + rect.splits[ 1 ] + comma //
                          + rect.splits[ 2 ] + comma + rect.splits[ 3 ] + "\n" );
        }

        if ( rect.pads != null )
        {
            if ( rect.splits == null ) writer.write( tab + "split" + colon + "0" + comma + "0" + comma + "0" + comma + "0\n" );
            writer.write(
                         tab + "pad" + colon + rect.pads[ 0 ] + comma + rect.pads[ 1 ] + comma + rect.pads[ 2 ] + comma + rect.pads[ 3 ] +
                         "\n" );
        }
    }

    private void WritePageLegacy( OutputStreamWriter writer, Page page )
    {
        writer.write( "\n" + page.imageName + "\n" );
        writer.write( "size: " + page.imageWidth + ", " + page.imageHeight + "\n" );
        writer.write( "format: " + settings.format + "\n" );
        writer.write( "filter: " + settings.filterMin + ", " + settings.filterMag + "\n" );
        var repeatValue = GetRepeatValue();
        writer.write( "repeat: " + ( repeatValue == null ? "none" : repeatValue ) + "\n" );
    }

    private void WriteRectLegacy( Writer writer, Page page, Rect rect, string name )
    {
        writer.write( Rect.getAtlasName( name, settings.flattenPaths ) + "\n" );
        writer.write( "  rotate: " + rect.Rotated + "\n" );
        writer
            .write( "  xy: " + ( page.x + rect.x ) + ", " + ( ( page.y + page.height ) - rect.y - ( rect.height - settings.paddingY ) ) +
                    "\n" );

        writer.write( "  size: " + rect.regionWidth + ", " + rect.regionHeight + "\n" );

        if ( rect.splits != null )
        {
            writer.write( "  split: " //
                          + rect.splits[ 0 ] + ", " + rect.splits[ 1 ] + ", " + rect.splits[ 2 ] + ", " + rect.splits[ 3 ] + "\n" );
        }

        if ( rect.pads != null )
        {
            if ( rect.splits == null ) writer.write( "  split: 0, 0, 0, 0\n" );
            writer.write( "  pad: " + rect.pads[ 0 ] + ", " + rect.pads[ 1 ] + ", " + rect.pads[ 2 ] + ", " + rect.pads[ 3 ] + "\n" );
        }

        writer.write( "  orig: " + rect.originalWidth + ", " + rect.originalHeight + "\n" );
        writer.write( "  offset: " + rect.offsetX + ", " + ( rect.originalHeight - rect.regionHeight - rect.offsetY ) + "\n" );
        writer.write( "  index: " + rect.index + "\n" );
    }

    private string? GetRepeatValue()
    {
        if ( ( _settings.wrapX == Texture.TextureWrap.Repeat ) && ( _settings.wrapY == Texture.TextureWrap.Repeat ) )
        {
            return "xy";
        }

        if ( ( _settings.wrapX == Texture.TextureWrap.Repeat ) && ( _settings.wrapY == Texture.TextureWrap.ClampToEdge ) )
        {
            return "x";
        }

        if ( ( _settings.wrapX == Texture.TextureWrap.ClampToEdge ) && ( _settings.wrapY == Texture.TextureWrap.Repeat ) )
        {
            return "y";
        }

        return null;
    }

    private int GetBufferedImageType( PixelType.Format format )
    {
        switch ( _settings.format )
        {
            case RGBA8888:
            case RGBA4444:
                return BufferedImage.TYPE_INT_ARGB;

            case RGB565:
            case RGB888:
                return BufferedImage.TYPE_INT_RGB;

            case Alpha:
                return BufferedImage.TYPE_BYTE_GRAY;

            default:
                throw new GdxRuntimeException( $"Unsupported format: {_settings.format}" );
        }
    }

    public void setProgressListener( ProgressListener progress )
    {
        this._progressListener = progress;
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

    [PublicAPI]
    public class Alias : IComparable< Alias >
    {
        public int    Index;
        public string Name;
        public int    OffsetX;
        public int    OffsetY;
        public int    OriginalHeight;
        public int    OriginalWidth;
        public int[]  Pads;
        public int[]  Splits;

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
            return string.Compare( Name, o?.Name, stringComparison.Ordinal );
        }
    }

    // ========================================================================

    [PublicAPI]
    public class Rect : IComparable< Rect >
    {
        private BufferedImage? _bufferedImage;
        private FileInfo       _file = null!;

        private bool    _isPatch;
        public  int     Score1;
        public  int     Score2;
        public  bool    CanRotate = true;
        public  int     Height; // Portion of page taken by this region, including padding.
        public  int     Index;
        public  string? Name = string.Empty;
        public  int     OffsetX;
        public  int     OffsetY;
        public  int     OriginalHeight;
        public  int     OriginalWidth;
        public  int[]   Pads = null!;
        public  int     RegionHeight;
        public  int     RegionWidth;
        public  bool    Rotated;

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
            X      = rect.X;
            Y      = rect.Y;
            Width  = rect.Width;
            Height = rect.Height;
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

        public BufferedImage getImage( ImageProcessor _imageProcessor )
        {
            if ( _bufferedImage != null ) return _bufferedImage;

            BufferedImage image;

            try
            {
                image = ImageIO.read( file );
            }
            catch ( IOException ex )
            {
                throw new GdxRuntimeException( "Error reading image: " + file, ex );
            }

            if ( image == null ) throw new GdxRuntimeException( "Unable to read image: " + file );

            var name             = this.Name;
            if ( _isPatch ) name += ".9";

            return _imageProcessor.ProcessImage( image, name ).GetImage( null );
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

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

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

        public override string ToString()
        {
            return $"{Name}{( Index != -1 ? $"_{Index}" : "" )}[{X},{Y} {Width}x{Height}]";
        }

        public static string GetAtlasName( string? name, bool flattenPaths )
        {
            ArgumentNullException.ThrowIfNull( name );

            return flattenPaths ? new FileInfo( name ).Name : name;
        }
    }

    // ========================================================================

    [PublicAPI]
    public interface IPacker
    {
        public List< Page > Pack( List< Rect > inputRects );

        public List< Page > Pack( ProgressListener _progressListener, List< Rect > inputRects );
    }

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

        public void Reset()
        {
            _scale  = 1;
            Message = "";
            Count   = 0;
            Total   = 0;

            Progress( 0 );
        }

        public static void Set( string msg )
        {
        }

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
            update( total == 0 ? 0 : count / ( float )total );

            return Canceled;
        }

        public void update( float percent )
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

    public class Settings
    {
        public bool                  multipleOfFour        { get; set; }
        public bool                  rotation              { get; set; }
        public bool                  pot                   { get; set; } = true;
        public int                   paddingX              { get; set; } = 2;
        public int                   paddingY              { get; set; } = 2;
        public bool                  edgePadding           { get; set; } = true;
        public bool                  duplicatePadding      { get; set; } = false;
        public int                   minWidth              { get; set; } = 16;
        public int                   minHeight             { get; set; } = 16;
        public int                   maxWidth              { get; set; } = 1024;
        public int                   maxHeight             { get; set; } = 1024;
        public bool                  square                { get; set; } = false;
        public bool                  stripWhitespaceX      { get; set; }
        public bool                  stripWhitespaceY      { get; set; }
        public int                   alphaThreshold        { get; set; }
        public Texture.TextureFilter filterMin             { get; set; } = Texture.TextureFilter.Nearest;
        public Texture.TextureFilter filterMag             { get; set; } = Texture.TextureFilter.Nearest;
        public Texture.TextureWrap   wrapX                 { get; set; } = Texture.TextureWrap.ClampToEdge;
        public Texture.TextureWrap   wrapY                 { get; set; } = Texture.TextureWrap.ClampToEdge;
        public PixelType.Format      format                { get; set; } = PixelType.Format.RGBA8888;
        public bool                  alias                 { get; set; } = true;
        public string                outputFormat          { get; set; } = "png";
        public float                 jpegQuality           { get; set; } = 0.9f;
        public bool                  ignoreBlankImages     { get; set; } = true;
        public bool                  fast                  { get; set; }
        public bool                  debug                 { get; set; }
        public bool                  silent                { get; set; }
        public bool                  combineSubdirectories { get; set; }
        public bool                  ignore                { get; set; }
        public bool                  flattenPaths          { get; set; }
        public bool                  premultiplyAlpha      { get; set; }
        public bool                  useIndexes            { get; set; } = true;
        public bool                  bleed                 { get; set; } = true;
        public int                   bleedIterations       { get; set; } = 2;
        public bool                  limitMemory           { get; set; } = true;
        public bool                  grid                  { get; set; }
        public float[]               scale                 { get; set; } = [ 1 ];
        public string[]              scaleSuffix           { get; set; } = [ "" ];
        public Resampling[]          scaleResampling       { get; set; } = [ Resampling.Bicubic ];
        public string                atlasExtension        { get; set; } = ".atlas";
        public bool                  prettyPrint           { get; set; } = true;
        public bool                  legacyOutput          { get; set; } = true;

        public Settings()
        {
        }

        public Settings( Settings settings )
        {
            Set( settings );
        }

        public void Set( Settings settings )
        {
            fast                  = settings.fast;
            rotation              = settings.rotation;
            pot                   = settings.pot;
            multipleOfFour        = settings.multipleOfFour;
            minWidth              = settings.minWidth;
            minHeight             = settings.minHeight;
            maxWidth              = settings.maxWidth;
            maxHeight             = settings.maxHeight;
            paddingX              = settings.paddingX;
            paddingY              = settings.paddingY;
            edgePadding           = settings.edgePadding;
            duplicatePadding      = settings.duplicatePadding;
            alphaThreshold        = settings.alphaThreshold;
            ignoreBlankImages     = settings.ignoreBlankImages;
            stripWhitespaceX      = settings.stripWhitespaceX;
            stripWhitespaceY      = settings.stripWhitespaceY;
            alias                 = settings.alias;
            format                = settings.format;
            jpegQuality           = settings.jpegQuality;
            outputFormat          = settings.outputFormat;
            filterMin             = settings.filterMin;
            filterMag             = settings.filterMag;
            wrapX                 = settings.wrapX;
            wrapY                 = settings.wrapY;
            debug                 = settings.debug;
            silent                = settings.silent;
            combineSubdirectories = settings.combineSubdirectories;
            ignore                = settings.ignore;
            flattenPaths          = settings.flattenPaths;
            premultiplyAlpha      = settings.premultiplyAlpha;
            square                = settings.square;
            useIndexes            = settings.useIndexes;
            bleed                 = settings.bleed;
            bleedIterations       = settings.bleedIterations;
            limitMemory           = settings.limitMemory;
            grid                  = settings.grid;
            atlasExtension        = settings.atlasExtension;
            prettyPrint           = settings.prettyPrint;
            legacyOutput          = settings.legacyOutput;

            settings.scale.CopyTo( scale, 0 );
            settings.scaleSuffix.CopyTo( scaleSuffix, 0 );
            settings.scaleResampling.CopyTo( scaleResampling, 0 );
        }

        public string GetScaledPackFileName( string packFileName, int scaleIndex )
        {
            // Use suffix if not empty string.
            if ( scaleSuffix[ scaleIndex ].Length > 0 )
            {
                packFileName += scaleSuffix[ scaleIndex ];
            }
            else
            {
                // Otherwise if scale != 1 or multiple scales, use subdirectory.
                var scaleValue = scale[ scaleIndex ];

                if ( scale.Length != 1 )
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