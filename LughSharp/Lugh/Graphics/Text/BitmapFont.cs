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
using LughSharp.Lugh.Graphics.G2D;
using LughSharp.Lugh.Graphics.Images;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Collections;

namespace LughSharp.Lugh.Graphics.Text;

/// <summary>
/// Renders bitmap fonts. The font consists of 2 files: an image file or <see cref="TextureRegion" />
/// containing the glyphs and a file in the <b>AngleCode BMFont</b> text format that describes where
/// each glyph is on the image.
/// <para>
/// Text is drawn using a <see cref="IBatch" />. Text can be cached in a <see cref="BitmapFontCache" />
/// for faster rendering of static text, which saves needing to compute the location of each glyph
/// each frame.
/// </para>
/// <para>
/// The texture for a BitmapFont loaded from a file is managed. <see cref="Dispose()" /> must be
/// called to free the texture when no longer needed. A BitmapFont which has loaded using a
/// <see cref="TextureRegion" /> is managed if the region's texture is managed. Disposing the
/// BitmapFont disposes the region's texture, which may not be desirable if the texture is still
/// being used elsewhere.
/// </para>
/// </summary>
[PublicAPI]
public partial class BitmapFont
{
    /// <summary>
    /// The BitmapFontCache used by this font, for rendering to a sprite batch.
    /// This can be used, for example, to manipulate glyph colors within a
    /// specific index.
    /// </summary>
    public BitmapFontCache Cache { get; set; }

    /// <summary>
    /// The underlying <see cref="BitmapFontData" /> for this BitmapFont.
    /// </summary>
    public BitmapFontData Data { get; set; }

    /// <summary>
    /// Specifies whether to use integer positions.
    /// Default is to use them so filtering doesn't kick in as badly.
    /// </summary>
    public bool UseIntegerPositions
    {
        get => _integer;
        set
        {
            _integer                  = value;
            Cache.UseIntegerPositions = value;
        }
    }

    public bool Flipped     { get; set; }
    public bool OwnsTexture { get; set; }

    // ========================================================================

    private const string REGEX_PATTERN      = ".*id=(\\d+)";
    private const string DEFAULT_FONT       = "Assets/Fonts/arial-15.fnt";
    private const string DEFAULT_FONT_IMAGE = "Assets/Fonts/arial-15.png";
    private const int    LOG2_PAGE_SIZE     = 9;
    private const int    PAGE_SIZE          = 1 << LOG2_PAGE_SIZE;
    private const int    PAGES              = 0x10000 / PAGE_SIZE;

    // ========================================================================

    private readonly PathTypes             _fileType;
    private readonly List< TextureRegion > _regions;

    private bool _integer;

    // ========================================================================

    /// <summary>
    /// Creates a BitmapFont using the default 15pt Arial font included in the library.
    /// This is convenient to easily display text without bothering without generating
    /// a bitmap font yourself.
    /// </summary>
    public BitmapFont() : this( GdxApi.Files.Internal( DEFAULT_FONT ),
                                GdxApi.Files.Internal( DEFAULT_FONT_IMAGE ),
                                false )
    {
        Logger.Checkpoint();

        _fileType = PathTypes.Internal;
    }

    /// <summary>
    /// Creates a BitmapFont using the default 15pt Arial font included in the LughSharp project.
    /// This is convenient to easily display text without bothering without generating a bitmap
    /// font yourself.
    /// </summary>
    /// <param name="flip">
    /// If true, the glyphs will be flipped for use with a perspective where 0,0 is the upper left corner.
    /// </param>
    public BitmapFont( bool flip ) : this( GdxApi.Files.Internal( DEFAULT_FONT ),
                                           GdxApi.Files.Internal( DEFAULT_FONT ),
                                           flip )
    {
        Logger.Checkpoint();
        
        _fileType = PathTypes.Internal;
    }

