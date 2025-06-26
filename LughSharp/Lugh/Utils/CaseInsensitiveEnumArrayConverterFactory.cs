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

using System.Text.Json.Serialization;

namespace LughSharp.Lugh.Utils;

[PublicAPI]
public class CaseInsensitiveEnumArrayConverterFactory : JsonConverterFactory
{
    public override bool CanConvert( Type typeToConvert )
    {
        if ( !typeToConvert.IsArray )
        {
            return false;
        }

        var elementType = typeToConvert.GetElementType();

        return elementType is { IsEnum: true };
    }

    public override JsonConverter? CreateConverter( Type typeToConvert, JsonSerializerOptions options )
    {
        var elementType   = typeToConvert.GetElementType();
        var converterType = typeof( CaseInsensitiveEnumArrayConverter<> ).MakeGenericType( elementType! );

        return ( JsonConverter? )Activator.CreateInstance( converterType );
    }
}

[PublicAPI]
public class CaseInsensitiveEnumArrayConverter< TEnum > : JsonConverter< TEnum[] > where TEnum : struct, Enum
{
    public override TEnum[] Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
    {
        if ( reader.TokenType != JsonTokenType.StartArray )
        {
            throw new JsonException( "Expected start of array." );
        }

        var list = new List< TEnum >();

        while ( reader.Read() )
        {
            if ( reader.TokenType == JsonTokenType.EndArray )
            {
                break;
            }

            if ( reader.TokenType == JsonTokenType.String )
            {
                var stringValue = reader.GetString();

                if ( Enum.TryParse< TEnum >( stringValue, true, out var enumValue ) ) // Case-insensitive parse
                {
                    list.Add( enumValue );
                }
                else
                {
                    throw new JsonException( $"Unable to convert '{stringValue}' to enum '{typeof( TEnum ).Name}'." );
                }
            }
            else
            {
                throw new JsonException( "Expected string value for enum." );
            }
        }

        return list.ToArray();
    }

    public override void Write( Utf8JsonWriter writer, TEnum[] value, JsonSerializerOptions options )
    {
        writer.WriteStartArray();

        foreach ( var enumValue in value )
        {
            writer.WriteStringValue( enumValue.ToString() );
        }

        writer.WriteEndArray();
    }
}