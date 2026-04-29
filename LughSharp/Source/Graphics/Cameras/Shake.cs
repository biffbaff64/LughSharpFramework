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

namespace LughSharp.Source.Graphics.Cameras;

/// <summary>
/// Represents a screen shake effect for use with cameras. Provides methods to start,
/// update, and reset the shake effect, allowing randomized movements to simulate a
/// dynamic shaking motion. Includes properties to enable or restrict the use of
/// screen shake.
/// </summary>
[PublicAPI]
public static class Shake
{
    /// <summary>
    /// Determines whether screen shake is allowed. Set externally to control
    /// whether shake effects are applied. The Shake effect will not be applied
    /// until this property is set to true and <see cref="Start"/> is called.
    /// </summary>
    public static bool ScreenShakeAllowed { get; set; }

    // ========================================================================

    private const float DefaultShakeDuration  = 450f;
    private const float DefaultShakeIntensity = 10f;

    private static float               _elapsedTime;
    private static float               _shakeDuration;
    private static float               _shakeIntensity;
    private static Vector3             _positionSave = Vector3.Zero;
    private static OrthographicCamera? _camera;

    // ========================================================================

    /// <summary>
    /// Initiates the screen shake effect with predefined settings, such as
    /// default duration, radius, and intensity, if screen shake is permitted.
    /// Any currently running shake effects are overridden.
    /// </summary>
    public static void Start( OrthographicCamera? camera )
    {
        Start( TimeSpan.FromMilliseconds( DefaultShakeDuration ), DefaultShakeIntensity, camera );
    }

    /// <summary>
    /// Starts the screen shake effect with custom parameters if screen shake is allowed.
    /// Initializes the shake duration, radius, and intensity, and any currently running
    /// shake effects are overridden.
    /// </summary>
    /// <param name="duration"> The duration of the shake effect, in <b>seconds.</b> </param>
    /// <param name="intensity">
    /// The intensity of the shake effect, in world units, affecting the radius.
    /// </param>
    /// <param name="camera">
    /// The <see cref="OrthographicCamera"/> to which the shake effect will be applied.
    /// </param>
    public static void Start( TimeSpan duration, float intensity, OrthographicCamera? camera )
    {
        if ( ScreenShakeAllowed )
        {
            _shakeDuration  = ( float )duration.TotalSeconds;
            _shakeIntensity = intensity;
            _elapsedTime    = 0f;
            _positionSave   = camera?.Position ?? Vector3.Zero;
            _camera         = camera;

            Logger.Debug( "Camera Shake Started!" );
        }
    }

    /// <summary>
    /// Updates the state of the screen shake effect based on the elapsed time and
    /// camera properties. If screen shake is enabled and allowed, applies a random
    /// shake effect to the provided camera.
    /// Automatically resets the shake effect when the shake duration is completed.
    /// </summary>
    /// <param name="delta">
    /// The elapsed time since the last frame, used to update shake progression.
    /// </param>
    public static void Update( float delta )
    {
        if ( _camera == null )
        {
            return;
        }

        if ( ScreenShakeAllowed )
        {
            // Only shake when required.
            if ( _elapsedTime < _shakeDuration )
            {
                // Calculate the amount of shake based on how long it has been shaking already
                float currentPower = _shakeIntensity * _camera.Zoom
                                                     * ( ( _shakeDuration - _elapsedTime ) / _shakeDuration );

                float x = ( MathUtils.Random() - 0.5f ) * currentPower;
                float y = ( MathUtils.Random() - 0.5f ) * currentPower;

                _camera.Translate( -x, -y );

                // Increase the elapsedTime time by the delta provided.
                _elapsedTime += delta;
            }
        }
    }
}

// ========================================================================
// ========================================================================