    /// <summary>
    /// Creates a BitmapFont with the glyphs relative to the specified region.
    /// If the region is null, the glyph textures are loaded from the image file
    /// given in the font file. The Dispose() method will not dispose the region's
    /// texture in this case!
    /// </summary>
    /// <param name="fontFile"> the font definition file.</param>
    /// <param name="region">
    /// The texture region containing the glyphs. The glyphs must be relative to
    /// the lower left corner (ie, the region should not be flipped). If the region
    /// is null the glyph images are loaded from the image path in the font file.
    /// </param>
    /// <param name="flip">
    /// If true, the glyphs will be flipped for use with a perspective where 0,0
    /// is the upper left corner.
    /// </param>
    public BitmapFont( FileInfo fontFile, TextureRegion region, bool flip = false )
        : this( new BitmapFontData( fontFile, flip ), region, true )
    {
        Logger.Checkpoint();
        
        _fileType = PathTypes.Local;
    }

    /// <summary>
    /// Creates a BitmapFont from a BMFont file. The image file name is read from
    /// the BMFont file and the image is loaded from the same directory.
    /// </summary>
    /// <param name="fontFile"> the font definition file.</param>
    /// <param name="flip">
    /// If true, the glyphs will be flipped for use with a perspective where 0,0
    /// is the upper left corner.
    /// </param>
    public BitmapFont( FileInfo fontFile, bool flip = false )
        : this( new BitmapFontData( fontFile, flip ), ( TextureRegion? )null, true )
    {
        Logger.Checkpoint();
        
        _fileType = PathTypes.Local;
    }

    /// <summary>
    /// Creates a BitmapFont from a BMFont file, using the specified image for glyphs. Any
    /// image specified in the BMFont file is ignored.
    /// </summary>
    /// <param name="fontFile"> the font definition file.</param>
    /// <param name="imageFile"></param>
    /// <param name="flip">
    /// If true, the glyphs will be flipped for use with a perspective where 0,0 is the upper
    /// left corner.
    /// </param>
    /// <param name="integer"></param>
    public BitmapFont( FileInfo fontFile, FileInfo imageFile, bool flip, bool integer = true )
        : this( new BitmapFontData( fontFile, flip ), new TextureRegion( new Texture( imageFile, false ) ), integer )
    {
        Logger.Checkpoint();
        
        OwnsTexture = true;
        _fileType   = PathTypes.Local;
    }

    /// <summary>
    /// Constructs a new BitmapFont from the given <see cref="BitmapFontData" /> and
    /// <see cref="TextureRegion" />. If the TextureRegion is null, the image path(s)
    /// will be read from the BitmapFontData.
    /// <para>
    /// The dispose() method will not dispose the texture of the region(s) if the region is != null.
    /// </para>
    /// <para>
    /// Passing a single TextureRegion assumes that your font only needs a single texture page. If
    /// you need to support multiple pages, either let the Font read the images themselves (by
    /// specifying null as the TextureRegion), or by specifying each page manually with the
    /// TextureRegion[] constructor.
    /// </para>
    /// </summary>
    /// <param name="data"> The BitmapFontData. </param>
    /// <param name="region"> The TextureRegion. </param>
    /// <param name="integer">
    /// If true, rendering positions will be at integer values to avoid filtering artifacts.
    /// </param>
    public BitmapFont( BitmapFontData data, TextureRegion? region, bool integer )
        : this( data, region != null ? ListExtensions.New( region ) : null, integer )
    {
        Logger.Checkpoint();
        
        _fileType = PathTypes.Local;
    }

