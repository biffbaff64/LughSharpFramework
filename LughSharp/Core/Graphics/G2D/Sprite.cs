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

using JetBrains.Annotations;

using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

using Rectangle = LughSharp.Core.Maths.Rectangle;

namespace LughSharp.Core.Graphics.G2D;

[PublicAPI]
public class Sprite : TextureRegion
{
    public const int VERTEX_SIZE = 2 + 1 + 2;
    public const int SPRITE_SIZE = 4 * VERTEX_SIZE;

    // ========================================================================

    public float[] Vertices { get; set; } = new float[ SPRITE_SIZE ];

    /// <returns> the width of the sprite, not accounting for scale. </returns>
    public float Width { get; set; }

    /// <returns> the height of the sprite, not accounting for scale. </returns>
    public float Height { get; set; }

    /// <summary>
    /// The origin influences <see cref="SetPosition(float, float)"/>,
    /// <see cref="Rotation"/> and the expansion direction of scaling
    /// <see cref="SetScale(float, float)"/>
    /// </summary>
    public float OriginX { get; set; }

    /// <summary>
    /// The origin influences <see cref="SetPosition(float, float)"/>,
    /// <see cref="Rotation"/> and the expansion direction of scaling
    /// <see cref="SetScale(float, float)"/>
    /// </summary>
    public float OriginY { get; private set; }

    /// <summary>
    /// X scale of the sprite, independent of size set
    /// by <see cref="SetSize(float, float)"/>
    /// </summary>
    public float ScaleX { get; private set; } = 1;

    /// <summary>
    /// Y scale of the sprite, independent of size set
    /// by <see cref="SetSize(float, float)"/>
    /// </summary>
    public float ScaleY { get; private set; } = 1;

    // ========================================================================

    private readonly Color _color = new( 1, 1, 1, 1 );

    private bool       _isDirty = true;
    private bool       _flipX;
    private bool       _flipY;
    private float      _uScrollOffset;
    private float      _vScrollOffset;
    private Rectangle? _bounds;
    private float      _x;
    private float      _y;

    // ========================================================================

    /// <summary>
    /// Creates an uninitialized sprite.
    /// <para>
    /// The sprite will need a texture region and bounds set before it can be drawn.
    /// </para>
    /// </summary>
    public Sprite()
    {
        SetColor( 1, 1, 1, 1 );
    }

    /// <summary>
    /// Creates a sprite with width, height, and texture region
    /// equal to the size of the texture.
    /// </summary>
    public Sprite( Texture texture )
        : this( texture, 0, 0, texture.Width, texture.Height )
    {
    }

    /// <summary>
    /// Creates a sprite with width, height, and texture region equal to the
    /// specified size. The texture region's upper left corner will be 0,0.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="srcWidth">
    /// The width of the texture region.
    /// May be negative to flip the sprite when drawn.
    /// </param>
    /// <param name="srcHeight">
    /// The height of the texture region.
    /// May be negative to flip the sprite when drawn.
    /// </param>
    public Sprite( Texture texture, int srcWidth, int srcHeight )
        : this( texture, 0, 0, srcWidth, srcHeight )
    {
    }

    /// <summary>
    /// Creates a sprite with width, height, and texture region equal
    /// to the specified size.
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="srcX"></param>
    /// <param name="srcY"></param>
    /// <param name="srcWidth">
    /// The width of the texture region.
    /// May be negative to flip the sprite when drawn.
    /// </param>
    /// <param name="srcHeight">
    /// The height of the texture region.
    /// May be negative to flip the sprite when drawn.
    /// </param>
    public Sprite( Texture? texture, int srcX, int srcY, int srcWidth, int srcHeight )
    {
        Texture = texture ?? throw new ArgumentException( "texture cannot be null." );

        SetRegion( srcX, srcY, srcWidth, srcHeight );
        SetColor( 1, 1, 1, 1 );

        SetSizeAndOrigin( Math.Abs( srcWidth ), Math.Abs( srcHeight ) );
    }

    /// <summary>
    /// Creates a sprite based on a specific TextureRegion.
    /// The new sprite's region is a copy of the parameter region - altering one
    /// does not affect the other.
    /// </summary>
    public Sprite( TextureRegion region )
    {
        SetRegion( region );
        SetColor( 1, 1, 1, 1 );

        SetSizeAndOrigin( region.GetRegionWidth(), region.GetRegionHeight() );
    }

