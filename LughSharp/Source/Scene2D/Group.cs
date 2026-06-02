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
using LughSharp.Source.Graphics.Utils;
using LughSharp.Source.Scene2D.Utils;

using Rectangle = LughSharp.Source.Maths.Rectangle;

namespace LughSharp.Source.Scene2D;

/// <summary>
/// 2D scene graph node that may contain other actors.
/// <para>
/// Actors have a z-order equal to the order they were inserted into the group. Actors inserted
/// later will be drawn on top of actors added earlier. Touch events that hit more than one actor
/// are distributed to topmost actors first.
/// </para>
/// </summary>
[PublicAPI]
public class Group : Actor, ICullable
{
    public SnapshotArrayList< Actor > Children { get; set; } = new( 4 );

    /// <summary>
    /// When true (the default), the Batch is transformed so children are drawn
    /// in their parent's coordinate system. This has a performance impact because
    /// <see cref="IBatch.Flush()"/> must be done before and after the transform.
    /// If the actors in a group are not rotated or scaled, then the transform for
    /// the group can be set to false. In this case, each child's position will be
    /// offset by the group's position for drawing, causing the children to appear
    /// in the correct location even though the Batch has not been transformed.
    /// </summary>
    public bool Transform { get; set; } = true;

    /// <summary>
    /// The area used for culling. If null, no culling is done.
    /// </summary>
    public Rectangle? CullingArea { get; set; }

    // ========================================================================

    private readonly Matrix4 _computedTransform = new();
    private readonly Matrix4 _oldTransform      = new();
    private readonly Vector2 _tmp               = new();
    private readonly Affine2 _worldTransform    = new();

    // ========================================================================

    /// <inheritdoc />
    public override void Act( float delta )
    {
        base.Act( delta );

        Actor[] actors = Children.Begin();

        for ( int i = 0, n = Children.Size; i < n; i++ )
        {
            actors[ i ].Act( delta );
        }

        Children.End();
    }

    /// <summary>
    /// Draws the group and its children.
    /// <para>
    /// The default implementation calls <see cref="ApplyTransform(IBatch, Matrix4)"/> if needed,
    /// then <see cref="DrawChildren(IBatch, float)"/>, then <see cref="ResetTransform(IBatch)"/>
    /// if needed.
    /// </para>
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="parentAlpha"></param>
    public override void Draw( IBatch batch, float parentAlpha )
    {
        if ( Transform )
        {
            ApplyTransform( batch, ComputeTransform() );
        }

        DrawChildren( batch, parentAlpha );

        if ( Transform )
        {
            ResetTransform( batch );
        }
    }

