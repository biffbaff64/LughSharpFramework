// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
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

using System.Text.RegularExpressions;

using LughSharp.Lugh.Graphics.Atlases;
using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Graphics.Text;

public partial class BitmapFont
{
    /// <summary>
    /// Backing data for a <see cref="BitmapFont" />.
    /// </summary>
    [PublicAPI]
    public class BitmapFontData
    {
        // ====================================================================

        /// <summary>
        /// Additional characters besides whitespace where text is wrapped.
        /// Eg, a hypen (-).
        /// </summary>
        public readonly char[]? BreakChars;

        public readonly char[] CapChars =
        [
            'M', 'N', 'B', 'D', 'C', 'E', 'F', 'K', 'A', 'G', 'H', 'I', 'J',
            'L', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
        ];

        public readonly char[] XChars =
        [
            'x', 'e', 'a', 'o', 'n', 's', 'r', 'c', 'u', 'm', 'v', 'w', 'z',
        ];

        // ====================================================================

        public string?     Name          { get; set; }
        public string[]?   ImagePaths    { get; set; }
        public FileInfo    FontFile      { get; set; }
        public bool        Flipped       { get; set; }
        public float       PadTop        { get; set; }
        public float       PadRight      { get; set; }
        public float       PadBottom     { get; set; }
        public float       PadLeft       { get; set; }
        public float       ScaleX        { get; set; } = 1;
        public float       ScaleY        { get; set; } = 1;
        public bool        MarkupEnabled { get; set; }
        public Glyph?[]?[] Glyphs        { get; set; } = new Glyph[ PAGES ][];

        /// <summary>
        /// The distance from one line of text to the next.
        /// </summary>
        public float LineHeight { get; set; }

        /// <summary>
        /// The distance from the top of most uppercase characters to the
        /// baseline. Since the drawing position is the cap height of the
        /// first line, the cap height can be used to get the location of
        /// the baseline.
        /// </summary>
        public float CapHeight { get; set; } = 1;

        /// <summary>
        /// The distance from the cap height to the top of the tallest glyph.
        /// </summary>
        public float Ascent { get; set; }

        /// <summary>
        /// The distance from the bottom of the glyph that extends the lowest
        /// to the baseline. This number is negative.
        /// </summary>
        public float Descent { get; set; }

        /// <summary>
        /// The distance to move down when \n is encountered.
        /// </summary>
        public float Down { get; set; }

        /// <summary>
        /// Multiplier for the line height of blank lines. down * blankLineHeight is
        /// used as the distance to move down for a blank line.
        /// </summary>
        public float BlankLineScale { get; set; } = 1;

        /// <summary>
        /// The amount to add to the glyph X position when drawing a cursor between
        /// glyphs. This field is not set by the BMFont file, it needs to be set
        /// manually depending on how the glyphs are rendered on the backing textures.
        /// </summary>
        public float CursorX { get; set; }

        /// <summary>
        /// The glyph to display for characters not in the font. May be null.
        /// </summary>
        public Glyph? MissingGlyph { get; set; }

        /// <summary>
        /// The width of the space character.
        /// </summary>
        public float SpaceXadvance { get; set; }

        /// <summary>
        /// The x-height, which is the distance from the top of most lowercase
        /// characters to the baseline.
        /// </summary>
        public float XHeight { get; set; } = 1;

        // ====================================================================

        /// <summary>
        /// Creates an empty BitmapFontData for configuration before calling
        /// <see cref="Load(FileInfo, bool)" />, to subclass, or to populate
        /// yourself, e.g. using stb-truetype or FreeType.
        /// </summary>
        public BitmapFontData()
        {
            FontFile = null!;
            Flipped  = false;
        }

        /// <summary>
        /// </summary>
        /// <param name="fontFile"></param>
        /// <param name="flip"></param>
        public BitmapFontData( FileInfo fontFile, bool flip )
        {
            Logger.Checkpoint();

            FontFile = fontFile;
            Flipped  = flip;

            Load( fontFile, flip );
        }

        // ====================================================================

