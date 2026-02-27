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

using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Utils;
using LughSharp.Core.Scenes.Scene2D.Utils;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;

using Rectangle = LughSharp.Core.Maths.Rectangle;

namespace LughSharp.Core.Scenes.Scene2D.UI;

/// <summary>
/// A group with a single child that sizes and positions the child using constraints.
/// This provides layout similar to a <see cref="Table"/> with a single cell but is
/// more lightweight.
/// </summary>
[PublicAPI]
public class Container< T > : WidgetGroup where T : Actor
{
    public override string? Name     => "Container";
    public          bool    Rounding { get; set; } = true;

    private T?              _actor;
    private int             _align;
    private ISceneDrawable? _background;
    private bool            _clip;
    private float           _fillX;
    private float           _fillY;

    private Value _maxHeight  = Value.Zero;
    private Value _maxWidth   = Value.Zero;
    private Value _minHeight  = Value.MinHeight;
    private Value _minWidth   = Value.MinWidth;
    private Value _padBottom  = Value.Zero;
    private Value _padLeft    = Value.Zero;
    private Value _padRight   = Value.Zero;
    private Value _padTop     = Value.Zero;
    private Value _prefHeight = Value.PrefHeight;
    private Value _prefWidth  = Value.PrefWidth;

    // ========================================================================

    /// <summary>
    /// Creates a container with no actor.
    /// </summary>
    public Container()
    {
        Touchable = Touchable.ChildrenOnly;
        Transform = false;
    }

    public Container( T? actor ) : this()
    {
        SetActor( actor );
    }

    /// <summary>
    /// Sets the background drawable and, if adjustPadding is true, sets the container's
    /// padding to <see cref="ISceneDrawable.BottomHeight"/> , <see cref="ISceneDrawable.TopHeight"/>,
    /// <see cref="ISceneDrawable.LeftWidth"/>, and <see cref="ISceneDrawable.RightWidth"/>.
    /// </summary>
    /// <param name="background"> If null, the background will be cleared and padding removed. </param>
    /// <param name="adjustPadding"></param>
    public void SetBackground( ISceneDrawable? background, bool adjustPadding = true )
    {
        if ( _background == background )
        {
            return;
        }

        _background = background;

        if ( adjustPadding )
        {
            if ( background == null )
            {
                SetPadding( Value.Zero );
            }
            else
            {
                SetPadding( background.TopHeight,
                            background.LeftWidth,
                            background.BottomHeight,
                            background.RightWidth );
            }

            Invalidate();
        }
    }

    public Container< T > Background( ISceneDrawable background )
    {
        SetBackground( background );

        return this;
    }

    public ISceneDrawable? GetBackground()
    {
        return _background;
    }

    public void Layout()
    {
        if ( _actor == null )
        {
            return;
        }

        float padLeft         = _padLeft.Get( this );
        float padBottom       = _padBottom.Get( this );
        float containerWidth  = Width - padLeft - _padRight.Get( this );
        float containerHeight = Height - padBottom - _padTop.Get( this );
        float minWidth        = _minWidth.Get( _actor );
        float minHeight       = _minHeight.Get( _actor );
        float prefWidth       = _prefWidth.Get( _actor );
        float prefHeight      = _prefHeight.Get( _actor );
        float maxWidth        = _maxWidth.Get( _actor );
        float maxHeight       = _maxHeight.Get( _actor );

        float width;

        if ( _fillX > 0 )
        {
            width = containerWidth * _fillX;
        }
        else
        {
            width = Math.Min( prefWidth, containerWidth );
        }

        if ( width < minWidth )
        {
            width = minWidth;
        }

        if ( ( maxWidth > 0 ) && ( width > maxWidth ) )
        {
            width = maxWidth;
        }

        float height;

        if ( _fillY > 0 )
        {
            height = containerHeight * _fillY;
        }
        else
        {
            height = Math.Min( prefHeight, containerHeight );
        }

        if ( height < minHeight )
        {
            height = minHeight;
        }

        if ( ( maxHeight > 0 ) && ( height > maxHeight ) )
        {
            height = maxHeight;
        }

        float x = padLeft;

        if ( ( _align & Align.RIGHT ) != 0 )
        {
            x += containerWidth - width;
        }
        else if ( ( _align & Align.LEFT ) == 0 )
        {
            x += ( containerWidth - width ) / 2;
        }

        float y = padBottom;

        if ( ( _align & Align.TOP ) != 0 )
        {
            y += containerHeight - height;
        }
        else if ( ( _align & Align.BOTTOM ) == 0 )
        {
            y += ( containerHeight - height ) / 2;
        }

        if ( Rounding )
        {
            x      = ( float )Math.Round( x );
            y      = ( float )Math.Round( y );
            width  = ( float )Math.Round( width );
            height = ( float )Math.Round( height );
        }

        _actor.SetBounds( x, y, width, height );

        if ( _actor is ILayout layoutActor )
        {
            layoutActor.Validate();
        }
    }

