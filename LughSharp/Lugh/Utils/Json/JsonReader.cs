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

using LughSharp.Lugh.Files;
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

    /// <summary>
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public JsonValue Parse( string json )
    {
        var data = json.ToCharArray();

        return Parse( data, 0, data.Length );
    }

    /// <summary>
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    public JsonValue Parse( TextReader reader )
    {
        var data   = new char[ 1024 ];
        var offset = 0;

        try
        {
            while ( true )
            {
                var length = reader.Read( data, offset, data.Length - offset );

                if ( length == -1 )
                {
                    break;
                }

                if ( length == 0 )
                {
                    var newData = new char[ data.Length * 2 ];

                    Array.Copy( data, 0, newData, 0, data.Length );
                    data = newData;
                }
                else
                {
                    offset += length;
                }
            }
        }
        catch ( IOException ex )
        {
            throw new SerializationException( "Error reading input.", ex );
        }
        finally
        {
            reader.Close();
        }

        return Parse( data, 0, offset );
    }

    /// <summary>
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    public JsonValue Parse( InputStream input )
    {
        StreamReader reader;

        try
        {
            reader = new StreamReader( input, Encoding.UTF8 );
        }
        catch ( Exception ex )
        {
            throw new SerializationException( "Error reading stream.", ex );
        }

        return Parse( reader );
    }

    public JsonValue Parse( FileInfo file )
    {
        StreamReader reader;

        try
        {
            reader = new StreamReader( file.Name, Encoding.UTF8 );
        }
        catch ( Exception ex )
        {
            throw new SerializationException( "Error reading file: " + file, ex );
        }

        try
        {
            return Parse( reader );
        }
        catch ( Exception ex )
        {
            throw new SerializationException( "Error parsing file: " + file, ex );
        }
    }

    public JsonValue Parse( char[] data, int offset, int length )
    {
        var p                = offset;
        var pe               = length;
        var eof              = pe;
        var stack            = new int[ 4 ];
        var s                = 0;
        var nameList         = new List< string >( 8 );
        var needsUnescape    = false;
        var stringIsName     = false;
        var stringIsUnquoted = false;

        GdxRuntimeException parseRuntimeEx = null!;

        Logger.NewLine();

        try
        {
            var cs       = JSON_START;
            var top      = 0;
            var gotoTarg = 0;

        _goto:

            while ( true )
            {
                if ( gotoTarg == 0 )
                {
                    if ( p == pe )
                    {
                        gotoTarg = 4;

                        goto _goto;
                    }

                    if ( cs == 0 )
                    {
                        gotoTarg = 5;

                        goto _goto;
                    }
                }

                if ( gotoTarg == 1 )
                {
                _match:
                    int trans;

                    do
                    {
                        int keys = _jsonKeyOffsets[ cs ];
                        int klen = _jsonSingleLengths[ cs ];
                        trans = _jsonIndexOffsets[ cs ];

                        if ( klen > 0 )
                        {
                            var lower = keys;
                            var upper = ( keys + klen ) - 1;

                            while ( true )
                            {
                                if ( upper < lower )
                                {
                                    break;
                                }

                                var mid = lower + ( ( upper - lower ) >> 1 );

                                if ( data[ p ] < _jsonTransKeys[ mid ] )
                                {
                                    upper = mid - 1;
                                }
                                else if ( data[ p ] > _jsonTransKeys[ mid ] )
                                {
                                    lower = mid + 1;
                                }
                                else
                                {
                                    trans += mid - keys;

                                    goto _match;
                                }
                            }

                            keys  += klen;
                            trans += klen;
                        }

                        klen = _jsonRangeLengths[ cs ];

                        if ( klen > 0 )
                        {
                            var lower = keys;
                            var upper = ( keys + ( klen << 1 ) ) - 2;

                            while ( true )
                            {
                                if ( upper < lower )
                                {
                                    break;
                                }

                                var mid = lower + ( ( ( upper - lower ) >> 1 ) & ~1 );

                                if ( data[ p ] < _jsonTransKeys[ mid ] )
                                {
                                    upper = mid - 2;
                                }
                                else if ( data[ p ] > _jsonTransKeys[ mid + 1 ] )
                                {
                                    lower = mid + 2;
                                }
                                else
                                {
                                    trans += ( mid - keys ) >> 1;

                                    goto _match;
                                }
                            }

                            trans += klen;
                        }
                    }
                    while ( false );

                    trans = _jsonIndicies[ trans ];
                    cs    = _jsonTransTargs[ trans ];

                    if ( _jsonTransActions[ trans ] != 0 )
                    {
                        int acts  = _jsonTransActions[ trans ];
                        var nacts = ( int )_jsonActions[ acts++ ];

                        while ( nacts-- > 0 )
                        {
                            switch ( _jsonActions[ acts++ ] )
                            {
                                case 0:
                                    stringIsName = true;

                                    break;

                                case 1:
                                    var value = new string( data, s, p - s );

                                    if ( needsUnescape )
                                    {
                                        value = Unescape( value );
                                    }

                                outer:

                                    if ( stringIsName )
                                    {
                                        stringIsName = false;

                                        Logger.Debug( $"name: {value}" );

                                        nameList.Add( value );
                                    }
                                    else
                                    {
                                        string name = nameList.size > 0 ? nameList.pop() : null;

                                        if ( stringIsUnquoted )
                                        {
                                            if ( value.Equals( "true" ) )
                                            {
                                                Logger.Debug( "bool: " + name + "=true" );

                                                Boolean( name, true );

                                                goto outer;
                                            }

                                            if ( value.Equals( "false" ) )
                                            {
                                                Logger.Debug( "bool: " + name + "=false" );

                                                Boolean( name, false );

                                                goto outer;
                                            }

                                            if ( value.Equals( "null" ) )
                                            {
                                                String( name, null );

                                                goto outer;
                                            }

                                            bool couldBeDouble = false, couldBeLong = true;
                                        outer2:

                                            for ( var i = s; i < p; i++ )
                                            {
                                                switch ( data[ i ] )
                                                {
                                                    case '0':
                                                    case '1':
                                                    case '2':
                                                    case '3':
                                                    case '4':
                                                    case '5':
                                                    case '6':
                                                    case '7':
                                                    case '8':
                                                    case '9':
                                                    case '-':
                                                    case '+':
                                                        break;

                                                    case '.':
                                                    case 'e':
                                                    case 'E':
                                                        couldBeDouble = true;
                                                        couldBeLong   = false;

                                                        break;

                                                    default:
                                                        couldBeDouble = false;
                                                        couldBeLong   = false;

                                                        goto outer2;
                                                }
                                            }

                                            if ( couldBeDouble )
                                            {
                                                try
                                                {
                                                    Logger.Debug( "double: " + name + "=" + double.Parse( value ) );

                                                    Number( name, ( double )double.Parse( value ), value );

                                                    goto outer;
                                                }
                                                catch ( ArithmeticException ignored )
                                                {
                                                }
                                            }
                                            else if ( couldBeLong )
                                            {
                                                Logger.Debug( "double: " + name + "=" + double.Parse( value ) );

                                                try
                                                {
                                                    Number( name, ( double )long.Parse( value ), value );

                                                    goto outer;
                                                }
                                                catch ( ArithmeticException )
                                                {
                                                }
                                            }
                                        }

                                        Logger.Debug( "string: " + name + "=" + value );

                                        String( name, value );
                                    }

                                    stringIsUnquoted = false;
                                    s                = p;

                                    break;

                                case 2:
                                    var name = nameList.Count > 0 ? nameList.Pop() : null;

                                    Logger.Debug( "startObject: " + name );

                                    StartObject( name );

                                    if ( top == stack.Length )
                                    {
                                        var newStack = new int[ stack.Length * 2 ];

                                        Array.Copy( stack, 0, newStack, 0, stack.Length );
                                        stack = newStack;
                                    }

                                    stack[ top++ ] = cs;
                                    cs             = 5;
                                    gotoTarg       = 2;

                                    goto _goto;

                                case 3:
                                    Logger.Debug( "endObject" );

                                    Pop();

                                    cs       = stack[ --top ];
                                    gotoTarg = 2;

                                    goto _goto;

                                case 4:
                                    var name = nameList.Count > 0 ? nameList.Pop() : null;

                                    Logger.Debug( $"startArray: {name}" );

                                    StartArray( name );

                                    if ( top == stack.Length )
                                    {
                                        var newStack = new int[ stack.Length * 2 ];

                                        Array.Copy( stack, 0, newStack, 0, stack.Length );

                                        stack = newStack;
                                    }

                                    stack[ top++ ] = cs;
                                    cs             = 23;
                                    gotoTarg       = 2;

                                    goto _goto;

                                case 5:
                                    Logger.Debug( "endArray" );

                                    Pop();

                                    cs       = stack[ --top ];
                                    gotoTarg = 2;

                                    goto _goto;

                                case 6:
                                    var start = p - 1;

                                    if ( data[ p++ ] == '/' )
                                    {
                                        while ( ( p != eof ) && ( data[ p ] != '\n' ) )
                                        {
                                            p++;
                                        }

                                        p--;
                                    }
                                    else
                                    {
                                        while ( ( ( ( p + 1 ) < eof ) && ( data[ p ] != '*' ) ) || ( data[ p + 1 ] != '/' ) )
                                        {
                                            p++;
                                        }

                                        p++;
                                    }

                                    Logger.Debug( $"comment {new string( data, start, p - start )}" );

                                    break;

                                case 7:
                                    Logger.Debug( "unquotedChars" );

                                    s                = p;
                                    needsUnescape    = false;
                                    stringIsUnquoted = true;

                                    if ( stringIsName )
                                    {
                                    outer2:

                                        while ( true )
                                        {
                                            switch ( data[ p ] )
                                            {
                                                case '\\':
                                                    needsUnescape = true;

                                                    break;

                                                case '/':
                                                    if ( ( p + 1 ) == eof )
                                                    {
                                                        break;
                                                    }

                                                    var c = data[ p + 1 ];

                                                    if ( ( c == '/' ) || ( c == '*' ) )
                                                    {
                                                        goto outer2;
                                                    }

                                                    break;

                                                case ':':
                                                case '\r':
                                                case '\n':
                                                    goto outer2;
                                            }

                                            Logger.Debug( $"unquotedChar (name): '{data[ p ]}'" );

                                            p++;

                                            if ( p == eof )
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                    outer3:

                                        while ( true )
                                        {
                                            switch ( data[ p ] )
                                            {
                                                case '\\':
                                                    needsUnescape = true;

                                                    break;

                                                case '/':
                                                    if ( ( p + 1 ) == eof )
                                                    {
                                                        break;
                                                    }

                                                    var c = data[ p + 1 ];

                                                    if ( ( c == '/' ) || ( c == '*' ) )
                                                    {
                                                        goto outer3;
                                                    }

                                                    break;

                                                case '}':
                                                case ']':
                                                case ',':
                                                case '\r':
                                                case '\n':
                                                    goto outer3;
                                            }

                                            Logger.Debug( $"unquotedChar (value): '{data[ p ]}'" );

                                            p++;

                                            if ( p == eof )
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    p--;

                                    while ( Character.IsSpace( data[ p ] ) )
                                    {
                                        p--;
                                    }

                                    break;

                                case 8:
                                    if ( debug )
                                    {
                                        System.out.
                                    }

                                    println( "quotedChars" );
                                    s             = ++p;
                                    needsUnescape = false;
                                outer:

                                    while ( true )
                                    {
                                        switch ( data[ p ] )
                                        {
                                            case '\\':
                                                needsUnescape = true;
                                                p++;

                                                break;

                                            case '"':
                                                break outer;
                                        }

                                        // if (debug) System.out.println("quotedChar: '" + data[p] + "'");
                                        p++;

                                        if ( p == eof )
                                        {
                                            break;
                                        }
                                    }

                                    p--;

                                    break;
                            }
                        }
                    }
                }

                if ( gotoTarg == 2 )
                {
                    if ( cs == 0 )
                    {
                        gotoTarg = 5;

                        goto _goto;
                    }

                    if ( ++p != pe )
                    {
                        gotoTarg = 1;

                        goto _goto;
                    }
                }

                if ( gotoTarg == 4 )
                {
                    if ( p == eof )
                    {
                        int acts  = _jsonEofActions[ cs ];
                        var nacts = ( int )_jsonActions[ acts++ ];

                        while ( nacts-- > 0 )
                        {
                            switch ( _jsonActions[ acts++ ] )
                            {
                                case 1:
                                    var value = new string( data, s, p - s );

                                    if ( needsUnescape )
                                    {
                                        value = Unescape( value );
                                    }

                                outer:

                                    if ( stringIsName )
                                    {
                                        stringIsName = false;

                                        Logger.Debug( $"name: {value}" );

                                        nameList.Add( value );
                                    }
                                    else
                                    {
                                        var name = nameList.Count > 0 ? nameList.Pop() : null;

                                        if ( stringIsUnquoted )
                                        {
                                            if ( value.Equals( "true" ) )
                                            {
                                                Logger.Debug( $"bool: {name}=true" );

                                                Boolean( name, true );

                                                goto outer;
                                            }

                                            if ( value.Equals( "false" ) )
                                            {
                                                Logger.Debug( $"bool: {name}=false" );

                                                Boolean( name, false );

                                                goto outer;
                                            }

                                            if ( value.Equals( "null" ) )
                                            {
                                                String( name, null );

                                                goto outer;
                                            }

                                            var couldBeDouble = false;
                                            var couldBeLong   = true;
                                        outer2:

                                            for ( var i = s; i < p; i++ )
                                            {
                                                switch ( data[ i ] )
                                                {
                                                    case '0':
                                                    case '1':
                                                    case '2':
                                                    case '3':
                                                    case '4':
                                                    case '5':
                                                    case '6':
                                                    case '7':
                                                    case '8':
                                                    case '9':
                                                    case '-':
                                                    case '+':
                                                        break;

                                                    case '.':
                                                    case 'e':
                                                    case 'E':
                                                        couldBeDouble = true;
                                                        couldBeLong   = false;

                                                        break;

                                                    default:
                                                        couldBeDouble = false;
                                                        couldBeLong   = false;

                                                        goto outer2;
                                                }
                                            }

                                            if ( couldBeDouble )
                                            {
                                                try
                                                {
                                                    Logger.Debug( $"double: {name}={double.Parse( value )}" );

                                                    Number( name, ( double )double.Parse( value ), value );

                                                    goto outer;
                                                }
                                                catch ( ArithmeticException ignored )
                                                {
                                                }
                                            }
                                            else if ( couldBeLong )
                                            {
                                                Logger.Debug( $"double: {name}={double.Parse( value )}" );

                                                try
                                                {
                                                    Number( name, ( double )long.Parse( value ), value );

                                                    goto outer;
                                                }
                                                catch ( ArithmeticException ignored )
                                                {
                                                }
                                            }
                                        }

                                        Logger.Debug( $"string: {name}={value}" );

                                        String( name, value );
                                    }

                                    stringIsUnquoted = false;
                                    s                = p;

                                    break;
                            }
                        }
                    }
                }

                break;
            }
        }
        catch ( GdxRuntimeException ex )
        {
            parseRuntimeEx = ex;
        }

        var root = _root;
        _root    = null!;
        _current = null!;
        _lastChild.Clear();

        if ( p < pe )
        {
            var lineNumber = 1;

            for ( var i = 0; i < p; i++ )
            {
                if ( data[ i ] == '\n' )
                {
                    lineNumber++;
                }
            }

            var start = Math.Max( 0, p - 32 );

            //TODO: Why??
            throw new SerializationException( $"Error parsing JSON on line {lineNumber} near: " +
                                              $"{new string( data, start, p - start )}*ERROR*" +
                                              $"{new string( data, p, Math.Min( 64, pe - p ) )}",
                                              parseRuntimeEx );
        }

        if ( _elements.Count != 0 )
        {
            var element = _elements.Peek();
            _elements.Clear();

            if ( element.IsObject() )
            {
                throw new SerializationException( "Error parsing JSON, unmatched brace." );
            }

            //TODO: Why??
            throw new SerializationException( "Error parsing JSON, unmatched bracket." );
        }

        if ( parseRuntimeEx != null )
        {
            throw new SerializationException( $"Error parsing JSON: {new string( data )}", parseRuntimeEx );
        }

        return root;
    }

    private void AddChild( string? name, JsonValue child )
    {
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

    protected void StartObject( string? name )
    {
        var value = new JsonValue( JsonValue.ValueTypes.ObjectValue );

        if ( _current != null )
        {
            AddChild( name, value );
        }

        _elements.Add( value );
        _current = value;
    }

    protected void StartArray( string? name )
    {
        var value = new JsonValue( JsonValue.ValueTypes.ArrayValue );

        if ( _current != null )
        {
            AddChild( name, value );
        }

        _elements.Add( value );
        _current = value;
    }

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

    protected void String( string name, string value )
    {
        AddChild( name, new JsonValue( value ) );
    }

    protected void Number( string name, double value, string stringValue )
    {
        AddChild( name, new JsonValue( value, stringValue ) );
    }

    protected void Number( string name, long value, string stringValue )
    {
        AddChild( name, new JsonValue( value, stringValue ) );
    }

    protected void Boolean( string name, bool value )
    {
        AddChild( name, new JsonValue( value ) );
    }

    // ========================================================================

    private string Unescape( string value )
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