        /// <summary>
        /// </summary>
        /// <param name="file"></param>
        /// <param name="flip"></param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="GdxRuntimeException"></exception>
        public void Load( FileInfo file, bool flip )
        {
            Logger.Checkpoint();
            
            if ( ImagePaths != null )
            {
                Logger.Warning( "BitmapFont Already loaded." );

                return;
            }

            Logger.Checkpoint();
            
            Name = Path.GetFileNameWithoutExtension( file.Name );

            var reader = new StreamReader( file.FullName );

            Logger.Checkpoint();
            
            try
            {
                var line = reader.ReadLine(); // info

                if ( line == null )
                {
                    throw new GdxRuntimeException( "File is empty." );
                }

                Logger.Debug( line );
                
                line = line.Substring( line.IndexOf( "padding=", StringComparison.Ordinal ) + 8 );

                var padding = line.Substring( 0, line.IndexOf( ' ' ) ).Split( ",", 4 );

                if ( padding.Length != 4 )
                {
                    throw new GdxRuntimeException( "Invalid padding." );
                }

                Logger.Checkpoint();
                
                PadTop    = int.Parse( padding[ 0 ] );
                PadRight  = int.Parse( padding[ 1 ] );
                PadBottom = int.Parse( padding[ 2 ] );
                PadLeft   = int.Parse( padding[ 3 ] );

                var padY = PadTop + PadBottom;

                line = reader.ReadLine();

                if ( line == null )
                {
                    throw new GdxRuntimeException( "Missing common header." );
                }

                Logger.Debug( line );
                
                var common = line.Split( " ", 9 ); // At most we want the 6th element; i.e. "page=N"

                // At least lineHeight and base are required.
                if ( common.Length < 3 )
                {
                    throw new GdxRuntimeException( "Invalid common header." );
                }

                if ( !common[ 1 ].StartsWith( "lineHeight=" ) )
                {
                    throw new GdxRuntimeException( "Missing: lineHeight" );
                }

                LineHeight = int.Parse( common[ 1 ].Substring( 11 ) );

                if ( !common[ 2 ].StartsWith( "base=" ) )
                {
                    throw new GdxRuntimeException( "Missing: base" );
                }

                float baseLine = int.Parse( common[ 2 ].Substring( 5 ) );

                var pageCount = 1;

                Logger.Checkpoint();
                
                if ( common is [ var _, var _, var _, var _, var _, not null, .. ]
                     && common[ 5 ].StartsWith( "pages=" ) )
                {
                    try
                    {
                        pageCount = Math.Max( 1, int.Parse( common[ 5 ].Substring( 6 ) ) );
                    }
                    catch ( Exception e )
                    {
                        Logger.Warning( $"IGNORED NumberFormatException: {e.Message}" );
                    }
                }

                ImagePaths = new string[ pageCount ];

                Logger.Checkpoint();
                
                // Read each page definition.
                for ( var p = 0; p < pageCount; p++ )
                {
                    // Read each "page" info line.
                    line = reader.ReadLine();

                    if ( line == null )
                    {
                        throw new GdxRuntimeException( "Missing additional page definitions." );
                    }

                    // Expect ID to mean "index".
                    var rx      = new Regex( ".*id=(\\d+)" );
                    var matches = rx.Matches( line );

                    if ( matches.Count > 0 )
                    {
                        rx = new Regex( "\\d+" );
                        var id = rx.Matches( matches[ 0 ].Value )[ 0 ];

                        try
                        {
                            var pageID = int.Parse( id.Value );

                            if ( pageID != p )
                            {
                                throw new GdxRuntimeException( $"Page IDs must be indices starting at 0: {id}" );
                            }
                        }
                        catch ( Exception ex )
                        {
                            throw new GdxRuntimeException( $"Invalid page id: {id}", ex );
                        }
                    }

                    rx = new Regex( ".*file=\"?([^\"]+)\"?" );

                    matches = rx.Matches( line );

                    if ( matches.Count <= 0 )
                    {
                        throw new GdxRuntimeException( "Missing: file" );
                    }

                    ImagePaths[ p ] = FontFile.FullName.Replace( @"\\\\", "/" );
                }

                Descent = 0;
                
                Logger.Checkpoint();
                
                while ( true )
                {
                    line = reader.ReadLine();

                    if ( line == null )
                    {
                        break; // EOF
                    }

                    Logger.Debug( line );
                    
                    if ( line.StartsWith( "kernings " ) )
                    {
                        break; // Starting kernings block.
                    }

                    if ( line.StartsWith( "metrics " ) )
                    {
                        break; // Starting metrics block.
                    }

                    if ( !line.StartsWith( "char " ) )
                    {
                        continue;
                    }

                    Logger.Checkpoint();
                    
                    var glyph = new Glyph();

                    Logger.Checkpoint();
                    
                    // Split the line by spaces and '=' characters.
                    // StringSplitOptions.RemoveEmptyEntries will skip any empty strings
                    // that result from multiple delimiters next to each other (e.g., "id==").
                    var parts = line.Split( [ ' ', '=' ], StringSplitOptions.RemoveEmptyEntries );

                    Logger.Checkpoint();
                    
                    // Based on the 'char id=N' format, the parts array should look like:
                    // ["char", "id", "N", "x", "X_VAL", "y", "Y_VAL", ...]
                    // So, "char" is parts[0], "id" is parts[1], and the char ID is parts[2].
                    if ( ( parts.Length < 3 ) || ( parts[ 0 ] != "char" ) || ( parts[ 1 ] != "id" ) )
                    {
                        // Handle unexpected format, maybe throw an exception
                        Logger.Warning( $"Unexpected 'char' line format: {line}" );

                        continue;
                    }

                    Logger.Checkpoint();
                    
                    if ( !int.TryParse( parts[ 2 ], out var ch ) )
                    {
                        Logger.Warning( $"Skipping malformed 'char' line (invalid ID): {line}" );

                        continue;
                    }

                    Logger.Checkpoint();
                    
                    switch ( ch )
                    {
                        case <= 0:
                            MissingGlyph = glyph;

                            break;

                        case <= char.MaxValue:
                            SetGlyph( ch, glyph );

                            break;

                        default:
                            continue;
                    }

                    Logger.Checkpoint();
                    
                    glyph.ID = ch;

                    // Now, map the remaining parts to glyph properties.
                    // This assumes a strict order as in the original `tokens.NextToken()` calls.
                    // It's a bit brittle if the order changes, but mimics the original behavior.
                    // Be careful with index if any fields are missing in the input line.
                    var currentPartIndex = 3; // Start after "char id N"

                    glyph.SrcX    = GetNextInt(); // Gets the value after "x="
                    glyph.SrcY    = GetNextInt(); // Gets the value after "y="
                    glyph.Width   = GetNextInt(); // Gets the value after "width="
                    glyph.Height  = GetNextInt(); // Gets the value after "height="
                    glyph.Xoffset = GetNextInt(); // Gets the value after "xoffset="

                    var yoffsetValue = GetNextInt(); // Get the raw yoffset value

                    if ( flip )
                    {
                        glyph.Yoffset = yoffsetValue;
                    }
                    else
                    {
                        glyph.Yoffset = -( glyph.Height + yoffsetValue );
                    }

                    glyph.Xadvance = GetNextInt(); // Gets the value after "xadvance="

                    Logger.Checkpoint();
                    
                    // We need to check if there are enough 'parts' left and if the next key is "page"
                    if ( currentPartIndex < parts.Length )
                    {
                        // Check if the current token is "page"
                        if ( parts[ currentPartIndex ] == "page" )
                        {
                            currentPartIndex++; // Move past "page"

                            if ( currentPartIndex < parts.Length )
                            {
                                try
                                {
                                    if ( !int.TryParse( parts[ currentPartIndex ], out var parsedPage ) )
                                    {
                                        Logger.Warning( $"IGNORED NumberFormatException: Invalid page ID: {parts[ currentPartIndex ]} on line: {line}" );
                                    }

                                    glyph.Page = parsedPage;
                                }
                                catch ( Exception ignored ) // Catching general exception for old code compatibility
                                {
                                    Logger.Warning( $"IGNORED Exception parsing page ID: {ignored.Message}" );
                                }

                                currentPartIndex++; // Move past the page value
                            }
                        }

                        // You might also have "chnl" after "page" or "xadvance"
                        // If you need "chnl", you'd add similar logic here:
                        if ( ( currentPartIndex < parts.Length ) && ( parts[ currentPartIndex ] == "chnl" ) )
                        {
                            // Handle chnl if needed, e.g., currentPartIndex + 1 for its value
                            // currentPartIndex += 2; // Move past key and value
                        }
                    }

                    Logger.Checkpoint();
                    
                    if ( glyph is { Width: > 0, Height: > 0 } )
                    {
                        Descent = Math.Min( baseLine + glyph.Yoffset, Descent );
                    }

                    continue;

                    // ========================================================
                    // Helper to safely get and parse the next token
                    int GetNextInt()
                    {
                        if ( ( currentPartIndex + 1 ) >= parts.Length )
                        {
                            throw new GdxRuntimeException( $"Missing expected value on char line: {line}" );
                        }

                        // Skip the key (e.g., "x", "y")
                        currentPartIndex++;

                        if ( !int.TryParse( parts[ currentPartIndex ], out var val ) )
                        {
                            throw new GdxRuntimeException( $"Invalid number format on char " +
                                                           $"line for {parts[ currentPartIndex - 1 ]}: " +
                                                           $"{parts[ currentPartIndex ]} in {line}" );
                        }

                        currentPartIndex++; // Move past the value

                        return val;
                    }
                }

                Descent += PadBottom;

                Logger.Checkpoint();
                
                while ( true )
                {
                    line = reader.ReadLine();

                    if ( line == null )
                    {
                        break;
                    }

                    Logger.Debug( line );
                    
                    if ( !line.StartsWith( "kerning " ) )
                    {
                        break;
                    }

                    var parts  = line.Split( [ ' ', '=' ], StringSplitOptions.RemoveEmptyEntries );
                    var first  = int.Parse( parts[ 2 ] );
                    var second = int.Parse( parts[ 4 ] );

                    if ( ( first < 0 )
                         || ( first > CharacterUtils.MAX_VALUE )
                         || ( second < 0 )
                         || ( second > CharacterUtils.MAX_VALUE ) )
                    {
                        continue;
                    }

                    var glyph  = GetGlyph( ( char )first );
                    var amount = int.Parse( parts[ 6 ] );

                    // Kernings may exist for glyph pairs not contained in the font.
                    glyph?.SetKerning( second, amount );
                }

                Logger.Checkpoint();
                
                var hasMetricsOverride    = false;
                var overrideAscent        = 0f;
                var overrideDescent       = 0f;
                var overrideDown          = 0f;
                var overrideCapHeight     = 0f;
                var overrideLineHeight    = 0f;
                var overrideSpaceXAdvance = 0f;
                var overrideXHeight       = 0f;

                // Metrics override
                if ( ( line != null ) && line.StartsWith( "metrics " ) )
                {
                    hasMetricsOverride = true;

                    var parts = line.Split( [ ' ', '=' ], StringSplitOptions.RemoveEmptyEntries );

                    overrideAscent        = float.Parse( parts[ 2 ] );
                    overrideDescent       = float.Parse( parts[ 4 ] );
                    overrideDown          = float.Parse( parts[ 6 ] );
                    overrideCapHeight     = float.Parse( parts[ 8 ] );
                    overrideLineHeight    = float.Parse( parts[ 10 ] );
                    overrideSpaceXAdvance = float.Parse( parts[ 12 ] );
                    overrideXHeight       = float.Parse( parts[ 14 ] );
                }

                Logger.Checkpoint();
                
                var spaceGlyph = GetGlyph( ' ' );

                if ( spaceGlyph == null )
                {
                    spaceGlyph = new Glyph { ID = ' ' };

                    var xadvanceGlyph = GetGlyph( 'l' ) ?? GetFirstGlyph();

                    spaceGlyph.Xadvance = xadvanceGlyph.Xadvance;

                    SetGlyph( ' ', spaceGlyph );
                }

                Logger.Checkpoint();
                
                if ( spaceGlyph.Width == 0 )
                {
                    spaceGlyph.Width   = ( int )( PadLeft + spaceGlyph.Xadvance + PadRight );
                    spaceGlyph.Xoffset = ( int )-PadLeft;
                }

                SpaceXadvance = spaceGlyph.Xadvance;

                Glyph? xGlyph = null;

                Logger.Checkpoint();
                
                foreach ( var xChar in XChars )
                {
                    xGlyph = GetGlyph( xChar );

                    if ( xGlyph != null )
                    {
                        break;
                    }
                }

                Logger.Checkpoint();
                
                xGlyph ??= GetFirstGlyph();

                XHeight = xGlyph.Height - padY;

                Glyph? capGlyph = null;

                Logger.Checkpoint();
                
                foreach ( var capChar in CapChars )
                {
                    capGlyph = GetGlyph( capChar );

                    if ( capGlyph != null )
                    {
                        break;
                    }
                }

                Logger.Checkpoint();
                
                if ( capGlyph == null )
                {
                    Logger.Checkpoint();

                    foreach ( Glyph?[]? page in Glyphs )
                    {
                        if ( page == null )
                        {
                            continue;
                        }

                        foreach ( var glyph in page )
                        {
                            if ( ( glyph == null )
                                 || ( glyph.Height == 0 )
                                 || ( glyph.Width == 0 ) )
                            {
                                continue;
                            }

                            CapHeight = Math.Max( CapHeight, glyph.Height );
                        }
                    }
                }
                else
                {
                    CapHeight = capGlyph.Height;
                }

                Logger.Checkpoint();
                
                CapHeight -= padY;

                Ascent = baseLine - CapHeight;
                Down   = -LineHeight;

                if ( flip )
                {
                    Ascent = -Ascent;
                    Down   = -Down;
                }

                if ( hasMetricsOverride )
                {
                    Ascent        = overrideAscent;
                    Descent       = overrideDescent;
                    Down          = overrideDown;
                    CapHeight     = overrideCapHeight;
                    LineHeight    = overrideLineHeight;
                    SpaceXadvance = overrideSpaceXAdvance;
                    XHeight       = overrideXHeight;
                }
            }
            catch ( Exception ex )
            {
                throw new GdxRuntimeException( "Error loading font file: " + file, ex );
            }
            finally
            {
                reader.Close();
            }
            
            Logger.Checkpoint();
        }

