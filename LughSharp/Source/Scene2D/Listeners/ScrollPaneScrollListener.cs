// /////////////////////////////////////////////////////////////////////////////
// //  MIT License
// //
// //  Copyright (c) 2024 Richard Ikin
// //
// //  Permission is hereby granted, free of charge, to any person obtaining a copy
// //  of this software and associated documentation files (the "Software"), to deal
// //  in the Software without restriction, including without limitation the rights
// //  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// //  copies of the Software, and to permit persons to whom the Software is
// //  furnished to do so, subject to the following conditions:
// //
// //  The above copyright notice and this permission notice shall be included in all
// //  copies or substantial portions of the Software.
// //
// //  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// //  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// //  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// //  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// //  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// //  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// //  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using LughSharp.Source.Scene2D.UI;

namespace LughSharp.Source.Scene2D.Listeners;

/// <summary>
/// A listener for handling scrolling events within a <see cref="ScrollPane"/>.
/// This listener responds to mouse wheel scroll inputs and adjusts the
/// scroll position of the associated <see cref="ScrollPane"/>.
/// </summary>
[PublicAPI]
public sealed class ScrollPaneScrollListener( ScrollPane parent ) : InputListener
{
    private readonly ScrollPane? _parent = parent;

    // ========================================================================

    /// <summary>
    /// Called when the mouse wheel has been scrolled. When true is returned,
    /// the event is handled in <see cref="Event.SetHandled"/>.
    /// </summary>
    public override bool OnScrolled( InputEvent? ev, float x, float y, float amountX, float amountY )
    {
        Guard.Against.Null( _parent );

        _parent!.SetScrollbarsVisible( true );

        if ( _parent!.IsScrollY || _parent!.IsScrollX )
        {
            if ( _parent!.IsScrollY )
            {
                if ( !_parent!.IsScrollX && ( amountY == 0 ) )
                {
                    amountY = amountX;
                }
            }
            else
            {
                if ( _parent!.IsScrollX && ( amountX == 0 ) )
                {
                    amountX = amountY;
                }
            }

            _parent!.ScrollAmountY += _parent!.GetMouseWheelY() * amountY;
            _parent!.ScrollAmountX += _parent!.GetMouseWheelX() * amountX;
        }
        else
        {
            return false;
        }

        return true;
    }
}

// ============================================================================
// ============================================================================



