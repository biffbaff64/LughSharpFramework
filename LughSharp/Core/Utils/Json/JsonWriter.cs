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
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices.JavaScript;

using JetBrains.Annotations;

using LughSharp.Core.Maths;

namespace LughSharp.Core.Utils.Json;

[PublicAPI]
public class JsonWriter : IJsonWriter
{
    public bool           QuoteLongValues { get; set; } = true;
    public JsonOutputType OutputType      { get; set; } = JsonOutputType.Json;
    public TextWriter?    Writer          { get; set; }

    // ========================================================================

    private readonly Stack< int > _stack = [ ];

    private int  _current;
    private bool _named;

    // ========================================================================

    private const int None        = 0;
    private const int NeedsComma  = 1;
    private const int CloseObject = '}' << 1;
    private const int CloseArray  = ']' << 1;
    private const int IsObject    = 0b1000000;

    // ========================================================================

    /// <summary>
    /// Default constructor. A Writer must be set before use.
    /// </summary>
    protected JsonWriter()
    {
    }

    public JsonWriter( TextWriter Writer )
    {
        Writer = Writer;
    }

    public void SetWriter( TextWriter Writer )
    {
        Writer = Writer;
    }

    public JsonWriter Array()
    {
        RequireCommaOrName();

        Writer?.Write( '[' );

        _stack.Push( _current );
        _current = CloseArray;

        return this;
    }

    public JsonWriter Array( string? name )
    {
        NameValue( name );

        return Array();
    }

    public JsonWriter Object()
    {
        RequireCommaOrName();

        Writer?.Write( '{' );

        _stack.Push( _current );
        _current = CloseObject;

        return this;
    }

    public JsonWriter Object( string? name )
    {
        NameValue( name );

        return Object();
    }

    public JsonWriter Name( string? name )
    {
        NameValue( name );
        _named = true;

        return this;
    }

    /** Prefer calling the more specific value() methods. */
    public JsonWriter Value( object? value )
    {
        if ( QuoteLongValues && ( value is long or double or decimal or BigInteger ) )
        {
            value = value.ToString();
        }
        else if ( value is ValueType && NumberUtils.IsNumeric( value ) )
        {
            // Convert to double to check if it's effectively a whole number
            var dblValue  = Convert.ToDouble( value );
            var longValue = ( long )dblValue;

            // If the double value is exactly the same as the long (e.g., 10.0 == 10)
            // we strip the decimal for cleaner JSON.
            if ( Math.Abs( dblValue - longValue ) < double.Epsilon )
            {
                value = longValue;
            }
        }

        RequireCommaOrName();
        Writer?.Write( JsonOutput.QuoteValue( value, OutputType ) );

        return this;
    }

    public JsonWriter Value( String value )
    {
        RequireCommaOrName();
        Writer?.Write( JsonOutput.QuoteValue( value, OutputType ) );

        return this;
    }

    public JsonWriter Value( bool value )
    {
        RequireCommaOrName();
        Writer?.Write( value ? "true" : "false" );

        return this;
    }

    public JsonWriter Value( int value )
    {
        RequireCommaOrName();
        Writer?.Write( value.ToString() );

        return this;
    }

    public JsonWriter Value( long value )
    {
        if ( QuoteLongValues )
        {
            Value( value.ToString() );
        }
        else
        {
            RequireCommaOrName();
            Writer?.Write( value.ToString() );
        }

        return this;
    }

    public JsonWriter Value( float value )
    {
        RequireCommaOrName();
        Writer?.Write( value.ToString( CultureInfo.InvariantCulture ) );

        return this;
    }

    public JsonWriter Value( double value )
    {
        if ( QuoteLongValues )
        {
            Value( value.ToString( CultureInfo.InvariantCulture ) );
        }
        else
        {
            RequireCommaOrName();
            Writer?.Write( value.ToString( CultureInfo.InvariantCulture ) );
        }

        return this;
    }

    private void RequireCommaOrName()
    {
        if ( ( _current & IsObject ) != 0 )
        {
            if ( !_named )
            {
                throw new InvalidOperationException( "Name must be set." );
            }

            _named = false;
        }
        else
        {
            if ( ( _current & NeedsComma ) != 0 )
            {
                Writer?.Write( ',' );
            }
            else if ( _current != None )
            {
                _current |= NeedsComma;
            }
        }
    }

    private void NameValue( string? name )
    {
        if ( ( _current & IsObject ) == 0 )
        {
            throw new InvalidOperationException( "Current item must be an object." );
        }

        if ( ( _current & NeedsComma ) != 0 )
        {
            Writer?.Write( ',' );
        }
        else
        {
            _current |= NeedsComma;
        }

        Writer?.Write( JsonOutput.QuoteName( name ?? "Not Set", OutputType ) );
        Writer?.Write( ':' );
    }
}

// ============================================================================
// ============================================================================