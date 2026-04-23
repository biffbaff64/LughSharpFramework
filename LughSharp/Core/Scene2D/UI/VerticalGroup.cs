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
using LughSharp.Core.Graphics.Utils;
using LughSharp.Core.Scene2D.Utils;

namespace LughSharp.Core.Scene2D.UI;

/// <summary>
/// A group that lays out its children top to bottom vertically, with optional wrapping.
/// <see cref="Group.Children"/> can be sorted to change the order of the actors (eg
/// <see cref="Actor.SetZIndex(int)"/>). This can be easier than using <see cref="Table"/>
/// when actors need to be inserted into or removed from the middle of the group.
/// <para>
/// <see cref="Invalidate()"/> must be called after changing the children order.
/// </para>
/// <para>
/// The preferred width is the largest preferred width of any child. The preferred height
/// is the sum of the children's preferred heights plus spacing. The preferred size is
/// slightly different when <see cref="Wrapping"/> is enabled. The min size is the
/// preferred size and the max size is 0.
/// </para>
/// <para>
/// Widgets are sized using their <see cref="ILayout.PrefHeight"/>, so widgets which return
/// 0 as their preferred height will be given a height of 0.
/// </para>
/// </summary>
[PublicAPI]
public class VerticalGroup : WidgetGroup
{
    /// <summary>
    /// Sets the vertical space between children.
    /// </summary>
    public float Space { get; set; }

    /// <summary>
    /// Sets the horizontal space between columns when wrap is enabled.
    /// </summary>
    public float WrapSpace { get; set; }

    public float PadTop { get; set; }
    public float PadBottom { get; set; }
    public float PadLeft { get; set; }
    public float PadRight { get; set; }

    /// <summary>
    /// Sets the alignment of all widgets within the vertical group. Set to
    /// <see cref="Align.Center"/>, <see cref="Align.Top"/>,
    /// <see cref="Align.Bottom"/>,
    /// <see cref="Align.Left"/>, <see cref="Align.Right"/>, or any combination of those.
    /// </summary>
    public Align Alignment { get; set; } = Align.Top;

    public float Fill { get; set; } = 1f;

    public bool Expand { get; set; }

    /// <summary>
    /// If false, the widgets are arranged in a single column and the preferred height
    /// is the widget heights plus spacing.
    /// <para>
    /// If true, the widgets will wrap using the height of the vertical group. The preferred
    /// height of the group will be 0 as it is expected that something external will set the
    /// height of the group. Widgets are sized to their preferred height unless it is larger
    /// than the group's height, in which case they are sized to the group's height but not
    /// less than their minimum height.
    /// </para>
    /// <para>
    /// Default is false.
    /// </para>
    /// <para>
    /// When wrap is enabled, the group's preferred width depends on the height of the group.
    /// In some cases the parent of the group will need to layout twice: once to set the
    /// height of the group and a second time to adjust to the group's new preferred width.
    /// </para>
    /// </summary>
    public bool Wrapping { get; set; }

    // ========================================================================

    private Align          _columnAlign;
    private List< float >? _columnSizes;
    private float          _lastPrefWidth;
    private float          _prefHeight;
    private float          _prefWidth;
    private bool           _reverse;
    private bool           _round       = true;
    private bool           _sizeInvalid = true;

    // ========================================================================

    public VerticalGroup()
    {
        Touchable = Touchable.ChildrenOnly;
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
        base.Invalidate();
        _sizeInvalid = true;
    }

