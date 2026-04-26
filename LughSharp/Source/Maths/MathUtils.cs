// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Circa64 Software Projects
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

namespace LughSharp.Source.Maths;

[PublicAPI]
public class MathUtils
{
    public const float NanoToSec = 1 / 1000000000f;
    public const float Pi          = 3.1415927f;
    public const float HalfPi     = Pi / 2;
    public const float Pi2         = Pi * 2;
    public const float E           = 2.7182818f;

    // multiply by this to convert from radians to degrees.
    public const float RadiansToDegrees = 180f / Pi;
    public const float RadDeg            = RadiansToDegrees;

    // multiply by this to convert from degrees to radians.
    public const float DegreesToRadians = Pi / 180;
    public const float DegRad            = DegreesToRadians;

    private const int   SinBits     = 14; // 16KB. Adjust for accuracy.
    private const int   SinMask     = ~( -1 << SinBits );
    private const int   SinCount    = SinMask + 1;
    private const float RadFull     = Pi * 2;
    private const float DegFull     = 360;
    private const float RadToIndex = SinCount / RadFull;
    private const float DegToIndex = SinCount / DegFull;

    private const int    BigEnoughInt   = 16 * 1024;
    private const double BigEnoughFloor = BigEnoughInt;
    private const double Ceiling          = 0.9999999;
    private const double BigEnoughCeil  = 16384.999999999996;
    private const double BigEnoughRound = BigEnoughInt + 0.5f;

    private static readonly Random _rand = new();

    // ========================================================================

    /// <summary>
    /// Returns the sine in radians from a lookup table.
    /// </summary>
    public static float Sin( float radians )
    {
        return SinClass.Table[ ( int )( radians * RadToIndex ) & SinMask ];
    }

    /// <summary>
    /// Returns the cosine in radians from a lookup table.
    /// </summary>
    public static float Cos( float radians )
    {
        return SinClass.Table[ ( int )( ( radians + ( Pi / 2 ) ) * RadToIndex ) & SinMask ];
    }

    /// <summary>
    /// Returns the sine in radians from a lookup table.
    /// </summary>
    public static float SinDeg( float degrees )
    {
        return SinClass.Table[ ( int )( degrees * DegToIndex ) & SinMask ];
    }

    /// <summary>
    /// Returns the cosine in radians from a lookup table.
    /// </summary>
    public static float CosDeg( float degrees )
    {
        return SinClass.Table[ ( int )( ( degrees + 90 ) * DegToIndex ) & SinMask ];
    }

    /// <summary>
    /// Returns atan2 in radians, faster but less accurate than Math.atan2.
    /// Average error of 0.00231 radians (0.1323 degrees),
    /// Largest error of 0.00488 radians (0.2796 degrees).
    /// </summary>
    public static float Atan2( float y, float x )
    {
        if ( x == 0f )
        {
            return y switch
                   {
                       > 0f  => Pi / 2,
                       0f    => 0f,
                       var _ => -Pi / 2
                   };
        }

        float atan, z = y / x;

        if ( Math.Abs( z ) < 1f )
        {
            atan = z / ( 1f + ( 0.28f * z * z ) );

            if ( x < 0f )
            {
                return atan + ( y < 0f ? -Pi : Pi );
            }

            return atan;
        }

        atan = ( Pi / 2 ) - ( z / ( ( z * z ) + 0.28f ) );

        return y < 0f ? atan - Pi : atan;
    }

    /// <summary>
    /// Returns a random number between 0 (inclusive) and the specified value (inclusive).
    /// </summary>
    public static int Random( int range )
    {
        return _rand.Next( range + 1 );
    }

    /// <summary>
    /// Returns a random number between start (inclusive) and end (inclusive).
    /// </summary>
    public static int Random( int start, int end )
    {
        return start + _rand.Next( end - start + 1 );
    }

    /// <summary>
    /// Returns a random number between 0 (inclusive) and the specified value (inclusive).
    /// </summary>
    public static long Random( long range )
    {
        return ( long )( _rand.NextDouble() * range );
    }

    /// <summary>
    /// Returns a random number between start (inclusive) and end (inclusive).
    /// </summary>
    public static long Random( long start, long end )
    {
        return start + ( long )( _rand.NextDouble() * ( end - start ) );
    }

    /// <summary>
    /// Returns the next random number as a 'long'.
    /// </summary>
    public static long RandomLong()
    {
        return ( long )_rand.NextInt64();
    }

    /// <summary>
    /// Returns a random bool value.
    /// </summary>
    public static bool RandomBool()
    {
        return _rand.Next( 2 ) == 0;
    }

    /// <summary>
    /// Returns true if a random value between 0 and 1 is less than the specified value.
    /// </summary>
    public static bool RandomBool( float chance )
    {
        return Random() < chance;
    }

