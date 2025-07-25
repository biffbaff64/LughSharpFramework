﻿// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Richard Ikin.
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

using System.Diagnostics.CodeAnalysis;

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

namespace LughSharp.Lugh.Maths;

/// <summary>
/// Column-Major Matrix4 class.
/// <code>
/// [M00] [M10] [M20] [M30]
/// [M01] [M11] [M21] [M31]
/// [M02] [M12] [M22] [M32]
/// [M03] [M13] [M23] [M33]
/// </code>
/// The Matrix is stored in memory as:-
/// <code>
///  +0   +1   +2   +3   +4   +5   +6   +7   +8   +9   +10  +11  +12  +13  +14  +15
/// [M00][M10][M20][M30][M01][M11][M21][M31][M02][M12][M22][M32][M03][M13][M23][M33]
/// </code>
/// </summary>
[PublicAPI]
public class Matrix4
{
    // ------------------------------------------
    // ROW 0
    // ------------------------------------------
    
    /// <summary>
    /// XX: Typically the unrotated X component for scaling, also the cosine
    /// of the angle when rotated on the Y and/or Z axis. On Vector3 multiplication
    /// this value is multiplied with the source X component and added to the
    /// target X component.
    /// </summary>
    public const int M00 = 0;

    /// <summary>
    /// XY: Typically the negative sine of the angle when rotated on the Z axis.
    /// On Vector3 multiplication this value is multiplied with the source Y
    /// component and added to the target X component.
    /// </summary>
    public const int M01 = 4;

    /// <summary>
    /// XZ: Typically the sine of the angle when rotated on the Y axis.
    /// On Vector3 multiplication this value is multiplied with the
    /// source Z component and added to the target X component.
    /// </summary>
    public const int M02 = 8;

    /// <summary>
    /// XW: Typically the translation of the X component. On Vector3 multiplication
    /// this value is added to the target X component.
    /// </summary>
    public const int M03 = 12;

    // ------------------------------------------
    // ROW 1
    // ------------------------------------------
    
    /// <summary>
    /// Column 1, Row 0.
    /// YX: Typically the sine of the angle when rotated on the Z axis. On Vector3
    /// multiplication this value is multiplied with the source X component and
    /// added to the target Y component.
    /// </summary>
    public const int M10 = 1;

    /// <summary>
    /// YY: Typically the unrotated Y component for scaling, also the cosine of the
    /// angle when rotated on the X and/or Z axis. On Vector3 multiplication this value
    /// is multiplied with the source Y component and added to the target Y component.
    /// </summary>
    public const int M11 = 5;

    /// <summary>
    /// YZ: Typically the negative sine of the angle when rotated on the X axis.
    /// On Vector3 multiplication this value is multiplied with the source Z component
    /// and added to the target Y component.
    /// </summary>
    public const int M12 = 9;

    /// <summary>
    /// YW: Typically the translation of the Y component.
    /// On Vector3 multiplication this value is added to the target Y component.
    /// </summary>
    public const int M13 = 13;

    // ------------------------------------------
    // ROW 2
    // ------------------------------------------
    
    /// <summary>
    /// ZX: Typically the negative sine of the angle when rotated on the Y axis.
    /// On Vector3 multiplication this value is multiplied with the source X component
    /// and added to the target Z component.
    /// </summary>
    public const int M20 = 2;

    /// <summary>
    /// ZY: Typically the sine of the angle when rotated on the X axis.
    /// On Vector3 multiplication this value is multiplied with the source Y component
    /// and added to the target Z component.
    /// </summary>
    public const int M21 = 6;

    /// <summary>
    /// ZZ: Typically the unrotated Z component for scaling, also the cosine of the angle
    /// when rotated on the X and/or Y axis. On Vector3 multiplication this value is
    /// multiplied with the source Z component and added to the target Z component.
    /// </summary>
    public const int M22 = 10;

    /// <summary>
    /// ZW: Typically the translation of the Z component. On Vector3 multiplication
    /// this value is added to the target Z component.
    /// </summary>
    public const int M23 = 14;

    // ------------------------------------------
    // ROW 3
    // ------------------------------------------

    /// <summary>
    /// WX: Typically the value zero. On Vector3 multiplication this value is ignored.
    /// </summary>
    public const int M30 = 3;

    /// <summary>
    /// WY: Typically the value zero. On Vector3 multiplication this value is ignored.
    /// </summary>
    public const int M31 = 7;

    /// <summary>
    /// WZ: Typically the value zero. On Vector3 multiplication this value is ignored.
    /// </summary>
    public const int M32 = 11;

    /// <summary>
    /// WW: Typically the value one. On Vector3 multiplication this value is ignored.
    /// </summary>
    public const int M33 = 15;

    // ========================================================================

    public static readonly Quaternion Quat       = new();
    public static readonly Quaternion Quat2      = new();
    public static readonly Vector3    LVez       = new();
    public static readonly Vector3    LVex       = new();
    public static readonly Vector3    LVey       = new();
    public static readonly Vector3    TmpVec     = new();
    public static readonly Vector3    Right      = new();
    public static readonly Vector3    TmpForward = new();
    public static readonly Vector3    TmpUp      = new();
    public static readonly Matrix4    TmpMat     = new();

    public float[] Val = new float[ 16 ];

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Constructs an identity matrix
    /// </summary>
    public Matrix4()
    {
        ToIdentity();
    }

    /// <summary>
    /// Constructs a matrix from the given matrix.
    /// </summary>
    /// <param name="matrix">
    /// The matrix to copy. (This matrix is not modified)
    /// </param>
    public Matrix4( Matrix4 matrix )
    {
        Set( matrix );
    }

    /// <summary>
    /// Constructs a matrix from the given float array. The array must have at
    /// least 16 elements; the first 16 will be copied.
    /// </summary>
    /// <param name="values">
    /// The float array to copy. Remember that this matrix is in column-major order.
    /// (The float array is not modified.)
    /// <para>
    /// See here:
    /// <a href="http://en.wikipedia.org/wiki/Row-major_order">wikipedia.org/wiki/Row-major_order</a>
    /// </para>
    /// </param>
    public Matrix4( float[] values )
    {
        Set( values );
    }

    /// <summary>
    /// Constructs a rotation matrix from the given <see cref="Quaternion" />.
    /// </summary>
    /// <param name="quaternion">The quaternion to be copied. (The quaternion is not modified)</param>
    public Matrix4( Quaternion quaternion )
    {
        Set( quaternion );
    }

    /// <summary>
    /// Construct a matrix from the given translation, rotation and scale.
    /// </summary>
    /// <param name="position"> The translation </param>
    /// <param name="rotation"> The rotation, must be normalized </param>
    /// <param name="scale"> The scale</param>
    public Matrix4( Vector3 position, Quaternion rotation, Vector3 scale )
    {
        Set( position, rotation, scale );
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <returns> the backing float array </returns>
    public float[] Values => Val;

    /// <summary>
    /// Sets the matrix to the given matrix.
    /// </summary>
    /// <param name="matrix"> The matrix that is to be copied.(The given matrix is not modified)</param>
    /// <returns> This matrix for the purpose of chaining methods together.</returns>
    public Matrix4 Set( Matrix4 matrix )
    {
        return Set( matrix.Val );
    }

    /// <summary>
    /// Sets the matrix to the given matrix as a float array. The float array must
    /// have at least 16 elements; the first 16 will be copied and any extra will
    /// be ignored.
    /// </summary>
    /// <param name="values">
    /// The matrix, in float form, that is to be copied. Remember that this matrix is in
    /// Column-Major order.
    /// <para>
    /// See here:
    /// <a href="http://en.wikipedia.org/wiki/Row-major_order">wikipedia.org/wiki/Row-major_order</a>
    /// </para>
    /// </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Set( float[] values )
    {
        Array.Copy( values, 0, Val, 0, Val.Length );

        return this;
    }

    /// <summary>
    /// Sets the matrix to a rotation matrix representing the quaternion.
    /// </summary>
    /// <param name="quaternion"> The quaternion that is to be used to Set this matrix. </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Set( Quaternion quaternion )
    {
        return Set( quaternion.X, quaternion.Y, quaternion.Z, quaternion.W );
    }

    /// <summary>
    /// Sets the matrix to a rotation matrix representing the quaternion.
    /// </summary>
    /// <param name="quaternionX">The X component of the quaternion that is to be used to Set this matrix.</param>
    /// <param name="quaternionY">The Y component of the quaternion that is to be used to Set this matrix.</param>
    /// <param name="quaternionZ">The Z component of the quaternion that is to be used to Set this matrix.</param>
    /// <param name="quaternionW">The W component of the quaternion that is to be used to Set this matrix.</param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Set( float quaternionX, float quaternionY, float quaternionZ, float quaternionW )
    {
        return Set( 0f, 0f, 0f, quaternionX, quaternionY, quaternionZ, quaternionW );
    }

    /// <summary>
    /// Set this matrix to the specified translation and rotation.
    /// </summary>
    /// <param name="position"> The translation </param>
    /// <param name="orientation"> The rotation, must be normalized </param>
    /// <returns> This matrix for chaining  </returns>
    public Matrix4 Set( Vector3 position, Quaternion orientation )
    {
        return Set( position.X, position.Y, position.Z, orientation.X, orientation.Y, orientation.Z, orientation.W );
    }

    /// <summary>
    /// Sets the matrix to a rotation matrix representing the translation and quaternion.
    /// </summary>
    /// <param name="translationX"> The X component of the translation that is to be used to Set this matrix. </param>
    /// <param name="translationY"> The Y component of the translation that is to be used to Set this matrix. </param>
    /// <param name="translationZ"> The Z component of the translation that is to be used to Set this matrix. </param>
    /// <param name="quaternionX"> The X component of the quaternion that is to be used to Set this matrix. </param>
    /// <param name="quaternionY"> The Y component of the quaternion that is to be used to Set this matrix. </param>
    /// <param name="quaternionZ"> The Z component of the quaternion that is to be used to Set this matrix. </param>
    /// <param name="quaternionW"> The W component of the quaternion that is to be used to Set this matrix. </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Set( float translationX,
                        float translationY,
                        float translationZ,
                        float quaternionX,
                        float quaternionY,
                        float quaternionZ,
                        float quaternionW )
    {
        float xs = quaternionX * 2f, ys = quaternionY * 2f, zs = quaternionZ * 2f;
        float wx = quaternionW * xs, wy = quaternionW * ys, wz = quaternionW * zs;
        float xx = quaternionX * xs, xy = quaternionX * ys, xz = quaternionX * zs;
        float yy = quaternionY * ys, yz = quaternionY * zs, zz = quaternionZ * zs;

        // Update the matrix row by row.
        // Top row.
        Val[ M00 ] = 1.0f - ( yy + zz );
        Val[ M10 ] = xy + wz;
        Val[ M20 ] = xz - wy;
        Val[ M30 ] = translationX;

        // Second row.
        Val[ M01 ] = xy - wz;
        Val[ M11 ] = 1.0f - ( xx + zz );
        Val[ M21 ] = yz + wx;
        Val[ M31 ] = translationY;

        // Third row.
        Val[ M02 ] = xz + wy;
        Val[ M12 ] = yz - wx;
        Val[ M22 ] = 1.0f - ( xx + yy );
        Val[ M32 ] = translationZ;

        // Bottom row.
        Val[ M03 ] = 0.0f;
        Val[ M13 ] = 0.0f;
        Val[ M23 ] = 0.0f;
        Val[ M33 ] = 1.0f;

        return this;
    }

    /// <summary>
    /// Set this matrix to the specified translation, rotation and scale.
    /// </summary>
    /// <param name="position"> The translation </param>
    /// <param name="orientation"> The rotation, must be normalized </param>
    /// <param name="scale"> The scale </param>
    /// <returns> This matrix for chaining  </returns>
    public Matrix4 Set( Vector3 position, Quaternion orientation, Vector3 scale )
    {
        return Set( position.X,
                    position.Y,
                    position.Z,
                    orientation.X,
                    orientation.Y,
                    orientation.Z,
                    orientation.W,
                    scale.X,
                    scale.Y,
                    scale.Z );
    }

