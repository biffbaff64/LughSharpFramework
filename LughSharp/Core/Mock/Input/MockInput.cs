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


using JetBrains.Annotations;

using LughSharp.Core.Input;

namespace LughSharp.Core.Mock.Input;

[PublicAPI]
public class MockInput : IInput
{
    /// <summary>
    /// The currently set <see cref="IInputProcessor"/>.
    /// </summary>
    public IInputProcessor? InputProcessor { get; set; }
    
    public int GetMaxPointers()
    {
        throw new NotImplementedException();
    }

    public int GetX( int pointer = 0 )
    {
        throw new NotImplementedException();
    }

    public int GetDeltaX( int pointer = 0 )
    {
        throw new NotImplementedException();
    }

    public int GetY( int pointer = 0 )
    {
        throw new NotImplementedException();
    }

    public int GetDeltaY( int pointer = 0 )
    {
        throw new NotImplementedException();
    }

    public int GetRotation()
    {
        throw new NotImplementedException();
    }

    public bool IsButtonPressed( int button )
    {
        throw new NotImplementedException();
    }

    public bool IsButtonJustPressed( int button )
    {
        throw new NotImplementedException();
    }

    public bool IsKeyPressed( int key )
    {
        throw new NotImplementedException();
    }

    public bool IsKeyJustPressed( int key )
    {
        throw new NotImplementedException();
    }

    public bool IsPeripheralAvailable( IInput.Peripheral peripheral )
    {
        throw new NotImplementedException();
    }

    public void GetTextInput( IInput.ITextInputListener listener, string title, string text, string hint, IInput.OnscreenKeyboardType? type )
    {
        throw new NotImplementedException();
    }

    public long GetCurrentEventTime()
    {
        throw new NotImplementedException();
    }

    public IInput.Orientation GetNativeOrientation()
    {
        throw new NotImplementedException();
    }

    public float GetAccelerometerX()
    {
        throw new NotImplementedException();
    }

    public float GetAccelerometerY()
    {
        throw new NotImplementedException();
    }

    public float GetAccelerometerZ()
    {
        throw new NotImplementedException();
    }

    public float GetGyroscopeX()
    {
        throw new NotImplementedException();
    }

    public float GetGyroscopeY()
    {
        throw new NotImplementedException();
    }

    public float GetGyroscopeZ()
    {
        throw new NotImplementedException();
    }

    public float GetPressure( int pointer = 0 )
    {
        throw new NotImplementedException();
    }

    public float GetAzimuth()
    {
        throw new NotImplementedException();
    }

    public float GetPitch()
    {
        throw new NotImplementedException();
    }

    public float GetRoll()
    {
        throw new NotImplementedException();
    }

    public void SetOnscreenKeyboardVisible( bool visible )
    {
        throw new NotImplementedException();
    }

    public void SetOnscreenKeyboardVisible( bool visible, IInput.OnscreenKeyboardType? type )
    {
        throw new NotImplementedException();
    }

    public void Vibrate( int milliseconds )
    {
        throw new NotImplementedException();
    }

    public void Vibrate( long[] pattern, int repeat )
    {
        throw new NotImplementedException();
    }

    public void CancelVibrate()
    {
        throw new NotImplementedException();
    }

    public void GetRotationMatrix( float[] matrix )
    {
        throw new NotImplementedException();
    }

    public bool IsTouched( int pointer = 0 )
    {
        throw new NotImplementedException();
    }

    public bool JustTouched()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// "Override key" refers to a mechanism for overriding the default system actions of specific
    /// keys (like BACK and MENU), allowing the application to handle them instead. It's used to
    /// prevent unwanted system behaviors and provide custom input handling for these special keys
    /// within games and applications.
    /// </summary>
    public void SetOverrideKey( int keycode, bool addKey )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks to see if the provided keycode applies to a key that is a member of the
    /// 'Override Key' group of keys.
    /// </summary>
    /// <param name="keycode"> The <see cref="IInput.Keys"/> code. </param>
    /// <returns> True if the key is an Override Key, otherwise false. </returns>
    public bool IsOverrideKey( int keycode )
    {
        throw new NotImplementedException();
    }

    public bool IsCursorOverridden()
    {
        throw new NotImplementedException();
    }

    public void SetCursorOverridden( bool caught )
    {
        throw new NotImplementedException();
    }

    public void SetCursorPosition( int x, int y )
    {
        throw new NotImplementedException();
    }

    public bool IsOverrideBackKey()
    {
        throw new NotImplementedException();
    }

    public void SetOverrideBackKey( bool catchBack )
    {
        throw new NotImplementedException();
    }

    public bool IsOverrideMenuKey()
    {
        throw new NotImplementedException();
    }

    public void SetOverrideMenuKey( bool catchMenu )
    {
        throw new NotImplementedException();
    }
}

// ============================================================================
// ============================================================================
