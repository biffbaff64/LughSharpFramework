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
using LughSharp.Source.Scene2D.Actions;
using LughSharp.Source.Scene2D.Listeners;
using LughSharp.Source.Scene2D.UI.Styles;
using LughSharp.Source.Scene2D.Utils;
using LughSharp.Source.Utils.Pooling;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// A select box (aka a drop-down list) allows a user to choose one of a number of values
/// from a list. When inactive, the selected value is displayed. When activated, it shows
/// the list of values that may be selected.
/// <para>
/// <see cref="ChangeListener.ChangeEvent"/> is fired when the selectbox selection changes.
/// </para>
/// <para>
/// The preferred size of the select box is determined by the maximum text bounds of the
/// items and the size of the <see cref="SelectBoxStyle.Background"/>.
/// </para>
/// </summary>
[PublicAPI]
[ActorDefinition( Role = "UI" )]
public class SelectBox< T > : Widget, IStyleable< SelectBoxStyle >, IDisableable where T : notnull
{
    public ClickListener        ClickListener { get; set; }
    public List< T >            Items         { get; } = [ ];
    public bool                 IsDisabled    { get; set; }
    public SelectBoxScrollPane? ScrollPane    { get; private set; }
    public ArraySelection< T >  Selection     { get; private set; }

    // ========================================================================

    protected const int DefaultMaxListCount = 0;

    // ========================================================================

    private readonly Vector2 _temp = new();

    private SelectBoxStyle _style     = null!;
    private Align          _alignment = Align.Left;
    private bool           _selectedPrefWidth;
    private float          _prefWidth;
    private float          _prefHeight;

    // ========================================================================

    /// <summary>
    /// Creates a select box with a <see cref="SelectBoxStyle"/> from the provided skin
    /// and style name, which defaults to "default".
    /// </summary>
    /// <param name="skin"></param>
    /// <param name="styleName"></param>
    public SelectBox( Skin skin, string styleName = "default" )
        : this( skin.Get< SelectBoxStyle >( styleName ) )
    {
    }

    /// <summary>
    /// Creates a select box with the provided style. Also initialises the <see cref="SelectBoxScrollPane"/>
    /// and <see cref="SelectBoxClickListener"/>.
    /// </summary>
    /// <param name="style"></param>
    public SelectBox( SelectBoxStyle style )
    {
        SetStyle( style );
        SetSize( GetPrefWidthUnchecked(), GetPrefHeightUnchecked() );

        Selection = new SelectBoxArraySelection( this, Items )
        {
            Actor    = this,
            Required = true
        };

        ScrollPane    = new SelectBoxScrollPane( this );
        ClickListener = new SelectBoxClickListener( this );

        AddListener( ClickListener );
    }

    /// <summary>
    /// Sets the stage this select box should be added to. If null, the selectbox
    /// will be hidden.
    /// </summary>
    /// <param name="stage"></param>
    public override void SetStage( Stage? stage )
    {
        if ( stage == null )
        {
            ScrollPane?.Hide();
        }

        base.SetStage( stage );
    }

    /// <summary>
    /// Returns the select box's style. Modifying the returned style may not have an effect
    /// until <see cref="SetStyle(SelectBoxStyle)"/> is called.
    /// </summary>
    public SelectBoxStyle GetStyle()
    {
        return _style;
    }

    /// <summary>
    /// Sets the <see cref="SelectBoxStyle"/> to use for this select box.
    /// </summary>
    /// <param name="style"></param>
    public void SetStyle( SelectBoxStyle style )
    {
        _style = new SelectBoxStyle
        {
            Font               = style.Font,
            FontColor          = style.FontColor,
            ScrollPaneStyle    = new ScrollPaneStyle( style.ScrollPaneStyle ),
            ListBoxStyle       = new ListBoxStyle( style.ListBoxStyle ),
            Background         = style.Background,
            BackgroundDisabled = style.BackgroundDisabled,
            BackgroundOpen     = style.BackgroundOpen,
            BackgroundOver     = style.BackgroundOver,
            DisabledFontColor  = style.DisabledFontColor,
            OverFontColor      = style.OverFontColor
        };

        if ( ScrollPane != null )
        {
            ScrollPane.SetStyle( style.ScrollPaneStyle );
            ScrollPane.ListBox.SetStyle( style.ListBoxStyle );
        }

        InvalidateHierarchy();
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
        Selection.Clear();
        ScrollPane?.ListBox.ClearItems();

        InvalidateHierarchy();
    }

