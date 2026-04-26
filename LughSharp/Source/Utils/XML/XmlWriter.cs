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

using JetBrains.Annotations;

using LughSharp.Source.Collections;
using LughSharp.Source.Utils.Exceptions;

namespace LughSharp.Source.Utils.XML;

/// <summary>
/// Builder style API for emitting XML.
/// <code>
///     StringWriter writer = new();
///     XmlWriter xml = new XmlWriter( writer );
///     xml.Element( "meow" )
///	       .Attribute( "moo", "cow" )
///	       .Element( "child" )
///	       .Attribute( "moo", "cow" )
///	       .Element( "child" )
///	       .Attribute( "moo", "cow" )
///	       .Text( "Pineapple DOES belong on Pizza!." )
///	       .Pop()
///	       .Pop()
///        .Pop();
///     Console.WriteLine( writer.ToString() );
/// </code>
/// </summary>
[PublicAPI]
public class XmlWriter : StringWriter
{
    private readonly StringWriter?  _writer;
    private readonly List< string > _stack;
    private          string?        _currentElement;
    private          bool           _indentNextClose;
    private          int            _indent;

    // ========================================================================

    /// <summary>
    /// Initializes a new instance of the XmlWriter class that writes XML data to
    /// the specified StringWriter.
    /// </summary>
    /// <param name="writer">
    /// The StringWriter to which the XML content will be written. Cannot be null.
    /// </param>
    public XmlWriter( StringWriter writer )
    {
        _writer = writer;
        _stack  = new List< string >();
    }

    /// <summary>
    /// Writes a start tag with the specified element name and prepares the writer
    /// for nested content.
    /// <para>
    /// If called within another element, this method writes the appropriate indentation
    /// and line breaks to maintain readable XML formatting. The method does not write
    /// any attributes or content for the element; use additional methods to add attributes
    /// or nested elements as needed.
    /// </para>
    /// </summary>
  
    /// <param name="name">The name of the element to write. Cannot be null or empty.</param>
    /// <returns>The current instance of the XmlWriter, allowing for method chaining.</returns>
    public XmlWriter Element( string name )
    {
        if ( StartElementContent() )
        {
            _writer?.Write( '\n' );
        }

        Indent();

        _writer?.Write( '<' );
        _writer?.Write( name );

        _currentElement = name;

        return this;
    }

    /// <summary>
    /// Writes an XML element with the specified name and text content, and returns the
    /// parent writer for further operations.
    /// </summary>
    /// <param name="name">The name of the XML element to write. Cannot be null or empty.</param>
    /// <param name="text">
    /// The text content to include within the XML element. If null, an empty element
    /// is written.
    /// </param>
    /// <returns>
    /// The parent XmlWriter instance, allowing for method chaining or additional element
    /// writing.
    /// </returns>
    public XmlWriter Element( string name, object text )
    {
        return Element( name ).Text( text ).Pop();
    }

    /// <summary>
    /// Writes an attribute with the specified name and value to the current XML element.
    /// </summary>
    /// <param name="name">The name of the attribute to write. Cannot be null.</param>
    /// <param name="value">
    /// The value to assign to the attribute. If null, the string "null" is written as
    /// the attribute value.
    /// </param>
    /// <returns>The current instance of the writer, enabling method chaining.</returns>
    public XmlWriter Attribute( string name, object? value )
    {
        Guard.Against.Null( _currentElement );

        _writer?.Write( ' ' );
        _writer?.Write( name );
        _writer?.Write( "=\"" );
        _writer?.Write( value == null ? "null" : value.ToString() );
        _writer?.Write( '"' );

        return this;
    }

