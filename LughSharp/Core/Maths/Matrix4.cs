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

using System.Text;

using JetBrains.Annotations;

using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace LughSharp.Core.Maths;

/// <summary>
/// Column-Major Matrix4 class.
/// <code>
/// [M00:( 00 )] [M10:( 01 )] [M20:( 02 )] [M30:( 03 )]
/// [M01:( 04 )] [M11:( 05 )] [M21:( 06 )] [M31:( 07 )]
/// [M02:( 08 )] [M12:( 09 )] [M22:( 10 )] [M32:( 11 )]
/// [M03:( 12 )] [M13:( 13 )] [M23:( 14 )] [M33:( 15 )]
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
    public const int M000 = 0;

    /// <summary>
    /// XY: Typically the negative sine of the angle when rotated on the Z axis.
    /// On Vector3 multiplication this value is multiplied with the source Y
    /// component and added to the target X component.
    /// </summary>
    public const int M014 = 4;

    /// <summary>
    /// XZ: Typically the sine of the angle when rotated on the Y axis.
    /// On Vector3 multiplication this value is multiplied with the
    /// source Z component and added to the target X component.
    /// </summary>
    public const int M028 = 8;

    /// <summary>
    /// XW: Typically the translation of the X component. On Vector3 multiplication
    /// this value is added to the target X component.
    /// </summary>
    public const int M0312 = 12;

    // ------------------------------------------
    // ROW 1
    // ------------------------------------------

    /// <summary>
    /// Column 1, Row 0.
    /// YX: Typically the sine of the angle when rotated on the Z axis. On Vector3
    /// multiplication this value is multiplied with the source X component and
    /// added to the target Y component.
    /// </summary>
    public const int M101 = 1;

    /// <summary>
    /// YY: Typically the unrotated Y component for scaling, also the cosine of the
    /// angle when rotated on the X and/or Z axis. On Vector3 multiplication this value
    /// is multiplied with the source Y component and added to the target Y component.
    /// </summary>
    public const int M115 = 5;

    /// <summary>
    /// YZ: Typically the negative sine of the angle when rotated on the X axis.
    /// On Vector3 multiplication this value is multiplied with the source Z component
    /// and added to the target Y component.
    /// </summary>
    public const int M129 = 9;

    /// <summary>
    /// YW: Typically the translation of the Y component.
    /// On Vector3 multiplication this value is added to the target Y component.
    /// </summary>
    public const int M1313 = 13;

    // ------------------------------------------
    // ROW 2
    // ------------------------------------------

    /// <summary>
    /// ZX: Typically the negative sine of the angle when rotated on the Y axis.
    /// On Vector3 multiplication this value is multiplied with the source X component
    /// and added to the target Z component.
    /// </summary>
    public const int M202 = 2;

    /// <summary>
    /// ZY: Typically the sine of the angle when rotated on the X axis.
    /// On Vector3 multiplication this value is multiplied with the source Y component
    /// and added to the target Z component.
    /// </summary>
    public const int M216 = 6;

    /// <summary>
    /// ZZ: Typically the unrotated Z component for scaling, also the cosine of the angle
    /// when rotated on the X and/or Y axis. On Vector3 multiplication this value is
    /// multiplied with the source Z component and added to the target Z component.
    /// </summary>
    public const int M2210 = 10;

    /// <summary>
    /// ZW: Typically the translation of the Z component. On Vector3 multiplication
    /// this value is added to the target Z component.
    /// </summary>
    public const int M2314 = 14;

    // ------------------------------------------
    // ROW 3
    // ------------------------------------------

    /// <summary>
    /// WX: Typically the value zero. On Vector3 multiplication this value is ignored.
    /// </summary>
    public const int M303 = 3;

    /// <summary>
    /// WY: Typically the value zero. On Vector3 multiplication this value is ignored.
    /// </summary>
    public const int M317 = 7;

    /// <summary>
    /// WZ: Typically the value zero. On Vector3 multiplication this value is ignored.
    /// </summary>
    public const int M3211 = 11;

    /// <summary>
    /// WW: Typically the value one. On Vector3 multiplication this value is ignored.
    /// </summary>
    public const int M3315 = 15;

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
    public static readonly Matrix4    Identity   = new();

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
    /// Constructs a rotation matrix from the given <see cref="Quaternion"/>.
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

        Val[ M000 ]  = 1.0f - ( yy + zz );
        Val[ M014 ]  = xy - wz;
        Val[ M028 ]  = xz + wy;
        Val[ M0312 ] = translationX;

        Val[ M101 ]  = xy + wz;
        Val[ M115 ]  = 1.0f - ( xx + zz );
        Val[ M129 ]  = yz - wx;
        Val[ M1313 ] = translationY;

        Val[ M202 ]  = xz - wy;
        Val[ M216 ]  = yz + wx;
        Val[ M2210 ] = 1.0f - ( xx + yy );
        Val[ M2314 ] = translationZ;

        Val[ M303 ]  = 0.0f;
        Val[ M317 ]  = 0.0f;
        Val[ M3211 ] = 0.0f;
        Val[ M3315 ] = 1.0f;

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

        Val[ M000 ]  = scaleX * ( 1.0f - ( yy + zz ) );
        Val[ M014 ]  = scaleY * ( xy - wz );
        Val[ M028 ]  = scaleZ * ( xz + wy );
        Val[ M0312 ] = translationX;

        Val[ M101 ]  = scaleX * ( xy + wz );
        Val[ M115 ]  = scaleY * ( 1.0f - ( xx + zz ) );
        Val[ M129 ]  = scaleZ * ( yz - wx );
        Val[ M1313 ] = translationY;

        Val[ M202 ]  = scaleX * ( xz - wy );
        Val[ M216 ]  = scaleY * ( yz + wx );
        Val[ M2210 ] = scaleZ * ( 1.0f - ( xx + yy ) );
        Val[ M2314 ] = translationZ;

        Val[ M303 ]  = 0.0f;
        Val[ M317 ]  = 0.0f;
        Val[ M3211 ] = 0.0f;
        Val[ M3315 ] = 1.0f;

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
        Val[ M000 ]  = xAxis.X;
        Val[ M014 ]  = xAxis.Y;
        Val[ M028 ]  = xAxis.Z;
        Val[ M0312 ] = pos.X;

        Val[ M101 ]  = yAxis.X;
        Val[ M115 ]  = yAxis.Y;
        Val[ M129 ]  = yAxis.Z;
        Val[ M1313 ] = pos.Y;

        Val[ M202 ]  = zAxis.X;
        Val[ M216 ]  = zAxis.Y;
        Val[ M2210 ] = zAxis.Z;
        Val[ M2314 ] = pos.Z;

        Val[ M303 ]  = 0.0f;
        Val[ M317 ]  = 0.0f;
        Val[ M3211 ] = 0.0f;
        Val[ M3315 ] = 1.0f;

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
        Val[ M0312 ] += vector.X;
        Val[ M1313 ] += vector.Y;
        Val[ M2314 ] += vector.Z;

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
        Val[ M0312 ] += x;
        Val[ M1313 ] += y;
        Val[ M2314 ] += z;

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
    ///    a b c d   - M00 M01 M02 M03
    ///    e f g h   - M10 M11 M12 M13
    ///    i j k l   - M20 M21 M22 M23
    ///    m n o p   - M30 M31 M32 M33
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
        float m01 = Val[ M014 ];
        float m02 = Val[ M028 ];
        float m03 = Val[ M0312 ];
        float m12 = Val[ M129 ];
        float m13 = Val[ M1313 ];
        float m23 = Val[ M2314 ];

        Val[ M014 ]  = Val[ M101 ];
        Val[ M028 ]  = Val[ M202 ];
        Val[ M0312 ] = Val[ M303 ];
        Val[ M101 ]  = m01;
        Val[ M129 ]  = Val[ M216 ];
        Val[ M1313 ] = Val[ M317 ];
        Val[ M202 ]  = m02;
        Val[ M216 ]  = m12;
        Val[ M2314 ] = Val[ M3211 ];
        Val[ M303 ]  = m03;
        Val[ M317 ]  = m13;
        Val[ M3211 ] = m23;

        //        Array.Copy( Val, 0, TmpMat.Val, 0, Val.Length );
        //
        //        // Transpose Column 1
        //        Val[ M00_0 ] = TmpMat.Val[ M00_0 ];
        //        Val[ M10_1 ] = TmpMat.Val[ M01_4 ];
        //        Val[ M20_2 ] = TmpMat.Val[ M02_8 ];
        //        Val[ M30_3 ] = TmpMat.Val[ M03_12 ];
        //
        //        // Transpose Column 2
        //        Val[ M01_4 ] = TmpMat.Val[ M10_1 ];
        //        Val[ M11_5 ] = TmpMat.Val[ M11_5 ];
        //        Val[ M21_6 ] = TmpMat.Val[ M12_9 ];
        //        Val[ M31_7 ] = TmpMat.Val[ M13_13 ];
        //
        //        // Transpose Column 3
        //        Val[ M02_8 ]  = TmpMat.Val[ M20_2 ];
        //        Val[ M12_9 ]  = TmpMat.Val[ M21_6 ];
        //        Val[ M22_10 ] = TmpMat.Val[ M22_10 ];
        //        Val[ M32_11 ] = TmpMat.Val[ M23_14 ];
        //
        //        // Transpose Column 4
        //        Val[ M03_12 ] = TmpMat.Val[ M30_3 ];
        //        Val[ M13_13 ] = TmpMat.Val[ M31_7 ];
        //        Val[ M23_14 ] = TmpMat.Val[ M32_11 ];
        //        Val[ M33_15 ] = TmpMat.Val[ M33_15 ];

        return this;
    }

    /// <summary>
    /// Sets the matrix to an identity matrix.
    /// </summary>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 ToIdentity()
    {
        // Column 1
        Val[ M000 ] = 1f;
        Val[ M101 ] = 0f;
        Val[ M202 ] = 0f;
        Val[ M303 ] = 0f;

        // Column 2
        Val[ M014 ] = 0f;
        Val[ M115 ] = 1f;
        Val[ M216 ] = 0f;
        Val[ M317 ] = 0f;

        // Column 3
        Val[ M028 ]  = 0f;
        Val[ M129 ]  = 0f;
        Val[ M2210 ] = 1f;
        Val[ M3211 ] = 0f;

        // Column 4
        Val[ M0312 ] = 0f;
        Val[ M1313 ] = 0f;
        Val[ M2314 ] = 0f;
        Val[ M3315 ] = 1f;

        return this;
    }

    /// <summary>
    /// Inverts the matrix. Stores the result in this matrix.
    /// </summary>
    /// <returns> This matrix for the purpose of chaining methods together. </returns>
    /// <exception cref="RuntimeException"> if the matrix is singular (not invertible)  </exception>
    public Matrix4 Invert()
    {
        //@formatter:off
        float lDet = (Val[M303] * Val[M216] * Val[M129] * Val[M0312])
                   - (Val[M202] * Val[M317] * Val[M129] * Val[M0312])
                   - (Val[M303] * Val[M115] * Val[M2210] * Val[M0312])
                   + (Val[M101] * Val[M317] * Val[M2210] * Val[M0312])
                   + (Val[M202] * Val[M115] * Val[M3211] * Val[M0312])
                   - (Val[M101] * Val[M216] * Val[M3211] * Val[M0312])
                   - (Val[M303] * Val[M216] * Val[M028] * Val[M1313])
                   + (Val[M202] * Val[M317] * Val[M028] * Val[M1313])
                   + (Val[M303] * Val[M014] * Val[M2210] * Val[M1313])
                   - (Val[M000] * Val[M317] * Val[M2210] * Val[M1313])
                   - (Val[M202] * Val[M014] * Val[M3211] * Val[M1313])
                   + (Val[M000] * Val[M216] * Val[M3211] * Val[M1313])
                   + (Val[M303] * Val[M115] * Val[M028] * Val[M2314])
                   - (Val[M101] * Val[M317] * Val[M028] * Val[M2314])
                   - (Val[M303] * Val[M014] * Val[M129] * Val[M2314])
                   + (Val[M000] * Val[M317] * Val[M129] * Val[M2314])
                   + (Val[M101] * Val[M014] * Val[M3211] * Val[M2314])
                   - (Val[M000] * Val[M115] * Val[M3211] * Val[M2314])
                   - (Val[M202] * Val[M115] * Val[M028] * Val[M3315])
                   + (Val[M101] * Val[M216] * Val[M028] * Val[M3315])
                   + (Val[M202] * Val[M014] * Val[M129] * Val[M3315])
                   - (Val[M000] * Val[M216] * Val[M129] * Val[M3315])
                   - (Val[M101] * Val[M014] * Val[M2210] * Val[M3315])
                   + (Val[M000] * Val[M115] * Val[M2210] * Val[M3315]);
        //@formatter:on

        if ( lDet == 0f )
        {
            throw new RuntimeException( "non-invertible matrix" );
        }

        //@formatter:off
        float m00 = (Val[M129] * Val[M2314] * Val[M317])
                  - (Val[M1313] * Val[M2210] * Val[M317])
                  + (Val[M1313] * Val[M216] * Val[M3211])
                  - (Val[M115] * Val[M2314] * Val[M3211])
                  - (Val[M129] * Val[M216] * Val[M3315])
                  + (Val[M115] * Val[M2210] * Val[M3315]);

        float m01 = (Val[M0312] * Val[M2210] * Val[M317])
                  - (Val[M028] * Val[M2314] * Val[M317])
                  - (Val[M0312] * Val[M216] * Val[M3211])
                  + (Val[M014] * Val[M2314] * Val[M3211])
                  + (Val[M028] * Val[M216] * Val[M3315])
                  - (Val[M014] * Val[M2210] * Val[M3315]);

        float m02 = (Val[M028] * Val[M1313] * Val[M317])
                  - (Val[M0312] * Val[M129] * Val[M317])
                  + (Val[M0312] * Val[M115] * Val[M3211])
                  - (Val[M014] * Val[M1313] * Val[M3211])
                  - (Val[M028] * Val[M115] * Val[M3315])
                  + (Val[M014] * Val[M129] * Val[M3315]);

        float m03 = (Val[M0312] * Val[M129] * Val[M216])
                  - (Val[M028] * Val[M1313] * Val[M216])
                  - (Val[M0312] * Val[M115] * Val[M2210])
                  + (Val[M014] * Val[M1313] * Val[M2210])
                  + (Val[M028] * Val[M115] * Val[M2314])
                  - (Val[M014] * Val[M129] * Val[M2314]);

        float m10 = (Val[M1313] * Val[M2210] * Val[M303])
                  - (Val[M129] * Val[M2314] * Val[M303])
                  - (Val[M1313] * Val[M202] * Val[M3211])
                  + (Val[M101] * Val[M2314] * Val[M3211])
                  + (Val[M129] * Val[M202] * Val[M3315])
                  - (Val[M101] * Val[M2210] * Val[M3315]);

        float m11 = (Val[M028] * Val[M2314] * Val[M303])
                  - (Val[M0312] * Val[M2210] * Val[M303])
                  + (Val[M0312] * Val[M202] * Val[M3211])
                  - (Val[M000] * Val[M2314] * Val[M3211])
                  - (Val[M028] * Val[M202] * Val[M3315])
                  + (Val[M000] * Val[M2210] * Val[M3315]);

        float m12 = (Val[M0312] * Val[M129] * Val[M303])
                  - (Val[M028] * Val[M1313] * Val[M303])
                  - (Val[M0312] * Val[M101] * Val[M3211])
                  + (Val[M000] * Val[M1313] * Val[M3211])
                  + (Val[M028] * Val[M101] * Val[M3315])
                  - (Val[M000] * Val[M129] * Val[M3315]);

        float m13 = (Val[M028] * Val[M1313] * Val[M202])
                  - (Val[M0312] * Val[M129] * Val[M202])
                  + (Val[M0312] * Val[M101] * Val[M2210])
                  - (Val[M000] * Val[M1313] * Val[M2210])
                  - (Val[M028] * Val[M101] * Val[M2314])
                  + (Val[M000] * Val[M129] * Val[M2314]);

        float m20 = (Val[M115] * Val[M2314] * Val[M303])
                  - (Val[M1313] * Val[M216] * Val[M303])
                  + (Val[M1313] * Val[M202] * Val[M317])
                  - (Val[M101] * Val[M2314] * Val[M317])
                  - (Val[M115] * Val[M202] * Val[M3315])
                  + (Val[M101] * Val[M216] * Val[M3315]);

        float m21 = (Val[M0312] * Val[M216] * Val[M303])
                  - (Val[M014] * Val[M2314] * Val[M303])
                  - (Val[M0312] * Val[M202] * Val[M317])
                  + (Val[M000] * Val[M2314] * Val[M317])
                  + (Val[M014] * Val[M202] * Val[M3315])
                  - (Val[M000] * Val[M216] * Val[M3315]);

        float m22 = (Val[M014] * Val[M1313] * Val[M303])
                  - (Val[M0312] * Val[M115] * Val[M303])
                  + (Val[M0312] * Val[M101] * Val[M317])
                  - (Val[M000] * Val[M1313] * Val[M317])
                  - (Val[M014] * Val[M101] * Val[M3315])
                  + (Val[M000] * Val[M115] * Val[M3315]);

        float m23 = (Val[M0312] * Val[M115] * Val[M202])
                  - (Val[M014] * Val[M1313] * Val[M202])
                  - (Val[M0312] * Val[M101] * Val[M216])
                  + (Val[M000] * Val[M1313] * Val[M216])
                  + (Val[M014] * Val[M101] * Val[M2314])
                  - (Val[M000] * Val[M115] * Val[M2314]);

        float m30 = (Val[M129] * Val[M216] * Val[M303])
                  - (Val[M115] * Val[M2210] * Val[M303])
                  - (Val[M129] * Val[M202] * Val[M317])
                  + (Val[M101] * Val[M2210] * Val[M317])
                  + (Val[M115] * Val[M202] * Val[M3211])
                  - (Val[M101] * Val[M216] * Val[M3211]);

        float m31 = (Val[M014] * Val[M2210] * Val[M303])
                  - (Val[M028] * Val[M216] * Val[M303])
                  + (Val[M028] * Val[M202] * Val[M317])
                  - (Val[M000] * Val[M2210] * Val[M317])
                  - (Val[M014] * Val[M202] * Val[M3211])
                  + (Val[M000] * Val[M216] * Val[M3211]);

        float m32 = (Val[M028] * Val[M115] * Val[M303])
                  - (Val[M014] * Val[M129] * Val[M303])
                  - (Val[M028] * Val[M101] * Val[M317])
                  + (Val[M000] * Val[M129] * Val[M317])
                  + (Val[M014] * Val[M101] * Val[M3211])
                  - (Val[M000] * Val[M115] * Val[M3211]);

        float m33 = (Val[M014] * Val[M129] * Val[M202])
                  - (Val[M028] * Val[M115] * Val[M202])
                  + (Val[M028] * Val[M101] * Val[M216])
                  - (Val[M000] * Val[M129] * Val[M216])
                  - (Val[M014] * Val[M101] * Val[M2210])
                  + (Val[M000] * Val[M115] * Val[M2210]);
        //@formatter:on

        float invDet = 1.0f / lDet;

        Val[ M000 ]  = m00 * invDet;
        Val[ M101 ]  = m10 * invDet;
        Val[ M202 ]  = m20 * invDet;
        Val[ M303 ]  = m30 * invDet;
        Val[ M014 ]  = m01 * invDet;
        Val[ M115 ]  = m11 * invDet;
        Val[ M216 ]  = m21 * invDet;
        Val[ M317 ]  = m31 * invDet;
        Val[ M028 ]  = m02 * invDet;
        Val[ M129 ]  = m12 * invDet;
        Val[ M2210 ] = m22 * invDet;
        Val[ M3211 ] = m32 * invDet;
        Val[ M0312 ] = m03 * invDet;
        Val[ M1313 ] = m13 * invDet;
        Val[ M2314 ] = m23 * invDet;
        Val[ M3315 ] = m33 * invDet;

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public float Determinant()
    {
        //@formatter:off
        return (Val[M303] * Val[M216] * Val[M129] * Val[M0312])
             - (Val[M202] * Val[M317] * Val[M129] * Val[M0312])
             - (Val[M303] * Val[M115] * Val[M2210] * Val[M0312])
             + (Val[M101] * Val[M317] * Val[M2210] * Val[M0312])
             + (Val[M202] * Val[M115] * Val[M3211] * Val[M0312])
             - (Val[M101] * Val[M216] * Val[M3211] * Val[M0312])
             - (Val[M303] * Val[M216] * Val[M028] * Val[M1313])
             + (Val[M202] * Val[M317] * Val[M028] * Val[M1313])
             + (Val[M303] * Val[M014] * Val[M2210] * Val[M1313])
             - (Val[M000] * Val[M317] * Val[M2210] * Val[M1313])
             - (Val[M202] * Val[M014] * Val[M3211] * Val[M1313])
             + (Val[M000] * Val[M216] * Val[M3211] * Val[M1313])
             + (Val[M303] * Val[M115] * Val[M028] * Val[M2314])
             - (Val[M101] * Val[M317] * Val[M028] * Val[M2314])
             - (Val[M303] * Val[M014] * Val[M129] * Val[M2314])
             + (Val[M000] * Val[M317] * Val[M129] * Val[M2314])
             + (Val[M101] * Val[M014] * Val[M3211] * Val[M2314])
             - (Val[M000] * Val[M115] * Val[M3211] * Val[M2314])
             - (Val[M202] * Val[M115] * Val[M028] * Val[M3315])
             + (Val[M101] * Val[M216] * Val[M028] * Val[M3315])
             + (Val[M202] * Val[M014] * Val[M129] * Val[M3315])
             - (Val[M000] * Val[M216] * Val[M129] * Val[M3315])
             - (Val[M101] * Val[M014] * Val[M2210] * Val[M3315])
                + (Val[M000] * Val[M115] * Val[M2210] * Val[M3315]);
        //@formatter:on
    }

    /// <summary>
    /// Calculates the determinant of the upper-left 3x3 submatrix of the 4x4 matrix.
    /// </summary>
    /// <returns>The determinant of the 3x3 submatrix.</returns>
    public float Determinant3X3()
    {
        return ( Val[ M000 ] * Val[ M115 ] * Val[ M2210 ] )
             + ( Val[ M014 ] * Val[ M129 ] * Val[ M202 ] )
             + ( Val[ M028 ] * Val[ M101 ] * Val[ M216 ] )
             - ( Val[ M000 ] * Val[ M129 ] * Val[ M216 ] )
             - ( Val[ M014 ] * Val[ M101 ] * Val[ M2210 ] )
             - ( Val[ M028 ] * Val[ M115 ] * Val[ M202 ] );
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
        ToIdentity();

        var   lfd = ( float )( ( double )1.0f / Math.Tan( ( double )( fovy * ( Math.PI / 1800 ) ) / ( double )2.0f ) );
        float la1 = ( far + near ) / ( near - far );
        float la2 = 2.0f * far * near / ( near - far );

        Val[ M000 ] = lfd / aspectRatio;
        Val[ M101 ] = 0;
        Val[ M202 ] = 0;
        Val[ M303 ] = 0;

        Val[ M014 ] = 0;
        Val[ M115 ] = lfd; // scale y
        Val[ M216 ] = 0;
        Val[ M317 ] = 0;

        Val[ M028 ]  = 0;
        Val[ M129 ]  = 0;
        Val[ M2210 ] = la1;   // scale z
        Val[ M3211 ] = -1.0f; // perspective transform

        Val[ M0312 ] = 0;
        Val[ M1313 ] = 0;
        Val[ M2314 ] = la2; // z translation
        Val[ M3315 ] = 0;

        return this;
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
        Val[ M000 ] = 2.0f * near / ( right - left );
        Val[ M101 ] = 0;
        Val[ M202 ] = 0;
        Val[ M303 ] = 0;

        Val[ M014 ] = 0;
        Val[ M115 ] = 2.0f * near / ( top - bottom );
        Val[ M216 ] = 0;
        Val[ M317 ] = 0;

        Val[ M028 ]  = ( right + left ) / ( right - left );
        Val[ M129 ]  = ( top + bottom ) / ( top - bottom );
        Val[ M2210 ] = ( far + near ) / ( near - far );
        Val[ M3211 ] = -1;

        Val[ M0312 ] = 0;
        Val[ M1313 ] = 0;
        Val[ M2314 ] = 2 * far * near / ( near - far );
        Val[ M3315 ] = 0;

        return this;
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

        float xOrth = 2f / ( right - left );
        float yOrth = 2f / ( top - bottom );
        float zOrth = 2f / ( far - near );

        float tx = -( right + left ) / ( right - left );
        float ty = -( top + bottom ) / ( top - bottom );
        float tz = -( far + near ) / ( far - near );

        // Column 1
        Val[ M000 ] = xOrth;
        Val[ M101 ] = 0;
        Val[ M202 ] = 0;
        Val[ M303 ] = 0;

        // Column 2
        Val[ M014 ] = 0;
        Val[ M115 ] = yOrth;
        Val[ M216 ] = 0;
        Val[ M317 ] = 0;

        // Column 3
        Val[ M028 ]  = 0;
        Val[ M129 ]  = 0;
        Val[ M2210 ] = zOrth;
        Val[ M3211 ] = 0;

        // Column 4
        Val[ M0312 ] = tx;
        Val[ M1313 ] = ty;
        Val[ M2314 ] = tz;
        Val[ M3315 ] = 1;

        return this;
    }

    /// <summary>
    /// Sets the 4th column to the translation vector.
    /// </summary>
    /// <param name="vector"> The translation vector </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetTranslation( Vector3 vector )
    {
        Val[ M0312 ] = vector.X;
        Val[ M1313 ] = vector.Y;
        Val[ M2314 ] = vector.Z;

        return this;
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
        Val[ M0312 ] = x;
        Val[ M1313 ] = y;
        Val[ M2314 ] = z;

        return this;
    }

    /// <summary>
    /// Sets this matrix to a translation matrix, overwriting it first by an identity matrix
    /// and then setting the 4th column to the translation vector.
    /// </summary>
    /// <param name="vector"> The translation vector </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToTranslation( Vector3 vector )
    {
        ToIdentity();

        Val[ M0312 ] = vector.X;
        Val[ M1313 ] = vector.Y;
        Val[ M2314 ] = vector.Z;

        return this;
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
        ToIdentity();

        Val[ M0312 ] = x;
        Val[ M1313 ] = y;
        Val[ M2314 ] = z;

        return this;
    }

    /// <summary>
    /// Sets this matrix to a translation and scaling matrix by first overwriting it with an
    /// identity and then setting the translation vector in the 4th column and the scaling
    /// vector in the diagonal.
    /// </summary>
    /// <param name="translation"> The translation vector </param>
    /// <param name="scaling"> The scaling vector </param>
    /// <returns> This matrix for the purpose of chaining methods together.  </returns>
    public Matrix4 SetToTranslationAndScaling( Vector3 translation, Vector3 scaling )
    {
        ToIdentity();

        Val[ M0312 ] = translation.X;
        Val[ M1313 ] = translation.Y;
        Val[ M2314 ] = translation.Z;
        Val[ M000 ]  = scaling.X;
        Val[ M115 ]  = scaling.Y;
        Val[ M2210 ] = scaling.Z;

        return this;
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
        ToIdentity();

        Val[ M0312 ] = translationX;
        Val[ M1313 ] = translationY;
        Val[ M2314 ] = translationZ;
        Val[ M000 ]  = scalingX;
        Val[ M115 ]  = scalingY;
        Val[ M2210 ] = scalingZ;

        return this;
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
        ToIdentity();

        Val[ M000 ]  = vector.X;
        Val[ M115 ]  = vector.Y;
        Val[ M2210 ] = vector.Z;

        return this;
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
        ToIdentity();

        Val[ M000 ]  = x;
        Val[ M115 ]  = y;
        Val[ M2210 ] = z;

        return this;
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
        LVez.Set( direction ).Nor();
        LVex.Set( direction ).Crs( up ).Nor();
        LVey.Set( LVex ).Crs( LVez ).Nor();

        ToIdentity();

        Val[ M000 ]  = LVex.X;
        Val[ M014 ]  = LVex.Y;
        Val[ M028 ]  = LVex.Z;
        Val[ M101 ]  = LVey.X;
        Val[ M115 ]  = LVey.Y;
        Val[ M129 ]  = LVey.Z;
        Val[ M202 ]  = -LVez.X;
        Val[ M216 ]  = -LVez.Y;
        Val[ M2210 ] = -LVez.Z;

        return this;
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
        float w = 1.0f / t.Length;

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
        Val[ M000 ] = mat.Val[ M000 ];
        Val[ M101 ] = mat.Val[ M101 ];
        Val[ M202 ] = mat.Val[ M202 ];
        Val[ M303 ] = 0.0f;

        Val[ M014 ] = mat.Val[ M303 ];
        Val[ M115 ] = mat.Val[ M014 ];
        Val[ M216 ] = mat.Val[ M115 ];
        Val[ M317 ] = 0.0f;

        Val[ M028 ]  = 0.0f;
        Val[ M129 ]  = 0.0f;
        Val[ M2210 ] = 1.0f;
        Val[ M3211 ] = 0.0f;

        Val[ M0312 ] = mat.Val[ M216 ];
        Val[ M1313 ] = mat.Val[ M317 ];
        Val[ M2314 ] = 0.0f;
        Val[ M3315 ] = mat.Val[ M028 ];

        return this;
    }

    /// <summary>
    /// Sets this matrix to the given affine matrix. The values are mapped as follows:
    /// <para></para>
    /// <para>     [  M00  M01  ___  M02  ]</para>
    /// <para>     [  M10_1  M11_5  ___  M12_9  ]</para>
    /// <para>     [  ___  ___  ___  ___  ]</para>
    /// <para>     [  ___  ___  ___  ___  ]</para>
    /// <para></para>
    /// </summary>
    /// <param name="affine"> the source matrix </param>
    /// <returns> This matrix for chaining </returns>
    public Matrix4 Set( Affine2 affine )
    {
        Val[ M000 ] = affine.M00;
        Val[ M101 ] = affine.M10;
        Val[ M202 ] = 0.0f;
        Val[ M303 ] = 0.0f;

        Val[ M014 ] = affine.M01;
        Val[ M115 ] = affine.M11;
        Val[ M216 ] = 0.0f;
        Val[ M317 ] = 0.0f;

        Val[ M028 ]  = 0.0f;
        Val[ M129 ]  = 0.0f;
        Val[ M2210 ] = 1.0f;
        Val[ M3211 ] = 0.0f;

        Val[ M0312 ] = affine.M02;
        Val[ M1313 ] = affine.M12;
        Val[ M2314 ] = 0.0f;
        Val[ M3315 ] = 1.0f;

        return this;
    }

    /// <summary>
    /// Assumes that both matrices are 2D affine transformations, copying only
    /// the relevant components. The copied are mapped as follows:
    /// <para></para>
    /// <para>     [  M00  M01  ___  M02  ]</para>
    /// <para>     [  M10_1  M11_5  ___  M12_9  ]</para>
    /// <para>     [  ___  ___  ___  ___  ]</para>
    /// <para>     [  ___  ___  ___  ___  ]</para>
    /// <para></para>
    /// </summary>
    /// <param name="affine"> the source matrix </param>
    /// <returns> This matrix for chaining </returns>
    public Matrix4 SetAsAffine( Affine2 affine )
    {
        Val[ M000 ]  = affine.M00;
        Val[ M101 ]  = affine.M10;
        Val[ M014 ]  = affine.M01;
        Val[ M115 ]  = affine.M11;
        Val[ M0312 ] = affine.M02;
        Val[ M1313 ] = affine.M12;

        return this;
    }

    /// <summary>
    /// Assumes that both matrices are 2D affine transformations, copying only
    /// the relevant components. The copied values are:
    /// <para></para>
    /// <para>     [  M00  M01  ___  M03_12  ]</para>
    /// <para>     [  M10_1  M11_5  ___  M13_13  ]</para>
    /// <para>     [  ___  ___  ___  ___  ]</para>
    /// <para>     [  ___  ___  ___  ___  ]</para>
    /// <para></para>
    /// </summary>
    /// <param name="mat"> the source matrix </param>
    /// <returns> This matrix for chaining </returns>
    public Matrix4 SetAsAffine( Matrix4 mat )
    {
        Val[ M000 ]  = mat.Val[ M000 ];
        Val[ M101 ]  = mat.Val[ M101 ];
        Val[ M014 ]  = mat.Val[ M014 ];
        Val[ M115 ]  = mat.Val[ M115 ];
        Val[ M0312 ] = mat.Val[ M0312 ];
        Val[ M1313 ] = mat.Val[ M1313 ];

        return this;
    }

    public Matrix4 Scl( Vector3 scale )
    {
        Val[ M000 ]  *= scale.X;
        Val[ M115 ]  *= scale.Y;
        Val[ M2210 ] *= scale.Z;

        return this;
    }

    public Matrix4 Scl( float x, float y, float z )
    {
        Val[ M000 ]  *= x;
        Val[ M115 ]  *= y;
        Val[ M2210 ] *= z;

        return this;
    }

    public Matrix4 Scl( float scale )
    {
        Val[ M000 ]  *= scale;
        Val[ M115 ]  *= scale;
        Val[ M2210 ] *= scale;

        return this;
    }

    public Vector3 GetTranslation( Vector3 position )
    {
        position.X = Val[ M0312 ];
        position.Y = Val[ M1313 ];
        position.Z = Val[ M2314 ];

        return position;
    }

    /// <summary>
    /// Gets the rotation of this matrix.
    /// </summary>
    /// <param name="rotation"> The <see cref="Quaternion"/> to receive the rotation </param>
    /// <param name="normalizeAxes"> True to normalize the axes, necessary when the matrix might also include scaling. </param>
    /// <returns> The provided <see cref="Quaternion"/> for chaining.  </returns>
    public Quaternion GetRotation( Quaternion rotation, bool normalizeAxes )
    {
        return rotation.SetFromMatrix( normalizeAxes, this );
    }

    /// <summary>
    /// Gets the rotation of this matrix.
    /// </summary>
    /// <param name="rotation"> The <see cref="Quaternion"/> to receive the rotation </param>
    /// <returns> The provided <see cref="Quaternion"/> for chaining.  </returns>
    public Quaternion GetRotation( Quaternion rotation )
    {
        return rotation.SetFromMatrix( this );
    }

    /// <summary>
    /// </summary>
    /// <returns> the squared scale factor on the X axis </returns>
    public float GetScaleXSquared()
    {
        return ( Val[ M000 ] * Val[ M000 ] )
             + ( Val[ M014 ] * Val[ M014 ] )
             + ( Val[ M028 ] * Val[ M028 ] );
    }

    /// <summary>
    /// </summary>
    /// <returns> the squared scale factor on the Y axis </returns>
    public float GetScaleYSquared()
    {
        return ( Val[ M101 ] * Val[ M101 ] )
             + ( Val[ M115 ] * Val[ M115 ] )
             + ( Val[ M129 ] * Val[ M129 ] );
    }

    /// <summary>
    /// </summary>
    /// <returns> the squared scale factor on the Z axis </returns>
    public float GetScaleZSquared()
    {
        return ( Val[ M202 ] * Val[ M202 ] )
             + ( Val[ M216 ] * Val[ M216 ] )
             + ( Val[ M2210 ] * Val[ M2210 ] );
    }

    /// <summary>
    /// </summary>
    /// <returns> the scale factor on the X axis (non-negative) </returns>
    public float GetScaleX()
    {
        return MathUtils.IsZero( Val[ M014 ] ) && MathUtils.IsZero( Val[ M028 ] )
            ? Math.Abs( Val[ M000 ] )
            : ( float )Math.Sqrt( GetScaleXSquared() );
    }

    /// <summary>
    /// </summary>
    /// <returns> the scale factor on the Y axis (non-negative) </returns>
    public float GetScaleY()
    {
        return MathUtils.IsZero( Val[ M101 ] ) && MathUtils.IsZero( Val[ M129 ] )
            ? Math.Abs( Val[ M115 ] )
            : ( float )Math.Sqrt( GetScaleYSquared() );
    }

    /// <summary>
    /// </summary>
    /// <returns> the scale factor on the Z axis (non-negative) </returns>
    public float GetScaleZ()
    {
        return MathUtils.IsZero( Val[ M202 ] ) && MathUtils.IsZero( Val[ M216 ] )
            ? Math.Abs( Val[ M2210 ] )
            : ( float )Math.Sqrt( GetScaleZSquared() );
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
        Val[ M0312 ] = 0;
        Val[ M1313 ] = 0;
        Val[ M2314 ] = 0;

        return Invert().Transpose();
    }

    /// <summary>
    /// Multiplies the matrix mata with matrix matb, storing the result in mata.
    /// The arrays are assumed to hold 4x4 column major matrices as you can get
    /// from <see cref="Val"/>.
    /// This is the same as <see cref="Matrix4.Mul(Matrix4)"/>.
    /// </summary>
    /// <param name="mata"> the first matrix. </param>
    /// <param name="matb"> the second matrix.  </param>
    public static void Mul( float[] mata, float[] matb )
    {
        // Row 0
        float m00 = ( mata[ 0 ] * matb[ 0 ] ) + ( mata[ 4 ] * matb[ 1 ] ) + ( mata[ 8 ] * matb[ 2 ] )
                  + ( mata[ 12 ] * matb[ 3 ] );
        float m01 = ( mata[ 0 ] * matb[ 4 ] ) + ( mata[ 4 ] * matb[ 5 ] ) + ( mata[ 8 ] * matb[ 6 ] )
                  + ( mata[ 12 ] * matb[ 7 ] );
        float m02 = ( mata[ 0 ] * matb[ 8 ] ) + ( mata[ 4 ] * matb[ 9 ] ) + ( mata[ 8 ] * matb[ 10 ] )
                  + ( mata[ 12 ] * matb[ 11 ] );
        float m03 = ( mata[ 0 ] * matb[ 12 ] ) + ( mata[ 4 ] * matb[ 13 ] ) + ( mata[ 8 ] * matb[ 14 ] )
                  + ( mata[ 12 ] * matb[ 15 ] );

        // Row 1
        float m10 = ( mata[ 1 ] * matb[ 0 ] ) + ( mata[ 5 ] * matb[ 1 ] ) + ( mata[ 9 ] * matb[ 2 ] )
                  + ( mata[ 13 ] * matb[ 3 ] );
        float m11 = ( mata[ 1 ] * matb[ 4 ] ) + ( mata[ 5 ] * matb[ 5 ] ) + ( mata[ 9 ] * matb[ 6 ] )
                  + ( mata[ 13 ] * matb[ 7 ] );
        float m12 = ( mata[ 1 ] * matb[ 8 ] ) + ( mata[ 5 ] * matb[ 9 ] ) + ( mata[ 9 ] * matb[ 10 ] )
                  + ( mata[ 13 ] * matb[ 11 ] );
        float m13 = ( mata[ 1 ] * matb[ 12 ] ) + ( mata[ 5 ] * matb[ 13 ] ) + ( mata[ 9 ] * matb[ 14 ] )
                  + ( mata[ 13 ] * matb[ 15 ] );

        // Row 2
        float m20 = ( mata[ 2 ] * matb[ 0 ] ) + ( mata[ 6 ] * matb[ 1 ] ) + ( mata[ 10 ] * matb[ 2 ] )
                  + ( mata[ 14 ] * matb[ 3 ] );
        float m21 = ( mata[ 2 ] * matb[ 4 ] ) + ( mata[ 6 ] * matb[ 5 ] ) + ( mata[ 10 ] * matb[ 6 ] )
                  + ( mata[ 14 ] * matb[ 7 ] );
        float m22 = ( mata[ 2 ] * matb[ 8 ] ) + ( mata[ 6 ] * matb[ 9 ] ) + ( mata[ 10 ] * matb[ 10 ] )
                  + ( mata[ 14 ] * matb[ 11 ] );
        float m23 = ( mata[ 2 ] * matb[ 12 ] ) + ( mata[ 6 ] * matb[ 13 ] ) + ( mata[ 10 ] * matb[ 14 ] )
                  + ( mata[ 14 ] * matb[ 15 ] );

        // Row 3
        float m30 = ( mata[ 3 ] * matb[ 0 ] ) + ( mata[ 7 ] * matb[ 1 ] ) + ( mata[ 11 ] * matb[ 2 ] )
                  + ( mata[ 15 ] * matb[ 3 ] );
        float m31 = ( mata[ 3 ] * matb[ 4 ] ) + ( mata[ 7 ] * matb[ 5 ] ) + ( mata[ 11 ] * matb[ 6 ] )
                  + ( mata[ 15 ] * matb[ 7 ] );
        float m32 = ( mata[ 3 ] * matb[ 8 ] ) + ( mata[ 7 ] * matb[ 9 ] ) + ( mata[ 11 ] * matb[ 10 ] )
                  + ( mata[ 15 ] * matb[ 11 ] );
        float m33 = ( mata[ 3 ] * matb[ 12 ] ) + ( mata[ 7 ] * matb[ 13 ] ) + ( mata[ 11 ] * matb[ 14 ] )
                  + ( mata[ 15 ] * matb[ 15 ] );

        mata[ 0 ]  = m00;
        mata[ 4 ]  = m01;
        mata[ 8 ]  = m02;
        mata[ 12 ] = m03;
        mata[ 1 ]  = m10;
        mata[ 5 ]  = m11;
        mata[ 9 ]  = m12;
        mata[ 13 ] = m13;
        mata[ 2 ]  = m20;
        mata[ 6 ]  = m21;
        mata[ 10 ] = m22;
        mata[ 14 ] = m23;
        mata[ 3 ]  = m30;
        mata[ 7 ]  = m31;
        mata[ 11 ] = m32;
        mata[ 15 ] = m33;
    }
//    public static void Mul( float[] mata, float[] matb )
//    {
//        var m00 = ( mata[ M00_0 ] * matb[ M00_0 ] )
//                + ( mata[ M01_4 ] * matb[ M10_1 ] )
//                + ( mata[ M02_8 ] * matb[ M20_2 ] )
//                + ( mata[ M03_12 ] * matb[ M30_3 ] );
//
//        var m01 = ( mata[ M00_0 ] * matb[ M01_4 ] )
//                + ( mata[ M01_4 ] * matb[ M11_5 ] )
//                + ( mata[ M02_8 ] * matb[ M21_6 ] )
//                + ( mata[ M03_12 ] * matb[ M31_7 ] );
//
//        var m02 = ( mata[ M00_0 ] * matb[ M02_8 ] )
//                + ( mata[ M01_4 ] * matb[ M12_9 ] )
//                + ( mata[ M02_8 ] * matb[ M22_10 ] )
//                + ( mata[ M03_12 ] * matb[ M32_11 ] );
//
//        var m03 = ( mata[ M00_0 ] * matb[ M03_12 ] )
//                + ( mata[ M01_4 ] * matb[ M13_13 ] )
//                + ( mata[ M02_8 ] * matb[ M23_14 ] )
//                + ( mata[ M03_12 ] * matb[ M33_15 ] );
//
//        var m10 = ( mata[ M10_1 ] * matb[ M00_0 ] )
//                + ( mata[ M11_5 ] * matb[ M10_1 ] )
//                + ( mata[ M12_9 ] * matb[ M20_2 ] )
//                + ( mata[ M13_13 ] * matb[ M30_3 ] );
//
//        var m11 = ( mata[ M10_1 ] * matb[ M01_4 ] )
//                + ( mata[ M11_5 ] * matb[ M11_5 ] )
//                + ( mata[ M12_9 ] * matb[ M21_6 ] )
//                + ( mata[ M13_13 ] * matb[ M31_7 ] );
//
//        var m12 = ( mata[ M10_1 ] * matb[ M02_8 ] )
//                + ( mata[ M11_5 ] * matb[ M12_9 ] )
//                + ( mata[ M12_9 ] * matb[ M22_10 ] )
//                + ( mata[ M13_13 ] * matb[ M32_11 ] );
//
//        var m13 = ( mata[ M10_1 ] * matb[ M03_12 ] )
//                + ( mata[ M11_5 ] * matb[ M13_13 ] )
//                + ( mata[ M12_9 ] * matb[ M23_14 ] )
//                + ( mata[ M13_13 ] * matb[ M33_15 ] );
//
//        var m20 = ( mata[ M20_2 ] * matb[ M00_0 ] )
//                + ( mata[ M21_6 ] * matb[ M10_1 ] )
//                + ( mata[ M22_10 ] * matb[ M20_2 ] )
//                + ( mata[ M23_14 ] * matb[ M30_3 ] );
//
//        var m21 = ( mata[ M20_2 ] * matb[ M01_4 ] )
//                + ( mata[ M21_6 ] * matb[ M11_5 ] )
//                + ( mata[ M22_10 ] * matb[ M21_6 ] )
//                + ( mata[ M23_14 ] * matb[ M31_7 ] );
//
//        var m22 = ( mata[ M20_2 ] * matb[ M02_8 ] )
//                + ( mata[ M21_6 ] * matb[ M12_9 ] )
//                + ( mata[ M22_10 ] * matb[ M22_10 ] )
//                + ( mata[ M23_14 ] * matb[ M32_11 ] );
//
//        var m23 = ( mata[ M20_2 ] * matb[ M03_12 ] )
//                + ( mata[ M21_6 ] * matb[ M13_13 ] )
//                + ( mata[ M22_10 ] * matb[ M23_14 ] )
//                + ( mata[ M23_14 ] * matb[ M33_15 ] );
//
//        var m30 = ( mata[ M30_3 ] * matb[ M00_0 ] )
//                + ( mata[ M31_7 ] * matb[ M10_1 ] )
//                + ( mata[ M32_11 ] * matb[ M20_2 ] )
//                + ( mata[ M33_15 ] * matb[ M30_3 ] );
//
//        var m31 = ( mata[ M30_3 ] * matb[ M01_4 ] )
//                + ( mata[ M31_7 ] * matb[ M11_5 ] )
//                + ( mata[ M32_11 ] * matb[ M21_6 ] )
//                + ( mata[ M33_15 ] * matb[ M31_7 ] );
//
//        var m32 = ( mata[ M30_3 ] * matb[ M02_8 ] )
//                + ( mata[ M31_7 ] * matb[ M12_9 ] )
//                + ( mata[ M32_11 ] * matb[ M22_10 ] )
//                + ( mata[ M33_15 ] * matb[ M32_11 ] );
//
//        var m33 = ( mata[ M30_3 ] * matb[ M03_12 ] )
//                + ( mata[ M31_7 ] * matb[ M13_13 ] )
//                + ( mata[ M32_11 ] * matb[ M23_14 ] )
//                + ( mata[ M33_15 ] * matb[ M33_15 ] );
//
//        mata[ M00_0 ] = m00;
//        mata[ M10_1 ] = m10;
//        mata[ M20_2 ] = m20;
//        mata[ M30_3 ] = m30;
//
//        // ---------------
//        mata[ M01_4 ] = m01;
//        mata[ M11_5 ] = m11;
//        mata[ M21_6 ] = m21;
//        mata[ M31_7 ] = m31;
//
//        // ---------------
//        mata[ M02_8 ]  = m02;
//        mata[ M12_9 ]  = m12;
//        mata[ M22_10 ] = m22;
//        mata[ M32_11 ] = m32;
//
//        // ---------------
//        mata[ M03_12 ] = m03;
//        mata[ M13_13 ] = m13;
//        mata[ M23_14 ] = m23;
//        mata[ M33_15 ] = m33;
//    }

    /// <summary>
    /// Multiplies the vector with the given matrix. The matrix array is assumed
    /// to hold a 4x4 column major matrix as you can get from <see cref="Val"/>.
    /// The vector array is assumed to hold a 3-component vector, with x being the
    /// first element, y being the second and z being the last component. The result
    /// is stored in the vector array. This is the same as <see cref="Vector3.Mul(Matrix4)"/>.
    /// </summary>
    /// <param name="mat"> the matrix </param>
    /// <param name="vec"> the vector.  </param>
    public static void MulVec( float[] mat, float[] vec )
    {
        float x = ( vec[ 0 ] * mat[ M000 ] ) + ( vec[ 1 ] * mat[ M014 ] ) + ( vec[ 2 ] * mat[ M028 ] )
                + mat[ M0312 ];
        float y = ( vec[ 0 ] * mat[ M101 ] ) + ( vec[ 1 ] * mat[ M115 ] ) + ( vec[ 2 ] * mat[ M129 ] )
                + mat[ M1313 ];
        float z = ( vec[ 0 ] * mat[ M202 ] ) + ( vec[ 1 ] * mat[ M216 ] ) + ( vec[ 2 ] * mat[ M2210 ] ) +
                  mat[ M2314 ];

        vec[ 0 ] = x;
        vec[ 1 ] = y;
        vec[ 2 ] = z;
    }

    /// <summary>
    /// Multiplies the vector with the given matrix, performing a division by w. The
    /// matrix array is assumed to hold a 4x4 column major matrix as you can get from
    /// <see cref="Val"/>. The vector array is assumed to hold a 3-component
    /// vector, with x being the first element, y being the second and z being the last
    /// component. The result is stored in the vector array. This is the same as
    /// <see cref="Vector3.Prj(Matrix4)"/>.
    /// </summary>
    /// <param name="mat"> the matrix </param>
    /// <param name="vec"> the vector.  </param>
    public static void Prj( float[] mat, float[] vec )
    {
        float invW = 1.0f / ( ( vec[ 0 ] * mat[ M303 ] )
                            + ( vec[ 1 ] * mat[ M317 ] )
                            + ( vec[ 2 ] * mat[ M3211 ] )
                            + mat[ M3315 ] );

        float x = ( ( vec[ 0 ] * mat[ M000 ] )
                  + ( vec[ 1 ] * mat[ M014 ] )
                  + ( vec[ 2 ] * mat[ M028 ] )
                  + mat[ M0312 ] ) * invW;

        float y = ( ( vec[ 0 ] * mat[ M101 ] )
                  + ( vec[ 1 ] * mat[ M115 ] )
                  + ( vec[ 2 ] * mat[ M129 ] )
                  + mat[ M1313 ] ) * invW;

        float z = ( ( vec[ 0 ] * mat[ M202 ] )
                  + ( vec[ 1 ] * mat[ M216 ] )
                  + ( vec[ 2 ] * mat[ M2210 ] )
                  + mat[ M2314 ] ) * invW;

        vec[ 0 ] = x;
        vec[ 1 ] = y;
        vec[ 2 ] = z;
    }

    /// <summary>
    /// Multiplies the vector with the top most 3x3 sub-matrix of the given matrix.
    /// The matrix array is assumed to hold a 4x4 column major matrix as you can get
    /// from <see cref="Val"/>. The vector array is assumed to hold a
    /// 3-component vector, with x being the first element, y being the second and z
    /// being the last component. The result is stored in the vector array. This is the
    /// same as <see cref="Vector3.Rot(Matrix4)"/>.
    /// </summary>
    /// <param name="mat"> the matrix </param>
    /// <param name="vec"> the vector.  </param>
    public static void Rot( float[] mat, float[] vec )
    {
        float x = ( vec[ 0 ] * mat[ M000 ] ) + ( vec[ 1 ] * mat[ M014 ] ) + ( vec[ 2 ] * mat[ M028 ] );
        float y = ( vec[ 0 ] * mat[ M101 ] ) + ( vec[ 1 ] * mat[ M115 ] ) + ( vec[ 2 ] * mat[ M129 ] );
        float z = ( vec[ 0 ] * mat[ M202 ] ) + ( vec[ 1 ] * mat[ M216 ] ) + ( vec[ 2 ] * mat[ M2210 ] );
        vec[ 0 ] = x;
        vec[ 1 ] = y;
        vec[ 2 ] = z;
    }

    /// <summary>
    /// Computes the inverse of the given matrix. The matrix array is assumed to
    /// hold a 4x4 column major matrix as you can get from <see cref="Val"/>.
    /// </summary>
    /// <param name="values"> the matrix values. </param>
    /// <returns> false in case the inverse could not be calculated, true otherwise.  </returns>
    public static bool Invert( float[] values )
    {
        float lDet = Determinant( values );

        if ( lDet == 0 )
        {
            return false;
        }

        //@formatter:off
        float m00 = (values[M129] * values[M2314] * values[M317])
                  - (values[M1313] * values[M2210] * values[M317])
                  + (values[M1313] * values[M216] * values[M3211])
                  - (values[M115] * values[M2314] * values[M3211])
                  - (values[M129] * values[M216] * values[M3315])
                  + (values[M115] * values[M2210] * values[M3315]);

        float m01 = (values[M0312] * values[M2210] * values[M317])
                  - (values[M028] * values[M2314] * values[M317])
                  - (values[M0312] * values[M216] * values[M3211])
                  + (values[M014] * values[M2314] * values[M3211])
                  + (values[M028] * values[M216] * values[M3315])
                  - (values[M014] * values[M2210] * values[M3315]);

        float m02 = (values[M028] * values[M1313] * values[M317])
                  - (values[M0312] * values[M129] * values[M317])
                  + (values[M0312] * values[M115] * values[M3211])
                  - (values[M014] * values[M1313] * values[M3211])
                  - (values[M028] * values[M115] * values[M3315])
                  + (values[M014] * values[M129] * values[M3315]);

        float m03 = (values[M0312] * values[M129] * values[M216])
                  - (values[M028] * values[M1313] * values[M216])
                  - (values[M0312] * values[M115] * values[M2210])
                  + (values[M014] * values[M1313] * values[M2210])
                  + (values[M028] * values[M115] * values[M2314])
                  - (values[M014] * values[M129] * values[M2314]);

        float m10 = (values[M1313] * values[M2210] * values[M303])
                  - (values[M129] * values[M2314] * values[M303])
                  - (values[M1313] * values[M202] * values[M3211])
                  + (values[M101] * values[M2314] * values[M3211])
                  + (values[M129] * values[M202] * values[M3315])
                  - (values[M101] * values[M2210] * values[M3315]);

        float m11 = (values[M028] * values[M2314] * values[M303])
                  - (values[M0312] * values[M2210] * values[M303])
                  + (values[M0312] * values[M202] * values[M3211])
                  - (values[M000] * values[M2314] * values[M3211])
                  - (values[M028] * values[M202] * values[M3315])
                  + (values[M000] * values[M2210] * values[M3315]);

        float m12 = (values[M0312] * values[M129] * values[M303])
                  - (values[M028] * values[M1313] * values[M303])
                  - (values[M0312] * values[M101] * values[M3211])
                  + (values[M000] * values[M1313] * values[M3211])
                  + (values[M028] * values[M101] * values[M3315])
                  - (values[M000] * values[M129] * values[M3315]);

        float m13 = (values[M028] * values[M1313] * values[M202])
                  - (values[M0312] * values[M129] * values[M202])
                  + (values[M0312] * values[M101] * values[M2210])
                  - (values[M000] * values[M1313] * values[M2210])
                  - (values[M028] * values[M101] * values[M2314])
                  + (values[M000] * values[M129] * values[M2314]);

        float m20 = (values[M115] * values[M2314] * values[M303])
                  - (values[M1313] * values[M216] * values[M303])
                  + (values[M1313] * values[M202] * values[M317])
                  - (values[M101] * values[M2314] * values[M317])
                  - (values[M115] * values[M202] * values[M3315])
                  + (values[M101] * values[M216] * values[M3315]);

        float m21 = (values[M0312] * values[M216] * values[M303])
                  - (values[M014] * values[M2314] * values[M303])
                  - (values[M0312] * values[M202] * values[M317])
                  + (values[M000] * values[M2314] * values[M317])
                  + (values[M014] * values[M202] * values[M3315])
                  - (values[M000] * values[M216] * values[M3315]);

        float m22 = (values[M014] * values[M1313] * values[M303])
                  - (values[M0312] * values[M115] * values[M303])
                  + (values[M0312] * values[M101] * values[M317])
                  - (values[M000] * values[M1313] * values[M317])
                  - (values[M014] * values[M101] * values[M3315])
                  + (values[M000] * values[M115] * values[M3315]);

        float m23 = (values[M0312] * values[M115] * values[M202])
                  - (values[M014] * values[M1313] * values[M202])
                  - (values[M0312] * values[M101] * values[M216])
                  + (values[M000] * values[M1313] * values[M216])
                  + (values[M014] * values[M101] * values[M2314])
                  - (values[M000] * values[M115] * values[M2314]);

        float m30 = (values[M129] * values[M216] * values[M303])
                  - (values[M115] * values[M2210] * values[M303])
                  - (values[M129] * values[M202] * values[M317])
                  + (values[M101] * values[M2210] * values[M317])
                  + (values[M115] * values[M202] * values[M3211])
                  - (values[M101] * values[M216] * values[M3211]);

        float m31 = (values[M014] * values[M2210] * values[M303])
                  - (values[M028] * values[M216] * values[M303])
                  + (values[M028] * values[M202] * values[M317])
                  - (values[M000] * values[M2210] * values[M317])
                  - (values[M014] * values[M202] * values[M3211])
                  + (values[M000] * values[M216] * values[M3211]);

        float m32 = (values[M028] * values[M115] * values[M303])
                  - (values[M014] * values[M129] * values[M303])
                  - (values[M028] * values[M101] * values[M317])
                  + (values[M000] * values[M129] * values[M317])
                  + (values[M014] * values[M101] * values[M3211])
                  - (values[M000] * values[M115] * values[M3211]);

        float m33 = (values[M014] * values[M129] * values[M202])
                  - (values[M028] * values[M115] * values[M202])
                  + (values[M028] * values[M101] * values[M216])
                  - (values[M000] * values[M129] * values[M216])
                  - (values[M014] * values[M101] * values[M2210])
                  + (values[M000] * values[M115] * values[M2210]);
        //@formatter:on

        float invDet = 1.0f / lDet;
        values[ M000 ]  = m00 * invDet;
        values[ M101 ]  = m10 * invDet;
        values[ M202 ]  = m20 * invDet;
        values[ M303 ]  = m30 * invDet;
        values[ M014 ]  = m01 * invDet;
        values[ M115 ]  = m11 * invDet;
        values[ M216 ]  = m21 * invDet;
        values[ M317 ]  = m31 * invDet;
        values[ M028 ]  = m02 * invDet;
        values[ M129 ]  = m12 * invDet;
        values[ M2210 ] = m22 * invDet;
        values[ M3211 ] = m32 * invDet;
        values[ M0312 ] = m03 * invDet;
        values[ M1313 ] = m13 * invDet;
        values[ M2314 ] = m23 * invDet;
        values[ M3315 ] = m33 * invDet;

        return true;
    }

    /// <summary>
    /// Computes the determinante of the given matrix. The matrix array is assumed
    /// to hold a 4x4 column major matrix as you can get from <see cref="Val"/>.
    /// </summary>
    /// <param name="values"> the matrix values. </param>
    /// <returns> the determinante.  </returns>
    public static float Determinant( float[] values )
    {
        //@formatter:off
        // BE VERY CAREFUL WITH EDITING THIS!!!!
        // --------------------------------------------------------------  
        return (values[M303] * values[M216] * values[M129] * values[M0312])
             - (values[M202] * values[M317] * values[M129] * values[M0312])
             - (values[M303] * values[M115] * values[M2210] * values[M0312])
               // --------------------------------------------------------------  
             + (values[M101] * values[M317] * values[M2210] * values[M0312])
             + (values[M202] * values[M115] * values[M3211] * values[M0312])
               // --------------------------------------------------------------  
             - (values[M101] * values[M216] * values[M3211] * values[M0312])
             - (values[M303] * values[M216] * values[M028] * values[M1313])
               // --------------------------------------------------------------  
             + (values[M202] * values[M317] * values[M028] * values[M1313])
             + (values[M303] * values[M014] * values[M2210] * values[M1313])
               // --------------------------------------------------------------  
             - (values[M000] * values[M317] * values[M2210] * values[M1313])
             - (values[M202] * values[M014] * values[M3211] * values[M1313])
               // --------------------------------------------------------------  
             + (values[M000] * values[M216] * values[M3211] * values[M1313])
             + (values[M303] * values[M115] * values[M028] * values[M2314])
               // --------------------------------------------------------------  
             - (values[M101] * values[M317] * values[M028] * values[M2314])
             - (values[M303] * values[M014] * values[M129] * values[M2314])
               // --------------------------------------------------------------  
             + (values[M000] * values[M317] * values[M129] * values[M2314])
             + (values[M101] * values[M014] * values[M3211] * values[M2314])
               // --------------------------------------------------------------  
             - (values[M000] * values[M115] * values[M3211] * values[M2314])
             - (values[M202] * values[M115] * values[M028] * values[M3315])
               // --------------------------------------------------------------  
             + (values[M101] * values[M216] * values[M028] * values[M3315])
             + (values[M202] * values[M014] * values[M129] * values[M3315])
               // --------------------------------------------------------------  
             - (values[M000] * values[M216] * values[M129] * values[M3315])
             - (values[M101] * values[M014] * values[M2210] * values[M3315])
               // --------------------------------------------------------------  
             + (values[M000] * values[M115] * values[M2210] * values[M3315]);
        //@formatter:on
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
        Val[ M0312 ] += ( Val[ M000 ] * x ) + ( Val[ M014 ] * y ) + ( Val[ M028 ] * z );
        Val[ M1313 ] += ( Val[ M101 ] * x ) + ( Val[ M115 ] * y ) + ( Val[ M129 ] * z );
        Val[ M2314 ] += ( Val[ M202 ] * x ) + ( Val[ M216 ] * y ) + ( Val[ M2210 ] * z );
        Val[ M3315 ] += ( Val[ M303 ] * x ) + ( Val[ M317 ] * y ) + ( Val[ M3211 ] * z );

        return this;
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
        if ( degrees == 0.0f )
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
        float x = rotation.X;
        float y = rotation.Y;
        float z = rotation.Z;
        float w = rotation.W;

        float xx = x * x;
        float xy = x * y;
        float xz = x * z;
        float xw = x * w;
        float yy = y * y;
        float yz = y * z;
        float yw = y * w;
        float zz = z * z;
        float zw = z * w;

        // Set matrix from quaternion
        float r00 = 1 - ( 2 * ( yy + zz ) );
        float r01 = 2 * ( xy - zw );
        float r02 = 2 * ( xz + yw );
        float r10 = 2 * ( xy + zw );
        float r11 = 1 - ( 2 * ( xx + zz ) );
        float r12 = 2 * ( yz - xw );
        float r20 = 2 * ( xz - yw );
        float r21 = 2 * ( yz + xw );
        float r22 = 1 - ( 2 * ( xx + yy ) );

        float m00 = ( Val[ M000 ] * r00 ) + ( Val[ M014 ] * r10 ) + ( Val[ M028 ] * r20 );
        float m01 = ( Val[ M000 ] * r01 ) + ( Val[ M014 ] * r11 ) + ( Val[ M028 ] * r21 );
        float m02 = ( Val[ M000 ] * r02 ) + ( Val[ M014 ] * r12 ) + ( Val[ M028 ] * r22 );
        float m10 = ( Val[ M101 ] * r00 ) + ( Val[ M115 ] * r10 ) + ( Val[ M129 ] * r20 );
        float m11 = ( Val[ M101 ] * r01 ) + ( Val[ M115 ] * r11 ) + ( Val[ M129 ] * r21 );
        float m12 = ( Val[ M101 ] * r02 ) + ( Val[ M115 ] * r12 ) + ( Val[ M129 ] * r22 );
        float m20 = ( Val[ M202 ] * r00 ) + ( Val[ M216 ] * r10 ) + ( Val[ M2210 ] * r20 );
        float m21 = ( Val[ M202 ] * r01 ) + ( Val[ M216 ] * r11 ) + ( Val[ M2210 ] * r21 );
        float m22 = ( Val[ M202 ] * r02 ) + ( Val[ M216 ] * r12 ) + ( Val[ M2210 ] * r22 );
        float m30 = ( Val[ M303 ] * r00 ) + ( Val[ M317 ] * r10 ) + ( Val[ M3211 ] * r20 );
        float m31 = ( Val[ M303 ] * r01 ) + ( Val[ M317 ] * r11 ) + ( Val[ M3211 ] * r21 );
        float m32 = ( Val[ M303 ] * r02 ) + ( Val[ M317 ] * r12 ) + ( Val[ M3211 ] * r22 );

        Val[ M000 ] = m00;
        Val[ M101 ] = m10;
        Val[ M202 ] = m20;
        Val[ M303 ] = m30;

        // ----------------------------
        Val[ M014 ] = m01;
        Val[ M115 ] = m11;
        Val[ M216 ] = m21;
        Val[ M317 ] = m31;

        // ----------------------------
        Val[ M028 ]  = m02;
        Val[ M129 ]  = m12;
        Val[ M2210 ] = m22;
        Val[ M3211 ] = m32;

        return this;
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
        LVez.Set( direction ).Nor();
        LVex.Set( direction ).Crs( up ).Nor();
        LVey.Set( LVex ).Crs( LVez ).Nor();

        float m00 = ( Val[ M000 ] * LVex.X ) + ( Val[ M014 ] * LVex.Y ) + ( Val[ M028 ] * LVex.Z );
        float m01 = ( Val[ M000 ] * LVey.X ) + ( Val[ M014 ] * LVey.Y ) + ( Val[ M028 ] * LVey.Z );
        float m02 = ( Val[ M000 ] * -LVez.X ) + ( Val[ M014 ] * -LVez.Y ) + ( Val[ M028 ] * -LVez.Z );
        float m10 = ( Val[ M101 ] * LVex.X ) + ( Val[ M115 ] * LVex.Y ) + ( Val[ M129 ] * LVex.Z );
        float m11 = ( Val[ M101 ] * LVey.X ) + ( Val[ M115 ] * LVey.Y ) + ( Val[ M129 ] * LVey.Z );
        float m12 = ( Val[ M101 ] * -LVez.X ) + ( Val[ M115 ] * -LVez.Y ) + ( Val[ M129 ] * -LVez.Z );
        float m20 = ( Val[ M202 ] * LVex.X ) + ( Val[ M216 ] * LVex.Y ) + ( Val[ M2210 ] * LVex.Z );
        float m21 = ( Val[ M202 ] * LVey.X ) + ( Val[ M216 ] * LVey.Y ) + ( Val[ M2210 ] * LVey.Z );
        float m22 = ( Val[ M202 ] * -LVez.X ) + ( Val[ M216 ] * -LVez.Y ) + ( Val[ M2210 ] * -LVez.Z );
        float m30 = ( Val[ M303 ] * LVex.X ) + ( Val[ M317 ] * LVex.Y ) + ( Val[ M3211 ] * LVex.Z );
        float m31 = ( Val[ M303 ] * LVey.X ) + ( Val[ M317 ] * LVey.Y ) + ( Val[ M3211 ] * LVey.Z );
        float m32 = ( Val[ M303 ] * -LVez.X ) + ( Val[ M317 ] * -LVez.Y ) + ( Val[ M3211 ] * -LVez.Z );

        Val[ M000 ] = m00;
        Val[ M101 ] = m10;
        Val[ M202 ] = m20;
        Val[ M303 ] = m30;

        // ----------------------------
        Val[ M014 ] = m01;
        Val[ M115 ] = m11;
        Val[ M216 ] = m21;
        Val[ M317 ] = m31;

        // ----------------------------
        Val[ M028 ]  = m02;
        Val[ M129 ]  = m12;
        Val[ M2210 ] = m22;
        Val[ M3211 ] = m32;

        return this;
    }

    /// <summary>
    /// Post-multiplies this matrix by a rotation toward a target.
    /// </summary>
    /// <param name="target"> the target to rotate to </param>
    /// <param name="up"> the up vector </param>
    /// <returns> This matrix for chaining  </returns>
    public Matrix4 RotateTowardTarget( Vector3 target, Vector3 up )
    {
        TmpVec.Set( target.X - Val[ M0312 ], target.Y - Val[ M1313 ], target.Z - Val[ M2314 ] );

        return RotateTowardDirection( TmpVec, up );
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
        Val[ M000 ] *= scaleX;
        Val[ M014 ] *= scaleY;
        Val[ M028 ] *= scaleZ;

        // ----------------------------
        Val[ M101 ] *= scaleX;
        Val[ M115 ] *= scaleY;
        Val[ M129 ] *= scaleZ;

        // ----------------------------
        Val[ M202 ]  *= scaleX;
        Val[ M216 ]  *= scaleY;
        Val[ M2210 ] *= scaleZ;

        // ----------------------------
        Val[ M303 ]  *= scaleX;
        Val[ M317 ]  *= scaleY;
        Val[ M3211 ] *= scaleZ;

        return this;
    }

    /// <summary>
    /// Copies the 4x3 upper-left sub-matrix into float array. The destination
    /// array is supposed to be a column major matrix.
    /// </summary>
    /// <param name="dst"> the destination matrix </param>
    public void Extract4X3Matrix( ref float[] dst )
    {
        dst[ 0 ]  = Val[ M000 ];
        dst[ 1 ]  = Val[ M101 ];
        dst[ 2 ]  = Val[ M202 ];
        dst[ 3 ]  = Val[ M014 ];
        dst[ 4 ]  = Val[ M115 ];
        dst[ 5 ]  = Val[ M216 ];
        dst[ 6 ]  = Val[ M028 ];
        dst[ 7 ]  = Val[ M129 ];
        dst[ 8 ]  = Val[ M2210 ];
        dst[ 9 ]  = Val[ M0312 ];
        dst[ 10 ] = Val[ M1313 ];
        dst[ 11 ] = Val[ M2314 ];
    }

    /// <summary>
    /// </summary>
    /// <returns>True if this matrix has any rotation or scaling, false otherwise </returns>
    public bool HasRotationOrScaling()
    {
        return !( MathUtils.IsEqual( Val[ M000 ], 1 )
               && MathUtils.IsEqual( Val[ M115 ], 1 )
               && MathUtils.IsEqual( Val[ M2210 ], 1 )
               && MathUtils.IsZero( Val[ M014 ] )
               && MathUtils.IsZero( Val[ M028 ] )
               && MathUtils.IsZero( Val[ M101 ] )
               && MathUtils.IsZero( Val[ M129 ] )
               && MathUtils.IsZero( Val[ M202 ] )
               && MathUtils.IsZero( Val[ M216 ] ) );
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Multiplies the vectors with the given matrix. The matrix array is assumed to hold a 4x4
    /// column major matrix as you can get from <see cref="Val"/>. The vectors array is assumed
    /// to hold 3-component vectors. Offset specifies the offset into the array where the x-component
    /// of the first vector is located. The numVecs parameter specifies the number of vectors stored
    /// in the vectors array. The stride parameter specifies the number of floats between subsequent
    /// vectors and must be >= 3. This is the same as <see cref="Vector3.Mul(Matrix4)"/> applied to
    /// multiple vectors.
    /// </summary>
    /// <param name="mat"> the matrix </param>
    /// <param name="vecs"> the vectors </param>
    /// <param name="offset"> the offset into the vectors array </param>
    /// <param name="numVecs"> the number of vectors </param>
    /// <param name="stride"> the stride between vectors in floats  </param>
    public static void MulVec( float[] mat, float[] vecs, int offset, int numVecs, int stride )
    {
        for ( var i = 0; i < numVecs; i++ )
        {
            int   idx = offset + ( i * stride );
            float x   = vecs[ idx ];
            float y   = vecs[ idx + 1 ];
            float z   = vecs[ idx + 2 ];

            vecs[ idx ]     = ( mat[ M000 ] * x ) + ( mat[ M014 ] * y ) + ( mat[ M028 ] * z ) + mat[ M0312 ];
            vecs[ idx + 1 ] = ( mat[ M101 ] * x ) + ( mat[ M115 ] * y ) + ( mat[ M129 ] * z ) + mat[ M1313 ];
            vecs[ idx + 2 ] = ( mat[ M202 ] * x ) + ( mat[ M216 ] * y ) + ( mat[ M2210 ] * z ) + mat[ M2314 ];
        }
    }

    /// <summary>
    /// Multiplies the vectors with the given matrix, performing a division by w.
    /// The matrix array is assumed to hold a 4x4 column major matrix as you can
    /// get from <see cref="Val"/>. The vectors array is assumed to hold
    /// 3-component vectors. Offset specifies the offset into the array where the
    /// x-component of the first vector is located. The numVecs parameter specifies
    /// the number of vectors stored in the vectors array. The stride parameter
    /// specifies the number of floats between subsequent vectors and must be >= 3.
    /// This is the same as <see cref="Vector3.Prj(Matrix4)"/> applied to multiple
    /// vectors.
    /// </summary>
    /// <param name="mat"> the matrix </param>
    /// <param name="vecs"> the vectors </param>
    /// <param name="offset"> the offset into the vectors array </param>
    /// <param name="numVecs"> the number of vectors </param>
    /// <param name="stride"> the stride between vectors in floats  </param>
    public static void Prj( float[] mat, float[] vecs, int offset, int numVecs, int stride )
    {
        for ( var i = 0; i < numVecs; i++ )
        {
            int vecIndex = offset + ( i * stride );
            float invW = 1.0f / ( ( vecs[ vecIndex ] * mat[ M303 ] )
                                + ( vecs[ vecIndex + 1 ] * mat[ M317 ] )
                                + ( vecs[ vecIndex + 2 ] * mat[ M3211 ] )
                                + mat[ M3315 ] );

            float x = ( ( vecs[ vecIndex ] * mat[ M000 ] )
                      + ( vecs[ vecIndex + 1 ] * mat[ M014 ] )
                      + ( vecs[ vecIndex + 2 ] * mat[ M028 ] )
                      + mat[ M0312 ] ) * invW;
            float y = ( ( vecs[ vecIndex ] * mat[ M101 ] )
                      + ( vecs[ vecIndex + 1 ] * mat[ M115 ] )
                      + ( vecs[ vecIndex + 2 ] * mat[ M129 ] )
                      + mat[ M1313 ] ) * invW;
            float z = ( ( vecs[ vecIndex ] * mat[ M202 ] )
                      + ( vecs[ vecIndex + 1 ] * mat[ M216 ] )
                      + ( vecs[ vecIndex + 2 ] * mat[ M2210 ] )
                      + mat[ M2314 ] ) * invW;

            vecs[ vecIndex ]     = x;
            vecs[ vecIndex + 1 ] = y;
            vecs[ vecIndex + 2 ] = z;
        }
    }

    /// <summary>
    /// Multiplies the vectors with the top most 3x3 sub-matrix of the given matrix.
    /// The matrix array is assumed to hold a 4x4 column major matrix as you can get
    /// from <see cref="Val"/>. The vectors array is assumed to hold
    /// 3-component vectors. Offset specifies the offset into the array where the
    /// x-component of the first vector is located. The numVecs parameter specifies
    /// the number of vectors stored in the vectors array. The stride parameter
    /// specifies the number of floats between subsequent vectors and must be >= 3.
    /// This is the same as <see cref="Vector3.Rot(Matrix4)"/> applied to multiple vectors.
    /// </summary>
    /// <param name="mat"> the matrix </param>
    /// <param name="vecs"> the vectors </param>
    /// <param name="offset"> the offset into the vectors array </param>
    /// <param name="numVecs"> the number of vectors </param>
    /// <param name="stride"> the stride between vectors in floats  </param>
    public static void Rot( float[] mat, float[] vecs, int offset, int numVecs, int stride )
    {
        for ( var i = 0; i < numVecs; i++ )
        {
            int   idx = offset + ( i * stride );
            float x   = vecs[ idx ];
            float y   = vecs[ idx + 1 ];
            float z   = vecs[ idx + 2 ];

            vecs[ idx ]     = ( mat[ M000 ] * x ) + ( mat[ M014 ] * y ) + ( mat[ M028 ] * z );
            vecs[ idx + 1 ] = ( mat[ M101 ] * x ) + ( mat[ M115 ] * y ) + ( mat[ M129 ] * z );
            vecs[ idx + 2 ] = ( mat[ M202 ] * x ) + ( mat[ M216 ] * y ) + ( mat[ M2210 ] * z );
        }
    }

    /// <summary>
    /// Method to check if matrix is identity
    /// </summary>
    public bool IsIdentity()
    {
        const float Epsilon = NumberUtils.FloatEpsilon;

        // Check diagonal elements are close to 1
        if ( ( Math.Abs( Values[ M000 ] - 1f ) > Epsilon )
          || ( Math.Abs( Values[ M115 ] - 1f ) > Epsilon )
          || ( Math.Abs( Values[ M2210 ] - 1f ) > Epsilon )
          || ( Math.Abs( Values[ M3315 ] - 1f ) > Epsilon ) )
        {
            return false;
        }

        // Check non-diagonal elements are close to 0
        return ( Math.Abs( Values[ M014 ] ) <= Epsilon )
            && ( Math.Abs( Values[ M028 ] ) <= Epsilon )
            && ( Math.Abs( Values[ M0312 ] ) <= Epsilon )
            && ( Math.Abs( Values[ M101 ] ) <= Epsilon )
            && ( Math.Abs( Values[ M129 ] ) <= Epsilon )
            && ( Math.Abs( Values[ M1313 ] ) <= Epsilon )
            && ( Math.Abs( Values[ M202 ] ) <= Epsilon )
            && ( Math.Abs( Values[ M216 ] ) <= Epsilon )
            && ( Math.Abs( Values[ M2314 ] ) <= Epsilon )
            && ( Math.Abs( Values[ M303 ] ) <= Epsilon )
            && ( Math.Abs( Values[ M317 ] ) <= Epsilon )
            && ( Math.Abs( Values[ M3211 ] ) <= Epsilon );
    }

    /// <summary>
    /// </summary>
    public Vector4 GetColumn( int index )
    {
        Guard.Against.OutOfRange( index, 0, 3 );

        return new Vector4( Val[ ( index * 4 ) + 0 ],
                            Val[ ( index * 4 ) + 1 ],
                            Val[ ( index * 4 ) + 2 ],
                            Val[ ( index * 4 ) + 3 ] );
    }

    /// <summary>
    /// </summary>
    public Vector4 GetRow( int index )
    {
        Guard.Against.OutOfRange( index, 0, 3 );

        return new Vector4( Val[ index ],
                            Val[ index + 4 ],
                            Val[ index + 8 ],
                            Val[ index + 12 ] );
    }

    // ========================================================================

    public override string ToString()
    {
        return $"Row 0: [{Val[ M000 ]:F6}|{Val[ M014 ]:F6}|{Val[ M028 ]:F6}|{Val[ M0312 ]:F6}]\n" +
               $"Row 1: [{Val[ M101 ]:F6}|{Val[ M115 ]:F6}|{Val[ M129 ]:F6}|{Val[ M1313 ]:F6}]\n" +
               $"Row 2: [{Val[ M202 ]:F6}|{Val[ M216 ]:F6}|{Val[ M2210 ]:F6}|{Val[ M2314 ]:F6}]\n" +
               $"Row 3: [{Val[ M303 ]:F6}|{Val[ M317 ]:F6}|{Val[ M3211 ]:F6}|{Val[ M3315 ]:F6}]\n";
    }

    /// <summary>
    /// Debug method to print the matrix in a readable format
    /// </summary>
    public void DebugPrint( string name = "Matrix" )
    {
        Logger.Debug( $"\n{name}:" );

        var sb = new StringBuilder();

        for ( var row = 0; row < 4; row++ )
        {
            sb.Append( '[' );

            for ( var col = 0; col < 4; col++ )
            {
                sb.Append( $"{Val[ row + ( col * 4 ) ]:F6}" );

                if ( col < 3 )
                {
                    sb.Append( ", " );
                }
            }

            sb.Append( ']' ).AppendLine();
        }
    }
}

// ============================================================================
// ============================================================================