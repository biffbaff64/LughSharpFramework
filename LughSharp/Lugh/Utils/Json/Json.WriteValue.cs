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

using System.Runtime.Serialization;

using LughSharp.Lugh.Maps;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Collections.DeleteCandidates;

namespace LughSharp.Lugh.Utils.Json;

public partial class Json
{
    /** Writes the value as a field on the current JSON object, without writing the actual class.
     * @param value May be null.
     * @see #WriteValue(string, object, Type, Type) */
    public void WriteValue( string name, object value )
    {
        try
        {
            writer.name( name );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }

        WriteValue( value, value == null ? null : value.GetType(), null );
    }

    /** Writes the value as a field on the current JSON object, writing the class of the object if it differs from the specified
     * known type.
     * @param value May be null.
     * @param knownType May be null if the type is unknown.
     * @see #WriteValue(string, object, Type, Type) */
    public void WriteValue( string name, object value, Type knownType )
    {
        try
        {
            writer.name( name );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }

        WriteValue( value, knownType, null );
    }

    /** Writes the value as a field on the current JSON object, writing the class of the object if it differs from the specified
     * known type. The specified element type is used as the default type for collections.
     * @param value May be null.
     * @param knownType May be null if the type is unknown.
     * @param elementType May be null if the type is unknown. */
    public void WriteValue( string name, object value, Type knownType, Type elementType )
    {
        try
        {
            writer.name( name );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }

        WriteValue( value, knownType, elementType );
    }

    /** Writes the value, without writing the class of the object.
     * @param value May be null. */
    public void WriteValue( object value )
    {
        if ( value == null )
        {
            WriteValue( value, null, null );
        }
        else
        {
            WriteValue( value, value.GetType(), null );
        }
    }

    /** Writes the value, writing the class of the object if it differs from the specified known type.
     * @param value May be null.
     * @param knownType May be null if the type is unknown. */
    public void WriteValue( object value, Type knownType )
    {
        WriteValue( value, knownType, null );
    }