    /// <summary>
    /// Draws all children.
    /// <para>
    /// <see cref="ApplyTransform(IBatch, Matrix4)"/> should be called before and
    /// <see cref="ResetTransform(IBatch)"/> after this method if <see cref="Transform"/>
    /// is true.
    /// </para>
    /// <para>
    /// If <see cref="Transform"/> is false these methods don't need to be called,
    /// children positions are temporarily offset by the group position when drawn.
    /// This method avoids drawing children completely outside the
    /// <see cref="CullingArea"/> culling area, if set.
    /// </para>
    /// </summary>
    protected void DrawChildren( IBatch batch, float parentAlpha )
    {
        parentAlpha *= ActorColor.A;

        Actor[] actors = Children.Begin();

        if ( CullingArea != null )
        {
            // Draw children only if inside culling area.
            float cullLeft   = CullingArea.X;
            float cullRight  = cullLeft + CullingArea.Width;
            float cullBottom = CullingArea.Y;
            float cullTop    = cullBottom + CullingArea.Height;

            if ( Transform )
            {
                for ( int i = 0, n = Children.Size; i < n; i++ )
                {
                    Actor child = actors[ i ];

                    if ( !child.IsVisible )
                    {
                        continue;
                    }

                    float cx = child.GetX();
                    float cy = child.GetY();

                    if ( ( cx <= cullRight )
                      && ( cy <= cullTop )
                      && ( ( cx + child.GetWidth() ) >= cullLeft )
                      && ( ( cy + child.GetHeight() ) >= cullBottom ) )
                    {
                        child.Draw( batch, parentAlpha );
                    }
                }
            }
            else
            {
                // No transform for this group, offset each child.
                float offsetX = GetX();
                float offsetY = GetY();

                SetX( 0 );
                SetY( 0 );

                for ( int i = 0, n = Children.Size; i < n; i++ )
                {
                    Actor child = actors[ i ];

                    if ( !child.IsVisible )
                    {
                        continue;
                    }

                    float cx = child.GetX();
                    float cy = child.GetY();

                    if ( ( cx <= cullRight )
                      && ( cy <= cullTop )
                      && ( ( cx + child.GetWidth() ) >= cullLeft )
                      && ( ( cy + child.GetHeight() ) >= cullBottom ) )
                    {
                        child.SetX( cx + offsetX );
                        child.SetY( cy + offsetY );

                        child.Draw( batch, parentAlpha );

                        child.SetX( cx );
                        child.SetY( cy );
                    }
                }

                SetX( offsetX );
                SetY( offsetY );
            }
        }
        else
        {
            // No culling, draw all children.
            if ( Transform )
            {
                for ( int i = 0, n = Children.Size; i < n; i++ )
                {
                    Actor child = actors[ i ];

                    if ( !child.IsVisible )
                    {
                        continue;
                    }

                    child.Draw( batch, parentAlpha );
                }
            }
            else
            {
                // No transform for this group, offset each child.
                float offsetX = GetX();
                float offsetY = GetY();

                SetX( 0 );
                SetY( 0 );

                for ( int i = 0, n = Children.Size; i < n; i++ )
                {
                    Actor child = actors[ i ];

                    if ( !child.IsVisible )
                    {
                        continue;
                    }

                    float cx = child.GetX();
                    float cy = child.GetY();

                    child.SetX( cx + offsetX );
                    child.SetY( cy + offsetY );

                    child.Draw( batch, parentAlpha );

                    child.SetX( cx );
                    child.SetY( cy );
                }

                SetX( offsetX );
                SetY( offsetY );
            }
        }

        Children.End();
    }

    /// <summary>
    /// Draws this actor's debug lines if <see cref="Actor.DebugActive"/> is
    /// true and, regardless of <see cref="Actor.DebugActive"/>, calls
    /// <see cref="Actor.DrawDebug(ShapeRenderer)"/> on each child.
    /// </summary>
    public override void DrawDebug( ShapeRenderer shapes )
    {
        DrawDebugBounds( shapes );

        if ( Transform )
        {
            ApplyTransform( shapes, ComputeTransform() );
        }

        DrawDebugChildren( shapes );

        if ( Transform )
        {
            ResetTransform( shapes );
        }
    }

