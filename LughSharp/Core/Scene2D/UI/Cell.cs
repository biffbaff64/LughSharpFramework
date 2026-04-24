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

using LughSharp.Core.Utils.Pooling;

namespace LughSharp.Core.Scene2D.UI;

/// <summary>
/// A Cell for use with <see cref="Table"/>s.
/// </summary>
[PublicAPI]
public class Cell : IPoolable, IResetable
{
    public Value MinWidth    { get; set; } = Value.Zero;
    public Value MinHeight   { get; set; } = Value.Zero;
    public Value PrefWidth   { get; set; } = Value.Zero;
    public Value PrefHeight  { get; set; } = Value.Zero;
    public Value MaxWidth    { get; set; } = Value.Zero;
    public Value MaxHeight   { get; set; } = Value.Zero;
    public Value SpaceTop    { get; set; } = Value.Zero;
    public Value SpaceLeft   { get; set; } = Value.Zero;
    public Value SpaceBottom { get; set; } = Value.Zero;
    public Value SpaceRight  { get; set; } = Value.Zero;
    public Value PadTop      { get; set; } = Value.Zero;
    public Value PadLeft     { get; set; } = Value.Zero;
    public Value PadBottom   { get; set; } = Value.Zero;
    public Value PadRight    { get; set; } = Value.Zero;

    // ========================================================================
    
    public Align Alignment   { get; set; } = Align.None;
    public int   ExpandX     { get; set; }
    public int   ExpandY     { get; set; }
    public int   Colspan     { get; set; }
    public bool  UniformX    { get; set; }
    public bool  UniformY    { get; set; }

    // ========================================================================

    public Actor? Actor       { get; set; }
    public float  ActorX      { get; set; }
    public float  ActorY      { get; set; }
    public float  ActorWidth  { get; set; }
    public float  ActorHeight { get; set; }
    public float  FillX       { get; set; }
    public float  FillY       { get; set; }

    // ========================================================================

    public Table? Table             { get; set; }
    public bool   EndRow            { get; set; }
    public int    Column            { get; set; }
    public int    Row               { get; set; }
    public int    CellAboveIndex    { get; set; }
    public float  ComputedPadTop    { get; set; }
    public float  ComputedPadLeft   { get; set; }
    public float  ComputedPadBottom { get; set; }
    public float  ComputedPadRight  { get; set; }

    // ========================================================================

    private const float Zerof = 0f;
    private const float Onef  = 1f;
    private const int   Zeroi = 0;
    private const int   Onei  = 1;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Default constructor. Creates a new Cell with properties set to
    /// Cell defaults.
    /// </summary>
    public Cell()
    {
        CellAboveIndex = -1;

        ResetToDefaults();
    }

    // ========================================================================

    /// <summary>
    /// Reset state so the cell can be reused, setting all constraints to their default values.
    /// </summary>
    public void Reset()
    {
        Actor          = null;
        Table          = null!;
        EndRow         = false;
        CellAboveIndex = -1;

        ResetToDefaults();
    }

    /// <summary>
    /// Reset all constraints to their default values.
    /// </summary>
    private void ResetToDefaults()
    {
        MinWidth    = Value.MinWidth;
        MinHeight   = Value.MinHeight;
        PrefWidth   = Value.PrefWidth;
        PrefHeight  = Value.PrefHeight;
        MaxWidth    = Value.MaxWidth;
        MaxHeight   = Value.MaxHeight;
        SpaceTop    = Value.Zero;
        SpaceLeft   = Value.Zero;
        SpaceBottom = Value.Zero;
        SpaceRight  = Value.Zero;
        PadTop      = Value.Zero;
        PadLeft     = Value.Zero;
        PadBottom   = Value.Zero;
        PadRight    = Value.Zero;
        FillX       = Zerof;
        FillY       = Zerof;
        Alignment   = Align.Center;
        ExpandX     = Zeroi;
        ExpandY     = Zeroi;
        Colspan     = Onei;
        UniformX    = false;
        UniformY    = false;
    }

    /// <summary>
    /// Sets the actor in this cell and adds the actor to the cell's table.
    /// If null, removes any current actor.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetActor< TA >( TA? newActor ) where TA : Actor
    {
        if ( Actor != newActor )
        {
            if ( Actor?.Parent == Table )
            {
                Actor?.Remove();
            }

            Actor = newActor;

            if ( Actor != null )
            {
                Table?.AddActor( Actor );
            }
        }

        return this;
    }

