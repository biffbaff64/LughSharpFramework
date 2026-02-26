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
using System.Globalization;
using System.IO;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Utils.Json;

[PublicAPI]
public class JsonValue
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

    public JsonValue( ValueType type )
    {
        this.Type = type;
    }

    public JsonValue( string? value )
    {
        set( value );
    }

    public JsonValue( double value )
    {
        set( value, null );
    }

    public JsonValue( long value )
    {
        set( value, null );
    }

    public JsonValue( double value, string stringValue )
    {
        set( value, stringValue );
    }

    public JsonValue( long value, string stringValue )
    {
        set( value, stringValue );
    }

    public JsonValue( bool value )
    {
        set( value );
    }

    /** Creates a deep copy of the specific value, except {@link #parent()}, {@link #next()}, and {@link #prev()} are null. */
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
                return ( byte )(LongValue != 0 ? 1 : 0);
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
                return ( char )(StringValue is { Length: > 0 } ? StringValue[ 0 ] : 0 );

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
        int      i     = 0;

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
                    v = ( char )(value.StringValue is { Length: > 0 } ? value.StringValue[ 0 ] : 0 );

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