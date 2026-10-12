// // /////////////////////////////////////////////////////////////////////////////
// //  MIT License
// //
// //  Copyright (c) 2024 Richard Ikin
// //
// //  Permission is hereby granted, free of charge, to any person obtaining a copy
// //  of this software and associated documentation files (the "Software"), to deal
// //  in the Software without restriction, including without limitation the rights
// //  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// //  copies of the Software, and to permit persons to whom the Software is
// //  furnished to do so, subject to the following conditions:
// //
// //  The above copyright notice and this permission notice shall be included in all
// //  copies or substantial portions of the Software.
// //
// //  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// //  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// //  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// //  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// //  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// //  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// //  SOFTWARE.
// // /////////////////////////////////////////////////////////////////////////////

using LughSharp.Source.Graphics.Images;
using LughSharp.Source.IO;
using LughSharp.Source.Scene2D;
using LughSharp.Source.Scene2D.Actions;
using LughSharp.Source.Scene2D.UI;
using LughSharp.Source.Scene2D.UI.Styles;
using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Input;

[PublicAPI]
public class VirtualController()
{
    public Touchpad Touchpad                   { get; set; } = null!;
    public Dir      CurrentRegisteredDirection { get; set; }
    public Dir      LastRegisteredDirection    { get; set; }

    public delegate void UserCallback( Dir direction );

    // ========================================================================

    private const float PadX      = 10;
    private const float PadY      = 10;
    private const float PadWidth  = 240;
    private const float PadHeight = 240;

    private const string BackgroundAsset = "touch_background.png";
    private const string BallAsset       = "joystick_ball.png";

    // ========================================================================

    /// <summary>
    /// Creates a Scene2D Virtual Controller that emulates a Joystick on-screen. 
    /// </summary>
    /// <param name="stage">
    /// The <see cref="Stage"/> to use, which <b>must</b> have added to the
    /// <see cref="InputMultiplexer"/> before passing.
    /// </param>
    /// <returns></returns>
    public VirtualController Create( Stage? stage )
    {
        Guard.Against.Null( stage );

        var touchpadSkin = new Skin( new FileInfo( @$"{Files.AssetsRoot}Skins\uiskin.json" ) );
        touchpadSkin.Add( "background", new Texture2D( @$"{Files.AssetsRoot}\touch_background.png" ) );
        touchpadSkin.Add( "ball", new Texture2D( @$"{Files.AssetsRoot}\joystick_ball.png" ) );

        ISceneDrawable touchBackground = touchpadSkin.GetDrawable( "background" );
        ISceneDrawable touchKnob       = touchpadSkin.GetDrawable( "ball" );

        var touchpadStyle = new TouchpadStyle
        {
            Background = touchBackground,
            Knob       = touchKnob,
        };

        Touchpad = new Touchpad( 1, touchpadStyle );
        Touchpad.SetBounds( PadX, PadY, PadWidth, PadHeight );
        Touchpad.SetStage( stage );
        Touchpad.ResetOnTouchUp = true;
        Touchpad.DebugActive    = true;
        
        stage.AddActor( Touchpad );

        Hide();

        return this;
    }

    /// <summary>
    /// Keeps track of the position of the touchpad knob, and calls a callback when
    /// the knob is moved.
    /// </summary>
    /// <param name="userProcessEvaluatedDirection">
    /// The callback to call when the touchpad knob is moved.
    /// </param>
    public void Update( UserCallback? userProcessEvaluatedDirection )
    {
        Dir direction = EvaluateJoypadDirection();

        userProcessEvaluatedDirection?.Invoke( direction );
    }

    /// <summary>
    /// Shows the touchpad.
    /// </summary>
    /// <returns> This controller for chaining. </returns>
    public VirtualController Show()
    {
        Touchpad.AddAction( SceneActions.Show() );

        return this;
    }

    /// <summary>
    /// Hides the touchpad.
    /// </summary>
    /// <returns> This controller for chaining. </returns>
    public VirtualController Hide()
    {
        Touchpad.AddAction( SceneActions.Hide() );

        return this;
    }

    /// <inheritdoc cref="Touchpad.KnobPercentX"/>
    public float GetXPercent() => Touchpad.KnobPercentX;

    /// <inheritdoc cref="Touchpad.KnobPercentY"/>
    public float GetYPercent() => Touchpad.KnobPercentY;

    /// <summary>
    /// Translates the joypad direction into a <see cref="Dir"/> value.
    /// </summary>
    /// <returns>The translated direction.</returns>
    private Dir EvaluateJoypadDirection()
    {
        // The default angle for joystick goes round anti-clockwise,
        // so modify so that the result is now clockwise.
        float angle = Math.Abs( GetJoystickAngle() - 360 );

        Dir joyDir = DirectionTranslationMap.map[ ( Index )( angle / 10 ) ].Translated;

        LastRegisteredDirection = joyDir;

        return joyDir;
    }

