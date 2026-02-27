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
using System.Collections.Generic;
using System.Reflection;

using JetBrains.Annotations;

using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.Utils.Json;

/// <summary>
/// Reads / Writes objects to / from JSON, automatically.
/// </summary>
[PublicAPI]
public class Json
{
    public string?                TypeName            { get; set; } = "class";
    public bool                   UsePrototypes       { get; set; } = true;
    public bool                   IgnoreUnknownFields { get; set; }
    public bool                   IgnoreDeprecated    { get; set; }
    public JsonOutputType         OutputType          { get; set; }
    public bool                   QuoteLongValues     { get; set; }
    public bool                   EnumNames           { get; set; } = true;
    public bool                   ReadDeprecated      { get; set; }
    public bool                   SortFields          { get; set; }
    public ISerializer< object >? DefaultSerializer   { get; set; }

    // ========================================================================

    private const bool DEBUG = false;

    private readonly Dictionary< Type, Dictionary< string, FieldMetadata > > _typeToFields      = [ ];
    private readonly Dictionary< Type, ISerializer< object > >               _classToSerializer = [ ];

    private readonly Dictionary< string, Type >   _tagToType            = [ ];
    private readonly Dictionary< Type, string >   _classToTag           = [ ];
    private readonly Dictionary< Type, object[] > _classToDefaultValues = [ ];

    private readonly object?[] _equals1 = new[] { ( object? )null };
    private readonly object?[] _equals2 = new[] { ( object? )null };

    private JsonWriter? _writer;
    private JsonReader  _reader = new();

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new Json instance, and sets the default output type to
    /// <see cref="JsonOutputType.Minimal"/>.
    /// </summary>
    public Json()
    {
        OutputType = JsonOutputType.Minimal;
    }

    /// <summary>
    /// Creates a new Json instance, and sets the default output type to the
    /// specified <see cref="JsonOutputType"/> value.
    /// </summary>
    public Json( JsonOutputType outputType )
    {
        OutputType = outputType;
    }

    public virtual bool IgnoreUnknownField( Type type, string fieldName )
    {
        throw new NotImplementedException();
    }

    public void SetSerializer< T >( ISerializer< T > serializer )
    {
        throw new NotImplementedException();
    }

    public void CopyFields( object from, object to )
    {
    }

    /// <summary>
    /// Sets a tag to use instead of the fully qualified class name. This can make
    /// the Json easier to read.
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="type"></param>
    public void AddClassTag( string tag, Type type )
    {
        _tagToType.Add( tag, type );
        _classToTag.Add( type, tag );
    }

    /// <summary>
    /// Returns the class for the specified tag, or null if no class is found.
    /// </summary>
    public Type? GetClass( string tag )
    {
        return _tagToType.GetValueOrDefault( tag );
    }

    /// <summary>
    /// Returns the tag for the specified class, or null if no tag is found.
    /// </summary>
    public string? GetTag( Type type )
    {
        return _classToTag.GetValueOrDefault( type );
    }

    public Type? GetTagType( string? valueMapName )
    {
        throw new NotImplementedException();
    }

    public ISerializer< object >? GetSerializer( Type type )
    {
        return _classToSerializer.GetValueOrDefault( type );
    }

    public void SetSerializer< T >( Type type, ISerializer< object > serializer )
    {
        _classToSerializer[ type ] = serializer;
    }