    /// <summary>
    /// Constructs a new BitmapFont from the given <see cref="BitmapFontData" /> and array
    /// of <see cref="TextureRegion" />. If the TextureRegion is null or empty, the image
    /// path(s) will be read from the BitmapFontData. The dispose() method will not dispose
    /// the texture of the region(s) if the regions array is != null and not empty.
    /// </summary>
    /// <param name="data"> The BitmapFontData. </param>
    /// <param name="pageRegions"> The list of TextureRegions. </param>
    /// <param name="integer">
    /// If true, rendering positions will be at integer values to avoid filtering artifacts.
    /// </param>
    public BitmapFont( BitmapFontData data, List< TextureRegion >? pageRegions, bool integer )
    {
        Logger.Checkpoint();
        
        Flipped             = data.Flipped;
        Data                = data;
        UseIntegerPositions = integer;
        _fileType           = PathTypes.Local;

        Logger.Checkpoint();
        
        if ( ( pageRegions == null ) || ( pageRegions.Count == 0 ) )
        {
            if ( data.ImagePaths == null )
            {
                throw new ArgumentException( "If no regions are specified, the" +
                                             "font data must have an images path." );
            }

            // Load each path.
            var n = data.ImagePaths.Length;

            _regions = new List< TextureRegion >( n );

            Logger.Checkpoint();
            
            for ( var i = 0; i < n; i++ )
            {
                var file = data.FontFile == null
                    ? GdxApi.Files.Internal( data.ImagePaths[ i ] )
                    : GdxApi.Files.GetFileHandle( data.ImagePaths[ i ], _fileType );

                _regions.Add( new TextureRegion( new Texture( file, false ) ) );
            }

            Logger.Checkpoint();
            
            OwnsTexture = true;
        }
        else
        {
            _regions    = pageRegions;
            OwnsTexture = false;
        }

        Logger.Checkpoint();
        
        Cache = new BitmapFontCache( this, UseIntegerPositions );

        Logger.Checkpoint();
        
        InitialLoad( data );
        
        Logger.Checkpoint();
    }

    /// <summary>
    /// Returns the <see cref="BitmapFontData.ScaleX" /> value.
    /// </summary>
    public float GetScaleX()
    {
        return Data.ScaleX;
    }

    /// <summary>
    /// Returns the <see cref="BitmapFontData.ScaleY" /> value.
    /// </summary>
    public float GetScaleY()
    {
        return Data.ScaleY;
    }

    // ========================================================================

    /// <summary>
    /// Helper method, allowing a call to <see cref="Load(BitmapFontData)" />,
    /// which is a <b>virtual</b> method, from constructors.
    /// </summary>
    private void InitialLoad( BitmapFontData data )
    {
        Load( data );
    }

    /// <summary>
    /// Loads the glyph regions for each glyph in the provided font data.
    /// </summary>
    /// <param name="data">
    /// The BitmapFontData containing information about the glyphs and their regions.
    /// </param>
    protected virtual void Load( BitmapFontData data )
    {
        // Iterate through each page of glyphs in the font data.
        foreach ( Glyph?[]? page in data.Glyphs )
        {
            // Skip null pages.
            if ( page == null )
            {
                continue;
            }

            // Iterate through each glyph in the page.
            foreach ( var glyph in page )
            {
                // Set the glyph region if the glyph is not null.
                if ( glyph != null )
                {
                    data.SetGlyphRegion( glyph, _regions[ glyph.Page ] );
                }
            }
        }

        // Set the glyph region for the missing glyph if it exists.
        if ( data.MissingGlyph != null )
        {
            data.MissingGlyph = data.SetGlyphRegion( data.MissingGlyph, _regions[ data.MissingGlyph.Page ] );
        }
    }

    // ========================================================================

    /// <summary>
    /// Returns the color of text drawn with this font.
    /// </summary>
    public Color GetColor()
    {
        return Cache.GetColor();
    }

    /// <summary>
    /// A convenience method for setting the font color.
    /// </summary>
    public void SetColor( Color color )
    {
        Cache.GetColor().Set( color );
    }

    /// <summary>
    /// A convenience method for setting the font color.
    /// </summary>
    public void SetColor( float r, float g, float b, float a )
    {
        Cache.GetColor().Set( r, g, b, a );
    }

    /// <summary>
    /// Returns the first texture region. This is included for backwards compatibility,
    /// and for convenience since most fonts only use one texture page.
    /// <para>
    /// For multi-page fonts, use <see cref="GetRegions()" />.
    /// </para>
    /// </summary>
    /// <returns>the first texture region</returns>
    public TextureRegion GetRegion()
    {
        return _regions.First();
    }

