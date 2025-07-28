// ///////////////////////////////////////////////////////////////////////////////
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

using LughSharp.Lugh.Utils.Exceptions;

using Exception = System.Exception;

namespace LughSharp.Lugh.Maths;

[PublicAPI]
public class Vector3 : IVector< Vector3 >
{
    public static readonly Vector3 XDefault = new( 1, 0, 0 );
    public static readonly Vector3 YDefault = new( 0, 1, 0 );
    public static readonly Vector3 ZDefault = new( 0, 0, 1 );
    public static readonly Vector3 Zero     = new( 0, 0, 0 );

    // ========================================================================

    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    // ========================================================================

    private static readonly Matrix4 _tmpMat = new();

    // ========================================================================

    /// <summary>
    /// Default constructor.
    /// Sets X, Y, and Z to zero.
    /// </summary>
    public Vector3() : this( 0, 0, 0 )
    {
    }

    /// <summary>
    /// Constructs a new vector using the supplied value for all components.
    /// </summary>
    /// <param name="value"> The value to use for X, Y, and Z. </param>
    public Vector3( float value ) : this( value, value, value )
    {
    }

    /// <summary>
    /// Constructs a new Vector3, with X, Y, and Z set to supplied values.
    /// </summary>
    public Vector3( float x, float y, float z )
    {
        Set( x, y, z );
    }

    /// <summary>
    /// Creates a new Vector3, as a copy of the supplied Vec tor3.
    /// </summary>
    public Vector3( Vector3 vector )
    {
        Set( vector );
    }

    /// <summary>
    /// Creates a new Vector3, using the supplied array for its X,Y and Z values.
    /// The valuies in the array must be ordered X, Y and Z.
    /// </summary>
    public Vector3( IReadOnlyList< float > values )
    {
        Set( values[ 0 ], values[ 1 ], values[ 2 ] );
    }

    /// <summary>
    /// Creates a new Vector3 from the supplied Vector2, which holds values
    /// for X and Y, and a float holding the Z value.
    /// </summary>
    public Vector3( Vector2 vector, float z )
    {
        Set( vector.X, vector.Y, z );
    }

    /// <summary>
    /// Sets this Vector3's X, Y and Z values from the supplied Vector3.
    /// </summary>
    public Vector3 Set( Vector3 vector )
    {
        return Set( vector.X, vector.Y, vector.Z );
    }

    /// <inheritdoc />
    public Vector3 SetToRandomDirection()
    {
        var u = MathUtils.Random();
        var v = MathUtils.Random();

        var theta = MathUtils.PI2 * u;                     // azimuthal angle
        var phi   = ( float )Math.Acos( ( 2f * v ) - 1f ); // polar angle

        return SetFromSpherical( theta, phi );
    }

    /// <inheritdoc />
    public Vector3 Cpy()
    {
        return new Vector3( this );
    }

    /// <inheritdoc />
    public Vector3 Add( Vector3 vector )
    {
        return Add( vector.X, vector.Y, vector.Z );
    }

    /// <inheritdoc />
    public Vector3 Sub( Vector3 vec )
    {
        return Sub( vec.X, vec.Y, vec.Z );
    }

    /// <inheritdoc />
    public Vector3 Scale( float scalar )
    {
        return Set( X * scalar, Y * scalar, Z * scalar );
    }

    /// <inheritdoc />
    public Vector3 Scale( Vector3 other )
    {
        return Set( X * other.X, Y * other.Y, Z * other.Z );
    }

    /// <inheritdoc />
    public Vector3 MulAdd( Vector3 vec, float scalar )
    {
        X += vec.X * scalar;
        Y += vec.Y * scalar;
        Z += vec.Z * scalar;

        return this;
    }

    /// <inheritdoc />
    public Vector3 MulAdd( Vector3 vec, Vector3 mulVec )
    {
        X += vec.X * mulVec.X;
        Y += vec.Y * mulVec.Y;
        Z += vec.Z * mulVec.Z;

        return this;
    }

    /// <inheritdoc />
    public float Len()
    {
        return ( float )Math.Sqrt( ( X * X ) + ( Y * Y ) + ( Z * Z ) );
    }

