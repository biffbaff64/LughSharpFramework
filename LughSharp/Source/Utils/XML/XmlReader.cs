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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Source.Utils.Exceptions;

namespace LughSharp.Source.Utils.XML;

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
    /// Parses the specified XML string and returns the root element of the parsed document.
    /// </summary>
    /// <param name="xml">A string containing the XML to parse. Cannot be null.</param>
    /// <returns>
    /// The root <see cref="Element"/> of the parsed XML document, or <c>null</c>
    /// if the input is empty or invalid.
    /// </returns>
    public Element? Parse( string xml )
    {
        char[] data = xml.ToCharArray();

        return Parse( data, 0, data.Length );
    }

    /// <summary>
    /// Parses an element from the specified text reader.
    /// <para>
    /// The method disposes the provided <paramref name="reader"/> after parsing
    /// is complete. The caller should not use the reader after calling this method.
    /// </para>
    /// </summary>
    /// <param name="reader">
    /// The text reader that provides the input data to parse. Cannot be null and must
    /// be positioned at the start of the element to parse.
    /// </param>
    /// <returns>
    /// An <see cref="Element"/> representing the parsed data, or <c>null</c> if no
    /// element could be parsed.
    /// </returns>
    /// <exception cref="SerializationException">
    /// Thrown when an I/O error occurs while reading from the text reader.
    /// </exception>
    public Element? Parse( TextReader reader )
    {
        try
        {
            var data   = new char[ 1024 ];
            var offset = 0;

            while ( true )
            {
                int length = reader.Read( data, offset, data.Length - offset );

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
    /// Parses the specified file and returns the corresponding element, or null
    /// if the file is null.
    /// </summary>
    /// <param name="file">The file to parse. If null, the method returns null.</param>
    /// <returns>
    /// An <see cref="Element"/> representing the parsed contents of the file, or
    /// null if <paramref name="file"/> is null.
    /// </returns>
    public Element? Parse( FileInfo? file )
    {
        if ( file == null )
        {
            return null;
        }

        StreamReader reader = file.OpenText();

        return Parse( reader );
    }

    /// <summary>
    /// Parses the input stream and returns the corresponding element, or null
    /// if parsing fails.
    /// </summary>
    /// <param name="input">
    /// The input stream containing the data to parse. The stream must be readable
    /// and positioned at the start of the data to be parsed.
    /// </param>
    /// <returns>
    /// An <see cref="Element"/> representing the parsed data, or <c>null</c> if
    /// the input does not contain a valid element.
    /// </returns>
    /// <exception cref="SerializationException">
    /// Thrown when an I/O error occurs while reading from the input stream.
    /// </exception>
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
    /// Parses a segment of a character array as XML and returns the root element of
    /// the parsed structure.
    /// </summary>
    /// <param name="data">The character array containing the XML data to parse.</param>
    /// <param name="offset">The zero-based index in the array at which to begin parsing.</param>
    /// <param name="length">
    /// The number of characters to parse from the array, starting at the specified offset.
    /// </param>
    /// <returns>
    /// The root <see cref="Element"/> of the parsed XML structure, or <c>null</c> if the
    /// input does not contain a valid root element.
    /// </returns>
    /// <exception cref="SerializationException">
    /// Thrown if the XML data is malformed, contains unclosed elements, or cannot be parsed
    /// successfully.
    /// </exception>
    /// <remarks>
    /// 15.03.2026<br/>
    /// This method is not particularly pretty, I'm happy to admit that. I'm loath to touch
    /// it because it works. I WILL get around to addressing it one day but, until then,
    /// there are more important things to do in this framework!
    /// </remarks>
    public Element? Parse( char[] data, int offset, int length )
    {
        int     cs;
        int     p             = offset;
        int     pe            = length;
        var     s             = 0;
        string? attributeName = null;
        var     hasBody       = false;

        cs = XmlStart;

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
                        int lower = keys;
                        int upper = keys + klen - 1;

                        while ( true )
                        {
                            if ( upper < lower )
                            {
                                break;
                            }

                            int mid = lower + ( ( upper - lower ) >> 1 );

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
                                trans   += mid - keys;
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
                            int lower = keys;
                            int upper = keys + ( klen << 1 ) - 2;

                            while ( true )
                            {
                                if ( upper < lower )
                                {
                                    break;
                                }

                                int mid = lower + ( ( ( upper - lower ) >> 1 ) & ~1 );

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
                                    trans   += ( mid - keys ) >> 1;
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
                                    char c = data[ s ];

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
                                    int end = p;

                                    while ( end != s )
                                    {
                                        char last = data[ end - 1 ];

                                        if ( last == ' ' || last == '\t' || last == '\n' || last == '\r' )
                                        {
                                            end--;

                                            continue;
                                        }

                                        break;
                                    }

                                    int currentPos  = s;
                                    var entityFound = false;

                                    while ( currentPos != end )
                                    {
                                        if ( data[ currentPos++ ] != '&' )
                                        {
                                            continue;
                                        }

                                        int entityStart = currentPos;

                                        while ( currentPos != end )
                                        {
                                            if ( data[ currentPos++ ] != ';' )
                                            {
                                                continue;
                                            }

                                            _textBuffer.Append( data, s, entityStart - s - 1 );

                                            var name = new string( data, entityStart, currentPos - entityStart - 1 );
                                            string? value = Entity( name );

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
            Element element = _elements[ _elements.Count - 1 ];
            _elements.Clear();

            throw new SerializationException( "Error parsing XML, unclosed element: " + element.Name );
        }

        Element? finalRoot = _root;
        _root = null;

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

    private const int XmlStart = 1;

    #endregion

    // ========================================================================

    /// <summary>
    /// Opens a new element with the specified name and adds it as a child of the current element.
    /// </summary>
    /// <param name="name">The name of the element to open. Cannot be null or empty.</param>
    protected virtual void Open( string name )
    {
        var child = new Element( name, _current );

        _current?.AddChild( child );

        _elements.Add( child );
        _current = child;
    }

    /// <summary>
    /// Sets an attribute with the specified name and value on the current element.
    /// </summary>
    /// <param name="name">The name of the attribute to set. Cannot be null.</param>
    /// <param name="value">The value to assign to the attribute. Cannot be null.</param>
    /// <exception cref="RuntimeException">Thrown if either the attribute name or value is null.</exception>
    protected virtual void Attribute( string? name, string? value )
    {
        if ( name == null || value == null )
        {
            throw new RuntimeException( $"Invalid attribute name or value: {name}, {value}" );
        }

        _current?.SetAttribute( name, value );
    }

    /// <summary>
    /// Resolves a named or numeric XML entity to its corresponding character representation.
    /// <para>
    /// Supports standard XML entities as well as hexadecimal numeric character references.
    /// Returns null for unrecognized entity names.
    /// </para>
    /// </summary>
    /// <param name="name">
    /// The name of the XML entity to resolve. This can be a standard entity name such as
    /// "lt", "gt", "amp", "apos", or "quot", or a numeric character reference in the form
    /// "#xNNNN" (hexadecimal).
    /// </param>
    /// <returns>
    /// A string containing the character represented by the specified entity name, or null
    /// if the entity name is not recognized.
    /// </returns>
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

    /// <summary>
    /// Appends the specified text to the current element's text content.
    /// </summary>
    /// <param name="text">
    /// The text to append to the current element. Can be null, in which case no text
    /// is added.
    /// </param>
    protected virtual void Text( string? text )
    {
        string? existing = _current?.Text;
        _current?.Text = existing != null ? existing + text : text;
    }

    /// <summary>
    /// Closes the current element and updates the internal state to reflect the
    /// parent element.
    /// <para>
    /// This method is intended to be overridden in derived classes to provide custom
    /// behavior when closing an element. After calling this method, the current element
    /// is set to the previous element in the stack, or null if no elements remain.
    /// </para>
    /// </summary>
    protected virtual void Close()
    {
        _root = _elements[ _elements.Count - 1 ];
        _elements.RemoveAt( _elements.Count - 1 );
        _current = _elements.Count > 0 ? _elements[ _elements.Count - 1 ] : null;
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Represents a hierarchical element with a name, optional text content, attributes,
    /// and child elements.
    /// <para>
    /// The Element class provides methods for managing attributes and child elements,
    /// supporting hierarchical data structures similar to XML or HTML. Elements can be
    /// nested, queried by name, and traversed recursively.
    /// <para>
    /// <b>
    /// This class is not thread-safe; concurrent access should be synchronized externally
    /// if needed.
    /// </b>
    /// </para>
    /// </para>
    /// </summary>
    [PublicAPI]
    public class Element : IEnumerable
    {
        public string  Name { get; }
        public string? Text { get; set; }

        // ====================================================================

        private Dictionary< string, string? >? _attributes;
        private List< Element? >?              _children;
        private Element?                       _parent;

        // ====================================================================

        /// <summary>
        /// Initializes a new instance of the Element class with the specified name and
        /// optional parent element.
        /// </summary>
        /// <param name="name">The name of the element. <b>Cannot be null.</b></param>
        /// <param name="parent">
        /// The parent element of this element, or null if the element has no parent.
        /// </param>
        public Element( string name, Element? parent )
        {
            Name    = name;
            _parent = parent;
        }

        /// <summary>
        /// Returns the attribute with the given name, or null if it doesn't exist.
        /// An exception is thrown if the attribute doesn't exist.
        /// </summary>
        /// <param name="name"> The name of the requested attribute. </param>
        /// <exception cref="Exception"> If the attribute doesn't exist. </exception>
        public string? GetAttribute( string name )
        {
            if ( _attributes == null || !_attributes.TryGetValue( name, out string? value ) )
            {
                throw new Exception( $"Element {Name} doesn't have attribute: {name}" );
            }

            return value;
        }

        /// <summary>
        /// Returns the attribute with the given name, or the given default value if
        /// it doesn't exist.'
        /// </summary>
        /// <param name="name"> The name of the requested attribute. </param>
        /// <param name="defaultValue">
        /// The default value to return if the attribute doesn't exist.
        /// </param>
        /// <returns>
        /// A string representation of the attribute, or the provided default value.
        /// </returns>
        public string? GetAttribute( string name, string? defaultValue )
        {
            if ( _attributes == null )
            {
                return defaultValue;
            }

            string? value = defaultValue;

            if ( _attributes.TryGetValue( name, out string? attribute ) )
            {
                value = attribute;
            }

            return value;
        }

        /// <summary>
        /// Sets the attribute with the given name to the given value.
        /// </summary>
        /// <param name="name"> The attribute name to set. </param>
        /// <param name="value"> The attribute value to set. </param>
        public void SetAttribute( string name, string value )
        {
            _attributes ??= new Dictionary< string, string? >();

            _attributes[ name ] = value;
        }

        /// <summary>
        /// Retrieves the child element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the child element to retrieve.</param>
        /// <returns>
        /// The child element at the specified index, or null if the element at that
        /// index is null.
        /// </returns>
        /// <exception cref="Exception">Thrown if this element has no children.</exception>
        public Element? GetChild( int index )
        {
            return _children == null
                ? throw new Exception( "This element has no children" )
                : _children[ index ];
        }

        /// <summary>
        /// Retrieves the first child element with the specified name.
        /// </summary>
        /// <param name="name">
        /// The name of the child element to locate. The comparison is case-sensitive.
        /// </param>
        /// <returns>
        /// The first child element whose name matches the specified value, or null if
        /// no such child exists.
        /// </returns>
        public Element? GetChildByName( string name )
        {
            if ( _children == null )
            {
                return null;
            }

            foreach ( Element? e in _children )
            {
                if ( e?.Name == name )
                {
                    return e;
                }
            }

            return null;
        }

        /// <summary>
        /// Searches for a child element with the specified name, recursively traversing
        /// all descendant elements.
        /// <para>
        /// The search includes all direct and indirect children of the current element. If
        /// multiple elements share the same name, the first one encountered in a depth-first
        /// traversal is returned.
        /// </para>
        /// </summary>
        /// <param name="name">
        /// The name of the child element to search for. The comparison is case-sensitive.
        /// </param>
        /// <returns>
        /// The first child element with the specified name, or null if no such element is found.
        /// </returns>
        public Element? GetChildByNameRecursive( string name )
        {
            if ( _children == null )
            {
                return null;
            }

            foreach ( Element? element in _children )
            {
                if ( element == null )
                {
                    continue;
                }

                if ( element.Name.Equals( name ) )
                {
                    return element;
                }

                Element? found = element.GetChildByNameRecursive( name );

                if ( found != null )
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether a child element with the specified name exists.
        /// </summary>
        /// <param name="name">
        /// The name of the child element to locate. <b>Cannot be null.</b>
        /// </param>
        /// <returns>
        /// <c>true</c> if a child element with the specified name exists; otherwise <c>false.</c>
        /// </returns>
        public bool HasChild( string name )
        {
            if ( _children == null )
            {
                return false;
            }

            return GetChildByName( name ) != null;
        }

        /// <summary>
        /// Determines whether a child with the specified name exists in the current node or
        /// any of its descendants.
        /// </summary>
        /// <param name="name">
        /// The name of the child to search for. The comparison may be case-sensitive depending
        /// on implementation.
        /// </param>
        /// <returns>
        /// true if a child with the specified name exists in the current node or any descendant;
        /// otherwise, false.
        /// </returns>
        public bool HasChildRecursive( string name )
        {
            if ( _children == null )
            {
                return false;
            }

            return GetChildByNameRecursive( name ) != null;
        }

        /// <summary>
        /// Retrieves a list of child elements whose names match the specified value.
        /// </summary>
        /// <param name="name">
        /// The name to match against the child elements' names. The comparison is case-sensitive.
        /// </param>
        /// <returns>
        /// A list of child elements with names equal to the specified value. The list is empty
        /// if no matching children are found.
        /// </returns>
        public List< Element? > GetChildrenByName( string name )
        {
            var result = new List< Element? >();

            if ( _children == null )
            {
                return result;
            }

            foreach ( Element? child in _children )
            {
                if ( child?.Name != null )
                {
                    if ( child.Name.Equals( name ) )
                    {
                        result.Add( child );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Retrieves all descendant elements with the specified name, searching recursively
        /// through the element hierarchy.
        /// </summary>
        /// <param name="name">
        /// The name of the elements to search for. The comparison may be case-sensitive
        /// depending on implementation.
        /// </param>
        /// <returns>
        /// A list of elements that match the specified name. The list is empty if no matching
        /// elements are found.
        /// </returns>
        public List< Element? > GetChildrenByNameRecursive( string name )
        {
            var result = new List< Element? >();

            GetChildrenByNameRecursive( name, result );

            return result;
        }

        /// <summary>
        /// Recursively searches for all child elements with the specified name and adds
        /// them to the provided result list.
        /// <para>
        /// This method traverses the entire hierarchy of child elements, adding each element
        /// whose name matches the specified value to the result list. The search includes all
        /// descendants, not just immediate children.
        /// </para>
        /// </summary>
        /// <param name="name">
        /// The name of the child elements to search for. Comparison is case-sensitive.
        /// </param>
        /// <param name="result">
        /// A list to which matching child elements will be added. <b>Must not be null.</b>
        /// </param>
        private void GetChildrenByNameRecursive( string name, List< Element? > result )
        {
            if ( _children == null )
            {
                return;
            }

            foreach ( Element? child in _children )
            {
                if ( child?.Name != null )
                {
                    if ( child.Name.Equals( name ) )
                    {
                        result.Add( child );
                    }
                }

                child?.GetChildrenByNameRecursive( name, result );
            }
        }

        /// <summary>
        /// Adds the specified child element to the current element's collection of children.
        /// </summary>
        /// <param name="element">
        /// The child <see cref="Element"/> to add. <b>Cannot be null.</b>
        /// </param>
        public void AddChild( Element element )
        {
            _children ??= new List< Element? >( 8 );

            _children.Add( element );
        }

        /// <summary>
        /// Retrieves the value of the specified attribute as a floating-point number.
        /// </summary>
        /// <param name="name">
        /// The name of the attribute to retrieve. <b>Cannot be null.</b>
        /// </param>
        /// <param name="defaultValue">
        /// The value to return if the attribute is not found or cannot be parsed as a float.
        /// </param>
        /// <returns>
        /// The floating-point value of the specified attribute, or the specified default
        /// value if the attribute is missing or not a valid float.
        /// </returns>
        public float GetFloatAttribute( string name, float defaultValue = 0 )
        {
            return float.TryParse( GetAttribute( name, "" ), out float res )
                ? res
                : defaultValue;
        }

        /// <summary>
        /// Gets the value of the specified attribute as an integer, or returns a default
        /// value if the attribute is missing or cannot be parsed.
        /// </summary>
        /// <param name="name">The name of the attribute to retrieve.</param>
        /// <param name="defaultValue">
        /// The value to return if the attribute is not found or cannot be converted to an
        /// integer. The default is 0.
        /// </param>
        /// <returns>
        /// The integer value of the specified attribute, or the specified default value if
        /// the attribute is missing or not a valid integer.
        /// </returns>
        public int GetIntAttribute( string name, int defaultValue = 0 )
        {
            return int.TryParse( GetAttribute( name, "" ), out int res )
                ? res
                : defaultValue;
        }

        /// <summary>
        /// Retrieves the value of the specified attribute as an unsigned integer.
        /// </summary>
        /// <param name="name">
        /// The name of the attribute to retrieve. <b>Cannot be null or empty.</b>
        /// </param>
        /// <param name="defaultValue">
        /// The value to return if the attribute is not found or cannot be parsed as an
        /// unsigned integer. The default is 0.
        /// </param>
        /// <returns>
        /// The value of the attribute converted to an unsigned integer, or the specified
        /// default value if the attribute is missing or not a valid unsigned integer.
        /// </returns>
        public uint GetUIntAttribute( string name, uint defaultValue = 0 )
        {
            return uint.TryParse( GetAttribute( name, "" ), out uint res )
                ? res
                : defaultValue;
        }

        /// <summary>
        /// Retrieves the value of the specified attribute or child element by name, returning
        /// a default value if not found.
        /// <para>
        /// If an attribute with the specified name exists, its value is returned. If not, the
        /// method searches for a child element with the given name and returns its text content.
        /// If neither is found, the specified default value is returned. Attribute values take
        /// precedence over child element values.
        /// </para>
        /// </summary>
        /// <param name="name">
        /// The name of the attribute or child element to retrieve the value for.
        /// <b>Cannot be null.</b>
        /// </param>
        /// <param name="defaultValue">
        /// The value to return if the specified attribute or child element is not found.
        /// <b>Can be null.</b>
        /// </param>
        /// <returns>
        /// The value of the specified attribute or child element if found; otherwise, the value
        /// of <paramref name="defaultValue"/>.
        /// </returns>
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

            Element? child = GetChildByName( name );

            if ( child == null )
            {
                return defaultValue;
            }

            value = child.Text;

            return value ?? defaultValue;
        }

        /// <summary>
        /// Returns true if the element has an attribute with the given name.
        /// </summary>
        public bool HasAttribute( string name )
        {
            return _attributes?.ContainsKey( name ) ?? false;
        }

        /// <summary>
        /// Removes the child element at the specified index from the collection of children.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the child element to remove. Must be within the valid
        /// range of the collection.
        /// </param>
        public void RemoveChild( int index )
        {
            _children?.RemoveAt( index );
        }

        /// <summary>
        /// Removes the specified child element from the collection of children.
        /// </summary>
        /// <param name="element">
        /// The child element to remove. If the element is not present in the collection,
        /// no action is taken. Can be null.
        /// </param>
        public void RemoveChild( Element? element )
        {
            _children?.Remove( element );
        }

        /// <summary>
        /// Removes this element from its parent collection or container.
        /// </summary>
        /// <para>If the element does not have a parent, this method has no effect.</para>
        public void Remove()
        {
            _parent?.RemoveChild( this );
        }

        /// <summary>
        /// Gets the number of child elements contained in this instance.
        /// </summary>
        /// <returns>The number of child elements. Returns 0 if there are no children.</returns>
        public int GetChildCount()
        {
            return _children?.Count ?? 0;
        }

        // ====================================================================

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

        // ====================================================================
        
        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return ToString( "" );
        }

        /// <summary>
        /// Returns a string that represents the current element and its children in XML format,
        /// using the specified indentation for formatting.
        /// <para>
        /// The output includes all attributes and child elements recursively, with each level
        /// indented according to the provided string. If the element has no children or text
        /// 0content, a self-closing tag is used.
        /// </para>
        /// </summary>
        /// <param name="indent">
        /// The string to use for indenting each level of the XML output. Typically consists
        /// of whitespace characters such as spaces or tabs.
        /// </param>
        /// <returns>
        /// A string containing the XML representation of the element, including its attributes,
        /// text content, and child elements, formatted with the specified indentation.
        /// </returns>
        public string ToString( string indent )
        {
            var sb = new StringBuilder();
            sb.Append( indent ).Append( '<' ).Append( Name );

            if ( _attributes != null )
            {
                foreach ( KeyValuePair< string, string? > entry in _attributes )
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
                string childIndent = indent + "\t";

                if ( !string.IsNullOrEmpty( Text ) )
                {
                    sb.Append( childIndent ).Append( Text ).Append( '\n' );
                }

                if ( _children != null )
                {
                    foreach ( Element? child in _children )
                    {
                        sb.Append( child?.ToString( childIndent ) ).Append( '\n' );
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