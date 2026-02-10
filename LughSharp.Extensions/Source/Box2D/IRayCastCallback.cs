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

using System.Numerics;

using JetBrains.Annotations;

namespace Extensions.Source.Box2D;

[PublicAPI]
public interface IRayCastCallback
{
    /// <summary>
    /// Called for each fixture found in the query. You control how the ray cast proceeds
    /// by returning a float:
    /// <li>return -1: ignore this fixture and continue.</li>
    /// <li>return 0: terminate the ray cast.</li>
    /// <li>return fraction: clip the ray to this point.</li>
    /// <li>return 1: don't clip the ray and continue.</li>
    /// <br></br>
    /// The <see cref="Vector2"/> instances passed to the callback will be reused for future
    /// calls so make a copy of them!
    /// </summary>
    /// <param name="fixture"> the fixture hit by the ray </param>
    /// <param name="point"> the point of initial intersection </param>
    /// <param name="normal"> the normal vector at the point of intersection </param>
    /// <param name="fraction"></param>
    float ReportRayFixture( Fixture fixture, Vector2 point, Vector2 normal, float fraction );
}

// ============================================================================
// ============================================================================