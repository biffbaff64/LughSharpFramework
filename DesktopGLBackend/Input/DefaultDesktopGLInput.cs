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

using LughSharp.Core.Graphics.Utils;
using LughSharp.Core.Input;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;

namespace DesktopGLBackend.Input;

[PublicAPI]
public class DefaultDesktopGLInput : AbstractInput, IDesktopGLInput
{
    private const int DEFAULT_MAX_POINTERS = 1;

    private readonly InputEventQueue _eventQueue         = new();
    private readonly bool[]          _justPressedButtons = new bool[ 5 ];
    private readonly DesktopGLWindow _window;
    private          int             _deltaX;
    private          int             _deltaY;

    private bool _justTouched;
    private char _lastCharacter;
    private int  _logicalMouseX;
    private int  _logicalMouseY;
    private int  _mousePressed;
    private int  _mouseX;
    private int  _mouseY;

    // ========================================================================

    /// <inheritdoc />
    public DefaultDesktopGLInput( DesktopGLWindow window )
    {
        Guard.Against.Null( window );

        _window = window;

        WindowHandleChanged( _window.GlfwWindow );
    }

    // ========================================================================

    #region From IDesktopGLInput

    /// <inheritdoc />
    public void WindowHandleChanged( DotGLFW.Window? windowHandle )
    {
        ResetPollingStates();

        DotGLFW.Glfw.SetKeyCallback( windowHandle, KeyCallback );
        DotGLFW.Glfw.SetCharCallback( windowHandle, CharCallback );
        DotGLFW.Glfw.SetMouseButtonCallback( windowHandle, MouseCallback );
        DotGLFW.Glfw.SetScrollCallback( windowHandle, ScrollCallback );
        DotGLFW.Glfw.SetCursorPosCallback( windowHandle, CursorPosCallback );
    }

    /// <inheritdoc />
    public void Update()
    {
        _eventQueue.Drain( InputProcessor );
    }

    /// <inheritdoc />
    public void PrepareNext()
    {
        if ( _justTouched )
        {
            _justTouched = false;

            Array.Fill( _justPressedButtons, false );
        }

        if ( KeyJustPressed )
        {
            KeyJustPressed = false;

            Array.Fill( JustPressedKeys, false );
        }

        _deltaX = 0;
        _deltaY = 0;
    }

    /// <inheritdoc />
    public void ResetPollingStates()
    {
        _justTouched   = false;
        KeyJustPressed = false;

        Array.Fill( JustPressedKeys, false );
        Array.Fill( _justPressedButtons, false );

        _eventQueue.Drain( null );
    }

    #endregion From IDesktopGLInput

    // ========================================================================

    #region From Abstract Input

    /// <inheritdoc />
    public override int GetMaxPointers()
    {
        return DEFAULT_MAX_POINTERS;
    }

    /// <inheritdoc />
    public override int GetX( int pointer = 0 )
    {
        return pointer == 0 ? _mouseX : 0;
    }

    /// <inheritdoc />
    public override int GetDeltaX( int pointer = 0 )
    {
        return pointer == 0 ? _deltaX : 0;
    }

    /// <inheritdoc />
    public override int GetY( int pointer = 0 )
    {
        return pointer == 0 ? _mouseY : 0;
    }

    /// <inheritdoc />
    public override int GetDeltaY( int pointer = 0 )
    {
        return pointer == 0 ? _deltaY : 0;
    }

    /// <inheritdoc />
    public override bool IsTouched( int pointer = 0 )
    {
        if ( pointer == 0 )
        {
            Guard.Against.Null( _window );

            return ( DotGLFW.Glfw.GetMouseButton( _window.GlfwWindow, DotGLFW.MouseButton.Button1 ) ==
                     DotGLFW.InputState.Press )
                || ( DotGLFW.Glfw.GetMouseButton( _window.GlfwWindow, DotGLFW.MouseButton.Button1 ) ==
                     DotGLFW.InputState.Press )
                || ( DotGLFW.Glfw.GetMouseButton( _window.GlfwWindow, DotGLFW.MouseButton.Button1 ) ==
                     DotGLFW.InputState.Press )
                || ( DotGLFW.Glfw.GetMouseButton( _window.GlfwWindow, DotGLFW.MouseButton.Button1 ) ==
                     DotGLFW.InputState.Press )
                || ( DotGLFW.Glfw.GetMouseButton( _window.GlfwWindow, DotGLFW.MouseButton.Button1 ) ==
                     DotGLFW.InputState.Press );
        }

        return false;
    }