    /// <summary>
    /// Set the backing List that makes up the choices available in the SelectBox
    /// </summary>
    public void SetItems( T[] newItems )
    {
        Guard.Against.Null( ScrollPane );

        float oldPrefWidth = GetPrefWidth();

        Items.Clear();
        Items.AddRange( newItems );

        Selection.Validate();
        ScrollPane.ListBox.SetItems( Items );

        InvalidateLayout();

        if ( Math.Abs( oldPrefWidth - GetPrefWidth() ) >= NumberUtils.FloatTolerance )
        {
            InvalidateHierarchy();
        }
    }

    /// <summary>
    /// Sets the items visible in the select box.
    /// </summary>
    public void SetItems( List< T > newItems )
    {
        SetItems( newItems.ToArray() );
    }

    /// <summary>
    /// Lays out the components of the <see cref="SelectBox{T}"/> based on its style and content.
    /// Calculates the preferred width and height of the select box, adjusts sizing, and validates
    /// that the required resources like style and font are initialized. Throws an exception if
    /// the style is not set.
    /// </summary>
    /// <exception cref="RuntimeException">
    /// Thrown when the style of the select box is null, indicating that no <see cref="SelectBoxStyle"/>
    /// is defined for the widget.
    /// </exception>
    public override void Layout()
    {
        ISceneDrawable? bg   = _style.Background;
        BitmapFont      font = _style.Font;

        if ( bg != null )
        {
            _prefHeight = ( Math.Max( bg.TopHeight + bg.BottomHeight + font.GetCapHeight()
                                    - ( font.GetDescent() * 2 ),
                                      bg.MinHeight ) );
        }
        else
        {
            _prefHeight = ( font.GetCapHeight() - ( font.GetDescent() * 2 ) );
        }

        Pool< GlyphLayout > layoutPool = PoolsMap.Get< GlyphLayout >( () => new GlyphLayout() );
        GlyphLayout         layout     = layoutPool.Obtain();

        if ( _selectedPrefWidth )
        {
            _prefWidth = ( bg != null ) ? ( bg.LeftWidth + bg.RightWidth ) : 0;

            T? selected = GetSelected();

            if ( selected != null )
            {
                layout.SetText( font, ToString( selected ) );
                _prefWidth += layout.Width;
            }
        }
        else
        {
            float maxItemWidth = 0;

            foreach ( T t in Items )
            {
                layout.SetText( font, ToString( t ) );
                maxItemWidth = Math.Max( layout.Width, maxItemWidth );
            }

            _prefWidth = ( maxItemWidth );

            if ( bg != null )
            {
                _prefWidth = ( Math.Max( _prefWidth + bg.LeftWidth + bg.RightWidth, bg.MinWidth ) );
            }

            ListBoxStyle    listStyle   = _style.ListBoxStyle;
            ScrollPaneStyle scrollStyle = _style.ScrollPaneStyle;

            float scrollWidth = maxItemWidth
                              + listStyle.Selection.LeftWidth
                              + listStyle.Selection.RightWidth;

            bg = scrollStyle.Background;

            if ( bg != null )
            {
                scrollWidth = Math.Max( ( scrollWidth + bg.LeftWidth + bg.RightWidth ), bg.MinWidth );
            }

            if ( _style.ScrollPaneStyle is { VScroll: not null, VScrollKnob: not null } )
            {
                if ( ScrollPane is not { DisableYScroll: true } )
                {
                    scrollWidth += Math.Max( _style.ScrollPaneStyle.VScroll.MinWidth,
                                             _style.ScrollPaneStyle.VScrollKnob.MinWidth );
                }
            }

            _prefWidth = ( Math.Max( _prefWidth, scrollWidth ) );
        }

        layoutPool.Free( layout );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="parentAlpha"></param>
    public override void Draw( IBatch batch, float parentAlpha )
    {
        Validate();

        ISceneDrawable background = GetBackgroundDrawable();
        Color          fontColor  = GetFontColor();
        BitmapFont     font       = _style.Font;

        // Make copies of x, y, width and height for local modification
        // and preserving the originals.
        float x      = GetX();
        float y      = GetY();
        float width  = GetWidth();
        float height = GetHeight();

        batch.SetColor( ActorColor.R, ActorColor.G, ActorColor.B, ActorColor.A * parentAlpha );

        background.Draw( batch, x, y, width, height );

        T? selected = Selection.First();

        if ( selected != null )
        {
            width  -= background.LeftWidth + background.RightWidth;
            height -= background.BottomHeight + background.TopHeight;
            x      += background.LeftWidth;
            y      += ( int )( ( height / 2 ) + background.BottomHeight + ( font.FontData.CapHeight / 2 ) );

            font.SetColor( fontColor.R, fontColor.G, fontColor.B, fontColor.A * parentAlpha );

            DrawItem( batch, font, selected, x, y, width );
        }
    }

    protected void DrawItem( IBatch batch, BitmapFont font, T item, float x, float y, float width )
    {
        string str = ToString( item );

        font.Draw( batch, str, x, y, 0, str.Length, width, _alignment, false, "..." );
    }

    /// <summary>
    /// Returns appropriate background Drawable from the style based on the current select box state.
    /// </summary>
    protected ISceneDrawable GetBackgroundDrawable()
    {
        Guard.Against.Null( _style.Background );

        if ( IsDisabled )
        {
            return _style.BackgroundDisabled ?? _style.Background;
        }

        if ( ScrollPane != null && ScrollPane.HasParent() )
        {
            return _style.BackgroundOpen ?? _style.Background;
        }

        return IsOver() ? _style.BackgroundOver ?? _style.Background : _style.Background;
    }

    /// <summary>
    /// Returns the appropriate label font color from the style based on the current button state.
    /// </summary>
    protected Color GetFontColor()
    {
        if ( IsDisabled )
        {
            return _style.DisabledFontColor ?? _style.FontColor;
        }

        if ( IsOver() || ( ScrollPane != null && ScrollPane.HasParent() ) )
        {
            return _style.OverFontColor ?? _style.FontColor;
        }

        return _style.FontColor;
    }

    /// <summary>
    /// Sets the alignment of the selected item in the select box. See <see cref="GetList()"/>
    /// and <see cref="SetAlignment(Align)"/> to set the alignment in the list shown when the
    /// select box is open.
    /// </summary>
    /// <param name="alignment"> See <see cref="Align"/>. </param>
    public void SetAlignment( Align alignment )
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
        return Selection;
    }

