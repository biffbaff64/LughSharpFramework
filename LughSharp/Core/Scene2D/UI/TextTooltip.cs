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

using LughSharp.Core.Scene2D.UI.Styles;

namespace LughSharp.Core.Scene2D.UI;

/// <summary>
/// A tooltip that shows a label.
/// </summary>
[PublicAPI]
public class TextTooltip : Tooltip< Label >
{
    public TextTooltip( string text, Skin skin )
        : this( text, new TooltipManager< Label >(), skin.Get< TextTooltipStyle >() )
    {
    }

    public TextTooltip( string text, Skin skin, string styleName )
        : this( text, new TooltipManager< Label >(), skin.Get< TextTooltipStyle >( styleName ) )
    {
    }

    public TextTooltip( string text, TextTooltipStyle style )
        : this( text, new TooltipManager< Label >(), style )
    {
    }

    public TextTooltip( string text, TooltipManager< Label > manager, Skin skin )
        : this( text, manager, skin.Get< TextTooltipStyle >() )
    {
    }

    public TextTooltip( string text, TooltipManager< Label > manager, Skin skin, string styleName )
        : this( text, manager, skin.Get< TextTooltipStyle >( styleName ) )
    {
    }

    public TextTooltip( string text, TooltipManager< Label > manager, TextTooltipStyle style )
        : base( null, manager )
    {
        var label = new Label( text, style.Label )
        {
            Wrap = true
        };

        Container.SetActor( label );
        Container.SetWidths( Math.Min( manager.MaxWidth, label.GlyphLayout.Width ) );

        Style = style;
    }

    public TextTooltipStyle Style
    {
        get;
        set
        {
            field = value;

            if ( Container == null )
            {
                throw new NullReferenceException( "Container cannot be null" );
            }

            Container.GetActor()!.Style = value.Label;
            Container.SetBackground( value.Background );
            Container.SetMaxWidth( value.WrapWidth );
        }
    }

    // ========================================================================
}

// ============================================================================
// ============================================================================