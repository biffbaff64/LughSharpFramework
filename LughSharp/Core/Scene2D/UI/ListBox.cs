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

using LughSharp.Core.Collections;
using LughSharp.Core.Graphics.Fonts;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Scene2D.Listeners;
using LughSharp.Core.Scene2D.UI.Styles;
using LughSharp.Core.Scene2D.Utils;
using LughSharp.Core.Utils.Pooling;

using Rectangle = LughSharp.Core.Maths.Rectangle;

namespace LughSharp.Core.Scene2D.UI;

/// <summary>
/// A list box displays textual items and highlights the currently selected item.
/// <para>
/// <see cref="ChangeListener.ChangeEvent"/> is fired when the list selection changes.
/// </para>
/// <para>
/// The preferred size of the list is determined by the text bounds of the items
/// and the size of the <see cref="Selection{T}"/>.
/// </para>
/// </summary>
[PublicAPI]
[ActorDefinition( Role = "UI" )]
public class ListBox< T > : Widget where T : notnull
{
    public Rectangle?          CullingArea  { get; set; }
    public InputListener?      KeyListener  { get; set; }
    public ArraySelection< T > Selection    { get; set; } = null!;
    public List< T >           Items        { get; set; } = [ ];
    public float               ItemHeight   { get; set; }
    public Align               Alignment    { get; set; } = Align.Left;
    public bool                TypeToSelect { get; set; }

    /// <summary>
    /// Returns the list's style. Modifying the returned style may not have an
    /// effect until <see cref="SetStyle(ListBoxStyle)"/> is called.
    /// </summary>
    public ListBoxStyle Style { get; set; } = null!;

    // ========================================================================

    private int   _overIndex    = -1;
    private int   _pressedIndex = -1;
    private float _prefHeight;
    private float _prefWidth;

    // ========================================================================

    /// <summary>
    /// Creates a new ListBox, using the supplied <see cref="Skin"/>.
    /// The <see cref="ListBoxStyle"/> embedded in the Skin will be used.
    /// </summary>
    /// <param name="skin"> The Skin to use. </param>
    public ListBox( Skin skin )
        : this( skin.Get< ListBoxStyle >() )
    {
    }

    /// <summary>
    /// Creates a new ListBox, using the supplied <see cref="Skin"/>. The
    /// <see cref="ListBoxStyle"/> to use will be extracted from the supplied
    /// skin using the name provided.
    /// </summary>
    /// <param name="skin"> The Skin to use. </param>
    /// <param name="styleName"> The name of the ListStyle to extract from the Skin. </param>
    public ListBox( Skin skin, string styleName )
        : this( skin.Get< ListBoxStyle >( styleName ) )
    {
    }

    /// <summary>
    /// Creates a new ListBox, using the supplied <see cref="ListBoxStyle"/>
    /// </summary>
    /// <param name="boxStyle"> The ListStyle to use. </param>
    public ListBox( ListBoxStyle boxStyle )
    {
        Create( boxStyle );
    }

    public override float GetPrefWidth()
    {
        Validate();

        return _prefWidth;
    }

    public void SetPrefWidth( float value )
    {
        _prefWidth = value;
    }

    public override float GetPrefHeight()
    {
        Validate();

        return _prefHeight;
    }

    public void SetPrefHeight( float value )
    {
        _prefHeight = value;
    }

    private void Create( ListBoxStyle boxStyle )
    {
        Selection = new ArraySelection< T >( Items )
        {
            Actor    = this,
            Required = true
        };

        SetStyle( boxStyle );
        SetSize( GetPrefWidth(), GetPrefHeight() );

        KeyListener = new ListKeyListener( this );

        AddListener( KeyListener );
        AddListener( new ListInputListener( this ) );
    }

    public void SetStyle( ListBoxStyle boxStyle )
    {
        Style = boxStyle ?? throw new ArgumentException( "style cannot be null." );

        InvalidateHierarchy();
    }

