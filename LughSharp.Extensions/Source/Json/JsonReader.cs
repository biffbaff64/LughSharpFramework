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

using System.Text;

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Collections;

namespace Extensions.Source.Json;

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

    private const int START_PARSE    = 0;
    private const int STRING_PARSE   = 1;
    private const int START_OBJECT   = 2;
    private const int END_OBJECT     = 3;
    private const int START_ARRAY    = 4;
    private const int END_ARRAY      = 5;
    private const int SKIP_COMMENT   = 6;
    private const int UNQUOTED_CHARS = 7;
    private const int QUOTED_CHARS   = 8;

    private int            _position;
    private int[]?         _stack;
    private int            _s                = 0;
    private List< string > _nameList         = new( 8 );
    private bool           _needsUnescape    = false;
    private bool           _stringIsName     = false;
    private bool           _stringIsUnquoted = false;
    private int            _currentState     = JSON_START;
    private int            _top              = 0;
    private int            _gotoTarg         = START_PARSE;
    private string?        _name;

    // ========================================================================

//    [SuppressMessage( "ReSharper", "ConditionIsAlwaysTrueOrFalse" )]
//    public JsonValue? Parse( char[] data, int offset, int length )
//    {
//        _position         = offset;
//        _stack            = new int[ 4 ];
//        _s                = 0;
//        _nameList         = new List< string >( 8 );
//        _needsUnescape    = false;
//        _stringIsName     = false;
//        _stringIsUnquoted = false;
//
//        GdxRuntimeException parseRuntimeEx = null!;
//
//        Logger.NewLine();
//
//        try
//        {
//            _currentState = JSON_START;
//            _top          = 0;
//            _gotoTarg     = START_PARSE;
//
//        _goto:
//
//            while ( true )
//            {
//                if ( _gotoTarg == START_PARSE )
//                {
//                    if ( _position == length )
//                    {
//                        _gotoTarg = START_ARRAY;
//
//                        goto _goto;
//                    }
//
//                    if ( _currentState == 0 )
//                    {
//                        _gotoTarg = END_ARRAY;
//
//                        goto _goto;
//                    }
//                }
//
//                if ( _gotoTarg == STRING_PARSE )
//                {
//                _match:
//                    int transition;
//
//                    do
//                    {
//                        int keys = _jsonKeyOffsets[ _currentState ];
//                        int klen = _jsonSingleLengths[ _currentState ];
//
//                        transition = _jsonIndexOffsets[ _currentState ];
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
//                                if ( data[ _position ] < _jsonTransitionKeys[ mid ] )
//                                {
//                                    upper = mid - 1;
//                                }
//                                else if ( data[ _position ] > _jsonTransitionKeys[ mid ] )
//                                {
//                                    lower = mid + 1;
//                                }
//                                else
//                                {
//                                    transition += mid - keys;
//
//                                    goto _match;
//                                }
//                            }
//
//                            keys       += klen;
//                            transition += klen;
//                        }
//
//                        klen = _jsonRangeLengths[ _currentState ];
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
//                                if ( data[ _position ] < _jsonTransitionKeys[ mid ] )
//                                {
//                                    upper = mid - 2;
//                                }
//                                else if ( data[ _position ] > _jsonTransitionKeys[ mid + 1 ] )
//                                {
//                                    lower = mid + 2;
//                                }
//                                else
//                                {
//                                    transition += ( mid - keys ) >> 1;
//
//                                    goto _match;
//                                }
//                            }
//
//                            transition += klen;
//                        }
//                    }
//                    while ( false );
//
//                    transition    = _jsonIndicies[ transition ];
//                    _currentState = _jsonTransitionTargs[ transition ];
//
//                    if ( _jsonTransitionActions[ transition ] != 0 )
//                    {
//                        int acts  = _jsonTransitionActions[ transition ];
//                        var nacts = ( int )_jsonActions[ acts++ ];
//
//                        while ( nacts-- > 0 )
//                        {
//                            switch ( _jsonActions[ acts++ ] )
//                            {
//                                case START_PARSE:
//                                    _stringIsName = true;
//
//                                    break;
//
//                                case STRING_PARSE:
//                                    StringParseActions( data );
//
//                                    break;
//
//                                case START_OBJECT:
//                                    StartObjectActions();
//
//                                    goto _goto;
//
//                                case END_OBJECT:
//                                    EndObjectActions();
//
//                                    goto _goto;
//
//                                case START_ARRAY:
//                                    StartArrayActions();
//
//                                    goto _goto;
//
//                                case END_ARRAY:
//                                    EndArrayActions();
//
//                                    goto _goto;
//
//                                case SKIP_COMMENT:
//                                    SkipCommentActions( data, length );
//
//                                    break;
//
//                                case UNQUOTED_CHARS:
//                                    UnquotedCharsActions( data, length );
//
//                                    break;
//
//                                case QUOTED_CHARS:
//                                    QuotedCharsActions( data, length );
//
//                                    break;
//                            }
//                        }
//                    }
//                }
//
//                if ( _gotoTarg == START_OBJECT )
//                {
//                    if ( _currentState == 0 )
//                    {
//                        _gotoTarg = END_ARRAY;
//
//                        goto _goto;
//                    }
//
//                    if ( ++_position != length )
//                    {
//                        _gotoTarg = STRING_PARSE;
//
//                        goto _goto;
//                    }
//                }
//
//                if ( _gotoTarg == START_ARRAY )
//                {
//                    if ( _position == length )
//                    {
//                        HandleEofActions( data );
//                    }
//                }
//
//                break;
//            }
//        }
//        catch ( GdxRuntimeException ex )
//        {
//            Logger.Checkpoint();
//            
//            parseRuntimeEx = ex;
//        }
//
//        var root = _root;
//        _root    = null!;
//        _current = null!;
//        _lastChild.Clear();
//
//        if ( _position < length )
//        {
//            var lineNumber = 1;
//
//            for ( var i = 0; i < _position; i++ )
//            {
//                if ( data[ i ] == '\n' )
//                {
//                    lineNumber++;
//                }
//            }
//
//            var start = Math.Max( 0, _position - 32 );
//
//            throw new SerializationException( $"Error parsing JSON on line {lineNumber} near: " +
//                                              $"{new string( data, start, _position - start )}*ERROR*" +
//                                              $"{new string( data, _position, Math.Min( 64, length - _position ) )}",
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
//
//    private void StringParseActions( char[] data )
//    {
//        var value = new string( data, _s, _position - _s );
//
//        if ( _needsUnescape )
//        {
//            value = Unescape( value );
//        }
//
//    outer:
//
//        if ( _stringIsName )
//        {
//            _stringIsName = false;
//
//            Logger.Debug( $"name: {value}" );
//
//            _nameList.Add( value );
//        }
//        else
//        {
//            _name = _nameList.Count > 0 ? _nameList.Pop() : null;
//
//            if ( _stringIsUnquoted )
//            {
//                switch ( value )
//                {
//                    case "true":
//                        Logger.Debug( "bool: " + _name + "=true" );
//
//                        AddBooleanChild( _name, true );
//
//                        goto outer;
//
//                    case "false":
//                        Logger.Debug( "bool: " + _name + "=false" );
//
//                        AddBooleanChild( _name, false );
//
//                        goto outer;
//
//                    case "null":
//                        AddStringChild( _name, null );
//
//                        goto outer;
//                }
//
//                var couldBeDouble = false;
//                var couldBeLong   = true;
//            outer2:
//
//                for ( var i = _s; i < _position; i++ )
//                {
//                    switch ( data[ i ] )
//                    {
//                        case '0':
//                        case '1':
//                        case '2':
//                        case '3':
//                        case '4':
//                        case '5':
//                        case '6':
//                        case '7':
//                        case '8':
//                        case '9':
//                        case '-':
//                        case '+':
//                            break;
//
//                        case '.':
//                        case 'e':
//                        case 'E':
//                            couldBeDouble = true;
//                            couldBeLong   = false;
//
//                            break;
//
//                        default:
//                            couldBeDouble = false;
//                            couldBeLong   = false;
//
//                            goto outer2;
//                    }
//                }
//
//                if ( couldBeDouble )
//                {
//                    try
//                    {
//                        Logger.Debug( "double: " + _name + "=" + double.Parse( value ) );
//
//                        AddNumberChild( _name, ( double )double.Parse( value ), value );
//
//                        goto outer;
//                    }
//                    catch ( ArithmeticException )
//                    {
//                    }
//                }
//                else if ( couldBeLong )
//                {
//                    Logger.Debug( "double: " + _name + "=" + double.Parse( value ) );
//
//                    try
//                    {
//                        AddNumberChild( _name, ( double )long.Parse( value ), value );
//
//                        goto outer;
//                    }
//                    catch ( ArithmeticException )
//                    {
//                    }
//                }
//            }
//
//            Logger.Debug( "string: " + _name + "=" + value );
//
//            AddStringChild( _name, value );
//        }
//
//        _stringIsUnquoted = false;
//        _s                = _position;
//    }
//
//    private void StartObjectActions()
//    {
//        _name = _nameList.Count > 0 ? _nameList.Pop() : null;
//
//        Logger.Debug( "startObject: " + _name );
//
//        StartObject( _name );
//
//        if ( _top == _stack?.Length )
//        {
//            var newStack = new int[ _stack.Length * 2 ];
//
//            Array.Copy( _stack, 0, newStack, 0, _stack.Length );
//            _stack = newStack;
//        }
//
//        _stack![ _top++ ] = _currentState;
//        _currentState     = 5;
//        _gotoTarg         = START_OBJECT;
//    }
//
//    private void EndObjectActions()
//    {
//        Logger.Debug( "endObject" );
//
//        Pop();
//
//        _currentState = _stack![ --_top ];
//        _gotoTarg     = START_OBJECT;
//    }
//
//    private void StartArrayActions()
//    {
//        _name = _nameList.Count > 0 ? _nameList.Pop() : null;
//
//        Logger.Debug( $"startArray: {_name}" );
//
//        StartArray( _name );
//
//        if ( _top == _stack?.Length )
//        {
//            var newStack = new int[ _stack.Length * 2 ];
//
//            Array.Copy( _stack, 0, newStack, 0, _stack.Length );
//
//            _stack = newStack;
//        }
//
//        _stack![ _top++ ] = _currentState;
//        _currentState     = 23;
//        _gotoTarg         = START_OBJECT;
//    }
//
//    private void EndArrayActions()
//    {
//        Logger.Debug( "endArray" );
//
//        Pop();
//
//        _currentState = _stack![ --_top ];
//        _gotoTarg     = START_OBJECT;
//    }
//
//    private void SkipCommentActions( char[] data, int length )
//    {
//        var start = _position - 1;
//
//        if ( data[ _position++ ] == '/' )
//        {
//            while ( ( _position != length ) && ( data[ _position ] != '\n' ) )
//            {
//                _position++;
//            }
//
//            _position--;
//        }
//        else
//        {
//            while ( ( ( ( _position + 1 ) < length ) && ( data[ _position ] != '*' ) ) ||
//                    ( data[ _position + 1 ] != '/' ) )
//            {
//                _position++;
//            }
//
//            _position++;
//        }
//
//        Logger.Debug( $"comment {new string( data, start, _position - start )}" );
//    }
//
//    private void UnquotedCharsActions( char[] data, int length )
//    {
//        Logger.Debug( "unquotedChars" );
//
//        _s                = _position;
//        _needsUnescape    = false;
//        _stringIsUnquoted = true;
//
//        if ( _stringIsName )
//        {
//        outer2:
//
//            while ( true )
//            {
//                switch ( data[ _position ] )
//                {
//                    case '\\':
//                        _needsUnescape = true;
//
//                        break;
//
//                    case '/':
//                        if ( ( _position + 1 ) == length )
//                        {
//                            break;
//                        }
//
//                        var c = data[ _position + 1 ];
//
//                        if ( ( c == '/' ) || ( c == '*' ) )
//                        {
//                            goto outer2;
//                        }
//
//                        break;
//
//                    case ':':
//                    case '\r':
//                    case '\n':
//                        goto outer2;
//                }
//
//                Logger.Debug( $"unquotedChar (name): '{data[ _position ]}'" );
//
//                _position++;
//
//                if ( _position == length )
//                {
//                    break;
//                }
//            }
//        }
//        else
//        {
//        outer3:
//
//            while ( true )
//            {
//                switch ( data[ _position ] )
//                {
//                    case '\\':
//                        _needsUnescape = true;
//
//                        break;
//
//                    case '/':
//                        if ( ( _position + 1 ) == length )
//                        {
//                            break;
//                        }
//
//                        var c = data[ _position + 1 ];
//
//                        if ( ( c == '/' ) || ( c == '*' ) )
//                        {
//                            goto outer3;
//                        }
//
//                        break;
//
//                    case '}':
//                    case ']':
//                    case ',':
//                    case '\r':
//                    case '\n':
//                        goto outer3;
//                }
//
//                Logger.Debug( $"unquotedChar (value): '{data[ _position ]}'" );
//
//                _position++;
//
//                if ( _position == length )
//                {
//                    break;
//                }
//            }
//        }
//
//        _position--;
//
//        while ( CharacterUtils.IsSpaceChar( data[ _position ] ) )
//        {
//            _position--;
//        }
//    }
//
//    private void QuotedCharsActions( char[] data, int length )
//    {
//        Logger.Debug( "quotedChars" );
//
//        _s             = ++_position;
//        _needsUnescape = false;
//
//    outer4:
//
//        while ( true )
//        {
//            switch ( data[ _position ] )
//            {
//                case '\\':
//                    _needsUnescape = true;
//                    _position++;
//
//                    break;
//
//                case '"':
//                    goto outer4;
//            }
//
//            Logger.Debug( "quotedChar: '" + data[ _position ] + "'" );
//
//            _position++;
//
//            if ( _position == length )
//            {
//                break;
//            }
//        }
//
//        _position--;
//    }
//
//    private void HandleEofActions( char[] data )
//    {
//        int acts  = _jsonEofActions[ _currentState ];
//        var nacts = ( int )_jsonActions[ acts++ ];
//
//        while ( nacts-- > 0 )
//        {
//            switch ( _jsonActions[ acts++ ] )
//            {
//                case 1:
//                    var value = new string( data, _s, _position - _s );
//
//                    if ( _needsUnescape )
//                    {
//                        value = Unescape( value );
//                    }
//
//                outer:
//
//                    if ( _stringIsName )
//                    {
//                        _stringIsName = false;
//
//                        Logger.Debug( $"name: {value}" );
//
//                        _nameList.Add( value );
//                    }
//                    else
//                    {
//                        var name = _nameList.Count > 0 ? _nameList.Pop() : null;
//
//                        if ( _stringIsUnquoted )
//                        {
//                            if ( value.Equals( "true" ) )
//                            {
//                                Logger.Debug( $"bool: {name}=true" );
//
//                                AddBooleanChild( name, true );
//
//                                goto outer;
//                            }
//
//                            if ( value.Equals( "false" ) )
//                            {
//                                Logger.Debug( $"bool: {name}=false" );
//
//                                AddBooleanChild( name, false );
//
//                                goto outer;
//                            }
//
//                            if ( value.Equals( "null" ) )
//                            {
//                                AddStringChild( name, null );
//
//                                goto outer;
//                            }
//
//                            var couldBeDouble = false;
//                            var couldBeLong   = true;
//                        outer2:
//
//                            for ( var i = _s; i < _position; i++ )
//                            {
//                                switch ( data[ i ] )
//                                {
//                                    case '0':
//                                    case '1':
//                                    case '2':
//                                    case '3':
//                                    case '4':
//                                    case '5':
//                                    case '6':
//                                    case '7':
//                                    case '8':
//                                    case '9':
//                                    case '-':
//                                    case '+':
//                                        break;
//
//                                    case '.':
//                                    case 'e':
//                                    case 'E':
//                                        couldBeDouble = true;
//                                        couldBeLong   = false;
//
//                                        break;
//
//                                    default:
//                                        couldBeDouble = false;
//                                        couldBeLong   = false;
//
//                                        goto outer2;
//                                }
//                            }
//
//                            if ( couldBeDouble )
//                            {
//                                try
//                                {
//                                    Logger.Debug( $"double: {name}={double.Parse( value )}" );
//
//                                    AddNumberChild( name, ( double )double.Parse( value ), value );
//
//                                    goto outer;
//                                }
//                                catch ( ArithmeticException )
//                                {
//                                }
//                            }
//                            else if ( couldBeLong )
//                            {
//                                Logger.Debug( $"double: {name}={double.Parse( value )}" );
//
//                                try
//                                {
//                                    AddNumberChild( name, ( double )long.Parse( value ), value );
//
//                                    goto outer;
//                                }
//                                catch ( ArithmeticException )
//                                {
//                                }
//                            }
//                        }
//
//                        Logger.Debug( $"string: {name}={value}" );
//
//                        AddStringChild( name, value );
//                    }
//
//                    _stringIsUnquoted = false;
//                    _s                = _position;
//
//                    break;
//            }
//        }
//    }

    // ========================================================================

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