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

using JetBrains.Annotations;

namespace LughSharp.Source.Input;

/// <summary>
/// Interface to the input facilities. This allows polling the state of the keyboard, the
/// touch screen and the accelerometer. On some backends (desktop, gwt, etc) the touch
/// screen is replaced by mouse input. The accelerometer is of course not available on all
/// backends. Instead of polling for events, one can process all input events with an
/// InputProcessor. You can set the InputProcessor via the SetInputProcessor(InputProcessor)
/// method. It will be called before the ApplicationListener.render() method in each frame.
/// Keyboard keys are translated to the constants in Input.Keys transparently on all systems.
/// <para>
/// Do not use system specific key constants.
/// </para>
/// <para>
/// The class also offers methods to use (and test for the presence of) other input systems
/// like vibration, compass, on-screen keyboards, and cursor capture.
/// </para>
/// <para>
/// Support for simple input dialogs is also provided.
/// </para>
/// </summary>
[PublicAPI]
public interface IInput
{
    // ====================================================================
    // ====================================================================

    /// <summary>
    /// Keyboard Types
    /// </summary>
    [PublicAPI]
    public enum OnscreenKeyboardType
    {
        Default,
        NumberPad,
        PhonePad,
        Email,
        Password,
        Uri
    }

    // ====================================================================
    // ====================================================================

    /// <summary>
    /// Screen orientation types.
    /// </summary>
    [PublicAPI]
    public enum Orientation
    {
        Landscape,
        Portrait
    }

    // ====================================================================
    // ====================================================================

    /// <summary>
    /// Supported peripherals.
    /// </summary>
    [PublicAPI]
    public enum Peripheral
    {
        HardwareKeyboard,
        OnscreenKeyboard,
        MultitouchScreen,
        Accelerometer,
        Compass,
        Vibrator,
        Gyroscope,
        RotationVector,
        Pressure
    }

    /// <summary>
    /// The currently set <see cref="IInputProcessor"/>.
    /// </summary>
    IInputProcessor? InputProcessor { get; set; }

    // ========================================================================

    int GetMaxPointers();
    int GetX( int pointer = 0 );
    int GetDeltaX( int pointer = 0 );
    int GetY( int pointer = 0 );
    int GetDeltaY( int pointer = 0 );
    int GetRotation();

    bool IsButtonPressed( int button );
    bool IsButtonJustPressed( int button );
    bool IsKeyPressed( int key );
    bool IsKeyJustPressed( int key );
    bool IsPeripheralAvailable( Peripheral peripheral );

    void GetTextInput( ITextInputListener listener,
                       string title,
                       string text,
                       string hint,
                       OnscreenKeyboardType? type );

    long GetCurrentEventTime();

    Orientation GetNativeOrientation();

    // ====================================================================
    // ====================================================================

    /// <summary>
    /// Mouse Buttons
    /// </summary>
    [PublicAPI]
    public static class Buttons
    {
        public const int Left    = 0;
        public const int Right   = 1;
        public const int Middle  = 2;
        public const int Back    = 3;
        public const int Forward = 4;
    }

    // ====================================================================
    // ====================================================================

    /// <summary>
    /// Available Keys
    /// </summary>
    [PublicAPI]
    public static class Keys
    {
        public const int Num0 = 7;
        public const int Num1 = 8;
        public const int Num2 = 9;
        public const int Num3 = 10;
        public const int Num4 = 11;
        public const int Num5 = 12;
        public const int Num6 = 13;
        public const int Num7 = 14;
        public const int Num8 = 15;
        public const int Num9 = 16;

        public const int A = 29;
        public const int B = 30;
        public const int C = 31;
        public const int D = 32;
        public const int E = 33;
        public const int F = 34;
        public const int G = 35;
        public const int H = 36;
        public const int I = 37;
        public const int J = 38;
        public const int K = 39;
        public const int L = 40;
        public const int M = 41;
        public const int N = 42;
        public const int O = 43;
        public const int P = 44;
        public const int Q = 45;
        public const int R = 46;
        public const int S = 47;
        public const int T = 48;
        public const int U = 49;
        public const int V = 50;
        public const int W = 51;
        public const int X = 52;
        public const int Y = 53;
        public const int Z = 54;

        public const int AltLeft    = 57;
        public const int AltRight   = 58;
        public const int Apostrophe = 75;
        public const int At         = 77;
        public const int Back       = 4;
        public const int Backslash  = 73;

        public const int Call    = 5;
        public const int Camera  = 27;
        public const int Endcall = 6;

        public const int CapsLock = 115;
        public const int Clear    = 28;
        public const int Comma    = 55;

        public const int Del        = 67;
        public const int Backspace  = 67;
        public const int ForwardDel = 112;