    /// <summary>
    /// Removes the current actor for the cell, if any.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell ClearActor()
    {
        SetActor< Actor >( null );

        return this;
    }

    /// <summary>
    /// Returns <b>true</b> if this Cells <see cref="Actor"/> is not null.
    /// </summary>
    public bool HasActor()
    {
        return Actor != null;
    }

    /// <summary>
    /// Sets the MinWidth, PrefWidth, MaxWidth, MinHeight, PrefHeight, and MaxHeight to
    /// the specified value.
    /// </summary>
    /// <param name="size">The <see cref="Value"/> to use.</param>
    /// <returns> This Cell for chaining. </returns>
    /// <exception cref="ArgumentNullException">If parameter <tt>size</tt> is null.</exception>
    public Cell Size( Value size )
    {
        MinWidth   = size;
        MinHeight  = size;
        PrefWidth  = size;
        PrefHeight = size;
        MaxWidth   = size;
        MaxHeight  = size;

        return this;
    }

    /// <summary>
    /// Sets the MinWidth, PrefWidth, MaxWidth, MinHeight, PrefHeight, and MaxHeight to
    /// the specified values.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell Size( Value width, Value height )
    {
        MinWidth   = width;
        MinHeight  = height;
        PrefWidth  = width;
        PrefHeight = height;
        MaxWidth   = width;
        MaxHeight  = height;

        return this;
    }

    /// <summary>
    /// Sets the MinWidth, PrefWidth, MaxWidth, MinHeight, PrefHeight, and MaxHeight to
    /// the specified value.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell Size( float size )
    {
        Size( Value.Fixed.ValueOf( size ) );

        return this;
    }

    /// <summary>
    /// Sets the MinWidth, PrefWidth, MaxWidth, MinHeight, PrefHeight,
    /// and MaxHeight to the specified values.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell Size( float width, float height )
    {
        Size( Value.Fixed.ValueOf( width ), Value.Fixed.ValueOf( height ) );

        return this;
    }

    /// <summary>
    /// Sets the minWidth, prefWidth, and maxWidth to the specified value.
    /// </summary>
    /// <param name="width"></param>
    /// <returns> This Cell for chaining. </returns>
    public Cell Width( Value width )
    {
        MinWidth  = width;
        PrefWidth = width;
        MaxWidth  = width;

        return this;
    }

    /// <summary>
    /// Sets the minWidth, prefWidth, and maxWidth to the specified value.
    /// </summary>
    /// <param name="width"> The width to set. </param>
    /// <returns> This Cell for chaining. </returns>
    public Cell Width( float width )
    {
        Width( Value.Fixed.ValueOf( width ) );

        return this;
    }

    /// <summary>
    /// Sets the minHeight, prefHeight, and maxHeight to the specified value.
    /// </summary>
    /// <param name="height"></param>
    /// <returns> This Cell for chaining. </returns>
    public Cell Height( Value height )
    {
        MinHeight  = height;
        PrefHeight = height;
        MaxHeight  = height;

        return this;
    }

    /// <summary>
    /// Sets the minHeight, prefHeight, and maxHeight to the specified value.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell Height( float height )
    {
        Height( Value.Fixed.ValueOf( height ) );

        return this;
    }

    /// <summary>
    /// Sets the minWidth and minHeight to the specified value.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell MinSize( Value size )
    {
        MinWidth  = size;
        MinHeight = size;

        return this;
    }

    /// <summary>
    /// Sets the minWidth and minHeight to the specified value.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell MinSize( float size )
    {
        MinSize( Value.Fixed.ValueOf( size ) );

        return this;
    }

    /// <summary>
    /// Sets the minWidth and minHeight to the specified values.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell MinSize( Value width, Value height )
    {
        MinWidth  = width;
        MinHeight = height;

        return this;
    }

    /// <summary>
    /// Sets the minWidth and minHeight to the specified values.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell MinSize( float width, float height )
    {
        MinSize( Value.Fixed.ValueOf( width ), Value.Fixed.ValueOf( height ) );

        return this;
    }

    /// <summary>
    /// Convenience method which sets the <see cref="MinWidth"/> property and then
    /// returns this Cell for chaining.
    /// </summary>
    /// <param name="minWidth"> The new value for MinWidth, passed as a <see cref="Value"/> </param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetMinWidth( Value minWidth )
    {
        MinWidth = minWidth;

        return this;
    }

