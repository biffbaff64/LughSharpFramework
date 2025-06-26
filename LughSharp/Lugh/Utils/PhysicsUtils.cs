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

namespace LughSharp.Lugh.Utils;

[PublicAPI]
public static class PhysicsUtils
{
    // Box2D typically works best with objects between 0.1 and 10 meters in size
    public const float PIXELS_TO_METERS = 1f / 32f; // 32 pixels = 1 meter
    public const float METERS_TO_PIXELS = 32f;      // 1 meter = 32 pixels

    public static Vector2 ToBox2D( Vector2 pixels )
    {
        return new Vector2( pixels.X * PIXELS_TO_METERS, pixels.Y * PIXELS_TO_METERS );
    }

    public static Vector2 ToPixels( Vector2 meters )
    {
        return new Vector2( meters.X * METERS_TO_PIXELS, meters.Y * METERS_TO_PIXELS );
    }

    public static float ToBox2D( float pixels )
    {
        return pixels * PIXELS_TO_METERS;
    }

    public static float ToPixels( float meters )
    {
        return meters * METERS_TO_PIXELS;
    }
}

// ========================================================================
// ========================================================================