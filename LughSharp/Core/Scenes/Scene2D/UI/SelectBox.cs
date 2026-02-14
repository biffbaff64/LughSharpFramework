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

using System;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Input;
using LughSharp.Core.Maths;
using LughSharp.Core.Scenes.Scene2D.Listeners;
using LughSharp.Core.Scenes.Scene2D.Utils;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Pooling;

using Color = LughSharp.Core.Graphics.Color;

namespace LughSharp.Core.Scenes.Scene2D.UI;

/// <summary>
/// A select box (aka a drop-down list) allows a user to choose one of a number of values
/// from a list. When inactive, the selected value is displayed. When activated, it shows
/// the list of values that may be selected.
/// <para>
/// <see cref="ChangeListener.ChangeEvent"/> is fired when the selectbox selection changes.
/// </para>
/// <para>
/// The preferred size of the select box is determined by the maximum text bounds of the items and the size of the
/// {@link SelectBoxStyle#background}.
/// </para>
/// </summary>
[PublicAPI]
public class SelectBox< T > : Widget, IDisableable where T : notnull
{
    public ClickListener  ClickListener { get; set; }
    public SelectBoxStyle Style         { get; set; } = null!;
    public List< T >      Items         { get; }      = [ ];
    public bool           IsDisabled    { get; set; }
    public float          PrefWidth     { get; private set; }
    public float          PrefHeight    { get; private set; }

    // ========================================================================

    private readonly SelectBoxScrollPane? _scrollPane;
    private readonly ArraySelection< T >  _selection;
    private readonly Vector2              _temp = new();

    private int  _alignment = Align.LEFT;
    private bool _selectedPrefWidth;

    // ========================================================================

    /// <summary>
    /// Creates a select box with a <see cref="SelectBoxStyle"/> from the provided skin,
    /// </summary>
    /// <param name="skin"></param>
    public SelectBox( Skin skin ) : this( skin.Get< SelectBoxStyle >() )
    {
    }

    /// <summary>
    /// Creates a select box with a <see cref="SelectBoxStyle"/> from the provided skin and style name,
    /// </summary>
    /// <param name="skin"></param>
    /// <param name="styleName"></param>
    public SelectBox( Skin skin, string styleName )
        : this( skin.Get< SelectBoxStyle >( styleName ) )
    {
    }

    /// <summary>
    /// Creates a select box with the provided style.
    /// </summary>
    /// <param name="style"></param>
    public SelectBox( SelectBoxStyle style )
    {
        SetStyle( style );
        SetSize( PrefWidth, PrefHeight );

        _selection = new SelectBoxArraySelection< T >( this, Items )
        {
            Actor    = this,
            Required = true,
        };

        _scrollPane   = new SelectBoxScrollPane( this );
        ClickListener = new SelectBoxClickListener( this );
    }

    /// <summary>
    /// Set the max number of items to display when the select box is opened. Set to
    /// 0 (the default) to display as many as fit in the stage height.
    /// </summary>
    public int MaxListCount
    {
        get => _scrollPane!.MaxListCount;
        set => _scrollPane!.MaxListCount = value;
    }

    /// <summary>
    /// Sets the stage this select box should be added to. If null, the selectbox
    /// will be hidden.
    /// </summary>
    /// <param name="stage"></param>
    protected void SetStage( Stage? stage )
    {
        if ( stage == null )
        {
            _scrollPane?.Hide();
        }

        Stage = stage;
    }

    /// <summary>
    /// Sets the <see cref="SelectBoxStyle"/> to use for this select box.
    /// </summary>
    /// <param name="style"></param>
    public void SetStyle( SelectBoxStyle style )
    {
        Guard.Against.Null( style );

        Style = style;

        if ( _scrollPane != null )
        {
            _scrollPane.Style         = style.ScrollStyle;
            _scrollPane.ListBox.Style = style.ListStyle;
        }

        InvalidateHierarchy();
    }

    /// <summary>
    /// Set the backing List that makes up the choices available in the SelectBox
    /// </summary>
    public void SetItems( params T[] newItems )
    {
        Guard.Against.Null( newItems );

        var oldPrefWidth = PrefWidth;

        Items.Clear();
        Items.AddAll( newItems );
        _selection.Validate();
        _scrollPane?.ListBox.SetItems( Items );

        Invalidate();

        if ( oldPrefWidth.Equals( PrefWidth ) )
        {
            InvalidateHierarchy();
        }
    }

