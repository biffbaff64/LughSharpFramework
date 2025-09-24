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

namespace LughSharp.Lugh.Graphics.Cameras;

/// <summary>
/// A 2D Camera with Orthographic Projection.
/// </summary>
[PublicAPI]
public class OrthographicCamera : Camera
{
    protected const float DEFAULT_ZOOM = 1.0f;

    // ========================================================================

    private readonly Vector3 _tmp = new();

    private float _zoom;

    // ========================================================================

    /// <summary>
    /// Constructs a default OrthographicCamera.
    /// All properties, such as viewport size etc, will need setting up before use.
    /// </summary>
    public OrthographicCamera() : base()
    {
        Near = 0f;
        Far  = 1f;
        Zoom = DEFAULT_ZOOM;
    }

    /// <summary>
    /// Constructs a new OrthographicCamera, using the given viewport width and height.
    /// For pixel perfect 2D rendering just supply the screen size, for other unit scales
    /// (e.g. meters for box2d) proceed accordingly. The camera will show the region
    /// <c>[-viewportWidth/2, -(viewportHeight/2-1)] - [(viewportWidth/2-1), viewportHeight/2].</c>
    /// The constructor does not call <see cref="Update"/> so this will need to be done
    /// after object construction. This is because <c>Update</c> is a <c>virtual</c> method
    /// and thus should not be called in the constructor.
    /// </summary>
    /// <param name="viewportWidth"> Width of this cameras viewport. </param>
    /// <param name="viewportHeight"> Height of this cameras viewport. </param>
    public OrthographicCamera( float viewportWidth, float viewportHeight ) : base()
    {
        ViewportWidth  = viewportWidth;
        ViewportHeight = viewportHeight;
        Near           = 0f;
        Far            = 1f;
        Zoom           = DEFAULT_ZOOM;
    }

    // ========================================================================

    /// <summary>
    /// Sets this camera to an orthographic projection using a viewport fitting
    /// the screen resolution, centered at:-
    /// <code>(GdxApi.graphics.getWidth()/2, GdxApi.graphics.getHeight()/2)</code>
    /// with the y-axis pointing up or down.
    /// </summary>
    /// <param name="yDown">whether y should be pointing down.</param>
    public void SetToOrtho( bool yDown )
    {
        SetToOrtho( Api.Graphics.Width, Api.Graphics.Height, yDown );
    }

    /// <summary>
    /// Sets this camera to an orthographic projection, centered at
    /// (viewportWidth/2, viewportHeight/2), with the y-axis pointing up or down.
    /// </summary>
    /// <param name="viewportWidth"></param>
    /// <param name="viewportHeight"></param>
    /// <param name="yDown">whether y should be pointing down.</param>
    public void SetToOrtho( float viewportWidth, float viewportHeight, bool yDown )
    {
        Up.Set( 0f, yDown ? -1f : 1f, 0 );
        Direction.Set( 0f, 0f, yDown ? 1f : -1f );
        Position.Set( ( Zoom * viewportWidth ) / 2.0f, ( Zoom * viewportHeight ) / 2.0f, 0f );

        ViewportWidth  = viewportWidth;
        ViewportHeight = viewportHeight;

        Update();
    }

    /// <summary>
    /// Updates the camera.
    /// Also updates the frustrum if <paramref name="updateFrustrum" /> is true.
    /// </summary>
    /// <param name="updateFrustrum">
    /// if <c>true</c> the frustrum will be updated. The frustum is a rectangle that
    /// defines the visible area on the screen. This rectangle is bounded by the near
    /// and far planes (which are effectively just a distance in 2D) and the camera's
    /// viewport dimensions.
    /// </param>
    public override void Update( bool updateFrustrum = true )
    {
        var left   = ( Zoom * -ViewportWidth ) / 2f;
        var right  = Zoom * ( ViewportWidth / 2f );
        var bottom = Zoom * -( ViewportHeight / 2f );
        var top    = ( Zoom * ViewportHeight ) / 2f;

        ProjectionMatrix.SetToOrtho( left, right, bottom, top, Near, Far );
        ViewMatrix.SetToLookAt( Position, _tmp.Set( Position ).Add( Direction ), Up );
        Combined.Set( ProjectionMatrix );
        Matrix4.Mul( Combined.Val, ViewMatrix.Val );

        if ( updateFrustrum )
        {
            InvProjectionView.Set( Combined );
            Matrix4.Invert( InvProjectionView.Val );
            Frustrum.Update( InvProjectionView );
        }
    }

    /// <summary>
    /// Rotates the camera by the given angle around the direction vector.
    /// The direction and up vector will not be orthogonalized.
    /// </summary>
    /// <param name="angle"></param>
    public void Rotate( float angle )
    {
        Rotate( Direction, angle );
    }

    /// <summary>
    /// Moves the camera by the given amount on each axis.
    /// </summary>
    /// <param name="x"> the displacement on the x-axis </param>
    /// <param name="y"> the displacement on the y-axis </param>
    public void Translate( float x, float y )
    {
        Translate( x, y, 0f );
    }

    /// <summary>
    /// Moves the camera by the given vector.
    /// </summary>
    /// <param name="vec">the displacement vector</param>
    public void Translate( Vector2 vec )
    {
        Translate( vec.X, vec.Y, 0f );
    }

    /// <summary>
    /// Gets or sets the zoom level for the orthographic camera.
    /// </summary>
    /// <remarks>
    /// The zoom scales the dimensions of the orthographic projection. Higher zoom
    /// values make objects appear larger (closer zoom), while lower values reduce
    /// the size of objects (zoom out). The value must be greater than 0, as a zoom
    /// of 0 is not valid and will throw an exception during camera updates.
    /// Default value is 1.0f. Setting this property to a value less than or equal
    /// to 0 will internally clamp it to a small positive value to prevent invalid
    /// behavior.
    /// </remarks>
    public float Zoom
    {
        get
        {
            _zoom = Math.Max( 0.0001f, _zoom );

            return _zoom;
        }
        set => _zoom = value;
    }
}

// ============================================================================
// ============================================================================