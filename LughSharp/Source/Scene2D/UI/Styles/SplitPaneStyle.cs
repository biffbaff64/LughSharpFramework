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

using LughSharp.Source.Scene2D.Utils;

namespace LughSharp.Source.Scene2D.UI.Styles;

/// <summary>
/// The style for a split pane, see <see cref="SplitPane"/>.
/// </summary>
[PublicAPI]
public class SplitPaneStyle : ISceneStyle
{
    /// <summary>
    /// The drawable element defining the appearance of the handle in a <see cref="SplitPane"/>.
    /// This property determines how the dividing handle is rendered, including its visual style
    /// and minimum size. The handle allows the user to resize the split panes interactively.
    /// </summary>
    public ISceneDrawable? Handle { get; set; }

    // ========================================================================

    /// <summary>
    /// Represents a style definition for a split pane component in a user interface.
    /// </summary>
    public SplitPaneStyle()
    {
    }

    /// <summary>
    /// Defines a style for a split pane component in a user interface, providing customization
    /// options for the appearance and behavior of the split pane.
    /// </summary>
    /// <param name="handle">
    /// The drawable element defining the appearance of the handle in a <see cref="SplitPane"/>.
    /// </param>
    public SplitPaneStyle( ISceneDrawable handle )
    {
        Handle = handle;
    }

    /// <summary>
    /// Creates a new SplitPaneStyle from the provided SplitPaneStyle.
    /// </summary>
    /// <param name="style">The SplitPaneStyle to copy.</param>
    public SplitPaneStyle( SplitPaneStyle style )
    {
        Handle = style.Handle;
    }
}

// ============================================================================
// ============================================================================

