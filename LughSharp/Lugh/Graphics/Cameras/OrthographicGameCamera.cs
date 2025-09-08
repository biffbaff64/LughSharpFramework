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

using LughSharp.Lugh.Graphics.Viewports;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Logging;

namespace LughSharp.Lugh.Graphics.Cameras;

/// <summary>
/// Represents a 2D orthographic game camera that handles camera configuration,
/// movement, zoom levels, and viewport settings. This class integrates functionalities
/// for camera animations such as lerp and shake effects. Implements <see cref="IGameCamera"/>
/// for standard camera functionality and <see cref="IDisposable"/> for resource management.
/// </summary>
[PublicAPI]
public class OrthographicGameCamera : IGameCamera, IDisposable
{
    public Viewport?             Viewport         { get; set; }
    public OrthographicCamera    Camera           { get; set; }
    public string                Name             { get; set; }
    public Vector3?              LerpVector       { get; set; }
    public bool                  IsInUse          { get; set; }
    public bool                  IsLerpingEnabled { get; set; }
    public float                 PPM              { get; set; }
    public Vector3               Position         { get; set; }
    public Viewport.ViewportType ViewportType     { get; set; } = Viewport.ViewportType.Stretch;

    // ========================================================================

    private float _defaultZoom;
    private Shake _shake    = new();
    private bool  _disposed = false;

    // ========================================================================

    /// <summary>
    /// Represents a 2D orthographic game camera responsible for managing camera properties
    /// such as position, zoom, and viewport. Provides utilities for animations including
    /// lerp and shake effects, as well as methods for viewport resizing and configuration.
    /// This class implements <see cref="IGameCamera"/> for standardized camera functionality
    /// and <see cref="IDisposable"/> for proper resource management.
    /// <para>
    /// This constructor creates a new instance of <see cref="OrthographicGameCamera"/> with
    /// default values for scene width (<see cref="CameraData.DEFAULT_SCENE_WIDTH"/>) and height
    /// (<see cref="CameraData.DEFAULT_SCENE_HEIGHT"/>), and a PPM value of <see cref="CameraData.DEFAULT_PPM"/>.
    /// </para>
    /// </summary>
    public OrthographicGameCamera()
        : this( CameraData.DEFAULT_SCENE_WIDTH, CameraData.DEFAULT_SCENE_HEIGHT )
    {
    }

    /// <summary>
    /// Represents an orthographic camera used for 2D games, maintaining properties
    /// such as position, zoom, and viewport configurations, alongside utilities for
    /// camera movement, resizing, and animations.
    /// <para>
    /// Note: The scene width and height provided to this constructor will be divided
    /// by the PPM value to determine the camera's viewport dimensions. This allows
    /// the camera to scale to the desired resolution, while maintaining a consistent
    /// aspect ratio. To use pixel-perfect rendering, set the PPM value to 1.0f.
    /// </para>
    /// <para>
    /// Implements <see cref="IGameCamera"/> to provide standard camera functionalities
    /// and supports disposal.
    /// </para>
    /// </summary>
    /// <param name="sceneWidth"> The width of the scene. Will be divided by ppm. </param>
    /// <param name="sceneHeight"> The height of the scene. Will be divided by ppm. </param>
    /// <param name="viewportType"> The type of viewport to use. </param>
    /// <param name="ppm"> The pixels-per-meter value to use for scaling. </param>
    /// <param name="name"> The name of the camera. </param>
    public OrthographicGameCamera( float sceneWidth,
                                   float sceneHeight,
                                   Viewport.ViewportType viewportType = Viewport.ViewportType.Stretch,
                                   float ppm = CameraData.DEFAULT_PPM,
                                   string name = "" )
    {
        Name             = name;
        IsInUse          = true;
        IsLerpingEnabled = false;
        Position         = Vector3.Zero;
        LerpVector       = Vector3.Zero;
        PPM              = ppm;

        // Create the camera instance.
        Camera = new OrthographicCamera( sceneWidth / ppm, sceneHeight / ppm );
        Camera.SetToOrtho( sceneWidth / ppm, sceneHeight / ppm, false );
        Camera.Position.Set( sceneWidth / 2, sceneHeight / 2, 0 );
        Camera.Update();

        // Add the viewport
        switch ( viewportType )
        {
            case Viewport.ViewportType.Fit:
                SetFitViewport();

                break;

            case Viewport.ViewportType.Stretch:
                SetStretchViewport();

                break;

            case Viewport.ViewportType.Extended:
                SetExtendedViewport();

                break;

            case Viewport.ViewportType.Fill:
                SetFillViewport();

                break;

            case Viewport.ViewportType.Screen:
                SetScreenViewport();

                break;

            case Viewport.ViewportType.Scaling:
                SetScalingViewport( Scaling.None ); //TODO: Add Scaling

                break;

            default:
                throw new GdxRuntimeException( $"Unknown Viewport Type: {viewportType}" );
        }
    }

