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
    public static readonly Value MinWidth = new LambdaValue( ctx => ctx is ILayout layout
                                                                 ? layout.GetMinWidth()
                                                                 : ( ctx?.Width ?? 0 ) );

    /// <summary>
    /// Value that is the minHeight of the actor in the cell.
    /// </summary>
    public static readonly Value MinHeight = new LambdaValue( ctx => ctx is ILayout layout
                                                                  ? layout.GetMinHeight()
                                                                  : ( ctx?.Height ?? 0 ) );

    /// <summary>
    /// Value that is the preferred width of the actor in the cell.
    /// </summary>
    public static readonly Value PrefWidth = new LambdaValue( ctx => ctx is ILayout layout
                                                                  ? layout.GetPrefWidth()
                                                                  : ( ctx?.Width ?? 0 ) );

    /// <summary>
    /// Value that is the preferred height of the actor in the cell.
    /// </summary>
    public static readonly Value PrefHeight = new LambdaValue( ctx => ctx is ILayout layout
                                                                   ? layout.GetPrefHeight()
                                                                   : ( ctx?.Height ?? 0 ) );

    /// <summary>
    /// Value that is the maxWidth of the actor in the cell.
    /// </summary>
    public static readonly Value MaxWidth = new LambdaValue( ctx => ctx is ILayout layout
                                                                 ? layout.GetMaxWidth()
                                                                 : ( ctx?.Width ?? 0 ) );

    /// <summary>
    /// Value that is the maxHeight of the actor in the cell.
    /// </summary>
    public static readonly Value MaxHeight = new LambdaValue( ctx => ctx is ILayout layout
                                                                  ? layout.GetMaxHeight()
                                                                  : ( ctx?.Height ?? 0 ) );

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

            if ( value is >= -10 and <= 100
              && Math.Abs( value - ( int )value ) < NumberUtils.FloatTolerance )
            {
                int index = ( int )value + 10;

                return _cache[ index ] ??= new Fixed( value );
            }

            return new Fixed( value );
        }

        /// Allow code like "cell.Width(10)" instead of "cell.Width(Value.Fixed.ValueOf(10))"
        public static implicit operator Fixed( float v )
        {
            return ValueOf( v );
        }

        public override string ToString()
        {
            return _value.ToString( CultureInfo.InvariantCulture );
        }
    }

    // ========================================================================
    // ========================================================================

    private class LambdaValue : Value
    {
        private readonly Func< Actor?, float > _getter;

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