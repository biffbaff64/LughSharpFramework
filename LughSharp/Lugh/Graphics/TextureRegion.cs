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

namespace LughSharp.Lugh.Graphics;

/// <summary>
/// Defines a rectangular area of a texture. The coordinate system used has
/// its origin in the upper left corner with the x-axis pointing to the
/// right and the y axis pointing downwards.
/// </summary>
[PublicAPI]
public class TextureRegion
{
    /// <summary>
    /// </summary>
    public Texture? Texture { get; set; }

    /// <summary>
    /// </summary>
    public virtual float U { get; set; }

    /// <summary>
    /// </summary>
    public virtual float U2 { get; set; }

    /// <summary>
    /// </summary>
    public virtual float V { get; set; }

    /// <summary>
    /// </summary>
    public virtual float V2 { get; set; }
    
    // ========================================================================

    /// <summary>
    /// Constructs a region that cannot be used until a texture and texture
    /// coordinates are set.
    /// </summary>
    public TextureRegion()
    {
    }

    /// <summary>
    /// Represents a portion of a texture, defined by specific coordinates,
    /// width, and height. Used for rendering only a subsection of the
    /// original texture.
    /// </summary>
    public TextureRegion( Texture? texture )
    {
        Texture = texture ?? throw new GdxRuntimeException( "Cannot create TextureRegion from null texture." );

        SetRegion( 0, 0, texture.Width, texture.Height );
    }

    /// <summary>
    /// Represents a rectangular region of a texture, defining texture coordinates
    /// and dimensions. Can be used to sample a sub-section of a texture image for
    /// rendering.
    /// </summary>
    /// <param name="texture"> The texture from which to extract the region. </param>
    /// <param name="width">
    /// The width of the texture region. May be negative to flip the sprite when drawn.
    /// </param>
    /// <param name="height">
    /// The height of the texture region. May be negative to flip the sprite when drawn.
    /// </param>
    public TextureRegion( Texture? texture, int width, int height )
        : this( texture, 0, 0, width, height )
    {
    }

    /// <summary>
    /// Represents a section of a texture, defined by specific coordinates and dimensions.
    /// Can be used to render a portion of the texture or to create sprite animations.
    /// </summary>
    /// <param name="texture"> The texture from which to extract the region. </param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width">
    /// The width of the texture region. May be negative to flip the sprite when drawn.
    /// </param>
    /// <param name="height">
    /// The height of the texture region. May be negative to flip the sprite when drawn.
    /// </param>
    public TextureRegion( Texture? texture, int x, int y, int width, int height )
    {
        Texture = texture ?? throw new GdxRuntimeException( "Cannot create TextureRegion from null texture." );
        SetRegion( x, y, width, height );
    }

    /// <summary>
    /// Represents a section of a texture, defined by specific coordinates and dimensions.
    /// Can be used to render a portion of the texture or to create sprite animations.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="u2"></param>
    /// <param name="v2"></param>
    /// <exception cref="GdxRuntimeException"></exception>
    public TextureRegion( Texture? texture, float u, float v, float u2, float v2 )
    {
        Texture = texture ?? throw new GdxRuntimeException( "Cannot create TextureRegion from null texture." );
        SafeSetRegion( u, v, u2, v2 );
    }

    /// <summary>
    /// Represents a rectangular region within a texture, with properties and methods
    /// for manipulation such as flipping, scrolling, and splitting.
    /// </summary>
    public TextureRegion( TextureRegion region )
    {
        SetRegion( region );
    }