    /// <summary>
    /// Creates a sprite with width, height, and texture region equal to the
    /// specified size, relative to specified sprite's texture region.
    /// </summary>
    /// <param name="region"></param>
    /// <param name="srcX"></param>
    /// <param name="srcY"></param>
    /// <param name="srcWidth">
    /// The width of the texture region.
    /// May be negative to flip the sprite when drawn.
    /// </param>
    /// <param name="srcHeight">
    /// The height of the texture region.
    /// May be negative to flip the sprite when drawn.
    /// </param>
    public Sprite( TextureRegion region, int srcX, int srcY, int srcWidth, int srcHeight )
    {
        SetRegion( region, srcX, srcY, srcWidth, srcHeight );
        SetColor( 1, 1, 1, 1 );

        SetSizeAndOrigin( Math.Abs( srcWidth ), Math.Abs( srcHeight ) );
    }

    /// <summary>
    /// Creates a sprite that is a copy in every way of the specified sprite.
    /// </summary>
    public Sprite( Sprite sprite )
    {
        Set( sprite );
    }

    /// <summary>
    /// Helper method for constructors which allows calls to virtual
    /// methods which cannot be called from constructors.
    /// </summary>
    /// <param name="srcWidth"></param>
    /// <param name="srcHeight"></param>
    private void SetSizeAndOrigin( int srcWidth, int srcHeight )
    {
        SetSize( srcWidth, srcHeight );
        SetOrigin( Width / 2, Height / 2 );
    }

    /// <summary>
    /// Make this sprite a copy in every way of the specified sprite
    /// </summary>
    /// <summary>
    /// Sets the properties of this Sprite to match those of the provided Sprite.
    /// </summary>
    /// <param name="sprite">The Sprite whose properties will be copied.</param>
    /// <exception cref="ArgumentNullException">Thrown if the provided sprite is null.</exception>
    public void Set( Sprite sprite )
    {
        Guard.Against.Null( sprite );

        try
        {
            // Copy vertices array
            Array.Copy( sprite.Vertices, 0, Vertices, 0, SPRITE_SIZE );
        }
        catch ( ArgumentException ex )
        {
            throw new InvalidOperationException( "Failed to copy vertices array.", ex );
        }

        // Assign properties
        Texture  = sprite.Texture;
        U        = sprite.U;
        V        = sprite.V;
        U2       = sprite.U2;
        V2       = sprite.V2;
        _x       = sprite.GetX();
        _y       = sprite.GetY();
        Width    = sprite.Width;
        Height   = sprite.Height;
        OriginX  = sprite.OriginX;
        OriginY  = sprite.OriginY;
        Rotation = sprite.Rotation;
        ScaleX   = sprite.ScaleX;
        ScaleY   = sprite.ScaleY;
        _isDirty = sprite._isDirty;

        // Copy color
        _color.Set( sprite._color );

        SetRegionWidth( sprite.GetRegionWidth() );
        SetRegionHeight( sprite.GetRegionHeight() );
    }

    /// <summary>
    /// Sets the position and size of the sprite when drawn, before scaling
    /// and rotation are applied, using the current values for <c>X</c>,
    /// <c>Y</c>, <c>Width</c>, and <c>Height</c>. If origin, rotation, or
    /// scale are changed, it is slightly more efficient to set the bounds
    /// after those operations.
    /// </summary>
    public virtual void SetBounds()
    {
        SetBounds( _x, _y, Width, Height );
    }

    /// <summary>
    /// Sets the position and size of the sprite when drawn, before scaling
    /// and rotation are applied. If origin, rotation, or scale are changed,
    /// it is slightly more efficient to set the bounds after those operations.
    /// </summary>
    public virtual void SetBounds( float x, float y, float width, float height )
    {
        _x     = x;
        _y     = y;
        Width  = width;
        Height = height;

        if ( _isDirty )
        {
            return;
        }

        if ( ( Rotation != 0 ) || ScaleX is not 1 || ScaleY is not 1 )
        {
            _isDirty = true;

            return;
        }

        var x2 = x + width;
        var y2 = y + height;

        Vertices[ IBatch.X1 ] = x; // Bottom left
        Vertices[ IBatch.Y1 ] = y;

        Vertices[ IBatch.X2 ] = x; // Top left
        Vertices[ IBatch.Y2 ] = y2;

        Vertices[ IBatch.X3 ] = x2; // Top right
        Vertices[ IBatch.Y3 ] = y2;

        Vertices[ IBatch.X4 ] = x2; // Bottom right
        Vertices[ IBatch.Y4 ] = y;
    }

