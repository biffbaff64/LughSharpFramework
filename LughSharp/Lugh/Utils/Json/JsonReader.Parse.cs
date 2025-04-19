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
using LughSharp.Lugh.Graphics.Text;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Utils.Json;

public partial class JsonReader
{
    private char[]               _parseData     = null!;
    private int[]                _parseStack    = new int[ 4 ];
    private int                  _stackPointer  = 0;
    private List< string >       _parseNameList = new( 8 );
    private int                  _parsePosition;
    private int                  _parseLength;
    private bool                 _needsUnescape;
    private bool                 _stringIsName;
    private bool                 _stringIsUnquoted;
    private GdxRuntimeException? _parseException;
    private int                  _stringValueStartPos; // Start position for string values

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public JsonValue? Parse( string json )
    {
        var data = json.ToCharArray();

        return Parse( data, 0, data.Length );
    }

    /// <summary>
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    public JsonValue? Parse( TextReader reader )
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
    public JsonValue? Parse( InputStream input )
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

    public JsonValue? Parse( FileInfo file )
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

    public JsonValue? Parse( char[] data, int offset, int length )
    {
        _parseData     = data;
        _parsePosition = offset;
        _parseLength   = length;

        return ParseInternal();
    }

    private JsonValue? ParseInternal()
    {
        var currentState = JSON_START;

        _stackPointer     = 0;
        _needsUnescape    = false;
        _stringIsName     = false;
        _stringIsUnquoted = false;
        _parseException   = null!;
        _parsePosition    = 0; // Reset parse position

        _parseNameList.Clear();

        try
        {
            while ( _parsePosition < _parseLength )
            {
                currentState = ProcessState( currentState );

                if ( _parseException != null )
                {
                    break;
                }
            }

            // Handle EOF actions if no error occurred
            if ( _parseException == null )
            {
                HandleEofActions( currentState );
            }
        }
        catch ( GdxRuntimeException ex )
        {
            _parseException = ex;
        }

        return FinalizeParsing( currentState );
    }

    private int ProcessState( int currentState )
    {
        if ( currentState == 0 )
        {
            return 0; // End state
        }

        var transition = FindTransition( currentState );

        if ( transition != -1 )
        {
            ExecuteActions( _jsonTransitionActions[ transition ] );
            
            if ( _parseException != null )
            {
                return currentState; // Stop if error
            }
            
            _parsePosition++;
            
            return _jsonTransitionTargs[ transition ];
        }

        if ( _parsePosition <  _parseLength )
        {
            // No valid transition found for the current character
            // This should ideally lead to a parsing error
            _parseException = new GdxRuntimeException( $"Unexpected character '{_parseData[ _parsePosition ]}' " +
                                                       $"at position {_parsePosition}" );
        }
        
        // Stay in the current state
        return currentState;
    }

    private int FindTransition( int currentState )
    {
        int keys       = _jsonKeyOffsets[ currentState ];
        int keylength  = _jsonSingleLengths[ currentState ];
        int transition = _jsonIndexOffsets[ currentState ];

        char currentChar;
            
        if ( ( _parsePosition < _parseLength ) && ( _parsePosition < _parseData.Length ) )
        {
            currentChar = _parseData[ _parsePosition ];
        }
        else
        {
            currentChar = ( char )0;
        }

        // Single character transitions
        if ( keylength > 0 )
        {
            var lower = keys;
            var upper = ( keys + keylength ) - 1;

            while ( lower <= upper )
            {
                var middle = lower + ( ( upper - lower ) >> 1 );

                if ( currentChar < _jsonTransitionKeys[ middle ] )
                {
                    upper = middle - 1;
                }
                else if ( currentChar > _jsonTransitionKeys[ middle ] )
                {
                    lower = middle + 1;
                }
                else
                {
                    return transition + ( middle - keys );
                }
            }

            transition += keylength;
        }

        // Range transitions
        keylength = _jsonRangeLengths[ currentState ];

        if ( keylength > 0 )
        {
            var lower = keys;
            var upper = ( keys + ( keylength << 1 ) ) - 2;

            while ( lower <= upper )
            {
                var mid = lower + ( ( ( upper - lower ) >> 1 ) & ~1 );

                if ( currentChar < _jsonTransitionKeys[ mid ] )
                {
                    upper = mid - 2;
                }
                else if ( currentChar > _jsonTransitionKeys[ mid + 1 ] )
                {
                    lower = mid + 2;
                }
                else
                {
                    return transition + ( ( mid - keys ) >> 1 );
                }
            }

            transition += keylength;
        }

        Logger.Checkpoint();
        
        return ( ( _parsePosition == _parseLength ) && ( _jsonEofActions[ currentState ] != 0 ) )
            ? _jsonEofActions[ currentState ]
            : -1; // No transition
    }

