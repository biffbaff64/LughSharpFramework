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
using System.Collections.Generic;
using System.IO;

using JetBrains.Annotations;

namespace LughSharp.Source.Utils.Json;

/// <summary>
/// Lightweight event-based JSON parser. All values are provided as strings to reduce work
/// when many values are ignored.
/// </summary>
[PublicAPI]
public class JsonSkimmer
{
    public bool IsStopped { get; set; }
    
    // ========================================================================

    protected readonly List< char > Buffer = [ ];

    // ========================================================================

    private JsonSkimmer.JsonToken _nameString;
    private JsonSkimmer.JsonToken _value;

    private int[] _stack = new int[ 8 ];

    // ========================================================================

    public JsonSkimmer()
    {
        _nameString = new JsonSkimmer.JsonToken( Buffer );
        _value      = new JsonSkimmer.JsonToken( Buffer );
    }

    public void Parse( string json )
    {
        char[] data = json.ToCharArray();
        
        Parse( data, 0, data.Length );
    }

    public void Parse( TextReader reader )
    {
    }

    public void Parse( StreamReader input )
    {
    }

    public void Parse( FileInfo file )
    {
    }

    public void Parse( char[] data, int offset, int length )
    {
    }

    /// <summary>
    /// Called when the start of an object or array is encountered in the JSON.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="isObject"> True: an <c>object</c> was encountered, False: an <c>array</c> was encountered. </param>
    protected virtual void Push( JsonToken? name, bool isObject )
    {
    }

    /// <summary>
    /// Called when the end of an object or array is encountered in the JSON.
    /// </summary>
    protected virtual void Pop()
    {
    }

    /// <summary>
    /// Called when a value is encountered in the JSON.
    /// </summary>
    protected virtual void Value( JsonToken? name, JsonToken value )
    {
    }
    
    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class JsonToken
    {
        [PublicAPI]
        public enum TokenType
        {
            NullValue,
            TrueValue,
            FalseValue,
            Other,
        }

        // ========================================================================

        public char[]    Chars       { get; set; }
        public int       Start       { get; set; }
        public int       Length      { get; set; }
        public TokenType Type        { get; set; } = TokenType.Other;
        public bool      IsUnEscaped { get; set; }

        // ========================================================================

        private readonly List< char > _buffer;

        // ========================================================================

        public JsonToken( List< char > buffer )
        {
            _buffer = buffer;
        }

        public JsonValue Value()
        {
            throw new NotImplementedException();
        }

        public bool Equals( string str )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// If <see cref="UnEscape"/> is true, an unescaped string is allocated for
        /// the comparison.
        /// </summary>
        public bool EqualsString( string str )
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Allocates an unescaped string.
        /// </summary>
        /// <returns> <c>null</c> if this token represents null. </returns>
        public override string ToString()
        {
            throw new NotImplementedException();
        }

        private string UnEscape()
        {
            throw new NotImplementedException();
        }
    }
}

// ============================================================================
// ============================================================================