    /// <summary>
    /// </summary>
    /// <param name="region"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public TextureRegion( TextureRegion region, int x, int y, int width, int height )
    {
        SetRegion( region, x, y, width, height );
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Non-Virtual version of <see cref="SetRegion( float, float, float, float )"/>,
    /// enabling this to be called from constructors.
    /// </summary>
    private void SafeSetRegion( float u, float v, float u2, float v2 )
    {
        SetRegion( u, v, u2, v2 );
    }

    /// <summary>
    /// </summary>
    /// <param name="texture"></param>
    public void SetRegion( Texture texture )
    {
        Texture = texture;
        SetRegion( 0, 0, texture.Width, texture.Height );
    }

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <exception cref="GdxRuntimeException"></exception>
    public void SetRegion( int x, int y, int width, int height )
    {
        if ( Texture == null )
        {
            throw new GdxRuntimeException( "Texture cannot be null" );
        }

        var invTexWidth  = 1f / Texture.Width;
        var invTexHeight = 1f / Texture.Height;

        SetRegion( x * invTexWidth, y * invTexHeight, ( x + width ) * invTexWidth, ( y + height ) * invTexHeight );

        SetRegionWidth( Math.Abs( width ), IsFlipX() );
        SetRegionHeight( Math.Abs( height ), IsFlipY() );
    }

    /// <summary>
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="u2"></param>
    /// <param name="v2"></param>
    /// <exception cref="GdxRuntimeException"></exception>
    public virtual void SetRegion( float u, float v, float u2, float v2 )
    {
        if ( Texture == null )
        {
            throw new GdxRuntimeException( "Texture cannot be null" );
        }

        var texWidth  = Texture.Width;
        var texHeight = Texture.Height;

        SetRegionWidth( ( int )Math.Round( Math.Abs( u2 - u ) * texWidth ), IsFlipX() );
        SetRegionHeight( ( int )Math.Round( Math.Abs( v2 - v ) * texHeight ), IsFlipY() );

        // For a 1x1 region, adjust UVs toward pixel center to avoid filtering
        // artifacts on AMD GPUs when drawing very stretched.
        if ( ( RegionWidth == 1 ) && ( RegionHeight == 1 ) )
        {
            var xAdjustment = 0.25f / texWidth;

            u  += xAdjustment;
            u2 -= xAdjustment;

            var yAdjustment = 0.25f / texHeight;

            v  += yAdjustment;
            v2 -= yAdjustment;
        }

        U  = u;
        V  = v;
        U2 = u2;
        V2 = v2;
    }

    /// <summary>
    /// </summary>
    /// <param name="region"></param>
    public void SetRegion( TextureRegion region )
    {
        Texture = region.Texture;
        SetRegion( region.U, region.V, region.U2, region.V2 );
    }

    /// <summary>
    /// </summary>
    /// <param name="region"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetRegion( TextureRegion region, int x, int y, int width, int height )
    {
        Texture = region.Texture;
        SetRegion( region.RegionX + x, region.RegionY + y, width, height );
    }

    /// <summary>
    /// Flips this TextureRegion horizontally, vertically, or both.
    /// </summary>
    /// <param name="x"> TRUE to flip horizontally. </param>
    /// <param name="y"> TRUE to flip vertically. </param>
    public virtual void Flip( bool x, bool y )
    {
        if ( x )
        {
            ( U, U2 ) = ( U2, U );
        }

        if ( y )
        {
            ( V, V2 ) = ( V2, V );
        }
    }

    /// <summary>
    /// Returns true if this TextureRegion is flipped horizontally.
    /// </summary>
    public virtual bool IsFlipX()
    {
        return U > U2;
    }

    /// <summary>
    /// Returns true if this TextureRegion is flipped vertically.
    /// </summary>
    public virtual bool IsFlipY()
    {
        return V > V2;
    }

    /// <summary>
    /// Offsets the region relative to the current region. Generally the region's
    /// size should be the entire size of the texture in the direction(s) it is
    /// scrolled.
    /// </summary>
    /// <param name="xAmount">The percentage to offset horizontally.</param>
    /// <param name="yAmount">
    /// The percentage to offset vertically.
    /// This is done in texture space, so up is negative.
    /// </param>
    public virtual void Scroll( float xAmount, float yAmount )
    {
        Guard.ThrowIfNull( Texture );

        if ( xAmount != 0 )
        {
            var width = ( U2 - U ) * Texture.Width;

            U  = ( U + xAmount ) % 1;
            U2 = U + ( width / Texture.Width );
        }

        if ( yAmount != 0 )
        {
            var height = ( V2 - V ) * Texture.Height;

            V  = ( V + yAmount ) % 1;
            V2 = V + ( height / Texture.Height );
        }
    }

    /// <summary>
    /// Helper function to create tiles out of this TextureRegion starting from the
    /// top left corner going to the right and ending at the bottom right corner.
    /// Only complete tiles will be returned so if the region's width or height are
    /// not a multiple of the tile width and height not all of the region will be
    /// used. This will not work on texture regions returned form a TextureAtlas that
    /// either have whitespace removed or where flipped before the region is split.
    /// </summary>
    /// <param name="tileWidth">Required tile's width in pixels.</param>
    /// <param name="tileHeight">Required tile's height in pixels.</param>
    /// <returns>A 2D array of TextureRegions index by [row, column].</returns>
    public TextureRegion[ , ] Split( int tileWidth, int tileHeight )
    {
        var x      = RegionX;
        var y      = RegionY;
        var width  = RegionWidth;
        var height = RegionHeight;

        var rows = height / tileHeight;
        var cols = width / tileWidth;

        var startX = x;
        var tiles  = new TextureRegion[ rows, cols ];

        for ( var row = 0; row < rows; row++, y += tileHeight )
        {
            x = startX;

            for ( var col = 0; col < cols; col++, x += tileWidth )
            {
                tiles[ row, col ] = new TextureRegion( Texture, x, y, tileWidth, tileHeight );
            }
        }

        return tiles;
    }

    /// <summary>
    /// Helper function to create tiles out of the given Texture starting from the
    /// top left corner going to the right and ending at the bottom right corner.
    /// Only complete tiles will be returned so if the texture's width or height are
    /// not a multiple of the tile width and height not all of the texture will be used.
    /// </summary>
    /// <param name="texture">The texture to split.</param>
    /// <param name="tileWidth">Required tile's width in pixels.</param>
    /// <param name="tileHeight">Required tile's height in pixels.</param>
    /// <returns>A 2D array of TextureRegions index by [row, column].</returns>
    public static TextureRegion[ , ] Split( Texture texture, int tileWidth, int tileHeight )
    {
        var region = new TextureRegion( texture );

        return region.Split( tileWidth, tileHeight );
    }

    // ========================================================================
    // ========================================================================
    // Properties with detailed getters and setters.

    /// <summary>
    /// </summary>
    public int RegionX
    {
        get
        {
            Guard.ThrowIfNull( Texture );

            if ( ( Texture == null ) || ( Texture.Width == 0 ) )
            {
                // Handle cases where Texture or its Width is not valid yet
                Logger.Debug( "Texture, or its Width, is not valid yet" );

                return 0;
            }

            return ( int )Math.Round( U * Texture.Width );
        }
    }

    /// <summary>
    /// </summary>
    public int RegionY
    {
        get
        {
            Guard.ThrowIfNull( Texture );

            if ( ( Texture == null ) || ( Texture.Height == 0 ) )
            {
                // Handle cases where Texture or its Height is not valid yet
                Logger.Debug( "Texture, or its Height, is not valid yet" );

                return 0;
            }

            return ( int )Math.Round( V * Texture.Height );
        }
    }

    /// <summary>
    /// This TextureRegions Width property.
    /// </summary>
    public int RegionWidth
    {
        get
        {
            if ( ( Texture == null ) || ( Texture.Width == 0 ) )
            {
                // Handle cases where Texture or its Width is not valid yet
                Logger.Debug( "Texture, or its Width, is not valid yet" );

                return 0;
            }

            return ( int )Math.Round( Math.Abs( U2 - U ) * Texture.Width );
        }

        // No setter for RegionHeight as it is derived.
    }

    /// <summary>
    /// This TextureRegions Height property.
    /// </summary>
    public int RegionHeight
    {
        get
        {
            if ( ( Texture == null ) || ( Texture.Height == 0 ) )
            {
                // Handle cases where Texture or its Height is not valid yet
                return 0;
            }

            return ( int )Math.Round( Math.Abs( V2 - V ) * Texture.Height );
        }

        // No setter for RegionHeight as it is derived.
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="desiredRegionWidth"></param>
    /// <param name="isFlipX"></param>
    public void SetRegionWidth( int desiredRegionWidth, bool isFlipX )
    {
        if ( ( Texture == null ) || ( Texture.Width == 0 ) )
        {
            // Handle cases where Texture or its Width is not valid yet
            return;
        }

        // Calculate the difference in U coordinates needed
        var uDiff = ( float )desiredRegionWidth / Texture.Width;

        if ( isFlipX )
        {
            // Adjust U based on U2 and desired height
            U = U2 + uDiff; // Use the property setter for U
        }
        else
        {
            // Adjust U2 based on U and desired height
            U2 = U + uDiff; // Use the property setter for U2
        }

        // The property setters for U/U2 will then implicitly update RegionWidth
        // when its getter is called.
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="desiredRegionHeight"></param>
    /// <param name="isFlipY"></param>
    public void SetRegionHeight( int desiredRegionHeight, bool isFlipY )
    {
        if ( ( Texture == null ) || ( Texture.Height == 0 ) )
        {
            // Handle cases where Texture or its Height is not valid yet
            return;
        }

        // Calculate the difference in V coordinates needed
        var vDiff = ( float )desiredRegionHeight / Texture.Height;

        if ( isFlipY )
        {
            // Adjust V based on V2 and desired height
            V = V2 + vDiff; // Use the property setter for V
        }
        else
        {
            // Adjust V2 based on V and desired height
            V2 = V + vDiff; // Use the property setter for V2
        }

        // The property setters for V/V2 will then implicitly update RegionHeight
        // when its getter is called.
    }
}

// ========================================================================
// ========================================================================