    /// <summary>
    /// Returns the first selected item, or null. For multiple selections
    /// use <see cref="GetSelection()"/>.
    /// </summary>
    public T? GetSelected()
    {
        return Selection.First();
    }

    /// <summary>
    /// Sets the selection to only the passed item, if it is a possible
    /// choice, else selects the first item.
    /// </summary>
    public void SetSelected( T item )
    {
        if ( Items.Contains( item ) )
        {
            Selection.Set( item );
        }
        else if ( Items.Count > 0 )
        {
            Selection.Set( Items.First() );
        }
        else
        {
            Selection.Clear();
        }
    }

    /// <returns>
    /// The index of the first selected item. The top item has an index of 0.
    /// Nothing selected has an index of -1.
    /// </returns>
    public int GetSelectedIndex()
    {
        SortedSet< T > selected = Selection.Items();

        return selected.Count == 0 ? -1 : Items.IndexOf( selected.First() );
    }

    /// <summary>
    /// Sets the selection to only the selected index.
    /// </summary>
    public void SetSelectedIndex( int index )
    {
        if ( index < 0 || index >= Items.Count )
        {
            throw new IndexOutOfRangeException( "Index out of range: " + index );
        }

        Selection.Set( Items[ index ] );
    }

    /// <summary>
    /// Disables the select box if the provided boolean is true, otherwise enables it.
    /// </summary>
    public void SetDisabled( bool disabled )
    {
        if ( disabled && !IsDisabled )
        {
            HideScrollPane();
        }

        IsDisabled = disabled;
    }

    /// <summary>
    /// Disables scrolling of the list shown when the select box is open.
    /// </summary>
    /// <param name="xstate"> true to disable horizontal scrolling, false to enable horizontal scrolling. </param>
    /// <param name="ystate"> true to disable vertical scrolling, false to enable vertical scrolling. </param>
    public void SetScrollingDisabled( bool xstate, bool ystate )
    {
        ScrollPane?.SetScrollingDisabled( xstate, ystate );

        InvalidateHierarchy();
    }

