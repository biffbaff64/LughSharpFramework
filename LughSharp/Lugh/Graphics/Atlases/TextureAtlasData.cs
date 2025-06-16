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
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

using Exception = System.Exception;

namespace LughSharp.Lugh.Graphics.Atlases;

[PublicAPI]
public partial class TextureAtlasData
{
    public List< Page >   Pages   { get; set; } = [ ];
    public List< Region > Regions { get; set; } = [ ];
    public string[]       Entry   { get; set; } = new string[ 5 ];

    // ========================================================================

    internal static readonly bool[] HasIndexes = [ false ];

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="packFile"></param>
    /// <param name="imagesDir"></param>
    /// <param name="flip"></param>
    public TextureAtlasData( FileInfo? packFile = null, DirectoryInfo? imagesDir = null, bool flip = false )
    {
        if ( packFile != null )
        {
            Load( packFile, imagesDir, flip );
        }
    }

    // ========================================================================

    private void Load( FileInfo? packFile, DirectoryInfo? imagesDir, bool flip )
    {
        ArgumentNullException.ThrowIfNull( packFile );
        
        //@formatter:off
        Dictionary< string, IField< Page > > pageFields = new( 15 )
        {
            { "size",       new PageFieldSize()   },
            { "format",     new PageFieldFormat() },
            { "filter",     new PageFieldFilter() },
            { "repeat",     new PageFieldRepeat() },
            { "pma",        new PageFieldPma()    },
        };

        Dictionary< string, IField< Region > > regionFields = new( 127 )
        {
            { "rotate",     new RegionFieldRotate()  },
            { "xy",         new RegionFieldXY()      },
            { "size",       new RegionFieldSize()    },
            { "bounds",     new RegionFieldBounds()  },
            { "orig",       new RegionFieldOrig()    },
            { "offset",     new RegionFieldOffset()  },
            { "offsets",    new RegionFieldOffsets() },
            { "index",      new RegionFieldIndex()   },
        };
        //@formatter:on

        var reader = new StreamReader( packFile.FullName, false );

        Logger.Checkpoint();
        
        try
        {
            var line = reader.ReadLine();

            // Ignore empty lines before first entry.
            while ( ( line != null ) && ( line.Trim().Length == 0 ) )
            {
                line = reader.ReadLine();
            }

            // Header entries.
            while ( true )
            {
                if ( ( line == null ) || ( line.Trim().Length == 0 ) )
                {
                    break;
                }

                Logger.Debug( "Call #1" );
                
                if ( ReadEntry( line ) == 0 )
                {
                    Logger.Checkpoint();
                    
                    // Silently ignore all header fields.
                    break;
                }

                Logger.Checkpoint();
                
                line = reader.ReadLine();
                
                Logger.Checkpoint();
            }

            // Page and region entries.
            Page?           page   = null;
            List< string >? names  = null;
            List< int[] >?  values = null;

            while ( true )
            {
                if ( line == null )
                {
                    break;
                }

                if ( line.Trim().Length == 0 )
                {
                    page = null;
                    line = reader.ReadLine();
                }
                else if ( page == null )
                {
                    Logger.Debug( $"line.Trim(): {IOUtils.NormalizePath( line.Trim() )}" );
                    
                    page = new Page
                    {
                        TextureFile = new FileInfo( IOUtils.NormalizePath( line.Trim() ) ),
                    };

                    Logger.Debug( $"page.TextureFile: {page.TextureFile.FullName}" );
                    Logger.Debug( "Call #2" );
                    
                    while ( true )
                    {
                        if ( ReadEntry( line = reader.ReadLine() ) == 0 )
                        {
                            break;
                        }

                        pageFields?[ Entry[ 0 ] ].Parse( page );
                    }

                    Logger.Checkpoint();
                    
                    Pages.Add( page );
                    
                    Logger.Checkpoint();
                }
                else
                {
                    var region = new Region
                    {
                        Page = page,
                        Name = line.Trim(),
                    };

                    if ( flip )
                    {
                        region.Flip = true;
                    }

                    Logger.Debug( "Call #3" );
                    
                    while ( true )
                    {
                        var count = ReadEntry( line = reader.ReadLine() );

                        if ( count == 0 )
                        {
                            break;
                        }

                        if ( regionFields?[ Entry[ 0 ] ] != null )
                        {
                            regionFields[ Entry[ 0 ] ].Parse( region );
                        }
                        else
                        {
                            if ( names == null )
                            {
                                names  = new List< string >( 8 );
                                values = new List< int[] >( 8 );
                            }

                            names.Add( Entry[ 0 ] );

                            var entryValues = new int[ count ];

                            for ( var i = 0; i < count; i++ )
                            {
                                try
                                {
                                    entryValues[ i ] = int.Parse( Entry[ i + 1 ] );
                                }
                                catch ( FormatException )
                                {
                                    // Silently ignore non-integer values.
                                }
                            }

                            values?.Add( entryValues );
                        }
                    }

                    if ( region is { OriginalWidth: 0, OriginalHeight: 0 } )
                    {
                        region.OriginalWidth  = region.Width;
                        region.OriginalHeight = region.Height;
                    }

                    if ( names is { Count: > 0 } )
                    {
                        region.Names  = names.ToArray();
                        region.Values = values?.ToArray();
                        
                        names.Clear();
                        values?.Clear();
                    }

                    Regions.Add( region );
                }
            }
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( "Error reading texture atlas file: " + packFile, ex );
        }
        finally
        {
            reader.Close();
        }

        if ( HasIndexes[ 0 ] )
        {
            Regions.Sort( new ComparatorAnonymousInnerClass( this ) );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    private int ReadEntry( string? line )
    {
        if ( line == null )
        {
            return 0;
        }

        Logger.Debug( line );
        
        line = line.Trim();

        Logger.Debug( line );

        if ( line.Length == 0 )
        {
            return 0;
        }

        var colon = line.IndexOf( ':' );

        Logger.Debug( $"colon: {colon}" );
        
        if ( colon == -1 )
        {
            return 0;
        }

        Entry[ 0 ] = line.Substring( 0, colon ).Trim();

        Logger.Debug( $"Entry[ 0 ]: {Entry[ 0 ]}" );

        for ( int i = 1, lastMatch = colon + 1;; i++ )
        {
            var comma = line.IndexOf( ',', lastMatch );

            if ( comma == -1 )
            {
                Entry[ i ] = line.Substring( lastMatch ).Trim();

                return i;
            }

            Entry[ i ] = line.Substring( lastMatch, comma ).Trim();

            lastMatch = comma + 1;

            if ( i == 4 )
            {
                return 4;
            }
        }
    }

    // ========================================================================

    [PublicAPI]
    protected interface IField< in T >
    {
        void Parse( T obj, params string[] entry );
    }

    // ========================================================================
    // ========================================================================
    
    [PublicAPI]
    public record Page
    {
        /// <summary>
        /// May be null if the texture is not yet loaded.
        /// </summary>
        public Texture? Texture { get; set; }

        /// <summary>
        /// May be null if this page isn't associated with a file. In that
        /// case, <see cref="Texture" /> must be set.
        /// </summary>
        public FileInfo? TextureFile { get; set; }

        public bool                  UseMipMaps         { get; set; }
        public PixelType.Format      Format             { get; set; } = PixelType.Format.RGBA8888;
        public Texture.TextureFilter MinFilter          { get; set; } = Texture.TextureFilter.Nearest;
        public Texture.TextureFilter MagFilter          { get; set; } = Texture.TextureFilter.Nearest;
        public Texture.TextureWrap   UWrap              { get; set; } = Texture.TextureWrap.ClampToEdge;
        public Texture.TextureWrap   VWrap              { get; set; } = Texture.TextureWrap.ClampToEdge;
        public float                 Width              { get; set; }
        public float                 Height             { get; set; }
        public bool                  PreMultipliedAlpha { get; set; }
    }

    [PublicAPI]
    public class Region
    {
        public Page?     Page           { get; init; }
        public string?   Name           { get; init; }
        public int       Left           { get; set; }
        public int       Top            { get; set; }
        public int       Width          { get; set; }
        public int       Height         { get; set; }
        public float     OffsetX        { get; set; }
        public float     OffsetY        { get; set; }
        public int       OriginalWidth  { get; set; }
        public int       OriginalHeight { get; set; }
        public int       Degrees        { get; set; }
        public bool      Rotate         { get; set; }
        public bool      Flip           { get; set; }
        public int       Index          { get; set; } = -1;
        public string[]? Names          { get; set; }
        public int[]?[]? Values         { get; set; }

        public int[]? FindValue( string name )
        {
            if ( Names != null )
            {
                for ( int i = 0, n = Names.Length; i < n; i++ )
                {
                    if ( name.Equals( Names[ i ] ) )
                    {
                        return Values?[ i ];
                    }
                }
            }

            return null;
        }
    }
}

