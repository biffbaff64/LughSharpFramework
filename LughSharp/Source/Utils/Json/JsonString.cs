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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Source.Collections;
using LughSharp.Source.Maths;

namespace LughSharp.Source.Utils.Json;

/// <summary>
/// Builder API for emitting JSON to a string.
/// </summary>
[PublicAPI]
public class JsonString
{
    public StringBuilder Buffer { get; private set; }

    // ========================================================================

    private const int DefaultBufferSize = 64;
    private const int None              = 0;
    private const int NeedsComma        = 1;
    private const int CloseObject       = '}' << 1;
    private const int CloseArray        = ']' << 1;
    private const int IsObject          = 0b1000000;

    private readonly List< int >    _stack = [ ];
    private          int            _current;
    private          bool           _named;
    private          JsonOutputType _outputType = JsonOutputType.Json;
    private          bool           _quoteLongValues;

    // ========================================================================

    /// <summary>
    /// Default constructor. Creates a new JsonString with a default initial buffer size.
    /// </summary>
    /// <param name="initialBufferSize"></param>
    public JsonString( int initialBufferSize = DefaultBufferSize )
    {
        Buffer = new StringBuilder( initialBufferSize );
    }

    /// <summary>
    /// Sets the type of JSON output. Default is <see cref="JsonOutputType.Minimal"/>.
    /// </summary>
    public void SetOutputType( JsonOutputType outputType )
    {
        this._outputType = outputType;
    }

    /// <summary>
    /// When true, long, double, BigInteger, BigDecimal types are output as strings to
    /// prevent truncation in languages like JavaScript and PHP. This is not necessary
    /// when using LughSharp, which handles these types without truncation. Default is
    /// false.
    /// </summary>
    public void SetQuoteLongValues( bool quoteLongValues )
    {
        this._quoteLongValues = quoteLongValues;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public JsonString Object()
    {
        RequireCommaOrName();

        Buffer.Append( '{' );
        _stack.Add( _current );
        _current = CloseObject;

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public JsonString Array()
    {
        RequireCommaOrName();

        Buffer.Append( '[' );
        _stack.Add( _current );
        _current = CloseArray;

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    public JsonString Value( object? value )
    {
        if ( _quoteLongValues && ( value is long or double or decimal or BigInteger ) )
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
        Buffer.Append( JsonOutput.QuoteValue( value, _outputType ) );

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Value( string value )
    {
        RequireCommaOrName();
        Buffer.Append( JsonOutput.QuoteValue( value, _outputType ) );

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Value( bool value )
    {
        RequireCommaOrName();
        Buffer.Append( value );

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Value( int value )
    {
        RequireCommaOrName();
        Buffer.Append( value );

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Value( long value )
    {
        if ( _quoteLongValues )
        {
            Value( value.ToString() );
        }
        else
        {
            RequireCommaOrName();
            Buffer.Append( value );
        }

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Value( float value )
    {
        RequireCommaOrName();
        Buffer.Append( value );

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Value( double value )
    {
        if ( _quoteLongValues )
        {
            Value( value.ToString( CultureInfo.InvariantCulture ) );
        }
        else
        {
            RequireCommaOrName();
            Buffer.Append( value );
        }

        return this;
    }

    /// <summary>
    /// Writes the specified JSON string, without quoting or escaping.
    /// </summary>
    public JsonString Json( string json )
    {
        RequireCommaOrName();
        Buffer.Append( json );

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
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
                Buffer.Append( ',' );
            }
            else if ( _current != None ) //
            {
                _current |= NeedsComma;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public JsonString Name( string name )
    {
        NameValue( name );
        _named = true;

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void NameValue( string name )
    {
        if ( ( _current & IsObject ) == 0 )
        {
            throw new InvalidOperationException( "Current item must be an object." );
        }

        if ( ( _current & NeedsComma ) != 0 )
        {
            Buffer.Append( ',' );
        }
        else
        {
            _current |= NeedsComma;
        }

        Buffer.Append( JsonOutput.QuoteName( name, _outputType ) );
        Buffer.Append( ':' );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public JsonString Object( string name )
    {
        NameValue( name );
        Buffer.Append( '{' );

        _stack.Add( _current );
        _current = CloseObject;

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public JsonString Array( string name )
    {
        NameValue( name );
        Buffer.Append( '[' );

        _stack.Add( _current );
        _current = CloseArray;

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Set( string name, object value )
    {
        Name( name );
        Value( value );

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Set( string name, string value )
    {
        NameValue( name );
        Buffer.Append( JsonOutput.QuoteValue( value, _outputType ) );

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Set( string name, bool value )
    {
        NameValue( name );
        Buffer.Append( value );

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Set( string name, int value )
    {
        NameValue( name );
        Buffer.Append( value );

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Set( string name, long value )
    {
        if ( _quoteLongValues )
        {
            Set( name, value.ToString() );
        }
        else
        {
            NameValue( name );
            Buffer.Append( value );
        }

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Set( string name, float value )
    {
        NameValue( name );
        Buffer.Append( value );

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public JsonString Set( string name, double value )
    {
        if ( _quoteLongValues )
        {
            Set( name, value.ToString( CultureInfo.InvariantCulture ) );
        }
        else
        {
            NameValue( name );
            Buffer.Append( value );
        }

        return this;
    }

    /// <summary>
    /// Writes the specified JSON string, without quoting or escaping.
    /// </summary>
    public JsonString Json( string name, string json )
    {
        NameValue( name );
        Buffer.Append( json );

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public JsonString Pop()
    {
        if ( _named )
        {
            throw new InvalidOperationException( "Expected an object, array, or value since a name was set." );
        }

        Buffer.Append( ( char )( _current >> 1 ) );

        _current = _stack.Count == 0 ? None : _stack.SafePop();

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public JsonString Close()
    {
        while ( _stack.Count > 0 )
        {
            Pop();
        }

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Reset()
    {
        Buffer.Clear();
        _stack.Clear();

        _current = None;
        _named   = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return Buffer.ToString();
    }
}

// ============================================================================
// ============================================================================