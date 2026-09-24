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

using LughSharp.Source.IO;
using LughSharp.Source.Utils.Pooling;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// A Cell for use with a <see cref="Table"/>s.
/// </summary>
[PublicAPI]
public class Cell : IPoolable, IResetable
{
    public Value? MinWidth    { get; set; }
    public Value? MinHeight   { get; set; }
    public Value? PrefWidth   { get; set; }
    public Value? PrefHeight  { get; set; }
    public Value? MaxWidth    { get; set; }
    public Value? MaxHeight   { get; set; }
    public Value? SpaceTop    { get; set; }
    public Value? SpaceLeft   { get; set; }
    public Value? SpaceBottom { get; set; }
    public Value? SpaceRight  { get; set; }
    public Value? PadTop      { get; set; }
    public Value? PadLeft     { get; set; }
    public Value? PadBottom   { get; set; }
    public Value? PadRight    { get; set; }
    public float  FillX       { get; set; }
    public float  FillY       { get; set; }
    public Align? Alignment   { get; set; }
    public int    ExpandX     { get; set; }
    public int    ExpandY     { get; set; }
    public int    Colspan     { get; set; }
    public bool   UniformX    { get; set; }
    public bool   UniformY    { get; set; }

    // ========================================================================

    public float ActorX      { get; set; }
    public float ActorY      { get; set; }
    public float ActorWidth  { get; set; }
    public float ActorHeight { get; set; }

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

    private const float DefaultFill    = 0.0f;
    private const float DefaultFill1F  = 1.0f;
    private const int   NoCellAbove    = -1;
    private const int   DefaultExpand  = 0;
    private const int   DefaultColspan = 1;

    private static Cell?   _defaultCell;
    private static IFiles? _files;

    private Actor? _actor;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Default constructor. Creates a new Cell with properties set to
    /// Cell defaults.
    /// </summary>
    public Cell()
    {
        CellAboveIndex = NoCellAbove;

        Cell? defaults = GetCellDefaults();

        if ( defaults != null )
        {
            Set( defaults );
        }
    }

    /// <summary>
    /// Gets the actor in this cell.
    /// </summary>
    public Actor? GetActor()
    {
        return _actor;
    }

