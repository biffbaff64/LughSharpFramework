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
using LughSharp.Source.Scene2D.Listeners;
using LughSharp.Source.Scene2D.UI.Styles;
using LughSharp.Source.Scene2D.Utils;

using Rectangle = LughSharp.Source.Maths.Rectangle;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// Represents a pane that splits the available space into two resizable sections.
/// </summary>
[PublicAPI]
public class SplitPane : WidgetGroup, IStyleable< SplitPaneStyle >
{
    protected        bool      CursorOverHandle { get; private set; }
    protected        Rectangle HandleBounds     { get; } = new();
    protected        Vector2   LastPoint        { get; } = new();
    private readonly Vector2   _handlePosition = new();

    // ========================================================================

    private readonly Rectangle _firstWidgetBounds  = new();
    private readonly Rectangle _secondWidgetBounds = new();
    private readonly Rectangle _tempScissors       = new();

    private Actor?          _firstWidget;
    private Actor?          _secondWidget;
    private float           _maxAmount = 1;
    private float           _minAmount;
    private float           _splitAmount = 0.5f;
    private SplitPaneStyle? _style;

    // ========================================================================

    /// <summary>
    /// Creates a new SplitPane with the given first and second widgets.
    /// </summary>
    /// <param name="firstWidget"> The first widget to be placed in the split pane. </param>
    /// <param name="secondWidget"> The second widget to be placed in the split pane. </param>
    /// <param name="isVertical"> Whether the split pane should be split vertically or horizontally. </param>
    /// <param name="skin"> The skin to use for styling the split pane. </param>
    public SplitPane( Actor? firstWidget, Actor? secondWidget, bool isVertical, Skin skin )
        : this( firstWidget,
                secondWidget,
                isVertical,
                skin,
                $"default-{( isVertical ? "vertical" : "horizontal" )}" )
    {
    }

    /// <summary>
    /// Creates a new SplitPane with the given first and second widgets, a flag to
    /// indicate whether the split pane should be split vertically or horizontally,
    /// and a skin to use for styling the split pane.
    /// </summary>
    /// <param name="firstWidget"> The first widget to be placed in the split pane. </param>
    /// <param name="secondWidget"> The second widget to be placed in the split pane. </param>
    /// <param name="isVertical"> Whether the split pane should be split vertically or horizontally. </param>
    /// <param name="skin"> The skin to use for styling the split pane. </param>
    /// <param name="styleName"> The name of the style to use for styling the split pane. </param>
    public SplitPane( Actor? firstWidget, Actor? secondWidget, bool isVertical, Skin skin, string styleName )
        : this( firstWidget,
                secondWidget,
                isVertical,
                skin.Get< SplitPaneStyle >( styleName ) )
    {
    }

    /// <summary>
    /// Creates a new SplitPane with the given first and second widgets, a flag to
    /// indicate whether the split pane should be split vertically or horizontally,
    /// and a <see cref="SplitPaneStyle"/> to use for styling the split pane.
    /// </summary>
    /// <param name="firstWidget"> The first widget to be placed in the split pane. </param>
    /// <param name="secondWidget"> The second widget to be placed in the split pane. </param>
    /// <param name="isVertical"> Whether the split pane should be split vertically or horizontally. </param>
    /// <param name="style"> The <see cref="SplitPaneStyle"/> to use for styling the split pane. </param>
    public SplitPane( Actor? firstWidget, Actor? secondWidget, bool isVertical, SplitPaneStyle style )
    {
        Guard.Against.Null( style );

        Orientation = isVertical;

        SetStyle( style );

        SetFirstWidget( firstWidget );
        SetSecondWidget( secondWidget );

        SetSize( GetPrefWidthUnchecked(), GetPrefHeightUnchecked() );

        AddListener( new SplitPaneInputListener( this ) );
    }

    /// <summary>
    /// Returns the preferred width of this actor.
    /// </summary>
    public override float GetPrefWidth() => GetPrefWidthUnchecked();

    /// <summary>
    /// Returns the preferred width of this actor. This method is non-virtual and is
    /// intended for calling from constructors.
    /// </summary>
    /// <returns>The preferred width of this actor.</returns>
    private float GetPrefWidthUnchecked()
    {
        float first = _firstWidget switch
                      {
                          null           => 0,
                          ILayout widget => widget.GetPrefWidth(),
                          var _          => _firstWidget.GetWidth()
                      };

        float second = _secondWidget switch
                       {
                           null           => 0,
                           ILayout layout => layout.GetPrefWidth(),
                           var _          => _secondWidget.GetWidth()
                       };

        if ( Orientation )
        {
            return Math.Max( first, second );
        }

        float handleMinWidth = _style?.Handle?.MinWidth ?? 0;

        return first + handleMinWidth + second;
    }

