// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024 Richard Ikin.
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

using System.Buffers;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Utils.Json;

/// <summary>
/// Lightweight JSON parser.
///
/// The default behavior is to parse the JSON into a DOM containing JsonValue objects. Extend this
/// class and override methods to perform event driven parsing. When this is done, the parse methods
/// will return null.
/// </summary>
[PublicAPI]
public class JsonReader
{
    public bool IsStopped { get; set; }

    // ========================================================================

    private const int JsonStart      = 1;
    private const int JsonFirstFinal = 35;
    private const int JsonError      = 0;
    private const int JsonEnObject   = 5;
    private const int JsonEnArray    = 23;
    private const int JsonEnMain     = 1;

    private readonly List< JsonValue? > _elements = new( 8 );
    private          JsonValue?         _root;
    private          JsonValue?         _current;

    // ========================================================================

    /// <summary>
    /// Parses the given JSON string, returning a <c>JsonValue</c> object representing
    /// the JSON.
    /// </summary>
    public JsonValue? Parse( string json )
    {
        char[] data = json.ToCharArray();

        return Parse( data, 0, data.Length );
    }

    /// <summary>
    /// Parses the given input stream, returning a <c>JsonValue</c> object representing
    /// the JSON obtained from the stream.
    /// </summary>
    public JsonValue? Parse( Stream input )
    {
        try
        {
            using ( var reader = new StreamReader( input ) )
            {
                return Parse( reader );
            }
        }
        catch ( IOException ex )
        {
            throw new SerializationException( "Error reading input.", ex );
        }
    }

    /// <summary>
    /// Parses the JSON obtained from the given <c>FileInfo</c> instance, returning a
    /// <c>JsonValue</c> object representing the JSON obtained from the stream.
    /// </summary>
    public JsonValue? Parse( FileInfo file )
    {
        try
        {
            using ( StreamReader reader = file.OpenText() )
            {
                return Parse( reader );
            }
        }
        catch ( IOException ex )
        {
            throw new SerializationException( "Error reading input.", ex );
        }
    }

    /// <summary>
    /// Parses the input from the given <c>StreamReader</c>, returning a <c>JsonValue</c>
    /// object representing the JSON obtained from the stream.
    /// </summary>
    public JsonValue? Parse( StreamReader reader )
    {
        try
        {
            using ( reader )
            {
                string content = reader.ReadToEnd();

                return Parse( content.ToCharArray(), 0, content.Length );
            }
        }
        catch ( IOException ex )
        {
            throw new SerializationException( "Error reading input.", ex );
        }
    }