    /// <summary>
    /// Sets the size of the sprite when drawn, before scaling and rotation are
    /// applied. If origin, rotation, or scale are changed, it is slightly more
    /// efficient to set the size after those operations.
    /// <para>
    /// If both position and size are to be changed, it is better to use
    /// <see cref="SetBounds(float, float, float, float)"/>.
    /// </para>
    /// </summary>
    public virtual void SetSize( float width, float height )
    {
        Width  = width;
        Height = height;

        if ( _isDirty )
        {
            return;
        }

        if ( ( Rotation != 0 ) || ScaleX is not 1 || ScaleY is not 1 )
        {
            _isDirty = true;

            return;
        }

        var x2 = _x + width;
        var y2 = _y + height;

        Vertices[ IBatch.X1 ] = _x;     // Bottom left
        Vertices[ IBatch.Y1 ] = _y;

        Vertices[ IBatch.X2 ] = _x;     // Top left
        Vertices[ IBatch.Y2 ] = y2;

        Vertices[ IBatch.X3 ] = x2;     // Top right
        Vertices[ IBatch.Y3 ] = y2;

        Vertices[ IBatch.X4 ] = x2;     // Bottom right
        Vertices[ IBatch.Y4 ] = _y;
    }

    /// <summary>
    /// Sets the position where the sprite will be drawn. If origin, rotation, or scale are changed, it is slightly more
    /// efficient
    /// to set the position after those operations. If both position and size are to be changed, it is better to use
    /// <see cref="SetBounds(float, float, float, float)"/>.
    /// </summary>
    public virtual void SetPosition( float x, float y )
    {
        _x = x;
        _y = y;

        if ( _isDirty )
        {
            return;
        }

        if ( ( Rotation != 0 ) || ScaleX is not 1 || ScaleY is not 1 )
        {
            _isDirty = true;

            return;
        }

        var x2 = x + Width;
        var y2 = y + Height;

        Vertices[ IBatch.X1 ] = x;      // Bottom left
        Vertices[ IBatch.Y1 ] = y;

        Vertices[ IBatch.X2 ] = x;      // Top left
        Vertices[ IBatch.Y2 ] = y2;

        Vertices[ IBatch.X3 ] = x2;     // Top right
        Vertices[ IBatch.Y3 ] = y2;

        Vertices[ IBatch.X4 ] = x2;     // Bottom right
        Vertices[ IBatch.Y4 ] = y;
    }

    /// <summary>
    /// Sets the position where the sprite will be drawn, relative to its current origin.
    /// </summary>
    public void SetOriginBasedPosition( float x, float y )
    {
        SetPosition( x - OriginX, y - OriginY );
    }

    /// <summary>
    /// Sets the position so that the sprite is centered on (x, y)
    /// </summary>
    public void SetCenter( float x, float y )
    {
        SetPosition( x - ( Width / 2 ), y - ( Height / 2 ) );
    }

    /// <summary>
    /// Sets the x position so that it is centered on the given x parameter
    /// </summary>
    public void SetCenterX( float value )
    {
        _x = value - ( Width / 2 );
    }

    /// <summary>
    /// Sets the y position so that it is centered on the given y parameter
    /// </summary>
    public void SetCenterY( float value )
    {
        _y = value - ( Height / 2 );
    }

    /// <summary>
    /// Sets the x position relative to the current position where the sprite will
    /// be drawn. If origin, rotation, or scale are
    /// changed, it is slightly more efficient to translate after those operations.
    /// </summary>
    public void TranslateX( float xAmount )
    {
        _x += xAmount;

        if ( _isDirty )
        {
            return;
        }

        if ( ( Rotation != 0 ) || ScaleX is not 1 || ScaleY is not 1 )
        {
            _isDirty = true;

            return;
        }

        Vertices[ IBatch.X1 ] += xAmount;
        Vertices[ IBatch.X2 ] += xAmount;
        Vertices[ IBatch.X3 ] += xAmount;
        Vertices[ IBatch.X4 ] += xAmount;
    }