    // ========================================================================

    /// <summary>
    /// Sets the position of the camera in the scene by updating the camera's 3D coordinates.
    /// </summary>
    /// <param name="position">
    /// The new position to set for the camera, represented as a <see cref="Vector3"/>.
    /// </param>
    public void SetPosition( Vector3 position )
    {
        SetPosition( position, null, false );
    }

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
    public void SetPosition( Vector3 position, float? zoom )
    {
        SetPosition( position, zoom, false );
    }

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
    public void SetPosition( Vector3 position, float? zoom, bool shake )
    {
        if ( IsInUse )
        {
            Camera.Position.X = position.X;
            Camera.Position.Y = position.Y;
            Camera.Position.Z = position.Z;

            if ( zoom != null )
            {
                Camera.Zoom += ( float )zoom;
            }

            if ( shake )
            {
                _shake.Update( Api.DeltaTime, Camera );
            }
        }
    }

    /// <summary>
    /// Updates the state of the orthographic game camera. If the camera is currently in use
    /// and a valid <see cref="OrthographicCamera"/> instance is associated, this method
    /// triggers the camera's update logic to refresh its internal state.
    /// </summary>
    public void Update()
    {
        if ( IsInUse )
        {
            Camera.Update();
        }
    }

    /// <summary>
    /// Updates the position of the camera based on the current state.
    /// This method is typically used for internal updates to reposition the camera
    /// when it is active and marked as in use.
    /// </summary>
    public virtual void UpdatePosition( float x = 0, float y = 0 )
    {
        if ( IsInUse )
        {
            var temp = Camera.Position;

            temp.X = Camera.Position.X + ( ( x - Camera.Position.X ) * 0.1f );
            temp.Y = Camera.Position.Y + ( ( y - Camera.Position.Y ) * 0.1f );

            Camera.Position.Set( temp );
            Camera.Update();
        }
    }

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
    public void LerpTo( Vector3 position, float speed )
    {
        if ( IsInUse && IsLerpingEnabled )
        {
            LerpVector?.Set( position.X, position.Y, position.Z );

            Camera.Position.Lerp( LerpVector!, speed );
            Camera.Update();
        }
    }

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
    public void LerpTo( Vector3 position, float speed, float zoom, bool shake )
    {
        if ( IsInUse && IsLerpingEnabled )
        {
            LerpVector?.Set( position.X, position.Y, position.Z );

            Camera.Position.Lerp( LerpVector!, speed );
            Camera.Zoom += zoom;

            if ( shake )
            {
                _shake.Update( Api.DeltaTime, Camera );
            }

            Camera.Update();
        }
    }

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
    public void ResizeViewport( int width, int height, bool centerCamera = false )
    {
        if ( Viewport == null )
        {
            return;
        }

        Viewport.Update( width, height, centerCamera );
        Camera.Update();
    }

    /// <summary>
    /// Configures the camera to use a stretch viewport, setting the viewport dimensions
    /// and applying the changes. The stretch viewport dynamically scales the camera's
    /// view to fit the available screen space.
    /// <para>
    /// This method creates a new instance of <see cref="StretchViewport"/>, using the camera's
    /// current width, height, and pixels-per-meter (PPM) setting. The viewport is then applied
    /// to synchronize its state.
    /// </para>
    /// </summary>
    public void SetStretchViewport()
    {
        Logger.Debug( $"PPM: {PPM}, "
                      + $"Camera: X:{Camera.Position.X}, Y:{Camera.Position.Y}, Z:{Camera.Position.Z},"
                      + $" {Camera.ViewportWidth * PPM}x{Camera.ViewportHeight * PPM}"
                      + $" ( {Camera.ViewportWidth}x{Camera.ViewportHeight} Units )" );

        Viewport = new StretchViewport( Camera.ViewportWidth * PPM,
                                        Camera.ViewportHeight * PPM,
                                        Camera );

        Viewport.SetScreenBounds( 0, 0,
                                  ( int )( Camera.ViewportWidth * PPM ),
                                  ( int )( Camera.ViewportHeight * PPM ) );

        Viewport.Apply();
    }

