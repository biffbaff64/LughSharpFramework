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

using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Scene2D.Listeners;
using LughSharp.Source.Scene2D.UI.Styles;
using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// A tree widget where each node has an icon, actor, and child nodes.
/// The preferred size of the tree is determined by the preferred size
/// of the actors for the expanded nodes.
/// </summary>
/// <typeparam name="TNode"> The type of nodes in the tree. </typeparam>
/// <typeparam name="TValue"> The type of values for each node. </typeparam>
[PublicAPI]
public class Tree< TNode, TValue > : WidgetGroup
    where TNode : Tree< TNode, TValue >.Node
{
    public TNode?         RangeStart    { get; set; }
    public ClickListener? ClickListener { get; set; }
    public TreeStyle?     Style         { get; set; }
    public List< TNode >  RootNodes     { get; set; } = [ ];
    public float          YSpacing      { get; set; } = 4;
    public float          IndentSpacing { get; set; }
    public TNode?         OverNode      { get; set; }

    // ========================================================================

    private readonly TreeSelection _selection;
    private readonly Vector2       _tmp = new();
    private          TNode?        _foundNode;
    private          float         _iconSpacingLeft  = 2;
    private          float         _iconSpacingRight = 2;
    private          float         _paddingLeft;
    private          float         _paddingRight;
    private          float         _prefHeight;
    private          float         _prefWidth;
    private          bool          _sizeInvalid = true;

    // ========================================================================

    /// <summary>
    /// Construct a new Tree using the supplied <see cref="Skin"/>
    /// and a default <see cref="TreeStyle"/> from that skin.
    /// </summary>
    public Tree( Skin skin ) : this( skin.Get< TreeStyle >() )
    {
    }

    /// <summary>
    /// A hierarchical widget for displaying and managing a tree-like structure of nodes.
    /// The tree can have one or more root nodes, and each node can include child nodes.
    /// The appearance and behavior of the tree are customizable through its style and other settings.
    /// </summary>
    /// <typeparam name="TNode">
    /// The type of the node used within the tree. This must inherit from <see cref="Tree{TNode, TValue}.Node"/>.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of the value that each node in the tree represents.
    /// </typeparam>
    public Tree( Skin skin, string styleName )
        : this( skin.Get< TreeStyle >( styleName ) )
    {
    }

    /// <summary>
    /// Represents a hierarchical UI component for displaying and managing nodes organized in a tree structure.
    /// </summary>
    /// <typeparam name="TNode">
    /// The type of the tree nodes, inheriting from <see cref="Tree{TNode, TValue}.Node"/>.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// The type of the value stored in each tree node.
    /// </typeparam>
    public Tree( TreeStyle style )
    {
        _selection = new TreeSelection( this )
        {
            Actor    = this,
            Multiple = true
        };

        SetStyle( style );

        ClickListener = new TreeClickListener();

        AddListener( ClickListener );
    }

    /// <summary>
    /// Sets the style to use for this tree.
    /// </summary>
    /// <param name="style"> The style to use for this tree. </param>
    public void SetStyle( TreeStyle style )
    {
        Style = style;

        // Reasonable default.
        if ( IndentSpacing == 0 )
        {
            IndentSpacing = PlusMinusWidth();
        }
    }

    /// <summary>
    /// Adds a node to the tree.
    /// </summary>
    /// <param name="node"> The node to add to the tree. </param>
    public void AddNode( TNode node )
    {
        Insert( RootNodes.Count, node );
    }

    /// <summary>
    /// Inserts a node at the specified index.
    /// </summary>
    /// <param name="index"> The index at which to insert the node. </param>
    /// <param name="node"> The node to insert into the tree. </param>
    public void Insert( int index, TNode node )
    {
        int actorIndex;

        if ( node.Parent != null )
        {
            node.Parent.Remove( node );
            node.Parent = null;
        }
        else
        {
            int existingIndex = RootNodes.IndexOf( node );

            if ( existingIndex != -1 )
            {
                if ( existingIndex == index )
                {
                    return;
                }

                if ( existingIndex < index )
                {
                    index--;
                }

                RootNodes.RemoveAt( existingIndex );

                actorIndex = node.Actor!.GetZIndex();

                if ( actorIndex != -1 )
                {
                    node.RemoveFromTree( this, actorIndex );
                }
            }
        }

        RootNodes.Insert( index, node );

        if ( index == 0 )
        {
            actorIndex = 0;
        }
        else if ( index < ( RootNodes.Count - 1 ) )
        {
            actorIndex = RootNodes[ index + 1 ].Actor!.GetZIndex();
        }
        else
        {
            TNode before = RootNodes[ index - 1 ];

            actorIndex = before.Actor!.GetZIndex() + before.CountActors();
        }

        node.AddToTree( this, actorIndex );
    }

    /// <summary>
    /// Removes the specified node from the tree.
    /// </summary>
    /// <param name="node"> The node to remove from the tree. </param>
    public void Remove( TNode node )
    {
        if ( node.Parent != null )
        {
            node.Parent.Remove( node );

            return;
        }

        if ( !RootNodes.Remove( node ) )
        {
            return;
        }

        int actorIndex = node.Actor!.GetZIndex();

        if ( actorIndex != -1 )
        {
            node.RemoveFromTree( this, actorIndex );
        }
    }

    /// <summary>
    /// Removes all nodes from the tree.
    /// </summary>
    /// <param name="unfocus"> Whether to unfocus the tree after clearing its children. </param>
    public override void ClearChildren( bool unfocus = true )
    {
        base.ClearChildren( unfocus );

        OverNode = null;
        RootNodes.Clear();
        _selection.Clear();
    }

    /// <summary>
    /// Invalidates the tree's layout.
    /// </summary>
    public override void InvalidateLayout()
    {
        base.InvalidateLayout();

        _sizeInvalid = true;
    }

    /// <summary>
    /// Calculates the maximum width among the components representing
    /// the expansion and collapse icons (plus and minus) in the tree structure.
    /// It takes into account the default, hovered states, and ensures the calculated width
    /// accommodates all potential visual states.
    /// </summary>
    /// <returns>
    /// The maximum width of the plus and minus icons, considering their default and
    /// hovered states defined in the <see cref="TreeStyle"/>.
    /// </returns>
    private float PlusMinusWidth()
    {
        Guard.Against.Null( Style );
        Guard.Against.Null( Style.Plus );
        Guard.Against.Null( Style.Minus );
        
        float width = Math.Max( Style.Plus.MinWidth, Style.Minus.MinWidth );

        if ( Style.PlusOver != null )
        {
            width = Math.Max( width, Style.PlusOver.MinWidth );
        }

        if ( Style.MinusOver != null )
        {
            width = Math.Max( width, Style.MinusOver.MinWidth );
        }

        return width;
    }

    /// <summary>
    /// Calculates the preferred size of the tree by determining the required width and height
    /// based on its content and configuration, such as indentation, spacing, and padding.
    /// </summary>
    /// <remarks>
    /// This method sets the internal `_prefWidth` and `_prefHeight`, which are used to define
    /// the size of the tree. It processes the tree's root nodes and any child nodes recursively
    /// to compute the total dimensions. Additionally, it ensures any cached dimensions are
    /// re-evaluated when invalidated.
    /// </remarks>
    private void ComputeSize()
    {
        _sizeInvalid = false;
        _prefWidth   = PlusMinusWidth();
        _prefHeight  = 0;

        ComputeSize( RootNodes, 0, _prefWidth );

        _prefWidth += _paddingLeft + _paddingRight;
    }

    /// <summary>
    /// Calculates the preferred size of the tree, including width and height,
    /// based on its root nodes, spacing, and padding values.
    /// This method ensures that the tree's layout dimensions are correctly
    /// updated and takes into account the presence of child nodes and their
    /// expansion states.
    /// </summary>
    /// <param name="nodes"> The list of root nodes to compute the size for. </param>
    /// <param name="indent"> The current indentation level for the nodes. </param>
    /// <param name="plusMinusWidth"> The width of the plus/minus icons. </param>
    private void ComputeSize( List< TNode > nodes, float indent, float plusMinusWidth )
    {
        float ySpacing = YSpacing;
        float spacing  = _iconSpacingLeft + _iconSpacingRight;

        for ( int i = 0, n = nodes.Count; i < n; i++ )
        {
            TNode  node     = nodes[ i ];
            float  rowWidth = indent + plusMinusWidth;
            Actor? actor    = node.Actor;

            if ( actor != null )
            {
                if ( actor is ILayout layout )
                {
                    rowWidth    += layout.GetPrefWidth();
                    node.Height =  layout.GetPrefHeight();
                }
                else
                {
                    rowWidth    += actor.GetWidth();
                    node.Height =  actor.GetHeight();
                }

                if ( node.Icon != null )
                {
                    rowWidth    += spacing + node.Icon.MinWidth;
                    node.Height =  Math.Max( node.Height, node.Icon.MinHeight );
                }

                _prefWidth  =  Math.Max( _prefWidth, rowWidth );
                _prefHeight += node.Height + ySpacing;

                if ( node.IsExpanded )
                {
                    //TODO: Refactor to remove recursiveness
                    ComputeSize( node.NodeChildren!, indent + IndentSpacing, plusMinusWidth );
                }
            }
        }
    }

    /// <summary>
    /// Positions and sizes children of the table using the cell associated with each child.
    /// The values given are the position within the parent and size of the table.
    /// </summary>
    /// <remarks>
    /// This method overrides the base method to call <see cref="ComputeSize()"/> if
    /// <see cref="_sizeInvalid"/> is true.
    /// </remarks>
    public override void Layout()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        Layout( RootNodes, _paddingLeft, GetHeight() - ( YSpacing / 2 ), PlusMinusWidth() );
    }

    /// <summary>
    /// Arranges and positions the nodes in the tree layout recursively, taking into account
    /// spacing, indentation, and node states (expanded or collapsed).
    /// </summary>
    /// <param name="nodes">The list of nodes to be laid out.</param>
    /// <param name="indent">The current horizontal indentation level for child nodes.</param>
    /// <param name="y">The starting vertical coordinate for layout calculation.</param>
    /// <param name="plusMinusWidth">The width allocated for the expand/collapse icon.</param>
    /// <returns>The updated vertical position after laying out all nodes.</returns>
    private float Layout( List< TNode > nodes, float indent, float y, float plusMinusWidth )
    {
        float ySpacing        = YSpacing;
        float iconSpacingLeft = _iconSpacingLeft;
        float spacing         = iconSpacingLeft + _iconSpacingRight;

        for ( int i = 0, n = nodes.Count; i < n; i++ )
        {
            TNode node = nodes[ i ];
            float x    = indent + plusMinusWidth;

            if ( node.Icon != null )
            {
                x += spacing + node.Icon.MinWidth;
            }
            else
            {
                x += iconSpacingLeft;
            }

            if ( node.Actor is ILayout layout )
            {
                layout.Pack();
            }

            y -= node.Height;

            node.Actor!.SetPosition( x, y );

            y -= ySpacing;

            if ( node.IsExpanded )
            {
                //TODO: Refactor to remove recursiveness
                y = Layout( node.NodeChildren!, indent + IndentSpacing, y, plusMinusWidth );
            }
        }

        return y;
    }

    /// <summary>
    /// Draws the group and its children.
    /// <para>
    /// This method overrides the default implementation to call <see cref="WidgetGroup.Validate"/>
    /// before drawing.
    /// </para>
    /// </summary>
    /// <param name="batch"> The <see cref="IBatch"/> to use. </param>
    /// <param name="parentAlpha"> The alpha value of the parent widget. </param>
    public override void Draw( IBatch batch, float parentAlpha )
    {
        DrawBackground( batch, parentAlpha );

        batch.SetColor( ActorColor.R, ActorColor.G, ActorColor.B, ActorColor.A * parentAlpha );

        Draw( batch, RootNodes, _paddingLeft, PlusMinusWidth() );

        // Draw node actors.
        base.Draw( batch, parentAlpha );
    }

    /// <summary>
    /// Called to draw the background.
    /// Default implementation draws the style background drawable.
    /// </summary>
    protected void DrawBackground( IBatch batch, float parentAlpha )
    {
        if ( Style?.Background != null )
        {
            batch.SetColor( ActorColor.R, ActorColor.G, ActorColor.B, ActorColor.A * parentAlpha );

            Style.Background.Draw( batch, GetX(), GetY(), GetWidth(), GetHeight() );
        }
    }

    /// <summary>
    /// Renders the visual representation of the tree, including its background,
    /// root nodes, and node actors. This method is overridden to handle custom
    /// drawing logic for the tree structure.
    /// </summary>
    /// <param name="batch">The batch used to render the tree's visual elements.</param>
    /// <param name="nodes">The list of nodes to be drawn.</param>
    /// <param name="indent">The current horizontal indentation level for child nodes.</param>
    /// <param name="plusMinusWidth">The width allocated for the expand/collapse icon.</param>
    private void Draw( IBatch batch, List< TNode > nodes, float indent, float plusMinusWidth )
    {
        Rectangle? cullingArea = CullingArea;
        float      cullBottom  = 0;
        float      cullTop     = 0;

        if ( cullingArea != null )
        {
            cullBottom = cullingArea.Y;
            cullTop    = cullBottom + cullingArea.Height;
        }

        TreeStyle? style = Style;

        float x       = GetX();
        float y       = GetY();
        float expandX = x + indent;
        float iconX   = expandX + plusMinusWidth + _iconSpacingLeft;

        for ( int i = 0, n = nodes.Count; i < n; i++ )
        {
            TNode node   = nodes[ i ];
            Actor actor  = node.Actor ?? throw new LughRuntimeException( "node.Actor cannot be null!" );
            float actorY = actor.GetY();
            float height = node.Height;

            if ( ( cullingArea == null ) || ( ( ( actorY + height ) >= cullBottom ) && ( actorY <= cullTop ) ) )
            {
                if ( _selection.Contains( node ) && ( style?.Selection != null ) )
                {
                    DrawSelection( node,
                                   style.Selection,
                                   batch,
                                   x,
                                   y + actorY - ( YSpacing / 2 ),
                                   GetWidth(),
                                   height + YSpacing );
                }
                else if ( ( node == OverNode ) && ( style?.Over != null ) )
                {
                    DrawOver( node, style.Over, batch, x, y + actorY - ( YSpacing / 2 ), GetWidth(), height + YSpacing );
                }

                if ( node.Icon != null )
                {
                    double iconY = y + actorY + Math.Round( ( height - node.Icon.MinHeight ) / 2 );

                    batch.Color = actor.ActorColor;
                    DrawIcon( node, node.Icon, batch, iconX, ( float )iconY );
                    batch.SetColor( 1, 1, 1, 1 );
                }

                if ( node.NodeChildren?.Count > 0 )
                {
                    ISceneDrawable expandIcon = GetExpandIcon( node, iconX );
                    double         iconY      = y + actorY + Math.Round( ( height - expandIcon.MinHeight ) / 2 );

                    DrawIcon( node, expandIcon, batch, expandX, ( float )iconY );
                }
            }
            else if ( actorY < cullBottom )
            {
                return;
            }

            if ( node is { IsExpanded: true, NodeChildren.Count: > 0 } )
            {
                //TODO: Refactor to remove recursiveness 
                Draw( batch, node.NodeChildren, indent + IndentSpacing, plusMinusWidth );
            }
        }
    }

    /// <summary>
    /// Draws the selection visual for a node within the tree.
    /// </summary>
    /// <param name="node">The node for which the selection is being drawn.</param>
    /// <param name="selection">The drawable resource used to represent the selection.</param>
    /// <param name="batch">The rendering batch used to draw the selection.</param>
    /// <param name="x">The X coordinate of the selection area.</param>
    /// <param name="y">The Y coordinate of the selection area.</param>
    /// <param name="width">The width of the selection area.</param>
    /// <param name="height">The height of the selection area.</param>
    protected void DrawSelection( TNode node, ISceneDrawable selection, IBatch batch, float x, float y, float width,
                                  float height )
    {
        selection.Draw( batch, x, y, width, height );
    }

    /// <summary>
    /// Draws the "over" visual representation for a tree node.
    /// </summary>
    /// <param name="node">The tree node to render the "over" visual representation for.</param>
    /// <param name="over">The drawable resource that represents the "over" visual style for the node.</param>
    /// <param name="batch">The batch used to render the drawable.</param>
    /// <param name="x">The x-coordinate of the position where the drawable should be drawn.</param>
    /// <param name="y">The y-coordinate of the position where the drawable should be drawn.</param>
    /// <param name="width">The width of the drawable to be rendered.</param>
    /// <param name="height">The height of the drawable to be rendered.</param>
    protected void DrawOver( TNode node, ISceneDrawable over, IBatch batch, float x, float y, float width,
                             float height )
    {
        over.Draw( batch, x, y, width, height );
    }

    /// <summary>
    /// Draws the icon for the specified node.
    /// </summary>
    /// <param name="node">The node to draw the icon for.</param>
    /// <param name="icon">The drawable resource that represents the icon.</param>
    /// <param name="batch">The batch used to render the drawable.</param>
    /// <param name="x">The x-coordinate of the position where the drawable should be drawn.</param>
    /// <param name="y">The y-coordinate of the position where the drawable should be drawn.</param>
    protected void DrawIcon( TNode node, ISceneDrawable icon, IBatch batch, float x, float y )
    {
        icon.Draw( batch, x, y, icon.MinWidth, icon.MinHeight );
    }

    /// <summary>
    /// Returns the drawable for the expand icon. The default implementation returns
    /// <see cref="TreeStyle.PlusOver"/> or <see cref="TreeStyle.MinusOver"/>
    /// on the desktop if the node is the over node, the mouse is left of iconX, and
    /// clicking would expand the node.
    /// </summary>
    /// <param name="node"> The node to get the expand icon for. </param>
    /// <param name="iconX"> The X coordinate of the expand icon. </param>
    protected ISceneDrawable GetExpandIcon( TNode node, float iconX )
    {
        var over = false;

        if ( ( node == OverNode )
          && ( Engine.App.AppType == Platform.ApplicationType.WindowsGL )
          && ( !_selection.Multiple || ( !InputUtils.CtrlKey() && !InputUtils.ShiftKey() ) ) )
        {
            float mouseX = ScreenToLocalCoordinates( _tmp.Set( Engine.Input.GetX(), 0 ) ).X;

            if ( ( mouseX >= 0 ) && ( mouseX < iconX ) )
            {
                over = true;
            }
        }

        if ( Style == null )
        {
            throw new LughRuntimeException( "Style is NULL!" );
        }

        ISceneDrawable? icon;
        
        if ( over )
        {
            icon = node.IsExpanded ? Style.MinusOver : Style.PlusOver;

            if ( icon != null )
            {
                return icon;
            }
        }

        icon = node.IsExpanded ? Style.Minus : Style.Plus;
        
        if ( icon != null )
        {
            return icon;
        }
        
        throw new LughRuntimeException( "Style.Plus or Style.Minus is NULL!" );
    }

    /// <summary>
    /// Retrieves the tree node located at the specified vertical position.
    /// </summary>
    /// <param name="y">The vertical position to search for a node.</param>
    /// <returns>
    /// The tree node found at the specified position, or <c>null</c> if no node exists at that position.
    /// </returns>
    public TNode? GetNodeAt( float y )
    {
        _foundNode = null;
        GetNodeAt( RootNodes, y, GetHeight() );

        return _foundNode;
    }

    /// <summary>
    /// Retrieves the first node at the specified vertical position within the tree.
    /// </summary>
    /// <param name="nodes">The list of nodes to search within.</param>
    /// <param name="y">The vertical position relative to the tree's coordinate space.</param>
    /// <param name="rowY">The current vertical position within the tree.</param>
    /// <returns>The node at the specified vertical position, or null if no node is found.</returns>
    private float GetNodeAt( List< TNode > nodes, float y, float rowY )
    {
        for ( int i = 0, n = nodes.Count; i < n; i++ )
        {
            TNode node   = nodes[ i ];
            float height = node.Height;

            rowY -= node.Height - height; // Node subclass may increase getHeight.

            if ( ( y >= ( rowY - height - YSpacing ) ) && ( y < rowY ) )
            {
                _foundNode = node;

                return -1;
            }

            rowY -= height + YSpacing;

            if ( node.IsExpanded )
            {
                //TODO: Refactor to remove recursiveness 
                rowY = GetNodeAt( node.NodeChildren!, y, rowY );

                if ( Math.Abs( rowY - -1 ) < 0.1f )
                {
                    return -1;
                }
            }
        }

        return rowY;
    }

    /// <summary>
    /// Selects nodes within the specified vertical range and adds them to the selection.
    /// </summary>
    /// <param name="nodes">The list of nodes to traverse and evaluate against the range.</param>
    /// <param name="low">The lower bound of the vertical range.</param>
    /// <param name="high">The upper bound of the vertical range.</param>
    private void SelectNodes( List< TNode > nodes, float low, float high )
    {
        for ( int i = 0, n = nodes.Count; i < n; i++ )
        {
            TNode node = nodes[ i ];

            if ( node.Actor?.GetY() < low )
            {
                break;
            }

            if ( !node.Selectable )
            {
                continue;
            }

            if ( node.Actor?.GetY() <= high )
            {
                _selection.Add( node );
            }

            if ( node.IsExpanded )
            {
                //TODO: Refactor to remove recursiveness 
                SelectNodes( node.NodeChildren!, low, high );
            }
        }
    }

    /// <summary>
    /// Gets the currently selected node.
    /// </summary>
    /// <returns> The node. </returns>
    public Selection< TNode > GetSelection()
    {
        return _selection;
    }

    /// <summary>
    /// Returns the first selected node, or null.
    /// </summary>
    public TNode? GetSelectedNode()
    {
        return _selection.First();
    }

    /// <summary>
    /// Returns the first selected value, or null.
    /// </summary>
    public TValue? GetSelectedValue()
    {
        return default( TValue? );
    }

    /// <summary>
    /// Updates the order of the actors in the tree for all root nodes and all
    /// child nodes.
    /// This is useful after changing the order of <see cref="RootNodes"/>.
    /// </summary>
    public void UpdateRootNodes()
    {
        for ( int i = 0, n = RootNodes.Count; i < n; i++ )
        {
            TNode node       = RootNodes[ i ];
            int   actorIndex = node.Actor!.GetZIndex();

            if ( actorIndex != -1 )
            {
                node.RemoveFromTree( this, actorIndex );
            }
        }

        for ( int i = 0, n = RootNodes.Count, actorIndex = 0; i < n; i++ )
        {
            actorIndex += RootNodes[ i ].AddToTree( this, actorIndex );
        }
    }

    /// <summary>
    /// Populates the provided list with values of nodes in the tree that are currently expanded.
    /// </summary>
    /// <param name="values">A list to be populated with the values of the expanded nodes in the tree.</param>
    public void FindExpandedValues( List< TValue > values )
    {
        FindExpandedValues( RootNodes, values );
    }

    /// <summary>
    /// Finds the expanded values from the current tree structure and adds them to the provided list of values.
    /// </summary>
    /// <param name="nodes">The list of nodes to traverse and evaluate for expanded values.</param>
    /// <param name="values">The list where the values of expanded nodes will be collected.</param>
    private static bool FindExpandedValues( List< TNode > nodes, List< TValue > values )
    {
        var expanded = false;

        for ( int i = 0, n = nodes.Count; i < n; i++ )
        {
            Node node = nodes[ i ];

            if ( ( node.NodeChildren != null ) && ( node.Value != null ) )
            {
                //TODO: Refactor to remove recursiveness 
                if ( node.IsExpanded && !FindExpandedValues( node.NodeChildren, values ) )
                {
                    values.Add( node.Value );

                    expanded = true;
                }
            }
        }

        return expanded;
    }

    public void RestoreExpandedValues( List< TValue > values )
    {
        for ( int i = 0, n = values.Count; i < n; i++ )
        {
            TNode? node = FindNode( values[ i ] );

            if ( node != null )
            {
                node.SetExpanded( true );
                node.ExpandTo();
            }
        }
    }

    /// <summary>
    /// Returns the node with the specified value, or null.
    /// </summary>
    public TNode? FindNode( TValue value )
    {
        Guard.Against.Null( value );

        return ( TNode? )FindNode( RootNodes, value );
    }

    private static Node? FindNode< T >( List< T > nodes, object value ) where T : Node
    {
        for ( int i = 0, n = nodes.Count; i < n; i++ )
        {
            if ( value.Equals( nodes[ i ].Value ) )
            {
                return nodes[ i ];
            }
        }

        for ( int i = 0, n = nodes.Count; i < n; i++ )
        {
            //TODO: Refactor to remove recursiveness 
            Node? found = FindNode( nodes[ i ].NodeChildren!, value );

            if ( found != null )
            {
                return found;
            }
        }

        return null;
    }

    public void CollapseAll()
    {
        CollapseAll( RootNodes );
    }

    private static void CollapseAll< T >( List< T > nodes ) where T : Node
    {
        for ( int i = 0, n = nodes.Count; i < n; i++ )
        {
            nodes[ i ].SetExpanded( false );

            //TODO: Refactor to remove recursiveness 
            CollapseAll( nodes[ i ].NodeChildren! );
        }
    }

    public void ExpandAll()
    {
        ExpandAll( RootNodes );
    }

    private static void ExpandAll< T >( List< T > nodes ) where T : Node
    {
        for ( int i = 0, n = nodes.Count; i < n; i++ )
        {
            nodes[ i ].ExpandAll();
        }
    }

    // ========================================================================

    public TValue? GetOverValue()
    {
        return OverNode == null ? default( TValue? ) : OverNode.Value;
    }

    public void SetPadding( float padding )
    {
        _paddingLeft  = padding;
        _paddingRight = padding;
    }

    public void SetPadding( float left, float right )
    {
        _paddingLeft  = left;
        _paddingRight = right;
    }

    public void SetIconSpacing( float left, float right )
    {
        _iconSpacingLeft  = left;
        _iconSpacingRight = right;
    }

    /// <summary>
    /// Gets the preferred width of this tree.
    /// </summary>
    public override float GetPrefWidth()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _prefWidth;
    }

    /// <summary>
    /// Gets the preferred height of this tree.
    /// </summary>
    public override float GetPrefHeight()
    {
        if ( _sizeInvalid )
        {
            ComputeSize();
        }

        return _prefHeight;
    }

    // ========================================================================
    // ========================================================================

    /// <inheritdoc />
    [PublicAPI]
    public class TreeSelection : Selection< TNode >
    {
        private readonly Tree< TNode, TValue > _parent;

        public TreeSelection( Tree< TNode, TValue > p )
        {
            _parent = p;
        }

        /// <inheritdoc />
        protected override void OnChanged()
        {
            _parent.RangeStart = Size() switch
                                 {
                                     0     => null,
                                     1     => First(),
                                     var _ => _parent.RangeStart
                                 };
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// A <see cref="Tree{TNode,TValue}"/> node which has an actor and value.
    /// <para>
    /// A subclass can be used so the generic type parameters don't need
    /// to be specified repeatedly.
    /// </para>
    /// </summary>
    [PublicAPI]
    public class Node
    {
        public TValue?         Value      { get; set; }
        public TNode?          Parent     { get; set; }
        public ISceneDrawable? Icon       { get; set; }
        public bool            Selectable { get; set; } = true;
        public float           Height     { get; set; }
        public bool            IsExpanded { get; private set; }

        /// <summary>
        /// If the children order is changed, <see cref="UpdateChildren()"/> must
        /// be called to ensure the node's actors are in the correct order. That
        /// is not necessary if this node is not in the tree or is not expanded,
        /// because then the child node's actors are not in the tree.
        /// </summary>
        public List< TNode >? NodeChildren { get; set; } = [ ];

        // ====================================================================

        /// <summary>
        /// Creates a node without an actor. An actor must be
        /// set before this node can be used.
        /// </summary>
        public Node()
        {
        }

        /// <summary>
        /// Creates a node with the specified actor.
        /// </summary>
        /// <param name="actor">The actor to be associated with the node.</param>
        public Node( Actor actor )
        {
            Actor = actor;
        }

        /// <summary>
        /// Gets or sets the actor associated with this node.
        /// </summary>
        public Actor? Actor
        {
            get;
            set
            {
                if ( field != null )
                {
                    Tree< TNode, TValue >? tree = GetTree();

                    if ( tree != null )
                    {
                        int index = field.GetZIndex();

                        tree.RemoveActorAt( index, true );
                        tree.AddActorAt( index, value! );
                    }
                }

                field = value;
            }
        }

        /// <summary>
        /// Sets the expansion state of the node. When expanded, the node's child nodes
        /// become visible, and when collapsed, the child nodes are hidden.
        /// </summary>
        /// <param name="expanded">
        /// A boolean flag indicating whether the node should be expanded (true) or
        /// collapsed (false).
        /// </param>
        public void SetExpanded( bool expanded )
        {
            if ( expanded == IsExpanded )
            {
                return;
            }

            IsExpanded = expanded;

            Tree< TNode, TValue >? tree = GetTree();

            if ( ( tree == null ) || ( NodeChildren == null ) || ( Actor == null ) )
            {
                return;
            }

            if ( NodeChildren.Count == 0 )
            {
                return;
            }

            TNode[]? children   = NodeChildren.ToArray();
            int      actorIndex = Actor.GetZIndex() + 1;

            if ( expanded )
            {
                for ( int i = 0, n = NodeChildren.Count; i < n; i++ )
                {
                    actorIndex += children[ i ].AddToTree( tree, actorIndex );
                }
            }
            else
            {
                for ( int i = 0, n = NodeChildren.Count; i < n; i++ )
                {
                    children?[ i ].RemoveFromTree( tree, actorIndex );
                }
            }
        }

        /// <summary>
        /// Called to add the actor to the tree when the node's parent is expanded.
        /// </summary>
        /// <param name="tree">The tree to which the actor should be added.</param>
        /// <param name="actorIndex">The index at which the actor should be added in the tree.</param>
        /// <returns> The number of node actors added to the tree. </returns>
        public int AddToTree( Tree< TNode, TValue > tree, int actorIndex )
        {
            if ( Actor == null )
            {
                return 0;
            }
            
            tree.AddActorAt( actorIndex, Actor );

            if ( !IsExpanded )
            {
                return 1;
            }

            int     childIndex = actorIndex + 1;
            TNode[] children   = NodeChildren!.ToArray();

            for ( int i = 0, n = children.Length; i < n; i++ )
            {
                childIndex += children[ i ].AddToTree( tree, childIndex );
            }

            return childIndex - actorIndex;
        }

        /// <summary>
        /// Called to remove the actor from the tree, eg when the node is
        /// removed or the node's parent is collapsed.
        /// </summary>
        /// <param name="tree">The tree from which the actor should be removed.</param>
        /// <param name="actorIndex">The index at which the actor should be removed from the tree.</param>
        public void RemoveFromTree( Tree< TNode, TValue > tree, int actorIndex )
        {
            if ( !IsExpanded )
            {
                return;
            }

            TNode[]? children = NodeChildren?.ToArray();

            if ( children == null )
            {
                return;
            }

            for ( int i = 0, n = NodeChildren!.Count; i < n; i++ )
            {
                children[ i ].RemoveFromTree( tree, actorIndex );
            }
        }

        /// <summary>
        /// Adds the specified node to the <see cref="NodeChildren"/> list.
        /// </summary>
        /// <param name="node">The node to be added.</param>
        public void Add( TNode node )
        {
            Guard.Against.Null( NodeChildren );

            Insert( NodeChildren.Count, node );
        }

        /// <summary>
        /// Adds all the specified nodes to the <see cref="NodeChildren"/> list.
        /// </summary>
        /// <param name="nodes">The nodes to be added.</param>
        public void AddAll( List< TNode > nodes )
        {
            Guard.Against.Null( NodeChildren );

            for ( int i = 0, n = nodes.Count; i < n; i++ )
            {
                Insert( NodeChildren.Count, nodes[ i ] );
            }
        }

        /// <summary>
        /// Inserts the supplied node into the <see cref="NodeChildren"/> list.
        /// </summary>
        /// <param name="childIndex">The index at which the node should be inserted.</param>
        /// <param name="node">The node to be inserted.</param>
        public void Insert( int childIndex, TNode node )
        {
            if ( ( NodeChildren == null ) || ( Actor == null ) )
            {
                return;
            }

            node.Parent = Parent;

            NodeChildren.Insert( childIndex, node );

            if ( !IsExpanded )
            {
                return;
            }

            Tree< TNode, TValue >? tree = GetTree();

            if ( tree != null )
            {
                int actorIndex;

                if ( childIndex == 0 )
                {
                    actorIndex = Actor.GetZIndex() + 1;
                }
                else if ( childIndex < ( NodeChildren.Count - 1 ) )
                {
                    TNode nchild = NodeChildren[ childIndex + 1 ];
                    
                    if ( nchild.Actor == null )
                    {
                        return;
                    }
                    
                    actorIndex = nchild.Actor.GetZIndex();
                }
                else
                {
                    TNode before = NodeChildren[ childIndex - 1 ];
                    
                    if ( before.Actor == null )
                    {
                        return;
                    }

                    actorIndex = before.Actor.GetZIndex() + before.CountActors();
                }

                node.AddToTree( tree, actorIndex );
            }
        }

        /// <summary>
        /// Return the current count of actors held in <see cref="NodeChildren"/>.
        /// If this node is not expanded, a count of 1 is returned by default.
        /// </summary>
        /// <returns>The count of actors held in <see cref="NodeChildren"/>.</returns>
        public int CountActors()
        {
            Guard.Against.Null( NodeChildren );

            if ( !IsExpanded )
            {
                return 1;
            }

            var actorCount = 1;

            for ( int i = 0, n = NodeChildren.Count; i < n; i++ )
            {
                actorCount += NodeChildren[ i ].CountActors();
            }

            return actorCount;
        }

        /// <summary>
        /// Remove this node from its parent.
        /// </summary>
        public void Remove()
        {
            Tree< TNode, TValue >? tree = GetTree();

            if ( tree != null )
            {
                tree.Remove( Parent! );
            }
            else
            {
                Parent?.Remove( Parent! );
            }
        }

        /// <summary>
        /// Remove the specified child node from this node.
        /// Does nothing if the node is not a child of this node.
        /// </summary>
        /// <param name="node">The node to be removed.</param>
        public void Remove( TNode? node )
        {
            if ( ( node == null )
              || ( NodeChildren == null )
              || !NodeChildren.Remove( node )
              || !IsExpanded )
            {
                return;
            }

            Tree< TNode, TValue >? tree = GetTree();

            if ( ( tree == null ) || ( node.Actor == null ) )
            {
                return;
            }

            node.RemoveFromTree( tree, node.Actor.GetZIndex() );
        }

        /// <summary>
        /// Removes all children from this node.
        /// </summary>
        public void ClearChildren()
        {
            if ( IsExpanded && ( Actor != null ) )
            {
                Tree< TNode, TValue >? tree = GetTree();

                if ( ( tree != null ) && ( NodeChildren != null ) )
                {
                    int actorIndex = Actor.GetZIndex() + 1;

                    for ( int i = 0, n = NodeChildren.Count; i < n; i++ )
                    {
                        NodeChildren[ i ].RemoveFromTree( tree, actorIndex );
                    }
                }
            }

            NodeChildren?.Clear();
        }

        /// <summary>
        /// Returns the tree this node's actor is currently in, or null.
        /// The actor is only in the tree when all of its parent nodes
        /// are expanded.
        /// </summary>
        /// <returns>The tree this node's actor is currently in, or null.</returns>
        public Tree< TNode, TValue >? GetTree()
        {
            Guard.Against.Null( Actor );

            if ( Actor.Parent is Tree< TNode, TValue > tree )
            {
                return tree;
            }

            return null;
        }

        /// <summary>
        /// Returns whether <see cref="NodeChildren"/> has any children.
        /// </summary>
        /// <returns>True if <see cref="NodeChildren"/> has children, false otherwise.</returns>
        public bool HasChildren()
        {
            return NodeChildren?.Count > 0;
        }

        /// <summary>
        /// Updates the order of the actors in the tree for this node and all child nodes.
        /// This is useful after changing the order of <see cref="NodeChildren"/>.
        /// </summary>
        public void UpdateChildren()
        {
            Guard.Against.Null( Actor );
            
            if ( !IsExpanded )
            {
                return;
            }

            Tree< TNode, TValue >? tree = GetTree();

            if ( tree == null )
            {
                return;
            }

            TNode[]? children   = NodeChildren?.ToArray();
            int?     n          = NodeChildren?.Count;
            int      actorIndex = Actor.GetZIndex() + 1;

            for ( var i = 0; i < n; i++ )
            {
                children?[ i ].RemoveFromTree( tree, actorIndex );
            }

            for ( var i = 0; i < n; i++ )
            {
                actorIndex += children![ i ].AddToTree( tree, actorIndex );
            }
        }

        /// <summary>
        /// Returns the level of this node in the tree.
        /// </summary>
        /// <returns>The level of this node in the tree.</returns>
        public int GetLevel()
        {
            var   level   = 0;
            Node? current = this;

            do
            {
                level++;
                current = current.Parent;
            }
            while ( current != null );

            return level;
        }

        /// <summary>
        /// Returns this node or the child node with the specified value, or null.
        /// </summary>
        /// <param name="value">The value to search for.</param>
        public TNode? FindNode( TValue? value )
        {
            Guard.Against.Null( value );

            if ( value.Equals( Value ) )
            {
                return ( TNode )this;
            }

            return ( TNode? )Tree< TNode, TValue >.FindNode( NodeChildren!, value );
        }

        /// <summary>
        /// Collapses all nodes under and including this node.
        /// </summary>
        public void CollapseAll()
        {
            SetExpanded( false );
            Tree< TNode, TValue >.CollapseAll( NodeChildren! );
        }

        /// <summary>
        /// Expands all nodes under and including this node.
        /// </summary>
        public void ExpandAll()
        {
            SetExpanded( true );

            if ( NodeChildren?.Count > 0 )
            {
                Tree< TNode, TValue >.ExpandAll( NodeChildren );
            }
        }

        /// <summary>
        /// Expands all parent nodes of this node.
        /// </summary>
        public void ExpandTo()
        {
            TNode? node = Parent;

            while ( node != null )
            {
                node.SetExpanded( true );
                node = node.Parent;
            }
        }

        /// <summary>
        /// Populates the specified list with the values of all expanded nodes under
        /// and including this node.
        /// </summary>
        /// <param name="values"> The list to populate with expanded values. </param>
        public void FindExpandedValues( List< TValue > values )
        {
            if ( IsExpanded
              && !Tree< TNode, TValue >.FindExpandedValues( NodeChildren!, values ) )
            {
                values.Add( Value! );
            }
        }

        /// <summary>
        /// Restores the expanded state of tree nodes specified by a list of values.
        /// For each value in the list, if a corresponding node is found, it is expanded,
        /// and its ancestors are expanded to ensure visibility.
        /// </summary>
        /// <param name="values">
        /// A list of values corresponding to nodes whose expanded state should be restored.
        /// </param>
        public void RestoreExpandedValues( List< TValue > values )
        {
            for ( int i = 0, n = values.Count; i < n; i++ )
            {
                TNode? node = FindNode( values[ i ] );

                if ( node is not null )
                {
                    node.SetExpanded( true );
                    node.ExpandTo();
                }
            }
        }

        /// <summary>
        /// Returns the height of the node as calculated for layout. A subclass
        /// may override and increase the returned height to create a blank space
        /// in the tree above the node, eg for a separator.
        /// </summary>
        public virtual float GetHeight()
        {
            return Height;
        }

        /// <summary>
        /// Returns true if the specified node is this node or an ascendant of this node.
        /// </summary>
        /// <param name="node">The node to check for ascendency.</param>
        public bool IsAscendantOf( TNode node )
        {
            Guard.Against.Null( node );

            TNode? current = node;

            do
            {
                if ( current == this )
                {
                    return true;
                }

                current = current.Parent;
            }
            while ( current != null );

            return false;
        }

        /// <summary>
        /// Returns true if the specified node is this node or an descendant of this node.
        /// </summary>
        /// <param name="node">The node to check for descendency.</param>
        public bool IsDescendantOf( TNode? node )
        {
            if ( node == null )
            {
                return false;
            }

            Node? parent = this;

            do
            {
                if ( parent == node )
                {
                    return true;
                }

                parent = parent.Parent;
            }
            while ( parent != null );

            return false;
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// A listener for handling click and mouse events on a tree widget.
    /// It extends the functionality of the ClickListener class to allow
    /// interaction with tree nodes, such as node selection, hover effects,
    /// and handling multi-selection or specific key-modifier interactions.
    /// </summary>
    [PublicAPI]
    public class TreeClickListener : ClickListener
    {
        private readonly Tree< TNode, TValue > _tree = null!;

        // ====================================================================

        /// <inheritdoc />
        public override void OnClicked( InputEvent ev, float x, float y )
        {
            TNode? node = _tree.GetNodeAt( y );

            if ( node == null )
            {
                return;
            }

            if ( node != _tree.GetNodeAt( TouchDownY ) )
            {
                return;
            }

            if ( _tree._selection.Multiple && _tree._selection.NotEmpty() && InputUtils.ShiftKey() )
            {
                // Select range (shift).
                _tree.RangeStart ??= node;

                TNode? rangeStart = _tree.RangeStart;

                if ( !InputUtils.CtrlKey() )
                {
                    _tree._selection.Clear();
                }

                if ( ( rangeStart.Actor == null ) || ( node.Actor == null ) )
                {
                    return;
                }

                float start = rangeStart.Actor.GetY();
                float end   = node.Actor.GetY();

                if ( start > end )
                {
                    _tree.SelectNodes( _tree.RootNodes, end, start );
                }
                else
                {
                    _tree.SelectNodes( _tree.RootNodes, start, end );
                    _tree._selection.Items().Reverse();
                }

                _tree._selection.FireChangeEvent();
                _tree.RangeStart = rangeStart;

                return;
            }

            if ( ( node.NodeChildren?.Count > 0 ) && ( !_tree._selection.Multiple || !InputUtils.CtrlKey() ) )
            {
                // Toggle expanded if left of icon.
                float? rowX = node.Actor?.GetX();

                if ( node.Icon != null )
                {
                    rowX -= _tree._iconSpacingRight + node.Icon.MinWidth;
                }

                if ( x < rowX )
                {
                    node.SetExpanded( !node.IsExpanded );

                    return;
                }
            }

            if ( !node.Selectable )
            {
                return;
            }

            _tree._selection.Choose( node );

            if ( !_tree._selection.IsEmpty )
            {
                _tree.RangeStart = node;
            }
        }

        /// <inheritdoc />
        public override bool OnMouseMoved( InputEvent? ev, float x, float y )
        {
            _tree.OverNode = _tree.GetNodeAt( y );

            return false;
        }

        /// <inheritdoc />
        public override void Enter( InputEvent? ev, float x, float y, int pointer, Actor? fromActor )
        {
            base.Enter( ev, x, y, pointer, fromActor );
            _tree.OverNode = _tree.GetNodeAt( y );
        }

        /// <inheritdoc />
        public override void Exit( InputEvent? ev, float x, float y, int pointer, Actor? toActor )
        {
            base.Exit( ev, x, y, pointer, toActor );

            if ( ( toActor == null ) || !toActor.IsDescendantOf( _tree ) )
            {
                _tree.OverNode = null;
            }
        }
    }
}

// ============================================================================
// ============================================================================