    /// <summary>
    /// Sets the items visible in the select box.
    /// </summary>
    public void SetItems( List< T > newItems )
    {
        Guard.Against.Null( newItems );

        var oldPrefWidth = PrefWidth;

        if ( newItems != Items )
        {
            Items.Clear();
            Items.AddAll( newItems );
        }

        _selection.Validate();
        _scrollPane?.ListBox.SetItems( Items );

        Invalidate();

        if ( oldPrefWidth.Equals( PrefWidth ) )
        {
            InvalidateHierarchy();
        }
    }

    /// <summary>
    /// Clears the items from the select box.
    /// </summary>
    public void ClearItems()
    {
        if ( Items.Count == 0 )
        {
            return;
        }

        Items.Clear();
        _selection.Clear();
        InvalidateHierarchy();
    }

    public void Layout()
    {
        if ( Style == null )
        {
            throw new RuntimeException( "SelectBoxStyle cannot be null." );
        }

        var bg   = Style.Background;
        var font = Style.Font;

        if ( bg != null )
        {
            PrefHeight = Math.Max( ( bg.TopHeight + bg.BottomHeight + font.GetCapHeight() )
                                 - ( font.GetDescent() * 2 ),
                                   bg.MinHeight );
        }
        else
        {
            PrefHeight = font.GetCapHeight() - ( font.GetDescent() * 2 );
        }

        var layoutPool = Pools.Get< GlyphLayout >( () => new GlyphLayout() );
        var layout     = layoutPool.Obtain();

        if ( _selectedPrefWidth )
        {
            PrefWidth = 0;

            if ( bg != null )
            {
                PrefWidth = bg.LeftWidth + bg.RightWidth;
            }

            var selected = GetSelected();

            if ( selected != null )
            {
                layout.SetText( font, ToString( selected ) ?? "" );
                PrefWidth += layout.Width;
            }
        }
        else
        {
            float maxItemWidth = 0;

            foreach ( var t in Items )
            {
                layout.SetText( font, ToString( t ) ?? "" );
                maxItemWidth = Math.Max( layout.Width, maxItemWidth );
            }

            PrefWidth = maxItemWidth;

            if ( bg != null )
            {
                PrefWidth = Math.Max( PrefWidth + bg.LeftWidth + bg.RightWidth, bg.MinWidth );
            }

            var listStyle   = Style.ListStyle;
            var scrollStyle = Style.ScrollStyle;

            var listWidth = maxItemWidth + listStyle.Selection?.LeftWidth + listStyle.Selection?.RightWidth;

            bg = scrollStyle.Background;

            if ( bg != null )
            {
                listWidth = Math.Max( ( float )( listWidth + bg.LeftWidth + bg.RightWidth )!, bg.MinWidth );
            }

            if ( _scrollPane is not { DisableYScroll: true } )
            {
                listWidth += Math.Max( Style.ScrollStyle.VScroll?.MinWidth ?? 0,
                                       Style.ScrollStyle.VScrollKnob?.MinWidth ?? 0 );
            }

            PrefWidth = Math.Max( PrefWidth, ( float )listWidth! );
        }

        layoutPool.Free( layout );
    }

    /// <summary>
    /// Returns appropriate background Drawable from the style
    /// based on the current select box state.
    /// </summary>
    protected ISceneDrawable? GetBackgroundIDrawable()
    {
        if ( IsDisabled && ( Style.BackgroundDisabled != null ) )
        {
            return Style.BackgroundDisabled;
        }

        if ( _scrollPane!.HasParent() && ( Style.BackgroundOpen != null ) )
        {
            return Style.BackgroundOpen;
        }

        if ( IsOver() && ( Style.BackgroundOver != null ) )
        {
            return Style.BackgroundOver;
        }

        return Style.Background;
    }

