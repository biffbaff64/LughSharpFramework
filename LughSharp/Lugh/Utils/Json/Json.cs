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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Maps;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Collections.DeleteCandidates;
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Guarding;

using SerializationException = LughSharp.Lugh.Utils.Exceptions.SerializationException;

namespace LughSharp.Lugh.Utils.Json;

[PublicAPI]
public partial class Json
{
    /// <summary>
    /// When true, fields in the Json that are not found on the class will not throw a
    /// <see cref="SerializationException"/>. 
    /// Default is false.
    /// </summary>
    public bool IgnoreUnknownFields { get; set; }

    /// <summary>
    /// When true, fields with the <c> deprecated </c> annotation will not be read
    /// or written. Default is false.
    /// </summary>
    public bool IgnoreDeprecated { get; set; }

    /// <summary>
    /// When true, fields with the <c> deprecated </c> annotation will be read (but not
    /// written) when <see cref="IgnoreDeprecated"/> is true. Default is false.
    /// </summary>
    public bool ReadDeprecated { get; set; }

    /// <summary>
    /// When true, <see cref="Enum.GetName"/> is used to write enum values. When false,
    /// <see cref="Enum.ToString()"/> is used which may not be unique. Default is true.
    /// </summary>
    public bool EnumNames { get; set; } = true;

    /// <summary>
    /// Sets the name of the JSON field to store the Java class name or class tag when required
    /// to avoid ambiguity during deserialization. Set to null to never output this information,
    /// but be warned that deserialization may fail.
    /// Default is "class".
    /// </summary>
    public string TypeName { get; set; } = "class";

    /// <summary>
    /// Sets the serializer to use when the type being deserialized is not known (null).
    /// </summary>
    public IJsonSerializer? DefaultSerializer { get; set; }

    /// <summary>
    /// When true, field values that are identical to a newly constructed instance are not
    /// written. Default is true.
    /// </summary>
    public bool UsePrototypes { get; set; } = true;

    /// <summary>
    /// When true, fields are sorted alphabetically when written, otherwise the source
    /// code order is used. Default is false.
    /// </summary>
    public bool SortFields { get; set; }

    public JsonWriter?     JsonWriter      { get; set; }
    public JsonOutputType? OutputType      { get; set; }
    public bool            QuoteLongValues { get; set; } = false;

    // ========================================================================

    private Dictionary< Type, OrderedMap< string, FieldMetadata >? > _typeToFields         = [ ];
    private Dictionary< string, Type >                               _tagToClass           = [ ];
    private Dictionary< Type, string >                               _classToTag           = [ ];
    private Dictionary< Type, IJsonSerializer >                      _classToSerializer    = [ ];
    private Dictionary< Type, object[]? >                            _classToDefaultValues = [ ];
    private object[]?                                                _equals1              = [ ];
    private object[]?                                                _equals2              = [ ];
    private TextWriter?                                              _textWriter;

    // ========================================================================

    public Json()
    {
        OutputType = JsonOutputType.Minimal;
    }

    public Json( JsonOutputType outputType )
    {
        OutputType = outputType;
    }

    // ========================================================================

    /// <summary>
    /// Sets a tag to use instead of the fully qualifier class name. This can make
    /// the JSON easier to read.
    /// </summary>
    public void AddClassTag( string tag, Type type )
    {
        _tagToClass[ tag ]  = type;
        _classToTag[ type ] = tag;
    }

    /// <summary>
    /// Returns the class for the specified tag, or null.
    /// </summary>
    public Type GetType( string tag )
    {
        return _tagToClass[ tag ];
    }

    /// <summary>
    /// Returns the tag for the specified class, or null.
    /// </summary>
    public string GetTag( Type type )
    {
        return _classToTag[ type ];
    }

    /// <summary>
    /// Registers a serializer to use for the specified type instead of the default behavior
    /// of serializing all of an objects fields.
    /// </summary>
    public void SetSerializer< T >( Type type, IJsonSerializer serializer )
    {
        _classToSerializer[ type ] = serializer;
    }