    /// <summary>
    /// Returns random number between 0.0 (inclusive) and 1.0 (exclusive).
    /// </summary>
    public static float Random()
    {
        return ( float )_rand.NextDouble();
    }

    /// <summary>
    /// Returns a random number between 0 (inclusive) and the specified value (exclusive).
    /// </summary>
    public static float Random( float range )
    {
        return ( float )_rand.NextDouble() * range;
    }

    /// <summary>
    /// Returns a random number between start (inclusive) and end (exclusive).
    /// </summary>
    public static float Random( float start, float end )
    {
        return start + ( ( float )_rand.NextDouble() * ( end - start ) );
    }

    /// <summary>
    /// Returns -1 or 1, randomly.
    /// </summary>
    public static int RandomSign()
    {
        return _rand.Next() >> 31;
    }

    /// <summary>
    /// Returns a triangularly distributed random number between -1.0 (exclusive) and
    /// 1.0 (exclusive), where values around zero are more likely.
    /// </summary>
    public static float RandomTriangular()
    {
        return ( float )_rand.NextDouble() - ( float )_rand.NextDouble();
    }

    /// <summary>
    /// Returns a triangularly distributed random number between <tt>-max</tt> (exclusive)
    /// and <tt>max</tt> (exclusive), where values around zero are more likely.
    /// </summary>
    /// <param name="max"> the upper limit  </param>
    public static float RandomTriangular( float max )
    {
        return ( float )( _rand.NextDouble() - _rand.NextDouble() ) * max;
    }

    /// <summary>
    /// Returns a triangularly distributed random number between <tt>min</tt> (inclusive)
    /// and <tt>max</tt> (exclusive), where the <tt>mode</tt> argument defaults to the
    /// midpoint between the bounds, giving a symmetric distribution.
    /// </summary>
    /// <param name="min"> the lower limit </param>
    /// <param name="max"> the upper limit  </param>
    public static float RandomTriangular( float min, float max )
    {
        return RandomTriangular( min, max, ( min + max ) * 0.5f );
    }

    /// <summary>
    /// Returns a triangularly distributed random number between <tt>min</tt> (inclusive)
    /// and <tt>max</tt> (exclusive), where values around <tt>mode</tt> are more likely.
    /// </summary>
    /// <param name="min"> the lower limit </param>
    /// <param name="max"> the upper limit </param>
    /// <param name="mode"> the point around which the values are more likely  </param>
    public static float RandomTriangular( float min, float max, float mode )
    {
        var   u = ( float )_rand.NextDouble();
        float d = max - min;

        if ( u <= ( ( mode - min ) / d ) )
        {
            return min + ( float )Math.Sqrt( u * d * ( mode - min ) );
        }

        return max - ( float )Math.Sqrt( ( 1 - u ) * d * ( max - mode ) );
    }

