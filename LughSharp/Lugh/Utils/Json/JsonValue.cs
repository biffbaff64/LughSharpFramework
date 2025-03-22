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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

using LughSharp.Lugh.Utils.Guarding;
using LughSharp.Lugh.Utils.Json;

namespace LughSharp.Lugh.Utils.Json;

[PublicAPI]
public class JsonValue : IEnumerable< JsonValue >
{
    public string?    Name   { get; set; }
    public JsonValue? Child  { get; set; }
    public JsonValue? Parent { get; set; }
    public int        Size   { get; set; }

    /// <summary>
    /// May be null.
    /// When changing this field the parent <see cref="Size"/> may need to be changed.
    /// </summary>
    public JsonValue? Next { get; set; }

    /// <summary>
    /// May be null.
    /// When changing this field the parent <see cref="Size"/> may need to be changed.
    /// </summary>
    public JsonValue? Prev { get; set; }

    private ValueType _valueType;
    private string?   _stringValue;
    private double?   _doubleValue;
    private long?     _longValue;

    // ========================================================================

    public JsonValue( ValueType valueType )
    {
        this._valueType = valueType;
    }

    public JsonValue( string? value )
    {
        Set( value );
    }

    public JsonValue( double value )
    {
        Set( value, null );
    }

    public JsonValue( long value )
    {
        Set( value, null );
    }

    public JsonValue( double value, string _stringValue )
    {
        Set( value, _stringValue );
    }

    public JsonValue( long value, string _stringValue )
    {
        Set( value, _stringValue );
    }

    public JsonValue( bool value )
    {
        Set( value );
    }

    // ========================================================================

    /** Returns the child at the specified index. This requires walking the linked list to the specified entry, see
     * {@link JsonValue} for how to iterate efficiently.
     * @return May be null. */
    public JsonValue? Get( int index )
    {
        var current = Child;

        while ( ( current != null ) && ( index > 0 ) )
        {
            index--;
            current = current.Next;
        }

        return current;
    }

    /** Returns the child with the specified name.
     * @return May be null. */
    public JsonValue? Get( string name )
    {
        var current = Child;

        while ( ( current != null )
                && ( ( current.Name == null )
                     || !current.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) ) )
        {
            current = current.Next;
        }