    /// <summary>
    /// Returns the angle of the joystick in degrees.
    /// </summary>
    /// <returns>The angle of the joystick in degrees.</returns>
    public float GetJoystickAngle()
    {
        return GetJoystickVector().AngleDeg();
    }

    /// <summary>
    /// Returns the vector representation of the joystick position.
    /// </summary>
    /// <returns>The vector representation of the joystick position.</returns>
    public Vector2 GetJoystickVector()
    {
        float xPerc = Touchpad.KnobPercentX;
        float yPerc = Touchpad.KnobPercentY;

        Vector2 vector2 = new Vector2( xPerc, yPerc );

        return vector2.Rotate90( -1 );
    }

    /// <summary>
    /// Removes the touchpad from the virtual controller.
    /// </summary>
    public void RemoveTouchpad()
    {
        Touchpad.AddAction( SceneActions.RemoveActor() );
    }

    // ========================================================================

    /// <summary>
    ///  
    /// </summary>
    [PublicAPI]
    public class DirectionTranslationMap
    {
        //@formatter:off
        public static readonly Movement.DirectionValue[] map =
        {
/* 00 */ new( Movement.DirectionStill, Movement.DirectionUp,    Dir.Up ),
/* 01 */ new( Movement.DirectionStill, Movement.DirectionUp,    Dir.Up ),
/* 02 */ new( Movement.DirectionStill, Movement.DirectionUp,    Dir.Up ),
/* 03 */ new( Movement.DirectionRight, Movement.DirectionUp,    Dir.UpRight ),
/* 04 */ new( Movement.DirectionRight, Movement.DirectionUp,    Dir.UpRight ),
/* 05 */ new( Movement.DirectionRight, Movement.DirectionUp,    Dir.UpRight ),
/* 06 */ new( Movement.DirectionRight, Movement.DirectionStill, Dir.Right ),
/* 07 */ new( Movement.DirectionRight, Movement.DirectionStill, Dir.Right ),
/* 08 */ new( Movement.DirectionRight, Movement.DirectionStill, Dir.Right ),
/* 09 */ new( Movement.DirectionRight, Movement.DirectionStill, Dir.Right ),
/* 10 */ new( Movement.DirectionRight, Movement.DirectionStill, Dir.Right ),
/* 11 */ new( Movement.DirectionRight, Movement.DirectionStill, Dir.Right ),
/* 12 */ new( Movement.DirectionRight, Movement.DirectionDown,  Dir.DownRight ),
/* 13 */ new( Movement.DirectionRight, Movement.DirectionDown,  Dir.DownRight ),
/* 14 */ new( Movement.DirectionRight, Movement.DirectionDown,  Dir.DownRight ),
/* 15 */ new( Movement.DirectionStill, Movement.DirectionDown,  Dir.Down ),
/* 16 */ new( Movement.DirectionStill, Movement.DirectionDown,  Dir.Down ),
/* 17 */ new( Movement.DirectionStill, Movement.DirectionDown,  Dir.Down ),
/* 18 */ new( Movement.DirectionStill, Movement.DirectionDown,  Dir.Down ),
/* 19 */ new( Movement.DirectionStill, Movement.DirectionDown,  Dir.Down ),
/* 20 */ new( Movement.DirectionStill, Movement.DirectionDown,  Dir.Down ),
/* 21 */ new( Movement.DirectionLeft,  Movement.DirectionDown,  Dir.DownLeft ),
/* 22 */ new( Movement.DirectionLeft,  Movement.DirectionDown,  Dir.DownLeft ),
/* 23 */ new( Movement.DirectionLeft,  Movement.DirectionDown,  Dir.DownLeft ),
/* 24 */ new( Movement.DirectionLeft,  Movement.DirectionStill, Dir.Left ),
/* 25 */ new( Movement.DirectionLeft,  Movement.DirectionStill, Dir.Left ),
/* 26 */ new( Movement.DirectionLeft,  Movement.DirectionStill, Dir.Left ),
/* 27 */ new( Movement.DirectionLeft,  Movement.DirectionStill, Dir.Left ),
/* 28 */ new( Movement.DirectionLeft,  Movement.DirectionStill, Dir.Left ),
/* 29 */ new( Movement.DirectionLeft,  Movement.DirectionStill, Dir.Left ),
/* 30 */ new( Movement.DirectionLeft,  Movement.DirectionUp,    Dir.UpLeft ),
/* 31 */ new( Movement.DirectionLeft,  Movement.DirectionUp,    Dir.UpLeft ),
/* 32 */ new( Movement.DirectionLeft,  Movement.DirectionUp,    Dir.UpLeft ),
/* 33 */ new( Movement.DirectionStill, Movement.DirectionUp,    Dir.Up ),
/* 34 */ new( Movement.DirectionStill, Movement.DirectionUp,    Dir.Up ),
/* 35 */ new( Movement.DirectionStill, Movement.DirectionUp,    Dir.Up ),
/* 36 */ new( Movement.DirectionStill, Movement.DirectionStill, Dir.Still ),
        };
        //@formatter:on
    }
}

// ============================================================================
// ============================================================================