    /// <summary>
    /// Convenience method which sets the <see cref="MinHeight"/> property and then returns
    /// this Cell for chaining.
    /// </summary>
    /// <param name="minHeight"> The new value for MinHeight, passed as a <see cref="Value"/> </param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetMinHeight( Value minHeight )
    {
        MinHeight = minHeight;

        return this;
    }

    /// <summary>
    /// Convenience method which sets the <see cref="MinWidth"/> property and then returns
    /// this Cell for chaining.
    /// </summary>
    /// <param name="minWidth"> The new value for MinWidth, passed as a float. </param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetMinWidth( float minWidth )
    {
        MinWidth = Value.Fixed.ValueOf( minWidth );

        return this;
    }

    /// <summary>
    /// Convenience method which sets the <see cref="MinHeight"/> property and then returns
    /// this Cell for chaining.
    /// </summary>
    /// <param name="minHeight"> The new value for MinHeight, passed as a float </param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetMinHeight( float minHeight )
    {
        MinHeight = Value.Fixed.ValueOf( minHeight );

        return this;
    }

    /// <summary>
    /// Sets the prefWidth and prefHeight to the specified value.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    /// <remarks>
    /// This method is provided for chaining purposes, which cannot be acheived by accessing the
    /// <see cref="PrefWidth"/> and <see cref="PrefHeight"/> properties directly.
    /// </remarks>
    public Cell SetPrefSize( Value size )
    {
        PrefWidth  = size;
        PrefHeight = size;

        return this;
    }

    /// <summary>
    /// Sets the prefWidth and prefHeight to the specified value.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    /// <remarks>
    /// This method is provided for chaining purposes, which cannot be acheived by accessing the
    /// <see cref="PrefWidth"/> and <see cref="PrefHeight"/> properties directly.
    /// </remarks>
    public Cell SetPrefSize( Value width, Value height )
    {
        PrefWidth  = width;
        PrefHeight = height;

        return this;
    }

    /// <summary>
    /// Sets the <see cref="PrefWidth"/> for this Cell.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    /// <remarks>
    /// This method is provided for chaining purposes, which cannot be acheived by accessing the
    /// <see cref="PrefWidth"/> property directly.
    /// </remarks>
    public Cell SetPrefWidth( Value prefWidth )
    {
        PrefWidth = prefWidth;

        return this;
    }

    /// <summary>
    /// Sets the <see cref="PrefWidth"/> for this Cell.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    /// <remarks>
    /// This method is provided for chaining purposes, which cannot be acheived by accessing the
    /// <see cref="PrefWidth"/> property directly.
    /// </remarks>
    public Cell SetPrefWidth( float prefWidth )
    {
        PrefWidth = Value.Fixed.ValueOf( prefWidth );

        return this;
    }

    /// <summary>
    /// Sets the <see cref="PrefHeight"/> for this Cell.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    /// <remarks>
    /// This method is provided for chaining purposes, which cannot be acheived by accessing the
    /// <see cref="PrefHeight"/> property directly.
    /// </remarks>
    public Cell SetPrefHeight( Value prefHeight )
    {
        PrefHeight = prefHeight;

        return this;
    }

    /// <summary>
    /// Sets the <see cref="PrefHeight"/> for this Cell.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    /// <remarks>
    /// This method is provided for chaining purposes, which cannot be acheived by accessing the
    /// <see cref="PrefHeight"/> property directly.
    /// </remarks>
    public Cell SetPrefHeight( float prefHeight )
    {
        PrefHeight = Value.Fixed.ValueOf( prefHeight );

        return this;
    }

    /// <summary>
    /// Sets the prefWidth and prefHeight to the specified value.
    /// </summary>
    /// <param name="width"> The new width. </param>
    /// <param name="height"> The new height. </param>
    /// <returns> This Cell for chaining. </returns>
    /// <remarks>
    /// This method is provided for chaining purposes, which cannot be acheived by accessing the
    /// <see cref="PrefWidth"/> and <see cref="PrefHeight"/> properties directly.
    /// </remarks>
    public Cell SetPrefSize( float width, float height )
    {
        SetPrefSize( Value.Fixed.ValueOf( width ), Value.Fixed.ValueOf( height ) );

        return this;
    }

