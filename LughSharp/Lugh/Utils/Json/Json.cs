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
    public Json.ISerializer? DefaultSerializer { get; set; }

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

    public JsonWriter?    JsonWriter      { get; set; }
    public JsonOutputType OutputType      { get; set; }
    public bool           QuoteLongValues { get; set; } = false;

    // ========================================================================

    private Dictionary< Type, OrderedDictionary > _typeToFields         = [ ];
    private Dictionary< string, Type >            _tagToClass           = [ ];
    private Dictionary< Type, string >            _classToTag           = [ ];
    private Dictionary< Type, ISerializer >       _classToSerializer    = [ ];
    private Dictionary< Type, object[] >          _classToDefaultValues = [ ];
    private object[]?                             _equals1              = [ ];
    private object[]?                             _equals2              = [ ];
    private TextWriter                            _textWriter;

    // ========================================================================

    public Json()
    {
        this.OutputType = JsonOutputType.Minimal;
    }

    public Json( JsonOutputType outputType )
    {
        this.OutputType = outputType;
    }

    // ========================================================================

    /** Sets a tag to use instead of the fully qualifier class name. This can make the JSON easier to read. */
    public void AddClassTag( string tag, Type type )
    {
        _tagToClass[ tag ]  = type;
        _classToTag[ type ] = tag;
    }

    /** Returns the class for the specified tag, or null. */
    public Type GetType( string tag )
    {
        return _tagToClass[ tag ];
    }

    /** Returns the tag for the specified class, or null. */
    public string GetTag( Type type )
    {
        return _classToTag[ type ];
    }

    /** Registers a serializer to use for the specified type instead of the default behavior of serializing all of an objects
     * fields. */
    public void SetSerializer< T >( Type type, ISerializer< T > serializer )
    {
        _classToSerializer[ type ] = serializer;
    }

    public ISerializer< T > GetSerializer< T >( Type type )
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
        FieldMetadata metadata = getFields( type ).get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type.Name})" );
        }

        metadata.ElementType = elementType;
    }

    /** The specified field will be treated as if it has or does not have the {@link Deprecated} annotation.
     * @see #setIgnoreDeprecated(bool)
     * @see #setReadDeprecated(bool) */
    public void SetDeprecated( Type type, string fieldName, bool deprecated )
    {
        FieldMetadata metadata = getFields( type ).get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type.Name})" );
        }

        metadata.Deprecated = deprecated;
    }

    private OrderedMap< string, FieldMetadata > getFields( Type type )
    {
        OrderedMap< string, FieldMetadata > fields = typeToFields.get( type );

        if ( fields != null ) return fields;

        Array< Type > classHierarchy = new Array();
        var           nextClass      = type;
        while ( nextClass != object.class) {
            classHierarchy.add( nextClass );
            nextClass = nextClass.getSuperclass();
        }
        List< Field > allFields = [ ];
        for ( var i = classHierarchy.size - 1; i >= 0; i-- )
            Collections.addAll( allFields, ClassReflection.getDeclaredFields( classHierarchy.get( i ) ) );

        OrderedMap< string, FieldMetadata > nameToField = new OrderedMap( allFields.size() );

        for ( int i = 0, n = allFields.size(); i < n; i++ )
        {
            Field field = allFields.get( i );

            if ( field.isTransient() ) continue;
            if ( field.isStatic() ) continue;
            if ( field.isSynthetic() ) continue;

            if ( !field.isAccessible() )
            {
                try
                {
                    field.setAccessible( true );
                }
                catch ( AccessControlException ex )
                {
                    continue;
                }
            }

            nameToField.put( field.getName(), new FieldMetadata( field ) );
        }

        if ( sortFields ) nameToField.keys.sort();
        typeToFields.put( type, nameToField );

        return nameToField;
    }

    /** Sets the writer where JSON output will be written. This is only necessary when not using the ToJson methods. */
    public void SetWriter( TextWriter writer )
    {
        if ( !( writer is JsonWriter ) ) writer = new JsonWriter( writer );
        this.writer = ( JsonWriter )writer;
        this.writer.setOutputType( outputType );
        this.writer.setQuoteLongValues( quoteLongValues );
    }

    public JsonWriter getWriter()
    {
        return writer;
    }

    private object[] getDefaultValues( Type type )
    {
        if ( !usePrototypes ) return null;
        if ( classToDefaultValues.containsKey( type ) ) return classToDefaultValues.get( type );

        object @object;

        try
        {
            object = newInstance( type );
        }
        catch ( Exception ex )
        {
            classToDefaultValues.put( type, null );

            return null;
        }

        OrderedMap< string, FieldMetadata > fields = getFields( type );
        var                                 values = new object[ fields.size ];
        classToDefaultValues.put( type, values );

        var             defaultIndex = 0;
        Array< string > fieldNames   = fields.orderedKeys();

        for ( int i = 0, n = fieldNames.size; i < n; i++ )
        {
            FieldMetadata metadata = fields.get( fieldNames.get( i ) );

            if ( ignoreDeprecated && metadata.deprecated ) continue;

            Field field = metadata.field;

            try
            {
                values[ defaultIndex++ ] = field.get( object );
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
            catch ( RuntimeException runtimeEx )
            {
                var ex = new SerializationException( runtimeEx );
                ex.addTrace( field + " (" + type.getName() + ")" );

                throw ex;
            }
        }

        return values;
    }

    public void readField( object @object, string name, JsonValue jsonData )
    {
        readField( object, name, name, null, jsonData );
    }

    public void readField( object @object, string name, Type elementType, JsonValue jsonData )
    {
        readField( object, name, name, elementType, jsonData );
    }

    public void readField( object @object, string fieldName, string jsonName, JsonValue jsonData )
    {
        readField( object, fieldName, jsonName, null, jsonData );
    }

    /** @param elementType May be null if the type is unknown. */
    public void readField( object @object, string fieldName, string jsonName, Type elementType, JsonValue jsonMap )
    {
        Type          type     = object.GetType();
        FieldMetadata metadata = getFields( type ).get( fieldName );

        if ( metadata == null ) throw new SerializationException( "Field not found: " + fieldName + " (" + type.getName() + ")" );

        Field field                            = metadata.field;
        if ( elementType == null ) elementType = metadata.elementType;
        readField( object, field, jsonName, elementType, jsonMap );
    }

    /** @param object May be null if the field is static.
     * @param elementType May be null if the type is unknown. */
    public void readField( object @object, Field field, string jsonName, Type elementType, JsonValue jsonMap )
    {
        JsonValue jsonValue = jsonMap.Get( jsonName );

        if ( jsonValue == null ) return;

        try
        {
            field.set( object, ReadValue( field.getType(), elementType, jsonValue ) );
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

    public void readFields( object @object, JsonValue jsonMap )
    {
        Type                                type   = object.GetType();
        OrderedMap< string, FieldMetadata > fields = getFields( type );

        for ( JsonValue child = jsonMap.Child; child != null; child = child.Next )
        {
            FieldMetadata metadata = fields.get( child.Name().replace( " ", "_" ) );

            if ( metadata == null )
            {
                if ( child.Name.equals( typeName ) ) continue;

                if ( ignoreUnknownFields || ignoreUnknownField( type, child.Name ) )
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
                field.set( object, ReadValue( field.getType(), metadata.elementType, child ) );
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
    protected bool ignoreUnknownField( Type type, string fieldName )
    {
        return false;
    }

    /** @param type May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( string name, Type< T > type, JsonValue jsonMap )
    {
        return ReadValue( type, null, jsonMap.Get( name ) );
    }

    /** @param type May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( string name, Type< T > type, T defaultValue, JsonValue jsonMap )
    {
        JsonValue jsonValue = jsonMap.Get( name );

        if ( jsonValue == null ) return defaultValue;

        return ReadValue( type, null, jsonValue );
    }

    /** @param type May be null if the type is unknown.
     * @param elementType May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( string name, Type< T > type, Type elementType, JsonValue jsonMap )
    {
        return ReadValue( type, elementType, jsonMap.Get( name ) );
    }

    /** @param type May be null if the type is unknown.
     * @param elementType May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( string name, Type< T > type, Type elementType, T defaultValue, JsonValue jsonMap )
    {
        JsonValue jsonValue = jsonMap.Get( name );

        return ReadValue( type, elementType, defaultValue, jsonValue );
    }

    /** @param type May be null if the type is unknown.
     * @param elementType May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( Type< T > type, Type elementType, T defaultValue, JsonValue jsonData )
    {
        if ( jsonData == null ) return defaultValue;

        return ReadValue( type, elementType, jsonData );
    }

    /** @param type May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( Type< T > type, JsonValue jsonData )
    {
        return ReadValue( type, null, jsonData );
    }

    /** @param type May be null if the type is unknown.
     * @param elementType May be null if the type is unknown.
     * @return May be null. */
    public T ReadValue< T >( Type< T > type, Type elementType, JsonValue jsonData )
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
                    return ReadValue( "value", type, jsonData );
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
                    ObjectMap result = ( ObjectMap )object;
                    for ( JsonValue child = jsonData.Child; child != null; child = child.Next )
                        result.put( child.Name, ReadValue( elementType, null, child ) );

                    return ( T )result;
                }

                if ( object is ObjectIntMap )
                {
                    ObjectIntMap result = ( ObjectIntMap )object;
                    for ( JsonValue child = jsonData.Child; child != null; child = child.Next )
                        result.put( child.Name, ReadValue( Integer.class, null, child));

                    return ( T )result;
                }

                if ( object is ObjectFloatMap )
                {
                    ObjectFloatMap result = ( ObjectFloatMap )object;
                    for ( JsonValue child = jsonData.Child; child != null; child = child.Next )
                        result.put( child.Name, ReadValue( Float.class, null, child));

                    return ( T )result;
                }

                if ( object is ObjectSet )
                {
                    ObjectSet result = ( ObjectSet )object;
                    for ( JsonValue child = jsonData.GetChild( "values" ); child != null; child = child.Next )
                        result.add( ReadValue( elementType, null, child ) );

                    return ( T )result;
                }

                if ( object is IntMap )
                {
                    IntMap result = ( IntMap )object;
                    for ( JsonValue child = jsonData.Child; child != null; child = child.Next )
                        result.put( Integer.parseInt( child.Name ), ReadValue( elementType, null, child ) );

                    return ( T )result;
                }

                if ( object is LongMap )
                {
                    LongMap result = ( LongMap )object;
                    for ( JsonValue child = jsonData.Child; child != null; child = child.Next )
                        result.put( Long.parseLong( child.Name ), ReadValue( elementType, null, child ) );

                    return ( T )result;
                }

                if ( object is IntSet )
                {
                    IntSet result = ( IntSet )object;
                    for ( JsonValue child = jsonData.GetChild( "values" ); child != null; child = child.Next )
                        result.add( child.asInt() );

                    return ( T )result;
                }

                if ( object is ArrayMap )
                {
                    ArrayMap result = ( ArrayMap )object;
                    for ( JsonValue child = jsonData.Child; child != null; child = child.Next )
                        result.put( child.Name, ReadValue( elementType, null, child ) );

                    return ( T )result;
                }

                if ( object is Map )
                {
                    var result = ( Map )object;

                    for ( JsonValue child = jsonData.Child; child != null; child = child.Next )
                    {
                        if ( child.Name.equals( typeName ) ) continue;

                        result.put( child.Name, ReadValue( elementType, null, child ) );
                    }

                    return ( T )result;
                }

                readFields( object, jsonData );

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
            if ( type == null || type == object.class) type = ( Type< T > )Array.class;
            if ( ClassReflection.isAssignableFrom( Array.class, type)) {
                Array result = type == Array.class ? new Array() : ( Array )newInstance( type );
                for ( JsonValue child = jsonData.Child; child != null; child = child.Next )
                    result.add( ReadValue( elementType, null, child ) );

                return ( T )result;
            }
            if ( ClassReflection.isAssignableFrom( Queue.class, type)) {
                Queue result = type == Queue.class ? new Queue() : ( Queue )newInstance( type );
                for ( JsonValue child = jsonData.Child; child != null; child = child.Next )
                    result.addLast( ReadValue( elementType, null, child ) );

                return ( T )result;
            }
            if ( ClassReflection.isAssignableFrom( Collection.class, type)) {
                Collection result = type.isInterface() ? new List() : ( Collection )newInstance( type );
                for ( JsonValue child = jsonData.Child; child != null; child = child.Next )
                    result.add( ReadValue( elementType, null, child ) );

                return ( T )result;
            }

            if ( type.isArray() )
            {
                Type componentType                     = type.getComponentType();
                if ( elementType == null ) elementType = componentType;
                object result                          = ArrayReflection.newInstance( componentType, jsonData.Size );
                var    i                               = 0;
                for ( JsonValue child = jsonData.Child; child != null; child = child.Next )
                    ArrayReflection.set( result, i++, ReadValue( elementType, null, child ) );

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

    /** Each field on the <code>to</code> object is set to the value for the field with the same name on the <code>from</code>
     * object. The <code>to</code> object must have at least all the fields of the <code>from</code> object with the same name and
     * type. */
    public void copyFields( object from, object to )
    {
        var toFields = getFields( to.GetType() );

        foreach ( ObjectMap.Entry< string, FieldMetadata > entry in getFields( from.GetType() ))
        {
            FieldMetadata toField   = toFields.get( entry.key );
            Field         fromField = entry.value.field;

            if ( toField == null ) throw new SerializationException( "To object is missing field: " + entry.key );

            try
            {
                toField.field.set( to, fromField.get( from ) );
            }
            catch ( ReflectionException ex )
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

    // ========================================================================

    [PublicAPI]
    public abstract class ReadOnlySerializer< T > : ISerializer< T >
    {
        public virtual void Write( Json json, T obj, Type knownType )
        {
        }

        public abstract T Read( Json json, JsonValue jsonData, Type type );
    }

    // ========================================================================

    [PublicAPI]
    public interface ISerializer
    {
        void Write( Json json, object obj, Type knownType );
        object Read( Json json, JsonValue jsonData, Type type );
    }

    [PublicAPI]
    public interface ISerializer< T >
    {
        void Write( Json json, T obj, Type knownType );
        T Read( Json json, JsonValue jsonData, Type type );
    }

    // ========================================================================

    [PublicAPI]
    public interface ISerializable
    {
        void Write( Json json );
        void Read( Json json, JsonValue jsonData );
    }
}

internal class OrderedMap : OrderedDictionary;