    /// <summary>
    /// Draws all children. <see cref="ApplyTransform(IBatch, Matrix4)"/> should be
    /// called before and <see cref="ResetTransform(IBatch)"/> after this method if
    /// <see cref="Transform"/> is true.
    /// <para>
    /// If <see cref="Transform"/> is false these methods don't need to be called,
    /// children positions are temporarily offset by the group position when drawn.
    /// This method avoids drawing children completely outside the
    /// <see cref="CullingArea"/> culling area, if set.
    /// </para>
    /// </summary>
    protected void DrawDebugChildren( ShapeRenderer shapes )
    {
        Actor?[] actors = Children.Begin();

        // No culling, draw all children.
        if ( Transform )
        {
            for ( int i = 0, n = Children.Size; i < n; i++ )
            {
                Actor? child = actors[ i ];

                if ( child == null )
                {
                    continue;
                }

                if ( !child.IsVisible )
                {
                    continue;
                }

                if ( !child.DebugActive && child is not Group )
                {
                    continue;
                }

                child.DrawDebug( shapes );
            }

            shapes.Flush();
        }
        else
        {
            // No transform for this group, offset each child.
            float offsetX = GetX();
            float offsetY = GetY();

            SetX( 0 );
            SetY( 0 );

            for ( int i = 0, n = Children.Size; i < n; i++ )
            {
                Actor? child = actors[ i ];

                if ( child == null )
                {
                    continue;
                }

                if ( !child.IsVisible )
                {
                    continue;
                }

                if ( !child.DebugActive && child is not Group )
                {
                    continue;
                }

                float cx = child.GetX();
                float cy = child.GetY();

                child.SetX( cx + offsetX );
                child.SetY( cy + offsetY );
                child.DrawDebug( shapes );
                child.SetX( cx );
                child.SetY( cy );
            }

            SetX( offsetX );
            SetY( offsetY );
        }

        Children.End();
    }

    /// <summary>
    /// Returns the transform for this group's coordinate system.
    /// </summary>
    protected Matrix4 ComputeTransform()
    {
        _worldTransform.SetToTrnRotScl( GetX() + OriginX, GetY() + OriginY, Rotation, ScaleX, ScaleY );

        if ( ( OriginX != 0 ) || ( OriginY != 0 ) )
        {
            _worldTransform.Translate( -OriginX, -OriginY );
        }

        // Find the first parent that transforms.
        Group? parentGroup = Parent;

        while ( parentGroup != null )
        {
            if ( parentGroup.Transform )
            {
                break;
            }

            parentGroup = parentGroup.Parent;
        }

        if ( parentGroup != null )
        {
            _worldTransform.PreMul( parentGroup._worldTransform );
        }

        _computedTransform.Set( _worldTransform );

        return _computedTransform;
    }

    /// <summary>
    /// Set the batch's transformation matrix, often with the result of
    /// <see cref="ComputeTransform()"/>. Note this causes the batch to
    /// be flushed.
    /// <see cref="ResetTransform(IBatch)"/> will restore the transform
    /// to what it was before this call.
    /// </summary>
    protected void ApplyTransform( IBatch batch, Matrix4 transform )
    {
        _oldTransform.Set( batch.TransformMatrix );

        batch.SetTransformMatrix( transform );
    }

    /// <summary>
    /// Restores the batch transform to what it was before
    /// <see cref="ApplyTransform(IBatch, Matrix4)"/>.
    /// Note this causes the batch to be flushed.
    /// </summary>
    protected void ResetTransform( IBatch batch )
    {
        batch.SetTransformMatrix( _oldTransform );
    }

    /// <summary>
    /// Set the shape renderer transformation matrix, often with the result of
    /// <see cref="ComputeTransform()"/>.
    /// <para>
    /// Note this causes the shape renderer to be flushed.
    /// <see cref="ResetTransform(ShapeRenderer)"/> will restore the transform
    /// to what it was before this call.
    /// </para>
    /// </summary>
    protected void ApplyTransform( ShapeRenderer shapes, Matrix4 transform )
    {
        _oldTransform.Set( shapes.TransformMatrix );

        shapes.TransformMatrix = transform;
        shapes.Flush();
    }

    /// <summary>
    /// Restores the shape renderer transform to what it was before
    /// <see cref="ApplyTransform(IBatch, Matrix4)"/>.
    /// Note this causes the shape renderer to be flushed.
    /// </summary>
    protected void ResetTransform( ShapeRenderer shapes )
    {
        shapes.TransformMatrix = _oldTransform;
    }

