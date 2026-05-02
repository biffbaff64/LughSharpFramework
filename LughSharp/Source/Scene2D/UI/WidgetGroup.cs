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
using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// A <see cref="Group"/> that participates in layout and provides a minimum,
/// preferred, and maximum size.
/// <para>
/// The default preferred size of a widget group is 0 and this is almost always
/// overridden by a subclass. The default minimum size returns the preferred size,
/// so a subclass may choose to return 0 for minimum size if it wants to allow
/// itself to be sized smaller than the preferred size. The default maximum size
/// is 0, which means no maximum size.
/// </para>
/// See <see cref="ILayout"/> for details on how a widget group should participate
/// in layout. A widget group's mutator methods should call <see cref="Invalidate()"/>
/// or <see cref="InvalidateHierarchy()"/> as needed. By default, InvalidateHierarchy
/// is called when child widgets are added and removed.
/// </summary>
[PublicAPI]
public class WidgetGroup : Group, ILayout
{
    // <summary>
    /// Returns true if the widget's layout has been invalidated.
    // </summary>
    public bool NeedsLayout { get; private set; } = true;

    public bool LayoutEnabled { get; set; } = true;

    public bool FillParent { get; set; }

    // ========================================================================

    /// <summary>
    /// Creates a new, empty, widget group.
    /// </summary>
    protected WidgetGroup()
    {
    }

    /// <summary>
    /// Creates a new widget group containing the specified actors.
    /// </summary>
    public WidgetGroup( params Actor[] actors )
    {
        foreach ( Actor actor in actors )
        {
            AddActor( actor );
        }
    }

    /// <summary>
    /// Sizes this actor to its preferred width and height, then calls <see cref="ILayout.Validate"/>.
    /// <para>
    /// Generally this method should not be called in an actor's constructor because it calls
    /// <see cref="ILayout.Layout"/>, which means a subclass would have Layout() called before the
    /// subclass' constructor. Instead, in constructors simply set the actor's size
    /// to <see cref="ILayout.GetPrefWidth"/> and <see cref="ILayout.GetPrefHeight"/>. This allows
    /// the actor to have a size at construction time for more convenient use with groups that do
    /// not layout their children.
    /// </para>
    /// </summary>
    public virtual void Pack()
    {
        //TODO: Establish why this is done twice. The original LibGDX code does this, and that
        // is why it'sa done here too. I need to understand why this is done twice.
        
        SetSize( GetPrefWidth(), GetPrefHeight() );

        Validate();

        // Validating the layout may change the pref size. Eg, a wrapped label doesn't
        // know its pref height until it knows its width, so it calls invalidateHierarchy()
        // in layout() if its pref height has changed.
        SetSize( GetPrefWidth(), GetPrefHeight() );

        Validate();
    }

    /// <summary>
    /// Positions and sizes children of the table using the cell associated with each child.
    /// The values given are the position within the parent and size of the table.
    /// </summary>
    public virtual void Layout()
    {
    }

    /// <summary>
    /// Ensures the actor has been laid out.
    /// <para>
    /// Calls <see cref="ILayout.Layout"/> if <see cref="ILayout.Invalidate"/> has been
    /// called since the last time <see cref="ILayout.Validate"/> was called, or if the
    /// actor otherwise needs to be laid out. This method is usually called in
    /// <see cref="Actor.Draw"/> by the actor itself before drawing is performed.
    /// </para>
    /// </summary>
    public virtual void Validate()
    {
        if ( !LayoutEnabled )
        {
            return;
        }

        Group? parent = Parent;

        if ( FillParent && ( parent != null ) )
        {
            float  parentWidth;
            float  parentHeight;

            if ( ( Stage != null ) && ( parent == Stage.RootGroup ) )
            {
                parentWidth  = Stage.Width;
                parentHeight = Stage.Height;
            }
            else
            {
                parentWidth  = parent.Width;
                parentHeight = parent.Height;
            }

            SetSize( parentWidth, parentHeight );
        }

        if ( !NeedsLayout ) return;

        NeedsLayout = false;

        Layout();

        // Widgets may call invalidateHierarchy during layout (eg, a wrapped label).
        // The root-most widget group retries layout a reasonable number of times.
        if ( NeedsLayout )
        {
            if ( parent is WidgetGroup )
            {
                return; // The parent widget will layout again.
            }

            for ( var i = 0; i < 5; i++ )
            {
                NeedsLayout = false;

                Layout();

                if ( !NeedsLayout )
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Invalidates this actor's layout, causing <see cref="ILayout.Layout"/> to happen the
    /// next time <see cref="ILayout.Validate"/> is called. This method should be called when
    /// state changes in the actor that requires a layout but does not change the minimum,
    /// preferred, maximum, or actual size of the actor (meaning it does not affect the
    /// parent actor's layout).
    /// </summary>
    public virtual void Invalidate()
    {
        NeedsLayout = true;
    }

    /// <summary>
    /// Invalidates this actor and its ascendants, calling <see cref="ILayout.Invalidate"/> on each.
    /// This method should be called when state changes in the actor that affects the minimum,
    /// preferred, maximum, or actual size of the actor (meaning it potentially affects the
    /// parent actor's layout).
    /// </summary>
    public virtual void InvalidateHierarchy()
    {
        Invalidate();

        if ( Parent is ILayout layout )
        {
            layout.InvalidateHierarchy();
        }
    }

    /// <summary>
    /// Sets the layout enabled flag on this group and all its children.
    /// </summary>
    /// <param name="enabled"> The value to set. </param>
    public void SetLayoutEnabled( bool enabled )
    {
        LayoutEnabled = enabled;
        SetLayoutEnabled( this, enabled );
    }

    /// <summary>
    /// Sets the layout enabled flag on the children of the specified parent group.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="enabled"></param>
    private static void SetLayoutEnabled( Group parent, bool enabled )
    {
        var children = new SnapshotArrayList< Actor? >( parent.Children.ToArray() );

        for ( int i = 0, n = children.Size; i < n; i++ )
        {
            Actor? actor = children.GetAt( i );

            if ( actor is ILayout layout )
            {
                layout.LayoutEnabled = enabled;
            }
            else if ( actor is Group group )
            {
                SetLayoutEnabled( group, enabled );
            }
        }
    }

    public virtual float GetMinWidth()
    {
        return GetPrefWidth();
    }

    public virtual float GetMinHeight()
    {
        return GetPrefHeight();
    }

    public virtual float GetPrefWidth()
    {
        return 0;
    }

    public virtual float GetPrefHeight()
    {
        return 0;
    }

    public virtual float GetMaxWidth()
    {
        return 0;
    }

    public virtual float GetMaxHeight()
    {
        return 0;
    }

    /// <summary>
    /// Called when actors are added to or removed from the group.
    /// </summary>
    protected override void ChildrenChanged()
    {
        InvalidateHierarchy();
    }

    /// <summary>
    /// Called when the actor's size has been changed.
    /// </summary>
    public override void OnSizeChanged()
    {
        Invalidate();
    }

    /// <summary>
    /// Draws the group and its children.
    /// <para>
    /// This method overrides the default implementation to call <see cref="Validate"/>
    /// before drawing.
    /// </para>
    /// </summary>
    /// <param name="batch"> The <see cref="IBatch"/> to use. </param>
    /// <param name="parentAlpha"> </param>
    public override void Draw( IBatch batch, float parentAlpha )
    {
        Validate();
        base.Draw( batch, parentAlpha );
    }
}

// ============================================================================
// ============================================================================