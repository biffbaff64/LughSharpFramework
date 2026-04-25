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
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Utils;
using LughSharp.Core.Scene2D.UI.Styles;
using LughSharp.Core.Scene2D.Utils;
using LughSharp.Core.Utils.Pooling;

using Rectangle = LughSharp.Core.Maths.Rectangle;

namespace LughSharp.Core.Scene2D.UI;

[PublicAPI]
public class Table : WidgetGroup
{
    public enum DebugType
    {
        None,
        All,
        Table,
        Cell,
        Actor
    }

    // ========================================================================

    /// <value>
    /// The skin that was passed to this table in its constructor, or null if none was given.
    /// </value>
    public Skin? Skin { get; set; }

    /// <summary>
    /// Causes the contents to be clipped if they exceed the table's bounds.
    /// Enabling clipping sets <see cref="Group.Transform"/> to true.
    /// </summary>
    public bool Clip
    {
        get => _clip;
        set
        {
            _clip     = value;
            Transform = value;
            Invalidate();
        }
    }

    public DebugType TableDebug { get; set; } = DebugType.None;

    /// <summary>
    /// If true (the default), positions and sizes of child actors are
    /// rounded and ceiled to the nearest integer value.
    /// </summary>
    public bool Round { get; set; } = true;

    public int          Rows         { get; private set; }
    public int          Columns      { get; set; }
    public Cell         CellDefaults { get; set; }
    public List< Cell > Cells        { get; private set; } = new( 4 );

    public Pool< Cell > CellPool { get; } = new()
    {
        NewObjectFactory = GetNewObject
    };

    public Align Alignment { get; private set; } = Align.Center;

    // ========================================================================

    public static readonly Color DebugTableColor = new( 0, 0, 1, 1 );
    public static readonly Color DebugCellColor  = new( 1, 0, 0, 1 );
    public static readonly Color DebugActorColor = new( 0, 1, 0, 1 );

    // ========================================================================

    private static float[]? _columnWeightedWidth;
    private static float[]? _rowWeightedHeight;

    private List< DebugRect >? _debugRects;
    private ISceneDrawable?    _background;

    private bool _clip;
    private bool _implicitEndRow;
    private bool _sizeInvalid = true;

    private readonly List< Cell > _columnDefaults = new( 2 );
    private          Cell         _rowDefaults    = new();

    private float[] _columnWidth     = null!;
    private float[] _rowHeight       = null!;
    private float[] _rowMinHeight    = null!;
    private float[] _rowPrefHeight   = null!;
    private float[] _columnMinWidth  = null!;
    private float[] _columnPrefWidth = null!;
    private float[] _expandHeight    = null!;
    private float[] _expandWidth     = null!;
    private float   _tableMinHeight;
    private float   _tableMinWidth;
    private float   _tablePrefHeight;
    private float   _tablePrefWidth;

    private Value _padBottom = BackgroundBottom;
    private Value _padLeft   = BackgroundLeft;
    private Value _padRight  = BackgroundRight;
    private Value _padTop    = BackgroundTop;

    // ========================================================================

    public Table() : this( null )
    {
    }

    /// <summary>
    /// Creates a table with a skin, which is required to use <see cref="AssetDescriptor"/>
    /// </summary>
    public Table( Skin? skin )
    {
        Skin         = skin;
        CellDefaults = ObtainCell();
        Transform    = false;
        Touchable    = Touchable.ChildrenOnly;
    }

    /// <summary>
    /// Obtains a <see cref="Cell"/> from the <see cref="CellPool"/>. Also sets the table
    /// for that cell to this table.
    /// </summary>
    public Cell ObtainCell()
    {
        Cell cell = CellPool.Obtain();

        cell.Table = this;

        return cell;
    }

    /// <summary>
    /// Draws the table and all its children.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="parentAlpha"></param>
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

