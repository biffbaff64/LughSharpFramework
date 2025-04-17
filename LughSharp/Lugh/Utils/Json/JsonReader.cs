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

using System.Diagnostics.CodeAnalysis;
using System.Text;

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics.Text;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Utils.Json;

/// <summary>
/// Lightweight JSON parser.
/// <para>
/// The default behavior is to Parse the JSON into a DOM containing <see cref="JsonValue"/> objects.
/// Extend this class and override methods to perform event driven parsing. When this is done, the
/// Parse methods will return null.
/// </para>
/// </summary>
[PublicAPI]
public partial class JsonReader : IJsonReader
{
    private static readonly byte[] _jsonEofActions = init__json_eof_actions_0();

    private const int JSON_START       = 1;
    private const int JSON_FIRST_FINAL = 35;
    private const int JSON_ERROR       = 0;
    private const int JSON_EN_OBJECT   = 5;
    private const int JSON_EN_ARRAY    = 23;
    private const int JSON_EN_MAIN     = 1;

    private readonly List< JsonValue > _elements  = new( 8 );
    private readonly List< JsonValue > _lastChild = new( 8 );

    private JsonValue? _root;
    private JsonValue? _current;

    // ========================================================================

//    /// <summary>
//    /// </summary>
//    /// <param name="data"></param>
//    /// <param name="offset"></param>
//    /// <param name="length"></param>
//    /// <returns></returns>
//    /// <exception cref="SerializationException"></exception>
//    [SuppressMessage( "ReSharper", "ConditionIsAlwaysTrueOrFalse" )]
//    public JsonValue? Parse( char[] data, int offset, int length )
//    {
//        var p                = offset;
//        var stack            = new int[ 4 ];
//        var s                = 0;
//        var nameList         = new List< string >( 8 );
//        var needsUnescape    = false;
//        var stringIsName     = false;
//        var stringIsUnquoted = false;
//
//        GdxRuntimeException parseRuntimeEx = null!;
//
//        Logger.NewLine();
//
//        try
//        {
//            var cs       = JSON_START;
//            var top      = 0;
//            var gotoTarg = 0;
//
//        _goto:
//
//            while ( true )
//            {
//                if ( gotoTarg == 0 )
//                {
//                    if ( p == length )
//                    {
//                        gotoTarg = 4;
//
//                        goto _goto;
//                    }
//
//                    if ( cs == 0 )
//                    {
//                        gotoTarg = 5;
//
//                        goto _goto;
//                    }
//                }
//
//                if ( gotoTarg == 1 )
//                {
//                _match:
//                    int trans;
//
//                    do
//                    {
//                        int keys = _jsonKeyOffsets[ cs ];
//                        int klen = _jsonSingleLengths[ cs ];
//                        trans = _jsonIndexOffsets[ cs ];
//
//                        if ( klen > 0 )
//                        {
//                            var lower = keys;
//                            var upper = ( keys + klen ) - 1;
//
//                            while ( true )
//                            {
//                                if ( upper < lower )
//                                {
//                                    break;
//                                }
//
//                                var mid = lower + ( ( upper - lower ) >> 1 );
//
//                                if ( data[ p ] < _jsonTransKeys[ mid ] )
//                                {
//                                    upper = mid - 1;
//                                }
//                                else if ( data[ p ] > _jsonTransKeys[ mid ] )
//                                {
//                                    lower = mid + 1;
//                                }
//                                else
//                                {
//                                    trans += mid - keys;
//
//                                    goto _match;
//                                }
//                            }
//
//                            keys  += klen;
//                            trans += klen;
//                        }
//
//                        klen = _jsonRangeLengths[ cs ];
//
//                        if ( klen > 0 )
//                        {
//                            var lower = keys;
//                            var upper = ( keys + ( klen << 1 ) ) - 2;
//
//                            while ( true )
//                            {
//                                if ( upper < lower )
//                                {
//                                    break;
//                                }
//
//                                var mid = lower + ( ( ( upper - lower ) >> 1 ) & ~1 );
//
//                                if ( data[ p ] < _jsonTransKeys[ mid ] )
//                                {
//                                    upper = mid - 2;
//                                }
//                                else if ( data[ p ] > _jsonTransKeys[ mid + 1 ] )
//                                {
//                                    lower = mid + 2;
//                                }
//                                else
//                                {
//                                    trans += ( mid - keys ) >> 1;
//
//                                    goto _match;
//                                }
//                            }
//
//                            trans += klen;
//                        }
//                    }
//                    while ( false );
//
//                    trans = _jsonIndicies[ trans ];
//                    cs    = _jsonTransTargs[ trans ];
//
//                    if ( _jsonTransActions[ trans ] != 0 )
//                    {
//                        int acts  = _jsonTransActions[ trans ];
//                        var nacts = ( int )_jsonActions[ acts++ ];
//
//                        while ( nacts-- > 0 )
//                        {
//                            string? name;
//
//                            switch ( _jsonActions[ acts++ ] )
//                            {
//                                case 0:
//                                    stringIsName = true;
//
//                                    break;
//
//                                case 1:
//                                    var value = new string( data, s, p - s );
//
//                                    if ( needsUnescape )
//                                    {
//                                        value = Unescape( value );
//                                    }
//
//                                outer:
//
//                                    if ( stringIsName )
//                                    {
//                                        stringIsName = false;
//
//                                        Logger.Debug( $"name: {value}" );
//
//                                        nameList.Add( value );
//                                    }
//                                    else
//                                    {
//                                        name = nameList.Count > 0 ? nameList.Pop() : null;
//
//                                        if ( stringIsUnquoted )
//                                        {
//                                            if ( value.Equals( "true" ) )
//                                            {
//                                                Logger.Debug( "bool: " + name + "=true" );
//
//                                                AddBooleanChild( name, true );
//
//                                                goto outer;
//                                            }
//
//                                            if ( value.Equals( "false" ) )
//                                            {
//                                                Logger.Debug( "bool: " + name + "=false" );
//
//                                                AddBooleanChild( name, false );
//
//                                                goto outer;
//                                            }
//
//                                            if ( value.Equals( "null" ) )
//                                            {
//                                                AddStringChild( name, null );
//
//                                                goto outer;
//                                            }
//
//                                            bool couldBeDouble = false, couldBeLong = true;
//                                        outer2:
//
//                                            for ( var i = s; i < p; i++ )
//                                            {
//                                                switch ( data[ i ] )
//                                                {
//                                                    case '0':
//                                                    case '1':
//                                                    case '2':
//                                                    case '3':
//                                                    case '4':
//                                                    case '5':
//                                                    case '6':
//                                                    case '7':
//                                                    case '8':
//                                                    case '9':
//                                                    case '-':
//                                                    case '+':
//                                                        break;
//
//                                                    case '.':
//                                                    case 'e':
//                                                    case 'E':
//                                                        couldBeDouble = true;
//                                                        couldBeLong   = false;
//
//                                                        break;
//
//                                                    default:
//                                                        couldBeDouble = false;
//                                                        couldBeLong   = false;
//
//                                                        goto outer2;
//                                                }
//                                            }
//
//                                            if ( couldBeDouble )
//                                            {
//                                                try
//                                                {
//                                                    Logger.Debug( "double: " + name + "=" + double.Parse( value ) );
//
//                                                    AddNumberChild( name, ( double )double.Parse( value ), value );
//
//                                                    goto outer;
//                                                }
//                                                catch ( ArithmeticException )
//                                                {
//                                                }
//                                            }
//                                            else if ( couldBeLong )
//                                            {
//                                                Logger.Debug( "double: " + name + "=" + double.Parse( value ) );
//
//                                                try
//                                                {
//                                                    AddNumberChild( name, ( double )long.Parse( value ), value );
//
//                                                    goto outer;
//                                                }
//                                                catch ( ArithmeticException )
//                                                {
//                                                }
//                                            }
//                                        }
//
//                                        Logger.Debug( "string: " + name + "=" + value );
//
//                                        AddStringChild( name, value );
//                                    }
//
//                                    stringIsUnquoted = false;
//                                    s                = p;
//
//                                    break;
//
//                                case 2:
//                                    name = nameList.Count > 0 ? nameList.Pop() : null;
//
//                                    Logger.Debug( "startObject: " + name );
//
//                                    StartObject( name );
//
//                                    if ( top == stack.Length )
//                                    {
//                                        var newStack = new int[ stack.Length * 2 ];
//
//                                        Array.Copy( stack, 0, newStack, 0, stack.Length );
//                                        stack = newStack;
//                                    }
//
//                                    stack[ top++ ] = cs;
//                                    cs             = 5;
//                                    gotoTarg       = 2;
//
//                                    goto _goto;
//
//                                case 3:
//                                    Logger.Debug( "endObject" );
//
//                                    Pop();
//
//                                    cs       = stack[ --top ];
//                                    gotoTarg = 2;
//
//                                    goto _goto;
//
//                                case 4:
//                                    name = nameList.Count > 0 ? nameList.Pop() : null;
//
//                                    Logger.Debug( $"startArray: {name}" );
//
//                                    StartArray( name );
//
//                                    if ( top == stack.Length )
//                                    {
//                                        var newStack = new int[ stack.Length * 2 ];
//
//                                        Array.Copy( stack, 0, newStack, 0, stack.Length );
//
//                                        stack = newStack;
//                                    }
//
//                                    stack[ top++ ] = cs;
//                                    cs             = 23;
//                                    gotoTarg       = 2;
//
//                                    goto _goto;
//
//                                case 5:
//                                    Logger.Debug( "endArray" );
//
//                                    Pop();
//
//                                    cs       = stack[ --top ];
//                                    gotoTarg = 2;
//
//                                    goto _goto;
//
//                                case 6:
//                                    var start = p - 1;
//
//                                    if ( data[ p++ ] == '/' )
//                                    {
//                                        while ( ( p != length ) && ( data[ p ] != '\n' ) )
//                                        {
//                                            p++;
//                                        }
//
//                                        p--;
//                                    }
//                                    else
//                                    {
//                                        while ( ( ( ( p + 1 ) < length ) && ( data[ p ] != '*' ) ) || ( data[ p + 1 ] != '/' ) )
//                                        {
//                                            p++;
//                                        }
//
//                                        p++;
//                                    }
//
//                                    Logger.Debug( $"comment {new string( data, start, p - start )}" );
//
//                                    break;
//
//                                case 7:
//                                    Logger.Debug( "unquotedChars" );
//
//                                    s                = p;
//                                    needsUnescape    = false;
//                                    stringIsUnquoted = true;
//
//                                    if ( stringIsName )
//                                    {
//                                    outer2:
//
//                                        while ( true )
//                                        {
//                                            switch ( data[ p ] )
//                                            {
//                                                case '\\':
//                                                    needsUnescape = true;
//
//                                                    break;
//
//                                                case '/':
//                                                    if ( ( p + 1 ) == length )
//                                                    {
//                                                        break;
//                                                    }
//
//                                                    var c = data[ p + 1 ];
//
//                                                    if ( ( c == '/' ) || ( c == '*' ) )
//                                                    {
//                                                        goto outer2;
//                                                    }
//
//                                                    break;
//
//                                                case ':':
//                                                case '\r':
//                                                case '\n':
//                                                    goto outer2;
//                                            }
//
//                                            Logger.Debug( $"unquotedChar (name): '{data[ p ]}'" );
//
//                                            p++;
//
//                                            if ( p == length )
//                                            {
//                                                break;
//                                            }
//                                        }
//                                    }
//                                    else
//                                    {
//                                    outer3:
//
//                                        while ( true )
//                                        {
//                                            switch ( data[ p ] )
//                                            {
//                                                case '\\':
//                                                    needsUnescape = true;
//
//                                                    break;
//
//                                                case '/':
//                                                    if ( ( p + 1 ) == length )
//                                                    {
//                                                        break;
//                                                    }
//
//                                                    var c = data[ p + 1 ];
//
//                                                    if ( ( c == '/' ) || ( c == '*' ) )
//                                                    {
//                                                        goto outer3;
//                                                    }
//
//                                                    break;
//
//                                                case '}':
//                                                case ']':
//                                                case ',':
//                                                case '\r':
//                                                case '\n':
//                                                    goto outer3;
//                                            }
//
//                                            Logger.Debug( $"unquotedChar (value): '{data[ p ]}'" );
//
//                                            p++;
//
//                                            if ( p == length )
//                                            {
//                                                break;
//                                            }
//                                        }
//                                    }
//
//                                    p--;
//
//                                    while ( CharacterUtils.IsSpaceChar( data[ p ] ) )
//                                    {
//                                        p--;
//                                    }
//
//                                    break;
//
//                                case 8:
//                                    Logger.Debug( "quotedChars" );
//
//                                    s             = ++p;
//                                    needsUnescape = false;
//                                    
//                                outer4:
//
//                                    while ( true )
//                                    {
//                                        switch ( data[ p ] )
//                                        {
//                                            case '\\':
//                                                needsUnescape = true;
//                                                p++;
//
//                                                break;
//
//                                            case '"':
//                                                goto outer4;
//                                        }
//
//                                        Logger.Debug( "quotedChar: '" + data[ p ] + "'" );
//
//                                        p++;
//
//                                        if ( p == length )
//                                        {
//                                            break;
//                                        }
//                                    }
//
//                                    p--;
//
//                                    break;
//                            }
//                        }
//                    }
//                }
//
//                if ( gotoTarg == 2 )
//                {
//                    if ( cs == 0 )
//                    {
//                        gotoTarg = 5;
//
//                        goto _goto;
//                    }
//
//                    if ( ++p != length )
//                    {
//                        gotoTarg = 1;
//
//                        goto _goto;
//                    }
//                }
//
//                if ( gotoTarg == 4 )
//                {
//                    if ( p == length )
//                    {
//                        int acts  = _jsonEofActions[ cs ];
//                        var nacts = ( int )_jsonActions[ acts++ ];
//
//                        while ( nacts-- > 0 )
//                        {
//                            switch ( _jsonActions[ acts++ ] )
//                            {
//                                case 1:
//                                    var value = new string( data, s, p - s );
//
//                                    if ( needsUnescape )
//                                    {
//                                        value = Unescape( value );
//                                    }
//
//                                outer:
//
//                                    if ( stringIsName )
//                                    {
//                                        stringIsName = false;
//
//                                        Logger.Debug( $"name: {value}" );
//
//                                        nameList.Add( value );
//                                    }
//                                    else
//                                    {
//                                        var name = nameList.Count > 0 ? nameList.Pop() : null;
//
//                                        if ( stringIsUnquoted )
//                                        {
//                                            if ( value.Equals( "true" ) )
//                                            {
//                                                Logger.Debug( $"bool: {name}=true" );
//
//                                                AddBooleanChild( name, true );
//
//                                                goto outer;
//                                            }
//
//                                            if ( value.Equals( "false" ) )
//                                            {
//                                                Logger.Debug( $"bool: {name}=false" );
//
//                                                AddBooleanChild( name, false );
//
//                                                goto outer;
//                                            }
//
//                                            if ( value.Equals( "null" ) )
//                                            {
//                                                AddStringChild( name, null );
//
//                                                goto outer;
//                                            }
//
//                                            var couldBeDouble = false;
//                                            var couldBeLong   = true;
//                                        outer2:
//
//                                            for ( var i = s; i < p; i++ )
//                                            {
//                                                switch ( data[ i ] )
//                                                {
//                                                    case '0':
//                                                    case '1':
//                                                    case '2':
//                                                    case '3':
//                                                    case '4':
//                                                    case '5':
//                                                    case '6':
//                                                    case '7':
//                                                    case '8':
//                                                    case '9':
//                                                    case '-':
//                                                    case '+':
//                                                        break;
//
//                                                    case '.':
//                                                    case 'e':
//                                                    case 'E':
//                                                        couldBeDouble = true;
//                                                        couldBeLong   = false;
//
//                                                        break;
//
//                                                    default:
//                                                        couldBeDouble = false;
//                                                        couldBeLong   = false;
//
//                                                        goto outer2;
//                                                }
//                                            }
//
//                                            if ( couldBeDouble )
//                                            {
//                                                try
//                                                {
//                                                    Logger.Debug( $"double: {name}={double.Parse( value )}" );
//
//                                                    AddNumberChild( name, ( double )double.Parse( value ), value );
//
//                                                    goto outer;
//                                                }
//                                                catch ( ArithmeticException )
//                                                {
//                                                }
//                                            }
//                                            else if ( couldBeLong )
//                                            {
//                                                Logger.Debug( $"double: {name}={double.Parse( value )}" );
//
//                                                try
//                                                {
//                                                    AddNumberChild( name, ( double )long.Parse( value ), value );
//
//                                                    goto outer;
//                                                }
//                                                catch ( ArithmeticException )
//                                                {
//                                                }
//                                            }
//                                        }
//
//                                        Logger.Debug( $"string: {name}={value}" );
//
//                                        AddStringChild( name, value );
//                                    }
//
//                                    stringIsUnquoted = false;
//                                    s                = p;
//
//                                    break;
//                            }
//                        }
//                    }
//                }
//
//                break;
//            }
//        }
//        catch ( GdxRuntimeException ex )
//        {
//            parseRuntimeEx = ex;
//        }
//
//        var root = _root;
//        _root    = null!;
//        _current = null!;
//        _lastChild.Clear();
//
//        if ( p < length )
//        {
//            var lineNumber = 1;
//
//            for ( var i = 0; i < p; i++ )
//            {
//                if ( data[ i ] == '\n' )
//                {
//                    lineNumber++;
//                }
//            }
//
//            var start = Math.Max( 0, p - 32 );
//
//            throw new SerializationException( $"Error parsing JSON on line {lineNumber} near: " +
//                                              $"{new string( data, start, p - start )}*ERROR*" +
//                                              $"{new string( data, p, Math.Min( 64, length - p ) )}",
//                                              parseRuntimeEx );
//        }
//
//        if ( _elements.Count != 0 )
//        {
//            var element = _elements.Peek();
//            _elements.Clear();
//
//            if ( element.IsObject() )
//            {
//                throw new SerializationException( "Error parsing JSON, unmatched brace." );
//            }
//
//            throw new SerializationException( "Error parsing JSON, unmatched bracket." );
//        }
//
//        if ( parseRuntimeEx != null )
//        {
//            throw new SerializationException( $"Error parsing JSON: {new string( data )}", parseRuntimeEx );
//        }
//
//        return root;
//    }

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="child"></param>
    private void AddChild( string? name, JsonValue child )
    {
        Logger.Checkpoint();
        
        child.Name = name;

        if ( _current == null )
        {
            _current = child;
            _root    = child;
        }
        else if ( _current.IsArray() || _current.IsObject() )
        {
            child.Parent = _current;

            if ( _current.Size == 0 )
            {
                _current.Child = child;
            }
            else
            {
                var last = _lastChild.Pop();
                last.Next  = child;
                child.Prev = last;
            }

            _lastChild.Add( child );
            _current.Size++;
        }
        else
        {
            _root = _current;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    protected void StartObject( string? name )
    {
        Logger.Checkpoint();
        
        var value = new JsonValue( JsonValue.ValueTypes.ObjectValue );

        if ( _current != null )
        {
            AddChild( name, value );
        }

        _elements.Add( value );
        _current = value;
    }

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    protected void StartArray( string? name )
    {
        Logger.Checkpoint();
        
        var value = new JsonValue( JsonValue.ValueTypes.ArrayValue );

        if ( _current != null )
        {
            AddChild( name, value );
        }

        _elements.Add( value );
        _current = value;
    }

    /// <summary>
    /// </summary>
    protected void Pop()
    {
        _root = _elements.Pop();

        if ( _current?.Size > 0 )
        {
            _lastChild.Pop();
        }

        _current = _elements.Count > 0 ? _elements.Peek() : null;
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    protected void AddStringChild( string? name, string? value )
    {
        Logger.Checkpoint();
        
        AddChild( name, new JsonValue( value ) );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="stringValue"></param>
    protected void AddNumberChild( string? name, double value, string stringValue )
    {
        Logger.Checkpoint();
        
        AddChild( name, new JsonValue( value, stringValue ) );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <param name="stringValue"></param>
    protected void AddNumberChild( string? name, long value, string stringValue )
    {
        Logger.Checkpoint();
        
        AddChild( name, new JsonValue( value, stringValue ) );
    }

    /// <summary>
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    protected void AddBooleanChild( string? name, bool value )
    {
        Logger.Checkpoint();
        
        AddChild( name, new JsonValue( value ) );
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    private static string Unescape( string value )
    {
        var length = value.Length;
        var buffer = new StringBuilder( length + 16 );

        for ( var i = 0; i < length; )
        {
            var c = value.ToCharArray()[ i++ ];

            if ( c != '\\' )
            {
                buffer.Append( c );

                continue;
            }

            if ( i == length )
            {
                break;
            }

            c = value.ToCharArray()[ i++ ];

            if ( c == 'u' )
            {
                buffer.Append( char.ConvertFromUtf32( Convert.ToInt32( value.Substring( i, 4 ), 16 ) ) );
                i += 4;

                continue;
            }

            switch ( c )
            {
                case '"':
                case '\\':
                case '/':
                    break;

                case 'b':
                    c = '\b';

                    break;

                case 'f':
                    c = '\f';

                    break;

                case 'n':
                    c = '\n';

                    break;

                case 'r':
                    c = '\r';

                    break;

                case 't':
                    c = '\t';

                    break;

                default:
                    throw new SerializationException( $"Illegal escaped character: \\{c}" );
            }

            buffer.Append( c );
        }

        return buffer.ToString();
    }
}