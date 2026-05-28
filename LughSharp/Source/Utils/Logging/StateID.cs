// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024 Richard Ikin.
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

namespace LughSharp.Source.Utils.Logging;

/// <summary>
/// Class representing Common Game States used in most implementations.
/// Extend this to add more.
/// </summary>
[PublicAPI]
public class StateID
{
    public static readonly StateID StateSetup  = new( 1, "StateSetup" );
    public static readonly StateID StatePaused = new( 2, "StatePaused" );

    // ------------------------------------------
    public static readonly StateID Inactive = new( 3, "Inactive" );
    public static readonly StateID Limbo    = new( 4, "Limbo" );
    public static readonly StateID Init     = new( 5, "Init" );
    public static readonly StateID Update   = new( 6, "Update" );
    public static readonly StateID Close    = new( 7, "Close" );

    // ------------------------------------------
    public static readonly StateID StateOpen    = new( 8, "StateOpen" );
    public static readonly StateID StateOpening = new( 9, "StateOpening" );
    public static readonly StateID StateClosing = new( 10, "StateClosing" );
    public static readonly StateID StateClosed  = new( 11, "StateClosed" );

    // ------------------------------------------
    public static readonly StateID StateFlashing = new( 12, "StateFlashing" );
    public static readonly StateID StateSteady   = new( 13, "StateSteady" );

    // ------------------------------------------
    public static readonly StateID StateZoomIn  = new( 14, "StateZoomIn" );
    public static readonly StateID StateZoomOut = new( 15, "StateZoomOut" );

    // ------------------------------------------
    public static readonly StateID StatePowerUp   = new( 16, "StatePowerUp" );
    public static readonly StateID StatePowerDown = new( 17, "StatePowerDown" );

    // ------------------------------------------
    public static readonly StateID StateDebugHang = new( 18, "StateDebugHang" );

    // ========================================================================

    public readonly int    ID;
    public readonly string Name;

    // ========================================================================

    /// <summary>
    /// Creates a new StateID with the specified ID and Name.
    /// </summary>
    /// <param name="id">The unique identifier for the state.</param>
    /// <param name="name">The name of the state.</param>
    protected StateID( int id, string name )
    {
        ID   = id;
        Name = name;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current StateID instance.   
    /// </summary>
    /// <param name="obj">The object to compare with the current StateID instance.</param>
    /// <returns>
    /// <c>true</c> if the specified object is a StateID and is equal to the current instance;
    /// otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals( object? obj ) => obj is StateID other && Equals( other );

    /// <summary>
    /// Returns a hash code for the current StateID instance.
    /// </summary>
    /// <returns>A hash code for the current StateID instance.</returns>
    public override int GetHashCode() => ( ID * ID ) + Name.Length;

    /// <summary>
    /// Determines whether the specified StateID is equal to the current StateID instance.
    /// </summary>
    /// <param name="other">The StateID to compare with the current instance.</param>
    /// <returns>
    /// <c>true</c> if the specified StateID is equal to the current instance;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool Equals( StateID other ) => ID == other.ID;

    /// <summary>
    /// Compares the current StateID instance with another StateID instance.
    /// </summary>
    /// <param name="other">The StateID to compare with the current instance.</param>
    /// <returns>
    /// A value less than zero if the current instance is less than the specified StateID;
    /// zero if the current instance is equal to the specified StateID;
    /// a value greater than zero if the current instance is greater than the specified StateID.
    /// </returns>
    public int CompareTo( StateID other ) => ID - other.ID;

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    public override string ToString() => Name;
}

// ============================================================================
// ============================================================================