    /// <summary>
    /// Returns the preferred height of this actor.
    /// </summary>
    /// <returns>The preferred height of this actor.</returns>
    public override float GetPrefHeight() => GetPrefHeightUnchecked();

    /// <summary>
    /// Returns the preferred height of this actor. This method is non-virtual and is
    /// intended for calling from constructors.
    /// </summary>
    /// <returns>The preferred height of this actor.</returns>
    private float GetPrefHeightUnchecked()
    {
        float first = _firstWidget switch
                      {
                          null           => 0,
                          ILayout widget => widget.GetPrefHeight(),
                          var _          => _firstWidget.GetHeight()
                      };

        float second = _secondWidget switch
                       {
                           null           => 0,
                           ILayout layout => layout.GetPrefHeight(),
                           var _          => _secondWidget.GetHeight()
                       };

        if ( !Orientation )
        {
            return Math.Max( first, second );
        }

        float handleMinHeight = _style?.Handle?.MinHeight ?? 0;

        return first + handleMinHeight + second;
    }

    /// <summary>
    /// Returns the minimum width of this actor.
    /// </summary>
    /// <returns> The minimum width. </returns>
    public override float GetMinWidth()
    {
        float first  = _firstWidget is ILayout layout ? layout.GetMinWidth() : 0;
        float second = _secondWidget is ILayout widget ? widget.GetMinWidth() : 0;

        if ( Orientation )
        {
            return Math.Max( first, second );
        }

        float handleMinWidth = _style?.Handle?.MinWidth ?? 0;

        return first + handleMinWidth + second;
    }

    /// <summary>
    /// Returns the minimum height of this actor.
    /// </summary>
    /// <returns> The minimum height. </returns>
    public override float GetMinHeight()
    {
        float first  = _firstWidget is ILayout layout ? layout.GetMinHeight() : 0;
        float second = _secondWidget is ILayout widget ? widget.GetMinHeight() : 0;

        if ( !Orientation )
        {
            return Math.Max( first, second );
        }

        float handleMinHeight = _style?.Handle?.MinHeight ?? 0;

        return first + handleMinHeight + second;
    }

    /// <summary>
    /// Gets the <see cref="SplitPaneStyle"/> used for styling the split pane.
    /// </summary>
    /// <returns> The <see cref="SplitPaneStyle"/> used for styling the split pane. </returns>
    /// <exception cref="NullReferenceException"> Thrown if the style is null. </exception>
    public SplitPaneStyle GetStyle()
    {
        return _style ?? throw new NullReferenceException( "Style cannot be null." );
    }

    /// <summary>
    /// Sets the <see cref="SplitPaneStyle"/> used for styling the split pane.
    /// </summary>
    /// <param name="value"> The <see cref="SplitPaneStyle"/> used for styling the split pane. </param>
    public void SetStyle( SplitPaneStyle value )
    {
        _style = value;
        InvalidateHierarchy();
    }

    /// <inheritdoc />
    public override void Layout()
    {
        ClampSplitAmount();

        if ( !Orientation )
        {
            CalculateHorizBoundsAndPositions();
        }
        else
        {
            CalculateVertBoundsAndPositions();
        }

        Actor? firstWidget = _firstWidget;

        if ( firstWidget != null )
        {
            firstWidget.SetBounds( _firstWidgetBounds.X,
                                   _firstWidgetBounds.Y,
                                   _firstWidgetBounds.Width,
                                   _firstWidgetBounds.Height );

            if ( firstWidget is ILayout widget )
            {
                widget.Validate();
            }
        }

        Actor? secondWidget = _secondWidget;

        if ( secondWidget != null )
        {
            secondWidget.SetBounds( _secondWidgetBounds.X,
                                    _secondWidgetBounds.Y,
                                    _secondWidgetBounds.Width,
                                    _secondWidgetBounds.Height );

            if ( secondWidget is ILayout widget )
            {
                widget.Validate();
            }
        }
    }

    /// <summary>
    /// The orientation of the split pane.
    /// If TRUE, orientation is vertical; otherwise, orientation is horizontal.
    /// </summary>
    public bool Orientation
    {
        // Returns the orientation of the split pane.
        get;
        // Sets the vertical orientation of the split pane. If the current orientation
        // matches the new orientation, this method does nothing.
        private set
        {
            if ( field == value )
            {
                return;
            }

            field = value;
            InvalidateHierarchy();
        }
    }