    public void SetCullingArea( Rectangle cullingArea )
    {
        CullingArea = cullingArea;

        if ( _fillX is 1f && _fillY is 1f && _actor is ICullable cullableActor )
        {
            cullableActor.CullingArea = cullingArea;
        }
    }

    public void SetActor( T? actor )
    {
        if ( actor == this )
        {
            throw new ArgumentException( "actor cannot be the Container." );
        }

        if ( actor == _actor )
        {
            return;
        }

        if ( _actor != null )
        {
            base.RemoveActor( _actor, true );
        }

        _actor = actor;

        if ( actor != null )
        {
            AddActor( actor );
        }
    }

    public T? GetActor()
    {
        return _actor;
    }

    // ========================================================================
    // ========================================================================

    public bool RemoveActor( Actor actor )
    {
        Guard.Against.Null( actor );

        if ( actor != _actor )
        {
            return false;
        }

        SetActor( null );

        return true;
    }

    public override bool RemoveActor( Actor actor, bool unfocus )
    {
        Guard.Against.Null( actor );

        if ( actor != _actor )
        {
            return false;
        }

        _actor = null;

        return base.RemoveActor( actor, unfocus );
    }

    public override Actor? RemoveActorAt( int index, bool unfocus )
    {
        Actor? actor = base.RemoveActorAt( index, unfocus );

        if ( actor == _actor )
        {
            _actor = null;
        }

        return actor;
    }

    /// <summary>
    /// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and
    /// maxHeight to the specified values.
    /// </summary>
    public Container< T > Size( Value size )
    {
        Guard.Against.Null( size );

        _minWidth   = size;
        _minHeight  = size;
        _prefWidth  = size;
        _prefHeight = size;
        _maxWidth   = size;
        _maxHeight  = size;

        return this;
    }

    /// <summary>
    /// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and
    /// maxHeight to the specified values.
    /// </summary>
    public Container< T > Size( Value width, Value height )
    {
        Guard.Against.Null( width );
        Guard.Against.Null( height );

        _minWidth   = width;
        _minHeight  = height;
        _prefWidth  = width;
        _prefHeight = height;
        _maxWidth   = width;
        _maxHeight  = height;

        return this;
    }

    /// <summary>
    /// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and
    /// maxHeight to the specified values.
    /// </summary>
    public Container< T > Size( float size )
    {
        Size( Value.Fixed.ValueOf( size ) );

        return this;
    }

    /// <summary>
    /// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and
    /// maxHeight to the specified values.
    /// </summary>
    public Container< T > Size( float width, float height )
    {
        Size( Value.Fixed.ValueOf( width ), Value.Fixed.ValueOf( height ) );

        return this;
    }

    // ========================================================================
    // ========================================================================

    public Container< T > SetFill( float x = 1f, float y = 1f )
    {
        _fillX = x;
        _fillY = y;

        return this;
    }

    /// <summary>
    /// Sets fillX to 1.
    /// </summary>
    public Container< T > SetFillX()
    {
        _fillX = 1f;

        return this;
    }

    /// <summary>
    /// Sets fillY to 1.
    /// </summary>
    public Container< T > SetFillY()
    {
        _fillY = 1f;

        return this;
    }

    /// <summary>
    /// Sets fillX and fillY to 1 if true, 0 if false.
    /// </summary>
    public Container< T > FillOnTrue( bool x, bool y )
    {
        _fillX = x ? 1f : 0;
        _fillY = y ? 1f : 0;

        return this;
    }

    /// <summary>
    /// Sets fillX and fillY to 1 if true, 0 if false.
    /// </summary>
    public Container< T > FillOnTrue( bool fill )
    {
        _fillX = fill ? 1f : 0;
        _fillY = fill ? 1f : 0;

        return this;
    }

    // ========================================================================
    // ========================================================================

    public float GetFillX()
    {
        return _fillX;
    }

