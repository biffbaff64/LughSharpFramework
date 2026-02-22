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
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Utils.XML;

/// <summary>
/// Builder style API for emitting XML.
/// <code>
/// StringWriter writer = new StringWriter();
/// XmlWriter xml = new XmlWriter(writer);
/// xml.element("meow")
///	.attribute("moo", "cow")
///	.element("child")
///		.attribute("moo", "cow")
///		.element("child")
///			.attribute("moo", "cow")
///			.text("XML is like pizza. If it doesn't solve your problem, you're not eating enough of it.")
///		.pop()
///	.pop()
/// .pop();
/// System.out.println(writer);
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

    public XmlWriter( StringWriter writer )
    {
        this._writer = writer;
        this._stack  = new List< string >();
    }

    private void Indent()
    {
        var count = _indent;
        
        if ( _currentElement != null )
        {
            count++;
        }

        for ( var i = 0; i < count; i++ )
        {
            _writer?.Write( '\t' );
        }
    }

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

    public XmlWriter Element( string name, object text )
    {
        return Element( name ).Text( text ).Pop();
    }

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

    public XmlWriter Text( object? text )
    {
        StartElementContent();
        
        var str = text == null ? "null" : text.ToString();

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

    public override void Write( char[] cbuf, int off, int len )
    {
        StartElementContent();

        _writer?.Write( cbuf, off, len );
    }

    public override void Flush()
    {
        _writer?.Flush();
    }
}

// ============================================================================
// ============================================================================