    /// <inheritdoc />
    public float Len2()
    {
        return ( X * X ) + ( Y * Y ) + ( Z * Z );
    }

    /// <inheritdoc />
    public float Distance( Vector3 vector )
    {
        var a = vector.X - X;
        var b = vector.Y - Y;
        var c = vector.Z - Z;

        return ( float )Math.Sqrt( ( a * a ) + ( b * b ) + ( c * c ) );
    }

    /// <inheritdoc />
    public float Distance2( Vector3 point )
    {
        var a = point.X - X;
        var b = point.Y - Y;
        var c = point.Z - Z;

        return ( a * a ) + ( b * b ) + ( c * c );
    }

    /// <inheritdoc />
    public Vector3 Nor()
    {
        var len2 = Len2();

        if ( ( len2 == 0f ) || MathUtils.IsEqual( len2, 1f ) )
        {
            return this;
        }

        return Scale( 1f / ( float )Math.Sqrt( len2 ) );
    }

    /// <inheritdoc />
    public float Dot( Vector3 vector )
    {
        return ( X * vector.X ) + ( Y * vector.Y ) + ( Z * vector.Z );
    }

    /// <inheritdoc />
    public bool IsUnit( float margin = NumberUtils.FLOAT_TOLERANCE )
    {
        return Math.Abs( Len2() - 1f ) < margin;
    }

    /// <inheritdoc />
    public bool IsZero()
    {
        return ( X == 0 ) && ( Y == 0 ) && ( Z == 0 );
    }

    /// <inheritdoc />
    public bool IsZero( float margin )
    {
        return Len2() < margin;
    }

    /// <inheritdoc />
    public bool IsOnLine( Vector3 other, float epsilon )
    {
        return Len2( ( Y * other.Z ) - ( Z * other.Y ), ( Z * other.X ) - ( X * other.Z ), ( X * other.Y ) - ( Y * other.X ) ) <= epsilon;
    }

    /// <inheritdoc />
    public bool IsOnLine( Vector3 other )
    {
        return Len2( ( Y * other.Z ) - ( Z * other.Y ), ( Z * other.X ) - ( X * other.Z ), ( X * other.Y ) - ( Y * other.X ) )
               <= NumberUtils.FLOAT_TOLERANCE;
    }

    /// <inheritdoc />
    public bool IsCollinear( Vector3 other, float epsilon )
    {
        return IsOnLine( other, epsilon ) && HasSameDirection( other );
    }

    /// <inheritdoc />
    public bool IsCollinear( Vector3 other )
    {
        return IsOnLine( other ) && HasSameDirection( other );
    }

    /// <inheritdoc />
    public bool IsCollinearOpposite( Vector3 other, float epsilon )
    {
        return IsOnLine( other, epsilon ) && HasOppositeDirection( other );
    }

    /// <inheritdoc />
    public bool IsCollinearOpposite( Vector3 other )
    {
        return IsOnLine( other ) && HasOppositeDirection( other );
    }

    /// <inheritdoc />
    public bool IsPerpendicular( Vector3 vector )
    {
        return MathUtils.IsZero( Dot( vector ) );
    }

    /// <inheritdoc />
    public bool IsPerpendicular( Vector3 vector, float epsilon )
    {
        return MathUtils.IsZero( Dot( vector ), epsilon );
    }

    /// <inheritdoc />
    public bool HasSameDirection( Vector3 vector )
    {
        return Dot( vector ) > 0;
    }

    /// <inheritdoc />
    public bool HasOppositeDirection( Vector3 vector )
    {
        return Dot( vector ) < 0;
    }

    /// <inheritdoc />
    public Vector3 Lerp( Vector3 target, float alpha )
    {
        X += alpha * ( target.X - X );
        Y += alpha * ( target.Y - Y );
        Z += alpha * ( target.Z - Z );

        return this;
    }

    /// <inheritdoc />
    public Vector3 Interpolate( Vector3 target, float alpha, IInterpolation interpolator )
    {
        return Lerp( target, interpolator.Apply( 0f, 1f, alpha ) );
    }

    /// <inheritdoc />
    public Vector3 Limit( float limit )
    {
        return Limit2( limit * limit );
    }

