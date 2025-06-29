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

using System.Collections;
using System.Globalization;
using System.Text;

using LughSharp.Lugh.Maths;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

using ArgumentException = System.ArgumentException;

namespace Extensions.Source.Json;

[PublicAPI]
public partial class JsonValue : IEnumerable< JsonValue >
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

    internal ValueTypes ValueType;
    internal string?    StringValue;
    internal double?    DoubleValue;
    internal long?      LongValue;

    // ========================================================================

    public JsonValue( ValueTypes valueType )
    {
        ValueType = valueType;
    }

    public JsonValue( string? value )
    {
        Set( value );
    }

    public JsonValue( double value, string stringValue = null! )
    {
        Set( value, stringValue );
    }

    public JsonValue( long value, string stringValue = null! )
    {
        Set( value, stringValue );
    }

    public JsonValue( bool value )
    {
        Set( value );
    }

    // ========================================================================

    /// <summary>
    /// Returns the child at the specified index. This requires walking the linked list to the
    /// specified entry, see <see cref="JsonValue"/> for how to iterate efficiently.
    /// </summary>
    /// <param name="index"> The index of the required child. </param>
    /// <returns> May be null. </returns>
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

    /// <summary>
    /// Returns the child with the specified name. This requires walking the linked list to the
    /// specified entry, see <see cref="JsonValue"/> for how to iterate efficiently.
    /// </summary>
    /// <param name="name"> The name of the required child. </param>
    /// <returns> May be null. </returns>
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

    /// <summary>
    /// Returns the child at the specified index. This requires walking the linked list to the
    /// specified entry, see <see cref="JsonValue"/> for how to iterate efficiently.
    /// This method works as <see cref="Get(int)"/> but requires the child to be present. If the
    /// child is not found, an exception will be thrown.
    /// </summary>
    /// <param name="index"> The index of the required child. </param>
    /// <returns> The requested child. </returns>
    /// <exception cref="ArgumentException"> If the child was not found. </exception>
    public JsonValue Require( int index )
    {
        var current = Get( index );

        if ( current == null )
        {
            throw new ArgumentException( $"Child not found with index: {index}" );
        }

        return current;
    }

    /// <summary>
    /// Returns the child with the specified name. This requires walking the linked list to the
    /// specified entry, see <see cref="JsonValue"/> for how to iterate efficiently.
    /// This method works as <see cref="Get(string)"/> but requires the child to be present. If the
    /// child is not found, an exception will be thrown.
    /// </summary>
    /// <param name="name"> The name of the required child. </param>
    /// <returns> The requested child. </returns>
    /// <exception cref="ArgumentException"> If the child was not found. </exception>
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

    /// <summary>
    /// Removes the child at the specified index. This requires walking the linked list to the
    /// specified entry, see <see cref="JsonValue"/> for how to iterate efficiently.
    /// </summary>
    /// <param name="index"> The index of the required child. </param>
    /// <returns> May be null. </returns>
    public JsonValue? Remove( int index )
    {
        var child = Get( index );

        if ( child == null )
        {
            return null;
        }

        if ( child.Prev == null )
        {
            Child = child.Next;

            if ( Child != null )
            {
                Child.Prev = null;
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
    /// Remves the child with the specified name. This requires walking the linked list to the
    /// specified entry, see <see cref="JsonValue"/> for how to iterate efficiently.
    /// </summary>
    /// <param name="name"> The name of the required child. </param>
    /// <returns> May be null. </returns>
    public JsonValue? Remove( string name )
    {
        var child = Get( name );

        if ( child == null )
        {
            return null;
        }

        if ( child.Prev == null )
        {
            Child = child.Next;

            if ( Child != null )
            {
                Child.Prev = null;
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
    /// <exception cref="ArgumentNullException"> If the parent is null. </exception>
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

    /// <summary>
    /// Returns true if there are one or more children in this JsonValue.
    /// </summary>
    public bool NotEmpty()
    {
        return Size > 0;
    }

    /// <summary>
    /// Returns true if there are not children in this JsonValue.
    /// </summary>
    public bool IsEmpty()
    {
        return Size == 0;
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

        return child?.Child;
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public bool IsArray()
    {
        return ValueType == ValueTypes.ArrayValue;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public bool IsObject()
    {
        return ValueType == ValueTypes.ObjectValue;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public bool IsString()
    {
        return ValueType == ValueTypes.StringValue;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public bool IsNumber()
    {
        return ValueType is ValueTypes.DoubleValue or ValueTypes.LongValue;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public bool IsDouble()
    {
        return ValueType == ValueTypes.DoubleValue;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public bool IsLong()
    {
        return ValueType == ValueTypes.LongValue;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public bool IsBoolean()
    {
        return ValueType == ValueTypes.BooleanValue;
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public bool IsNull()
    {
        return ValueType == ValueTypes.NullValue;
    }

    /// <summary>
    /// Returns true if this is not an array or object.
    /// </summary>
    public bool IsValue()
    {
        return ValueType switch
        {
            ValueTypes.StringValue
                or ValueTypes.DoubleValue
                or ValueTypes.LongValue
                or ValueTypes.BooleanValue
                or ValueTypes.NullValue => true,
            var _ => false,
        };
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
        StringValue = value;
        ValueType   = value == null ? ValueTypes.NullValue : ValueTypes.StringValue;
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
        ValueType   = ValueTypes.DoubleValue;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="stringValue">
    /// May be null if the string representation is the string value of the long (eg, no leading zeros).
    /// </param>
    public void Set( long value, string stringValue )
    {
        LongValue   = value;
        DoubleValue = value;
        StringValue = stringValue;
        ValueType   = ValueTypes.LongValue;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    public void Set( bool value )
    {
        LongValue = value ? 1 : 0;
        ValueType = ValueTypes.BooleanValue;
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="outputType"></param>
    /// <returns></returns>
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

    /// <summary>
    /// </summary>
    /// <param name="jsonval"></param>
    /// <param name="buffer"></param>
    /// <param name="outputType"></param>
    /// <exception cref="SerializationException"></exception>
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

            buffer.Append( Math.Abs( doubleValue - longValue ) < NumberUtils.FLOAT_TOLERANCE ? longValue : doubleValue );
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
            return ValueType switch
            {
                ValueTypes.ArrayValue  => "[]",
                ValueTypes.ObjectValue => "{}",
                var _                  => "",
            };
        }

        string trace;

        if ( Parent.ValueType == ValueTypes.ArrayValue )
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

    /// <summary>
    /// Pretty-prints the JsonValue to a string using the specified output type and single line column limit.
    /// </summary>
    /// <param name="outputType">The desired JsonOutputType for the output string.</param>
    /// <param name="singleLineColumns">The maximum number of characters allowed on a single line before wrapping.</param>
    /// <returns>A pretty-printed string representation of the JsonValue.</returns>
    public string PrettyPrint( JsonOutputType? outputType, int singleLineColumns )
    {
        Guard.ThrowIfNull( outputType );

        var settings = new PrettyPrintSettings
        {
            JsonOutputType    = ( JsonOutputType )outputType,
            SingleLineColumns = singleLineColumns,
        };

        return PrettyPrint( settings );
    }

    /// <summary>
    /// Pretty-prints the JsonValue to a string using the provided PrettyPrintSettings.
    /// </summary>
    /// <param name="settings">The PrettyPrintSettings to control the output format.</param>
    /// <returns>A pretty-printed string representation of the JsonValue.</returns>
    public string PrettyPrint( PrettyPrintSettings settings )
    {
        Logger.Checkpoint();

        var buffer = new StringBuilder( 512 );

        PrettyPrint( this, buffer, 0, settings );

        return buffer.ToString();
    }

    /// <summary>
    /// Pretty-prints a JsonValue to a StringBuilder with specified indentation and formatting settings.
    /// </summary>
    /// <param name="jsonval">The JsonValue to pretty-print.</param>
    /// <param name="buffer">The StringBuilder to write the pretty-printed JSON to.</param>
    /// <param name="indent">The current indentation level.</param>
    /// <param name="settings">The PrettyPrintSettings that control the output format.</param>
    /// <exception cref="SerializationException">Thrown when an unknown JsonValue type is encountered.</exception>
    private static void PrettyPrint( JsonValue jsonval, StringBuilder buffer, int indent, PrettyPrintSettings settings )
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

            buffer.Append( Math.Abs( doubleValue - longValue ) < NumberUtils.FLOAT_TOLERANCE
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

    /// <summary>
    /// Pretty-prints the JsonValue to a TextWriter using the specified JsonOutputType.
    /// </summary>
    /// <param name="outputType">The desired JsonOutputType for the output.</param>
    /// <param name="writer">The TextWriter to write the pretty-printed JSON to.</param>
    public void PrettyPrint( JsonOutputType? outputType, TextWriter writer )
    {
        Guard.ThrowIfNull( outputType );

        var settings = new PrettyPrintSettings
        {
            JsonOutputType = ( JsonOutputType )outputType,
        };

        PrettyPrint( this, writer, 0, settings );
    }

    /// <summary>
    /// Pretty-prints a JsonValue to a TextWriter with specified indentation and formatting settings.
    /// </summary>
    /// <param name="jsonval">The JsonValue to pretty-print.</param>
    /// <param name="writer">The TextWriter to write the pretty-printed JSON to.</param>
    /// <param name="indent">The current indentation level.</param>
    /// <param name="settings">The PrettyPrintSettings that control the output format.</param>
    /// <exception cref="SerializationException">Thrown when an unknown JsonValue type is encountered.</exception>
    private static void PrettyPrint( JsonValue jsonval, TextWriter writer, int indent, PrettyPrintSettings settings )
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

            writer.Write( Math.Abs( doubleValue - longValue ) < NumberUtils.FLOAT_TOLERANCE
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

    /// <summary>
    /// </summary>
    /// <param name="jsonval"></param>
    /// <returns></returns>
    private static bool IsFlat( JsonValue jsonval )
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

    /// <summary>
    /// </summary>
    /// <param name="jsonval"></param>
    /// <returns></returns>
    private static bool IsNumeric( JsonValue jsonval )
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

    /// <summary>
    /// </summary>
    /// <param name="count"></param>
    /// <param name="buffer"></param>
    private static void Indent( int count, StringBuilder buffer )
    {
        for ( var i = 0; i < count; i++ )
        {
            buffer.Append( '\t' );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="count"></param>
    /// <param name="buffer"></param>
    private static void Indent( int count, TextWriter buffer )
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
        return new JsonValueIterator( this, Size );
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// </summary>
    [PublicAPI]
    public class JsonValueIterator : IEnumerator< JsonValue >, IEnumerable< JsonValue >
    {
        private JsonValue? _child;
        private JsonValue? _current;
        private int        _size;

        public JsonValueIterator( JsonValue child, int size )
        {
            _child = child;
            _size  = size;
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public JsonValue Current => _current!;

        /// <inheritdoc />
        object IEnumerator.Current => Current;

        /// <inheritdoc />
        public void Reset()
        {
            throw new NotSupportedException();
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

        /// <inheritdoc />
        public IEnumerator< JsonValue > GetEnumerator()
        {
            return this;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize( this );
        }
    }

    // ========================================================================

    /// <summary>
    /// All valid <see cref="JsonValue"/> Types.
    /// </summary>
    [PublicAPI]
    public enum ValueTypes : int
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

    /// <summary>
    /// Settings for use with PrettyPrint methods.
    /// </summary>
    [PublicAPI]
    public class PrettyPrintSettings
    {
        /// <summary>
        /// The Json output type, one of <see cref="JsonOutputType.Minimal"/>,
        /// <see cref="JsonOutputType.Json"/>, or <see cref="JsonOutputType.Javascript"/>.
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