    /// <summary>
    /// Configures the camera to use a FitViewport, matching the world dimensions
    /// with the aspect ratio preserved. This viewport ensures that the entire
    /// world is visible within the bounds of the viewport, applying letterboxing
    /// or pillarboxing as needed.
    /// <para>
    /// This method requires the associated OrthographicCamera to be properly
    /// configured, including its viewport dimensions. Without a valid camera,
    /// the operation will not be performed.
    /// </para>
    /// </summary>
    public void SetFitViewport()
    {
        Viewport = new FitViewport( Camera.ViewportWidth * PPM, Camera.ViewportHeight * PPM, Camera );

        Viewport.SetScreenBounds( 0, 0,
                                  ( int )( Camera.ViewportWidth * PPM ),
                                  ( int )( Camera.ViewportHeight * PPM ) );

        Viewport.Apply();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetExtendedViewport()
    {
        Viewport = new ExtendViewport( Camera.ViewportWidth * PPM, Camera.ViewportHeight * PPM, Camera );

        Viewport.SetScreenBounds( 0, 0,
                                  ( int )( Camera.ViewportWidth * PPM ),
                                  ( int )( Camera.ViewportHeight * PPM ) );

        Viewport.Apply();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetFillViewport()
    {
        Viewport = new FillViewport( Camera.ViewportWidth * PPM, Camera.ViewportHeight * PPM, Camera );

        Viewport.SetScreenBounds( 0, 0,
                                  ( int )( Camera.ViewportWidth * PPM ),
                                  ( int )( Camera.ViewportHeight * PPM ) );

        Viewport.Apply();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetScreenViewport()
    {
        Viewport = new ScreenViewport();

        Viewport.SetScreenBounds( 0, 0,
                                  ( int )( Camera.ViewportWidth * PPM ),
                                  ( int )( Camera.ViewportHeight * PPM ) );

        Viewport.Apply();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetScalingViewport( Scaling scaling )
    {
        Viewport = new ScalingViewport( scaling, Camera.ViewportWidth * PPM, Camera.ViewportHeight * PPM, Camera );

        Viewport.SetScreenBounds( 0, 0,
                                  ( int )( Camera.ViewportWidth * PPM ),
                                  ( int )( Camera.ViewportHeight * PPM ) );

        Viewport.Apply();
    }

    // ========================================================================

    /// <summary>
    /// Resets the camera's position and zoom to their default values, ensuring the
    /// camera is centered and updated to reflect these changes. This operation sets
    /// the zoom level to default and clears the position to its zero vector.
    /// </summary>
    public void Reset()
    {
        CameraZoom = CameraData.DEFAULT_ZOOM;

        Camera.Position.SetZero();
        Camera.Update();
    }

    // ========================================================================

    /// <summary>
    /// Gets or sets the zoom level of the orthographic camera.
    /// The zoom level directly affects the scale of the view, where higher values
    /// zoom in closer and lower values zoom out. Updating this property recalculates
    /// the camera's view.
    /// If the internal camera instance is not initialized, the getter returns 0.0f.
    /// </summary>
    public float CameraZoom
    {
        get => Camera.Zoom;
        set
        {
            Camera.Zoom = value;
            Camera.Update();
        }
    }

    /// <summary>
    /// Sets the default zoom level for the camera and updates its current zoom value.
    /// </summary>
    /// <param name="zoom">The zoom level to set as default and apply to the camera.</param>
    public void SetZoomDefault( float zoom )
    {
        CameraZoom   = zoom;
        _defaultZoom = zoom;
    }

    // ========================================================================

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
    }

    protected virtual void Dispose( bool disposing )
    {
        if ( !_disposed )
        {
            if ( disposing )
            {
                //TODO:
                Camera     = null!;
                Viewport   = null;
                LerpVector = null!;
                Name       = null!;
            }

            _disposed = true;
        }
    }
}

// ========================================================================
// ========================================================================