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

using System.Diagnostics;

using JetBrains.Annotations;

using LughSharp.Core.Main;

using Exception = System.Exception;

namespace LughSharp.Core.Input;

/// <summary>
/// Sends all inputs from touch, key, accelerometer and compass to
/// a <see cref="RemoteInput"/> at the given ip/port. Instantiate
/// this and call SendUpdate() periodically.
/// </summary>
[PublicAPI]
public class RemoteSender : IInputProcessor
{
    public const int KeyDown  = 0;
    public const int KeyUp    = 1;
    public const int KeyTyped = 2;

    public const int TouchDown    = 3;
    public const int TouchUp      = 4;
    public const int TouchDragged = 5;

    public const int Accel   = 6;
    public const int Compass = 7;
    public const int Size    = 8;
    public const int Gyro    = 9;

    // ========================================================================

    private bool _connected;

    private BinaryWriter? _out;

    // ========================================================================

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

    public bool OnKeyDown( int keycode )
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
            _out.Write( KeyDown );
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

    public bool OnKeyUp( int keycode )
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
            _out?.Write( KeyUp );
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

    public virtual bool OnKeyTyped( char character )
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
            _out?.Write( KeyTyped );
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

    public bool OnTouchDown( int x, int y, int pointer, int button )
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
            _out.Write( TouchDown );
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

    public bool OnTouchUp( int x, int y, int pointer, int button )
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
            _out.Write( TouchUp );
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

    public bool OnTouchDragged( int x, int y, int pointer )
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
            _out.Write( TouchDragged );
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

    public bool OnMouseMoved( int x, int y )
    {
        return false;
    }

    public bool OnScrolled( float amountX, float amountY )
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
            _out.Write( Accel );
            _out.Write( Engine.Api.Input.GetAccelerometerX() );
            _out.Write( Engine.Api.Input.GetAccelerometerY() );
            _out.Write( Engine.Api.Input.GetAccelerometerZ() );
            _out.Write( Compass );
            _out.Write( Engine.Api.Input.GetAzimuth() );
            _out.Write( Engine.Api.Input.GetPitch() );
            _out.Write( Engine.Api.Input.GetRoll() );
            _out.Write( Size );
            _out.Write( Engine.Api.Graphics.WindowWidth );
            _out.Write( Engine.Api.Graphics.WindowHeight );
            _out.Write( Gyro );
            _out.Write( Engine.Api.Input.GetGyroscopeX() );
            _out.Write( Engine.Api.Input.GetGyroscopeY() );
            _out.Write( Engine.Api.Input.GetGyroscopeZ() );
        }
        catch ( Exception )
        {
            _out       = null;
            _connected = false;
        }
    }
}

// ============================================================================
// ============================================================================