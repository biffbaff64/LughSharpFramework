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

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// Manages a group of buttons to enforce a minimum and maximum number of checked buttons. This
/// enables "radio button" functionality and more. A button may only be in one group at a time.
/// <para>
/// The <see cref="CanCheck(T, bool)"/> method can be overridden to control if a button check
/// or uncheck is allowed.
/// </para>
/// </summary>
[PublicAPI]
public class ButtonGroup< T > where T : Button
{
    /// <summary>
    /// Gets or sets the list of buttons managed by the button group. The buttons in
    /// this list can be checked or unchecked, depending on the rules defined for the
    /// group, such as minimum or maximum number of buttons that can be checked at any
    /// time. Each button in the list is assigned to this group, and adding or removing
    /// a button from the list will update its associated group reference accordingly.
    /// </summary>
    public List< T > Buttons { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of buttons that are currently checked within the group.
    /// This property contains all the buttons from the group that have their checked
    /// state set to true. Changes to this list will also affect the button states,
    /// ensuring consistency with the group's rules for checked buttons, such as
    /// minimum and maximum limits.
    /// </summary>
    public List< T > CheckedButtons { get; set; } = new( 1 );

    // ========================================================================

    private T    _lastChecked   = null!;
    private bool _uncheckLast   = true;
    private int  _maxCheckCount = 1;
    private int  _minCheckCount;

    // ========================================================================

    /// <summary>
    /// Creates a new button group with a minimum check count of 1.
    /// </summary>
    public ButtonGroup()
    {
        _minCheckCount = 1;
    }

    /// <summary>
    /// Creates a new button group with the specified buttons and a minimum check count of 1.
    /// </summary>
    /// <param name="buttons"> The buttons to be added to the group. </param>
    public ButtonGroup( params T[] buttons )
    {
        _minCheckCount = 0;

        Add( buttons );

        _minCheckCount = 1;
    }

    /// <summary>
    /// Adds the specified button to the group.
    /// </summary>
    /// <param name="button"> The button to be added to the group. </param>
    public void Add( T button )
    {
        button.ButtonGroup = null!;

        bool shouldCheck = button.IsChecked || ( Buttons.Count < _minCheckCount );

        button.SetChecked( false );
        button.ButtonGroup = ( this as ButtonGroup< Button > )!;

        Buttons.Add( button );

        button.SetChecked( shouldCheck );
    }

    /// <summary>
    /// Adds the specified buttons to the group.
    /// </summary>
    /// <param name="buttons"> The buttons to be added to the group. </param>
    public void Add( T[] buttons )
    {
        for ( int i = 0, n = buttons.Length; i < n; i++ )
        {
            Add( buttons[ i ] );
        }
    }

    /// <summary>
    /// Adds the specified buttons list to the group.
    /// </summary>
    /// <param name="buttons"></param>
    public void Add( List< T > buttons )
    {
        Add( buttons.ToArray() );
    }
    
    /// <summary>
    /// Removes the specified button from the group.
    /// </summary>
    /// <param name="button"> The button to be removed from the group. </param>
    public void Remove( T button )
    {
        button.ButtonGroup = null!;

        Buttons.Remove( button );
        CheckedButtons.Remove( button );
    }

    /// <summary>
    /// Removes the specified buttons from the group.
    /// </summary>
    /// <param name="buttons"> The buttons to be removed from the group. </param>
    public void Remove( T[] buttons )
    {
        for ( int i = 0, n = buttons.Length; i < n; i++ )
        {
            Remove( buttons[ i ] );
        }
    }

    /// <summary>
    /// Clears the group of all buttons.
    /// </summary>
    public void Clear()
    {
        Buttons.Clear();
        CheckedButtons.Clear();
    }

    /// <summary>
    /// Sets the first <see cref="TextButton"/> with the specified text to checked.
    /// </summary>
    public void SetChecked( string text )
    {
        for ( int i = 0, n = Buttons.Count; i < n; i++ )
        {
            Button button = Buttons[ i ];

            if ( ( button.GetType() == typeof( TextButton ) )
              && text.Equals( ( ( TextButton )button ).GetText() ) )
            {
                button.SetChecked( true );

                return;
            }
        }
    }

    /// <summary>
    /// Called when a button is checked or unchecked. If overridden, generally changing button
    /// checked states should not be done from within this method.
    /// </summary>
    /// <returns> True if the new state should be allowed. </returns>
    public bool CanCheck( T button, bool newState )
    {
        if ( button.IsChecked == newState )
        {
            return false;
        }

        if ( !newState )
        {
            // Keep button checked to enforce minCheckCount.
            if ( CheckedButtons.Count <= _minCheckCount )
            {
                return false;
            }

            CheckedButtons.Remove( button );
        }
        else
        {
            // Keep button unchecked to enforce maxCheckCount.
            if ( ( _maxCheckCount != -1 ) && ( CheckedButtons.Count >= _maxCheckCount ) )
            {
                if ( _uncheckLast )
                {
                    int old = _minCheckCount;

                    _minCheckCount = 0;
                    _lastChecked.SetChecked( false );
                    _minCheckCount = old;
                }
                else
                {
                    return false;
                }
            }

            CheckedButtons.Add( button );
            _lastChecked = button;
        }

        return true;
    }

    /// <summary>
    /// Sets all buttons' <see cref="Button.IsChecked"/> property to false, regardless
    /// of <see cref="SetMinCheckCount(int)"/>.
    /// </summary>
    public void UncheckAll()
    {
        int old = _minCheckCount;

        _minCheckCount = 0;

        for ( int i = 0, n = Buttons.Count; i < n; i++ )
        {
            T button = Buttons[ i ];
            button.SetChecked( false );
        }

        _minCheckCount = old;
    }

    /// <summary>
    /// Returns the first checked button, or null.
    /// </summary>
    public T? GetFirstChecked()
    {
        return CheckedButtons.Count > 0 ? CheckedButtons[ 0 ] : null;
    }

    /// <summary>
    /// Returns the first checked button index, or -1.
    /// </summary>
    public int GetFirstCheckedIndex()
    {
        if ( CheckedButtons.Count > 0 )
        {
            return Buttons.IndexOf( CheckedButtons[ 0 ] );
        }

        return -1;
    }

    /// <summary>
    /// Sets the minimum number of buttons that must be checked. Default is 1.
    /// </summary>
    public void SetMinCheckCount( int minCheckCount = 1 )
    {
        _minCheckCount = minCheckCount;
    }

    /// <summary>
    /// Sets the maximum number of buttons that can be checked. Set to -1 for no maximum.
    /// Default is 1.
    /// </summary>
    public void SetMaxCheckCount( int maxCheckCount = 1 )
    {
        if ( maxCheckCount == 0 )
        {
            maxCheckCount = -1;
        }

        _maxCheckCount = maxCheckCount;
    }

    /// <summary>
    /// If true, when the maximum number of buttons are checked and an additional button is
    /// checked, the last button to be checked is unchecked so that the maximum is not exceeded.
    /// If false, additional buttons beyond the maximum are not allowed to be checked.
    /// Default is true.
    /// </summary>
    public void SetUncheckLast( bool uncheckLast = true )
    {
        _uncheckLast = uncheckLast;
    }
}

// ============================================================================
// ============================================================================