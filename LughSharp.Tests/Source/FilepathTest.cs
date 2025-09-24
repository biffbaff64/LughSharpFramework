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

using JetBrains.Annotations;

using NUnit.Framework;

using LughSharp.Lugh.Core;
using LughSharp.Lugh.Utils.Logging;

namespace LughSharp.Tests.Source;

[TestFixture]
[PublicAPI]
public class FilepathTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Run()
    {
        try
        {
            Logger.Debug( $"Absolute : {Engine.Api.Files.Absolute( "C:/Development/Projects/CSharp/ConsoleApp1/bin/Debug/net8.0/PackedImages/objects/rover_wheel.png" ).FullName}" );
            Logger.Debug( $"Assembly : {Engine.Api.Files.Assembly( "PackedImages/objects/rover_wheel.png" ).FullName}" );
            Logger.Debug( $"Classpath: {Engine.Api.Files.Classpath( "PackedImages/objects/rover_wheel.png" ).FullName}" );
            Logger.Debug( $"External : {Engine.Api.Files.External( "PackedImages/objects/rover_wheel.png" ).FullName}" );
            Logger.Debug( $"Internal : {Engine.Api.Files.Internal( "PackedImages/objects/rover_wheel.png" ).FullName}" );
            Logger.Debug( $"Local    : {Engine.Api.Files.Local( "PackedImages/objects/rover_wheel.png" ).FullName}" );
        }
        catch ( Exception )
        {
            // Ignore
        }
    }

    [TearDown]
    public void TearDown()
    {
    }
}

// ========================================================================
// ========================================================================