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

using System.Collections;
using System.Text;

using JetBrains.Annotations;

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Json;

namespace LughSharp.Tests.Source;

public class Person
{
    public string              Name    { get; set; } = "";
    public int                 Age     { get; set; }
    public List< PhoneNumber > Numbers { get; set; } = null!;
}

public class PhoneNumber
{
    public string Name   { get; set; }
    public string Number { get; set; }

    public PhoneNumber( string name, string number )
    {
        Name = name;
        Number = number;
    }
}

[PublicAPI]
public class JsonTest : LughTestAdapter
{
    private Json? _json;

    public override void Create()
    {
        Logger.Checkpoint();

        var person = new Person
        {
            Name = "Richard",
            Age  = 61,
        };
        
        List< PhoneNumber > numbers =
        [
            new( "Home", "123-1234-12345" ),
            new( "Work", "456-7890-12345" ),
        ];
        
        person.Numbers = numbers;

        _json = new Json();

        Logger.Debug( _json.ToJson( person ) );
        
//        var arr1 = new[] { 1, 2, 3 };
        var arr2 = new[] { "4", "5", "6" };
        var arr3 = new[] { " 1", "2 ", " 3 " };
        var arr4 = new[] { "7", "", "9" };

        var sb = new StringBuilder();
        
//        sb.Append( _json.ToJson( arr1, null, typeof( int ) ) );
        sb.Append( _json.ToJson( arr2, null, typeof( string ) ) );
        sb.Append( _json.ToJson( arr3, null, typeof( string ) ) );
        sb.Append( _json.ToJson( arr4, null, typeof( string ) ) );

        Logger.Debug( sb.ToString() );        
        Logger.Debug( _json.PrettyPrint( sb.ToString(), new JsonValue.PrettyPrintSettings() ) );
        Logger.Debug( "Finished" );
    }

    private string? PerformTests( object? obj )
    {
        Logger.Checkpoint();

        var text = _json?.ToJson( obj );

        Logger.Debug( text! );
        Test( text, obj );

        text = _json?.PrettyPrint( obj!, 130 );
        Logger.Debug( text! );
        Test( text, obj );

        return text;
    }

    private static void TestObjectGraph( TestMapGraph obj, string typeName )
    {
        Logger.Checkpoint();

        var json = new Json
        {
            TypeName            = typeName,
            UsePrototypes       = false,
            IgnoreUnknownFields = true,
            OutputType          = JsonOutputType.Json,
        };

        var text    = json.PrettyPrint( obj );
        var object2 = json.FromJson< TestMapGraph >( typeof( TestMapGraph ), text ?? "ERROR" );

        if ( object2?.Map.Count != obj.Map.Count )
        {
            throw new GdxRuntimeException( "Too many items in deserialized json map." );
        }

        if ( object2.ObjectMap.Size != obj.ObjectMap.Size )
        {
            throw new GdxRuntimeException( "Too many items in deserialized json obj map." );
        }

        if ( object2.ArrayMap.Count != obj.ArrayMap.Count )
        {
            throw new GdxRuntimeException( "Too many items in deserialized json map." );
        }
    }

    private void Test( string? text, object? obj )
    {
        Logger.Checkpoint();
        Logger.Debug( $"text: {text}" );
        Logger.Debug( $"obj: {obj?.GetType().Name}" );

        Check( text, obj );

        text = text?.Replace( "{", "/*moo*/{/*moo*/" );
        Check( text, obj );

        text = text?.Replace( "}", "/*moo*/}/*moo*/" );
        text = text?.Replace( "[", "/*moo*/[/*moo*/" );
        text = text?.Replace( "]", "/*moo*/]/*moo*/" );
        text = text?.Replace( ":", "/*moo*/:/*moo*/" );
        text = text?.Replace( ",", "/*moo*/,/*moo*/" );
        Check( text, obj );

        text = text?.Replace( "/*moo*/", " /*moo*/ " );
        Check( text, obj );

        text = text?.Replace( "/*moo*/", "// moo\n" );
        Check( text, obj );

        text = text?.Replace( "\n", "\r\n" );
        Check( text, obj );

        text = text?.Replace( ",", "\n" );
        Check( text, obj );

        text = text?.Replace( "\n", "\r\n" );
        Check( text, obj );

        text = text?.Replace( "\r\n", "\r\n\r\n" );
        Check( text, obj );
    }

    private void Check( string? text, object? obj )
    {
        Logger.Checkpoint();

        var object2 = _json?.FromJson< object >( obj?.GetType(), text ?? "ERROR" );

        Logger.Checkpoint();

        Equals( obj!, object2 );
    }