    /// <inheritdoc />
    public Vector3 Limit2( float limit2 )
    {
        var len2 = Len2();

        if ( len2 > limit2 )
        {
            Scale( ( float )Math.Sqrt( limit2 / len2 ) );
        }

        return this;
    }

    /// <inheritdoc />
    public Vector3 SetLength( float len )
    {
        return SetLength2( len * len );
    }

    /// <inheritdoc />
    public Vector3 SetLength2( float len2 )
    {
        var oldLen2 = Len2();

        return ( oldLen2 == 0 ) || MathUtils.IsEqual( oldLen2, len2 ) ? this : Scale( ( float )Math.Sqrt( len2 / oldLen2 ) );
    }

    /// <inheritdoc />
    public Vector3 Clamp( float min, float max )
    {
        var len2 = Len2();

        if ( len2 == 0f )
        {
            return this;
        }

        var max2 = max * max;

        if ( len2 > max2 )
        {
            return Scale( ( float )Math.Sqrt( max2 / len2 ) );
        }

        var min2 = min * min;

        return len2 < min2 ? Scale( ( float )Math.Sqrt( min2 / len2 ) ) : this;
    }

    /// <inheritdoc />
    public bool EpsilonEquals( Vector3? other, float epsilon = NumberUtils.FLOAT_TOLERANCE )
    {
        if ( other is null )
        {
            return false;
        }

        if ( Math.Abs( other.X - X ) > epsilon )
        {
            return false;
        }

        if ( Math.Abs( other.Y - Y ) > epsilon )
        {
            return false;
        }

        return !( Math.Abs( other.Z - Z ) > epsilon );
    }

    /// <inheritdoc />
    public Vector3 SetZero()
    {
        X = 0;
        Y = 0;
        Z = 0;

        return this;
    }

    public Vector3 Set( float x, float y, float z )
    {
        X = x;
        Y = y;
        Z = z;

        return this;
    }

    public Vector3 Set( float[] values )
    {
        return Set( values[ 0 ], values[ 1 ], values[ 2 ] );
    }

    public Vector3 Set( Vector2 vector, float z )
    {
        return Set( vector.X, vector.Y, z );
    }

    /// <summary>
    /// </summary>
    /// <param name="azimuthalAngle"></param>
    /// <param name="polarAngle"></param>
    /// <returns></returns>
    public Vector3 SetFromSpherical( float azimuthalAngle, float polarAngle )
    {
        var cosPolar = MathUtils.Cos( polarAngle );
        var sinPolar = MathUtils.Sin( polarAngle );

        var cosAzim = MathUtils.Cos( azimuthalAngle );
        var sinAzim = MathUtils.Sin( azimuthalAngle );

        return Set( cosAzim * sinPolar, sinAzim * sinPolar, cosPolar );
    }

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public Vector3 Add( float x, float y, float z )
    {
        return Set( X + x, Y + y, Z + z );
    }

    public Vector3 Add( float values )
    {
        return Set( X + values, Y + values, Z + values );
    }

    public Vector3 Sub( float x, float y, float z )
    {
        return Set( X - x, Y - y, Z - z );
    }

    public Vector3 Sub( float value )
    {
        return Set( X - value, Y - value, Z - value );
    }

    public Vector3 Scl( float vx, float vy, float vz )
    {
        return Set( X * vx, Y * vy, Z * vz );
    }

    public static float Len( float x, float y, float z )
    {
        return ( float )Math.Sqrt( ( x * x ) + ( y * y ) + ( z * z ) );
    }

    public static float Len2( float x, float y, float z )
    {
        return ( x * x ) + ( y * y ) + ( z * z );
    }

    public bool Idt( Vector3 vector )
    {
        return MathUtils.IsEqual( X, vector.X )
               && MathUtils.IsEqual( Y, vector.Y )
               && MathUtils.IsEqual( Z, vector.Z );
    }

    public static float Dst( float x1, float y1, float z1, float x2, float y2, float z2 )
    {
        var a = x2 - x1;
        var b = y2 - y1;
        var c = z2 - z1;

        return ( float )Math.Sqrt( ( a * a ) + ( b * b ) + ( c * c ) );
    }

