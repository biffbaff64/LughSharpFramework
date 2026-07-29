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

using LughSharp.Source.Graphics.Fonts;
using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Scene2D.Listeners;
using LughSharp.Source.Scene2D.UI.Styles;
using LughSharp.Source.Scene2D.Utils;
using LughSharp.Source.Utils.Pooling;

using Rectangle = LughSharp.Source.Maths.Rectangle;

namespace LughSharp.Source.Scene2D.UI;

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
public class ListBox< T > : Widget, IStyleable< ListBoxStyle > where T : notnull
{
    public Rectangle?          CullingArea  { get; set; }
    public InputListener       KeyListener  { get; set; }
    public ArraySelection< T > Selection    { get; set; }
    public List< T >           Items        { get; set; } = [ ];
    public float               ItemHeight   { get; set; }
    public Align               Alignment    { get; set; } = Align.Left;
    public bool                TypeToSelect { get; set; }

    // ========================================================================

    private int          _overIndex    = -1;
    private int          _pressedIndex = -1;
    private ListBoxStyle _style        = null!;
    private float        _prefHeight;
    private float        _prefWidth;

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
        Selection = new ArraySelection< T >( Items )
        {
            Actor    = this,
            Required = true
        };

        SetStyle( boxStyle );
        SetSize( GetPrefWidthUnchecked(), GetPrefHeightUnchecked() );

        KeyListener = new ListKeyListener( this );