    /// <summary>
    /// Determines the topmost child actor at the specified coordinates that is visible and,
    /// if required, touchable.
    /// </summary>
    /// <remarks>
    /// The method traverses child actors in reverse order, returning the first child that is 
    /// hit. If no child is hit, the method delegates to the base implementation. Coordinates
    /// are interpreted in the local coordinate system of each child.
    /// </remarks>
    /// <param name="x">The x-coordinate, in the parent's local coordinate system, to test for a hit.</param>
    /// <param name="y">The y-coordinate, in the parent's local coordinate system, to test for a hit.</param>
    /// <param name="touchable">
    /// true to consider only actors that are touchable; otherwise, false to include all actors
    /// regardless of their touchable state.</param>
    /// <returns>
    /// The topmost Actor at the specified coordinates that meets the visibility and touchability
    /// criteria; otherwise, null if no such actor is found.
    /// </returns>
    public override Actor? Hit( float x, float y, bool touchable )
    {
        if ( touchable && ( Touchable == Touchable.Disabled ) )
        {
            return null;
        }

        if ( !IsVisible )
        {
            return null;
        }

        for ( int i = Children.Size - 1; i >= 0; i-- )
        {
            Actor child = Children.GetAt( i );

            child.ParentToLocalCoordinates( _tmp.Set( x, y ) );

            Actor? hit = child.Hit( _tmp.X, _tmp.Y, touchable );

            if ( hit != null )
            {
                return hit;
            }
        }

        return base.Hit( x, y, touchable );
    }

    /// <summary>
    /// Called when actors are added to or removed from the group.
    /// </summary>
    protected virtual void ChildrenChanged()
    {
    }

    /// <summary>
    /// Adds an actor as a child of this group, removing it from its previous
    /// parent. If the actor is already a child of this group, no changes are made.
    /// </summary>
    public void AddActor( Actor actor )
    {
        if ( actor.Parent != null )
        {
            if ( actor.Parent == this )
            {
                return;
            }

            actor.Parent.RemoveActor( actor, false );
        }

        Children.Add( actor );

        actor.Parent = this;
        actor.SetStage( GetStage() );

        ChildrenChanged();
    }

    /// <summary>
    /// Adds an actor as a child of this group at a specific index, removing
    /// it from its previous parent. If the actor is already a child of this
    /// group, no changes are made.
    /// </summary>
    /// <param name="index"> May be greater than the number of children. </param>
    /// <param name="actor"></param>
    public virtual void AddActorAt( int index, Actor actor )
    {
        if ( actor.Parent != null )
        {
            if ( actor.Parent == this )
            {
                return;
            }

            actor.Parent.RemoveActor( actor, false );
        }

        if ( index >= Children.Size )
        {
            Children.Add( actor );
        }
        else
        {
            Children.Insert( index, actor );
        }

        actor.Parent = this;
        actor.SetStage( GetStage() );

        ChildrenChanged();
    }

    /// <summary>
    /// Adds an actor as a child of this group immediately before another child
    /// actor, removing it from its previous parent. If the actor is already a
    /// child of this group, no changes are made.
    /// </summary>
    public virtual void AddActorBefore( Actor actorBefore, Actor actor )
    {
        if ( actor.Parent != null )
        {
            if ( actor.Parent == this )
            {
                return;
            }

            actor.Parent.RemoveActor( actor, false );
        }

        int index = Children.IndexOf( actorBefore );

        Children.Insert( index, actor );

        actor.Parent = this;
        actor.SetStage( GetStage() );

        ChildrenChanged();
    }

    /// <summary>
    /// Adds an actor as a child of this group immediately after another child actor,
    /// removing it from its previous parent. If the actor is already a child of this
    /// group, no changes are made. If <tt>actorAfter</tt> is not in this group, the
    /// actor is added as the last child.
    /// </summary>
    public virtual void AddActorAfter( Actor actorAfter, Actor actor )
    {
        if ( actor.Parent != null )
        {
            if ( actor.Parent == this )
            {
                return;
            }

            actor.Parent.RemoveActor( actor, false );
        }

        int index = Children.IndexOf( actorAfter );

        if ( ( index == Children.Size ) || ( index == -1 ) )
        {
            Children.Add( actor );
        }
        else
        {
            Children.Insert( index + 1, actor );
        }

        actor.Parent = this;
        actor.SetStage( GetStage() );

        ChildrenChanged();
    }

