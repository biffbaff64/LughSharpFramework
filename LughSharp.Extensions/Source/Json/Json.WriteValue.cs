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
using System.Reflection;

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;

namespace Extensions.Source.Json;

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
                Logger.Debug( "Value is null, quitting." );

                JsonWriter?.Value( null );

                return;
            }

            Logger.Debug( $"knownType: {knownType}" );
            Logger.Debug( $"elementType: {elementType}" );
            Logger.Debug( $"knownType is Primitive: {knownType?.IsPrimitive ?? false}" );
            
            if ( knownType is { IsPrimitive: true }
                 || ( knownType == typeof( string ) )
                 || ( knownType == typeof( int ) )
                 || ( knownType == typeof( bool ) )
                 || ( knownType == typeof( float ) )
                 || ( knownType == typeof( long ) )
                 || ( knownType == typeof( double ) )
                 || ( knownType == typeof( short ) )
                 || ( knownType == typeof( byte ) )
                 || ( knownType == typeof( char ) ) )
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

            if ( value is ICollection collection )
            {
                var type = value.GetType();

                if ( ( type != typeof( List< object > ) )
                     && ( ( knownType == null ) || ( knownType != type ) ) )
                {
                    WriteObjectStart( type, knownType );
                    WriteArrayStart( "items" );

                    foreach ( var item in collection )
                    {
                        WriteValue( item, elementType, null );
                    }

                    WriteArrayEnd();
                    WriteObjectEnd();
                }
                else
                {
                    WriteArrayStart();

                    foreach ( var item in collection )
                    {
                        WriteValue( item, elementType, null );
                    }

                    WriteArrayEnd();
                }

                return;
            }

            if ( value.GetType().IsArray )
            {
                var type          = value.GetType();
                var componentType = type.GetElementType();

                if ( elementType == null )
                {
                    elementType = componentType;
                }

                var arr    = ( Array )value;
                var length = arr.Length;

                WriteArrayStart();

                for ( var i = 0; i < length; i++ )
                {
                    WriteValue( arr.GetValue( i ), elementType, null );
                }

                WriteArrayEnd();

                return;
            }

            if ( value.GetType().IsGenericType
                 && ( value.GetType().GetGenericTypeDefinition() == typeof( Dictionary< , > ) ) )
            {
                var dictionary = ( IDictionary )value;

                WriteObjectStart( actualType, knownType );

                foreach ( var key in dictionary.Keys )
                {
                    if ( key != null )
                    {
                        JsonWriter?.Name( ConvertToString( key )! );
                        WriteValue( dictionary[ key ], elementType, null );
                    }
                }

                WriteObjectEnd();

                return;
            }

            // Enum special case.
            if ( value.GetType().IsEnum )
            {
                if ( ( TypeName != null ) && ( ( knownType == null ) || ( knownType != actualType ) ) )
                {
                    if ( !actualType.IsDefined( typeof( FlagsAttribute ), false )
                         && ( Enum.GetUnderlyingType( actualType ) == typeof( int ) )
                         && ( Enum.GetValues( actualType ).Length == 0 ) )
                    {
                        actualType = actualType.BaseType;
                    }

                    Guard.ThrowIfNull( actualType );

                    WriteObjectStart( actualType, null );
                    JsonWriter?.Name( "value" );
                    JsonWriter?.Value( ConvertToString( ( Enum )value ) );
                    WriteObjectEnd();
                }
                else
                {
                    JsonWriter?.Value( ConvertToString( value ) );
                }
            }

            // Other types
            if ( !actualType.IsPrimitive
                 && ( actualType != typeof( string ) )
                 && ( actualType != typeof( decimal ) )
                 && ( actualType != typeof( DateTime ) ) ) // Basic check to identify complex objects
            {
                AddClassTag( actualType.Name, actualType );
                
                WriteObjectStart( actualType, knownType );

                Logger.Checkpoint();
                
                var properties    = actualType.GetProperties( BindingFlags.Public | BindingFlags.Instance );
                var firstProperty = true;

                foreach ( var property in properties )
                {
                    if ( property.CanRead )
                    {
                        var propertyValue = property.GetValue( value );

                        if ( !firstProperty )
                        {
                            Console.Write( "," ); // Add comma between properties
                        }

                        JsonWriter?.Name( property.Name );
                        WriteValue( propertyValue, property.PropertyType, null ); // Recursive call for property value

                        firstProperty = false;
                    }
                }

                WriteObjectEnd();

                return;
            }

            Logger.Checkpoint();
            Logger.Debug( $"actualType: {actualType.Name}" );
            Logger.Debug( $"knownType: {knownType?.Name}" );
            Logger.Debug( $"value: {value.GetType().Name}" );

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
                var newEx = new LughSharp.Lugh.Utils.Exceptions.SerializationException( ex.Message );
                newEx.AddTrace( $"{field} ({type.Name})" );

                throw newEx;
            }
            catch ( Exception runtimeEx )
            {
                var ex = new LughSharp.Lugh.Utils.Exceptions.SerializationException( runtimeEx );
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
            var newEx = new LughSharp.Lugh.Utils.Exceptions.SerializationException( ex.Message );
            newEx.AddTrace( $"{field} ({type?.Name})" );

            throw newEx;
        }
        catch ( Exception runtimeEx )
        {
            var ex = new LughSharp.Lugh.Utils.Exceptions.SerializationException( runtimeEx );
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

        var className = GetTag( type ) ?? type.Name;

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