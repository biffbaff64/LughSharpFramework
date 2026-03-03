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
using System.Text;

using JetBrains.Annotations;

namespace LughSharp.Core.Utils.Json;

/// <summary>
/// Builder API for emitting JSON to a string.
/// </summary>
[PublicAPI]
public class JsonString
{
    private const int None        = 0;
    private const int NeedsComma  = 1;
    private const int CloseObject = '}' << 1;
    private const int CloseArray  = ']' << 1;
    private const int IsObject    = 0b1000000;

    private readonly StringBuilder  _buffer;
    private readonly List< int >    _stack = [ ];
    private          int            _current;
    private          bool           _named;
    private          JsonOutputType _outputType = JsonOutputType.Json;
    private          bool           _quoteLongValues;

    // ========================================================================

    public JsonString() : this( 64 )
    {
    }

    public JsonString( int initialBufferSize )
    {
        _buffer = new StringBuilder( initialBufferSize );
    }

    public StringBuilder GetBuffer()
    {
        return _buffer;
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

        _buffer.Append( '{' );
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

        _buffer.Append( '[' );
        _stack.Add( _current );
        _current = CloseArray;

        return this;
    }

    /// <summary>
    /// Prefer calling the more specific value() methods.
    /// </summary>
    public JsonString Value( object? value )
    {
        if ( _quoteLongValues
          && ( value instanceof Long || value instanceof double || value instanceof BigDecimal || value instanceof
            bigInteger)) {
            value = value.toString();
        } else if ( value instanceof number) {
            var  number    = ( JSType.Number )value;
            long longValue = number.longValue();

            if ( number.doubleValue() == longValue )
            {
                value = longValue;
            }
        }
        RequireCommaOrName();
        _buffer.append( _outputType.quoteValue( value ) );

        return this;
    }

    public JsonString Value( string value )
    {
        RequireCommaOrName();
        _buffer.append( _outputType.quoteValue( value ) );

        return this;
    }

    public JsonString Value( bool value )
    {
        RequireCommaOrName();
        _buffer.append( value );

        return this;
    }

    public JsonString Value( int value )
    {
        RequireCommaOrName();
        _buffer.append( value );

        return this;
    }

    public JsonString Value( long value )
    {
        if ( _quoteLongValues )
        {
            value( Long.toString( value ) );
        }
        else
        {
            RequireCommaOrName();
            _buffer.append( value );
        }

        return this;
    }

    public JsonString Value( float value )
    {
        RequireCommaOrName();
        _buffer.append( value );

        return this;
    }

    public JsonString Value( double value )
    {
        if ( _quoteLongValues )
        {
            value( double.toString( value ) );
        }
        else
        {
            RequireCommaOrName();
            _buffer.append( value );
        }

        return this;
    }

    /** Writes the specified JSON string, without quoting or escaping. */
    public JsonString Json( string json )
    {
        RequireCommaOrName();
        _buffer.append( json );

        return this;
    }

    private void RequireCommaOrName()
    {
        if ( ( _current & IsObject ) != 0 )
        {
            if ( !_named )
            {
                throw new IllegalStateException( "Name must be set." );
            }

            _named = false;
        }
        else
        {
            if ( ( _current & NeedsComma ) != 0 )
            {
                _buffer.append( ',' );
            }
            else if ( _current != None ) //
            {
                _current |= NeedsComma;
            }
        }
    }

    public JsonString Name( string name )
    {
        NameValue( name );
        _named = true;

        return this;
    }

    private void NameValue( string name )
    {
        if ( ( _current & IsObject ) == 0 )
        {
            throw new IllegalStateException( "Current item must be an object." );
        }

        if ( ( _current & NeedsComma ) != 0 )
        {
            _buffer.append( ',' );
        }
        else
        {
            _current |= NeedsComma;
        }

        _buffer.append( _outputType.quoteName( name ) );
        _buffer.append( ':' );
    }

    public JsonString object (string name) {
        NameValue( Name );
        _buffer.append( '{' );
        _stack.add( _current );
        _current = object;

        return this;
    }

    public JsonString Array( string name )
    {
        NameValue( name );
        _buffer.append( '[' );
        _stack.add( _current );
        _current = array;

        return this;
    }

    /** Prefer calling the more specific set() methods. */
    public JsonString Set( string name, object value )
    {
        name( name );
        value( value );

        return this;
    }

    public JsonString Set( string name, string value )
    {
        NameValue( name );
        _buffer.append( _outputType.quoteValue( value ) );

        return this;
    }

    public JsonString Set( string name, bool value )
    {
        NameValue( name );
        _buffer.append( value );

        return this;
    }

    public JsonString Set( string name, int value )
    {
        NameValue( name );
        _buffer.append( value );

        return this;
    }

    public JsonString Set( string name, long value )
    {
        if ( _quoteLongValues )
        {
            set( name, Long.toString( value ) );
        }
        else
        {
            NameValue( name );
            _buffer.append( value );
        }

        return this;
    }

    public JsonString Set( string name, float value )
    {
        NameValue( name );
        _buffer.append( value );

        return this;
    }

    public JsonString Set( string name, double value )
    {
        if ( _quoteLongValues )
        {
            set( name, double.toString( value ) );
        }
        else
        {
            NameValue( name );
            _buffer.append( value );
        }

        return this;
    }

    /** Writes the specified JSON string, without quoting or escaping. */
    public JsonString Json( string name, string json )
    {
        NameValue( name );
        _buffer.append( json );

        return this;
    }

    public JsonString Pop()
    {
        if ( _named )
        {
            throw new IllegalStateException( "Expected an object, array, or value since a name was set." );
        }

        _buffer.append( ( char )( _current >> 1 ) );
        _current = _stack.size == 0 ? None : _stack.items[ --_stack.size ];

        return this;
    }

    public JsonString Close()
    {
        while ( _stack.size > 0 )
        {
            Pop();
        }

        return this;
    }

    public void Reset()
    {
        _buffer.setLength( 0 );
        _stack.size = 0;
        _current    = None;
        _named      = false;
    }

    public string ToString()
    {
        return _buffer.toString();
    }
}

// ============================================================================
// ============================================================================