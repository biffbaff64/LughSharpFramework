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

using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Graphics.Images;

namespace LughSharp.Source.Scene2D.Utils;

/// <summary>
/// Drawable for a <see cref="NinePatch"/>.
/// <para>
/// The drawable sizes are set when the ninepatch is set, but they are separate
/// values. Eg, <see cref="ISceneDrawable.LeftWidth"/> could be set to more than
/// <see cref="NinePatch.LeftWidth"/> in order to provide more space on the left
/// than actually exists in the ninepatch.
/// </para>
/// The min size is set to the ninepatch total size by default. It could be set
/// to the left+right and top+bottom, excluding the middle size, to allow the
/// drawable to be sized down as small as possible.
/// </summary>
[PublicAPI]
public class NinePatchDrawable : BaseDrawable, ITransformDrawable
{
    public NinePatch? Patch { get; set; }

    // ========================================================================

    /// <summary>
    /// Creates an uninitialized NinePatchDrawable. The ninepatch must be set before use.
    /// </summary>
    public NinePatchDrawable()
    {
    }

    /// <summary>
    /// Creates a new NinePatchDrawable, initialised with the supplied <see cref="NinePatch"/>.
    /// </summary>
    /// <param name="patch"></param>
    public NinePatchDrawable( NinePatch patch )
    {
        SetPatch( patch );
    }

    /// <summary>
    /// Creates a new NinePatchDrawable, initialised with the <see cref="NinePatch"/>
    /// from another NinePatchDrawable.
    /// </summary>
    /// <param name="drawable"></param>
    public NinePatchDrawable( NinePatchDrawable drawable )
        : base( drawable )
    {
        Patch = drawable.Patch;
    }

    /// <inheritdoc />
    public override void Draw( IBatch batch, float x, float y, float width, float height )
    {
        Patch?.Draw( batch, x, y, width, height );
    }

    /// <summary>
    /// Draws the NinePatch image using the specified parameters.
    /// </summary>
    /// <param name="batch">The batch object used to render the drawable.</param>
    /// <param name="region">
    /// The rectangular region specifying the dimensions and position to render the drawable.
    /// </param>
    /// <param name="origin">The origin point for scaling and rotation.</param>
    /// <param name="scale">The scaling factors to apply to the width and height of the drawable.</param>
    /// <param name="rotation">The rotation angle, in degrees, to apply to the drawable.</param>
    public void Draw( IBatch batch, GRect region, Point2D origin, Point2D scale, float rotation )
    {
        Patch?.Draw( batch,
                     region.X,
                     region.Y,
                     origin.X,
                     origin.Y,
                     region.Width,
                     region.Height,
                     scale.X,
                     scale.Y,
                     rotation );
    }

    /// <summary>
    /// Sets the nine-patch for this drawable and updates its dimensions and paddings.
    /// </summary>
    /// <param name="patch">
    /// The <see cref="NinePatch"/> to be set. If null, no patch will be associated with
    /// the drawable.
    /// </param>
    public void SetPatch( NinePatch patch )
    {
        Patch = patch;

        if ( Patch != null )
        {
            MinWidth     = patch.TotalWidth;
            MinHeight    = patch.TotalHeight;
            TopHeight    = patch.PadTop;
            RightWidth   = patch.PadRight;
            BottomHeight = patch.PadBottom;
            LeftWidth    = patch.PadLeft;
        }
    }

    /// <summary>
    /// Creates a new drawable that renders the same as this drawable tinted
    /// the specified color.
    /// </summary>
    public NinePatchDrawable Tint( Color tint )
    {
        var drawable = new NinePatchDrawable( this );

        drawable.Patch = new NinePatch( drawable.Patch!, tint );

        return drawable;
    }

    /// <summary>
    /// Creates a copy of this drawable.
    /// </summary>
    public override NinePatchDrawable Copy() => new( this );
}

// ============================================================================
// ============================================================================