    /// <summary>
    /// Computes and caches any information needed for drawing and, if this actor has
    /// children, positions and sizes each child, calls <see cref="ILayout.Invalidate"/>
    /// on any each child whose width or height has changed, and calls <see cref="ILayout.Validate"/>
    /// on each child. This method should almost never be called directly, instead
    /// <see cref="ILayout.Validate"/> should be used.
    /// </summary>
    public override void Layout()
    {
        BitmapFont?     font             = Style.Font;
        ISceneDrawable? selectedDrawable = Style.Selection;

        if ( font == null )
        {
            throw new RuntimeException( "Layout: supplied style has a null font!" );
        }

        if ( selectedDrawable == null )
        {
            throw new RuntimeException( "Layout: supplied style has a null selected drawable!" );
        }

        ItemHeight =  font.GetCapHeight() - ( font.GetDescent() * 2 );
        ItemHeight += selectedDrawable.TopHeight + selectedDrawable.BottomHeight;

        _prefWidth = 0;

        Pool< GlyphLayout > layoutPool = Pools.Get< GlyphLayout >( () => new GlyphLayout() );
        GlyphLayout         layout     = layoutPool.Obtain();

        foreach ( T item in Items )
        {
            layout.SetText( font, ToString( item ) );
            _prefWidth = Math.Max( layout.Width, _prefWidth );
        }

        layoutPool.Free( layout );
        _prefWidth  += selectedDrawable.LeftWidth + selectedDrawable.RightWidth;
        _prefHeight =  Items.Count * ItemHeight;

        ISceneDrawable? background = Style.Background;

        if ( background != null )
        {
            _prefWidth = Math.Max( _prefWidth + background.LeftWidth + background.RightWidth, background.MinWidth );
            _prefHeight = Math.Max( _prefHeight + background.TopHeight + background.BottomHeight,
                                    background.MinHeight );
        }
    }

    /// <inheritdoc />
    public override void Draw( IBatch batch, float parentAlpha )
    {
        Validate();

        DrawBackground( batch, parentAlpha );

        BitmapFont      font                = Style.Font;
        ISceneDrawable? selectedDrawable    = Style.Selection;
        Color           fontColorSelected   = Style.FontColorSelected;
        Color           fontColorUnselected = Style.FontColorUnselected;

        batch.SetColor( ActorColor.R, ActorColor.G, ActorColor.B, ActorColor.A * parentAlpha );

        float x      = X;
        float y      = Y;
        float width  = Width;
        float height = Height;
        float itemY  = height;

        ISceneDrawable? background = Style.Background;

        if ( background != null )
        {
            float leftWidth = background.LeftWidth;

            x     += leftWidth;
            itemY -= background.TopHeight;
            width -= leftWidth + background.RightWidth;
        }

        float? textOffsetX = selectedDrawable?.LeftWidth;
        float? textWidth   = width - textOffsetX - selectedDrawable?.RightWidth;
        float  textOffsetY = selectedDrawable!.TopHeight - font.GetDescent();

        font.SetColor( fontColorUnselected.R,
                       fontColorUnselected.G,
                       fontColorUnselected.B,
                       fontColorUnselected.A * parentAlpha );

        for ( var i = 0; i < Items.Count; i++ )
        {
            if ( ( CullingArea == null )
              || ( ( ( itemY - ItemHeight ) <= ( CullingArea.Y + CullingArea.Height ) )
                && ( itemY >= CullingArea.Y ) ) )
            {
                T               item     = Items[ i ];
                bool            selected = Selection.Contains( item );
                ISceneDrawable? drawable = null;

                if ( ( _pressedIndex == i ) && ( Style?.Down != null ) )
                {
                    drawable = Style.Down;
                }
                else if ( selected )
                {
                    drawable = selectedDrawable;
                    font.SetColor( fontColorSelected.R,
                                   fontColorSelected.G,
                                   fontColorSelected.B,
                                   fontColorSelected.A * parentAlpha );
                }
                else if ( ( _overIndex == i ) && ( Style?.Over != null ) )
                {
                    drawable = Style.Over;
                }

                drawable?.Draw( batch, x, y + itemY - ItemHeight, width, ItemHeight );

                DrawItem( batch,
                          font,
                          i,
                          item,
                          ( float )( x + textOffsetX )!,
                          y + itemY - textOffsetY,
                          ( float )textWidth! );

                if ( selected )
                {
                    font.SetColor( fontColorUnselected.R,
                                   fontColorUnselected.G,
                                   fontColorUnselected.B,
                                   fontColorUnselected.A * parentAlpha );
                }
            }
            else if ( itemY < CullingArea.Y )
            {
                break;
            }

            itemY -= ItemHeight;
        }
    }