    /// <inheritdoc />
    public override bool JustTouched()
    {
        return _justTouched;
    }

    /// <inheritdoc />
    public override float GetPressure( int pointer = 0 )
    {
        return IsTouched( pointer ) ? 1 : 0;
    }

    /// <inheritdoc />
    public override bool IsButtonPressed( int button )
    {
        return DotGLFW.Glfw.GetMouseButton( _window.GlfwWindow, TranslateToMouseButton( button ) )
            == DotGLFW.InputState.Press;
    }

    /// <inheritdoc />
    public override bool IsButtonJustPressed( int button )
    {
        if ( ( button < 0 ) || ( button >= _justPressedButtons.Length ) )
        {
            return false;
        }

        return _justPressedButtons[ button ];
    }

    /// <inheritdoc />
    public override void GetTextInput( IInput.ITextInputListener listener,
                                       string title,
                                       string text,
                                       string hint,
                                       IInput.OnscreenKeyboardType? type = IInput.OnscreenKeyboardType.Default )
    {
        //FIXME: TextInput does nothing ( this fixme from Java/LibGdx )
        listener.Canceled();
    }

    /// <inheritdoc />
    public override long GetCurrentEventTime()
    {
        // queue sets its event time for each event dequeued/processed
        return _eventQueue.CurrentEventTime;
    }

    /// <inheritdoc />
    public override void SetCursorOverridden( bool caught )
    {
        DotGLFW.Glfw.SetInputMode( _window.GlfwWindow,
                                   DotGLFW.InputMode.Cursor,
                                   caught ? DotGLFW.CursorMode.Disabled : DotGLFW.CursorMode.Normal );
    }

    /// <inheritdoc />
    public override bool IsCursorOverridden()
    {
        return DotGLFW.Glfw.GetInputMode( _window.GlfwWindow, DotGLFW.InputMode.Cursor ) == DotGLFW.CursorMode.Disabled;
    }

    /// <inheritdoc />
    public override void SetCursorPosition( int x, int y )
    {
        if ( _window.AppConfig.HdpiMode == HdpiMode.Pixels )
        {
            var xScale = _window.Graphics.LogicalWidth / ( float )_window.Graphics.BackBufferWidth;
            var yScale = _window.Graphics.LogicalHeight / ( float )_window.Graphics.BackBufferHeight;

            x = ( int )( x * xScale );
            y = ( int )( y * yScale );
        }

        DotGLFW.Glfw.SetCursorPos( _window.GlfwWindow, x, y );
    }

    public override bool IsPeripheralAvailable( IInput.Peripheral peripheral )
    {
        return peripheral == IInput.Peripheral.HardwareKeyboard;
    }

    public override IInput.Orientation GetNativeOrientation()
    {
        return IInput.Orientation.Landscape;
    }

    private static DotGLFW.MouseButton TranslateToMouseButton( int button )
    {
        return button switch
               {
                   0 => DotGLFW.MouseButton.Button1,
                   1 => DotGLFW.MouseButton.Button2,
                   2 => DotGLFW.MouseButton.Button3,
                   3 => DotGLFW.MouseButton.Button4,
                   4 => DotGLFW.MouseButton.Button5,
                   5 => DotGLFW.MouseButton.Button6,
                   6 => DotGLFW.MouseButton.Button7,
                   7 => DotGLFW.MouseButton.Button8,

                   // ----------------------------------

                   var _ => throw new GdxRuntimeException( $"Unknown MouseButton: {button}" ),
               };
    }

