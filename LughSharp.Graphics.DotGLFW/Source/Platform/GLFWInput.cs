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

using DotGLFW;

using JetBrains.Annotations;

using LughSharp.Graphics.Abstractions.Source.Interfaces;
using LughSharp.Lugh.Input;

namespace LughSharp.Graphics.DotGLFW.Source.Platform;

[PublicAPI]
public class GLFWInput : IInputHandler
{
    /// <inheritdoc />
    public event Action< int >? KeyPressed;

    /// <inheritdoc />
    public event Action< int >? KeyReleased;

    /// <inheritdoc />
    public event Action< float, float >? MouseMoved;

    // ========================================================================

    private readonly GLFWWindow _window;

    // ========================================================================

    public GLFWInput( GLFWWindow window )
    {
        _window = window;
    }

    /// <inheritdoc />
    public void WindowHandleChanged( Window windowHandle )
    {
    }

    /// <inheritdoc />
    public void Update()
    {
    }

    /// <inheritdoc />
    public void PrepareNext()
    {
    }

    /// <inheritdoc />
    public void ResetPollingStates()
    {
    }

    public bool IsKeyPressed( int key )
    {
        var glfwKey = ConvertToGLFWKey( key );

//        return _window.GetKey( glfwKey ) == DotGLFW.KeyState.Press;
        return false;
    }

    /// <inheritdoc />
    public bool IsButtonPressed( int button )
    {
        return false;
    }

    /// <inheritdoc />
    public (float x, float y) GetMousePosition()
    {
        return ( 0, 0 );
    }

    // ========================================================================