    /// <summary>
    /// Sets the type of elements in a collection. When the element type is known, the class for
    /// each element in the collection does not need to be written unless different from the
    /// element type.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="fieldName"></param>
    /// <param name="elementType"></param>
    /// <exception cref="SerializationException"></exception>
    public void SetElementType( Type type, string fieldName, Type elementType )
    {
        FieldMetadata metadata = GetFields( type ).Get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type.Name})" );
        }

        metadata.ElementType = elementType;
    }

    /// <summary>
    /// The specified field will be treated as if it has or does not have the <c>Deprecated</c> annotation.
    /// </summary>
    public void SetDeprecated( Type type, string fieldName, bool deprecated )
    {
        FieldMetadata metadata = GetFields( type ).Get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type.Name})" );
        }

        metadata.Deprecated = deprecated;
    }

    // ========================================================================

    public virtual T? ReadValue< T >( string name, Type? type, JsonValue jsonMap )
    {
//        return readValue( type, null, jsonMap.get( name ) );
        throw new NotImplementedException();
    }

    public virtual T? ReadValue< T >( string name, Type? type, T defaultValue, JsonValue jsonMap )
    {
        JsonValue? jsonValue = jsonMap.Get( name );

        if ( jsonValue == null )
        {
            return defaultValue;
        }

        return ReadValue< T >( type, null, jsonValue );
    }

    public virtual T? ReadValue< T >( string name, Type? type, Type? elementType, JsonValue jsonMap )
    {
//        return readValue( type, elementType, jsonMap.get( name ) );
        throw new NotImplementedException();
    }

    public virtual T? ReadValue< T >( string name, Type? type, Type? elementType, T defaultValue, JsonValue jsonMap )
    {
//        JsonValue jsonValue = jsonMap.get( name );
//
//        return readValue( type, elementType, defaultValue, jsonValue );
        throw new NotImplementedException();
    }

    public virtual T? ReadValue< T >( Type? type, Type? elementType, T defaultValue, JsonValue jsonData )
    {
//        if ( jsonData == null ) return defaultValue;
//
//        return readValue( type, elementType, jsonData );
        throw new NotImplementedException();
    }

    public virtual T? ReadValue< T >( Type? type, JsonValue jsonData )
    {
//        return readValue( type, null, jsonData );
        throw new NotImplementedException();
    }

    public virtual object? ReadValue( Type? type, JsonValue jsonData )
    {
//        return readValue( type, null, jsonData );
        throw new NotImplementedException();
    }

    public virtual T? ReadValue< T >( Type? type, Type? elementType, JsonValue? jsonData )
    {
//        if ( jsonData == null ) return null;
//
//        if ( jsonData.isObject() )
//        {
//            string className = typeName == null ? null : jsonData.getString( typeName, null );
//
//            if ( className != null )
//            {
//                type = getClass( className );
//
//                if ( type == null )
//                {
//                    try
//                    {
//                        type = ClassReflection.forName( className );
//                    }
//                    catch ( ReflectionException ex )
//                    {
//                        throw new SerializationException( ex );
//                    }
//                }
//            }
//
//            if ( type == null )
//            {
//                if ( defaultSerializer != null ) return ( T )defaultSerializer.read( this, jsonData, type );
//
//                return ( T )jsonData;
//            }
//
//            if ( typeName != null && ClassReflection.isAssignableFrom( Collection.class, type)) {
//                // JSON object wrapper to specify type.
//                jsonData = jsonData.get( "items" );
//
//                if ( jsonData == null )
//                    throw new SerializationException(
//                                                     "Unable to convert object to collection: " + jsonData + " ("
//                                                   + type.getName() + ")" );
//            } else {
//                Serializer serializer = classToSerializer.get( type );
//
//                if ( serializer != null ) return ( T )serializer.read( this, jsonData, type );
//
//                if ( type == string.class || type == Integer.class || type == Boolean.class || type == Float.class
//                || type == Long.class || type == Double.class || type == Short.class || type == Byte.class
//                || type == Character.class || ClassReflection.isAssignableFrom( Enum.class, type)) {
//                    return readValue( "value", type, jsonData );
//                }
//
//                Object object = newInstance( type );
//
//                if ( object instanceof Serializable) {
//                    ( ( Serializable )object ).read( this, jsonData );
//
//                    return ( T )object;
//                }
//
//                // JSON object special cases.
//                if ( object instanceof ObjectMap) {
//                    ObjectMap result = ( ObjectMap )object;
//                    for ( JsonValue child = jsonData.child; child != null; child = child.next )
//                        result.put( child.name, readValue( elementType, null, child ) );
//
//                    return ( T )result;
//                }
//                if ( object instanceof ObjectIntMap) {
//                    ObjectIntMap result = ( ObjectIntMap )object;
//                    for ( JsonValue child = jsonData.child; child != null; child = child.next )
//                        result.put( child.name, readValue( Integer.class, null, child));
//
//                    return ( T )result;
//                }
//                if ( object instanceof ObjectFloatMap) {
//                    ObjectFloatMap result = ( ObjectFloatMap )object;
//                    for ( JsonValue child = jsonData.child; child != null; child = child.next )
//                        result.put( child.name, readValue( Float.class, null, child));
//
//                    return ( T )result;
//                }
//                if ( object instanceof ObjectSet) {
//                    ObjectSet result = ( ObjectSet )object;
//                    for ( JsonValue child = jsonData.getChild( "values" ); child != null; child = child.next )
//                        result.add( readValue( elementType, null, child ) );
//
//                    return ( T )result;
//                }
//                if ( object instanceof IntMap) {
//                    IntMap result = ( IntMap )object;
//                    for ( JsonValue child = jsonData.child; child != null; child = child.next )
//                        result.put( Integer.parseInt( child.name ), readValue( elementType, null, child ) );
//
//                    return ( T )result;
//                }
//                if ( object instanceof LongMap) {
//                    LongMap result = ( LongMap )object;
//                    for ( JsonValue child = jsonData.child; child != null; child = child.next )
//                        result.put( Long.parseLong( child.name ), readValue( elementType, null, child ) );
//
//                    return ( T )result;
//                }
//                if ( object instanceof IntSet) {
//                    IntSet result = ( IntSet )object;
//                    for ( JsonValue child = jsonData.getChild( "values" ); child != null; child = child.next )
//                        result.add( child.asInt() );
//
//                    return ( T )result;
//                }
//                if ( object instanceof ArrayMap) {
//                    ArrayMap result = ( ArrayMap )object;
//                    for ( JsonValue child = jsonData.child; child != null; child = child.next )
//                        result.put( child.name, readValue( elementType, null, child ) );
//
//                    return ( T )result;
//                }
//                if ( object instanceof Map) {
//                    Map result = ( Map )object;
//
//                    for ( JsonValue child = jsonData.child; child != null; child = child.next )
//                    {
//                        if ( child.name.equals( typeName ) ) continue;
//
//                        result.put( child.name, readValue( elementType, null, child ) );
//                    }
//
//                    return ( T )result;
//                }
//
//                readFields( object, jsonData );
//
//                return ( T )object;
//            }
//        }
//
//        if ( type != null )
//        {
//            Serializer serializer = classToSerializer.get( type );
//
//            if ( serializer != null ) return ( T )serializer.read( this, jsonData, type );
//
//            if ( ClassReflection.isAssignableFrom( Serializable.class, type)) {
//                // A Serializable may be read as an array, string, etc, even though it will be written as an object.
//                Object object = newInstance( type );
//                ( ( Serializable )object ).read( this, jsonData );
//
//                return ( T )object;
//            }
//        }
//
//        if ( jsonData.isArray() )
//        {
//            // JSON array special cases.
//            if ( type == null || type == Object.class) type = ( Class< T > )Array.class;
//            if ( ClassReflection.isAssignableFrom( Array.class, type)) {
//                Array result = type == Array.class ? new Array() : ( Array )newInstance( type );
//                for ( JsonValue child = jsonData.child; child != null; child = child.next )
//                    result.add( readValue( elementType, null, child ) );
//
//                return ( T )result;
//            }
//            if ( ClassReflection.isAssignableFrom( Queue.class, type)) {
//                Queue result = type == Queue.class ? new Queue() : ( Queue )newInstance( type );
//                for ( JsonValue child = jsonData.child; child != null; child = child.next )
//                    result.addLast( readValue( elementType, null, child ) );
//
//                return ( T )result;
//            }
//            if ( ClassReflection.isAssignableFrom( Collection.class, type)) {
//                Collection result = type.isInterface() ? new ArrayList() : ( Collection )newInstance( type );
//                for ( JsonValue child = jsonData.child; child != null; child = child.next )
//                    result.add( readValue( elementType, null, child ) );
//
//                return ( T )result;
//            }
//
//            if ( type.isArray() )
//            {
//                Class componentType                    = type.getComponentType();
//                if ( elementType == null ) elementType = componentType;
//                Object result                          = ArrayReflection.newInstance( componentType, jsonData.size );
//                int    i                               = 0;
//                for ( JsonValue child = jsonData.child; child != null; child = child.next )
//                    ArrayReflection.set( result, i++, readValue( elementType, null, child ) );
//
//                return ( T )result;
//            }
//
//            throw new SerializationException( "Unable to convert value to required type: " + jsonData + " ("
//                                            + type.getName() + ")" );
//        }
//
//        if ( jsonData.isNumber() )
//        {
//            try
//            {
//                if ( type == null || type == float.class || type == Float.class) return
//                    ( T )( Float )jsonData.asFloat();
//
//                if ( type == int.class || type == Integer.class) return ( T )( Integer )jsonData.asInt();
//
//                if ( type == long.class || type == Long.class) return ( T )( Long )jsonData.asLong();
//
//                if ( type == double.class || type == Double.class) return ( T )( Double )jsonData.asDouble();
//
//                if ( type == string.class) return ( T )jsonData.asString();
//
//                if ( type == short.class || type == Short.class) return ( T )( Short )jsonData.asShort();
//
//                if ( type == byte.class || type == Byte.class) return ( T )( Byte )jsonData.asByte();
//
//                if ( type == char.class || type == Character.class) return ( T )( Character )jsonData.asChar();
//            }
//            catch ( NumberFormatException ignored )
//            {
//            }
//
//            jsonData = new JsonValue( jsonData.asString() );
//        }
//
//        if ( jsonData.isBoolean() )
//        {
//            try
//            {
//                if ( type == null || type == boolean.class || type == Boolean.class) return ( T )( Boolean )jsonData
//                    .asBoolean();
//            }
//            catch ( NumberFormatException ignored )
//            {
//            }
//
//            jsonData = new JsonValue( jsonData.asString() );
//        }
//
//        if ( jsonData.isString() )
//        {
//            string string = jsonData.asString();
//            if ( type == null || type == string.class) return ( T )string;
//
//            try
//            {
//                if ( type == int.class || type == Integer.class) return ( T )Integer.valueOf( string );
//
//                if ( type == float.class || type == Float.class) return ( T )Float.valueOf( string );
//
//                if ( type == long.class || type == Long.class) return ( T )Long.valueOf( string );
//
//                if ( type == double.class || type == Double.class) return ( T )Double.valueOf( string );
//
//                if ( type == short.class || type == Short.class) return ( T )Short.valueOf( string );
//
//                if ( type == byte.class || type == Byte.class) return ( T )Byte.valueOf( string );
//            }
//            catch ( NumberFormatException ignored )
//            {
//            }
//
//            if ( type == boolean.class || type == Boolean.class) return ( T )Boolean.valueOf( string );
//
//            if ( type == char.class || type == Character.class) return ( T )( Character )string.charAt( 0 );
//
//            if ( ClassReflection.isAssignableFrom( Enum.class, type)) {
//                Enum[] constants = ( Enum[] )type.getEnumConstants();
//
//                for ( int i = 0, n = constants.length; i < n; i++ )
//                {
//                    Enum e = constants[ i ];
//
//                    if ( string.equals( convertToString( e ) ) ) return ( T )e;
//                }
//            }
//            if ( type == CharSequence.class) return ( T )string;
//
//            throw new SerializationException( "Unable to convert value to required type: " + jsonData + " ("
//                                            + type.getName() + ")" );
//        }
//
//        return null;
        throw new NotImplementedException();
    }

    public virtual void ReadFields( object obj, JsonValue jsonMap )
    {
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class FieldMetadata
    {
        public FieldInfo Field       { get; private set; }
        public Type?     ElementType { get; private set; }
        public bool      Deprecated  { get; private set; }

        public FieldMetadata( FieldInfo field )
        {
            Field = field;
            Type fieldType = field.FieldType;

            bool isMap = IsSubclassOfRawGeneric( typeof( ObjectMap< , > ), fieldType ) ||
                         typeof( IDictionary< , > ).IsAssignableFrom( fieldType );

            int index = isMap ? 1 : 0;

            if ( fieldType.IsGenericType )
            {
                Type[] genericArgs = fieldType.GetGenericArguments();
                ElementType = ( genericArgs.Length > index ) ? genericArgs[ index ] : null;
            }

            Deprecated = field.IsDefined( typeof( ObsoleteAttribute ), true );
        }

        // Helper to check for open generic types like ObjectMap<,>
        private static bool IsSubclassOfRawGeneric( Type generic, Type? toCheck )
        {
            while ( toCheck != null && toCheck != typeof( object ) )
            {
                Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;

                if ( generic == cur )
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public interface ISerializer< in T >
    {
        public void Write( Json json, T obj, Type knownType );

        public object Read( Json json, JsonValue jsonData, Type type );
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [PublicAPI]
    public abstract class ReadOnlySerializer< T > : ISerializer< T >
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="obj"></param>
        /// <param name="knownType"></param>
        public void Write( Json json, T obj, Type knownType )
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="jsonData"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract object Read( Json json, JsonValue jsonData, Type type );
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public interface ISerializable
    {
        public void Write( Json json );

        public void Read( Json json, JsonValue jsonData );
    }
}

// ============================================================================
// ============================================================================