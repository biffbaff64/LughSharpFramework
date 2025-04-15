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

using System.Globalization;

using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Utils.Json;

public partial class JsonValue
{
    /// <summary>
    /// Returns this value as a string.
    /// </summary>
    /// <returns> May be null if this value is null. </returns>
    /// <exception cref="InvalidOperationException"> if this an array or object. </exception>
    public string? AsString()
    {
        return ValueType switch
        {
            ValueTypes.StringValue  => StringValue,
            ValueTypes.DoubleValue  => StringValue ?? DoubleValue.ToString(),
            ValueTypes.LongValue    => StringValue ?? LongValue.ToString(),
            ValueTypes.BooleanValue => LongValue != 0 ? "true" : "false",
            ValueTypes.NullValue    => null,

            var _ => throw new InvalidOperationException( $"Value cannot be converted to string: {ValueType}" ),
        };
    }

    /// <summary>
    /// Returns this value as a Float.
    /// </summary>
    /// <returns> May be null if this value is null. </returns>
    /// <exception cref="InvalidOperationException"> if this an array or object. </exception>
    public float AsFloat()
    {
        return ValueType switch
        {
            ValueTypes.StringValue  => float.Parse( StringValue!, NumberStyles.Float, CultureInfo.InvariantCulture ),
            ValueTypes.DoubleValue  => ( float )DoubleValue!,
            ValueTypes.LongValue    => ( float )LongValue!,
            ValueTypes.BooleanValue => LongValue != 0 ? 1 : 0,
            var _                   => throw new InvalidOperationException( $"Value cannot be converted to float: {ValueType}" ),
        };
    }

    /// <summary>
    /// Returns this value as a Double.
    /// </summary>
    /// <returns> May be null if this value is null. </returns>
    /// <exception cref="InvalidOperationException"> if this an array or object. </exception>
    public double AsDouble()
    {
        return ValueType switch
        {
            ValueTypes.StringValue  => double.Parse( StringValue! ),
            ValueTypes.DoubleValue  => ( double )DoubleValue!,
            ValueTypes.LongValue    => ( double )LongValue!,
            ValueTypes.BooleanValue => LongValue != 0 ? 1 : 0,
            var _                   => throw new InvalidOperationException( $"Value cannot be converted to double: {ValueType}" ),
        };
    }

    /// <summary>
    /// Returns this value as a Long.
    /// </summary>
    /// <returns> May be null if this value is null. </returns>
    /// <exception cref="InvalidOperationException"> if this an array or object. </exception>
    public long AsLong()
    {
        return ValueType switch
        {
            ValueTypes.StringValue  => long.Parse( StringValue! ),
            ValueTypes.DoubleValue  => ( long )DoubleValue!,
            ValueTypes.LongValue    => ( long )LongValue!,
            ValueTypes.BooleanValue => LongValue != 0 ? 1 : 0,
            var _                   => throw new InvalidOperationException( $"Value cannot be converted to long: {ValueType}" ),
        };
    }

    /// <summary>
    /// Returns this value as an Int.
    /// </summary>
    /// <returns> May be null if this value is null. </returns>
    /// <exception cref="InvalidOperationException"> if this an array or object. </exception>
    public int AsInt()
    {
        return ValueType switch
        {
            ValueTypes.StringValue  => int.Parse( StringValue! ),
            ValueTypes.DoubleValue  => ( int )DoubleValue!,
            ValueTypes.LongValue    => ( int )LongValue!,
            ValueTypes.BooleanValue => LongValue != 0 ? 1 : 0,
            var _                   => throw new InvalidOperationException( $"Value cannot be converted to int: {ValueType}" ),
        };
    }

    /// <summary>
    /// Returns this value as a Bool.
    /// </summary>
    /// <returns> May be null if this value is null. </returns>
    /// <exception cref="InvalidOperationException"> if this an array or object. </exception>
    public bool AsBoolean()
    {
        return ValueType switch
        {
            ValueTypes.StringValue  => StringValue!.Equals( "true", StringComparison.OrdinalIgnoreCase ),
            ValueTypes.DoubleValue  => DoubleValue != 0,
            ValueTypes.LongValue    => LongValue != 0,
            ValueTypes.BooleanValue => LongValue != 0,
            var _                   => throw new InvalidOperationException( $"Value cannot be converted to bool: {ValueType}" ),
        };
    }

