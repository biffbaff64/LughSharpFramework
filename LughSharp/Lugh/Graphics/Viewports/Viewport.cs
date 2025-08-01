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

using LughSharp.Lugh.Graphics.Cameras;
using LughSharp.Lugh.Graphics.Utils;
using LughSharp.Lugh.Maths.Collision;
using LughSharp.Lugh.Scenes.Scene2D.Utils;
using LughSharp.Lugh.Utils;


using Rectangle = LughSharp.Lugh.Maths.Rectangle;

namespace LughSharp.Lugh.Graphics.Viewports;

/// <summary>
/// Manages a <see cref="Camera" /> and determines how world coordinates
/// are mapped to and from the screen.
/// Extending classes should initialise <see cref="Camera" /> to avoid
/// causing exceptions.
/// </summary>
[PublicAPI]
public abstract class Viewport
{
    public enum ViewportType
    {
        Extended,
        Fill,
        Fit,
        Stretch,
        Screen,
        Scaling,
    }

    // ========================================================================
    
    public Camera? Camera       { get; set; }
    public int     ScreenX      { get; private set; }
    public int     ScreenY      { get; private set; }
    public int     ScreenWidth  { get; private set; }
    public int     ScreenHeight { get; private set; }
    public float   WorldWidth   { get; private set; }
    public float   WorldHeight  { get; private set; }

    // ========================================================================

    private Vector3 _tmp = Vector3.Zero;

    // ========================================================================

    /// <summary>
    /// Creates a new viewport using the supplied <see cref="OrthographicCamera" />.
    /// </summary>
    /// <param name="camera"> The camera to use. </param>
    protected Viewport( Camera camera )
    {
        Camera = camera;
    }

    // ========================================================================

    /// <summary>
    /// Configures this viewport's screen bounds using the specified screen
    /// size and calls <see cref="Apply(bool)" />. Typically called
    /// from <see cref="IApplicationListener.Resize" /> or
    /// <see cref="IScreen.Resize(int, int)" />.
    /// </summary>
    /// <param name="screenWidth"></param>
    /// <param name="screenHeight"></param>
    /// <param name="centerCamera"></param>
    /// <remarks>
    /// The default implementation only calls <see cref="Apply(bool)" />.
    /// </remarks>
    public virtual void Update( int screenWidth, int screenHeight, bool centerCamera = false )
    {
        Apply( centerCamera );
    }

    /// <summary>
    /// Applies the viewport to the camera and sets the glViewport.
    /// </summary>
    /// <param name="centerCamera">
    /// If true, the camera position is set to the center of the world.
    /// </param>
    public virtual void Apply( bool centerCamera = false )
    {
        if ( Camera == null )
        {
            throw new NullReferenceException();
        }

        HdpiUtils.GLViewport( ScreenX, ScreenY, ScreenWidth, ScreenHeight );

        Camera.ViewportWidth  = WorldWidth;
        Camera.ViewportHeight = WorldHeight;

        if ( centerCamera )
        {
            Camera.Position.Set( WorldWidth / 2, WorldHeight / 2, 0 );
        }

        Camera.Update();
    }

    /// <summary>
    /// Transforms the specified screen coordinate to world coordinates.
    /// </summary>
    /// <returns> The vector that was passed in, transformed to world coordinates.</returns>
    /// <see cref="Camera.Unproject(Vector3)" />
    public virtual Vector2 Unproject( Vector2 screenCoords )
    {
        if ( Camera == null )
        {
            throw new NullReferenceException();
        }

        _tmp = new Vector3
        {
            X = screenCoords.X,
            Y = screenCoords.Y,
            Z = 1.0f,
        };

        Camera.Unproject( _tmp, ScreenX, ScreenY, ScreenWidth, ScreenHeight );

        screenCoords.Set( _tmp.X, _tmp.Y );

        return screenCoords;
    }

    /// <summary>
    /// Transforms the specified world coordinate to screen coordinates.
    /// </summary>
    /// <returns> The vector that was passed in, transformed to screen coordinates.</returns>
    /// <see cref="Camera.Project(Vector3) " />
    public virtual Vector2 Project( Vector2 worldCoords )
    {
        if ( Camera == null )
        {
            throw new NullReferenceException();
        }

        _tmp.Set( worldCoords.X, worldCoords.Y, 1 );

        Camera.Project( _tmp, ScreenX, ScreenY, ScreenWidth, ScreenHeight );
        worldCoords.Set( _tmp.X, _tmp.Y );

        return worldCoords;
    }

    /// <summary>
    /// Transforms the specified screen coordinate to world coordinates.
    /// </summary>
    /// <returns> The vector that was passed in, transformed to world coordinates.</returns>
    /// <see cref="Camera.Unproject(Vector3)" />
    public virtual Vector3 Unproject( Vector3 screenCoords )
    {
        if ( Camera == null )
        {
            throw new NullReferenceException();
        }

        Camera.Unproject( _tmp, ScreenX, ScreenY, ScreenWidth, ScreenHeight );

        return screenCoords;
    }

    /// <summary>
    /// Transforms the specified world coordinate to screen coordinates.
    /// </summary>
    /// <returns> The vector that was passed in, transformed to screen coordinates. </returns>
    /// <see cref="Camera.Project(Vector3) " />
    public virtual Vector3 Project( Vector3 worldCoords )
    {
        if ( Camera == null )
        {
            throw new NullReferenceException();
        }

        Camera.Project( _tmp, ScreenX, ScreenY, ScreenWidth, ScreenHeight );

        return worldCoords;
    }

