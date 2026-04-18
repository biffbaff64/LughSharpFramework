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

using LughSharp.Core.Scene2D;
using LughSharp.Core.Scene2D.Listeners;

using NSubstitute;

using NUnit.Framework;

namespace LughSharp.Tests.Source;

[PublicAPI]
[TestFixture]
public class ClickListenerTests
{
    // A simple derived class so we can spy on OnClicked
    private class TestClickListener : ClickListener
    {
        public bool  WasClicked { get; private set; }
        public float ClickX     { get; private set; }
        public float ClickY     { get; private set; }

        public override void OnClicked( InputEvent ev, float x, float y )
        {
            WasClicked = true;
            ClickX     = x;
            ClickY     = y;
        }
    }

    private InputEvent CreateTouchEvent( InputEvent.EventType type, int pointer = 0, int button = 0 )
    {
        return new InputEvent
        {
            Type    = type,
            Pointer = pointer,
            Button  = button,
            StageX  = 100,
            StageY  = 100
        };
    }

    [Test]
    public void TouchDown_Then_TouchUp_TriggersClick()
    {
        // Arrange
        var        listener  = new TestClickListener();
        InputEvent touchDown = CreateTouchEvent( InputEvent.EventType.TouchDown );
        InputEvent touchUp   = CreateTouchEvent( InputEvent.EventType.TouchUp );

        // Act
        listener.OnTouchDown( touchDown, 50, 50, 0, 0 );
        listener.OnTouchUp( touchUp, 50, 50, 0, 0 );

        // Assert
        Assert.That( listener.WasClicked, Is.True );
        Assert.That( listener.TapCount, Is.EqualTo( 1 ) );
    }

    [Test]
    public void TwoRapidTouches_IncrementsTapCount()
    {
        // Arrange
        var        listener   = new TestClickListener();
        InputEvent touchEvent = CreateTouchEvent( InputEvent.EventType.TouchDown );

        // Act - First Click
        listener.OnTouchDown( touchEvent, 10, 10, 0, 0 );
        listener.OnTouchUp( touchEvent, 10, 10, 0, 0 );

        // Act - Second Click immediately after
        listener.OnTouchDown( touchEvent, 10, 10, 0, 0 );
        listener.OnTouchUp( touchEvent, 10, 10, 0, 0 );

        // Assert
        Assert.That( listener.TapCount, Is.EqualTo( 2 ) );
    }

    [Test]
    public void DraggingOutside_AndReleasing_DoesNotTriggerClick()
    {
        // Arrange
        var listener = new TestClickListener();
        var actor    = Substitute.For< Actor >(); // NSubstitute mocking
        actor.Hit( Arg.Any< float >(), Arg.Any< float >(), true ).Returns( ( Actor )null! );

        InputEvent ev = CreateTouchEvent( InputEvent.EventType.TouchDragged );
        ev.ListenerActor = actor;

        // Act
        listener.OnTouchDown( ev, 0, 0, 0, 0 );

        // Drag far away (Assuming default TapSquareSize is 14)
        listener.OnTouchDragged( ev, 100, 100, 0 );
        listener.OnTouchUp( ev, 100, 100, 0, 0 );

        // Assert
        Assert.That( listener.WasClicked, Is.False );
        Assert.That( listener.Pressed, Is.False );
    }

    [Test]
    public void TouchFocusCancel_AbortsTheClick()
    {
        // Arrange
        var        listener  = new TestClickListener();
        InputEvent touchDown = CreateTouchEvent( InputEvent.EventType.TouchDown );

        var touchUpCancelled = new InputEvent
        {
            Type   = InputEvent.EventType.TouchUp,
            StageX = int.MinValue, // Triggers TouchFocusCancel check
            StageY = int.MinValue
        };

        // Act
        listener.OnTouchDown( touchDown, 10, 10, 0, 0 );
        listener.OnTouchUp( touchUpCancelled, 10, 10, 0, 0 );

        // Assert
        Assert.That( listener.WasClicked, Is.False );
        Assert.That( listener.Pressed, Is.False );
    }
}

// ============================================================================
// ============================================================================