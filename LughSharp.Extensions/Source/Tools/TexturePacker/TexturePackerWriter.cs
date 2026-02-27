// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Versioning;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Exceptions;

using Bitmap = System.Drawing.Bitmap;
using Encoder = System.Drawing.Imaging.Encoder;
using Pen = System.Drawing.Pen;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class TexturePackerWriter
{
    private readonly TexturePackerSettings          _settings;
    private readonly TexturePackerProgressListener? _progressListener;
    private readonly ImageProcessor                 _imageProcessor;

    // ========================================================================

    public TexturePackerWriter( TexturePackerSettings settings,
                                TexturePackerProgressListener? progressListener,
                                ImageProcessor imageProcessor )
    {
        _settings         = settings;
        _progressListener = progressListener;
        _imageProcessor   = imageProcessor;
    }

    /// <summary>
    /// Writes packed images to disk, applying padding, power-of-two, and other settings.
    /// </summary>
    /// <param name="outputDir"></param>
    /// <param name="scaledPackFileName"></param>
    /// <param name="pages"></param>
    /// <exception cref="RuntimeException"></exception>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="Exception"></exception>
    public void WriteImages( string outputDir, string scaledPackFileName, List< TexturePackerPage > pages )
    {
        var            packFileNoExt = new FileInfo( Path.Combine( outputDir, scaledPackFileName ) );
        DirectoryInfo? packDir       = packFileNoExt.Directory;
        string         imageName     = Path.GetFileName( packFileNoExt.FullName );

        if ( packDir == null )
        {
            throw new RuntimeException( "Error creating pack directory." );
        }

        var fileIndex = 1;

        // Iterate over each page to write its image
        for ( var p = 0; p < pages.Count; p++ )
        {
            TexturePackerPage page = pages[ p ] ?? throw new NullReferenceException( "Page is null" );

            int width  = page.Width;
            int height = page.Height;

            // Apply edge padding if enabled
            if ( _settings.EdgePadding )
            {
                int edgePadX = _settings.PaddingX;
                int edgePadY = _settings.PaddingY;

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

            // Adjust to next power of two if required
            if ( _settings.PowerOfTwo )
            {
                width  = MathUtils.NextPowerOfTwo( width );
                height = MathUtils.NextPowerOfTwo( height );
            }

            // Adjust to multiple of four if required
            if ( _settings.MultipleOfFour )
            {
                width  = ( width % 4 ) == 0 ? width : width + 4 - ( width % 4 );
                height = ( height % 4 ) == 0 ? height : height + 4 - ( height % 4 );
            }

            // Enforce minimum dimensions
            width            = Math.Max( _settings.MinWidth, width );
            height           = Math.Max( _settings.MinHeight, height );
            page.ImageWidth  = width;
            page.ImageHeight = height;

            string outputFile;

            // Find a unique output file name
            while ( true )
            {
                string name = imageName;

                if ( fileIndex > 1 )
                {
                    char last = name[ name.Length - 1 ];

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

                outputFile = Path.Combine( packDir.FullName, name + "." + _settings.OutputFormat );

                if ( !File.Exists( outputFile ) )
                {
                    break;
                }
            }

            // Create output directories if needed
            Directory.CreateDirectory( Path.GetDirectoryName( outputFile )
                                    ?? throw new RuntimeException( "Error creating output directory" ) );

            page.ImageName = Path.GetFileName( outputFile );

            // Create a bitmap canvas for the page
            var canvas = new Bitmap( width,
                                     height,
                                     LughSharp.Core.Graphics.PixelFormat.ToSystemPixelFormat( _settings.Format ) );
            Graphics g = Graphics.FromImage( canvas );

            if ( page.OutputRects == null )
            {
                throw new NullReferenceException( "OutputRects for page is null" );
            }

            _progressListener?.Start( 1f / pages.Count );

            // Draw each rect (image region) onto the canvas
            for ( var r = 0; r < page.OutputRects.Count; r++ )
            {
                TexturePackerRect rect = page.OutputRects[ r ];

                using ( Bitmap image = rect.GetImage( _imageProcessor ) )
                {
                    int iw    = image.Width;
                    int ih    = image.Height;
                    int rectX = page.X + rect.X;
                    int rectY = page.Y + page.Height - rect.Y - ( rect.Height - _settings.PaddingY );

                    // Apply duplicate padding if enabled
                    if ( _settings.DuplicatePadding )
                    {
                        int amountX = _settings.PaddingX / 2;
                        int amountY = _settings.PaddingY / 2;

                        if ( rect.Rotated )
                        {
                            // Fill corners and edges for rotated rects
                            for ( var i = 1; i <= amountX; i++ )
                            {
                                for ( var j = 1; j <= amountY; j++ )
                                {
                                    Plot( canvas, rectX - j, rectY + iw - 1 + i, image.GetPixel( 0, 0 ) );
                                    Plot( canvas,
                                          rectX + ih - 1 + j,
                                          rectY + iw - 1 + i,
                                          image.GetPixel( 0, ih - 1 ) );
                                    Plot( canvas, rectX - j, rectY - i, image.GetPixel( iw - 1, 0 ) );
                                    Plot( canvas,
                                          rectX + ih - 1 + j,
                                          rectY - i,
                                          image.GetPixel( iw - 1, ih - 1 ) );
                                }
                            }

                            // Fill edge pixels for rotated rects
                            for ( var i = 1; i <= amountY; i++ )
                            {
                                for ( var j = 0; j < iw; j++ )
                                {
                                    Plot( canvas, rectX - i, rectY + iw - 1 - j, image.GetPixel( j, 0 ) );
                                    Plot( canvas,
                                          rectX + ih - 1 + i,
                                          rectY + iw - 1 - j,
                                          image.GetPixel( j, ih - 1 ) );
                                }
                            }

                            for ( var i = 1; i <= amountX; i++ )
                            {
                                for ( var j = 0; j < ih; j++ )
                                {
                                    Plot( canvas, rectX + j, rectY - i, image.GetPixel( iw - 1, j ) );
                                    Plot( canvas, rectX + j, rectY + iw - 1 + i, image.GetPixel( 0, j ) );
                                }
                            }
                        }
                        else
                        {
                            // Fill corners and edges for non-rotated rects
                            for ( var i = 1; i <= amountX; i++ )
                            {
                                for ( var j = 1; j <= amountY; j++ )
                                {
                                    Plot( canvas, rectX - i, rectY - j, image.GetPixel( 0, 0 ) );
                                    Plot( canvas, rectX - i, rectY + ih - 1 + j, image.GetPixel( 0, ih - 1 ) );
                                    Plot( canvas, rectX + iw - 1 + i, rectY - j, image.GetPixel( iw - 1, 0 ) );
                                    Plot( canvas,
                                          rectX + iw - 1 + i,
                                          rectY + ih - 1 + j,
                                          image.GetPixel( iw - 1, ih - 1 ) );
                                }
                            }

                            // Fill edge pixels for non-rotated rects
                            for ( var i = 1; i <= amountY; i++ )
                            {
                                Copy( image, 0, 0, iw, 1, canvas, rectX, rectY - i, rect.Rotated );
                                Copy( image,
                                      0,
                                      ih - 1,
                                      iw,
                                      1,
                                      canvas,
                                      rectX,
                                      rectY + ih - 1 + i,
                                      rect.Rotated );
                            }

                            for ( var i = 1; i <= amountX; i++ )
                            {
                                Copy( image, 0, 0, 1, ih, canvas, rectX - i, rectY, rect.Rotated );
                                Copy( image,
                                      iw - 1,
                                      0,
                                      1,
                                      ih,
                                      canvas,
                                      rectX + iw - 1 + i,
                                      rectY,
                                      rect.Rotated );
                            }
                        }
                    }

                    // Copy the actual image region to the canvas
                    Copy( image, 0, 0, iw, ih, canvas, rectX, rectY, rect.Rotated );

                    // Draw debug rectangle if enabled
                    if ( _settings.Debug )
                    {
                        using ( var pen = new Pen( Color.Magenta ) )
                        {
                            g.DrawRectangle( pen,
                                             rectX,
                                             rectY,
                                             rect.Width - _settings.PaddingX - 1,
                                             rect.Height - _settings.PaddingY - 1 );
                        }
                    }

                    // Update progress, abort if requested
                    if ( _progressListener!.Update( r + 1, page.OutputRects.Count ) )
                    {
                        return;
                    }
                }
            }

            _progressListener?.End();

            // Apply color bleed effect if enabled and not saving as JPEG
            if ( _settings is { Bleed: true, PremultiplyAlpha: false }
              && !( _settings.OutputFormat.Equals( "jpg", StringComparison.OrdinalIgnoreCase ) ||
                    _settings.OutputFormat.Equals( "jpeg", StringComparison.OrdinalIgnoreCase ) ) )
            {
                canvas = new ColorBleedEffect().ProcessImage( canvas, _settings.BleedIterations );

                g.Dispose(); // Dispose of previous Graphics object
                g = Graphics.FromImage( canvas );
            }

            // Draw debug border if enabled
            if ( _settings.Debug )
            {
                var pen = new Pen( Color.Magenta );
                g.DrawRectangle( pen, 0, 0, width - 1, height - 1 );
            }

            try
            {
                // Save as JPEG with quality settings
                if ( _settings.OutputFormat.Equals( "jpg", StringComparison.OrdinalIgnoreCase ) ||
                     _settings.OutputFormat.Equals( "jpeg", StringComparison.OrdinalIgnoreCase ) )
                {
                    using ( var newImage = new Bitmap( canvas.Width, canvas.Height, PixelFormat.Format24bppRgb ) )
                    using ( Graphics newGraphics = Graphics.FromImage( newImage ) )
                    {
                        // Clear the background with the configured color before drawing.
                        newGraphics.Clear( Color.White );
                        newGraphics.DrawImage( canvas, 0, 0 );

                        ImageCodecInfo jpgEncoder          = GetEncoder( ImageFormat.Jpeg );
                        Encoder        myEncoder           = Encoder.Quality;
                        var            myEncoderParameters = new EncoderParameters( 1 );
                        var myEncoderParameter =
                            new EncoderParameter( myEncoder, ( long )( _settings.JpegQuality * 100 ) );

                        myEncoderParameters.Param[ 0 ] = myEncoderParameter;
                        newImage.Save( outputFile, jpgEncoder, myEncoderParameters );
                    }
                }
                else
                {
                    // Premultiply alpha if required
                    if ( _settings.PremultiplyAlpha )
                    {
                        // Use LockBits for massive performance improvement
                        BitmapData bitmapData = canvas.LockBits( new System.Drawing.Rectangle( 0, 0, width, height ),
                                                                 ImageLockMode.ReadWrite,
                                                                 PixelFormat.Format32bppArgb );

                        try
                        {
                            unsafe
                            {
                                var ptr    = ( byte* )bitmapData.Scan0;
                                int stride = bitmapData.Stride;

                                for ( var y = 0; y < height; y++ )
                                {
                                    byte* rowPtr = ptr + ( y * stride );

                                    for ( var x = 0; x < width; x++ )
                                    {
                                        // Pixels are stored in BGRA order (Blue, Green, Red, Alpha)
                                        int index = x * 4;

                                        byte alpha = rowPtr[ index + 3 ]; // Alpha (index 3)

                                        if ( alpha == 0 )
                                        {
                                            // Optimization: Skip fully transparent pixels
                                            continue;
                                        }

                                        // Alpha factor (0.0 to 1.0)
                                        float alphaFactor = alpha / 255f;

                                        // Apply premultiplication: R = R * A, G = G * A, B = B * A
                                        // Note: Alpha (index 3) remains unchanged

                                        // Blue (index 0)
                                        rowPtr[ index + 0 ] = ( byte )( rowPtr[ index + 0 ] * alphaFactor );

                                        // Green (index 1)
                                        rowPtr[ index + 1 ] = ( byte )( rowPtr[ index + 1 ] * alphaFactor );

                                        // Red (index 2)
                                        rowPtr[ index + 2 ] = ( byte )( rowPtr[ index + 2 ] * alphaFactor );
                                    }
                                }
                            }
                        }
                        finally
                        {
                            // Unlock the bits to ensure the changes are
                            // applied and memory is released.
                            canvas.UnlockBits( bitmapData );
                        }
                    }

                    // Save as PNG
                    canvas.Save( outputFile, ImageFormat.Png );
                }
            }
            catch ( IOException ex )
            {
                throw new Exception( "Error writing file: " + outputFile, ex );
            }

            // Update progress, abort if requested
            if ( _progressListener!.Update( p + 1, pages.Count ) )
            {
                return;
            }

            _progressListener.Count++;
        }
    }

    /// <summary>
    /// Plots a single pixel to the destination bitmap if within bounds.
    /// </summary>
    private void Plot( Bitmap dst, int x, int y, Color argb )
    {
        if ( ( 0 <= x ) && ( x < dst.Width ) && ( 0 <= y ) && ( y < dst.Height ) )
        {
            dst.SetPixel( x, y, argb );
        }
    }

    /// <summary>
    /// Copies a region from the source bitmap to the destination bitmap, handling rotation.
    /// </summary>
    private void Copy( Bitmap src, int x, int y, int w, int h, Bitmap dst, int dx, int dy, bool rotated )
    {
        for ( var i = 0; i < w; i++ )
        {
            for ( var j = 0; j < h; j++ )
            {
                if ( rotated )
                {
                    Plot( dst, dx + j, dy + w - i - 1, src.GetPixel( x + i, y + j ) );
                }
                else
                {
                    Plot( dst, dx + i, dy + j, src.GetPixel( x + i, y + j ) );
                }
            }
        }
    }

    /// <summary>
    /// Writes the atlas pack file, including all page and rect metadata.
    /// </summary>
    public void WritePackFile( DirectoryInfo outputDir, string scaledPackFileName, List< TexturePackerPage > pages )
    {
        var packFile =
            new FileInfo( Path.Combine( outputDir.FullName, scaledPackFileName + _settings.AtlasExtension ) );
        DirectoryInfo? packDir = packFile.Directory;

        packDir?.Create();

        // Check for duplicate region names if appending
        if ( packFile.Exists )
        {
            var textureAtlasData = new TextureAtlasData( packFile, packDir!, _settings.FlattenPaths );

            foreach ( TexturePackerPage page in pages )
            {
                Guard.Against.Null( page );

                foreach ( TexturePackerRect rect in page.OutputRects )
                {
                    string rectName = TexturePackerRect.GetAtlasName( rect.Name, _settings.FlattenPaths );

                    foreach ( TextureAtlasData.Region region in textureAtlasData.Regions )
                    {
                        Guard.Against.Null( region.Name );

                        if ( region.Name.Equals( rectName ) )
                        {
                            throw new RuntimeException( $"A region with the name \"{rectName}\" " +
                                                        $"has already been packed: {rect.Name}" );
                        }
                    }
                }
            }
        }

        bool appending = packFile.Exists;

        // Write metadata for each page and its rects
        using ( var writer = new StreamWriter( packFile.FullName, appending, System.Text.Encoding.UTF8 ) )
        {
            for ( var i = 0; i < pages.Count; i++ )
            {
                TexturePackerPage page = pages[ i ];

                Guard.Against.Null( page );
                Guard.Against.Null( page.OutputRects );

                // ---------- Writing Atlas Header ----------

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

                // ---------- Writing Atlas Rects ----------

                page.OutputRects.Sort();

                foreach ( TexturePackerRect rect in page.OutputRects )
                {
                    if ( ( rect.Name == null ) || ( rect.Name.Length == 0 ) )
                    {
                        throw new RuntimeException( "rect.Name must not be null or empty" );
                    }

                    if ( _settings.LegacyOutput )
                    {
                        WriteRectLegacy( writer, page, rect, rect.Name );
                    }
                    else
                    {
                        WriteRect( writer, page, rect, rect.Name );
                    }

                    // Write aliases for the rect
                    var aliases = new List< TexturePackerAlias >( rect.Aliases );

                    aliases.Sort();

                    foreach ( TexturePackerAlias alias in aliases )
                    {
                        if ( ( alias.Name == null ) || ( alias.Name.Length == 0 ) )
                        {
                            throw new RuntimeException( "alias.Name must not be null or empty" );
                        }

                        var aliasRect = new TexturePackerRect();
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

    /// <summary>
    /// Writes page metadata in non-legacy format.
    /// </summary>
    private void WritePage( TextWriter writer, bool appending, TexturePackerPage page )
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

        if ( LughSharp.Core.Graphics.PixelFormat.ToSystemPixelFormat( _settings.Format ) !=
             PixelFormat.Format32bppArgb )
        {
            writer.WriteLine( $"{tab}format{colon}{_settings.Format}" );
        }

        if ( ( _settings.FilterMin != TextureFilterMode.Nearest ) ||
             ( _settings.FilterMag != TextureFilterMode.Nearest ) )
        {
            writer.WriteLine( $"{tab}filter{colon}{_settings.FilterMin}{comma}{_settings.FilterMag}" );
        }

        string? repeatValue = GetRepeatValue(); // Pass settings to GetRepeatValue

        if ( repeatValue != null )
        {
            writer.WriteLine( $"{tab}repeat{colon}{repeatValue}" );
        }

        if ( _settings.PremultiplyAlpha )
        {
            writer.WriteLine( $"{tab}pma{colon}true" );
        }
    }

    /// <summary>
    /// Writes rect metadata in non-legacy format.
    /// </summary>
    private void WriteRect( TextWriter writer, TexturePackerPage page, TexturePackerRect rect, string name )
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

        writer.WriteLine( TexturePackerRect.GetAtlasName( name, _settings.FlattenPaths ) );

        if ( rect.Index != -1 )
        {
            writer.WriteLine( $"{tab}index{colon}{rect.Index}" );
        }

        int atlasY = page.ImageHeight - rect.Y - rect.RegionHeight;
        writer.WriteLine( $"{tab}bounds{colon}{page.X + rect.X}{comma}{atlasY}{comma}{rect.RegionWidth}{comma}{rect.RegionHeight}" );

        int offsetY = rect.OriginalHeight - rect.RegionHeight - rect.OffsetY;

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

    /// <summary>
    /// Writes page metadata in legacy format.
    /// </summary>
    private void WritePageLegacy( TextWriter writer, TexturePackerPage page )
    {
        writer.WriteLine();
        writer.WriteLine( page.ImageName );
        writer.WriteLine( $"size: {page.ImageWidth}, {page.ImageHeight}" );
        writer.WriteLine( $"format: {_settings.Format}" );
        writer.WriteLine( $"filter: {_settings.FilterMin}, {_settings.FilterMag}" );

        string? repeatValue = GetRepeatValue();
        writer.WriteLine( $"repeat: {repeatValue ?? "none"}" );
    }

    /// <summary>
    /// Writes rect metadata in legacy format.
    /// </summary>
    private void WriteRectLegacy( TextWriter writer, TexturePackerPage page, TexturePackerRect rect, string name )
    {
        writer.WriteLine( TexturePackerRect.GetAtlasName( name, _settings.FlattenPaths ) );
        writer.WriteLine( $"  rotate: {rect.Rotated}" );

        int atlasY = page.ImageHeight - rect.Y - rect.RegionHeight;
        writer.WriteLine( $"  xy: {page.X + rect.X}, {atlasY}" );

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

    /// <summary>
    /// Gets the encoder for a given image format.
    /// </summary>
    private static ImageCodecInfo GetEncoder( ImageFormat format )
    {
        ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

        foreach ( ImageCodecInfo codec in codecs )
        {
            if ( codec.FormatID == format.Guid )
            {
                return codec;
            }
        }

        throw new RuntimeException( $"Decode for ImageFormat {format} not found" );
    }

    /// <summary>
    /// Returns the repeat value for texture wrapping based on settings.
    /// </summary>
    private string? GetRepeatValue()
    {
        return _settings switch
               {
                   { WrapX: TextureWrapMode.Repeat, WrapY     : TextureWrapMode.Repeat }      => "xy",
                   { WrapX: TextureWrapMode.Repeat, WrapY     : TextureWrapMode.ClampToEdge } => "x",
                   { WrapX: TextureWrapMode.ClampToEdge, WrapY: TextureWrapMode.Repeat }      => "y",

                   var _ => null
               };
    }
}

// ============================================================================
// ============================================================================