    private void ComputeSize()
    {
        _sizeInvalid = false;

        SnapshotArrayList< Actor? > children = Children;

        int n = children.Size;

        _prefWidth = 0;

        if ( Wrapping )
        {
            _prefHeight = 0;

            if ( _columnSizes == null )
            {
                _columnSizes = [ ];
            }
            else
            {
                _columnSizes.Clear();
            }

            List< float >? columnSizes = _columnSizes;

            float space       = Space;
            float wrapSpace   = WrapSpace;
            float pad         = PadTop + PadBottom;
            float groupHeight = Height - pad;

            float x           = 0;
            float y           = 0;
            float columnWidth = 0;
            var   i           = 0;
            var   incr        = 1;

            if ( _reverse )
            {
                i    = n - 1;
                n    = -1;
                incr = -1;
            }

            for ( ; i != n; i += incr )
            {
                Actor? child = children.GetAt( i );

                if ( child == null )
                {
                    continue;
                }

                float width;
                float height;

                if ( child is ILayout layoutChild )
                {
                    width  = layoutChild.GetPrefWidth();
                    height = layoutChild.GetPrefHeight();

                    if ( height > groupHeight )
                    {
                        height = Math.Max( groupHeight, layoutChild.GetMinHeight() );
                    }
                }
                else
                {
                    width  = child.Width;
                    height = child.Height;
                }

                float incrY = height + ( y > 0 ? space : 0 );

                if ( ( ( y + incrY ) > groupHeight ) && ( y > 0 ) )
                {
                    columnSizes.Add( y );
                    columnSizes.Add( columnWidth );

                    _prefHeight = Math.Max( _prefHeight, y + pad );

                    if ( x > 0 )
                    {
                        x += wrapSpace;
                    }

                    x           += columnWidth;
                    columnWidth =  0;
                    y           =  0;
                    incrY       =  height;
                }

                y           += incrY;
                columnWidth =  Math.Max( columnWidth, width );
            }

            columnSizes.Add( y );
            columnSizes.Add( columnWidth );

            _prefHeight = Math.Max( _prefHeight, y + pad );

            if ( x > 0 )
            {
                x += wrapSpace;
            }

            _prefWidth = Math.Max( _prefWidth, x + columnWidth );
        }
        else
        {
            _prefHeight = PadTop + PadBottom + ( Space * ( n - 1 ) );

            for ( var i = 0; i < n; i++ )
            {
                Actor? child = children.GetAt( i );

                if ( child == null )
                {
                    continue;
                }

                if ( child is ILayout layoutChild )
                {
                    _prefWidth  =  Math.Max( _prefWidth, layoutChild.GetPrefWidth() );
                    _prefHeight += layoutChild.GetPrefHeight();
                }
                else
                {
                    _prefWidth  =  Math.Max( _prefWidth, child.Width );
                    _prefHeight += child.Height;
                }
            }
        }

        _prefWidth += PadLeft + PadRight;

        if ( _round )
        {
            _prefWidth  = ( long )Math.Round( _prefWidth, MidpointRounding.AwayFromZero );
            _prefHeight = ( long )Math.Round( _prefHeight, MidpointRounding.AwayFromZero );
        }
    }

    /// <summary>
    /// Computes and caches any information needed for drawing and, if this actor
    /// has children, positions and sizes each child, calls <see cref="ILayout.Invalidate"/>
    /// on any each child whose width or height has changed, and calls <see cref="ILayout.Validate"/>
    /// on each child. This method should almost never be called directly, instead
    /// <see cref="ILayout.Validate"/> should be used.
    /// </summary>
    public override void Layout() 
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        if ( Wrapping )
        {
            LayoutWrapped();

            return;
        }

        bool  round       = _round;
        Align align       = Alignment;
        float space       = Space;
        float padLeft     = PadLeft;
        float fill        = Fill;
        float columnWidth = ( Expand ? Width : _prefWidth ) - padLeft - PadRight;
        float y           = _prefHeight - PadTop + space;

        if ( ( align & Align.Top ) != 0 )
        {
            y += Height - _prefHeight;
        }
        else if ( ( align & Align.Bottom ) == 0 ) // center
        {
            y += ( Height - _prefHeight ) / 2;
        }

        float startX;

        if ( ( align & Align.Left ) != 0 )
        {
            startX = padLeft;
        }
        else if ( ( align & Align.Right ) != 0 )
        {
            startX = Width - PadRight - columnWidth;
        }
        else
        {
            startX = padLeft + ( ( Width - padLeft - PadRight - columnWidth ) / 2 );
        }

        align = _columnAlign;

        SnapshotArrayList< Actor? > children = Children;

        var i    = 0;
        int n    = children.Size;
        var incr = 1;

        if ( _reverse )
        {
            i    = n - 1;
            n    = -1;
            incr = -1;
        }

