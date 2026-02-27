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

using JetBrains.Annotations;

using LughSharp.Core.Scenes.Scene2D.Utils;
using LughSharp.Core.Utils.Collections;

namespace LughSharp.Core.Scenes.Scene2D.UI;

[PublicAPI]
public class Stack : WidgetGroup
{
    private bool _sizeInvalid = true;

    // ========================================================================

    public Stack( Actor[] actors ) : this()
    {
        LoadActors( actors );
    }

    public Stack()
    {
        Transform = false;
        Width     = 150;
        Height    = 150;
        Touchable = Touchable.ChildrenOnly;
    }

    public float PrefWidth
    {
        get
        {
            if ( _sizeInvalid )
            {
                ComputeSize();
            }

            return field;
        }
        set;
    }

    public float PrefHeight
    {
        get
        {
            if ( _sizeInvalid )
            {
                ComputeSize();
            }

            return field;
        }
        set;
    }

    public float MinWidth
    {
        get
        {
            if ( _sizeInvalid )
            {
                ComputeSize();
            }

            return field;
        }
        set;
    }

    public float MinHeight
    {
        get
        {
            if ( _sizeInvalid )
            {
                ComputeSize();
            }

            return field;
        }
        set;
    }

    public float MaxWidth
    {
        get
        {
            if ( _sizeInvalid )
            {
                ComputeSize();
            }

            return field;
        }
        set;
    }

    public float MaxHeight
    {
        get
        {
            if ( _sizeInvalid )
            {
                ComputeSize();
            }

            return field;
        }
        set;
    }

    public override void Invalidate()
    {
        base.Invalidate();
        _sizeInvalid = true;
    }

    private void ComputeSize()
    {
        _sizeInvalid = false;
        PrefWidth    = 0;
        PrefHeight   = 0;
        MinWidth     = 0;
        MinHeight    = 0;
        MaxWidth     = 0;
        MaxHeight    = 0;

        SnapshotArrayList< Actor? > children = Children;

        for ( int i = 0, n = children.Size; i < n; i++ )
        {
            Actor? child = children.GetAt( i );
            float  childMaxWidth, childMaxHeight;

            if ( child == null )
            {
                continue;
            }

            if ( child is ILayout layout )
            {
                PrefWidth  = Math.Max( PrefWidth, layout.GetPrefWidth() );
                PrefHeight = Math.Max( PrefHeight, layout.GetPrefHeight() );
                MinWidth   = Math.Max( MinWidth, layout.GetMinWidth() );
                MinHeight  = Math.Max( MinHeight, layout.GetMinHeight() );

                childMaxWidth  = layout.GetMaxWidth();
                childMaxHeight = layout.GetMaxHeight();
            }
            else
            {
                PrefWidth  = Math.Max( PrefWidth, child.Width );
                PrefHeight = Math.Max( PrefHeight, child.Height );
                MinWidth   = Math.Max( MinWidth, child.Width );
                MinHeight  = Math.Max( MinHeight, child.Height );

                childMaxWidth  = 0;
                childMaxHeight = 0;
            }

            if ( childMaxWidth > 0 )
            {
                MaxWidth = MaxWidth == 0 ? childMaxWidth : Math.Min( MaxWidth, childMaxWidth );
            }

            if ( childMaxHeight > 0 )
            {
                MaxHeight = MaxHeight == 0 ? childMaxHeight : Math.Min( MaxHeight, childMaxHeight );
            }
        }
    }

    public void Layout()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        float width  = Width;
        float height = Height;

        SnapshotArrayList< Actor? > children = Children;

        for ( int i = 0, n = children.Size; i < n; i++ )
        {
            Actor? child = children.GetAt( i );
            child?.SetBounds( 0, 0, width, height );

            if ( child is ILayout layout )
            {
                layout.Validate();
            }
        }
    }
}

// ============================================================================
// ============================================================================