    /// <summary>
    /// Sets the y position relative to the current position where the sprite will
    /// be drawn. If origin, rotation, or scale are changed, it is slightly more
    /// efficient to translate after those operations.
    /// </summary>
    public void TranslateY( float yAmount )
    {
        _y += yAmount;

        if ( _isDirty )
        {
            return;
        }

        if ( ( Rotation != 0 ) || ScaleX is not 1 || ScaleY is not 1 )
        {
            _isDirty = true;

            return;
        }

        Vertices[ IBatch.Y1 ] += yAmount;
        Vertices[ IBatch.Y2 ] += yAmount;
        Vertices[ IBatch.Y3 ] += yAmount;
        Vertices[ IBatch.Y4 ] += yAmount;
    }

    /// <summary>
    /// Sets the position relative to the current position where the sprite will
    /// be drawn. If origin, rotation, or scale are changed, it is slightly more
    /// efficient to translate after those operations.
    /// </summary>
    public void Translate( float xAmount, float yAmount )
    {
        _x += xAmount;
        _y += yAmount;

        if ( _isDirty )
        {
            return;
        }

        if ( ( Rotation != 0 ) || ScaleX is not 1 || ScaleY is not 1 )
        {
            _isDirty = true;

            return;
        }

        Vertices[ IBatch.X1 ] += xAmount;   // Bottom left
        Vertices[ IBatch.Y1 ] += yAmount;

        Vertices[ IBatch.X2 ] += xAmount;   // Top left
        Vertices[ IBatch.Y2 ] += yAmount;

        Vertices[ IBatch.X3 ] += xAmount;   // Top right
        Vertices[ IBatch.Y3 ] += yAmount;

        Vertices[ IBatch.X4 ] += xAmount;   // Bottom right
        Vertices[ IBatch.Y4 ] += yAmount;
    }

    /// <summary>
    /// Sets the color used to tint this sprite.
    /// Default is <see cref="Color.White"/>.
    /// </summary>
    public void SetColor( Color tint )
    {
        Color.Set( tint );

        var color = tint.ToFloatBitsAbgr();

        Vertices[ IBatch.C1 ] = color;
        Vertices[ IBatch.C2 ] = color;
        Vertices[ IBatch.C3 ] = color;
        Vertices[ IBatch.C4 ] = color;
    }

    /// <summary>
    /// Sets the color used to tint this sprite.
    /// </summary>
    public void SetColor( float r, float g, float b, float a )
    {
        Color.Set( r, g, b, a );

        var color = Color.ToFloatBitsAbgr();

        Vertices[ IBatch.C1 ] = color;
        Vertices[ IBatch.C2 ] = color;
        Vertices[ IBatch.C3 ] = color;
        Vertices[ IBatch.C4 ] = color;
    }

    /// <summary>
    /// Sets the origin in relation to the sprite's position for scaling and rotation.
    /// </summary>
    public virtual void SetOrigin( float originX, float originY )
    {
        OriginX  = originX;
        OriginY  = originY;
        _isDirty = true;
    }

    /// <summary>
    /// Place origin in the center of the sprite
    /// </summary>
    public virtual void SetOriginCenter()
    {
        OriginX  = Width / 2;
        OriginY  = Height / 2;
        _isDirty = true;
    }

    /// <summary>
    /// Sets the sprite's rotation in degrees relative to the current rotation.
    /// Rotation is centered on the origin set in <see cref="SetOrigin(float, float)"/>
    /// </summary>
    public void Rotate( float degrees )
    {
        if ( degrees == 0 )
        {
            return;
        }

        Rotation += degrees;
        _isDirty =  true;
    }

