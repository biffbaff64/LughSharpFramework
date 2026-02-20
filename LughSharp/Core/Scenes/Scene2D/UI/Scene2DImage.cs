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

using JetBrains.Annotations;

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Maths;
using LughSharp.Core.Scenes.Scene2D.Utils;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;

using IDrawable = LughSharp.Core.Scenes.Scene2D.Utils.IDrawable;

namespace LughSharp.Core.Scenes.Scene2D.UI;

/// <summary>
/// Displays a <see cref="Utils.IDrawable"/>, scaled various way within the widgets
/// bounds. The preferred size is the min size of the drawable. Only when using
/// a <see cref="TextureRegionDrawable"/> will the actor's scale, rotation, and
/// origin be used when drawing.
/// </summary>
[PublicAPI]
public class Scene2DImage : Widget
{
    public override string? Name => "Scene2DImage";
    public float           ImageX      { get; set; }
    public float           ImageY      { get; set; }
    public float           ImageWidth  { get; set; }
    public float           ImageHeight { get; set; }
    public IDrawable? Drawable    { get; private set; }

    // ========================================================================

    /// <summary>
    /// The alignment of the image within the widget.
    /// </summary>
    public int Alignment
    {
        get;
        set
        {
            field = value;
            Invalidate();
        }
    }

    // ========================================================================

    private Scaling _scaling;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new, unitialised, Image instance.
    /// </summary>
    public Scene2DImage()
    {
        Drawable = null;
        _scaling = Scaling.None;
    }

    /// <summary>
    /// Creates a new Image instance with the specified <see cref="NinePatch"/>.
    /// </summary>
    /// <param name="patch"></param>
    public Scene2DImage( NinePatch patch )
        : this( new NinePatchDrawable( patch ), Scaling.None )
    {
    }

    /// <summary>
    /// Creates a new Image instance with the specified <see cref="TextureRegion"/>.
    /// </summary>
    /// <param name="region"></param>
    public Scene2DImage( TextureRegion region )
        : this( new TextureRegionDrawable( region ), Scaling.None )
    {
    }

    /// <summary>
    /// Creates a new Image instance with the specified <see cref="Texture"/>.
    /// </summary>
    /// <param name="texture"></param>
    public Scene2DImage( Texture texture )
        : this( new TextureRegionDrawable( new TextureRegion( texture ) ) )
    {
    }

    /// <summary>
    /// Creates a new Image instance with the specified <see cref="Skin"/>,
    /// and using the drawable with the specified name.
    /// </summary>
    /// <param name="skin"></param>
    /// <param name="drawableName"></param>
    public Scene2DImage( Skin skin, string drawableName )
        : this( skin.GetDrawable( drawableName ), Scaling.None )
    {
    }

    /// <summary>
    /// Creates a new Image instance with the specified <see cref="IDrawable"/>.
    /// </summary>
    /// <param name="drawable"></param>
    public Scene2DImage( IDrawable drawable )
        : this( drawable, Scaling.None )
    {
    }

    /// <summary>
    /// Creates a new Image instance with the specified <see cref="IDrawable"/>,
    /// <see cref="Scaling"/>Mode, and alignment. Alignment defaults to <see cref="Align.CENTER"/>.
    /// </summary>
    /// <param name="drawable"></param>
    /// <param name="scaling"></param>
    /// <param name="align"></param>
    public Scene2DImage( IDrawable drawable, Scaling scaling, int align = Align.CENTER )
    {
        SetDrawable( drawable );

        _scaling  = scaling;
        Alignment = align;

        SetSize( GetPrefWidthSafe(), GetPrefHeightSafe() );
    }

    /// <summary>
    /// The preferred width of the image widget. This value corresponds to the
    /// minimum width of the associated drawable if one is set, or 0 if no
    /// drawable is assigned.
    /// </summary>
    public override float GetPrefWidth() => GetPrefWidthSafe();

    protected float GetPrefWidthSafe() => Drawable?.MinWidth ?? 0;

    /// <summary>
    /// The preferred height of the widget, typically derived from the minimum
    /// height of the drawable.
    /// </summary>
    public override float GetPrefHeight() => GetPrefHeightSafe();

    protected float GetPrefHeightSafe() => Drawable?.MinHeight ?? 0;

