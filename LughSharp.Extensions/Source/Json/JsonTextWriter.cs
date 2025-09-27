// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using System.Numerics;
using System.Text;

using LughSharp.Lugh.Maths;
using LughUtils.source.Exceptions;

namespace Extensions.Source.Json;

[PublicAPI]
public class JsonTextWriter : TextWriter
{
    private readonly TextWriter          _writer;
    private readonly Stack< JsonObject > _stack = new();
    private          JsonObject?         _current;
    private          bool                _named;
    private          JsonOutputType      _outputType      = JsonOutputType.Json;
    private          bool                _quoteLongValues = false;

    // ========================================================================

    public JsonTextWriter() : this( new StringWriter() )
    {
    }

    public JsonTextWriter( TextWriter writer )
    {
        _writer = writer;
    }

    // ========================================================================

    /// <summary>
    /// Sets the type of JSON output. Default is <see cref="JsonOutputType.Minimal"/>.
    /// </summary>
    public void SetOutputType( JsonOutputType? outputType )
    {
        Guard.ThrowIfNull( outputType );

        _outputType = ( JsonOutputType )outputType;
    }

    /// <summary>
    /// When true, quotes long, double, BigInteger, BigDecimal types to prevent truncation in
    /// languages like JavaScript and PHP. This is not necessary when using libgdx, which handles
    /// these types without truncation.
    /// Default is false.
    /// </summary>
    public void SetQuoteLongValues( bool quoteLongValues )
    {
        _quoteLongValues = quoteLongValues;
    }

    public JsonTextWriter Name( string name )
    {
        if ( ( _current == null ) || _current.Array )
        {
            throw new InvalidOperationException( "Current item must be an object." );
        }

        if ( !_current.NeedsComma )
        {
            _current.NeedsComma = true;
        }
        else
        {
            _writer.Write( ',' );
        }

        _writer.Write( _outputType.QuoteName( name ) );
        _writer.Write( ':' );
        _named = true;

        return this;
    }

    public JsonTextWriter Object()
    {
        RequireCommaOrName();

        _stack.Push( _current = new JsonObject( false, _writer ) );

        return this;
    }

    public JsonTextWriter Object( string name )
    {
        return Name( name ).Object();
    }

    public JsonTextWriter Array()
    {
        RequireCommaOrName();

        _stack.Push( _current = new JsonObject( true, _writer ) );

        return this;
    }

    public JsonTextWriter Array( string name )
    {
        return Name( name ).Array();
    }

    /// <summary>
    /// Writes the specified JSON value, without quoting or escaping.
    /// </summary>
    public JsonTextWriter Json( string json )
    {
        RequireCommaOrName();

        _writer.Write( json );

        return this;
    }

    /// <summary>
    /// Writes the specified JSON value, without quoting or escaping.
    /// </summary>
    public JsonTextWriter Json( string name, string json )
    {
        return Name( name ).Json( json );
    }

    public JsonTextWriter Value( object? value )
    {
        if ( _quoteLongValues && value is long or double or decimal or BigInteger )
        {
            value = value.ToString();
        }
        else if ( value is int or long or float or double or decimal )
        {
            if ( value is float floatValue && ( Math.Abs( floatValue - ( long )floatValue ) < NumberUtils.FLOAT_TOLERANCE ) )
            {
                value = ( long )floatValue;
            }

            if ( value is double doubleValue && ( Math.Abs( doubleValue - ( long )doubleValue ) < NumberUtils.FLOAT_TOLERANCE ) )
            {
                value = ( long )doubleValue;
            }

            if ( value is decimal decimalValue && ( decimalValue == ( long )decimalValue ) )
            {
                value = ( long )decimalValue;
            }
        }

        RequireCommaOrName();

        _writer.Write( _outputType.QuoteValue( value ) );

        return this;
    }

    public JsonTextWriter Set( string name, object value )
    {
        return Name( name ).Value( value );
    }

    public JsonTextWriter Pop()
    {
        if ( _named )
        {
            throw new InvalidOperationException( "Expected an object, array, or value since a name was set." );
        }

        _stack.Pop().Close();
        _current = _stack.Count == 0 ? null : _stack.Peek();

        return this;
    }

    public override void Write( char[] cbuf, int off, int len )
    {
        _writer.Write( cbuf, off, len );
    }

    /// <inheritdoc />
    public override void Write( char value )
    {
        _writer.Write( value );
    }

    public override void Flush()
    {
        _writer.Flush();
    }

    public TextWriter GetWriter()
    {
        return _writer;
    }

    public override void Close()
    {
        while ( _stack.Count > 0 )
        {
            Pop();
        }

        _writer.Close();
    }

    private void RequireCommaOrName()
    {
        if ( _current == null )
        {
            return;
        }

        if ( _current.Array )
        {
            if ( !_current.NeedsComma )
            {
                _current.NeedsComma = true;
            }
            else
            {
                _writer.Write( ',' );
            }
        }
        else
        {
            if ( !_named )
            {
                throw new InvalidOperationException( "Name must be set." );
            }

            _named = false;
        }
    }

    // ========================================================================
    // Abstract Property from TextWriter

    /// <inheritdoc />
    public override Encoding Encoding { get; } = null!;

    // ========================================================================
    // ========================================================================

    private class JsonObject
    {
        public bool Array      { get; set; }
        public bool NeedsComma { get; set; }

        private readonly TextWriter _writer;

        public JsonObject( bool array, TextWriter writer )
        {
            Array   = array;
            _writer = writer;

            writer.Write( array ? '[' : '{' );
        }

        public void Close()
        {
            _writer.Write( Array ? ']' : '}' );
        }
    }
}