    /// <summary>
    /// Calculates the horizontal bounds and positions of the handle, first widget,
    /// and second widget within the split pane based on the current split amount
    /// and styling properties.
    /// </summary>
    private void CalculateHorizBoundsAndPositions()
    {
        Guard.Against.Null( _style );
        Guard.Against.Null( _style.Handle );

        ISceneDrawable handle = _style.Handle;

        float height         = GetHeight();
        float availableWidth = GetWidth() - handle.MinWidth;
        float leftAreaWidth  = availableWidth * _splitAmount;
        float rightAreaWidth = availableWidth - leftAreaWidth;
        float handleWidth    = handle.MinWidth;

        _firstWidgetBounds.Set( 0, 0, leftAreaWidth, height );
        _secondWidgetBounds.Set( leftAreaWidth + handleWidth, 0, rightAreaWidth, height );
        HandleBounds.Set( leftAreaWidth, 0, handleWidth, height );
    }

    /// <summary>
    /// Calculates the vertical bounds and positions of the widgets and handle within the SplitPane.
    /// </summary>
    /// This method determines the layout dimensions for the first and second widgets, as well as the handle,
    /// based on the current split amount and dimensions of the SplitPane. It ensures that all components are
    /// correctly positioned and sized within the pane.
    private void CalculateVertBoundsAndPositions()
    {
        Guard.Against.Null( _style );
        Guard.Against.Null( _style.Handle );

        ISceneDrawable handle = _style.Handle;

        float width            = GetWidth();
        float height           = GetHeight();
        float availHeight      = height - handle.MinHeight;
        float topAreaHeight    = availHeight * _splitAmount;
        float bottomAreaHeight = availHeight - topAreaHeight;
        float handleHeight     = handle.MinHeight;

        _firstWidgetBounds.Set( 0, height - topAreaHeight, width, topAreaHeight );
        _secondWidgetBounds.Set( 0, 0, width, bottomAreaHeight );
        HandleBounds.Set( 0, bottomAreaHeight, width, handleHeight );
    }

    /// <inheritdoc />
    public override void Draw( IBatch batch, float parentAlpha )
    {
        Stage? stage = GetStage();

        if ( stage == null )
        {
            return;
        }

        Validate();

        Color color = ActorColor;
        float alpha = color.A * parentAlpha;

        ApplyTransform( batch, ComputeTransform() );

        if ( _firstWidget is { IsVisible: true } )
        {
            batch.Flush();
            stage.CalculateScissors( _firstWidgetBounds, _tempScissors );

            if ( ScissorStack.PushScissors( _tempScissors ) )
            {
                _firstWidget.Draw( batch, alpha );

                batch.Flush();
                ScissorStack.PopScissors();
            }
        }

        if ( _secondWidget is { IsVisible: true } )
        {
            batch.Flush();
            stage.CalculateScissors( _secondWidgetBounds, _tempScissors );

            if ( ScissorStack.PushScissors( _tempScissors ) )
            {
                _secondWidget.Draw( batch, alpha );

                batch.Flush();
                ScissorStack.PopScissors();
            }
        }

        batch.SetColor( color.R, color.G, color.B, alpha );
        _style?.Handle?.Draw( batch, HandleBounds.X, HandleBounds.Y, HandleBounds.Width, HandleBounds.Height );

        ResetTransform( batch );
    }

    /// <summary>
    /// Sets the split amount for the split pane, determining the relative size of
    /// the first and second widgets.
    /// </summary>
    /// <param name="splitAmount">
    /// The split amount as a floating-point value. This determines the division of
    /// space between the widgets and is clamped during layout to ensure it remains
    /// within the allowable range.
    /// </param>
    public void SetSplitAmount( float splitAmount )
    {
        _splitAmount = splitAmount;
        InvalidateLayout();
    }

    /// <summary>
    /// Gets the split amount for the split pane.
    /// </summary>
    /// <returns> The split amount as a floating-point value. </returns>
    public float GetSplitAmount()
    {
        return _splitAmount;
    }