        return current;
    }

    /** Returns true if a child with the specified name exists. */
    public bool ChildExists( string name )
    {
        return Get( name ) != null;
    }

    /** Returns the child at the specified index. This requires walking the linked list to the specified entry, see
     * {@link JsonValue} for how to iterate efficiently.
     * @throws ArgumentException if the child was not found. */
    public JsonValue Require( int index )
    {
        var current = Child;

        while ( ( current != null ) && ( index > 0 ) )
        {
            index--;
            current = current.Next;
        }

        if ( current == null ) throw new ArgumentException( $"Child not found with index: {index}" );

        return current;
    }

    /** Returns the child with the specified name.
     * @throws ArgumentException if the child was not found. */
    public JsonValue Require( string name )
    {
        var current = Child;

        while ( ( current != null )
                && ( ( current.Name == null )
                     || !current.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) ) )
        {
            current = current.Next;
        }

        if ( current == null ) throw new ArgumentException( $"Child not found with name: {name}" );

        return current;
    }

    /** Removes the child with the specified index. This requires walking the linked list to the specified entry, see
     * {@link JsonValue} for how to iterate efficiently.
     * @return May be null. */
    public JsonValue? Remove( int index )
    {
        var child = Get( index );

        if ( child == null ) return null;

        if ( child.Prev == null )
        {
            this.Child = child.Next;
            if ( this.Child != null ) this.Child.Prev = null;
        }
        else
        {
            child.Prev.Next = child.Next;
            if ( child.Next != null ) child.Next.Prev = child.Prev;
        }

        Size--;

        return child;
    }

    /** Removes the child with the specified name.
     * @return May be null. */
    public JsonValue? Remove( string name )
    {
        var child = Get( name );

        if ( child == null ) return null;

        if ( child.Prev == null )
        {
            this.Child = child.Next;
            if ( this.Child != null ) this.Child.Prev = null;
        }
        else
        {
            child.Prev.Next = child.Next;
            if ( child.Next != null ) child.Next.Prev = child.Prev;
        }

        Size--;

        return child;
    }

    /** Removes this value from its parent. */
    public void Remove()
    {
        Guard.ThrowIfNull( Parent );

        if ( Prev == null )
        {
            Parent!.Child = Next;
            if ( Parent.Child != null ) Parent.Child.Prev = null;
        }
        else
        {
            Prev.Next = Next;
            if ( Next != null ) Next.Prev = Prev;
        }

        Parent!.Size--;
    }

    /** Returns true if there are one or more children in the array or object. */
    public bool NotEmpty()
    {
        return Size > 0;
    }

    /** Returns true if there are not children in the array or object. */
    public bool IsEmpty()
    {
        return Size == 0;
    }

    /** Returns this value as a string.
     * @return May be null if this value is null.
     * @throws InvalidOperationException if this an array or object. */
    public string? AsString()
    {
        return _valueType switch
        {
            ValueType.StringValue  => _stringValue,
            ValueType.DoubleValue  => _stringValue ?? _doubleValue.ToString(),
            ValueType.LongValue    => _stringValue ?? _longValue.ToString(),
            ValueType.BooleanValue => _longValue != 0 ? "true" : "false",
            ValueType.NullValue    => null,

            var _ => throw new InvalidOperationException( $"Value cannot be converted to string: {_valueType}" ),
        };
    }

    /** Returns this value as a float.
     * @throws InvalidOperationException if this an array or object. */
    public float asFloat()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
                return Float.parseFloat( _stringValue );

            case ValueType.DoubleValue:
                return ( float )_doubleValue;

            case ValueType.LongValue:
                return _longValue;

            case ValueType.BooleanValue:
                return _longValue != 0 ? 1 : 0;
        }

        throw new InvalidOperationException( $"Value cannot be converted to float: {_valueType}" );
    }

    /** Returns this value as a double.
     * @throws InvalidOperationException if this an array or object. */
    public double asDouble()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
                return Double.parseDouble( _stringValue );

            case ValueType.DoubleValue:
                return _doubleValue;

            case ValueType.LongValue:
                return _longValue;

            case ValueType.BooleanValue:
                return _longValue != 0 ? 1 : 0;
        }

        throw new InvalidOperationException( $"Value cannot be converted to double: {_valueType}" );
    }

    /** Returns this value as a long.
     * @throws InvalidOperationException if this an array or object. */
    public long asLong()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
                return Long.parseLong( _stringValue );

            case ValueType.DoubleValue:
                return ( long )_doubleValue;

            case ValueType.LongValue:
                return _longValue;

            case ValueType.BooleanValue:
                return _longValue != 0 ? 1 : 0;
        }

        throw new InvalidOperationException( $"Value cannot be converted to long: {_valueType}" );
    }

    /** Returns this value as an int.
     * @throws InvalidOperationException if this an array or object. */
    public int asInt()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
                return Integer.parseInt( _stringValue );

            case ValueType.DoubleValue:
                return ( int )_doubleValue;

            case ValueType.LongValue:
                return ( int )_longValue;

            case ValueType.BooleanValue:
                return _longValue != 0 ? 1 : 0;
        }

        throw new InvalidOperationException( $"Value cannot be converted to int: {_valueType}" );
    }

    /** Returns this value as a bool.
     * @throws InvalidOperationException if this an array or object. */
    public bool asBoolean()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
                return _stringValue.Equals( "true", StringComparison.OrdinalIgnoreCase );

            case ValueType.DoubleValue:
                return _doubleValue != 0;

            case ValueType.LongValue:
                return _longValue != 0;

            case ValueType.BooleanValue:
                return _longValue != 0;
        }

        throw new InvalidOperationException( $"Value cannot be converted to bool: {_valueType}" );
    }

    /** Returns this value as a byte.
     * @throws InvalidOperationException if this an array or object. */
    public byte AsByte()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
                return byte.Parse( _stringValue );

            case ValueType.DoubleValue:
                return ( byte )_doubleValue;

            case ValueType.LongValue:
                return ( byte )_longValue;

            case ValueType.BooleanValue:
                return ( byte )( _longValue != 0 ? 1 : 0 );
        }

        throw new InvalidOperationException( $"Value cannot be converted to byte: {_valueType}" );
    }

    /** Returns this value as a short.
     * @throws InvalidOperationException if this an array or object. */
    public short AsShort()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
                return short.Parse( _stringValue );

            case ValueType.DoubleValue:
                return ( short )_doubleValue;

            case ValueType.LongValue:
                return ( short )_longValue;

            case ValueType.BooleanValue:
                return ( short )( _longValue != 0 ? 1 : 0 );
        }

        throw new InvalidOperationException( $"Value cannot be converted to short: {_valueType}" );
    }

    /** Returns this value as a char.
     * @throws InvalidOperationException if this an array or object. */
    public char AsChar()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
                return _stringValue.Length == 0 ? ( char )0 : _stringValue[ 0 ];

            case ValueType.DoubleValue:
                return ( char )_doubleValue;

            case ValueType.LongValue:
                return ( char )_longValue;
