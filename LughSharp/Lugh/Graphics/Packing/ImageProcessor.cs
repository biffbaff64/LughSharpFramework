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

using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Versioning;

using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

using Image = System.Drawing.Image;

namespace LughSharp.Lugh.Graphics.Packing;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public partial class ImageProcessor
{
    public float Scale { get; set; }

    private static readonly BufferedImage _emptyImage   = new( 1, 1, PixelFormat.Format32bppArgb );
    private static          Regex  _indexPattern = MyRegex();

    private readonly TexturePacker.Settings                    _settings;
    private readonly Dictionary< string, TexturePacker.Rect? > _crcs  = [ ];
    private readonly List< TexturePacker.Rect >                _rects = [ ];

    private float      _scale      = 1;
    private Resampling _resampling = Resampling.Bicubic;

    // ========================================================================
    // ========================================================================

    public ImageProcessor( TexturePacker.Settings settings )
    {
        this._settings = settings;
    }

    public TexturePacker.Rect? AddImage( FileInfo file, string? rootPath )
    {
        BufferedImage image;

        try
        {
            image = new BufferedImage( file.FullName );
        }
        catch ( IOException ex )
        {
            throw new GdxRuntimeException( "Error reading image: " + file, ex );
        }

        if ( image == null ) throw new GdxRuntimeException( "Unable to read image: " + file );

        var name = Path.GetFullPath( file.FullName ).Replace( '\\', '/' );

        // Strip root dir off front of image path.
        if ( rootPath != null )
        {
            if ( !name.StartsWith( rootPath ) )
            {
                throw new GdxRuntimeException( "Path '" + name + "' does not start with root: " + rootPath );
            }

            name = name.Substring( rootPath.Length );
        }

        // Strip extension.
        var dotIndex = name.LastIndexOf( '.' );

        if ( dotIndex != -1 ) name = name.Substring( 0, dotIndex );

        var rect = AddImage( image, name );

        if ( ( rect != null ) && _settings.LimitMemory ) rect.UnloadImage( file );

        return rect;
    }

    public TexturePacker.Rect? AddImage( BufferedImage? image, string? name )
    {
        ArgumentNullException.ThrowIfNull( image );
        ArgumentNullException.ThrowIfNull( name );

        var rect = ProcessImage( image, name );

        if ( rect == null )
        {
            if ( !_settings.Silent ) Console.WriteLine( "Ignoring blank input image: " + name );

            return null;
        }

        if ( _settings.IsAlias )
        {
            var crc      = Hash( rect.GetImage( this ) );
            var existing = _crcs[ crc ];

            if ( existing != null )
            {
                if ( !_settings.Silent )
                {
                    var rectName     = rect.Name + ( rect.Index != -1 ? "_" + rect.Index : "" );
                    var existingName = existing.Name + ( existing.Index != -1 ? "_" + existing.Index : "" );

                    Console.WriteLine( rectName + " (alias of " + existingName + ")" );
                }

                existing.Aliases.Add( new TexturePacker.Alias( rect ) );

                return null;
            }

            _crcs[ crc ] = rect;
        }

        _rects.Add( rect );

        return rect;
    }

    public void SetResampling( Resampling resampling )
    {
        this._resampling = resampling;
    }

    public List< TexturePacker.Rect > GetImages()
    {
        return _rects;
    }

    public TexturePacker.Settings GetSettings()
    {
        return _settings;
    }

    public void Clear()
    {
        _rects.Clear();
        _crcs.Clear();
    }