    /// <summary>
    /// Sets the prefWidth and prefHeight to the specified values.
    /// </summary>
    /// <param name="size"> The new size, which applies to both width and height. </param>
    /// <returns> This Cell for chaining. </returns>
    /// <remarks>
    /// This method is provided for chaining purposes, which cannot be acheived by accessing the
    /// <see cref="PrefWidth"/> and <see cref="PrefHeight"/> properties directly.
    /// </remarks>
    public Cell SetPrefSize( float size )
    {
        SetPrefSize( Value.Fixed.ValueOf( size ) );

        return this;
    }

    /// <summary>
    /// Sets the maxWidth and maxHeight to the specified value.
    /// If the max size is 0, no maximum size is used.
    /// </summary>
    /// <param name="size"></param>
    /// <returns> This Cell for chaining. </returns>
    /// <exception cref="ArgumentException"></exception>
    public Cell SetMaxSize( Value size )
    {
        MaxWidth  = size;
        MaxHeight = size;

        return this;
    }

    /// <summary>
    /// Sets the maxWidth and maxHeight to the specified values.
    /// If the max size is 0, no maximum size is used.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns> This Cell for chaining. </returns>
    /// <exception cref="ArgumentException"></exception>
    public Cell SetMaxSize( Value width, Value height )
    {
        MaxWidth  = width;
        MaxHeight = height;

        return this;
    }

    /// <summary>
    /// If the maxWidth is 0, no maximum width is used.
    /// </summary>
    /// <param name="maxWidth"></param>
    /// <returns> This Cell for chaining. </returns>
    /// <exception cref="ArgumentException"></exception>
    public Cell SetMaxWidth( Value maxWidth )
    {
        MaxWidth = maxWidth;

        return this;
    }

    /// <summary>
    /// If the maxHeight is 0, no maximum height is used.
    /// </summary>
    /// <param name="maxHeight"></param>
    /// <returns> This Cell for chaining. </returns>
    /// <exception cref="ArgumentException"></exception>
    public Cell SetMaxHeight( Value maxHeight )
    {
        MaxHeight = maxHeight;

        return this;
    }

    /// <summary>
    /// Sets the maxWidth and maxHeight to the specified value.
    /// If the max size is 0, no maximum size is used.
    /// </summary>
    /// <param name="size"></param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetMaxSize( float size )
    {
        SetMaxSize( Value.Fixed.ValueOf( size ) );

        return this;
    }

    /// <summary>
    /// Sets the maxWidth and maxHeight to the specified values.
    /// If the max size is 0, no maximum size is used.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetMaxSize( float width, float height )
    {
        SetMaxSize( Value.Fixed.ValueOf( width ), Value.Fixed.ValueOf( height ) );

        return this;
    }

    /// <summary>
    /// If the maxWidth is 0, no maximum width is used.
    /// </summary>
    /// <param name="maxWidth"></param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetMaxWidth( float maxWidth )
    {
        MaxWidth = Value.Fixed.ValueOf( maxWidth );

        return this;
    }

    /// <summary>
    /// If the maxHeight is 0, no maximum height is used.
    /// </summary>
    /// <param name="maxHeight"></param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetMaxHeight( float maxHeight )
    {
        MaxHeight = Value.Fixed.ValueOf( maxHeight );

        return this;
    }

    /// <summary>
    /// Sets the spaceTop, spaceLeft, spaceBottom, and spaceRight to the specified value.
    /// </summary>
    /// <param name="space"></param>
    /// <returns> This Cell for chaining. </returns>
    /// <exception cref="ArgumentException"></exception>
    public Cell Space( Value space )
    {
        SpaceTop    = space;
        SpaceLeft   = space;
        SpaceBottom = space;
        SpaceRight  = space;

        return this;
    }

    /// <summary>
    /// Sets the space values for the top, left, bottom, and right sides of this Cell.
    /// </summary>
    /// <param name="top">The space value to be applied to the top side.</param>
    /// <param name="left">The space value to be applied to the left side.</param>
    /// <param name="bottom">The space value to be applied to the bottom side.</param>
    /// <param name="right">The space value to be applied to the right side.</param>
    /// <returns>This Cell for chaining.</returns>
    public Cell Space( Value top, Value left, Value bottom, Value right )
    {
        SpaceTop    = top;
        SpaceLeft   = left;
        SpaceBottom = bottom;
        SpaceRight  = right;

        return this;
    }

    /// <summary>
    /// Sets the space value for the top side of this Cell.
    /// </summary>
    /// <param name="spaceTop"></param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetSpaceTop( Value spaceTop )
    {
        SpaceTop = spaceTop;

        return this;
    }