        /// <summary>
        /// </summary>
        /// <param name="glyph">
        /// A reference to the Glyph whose region is to be set.
        /// </param>
        /// <param name="region"> The <see cref="TextureRegion" />. </param>
        /// <remarks>This method is a candidate for reworking using 'ref'</remarks>
        public Glyph SetGlyphRegion( Glyph glyph, TextureRegion region )
        {
            var invTexWidth  = 1.0f / region.Texture.Width;
            var invTexHeight = 1.0f / region.Texture.Height;

            var u = region.U;
            var v = region.V;

            var offsetX = 0;
            var offsetY = 0;

            if ( region is AtlasRegion atlasRegion )
            {
                // Compensate for whitespace stripped from left and top edges.
                offsetX = ( int )atlasRegion.OffsetX;

                offsetY = ( int )( atlasRegion.OriginalHeight
                                   - atlasRegion.PackedHeight
                                   - atlasRegion.OffsetY );
            }

            var x  = glyph.SrcX;
            var x2 = glyph.SrcX + glyph.Width;
            var y  = glyph.SrcY;
            var y2 = glyph.SrcY + glyph.Height;

            // Shift glyph for left and top edge stripped whitespace.
            // Clip glyph for right and bottom edge stripped whitespace.
            // Note: if the font region has padding, whitespace stripping must not be used.
            if ( offsetX > 0 )
            {
                x -= offsetX;

                if ( x < 0 )
                {
                    glyph.Width   += x;
                    glyph.Xoffset -= x;

                    x = 0;
                }

                x2 -= offsetX;

                if ( x2 > region.RegionWidth )
                {
                    glyph.Width -= x2 - region.RegionWidth;

                    x2 = region.RegionWidth;
                }
            }

            if ( offsetY > 0 )
            {
                y -= offsetY;

                if ( y < 0 )
                {
                    glyph.Height += y;

                    if ( glyph.Height < 0 )
                    {
                        glyph.Height = 0;
                    }

                    y = 0;
                }

                y2 -= offsetY;

                if ( y2 > region.RegionHeight )
                {
                    var amount = y2 - region.RegionHeight;

                    glyph.Height  -= amount;
                    glyph.Yoffset += amount;

                    y2 = region.RegionHeight;
                }
            }

            glyph.U  = u + ( x * invTexWidth );
            glyph.U2 = u + ( x2 * invTexWidth );

            if ( Flipped )
            {
                glyph.V  = v + ( y * invTexHeight );
                glyph.V2 = v + ( y2 * invTexHeight );
            }
            else
            {
                glyph.V2 = v + ( y * invTexHeight );
                glyph.V  = v + ( y2 * invTexHeight );
            }

            return glyph;
        }