    public float Dst( float x, float y, float z )
    {
        var a = x - X;
        var b = y - Y;
        var c = z - Z;

        return ( float )Math.Sqrt( ( a * a ) + ( b * b ) + ( c * c ) );
    }

    public static float Dst2( float x1, float y1, float z1, float x2, float y2, float z2 )
    {
        var a = x2 - x1;
        var b = y2 - y1;
        var c = z2 - z1;

        return ( a * a ) + ( b * b ) + ( c * c );
    }

    public float Dst2( float x, float y, float z )
    {
        var a = x - X;
        var b = y - Y;
        var c = z - Z;

        return ( a * a ) + ( b * b ) + ( c * c );
    }

    public static float Dot( float x1, float y1, float z1, float x2, float y2, float z2 )
    {
        return ( x1 * x2 ) + ( y1 * y2 ) + ( z1 * z2 );
    }

    public float Dot( float x, float y, float z )
    {
        return ( X * x ) + ( Y * y ) + ( Z * z );
    }

    public Vector3 Crs( Vector3 vector )
    {
        return Set( ( Y * vector.Z ) - ( Z * vector.Y ), ( Z * vector.X ) - ( X * vector.Z ), ( X * vector.Y ) - ( Y * vector.X ) );
    }

    public Vector3 Crs( float x, float y, float z )
    {
        return Set( ( Y * z ) - ( Z * y ), ( Z * x ) - ( X * z ), ( X * y ) - ( Y * x ) );
    }

    public Vector3 Mul4X3( float[] matrix )
    {
        return Set(
                   ( X * matrix[ 0 ] ) + ( Y * matrix[ 3 ] ) + ( Z * matrix[ 6 ] ) + matrix[ 9 ],
                   ( X * matrix[ 1 ] )
                   + ( Y * matrix[ 4 ] )
                   + ( Z * matrix[ 7 ] )
                   + matrix[ 10 ],
                   ( X * matrix[ 2 ] ) + ( Y * matrix[ 5 ] ) + ( Z * matrix[ 8 ] ) + matrix[ 11 ]
                  );
    }

    public Vector3 Mul( Matrix4 matrix )
    {
        var lMat = matrix.Val;

        return Set(
                   ( X * lMat[ Matrix4.M00_0 ] ) + ( Y * lMat[ Matrix4.M01_4 ] ) + ( Z * lMat[ Matrix4.M02_8 ] ) + lMat[ Matrix4.M03_12 ],
                   ( X
                     * lMat[ Matrix4.M10_1 ] )
                   + ( Y * lMat[ Matrix4.M11_5 ] )
                   + ( Z * lMat[ Matrix4.M12_9 ] )
                   + lMat[ Matrix4.M13_13 ],
                   ( X * lMat[ Matrix4.M20_2 ] )
                   + ( Y
                       * lMat[ Matrix4.M21_6 ] )
                   + ( Z * lMat[ Matrix4.M22_10 ] )
                   + lMat[ Matrix4.M23_14 ]
                  );
    }

    public Vector3 TraMul( Matrix4 matrix )
    {
        var lMat = matrix.Val;

        return Set(
                   ( X * lMat[ Matrix4.M00_0 ] ) + ( Y * lMat[ Matrix4.M10_1 ] ) + ( Z * lMat[ Matrix4.M20_2 ] ) + lMat[ Matrix4.M30_3 ],
                   ( X
                     * lMat[ Matrix4.M01_4 ] )
                   + ( Y * lMat[ Matrix4.M11_5 ] )
                   + ( Z * lMat[ Matrix4.M21_6 ] )
                   + lMat[ Matrix4.M31_7 ],
                   ( X * lMat[ Matrix4.M02_8 ] )
                   + ( Y
                       * lMat[ Matrix4.M12_9 ] )
                   + ( Z * lMat[ Matrix4.M22_10 ] )
                   + lMat[ Matrix4.M32_11 ]
                  );
    }

