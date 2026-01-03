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
using System.Security.Cryptography;
using LughSharp.Core.Files;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;
using Bitmap = System.Drawing.Bitmap;
using Image = System.Drawing.Image;
using Rectangle = System.Drawing.Rectangle;

namespace Extensions.Source.Tools.TexturePacker;

/// <summary>
/// 
/// </summary>
[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class ImageProcessor
{
    public Resampling                Resampling { get; set; } = Resampling.Bicubic;
    public List< TexturePackerRect > ImageRects { get; set; } = [ ];
    public TexturePackerSettings     Settings   { get; }

    public float Scale
    {
        get => field;
        set
        {
            field = value;
            field = Math.Max( 0.0f, Math.Min( 1.0f, field ) );
        }
    } = 1.0f;

    // ========================================================================

    private static readonly Bitmap EmptyImage   = new( 1, 1, PixelFormat.Format32bppArgb );
    private static readonly Regex  IndexPattern = RegexUtils.ItemWithUnderscoreSuffixRegex();

    private readonly Dictionary< string, TexturePackerRect? > _crcs = [ ];

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
    public TexturePackerRect? AddImage( FileInfo? file, string? rootPath )
    {
        Guard.Against.Null( file );

        Bitmap? image;

        try
        {
            image = new Bitmap( file.FullName );
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( $"Error reading image: {file}", ex );
        }

        var name = IOUtils.NormalizePath( file.FullName );

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
    public TexturePackerRect? AddImage( Bitmap? image, string? name )
    {
        var rect = ProcessImage( image, name );

        if ( rect == null )
        {
            return null;
        }

        if ( Settings.IsAlias )
        {
            var crc = Hash( rect.GetImage( this ) );

            if ( _crcs.TryGetValue( crc, out var existing ) )
            {
                // Image already exists, add current rect as an alias to the existing one.
                existing?.Aliases.Add( new TexturePackerAlias( rect ) );

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
        // Only clear the image rectangles processed for the current scale.
        ImageRects.Clear();

        // The CRC dictionary (_crcs) must REMAIN POPULATED to track aliased 
        // images across all scale iterations defined in settings.
        // DO NOT CALL: _crcs.Clear();
    }

    /// <summary>
    /// Returns a rect for the image describing the texture region to be packed,
    /// or null if the image should not be packed.
    /// </summary>
    public TexturePackerRect? ProcessImage( Bitmap? image, string? name )
    {
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

        TexturePackerRect? rect;

        if ( isPatch )
        {
            // Strip ".9" from file name, read ninepatch split pixels,
            // and strip ninepatch split pixels.
            name   = name.Substring( 0, name.Length - 2 );
            splits = GetSplits( image, name );
            pads   = GetPads( image, name, splits );

            // Preserve reference to the large image before re-assigning the 'image' variable.
            var sourceImage = image;

            // Strip split pixels.
            width  -= 2;
            height -= 2;

            // Create the new, smaller destination bitmap.
            var newImage = new Bitmap( width, height, PixelFormat.Format32bppArgb );

            // Draw the content from the original image (starting at 1,1)
            // onto the new smaller bitmap (starting at 0,0).
            using ( var g = Graphics.FromImage( newImage ) )
            {
                g.DrawImage( sourceImage,
                             new Rectangle( 0, 0, width, height ), // Destination Rectangle (0, 0 to new W, H)
                             1, 1,                                 // Source X, Y (Start at 1, 1 to skip border)
                             width, height,                        // Source Width, Height (FIXED DIMENSIONS)
                             GraphicsUnit.Pixel );
            }

            // The 'image' variable must be updated to the new, cropped
            // bitmap for scaling/stripping steps below.
            image = newImage;
        }

        // Scale image.
        if ( Math.Abs( Scale - 1f ) > NumberUtils.FLOAT_TOLERANCE )
        {
            width  = ( int )Math.Max( 1, Math.Round( width * Scale ) );
            height = ( int )Math.Max( 1, Math.Round( height * Scale ) );

            var newImage = new Bitmap( width, height, PixelFormat.Format32bppArgb );

            if ( Scale < 1 )
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

        // Strip digits, if any, from the end of name and use as index.
        var index = -1;

        if ( Settings.UseIndexes )
        {
            var match = IndexPattern.Match( name );

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
            rect = new TexturePackerRect( image, 0, 0, width, height, true )
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
    protected TexturePackerRect? StripWhitespace( string name, Bitmap source )
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
            Logger.Error( $"Bitmap '{name}' has already been disposed!" );

            return null; // Or handle the error appropriately
        }
        catch ( ArgumentException ex )
        {
            Logger.Error( $"ArgumentException accessing Bitmap '{name}': {ex.Message}" );

            return null; // Or handle the error appropriately
        }

        // Check the PixelFormat of the Bitmap. If it doesn't have an alpha channel,
        // or if both StripWhitespaceX and StripWhitespaceY are false, a new Rect
        // encompassing the entire source image is returned.
        if ( !Image.IsAlphaPixelFormat( source.PixelFormat )
          || Settings is { StripWhitespaceX: false, StripWhitespaceY: false } )
        {
            return new TexturePackerRect( source, 0, 0, width, height, false );
        }

        var bitmapData = source.LockBits( new Rectangle( 0, 0, width, height ),
                                          ImageLockMode.ReadOnly,
                                          PixelFormat.Format32bppArgb );

        try
        {
            unsafe
            {
                var ptr    = ( byte* )bitmapData.Scan0;
                var stride = bitmapData.Stride;

                // Y-Stripping Logic
                var top    = 0;
                var bottom = height;

                if ( Settings.StripWhitespaceY )
                {
                    // Find TOP
                outer1:

                    for ( var y = 0; y < height; y++ )
                    {
                        var rowPtr = ptr + ( y * stride );

                        for ( var x = 0; x < width; x++ )
                        {
                            // Alpha is the 4th byte (index 3) in BGRA
                            if ( rowPtr[ ( x * 4 ) + 3 ] > Settings.AlphaThreshold )
                            {
                                goto outer1;
                            }
                        }

                        top++;
                    }

                    // Find BOTTOM
                outer2:

                    for ( var y = height - 1; y >= top; y-- )
                    {
                        var rowPtr = ptr + ( y * stride );

                        for ( var x = 0; x < width; x++ )
                        {
                            if ( rowPtr[ ( x * 4 ) + 3 ] > Settings.AlphaThreshold )
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

                // X-Stripping Logic
                var left  = 0;
                var right = width;

                if ( Settings.StripWhitespaceX )
                {
                    // Find LEFT
                outer3:

                    for ( var x = 0; x < width; x++ )
                    {
                        for ( var y = top; y < bottom; y++ )
                        {
                            var rowPtr = ptr + ( y * stride );

                            if ( rowPtr[ ( x * 4 ) + 3 ] > Settings.AlphaThreshold )
                            {
                                goto outer3;
                            }
                        }

                        left++;
                    }

                    // Find RIGHT
                outer4:

                    for ( var x = width - 1; x >= left; x-- )
                    {
                        for ( var y = top; y < bottom; y++ )
                        {
                            var rowPtr = ptr + ( y * stride );

                            if ( rowPtr[ ( x * 4 ) + 3 ] > Settings.AlphaThreshold )
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
                        : new TexturePackerRect( EmptyImage, 0, 0, 1, 1, false );
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

                return new TexturePackerRect( strippedImage, 0, 0, newWidth, newHeight, false );
            }
        }
        finally
        {
            // This MUST be called to release the bitmap data.
            source.UnlockBits( bitmapData );
        }
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

        if ( Math.Abs( Scale - 1.0f ) > NumberUtils.FLOAT_TOLERANCE )
        {
            startX = ( int )Math.Round( startX * Scale );
            endX   = ( int )Math.Round( endX * Scale );
            startY = ( int )Math.Round( startY * Scale );
            endY   = ( int )Math.Round( endY * Scale );
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

        if ( Math.Abs( Scale - 1.0f ) > NumberUtils.FLOAT_TOLERANCE )
        {
            startX = ( int )Math.Round( startX * Scale );
            endX   = ( int )Math.Round( endX * Scale );
            startY = ( int )Math.Round( startY * Scale );
            endY   = ( int )Math.Round( endY * Scale );
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
                const int BYTES_PER_PIXEL = 4;
                // Buffer sized to hold the largest row, using Stride for safety, but width*4 is often sufficient.
                // Let's use a standard array based on width for clear intent:
                var pixels = new byte[ width * BYTES_PER_PIXEL ];

                for ( var y = 0; y < height; y++ )
                {
                    var row = bitmapData.Scan0 + ( y * bitmapData.Stride );
                    // Copy the row data from the unmanaged pointer into the managed byte array
                    Marshal.Copy( row, pixels, 0, width * BYTES_PER_PIXEL );

                    // Hash the entire row block at once, efficiently.
                    sha1.TransformBlock( pixels, 0, pixels.Length, pixels, 0 );
                }

                // Hash width and height (using the existing helper)
                HashTransformBlock( sha1, width );
                HashTransformBlock( sha1, height );

                // Finalize the hash with any remaining data in the internal buffer
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
        // The TransformBlock method with an output buffer set to null is fine for 
        // feeding data sequentially before the final block.
        digest.TransformBlock( BitConverter.GetBytes( value ), 0, 4, null!, 0 );
    }
}

// ============================================================================
// ============================================================================