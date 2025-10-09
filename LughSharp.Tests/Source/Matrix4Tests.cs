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

using LughUtils.source.Maths;

using NUnit.Framework;

namespace LughSharp.Tests.Source;

[TestFixture]
public class Matrix4Tests
{
    private const float EPSILON = 0.000001f; // For float comparisons

    [Test]
    public void Identity_WhenCreated_HasCorrectValues()
    {
        var matrix = new Matrix4();

        Assert.Multiple( () =>
        {
            // Check diagonal elements are 1
            Assert.That( matrix.Val[ Matrix4.M00_0 ], Is.EqualTo( 1f ) );
            Assert.That( matrix.Val[ Matrix4.M11_5 ], Is.EqualTo( 1f ) );
            Assert.That( matrix.Val[ Matrix4.M22_10 ], Is.EqualTo( 1f ) );
            Assert.That( matrix.Val[ Matrix4.M33_15 ], Is.EqualTo( 1f ) );

            // Check non-diagonal elements are 0
            Assert.That( matrix.Val[ Matrix4.M01_4 ], Is.EqualTo( 0f ) );
            Assert.That( matrix.Val[ Matrix4.M02_8 ], Is.EqualTo( 0f ) );
            Assert.That( matrix.Val[ Matrix4.M03_12 ], Is.EqualTo( 0f ) );
            Assert.That( matrix.Val[ Matrix4.M10_1 ], Is.EqualTo( 0f ) );
            Assert.That( matrix.Val[ Matrix4.M12_9 ], Is.EqualTo( 0f ) );
            Assert.That( matrix.Val[ Matrix4.M13_13 ], Is.EqualTo( 0f ) );
            Assert.That( matrix.Val[ Matrix4.M20_2 ], Is.EqualTo( 0f ) );
            Assert.That( matrix.Val[ Matrix4.M21_6 ], Is.EqualTo( 0f ) );
            Assert.That( matrix.Val[ Matrix4.M23_14 ], Is.EqualTo( 0f ) );
            Assert.That( matrix.Val[ Matrix4.M30_3 ], Is.EqualTo( 0f ) );
            Assert.That( matrix.Val[ Matrix4.M31_7 ], Is.EqualTo( 0f ) );
            Assert.That( matrix.Val[ Matrix4.M32_11 ], Is.EqualTo( 0f ) );
        } );
    }

    [Test]
    public void SetToOrtho2D_CreatesCorrectOrthoMatrix()
    {
        var matrix = new Matrix4();
        var x      = 0f;
        var y      = 0f;
        var width  = 800f;
        var height = 600f;

        matrix.SetToOrtho2D( x, y, width, height );

        // For 2D orthographic projection, we expect:
        // Scale X = 2/width
        // Scale Y = 2/height
        // Translation X = -(right + left)/(right - left)
        // Translation Y = -(top + bottom)/(top - bottom)
        Assert.Multiple( () =>
        {
            Assert.That( matrix.Val[ Matrix4.M00_0 ], Is.EqualTo( 2f / width ) );
            Assert.That( matrix.Val[ Matrix4.M11_5 ], Is.EqualTo( 2f / height ) );
            Assert.That( matrix.Val[ Matrix4.M22_10 ], Is.EqualTo( -2f ) ); // -2/(far-near) where far-near = 1
            Assert.That( matrix.Val[ Matrix4.M23_14 ], Is.EqualTo( -1f ) ); // -(far+near)/(far-near)
            Assert.That( matrix.Val[ Matrix4.M33_15 ], Is.EqualTo( 1f ) );
        } );
    }

    [Test]
    public void Translation_AddsCorrectValues()
    {
        var matrix = new Matrix4();
        var tx     = 10f;
        var ty     = 20f;
        var tz     = 30f;

        matrix.Trn( tx, ty, tz );

        Assert.Multiple( () =>
        {
            Assert.That( matrix.Val[ Matrix4.M03_12 ], Is.EqualTo( tx ) );
            Assert.That( matrix.Val[ Matrix4.M13_13 ], Is.EqualTo( ty ) );
            Assert.That( matrix.Val[ Matrix4.M23_14 ], Is.EqualTo( tz ) );
        } );
    }

    [Test]
    public void Multiplication_WithIdentity_ReturnsOriginalMatrix()
    {
        var matrix = new Matrix4();
        matrix.Trn( 10f, 20f, 30f ); // Add some translation to make it non-identity
        var original = matrix.Cpy();
        var identity = new Matrix4();

        matrix.Mul( identity );

        for ( var i = 0; i < 16; i++ )
        {
            Assert.That( matrix.Val[ i ], Is.EqualTo( original.Val[ i ] ).Within( EPSILON ) );
        }
    }