    private static Key ConvertToGLFWKey( int inputKey )
    {
        // Map from IInput.Keys to GLFW.Keys
        return inputKey switch
        {
            IInput.Keys.NUM_0           => Key.D0,
            IInput.Keys.NUM_1           => Key.D1,
            IInput.Keys.NUM_2           => Key.D2,
            IInput.Keys.NUM_3           => Key.D3,
            IInput.Keys.NUM_4           => Key.D4,
            IInput.Keys.NUM_5           => Key.D5,
            IInput.Keys.NUM_6           => Key.D6,
            IInput.Keys.NUM_7           => Key.D7,
            IInput.Keys.NUM_8           => Key.D8,
            IInput.Keys.NUM_9           => Key.D9,
            IInput.Keys.A               => Key.A,
            IInput.Keys.ALT_LEFT        => Key.LeftAlt,
            IInput.Keys.ALT_RIGHT       => Key.RightAlt,
            IInput.Keys.APOSTROPHE      => Key.Apostrophe,
            IInput.Keys.B               => Key.B,
            IInput.Keys.BACK            => Key.Backspace,
            IInput.Keys.BACKSLASH       => Key.Backslash,
            IInput.Keys.C               => Key.C,
            IInput.Keys.CAPS_LOCK       => Key.CapsLock,
            IInput.Keys.COMMA           => Key.Comma,
            IInput.Keys.D               => Key.D,
            IInput.Keys.BACKSPACE       => Key.Backspace,
            IInput.Keys.FORWARD_DEL     => Key.Delete,
            IInput.Keys.DOWN            => Key.Down,
            IInput.Keys.LEFT            => Key.Left,
            IInput.Keys.RIGHT           => Key.Right,
            IInput.Keys.UP              => Key.Up,
            IInput.Keys.E               => Key.E,
            IInput.Keys.ENTER           => Key.Enter,
            IInput.Keys.EQUALS_SIGN     => Key.Equal,
            IInput.Keys.F               => Key.F,
            IInput.Keys.G               => Key.G,
            IInput.Keys.GRAVE           => Key.GraveAccent,
            IInput.Keys.H               => Key.H,
            IInput.Keys.HOME            => Key.Home,
            IInput.Keys.I               => Key.I,
            IInput.Keys.J               => Key.J,
            IInput.Keys.K               => Key.K,
            IInput.Keys.L               => Key.L,
            IInput.Keys.LEFT_BRACKET    => Key.LeftBracket,
            IInput.Keys.M               => Key.M,
            IInput.Keys.MENU            => Key.Menu,
            IInput.Keys.MINUS           => Key.Minus,
            IInput.Keys.N               => Key.N,
            IInput.Keys.NUM             => Key.NumLock,
            IInput.Keys.O               => Key.O,
            IInput.Keys.P               => Key.P,
            IInput.Keys.PAUSE           => Key.Pause,
            IInput.Keys.PERIOD          => Key.Period,
            IInput.Keys.PRINT_SCREEN    => Key.PrintScreen,
            IInput.Keys.Q               => Key.Q,
            IInput.Keys.R               => Key.R,
            IInput.Keys.RIGHT_BRACKET   => Key.RightBracket,
            IInput.Keys.S               => Key.S,
            IInput.Keys.SCROLL_LOCK     => Key.ScrollLock,
            IInput.Keys.SEMICOLON       => Key.Semicolon,
            IInput.Keys.SHIFT_LEFT      => Key.LeftShift,
            IInput.Keys.SHIFT_RIGHT     => Key.RightShift,
            IInput.Keys.SLASH           => Key.Slash,
            IInput.Keys.SPACE           => Key.Space,
            IInput.Keys.T               => Key.T,
            IInput.Keys.TAB             => Key.Tab,
            IInput.Keys.U               => Key.U,
            IInput.Keys.V               => Key.V,
            IInput.Keys.W               => Key.W,
            IInput.Keys.X               => Key.X,
            IInput.Keys.Y               => Key.Y,
            IInput.Keys.Z               => Key.Z,
            IInput.Keys.CONTROL_LEFT    => Key.LeftControl,
            IInput.Keys.CONTROL_RIGHT   => Key.RightControl,
            IInput.Keys.ESCAPE          => Key.Escape,
            IInput.Keys.END             => Key.End,
            IInput.Keys.INSERT          => Key.Insert,
            IInput.Keys.PAGE_UP         => Key.PageUp,
            IInput.Keys.PAGE_DOWN       => Key.PageDown,
            IInput.Keys.NUMPAD_0        => Key.Kp0,
            IInput.Keys.NUMPAD_1        => Key.Kp1,
            IInput.Keys.NUMPAD_2        => Key.Kp2,
            IInput.Keys.NUMPAD_3        => Key.Kp3,
            IInput.Keys.NUMPAD_4        => Key.Kp4,
            IInput.Keys.NUMPAD_5        => Key.Kp5,
            IInput.Keys.NUMPAD_6        => Key.Kp6,
            IInput.Keys.NUMPAD_7        => Key.Kp7,
            IInput.Keys.NUMPAD_8        => Key.Kp8,
            IInput.Keys.NUMPAD_9        => Key.Kp9,
            IInput.Keys.NUMPAD_DIVIDE   => Key.KpDivide,
            IInput.Keys.NUMPAD_MULTIPLY => Key.KpMultiply,
            IInput.Keys.NUMPAD_SUBTRACT => Key.KpSubtract,
            IInput.Keys.NUMPAD_ADD      => Key.KpAdd,
            IInput.Keys.NUMPAD_DOT      => Key.KpDecimal,
            IInput.Keys.NUMPAD_ENTER    => Key.KpEnter,
            IInput.Keys.NUMPAD_EQUALS   => Key.KpEqual,
            IInput.Keys.NUM_LOCK        => Key.NumLock,
            IInput.Keys.COLON           => Key.Semicolon,
            IInput.Keys.F1              => Key.F1,
            IInput.Keys.F2              => Key.F2,
            IInput.Keys.F3              => Key.F3,
            IInput.Keys.F4              => Key.F4,
            IInput.Keys.F5              => Key.F5,
            IInput.Keys.F6              => Key.F6,
            IInput.Keys.F7              => Key.F7,
            IInput.Keys.F8              => Key.F8,
            IInput.Keys.F9              => Key.F9,
            IInput.Keys.F10             => Key.F10,
            IInput.Keys.F11             => Key.F11,
            IInput.Keys.F12             => Key.F12,
            IInput.Keys.F13             => Key.F13,
            IInput.Keys.F14             => Key.F14,
            IInput.Keys.F15             => Key.F15,
            IInput.Keys.F16             => Key.F16,
            IInput.Keys.F17             => Key.F17,
            IInput.Keys.F18             => Key.F18,
            IInput.Keys.F19             => Key.F19,
            IInput.Keys.F20             => Key.F20,
            IInput.Keys.F21             => Key.F21,
            IInput.Keys.F22             => Key.F22,
            IInput.Keys.F23             => Key.F23,
            IInput.Keys.F24             => Key.F24,

//            IInput.Keys.ANY_KEY             => Key.AnyKey,
//            IInput.Keys.AT                  => Key.At,
//            IInput.Keys.CALL                => Key.Call,
//            IInput.Keys.CAMERA              => Key.Camera,
//            IInput.Keys.CLEAR               => Key.Clear,
//            IInput.Keys.DEL                 => Key.Delete,
//            IInput.Keys.DPAD_CENTER         => Key.,
//            IInput.Keys.DPAD_DOWN           => Key.,
//            IInput.Keys.DPAD_LEFT           => Key.,
//            IInput.Keys.DPAD_RIGHT          => Key.,
//            IInput.Keys.DPAD_UP             => Key.,
//            IInput.Keys.CENTER              => Key.,
//            IInput.Keys.ENDCALL             => Key.EndCall,
//            IInput.Keys.ENVELOPE            => Key.Envelope,
//            IInput.Keys.EXPLORER            => Key.Explorer,
//            IInput.Keys.FOCUS               => Key.Focus,
//            IInput.Keys.HEADSETHOOK         => Key.HeadsetHook,
//            IInput.Keys.MEDIA_FAST_FORWARD  => Key.MediaFastForward,
//            IInput.Keys.MEDIA_NEXT          => Key.,
//            IInput.Keys.MEDIA_PLAY_PAUSE    => Key.,
//            IInput.Keys.MEDIA_PREVIOUS      => Key.,
//            IInput.Keys.MEDIA_REWIND        => Key.,
//            IInput.Keys.MEDIA_STOP          => Key.,
//            IInput.Keys.MUTE                => Key.Mute,
//            IInput.Keys.NOTIFICATION        => Key.Notification,
//            IInput.Keys.PLUS                => Key.Plus,
//            IInput.Keys.POUND               => Key.Pound,
//            IInput.Keys.POWER               => Key.Power,
//            IInput.Keys.SEARCH              => Key.Search,
//            IInput.Keys.SOFT_LEFT           => Key.LeftSoft,
//            IInput.Keys.SOFT_RIGHT          => Key.RightSoft,
//            IInput.Keys.STAR                => Key.Star,
//            IInput.Keys.SYM                 => Key.Symbol,
//            IInput.Keys.UNKNOWN             => Key.Unknown,
//            IInput.Keys.VOLUME_DOWN         => Key.VolumeDown,
//            IInput.Keys.VOLUME_UP           => Key.VolumeUp,
//            IInput.Keys.META_ALT_LEFT_ON    => Key.,
//            IInput.Keys.META_ALT_ON         => Key.,
//            IInput.Keys.META_ALT_RIGHT_ON   => Key.,
//            IInput.Keys.META_SHIFT_LEFT_ON  => Key.,
//            IInput.Keys.META_SHIFT_ON       => Key.,
//            IInput.Keys.META_SHIFT_RIGHT_ON => Key.,
//            IInput.Keys.META_SYM_ON         => Key.,
//            IInput.Keys.PICTSYMBOLS         => Key.PictSymbols,
//            IInput.Keys.SWITCH_CHARSET      => Key.SwitchCharset,
//            IInput.Keys.BUTTON_CIRCLE       => Key.ButtonCircle,
//            IInput.Keys.BUTTON_A            => Key.,
//            IInput.Keys.BUTTON_B            => Key.,
//            IInput.Keys.BUTTON_C            => Key.,
//            IInput.Keys.BUTTON_X            => Key.,
//            IInput.Keys.BUTTON_Y            => Key.,
//            IInput.Keys.BUTTON_Z            => Key.,
//            IInput.Keys.BUTTON_L1           => Key.,
//            IInput.Keys.BUTTON_R1           => Key.,
//            IInput.Keys.BUTTON_L2           => Key.,
//            IInput.Keys.BUTTON_R2           => Key.,
//            IInput.Keys.BUTTON_THUMBL       => Key.,
//            IInput.Keys.BUTTON_THUMBR       => Key.,
//            IInput.Keys.BUTTON_START        => Key.,
//            IInput.Keys.BUTTON_SELECT       => Key.,
//            IInput.Keys.BUTTON_MODE         => Key.,
//            IInput.Keys.NUMPAD_COMMA        => Key.,
//            IInput.Keys.NUMPAD_LEFT_PAREN   => Key.,
//            IInput.Keys.NUMPAD_RIGHT_PAREN  => Key.,
//            IInput.Keys.MAX_KEYCODE         => Key.,

            // ... other mappings
            var _ => throw new ArgumentException( $"Invalid key: {inputKey}" ),
        };
    }
}

// ========================================================================
// ========================================================================