    /// <summary>
    /// Called to draw the background. Default implementation draws the style background drawable.
    /// </summary>
    protected void DrawBackground( IBatch batch, float parentAlpha )
    {
        if ( Style.Background != null )
        {
            batch.SetColor( ActorColor.R, ActorColor.G, ActorColor.B, ActorColor.A * parentAlpha );

            Style.Background.Draw( batch, X, Y, Width, Height );
        }
    }

    protected GlyphLayout DrawItem( IBatch batch, BitmapFont font, int index, T item, float x, float y, float width )
    {
        string str = ToString( item );

        return font.Draw( batch, str, x, y, 0, str.Length, width, Alignment, false, "..." );
    }

    /// <summary>
    /// Returns the first selected item, or null.
    /// </summary>
    public T? GetSelected()
    {
        return Selection.First();
    }

    /// <summary>
    /// Sets the selection to only the passed item, if it is a possible choice.
    /// </summary>
    /// <param name="item"> May be null. </param>
    public void SetSelected( T item )
    {
        if ( Items.Contains( item ) )
        {
            Selection.Set( item );
        }
        else if ( Selection.Required && ( Items.Count > 0 ) )
        {
            Selection.Set( Items.First() );
        }
        else
        {
            Selection.Clear();
        }
    }

    /// <summary>
    /// Returns the index of the first selected item. The top item has an index of 0.
    /// Nothing selected has an index of -1.
    /// </summary>
    public int GetSelectedIndex()
    {
        List< T > selected = Selection.ToArray();

        return selected.Count == 0 ? -1 : Items.IndexOf( selected.First() );
    }

    /// <summary>
    /// Sets the selection to only the selected index.
    /// </summary>
    /// <param name="index"> -1 to clear the selection. </param>
    public void SetSelectedIndex( int index )
    {
        if ( ( index < -1 ) || ( index >= Items.Count ) )
        {
            throw new ArgumentException( $"index must be >= -1 and < {Items.Count}: {index}" );
        }

        if ( index == -1 )
        {
            Selection.Clear();
        }
        else
        {
            Selection.Set( Items[ index ] );
        }
    }

    public T? GetOverItem()
    {
        return _overIndex == -1 ? default( T? ) : Items[ _overIndex ];
    }

    public T? GetPressedItem()
    {
        return _pressedIndex == -1 ? default( T? ) : Items[ _pressedIndex ];
    }

    public T? GetItemAt( float y )
    {
        int index = GetItemIndexAt( y );

        return index == -1 ? default( T? ) : Items[ index ];
    }

    /// <summary>
    /// </summary>
    /// <returns> -1 if not over an item. </returns>
    public int GetItemIndexAt( float y )
    {
        float           height     = Height;
        ISceneDrawable? background = Style.Background;

        if ( background != null )
        {
            height -= background.TopHeight + background.BottomHeight;
            y      -= background.BottomHeight;
        }

        var index = ( int )( ( height - y ) / ItemHeight );

        if ( ( index < 0 ) || ( index >= Items.Count ) )
        {
            return -1;
        }

        return index;
    }

    public void SetItems( params T[] newItems )
    {
        Guard.Against.Null( newItems );

        float oldPrefWidth  = GetPrefWidth();
        float oldPrefHeight = GetPrefHeight();

        Items.Clear();
        Items.AddAll( newItems );

        _overIndex    = -1;
        _pressedIndex = -1;

        Selection.Validate();

        Invalidate();

        if ( !oldPrefWidth.Equals( GetPrefWidth() ) || !oldPrefHeight.Equals( GetPrefHeight() ) )
        {
            InvalidateHierarchy();
        }
    }