    /// <summary>
    /// Called during layout to clamp the <see cref="_splitAmount"/> within the set limits.
    /// By default it imposes the limits of the <see cref="GetMinSplitAmount()"/>,
    /// <see cref="GetMaxSplitAmount()"/>, and min sizes of the children.
    /// This method is internally called in response to layout, so it should not call
    /// <see cref="WidgetGroup.InvalidateLayout"/>.
    /// </summary>
    protected void ClampSplitAmount()
    {
        float effectiveMinAmount = _minAmount, effectiveMaxAmount = _maxAmount;

        if ( Orientation )
        {
            float styleHandleMinWidth = _style?.Handle?.MinHeight ?? 0;
            float availableHeight     = GetHeight() - styleHandleMinWidth;

            if ( _firstWidget is ILayout layout )
            {
                effectiveMinAmount = Math.Max( effectiveMinAmount,
                                               Math.Min( layout.GetMinHeight() / availableHeight, 1 ) );
            }

            if ( _secondWidget is ILayout layout2 )
            {
                effectiveMaxAmount = Math.Min( effectiveMaxAmount,
                                               1 - Math.Min( layout2.GetMinHeight() / availableHeight, 1 ) );
            }
        }
        else
        {
            float styleHandleMinHeight = _style?.Handle?.MinHeight ?? 0;
            float availableWidth       = GetWidth() - styleHandleMinHeight;

            if ( _firstWidget is ILayout layout )
            {
                effectiveMinAmount = Math.Max( effectiveMinAmount,
                                               Math.Min( layout.GetMinWidth() / availableWidth, 1 ) );
            }

            if ( _secondWidget is ILayout layout2 )
            {
                effectiveMaxAmount = Math.Min( effectiveMaxAmount,
                                               1 - Math.Min( layout2.GetMinWidth() / availableWidth, 1 ) );
            }
        }

        if ( effectiveMinAmount > effectiveMaxAmount ) // Locked handle. Average the position.
        {
            _splitAmount = 0.5f * ( effectiveMinAmount + effectiveMaxAmount );
        }
        else
        {
            _splitAmount = Math.Max( Math.Min( _splitAmount, effectiveMaxAmount ), effectiveMinAmount );
        }
    }

    /// <summary>
    /// Gets the minimum split amount for the split pane.
    /// </summary>
    /// <returns> The minimum split amount as a floating-point value. </returns>
    public float GetMinSplitAmount()
    {
        return _minAmount;
    }

    /// <summary>
    /// Sets the minimum split amount for the split pane.
    /// </summary>
    /// <param name="minAmount"> The minimum split amount as a floating-point value. </param>
    /// <exception cref="RuntimeException"> Thrown if the minimum split amount is outside the valid range. </exception>
    public void SetMinSplitAmount( float minAmount )
    {
        if ( minAmount is < 0 or > 1 )
        {
            throw new RuntimeException( "minAmount has to be >= 0 and <= 1" );
        }

        _minAmount = minAmount;
    }

    /// <summary>
    /// Gets the maximum split amount for the split pane.
    /// </summary>
    /// <returns> The maximum split amount as a floating-point value. </returns>
    public float GetMaxSplitAmount()
    {
        return _maxAmount;
    }

    /// <summary>
    /// Sets the maximum split amount for the split pane.
    /// </summary>
    /// <param name="maxAmount"> The maximum split amount as a floating-point value. </param>
    /// <exception cref="RuntimeException"> Thrown if the maximum split amount is outside the valid range. </exception>
    public void SetMaxSplitAmount( float maxAmount )
    {
        if ( maxAmount is < 0 or > 1 )
        {
            throw new RuntimeException( "maxAmount has to be >= 0 and <= 1" );
        }

        _maxAmount = maxAmount;
    }

    /// <summary>
    /// Sets the first widget in the SplitPane. If a widget is already set, it will be
    /// removed before the new widget is added.
    /// </summary>
    /// <param name="widget">
    /// The widget to be set as the first widget in the SplitPane. Can be null to clear
    /// the current widget.
    /// </param>
    public void SetFirstWidget( Actor? widget )
    {
        if ( _firstWidget != null )
        {
            base.RemoveActor( _firstWidget, true );
        }

        _firstWidget = widget;

        if ( widget != null )
        {
            AddActor( widget );
        }

        InvalidateLayout();
    }

    /// <summary>
    /// Sets the second widget of the split pane, replacing any existing second widget.
    /// </summary>
    /// <param name="widget">
    /// The widget to set as the second widget. If null, any existing second widget
    /// will be removed.
    /// </param>
    public void SetSecondWidget( Actor? widget )
    {
        if ( _secondWidget != null )
        {
            base.RemoveActor( _secondWidget, true );
        }

        _secondWidget = widget;

        if ( widget != null )
        {
            AddActor( widget );
        }

        InvalidateLayout();
    }

    /// <summary>
    /// Removes the specified actor from the split pane.
    /// </summary>
    /// <param name="actor">The actor to be removed from the split pane.</param>
    /// <returns>Returns true if the removal is successful; otherwise, returns false.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided actor is null.</exception>
    public bool RemoveActor( Actor actor )
    {
        if ( actor == null )
        {
            throw new ArgumentException( "actor cannot be null." );
        }

        if ( actor == _firstWidget )
        {
            SetFirstWidget( null );
        }
        else if ( actor == _secondWidget )
        {
            SetSecondWidget( null );
        }

        return true;
    }

