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

using System.Globalization;

using LughSharp.Core.Scene2D.Utils;

namespace LughSharp.Core.Scene2D.UI;

/// <summary>
/// Value placeholder, allowing the value to be computed on request. Values can be
/// provided an actor for context to reduce the number of value instances that need
/// to be created and reduce verbosity in code that specifies values.
/// </summary>
[PublicAPI]
public abstract class Value
{
    /// <summary>
    /// A value that is always zero.
    /// </summary>
    public static readonly Fixed Zero = new( 0 );

    /// <summary>
    /// Value that is the minWidth of the actor in the cell.
    /// </summary>
    public static readonly Value MinWidth = FromLayout( l => l.GetMinWidth(), ctx => ctx?.Width ?? 0 );

    /// <summary>
    /// Value that is the minHeight of the actor in the cell.
    /// </summary>
    public static readonly Value MinHeight = FromLayout( l => l.GetMinHeight(), ctx => ctx?.Height ?? 0 );

    /// <summary>
    /// Value that is the preferred width of the actor in the cell.
    /// </summary>
    public static readonly Value PrefWidth = FromLayout( l => l.GetPrefWidth(), ctx => ctx?.Width ?? 0 );

    /// <summary>
    /// Value that is the preferred height of the actor in the cell.
    /// </summary>
    public static readonly Value PrefHeight = FromLayout( l => l.GetPrefHeight(), ctx => ctx?.Height ?? 0 );

    /// <summary>
    /// Value that is the maxWidth of the actor in the cell.
    /// </summary>
    public static readonly Value MaxWidth = FromLayout( l => l.GetMaxWidth(), ctx => ctx?.Width ?? 0 );

    /// <summary>
    /// Value that is the maxHeight of the actor in the cell.
    /// </summary>
    public static readonly Value MaxHeight = FromLayout( l => l.GetMaxHeight(), ctx => ctx?.Height ?? 0 );

    /// <summary>
    /// Returns a value that is the specified percent of the width of the actor.
    /// </summary>
    public static Value PercentWidth( float percent )
    {
        return new LambdaValue( ctx => ( ctx?.Width ?? 0 ) * percent );
    }

    /// <summary>
    /// Returns a value that is the specified percent of the height of the actor.
    /// </summary>
    public static Value PercentHeight( float percent )
    {
        return new LambdaValue( ctx => ( ctx?.Height ?? 0 ) * percent );
    }

    /// <summary>
    /// Returns a value that is the specified percent of the width of the specified actor.
    /// </summary>
    public static Value PercentWidth( float percent, Actor actor )
    {
        Guard.Against.Null( actor );

        return new LambdaValue( _ => actor.Width * percent );
    }

    /// <summary>
    /// Returns a value that is the specified percent of the height of the specified actor.
    /// </summary>
    public static Value PercentHeight( float percent, Actor actor )
    {
        Guard.Against.Null( actor );

        return new LambdaValue( _ => actor.Height * percent );
    }

    /// <summary>
    /// Creates a <see cref="Value"/> that delegates to <paramref name="fromLayout"/> when the
    /// context actor implements <see cref="ILayout"/>, and to <paramref name="fallback"/> otherwise.
    /// </summary>
    /// <param name="fromLayout">Invoked with the context cast to <see cref="ILayout"/>.</param>
    /// <param name="fallback">Invoked with the raw context actor when it is not an <see cref="ILayout"/>.</param>
    private static Value FromLayout( Func< ILayout, float > fromLayout, Func< Actor?, float > fallback )
        => new LambdaValue( ctx => ctx is ILayout layout ? fromLayout( layout ) : fallback( ctx ) );

    public abstract float Get( Actor? context = null );

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// A fixed value that is not computed each time it is used.
    /// </summary>
    [PublicAPI]
    public class Fixed : Value
    {
        private static readonly Fixed?[] _cache = new Fixed[ 111 ];
        private readonly        float    _value;

        // ====================================================================

        /// <summary>
        /// Creates a new fixed value from the supplied float.
        /// </summary>
        public Fixed( float value )
        {
            _value = value;
        }

        public override float Get( Actor? context = null )
        {
            return _value;
        }

        public static Fixed ValueOf( float value )
        {
            if ( value == 0 )
            {
                return Zero;
            }

            if ( ( value is >= -10 and <= 100 )
              && Math.Abs( value - ( int )value ) < NumberUtils.FloatTolerance )
            {
                int index = ( int )value + 10;

                return _cache[ index ] ??= new Fixed( value );
            }

            return new Fixed( value );
        }

        /// <summary>
        /// Allows code like <c>Cell.Width(10)</c> instead of <c>Cell.Width(Value.Fixed.ValueOf(10))</c>
        /// </summary>
        public static implicit operator Fixed( float v )
        {
            return ValueOf( v );
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _value.ToString( CultureInfo.InvariantCulture );
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// A value computed on demand via a delegate, used for all built-in layout-relative values.
    /// </summary>
    private class LambdaValue : Value
    {
        private readonly Func< Actor?, float > _getter;

        // ====================================================================

        public LambdaValue( Func< Actor?, float > getter )
        {
            _getter = getter;
        }

        public override float Get( Actor? context = null )
        {
            return _getter( context );
        }
    }
}

// ============================================================================
// ============================================================================