    /// <summary>
    /// Sets the items visible in the list, clearing the selection if it is no longer
    /// valid. If a selection is <see cref="ArraySelection{T}.Required()"/>, the first
    /// item is selected. This can safely be called with a (modified) array returned
    /// from <see cref="Items"/>
    /// </summary>
    public void SetItems( List< T > newItems )
    {
        Guard.Against.Null( newItems );

        float oldPrefWidth  = GetPrefWidth();
        float oldPrefHeight = GetPrefHeight();

        if ( !newItems.Equals( Items ) )
        {
            Items.Clear();
            Items.AddAll( newItems );
        }

        _overIndex    = -1;
        _pressedIndex = -1;
        Selection.Validate();

        Invalidate();

        if ( !oldPrefWidth.Equals( GetPrefWidth() ) || !oldPrefHeight.Equals( GetPrefHeight() ) )
        {
            InvalidateHierarchy();
        }
    }

    public void ClearItems()
    {
        if ( Items.Count == 0 )
        {
            return;
        }

        Items.Clear();

        _overIndex    = -1;
        _pressedIndex = -1;

        Selection.Clear();

        InvalidateHierarchy();
    }

    public string ToString( T? obj )
    {
        return obj?.ToString() ?? string.Empty;
    }

    // ========================================================================
    // ========================================================================

    internal class ListKeyListener : InputListener
    {
        private readonly ListBox< T > _parent;
        private          string       _prefix;
        private          long         _typeTimeout;

        public ListKeyListener( ListBox< T > lb )
        {
            _prefix = string.Empty;
            _parent = lb;
        }

        /// <summary>
        /// Called when a key goes down. When true is returned, the event is
        /// handled by <see cref="Event.SetHandled"/>.
        /// </summary>
        public override bool OnKeyDown( InputEvent? ev, int keycode )
        {
            if ( _parent.Items.Count == 0 )
            {
                return false;
            }

            int index;

            switch ( keycode )
            {
                case IInput.Keys.A:
                    if ( InputUtils.CtrlKey() && _parent.Selection.Multiple )
                    {
                        _parent.Selection.Clear();
                        _parent.Selection.AddAll( _parent.Items );

                        return true;
                    }

                    break;

                case IInput.Keys.Home:
                    _parent.SetSelectedIndex( 0 );

                    return true;

                case IInput.Keys.End:
                    _parent.SetSelectedIndex( _parent.Items.Count - 1 );

                    return true;

                case IInput.Keys.Down:
                    index = _parent.Items.IndexOf( _parent.GetSelected()! ) + 1;

                    if ( index >= _parent.Items.Count )
                    {
                        index = 0;
                    }

                    _parent.SetSelectedIndex( index );

                    return true;

                case IInput.Keys.Up:
                    index = _parent.Items.IndexOf( _parent.GetSelected()! ) - 1;

                    if ( index < 0 )
                    {
                        index = _parent.Items.Count - 1;
                    }

                    _parent.SetSelectedIndex( index );

                    return true;

                case IInput.Keys.Escape:
                    if ( _parent.Stage != null )
                    {
                        _parent.Stage.SetKeyboardFocus( null );
                    }

                    return true;
            }

            return false;
        }

        /// <summary>
        /// Called when a key is typed. When true is returned, the event is
        /// handled by <see cref="Event.SetHandled"/>.
        /// </summary>
        /// <param name="ev"> The input event. </param>
        /// <param name="character">
        /// May be 0 for key typed events that don't map to a character (ctrl, shift, etc).
        /// </param>
        public override bool OnKeyTyped( InputEvent? ev, char character )
        {
            if ( !_parent.TypeToSelect )
            {
                return false;
            }

            long time = TimeUtils.Millis();

            if ( time > _typeTimeout )
            {
                _prefix = "";
            }

            _typeTimeout =  time + 300;
            _prefix      += char.ToLower( character );

            for ( int i = 0, n = _parent.Items.Count; i < n; i++ )
            {
                if ( _parent.ToString( _parent.Items[ i ] ).ToLower().StartsWith( _prefix ) )
                {
                    _parent.SetSelectedIndex( i );

                    break;
                }
            }

            return false;
        }
    }