    /** Writes the value, writing the class of the object if it differs from the specified known type. The specified element type
     * is used as the default type for collections.
     * @param value May be null.
     * @param knownType May be null if the type is unknown.
     * @param elementType May be null if the type is unknown. */
    public void WriteValue( object value, Type knownType, Type elementType )
    {
        try
        {
            if ( value == null )
            {
                writer.value( null );

                return;
            }

            if ( ( ( knownType != null ) && knownType.isPrimitive() ) || ( knownType == string. )class || knownType == Integer.class
            || knownType == Boolean.class || knownType == Float.class || knownType == Long.class || knownType == Double.class
            || knownType == Short.class || knownType == Byte.class || knownType == Character.class) {
                writer.value( value );

                return;
            }

            Type actualType = value.GetType();

            if ( actualType.IsPrimitive
                 || ( actualType == typeof( string ) )
                 || ( actualType == typeof( int ) )
                 || ( actualType == typeof( bool ) )
                 || ( actualType == typeof( float ) )
                 || ( actualType == typeof( long ) )
                 || ( actualType == typeof( double ) )
                 || ( actualType == typeof( short ) )
                 || ( actualType == typeof( byte ) )
                 || ( actualType == typeof( char ) ) )
            {
                WriteObjectStart( actualType, null );
                WriteValue( "value", value );
                WriteObjectEnd();

                return;
            }

            if ( value is Serializable )
            {
                WriteObjectStart( actualType, knownType );
                ( ( Serializable )value ).write( this );
                WriteObjectEnd();

                return;
            }

            Serializer serializer = classToSerializer.get( actualType );

            if ( serializer != null )
            {
                serializer.write( this, value, knownType );

                return;
            }

            // JSON array special cases.
            if ( value is Array )
            {
                if ( ( knownType != null ) && ( actualType != knownType ) && ( actualType != Array. )class)

                throw new SerializationException( "Serialization of an Array other than the known type is not supported.\n"
                                                  + "Known type: " + knownType + "\nActual type: " + actualType );

                WriteArrayStart();
                var array = ( Array )value;

                for ( int i = 0, n = array.size; i < n; i++ )
                {
                    WriteValue( array.get( i ), elementType, null );
                }

                WriteArrayEnd();

                return;
            }

            if ( value is Queue )
            {
                if ( ( knownType != null ) && ( actualType != knownType ) && ( actualType != Queue. )class)

                throw new SerializationException( "Serialization of a Queue other than the known type is not supported.\n"
                                                  + "Known type: " + knownType + "\nActual type: " + actualType );

                WriteArrayStart();
                Queue queue = ( Queue )value;

                for ( int i = 0, n = queue.size; i < n; i++ )
                {
                    WriteValue( queue.get( i ), elementType, null );
                }

                WriteArrayEnd();

                return;
            }

            if ( value is Collection )
            {
                if ( ( TypeName != null ) && ( actualType != List. )class && ( ( knownType == null ) || ( knownType != actualType ) )) {
                    WriteObjectStart( actualType, knownType );
                    WriteArrayStart( "items" );
                    for ( object item :
                    ( Collection )value)
                    WriteValue( item, elementType, null );
                    WriteArrayEnd();
                    WriteObjectEnd();
                } else {
                    WriteArrayStart();
                    for ( object item :
                    ( Collection )value)
                    WriteValue( item, elementType, null );
                    WriteArrayEnd();
                }

                return;
            }

            if ( actualType.isArray() )
            {
                if ( elementType == null )
                {
                    elementType = actualType.getComponentType();
                }

                int length = ArrayReflection.getLength( value );
                WriteArrayStart();

                for ( var i = 0; i < length; i++ )
                {
                    WriteValue( ArrayReflection.get( value, i ), elementType, null );
                }

                WriteArrayEnd();

                return;
            }

            // JSON object special cases.
            if ( value is ObjectMap< , > )
            {
                if ( knownType == null )
                {
                    knownType = ObjectMap.
                }class;
                WriteObjectStart( actualType, knownType );
                for ( Entry entry :
                ( ( ObjectMap < ?,  ?>)value).entries()) {
                    writer.name( convertToString( entry.key ) );
                    WriteValue( entry.value, elementType, null );
                }
                WriteObjectEnd();

                return;
            }

            if ( value is ObjectIntMap )
            {
                if ( knownType == null )
                {
                    knownType = ObjectIntMap.
                }class;
                WriteObjectStart( actualType, knownType );
                for ( ObjectIntMap.Entry entry :
                ( ( ObjectIntMap < ?>)value).entries()) {
                    writer.name( convertToString( entry.key ) );
                    WriteValue( entry.value, Integer.class);
                }
                WriteObjectEnd();

                return;
            }

            if ( value is ObjectFloatMap )
            {
                if ( knownType == null )
                {
                    knownType = ObjectFloatMap.
                }class;
                WriteObjectStart( actualType, knownType );
                for ( ObjectFloatMap.Entry entry :
                ( ( ObjectFloatMap < ?>)value).entries()) {
                    writer.name( convertToString( entry.key ) );
                    WriteValue( entry.value, Float.class);
                }
                WriteObjectEnd();

                return;
            }

            if ( value is ObjectSet )
            {
                if ( knownType == null )
                {
                    knownType = ObjectSet.
                }class;
                WriteObjectStart( actualType, knownType );
                writer.name( "values" );
                WriteArrayStart();
                for ( object entry :
                ( ObjectSet )value)
                WriteValue( entry, elementType, null );
                WriteArrayEnd();
                WriteObjectEnd();

                return;
            }

            if ( value is IntMap )
            {
                if ( knownType == null )
                {
                    knownType = IntMap.
                }class;
                WriteObjectStart( actualType, knownType );
                for ( IntMap.Entry entry :
                ( ( IntMap < ?>)value).entries()) {
                    writer.name( string.valueOf( entry.key ) );
                    WriteValue( entry.value, elementType, null );
                }
                WriteObjectEnd();

                return;
            }

            if ( value is LongMap )
            {
                if ( knownType == null )
                {
                    knownType = LongMap.
                }class;
                WriteObjectStart( actualType, knownType );
                for ( LongMap.Entry entry :
                ( ( LongMap < ?>)value).entries()) {
                    writer.name( string.valueOf( entry.key ) );
                    WriteValue( entry.value, elementType, null );
                }
                WriteObjectEnd();

                return;
            }

            if ( value is IntSet )
            {
                if ( knownType == null )
                {
                    knownType = IntSet.
                }class;
                WriteObjectStart( actualType, knownType );
                writer.name( "values" );
                WriteArrayStart();

                for ( IntSetIterator iter = ( ( IntSet )value ).iterator(); iter.hasNext; )
                {
                    WriteValue( iter.next(), Integer.
                }class, null);
                WriteArrayEnd();
                WriteObjectEnd();

                return;
            }

            if ( value is ArrayMap )
            {
                if ( knownType == null )
                {
                    knownType = ArrayMap.
                }class;
                WriteObjectStart( actualType, knownType );
                ArrayMap map = ( ArrayMap )value;

                for ( int i = 0, n = map.size; i < n; i++ )
                {
                    writer.name( convertToString( map.keys[ i ] ) );
                    WriteValue( map.values[ i ], elementType, null );
                }

                WriteObjectEnd();

                return;
            }

            if ( value is Map )
            {
                if ( knownType == null )
                {
                    knownType = HashMap.
                }class;
                WriteObjectStart( actualType, knownType );
                for ( Map.Entry entry :
                ( ( Map < ?,  ?>)value).entrySet()) {
                    writer.name( convertToString( entry.getKey() ) );
                    WriteValue( entry.getValue(), elementType, null );
                }
                WriteObjectEnd();

                return;
            }

            // Enum special case.
            if ( ClassReflection.isAssignableFrom( Enum.class, actualType)) {
                if ( ( TypeName != null ) && ( ( knownType == null ) || ( knownType != actualType ) ) )
                {
                    // Ensures that enums with specific implementations (abstract logic) serialize correctly.
                    if ( actualType.getEnumConstants() == null )
                    {
                        actualType = actualType.getSuperclass();
                    }

                    WriteObjectStart( actualType, null );
                    writer.name( "value" );
                    writer.value( convertToString( ( Enum )value ) );
                    WriteObjectEnd();
                }
                else
                {
                    writer.value( convertToString( ( Enum )value ) );
                }

                return;
            }

            WriteObjectStart( actualType, knownType );
            WriteFields( value );
            WriteObjectEnd();
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }
    }

