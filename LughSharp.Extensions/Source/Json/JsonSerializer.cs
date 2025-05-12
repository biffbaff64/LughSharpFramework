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

using System.Text;

namespace Extensions.Source.Json;

// ========================================================================

[PublicAPI]
public class JsonSerializer
{
    public string? SerializeToString( object obj )
    {
        using ( var stringWriter = new StringWriter() )
        using ( var jsonWriter = new JsonTextWriter( stringWriter ) )
        {
            WriteValue( obj, null, null, jsonWriter );

            return jsonWriter.ToString();
        }
    }

    public void SerializeToFileUTF8( object obj, string filePath )
    {
        SerializeToFile( obj, filePath, Encoding.UTF8 );
    }
    
    public void SerializeToFile( object obj, string filePath, Encoding encoding )
    {
        using ( var fileStream = new FileStream( filePath, FileMode.Create, FileAccess.Write ) )
        using ( var streamWriter = new StreamWriter( fileStream, encoding ) )
        using ( var jsonWriter = new JsonTextWriter( streamWriter ) )
        {
            WriteValue( obj, null, null, jsonWriter );
        }
    }

    private void WriteValue( object? value, Type? elementType, Type? knownType, TextWriter writer )
    {
        if ( value == null )
        {
            writer.Write( "null" );

            return;
        }

        var actualType = value.GetType();

        if ( actualType == typeof( string ) )
        {
            WriteString( ( string )value, writer );
        }
        else if ( actualType.IsPrimitive )
        {
            writer.Write( value.ToString() );
        }

        // ... and so on, using writer.Write() ...
    }

    private static void WriteString( string value, TextWriter writer )
    {
        writer.Write( $"\"{EscapeString( value )}\"" );
    }

    private static string EscapeString( string str )
    {
        return str.Replace( "\\", @"\\" ).Replace( "\"", "\\\"" );
    }
}

// ========================================================================

[PublicAPI]
public abstract class ReadOnlySerializer< T > : IJsonSerializer
{
    public virtual void Write( Json json, object obj, Type? knownType )
    {
    }

    public abstract object Read( Json json, JsonValue jsonData, Type? type );
}

// ========================================================================

[PublicAPI]
public interface IJsonSerializer
{
    void Write( Json json, object obj, Type? knownType );

    object Read( Json json, JsonValue jsonData, Type? type );
}

// ========================================================================

[PublicAPI]
public interface IJsonSerializer< T >
{
    void Write( Json json, T obj, Type knownType );

    T Read( Json json, JsonValue jsonData, Type? type );
}

// ========================================================================

[PublicAPI]
public interface IJsonSerializable
{
    void Write( Json json );

    void Read( Json json, JsonValue jsonData );
}