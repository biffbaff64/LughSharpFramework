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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

using LughSharp.Lugh.Maths;
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

        if ( current == null )
        {
            throw new ArgumentException( $"Child not found with index: {index}" );
        }

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

        if ( current == null )
        {
            throw new ArgumentException( $"Child not found with name: {name}" );
        }

        return current;
    }

    /** Removes the child with the specified index. This requires walking the linked list to the specified entry, see
     * {@link JsonValue} for how to iterate efficiently.
     * @return May be null. */
    public JsonValue? Remove( int index )
    {
        var child = Get( index );

        if ( child == null )
        {
            return null;
        }

        if ( child.Prev == null )
        {
            this.Child = child.Next;

            if ( this.Child != null )
            {
                this.Child.Prev = null;
            }
        }
        else
        {
            child.Prev.Next = child.Next;

            if ( child.Next != null )
            {
                child.Next.Prev = child.Prev;
            }
        }

        Size--;

        return child;
    }

    /** Removes the child with the specified name.
     * @return May be null. */
    public JsonValue? Remove( string name )
    {
        var child = Get( name );

        if ( child == null )
        {
            return null;
        }

        if ( child.Prev == null )
        {
            this.Child = child.Next;

            if ( this.Child != null )
            {
                this.Child.Prev = null;
            }
        }
        else
        {
            child.Prev.Next = child.Next;

            if ( child.Next != null )
            {
                child.Next.Prev = child.Prev;
            }
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

            if ( Parent.Child != null )
            {
                Parent.Child.Prev = null;
            }
        }
        else
        {
            Prev.Next = Next;

            if ( Next != null )
            {
                Next.Prev = Prev;
            }
        }

        Parent!.Size--;
    }

    /** Returns true if there are one or more children in the array or @object. */
    public bool NotEmpty()
    {
        return Size > 0;
    }

    /** Returns true if there are not children in the array or @object. */
    public bool IsEmpty()
    {
        return Size == 0;
    }

    /** Returns this value as a string.
     * @return May be null if this value is null.
     * @throws InvalidOperationException if this an array or @object. */
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

    public float AsFloat()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
                return float.Parse( _stringValue );

            case ValueType.DoubleValue:
                return ( float )_doubleValue;

            case ValueType.LongValue:
                return ( float )_longValue;

            case ValueType.BooleanValue:
                return _longValue != 0 ? 1 : 0;
            
            default:
                break;
        }

        throw new InvalidOperationException( $"Value cannot be converted to float: {_valueType}" );
    }

    /** Returns this value as a double.
     * @throws InvalidOperationException if this an array or @object. */
    public double AsDouble()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
                return double.Parse( _stringValue );

            case ValueType.DoubleValue:
                return ( double )_doubleValue!;

            case ValueType.LongValue:
                return ( double )_longValue!;

            case ValueType.BooleanValue:
                return _longValue != 0 ? 1 : 0;
            
            default:
                break;
        }

        throw new InvalidOperationException( $"Value cannot be converted to double: {_valueType}" );
    }

    /** Returns this value as a long.
     * @throws InvalidOperationException if this an array or @object. */
    public long AsLong()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
                return long.Parse( _stringValue );

            case ValueType.DoubleValue:
                return ( long )_doubleValue;

            case ValueType.LongValue:
                return ( long )_longValue;

            case ValueType.BooleanValue:
                return _longValue != 0 ? 1 : 0;
            
            default:
                break;
        }

        throw new InvalidOperationException( $"Value cannot be converted to long: {_valueType}" );
    }

    /** Returns this value as an int.
     * @throws InvalidOperationException if this an array or @object. */
    public int AsInt()
    {
        switch ( _valueType )
        {
            case ValueType.StringValue:
                return int.Parse( _stringValue );

            case ValueType.DoubleValue:
                return ( int )_doubleValue;

            case ValueType.LongValue:
                return ( int )_longValue;

            case ValueType.BooleanValue:
                return _longValue != 0 ? 1 : 0;
            
            default:
                break;
        }

        throw new InvalidOperationException( $"Value cannot be converted to int: {_valueType}" );
    }

    /** Returns this value as a bool.
     * @throws InvalidOperationException if this an array or @object. */
    public bool AsBoolean()
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
            
            default:
                break;
        }

        throw new InvalidOperationException( $"Value cannot be converted to bool: {_valueType}" );
    }

    /** Returns this value as a byte.
     * @throws InvalidOperationException if this an array or @object. */
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
            
            default:
                break;
        }

        throw new InvalidOperationException( $"Value cannot be converted to byte: {_valueType}" );
    }

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
            
            default:
                break;
        }

        throw new InvalidOperationException( $"Value cannot be converted to short: {_valueType}" );
    }

    /** Returns this value as a char.
     * @throws InvalidOperationException if this an array or @object. */
    public char AsChar()
    {
        Guard.ThrowIfNull( _stringValue );

        return _valueType switch
        {
            ValueType.StringValue  => _stringValue.Length == 0 ? ( char )0 : _stringValue[ 0 ],
            ValueType.DoubleValue  => ( char )_doubleValue,
            ValueType.LongValue    => ( char )_longValue,
            ValueType.BooleanValue => ( char )( _longValue != 0 ? 1 : 0 ),
            var _                  => throw new InvalidOperationException( $"Value cannot be converted to char: {_valueType}" )
        };
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
        var i     = 0;

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
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            double v;

            switch ( value._valueType )
            {
                case ValueType.StringValue:
                    v = double.Parse( value._stringValue );

                    break;

                case ValueType.DoubleValue:
                    v = ( double )value._doubleValue;

                    break;

                case ValueType.LongValue:
                    v = ( double )value._longValue;

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
        var i     = 0;

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
        var i     = 0;

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
        var i     = 0;

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
                    v = ( short )( value._longValue != 0 ? 1 : 0 );

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
    public char[] AsCharArray()
    {
        if ( _valueType != ValueType.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {_valueType}" );
        }

        var array = new char[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            char v;

            switch ( value._valueType )
            {
                case ValueType.StringValue:
                    v = ( char )( value._stringValue.Length == 0 ? 0 : value._stringValue.ToCharArray()[ 0 ] );

                    break;

                case ValueType.DoubleValue:
                    v = ( char )value._doubleValue;

                    break;

                case ValueType.LongValue:
                    v = ( char )value._longValue;

                    break;

                case ValueType.BooleanValue:
                    v = ( char )( value._longValue != 0 ? 1 : 0 );

                    break;

                default:
                    throw new InvalidOperationException( $"Value cannot be converted to char: {value._valueType}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    /// <summary>
    /// Returns true if a child with the specified name exists.
    /// </summary>
    public bool ChildExists( string name )
    {
        return Get( name ) != null;
    }

    /// <summary>
    /// Returns true if a child with the specified name exists, and has a child.
    /// </summary>
    public bool ChildWithChildExists( string name )
    {
        return GetChild( name ) != null;
    }

    /// <summary>
    /// Finds the child with the specified name and returns its first child.
    /// </summary>
    public JsonValue? GetChild( string name )
    {
        var child = Get( name );

        return child == null ? null : child.Child;
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a string.
    /// Returns defaultValue if not found.
    /// </summary>
    public string? GetString( string name, string defaultValue )
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
    public string GetString( string name )
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

    public bool IsArray()
    {
        return _valueType == ValueType.ArrayValue;
    }

    public bool IsObject()
    {
        return _valueType == ValueType.ObjectValue;
    }

    public bool IsString()
    {
        return _valueType == ValueType.StringValue;
    }

    public bool IsNumber()
    {
        return _valueType is ValueType.DoubleValue or ValueType.LongValue;
    }

    public bool IsDouble()
    {
        return _valueType == ValueType.DoubleValue;
    }

    public bool IsLong()
    {
        return _valueType == ValueType.LongValue;
    }

    public bool IsBoolean()
    {
        return _valueType == ValueType.BooleanValue;
    }

    public bool IsNull()
    {
        return _valueType == ValueType.NullValue;
    }

    /// <summary>
    /// Returns true if this is not an array or object.
    /// </summary>
    public bool IsValue()
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

    // ========================================================================

    /// <summary>
    /// Sets the name of the specified value and adds it after the last child.
    /// </summary>
    public void AddChild( string name, JsonValue value )
    {
        if ( name == null )
        {
            throw new ArgumentException( "name cannot be null." );
        }

        value.Name = name;
        
        AddChild( value );
    }

    /// <summary>
    /// Adds the specified value after the last child.
    /// </summary>
    public void AddChild( JsonValue value )
    {
        value.Parent = this;
        Size++;

        var current = Child;

        if ( current == null )
        {
            Child = value;
        }
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

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    public void Set( string? value )
    {
        _stringValue = value;
        _valueType   = value == null ? ValueType.NullValue : ValueType.StringValue;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="stringValue">
    /// May be null if the string representation is the string value of the double (eg, no leading zeros).
    /// </param>
    public void Set( double value, string? stringValue )
    {
        _doubleValue = value;
        _longValue   = ( long )value;
        _stringValue = stringValue;
        _valueType   = ValueType.DoubleValue;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="stringValue">
    /// May be null if the string representation is the string value of the long (eg, no leading zeros).
    /// </param>
    public void Set( long value, string stringValue )
    {
        _longValue   = value;
        _doubleValue = value;
        _stringValue = stringValue;
        _valueType   = ValueType.LongValue;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    public void Set( bool value )
    {
        _longValue = value ? 1 : 0;
        _valueType = ValueType.BooleanValue;
    }

    // ========================================================================

    public string? ToJson( JsonOutputType outputType )
    {
        if ( IsValue() )
        {
            return AsString();
        }

        var buffer = new StringBuilder( 512 );

        Json( this, buffer, outputType );

        return buffer.ToString();
    }

    private static void Json( JsonValue jsonval, StringBuilder buffer, JsonOutputType outputType )
    {
        if ( jsonval.IsObject() )
        {
            if ( jsonval.Child == null )
            {
                buffer.Append( "{}" );
            }
            else
            {
                var start = buffer.Length;

                while ( true )
                {
                    buffer.Append( '{' );

                    for ( var child = jsonval.Child; child != null; child = child.Next )
                    {
                        buffer.Append( outputType.QuoteName( child.Name ?? "" ) );
                        buffer.Append( ':' );

                        Json( child, buffer, outputType );

                        if ( child.Next != null )
                        {
                            buffer.Append( ',' );
                        }
                    }

                    break;
                }

                buffer.Append( '}' );
            }
        }
        else if ( jsonval.IsArray() )
        {
            if ( jsonval.Child == null )
            {
                buffer.Append( "[]" );
            }
            else
            {
                while ( true )
                {
                    buffer.Append( '[' );

                    for ( var child = jsonval.Child; child != null; child = child.Next )
                    {
                        Json( child, buffer, outputType );

                        if ( child.Next != null )
                        {
                            buffer.Append( ',' );
                        }
                    }

                    break;
                }

                buffer.Append( ']' );
            }
        }
        else if ( jsonval.IsString() )
        {
            buffer.Append( outputType.QuoteValue( jsonval.AsString() ) );
        }
        else if ( jsonval.IsDouble() )
        {
            var doubleValue = jsonval.AsDouble();
            var longValue   = jsonval.AsLong();

            buffer.Append( Math.Abs( doubleValue - longValue ) < Number.FLOAT_TOLERANCE ? longValue : doubleValue );
        }
        else if ( jsonval.IsLong() )
        {
            buffer.Append( jsonval.AsLong() );
        }
        else if ( jsonval.IsBoolean() )
        {
            buffer.Append( jsonval.AsBoolean() );
        }
        else if ( jsonval.IsNull() )
        {
            buffer.Append( "null" );
        }
        else
        {
            throw new SerializationException( $"Unknown jsonval type: {jsonval}" );
        }
    }

    // ========================================================================
    
    /// <inheritdoc />
    public override string? ToString()
    {
        if ( IsValue() )
        {
            return Name == null ? AsString() : $"{Name}: {AsString()}";
        }

        return ( Name == null ? "" : $"{Name}: " ) + PrettyPrint( JsonOutputType.Minimal, 0 );
    }

    /// <summary>
    /// Returns a human readable string representing the path from the root of the
    /// JSON object graph to this value.
    /// </summary>
    public string Trace()
    {
        if ( Parent == null )
        {
            return _valueType switch
            {
                ValueType.ArrayValue  => "[]",
                ValueType.ObjectValue => "{}",
                _                     => ""
            };
        }

        string trace;

        if ( Parent._valueType == ValueType.ArrayValue )
        {
            trace = "[]";
            var i = 0;

            for ( var child = Parent.Child; child != null; child = child.Next, i++ )
            {
                if ( child == this )
                {
                    trace = $"[{i}]";

                    break;
                }
            }
        }
        else if ( Name?.IndexOf( '.' ) != -1 )
        {
            trace = $".\"{Name?.Replace( "\"", "\\\"" )}\"";
        }
        else
        {
            trace = '.' + Name;
        }

        return Parent.Trace() + trace;
    }

    public string PrettyPrint( JsonOutputType outputType, int singleLineColumns )
    {
        var settings = new PrettyPrintSettings
        {
            JsonOutputType = outputType,
            SingleLineColumns = singleLineColumns,
        };
        
        return PrettyPrint( settings );
    }

    public string PrettyPrint( PrettyPrintSettings settings )
    {
        var buffer = new StringBuilder( 512 );

        PrettyPrint( this, buffer, 0, settings );

        return buffer.ToString();
    }

    private void PrettyPrint( JsonValue jsonval, StringBuilder buffer, int indent, PrettyPrintSettings settings )
    {
        var outputType = settings.JsonOutputType;

        if ( jsonval.IsObject() )
        {
            if ( jsonval.Child == null )
            {
                buffer.Append( "{}" );
            }
            else
            {
                var newLines = !IsFlat( jsonval );
                var start    = buffer.Length;
            outer:

                while ( true )
                {
                    buffer.Append( newLines ? "{\n" : "{ " );
                    var i = 0;

                    for ( var child = jsonval.Child; child != null; child = child.Next )
                    {
                        if ( newLines )
                        {
                            Indent( indent, buffer );
                        }

                        buffer.Append( outputType.QuoteName( child.Name ) );
                        buffer.Append( ": " );
                        PrettyPrint( child, buffer, indent + 1, settings );

                        if ( ( !newLines || ( outputType != JsonOutputType.Minimal ) ) && ( child.Next != null ) )
                        {
                            buffer.Append( ',' );
                        }

                        buffer.Append( newLines ? '\n' : ' ' );

                        if ( !newLines && ( ( buffer.Length - start ) > settings.SingleLineColumns ) )
                        {
                            buffer.Length = start;
                            newLines = true;

                            goto outer;
                        }
                    }

                    break;
                }

                if ( newLines )
                {
                    Indent( indent - 1, buffer );
                }

                buffer.Append( '}' );
            }
        }
        else if ( jsonval.IsArray() )
        {
            if ( jsonval.Child == null )
            {
                buffer.Append( "[]" );
            }
            else
            {
                var newLines = !IsFlat( jsonval );
                var wrap     = settings.WrapNumericArrays || !IsNumeric( jsonval );
                var start    = buffer.Length;
            outer:

                while ( true )
                {
                    buffer.Append( newLines ? "[\n" : "[ " );

                    for ( var child = jsonval.Child; child != null; child = child.Next )
                    {
                        if ( newLines )
                        {
                            Indent( indent, buffer );
                        }

                        PrettyPrint( child, buffer, indent + 1, settings );

                        if ( ( !newLines || ( outputType != JsonOutputType.Minimal ) )
                             && ( child.Next != null ) )
                        {
                            buffer.Append( ',' );
                        }

                        buffer.Append( newLines ? '\n' : ' ' );

                        if ( wrap && !newLines && ( ( buffer.Length - start ) > settings.SingleLineColumns ) )
                        {
                            buffer.Length = start;
                            newLines      = true;

                            goto outer;
                        }
                    }

                    break;
                }

                if ( newLines )
                {
                    Indent( indent - 1, buffer );
                }

                buffer.Append( ']' );
            }
        }
        else if ( jsonval.IsString() )
        {
            buffer.Append( outputType.QuoteValue( jsonval.AsString() ) );
        }
        else if ( jsonval.IsDouble() )
        {
            var doubleValue = jsonval.AsDouble();
            var longValue   = jsonval.AsLong();

            buffer.Append( Math.Abs( doubleValue - longValue ) < Number.FLOAT_TOLERANCE
                               ? longValue
                               : doubleValue );
        }
        else if ( jsonval.IsLong() )
        {
            buffer.Append( jsonval.AsLong() );
        }
        else if ( jsonval.IsBoolean() )
        {
            buffer.Append( jsonval.AsBoolean() );
        }
        else if ( jsonval.IsNull() )
        {
            buffer.Append( "null" );
        }
        else
        {
            throw new SerializationException( $"Unknown @object type: {jsonval}" );
        }
    }

    public void PrettyPrint( JsonOutputType outputType, TextWriter writer )
    {
        var settings = new PrettyPrintSettings
        {
            JsonOutputType = outputType,
        };

        PrettyPrint( this, writer, 0, settings );
    }

    private void PrettyPrint( JsonValue jsonval, TextWriter writer, int indent, PrettyPrintSettings settings )
    {
        var outputType = settings.JsonOutputType;

        if ( jsonval.IsObject() )
        {
            if ( jsonval.Child == null )
            {
                writer.Write( "{}" );
            }
            else
            {
                var newLines = !IsFlat( jsonval ) || ( jsonval.Size > 6 );

                writer.Write( newLines ? "{\n" : "{ " );

                for ( var child = jsonval.Child; child != null; child = child.Next )
                {
                    if ( newLines )
                    {
                        Indent( indent, writer );
                    }

                    writer.Write( outputType.QuoteName( child.Name ) );
                    writer.Write( ": " );

                    PrettyPrint( child, writer, indent + 1, settings );

                    if ( ( !newLines || ( outputType != JsonOutputType.Minimal ) )
                         && ( child.Next != null ) )
                    {
                        writer.Write( ',' );
                    }

                    writer.Write( newLines ? '\n' : ' ' );
                }

                if ( newLines )
                {
                    Indent( indent - 1, writer );
                }

                writer.Write( '}' );
            }
        }
        else if ( jsonval.IsArray() )
        {
            if ( jsonval.Child == null )
            {
                writer.Write( "[]" );
            }
            else
            {
                var newLines = !IsFlat( jsonval );

                writer.Write( newLines ? "[\n" : "[ " );

                for ( var child = jsonval.Child; child != null; child = child.Next )
                {
                    if ( newLines )
                    {
                        Indent( indent, writer );
                    }

                    PrettyPrint( child, writer, indent + 1, settings );

                    if ( ( !newLines || ( outputType != JsonOutputType.Minimal ) )
                         && ( child.Next != null ) )
                    {
                        writer.Write( ',' );
                    }

                    writer.Write( newLines ? '\n' : ' ' );
                }

                if ( newLines )
                {
                    Indent( indent - 1, writer );
                }

                writer.Write( ']' );
            }
        }
        else if ( jsonval.IsString() )
        {
            writer.Write( outputType.QuoteValue( jsonval.AsString() ) );
        }
        else if ( jsonval.IsDouble() )
        {
            var doubleValue = jsonval.AsDouble();
            var longValue   = jsonval.AsLong();

            writer.Write( Math.Abs( doubleValue - longValue ) < Number.FLOAT_TOLERANCE
                              ? longValue.ToString( CultureInfo.InvariantCulture )
                              : doubleValue.ToString( CultureInfo.InvariantCulture ) );
        }
        else if ( jsonval.IsLong() )
        {
            writer.Write( jsonval.AsLong().ToString() );
        }
        else if ( jsonval.IsBoolean() )
        {
            writer.Write( jsonval.AsBoolean().ToString() );
        }
        else if ( jsonval.IsNull() )
        {
            writer.Write( "null" );
        }
        else
        {
            throw new SerializationException( $"Unknown @object type: {jsonval}" );
        }
    }

    private bool IsFlat( JsonValue jsonval )
    {
        for ( var child = jsonval.Child; child != null; child = child.Next )
        {
            if ( child.IsObject() || child.IsArray() )
            {
                return false;
            }
        }

        return true;
    }

    private bool IsNumeric( JsonValue jsonval )
    {
        for ( var child = jsonval.Child; child != null; child = child.Next )
        {
            if ( !child.IsNumber() )
            {
                return false;
            }
        }

        return true;
    }

    private void Indent( int count, StringBuilder buffer )
    {
        for ( var i = 0; i < count; i++ )
        {
            buffer.Append( '\t' );
        }
    }

    private void Indent( int count, TextWriter buffer )
    {
        for ( var i = 0; i < count; i++ )
        {
            buffer.Write( '\t' );
        }
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
        }

        public void Remove()
        {
            if ( _current == null )
            {
                throw new InvalidOperationException( "Remove can only be called after Next." );
            }

            if ( _current.Prev == null )
            {
                _child = _current.Next;

                if ( _child != null )
                {
                    _child.Prev = null;
                }
            }
            else
            {
                _current.Prev.Next = _current.Next;

                if ( _current.Next != null )
                {
                    _current.Next.Prev = _current.Prev;
                }
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
        public JsonOutputType JsonOutputType { get; set; }

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