    [Test]
    public void MatrixMultiplication_IsCorrect()
    {
        var m1 = new Matrix4();
        var m2 = new Matrix4();

        // Set some test values
        m1.Trn( 1f, 2f, 3f );
        m2.Trn( 4f, 5f, 6f );

        var result = m1.Cpy().Mul( m2 );

        // The translation components should add
        Assert.Multiple( () =>
        {
            Assert.That( result.Val[ Matrix4.M03_12 ], Is.EqualTo( 5f ).Within( EPSILON ) ); // 1 + 4
            Assert.That( result.Val[ Matrix4.M13_13 ], Is.EqualTo( 7f ).Within( EPSILON ) ); // 2 + 5
            Assert.That( result.Val[ Matrix4.M23_14 ], Is.EqualTo( 9f ).Within( EPSILON ) ); // 3 + 6
        } );
    }

    [Test]
    public void Invert_OfTranslationMatrix_ReturnsNegativeTranslation()
    {
        var matrix = new Matrix4();
        matrix.Trn( 2f, 3f, 4f );

        matrix.Invert();

        Assert.Multiple( () =>
        {
            Assert.That( matrix.Val[ Matrix4.M03_12 ], Is.EqualTo( -2f ).Within( EPSILON ) );
            Assert.That( matrix.Val[ Matrix4.M13_13 ], Is.EqualTo( -3f ).Within( EPSILON ) );
            Assert.That( matrix.Val[ Matrix4.M23_14 ], Is.EqualTo( -4f ).Within( EPSILON ) );
        } );
    }

    [Test]
    public void SetToOrtho_ValidatesAgainstKnownValues()
    {
        var matrix = new Matrix4();
        matrix.SetToOrtho( -10f, 10f, -10f, 10f, 1f, 100f );

        Assert.Multiple( () =>
        {
            // For orthographic projection with these parameters:
            // M22_10 = -2/(far-near) = -2/99 ≈ -0.0202020202
            // M23_14 = -(far+near)/(far-near) = -(101/99) ≈ -1.0202020202
            Assert.That( matrix.Val[ Matrix4.M22_10 ], Is.EqualTo( -0.0202020202f ).Within( EPSILON ) );
            Assert.That( matrix.Val[ Matrix4.M23_14 ], Is.EqualTo( -1.0202020202f ).Within( EPSILON ) );
        } );
    }

    [Test]
    public void Mul_CombiningProjectionAndView_IsCorrect()
    {
        var matrix = new Matrix4();

        // Setup projection for 800x600 screen
        matrix.SetToOrtho2D( 0, 0, 800, 600 );
        var projection = matrix.Cpy();

        // Create transform matrix with translation
        var transform = new Matrix4();
        transform.SetToTranslation( 100f, 100f, 0f );

        // Combine matrices (projection * transform)
        matrix.Mul( transform );

        // Test transforming a point (0,0,0)
        // In normalized device coordinates (NDC):
        // x goes from -1 (left) to 1 (right)
        // y goes from -1 (bottom) to 1 (top)
        var x = matrix.Val[ Matrix4.M03_12 ]; // Translation X after projection
        var y = matrix.Val[ Matrix4.M13_13 ]; // Translation Y after projection

        Assert.Multiple( () =>
        {
            // For 800x600 screen:
            // 100 pixels from left = 100/800 * 2 - 1 = -0.75
            // 100 pixels from bottom = 100/600 * 2 - 1 = -0.667
            Assert.That( x, Is.EqualTo( -0.75f ).Within( 0.01f ) );
            Assert.That( y, Is.EqualTo( -0.667f ).Within( 0.01f ) );
        } );
    }

    [Test]
    public void SetToTranslation_PreservesScaleAndRotation()
    {
        var matrix = new Matrix4();

        // First set scale and rotation
        matrix.SetToScaling( 2f, 3f, 1f );
        matrix.Rotate( Vector3.YDefault, 45f );

        // Now set translation
        matrix.SetTranslation( 10f, 20f, 30f );

        // Check translation is set
        Assert.Multiple( () =>
        {
            Assert.That( matrix.Val[ Matrix4.M03_12 ], Is.EqualTo( 10f ).Within( EPSILON ) );
            Assert.That( matrix.Val[ Matrix4.M13_13 ], Is.EqualTo( 20f ).Within( EPSILON ) );
            Assert.That( matrix.Val[ Matrix4.M23_14 ], Is.EqualTo( 30f ).Within( EPSILON ) );

            // Verify scale wasn't affected (length of basis vectors)
            Assert.That( Math.Sqrt( ( matrix.Val[ Matrix4.M00_0 ] * matrix.Val[ Matrix4.M00_0 ] ) +
                                    ( matrix.Val[ Matrix4.M01_4 ] * matrix.Val[ Matrix4.M01_4 ] ) +
                                    ( matrix.Val[ Matrix4.M02_8 ] * matrix.Val[ Matrix4.M02_8 ] ) ),
                         Is.EqualTo( 2f ).Within( EPSILON ) );
        } );
    }
}

// ========================================================================
// ========================================================================