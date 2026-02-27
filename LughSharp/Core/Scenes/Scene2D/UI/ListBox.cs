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

using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Input;
using LughSharp.Core.Scenes.Scene2D.Listeners;
using LughSharp.Core.Scenes.Scene2D.Styles;
using LughSharp.Core.Scenes.Scene2D.Utils;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Pooling;

using Color = LughSharp.Core.Graphics.Color;
using Rectangle = LughSharp.Core.Maths.Rectangle;

namespace LughSharp.Core.Scenes.Scene2D.UI;

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
public class ListBox< T > : Widget, IStyleable< ListBox< T >.ListBoxStyle > where T : notnull
{
    public Rectangle?          CullingArea  { get; set; }
    public InputListener?      KeyListener  { get; set; }
    public ArraySelection< T > Selection    { get; set; } = null!;
    public List< T >           Items        { get; set; } = [ ];
    public float               ItemHeight   { get; set; }
    public int                 Alignment    { get; set; } = Align.LEFT;
    public bool                TypeToSelect { get; set; }

    // ========================================================================

    public override string? Name => "ListBox";

    // ========================================================================

    private int   _overIndex = -1;
    private float _prefHeight;
    private float _prefWidth;
    private int   _pressedIndex = -1;

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

    /// <summary>
    /// Returns the list's style. Modifying the returned style may not have an
    /// effect until <see cref="SetStyle(ListBoxStyle)"/> is called.
    /// </summary>
    public ListBoxStyle? Style { get; set; }

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

    public override void SetLayout()
    {
        BitmapFont?     font             = Style?.Font;
        ISceneDrawable? selectedDrawable = Style?.Selection;

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

        ISceneDrawable? background = Style?.Background;

        if ( background != null )
        {
            _prefWidth = Math.Max( _prefWidth + background.LeftWidth + background.RightWidth, background.MinWidth );
            _prefHeight = Math.Max( _prefHeight + background.TopHeight + background.BottomHeight,
                                    background.MinHeight );
        }
    }

    public override void Draw( IBatch batch, float parentAlpha )
    {
        Validate();

        DrawBackground( batch, parentAlpha );

        BitmapFont?     font                = Style?.Font;
        ISceneDrawable? selectedDrawable    = Style?.Selection;
        Color?          fontColorSelected   = Style?.FontColorSelected;
        Color?          fontColorUnselected = Style?.FontColorUnselected;

        batch.SetColor( Color.R, Color.G, Color.B, Color.A * parentAlpha );

        float x      = X;
        float y      = Y;
        float width  = Width;
        float height = Height;
        float itemY  = height;

        ISceneDrawable? background = Style?.Background;

        if ( background != null )
        {
            float leftWidth = background.LeftWidth;

            x     += leftWidth;
            itemY -= background.TopHeight;
            width -= leftWidth + background.RightWidth;
        }

        float? textOffsetX = selectedDrawable?.LeftWidth;
        float? textWidth   = width - textOffsetX - selectedDrawable?.RightWidth;
        float  textOffsetY = selectedDrawable!.TopHeight - font!.GetDescent();

        font.SetColor( fontColorUnselected!.R,
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
                    font.SetColor( fontColorSelected!.R,
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
        if ( Style?.Background != null )
        {
            batch.SetColor( Color.R, Color.G, Color.B, Color.A * parentAlpha );

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
        ISceneDrawable? background = Style?.Background;

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
    /// Sets the items visible in the list, clearing the selection if it is no longer valid. If a
    /// selection is <see cref="ArraySelection{T}.Required()"/>, the first item is selected. This
    /// can safely be called with a (modified) array returned from <see cref="Items"/>
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

        public override bool KeyDown( InputEvent? ev, int keycode )
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

                case IInput.Keys.HOME:
                    _parent.SetSelectedIndex( 0 );

                    return true;

                case IInput.Keys.END:
                    _parent.SetSelectedIndex( _parent.Items.Count - 1 );

                    return true;

                case IInput.Keys.DOWN:
                    index = _parent.Items.IndexOf( _parent.GetSelected()! ) + 1;

                    if ( index >= _parent.Items.Count )
                    {
                        index = 0;
                    }

                    _parent.SetSelectedIndex( index );

                    return true;

                case IInput.Keys.UP:
                    index = _parent.Items.IndexOf( _parent.GetSelected()! ) - 1;

                    if ( index < 0 )
                    {
                        index = _parent.Items.Count - 1;
                    }

                    _parent.SetSelectedIndex( index );

                    return true;

                case IInput.Keys.ESCAPE:
                    if ( _parent.Stage != null )
                    {
                        _parent.Stage.KeyboardFocus = null;
                    }

                    return true;
            }

            return false;
        }

        public override bool KeyTyped( InputEvent? ev, char character )
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

        public override bool TouchDown( InputEvent? ev, float x, float y, int pointer, int button )
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
                _parent.Stage.KeyboardFocus = _parent;
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

        public override void TouchUp( InputEvent? ev, float x, float y, int pointer, int button )
        {
            if ( ( pointer != 0 ) || ( button != 0 ) )
            {
                return;
            }

            _parent._pressedIndex = -1;
        }

        public override void TouchDragged( InputEvent? ev, float x, float y, int pointer )
        {
            _parent._overIndex = _parent.GetItemIndexAt( y );
        }

        public override bool MouseMoved( InputEvent? ev, float x, float y )
        {
            _parent._overIndex = _parent.GetItemIndexAt( y );

            return false;
        }

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

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// The style for a list, see <see cref="ListBox{T}"/>.
    /// </summary>
    [PublicAPI]
    public class ListBoxStyle
    {
        public ListBoxStyle()
        {
            Font = new BitmapFont();
        }

        public ListBoxStyle( BitmapFont font, Color fontColorSelected, Color fontColorUnselected,
                             ISceneDrawable selection )
        {
            Font = font;
            FontColorSelected.Set( fontColorSelected );
            FontColorUnselected.Set( fontColorUnselected );
            Selection = selection;
        }

        public ListBoxStyle( ListBoxStyle boxStyle )
        {
            Font = boxStyle.Font;
            FontColorSelected.Set( boxStyle.FontColorSelected );
            FontColorUnselected.Set( boxStyle.FontColorUnselected );
            Selection = boxStyle.Selection;

            Down       = boxStyle.Down;
            Over       = boxStyle.Over;
            Background = boxStyle.Background;
        }

        public BitmapFont      Font                { get; set; }
        public Color           FontColorSelected   { get; set; } = new( 1, 1, 1, 1 );
        public Color           FontColorUnselected { get; set; } = new( 1, 1, 1, 1 );
        public ISceneDrawable? Selection           { get; set; }
        public ISceneDrawable? Down                { get; set; }
        public ISceneDrawable? Over                { get; set; }
        public ISceneDrawable? Background          { get; set; }
    }
}