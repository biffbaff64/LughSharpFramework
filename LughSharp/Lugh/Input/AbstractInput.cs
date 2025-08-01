﻿// ///////////////////////////////////////////////////////////////////////////////
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

namespace LughSharp.Lugh.Input;

/// <summary>
/// Base class for Input classes.
/// </summary>
[PublicAPI]
public abstract class AbstractInput : IInput
{
    // ========================================================================

    private readonly List< int > _keysToOverride = [ ];

    /// <summary>
    /// A List of keys that are currently pressed.
    /// </summary>
    protected bool[] PressedKeys { get; set; } = new bool[ IInput.Keys.MAX_KEYCODE + 1 ];

    /// <summary>
    /// A list of keys that have JUST been pressed.
    /// </summary>
    protected bool[] JustPressedKeys { get; set; } = new bool[ IInput.Keys.MAX_KEYCODE + 1 ];

    /// <summary>
    /// True if any key has just been pressed.
    /// </summary>
    protected bool KeyJustPressed { get; set; } = false;

    /// <summary>
    /// The number of currently pressed keys.
    /// </summary>
    protected int PressedKeyCount { get; set; } = 0;

    /// <inheritdoc />
    public IInputProcessor? InputProcessor { get; set; }

    // ========================================================================

    /// <summary>
    /// Returns TRUE if the key identified by the supplied <see cref="IInput.Keys" /> key code is pressed.
    /// </summary>
    public virtual bool IsKeyPressed( int key )
    {
        if ( key == IInput.Keys.ANY_KEY )
        {
            return PressedKeyCount > 0;
        }

        return key is >= 0 and <= IInput.Keys.MAX_KEYCODE && PressedKeys[ key ];
    }

    /// <summary>
    /// Returns TRUE if the key identified by the supplied <see cref="IInput.Keys" />
    /// key code has <b>just</b> been pressed.
    /// </summary>
    public virtual bool IsKeyJustPressed( int key )
    {
        if ( key == IInput.Keys.ANY_KEY )
        {
            return KeyJustPressed;
        }

        return key is >= 0 and <= IInput.Keys.MAX_KEYCODE && JustPressedKeys[ key ];
    }

    // ========================================================================

    /// <inheritdoc />
    public virtual void SetOverrideKey( int keycode, bool addKey )
    {
        if ( addKey )
        {
            _keysToOverride.Add( keycode );
        }
        else
        {
            _keysToOverride.Remove( keycode );
        }
    }

    /// <inheritdoc />
    public virtual bool IsOverrideKey( int keycode )
    {
        return _keysToOverride.Contains( keycode );
    }

    /// <summary>
    /// Returns <b>true</b> if the list of Override Keys contains <see cref="IInput.Keys.BACK" />
    /// </summary>
    public virtual bool IsOverrideBackKey()
    {
        return _keysToOverride.Contains( IInput.Keys.BACK );
    }

    /// <summary>
    /// Either <b>adds</b> or <b>removes</b> the <see cref="IInput.Keys.BACK" /> key.
    /// </summary>
    /// <param name="addKey"> True to add, false to remove. </param>
    public virtual void SetOverrideBackKey( bool addKey )
    {
        SetOverrideKey( IInput.Keys.BACK, addKey );
    }

    /// <summary>
    /// Returns <b>true</b> if the list of Override Keys contains <see cref="IInput.Keys.MENU" />
    /// </summary>
    public virtual bool IsOverrideMenuKey()
    {
        return _keysToOverride.Contains( IInput.Keys.MENU );
    }

    /// <summary>
    /// Either <b>adds</b> or <b>removes</b> the <see cref="IInput.Keys.MENU" /> key.
    /// </summary>
    /// <param name="addKey"> True to add, false to remove. </param>
    public virtual void SetOverrideMenuKey( bool addKey )
    {
        SetOverrideKey( IInput.Keys.MENU, addKey );
    }

    // ========================================================================
    // Abstract methods to be implemented by any inheriting classes.
    // ========================================================================

    #region abstract methods

    public abstract float GetAccelerometerX();
    public abstract float GetAccelerometerY();
    public abstract float GetAccelerometerZ();
    public abstract float GetGyroscopeX();
    public abstract float GetGyroscopeY();
    public abstract float GetGyroscopeZ();
    public abstract void Vibrate( int milliseconds );
    public abstract void Vibrate( long[] pattern, int repeat );
    public abstract void CancelVibrate();
    public abstract float GetAzimuth();
    public abstract float GetPitch();
    public abstract float GetRoll();
    public abstract void GetRotationMatrix( float[] matrix );
    public abstract long GetCurrentEventTime();
    public abstract bool IsTouched( int pointer = 0 );
    public abstract bool JustTouched();
    public abstract float GetPressure( int pointer = 0 );
    public abstract int GetRotation();
    public abstract IInput.Orientation GetNativeOrientation();
    public abstract int GetMaxPointers();
    public abstract int GetX( int pointer = 0 );
    public abstract int GetY( int pointer = 0 );
    public abstract int GetDeltaX( int pointer = 0 );
    public abstract int GetDeltaY( int pointer = 0 );
    public abstract bool IsButtonPressed( int button );
    public abstract bool IsButtonJustPressed( int button );
    public abstract bool IsPeripheralAvailable( IInput.Peripheral peripheral );
    public abstract void SetCursorOverridden( bool caught );
    public abstract bool IsCursorOverridden();
    public abstract void SetCursorPosition( int x, int y );
    public abstract void SetOnscreenKeyboardVisible( bool visible );
    public abstract void SetOnscreenKeyboardVisible( bool visible, IInput.OnscreenKeyboardType? type );

    public abstract void GetTextInput( IInput.ITextInputListener listener,
                                       string title,
                                       string text,
                                       string hint,
                                       IInput.OnscreenKeyboardType? type = IInput.OnscreenKeyboardType.Default );

    #endregion abstract methods

    // ========================================================================
}