    /// <summary>
    /// Returns the next power of two. Returns the specified value if the
    /// value is already a power of two.
    /// </summary>
    public static int NextPowerOfTwo( int value )
    {
        if ( value == 0 )
        {
            return 1;
        }

        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;

        return value + 1;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsPowerOfTwo( int value )
    {
        return ( value != 0 ) && ( ( value & ( value - 1 ) ) == 0 );
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static byte ClampByte( byte value, byte min, byte max )
    {
        return Clamp( value, ( byte )min, ( byte )max );
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static byte Clamp( byte value, byte min, byte max )
    {
        if ( value < min )
        {
            return min;
        }

        return value > max ? max : value;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static short Clamp( short value, short min, short max )
    {
        if ( value < min )
        {
            return min;
        }

        return value > max ? max : value;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int Clamp( int value, int min, int max )
    {
        if ( value < min )
        {
            return min;
        }

        return value > max ? max : value;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static long Clamp( long value, long min, long max )
    {
        if ( value < min )
        {
            return min;
        }

        return value > max ? max : value;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float Clamp( float value, float min, float max )
    {
        if ( value < min )
        {
            return min;
        }

        return value > max ? max : value;
    }

    /// <summary>
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static double Clamp( double value, double min, double max )
    {
        if ( value < min )
        {
            return min;
        }

        return value > max ? max : value;
    }

    /// <summary>
    /// Linearly interpolates between fromValue to toValue on progress position.
    /// </summary>
    public static float Lerp( float fromValue, float toValue, float progress )
    {
        return fromValue + ( ( toValue - fromValue ) * progress );
    }

    /// <summary>
    /// Linearly interpolates between two angles in radians. Takes into account that
    /// angles wrap at two pi and always takes the direction with the smallest delta angle.
    /// </summary>
    /// <param name="fromRadians"> start angle in radians </param>
    /// <param name="toRadians"> target angle in radians </param>
    /// <param name="progress"> interpolation value in the range [0, 1] </param>
    /// <returns> the interpolated angle in the range [0, PI2[  </returns>
    public static float LerpAngle( float fromRadians, float toRadians, float progress )
    {
        float delta = ( ( toRadians - fromRadians + Pi2 + Pi ) % Pi2 ) - Pi;

        return ( fromRadians + ( delta * progress ) + Pi2 ) % Pi2;
    }

    /// <summary>
    /// Linearly interpolates between two angles in degrees. Takes into account
    /// that angles wrap at 360 degrees and always takes the direction with the
    /// smallest delta angle.
    /// </summary>
    /// <param name="fromDegrees"> start angle in degrees </param>
    /// <param name="toDegrees"> target angle in degrees </param>
    /// <param name="progress"> interpolation value in the range [0, 1] </param>
    /// <returns> the interpolated angle in the range [0, 360[  </returns>
    public static float LerpAngleDeg( float fromDegrees, float toDegrees, float progress )
    {
        float delta = ( ( toDegrees - fromDegrees + 360 + 180 ) % 360 ) - 180;

        return ( fromDegrees + ( delta * progress ) + 360 ) % 360;
    }

    /// <summary>
    /// Returns the largest integer less than or equal to the specified float.
    /// This method will only properly floor floats from
    /// -(2^14) to (Float.MAX_VALUE - 2^14).
    /// </summary>
    public static int Floor( float value )
    {
        return ( int )( value + BigEnoughFloor ) - BigEnoughInt;
    }

    /// <summary>
    /// Returns the largest integer less than or equal to the specified float.
    /// This method will only properly floor floats that are positive.
    /// Note this method simply casts the float to int.
    /// </summary>
    public static int FloorPositive( float value )
    {
        return ( int )value;
    }

    /// <summary>
    /// Returns the smallest integer greater than or equal to the specified float.
    /// This method will only properly ceil floats from -(2^14) to (Float.MAX_VALUE - 2^14).
    /// </summary>
    public static int Ceil( float value )
    {
        return ( int )( value + BigEnoughCeil ) - BigEnoughInt;
    }

    /// <summary>
    /// Returns the smallest integer greater than or equal to the specified float.
    /// This method will only properly ceil floats that are positive.
    /// </summary>
    public static int CeilPositive( float value )
    {
        return ( int )( value + Ceiling );
    }

    /// <summary>
    /// Returns the closest integer to the specified float.
    /// This method will only properly round floats from -(2^14) to (Float.MAX_VALUE - 2^14).
    /// </summary>
    public static int Round( float value )
    {
        return ( int )( value + BigEnoughRound ) - BigEnoughInt;
    }

    /// <summary>
    /// Returns the closest integer to the specified float.
    /// This method will only properly round floats that are positive.
    /// </summary>
    public static int RoundPositive( float value )
    {
        return ( int )( value + 0.5f );
    }

    /// <summary>
    /// Returns true if the value is zero.
    /// </summary>
    /// <param name="value">the value to test.</param>
    /// <param name="tolerance"> represent an upper bound below which the value is considered zero.</param>
    public static bool IsZero( float value, float tolerance = NumberUtils.FloatTolerance )
    {
        return Math.Abs( value ) <= tolerance;
    }

    /// <summary>
    /// Calculates the logarithm of a specified value with a specified base.
    /// </summary>
    /// <param name="a">The base of the logarithm.</param>
    /// <param name="value">The value for which the logarithm is to be calculated.</param>
    /// <returns>The logarithm of <paramref name="value"/> with base <paramref name="a"/>.</returns>
    public static float Log( float a, float value )
    {
        return ( float )( Math.Log( value ) / Math.Log( a ) );
    }

    /// <summary>
    /// Returns the base-2 logarithm of the specified value.
    /// </summary>
    /// <param name="value">The value for which to calculate the base-2 logarithm.</param>
    /// <returns>The base-2 logarithm of the specified value.</returns>
    public static float Log2( float value )
    {
        return Log( 2, value );
    }

    /// <summary>
    /// Helper method for convenience. Simply 'converts' a ubyte to an int.
    /// </summary>
    /// <param name="b"> The 8-bit unsigned byte to convert. </param>
    public static int UnsignedByteToInt( byte b )
    {
        return b & 0xFF;
    }

    // ========================================================================

    /// <summary>
    /// Provides a precomputed table of sine values to enhance performance for
    /// trigonometric calculations. The table is statically initialized and
    /// supports operations like fast sine and cosine lookups.
    /// </summary>
    public class SinClass
    {
        public static readonly float[] Table = new float[ SinCount ];

        static SinClass()
        {
            for ( var i = 0; i < SinCount; i++ )
            {
                Table[ i ] = ( float )Math.Sin( ( i + 0.5f ) / SinCount * RadFull );
            }

            for ( var i = 0; i < 360; i += 90 )
            {
                Table[ ( int )( i * DegToIndex ) & SinMask ] = ( float )Math.Sin( i * DegreesToRadians );
            }
        }
    }
}

// ========================================================================
// ========================================================================