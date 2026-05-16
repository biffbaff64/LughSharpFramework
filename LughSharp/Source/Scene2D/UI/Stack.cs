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

using LughSharp.Source.Collections;
using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Scene2D.UI;

[PublicAPI]
public class Stack : WidgetGroup
{
    private bool  _sizeInvalid = true;
    private float _prefWidth;
    private float _prefHeight;
    private float _minWidth;
    private float _minHeight;
    private float _maxWidth;
    private float _maxHeight;

    // ========================================================================

    /// <summary>
    /// Creates a new Stack.
    /// </summary>
    public Stack()
    {
        Initialise();
    }

    /// <summary>
    /// Creates a new Stack with the specified actors.
    /// </summary>
    public Stack( params Actor[] actors ) : this()
    {
        foreach ( Actor actor in actors )
        {
            AddActor( actor );
        }
    }

    /// <summary>
    /// Initialises the Stack. Sets the default values for the width and height to 150.
    /// Sets the touchable to ChildrenOnly.
    /// </summary>
    private void Initialise()
    {
        Transform = false;

        SetWidth( 150 );
        SetHeight( 150 );

        Touchable = Touchable.ChildrenOnly;
    }

    /// <summary>
    /// Returns the preferred width of this Stack.
    /// </summary>
    /// <returns></returns>
    public override float GetPrefWidth()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _prefWidth;
    }

    /// <summary>
    /// Returns the preferred height of this Stack.
    /// </summary>
    /// <returns></returns>
    public override float GetPrefHeight()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _prefHeight;
    }

    /// <summary>
    /// Returns the minimum width of this Stack.
    /// </summary>
    /// <returns></returns>
    public override float GetMinWidth()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _minWidth;
    }

    /// <summary>
    /// Returns the minimum height of this Stack.
    /// </summary>
    /// <returns></returns>
    public override float GetMinHeight()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _minHeight;
    }

    /// <summary>
    /// Returns the maximum width of this Stack.
    /// </summary>
    public override float GetMaxWidth()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _maxWidth;
    }

    /// <summary>
    /// Returns the maximum height of this Stack.
    /// </summary>
    public override float GetMaxHeight()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _maxHeight;
    }

    /// <summary>
    /// Invalidates this actor's layout, causing <see cref="ILayout.Layout"/> to happen the
    /// next time <see cref="ILayout.Validate"/> is called. This method should be called when
    /// state changes in the actor that requires a layout but does not change the minimum,
    /// preferred, maximum, or actual size of the actor (meaning it does not affect the
    /// parent actor's layout).
    /// </summary>
    public override void InvalidateLayout()
    {
        base.InvalidateLayout();
        _sizeInvalid = true;
    }

    private void ComputeSize()
    {
        _sizeInvalid = false;
        _prefWidth   = 0;
        _prefHeight  = 0;
        _minWidth    = 0;
        _minHeight   = 0;
        _maxWidth    = 0;
        _maxHeight   = 0;

        var children = new SnapshotArrayList< Actor >( Children );

        for ( int i = 0, n = children.Size; i < n; i++ )
        {
            Actor child = children.GetAt( i );
            float childMaxWidth, childMaxHeight;

            if ( child is ILayout layout )
            {
                _prefWidth  = Math.Max( _prefWidth, layout.GetPrefWidth() );
                _prefHeight = Math.Max( _prefHeight, layout.GetPrefHeight() );
                _minWidth   = Math.Max( _minWidth, layout.GetMinWidth() );
                _minHeight  = Math.Max( _minHeight, layout.GetMinHeight() );

                childMaxWidth  = layout.GetMaxWidth();
                childMaxHeight = layout.GetMaxHeight();
            }
            else
            {
                _prefWidth  = Math.Max( _prefWidth, child.GetWidth() );
                _prefHeight = Math.Max( _prefHeight, child.GetHeight() );
                _minWidth   = Math.Max( _minWidth, child.GetWidth() );
                _minHeight  = Math.Max( _minHeight, child.GetHeight() );

                childMaxWidth  = 0;
                childMaxHeight = 0;
            }

            if ( childMaxWidth > 0 )
            {
                _maxWidth = _maxWidth == 0 ? childMaxWidth : Math.Min( _maxWidth, childMaxWidth );
            }

            if ( childMaxHeight > 0 )
            {
                _maxHeight = _maxHeight == 0 ? childMaxHeight : Math.Min( _maxHeight, childMaxHeight );
            }
        }
    }

    /// <summary>
    /// Positions and sizes children of the table using the cell associated with each child.
    /// The values given are the position within the parent and size of the table.
    /// </summary>
    public override void Layout()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        float width  = GetWidth();
        float height = GetHeight();

        var children = new SnapshotArrayList< Actor >( Children );

        for ( int i = 0, n = children.Size; i < n; i++ )
        {
            Actor child = children.GetAt( i );
            child.SetBounds( 0, 0, width, height );

            if ( child is ILayout layout )
            {
                layout.Validate();
            }
        }
    }
}

// ============================================================================
// ============================================================================