    public float GetFillY()
    {
        return _fillY;
    }

    public Container< T > Clip( bool enabled = true )
    {
        SetClip( enabled );

        return this;
    }

    /// <summary>
    /// Causes the contents to be clipped if they exceed the container bounds.
    /// Enabling clipping will set <see cref="Group.Transform"/> to true.
    /// </summary>
    public void SetClip( bool enabled )
    {
        _clip     = enabled;
        Transform = enabled;

        Invalidate();
    }

    public bool GetClip()
    {
        return _clip;
    }

    public override Actor? Hit( float x, float y, bool touchable )
    {
        if ( _clip )
        {
            if ( touchable && ( Touchable == Touchable.Disabled ) )
            {
                return null;
            }

            if ( ( x < 0 ) || ( x >= Width ) || ( y < 0 ) || ( y >= Height ) )
            {
                return null;
            }
        }

        return base.Hit( x, y, touchable );
    }

    // ========================================================================
    // ========================================================================

    #region widths

    /// <summary>
    /// Sets the minWidth, prefWidth, and maxWidth to the specified value.
    /// </summary>
    public Container< T > SetWidths( Value width )
    {
        Guard.Against.Null( width );

        _minWidth  = width;
        _prefWidth = width;
        _maxWidth  = width;

        return this;
    }

    /// <summary>
    /// Sets the minWidth, prefWidth, and maxWidth to the specified value.
    /// </summary>
    public Container< T > SetWidths( float width )
    {
        SetWidths( Value.Fixed.ValueOf( width ) );

        return this;
    }

    public Container< T > SetMinWidth( Value minWidth )
    {
        Guard.Against.Null( minWidth );

        _minWidth = minWidth;

        return this;
    }

    public Container< T > SetMinWidth( float minWidth )
    {
        _minWidth = Value.Fixed.ValueOf( minWidth );

        return this;
    }

    public override float GetMinWidth()
    {
        return _minWidth.Get( _actor ) + _padLeft.Get( this ) + _padRight.Get( this );
    }

    public Value GetMinWidthValue()
    {
        return _minWidth;
    }

    public override float GetMaxWidth()
    {
        float v = _maxWidth.Get( _actor );

        if ( v > 0 )
        {
            v += _padLeft.Get( this ) + _padRight.Get( this );
        }

        return v;
    }

    public Value GetMaxWidthValue()
    {
        return _maxWidth;
    }

    #endregion widths

    // ========================================================================
    // ========================================================================

    #region heights

    /// <summary>
    /// Sets the minHeight, prefHeight, and maxHeight to the specified value.
    /// </summary>
    public Container< T > SetHeights( Value height )
    {
        Guard.Against.Null( height );

        _minHeight  = height;
        _prefHeight = height;
        _maxHeight  = height;

        return this;
    }

    /// <summary>
    /// Sets the minHeight, prefHeight, and maxHeight to the specified value.
    /// </summary>
    public Container< T > SetHeights( float height )
    {
        SetHeights( Value.Fixed.ValueOf( height ) );

        return this;
    }

    public Container< T > SetMinHeight( Value minHeight )
    {
        Guard.Against.Null( minHeight );

        _minHeight = minHeight;

        return this;
    }

    public Container< T > SetMinHeight( float minHeight )
    {
        _minHeight = Value.Fixed.ValueOf( minHeight );

        return this;
    }

    public override float GetMinHeight()
    {
        return _minHeight.Get( _actor ) + _padTop.Get( this ) + _padBottom.Get( this );
    }

    public Value GetMinHeightValue()
    {
        return _minHeight;
    }

    public override float GetMaxHeight()
    {
        float v = _maxHeight.Get( _actor );

        if ( v > 0 )
        {
            v += _padTop.Get( this ) + _padBottom.Get( this );
        }

        return v;
    }

    public Value GetMaxHeightValue()
    {
        return _maxHeight;
    }

    #endregion heights

    // ========================================================================
    // ========================================================================

    #region minimum sizes

    /// <summary>
    /// Sets the minWidth and minHeight to the specified values.
    /// </summary>
    public Container< T > SetMinSize( Value size )
    {
        Guard.Against.Null( size );

        _minWidth  = size;
        _minHeight = size;

        return this;
    }

    /// <summary>
    /// Sets the minWidth and minHeight to the specified values.
    /// </summary>
    public Container< T > SetMinSize( Value width, Value height )
    {
        Guard.Against.Null( width );
        Guard.Against.Null( height );

        _minWidth  = width;
        _minHeight = height;

        return this;
    }

