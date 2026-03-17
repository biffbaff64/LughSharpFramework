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

using System;

using JetBrains.Annotations;

using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils;

namespace LughSharp.Core.SceneGraph2D.UI;

[PublicAPI]
public class CheckBox : TextButton
{
    public Scene2DImage? Image     { get; set; }
    public Cell?         ImageCell { get; set; }

    // ========================================================================

    public CheckBox( string text, Skin skin )
        : this( text, skin.Get< CheckBoxStyle >() )
    {
    }

    public CheckBox( string text, Skin skin, string styleName )
        : this( text, skin.Get< CheckBoxStyle >( styleName ) )
    {
    }

    public CheckBox( string text, CheckBoxStyle style ) : base( text, style )
    {
        Setup( style );
    }

    public new CheckBoxStyle? Style
    {
        get;
        set
        {
            field      = value ?? throw new ArgumentException( "style must be a CheckBoxStyle." );
            base.Style = value;
        }
    }

    /// <summary>
    /// Private setup method to allow calls to virtual methods that can't
    /// be called from constructors.
    /// </summary>
    private void Setup( CheckBoxStyle style )
    {
        ClearChildren();

        Label? label = Label;

        Image = new Scene2DImage( style.CheckboxOff, Scaling.None );

        ImageCell = Add( Image );

        Add( label );

        label?.SetAlignment( Align.Left );

        SetSize( GetPrefWidth(), GetPrefHeight() );
    }

    public override void Draw( IBatch batch, float parentAlpha )
    {
        ISceneDrawable? checkbox = null;

        if ( IsDisabled )
        {
            if ( IsChecked && ( Style?.CheckboxOnDisabled != null ) )
            {
                checkbox = Style?.CheckboxOnDisabled;
            }
            else
            {
                checkbox = Style?.CheckboxOffDisabled;
            }
        }

        if ( checkbox == null )
        {
            bool over = IsOver() && !IsDisabled;

            if ( IsChecked && ( Style?.CheckboxOn != null ) )
            {
                checkbox = over && ( Style?.CheckboxOnOver != null )
                    ? Style?.CheckboxOnOver
                    : Style?.CheckboxOn;
            }
            else if ( over && ( Style?.CheckboxOver != null ) )
            {
                checkbox = Style?.CheckboxOver;
            }
            else
            {
                checkbox = Style?.CheckboxOff;
            }
        }

        Image?.SetDrawable( checkbox );

        base.Draw( batch, parentAlpha );
    }
    
    public override string? Name => GetType().Name;
}

// ============================================================================
// ============================================================================

