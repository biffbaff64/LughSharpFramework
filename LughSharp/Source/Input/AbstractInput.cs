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

using System.Collections.Generic;

using JetBrains.Annotations;

namespace LughSharp.Source.Input;

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
    protected bool[] PressedKeys { get; set; } = new bool[ IInput.Keys.MaxKeycode + 1 ];

    /// <summary>
    /// A list of keys that have JUST been pressed.
    /// </summary>
    protected bool[] JustPressedKeys { get; set; } = new bool[ IInput.Keys.MaxKeycode + 1 ];

    /// <summary>
    /// True if any key has just been pressed.
    /// </summary>
    protected bool KeyJustPressed { get; set; }

    /// <summary>
    /// The number of currently pressed keys.
    /// </summary>
    protected int PressedKeyCount { get; set; }

    /// <inheritdoc />
    public IInputProcessor? InputProcessor { get; set; }

    // ========================================================================

    /// <summary>
    /// Returns TRUE if the key identified by the supplied <see cref="IInput.Keys"/> key code is pressed.
    /// </summary>
    public virtual bool IsKeyPressed( int key )
    {
        if ( key == IInput.Keys.AnyKey )
        {
            return PressedKeyCount > 0;
        }

        return key is >= 0 and <= IInput.Keys.MaxKeycode && PressedKeys[ key ];
    }

    /// <summary>
    /// Returns TRUE if the key identified by the supplied <see cref="IInput.Keys"/>
    /// key code has <b>just</b> been pressed.
    /// </summary>
    public virtual bool IsKeyJustPressed( int key )
    {
        if ( key == IInput.Keys.AnyKey )
        {
            return KeyJustPressed;
        }

        return key is >= 0 and <= IInput.Keys.MaxKeycode && JustPressedKeys[ key ];
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
    /// Returns <b>true</b> if the list of Override Keys contains <see cref="IInput.Keys.Back"/>
    /// </summary>
    public virtual bool IsOverrideBackKey()
    {
        return _keysToOverride.Contains( IInput.Keys.Back );
    }

    /// <summary>
    /// Either <b>adds</b> or <b>removes</b> the <see cref="IInput.Keys.Back"/> key.
    /// </summary>
    /// <param name="addKey"> True to add, false to remove. </param>
    public virtual void SetOverrideBackKey( bool addKey )
    {
        SetOverrideKey( IInput.Keys.Back, addKey );
    }

    /// <summary>
    /// Returns <b>true</b> if the list of Override Keys contains <see cref="IInput.Keys.Menu"/>
    /// </summary>
    public virtual bool IsOverrideMenuKey()
    {
        return _keysToOverride.Contains( IInput.Keys.Menu );
    }

    /// <summary>
    /// Either <b>adds</b> or <b>removes</b> the <see cref="IInput.Keys.Menu"/> key.
    /// </summary>
    /// <param name="addKey"> True to add, false to remove. </param>
    public virtual void SetOverrideMenuKey( bool addKey )
    {
        SetOverrideKey( IInput.Keys.Menu, addKey );
    }

    // ========================================================================
    // Abstract methods to be implemented by any inheriting classes.
    // ========================================================================

    //@formatter:off
    public virtual float GetAccelerometerX() => 0;
    public virtual float GetAccelerometerY() => 0;
    public virtual float GetAccelerometerZ() => 0;
    public virtual float GetGyroscopeX() => 0;
    public virtual float GetGyroscopeY() => 0;
    public virtual float GetGyroscopeZ() => 0;
    public virtual float GetAzimuth() => 0;
    public virtual float GetPitch() => 0;
    public virtual float GetRoll() => 0;
    public virtual bool IsTouched( int pointer = 0 ) => false;
    public virtual bool JustTouched() => false;
    public virtual float GetPressure( int pointer = 0 ) => 0;
    public virtual int GetRotation() => 0;
    public virtual IInput.Orientation GetNativeOrientation() => 0;
    public virtual int GetMaxPointers() => 0;
    public virtual int GetX( int pointer = 0 ) => 0;
    public virtual int GetY( int pointer = 0 ) => 0;
    public virtual int GetDeltaX( int pointer = 0 ) => 0;
    public virtual int GetDeltaY( int pointer = 0 ) => 0;
    public virtual bool IsButtonPressed( int button ) => false;
    public virtual bool IsButtonJustPressed( int button ) => false;
    public virtual bool IsPeripheralAvailable( IInput.Peripheral peripheral ) => false;
    public virtual long GetCurrentEventTime() => 0;
    public virtual bool IsCursorOverridden() => false;

    public virtual void Vibrate( int milliseconds ) {}
    public virtual void Vibrate( long[] pattern, int repeat ) {}
    public virtual void CancelVibrate() {}
    public virtual void GetRotationMatrix( float[] matrix ) {}
    public virtual void SetCursorOverridden( bool caught ) {}
    public virtual void SetCursorPosition( int x, int y ) {}
    public virtual void SetOnscreenKeyboardVisible( bool visible ) {}
    public virtual void SetOnscreenKeyboardVisible( bool visible, IInput.OnscreenKeyboardType? type ) {}
    public virtual void GetTextInput( IInput.ITextInputListener listener,
                                      string title,
                                      string text,
                                      string hint,
                                      IInput.OnscreenKeyboardType? type = IInput.OnscreenKeyboardType.Default ) {}
    //@formatter:on
}

// ============================================================================
// ============================================================================