    /// <summary>
    /// Sets the space value for the left side of this Cell.
    /// </summary>
    /// <param name="spaceLeft"></param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetSpaceLeft( Value spaceLeft )
    {
        SpaceLeft = spaceLeft;

        return this;
    }

    /// <summary>
    /// Sets the space value for the bottom side of this Cell.
    /// </summary>
    /// <param name="spaceBottom"></param>
    /// <returns></returns>
    public Cell SetSpaceBottom( Value spaceBottom )
    {
        SpaceBottom = spaceBottom;

        return this;
    }

    /// <summary>
    /// Sets the space value for the right side of this Cell.
    /// </summary>
    /// <param name="spaceRight"></param>
    /// <returns></returns>
    public Cell SetSpaceRight( Value spaceRight )
    {
        SpaceRight = spaceRight;

        return this;
    }

    public Cell Space( float space )
    {
        if ( space < 0 )
        {
            throw new ArgumentException( $"space cannot be < 0: {space}" );
        }

        Space( Value.Fixed.ValueOf( space ) );

        return this;
    }

    public Cell Space( float top, float left, float bottom, float right )
    {
        if ( top < 0 )
        {
            throw new ArgumentException( $"top cannot be < 0: {top}" );
        }

        if ( left < 0 )
        {
            throw new ArgumentException( $"left cannot be < 0: {left}" );
        }

        if ( bottom < 0 )
        {
            throw new ArgumentException( $"bottom cannot be < 0: {bottom}" );
        }

        if ( right < 0 )
        {
            throw new ArgumentException( $"right cannot be < 0: {right}" );
        }

        Space( Value.Fixed.ValueOf( top ),
               Value.Fixed.ValueOf( left ),
               Value.Fixed.ValueOf( bottom ),
               Value.Fixed.ValueOf( right ) );

        return this;
    }

    public Cell SetSpaceTop( float spaceTop )
    {
        if ( spaceTop < 0 )
        {
            throw new ArgumentException( $"spaceTop cannot be < 0: {spaceTop}" );
        }

        SpaceTop = Value.Fixed.ValueOf( spaceTop );

        return this;
    }

    public Cell SetSpaceLeft( float spaceLeft )
    {
        if ( spaceLeft < 0 )
        {
            throw new ArgumentException( $"spaceLeft cannot be < 0: {spaceLeft}" );
        }

        SpaceLeft = Value.Fixed.ValueOf( spaceLeft );

        return this;
    }

    public Cell SetSpaceBottom( float spaceBottom )
    {
        if ( spaceBottom < 0 )
        {
            throw new ArgumentException( $"spaceBottom cannot be < 0: {spaceBottom}" );
        }

        SpaceBottom = Value.Fixed.ValueOf( spaceBottom );

        return this;
    }

    public Cell SetSpaceRight( float spaceRight )
    {
        if ( spaceRight < 0 )
        {
            throw new ArgumentException( $"spaceRight cannot be < 0: {spaceRight}" );
        }

        SpaceRight = Value.Fixed.ValueOf( spaceRight );

        return this;
    }

    /// <summary>
    /// Sets the padTop, padLeft, padBottom, and padRight to the specified value.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell Pad( Value pad )
    {
        PadTop    = pad;
        PadLeft   = pad;
        PadBottom = pad;
        PadRight  = pad;

        return this;
    }

    /// <summary>
    /// Sets the pad values for the top, left, bottom, and right sides of this Cell.
    /// </summary>
    /// <param name="top"> The top padding value. </param>
    /// <param name="left"> The left padding value. </param>
    /// <param name="bottom"> The bottom padding value. </param>
    /// <param name="right"> The right padding value. </param>
    /// <returns> This Cell for chaining. </returns>
    public Cell Pad( Value top, Value left, Value bottom, Value right )
    {
        PadTop    = top;
        PadLeft   = left;
        PadBottom = bottom;
        PadRight  = right;

        return this;
    }

    public Cell SetPadTop( Value padTop )
    {
        PadTop = padTop;

        return this;
    }

    public Cell SetPadLeft( Value padLeft )
    {
        PadLeft = padLeft;

        return this;
    }

    public Cell SetPadBottom( Value padBottom )
    {
        PadBottom = padBottom;

        return this;
    }

    public Cell SetPadRight( Value padRight )
    {
        PadRight = padRight;

        return this;
    }

    public Cell Pad( float pad )
    {
        Pad( Value.Fixed.ValueOf( pad ) );

        return this;
    }

