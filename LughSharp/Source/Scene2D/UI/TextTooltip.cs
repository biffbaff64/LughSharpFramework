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

using LughSharp.Source.Scene2D.UI.Styles;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// A tooltip that shows a label.
/// </summary>
[PublicAPI]
public class TextTooltip : Tooltip< Label >, IStyleable< TextTooltipStyle >
{
    private TextTooltipStyle? _style;
    
    // ========================================================================

    /// <summary>
    /// Represents a tooltip that displays text in the form of a label.
    /// This class extends the functionality of the base <see cref="Tooltip{T}"/> class,
    /// providing specific support for displaying styled labels as tooltips.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    /// <param name="skin">The skin to use for the tooltip.</param>
    /// <param name="style">The style to use for the tooltip.</param>
    public TextTooltip( string text, Skin skin, TextTooltipStyle style )
        : this( text, new TooltipManager< Label >(), style )
    {
    }

    /// <summary>
    /// Represents a tooltip that displays a text label.
    /// This class extends the functionality of the <see cref="Tooltip{T}"/> class, allowing
    /// the integration of styled labels as part of tooltip functionality in a UI context.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    /// <param name="skin">The skin to use for the tooltip.</param>
    public TextTooltip( string text, Skin skin )
        : this( text, new TooltipManager< Label >(), skin.Get< TextTooltipStyle >() )
    {
    }

    /// <summary>
    /// Represents a tooltip that displays text within a styled label.
    /// Extends the generic <see cref="Tooltip{T}"/> class, utilizing <see cref="Label"/> as
    /// the tooltip content. This class provides various constructors for creating text tooltips
    /// using different combinations of text, styles, and tooltip management options.
    /// </summary>
    /// <param name="text">The text to display in the tooltip.</param>
    /// <param name="skin">The skin to use for retrieving the tooltip style.</param>
    /// <param name="styleName">The name of the style to retrieve from the skin.</param>
    public TextTooltip( string text, Skin skin, string styleName )
        : this( text, new TooltipManager< Label >(), skin.Get< TextTooltipStyle >( styleName ) )
    {
    }

    /// <summary>
    /// Represents a tooltip that displays text within a label.
    /// This class extends the <see cref="Tooltip{T}"/> class, offering specific configurations
    /// for creating and managing tooltips styled with <see cref="TextTooltipStyle"/>.
    /// </summary>
    /// <param name="text">The text content to display in the tooltip, rendered as a styled label.</param>
    /// <param name="style">The <see cref="TextTooltipStyle"/> used to define the tooltip's appearance and behavior.</param>
    public TextTooltip( string text, TextTooltipStyle style )
        : this( text, new TooltipManager< Label >(), style )
    {
    }
    
    /// <summary>
    /// Represents a tooltip that displays text within a label.
    /// This class extends the <see cref="Tooltip{T}"/> class, offering specific configurations
    /// for creating and managing tooltips styled with <see cref="TextTooltipStyle"/>.
    /// </summary>
    /// <param name="text">The text content to display in the tooltip, rendered as a styled label.</param>
    /// <param name="manager">
    /// The <see cref="TooltipManager{T}"/> responsible for managing the tooltip's lifecycle and display.
    /// </param>
    /// <param name="skin">The skin to use for retrieving the tooltip style.</param>
    public TextTooltip( string text, TooltipManager< Label > manager, Skin skin )
        : this( text, manager, skin.Get< TextTooltipStyle >() )
    {
    }

    /// <summary>
    /// Represents a tooltip that displays text using a <see cref="Label"/> component.
    /// This class extends the functionality of the <see cref="Tooltip{T}"/> class,
    /// providing specialized support for creating and managing tooltips styled with
    /// <see cref="TextTooltipStyle"/>.
    /// </summary>
    /// <param name="text">The text content to display within the tooltip.</param>
    /// <param name="skin">
    /// The <see cref="Skin"/> used to configure the tooltip styling and resources.
    /// </param>
    /// <param name="manager">
    /// The <see cref="TooltipManager{T}"/> responsible for managing tooltip behavior
    /// and positioning.
    /// </param>
    /// <param name="styleName">The name of the style specified in the <see cref="Skin"/>.</param>
    public TextTooltip( string text, TooltipManager< Label > manager, Skin skin, string styleName )
        : this( text, manager, skin.Get< TextTooltipStyle >( styleName ) )
    {
    }

    /// <summary>
    /// Represents a tooltip that displays textual content with configurable styles and behavior.
    /// This class extends the <see cref="Tooltip{T}"/> functionality, using a <see cref="Label"/>
    /// to present styled, wrapped text within the tooltip.
    /// </summary>
    /// <remarks>
    /// This tooltip supports customization through the use of a <see cref="TextTooltipStyle"/>,
    /// providing a flexible mechanism to define visual appearance (e.g., font styles) and layout.
    /// It works in conjunction with a <see cref="TooltipManager{T}"/> to manage tooltip behavior
    /// such as maximum width or other contextual settings.
    /// </remarks>
    /// <param name="text">The textual content to be displayed in the tooltip.</param>
    /// <param name="style">
    /// The <see cref="TextTooltipStyle"/> that defines appearance and layout properties for the tooltip.
    /// </param>
    /// <param name="manager">
    /// The <see cref="TooltipManager{T}"/> responsible for handling tooltip behavior and constraints.
    /// </param>
    public TextTooltip( string text, TooltipManager< Label > manager, TextTooltipStyle style )
        : base( null, manager )
    {
        var label = new Label( text, style.LabelStyle )
        {
            Wrap = true
        };

        Container.SetContainerActor( label );
        Container.SetWidths( Math.Min( manager.MaxWidth, label.GlyphLayout.Width ) );

        SetStyle( style );
    }

    /// <summary>
    /// Get the current style of the actor
    /// </summary>
    /// <returns>The current <see cref="TextTooltipStyle"/> applied to the tooltip.</returns>
    public TextTooltipStyle GetStyle()
    {
        return _style ?? throw new NullReferenceException( "Style cannot be null" );
    }

    /// <summary>
    /// Set the current style of the actor
    /// </summary>
    /// <param name="value">The <see cref="TextTooltipStyle"/> to apply to the tooltip.</param>
    public void SetStyle( TextTooltipStyle value )
    {
        _style = value;

        if ( Container == null )
        {
            throw new NullReferenceException( "Container cannot be null" );
        }

        Container.GetContainerActor()?.SetStyle( value.LabelStyle );
        Container.SetBackground( value.Background );
        Container.SetMaxWidth( value.WrapWidth );
    }
}

// ============================================================================
// ============================================================================