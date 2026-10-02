// // /////////////////////////////////////////////////////////////////////////////
// //  MIT License
// //
// //  Copyright (c) 2024 Richard Ikin
// //
// //  Permission is hereby granted, free of charge, to any person obtaining a copy
// //  of this software and associated documentation files (the "Software"), to deal
// //  in the Software without restriction, including without limitation the rights
// //  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// //  copies of the Software, and to permit persons to whom the Software is
// //  furnished to do so, subject to the following conditions:
// //
// //  The above copyright notice and this permission notice shall be included in all
// //  copies or substantial portions of the Software.
// //
// //  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// //  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// //  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// //  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// //  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// //  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// //  SOFTWARE.
// // /////////////////////////////////////////////////////////////////////////////

using LughSharp.Source;

namespace LughSharp.Tests;

[PublicAPI]
[TestFixture]
public class PreferencesTest
{
    [Test]
    public void Run()
    {
        IPreferences prefs = Engine.App.GetPreferences( ".test" );

        if ( prefs.Contains( "bool" ) )
        {
            if ( prefs.GetBoolean( "bool" ) != true ) throw new LughRuntimeException( "bool failed" );
            if ( prefs.GetInteger( "int" ) != 1234 ) throw new LughRuntimeException( "int failed" );
            if ( prefs.GetLong( "long" ) != long.MaxValue ) throw new LughRuntimeException( "long failed" );
            if ( Math.Abs( prefs.GetFloat( "float" ) - 1.2345f ) > 0.0001f ) throw new LughRuntimeException( "float failed" );
            if ( !prefs.GetString( "string" ).Equals( "test!" ) ) throw new LughRuntimeException( "string failed" );
        }

        prefs.Clear();
        prefs.PutBoolean( "bool", true );
        prefs.PutInteger( "int", 1234 );
        prefs.PutLong( "long", long.MaxValue );
        prefs.PutFloat( "float", 1.2345f );
        prefs.PutString( "string", "test!" );
        prefs.Flush();

        if ( prefs.GetBoolean( "bool" ) != true ) throw new LughRuntimeException( "bool failed" );
        if ( prefs.GetInteger( "int" ) != 1234 ) throw new LughRuntimeException( "int failed" );
        if ( prefs.GetLong( "long" ) != long.MaxValue ) throw new LughRuntimeException( "long failed" );
        if ( Math.Abs( prefs.GetFloat( "float" ) - 1.2345f ) > 0.0001f ) throw new LughRuntimeException( "float failed" );
        if ( !prefs.GetString( "string" ).Equals( "test!" ) ) throw new LughRuntimeException( "string failed" );
    }
}

// ============================================================================
// ============================================================================