    /// <summary>
    /// Creates a picking Ray from the coordinates given in screen coordinates.
    /// </summary>
    /// <see cref="Camera.GetPickRay(float, float, float, float, float, float)" />
    public virtual Ray GetPickRay( float screenX, float screenY )
    {
        if ( Camera == null )
        {
            throw new NullReferenceException();
        }

        return Camera.GetPickRay( screenX, screenY, ScreenX, ScreenY, ScreenWidth, ScreenHeight );
    }

    /// <summary>
    /// Calculates a scissor rectangle in OpenGL window coordinates.
    /// <see cref="ScissorStack" />.CalculateScissors methods for more details.
    /// </summary>
    public virtual void CalculateScissors( Matrix4 batchTransform, Rectangle area, Rectangle scissor )
    {
        if ( Camera == null )
        {
            return;
        }

        ScissorStack.CalculateScissors( Camera, ScreenX, ScreenY, ScreenWidth, ScreenHeight, batchTransform, area, scissor );
    }

    /// <summary>
    /// Transforms a point to real screen coordinates (as opposed to OpenGL
    /// window coordinates), where the origin is in the top left and the
    /// the y-axis is pointing downwards.
    /// </summary>
    public virtual Vector2 ToScreenCoordinates( Vector2 worldCoords, Matrix4 transformMatrix )
    {
        if ( Camera == null )
        {
            throw new NullReferenceException();
        }

        _tmp.Set( worldCoords.X, worldCoords.Y, 0 );
        _tmp.Mul( transformMatrix );

        Camera.Project( _tmp, ScreenX, ScreenY, ScreenWidth, ScreenHeight );

        _tmp.Y = Api.Graphics.Height - _tmp.Y;

        worldCoords.X = _tmp.X;
        worldCoords.Y = _tmp.Y;

        return worldCoords;
    }

    /// <summary>
    /// Sets the World Size to the supplied width and height.
    /// </summary>
    /// <param name="worldWidth"> New World width in pixels. </param>
    /// <param name="worldHeight"> New World height in pixels. </param>
    public void SetWorldSize( float worldWidth, float worldHeight )
    {
        Logger.Debug( $"SetWorldSize: {worldWidth}, {worldHeight}" );

        WorldWidth  = worldWidth;
        WorldHeight = worldHeight;
    }

    /// <summary>
    /// Sets the viewport's position in screen coordinates.
    /// This is typically set by <see cref="Update(int, int, bool)" />.
    /// </summary>
    public void SetScreenPosition( int screenX, int screenY )
    {
        Logger.Debug( $"SetScreenPosition: {screenX}, {screenY}" );

        ScreenX = screenX;
        ScreenY = screenY;
    }

    /// <summary>
    /// Sets the viewport's size in screen coordinates.
    /// This is typically set by <see cref="Update(int, int, bool)" />.
    /// </summary>
    public void SetScreenSize( int screenWidth, int screenHeight )
    {
        Logger.Debug( $"SetScreenSize: {screenWidth}, {screenHeight}" );

        ScreenWidth  = screenWidth;
        ScreenHeight = screenHeight;
    }

    /// <summary>
    /// Sets the viewport's bounds in screen coordinates.
    /// This is typically set by <see cref="Update(int, int, bool)" />.
    /// </summary>
    public void SetScreenBounds( int screenX, int screenY, int screenWidth, int screenHeight )
    {
        Logger.Debug( $"SetScreenBounds: {screenX}, {screenY}, {screenWidth}, {screenHeight}" );

        if ( ( screenWidth <= 0 ) || ( screenHeight <= 0 ) )
        {
            Logger.Warning( "Screen bounds size must be positive and > zero!" );
        }

        ScreenX      = screenX;
        ScreenY      = screenY;
        ScreenWidth  = screenWidth;
        ScreenHeight = screenHeight;
    }

    // ========================================================================

    /// <summary>
    /// Returns the left gutter (black bar) width in screen coordinates.
    /// </summary>
    public virtual int LeftGutterWidth => ScreenX;

    /// <summary>
    /// Returns the right gutter (black bar) width in screen coordinates.
    /// </summary>
    public virtual int RightGutterWidth => Api.Graphics.Width - ( ScreenX + ScreenWidth );

    /// <summary>
    /// Returns the right gutter (black bar) x in screen coordinates.
    /// </summary>
    public virtual int RightGutterX => ScreenX + ScreenWidth;

    /// <summary>
    /// Returns the top gutter (black bar) y in screen coordinates.
    /// </summary>
    public virtual int TopGutterY => ScreenY + ScreenHeight;

    /// <summary>
    /// Returns the bottom gutter (black bar) height in screen coordinates.
    /// </summary>
    public virtual int BottomGutterHeight => ScreenY;

    /// <summary>
    /// Returns the top gutter (black bar) height in screen coordinates.
    /// </summary>
    public virtual int TopGutterHeight => Api.Graphics.Height - ( ScreenY + ScreenHeight );

    // ========================================================================

    public void Debug()
    {
        Logger.Checkpoint();
        Logger.Debug( $"ScreenX: {ScreenX}" );
        Logger.Debug( $"ScreenY: {ScreenY}" );
        Logger.Debug( $"ScreenWidth: {ScreenWidth}" );
        Logger.Debug( $"ScreenHeight: {ScreenHeight}" );
        Logger.Debug( $"WorldWidth: {WorldWidth}" );
        Logger.Debug( $"WorldHeight: {WorldHeight}" );
        Logger.Debug( $"LeftGutterWidth: {LeftGutterWidth}" );
        Logger.Debug( $"RightGutterWidth: {RightGutterWidth}" );
        Logger.Debug( $"TopGutterHeight: {TopGutterHeight}" );
        Logger.Debug( $"BottomGutterHeight: {BottomGutterHeight}" );
        Logger.Debug( $"RightGutterX: {RightGutterX}" );
        Logger.Debug( $"TopGutterY: {TopGutterY}" );
    }
}