    public Vector3 Mul( Matrix3 matrix )
    {
        var lMat = matrix.Val;

        return Set(
                   ( X * lMat[ Matrix3.M00 ] ) + ( Y * lMat[ Matrix3.M01 ] ) + ( Z * lMat[ Matrix3.M02 ] ),
                   ( X * lMat[ Matrix3.M10 ] )
                   + ( Y
                       * lMat[ Matrix3.M11 ] )
                   + ( Z * lMat[ Matrix3.M12 ] ),
                   ( X * lMat[ Matrix3.M20 ] ) + ( Y * lMat[ Matrix3.M21 ] ) + ( Z * lMat[ Matrix3.M22 ] )
                  );
    }

    public Vector3 TraMul( Matrix3 matrix )
    {
        var lMat = matrix.Val;

        return Set(
                   ( X * lMat[ Matrix3.M00 ] ) + ( Y * lMat[ Matrix3.M10 ] ) + ( Z * lMat[ Matrix3.M20 ] ),
                   ( X * lMat[ Matrix3.M01 ] )
                   + ( Y
                       * lMat[ Matrix3.M11 ] )
                   + ( Z * lMat[ Matrix3.M21 ] ),
                   ( X * lMat[ Matrix3.M02 ] ) + ( Y * lMat[ Matrix3.M12 ] ) + ( Z * lMat[ Matrix3.M22 ] )
                  );
    }

    public Vector3 Mul( Quaternion quat )
    {
        return quat.Transform( this );
    }

    public Vector3 Prj( Matrix4 matrix )
    {
        var lMat = matrix.Val;
        var lW   = 1f / ( ( X * lMat[ Matrix4.M30_3 ] ) + ( Y * lMat[ Matrix4.M31_7 ] ) + ( Z * lMat[ Matrix4.M32_11 ] ) + lMat[ Matrix4.M33_15 ] );

        return Set(
                   ( ( X * lMat[ Matrix4.M00_0 ] ) + ( Y * lMat[ Matrix4.M01_4 ] ) + ( Z * lMat[ Matrix4.M02_8 ] ) + lMat[ Matrix4.M03_12 ] ) * lW,
                   ( ( X
                       * lMat[ Matrix4.M10_1 ] )
                     + ( Y * lMat[ Matrix4.M11_5 ] )
                     + ( Z * lMat[ Matrix4.M12_9 ] )
                     + lMat[ Matrix4.M13_13 ] )
                   * lW,
                   ( ( X * lMat[ Matrix4.M20_2 ] ) + ( Y * lMat[ Matrix4.M21_6 ] ) + ( Z * lMat[ Matrix4.M22_10 ] ) + lMat[ Matrix4.M23_14 ] ) * lW
                  );
    }

    public Vector3 Rot( Matrix4 matrix )
    {
        var lMat = matrix.Val;

        return Set(
                   ( X * lMat[ Matrix4.M00_0 ] ) + ( Y * lMat[ Matrix4.M01_4 ] ) + ( Z * lMat[ Matrix4.M02_8 ] ),
                   ( X * lMat[ Matrix4.M10_1 ] )
                   + ( Y
                       * lMat[ Matrix4.M11_5 ] )
                   + ( Z * lMat[ Matrix4.M12_9 ] ),
                   ( X * lMat[ Matrix4.M20_2 ] ) + ( Y * lMat[ Matrix4.M21_6 ] ) + ( Z * lMat[ Matrix4.M22_10 ] )
                  );
    }

    public Vector3 Unrotate( Matrix4 matrix )
    {
        var lMat = matrix.Val;

        return Set(
                   ( X * lMat[ Matrix4.M00_0 ] ) + ( Y * lMat[ Matrix4.M10_1 ] ) + ( Z * lMat[ Matrix4.M20_2 ] ),
                   ( X * lMat[ Matrix4.M01_4 ] )
                   + ( Y
                       * lMat[ Matrix4.M11_5 ] )
                   + ( Z * lMat[ Matrix4.M21_6 ] ),
                   ( X * lMat[ Matrix4.M02_8 ] ) + ( Y * lMat[ Matrix4.M12_9 ] ) + ( Z * lMat[ Matrix4.M22_10 ] )
                  );
    }

