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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using JetBrains.Annotations;
using LughUtils.source.Collections;
using LughUtils.source.Exceptions;
using LughUtils.source.Logging;
using Bitmap = System.Drawing.Bitmap;
using Image = System.Drawing.Image;

namespace Extensions.Source.Tools.TexturePacker;

[SupportedOSPlatform( "windows" )]
public partial class TexturePacker
{
    [PublicAPI]
    public class Page
    {
        public string?      ImageName      { get; set; } = "";
        public List< Rect > OutputRects    { get; set; } = [ ];
        public List< Rect > RemainingRects { get; set; } = [ ];
        public float        Occupancy      { get; set; }
        public int          X              { get; set; }
        public int          Y              { get; set; }
        public int          Width          { get; set; }
        public int          Height         { get; set; }
        public int          ImageWidth     { get; set; }
        public int          ImageHeight    { get; set; }

        public void Debug()
        {
            Logger.Block();
            Logger.Debug( $"ImageName: {ImageName}" );
            Logger.Debug( $"OutputRects: {OutputRects.Count}" );
            Logger.Debug( $"RemainingRects: {RemainingRects.Count}" );
            Logger.Debug( $"Occupancy: {Occupancy}" );
            Logger.Debug( $"X: {X}" );
            Logger.Debug( $"Y: {Y}" );
            Logger.Debug( $"Width: {Width}" );
            Logger.Debug( $"Height: {Height}" );
            Logger.Debug( $"ImageWidth: {ImageWidth}" );
            Logger.Debug( $"ImageHeight: {ImageHeight}" );
            Logger.EndBlock();
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Alias : IComparable< Alias >
    {
        public int     Index;
        public int     OffsetX;
        public int     OffsetY;
        public int     OriginalHeight;
        public int     OriginalWidth;
        public string? Name;
        public int[]?  Pads;
        public int[]?  Splits;

        public Alias( Rect rect )
        {
            Index          = rect.Index;
            Name           = rect.Name;
            OffsetX        = rect.OffsetX;
            OffsetY        = rect.OffsetY;
            OriginalHeight = rect.OriginalHeight;
            OriginalWidth  = rect.OriginalWidth;
            Pads           = rect.Pads;
            Splits         = rect.Splits;
        }

        public void Apply( Rect rect )
        {
            rect.Name           = Name;
            rect.Index          = Index;
            rect.Splits         = Splits;
            rect.Pads           = Pads;
            rect.OffsetX        = OffsetX;
            rect.OffsetY        = OffsetY;
            rect.OriginalWidth  = OriginalWidth;
            rect.OriginalHeight = OriginalHeight;
        }

        public int CompareTo( Alias? o )
        {
            return string.Compare( Name, o?.Name, StringComparison.Ordinal );
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Rect : IComparable< Rect >
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

        public string?       Name    = string.Empty;
        public List< Alias > Aliases = [ ];
        public int[]?        Splits;
        public int[]?        Pads;

        // ====================================================================

        private Bitmap?  _image;
        private FileInfo _file = null!;
        private bool     _isPatch;

        // ====================================================================

        public Rect()
        {
        }

        public Rect( Rect rect )
        {
            X      = rect.X;
            Y      = rect.Y;
            Width  = rect.Width;
            Height = rect.Height;
        }

        public Rect( Bitmap source, int left, int top, int newWidth, int newHeight, bool isPatch )
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

        public int CompareTo( Rect? o )
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
        /// <exception cref="GdxRuntimeException"></exception>
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
                throw new GdxRuntimeException( $"Error reading image: {_file.FullName}", ex );
            }

            if ( bitmap == null )
            {
                throw new GdxRuntimeException( $"Unable to read image: {_file.FullName}" );
            }

            var name = _isPatch ? $"{Name}.9" : Name;
            var rect = imageProcessor.ProcessImage( bitmap, name );

            return rect == null
                ? throw new GdxRuntimeException( "ProcessImage returned null" )
                : rect.GetImage( imageProcessor );
        }

        public void Set( Rect rect )
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
            hash.Add(Name, StringComparer.Ordinal);
    
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

            var other = ( Rect )obj;

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

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public interface IPacker
    {
        List< Page > Pack( List< Rect > inputRects );

        List< Page > Pack( TexturePackerProgressListener progressListener,
                           List< Rect > inputRects );
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class InputImage
    {
        public FileInfo? FileInfo { get; set; }
        public string?   RootPath { get; set; }
        public string?   Name     { get; set; }
        public Bitmap?   Image    { get; set; }

        public void DebugPrint()
        {
            Logger.Debug( $"FileInfo: {FileInfo?.FullName}" );
            Logger.Debug( $"RootPath: {RootPath}" );
            Logger.Debug( $"Name: {Name}" );
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class TexturePackerProgressListener
    {
        public int  Count    { get; set; }
        public int  Total    { get; set; }
        public bool Canceled { get; set; }

        // ====================================================================

        private readonly List< float > _portions = new( 8 );

        private float _scale = 1;
        private float _lastUpdate;

        // ====================================================================

        public void Start( float portion )
        {
            if ( portion == 0 )
            {
                throw new ArgumentException( "portion cannot be 0." );
            }

            _portions.Add( _lastUpdate );
            _portions.Add( _scale * portion );
            _portions.Add( _scale );

            _scale *= portion;
        }

        public bool Update( int count, int total )
        {
            Update( total == 0 ? 0 : count / ( float )total );

            return Canceled;
        }

        public void Update( float percent )
        {
            _lastUpdate = _portions[ _portions.Count - 3 ]
                        + ( _portions[ _portions.Count - 2 ] * percent );

            Progress( _lastUpdate );
        }

        public void End()
        {
            _scale = _portions.Pop();

            var portion = _portions.Pop();

            _lastUpdate = _portions.Pop() + portion;

            Progress( _lastUpdate );
        }

        public void Reset()
        {
            _scale  = 1;
            Message = "";
            Count   = 0;
            Total   = 0;

            Progress( 0 );
        }

        public string Message
        {
            get;
            set
            {
                field = value;

                Progress( _lastUpdate );
            }
        } = "";

        protected virtual void Progress( float progress )
        {
        }
    }
}