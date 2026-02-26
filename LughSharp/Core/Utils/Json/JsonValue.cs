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
using System.Globalization;
using System.IO;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Core.Maths;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Json;

namespace LughSharp.Core.Utils.Json;

[PublicAPI]
public class JsonValue : IEnumerator< JsonValue >
{
    public string?    Name        { get; set; } = string.Empty;
    public JsonValue? Child       { get; set; }
    public JsonValue? Last        { get; set; }
    public JsonValue? Parent      { get; set; }
    public int        Size        { get; set; }
    public ValueType  Type        { get; set; }
    public string?    StringValue { get; set; }
    public double     DoubleValue { get; set; }
    public long       LongValue   { get; set; }

    /// <summary>
    /// May be null. When changing this field the parent <see cref="Size"/>
    /// may need to be changed.
    /// </summary>
    public JsonValue? Next { get; set; }

    /// <summary>
    /// May be null. When changing this field the parent <see cref="Size"/>
    /// may need to be changed.
    /// </summary>
    public JsonValue? Prev { get; set; }

    // ========================================================================

    private bool _disposed = false;

    // ========================================================================

    public JsonValue( ValueType type )
    {
        this.Type = type;
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

    public JsonValue( double value, string stringValue )
    {
        Set( value, stringValue );
    }

    public JsonValue( long value, string stringValue )
    {
        Set( value, stringValue );
    }

    public JsonValue( bool value )
    {
        Set( value );
    }

    /// <summary>
    /// Creates a deep copy of the specific value, except <see cref="Parent"/>,
    /// <see cref="Next"/>, and <see cref="Prev"/> are null.
    /// </summary>
    public JsonValue( JsonValue value )
        : this( value, null, null )
    {
    }

    private JsonValue( JsonValue other, JsonValue? otherLast, JsonValue? parent )
    {
        Type        = other.Type;
        StringValue = other.StringValue;
        DoubleValue = other.DoubleValue;
        LongValue   = other.LongValue;
        Name        = other.Name;
        Parent      = parent;

        if ( other.Child != null )
        {
            Child = new JsonValue( other.Child, other.Last, this );
        }

        if ( other == otherLast )
        {
            parent?.Last = this;
        }

        if ( parent != null && other.Next != null )
        {
            Next = new JsonValue( other.Next, otherLast, parent )
            {
                Prev = this
            };
        }

        Size = other.Size;
    }

    public bool Has( string name )
    {
        return Get( name ) != null;
    }

    public JsonValue? Get( int index )
    {
        if ( index == Size - 1 )
        {
            return Last;
        }

        JsonValue? current = Child;

        while ( current != null && index > 0 )
        {
            index--;
            current = current.Next;
        }

        return current;
    }

    public JsonValue? Get( string name )
    {
        JsonValue? current = Child;

        while ( current != null
             && ( current.Name == null || !current.Name.Equals( name ) ) )
        {
            current = current.Next;
        }

        return current;
    }

    public JsonValue? GetIgnoreCase( string name )
    {
        JsonValue? current = Child;

        while ( current != null
             && ( current.Name == null || !current.Name.Equals( name, StringComparison.OrdinalIgnoreCase ) ) )
        {
            current = current.Next;
        }

        return current;
    }

    public JsonValue Require( int index )
    {
        JsonValue? current = Get( index );

        if ( current == null )
        {
            throw new ArgumentException( $"Child not found with index: {index}" );
        }

        return current;
    }

    public JsonValue Require( string name )
    {
        JsonValue? current = Get( name );

        if ( current == null )
        {
            throw new ArgumentException( $"Child not found with name: {name}" );
        }

        return current;
    }

    public JsonValue? Remove( int index )
    {
        JsonValue? child = Get( index );

        if ( child == null )
        {
            return null;
        }

        if ( Last == child )
        {
            Last = child.Prev;
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

    public JsonValue? Remove( string name )
    {
        JsonValue? child = Get( name );

        if ( child == null )
        {
            return null;
        }

        if ( Last == child )
        {
            Last = child.Prev;
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

    /// <summary>
    /// Removes this value from its parent.
    /// </summary>
    public void Remove()
    {
        Guard.Against.Null( Parent );

        if ( Parent.Last == this )
        {
            Parent.Last = Prev;
        }

        if ( Prev == null )
        {
            Parent.Child = Next;

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

        Parent.Size--;
    }

    /// <summary>
    /// Returns true if there are one or more children in the array or object.
    /// </summary>
    public bool NotEmpty()
    {
        return Size > 0;
    }

    /// <summary>
    /// Returns true if there are not children in the array or object.
    /// </summary>
    public bool IsEmpty()
    {
        return Size == 0;
    }

    public bool IsArray()
    {
        return Type == ValueType.ArrayType;
    }

    public bool IsObject()
    {
        return Type == ValueType.ObjectType;
    }

    public bool IsString()
    {
        return Type == ValueType.StringValue;
    }

    public bool IsNumber()
    {
        return Type is ValueType.DoubleValue or ValueType.LongValue;
    }

    public bool IsDouble()
    {
        return Type == ValueType.DoubleValue;
    }

    public bool IsLong()
    {
        return Type == ValueType.LongValue;
    }

    public bool IsBoolean()
    {
        return Type == ValueType.BooleanValue;
    }

    public bool IsNull()
    {
        return Type == ValueType.NullValue;
    }

    /** Returns true if this is not an array or object. */
    public bool IsValue()
    {
        switch ( Type )
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

    private bool IsFlat( JsonValue obj )
    {
        for ( JsonValue? child = obj.Child; child != null; child = child.Next )
        {
            if ( child.IsObject() || child.IsArray() )
            {
                return false;
            }
        }

        return true;
    }

    private bool IsNumeric( JsonValue obj )
    {
        for ( JsonValue? child = obj.Child; child != null; child = child.Next )
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
        for ( int i = 0; i < count; i++ )
        {
            buffer.Append( '\t' );
        }
    }

    private void Indent( int count, TextWriter writer )
    {
        for ( int i = 0; i < count; i++ )
        {
            writer.Write( '\t' );
        }
    }

    /// <summary>
    /// Returns this value as a string.
    /// </summary>
    /// <returns> May be null if this value is null. </returns>
    /// <exception cref="RuntimeException"> if this is an array or object. </exception>
    public string? AsString()
    {
        switch ( Type )
        {
            case ValueType.StringValue:
                return StringValue;

            case ValueType.DoubleValue:
                return StringValue ?? DoubleValue.ToString( CultureInfo.InvariantCulture );

            case ValueType.LongValue:
                return StringValue ?? LongValue.ToString();

            case ValueType.BooleanValue:
                return LongValue != 0 ? "true" : "false";

            case ValueType.NullValue:
                return null;
        }

        throw new RuntimeException( $"Value cannot be converted to string: {Type}" );
    }

    /// <summary>
    /// Returns this value as a float.
    /// </summary>
    /// <exception cref="RuntimeException"> if this is an array or object. </exception>
    public float AsFloat()
    {
        switch ( Type )
        {
            case ValueType.StringValue:
                return float.Parse( StringValue ?? "" );

            case ValueType.DoubleValue:
                return ( float )DoubleValue;

            case ValueType.LongValue:
                return LongValue;

            case ValueType.BooleanValue:
                return LongValue != 0 ? 1 : 0;
        }

        throw new RuntimeException( $"Value cannot be converted to float: {Type}" );
    }

    /// <summary>
    /// Returns this value as a double.
    /// </summary>
    /// <exception cref="RuntimeException"> if this is an array or object. </exception>
    public double AsDouble()
    {
        switch ( Type )
        {
            case ValueType.StringValue:
                return double.Parse( StringValue ?? "" );

            case ValueType.DoubleValue:
                return DoubleValue;

            case ValueType.LongValue:
                return LongValue;

            case ValueType.BooleanValue:
                return LongValue != 0 ? 1 : 0;
        }

        throw new RuntimeException( $"Value cannot be converted to double: {Type}" );
    }

    /// <summary>
    /// Returns this value as a long.
    /// </summary>
    /// <exception cref="RuntimeException"> if this is an array or object. </exception>
    public long AsLong()
    {
        switch ( Type )
        {
            case ValueType.StringValue:
                return long.Parse( StringValue ?? "" );

            case ValueType.DoubleValue:
                return ( long )DoubleValue;

            case ValueType.LongValue:
                return LongValue;

            case ValueType.BooleanValue:
                return LongValue != 0 ? 1 : 0;
        }

        throw new RuntimeException( $"Value cannot be converted to long: {Type}" );
    }

    /// <summary>
    /// Returns this value as an int.
    /// </summary>
    /// <exception cref="RuntimeException"> if this is an array or object. </exception>
    public int AsInt()
    {
        switch ( Type )
        {
            case ValueType.StringValue:
                return int.Parse( StringValue ?? "" );

            case ValueType.DoubleValue:
                return ( int )DoubleValue;

            case ValueType.LongValue:
                return ( int )LongValue;

            case ValueType.BooleanValue:
                return LongValue != 0 ? 1 : 0;
        }

        throw new RuntimeException( $"Value cannot be converted to int: {Type}" );
    }

    /// <summary>
    /// Returns this value as a bool.
    /// </summary>
    /// <exception cref="RuntimeException"> if this is an array or object. </exception>
    public bool AsBoolean()
    {
        switch ( Type )
        {
            case ValueType.StringValue:
                return StringValue != null
                    && StringValue.Equals( "true", StringComparison.OrdinalIgnoreCase );

            case ValueType.DoubleValue:
                return DoubleValue != 0;

            case ValueType.LongValue:
            case ValueType.BooleanValue:
                return LongValue != 0;
        }

        throw new RuntimeException( $"Value cannot be converted to bool: {Type}" );
    }

    public byte AsByte()
    {
        switch ( Type )
        {
            case ValueType.StringValue:
                return byte.Parse( StringValue ?? "" );

            case ValueType.DoubleValue:
                return ( byte )DoubleValue;

            case ValueType.LongValue:
                return ( byte )LongValue;

            case ValueType.BooleanValue:
                return ( byte )( LongValue != 0 ? 1 : 0 );
        }

        throw new RuntimeException( $"Value cannot be converted to byte: {Type}" );
    }

    public short AsShort()
    {
        switch ( Type )
        {
            case ValueType.StringValue:
                return short.Parse( StringValue ?? "" );

            case ValueType.DoubleValue:
                return ( short )DoubleValue;

            case ValueType.LongValue:
                return ( short )LongValue;

            case ValueType.BooleanValue:
                return ( short )( LongValue != 0 ? 1 : 0 );
        }

        throw new RuntimeException( $"Value cannot be converted to short: {Type}" );
    }

    public char AsChar()
    {
        switch ( Type )
        {
            case ValueType.StringValue:
                return ( char )( StringValue is { Length: > 0 } ? StringValue[ 0 ] : 0 );

            case ValueType.DoubleValue:
                return ( char )DoubleValue;

            case ValueType.LongValue:
                return ( char )LongValue;

            case ValueType.BooleanValue:
                return ( char )( LongValue != 0 ? 1 : 0 );
        }

        throw new RuntimeException( $"Value cannot be converted to char: {Type}" );
    }

    /// <summary>
    /// Returns the children of this value as a newly allocated string array.
    /// </summary>
    /// <exception cref="RuntimeException"> if this is not an array. </exception>
    public string?[] AsStringArray()
    {
        if ( Type != ValueType.ArrayType )
        {
            throw new RuntimeException( $"Value is not an array: {Type}" );
        }

        string?[] array = new string[ Size ];
        int       i     = 0;

        for ( JsonValue? value = Child; value != null; value = value.Next, i++ )
        {
            string? v;

            switch ( value.Type )
            {
                case ValueType.StringValue:
                    v = value.StringValue ?? "";

                    break;

                case ValueType.DoubleValue:
                    v = StringValue ?? value.DoubleValue.ToString( CultureInfo.InvariantCulture );

                    break;

                case ValueType.LongValue:
                    v = StringValue ?? value.LongValue.ToString( CultureInfo.InvariantCulture );

                    break;

                case ValueType.BooleanValue:
                    v = value.LongValue != 0 ? "true" : "false";

                    break;

                case ValueType.NullValue:
                    v = null;

                    break;

                default:
                    throw new RuntimeException( $"Value cannot be converted to string: {value.Type}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    public float[] AsFloatArray()
    {
        if ( Type != ValueType.ArrayType )
        {
            throw new RuntimeException( $"Value is not an array: {Type}" );
        }

        float[] array = new float[ Size ];
        int     i     = 0;

        for ( JsonValue? value = Child; value != null; value = value.Next, i++ )
        {
            float v;

            switch ( value.Type )
            {
                case ValueType.StringValue:
                    v = float.Parse( value.StringValue ?? "" );

                    break;

                case ValueType.DoubleValue:
                    v = ( float )value.DoubleValue;

                    break;

                case ValueType.LongValue:
                    v = value.LongValue;

                    break;

                case ValueType.BooleanValue:
                    v = value.LongValue != 0 ? 1 : 0;

                    break;

                default:
                    throw new RuntimeException( $"Value cannot be converted to float: {value.Type}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    public double[] AsDoubleArray()
    {
        if ( Type != ValueType.ArrayType )
        {
            throw new RuntimeException( $"Value is not an array: {Type}" );
        }

        double[] array = new double[ Size ];
        int      i     = 0;

        for ( JsonValue? value = Child; value != null; value = value.Next, i++ )
        {
            double v;

            switch ( value.Type )
            {
                case ValueType.StringValue:
                    v = double.Parse( value.StringValue ?? "" );

                    break;

                case ValueType.DoubleValue:
                    v = value.DoubleValue;

                    break;

                case ValueType.LongValue:
                    v = value.LongValue;

                    break;

                case ValueType.BooleanValue:
                    v = value.LongValue != 0 ? 1 : 0;

                    break;

                default:
                    throw new RuntimeException( $"Value cannot be converted to double: {value.Type}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    public long[] AsLongArray()
    {
        if ( Type != ValueType.ArrayType )
        {
            throw new RuntimeException( $"Value is not an array: {Type}" );
        }

        long[] array = new long[ Size ];
        int    i     = 0;

        for ( JsonValue? value = Child; value != null; value = value.Next, i++ )
        {
            long v;

            switch ( value.Type )
            {
                case ValueType.StringValue:
                    v = long.Parse( value.StringValue ?? "" );

                    break;

                case ValueType.DoubleValue:
                    v = ( long )value.DoubleValue;

                    break;

                case ValueType.LongValue:
                    v = value.LongValue;

                    break;

                case ValueType.BooleanValue:
                    v = value.LongValue != 0 ? 1 : 0;

                    break;

                default:
                    throw new RuntimeException( $"Value cannot be converted to long: {value.Type}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    public int[] AsIntArray()
    {
        if ( Type != ValueType.ArrayType )
        {
            throw new RuntimeException( $"Value is not an array: {Type}" );
        }

        int[] array = new int[ Size ];
        int   i     = 0;

        for ( JsonValue? value = Child; value != null; value = value.Next, i++ )
        {
            int v;

            switch ( value.Type )
            {
                case ValueType.StringValue:
                    v = int.Parse( value.StringValue ?? "" );

                    break;

                case ValueType.DoubleValue:
                    v = ( int )value.DoubleValue;

                    break;

                case ValueType.LongValue:
                    v = ( int )value.LongValue;

                    break;

                case ValueType.BooleanValue:
                    v = value.LongValue != 0 ? 1 : 0;

                    break;

                default:
                    throw new RuntimeException( $"Value cannot be converted to int: {value.Type}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    /** Returns the children of this value as a newly allocated bool array.
     * @throws RuntimeException if this is not an array. */
    public bool[] AsBooleanArray()
    {
        if ( Type != ValueType.ArrayType )
        {
            throw new RuntimeException( $"Value is not an array: {Type}" );
        }

        bool[] array = new bool[ Size ];
        int    i     = 0;

        for ( JsonValue? value = Child; value != null; value = value.Next, i++ )
        {
            bool v;

            switch ( value.Type )
            {
                case ValueType.StringValue:
                    v = bool.Parse( value.StringValue ?? "" );

                    break;

                case ValueType.DoubleValue:
                    v = value.DoubleValue == 0;

                    break;

                case ValueType.LongValue:
                    v = value.LongValue == 0;

                    break;

                case ValueType.BooleanValue:
                    v = value.LongValue != 0;

                    break;

                default:
                    throw new RuntimeException( $"Value cannot be converted to bool: {value.Type}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    public byte[] AsByteArray()
    {
        if ( Type != ValueType.ArrayType )
        {
            throw new RuntimeException( $"Value is not an array: {Type}" );
        }

        byte[] array = new byte[ Size ];
        int    i     = 0;

        for ( JsonValue? value = Child; value != null; value = value.Next, i++ )
        {
            byte v;

            switch ( value.Type )
            {
                case ValueType.StringValue:
                    v = byte.Parse( value.StringValue ?? "" );

                    break;

                case ValueType.DoubleValue:
                    v = ( byte )value.DoubleValue;

                    break;

                case ValueType.LongValue:
                    v = ( byte )value.LongValue;

                    break;

                case ValueType.BooleanValue:
                    v = ( byte )( value.LongValue != 0 ? 1 : 0 );

                    break;

                default:
                    throw new RuntimeException( $"Value cannot be converted to byte: {value.Type}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    public short[] AsShortArray()
    {
        if ( Type != ValueType.ArrayType )
        {
            throw new RuntimeException( $"Value is not an array: {Type}" );
        }

        short[] array = new short[ Size ];
        int     i     = 0;

        for ( JsonValue? value = Child; value != null; value = value.Next, i++ )
        {
            short v;

            switch ( value.Type )
            {
                case ValueType.StringValue:
                    v = short.Parse( value.StringValue ?? "" );

                    break;

                case ValueType.DoubleValue:
                    v = ( short )value.DoubleValue;

                    break;

                case ValueType.LongValue:
                    v = ( short )value.LongValue;

                    break;

                case ValueType.BooleanValue:
                    v = ( short )( value.LongValue != 0 ? 1 : 0 );

                    break;

                default:
                    throw new RuntimeException( $"Value cannot be converted to short: {value.Type}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    public char[] AsCharArray()
    {
        if ( Type != ValueType.ArrayType )
        {
            throw new RuntimeException( $"Value is not an array: {Type}" );
        }

        char[] array = new char[ Size ];
        int    i     = 0;

        for ( JsonValue? value = Child; value != null; value = value.Next, i++ )
        {
            char v;

            switch ( value.Type )
            {
                case ValueType.StringValue:
                    v = ( char )( value.StringValue is { Length: > 0 } ? value.StringValue[ 0 ] : 0 );

                    break;

                case ValueType.DoubleValue:
                    v = ( char )value.DoubleValue;

                    break;

                case ValueType.LongValue:
                    v = ( char )value.LongValue;

                    break;

                case ValueType.BooleanValue:
                    v = ( char )( value.LongValue != 0 ? 1 : 0 );

                    break;

                default:
                    throw new RuntimeException( $"Value cannot be converted to char: {value.Type}" );
            }

            array[ i ] = v;
        }

        return array;
    }

    /// <summary>
    /// Returns true if a child with the specified name exists and has a child.
    /// </summary>
    public bool HasChild( string name )
    {
        return GetChild( name ) != null;
    }

    /// <summary>
    /// Finds the child with the specified name and returns its first child.
    /// </summary>
    /// <returns> May be null. </returns>
    public JsonValue? GetChild( string name )
    {
        JsonValue? child = Get( name );

        return child?.Child;
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a string.
    /// Returns defaultValue if not found.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="defaultValue"> May be null. </param>
    public string? GetString( string name, string? defaultValue )
    {
        JsonValue? child = Get( name );

        return ( child == null || !child.IsValue() || child.IsNull() )
            ? defaultValue
            : child.AsString();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a float.
    /// Returns defaultValue if not found.
    /// </summary>
    public float GetFloat( string name, float defaultValue )
    {
        JsonValue? child = Get( name );

        return ( child == null || !child.IsValue() || child.IsNull() )
            ? defaultValue
            : child.AsFloat();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a double.
    /// Returns defaultValue if not found.
    /// </summary>
    public double GetDouble( string name, double defaultValue )
    {
        JsonValue? child = Get( name );

        return ( child == null || !child.IsValue() || child.IsNull() )
            ? defaultValue
            : child.AsDouble();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a long.
    /// Returns defaultValue if not found.
    /// </summary>
    public long GetLong( string name, long defaultValue )
    {
        JsonValue? child = Get( name );

        return ( child == null || !child.IsValue() || child.IsNull() )
            ? defaultValue
            : child.AsLong();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as an int.
    /// Returns defaultValue if not found.
    /// </summary>
    public int GetInt( string name, int defaultValue )
    {
        JsonValue? child = Get( name );

        return ( child == null || !child.IsValue() || child.IsNull() )
            ? defaultValue
            : child.AsInt();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a bool.
    /// Returns defaultValue if not found.
    /// </summary>
    public bool GetBoolean( string name, bool defaultValue )
    {
        JsonValue? child = Get( name );

        return ( child == null || !child.IsValue() || child.IsNull() )
            ? defaultValue
            : child.AsBoolean();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a byte.
    /// Returns defaultValue if not found.
    /// </summary>
    public byte GetByte( string name, byte defaultValue )
    {
        JsonValue? child = Get( name );

        return ( child == null || !child.IsValue() || child.IsNull() )
            ? defaultValue
            : child.AsByte();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a short.
    /// Returns defaultValue if not found.
    /// </summary>
    public short GetShort( string name, short defaultValue )
    {
        JsonValue? child = Get( name );

        return ( child == null || !child.IsValue() || child.IsNull() )
            ? defaultValue
            : child.AsShort();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a char.
    /// Returns defaultValue if not found.
    /// </summary>
    public char GetChar( string name, char defaultValue )
    {
        JsonValue? child = Get( name );

        return ( child == null || !child.IsValue() || child.IsNull() )
            ? defaultValue
            : child.AsChar();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a string.
    /// Returns defaultValue if not found.
    /// </summary>
    public string? GetString( string name )
    {
        JsonValue? child = Get( name );

        if ( child == null )
        {
            throw new RuntimeException( $"Named value not found: {name}" );
        }

        return child.AsString();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a float.
    /// Returns defaultValue if not found.
    /// </summary>
    public float GetFloat( string name )
    {
        JsonValue? child = Get( name );

        if ( child == null )
        {
            throw new RuntimeException( $"Named value not found: {name}" );
        }

        return child.AsFloat();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a double.
    /// Returns defaultValue if not found.
    /// </summary>
    public double GetDouble( string name )
    {
        JsonValue? child = Get( name );

        if ( child == null )
        {
            throw new RuntimeException( $"Named value not found: {name}" );
        }

        return child.AsDouble();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a long.
    /// Returns defaultValue if not found.
    /// </summary>
    public long GetLong( string name )
    {
        JsonValue? child = Get( name );

        if ( child == null )
        {
            throw new RuntimeException( $"Named value not found: {name}" );
        }

        return child.AsLong();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a int.
    /// Returns defaultValue if not found.
    /// </summary>
    public int GetInt( string name )
    {
        JsonValue? child = Get( name );

        if ( child == null )
        {
            throw new RuntimeException( $"Named value not found: {name}" );
        }

        return child.AsInt();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a bool.
    /// Returns defaultValue if not found.
    /// </summary>
    public bool GetBoolean( string name )
    {
        JsonValue? child = Get( name );

        if ( child == null )
        {
            throw new RuntimeException( $"Named value not found: {name}" );
        }

        return child.AsBoolean();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a byte.
    /// Returns defaultValue if not found.
    /// </summary>
    public byte GetByte( string name )
    {
        JsonValue? child = Get( name );

        if ( child == null )
        {
            throw new RuntimeException( $"Named value not found: {name}" );
        }

        return child.AsByte();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a short.
    /// Returns defaultValue if not found.
    /// </summary>
    public short GetShort( string name )
    {
        JsonValue? child = Get( name );

        if ( child == null )
        {
            throw new RuntimeException( $"Named value not found: {name}" );
        }

        return child.AsShort();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a char.
    /// Returns defaultValue if not found.
    /// </summary>
    public char GetChar( string name )
    {
        JsonValue? child = Get( name );

        if ( child == null )
        {
            throw new RuntimeException( $"Named value not found: {name}" );
        }

        return child.AsChar();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a string.
    /// Returns defaultValue if not found.
    /// </summary>
    public string? GetString( int index )
    {
        JsonValue? child = Get( index );

        if ( child == null )
        {
            throw new RuntimeException( $"Indexed value not found: {Name}" );
        }

        return child.AsString();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a float.
    /// Returns defaultValue if not found.
    /// </summary>
    public float GetFloat( int index )
    {
        JsonValue? child = Get( index );

        if ( child == null )
        {
            throw new RuntimeException( $"Indexed value not found: {Name}" );
        }

        return child.AsFloat();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a double.
    /// Returns defaultValue if not found.
    /// </summary>
    public double GetDouble( int index )
    {
        JsonValue? child = Get( index );

        if ( child == null )
        {
            throw new RuntimeException( $"Indexed value not found: {Name}" );
        }

        return child.AsDouble();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a long.
    /// Returns defaultValue if not found.
    /// </summary>
    public long GetLong( int index )
    {
        JsonValue? child = Get( index );

        if ( child == null )
        {
            throw new RuntimeException( $"Indexed value not found: {Name}" );
        }

        return child.AsLong();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a int.
    /// Returns defaultValue if not found.
    /// </summary>
    public int GetInt( int index )
    {
        JsonValue? child = Get( index );

        if ( child == null )
        {
            throw new RuntimeException( $"Indexed value not found: {Name}" );
        }

        return child.AsInt();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a by bool.
    /// Returns defaultValue if not found.
    /// </summary>
    public bool GetBoolean( int index )
    {
        JsonValue? child = Get( index );

        if ( child == null )
        {
            throw new RuntimeException( $"Indexed value not found: {Name}" );
        }

        return child.AsBoolean();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a byte.
    /// Returns defaultValue if not found.
    /// </summary>
    public byte GetByte( int index )
    {
        JsonValue? child = Get( index );

        if ( child == null )
        {
            throw new RuntimeException( $"Indexed value not found: {Name}" );
        }

        return child.AsByte();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a short.
    /// Returns defaultValue if not found.
    /// </summary>
    public short GetShort( int index )
    {
        JsonValue? child = Get( index );

        if ( child == null )
        {
            throw new RuntimeException( $"Indexed value not found: {Name}" );
        }

        return child.AsShort();
    }

    /// <summary>
    /// Finds the child with the specified name and returns it as a char.
    /// Returns defaultValue if not found.
    /// </summary>
    public char GetChar( int index )
    {
        JsonValue? child = Get( index );

        if ( child == null )
        {
            throw new RuntimeException( $"Indexed value not found: {Name}" );
        }

        return child.AsChar();
    }

    /// <summary>
    /// Sets the name of the specified value and replaces an existing child with
    /// the same name, else adds it after the last child.
    /// </summary>
    public void SetChild( string name, JsonValue value )
    {
        if ( name == null )
        {
            throw new ArgumentException( "name cannot be null." );
        }

        value.Name = name;
        SetChild( value );
    }

    /// <summary>
    /// Replaces an existing child with the same name as the specified
    /// value. If an existing child does not exist, it adds it after
    /// the last child.
    /// </summary>
    public void SetChild( JsonValue value )
    {
        string? name = value.Name;

        if ( name == null )
        {
            throw new RuntimeException( $"An object child requires a name: {value}" );
        }

        JsonValue? current = Child;

        while ( current != null )
        {
            if ( current.Name != null && current.Name.Equals( name ) )
            {
                current.Replace( value );

                return;
            }

            current = current.Next;
        }

        AddChild( value );
    }

    /// <summary>
    /// Replaces this value in its parent with the specified value.
    /// </summary>
    public void Replace( JsonValue value )
    {
        if ( Parent?.Last == this )
        {
            Parent.Last = value;
        }

        if ( Prev != null )
        {
            Prev.Next = value;
        }
        else
        {
            Parent?.Child = value;
        }

        value.Prev = Prev;
        value.Next = Next;

        if ( Next != null )
        {
            Next.Prev = value;
        }

        value.Parent = Parent;
        Prev         = null;
        Next         = null;
        Parent       = null;
    }

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
    /// <exception cref="RuntimeException">
    /// if this is an object and the specified child's name is null.
    /// </exception>
    public void AddChild( JsonValue value )
    {
        if ( Type == ValueType.ObjectType && value.Name == null )
        {
            throw new RuntimeException( $"An object child requires a name: {value}" );
        }

        value.Parent = this;
        value.Next   = null;

        if ( Child == null )
        {
            value.Prev = null;
            Child      = value;
        }
        else
        {
            Last?.Next = value;
            value.Prev = Last;
        }

        Last = value;
        Size++;
    }

    /// <summary>
    /// Adds the specified value as the first child.
    /// </summary>
    /// <exception cref="RuntimeException">
    /// if this is an object and the specified child's name is null.
    /// </exception>
    public void AddChildFirst( JsonValue value )
    {
        if ( Type == ValueType.ObjectType && value.Name == null )
        {
            throw new RuntimeException( $"An object child requires a name: {value}" );
        }

        value.Parent = this;
        value.Next   = Child;
        value.Prev   = null;

        if ( Child == null )
        {
            Child = value;
            Last  = value;
        }
        else
        {
            Child.Prev = value;
            Child      = value;
        }

        Size++;
    }

    /// <summary>
    /// Sets the type and value to the specified JsonValue.
    /// </summary>
    public void Set( JsonValue value )
    {
        Type        = value.Type;
        StringValue = value.StringValue;
        DoubleValue = value.DoubleValue;
        LongValue   = value.LongValue;
    }

    public void Set( string? value )
    {
        StringValue = value;
        Type        = value == null ? ValueType.NullValue : ValueType.StringValue;
    }

    public void SetNull()
    {
        StringValue = null;
        Type        = ValueType.NullValue;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="stringValue">
    /// May be null if the string representation is the string value of the double (eg, no leading zeros).
    /// </param>
    public void Set( double value, string? stringValue )
    {
        DoubleValue = value;
        LongValue   = ( long )value;
        StringValue = stringValue;
        Type        = ValueType.DoubleValue;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="stringValue">
    /// May be null if the string representation is the string value of the long (eg, no leading zeros).
    /// </param>
    public void Set( long value, string? stringValue )
    {
        LongValue   = value;
        DoubleValue = value;
        StringValue = stringValue;
        Type        = ValueType.LongValue;
    }

    public void Set( bool value )
    {
        LongValue = value ? 1 : 0;
        Type      = ValueType.BooleanValue;
    }

    public bool EqualsString( string value )
    {
        return string.Equals( AsString(), value, StringComparison.OrdinalIgnoreCase );
    }

    public bool NameEquals( string value )
    {
        return string.Equals( Name, value, StringComparison.OrdinalIgnoreCase );
    }

    public string? ToJson( JsonOutputType outputType )
    {
        if ( IsValue() )
        {
            return AsString();
        }

        StringWriter writer = new StringWriter( new StringBuilder( 512 ) );

        try
        {
            ToJson( outputType, writer );
        }
        catch ( IOException ex )
        {
            throw new RuntimeException( ex );
        }

        return writer.ToString();
    }

    public void ToJson( JsonOutputType outputType, StringWriter writer )
    {
        if ( IsObject() )
        {
            writer.Write( '{' );

            for ( JsonValue? child = this.Child; child != null; child = child.Next )
            {
                writer.Write( JsonOutput.QuoteName( child.Name, outputType ) );
                writer.Write( ':' );
                child.ToJson( outputType, writer );

                if ( child.Next != null )
                {
                    writer.Write( ',' );
                }
            }

            writer.Write( '}' );
        }
        else if ( IsArray() )
        {
            writer.Write( '[' );

            for ( JsonValue? child = this.Child; child != null; child = child.Next )
            {
                child.ToJson( outputType, writer );

                if ( child.Next != null )
                {
                    writer.Write( ',' );
                }
            }

            writer.Write( ']' );
        }
        else if ( IsString() )
        {
            writer.Write( JsonOutput.QuoteValue( AsString(), outputType ) );
        }
        else if ( IsDouble() )
        {
            double doubleValue = AsDouble();
            long   longValue   = AsLong();

            writer.Write( Math.Abs( doubleValue - longValue ) < NumberUtils.FLOAT_TOLERANCE
                              ? LongValue.ToString()
                              : DoubleValue.ToString( CultureInfo.InvariantCulture ) );
        }
        else if ( IsLong() )
        {
            writer.Write( AsLong().ToString() );
        }
        else if ( IsBoolean() )
        {
            writer.Write( AsBoolean() ? "true" : "false" );
        }
        else if ( IsNull() )
        {
            writer.Write( "null" );
        }
        else
        {
            throw new SerializationException( $"Unknown object type: {this}" );
        }
    }

    public override string ToString()
    {
        if ( IsValue() )
        {
            string? str = Name == null ? AsString() : $"{Name}: {AsString()}";

            return str ?? "null";
        }

        return ( Name == null ? "" : $"{Name}: " ) + PrettyPrint( JsonOutputType.Minimal, 0 );
    }

    /// <summary>
    /// Returns a human readable string representing the path from
    /// the root of the JSON object graph to this value.
    /// </summary>
    public string Trace()
    {
        if ( Parent == null )
        {
            if ( Type == ValueType.ArrayType )
            {
                return "[]";
            }

            if ( Type == ValueType.ObjectType )
            {
                return "{}";
            }

            return "";
        }

        string trace;

        if ( Parent.Type == ValueType.ArrayType )
        {
            trace = "[]";

            int i = 0;

            for ( JsonValue? child = Parent.Child; child != null; child = child.Next, i++ )
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
        PrettyPrintSettings settings = new PrettyPrintSettings
        {
            OutputType        = outputType,
            SingleLineColumns = singleLineColumns
        };

        return PrettyPrint( settings );
    }

    public string PrettyPrint( PrettyPrintSettings settings )
    {
        StringBuilder buffer = new StringBuilder( 512 );
        PrettyPrint( this, buffer, 0, settings );

        return buffer.ToString();
    }

    private void PrettyPrint( JsonValue obj, StringBuilder buffer, int indent, PrettyPrintSettings settings )
    {
        JsonOutputType outputType = settings.OutputType;

        if ( obj.IsObject() )
        {
            if ( obj.Child == null )
            {
                buffer.Append( "{}" );
            }
            else
            {
                bool newLines = !IsFlat( obj );
                int  start    = buffer.Length;

                while ( true )
                {
                    buffer.Append( newLines ? "{\n" : "{ " );

                    for ( JsonValue? child = obj.Child; child != null; child = child.Next )
                    {
                        if ( newLines )
                        {
                            Indent( indent, buffer );
                        }

                        buffer.Append( JsonOutput.QuoteName( child.Name, outputType ) );
                        buffer.Append( ": " );
                        PrettyPrint( child, buffer, indent + 1, settings );

                        if ( ( !newLines || outputType != JsonOutputType.Minimal ) && child.Next != null )
                        {
                            buffer.Append( ',' );
                        }

                        buffer.Append( newLines ? '\n' : ' ' );

                        if ( !newLines && buffer.Length - start > settings.SingleLineColumns )
                        {
                            buffer.Length = start;
                            newLines      = true;

                            goto outer;
                        }
                    }

                    break;
                }

            outer:

                if ( newLines )
                {
                    Indent( indent - 1, buffer );
                }

                buffer.Append( '}' );
            }
        }
        else if ( obj.IsArray() )
        {
            if ( obj.Child == null )
            {
                buffer.Append( "[]" );
            }
            else
            {
                bool newLines = !IsFlat( obj );
                bool wrap     = settings.WrapNumericArrays || !IsNumeric( obj );
                int  start    = buffer.Length;

                while ( true )
                {
                    buffer.Append( newLines ? "[\n" : "[ " );

                    for ( JsonValue? child = obj.Child; child != null; child = child.Next )
                    {
                        if ( newLines )
                        {
                            Indent( indent, buffer );
                        }

                        PrettyPrint( child, buffer, indent + 1, settings );

                        if ( ( !newLines || outputType != JsonOutputType.Minimal ) && child.Next != null )
                        {
                            buffer.Append( ',' );
                        }

                        buffer.Append( newLines ? '\n' : ' ' );

                        if ( wrap && !newLines && buffer.Length - start > settings.SingleLineColumns )
                        {
                            buffer.Length = start;
                            newLines      = true;

                            goto outer;
                        }
                    }

                    break;
                }

            outer:

                if ( newLines )
                {
                    Indent( indent - 1, buffer );
                }

                buffer.Append( ']' );
            }
        }
        else if ( obj.IsString() )
        {
            buffer.Append( JsonOutput.QuoteValue( obj.AsString(), outputType ) );
        }
        else if ( obj.IsDouble() )
        {
            double doubleValue = obj.AsDouble();
            long   longValue   = obj.AsLong();
            buffer.Append( Math.Abs( doubleValue - longValue ) < NumberUtils.FLOAT_TOLERANCE
                               ? longValue
                               : doubleValue );
        }
        else if ( obj.IsLong() )
        {
            buffer.Append( obj.AsLong() );
        }
        else if ( obj.IsBoolean() )
        {
            buffer.Append( obj.AsBoolean() );
        }
        else if ( obj.IsNull() )
        {
            buffer.Append( "null" );
        }
        else
        {
            throw new SerializationException( $"Unknown object type: {obj}" );
        }
    }

    /// <summary>
    /// More efficient than <see cref="PrettyPrint(PrettyPrintSettings)"/> but
    /// <see cref="PrettyPrintSettings.SingleLineColumns"/> and
    /// <see cref="PrettyPrintSettings.WrapNumericArrays"/> are not supported.
    /// </summary>
    public void PrettyPrint( JsonOutputType outputType, TextWriter writer )
    {
        var settings = new PrettyPrintSettings()
        {
            OutputType = outputType,
        };

        PrettyPrint( this, writer, 0, settings );
    }

    private void PrettyPrint( JsonValue obj, TextWriter writer, int indent, PrettyPrintSettings settings )
    {
        JsonOutputType outputType = settings.OutputType;

        if ( obj.IsObject() )
        {
            if ( obj.Child == null )
            {
                writer.Write( "{}" );
            }
            else
            {
                bool newLines = !IsFlat( obj ) || obj.Size > 6;

                writer.Write( newLines ? "{\n" : "{ " );

                for ( JsonValue? child = obj.Child; child != null; child = child.Next )
                {
                    if ( newLines )
                    {
                        Indent( indent, writer );
                    }

                    writer.Write( JsonOutput.QuoteName( child.Name, outputType ) );
                    writer.Write( ": " );

                    PrettyPrint( child, writer, indent + 1, settings );

                    if ( ( !newLines || outputType != JsonOutputType.Minimal ) && child.Next != null )
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
        else if ( obj.IsArray() )
        {
            if ( obj.Child == null )
            {
                writer.Write( "[]" );
            }
            else
            {
                bool newLines = !IsFlat( obj );

                writer.Write( newLines ? "[\n" : "[ " );

                int i = 0;

                for ( JsonValue? child = obj.Child; child != null; child = child.Next )
                {
                    if ( newLines )
                    {
                        Indent( indent, writer );
                    }

                    PrettyPrint( child, writer, indent + 1, settings );

                    if ( ( !newLines || outputType != JsonOutputType.Minimal ) && child.Next != null )
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
        else if ( obj.IsString() )
        {
            writer.Write( JsonOutput.QuoteValue( obj.AsString(), outputType ) );
        }
        else if ( obj.IsDouble() )
        {
            double doubleValue = obj.AsDouble();
            long   longValue   = obj.AsLong();
            
            writer.Write( Math.Abs( doubleValue - longValue ) < NumberUtils.FLOAT_TOLERANCE
                              ? longValue.ToString()
                              : doubleValue.ToString( CultureInfo.InvariantCulture ) );
        }
        else if ( object.isLong() )
        {
            writer.write( Long.toString( object.AsLong() ) );
        }
        else if ( obj.IsBoolean() )
        {
            writer.Write( obj.AsBoolean().ToString() );
        }
        else if ( obj.IsNull() )
        {
            writer.Write( "null" );
        }
        else
        {
            throw new SerializationException( $"Unknown object type: {obj}" );
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Advances the enumerator to the next element of the collection.
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">
    /// The collection was modified after the enumerator was created.
    /// </exception>
    /// <returns>
    /// <c>true</c> if the enumerator was successfully advanced to the next element,
    /// <c>false</c> if the enumerator has passed the end of the collection.
    /// </returns>
    public bool MoveNext()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Sets the enumerator to its initial position, which is before the first
    /// element in the collection.
    /// </summary>
    /// <exception cref="T:System.InvalidOperationException">
    /// The collection was modified after the enumerator was created.
    /// </exception>
    /// <exception cref="T:System.NotSupportedException">
    /// The enumerator does not support being reset.
    /// </exception>
    public void Reset()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    /// <returns>The element in the collection at the current position of the enumerator.</returns>
    public JsonValue Current { get; set; }

    /// <summary>
    /// Gets the element in the collection at the current position of the enumerator.
    /// </summary>
    /// <returns>The element in the collection at the current position of the enumerator.</returns>
    object? IEnumerator.Current { get; }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing,
    /// or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    protected void Dispose( bool disposing )
    {
        if ( !_disposed )
        {
            if ( disposing )
            {
            }

            _disposed = true;
        }
    }

    // ========================================================================
    // ========================================================================

    public enum ValueType
    {
        ObjectType,
        ArrayType,
        StringValue,
        DoubleValue,
        LongValue,
        BooleanValue,
        NullValue
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class PrettyPrintSettings
    {
        public JsonOutputType OutputType { get; set; }

        /// <summary>
        /// If an object on a single line fits this many columns, it won't wrap.
        /// </summary>
        public int SingleLineColumns { get; set; }

        /// <summary>
        /// Arrays of floats won't wrap.
        /// </summary>
        public bool WrapNumericArrays { get; set; }
    }
}

// ============================================================================
// ============================================================================