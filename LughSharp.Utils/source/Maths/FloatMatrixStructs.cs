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

namespace LughSharp.Utils.source.Maths;

[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct Matrix2F( float m11, float m12, float m21, float m22 )
{
    public float M11 = m11;
    public float M12 = m12;
    public float M21 = m21;
    public float M22 = m22;
}

[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct Matrix3F(
    float m11,
    float m12,
    float m13,
    float m21,
    float m22,
    float m23,
    float m31,
    float m32,
    float m33 )
{
    public float M11 = m11;
    public float M12 = m12;
    public float M13 = m13;
    public float M21 = m21;
    public float M22 = m22;
    public float M23 = m23;
    public float M31 = m31;
    public float M32 = m32;
    public float M33 = m33;
}

[PublicAPI]
[StructLayout( LayoutKind.Sequential )]
public struct Matrix4F(
    float m11,
    float m12,
    float m13,
    float m14,
    float m21,
    float m22,
    float m23,
    float m24,
    float m31,
    float m32,
    float m33,
    float m34,
    float m41,
    float m42,
    float m43,
    float m44 )
{
    public float M11 = m11;
    public float M12 = m12;
    public float M13 = m13;
    public float M14 = m14;
    public float M21 = m21;
    public float M22 = m22;
    public float M23 = m23;
    public float M24 = m24;
    public float M31 = m31;
    public float M32 = m32;
    public float M33 = m33;
    public float M34 = m34;
    public float M41 = m41;
    public float M42 = m42;
    public float M43 = m43;
    public float M44 = m44;
}