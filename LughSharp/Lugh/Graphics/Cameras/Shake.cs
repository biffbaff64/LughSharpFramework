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

using LughSharp.Lugh.Maths;

namespace LughSharp.Lugh.Graphics.Cameras;

/// <summary>
/// Represents a screen shake effect for use with cameras. Provides methods
/// to start, update, and reset the shake effect, allowing randomized movements
/// to simulate a dynamic shaking motion. Includes properties to enable or
/// restrict the use of screen shake.
/// </summary>
[PublicAPI]
public class Shake
{
    public bool ScreenShakeEnabled { get; set; }
    public bool ScreenShakeAllowed { get; set; } = false;

    // ========================================================================

    private float _shakeRadius    = 0;
    private float _randomAngle    = 0;
    private float _elapsedTime    = 0;
    private float _shakeDuration  = 0;
    private float _shakeIntensity = 0;

    // ========================================================================

    /// <summary>
    /// Initiates the screen shake effect with predefined settings, such as
    /// default duration, radius, and intensity, if screen shake is permitted
    /// and not currently in progress.
    /// </summary>
    public void Start()
    {
        Start( 30, 450, 10 );
    }

    /// <summary>
    /// Starts the screen shake effect with default parameters if screen shake is allowed
    /// and not already active. Initializes the shake duration, radius, and intensity.
    /// </summary>
    public void Start( float radius, float duration, float intensity )
    {
        if ( ScreenShakeAllowed )
        {
            if ( !ScreenShakeEnabled )
            {
                _shakeDuration  = duration / 1000f;
                _shakeRadius    = radius;
                _shakeIntensity = intensity;

                _elapsedTime       = 0;
                _randomAngle       = MathUtils.Random() % 360f;
                ScreenShakeEnabled = true;
            }
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
    /// <param name="camera">The camera to which the shake effect will be applied.</param>
    public void Update( float delta, OrthographicCamera camera )
    {
        if ( !ScreenShakeAllowed )
        {
            Reset();
        }
        else
        {
            if ( ScreenShakeEnabled )
            {
                // Only shake when required.
                if ( _elapsedTime < _shakeDuration )
                {
                    // Calculate the amount of shake based on how long it has been shaking already
                    var currentPower = _shakeIntensity * camera.Zoom * ( ( _shakeDuration - _elapsedTime ) / _shakeDuration );

                    var x = ( MathUtils.Random() - 0.5f ) * currentPower;
                    var y = ( MathUtils.Random() - 0.5f ) * currentPower;

                    camera.Translate( -x, -y );

                    // Increase the elapsedTime time by the delta provided.
                    _elapsedTime += delta;
                }
                else
                {
                    Reset();
                }
            }
        }
    }

    /// <summary>
    /// Resets the screen shake effect, disabling any ongoing shake.
    /// This method ensures that the ScreenShakeEnabled property is set to false,
    /// stopping any active shaking effects and marking the shake as complete.
    /// </summary>
    public void Reset()
    {
        ScreenShakeEnabled = false;
    }
}

// ========================================================================
// ========================================================================