        for ( ; i != n; i += incr )
        {
            Actor? child = children.GetAt( i );

            if ( child == null )
            {
                continue;
            }
            
            float    width;
            float    height;
            ILayout? layout = null;

            if ( child is ILayout layoutchild )
            {
                layout = layoutchild;
                width  = layout.GetPrefWidth();
                height = layout.GetPrefHeight();
            }
            else
            {
                width  = child.Width;
                height = child.Height;
            }

            if ( fill > 0 )
            {
                width = columnWidth * fill;
            }

            if ( layout != null )
            {
                width = Math.Max( width, layout.GetMinWidth() );

                float maxWidth = layout.GetMaxWidth();

                if ( ( maxWidth > 0 ) && ( width > maxWidth ) )
                {
                    width = maxWidth;
                }
            }

            float x = startX;

            if ( ( align & Align.Right ) != 0 )
            {
                x += columnWidth - width;
            }
            else if ( ( align & Align.Left ) == 0 ) // center
            {
                x += ( columnWidth - width ) / 2;
            }

            y -= height + space;

            if ( round )
            {
                child.SetBounds( ( int )Math.Round( x, MidpointRounding.AwayFromZero ),
                                 ( int )Math.Round( y, MidpointRounding.AwayFromZero ),
                                 ( int )Math.Round( width, MidpointRounding.AwayFromZero ),
                                 ( int )Math.Round( height, MidpointRounding.AwayFromZero ) );
            }
            else
            {
                child.SetBounds( x, y, width, height );
            }

            layout?.Validate();
        }
    }

    private void LayoutWrapped()
    {
        float prefWidth = GetPrefWidth();

        if ( !prefWidth.Equals( _lastPrefWidth ) )
        {
            _lastPrefWidth = prefWidth;
            InvalidateHierarchy();
        }

        Align align       = Alignment;
        bool  round       = _round;
        float space       = Space;
        float padLeft     = PadLeft;
        float fill        = Fill;
        float wrapSpace   = WrapSpace;
        float maxHeight   = _prefHeight - PadTop - PadBottom;
        float columnX     = padLeft;
        float groupHeight = Height;
        float yStart      = _prefHeight - PadTop + space;
        var   y           = 0f;
        var   columnWidth = 0f;

        if ( ( align & Align.Right ) != 0 )
        {
            columnX += Width - prefWidth;
        }
        else if ( ( align & Align.Left ) == 0 ) // center
        {
            columnX += ( Width - prefWidth ) / 2;
        }

        if ( ( align & Align.Top ) != 0 )
        {
            yStart += groupHeight - _prefHeight;
        }
        else if ( ( align & Align.Bottom ) == 0 ) // center
        {
            yStart += ( groupHeight - _prefHeight ) / 2;
        }

        groupHeight -= PadTop;
        align       =  _columnAlign;

        List< float >?              columnSizes = _columnSizes!;
        SnapshotArrayList< Actor? > children    = Children;

        var i    = 0;
        int n    = children.Size;
        var incr = 1;

        if ( _reverse )
        {
            i    = n - 1;
            n    = -1;
            incr = -1;
        }

        for ( var r = 0; i != n; i += incr )
        {
            Actor? child = children.GetAt( i );

            if ( child == null )
            {
                continue;
            }

            float    width;
            float    height;
            ILayout? layout = null;

            if ( child is ILayout layoutchild )
            {
                layout = layoutchild;
                width  = layout.GetPrefWidth();
                height = layout.GetPrefHeight();

                if ( height > groupHeight )
                {
                    height = Math.Max( groupHeight, layout.GetMinHeight() );
                }
            }
            else
            {
                width  = child.Width;
                height = child.Height;
            }

            if ( ( ( y - height - space ) < PadBottom ) || ( r == 0 ) )
            {
                // In case an actor changed size without invalidating this layout.
                r = Math.Min( r, columnSizes.Count - 2 );

                y = yStart;

                if ( ( align & Align.Bottom ) != 0 )
                {
                    y -= maxHeight - columnSizes[ r ];
                }
                else if ( ( align & Align.Top ) == 0 ) // center
                {
                    y -= ( maxHeight - columnSizes[ r ] ) / 2;
                }

                if ( r > 0 )
                {
                    columnX += wrapSpace;
                    columnX += columnWidth;
                }

                columnWidth =  columnSizes[ r + 1 ];
                r           += 2;
            }

            if ( fill > 0 )
            {
                width = columnWidth * fill;
            }

            if ( layout != null )
            {
                width = Math.Max( width, layout.GetMinWidth() );

                float maxWidth = layout.GetMaxWidth();

                if ( ( maxWidth > 0 ) && ( width > maxWidth ) )
                {
                    width = maxWidth;
                }
            }

            float x = columnX;

            if ( ( align & Align.Right ) != 0 )
            {
                x += columnWidth - width;
            }
            else if ( ( align & Align.Left ) == 0 ) // center
            {
                x += ( columnWidth - width ) / 2;
            }

            y -= height + space;

            if ( round )
            {
                child.SetBounds(
                                ( float )Math.Round( x ),
                                ( float )Math.Round( y ),
                                ( float )Math.Round( width ),
                                ( float )Math.Round( height )
                               );
            }
            else
            {
                child.SetBounds( x, y, width, height );
            }

            layout?.Validate();
        }
    }

    public override float GetPrefWidth()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _prefWidth;
    }

    public override float GetPrefHeight()
    {
        if ( Wrapping )
        {
            return 0;
        }

        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _prefHeight;
    }

    /// <summary>
    /// If true (the default), positions and sizes are rounded to integers.
    /// </summary>
    public void SetRound( bool round )
    {
        _round = round;
    }

    /// <summary>
    /// If true, The children will be displayed last to first.
    /// </summary>
    public VerticalGroup Reverse( bool reverse = true )
    {
        _reverse = reverse;

        return this;
    }

    public bool GetReverse()
    {
        return _reverse;
    }

    /// <summary>
    /// Sets the padTop, padLeft, padBottom, and padRight to the specified value.
    /// </summary>
    public VerticalGroup PadAll( float pad )
    {
        PadTop    = pad;
        PadLeft   = pad;
        PadBottom = pad;
        PadRight  = pad;

        return this;
    }

    /// <summary>
    /// Sets the padTop, padLeft, padBottom, and padRight to the specified value.
    /// </summary>
    public VerticalGroup SetPadAll( float top, float left, float bottom, float right )
    {
        PadTop    = top;
        PadLeft   = left;
        PadBottom = bottom;
        PadRight  = right;

        return this;
    }

    /// <summary>
    /// Sets the alignment of all widgets within the vertical group to
    /// <see cref="Align.Center"/>. This clears any other alignment.
    /// </summary>
    public VerticalGroup AlignCenter()
    {
        Alignment = Align.Center;

        return this;
    }

    /// <summary>
    /// Sets <see cref="Align.Top"/> and clears <see cref="Align.Bottom"/> for the
    /// alignment of all widgets within the vertical group.
    /// </summary>
    public VerticalGroup AlignTop()
    {
        Alignment |= Align.Top;
        Alignment &= ~Align.Bottom;

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Left"/> and clears <see cref="Align.Right"/> for the
    /// alignment of all widgets within the vertical group.
    /// </summary>
    public VerticalGroup AlignLeft()
    {
        Alignment |= Align.Left;
        Alignment &= ~Align.Right;

        return this;
    }

    /// <summary>
    /// Sets <see cref="Align.Bottom"/> and clears <see cref="Align.Top"/> for the
    /// alignment of all widgets within the vertical group.
    /// </summary>
    public VerticalGroup AlignBottom()
    {
        Alignment |= Align.Bottom;
        Alignment &= ~Align.Top;

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Right"/> and clears <see cref="Align.Left"/> for the
    /// alignment of all widgets within the vertical group.
    /// </summary>
    public VerticalGroup AlignRight()
    {
        Alignment |= Align.Right;
        Alignment &= ~Align.Left;

        return this;
    }

    /// <summary>
    /// Sets fill to 1 and expand to true.
    /// </summary>
    public VerticalGroup Grow()
    {
        Expand = true;
        Fill   = 1f;

        return this;
    }

    /// <summary>
    /// Sets the vertical alignment of each column of widgets when <see cref="Wrapping"/>
    /// is enabled and sets the horizontal alignment of widgets within each column. Set
    /// to <see cref="Align.Center"/>, <see cref="Align.Top"/>,
    /// <see cref="Align.Bottom"/>,
    /// <see cref="Align.Left"/>, <see cref="Align.Right"/>, or any combination of those.
    /// </summary>
    public VerticalGroup ColumnAlign( Align columnAlign )
    {
        _columnAlign = columnAlign;

        return this;
    }

    /// <summary>
    /// Sets the alignment of widgets within each column to <see cref="Align.Center"/>.
    /// This clears any other alignment.
    /// </summary>
    public virtual VerticalGroup ColumnCenter()
    {
        _columnAlign = Align.Center;

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Top"/> and clears <see cref="Align.Bottom"/> for the
    /// alignment of each column of widgets when <see cref="Wrapping"/> is enabled.
    /// </summary>
    public virtual VerticalGroup ColumnTop()
    {
        _columnAlign |= Align.Top;
        _columnAlign &= ~Align.Bottom;

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Left"/> and clears <see cref="Align.Right"/> for the
    /// alignment of widgets within each column.
    /// </summary>
    public virtual VerticalGroup ColumnLeft()
    {
        _columnAlign |= Align.Left;
        _columnAlign &= ~Align.Right;

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Bottom"/> and clears <see cref="Align.Top"/> for the
    /// alignment of each column of widgets when <see cref="Wrapping"/>
    /// wrapping} is enabled.
    /// </summary>
    public virtual VerticalGroup ColumnBottom()
    {
        _columnAlign |= Align.Bottom;
        _columnAlign &= ~Align.Top;

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Right"/> and clears <see cref="Align.Left"/>
    /// for the alignment of widgets within each column.
    /// </summary>
    public virtual VerticalGroup ColumnRight()
    {
        _columnAlign |= Align.Right;
        _columnAlign &= ~Align.Left;

        return this;
    }

    /// <inheritdoc />
    protected override void DrawDebugBounds( ShapeRenderer shapes )
    {
        base.DrawDebugBounds( shapes );

        if ( !DebugActive )
        {
            return;
        }

        shapes.Set( ShapeRenderer.ShapeRenderType.Lines );

        if ( Stage != null )
        {
            shapes.Color = Stage.DebugColor;
        }

        shapes.Rect( X + PadLeft,
                     Y + PadBottom,
                     OriginX,
                     OriginY,
                     Width - PadLeft - PadRight,
                     Height - PadBottom - PadTop,
                     ScaleX,
                     ScaleY,
                     Rotation );
    }
}