    /// <summary>
    /// Returns the appropriate label font color from the style based on the current button state.
    /// </summary>
    protected Color GetFontColor()
    {
        if ( IsDisabled && ( Style.DisabledFontColor != null ) )
        {
            return Style.DisabledFontColor;
        }

        if ( ( Style.OverFontColor != null ) && ( IsOver() || _scrollPane!.HasParent() ) )
        {
            return Style.OverFontColor;
        }

        return Style.FontColor;
    }

    public override void Draw( IBatch batch, float parentAlpha )
    {
        Validate();

        var background = GetBackgroundIDrawable();
        var fontColor  = GetFontColor();
        var font       = Style.Font;

        // Make copies of x,y,width and height for local modification
        // and preserving the originals.
        var x      = X;
        var y      = Y;
        var width  = Width;
        var height = Height;

        batch.SetColor( Color.R, Color.G, Color.B, Color.A * parentAlpha );

        background?.Draw( batch, x, y, width, height );

        var selected = _selection.First();

        if ( selected != null )
        {
            if ( background != null )
            {
                width  -= background.LeftWidth + background.RightWidth;
                height -= background.BottomHeight + background.TopHeight;
                x      += background.LeftWidth;
                y      += ( int )( ( height / 2 ) + background.BottomHeight + ( font.FontData.CapHeight / 2 ) );
            }
            else
            {
                y += ( int )( ( height / 2 ) + ( font.FontData.CapHeight / 2 ) );
            }

            font.SetColor( fontColor.R, fontColor.G, fontColor.B, fontColor.A * parentAlpha );

            DrawItem( batch, font, selected, x, y, width );
        }
    }

    protected GlyphLayout DrawItem( IBatch batch, BitmapFont font, T item, float x, float y, float width )
    {
        var str = ToString( item ) ?? "";

        return font.Draw( batch, str, x, y, 0, str.Length, width, _alignment, false, "..." );
    }

    /// <summary>
    /// Sets the alignment of the selected item in the select box. See <see cref="GetList()"/>
    /// and <see cref="SetAlignment(int)"/> to set the alignment in the list shown when the
    /// select box is open.
    /// </summary>
    /// <param name="alignment"> See <see cref="Align"/>. </param>
    public void SetAlignment( int alignment )
    {
        //TODO: Property?
        _alignment = alignment;
    }

    /// <summary>
    /// Get the set of selected items, useful when multiple items are selected
    /// </summary>
    /// <returns> a Selection object containing the selected elements </returns>
    public ArraySelection< T > GetSelection()
    {
        //TODO: Property?
        return _selection;
    }

    /// <summary>
    /// Returns the first selected item, or null. For multiple selections
    /// use <see cref="GetSelection()"/>.
    /// </summary>
    public T? GetSelected()
    {
        return _selection.First();
    }

    /// <summary>
    /// Sets the selection to only the passed item, if it is a possible
    /// choice, else selects the first item.
    /// </summary>
    public void SetSelected( T item )
    {
        if ( Items.Contains( item ) )
        {
            _selection.Set( item );
        }
        else if ( Items.Count > 0 )
        {
            _selection.Set( Items.First() );
        }
        else
        {
            _selection.Clear();
        }
    }

    /// <returns>
    /// The index of the first selected item. The top item has an index of 0.
    /// Nothing selected has an index of -1.
    /// </returns>
    public int GetSelectedIndex()
    {
        var selected = _selection.Items();

        return selected.Count == 0 ? -1 : Items.IndexOf( selected.First() );
    }

    /// <summary>
    /// Sets the selection to only the selected index.
    /// </summary>
    public void SetSelectedIndex( int index )
    {
        _selection.Set( Items[ index ] );
    }

    /// <summary>
    /// When true the pref width is based on the selected item.
    /// </summary>
    public void SetSelectedPrefWidth( bool selectedPrefWidth )
    {
        _selectedPrefWidth = selectedPrefWidth;
    }

    /// <summary>
    /// Returns the pref width of the select box if the widest item was selected,
    /// for use when <see cref="SetSelectedPrefWidth(bool)"/> is true.
    /// </summary>
    public float GetMaxSelectedPrefWidth()
    {
        var   layoutPool = Pools.Get< GlyphLayout >( () => new GlyphLayout() );
        var   layout     = layoutPool.Obtain();
        float width      = 0;

        foreach ( var t in Items )
        {
            layout.SetText( Style.Font, ToString( t ) ?? "" );
            width = Math.Max( layout.Width, width );
        }

        var bg = Style.Background;

        if ( bg != null )
        {
            width = Math.Max( width + bg.LeftWidth + bg.RightWidth, bg.MinWidth );
        }

        return width;
    }