    /// <summary>
    /// Returns a rect for the image describing the texture region to be packed,
    /// or null if the image should not be packed.
    /// </summary>
    public TexturePacker.Rect? ProcessImage( BufferedImage? image, string? name )
    {
        if ( _scale <= 0 ) throw new GdxRuntimeException( $"scale cannot be <= 0: {_scale}" );

        var width  = image.Width;
        var height = image.Height;

        if ( image.PixelFormat != PixelFormat.Format32bppArgb )
        {
            using ( image = new BufferedImage( width, height, PixelFormat.Format32bppArgb ) )
            using ( var g = System.Drawing.Graphics.FromImage( image ) )
            {
                g.DrawImage( image, new Rectangle( 0, 0, width, height ) );
            }
        }

        var                 isPatch = name.EndsWith( ".9" );
        int[]?              splits  = null;
        int[]?              pads    = null;
        TexturePacker.Rect? rect    = null;

        if ( isPatch )
        {
            // Strip ".9" from file name, read ninepatch split pixels, and strip ninepatch split pixels.
            name   = name.Substring( 0, name.Length - 2 );
            splits = GetSplits( image, name );
            pads   = GetPads( image, name, splits );

            // Strip split pixels.
            width  -= 2;
            height -= 2;

            using ( image = new BufferedImage( width, height, PixelFormat.Format32bppArgb ) )
            using ( var g = System.Drawing.Graphics.FromImage( image ) )
            {
                g.DrawImage( image,
                             new Rectangle( 1, 1, width + 1, height + 1 ),
                             0, 0, width, height,
                             GraphicsUnit.Pixel );
            }
        }

        // Scale image.
        if ( Math.Abs( _scale - 1f ) > Constants.FLOAT_TOLERANCE )
        {
            width  = ( int )Math.Max( 1, Math.Round( width * _scale ) );
            height = ( int )Math.Max( 1, Math.Round( height * _scale ) );

            var newImage = new BufferedImage( width, height, PixelFormat.Format32bppArgb );

            if ( _scale < 1 )
            {
                newImage.GetGraphics().drawImage( image.getScaledInstance( width, height, Image.SCALE_AREA_AVERAGING ), 0, 0, null );
            }
            else
            {
//                Graphics2D g = ( Graphics2D )newImage.getGraphics();
//                g.setRenderingHint( RenderingHints.KEY_RENDERING, RenderingHints.VALUE_RENDER_QUALITY );
//                g.setRenderingHint( RenderingHints.KEY_INTERPOLATION, resampling.value );
//                g.drawImage( image, 0, 0, width, height, null );

//                System.Drawing.Graphics g = newImage.GetGraphics();

//                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
//                g.InterpolationMode  = _resampling.ToInterpolationMode();
//                g.DrawImage( image, Rectangle( 0, 0, width, height ), 0, 0, width, height, g.InterpolationMode 
            }

            image = newImage;
        }

        // Strip digits off end of name and use as index.
        var index = -1;

        if ( _settings.UseIndexes )
        {
            var match = _indexPattern.Match( name );

            if ( match.Success )
            {
                name = match.Groups[ 1 ].Value;

                if ( int.TryParse( match.Groups[ 2 ].Value, out index ) )
                {
                    // index now contains the parsed integer value.
                    // name now contains the first group's value.
                }
                else
                {
                    // Handle the case where the second group is not a valid integer.
                    Console.WriteLine( "Error: Index is not a valid integer." );
                }
            }
            else
            {
                // Handle the case where the pattern does not match.
                Console.WriteLine( "Pattern did not match." );
            }
        }

        if ( isPatch )
        {
            // Ninepatches aren't rotated or whitespace stripped.
            rect = new TexturePacker.Rect( image, 0, 0, width, height, true )
            {
                Splits    = splits,
                Pads      = pads,
                CanRotate = false,
            };
        }
        else
        {
            rect = StripWhitespace( name, image );

            if ( rect == null ) return null;
        }

        rect.Name  = name;
        rect.Index = index;

        return rect;
    }

    /** Strips whitespace and returns the rect, or null if the image should be ignored. */
    protected TexturePacker.Rect StripWhitespace( string name, BufferedImage source )
    {
        throw new NotImplementedException();

//        WritableRaster alphaRaster = source.getAlphaRaster();
//
//        if ( alphaRaster == null || ( !settings.stripWhitespaceX && !settings.stripWhitespaceY ) )
//            return new TexturePacker.Rect( source, 0, 0, source.getWidth(), source.getHeight(), false );
//
//        readonly byte[] a =
//        new byte[ 1 ];
//        var top    = 0;
//        int bottom = source.getHeight();
//
//        if ( settings.stripWhitespaceY )
//        {
//        outer:
//
//            for ( var y = 0; y < source.getHeight(); y++ )
//            {
//                for ( var x = 0; x < source.getWidth(); x++ )
//                {
//                    alphaRaster.getDataElements( x, y, a );
//                    int alpha              = a[ 0 ];
//                    if ( alpha < 0 ) alpha += 256;
//
//                    if ( alpha > settings.alphaThreshold ) break
//
//                    outer;
//                }
//
//                top++;
//            }
//
//        outer:
//
//            for ( int y = source.getHeight(); --y >= top; )
//            {
//                for ( var x = 0; x < source.getWidth(); x++ )
//                {
//                    alphaRaster.getDataElements( x, y, a );
//                    int alpha              = a[ 0 ];
//                    if ( alpha < 0 ) alpha += 256;
//
//                    if ( alpha > settings.alphaThreshold ) break
//
//                    outer;
//                }
//
//                bottom--;
//            }
//
//            // Leave 1px so nothing is copied into padding.
//            if ( settings.duplicatePadding )
//            {
//                if ( top > 0 ) top--;
//                if ( bottom < source.getHeight() ) bottom++;
//            }
//        }
//
//        var left  = 0;
//        int right = source.getWidth();
//
//        if ( settings.stripWhitespaceX )
//        {
//        outer:
//
//            for ( var x = 0; x < source.getWidth(); x++ )
//            {
//                for ( var y = top; y < bottom; y++ )
//                {
//                    alphaRaster.getDataElements( x, y, a );
//                    int alpha              = a[ 0 ];
//                    if ( alpha < 0 ) alpha += 256;
//
//                    if ( alpha > settings.alphaThreshold ) break
//
//                    outer;
//                }
//
//                left++;
//            }
//
//        outer:
//
//            for ( int x = source.getWidth(); --x >= left; )
//            {
//                for ( var y = top; y < bottom; y++ )
//                {
//                    alphaRaster.getDataElements( x, y, a );
//                    int alpha              = a[ 0 ];
//                    if ( alpha < 0 ) alpha += 256;
//
//                    if ( alpha > settings.alphaThreshold ) break
//
//                    outer;
//                }
//
//                right--;
//            }
//
//            // Leave 1px so nothing is copied into padding.
//            if ( settings.duplicatePadding )
//            {
//                if ( left > 0 ) left--;
//                if ( right < source.getWidth() ) right++;
//            }
//        }
//
//        var newWidth  = right - left;
//        var newHeight = bottom - top;
//
//        if ( newWidth <= 0 || newHeight <= 0 )
//        {
//            if ( _settings.ignoreBlankImages )
//                return null;
//            else
//                return new TexturePacker.Rect( emptyImage, 0, 0, 1, 1, false );
//        }
//
//        return new TexturePacker.Rect( source, left, top, newWidth, newHeight, false );
    }