    /// <summary>
    /// Sets the minWidth and minHeight to the specified value.
    /// </summary>
    public Container< T > SetMinSize( float size )
    {
        SetMinSize( Value.Fixed.ValueOf( size ) );

        return this;
    }

    /// <summary>
    /// Sets the minWidth and minHeight to the specified values.
    /// </summary>
    public Container< T > SetMinSize( float width, float height )
    {
        SetMinSize( Value.Fixed.ValueOf( width ), Value.Fixed.ValueOf( height ) );

        return this;
    }

    #endregion minimum sizes

    // ========================================================================
    // ========================================================================

    #region maximum sizes

    /// <summary>
    /// Sets the maxWidth and maxHeight to the specified values.
    /// </summary>
    public Container< T > SetMaxSize( Value size )
    {
        Guard.Against.Null( size );

        _maxWidth  = size;
        _maxHeight = size;

        return this;
    }

    /// <summary>
    /// Sets the maxWidth and maxHeight to the specified values.
    /// </summary>
    public Container< T > SetMaxSize( Value width, Value height )
    {
        Guard.Against.Null( width );
        Guard.Against.Null( height );

        _maxWidth  = width;
        _maxHeight = height;

        return this;
    }

    public Container< T > SetMaxWidth( Value maxWidth )
    {
        Guard.Against.Null( maxWidth );

        _maxWidth = maxWidth;

        return this;
    }

    public Container< T > SetMaxHeight( Value maxHeight )
    {
        Guard.Against.Null( maxHeight );

        _maxHeight = maxHeight;

        return this;
    }

    /// <summary>
    /// Sets the maxWidth and maxHeight to the specified values.
    /// </summary>
    public Container< T > SetMaxSize( float size )
    {
        SetMaxSize( Value.Fixed.ValueOf( size ) );

        return this;
    }

    /// <summary>
    /// Sets the maxWidth and maxHeight to the specified values.
    /// </summary>
    public Container< T > SetMaxSize( float width, float height )
    {
        SetMaxSize( Value.Fixed.ValueOf( width ), Value.Fixed.ValueOf( height ) );

        return this;
    }

    public Container< T > SetMaxWidth( float maxWidth )
    {
        _maxWidth = Value.Fixed.ValueOf( maxWidth );

        return this;
    }

    public Container< T > SetMaxHeight( float maxHeight )
    {
        _maxHeight = Value.Fixed.ValueOf( maxHeight );

        return this;
    }

    #endregion maximum sizes

    // ========================================================================
    // ========================================================================

    #region preferred sizing

    /// <summary>
    /// Sets the prefWidth and prefHeight to the specified value.
    /// </summary>
    public Container< T > SetPrefSize( Value size )
    {
        Guard.Against.Null( size );

        _prefWidth  = size;
        _prefHeight = size;

        return this;
    }

    /// <summary>
    /// Sets the prefWidth and prefHeight to the specified values.
    /// </summary>
    public Container< T > SetPrefSize( Value width, Value height )
    {
        Guard.Against.Null( width );
        Guard.Against.Null( height );

        _prefWidth  = width;
        _prefHeight = height;

        return this;
    }

    /// <summary>
    /// Sets the prefWidth and prefHeight to the specified value.
    /// </summary>
    public Container< T > SetPrefSize( float width, float height )
    {
        SetPrefSize( Value.Fixed.ValueOf( width ), Value.Fixed.ValueOf( height ) );

        return this;
    }

    /// <summary>
    /// Sets the prefWidth and prefHeight to the specified value.
    /// </summary>
    public Container< T > SetPrefSize( float size )
    {
        SetPrefSize( Value.Fixed.ValueOf( size ) );

        return this;
    }

    public Container< T > SetPrefWidth( Value prefWidth )
    {
        Guard.Against.Null( prefWidth );

        _prefWidth = prefWidth;

        return this;
    }

    public Container< T > SetPrefWidth( float prefWidth )
    {
        _prefWidth = Value.Fixed.ValueOf( prefWidth );

        return this;
    }

    public Container< T > SetPrefHeight( Value prefHeight )
    {
        Guard.Against.Null( prefHeight );

        _prefHeight = prefHeight;

        return this;
    }

    public Container< T > SetPrefHeight( float prefHeight )
    {
        _prefHeight = Value.Fixed.ValueOf( prefHeight );

        return this;
    }