                if ( ClipBegin( padLeft,
                                padBottom,
                                Width - padLeft - _padRight.Get( this ),
                                Height - padBottom - _padTop.Get( this ) ) )
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
            DrawChildren( batch, parentAlpha );
        }
    }

    /// <summary>
    /// Called to draw the background, before clipping is applied (if enabled).
    /// Default implementation draws the background drawable.
    /// </summary>
    protected virtual void DrawBackground( IBatch batch, float parentAlpha, float x, float y )
    {
        if ( _background == null )
        {
            return;
        }

        batch.SetColor( ActorColor.R, ActorColor.G, ActorColor.B, ActorColor.A * parentAlpha );

        _background.Draw( batch, x, y, Width, Height );
    }

    /// <summary>
    /// Sets the background drawable from the skin and adjusts the table's padding
    /// to match the background. This may only be called if a skin has been set
    /// with <see cref="Table"/> or <see cref="Skin"/>.
    /// </summary>
    public void SetBackground( string drawableName )
    {
        if ( Skin == null )
        {
            throw new RuntimeException( "Table must have a skin set to use this method." );
        }

        SetBackground( Skin.GetDrawable( drawableName ) );
    }

    /// <summary>
    /// <param name="background"> May be null to clear the background. </param>
    /// </summary>
    public void SetBackground( ISceneDrawable? background )
    {
        if ( _background == background )
        {
            return;
        }

        float padTopOld    = GetPadTop();
        float padLeftOld   = GetPadLeft();
        float padBottomOld = GetPadBottom();
        float padRightOld  = GetPadRight();

        _background = background; // The default pad values use the background's padding.

        float padTopNew    = GetPadTop();
        float padLeftNew   = GetPadLeft();
        float padBottomNew = GetPadBottom();
        float padRightNew  = GetPadRight();

        if ( Math.Abs( ( padTopOld + padBottomOld ) - ( padTopNew + padBottomNew ) ) > NumberUtils.FloatTolerance
          || Math.Abs( ( padLeftOld + padRightOld ) - ( padLeftNew + padRightNew ) ) > NumberUtils.FloatTolerance )
        {
            InvalidateHierarchy();
        }
        else if ( Math.Abs( padTopOld - padTopNew ) > NumberUtils.FloatTolerance
               || Math.Abs( padLeftOld - padLeftNew ) > NumberUtils.FloatTolerance
               || Math.Abs( padBottomOld - padBottomNew ) > NumberUtils.FloatTolerance
               || Math.Abs( padRightOld - padRightNew ) > NumberUtils.FloatTolerance )
        {
            Invalidate();
        }
    }

    public ISceneDrawable? GetBackground()
    {
        return _background;
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

    public Table SetClip( bool enabled = true )
    {
        Clip = enabled;

        return this;
    }

    /// <summary>
    /// Invalidates this actor's layout, causing <see cref="ILayout.Layout"/> to happen the
    /// next time <see cref="ILayout.Validate"/> is called. This method should be called when
    /// state changes in the actor that requires a layout but does not change the minimum,
    /// preferred, maximum, or actual size of the actor (meaning it does not affect the
    /// parent actor's layout).
    /// </summary>
    public override void Invalidate()
    {
        _sizeInvalid = true;
        base.Invalidate();
    }

    /// <summary>
    /// Adds a new cell to the table with the specified actor.
    /// </summary>
    public Cell AddCell< T >( T? actor ) where T : Actor
    {
        Cell cell = ObtainCell();
        cell.Actor = actor;

        // The row was ended for layout, not by the user, so revert it.
        if ( _implicitEndRow )
        {
            _implicitEndRow = false;
            Rows--;
            Cells.Peek().EndRow = false;
        }

        int cellCount = Cells.Count;

        if ( cellCount > 0 )
        {
            // Set cell column and row.
            Cell lastCell = Cells.Peek();

            if ( !lastCell.EndRow )
            {
                cell.Column = lastCell.Column + lastCell.Colspan;
                cell.Row    = lastCell.Row;
            }
            else
            {
                cell.Column = 0;
                cell.Row    = lastCell.Row + 1;
            }

            // Set the index of the cell above.
            if ( cell.Row > 0 )
            {
                for ( int i = cellCount - 1; i >= 0; i-- )
                {
                    Cell other = Cells[ i ];

                    for ( int column = other.Column, nn = column + other.Colspan; column < nn; column++ )
                    {
                        if ( column == cell.Column )
                        {
                            cell.CellAboveIndex = i;

                            goto outer;
                        }
                    }
                }

            outer: ;
            }
        }
        else
        {
            cell.Column = 0;
            cell.Row    = 0;
        }

        cell.Set( CellDefaults );

        if ( cell.Column < _columnDefaults.Count )
        {
            cell.Merge( _columnDefaults[ cell.Column ] );
        }

        cell.Merge( _rowDefaults );

        Cells.Add( cell );

        if ( actor != null )
        {
            AddActor( actor );
        }

        return cell;
    }

    public Table AddCell( Actor[] actors )
    {
        for ( int i = 0, n = actors.Length; i < n; i++ )
        {
            AddCell< Actor >( actors[ i ] );
        }

        return this;
    }

    /// <summary>
    /// Adds a new cell with a label. This may only be called if a skin
    /// has been set with <see cref="Table"/> or <see cref="Skin"/>
    /// </summary>
    public Cell AddCell( string? text )
    {
        Guard.Against.Null( text );

        return Skin == null
            ? throw new RuntimeException( "Table must have a skin set to use this method." )
            : AddCell( new Label( text, Skin ) );
    }

    /// <summary>
    /// Adds a new cell with a label. This may only be called if a skin
    /// has been set with <see cref="Table"/> or <see cref="Skin"/>
    /// </summary>
    public Cell AddCell( string text, string labelStyleName )
    {
        return Skin == null
            ? throw new RuntimeException( "Table must have a skin set to use this method." )
            : AddCell( new Label( text, Skin.Get< LabelStyle >( labelStyleName ) ) );
    }

    /// <summary>
    /// Adds a new cell with a label. This may only be called if a skin
    /// has been set with <see cref="Table"/> or <see cref="Skin"/>.
    /// </summary>
    public Cell AddCell( string? text, string fontName, Color color )
    {
        return Skin == null
            ? throw new RuntimeException( "Table must have a skin set to use this method." )
            : AddCell( new Label( text, new LabelStyle( Skin.GetFont( fontName ), color ) ) );
    }

    /// <summary>
    /// Adds a new cell with a label. This may only be called if a skin
    /// has been set with <see cref="Table"/> or <see cref="Skin"/>.
    /// </summary>
    public Cell AddCell( string? text, string fontName, string colorName )
    {
        return Skin == null
            ? throw new RuntimeException( "Table must have a skin set to use this method." )
            : AddCell( new Label( text, new LabelStyle( Skin.GetFont( fontName ), Skin.GetColor( colorName ) ) ) );
    }

    /// <summary>
    /// Adds a new cell to the table with the specified actors in a <see cref="Stack"/>.
    /// </summary>
    /// <param name="actors"> May be null or empty to add a stack without any actors. </param>
    public Cell Stack( params Actor[]? actors )
    {
        var stack = new Stack();

        if ( actors != null )
        {
            for ( int i = 0, n = actors.Length; i < n; i++ )
            {
                stack.AddActor( actors[ i ] );
            }
        }

        return AddCell( stack );
    }

    public override bool RemoveActor( Actor actor, bool unfocus )
    {
        if ( !base.RemoveActor( actor, unfocus ) )
        {
            return false;
        }

        if ( GetCell( actor ) != null )
        {
            GetCell( actor )!.Actor = null;
        }

        return true;
    }

    public override Actor? RemoveActorAt( int index, bool unfocus )
    {
        Actor? actor = base.RemoveActorAt( index, unfocus );

        GetCell( actor )!.Actor = null;

        return actor;
    }

    /// <summary>
    /// Removes all actors and cells from the table.
    /// </summary>
    public override void ClearChildren()
    {
        Cell[] cells = Cells.ToArray();

        for ( int i = Cells.Count - 1; i >= 0; i-- )
        {
            cells[ i ].Actor?.Remove();
        }

        CellPool.FreeAll( Cells );

        Cells.Clear();

        Rows    = 0;
        Columns = 0;

        if ( _rowDefaults != null )
        {
            CellPool.Free( _rowDefaults );
        }

        _rowDefaults    = null;
        _implicitEndRow = false;

        base.ClearChildren();
    }

    /// <summary>
    /// Removes all actors and cells from the table (same as <see cref="ClearChildren"/>)
    /// and additionally resets all table properties and cell, column, and row defaults.
    /// </summary>
    public void Reset()
    {
        ClearChildren();

        _padTop    = BackgroundTop;
        _padLeft   = BackgroundLeft;
        _padBottom = BackgroundBottom;
        _padRight  = BackgroundRight;

        Alignment = Align.Center;

        DebugLines( DebugType.None );

        CellDefaults.Reset();

        for ( int i = 0, n = _columnDefaults.Count; i < n; i++ )
        {
            if ( _columnDefaults?[ i ] != null )
            {
                CellPool.Free( _columnDefaults[ i ] );
            }
        }

        _columnDefaults?.Clear();
    }

    /// <summary>
    /// Indicates that subsequent cells should be added to a new row and returns the
    /// cell values that will be used as the defaults for all cells in the new row.
    /// </summary>
    public Cell AddRow()
    {
        if ( Cells.Count > 0 )
        {
            if ( !_implicitEndRow )
            {
                if ( Cells.Peek().EndRow )
                {
                    // Row was already ended.
                    return _rowDefaults;
                }

                EndRow();
            }

            Invalidate();
        }

        _implicitEndRow = false;

        if ( _rowDefaults != null )
        {
            CellPool.Free( _rowDefaults );
        }

        _rowDefaults = ObtainCell();
        _rowDefaults.Clear();

        return _rowDefaults;
    }

    private void EndRow()
    {
        Cell[] cells      = Cells.ToArray();
        var    rowColumns = 0;

        for ( int i = Cells.Count - 1; i >= 0; i-- )
        {
            Cell cell = cells[ i ];

            if ( cell.EndRow )
            {
                break;
            }

            rowColumns += cell.Colspan;
        }

        Columns = Math.Max( Columns, rowColumns );

        Rows++;

        Cells.Peek().EndRow = true;
    }

    /// <summary>
    /// Gets the cell values that will be used as the defaults for all cells in the
    /// specified column. Columns are indexed starting at 0.
    /// </summary>
    /// <param name="column"></param>
    public Cell ColumnDefaults( int column )
    {
        Cell? cell = _columnDefaults.Count > column ? _columnDefaults[ column ] : null;

        if ( cell == null )
        {
            cell = ObtainCell();
            cell.Clear();

            if ( column >= _columnDefaults.Count )
            {
                for ( int i = _columnDefaults.Count; i < column; i++ )
                {
                    _columnDefaults.Add( null! );
                }

                _columnDefaults.Add( cell );
            }
            else
            {
                _columnDefaults[ column ] = cell;
            }
        }

        return cell;
    }

    /// <summary>
    /// Returns the cell for the specified actor in this table, or null.
    /// </summary>
    public Cell? GetCell< T >( T? actor ) where T : Actor
    {
        if ( actor == null )
        {
            throw new ArgumentException( "actor cannot be null." );
        }

        Cell[] cells = Cells.ToArray();

        for ( int i = 0, n = Cells.Count; i < n; i++ )
        {
            Cell c = cells[ i ];

            if ( c.Actor == actor )
            {
                return c;
            }
        }

        return null;
    }

    public override float GetPrefWidth()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        float width = _tablePrefWidth;

        return _background != null ? Math.Max( width, _background.MinWidth ) : width;
    }

    public override float GetPrefHeight()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        float height = _tablePrefHeight;

        return _background != null ? Math.Max( height, _background.MinHeight ) : height;
    }

    public override float GetMinWidth()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _tableMinWidth;
    }

    public override float GetMinHeight()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _tableMinHeight;
    }

    /// <summary>
    /// Sets the padTop, padLeft, padBottom, and padRight around the
    /// table to the specified value.
    /// </summary>
    /// <param name="pad"></param>
    /// <returns></returns>
    public Table Pad( Value pad )
    {
        Guard.Against.Null( pad );

        _padTop      = pad;
        _padLeft     = pad;
        _padBottom   = pad;
        _padRight    = pad;
        _sizeInvalid = true;

        return this;
    }

    public Table Pad( Value top, Value left, Value bottom, Value right )
    {
        _padTop      = top ?? throw new ArgumentException( "top cannot be null." );
        _padLeft     = left ?? throw new ArgumentException( "left cannot be null." );
        _padBottom   = bottom ?? throw new ArgumentException( "bottom cannot be null." );
        _padRight    = right ?? throw new ArgumentException( "right cannot be null." );
        _sizeInvalid = true;

        return this;
    }

    /// <summary>
    /// Padding at the top edge of the table.
    /// </summary>
    /// <param name="padTop"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Table PadTop( Value padTop )
    {
        _padTop = padTop ?? throw new ArgumentException( "padTop cannot be null." );

        _sizeInvalid = true;

        return this;
    }

    /// <summary>
    /// Padding at the left edge of the table.
    /// </summary>
    /// <param name="padLeft"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Table PadLeft( Value padLeft )
    {
        _padLeft = padLeft ?? throw new ArgumentException( "padLeft cannot be null." );

        _sizeInvalid = true;

        return this;
    }

    /// <summary>
    /// Padding at the bottom edge of the table.
    /// </summary>
    /// <param name="padBottom"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Table PadBottom( Value padBottom )
    {
        _padBottom = padBottom ?? throw new ArgumentException( "padBottom cannot be null." );

        _sizeInvalid = true;

        return this;
    }

    /// <summary>
    /// Padding at the right edge of the table.
    /// </summary>
    /// <param name="padRight"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Table PadRight( Value padRight )
    {
        _padRight = padRight ?? throw new ArgumentException( "padRight cannot be null." );

        _sizeInvalid = true;

        return this;
    }

    /// <summary>
    /// Sets the padTop, padLeft, padBottom, and padRight around the table to the specified value.
    /// </summary>
    /// <param name="pad"></param>
    /// <returns></returns>
    public Table Pad( float pad )
    {
        Pad( Value.Fixed.ValueOf( pad ) );

        return this;
    }

    public Table Pad( float top, float left, float bottom, float right )
    {
        _padTop      = Value.Fixed.ValueOf( top );
        _padLeft     = Value.Fixed.ValueOf( left );
        _padBottom   = Value.Fixed.ValueOf( bottom );
        _padRight    = Value.Fixed.ValueOf( right );
        _sizeInvalid = true;

        return this;
    }

    /// <summary>
    /// Padding at the top edge of the table.
    /// </summary>
    /// <param name="padTop"></param>
    /// <returns></returns>
    public Table PadTop( float padTop )
    {
        _padTop = Value.Fixed.ValueOf( padTop );

        _sizeInvalid = true;

        return this;
    }

    /// <summary>
    /// Padding at the left edge of the table.
    /// </summary>
    /// <param name="padLeft"></param>
    /// <returns></returns>
    public Table PadLeft( float padLeft )
    {
        _padLeft = Value.Fixed.ValueOf( padLeft );

        _sizeInvalid = true;

        return this;
    }

    /// <summary>
    /// Padding at the bottom edge of the table.
    /// </summary>
    /// <param name="padBottom"></param>
    /// <returns></returns>
    public Table PadBottom( float padBottom )
    {
        _padBottom = Value.Fixed.ValueOf( padBottom );

        _sizeInvalid = true;

        return this;
    }

    /// <summary>
    /// Padding at the right edge of the table.
    /// </summary>
    /// <param name="padRight"></param>
    /// <returns></returns>
    public Table PadRight( float padRight )
    {
        _padRight = Value.Fixed.ValueOf( padRight );

        _sizeInvalid = true;

        return this;
    }

    /// <summary>
    /// Alignment of the logical table within the table actor.
    /// Set to <see cref="Align.Center"/>, <see cref="Align.Top"/>, <see cref="Align.Bottom"/>,
    /// <see cref="Align.Left"/>, <see cref="Align.Right"/>, or any combination of those.
    /// </summary>
    /// <param name="align"></param>
    /// <returns></returns>
    public Table SetAlignment( Align align )
    {
        Alignment = align;

        return this;
    }

    /// <summary>
    /// Sets the alignment of the logical table within the table actor to
    /// <see cref="Align.Center"/>. This clears any other alignment.
    /// </summary>
    /// <returns></returns>
    public Table Center()
    {
        Alignment = Align.Center;

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Top"/> and clears <see cref="Align.Bottom"/> for
    /// the alignment of the logical table within the table actor.
    /// </summary>
    /// <returns></returns>
    public Table AddTopAlignment()
    {
        Alignment |= Align.Top;
        Alignment &= ~Align.Bottom;

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Left"/> and clears <see cref="Align.Right"/> for
    /// the alignment of the logical table within the table actor.
    /// </summary>
    /// <returns></returns>
    public Table AddLeftAlignment()
    {
        Alignment |= Align.Left;
        Alignment &= ~Align.Right;

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Bottom"/> and clears <see cref="Align.Top"/> for the
    /// alignment of the logical table within the table actor.
    /// </summary>
    public Table AddBottomAlignment()
    {
        Alignment |= Align.Bottom;
        Alignment &= ~Align.Top;

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Right"/> and clears <see cref="Align.Left"/> for
    /// the alignment of the logical table within the table actor.
    /// </summary>
    public Table AddRightAlignment()
    {
        Alignment |= Align.Right;
        Alignment &= ~Align.Left;

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

    /// <summary>
    /// Returns <see cref="_padLeft"/> plus <see cref="_padRight"/>.
    /// </summary>
    /// <returns></returns>
    public float GetPadX()
    {
        return _padLeft.Get( this ) + _padRight.Get( this );
    }

    /// <summary>
    /// Returns <see cref="_padTop"/> plus <see cref="_padBottom"/>.
    /// </summary>
    /// <returns></returns>
    public float GetPadY()
    {
        return _padTop.Get( this ) + _padBottom.Get( this );
    }

    /// <summary>
    /// Returns the row index for the y coordinate, or -1 if not over a row.
    /// </summary>
    /// <param name="y"> The y coordinate, where 0 is the top of the table. </param>
    public int GetRow( float y )
    {
        int n = Cells.Count;

        if ( n == 0 )
        {
            return -1;
        }

        y += GetPadTop();

        Cell[] cells = Cells.ToArray();

        for ( int i = 0, row = 0; i < n; )
        {
            Cell c = cells[ i++ ];

            if ( ( c.ActorY + c.ComputedPadTop ) < y )
            {
                return row;
            }

            if ( c.EndRow )
            {
                row++;
            }
        }

        return -1;
    }

    /// <summary>
    /// Returns the height of the specified row, or 0 if the table layout has not been validated.
    /// </summary>
    public float GetRowHeight( int rowIndex )
    {
        return _rowHeight[ rowIndex ];
    }

    /// <summary>
    /// Returns the min height of the specified row.
    /// </summary>
    /// <param name="rowIndex">The row number</param>
    public float GetRowMinHeight( int rowIndex )
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _rowMinHeight[ rowIndex ];
    }

    /// <summary>
    /// Returns the pref height of the specified row.
    /// </summary>
    /// <param name="rowIndex">The row number</param>
    public float GetRowPrefHeight( int rowIndex )
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _rowPrefHeight[ rowIndex ];
    }

    /// <summary>
    /// Returns the width of the specified column, or 0 if the
    /// table layout has not been validated.
    /// </summary>
    /// <param name="columnIndex">The column number</param>
    public float GetColumnWidth( int columnIndex )
    {
        return _columnWidth[ columnIndex ];
    }

    /// <summary>
    /// Returns the min height of the specified column.
    /// </summary>
    /// <param name="columnIndex">The column number</param>
    public float GetColumnMinWidth( int columnIndex )
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _columnMinWidth[ columnIndex ];
    }

    /// <summary>
    /// Returns the pref height of the specified column.
    /// </summary>
    /// <param name="columnIndex">The column number</param>
    public float GetColumnPrefWidth( int columnIndex )
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _columnPrefWidth[ columnIndex ];
    }

    private float[] EnsureSize( float[]? array, int size )
    {
        if ( ( array == null ) || ( array.Length < size ) )
        {
            return new float[ size ];
        }

        Array.Fill( array, 0f, 0, size );

        return array;
    }

    private void ComputeSize()
    {
        _sizeInvalid = false;

        Cell[] cells     = Cells.ToArray();
        int    cellCount = Cells.Count;

        // Implicitly end the row for layout purposes.
        if ( ( cellCount > 0 ) && !cells[ cellCount - 1 ].EndRow )
        {
            EndRow();

            _implicitEndRow = true;
        }

        int columns = Columns;
        int rows    = Rows;

        _columnMinWidth  = EnsureSize( _columnMinWidth, columns );
        _rowMinHeight    = EnsureSize( _rowMinHeight, rows );
        _columnPrefWidth = EnsureSize( _columnPrefWidth, columns );
        _rowPrefHeight   = EnsureSize( _rowPrefHeight, rows );
        _expandWidth     = EnsureSize( _expandWidth, columns );
        _expandHeight    = EnsureSize( _expandHeight, rows );

        float[] columnMinWidth  = _columnMinWidth.ToArray();
        float[] rowMinHeight    = _rowMinHeight.ToArray();
        float[] columnPrefWidth = _columnPrefWidth.ToArray();
        float[] rowPrefHeight   = _rowPrefHeight.ToArray();
        float[] expandWidth     = _expandWidth.ToArray();
        float[] expandHeight    = _expandHeight.ToArray();

        float? spaceRightLast = 0;

        for ( var i = 0; i < cellCount; i++ )
        {
            Cell c = cells[ i ];

            // Collect rows that expand and colspan=1 columns that expand.
            if ( ( c.ExpandY != 0 ) && ( expandHeight[ c.Row ] == 0 ) )
            {
                expandHeight[ c.Row ] = c.ExpandY;
            }

            if ( ( c.Colspan == 1 ) && ( c.ExpandX != 0 ) && ( expandWidth[ c.Column ] == 0 ) )
            {
                expandWidth[ c.Column ] = c.ExpandX;
            }

            // Compute combined padding/spacing for cells. Spacing between actors
            // isn't additive, the larger is used. Also, no spacing around edges.
            c.ComputedPadLeft = ( c.PadLeft.Get( c.Actor )
                                + ( c.Column == 0
                                      ? 0
                                      : Math.Max( 0f,
                                                  ( float )( c.SpaceLeft.Get( c.Actor )
                                                           - spaceRightLast ) ) ) )!;

            c.ComputedPadTop = c.PadTop.Get( c.Actor );

            if ( c.CellAboveIndex != -1 )
            {
                Cell above = cells[ c.CellAboveIndex ];

                c.ComputedPadTop += Math.Max( 0,
                                              ( c.SpaceTop.Get( c.Actor )
                                              - above.SpaceBottom.Get( c.Actor ) )! );
            }

            float? spaceRight = c.SpaceRight.Get( c.Actor );

            c.ComputedPadRight = ( float )( c.PadRight.Get( c.Actor )
                                          + ( ( c.Column + c.Colspan ) == columns ? 0f : spaceRight ) );

            c.ComputedPadBottom = ( c.PadBottom.Get( c.Actor )
                                  + ( c.Row == ( rows - 1 ) ? 0 : c.SpaceBottom.Get( c.Actor ) ) )!;

            spaceRightLast = spaceRight;

            // Determine minimum and preferred cell sizes.
            float prefWidth  = c.PrefWidth.Get( c.Actor );
            float prefHeight = c.PrefHeight.Get( c.Actor );
            float minWidth   = c.MinWidth.Get( c.Actor );
            float minHeight  = c.MinHeight.Get( c.Actor );
            float maxWidth   = c.MaxWidth.Get( c.Actor );
            float maxHeight  = c.MaxHeight.Get( c.Actor );

            if ( prefWidth < minWidth )
            {
                prefWidth = minWidth;
            }

            if ( prefHeight < minHeight )
            {
                prefHeight = minHeight;
            }

            if ( ( maxWidth > 0 ) && ( prefWidth > maxWidth ) )
            {
                prefWidth = maxWidth;
            }

            if ( ( maxHeight > 0 ) && ( prefHeight > maxHeight ) )
            {
                prefHeight = maxHeight;
            }

            if ( Round )
            {
                minWidth   = ( float )Math.Ceiling( minWidth );
                minHeight  = ( float )Math.Ceiling( minHeight );
                prefWidth  = ( float )Math.Ceiling( prefWidth );
                prefHeight = ( float )Math.Ceiling( prefHeight );
            }

            if ( c.Colspan == 1 )
            {
                // Spanned column min and pref width is added later.
                float hpadding = c.ComputedPadLeft + c.ComputedPadRight;

                columnPrefWidth[ c.Column ] = Math.Max( columnPrefWidth[ c.Column ],
                                                        ( prefWidth + hpadding )! );

                columnMinWidth[ c.Column ] = Math.Max( columnMinWidth[ c.Column ],
                                                       ( minWidth + hpadding )! );
            }

            float vpadding = c.ComputedPadTop + c.ComputedPadBottom;

            rowPrefHeight[ c.Row ] = Math.Max( rowPrefHeight[ c.Row ], ( prefHeight + vpadding )! );
            rowMinHeight[ c.Row ]  = Math.Max( rowMinHeight[ c.Row ], ( minHeight + vpadding )! );
        }

        float uniformMinWidth  = 0, uniformMinHeight  = 0;
        float uniformPrefWidth = 0, uniformPrefHeight = 0;

        for ( var i = 0; i < cellCount; i++ )
        {
            Cell c      = cells[ i ];
            int  column = c.Column;

            // Colspan with expand will expand all spanned columns
            // if none of the spanned columns have expand.
            int expandX = c.ExpandX;

            if ( expandX != 0 )
            {
                int nn = column + c.Colspan;

                for ( int ii = column; ii < nn; ii++ )
                {
                    if ( expandWidth[ ii ] != 0 )
                    {
                        break;
                    }
                }

                for ( int ii = column; ii < nn; ii++ )
                {
                    expandWidth[ ii ] = expandX;
                }
            }

            // Collect uniform sizes.
            if ( c is { UniformX: true, Colspan: 1 } )
            {
                float hpadding = c.ComputedPadLeft + c.ComputedPadRight;

                uniformMinWidth  = Math.Max( uniformMinWidth, columnMinWidth[ column ] - hpadding );
                uniformPrefWidth = Math.Max( uniformPrefWidth, columnPrefWidth[ column ] - hpadding );
            }

            if ( c.UniformY )
            {
                float vpadding = c.ComputedPadTop + c.ComputedPadBottom;

                uniformMinHeight  = Math.Max( uniformMinHeight, rowMinHeight[ c.Row ] - vpadding );
                uniformPrefHeight = Math.Max( uniformPrefHeight, rowPrefHeight[ c.Row ] - vpadding );
            }
        }

        // Size uniform cells to the same width/height.
        if ( ( uniformPrefWidth > 0 ) || ( uniformPrefHeight > 0 ) )
        {
            for ( var i = 0; i < cellCount; i++ )
            {
                Cell c = cells[ i ];

                if ( ( uniformPrefWidth > 0 ) && c is { UniformX: true, Colspan: 1 } )
                {
                    float computedPadding = c.ComputedPadLeft + c.ComputedPadRight;

                    columnMinWidth[ c.Column ]  = uniformMinWidth + computedPadding;
                    columnPrefWidth[ c.Column ] = uniformPrefWidth + computedPadding;
                }

                if ( ( uniformPrefHeight > 0 ) && c.UniformY )
                {
                    float computedPadding = c.ComputedPadTop + c.ComputedPadBottom;

                    rowMinHeight[ c.Row ]  = uniformMinHeight + computedPadding;
                    rowPrefHeight[ c.Row ] = uniformPrefHeight + computedPadding;
                }
            }
        }

        // Distribute any additional min and pref width added by colspanned cells to the columns spanned.
        for ( var i = 0; i < cellCount; i++ )
        {
            Cell c       = cells[ i ];
            int  colspan = c.Colspan;

            if ( colspan <= 1 )
            {
                continue;
            }

            int   column    = c.Column;
            float minWidth  = c.MinWidth.Get( c.Actor );
            float prefWidth = c.PrefWidth.Get( c.Actor );
            float maxWidth  = c.MaxWidth.Get( c.Actor );

            if ( prefWidth < minWidth )
            {
                prefWidth = minWidth;
            }

            if ( ( maxWidth > 0 ) && ( prefWidth > maxWidth ) )
            {
                prefWidth = maxWidth;
            }

            if ( Round )
            {
                minWidth  = ( float )Math.Ceiling( minWidth );
                prefWidth = ( float )Math.Ceiling( prefWidth );
            }

            float spannedMinWidth  = -( c.ComputedPadLeft + c.ComputedPadRight );
            float spannedPrefWidth = spannedMinWidth;
            float totalExpandWidth = 0;


            for ( int ii = column, nn = ii + colspan; ii < nn; ii++ )
            {
                spannedMinWidth  += columnMinWidth[ ii ];
                spannedPrefWidth += columnPrefWidth[ ii ];

                // Distribute extra space using expand, if any columns have expand.
                totalExpandWidth += expandWidth[ ii ];
            }

            float extraMinWidth  = Math.Max( 0, ( minWidth - spannedMinWidth )! );
            float extraPrefWidth = Math.Max( 0, ( prefWidth - spannedPrefWidth )! );

            for ( int ii = column, nn = ii + colspan; ii < nn; ii++ )
            {
                float ratio = totalExpandWidth == 0
                    ? ( 1f / colspan )
                    : ( expandWidth[ ii ] / totalExpandWidth );

                columnMinWidth[ ii ]  += extraMinWidth * ratio;
                columnPrefWidth[ ii ] += extraPrefWidth * ratio;
            }
        }

        // Determine table min and pref size.
        // Horizontal and Vertical padding.
        float hpad = _padLeft.Get( this ) + _padRight.Get( this );
        float vpad = _padTop.Get( this ) + _padBottom.Get( this );

        _tableMinWidth  = hpad;
        _tablePrefWidth = hpad;

        for ( var i = 0; i < columns; i++ )
        {
            _tableMinWidth  += columnMinWidth[ i ];
            _tablePrefWidth += columnPrefWidth[ i ];
        }

        _tableMinHeight  = vpad;
        _tablePrefHeight = vpad;

        for ( var i = 0; i < rows; i++ )
        {
            _tableMinHeight  += rowMinHeight[ i ];
            _tablePrefHeight += Math.Max( rowMinHeight[ i ], rowPrefHeight[ i ] );
        }

        _tablePrefWidth  = Math.Max( _tableMinWidth, _tablePrefWidth );
        _tablePrefHeight = Math.Max( _tableMinHeight, _tablePrefHeight );
    }

    /// <summary>
    /// Positions and sizes children of the table using the cell associated
    /// with each child. The values given are the position within the parent
    /// and size of the table.
    /// </summary>
    public override void Layout()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        float layoutWidth  = Width;
        float layoutHeight = Height;
        int   columns      = Columns;
        int   rows         = Rows;

        _columnWidth = EnsureSize( _columnWidth, columns );
        _rowHeight   = EnsureSize( _rowHeight, rows );

        float[] columnWidth = _columnWidth.ToArray();
        float[] rowHeight   = _rowHeight.ToArray();
        float   padLeft     = _padLeft.Get( this );
        float   hpadding    = padLeft + _padRight.Get( this );
        float   padTop      = _padTop.Get( this );
        float   vpadding    = padTop + _padBottom.Get( this );

        // Size columns and rows between min and pref size using (preferred - min)
        // size to weight distribution of extra space.
        float[] columnWeightedWidth;
        float   totalGrowWidth = _tablePrefWidth - _tableMinWidth;

        if ( totalGrowWidth <= 0 )
        {
            columnWeightedWidth = _columnMinWidth.ToArray();
        }
        else
        {
            float extraWidth = Math.Min( totalGrowWidth, Math.Max( 0, layoutWidth - _tableMinWidth ) );

            columnWeightedWidth = _columnWeightedWidth = EnsureSize( _columnWeightedWidth, columns );

            float[] columnMinWidth  = _columnMinWidth.ToArray();
            float[] columnPrefWidth = _columnPrefWidth.ToArray();

            for ( var i = 0; i < columns; i++ )
            {
                float growWidth = columnPrefWidth[ i ] - columnMinWidth[ i ];
                float growRatio = growWidth / totalGrowWidth;

                columnWeightedWidth[ i ] = columnMinWidth[ i ] + ( extraWidth * growRatio );
            }
        }

        float[] rowWeightedHeight;
        float   totalGrowHeight = _tablePrefHeight - _tableMinHeight;

        if ( totalGrowHeight <= 0 )
        {
            rowWeightedHeight = _rowMinHeight.ToArray();
        }
        else
        {
            rowWeightedHeight = _rowWeightedHeight = EnsureSize( _rowWeightedHeight, rows );

            float   extraHeight   = Math.Min( totalGrowHeight, Math.Max( 0, layoutHeight - _tableMinHeight ) );
            float[] rowMinHeight  = _rowMinHeight.ToArray();
            float[] rowPrefHeight = _rowPrefHeight.ToArray();

            for ( var i = 0; i < rows; i++ )
            {
                float growHeight = rowPrefHeight[ i ] - rowMinHeight[ i ];
                float growRatio  = growHeight / totalGrowHeight;

                rowWeightedHeight[ i ] = rowMinHeight[ i ] + ( extraHeight * growRatio );
            }
        }

        // --------------------------------------------------------------------
        // 1. Determine actor and cell sizes (before expand or fill).
        Cell[] cells     = Cells.ToArray();
        int    cellCount = cells.Length;

        for ( var i = 0; i < cellCount; i++ )
        {
            Cell   c      = cells[ i ];
            int    column = c.Column;
            int    row    = c.Row;
            Actor? a      = c.Actor;

            float spannedWeightedWidth = 0;
            int   colspan              = c.Colspan;

            for ( int ii = column, nn = ii + colspan; ii < nn; ii++ )
            {
                spannedWeightedWidth += columnWeightedWidth[ ii ];
            }

            float weightedHeight = rowWeightedHeight[ row ];

            float prefWidth  = c.PrefWidth.Get( a );
            float prefHeight = c.PrefHeight.Get( a );
            float minWidth   = c.MinWidth.Get( a );
            float minHeight  = c.MinHeight.Get( a );
            float maxWidth   = c.MaxWidth.Get( a );
            float maxHeight  = c.MaxHeight.Get( a );

            if ( prefWidth < minWidth )
            {
                prefWidth = minWidth;
            }

            if ( prefHeight < minHeight )
            {
                prefHeight = minHeight;
            }

            if ( ( maxWidth > 0 ) && ( prefWidth > maxWidth ) )
            {
                prefWidth = maxWidth;
            }

            if ( ( maxHeight > 0 ) && ( prefHeight > maxHeight ) )
            {
                prefHeight = maxHeight;
            }

            c.ActorWidth = Math.Min( Math.Max( 0, spannedWeightedWidth - c.ComputedPadLeft - c.ComputedPadRight ),
                                     prefWidth );
            c.ActorHeight = Math.Min( Math.Max( 0, weightedHeight - c.ComputedPadTop - c.ComputedPadBottom ),
                                      prefHeight );

            if ( colspan == 1 )
            {
                columnWidth[ column ] = Math.Max( columnWidth[ column ], spannedWeightedWidth );
            }

            rowHeight[ row ] = Math.Max( rowHeight[ row ], weightedHeight );
        }

        // --------------------------------------------------------------------
        // 2. Distribute remaining space to any expanding columns/rows.
        float[] expandWidth  = _expandWidth.ToArray();
        float[] expandHeight = _expandHeight.ToArray();
        float   totalExpand  = 0;

        for ( var i = 0; i < columns; i++ )
        {
            totalExpand += expandWidth[ i ];
        }

        if ( totalExpand > 0 )
        {
            float extra = layoutWidth - hpadding;

            for ( var i = 0; i < columns; i++ )
            {
                extra -= columnWidth[ i ];
            }

            if ( extra > 0 )
            {
                var used      = 0.0f;
                var lastIndex = 0;

                for ( var i = 0; i < columns; i++ )
                {
                    if ( expandWidth[ i ] <= 0f )
                    {
                        continue;
                    }

                    float amount = extra * expandWidth[ i ] / totalExpand;

                    columnWidth[ i ] += amount;

                    used      += amount;
                    lastIndex =  i;
                }

                if ( lastIndex >= 0 )
                {
                    columnWidth[ lastIndex ] += extra - used;
                }
            }
        }

        totalExpand = 0;

        for ( var i = 0; i < rows; i++ )
        {
            totalExpand += expandHeight[ i ];
        }

        if ( totalExpand > 0 )
        {
            float extra = layoutHeight - vpadding;

            for ( var i = 0; i < rows; i++ )
            {
                extra -= rowHeight[ i ];
            }

            if ( extra > 0 )
            {
                var used      = 0f;
                var lastIndex = 0;

                for ( var i = 0; i < rows; i++ )
                {
                    if ( expandHeight[ i ] <= 0f )
                    {
                        continue;
                    }

                    float amount = extra * expandHeight[ i ] / totalExpand;
                    rowHeight[ i ] += amount;
                    used           += amount;
                    lastIndex      =  i;
                }

                if ( lastIndex >= 0 )
                {
                    rowHeight[ lastIndex ] += extra - used;
                }
            }
        }

        // --------------------------------------------------------------------
        // 3. Distribute any additional width added by colspanned cells to the columns spanned.
        for ( var i = 0; i < cellCount; i++ )
        {
            Cell c       = cells[ i ];
            int  colspan = c.Colspan;

            if ( colspan <= 1 )
            {
                continue;
            }

            float extraWidth = 0;

            for ( int column = c.Column, nn = column + colspan; column < nn; column++ )
            {
                extraWidth += columnWeightedWidth[ column ] - columnWidth[ column ];
            }

            extraWidth -= Math.Max( 0, c.ComputedPadLeft + c.ComputedPadRight );

            extraWidth /= colspan;

            if ( extraWidth > 0 )
            {
                for ( int column = c.Column, nn = column + colspan; column < nn; column++ )
                {
                    columnWidth[ column ] += extraWidth;
                }
            }
        }

        // --------------------------------------------------------------------
        // 4. Determine table size.
        float tableWidth  = hpadding;
        float tableHeight = vpadding;

        for ( var i = 0; i < columns; i++ )
        {
            tableWidth += columnWidth[ i ];
        }

        for ( var i = 0; i < rows; i++ )
        {
            tableHeight += rowHeight[ i ];
        }

        // Position table within the container.
        float x = padLeft;

        if ( ( Alignment & Align.Right ) != 0 )
        {
            x += layoutWidth - tableWidth;
        }
        else if ( ( Alignment & Align.Left ) == 0 )
        {
            x += ( layoutWidth - tableWidth ) / 2;
        }

        float y = padTop;

        if ( ( Alignment & Align.Bottom ) != 0 )
        {
            y += layoutHeight - tableHeight;
        }
        else if ( ( Alignment & Align.Top ) == 0 )
        {
            y += ( layoutHeight - tableHeight ) / 2;
        }

        // Size and position actors within cells.
        float currentX = x, currentY = y;

        for ( var i = 0; i < cellCount; i++ )
        {
            Cell c = cells[ i ];

            float spannedCellWidth = 0;

            for ( int column = c.Column, nn = ( column + c.Colspan ); column < nn; column++ )
            {
                spannedCellWidth += columnWidth[ column ];
            }

            spannedCellWidth -= c.ComputedPadLeft + c.ComputedPadRight;

            currentX += c.ComputedPadLeft;

            float fillX = c.FillX;
            float fillY = c.FillY;

            if ( fillX > 0 )
            {
                c.ActorWidth = Math.Max( spannedCellWidth * fillX, c.MinWidth.Get( c.Actor ) );

                float maxWidth = c.MaxWidth.Get( c.Actor );

                if ( maxWidth > 0 )
                {
                    c.ActorWidth = Math.Min( c.ActorWidth, maxWidth );
                }
            }

            if ( fillY > 0 )
            {
                c.ActorHeight = Math.Max( ( rowHeight[ c.Row ] * fillY ) - c.ComputedPadTop - c.ComputedPadBottom,
                                          c.MinHeight.Get( c.Actor ) );

                float maxHeight = c.MaxHeight.Get( c.Actor );

                if ( maxHeight > 0 )
                {
                    c.ActorHeight = Math.Min( c.ActorHeight, maxHeight );
                }
            }

            Align cellAlign = c.Alignment;

            if ( ( cellAlign & Align.Left ) != 0 )
            {
                c.ActorX = currentX;
            }
            else if ( ( cellAlign & Align.Right ) != 0 )
            {
                c.ActorX = currentX + spannedCellWidth - c.ActorWidth;
            }
            else
            {
                c.ActorX = currentX + ( ( spannedCellWidth - c.ActorWidth ) / 2 );
            }

            if ( ( cellAlign & Align.Top ) != 0 )
            {
                c.ActorY = c.ComputedPadTop;
            }
            else if ( ( cellAlign & Align.Bottom ) != 0 )
            {
                c.ActorY = rowHeight[ c.Row ] - c.ActorHeight - c.ComputedPadBottom;
            }
            else
            {
                c.ActorY = ( rowHeight[ c.Row ] - c.ActorHeight + c.ComputedPadTop - c.ComputedPadBottom ) / 2;
            }

            c.ActorY = layoutHeight - currentY - c.ActorY - c.ActorHeight;

            if ( Round )
            {
                c.ActorWidth  = ( float )Math.Ceiling( c.ActorWidth );
                c.ActorHeight = ( float )Math.Ceiling( c.ActorHeight );
                c.ActorX      = ( float )Math.Floor( c.ActorX );
                c.ActorY      = ( float )Math.Floor( c.ActorY );
            }

            c.Actor?.SetBounds( c.ActorX, c.ActorY, c.ActorWidth, c.ActorHeight );

            if ( c.EndRow )
            {
                currentX =  x;
                currentY += rowHeight[ c.Row ];
            }
            else
            {
                currentX += spannedCellWidth + c.ComputedPadRight;
            }
        }

        // Store final computed widths/heights back to fields for external access.
        Array.Copy( columnWidth, _columnWidth, columns );
        Array.Copy( rowHeight, _rowHeight, rows );

        // Validate all children (some may not be in cells).
        for ( int i = 0, n = Children.Size; i < n; i++ )
        {
            if ( Children.GetAt( i ) is ILayout child )
            {
                child.Validate();
            }
        }

        // Store debug rectangles.
        if ( TableDebug != DebugType.None )
        {
            AddDebugRects( x, y, tableWidth - hpadding, tableHeight - vpadding );
        }
    }

    /// <summary>
    /// Creates and returns a new <see cref="Cell"/> object. This method is primarily
    /// intended for use by the Pooling system to efficiently manage <see cref="Cell"/>
    /// instances.
    /// </summary>
    public static Cell GetNewObject()
    {
        return new Cell();
    }

    // ========================================================================

    [PublicAPI]
    public class DebugRect : Rectangle
    {
        public Pool< DebugRect > Pool  { get; }
        public Color             Color { get; set; }

        public DebugRect()
        {
            Color = Color.Red;

            Pool = new Pool< DebugRect >
            {
                NewObjectFactory = GetNewDebugRect
            };
        }

        public DebugRect GetNewDebugRect()
        {
            return new DebugRect();
        }
    }

    // ========================================================================

    #region Debugging

    public void SetDebug( bool enabled )
    {
        DebugLines( enabled ? DebugType.All : DebugType.None );
    }

    public override Table EnableDebug()
    {
        base.EnableDebug();

        return this;
    }

    public override Table DebugAll()
    {
        base.DebugAll();

        return this;
    }

    /// <summary>
    /// Turns on table debug lines.
    /// </summary>
    public Table DebugTable()
    {
        base.SetDebug( true, false );

        if ( TableDebug != DebugType.Table )
        {
            TableDebug = DebugType.Table;
            Invalidate();
        }

        return this;
    }

    /// <summary>
    /// Turns on cell debug lines.
    /// </summary>
    public Table DebugCell()
    {
        base.SetDebug( true, false );

        if ( TableDebug != DebugType.Cell )
        {
            TableDebug = DebugType.Cell;
            Invalidate();
        }

        return this;
    }

    /// <summary>
    /// Turns on actor debug lines.
    /// </summary>
    public Table DebugActor()
    {
        base.SetDebug( true, false );

        if ( TableDebug != DebugType.Actor )
        {
            TableDebug = DebugType.Actor;
            Invalidate();
        }

        return this;
    }

    /// <summary>
    /// Turns debug lines on or off.
    /// </summary>
    public Table DebugLines( DebugType debug )
    {
        base.SetDebug( debug != DebugType.None, false );

        if ( TableDebug != debug )
        {
            TableDebug = debug;

            if ( debug == DebugType.None )
            {
                ClearDebugRects();
            }
            else
            {
                Invalidate();
            }
        }

        return this;
    }

    private void ClearDebugRects()
    {
//        _debugRects ??= [ ];

//TODO:        DebugRect.Pool.FreeAll( _debugRects );

        _debugRects?.Clear();
    }

    private void AddDebugRect( float x, float y, float w, float h, Color color )
    {
        var rect = new DebugRect
        {
            Color = color
        };

        rect.Set( x, y, w, h );

        _debugRects?.Add( rect );
    }

    private void AddDebugRects( float currentX, float currentY, float width, float height )
    {
        ClearDebugRects();

        if ( TableDebug is DebugType.Table or DebugType.All )
        {
            AddDebugRect( 0, 0, Width, Height, DebugTableColor );
            AddDebugRect( currentX, currentY, width, height, DebugTableColor );
        }

        float x = currentX;

        for ( int i = 0, n = Cells.Count; i < n; i++ )
        {
            Cell c = Cells[ i ];

            // Actor bounds.
            if ( TableDebug is DebugType.Actor or DebugType.All )
            {
                AddDebugRect( c.ActorX, c.ActorY, c.ActorWidth, c.ActorHeight, DebugActorColor );
            }

            // Cell bounds.
            float spannedCellWidth = 0;

            for ( int column = c.Column, nn = column + c.Colspan; column < nn; column++ )
            {
                spannedCellWidth += _columnWidth[ column ];
            }

            spannedCellWidth -= c.ComputedPadLeft + c.ComputedPadRight;
            currentX         += c.ComputedPadLeft;

            if ( TableDebug is DebugType.Cell or DebugType.All )
            {
                AddDebugRect( currentX,
                              currentY + c.ComputedPadTop,
                              spannedCellWidth,
                              _rowHeight[ c.Row ] - c.ComputedPadTop - c.ComputedPadBottom,
                              DebugCellColor );
            }

            if ( c.EndRow )
            {
                currentX =  x;
                currentY += _rowHeight[ c.Row ];
            }
            else
            {
                currentX += spannedCellWidth + c.ComputedPadRight;
            }
        }
    }

    public override void DrawDebug( ShapeRenderer shapes )
    {
        if ( Transform )
        {
            ApplyTransform( shapes, ComputeTransform() );
            DrawDebugRects( shapes );

            if ( _clip )
            {
                shapes.Flush();

                var   x      = 0.0f;
                var   y      = 0.0f;
                float width  = Width;
                float height = Height;

                if ( _background != null )
                {
                    x      =  _padLeft.Get( this );
                    y      =  _padBottom.Get( this );
                    width  -= x + _padRight.Get( this );
                    height -= y + _padTop.Get( this );
                }

                if ( ClipBegin( x, y, width, height ) )
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
            DrawDebugRects( shapes );
            base.DrawDebug( shapes );
        }
    }

    private void DrawDebugRects( ShapeRenderer shapes )
    {
        if ( ( _debugRects == null ) || !DebugActive )
        {
            return;
        }

        shapes.Set( ShapeRenderer.ShapeRenderType.Lines );

        if ( Stage != null )
        {
            shapes.Color = Stage.DebugColor;
        }

        float x = 0;
        float y = 0;

        if ( !Transform )
        {
            x = X;
            y = Y;
        }

        for ( int i = 0, n = _debugRects.Count; i < n; i++ )
        {
            DebugRect debugRect = _debugRects[ i ];

            shapes.Color = debugRect.Color;
            shapes.Rect( x + debugRect.X, y + debugRect.Y, debugRect.Width, debugRect.Height );
        }
    }

    #endregion Debugging

    // ========================================================================

    #region backgrounds

    /// <summary>
    /// Value that is the top padding of the table's background.
    /// </summary>
    public static readonly Value BackgroundTop = new BackgroundTopDelegate();

    /// <summary>
    /// Value that is the bottom padding of the table's background.
    /// </summary>
    public static readonly Value BackgroundBottom = new BackgroundBottomDelegate();

    /// <summary>
    /// Value that is the left padding of the table's background.
    /// </summary>
    public static readonly Value BackgroundLeft = new BackgroundLeftDelegate();

    /// <summary>
    /// Value that is the right padding of the table's background.
    /// </summary>
    public static readonly Value BackgroundRight = new BackgroundRightDelegate();

    private class BackgroundTopDelegate : Value
    {
        public override float Get( Actor? context = null )
        {
            return ( ( Table? )context )?.GetBackground()?.TopHeight ?? 0;
        }
    }

    private class BackgroundBottomDelegate : Value
    {
        public override float Get( Actor? context = null )
        {
            return ( ( Table? )context )?.GetBackground()?.BottomHeight ?? 0;
        }
    }

    private class BackgroundLeftDelegate : Value
    {
        public override float Get( Actor? context = null )
        {
            return ( ( Table? )context )?.GetBackground()?.LeftWidth ?? 0;
        }
    }

    private class BackgroundRightDelegate : Value
    {
        public override float Get( Actor? context = null )
        {
            return ( ( Table? )context )?.GetBackground()?.RightWidth ?? 0;
        }
    }

    #endregion
}

// ============================================================================
// ============================================================================