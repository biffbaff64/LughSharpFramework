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

using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Maths;
using LughSharp.Core.Scenes.Scene2D.Listeners;
using LughSharp.Core.Scenes.Scene2D.Utils;
using LughSharp.Core.Utils.Exceptions;

using Rectangle = LughSharp.Core.Maths.Rectangle;

namespace LughSharp.Core.Scenes.Scene2D.UI;

[PublicAPI]
public class SplitPane : WidgetGroup
{
    protected bool      CursorOverHandle { get; private set; }
    protected Rectangle HandleBounds     { get; } = new();
    protected Vector2   LastPoint        { get; } = new();
    private readonly Vector2   _handlePosition     = new();

    // ========================================================================

    private readonly Rectangle _firstWidgetBounds  = new();
    private readonly Rectangle _secondWidgetBounds = new();
    private readonly Rectangle _tempScissors       = new();

    private Actor?         _firstWidget;
    private Actor?         _secondWidget;
    private float          _maxAmount = 1;
    private float          _minAmount;
    private float          _splitAmount = 0.5f;
    private bool           _vertical;

    // ========================================================================

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
        _vertical = vertical;
        Style     = style;

        SetFirstWidget( firstWidget );
        SetSecondWidget( secondWidget );

        SetSize( PrefWidth, PrefHeight );

