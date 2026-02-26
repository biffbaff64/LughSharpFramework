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

using JetBrains.Annotations;

namespace LughSharp.Core.Utils.Json;

/// <summary>
/// Reads / Writes objects to / from JSON, automatically.
/// </summary>
[PublicAPI]
public class Json
{
    public string? TypeName      { get; set; } = "class";
    public bool    UsePrototypes { get; set; } = true;

    // ========================================================================

    private const bool DEBUG = false;

    private readonly Dictionary< Type, Dictionary< string, FieldMetadata > > _typeToFields      = [ ];
    private readonly Dictionary< Type, ISerializer< object > >               _classToSerializer = [ ];

    private readonly Dictionary< string, Type >   _tagToType            = [ ];
    private readonly Dictionary< Type, string >   _classToTag           = [ ];
    private readonly Dictionary< Type, object[] > _classToDefaultValues = [ ];

    private readonly object?[] _equals1 = new[] { ( object? )null, };
    private readonly object?[] _equals2 = new[] { ( object? )null, };

    private JsonWriter?            _writer;
    private JsonReader             _reader = new();
    private JsonOutputType         _outputType;
    private ISerializer< object >? _defaultSerializer;

    private bool _quoteLongValues;
    private bool _ignoreUnknownFields;
    private bool _ignoreDeprecated;
    private bool _readDeprecated;
    private bool _enumNames = true;
    private bool _sortFields;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a new Json instance, and sets the default output type to
    /// <see cref="JsonOutputType.Minimal"/>.
    /// </summary>
    public Json()
    {
        _outputType = JsonOutputType.Minimal;
    }

    /// <summary>
    /// Creates a new Json instance, and sets the default output type to the
    /// specified <see cref="JsonOutputType"/> value.
    /// </summary>
    public Json( JsonOutputType outputType )
    {
        this._outputType = outputType;
    }

    public T? ReadValue< T >( string? name, Type? type, JsonValue jsonMap )
    {
        return ReadValue< T >( type, null, jsonMap.Get( name ) );
    }

    public T? ReadValue< T >( string name, Type? type, T? defaultValue, JsonValue jsonMap )
    {
//        JsonValue jsonValue = jsonMap.get( name );

//        if ( jsonValue == null ) return defaultValue;

//        return ReadValue< T >( type, null, jsonValue );
        throw new NotImplementedException();
    }

    public T? ReadValue< T >( string name, Type? type, Type? elementType, JsonValue jsonMap )
    {
//        return ReadValue< T >( type, elementType, jsonMap.get( name ) );
        throw new NotImplementedException();
    }

    public T? ReadValue< T >( string name, Type? type, Type? elementType, T defaultValue, JsonValue jsonMap )
    {
//        JsonValue jsonValue = jsonMap.get( name );

//        return ReadValue< T >( type, elementType, defaultValue, jsonValue );
        throw new NotImplementedException();
    }

    public T ReadValue< T >( Type? type, Type? elementType, T? defaultValue, JsonValue jsonData )
    {
//        if ( jsonData == null ) return defaultValue;

//        return ReadValue< T >( type, elementType, jsonData );
        throw new NotImplementedException();
    }

    public T? ReadValue< T >( Type? type, JsonValue jsonData )
    {
        throw new NotImplementedException();
    }

    public T? ReadValue< T >( Type? type, Type? elementType, JsonValue jsonData )
    {
        throw new NotImplementedException();
    }

    public virtual void ReadFields( object obj, JsonValue jsonMap )
    {
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

    public void AddClassTag( string entryKey, Type entryValue )
    {
        throw new NotImplementedException();
    }

    public Type? GetTagType( string valueMapName )
    {
        throw new NotImplementedException();
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class FieldMetadata
    {
//        final Field field;
//        Class   elementType;
//        boolean deprecated;
//
//        public FieldMetadata( Field field )
//        {
//            this.field = field;
//            int index = ( ClassReflection.isAssignableFrom( ObjectMap.class, field.getType())
//                || ClassReflection.isAssignableFrom( Map.class, field.getType())) ? 1 : 0;
//            this.elementType = field.getElementType( index );
//            deprecated       = field.isAnnotationPresent( Deprecated.class);
//        }
    }

    [PublicAPI]
    public interface ISerializer< T >
    {
        public void Write( Json json, T obj, Type knownType );

        public T Read( Json json, JsonValue jsonData, Type type );
    }

    [PublicAPI]
    public abstract class ReadOnlySerializer< T > : ISerializer< T >
    {
        public void Write( Json json, T obj, Type knownType )
        {
        }

        public abstract T Read( Json json, JsonValue jsonData, Type type );
    }

    [PublicAPI]
    public interface ISerializable
    {
        public void Write( Json json );

        public void Read( Json json, JsonValue jsonData );
    }
}

// ============================================================================
// ============================================================================