    /// <summary>
    /// Removes an actor from this group.
    /// <param name="actor"> The actor to remove. </param>
    /// <param name="unfocus"> Unfocuses the actor if true. </param>
    /// </summary>
    public virtual bool RemoveActor( Actor actor, bool unfocus )
    {
        int index = Children.IndexOf( actor );

        if ( index == -1 )
        {
            return false;
        }

        RemoveActorAt( index, unfocus );

        return true;
    }

    /// <summary>
    /// Removes an actor from this group. If the actor will not be used again and
    /// has actions, they should be cleared using <see cref="Actor.ClearActions()"/>
    /// so the actions will be returned to their <see cref="SceneAction.Pool"/>, if
    /// any. This is not done automatically.
    /// </summary>
    /// <param name="index"> The group index of the actor to remove. </param>
    /// <param name="unfocus"> Unfocuses the actor if true. </param>
    /// <returns> The actor removed from this group. </returns>
    public virtual Actor? RemoveActorAt( int index, bool unfocus )
    {
        Actor actor = Children.RemoveAt( index );

        if ( GetStage() != null )
        {
            if ( unfocus )
            {
                GetStage().Unfocus( actor );
            }

            actor.Parent = null;
            actor.SetStage( null );
        }

        ChildrenChanged();

        return actor;
    }

    /// <summary>
    /// Removes all actors from this group.
    /// </summary>
    public virtual void ClearChildren( bool unfocus = true )
    {
        Actor?[] actors = Children.Begin();

        for ( int i = 0, n = Children.Size; i < n; i++ )
        {
            Actor? child = actors[ i ];

            if ( child != null )
            {
                if ( unfocus )
                {
                    GetStage()?.Unfocus( child );
                }

                child.SetStage( null );
                child.Parent = null;
            }
        }

        Children.End();
        Children.Clear();

        ChildrenChanged();
    }

    /// <summary>
    /// Removes all children, actions, and listeners from this group.
    /// </summary>
    public override void Clear()
    {
        base.Clear();
        ClearChildren();
    }

