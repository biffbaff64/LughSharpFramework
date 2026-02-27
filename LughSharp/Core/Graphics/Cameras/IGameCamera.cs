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

using JetBrains.Annotations;

using LughSharp.Core.Graphics.Viewports;
using LughSharp.Core.Maths;

namespace LughSharp.Core.Graphics.Cameras;

[PublicAPI]
public interface IGameCamera
{
    Viewport?             Viewport         { get; set; }
    OrthographicCamera    Camera           { get; set; }
    string                Name             { get; set; }
    Vector3?              LerpVector       { get; set; }
    bool                  IsInUse          { get; set; }
    bool                  IsLerpingEnabled { get; set; }
    Vector3               Position         { get; }
    float                 PPM              { get; set; }
    float                 CameraZoom       { get; set; }
    Viewport.ViewportType ViewportType     { get; set; }

    // ========================================================================

    void SetPosition( Vector3 position );

    void SetPosition( Vector3 position, float? zoom );

    void SetPosition( Vector3 position, float? zoom, bool shake );

    void UpdatePosition( float x, float y );

    void LerpTo( Vector3 position, float speed );

    void LerpTo( Vector3 position, float speed, float zoom, bool shake );

    void ResizeViewport( int width, int height, bool centerCamera );

    void SetZoomDefault( float zoom );

    void Reset();
}

// ========================================================================
// ========================================================================