    /// <summary>
    /// Returns the array of TextureRegions that represents each texture page of glyphs.
    /// </summary>
    /// <returns> The array of texture regions; modifying it may produce undesirable results </returns>
    public List< TextureRegion > GetRegions()
    {
        return _regions;
    }

    /// <summary>
    /// Returns the texture page at the given index.
    /// </summary>
    public TextureRegion GetRegion( int index )
    {
        return _regions[ index ];
    }

    /// <summary>
    /// Returns the line height, which is the distance from one line of text to the next.
    /// </summary>
    public float GetLineHeight()
    {
        return Data.LineHeight;
    }

    /// <summary>
    /// Returns the x-advance of the space character.
    /// </summary>
    public virtual float GetSpaceXadvance()
    {
        return Data.SpaceXadvance;
    }

    /// <summary>
    /// Returns the x-height, which is the distance from the top of most lowercase
    /// characters to the baseline.
    /// </summary>
    public float GetXHeight()
    {
        return Data.XHeight;
    }

    /// <summary>
    /// Returns the cap height, which is the distance from the top of most uppercase
    /// characters to the baseline. Since the drawing position is the cap height of
    /// the first line, the cap height can be used to get the location of the baseline.
    /// </summary>
    public float GetCapHeight()
    {
        return Data.CapHeight;
    }

    /// <summary>
    /// Returns the ascent, which is the distance from the cap height to the top of
    /// the tallest glyph.
    /// </summary>
    public float GetAscent()
    {
        return Data.Ascent;
    }

    /// <summary>
    /// Returns the descent, which is the distance from the bottom of the glyph that
    /// extends the lowest to the baseline. This number is negative.
    /// </summary>
    public float GetDescent()
    {
        return Data.Descent;
    }

    /// <summary>
    /// Makes the specified glyphs fixed width. This can be useful to make the numbers
    /// in a font fixed width. Eg, when horizontally centering a score or loading
    /// percentage text, it will not jump around as different numbers are shown.
    /// </summary>
    public void SetFixedWidthGlyphs( string glyphs )
    {
        var data       = Data;
        var maxAdvance = 0;

        for ( int index = 0, end = glyphs.Length; index < end; index++ )
        {
            var g = data.GetGlyph( glyphs[ index ] );

            if ( ( g != null ) && ( g.Xadvance > maxAdvance ) )
            {
                maxAdvance = g.Xadvance;
            }
        }

        for ( int index = 0, end = glyphs.Length; index < end; index++ )
        {
            var g = data.GetGlyph( glyphs[ index ] );

            if ( g == null )
            {
                continue;
            }

            g.Xoffset    += ( maxAdvance - g.Xadvance ) / 2;
            g.Xadvance   =  maxAdvance;
            g.Kerning    =  null;
            g.FixedWidth =  true;
        }
    }

    /// <summary>
    /// Creates a new BitmapFontCache for this font. Using this method allows the
    /// font to provide the BitmapFontCache implementation to customize rendering.
    /// </summary>
    /// <para>
    /// Note this method is called by the BitmapFont constructors. If a subclass
    /// overrides this method, it will be called before the subclass constructors.
    /// </para>
    public virtual BitmapFontCache NewFontCache()
    {
        return new BitmapFontCache( this, UseIntegerPositions );
    }

    /// <inheritdoc />
    public override string? ToString()
    {
        return Data.Name ?? base.ToString();
    }

    /// <summary>
    /// Returns the index of the character 'ch' in the supplied text string.
    /// Scanning for the character begins at the index specified by 'start'.
    /// </summary>
    private static int IndexOf( string text, char ch, int start )
    {
        var n = text.Length;

        for ( ; start < n; start++ )
        {
            if ( text[ start ] == ch )
            {
                return start;
            }
        }

        return n;
    }

