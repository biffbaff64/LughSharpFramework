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
using System.Runtime.Serialization;

using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Guarding;

namespace LughSharp.Lugh.Utils.Json;

public partial class Json
{
    /// <summary>
    /// Writes the value as a field on the current JSON object, without writing the actual class.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"> May be null. </param>
    public void WriteValue( string name, object? value )
    {
        try
        {
            JsonWriter?.Name( name );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }

        WriteValue( value, value?.GetType(), null );
    }

    /// <summary>
    /// Writes the value as a field on the current JSON object, writing the class of the object
    /// if it differs from the specified known type.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"> May be null. </param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    public void WriteValue( string name, object value, Type knownType )
    {
        try
        {
            JsonWriter?.Name( name );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }

        WriteValue( value, knownType, null );
    }

    /// <summary>
    /// Writes the value as a field on the current JSON object, writing the class of the object
    /// if it differs from the specified known type. The specified element type is used as the
    /// default type for collections.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"> May be null. </param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    /// <param name="elementType"> May be null if the type is unknown. </param>
    public void WriteValue( string name, object value, Type knownType, Type elementType )
    {
        try
        {
            JsonWriter?.Name( name );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }

        WriteValue( value, knownType, elementType );
    }

    /// <summary>
    /// Writes the value, without writing the class of the object.
    /// </summary>
    /// <param name="value"> May be null. </param>
    public void WriteValue( object? value )
    {
        WriteValue( value, value?.GetType(), null );
    }

    /// <summary>
    /// Writes the value, writing the class of the object if it differs from the specified known type.
    /// </summary>
    /// <param name="value"> May be null. </param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    public void WriteValue( object value, Type knownType )
    {
        WriteValue( value, knownType, null );
    }

