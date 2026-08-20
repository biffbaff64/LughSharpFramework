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

using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Graphics.Utils;
using LughSharp.Source.Scene2D.Utils;

using Rectangle = LughSharp.Source.Maths.Rectangle;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// A group with a single child that sizes and positions the child using constraints.
/// This provides layout similar to a <see cref="Table"/> with a single cell but is
/// more lightweight.
/// </summary>
[PublicAPI]
public class Container< T > : WidgetGroup where T : Actor
{
    /// <summary>
    /// Gets or sets a value indicating whether the dimensions and positions of the
    /// container's child actor are rounded to the nearest integer. Rounding can help
    /// avoid subpixel rendering artifacts for visual elements.
    /// </summary>
    public bool Rounding { get; set; } = true;

    // ========================================================================
    
    private T?              _actor;
    private Align           _align;
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
    /// Creates a container with no associated actor.
    /// </summary>
    public Container()
    {
        Touchable = Touchable.ChildrenOnly;
        Transform = false;
    }

    /// <summary>
    /// Creates a container with the specified actor.
    /// </summary>
    public Container( T? actor ) : this()
    {
        SetContainerActor( actor );
    }

    /// <summary>
    /// Gets the background drawable.
    /// </summary>
    /// <returns> The background drawable. </returns>
    public ISceneDrawable? GetBackground()
    {
        return _background;
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

            InvalidateLayout();
        }
    }

    /// <summary>
    /// Sets the background drawable and, if adjustPadding is true, sets the container's
    /// padding to <see cref="ISceneDrawable.BottomHeight"/> , <see cref="ISceneDrawable.TopHeight"/>,
    /// <see cref="ISceneDrawable.LeftWidth"/>, and <see cref="ISceneDrawable.RightWidth"/>.
    /// </summary>
    /// <param name="background"> If null, the background will be cleared and padding removed. </param>
    /// <returns></returns>
    public Container< T > Background( ISceneDrawable background )
    {
        SetBackground( background );

        return this;
    }

    /// <summary>
    /// Positions and sizes children of the table using the cell associated with each child.
    /// The values given are the position within the parent and size of the table.
    /// </summary>
    public override void Layout()
    {
        if ( _actor == null )
        {
            return;
        }

        float padLeft         = _padLeft.Get( this );
        float padBottom       = _padBottom.Get( this );
        float containerWidth  = GetWidth() - padLeft - _padRight.Get( this );
        float containerHeight = GetHeight() - padBottom - _padTop.Get( this );
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

        if ( ( _align & Align.Right ) != 0 )
        {
            x += containerWidth - width;
        }
        else if ( ( _align & Align.Left ) == 0 )
        {
            x += ( containerWidth - width ) / 2;
        }

        float y = padBottom;

        if ( ( _align & Align.Top ) != 0 )
        {
            y += containerHeight - height;
        }
        else if ( ( _align & Align.Bottom ) == 0 )
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

    /// <summary>
    /// Sets the culling area for the container's layout and, if applicable,
    /// updates the culling area of the contained actor that implements ICullable.
    /// </summary>
    /// <param name="cullingArea">The rectangular area used for culling.</param>
    public void SetCullingArea( Rectangle cullingArea )
    {
        CullingArea = cullingArea;

        if ( _fillX is 1f && _fillY is 1f && _actor is ICullable cullableActor )
        {
            cullableActor.CullingArea = cullingArea;
        }
    }

    /// <summary>
    /// Gets the actor associated with the container.
    /// </summary>
    /// <returns> The actor associated with the container. </returns>
    public T? GetContainerActor()
    {
        return _actor;
    }

    /// <summary>
    /// Sets the actor contained within the container.
    /// </summary>
    /// <param name="actor">
    /// The actor to set within the container. Passing null will remove the current actor.
    /// </param>
    /// <exception cref="ArgumentException">Thrown when the actor is the container itself.</exception>
    public void SetContainerActor( T? actor )
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

    /// <summary>
    /// Removes the specified actor from the container.
    /// </summary>
    /// <param name="actor">The actor to remove.</param>
    /// <returns>True if the actor was removed, false otherwise.</returns>
    public bool RemoveActor( Actor actor )
    {
        Guard.Against.Null( actor );

        if ( actor != _actor )
        {
            return false;
        }

        SetContainerActor( null );

        return true;
    }

    /// <summary>
    /// Removes an actor from this group.
    /// <param name="actor"> The actor to remove. </param>
    /// <param name="unfocus"> Unfocuses the actor if true. </param>
    /// </summary>
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

    /// <summary>
    /// Removes an actor from this group. If the actor will not be used again and
    /// has actions, they should be cleared using <see cref="Actor.ClearActions()"/>
    /// so the actions will be returned to their <see cref="SceneAction.Pool"/>, if
    /// any. This is not done automatically.
    /// </summary>
    /// <param name="index"> The group index of the actor to remove. </param>
    /// <param name="unfocus"> Unfocuses the actor if true. </param>
    /// <returns> The actor removed from this group. </returns>
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
    /// <returns> This container for chaining. </returns>
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
    /// <returns> This container for chaining. </returns>
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
    /// <returns> This container for chaining. </returns>
    public Container< T > Size( float size )
    {
        Size( Value.Fixed.ValueOf( size ) );

        return this;
    }

    /// <summary>
    /// Sets the minWidth, prefWidth, maxWidth, minHeight, prefHeight, and
    /// maxHeight to the specified values.
    /// </summary>
    /// <returns> This container for chaining. </returns>
    public Container< T > Size( float width, float height )
    {
        Size( Value.Fixed.ValueOf( width ), Value.Fixed.ValueOf( height ) );

        return this;
    }

    /// <summary>
    /// Sets the fill values for the container along the X and Y axes.
    /// </summary>
    /// <param name="x">
    /// The horizontal fill value. A value of 1 means full width, and 0 means no width. Default is 1.
    /// </param>
    /// <param name="y">
    /// The vertical fill value. A value of 1 means full height, and 0 means no height. Default is 1.
    /// </param>
    /// <returns>The container instance, for method chaining.</returns>
    public Container< T > SetFill( float x = 1f, float y = 1f )
    {
        _fillX = x;
        _fillY = y;

        return this;
    }

    /// <summary>
    /// Sets the horizontal fill value of the container to 1, ensuring the actor inside the container
    /// will expand to fully occupy the container's width if possible.
    /// </summary>
    /// <returns>The current container instance, allowing for method chaining.</returns>
    public Container< T > SetFillX()
    {
        _fillX = 1f;

        return this;
    }

    /// <summary>
    /// Sets the vertical fill value of the container to 1, ensuring the actor inside the container
    /// will expand to fully occupy the container's height if possible.
    /// </summary>
    /// <returns>The current container instance, allowing for method chaining.</returns>
    public Container< T > SetFillY()
    {
        _fillY = 1f;

        return this;
    }

    /// <summary>
    /// Sets horizontal and vertical fill values based on the specified bool conditions.
    /// </summary>
    /// <param name="x">If true, sets the horizontal fill to 1; otherwise, sets it to 0.</param>
    /// <param name="y">If true, sets the vertical fill to 1; otherwise, sets it to 0.</param>
    /// <returns>Returns the current container instance with updated fill values.</returns>
    public Container< T > FillOnTrue( bool x, bool y )
    {
        _fillX = x ? 1f : 0;
        _fillY = y ? 1f : 0;

        return this;
    }

    /// <summary>
    /// Sets the container's horizontal and vertical fill values to 1 if the specified condition is true,
    /// otherwise sets them to 0.
    /// </summary>
    /// <param name="fill">A bool value that determines whether the container should be filled
    /// horizontally and vertically.</param>
    /// <returns>The current instance of the container, with updated fill values.</returns>
    public Container< T > FillOnTrue( bool fill )
    {
        _fillX = fill ? 1f : 0;
        _fillY = fill ? 1f : 0;

        return this;
    }

    /// <summary>
    /// Gets the horizontal fill value of the container.
    /// </summary>
    /// <returns> The horizontal fill value of the container. </returns>
    public float GetFillX()
    {
        return _fillX;
    }

    /// <summary>
    /// Gets the vertical fill value of the container.
    /// </summary>
    /// <returns> The vertical fill value of the container. </returns>
    public float GetFillY()
    {
        return _fillY;
    }

    /// <summary>
    /// Enables or disables clipping for this container.
    /// </summary>
    /// <param name="enabled">
    /// If true, clipping will be enabled for the container; otherwise, it will be disabled.
    /// </param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
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

        InvalidateLayout();
    }

    /// <summary>
    /// Gets the clipping state of the container.
    /// </summary>
    /// <returns>True if clipping is enabled for the container; otherwise, false.</returns>
    public bool GetClip()
    {
        return _clip;
    }

    /// <summary>
    /// Determines the topmost child actor at the specified coordinates that is visible and,
    /// if required, touchable.
    /// </summary>
    /// <remarks>
    /// The method traverses child actors in reverse order, returning the first child that is 
    /// hit. If no child is hit, the method delegates to the base implementation. Coordinates
    /// are interpreted in the local coordinate system of each child.
    /// </remarks>
    /// <param name="x">The x-coordinate, in the parent's local coordinate system, to test for a hit.</param>
    /// <param name="y">The y-coordinate, in the parent's local coordinate system, to test for a hit.</param>
    /// <param name="touchable">
    /// true to consider only actors that are touchable; otherwise, false to include all actors
    /// regardless of their touchable state.</param>
    /// <returns>
    /// The topmost Actor at the specified coordinates that meets the visibility and touchability
    /// criteria; otherwise, null if no such actor is found.
    /// </returns>
    public override Actor? Hit( float x, float y, bool touchable )
    {
        if ( _clip )
        {
            if ( touchable && ( Touchable == Touchable.Disabled ) )
            {
                return null;
            }

            if ( ( x < 0 ) || ( x >= GetWidth() ) || ( y < 0 ) || ( y >= GetHeight() ) )
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

    /// <summary>
    /// Sets the MinWidth value.
    /// </summary>
    /// <param name="minWidth"> The minimum width value to set. </param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetMinWidth( Value minWidth )
    {
        Guard.Against.Null( minWidth );

        _minWidth = minWidth;

        return this;
    }

    /// <summary>
    /// Sets the MinWidth value.
    /// </summary>
    /// <param name="minWidth"> The minimum width value to set. </param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetMinWidth( float minWidth )
    {
        _minWidth = Value.Fixed.ValueOf( minWidth );

        return this;
    }

    /// <summary>
    /// Returns the minimum width of this container.
    /// </summary>
    /// <returns>The minimum width of this container.</returns>
    public override float GetMinWidth()
    {
        return _minWidth.Get( _actor ) + _padLeft.Get( this ) + _padRight.Get( this );
    }

    /// <summary>
    /// Calculates and returns the maximum width of the container, considering the
    /// associated actor's width and any padding applied to the left and right sides
    /// of the container.
    /// </summary>
    /// <returns>
    /// The maximum width of the container, including the associated actor's width and any additional padding.
    /// </returns>
    public override float GetMaxWidth()
    {
        float v = _maxWidth.Get( _actor );

        if ( v > 0 )
        {
            v += _padLeft.Get( this ) + _padRight.Get( this );
        }

        return v;
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

    /// <summary>
    /// Sets the MinHeight value.
    /// </summary>
    /// <param name="minHeight"> The minimum height value to set. </param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetMinHeight( Value minHeight )
    {
        Guard.Against.Null( minHeight );

        _minHeight = minHeight;

        return this;
    }

    /// <summary>
    /// Sets the MinHeight value.
    /// </summary>
    /// <param name="minHeight"> The minimum height value to set. </param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetMinHeight( float minHeight )
    {
        _minHeight = Value.Fixed.ValueOf( minHeight );

        return this;
    }

    /// <summary>
    /// Returns the minimum height of this container.
    /// </summary>
    /// <returns>The minimum height of this container.</returns>
    public override float GetMinHeight()
    {
        return _minHeight.Get( _actor ) + _padTop.Get( this ) + _padBottom.Get( this );
    }

    /// <summary>
    /// Calculates and returns the maximum height of the container, considering the
    /// associated actor's height and any padding applied to the top and bottom
    /// of the container.
    /// </summary>
    /// <returns>
    /// The maximum height of the container, including the associated actor's height and any additional padding.
    /// </returns>
    public override float GetMaxHeight()
    {
        float v = _maxHeight.Get( _actor );

        if ( v > 0 )
        {
            v += _padTop.Get( this ) + _padBottom.Get( this );
        }

        return v;
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

    /// <summary>
    /// Sets the maxWidth to the specified value.
    /// </summary>
    /// <param name="maxWidth"> The new maximum width </param>
    /// <returns> This container for method chaining. </returns>
    public Container< T > SetMaxWidth( Value maxWidth )
    {
        Guard.Against.Null( maxWidth );

        _maxWidth = maxWidth;

        return this;
    }

    /// <summary>
    /// Sets the maxHeight to the specified value.
    /// </summary>
    /// <param name="maxHeight"> The new maximum height </param>
    /// <returns> This container for method chaining. </returns>
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

    /// <summary>
    /// Sets the maxWidth to the specified value.
    /// </summary>
    /// <param name="maxWidth"> The new maximum width </param>
    /// <returns> This container for method chaining. </returns>
    public Container< T > SetMaxWidth( float maxWidth )
    {
        _maxWidth = Value.Fixed.ValueOf( maxWidth );

        return this;
    }

    /// <summary>
    /// Sets the maxHeight to the specified value.
    /// </summary>
    /// <param name="maxHeight"> The new maximum height </param>
    /// <returns> This container for method chaining. </returns>
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

    /// <summary>
    /// Sets the preferred width for the container.
    /// </summary>
    /// <param name="prefWidth">The value representing the preferred width to set. Cannot be null.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPrefWidth( Value prefWidth )
    {
        Guard.Against.Null( prefWidth );

        _prefWidth = prefWidth;

        return this;
    }

    /// <summary>
    /// Sets the preferred width of the container.
    /// </summary>
    /// <param name="prefWidth">The preferred width for the container in pixels.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPrefWidth( float prefWidth )
    {
        _prefWidth = Value.Fixed.ValueOf( prefWidth );

        return this;
    }

    /// <summary>
    /// Sets the preferred height for the container.
    /// </summary>
    /// <param name="prefHeight">The value representing the preferred height to set. Cannot be null.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPrefHeight( Value prefHeight )
    {
        Guard.Against.Null( prefHeight );

        _prefHeight = prefHeight;

        return this;
    }

    /// <summary>
    /// Sets the preferred height of the container.
    /// </summary>
    /// <param name="prefHeight">The preferred height for the container in pixels.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPrefHeight( float prefHeight )
    {
        _prefHeight = Value.Fixed.ValueOf( prefHeight );

        return this;
    }

    /// <summary>
    /// Calculates and returns the preferred width of the container, including adjustments for padding
    /// and consideration of the associated actor and background's minimum width.
    /// </summary>
    /// <returns>The preferred width of the container.</returns>
    public override float GetPrefWidth()
    {
        float v = _prefWidth.Get( _actor );

        if ( _background != null )
        {
            v = Math.Max( v, _background.MinWidth );
        }

        return Math.Max( GetMinWidth(), v + _padLeft.Get( this ) + _padRight.Get( this ) );
    }

    /// <summary>
    /// Calculates and returns the preferred height of the container, including adjustments for padding
    /// and consideration of the associated actor and background's minimum height.
    /// </summary>
    /// <returns>The preferred height of the container.</returns>
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
    /// <param name="pad"> The value to set for all padding sides. Cannot be null.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
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
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
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
    /// <param name="pad"> The value to set for all padding sides. Cannot be null.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPadding( float pad )
    {
        Value value = Value.Fixed.ValueOf( pad );

        _padTop    = value;
        _padLeft   = value;
        _padBottom = value;
        _padRight  = value;

        return this;
    }

    /// <summary>
    /// Sets the padding values for the container.
    /// </summary>
    /// <param name="top">The padding value for the top edge.</param>
    /// <param name="left">The padding value for the left edge.</param>
    /// <param name="bottom">The padding value for the bottom edge.</param>
    /// <param name="right">The padding value for the right edge.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPadding( float top, float left, float bottom, float right )
    {
        _padTop    = Value.Fixed.ValueOf( top );
        _padLeft   = Value.Fixed.ValueOf( left );
        _padBottom = Value.Fixed.ValueOf( bottom );
        _padRight  = Value.Fixed.ValueOf( right );

        return this;
    }

    /// <summary>
    /// Sets the top padding value for the container.
    /// </summary>
    /// <param name="padTop">The padding value for the top edge.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPadTop( Value padTop )
    {
        Guard.Against.Null( padTop );

        _padTop = padTop;

        return this;
    }

    /// <summary>
    /// Sets the left padding value for the container.
    /// </summary>
    /// <param name="padLeft">The padding value for the left edge.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPadLeft( Value padLeft )
    {
        Guard.Against.Null( padLeft );

        _padLeft = padLeft;

        return this;
    }

    /// <summary>
    /// Sets the Bottom padding value for the container.
    /// </summary>
    /// <param name="padBottom">The padding value for the bottom edge.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPadBottom( Value padBottom )
    {
        Guard.Against.Null( padBottom );

        _padBottom = padBottom;

        return this;
    }

    /// <summary>
    /// Sets the right padding value for the container.
    /// </summary>
    /// <param name="padRight">The padding value for the right edge.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPadRight( Value padRight )
    {
        Guard.Against.Null( padRight );

        _padRight = padRight;

        return this;
    }

    /// <summary>
    /// Sets the top padding value for the container.
    /// </summary>
    /// <param name="padTop">The padding value for the top edge.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPadTop( float padTop )
    {
        _padTop = Value.Fixed.ValueOf( padTop );

        return this;
    }

    /// <summary>
    /// Sets the left padding value for the container.
    /// </summary>
    /// <param name="padLeft">The padding value for the left edge.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPadLeft( float padLeft )
    {
        _padLeft = Value.Fixed.ValueOf( padLeft );

        return this;
    }

    /// <summary>
    /// Sets the Bottom padding value for the container.
    /// </summary>
    /// <param name="padBottom">The padding value for the bottom edge.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPadBottom( float padBottom )
    {
        _padBottom = Value.Fixed.ValueOf( padBottom );

        return this;
    }

    /// <summary>
    /// Sets the right padding value for the container.
    /// </summary>
    /// <param name="padRight">The padding value for the right edge.</param>
    /// <returns>The current instance of the container, allowing for method chaining.</returns>
    public Container< T > SetPadRight( float padRight )
    {
        _padRight = Value.Fixed.ValueOf( padRight );

        return this;
    }

    /// <summary>
    /// Gets the padding value for the top edge of the container.
    /// </summary>
    /// <returns>The padding value for the top edge.</returns>
    public float GetPadTop()
    {
        return _padTop.Get( this );
    }

    /// <summary>
    /// Gets the padding value for the left edge of the container.
    /// </summary>
    /// <returns>The padding value for the left edge.</returns>
    public float GetPadLeft()
    {
        return _padLeft.Get( this );
    }

    /// <summary>
    /// Gets the padding value for the bottom edge of the container.
    /// </summary>
    /// <returns>The padding value for the bottom edge.</returns>
    public float GetPadBottom()
    {
        return _padBottom.Get( this );
    }

    /// <summary>
    /// Gets the padding value for the right edge of the container.
    /// </summary>
    /// <returns>The padding value for the right edge.</returns>
    public float GetPadRight()
    {
        return _padRight.Get( this );
    }

    /// <summary>
    /// Calculates and returns the total horizontal padding of the container.
    /// </summary>
    /// <returns>The sum of the left and right padding values.</returns>
    public float GetTotalHorizontalPadding()
    {
        return _padLeft.Get( this ) + _padRight.Get( this );
    }

    /// <summary>
    /// Calculates and returns the total vertical padding of the container.
    /// </summary>
    /// <returns>The sum of the top and bottom padding values.</returns>
    public float GetTotalVerticalPadding()
    {
        return _padTop.Get( this ) + _padBottom.Get( this );
    }

    #endregion padding

    // ========================================================================
    // ========================================================================

    #region alignment

    /// <summary>
    /// Sets the alignment of the actor within the container.
    /// Set to <see cref="Align.Center"/>, <see cref="Align.Top"/>,
    /// <see cref="Align.Bottom"/>, <see cref="Align.Left"/>,
    /// <see cref="Align.Right"/>, or any combination of those.
    /// </summary>
    /// <param name="align">The alignment to set.</param>
    /// <returns>The container instance for method chaining.</returns>
    public Container< T > SetAlignment( Align align )
    {
        _align = align;

        return this;
    }

    /// <summary>
    /// Sets the alignment of the actor within the container to <see cref="Align.Center"/>.
    /// This clears any other alignment.
    /// </summary>
    /// <returns>The container instance for method chaining.</returns>
    public Container< T > AlignCenter()
    {
        _align = Align.Center;

        return this;
    }

    /// <summary>
    /// Sets <see cref="Align.Top"/> and clears <see cref="Align.Bottom"/> for
    /// the alignment of the actor within the container.
    /// </summary>
    /// <returns>The container instance for method chaining.</returns>
    public Container< T > AlignTop()
    {
        _align |= Align.Top;
        _align &= ~Align.Bottom;

        return this;
    }

    /// <summary>
    /// Sets <see cref="Align.Left"/> and clears <see cref="Align.Right"/> for
    /// the alignment of the actor within the container.
    /// </summary>
    /// <returns>The container instance for method chaining.</returns>
    public Container< T > AlignLeft()
    {
        _align |= Align.Left;
        _align &= ~Align.Right;

        return this;
    }

    /// <summary>
    /// Sets <see cref="Align.Bottom"/> and clears <see cref="Align.Top"/> for
    /// the alignment of the actor within the container.
    /// </summary>
    /// <returns>The container instance for method chaining.</returns>
    public Container< T > AlignBottom()
    {
        _align |= Align.Bottom;
        _align &= ~Align.Top;

        return this;
    }

    /// <summary>
    /// Sets <see cref="Align.Right"/> and clears <see cref="Align.Left"/> for the
    /// alignment of the actor within the container.
    /// </summary>
    /// <returns>The container instance for method chaining.</returns>
    public Container< T > AlignRight()
    {
        _align |= Align.Right;
        _align &= ~Align.Left;

        return this;
    }

    /// <summary>
    /// Returns the alignment of the actor within the container.
    /// </summary>
    /// <returns>The alignment.</returns>
    public Align GetAlignment()
    {
        return _align;
    }

    #endregion alignment

    // ========================================================================
    // ========================================================================

    #region drawing

    /// <summary>
    /// Renders the container and its children.
    /// </summary>
    /// <param name="batch">The batch to draw with.</param>
    /// <param name="parentAlpha">The parent alpha value.</param>
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
                               GetWidth() - padLeft - _padRight.Get( this ),
                               GetHeight() - padBottom - _padTop.Get( this )
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
            DrawBackground( batch, parentAlpha, GetX(), GetY() );
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

        batch.SetColor( ActorColor.R, ActorColor.G, ActorColor.B, ActorColor.A * parentAlpha );
        _background.Draw( batch, x, y, GetWidth(), GetHeight() );
    }

    /// <summary>
    /// Renders the debug representation of the container and its children.
    /// </summary>
    /// <param name="shapes">The shape renderer to draw with.</param>
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
                    ? ClipBegin( 0, 0, GetWidth(), GetHeight() )
                    : ClipBegin(
                                padLeft,
                                padBottom,
                                GetWidth() - padLeft - _padRight.Get( this ),
                                GetHeight() - padBottom - _padTop.Get( this )
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

// ============================================================================
// ============================================================================