    protected static char CharacterForKeyCode( int key )
    {
        return key switch
               {
                   IInput.Keys.BACKSPACE    => ( char )8,
                   IInput.Keys.TAB          => '\t',
                   IInput.Keys.FORWARD_DEL  => ( char )127,
                   IInput.Keys.NUMPAD_ENTER => '\n',
                   IInput.Keys.ENTER        => '\n',
                   var _                    => ( char )0,
               };
    }

    public static int GetGdxKeycode( DotGLFW.Key glKeycode )
    {
        return glKeycode switch
               {
                   DotGLFW.Key.Space        => IInput.Keys.SPACE,
                   DotGLFW.Key.Apostrophe   => IInput.Keys.APOSTROPHE,
                   DotGLFW.Key.Comma        => IInput.Keys.COMMA,
                   DotGLFW.Key.Minus        => IInput.Keys.MINUS,
                   DotGLFW.Key.Period       => IInput.Keys.PERIOD,
                   DotGLFW.Key.Slash        => IInput.Keys.SLASH,
                   DotGLFW.Key.D0           => IInput.Keys.NUM_0,
                   DotGLFW.Key.D1           => IInput.Keys.NUM_1,
                   DotGLFW.Key.D2           => IInput.Keys.NUM_2,
                   DotGLFW.Key.D3           => IInput.Keys.NUM_3,
                   DotGLFW.Key.D4           => IInput.Keys.NUM_4,
                   DotGLFW.Key.D5           => IInput.Keys.NUM_5,
                   DotGLFW.Key.D6           => IInput.Keys.NUM_6,
                   DotGLFW.Key.D7           => IInput.Keys.NUM_7,
                   DotGLFW.Key.D8           => IInput.Keys.NUM_8,
                   DotGLFW.Key.D9           => IInput.Keys.NUM_9,
                   DotGLFW.Key.Semicolon    => IInput.Keys.SEMICOLON,
                   DotGLFW.Key.Equal        => IInput.Keys.EQUALS_SIGN,
                   DotGLFW.Key.A            => IInput.Keys.A,
                   DotGLFW.Key.B            => IInput.Keys.B,
                   DotGLFW.Key.C            => IInput.Keys.C,
                   DotGLFW.Key.D            => IInput.Keys.D,
                   DotGLFW.Key.E            => IInput.Keys.E,
                   DotGLFW.Key.F            => IInput.Keys.F,
                   DotGLFW.Key.G            => IInput.Keys.G,
                   DotGLFW.Key.H            => IInput.Keys.H,
                   DotGLFW.Key.I            => IInput.Keys.I,
                   DotGLFW.Key.J            => IInput.Keys.J,
                   DotGLFW.Key.K            => IInput.Keys.K,
                   DotGLFW.Key.L            => IInput.Keys.L,
                   DotGLFW.Key.M            => IInput.Keys.M,
                   DotGLFW.Key.N            => IInput.Keys.N,
                   DotGLFW.Key.O            => IInput.Keys.O,
                   DotGLFW.Key.P            => IInput.Keys.P,
                   DotGLFW.Key.Q            => IInput.Keys.Q,
                   DotGLFW.Key.R            => IInput.Keys.R,
                   DotGLFW.Key.S            => IInput.Keys.S,
                   DotGLFW.Key.T            => IInput.Keys.T,
                   DotGLFW.Key.U            => IInput.Keys.U,
                   DotGLFW.Key.V            => IInput.Keys.V,
                   DotGLFW.Key.W            => IInput.Keys.W,
                   DotGLFW.Key.X            => IInput.Keys.X,
                   DotGLFW.Key.Y            => IInput.Keys.Y,
                   DotGLFW.Key.Z            => IInput.Keys.Z,
                   DotGLFW.Key.LeftBracket  => IInput.Keys.LEFT_BRACKET,
                   DotGLFW.Key.Backslash    => IInput.Keys.BACKSLASH,
                   DotGLFW.Key.RightBracket => IInput.Keys.RIGHT_BRACKET,
                   DotGLFW.Key.GraveAccent  => IInput.Keys.GRAVE,

//            DotGLFW.Key.Unknown      => IInput.Keys.UNKNOWN,
                   DotGLFW.Key.Escape       => IInput.Keys.ESCAPE,
                   DotGLFW.Key.Enter        => IInput.Keys.ENTER,
                   DotGLFW.Key.Tab          => IInput.Keys.TAB,
                   DotGLFW.Key.Backspace    => IInput.Keys.BACKSPACE,
                   DotGLFW.Key.Insert       => IInput.Keys.INSERT,
                   DotGLFW.Key.Delete       => IInput.Keys.FORWARD_DEL,
                   DotGLFW.Key.Right        => IInput.Keys.RIGHT,
                   DotGLFW.Key.Left         => IInput.Keys.LEFT,
                   DotGLFW.Key.Down         => IInput.Keys.DOWN,
                   DotGLFW.Key.Up           => IInput.Keys.UP,
                   DotGLFW.Key.PageUp       => IInput.Keys.PAGE_UP,
                   DotGLFW.Key.PageDown     => IInput.Keys.PAGE_DOWN,
                   DotGLFW.Key.Home         => IInput.Keys.HOME,
                   DotGLFW.Key.End          => IInput.Keys.END,
                   DotGLFW.Key.CapsLock     => IInput.Keys.CAPS_LOCK,
                   DotGLFW.Key.ScrollLock   => IInput.Keys.SCROLL_LOCK,
                   DotGLFW.Key.PrintScreen  => IInput.Keys.PRINT_SCREEN,
                   DotGLFW.Key.Pause        => IInput.Keys.PAUSE,
                   DotGLFW.Key.F1           => IInput.Keys.F1,
                   DotGLFW.Key.F2           => IInput.Keys.F2,
                   DotGLFW.Key.F3           => IInput.Keys.F3,
                   DotGLFW.Key.F4           => IInput.Keys.F4,
                   DotGLFW.Key.F5           => IInput.Keys.F5,
                   DotGLFW.Key.F6           => IInput.Keys.F6,
                   DotGLFW.Key.F7           => IInput.Keys.F7,
                   DotGLFW.Key.F8           => IInput.Keys.F8,
                   DotGLFW.Key.F9           => IInput.Keys.F9,
                   DotGLFW.Key.F10          => IInput.Keys.F10,
                   DotGLFW.Key.F11          => IInput.Keys.F11,
                   DotGLFW.Key.F12          => IInput.Keys.F12,
                   DotGLFW.Key.F13          => IInput.Keys.F13,
                   DotGLFW.Key.F14          => IInput.Keys.F14,
                   DotGLFW.Key.F15          => IInput.Keys.F15,
                   DotGLFW.Key.F16          => IInput.Keys.F16,
                   DotGLFW.Key.F17          => IInput.Keys.F17,
                   DotGLFW.Key.F18          => IInput.Keys.F18,
                   DotGLFW.Key.F19          => IInput.Keys.F19,
                   DotGLFW.Key.F20          => IInput.Keys.F20,
                   DotGLFW.Key.F21          => IInput.Keys.F21,
                   DotGLFW.Key.F22          => IInput.Keys.F22,
                   DotGLFW.Key.F23          => IInput.Keys.F23,
                   DotGLFW.Key.F24          => IInput.Keys.F24,
                   DotGLFW.Key.F25          => IInput.Keys.UNKNOWN,
                   DotGLFW.Key.NumLock      => IInput.Keys.NUM_LOCK,
                   DotGLFW.Key.Kp0          => IInput.Keys.NUMPAD_0,
                   DotGLFW.Key.Kp1          => IInput.Keys.NUMPAD_1,
                   DotGLFW.Key.Kp2          => IInput.Keys.NUMPAD_2,
                   DotGLFW.Key.Kp3          => IInput.Keys.NUMPAD_3,
                   DotGLFW.Key.Kp4          => IInput.Keys.NUMPAD_4,
                   DotGLFW.Key.Kp5          => IInput.Keys.NUMPAD_5,
                   DotGLFW.Key.Kp6          => IInput.Keys.NUMPAD_6,
                   DotGLFW.Key.Kp7          => IInput.Keys.NUMPAD_7,
                   DotGLFW.Key.Kp8          => IInput.Keys.NUMPAD_8,
                   DotGLFW.Key.Kp9          => IInput.Keys.NUMPAD_9,
                   DotGLFW.Key.KpDecimal    => IInput.Keys.NUMPAD_DOT,
                   DotGLFW.Key.KpDivide     => IInput.Keys.NUMPAD_DIVIDE,
                   DotGLFW.Key.KpMultiply   => IInput.Keys.NUMPAD_MULTIPLY,
                   DotGLFW.Key.KpSubtract   => IInput.Keys.NUMPAD_SUBTRACT,
                   DotGLFW.Key.KpAdd        => IInput.Keys.NUMPAD_ADD,
                   DotGLFW.Key.KpEnter      => IInput.Keys.NUMPAD_ENTER,
                   DotGLFW.Key.KpEqual      => IInput.Keys.NUMPAD_EQUALS,
                   DotGLFW.Key.LeftShift    => IInput.Keys.SHIFT_LEFT,
                   DotGLFW.Key.LeftControl  => IInput.Keys.CONTROL_LEFT,
                   DotGLFW.Key.LeftAlt      => IInput.Keys.ALT_LEFT,
                   DotGLFW.Key.LeftSuper    => IInput.Keys.SYM,
                   DotGLFW.Key.RightShift   => IInput.Keys.SHIFT_RIGHT,
                   DotGLFW.Key.RightControl => IInput.Keys.CONTROL_RIGHT,
                   DotGLFW.Key.RightAlt     => IInput.Keys.ALT_RIGHT,
                   DotGLFW.Key.RightSuper   => IInput.Keys.SYM,
                   DotGLFW.Key.Menu         => IInput.Keys.MENU,
                   var _                    => IInput.Keys.UNKNOWN,
               };
    }

