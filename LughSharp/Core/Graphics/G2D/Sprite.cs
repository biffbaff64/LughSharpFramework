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

    private bool _isDirty = true;

    private bool  _flipX;
    private bool  _flipY;
    private float _uScrollOffset = 0;
    private float _vScrollOffset = 0;

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

        SetSizeAndOrigin( region.RegionWidth, region.RegionHeight );
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
        X        = sprite.X;
        Y        = sprite.Y;
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

        SetRegionWidth( sprite.RegionWidth, IsFlipX() );
        SetRegionHeight( sprite.RegionHeight, IsFlipY() );
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
        SetBounds( X, Y, Width, Height );
    }
    
    /// <summary>
    /// Sets the position and size of the sprite when drawn, before scaling
    /// and rotation are applied. If origin, rotation, or scale are changed,
    /// it is slightly more efficient to set the bounds after those operations.
    /// </summary>
    public virtual void SetBounds( float x, float y, float width, float height )
    {
        X      = x;
        Y      = y;
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

        var x2 = X + width;
        var y2 = Y + height;

        Vertices[ IBatch.X1 ] = X;
        Vertices[ IBatch.Y1 ] = Y;

        Vertices[ IBatch.X2 ] = X;
        Vertices[ IBatch.Y2 ] = y2;

        Vertices[ IBatch.X3 ] = x2;
        Vertices[ IBatch.Y3 ] = y2;

        Vertices[ IBatch.X4 ] = x2;
        Vertices[ IBatch.Y4 ] = Y;
    }

    /// <summary>
    /// Sets the position where the sprite will be drawn. If origin, rotation, or scale are changed, it is slightly more
    /// efficient
    /// to set the position after those operations. If both position and size are to be changed, it is better to use
    /// <see cref="SetBounds(float, float, float, float)"/>.
    /// </summary>
    public void SetPosition( float x, float y )
    {
        X = x;
        Y = y;

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

        Vertices[ IBatch.X1 ] = x;
        Vertices[ IBatch.Y1 ] = y;

        Vertices[ IBatch.X2 ] = x;
        Vertices[ IBatch.Y2 ] = y2;

        Vertices[ IBatch.X3 ] = x2;
        Vertices[ IBatch.Y3 ] = y2;

        Vertices[ IBatch.X4 ] = x2;
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
        X = value - ( Width / 2 );
    }

    /// <summary>
    /// Sets the y position so that it is centered on the given y parameter
    /// </summary>
    public void SetCenterY( float value )
    {
        Y = value - ( Height / 2 );
    }

    /// <summary>
    /// Sets the x position relative to the current position where the sprite will
    /// be drawn. If origin, rotation, or scale are
    /// changed, it is slightly more efficient to translate after those operations.
    /// </summary>
    public void TranslateX( float xAmount )
    {
        X += xAmount;

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
        Y += yAmount;

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
        X += xAmount;
        Y += yAmount;

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
        Vertices[ IBatch.Y1 ] += yAmount;

        Vertices[ IBatch.X2 ] += xAmount;
        Vertices[ IBatch.Y2 ] += yAmount;

        Vertices[ IBatch.X3 ] += xAmount;
        Vertices[ IBatch.Y3 ] += yAmount;

        Vertices[ IBatch.X4 ] += xAmount;
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

        // --- 1. GEOMETRY CALCULATIONS ---
        var originX = Width / 2f;
        var originY = Height / 2f;

        // Local corners relative to center
        var localX1 = -originX * ScaleX;
        var localY1 = -originY * ScaleY;
        var localX2 = ( Width - originX ) * ScaleX;
        var localY2 = ( Height - originY ) * ScaleY;

        var worldCenterX = X + originX;
        var worldCenterY = Y + originY;

        if ( Rotation != 0 )
        {
            var cos = MathUtils.CosDeg( Rotation );
            var sin = MathUtils.SinDeg( Rotation );

            Vertices[ IBatch.X1 ] = localX1 * cos - localY1 * sin + worldCenterX;
            Vertices[ IBatch.Y1 ] = localX1 * sin + localY1 * cos + worldCenterY;

            Vertices[ IBatch.X2 ] = localX1 * cos - localY2 * sin + worldCenterX;
            Vertices[ IBatch.Y2 ] = localX1 * sin + localY2 * cos + worldCenterY;

            Vertices[ IBatch.X3 ] = localX2 * cos - localY2 * sin + worldCenterX;
            Vertices[ IBatch.Y3 ] = localX2 * sin + localY2 * cos + worldCenterY;

            Vertices[ IBatch.X4 ] = localX2 * cos - localY1 * sin + worldCenterX;
            Vertices[ IBatch.Y4 ] = localX2 * sin + localY1 * cos + worldCenterY;
        }
        else
        {
            Vertices[ IBatch.X1 ] = localX1 + worldCenterX;
            Vertices[ IBatch.Y1 ] = localY1 + worldCenterY;
            Vertices[ IBatch.X2 ] = localX1 + worldCenterX;
            Vertices[ IBatch.Y2 ] = localY2 + worldCenterY;
            Vertices[ IBatch.X3 ] = localX2 + worldCenterX;
            Vertices[ IBatch.Y3 ] = localY2 + worldCenterY;
            Vertices[ IBatch.X4 ] = localX2 + worldCenterX;
            Vertices[ IBatch.Y4 ] = localY1 + worldCenterY;
        }

        // --- 2. UV (TEXTURE) CALCULATIONS ---
        // Start with the base region coordinates + our scroll offsets
        var u  = U + _uScrollOffset;
        var v  = V + _vScrollOffset;
        var u2 = U2 + _uScrollOffset;
        var v2 = V2 + _vScrollOffset;

        if ( _flipX )
        {
            ( u, u2 ) = ( u2, u );
        }

        if ( _flipY )
        {
            ( v, v2 ) = ( v2, v );
        }

        // Assign UVs to the vertex array in GetVertices()
        // Match the Bottom-Up geometry (V1=BL, V2=TL, V3=TR, V4=BR) 
        // to the coordinate system of the texture
        Vertices[ IBatch.U1 ] = u;
        Vertices[ IBatch.V1 ] = v2;

        Vertices[ IBatch.U2 ] = u;
        Vertices[ IBatch.V2 ] = v;

        Vertices[ IBatch.U3 ] = u2;
        Vertices[ IBatch.V3 ] = v;

        Vertices[ IBatch.U4 ] = u2;
        Vertices[ IBatch.V4 ] = v2;
        
        return Vertices;
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
    public override void Scroll(float xAmount, float yAmount)
    {
        if (xAmount == 0 && yAmount == 0)
        {
            return;
        }

        _uScrollOffset = (_uScrollOffset + xAmount) % 1.0f;
        _vScrollOffset = (_vScrollOffset + yAmount) % 1.0f;

        // Handle negative scrolling (ensure offset is always positive)
        if (_uScrollOffset < 0)
        {
            _uScrollOffset += 1.0f;
        }

        if (_vScrollOffset < 0)
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

    public override float U
    {
        get => base.U;
        set
        {
            base.U = value;

            Vertices[ IBatch.U1 ] = value;
            Vertices[ IBatch.U2 ] = value;
        }
    }

    public override float V
    {
        get => base.V;
        set
        {
            base.V = value;

            Vertices[ IBatch.V2 ] = value;
            Vertices[ IBatch.V3 ] = value;
        }
    }

    public override float U2
    {
        get => base.U2;
        set
        {
            base.U2 = value;

            Vertices[ IBatch.U3 ] = value;
            Vertices[ IBatch.U4 ] = value;
        }
    }

    public override float V2
    {
        get => base.V2;
        set
        {
            base.V2 = value;

            Vertices[ IBatch.V1 ] = value;
            Vertices[ IBatch.V4 ] = value;
        }
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

    /// <summary>
    /// Sets the x position where the sprite will be drawn. If origin, rotation,
    /// or scale are changed, it is slightly more efficient to set the position
    /// after those operations. If both position and size are to be changed, it
    /// is better to use <see cref="SetBounds(float, float, float, float)"/>.
    /// </summary>
    public virtual float X
    {
        get;
        set
        {
            field = value;

            if ( _isDirty )
            {
                return;
            }

            if ( ( Rotation != 0 ) || ScaleX is not 1 || ScaleY is not 1 )
            {
                _isDirty = true;

                return;
            }

            Vertices[ IBatch.X1 ] = value;
            Vertices[ IBatch.X2 ] = value;
            Vertices[ IBatch.X3 ] = value + Width;
            Vertices[ IBatch.X4 ] = value + Width;
        }
    }

    /// <summary>
    /// Sets the y position where the sprite will be drawn. If origin, rotation,
    /// or scale are changed, it is slightly more efficient to set the position
    /// after those operations. If both position and size are to be changed, it
    /// is better to use <see cref="SetBounds(float, float, float, float)"/>.
    /// </summary>
    public virtual float Y
    {
        get;
        set
        {
            field = value;

            if ( _isDirty )
            {
                return;
            }

            if ( ( Rotation != 0 ) || ScaleX is not 1 || ScaleY is not 1 )
            {
                _isDirty = true;

                return;
            }

            Vertices[ IBatch.Y1 ] = value;
            Vertices[ IBatch.Y2 ] = value + Height;
            Vertices[ IBatch.Y3 ] = value + Height;
            Vertices[ IBatch.Y4 ] = value;
        }
    }
}

// ============================================================================
// ============================================================================