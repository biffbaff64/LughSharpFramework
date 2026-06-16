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

using LughSharp.Source.Graphics.Atlases;
using LughSharp.Source.Graphics.G2D;

namespace LughSharp.Source.Scene2D.Utils;

/// <summary>
/// Drawable for a <see cref="Sprite2D"/>, which is a <see cref="ISceneDrawable"/>
/// that can be drawn using <see cref="Sprite2D.Draw(IBatch)"/>.
/// </summary>
[PublicAPI]
public class SpriteDrawable : BaseDrawable, ITransformDrawable
{
    private Sprite2D? _sprite;

    // ========================================================================
    
    /// <summary>
    /// Creates an uninitialized SpriteDrawable. The sprite must be set before use.
    /// </summary>
    public SpriteDrawable()
    {
    }

    /// <summary>
    /// Creates a new SpriteDrawable using the specified <see cref="Sprite2D"/>.
    /// </summary>
    /// <param name="sprite"> The <see cref="Sprite2D"/> to use. </param>
    public SpriteDrawable( Sprite2D sprite )
    {
        Sprite = sprite;
    }

    /// <summary>
    /// Creates a new SpriteDrawable, initialised with the <see cref="Sprite2D"/>
    /// from another SpriteDrawable.
    /// </summary>
    /// <param name="drawable"> The <see cref="SpriteDrawable"/> to copy. </param>
    public SpriteDrawable( SpriteDrawable? drawable ) : base( drawable )
    {
        Sprite = drawable?.Sprite;
    }

    /// <summary>
    /// The <see cref="Sprite2D"/> component of this <see cref="ISceneDrawable"/>.
    /// </summary>
    public Sprite2D? Sprite
    {
        get => _sprite;
        set
        {
            _sprite = value;

            MinWidth  = value?.Width ?? 0;
            MinHeight = value?.Height ?? 0;
        }
    }

    /// <summary>
    /// Draws this drawable at the specified bounds. The drawable should be tinted
    /// with <see cref="IBatch.Color"/>, possibly by mixing its own color.
    /// The default implementation does nothing.
    /// </summary>
    public override void Draw( IBatch batch, float x, float y, float width, float height )
    {
        if ( Sprite == null )
        {
            return;
        }

        Color spriteColor = Sprite.GetColor();
        float oldColor    = spriteColor.ToFloatBitsRgba();

        Sprite.SetColor( spriteColor.Mul( batch.Color ) );

        Sprite.Rotation = 0;
        Sprite.SetScale( 1, 1 );
        Sprite.SetBounds( x, y, width, height );
        Sprite.Draw( batch );

        Sprite.SetPackedColor( oldColor );
    }

    /// <summary>
    /// Draws the sprite using the specified batch, applying transformations such as position,
    /// scale, origin, and rotation.
    /// </summary>
    /// <param name="batch">The batch used to draw the sprite.</param>
    /// <param name="region">The rectangular region where the sprite will be drawn.</param>
    /// <param name="origin">The origin point for scaling and rotation of the sprite.</param>
    /// <param name="scale">The scaling factors to be applied to the sprite along the X and Y axes.</param>
    /// <param name="rotation">The rotation angle, in degrees, to be applied to the sprite.</param>
    public void Draw( IBatch batch, GRect region, Point2D origin, Point2D scale, float rotation )
    {
        if ( Sprite == null )
        {
            return;
        }

        Color spriteColor = Sprite.GetColor();
        float oldColor    = spriteColor.ToFloatBitsRgba();

        Sprite.SetColor( spriteColor.Mul( batch.Color ) );

        Sprite.SetOrigin( origin.X, origin.Y );
        Sprite.Rotation = rotation;
        Sprite.SetScale( scale.X, scale.Y );
        Sprite.SetBounds( region.X, region.Y, region.Width, region.Height );
        Sprite.Draw( batch );
        Sprite.SetPackedColor( oldColor );
    }

    /// <summary>
    /// Creates a new drawable that renders the same as this drawable,
    /// tinted with the specified color.
    /// </summary>
    public SpriteDrawable Tint( Color tint )
    {
        Sprite2D newSprite;

        if ( _sprite is AtlasSprite sprite )
        {
            newSprite = new AtlasSprite( sprite );
        }
        else
        {
            newSprite = new Sprite2D( _sprite! );
        }

        newSprite.SetColor( tint );
        newSprite.SetSize( MinWidth, MinHeight );

        var drawable = new SpriteDrawable( newSprite )
        {
            LeftWidth    = LeftWidth,
            RightWidth   = RightWidth,
            TopHeight    = TopHeight,
            BottomHeight = BottomHeight
        };

        return drawable;
    }

    /// <summary>
    /// Creates a copy of this drawable.
    /// </summary>
    public override SpriteDrawable Copy() => new( this );
}

// ============================================================================
// ============================================================================

