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
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;

using JetBrains.Annotations;

using LughSharp.Lugh.Utils;

using LughUtils.source;
using LughUtils.source.Collections;
using LughUtils.source.Exceptions;
using LughUtils.source.Logging;

namespace Extensions.Source.Json;

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
    public string? TypeName { get; set; } = "class";

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

    public JsonTextWriter? JsonWriter      { get; set; }
    public JsonOutputType? OutputType      { get; set; }
    public bool            QuoteLongValues { get; set; } = false;

    // ========================================================================

    private Dictionary< Type, OrderedMap< string, FieldMetadata >? > _typeToFields         = [ ];
    private Dictionary< string, Type? >                              _tagToClass           = [ ];
    private Dictionary< Type, string >                               _classToTag           = [ ];
    private Dictionary< Type, IJsonSerializer >                      _classToSerializer    = [ ];
    private Dictionary< Type, object[]? >                            _classToDefaultValues = [ ];

    private object[] _equals1 = [ ];
    private object[] _equals2 = [ ];

//TODO:    private TextWriter? _textWriter;

    // ========================================================================

    public Json() : this( JsonOutputType.Minimal )
    {
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
        Logger.Debug( $"{tag}::{type}" );

        _tagToClass[ tag ]  = type;
        _classToTag[ type ] = tag;
    }

    /// <summary>
    /// Returns the class for the specified tag, or null.
    /// </summary>
    public Type? GetType( string tag )
    {
        return _tagToClass[ tag ] ?? null;
    }

    /// <summary>
    /// Returns the tag for the specified class, or null.
    /// </summary>
    public string? GetTag( Type type )
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
    public void SetElementType( Type? type, string fieldName, Type elementType )
    {
        var metadata = GetFields( type )?.Get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type?.Name})" );
        }

        metadata.ElementType = elementType;
    }

    /// <summary>
    /// The specified field will be treated as if it has or does not have the
    /// <see cref="System.ObsoleteAttribute"/> annotation.
    /// </summary>
    public void SetDeprecated( Type? type, string fieldName, bool deprecated )
    {
        var metadata = GetFields( type )?.Get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type?.Name})" );
        }

        metadata.Deprecated = deprecated;
    }

    /// <summary>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private OrderedMap< string, FieldMetadata >? GetFields( Type? type )
    {
        Guard.ThrowIfNull( type );

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

            if ( field.GetCustomAttribute< NonSerializedAttribute >() != null )
            {
                continue;
            }

            if ( field.IsStatic )
            {
                continue;
            }

            if ( field.IsPrivate && field.Name.Contains( '<' ) )
            {
                continue;
            }

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
        if ( writer is not JsonTextWriter )
        {
            writer = new JsonTextWriter( writer );
        }

        JsonWriter = ( JsonTextWriter )writer;
        JsonWriter.SetOutputType( OutputType );
        JsonWriter.SetQuoteLongValues( QuoteLongValues );
    }

    /// <summary>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    private object?[]? GetDefaultValues( Type type )
    {
        if ( !UsePrototypes )
        {
            return null;
        }

        if ( _classToDefaultValues.TryGetValue( type, out var value ) )
        {
            return value;
        }

        object? instance;

        try
        {
            instance = NewInstance( type );
        }
        catch ( Exception ignored )
        {
            Logger.Error( $"Exception (IGNORED): {ignored.Message}" );

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

            if ( IgnoreDeprecated && ( bool )metadata?.Deprecated! )
            {
                continue;
            }

            var field = metadata?.FieldInfo;

            Guard.ThrowIfNull( field );

            try
            {
                values[ defaultIndex++ ] = field.GetValue( instance )!;
            }
            catch ( FieldAccessException ex )
            {
                throw new SerializationException( $"Error accessing field: {field.Name} ({type.Name})", ex );
            }
            catch ( SerializationException ex )
            {
                var newEx = new SerializationException( ex.Message );
                newEx.AddTrace( $"{field} ({type.Name})" );

                throw newEx;
            }
            catch ( GdxRuntimeException runtimeEx )
            {
                var ex = new SerializationException( runtimeEx.ToString() );
                ex.AddTrace( $"{field} ({type.Name})" );

                throw ex;
            }
        }

        return values;
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="jsonData"></param>
    public void ReadField( object obj, string name, JsonValue jsonData )
    {
        ReadField( obj, name, name, null, jsonData );
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="elementType"></param>
    /// <param name="jsonData"></param>
    public void ReadField( object obj, string name, Type elementType, JsonValue jsonData )
    {
        ReadField( obj, name, name, elementType, jsonData );
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fieldName"></param>
    /// <param name="jsonName"></param>
    /// <param name="jsonData"></param>
    public void ReadField( object obj, string fieldName, string jsonName, JsonValue jsonData )
    {
        ReadField( obj, fieldName, jsonName, null, jsonData );
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fieldName"></param>
    /// <param name="jsonName"></param>
    /// <param name="elementType"></param>
    /// <param name="jsonMap"></param>
    /// <exception cref="SerializationException"></exception>
    public void ReadField( object obj, string fieldName, string jsonName, Type? elementType, JsonValue jsonMap )
    {
        var type     = obj.GetType();
        var metadata = GetFields( type )?.Get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type.Name})" );
        }

        var field = metadata.FieldInfo;

        if ( elementType == null )
        {
            elementType = metadata.ElementType;
        }

        ReadField( obj, field, jsonName, elementType, jsonMap );
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="field"></param>
    /// <param name="jsonName"></param>
    /// <param name="elementType"></param>
    /// <param name="jsonMap"></param>
    /// <exception cref="SerializationException"></exception>
    public void ReadField( object obj, FieldInfo field, string jsonName, Type? elementType, JsonValue jsonMap )
    {
        var jsonValue = jsonMap.Get( jsonName );

        if ( jsonValue == null )
        {
            return;
        }

        try
        {
            field.SetValue( obj, ReadValue< object >( field.GetType(), elementType, jsonValue ) );
        }
        catch ( FieldAccessException ex )
        {
            throw new SerializationException( $"Error accessing field: {field.Name} ({field.DeclaringType?.Name})", ex );
        }
        catch ( SerializationException ex )
        {
            var newEx = new SerializationException( ex );
            newEx.AddTrace( $"{field.Name} ({field.DeclaringType?.Name})" );

            throw newEx;
        }
        catch ( GdxRuntimeException runtimeEx )
        {
            var ex = new SerializationException( runtimeEx );
            ex.AddTrace( jsonValue.Trace() );
            ex.AddTrace( $"{field.Name} ({field.DeclaringType?.Name})" );

            throw ex;
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="jsonMap"></param>
    /// <exception cref="SerializationException"></exception>
    public void ReadFields( object obj, JsonValue jsonMap )
    {
        var type   = obj.GetType();
        var fields = GetFields( type );

        Guard.ThrowIfNull( fields );

        for ( var child = jsonMap.Child; child != null; child = child.Next )
        {
            Guard.ValidateString( child.Name );

            var metadata = fields.Get( child.Name!.Replace( " ", "_" ) );

            if ( metadata == null )
            {
                if ( child.Name.Equals( TypeName ) )
                {
                    continue;
                }

                if ( IgnoreUnknownFields || IgnoreUnknownField( type, child.Name ) )
                {
                    Logger.Debug( $"Ignoring unknown field: {child.Name} ({type.Name})" );

                    continue;
                }

                var ex = new SerializationException( $"Field not found: {child.Name} ({type.Name})" );
                ex.AddTrace( child.Trace() );

                throw ex;
            }

            if ( IgnoreDeprecated && !ReadDeprecated && metadata.Deprecated )
            {
                continue;
            }

            var field = metadata.FieldInfo;

            try
            {
                field.SetValue( obj, ReadValue< object >( field.GetType(), metadata.ElementType, child ) );
            }
            catch ( FieldAccessException ex )
            {
                throw new SerializationException( $"Error accessing field: {field.Name} ({type.Name})", ex );
            }
            catch ( SerializationException ex )
            {
                var newEx = new SerializationException( ex.Message );
                newEx.AddTrace( $"{field.Name} ({type.Name})" );

                throw newEx;
            }
            catch ( GdxRuntimeException runtimeEx )
            {
                var ex = new SerializationException( runtimeEx );
                ex.AddTrace( child.Trace() );
                ex.AddTrace( $"{field.Name} ({type.Name})" );

                throw ex;
            }
        }
    }

    /// <summary>
    /// Called for each unknown field name encountered by <see cref="ReadFields(object,JsonValue)"/>
    /// when <see cref="IgnoreUnknownFields"/> is false to determine whether the unknown field name
    /// should be ignored.
    /// </summary>
    /// <param name="type"> The object type being read. </param>
    /// <param name="fieldName">
    /// A field name encountered in the JSON for which there is no matching class field.
    /// </param>
    /// <returns>
    /// true if the field name should be ignored and an exception won't be thrown by
    /// {@link #readFields(object, JsonValue)}.
    /// </returns>
    protected static bool IgnoreUnknownField( Type? type, string fieldName )
    {
        return false;
    }

    public T? ReadValue< T >( string name, Type? type, JsonValue? jsonMap )
    {
        return ReadValue< T >( type, null, jsonMap?.Get( name ) );
    }

    public T? ReadValue< T >( string name, Type? type, T defaultValue, JsonValue jsonMap )
    {
        var jsonValue = jsonMap.Get( name );

        return jsonValue == null ? defaultValue : ReadValue< T >( type, null, jsonValue );
    }

    public T? ReadValue< T >( string name, Type? type, Type? elementType, JsonValue jsonMap )
    {
        return ReadValue< T >( type, elementType, jsonMap.Get( name ) );
    }

    public T? ReadValue< T >( string name, Type? type, Type? elementType, T defaultValue, JsonValue jsonMap )
    {
        var jsonValue = jsonMap.Get( name );

        return ReadValue< T >( type, elementType, defaultValue, jsonValue );
    }

    public T? ReadValue< T >( Type? type, Type? elementType, T defaultValue, JsonValue? jsonData )
    {
        return jsonData == null ? defaultValue : ReadValue< T >( type, elementType, jsonData );
    }

    public T? ReadValue< T >( Type? type, JsonValue jsonData )
    {
        return ReadValue< T >( type, null, jsonData );
    }

    /// <summary>
    /// </summary>
    /// <param name="type"></param>
    /// <param name="elementType"></param>
    /// <param name="jsonData"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    public T? ReadValue< T >( Type? type, Type? elementType, JsonValue? jsonData )
    {
        if ( jsonData == null )
        {
            return default( T );
        }

        if ( jsonData.IsObject() )
        {
            var className = TypeName == null ? null : jsonData.GetString( TypeName, null );

            if ( className != null )
            {
                type = GetType( className );

                if ( type == null )
                {
                    try
                    {
                        type = Type.GetType( className );
                    }
                    catch ( Exception ex )
                    {
                        throw new SerializationException( ex.Message, ex );
                    }
                }
            }

            if ( type == null )
            {
                if ( DefaultSerializer != null )
                {
                    return ( T )DefaultSerializer.Read( this, jsonData, type );
                }

                return ( T )( object )jsonData;
            }

            if ( ( TypeName != null ) && typeof( ICollection ).IsAssignableFrom( type ) )
            {
                // JSON object wrapper to specify type.
                jsonData = jsonData.Get( "items" );

                if ( jsonData == null )
                {
                    throw new SerializationException( $"Unable to convert object to collection: " +
                                                      $"{jsonData} ({type.FullName})" );
                }
            }
            else
            {
                if ( _classToSerializer.TryGetValue( type, out var serializer ) )
                {
                    return ( T )serializer.Read( this, jsonData, type );
                }

                if ( serializer != null )
                {
                    return ( T )serializer.Read( this, jsonData, type );
                }

                if ( ( type == typeof( string ) )
                     || ( type == typeof( int ) )
                     || ( type == typeof( bool ) )
                     || ( type == typeof( float ) )
                     || ( type == typeof( long ) )
                     || ( type == typeof( double ) )
                     || ( type == typeof( short ) )
                     || ( type == typeof( byte ) )
                     || ( type == typeof( char ) )
                     || type.IsEnum )
                {
                    return ReadValue< T >( "value", type, jsonData.Get( "value" ) );
                }

                var obj = Activator.CreateInstance( type );

                Guard.ThrowIfNull( obj );

                if ( obj is IJsonSerializable serializable )
                {
                    serializable.Read( this, jsonData );

                    return ( T )obj;
                }

                // JSON object special cases.
                if ( obj is ObjectMap< string, object > objectMap )
                {
                    for ( var child = jsonData.Child; child != null; child = child.Next )
                    {
                        Guard.ThrowIfNull( child.Name );

                        objectMap.Put( child.Name, ReadValue< object >( elementType, null, child ) );
                    }

                    return ( T )( object )objectMap;
                }

//                if ( obj is ObjectIntMap )
//                {
//                    var result = ( ObjectIntMap )obj;
//
//                    for ( var child = jsonData.Child; child != null; child = child.Next )
//                    {
//                        result.put( child.Name, ReadValue< T >( typeof( int ), null, child));
//                    }
//
//                    return ( T )result;
//                }

//                if ( obj is ObjectFloatMap )
//                {
//                    var result = ( ObjectFloatMap )obj;
//                    
//                    for ( var child = jsonData.Child; child != null; child = child.Next )
//                    {
//                        result.put( child.Name, ReadValue< T >( typeof( float ), null, child) );
//                    }
//
//                    return ( T )result;
//                }

//                if ( obj is ObjectSet )
//                {
//                    var result = ( ObjectSet )obj;
//
//                    for ( var child = jsonData.GetChild( "values" ); child != null; child = child.Next )
//                    {
//                        result.add( ReadValue< T >( elementType, null, child ) );
//                    }
//
//                    return ( T )result;
//                }

//                if ( obj is IntMap )
//                {
//                    var result = ( IntMap )object;
//
//                    for ( var child = jsonData.Child; child != null; child = child.Next )
//                    {
//                        result.put( int.Parse( child.Name ), ReadValue< T >( elementType, null, child ) );
//                    }
//
//                    return ( T )result;
//                }

//                if ( obj is LongMap )
//                {
//                    var result = ( LongMap )obj;
//
//                    for ( var child = jsonData.Child; child != null; child = child.Next )
//                    {
//                        result.put( long.Parse( child.Name ), ReadValue< T >( elementType, null, child ) );
//                    }
//
//                    return ( T )result;
//                }

//                if ( obj is IntSet )
//                {
//                    var result = ( IntSet )obj;
//
//                    for ( var child = jsonData.GetChild( "values" ); child != null; child = child.Next )
//                    {
//                        result.add( child.AsInt() );
//                    }
//
//                    return ( T )result;
//                }

//                if ( obj is ArrayMap )
//                {
//                    var result = ( ArrayMap )obj;
//
//                    for ( var child = jsonData.Child; child != null; child = child.Next )
//                    {
//                        result.put( child.Name, ReadValue< T >( elementType, null, child ) );
//                    }
//
//                    return ( T )result;
//                }

//                if ( obj is Map map )
//                {
//                    for ( var child = jsonData.Child; child != null; child = child.Next )
//                    {
//                        if ( child.Name.Equals( TypeName ) )
//                        {
//                            continue;
//                        }
//
//                        map[ child.Name ] = ReadValue< T >( elementType, null, child );
//                    }
//
//                    return ( T )( object )map;
//                }

                ReadFields( obj, jsonData );

                return ( T )obj;
            }
        }

        if ( type != null )
        {
            if ( _classToSerializer.TryGetValue( type, out var serializer ) )
            {
                return ( T )serializer.Read( this, jsonData, type );
            }

            if ( typeof( ISerializable ).IsAssignableFrom( type ) )
            {
                var obj = Activator.CreateInstance( type );

                Guard.ThrowIfNull( obj );

                ( ( IJsonSerializable )obj ).Read( this, jsonData );

                return ( T )obj;
            }
        }

        if ( jsonData.IsArray() )
        {
            // JSON array special cases.
            if ( ( type == null ) || ( type == typeof( object ) ) )
            {
                type = typeof( List< object > );
            }

            if ( typeof( IList ).IsAssignableFrom( type ) )
            {
                var result = type == typeof( List< object > )
                    ? new List< object >()
                    : ( IList? )Activator.CreateInstance( type );

                foreach ( var element in jsonData )
                {
                    result?.Add( ReadValue< object >( elementType, null, element ) );
                }

                return ( T? )result;
            }

            if ( type.IsGenericType
                 && typeof( ICollection<> ).IsAssignableFrom( type.GetGenericTypeDefinition() ) )
            {
                var genericArgument = type.GetGenericArguments()[ 0 ];
                var listType        = typeof( List<> ).MakeGenericType( genericArgument );
                var result          = ( IList? )Activator.CreateInstance( listType );

                foreach ( var element in jsonData )
                {
                    result?.Add( ReadValue< object >( genericArgument, null, element ) );
                }

                return ( T? )result;
            }

            if ( typeof( Queue ).IsAssignableFrom( type ) )
            {
                var result = type == typeof( Queue )
                    ? new Queue()
                    : ( Queue? )Activator.CreateInstance( type );

                foreach ( var element in jsonData )
                {
                    result?.Enqueue( ReadValue< object >( elementType, null, element ) );
                }

                return ( T? )( object? )result;
            }

//            if ( type.IsArray )
//            {
//                var componentType = type.GetElementType();
//
//                if ( elementType == null )
//                {
//                    elementType = componentType;
//                }
//
//                var list = new List< object >();
//
//                foreach ( var element in jsonData )
//                {
//                    list.Add( ReadValue< object >( elementType, null, element ) );
//                }
//
//                var array = Array.CreateInstance( componentType, list.Count );
//                Array.Copy( list.ToArray(), array, list.Count );
//
//                return (T)array;
//            }

            throw new SerializationException( $"Unable to convert value to required type: {jsonData} ({type.FullName})" );
        }

        if ( jsonData.IsNumber() )
        {
            try
            {
                if ( ( type == null ) || ( type == typeof( float ) ) )
                {
                    return ( T )( object )jsonData.AsFloat();
                }

                if ( type == typeof( int ) )
                {
                    return ( T )( object )jsonData.AsInt();
                }

                if ( type == typeof( long ) )
                {
                    return ( T )( object )jsonData.AsDouble();
                }

                if ( type == typeof( double ) )
                {
                    return ( T )( object )jsonData.AsDouble();
                }

                if ( type == typeof( string ) )
                {
                    return ( T )( object )jsonData.AsString()!;
                }

                if ( type == typeof( short ) )
                {
                    return ( T )( object )jsonData.AsShort();
                }

                if ( type == typeof( byte ) )
                {
                    return ( T )( object )jsonData.AsByte();
                }
            }
            catch ( ArithmeticException ignored )
            {
                Logger.Error( $"ArithmeticException (IGNORED): {ignored.Message}" );
            }

            jsonData = new JsonValue( jsonData.AsString() );
        }

        if ( jsonData.IsBoolean() )
        {
            try
            {
                if ( ( type == null ) || ( type == typeof( bool ) ) )
                {
                    return ( T )( object )jsonData.AsBoolean();
                }
            }
            catch ( ArithmeticException ignored )
            {
                Logger.Error( $"ArithmeticException (IGNORED): {ignored.Message}" );
            }

            jsonData = new JsonValue( jsonData.AsString() );
        }

        if ( jsonData.IsString() )
        {
            var str = jsonData.AsString();

            Guard.ThrowIfNull( str );

            if ( ( type == null ) || ( type == typeof( string ) ) )
            {
                return ( T )( object )str;
            }

            try
            {
                if ( type == typeof( int ) )
                {
                    return ( T )( object )int.Parse( str );
                }

                if ( type == typeof( float ) )
                {
                    return ( T )( object )float.Parse( str );
                }

                if ( type == typeof( long ) )
                {
                    return ( T )( object )long.Parse( str );
                }

                if ( type == typeof( double ) )
                {
                    return ( T )( object )double.Parse( str );
                }

                if ( type == typeof( short ) )
                {
                    return ( T )( object )short.Parse( str );
                }

                if ( type == typeof( byte ) )
                {
                    return ( T )( object )byte.Parse( str );
                }
            }
            catch ( FormatException ignored )
            {
                // Handle invalid number format
                Logger.Error( $"FormatException (IGNORED): {ignored.Message}" );
            }

            if ( type == typeof( bool ) )
            {
                return ( T )( object )bool.Parse( str );
            }

            if ( type == typeof( char ) )
            {
                return ( T )( object )str[ 0 ];
            }

            if ( type.IsEnum )
            {
                foreach ( var enumValue in Enum.GetValues( type ) )
                {
                    if ( str.Equals( ConvertToString( enumValue ) ) )
                    {
                        return ( T )enumValue;
                    }
                }
            }

            if ( type == typeof( System.Text.StringBuilder ) )
            {
                return ( T )( object )new System.Text.StringBuilder( str );
            }

            throw new SerializationException( $"Unable to convert value to required type: " +
                                              $"{jsonData} ({type.FullName})" );
        }

        return default( T );
    }

    /// <summary>
    /// Each field on the <c>to</c> object is set to the value for the field with the same name
    /// on the <c>from</c> object. The <c>to</c> object must have at least all the fields of the
    /// <c>from</c> object with the same name and type.
    /// </summary>
    public void CopyFields( object from, object to )
    {
        var toFields   = GetFields( to.GetType() );
        var fromFields = GetFields( from.GetType() );

        Guard.ThrowIfNull( fromFields );

        foreach ( var entry in fromFields )
        {
            var toField   = toFields?.Get( entry.Key );
            var fromField = entry.Value.FieldInfo;

            if ( toField == null )
            {
                throw new SerializationException( $"To object is missing field: {entry.Key}" );
            }

            try
            {
                toField.FieldInfo.SetValue( to, fromField.GetValue( from ) );
            }
            catch ( FieldAccessException ex )
            {
                throw new SerializationException( $"Error copying field: {fromField.Name}", ex );
            }
        }
    }

    // ========================================================================

    private string? ConvertToString( Enum e )
    {
        return EnumNames ? Enum.GetName( e.GetType(), e ) : e.ToString();
    }

    private string? ConvertToString( object? obj )
    {
        return obj switch
        {
            Enum enumType => ConvertToString( enumType ),
            Type typeType => typeType.FullName,
            var _         => StringUtils.ValueOf( obj ) ?? "",
        };
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    protected static object? NewInstance( Type type )
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
                var constructor = type.GetConstructor( BindingFlags.Instance | BindingFlags.NonPublic,
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
    // PrettyPrint Methods

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

    public string PrettyPrint( object obj, JsonValue.PrettyPrintSettings settings )
    {
        return PrettyPrint( ToJson( obj ), settings );
    }

    public string PrettyPrint( string json, int singleLineColumns )
    {
        var jval = new JsonReader().Parse( json );

        return jval != null ? jval.PrettyPrint( OutputType, singleLineColumns ) : "**ERROR**";
    }

    public static string PrettyPrint( string json, JsonValue.PrettyPrintSettings settings )
    {
        Logger.Checkpoint();

        var jval = new JsonReader().Parse( json );

        Logger.Checkpoint();

        Logger.Debug( $"jval: {jval}" );

        return jval != null ? jval.PrettyPrint( settings ) : "**ERROR**";
    }

    // ========================================================================
    // ========================================================================

    private class FieldMetadata
    {
        public FieldInfo FieldInfo   { get; }
        public Type?     ElementType { get; set; }
        public bool      Deprecated  { get; set; }

        public FieldMetadata( FieldInfo field )
        {
            FieldInfo = field;

            var index = typeof( IDictionary< , > ).IsAssignableFrom( field.FieldType.GetGenericTypeDefinition() )
                        || typeof( IDictionary ).IsAssignableFrom( field.FieldType )
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