        AddListener( new SplitPaneInputListener( this ) );
    }

    public float PrefWidth
    {
        get
        {
            var first = _firstWidget switch
                        {
                            null           => 0,
                            ILayout widget => widget.GetPrefWidth(),
                            var _          => _firstWidget.Width,
                        };

            var second = _secondWidget switch
                         {
                             null           => 0,
                             ILayout layout => layout.GetPrefWidth(),
                             var _          => _secondWidget.Width,
                         };

            if ( _vertical )
            {
                return Math.Max( first, second );
            }

            return first + Style.Handle.MinWidth + second;
        }
    }

    public float PrefHeight
    {
        get
        {
            var first = _firstWidget switch
                        {
                            null           => 0,
                            ILayout widget => widget.GetPrefHeight(),
                            var _          => _firstWidget.Height,
                        };

            var second = _secondWidget switch
                         {
                             null           => 0,
                             ILayout layout => layout.GetPrefHeight(),
                             var _          => _secondWidget.Height,
                         };

            if ( !_vertical )
            {
                return Math.Max( first, second );
            }

            return first + Style.Handle.MinHeight + second;
        }
    }

    public float MinWidth
    {
        get
        {
            var first  = _firstWidget is ILayout layout ? layout.GetMinWidth() : 0;
            var second = _secondWidget is ILayout widget ? widget.GetMinWidth() : 0;

            if ( _vertical )
            {
                return Math.Max( first, second );
            }

            return first + Style.Handle.MinWidth + second;
        }
    }

    public float MinHeight
    {
        get
        {
            var first  = _firstWidget is ILayout layout ? layout.GetMinHeight() : 0;
            var second = _secondWidget is ILayout widget ? widget.GetMinHeight() : 0;

            if ( !_vertical )
            {
                return Math.Max( first, second );
            }

            return first + Style.Handle.MinHeight + second;
        }
    }

    public SplitPaneStyle Style
    {
        get;
        set
        {
            field = value;
            InvalidateHierarchy();
        }
    }

    public void Layout()
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

        var firstWidget = _firstWidget;

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

        var secondWidget = _secondWidget;

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

    private float GetPrefWidth()
    {
        return PrefWidth;
    }

    private float GetPrefHeight()
    {
        return PrefHeight;
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
        var handle = Style.Handle;

        var height         = Height;
        var availWidth     = Width - handle.MinWidth;
        var leftAreaWidth  = availWidth * _splitAmount;
        var rightAreaWidth = availWidth - leftAreaWidth;
        var handleWidth    = handle.MinWidth;

        _firstWidgetBounds.Set( 0, 0, leftAreaWidth, height );
        _secondWidgetBounds.Set( leftAreaWidth + handleWidth, 0, rightAreaWidth, height );
        HandleBounds.Set( leftAreaWidth, 0, handleWidth, height );
    }

    private void CalculateVertBoundsAndPositions()
    {
        var handle = Style.Handle;

        var width  = Width;
        var height = Height;

        var availHeight      = height - handle.MinHeight;
        var topAreaHeight    = availHeight * _splitAmount;
        var bottomAreaHeight = availHeight - topAreaHeight;
        var handleHeight     = handle.MinHeight;

        _firstWidgetBounds.Set( 0, height - topAreaHeight, width, topAreaHeight );
        _secondWidgetBounds.Set( 0, 0, width, bottomAreaHeight );
        HandleBounds.Set( 0, bottomAreaHeight, width, handleHeight );
    }

    public override void Draw( IBatch batch, float parentAlpha )
    {
        var stage = Stage;

        if ( stage == null )
        {
            return;
        }

        Validate();

        var color = Color;
        var alpha = color.A * parentAlpha;

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
        Style.Handle.Draw( batch, HandleBounds.X, HandleBounds.Y, HandleBounds.Width, HandleBounds.Height );

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
        Invalidate();
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
    /// <see cref="WidgetGroup.Invalidate()"/>.
    /// </summary>
    protected void ClampSplitAmount()
    {
        float effectiveMinAmount = _minAmount, effectiveMaxAmount = _maxAmount;

        if ( _vertical )
        {
            var availableHeight = Height - Style.Handle.MinHeight;

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
            var availableWidth = Width - Style.Handle.MinWidth;

            if ( _firstWidget is ILayout layout )
            {
                effectiveMinAmount =
                    Math.Max( effectiveMinAmount, Math.Min( layout.GetMinWidth() / availableWidth, 1 ) );
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

        Invalidate();
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

        Invalidate();
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
            Invalidate();

            return true;
        }

        if ( actor == _secondWidget )
        {
            base.RemoveActor( actor, unfocus );
            _secondWidget = null;
            Invalidate();

            return true;
        }

        return false;
    }

    public override Actor? RemoveActorAt( int index, bool unfocus )
    {
        var actor = base.RemoveActorAt( index, unfocus );

        if ( actor != null )
        {
            if ( actor == _firstWidget )
            {
                base.RemoveActor( actor, unfocus );
                _firstWidget = null;
                Invalidate();
            }
            else if ( actor == _secondWidget )
            {
                base.RemoveActor( actor, unfocus );
                _secondWidget = null;
                Invalidate();
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

        public override bool TouchDown( InputEvent? ev, float x, float y, int pointer, int button )
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

        public override void TouchUp( InputEvent? ev, float x, float y, int pointer, int button )
        {
            if ( pointer == _draggingPointer )
            {
                _draggingPointer = -1;
            }
        }

        public override void TouchDragged( InputEvent? ev, float x, float y, int pointer )
        {
            if ( pointer != _draggingPointer )
            {
                return;
            }

            var handle = _parent.Style.Handle;

            if ( !_parent._vertical )
            {
                var delta      = x - _parent.LastPoint.X;
                var availWidth = _parent.Width - handle.MinWidth;
                var dragX      = _parent._handlePosition.X + delta;

                _parent._handlePosition.X = dragX;

                dragX = Math.Max( 0, dragX );
                dragX = Math.Min( availWidth, dragX );

                _parent._splitAmount = dragX / availWidth;
            }
            else
            {
                var delta       = y - _parent.LastPoint.Y;
                var availHeight = _parent.Height - handle.MinHeight;
                var dragY       = _parent._handlePosition.Y + delta;

                _parent._handlePosition.Y = dragY;

                dragY = Math.Max( 0, dragY );
                dragY = Math.Min( availHeight, dragY );

                _parent._splitAmount = 1 - ( dragY / availHeight );
            }

            _parent.LastPoint.Set( x, y );

            _parent.Invalidate();
        }

        public override bool MouseMoved( InputEvent? ev, float x, float y )
        {
            _parent.CursorOverHandle = _parent.HandleBounds.Contains( x, y );

            return false;
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class SplitPaneStyle
    {
        public ISceneDrawable Handle { get; }

        public SplitPaneStyle()
        {
            Handle = null!;
        }

        public SplitPaneStyle( ISceneDrawable handle )
        {
            Handle = handle;
        }

        public SplitPaneStyle( SplitPaneStyle style )
        {
            Handle = style.Handle;
        }
    }
}

// ============================================================================
// ============================================================================