    /// <inheritdoc />
    public override bool RemoveActor( Actor actor, bool unfocus )
    {
        if ( actor == null )
        {
            throw new ArgumentException( "actor cannot be null." );
        }

        if ( actor == _firstWidget )
        {
            base.RemoveActor( actor, unfocus );
            _firstWidget = null;
            InvalidateLayout();

            return true;
        }

        if ( actor == _secondWidget )
        {
            base.RemoveActor( actor, unfocus );
            _secondWidget = null;
            InvalidateLayout();

            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override Actor? RemoveActorAt( int index, bool unfocus )
    {
        Actor? actor = base.RemoveActorAt( index, unfocus );

        if ( actor != null )
        {
            if ( actor == _firstWidget )
            {
                base.RemoveActor( actor, unfocus );
                _firstWidget = null;
                InvalidateLayout();
            }
            else if ( actor == _secondWidget )
            {
                base.RemoveActor( actor, unfocus );
                _secondWidget = null;
                InvalidateLayout();
            }
        }

        return actor;
    }

    /// <summary>
    /// Determines whether the cursor is currently positioned over the handle of the split pane.
    /// </summary>
    /// <returns>True if the cursor is over the handle; otherwise, false.</returns>
    public bool IsCursorOverHandle()
    {
        return CursorOverHandle;
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Processes and handles user input events for a <see cref="SplitPane"/> widget.
    /// Enables interaction with the split bar, allowing users to resize the sections
    /// of the pane by dragging the handle.
    /// </summary>
    [PublicAPI]
    public class SplitPaneInputListener : InputListener
    {
        private readonly SplitPane _parent;
        private          int       _draggingPointer = -1;

        /// <summary>
        /// Processes and handles user input events for a <see cref="SplitPane"/> widget.
        /// Enables interaction with the split bar, allowing users to adjust the size
        /// of the sections by dragging the handle.
        /// </summary>
        /// <param name="parent">The parent <see cref="SplitPane"/> widget.</param>
        public SplitPaneInputListener( SplitPane parent )
        {
            _parent = parent;
        }

        /// <inheritdoc />
        public override bool OnTouchDown( InputEvent? ev, float x, float y, int pointer, int button )
        {
            if ( _draggingPointer != -1 )
            {
                return false;
            }

            if ( ( pointer == 0 ) && ( button != 0 ) )
            {
                return false;
            }

            if ( _parent.HandleBounds.Contains( x, y ) )
            {
                _draggingPointer = pointer;

                _parent.LastPoint.Set( x, y );
                _parent._handlePosition.Set( _parent.HandleBounds.X, _parent.HandleBounds.Y );

                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public override void OnTouchUp( InputEvent? ev, float x, float y, int pointer, int button )
        {
            if ( pointer == _draggingPointer )
            {
                _draggingPointer = -1;
            }
        }

        /// <inheritdoc />
        public override void OnTouchDragged( InputEvent? ev, float x, float y, int pointer )
        {
            if ( pointer != _draggingPointer )
            {
                return;
            }

            ISceneDrawable handle = _parent.GetStyle().Handle
                                 ?? throw new NullReferenceException( "Handle cannot be null." );

            if ( !_parent.Orientation )
            {
                float delta      = x - _parent.LastPoint.X;
                float availWidth = _parent.GetWidth() - handle.MinWidth;
                float dragX      = _parent._handlePosition.X + delta;

                _parent._handlePosition.X = dragX;

                dragX = Math.Max( 0, dragX );
                dragX = Math.Min( availWidth, dragX );

                _parent._splitAmount = dragX / availWidth;
            }
            else
            {
                float delta       = y - _parent.LastPoint.Y;
                float availHeight = _parent.GetHeight() - handle.MinHeight;
                float dragY       = _parent._handlePosition.Y + delta;

                _parent._handlePosition.Y = dragY;

                dragY = Math.Max( 0, dragY );
                dragY = Math.Min( availHeight, dragY );

                _parent._splitAmount = 1 - ( dragY / availHeight );
            }

            _parent.LastPoint.Set( x, y );
            _parent.InvalidateLayout();
        }

        public override bool OnMouseMoved( InputEvent? ev, float x, float y )
        {
            _parent.CursorOverHandle = _parent.HandleBounds.Contains( x, y );

            return false;
        }
    }
}

// ============================================================================
// ============================================================================