    private static string SplitError( int x, int y, int[] rgba, string name )
    {
        throw new GdxRuntimeException( "Invalid " + name + " ninepatch split pixel at " + x + ", " + y + ", rgba: " + rgba[ 0 ] + ", "
                                       + rgba[ 1 ] + ", " + rgba[ 2 ] + ", " + rgba[ 3 ] );
    }

    /** Returns the splits, or null if the image had no splits or the splits were only a single region. Splits are an int[4] that
     * has left, right, top, bottom. */
    private int[] GetSplits( BufferedImage image, string name )
    {
        throw new NotImplementedException();

//        WritableRaster raster = image.getRaster();
//
//        var startX = getSplitPoint( raster, name, 1, 0, true, true );
//        var endX   = getSplitPoint( raster, name, startX, 0, false, true );
//        var startY = getSplitPoint( raster, name, 0, 1, true, false );
//        var endY   = getSplitPoint( raster, name, 0, startY, false, false );
//
//        // Ensure pixels after the end are not invalid.
//        getSplitPoint( raster, name, endX + 1, 0, true, true );
//        getSplitPoint( raster, name, 0, endY + 1, true, false );
//
//        // No splits, or all splits.
//        if ( startX == 0 && endX == 0 && startY == 0 && endY == 0 ) return null;
//
//        // Subtraction here is because the coordinates were computed before the 1px border was stripped.
//        if ( startX != 0 )
//        {
//            startX--;
//            endX = raster.getWidth() - 2 - ( endX - 1 );
//        }
//        else
//        {
//            // If no start point was ever found, we assume full stretch.
//            endX = raster.getWidth() - 2;
//        }
//
//        if ( startY != 0 )
//        {
//            startY--;
//            endY = raster.getHeight() - 2 - ( endY - 1 );
//        }
//        else
//        {
//            // If no start point was ever found, we assume full stretch.
//            endY = raster.getHeight() - 2;
//        }
//
//        if ( scale != 1 )
//        {
//            startX = ( int )Math.round( startX * scale );
//            endX   = ( int )Math.round( endX * scale );
//            startY = ( int )Math.round( startY * scale );
//            endY   = ( int )Math.round( endY * scale );
//        }
//
//        return new int[] { startX, endX, startY, endY };
    }