`
            case ValueType.BooleanValue:
                return ( char )( _longValue != 0 ? 1 : 0 );
        }

        throw new InvalidOperationException( $"Value cannot be converted to char: {_valueType}" );
    }

    /// <summary>
    /// Returns the children of this value as a newly allocated string array.
    /// </summary>
    /// <exception cref="InvalidOperationException"> if this is not an array. </exception>
    public string[] AsStringArray()
    {
        if ( _valueType is not ValueType.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {_valueType}" );
        }

        var array = new string[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            string? v;

            switch ( value._valueType )
            {
                case ValueType.StringValue:
                    v = value._stringValue;

                    break;

                case ValueType.DoubleValue:
                    v = _stringValue ?? value._doubleValue.ToString();

                    break;

                case ValueType.LongValue:
                    v = _stringValue ?? value._longValue.ToString();

                    break;

                case ValueType.BooleanValue:
                    v = value._longValue != 0 ? "true" : "false";

                    break;

                case ValueType.NullValue:
                    v = null;

                    break;

                default:
                    throw new InvalidOperationException( $"Value cannot be converted to string: {value._valueType}" );
            }

            array[ i ] = v!;
        }

        return array;
    }

    /** Returns the children of this value as a newly allocated float array.
     * @throws InvalidOperationException if this is not an array. */
    public float[] AsFloatArray()
    {
        if ( _valueType != ValueType.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {_valueType}" );
        }

        var array = new float[ Size ];
        var     i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            var v = value._valueType switch
            {
                ValueType.StringValue  => float.Parse( value._stringValue ),
                ValueType.DoubleValue  => ( float )value._doubleValue,
                ValueType.LongValue    => ( float )value._longValue,
                ValueType.BooleanValue => value._longValue != 0 ? 1 : 0,
                
                var _ => throw new InvalidOperationException( $"Value cannot be converted to float: {value._valueType}" )
            };

            array[ i ] = v;
        }

        return array;
    }

    /** Returns the children of this value as a newly allocated double array.
     * @throws InvalidOperationException if this is not an array. */
    public double[] AsDoubleArray()
    {
        if ( _valueType != ValueType.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {_valueType}" );
        }

        var array = new double[ Size ];
        var      i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            double v;

            switch ( value._valueType )
            {
                case ValueType.StringValue:
                    v = double.Parse( value._stringValue );

                    break;

                case ValueType.DoubleValue:
                    v = value._doubleValue;

                    break;

                case ValueType.LongValue:
                    v = value._longValue;

                    break;

                case ValueType.BooleanValue:
                    v = value._longValue != 0 ? 1 : 0;

                    break;

                default:
                    throw new InvalidOperationException( $"Value cannot be converted to double: {value._valueType}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    /// <summary>
    /// Returns the children of this value as a newly allocated long array.
    /// </summary>
    /// <exception cref="InvalidOperationException"> if this is not an array. </exception>
    public long?[] AsLongArray()
    {
        if ( _valueType != ValueType.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {_valueType}" );
        }

        var array = new long?[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            var v = value._valueType switch
            {
                ValueType.StringValue  => long.Parse( value._stringValue! ),
                ValueType.DoubleValue  => ( long )value._doubleValue!,
                ValueType.LongValue    => value._longValue,
                ValueType.BooleanValue => value._longValue != 0 ? 1 : 0,

                var _ => throw new InvalidOperationException( $"Value cannot be converted to long: {value._valueType}" ),
            };

            array[ i ] = v!;
        }

        return array;
    }

    /// <summary>
    /// Returns the children of this value as a newly allocated int array.
    /// </summary>
    /// <exception cref="InvalidOperationException"> if this is not an array. </exception>
    public int[] AsIntArray()
    {
        if ( _valueType != ValueType.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {_valueType}" );
        }

        var array = new int[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            int v;

            switch ( value._valueType )
            {
                case ValueType.StringValue:
                    v = int.Parse( value._stringValue );

                    break;

                case ValueType.DoubleValue:
                    v = ( int )value._doubleValue;

                    break;

                case ValueType.LongValue:
                    v = ( int )value._longValue;

                    break;

                case ValueType.BooleanValue:
                    v = value._longValue != 0 ? 1 : 0;

                    break;

                default:
                    throw new InvalidOperationException( $"Value cannot be converted to int: {value._valueType}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    /** Returns the children of this value as a newly allocated bool array.
     * @throws InvalidOperationException if this is not an array. */
    public bool[] AsBooleanArray()
    {
        if ( _valueType != ValueType.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {_valueType}" );
        }

        var array = new bool[ Size ];
        var    i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            bool v;

            switch ( value._valueType )
            {
                case ValueType.StringValue:
                    v = bool.Parse( value._stringValue );

                    break;

                case ValueType.DoubleValue:
                    v = value._doubleValue == 0;

                    break;

                case ValueType.LongValue:
                    v = value._longValue == 0;

                    break;

                case ValueType.BooleanValue:
                    v = value._longValue != 0;

                    break;

                default:
                    throw new InvalidOperationException( $"Value cannot be converted to bool: {value._valueType}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    /** Returns the children of this value as a newly allocated byte array.
     * @throws InvalidOperationException if this is not an array. */
    public byte[] AsByteArray()
    {
        if ( _valueType != ValueType.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {_valueType}" );
        }

        var array = new byte[ Size ];
        var    i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            byte v;

            switch ( value._valueType )
            {
                case ValueType.StringValue:
                    v = byte.Parse( value._stringValue );

                    break;

                case ValueType.DoubleValue:
                    v = ( byte )value._doubleValue;

                    break;

                case ValueType.LongValue:
                    v = ( byte )value._longValue;

                    break;

                case ValueType.BooleanValue:
                    v = ( byte )( value._longValue != 0 ? 1 : 0 );

                    break;

                default:
                    throw new InvalidOperationException( $"Value cannot be converted to byte: {value._valueType}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    /** Returns the children of this value as a newly allocated short array.
     * @throws InvalidOperationException if this is not an array. */
    public short[] AsShortArray()
    {
        if ( _valueType != ValueType.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {_valueType}" );
        }

        var array = new short[ Size ];
        var     i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            short v;

            switch ( value._valueType )
            {
                case ValueType.StringValue:
                    v = short.Parse( value._stringValue );

                    break;

                case ValueType.DoubleValue:
                    v = ( short )value._doubleValue;

                    break;

                case ValueType.LongValue:
                    v = ( short )value._longValue;

                    break;

                case ValueType.BooleanValue:
                    v = value._longValue != 0 ? ( short )1 : 0;

                    break;

                default:
                    throw new InvalidOperationException( $"Value cannot be converted to short: {value._valueType}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    /** Returns the children of this value as a newly allocated char array.
     * @throws InvalidOperationException if this is not an array. */
    public char[] asCharArray()
    {
        if ( _valueType != ValueType.ArrayValue ) throw new InvalidOperationException( $"Value is not an array: {_valueType}" );

        var array = new char[ size ];
        var    i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            char v;

            switch ( value._valueType )
            {
                case ValueType.StringValue:
                    v = value._stringValue.Length == 0 ? 0 : value._stringValue.charAt( 0 );

                    break;

                case ValueType.DoubleValue:
                    v = ( char )value._doubleValue;

                    break;

                case ValueType.LongValue:
                    v = ( char )value._longValue;

                    break;

                case ValueType.BooleanValue:
                    v = value._longValue != 0 ? ( char )1 : 0;

                    break;

                default:
                    throw new InvalidOperationException( $"Value cannot be converted to char: {value._valueType}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    /** Returns true if a child with the specified name exists and has a child. */
    public bool hasChild( string name )
    {
        return getChild( name ) != null;
    }

    /** Finds the child with the specified name and returns its first child.
     * @return May be null. */
    public JsonValue getChild( string name )
    {
        var child = Get( name );

        return child == null ? null : child.child;
    }

    /** Finds the child with the specified name and returns it as a string. Returns defaultValue if not found.
     * @param defaultValue May be null. */
    public string getString( string name, string defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.isValue() || child.isNull() ) ? defaultValue : child.asString();
    }

    /** Finds the child with the specified name and returns it as a float. Returns defaultValue if not found. */
    public float getFloat( string name, float defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.isValue() || child.isNull() ) ? defaultValue : child.asFloat();
    }

    /** Finds the child with the specified name and returns it as a double. Returns defaultValue if not found. */
    public double getDouble( string name, double defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.isValue() || child.isNull() ) ? defaultValue : child.asDouble();
    }

    /** Finds the child with the specified name and returns it as a long. Returns defaultValue if not found. */
    public long getLong( string name, long defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.isValue() || child.isNull() ) ? defaultValue : child.asLong();
    }

    /** Finds the child with the specified name and returns it as an int. Returns defaultValue if not found. */
    public int getInt( string name, int defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.isValue() || child.isNull() ) ? defaultValue : child.asInt();
    }

    /** Finds the child with the specified name and returns it as a bool. Returns defaultValue if not found. */
    public bool getBoolean( string name, bool defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.isValue() || child.isNull() ) ? defaultValue : child.asBoolean();
    }

    /** Finds the child with the specified name and returns it as a byte. Returns defaultValue if not found. */
    public byte getByte( string name, byte defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.isValue() || child.isNull() ) ? defaultValue : child.AsByte();
    }

    /** Finds the child with the specified name and returns it as a short. Returns defaultValue if not found. */
    public short getShort( string name, short defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.isValue() || child.isNull() ) ? defaultValue : child.AsShort();
    }

    /** Finds the child with the specified name and returns it as a char. Returns defaultValue if not found. */
    public char getChar( string name, char defaultValue )
    {
        var child = Get( name );

        return ( ( child == null ) || !child.isValue() || child.isNull() ) ? defaultValue : child.AsChar();
    }

    /** Finds the child with the specified name and returns it as a string.
     * @throws ArgumentException if the child was not found. */
    public string getString( string name )
    {
        var child = Get( name );

        if ( child == null ) throw new ArgumentException( $"Named value not found: {name}" );

        return child.asString();
    }

    /** Finds the child with the specified name and returns it as a float.
     * @throws ArgumentException if the child was not found. */
    public float getFloat( string name )
    {
        var child = Get( name );

        if ( child == null ) throw new ArgumentException( $"Named value not found: {name}" );

        return child.asFloat();
    }

    /** Finds the child with the specified name and returns it as a double.
     * @throws ArgumentException if the child was not found. */
    public double getDouble( string name )
    {
        var child = Get( name );

        if ( child == null ) throw new ArgumentException( $"Named value not found: {name}" );

        return child.asDouble();
    }

    /** Finds the child with the specified name and returns it as a long.
     * @throws ArgumentException if the child was not found. */
    public long getLong( string name )
    {
        var child = Get( name );

        if ( child == null ) throw new ArgumentException( $"Named value not found: {name}" );

        return child.asLong();
    }

    /** Finds the child with the specified name and returns it as an int.
     * @throws ArgumentException if the child was not found. */
    public int getInt( string name )
    {
        var child = Get( name );

        if ( child == null ) throw new ArgumentException( $"Named value not found: {name}" );

        return child.asInt();
    }

    /** Finds the child with the specified name and returns it as a bool.
     * @throws ArgumentException if the child was not found. */
    public bool getBoolean( string name )
    {
        var child = Get( name );

        if ( child == null ) throw new ArgumentException( $"Named value not found: {name}" );

        return child.asBoolean();
    }

    /** Finds the child with the specified name and returns it as a byte.
     * @throws ArgumentException if the child was not found. */
    public byte getByte( string name )
    {
        var child = Get( name );

        if ( child == null ) throw new ArgumentException( $"Named value not found: {name}" );

        return child.AsByte();
    }

    /** Finds the child with the specified name and returns it as a short.
     * @throws ArgumentException if the child was not found. */
    public short getShort( string name )
    {
        var child = Get( name );

        if ( child == null ) throw new ArgumentException( $"Named value not found: {name}" );

        return child.AsShort();
    }

    /** Finds the child with the specified name and returns it as a char.
     * @throws ArgumentException if the child was not found. */
    public char getChar( string name )
    {
        var child = Get( name );

        if ( child == null ) throw new ArgumentException( $"Named value not found: {name}" );

        return child.AsChar();
    }

    /** Finds the child with the specified index and returns it as a string.
     * @throws ArgumentException if the child was not found. */
    public string getString( int index )
    {
        var child = get( index );

        if ( child == null ) throw new ArgumentException( $"Indexed value not found: {name}" );

        return child.asString();
    }

    /** Finds the child with the specified index and returns it as a float.
     * @throws ArgumentException if the child was not found. */
    public float getFloat( int index )
    {
        var child = get( index );

        if ( child == null ) throw new ArgumentException( $"Indexed value not found: {name}" );

        return child.asFloat();
    }

    /** Finds the child with the specified index and returns it as a double.
     * @throws ArgumentException if the child was not found. */
    public double getDouble( int index )
    {
        var child = get( index );

        if ( child == null ) throw new ArgumentException( $"Indexed value not found: {name}" );

        return child.asDouble();
    }

    /** Finds the child with the specified index and returns it as a long.
     * @throws ArgumentException if the child was not found. */
    public long getLong( int index )
    {
        var child = get( index );

        if ( child == null ) throw new ArgumentException( $"Indexed value not found: {name}" );

        return child.asLong();
    }

    /** Finds the child with the specified index and returns it as an int.
     * @throws ArgumentException if the child was not found. */
    public int getInt( int index )
    {
        var child = get( index );

        if ( child == null ) throw new ArgumentException( $"Indexed value not found: {name}" );

        return child.asInt();
    }

    /** Finds the child with the specified index and returns it as a bool.
     * @throws ArgumentException if the child was not found. */
    public bool getBoolean( int index )
    {
        var child = get( index );

        if ( child == null ) throw new ArgumentException( $"Indexed value not found: {name}" );

        return child.asBoolean();
    }

    /** Finds the child with the specified index and returns it as a byte.
     * @throws ArgumentException if the child was not found. */
    public byte getByte( int index )
    {
        var child = get( index );

        if ( child == null ) throw new ArgumentException( $"Indexed value not found: {name}" );

        return child.asByte();
    }

    /** Finds the child with the specified index and returns it as a short.
     * @throws ArgumentException if the child was not found. */
    public short getShort( int index )
    {
        var child = get( index );

        if ( child == null ) throw new ArgumentException( $"Indexed value not found: {name}" );

        return child.asShort();
    }

    /** Finds the child with the specified index and returns it as a char.
     * @throws ArgumentException if the child was not found. */
    public char getChar( int index )
    {
        var child = get( index );

        if ( child == null ) throw new ArgumentException( $"Indexed value not found: {name}" );

        return child.asChar();
    }

    public ValueType type()
    {
        return type;
    }

    public void setType( ValueType type )
    {
        if ( type == null ) throw new ArgumentException( "type cannot be null." );

        this.type = type;
    }

    public bool isArray()
    {
        return type == ValueType.ArrayValue;
    }

    public bool isObject()
    {
        return type == ValueType.object;
    }

    public bool isString()
    {
        return type == ValueType._stringValue;
    }

    /** Returns true if this is a double or long value. */
    public bool isNumber()
    {
        return ( type == ValueType._doubleValue ) || ( type == ValueType._longValue );
    }

    public bool isDouble()
    {
        return type == ValueType._doubleValue;
    }

    public bool isLong()
    {
        return type == ValueType._longValue;
    }

    public bool isBoolean()
    {
        return type == ValueType.booleanValue;
    }

    public bool isNull()
    {
        return type == ValueType.nullValue;
    }

    /** Returns true if this is not an array or object. */
    public bool isValue()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
            case ValueType.DoubleValue:
            case ValueType.LongValue:
            case ValueType.BooleanValue:
            case ValueType.NullValue:
                return true;
        }

        return false;
    }

    /** Sets the name of the specified value and adds it after the last child. */
    public void addChild( string name, JsonValue value )
    {
        if ( name == null ) throw new ArgumentException( "name cannot be null." );

        value.name = name;
        addChild( value );
    }

    /** Adds the specified value after the last child. */
    public void addChild( JsonValue value )
    {
        value.Parent = this;
        size++;
        JsonValue current = child;

        if ( current == null )
            child = value;
        else
        {
            while ( true )
            {
                if ( current.Next == null )
                {
                    current.Next = value;
                    value.Prev   = current;

                    return;
                }

                current = current.Next;
            }
        }
    }

    /** @param value May be null. */
    public void set( string value )
    {
        _stringValue = value;
        type        = value == null ? ValueType.nullValue : ValueType._stringValue;
    }

    /** @param _stringValue May be null if the string representation is the string value of the double (eg, no leading zeros). */
    public void set( double value, string _stringValue )
    {
        _doubleValue      = value;
        _longValue        = ( long )value;
        this._stringValue = _stringValue;
        type             = ValueType._doubleValue;
    }

    /** @param _stringValue May be null if the string representation is the string value of the long (eg, no leading zeros). */
    public void set( long value, string _stringValue )
    {
        _longValue        = value;
        _doubleValue      = value;
        this._stringValue = _stringValue;
        type             = ValueType._longValue;
    }

    public void set( bool value )
    {
        _longValue = value ? 1 : 0;
        type      = ValueType.booleanValue;
    }

    public string toJson( OutputType outputType )
    {
        if ( isValue() ) return asString();

        var buffer = new StringBuilder( 512 );
        json( this, buffer, outputType );

        return buffer.toString();
    }

    private void json( JsonValue object, StringBuilder buffer, OutputType outputType) {
        if ( object.isObject() )
        {
            if ( object.child == null )
                buffer.append( "{}" );
            else
            {
                int start = buffer.Length;

                while ( true )
                {
                    buffer.append( '{' );
                    var i = 0;

                    for ( JsonValue child = object.child; child != null; child = child.Next )
                    {
                        buffer.append( outputType.quoteName( child.name ) );
                        buffer.append( ':' );
                        json( child, buffer, outputType );
                        if ( child.Next != null ) buffer.append( ',' );
                    }

                    break;
                }

                buffer.append( '}' );
            }
        }
        else if ( object.isArray() )
        {
            if ( object.child == null )
                buffer.append( "[]" );
            else
            {
                int start = buffer.Length;

                while ( true )
                {
                    buffer.append( '[' );

                    for ( JsonValue child = object.child; child != null; child = child.Next )
                    {
                        json( child, buffer, outputType );
                        if ( child.Next != null ) buffer.append( ',' );
                    }

                    break;
                }

                buffer.append( ']' );
            }
        }
        else if ( object.isString() )
        {
            buffer.append( outputType.quoteValue( object.asString() ) );
        }
        else if ( object.isDouble() )
        {
            double _doubleValue = object.asDouble();
            long   _longValue   = object.asLong();
            buffer.append( _doubleValue == _longValue ? _longValue : _doubleValue );
        }
        else if ( object.isLong() )
        {
            buffer.append( object.asLong() );
        }
        else if ( object.isBoolean() )
        {
            buffer.append( object.asBoolean() );
        }
        else if ( object.isNull() )
        {
            buffer.append( "null" );
        }
        else
            throw new SerializationException( $"Unknown object type: {object}" );
    }

    public string toString()
    {
        if ( isValue() ) return name == null ? asString() : $"{name}: {asString()}";

        return ( name == null ? "" : $"{name}: " ) + prettyPrint( OutputType.minimal, 0 );
    }

    /** Returns a human readable string representing the path from the root of the JSON object graph to this value. */
    public string trace()
    {
        if ( parent == null )
        {
            if ( type == ValueType.ArrayValue ) return "[]";

            if ( type == ValueType.object) return "{}";
            return "";
        }

        string trace;

        if ( parent.type == ValueType.ArrayValue )
        {
            trace = "[]";
            var i = 0;

            for ( JsonValue child = parent.child; child != null; child = child.Next, i++ )
            {
                if ( child == this )
                {
                    trace = $"[{i}]";

                    break;
                }
            }
        }
        else if ( name.indexOf( '.' ) != -1 )
            trace = $".\"{name.replace( "\"", "\\\"" )}\"";
        else
            trace = '.' + name;

        return parent.trace() + trace;
    }

    public string prettyPrint( OutputType outputType, int singleLineColumns )
    {
        var settings = new PrettyPrintSettings();
        settings.outputType        = outputType;
        settings.singleLineColumns = singleLineColumns;

        return prettyPrint( settings );
    }

    public string prettyPrint( PrettyPrintSettings settings )
    {
        var buffer = new StringBuilder( 512 );
        prettyPrint( this, buffer, 0, settings );

        return buffer.toString();
    }

    private void prettyPrint( JsonValue object, StringBuilder buffer, int indent, PrettyPrintSettings settings) {
        OutputType outputType = settings.outputType;

        if ( object.isObject() )
        {
            if ( object.child == null )
                buffer.append( "{}" );
            else
            {
                bool newLines = !isFlat( object );
                int  start    = buffer.Length;
            outer:

                while ( true )
                {
                    buffer.append( newLines ? "{\n" : "{ " );
                    var i = 0;

                    for ( JsonValue child = object.child; child != null; child = child.Next )
                    {
                        if ( newLines ) indent( indent, buffer );
                        buffer.append( outputType.quoteName( child.name ) );
                        buffer.append( ": " );
                        prettyPrint( child, buffer, indent + 1, settings );
                        if ( ( !newLines || ( outputType != OutputType.minimal ) ) && ( child.Next != null ) ) buffer.append( ',' );
                        buffer.append( newLines ? '\n' : ' ' );

                        if ( !newLines && ( ( buffer.Length - start ) > settings.singleLineColumns ) )
                        {
                            buffer.setLength( start );
                            newLines = true;

                            continue outer;
                        }
                    }

                    break;
                }

                if ( newLines ) indent( indent - 1, buffer );
                buffer.append( '}' );
            }
        }
        else if ( object.isArray() )
        {
            if ( object.child == null )
                buffer.append( "[]" );
            else
            {
                bool newLines = !isFlat( object );
                var wrap     = settings.wrapNumericArrays || !isNumeric( object );
                int  start    = buffer.Length;
            outer:

                while ( true )
                {
                    buffer.append( newLines ? "[\n" : "[ " );

                    for ( JsonValue child = object.child; child != null; child = child.Next )
                    {
                        if ( newLines ) indent( indent, buffer );
                        prettyPrint( child, buffer, indent + 1, settings );
                        if ( ( !newLines || ( outputType != OutputType.minimal ) ) && ( child.Next != null ) ) buffer.append( ',' );
                        buffer.append( newLines ? '\n' : ' ' );

                        if ( wrap && !newLines && ( ( buffer.Length - start ) > settings.singleLineColumns ) )
                        {
                            buffer.setLength( start );
                            newLines = true;

                            continue outer;
                        }
                    }

                    break;
                }

                if ( newLines ) indent( indent - 1, buffer );
                buffer.append( ']' );
            }
        }
        else if ( object.isString() )
        {
            buffer.append( outputType.quoteValue( object.asString() ) );
        }
        else if ( object.isDouble() )
        {
            double _doubleValue = object.asDouble();
            long   _longValue   = object.asLong();
            buffer.append( _doubleValue == _longValue ? _longValue : _doubleValue );
        }
        else if ( object.isLong() )
        {
            buffer.append( object.asLong() );
        }
        else if ( object.isBoolean() )
        {
            buffer.append( object.asBoolean() );
        }
        else if ( object.isNull() )
        {
            buffer.append( "null" );
        }
        else
            throw new SerializationException( $"Unknown object type: {object}" );
    }

    /** More efficient than {@link #prettyPrint(PrettyPrintSettings)} but {@link PrettyPrintSettings#singleLineColumns} and
     * {@link PrettyPrintSettings#wrapNumericArrays} are not supported. */
    public void prettyPrint( OutputType outputType, Writer writer ) throws IOException
    {
        PrettyPrintSettings settings = new PrettyPrintSettings();
        settings.outputType = outputType;
        prettyPrint(this, writer, 0, settings);
    }

    private void prettyPrint( JsonValue object, Writer writer, int indent, PrettyPrintSettings settings) throws
        IOException { OutputType outputType = settings.outputType;
        if (object.isObject()) {
        if ( object.child == null )
            writer.append( "{}" );
        else
        {
            var newLines = !isFlat( object ) || ( object.size > 6 );
            writer.append( newLines ? "{\n" : "{ " );
            var i = 0;

            for ( JsonValue child = object.child; child != null; child = child.Next )
            {
                if ( newLines ) indent( indent, writer );
                writer.append( outputType.quoteName( child.name ) );
                writer.append( ": " );
                prettyPrint( child, writer, indent + 1, settings );
                if ( ( !newLines || ( outputType != OutputType.minimal ) ) && ( child.Next != null ) ) writer.append( ',' );
                writer.append( newLines ? '\n' : ' ' );
            }

            if ( newLines ) indent( indent - 1, writer );
            writer.append( '}' );
        }
    }

else if ( object.isArray() )
{
    if ( object.child == null )
        writer.append( "[]" );
    else
    {
        bool newLines = !isFlat( object );
        writer.append( newLines ? "[\n" : "[ " );
        var i = 0;

        for ( JsonValue child = object.child; child != null; child = child.Next )
        {
            if ( newLines ) indent( indent, writer );
            prettyPrint( child, writer, indent + 1, settings );
            if ( ( !newLines || ( outputType != OutputType.minimal ) ) && ( child.Next != null ) ) writer.append( ',' );
            writer.append( newLines ? '\n' : ' ' );
        }

        if ( newLines ) indent( indent - 1, writer );
        writer.append( ']' );
    }
}
else if ( object.isString() )
{
    writer.append( outputType.quoteValue( object.asString() ) );
}
else if ( object.isDouble() )
{
    double _doubleValue = object.asDouble();
    long   _longValue   = object.asLong();
    writer.append( Double.toString( _doubleValue == _longValue ? _longValue : _doubleValue ) );
}
else if ( object.isLong() )
{
    writer.append( Long.toString( object.asLong() ) );
}
else if ( object.isBoolean() )
{
    writer.append( Boolean.toString( object.asBoolean() ) );
}
else if ( object.isNull() )
{
    writer.append( "null" );
}
else
    throw new SerializationException( $"Unknown object type: {object}" );

}

static private bool isFlat( JsonValue
object) {
    for ( JsonValue child = object.child; child != null; child = child.Next )
        if ( child.isObject() || child.isArray() )
            return false;

    return true;
}

static private bool isNumeric( JsonValue
object) {
    for ( JsonValue child = object.child; child != null; child = child.Next )
        if ( !child.isNumber() )
            return false;

    return true;
}

static private void indent( int count, StringBuilder buffer )
{
    for ( var i = 0; i < count; i++ )
        buffer.append( '\t' );
}

static private void indent( int count, Writer buffer )
throws IOException {
    for ( var i = 0; i < count; i++ )
        buffer.append( '\t' );
}

// ========================================================================

/// <inheritdoc />
public IEnumerator< JsonValue > GetEnumerator()
{
    return new JsonIterator( this, this.Size );
}

/// <inheritdoc />
IEnumerator IEnumerable.GetEnumerator()
{
    return GetEnumerator();
}

// ========================================================================
// ========================================================================

[PublicAPI]
public class JsonIterator : IEnumerator< JsonValue >, IEnumerable< JsonValue >
{
    private JsonValue? _child;
    private JsonValue? _current;
    private int        _size;

    public JsonIterator( JsonValue child, int size )
    {
        this._child = child;
        this._size  = size;
    }

    public bool MoveNext()
    {
        if ( _child != null )
        {
            _current = _child;
            _child   = _child.Next;

            return true;
        }

        return false;
    }

    public JsonValue Current => _current!;

    object IEnumerator.Current => Current;

    public void Reset()
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public void Dispose()
    {
    } // Nothing to dispose

    public void Remove()
    {
        if ( _current == null )
        {
            throw new InvalidOperationException( "Remove can only be called after Next." );
        }

        if ( _current.Prev == null )
        {
            _child = _current.Next;
            if ( _child != null ) _child.Prev = null;
        }
        else
        {
            _current.Prev.Next = _current.Next;
            if ( _current.Next != null ) _current.Next.Prev = _current.Prev;
        }

        _size--;
    }

    public IEnumerator< JsonValue > GetEnumerator() => this;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

// ========================================================================

[PublicAPI]
public enum ValueType
{
    ObjectValue,
    ArrayValue,
    StringValue,
    DoubleValue,
    LongValue,
    BooleanValue,
    NullValue,
}

// ========================================================================

[PublicAPI]
public class PrettyPrintSettings
{
    /// <summary>
    /// </summary>
    public JsonOutputType OutputType { get; set; }

    /// <summary>
    /// If an object on a single line fits this many columns, it won't wrap.
    /// </summary>
    public int SingleLineColumns { get; set; }

    /// <summary>
    /// Enables or Disables float array wrapping.
    /// </summary>
    public bool WrapNumericArrays { get; set; }
}

}