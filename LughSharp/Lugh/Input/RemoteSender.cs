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

using System.Diagnostics;

using Exception = System.Exception;

namespace LughSharp.Lugh.Input;

/// <summary>
/// Sends all inputs from touch, key, accelerometer and compass to
/// a <see cref="RemoteInput" /> at the given ip/port. Instantiate
/// this and call SendUpdate() periodically.
/// </summary>
public class RemoteSender : IInputProcessor
{
    public const int KEY_DOWN  = 0;
    public const int KEY_UP    = 1;
    public const int KEY_TYPED = 2;

    public const int TOUCH_DOWN    = 3;
    public const int TOUCH_UP      = 4;
    public const int TOUCH_DRAGGED = 5;

    public const int  ACCEL      = 6;
    public const int  COMPASS    = 7;
    public const int  SIZE       = 8;
    public const int  GYRO       = 9;
    private      bool _connected = false;

    private BinaryWriter? _out;

    public RemoteSender( string ip, int port )
    {
//        try
//        {
//            Socket socket = new Socket( ip, port );

//            _out = new DataOutputStream( socket.GetOutputStream() );
//            _out.WriteBoolean( GdxApi.Input.IsPeripheralAvailable( IInput.Peripheral.MultitouchScreen ) );

//            _out = new BinaryWriter()
//            _connected = true;

//            GdxApi.Input.SetInputProcessor( this );
//        }
//        catch ( System.Exception )
//        {
//            GdxApi.App.Log( "RemoteSender", "couldn't connect to " + ip + ":" + port );
//        }
    }

    public virtual bool Connected
    {
        get
        {
            lock ( this )
            {
                return _connected;
            }
        }
    }

    public bool KeyDown( int keycode )
    {
        lock ( this )
        {
            if ( !_connected )
            {
                return false;
            }
        }

        Debug.Assert( _out != null, nameof( _out ) + " is null" );

        try
        {
            _out.Write( KEY_DOWN );
            _out.Write( keycode );
        }
        catch ( Exception )
        {
            lock ( this )
            {
                _connected = false;
            }
        }

        return false;
    }

    public bool KeyUp( int keycode )
    {
        lock ( this )
        {
            if ( !_connected )
            {
                return false;
            }
        }

        try
        {
            _out?.Write( KEY_UP );
            _out?.Write( keycode );
        }
        catch ( Exception )
        {
            lock ( this )
            {
                _connected = false;
            }
        }

        return false;
    }

    public virtual bool KeyTyped( char character )
    {
        lock ( this )
        {
            if ( !_connected )
            {
                return false;
            }
        }

        try
        {
            _out?.Write( KEY_TYPED );
            _out?.Write( character );
        }
        catch ( Exception )
        {
            lock ( this )
            {
                _connected = false;
            }
        }

        return false;
    }

    public bool TouchDown( int x, int y, int pointer, int button )
    {
        lock ( this )
        {
            if ( !_connected )
            {
                return false;
            }
        }

        Debug.Assert( _out != null, nameof( _out ) + " is null" );

        try
        {
            _out.Write( TOUCH_DOWN );
            _out.Write( x );
            _out.Write( y );
            _out.Write( pointer );
        }
        catch ( Exception )
        {
            lock ( this )
            {
                _connected = false;
            }
        }

        return false;
    }

    public bool TouchUp( int x, int y, int pointer, int button )
    {
        lock ( this )
        {
            if ( !_connected )
            {
                return false;
            }
        }

        Debug.Assert( _out != null, nameof( _out ) + " is null" );

        try
        {
            _out.Write( TOUCH_UP );
            _out.Write( x );
            _out.Write( y );
            _out.Write( pointer );
        }
        catch ( Exception )
        {
            lock ( this )
            {
                _connected = false;
            }
        }

        return false;
    }

    public bool TouchDragged( int x, int y, int pointer )
    {
        lock ( this )
        {
            if ( !_connected )
            {
                return false;
            }
        }

        Debug.Assert( _out != null, nameof( _out ) + " is null" );

        try
        {
            _out.Write( TOUCH_DRAGGED );
            _out.Write( x );
            _out.Write( y );
            _out.Write( pointer );
        }
        catch ( Exception )
        {
            lock ( this )
            {
                _connected = false;
            }
        }

        return false;
    }

    public bool MouseMoved( int x, int y )
    {
        return false;
    }

    public bool Scrolled( float amountX, float amountY )
    {
        return false;
    }

    public void SendUpdate()
    {
        lock ( this )
        {
            if ( !_connected )
            {
                return;
            }
        }

        Debug.Assert( _out != null, nameof( _out ) + " is null" );

        try
        {
            _out.Write( ACCEL );
            _out.Write( Api.Input.GetAccelerometerX() );
            _out.Write( Api.Input.GetAccelerometerY() );
            _out.Write( Api.Input.GetAccelerometerZ() );
            _out.Write( COMPASS );
            _out.Write( Api.Input.GetAzimuth() );
            _out.Write( Api.Input.GetPitch() );
            _out.Write( Api.Input.GetRoll() );
            _out.Write( SIZE );
            _out.Write( Api.Graphics.Width );
            _out.Write( Api.Graphics.Height );
            _out.Write( GYRO );
            _out.Write( Api.Input.GetGyroscopeX() );
            _out.Write( Api.Input.GetGyroscopeY() );
            _out.Write( Api.Input.GetGyroscopeZ() );
        }
        catch ( Exception )
        {
            _out       = null;
            _connected = false;
        }
    }
}