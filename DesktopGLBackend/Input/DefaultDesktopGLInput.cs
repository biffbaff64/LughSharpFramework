// ///////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2024 Circa64 Software Projects / Richard Ikin.
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

using DesktopGLBackend.Window;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.Utils;
using LughSharp.Core.Input;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace DesktopGLBackend.Input;

[PublicAPI]
public class DefaultDesktopGLInput : AbstractInput, IDesktopGLInput
{
    private const int DefaultMaxPointers = 1;

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
    // From IDesktopGLInput
    // ========================================================================

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
        _eventQueue.ProcessInputEvents( InputProcessor );
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

        _eventQueue.ProcessInputEvents( null );
    }

    // ========================================================================
    // From Abstract Input
    // ========================================================================

    /// <inheritdoc />
    public override int GetMaxPointers()
    {
        return DefaultMaxPointers;
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
            float xScale = _window.Graphics.LogicalWidth / ( float )_window.Graphics.BackBufferWidth;
            float yScale = _window.Graphics.LogicalHeight / ( float )_window.Graphics.BackBufferHeight;

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

                   var _ => throw new RuntimeException( $"Unknown MouseButton: {button}" )
               };
    }

    protected static char CharacterForKeyCode( int key )
    {
        return key switch
               {
                   IInput.Keys.Backspace   => ( char )8,
                   IInput.Keys.Tab         => '\t',
                   IInput.Keys.ForwardDel  => ( char )127,
                   IInput.Keys.NumpadEnter => '\n',
                   IInput.Keys.Enter       => '\n',
                   var _                   => ( char )0
               };
    }

    public static int TranslateKeyCode( DotGLFW.Key glKeycode )
    {
        return glKeycode switch
               {
                   DotGLFW.Key.Space        => IInput.Keys.Space,
                   DotGLFW.Key.Apostrophe   => IInput.Keys.Apostrophe,
                   DotGLFW.Key.Comma        => IInput.Keys.Comma,
                   DotGLFW.Key.Minus        => IInput.Keys.Minus,
                   DotGLFW.Key.Period       => IInput.Keys.Period,
                   DotGLFW.Key.Slash        => IInput.Keys.Slash,
                   DotGLFW.Key.D0           => IInput.Keys.Num0,
                   DotGLFW.Key.D1           => IInput.Keys.Num1,
                   DotGLFW.Key.D2           => IInput.Keys.Num2,
                   DotGLFW.Key.D3           => IInput.Keys.Num3,
                   DotGLFW.Key.D4           => IInput.Keys.Num4,
                   DotGLFW.Key.D5           => IInput.Keys.Num5,
                   DotGLFW.Key.D6           => IInput.Keys.Num6,
                   DotGLFW.Key.D7           => IInput.Keys.Num7,
                   DotGLFW.Key.D8           => IInput.Keys.Num8,
                   DotGLFW.Key.D9           => IInput.Keys.Num9,
                   DotGLFW.Key.Semicolon    => IInput.Keys.Semicolon,
                   DotGLFW.Key.Equal        => IInput.Keys.EqualsSign,
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
                   DotGLFW.Key.LeftBracket  => IInput.Keys.LeftBracket,
                   DotGLFW.Key.Backslash    => IInput.Keys.Backslash,
                   DotGLFW.Key.RightBracket => IInput.Keys.RightBracket,
                   DotGLFW.Key.GraveAccent  => IInput.Keys.Grave,
                   DotGLFW.Key.Escape       => IInput.Keys.Escape,
                   DotGLFW.Key.Enter        => IInput.Keys.Enter,
                   DotGLFW.Key.Tab          => IInput.Keys.Tab,
                   DotGLFW.Key.Backspace    => IInput.Keys.Backspace,
                   DotGLFW.Key.Insert       => IInput.Keys.Insert,
                   DotGLFW.Key.Delete       => IInput.Keys.ForwardDel,
                   DotGLFW.Key.Right        => IInput.Keys.Right,
                   DotGLFW.Key.Left         => IInput.Keys.Left,
                   DotGLFW.Key.Down         => IInput.Keys.Down,
                   DotGLFW.Key.Up           => IInput.Keys.Up,
                   DotGLFW.Key.PageUp       => IInput.Keys.PageUp,
                   DotGLFW.Key.PageDown     => IInput.Keys.PageDown,
                   DotGLFW.Key.Home         => IInput.Keys.Home,
                   DotGLFW.Key.End          => IInput.Keys.End,
                   DotGLFW.Key.CapsLock     => IInput.Keys.CapsLock,
                   DotGLFW.Key.ScrollLock   => IInput.Keys.ScrollLock,
                   DotGLFW.Key.PrintScreen  => IInput.Keys.PrintScreen,
                   DotGLFW.Key.Pause        => IInput.Keys.Pause,
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
                   DotGLFW.Key.F25          => IInput.Keys.Unknown,
                   DotGLFW.Key.NumLock      => IInput.Keys.NumLock,
                   DotGLFW.Key.Kp0          => IInput.Keys.Numpad0,
                   DotGLFW.Key.Kp1          => IInput.Keys.Numpad1,
                   DotGLFW.Key.Kp2          => IInput.Keys.Numpad2,
                   DotGLFW.Key.Kp3          => IInput.Keys.Numpad3,
                   DotGLFW.Key.Kp4          => IInput.Keys.Numpad4,
                   DotGLFW.Key.Kp5          => IInput.Keys.Numpad5,
                   DotGLFW.Key.Kp6          => IInput.Keys.Numpad6,
                   DotGLFW.Key.Kp7          => IInput.Keys.Numpad7,
                   DotGLFW.Key.Kp8          => IInput.Keys.Numpad8,
                   DotGLFW.Key.Kp9          => IInput.Keys.Numpad9,
                   DotGLFW.Key.KpDecimal    => IInput.Keys.NumpadDot,
                   DotGLFW.Key.KpDivide     => IInput.Keys.NumpadDivide,
                   DotGLFW.Key.KpMultiply   => IInput.Keys.NumpadMultiply,
                   DotGLFW.Key.KpSubtract   => IInput.Keys.NumpadSubtract,
                   DotGLFW.Key.KpAdd        => IInput.Keys.NumpadAdd,
                   DotGLFW.Key.KpEnter      => IInput.Keys.NumpadEnter,
                   DotGLFW.Key.KpEqual      => IInput.Keys.NumpadEquals,
                   DotGLFW.Key.LeftShift    => IInput.Keys.ShiftLeft,
                   DotGLFW.Key.LeftControl  => IInput.Keys.ControlLeft,
                   DotGLFW.Key.LeftAlt      => IInput.Keys.AltLeft,
                   DotGLFW.Key.LeftSuper    => IInput.Keys.Sym,
                   DotGLFW.Key.RightShift   => IInput.Keys.ShiftRight,
                   DotGLFW.Key.RightControl => IInput.Keys.ControlRight,
                   DotGLFW.Key.RightAlt     => IInput.Keys.AltRight,
                   DotGLFW.Key.RightSuper   => IInput.Keys.Sym,
                   DotGLFW.Key.Menu         => IInput.Keys.Menu,
                   var _                    => IInput.Keys.Unknown
               };
    }

    // ========================================================================

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose( true );
        GC.SuppressFinalize( this );
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
                gdxKey = TranslateKeyCode( key );

                _eventQueue.OnKeyDown( gdxKey, TimeUtils.NanoTime() );

                PressedKeyCount++;
                KeyJustPressed            = true;
                PressedKeys[ gdxKey ]     = true;
                JustPressedKeys[ gdxKey ] = true;

                _window.Graphics.RequestRendering();
                _lastCharacter = ( char )0;

                char character = CharacterForKeyCode( gdxKey );

                if ( character != 0 )
                {
                    CharCallback( window, character );
                }

                break;
            }

            case DotGLFW.InputState.Release:
            {
                gdxKey = TranslateKeyCode( key );

                PressedKeyCount--;
                PressedKeys[ gdxKey ] = false;

                _window.Graphics.RequestRendering();

                _eventQueue.OnKeyUp( gdxKey, TimeUtils.NanoTime() );

                break;
            }

            case DotGLFW.InputState.Repeat:
            {
                if ( _lastCharacter != 0 )
                {
                    _window.Graphics.RequestRendering();

                    _eventQueue.OnKeyTyped( _lastCharacter, TimeUtils.NanoTime() );
                }

                break;
            }

            default:
            {
                break;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    /// <param name="codepoint"></param>
    public void CharCallback( DotGLFW.Window window, uint codepoint )
    {
        if ( ( codepoint & 0xff00 ) == 0xf700 )
        {
            return;
        }

        _lastCharacter = ( char )codepoint;
        _window.Graphics.RequestRendering();
        _eventQueue.OnKeyTyped( ( char )codepoint, TimeUtils.NanoTime() );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    /// <param name="button"></param>
    /// <param name="state"></param>
    /// <param name="mods"></param>
    public void MouseCallback( DotGLFW.Window window, DotGLFW.MouseButton button, DotGLFW.InputState state,
                               DotGLFW.ModifierKey mods )
    {
        int gdxButton = button switch
                        {
                            DotGLFW.MouseButton.ButtonLeft   => IInput.Buttons.Left,
                            DotGLFW.MouseButton.ButtonRight  => IInput.Buttons.Right,
                            DotGLFW.MouseButton.ButtonMiddle => IInput.Buttons.Middle,
                            DotGLFW.MouseButton.Button4      => IInput.Buttons.Back,
                            DotGLFW.MouseButton.Button5      => IInput.Buttons.Forward,

                            // ----------------------------------

                            var _ => -1
                        };

        if ( Enum.IsDefined( typeof( DotGLFW.MouseButton ), button ) && ( gdxButton == -1 ) )
        {
            return;
        }

        long time = TimeUtils.NanoTime();

        if ( state == DotGLFW.InputState.Press )
        {
            _mousePressed++;
            _justTouched                     = true;
            _justPressedButtons[ gdxButton ] = true;

            _window.Graphics.RequestRendering();
            _eventQueue.OnTouchDown( _mouseX, _mouseY, 0, gdxButton, time );
        }
        else
        {
            _mousePressed = Math.Max( 0, _mousePressed - 1 );

            _window.Graphics.RequestRendering();
            _eventQueue.OnTouchUp( _mouseX, _mouseY, 0, gdxButton, time );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void ScrollCallback( DotGLFW.Window window, double x, double y )
    {
        _window.Graphics.RequestRendering();
        _eventQueue.OnScrolled( -( float )x, -( float )y, TimeUtils.NanoTime() );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="window"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void CursorPosCallback( DotGLFW.Window window, double x, double y )
    {
        _deltaX = ( int )x - _logicalMouseX;
        _deltaY = ( int )y - _logicalMouseY;
        _mouseX = _logicalMouseX = ( int )x;
        _mouseY = _logicalMouseY = ( int )y;

        if ( _window.AppConfig.HdpiMode == HdpiMode.Pixels )
        {
            // null check can be surpressed here because of above
            float xScale = _window.Graphics.BackBufferWidth / ( float )_window.Graphics.LogicalWidth;
            float yScale = _window.Graphics.BackBufferHeight / ( float )_window.Graphics.LogicalHeight;

            _deltaX = ( int )( _deltaX * xScale );
            _deltaY = ( int )( _deltaY * yScale );
            _mouseX = ( int )( _mouseX * xScale );
            _mouseY = ( int )( _mouseY * yScale );
        }

        _window.Graphics.RequestRendering();

        if ( _mousePressed > 0 )
        {
            _eventQueue.OnTouchDragged( _mouseX, _mouseY, 0, TimeUtils.NanoTime() );
        }
        else
        {
            _eventQueue.OnMouseMoved( _mouseX, _mouseY, TimeUtils.NanoTime() );
        }
    }
}

// ============================================================================
// ============================================================================