    /// <summary>
    /// Returns true if the mouse or touch is over the actor or pressed and within
    /// the tap square.
    /// </summary>
    public bool IsOver()
    {
        return ClickListener.Over;
    }

    /// <summary>
    /// Scene2D Actions to perform when the specified select box is first shown.
    /// </summary>
    /// <param name="selectBox"></param>
    /// <param name="below"></param>
    protected void OnShow( Actor selectBox, bool below )
    {
        // Sets the fade in starting point to the current alpha value.
        selectBox.ActorColor.A = 0;

        selectBox.AddAction( SceneActions.FadeIn( 0.3f, Interpolation.Fade ) );
    }

    /// <summary>
    /// Scene2D Actions to perform when the specified select box is hidden.
    /// </summary>
    /// <param name="selectBox"></param>
    protected void OnHide( Actor selectBox )
    {
        // Sets the fade out starting point to the current alpha value.
        selectBox.ActorColor.A = 1f;

        selectBox.AddAction( SceneActions.Sequence( SceneActions.FadeOut( 0.15f, Interpolation.Fade ),
                                                    SceneActions.RemoveActor() ) );
    }

    /// <summary>
    /// Shows the ScrollPane containing the list of items shown when the select box is open.
    /// </summary>
    public void ShowScrollPane()
    {
        if ( Items.Count == 0 )
        {
            return;
        }

        var stage = GetStage();
        
        if ( stage != null )
        {
            ScrollPane?.Show( stage );
        }
    }

    /// <summary>
    /// Hides the ScrollPane containing the list of items shown.
    /// </summary>
    public void HideScrollPane()
    {
        ScrollPane?.Hide();
    }

    /// <summary>
    /// The preferred width of the select box based on the items it contains.
    /// </summary>
    public override float GetPrefWidth()
    {
        Validate();

        return _prefWidth;
    }

    /// <summary>
    /// The preferred height of the select box based on the items it contains.
    /// </summary>
    public override float GetPrefHeight()
    {
        Validate();

        return _prefHeight;
    }

    /// <summary>
    /// The preferred width of the select box based on the items it contains. This
    /// method is a wrapper for <see cref="GetPrefWidth()"/>, and is provided to
    /// allow calling safely from constructors.
    /// </summary>
    public float GetPrefWidthUnchecked()
    {
        return GetPrefWidth();
    }

    /// <summary>
    /// The preferred height of the select box based on the items it contains. This
    /// method is a wrapper for <see cref="GetPrefHeight()"/>, and is provided to
    /// allow calling safely from constructors.
    /// </summary>
    private float GetPrefHeightUnchecked()
    {
        return GetPrefHeight();
    }

    /// <summary>
    /// The width of the select box based on the items it contains. This method is
    /// a wrapper for <see cref="Actor.GetWidth()"/>, and is provided to allow calling
    /// safely from constructors.
    /// </summary>
    /// <returns></returns>
    private float GetWidthUnchecked()
    {
        return GetWidth();
    }

    /// <summary>
    /// The height of the select box based on the items it contains. This method is
    /// a wrapper for <see cref="Actor.GetHeight()"/>, and is provided to allow calling
    /// safely from constructors.
    /// </summary>
    /// <returns></returns>
    public float GetHeightUnchecked()
    {
        return GetHeight();
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
        Pool< GlyphLayout > layoutPool = PoolsMap.Get< GlyphLayout >( () => new GlyphLayout() );
        GlyphLayout         layout     = layoutPool.Obtain();
        float               width      = 0;

        foreach ( T item in Items )
        {
            layout.SetText( _style.Font, ToString( item ) );
            width = Math.Max( layout.Width, width );
        }

        ISceneDrawable? bg = _style.Background;

        if ( bg != null )
        {
            width = Math.Max( width + bg.LeftWidth + bg.RightWidth, bg.MinWidth );
        }

        layoutPool.Free( layout );

        return width;
    }

    /// <summary>
    /// Set the max number of items to display when the select box is opened. Set to
    /// 0 (the default) to display as many as fit in the stage height.
    /// </summary>
    public int MaxListCount
    {
        get => ScrollPane?.MaxListCount ?? DefaultMaxListCount;
        set => ScrollPane?.MaxListCount = value;
    }

