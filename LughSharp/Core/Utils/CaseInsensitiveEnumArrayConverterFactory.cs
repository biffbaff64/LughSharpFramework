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

using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace LughSharp.Core.Utils;

/// <summary>
/// A <see cref="JsonConverterFactory"/> that creates converters for arrays of enum types,
/// enabling case-insensitive deserialization of enum values from JSON string arrays.
/// </summary>
[PublicAPI]
public class CaseInsensitiveEnumArrayConverterFactory : JsonConverterFactory
{
    /// <summary>
    /// Determines whether the specified type can be converted by this factory.
    /// </summary>
    /// <param name="typeToConvert">The type to check for convertibility.</param>
    /// <returns>
    /// <c>true</c> if the type is an array of enum values; otherwise, <c>false</c>.
    /// </returns>
    public override bool CanConvert( Type typeToConvert )
    {
        if ( !typeToConvert.IsArray )
        {
            return false;
        }

        var elementType = typeToConvert.GetElementType();

        return elementType is { IsEnum: true };
    }

    /// <summary>
    /// Creates a converter for the specified type.
    /// </summary>
    /// <param name="typeToConvert">The type to create a converter for.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>
    /// A <see cref="JsonConverter"/> instance for the specified type, or <c>null</c> if not applicable.
    /// </returns>
    public override JsonConverter? CreateConverter( Type typeToConvert, JsonSerializerOptions options )
    {
        var elementType   = typeToConvert.GetElementType();
        var converterType = typeof( CaseInsensitiveEnumArrayConverter<> ).MakeGenericType( elementType! );

        return ( JsonConverter? )Activator.CreateInstance( converterType );
    }
}

/// <summary>
/// A <see cref="JsonConverter{T}"/> for arrays of enum types that enables
/// case-insensitive deserialization from JSON string arrays.
/// </summary>
/// <typeparam name="Tenum">
/// The enum type to convert. Must be a struct and an <see cref="Enum"/>.
/// </typeparam>
[PublicAPI]
public class CaseInsensitiveEnumArrayConverter< Tenum > : JsonConverter< Tenum[] > where Tenum : struct, Enum
{
    /// <summary>
    /// Reads and converts a JSON array of strings to an array of enum values,
    /// performing case-insensitive parsing.
    /// </summary>
    /// <param name="reader">The reader to read from.</param>
    /// <param name="typeToConvert">The type to convert.</param>
    /// <param name="options">The serializer options to use.</param>
    /// <returns>
    /// An array of <typeparamref name="Tenum"/> values parsed from the JSON array.
    /// </returns>
    /// <exception cref="JsonException">
    /// Thrown if the JSON is not an array of strings or if a value cannot be parsed.
    /// </exception>
    public override Tenum[] Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        if ( reader.TokenType != JsonTokenType.StartArray )
        {
            throw new JsonException( "Expected start of array." );
        }

        var list = new List< Tenum >();

        while ( reader.Read() )
        {
            if ( reader.TokenType == JsonTokenType.EndArray )
            {
                break;
            }

            if ( reader.TokenType == JsonTokenType.String )
            {
                var stringValue = reader.GetString();

                if ( Enum.TryParse< Tenum >( stringValue, true, out var enumValue ) ) // Case-insensitive parse
                {
                    list.Add( enumValue );
                }
                else
                {
                    throw new JsonException( $"Unable to convert '{stringValue}' to enum '{typeof( Tenum ).Name}'." );
                }
            }
            else
            {
                throw new JsonException( "Expected string value for enum." );
            }
        }

        return list.ToArray();
    }

    /// <summary>
    /// Writes an array of enum values as a JSON array of strings.
    /// </summary>
    /// <param name="writer">The writer to write to.</param>
    /// <param name="value">The array of enum values to write.</param>
    /// <param name="options">The serializer options to use.</param>
    public override void Write( Utf8JsonWriter writer, Tenum[] value, JsonSerializerOptions options )
    {
        writer.WriteStartArray();

        foreach ( var enumValue in value )
        {
            writer.WriteStringValue( enumValue.ToString() );
        }

        writer.WriteEndArray();
    }
}