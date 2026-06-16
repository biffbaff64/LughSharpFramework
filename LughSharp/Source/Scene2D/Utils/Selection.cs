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
using LughSharp.Source.Scene2D.Listeners;
using LughSharp.Source.Utils.Pooling;

namespace LughSharp.Source.Scene2D.Utils;

/// <summary>
/// Manages selected objects. Optionally fires a <see cref="ChangeListener.ChangeEvent"/>
/// on an actor.
/// <para>
/// Selection changes can be vetoed via <see cref="ChangeListener.ChangeEvent.Cancel()"/>.
/// </para>
/// </summary>
[PublicAPI]
public class Selection< T > : IDisableable, IDisposable
{
    private readonly SortedSet< T > _old = [ ];

    public SortedSet< T > Selected                 { get; set; } = [ ];
    public bool           Multiple                 { get; set; }
    public bool           Required                 { get; set; }
    public T?             LastSelected             { get; set; }
    public bool           Toggle                   { get; set; }
    public bool           ProgrammaticChangeEvents { get; set; } = true;
    public bool           IsDisabled               { get; set; }

    /// <summary>
    /// <param name="value">
    /// An actor to fire a <see cref="ChangeListener.ChangeEvent"/> on when the
    /// selection changes, or null.
    /// </param>
    /// </summary>
    public Actor? Actor { get; set; }

    public bool IsEmpty => Selected.Count == 0;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Selects or deselects the specified item based on how the selection is
    /// configured, whether ctrl is currently pressed, etc.
    /// <para>
    /// This is typically invoked by user interaction.
    /// </para>
    /// </summary>
    public virtual void Choose( T item )
    {
        Guard.Against.Null( item );

        if ( IsDisabled )
        {
            return;
        }

        Snapshot();

        try
        {
            if ( ( Toggle || InputUtils.CtrlKey() ) && Selected.Contains( item ) )
            {
                if ( Required && ( Selected.Count == 1 ) )
                {
                    return;
                }

                Selected.Remove( item );
                LastSelected = default( T? );
            }
            else
            {
                var modified = false;

                if ( !Multiple || ( !Toggle && !InputUtils.CtrlKey() ) )
                {
                    if ( ( Selected.Count == 1 ) && Selected.Contains( item ) )
                    {
                        return;
                    }

                    modified = Selected.Count > 0;
                    Selected.Clear();
                }

                if ( !Selected.Add( item ) && !modified )
                {
                    return;
                }

                LastSelected = item;
            }

            if ( FireChangeEvent() )
            {
                Revert();
            }
            else
            {
                OnChanged();
            }
        }
        finally
        {
            Cleanup();
        }
    }

    /// <summary>
    /// Returns TRUE if this set has items in it.
    /// </summary>
    public bool NotEmpty()
    {
        return !IsEmpty;
    }

    /// <summary>
    /// Returns the number of items in the selection.
    /// </summary>
    public int Size()
    {
        return Selected.Count;
    }

    /// <summary>
    /// Returns true if the selection has items in it.
    /// </summary>
    public bool HasItems()
    {
        return Selected.Count > 0;
    }

    /// <summary>
    /// Returns the items in the selection.
    /// </summary>
    public SortedSet< T > Items()
    {
        return Selected;
    }

    /// <summary>
    /// Returns the first selected item, or null.
    /// </summary>
    public T? First()
    {
        return Selected.Count == 0 ? default( T? ) : Selected.First();
    }

    /// <summary>
    /// Creates a snapshot of the current selection.
    /// </summary>
    public void Snapshot()
    {
        _old.Clear();

        foreach ( T item in Selected )
        {
            _old.Add( item );
        }
    }

    /// <summary>
    /// Reverts the selection to the snapshot taken by <see cref="Snapshot()"/>.
    /// </summary>
    public void Revert()
    {
        Selected.Clear();

        foreach ( T item in _old )
        {
            Selected.Add( item );
        }
    }

    /// <summary>
    /// Clears the snapshot.
    /// </summary>
    public void Cleanup()
    {
        _old.Clear();
    }

    /// <summary>
    /// Sets the selection to only the specified item.
    /// </summary>
    public void Set( T? item )
    {
        if ( item == null )
        {
            return;
        }

        if ( ( Selected.Count == 1 ) && Equals( Selected.First(), item ) )
        {
            return;
        }

        Snapshot();
        Selected.Clear();
        Selected.Add( item );

        if ( ProgrammaticChangeEvents && FireChangeEvent() )
        {
            Revert();
        }
        else
        {
            LastSelected = item;
            OnChanged();
        }

        Cleanup();
    }

    /// <summary>
    /// Sets the selection to the specified items.
    /// </summary>
    /// <param name="items"> The items to set the selection to. </param>
    /// <exception cref="ArgumentNullException"> Thrown if <paramref name="items"/> is null. </exception>
    public void SetAll( List< T > items )
    {
        if ( items == null )
        {
            throw new ArgumentNullException( nameof( items ) );
        }

        var added = false;

        Snapshot();

        LastSelected = default( T? );

        Selected.Clear();

        for ( int i = 0, n = items.Count; i < n; i++ )
        {
            T item = items[ i ];

            if ( item == null )
            {
                throw new ArgumentException( "item cannot be null." );
            }

            if ( Selected.Add( item ) )
            {
                added = true;
            }
        }

        if ( added )
        {
            if ( ProgrammaticChangeEvents && FireChangeEvent() )
            {
                Revert();
            }
            else if ( items.Count > 0 )
            {
                LastSelected = items.Peek();
                OnChanged();
            }
        }

        Cleanup();
    }