    public Vector3 Untransform( Matrix4 matrix )
    {
        var lMat = matrix.Val;

        X -= lMat[ Matrix4.M03_12 ];
        Y -= lMat[ Matrix4.M03_12 ];
        Z -= lMat[ Matrix4.M03_12 ];

        return Set(
                   ( X * lMat[ Matrix4.M00_0 ] ) + ( Y * lMat[ Matrix4.M10_1 ] ) + ( Z * lMat[ Matrix4.M20_2 ] ),
                   ( X * lMat[ Matrix4.M01_4 ] )
                   + ( Y
                       * lMat[ Matrix4.M11_5 ] )
                   + ( Z * lMat[ Matrix4.M21_6 ] ),
                   ( X * lMat[ Matrix4.M02_8 ] ) + ( Y * lMat[ Matrix4.M12_9 ] ) + ( Z * lMat[ Matrix4.M22_10 ] )
                  );
    }

    public Vector3 Rotate( float degrees, float axisX, float axisY, float axisZ )
    {
        return Mul( _tmpMat.SetToRotation( axisX, axisY, axisZ, degrees ) );
    }

    public Vector3 RotateRad( float radians, float axisX, float axisY, float axisZ )
    {
        return Mul( _tmpMat.SetToRotationRad( axisX, axisY, axisZ, radians ) );
    }

    public Vector3 Rotate( Vector3 axis, float degrees )
    {
        _tmpMat.SetToRotation( axis, degrees );

        return Mul( _tmpMat );
    }

    public Vector3 RotateRad( Vector3 axis, float radians )
    {
        _tmpMat.SetToRotationRad( axis, radians );

        return Mul( _tmpMat );
    }

