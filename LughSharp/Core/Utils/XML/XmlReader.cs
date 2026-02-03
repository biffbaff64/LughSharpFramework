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

using System.Collections;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Utils.XML;

/// <summary>
/// Lightweight XML parser. Supports a subset of XML features: elements, attributes,
/// text, predefined entities, CDATA, mixed content. Namespaces are parsed as part of
/// the element or attribute name. Prologs and doctypes are ignored. Only 8-bit character
/// encodings are supported. Input is assumed to be well formed.
/// <para>
/// The default behavior is to parse the XML into a DOM. Extends this class and override
/// methods to perform event driven parsing. When this is done, the parse methods will
/// return null.
/// </para>
/// </summary>
[PublicAPI]
public class XmlReader
{
    private readonly List< Element > _elements   = new( 8 );
    private readonly StringBuilder   _textBuffer = new( 64 );
    private          Element?        _root;
    private          Element?        _current;
    private          string?         _entitiesText;

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    public Element? Parse( string xml )
    {
        var data = xml.ToCharArray();

        return Parse( data, 0, data.Length );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reader"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    public Element? Parse( TextReader reader )
    {
        try
        {
            var data   = new char[ 1024 ];
            var offset = 0;

            while ( true )
            {
                var length = reader.Read( data, offset, data.Length - offset );

                if ( length == 0 )
                {
                    // Check if we reached end of stream
                    if ( reader.Peek() == -1 )
                    {
                        break;
                    }

                    var newData = new char[ data.Length * 2 ];

                    Array.Copy( data, 0, newData, 0, data.Length );
                    data = newData;
                }
                else
                {
                    offset += length;
                }
            }

            return Parse( data, 0, offset );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex );
        }
        finally
        {
            reader.Dispose();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public Element? Parse( FileInfo file ) => Parse( file.OpenRead() );
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    public Element? Parse( Stream input )
    {
        try
        {
            using ( var reader = new StreamReader( input, Encoding.UTF8 ) )
            {
                return Parse( reader );
            }
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    public Element? Parse( char[] data, int offset, int length )
    {
        int     cs;
        var     p             = offset;
        var     pe            = length;
        var     s             = 0;
        string? attributeName = null;
        var     hasBody       = false;

        cs = XML_START;

        //TODO:
        int klen;
        var trans = 0;
        int acts;
        int nacts;
        int keys;
        var gotoTarg = 0;

        // Ragel State Machine Logic
        while ( true )
        {
            switch ( gotoTarg )
            {
                case 0:
                    if ( p == pe )
                    {
                        gotoTarg = 4;

                        continue;
                    }

                    if ( cs == 0 )
                    {
                        gotoTarg = 5;

                        continue;
                    }

                    goto case 1;

                case 1:
                    keys  = _xmlKeyOffsets[ cs ];
                    trans = _xmlIndexOffsets[ cs ];
                    klen  = _xmlSingleLengths[ cs ];

                    var matched = false;

                    if ( klen > 0 )
                    {
                        var lower = keys;
                        var upper = keys + klen - 1;

                        while ( true )
                        {
                            if ( upper < lower )
                            {
                                break;
                            }

                            var mid = lower + ( ( upper - lower ) >> 1 );

                            if ( data[ p ] < _xmlTransKeys[ mid ] )
                            {
                                upper = mid - 1;
                            }
                            else if ( data[ p ] > _xmlTransKeys[ mid ] )
                            {
                                lower = mid + 1;
                            }
                            else
                            {
                                trans   += ( mid - keys );
                                matched =  true;

                                break;
                            }
                        }

                        if ( !matched )
                        {
                            keys  += klen;
                            trans += klen;
                        }
                    }

                    if ( !matched )
                    {
                        klen = _xmlRangeLengths[ cs ];

                        if ( klen > 0 )
                        {
                            var lower = keys;
                            var upper = keys + ( klen << 1 ) - 2;

                            while ( true )
                            {
                                if ( upper < lower )
                                {
                                    break;
                                }

                                var mid = lower + ( ( ( upper - lower ) >> 1 ) & ~1 );

                                if ( data[ p ] < _xmlTransKeys[ mid ] )
                                {
                                    upper = mid - 2;
                                }
                                else if ( data[ p ] > _xmlTransKeys[ mid + 1 ] )
                                {
                                    lower = mid + 2;
                                }
                                else
                                {
                                    trans   += ( ( mid - keys ) >> 1 );
                                    matched =  true;

                                    break;
                                }
                            }

                            if ( !matched )
                            {
                                trans += klen;
                            }
                        }
                    }

                    trans = _xmlIndicies[ trans ];
                    cs    = _xmlTransTargs[ trans ];

                    if ( _xmlTransActions[ trans ] != 0 )
                    {
                        acts  = _xmlTransActions[ trans ];
                        nacts = _xmlActions[ acts++ ];

                        while ( nacts-- > 0 )
                        {
                            switch ( _xmlActions[ acts++ ] )
                            {
                                case 0:
                                    s = p;

                                    break;

                                case 1:
                                    var c = data[ s ];

                                    if ( c is '?' or '!' )
                                    {
                                        if ( s + 7 < pe && data[ s + 1 ] == '[' && data[ s + 2 ] == 'C'
                                          && data[ s + 3 ] == 'D' && data[ s + 4 ] == 'A' && data[ s + 5 ] == 'T'
                                          && data[ s + 6 ] == 'A' && data[ s + 7 ] == '[' )
                                        {
                                            s += 8;
                                            p =  s + 2;

                                            while ( data[ p - 2 ] != ']' || data[ p - 1 ] != ']' || data[ p ] != '>' )
                                            {
                                                p++;
                                            }

                                            Text( new string( data, s, p - s - 2 ) );
                                        }
                                        else if ( c == '!' && s + 2 < pe && data[ s + 1 ] == '-'
                                               && data[ s + 2 ] == '-' )
                                        {
                                            p = s + 3;

                                            while ( data[ p ] != '-' || data[ p + 1 ] != '-' || data[ p + 2 ] != '>' )
                                            {
                                                p++;
                                            }

                                            p += 2;
                                        }
                                        else
                                        {
                                            while ( data[ p ] != '>' )
                                            {
                                                p++;
                                            }
                                        }

                                        cs       = 15;
                                        gotoTarg = 2;

                                        goto end_switch;
                                    }

                                    hasBody = true;
                                    Open( new string( data, s, p - s ) );

                                    break;

                                case 2:
                                    hasBody = false;
                                    Close();
                                    cs       = 15;
                                    gotoTarg = 2;

                                    goto end_switch;

                                case 3:
                                    Close();
                                    cs       = 15;
                                    gotoTarg = 2;

                                    goto end_switch;

                                case 4:
                                    if ( hasBody )
                                    {
                                        cs       = 15;
                                        gotoTarg = 2;

                                        goto end_switch;
                                    }

                                    break;

                                case 5:
                                    attributeName = new string( data, s, p - s );

                                    break;

                                case 6:
                                    var end = p;

                                    while ( end != s )
                                    {
                                        var last = data[ end - 1 ];

                                        if ( last == ' ' || last == '\t' || last == '\n' || last == '\r' )
                                        {
                                            end--;

                                            continue;
                                        }

                                        break;
                                    }

                                    var currentPos  = s;
                                    var entityFound = false;

                                    while ( currentPos != end )
                                    {
                                        if ( data[ currentPos++ ] != '&' )
                                        {
                                            continue;
                                        }

                                        var entityStart = currentPos;

                                        while ( currentPos != end )
                                        {
                                            if ( data[ currentPos++ ] != ';' )
                                            {
                                                continue;
                                            }

                                            _textBuffer.Append( data, s, entityStart - s - 1 );

                                            var name  = new string( data, entityStart, currentPos - entityStart - 1 );
                                            var value = Entity( name );

                                            _textBuffer.Append( value ?? name );
                                            s           = currentPos;
                                            entityFound = true;

                                            break;
                                        }
                                    }

                                    if ( entityFound )
                                    {
                                        if ( s < end )
                                        {
                                            _textBuffer.Append( data, s, end - s );
                                        }

                                        _entitiesText      = _textBuffer.ToString();
                                        _textBuffer.Length = 0;
                                    }
                                    else
                                    {
                                        _entitiesText = new string( data, s, end - s );
                                    }

                                    break;

                                case 7:
                                    Attribute( attributeName, _entitiesText );

                                    break;

                                case 8:
                                    Text( _entitiesText );

                                    break;
                            }
                        }
                    }

                    goto case 2;

                case 2:
                    if ( cs == 0 )
                    {
                        gotoTarg = 5;

                        continue;
                    }

                    if ( ++p != pe )
                    {
                        gotoTarg = 1;

                        continue;
                    }

                    goto case 4;

                case 4:
                case 5:
                    goto end_loop;
            }

        end_switch: ;
        }

    end_loop:

        _entitiesText = null;

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

            throw new SerializationException( "Error parsing XML on line " + lineNumber + " near: " +
                                              new string( data, p, Math.Min( 32, pe - p ) ) );
        }
        
        if ( _elements.Count != 0 )
        {
            var element = _elements[ _elements.Count - 1 ];
            _elements.Clear();

            throw new SerializationException( "Error parsing XML, unclosed element: " + element.Name );
        }

        var finalRoot = this._root;
        this._root = null;

        return finalRoot;
    }

    // ========================================================================
    
    #region Ragel Data Arrays

    //TODO: Add a brief description / explanation of Ragel data arrays here
    
    private static readonly byte[] _xmlActions =
        { 0, 1, 0, 1, 1, 1, 2, 1, 3, 1, 4, 1, 5, 2, 1, 4, 2, 2, 4, 2, 6, 7, 2, 6, 8, 3, 0, 6, 7 };

    private static readonly byte[] _xmlKeyOffsets =
    {
        0, 0, 4, 9, 14, 20, 26, 30, 35, 36, 37, 42, 46, 50, 51, 52, 56, 57, 62, 67, 73, 79, 83, 88, 89, 90, 95, 99, 103,
        104, 108, 109, 110, 111, 112, 115
    };

    private static readonly char[] _xmlTransKeys =
    {
        ( char )32, ( char )60, ( char )9, ( char )13, ( char )32, ( char )47, ( char )62, ( char )9, ( char )13,
        ( char )32, ( char )47, ( char )62, ( char )9, ( char )13, ( char )32, ( char )47, ( char )61, ( char )62,
        ( char )9, ( char )13, ( char )32, ( char )47, ( char )61, ( char )62, ( char )9, ( char )13, ( char )32,
        ( char )61, ( char )9, ( char )13, ( char )32, ( char )34, ( char )39, ( char )9, ( char )13, ( char )34,
        ( char )34, ( char )32, ( char )47, ( char )62, ( char )9, ( char )13, ( char )32, ( char )62, ( char )9,
        ( char )13, ( char )32, ( char )62, ( char )9, ( char )13, ( char )39, ( char )39, ( char )32, ( char )60,
        ( char )9, ( char )13, ( char )60, ( char )32, ( char )47, ( char )62, ( char )9, ( char )13, ( char )32,
        ( char )47, ( char )62, ( char )9, ( char )13, ( char )32, ( char )47, ( char )61, ( char )62, ( char )9,
        ( char )13, ( char )32, ( char )47, ( char )61, ( char )62, ( char )9, ( char )13, ( char )32, ( char )61,
        ( char )9, ( char )13, ( char )32, ( char )34, ( char )39, ( char )9, ( char )13, ( char )34, ( char )34,
        ( char )32, ( char )47, ( char )62, ( char )9, ( char )13, ( char )32, ( char )62, ( char )9, ( char )13,
        ( char )32, ( char )62, ( char )9, ( char )13, ( char )60, ( char )32, ( char )47, ( char )9, ( char )13,
        ( char )62, ( char )62, ( char )39, ( char )39, ( char )32, ( char )9, ( char )13, ( char )0
    };

    private static readonly byte[] _xmlSingleLengths =
        { 0, 2, 3, 3, 4, 4, 2, 3, 1, 1, 3, 2, 2, 1, 1, 2, 1, 3, 3, 4, 4, 2, 3, 1, 1, 3, 2, 2, 1, 2, 1, 1, 1, 1, 1, 0 };

    private static readonly byte[] _xmlRangeLengths =
        { 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 1, 0 };

    private static readonly short[] _xmlIndexOffsets =
    {
        0, 0, 4, 9, 14, 20, 26, 30, 35, 37, 39, 44, 48, 52, 54, 56, 60, 62, 67, 72, 78, 84, 88, 93, 95, 97, 102, 106,
        110, 112, 116, 118, 120, 122, 124, 127
    };

    private static readonly byte[] _xmlIndicies =
    {
        0, 2, 0, 1, 2, 1, 1, 2, 3, 5, 6, 7, 5, 4, 9, 10, 1, 11, 9, 8, 13, 1, 14, 1, 13, 12, 15, 16, 15, 1, 16, 17, 18,
        16, 1, 20, 19, 22, 21, 9, 10, 11, 9, 1, 23, 24, 23, 1, 25, 11, 25, 1, 20, 26, 22, 27, 29, 30, 29, 28, 32, 31,
        30, 34, 1, 30, 33, 36, 37, 38, 36, 35, 40, 41, 1, 42, 40, 39, 44, 1, 45, 1, 44, 43, 46, 47, 46, 1, 47, 48, 49,
        47, 1, 51, 50, 53, 52, 40, 41, 42, 40, 1, 54, 55, 54, 1, 56, 42, 56, 1, 57, 1, 57, 34, 57, 1, 1, 58, 59, 58, 51,
        60, 53, 61, 62, 62, 1, 1, 0
    };

    private static readonly byte[] _xmlTransTargs =
    {
        1, 0, 2, 3, 3, 4, 11, 34, 5, 4, 11, 34, 5, 6, 7, 6, 7, 8, 13, 9, 10, 9, 10, 12, 34, 12, 14, 14, 16, 15, 17, 16,
        17, 18, 30, 18, 19, 26, 28, 20, 19, 26, 28, 20, 21, 22, 21, 22, 23, 32, 24, 25, 24, 25, 27, 28, 27, 29, 31, 35,
        33, 33, 34
    };

    private static readonly byte[] _xmlTransActions =
    {
        0, 0, 0, 1, 0, 3, 3, 13, 1, 0, 0, 9, 0, 11, 11, 0, 0, 0, 0, 1, 25, 0, 19, 5, 16, 0, 1, 0, 1, 0, 0, 0, 22, 1, 0,
        0, 3, 3, 13, 1, 0, 0, 9, 0, 11, 11, 0, 0, 0, 0, 1, 25, 0, 19, 5, 16, 0, 0, 0, 7, 1, 0, 0
    };

    private const int XML_START = 1;

    #endregion

    // ========================================================================
    
    protected virtual void Open( string name )
    {
        var child = new Element( name, _current );

        _current?.AddChild( child );

        _elements.Add( child );
        _current = child;
    }

    protected virtual void Attribute( string? name, string? value )
    {
        _current?.SetAttribute( name, value );
    }

    protected virtual string? Entity( string name )
    {
        if ( name == "lt" )
        {
            return "<";
        }

        if ( name == "gt" )
        {
            return ">";
        }

        if ( name == "amp" )
        {
            return "&";
        }

        if ( name == "apos" )
        {
            return "'";
        }

        if ( name == "quot" )
        {
            return "\"";
        }

        return name.StartsWith( "#x" )
            ? ( ( char )Convert.ToInt32( name.Substring( 2 ), 16 ) ).ToString()
            : null;
    }

    protected virtual void Text( string? text )
    {
        var existing = _current?.Text;
        _current?.Text = ( existing != null ? existing + text : text );
    }

    protected virtual void Close()
    {
        _root = _elements[ _elements.Count - 1 ];
        _elements.RemoveAt( _elements.Count - 1 );
        _current = _elements.Count > 0 ? _elements[ _elements.Count - 1 ] : null;
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Element : IEnumerable
    {
        public string  Name { get; }
        public string? Text { get; set; }

        // ====================================================================
        
        private Dictionary< string, string? >? _attributes;
        private List< Element? >?              _children;
        private Element                        _parent;

        // ====================================================================

        public Element( string name, Element? parent )
        {
            Guard.Against.Null( parent );
            
            this.Name    = name;
            this._parent = parent;
        }

        /// <summary>
        /// Returns the attribute with the given name, or null if it doesn't exist. An exception is
        /// thrown if the attribute doesn't exist.
        /// </summary>
        /// <param name="name"> The name of the requested attribute. </param>
        /// <exception cref="Exception"> If the attribute doesn't exist. </exception>
        public string? GetAttribute( string name )
        {
            if ( _attributes == null || !_attributes.TryGetValue( name, out var value ) )
            {
                throw new Exception( $"Element {this.Name} doesn't have attribute: {name}" );
            }

            return value;
        }

        /// <summary>
        /// Returns the attribute with the given name, or the given default value if it doesn't exist.'
        /// </summary>
        /// <param name="name"> The name of the requested attribute. </param>
        /// <param name="defaultValue"> The default value to return if the attribute doesn't exist. </param>
        /// <returns></returns>
        public string? GetAttribute( string name, string? defaultValue )
        {
            if ( _attributes == null )
            {
                return defaultValue;
            }

            var value = _attributes[ name ];
            
            return value ?? defaultValue;
        }

        public bool HasAttribute( string name )
        {
            return _attributes?.ContainsKey( name ) ?? false;
        }
        
        public void SetAttribute( string name, string value )
        {
            _attributes ??= new Dictionary< string, string? >();

            _attributes[ name ] = value;
        }

        public int GetChildCount()
        {
            return _children?.Count ?? 0;
        }

        public Element? GetChild( int index )
        {
            return _children == null
                ? throw new Exception( "This element has no children" )
                : _children[ index ];
        }

        public Element? GetChildByName( string name )
        {
            return _children?.FirstOrDefault( e => e.Name == name );
        }

        public Element? GetChildByNameRecursive( string name )
        {
            return _children?.Select( e => e.GetChildByNameRecursive( name ) )
                            .FirstOrDefault( e => e != null )
                ?? throw new Exception( $"This element has no child named: {name}" );
        }

        public bool HasChild( string name )
        {
            return _children?.Any( e => e.Name == name ) ?? false;
        }

        public bool HasChildRecursive( string name )
        {
            if ( _children == null )
            {
                return false;
            }
            
            return GetChildByNameRecursive( name ) != null;
        }

        public List< Element? > GetChildrenByName( string name )
        {
            return _children?.Where( e => e?.Name == name )
                            .ToList() ?? new List< Element? >( 0 );
        }

        public List< Element? > GetChildrenByNameRecursive( string name )
        {
            return _children?.SelectMany( e => e?.GetChildrenByNameRecursive( name )! )
                            .ToList() ?? new List< Element? >( 0 );
        }
        
        public void AddChild( Element element )
        {
            _children ??= new List< Element? >( 8 );

            _children.Add( element );
        }

        public void RemoveChild( int index )
        {
            _children?.RemoveAt( index );
        }
        
        public void RemoveChild( Element? element )
        {
            _children?.Remove( element );
        }

        public void Remove()
        {
            _parent?.RemoveChild( this );
        }
        
        // Helper for parsing numeric attributes
        public float GetFloatAttribute( string name, float defaultValue = 0 )
        {
            return float.TryParse( GetAttribute( name, "" ), out var res )
                ? res
                : defaultValue;
        }

        public int GetIntAttribute( string name, int defaultValue = 0 )
        {
            return int.TryParse( GetAttribute( name, "" ), out var res )
                ? res
                : defaultValue;
        }

        public string? Get( string name, string? defaultValue = "" )
        {
            string? value;
            
            if ( _attributes != null )
            {
                value = _attributes[ name ];

                if ( value != null )
                {
                    return value;
                }
            }
            
            var child = GetChildByName( name );

            if ( child == null )
            {
                return defaultValue;
            }
            
            value = child.Text;

            if ( value == null )
            {
                return defaultValue;
            }
            
            return value;
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be
        /// used to iterate through the collection.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            return _children?.GetEnumerator() ?? Enumerable.Empty< Element? >().GetEnumerator();
        }

        public override string ToString() => ToString( "" );

        public string ToString( string indent )
        {
            var sb = new StringBuilder();
            sb.Append( indent ).Append( '<' ).Append( Name );

            if ( _attributes != null )
            {
                foreach ( var entry in _attributes )
                {
                    sb.Append( ' ' ).Append( entry.Key ).Append( "=\"" ).Append( entry.Value ).Append( '\"' );
                }
            }

            if ( _children == null && string.IsNullOrEmpty( Text ) )
            {
                sb.Append( "/>" );
            }
            else
            {
                sb.Append( ">\n" );
                var childIndent = indent + "\t";

                if ( !string.IsNullOrEmpty( Text ) )
                {
                    sb.Append( childIndent ).Append( Text ).Append( '\n' );
                }

                if ( _children != null )
                {
                    foreach ( var child in _children )
                    {
                        sb.Append( child.ToString( childIndent ) ).Append( '\n' );
                    }
                }

                sb.Append( indent ).Append( "</" ).Append( Name ).Append( '>' );
            }

            return sb.ToString();
        }
    }
}

// ============================================================================
// ============================================================================