    /// <summary>
    /// Adds the item to the selection.
    /// </summary>
    public void Add( T item )
    {
        Guard.Against.Null( item );

        if ( !Selected.Add( item ) )
        {
            return;
        }

        if ( ProgrammaticChangeEvents && FireChangeEvent() )
        {
            Selected.Remove( item );
        }
        else
        {
            LastSelected = item;
            OnChanged();
        }
    }

    /// <summary>
    /// Adds all items from the supplied list to the selection.
    /// </summary>
    public void AddAll( List< T > items )
    {
        var added = false;

        Snapshot();

        for ( int i = 0, n = items.Count; i < n; i++ )
        {
            T item = items[ i ];

            if ( item == null )
            {
                throw new ArgumentException( "item cannot be null." );
            }

            if ( Selected.Add( item ) )
            {
                added = true;
            }
        }

        if ( added )
        {
            if ( ProgrammaticChangeEvents && FireChangeEvent() )
            {
                Revert();
            }
            else
            {
                LastSelected = items.Peek();
                OnChanged();
            }
        }

        Cleanup();
    }

    /// <summary>
    /// Removes the item from the selection.
    /// </summary>
    /// <param name="item"> The item to remove. </param>
    public void Remove( T item )
    {
        Guard.Against.Null( item );

        if ( !Selected.Remove( item ) )
        {
            return;
        }

        if ( ProgrammaticChangeEvents && FireChangeEvent() )
        {
            Selected.Add( item );
        }
        else
        {
            LastSelected = default( T? );
            OnChanged();
        }
    }

    /// <summary>
    /// Removes all specified items from the selection.
    /// </summary>
    /// <param name="items"> The items to remove. </param>
    /// <exception cref="ArgumentNullException"> Thrown if <paramref name="items"/> is null. </exception>
    public void RemoveAll( List< T > items )
    {
        var removed = false;

        Snapshot();

        Guard.Against.Null( items );

        for ( int i = 0, n = items.Count; i < n; i++ )
        {
            T item = items[ i ];

            if ( item == null )
            {
                throw new ArgumentException( "item cannot be null." );
            }

            if ( Selected.Remove( item ) )
            {
                removed = true;
            }
        }

        if ( removed )
        {
            if ( ProgrammaticChangeEvents && FireChangeEvent() )
            {
                Revert();
            }
            else
            {
                LastSelected = default( T? );
                OnChanged();
            }
        }

        Cleanup();
    }

    /// <summary>
    /// Clears all selected items from the selection.
    /// Ensures that any state or references linked to the previously
    /// selected items are reset appropriately.
    /// <para>
    /// If programmatic change events are enabled and a change event
    /// is fired but canceled, the selection state will revert to its
    /// previous state. Otherwise, the selection is cleared, the last
    /// selected item is reset, and the necessary cleanup is performed.
    /// </para>
    /// </summary>
    public void Clear()
    {
        if ( Selected.Count == 0 )
        {
            return;
        }

        Snapshot();

        Selected.Clear();

        if ( ProgrammaticChangeEvents && FireChangeEvent() )
        {
            Revert();
        }
        else
        {
            LastSelected = default( T? );
            OnChanged();
        }

        Cleanup();
    }

    /// <summary>
    /// Called after the selection changes. The default implementation does nothing.
    /// </summary>
    protected virtual void OnChanged()
    {
    }

    /// <summary>
    /// Fires a change event on the selection's actor, if any. Called internally when
    /// the selection changes, depending on <see cref="ProgrammaticChangeEvents"/>.
    /// </summary>
    /// <returns> true if the change should be undone. </returns>
    public virtual bool FireChangeEvent()
    {
        if ( Actor == null )
        {
            return false;
        }

        var changeEvent = PoolsMap.Obtain< ChangeListener.ChangeEvent >();

        if ( changeEvent == null )
        {
            return false;
        }

        try
        {
            return Actor.Fire( changeEvent );
        }
        finally
        {
            PoolsMap.Free< ChangeListener.ChangeEvent >( changeEvent );
        }
    }

    /// <summary>
    /// Returns true if the selection contains the specified item.
    /// </summary>
    public bool Contains( T? item )
    {
        return ( item != null ) && Selected.Contains( item );
    }

    /// <summary>
    /// Makes a best effort to return the last item selected, else returns an
    /// arbitrary item or null if the selection is empty.
    /// </summary>
    public T? GetLastSelected()
    {
        if ( LastSelected != null )
        {
            return LastSelected;
        }

        return Selected.Count > 0 ? Selected.First() : default( T? );
    }

    /// <summary>
    /// Returns a list of the selected items.
    /// </summary>
    public List< T > ToArray()
    {
        return Selected.ToList();
    }

    /// <summary>
    /// Converts the selected items and the provided array into a single list of items.
    /// </summary>
    /// <param name="array">A list of items to be combined with the selected items.</param>
    /// <returns>A list containing all selected items and the items from the provided array.</returns>
    public List< T > ToArray( List< T > array )
    {
        List< T > list = Selected.ToList();

        list.AddRange( array );

        return list;
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string? ToString()
    {
        return Selected.ToString();
    }

    // ========================================================================

    #region dispose pattern

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
        
        GC.SuppressFinalize( this );
    }

    protected void Dispose( bool disposing )
    {
        if ( disposing )
        {
        }
    }

    #endregion dispose pattern
}

// ============================================================================
// ============================================================================

