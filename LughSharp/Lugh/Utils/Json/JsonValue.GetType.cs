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

namespace LughSharp.Lugh.Utils.Json;

public partial class JsonValue
{
    /// <summary>
    /// Finds the child with the specified name and returns it as a string.
    /// Returns defaultValue if not found.
    /// </summary>
    public string? GetString( string name, string? defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.IsValue() || child.IsNull() ) ? defaultValue : child.AsString();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a float.
    /// Returns defaultValue if not found.
    /// </summary>
    public float GetFloat( string name, float defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.IsValue() || child.IsNull() ) ? defaultValue : child.AsFloat();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a double.
    /// Returns defaultValue if not found.
    /// </summary>
    public double GetDouble( string name, double defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.IsValue() || child.IsNull() ) ? defaultValue : child.AsDouble();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a long.
    /// Returns defaultValue if not found.
    /// </summary>
    public long GetLong( string name, long defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.IsValue() || child.IsNull() ) ? defaultValue : child.AsLong();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a int.
    /// Returns defaultValue if not found.
    /// </summary>
    public int GetInt( string name, int defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.IsValue() || child.IsNull() ) ? defaultValue : child.AsInt();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a bool.
    /// Returns defaultValue if not found.
    /// </summary>
    public bool GetBoolean( string name, bool defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.IsValue() || child.IsNull() ) ? defaultValue : child.AsBoolean();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a byte.
    /// Returns defaultValue if not found.
    /// </summary>
    public byte GetByte( string name, byte defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.IsValue() || child.IsNull() ) ? defaultValue : child.AsByte();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a short.
    /// Returns defaultValue if not found.
    /// </summary>
    public short GetShort( string name, short defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.IsValue() || child.IsNull() ) ? defaultValue : child.AsShort();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a char.
    /// Returns defaultValue if not found.
    /// </summary>
    public char GetChar( string name, char defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.IsValue() || child.IsNull() ) ? defaultValue : child.AsChar();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a string.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public string? GetString( string name )
    {
        var child = Get( name );

        if ( child == null )
        {
            throw new ArgumentException( $"Named value not found: {name}" );
        }

        return child.AsString();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a float.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public float GetFloat( string name )
    {
        var child = Get( name );

        if ( child == null )
        {
            throw new ArgumentException( $"Named value not found: {name}" );
        }

        return child.AsFloat();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a double.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public double GetDouble( string name )
    {
        var child = Get( name );

        if ( child == null )
        {
            throw new ArgumentException( $"Named value not found: {name}" );
        }

        return child.AsDouble();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a long.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public long GetLong( string name )
    {
        var child = Get( name );

        if ( child == null )
        {
            throw new ArgumentException( $"Named value not found: {name}" );
        }

        return child.AsLong();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a int.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public int GetInt( string name )
    {
        var child = Get( name );

        if ( child == null )
        {
            throw new ArgumentException( $"Named value not found: {name}" );
        }

        return child.AsInt();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a bool.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public bool GetBoolean( string name )
    {
        var child = Get( name );

        if ( child == null )
        {
            throw new ArgumentException( $"Named value not found: {name}" );
        }

        return child.AsBoolean();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a byte.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public byte GetByte( string name )
    {
        var child = Get( name );

        if ( child == null )
        {
            throw new ArgumentException( $"Named value not found: {name}" );
        }

        return child.AsByte();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a short.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public short GetShort( string name )
    {
        var child = Get( name );

        if ( child == null )
        {
            throw new ArgumentException( $"Named value not found: {name}" );
        }

        return child.AsShort();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a char.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public char GetChar( string name )
    {
        var child = Get( name );

        if ( child == null )
        {
            throw new ArgumentException( $"Named value not found: {name}" );
        }

        return child.AsChar();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a string.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public string? GetString( int index )
    {
        var child = Get( index );

        if ( child == null )
        {
            throw new ArgumentException( $"Indexed value not found: {Name}" );
        }

        return child.AsString();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a float.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public float GetFloat( int index )
    {
        var child = Get( index );

        if ( child == null )
        {
            throw new ArgumentException( $"Indexed value not found: {Name}" );
        }

        return child.AsFloat();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a double.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public double GetDouble( int index )
    {
        var child = Get( index );

        if ( child == null )
        {
            throw new ArgumentException( $"Indexed value not found: {Name}" );
        }

        return child.AsDouble();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a long.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public long GetLong( int index )
    {
        var child = Get( index );

        if ( child == null )
        {
            throw new ArgumentException( $"Indexed value not found: {Name}" );
        }

        return child.AsLong();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a int.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public int GetInt( int index )
    {
        var child = Get( index );

        if ( child == null )
        {
            throw new ArgumentException( $"Indexed value not found: {Name}" );
        }

        return child.AsInt();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a bool.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public bool GetBoolean( int index )
    {
        var child = Get( index );

        if ( child == null )
        {
            throw new ArgumentException( $"Indexed value not found: {Name}" );
        }

        return child.AsBoolean();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a byte.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public byte GetByte( int index )
    {
        var child = Get( index );

        if ( child == null )
        {
            throw new ArgumentException( $"Indexed value not found: {Name}" );
        }

        return child.AsByte();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a short.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public short GetShort( int index )
    {
        var child = Get( index );

        if ( child == null )
        {
            throw new ArgumentException( $"Indexed value not found: {Name}" );
        }

        return child.AsShort();
    }

    /// <summary>
    /// Finds the child with the specified index and returns it as a char.
    /// </summary>
    /// <exception cref="ArgumentException"> if the child was not found. </exception>
    public char GetChar( int index )
    {
        var child = Get( index );

        if ( child == null )
        {
            throw new ArgumentException( $"Indexed value not found: {Name}" );
        }

        return child.AsChar();
    }
}

