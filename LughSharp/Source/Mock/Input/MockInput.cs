// ///////////////////////////////////////////////////////////////////////////////
// MIT License
// 
// Copyright (c) 2024, 2025, 2026 Circa64 Software Projects / Richard Ikin.
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

namespace LughSharp.Source.Mock.Input;

[PublicAPI]
public class MockInput : IInput
{
    public IInputProcessor? InputProcessor { get; set; }

    public int GetMaxPointers()
    {
        return 0;
    }

    public int GetX( int pointer = 0 )
    {
        return 0;
    }

    public int GetDeltaX( int pointer = 0 )
    {
        return 0;
    }

    public int GetY( int pointer = 0 )
    {
        return 0;
    }

    public int GetDeltaY( int pointer = 0 )
    {
        return 0;
    }

    /// <summary>
    /// This will set a keyboard height callback. This will get called, whenever the keyboard
    /// height changes. Note: When using <see cref="IInput.OpenTextInputField"/>, it will report the
    /// height of the native input field too.
    /// </summary>
    /// <param name="observer"> The observer to set. </param>
    public void SetKeyboardHeightObserver( IInput.IKeyboardHeightObserver observer )
    {
    }

    public int GetRotation()
    {
        return 0;
    }

    public bool IsButtonPressed( int button )
    {
        return false;
    }

    public bool IsButtonJustPressed( int button )
    {
        return false;
    }

    public bool IsKeyPressed( int key )
    {
        return false;
    }

    public bool IsKeyJustPressed( int key )
    {
        return false;
    }

    public bool IsPeripheralAvailable( IInput.Peripheral peripheral )
    {
        return false;
    }

    public void GetTextInput( IInput.ITextInputListener listener, string title, string text, string hint,
                              IInput.OnscreenKeyboardType? type )
    {
    }

    public long GetCurrentEventTime()
    {
        return 0;
    }

    /// <summary>
    /// Sets the on-screen keyboard visible if available.
    /// </summary>
    /// <param name="configuration"> The configuration for the native input field </param> 
    public void OpenTextInputField( NativeInputConfiguration configuration )
    {
    }

    /// <summary>
    /// Closes the native input field and applies the result to the input wrapper.
    /// </summary>
    /// <param name="isConfirmative">
    /// Whether the closing can be considered confirmative. Will be passed to the
    /// <see cref="NativeInputCloseCallback"/>
    /// </param>
    /// <param name="callback">
    /// An optional callback to also run, when the close was processed. Will be called
    /// on the main thread. Will be called after <see cref="NativeInputCloseCallback"/>
    /// </param>
    public void CloseTextInputField( bool isConfirmative, NativeInputCloseCallback? callback = null )
    {
    }

    /// <summary>
    /// Returns if a native input field is currently open
    /// </summary>
    public bool IsTextInputFieldOpened()
    {
        return false;
    }

    public IInput.Orientation GetNativeOrientation()
    {
        return IInput.Orientation.Landscape;
    }

    public float GetAccelerometerX()
    {
        return 0f;
    }

    public float GetAccelerometerY()
    {
        return 0f;
    }

    public float GetAccelerometerZ()
    {
        return 0f;
    }

    public float GetGyroscopeX()
    {
        return 0f;
    }

    public float GetGyroscopeY()
    {
        return 0f;
    }

    public float GetGyroscopeZ()
    {
        return 0f;
    }

    public float GetPressure( int pointer = 0 )
    {
        return 0f;
    }

    public float GetAzimuth()
    {
        return 0f;
    }

    public float GetPitch()
    {
        return 0f;
    }

    public float GetRoll()
    {
        return 0f;
    }

    public void SetOnscreenKeyboardVisible( bool visible )
    {
    }

    public void SetOnscreenKeyboardVisible( bool visible, IInput.OnscreenKeyboardType? type )
    {
    }

    public void Vibrate( int milliseconds )
    {
    }

    public void Vibrate( long[] pattern, int repeat )
    {
    }

    public void CancelVibrate()
    {
    }

    public void GetRotationMatrix( float[] matrix )
    {
    }

    public bool IsTouched( int pointer = 0 )
    {
        return false;
    }

    public bool JustTouched()
    {
        return false;
    }

    public void SetOverrideKey( int keycode, bool addKey )
    {
    }

    public void AddOverrideKey( int keycode )
    {
    }

    public void RemoveOverrideKey( int keycode )
    {
    }

    /// <summary>
    /// Checks to see if the provided keycode applies to a key that is a member of the
    /// 'Override Key' group of keys.
    /// </summary>
    /// <param name="keycode"> The <see cref="IInput.Keys"/> code. </param>
    /// <returns> True if the key is an Override Key, otherwise false. </returns>
    public bool IsOverrideKey( int keycode )
    {
        return false;
    }

    public bool IsCursorOverridden()
    {
        return false;
    }

    public void SetCursorOverridden( bool caught )
    {
    }

    public void SetCursorPosition( int x, int y )
    {
    }

    public bool IsOverrideBackKey()
    {
        return false;
    }

    public void SetOverrideBackKey( bool catchBack )
    {
    }

    public bool IsOverrideMenuKey()
    {
        return false;
    }

    public void SetOverrideMenuKey( bool catchMenu )
    {
    }
}

// ============================================================================
// ============================================================================