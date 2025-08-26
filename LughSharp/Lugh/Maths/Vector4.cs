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

namespace LughSharp.Lugh.Maths;

[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct Vector4 : IEquatable< Vector4 >
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public static Vector4 One => new( 1f, 1f, 1f, 1f );

    public Vector4( float x, float y, float z, float w )
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    // For shader interop, we need to be able to compare vectors
    public bool Equals( Vector4 other )
    {
        return X.Equals( other.X )
               && Y.Equals( other.Y )
               && Z.Equals( other.Z )
               && W.Equals( other.W );
    }

    public override bool Equals( object? obj )
    {
        return obj is Vector4 other && Equals( other );
    }

    public override int GetHashCode()
    {
        return HashCode.Combine( X, Y, Z, W );
    }

    public static bool operator ==( Vector4 left, Vector4 right )
    {
        return left.Equals( right );
    }

    public static bool operator !=( Vector4 left, Vector4 right )
    {
        return !left.Equals( right );
    }
}

// ========================================================================
// ========================================================================