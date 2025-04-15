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

using JetBrains.Annotations;

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Json;

namespace LughSharp.Tests.Source;

public class Person
{
    public string              Name    { get; set; }
    public int                 Age     { get; set; }
    public List< PhoneNumber > Numbers { get; set; }
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
        
//        var test = new Test1
//        {
//            BooleanField    = true,
//            ByteField       = 123,
//            CharField       = 'Z',
//            ShortField      = 12345,
//            IntField        = 123456,
//            LongField       = 123456789,
//            FloatField      = 123.456f,
//            DoubleField     = 1.23456d,
//            WBooleanField   = true,
//            WByteField      = -12,
//            WCharacterField = 'X',
//            WShortField     = -12345,
//            WIntegerField   = -123456,
//            WLongField      = -123456789L,
//            WFloatField     = -123.3f,
//            WDoubleField    = -0.121231d,
//            StringField     = "stringvalue",
//            ByteArrayField  = [ 2, 1, 0, -1, -2 ],
//            Map             = [ ],
//        };
//
//        Logger.Checkpoint();
//
//        test.Map.Put( "one", 1 );
//        test.Map.Put( "two", 2 );
//        test.Map.Put( "nine", 9 );
//        test.StringArray =
//        [
//            "meow",
//            "moo",
//        ];
//        test.ObjectArray =
//        [
//            "meow",
//            new Test1(),
//        ];
//        test.LongMap = new Dictionary< long, string >( 4 );
//        test.LongMap.Put( 42L, "The Answer" );
//        test.LongMap.Put( 1964, "A Great Year" );
//        test.StringFloatMap = new Dictionary< string, float >( 4 );
//        test.StringFloatMap.Put( "point one", 0.1f );
//        test.StringFloatMap.Put( "point double oh seven", 0.007f );
//        test.SomeEnum = SomeEnum.B;
//
//        Logger.Checkpoint();
//        
//        // IntIntMap can be written, but only as a normal obj, not as a kind of map.
//        test.IntsToIntsUnboxed = [ ];
//        test.IntsToIntsUnboxed.Put( 102, 14 );
//        test.IntsToIntsUnboxed.Put( 107, 1 );
//        test.IntsToIntsUnboxed.Put( 10, 2 );
//        test.IntsToIntsUnboxed.Put( 2, 1 );
//        test.IntsToIntsUnboxed.Put( 7, 3 );
//        test.IntsToIntsUnboxed.Put( 101, 63 );
//        test.IntsToIntsUnboxed.Put( 4, 2 );
//        test.IntsToIntsUnboxed.Put( 106, 4 );
//        test.IntsToIntsUnboxed.Put( 1, 1 );
//        test.IntsToIntsUnboxed.Put( 103, 2 );
//        test.IntsToIntsUnboxed.Put( 6, 2 );
//        test.IntsToIntsUnboxed.Put( 3, 1 );
//        test.IntsToIntsUnboxed.Put( 105, 6 );
//        test.IntsToIntsUnboxed.Put( 8, 2 );
//
//        Logger.Checkpoint();
//
//        // The above "should" print like this:
//        // {size:14,keyTable:[0,0,102,0,0,0,0,0,107,0,0,10,0,0,0,2,0,0,0,0,7,0,0,0,0,0,101,0,0,0,4,0,106,
//        //                    0,0,0,0,0,0,1,0,0,103,0,0,6,0,0,0,0,0,0,0,0,3,0,0,105,0,0,8,0,0,0],
//        //                    valueTable:[0,0,14,0,0,0,0,0,1,0,0,2,0,0,0,1,0,0,0,0,3,0,0,0,0,0,63,0,0,0,2,
//        //                    0,4,0,0,0,0,0,0,1,0,0,2,0,0,2,0,0,0,0,0,0,0,0,1,0,0,6,0,0,2,0,0,0]}
//        // This is potentially correct, but also quite large considering the contents.
//        // It would be nice to have IntIntMap look like IntMap<Integer> does, below.
//
//        // IntMap gets special treatment and is written as a kind of map.
//        test.IntsToIntsBoxed = [ ];
//        test.IntsToIntsBoxed.Put( 102, 14 );
//        test.IntsToIntsBoxed.Put( 107, 1 );
//        test.IntsToIntsBoxed.Put( 10, 2 );
//        test.IntsToIntsBoxed.Put( 2, 1 );
//        test.IntsToIntsBoxed.Put( 7, 3 );
//        test.IntsToIntsBoxed.Put( 101, 63 );
//        test.IntsToIntsBoxed.Put( 4, 2 );
//        test.IntsToIntsBoxed.Put( 106, 4 );
//        test.IntsToIntsBoxed.Put( 1, 1 );
//        test.IntsToIntsBoxed.Put( 103, 2 );
//        test.IntsToIntsBoxed.Put( 6, 2 );
//        test.IntsToIntsBoxed.Put( 3, 1 );
//        test.IntsToIntsBoxed.Put( 105, 6 );
//        test.IntsToIntsBoxed.Put( 8, 2 );
//
//        Logger.Checkpoint();
//        
//        // The above should print like this:
//        // {102:14,107:1,10:2,2:1,7:3,101:63,4:2,106:4,1:1,103:2,6:2,3:1,105:6,8:2}
//
//        RoundTrip( test );
//        var sum = 0;
//
//        // iterate over an Dictionary<int,int> so one of its Entries is instantiated
//        foreach ( var e in test.IntsToIntsUnboxed )
//        {
//            sum += e.Value + 1;
//        }
//
//        // also iterate over an Array, which does not have any problems
//        var concat = "";
//
//        foreach ( var s in test.StringArray )
//        {
//            concat += s;
//        }
//
//        // by round-tripping again, we verify that the Entries is correctly skipped
//        RoundTrip( test );
//        var sum2 = 0;
//
//        // check and make sure that no entries are skipped over or incorrectly added
//        foreach ( var e in test.IntsToIntsUnboxed )
//        {
//            sum2 += e.Value + 1;
//        }
//
//        var concat2 = "";
//
//        // also check the Array again
//        foreach ( var s in test.StringArray )
//        {
//            concat2 += s;
//        }
//
//        Logger.Debug( "before: " + sum + ", after: " + sum2 );
//        Logger.Debug( "before: " + concat + ", after: " + concat2 );
//
//        test.SomeEnum = default( SomeEnum );
//        RoundTrip( test );
//
//        test = new Test1();
//        RoundTrip( test );
//
//        test.StringArray = [ ];
//        RoundTrip( test );
//
//        test.StringArray.Add( "meow" );
//        RoundTrip( test );
//
//        test.StringArray.Add( "moo" );
//        RoundTrip( test );
//
//        var objectGraph = new TestMapGraph();
//        TestObjectGraph( objectGraph, "exoticTypeName" );
//
//        test = new Test1
//        {
//            Map = [ ],
//        };
//
//        RoundTrip( test );
//
//        test.Map.Put( "one", 1 );
//        RoundTrip( test );
//
//        test.Map.Put( "two", 2 );
//        test.Map.Put( "nine", 9 );
//        RoundTrip( test );
//
//        test.Map.Put( "\nst\nuff\n", 9 );
//        test.Map.Put( "\r\nst\r\nuff\r\n", 9 );
//        RoundTrip( test );
//
//        Equals( _json.ToJson( "meow" ), "meow" );
//        Equals( _json.ToJson( "meow " ), "\"meow \"" );
//        Equals( _json.ToJson( " meow" ), "\" meow\"" );
//        Equals( _json.ToJson( " meow " ), "\" meow \"" );
//        Equals( _json.ToJson( "\nmeow\n" ), @"\nmeow\n" );
//
//        var arr1 = new[] { 1, 2, 3 };
//        var arr2 = new[] { "1", "2", "3" };
//        var arr3 = new[] { " 1", "2 ", " 3 " };
//        var arr4 = new[] { "1", "", "3" };
//
//        Equals( _json.ToJson( arr1, null, typeof( int ) ), "[1,2,3]" );
//        Equals( _json.ToJson( arr2, null, typeof( string ) ), "[1,2,3]" );
//        Equals( _json.ToJson( arr3, null, typeof( string ) ), "[\" 1\",\"2 \",\" 3 \"]" );
//        Equals( _json.ToJson( arr4, null, typeof( string ) ), "[1,\"\",3]" );

        Logger.NewLine();
        Logger.Debug( "Finished" );
    }

    private string? RoundTrip( object? obj )
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