    /// <summary>
    /// Rotates this sprite 90 degrees in-place by rotating the texture coordinates.
    /// This rotation is unaffected by <see cref="Rotation"/> and
    /// <see cref="Rotate(float)"/>.
    /// </summary>
    public virtual void Rotate90( bool clockwise )
    {
        if ( clockwise )
        {
            var temp = Vertices[ IBatch.V1 ];

            Vertices[ IBatch.V1 ] = Vertices[ IBatch.V4 ];
            Vertices[ IBatch.V4 ] = Vertices[ IBatch.V3 ];
            Vertices[ IBatch.V3 ] = Vertices[ IBatch.V2 ];
            Vertices[ IBatch.V2 ] = temp;

            temp = Vertices[ IBatch.U1 ];

            Vertices[ IBatch.U1 ] = Vertices[ IBatch.U4 ];
            Vertices[ IBatch.U4 ] = Vertices[ IBatch.U3 ];
            Vertices[ IBatch.U3 ] = Vertices[ IBatch.U2 ];
            Vertices[ IBatch.U2 ] = temp;
        }
        else
        {
            var temp = Vertices[ IBatch.V1 ];

            Vertices[ IBatch.V1 ] = Vertices[ IBatch.V2 ];
            Vertices[ IBatch.V2 ] = Vertices[ IBatch.V3 ];
            Vertices[ IBatch.V3 ] = Vertices[ IBatch.V4 ];
            Vertices[ IBatch.V4 ] = temp;

            temp = Vertices[ IBatch.U1 ];

            Vertices[ IBatch.U1 ] = Vertices[ IBatch.U2 ];
            Vertices[ IBatch.U2 ] = Vertices[ IBatch.U3 ];
            Vertices[ IBatch.U3 ] = Vertices[ IBatch.U4 ];
            Vertices[ IBatch.U4 ] = temp;
        }
    }

    /// <summary>
    /// Sets the sprite's scale for both X and Y uniformly. The sprite
    /// scales out from the origin. This will not affect the values
    /// returned by <see cref="Width"/> and <see cref="Height"/>
    /// </summary>
    public void SetScale( float scaleXY )
    {
        ScaleX   = scaleXY;
        ScaleY   = scaleXY;
        _isDirty = true;
    }

    /// <summary>
    /// Sets the sprite's scale for both X and Y. The sprite scales out from
    /// the origin. This will not affect the values returned by
    /// <see cref="Width"/> and <see cref="Height"/>
    /// </summary>
    public void SetScale( float scaleX, float scaleY )
    {
        ScaleX   = scaleX;
        ScaleY   = scaleY;
        _isDirty = true;
    }

    /// <summary>
    /// Sets the sprite's scale relative to the current scale. for example:
    /// original scale 2 -> sprite.scale(4) -> final scale 6.
    /// <para>
    /// The sprite scales out from the origin. This will not affect the values
    /// returned by <see cref="Width"/> and <see cref="Height"/>
    /// </para>
    /// </summary>
    public void AddScale( float amount )
    {
        ScaleX   += amount;
        ScaleY   += amount;
        _isDirty =  true;
    }

    /// <summary>
    /// Returns the packed vertices, colors, and texture coordinates
    /// for this sprite.
    /// </summary>
    public float[] GetVertices()
    {
        if ( !_isDirty )
        {
            return Vertices;
        }

        _isDirty = false;

        var originX = Width / 2f;
        var originY = Height / 2f;

        // Local corners relative to center
        var localX1      = -originX;
        var localY1      = -originY;
        var localX2      = localX1 + Width;
        var localY2      = localY1 + Height;
        var worldOriginX = _x - localX1;
        var worldOriginY = _y - localY1;

        if ( ScaleX is not 1 || ScaleY is not 1 )
        {
            localX1 *= ScaleX;
            localY1 *= ScaleY;
            localX2 *= ScaleX;
            localY2 *= ScaleY;
        }

        if ( Rotation != 0 )
        {
            var cos = MathUtils.CosDeg( Rotation );
            var sin = MathUtils.SinDeg( Rotation );

            var localXCos  = localX1 * cos;
            var localXSin  = localX1 * sin;
            var localYCos  = localY1 * cos;
            var localYSin  = localY1 * sin;
            var localX2Cos = localX2 * cos;
            var localX2Sin = localX2 * sin;
            var localY2Cos = localY2 * cos;
            var localY2Sin = localY2 * sin;

            var x1 = localXCos - localYSin + worldOriginX;
            var y1 = localYCos + localXSin + worldOriginY;
            Vertices[ IBatch.X1 ] = x1;
            Vertices[ IBatch.Y1 ] = y1;

            var x2 = localXCos - localY2Sin + worldOriginX;
            var y2 = localY2Cos + localXSin + worldOriginY;
            Vertices[ IBatch.X2 ] = x2;
            Vertices[ IBatch.Y2 ] = y2;

            var x3 = localX2Cos - localY2Sin + worldOriginX;
            var y3 = localY2Cos + localX2Sin + worldOriginY;
            Vertices[ IBatch.X3 ] = x3;
            Vertices[ IBatch.Y3 ] = y3;

            Vertices[ IBatch.X4 ] = x1 + ( x3 - x2 );
            Vertices[ IBatch.Y4 ] = y3 - ( y2 - y1 );
        }
        else
        {
            var x1 = localX1 + worldOriginX;
            var y1 = localY1 + worldOriginY;
            var x2 = localX2 + worldOriginX;
            var y2 = localY2 + worldOriginY;

            Vertices[ IBatch.X1 ] = x1;
            Vertices[ IBatch.Y1 ] = y1;
            Vertices[ IBatch.X2 ] = x1;
            Vertices[ IBatch.Y2 ] = y2;
            Vertices[ IBatch.X3 ] = x2;
            Vertices[ IBatch.Y3 ] = y2;
            Vertices[ IBatch.X4 ] = x2;
            Vertices[ IBatch.Y4 ] = y1;
        }

        return Vertices;
    }

