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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

using JetBrains.Annotations;

using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

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

    private readonly Dictionary< Type, Func< object > >                  _constructorCache = new();
    private readonly Dictionary< Type, Dictionary< string, FieldInfo > > _fieldCache       = new();
    private readonly Dictionary< Type, List< CachedField > >             _typeCache        = new();
    private readonly Dictionary< Type, Dictionary< string, object > >    _enumCache        = new();
    private readonly Dictionary< string, Type >                          _typeAliases      = new();

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

    /// <summary>
    /// Gets the serializer for the specified class, or null if no serializer is found.
    /// </summary>
    public ISerializer< object >? GetSerializer( Type type )
    {
        return _classToSerializer.GetValueOrDefault( type );
    }

    /// <summary>
    /// Sets the default serializer for the specified class.
    /// </summary>
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
        FieldMetadata? metadata = GetFields( type ).Get( fieldName );

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
        FieldMetadata? metadata = GetFields( type ).Get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type.Name})" );
        }

        metadata.Deprecated = deprecated;
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="jsonMap"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual T? ReadValue< T >( string name, Type? type, JsonValue jsonMap )
    {
        return ReadValue< T >( type, null, jsonMap.Get( name ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="defaultValue"></param>
    /// <param name="jsonMap"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual T? ReadValue< T >( string name, Type? type, T defaultValue, JsonValue jsonMap )
    {
        JsonValue? jsonValue = jsonMap.Get( name );

        return jsonValue == null ? defaultValue : ReadValue< T >( type, null, jsonValue );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="elementType"></param>
    /// <param name="jsonMap"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual T? ReadValue< T >( string name, Type? type, Type? elementType, JsonValue jsonMap )
    {
        return ReadValue< T >( type, elementType, jsonMap.Get( name ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="elementType"></param>
    /// <param name="defaultValue"></param>
    /// <param name="jsonMap"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual T? ReadValue< T >( string name, Type? type, Type? elementType, T defaultValue, JsonValue jsonMap )
    {
        JsonValue? jsonValue = jsonMap.Get( name );

        return ReadValue< T >( type, elementType, defaultValue, jsonValue );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="elementType"></param>
    /// <param name="defaultValue"></param>
    /// <param name="jsonData"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual T? ReadValue< T >( Type? type, Type? elementType, T defaultValue, JsonValue? jsonData )
    {
        if ( jsonData == null )
        {
            return defaultValue;
        }

        return ReadValue< T >( type, elementType, jsonData );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="jsonData"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public virtual T? ReadValue< T >( Type? type, JsonValue jsonData )
    {
        return ReadValue< T >( type, null, jsonData );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="jsonData"></param>
    /// <returns></returns>
    public virtual object? ReadValue( Type? type, JsonValue jsonData )
    {
        return ReadValue< object >( type, null, jsonData );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="elementType"></param>
    /// <param name="jsonData"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    public virtual T? ReadValue< T >( Type? type, Type? elementType, JsonValue? jsonData )
    {
        if ( jsonData == null )
        {
            return default;
        }

        if ( jsonData.IsObject() )
        {
            // Resolve Type from JSON tag if necessary
            string? className = TypeName == null ? null : jsonData.GetString( TypeName, null );

            if ( className != null )
            {
                Type? concreteType = GetClass( className ) ?? ResolveType( className );

                if ( concreteType != null )
                {
                    // Use the concreteType for instantiation,  even if the 'type'
                    // parameter passed in was an interface!
                    type = concreteType;
                }
            }

            if ( type == null )
            {
                if ( DefaultSerializer != null )
                {
                    return ( T? )DefaultSerializer.Read( this, jsonData, type );
                }

                return ( T? )( object )jsonData; // Direct cast to T if possible
            }

            // Handle Collection Wrapper (JSON object with "items" key)
            if ( TypeName != null && typeof( IEnumerable ).IsAssignableFrom( type )
                                  && !typeof( IDictionary ).IsAssignableFrom( type ) )
            {
                jsonData = jsonData.Get( "items" );

                if ( jsonData == null )
                {
                    throw new SerializationException( $"Unable to convert object to collection: {type.Name}" );
                }
            }
            else
            {
                // Custom Serializers
                if ( _classToSerializer.TryGetValue( type, out ISerializer< object >? serializer ) )
                {
                    return ( T? )serializer.Read( this, jsonData, type );
                }

                // Primitive/Enum "Value" Wrapper check
                if ( type.IsPrimitive || type == typeof( string ) || type == typeof( decimal ) || type.IsEnum )
                {
                    return ReadValue< T >( type, null, jsonData.Get( "value" ) );
                }

                object obj = NewInstance( type );

                // Interface-based Serialization (ISerializable)
                if ( obj is ISerializable serializable )
                {
                    serializable.Read( this, jsonData );

                    return ( T? )obj;
                }

                // Map / Dictionary handling
                if ( obj is IDictionary dictionary )
                {
                    for ( JsonValue? child = jsonData.Child; child != null; child = child.Next )
                    {
                        if ( child.Name == TypeName )
                        {
                            continue;
                        }

                        // Dictionary keys are usually strings here, but also check for numeric maps
                        object? key = child.Name;

                        if ( child.Name is { Length: > 0 } )
                        {
                            if ( obj is IDictionary< int, object > )
                            {
                                key = int.Parse( child.Name );
                            }
                            else if ( obj is IDictionary< long, object > )
                            {
                                key = long.Parse( child.Name );
                            }

                            if ( key != null )
                            {
                                dictionary.Add( key, ReadValue< object >( elementType, null, child ) );
                            }
                        }
                    }

                    return ( T? )obj;
                }

                // Handle Sets (HashSet-like objects)
                if ( obj is ISet< object? > set )
                {
                    for ( JsonValue? child = jsonData.Get( "values" )?.Child; child != null; child = child.Next )
                    {
                        set.Add( ReadValue< object >( elementType, null, child ) );
                    }

                    return ( T? )obj;
                }

                // Fallback to Field Reflection
                ReadFields( obj, jsonData );

                return ( T? )obj;
            }
        }

        // --- Array Handling ---
        if ( jsonData.IsArray() )
        {
            if ( type == null || type == typeof( object ) )
            {
                type = typeof( List< object > );
            }

            // List/Collection Handling
            if ( typeof( IEnumerable ).IsAssignableFrom( type ) && !type.IsArray )
            {
                var list = ( IList )( type.IsInterface ? new List< object >() : NewInstance( type ) );

                for ( JsonValue? child = jsonData.Child; child != null; child = child.Next )
                {
                    list.Add( ReadValue< object >( elementType, null, child ) );
                }

                return ( T? )list;
            }

            // Native Array Handling (T[])
            if ( type.IsArray )
            {
                Type componentType = type.GetElementType()!;

                elementType ??= componentType;

                var resultArray = Array.CreateInstance( componentType, jsonData.Size );
                var i           = 0;

                for ( JsonValue? child = jsonData.Child; child != null; child = child.Next )
                {
                    resultArray.SetValue( ReadValue< object >( elementType, null, child ), i++ );
                }

                return ( T? )( object )resultArray;
            }

            throw new SerializationException( $"Unable to convert array to: {type.Name}" );
        }

        // --- Primitive Value Handling (Number, Bool, String) ---
        return ( T? )ConvertPrimitive( type, jsonData );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    public void AddAlias( string name, Type type )
    {
        _typeAliases[ name ] = type;
    }

    private Type? ResolveType( string name )
    {
        // Check short-name aliases first
        if ( _typeAliases.TryGetValue( name, out Type? type ) )
        {
            return type;
        }

        // Try to find the type normally
        return Type.GetType( name );
    }

    private object? ConvertPrimitive( Type? type, JsonValue jsonData )
    {
        if ( jsonData.IsNumber() )
        {
            if ( type == null || type == typeof( float ) )
            {
                return jsonData.AsFloat();
            }

            if ( type.IsEnum )
            {
                // C# can cast numbers directly to the enum underlying type
                return Enum.ToObject( type, jsonData.AsInt() );
            }

            if ( type == typeof( int ) )
            {
                return jsonData.AsInt();
            }

            if ( type == typeof( long ) )
            {
                return jsonData.AsLong();
            }

            return type == typeof( double )
                ? jsonData.AsDouble()
                : Convert.ChangeType( jsonData.AsShort(), type );
        }

        if ( jsonData.IsBoolean() )
        {
            return jsonData.AsBoolean();
        }

        if ( jsonData.IsString() )
        {
            string? s = jsonData.AsString();

            if ( s is { Length: > 0 } )
            {
                if ( type == null || type == typeof( string ) )
                {
                    return s;
                }

                return type.IsEnum
                    ? ParseEnum( type, jsonData.AsString()! )
                    : Convert.ChangeType( s, type );
            }
        }

        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    private object NewInstance( Type type )
    {
        if ( type.IsInterface || type.IsAbstract )
        {
            throw new SerializationException( $"Cannot create an instance of {type.Name}. " +
                                              $"Is it missing a '{TypeName}' tag in the JSON?" );
        }

        if ( _constructorCache.TryGetValue( type, out Func< object >? factory ) )
        {
            return factory();
        }

        ConstructorInfo? constructor = type.GetConstructor( BindingFlags.Public
                                                          | BindingFlags.NonPublic
                                                          | BindingFlags.Instance,
                                                            null,
                                                            Type.EmptyTypes,
                                                            null );

        if ( constructor == null && !type.IsValueType )
        {
            throw new SerializationException( $"No parameterless constructor for {type.Name}" );
        }

        _constructorCache[ type ] = NewFactory;

        return NewFactory();

        // --------------------------------------

        // Create a delegate to call the constructor (much faster for subsequent calls)
        object NewFactory()
        {
            return constructor != null ? constructor.Invoke( null ) : Activator.CreateInstance( type )!;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="jsonData"></param>
    private void ReadFields( object obj, JsonValue jsonData )
    {
        Type type = obj.GetType();

        // Ensure cache is built
        if ( !_typeCache.TryGetValue( type, out var cachedFields ) )
        {
            cachedFields       = BuildFieldCache( type );
            _typeCache[ type ] = cachedFields;
        }

        // Track what we find
        var fieldsFound = new HashSet< string >( StringComparer.OrdinalIgnoreCase );

        for ( JsonValue? child = jsonData.Child; child != null; child = child.Next )
        {
            if ( child.Name == TypeName )
            {
                continue;
            }

            // Find the matching cached field by its JSON name
            CachedField? target =
                cachedFields.Find( f => f.JsonName.Equals( child.Name, StringComparison.OrdinalIgnoreCase ) );

            if ( target != null )
            {
                var value = ReadValue< object >( target.Field.FieldType, null, child );
                target.Field.SetValue( obj, value );
                fieldsFound.Add( target.JsonName );
            }
        }

        // Post-Loop Validation: Check for missing required fields
        foreach ( CachedField fieldMeta in cachedFields )
        {
            if ( fieldMeta.IsRequired && !fieldsFound.Contains( fieldMeta.JsonName ) )
            {
                throw new SerializationException( $"Required field '{fieldMeta.JsonName}' "
                                                + $"was missing in JSON for type {type.Name}" );
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="enumType"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private object ParseEnum( Type enumType, string value )
    {
        // Check the cache
        if ( !_enumCache.TryGetValue( enumType, out var nameMap ) )
        {
            nameMap = new Dictionary< string, object >( StringComparer.OrdinalIgnoreCase );

            // Reflect on all enum members
            foreach ( FieldInfo field in enumType.GetFields( BindingFlags.Public | BindingFlags.Static ) )
            {
                var    attr = field.GetCustomAttribute< JsonNameAttribute >();
                string name = attr?.Name ?? field.Name;

                // GetValue(null) works because enum members are static constants
                nameMap[ name ] = field.GetValue( null )!;
            }

            _enumCache[ enumType ] = nameMap;
        }

        return nameMap.TryGetValue( value, out object? result )
            ? result
            // Fallback: Try standard C# parsing (handles integers or exact names)
            : Enum.Parse( enumType, value, true );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="fieldNames"></param>
    public virtual void SortFieldNames( Type type, List< string > fieldNames )
    {
        if ( SortFields )
        {
            fieldNames.Sort();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private Dictionary< string, FieldMetadata > GetFields( Type type )
    {
        Dictionary< string, FieldMetadata >? fields = _typeToFields.Get( type );

        if ( fields != null )
        {
            return fields;
        }

        List< Type? > classHierarchy = [ ];
        Type?         nextClass      = type;

        while ( nextClass != typeof( object ) )
        {
            classHierarchy.Add( nextClass );
            nextClass = nextClass?.BaseType;
        }

        List< FieldInfo > allFields = [ ];

        foreach ( Type? currentType in classHierarchy )
        {
            if ( currentType == null )
            {
                continue;
            }

            allFields.AddRange( currentType.GetFields( BindingFlags.Instance
                                                     | BindingFlags.NonPublic
                                                     | BindingFlags.Public ) );
        }

        var nameToField = new Dictionary< string, FieldMetadata >( allFields.Count );

        foreach ( FieldInfo field in allFields )
        {
            if ( field.IsStatic )
            {
                continue;
            }

            nameToField[ field.Name ] = new FieldMetadata( field );
        }

        SortFieldNames( type, nameToField.Keys.ToList() );

        _typeToFields[ type ] = nameToField;

        return nameToField;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private List< CachedField > BuildFieldCache( Type type )
    {
        var list = new List< CachedField >();
        FieldInfo[] allFields = type.GetFields( BindingFlags.Public
                                              | BindingFlags.NonPublic
                                              | BindingFlags.Instance );

        foreach ( FieldInfo f in allFields )
        {
            if ( f.IsDefined( typeof( NonSerializedAttribute ) ) )
            {
                continue;
            }

            var    attr       = f.GetCustomAttribute< JsonFieldAttribute >();
            string jsonName   = attr?.Name ?? f.Name;
            bool   isRequired = attr?.Required ?? false;

            list.Add( new CachedField( f, jsonName, isRequired ) );
        }

        return list;
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Simple Field Metadata container
    /// </summary>
    private class CachedField
    {
        public FieldInfo Field      { get; }
        public string    JsonName   { get; }
        public bool      IsRequired { get; }

        public CachedField( FieldInfo field, string jsonName, bool isRequired )
        {
            Field      = field;
            JsonName   = jsonName;
            IsRequired = isRequired;
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class FieldMetadata
    {
        public FieldInfo Field       { get; private set; }
        public Type?     ElementType { get; set; }
        public bool      Deprecated  { get; set; }

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

        public object Read( Json json, JsonValue jsonData, Type? type );
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
        public abstract object Read( Json json, JsonValue jsonData, Type? type );
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