        public const int DpadCenter = 23;
        public const int DpadDown   = 20;
        public const int DpadLeft   = 21;
        public const int DpadRight  = 22;
        public const int DpadUp     = 19;

        public const int Center = 23;
        public const int Down   = 20;
        public const int Left   = 21;
        public const int Right  = 22;
        public const int Up     = 19;

        public const int Enter            = 66;
        public const int Envelope         = 65;
        public const int EqualsSign       = 70;
        public const int Explorer         = 64;
        public const int Focus            = 80;
        public const int Grave            = 68;
        public const int Headsethook      = 79;
        public const int Home             = 3;
        public const int LeftBracket      = 71;
        public const int MediaFastForward = 90;
        public const int MediaNext        = 87;
        public const int MediaPlayPause   = 85;
        public const int MediaPrevious    = 88;
        public const int MediaRewind      = 89;
        public const int MediaStop        = 86;
        public const int Menu             = 82;
        public const int Minus            = 69;
        public const int Mute             = 91;
        public const int Notification     = 83;
        public const int Num              = 78;
        public const int Pause            = 121; // aka break
        public const int Period           = 56;
        public const int Plus             = 81;
        public const int Pound            = 18;
        public const int Power            = 26;
        public const int PrintScreen      = 120; // aka SYSRQ, PrtSc
        public const int RightBracket     = 72;
        public const int ScrollLock       = 116;
        public const int Search           = 84;
        public const int Semicolon        = 74;
        public const int Colon            = 243;
        public const int ShiftLeft        = 59;
        public const int ShiftRight       = 60;
        public const int Slash            = 76;
        public const int SoftLeft         = 1;
        public const int SoftRight        = 2;
        public const int Space            = 62;
        public const int Star             = 17;
        public const int Sym              = 63;
        public const int Tab              = 61;
        public const int VolumeDown       = 25;
        public const int VolumeUp         = 24;
        public const int MetaAltLeftOn    = 16;
        public const int MetaAltOn        = 2;
        public const int MetaAltRightOn   = 32;
        public const int MetaShiftLeftOn  = 64;
        public const int MetaShiftOn      = 1;
        public const int MetaShiftRightOn = 128;
        public const int MetaSymOn        = 4;
        public const int ControlLeft      = 129;
        public const int ControlRight     = 130;
        public const int Escape           = 111;
        public const int End              = 123;
        public const int Insert           = 124;
        public const int PageUp           = 92;
        public const int PageDown         = 93;
        public const int Pictsymbols      = 94;
        public const int SwitchCharset    = 95;
        public const int ButtonCircle     = 255;
        public const int ButtonA          = 96;
        public const int ButtonB          = 97;
        public const int ButtonC          = 98;
        public const int ButtonX          = 99;
        public const int ButtonY          = 100;
        public const int ButtonZ          = 101;
        public const int ButtonL1         = 102;
        public const int ButtonR1         = 103;
        public const int ButtonL2         = 104;
        public const int ButtonR2         = 105;
        public const int ButtonThumbl     = 106;
        public const int ButtonThumbr     = 107;
        public const int ButtonStart      = 108;
        public const int ButtonSelect     = 109;
        public const int ButtonMode       = 110;

        public const int Numpad0          = 144;
        public const int Numpad1          = 145;
        public const int Numpad2          = 146;
        public const int Numpad3          = 147;
        public const int Numpad4          = 148;
        public const int Numpad5          = 149;
        public const int Numpad6          = 150;
        public const int Numpad7          = 151;
        public const int Numpad8          = 152;
        public const int Numpad9          = 153;
        public const int NumpadDivide     = 154;
        public const int NumpadMultiply   = 155;
        public const int NumpadSubtract   = 156;
        public const int NumpadAdd        = 157;
        public const int NumpadDot        = 158;
        public const int NumpadComma      = 159;
        public const int NumpadEnter      = 160;
        public const int NumpadEquals     = 161;
        public const int NumpadLeftParen  = 162;
        public const int NumpadRightParen = 163;
        public const int NumLock          = 143;

        public const int F1  = 131;
        public const int F2  = 132;
        public const int F3  = 133;
        public const int F4  = 134;
        public const int F5  = 135;
        public const int F6  = 136;
        public const int F7  = 137;
        public const int F8  = 138;
        public const int F9  = 139;
        public const int F10 = 140;
        public const int F11 = 141;
        public const int F12 = 142;
        public const int F13 = 183;
        public const int F14 = 184;
        public const int F15 = 185;
        public const int F16 = 186;
        public const int F17 = 187;
        public const int F18 = 188;
        public const int F19 = 189;
        public const int F20 = 190;
        public const int F21 = 191;
        public const int F22 = 192;
        public const int F23 = 193;
        public const int F24 = 194;

        public const int AnyKey     = -1; // See, it DOES exist!.....
        public const int Unknown    = 0;
        public const int MaxKeycode = 255;