    /// <summary>
    /// Returns this value as a Byte.
    /// </summary>
    /// <returns> May be null if this value is null. </returns>
    /// <exception cref="InvalidOperationException"> if this an array or object. </exception>
    public byte AsByte()
    {
        return ValueType switch
        {
            ValueTypes.StringValue  => byte.Parse( StringValue! ),
            ValueTypes.DoubleValue  => ( byte )DoubleValue!,
            ValueTypes.LongValue    => ( byte )LongValue!,
            ValueTypes.BooleanValue => ( byte )( LongValue != 0 ? 1 : 0 ),
            var _                   => throw new InvalidOperationException( $"Value cannot be converted to byte: {ValueType}" ),
        };
    }

    /// <summary>
    /// Returns this value as a Short.
    /// </summary>
    /// <returns> May be null if this value is null. </returns>
    /// <exception cref="InvalidOperationException"> if this an array or object. </exception>
    public short AsShort()
    {
        return ValueType switch
        {
            ValueTypes.StringValue  => short.Parse( StringValue! ),
            ValueTypes.DoubleValue  => ( short )DoubleValue!,
            ValueTypes.LongValue    => ( short )LongValue!,
            ValueTypes.BooleanValue => ( short )( LongValue != 0 ? 1 : 0 ),
            var _                   => throw new InvalidOperationException( $"Value cannot be converted to short: {ValueType}" ),
        };
    }

    /// <summary>
    /// Returns this value as a Char.
    /// </summary>
    /// <returns> May be null if this value is null. </returns>
    /// <exception cref="InvalidOperationException"> if this an array or object. </exception>
    public char AsChar()
    {
        Guard.ThrowIfNull( StringValue );

        return ValueType switch
        {
            ValueTypes.StringValue  => ( char )( StringValue.Length == 0 ? 0 : StringValue[ 0 ] ),
            ValueTypes.DoubleValue  => ( char )DoubleValue!,
            ValueTypes.LongValue    => ( char )LongValue!,
            ValueTypes.BooleanValue => ( char )( LongValue != 0 ? 1 : 0 ),
            var _                   => throw new InvalidOperationException( $"Value cannot be converted to char: {ValueType}" ),
        };
    }

    // ========================================================================

    /// <summary>
    /// Returns the children of this value as a newly allocated string array.
    /// </summary>
    /// <exception cref="InvalidOperationException"> if this is not an array. </exception>
    public string[] AsStringArray()
    {
        if ( ValueType is not ValueTypes.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {ValueType}" );
        }

        var array = new string[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            var v = value.ValueType switch
            {
                ValueTypes.StringValue  => value.StringValue,
                ValueTypes.DoubleValue  => StringValue ?? value.DoubleValue.ToString(),
                ValueTypes.LongValue    => StringValue ?? value.LongValue.ToString(),
                ValueTypes.BooleanValue => value.LongValue != 0 ? "true" : "false",
                ValueTypes.NullValue    => null,
                var _                   => throw new InvalidOperationException( $"Value cannot be converted to string: {value.ValueType}" ),
            };

            array[ i ] = v!;
        }

        return array;
    }

    /// <summary>
    /// Returns the children of this value as a newly allocated float array.
    /// </summary>
    /// <exception cref="InvalidOperationException"> if this is not an array. </exception>
    public float[] AsFloatArray()
    {
        if ( ValueType != ValueTypes.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {ValueType}" );
        }

        var array = new float[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            var v = value.ValueType switch
            {
                ValueTypes.StringValue  => float.Parse( value.StringValue! ),
                ValueTypes.DoubleValue  => ( float )value.DoubleValue!,
                ValueTypes.LongValue    => ( float )value.LongValue!,
                ValueTypes.BooleanValue => value.LongValue != 0 ? 1 : 0,

                var _ => throw new InvalidOperationException( $"Value cannot be converted to float: {value.ValueType}" ),
            };

            array[ i ] = v;
        }

        return array;
    }

