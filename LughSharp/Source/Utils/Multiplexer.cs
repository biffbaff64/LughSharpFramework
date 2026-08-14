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

using System.Runtime.InteropServices.JavaScript;

namespace LughSharp.Source.Utils;

/// <summary>
/// Base class for multiplexers that forward e.g. events to an Array of receivers
/// <typeparam name="T"> The type of the receivers. </typeparam>
/// </summary>
/// <remarks>
/// Based on Multiplexer from libGDX, written by dermetfan.
/// </remarks>
[PublicAPI]
public abstract class Multiplexer< T >
{
    public List< T > Receivers { get; set; }
    
    // ========================================================================

    /// <summary>
    /// Creates a new multiplexer with an empty receivers list.
    /// </summary>
    protected Multiplexer()
    {
        Receivers = new List< T >();
    }

    /// <summary>
    /// Creates a new multiplexer with the specified initial capacity.
    /// </summary>
    /// <param name="size"> The initial capacity of the multiplexer. </param>
    protected Multiplexer( int size )
    {
        Receivers = new List< T >( size );
    }

    /// <summary>
    /// Creates a new multiplexer with the specified receivers.
    /// </summary>
    /// <param name="receivers"> The initial receivers of the multiplexer. </param>
    protected Multiplexer( params T[] receivers )
    {
        this.Receivers = new List<T>( receivers );
    }

    /// <summary>
    /// Creates a new multiplexer with the specified receivers.
    /// </summary>
    /// <param name="receivers"> The initial receivers of the multiplexer. </param>
    protected Multiplexer( List< T > receivers )
    {
        this.Receivers = new List<T>( receivers );
    }

    /// <summary>
    /// Adds a receiver to the multiplexer.
    /// </summary>
    /// <param name="receiver"> The receiver to add. </param>
    public void Add( T receiver )
    {
        Receivers.Add( receiver );
    }

    /// <summary>
    /// Removes a receiver from the multiplexer.
    /// </summary>
    /// <param name="receiver"> The receiver to remove. </param>
    /// <returns> True if the receiver was removed, false otherwise. </returns>
    public bool Remove( T receiver )
    {
        return Receivers.Remove( receiver );
    }

    /// <summary>
    /// Clears the receivers of the multiplexer.
    /// </summary>
    public void Clear()
    {
        Receivers.Clear();
    }

    /// <summary>
    /// Sets the receivers of the multiplexer.
    /// </summary>
    /// <param name="receivers"> The new receivers of the multiplexer. </param>
    public void SetReceivers( List< T > receivers )
    {
        this.Receivers.Clear();
        this.Receivers.AddRange( receivers );
    }

    /// <summary>
    /// Sets the receivers of the multiplexer.
    /// </summary>
    /// <param name="receivers"> The new receivers of the multiplexer. </param>
    public void SetReceivers( params T[] receivers )
    {
        this.Receivers.Clear();
        this.Receivers.AddRange( receivers );
    }
}

// ============================================================================
// ============================================================================
