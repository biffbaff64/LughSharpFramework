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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

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
    public JsonReader             Reader              { get; set; } = new();
    public JsonWriter?            JsonWriter          { get; set; }

    // ========================================================================

    private const bool DEBUG = false;

    private readonly Dictionary< Type, Dictionary< string, FieldMetadata > > _typeToFields      = [ ];
    private readonly Dictionary< Type, ISerializer< object > >               _classToSerializer = [ ];

    private readonly Dictionary< string, Type >   _tagToType            = [ ];
    private readonly Dictionary< Type, string >   _classToTag           = [ ];
    private readonly Dictionary< Type, object[] > _classToDefaultValues = [ ];

    private readonly object?[] _equals1 = new[] { ( object? )null };
    private readonly object?[] _equals2 = new[] { ( object? )null };

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

    /// <summary>
    /// Called for each unknown field name encountered by <see cref="ReadFields(object,JsonValue)"/>
    /// when <see cref="IgnoreUnknownFields"/> is false to determine whether the unknown field
    /// name should be ignored.
    /// </summary>
    /// <param name="type"> The object type being read. </param>
    /// <param name="fieldName">
    /// A field name encountered in the JSON for which there is no matching class field.
    /// </param>
    /// <returns>
    /// true if the field name should be ignored and an exception won't be thrown
    /// by <see cref="ReadFields(object,JsonValue)"/>.
    /// </returns>
    public virtual bool IgnoreUnknownField( Type type, string fieldName )
    {
        return false;
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
    /// Sets the default serializer for the specified Type.
    /// </summary>
    public void SetSerializer< T >( ISerializer< T > serializer )
    {
        _classToSerializer[ typeof( T ) ] = ( ISerializer< object > )serializer;
    }

    /// <summary>
    /// Sets the default serializer for the specified Type.
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

        // --- List Handling ---
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

            // Native List Handling (T[])
            if ( type.IsArray )
            {
                Type componentType = type.GetElementType()!;

                // Ensure we know what we are reading into the array
                elementType ??= componentType;

                // jsonData.Size represents the number of elements in the JSON array
                var resultArray = Array.CreateInstance( componentType, jsonData.Size );
                var i           = 0;

                for ( JsonValue? child = jsonData.Child; child != null; child = child.Next )
                {
                    // Read the value and place it in the specific index of the array
                    var value = ReadValue< object >( elementType, null, child );

                    resultArray.SetValue( value, i++ );
                }

                return ( T )( object )resultArray;
            }

            throw new SerializationException( $"Unable to convert array to: {type.Name}" );
        }

        // --- Primitive Value Handling (Number, Bool, string) ---
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
        return System.Type.GetType( name );
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
    /// <param name="value"></param>
    /// <returns></returns>
    public string? ConvertToString( Enum? value )
    {
        if ( value == null )
        {
            return null;
        }

        Type enumType = value.GetType();

        if ( Attribute.IsDefined( enumType, typeof( FlagsAttribute ) ) )
        {
            return value.ToString();
        }

        return Enum.GetName( enumType, value );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public string? ConvertToString( object? value )
    {
        if ( value is Enum e )
        {
            return ConvertToString( e );
        }

        if ( value is Type type )
        {
            return type.Name;
        }

        return value?.ToString();
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
                                                            System.Type.EmptyTypes,
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public string ToJson( object? obj )
    {
        return ToJson( obj, obj?.GetType(), ( Type? )null );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="knownType"></param>
    /// <returns></returns>
    public string ToJson( object? obj, Type? knownType )
    {
        return ToJson( obj, knownType, ( Type? )null );
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    /// <param name="elementType"> May be null if the type is unknown. </param>
    public string ToJson( object? obj, Type? knownType, Type? elementType )
    {
        using ( var buffer = new StringWriter() )
        {
            ToJson( obj, knownType, elementType, buffer );

            return buffer.ToString();
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="file"></param>
    public void ToJson( object? obj, FileInfo file )
    {
        ToJson( obj, obj?.GetType(), null, file );
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    /// <param name="file"></param>
    public void ToJson( object? obj, Type? knownType, FileInfo file )
    {
        ToJson( obj, knownType, null, file );
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    /// <param name="elementType"> May be null if the type is unknown. </param>
    /// <param name="file"></param>
    public void ToJson( object? obj, Type? knownType, Type? elementType, FileInfo file )
    {
        StreamWriter? writer = null;

        try
        {
            writer = file.CreateText();

            ToJson( obj, knownType, elementType, writer );
        }
        catch ( Exception ex )
        {
            throw new SerializationException( "Error writing file: " + file, ex );
        }
        finally
        {
            writer?.Close();
        }
    }

    public void ToJson( object? obj, StreamWriter writer )
    {
        ToJson( obj, obj?.GetType(), null, writer );
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    /// <param name="writer"></param>
    public void ToJson( object? obj, Type? knownType, StreamWriter writer )
    {
        ToJson( obj, knownType, null, writer );
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    /// <param name="elementType"> May be null if the type is unknown. </param>
    /// <param name="writer"></param>
    public void ToJson( object? obj, Type? knownType, Type? elementType, StreamWriter writer )
    {
        SetJsonWriter( writer );

        try
        {
            WriteValue( obj, knownType, elementType );
        }
        finally
        {
            writer.Close();
        }
    }

    // ========================================================================

    /// <summary>
    /// Sets the writer where JSON output will be written. This is only necessary
    /// when not using the ToJson methods.
    /// </summary>
    public void SetJsonWriter( StreamWriter writer )
    {
        this.JsonWriter = new JsonWriter( writer )
        {
            OutputType      = OutputType,
            QuoteLongValues = QuoteLongValues
        };
    }

    /// <summary>
    /// Writes all fields of the specified object to the current JSON object.
    /// </summary>
    public void WriteFields( object obj )
    {
        Type type = obj.GetType();

        object?[]? defaultValues = GetDefaultValues( type );

        Dictionary< string, FieldMetadata > fields       = GetFields( type );
        var                                 defaultIndex = 0;
        List< string >                      fieldNames   = fields.Keys.ToList();

        for ( int i = 0, n = fieldNames.Count; i < n; i++ )
        {
            FieldMetadata metadata = fields[ fieldNames[ i ] ];

            if ( IgnoreDeprecated && metadata.Deprecated )
            {
                continue;
            }

            FieldInfo field = metadata.Field;

            try
            {
                object? value = field.GetValue( obj );

                if ( defaultValues != null )
                {
                    object? defaultValue = defaultValues[ defaultIndex++ ];

                    if ( value == null && defaultValue == null )
                    {
                        continue;
                    }

                    if ( value != null && defaultValue != null )
                    {
                        if ( value.Equals( defaultValue ) )
                        {
                            continue;
                        }

                        if ( value.GetType().IsArray && defaultValue.GetType().IsArray )
                        {
                            _equals1[ 0 ] = value;
                            _equals2[ 0 ] = defaultValue;

                            // perform a 'deep equals' check on the arrays
                            if ( StructuralComparisons.StructuralEqualityComparer
                                                      .Equals( _equals1, _equals2 ) )
                            {
                                continue;
                            }
                        }
                    }
                }

                if ( DEBUG )
                {
                    Console.WriteLine( $"Writing field: {field.Name} ({type.Name})" );
                }

                JsonWriter?.Name( field.Name );
                WriteValue( value, field.GetType(), metadata.ElementType );
            }
            catch ( FieldAccessException ex )
            {
                throw new SerializationException( $"Error accessing field: {field.Name} ({type.Name})",
                                                  ex );
            }
            catch ( SerializationException ex )
            {
                ex.AddTrace( $"{field} ({type.Name})" );

                throw;
            }
            catch ( Exception runtimeEx )
            {
                var ex = new SerializationException( runtimeEx );
                ex.AddTrace( $"{field} ({type.Name})" );

                throw ex;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    private object[]? GetDefaultValues( Type type )
    {
        if ( !UsePrototypes )
        {
            return null;
        }

        if ( _classToDefaultValues.TryGetValue( type, out object[]? defaultValues ) )
        {
            return defaultValues;
        }

        object obj;

        try
        {
            obj = NewInstance( type );
        }
        catch ( Exception ex )
        {
            _classToDefaultValues[ type ] = null!; //TODO: Or remove?

            return null;
        }

        Dictionary< string, FieldMetadata > fields = GetFields( type );

        var defaultIndex = 0;
        var values       = new object[ fields.Count ];

        _classToDefaultValues[ type ] = values;

        List< string > fieldNames = fields.Keys.ToList();

        for ( int i = 0, n = fieldNames.Count; i < n; i++ )
        {
            FieldMetadata? metadata = fields.Get( fieldNames[ i ] );

            if ( ( metadata == null ) || ( IgnoreDeprecated && metadata.Deprecated ) )
            {
                continue;
            }

            FieldInfo field = metadata.Field;

            try
            {
                object? val = field.GetValue( obj );

                if ( val != null )
                {
                    values[ defaultIndex++ ] = val;
                }
            }
            catch ( FieldAccessException ex )
            {
                throw new SerializationException( $"Error accessing field: {field.Name} ({type.Name})",
                                                  ex );
            }
            catch ( SerializationException ex )
            {
                ex.AddTrace( $"{field} ({type.Name})" );

                throw;
            }
            catch ( RuntimeException runtimeEx )
            {
                var ex = new SerializationException( runtimeEx );
                ex.AddTrace( $"{field} ({type.Name})" );

                throw ex;
            }
        }

        return values;
    }

    public void WriteField( object obj, string name )
    {
        WriteField( obj, name, name, null );
    }

    public void WriteField( object obj, string name, Type? elementType )
    {
        WriteField( obj, name, name, elementType );
    }

    public void WriteField( object obj, string fieldName, string jsonName )
    {
        WriteField( obj, fieldName, jsonName, null );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fieldName"></param>
    /// <param name="jsonName"></param>
    /// <param name="elementType"></param>
    /// <exception cref="SerializationException"></exception>
    public void WriteField( object obj, string fieldName, string jsonName, Type? elementType )
    {
        Type           type     = obj.GetType();
        FieldMetadata? metadata = GetFields( type ).Get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type.Name})" );
        }

        FieldInfo field = metadata.Field;

        elementType ??= metadata.ElementType;

        try
        {
            if ( DEBUG )
            {
                Console.WriteLine( $"Writing {nameof( field )}: {field.Name} ({type.Name})" );
            }

            JsonWriter?.Name( jsonName );
            WriteValue( field.GetValue( obj ), field.GetType(), elementType );
        }
        catch ( FieldAccessException ex )
        {
            throw new SerializationException( "Error accessing field: " + field.Name + " (" + type.Name + ")",
                                              ex );
        }
        catch ( SerializationException ex )
        {
            ex.AddTrace( field + " (" + type.Name + ")" );

            throw;
        }
        catch ( Exception runtimeEx )
        {
            SerializationException ex = new SerializationException( runtimeEx );
            ex.AddTrace( field + " (" + type.Name + ")" );

            throw ex;
        }
    }

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
            throw new SerializationException( ex );
        }

        if ( value == null )
        {
            WriteValue( value, null, null );
        }
        else
        {
            WriteValue( value, value.GetType(), null );
        }
    }

    /// <summary>
    /// Writes the value as a field on the current JSON object, writing the class
    /// of the object if it differs from the specified known type.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"> May be null. </param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    public void WriteValue( string name, object? value, Type? knownType )
    {
        try
        {
            JsonWriter?.Name( name );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex );
        }

        WriteValue( value, knownType, null );
    }

    /// <summary>
    /// Writes the value as a field on the current JSON object, writing the class of the
    /// object if it differs from the specified known type. The specified element type is
    /// used as the default type for collections.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"> May be null. </param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    /// <param name="elementType"> May be null if the type is unknown. </param>
    public void WriteValue( string name, object? value, Type? knownType, Type? elementType )
    {
        try
        {
            JsonWriter?.Name( name );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex );
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
    /// Writes the value, writing the class of the object if it differs from the
    /// specified known type.
    /// </summary>
    /// <param name="value"> May be null. </param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    public void WriteValue( object? value, Type? knownType )
    {
        WriteValue( value, knownType, null );
    }

    /// <summary>
    /// Writes the value, writing the class of the object if it differs from the specified
    /// known type. The specified element type is used as the default type for collections.
    /// </summary>
    /// <param name="value"> May be null. </param>
    /// <param name="knownType"> May be null if the type is unknown. </param>
    /// <param name="elementType"> May be null if the type is unknown. </param>
    public void WriteValue( object? value, Type? knownType = null, Type? elementType = null )
    {
        try
        {
            if ( value == null )
            {
                JsonWriter?.Value( null );

                return;
            }

            Type actualType = value.GetType();

            // 1. Primitive / Simple types
            // In C#, actualType.IsPrimitive covers int, float, bool, etc. 
            // We add decimal and string to round it out.
            if ( actualType.IsPrimitive || actualType == typeof( string ) || actualType == typeof( decimal ) )
            {
                if ( knownType != null && knownType != actualType )
                {
                    WriteObjectStart( actualType, null );
                    WriteValue( "value", value );
                    WriteObjectEnd();
                }
                else
                {
                    JsonWriter?.Value( value );
                }

                return;
            }

            // 2. Custom Serializable (LibGDX pattern)
            if ( value is ISerializable serializable )
            {
                WriteObjectStart( actualType, knownType );
                serializable.Write( this );
                WriteObjectEnd();

                return;
            }

            // 3. Custom Registered Serializers
            if ( _classToSerializer.TryGetValue( actualType, out var serializer ) )
            {
                serializer.Write( this, value, knownType );

                return;
            }

            // 4. Dictionary / Map special cases
            // This replaces Map, ObjectMap, ArrayMap, etc.
            if ( value is IDictionary dict )
            {
                knownType ??= typeof( IDictionary );

                WriteObjectStart( actualType, knownType );

                foreach ( DictionaryEntry entry in dict )
                {
                    JsonWriter?.Name( ConvertToString( entry.Key ) );
                    WriteValue( entry.Value, elementType, null );
                }

                WriteObjectEnd();

                return;
            }

            // 5. Enum special case
            if ( typeof( Enum ).IsAssignableFrom( actualType ) )
            {
                // Handle Java-style enum-inner-classes (rare in C#, but good for safety)
                if ( actualType is { IsEnum: false, BaseType.IsEnum: true } )
                {
                    actualType = actualType.BaseType;
                }

                if ( TypeName != null && ( knownType == null || knownType != actualType ) )
                {
                    WriteObjectStart( actualType, null );
                    JsonWriter?.Name( "value" );
                    JsonWriter?.Value( ConvertToString( ( Enum )value ) );
                    WriteObjectEnd();
                }
                else
                {
                    JsonWriter?.Value( ConvertToString( ( Enum )value ) );
                }

                return;
            }

            // 6. List / Collection / Array cases
            // This covers Array, List, Collection, Queue, Set, etc.
            if ( value is IEnumerable enumerable )
            {
                // If it's a standard array, we need the element type
                if ( actualType.IsArray && elementType == null )
                    elementType = actualType.GetElementType();

                WriteArrayStart();

                foreach ( var item in enumerable )
                {
                    WriteValue( item, elementType, null );
                }

                WriteArrayEnd();

                return;
            }

            // 7. Default: Write fields via reflection
            WriteObjectStart( actualType, knownType );
            WriteFields( value );
            WriteObjectEnd();
        }
        catch ( Exception ex )
        {
            throw new SerializationException( ex );
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
            throw new SerializationException( ex );
        }

        WriteObjectStart();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="actualType"></param>
    /// <param name="knownType"></param>
    /// <exception cref="SerializationException"></exception>
    public void WriteObjectStart( string name, Type actualType, Type? knownType )
    {
        try
        {
            JsonWriter?.Name( name );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex );
        }

        WriteObjectStart( actualType, knownType );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="SerializationException"></exception>
    public void WriteObjectStart()
    {
        try
        {
            JsonWriter?.Object();
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actualType"></param>
    /// <param name="knownType"></param>
    /// <exception cref="SerializationException"></exception>
    public void WriteObjectStart( Type actualType, Type? knownType )
    {
        try
        {
            JsonWriter?.Object();
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex );
        }

        if ( knownType == null || knownType != actualType )
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
            throw new SerializationException( ex );
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
            throw new SerializationException( ex );
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
            throw new SerializationException( ex );
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
            throw new SerializationException( ex );
        }
    }

    public void WriteType( Type type )
    {
        if ( TypeName == null )
        {
            return;
        }

        string className = GetTag( type ) ?? type.Name;

        try
        {
            JsonWriter?.Set( TypeName, className );
        }
        catch ( IOException ex )
        {
            throw new SerializationException( ex );
        }

        if ( DEBUG )
        {
            Console.WriteLine( $"Writing type: {type.Name}" );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="reader"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? FromJson< T >( Type type, StreamReader reader )
    {
        return ReadValue< T >( type, null, this.Reader.Parse( reader ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="elementType"></param>
    /// <param name="reader"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? FromJson< T >( Type type, Type elementType, StreamReader reader )
    {
        return ReadValue< T >( type, elementType, this.Reader.Parse( reader ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="elementType"></param>
    /// <param name="input"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? FromJson< T >( Type type, Type elementType, Stream input )
    {
        return ReadValue< T >( type, elementType, this.Reader.Parse( input ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="file"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    public T? FromJson< T >( Type type, FileInfo file )
    {
        try
        {
            return ReadValue< T >( type, null, this.Reader.Parse( file ) );
        }
        catch ( Exception ex )
        {
            throw new SerializationException( "Error reading file: " + file, ex );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="elementType"></param>
    /// <param name="file"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="SerializationException"></exception>
    public T? FromJson< T >( Type type, Type elementType, FileInfo file )
    {
        try
        {
            return ReadValue< T >( type, elementType, this.Reader.Parse( file ) );
        }
        catch ( Exception ex )
        {
            throw new SerializationException( "Error reading file: " + file, ex );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? FromJson< T >( Type type, char[] data, int offset, int length )
    {
        return ReadValue< T >( type, null, this.Reader.Parse( data, offset, length ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="elementType"></param>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="length"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? FromJson< T >( Type type, Type elementType, char[] data, int offset, int length )
    {
        return ReadValue< T >( type, elementType, this.Reader.Parse( data, offset, length ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="json"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T? FromJson< T >( Type type, string json )
    {
        return ReadValue< T >( type, null, this.Reader.Parse( json ) );
    }

    /// <param name="type"> May be null if the type is unknown. </param>
    /// <param name="elementType"></param>
    /// <param name="json"></param>
    /// <returns> May be null. </returns>
    public T? FromJson< T >( Type type, Type elementType, string json )
    {
        return ReadValue< T >( type, elementType, this.Reader.Parse( json ) );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="jsonData"></param>
    public void ReadField( object obj, string name, JsonValue jsonData )
    {
        ReadField( obj, name, name, null, jsonData );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <param name="elementType"></param>
    /// <param name="jsonData"></param>
    public void ReadField( object obj, string name, Type? elementType, JsonValue jsonData )
    {
        ReadField( obj, name, name, elementType, jsonData );
    }

    /// <summary>
    /// 
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
    /// <param name="obj"> May be null if the field is static. </param>
    /// <param name="fieldName"></param>
    /// <param name="jsonName"></param>
    /// <param name="elementType"> May be null if the type is unknown. </param>
    /// <param name="jsonMap"></param>
    public void ReadField( object obj, string fieldName, string jsonName, Type? elementType, JsonValue jsonMap )
    {
        Type           type     = obj.GetType();
        FieldMetadata? metadata = GetFields( type ).Get( fieldName );

        if ( metadata == null )
        {
            throw new SerializationException( $"Field not found: {fieldName} ({type.Name})" );
        }

        FieldInfo field = metadata.Field;

        elementType ??= metadata.ElementType;

        ReadField( obj, field, jsonName, elementType, jsonMap );
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"> May be null if the field is static. </param>
    /// <param name="field"></param>
    /// <param name="jsonName"></param>
    /// <param name="elementType"> May be null if the type is unknown. </param>
    /// <param name="jsonMap"></param>
    public void ReadField( object? obj, FieldInfo field, string jsonName, Type? elementType, JsonValue jsonMap )
    {
        JsonValue? jsonValue = jsonMap.Get( jsonName );

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
            throw new SerializationException( $"Error accessing field: {field.Name} "
                                            + $"({field.DeclaringType?.Name})",
                                              ex );
        }
        catch ( SerializationException ex )
        {
            ex.AddTrace( $"{field.Name} ({field.DeclaringType?.Name})" );

            throw;
        }
        catch ( RuntimeException runtimeEx )
        {
            var ex = new SerializationException( runtimeEx );
            ex.AddTrace( jsonValue.Trace() );
            ex.AddTrace( $"{field.Name} ({field.DeclaringType?.Name})" );

            throw ex;
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
            CachedField? target = cachedFields.Find
                ( f => f.JsonName.Equals( child.Name, StringComparison.OrdinalIgnoreCase ) );

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

    /// <summary>
    /// Represents metadata for a field, containing information such as the field's type,
    /// associated element type, and deprecated status.
    /// <para>
    /// This class is used to encapsulate detailed information about a field's structure
    /// and attributes, which can be leveraged for tasks such as serialization or runtime
    /// type introspection.
    /// </para>
    /// </summary>
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

        /// <summary>
        /// Helper to check for open generic types like <c>ObjectMap</c>, <c>Dictionary</c> etc.
        /// </summary>
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
    /// Provides a mechanism for serializing and deserializing objects of type <c>T</c>
    /// in a read-only manner. This abstract class allows derived classes to implement custom
    /// deserialization while prohibiting modification of the serialized data.
    /// </summary>
    /// <typeparam name="T">The type of object to be serialized and deserialized.</typeparam>
    [PublicAPI]
    public abstract class ReadOnlySerializer< T > : ISerializer< T >
    {
        /// <summary>
        /// Writes the specified object of type <c>T</c> to the provided <see cref="Json"/> instance.
        /// </summary>
        /// <param name="json">The <see cref="Json"/> instance to write the data into.</param>
        /// <param name="obj">The object of type <typeparamref name="T"/> to be written.</param>
        /// <param name="knownType">The known type of the object being written, if applicable.</param>
        public void Write( Json json, T obj, Type knownType )
        {
        }

        /// <summary>
        /// Reads data from a JSON value and converts it to an object of the specified type.
        /// </summary>
        /// <param name="json">The JSON context used for deserialization.</param>
        /// <param name="jsonData">The JSON data to be read and deserialized.</param>
        /// <param name="type">
        /// The type of object to deserialize the JSON value into. This can be null,
        /// allowing the method to infer the type.
        /// </param>
        /// <returns>Returns the deserialized object of the specified type.</returns>
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