    /// <summary>
    /// Returns the children of this value as a newly allocated double array.
    /// </summary>
    /// <exception cref="InvalidOperationException"> if this is not an array. </exception>
    public double[] AsDoubleArray()
    {
        if ( ValueType != ValueTypes.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {ValueType}" );
        }

        var array = new double[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            var v = value.ValueType switch
            {
                ValueTypes.StringValue  => double.Parse( value.StringValue! ),
                ValueTypes.DoubleValue  => ( double )value.DoubleValue!,
                ValueTypes.LongValue    => ( double )value.LongValue!,
                ValueTypes.BooleanValue => value.LongValue != 0 ? 1 : 0,
                var _                   => throw new InvalidOperationException( $"Value cannot be converted to double: {value.ValueType}" ),
            };

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
        if ( ValueType != ValueTypes.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {ValueType}" );
        }

        var array = new long?[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            var v = value.ValueType switch
            {
                ValueTypes.StringValue  => long.Parse( value.StringValue! ),
                ValueTypes.DoubleValue  => ( long )value.DoubleValue!,
                ValueTypes.LongValue    => value.LongValue,
                ValueTypes.BooleanValue => value.LongValue != 0 ? 1 : 0,

                var _ => throw new InvalidOperationException( $"Value cannot be converted to long: {value.ValueType}" ),
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
        if ( ValueType != ValueTypes.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {ValueType}" );
        }

        var array = new int[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            var v = value.ValueType switch
            {
                ValueTypes.StringValue  => int.Parse( value.StringValue! ),
                ValueTypes.DoubleValue  => ( int )value.DoubleValue!,
                ValueTypes.LongValue    => ( int )value.LongValue!,
                ValueTypes.BooleanValue => value.LongValue != 0 ? 1 : 0,
                var _                   => throw new InvalidOperationException( $"Value cannot be converted to int: {value.ValueType}" ),
            };

            array[ i ] = v;
        }

        return array;
    }

    /// <summary>
    /// Returns the children of this value as a newly allocated bool array.
    /// </summary>
    /// <exception cref="InvalidOperationException"> if this is not an array. </exception>
    public bool[] AsBooleanArray()
    {
        if ( ValueType != ValueTypes.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {ValueType}" );
        }

        var array = new bool[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            var v = value.ValueType switch
            {
                ValueTypes.StringValue  => bool.Parse( value.StringValue! ),
                ValueTypes.DoubleValue  => value.DoubleValue == 0,
                ValueTypes.LongValue    => value.LongValue == 0,
                ValueTypes.BooleanValue => value.LongValue != 0,
                var _                   => throw new InvalidOperationException( $"Value cannot be converted to bool: {value.ValueType}" ),
            };

            array[ i ] = v;
        }

        return array;
    }

    /// <summary>
    /// Returns the children of this value as a newly allocated byte array.
    /// </summary>
    /// <exception cref="InvalidOperationException"> if this is not an array. </exception>
    public byte[] AsByteArray()
    {
        if ( ValueType != ValueTypes.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {ValueType}" );
        }

        var array = new byte[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            var v = value.ValueType switch
            {
                ValueTypes.StringValue  => byte.Parse( value.StringValue! ),
                ValueTypes.DoubleValue  => ( byte )value.DoubleValue!,
                ValueTypes.LongValue    => ( byte )value.LongValue!,
                ValueTypes.BooleanValue => ( byte )( value.LongValue != 0 ? 1 : 0 ),
                var _                   => throw new InvalidOperationException( $"Value cannot be converted to byte: {value.ValueType}" ),
            };

            array[ i ] = v;
        }

        return array;
    }

    /// <summary>
    /// Returns the children of this value as a newly allocated short array.
    /// </summary>
    /// <exception cref="InvalidOperationException"> if this is not an array. </exception>
    public short[] AsShortArray()
    {
        if ( ValueType != ValueTypes.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {ValueType}" );
        }

        var array = new short[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            var v = value.ValueType switch
            {
                ValueTypes.StringValue  => short.Parse( value.StringValue! ),
                ValueTypes.DoubleValue  => ( short )value.DoubleValue!,
                ValueTypes.LongValue    => ( short )value.LongValue!,
                ValueTypes.BooleanValue => ( short )( value.LongValue != 0 ? 1 : 0 ),
                var _                   => throw new InvalidOperationException( $"Value cannot be converted to short: {value.ValueType}" ),
            };

            array[ i ] = v;
        }

        return array;
    }

    /// <summary>
    /// Returns the children of this value as a newly allocated char array.
    /// </summary>
    /// <exception cref="InvalidOperationException"> if this is not an array. </exception>
    public char[] AsCharArray()
    {
        if ( ValueType != ValueTypes.ArrayValue )
        {
            throw new InvalidOperationException( $"Value is not an array: {ValueType}" );
        }

        var array = new char[ Size ];
        var i     = 0;

        for ( var value = Child; value != null; value = value.Next, i++ )
        {
            var v = value.ValueType switch
            {
                ValueTypes.StringValue  => ( char )( value.StringValue?.Length == 0 ? 0 : value.StringValue!.ToCharArray()[ 0 ] ),
                ValueTypes.DoubleValue  => ( char )value.DoubleValue!,
                ValueTypes.LongValue    => ( char )value.LongValue!,
                ValueTypes.BooleanValue => ( char )( value.LongValue != 0 ? 1 : 0 ),
                var _                   => throw new InvalidOperationException( $"Value cannot be converted to char: {value.ValueType}" ),
            };

            array[ i ] = v;
        }

        return array;
    }
}