    /** Writes all fields of the specified object to the current JSON object. */
    public void WriteFields( object @object )
    {
        Type type = object.GetType();

        var defaultValues = GetDefaultValues( type );

        OrderedMap< string, FieldMetadata > fields       = GetFields( type );
        var                                 defaultIndex = 0;
        Array< string >                     fieldNames   = fields.orderedKeys();

        for ( int i = 0, n = fieldNames.size; i < n; i++ )
        {
            FieldMetadata metadata = fields.get( fieldNames.get( i ) );

            if ( ignoreDeprecated && metadata.deprecated )
            {
                continue;
            }

            Field field = metadata.field;

            try
            {
                object value = field.get( object );

                if ( defaultValues != null )
                {
                    var defaultValue = defaultValues[ defaultIndex++ ];

                    if ( ( value == null ) && ( defaultValue == null ) )
                    {
                        continue;
                    }

                    if ( ( value != null ) && ( defaultValue != null ) )
                    {
                        if ( value.equals( defaultValue ) )
                        {
                            continue;
                        }

                        if ( value.GetType().isArray() && defaultValue.GetType().isArray() )
                        {
                            equals1[ 0 ] = value;
                            equals2[ 0 ] = defaultValue;

                            if ( Arrays.deepEquals( equals1, equals2 ) )
                            {
                                continue;
                            }
                        }
                    }
                }

                if ( debug )
                {
                    System.out.
                }

                println( "Writing field: " + field.getName() + " (" + type.getName() + ")" );
                writer.name( field.getName() );
                writeValue( value, field.getType(), metadata.elementType );
            }
            catch ( ReflectionException ex )
            {
                throw new SerializationException( "Error accessing field: " + field.getName() + " (" + type.getName() + ")", ex );
            }
            catch ( SerializationException ex )
            {
                ex.addTrace( field + " (" + type.getName() + ")" );

                throw ex;
            }
            catch ( Exception runtimeEx )
            {
                var ex = new SerializationException( runtimeEx );
                ex.addTrace( field + " (" + type.getName() + ")" );

                throw ex;
            }
        }
    }

