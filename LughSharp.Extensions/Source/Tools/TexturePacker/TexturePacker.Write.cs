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

using Extensions.Source.Drawing;

using LughSharp.Lugh.Graphics.Atlases;
using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Logging;

using Bitmap = System.Drawing.Bitmap;
using Pen = System.Drawing.Pen;

namespace Extensions.Source.Tools.TexturePacker;

[SupportedOSPlatform( "windows" )]
public partial class TexturePacker
{
    /// <summary>
    /// </summary>
    /// <param name="outputDir"></param>
    /// <param name="scaledPackFileName"></param>
    /// <param name="pages"></param>
    /// <exception cref="GdxRuntimeException"></exception>
    /// <exception cref="NullReferenceException"></exception>
    /// <exception cref="Exception"></exception>
    private void WriteImages( string outputDir, string scaledPackFileName, List< Page? > pages )
    {
        Logger.Checkpoint();

        var packFileNoExt = Path.Combine( outputDir, scaledPackFileName );
        var packDir       = Path.GetDirectoryName( packFileNoExt );
        var imageName     = Path.GetFileName( packFileNoExt );

        if ( packDir == null )
        {
            throw new GdxRuntimeException( "Error creating pack directory." );
        }

        var fileIndex = 1;

        Logger.Debug( $"packFileNoExt: {packFileNoExt}" );
        Logger.Debug( $"packDir: {packDir}" );
        Logger.Debug( $"imageName: {imageName}" );
        Logger.Debug( $"pages.Count: {pages.Count}" );

        for ( var p = 0; p < pages.Count; p++ )
        {
            var page = pages[ p ] ?? throw new NullReferenceException( "Page is null" );

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

            Logger.Debug( $"page.ImageWidth: {page.ImageWidth}" );
            Logger.Debug( $"page.ImageHeight: {page.ImageHeight}" );

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

            Logger.Debug( $"outputFile: {outputFile}" );

            // Create output directories
            Directory.CreateDirectory( Path.GetDirectoryName( outputFile )
                                       ?? throw new GdxRuntimeException( "Error creating output directory" ) );

            page.ImageName = Path.GetFileName( outputFile );

            var canvas = new Bitmap( width, height, PixmapFormatExtensions.ToSystemPixelFormat( _settings.Format ) );
            var g      = Graphics.FromImage( canvas );

            if ( page.OutputRects == null )
            {
                throw new NullReferenceException( "OutputRects for page is null" );
            }

            ProgressListener?.Start( 1f / pages.Count );

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
                            // Copy corner pixels to fill the corners of the padding.
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
                            // Copy corner pixels to fill the corners of the padding.
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
                        using ( var pen = new Pen( Color.Magenta ) )
                        {
                            g.DrawRectangle( pen, rectX, rectY,
                                             rect.Width - _settings.PaddingX - 1,
                                             rect.Height - _settings.PaddingY - 1 );
                        }
                    }

                    if ( ProgressListener!.Update( r + 1, page.OutputRects.Count ) )
                    {
                        return;
                    }
                }
            }

            ProgressListener?.End();

            if ( _settings is { Bleed: true, PremultiplyAlpha: false }
                 && !( _settings.OutputFormat.Equals( "jpg", StringComparison.OrdinalIgnoreCase ) ||
                       _settings.OutputFormat.Equals( "jpeg", StringComparison.OrdinalIgnoreCase ) ) )
            {
                canvas = new ColorBleedEffect().ProcessImage( canvas, _settings.BleedIterations );

                g.Dispose(); // Dispose of previous Graphics object
                g = Graphics.FromImage( canvas );
            }

            if ( _settings.Debug )
            {
                var pen = new Pen( Color.Magenta );
                g.DrawRectangle( pen, 0, 0, width - 1, height - 1 );
            }

            try
            {
                if ( _settings.OutputFormat.Equals( "jpg", StringComparison.OrdinalIgnoreCase ) ||
                     _settings.OutputFormat.Equals( "jpeg", StringComparison.OrdinalIgnoreCase ) )
                {
                    using ( var newImage = new Bitmap( canvas.Width, canvas.Height, PixelFormat.Format24bppRgb ) )
                    using ( var newGraphics = Graphics.FromImage( newImage ) )
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

                                canvas.SetPixel( x, y, Color.FromArgb( color.A, red, green, blue ) );
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

            if ( ProgressListener!.Update( p + 1, pages.Count ) )
            {
                return;
            }

            ProgressListener.Count++;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="dst"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="argb"></param>
    private static void Plot( Bitmap dst, int x, int y, Color argb )
    {
        if ( ( 0 <= x ) && ( x < dst.Width ) && ( 0 <= y ) && ( y < dst.Height ) )
        {
            dst.SetPixel( x, y, argb );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="src"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="dst"></param>
    /// <param name="dx"></param>
    /// <param name="dy"></param>
    /// <param name="rotated"></param>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputDir"></param>
    /// <param name="scaledPackFileName"></param>
    /// <param name="pages"></param>
    /// <exception cref="GdxRuntimeException"></exception>
    /// <exception cref="NullReferenceException"></exception>
    private void WritePackFile( DirectoryInfo outputDir, string scaledPackFileName, List< Page? > pages )
    {
        Logger.Checkpoint();

        var packFile = new FileInfo( Path.Combine( outputDir.FullName, scaledPackFileName + _settings.AtlasExtension ) );
        var packDir  = packFile.Directory;

        packDir?.Create();

        if ( packFile.Exists )
        {
            // Make sure there aren't duplicate names.
            var textureAtlasData = new TextureAtlasData( packFile, flip: _settings.FlattenPaths );

            foreach ( var page in pages )
            {
                Guard.ThrowIfNull( page );
                Guard.ThrowIfNull( page.OutputRects );

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

        Logger.Debug( $"packFile.FullName: {packFile.FullName}" );
        Logger.Debug( $"packFile.Exists: {File.Exists( packFile.FullName )}" );

        var appending = packFile.Exists;

        using ( var writer = new StreamWriter( packFile.FullName, appending, System.Text.Encoding.UTF8 ) )
        {
            Logger.Debug( $"pages.Count: {pages.Count}" );

            for ( var i = 0; i < pages.Count; i++ )
            {
                var page = pages[ i ];

                Guard.ThrowIfNull( page );
                Guard.ThrowIfNull( page.OutputRects );

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

        if ( PixmapFormatExtensions.ToSystemPixelFormat( _settings.Format ) != PixelFormat.Format32bppArgb )
        {
            writer.WriteLine( $"{tab}format{colon}{_settings.Format}" );
        }

        if ( ( _settings.FilterMin != TextureFilterMode.Nearest ) || ( _settings.FilterMag != TextureFilterMode.Nearest ) )
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

    /// <summary>
    /// Writes the header details for the atlas.
    /// </summary>
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

    /// <summary>
    /// Writes the details for a single packed image to the atlas.
    /// </summary>
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
    /// 
    /// </summary>
    /// <returns></returns>
    private string? GetRepeatValue()
    {
        return _settings switch
        {
            { WrapX: TextureWrapMode.Repeat, WrapY     : TextureWrapMode.Repeat }      => "xy",
            { WrapX: TextureWrapMode.Repeat, WrapY     : TextureWrapMode.ClampToEdge } => "x",
            { WrapX: TextureWrapMode.ClampToEdge, WrapY: TextureWrapMode.Repeat }      => "y",

            var _ => null,
        };
    }
}

// ============================================================================
// ============================================================================