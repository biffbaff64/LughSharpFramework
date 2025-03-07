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

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.TexturePacker;

[PublicAPI]
public class TexturePacker
{
    public string? RootPath { get; private set; }

    // ========================================================================

    private Settings?           _settings;
    private IPacker?            _packer;
    private ImageProcessor?     _imageProcessor;
    private List< InputImage >? _inputImages = [ ];
    private ProgressListener?   _progressListenerListener;

    // ========================================================================

    public TexturePacker( FileInfo? rootDir, Settings settings )
    {
        this._settings = settings;

        if ( settings.pot )
        {
            if ( settings.maxWidth != MathUtils.NextPowerOfTwo( settings.maxWidth ) )
            {
                throw new GdxRuntimeException( $"If pot is true, maxWidth must be a power of two: {settings.MaxWidth}" );
            }

            if ( settings.maxHeight != MathUtils.NextPowerOfTwo( settings.maxHeight ) )
            {
                throw new GdxRuntimeException( $"If pot is true, maxHeight must be a power of two: {settings.MaxHeight}" );
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

    protected ImageProcessor NewImageProcessor( Settings settings )
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

        if ( !RootPath.EndsWith( "/" ) )
        {
            RootPath += "/";
        }
    }

    public void AddImage( FileInfo file )
    {
        var inputImage = new InputImage()
        {
            FileInfo = file,
            RootPath = RootPath,
        };

        _inputImages?.Add( inputImage );
    }

    public void AddImage( BufferedImage image, string name )
    {
        var inputImage = new InputImage()
        {
            Image = image,
            Name  = name,
        };

        _inputImages?.Add( inputImage );
    }

    public void SetPacker( IPacker packer )
    {
        this._packer = packer;
    }

    public void Pack( FileInfo outputDir, string packFileName )
    {
        if ( packFileName.EndsWith( Settings.atlasExtension ) )
        {
            packFileName = packFileName.Substring( 0, packFileName.Length - Settings.atlasExtension.length() );
        }

        outputDir.mkdirs();

        if ( _progressListener == null )
        {
            _progressListener = new ProgressListener()
            {
 

                public void Progress(float progress)
                {
            }
            };
        }

        _progressListener.start( 1 );
        int n = settings.scale.length;

        for ( var i = 0; i < n; i++ )
        {
            _progressListener.start( 1f / n );

            _imageProcessor.setScale( settings.scale[ i ] );

            if ( settings.scaleResampling != null && settings.scaleResampling.length > i && settings.scaleResampling[ i ] != null )
                _imageProcessor.setResampling( settings.scaleResampling[ i ] );

            _progressListener.start( 0.35f );
            _progressListener.count = 0;
            _progressListener.total = inputImages.size;

            for ( int ii = 0, nn = inputImages.size; ii < nn; ii++, _progressListener.count++ )
            {
                InputImage inputImage = inputImages.get( ii );
                if ( inputImage.file != null )
                    _imageProcessor.addImage( inputImage.file, inputImage.rootPath );
                else
                    _imageProcessor.addImage( inputImage.image, inputImage.name );

                if ( _progressListener.update( ii + 1, nn ) ) return;
            }

            _progressListener.end();

            _progressListener.start( 0.35f );
            _progressListener.count = 0;
            _progressListener.total = _imageProcessor.getImages().size;
            List< Page > pages = packer.pack( _progressListener, _imageProcessor.getImages() );
            _progressListener.end();

            _progressListener.start( 0.29f );
            _progressListener.count = 0;
            _progressListener.total = pages.size;
            string scaledPackFileName = settings.getScaledPackFileName( packFileName, i );
            writeImages( outputDir, scaledPackFileName, pages );
            _progressListener.end();

            _progressListener.start( 0.01f );

            try
            {
                writePackFile( outputDir, scaledPackFileName, pages );
            }
            catch ( IOException ex )
            {
                throw new GdxRuntimeException( "Error writing pack file.", ex );
            }

            _imageProcessor.clear();
            _progressListener.end();

            _progressListener.end();

            if ( _progressListener.update( i + 1, n ) ) return;
        }

        _progressListener.end();
    }

    private void writeImages( FileInfo outputDir, string scaledPackFileName, List< Page > pages )
    {
        var      packFileNoExt = new FileInfo( outputDir, scaledPackFileName );
        FileInfo packDir       = packFileNoExt.getParentFile();
        string   imageName     = packFileNoExt.getName();

        var fileIndex = 1;

        for ( int p = 0, pn = pages.size; p < pn; p++ )
        {
            Page page = pages.get( p );

            int width    = page.width, height   = page.height;
            int edgePadX = 0,          edgePadY = 0;

            if ( settings.edgePadding )
            {
                edgePadX = settings.paddingX;
                edgePadY = settings.paddingY;

                if ( settings.duplicatePadding )
                {
                    edgePadX /= 2;
                    edgePadY /= 2;
                }

                page.x =  edgePadX;
                page.y =  edgePadY;
                width  += edgePadX * 2;
                height += edgePadY * 2;
            }

            if ( settings.pot )
            {
                width  = MathUtils.NextPowerOfTwo( width );
                height = MathUtils.NextPowerOfTwo( height );
            }

            if ( settings.multipleOfFour )
            {
                width  = width % 4 == 0 ? width : width + 4 - ( width % 4 );
                height = height % 4 == 0 ? height : height + 4 - ( height % 4 );
            }

            width            = Math.Max( _settings.minWidth, width );
            height           = Math.Max( _settings.minHeight, height );
            page.imageWidth  = width;
            page.imageHeight = height;

            FileInfo outputFile;

            while ( true )
            {
                var name = imageName;

                if ( fileIndex > 1 )
                {
                    // Last character is a digit or a digit + 'x'.
                    char last = name.charAt( name.length() - 1 );

                    if ( Character.isDigit( last )
                         || ( name.length() > 3 && last == 'x' && Character.isDigit( name.charAt( name.length() - 2 ) ) ) )
                    {
                        name += "-";
                    }

                    name += fileIndex;
                }

                fileIndex++;
                outputFile = new FileInfo( packDir, name + "." + settings.outputFormat );

                if ( !outputFile.exists() ) break;
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
                Rect          rect  = page.outputRects.get( r );
                BufferedImage image = rect.getImage( _imageProcessor );
                int           iw    = image.getWidth();
                int           ih    = image.getHeight();
                int           rectX = page.x + rect.x, rectY = page.y + page.height - rect.y - ( rect.height - settings.paddingY );

                if ( settings.duplicatePadding )
                {
                    var amountX = settings.paddingX / 2;
                    var amountY = settings.paddingY / 2;

                    if ( rect.rotated )
                    {
                        // Copy corner pixels to fill corners of the padding.
                        for ( var i = 1; i <= amountX; i++ )
                        {
                            for ( var j = 1; j <= amountY; j++ )
                            {
                                plot( canvas, rectX - j, rectY + iw - 1 + i, image.getRGB( 0, 0 ) );
                                plot( canvas, rectX + ih - 1 + j, rectY + iw - 1 + i, image.getRGB( 0, ih - 1 ) );
                                plot( canvas, rectX - j, rectY - i, image.getRGB( iw - 1, 0 ) );
                                plot( canvas, rectX + ih - 1 + j, rectY - i, image.getRGB( iw - 1, ih - 1 ) );
                            }
                        }

                        // Copy edge pixels into padding.
                        for ( var i = 1; i <= amountY; i++ )
                        {
                            for ( var j = 0; j < iw; j++ )
                            {
                                plot( canvas, rectX - i, rectY + iw - 1 - j, image.getRGB( j, 0 ) );
                                plot( canvas, rectX + ih - 1 + i, rectY + iw - 1 - j, image.getRGB( j, ih - 1 ) );
                            }
                        }

                        for ( var i = 1; i <= amountX; i++ )
                        {
                            for ( var j = 0; j < ih; j++ )
                            {
                                plot( canvas, rectX + j, rectY - i, image.getRGB( iw - 1, j ) );
                                plot( canvas, rectX + j, rectY + iw - 1 + i, image.getRGB( 0, j ) );
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
                                plot( canvas, rectX - i, rectY - j, image.getRGB( 0, 0 ) );
                                plot( canvas, rectX - i, rectY + ih - 1 + j, image.getRGB( 0, ih - 1 ) );
                                plot( canvas, rectX + iw - 1 + i, rectY - j, image.getRGB( iw - 1, 0 ) );
                                plot( canvas, rectX + iw - 1 + i, rectY + ih - 1 + j, image.getRGB( iw - 1, ih - 1 ) );
                            }
                        }

                        // Copy edge pixels into padding.
                        for ( var i = 1; i <= amountY; i++ )
                        {
                            copy( image, 0, 0, iw, 1, canvas, rectX, rectY - i, rect.rotated );
                            copy( image, 0, ih - 1, iw, 1, canvas, rectX, rectY + ih - 1 + i, rect.rotated );
                        }

                        for ( var i = 1; i <= amountX; i++ )
                        {
                            copy( image, 0, 0, 1, ih, canvas, rectX - i, rectY, rect.rotated );
                            copy( image, iw - 1, 0, 1, ih, canvas, rectX + iw - 1 + i, rectY, rect.rotated );
                        }
                    }
                }

                copy( image, 0, 0, iw, ih, canvas, rectX, rectY, rect.rotated );

                if ( settings.debug )
                {
                    g.setColor( Color.magenta );
                    g.drawRect( rectX, rectY, rect.width - settings.paddingX - 1, rect.height - settings.paddingY - 1 );
                }

                if ( _progressListener.update( r + 1, rn ) ) return;
            }

            _progressListener.end();

            if ( settings.bleed && !settings.premultiplyAlpha
                                && !( settings.outputFormat.equalsIgnoreCase( "jpg" ) ||
                                      settings.outputFormat.equalsIgnoreCase( "jpeg" ) ) )
            {
                canvas = new ColorBleedEffect().processImage( canvas, settings.bleedIterations );
                g      = ( Graphics2D )canvas.getGraphics();
            }

            if ( settings.debug )
            {
                g.setColor( Color.magenta );
                g.drawRect( 0, 0, width - 1, height - 1 );
            }

            ImageOutputStream ios = null;

            try
            {
                if ( settings.outputFormat.equalsIgnoreCase( "jpg" ) || settings.outputFormat.equalsIgnoreCase( "jpeg" ) )
                {
                    var newImage = new BufferedImage( canvas.getWidth(), canvas.getHeight(), BufferedImage.TYPE_3BYTE_BGR );
                    newImage.getGraphics().drawImage( canvas, 0, 0, null );
                    canvas = newImage;

                    Iterator< ImageWriter > writers = ImageIO.getImageWritersByFormatName( "jpg" );
                    ImageWriter             writer  = writers.next();
                    ImageWriteParam         param   = writer.getDefaultWriteParam();
                    param.setCompressionMode( ImageWriteParam.MODE_EXPLICIT );
                    param.setCompressionQuality( settings.jpegQuality );
                    ios = ImageIO.createImageOutputStream( outputFile );
                    writer.setOutput( ios );
                    writer.write( null, new IIOImage( canvas, null, null ), param );
                }
                else
                {
                    if ( settings.premultiplyAlpha ) canvas.getColorModel().coerceData( canvas.getRaster(), true );
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

            if ( _progressListener.update( p + 1, pn ) ) return;

            _progressListener.count++;
        }
    }

    static private void plot( BufferedImage dst, int x, int y, int argb )
    {
        if ( 0 <= x && x < dst.getWidth() && 0 <= y && y < dst.getHeight() ) dst.setRGB( x, y, argb );
    }

    static private void copy( BufferedImage src, int x, int y, int w, int h, BufferedImage dst, int dx, int dy, boolean rotated )
    {
        if ( rotated )
        {
            for ( var i = 0; i < w; i++ )
                for ( var j = 0; j < h; j++ )
                    plot( dst, dx + j, dy + w - i - 1, src.getRGB( x + i, y + j ) );
        }
        else
        {
            for ( var i = 0; i < w; i++ )
                for ( var j = 0; j < h; j++ )
                    plot( dst, dx + i, dy + j, src.getRGB( x + i, y + j ) );
        }
    }

    private void writePackFile( FileInfo outputDir, string scaledPackFileName, List< Page > pages )
    {
        FileInfo packFile = new
            FileInfo( outputDir, scaledPackFileName + settings.atlasExtension );
        FileInfo packDir = packFile.getParentFile();
        packDir.mkdirs();

        if ( packFile.exists() )
        {
            // Make sure there aren't duplicate names.
            TextureAtlasData textureAtlasData = new TextureAtlasData( new FileHandle( packFile ), new FileHandle( packFile ), false );
            for ( Page page :
            pages) {
                for ( Rect rect :
                page.outputRects)
                {
                    string rectName = Rect.getAtlasName( rect.name, settings.flattenPaths );
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

        if ( settings.prettyPrint )
        {
            tab   = "\t";
            colon = ": ";
            comma = ", ";
        }

        boolean            appending = packFile.exists();
        OutputStreamWriter writer    = new OutputStreamWriter( new FileOutputStream( packFile, true ), "UTF-8" );

        for ( int i = 0, n = pages.size; i < n; i++ )
        {
            Page page = pages.get( i );

            if ( settings.legacyOutput )
                WritePageLegacy( writer, page );
            else
            {
                if ( i != 0 || appending ) writer.write( "\n" );
                writePage( writer, appending, page );
            }

            page.outputRects.sort();
            for ( Rect rect :
            page.outputRects) {
                if ( settings.legacyOutput )
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
                    if ( settings.legacyOutput )
                        WriteRectLegacy( writer, page, aliasRect, alias.name );
                    else
                        writeRect( writer, page, aliasRect, alias.name );
                }
            }
        }

        writer.close();
    }

    private void writePage( OutputStreamWriter writer, boolean appending, Page page )
    {
        string tab = "", colon = ":", comma = ",";

        if ( settings.prettyPrint )
        {
            tab   = "\t";
            colon = ": ";
            comma = ", ";
        }

        writer.write( page.imageName + "\n" );
        writer.write( tab + "size" + colon + page.imageWidth + comma + page.imageHeight + "\n" );

        if ( settings.format != PixelType.Format.RGBA8888 ) writer.write( tab + "format" + colon + settings.format + "\n" );

        if ( settings.filterMin != Texture.TextureFilter.Nearest || settings.filterMag != Texture.TextureFilter.Nearest )
            writer.write( tab + "filter" + colon + settings.filterMin + comma + settings.filterMag + "\n" );

        string repeatValue = GetRepeatValue();
        if ( repeatValue != null ) writer.write( tab + "repeat" + colon + repeatValue + "\n" );

        if ( settings.premultiplyAlpha ) writer.write( tab + "pma" + colon + "true\n" );
    }

    private void writeRect( Writer writer, Page page, Rect rect, string name )
    {
        string tab = "", colon = ":", comma = ",";

        if ( settings.prettyPrint )
        {
            tab   = "\t";
            colon = ": ";
            comma = ", ";
        }

        writer.write( Rect.getAtlasName( name, settings.flattenPaths ) + "\n" );
        if ( rect.index != -1 ) writer.write( tab + "index" + colon + rect.index + "\n" );

        writer.write( tab + "bounds" + colon                                                                                          //
                      + ( page.x + rect.x ) + comma + ( page.y + page.height - rect.y - ( rect.height - settings.paddingY ) ) + comma //
                      + rect.regionWidth + comma + rect.regionHeight + "\n" );

        int offsetY = rect.originalHeight - rect.regionHeight - rect.offsetY;

        if ( rect.offsetX != 0 || offsetY != 0 //
                               || rect.originalWidth != rect.regionWidth || rect.originalHeight != rect.regionHeight )
        {
            writer.write( tab + "offsets" + colon                  //
                          + rect.offsetX + comma + offsetY + comma //
                          + rect.originalWidth + comma + rect.originalHeight + "\n" );
        }

        if ( rect.rotated ) writer.write( tab + "rotate" + colon + rect.rotated + "\n" );

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
        string repeatValue = GetRepeatValue();
        writer.write( "repeat: " + ( repeatValue == null ? "none" : repeatValue ) + "\n" );
    }

    private void WriteRectLegacy( Writer writer, Page page, Rect rect, string name )
    {
        writer.write( Rect.getAtlasName( name, settings.flattenPaths ) + "\n" );
        writer.write( "  rotate: " + rect.rotated + "\n" );
        writer
            .write( "  xy: " + ( page.x + rect.x ) + ", " + ( page.y + page.height - rect.y - ( rect.height - settings.paddingY ) ) +
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
        if ( _settings.wrapX == Texture.TextureWrap.Repeat && _settings.wrapY == Texture.TextureWrap.Repeat )
        {
            return "xy";
        }

        if ( _settings.wrapX == Texture.TextureWrap.Repeat && _settings.wrapY == Texture.TextureWrap.ClampToEdge )
        {
            return "x";
        }

        if ( _settings.wrapX == Texture.TextureWrap.ClampToEdge && _settings.wrapY == Texture.TextureWrap.Repeat )
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
        private BufferedImage image;
        private FileInfo _file = null!;

        private bool   _isPatch;
        private int    _score1;
        private int    _score2;
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

//        public Set< Alias > aliases = new HashSet< Alias >();
        public int[] Splits = null!;
        public int   Width; // Portion of page taken by this region, including padding.
        public int   X;
        public int   Y;

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
            return string.Compare( Name, o?.Name, stringComparison.Ordinal );
        }

        public Rect( BufferedImage source, int left, int top, int newWidth, int newHeight, bool isPatch )
        {
            image = new BufferedImage( source.getColorModel(),
                                       source.getRaster().createWritableChild( left, top, newWidth, newHeight, 0, 0, null ),
                                       source.getColorModel().isAlphaPremultiplied(), null );
            offsetX        = left;
            offsetY        = top;
            regionWidth    = newWidth;
            regionHeight   = newHeight;
            originalWidth  = source.getWidth();
            originalHeight = source.getHeight();
            width          = newWidth;
            height         = newHeight;
            this.isPatch   = isPatch;
        }

        public void UnloadImage( FileInfo fileInfo )
        {
            _file = fileInfo;

            image     = null;
        }

        public BufferedImage getImage( ImageProcessor _imageProcessor )
        {
            if ( image != null ) return image;

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
            string name         = this.name;
            if ( isPatch ) name += ".9";
            return _imageProcessor.ProcessImage( image, name ).getImage( null );
        }

        protected void Set( Rect rect )
        {
            Name = rect.Name;

            image          = rect.image;
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

            aliases        = rect.aliases;
            Splits    = rect.Splits;
            Pads      = rect.Pads;
            CanRotate = rect.CanRotate;
            _score1   = rect._score1;
            _score2   = rect._score2;
            _file     = rect._file;
            _isPatch  = rect._isPatch;
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

            if ( getClass() != obj.getClass() ) return false;

            var other = ( Rect )obj;

            if ( name == null )
            {
                if ( other.name != null ) return false;
            }
            else
            {
                if ( !name.Equals( other.name ) ) return false;
            }

            return true;
        }

        public override string ToString()
        {
            return $"{Name}{( Index != -1 ? $"_{Index}" : "" )}[{X},{Y} {Width}x{Height}]";
        }

        public static string GetAtlasName( string name, bool flattenPaths )
        {
            return flattenPaths ? new FileInfo( name ).Name : name;
        }
    }

    // ========================================================================

    public sealed class InputImage
    {
        public FileInfo?      FileInfo { get; set; }
        public string?        RootPath { get; set; }
        public string?        Name     { get; set; }
        public BufferedImage? Image    { get; set; }
    }

// ========================================================================

    public interface IPacker
    {
        public List< Page > Pack( List< Rect > inputRects );

        public List< Page > Pack( ProgressListener _progressListener, List< Rect > inputRects );
    }

// ========================================================================

    public enum Resampling
    {
        Nearest  = RenderingHints.VALUE_INTERPOLATION_NEAREST_NEIGHBOR,
        Bilinear = RenderingHints.VALUE_INTERPOLATION_BILINEAR,
        Bicubic  = RenderingHints.VALUE_INTERPOLATION_BICUBIC,
    }
}