    public override float GetPrefWidth()
    {
        float v = _prefWidth.Get( _actor );

        if ( _background != null )
        {
            v = Math.Max( v, _background.MinWidth );
        }

        return Math.Max( GetMinWidth(), v + _padLeft.Get( this ) + _padRight.Get( this ) );
    }

    public override float GetPrefHeight()
    {
        float v = _prefHeight.Get( _actor );

        if ( _background != null )
        {
            v = Math.Max( v, _background.MinHeight );
        }

        return Math.Max( GetMinHeight(), v + _padTop.Get( this ) + _padBottom.Get( this ) );
    }

    #endregion preferred sizing

    // ========================================================================
    // ========================================================================

    #region padding

    /// <summary>
    /// Sets the padTop, padLeft, padBottom, and padRight to the specified value.
    /// </summary>
    public Container< T > SetPadding( Value pad )
    {
        Guard.Against.Null( pad );

        _padTop    = pad;
        _padLeft   = pad;
        _padBottom = pad;
        _padRight  = pad;

        return this;
    }

    /// <summary>
    /// Sets the padTop, padLeft, padBottom, and padRight to the specified value.
    /// </summary>
    public Container< T > SetPadding( Value top, Value left, Value bottom, Value right )
    {
        Guard.Against.Null( top );
        Guard.Against.Null( left );
        Guard.Against.Null( bottom );
        Guard.Against.Null( right );

        _padTop    = top;
        _padLeft   = left;
        _padBottom = bottom;
        _padRight  = right;

        return this;
    }

    /// <summary>
    /// Sets the padTop, padLeft, padBottom, and padRight to the specified value.
    /// </summary>
    public Container< T > SetPadding( float pad )
    {
        Value value = Value.Fixed.ValueOf( pad );

        _padTop    = value;
        _padLeft   = value;
        _padBottom = value;
        _padRight  = value;

        return this;
    }

    public Container< T > SetPadding( float top, float left, float bottom, float right )
    {
        _padTop    = Value.Fixed.ValueOf( top );
        _padLeft   = Value.Fixed.ValueOf( left );
        _padBottom = Value.Fixed.ValueOf( bottom );
        _padRight  = Value.Fixed.ValueOf( right );

        return this;
    }

    public Container< T > SetPadTop( Value padTop )
    {
        Guard.Against.Null( padTop );

        _padTop = padTop;

        return this;
    }

    public Container< T > SetPadLeft( Value padLeft )
    {
        Guard.Against.Null( padLeft );

        _padLeft = padLeft;

        return this;
    }

    public Container< T > SetPadBottom( Value padBottom )
    {
        Guard.Against.Null( padBottom );

        _padBottom = padBottom;

        return this;
    }

    public Container< T > SetPadRight( Value padRight )
    {
        Guard.Against.Null( padRight );

        _padRight = padRight;

        return this;
    }

    public Container< T > SetPadTop( float padTop )
    {
        _padTop = Value.Fixed.ValueOf( padTop );

        return this;
    }

    public Container< T > SetPadLeft( float padLeft )
    {
        _padLeft = Value.Fixed.ValueOf( padLeft );

        return this;
    }

    public Container< T > SetPadBottom( float padBottom )
    {
        _padBottom = Value.Fixed.ValueOf( padBottom );

        return this;
    }

    public Container< T > SetPadRight( float padRight )
    {
        _padRight = Value.Fixed.ValueOf( padRight );

        return this;
    }

    public Value GetPadTopValue()
    {
        return _padTop;
    }

    public float GetPadTop()
    {
        return _padTop.Get( this );
    }

    public Value GetPadLeftValue()
    {
        return _padLeft;
    }

    public float GetPadLeft()
    {
        return _padLeft.Get( this );
    }

    public Value GetPadBottomValue()
    {
        return _padBottom;
    }

    public float GetPadBottom()
    {
        return _padBottom.Get( this );
    }

    public Value GetPadRightValue()
    {
        return _padRight;
    }

    public float GetPadRight()
    {
        return _padRight.Get( this );
    }

    public float GetPadX()
    {
        return _padLeft.Get( this ) + _padRight.Get( this );
    }

    public float GetPadY()
    {
        return _padTop.Get( this ) + _padBottom.Get( this );
    }

    #endregion padding

    // ========================================================================
    // ========================================================================

    #region alignment

