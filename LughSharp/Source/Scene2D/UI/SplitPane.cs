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

[PublicAPI]
public class SplitPane : WidgetGroup
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
    private bool            _vertical;
    private SplitPaneStyle? _style;

    // ========================================================================

    /// <summary>
    /// Creates a new SplitPane with the given first and second widgets.
    /// </summary>
    /// <param name="firstWidget"></param>
    /// <param name="secondWidget"></param>
    /// <param name="vertical"></param>
    /// <param name="skin"></param>
    public SplitPane( Actor? firstWidget, Actor? secondWidget, bool vertical, Skin skin )
        : this( firstWidget,
                secondWidget,
                vertical,
                skin,
                $"default-{( vertical ? "vertical" : "horizontal" )}" )
    {
    }

    public SplitPane( Actor? firstWidget, Actor? secondWidget, bool vertical, Skin skin, string styleName )
        : this( firstWidget,
                secondWidget,
                vertical,
                skin.Get< SplitPaneStyle >( styleName ) )
    {
    }

    public SplitPane( Actor? firstWidget, Actor? secondWidget, bool vertical, SplitPaneStyle style )
    {
        Guard.Against.Null( style );

        _vertical = vertical;

        SetStyle( style );

        SetFirstWidget( firstWidget );
        SetSecondWidget( secondWidget );

        SetSize( GetPrefWidthUnchecked(), GetPrefHeightUnchecked() );

        AddListener( new SplitPaneInputListener( this ) );
    }

    private float GetPrefWidthUnchecked() => GetPrefWidth();

    private float GetPrefHeightUnchecked() => GetPrefHeight();

    /// <summary>
    /// Returns the preferred width of this actor.
    /// </summary>
    public override float GetPrefWidth()
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

        if ( _vertical )
        {
            return Math.Max( first, second );
        }

        float handleMinWidth = _style?.Handle?.MinWidth ?? 0;

        return first + handleMinWidth + second;
    }

    /// <summary>
    /// Returns the preferred height of this actor.
    /// </summary>
    /// <returns></returns>
    public override float GetPrefHeight()
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

        if ( !_vertical )
        {
            return Math.Max( first, second );
        }

        float handleMinHeight = _style?.Handle?.MinHeight ?? 0;

        return first + handleMinHeight + second;
    }

    /// <summary>
    /// Returns the minimum width of this actor.
    /// </summary>
    /// <returns></returns>
    public override float GetMinWidth()
    {
        float first  = _firstWidget is ILayout layout ? layout.GetMinWidth() : 0;
        float second = _secondWidget is ILayout widget ? widget.GetMinWidth() : 0;

        if ( _vertical )
        {
            return Math.Max( first, second );
        }

        float handleMinWidth = _style?.Handle?.MinWidth ?? 0;

        return first + handleMinWidth + second;
    }

    /// <summary>
    /// Returns the minimum height of this actor.
    /// </summary>
    /// <returns></returns>
    public override float GetMinHeight()
    {
        float first  = _firstWidget is ILayout layout ? layout.GetMinHeight() : 0;
        float second = _secondWidget is ILayout widget ? widget.GetMinHeight() : 0;

        if ( !_vertical )
        {
            return Math.Max( first, second );
        }

        float handleMinHeight = _style?.Handle?.MinHeight ?? 0;

        return first + handleMinHeight + second;
    }

    public SplitPaneStyle GetStyle()
    {
        return _style ?? throw new NullReferenceException( "Style cannot be null." );
    }

    public void SetStyle( SplitPaneStyle value )
    {
        _style = value;
        InvalidateHierarchy();
    }

    /// <summary>
    /// Positions and sizes children of the table using the cell associated with each child.
    /// The values given are the position within the parent and size of the table.
    /// </summary>
    public override void Layout()
    {
        ClampSplitAmount();

        if ( !_vertical )
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

    public void SetVertical( bool vertical )
    {
        if ( _vertical == vertical )
        {
            return;
        }

        _vertical = vertical;
        InvalidateHierarchy();
    }

    public bool IsVertical()
    {
        return _vertical;
    }

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

    public override void Draw( IBatch batch, float parentAlpha )
    {
        Stage? stage = Stage;

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
    /// </summary>
    /// <param name="splitAmount">
    /// The split amount between the min and max amount. This parameter is clamped during layout.
    /// </param>
    public void SetSplitAmount( float splitAmount )
    {
        _splitAmount = splitAmount;
        InvalidateLayout();
    }

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

        if ( _vertical )
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

    public float GetMinSplitAmount()
    {
        return _minAmount;
    }

    public void SetMinSplitAmount( float minAmount )
    {
        if ( minAmount is < 0 or > 1 )
        {
            throw new RuntimeException( "minAmount has to be >= 0 and <= 1" );
        }

        _minAmount = minAmount;
    }

    public float GetMaxSplitAmount()
    {
        return _maxAmount;
    }

    public void SetMaxSplitAmount( float maxAmount )
    {
        if ( maxAmount is < 0 or > 1 )
        {
            throw new RuntimeException( "maxAmount has to be >= 0 and <= 1" );
        }

        _maxAmount = maxAmount;
    }

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

    public bool IsCursorOverHandle()
    {
        return CursorOverHandle;
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class SplitPaneInputListener : InputListener
    {
        private readonly SplitPane _parent;
        private          int       _draggingPointer = -1;

        public SplitPaneInputListener( SplitPane parent )
        {
            _parent = parent;
        }

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

        public override void OnTouchUp( InputEvent? ev, float x, float y, int pointer, int button )
        {
            if ( pointer == _draggingPointer )
            {
                _draggingPointer = -1;
            }
        }

        public override void OnTouchDragged( InputEvent? ev, float x, float y, int pointer )
        {
            if ( pointer != _draggingPointer )
            {
                return;
            }

            ISceneDrawable handle = _parent.GetStyle().Handle
                                 ?? throw new NullReferenceException( "Handle cannot be null." );

            if ( !_parent._vertical )
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

    // ========================================================================
    // ========================================================================
}

// ============================================================================
// ============================================================================