    /// <summary>
    /// Disposes the texture used by this BitmapFont's region IF this BitmapFont
    /// created the texture.
    /// </summary>
    public void Dispose()
    {
        if ( OwnsTexture )
        {
            foreach ( var t in _regions )
            {
                t.Texture.Dispose();
            }
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Represents a single character in a font page.
    /// </summary>
    [PublicAPI]
    public class Glyph
    {
        public bool       FixedWidth { get; set; }
        public int        Height     { get; set; }
        public int        ID         { get; set; }
        public byte[]?[]? Kerning    { get; set; }
        public int        SrcX       { get; set; }
        public int        SrcY       { get; set; }
        public float      U          { get; set; }
        public float      U2         { get; set; }
        public float      V          { get; set; }
        public float      V2         { get; set; }
        public int        Width      { get; set; }
        public int        Xadvance   { get; set; }
        public int        Xoffset    { get; set; }
        public int        Yoffset    { get; set; }

        /// <summary>
        /// The index to the texture page that holds this glyph.
        /// </summary>
        public int Page { get; set; }

        public int GetKerning( char ch )
        {
            if ( Kerning != null )
            {
                var page = Kerning[ ch >>> LOG2_PAGE_SIZE ];

                return page != null ? page[ ch & ( PAGE_SIZE - 1 ) ] : 0;
            }

            return 0;
        }

        public void SetKerning( int ch, int value )
        {
            Kerning ??= new byte[ PAGES ][];

            var page = Kerning[ ch >>> LOG2_PAGE_SIZE ];

            if ( page == null )
            {
                Kerning[ ch >>> LOG2_PAGE_SIZE ] = page = new byte[ PAGE_SIZE ];
            }

            page[ ch & ( PAGE_SIZE - 1 ) ] = ( byte )value;
        }

        public override string ToString()
        {
            return ID.ToString();
        }
    }

    // ========================================================================

    #region font drawing

    /// <summary>
    /// Draws text at the specified position.
    /// </summary>
    /// <param name="batch"> The <see cref="IBatch" /> to use. </param>
    /// <param name="str"> The text message to draw. </param>
    /// <param name="x"> X coordinate. </param>
    /// <param name="y"> Y coordinate. </param>
    public GlyphLayout Draw( IBatch batch, string str, float x, float y )
    {
        Cache.Clear();

        var layout = Cache.AddText( str, x, y );

        Cache.Draw( batch );

        return layout;
    }

    /// <summary>
    /// Draws text at the specified position.
    /// </summary>
    public GlyphLayout Draw( IBatch batch, string str, float x, float y, int targetWidth, int halign, bool wrap )
    {
        Cache.Clear();

        var layout = Cache.AddText( str, x, y, targetWidth, halign, wrap );

        Cache.Draw( batch );

        return layout;
    }

    /// <summary>
    /// Draws text at the specified position.
    /// </summary>
    public GlyphLayout Draw( IBatch batch,
                             string str,
                             float x,
                             float y,
                             int start,
                             int end,
                             float targetWidth,
                             int halign,
                             bool wrap )
    {
        Cache.Clear();

        var layout = Cache.AddText( str, x, y, start, end, targetWidth, halign, wrap );

        Cache.Draw( batch );

        return layout;
    }

    /// <summary>
    /// Draws text at the specified position.
    /// </summary>
    public GlyphLayout Draw( IBatch batch,
                             string str,
                             float x,
                             float y,
                             int start,
                             int end,
                             float targetWidth,
                             int halign,
                             bool wrap,
                             string truncate )
    {
        Cache.Clear();

        var layout = Cache.AddText( str, x, y, start, end, targetWidth, halign, wrap, truncate );

        Cache.Draw( batch );

        return layout;
    }

    /// <summary>
    /// Draws text at the specified position.
    /// </summary>
    public void Draw( IBatch batch, GlyphLayout layout, float x, float y )
    {
        Cache.Clear();
        Cache.AddText( layout, x, y );
        Cache.Draw( batch );
    }

    #endregion font drawing
}