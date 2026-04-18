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

using JetBrains.Annotations;

using LughSharp.Core;
using LughSharp.Core.Files;
using LughSharp.Core.Graphics.Fonts;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Input;

using NUnit.Framework;

using Logger = LughSharp.Core.Utils.Logging.Logger;

namespace LughSharp.Tests.Source;

[TestFixture]
[PublicAPI]
public class InputTest : InputAdapter, ILughTest
{
    public InputTest( ref InputMultiplexer inputMultiplexer )
    {
        inputMultiplexer.AddProcessor( this );
    }

    [SetUp]
    public void Setup()
    {
    }

    public void Run()
    {
    }

    public void Update()
    {
        if ( Engine.Input.JustTouched() )
        {
            Logger.Debug( "JustTouched: button: "
                        + ( Engine.Input.IsButtonPressed( IInput.Buttons.Left ) ? "left " : "" )
                        + ( Engine.Input.IsButtonPressed( IInput.Buttons.Middle ) ? "middle " : "" )
                        + ( Engine.Input.IsButtonPressed( IInput.Buttons.Right ) ? "right" : "" )
                        + ( Engine.Input.IsButtonPressed( IInput.Buttons.Back ) ? "back" : "" )
                        + ( Engine.Input.IsButtonPressed( IInput.Buttons.Forward ) ? "forward" : "" ) );
        }

        for ( int i = 0; i < 10; i++ )
        {
            if ( Engine.Input.GetDeltaX( i ) != 0 || Engine.Input.GetDeltaY( i ) != 0 )
            {
                Logger.Debug( $"delta[{i}]: {Engine.Input.GetDeltaX( i )}, {Engine.Input.GetDeltaY( i )}" );
            }
        }
    }

    public void Render( SpriteBatch spriteBatch )
    {
    }

    [TearDown]
    public void TearDown()
    {
    }

    /// <inheritdoc />
    public override bool OnKeyDown( int keycode )
    {
        Logger.Debug( $"key down: {IInput.Keys.NameOf( keycode, true )}" );

        if ( keycode == IInput.Keys.G )
        {
            Engine.Input.SetCursorOverridden( !Engine.Input.IsCursorOverridden() );
        }

        return false;
    }

    /// <inheritdoc />
    public override bool OnKeyUp( int keycode )
    {
        Logger.Debug( $"key up: {IInput.Keys.NameOf( keycode, true )}" );

        return false;
    }

    /// <inheritdoc />
    public override bool OnKeyTyped( char character )
    {
        Logger.Debug( $"key typed: '{character}'" );

        return false;
    }

    /// <inheritdoc />
    public override bool OnTouchDown( int x, int y, int pointer, int button )
    {
        Logger.Debug( $"touch down: {x}, {y}, button: {IInput.Keys.NameOf( button, true )}" );

        return false;
    }

    /// <inheritdoc />
    public override bool OnTouchUp( int x, int y, int pointer, int button )
    {
        Logger.Debug( $"touch up: {x}, {y}, button: {IInput.Keys.NameOf( button, true )}" );

        return false;
    }

    /// <inheritdoc />
    public override bool OnTouchDragged( int x, int y, int pointer )
    {
        Logger.Debug( $"touch dragged: {x}, {y}, pointer: {pointer}" );

        return false;
    }

    /// <inheritdoc />
    public override bool OnMouseMoved( int x, int y )
    {
        Logger.Debug( $"touch moved: {x}, {y}" );

        return false;
    }

    /// <inheritdoc />
    public override bool OnScrolled( float amountX, float amountY )
    {
        Logger.Debug( $"scrolled: {amountY}" );

        return false;
    }
}

// ============================================================================
// ============================================================================