    /** Returns the pads, or null if the image had no pads or the pads match the splits. Pads are an int[4] that has left, right,
     * top, bottom. */
    private int[] GetPads( BufferedImage image, string name, int[] splits )
    {
        throw new NotImplementedException();

//        WritableRaster raster = image.getRaster();
//
//        var bottom = raster.getHeight() - 1;
//        var right  = raster.getWidth() - 1;
//
//        var startX = getSplitPoint( raster, name, 1, bottom, true, true );
//        var startY = getSplitPoint( raster, name, right, 1, true, false );
//
//        // No need to hunt for the end if a start was never found.
//        var endX                = 0;
//        var endY                = 0;
//        if ( startX != 0 ) endX = getSplitPoint( raster, name, startX + 1, bottom, false, true );
//        if ( startY != 0 ) endY = getSplitPoint( raster, name, right, startY + 1, false, false );
//
//        // Ensure pixels after the end are not invalid.
//        getSplitPoint( raster, name, endX + 1, bottom, true, true );
//        getSplitPoint( raster, name, right, endY + 1, true, false );
//
//        // No pads.
//        if ( startX == 0 && endX == 0 && startY == 0 && endY == 0 )
//        {
//            return null;
//        }
//
//        // -2 here is because the coordinates were computed before the 1px border was stripped.
//        if ( startX == 0 && endX == 0 )
//        {
//            startX = -1;
//            endX   = -1;
//        }
//        else
//        {
//            if ( startX > 0 )
//            {
//                startX--;
//                endX = raster.getWidth() - 2 - ( endX - 1 );
//            }
//            else
//            {
//                // If no start point was ever found, we assume full stretch.
//                endX = raster.getWidth() - 2;
//            }
//        }
//
//        if ( startY == 0 && endY == 0 )
//        {
//            startY = -1;
//            endY   = -1;
//        }
//        else
//        {
//            if ( startY > 0 )
//            {
//                startY--;
//                endY = raster.getHeight() - 2 - ( endY - 1 );
//            }
//            else
//            {
//                // If no start point was ever found, we assume full stretch.
//                endY = raster.getHeight() - 2;
//            }
//        }
//
//        if ( _scale != 1 )
//        {
//            startX = ( int )Math.Round( startX * _scale );
//            endX   = ( int )Math.Round( endX * _scale );
//            startY = ( int )Math.Round( startY * _scale );
//            endY   = ( int )Math.Round( endY * _scale );
//        }
//
//        var pads = new[] { startX, endX, startY, endY };
//
//        if ( splits != null && Array.Equals( pads, splits ) )
//        {
//            return null;
//        }
//
//        return pads;
    }

    /** Hunts for the start or end of a sequence of split pixels. Begins searching at (startX, startY) then follows along the x or
     * y axis (depending on value of xAxis) for the first non-transparent pixel if startPoint is true, or the first transparent
     * pixel if startPoint is false. Returns 0 if none found, as 0 is considered an invalid split point being in the outer border
     * which will be stripped. */
    private static int GetSplitPoint( WritableRaster raster, string name, int startX, int startY, bool startPoint,
                                      bool xAxis )
    {
        throw new NotImplementedException();

//        var rgba = new int[ 4 ];
//
//        var next   = xAxis ? startX : startY;
//        int end    = xAxis ? raster.getWidth() : raster.getHeight();
//        var breakA = startPoint ? 255 : 0;
//
//        var x = startX;
//        var y = startY;
//
//        while ( next != end )
//        {
//            if ( xAxis )
//                x = next;
//            else
//                y = next;
//
//            raster.getPixel( x, y, rgba );
//
//            if ( rgba[ 3 ] == breakA ) return next;
//
//            if ( !startPoint && ( rgba[ 0 ] != 0 || rgba[ 1 ] != 0 || rgba[ 2 ] != 0 || rgba[ 3 ] != 255 ) ) splitError( x, y, rgba, name );
//
//            next++;
//        }
//
//        return 0;
    }

    private static string Hash( BufferedImage image )
    {
        throw new NotImplementedException();

//        try
//        {
//            MessageDigest digest = MessageDigest.getInstance( "SHA1" );
//
//            // Ensure image is the correct format.
//            int width  = image.getWidth();
//            int height = image.getHeight();
//
//            if ( image.getType() != BufferedImage.TYPE_INT_ARGB )
//            {
//                var newImage = new BufferedImage( width, height, BufferedImage.TYPE_INT_ARGB );
//                newImage.getGraphics().drawImage( image, 0, 0, null );
//                image = newImage;
//            }
//
//            Image.WritableRaster raster = image.getRaster();
//            var            pixels = new int[ width ];
//
//            for ( var y = 0; y < height; y++ )
//            {
//                raster.getDataElements( 0, y, width, 1, pixels );
//                for ( var x = 0; x < width; x++ )
//                    hash( digest, pixels[ x ] );
//            }
//
//            hash( digest, width );
//            hash( digest, height );
//
//            return new BigInteger( 1, digest.digest() ).ToString( 16 );
//        }
//        catch ( Exception ex )  // Missing NoSuchAlgorithmException
//        {
//            throw new GdxRuntimeException( ex. );
//        }
    }

//    private static void hash( MessageDigest digest, int value )
//    {
//        digest.update( ( byte )( value >> 24 ) );
//        digest.update( ( byte )( value >> 16 ) );
//        digest.update( ( byte )( value >> 8 ) );
//        digest.update( ( byte )value );
//    }

    // ========================================================================

    [GeneratedRegex( "(.+)_(\\d+)$" )]
    private static partial Regex MyRegex();
}