    /// <summary>
    /// Gets the <see cref="IJsonSerializer"/> that has been registered for the supplied
    /// <see cref="Type"/>.
    /// </summary>
    public IJsonSerializer GetSerializer< T >( Type type )
    {
        return _classToSerializer[ type ];
    }

    /// <summary>
    /// Sets the type of elements in a collection. When the element type is known, the
    /// class for each element in the collection does not need to be written unless
    /// different from the element type.
    /// </summary>
    public void SetElementType( Type type, string fieldName, Type elementType )
    {
        var metadata = GetFields( type ).Get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type.Name})" );
        }

        metadata.ElementType = elementType;
    }

    /// <summary>
    /// The specified field will be treated as if it has or does not have the
    /// <see cref="Deprecated"/> annotation.
    /// </summary>

    //TODO: Should this be 'Obsolete' in line with C#?
    public void SetDeprecated( Type type, string fieldName, bool deprecated )
    {
        var metadata = GetFields( type ).Get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type.Name})" );
        }

        metadata.Deprecated = deprecated;
    }

    /// <summary>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private OrderedMap< string, FieldMetadata >? GetFields( Type type )
    {
        if ( _typeToFields.TryGetValue( type, out var fields ) )
        {
            return fields;
        }

        var classHierarchy = new List< Type >();
        var nextClass      = type;

        while ( ( nextClass != typeof( object ) ) && ( nextClass != null ) )
        {
            classHierarchy.Add( nextClass );
            nextClass = nextClass.BaseType;
        }

        List< FieldInfo > allFields = [ ];

        for ( var i = classHierarchy.Count - 1; i >= 0; i-- )
        {
            allFields.AddRange( classHierarchy[ i ].GetFields( BindingFlags.DeclaredOnly
                                                               | BindingFlags.Instance
                                                               | BindingFlags.Public
                                                               | BindingFlags.NonPublic ) );
        }

        var nameToField = new OrderedMap< string, FieldMetadata >( allFields.Count );

        for ( int i = 0, n = allFields.Count; i < n; i++ )
        {
            var field = allFields[ i ];

            if ( field.GetCustomAttribute< NonSerializedAttribute >() != null ) continue;
            if ( field.IsStatic ) continue;
            if ( field.IsPrivate && field.Name.Contains( '<' ) ) continue;

            if ( !field.IsPublic )
            {
                try
                {
                    //TODO:
                    // No direct equivalent to setAccessible. In C#, private fields are
                    // accessible via reflection.
                    // If there are specific security constraints, handle them here.
                }
                catch ( SecurityException )
                {
                    continue;
                }
            }

            nameToField.Put( field.Name, new FieldMetadata( field ) );
        }

        if ( SortFields )
        {
            nameToField.OrderedKeys().Sort();
        }

        _typeToFields[ type ] = nameToField;

        return nameToField;
    }

    /// <summary>
    /// Sets the writer where JSON output will be written. This is only necessary when
    /// not using the ToJson methods.
    /// </summary>
    public void SetWriter( TextWriter writer )
    {
        if ( writer is not Utils.Json.JsonWriter )
        {
            writer = new JsonWriter( writer );
        }

        this.JsonWriter = ( JsonWriter )writer;
        this.JsonWriter.SetOutputType( OutputType );
        this.JsonWriter.SetQuoteLongValues( QuoteLongValues );
    }

    /// <summary>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    private object[]? GetDefaultValues( Type type )
    {
        if ( !UsePrototypes ) return null;

        if ( _classToDefaultValues.TryGetValue( type, out var value ) )
        {
            return value;
        }

        object? instance;

        try
        {
            instance = NewInstance( type );
        }
        catch ( Exception ex )
        {
            _classToDefaultValues[ type ] = null;

            return null;
        }

        var fields = GetFields( type ) ?? throw new GdxRuntimeException( "GetFields returned NULL" );
        var values = new object[ fields.Size ];

        _classToDefaultValues[ type ] = values;

        var defaultIndex = 0;
        var fieldNames   = fields.OrderedKeys();

        for ( int i = 0, n = fieldNames.Count; i < n; i++ )
        {
            var metadata = fields.Get( fieldNames[ i ] );

            if ( IgnoreDeprecated && ( bool )metadata?.Deprecated ) continue;

            var field = metadata?.Field;

            Guard.ThrowIfNull( field );
            
            try
            {
                values[ defaultIndex++ ] = field.GetValue( instance )!;
            }
            catch ( FieldAccessException ex )
            {
                throw new SerializationException( $"Error accessing field: {field.Name} " +
                                                  $"({type.Name})", ex );
            }
            catch ( SerializationException ex )
            {
                var newEx = new SerializationException( ex.Message );
                newEx.AddTrace( field + " (" + type.Name + ")" );

                throw newEx;
            }
            catch ( GdxRuntimeException runtimeEx )
            {
                var ex = new SerializationException( runtimeEx.ToString() );
                ex.AddTrace( field + " (" + type.Name + ")" );

                throw ex;
            }
        }

        return values;
    }

    public void ReadField( object @object, string name, JsonValue jsonData )
    {
        ReadField( @object, name, name, null, jsonData );
    }

    public void ReadField( object @object, string name, Type elementType, JsonValue jsonData )
    {
        ReadField( @object, name, name, elementType, jsonData );
    }

    public void ReadField( object @object, string fieldName, string jsonName, JsonValue jsonData )
    {
        ReadField( @object, fieldName, jsonName, null, jsonData );
    }

    /** @param elementType May be null if the type is unknown. */
    public void ReadField( object @object, string fieldName, string jsonName, Type elementType, JsonValue jsonMap )
    {
        var           type     = @object.GetType();
        FieldMetadata metadata = GetFields( type ).get( fieldName );

        if ( metadata == null ) throw new SerializationException( "Field not found: " + fieldName + " (" + type.getName() + ")" );

        Field field                            = metadata.field;
        if ( elementType == null ) elementType = metadata.elementType;
        ReadField( @object, field, jsonName, elementType, jsonMap );
    }

    /** @param object May be null if the field is static.
     * @param elementType May be null if the type is unknown. */
    public void ReadField( object @object, Field field, string jsonName, Type elementType, JsonValue jsonMap )
    {
        var jsonValue = jsonMap.Get( jsonName );

        if ( jsonValue == null ) return;

        try
        {
            field.set( @object, ReadValue< T >( field.getType(), elementType, jsonValue ) );
        }
        catch ( ReflectionException ex )
        {
            throw new SerializationException(
                                             "Error accessing field: " + field.getName() + " (" + field.getDeclaringClass().getName() + ")",
                                             ex );
        }
        catch ( SerializationException ex )
        {
            ex.addTrace( field.getName() + " (" + field.getDeclaringClass().getName() + ")" );

            throw ex;
        }
        catch ( RuntimeException runtimeEx )
        {
            var ex = new SerializationException( runtimeEx );
            ex.addTrace( jsonValue.trace() );
            ex.addTrace( field.getName() + " (" + field.getDeclaringClass().getName() + ")" );

            throw ex;
        }
    }

    public void ReadFields( object @object, JsonValue jsonMap )
    {
        var                                 type   = @object.GetType();
        OrderedMap< string, FieldMetadata > fields = GetFields( type );

        for ( var child = jsonMap.Child; child != null; child = child.Next )
        {
            FieldMetadata metadata = fields.get( child.Name().replace( " ", "_" ) );

            if ( metadata == null )
            {
                if ( child.Name.equals( typeName ) ) continue;

                if ( ignoreUnknownFields || IgnoreUnknownField( type, child.Name ) )
                {
                    if ( debug ) System.out.
                    println( "Ignoring unknown field: " + child.Name + " (" + type.getName() + ")" );

                    continue;
                }
                else
                {
                    var ex = new SerializationException(
                                                        "Field not found: " + child.Name + " (" + type.getName() + ")" );
                    ex.addTrace( child.trace() );

                    throw ex;
                }
            }
            else
            {
                if ( ignoreDeprecated && !readDeprecated && metadata.deprecated ) continue;
            }

            Field field = metadata.field;

            try
            {
                field.set( @object, ReadValue( field.getType(), metadata.elementType, child ) );
            }
            catch ( ReflectionException ex )
            {
                throw new SerializationException( "Error accessing field: " + field.getName() + " (" + type.getName() + ")", ex );
            }
            catch ( SerializationException ex )
            {
                ex.addTrace( field.getName() + " (" + type.getName() + ")" );

                throw ex;
            }
            catch ( RuntimeException runtimeEx )
            {
                var ex = new SerializationException( runtimeEx );
                ex.addTrace( child.trace() );
                ex.addTrace( field.getName() + " (" + type.getName() + ")" );

                throw ex;
            }
        }
    }

    /** Called for each unknown field name encountered by {@link #readFields(object, JsonValue)} when {@link #ignoreUnknownFields}
     * is false to determine whether the unknown field name should be ignored.
     * @param type The object type being read.
     * @param fieldName A field name encountered in the JSON for which there is no matching class field.
     * @return true if the field name should be ignored and an exception won't be thrown by
     *         {@link #readFields(object, JsonValue)}. */
    protected bool IgnoreUnknownField( Type type, string fieldName )
    {
        return false;
    }

    /** @param type May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( string name, Type type, JsonValue jsonMap )
    {
        return ReadValue< T >( type, null, jsonMap.Get( name ) );
    }

    /** @param type May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( string name, Type type, T defaultValue, JsonValue jsonMap )
    {
        var jsonValue = jsonMap.Get( name );

        if ( jsonValue == null ) return defaultValue;

        return ReadValue< T >( type, null, jsonValue );
    }

    /** @param type May be null if the type is unknown.
     * @param elementType May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( string name, Type type, Type elementType, JsonValue jsonMap )
    {
        return ReadValue< T >( type, elementType, jsonMap.Get( name ) );
    }

    /** @param type May be null if the type is unknown.
     * @param elementType May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( string name, Type type, Type elementType, T defaultValue, JsonValue jsonMap )
    {
        var jsonValue = jsonMap.Get( name );

        return ReadValue< T >( type, elementType, defaultValue, jsonValue );
    }

    /** @param type May be null if the type is unknown.
     * @param elementType May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( Type type, Type elementType, T defaultValue, JsonValue jsonData )
    {
        if ( jsonData == null ) return defaultValue;

        return ReadValue< T >( type, elementType, jsonData );
    }

    /** @param type May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( Type type, JsonValue jsonData )
    {
        return ReadValue< T >( type, null, jsonData );
    }

    /** @param type May be null if the type is unknown.
     * @param elementType May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( Type type, Type? elementType, JsonValue jsonData )
    {
        if ( jsonData == null ) return null;

        if ( jsonData.isObject() )
        {
            string className = typeName == null ? null : jsonData.getString( typeName, null );

            if ( className != null )
            {
                type = GetType( className );

                if ( type == null )
                {
                    try
                    {
                        type = ClassReflection.forName( className );
                    }
                    catch ( ReflectionException ex )
                    {
                        throw new SerializationException( ex );
                    }
                }
            }

            if ( type == null )
            {
                if ( defaultSerializer != null ) return ( T )defaultSerializer.read( this, jsonData, type );

                return ( T )jsonData;
            }

            if ( typeName != null && ClassReflection.isAssignableFrom( Collection.class, type)) {
                // JSON object wrapper to specify type.
                jsonData = jsonData.Get( "items" );

                if ( jsonData == null )
                    throw new SerializationException(
                                                     "Unable to convert object to collection: " + jsonData + " (" + type.getName() + ")" );
            } else {
                Serializer serializer = classToSerializer.get( type );

                if ( serializer != null ) return ( T )serializer.read( this, jsonData, type );

                if ( type == string.class || type == Integer.class || type == Boolean.class || type == Float.class
                || type == Long.class || type == Double.class || type == Short.class || type == Byte.class
                || type == Character.class || ClassReflection.isAssignableFrom( Enum.class, type)) {
                    return ReadValue< T >( "value", type, jsonData );
                }

                var @object = newInstance( type );

                if ( object is Serializable )
                {
                    ( ( Serializable )object ).read( this, jsonData );

                    return ( T )object;
                }

                // JSON object special cases.
                if ( object is ObjectMap )
                {
                    var result = ( ObjectMap )object;
                    for ( var child = jsonData.Child; child != null; child = child.Next )
                        result.put( child.Name, ReadValue< T >( elementType, null, child ) );

                    return ( T )result;
                }

                if ( object is ObjectIntMap )
                {
                    ObjectIntMap result = ( ObjectIntMap )object;
                    for ( var child = jsonData.Child; child != null; child = child.Next )
                        result.put( child.Name, ReadValue< T >( Integer.class, null, child));

                    return ( T )result;
                }

                if ( object is ObjectFloatMap )
                {
                    ObjectFloatMap result = ( ObjectFloatMap )object;
                    for ( var child = jsonData.Child; child != null; child = child.Next )
                        result.put( child.Name, ReadValue< T >( Float.class, null, child));

                    return ( T )result;
                }

                if ( object is ObjectSet )
                {
                    ObjectSet result = ( ObjectSet )object;
                    for ( var child = jsonData.GetChild( "values" ); child != null; child = child.Next )
                        result.add( ReadValue< T >( elementType, null, child ) );

                    return ( T )result;
                }

                if ( object is IntMap )
                {
                    IntMap result = ( IntMap )object;
                    for ( var child = jsonData.Child; child != null; child = child.Next )
                        result.put( Integer.parseInt( child.Name ), ReadValue< T >( elementType, null, child ) );

                    return ( T )result;
                }

                if ( object is LongMap )
                {
                    LongMap result = ( LongMap )object;
                    for ( var child = jsonData.Child; child != null; child = child.Next )
                        result.put( Long.parseLong( child.Name ), ReadValue< T >( elementType, null, child ) );

                    return ( T )result;
                }

                if ( object is IntSet )
                {
                    IntSet result = ( IntSet )object;
                    for ( var child = jsonData.GetChild( "values" ); child != null; child = child.Next )
                        result.add( child.asInt() );

                    return ( T )result;
                }

                if ( object is ArrayMap )
                {
                    ArrayMap result = ( ArrayMap )object;
                    for ( var child = jsonData.Child; child != null; child = child.Next )
                        result.put( child.Name, ReadValue< T >( elementType, null, child ) );

                    return ( T )result;
                }

                if ( object is Map )
                {
                    var result = ( Map )object;

                    for ( var child = jsonData.Child; child != null; child = child.Next )
                    {
                        if ( child.Name.equals( typeName ) ) continue;

                        result.put( child.Name, ReadValue< T >( elementType, null, child ) );
                    }

                    return ( T )result;
                }

                ReadFields( object, jsonData );

                return ( T )object;
            }
        }

        if ( type != null )
        {
            Serializer serializer = classToSerializer.get( type );

            if ( serializer != null ) return ( T )serializer.read( this, jsonData, type );

            if ( ClassReflection.isAssignableFrom( Serializable.class, type)) {
                // A Serializable may be read as an array, string, etc, even though it will be written as an object.
                var @object = newInstance( type );
                ( ( Serializable )object ).read( this, jsonData );

                return ( T )object;
            }
        }

        if ( jsonData.isArray() )
        {
            // JSON array special cases.
            if ( type == null || type == object.class) type = ( Type )List.class;
            if ( ClassReflection.isAssignableFrom( List.class, type)) {
                List result = type == List.class ? new List() : ( List )newInstance( type );
                for ( var child = jsonData.Child; child != null; child = child.Next )
                    result.add( ReadValue< T >( elementType, null, child ) );

                return ( T )result;
            }
            if ( ClassReflection.isAssignableFrom( Queue.class, type)) {
                Queue result = type == Queue.class ? new Queue() : ( Queue )newInstance( type );
                for ( var child = jsonData.Child; child != null; child = child.Next )
                    result.addLast( ReadValue< T >( elementType, null, child ) );

                return ( T )result;
            }
            if ( ClassReflection.isAssignableFrom( Collection.class, type)) {
                Collection result = type.isInterface() ? new List() : ( Collection )newInstance( type );
                for ( var child = jsonData.Child; child != null; child = child.Next )
                    result.add( ReadValue< T >( elementType, null, child ) );

                return ( T )result;
            }

            if ( type.isArray() )
            {
                Type? componentType                    = type.getComponentType();
                if ( elementType == null ) elementType = componentType;
                object result                          = ArrayReflection.newInstance( componentType, jsonData.Size );
                var    i                               = 0;
                for ( var child = jsonData.Child; child != null; child = child.Next )
                    ArrayReflection.set( result, i++, ReadValue< T >( elementType, null, child ) );

                return ( T )result;
            }

            throw new SerializationException( "Unable to convert value to required type: " + jsonData + " (" + type.getName() + ")" );
        }

        if ( jsonData.isNumber() )
        {
            try
            {
                if ( type == null || type == float.class || type == Float.class) return ( T )( Float )jsonData.asFloat();

                if ( type == int.class || type == Integer.class) return ( T )( Integer )jsonData.asInt();

                if ( type == long.class || type == Long.class) return ( T )( Long )jsonData.asLong();

                if ( type == double.class || type == Double.class) return ( T )( Double )jsonData.asDouble();

                if ( type == string.class) return ( T )jsonData.asString();

                if ( type == short.class || type == Short.class) return ( T )( Short )jsonData.AsShort();

                if ( type == byte.class || type == Byte.class) return ( T )( Byte )jsonData.AsByte();
            }
            catch ( NumberFormatException ignored )
            {
            }

            jsonData = new JsonValue( jsonData.asString() );
        }

        if ( jsonData.isBoolean() )
        {
            try
            {
                if ( type == null || type == bool.class || type == Boolean.class) return ( T )( Boolean )jsonData.asBoolean();
            }
            catch ( NumberFormatException ignored )
            {
            }

            jsonData = new JsonValue( jsonData.asString() );
        }

        if ( jsonData.isString() )
        {
            string string = jsonData.asString();
            if ( type == null || type == string.class) return ( T )string;

            try
            {
                if ( type == int.class || type == Integer.class) return ( T )Integer.valueOf( string );

                if ( type == float.class || type == Float.class) return ( T )Float.valueOf( string );

                if ( type == long.class || type == Long.class) return ( T )Long.valueOf( string );

                if ( type == double.class || type == Double.class) return ( T )Double.valueOf( string );

                if ( type == short.class || type == Short.class) return ( T )Short.valueOf( string );

                if ( type == byte.class || type == Byte.class) return ( T )Byte.valueOf( string );
            }
            catch ( NumberFormatException ignored )
            {
            }

            if ( type == bool.class || type == Boolean.class) return ( T )Boolean.valueOf( string );

            if ( type == char.class || type == Character.class) return ( T )( Character )string.charAt( 0 );

            if ( ClassReflection.isAssignableFrom( Enum.class, type)) {
                Enum[] constants = ( Enum[] )type.getEnumConstants();

                for ( int i = 0, n = constants.length; i < n; i++ )
                {
                    var e = constants[ i ];

                    if ( string.equals( convertToString( e ) ) ) return ( T )e;
                }
            }
            if ( type == CharSequence.class) return ( T )string;

            throw new SerializationException( "Unable to convert value to required type: " + jsonData + " (" + type.getName() + ")" );
        }

        return null;
    }

    /// <summary>
    /// Each field on the <c>to</c> object is set to the value for the field with the same name
    /// on the <c>from</c> object. The <c>to</c> object must have at least all the fields of the
    /// <c>from</c> object with the same name and type.
    /// </summary>
    public void CopyFields( object from, object to )
    {
        var toFields = GetFields( to.GetType() );

        foreach ( ObjectMap.Entry< string, FieldMetadata > entry in GetFields( from.GetType() ) )
        {
            FieldMetadata toField   = toFields.get( entry.key );
            Field         fromField = entry.value.field;

            if ( toField == null ) throw new SerializationException( "To object is missing field: " + entry.key );

            try
            {
                toField.Field.set( to, fromField.get( from ) );
            }
            catch ( FieldAccessException ex )
            {
                throw new SerializationException( "Error copying field: " + fromField.getName(), ex );
            }
        }
    }

    // ========================================================================

    private string? ConvertToString( Enum e )
    {
        return EnumNames ? Enum.GetName( e.GetType(), e ) : e.ToString();
    }

    private string? ConvertToString( object obj )
    {
        return obj switch
        {
            Enum enumType => ConvertToString( enumType ),
            Type typeType => typeType.FullName,
            var _         => StringUtils.ValueOf( obj ) ?? "",
        };
    }

    // ========================================================================

    protected object? NewInstance( Type type )
    {
        try
        {
            return Activator.CreateInstance( type );
        }
        catch ( Exception ex )
        {
            try
            {
                // Try a private constructor.
                var constructor = type.GetConstructor( ( BindingFlags.Instance | BindingFlags.NonPublic ),
                                                       null,
                                                       Type.EmptyTypes,
                                                       null );

                if ( constructor != null )
                {
                    return constructor.Invoke( null );
                }
            }
            catch ( SecurityException )
            {
                // Ignored
            }
            catch ( TargetInvocationException )
            {
                if ( type.IsEnum )
                {
                    var enumValues = Enum.GetValues( type );

                    if ( enumValues.Length > 0 )
                    {
                        return enumValues.GetValue( 0 );
                    }
                }

                if ( type.IsArray )
                {
                    throw new Exception( $"Encountered JSON object when expected array of type: {type.FullName}", ex );
                }

                if ( type is { IsNested: true, IsNestedPublic: false } )
                {
                    throw new Exception( $"Type cannot be created (non-static member class): {type.FullName}", ex );
                }

                throw new Exception( $"Type cannot be created (missing no-arg constructor): {type.FullName}", ex );
            }
            catch ( Exception privateConstructorException )
            {
                ex = privateConstructorException;
            }

            throw new Exception( $"Error constructing instance of class: {type.FullName}", ex );
        }
    }

    // ========================================================================

    public string PrettyPrint( object obj )
    {
        return PrettyPrint( obj, 0 );
    }

    public string PrettyPrint( string json )
    {
        return PrettyPrint( json, 0 );
    }

    public string PrettyPrint( object obj, int singleLineColumns )
    {
        return PrettyPrint( ToJson( obj ), singleLineColumns );
    }

    public string PrettyPrint( string json, int singleLineColumns )
    {
        return new JsonReader().Parse( json ).PrettyPrint( OutputType, singleLineColumns );
    }

    public string PrettyPrint( object obj, JsonValue.PrettyPrintSettings settings )
    {
        return PrettyPrint( ToJson( obj ), settings );
    }

    public string PrettyPrint( string json, JsonValue.PrettyPrintSettings settings )
    {
        return new JsonReader().Parse( json ).PrettyPrint( settings );
    }

    // ========================================================================
    // ========================================================================

    private class FieldMetadata
    {
        public FieldInfo Field       { get; }
        public Type?     ElementType { get; set; }
        public bool      Deprecated  { get; set; }

        public FieldMetadata( FieldInfo field )
        {
            Field = field;
            var index = ( typeof( IDictionary< , > ).IsAssignableFrom( field.FieldType.GetGenericTypeDefinition() )
                          || typeof( System.Collections.IDictionary ).IsAssignableFrom( field.FieldType ) )
                ? 1
                : 0;

            if ( field.FieldType.IsGenericType && ( field.FieldType.GetGenericArguments().Length > index ) )
            {
                ElementType = field.FieldType.GetGenericArguments()[ index ];
            }
            else
            {
                ElementType = field.FieldType.GetElementType();
            }

            Deprecated = field.GetCustomAttribute< ObsoleteAttribute >() != null;
        }
    }
}