    /// <summary>
    /// Returns the first actor found with the specified name. Note this recursively
    /// compares the name of every actor in the group.
    /// </summary>
    public T? FindActor< T >( string name ) where T : Actor
    {
        for ( int i = 0, n = Children.Size; i < n; i++ )
        {
            Actor child = Children.GetAt( i );

            if ( name.Equals( child.GetType().Name ) )
            {
                return ( T )child;
            }
        }

        for ( int i = 0, n = Children.Size; i < n; i++ )
        {
            Actor child = Children.GetAt( i );

            if ( child is Group group )
            {
                Actor? actor = group.FindActor< T >( name );

                if ( actor != null )
                {
                    return ( T )actor;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Sets the stage for this groups actors to act upon.
    /// </summary>
    public override void SetStage( Stage? stage )
    {
        base.SetStage( stage );

        for ( int i = 0, n = Children.Size; i < n; i++ )
        {
            Children.GetAt( i ).SetStage( stage );
        }
    }

    /// <summary>
    /// Swaps two actors by index. Returns false if the swap did not
    /// occur because the indexes were out of bounds.
    /// </summary>
    public bool SwapActor( int first, int second )
    {
        int maxIndex = Children.Size;

        if ( ( first < 0 ) || ( first >= maxIndex ) )
        {
            return false;
        }

        if ( ( second < 0 ) || ( second >= maxIndex ) )
        {
            return false;
        }

        Children.Swap( first, second );

        return true;
    }

    /// <summary>
    /// Swaps two actors. Returns false if the swap did not occur
    /// because the actors are not children of this group.
    /// </summary>
    public bool SwapActor( Actor first, Actor second )
    {
        int firstIndex  = Children.IndexOf( first );
        int secondIndex = Children.IndexOf( second );

        if ( ( firstIndex == -1 ) || ( secondIndex == -1 ) )
        {
            return false;
        }

        Children.Swap( firstIndex, secondIndex );

        return true;
    }

    /// <summary>
    /// Returns the child at the specified index.
    /// </summary>
    public Actor GetChild( int index )
    {
        return Children.GetAt( index );
    }

    /// <summary>
    /// Returns true if this group has children.
    /// </summary>
    public bool HasChildren()
    {
        return Children.Size > 0;
    }

    /// <summary>
    /// Converts coordinates for this group to those of a descendant actor.
    /// The descendant does not need to be a direct child.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// if the specified actor is not a descendant of this group.
    /// </exception>
    public Vector2 LocalToDescendantCoordinates( Actor descendant, Vector2 localCoords )
    {
        Group? parent = descendant.Parent;

        if ( parent == null )
        {
            throw new ArgumentException( $"Child is not a descendant: {descendant}" );
        }

        // First convert to the actor's parent coordinates.
        if ( parent != this )
        {
            LocalToDescendantCoordinates( parent, localCoords );
        }

        // Then from each parent down to the descendant.
        descendant.ParentToLocalCoordinates( localCoords );

        return localCoords;
    }

    /// <summary>
    /// If true, <see cref="DrawDebug(ShapeRenderer)"/> will be called for this
    /// group and, optionally, all children recursively.
    /// </summary>
    public void SetDebug( bool enabled, bool recursively )
    {
        DebugActive = enabled;

        if ( recursively )
        {
            foreach ( Actor child in Children )
            {
                if ( child is Group group )
                {
                    group.SetDebug( enabled, recursively );
                }
                else
                {
                    child.DebugActive = enabled;
                }
            }
        }
    }

    /// <summary>
    /// Enables debug mode for this group and all its children recursively.
    /// </summary>
    /// <returns> This group for chaining. </returns>
    public virtual Group DebugAll()
    {
        SetDebug( true, true );

        return this;
    }

    /// <summary>
    /// Returns a description of the actor hierarchy, recursively.
    /// </summary>
    public override string ToString()
    {
        var buffer = new StringBuilder( 128 );

        ToString( buffer, 1 );

        return buffer.ToString();
    }

    /// <summary>
    /// Appends a string representation of the current object and its child actors to the
    /// specified buffer, using indentation to reflect the hierarchy.
    /// </summary>
    /// <remarks>
    /// This method is intended for internal use to build a hierarchical textual representation
    /// of the object and its children. It recursively processes child groups, increasing the
    /// indentation for each level.
    /// </remarks>
    /// <param name="buffer">
    /// The buffer to which the string representation is appended. Must not be null, otherwise
    /// an exception will be thrown.
    /// </param>
    /// <param name="indent">
    /// The number of indentation levels to apply for child actors. Must be zero or greater. If
    /// a negative value is provided, it will default to zero
    /// </param>
    private void ToString( StringBuilder buffer, int indent )
    {
        Guard.Against.Null( buffer );
        
        buffer.Append( base.ToString() ?? string.Empty );
        buffer.Append( '\n' );

        Actor?[] actors = Children.Begin();

        indent = Math.Max( 0, indent );
        
        for ( int i = 0, n = Children.Size; i < n; i++ )
        {
            for ( var ii = 0; ii < indent; ii++ )
            {
                buffer.Append( "|  " );
            }

            if ( actors[ i ] is Group group )
            {
                group.ToString( buffer, indent + 1 );
            }
            else
            {
                buffer.Append( actors[ i ] );
                buffer.Append( '\n' );
            }
        }

        Children.End();
    }
}

// ============================================================================
// ============================================================================