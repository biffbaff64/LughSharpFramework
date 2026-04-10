// ///////////////////////////////////////////////////////////////////////////////
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

using JetBrains.Annotations;

using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils.Exceptions;

namespace LughSharp.Core.SceneGraph2D.UI.Styles;

/// <summary>
/// The style for a button, see <see cref="Button"/>.
/// </summary>
[PublicAPI]
public class ButtonStyle : ISceneStyle
{
    public ISceneDrawable? Up             { get; set; }
    public ISceneDrawable? Down           { get; set; }
    public ISceneDrawable? Over           { get; set; }
    public ISceneDrawable? Focused        { get; set; }
    public ISceneDrawable? Disabled       { get; set; }
    public ISceneDrawable? Checked        { get; set; }
    public ISceneDrawable? CheckedOver    { get; set; }
    public ISceneDrawable? CheckedDown    { get; set; }
    public ISceneDrawable? CheckedFocused { get; set; }

    public float PressedOffsetX   { get; set; }
    public float PressedOffsetY   { get; set; }
    public float UnpressedOffsetX { get; set; }
    public float UnpressedOffsetY { get; set; }
    public float CheckedOffsetX   { get; set; }
    public float CheckedOffsetY   { get; set; }

    // ====================================================================

    /// <summary>
    /// Creates a new empty ButtonStyle instance. Before using this style it will
    /// be necessary to set the <see cref="Up"/>, <see cref="Down"/> ISceneDrawable
    /// properties.
    /// </summary>
    public ButtonStyle()
    {
    }

    /// <summary>
    /// Creates a new ButtonStyle instance with the given ISceneDrawables.
    /// </summary>
    /// <param name="up"> The ISceneDrawable image for button UP. </param>
    /// <param name="down"> The ISceneDrawable image for button DOWN. </param>
    /// <param name="chcked"></param>
    public ButtonStyle( ISceneDrawable? up, ISceneDrawable? down, ISceneDrawable? chcked )
    {
        Up      = up;
        Down    = down;
        Checked = chcked;
    }

    /// <summary>
    /// Creates a new ButtonStyle as a copy of the given ButtonStyle.
    /// </summary>
    public ButtonStyle( ButtonStyle style )
    {
        Up               = style.Up;
        Down             = style.Down;
        Over             = style.Over;
        Focused          = style.Focused;
        Disabled         = style.Disabled;
        Checked          = style.Checked;
        CheckedOver      = style.CheckedOver;
        CheckedDown      = style.CheckedDown;
        CheckedFocused   = style.CheckedFocused;
        PressedOffsetX   = style.PressedOffsetX;
        PressedOffsetY   = style.PressedOffsetY;
        UnpressedOffsetX = style.UnpressedOffsetX;
        UnpressedOffsetY = style.UnpressedOffsetY;
        CheckedOffsetX   = style.CheckedOffsetX;
        CheckedOffsetY   = style.CheckedOffsetY;
    }

    /// <summary>
    /// Sets the properties of this ButtonStyle to the properties from the given
    /// style, which must be a subclass of ButtonStyle.
    /// </summary>
    /// <param name="style"></param>
    /// <typeparam name="T"></typeparam>
    public void Set< T >( T style ) where T : ButtonStyle
    {
        Guard.Against.Null( style );

        if ( typeof( T ).IsSubclassOf( typeof( ButtonStyle ) ) )
        {
            Up               = style.Up;
            Down             = style.Down;
            Over             = style.Over;
            Focused          = style.Focused;
            Disabled         = style.Disabled;
            Checked          = style.Checked;
            CheckedOver      = style.CheckedOver;
            CheckedDown      = style.CheckedDown;
            CheckedFocused   = style.CheckedFocused;
            PressedOffsetX   = style.PressedOffsetX;
            PressedOffsetY   = style.PressedOffsetY;
            UnpressedOffsetX = style.UnpressedOffsetX;
            UnpressedOffsetY = style.UnpressedOffsetY;
            CheckedOffsetX   = style.CheckedOffsetX;
            CheckedOffsetY   = style.CheckedOffsetY;
        }
    }
}

// ============================================================================
// ============================================================================