    /// <summary>
    /// Sets the alignment of the actor within the container.
    /// Set to <see cref="Align.CENTER"/>, <see cref="Align.TOP"/>,
    /// <see cref="Align.BOTTOM"/>, <see cref="Align.LEFT"/>,
    /// <see cref="Align.RIGHT"/>, or any combination of those.
    /// </summary>
    public Container< T > SetAlignment( int align )
    {
        _align = align;

        return this;
    }

    /// <summary>
    /// Sets the alignment of the actor within the container to <see cref="Align.CENTER"/>.
    /// This clears any other alignment.
    /// </summary>
    public Container< T > AlignCenter()
    {
        _align = Align.CENTER;

        return this;
    }

    /// <summary>
    /// Sets <see cref="Align.TOP"/> and clears <see cref="Align.BOTTOM"/> for
    /// the alignment of the actor within the container.
    /// </summary>
    public Container< T > AlignTop()
    {
        _align |= Align.TOP;
        _align &= ~Align.BOTTOM;

        return this;
    }

    /// <summary>
    /// Sets <see cref="Align.LEFT"/> and clears <see cref="Align.RIGHT"/> for
    /// the alignment of the actor within the container.
    /// </summary>
    public Container< T > AlignLeft()
    {
        _align |= Align.LEFT;
        _align &= ~Align.RIGHT;

        return this;
    }

    /// <summary>
    /// Sets <see cref="Align.BOTTOM"/> and clears <see cref="Align.TOP"/> for
    /// the alignment of the actor within the container.
    /// </summary>
    public Container< T > AlignBottom()
    {
        _align |= Align.BOTTOM;
        _align &= ~Align.TOP;

        return this;
    }

    /// <summary>
    /// Sets <see cref="Align.RIGHT"/> and clears <see cref="Align.LEFT"/> for the
    /// alignment of the actor within the container.
    /// </summary>
    public Container< T > AlignRight()
    {
        _align |= Align.RIGHT;
        _align &= ~Align.LEFT;

        return this;
    }

    public int GetAlignment()
    {
        return _align;
    }

    #endregion alignment

    // ========================================================================
    // ========================================================================

    #region drawing

    public override void Draw( IBatch batch, float parentAlpha )
    {
        Validate();

        if ( Transform )
        {
            ApplyTransform( batch, ComputeTransform() );
            DrawBackground( batch, parentAlpha, 0, 0 );

            if ( _clip )
            {
                batch.Flush();

                float padLeft   = _padLeft.Get( this );
                float padBottom = _padBottom.Get( this );

                if ( ClipBegin(
                               padLeft,
                               padBottom,
                               Width - padLeft - _padRight.Get( this ),
                               Height - padBottom - _padTop.Get( this )
                              ) )
                {
                    DrawChildren( batch, parentAlpha );
                    batch.Flush();
                    ClipEnd();
                }
            }
            else
            {
                DrawChildren( batch, parentAlpha );
            }

            ResetTransform( batch );
        }
        else
        {
            DrawBackground( batch, parentAlpha, X, Y );
            base.Draw( batch, parentAlpha );
        }
    }

    /// <summary>
    /// Called to draw the background, before clipping is applied (if enabled).
    /// Default implementation draws the background drawable.
    /// </summary>
    protected void DrawBackground( IBatch batch, float parentAlpha, float x, float y )
    {
        if ( _background == null )
        {
            return;
        }

        batch.SetColor( Color.R, Color.G, Color.B, Color.A * parentAlpha );
        _background.Draw( batch, x, y, Width, Height );
    }

    public override void DrawDebug( ShapeRenderer shapes )
    {
        Validate();

        if ( Transform )
        {
            ApplyTransform( shapes, ComputeTransform() );

            if ( _clip )
            {
                shapes.Flush();

                float padLeft   = _padLeft.Get( this );
                float padBottom = _padBottom.Get( this );

                bool draw = _background == null
                    ? ClipBegin( 0, 0, Width, Height )
                    : ClipBegin(
                                padLeft,
                                padBottom,
                                Width - padLeft - _padRight.Get( this ),
                                Height - padBottom - _padTop.Get( this )
                               );

                if ( draw )
                {
                    DrawDebugChildren( shapes );
                    ClipEnd();
                }
            }
            else
            {
                DrawDebugChildren( shapes );
            }

            ResetTransform( shapes );
        }
        else
        {
            base.DrawDebug( shapes );
        }
    }

    #endregion drawing
}