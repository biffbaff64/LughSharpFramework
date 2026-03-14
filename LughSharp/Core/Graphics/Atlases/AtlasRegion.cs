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

using JetBrains.Annotations;

using LughSharp.Core.Graphics.Images;

namespace LughSharp.Core.Graphics.Atlases;

/// <summary>
/// 
/// </summary>
[PublicAPI]
public class AtlasRegion : TextureRegion, IDisposable
{
    /// <summary>
    /// The number at the end of the original image file name, or -1 if none.
    /// When sprites are packed, if the original file name ends with a number,
    /// it is stored as the index and is not considered as part of the sprite's
    /// name. This is useful for keeping animation frames in order.
    /// </summary>
    public int Index { get; set; } = -1;

    /// <summary>
    /// The name of the original image file, without the files extension.
    /// If the name ends with an underscore followed by only numbers, that part
    /// is excluded: underscores denote special instructions to the texture packer.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The offset from the left of the original image to the left of the packed
    /// image, after whitespace was removed for packing.
    /// </summary>
    public float OffsetX { get; set; }

    /// <summary>
    /// The offset from the bottom of the original image to the bottom of the packed
    /// image, after whitespace was removed for packing.
    /// </summary>
    public float OffsetY { get; set; }

    /// <summary>
    /// The width of the image, after whitespace was removed for packing.
    /// </summary>
    public int PackedWidth { get; set; }

    /// <summary>
    /// The height of the image, after whitespace was removed for packing.
    /// </summary>
    public int PackedHeight { get; set; }

    /// <summary>
    /// The width of the image, before whitespace was removed and rotation
    /// was applied for packing.
    /// </summary>
    public int OriginalWidth { get; set; }

    /// <summary>
    /// The height of the image, before whitespace was removed for packing.
    /// </summary>
    public int OriginalHeight { get; set; }

    /// <summary>
    /// If true, the region has been rotated 90 degrees counter clockwise.
    /// </summary>
    public bool Rotate { get; set; }

    /// <summary>
    /// The degrees the region has been rotated, counter clockwise between 0 and 359.
    /// Most atlas region handling deals only with 0 or 90 degree rotation (enough to
    /// handle rectangles). More advanced texture packing may support other rotations
    /// (eg, for tightly packing polygons).
    /// </summary>
    public int Degrees { get; set; }

    /// <summary>
    /// Custom name/value pairs for this region. Using a Dictionary for 
    /// O(1) lookup and cleaner syntax than parallel arrays.
    /// </summary>
    public Dictionary< string, int[]? > NameValuePairs { get; set; } = [ ];

    // ========================================================================

    private bool _isDisposed;

    // ========================================================================

    /// <summary>
    /// Creates a new AtlasRegion using the specified texture, coordinates, and dimensions.
    /// </summary>
    /// <param name="texture"> The texture from which to extract the region. </param>
    /// <param name="x"> The X coord of the region. </param>
    /// <param name="y"> The Y coord of the region. </param>
    /// <param name="width"> The region width. </param>
    /// <param name="height"> The region height. </param>
    public AtlasRegion( Texture? texture, int x, int y, int width, int height )
        : base( texture!, x, y, width, height )
    {
        OriginalWidth  = width;
        OriginalHeight = height;
        PackedWidth    = width;
        PackedHeight   = height;
    }

    /// <summary>
    /// Creates a new AtlasRegion with the same values as the specified region.
    /// Performs a shallow copy of the Properties dictionary.
    /// </summary>
    public AtlasRegion( AtlasRegion region )
    {
        SetRegion( region );

        Index          = region.Index;
        Name           = region.Name;
        OffsetX        = region.OffsetX;
        OffsetY        = region.OffsetY;
        PackedWidth    = region.PackedWidth;
        PackedHeight   = region.PackedHeight;
        OriginalWidth  = region.OriginalWidth;
        OriginalHeight = region.OriginalHeight;
        Rotate         = region.Rotate;
        Degrees        = region.Degrees;

        // Create a new dictionary instance so clones don't interfere with each other
        NameValuePairs = new Dictionary< string, int[]? >( region.NameValuePairs );
    }

    /// <summary>
    /// Creates a new AtlasRegion using the supplied <see cref="TextureRegion"/>.
    /// </summary>
    public AtlasRegion( TextureRegion region )
    {
        SetRegion( region );

        PackedWidth    = region.GetRegionWidth();
        PackedHeight   = region.GetRegionHeight();
        OriginalWidth  = PackedWidth;
        OriginalHeight = PackedHeight;
    }

    // ========================================================================

    /// <summary>
    /// Returns the packed width considering the <see cref="Rotate"/> value, if it is
    /// true then it returns the packedHeight, otherwise it returns the packedWidth.
    /// </summary>
    public float RotatedPackedWidth => Rotate ? PackedHeight : PackedWidth;

    /// <summary>
    /// Returns the packed height considering the <see cref="Rotate"/> value, if it is
    /// true then it returns the packedWidth, otherwise it returns the packedHeight.
    /// </summary>
    public float RotatedPackedHeight => Rotate ? PackedWidth : PackedHeight;

    /// <summary>
    /// Flips the region horizontally and/or vertically.
    /// </summary>
    /// <param name="x"> True to flip horizontally. </param>
    /// <param name="y"> False to flip vertically. </param>
    public override void Flip( bool x, bool y )
    {
        base.Flip( x, y );

        if ( x )
        {
            OffsetX = OriginalWidth - OffsetX - RotatedPackedWidth;
        }

        if ( y )
        {
            OffsetY = OriginalHeight - OffsetY - RotatedPackedHeight;
        }
    }

    /// <summary>
    /// Returns the value associated with the specified name, or null if not found.
    /// </summary>
    public int[]? FindValue( string name )
    {
        return NameValuePairs.GetValueOrDefault( name );
    }

    /// <inheritdoc />
    public override string? ToString()
    {
        return Name;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    protected void Dispose( bool disposing )
    {
        if ( !_isDisposed )
        {
            if ( disposing )
            {
            }

            _isDisposed = true;
        }
    }
}

// ============================================================================
// ============================================================================