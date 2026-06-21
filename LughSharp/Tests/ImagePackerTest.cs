// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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

using System.Drawing.Imaging;
using System.Runtime.Versioning;

using LughSharp.Source.Graphics.Packing.ImagePacker;
using LughSharp.Source.IO;

namespace LughSharp.Tests;

[TestFixture]
[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class ImagePackerTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Run()
    {
        var rand   = new Random( 0 );
        var packer = new ImagePacker( 2048, 2048, 1, true );

        var images = Enumerable.Range( 0, 100 )
                               .Select( _ => ImagePacker.CreateImage( rand.Next( 10, 61 ),
                                                                       rand.Next( 10, 61 ),
                                                                       PixelFormat.Format32bppArgb,
                                                                       System.Drawing.Color.FromArgb( 255,
                                                                                       ( byte ) rand.Next( 256 ),
                                                                                       ( byte ) rand.Next( 256 ),
                                                                                       ( byte ) rand.Next( 256 ) ) ) )
                               .OrderByDescending( img => img.Width * img.Height )
                               .ToArray();

        for ( var i = 0; i < images.Length; i++ )
        {
            packer.InsertImage( $"image_{i}", images[ i ] );
        }

        packer.Image.Save( Files.AssetPath( "packed.png" ) );
    }

    [TearDown]
    public void TearDown()
    {
    }
}

// ========================================================================
// ========================================================================