    /// <summary>
    /// Sets the actor in this cell and adds the actor to the cell's table.
    /// If null, removes any current actor.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetActor< TA >( TA? newActor ) where TA : Actor
    {
        if ( _actor != newActor )
        {
            if ( _actor?.Parent == Table )
            {
                _actor?.Remove();
            }

            _actor = newActor;

            if ( _actor != null )
            {
                Table?.AddActor( _actor );
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
    /// Returns <b>true</b> if this Cells <see cref="_actor"/> is not null.
    /// </summary>
    public bool HasActor() => _actor != null;

    // ------------------------------------------------------------------------

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

    // ------------------------------------------------------------------------

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

    // ------------------------------------------------------------------------

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

    // ------------------------------------------------------------------------

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

    // ------------------------------------------------------------------------

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

    // ------------------------------------------------------------------------

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

    // ------------------------------------------------------------------------

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

    // ------------------------------------------------------------------------

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

    // ------------------------------------------------------------------------

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

    // ------------------------------------------------------------------------

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

    // ------------------------------------------------------------------------

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
    /// If the maxWidth is 0, no maximum width is used.
    /// </summary>
    /// <param name="maxWidth"></param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetMaxWidth( float maxWidth )
    {
        MaxWidth = Value.Fixed.ValueOf( maxWidth );

        return this;
    }

    // ------------------------------------------------------------------------

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
    /// If the maxHeight is 0, no maximum height is used.
    /// </summary>
    /// <param name="maxHeight"></param>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetMaxHeight( float maxHeight )
    {
        MaxHeight = Value.Fixed.ValueOf( maxHeight );

        return this;
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Sets the space for all sides of the cell to the specified value.
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
    /// Sets the space for all sides of the cell to the specified value.
    /// </summary>
    /// <param name="space">The space value to set. Must be greater than or equal to 0.</param>
    /// <returns> This Cell for chaining. </returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="space"/> is less than 0.</exception>
    public Cell Space( float space )
    {
        if ( space < 0 )
        {
            throw new ArgumentException( $"space cannot be < 0: {space}" );
        }

        Space( Value.Fixed.ValueOf( space ) );

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
    /// Sets the spacing values around the cell using fixed pixel values.
    /// </summary>
    /// <param name="top">Amount of space to set at the top. Must be non-negative.</param>
    /// <param name="left">Amount of space to set on the left. Must be non-negative.</param>
    /// <param name="bottom">Amount of space to set at the bottom. Must be non-negative.</param>
    /// <param name="right">Amount of space to set on the right. Must be non-negative.</param>
    /// <returns>The current Cell instance with updated spacing values.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if any of the input values (top, left, bottom, or right) are negative.
    /// </exception>
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

        Space
            (
             Value.Fixed.ValueOf( top ),
             Value.Fixed.ValueOf( left ),
             Value.Fixed.ValueOf( bottom ),
             Value.Fixed.ValueOf( right )
            );

        return this;
    }

    // ------------------------------------------------------------------------

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
    /// Sets the space value for the top side of this Cell.
    /// </summary>
    /// <param name="spaceTop"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Cell SetSpaceTop( float spaceTop )
    {
        if ( spaceTop < 0 )
        {
            throw new ArgumentException( $"spaceTop cannot be < 0: {spaceTop}" );
        }

        SpaceTop = Value.Fixed.ValueOf( spaceTop );

        return this;
    }

    // ------------------------------------------------------------------------

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
    /// Sets the space value for the left side of this Cell.
    /// </summary>
    /// <param name="spaceLeft"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Cell SetSpaceLeft( float spaceLeft )
    {
        if ( spaceLeft < 0 )
        {
            throw new ArgumentException( $"spaceLeft cannot be < 0: {spaceLeft}" );
        }

        SpaceLeft = Value.Fixed.ValueOf( spaceLeft );

        return this;
    }

    // ------------------------------------------------------------------------

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
    /// Sets the space value for the bottom side of this Cell.
    /// </summary>
    /// <param name="spaceBottom"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Cell SetSpaceBottom( float spaceBottom )
    {
        if ( spaceBottom < 0 )
        {
            throw new ArgumentException( $"spaceBottom cannot be < 0: {spaceBottom}" );
        }

        SpaceBottom = Value.Fixed.ValueOf( spaceBottom );

        return this;
    }

    // ------------------------------------------------------------------------

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

    /// <summary>
    /// Sets the space value for the right side of this Cell.
    /// </summary>
    /// <param name="spaceRight"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public Cell SetSpaceRight( float spaceRight )
    {
        if ( spaceRight < 0 )
        {
            throw new ArgumentException( $"spaceRight cannot be < 0: {spaceRight}" );
        }

        SpaceRight = Value.Fixed.ValueOf( spaceRight );

        return this;
    }

    // ------------------------------------------------------------------------

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
    /// Sets the padTop, padLeft, padBottom, and padRight to the specified float.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell Pad( float pad )
    {
        Pad( Value.Fixed.ValueOf( pad ) );

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

    /// <summary>
    /// Set the pad values for the top, left, bottom, and right sides of this Cell.
    /// </summary>
    /// <param name="top"> Top edge pad value. </param>
    /// <param name="left"> Left side pad value. </param>
    /// <param name="bottom"> Bottom edge pad value. </param>
    /// <param name="right"> Right side pad value. </param>
    /// <returns> This Cell for chaining. </returns>
    public Cell Pad( float top, float left, float bottom, float right )
    {
        Pad
            (
             Value.Fixed.ValueOf( top ),
             Value.Fixed.ValueOf( left ),
             Value.Fixed.ValueOf( bottom ),
             Value.Fixed.ValueOf( right )
            );

        return this;
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Sets the pad value for the top edge of this Cell.
    /// </summary>
    public Cell SetPadTop( Value padTop )
    {
        PadTop = padTop;

        return this;
    }

    /// <summary>
    /// Sets the pad value for the top edge of this Cell.
    /// </summary>
    public Cell SetPadTop( float padTop )
    {
        PadTop = Value.Fixed.ValueOf( padTop );

        return this;
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Sets the pad value for the left side of this Cell.
    /// </summary>
    public Cell SetPadLeft( Value padLeft )
    {
        PadLeft = padLeft;

        return this;
    }

    /// <summary>
    /// Sets the pad value for the left side of this Cell.
    /// </summary>
    public Cell SetPadLeft( float padLeft )
    {
        PadLeft = Value.Fixed.ValueOf( padLeft );

        return this;
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Sets the pad value for the bottom edge of this Cell.
    /// </summary>
    public Cell SetPadBottom( Value padBottom )
    {
        PadBottom = padBottom;

        return this;
    }

    /// <summary>
    /// Sets the pad value for the bottom edge of this Cell.
    /// </summary>
    public Cell SetPadBottom( float padBottom )
    {
        PadBottom = Value.Fixed.ValueOf( padBottom );

        return this;
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Sets the pad value for the right side of this Cell.
    /// </summary>
    public Cell SetPadRight( Value padRight )
    {
        PadRight = padRight;

        return this;
    }

    /// <summary>
    /// Sets the pad value for the right side of this Cell.
    /// </summary>
    public Cell SetPadRight( float padRight )
    {
        PadRight = Value.Fixed.ValueOf( padRight );

        return this;
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Sets <see cref="FillX"/> and <see cref="FillY"/> to the supplied float value. The
    /// values for fx and fy default to <see cref="DefaultFill1F"/>.
    /// </summary>
    /// <returns> This Cell for chaining </returns>
    public Cell Fill( float fx = DefaultFill1F, float fy = DefaultFill1F )
    {
        FillX = fx;
        FillY = fy;

        return this;
    }

    /// <summary>
    /// Sets <see cref="FillX"/> and <see cref="FillY"/> to either <see cref="DefaultFill1F"/>
    /// or <see cref="DefaultFill"/> depending upon the values of the supplied bool parameters.
    /// </summary>
    /// <param name="x"> If true, FillX is set to DefaultFill1F, otherwise DefaultFill. </param>
    /// <param name="y"> If true, FillY is set to DefaultFill1F, otherwise DefaultFill.  </param>
    /// <returns> This Cell for chaining </returns>
    public Cell Fill( bool x, bool y )
    {
        FillX = x ? DefaultFill1F : DefaultFill;
        FillY = y ? DefaultFill1F : DefaultFill;

        return this;
    }

    /// <summary>
    /// Sets <see cref="FillX"/> to <see cref="DefaultFill1F"/>. Leaves <see cref="FillY"/> unchanged.
    /// </summary>
    /// <returns> This Cell for chaining </returns>
    public Cell SetFillX( float fx = DefaultFill1F )
    {
        FillX = fx;

        return this;
    }

    /// <summary>
    /// Sets <see cref="FillY"/> to <see cref="DefaultFill1F"/>. Leaves <see cref="FillX"/> unchanged.
    /// </summary>
    /// <returns> This Cell for chaining </returns>
    public Cell SetFillY( float fy = DefaultFill1F )
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

    // ------------------------------------------------------------------------

    /// <summary>
    /// Grow this Cell in X and Y. Using this method is the equivalent of calling:-
    /// <code>
    /// Expand().Fill();
    /// </code>
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell Grow()
    {
        ExpandX = DefaultColspan;
        ExpandY = DefaultColspan;
        FillX   = DefaultFill1F;
        FillY   = DefaultFill1F;

        return this;
    }

    /// <summary>
    /// Grow this Cell in X.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell GrowX()
    {
        ExpandX = DefaultColspan;
        FillX   = DefaultFill1F;

        return this;
    }

    /// <summary>
    /// Grow this Cell in Y.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell GrowY()
    {
        ExpandY = DefaultColspan;
        FillY   = DefaultFill1F;

        return this;
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Expand this Cell in X.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetExpandX()
    {
        ExpandX = DefaultColspan;

        return this;
    }

    /// <summary>
    /// Expand this Cell in Y.
    /// </summary>
    /// <returns> This Cell for chaining. </returns>
    public Cell SetExpandY()
    {
        ExpandY = DefaultColspan;

        return this;
    }

    /// <summary>
    /// Expand this Cell in X and Y by the provided values.
    /// </summary>
    /// <param name="x"> The value to expand in X. Default is <see cref="DefaultColspan"/>. </param>
    /// <param name="y"> The value to expand in Y. Default is <see cref="DefaultColspan"/>. </param>
    /// <returns> This Cell for chaining. </returns>
    public Cell Expand( int x = DefaultColspan, int y = DefaultColspan )
    {
        ExpandX = x;
        ExpandY = y;

        return this;
    }

    /// <summary>
    /// Expand in X if X condition 'expandX' is <b>true</b>.
    /// Expand in Y if Y condition 'expandY' is <b>true</b>.
    /// </summary>
    /// <param name="expandX"> True to expand in X. </param>
    /// <param name="expandY"> True to expand in Y. </param>
    /// <returns> This Cell for chaining. </returns>
    public Cell Expand( bool expandX, bool expandY )
    {
        ExpandX = expandX ? DefaultColspan : DefaultExpand;
        ExpandY = expandY ? DefaultColspan : DefaultExpand;

        return this;
    }

    // ------------------------------------------------------------------------

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

    // ------------------------------------------------------------------------

    /// <summary>
    /// Configures whether this cell has uniform X and Y size with other cells in its row.
    /// </summary>
    /// <param name="ux">
    /// A bool value indicating if the cell should have uniform X sizing. Defaults to true.
    /// </param>
    /// <param name="uy">
    /// A bool value indicating if the cell should have uniform Y sizing. Defaults to true.
    /// </param>
    /// <returns>The current instance of the <see cref="Cell"/> for method chaining.</returns>
    public Cell SetUniform( bool ux = true, bool uy = true )
    {
        UniformX = ux;
        UniformY = uy;

        return this;
    }

    /// <summary>
    /// Configures whether this cell has uniform X size with other cells in its row.
    /// </summary>
    /// <param name="ux">
    /// A bool value indicating if the cell should have uniform X sizing. Defaults to true.
    /// </param>
    /// <returns>The current instance of the <see cref="Cell"/> for method chaining.</returns>
    public Cell SetUniformX( bool ux = true )
    {
        UniformX = ux;

        return this;
    }

    /// <summary>
    /// Configures whether this cell has uniform Y size with other cells in its row.
    /// </summary>
    /// <param name="uy">
    /// A bool value indicating if the cell should have uniform Y sizing. Defaults to true.
    /// </param>
    /// <returns>The current instance of the <see cref="Cell"/> for method chaining.</returns>
    public Cell SetUniformY( bool uy = true )
    {
        UniformY = uy;

        return this;
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Sets the bounds for this Cell <see cref="_actor"/>.
    /// </summary>
    public void SetActorBounds( float x, float y, float width, float height )
    {
        ActorX      = x;
        ActorY      = y;
        ActorWidth  = width;
        ActorHeight = height;
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Gets the preferred width of the actor.
    /// </summary>
    public virtual float GetPrefWidth()
    {
        PrefWidth ??= Value.PrefWidth;
        
        return PrefWidth.Get( _actor );
    }

    /// <summary>
    /// Gets the preferred height of the actor.
    /// </summary>
    public virtual float GetPrefHeight()
    {
        PrefHeight ??= Value.PrefHeight;
        
        return PrefHeight.Get( _actor );
    }

    /// <summary>
    /// Gets the minimum width of the actor.
    /// </summary>
    public float GetMinWidth()
    {
        MinWidth ??= Value.MinWidth;
        
        return MinWidth.Get( _actor );
    }

    /// <summary>
    /// Gets the minimum height of the actor.
    /// </summary>
    public float GetMinHeight()
    {
        MinHeight ??= Value.MinHeight;
        
        return MinHeight.Get( _actor );
    }

    /// <summary>
    /// Gets the maximum width of the actor.
    /// </summary>
    public float GetMaxWidth()
    {
        MaxWidth ??= Value.MaxWidth;
        
        return MaxWidth.Get( _actor );
    }

    /// <summary>
    /// Gets the maximum height of the actor. 
    /// </summary>
    public float GetMaxHeight()
    {
        MaxHeight ??= Value.MaxHeight;
        
        return MaxHeight.Get( _actor );
    }

    /// <summary>
    /// Gets the space between the top of the cell and the top of the actor.
    /// </summary>
    public float GetSpaceTop()
    {
        SpaceTop ??= Value.Zero;
        
        return SpaceTop.Get( _actor );
    }

    /// <summary>
    /// Gets the space between the left of the cell and the left of the actor.
    /// </summary>
    public float GetSpaceLeft()
    {
        SpaceLeft ??= Value.Zero;

        return SpaceLeft.Get( _actor );
    }

    /// <summary>
    /// Gets the space between the bottom of the cell and the bottom of the actor.
    /// </summary>
    public float GetSpaceBottom()
    {
        SpaceBottom ??= Value.Zero;

        return SpaceBottom.Get( _actor );
    }

    /// <summary>
    /// Gets the space between the right of the cell and the right of the actor.
    /// </summary>
    public float GetSpaceRight()
    {
        SpaceRight ??= Value.Zero;
        
        return SpaceRight.Get( _actor );
    }

    /// <summary>
    /// Gets the pad between the top of the cell and the top of the actor.
    /// </summary>
    public float GetPadTop()
    {
        PadTop ??= Value.Zero;
        
        return PadTop.Get( _actor );
    }

    /// <summary>
    /// Gets the pad between the left of the cell and the left of the actor.
    /// </summary>
    public float GetPadLeft()
    {
        PadLeft ??= Value.Zero;

        return PadLeft.Get( _actor );
    }

    /// <summary>
    /// Gets the pad between the bottom of the cell and the bottom of the actor
    /// </summary>
    public float GetPadBottom()
    {
        PadBottom ??= Value.Zero;
        
        return PadBottom.Get( _actor );
    }

    /// <summary>
    /// Gets the pad between the right of the cell and the right of the actor.
    /// </summary>
    public float GetPadRight()
    {
        PadRight ??= Value.Zero;

        return PadRight.Get( _actor );
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Gets the X-padding for this Cell, by adding together <see cref="PadLeft"/>
    /// and <see cref="PadRight"/>.
    /// </summary>
    public float GetPadX()
    {
        float padleft  = PadLeft?.Get( _actor ) ?? 0f;
        float padright = PadRight?.Get( _actor ) ?? 0f;

        return padleft + padright;
    }

    /// <summary>
    /// Gets the Y-padding for this Cell, by adding together <see cref="PadTop"/>
    /// and <see cref="PadBottom"/>.
    /// </summary>
    public float GetPadY()
    {
        float padtop    = PadTop?.Get( _actor ) ?? 0f;
        float padbottom = PadBottom?.Get( _actor ) ?? 0f;

        return padtop + padbottom;
    }

    // ------------------------------------------------------------------------

    /// <summary>
    /// Sets constraint fields to null.
    /// </summary>
    public void Clear()
    {
        MinWidth    = null;
        MinHeight   = null;
        PrefWidth   = null;
        PrefHeight  = null;
        MaxWidth    = null;
        MaxHeight   = null;
        SpaceTop    = null;
        SpaceLeft   = null;
        SpaceBottom = null;
        SpaceRight  = null;
        PadTop      = null;
        PadLeft     = null;
        PadBottom   = null;
        PadRight    = null;
        FillX       = 0;
        FillY       = 0;
        Alignment   = null;
        ExpandX     = 0;
        ExpandY     = 0;
        Colspan     = 0;
        UniformX    = false;
        UniformY    = false;
    }

    /// <summary>
    /// Reset state so the cell can be reused, setting all constraints to their default values.
    /// </summary>
    public void Reset()
    {
        ClearRuntimeState();
        Set( GetCellDefaults() );
    }

    /// <summary>
    /// Clears state that is specific to this cell instance while it is attached to a table.
    /// </summary>
    private void ClearRuntimeState()
    {
        _actor         = null;
        Table          = null;
        EndRow         = false;
        CellAboveIndex = NoCellAbove;
    }

    /// <summary>
    /// Returns the defaults to use for all cells. This can be used to avoid needing to
    /// set the same defaults for every table (eg, for spacing).
    /// </summary>
    private Cell? GetCellDefaults()
    {
        if ( ( _files == null ) || ( _files != Engine.Files ) )
        {
            _files = Engine.Files;

            _defaultCell = new Cell
            {
                MinWidth    = Value.MinWidth,
                MinHeight   = Value.MinHeight,
                PrefWidth   = Value.PrefWidth,
                PrefHeight  = Value.PrefHeight,
                MaxWidth    = Value.MaxWidth,
                MaxHeight   = Value.MaxHeight,
                SpaceTop    = Value.Zero,
                SpaceLeft   = Value.Zero,
                SpaceBottom = Value.Zero,
                SpaceRight  = Value.Zero,
                PadTop      = Value.Zero,
                PadLeft     = Value.Zero,
                PadBottom   = Value.Zero,
                PadRight    = Value.Zero,
                FillX       = DefaultFill,
                FillY       = DefaultFill,
                Alignment   = Align.Center,
                ExpandX     = DefaultExpand,
                ExpandY     = DefaultExpand,
                Colspan     = DefaultColspan,
                UniformX    = false,
                UniformY    = false,
            };
        }

        return _defaultCell;
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
            throw new LughRuntimeException( "Cannot set constraints from null Cell" );
        }

        CopyConstraintsFrom( cell );
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

        //@formatter:off
        if ( cell.MinWidth != null )    MinWidth    = cell.MinWidth;
        if ( cell.MinHeight != null )   MinHeight   = cell.MinHeight;
        if ( cell.PrefWidth != null )   PrefWidth   = cell.PrefWidth;
        if ( cell.PrefHeight != null )  PrefHeight  = cell.PrefHeight;
        if ( cell.MaxWidth != null )    MaxWidth    = cell.MaxWidth;
        if ( cell.MaxHeight != null )   MaxHeight   = cell.MaxHeight;
        if ( cell.SpaceTop != null )    SpaceTop    = cell.SpaceTop;
        if ( cell.SpaceLeft != null )   SpaceLeft   = cell.SpaceLeft;
        if ( cell.SpaceBottom != null ) SpaceBottom = cell.SpaceBottom;
        if ( cell.SpaceRight != null )  SpaceRight  = cell.SpaceRight;
        if ( cell.PadTop != null )      PadTop      = cell.PadTop;
        if ( cell.PadLeft != null )     PadLeft     = cell.PadLeft;
        if ( cell.PadBottom != null )   PadBottom   = cell.PadBottom;
        if ( cell.PadRight != null )    PadRight    = cell.PadRight;
        
        FillX       = cell.FillX;
        FillY       = cell.FillY;
        Alignment   = cell.Alignment;
        ExpandX     = cell.ExpandX;
        ExpandY     = cell.ExpandY;
        Colspan     = cell.Colspan;
        UniformX    = cell.UniformX;
        UniformY    = cell.UniformY;
        //@formatter:on
    }

    /// <summary>
    /// Copies the constraints from the given Cell to this Cell.
    /// </summary>
    private void CopyConstraintsFrom( Cell? cell )
    {
        if ( cell == null )
        {
            throw new LughRuntimeException( "Cannot copy constraints from null Cell" );
        }

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
        Alignment   = cell.Alignment;
        ExpandX     = cell.ExpandX;
        ExpandY     = cell.ExpandY;
        Colspan     = cell.Colspan;
        UniformX    = cell.UniformX;
        UniformY    = cell.UniformY;
        FillX       = cell.FillX;
        FillY       = cell.FillY;
    }

    /// <inheritdoc />
    public override string? ToString()
    {
        return _actor != null ? _actor.ToString() : base.ToString();
    }
}

// ============================================================================
// ============================================================================
