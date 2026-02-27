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

using NUnit.Framework;

using Color = LughSharp.Core.Graphics.Color;

namespace LughSharp.Tests.Source.Core.Graphics;

[TestFixture]
[TestOf( typeof( Color ) )]
public class ColorTest
{
    [Test]
    public void Constructor_Default_SetsComponentsToZero()
    {
        var color = new Color();

//        Assert.AreEqual( 0, color.R );
//        Assert.AreEqual( 0, color.G );
//        Assert.AreEqual( 0, color.B );
//        Assert.AreEqual( 0, color.A );
    }

    [TestCase( 0xff0000ff, 1f, 0f, 0f, 1f )]
    [TestCase( 0x00ff00ff, 0f, 1f, 0f, 1f )]
    [TestCase( 0x0000ffff, 0f, 0f, 1f, 1f )]
    [TestCase( 0x00000000, 0f, 0f, 0f, 0f )]
    public void Constructor_FromUint_SetsComponentsCorrectly( uint rgba, float expectedR, float expectedG,
                                                              float expectedB, float expectedA )
    {
        var color = new Color( rgba );

//        Assert.AreEqual( expectedR, color.R );
//        Assert.AreEqual( expectedG, color.G );
//        Assert.AreEqual( expectedB, color.B );
//        Assert.AreEqual( expectedA, color.A );
    }

    [Test]
    public void Constructor_FromColor_CopiesComponents()
    {
        var original = new Color( 0.5f, 0.4f, 0.3f, 0.2f );
        var color    = new Color( original );

//        Assert.AreEqual( original.R, color.R );
//        Assert.AreEqual( original.G, color.G );
//        Assert.AreEqual( original.B, color.B );
//        Assert.AreEqual( original.A, color.A );
    }

    [TestCase( 1.5f, 0f, -0.5f, 1.2f, 1f, 0f, 0f, 1f )] // Clamping
    public void Constructor_SetsComponentsAndClamps( float r, float g, float b, float a, float expectedR,
                                                     float expectedG, float expectedB, float expectedA )
    {
        var color = new Color( r, g, b, a );

//        Assert.AreEqual( expectedR, color.R );
//        Assert.AreEqual( expectedG, color.G );
//        Assert.AreEqual( expectedB, color.B );
//        Assert.AreEqual( expectedA, color.A );
    }

    [Test]
    public void ToFloatBitsRgba_ReturnsCorrectValue()
    {
        var   color     = new Color( 1f, 0.5f, 0f, 0.5f );
        float floatBits = color.ToFloatBitsRgba();

//        Assert.AreEqual( NumberUtils.UIntToFloatColor( 0xff7f007f ), floatBits );
    }

    [Test]
    public void ToFloatBitsAbgr_ReturnsCorrectValue()
    {
        var   color     = new Color( 1f, 0.5f, 0f, 0.5f );
        float floatBits = color.ToFloatBitsAbgr();

//        Assert.AreEqual( NumberUtils.UIntToFloatColor( 0x7f007fff ), floatBits );
    }

    [Test]
    public void Set_WithComponents_SetsCorrectly()
    {
        var color = new Color();
        color.Set( 0.1f, 0.2f, 0.4f, 0.6f );

//        Assert.AreEqual( 0.1f, color.R );
//        Assert.AreEqual( 0.2f, color.G );
//        Assert.AreEqual( 0.4f, color.B );
//        Assert.AreEqual( 0.6f, color.A );
    }

    [Test]
    public void Mul_WithValue_MultipliesComponents()
    {
        var color = new Color( 0.5f, 0.5f, 0.5f, 0.5f );

        color.Mul( 0.5f );

//        Assert.AreEqual( 0.25f, color.R );
//        Assert.AreEqual( 0.25f, color.G );
//        Assert.AreEqual( 0.25f, color.B );
//        Assert.AreEqual( 0.25f, color.A );
    }

    [Test]
    public void Add_WithComponents_AddsCorrectly()
    {
        var color = new Color( 0.1f, 0.2f, 0.3f, 0.4f );

        color.Add( 0.5f, 0.5f, 0.5f, 0.5f );

//        Assert.AreEqual( 0.6f, color.R );
//        Assert.AreEqual( 0.7f, color.G );
//        Assert.AreEqual( 0.8f, color.B );
//        Assert.AreEqual( 0.9f, color.A );
    }

    [Test]
    public void Sub_WithComponents_SubtractsCorrectly()
    {
        var color = new Color( 0.5f, 0.5f, 0.5f, 0.5f );

        color.Sub( 0.1f, 0.1f, 0.1f, 0.1f );

//        Assert.AreEqual( 0.4f, color.R );
//        Assert.AreEqual( 0.4f, color.G );
//        Assert.AreEqual( 0.4f, color.B );
//        Assert.AreEqual( 0.4f, color.A );
    }

    [Test]
    public void Clamp_ClampsValues()
    {
        var color = new Color( 1.5f, -0.2f, 0.8f, 1.2f );

        color.PremultiplyAlpha();

//        Assert.AreEqual( 1f, color.R );
//        Assert.AreEqual( 0f, color.G );
//        Assert.AreEqual( 0.8f, color.B );
//        Assert.AreEqual( 1f, color.A );
    }

    [Test]
    public void Equals_TrueForEqualColors()
    {
        var c1 = new Color( 0.1f, 0.2f, 0.3f, 0.4f );
        var c2 = new Color( 0.1f, 0.2f, 0.3f, 0.4f );

//        Assert.IsTrue( c1.Equals( c2 ) );
    }

    [Test]
    public void EqualsOperator_TrueForEqualColors()
    {
        var c1 = new Color( 0.1f, 0.2f, 0.3f, 0.4f );
        var c2 = new Color( 0.1f, 0.2f, 0.3f, 0.4f );

//        Assert.IsTrue( c1 == c2 );
    }

    [Test]
    public void Lerp_InterpolatesCorrectly()
    {
        var c1 = new Color( 0f, 0f, 0f, 1f );
        var c2 = new Color( 1f, 1f, 1f, 1f );

        c1.Lerp( c2, 0.5f );

//        Assert.AreEqual( 0.5f, c1.R );
//        Assert.AreEqual( 0.5f, c1.G );
//        Assert.AreEqual( 0.5f, c1.B );
//        Assert.AreEqual( 1f, c1.A );
    }

    [Test]
    public void PackedColorRgba_ReturnsCorrectValue()
    {
        var color = new Color( 1f, 0.5f, 0f, 1f );

        uint packed = color.PackedColorRgba();

        Assert.That( packed, Is.EqualTo( 0xff7f00ff ) );
    }

    [Test]
    public void FromHexString_ParsesCorrectly()
    {
        Color color = Color.FromHexString( "#ff00ff" );

        Assert.That( color.R, Is.EqualTo( 0xff ) );
        Assert.That( color.G, Is.EqualTo( 0f ).Within( 0.0001f ) );
        Assert.That( color.B, Is.EqualTo( 0xff ) );
        Assert.That( color.A, Is.EqualTo( 1f ).Within( 0.0001f ) );
    }

    [Test]
    public void FromHsv_SetsCorrectly()
    {
        var color = new Color();
        color.FromHsv( 0f, 1f, 1f );

        Assert.That( color.R, Is.EqualTo( 0f ).Within( 0.0001f ) );
        Assert.That( color.G, Is.EqualTo( 1f ).Within( 0.0001f ) );
        Assert.That( color.B, Is.EqualTo( 1f ).Within( 0.0001f ) );
    }
}