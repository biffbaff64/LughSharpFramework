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

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.Versioning;

using JetBrains.Annotations;

using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class TexturePackerRect : IComparable< TexturePackerRect >
{
    public int  Score1;
    public int  Score2;
    public bool Rotated;
    public bool CanRotate = true;
    public bool IsPadded;
    public int  X;
    public int  Y;
    public int  Width;  // Portion of page taken by this region, including padding.
    public int  Height; // Portion of page taken by this region, including padding.
    public int  Index;
    public int  OffsetX;
    public int  OffsetY;
    public int  OriginalHeight;
    public int  OriginalWidth;
    public int  RegionHeight;
    public int  RegionWidth;

    public string?                    Name    = string.Empty;
    public List< TexturePackerAlias > Aliases = [ ];
    public int[]?                     Splits;
    public int[]?                     Pads;

    // ====================================================================

    private Bitmap?  _image;
    private FileInfo _file = null!;
    private bool     _isPatch;

    // ====================================================================

    public TexturePackerRect()
    {
    }

    public TexturePackerRect( TexturePackerRect rect )
    {
        X      = rect.X;
        Y      = rect.Y;
        Width  = rect.Width;
        Height = rect.Height;
    }

    public TexturePackerRect( Bitmap source, int left, int top, int newWidth, int newHeight, bool isPatch )
    {
        _image         = source;
        _isPatch       = isPatch;
        OffsetX        = left;
        OffsetY        = top;
        RegionWidth    = newWidth;
        RegionHeight   = newHeight;
        OriginalWidth  = source.Width;
        OriginalHeight = source.Height;
        Width          = newWidth;
        Height         = newHeight;
    }

    public int CompareTo( TexturePackerRect? o )
    {
        return string.Compare( Name, o?.Name, StringComparison.Ordinal );
    }

    public void UnloadImage( FileInfo fileInfo )
    {
        _file  = fileInfo;
        _image = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="imageProcessor"></param>
    /// <returns></returns>
    /// <exception cref="RuntimeException"></exception>
    public Bitmap GetImage( ImageProcessor imageProcessor )
    {
        if ( _image != null )
        {
            return _image;
        }

        Bitmap bitmap;

        try
        {
            bitmap = new Bitmap( Image.FromFile( _file.FullName ) );
        }
        catch ( IOException ex )
        {
            throw new RuntimeException( $"Error reading image: {_file.FullName}", ex );
        }

        if ( bitmap == null )
        {
            throw new RuntimeException( $"Unable to read image: {_file.FullName}" );
        }

        string?            name = _isPatch ? $"{Name}.9" : Name;
        TexturePackerRect? rect = imageProcessor.ProcessImage( bitmap, name );

        return rect == null
            ? throw new RuntimeException( "ProcessImage returned null" )
            : rect.GetImage( imageProcessor );
    }

    public void Set( TexturePackerRect rect )
    {
        Name           = rect.Name;
        _image         = rect._image;
        OffsetX        = rect.OffsetX;
        OffsetY        = rect.OffsetY;
        RegionWidth    = rect.RegionWidth;
        RegionHeight   = rect.RegionHeight;
        OriginalWidth  = rect.OriginalWidth;
        OriginalHeight = rect.OriginalHeight;
        X              = rect.X;
        Y              = rect.Y;
        Width          = rect.Width;
        Height         = rect.Height;
        Index          = rect.Index;
        Rotated        = rect.Rotated;
        Aliases        = rect.Aliases;
        Splits         = rect.Splits;
        Pads           = rect.Pads;
        CanRotate      = rect.CanRotate;
        Score1         = rect.Score1;
        Score2         = rect.Score2;
        _file          = rect._file;
        _isPatch       = rect._isPatch;
        IsPadded       = rect.IsPadded;
    }

    public static string GetAtlasName( string? name, bool flattenPaths )
    {
        Guard.Against.Null( name );

        return flattenPaths ? new FileInfo( name ).Name : name;
    }

    /// <inheritdoc />
    [SuppressMessage( "ReSharper", "NonReadonlyMemberInGetHashCode" )]
    public override int GetHashCode()
    {
        // If two objects are equal (based only on Name), they MUST have the same hash code.
        // If the hash code is based on other fields, it violates the hash contract.

        // Use the modern HashCode struct for efficient hashing of a single field.
        var hash = new HashCode();
        hash.Add( Name, StringComparer.Ordinal );

        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override bool Equals( object? obj )
    {
        if ( this == obj )
        {
            return true;
        }

        if ( obj == null )
        {
            return false;
        }

        if ( GetType() != obj.GetType() )
        {
            return false;
        }

        var other = ( TexturePackerRect )obj;

        if ( Name == null )
        {
            if ( other.Name != null )
            {
                return false;
            }
        }
        else
        {
            if ( !Name.Equals( other.Name ) )
            {
                return false;
            }
        }

        return true;
    }

    public void DebugPrint()
    {
        Logger.Block();
        Logger.Debug( $"Name: {Name}" );
        Logger.Debug( $"X: {X}" );
        Logger.Debug( $"Y: {Y}" );
        Logger.Debug( $"Width: {Width}: " );
        Logger.Debug( $"Height: {Height}: " );
        Logger.Debug( $"Rotated: {Rotated}" );
        Logger.Debug( $"IsPadded: {IsPadded}: " );
        Logger.EndBlock();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Name}{( Index != -1 ? $"_{Index}" : "" )}[{X},{Y} {Width}x{Height}]";
    }
}

// ============================================================================
// ============================================================================