﻿// ///////////////////////////////////////////////////////////////////////////////
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
using LughSharp.Lugh.Utils.Collections;
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
            Logger.Debug( $"packFile FullName: {packFile.FullName}" );
            Logger.Debug( $"imagesDir FullName: {imagesDir?.FullName}" );

            Load( packFile, imagesDir, flip );
        }
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="packFile"></param>
    /// <param name="imagesDir"></param>
    /// <param name="flip"></param>
    /// <exception cref="GdxRuntimeException"></exception>
    private void Load( FileInfo? packFile, DirectoryInfo? imagesDir, bool flip )
    {
        Guard.ThrowIfNull( packFile );

        ObjectMap< string, IField< Page > > pageFields = new( 15, 0.99f )
        {
            { "size", new PageFieldSize() },
            { "format", new PageFieldFormat() },
            { "filter", new PageFieldFilter() },
            { "repeat", new PageFieldRepeat() },
            { "pma", new PageFieldPma() },
        };

        ObjectMap< string, IField< Region > > regionFields = new( 127, 0.99f )
        {
            { "xy", new RegionFieldXY() },
            { "size", new RegionFieldSize() },
            { "bounds", new RegionFieldBounds() },
            { "offset", new RegionFieldOffset() },
            { "orig", new RegionFieldOrig() },
            { "offsets", new RegionFieldOffsets() },
            { "rotate", new RegionFieldRotate() },
            { "index", new RegionFieldIndex() },
        };

        var reader = new StreamReader( packFile.FullName, false );

        try
        {
            var line = ReadLine();

            // Ignore empty lines before first entry.
            while ( ( line != null ) && ( line.Trim().Length == 0 ) )
            {
                line = ReadLine();
            }

            // Header entries.
            while ( true )
            {
                if ( ( line == null ) || ( line.Trim().Length == 0 ) )
                {
                    break;
                }

                if ( ReadEntry( line ) == 0 )
                {
                    break; // Silently ignore all header fields.
                }

                line = ReadLine();
            }

            // Page and region entries.
            Page?           page   = null;
            List< string >? names  = null;
            List< int[] >?  values = null;

            Logger.Debug( $"Packfile: {packFile.FullName}" );
            Logger.Debug( $"Packfile Dir: {packFile.Directory?.FullName}" );

            while ( true )
            {
                if ( line == null )
                {
                    break;
                }

                if ( line.Trim().Length == 0 )
                {
                    page = null;
                    line = ReadLine();
                }
                else if ( page == null )
                {
                    line = IOUtils.NormalizePath( $"{packFile.Directory?.FullName}/{line}" );

                    Logger.Debug( $"line: {line}" );

                    page = new Page
                    {
                        TextureFile = new FileInfo( line ),
                    };

                    while ( true )
                    {
                        if ( ReadEntry( line = ReadLine() ) == 0 )
                        {
                            break;
                        }

                        var field = pageFields.Get( Entry[ 0 ] );

                        field?.Parse( page, Entry ); // Silently ignore unknown page fields.
                    }

                    Pages.Add( page );
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
                        var count = ReadEntry( line = ReadLine() );

                        if ( count == 0 )
                        {
                            break;
                        }

                        var field = regionFields.Get( Entry[ 0 ] );

                        if ( field != null )
                        {
                            field.Parse( region, Entry );
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
                                catch ( NumberFormatException )
                                {
                                    // Silently ignore non-integer values.
                                }
                            }

                            values?.Add( entryValues );
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

        return;

        // --------------------------------------------------------------------
        //TODO: When testing is complete, remove this and replace calls
        // with reader.ReadLine()
        //
        string? ReadLine()
        {
            var line = reader.ReadLine();

//            Logger.Debug( $"line: {line}" );

            return line;
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

        Entry[ 0 ] = line.Substring( 0, colon ).Trim();

        for ( int i = 1, lastMatch = colon + 1;; i++ )
        {
            var comma = line.IndexOf( ',', lastMatch );

            if ( comma == -1 )
            {
                Entry[ i ] = line.Substring( lastMatch ).Trim();

                return i;
            }

            Entry[ i ] = line.Substring( lastMatch, comma - lastMatch ).Trim();

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

        public bool                          UseMipMaps         { get; set; }
        public Gdx2DPixmap.Gdx2DPixmapFormat Format             { get; set; } = Gdx2DPixmap.Gdx2DPixmapFormat.RGBA8888;
        public TextureFilterMode             MinFilter          { get; set; } = TextureFilterMode.Nearest;
        public TextureFilterMode             MagFilter          { get; set; } = TextureFilterMode.Nearest;
        public TextureWrapMode               UWrap              { get; set; } = TextureWrapMode.ClampToEdge;
        public TextureWrapMode               VWrap              { get; set; } = TextureWrapMode.ClampToEdge;
        public float                         Width              { get; set; }
        public float                         Height             { get; set; }
        public bool                          PreMultipliedAlpha { get; set; }
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