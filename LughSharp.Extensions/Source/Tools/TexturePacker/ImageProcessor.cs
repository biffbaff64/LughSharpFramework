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

using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics.Text;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Logging;

using Bitmap = System.Drawing.Bitmap;
using Image = System.Drawing.Image;
using Rectangle = System.Drawing.Rectangle;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class ImageProcessor
{
    public float                      Scale      { get; set; }
    public Resampling                 Resampling { get; set; } = Resampling.Bicubic;
    public List< TexturePacker.Rect > ImageRects { get; set; } = [ ];
    public TexturePackerSettings      Settings   { get; }

    // ========================================================================

    private static readonly Bitmap _emptyImage   = new( 1, 1, PixelFormat.Format32bppArgb );
    private static readonly Regex  _indexPattern = RegexUtils.ItemWithUnderscoreSuffixRegex();

    private readonly Dictionary< string, TexturePacker.Rect? > _crcs = [ ];

    private float _scale = 1;

    // ========================================================================
    // ========================================================================

    public ImageProcessor( TexturePackerSettings settings )
    {
        Settings = settings;
    }

    /// <summary>
    /// Gets the image from the specified path and adds it to the list of images.
    /// The resolved image will not be kept in-memory during packing if
    /// <see cref="TexturePackerSettings.LimitMemory"/> is true.
    /// </summary>
    /// <param name="file"> The path to the image file. </param>
    /// <param name="rootPath">
    /// Used to strip the root directory prefix from image file names, can be null.
    /// </param>
    public TexturePacker.Rect? AddImage( FileInfo? file, string? rootPath )
    {
        ArgumentNullException.ThrowIfNull( file );

        rootPath = IOUtils.NormalizePath( rootPath );

        Bitmap? image;

        try
        {
            image = new Bitmap( file.FullName );
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( $"Error reading image: {file}", ex );
        }

        if ( image == null )
        {
            throw new GdxRuntimeException( $"Unable to read image: {file}" );
        }

        var name = IOUtils.NormalizePath( file.FullName );

        // Strip root dir from the front of the image path.
        if ( !name.StartsWith( rootPath ) )
        {
            throw new GdxRuntimeException( $"Path '{name}' does not start with root: {rootPath}" );
        }

        // Strip extension.
        name = Path.GetFileNameWithoutExtension( name );

        var rect = AddImage( image, name );

        if ( ( rect != null ) && Settings.LimitMemory )
        {
            rect.UnloadImage( file );
        }
        
        return rect;
    }

    /// <summary>
    /// Adds the provided <see cref="Bitmap"/> image to the list.
    /// </summary>
    /// <param name="image"> The Image. </param>
    /// <param name="name"> The Image name, without patch or extension! </param>
    /// <returns></returns>
    public TexturePacker.Rect? AddImage( Bitmap? image, string? name )
    {
        var rect = ProcessImage( image, name );

        if ( rect == null )
        {
            Logger.Debug( "Returning early: Rect is null" );
            Logger.Debug( $"Ignoring blank input image: {name}" );

            return null;
        }

        if ( Settings.IsAlias )
        {
            var crc = Hash( rect.GetImage( this ) );

            TexturePacker.Rect? existing = null;

            try
            {
                existing = _crcs[ crc ];
            }
            catch ( KeyNotFoundException )
            {
                // This is expected for new images
                Logger.Debug( $"CRC '{crc}' not found in _crcs (first time?)" );
            }

            if ( existing != null )
            {
                existing.Aliases.Add( new TexturePacker.Alias( rect ) );

                return null;
            }

            _crcs[ crc ] = rect;
        }

        ImageRects.Add( rect );

        return rect;
    }

    /// <summary>
    /// Clears the image and crc collections.
    /// </summary>
    public void Clear()
    {
        ImageRects.Clear();
        _crcs.Clear();
    }

    /// <summary>
    /// Returns a rect for the image describing the texture region to be packed,
    /// or null if the image should not be packed.
    /// </summary>
    public TexturePacker.Rect? ProcessImage( Bitmap? image, string? name )
    {
        if ( _scale <= 0 )
        {
            throw new GdxRuntimeException( $"scale cannot be <= 0: {_scale}" );
        }

        if ( image == null )
        {
            throw new GdxRuntimeException( $"Unable to read image: {name}" );
        }
        name ??= string.Empty;

        var width  = image.Width;
        var height = image.Height;

        if ( image.PixelFormat != PixelFormat.Format32bppArgb )
        {
            image = new Bitmap( width, height, PixelFormat.Format32bppArgb );

            Graphics.FromImage( image ).DrawImage( image, 0, 0, width, height );
        }

        var    isPatch = name.EndsWith( ".9" );
        int[]? splits  = null;
        int[]? pads    = null;

        TexturePacker.Rect? rect;

        if ( isPatch )
        {
            // Strip ".9" from file name, read ninepatch split pixels,
            // and strip ninepatch split pixels.
            name   = name.Substring( 0, name.Length - 2 );
            splits = GetSplits( image, name );
            pads   = GetPads( image, name, splits );

            // Strip split pixels.
            width  -= 2;
            height -= 2;

            image = new Bitmap( width, height, PixelFormat.Format32bppArgb );

            Graphics.FromImage( image ).DrawImage( image,
                                                   new Rectangle( 1, 1, width + 1, height + 1 ),
                                                   0,
                                                   0,
                                                   width,
                                                   height,
                                                   GraphicsUnit.Pixel );
        }

        // Scale image.
        if ( Math.Abs( _scale - 1f ) > NumberUtils.FLOAT_TOLERANCE )
        {
            width  = ( int )Math.Max( 1, Math.Round( width * _scale ) );
            height = ( int )Math.Max( 1, Math.Round( height * _scale ) );

            var newImage = new Bitmap( width, height, PixelFormat.Format32bppArgb );

            if ( _scale < 1 )
            {
                // Scaling down: Use HighQualityBicubic for good quality downscaling
                var g = Graphics.FromImage( newImage );
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage( image, 0, 0, width, height );
                g.Dispose();
            }
            else
            {
                // Scaling up or no scaling: Apply rendering hints
                var g = Graphics.FromImage( newImage );
                g.CompositingQuality = CompositingQuality.HighQuality;   // VALUE_RENDER_QUALITY
                g.InterpolationMode  = Resampling.ToInterpolationMode(); // Map resampling value
                g.DrawImage( image, 0, 0, width, height );
                g.Dispose();
            }
        }

        // Strip digits, if any, off end of name and use as index.
        var index = -1;

        if ( Settings.UseIndexes )
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

            if ( rect == null )
            {
                return null;
            }
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
        int width;
        int height;

        try
        {
            width  = source.Width;
            height = source.Height;
        }
        catch ( ObjectDisposedException )
        {
            Logger.Warning( $"Bitmap '{name}' has already been disposed!" );

            return null; // Or handle the error appropriately
        }
        catch ( ArgumentException ex )
        {
            Logger.Warning( $"ArgumentException accessing Bitmap '{name}': {ex.Message}" );

            return null; // Or handle the error appropriately
        }

        // Check the PixelFormat of the Bitmap. If it doesn't have an alpha channel,
        // or if both StripWhitespaceX and StripWhitespaceY are false, a new Rect
        // encompassing the entire source image is returned.
        if ( !Image.IsAlphaPixelFormat( source.PixelFormat )
             || Settings is { StripWhitespaceX: false, StripWhitespaceY: false } )
        {
            return new TexturePacker.Rect( source, 0, 0, width, height, false );
        }

        var top    = 0;
        var bottom = height;

        if ( Settings.StripWhitespaceY )
        {
        outer1:

            for ( var y = 0; y < height; y++ )
            {
                for ( var x = 0; x < width; x++ )
                {
                    var pixel = source.GetPixel( x, y );

                    if ( pixel.A > Settings.AlphaThreshold )
                    {
                        goto outer1;
                    }
                }

                top++;
            }

        outer2:

            for ( var y = height - 1; y >= top; y-- )
            {
                for ( var x = 0; x < width; x++ )
                {
                    var pixel = source.GetPixel( x, y );

                    if ( pixel.A > Settings.AlphaThreshold )
                    {
                        goto outer2;
                    }
                }

                bottom--;
            }

            if ( Settings.DuplicatePadding )
            {
                if ( top > 0 )
                {
                    top--;
                }

                if ( bottom < height )
                {
                    bottom++;
                }
            }
        }

        var left  = 0;
        var right = width;

        if ( Settings.StripWhitespaceX )
        {
        outer3:

            for ( var x = 0; x < width; x++ )
            {
                for ( var y = top; y < bottom; y++ )
                {
                    var pixel = source.GetPixel( x, y );

                    if ( pixel.A > Settings.AlphaThreshold )
                    {
                        goto outer3;
                    }
                }

                left++;
            }

        outer4:

            for ( var x = width - 1; x >= left; x-- )
            {
                for ( var y = top; y < bottom; y++ )
                {
                    var pixel = source.GetPixel( x, y );

                    if ( pixel.A > Settings.AlphaThreshold )
                    {
                        goto outer4;
                    }
                }

                right--;
            }

            if ( Settings.DuplicatePadding )
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

        var newWidth  = right - left;
        var newHeight = bottom - top;

        if ( ( newWidth <= 0 ) || ( newHeight <= 0 ) )
        {
            return Settings.IgnoreBlankImages
                ? null
                : new TexturePacker.Rect( _emptyImage, 0, 0, 1, 1, false );
        }

        // Create a new Bitmap representing the stripped area
        var strippedImage = new Bitmap( newWidth, newHeight, source.PixelFormat );

        using ( var g = Graphics.FromImage( strippedImage ) )
        {
            g.DrawImage( source,
                         new Rectangle( 0, 0, newWidth, newHeight ),
                         new Rectangle( left, top, newWidth, newHeight ),
                         GraphicsUnit.Pixel );
        }

        return new TexturePacker.Rect( strippedImage, 0, 0, newWidth, newHeight, false );
    }

    /// <summary>
    /// Fetch the Alpha value of a pixel at the given coordinates.
    /// </summary>
    /// <param name="data"> The BitmapData holding the pixels. </param>
    /// <param name="x"> The X Co-ordinate. </param>
    /// <param name="y"> The Y Co-ordinate. </param>
    /// <returns> The Alpha value. </returns>
    private unsafe int GetAlpha( BitmapData data, int x, int y )
    {
        var ptr      = ( byte* )data.Scan0;
        var pixelPtr = ptr + ( y * data.Stride ) + ( x * 4 ); // 4 bytes per pixel (ARGB)
        int alpha    = pixelPtr[ 3 ];

        return alpha;
    }

    /// <summary>
    /// Returns the splits, or null if the image had no splits or the splits were
    /// only a single region. Splits are an int[4] that has left, right, top, bottom.
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

        // Subtraction here is because the coordinates were computed before the 1px
        // border was stripped.
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

        if ( Math.Abs( _scale - 1.0f ) > NumberUtils.FLOAT_TOLERANCE )
        {
            startX = ( int )Math.Round( startX * _scale );
            endX   = ( int )Math.Round( endX * _scale );
            startY = ( int )Math.Round( startY * _scale );
            endY   = ( int )Math.Round( endY * _scale );
        }

        return [ startX, endX, startY, endY ];
    }

    /// <summary>
    /// Returns the pads, or null if the image had no pads or the pads match
    /// the splits. Pads are an int[4] that has left, right, top, bottom.
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

        if ( startX != 0 )
        {
            endX = GetSplitPoint( image, name, startX + 1, bottom, false, true );
        }

        if ( startY != 0 )
        {
            endY = GetSplitPoint( image, name, right, startY + 1, false, false );
        }

        // Ensure pixels after the end are not invalid.
        GetSplitPoint( image, name, endX + 1, bottom, true, true );
        GetSplitPoint( image, name, right, endY + 1, true, false );

        switch ( startX )
        {
            // No pads.
            case 0 when ( endX == 0 ) && ( startY == 0 ) && ( endY == 0 ):
                return null;

            case 0 when endX == 0:
                startX = -1;
                endX   = -1;

                break;

            case > 0:
                startX--;
                endX = image.Width - 2 - ( endX - 1 );

                break;

            default:
                // If no start point was ever found, we assume full stretch.
                endX = image.Width - 2;

                break;
        }

        switch ( startY )
        {
            case 0 when endY == 0:
                startY = -1;
                endY   = -1;

                break;

            case > 0:
                startY--;
                endY = image.Height - 2 - ( endY - 1 );

                break;

            default:
                // If no start point was ever found, we assume full stretch.
                endY = image.Height - 2;

                break;
        }

        if ( Math.Abs( _scale - 1.0f ) > NumberUtils.FLOAT_TOLERANCE )
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
    /// Hunts for the start or end of a sequence of split pixels. Begins searching
    /// at (startX, startY) then follows along the x or y axis (depending on value
    /// of xAxis) for the first non-transparent pixel if startPoint is true, or the
    /// first transparent pixel if startPoint is false. Returns 0 if none found, as
    /// 0 is considered an invalid split point being in the outer border which will
    /// be stripped.
    /// </summary>
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

                    if ( rgba[ 3 ] == breakA )
                    {
                        return next;
                    }

                    if ( !startPoint && ( ( rgba[ 0 ] != 0 )
                                          || ( rgba[ 1 ] != 0 )
                                          || ( rgba[ 2 ] != 0 )
                                          || ( rgba[ 3 ] != 255 ) ) )
                    {
                        throw new
                            GdxRuntimeException( $"Invalid {name} ninepatch split pixel at {x}, {y}, rgba: {rgba[ 0 ]}, {rgba[ 1 ]}, {rgba[ 2 ]}, {rgba[ 3 ]}" );
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

                using ( var g = Graphics.FromImage( convertedImage ) )
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
                const int BYTES_PER_PIXEL = 4; // 32bppArgb = 4 bytes per pixel
                var       pixels          = new byte[ width * BYTES_PER_PIXEL ];

                for ( var y = 0; y < height; y++ )
                {
                    var row = bitmapData.Scan0 + ( y * bitmapData.Stride );
                    Marshal.Copy( row, pixels, 0, width * BYTES_PER_PIXEL );

                    for ( var x = 0; x < width; x++ )
                    {
                        var pixelValue = BitConverter.ToInt32( pixels, x * BYTES_PER_PIXEL );
                        HashTransformBlock( sha1, pixelValue ); // Use TransformBlock
                    }
                }

                HashTransformBlock( sha1, width );  // Use TransformBlock
                HashTransformBlock( sha1, height ); // Use TransformBlock

                // Finalize the hash
                sha1.TransformFinalBlock( [ ], 0, 0 ); // Pass an empty byte array

                var hashBytes = sha1.Hash;

                return new BigInteger( hashBytes! ).ToString( "x" ).ToLower();
            }
            finally
            {
                image.UnlockBits( bitmapData );
            }
        }
        catch ( Exception ex )
        {
            throw new Exception( "Error hashing image.", ex );
        }
    }

    private static void HashTransformBlock( SHA1 digest, int value )
    {
        digest.TransformBlock( BitConverter.GetBytes( value ), 0, 4, null, 0 );
    }
}