    /// <summary>
    /// Returns the bounding axis aligned <see cref="Rectangle"/> that bounds this
    /// sprite. The rectangles x and y coordinates describe its bottom left corner.
    /// If you change the position or size of the sprite, you have to fetch the
    /// triangle again for it to be recomputed.
    /// </summary>
    /// <returns> the bounding Rectangle </returns>
    public Rectangle GetBoundingRectangle()
    {
        var vertices = GetVertices();

        var minx = vertices[ IBatch.X1 ];
        var miny = vertices[ IBatch.Y1 ];
        var maxx = vertices[ IBatch.X1 ];
        var maxy = vertices[ IBatch.Y1 ];

        minx = minx > vertices[ IBatch.X2 ] ? vertices[ IBatch.X2 ] : minx;
        minx = minx > vertices[ IBatch.X3 ] ? vertices[ IBatch.X3 ] : minx;
        minx = minx > vertices[ IBatch.X4 ] ? vertices[ IBatch.X4 ] : minx;

        maxx = maxx < vertices[ IBatch.X2 ] ? vertices[ IBatch.X2 ] : maxx;
        maxx = maxx < vertices[ IBatch.X3 ] ? vertices[ IBatch.X3 ] : maxx;
        maxx = maxx < vertices[ IBatch.X4 ] ? vertices[ IBatch.X4 ] : maxx;

        miny = miny > vertices[ IBatch.Y2 ] ? vertices[ IBatch.Y2 ] : miny;
        miny = miny > vertices[ IBatch.Y3 ] ? vertices[ IBatch.Y3 ] : miny;
        miny = miny > vertices[ IBatch.Y4 ] ? vertices[ IBatch.Y4 ] : miny;

        maxy = maxy < vertices[ IBatch.Y2 ] ? vertices[ IBatch.Y2 ] : maxy;
        maxy = maxy < vertices[ IBatch.Y3 ] ? vertices[ IBatch.Y3 ] : maxy;
        maxy = maxy < vertices[ IBatch.Y4 ] ? vertices[ IBatch.Y4 ] : maxy;

        _bounds ??= new Rectangle();

        _bounds.X      = minx;
        _bounds.Y      = miny;
        _bounds.Width  = maxx - minx;
        _bounds.Height = maxy - miny;

        return _bounds;
    }

    public void Draw( IBatch batch )
    {
        if ( Texture != null )
        {
            batch.Draw( Texture, GetVertices(), 0, SPRITE_SIZE );
        }
    }

    public void Draw( IBatch batch, float alphaModulation )
    {
        var oldAlpha = Alpha;

        Alpha = oldAlpha * alphaModulation;

        Draw( batch );

        Alpha = oldAlpha;
    }

    /// <summary>
    /// </summary>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <param name="u2"></param>
    /// <param name="v2"></param>
    public override void SetRegion( float u, float v, float u2, float v2 )
    {
        base.SetRegion( u, v, u2, v2 );

        Vertices[ IBatch.U1 ] = u;
        Vertices[ IBatch.V1 ] = v2;

        Vertices[ IBatch.U2 ] = u;
        Vertices[ IBatch.V2 ] = v;

        Vertices[ IBatch.U3 ] = u2;
        Vertices[ IBatch.V3 ] = v;

        Vertices[ IBatch.U4 ] = u2;
        Vertices[ IBatch.V4 ] = v2;
    }