    /// <summary>
    /// Writes the value, writing the class of the object if it differs from the specified known type.
    /// The specified element type is used as the default type for collections.
    /// </summary>
    /// <param name="value"> May be null. </param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    /// <param name="elementType"> May be null if the type is unknown. </param>
    public void WriteValue( object? value, Type? knownType, Type? elementType )
    {
        try
        {
            if ( value == null )
            {
                JsonWriter?.Value( null );

                return;
            }

            if ( knownType is { IsPrimitive: true }
                 || ( knownType == typeof( string ) )
                 || ( knownType == typeof( int ) )
                 || ( knownType == typeof( bool ) )
                 || ( knownType == typeof( float ) )
                 || ( knownType == typeof( long ) )
                 || ( knownType == typeof( double ) )
                 || ( knownType == typeof( short ) )
                 || ( knownType == typeof( byte ) ) )

//                || knownType == Character )
            {
                JsonWriter?.Value( value );

                return;
            }

            var actualType = value.GetType();

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

            if ( value is IJsonSerializable serializable )
            {
                WriteObjectStart( actualType, knownType );
                serializable.Write( this );
                WriteObjectEnd();

                return;
            }

            var serializer = _classToSerializer.Get( actualType );

            if ( serializer != null )
            {
                serializer.Write( this, value, knownType );

                return;
            }

            // JSON array special cases.
            if ( value is Array array )
            {
                if ( ( knownType != null ) && ( actualType != knownType ) && ( actualType != typeof( Array ) ) )
                {
                    throw new SerializationException( $"Serialization of an Array other than the" +
                                                      $"known type is not supported.\nKnown type: " +
                                                      $"{knownType}\nActual type: {actualType}" );
                }

                WriteArrayStart();

                for ( int i = 0, n = array.Length; i < n; i++ )
                {
                    WriteValue( array.GetValue( i ), elementType, null );
                }

                WriteArrayEnd();

                return;
            }

            if ( value is Queue queue )
            {
                if ( ( knownType != null ) && ( actualType != knownType ) && ( actualType != typeof( Queue ) ) )
                {
                    throw new SerializationException( $"Serialization of a Queue other than the " +
                                                      $"known type is not supported.\nKnown type: " +
                                                      $"{knownType}\nActual type: {actualType}" );
                }

                WriteArrayStart();

                var queueElements = queue.ToArray();

                for ( int i = 0, n = queue.Count; i < n; i++ )
                {
                    WriteValue( queueElements[ i ], elementType, null );
                }

                WriteArrayEnd();

                return;
            }

//            if ( value is Collection )
//            {
//                if ( ( TypeName != null ) && ( actualType != List. )class && ( ( knownType == null ) || ( knownType != actualType ) )) {
//                    WriteObjectStart( actualType, knownType );
//                    WriteArrayStart( "items" );
//                    for ( object item :
//                    ( Collection )value)
//                    WriteValue( item, elementType, null );
//                    WriteArrayEnd();
//                    WriteObjectEnd();
//                } else {
//                    WriteArrayStart();
//                    for ( object item :
//                    ( Collection )value)
//                    WriteValue( item, elementType, null );
//                    WriteArrayEnd();
//                }
//
//                return;
//            }

//            if ( actualType.isArray() )
//            {
//                if ( elementType == null )
//                {
//                    elementType = actualType.getComponentType();
//                }
//
//                int length = ArrayReflection.getLength( value );
//                WriteArrayStart();
//
//                for ( var i = 0; i < length; i++ )
//                {
//                    WriteValue( ArrayReflection.get( value, i ), elementType, null );
//                }
//
//                WriteArrayEnd();
//
//                return;
//            }

            // JSON object special cases.
//            if ( value is ObjectMap< , > )
//            {
//                if ( knownType == null )
//                {
//                    knownType = ObjectMap.
//                }class;
//                WriteObjectStart( actualType, knownType );
//                for ( Entry entry :
//                ( ( ObjectMap < ?,  ?>)value).entries()) {
//                    writer.name( convertToString( entry.key ) );
//                    WriteValue( entry.value, elementType, null );
//                }
//                WriteObjectEnd();
//
//                return;
//            }

//            if ( value is ObjectIntMap )
//            {
//                if ( knownType == null )
//                {
//                    knownType = ObjectIntMap.
//                }class;
//                WriteObjectStart( actualType, knownType );
//                for ( ObjectIntMap.Entry entry :
//                ( ( ObjectIntMap < ?>)value).entries()) {
//                    writer.name( convertToString( entry.key ) );
//                    WriteValue( entry.value, Integer.class);
//                }
//                WriteObjectEnd();
//
//                return;
//            }

//            if ( value is ObjectFloatMap )
//            {
//                if ( knownType == null )
//                {
//                    knownType = ObjectFloatMap.
//                }class;
//                WriteObjectStart( actualType, knownType );
//                for ( ObjectFloatMap.Entry entry :
//                ( ( ObjectFloatMap < ?>)value).entries()) {
//                    writer.name( convertToString( entry.key ) );
//                    WriteValue( entry.value, Float.class);
//                }
//                WriteObjectEnd();
//
//                return;
//            }

//            if ( value is ObjectSet )
//            {
//                if ( knownType == null )
//                {
//                    knownType = ObjectSet.
//                }class;
//                WriteObjectStart( actualType, knownType );
//                writer.name( "values" );
//                WriteArrayStart();
//                for ( object entry :
//                ( ObjectSet )value)
//                WriteValue( entry, elementType, null );
//                WriteArrayEnd();
//                WriteObjectEnd();
//
//                return;
//            }

//            if ( value is IntMap )
//            {
//                if ( knownType == null )
//                {
//                    knownType = IntMap.
//                }class;
//                WriteObjectStart( actualType, knownType );
//                for ( IntMap.Entry entry :
//                ( ( IntMap < ?>)value).entries()) {
//                    writer.name( string.valueOf( entry.key ) );
//                    WriteValue( entry.value, elementType, null );
//                }
//                WriteObjectEnd();
//
//                return;
//            }

//            if ( value is LongMap )
//            {
//                if ( knownType == null )
//                {
//                    knownType = LongMap.
//                }class;
//                WriteObjectStart( actualType, knownType );
//                for ( LongMap.Entry entry :
//                ( ( LongMap < ?>)value).entries()) {
//                    writer.name( string.valueOf( entry.key ) );
//                    WriteValue( entry.value, elementType, null );
//                }
//                WriteObjectEnd();
//
//                return;
//            }

//            if ( value is IntSet )
//            {
//                if ( knownType == null )
//                {
//                    knownType = IntSet.
//                }class;
//                WriteObjectStart( actualType, knownType );
//                writer.name( "values" );
//                WriteArrayStart();
//
//                for ( IntSetIterator iter = ( ( IntSet )value ).iterator(); iter.hasNext; )
//                {
//                    WriteValue( iter.next(), Integer.
//                }class, null);
//                WriteArrayEnd();
//                WriteObjectEnd();
//
//                return;
//            }

//            if ( value is ArrayMap )
//            {
//                if ( knownType == null )
//                {
//                    knownType = ArrayMap.
//                }class;
//                WriteObjectStart( actualType, knownType );
//                ArrayMap map = ( ArrayMap )value;
//
//                for ( int i = 0, n = map.size; i < n; i++ )
//                {
//                    writer.name( convertToString( map.keys[ i ] ) );
//                    WriteValue( map.values[ i ], elementType, null );
//                }
//
//                WriteObjectEnd();
//
//                return;
//            }

//            if ( value is Map )
//            {
//                if ( knownType == null )
//                {
//                    knownType = HashMap.
//                }class;
//                WriteObjectStart( actualType, knownType );
//                for ( Map.Entry entry :
//                ( ( Map < ?,  ?>)value).entrySet()) {
//                    writer.name( convertToString( entry.getKey() ) );
//                    WriteValue( entry.getValue(), elementType, null );
//                }
//                WriteObjectEnd();
//
//                return;
//            }

            // Enum special case.
//            if ( ClassReflection.isAssignableFrom( Enum.class, actualType)) {
//                if ( ( TypeName != null ) && ( ( knownType == null ) || ( knownType != actualType ) ) )
//                {
//                    // Ensures that enums with specific implementations (abstract logic) serialize correctly.
//                    if ( actualType.getEnumConstants() == null )
//                    {
//                        actualType = actualType.getSuperclass();
//                    }
//
//                    WriteObjectStart( actualType, null );
//                    writer.name( "value" );
//                    writer.value( convertToString( ( Enum )value ) );
//                    WriteObjectEnd();
//                }
//                else
//                {
//                    writer.value( convertToString( ( Enum )value ) );
//                }
//
//                return;
//            }

            WriteObjectStart( actualType, knownType );
            WriteFields( value );
            WriteObjectEnd();
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }
    }