    /// <summary>
    /// Sets the matrix to a rotation matrix representing the translation and quaternion.
    /// </summary>
    /// <param name="translationX"> The X component of the translation that is to be used to Set this matrix. </param>
    /// <param name="translationY"> The Y component of the translation that is to be used to Set this matrix. </param>
    /// <param name="translationZ"> The Z component of the translation that is to be used to Set this matrix. </param>
    /// <param name="quaternionX"> The X component of the quaternion that is to be used to Set this matrix. </param>
    /// <param name="quaternionY"> The Y component of the quaternion that is to be used to Set this matrix. </param>
    /// <param name="quaternionZ"> The Z component of the quaternion that is to be used to Set this matrix. </param>
    /// <param name="quaternionW"> The W component of the quaternion that is to be used to Set this matrix. </param>
    /// <param name="scaleX"> The X component of the scaling that is to be used to Set this matrix. </param>
    /// <param name="scaleY"> The Y component of the scaling that is to be used to Set this matrix. </param>
    /// <param name="scaleZ"> The Z component of the scaling that is to be used to Set this matrix. </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Set( float translationX,
                        float translationY,
                        float translationZ,
                        float quaternionX,
                        float quaternionY,
                        float quaternionZ,
                        float quaternionW,
                        float scaleX,
                        float scaleY,
                        float scaleZ )
    {
        float xs = quaternionX * 2f, ys = quaternionY * 2f, zs = quaternionZ * 2f;
        float wx = quaternionW * xs, wy = quaternionW * ys, wz = quaternionW * zs;
        float xx = quaternionX * xs, xy = quaternionX * ys, xz = quaternionX * zs;
        float yy = quaternionY * ys, yz = quaternionY * zs, zz = quaternionZ * zs;

        // Update the matrix row by row.
        // Top row.
        Val[ M00 ] = scaleX * ( 1.0f - ( yy + zz ) );
        Val[ M10 ] = scaleY * ( xy - wz );
        Val[ M20 ] = scaleZ * ( xz + wy );
        Val[ M30 ] = translationX;

        // Second row.
        Val[ M01 ] = scaleX * ( xy + wz );
        Val[ M11 ] = scaleY * ( 1.0f - ( xx + zz ) );
        Val[ M21 ] = scaleZ * ( yz - wx );
        Val[ M31 ] = translationY;

        // Third row.
        Val[ M02 ] = scaleX * ( xz - wy );
        Val[ M12 ] = scaleY * ( yz + wx );
        Val[ M22 ] = scaleZ * ( 1.0f - ( xx + yy ) );
        Val[ M32 ] = translationZ;

        // Bottom row.
        Val[ M03 ] = 0.0f;
        Val[ M13 ] = 0.0f;
        Val[ M23 ] = 0.0f;
        Val[ M33 ] = 1.0f;

        return this;
    }

    /// <summary>
    /// Sets the four columns of the matrix which correspond to the x-, y- and z-axis of the vector space this matrix
    /// creates as
    /// well as the 4th column representing the translation of any point that is multiplied by this matrix.
    /// </summary>
    /// <param name="xAxis"> The x-axis. </param>
    /// <param name="yAxis"> The y-axis. </param>
    /// <param name="zAxis"> The z-axis. </param>
    /// <param name="pos"> The translation vector.  </param>
    public Matrix4 Set( Vector3 xAxis, Vector3 yAxis, Vector3 zAxis, Vector3 pos )
    {
        // Update the matrix row by row.
        // Top row.
        Val[ M00 ] = xAxis.X;
        Val[ M10 ] = xAxis.Y;
        Val[ M20 ] = xAxis.Z;
        Val[ M30 ] = pos.X;

        // Second row.
        Val[ M01 ] = yAxis.X;
        Val[ M11 ] = yAxis.Y;
        Val[ M21 ] = yAxis.Z;
        Val[ M31 ] = pos.Y;

        // Third row.
        Val[ M02 ] = zAxis.X;
        Val[ M12 ] = zAxis.Y;
        Val[ M22 ] = zAxis.Z;
        Val[ M32 ] = pos.Z;

        // Bottom row.
        Val[ M03 ] = 0.0f;
        Val[ M13 ] = 0.0f;
        Val[ M23 ] = 0.0f;
        Val[ M33 ] = 1.0f;

        return this;
    }

    /// <returns> a copy of this matrix </returns>
    public Matrix4 Cpy()
    {
        return new Matrix4( this );
    }

    /// <summary>
    /// Adds a translational component to the matrix in the 4th column.
    /// The other columns are untouched.
    /// </summary>
    /// <param name="vector">
    /// The translation vector to add to the current matrix. (This vector is not modified)
    /// </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Trn( Vector3 vector )
    {
        Val[ M30 ] += vector.X;
        Val[ M31 ] += vector.Y;
        Val[ M32 ] += vector.Z;

        return this;
    }

    /// <summary>
    /// Adds a translational component to the matrix in the 4th column.
    /// The other columns are untouched.
    /// </summary>
    /// <param name="x"> The x-component of the translation vector. </param>
    /// <param name="y"> The y-component of the translation vector. </param>
    /// <param name="z"> The z-component of the translation vector. </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Trn( float x, float y, float z )
    {
        Val[ M30 ] += x;
        Val[ M31 ] += y;
        Val[ M32 ] += z;

        return this;
    }

    /// <summary>
    /// Postmultiplies this matrix with the given matrix, storing the result in this matrix. For example:
    /// <code>
    /// A.mul(B) results in A := AB.
    /// </code>
    /// </summary>
    /// <param name="matrix"> The other matrix to multiply by. </param>
    /// <returns> This matrix for the purpose of chaining operations together.  </returns>
    public Matrix4 Mul( Matrix4 matrix )
    {
        Mul( Val, matrix.Val );

        return this;
    }

    /// <summary>
    /// Premultiplies this matrix with the given matrix, storing the result in this matrix. For example:
    /// <para>
    /// <tt>A.mulLeft(B) results in A := BA.</tt>
    /// </para>
    /// </summary>
    /// <param name="matrix"> The other matrix to multiply by. </param>
    /// <returns> This matrix for the purpose of chaining operations together.  </returns>
    public Matrix4 MulLeft( Matrix4 matrix )
    {
        TmpMat.Set( matrix );

        Mul( TmpMat.Val, Val );

        return Set( TmpMat );
    }

    /// <summary>
    /// Transposes the matrix.
    /// A matrix:-
    /// <code>
    ///    a b c d
    ///    e f g h
    ///    i j k l
    ///    m n o p
    /// </code>
    /// will be transposed to:-
    /// <code>
    ///    a e i m
    ///    b f j n
    ///    c g k o
    ///    d h l p
    /// </code>
    /// </summary>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Transpose()
    {
        Array.Copy( Val, 0, TmpMat.Val, 0, Val.Length );

        // Transpose Column 1
        Val[ M00 ] = TmpMat.Val[ M00 ];
        Val[ M01 ] = TmpMat.Val[ M10 ];
        Val[ M02 ] = TmpMat.Val[ M20 ];
        Val[ M03 ] = TmpMat.Val[ M30 ];

        // Transpose Column 2
        Val[ M10 ] = TmpMat.Val[ M01 ];
        Val[ M11 ] = TmpMat.Val[ M11 ];
        Val[ M12 ] = TmpMat.Val[ M21 ];
        Val[ M13 ] = TmpMat.Val[ M31 ];

        // Transpose Column 3
        Val[ M20 ] = TmpMat.Val[ M02 ];
        Val[ M21 ] = TmpMat.Val[ M12 ];
        Val[ M22 ] = TmpMat.Val[ M22 ];
        Val[ M23 ] = TmpMat.Val[ M32 ];

        // Transpose Column 4
        Val[ M30 ] = TmpMat.Val[ M03 ];
        Val[ M31 ] = TmpMat.Val[ M13 ];
        Val[ M32 ] = TmpMat.Val[ M23 ];
        Val[ M33 ] = TmpMat.Val[ M33 ];

        return this;
    }

    /// <summary>
    /// Sets the matrix to an identity matrix.
    /// </summary>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 ToIdentity()
    {
        // Column 1
        Val[ M00 ] = 1f;
        Val[ M01 ] = 0f;
        Val[ M02 ] = 0f;
        Val[ M03 ] = 0f;

        // Column 2
        Val[ M10 ] = 0f;
        Val[ M11 ] = 1f;
        Val[ M12 ] = 0f;
        Val[ M13 ] = 0f;

        // Column 3
        Val[ M20 ] = 0f;
        Val[ M21 ] = 0f;
        Val[ M22 ] = 1f;
        Val[ M23 ] = 0f;

        // Column 4
        Val[ M30 ] = 0f;
        Val[ M31 ] = 0f;
        Val[ M32 ] = 0f;
        Val[ M33 ] = 1f;

        return this;
    }

    /// <summary>
    /// Inverts the matrix. Stores the result in this matrix.
    /// </summary>
    /// <returns> This matrix for the purpose of chaining methods together. </returns>
    /// <exception cref="GdxRuntimeException"> if the matrix is singular (not invertible)  </exception>
    public Matrix4 Invert()
    {
//        //@formatter:off
//        var lDet = ( ( ( ( ( ( ( ( ( ( (
//              ( Val[ M30 ] * Val[ M21 ] * Val[ M12 ] * Val[ M03 ] )
//            - ( Val[ M20 ] * Val[ M31 ] * Val[ M12 ] * Val[ M03 ] )
//            - ( Val[ M30 ] * Val[ M11 ] * Val[ M22 ] * Val[ M03 ] ) )
//            + ( Val[ M10 ] * Val[ M31 ] * Val[ M22 ] * Val[ M03 ] )
//            + ( Val[ M20 ] * Val[ M11 ] * Val[ M32 ] * Val[ M03 ] ) )
//            - ( Val[ M10 ] * Val[ M21 ] * Val[ M32 ] * Val[ M03 ] )
//            - ( Val[ M30 ] * Val[ M21 ] * Val[ M02 ] * Val[ M13 ] ) )
//            + ( Val[ M20 ] * Val[ M31 ] * Val[ M02 ] * Val[ M13 ] )
//            + ( Val[ M30 ] * Val[ M01 ] * Val[ M22 ] * Val[ M13 ] ) )
//            - ( Val[ M00 ] * Val[ M31 ] * Val[ M22 ] * Val[ M13 ] )
//            - ( Val[ M20 ] * Val[ M01 ] * Val[ M32 ] * Val[ M13 ] ) )
//            + ( Val[ M00 ] * Val[ M21 ] * Val[ M32 ] * Val[ M13 ] )
//            + ( Val[ M30 ] * Val[ M11 ] * Val[ M02 ] * Val[ M23 ] ) )
//            - ( Val[ M10 ] * Val[ M31 ] * Val[ M02 ] * Val[ M23 ] )
//            - ( Val[ M30 ] * Val[ M01 ] * Val[ M12 ] * Val[ M23 ] ) )
//            + ( Val[ M00 ] * Val[ M31 ] * Val[ M12 ] * Val[ M23 ] )
//            + ( Val[ M10 ] * Val[ M01 ] * Val[ M32 ] * Val[ M23 ] ) )
//            - ( Val[ M00 ] * Val[ M11 ] * Val[ M32 ] * Val[ M23 ] )
//            - ( Val[ M20 ] * Val[ M11 ] * Val[ M02 ] * Val[ M33 ] ) )
//            + ( Val[ M10 ] * Val[ M21 ] * Val[ M02 ] * Val[ M33 ] )
//            + ( Val[ M20 ] * Val[ M01 ] * Val[ M12 ] * Val[ M33 ] ) )
//            - ( Val[ M00 ] * Val[ M21 ] * Val[ M12 ] * Val[ M33 ] )
//            - ( Val[ M10 ] * Val[ M01 ] * Val[ M22 ] * Val[ M33 ] ) )
//            + ( Val[ M00 ] * Val[ M11 ] * Val[ M22 ] * Val[ M33 ] );
//        //@formatter:on
//
//        if ( lDet == 0f )
//        {
//            throw new GdxRuntimeException( "non-invertible matrix" );
//        }
//
//        //@formatter:off
//        var m00 = ( ( ( ( Val[ M12 ] * Val[ M23 ] * Val[ M31 ] )
//                      - ( Val[ M13 ] * Val[ M22 ] * Val[ M31 ] ) )
//                      + ( Val[ M13 ] * Val[ M21 ] * Val[ M32 ] ) )
//                      - ( Val[ M11 ] * Val[ M23 ] * Val[ M32 ] )
//                      - ( Val[ M12 ] * Val[ M21 ] * Val[ M33 ] ) )
//                      + ( Val[ M11 ] * Val[ M22 ] * Val[ M33 ] );
//
//        var m01 = ( ( ( Val[ M03 ] * Val[ M22 ] * Val[ M31 ] )
//                    - ( Val[ M02 ] * Val[ M23 ] * Val[ M31 ] )
//                    - ( Val[ M03 ] * Val[ M21 ] * Val[ M32 ] ) )
//                    + ( Val[ M01 ] * Val[ M23 ] * Val[ M32 ] )
//                    + ( Val[ M02 ] * Val[ M21 ] * Val[ M33 ] ) )
//                    - ( Val[ M01 ] * Val[ M22 ] * Val[ M33 ] );
//
//        var m02 = ( ( ( ( Val[ M02 ] * Val[ M13 ] * Val[ M31 ] )
//                      - ( Val[ M03 ] * Val[ M12 ] * Val[ M31 ] ) )
//                      + ( Val[ M03 ] * Val[ M11 ] * Val[ M32 ] ) )
//                      - ( Val[ M01 ] * Val[ M13 ] * Val[ M32 ] )
//                      - ( Val[ M02 ] * Val[ M11 ] * Val[ M33 ] ) )
//                      + ( Val[ M01 ] * Val[ M12 ] * Val[ M33 ] );
//
//        var m03 = ( ( ( Val[ M03 ] * Val[ M12 ] * Val[ M21 ] )
//                    - ( Val[ M02 ] * Val[ M13 ] * Val[ M21 ] )
//                    - ( Val[ M03 ] * Val[ M11 ] * Val[ M22 ] ) )
//                    + ( Val[ M01 ] * Val[ M13 ] * Val[ M22 ] )
//                    + ( Val[ M02 ] * Val[ M11 ] * Val[ M23 ] ) )
//                    - ( Val[ M01 ] * Val[ M12 ] * Val[ M23 ] );
//
//        var m10 = ( ( ( Val[ M13 ] * Val[ M22 ] * Val[ M30 ] )
//                    - ( Val[ M12 ] * Val[ M23 ] * Val[ M30 ] )
//                    - ( Val[ M13 ] * Val[ M20 ] * Val[ M32 ] ) )
//                    + ( Val[ M10 ] * Val[ M23 ] * Val[ M32 ] )
//                    + ( Val[ M12 ] * Val[ M20 ] * Val[ M33 ] ) )
//                    - ( Val[ M10 ] * Val[ M22 ] * Val[ M33 ] );
//
//        var m11 = ( ( ( ( Val[ M02 ] * Val[ M23 ] * Val[ M30 ] )
//                      - ( Val[ M03 ] * Val[ M22 ] * Val[ M30 ] ) )
//                      + ( Val[ M03 ] * Val[ M20 ] * Val[ M32 ] ) )
//                      - ( Val[ M00 ] * Val[ M23 ] * Val[ M32 ] )
//                      - ( Val[ M02 ] * Val[ M20 ] * Val[ M33 ] ) )
//                      + ( Val[ M00 ] * Val[ M22 ] * Val[ M33 ] );
//
//        var m12 = ( ( ( Val[ M03 ] * Val[ M12 ] * Val[ M30 ] )
//                    - ( Val[ M02 ] * Val[ M13 ] * Val[ M30 ] )
//                    - ( Val[ M03 ] * Val[ M10 ] * Val[ M32 ] ) )
//                    + ( Val[ M00 ] * Val[ M13 ] * Val[ M32 ] )
//                    + ( Val[ M02 ] * Val[ M10 ] * Val[ M33 ] ) )
//                    - ( Val[ M00 ] * Val[ M12 ] * Val[ M33 ] );
//
//        var m13 = ( ( ( ( Val[ M02 ] * Val[ M13 ] * Val[ M20 ] )
//                      - ( Val[ M03 ] * Val[ M12 ] * Val[ M20 ] ) )
//                      + ( Val[ M03 ] * Val[ M10 ] * Val[ M22 ] ) )
//                      - ( Val[ M00 ] * Val[ M13 ] * Val[ M22 ] )
//                      - ( Val[ M02 ] * Val[ M10 ] * Val[ M23 ] ) )
//                      + ( Val[ M00 ] * Val[ M12 ] * Val[ M23 ] );
//
//        var m20 = ( ( ( ( Val[ M11 ] * Val[ M23 ] * Val[ M30 ] )
//                      - ( Val[ M13 ] * Val[ M21 ] * Val[ M30 ] ) )
//                      + ( Val[ M13 ] * Val[ M20 ] * Val[ M31 ] ) )
//                      - ( Val[ M10 ] * Val[ M23 ] * Val[ M31 ] )
//                      - ( Val[ M11 ] * Val[ M20 ] * Val[ M33 ] ) )
//                      + ( Val[ M10 ] * Val[ M21 ] * Val[ M33 ] );
//
//        var m21 = ( ( ( Val[ M03 ] * Val[ M21 ] * Val[ M30 ] )
//                    - ( Val[ M01 ] * Val[ M23 ] * Val[ M30 ] )
//                    - ( Val[ M03 ] * Val[ M20 ] * Val[ M31 ] ) )
//                    + ( Val[ M00 ] * Val[ M23 ] * Val[ M31 ] )
//                    + ( Val[ M01 ] * Val[ M20 ] * Val[ M33 ] ) )
//                    - ( Val[ M00 ] * Val[ M21 ] * Val[ M33 ] );
//
//        var m22 = ( ( ( ( Val[ M01 ] * Val[ M13 ] * Val[ M30 ] )
//                      - ( Val[ M03 ] * Val[ M11 ] * Val[ M30 ] ) )
//                      + ( Val[ M03 ] * Val[ M10 ] * Val[ M31 ] ) )
//                      - ( Val[ M00 ] * Val[ M13 ] * Val[ M31 ] )
//                      - ( Val[ M01 ] * Val[ M10 ] * Val[ M33 ] ) )
//                      + ( Val[ M00 ] * Val[ M11 ] * Val[ M33 ] );
//
//        var m23 = ( ( ( Val[ M03 ] * Val[ M11 ] * Val[ M20 ] )
//                    - ( Val[ M01 ] * Val[ M13 ] * Val[ M20 ] )
//                    - ( Val[ M03 ] * Val[ M10 ] * Val[ M21 ] ) )
//                    + ( Val[ M00 ] * Val[ M13 ] * Val[ M21 ] )
//                    + ( Val[ M01 ] * Val[ M10 ] * Val[ M23 ] ) )
//                    - ( Val[ M00 ] * Val[ M11 ] * Val[ M23 ] );
//
//        var m30 = ( ( ( Val[ M12 ] * Val[ M21 ] * Val[ M30 ] )
//                    - ( Val[ M11 ] * Val[ M22 ] * Val[ M30 ] )
//                    - ( Val[ M12 ] * Val[ M20 ] * Val[ M31 ] ) )
//                    + ( Val[ M10 ] * Val[ M22 ] * Val[ M31 ] )
//                    + ( Val[ M11 ] * Val[ M20 ] * Val[ M32 ] ) )
//                    - ( Val[ M10 ] * Val[ M21 ] * Val[ M32 ] );
//
//        var m31 = ( ( ( ( Val[ M01 ] * Val[ M22 ] * Val[ M30 ] )
//                      - ( Val[ M02 ] * Val[ M21 ] * Val[ M30 ] ) )
//                     + ( Val[ M02 ] * Val[ M20 ] * Val[ M31 ] ) )
//                     - ( Val[ M00 ] * Val[ M22 ] * Val[ M31 ] )
//                     - ( Val[ M01 ] * Val[ M20 ] * Val[ M32 ] ) )
//                     + ( Val[ M00 ] * Val[ M21 ] * Val[ M32 ] );
//
//        var m32 = ( ( ( Val[ M02 ] * Val[ M11 ] * Val[ M30 ] )
//                    - ( Val[ M01 ] * Val[ M12 ] * Val[ M30 ] )
//                    - ( Val[ M02 ] * Val[ M10 ] * Val[ M31 ] ) )
//                    + ( Val[ M00 ] * Val[ M12 ] * Val[ M31 ] )
//                    + ( Val[ M01 ] * Val[ M10 ] * Val[ M32 ] ) )
//                    - ( Val[ M00 ] * Val[ M11 ] * Val[ M32 ] );
//
//        var m33 = ( ( ( ( Val[ M01 ] * Val[ M12 ] * Val[ M20 ] )
//                      - ( Val[ M02 ] * Val[ M11 ] * Val[ M20 ] ) )
//                      + ( Val[ M02 ] * Val[ M10 ] * Val[ M21 ] ) )
//                      - ( Val[ M00 ] * Val[ M12 ] * Val[ M21 ] )
//                      - ( Val[ M01 ] * Val[ M10 ] * Val[ M22 ] ) )
//                      + ( Val[ M00 ] * Val[ M11 ] * Val[ M22 ] );
//        //@formatter:on
//
//        var invDet = 1.0f / lDet;
//
//        Val[ M00 ] = m00 * invDet;
//        Val[ M10 ] = m10 * invDet;
//        Val[ M20 ] = m20 * invDet;
//        Val[ M30 ] = m30 * invDet;
//        Val[ M01 ] = m01 * invDet;
//        Val[ M11 ] = m11 * invDet;
//        Val[ M21 ] = m21 * invDet;
//        Val[ M31 ] = m31 * invDet;
//        Val[ M02 ] = m02 * invDet;
//        Val[ M12 ] = m12 * invDet;
//        Val[ M22 ] = m22 * invDet;
//        Val[ M32 ] = m32 * invDet;
//        Val[ M03 ] = m03 * invDet;
//        Val[ M13 ] = m13 * invDet;
//        Val[ M23 ] = m23 * invDet;
//        Val[ M33 ] = m33 * invDet;
//
//        return this;
    }

    public float Determinant()
    {
//        //@formatter:off
//        return ( ( ( ( ( ( ( ( ( ( (
//                  ( Val[ M30 ] * Val[ M21 ] * Val[ M12 ] * Val[ M03 ] )
//                - ( Val[ M20 ] * Val[ M31 ] * Val[ M12 ] * Val[ M03 ] )
//                - ( Val[ M30 ] * Val[ M11 ] * Val[ M22 ] * Val[ M03 ] ) )
//                + ( Val[ M10 ] * Val[ M31 ] * Val[ M22 ] * Val[ M03 ] )
//                + ( Val[ M20 ] * Val[ M11 ] * Val[ M32 ] * Val[ M03 ] ) )
//                - ( Val[ M10 ] * Val[ M21 ] * Val[ M32 ] * Val[ M03 ] )
//                - ( Val[ M30 ] * Val[ M21 ] * Val[ M02 ] * Val[ M13 ] ) )
//                + ( Val[ M20 ] * Val[ M31 ] * Val[ M02 ] * Val[ M13 ] )
//                + ( Val[ M30 ] * Val[ M01 ] * Val[ M22 ] * Val[ M13 ] ) )
//                - ( Val[ M00 ] * Val[ M31 ] * Val[ M22 ] * Val[ M13 ] )
//                - ( Val[ M20 ] * Val[ M01 ] * Val[ M32 ] * Val[ M13 ] ) )
//                + ( Val[ M00 ] * Val[ M21 ] * Val[ M32 ] * Val[ M13 ] )
//                + ( Val[ M30 ] * Val[ M11 ] * Val[ M02 ] * Val[ M23 ] ) )
//                - ( Val[ M10 ] * Val[ M31 ] * Val[ M02 ] * Val[ M23 ] )
//                - ( Val[ M30 ] * Val[ M01 ] * Val[ M12 ] * Val[ M23 ] ) )
//                + ( Val[ M00 ] * Val[ M31 ] * Val[ M12 ] * Val[ M23 ] )
//                + ( Val[ M10 ] * Val[ M01 ] * Val[ M32 ] * Val[ M23 ] ) )
//                - ( Val[ M00 ] * Val[ M11 ] * Val[ M32 ] * Val[ M23 ] )
//                - ( Val[ M20 ] * Val[ M11 ] * Val[ M02 ] * Val[ M33 ] ) )
//                + ( Val[ M10 ] * Val[ M21 ] * Val[ M02 ] * Val[ M33 ] )
//                + ( Val[ M20 ] * Val[ M01 ] * Val[ M12 ] * Val[ M33 ] ) )
//                - ( Val[ M00 ] * Val[ M21 ] * Val[ M12 ] * Val[ M33 ] )
//                - ( Val[ M10 ] * Val[ M01 ] * Val[ M22 ] * Val[ M33 ] ) )
//                + ( Val[ M00 ] * Val[ M11 ] * Val[ M22 ] * Val[ M33 ] );
//        //@formatter:on
    }

    /// <summary>
    /// Calculates the determinant of the upper-left 3x3 submatrix of the 4x4 matrix.
    /// </summary>
    /// <returns>The determinant of the 3x3 submatrix.</returns>
    public float Determinant3X3()
    {
        var aPrime = Val[ M00 ];
        var bPrime = Val[ M01 ];
        var cPrime = Val[ M02 ];
        var dPrime = Val[ M10 ];
        var ePrime = Val[ M11 ];
        var fPrime = Val[ M12 ];
        var gPrime = Val[ M20 ];
        var hPrime = Val[ M21 ];
        var iPrime = Val[ M22 ];

        var determinant = ( ( aPrime * ( ( ePrime * iPrime ) - ( fPrime * hPrime ) ) )
                          - ( bPrime * ( ( dPrime * iPrime ) - ( fPrime * gPrime ) ) ) )
                          + ( cPrime * ( ( dPrime * hPrime ) - ( ePrime * gPrime ) ) );

        return determinant; 
    }

    /// <summary>
    /// Sets the matrix to a projection matrix with a near- and far plane, a field
    /// of view in degrees and an aspect ratio. Note that the field of view specified
    /// is the angle in degrees for the height, the field of view for the width will
    /// be calculated according to the aspect ratio.
    /// </summary>
    /// <param name="near"> The near plane </param>
    /// <param name="far"> The far plane </param>
    /// <param name="fovy"> The field of view of the height in degrees </param>
    /// <param name="aspectRatio"> The "width over height" aspect ratio </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToProjection( float near, float far, float fovy, float aspectRatio )
    {
//        ToIdentity();
//
//        var lFd = ( float )( 1.0 / Math.Tan( ( fovy * ( Math.PI / 180 ) ) / 2.0 ) );
//        var lA1 = ( far + near ) / ( near - far );
//        var lA2 = ( 2 * far * near ) / ( near - far );
//
//        Val[ M00 ] = lFd / aspectRatio;
//        Val[ M10 ] = 0;
//        Val[ M20 ] = 0;
//        Val[ M30 ] = 0;
//        Val[ M01 ] = 0;
//        Val[ M11 ] = lFd;
//        Val[ M21 ] = 0;
//        Val[ M31 ] = 0;
//        Val[ M02 ] = 0;
//        Val[ M12 ] = 0;
//        Val[ M22 ] = lA1;
//        Val[ M32 ] = -1;
//        Val[ M03 ] = 0;
//        Val[ M13 ] = 0;
//        Val[ M23 ] = lA2;
//        Val[ M33 ] = 0;
//
//        return this;
    }

    /// <summary>
    /// Sets the matrix to a projection matrix with a near/far plane, and left, bottom,
    /// right and top specifying the points on the near plane that are mapped to the lower
    /// left and upper right corners of the viewport. This allows to create projection
    /// matrix with off-center vanishing point.
    /// </summary>
    /// <param name="left"> </param>
    /// <param name="right"> </param>
    /// <param name="bottom"> </param>
    /// <param name="top"> </param>
    /// <param name="near"> The near plane </param>
    /// <param name="far"> The far plane </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToProjection( float left, float right, float bottom, float top, float near, float far )
    {
//        var x   = ( 2.0f * near ) / ( right - left );
//        var y   = ( 2.0f * near ) / ( top - bottom );
//        var a   = ( right + left ) / ( right - left );
//        var b   = ( top + bottom ) / ( top - bottom );
//        var lA1 = ( far + near ) / ( near - far );
//        var lA2 = ( 2 * far * near ) / ( near - far );
//
//        Val[ M00 ] = x;
//        Val[ M10 ] = 0;
//        Val[ M20 ] = 0;
//        Val[ M30 ] = 0;
//        Val[ M01 ] = 0;
//        Val[ M11 ] = y;
//        Val[ M21 ] = 0;
//        Val[ M31 ] = 0;
//        Val[ M02 ] = a;
//        Val[ M12 ] = b;
//        Val[ M22 ] = lA1;
//        Val[ M32 ] = -1;
//        Val[ M03 ] = 0;
//        Val[ M13 ] = 0;
//        Val[ M23 ] = lA2;
//        Val[ M33 ] = 0;
//
//        return this;
    }

    /// <summary>
    /// Sets this matrix to an orthographic projection matrix with the origin at (x,y) extending by
    /// width and height. The near plane is Set to 0, the far plane is Set to 1.
    /// </summary>
    /// <param name="x"> The x-coordinate of the origin </param>
    /// <param name="y"> The y-coordinate of the origin </param>
    /// <param name="width"> The width </param>
    /// <param name="height"> The height </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToOrtho2D( float x, float y, float width, float height )
    {
        SetToOrtho( x, x + width, y, y + height, 0f, 1f );

        return this;
    }

    /// <summary>
    /// Sets this matrix to an orthographic projection matrix with the origin at (x,y) extending
    /// by width and height, having a near and far plane.
    /// </summary>
    /// <param name="x"> The x-coordinate of the origin </param>
    /// <param name="y"> The y-coordinate of the origin </param>
    /// <param name="width"> The width </param>
    /// <param name="height"> The height </param>
    /// <param name="near"> The near plane </param>
    /// <param name="far"> The far plane </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToOrtho2D( float x, float y, float width, float height, float near, float far )
    {
        SetToOrtho( x, x + width, y, y + height, near, far );

        return this;
    }

    /// <summary>
    /// Sets the matrix to an orthographic projection like glOrtho (http://www.opengl.org/sdk/docs/man/xhtml/glOrtho.xml)
    /// following the OpenGL equivalent
    /// </summary>
    /// <param name="left"> The left clipping plane </param>
    /// <param name="right"> The right clipping plane </param>
    /// <param name="bottom"> The bottom clipping plane </param>
    /// <param name="top"> The top clipping plane </param>
    /// <param name="near"> The near clipping plane </param>
    /// <param name="far"> The far clipping plane </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToOrtho( float left, float right, float bottom, float top, float near, float far )
    {
        if ( ( right - left ) == 0 )
        {
            Logger.Debug( $"left: {left}, right: {right}, bottom: {bottom}, top: {top}, near: {near}, far: {far}" );

            throw new ArgumentException( "Right and left cannot be equal." );
        }

        if ( ( top - bottom ) == 0 )
        {
            Logger.Debug( $"left: {left}, right: {right}, bottom: {bottom}, top: {top}, near: {near}, far: {far}" );

            throw new ArgumentException( "Top and bottom cannot be equal." );
        }

        if ( ( far - near ) == 0 )
        {
            Logger.Debug( $"left: {left}, right: {right}, bottom: {bottom}, top: {top}, near: {near}, far: {far}" );

            throw new ArgumentException( "Far and near cannot be equal." );
        }

//        var xOrth = 2f / ( right - left );
//        var yOrth = 2f / ( top - bottom );
//        var zOrth = 2f / ( far - near );
//
//        var tx = -( right + left ) / ( right - left );
//        var ty = -( top + bottom ) / ( top - bottom );
//        var tz = -( far + near ) / ( far - near );
//
//        Val[ M00 ] = xOrth;
//        Val[ M10 ] = 0;
//        Val[ M20 ] = 0;
//        Val[ M30 ] = 0;
//
//        // ------------------
//        Val[ M01 ] = 0;
//        Val[ M11 ] = yOrth;
//        Val[ M21 ] = 0;
//        Val[ M31 ] = 0;
//
//        // ------------------
//        Val[ M02 ] = 0;
//        Val[ M12 ] = 0;
//        Val[ M22 ] = zOrth;
//        Val[ M32 ] = 0;
//
//        // ------------------
//        Val[ M03 ] = tx;
//        Val[ M13 ] = ty;
//        Val[ M23 ] = tz;
//        Val[ M33 ] = 1;
//
//        return this;
    }

    /// <summary>
    /// Sets the 4th column to the translation vector.
    /// </summary>
    /// <param name="vector"> The translation vector </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetTranslation( Vector3 vector )
    {
//        Val[ M03 ] = vector.X;
//        Val[ M13 ] = vector.Y;
//        Val[ M23 ] = vector.Z;
//
//        return this;
    }

    /// <summary>
    /// Sets the 4th column to the translation vector.
    /// </summary>
    /// <param name="x"> The X coordinate of the translation vector </param>
    /// <param name="y"> The Y coordinate of the translation vector </param>
    /// <param name="z"> The Z coordinate of the translation vector </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetTranslation( float x, float y, float z )
    {
//        Val[ M03 ] = x;
//        Val[ M13 ] = y;
//        Val[ M23 ] = z;
//
//        return this;
    }

    /// <summary>
    /// Sets this matrix to a translation matrix, overwriting it first by an identity matrix
    /// and then setting the 4th column to the translation vector.
    /// </summary>
    /// <param name="vector"> The translation vector </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToTranslation( Vector3 vector )
    {
//        ToIdentity();
//
//        Val[ M03 ] = vector.X;
//        Val[ M13 ] = vector.Y;
//        Val[ M23 ] = vector.Z;
//
//        return this;
    }

    /// <summary>
    /// Sets this matrix to a translation matrix, overwriting it first by an identity matrix and then setting the 4th
    /// column to the
    /// translation vector.
    /// </summary>
    /// <param name="x"> The x-component of the translation vector. </param>
    /// <param name="y"> The y-component of the translation vector. </param>
    /// <param name="z"> The z-component of the translation vector. </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToTranslation( float x, float y, float z )
    {
//        ToIdentity();
//
//        Val[ M03 ] = x;
//        Val[ M13 ] = y;
//        Val[ M23 ] = z;
//
//        return this;
    }

    /// <summary>
    /// Sets this matrix to a translation and scaling matrix by first overwriting it with an identity and then setting the
    /// translation vector in the 4th column and the scaling vector in the diagonal.
    /// </summary>
    /// <param name="translation"> The translation vector </param>
    /// <param name="scaling"> The scaling vector </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToTranslationAndScaling( Vector3 translation, Vector3 scaling )
    {
//        ToIdentity();
//
//        Val[ M03 ] = translation.X;
//        Val[ M13 ] = translation.Y;
//        Val[ M23 ] = translation.Z;
//        Val[ M00 ] = scaling.X;
//        Val[ M11 ] = scaling.Y;
//        Val[ M22 ] = scaling.Z;
//
//        return this;
    }

    /// <summary>
    /// Sets this matrix to a translation and scaling matrix by first overwriting it with an identity and then setting the
    /// translation vector in the 4th column and the scaling vector in the diagonal.
    /// </summary>
    /// <param name="translationX"> The x-component of the translation vector </param>
    /// <param name="translationY"> The y-component of the translation vector </param>
    /// <param name="translationZ"> The z-component of the translation vector </param>
    /// <param name="scalingX"> The x-component of the scaling vector </param>
    /// <param name="scalingY"> The x-component of the scaling vector </param>
    /// <param name="scalingZ"> The x-component of the scaling vector </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToTranslationAndScaling( float translationX,
                                               float translationY,
                                               float translationZ,
                                               float scalingX,
                                               float scalingY,
                                               float scalingZ )
    {
//        ToIdentity();
//
//        Val[ M03 ] = translationX;
//        Val[ M13 ] = translationY;
//        Val[ M23 ] = translationZ;
//        Val[ M00 ] = scalingX;
//        Val[ M11 ] = scalingY;
//        Val[ M22 ] = scalingZ;
//
//        return this;
    }

    /// <summary>
    /// Sets the matrix to a rotation matrix around the given axis.
    /// </summary>
    /// <param name="axis"> The axis </param>
    /// <param name="degrees"> The angle in degrees </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToRotation( Vector3 axis, float degrees )
    {
        if ( degrees == 0 )
        {
            ToIdentity();

            return this;
        }

        return Set( Quat.SetFromAxis( axis, degrees ) );
    }

    /// <summary>
    /// Sets the matrix to a rotation matrix around the given axis.
    /// </summary>
    /// <param name="axis"> The axis </param>
    /// <param name="radians"> The angle in radians </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToRotationRad( Vector3 axis, float radians )
    {
        if ( radians == 0 )
        {
            ToIdentity();

            return this;
        }

        return Set( Quat.SetFromAxisRad( axis, radians ) );
    }

    /// <summary>
    /// Sets the matrix to a rotation matrix around the given axis.
    /// </summary>
    /// <param name="axisX"> The x-component of the axis </param>
    /// <param name="axisY"> The y-component of the axis </param>
    /// <param name="axisZ"> The z-component of the axis </param>
    /// <param name="degrees"> The angle in degrees </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToRotation( float axisX, float axisY, float axisZ, float degrees )
    {
        if ( degrees == 0 )
        {
            ToIdentity();

            return this;
        }

        return Set( Quat.SetFromAxis( axisX, axisY, axisZ, degrees ) );
    }

    /// <summary>
    /// Sets the matrix to a rotation matrix around the given axis.
    /// </summary>
    /// <param name="axisX"> The x-component of the axis </param>
    /// <param name="axisY"> The y-component of the axis </param>
    /// <param name="axisZ"> The z-component of the axis </param>
    /// <param name="radians"> The angle in radians </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToRotationRad( float axisX, float axisY, float axisZ, float radians )
    {
        if ( radians == 0 )
        {
            ToIdentity();

            return this;
        }

        return Set( Quat.SetFromAxisRad( axisX, axisY, axisZ, radians ) );
    }

    /// <summary>
    /// Set the matrix to a rotation matrix between two vectors.
    /// </summary>
    /// <param name="v1"> The base vector </param>
    /// <param name="v2"> The target vector </param>
    /// <returns> This matrix for the purpose of chaining methods together  </returns>
    public Matrix4 SetToRotation( Vector3 v1, Vector3 v2 )
    {
        return Set( Quat.SetFromCross( v1, v2 ) );
    }

    /// <summary>
    /// Set the matrix to a rotation matrix between two vectors.
    /// </summary>
    /// <param name="x1"> The base vectors x value </param>
    /// <param name="y1"> The base vectors y value </param>
    /// <param name="z1"> The base vectors z value </param>
    /// <param name="x2"> The target vector x value </param>
    /// <param name="y2"> The target vector y value </param>
    /// <param name="z2"> The target vector z value </param>
    /// <returns> This matrix for the purpose of chaining methods together  </returns>
    public Matrix4 SetToRotation( float x1, float y1, float z1, float x2, float y2, float z2 )
    {
        return Set( Quat.SetFromCross( x1, y1, z1, x2, y2, z2 ) );
    }

    /// <summary>
    /// Sets this matrix to a rotation matrix from the given euler angles.
    /// </summary>
    /// <param name="yaw"> the yaw in degrees </param>
    /// <param name="pitch"> the pitch in degrees </param>
    /// <param name="roll"> the roll in degrees </param>
    /// <returns> This matrix  </returns>
    public Matrix4 SetFromEulerAngles( float yaw, float pitch, float roll )
    {
        Quat.SetEulerAngles( yaw, pitch, roll );

        return Set( Quat );
    }

    /// <summary>
    /// Sets this matrix to a rotation matrix from the given euler angles.
    /// </summary>
    /// <param name="yaw"> the yaw in radians </param>
    /// <param name="pitch"> the pitch in radians </param>
    /// <param name="roll"> the roll in radians </param>
    /// <returns> This matrix  </returns>
    public Matrix4 SetFromEulerAnglesRad( float yaw, float pitch, float roll )
    {
        Quat.SetEulerAnglesRad( yaw, pitch, roll );

        return Set( Quat );
    }

    /// <summary>
    /// Sets this matrix to a scaling matrix
    /// </summary>
    /// <param name="vector"> The scaling vector </param>
    /// <returns> This matrix for chaining.  </returns>
    public Matrix4 SetToScaling( Vector3 vector )
    {
//        ToIdentity();
//
//        Val[ M00 ] = vector.X;
//        Val[ M11 ] = vector.Y;
//        Val[ M22 ] = vector.Z;
//
//        return this;
    }

    /// <summary>
    /// Sets this matrix to a scaling matrix
    /// </summary>
    /// <param name="x"> The x-component of the scaling vector </param>
    /// <param name="y"> The y-component of the scaling vector </param>
    /// <param name="z"> The z-component of the scaling vector </param>
    /// <returns> This matrix for chaining.  </returns>
    public Matrix4 SetToScaling( float x, float y, float z )
    {
//        ToIdentity();
//
//        Val[ M00 ] = x;
//        Val[ M11 ] = y;
//        Val[ M22 ] = z;
//
//        return this;
    }

    /// <summary>
    /// Sets the matrix to a look at matrix with a direction and an up vector. Multiply
    /// with a translation matrix to get a camera model view matrix.
    /// </summary>
    /// <param name="direction"> The direction vector </param>
    /// <param name="up"> The up vector </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToLookAt( Vector3 direction, Vector3 up )
    {
//        LVez.Set( direction ).Nor();
//        LVex.Set( direction ).Crs( up ).Nor();
//        LVey.Set( LVex ).Crs( LVez ).Nor();
//
//        ToIdentity();
//
//        Val[ M00 ] = LVex.X;
//        Val[ M01 ] = LVex.Y;
//        Val[ M02 ] = LVex.Z;
//        Val[ M10 ] = LVey.X;
//        Val[ M11 ] = LVey.Y;
//        Val[ M12 ] = LVey.Z;
//        Val[ M20 ] = -LVez.X;
//        Val[ M21 ] = -LVez.Y;
//        Val[ M22 ] = -LVez.Z;
//
//        return this;
    }

    /// <summary>
    /// Sets this matrix to a look at matrix with the given position, target and up vector.
    /// </summary>
    /// <param name="position"> the position </param>
    /// <param name="target"> the target </param>
    /// <param name="up"> the up vector </param>
    /// <returns> This matrix  </returns>
    public Matrix4 SetToLookAt( Vector3 position, Vector3 target, Vector3 up )
    {
        TmpVec.Set( target ).Sub( position );
        SetToLookAt( TmpVec, up );
        Mul( TmpMat.SetToTranslation( -position.X, -position.Y, -position.Z ) );

        return this;
    }

    public Matrix4 SetToWorld( Vector3 position, Vector3 forward, Vector3 up )
    {
        TmpForward.Set( forward ).Nor();
        Right.Set( TmpForward ).Crs( up ).Nor();
        TmpUp.Set( Right ).Crs( TmpForward ).Nor();
        Set( Right, TmpUp, TmpForward.Scale( -1 ), position );

        return this;
    }

    /// <summary>
    /// Linearly interpolates between this matrix and the given matrix mixing by alpha
    /// </summary>
    /// <param name="matrix"> the matrix </param>
    /// <param name="alpha"> the alpha value in the range [0,1] </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Lerp( Matrix4 matrix, float alpha )
    {
        for ( var i = 0; i < 16; i++ )
        {
            Val[ i ] = ( Val[ i ] * ( 1 - alpha ) ) + ( matrix.Val[ i ] * alpha );
        }

        return this;
    }

    /// <summary>
    /// Averages the given transform with this one and stores the result in this matrix. Translations and scales are lerped
    /// while
    /// rotations are slerped.
    /// </summary>
    /// <param name="other"> The other transform </param>
    /// <param name="w"> Weight of this transform; weight of the other transform is (1 - w) </param>
    /// <returns> This matrix for chaining  </returns>
    public Matrix4 Average( Matrix4 other, float w )
    {
        GetScale( TmpVec );
        other.GetScale( TmpForward );

        GetRotation( Quat );
        other.GetRotation( Quat2 );

        GetTranslation( TmpUp );
        other.GetTranslation( Right );

        SetToScaling( TmpVec.Scale( w ).Add( TmpForward.Scale( 1 - w ) ) );
        Rotate( Quat.Slerp( Quat2, 1 - w ) );
        SetTranslation( TmpUp.Scale( w ).Add( Right.Scale( 1 - w ) ) );

        return this;
    }

    /// <summary>
    /// Averages the given transforms and stores the result in this matrix. Translations
    /// and scales are lerped while rotations are slerped. Does not destroy the data
    /// contained in t.
    /// </summary>
    /// <param name="t"> List of transforms </param>
    /// <returns> This matrix for chaining  </returns>
    public Matrix4 Average( Matrix4[] t )
    {
        var w = 1.0f / t.Length;

        TmpVec.Set( t[ 0 ].GetScale( TmpUp ).Scale( w ) );
        Quat.Set( t[ 0 ].GetRotation( Quat2 ).Exp( w ) );
        TmpForward.Set( t[ 0 ].GetTranslation( TmpUp ).Scale( w ) );

        for ( var i = 1; i < t.Length; i++ )
        {
            TmpVec.Add( t[ i ].GetScale( TmpUp ).Scale( w ) );
            Quat.Mul( t[ i ].GetRotation( Quat2 ).Exp( w ) );
            TmpForward.Add( t[ i ].GetTranslation( TmpUp ).Scale( w ) );
        }

        Quat.Nor();

        SetToScaling( TmpVec );
        Rotate( Quat );
        SetTranslation( TmpForward );

        return this;
    }

    /// <summary>
    /// Averages the given transforms with the given weights and stores the result
    /// in this matrix. Translations and scales are lerped while rotations are slerped.
    /// Does not destroy the data contained in t or w; Sum of w_i must be equal to 1, or
    /// unexpected results will occur.
    /// </summary>
    /// <param name="t"> List of transforms </param>
    /// <param name="w"> List of weights </param>
    /// <returns> This matrix for chaining  </returns>
    public Matrix4 Average( Matrix4[] t, float[] w )
    {
        TmpVec.Set( t[ 0 ].GetScale( TmpUp ).Scale( w[ 0 ] ) );
        Quat.Set( t[ 0 ].GetRotation( Quat2 ).Exp( w[ 0 ] ) );
        TmpForward.Set( t[ 0 ].GetTranslation( TmpUp ).Scale( w[ 0 ] ) );

        for ( var i = 1; i < t.Length; i++ )
        {
            TmpVec.Add( t[ i ].GetScale( TmpUp ).Scale( w[ i ] ) );
            Quat.Mul( t[ i ].GetRotation( Quat2 ).Exp( w[ i ] ) );
            TmpForward.Add( t[ i ].GetTranslation( TmpUp ).Scale( w[ i ] ) );
        }

        Quat.Nor();

        SetToScaling( TmpVec );
        Rotate( Quat );
        SetTranslation( TmpForward );

        return this;
    }

    /// <summary>
    /// Sets this matrix to the given 3x3 matrix.
    /// The third column of this matrix is Set to ( 0, 0, 1, 0 ).
    /// </summary>
    /// <param name="mat"> the matrix </param>
    public Matrix4 Set( Matrix3 mat )
    {
//        Val[ 0 ]  = mat.Val[ 0 ];
//        Val[ 1 ]  = mat.Val[ 1 ];
//        Val[ 2 ]  = mat.Val[ 2 ];
//        Val[ 3 ]  = 0;
//        Val[ 4 ]  = mat.Val[ 3 ];
//        Val[ 5 ]  = mat.Val[ 4 ];
//        Val[ 6 ]  = mat.Val[ 5 ];
//        Val[ 7 ]  = 0;
//        Val[ 8 ]  = 0;
//        Val[ 9 ]  = 0;
//        Val[ 10 ] = 1;
//        Val[ 11 ] = 0;
//        Val[ 12 ] = mat.Val[ 6 ];
//        Val[ 13 ] = mat.Val[ 7 ];
//        Val[ 14 ] = 0;
//        Val[ 15 ] = mat.Val[ 8 ];
//
//        return this;
    }

    /// <summary>
    /// Sets this matrix to the given affine matrix. The values are mapped as follows:
    /// <para></para>
    /// <para>     [  M00  M01  ___  M02  ]</para>
    /// <para>     [  M10  M11  ___  M12  ]</para>
    /// <para>     [  ___  ___  ___  ___  ]</para>
    /// <para>     [  ___  ___  ___  ___  ]</para>
    /// <para></para>
    /// </summary>
    /// <param name="affine"> the source matrix </param>
    /// <returns> This matrix for chaining </returns>
    public Matrix4 Set( Affine2 affine )
    {
//        Val[ M00 ] = affine.M00;
//        Val[ M10 ] = affine.M10;
//        Val[ M20 ] = 0;
//        Val[ M30 ] = 0;
//        Val[ M01 ] = affine.M01;
//        Val[ M11 ] = affine.M11;
//        Val[ M21 ] = 0;
//        Val[ M31 ] = 0;
//        Val[ M02 ] = 0;
//        Val[ M12 ] = 0;
//        Val[ M22 ] = 1;
//        Val[ M32 ] = 0;
//        Val[ M03 ] = affine.M02;
//        Val[ M13 ] = affine.M12;
//        Val[ M23 ] = 0;
//        Val[ M33 ] = 1;
//
//        return this;
    }

    /// <summary>
    /// Assumes that both matrices are 2D affine transformations, copying only
    /// the relevant components. The copied are mapped as follows:
    /// <para></para>
    /// <para>     [  M00  M01  ___  M02  ]</para>
    /// <para>     [  M10  M11  ___  M12  ]</para>
    /// <para>     [  ___  ___  ___  ___  ]</para>
    /// <para>     [  ___  ___  ___  ___  ]</para>
    /// <para></para>
    /// </summary>
    /// <param name="affine"> the source matrix </param>
    /// <returns> This matrix for chaining </returns>
    public Matrix4 SetAsAffine( Affine2 affine )
    {
//        Val[ M00 ] = affine.M00;
//        Val[ M10 ] = affine.M10;
//        Val[ M01 ] = affine.M01;
//        Val[ M11 ] = affine.M11;
//        Val[ M03 ] = affine.M02;
//        Val[ M13 ] = affine.M12;
//
//        return this;
    }

    /// <summary>
    /// Assumes that both matrices are 2D affine transformations, copying only
    /// the relevant components. The copied values are:
    /// <para></para>
    /// <para>     [  M00  M01  ___  M03  ]</para>
    /// <para>     [  M10  M11  ___  M13  ]</para>
    /// <para>     [  ___  ___  ___  ___  ]</para>
    /// <para>     [  ___  ___  ___  ___  ]</para>
    /// <para></para>
    /// </summary>
    /// <param name="mat"> the source matrix </param>
    /// <returns> This matrix for chaining </returns>
    public Matrix4 SetAsAffine( Matrix4 mat )
    {
//        Val[ M00 ] = mat.Val[ M00 ];
//        Val[ M10 ] = mat.Val[ M10 ];
//        Val[ M01 ] = mat.Val[ M01 ];
//        Val[ M11 ] = mat.Val[ M11 ];
//        Val[ M03 ] = mat.Val[ M03 ];
//        Val[ M13 ] = mat.Val[ M13 ];
//
//        return this;
    }

    public Matrix4 Scl( Vector3 scale )
    {
//        Val[ M00 ] *= scale.X;
//        Val[ M11 ] *= scale.Y;
//        Val[ M22 ] *= scale.Z;
//
//        return this;
    }

    public Matrix4 Scl( float x, float y, float z )
    {
//        Val[ M00 ] *= x;
//        Val[ M11 ] *= y;
//        Val[ M22 ] *= z;
//
//        return this;
    }

    public Matrix4 Scl( float scale )
    {
//        Val[ M00 ] *= scale;
//        Val[ M11 ] *= scale;
//        Val[ M22 ] *= scale;
//
//        return this;
    }

    public Vector3 GetTranslation( Vector3 position )
    {
//        position.X = Val[ M03 ];
//        position.Y = Val[ M13 ];
//        position.Z = Val[ M23 ];
//
//        return position;
    }

    /// <summary>
    /// Gets the rotation of this matrix.
    /// </summary>
    /// <param name="rotation"> The <see cref="Quaternion" /> to receive the rotation </param>
    /// <param name="normalizeAxes"> True to normalize the axes, necessary when the matrix might also include scaling. </param>
    /// <returns> The provided <see cref="Quaternion" /> for chaining.  </returns>
    public Quaternion GetRotation( Quaternion rotation, bool normalizeAxes )
    {
        return rotation.SetFromMatrix( normalizeAxes, this );
    }

    /// <summary>
    /// Gets the rotation of this matrix.
    /// </summary>
    /// <param name="rotation"> The <see cref="Quaternion" /> to receive the rotation </param>
    /// <returns> The provided <see cref="Quaternion" /> for chaining.  </returns>
    public Quaternion GetRotation( Quaternion rotation )
    {
        return rotation.SetFromMatrix( this );
    }

    /// <summary>
    /// </summary>
    /// <returns> the squared scale factor on the X axis </returns>
    public float GetScaleXSquared()
    {
//        return ( Val[ M00 ] * Val[ M00 ] ) + ( Val[ M01 ] * Val[ M01 ] ) + ( Val[ M02 ] * Val[ M02 ] );
    }

    /// <summary>
    /// </summary>
    /// <returns> the squared scale factor on the Y axis </returns>
    public float GetScaleYSquared()
    {
//        return ( Val[ M10 ] * Val[ M10 ] ) + ( Val[ M11 ] * Val[ M11 ] ) + ( Val[ M12 ] * Val[ M12 ] );
    }

    /// <summary>
    /// </summary>
    /// <returns> the squared scale factor on the Z axis </returns>
    public float GetScaleZSquared()
    {
//        return ( Val[ M20 ] * Val[ M20 ] ) + ( Val[ M21 ] * Val[ M21 ] ) + ( Val[ M22 ] * Val[ M22 ] );
    }

    /// <summary>
    /// </summary>
    /// <returns> the scale factor on the X axis (non-negative) </returns>
    public float GetScaleX()
    {
//        return MathUtils.IsZero( Val[ M01 ] ) && MathUtils.IsZero( Val[ M02 ] )
//            ? Math.Abs( Val[ M00 ] )
//            : ( float )Math.Sqrt( GetScaleXSquared() );
    }

    /// <summary>
    /// </summary>
    /// <returns> the scale factor on the Y axis (non-negative) </returns>
    public float GetScaleY()
    {
//        return MathUtils.IsZero( Val[ M10 ] ) && MathUtils.IsZero( Val[ M12 ] )
//            ? Math.Abs( Val[ M11 ] )
//            : ( float )Math.Sqrt( GetScaleYSquared() );
    }

    /// <summary>
    /// </summary>
    /// <returns> the scale factor on the Z axis (non-negative) </returns>
    public float GetScaleZ()
    {
//        return MathUtils.IsZero( Val[ M20 ] ) && MathUtils.IsZero( Val[ M21 ] )
//            ? Math.Abs( Val[ M22 ] )
//            : ( float )Math.Sqrt( GetScaleZSquared() );
    }

    /// <summary>
    /// <param name="scale"> The vector which will receive the (non-negative) scale components on each axis. </param>
    /// <returns> The provided vector for chaining.  </returns>
    /// </summary>
    public Vector3 GetScale( Vector3 scale )
    {
        return scale.Set( GetScaleX(), GetScaleY(), GetScaleZ() );
    }

    /// <summary>
    /// removes the translational part and transposes the matrix.
    /// </summary>
    public Matrix4 ToNormalMatrix()
    {
//        Val[ M03 ] = 0;
//        Val[ M13 ] = 0;
//        Val[ M23 ] = 0;
//
//        return Invert().Transpose();
    }

    /// <summary>
    /// Multiplies the matrix mata with matrix matb, storing the result in mata.
    /// The arrays are assumed to hold 4x4 column major matrices as you can get
    /// from <see cref="Val" />.
    /// This is the same as <see cref="Matrix4.Mul(Matrix4)" />.
    /// </summary>
    /// <param name="mata"> the first matrix. </param>
    /// <param name="matb"> the second matrix.  </param>
    public static void Mul( float[] mata, float[] matb )
    {
//        var m00 = ( mata[ M00 ] * matb[ M00 ] )
//                  + ( mata[ M01 ] * matb[ M10 ] )
//                  + ( mata[ M02 ] * matb[ M20 ] )
//                  + ( mata[ M03 ] * matb[ M30 ] );
//
//        var m01 = ( mata[ M00 ] * matb[ M01 ] )
//                  + ( mata[ M01 ] * matb[ M11 ] )
//                  + ( mata[ M02 ] * matb[ M21 ] )
//                  + ( mata[ M03 ] * matb[ M31 ] );
//
//        var m02 = ( mata[ M00 ] * matb[ M02 ] )
//                  + ( mata[ M01 ] * matb[ M12 ] )
//                  + ( mata[ M02 ] * matb[ M22 ] )
//                  + ( mata[ M03 ] * matb[ M32 ] );
//
//        var m03 = ( mata[ M00 ] * matb[ M03 ] )
//                  + ( mata[ M01 ] * matb[ M13 ] )
//                  + ( mata[ M02 ] * matb[ M23 ] )
//                  + ( mata[ M03 ] * matb[ M33 ] );
//
//        var m10 = ( mata[ M10 ] * matb[ M00 ] )
//                  + ( mata[ M11 ] * matb[ M10 ] )
//                  + ( mata[ M12 ] * matb[ M20 ] )
//                  + ( mata[ M13 ] * matb[ M30 ] );
//
//        var m11 = ( mata[ M10 ] * matb[ M01 ] )
//                  + ( mata[ M11 ] * matb[ M11 ] )
//                  + ( mata[ M12 ] * matb[ M21 ] )
//                  + ( mata[ M13 ] * matb[ M31 ] );
//
//        var m12 = ( mata[ M10 ] * matb[ M02 ] )
//                  + ( mata[ M11 ] * matb[ M12 ] )
//                  + ( mata[ M12 ] * matb[ M22 ] )
//                  + ( mata[ M13 ] * matb[ M32 ] );
//
//        var m13 = ( mata[ M10 ] * matb[ M03 ] )
//                  + ( mata[ M11 ] * matb[ M13 ] )
//                  + ( mata[ M12 ] * matb[ M23 ] )
//                  + ( mata[ M13 ] * matb[ M33 ] );
//
//        var m20 = ( mata[ M20 ] * matb[ M00 ] )
//                  + ( mata[ M21 ] * matb[ M10 ] )
//                  + ( mata[ M22 ] * matb[ M20 ] )
//                  + ( mata[ M23 ] * matb[ M30 ] );
//
//        var m21 = ( mata[ M20 ] * matb[ M01 ] )
//                  + ( mata[ M21 ] * matb[ M11 ] )
//                  + ( mata[ M22 ] * matb[ M21 ] )
//                  + ( mata[ M23 ] * matb[ M31 ] );
//
//        var m22 = ( mata[ M20 ] * matb[ M02 ] )
//                  + ( mata[ M21 ] * matb[ M12 ] )
//                  + ( mata[ M22 ] * matb[ M22 ] )
//                  + ( mata[ M23 ] * matb[ M32 ] );
//
//        var m23 = ( mata[ M20 ] * matb[ M03 ] )
//                  + ( mata[ M21 ] * matb[ M13 ] )
//                  + ( mata[ M22 ] * matb[ M23 ] )
//                  + ( mata[ M23 ] * matb[ M33 ] );
//
//        var m30 = ( mata[ M30 ] * matb[ M00 ] )
//                  + ( mata[ M31 ] * matb[ M10 ] )
//                  + ( mata[ M32 ] * matb[ M20 ] )
//                  + ( mata[ M33 ] * matb[ M30 ] );
//
//        var m31 = ( mata[ M30 ] * matb[ M01 ] )
//                  + ( mata[ M31 ] * matb[ M11 ] )
//                  + ( mata[ M32 ] * matb[ M21 ] )
//                  + ( mata[ M33 ] * matb[ M31 ] );
//
//        var m32 = ( mata[ M30 ] * matb[ M02 ] )
//                  + ( mata[ M31 ] * matb[ M12 ] )
//                  + ( mata[ M32 ] * matb[ M22 ] )
//                  + ( mata[ M33 ] * matb[ M32 ] );
//
//        var m33 = ( mata[ M30 ] * matb[ M03 ] )
//                  + ( mata[ M31 ] * matb[ M13 ] )
//                  + ( mata[ M32 ] * matb[ M23 ] )
//                  + ( mata[ M33 ] * matb[ M33 ] );
//
//        mata[ M00 ] = m00;
//        mata[ M10 ] = m10;
//        mata[ M20 ] = m20;
//        mata[ M30 ] = m30;
//
//        // ---------------
//        mata[ M01 ] = m01;
//        mata[ M11 ] = m11;
//        mata[ M21 ] = m21;
//        mata[ M31 ] = m31;
//
//        // ---------------
//        mata[ M02 ] = m02;
//        mata[ M12 ] = m12;
//        mata[ M22 ] = m22;
//        mata[ M32 ] = m32;
//
//        // ---------------
//        mata[ M03 ] = m03;
//        mata[ M13 ] = m13;
//        mata[ M23 ] = m23;
//        mata[ M33 ] = m33;
    }

    /// <summary>
    /// Multiplies the vector with the given matrix. The matrix array is assumed
    /// to hold a 4x4 column major matrix as you can get from <see cref="Val" />.
    /// The vector array is assumed to hold a 3-component vector, with x being the
    /// first element, y being the second and z being the last component. The result
    /// is stored in the vector array. This is the same as <see cref="Vector3.Mul(Matrix4)" />.
    /// </summary>
    /// <param name="mat"> the matrix </param>
    /// <param name="vec"> the vector.  </param>
    public static void MulVec( float[] mat, float[] vec )
    {
//        var x = ( vec[ 0 ] * mat[ M00 ] ) + ( vec[ 1 ] * mat[ M01 ] ) + ( vec[ 2 ] * mat[ M02 ] ) + mat[ M03 ];
//        var y = ( vec[ 0 ] * mat[ M10 ] ) + ( vec[ 1 ] * mat[ M11 ] ) + ( vec[ 2 ] * mat[ M12 ] ) + mat[ M13 ];
//        var z = ( vec[ 0 ] * mat[ M20 ] ) + ( vec[ 1 ] * mat[ M21 ] ) + ( vec[ 2 ] * mat[ M22 ] ) + mat[ M23 ];
//
//        vec[ 0 ] = x;
//        vec[ 1 ] = y;
//        vec[ 2 ] = z;
    }

    /// <summary>
    /// Multiplies the vector with the given matrix, performing a division by w. The
    /// matrix array is assumed to hold a 4x4 column major matrix as you can get from
    /// <see cref="Val" />. The vector array is assumed to hold a 3-component
    /// vector, with x being the first element, y being the second and z being the last
    /// component. The result is stored in the vector array. This is the same as
    /// <see cref="Vector3.Prj(Matrix4)" />.
    /// </summary>
    /// <param name="mat"> the matrix </param>
    /// <param name="vec"> the vector.  </param>
    public static void Prj( float[] mat, float[] vec )
    {
//        var invW = 1.0f / ( ( vec[ 0 ] * mat[ M30 ] )
//                            + ( vec[ 1 ] * mat[ M31 ] )
//                            + ( vec[ 2 ] * mat[ M32 ] )
//                            + mat[ M33 ] );
//
//        var x = ( ( vec[ 0 ] * mat[ M00 ] )
//                  + ( vec[ 1 ] * mat[ M01 ] )
//                  + ( vec[ 2 ] * mat[ M02 ] )
//                  + mat[ M03 ] ) * invW;
//
//        var y = ( ( vec[ 0 ] * mat[ M10 ] )
//                  + ( vec[ 1 ] * mat[ M11 ] )
//                  + ( vec[ 2 ] * mat[ M12 ] )
//                  + mat[ M13 ] ) * invW;
//
//        var z = ( ( vec[ 0 ] * mat[ M20 ] )
//                  + ( vec[ 1 ] * mat[ M21 ] )
//                  + ( vec[ 2 ] * mat[ M22 ] )
//                  + mat[ M23 ] ) * invW;
//
//        vec[ 0 ] = x;
//        vec[ 1 ] = y;
//        vec[ 2 ] = z;
    }

    /// <summary>
    /// Multiplies the vector with the top most 3x3 sub-matrix of the given matrix.
    /// The matrix array is assumed to hold a 4x4 column major matrix as you can get
    /// from <see cref="Val" />. The vector array is assumed to hold a
    /// 3-component vector, with x being the first element, y being the second and z
    /// being the last component. The result is stored in the vector array. This is the
    /// same as <see cref="Vector3.Rot(Matrix4)" />.
    /// </summary>
    /// <param name="mat"> the matrix </param>
    /// <param name="vec"> the vector.  </param>
    public static void Rot( float[] mat, float[] vec )
    {
//        var x = ( vec[ 0 ] * mat[ M00 ] ) + ( vec[ 1 ] * mat[ M01 ] ) + ( vec[ 2 ] * mat[ M02 ] );
//        var y = ( vec[ 0 ] * mat[ M10 ] ) + ( vec[ 1 ] * mat[ M11 ] ) + ( vec[ 2 ] * mat[ M12 ] );
//        var z = ( vec[ 0 ] * mat[ M20 ] ) + ( vec[ 1 ] * mat[ M21 ] ) + ( vec[ 2 ] * mat[ M22 ] );
//        vec[ 0 ] = x;
//        vec[ 1 ] = y;
//        vec[ 2 ] = z;
    }

    /// <summary>
    /// Computes the inverse of the given matrix. The matrix array is assumed to
    /// hold a 4x4 column major matrix as you can get from <see cref="Val" />.
    /// </summary>
    /// <param name="values"> the matrix values. </param>
    /// <returns> false in case the inverse could not be calculated, true otherwise.  </returns>
    public static bool Invert( float[] values )
    {
//        var lDet = Determinant( values );
//
//        if ( lDet == 0 )
//        {
//            return false;
//        }
//
//        //@formatter:off
//        var m00 = ( ( ( ( values[ M12 ] * values[ M23 ] * values[ M31 ] )
//                      - ( values[ M13 ] * values[ M22 ] * values[ M31 ] ) )
//                      + ( values[ M13 ] * values[ M21 ] * values[ M32 ] ) )
//                      - ( values[ M11 ] * values[ M23 ] * values[ M32 ] )
//                      - ( values[ M12 ] * values[ M21 ] * values[ M33 ] ) )
//                      + ( values[ M11 ] * values[ M22 ] * values[ M33 ] );
//
//        var m01 = ( ( ( values[ M03 ] * values[ M22 ] * values[ M31 ] )
//                    - ( values[ M02 ] * values[ M23 ] * values[ M31 ] )
//                    - ( values[ M03 ] * values[ M21 ] * values[ M32 ] ) )
//                    + ( values[ M01 ] * values[ M23 ] * values[ M32 ] )
//                    + ( values[ M02 ] * values[ M21 ] * values[ M33 ] ) )
//                    - ( values[ M01 ] * values[ M22 ] * values[ M33 ] );
//
//        var m02 = ( ( ( ( values[ M02 ] * values[ M13 ] * values[ M31 ] )
//                      - ( values[ M03 ] * values[ M12 ] * values[ M31 ] ) )
//                      + ( values[ M03 ] * values[ M11 ] * values[ M32 ] ) )
//                      - ( values[ M01 ] * values[ M13 ] * values[ M32 ] )
//                      - ( values[ M02 ] * values[ M11 ] * values[ M33 ] ) )
//                      + ( values[ M01 ] * values[ M12 ] * values[ M33 ] );
//
//        var m03 = ( ( ( values[ M03 ] * values[ M12 ] * values[ M21 ] )
//                    - ( values[ M02 ] * values[ M13 ] * values[ M21 ] )
//                    - ( values[ M03 ] * values[ M11 ] * values[ M22 ] ) )
//                    + ( values[ M01 ] * values[ M13 ] * values[ M22 ] )
//                    + ( values[ M02 ] * values[ M11 ] * values[ M23 ] ) )
//                    - ( values[ M01 ] * values[ M12 ] * values[ M23 ] );
//
//        var m10 = ( ( ( values[ M13 ] * values[ M22 ] * values[ M30 ] )
//                    - ( values[ M12 ] * values[ M23 ] * values[ M30 ] )
//                    - ( values[ M13 ] * values[ M20 ] * values[ M32 ] ) )
//                    + ( values[ M10 ] * values[ M23 ] * values[ M32 ] )
//                    + ( values[ M12 ] * values[ M20 ] * values[ M33 ] ) )
//                    - ( values[ M10 ] * values[ M22 ] * values[ M33 ] );
//
//        var m11 = ( ( ( ( values[ M02 ] * values[ M23 ] * values[ M30 ] )
//                      - ( values[ M03 ] * values[ M22 ] * values[ M30 ] ) )
//                      + ( values[ M03 ] * values[ M20 ] * values[ M32 ] ) )
//                      - ( values[ M00 ] * values[ M23 ] * values[ M32 ] )
//                      - ( values[ M02 ] * values[ M20 ] * values[ M33 ] ) )
//                      + ( values[ M00 ] * values[ M22 ] * values[ M33 ] );
//
//        var m12 = ( ( ( values[ M03 ] * values[ M12 ] * values[ M30 ] )
//                    - ( values[ M02 ] * values[ M13 ] * values[ M30 ] )
//                    - ( values[ M03 ] * values[ M10 ] * values[ M32 ] ) )
//                    + ( values[ M00 ] * values[ M13 ] * values[ M32 ] )
//                    + ( values[ M02 ] * values[ M10 ] * values[ M33 ] ) )
//                    - ( values[ M00 ] * values[ M12 ] * values[ M33 ] );
//
//        var m13 = ( ( ( ( values[ M02 ] * values[ M13 ] * values[ M20 ] )
//                      - ( values[ M03 ] * values[ M12 ] * values[ M20 ] ) )
//                      + ( values[ M03 ] * values[ M10 ] * values[ M22 ] ) )
//                      - ( values[ M00 ] * values[ M13 ] * values[ M22 ] )
//                      - ( values[ M02 ] * values[ M10 ] * values[ M23 ] ) )
//                      + ( values[ M00 ] * values[ M12 ] * values[ M23 ] );
//
//        var m20 = ( ( ( ( values[ M11 ] * values[ M23 ] * values[ M30 ] )
//                      - ( values[ M13 ] * values[ M21 ] * values[ M30 ] ) )
//                      + ( values[ M13 ] * values[ M20 ] * values[ M31 ] ) )
//                      - ( values[ M10 ] * values[ M23 ] * values[ M31 ] )
//                      - ( values[ M11 ] * values[ M20 ] * values[ M33 ] ) )
//                      + ( values[ M10 ] * values[ M21 ] * values[ M33 ] );
//
//        var m21 = ( ( ( values[ M03 ] * values[ M21 ] * values[ M30 ] )
//                    - ( values[ M01 ] * values[ M23 ] * values[ M30 ] )
//                    - ( values[ M03 ] * values[ M20 ] * values[ M31 ] ) )
//                    + ( values[ M00 ] * values[ M23 ] * values[ M31 ] )
//                    + ( values[ M01 ] * values[ M20 ] * values[ M33 ] ) )
//                    - ( values[ M00 ] * values[ M21 ] * values[ M33 ] );
//
//        var m22 = ( ( ( ( values[ M01 ] * values[ M13 ] * values[ M30 ] )
//                      - ( values[ M03 ] * values[ M11 ] * values[ M30 ] ) )
//                      + ( values[ M03 ] * values[ M10 ] * values[ M31 ] ) )
//                      - ( values[ M00 ] * values[ M13 ] * values[ M31 ] )
//                      - ( values[ M01 ] * values[ M10 ] * values[ M33 ] ) )
//                      + ( values[ M00 ] * values[ M11 ] * values[ M33 ] );
//
//        var m23 = ( ( ( values[ M03 ] * values[ M11 ] * values[ M20 ] )
//                    - ( values[ M01 ] * values[ M13 ] * values[ M20 ] )
//                    - ( values[ M03 ] * values[ M10 ] * values[ M21 ] ) )
//                    + ( values[ M00 ] * values[ M13 ] * values[ M21 ] )
//                    + ( values[ M01 ] * values[ M10 ] * values[ M23 ] ) )
//                    - ( values[ M00 ] * values[ M11 ] * values[ M23 ] );
//
//        var m30 = ( ( ( values[ M12 ] * values[ M21 ] * values[ M30 ] )
//                    - ( values[ M11 ] * values[ M22 ] * values[ M30 ] )
//                    - ( values[ M12 ] * values[ M20 ] * values[ M31 ] ) )
//                    + ( values[ M10 ] * values[ M22 ] * values[ M31 ] )
//                    + ( values[ M11 ] * values[ M20 ] * values[ M32 ] ) )
//                    - ( values[ M10 ] * values[ M21 ] * values[ M32 ] );
//
//        var m31 = ( ( ( ( values[ M01 ] * values[ M22 ] * values[ M30 ] )
//                      - ( values[ M02 ] * values[ M21 ] * values[ M30 ] ) )
//                      + ( values[ M02 ] * values[ M20 ] * values[ M31 ] ) )
//                      - ( values[ M00 ] * values[ M22 ] * values[ M31 ] )
//                      - ( values[ M01 ] * values[ M20 ] * values[ M32 ] ) )
//                      + ( values[ M00 ] * values[ M21 ] * values[ M32 ] );
//
//        var m32 = ( ( ( values[ M02 ] * values[ M11 ] * values[ M30 ] )
//                    - ( values[ M01 ] * values[ M12 ] * values[ M30 ] )
//                    - ( values[ M02 ] * values[ M10 ] * values[ M31 ] ) )
//                    + ( values[ M00 ] * values[ M12 ] * values[ M31 ] )
//                    + ( values[ M01 ] * values[ M10 ] * values[ M32 ] ) )
//                    - ( values[ M00 ] * values[ M11 ] * values[ M32 ] );
//
//        var m33 = ( ( ( ( values[ M01 ] * values[ M12 ] * values[ M20 ] )
//                      - ( values[ M02 ] * values[ M11 ] * values[ M20 ] ) )
//                      + ( values[ M02 ] * values[ M10 ] * values[ M21 ] ) )
//                      - ( values[ M00 ] * values[ M12 ] * values[ M21 ] )
//                      - ( values[ M01 ] * values[ M10 ] * values[ M22 ] ) )
//                      + ( values[ M00 ] * values[ M11 ] * values[ M22 ] );
//        //@formatter:on
//
//        var invDet = 1.0f / lDet;
//        values[ M00 ] = m00 * invDet;
//        values[ M10 ] = m10 * invDet;
//        values[ M20 ] = m20 * invDet;
//        values[ M30 ] = m30 * invDet;
//        values[ M01 ] = m01 * invDet;
//        values[ M11 ] = m11 * invDet;
//        values[ M21 ] = m21 * invDet;
//        values[ M31 ] = m31 * invDet;
//        values[ M02 ] = m02 * invDet;
//        values[ M12 ] = m12 * invDet;
//        values[ M22 ] = m22 * invDet;
//        values[ M32 ] = m32 * invDet;
//        values[ M03 ] = m03 * invDet;
//        values[ M13 ] = m13 * invDet;
//        values[ M23 ] = m23 * invDet;
//        values[ M33 ] = m33 * invDet;
//
//        return true;
    }

    /// <summary>
    /// Computes the determinante of the given matrix. The matrix array is assumed
    /// to hold a 4x4 column major matrix as you can get from <see cref="Val" />.
    /// </summary>
    /// <param name="values"> the matrix values. </param>
    /// <returns> the determinante.  </returns>
    public static float Determinant( float[] values )
    {
//        //@formatter:off
//        // BE VERY CAREFUL WITH EDITING THIS!!!!
//        return ( ( ( ( ( ( ( ( ( ( (
//        // --------------------------------------------------------------  
//           ( values[ M30 ] * values[ M21 ] * values[ M12 ] * values[ M03 ] )
//         - ( values[ M20 ] * values[ M31 ] * values[ M12 ] * values[ M03 ] )
//         - ( values[ M30 ] * values[ M11 ] * values[ M22 ] * values[ M03 ] ) )
//        // --------------------------------------------------------------  
//         + ( values[ M10 ] * values[ M31 ] * values[ M22 ] * values[ M03 ] )
//         + ( values[ M20 ] * values[ M11 ] * values[ M32 ] * values[ M03 ] ) )
//         // --------------------------------------------------------------  
//        - ( values[ M10 ] * values[ M21 ] * values[ M32 ] * values[ M03 ] )
//        - ( values[ M30 ] * values[ M21 ] * values[ M02 ] * values[ M13 ] ) )
//        // --------------------------------------------------------------  
//        + ( values[ M20 ] * values[ M31 ] * values[ M02 ] * values[ M13 ] )
//        + ( values[ M30 ] * values[ M01 ] * values[ M22 ] * values[ M13 ] ) )
//        // --------------------------------------------------------------  
//        - ( values[ M00 ] * values[ M31 ] * values[ M22 ] * values[ M13 ] )
//        - ( values[ M20 ] * values[ M01 ] * values[ M32 ] * values[ M13 ] ) )
//        // --------------------------------------------------------------  
//        + ( values[ M00 ] * values[ M21 ] * values[ M32 ] * values[ M13 ] )
//        + ( values[ M30 ] * values[ M11 ] * values[ M02 ] * values[ M23 ] ) )
//        // --------------------------------------------------------------  
//        - ( values[ M10 ] * values[ M31 ] * values[ M02 ] * values[ M23 ] )
//        - ( values[ M30 ] * values[ M01 ] * values[ M12 ] * values[ M23 ] ) )
//        // --------------------------------------------------------------  
//        + ( values[ M00 ] * values[ M31 ] * values[ M12 ] * values[ M23 ] )
//        + ( values[ M10 ] * values[ M01 ] * values[ M32 ] * values[ M23 ] ) )
//        // --------------------------------------------------------------  
//        - ( values[ M00 ] * values[ M11 ] * values[ M32 ] * values[ M23 ] )
//        - ( values[ M20 ] * values[ M11 ] * values[ M02 ] * values[ M33 ] ) )
//        // --------------------------------------------------------------  
//        + ( values[ M10 ] * values[ M21 ] * values[ M02 ] * values[ M33 ] )
//        + ( values[ M20 ] * values[ M01 ] * values[ M12 ] * values[ M33 ] ) )
//        // --------------------------------------------------------------  
//        - ( values[ M00 ] * values[ M21 ] * values[ M12 ] * values[ M33 ] )
//        - ( values[ M10 ] * values[ M01 ] * values[ M22 ] * values[ M33 ] ) )
//        // --------------------------------------------------------------  
//        + ( values[ M00 ] * values[ M11 ] * values[ M22 ] * values[ M33 ] );
//        //@formatter:on
    }

    /// <summary>
    /// Postmultiplies this matrix by a translation matrix.
    /// Postmultiplication is also used by OpenGL ES' glTranslate/glRotate/glScale
    /// </summary>
    /// <param name="translation"> </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Translate( Vector3 translation )
    {
        return Translate( translation.X, translation.Y, translation.Z );
    }

    /// <summary>
    /// Postmultiplies this matrix by a translation matrix.
    /// Postmultiplication is also used by OpenGL ES' 1.x glTranslate/glRotate/glScale.
    /// </summary>
    /// <param name="x"> Translation in the x-axis. </param>
    /// <param name="y"> Translation in the y-axis. </param>
    /// <param name="z"> Translation in the z-axis. </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Translate( float x, float y, float z )
    {
//        Val[ M03 ] += ( Val[ M00 ] * x ) + ( Val[ M01 ] * y ) + ( Val[ M02 ] * z );
//        Val[ M13 ] += ( Val[ M10 ] * x ) + ( Val[ M11 ] * y ) + ( Val[ M12 ] * z );
//        Val[ M23 ] += ( Val[ M20 ] * x ) + ( Val[ M21 ] * y ) + ( Val[ M22 ] * z );
//        Val[ M33 ] += ( Val[ M30 ] * x ) + ( Val[ M31 ] * y ) + ( Val[ M32 ] * z );
//
//        return this;
    }

    /// <summary>
    /// Postmultiplies this matrix with a (counter-clockwise) rotation matrix.
    /// Postmultiplication is also used by OpenGL ES' 1.x glTranslate/glRotate/glScale.
    /// </summary>
    /// <param name="axis"> The vector axis to rotate around. </param>
    /// <param name="degrees"> The angle in degrees. </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Rotate( Vector3 axis, float degrees )
    {
        if ( degrees == 0 )
        {
            return this;
        }

        Quat.SetFromAxis( axis, degrees );

        return Rotate( Quat );
    }

    /// <summary>
    /// Postmultiplies this matrix with a (counter-clockwise) rotation matrix.
    /// Postmultiplication is also used by OpenGL ES' 1.x glTranslate/glRotate/glScale.
    /// </summary>
    /// <param name="axis"> The vector axis to rotate around. </param>
    /// <param name="radians"> The angle in radians. </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 RotateRad( Vector3 axis, float radians )
    {
        if ( radians == 0 )
        {
            return this;
        }

        Quat.SetFromAxisRad( axis, radians );

        return Rotate( Quat );
    }

    /// <summary>
    /// Postmultiplies this matrix with a (counter-clockwise) rotation matrix.
    /// Postmultiplication is also used by OpenGL ES' 1.x glTranslate/glRotate/glScale
    /// </summary>
    /// <param name="axisX"> The x-axis component of the vector to rotate around. </param>
    /// <param name="axisY"> The y-axis component of the vector to rotate around. </param>
    /// <param name="axisZ"> The z-axis component of the vector to rotate around. </param>
    /// <param name="degrees"> The angle in degrees </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Rotate( float axisX, float axisY, float axisZ, float degrees )
    {
        if ( degrees == 0 )
        {
            return this;
        }

        Quat.SetFromAxis( axisX, axisY, axisZ, degrees );

        return Rotate( Quat );
    }

    /// <summary>
    /// Postmultiplies this matrix with a (counter-clockwise) rotation matrix.
    /// Postmultiplication is also used by OpenGL ES' 1.x glTranslate/glRotate/glScale
    /// </summary>
    /// <param name="axisX"> The x-axis component of the vector to rotate around. </param>
    /// <param name="axisY"> The y-axis component of the vector to rotate around. </param>
    /// <param name="axisZ"> The z-axis component of the vector to rotate around. </param>
    /// <param name="radians"> The angle in radians </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 RotateRad( float axisX, float axisY, float axisZ, float radians )
    {
        if ( radians == 0 )
        {
            return this;
        }

        Quat.SetFromAxisRad( axisX, axisY, axisZ, radians );

        return Rotate( Quat );
    }

    /// <summary>
    /// Postmultiplies this matrix with a (counter-clockwise) rotation matrix.
    /// Postmultiplication is also used by OpenGL ES' 1.x glTranslate/glRotate/glScale.
    /// </summary>
    /// <param name="rotation"> </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Rotate( Quaternion rotation )
    {
//        float x = rotation.X, y = rotation.Y, z = rotation.Z, w = rotation.W;
//
//        var xx = x * x;
//        var xy = x * y;
//        var xz = x * z;
//        var xw = x * w;
//        var yy = y * y;
//        var yz = y * z;
//        var yw = y * w;
//        var zz = z * z;
//        var zw = z * w;
//
//        // Set matrix from quaternion
//        var r00 = 1 - ( 2 * ( yy + zz ) );
//        var r01 = 2 * ( xy - zw );
//        var r02 = 2 * ( xz + yw );
//        var r10 = 2 * ( xy + zw );
//        var r11 = 1 - ( 2 * ( xx + zz ) );
//        var r12 = 2 * ( yz - xw );
//        var r20 = 2 * ( xz - yw );
//        var r21 = 2 * ( yz + xw );
//        var r22 = 1 - ( 2 * ( xx + yy ) );
//
//        var m00 = ( Val[ M00 ] * r00 ) + ( Val[ M01 ] * r10 ) + ( Val[ M02 ] * r20 );
//        var m01 = ( Val[ M00 ] * r01 ) + ( Val[ M01 ] * r11 ) + ( Val[ M02 ] * r21 );
//        var m02 = ( Val[ M00 ] * r02 ) + ( Val[ M01 ] * r12 ) + ( Val[ M02 ] * r22 );
//        var m10 = ( Val[ M10 ] * r00 ) + ( Val[ M11 ] * r10 ) + ( Val[ M12 ] * r20 );
//        var m11 = ( Val[ M10 ] * r01 ) + ( Val[ M11 ] * r11 ) + ( Val[ M12 ] * r21 );
//        var m12 = ( Val[ M10 ] * r02 ) + ( Val[ M11 ] * r12 ) + ( Val[ M12 ] * r22 );
//        var m20 = ( Val[ M20 ] * r00 ) + ( Val[ M21 ] * r10 ) + ( Val[ M22 ] * r20 );
//        var m21 = ( Val[ M20 ] * r01 ) + ( Val[ M21 ] * r11 ) + ( Val[ M22 ] * r21 );
//        var m22 = ( Val[ M20 ] * r02 ) + ( Val[ M21 ] * r12 ) + ( Val[ M22 ] * r22 );
//        var m30 = ( Val[ M30 ] * r00 ) + ( Val[ M31 ] * r10 ) + ( Val[ M32 ] * r20 );
//        var m31 = ( Val[ M30 ] * r01 ) + ( Val[ M31 ] * r11 ) + ( Val[ M32 ] * r21 );
//        var m32 = ( Val[ M30 ] * r02 ) + ( Val[ M31 ] * r12 ) + ( Val[ M32 ] * r22 );
//
//        Val[ M00 ] = m00;
//        Val[ M10 ] = m10;
//        Val[ M20 ] = m20;
//        Val[ M30 ] = m30;
//
//        // ----------------------------
//        Val[ M01 ] = m01;
//        Val[ M11 ] = m11;
//        Val[ M21 ] = m21;
//        Val[ M31 ] = m31;
//
//        // ----------------------------
//        Val[ M02 ] = m02;
//        Val[ M12 ] = m12;
//        Val[ M22 ] = m22;
//        Val[ M32 ] = m32;
//
//        return this;
    }

    /// <summary>
    /// Postmultiplies this matrix by the rotation between two vectors.
    /// </summary>
    /// <param name="v1"> The base vector </param>
    /// <param name="v2"> The target vector </param>
    /// <returns> This matrix for the purpose of chaining methods together  </returns>
    public Matrix4 Rotate( Vector3 v1, Vector3 v2 )
    {
        return Rotate( Quat.SetFromCross( v1, v2 ) );
    }

    /// <summary>
    /// Post-multiplies this matrix by a rotation toward a direction.
    /// </summary>
    /// <param name="direction"> direction to rotate toward </param>
    /// <param name="up"> up vector </param>
    /// <returns> This matrix for chaining  </returns>
    public Matrix4 RotateTowardDirection( Vector3 direction, Vector3 up )
    {
//        LVez.Set( direction ).Nor();
//        LVex.Set( direction ).Crs( up ).Nor();
//        LVey.Set( LVex ).Crs( LVez ).Nor();
//
//        var m00 = ( Val[ M00 ] * LVex.X ) + ( Val[ M01 ] * LVex.Y ) + ( Val[ M02 ] * LVex.Z );
//        var m01 = ( Val[ M00 ] * LVey.X ) + ( Val[ M01 ] * LVey.Y ) + ( Val[ M02 ] * LVey.Z );
//        var m02 = ( Val[ M00 ] * -LVez.X ) + ( Val[ M01 ] * -LVez.Y ) + ( Val[ M02 ] * -LVez.Z );
//        var m10 = ( Val[ M10 ] * LVex.X ) + ( Val[ M11 ] * LVex.Y ) + ( Val[ M12 ] * LVex.Z );
//        var m11 = ( Val[ M10 ] * LVey.X ) + ( Val[ M11 ] * LVey.Y ) + ( Val[ M12 ] * LVey.Z );
//        var m12 = ( Val[ M10 ] * -LVez.X ) + ( Val[ M11 ] * -LVez.Y ) + ( Val[ M12 ] * -LVez.Z );
//        var m20 = ( Val[ M20 ] * LVex.X ) + ( Val[ M21 ] * LVex.Y ) + ( Val[ M22 ] * LVex.Z );
//        var m21 = ( Val[ M20 ] * LVey.X ) + ( Val[ M21 ] * LVey.Y ) + ( Val[ M22 ] * LVey.Z );
//        var m22 = ( Val[ M20 ] * -LVez.X ) + ( Val[ M21 ] * -LVez.Y ) + ( Val[ M22 ] * -LVez.Z );
//        var m30 = ( Val[ M30 ] * LVex.X ) + ( Val[ M31 ] * LVex.Y ) + ( Val[ M32 ] * LVex.Z );
//        var m31 = ( Val[ M30 ] * LVey.X ) + ( Val[ M31 ] * LVey.Y ) + ( Val[ M32 ] * LVey.Z );
//        var m32 = ( Val[ M30 ] * -LVez.X ) + ( Val[ M31 ] * -LVez.Y ) + ( Val[ M32 ] * -LVez.Z );
//
//        Val[ M00 ] = m00;
//        Val[ M10 ] = m10;
//        Val[ M20 ] = m20;
//        Val[ M30 ] = m30;
//
//        // ----------------------------
//        Val[ M01 ] = m01;
//        Val[ M11 ] = m11;
//        Val[ M21 ] = m21;
//        Val[ M31 ] = m31;
//
//        // ----------------------------
//        Val[ M02 ] = m02;
//        Val[ M12 ] = m12;
//        Val[ M22 ] = m22;
//        Val[ M32 ] = m32;
//
//        return this;
    }

    /// <summary>
    /// Post-multiplies this matrix by a rotation toward a target.
    /// </summary>
    /// <param name="target"> the target to rotate to </param>
    /// <param name="up"> the up vector </param>
    /// <returns> This matrix for chaining  </returns>
    public Matrix4 RotateTowardTarget( Vector3 target, Vector3 up )
    {
//        TmpVec.Set( target.X - Val[ M03 ], target.Y - Val[ M13 ], target.Z - Val[ M23 ] );
//
//        return RotateTowardDirection( TmpVec, up );
    }

    /// <summary>
    /// Postmultiplies this matrix with a scale matrix.
    /// Postmultiplication is also used by OpenGL ES' 1.x glTranslate/glRotate/glScale.
    /// </summary>
    /// <param name="scaleX"> The scale in the x-axis. </param>
    /// <param name="scaleY"> The scale in the y-axis. </param>
    /// <param name="scaleZ"> The scale in the z-axis. </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 Scale( float scaleX, float scaleY, float scaleZ )
    {
//        Val[ M00 ] *= scaleX;
//        Val[ M01 ] *= scaleY;
//        Val[ M02 ] *= scaleZ;
//
//        // ----------------------------
//        Val[ M10 ] *= scaleX;
//        Val[ M11 ] *= scaleY;
//        Val[ M12 ] *= scaleZ;
//
//        // ----------------------------
//        Val[ M20 ] *= scaleX;
//        Val[ M21 ] *= scaleY;
//        Val[ M22 ] *= scaleZ;
//
//        // ----------------------------
//        Val[ M30 ] *= scaleX;
//        Val[ M31 ] *= scaleY;
//        Val[ M32 ] *= scaleZ;
//
//        return this;
    }

    /// <summary>
    /// Copies the 4x3 upper-left sub-matrix into float array. The destination
    /// array is supposed to be a column major matrix.
    /// </summary>
    /// <param name="dst"> the destination matrix </param>
    public void Extract4X3Matrix( ref float[] dst )
    {
//        dst[ 0 ]  = Val[ M00 ];
//        dst[ 1 ]  = Val[ M10 ];
//        dst[ 2 ]  = Val[ M20 ];
//        dst[ 3 ]  = Val[ M01 ];
//        dst[ 4 ]  = Val[ M11 ];
//        dst[ 5 ]  = Val[ M21 ];
//        dst[ 6 ]  = Val[ M02 ];
//        dst[ 7 ]  = Val[ M12 ];
//        dst[ 8 ]  = Val[ M22 ];
//        dst[ 9 ]  = Val[ M03 ];
//        dst[ 10 ] = Val[ M13 ];
//        dst[ 11 ] = Val[ M23 ];
    }

    /// <summary>
    /// </summary>
    /// <returns>True if this matrix has any rotation or scaling, false otherwise </returns>
    public bool HasRotationOrScaling()
    {
//        return !( MathUtils.IsEqual( Val[ M00 ], 1 )
//                  && MathUtils.IsEqual( Val[ M11 ], 1 )
//                  && MathUtils.IsEqual( Val[ M22 ], 1 )
//                  && MathUtils.IsZero( Val[ M01 ] )
//                  && MathUtils.IsZero( Val[ M02 ] )
//                  && MathUtils.IsZero( Val[ M10 ] )
//                  && MathUtils.IsZero( Val[ M12 ] )
//                  && MathUtils.IsZero( Val[ M20 ] )
//                  && MathUtils.IsZero( Val[ M21 ] ) );
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Multiplies the vectors with the given matrix. The matrix array is assumed to hold a 4x4
    /// column major matrix as you can get from <see cref="Val" />. The vectors array is assumed
    /// to hold 3-component vectors. Offset specifies the offset into the array where the x-component
    /// of the first vector is located. The numVecs parameter specifies the number of vectors stored
    /// in the vectors array. The stride parameter specifies the number of floats between subsequent
    /// vectors and must be >= 3. This is the same as <see cref="Vector3.Mul(Matrix4)" /> applied to
    /// multiple vectors.
    /// </summary>
    /// <param name="mat"> the matrix </param>
    /// <param name="vecs"> the vectors </param>
    /// <param name="offset"> the offset into the vectors array </param>
    /// <param name="numVecs"> the number of vectors </param>
    /// <param name="stride"> the stride between vectors in floats  </param>
    public static void MulVec( float[] mat, float[] vecs, int offset, int numVecs, int stride )
    {
//        for ( var i = 0; i < numVecs; i++ )
//        {
//            var idx = offset + ( i * stride );
//            var x   = vecs[ idx ];
//            var y   = vecs[ idx + 1 ];
//            var z   = vecs[ idx + 2 ];
//
//            vecs[ idx ]     = ( mat[ M00 ] * x ) + ( mat[ M01 ] * y ) + ( mat[ M02 ] * z ) + mat[ M03 ];
//            vecs[ idx + 1 ] = ( mat[ M10 ] * x ) + ( mat[ M11 ] * y ) + ( mat[ M12 ] * z ) + mat[ M13 ];
//            vecs[ idx + 2 ] = ( mat[ M20 ] * x ) + ( mat[ M21 ] * y ) + ( mat[ M22 ] * z ) + mat[ M23 ];
//        }
    }

    /// <summary>
    /// Multiplies the vectors with the given matrix, performing a division by w.
    /// The matrix array is assumed to hold a 4x4 column major matrix as you can
    /// get from <see cref="Val" />. The vectors array is assumed to hold
    /// 3-component vectors. Offset specifies the offset into the array where the
    /// x-component of the first vector is located. The numVecs parameter specifies
    /// the number of vectors stored in the vectors array. The stride parameter
    /// specifies the number of floats between subsequent vectors and must be >= 3.
    /// This is the same as <see cref="Vector3.Prj(Matrix4)" /> applied to multiple
    /// vectors.
    /// </summary>
    /// <param name="mat"> the matrix </param>
    /// <param name="vecs"> the vectors </param>
    /// <param name="offset"> the offset into the vectors array </param>
    /// <param name="numVecs"> the number of vectors </param>
    /// <param name="stride"> the stride between vectors in floats  </param>
    public static void Prj( float[] mat, float[] vecs, int offset, int numVecs, int stride )
    {
//        for ( var i = 0; i < numVecs; i++ )
//        {
//            var vecIndex = offset + ( i * stride );
//            var invW = 1.0f / ( ( vecs[ vecIndex ] * mat[ M30 ] )
//                                + ( vecs[ vecIndex + 1 ] * mat[ M31 ] )
//                                + ( vecs[ vecIndex + 2 ] * mat[ M32 ] )
//                                + mat[ M33 ] );
//
//            var x = ( ( vecs[ vecIndex ] * mat[ M00 ] )
//                      + ( vecs[ vecIndex + 1 ] * mat[ M01 ] )
//                      + ( vecs[ vecIndex + 2 ] * mat[ M02 ] )
//                      + mat[ M03 ] ) * invW;
//            var y = ( ( vecs[ vecIndex ] * mat[ M10 ] )
//                      + ( vecs[ vecIndex + 1 ] * mat[ M11 ] )
//                      + ( vecs[ vecIndex + 2 ] * mat[ M12 ] )
//                      + mat[ M13 ] ) * invW;
//            var z = ( ( vecs[ vecIndex ] * mat[ M20 ] )
//                      + ( vecs[ vecIndex + 1 ] * mat[ M21 ] )
//                      + ( vecs[ vecIndex + 2 ] * mat[ M22 ] )
//                      + mat[ M23 ] ) * invW;
//
//            vecs[ vecIndex ]     = x;
//            vecs[ vecIndex + 1 ] = y;
//            vecs[ vecIndex + 2 ] = z;
//        }
    }

    /// <summary>
    /// Multiplies the vectors with the top most 3x3 sub-matrix of the given matrix.
    /// The matrix array is assumed to hold a 4x4 column major matrix as you can get
    /// from <see cref="Val" />. The vectors array is assumed to hold
    /// 3-component vectors. Offset specifies the offset into the array where the
    /// x-component of the first vector is located. The numVecs parameter specifies
    /// the number of vectors stored in the vectors array. The stride parameter
    /// specifies the number of floats between subsequent vectors and must be >= 3.
    /// This is the same as <see cref="Vector3.Rot(Matrix4)" /> applied to multiple vectors.
    /// </summary>
    /// <param name="mat"> the matrix </param>
    /// <param name="vecs"> the vectors </param>
    /// <param name="offset"> the offset into the vectors array </param>
    /// <param name="numVecs"> the number of vectors </param>
    /// <param name="stride"> the stride between vectors in floats  </param>
    public static void Rot( float[] mat, float[] vecs, int offset, int numVecs, int stride )
    {
//        for ( var i = 0; i < numVecs; i++ )
//        {
//            var idx = offset + ( i * stride );
//            var x   = vecs[ idx ];
//            var y   = vecs[ idx + 1 ];
//            var z   = vecs[ idx + 2 ];
//
//            vecs[ idx ]     = ( mat[ M00 ] * x ) + ( mat[ M01 ] * y ) + ( mat[ M02 ] * z );
//            vecs[ idx + 1 ] = ( mat[ M10 ] * x ) + ( mat[ M11 ] * y ) + ( mat[ M12 ] * z );
//            vecs[ idx + 2 ] = ( mat[ M20 ] * x ) + ( mat[ M21 ] * y ) + ( mat[ M22 ] * z );
//        }
    }

    /// <summary>
    /// Validates that the matrix is properly constructed and contains valid values.
    /// </summary>
    /// <returns>True if the matrix is valid, false otherwise</returns>
    public bool IsValid()
    {
        Logger.Checkpoint();

        // Check for NaN or Infinity
        for ( var i = 0; i < 16; i++ )
        {
            if ( float.IsNaN( Val[ i ] ) || float.IsInfinity( Val[ i ] ) )
            {
                Logger.Checkpoint();

                return false;
            }
        }

        Logger.Debug( $"[{Val[ M30 ]}]:[{Val[ M31 ]}]:[{Val[ M32 ]}]:[{Val[ M33 ]}]" );

        // For a valid transformation matrix, the bottom row should be [0,0,0,1]
        if ( ( Math.Abs( Val[ M30 ] ) > float.Epsilon )
             || ( Math.Abs( Val[ M31 ] ) > float.Epsilon )
             || ( Math.Abs( Val[ M32 ] ) > float.Epsilon )
             || ( Math.Abs( Val[ M33 ] - 1f ) > float.Epsilon ) )
        {
            Logger.Checkpoint();

            return false;
        }

        Logger.Checkpoint();

        return true;
    }

    /// <summary>
    /// Validates that the matrix can be used as a view or projection matrix
    /// </summary>
    public bool IsValidViewMatrix()
    {
//        if ( !IsValid() )
//        {
//            Logger.Checkpoint();
//
//            return false;
//        }
//
//        Logger.Checkpoint();
//
//        const float TOLERANCE = 0.01f;
//
//        // Check orthogonality
//        var dotXY = ( Val[ M00 ] * Val[ M10 ] ) + ( Val[ M01 ] * Val[ M11 ] ) + ( Val[ M02 ] * Val[ M12 ] );
//        var dotXZ = ( Val[ M00 ] * Val[ M20 ] ) + ( Val[ M01 ] * Val[ M21 ] ) + ( Val[ M02 ] * Val[ M22 ] );
//        var dotYZ = ( Val[ M10 ] * Val[ M20 ] ) + ( Val[ M11 ] * Val[ M21 ] ) + ( Val[ M12 ] * Val[ M22 ] );
//
//        // Check unit length of basis vectors
//        var lenX = ( Val[ M00 ] * Val[ M00 ] ) + ( Val[ M01 ] * Val[ M01 ] ) + ( Val[ M02 ] * Val[ M02 ] );
//        var lenY = ( Val[ M10 ] * Val[ M10 ] ) + ( Val[ M11 ] * Val[ M11 ] ) + ( Val[ M12 ] * Val[ M12 ] );
//        var lenZ = ( Val[ M20 ] * Val[ M20 ] ) + ( Val[ M21 ] * Val[ M21 ] ) + ( Val[ M22 ] * Val[ M22 ] );
//
//        // Check bottom row is [0,0,0,1]
//        var validW = ( Math.Abs( Val[ M03 ] ) < TOLERANCE )
//                     && ( Math.Abs( Val[ M13 ] ) < TOLERANCE )
//                     && ( Math.Abs( Val[ M23 ] ) < TOLERANCE )
//                     && ( Math.Abs( Val[ M33 ] - 1f ) < TOLERANCE );
//
//        return ( Math.Abs( dotXY ) < TOLERANCE )
//               && ( Math.Abs( dotXZ ) < TOLERANCE )
//               && ( Math.Abs( dotYZ ) < TOLERANCE )
//               && ( Math.Abs( lenX - 1f ) < TOLERANCE )
//               && ( Math.Abs( lenY - 1f ) < TOLERANCE )
//               && ( Math.Abs( lenZ - 1f ) < TOLERANCE )
//               && validW;
    }

    /// <summary>
    /// Method to check if matrix is identity
    /// </summary>
    public bool IsIdentity()
    {
        const float EPSILON = NumberUtils.FLOAT_EPSILON;

        // Check diagonal elements are close to 1
        if ( ( Math.Abs( Values[ M00 ] - 1f ) > EPSILON )
             || ( Math.Abs( Values[ M11 ] - 1f ) > EPSILON )
             || ( Math.Abs( Values[ M22 ] - 1f ) > EPSILON )
             || ( Math.Abs( Values[ M33 ] - 1f ) > EPSILON ) )
        {
            return false;
        }

        // Check non-diagonal elements are close to 0
        return ( Math.Abs( Values[ M01 ] ) <= EPSILON )
               && ( Math.Abs( Values[ M02 ] ) <= EPSILON )
               && ( Math.Abs( Values[ M03 ] ) <= EPSILON )
               && ( Math.Abs( Values[ M10 ] ) <= EPSILON )
               && ( Math.Abs( Values[ M12 ] ) <= EPSILON )
               && ( Math.Abs( Values[ M13 ] ) <= EPSILON )
               && ( Math.Abs( Values[ M20 ] ) <= EPSILON )
               && ( Math.Abs( Values[ M21 ] ) <= EPSILON )
               && ( Math.Abs( Values[ M23 ] ) <= EPSILON )
               && ( Math.Abs( Values[ M30 ] ) <= EPSILON )
               && ( Math.Abs( Values[ M31 ] ) <= EPSILON )
               && ( Math.Abs( Values[ M32 ] ) <= EPSILON );
    }

    /// <summary>
    /// </summary>
    public Vector4 GetColumn( int index )
    {
        Guard.InRange( index, 0, 3 );

        return new Vector4( Val[ ( index * 4 ) + 0 ],
                            Val[ ( index * 4 ) + 1 ],
                            Val[ ( index * 4 ) + 2 ],
                            Val[ ( index * 4 ) + 3 ] );
    }

    /// <summary>
    /// </summary>
    public Vector4 GetRow( int index )
    {
        Guard.InRange( index, 0, 3 );

        return new Vector4( Val[ index ],
                            Val[ index + 4 ],
                            Val[ index + 8 ],
                            Val[ index + 12 ] );
    }

    // ========================================================================

    public override string ToString()
    {
        return $"Row 0: [{Val[ M00 ]:F6}|{Val[ M01 ]:F6}|{Val[ M02 ]:F6}|{Val[ M03 ]:F6}]\n" +
               $"Row 1: [{Val[ M10 ]:F6}|{Val[ M11 ]:F6}|{Val[ M12 ]:F6}|{Val[ M13 ]:F6}]\n" +
               $"Row 2: [{Val[ M20 ]:F6}|{Val[ M21 ]:F6}|{Val[ M22 ]:F6}|{Val[ M23 ]:F6}]\n" +
               $"Row 3: [{Val[ M30 ]:F6}|{Val[ M31 ]:F6}|{Val[ M32 ]:F6}|{Val[ M33 ]:F6}]\n";
    }

    /// <summary>
    /// Debug method to print the matrix in a readable format
    /// </summary>
    public void Debug()
    {
        Logger.Debug( $"\n{ToString()}" );
    }

    // ========================================================================
    // ========================================================================
}