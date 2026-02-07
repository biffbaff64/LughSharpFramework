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

using System;
using System.Collections.Generic;
using System.IO;

using JetBrains.Annotations;
using LughSharp.Core.Graphics.OpenGL.Enums;
using LughSharp.Core.Utils.Exceptions;
using Exception = System.Exception;

namespace LughSharp.Core.Graphics.Atlases;

[PublicAPI]
public class TextureAtlasData
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
    public TextureAtlasData( FileInfo packFile, DirectoryInfo imagesDir, bool flip = false )
    {
        try
        {
            Load( packFile, imagesDir, flip );
        }
        catch ( Exception ex )
        {
            throw new RuntimeException( $"Error reading texture atlas file: {packFile}", ex );
        }
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="packFile"></param>
    /// <param name="imagesDir"></param>
    /// <param name="flip"></param>
    /// <exception cref="RuntimeException"></exception>
    public void Load( FileInfo packFile, DirectoryInfo imagesDir, bool flip )
    {
        var entry = new string[ 5 ];

        // Page field parsers
        var pageFields = new Dictionary< string, Action< Page > >
        {
            [ "size" ] = page =>
            {
                page.Width  = int.Parse( entry[ 1 ] );
                page.Height = int.Parse( entry[ 2 ] );
            },
            [ "format" ] = page =>
            {
                page.Format = int.Parse( entry[ 1 ] );
            },
            [ "filter" ] = page =>
            {
                page.MinFilter = Enum.Parse< TextureFilterMode >( entry[ 1 ] );
                page.MagFilter = Enum.Parse< TextureFilterMode >( entry[ 2 ] );
                page.UseMipMaps = ( page.MinFilter != TextureFilterMode.Nearest )
                                  && ( page.MinFilter != TextureFilterMode.Linear );
            },
            [ "repeat" ] = page =>
            {
                if ( entry[ 1 ].Contains( 'x' ) )
                {
                    page.UWrap = TextureWrapMode.Repeat;
                }

                if ( entry[ 1 ].Contains( 'y' ) )
                {
                    page.VWrap = TextureWrapMode.Repeat;
                }
            },
            [ "pma" ] = page =>
            {
                page.PreMultipliedAlpha = entry[ 1 ] == "true";
            }
        };

        var hasIndexes = false;

        // Region field parsers
        var regionFields = new Dictionary< string, Action< Region > >
        {
            [ "xy" ] = region =>
            {
                region.Left = int.Parse( entry[ 1 ] );
                region.Top  = int.Parse( entry[ 2 ] );
            },
            [ "size" ] = region =>
            {
                region.Width  = int.Parse( entry[ 1 ] );
                region.Height = int.Parse( entry[ 2 ] );
            },
            [ "bounds" ] = region =>
            {
                region.Left   = int.Parse( entry[ 1 ] );
                region.Top    = int.Parse( entry[ 2 ] );
                region.Width  = int.Parse( entry[ 3 ] );
                region.Height = int.Parse( entry[ 4 ] );
            },
            [ "offset" ] = region =>
            {
                region.OffsetX = int.Parse( entry[ 1 ] );
                region.OffsetY = int.Parse( entry[ 2 ] );
            },
            [ "orig" ] = region =>
            {
                region.OriginalWidth  = int.Parse( entry[ 1 ] );
                region.OriginalHeight = int.Parse( entry[ 2 ] );
            },
            [ "offsets" ] = region =>
            {
                region.OffsetX        = int.Parse( entry[ 1 ] );
                region.OffsetY        = int.Parse( entry[ 2 ] );
                region.OriginalWidth  = int.Parse( entry[ 3 ] );
                region.OriginalHeight = int.Parse( entry[ 4 ] );
            },
            [ "rotate" ] = region =>
            {
                var value = entry[ 1 ];

                if ( value == "true" )
                {
                    region.Degrees = 90;
                }
                else if ( ( value != "false" )
                         && ( int.TryParse( value, out var degrees ) ) )
                {
                    region.Degrees = degrees;
                }

                region.Rotate = region.Degrees == 90;
            },
            [ "index" ] = region =>
            {
                region.Index = int.Parse( entry[ 1 ] );

                if ( region.Index != -1 )
                {
                    hasIndexes = true;
                }
            }
        };

        using var reader = new StreamReader( packFile.FullName );

        try
        {
            var line = reader.ReadLine();

            // Ignore empty lines before first entry
            while ( ( line != null ) && ( line.Trim().Length == 0 ) )
            {
                line = reader.ReadLine();
            }

            // Header entries
            while ( true )
            {
                if ( ( line == null ) || ( line.Trim().Length == 0 ) )
                {
                    break;
                }

                if ( ReadEntry( entry, line ) == 0 )
                {
                    break;
                }

                line = reader.ReadLine();
            }

            // Page and region entries
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
                    if ( imagesDir != null )
                    {
                        page = new Page
                        {
                            TextureFile = new FileInfo( Path.Combine( imagesDir.FullName, line ) ),
                        };

                        while ( true )
                        {
                            if ( ReadEntry( entry, line = reader.ReadLine() ) == 0 )
                            {
                                break;
                            }

                            if ( pageFields.TryGetValue( entry[ 0 ], out var field ) )
                            {
                                field( page );
                            }
                        }

                        Pages.Add( page );
                    }
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

                    while ( true )
                    {
                        var count = ReadEntry( entry, line = reader.ReadLine() );

                        if ( count == 0 )
                        {
                            break;
                        }

                        if ( regionFields.TryGetValue( entry[ 0 ], out var field ) )
                        {
                            field( region );
                        }
                        else
                        {
                            names  ??= new List< string >( 8 );
                            values ??= new List< int[] >( 8 );
                            names.Add( entry[ 0 ] );

                            var entryValues = new int[ count ];

                            for ( var i = 0; i < count; i++ )
                            {
                                if ( int.TryParse( entry[ i + 1 ], out var val ) )
                                {
                                    entryValues[ i ] = val;
                                }
                            }

                            values.Add( entryValues );
                        }
                    }

                    if ( region is { OriginalWidth : 0, OriginalHeight: 0 } )
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
            throw new RuntimeException( $"Error reading texture atlas file: {packFile}", ex );
        }

        // Sort regions by index if needed
        if ( hasIndexes )
        {
            Regions.Sort( ( region1, region2 ) =>
            {
                var i1 = region1.Index != -1 ? region1.Index : int.MaxValue;
                var i2 = region2.Index != -1 ? region2.Index : int.MaxValue;

                return i1.CompareTo( i2 );
            } );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// <param name="line"></param>
    /// <returns></returns>
    private int ReadEntry( string[] entry, string? line )
    {
        if ( line == null )
        {
            return 0;
        }

        line = line.Trim();

        if ( line.Length == 0 )
        {
            return 0;
        }

        var colon = line.IndexOf( ':' );

        if ( colon == -1 )
        {
            return 0;
        }

        entry[ 0 ] = line.Substring( 0, colon ).Trim();

        for ( int i = 1, lastMatch = colon + 1;; i++ )
        {
            var comma = line.IndexOf( ',', lastMatch );

            if ( comma == -1 )
            {
                entry[ i ] = line.Substring( lastMatch ).Trim();

                return i;
            }

            entry[ i ] = line.Substring( lastMatch, comma - lastMatch ).Trim();

            lastMatch = comma + 1;

            if ( i == 4 )
            {
                return 4;
            }
        }
    }

    // ========================================================================
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
        /// case, <see cref="Texture"/> must be set.
        /// </summary>
        public FileInfo? TextureFile { get; set; }

        public bool              UseMipMaps         { get; set; }
        public int               Format             { get; set; } = LughFormat.RGBA8888; // Gdx2DPixmap.GDX_2D_FORMAT_RGBA8888;
        public TextureFilterMode MinFilter          { get; set; } = TextureFilterMode.Nearest;
        public TextureFilterMode MagFilter          { get; set; } = TextureFilterMode.Nearest;
        public TextureWrapMode   UWrap              { get; set; } = TextureWrapMode.ClampToEdge;
        public TextureWrapMode   VWrap              { get; set; } = TextureWrapMode.ClampToEdge;
        public float             Width              { get; set; }
        public float             Height             { get; set; }
        public bool              PreMultipliedAlpha { get; set; }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Region
    {
        public Page?     Page           { get; set; }
        public string    Name           { get; set; } = string.Empty;
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

// ============================================================================
// ============================================================================