    /// <summary>
    /// Writes all fields of the specified object to the current JSON object.
    /// </summary>
    public void WriteFields( object obj )
    {
        var type          = obj.GetType();
        var defaultValues = GetDefaultValues( type );
        var fields        = GetFields( type );
        var fieldNames    = fields?.OrderedKeys();
        var defaultIndex  = 0;

        for ( int i = 0, n = fieldNames!.Count; i < n; i++ )
        {
            var metadata = fields?.Get( fieldNames[ i ] );

            Guard.ThrowIfNull( metadata );

            if ( IgnoreDeprecated && metadata.Deprecated )
            {
                continue;
            }

            var field = metadata.FieldInfo;

            try
            {
                var value = field.GetValue( obj );

                if ( defaultValues != null )
                {
                    var defaultValue = defaultValues[ defaultIndex++ ];

                    if ( ( value == null ) && ( defaultValue == null ) )
                    {
                        continue;
                    }

                    if ( ( value != null ) && ( defaultValue != null ) )
                    {
                        if ( value.Equals( defaultValue ) )
                        {
                            continue;
                        }

                        if ( value.GetType().IsArray && defaultValue.GetType().IsArray )
                        {
                            _equals1[ 0 ] = value;
                            _equals2[ 0 ] = defaultValue;

                            if ( SystemArrayUtils.DeepEquals( _equals1, _equals2 ) )
                            {
                                continue;
                            }
                        }
                    }
                }

                Logger.Debug( $"Writing field: {field.Name} ({type.Name})" );

                JsonWriter?.Name( field.Name );
                WriteValue( value, field.GetType(), metadata.ElementType );
            }
            catch ( FieldAccessException ex )
            {
                throw new SerializationException( $"Error accessing field: {field.Name} ({type.Name})", ex );
            }
            catch ( SerializationException ex )
            {
                var newEx = new Exceptions.SerializationException( ex.Message );
                newEx.AddTrace( $"{field} ({type.Name})" );

                throw newEx;
            }
            catch ( Exception runtimeEx )
            {
                var ex = new Exceptions.SerializationException( runtimeEx );
                ex.AddTrace( $"{field} ({type.Name})" );

                throw ex;
            }
        }
    }

    public void WriteField( object? obj, string name )
    {
        WriteField( obj, name, name, null );
    }

    public void WriteField( object? obj, string name, Type elementType )
    {
        WriteField( obj, name, name, elementType );
    }

    public void WriteField( object? obj, string fieldName, string jsonName )
    {
        WriteField( obj, fieldName, jsonName, null );
    }

    /// <summary>
    /// Writes the specified field to the current JSON object.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fieldName"></param>
    /// <param name="jsonName"></param>
    /// <param name="elementType"> May be null if the type is unknown. </param>
    public void WriteField( object? obj, string fieldName, string jsonName, Type? elementType )
    {
        var type     = obj?.GetType();
        var metadata = GetFields( type )?.Get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type?.Name})" );
        }

        var field = metadata.FieldInfo;
        elementType ??= metadata.ElementType;

        try
        {
            Logger.Debug( $"Writing field: {field.Name} ({type?.Name})" );

            JsonWriter?.Name( jsonName );
            WriteValue( field.GetValue( obj ), field.GetType(), elementType );
        }
        catch ( FieldAccessException ex )
        {
            throw new SerializationException( $"Error accessing field: {field.Name} ({type?.Name})", ex );
        }
        catch ( SerializationException ex )
        {
            var newEx = new Exceptions.SerializationException( ex.Message );
            newEx.AddTrace( $"{field} ({type?.Name})" );

            throw newEx;
        }
        catch ( Exception runtimeEx )
        {
            var ex = new Utils.Exceptions.SerializationException( runtimeEx );
            ex.AddTrace( $"{field} ({type?.Name})" );

            throw ex;
        }
    }

    public void WriteObjectStart( string name )
    {
        try
        {
            JsonWriter?.Name( name );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }

        WriteObjectStart();
    }

    public void WriteObjectStart( string name, Type actualType, Type knownType )
    {
        try
        {
            JsonWriter?.Name( name );
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
            JsonWriter?.Object();
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }
    }

    /// <summary>
    /// Starts writing an object, writing the actualType to a field if needed.
    /// </summary>
    /// <param name="actualType"></param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    public void WriteObjectStart( Type actualType, Type? knownType )
    {
        try
        {
            JsonWriter?.Object();
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
            JsonWriter?.Pop();
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
            JsonWriter?.Name( name );
            JsonWriter?.Array();
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
            JsonWriter?.Array();
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
            JsonWriter?.Pop();
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

        try
        {
            JsonWriter?.Set( TypeName, className );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex.Message );
        }

        Logger.Debug( $"Writing type: {type.Name}" );
    }
}