        AddListener( KeyListener );
        AddListener( new ListInputListener( this ) );
    }

    /// <summary>
    /// Returns the list's style. Modifying the returned style may not have an
    /// effect until <see cref="SetStyle(ListBoxStyle)"/> is called.
    /// </summary>
    public ListBoxStyle GetStyle() => _style;

    /// <summary>
    /// Sets the style of the list box to the specified <see cref="ListBoxStyle"/>.
    /// This style determines the visual appearance of the list box, including colors,
    /// fonts, and other visual properties.
    /// </summary>
    /// <param name="boxStyle">
    /// The <see cref="ListBoxStyle"/> to apply to the list box. Cannot be null.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided <paramref name="boxStyle"/> is null.
    /// </exception>
    public void SetStyle( ListBoxStyle boxStyle )
    {
        _style = boxStyle ?? throw new ArgumentException( "style cannot be null." );

        InvalidateHierarchy();
    }

    /// <summary>
    /// Computes and caches any information needed for drawing and, if this actor has
    /// children, positions and sizes each child, calls <see cref="ILayout.InvalidateLayout"/>
    /// on any each child whose width or height has changed, and calls <see cref="ILayout.Validate"/>
    /// on each child. This method should almost never be called directly, instead
    /// <see cref="ILayout.Validate"/> should be used.
    /// </summary>
    public override void Layout()
    {
        BitmapFont?     font             = _style.Font;
        ISceneDrawable? selectedDrawable = _style.Selection;

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

        Pool< GlyphLayout > layoutPool = PoolsMap.Get< GlyphLayout >( () => new GlyphLayout() );
        GlyphLayout         layout     = layoutPool.Obtain();

        foreach ( T item in Items )
        {
            layout.SetText( font, ToString( item ) );
            _prefWidth = Math.Max( layout.Width, _prefWidth );
        }

        layoutPool.Free( layout );
        _prefWidth  += selectedDrawable.LeftWidth + selectedDrawable.RightWidth;
        _prefHeight =  Items.Count * ItemHeight;

        ISceneDrawable? background = _style.Background;

        if ( background != null )
        {
            _prefWidth = Math.Max( _prefWidth + background.LeftWidth + background.RightWidth, background.MinWidth );
            _prefHeight = Math.Max( _prefHeight + background.TopHeight + background.BottomHeight,
                                    background.MinHeight );
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

        DrawBackground( batch, parentAlpha );

        BitmapFont      font                = _style.Font;
        ISceneDrawable? selectedDrawable    = _style.Selection;
        Color           fontColorSelected   = _style.FontColorSelected;
        Color           fontColorUnselected = _style.FontColorUnselected;

        batch.SetColor( ActorColor.R, ActorColor.G, ActorColor.B, ActorColor.A * parentAlpha );

        float x      = GetX();
        float y      = GetY();
        float width  = GetWidth();
        float height = GetHeight();
        float itemY  = height;

        ISceneDrawable? background = _style.Background;

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

                if ( ( _pressedIndex == i ) && ( _style?.Down != null ) )
                {
                    drawable = _style.Down;
                }
                else if ( selected )
                {
                    drawable = selectedDrawable;
                    font.SetColor( fontColorSelected.R,
                                   fontColorSelected.G,
                                   fontColorSelected.B,
                                   fontColorSelected.A * parentAlpha );
                }
                else if ( ( _overIndex == i ) && ( _style?.Over != null ) )
                {
                    drawable = _style.Over;
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
        if ( _style.Background != null )
        {
            batch.SetColor( ActorColor.R, ActorColor.G, ActorColor.B, ActorColor.A * parentAlpha );

            _style.Background.Draw( batch, GetX(), GetY(), GetWidth(), GetHeight() );
        }
    }

    /// <summary>
    /// Draws an item in the ListBox at the specified position and size using the provided
    /// font and render batch.
    /// </summary>
    /// <param name="batch">The rendering batch used to draw the item.</param>
    /// <param name="font">The font used to render the text representation of the item.</param>
    /// <param name="index">The index of the item in the list.</param>
    /// <param name="item">The item to be drawn.</param>
    /// <param name="x">The x-coordinate where the item should be drawn.</param>
    /// <param name="y">The y-coordinate where the item should be drawn.</param>
    /// <param name="width">The width available for rendering the item.</param>
    /// <returns>
    /// A <see cref="GlyphLayout"/> object that describes the resulting layout of the rendered item.
    /// </returns>
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

    /// <summary>
    /// Retrieves the item that is currently under the cursor or highlighted.
    /// If no item is under the cursor, returns the default value of the item type.
    /// </summary>
    /// <returns>
    /// The item currently under the cursor, or the default value of the item type
    /// if no item is highlighted.
    /// </returns>
    public T? GetOverItem()
    {
        return _overIndex == -1 ? default( T? ) : Items[ _overIndex ];
    }

    /// <summary>
    /// Retrieves the item currently pressed in the list box, or null if no item is pressed.
    /// </summary>
    /// <returns>
    /// The pressed item of type <typeparamref name="T"/>, or null if no item is pressed.
    /// </returns>
    public T? GetPressedItem()
    {
        return _pressedIndex == -1 ? default( T? ) : Items[ _pressedIndex ];
    }

    /// <summary>
    /// Retrieves the item at the specified vertical position within the list.
    /// </summary>
    /// <param name="y">The vertical position to check for an item, relative to the list's origin.</param>
    /// <returns>
    /// The item located at the specified vertical position, or <see langword="null"/> if no item is found.
    /// </returns>
    public T? GetItemAt( float y )
    {
        int index = GetItemIndexAt( y );

        return index == -1 ? default( T? ) : Items[ index ];
    }

    /// <summary>
    /// Returns the index of the item at the passed y coordinate. The top item has an index of 0.
    /// </summary>
    /// <param name="y">The vertical position to check for an item, relative to the list's origin.</param>
    /// <returns>The index of the item at the specified vertical position, or -1 if no item is found.</returns>
    public int GetItemIndexAt( float y )
    {
        float           height     = GetHeight();
        ISceneDrawable? background = _style.Background;

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

    /// <summary>
    /// Sets the items for the ListBox, replacing any existing items.
    /// </summary>
    /// <param name="newItems">The new items to populate the ListBox with.</param>
    public void SetItems( params T[] newItems )
    {
        Guard.Against.Null( newItems );

        float oldPrefWidth  = GetPrefWidth();
        float oldPrefHeight = GetPrefHeight();

        Items.Clear();
        Items.AddRange( newItems );

        _overIndex    = -1;
        _pressedIndex = -1;

        Selection.Validate();

        InvalidateLayout();

        if ( !oldPrefWidth.Equals( GetPrefWidth() ) || !oldPrefHeight.Equals( GetPrefHeight() ) )
        {
            InvalidateHierarchy();
        }
    }

    /// <summary>
    /// Retrieves the preferred width of the ListBox without applying any external constraints
    /// or recomputation. The value is used internally to determine the ListBox's optimal size.
    /// </summary>
    /// <returns>The preferred width of the ListBox.</returns>
    private float GetPrefWidthUnchecked()
    {
        Validate();

        return _prefWidth;
    }

    /// <summary>
    /// Returns the preferred height of the ListBox without applying layout validation.
    /// This value is determined based on internal state and may not reflect
    /// changes made to the layout or configuration since the last validation call.
    /// </summary>
    /// <returns>The preferred height of the ListBox.</returns>
    private float GetPrefHeightUnchecked()
    {
        Validate();

        return _prefHeight;
    }

    /// <summary>
    /// Calculates and returns the preferred width of the widget. This value is used
    /// during layout to determine the optimal size for the widget while adhering to
    /// its layout constraints and content requirements. Subclasses may override this
    /// method to provide a specific calculation based on their individual behavior and content.
    /// </summary>
    /// <returns>
    /// The preferred width of the widget, expressed as a floating-point value.
    /// </returns>
    public override float GetPrefWidth()
    {
        return GetPrefWidthUnchecked();   
    }

    /// <summary>
    /// Sets the preferred width of the list box.
    /// </summary>
    /// <param name="value">The value to set as the preferred width.</param>
    public void SetPrefWidth( float value )
    {
        _prefWidth = value;
    }

    /// <summary>
    /// Returns the preferred height of the widget. This value is typically used to determine
    /// the desired height of the widget for layout purposes. The value may vary depending
    /// on the widget's contents, styling, or other attributes.
    /// </summary>
    /// <returns>The preferred height of the widget in pixels.</returns>
    public override float GetPrefHeight()
    {
        return GetPrefHeightUnchecked();
    }

    /// <summary>
    /// Sets the preferred height for the list box.
    /// </summary>
    /// <param name="value">The preferred height in pixels.</param>
    public void SetPrefHeight( float value )
    {
        _prefHeight = value;
    }

    /// <summary>
    /// Sets the items visible in the list, clearing the selection if it is no longer
    /// valid. If a selection is <see cref="ArraySelection{T}.Required()"/>, the first
    /// item is selected. This can safely be called with a (modified) array returned
    /// from <see cref="Items"/>
    /// </summary>
    public void SetItems( List< T > newItems )
    {
        float oldPrefWidth  = GetPrefWidth();
        float oldPrefHeight = GetPrefHeight();

        Items.Clear();
        Items.AddRange( newItems );

        _overIndex    = -1;
        _pressedIndex = -1;
        Selection.Validate();

        InvalidateLayout();

        if ( Math.Abs( oldPrefWidth - GetPrefWidth() ) > NumberUtils.FloatTolerance
          || Math.Abs( oldPrefHeight - GetPrefHeight() ) > NumberUtils.FloatTolerance )
        {
            InvalidateHierarchy();
        }
    }

    /// <summary>
    /// Clears all items from the <see cref="ListBox{T}"/>. This includes resetting the indices for
    /// hover and pressed items, clearing the selection, and invalidating the widget's hierarchy
    /// to ensure proper layout updates.
    /// </summary>
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

    /// <summary>
    /// Returns a string representation of the specified object. If the object is null,
    /// an empty string is returned.
    /// </summary>
    /// <param name="obj">The object to convert to a string.</param>
    /// <returns>A string representation of the object.</returns>
    public string ToString( T? obj )
    {
        return obj?.ToString() ?? string.Empty;
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// A listener that handles key input events for a <see cref="ListBox{T}"/>.
    /// <para>
    /// This listener manages navigation and selection within a list using keyboard input.
    /// It supports various key actions, such as moving up and down the list, navigating
    /// to the beginning or end, and performing selection operations.
    /// </para>
    /// <para>
    /// The behavior of key events is influenced by the list's selection mode and the
    /// current state of the modifier keys.
    /// </para>
    /// </summary>
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
                    var parentStage = _parent.GetStage();
                    
                    if ( parentStage != null )
                    {
                        parentStage.SetKeyboardFocus( null );
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
                _prefix = string.Empty;
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

    /// <summary>
    /// A listener specifically designed to handle input events for a <see cref="ListBox{T}"/>.
    /// <para>
    /// The <see cref="ListInputListener"/> enables handling of touch, mouse, and gesture events
    /// to support interaction with the items in the list. It determines the item index based on user input
    /// and updates the selection accordingly.
    /// </para>
    /// </summary>
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

            if ( _parent.GetStage() != null )
            {
                _parent.GetStage()?.SetKeyboardFocus( _parent );
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