    public void SetDisabled( bool disabled )
    {
        if ( disabled && !IsDisabled )
        {
            HideScrollPane();
        }

        IsDisabled = disabled;
    }

    /// <summary>
    /// Shows the ScrollPane containing the list of items shown
    /// when the select box is open.
    /// </summary>
    public void ShowScrollPane()
    {
        if ( Items.Count == 0 )
        {
            return;
        }

        if ( Stage != null )
        {
            _scrollPane?.Show( Stage );
        }
    }

    /// <summary>
    /// Hides the ScrollPane containing the list of items shown.
    /// </summary>
    public void HideScrollPane()
    {
        _scrollPane?.Hide();
    }

    /// <summary>
    /// Returns the list shown when the select box is open.
    /// </summary>
    public ListBox< T >? GetList()
    {
        return _scrollPane?.ListBox;
    }

    /// <summary>
    /// Disables scrolling of the list shown when the select box is open.
    /// </summary>
    public void SetScrollingDisabled( bool state )
    {
        _scrollPane?.SetScrollingDisabled( true, state );

        InvalidateHierarchy();
    }

    /// <summary>
    /// Returns the scroll pane containing the list that is shown
    /// when the select box is open.
    /// </summary>
    public SelectBoxScrollPane? GetScrollPane()
    {
        return _scrollPane;
    }

    /// <summary>
    /// Returns true if the mouse or touch is over the actor or pressed
    /// and within the tap square.
    /// </summary>
    public bool IsOver()
    {
        return ClickListener.Over;
    }

    /// <summary>
    /// Scene2D Actions to perform when the specified select box is first shown.
    /// </summary>
    /// <param name="selectBox"></param>
    protected void OnShow( Actor selectBox )
    {
        selectBox.Color.A = 0;
        selectBox.AddAction( Scene2D.Actions.Actions.FadeIn( 0.3f, Interpolation.Fade ) );
    }

    /// <summary>
    /// Scene2D Actions to perform when the specified select box is hidden.
    /// </summary>
    /// <param name="selectBox"></param>
    protected void OnHide( Actor selectBox )
    {
        selectBox.Color.A = 1f;

        var action1  = Scene2D.Actions.Actions.FadeOut( 0.15f, Interpolation.Fade );
        var action2  = Scene2D.Actions.Actions.RemoveActor();
        var sequence = Scene2D.Actions.Actions.Sequence( action1, action2 );

        selectBox.AddAction( sequence );
    }