    /// <summary>
    /// Parses the given char data array, returning a <c>JsonValue</c> object representing JSON.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    public JsonValue? Parse( char[] data, int offset, int length )
    {
        IsStopped = false;

        int     p                = offset;
        var     stack            = new int[ 4 ];
        var     s                = 0;
        string? name             = null;
        var     needsUnescape    = false;
        var     stringIsName     = false;
        var     stringIsUnquoted = false;

        RuntimeException? parseRuntimeEx = null;

        try
        {
            int cs  = JsonStart;
            var top = 0;

            {
                var gotoTarg = 0;

                while ( true )
                {
                    switch ( gotoTarg )
                    {
                        case 0:
                            if ( p == length )
                            {
                                gotoTarg = 4;

                                goto _goto;
                            }

                            if ( cs == 0 )
                            {
                                gotoTarg = 5;

                                goto _goto;
                            }

                            break;

                        case 1:

                            int trans;

                            do
                            {
                                int keys = _jsonKeyOffsets[ cs ];
                                int klen = _jsonSingleLengths[ cs ];
                                trans = _jsonIndexOffsets[ cs ];

                                if ( klen > 0 )
                                {
                                    int lower = keys;
                                    int upper = keys + klen - 1;

                                    while ( true )
                                    {
                                        if ( upper < lower )
                                        {
                                            break;
                                        }

                                        int mid = lower + ( ( upper - lower ) >> 1 );

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
                                            trans += ( mid - keys );

                                            goto _match;
                                        }
                                    }

                                    keys  += klen;
                                    trans += klen;
                                }

                                klen = _jsonRangeLengths[ cs ];

                                if ( klen > 0 )
                                {
                                    int lower = keys;
                                    int mid;
                                    int upper = keys + ( klen << 1 ) - 2;

                                    while ( true )
                                    {
                                        if ( upper < lower )
                                        {
                                            break;
                                        }

                                        mid = lower + ( ( ( upper - lower ) >> 1 ) & ~1 );

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
                                            trans += ( ( mid - keys ) >> 1 );

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
                                int nacts = _jsonActions[ acts++ ];

                                while ( nacts-- > 0 )
                                {
                                    switch ( _jsonActions[ acts++ ] )
                                    {
                                        // ------------------------------------
                                        case 0:
                                            stringIsName = true;

                                            break;

                                        // ------------------------------------
                                        case 1:
                                            var value = new string( data, s, p - s );

                                            if ( needsUnescape )
                                            {
                                                value = Unescape( value );
                                            }

                                            if ( stringIsName )
                                            {
                                                stringIsName = false;
                                                name         = value;
                                            }
                                            else
                                            {
                                                string? valueName = name;
                                                name = null;

                                                if ( stringIsUnquoted )
                                                {
                                                    if ( value.Equals( "true" ) )
                                                    {
                                                        HandleBool( valueName, true );

                                                        goto outer1;
                                                    }

                                                    if ( value.Equals( "false" ) )
                                                    {
                                                        HandleBool( valueName, false );

                                                        goto outer1;
                                                    }

                                                    if ( value.Equals( "null" ) )
                                                    {
                                                        HandleString( valueName, null );

                                                        goto outer1;
                                                    }

                                                    bool couldBeDouble = false, couldBeLong = true;

                                                    for ( int i = s; i < p; i++ )
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

                                                                break;
                                                        }
                                                    }

                                                    if ( couldBeDouble )
                                                    {
                                                        try
                                                        {
                                                            HandleNumber( valueName, double.Parse( value ), value );

                                                            goto outer1;
                                                        }
                                                        catch ( FormatException )
                                                        {
                                                            // Ignored
                                                        }
                                                    }
                                                    else if ( couldBeLong )
                                                    {
                                                        try
                                                        {
                                                            HandleNumber( valueName, long.Parse( value ), value );

                                                            goto outer1;
                                                        }
                                                        catch ( FormatException )
                                                        {
                                                            // Ignored
                                                        }
                                                    }
                                                }

                                                HandleString( valueName, value );
                                            }

                                        outer1:

                                            if ( IsStopped )
                                            {
                                                goto _goto;
                                            }

                                            stringIsUnquoted = false;
                                            s                = p;

                                            break;

                                        // ------------------------------------
                                        case 2:
                                            StartObject( name );

                                            if ( IsStopped )
                                            {
                                                goto _goto;
                                            }

                                            name = null;

                                            if ( top == stack.Length )
                                            {
                                                int[] newStack = ArrayPool< int >.Shared.Rent( stack.Length * 2 );

                                                Array.Copy( stack, 0, newStack, 0, stack.Length );
                                                ArrayPool< int >.Shared.Return( stack );
                                                stack = newStack;
                                            }

                                            stack[ top++ ] = cs;
                                            cs             = 5;
                                            gotoTarg       = 2;

                                            if ( true )
                                            {
                                                goto _goto;
                                            }

                                            break;

                                        // ------------------------------------
                                        case 3:
                                            Pop();

                                            if ( IsStopped )
                                            {
                                                goto _goto;
                                            }

                                            cs       = stack[ --top ];
                                            gotoTarg = 2;

                                            if ( true )
                                            {
                                                goto _goto;
                                            }

                                            break;

                                        // ------------------------------------
                                        case 4:
                                            StartArray( name );

                                            if ( IsStopped )
                                            {
                                                goto _goto;
                                            }

                                            name = null;

                                            if ( top == stack.Length )
                                            {
                                                int[] newStack = ArrayPool< int >.Shared.Rent( stack.Length * 2 );

                                                Array.Copy( stack, 0, newStack, 0, stack.Length );
                                                ArrayPool< int >.Shared.Return( stack );
                                                stack = newStack;
                                            }

                                            stack[ top++ ] = cs;
                                            cs             = 23;
                                            gotoTarg       = 2;

                                            if ( true )
                                            {
                                                goto _goto;
                                            }

                                            break;

                                        // ------------------------------------
                                        case 5:
                                            Pop();

                                            if ( IsStopped )
                                            {
                                                goto _goto;
                                            }

                                            cs       = stack[ --top ];
                                            gotoTarg = 2;

                                            if ( true )
                                            {
                                                goto _goto;
                                            }

                                            break;

                                        // ------------------------------------
                                        case 6:
                                            int start = p - 1;

                                            if ( data[ p++ ] == '/' )
                                            {
                                                while ( p != length && data[ p ] != '\n' )
                                                {
                                                    p++;
                                                }

                                                p--;
                                            }
                                            else
                                            {
                                                while ( p + 1 < length && ( data[ p ] != '*' || data[ p + 1 ] != '/' ) )
                                                {
                                                    p++;
                                                }

                                                p++;
                                            }

                                            break;

                                        // ------------------------------------
                                        case 7:
                                            s                = p;
                                            needsUnescape    = false;
                                            stringIsUnquoted = true;

                                            if ( stringIsName )
                                            {
                                                while ( true )
                                                {
                                                    switch ( data[ p ] )
                                                    {
                                                        case '\\':
                                                            needsUnescape = true;

                                                            break;

                                                        case '/':
                                                            if ( p + 1 == length )
                                                            {
                                                                break;
                                                            }

                                                            char c = data[ p + 1 ];

                                                            if ( c == '/' || c == '*' )
                                                            {
                                                                goto outer7;
                                                            }

                                                            break;

                                                        case ':':
                                                        case '\r':
                                                        case '\n':
                                                            goto outer7;
                                                    }

                                                    p++;

                                                    if ( p == length )
                                                    {
                                                        break;
                                                    }
                                                }

                                            outer7: ;
                                            }
                                            else
                                            {
                                                while ( true )
                                                {
                                                    switch ( data[ p ] )
                                                    {
                                                        case '\\':
                                                            needsUnescape = true;

                                                            break;

                                                        case '/':
                                                            if ( p + 1 == length )
                                                            {
                                                                break;
                                                            }

                                                            char c = data[ p + 1 ];

                                                            if ( c == '/' || c == '*' )
                                                            {
                                                                goto outer7b;
                                                            }

                                                            break;

                                                        case '}':
                                                        case ']':
                                                        case ',':
                                                        case '\r':
                                                        case '\n':
                                                            goto outer7b;
                                                    }

                                                    p++;

                                                    if ( p == length )
                                                    {
                                                        break;
                                                    }
                                                }

                                            outer7b: ;
                                            }

                                            p--;

                                            while ( char.IsWhiteSpace( data[ p ] ) )
                                            {
                                                p--;
                                            }

                                            break;

                                        // ------------------------------------
                                        case 8:
                                            s             = ++p;
                                            needsUnescape = false;

                                            while ( true )
                                            {
                                                switch ( data[ p ] )
                                                {
                                                    case '\\':
                                                        needsUnescape = true;
                                                        p++;

                                                        break;

                                                    case '"':
                                                        goto outer8;
                                                }

                                                p++;

                                                if ( p == length )
                                                {
                                                    break;
                                                }
                                            }

                                        outer8:

                                            p--;

                                            break;
                                    }
                                }
                            }

                        _match: ;

                            break;

                        case 2:
                            if ( cs == 0 )
                            {
                                gotoTarg = 5;

                                goto _goto;
                            }

                            if ( ++p != length )
                            {
                                gotoTarg = 1;

                                goto _goto;
                            }

                            break;

                        case 4:
                            if ( p == length )
                            {
                                int acts2  = _jsonEofActions[ cs ];
                                var nacts2 = ( int )_jsonActions[ acts2++ ];

                                while ( nacts2-- > 0 )
                                {
                                    switch ( _jsonActions[ acts2++ ] )
                                    {
                                        case 1:
                                            var value = new string( data, s, p - s );

                                            if ( needsUnescape )
                                            {
                                                value = Unescape( value );
                                            }

                                            if ( stringIsName )
                                            {
                                                stringIsName = false;
                                                name         = value;
                                            }
                                            else
                                            {
                                                string? valueName = name;
                                                name = null;

                                                if ( stringIsUnquoted )
                                                {
                                                    if ( value.Equals( "true" ) )
                                                    {
                                                        HandleBool( valueName, true );

                                                        goto outer41;
                                                    }

                                                    if ( value.Equals( "false" ) )
                                                    {
                                                        HandleBool( valueName, false );

                                                        goto outer41;
                                                    }

                                                    if ( value.Equals( "null" ) )
                                                    {
                                                        HandleString( valueName, null );

                                                        goto outer41;
                                                    }

                                                    var couldBeDouble = false;
                                                    var couldBeLong   = true;

                                                    for ( int i = s; i < p; i++ )
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

                                                                break;
                                                        }
                                                    }

                                                    if ( couldBeDouble )
                                                    {
                                                        try
                                                        {
                                                            HandleNumber( valueName, double.Parse( value ), value );

                                                            goto outer41;
                                                        }
                                                        catch ( FormatException )
                                                        {
                                                        }
                                                    }
                                                    else if ( couldBeLong )
                                                    {
                                                        try
                                                        {
                                                            HandleNumber( valueName, long.Parse( value ), value );

                                                            goto outer41;
                                                        }
                                                        catch ( FormatException )
                                                        {
                                                        }
                                                    }
                                                }

                                                HandleString( valueName, value );
                                            }

                                        outer41:

                                            if ( IsStopped )
                                            {
                                                goto _goto;
                                            }

                                            stringIsUnquoted = false;
                                            s                = p;

                                            break;
                                    }
                                }
                            }

                            break;
                    }

                    break;
                }

