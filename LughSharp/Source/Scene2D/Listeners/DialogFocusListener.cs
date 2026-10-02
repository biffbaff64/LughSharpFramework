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

using LughSharp.Source.Scene2D.UI;

namespace LughSharp.Source.Scene2D.Listeners;

/// <summary>
/// Focus listener for Scene2D dialogs.
/// </summary>
/// <param name="dialog"> The parent <see cref="Dialog"/>. </param>
public sealed class DialogFocusListener( Dialog dialog ) : FocusListener
{
    public override void KeyboardFocusChanged( FocusEvent ev, Actor? actor, bool focused )
    {
        if ( !focused )
        {
            FocusChanged( ev );
        }
    }

    public override void ScrollFocusChanged( FocusEvent ev, Actor? actor, bool focused )
    {
        if ( !focused )
        {
            FocusChanged( ev );
        }
    }

    private void FocusChanged( FocusEvent ev )
    {
        Stage? stage = dialog.GetStage();
        
        if ( stage == null )
        {
            return;
        }

        if ( dialog.IsModal
          && ( stage.RootGroup.Children.Size > 0 )
          && ( stage.RootGroup.Children.Peek() == dialog ) )
        {
            // Dialog is top most actor.
            Actor? newFocusedActor = ev.RelatedActor;

            if ( ( newFocusedActor != null )
              && !newFocusedActor.IsDescendantOf( dialog )
              && !( newFocusedActor.Equals( dialog.PreviousKeyboardFocus )
                 || newFocusedActor.Equals( dialog.PreviousScrollFocus ) ) )
            {
                ev.Cancel();
            }
        }
    }
}

// ============================================================================
// ============================================================================