    /// <summary>
    /// Returns the list shown when the select box is open.
    /// </summary>
    public ListBox< T > GetList()
    {
        return ScrollPane?.ListBox ?? throw new RuntimeException( "No ListBox available!" );
    }

    protected string ToString( T item )
    {
        if ( item is string str )
        {
            return str;
        }

        return item.ToString() ?? string.Empty;
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// A SelectBoxScrollPane extends the <see cref="ScrollPane"/> to provide a specialized
    /// scrollable container for displaying a list of items in a <see cref="SelectBox{T}"/>.
    /// </summary>
    [PublicAPI]
    public class SelectBoxScrollPane : ScrollPane
    {
        public int            MaxListCount    { get; set; }
        public ListBox< T >   ListBox         { get; set; }
        public SelectBox< T > ParentSelectBox { get; set; }

        // ====================================================================

        private readonly InputListener _hideListener;
        private readonly Vector2       _stagePosition = new();
        private          Actor?        _previousScrollFocus;

        // ====================================================================

        public SelectBoxScrollPane( SelectBox< T > parent ) : base( null, parent._style.ScrollPaneStyle )
        {
            ParentSelectBox = parent;

            SetOverscroll( false, false );
            SetFadeScrollBars( false );
            SetScrollingDisabled( true, false );

            ListBox = new ListBox< T >( ParentSelectBox._style.ListBoxStyle )
            {
                Touchable    = Touchable.Disabled,
                TypeToSelect = true
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

            Logger.Checkpoint();

            stage.AddActor( this );
            stage.AddCaptureListener( _hideListener );
            stage.AddListener( ListBox.KeyListener );

            ParentSelectBox.LocalToStageCoordinates( _stagePosition.Set( 0, 0 ) );

            // Show the list above or below the select box, limited to a number
            // of items and the available height in the stage.
            float itemHeight = ListBox.ItemHeight;

            float height = itemHeight * ( MaxListCount <= 0
                                              ? ParentSelectBox.Items.Count
                                              : Math.Min( MaxListCount, ParentSelectBox.Items.Count ) );

            ISceneDrawable? scrollPaneBackground = GetStyle().Background;

            if ( scrollPaneBackground != null )
            {
                height += scrollPaneBackground.TopHeight + scrollPaneBackground.BottomHeight;
            }

            ISceneDrawable? listBackground = ListBox.GetStyle().Background;

            if ( listBackground != null )
            {
                height += listBackground.TopHeight + listBackground.BottomHeight;
            }

            float heightBelow = _stagePosition.Y;
            float heightAbove = stage.Height - heightBelow - ParentSelectBox.GetHeight();
            var   below       = true;

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
                SetY( _stagePosition.Y - height );
            }
            else
            {
                SetY( _stagePosition.Y + ParentSelectBox.GetHeight() );
            }

            SetHeight( height );
            Validate();

            float width = Math.Max( GetPrefWidth(), ParentSelectBox.GetWidth() );

            SetWidth( width );

            float x = _stagePosition.X;

            if ( x + width > stage.Width )
            {
                x -= width - ParentSelectBox.GetWidth() - 1;

                if ( x < 0 ) x = 0;
            }

            SetX( x );
            Validate();

            int idx = ParentSelectBox.GetSelectedIndex();

            if ( idx >= 0 )
            {
                ScrollTo( x: 0,
                          y: ListBox.GetHeight() - ( idx * itemHeight ) - ( itemHeight / 2 ),
                          width: 0,
                          height: 0,
                          centerHorizontal: true,
                          centerVertical: true );
            }

            UpdateVisualScroll();

            _previousScrollFocus = null;

            Actor? actor = stage.ScrollFocus;

            if ( ( actor != null ) && !actor.IsDescendantOf( this ) )
            {
                _previousScrollFocus = actor;
            }

            stage.ScrollFocus = this;

            ListBox.Selection.Set( ParentSelectBox.GetSelected() );
            ListBox.Touchable = Touchable.Enabled;

            ClearActions();

            ParentSelectBox.OnShow( this, below );
        }

        public void Hide()
        {
            if ( !ListBox.IsTouchable() || !HasParent() )
            {
                return;
            }

            ListBox.Touchable = Touchable.Disabled;

            if ( GetStage() != null )
            {
                // Returns false if '_hideListener' is null.
                _ = GetStage()?.RemoveCaptureListener( _hideListener );

                // Returns false if 'ListBox.KeyListener' is null.
                _ = GetStage()?.RemoveListener( ListBox.KeyListener );

                if ( _previousScrollFocus != null && _previousScrollFocus.GetStage() == null )
                {
                    _previousScrollFocus = null;
                }

                Actor? actor = GetStage()?.ScrollFocus;

                if ( ( actor == null ) || IsAscendantOf( actor ) )
                {
                    GetStage()?.ScrollFocus = _previousScrollFocus;
                }
            }

            ClearActions();

            ParentSelectBox.OnHide( this );
        }

        private readonly Vector2 _drawTemp = new();

        public override void Draw( IBatch batch, float parentAlpha )
        {
            ParentSelectBox.LocalToStageCoordinates( _drawTemp.Set( 0, 0 ) );

            if ( Math.Abs( _drawTemp.X - _stagePosition.X ) > 0.01f
              || Math.Abs( _drawTemp.Y - _stagePosition.Y ) > 0.01f )
            {
                Hide();

                return;
            }

            base.Draw( batch, parentAlpha );
        }

        public override void Act( float delta )
        {
            base.Act( delta );
            BringToFront();
        }

        public override void SetStage( Stage? stage )
        {
            Stage? oldStage = GetStage();

            if ( oldStage != null )
            {
                oldStage.RemoveCaptureListener( _hideListener );
                oldStage.RemoveListener( ListBox.KeyListener );
            }

            base.SetStage( stage );
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Represents a selection functionality specific to the <see cref="SelectBox{T}"/> widget,
    /// where the selections are backed by an array or list of items.  This class extends the
    /// <see cref="ArraySelection{T}"/> to provide custom selection behavior that integrates
    /// seamlessly with the <see cref="SelectBox{T}"/> component.
    /// </summary>
    [PublicAPI]
    public class SelectBoxArraySelection : ArraySelection< T >
    {
        private readonly SelectBox< T > _parent;

        // ====================================================================

        public SelectBoxArraySelection( SelectBox< T > parent, List< T > array )
            : base( array )
        {
            _parent = parent;
        }

        /// <summary>
        /// Fires a change event on the selection's actor, if any. Called internally when the selection
        /// changes, depending on <see cref="ArraySelection{T}.ProgrammaticChangeEvents"/>.
        /// </summary>
        /// <returns> true if the change should be undone. </returns>
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
            if ( button != 0 )
            {
                return false;
            }

            if ( _parent.IsDisabled )
            {
                return false;
            }

            if ( _parent.ScrollPane != null )
            {
                if ( _parent.ScrollPane.HasParent() )
                {
                    _parent.HideScrollPane();
                }
                else
                {
                    _parent.ShowScrollPane();
                }
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
            T? selected = _parent.ListBox.GetSelected();

            // Force clicking the already selected item to trigger a change event.
            if ( selected != null )
            {
                _parent.ParentSelectBox.Selection.Clear();
                _parent.ParentSelectBox.Selection.Choose( selected );
                _parent.Hide();
            }
        }

        public override bool OnMouseMoved( InputEvent? ev, float x, float y )
        {
            int index = _parent.ListBox.GetItemIndexAt( y );

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
                _parent.ListBox.Selection.Set( _parent.ParentSelectBox.GetSelected() );
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

        public override bool OnTouchDown( InputEvent? ev, float x, float y, int pointer, int button )
        {
            Actor? target = ev?.TargetActor;

            if ( _parent.IsAscendantOf( target ) )
            {
                return false;
            }

            _parent.ListBox.Selection.Set( _parent.ParentSelectBox.GetSelected() );
            _parent.Hide();

            return false;
        }

        public override bool OnKeyDown( InputEvent? ev, int keycode )
        {
            switch ( keycode )
            {
                case IInput.Keys.NumpadEnter:
                case IInput.Keys.Enter:
                {
                    _parent.ParentSelectBox.Selection.Choose( _parent.ListBox.GetSelected()! );
                    _parent.Hide();
                    ev?.Stop();

                    break;
                }

                case IInput.Keys.Escape:
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
}