    private JsonValue? FinalizeParsing( int currentState )
    {
        var root = _root;

        _root    = null!;
        _current = null!;

        _lastChild.Clear();

        if ( _parsePosition < _parseLength )
        {
            var lineNumber = _parseData.Take( _parsePosition ).Count( c => c == '\n' ) + 1;
            var start      = Math.Max( 0, _parsePosition - 32 );

            throw new SerializationException( $"Error parsing JSON on line {lineNumber} near: " +
                                              $"{new string( _parseData, start, Math.Min( 32, _parsePosition - start ) )}" +
                                              $"*ERROR*{new string( _parseData, _parsePosition, Math.Min( 64, _parseLength - _parsePosition ) )}",
                                              _parseException! );
        }

        if ( _elements.Count != 0 )
        {
            var element = _elements.Peek();
            _elements.Clear();

            throw new SerializationException( $"Error parsing JSON, unmatched " +
                                              $"{( element.IsObject() ? "brace" : "bracket" )}." );
        }

        if ( _parseException != null )
        {
            throw new SerializationException( $"Error parsing JSON: {new string( _parseData )}", _parseException );
        }

        return root;
    }

    private void ExecuteActions( int actionOffset )
    {
        if ( actionOffset == 0 )
        {
            return;
        }

        var acts  = actionOffset;
        var nacts = ( int )_jsonActions[ acts++ ];

        while ( nacts-- > 0 )
        {
            switch ( _jsonActions[ acts++ ] )
            {
                case 0:
                    _stringIsName = true;

                    break;

                case 1:
                    HandleStringValue();

                    break;

                case 2:
                    StartObjectAction();

                    break;

                case 3:
                    EndObjectAction();

                    break;

                case 4:
                    StartArrayAction();

                    break;

                case 5:
                    EndArrayAction();

                    break;

                case 6:
                    SkipComment();

                    break;

                case 7:
                    HandleUnquotedChars();

                    break;

                case 8:
                    HandleQuotedChars();

                    break;
            }
        }
    }

    private void HandleStringValue()
    {
        var value = new string( _parseData, _stringValueStartPos, _parsePosition - _stringValueStartPos );

        if ( _needsUnescape )
        {
            value = Unescape( value );
        }

        if ( _stringIsName )
        {
            _stringIsName = false;
            Logger.Debug( $"name: {value}" );

            _parseNameList.Add( value );
        }
        else
        {
            var name = _parseNameList.Count > 0 ? _parseNameList.Pop() : null;

            if ( _stringIsUnquoted )
            {
                switch ( value )
                {
                    case "true":
                        Logger.Debug( $"bool: {name}=true" );

                        AddBooleanChild( name, true );

                        break;

                    case "false":
                        Logger.Debug( $"bool: {name}=false" );

                        AddBooleanChild( name, false );

                        break;

                    case "null":
                        AddStringChild( name, null );

                        break;

                    default:
                    {
                        if ( !TryParseNumber( name, value ) )
                        {
                            Logger.Debug( $"string: {name}={value}" );

                            AddStringChild( name, value );
                        }

                        break;
                    }
                }
            }
            else
            {
                Logger.Debug( $"string: {name}={value}" );

                AddStringChild( name, value );
            }
        }

        _stringIsUnquoted    = false;
        _stringValueStartPos = _parsePosition;
    }

    private bool TryParseNumber( string? name, string value )
    {
        bool couldBeDouble = false, couldBeLong = true;

        for ( var i = _stringValueStartPos; i < _parsePosition; i++ )
        {
            switch ( _parseData[ i ] )
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

        if ( couldBeDouble && double.TryParse( value, out var doubleValue ) )
        {
            Logger.Debug( $"double: {name}={doubleValue}" );

            AddNumberChild( name, doubleValue, value );

            return true;
        }

        if ( couldBeLong && long.TryParse( value, out var longValue ) )
        {
            Logger.Debug( $"long: {name}={longValue}" );

            AddNumberChild( name, ( double )longValue, value );

            return true;
        }

        return false;
    }

    private void StartObjectAction()
    {
        Logger.Checkpoint();

        var name = _parseNameList.Count > 0 ? _parseNameList.Pop() : null;

        Logger.Debug( $"startObject: {name}" );

        StartObject( name );
        PushState( 5 );
    }

    private void EndObjectAction()
    {
        Logger.Debug( "endObject" );

        PopState();
    }

    private void StartArrayAction()
    {
        var name = _parseNameList.Count > 0 ? _parseNameList.Pop() : null;

        Logger.Debug( $"startArray: {name}" );

        StartArray( name );
        PushState( 23 );
    }

    private void EndArrayAction()
    {
        Logger.Debug( "endArray" );

        PopState();
    }

    private void PushState( int state )
    {
        if ( _stackPointer == _parseStack.Length )
        {
            Array.Resize( ref _parseStack, _parseStack.Length * 2 );
        }

        _parseStack[ _stackPointer++ ] = state;
    }

    private void PopState()
    {
        if ( _stackPointer > 0 )
        {
            _stackPointer--;
        }
    }

    private void SkipComment()
    {
        var start = _parsePosition - 1;

        if ( ( _parsePosition < _parseLength ) && ( _parseData[ _parsePosition ] == '/' ) )
        {
            _parsePosition++;

            while ( ( _parsePosition < _parseLength ) && ( _parseData[ _parsePosition ] != '\n' ) )
            {
                _parsePosition++;
            }

            if ( _parsePosition < _parseLength )
            {
                _parsePosition--; // Move back to the newline
            }
        }
        else if ( ( _parsePosition < _parseLength ) && ( _parseData[ _parsePosition ] == '*' ) )
        {
            _parsePosition++;

            while ( ( ( _parsePosition + 1 ) < _parseLength ) &&
                    ( ( _parseData[ _parsePosition ] != '*' ) || ( _parseData[ _parsePosition + 1 ] != '/' ) ) )
            {
                _parsePosition++;
            }

            _parsePosition += 2; // Consume '*/'
        }

        Logger.Debug( $"comment {new string( _parseData, start,
                                             Math.Min( ( _parsePosition - start ) + 1, _parseLength - start ) )}" );
    }