    /// <summary>
    /// Set the sprite's flip state regardless of current condition.
    /// </summary>
    /// <param name="flipx"> the desired horizontal flip state </param>
    /// <param name="flipy"> the desired vertical flip state  </param>
    public void SetFlip( bool flipx, bool flipy )
    {
        var performX = false;
        var performY = false;

        if ( IsFlipX() != flipx )
        {
            performX = true;
        }

        if ( IsFlipY() != flipy )
        {
            performY = true;
        }

        Flip( performX, performY );
    }

    /// <summary>
    /// </summary>
    /// <param name="flipx"> perform horizontal flip </param>
    /// <param name="flipy"> perform vertical flip  </param>
    public override void Flip( bool flipx, bool flipy )
    {
        if ( flipx )
        {
            _flipX = !_flipX;
        }

        if ( flipy )
        {
            _flipY = !_flipY;
        }

        _isDirty = true; // Ensure GetVertices runs to apply the new UVs
    }

    /// <summary>
    /// </summary>
    /// <param name="xAmount"></param>
    /// <param name="yAmount"></param>
    public override void Scroll( float xAmount, float yAmount )
    {
        if ( xAmount == 0 && yAmount == 0 )
        {
            return;
        }

        _uScrollOffset = ( _uScrollOffset + xAmount ) % 1.0f;
        _vScrollOffset = ( _vScrollOffset + yAmount ) % 1.0f;

        // Handle negative scrolling (ensure offset is always positive)
        if ( _uScrollOffset < 0 )
        {
            _uScrollOffset += 1.0f;
        }

        if ( _vScrollOffset < 0 )
        {
            _vScrollOffset += 1.0f;
        }

        _isDirty = true;
    }

    // ========================================================================

    public void DebugVertices()
    {
        Logger.Debug( "Sprite Vertices:" );

        for ( var i = 0; i < 20; i++ )
        {
            if ( i is 2 or 7 or 12 or 17 )
            {
                Logger.Debug( $"Vertices[{i}]: {( uint )Vertices[ i ]:X}" );
            }
            else
            {
                Logger.Debug( $"Vertices[{i}]: {Vertices[ i ]}" );
            }
        }
    }

    // ========================================================================

    public override void SetU( float u )
    {
        base.SetU( u );

        Vertices[ IBatch.U1 ] = u;
        Vertices[ IBatch.U2 ] = u;
    }

    public override void SetV( float v )
    {
        base.SetV( v );

        Vertices[ IBatch.V2 ] = v;
        Vertices[ IBatch.V3 ] = v;
    }

    public override void SetU2( float u2 )
    {
        base.SetU2( u2 );

        Vertices[ IBatch.U3 ] = u2;
        Vertices[ IBatch.U4 ] = u2;
    }

    public override void SetV2( float v2 )
    {
        base.SetV2( v2 );

        Vertices[ IBatch.V1 ] = v2;
        Vertices[ IBatch.V4 ] = v2;
    }

    /// <summary>
    /// Sets the color of this sprite, expanding the alpha from 0-254 to 0-255.
    /// </summary>
    public float PackedColor
    {
        set
        {
            var color = Color;

            Color.Abgr8888ToColor( ref color, value );

            Vertices[ IBatch.C1 ] = value;
            Vertices[ IBatch.C2 ] = value;
            Vertices[ IBatch.C3 ] = value;
            Vertices[ IBatch.C4 ] = value;
        }
    }