    public Vector3 Slerp( Vector3 target, float alpha )
    {
        var dot = Dot( target );

        // If the inputs are too close for comfort, simply linearly interpolate.
        if ( ( dot > 0.9995 ) || ( dot < -0.9995 ) )
        {
            return Lerp( target, alpha );
        }

        // theta0 = angle between input vectors
        var theta0 = ( float )Math.Acos( dot );

        // theta = angle between this vector and result
        var theta = theta0 * alpha;

        var st = ( float )Math.Sin( theta );
        var tx = target.X - ( X * dot );
        var ty = target.Y - ( Y * dot );
        var tz = target.Z - ( Z * dot );
        var l2 = ( tx * tx ) + ( ty * ty ) + ( tz * tz );
        var dl = st * ( l2 < 0.0001f ? 1f : 1f / ( float )Math.Sqrt( l2 ) );

        return Scale( ( float )Math.Cos( theta ) ).Add( tx * dl, ty * dl, tz * dl ).Nor();
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
        const int PRIME = 31;

        var result = PRIME + NumberUtils.FloatToIntBits( 10f );
        result = ( PRIME * result ) + NumberUtils.FloatToIntBits( 15f );
        result = ( PRIME * result ) + NumberUtils.FloatToIntBits( 20f );

        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals( object? obj )
    {
        if ( ReferenceEquals( obj, this ) )
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

        var other = ( Vector3 )obj;

        if ( NumberUtils.FloatToIntBits( X ) != NumberUtils.FloatToIntBits( other.X ) )
        {
            return false;
        }

        if ( NumberUtils.FloatToIntBits( Y ) != NumberUtils.FloatToIntBits( other.Y ) )
        {
            return false;
        }

        return NumberUtils.FloatToIntBits( Z ) == NumberUtils.FloatToIntBits( other.Z );
    }

    /// <summary>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public bool EpsilonEquals( float x, float y, float z, float epsilon = NumberUtils.FLOAT_TOLERANCE )
    {
        if ( Math.Abs( x - X ) > epsilon )
        {
            return false;
        }

        if ( Math.Abs( y - Y ) > epsilon )
        {
            return false;
        }

        return !( Math.Abs( z - Z ) > epsilon );
    }

    public override string ToString()
    {
        return "(" + X + "," + Y + "," + Z + ")";
    }

    public Vector3 FromString( string v )
    {
        var s0 = v.IndexOf( ',', 1 );
        var s1 = v.IndexOf( ',', s0 + 1 );

        if ( ( s0 != -1 ) && ( s1 != -1 ) && ( v[ 0 ] == '(' ) && ( v[ ^1 ] == ')' ) )
        {
            try
            {
                var x = float.Parse( v.Substring( 1, s0 ) );
                var y = float.Parse( v.Substring( s0 + 1, s1 ) );
                var z = float.Parse( v.Substring( s1 + 1, v.Length - 1 ) );

                return Set( x, y, z );
            }
            catch ( Exception ex )
            {
                throw new GdxRuntimeException( ex.Message );
            }
        }

        throw new GdxRuntimeException( "Malformed Vector3: " + v );
    }

    // ========================================================================

    /// <summary>Adds two vectors together.</summary>
    /// <param name="left">The first vector to add.</param>
    /// <param name="right">The second vector to add.</param>
    /// <returns>The summed vector.</returns>
    /// <remarks>
    /// The <see cref="op_Addition" /> method defines the addition operation for <see cref="Vector3" /> objects.
    /// </remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Vector3 operator +( Vector3 left, Vector3 right )
    {
        return new Vector3( left.X + right.X, left.Y + right.Y, left.Z + right.Z );
    }

    /// <summary>Divides the first vector by the second.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector that results from dividing <paramref name="left" /> by <paramref name="right" />.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Vector3 operator /( Vector3 left, Vector3 right )
    {
        return new Vector3( left.X / right.X, left.Y / right.Y, left.Z / right.Z );
    }

    /// <summary>Divides the specified vector by a specified scalar value.</summary>
    /// <param name="value1">The vector.</param>
    /// <param name="value2">The scalar value.</param>
    /// <returns>The result of the division.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Vector3 operator /( Vector3 value1, float value2 )
    {
        return ( value1 / new Vector3( value2 ) );
    }

    /// <summary>Returns a value that indicates whether each pair of elements in two specified vectors is equal.</summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
    /// <remarks>Two <see cref="Vector3" /> objects are equal if each element in <paramref name="left" /> is equal to the corresponding element in <paramref name="right" />.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==( Vector3 left, Vector3 right )
    {
        return ( left.X == right.X ) && ( left.Y == right.Y ) && ( left.Z == right.Z );
    }

    /// <summary>Returns a value that indicates whether two specified vectors are not equal.</summary>
    /// <param name="left">The first vector to compare.</param>
    /// <param name="right">The second vector to compare.</param>
    /// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=( Vector3? left, Vector3? right )
    {
        return !ReferenceEquals( left, right );
    }

    /// <summary>Returns a new vector whose values are the product of each pair of elements in two specified vectors.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The element-wise product vector.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Vector3 operator *( Vector3 left, Vector3 right )
    {
        return new Vector3( left.X * right.X, left.Y * right.Y, left.Z * right.Z );
    }

    /// <summary>Multiplies the specified vector by the specified scalar value.</summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Vector3 operator *( Vector3 left, float right )
    {
        return left * new Vector3( right );
    }

    /// <summary>Multiplies the scalar value by the specified vector.</summary>
    /// <param name="left">The vector.</param>
    /// <param name="right">The scalar value.</param>
    /// <returns>The scaled vector.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Vector3 operator *( float left, Vector3 right )
    {
        return right * left;
    }

    /// <summary>Subtracts the second vector from the first.</summary>
    /// <param name="left">The first vector.</param>
    /// <param name="right">The second vector.</param>
    /// <returns>The vector that results from subtracting <paramref name="right" /> from <paramref name="left" />.</returns>
    /// <remarks>The <see cref="op_Subtraction" /> method defines the subtraction operation for <see cref="Vector3" /> objects.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Vector3 operator -( Vector3 left, Vector3 right )
    {
        return new Vector3( left.X - right.X, left.Y - right.Y, left.Z - right.Z );
    }

    /// <summary>Negates the specified vector.</summary>
    /// <param name="value">The vector to negate.</param>
    /// <returns>The negated vector.</returns>
    /// <remarks>The <see cref="op_UnaryNegation" /> method defines the unary negation operation for <see cref="Vector3" /> objects.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Vector3 operator -( Vector3 value )
    {
        return Zero - value;
    }
}