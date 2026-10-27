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
using LughSharp.Source.Scene2D.UI.Styles;

namespace LughSharp.Source.Miscellaneous;

[PublicAPI]
public class TestDialog : Dialog
{
    /// <summary>
    /// Creates a new Dialog, using the supplied title and <see cref="Skin"/>
    /// </summary>
    /// <param name="title"> A string holding the dialog name to display. </param>
    /// <param name="skin"> The Skin holding the WindowStyle.</param>
    public TestDialog( string title, Skin skin ) : base( title, skin )
    {
    }

    /// <summary>
    /// Creates a new Dialog, using the supplied title, <see cref="Skin"/>, and
    /// <see cref="WindowStyle"/>.
    /// </summary>
    /// <param name="title"> A string holding the dialog name to display. </param>
    /// <param name="skin"> The Skin holding the WindowStyle.</param>
    /// <param name="dialogStyle"> The <see cref="WindowStyle"/> to use. </param>
    public TestDialog( string title, Skin skin, string dialogStyle ) : base( title, skin, dialogStyle )
    {
    }

    /// <summary>
    /// Creates a new Dialog window, using the supplied name and <see cref="WindowStyle"/>.
    /// </summary>
    /// <param name="title"> A string holding the dialog name to display. </param>
    /// <param name="dialogStyle"> The <see cref="WindowStyle"/> to use. </param>
    public TestDialog( string title, WindowStyle dialogStyle ) : base( title, dialogStyle )
    {
    }

    /// <summary>
    /// Called when a button is clicked. The dialog will be hidden after this
    /// method returns unless <see cref="Dialog.CancelHide"/> is set.
    /// </summary>
    /// <param name="obj"> The object specified when the button was added. </param>
    public override void ClickResult( object? obj )
    {
        Logger.Debug( $"Chosen: {obj}" );
    }
}

// ============================================================================
// ============================================================================