    /// <summary>
    /// Computes and caches any information needed for drawing and, if this actor has
    /// children, positions and sizes each child, calls <see cref="ILayout.Invalidate"/>
    /// on any each child whose width or height has changed, and calls <see cref="ILayout.Validate"/>
    /// on each child. This method should almost never be called directly, instead
    /// <see cref="ILayout.Validate"/> should be used.
    /// </summary>
    public override void SetLayout()
    {
        if ( Drawable is null )
        {
            throw new InvalidOperationException( "Drawable cannot be null" );
        }
        
        var regionWidth  = Drawable.MinWidth;
        var regionHeight = Drawable.MinHeight;
        var width        = Width;
        var height       = Height;

        var size = _scaling.Apply( regionWidth, regionHeight, width, height );

        ImageWidth  = size.X;
        ImageHeight = size.Y;

        if ( ( Alignment & Align.LEFT ) != 0 )
        {
            ImageX = 0;
        }
        else if ( ( Alignment & Align.RIGHT ) != 0 )
        {
            ImageX = ( int )( width - ImageWidth );
        }
        else
        {
            ImageX = ( int )( ( width / 2 ) - ( ImageWidth / 2 ) );
        }

        if ( ( Alignment & Align.TOP ) != 0 )
        {
            ImageY = ( int )( height - ImageHeight );
        }
        else if ( ( Alignment & Align.BOTTOM ) != 0 )
        {
            ImageY = 0;
        }
        else
        {
            ImageY = ( int )( ( height / 2 ) - ( ImageHeight / 2 ) );
        }
    }

    /// <summary>
    /// Draws the actor. The batch is configured to draw in the parent's coordinate system. This
    /// draw method is convenient to draw a rotated and scaled TextureRegion.
    /// <para>
    /// <see cref="IBatch.Begin"/> has already been called on the batch. If <see cref="IBatch.End()"/>
    /// is called to draw without the batch then <see cref="IBatch.Begin"/> must be called before
    /// the method returns.
    /// </para>
    /// <para>
    /// <b>The default implementation does nothing. Child classes should override and implement.</b>
    /// </para>
    /// </summary>
    /// <param name="batch"> The <see cref="IBatch"/> to use. </param>
    /// <param name="parentAlpha">
    /// The parent alpha, to be multiplied with this actor's alpha,
    /// allowing the parent's alpha to affect all children.
    /// </param>
    public override void Draw( IBatch batch, float parentAlpha )
    {
        Validate();

        batch.SetColor( Color.R, Color.G, Color.B, Color.A * parentAlpha );

        var x      = X;
        var y      = Y;
        var scaleX = ScaleX;
        var scaleY = ScaleY;

        if ( Drawable is ITransformDrawable drawable )
        {
            var rotation = Rotation;

            if ( scaleX is not 1 || scaleY is not 1 || rotation is not 0 )
            {
                var region = new GRect
                {
                    X      = ( int )( x + ImageX ),
                    Y      = ( int )( y + ImageY ),
                    Width  = ( int )ImageWidth,
                    Height = ( int )ImageHeight,
                };

                var origin = new Point2D
                {
                    X = ( int )( OriginX - ImageX ),
                    Y = ( int )( OriginY - ImageY ),
                };

                var scale = new Point2D
                {
                    X = ( int )scaleX,
                    Y = ( int )scaleY,
                };

                drawable.Draw( batch, region, origin, scale, rotation );

                return;
            }
        }

        Drawable?.Draw( batch, x + ImageX, y + ImageY, ImageWidth * scaleX, ImageHeight * scaleY );
    }

    /// <summary>
    /// Sets the drawable for the image using the specified skin and drawable name.
    /// </summary>
    /// <param name="skin"></param>
    /// <param name="drawableName"></param>
    public void SetDrawable( Skin skin, string drawableName )
    {
        SetDrawable( skin.GetDrawable( drawableName ) );
    }

    /// <summary>
    /// Sets a new drawable for the image. The image's pref size is the drawable's min size.
    /// If using the image actor's size rather than the pref size, <see cref="Widget.Pack"/>
    /// can be used to size the image to its pref size.
    /// </summary>
    /// <param name="drawable"> May be null. </param>
    public void SetDrawable( IDrawable drawable )
    {
        if ( Drawable == drawable )
        {
            return;
        }

        if ( !GetPrefWidth().Equals( drawable.MinWidth ) || !GetPrefHeight().Equals( drawable.MinHeight ) )
        {
            InvalidateHierarchy();
        }

        Drawable = drawable;
    }

    /// <summary>
    /// Sets the <see cref="Scaling"/>Mode for this Image.
    /// </summary>
    public void SetScaling( Scaling scale )
    {
        Guard.Against.Null( scale );

        _scaling = scale;
        Invalidate();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var name = Name;

        if ( name != null )
        {
            return name;
        }

        var className = GetType().Name;
        var dotIndex  = className.LastIndexOf( '.' );

        if ( dotIndex != -1 )
        {
            className = className.Substring( dotIndex + 1 );
        }

        return ( className.IndexOf( '$' ) != -1 ? "Image " : "" ) + className + ": " + Drawable;
    }
}

// ============================================================================
// ============================================================================