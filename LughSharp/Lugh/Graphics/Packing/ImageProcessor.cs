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
using System.Numerics;
using System.Runtime.Versioning;
using System.Security.Cryptography;

using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

using Image = System.Drawing.Image;

namespace LughSharp.Lugh.Graphics.Packing;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public partial class ImageProcessor
{
    public float Scale { get; set; }

    // ========================================================================

    private static readonly Bitmap                                    _emptyImage   = new( 1, 1, PixelFormat.Format32bppArgb );
    private static          Regex                                     _indexPattern = MyRegex();
    private readonly        TexturePacker.Settings                    _settings;
    private readonly        Dictionary< string, TexturePacker.Rect? > _crcs       = [ ];
    private readonly        List< TexturePacker.Rect >                _rects      = [ ];
    private                 float                                     _scale      = 1;
    private                 Resampling                                _resampling = Resampling.Bicubic;

    // ========================================================================
    // ========================================================================

    public ImageProcessor( TexturePacker.Settings settings )
    {
        this._settings = settings;
    }

    public TexturePacker.Rect? AddImage( FileInfo file, string? rootPath )
    {
        Bitmap image;

        try
        {
            image = new Bitmap( file.Name );
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
                throw new GdxRuntimeException( $"Path '{name}' does not start with root: {rootPath}" );
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

    public TexturePacker.Rect? AddImage( Bitmap? image, string? name )
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
    public TexturePacker.Rect? ProcessImage( Bitmap? image, string? name )
    {
        ArgumentNullException.ThrowIfNull( image );
        ArgumentNullException.ThrowIfNull( name );

        if ( _scale <= 0 ) throw new GdxRuntimeException( $"scale cannot be <= 0: {_scale}" );

        var width  = image.Width;
        var height = image.Height;

        if ( image.PixelFormat != PixelFormat.Format32bppArgb )
        {
            using ( image = new Bitmap( width, height, PixelFormat.Format32bppArgb ) )
            using ( var g = System.Drawing.Graphics.FromImage( image ) )
            {
                g.DrawImage( image, 0, 0, width, height );
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

            using ( image = new Bitmap( width, height, PixelFormat.Format32bppArgb ) )
            using ( var g = System.Drawing.Graphics.FromImage( image ) )
            {
                g.DrawImage( image: image,
                             destRect: new Rectangle( 1, 1, width + 1, height + 1 ),
                             srcX: 0,
                             srcY: 0,
                             srcWidth: width,
                             srcHeight: height,
                             srcUnit: GraphicsUnit.Pixel );
            }
        }

        // Scale image.
        if ( Math.Abs( _scale - 1f ) > Number.FLOAT_TOLERANCE )
        {
            width  = ( int )Math.Max( 1, Math.Round( width * _scale ) );
            height = ( int )Math.Max( 1, Math.Round( height * _scale ) );

            using ( var newImage = new Bitmap( width, height, PixelFormat.Format32bppArgb ) )
            using ( var g = System.Drawing.Graphics.FromImage( newImage ) )
            {
                if ( _scale < 1 )
                {
                    //TODO:
//                    g.DrawImage( image.GetScaledInstance( width, height, Image.SCALE_AREA_AVERAGING ), 0, 0, null );
                }
                else
                {
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.InterpolationMode  = _resampling.ToInterpolationMode();
                    g.DrawImage( image, new Rectangle( 0, 0, width, height ), 0, 0, width, height, GraphicsUnit.Pixel );
                }

                image = newImage;
            }
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

    /// <summary>
    /// Strips whitespace and returns the rect, or null if the image should be ignored.
    /// </summary>
    protected TexturePacker.Rect? StripWhitespace( string name, Bitmap source )
    {
        BitmapData? alphaData = null;

        try
        {
            alphaData = source.LockBits( new Rectangle( 0, 0, source.Width, source.Height ), ImageLockMode.ReadOnly,
                                         PixelFormat.Format32bppArgb );

            if ( alphaData.Stride == ( source.Width * 4 ) )
            {
                if ( _settings is { StripWhitespaceX: false, StripWhitespaceY: false } )
                {
                    return new TexturePacker.Rect( source, 0, 0, source.Width, source.Height, false );
                }
            }

            int top    = 0;
            int bottom = source.Height;

            if ( _settings.StripWhitespaceY )
            {
            outer1:

                for ( int y = 0; y < source.Height; y++ )
                {
                    for ( int x = 0; x < source.Width; x++ )
                    {
                        int alpha = GetAlpha( alphaData, x, y );

                        if ( alpha > _settings.AlphaThreshold )
                        {
                            goto outer1;
                        }
                    }

                    top++;
                }

            outer2:

                for ( int y = source.Height - 1; y >= top; y-- )
                {
                    for ( int x = 0; x < source.Width; x++ )
                    {
                        int alpha = GetAlpha( alphaData, x, y );

                        if ( alpha > _settings.AlphaThreshold )
                        {
                            goto outer2;
                        }
                    }

                    bottom--;
                }

                if ( _settings.DuplicatePadding )
                {
                    if ( top > 0 )
                    {
                        top--;
                    }

                    if ( bottom < source.Height )
                    {
                        bottom++;
                    }
                }
            }

            int left  = 0;
            int right = source.Width;

            if ( _settings.StripWhitespaceX )
            {
            outer3:

                for ( int x = 0; x < source.Width; x++ )
                {
                    for ( int y = top; y < bottom; y++ )
                    {
                        int alpha = GetAlpha( alphaData, x, y );

                        if ( alpha > _settings.AlphaThreshold )
                        {
                            goto outer3;
                        }
                    }

                    left++;
                }

            outer4:

                for ( int x = source.Width - 1; x >= left; x-- )
                {
                    for ( int y = top; y < bottom; y++ )
                    {
                        int alpha = GetAlpha( alphaData, x, y );

                        if ( alpha > _settings.AlphaThreshold )
                        {
                            goto outer4;
                        }
                    }

                    right--;
                }

                if ( _settings.DuplicatePadding )
                {
                    if ( left > 0 )
                    {
                        left--;
                    }

                    if ( right < source.Width )
                    {
                        right++;
                    }
                }
            }

            int newWidth  = right - left;
            int newHeight = bottom - top;

            if ( ( newWidth <= 0 ) || ( newHeight <= 0 ) )
            {
                if ( _settings.IgnoreBlankImages )
                {
                    return null;
                }
                else
                {
                    return new TexturePacker.Rect( _emptyImage, 0, 0, 1, 1, false );
                }
            }

            return new TexturePacker.Rect( source, left, top, newWidth, newHeight, false );
        }
        finally
        {
            if ( alphaData != null )
            {
                source.UnlockBits( alphaData );
            }
        }
    }

    private unsafe int GetAlpha( BitmapData data, int x, int y )
    {
        byte* ptr      = ( byte* )data.Scan0;
        byte* pixelPtr = ptr + ( y * data.Stride ) + ( x * 4 ); // 4 bytes per pixel (ARGB)
        int   alpha    = pixelPtr[ 3 ];

        return alpha;
    }

    /// <summary>
    /// Returns the splits, or null if the image had no splits or the splits were only a single
    /// region. Splits are an int[4] that has left, right, top, bottom.
    /// </summary>
    private int[]? GetSplits( Bitmap image, string name )
    {
        var startX = GetSplitPoint( image, name, 1, 0, true, true );
        var endX   = GetSplitPoint( image, name, startX, 0, false, true );
        var startY = GetSplitPoint( image, name, 0, 1, true, false );
        var endY   = GetSplitPoint( image, name, 0, startY, false, false );

        // Ensure pixels after the end are not invalid.
        GetSplitPoint( image, name, endX + 1, 0, true, true );
        GetSplitPoint( image, name, 0, endY + 1, true, false );

        // No splits, or all splits.
        if ( ( startX == 0 ) && ( endX == 0 ) && ( startY == 0 ) && ( endY == 0 ) )
        {
            return null;
        }

        // Subtraction here is because the coordinates were computed before the 1px border was stripped.
        if ( startX != 0 )
        {
            startX--;
            endX = image.Width - 2 - ( endX - 1 );
        }
        else
        {
            // If no start point was ever found, we assume full stretch.
            endX = image.Width - 2;
        }

        if ( startY != 0 )
        {
            startY--;
            endY = image.Height - 2 - ( endY - 1 );
        }
        else
        {
            // If no start point was ever found, we assume full stretch.
            endY = image.Height - 2;
        }

        if ( Math.Abs( _scale - 1.0f ) > Number.FLOAT_TOLERANCE )
        {
            startX = ( int )Math.Round( startX * _scale );
            endX   = ( int )Math.Round( endX * _scale );
            startY = ( int )Math.Round( startY * _scale );
            endY   = ( int )Math.Round( endY * _scale );
        }

        return [ startX, endX, startY, endY ];
    }

    /// <summary>
    /// Returns the pads, or null if the image had no pads or the pads match the splits. Pads are
    /// an int[4] that has left, right, top, bottom.
    /// </summary>
    private int[]? GetPads( Bitmap image, string name, int[]? splits )
    {
        var bottom = image.Height - 1;
        var right  = image.Width - 1;

        var startX = GetSplitPoint( image, name, 1, bottom, true, true );
        var startY = GetSplitPoint( image, name, right, 1, true, false );

        // No need to hunt for the end if a start was never found.
        var endX = 0;
        var endY = 0;

        if ( startX != 0 ) endX = GetSplitPoint( image, name, startX + 1, bottom, false, true );
        if ( startY != 0 ) endY = GetSplitPoint( image, name, right, startY + 1, false, false );

        // Ensure pixels after the end are not invalid.
        GetSplitPoint( image, name, endX + 1, bottom, true, true );
        GetSplitPoint( image, name, right, endY + 1, true, false );

        // No pads.
        if ( ( startX == 0 ) && ( endX == 0 ) && ( startY == 0 ) && ( endY == 0 ) )
        {
            return null;
        }

        // -2 here is because the coordinates were computed before the 1px border was stripped.
        if ( ( startX == 0 ) && ( endX == 0 ) )
        {
            startX = -1;
            endX   = -1;
        }
        else
        {
            if ( startX > 0 )
            {
                startX--;
                endX = image.Width - 2 - ( endX - 1 );
            }
            else
            {
                // If no start point was ever found, we assume full stretch.
                endX = image.Width - 2;
            }
        }

        if ( ( startY == 0 ) && ( endY == 0 ) )
        {
            startY = -1;
            endY   = -1;
        }
        else
        {
            if ( startY > 0 )
            {
                startY--;
                endY = image.Height - 2 - ( endY - 1 );
            }
            else
            {
                // If no start point was ever found, we assume full stretch.
                endY = image.Height - 2;
            }
        }

        if ( Math.Abs( _scale - 1.0f ) > Number.FLOAT_TOLERANCE )
        {
            startX = ( int )Math.Round( startX * _scale );
            endX   = ( int )Math.Round( endX * _scale );
            startY = ( int )Math.Round( startY * _scale );
            endY   = ( int )Math.Round( endY * _scale );
        }

        var pads = new[] { startX, endX, startY, endY };

        if ( ( splits != null ) && Equals( pads, splits ) )
        {
            return null;
        }

        return pads;
    }

    /// <summary>
    /// Hunts for the start or end of a sequence of split pixels. Begins searching at (startX, startY)
    /// then follows along the x or y axis (depending on value of xAxis) for the first non-transparent
    /// pixel if startPoint is true, or the first transparent pixel if startPoint is false. Returns 0
    /// if none found, as 0 is considered an invalid split point being in the outer border which will
    /// be stripped.
    /// </summary>
    [SupportedOSPlatform( "windows" )]
    public static int GetSplitPoint( Bitmap bitmap, string name, int startX, int startY, bool startPoint, bool xAxis )
    {
        var rgba = new int[ 4 ];

        var next   = xAxis ? startX : startY;
        var end    = xAxis ? bitmap.Width : bitmap.Height;
        var breakA = startPoint ? 255 : 0;

        var x = startX;
        var y = startY;

        var bitmapData = bitmap.LockBits( new Rectangle( 0, 0, bitmap.Width, bitmap.Height ),
                                          ImageLockMode.ReadOnly,
                                          PixelFormat.Format32bppArgb );

        try
        {
            unsafe
            {
                var ptr = ( byte* )bitmapData.Scan0;

                while ( next != end )
                {
                    if ( xAxis )
                    {
                        x = next;
                    }
                    else
                    {
                        y = next;
                    }

                    var pixelPtr = ptr + ( y * bitmapData.Stride ) + ( x * 4 ); // 4 bytes per pixel (ARGB)

                    rgba[ 0 ] = pixelPtr[ 2 ]; // Red
                    rgba[ 1 ] = pixelPtr[ 1 ]; // Green
                    rgba[ 2 ] = pixelPtr[ 0 ]; // Blue
                    rgba[ 3 ] = pixelPtr[ 3 ]; // Alpha

                    if ( rgba[ 3 ] == breakA ) return next;

                    if ( !startPoint && ( ( rgba[ 0 ] != 0 ) || ( rgba[ 1 ] != 0 ) || ( rgba[ 2 ] != 0 ) || ( rgba[ 3 ] != 255 ) ) )
                    {
                        throw new GdxRuntimeException( $"Invalid {name} ninepatch split pixel at {x}, {y}, " +
                                                       $"rgba: {rgba[ 0 ]}, {rgba[ 1 ]}, {rgba[ 2 ]}, {rgba[ 3 ]}" );
                    }

                    next++;
                }
            }
        }
        finally
        {
            bitmap.UnlockBits( bitmapData );
        }

        return 0;
    }

    [SupportedOSPlatform( "windows" )]
    public static string Hash( Bitmap image )
    {
        try
        {
            using var sha1 = SHA1.Create();

            // Ensure image is the correct format.
            var width  = image.Width;
            var height = image.Height;

            if ( image.PixelFormat != PixelFormat.Format32bppArgb )
            {
                var convertedImage = new Bitmap( width, height, PixelFormat.Format32bppArgb );

                using ( var g = System.Drawing.Graphics.FromImage( convertedImage ) )
                {
                    g.DrawImage( image, 0, 0 );
                }

                image = convertedImage;
            }

            var bitmapData = image.LockBits( new Rectangle( 0, 0, width, height ),
                                             ImageLockMode.ReadOnly,
                                             PixelFormat.Format32bppArgb );

            try
            {
                var bytesPerPixel = 4; // 32bppArgb = 4 bytes per pixel
                var pixels        = new byte[ width * bytesPerPixel ];

                for ( var y = 0; y < height; y++ )
                {
                    var row = bitmapData.Scan0 + ( y * bitmapData.Stride );
                    System.Runtime.InteropServices.Marshal.Copy( row, pixels, 0, width * bytesPerPixel );

                    for ( var x = 0; x < width; x++ )
                    {
                        var pixelValue = BitConverter.ToInt32( pixels, x * bytesPerPixel );
                        Hash( sha1, pixelValue );
                    }
                }

                Hash( sha1, width );
                Hash( sha1, height );
            }
            finally
            {
                image.UnlockBits( bitmapData );
            }

            var hashBytes = sha1.Hash;

            return new BigInteger( hashBytes! ).ToString( "x" ).ToLower(); // Use ReadOnlySpan implicitly
        }
        catch ( Exception ex )
        {
            throw new Exception( "Error hashing image.", ex );
        }
    }

    private static void Hash( SHA1 digest, int value )
    {
        digest.TransformBlock( BitConverter.GetBytes( value ), 0, 4, null, 0 );
    }

    // ========================================================================

    [GeneratedRegex( "(.+)_(\\d+)$" )]
    private static partial Regex MyRegex();
}