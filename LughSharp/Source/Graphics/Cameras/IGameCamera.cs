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


using LughSharp.Source.Graphics.Viewports;

namespace LughSharp.Source.Graphics.Cameras;

[PublicAPI]
public interface IGameCamera
{
    /// <summary>
    /// The current viewport associated with the GameCamera.
    /// </summary>
    Viewport? Viewport { get; set; }

    /// <summary>
    /// The underlying <see cref="OrthographicCamera"/> instance used by the GameCamera.
    /// </summary>
    OrthographicCamera Camera { get; set; }

    /// <summary>
    /// A vector used to define the target position for linear interpolation (lerp) of
    /// the camera's position and zoom.
    /// </summary>
    Vector3? LerpVector { get; set; }

    /// <summary>
    /// Pixels-per-meter (PPM) value used for scaling the camera's view.
    /// </summary>
    float PPM { get; set; }

    /// <summary>
    /// The type of viewport to use for rendering. This determines the size and aspect
    /// ratio of the camera's view.
    /// <para>
    /// Default is <see cref="Viewport.ViewportType.Stretch"/>. Other valid options include:
    /// <see cref="Viewport.ViewportType.Fit"/>, <see cref="Viewport.ViewportType.Extended"/>,
    /// <see cref="Viewport.ViewportType.Fill"/>, <see cref="Viewport.ViewportType.Screen"/>,
    /// and <see cref="Viewport.ViewportType.Scaling"/>.
    /// </para>
    /// </summary>
    Viewport.ViewportType ViewportType { get; set; }

    string  Name             { get; set; }
    bool    IsInUse          { get; set; }
    bool    IsLerpingEnabled { get; set; }
    Vector3 Position         { get; set; }

    // ========================================================================

    /// <summary>
    /// Sets the position of the camera in the scene by updating the camera's 3D coordinates.
    /// </summary>
    /// <param name="position">
    /// The new position to set for the camera, represented as a <see cref="Vector3"/>.
    /// </param>
    void SetPosition( Vector3 position );

    /// <summary>
    /// Sets the position of the camera in the scene and adjusts the zoom level.
    /// Updates the camera's position in 3D space while optionally modifying the
    /// zoom factor.
    /// </summary>
    /// <param name="position">
    /// The new position of the camera in 3D space, represented as a <see cref="Vector3"/>.
    /// </param>
    /// <param name="zoom">
    /// The zoom adjustment to apply to the camera. Positive values increase zoom, and
    /// negative values decrease zoom.
    /// </param>
    void SetPosition( Vector3 position, float? zoom );

    /// <summary>
    /// Sets the position and configuration of the camera in the scene.
    /// Updates the camera's position and zoom settings. Optionally applies a
    /// shake effect to simulate camera movement or vibration, if specified.
    /// This does not call <see cref="Camera.Update"/>, which should be called
    /// after setting the position to ensure the camera is updated.
    /// </summary>
    /// <param name="position">The new position of the camera in 3D space.</param>
    /// <param name="zoom">
    /// The zoom level to apply to the camera. A positive value increases zoom.
    /// </param>
    /// <param name="shake">
    /// A boolean indicating whether to apply a shake effect to the camera.
    /// </param>
    void SetPosition( Vector3 position, float? zoom, bool shake );

    /// <summary>
    /// Updates the position of the camera based on the current state.
    /// This method is typically used for internal updates to reposition the camera
    /// when it is active and marked as in use.
    /// </summary>
    void UpdatePosition( float x, float y );

    /// <summary>
    /// Smoothly transitions the camera's position towards a target position at a
    /// specified speed.
    /// This method is commonly used for implementing camera movement while maintaining
    /// a smooth interpolation effect.
    /// The interpolation is performed only if the camera is active, lerping is enabled,
    /// and valid vector and camera components exist.
    /// </summary>
    /// <param name="position">The target position to which the camera should move.
    /// </param>
    /// <param name="speed">
    /// The speed of the interpolation, controlling how quickly the camera transitions
    /// towards the target position.
    /// </param>
    void LerpTo( Vector3 position, float speed );

    /// <summary>
    /// Smoothly interpolates the camera position, zoom, and applies optional shake
    /// effects. This method is used to transition the camera state to a target
    /// position over a period of time, providing a natural and fluid movement effect.
    /// </summary>
    /// <param name="position">
    /// The target position to interpolate the camera position towards.
    /// </param>
    /// <param name="speed">
    /// The speed of the interpolation. Higher values result in faster transitions.
    /// </param>
    /// <param name="zoom">
    /// The incremental zoom value to apply during the interpolation.
    /// </param>
    /// <param name="shake">
    /// Indicates whether shake effects should be applied during the transition.
    /// </param>
    void LerpTo( Vector3 position, float speed, float zoom, bool shake );

    /// <summary>
    /// Resizes the viewport to the specified width and height.
    /// Optionally centers the camera within the viewport during resizing.
    /// </summary>
    /// <param name="width">The new width of the viewport.</param>
    /// <param name="height">The new height of the viewport.</param>
    /// <param name="centerCamera">
    /// A boolean value indicating whether the camera should be centered within
    /// the viewport after resizing.
    /// </param>
    void ResizeViewport( int width, int height, bool centerCamera );

    /// <summary>
    /// Sets the default zoom level for the camera and updates its current zoom value.
    /// </summary>
    /// <param name="zoom">The zoom level to set as default and apply to the camera.</param>
    void SetZoomDefault( float zoom );

    /// <summary>
    /// Resets the camera's position and zoom to their default values, ensuring the
    /// camera is centered and updated to reflect these changes. This operation sets
    /// the zoom level to default and clears the position to its zero vector.
    /// </summary>
    void Reset();
}

// ========================================================================
// ========================================================================