    protected string? ToString( T? item )
    {
        return item?.ToString();
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    [PublicAPI]
    public class SelectBoxScrollPane : ScrollPane
    {
        public int            MaxListCount { get; set; }
        public ListBox< T >   ListBox      { get; set; }
        public SelectBox< T > SelectBox    { get; set; }

        // ====================================================================

        private readonly InputListener _hideListener;
        private readonly Vector2       _stagePosition = new();
        private          Actor?        _previousScrollFocus;

        // ====================================================================

        public SelectBoxScrollPane( SelectBox< T > selectBox )
            : base( null, selectBox.Style.ScrollStyle )
        {
            SelectBox = selectBox;

            SetOverscroll( false, false );
            SetFadeScrollBars( false );
            SetScrollingDisabled( true, false );

            ListBox = new ListBox< T >( SelectBox.Style.ListStyle )
            {
                Touchable    = Touchable.Disabled,
                TypeToSelect = true,
            };

            SetActor( ListBox );

            _hideListener = new SelectBoxScrollPaneHideListener( this );

            AddListener( new SelectBoxScrollPaneClickListener( this ) );
            AddListener( new SelectBoxScrollPaneInputListener( this ) );
        }

        public void Show( Stage stage )
        {
            if ( ListBox.IsTouchable() )
            {
                return;
            }

            stage.AddActor( this );
            stage.AddCaptureListener( _hideListener );

            stage.AddListener( ListBox.KeyListener
                            ?? throw new RuntimeException( "No ListBox KeyListener available!" ) );

            SelectBox.LocalToStageCoordinates( _stagePosition.Set( 0, 0 ) );

            // Show the list above or below the select box, limited to a number
            // of items and the available height in the stage.
            var itemHeight = ListBox.ItemHeight;

            var height = itemHeight * ( MaxListCount <= 0
                ? SelectBox.Items.Count
                : Math.Min( MaxListCount, SelectBox.Items.Count ) );

            var scrollPaneBackground = Style.Background;

            if ( scrollPaneBackground != null )
            {
                height += scrollPaneBackground.TopHeight + scrollPaneBackground.BottomHeight;
            }

            var listBackground = ListBox.Style?.Background;

            if ( listBackground != null )
            {
                height += listBackground.TopHeight + listBackground.BottomHeight;
            }

            var heightBelow = _stagePosition.Y;
            var heightAbove = stage.Height - heightBelow - SelectBox.Height;
            var below       = true;

            if ( height > heightBelow )
            {
                if ( heightAbove > heightBelow )
                {
                    below  = false;
                    height = Math.Min( height, heightAbove );
                }
                else
                {
                    height = heightBelow;
                }
            }

            if ( below )
            {
                Y = _stagePosition.Y - height;
            }
            else
            {
                Y = _stagePosition.Y + SelectBox.Height;
            }

            Height = height;
            Validate();

            var width = Math.Max( SelectBox.PrefWidth, SelectBox.Width );
            Width = width;

            var x = _stagePosition.X;

            if ( x + width > stage.Width )
            {
                x -= width - SelectBox.Width - 1;
            }

            X = x;
            
            Validate();

            ScrollTo( 0,
                      ListBox.Height - ( SelectBox.GetSelectedIndex() * itemHeight ) - ( itemHeight / 2 ),
                      0,
                      0,
                      true,
                      true );

            UpdateVisualScroll();

            _previousScrollFocus = null;

            var actor = stage.ScrollFocus;

            if ( ( actor != null ) && !actor.IsDescendantOf( this ) )
            {
                _previousScrollFocus = actor;
            }

            stage.ScrollFocus = this;

            ListBox.Selection.Set( SelectBox.GetSelected() );
            ListBox.Touchable = Touchable.Enabled;

            ClearActions();

            SelectBox.OnShow( this );
        }

        public void Hide()
        {
            if ( !ListBox.IsTouchable() || !HasParent() )
            {
                return;
            }

            ListBox.Touchable = Touchable.Disabled;

            if ( Stage != null )
            {
                Stage.RemoveCaptureListener( _hideListener );
                Stage.RemoveListener( ListBox.KeyListener! );

                if ( _previousScrollFocus is { Stage: null } )
                {
                    _previousScrollFocus = null;
                }

                if ( ( Stage.ScrollFocus == null ) || IsAscendantOf( Stage.ScrollFocus ) )
                {
                    Stage.ScrollFocus = _previousScrollFocus;
                }
            }

            ClearActions();
            SelectBox.OnHide( this );
        }

        public override void Draw( IBatch batch, float parentAlpha )
        {
            SelectBox.LocalToStageCoordinates( SelectBox._temp.Set( 0, 0 ) );

            if ( !SelectBox._temp.Equals( _stagePosition ) )
            {
                Hide();
            }

            base.Draw( batch, parentAlpha );
        }

        public override void Act( float delta )
        {
            base.Act( delta );
            ToFront();
        }

        public override void SetStage( Stage? stage )
        {
            if ( stage != null )
            {
                stage.RemoveCaptureListener( _hideListener );
                stage.RemoveListener( ListBox.KeyListener! );
            }

            Stage = stage;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Represents a selection functionality specific to the <see cref="SelectBox{T}"/> widget,
    /// where the selections are backed by an array or list of items.
    /// This class extends the <see cref="ArraySelection{T}"/> to provide custom selection behavior
    /// that integrates seamlessly with the <see cref="SelectBox{T}"/> component.
    /// </summary>
    /// <typeparam name="TT">
    /// The type of items that can be selected. Must be a non-nullable type.
    /// </typeparam>
    [PublicAPI]
    public class SelectBoxArraySelection< TT > : ArraySelection< T > where TT : notnull
    {
        private readonly SelectBox< TT > _parent;

        public SelectBoxArraySelection( SelectBox< TT > parent, List< T >? array )
            : base( array )
        {
            _parent = parent;
        }

        public override bool FireChangeEvent()
        {
            if ( _parent._selectedPrefWidth )
            {
                _parent.InvalidateHierarchy();
            }

            return base.FireChangeEvent();
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// A specialized <see cref="ClickListener"/> designed to handle user interactions
    /// with a <see cref="SelectBox{T}"/>. This listener processes user input events such
    /// as mouse clicks or touch input, enabling the display or dismissal of the options
    /// list for the associated <see cref="SelectBox{T}"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="SelectBoxClickListener"/> determines whether the options list should
    /// be shown or hidden based on the current state of the <see cref="SelectBox{T}"/> and
    /// the specific input received. It ensures proper interaction while considering the
    /// enabled or disabled status of the <see cref="SelectBox{T}"/>.
    /// </remarks>
    /// <seealso cref="SelectBox{T}"/>
    /// <seealso cref="ClickListener"/>
    [PublicAPI]
    public class SelectBoxClickListener : ClickListener
    {
        private readonly SelectBox< T > _parent;

        public SelectBoxClickListener( SelectBox< T > parent )
        {
            _parent = parent;
        }

        public override bool TouchDown( InputEvent? ev, float x, float y, int pointer, int button )
        {
            if ( ( pointer == 0 ) && ( button != 0 ) )
            {
                return false;
            }

            if ( _parent.IsDisabled )
            {
                return false;
            }

            if ( _parent._scrollPane!.HasParent() )
            {
                _parent.HideScrollPane();
            }
            else
            {
                _parent.ShowScrollPane();
            }

            return true;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Handles user interaction with the drop-down list portion of a <see cref="SelectBox{T}"/> by
    /// responding to mouse clicks and movements.
    /// <para>
    /// This listener is responsible for detecting click events on the list and updating the selection
    /// state of the parent <see cref="SelectBox{T}"/> accordingly. Clicking an item triggers a selection
    /// change, while moving the mouse over the list updates the hovered item.
    /// </para>
    /// <para>
    /// Overrides of <see cref="ClickListener"/> methods are used to implement custom behaviors such as
    /// triggering change events even when clicking the already selected item.
    /// </para>
    /// </summary>
    [PublicAPI]
    public class SelectBoxScrollPaneClickListener : ClickListener
    {
        private readonly SelectBoxScrollPane _parent;

        public SelectBoxScrollPaneClickListener( SelectBoxScrollPane parent )
        {
            _parent = parent;
        }

        public override void OnClicked( InputEvent? ev, float x, float y )
        {
            var selected = _parent.ListBox.GetSelected();

            // Force clicking the already selected item to trigger a change event.
            if ( selected != null )
            {
                _parent.SelectBox._selection.Clear();
            }

            _parent.SelectBox._selection.Choose( selected! );
            _parent.Hide();
        }

        public override bool MouseMoved( InputEvent? ev, float x, float y )
        {
            var index = _parent.ListBox.GetItemIndexAt( y );

            if ( index != -1 )
            {
                _parent.ListBox.SetSelectedIndex( index );
            }

            return true;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Represents an input listener specifically designed to handle interactions for
    /// a SelectBoxList. This listener is responsible for monitoring input events, such
    /// as when the pointer exits the bounds of the list, and updating the selection
    /// state accordingly.
    /// <para>
    /// The main purpose of this class is to ensure that the current selection remains correctly
    /// synchronized when the pointer moves outside the SelectBoxList and onto other actors.
    /// </para>
    /// <para>
    /// Typically, this is used internally within the SelectBox and its associated list component
    /// to provide appropriate input handling behavior.
    /// </para>
    /// </summary>
    [PublicAPI]
    public class SelectBoxScrollPaneInputListener : InputListener
    {
        private readonly SelectBoxScrollPane _parent;

        public SelectBoxScrollPaneInputListener( SelectBoxScrollPane parent )
        {
            _parent = parent;
        }

        public override void Exit( InputEvent? ev, float x, float y, int pointer, Actor? toActor )
        {
            if ( ( toActor == null ) || !_parent.IsAscendantOf( toActor ) )
            {
                _parent.ListBox.Selection.Set( _parent.SelectBox.GetSelected() );
            }
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Listener for managing the behavior of hiding a SelectBox's dropdown list in response
    /// to input events such as mouse clicks outside of the list or specific key presses.
    /// Acts as an InputListener attached to the SelectBoxList to handle interactions and
    /// ensure proper closure behavior.
    /// </summary>
    /// <remarks>
    /// The SelectBoxListHideListener is responsible for monitoring input events and determining
    /// when the dropdown list of the SelectBox should be hidden. It detects whether the input
    /// originated from outside the list and responds to specific key presses like Enter or Escape
    /// to trigger closure of the dropdown.
    /// </remarks>
    [PublicAPI]
    public class SelectBoxScrollPaneHideListener : InputListener
    {
        private readonly SelectBoxScrollPane _parent;

        public SelectBoxScrollPaneHideListener( SelectBoxScrollPane parent )
        {
            _parent = parent;
        }

        public override bool TouchDown( InputEvent? ev, float x, float y, int pointer, int button )
        {
            var target = ev?.TargetActor;

            if ( _parent.IsAscendantOf( target ) )
            {
                return false;
            }

            _parent.ListBox.Selection.Set( _parent.SelectBox.GetSelected() );
            _parent.Hide();

            return false;
        }

        public override bool KeyDown( InputEvent? ev, int keycode )
        {
            switch ( keycode )
            {
                case IInput.Keys.NUMPAD_ENTER:
                case IInput.Keys.ENTER:
                {
                    _parent.SelectBox._selection.Choose( _parent.ListBox.GetSelected()! );
                    _parent.Hide();
                    ev?.Stop();

                    break;
                }

                // Fall thru.
                case IInput.Keys.ESCAPE:
                {
                    _parent.Hide();
                    ev?.Stop();

                    return true;
                }
            }

            return false;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// The Style for a <see cref="SelectBox{T}"/>.
    /// </summary>
    [PublicAPI]
    public class SelectBoxStyle
    {
        public BitmapFont                 Font               { get; } = null!;
        public ScrollPane.ScrollPaneStyle ScrollStyle        { get; } = null!;
        public ListBox< T >.ListStyle     ListStyle          { get; } = null!;
        public Color                      FontColor          { get; } = new( 1, 1, 1, 1 );
        public Color?                     OverFontColor      { get; }
        public Color?                     DisabledFontColor  { get; }
        public ISceneDrawable?            Background         { get; }
        public ISceneDrawable?            BackgroundOver     { get; }
        public ISceneDrawable?            BackgroundOpen     { get; }
        public ISceneDrawable?            BackgroundDisabled { get; }

        // ====================================================================

        public SelectBoxStyle()
        {
        }

        public SelectBoxStyle( BitmapFont font,
                               Color fontColor,
                               ISceneDrawable background,
                               ScrollPane.ScrollPaneStyle scrollStyle,
                               ListBox< T >.ListStyle listStyle )
        {
            Font        = font;
            Background  = background;
            ScrollStyle = scrollStyle;
            ListStyle   = listStyle;

            FontColor.Set( fontColor );
        }

        public SelectBoxStyle( SelectBoxStyle? style )
        {
            Guard.Against.Null( style );

            Font = style.Font;
            FontColor.Set( style.FontColor );

            if ( style.OverFontColor != null )
            {
                OverFontColor = new Color( style.OverFontColor );
            }

            if ( style.DisabledFontColor != null )
            {
                DisabledFontColor = new Color( style.DisabledFontColor );
            }

            Background  = style.Background;
            ScrollStyle = new ScrollPane.ScrollPaneStyle( style.ScrollStyle );
            ListStyle   = new ListBox< T >.ListStyle( style.ListStyle );

            BackgroundOver     = style.BackgroundOver;
            BackgroundOpen     = style.BackgroundOpen;
            BackgroundDisabled = style.BackgroundDisabled;
        }
    }
}