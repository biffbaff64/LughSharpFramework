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

namespace LughSharp.Source.Utils;

/// <summary>
/// Represents an integer value constrained within a defined minimum and maximum
/// range. Allows operations such as resetting, refilling, manual modification, and
/// boundary adjustments.
/// Triggers events when the value changes.
/// </summary>
[PublicAPI]
public class BoundedValue
{
    // Event triggered automatically whenever the CurrentTotal changes
    public event Action< int >? OnValueChanged;

    // Backing field to allow custom logic during value updates
    private int _currentTotal;

    // ========================================================================

    public int CurrentTotal
    {
        get => _currentTotal;
        private set
        {
            // Ensure the value never goes outside the allowed bounds
            int clampedValue = Math.Clamp( value, Minimum, Maximum );

            if ( _currentTotal != clampedValue )
            {
                _currentTotal = clampedValue;
                // Safely invoke the event to notify listeners (like UI)
                OnValueChanged?.Invoke( _currentTotal );
            }
        }
    }

    public int Minimum      { get; set; }
    public int Maximum      { get; set; }
    public int RefillAmount { get; set; }
    public int ResetAmount  { get; set; }

    // ========================================================================

    /// <summary>
    /// Default constructor. Sets all properties to default values.
    /// </summary>
    public BoundedValue() : this( 0, 100, 0 )
    {
    }
    
    /// <summary>
    /// Creates a new BoundedValue object, setting properties to the provided values.
    /// </summary>
    /// <param name="minimum"> The allowed Minimum value. </param>
    /// <param name="maximum"> The allowed Maximum value. </param>
    /// <param name="initialValue"> The initial value for CurrentTotal. </param>
    /// <param name="refillAmount"> The Refill amount. </param>
    /// <param name="resetAmount"> The Reset amount. </param>
    /// <exception cref="ArgumentException"></exception>
    public BoundedValue( int minimum, int maximum, int initialValue, int refillAmount = 0, int resetAmount = 0 )
    {
        if ( minimum > maximum )
        {
            throw new ArgumentException( "Minimum value cannot be greater than Maximum value." );
        }

        Minimum      = minimum;
        Maximum      = maximum;
        RefillAmount = refillAmount;
        ResetAmount  = resetAmount;

        // Set the backing field directly to avoid triggering events during initialization
        _currentTotal = Math.Clamp( initialValue, minimum, maximum );
    }

    /// <summary>
    /// Resets the total to the predefined ResetAmount.
    /// </summary>
    public void Reset()
    {
        CurrentTotal = ResetAmount;
    }

    /// <summary>
    /// Increases the total by the predefined RefillAmount.
    /// </summary>
    public void Refill()
    {
        CurrentTotal += RefillAmount;
    }

    /// <summary>
    /// Manually modifies the total by a custom amount.
    /// </summary>
    /// <param name="amount">The value to add (positive) or subtract (negative).</param>
    public void Modify( int amount )
    {
        CurrentTotal += amount;
    }

    /// <summary>
    /// Forces the total to immediately reach its Maximum bound.
    /// </summary>
    public void Maximize()
    {
        CurrentTotal = Maximum;
    }

    /// <summary>
    /// Forces the total to immediately drop to its Minimum bound.
    /// </summary>
    public void Minimize()
    {
        CurrentTotal = Minimum;
    }

    /// <summary>
    /// Convenience method which allows the setting of the Minimum and Maximum values.
    /// </summary>
    /// <param name="minimum"> The new allowed Minimum value. </param>
    /// <param name="maximum"> The new allowed Maximum value. </param>
    public void SetMinMax( int minimum, int maximum )
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    /// <summary>
    /// Forces the CurrentTotal to the Maximum allowed value.
    /// </summary>
    public void SetToMaximum()
    {
        CurrentTotal = Maximum;
    }

    /// <summary>
    /// Forces the CurrentTotal to the Minimum allowed value.
    /// </summary>
    public void SetToMinimum()
    {
        CurrentTotal = Minimum;
    }
    
    /// <summary>
    /// Returns <c>true</c> if the CurrentTotal is greater than, or equal to,
    /// the allowed maximum.
    /// </summary>
    public bool IsFull()
    {
        return CurrentTotal >= Maximum;
    }

    /// <summary>
    /// Returns <c>true</c> if the CurrentTotal is less than, or equal to,
    /// the allowed minimum value.
    /// </summary>
    public bool IsEmpty()
    {
        return CurrentTotal <= Minimum;
    }

    /// <summary>
    /// Returns <c>true</c> if the CurrentTotal is less than the allowed maximum.
    /// </summary>
    public bool HasRoom()
    {
        return GetFreeSpace() > 0;
    }

    /// <summary>
    /// Returns <c>true</c> if the CurrentTotal has exceeded the allowed Maximum.
    /// </summary>
    public bool IsOverflowing()
    {
        return CurrentTotal > Maximum;
    }

    /// <summary>
    /// Returns <c>true</c> if the CurrentTotal has gone under the allowed Minimum.
    /// </summary>
    public bool IsUnderflowing()
    {
        return CurrentTotal < Minimum;
    }

    /// <summary>
    /// Returns the amount of free space currently available in this BoundedValue.
    /// </summary>
    public int GetFreeSpace()
    {
        return Maximum - CurrentTotal;
    }

    /// <summary>
    /// Boosts the allowed Maximum by the specified amount.
    /// </summary>
    /// <param name="boost"> The amount to boost Maximum by. </param>
    public void BoostMax( int boost )
    {
        Maximum += boost;
    }

    /// <summary>
    /// Boosts the allowed Minimum by the specified amount.
    /// </summary>
    /// <param name="boost"> The amount to boost Minimum by. </param>
    public void BoostMin( int boost )
    {
        Minimum += boost;
    }
}

// ============================================================================
// ============================================================================