    public new void Equals( object a, object? b )
    {
        if ( !a.Equals( b ) )
        {
            throw new GdxRuntimeException( "Fail!\n" + a + "\n!=\n" + b );
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Test1
    {
        // Primitives.
        public bool   BooleanField { get; set; }
        public byte   ByteField    { get; set; }
        public char   CharField    { get; set; }
        public short  ShortField   { get; set; }
        public int    IntField     { get; set; }
        public long   LongField    { get; set; }
        public float  FloatField   { get; set; }
        public double DoubleField  { get; set; }

        // Primitive wrappers.
        public bool?   WBooleanField   { get; set; }
        public sbyte?  WByteField      { get; set; }
        public char?   WCharacterField { get; set; }
        public short?  WShortField     { get; set; }
        public int?    WIntegerField   { get; set; }
        public long?   WLongField      { get; set; }
        public float?  WFloatField     { get; set; }
        public double? WDoubleField    { get; set; }

        // Other.
        public string?                      StringField       { get; set; }
        public sbyte[]?                     ByteArrayField    { get; set; }
        public object?                      Obj               { get; set; }
        public Dictionary< string, int >?   Map               { get; set; }
        public List< string >?              StringArray       { get; set; }
        public List< object >?              ObjectArray       { get; set; }
        public Dictionary< long, string >?  LongMap           { get; set; }
        public Dictionary< string, float >? StringFloatMap    { get; set; }
        public SomeEnum                     SomeEnum          { get; set; }
        public Dictionary< int, int >?      IntsToIntsBoxed   { get; set; }
        public Dictionary< int, int >?      IntsToIntsUnboxed { get; set; }

        // ====================================================================

        public new bool Equals( object? obj )
        {
            if ( this == obj )
            {
                return true;
            }

            if ( obj == null )
            {
                return false;
            }

            if ( GetType() != obj.GetType() )
            {
                return false;
            }

            var other = ( Test1 )obj;

            if ( WBooleanField == null )
            {
                if ( other.WBooleanField != null )
                {
                    return false;
                }
            }
            else if ( !WBooleanField.Equals( other.WBooleanField ) )
            {
                return false;
            }

            if ( WByteField == null )
            {
                if ( other.WByteField != null )
                {
                    return false;
                }
            }
            else if ( !WByteField.Equals( other.WByteField ) )
            {
                return false;
            }

            if ( WCharacterField == null )
            {
                if ( other.WCharacterField != null )
                {
                    return false;
                }
            }
            else if ( !WCharacterField.Equals( other.WCharacterField ) )
            {
                return false;
            }

            if ( WDoubleField == null )
            {
                if ( other.WDoubleField != null )
                {
                    return false;
                }
            }
            else if ( !WDoubleField.Equals( other.WDoubleField ) )
            {
                return false;
            }

            if ( WFloatField == null )
            {
                if ( other.WFloatField != null )
                {
                    return false;
                }
            }
            else if ( !WFloatField.Equals( other.WFloatField ) )
            {
                return false;
            }

            if ( WIntegerField == null )
            {
                if ( other.WIntegerField != null )
                {
                    return false;
                }
            }
            else if ( !WIntegerField.Equals( other.WIntegerField ) )
            {
                return false;
            }

            if ( WLongField == null )
            {
                if ( other.WLongField != null )
                {
                    return false;
                }
            }
            else if ( !WLongField.Equals( other.WLongField ) )
            {
                return false;
            }

            if ( WShortField == null )
            {
                if ( other.WShortField != null )
                {
                    return false;
                }
            }
            else if ( !WShortField.Equals( other.WShortField ) )
            {
                return false;
            }

            if ( StringField == null )
            {
                if ( other.StringField != null )
                {
                    return false;
                }
            }
            else if ( !StringField.Equals( other.StringField ) )
            {
                return false;
            }

            if ( BooleanField != other.BooleanField )
            {
                return false;
            }

            var list1 = ArrayToList( ByteArrayField );
            var list2 = ArrayToList( other.ByteArrayField );

            if ( list1 != list2 )
            {
                if ( ( list1 == null ) || ( list2 == null ) )
                {
                    return false;
                }

                if ( !list1.Equals( list2 ) )
                {
                    return false;
                }
            }

            if ( obj != other.Obj )
            {
                if ( ( obj == null ) || ( other.Obj == null ) )
                {
                    return false;
                }

                if ( ( obj != this ) && !obj.Equals( other.Obj ) )
                {
                    return false;
                }
            }

            if ( !Equals( Map, other.Map ) )
            {
                if ( ( Map == null ) || ( other.Map == null ) )
                {
                    return false;
                }

                if ( !Map.Keys.ToArray().Equals( other.Map.Keys.ToArray() ) )
                {
                    return false;
                }

                if ( !Map.Values.ToArray().Equals( other.Map.Values.ToArray() ) )
                {
                    return false;
                }
            }

            if ( StringArray != other.StringArray )
            {
                if ( ( StringArray == null ) || ( other.StringArray == null ) )
                {
                    return false;
                }

                if ( !StringArray.Equals( other.StringArray ) )
                {
                    return false;
                }
            }

            if ( ObjectArray != other.ObjectArray )
            {
                if ( ( ObjectArray == null ) || ( other.ObjectArray == null ) )
                {
                    return false;
                }

                if ( !ObjectArray.Equals( other.ObjectArray ) )
                {
                    return false;
                }
            }

            if ( LongMap != other.LongMap )
            {
                if ( ( LongMap == null ) || ( other.LongMap == null ) )
                {
                    return false;
                }

                if ( !LongMap.Equals( other.LongMap ) )
                {
                    return false;
                }
            }

            if ( StringFloatMap != other.StringFloatMap )
            {
                if ( ( StringFloatMap == null ) || ( other.StringFloatMap == null ) )
                {
                    return false;
                }

                if ( !StringFloatMap.Equals( other.StringFloatMap ) )
                {
                    return false;
                }
            }

            if ( IntsToIntsBoxed != other.IntsToIntsBoxed )
            {
                if ( ( IntsToIntsBoxed == null ) || ( other.IntsToIntsBoxed == null ) )
                {
                    return false;
                }

                if ( !IntsToIntsBoxed.Equals( other.IntsToIntsBoxed ) )
                {
                    return false;
                }
            }

            if ( IntsToIntsUnboxed != other.IntsToIntsUnboxed )
            {
                if ( ( IntsToIntsUnboxed == null ) || ( other.IntsToIntsUnboxed == null ) )
                {
                    return false;
                }

                if ( !IntsToIntsUnboxed.Equals( other.IntsToIntsUnboxed ) )
                {
                    return false;
                }
            }

            if ( ByteField != other.ByteField )
            {
                return false;
            }

            if ( CharField != other.CharField )
            {
                return false;
            }

            if ( BitConverter.DoubleToInt64Bits( DoubleField ) != BitConverter.DoubleToInt64Bits( other.DoubleField ) )
            {
                return false;
            }

            if ( BitConverter.SingleToInt32Bits( FloatField ) != BitConverter.SingleToInt32Bits( other.FloatField ) )
            {
                return false;
            }

            if ( IntField != other.IntField )
            {
                return false;
            }

            if ( LongField != other.LongField )
            {
                return false;
            }

            if ( ShortField != other.ShortField )
            {
                return false;
            }

            return true;
        }
    }

    private static object? ArrayToList( object? array )
    {
        if ( ( array == null ) || !array.GetType().IsArray )
        {
            return array;
        }

        var elementType = array.GetType().GetElementType();

        if ( elementType == null )
        {
            return new List< object >(); // Handle empty array type
        }

        var listType = typeof( List<> ).MakeGenericType( elementType );
        var list     = ( IList? )Activator.CreateInstance( listType );
        var arr      = ( Array )array; // Cast 'array' to Array

        var rank    = arr.Rank; // Now you can access Rank
        var indices = new int[ rank ];

        foreach ( var item in IterateArray( arr, 0 ) )
        {
            list?.Add( ArrayToList( item ) ); // Recursively convert nested arrays
        }

        return list;

        IEnumerable IterateArray( Array arrLocal, int dimension )
        {
            if ( dimension < rank )
            {
                for ( var i = 0; i < arrLocal.GetLength( dimension ); i++ )
                {
                    indices[ dimension ] = i;

                    foreach ( var item in IterateArray( arrLocal, dimension + 1 ) )
                    {
                        yield return item;
                    }
                }
            }
            else
            {
                yield return arrLocal.GetValue( indices );
            }
        }
    }

    // ========================================================================

    [PublicAPI]
    public class TestMapGraph
    {
        public Dictionary< string, string > Map       = [ ];
        public ObjectMap< string, string >  ObjectMap = [ ];
        public Dictionary< string, string > ArrayMap  = [ ];

        public TestMapGraph()
        {
            Map.Put( "a", "b" );
            Map.Put( "c", "d" );
            ObjectMap.Put( "a", "b" );
            ObjectMap.Put( "c", "d" );
            ArrayMap.Put( "a", "b" );
            ArrayMap.Put( "c", "d" );
        }
    }

    // ========================================================================

    [PublicAPI]
    public enum SomeEnum
    {
        A,
        B,
        C,
    }
}