    /// <summary>
    /// Returns the bounding axis aligned <see cref="Rectangle"/> that
    /// bounds this sprite. The rectangles x and y coordinates describe its
    /// bottom left corner. If you change the position or size of the sprite,
    /// you must fetch the triangle again for it to be recomputed.
    /// </summary>
    public Rectangle BoundingRectangle
    {
        get
        {
            var minx = Vertices[ IBatch.X1 ];
            var miny = Vertices[ IBatch.Y1 ];
            var maxx = Vertices[ IBatch.X1 ];
            var maxy = Vertices[ IBatch.Y1 ];

            minx = minx > Vertices[ IBatch.X2 ] ? Vertices[ IBatch.X2 ] : minx;
            minx = minx > Vertices[ IBatch.X3 ] ? Vertices[ IBatch.X3 ] : minx;
            minx = minx > Vertices[ IBatch.X4 ] ? Vertices[ IBatch.X4 ] : minx;

            maxx = maxx < Vertices[ IBatch.X2 ] ? Vertices[ IBatch.X2 ] : maxx;
            maxx = maxx < Vertices[ IBatch.X3 ] ? Vertices[ IBatch.X3 ] : maxx;
            maxx = maxx < Vertices[ IBatch.X4 ] ? Vertices[ IBatch.X4 ] : maxx;

            miny = miny > Vertices[ IBatch.Y2 ] ? Vertices[ IBatch.Y2 ] : miny;
            miny = miny > Vertices[ IBatch.Y3 ] ? Vertices[ IBatch.Y3 ] : miny;
            miny = miny > Vertices[ IBatch.Y4 ] ? Vertices[ IBatch.Y4 ] : miny;

            maxy = maxy < Vertices[ IBatch.Y2 ] ? Vertices[ IBatch.Y2 ] : maxy;
            maxy = maxy < Vertices[ IBatch.Y3 ] ? Vertices[ IBatch.Y3 ] : maxy;
            maxy = maxy < Vertices[ IBatch.Y4 ] ? Vertices[ IBatch.Y4 ] : maxy;

            field ??= new Rectangle();

            field.X      = minx;
            field.Y      = miny;
            field.Width  = maxx - minx;
            field.Height = maxy - miny;

            return field;
        }
    }

    /// <summary>
    /// Sets the alpha portion of the color used to tint this sprite.
    /// </summary>
    public float Alpha
    {
        get => _color.A;
        set
        {
            _color.A = value;

            var color = Color.ToFloatBitsAbgr();

            Vertices[ IBatch.C1 ] = color;
            Vertices[ IBatch.C2 ] = color;
            Vertices[ IBatch.C3 ] = color;
            Vertices[ IBatch.C4 ] = color;
        }
    }

    /// <summary>
    /// Sets the rotation of the sprite in degrees. Rotation is centered on the
    /// origin set in <see cref="SetOrigin(float, float)"/>
    /// </summary>
    public float Rotation
    {
        get;
        set
        {
            field    = value;
            _isDirty = true;
        }
    }

    /// <summary>
    /// Returns the color of this sprite. If the returned instance is
    /// manipulated, <see cref="SetColor(float,float,float,float)"/> must be
    /// called afterward.
    /// </summary>
    public Color Color
    {
        get
        {
            var intBits = NumberUtils.FloatToIntColor( Vertices[ IBatch.C1 ] );

            var color = _color;

            color.R = ( intBits & 0xff ) / 255f;
            color.G = ( ( intBits >>> 8 ) & 0xff ) / 255f;
            color.B = ( ( intBits >>> 16 ) & 0xff ) / 255f;
            color.A = ( ( intBits >>> 24 ) & 0xff ) / 255f;

            return color;
        }
    }

    public virtual float GetX() => _x;

    public virtual float GetY() => _y;

    /// <summary>
    /// Sets the x position where the sprite will be drawn. If origin, rotation,
    /// or scale are changed, it is slightly more efficient to set the position
    /// after those operations. If both position and size are to be changed, it
    /// is better to use <see cref="SetBounds(float, float, float, float)"/>.
    /// </summary>
    public virtual void SetX( float x )
    {
        _x = x;

        if ( _isDirty )
        {
            return;
        }

        if ( ( Rotation != 0 ) || ScaleX is not 1 || ScaleY is not 1 )
        {
            _isDirty = true;

            return;
        }

        Vertices[ IBatch.X1 ] = x;
        Vertices[ IBatch.X2 ] = x;
        Vertices[ IBatch.X3 ] = x + Width;
        Vertices[ IBatch.X4 ] = x + Width;
    }

    /// <summary>
    /// Sets the y position where the sprite will be drawn. If origin, rotation,
    /// or scale are changed, it is slightly more efficient to set the position
    /// after those operations. If both position and size are to be changed, it
    /// is better to use <see cref="SetBounds(float, float, float, float)"/>.
    /// </summary>
    public virtual void SetY( float y )
    {
        _y = y;

        if ( _isDirty )
        {
            return;
        }

        if ( ( Rotation != 0 ) || ScaleX is not 1 || ScaleY is not 1 )
        {
            _isDirty = true;

            return;
        }

        Vertices[ IBatch.Y1 ] = y;
        Vertices[ IBatch.Y2 ] = y + Height;
        Vertices[ IBatch.Y3 ] = y + Height;
        Vertices[ IBatch.Y4 ] = y;
    }
}

// ============================================================================
// ============================================================================