    /// <summary>
    /// Writes the string representation of the specified object as text content to
    /// the current XML element. If the resulting text exceeds 64 characters in length,
    /// the output is indented for readability.
    /// </summary>
    /// <param name="text">
    /// The object whose string representation is written as text. If null, the string
    /// "null" is written.
    /// </param>
    /// <returns>The current instance of the XmlWriter, enabling method chaining.</returns>
    public XmlWriter Text( object? text )
    {
        StartElementContent();

        string? str = text == null ? "null" : text.ToString();

        _indentNextClose = str?.Length > 64;

        if ( _indentNextClose )
        {
            _writer?.Write( '\n' );
            Indent();
        }

        _writer?.Write( str );

        if ( _indentNextClose )
        {
            _writer?.Write( '\n' );
        }

        return this;
    }

    /// <summary>
    /// Closes the current XML element or ends the most recently opened element, and returns
    /// the underlying XmlWriter for further writing.
    /// <para>
    /// If an element is currently open, this method writes a self-closing tag. Otherwise,
    /// it writes a closing tag for the most recently opened element. This method maintains
    /// proper indentation and element nesting for the generated XML.
    /// </para>
    /// </summary>
    /// <returns>
    /// The underlying XmlWriter instance, allowing for additional XML content to be written.
    /// </returns>
    public XmlWriter Pop()
    {
        if ( _currentElement != null )
        {
            _writer?.Write( "/>\n" );
            _currentElement = null;
        }
        else
        {
            _indent = Math.Max( _indent - 1, 0 );

            if ( _indentNextClose )
            {
                Indent();
            }

            _writer?.Write( "</" );
            _writer?.Write( _stack.Pop() );
            _writer?.Write( ">\n" );
        }

        _indentNextClose = true;

        return this;
    }

    /// <summary>
    /// Calls <see cref="Pop"/> for each remaining open element, if any,
    /// and closes the stream.
    /// </summary>
    public override void Close()
    {
        while ( _stack.Count != 0 )
        {
            Pop();
        }

        _writer?.Close();
    }

    /// <summary>
    /// Writes a specified range of characters from the given buffer to the underlying writer.
    /// </summary>
    /// <param name="cbuf">The character array containing the data to write.</param>
    /// <param name="off">The zero-based index in the buffer at which to begin writing characters.</param>
    /// <param name="len">The number of characters to write from the buffer.</param>
    public override void Write( char[] cbuf, int off, int len )
    {
        StartElementContent();

        _writer?.Write( cbuf, off, len );
    }

    /// <summary>
    /// Clears all buffers for the current writer and causes any buffered data to be
    /// written to the underlying device.
    /// <para>
    /// Call this method to ensure that all buffered data is written to the underlying
    /// storage or stream. This is especially important before closing the writer or
    /// when immediate data persistence is required.
    /// </para>
    /// </summary>
    public override void Flush()
    {
        _writer?.Flush();
    }

    /// <summary>
    /// Begins writing the content section of the current XML element, finalizing the
    /// element's start tag if applicable.
    /// <para>
    /// Call this method after writing an element's start tag to transition to writing
    /// its content. If there is no current element to start, the method returns false
    /// and no changes are made.
    /// </para>
    /// </summary>
    /// <returns>true if the content section was successfully started; otherwise, false.</returns>
    private bool StartElementContent()
    {
        if ( _currentElement == null )
        {
            return false;
        }

        _indent++;

        _stack.Add( _currentElement );
        _currentElement = null;

        _writer?.Write( ">" );

        return true;
    }

    /// <summary>
    /// Writes indentation to the underlying writer based on the current indentation level
    /// and element context.
    /// <para>
    /// Increases the indentation by one level if an element is currently open. Indentation
    /// is written using tab characters. This method is intended for internal formatting
    /// and is not meant to be called directly by consumers.
    /// </para>
    /// </summary>
    private void Indent()
    {
        int count = _indent;

        if ( _currentElement != null )
        {
            count++;
        }

        for ( var i = 0; i < count; i++ )
        {
            _writer?.Write( '\t' );
        }
    }
}

// ============================================================================
// ============================================================================