        // ====================================================================

        /// <summary>
        /// Returns the keycode as a string.
        /// </summary>
        /// <param name="keycode"> The keycode. </param>
        /// <param name="verbose"> True for a more verbose output, e.g. 'Minus' instead of '-'. </param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the <c>keycode</c> is &lt; 0 or &gt; <see cref="MaxKeycode"/>. </exception>
        public static string? NameOf( int keycode, bool verbose = false )
        {
            if ( keycode < 0 )
            {
                throw new ArgumentException( "keycode cannot be < 0 - " + keycode );
            }

            if ( keycode > MaxKeycode )
            {
                throw new ArgumentException( "keycode cannot be > MaxKeycode - " + keycode );
            }

            string? str = keycode switch
                          {
                              Unknown          => "Unknown",
                              SoftLeft         => "Soft Left",
                              SoftRight        => "Soft Right",
                              Home             => "Home",
                              Back             => "Back",
                              Call             => "Call",
                              Endcall          => "End Call",
                              Num0             => "0",
                              Num1             => "1",
                              Num2             => "2",
                              Num3             => "3",
                              Num4             => "4",
                              Num5             => "5",
                              Num6             => "6",
                              Num7             => "7",
                              Num8             => "8",
                              Num9             => "9",
                              Star             => verbose ? "Star" : "*",
                              Pound            => verbose ? "Pound" : "#",
                              Up               => "Up",
                              Down             => "Down",
                              Left             => "Left",
                              Right            => "Right",
                              Center           => "Center",
                              VolumeUp         => "Volume Up",
                              VolumeDown       => "Volume Down",
                              Power            => "Power",
                              Camera           => "Camera",
                              Clear            => "Clear",
                              A                => "A",
                              B                => "B",
                              C                => "C",
                              D                => "D",
                              E                => "E",
                              F                => "F",
                              G                => "G",
                              H                => "H",
                              I                => "I",
                              J                => "J",
                              K                => "K",
                              L                => "L",
                              M                => "M",
                              N                => "N",
                              O                => "O",
                              P                => "P",
                              Q                => "Q",
                              R                => "R",
                              S                => "S",
                              T                => "T",
                              U                => "U",
                              V                => "V",
                              W                => "W",
                              X                => "X",
                              Y                => "Y",
                              Z                => "Z",
                              Comma            => verbose ? "Comma" : ",",
                              Period           => verbose ? "Period" : ".",
                              AltLeft          => "L-Alt",
                              AltRight         => "R-Alt",
                              ShiftLeft        => "L-Shift",
                              ShiftRight       => "R-Shift",
                              Tab              => "Tab",
                              Space            => "Space",
                              Sym              => "SYM",
                              Explorer         => "Explorer",
                              Envelope         => "Envelope",
                              Enter            => "Enter",
                              Del              => "Delete", // also BACKSPACE
                              Grave            => verbose ? "Grave" : "`",
                              Minus            => verbose ? "Minus" : "-",
                              EqualsSign       => verbose ? "Equals" : "=",
                              LeftBracket      => verbose ? "Left Bracket" : "[",
                              RightBracket     => verbose ? "Right Bracket" : "]",
                              Backslash        => verbose ? "Backslash" : "\\",
                              Semicolon        => verbose ? "Semicolon" : ";",
                              Apostrophe       => verbose ? "Apostrophe" : "'",
                              Slash            => verbose ? "Slash" : "/",
                              At               => verbose ? "At" : "@",
                              Num              => "Num",
                              Headsethook      => "Headset Hook",
                              Focus            => "Focus",
                              Plus             => "Plus",
                              Menu             => "Menu",
                              Notification     => "Notification",
                              Search           => "Search",
                              MediaPlayPause   => "Play/Pause",
                              MediaStop        => "Stop Media",
                              MediaNext        => "Next Media",
                              MediaPrevious    => "Prev Media",
                              MediaRewind      => "Rewind",
                              MediaFastForward => "Fast Forward",
                              Mute             => "Mute",
                              PageUp           => "Page Up",
                              PageDown         => "Page Down",
                              Pictsymbols      => "Pictsymbols",
                              SwitchCharset    => "SwitchCharset",
                              ButtonA          => "A Button",
                              ButtonB          => "B Button",
                              ButtonC          => "C Button",
                              ButtonX          => "X Button",
                              ButtonY          => "Y Button",
                              ButtonZ          => "Z Button",
                              ButtonL1         => "L1 Button",
                              ButtonR1         => "R1 Button",
                              ButtonL2         => "L2 Button",
                              ButtonR2         => "R2 Button",
                              ButtonThumbl     => "Left Thumb",
                              ButtonThumbr     => "Right Thumb",
                              ButtonStart      => "Start",
                              ButtonSelect     => "Select",
                              ButtonMode       => "Button Mode",
                              ForwardDel       => "Forward Delete",
                              ControlLeft      => "L-Ctrl",
                              ControlRight     => "R-Ctrl",
                              Escape           => "Escape",
                              End              => "End",
                              Insert           => "Insert",
                              Numpad0          => "Numpad 0",
                              Numpad1          => "Numpad 1",
                              Numpad2          => "Numpad 2",
                              Numpad3          => "Numpad 3",
                              Numpad4          => "Numpad 4",
                              Numpad5          => "Numpad 5",
                              Numpad6          => "Numpad 6",
                              Numpad7          => "Numpad 7",
                              Numpad8          => "Numpad 8",
                              Numpad9          => "Numpad 9",
                              Colon            => verbose ? "Colon" : ":",
                              F1               => "F1",
                              F2               => "F2",
                              F3               => "F3",
                              F4               => "F4",
                              F5               => "F5",
                              F6               => "F6",
                              F7               => "F7",
                              F8               => "F8",
                              F9               => "F9",
                              F10              => "F10",
                              F11              => "F11",
                              F12              => "F12",
                              F13              => "F13",
                              F14              => "F14",
                              F15              => "F15",
                              F16              => "F16",
                              F17              => "F17",
                              F18              => "F18",
                              F19              => "F19",
                              F20              => "F20",
                              F21              => "F21",
                              F22              => "F22",
                              F23              => "F23",
                              F24              => "F24",
                              NumpadDivide     => verbose ? "Numpad Divide" : "Num /",
                              NumpadMultiply   => verbose ? "Numpad Multiply" : "Num *",
                              NumpadSubtract   => verbose ? "Numpad Subtract" : "Num -",
                              NumpadAdd        => verbose ? "Numpad Add" : "Num +",
                              NumpadDot        => verbose ? "Numpad Dot" : "Num .",
                              NumpadComma      => verbose ? "Numpad Comma" : "Num ,",
                              NumpadEnter      => verbose ? "Numpad Enter" : "Num Enter",
                              NumpadEquals     => verbose ? "Numpad Equals" : "Num =",
                              NumpadLeftParen  => verbose ? "Numpad Left Paren" : "Num (",
                              NumpadRightParen => verbose ? "Numpad Right Paren" : "Num )",
                              NumLock          => "Num Lock",
                              CapsLock         => "Caps Lock",
                              ScrollLock       => "Scroll Lock",
                              Pause            => "Pause",
                              PrintScreen      => verbose ? "Print Screen": "PrtSc",
                              var _            => null
                          };

            return str;
        }
    }