    // ========================================================================
    // ========================================================================

    public class ListInputListener : InputListener
    {
        private readonly ListBox< T > _parent;

        public ListInputListener( ListBox< T > lb )
        {
            _parent = lb;
        }

        /// <summary>
        /// Called when a mouse button or a finger touch goes down on the actor.
        /// If true is returned, this listener will have
        /// <see cref="Stage.AddTouchFocus(IEventListener, Actor, Actor, int, int)"/>,
        /// so it will receive all touchDragged and touchUp events, even those not
        /// over this actor, until touchUp is received. Also when true is returned,
        /// the event is handled by <see cref="Event.SetHandled"/>.
        /// </summary>
        public override bool OnTouchDown( InputEvent? ev, float x, float y, int pointer, int button )
        {
            if ( ( pointer != 0 ) || ( button != 0 ) )
            {
                return true;
            }

            if ( _parent.Selection.IsDisabled )
            {
                return true;
            }

            if ( _parent.Stage != null )
            {
                _parent.Stage.SetKeyboardFocus( _parent );
            }

            if ( _parent.Items.Count == 0 )
            {
                return true;
            }

            int index = _parent.GetItemIndexAt( y );

            if ( index == -1 )
            {
                return true;
            }

            _parent.Selection.Choose( _parent.Items[ index ] );
            _parent._pressedIndex = index;

            return true;
        }

        /// <summary>
        /// Called when a mouse button or a finger touch goes up anywhere, but only
        /// if touchDown previously returned true for the mouse button or touch.
        /// The touchUp event is always handled by <see cref="Event.SetHandled"/>.
        /// </summary>
        public override void OnTouchUp( InputEvent? ev, float x, float y, int pointer, int button )
        {
            if ( ( pointer != 0 ) || ( button != 0 ) )
            {
                return;
            }

            _parent._pressedIndex = -1;
        }

        /// <summary>
        /// Called when a mouse button or a finger touch is moved anywhere, but only
        /// if touchDown previously returned true for the mouse button or touch.
        /// The touchDragged event is always handled by <see cref="Event.SetHandled"/>.
        /// </summary>
        public override void OnTouchDragged( InputEvent? ev, float x, float y, int pointer )
        {
            _parent._overIndex = _parent.GetItemIndexAt( y );
        }

        /// <summary>
        /// Called any time the mouse is moved when a button is not down. This event
        /// only occurs on the desktop. When true is returned, the event is handled
        /// by <see cref="Event.SetHandled"/>.
        /// </summary>
        public override bool OnMouseMoved( InputEvent? ev, float x, float y )
        {
            _parent._overIndex = _parent.GetItemIndexAt( y );

            return false;
        }

        /// <summary>
        /// Called any time the mouse cursor or a finger touch is moved out of an actor.
        /// On the desktop, this event occurs even when no mouse buttons are pressed
        /// (pointer will be -1).
        /// </summary>
        /// <param name="ev"> The input event. </param>
        /// <param name="x"> The x coordinate of the mouse cursor or touch. </param>
        /// <param name="y"> The y coordinate of the mouse cursor or touch. </param>
        /// <param name="pointer"> The pointer index of the mouse cursor or touch. </param>
        /// <param name="toActor"> The actor that the mouse cursor or touch is exiting. </param>
        public override void Exit( InputEvent? ev, float x, float y, int pointer, Actor? toActor )
        {
            if ( pointer == 0 )
            {
                _parent._pressedIndex = -1;
            }

            if ( pointer == -1 )
            {
                _parent._overIndex = -1;
            }
        }
    }
}

// ============================================================================
// ============================================================================