    /** @see #WriteField(object, string, string, Type) */
    public void WriteField( object @object, string name )
    {
        WriteField( object, name, name, null );
    }

    /** @param elementType May be null if the type is unknown.
     * @see #WriteField(object, string, string, Type) */
    public void WriteField( object @object, string name, Type elementType )
    {
        WriteField( object, name, name, elementType );
    }

    /** @see #WriteField(object, string, string, Type) */
    public void WriteField( object @object, string fieldName, string jsonName )
    {
        WriteField( object, fieldName, jsonName, null );
    }

    /** Writes the specified field to the current JSON object.
     * @param elementType May be null if the type is unknown. */
    public void WriteField( object @object, string fieldName, string jsonName, Type elementType )
    {
        Type          type     = object.GetType();
        FieldMetadata metadata = GetFields( type ).get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( "Field not found: " + fieldName + " (" + type.getName() + ")" );
        }

        Field field                            = metadata.field;
        if ( elementType == null )
        {
            elementType = metadata.elementType;
        }

        try
        {
            if ( debug )
            {
                System.out.
            }

            println( "Writing field: " + field.getName() + " (" + type.getName() + ")" );
            writer.name( jsonName );
            writeValue( field.get( object ), field.getType(), elementType );
        }
        catch ( ReflectionException ex )
        {
            throw new SerializationException( "Error accessing field: " + field.getName() + " (" + type.getName() + ")", ex );
        }
        catch ( SerializationException ex )
        {
            ex.addTrace( field + " (" + type.getName() + ")" );

            throw ex;
        }
        catch ( Exception runtimeEx )
        {
            var ex = new SerializationException( runtimeEx );
            ex.addTrace( field + " (" + type.getName() + ")" );

            throw ex;
        }
    }

    public void WriteObjectStart( string name )
    {
        try
        {
            writer.name( name );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }

        WriteObjectStart();
    }

    /** @param knownType May be null if the type is unknown. */
    public void WriteObjectStart( string name, Type actualType, Type knownType )
    {
        try
        {
            writer.name( name );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }

        WriteObjectStart( actualType, knownType );
    }

    public void WriteObjectStart()
    {
        try
        {
            writer.object();
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }
    }

    /** Starts writing an object, writing the actualType to a field if needed.
     * @param knownType May be null if the type is unknown. */
    public void WriteObjectStart( Type actualType, Type? knownType )
    {
        try
        {
            writer.object();
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }

        if ( ( knownType == null ) || ( knownType != actualType ) )
        {
            WriteType( actualType );
        }
    }

    public void WriteObjectEnd()
    {
        try
        {
            writer.pop();
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }
    }

    public void WriteArrayStart( string name )
    {
        try
        {
            writer.name( name );
            writer.array();
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }
    }

    public void WriteArrayStart()
    {
        try
        {
            writer.array();
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }
    }

    public void WriteArrayEnd()
    {
        try
        {
            writer.pop();
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }
    }

    public void WriteType( Type type )
    {
        if ( TypeName == null )
        {
            return;
        }

        var className = GetTag( type );

        if ( className == null )
        {
            className = type.getName();
        }

        try
        {
            writer.set( TypeName, className );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }

        if ( debug )
        {
            System.out.
        }

        println( "Writing type: " + type.getName() );
    }
}