    public Cell Pad( float top, float left, float bottom, float right )
    {
        Pad( Value.Fixed.ValueOf( top ),
             Value.Fixed.ValueOf( left ),
             Value.Fixed.ValueOf( bottom ),
             Value.Fixed.ValueOf( right ) );

        return this;
    }

    public Cell SetPadTop( float padTop )
    {
        PadTop = Value.Fixed.ValueOf( padTop );

        return this;
    }

    public Cell SetPadLeft( float padLeft )
    {
        PadLeft = Value.Fixed.ValueOf( padLeft );

        return this;
    }

    public Cell SetPadBottom( float padBottom )
    {
        PadBottom = Value.Fixed.ValueOf( padBottom );

        return this;
    }

    public Cell SetPadRight( float padRight )
    {
        PadRight = Value.Fixed.ValueOf( padRight );

        return this;
    }

    /// <summary>
    /// Sets <see cref="FillX"/> and <see cref="FillY"/> to <see cref="Onef"/>
    /// </summary>
    /// <returns> This Cell for chaining </returns>
    public Cell Fill( float fx = Onef, float fy = Onef )
    {
        FillX = fx;
        FillY = fy;

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Cell Fill( bool x, bool y )
    {
        FillX = x ? Onef : Zerof;
        FillY = y ? Onef : Zerof;

        return this;
    }

    /// <summary>
    /// Sets <see cref="FillX"/> to <see cref="Onef"/>. Leaves <see cref="FillY"/> unchanged.
    /// </summary>
    /// <returns> This Cell for chaining </returns>
    public Cell SetFillX( float fx = Onef )
    {
        FillX = fx;

        return this;
    }

    /// <summary>
    /// Sets <see cref="FillY"/> to <see cref="Onef"/>. Leaves <see cref="FillX"/> unchanged.
    /// </summary>
    /// <returns> This Cell for chaining </returns>
    public Cell SetFillY( float fy = Onef )
    {
        FillY = fy;

        return this;
    }

    /// <summary>
    /// Sets the alignment of the actor within the cell. Set to <see cref="Align.Center"/>,
    /// <see cref="Align.Top"/>, <see cref="Align.Bottom"/>,
    /// <see cref="Align.Left"/>,
    /// <see cref="Align.Right"/>, or any combination of those.
    /// </summary>
    public Cell SetAlignment( Align align )
    {
        Alignment = align;

        return this;
    }

    /// <summary>
    /// Sets the alignment of the actor within the cell to <see cref="Align.Center"/>.
    /// This clears any other alignment.
    /// </summary>
    public Cell Center()
    {
        Alignment = Align.Center;

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Top"/> and clears <see cref="Align.Bottom"/> for
    /// the alignment of the actor within the cell.
    /// </summary>
    public Cell Top()
    {
        if ( Alignment == Align.None )
        {
            Alignment = Align.Top;
        }
        else
        {
            Alignment = ( Alignment | Align.Top ) & ~Align.Bottom;
        }

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Left"/> and clears <see cref="Align.Right"/> for
    /// the alignment of the actor within the cell.
    /// </summary>
    public Cell Left()
    {
        if ( Alignment == Align.None )
        {
            Alignment = Align.Left;
        }
        else
        {
            Alignment = ( Alignment | Align.Left ) & ~Align.Right;
        }

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Bottom"/> and clears <see cref="Align.Top"/> for
    /// the alignment of the actor within the cell.
    /// </summary>
    public Cell Bottom()
    {
        if ( Alignment == Align.None )
        {
            Alignment = Align.Bottom;
        }
        else
        {
            Alignment = ( Alignment | Align.Bottom ) & ~Align.Top;
        }

        return this;
    }

    /// <summary>
    /// Adds <see cref="Align.Right"/> and clears <see cref="Align.Left"/> for
    /// the alignment of the actor within the cell.
    /// </summary>
    public Cell Right()
    {
        if ( Alignment == Align.None )
        {
            Alignment = Align.Right;
        }
        else
        {
            Alignment = ( Alignment | Align.Right ) & ~Align.Left;
        }

        return this;
    }

    /// <summary>
    /// Grow this Cell in X and Y. Using this method is the equivalent of calling:-
    /// <code>
    /// Expand().Fill();
    /// </code>
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell Grow()
    {
        ExpandX = Onei;
        ExpandY = Onei;
        FillX   = Onef;
        FillY   = Onef;

        return this;
    }

    /// <summary>
    /// Grow this Cell in X.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell GrowX()
    {
        ExpandX = Onei;
        FillX   = Onef;

        return this;
    }

    /// <summary>
    /// Grow this Cell in Y.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell GrowY()
    {
        ExpandY = Onei;
        FillY   = Onef;

        return this;
    }

    /// <summary>
    /// Expand this Cell in X.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetExpandX()
    {
        ExpandX = Onei;

        return this;
    }

    /// <summary>
    /// Expand this Cell in Y.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetExpandY()
    {
        ExpandY = Onei;

        return this;
    }

    /// <summary>
    /// Expand this Cell in X and Y.
    /// </summary>
    /// <param name="x"> The value to expand in X. Default is <see cref="Onei"/>. </param>
    /// <param name="y"> The value to expand in Y. Default is <see cref="Onei"/>. </param>
    /// <returns> This Cell for chaining. </returns>
    public Cell Expand( int x = Onei, int y = Onei )
    {
        ExpandX = x;
        ExpandY = y;

        return this;
    }

    /// <summary>
    /// Expand in X if X condition is <b>true</b>.
    /// Expand in Y if Y condition is <b>true</b>.
    /// </summary>
    /// <param name="expandX"> True to expand in X. </param>
    /// <param name="expandY"> True to expand in Y. </param>
    /// <returns> This Cell for chaining. </returns>
    public Cell Expand( bool expandX, bool expandY )
    {
        ExpandX = expandX ? Onei : Zeroi;
        ExpandY = expandY ? Onei : Zeroi;

        return this;
    }

    /// <summary>
    /// Sets the number of columns this cell spans.
    /// </summary>
    /// <param name="colspan"> The number of columns to span. </param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetColspan( int colspan )
    {
        Colspan = colspan;

        return this;
    }

    public Cell SetUniform( bool ux = true, bool uy = true )
    {
        UniformX = ux;
        UniformY = uy;

        return this;
    }

    public Cell SetUniformX( bool ux = true )
    {
        UniformX = ux;

        return this;
    }

    public Cell SetUniformY( bool uy = true )
    {
        UniformY = uy;

        return this;
    }

    /// <summary>
    /// Sets the bounds for this Cells <see cref="Actor"/>.
    /// </summary>
    public void SetActorBounds( float x, float y, float width, float height )
    {
        ActorX      = x;
        ActorY      = y;
        ActorWidth  = width;
        ActorHeight = height;
    }

    // ========================================================================

    /// <summary>
    /// Gets the preferred width of the actor.
    /// </summary>
    public virtual float GetPrefWidth() => PrefWidth.Get( Actor );

    /// <summary>
    /// Gets the preferred height of the actor.
    /// </summary>
    public virtual float GetPrefHeight() => PrefHeight.Get( Actor );

    /// <summary>
    /// Gets the minimum width of the actor.
    /// </summary>
    public float GetMinWidth() => MinWidth.Get( Actor );

    /// <summary>
    /// Gets the minimum height of the actor.
    /// </summary>
    public float GetMinHeight() => MinHeight.Get( Actor );

    /// <summary>
    /// Gets the maximum width of the actor.
    /// </summary>
    public float GetMaxWidth() => MaxWidth.Get( Actor );

    /// <summary>
    /// Gets the maximum height of the actor. 
    /// </summary>
    public float GetMaxHeight() => MaxHeight.Get( Actor );

    /// <summary>
    /// Gets the space between the top of the cell and the top of the actor.
    /// </summary>
    public float GetSpaceTop() => SpaceTop.Get( Actor );

    /// <summary>
    /// Gets the space between the left of the cell and the left of the actor.
    /// </summary>
    public float GetSpaceLeft() => SpaceLeft.Get( Actor );

    /// <summary>
    /// Gets the space between the bottom of the cell and the bottom of the actor.
    /// </summary>
    public float GetSpaceBottom() => SpaceBottom.Get( Actor );

    /// <summary>
    /// Gets the space between the right of the cell and the right of the actor.
    /// </summary>
    public float GetSpaceRight() => SpaceRight.Get( Actor );

    /// <summary>
    /// Gets the pad between the top of the cell and the top of the actor.
    /// </summary>
    public float GetPadTop() => PadTop.Get( Actor );

    /// <summary>
    /// Gets the pad between the left of the cell and the left of the actor.
    /// </summary>
    public float GetPadLeft() => PadLeft.Get( Actor );

    /// <summary>
    /// Gets the pad between the bottom of the cell and the bottom of the actor
    /// </summary>
    public float GetPadBottom() => PadBottom.Get( Actor );

    /// <summary>
    /// Gets the pad between the right of the cell and the right of the actor.
    /// </summary>
    public float GetPadRight() => PadRight.Get( Actor );

    // ========================================================================

    /// <summary>
    /// Gets the X-padding for this Cell, by adding together <see cref="PadLeft"/>
    /// and <see cref="PadRight"/>.
    /// </summary>
    public float GetPadX() => PadLeft.Get( Actor ) + PadRight.Get( Actor );

    /// <summary>
    /// Gets the Y-padding for this Cell, by adding together <see cref="PadTop"/>
    /// and <see cref="PadBottom"/>.
    /// </summary>
    public float GetPadY() => PadTop.Get( Actor ) + PadBottom.Get( Actor );

    /// <summary>
    /// Resets constraint fields.
    /// </summary>
    public void Clear()
    {
        MinWidth    = Value.Zero;
        MinHeight   = Value.Zero;
        PrefWidth   = Value.Zero;
        PrefHeight  = Value.Zero;
        MaxWidth    = Value.Zero;
        MaxHeight   = Value.Zero;
        SpaceTop    = Value.Zero;
        SpaceLeft   = Value.Zero;
        SpaceBottom = Value.Zero;
        SpaceRight  = Value.Zero;
        PadTop      = Value.Zero;
        PadLeft     = Value.Zero;
        PadBottom   = Value.Zero;
        PadRight    = Value.Zero;
        FillX       = 0f;
        FillY       = 0f;
        Alignment   = Align.None;
        ExpandX     = 0;
        ExpandY     = 0;
        Colspan     = 0;
        UniformX    = false;
        UniformY    = false;
    }

    /// <summary>
    /// Sets the properties of this Cell to the properties from the given
    /// Cell, if the other Cell is not null.
    /// </summary>
    /// <param name="cell"> The other Cell. </param>
    public void Set( Cell? cell )
    {
        if ( cell == null )
        {
            return;
        }

        CopyConstraintsFrom( cell );
        
        Alignment = cell.Alignment;
        ExpandX   = cell.ExpandX;
        ExpandY   = cell.ExpandY;
        Colspan   = cell.Colspan;
        UniformX  = cell.UniformX;
        UniformY  = cell.UniformY;
        FillX     = cell.FillX;
        FillY     = cell.FillY;
    }

    /// <summary>
    /// Merge this Cell with the given Cell.
    /// </summary>
    /// <param name="cell"> The Cell to merge with. </param>
    public void Merge( Cell? cell )
    {
        if ( cell == null )
        {
            return;
        }

        CopyConstraintsFrom( cell );

        //@formatter:off
        if ( cell.Alignment != 0 ) Alignment = cell.Alignment;
        if ( cell.ExpandX != 0 )   ExpandX   = cell.ExpandX;
        if ( cell.ExpandY != 0 )   ExpandY   = cell.ExpandY;
        if ( cell.Colspan != 0 )   Colspan   = cell.Colspan;
        if ( cell.UniformX )       UniformX  = cell.UniformX;
        if ( cell.UniformY )       UniformY  = cell.UniformY;
        //@formatter:on
        
        FillX = cell.FillX;
        FillY = cell.FillY;
    }

    /// <summary>
    /// Copies the constraints from the given Cell to this Cell.
    /// </summary>
    private void CopyConstraintsFrom( Cell cell )
    {
        MinWidth    = cell.MinWidth;
        MinHeight   = cell.MinHeight;
        PrefWidth   = cell.PrefWidth;
        PrefHeight  = cell.PrefHeight;
        MaxWidth    = cell.MaxWidth;
        MaxHeight   = cell.MaxHeight;
        SpaceTop    = cell.SpaceTop;
        SpaceLeft   = cell.SpaceLeft;
        SpaceBottom = cell.SpaceBottom;
        SpaceRight  = cell.SpaceRight;
        PadTop      = cell.PadTop;
        PadLeft     = cell.PadLeft;
        PadBottom   = cell.PadBottom;
        PadRight    = cell.PadRight;
    }

    /// <inheritdoc />
    public override string? ToString()
    {
        return Actor != null ? Actor.ToString() : base.ToString();
    }
}

// ============================================================================
// ============================================================================