    // ====================================================================
    // ====================================================================

    /// <summary>
    /// Interface describing a listener for text input.
    /// </summary>
    [PublicAPI]
    public interface ITextInputListener
    {
        void Input( string text );
        void Canceled();
    }

    // ====================================================================
    // ====================================================================

    #region Mobile Devices

    float GetAccelerometerX();
    float GetAccelerometerY();
    float GetAccelerometerZ();
    float GetGyroscopeX();
    float GetGyroscopeY();
    float GetGyroscopeZ();
    float GetPressure( int pointer = 0 );
    float GetAzimuth();
    float GetPitch();
    float GetRoll();
    void SetOnscreenKeyboardVisible( bool visible );
    void SetOnscreenKeyboardVisible( bool visible, OnscreenKeyboardType? type );
    void Vibrate( int milliseconds );
    void Vibrate( long[] pattern, int repeat );
    void CancelVibrate();
    void GetRotationMatrix( float[] matrix );
    bool IsTouched( int pointer = 0 );
    bool JustTouched();

    #endregion Mobile Devices

    // ========================================================================

    #region override keys

    /// <summary>
    /// "Override key" refers to a mechanism for overriding the default system actions of specific
    /// keys (like BACK and MENU), allowing the application to handle them instead. It's used to
    /// prevent unwanted system behaviors and provide custom input handling for these special keys
    /// within games and applications.
    /// </summary>
    void SetOverrideKey( int keycode, bool addKey );

    /// <summary>
    /// Checks to see if the provided keycode applies to a key that is a member of the
    /// 'Override Key' group of keys.
    /// </summary>
    /// <param name="keycode"> The <see cref="Keys"/> code. </param>
    /// <returns> True if the key is an Override Key, otherwise false. </returns>
    bool IsOverrideKey( int keycode );

    bool IsCursorOverridden();
    void SetCursorOverridden( bool caught );
    void SetCursorPosition( int x, int y );

    // Mobile device Back Key
    bool IsOverrideBackKey();
    void SetOverrideBackKey( bool catchBack );

    // Mobile device Menu Key
    bool IsOverrideMenuKey();
    void SetOverrideMenuKey( bool catchMenu );

    #endregion override keys
}

// ============================================================================
// ============================================================================