            _goto: ;
            }
        }
        catch ( RuntimeException ex )
        {
            parseRuntimeEx = ex;
        }

        ( JsonValue? root, this._root ) = ( this._root, null );

        _current = null;

        if ( !IsStopped )
        {
            if ( p < length )
            {
                int lineNumber = 1 + data.Count( c => c == '\n' );
                int start      = Math.Max( 0, p - 32 );

                throw new SerializationException( $"Error parsing JSON on line {lineNumber} "
                                                + $"near: {new string( data, start, p - start )}"
                                                + $"*ERROR*{new string( data, p, Math.Min( 64, length - p ) )}",
                                                  parseRuntimeEx );
            }

            if ( _elements.Count != 0 )
            {
                JsonValue? element = _elements.Peek();
                _elements.Clear();

                if ( element != null && element.IsObject() )
                {
                    throw new SerializationException( "Error parsing JSON, unmatched brace." );
                }
                else
                {
                    throw new SerializationException( "Error parsing JSON, unmatched bracket." );
                }
            }

            if ( parseRuntimeEx != null )
            {
                throw new SerializationException( $"Error parsing JSON: {new string( data )}", parseRuntimeEx );
            }
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
            _current.AddChild( child );
        }
        else
        {
            _root = _current;
        }
    }

    /// <summary>
    /// Called when an object is encountered in the JSON.
    /// </summary>
    protected void StartObject( string? name )
    {
        var value = new JsonValue( JsonValue.ValueType.ObjectType );

        if ( _current != null )
        {
            AddChild( name, value );
        }

        _elements.Add( value );
        _current = value;
    }

    /// <summary>
    /// Called when an array is encountered in the JSON.
    /// </summary>
    protected void StartArray( string? name )
    {
        var value = new JsonValue( JsonValue.ValueType.ArrayType );

        if ( _current != null )
        {
            AddChild( name, value );
        }

        _elements.Add( value );
        _current = value;
    }

    /// <summary>
    /// Called when the end of an object or array is encountered in the JSON.
    /// </summary>
    protected void Pop()
    {
        _root    = _elements.Pop();
        _current = _elements.Count > 0 ? _elements.Peek() : null;
    }

    /// <summary>
    /// Called when a string or null value is encountered in the JSON.
    /// </summary>
    protected void HandleString( string? name, string? value )
    {
        AddChild( name, new JsonValue( value ) );
    }

    /// <summary>
    /// Called when a double value is encountered in the JSON.
    /// </summary>
    protected void HandleNumber( string? name, double value, string stringValue )
    {
        AddChild( name, new JsonValue( value, stringValue ) );
    }

    /// <summary>
    /// Called when a long value is encountered in the JSON.
    /// </summary>
    protected void HandleNumber( string? name, long value, string stringValue )
    {
        AddChild( name, new JsonValue( value, stringValue ) );
    }

    /// <summary>
    /// Called when a boolean value is encountered in the JSON.
    /// </summary>
    protected void HandleBool( string? name, bool value )
    {
        AddChild( name, new JsonValue( value ) );
    }

    /// <summary>
    /// Called to unescape string values. The default implementation does standard JSON unescaping.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    protected string Unescape( string value )
    {
        int length = value.Length;
        var buffer = new StringBuilder( length + 16 );

        for ( var i = 0; i < length; )
        {
            char c = value[ i++ ];

            if ( c != '\\' )
            {
                buffer.Append( c );

                continue;
            }

            if ( i == length )
            {
                break;
            }

            c = value[ i++ ];

            if ( c == 'u' )
            {
                ReadOnlySpan< char > hexSpan = value.AsSpan( i, 4 );

                if ( int.TryParse( hexSpan, System.Globalization.NumberStyles.HexNumber, null, out int code ) )
                {
                    buffer.Append( ( char )code );
                    i += 4;
                }

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
                    throw new SerializationException( "Illegal escaped character: \\" + c );
            }

            buffer.Append( c );
        }

        return buffer.ToString();
    }

    // ========================================================================
    // ========================================================================

    private static byte[] InitializeJsonActions0()
    {
        return new byte[]
        {
            0, 1, 1, 1, 2, 1, 3, 1, 4, 1, 5, 1, 6, 1, 7,
            1, 8, 2, 0, 7, 2, 0, 8, 2, 1, 3, 2, 1, 5
        };
    }

    private static readonly byte[] _jsonActions = InitializeJsonActions0();

    // ----------------------------------------------------

    private static short[] init__json_key_offsets_0()
    {
        return new short[]
        {
            0, 0, 11, 13, 14, 16, 25, 31, 37, 39, 50, 57, 64, 73, 74, 83,
            85, 87, 96, 98, 100, 101, 103, 105, 116, 123, 130, 141, 142,
            153, 155, 157, 168, 170, 172, 174, 179, 184, 184
        };
    }

    private static readonly short[] _jsonKeyOffsets = init__json_key_offsets_0();

    // ----------------------------------------------------

    private static char[] InitializeJsonTransKeys0()
    {
        return new[]
        {
            ( char )13, ( char )32, ( char )34, ( char )44, ( char )47, ( char )58, ( char )91, ( char )93, ( char )123,
            ( char )9,
            ( char )10, ( char )42, ( char )47, ( char )34, ( char )42, ( char )47, ( char )13, ( char )32, ( char )34,
            ( char )44,
            ( char )47, ( char )58, ( char )125, ( char )9, ( char )10, ( char )13, ( char )32, ( char )47, ( char )58,
            ( char )9,
            ( char )10, ( char )13, ( char )32, ( char )47, ( char )58, ( char )9, ( char )10, ( char )42, ( char )47,
            ( char )13,
            ( char )32, ( char )34, ( char )44, ( char )47, ( char )58, ( char )91, ( char )93, ( char )123, ( char )9,
            ( char )10,
            ( char )9, ( char )10, ( char )13, ( char )32, ( char )44, ( char )47, ( char )125, ( char )9, ( char )10,
            ( char )13,
            ( char )32, ( char )44, ( char )47, ( char )125, ( char )13, ( char )32, ( char )34, ( char )44, ( char )47,
            ( char )58,
            ( char )125, ( char )9, ( char )10, ( char )34, ( char )13, ( char )32, ( char )34, ( char )44, ( char )47,
            ( char )58,
            ( char )125, ( char )9, ( char )10, ( char )42, ( char )47, ( char )42, ( char )47, ( char )13, ( char )32,
            ( char )34,
            ( char )44, ( char )47, ( char )58, ( char )125, ( char )9, ( char )10, ( char )42, ( char )47, ( char )42,
            ( char )47,
            ( char )34, ( char )42, ( char )47, ( char )42, ( char )47, ( char )13, ( char )32, ( char )34, ( char )44,
            ( char )47,
            ( char )58, ( char )91, ( char )93, ( char )123, ( char )9, ( char )10, ( char )9, ( char )10, ( char )13,
            ( char )32,
            ( char )44, ( char )47, ( char )93, ( char )9, ( char )10, ( char )13, ( char )32, ( char )44, ( char )47,
            ( char )93,
            ( char )13, ( char )32, ( char )34, ( char )44, ( char )47, ( char )58, ( char )91, ( char )93, ( char )123,
            ( char )9,
            ( char )10, ( char )34, ( char )13, ( char )32, ( char )34, ( char )44, ( char )47, ( char )58, ( char )91,
            ( char )93,
            ( char )123, ( char )9, ( char )10, ( char )42, ( char )47, ( char )42, ( char )47, ( char )13, ( char )32,
            ( char )34,
            ( char )44, ( char )47, ( char )58, ( char )91, ( char )93, ( char )123, ( char )9, ( char )10, ( char )42,
            ( char )47,
            ( char )42, ( char )47, ( char )42, ( char )47, ( char )13, ( char )32, ( char )47, ( char )9, ( char )10,
            ( char )13,
            ( char )32, ( char )47, ( char )9, ( char )10, ( char )0
        };
    }

    private static readonly char[] _jsonTransKeys = InitializeJsonTransKeys0();

    // ----------------------------------------------------

    private static byte[] InitializeJsonSingleLengths0()
    {
        return new byte[]
        {
            0, 9, 2, 1, 2, 7, 4, 4, 2, 9, 7, 7, 7, 1, 7, 2, 2, 7, 2, 2,
            1, 2, 2, 9, 7, 7, 9, 1, 9, 2, 2, 9, 2, 2, 2, 3, 3, 0, 0
        };
    }

    private static readonly byte[] _jsonSingleLengths = InitializeJsonSingleLengths0();

    // ----------------------------------------------------

    private static byte[] InitializeJsonRangeLengths0()
    {
        return new byte[]
        {
            0, 1, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0,
            0, 0, 0, 1, 0, 0, 1, 0, 1, 0, 0, 1, 0, 0, 0, 1, 1, 0, 0
        };
    }

    private static readonly byte[] _jsonRangeLengths = InitializeJsonRangeLengths0();

    // ----------------------------------------------------

    private static short[] InitializeJsonIndexOffsets0()
    {
        return new short[]
        {
            0, 0, 11, 14, 16, 19, 28, 34, 40, 43, 54, 62, 70, 79, 81, 90, 93, 96, 105, 108, 111, 113,
            116, 119, 130, 138, 146, 157, 159, 170, 173, 176, 187, 190, 193, 196, 201, 206, 207
        };
    }

    private static readonly short[] _jsonIndexOffsets = InitializeJsonIndexOffsets0();

    private static byte[] InitializeJsonIndicies0()
    {
        return new byte[]
        {
            1, 1, 2, 3, 4, 3, 5, 3, 6, 1, 0, 7, 7, 3, 8, 3, 9, 9, 3, 11,
            11, 12, 13, 14, 3, 15, 11, 10, 16, 16, 17, 18, 16, 3, 19, 19,
            20, 21, 19, 3, 22, 22, 3, 21, 21, 24, 3, 25, 3, 26, 3, 27, 21,
            23, 28, 29, 29, 28, 30, 31, 32, 3, 33, 34, 34, 33, 13, 35, 15,
            3, 34, 34, 12, 36, 37, 3, 15, 34, 10, 16, 3, 36, 36, 12, 3, 38,
            3, 3, 36, 10, 39, 39, 3, 40, 40, 3, 13, 13, 12, 3, 41, 3, 15,
            13, 10, 42, 42, 3, 43, 43, 3, 28, 3, 44, 44, 3, 45, 45, 3, 47,
            47, 48, 49, 50, 3, 51, 52, 53, 47, 46, 54, 55, 55, 54, 56, 57,
            58, 3, 59, 60, 60, 59, 49, 61, 52, 3, 60, 60, 48, 62, 63, 3,
            51, 52, 53, 60, 46, 54, 3, 62, 62, 48, 3, 64, 3, 51, 3, 53, 62,
            46, 65, 65, 3, 66, 66, 3, 49, 49, 48, 3, 67, 3, 51, 52, 53, 49,
            46, 68, 68, 3, 69, 69, 3, 70, 70, 3, 8, 8, 71, 8, 3, 72, 72, 73,
            72, 3, 3, 3, 0
        };
    }

    private static readonly byte[] _jsonIndicies = InitializeJsonIndicies0();

    // ----------------------------------------------------

    private static byte[] InitializeJsonTransitionTargets0()
    {
        return new byte[]
        {
            35, 1, 3, 0, 4, 36, 36, 36, 36, 1, 6, 5, 13, 17, 22, 37, 7, 8, 9,
            7, 8, 9, 7, 10, 20, 21, 11, 11, 11, 12, 17, 19, 37, 11, 12, 19, 14,
            16, 15, 14, 12, 18, 17, 11, 9, 5, 24, 23, 27, 31, 34, 25, 38, 25,
            25, 26, 31, 33, 38, 25, 26, 33, 28, 30, 29, 28, 26, 32, 31, 25,
            23, 2, 36, 2
        };
    }

    private static readonly byte[] _jsonTransTargs = InitializeJsonTransitionTargets0();

    // ----------------------------------------------------

    private static byte[] InitializeJsonTransitionActions0()
    {
        return new byte[]
        {
            13, 0, 15, 0, 0, 7, 3, 11, 1, 11, 17, 0, 20, 0, 0, 5, 1, 1, 1,
            0, 0, 0, 11, 13, 15, 0, 7, 3, 1, 1, 1, 1, 23, 0, 0, 0, 0, 0, 0,
            11, 11, 0, 11, 11, 11, 11, 13, 0, 15, 0, 0, 7, 9, 3, 1, 1, 1,
            1, 26, 0, 0, 0, 0, 0, 0, 11, 11, 0, 11, 11, 11, 1, 0, 0
        };
    }

    private static readonly byte[] _jsonTransActions = InitializeJsonTransitionActions0();

    // ----------------------------------------------------

    private static byte[] InitJsonEofActions0()
    {
        return new byte[]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0
        };
    }

    private static readonly byte[] _jsonEofActions = InitJsonEofActions0();
}

// ============================================================================
// ============================================================================