    // ========================================================================

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
    }

    protected void Dispose( bool disposing )
    {
        if ( disposing )
        {
        }
    }

    // ========================================================================
    // Callbacks
    // ========================================================================

    public void KeyCallback( DotGLFW.Window window, DotGLFW.Key key, int scancode, DotGLFW.InputState action,
                             DotGLFW.ModifierKey mods )
    {
        int gdxKey;

        switch ( action )
        {
            case DotGLFW.InputState.Press:
            {
                gdxKey = GetGdxKeycode( key );

                _eventQueue.KeyDown( gdxKey, TimeUtils.NanoTime() );

                PressedKeyCount++;
                KeyJustPressed            = true;
                PressedKeys[ gdxKey ]     = true;
                JustPressedKeys[ gdxKey ] = true;

                _window.Graphics.RequestRendering();
                _lastCharacter = ( char )0;

                var character = CharacterForKeyCode( gdxKey );

                if ( character != 0 )
                {
                    CharCallback( window, character );
                }

                break;
            }

            case DotGLFW.InputState.Release:
            {
                gdxKey = GetGdxKeycode( key );

                PressedKeyCount--;
                PressedKeys[ gdxKey ] = false;

                _window.Graphics.RequestRendering();

                _eventQueue.KeyUp( gdxKey, TimeUtils.NanoTime() );

                break;
            }

            case DotGLFW.InputState.Repeat:
            {
                if ( _lastCharacter != 0 )
                {
                    _window.Graphics.RequestRendering();

                    _eventQueue.KeyTyped( _lastCharacter, TimeUtils.NanoTime() );
                }

                break;
            }

            default:
            {
                break;
            }
        }
    }

    public void CharCallback( DotGLFW.Window window, uint codepoint )
    {
        if ( ( codepoint & 0xff00 ) == 0xf700 )
        {
            return;
        }

        _lastCharacter = ( char )codepoint;
        _window.Graphics.RequestRendering();
        _eventQueue.KeyTyped( ( char )codepoint, TimeUtils.NanoTime() );
    }

    public void MouseCallback( DotGLFW.Window window, DotGLFW.MouseButton button, DotGLFW.InputState state,
                               DotGLFW.ModifierKey mods )
    {
        var gdxButton = button switch
                        {
                            DotGLFW.MouseButton.ButtonLeft   => IInput.Buttons.LEFT,
                            DotGLFW.MouseButton.ButtonRight  => IInput.Buttons.RIGHT,
                            DotGLFW.MouseButton.ButtonMiddle => IInput.Buttons.MIDDLE,
                            DotGLFW.MouseButton.Button4      => IInput.Buttons.BACK,
                            DotGLFW.MouseButton.Button5      => IInput.Buttons.FORWARD,

                            // ----------------------------------

                            var _ => -1,
                        };

        if ( Enum.IsDefined( typeof( DotGLFW.MouseButton ), button ) && ( gdxButton == -1 ) )
        {
            return;
        }

        var time = TimeUtils.NanoTime();

        if ( state == DotGLFW.InputState.Press )
        {
            _mousePressed++;
            _justTouched                     = true;
            _justPressedButtons[ gdxButton ] = true;

            _window.Graphics.RequestRendering();
            _eventQueue.TouchDown( _mouseX, _mouseY, 0, gdxButton, time );
        }
        else
        {
            _mousePressed = Math.Max( 0, _mousePressed - 1 );

            _window.Graphics.RequestRendering();
            _eventQueue.TouchUp( _mouseX, _mouseY, 0, gdxButton, time );
        }
    }

    public void ScrollCallback( DotGLFW.Window window, double x, double y )
    {
        _window.Graphics.RequestRendering();
        _eventQueue.Scrolled( -( float )x, -( float )y, TimeUtils.NanoTime() );
    }

    public void CursorPosCallback( DotGLFW.Window window, double x, double y )
    {
        _deltaX = ( int )x - _logicalMouseX;
        _deltaY = ( int )y - _logicalMouseY;
        _mouseX = _logicalMouseX = ( int )x;
        _mouseY = _logicalMouseY = ( int )y;

        if ( _window.AppConfig.HdpiMode == HdpiMode.Pixels )
        {
            // null check can be surpressed here because of above
            var xScale = _window.Graphics.BackBufferWidth / ( float )_window.Graphics.LogicalWidth;
            var yScale = _window.Graphics.BackBufferHeight / ( float )_window.Graphics.LogicalHeight;

            _deltaX = ( int )( _deltaX * xScale );
            _deltaY = ( int )( _deltaY * yScale );
            _mouseX = ( int )( _mouseX * xScale );
            _mouseY = ( int )( _mouseY * yScale );
        }

        _window.Graphics.RequestRendering();

        if ( _mousePressed > 0 )
        {
            _eventQueue.TouchDragged( _mouseX, _mouseY, 0, TimeUtils.NanoTime() );
        }
        else
        {
            _eventQueue.MouseMoved( _mouseX, _mouseY, TimeUtils.NanoTime() );
        }
    }

    // ========================================================================
    // Stubs
    // ========================================================================

    public override float GetAccelerometerX()
    {
        return 0;
    }

    public override float GetAccelerometerY()
    {
        return 0;
    }

    public override float GetAccelerometerZ()
    {
        return 0;
    }

    public override int GetRotation()
    {
        return 0;
    }

    public override float GetAzimuth()
    {
        return 0;
    }

    public override float GetPitch()
    {
        return 0;
    }

    public override float GetRoll()
    {
        return 0;
    }

    public override float GetGyroscopeX()
    {
        return 0;
    }

    public override float GetGyroscopeY()
    {
        return 0;
    }

    public override float GetGyroscopeZ()
    {
        return 0;
    }

    public override void SetOnscreenKeyboardVisible( bool visible )
    {
    }

    public override void SetOnscreenKeyboardVisible( bool visible, IInput.OnscreenKeyboardType? type )
    {
    }

    public override void Vibrate( int milliseconds )
    {
    }

    public override void Vibrate( long[] pattern, int repeat )
    {
    }

    public override void CancelVibrate()
    {
    }

    public override void GetRotationMatrix( float[] matrix )
    {
    }

    #endregion From Abstract Input
}

// ============================================================================
// ============================================================================