    private void HandleUnquotedChars()
    {
        Logger.Debug( "unquotedChars" );

        _stringValueStartPos = _parsePosition;
        _needsUnescape       = false;
        _stringIsUnquoted    = true;
        var shouldBreak = false;

        if ( _stringIsName )
        {
            while ( ( _parsePosition < _parseLength ) && !shouldBreak )
            {
                var currentChar = _parseData[ _parsePosition ];

                switch ( currentChar )
                {
                    case '\\':
                        _needsUnescape = true;

                        break;

                    case '/':
                        if ( ( ( _parsePosition + 1 ) < _parseLength ) &&
                             ( ( _parseData[ _parsePosition + 1 ] == '/' )
                               || ( _parseData[ _parsePosition + 1 ] == '*' ) ) )
                        {
                            shouldBreak = true;
                        }

                        break;

                    case ':':
                    case '\r':
                    case '\n':
                        shouldBreak = true;

                        break;
                }

                if ( !shouldBreak )
                {
                    Logger.Debug( $"unquotedChar (name): '{currentChar}'" );

                    _parsePosition++;
                }
            }
        }
        else
        {
            while ( ( _parsePosition < _parseLength ) && !shouldBreak )
            {
                var currentChar = _parseData[ _parsePosition ];

                switch ( currentChar )
                {
                    case '\\':
                        _needsUnescape = true;

                        break;

                    case '/':
                        if ( ( ( _parsePosition + 1 ) < _parseLength ) &&
                             ( ( _parseData[ _parsePosition + 1 ] == '/' )
                               || ( _parseData[ _parsePosition + 1 ] == '*' ) ) )
                        {
                            shouldBreak = true;
                        }

                        break;

                    case '}':
                    case ']':
                    case ',':
                    case '\r':
                    case '\n':
                        shouldBreak = true;

                        break;
                }

                if ( !shouldBreak )
                {
                    Logger.Debug( $"unquotedChar (value): '{currentChar}'" );

                    _parsePosition++;
                }
            }
        }

        _parsePosition--;

        while ( ( _parsePosition >= 0 ) && CharacterUtils.IsSpaceChar( _parseData[ _parsePosition ] ) )
        {
            _parsePosition--;
        }

        _parsePosition++;

        Logger.Debug( "Done" );
    }

    private void HandleQuotedChars()
    {
        Logger.Debug( "quotedChars" );

        _stringValueStartPos = ++_parsePosition;
        _needsUnescape       = false;

        while ( _parsePosition < _parseLength )
        {
            var currentChar = _parseData[ _parsePosition ];

            if ( currentChar == '"' )
            {
                _parsePosition++; // Consume the closing quote

                break;
            }

            if ( currentChar == '\\' )
            {
                _needsUnescape = true;
                _parsePosition++; // Skip the escape character

                if ( _parsePosition < _parseLength )
                {
                    Logger.Debug( $"escapedChar: '{_parseData[ _parsePosition ]}'" );

                    _parsePosition++; // Skip the escaped character
                }
            }
            else
            {
                Logger.Debug( $"quotedChar: '{currentChar}'" );

                _parsePosition++;
            }
        }

        _parsePosition--;

        Logger.Debug( "Done" );
    }

    private void HandleEofActions( int currentState )
    {
        Logger.Debug( "Eof Actions" );

        int acts  = _jsonEofActions[ currentState ];
        var nacts = ( int )_jsonActions[ acts++ ];

        while ( nacts-- > 0 )
        {
            if ( _jsonActions[ acts++ ] == 1 )
            {
                HandleStringValue(); // Reuse the existing string value handler
            }

            // Add handling for other EOF actions if they exist
        }

        Logger.Debug( "Done" );
    }
}