        /// <summary>
        /// Sets the line height, which is the distance from one line of text to the next.
        /// </summary>
        public void SetLineHeight( float height )
        {
            LineHeight = height * ScaleY;
            Down       = Flipped ? LineHeight : -LineHeight;
        }

        /// <summary>
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="glyph"></param>
        public void SetGlyph( int ch, Glyph glyph )
        {
            Glyph?[]? page = Glyphs[ ch / PAGE_SIZE ];

            if ( page == null )
            {
                page = new Glyph[ PAGE_SIZE ];

                Glyphs[ ch / PAGE_SIZE ] = page;
            }

            page[ ch & ( PAGE_SIZE - 1 ) ] = glyph;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        /// <exception cref="GdxRuntimeException"></exception>
        public Glyph GetFirstGlyph()
        {
            foreach ( Glyph?[]? page in Glyphs )
            {
                if ( page != null )
                {
                    foreach ( var glyph in page )
                    {
                        if ( ( glyph == null ) || ( glyph.Height == 0 ) || ( glyph.Width == 0 ) )
                        {
                            continue;
                        }

                        return glyph;
                    }
                }
            }

            throw new GdxRuntimeException( "No glyphs found." );
        }

        /// <summary>
        /// Returns true if the font has the glyph, or if the font has a <see cref="MissingGlyph" />.
        /// </summary>
        public bool HasGlyph( char ch )
        {
            if ( MissingGlyph != null )
            {
                return true;
            }

            return GetGlyph( ch ) != null;
        }

        /// <summary>
        /// Returns the glyph for the specified character, or null if no such
        /// glyph exists. Note that
        /// </summary>
        /// See also
        /// <see cref="GetGlyphs" />
        /// should be be used to shape a string
        /// of characters into a list of glyphs.
        public virtual Glyph? GetGlyph( char ch )
        {
            return Glyphs[ ch / PAGE_SIZE ]?[ ch & ( PAGE_SIZE - 1 ) ];
        }

        /// <summary>
        /// Using the specified string, populates the glyphs and positions of the
        /// specified glyph run.
        /// </summary>
        /// <param name="run"></param>
        /// <param name="str">
        /// Characters to convert to glyphs. Will not contain newline or color tags.
        /// May contain "[[" for an escaped left square bracket.
        /// </param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lastGlyph">
        /// The glyph immediately before this run, or null if this is run is the
        /// first on a line of text.
        /// </param>
        public virtual void GetGlyphs( GlyphLayout.GlyphRun? run, string str, int start, int end, Glyph? lastGlyph )
        {
            ArgumentNullException.ThrowIfNull( run );

            var max = end - start;

            if ( max == 0 )
            {
                return;
            }

            var markupEnabled = MarkupEnabled;
            var scaleX        = ScaleX;

            List< Glyph > glyphs    = run.Glyphs;
            var           xAdvances = run.XAdvances;

            // Guess at number of glyphs needed.
            glyphs.EnsureCapacity( max );
            run.XAdvances.EnsureCapacity( max + 1 );

            do
            {
                var ch = str[ start++ ];

                if ( ch == '\r' )
                {
                    continue; // Ignore.
                }

                var glyph = GetGlyph( ch );

                if ( glyph == null )
                {
                    if ( MissingGlyph == null )
                    {
                        continue;
                    }

                    glyph = MissingGlyph;
                }

                glyphs.Add( glyph );

                xAdvances.Add( lastGlyph == null // First glyph on line, adjust the position so it isn't drawn left of 0.
                                   ? glyph.FixedWidth ? 0 : ( -glyph.Xoffset * scaleX ) - PadLeft
                                   : ( lastGlyph.Xadvance + lastGlyph.GetKerning( ch ) ) * scaleX );

                lastGlyph = glyph;

                // "[[" is an escaped left square bracket, skip second character.
                if ( markupEnabled
                     && ( ch == '[' )
                     && ( start < end )
                     && ( str[ start ] == '[' ) )
                {
                    start++;
                }
            }
            while ( start < end );

            if ( lastGlyph != null )
            {
                var lastGlyphWidth = lastGlyph.FixedWidth
                    ? lastGlyph.Xadvance * scaleX
                    : ( ( lastGlyph.Width + lastGlyph.Xoffset ) * scaleX ) - PadRight;

                xAdvances.Add( lastGlyphWidth );
            }
        }

        /// <summary>
        /// Returns the first valid glyph index to use to wrap to the next line,
        /// starting at the specified start index and (typically) moving toward
        /// the beginning of the glyphs array.
        /// </summary>
        public int GetWrapIndex( List< Glyph > glyphList, int start )
        {
            var i  = start - 1;
            var ch = ( char )glyphList[ i ].ID;

            if ( IsWhitespace( ch ) )
            {
                return i;
            }

            if ( IsBreakChar( ch ) )
            {
                i--;
            }

            for ( ; i > 0; i-- )
            {
                ch = ( char )glyphList[ i ].ID;

                if ( IsWhitespace( ch ) || IsBreakChar( ch ) )
                {
                    return i + 1;
                }
            }

            return 0;
        }

        private Dictionary< string, string > ParseKeyValueLine( string line )
        {
            var dict  = new Dictionary< string, string >( StringComparer.OrdinalIgnoreCase );
            var parts = line.Split( ' ', StringSplitOptions.RemoveEmptyEntries ); // Split by space

            // Skip the first token (e.g., "info", "common")
            for ( var i = 1; i < parts.Length; i++ )
            {
                var keyValue = parts[ i ].Split( '=', 2 ); // Split each part by the first '='

                if ( keyValue.Length == 2 )
                {
                    dict[ keyValue[ 0 ] ] = keyValue[ 1 ];
                }
            }

            return dict;
        }

        /// <summary>
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsBreakChar( char c )
        {
            if ( BreakChars == null )
            {
                return false;
            }

            foreach ( var br in BreakChars )
            {
                if ( c == br )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool IsWhitespace( char c )
        {
            switch ( c )
            {
                case '\n':
                case '\r':
                case '\t':
                case ' ':
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Scales the font by the specified amounts on both axes
        /// <para>
        /// Note that smoother scaling can be achieved if the texture backing
        /// the BitmapFont is using <see cref="Texture.TextureFilter.Linear" />.
        /// The default is Nearest, so use a BitmapFont constructor that takes
        /// a <see cref="TextureRegion" />.
        /// </para>
        /// </summary>
        /// <exception cref="ArgumentException">if scaleX or scaleY is zero.</exception>
        public void SetScale( float scalex, float scaley )
        {
            if ( scalex == 0 )
            {
                throw new ArgumentException( "scaleX cannot be 0." );
            }

            if ( scaley == 0 )
            {
                throw new ArgumentException( "scaleY cannot be 0." );
            }

            var x = scalex / ScaleX;
            var y = scaley / ScaleY;

            ScaleX = scalex;
            ScaleY = scaley;

            LineHeight    *= y;
            SpaceXadvance *= x;
            XHeight       *= y;
            CapHeight     *= y;
            Ascent        *= y;
            Descent       *= y;
            Down          *= y;
            PadLeft       *= x;
            PadRight      *= x;
            PadTop        *= y;
            PadBottom     *= y;
        }

        /// <summary>
        /// Scales the font by the specified amount in both directions.
        /// </summary>
        /// See also
        /// <see cref="SetScale(float, float)" />
        /// <exception cref="ArgumentException">if scaleX or scaleY is zero.</exception>
        public void SetScale( float scaleXy )
        {
            SetScale( scaleXy, scaleXy );
        }

        /// <summary>
        /// Sets the font's scale relative to the current scale.
        /// </summary>
        /// See also
        /// <see cref="SetScale(float, float)" />
        /// <exception cref="ArgumentException">if the resulting scale is zero.</exception>
        public void Scale( float amount )
        {
            SetScale( ScaleX + amount, ScaleY + amount );
        }

        /// <inheritdoc />
        public override string? ToString()
        {
            return Name ?? base.ToString();
        }
    }
}