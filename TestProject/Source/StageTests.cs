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

using System;
using System.IO;

using JetBrains.Annotations;

using LughSharp.Core;
using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.Cameras;
using LughSharp.Core.Graphics.Fonts;
using LughSharp.Core.Graphics.Images;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Input;
using LughSharp.Core.Scene2D;
using LughSharp.Core.Scene2D.Listeners;
using LughSharp.Core.Scene2D.RegistryStyles;
using LughSharp.Core.Scene2D.UI;
using LughSharp.Core.Scene2D.UI.Styles;
using LughSharp.Core.Scene2D.Utils;
using LughSharp.Core.Utils;
using LughSharp.Core.Utils.Logging;

using TestProject.Source;

namespace TestProject.Source;

[PublicAPI]
public class StageTests : IDisposable
{
    public Stage? Stage { get; private set; }

    public Button?          Button           { get; private set; }
    public Button?          Button2          { get; private set; }
    public TextButton?      TextButton       { get; private set; }
    public TextButton?      TextButton2      { get; private set; }
    public ImageButton?     ImageButton      { get; private set; }
    public ImageButton?     ImageButton2     { get; private set; }
    public ImageTextButton? ImageTextButton  { get; private set; }
    public ImageTextButton? ImageTextButton2 { get; private set; }
    public CheckBox?        CheckBox         { get; private set; }
    public ProgressBar?     ProgressBar      { get; private set; }
    public Slider?          Slider           { get; private set; }
    public Label?           Label            { get; private set; }
    public Table?           Table            { get; private set; }

    // ========================================================================

    public void CreateStage( OrthographicGameCamera? hudCam, ref InputMultiplexer inputMultiplexer )
    {
        if ( hudCam == null )
        {
            throw new InvalidOperationException( "STAGE CAMERA must be created before creating the stage!" );
        }

        Stage = new Stage( hudCam.Viewport )
        {
            DebugAll = true,
        };

        if ( Stage != null )
        {
            inputMultiplexer.AddProcessor( Stage );

            CreateSkinActors();
            CreateStyleRegistryActors();
        }
        else
        {
            Logger.Debug( "Stage is null!" );
        }
    }

    public void Update( Stage stage, bool isDrawingStage )
    {
        if ( Stage != null && isDrawingStage )
        {
            Stage.Act( Math.Min( Engine.DeltaTime, 1.0f / 60.0f ) );
            Stage.Draw();
        }
    }

    public void CreateSkinActors()
    {
        var imageActor           = false;
        var windowActor          = false;
        var buttonActor          = false;
        var textButtonActor      = false;
        var imageButtonActor     = false;
        var imageTextButtonActor = false;
        var checkBoxActor        = false;
        var progressBarActor     = false;
        var sliderActor          = false;
        var labelActor           = false;
        var tableActor           = false;
        var dialogActor          = true;

        var skin = new Skin( new FileInfo( Assets.UiSkin ) );

        if ( imageActor )
        {
            // Working
            // Draws an image correctly.

            var scene2DImage = new Scene2DImage( new Texture2D( Assets.HudPanel ) )
            {
                IsVisible = true
            };
            scene2DImage.SetPosition( 0, 0 );
            Stage?.AddActor( scene2DImage );
        }

        // --------------------------------------

        if ( windowActor )
        {
            // Draws a window correctly. Displays title, background.
            // Window content needs testing.

            var windowStyle = new WindowStyle
            {
                TitleFont      = new BitmapFont( new FileInfo( Assets.ArialFont ) ),
                TitleFontColor = Color.Red,
                Background     = new TextureRegionDrawable( new Texture2D( Assets.WindowBackground ) )
            };
            var window = new Window( "Window Title", windowStyle )
            {
                IsVisible = true,
            };
            window.SetPosition( 200, 200 );
            window.AddActor( new Label( "Window Content", skin ) );
            Stage?.AddActor( window );
        }

        // --------------------------------------

        if ( buttonActor )
        {
            // Working
            // Button actor draws, detects click, release, hover. Can be drawn with custom styles,
            // or default styles from skins.

            var btStyle = new ButtonStyle
            {
                Up      = new TextureRegionDrawable( new Texture2D( Assets.ButtonBUp ) ),
                Down    = new TextureRegionDrawable( new Texture2D( Assets.ButtonBDown ) ),
                Over    = new TextureRegionDrawable( new Texture2D( Assets.ButtonBOver ) ),
                Checked = new TextureRegionDrawable( new Texture2D( Assets.ButtonBChecked ) ),
            };
            Button = new Button( btStyle )
            {
                IsVisible = true,
            };
            Button.SetPosition( 100, 250 );
            Stage?.AddActor( Button );

            Button.AddListener( new ChangeListener( ( ev, actor ) =>
            {
//                var source  = ev.TargetActor; // Actor that fired the event
//                var stage   = ev.Stage;       // The Stage it belongs to
//                var capture = ev.Capture;     // true if in capture phase

                if ( actor is Button { IsChecked: true } )
                {
                    Logger.Debug( "Button is checked!" );
                }

                if ( actor is Button { IsPressed: true } )
                {
                    Logger.Debug( "Button is pressed!" );
                }
                
                ev.SetHandled();
            } ) );
            
            Button2 = new Button( skin, "default" )
            {
                IsVisible = true,
            };
            Button2.SetPosition( 100, 70 );
            Button2.SetSize( Button.Width, Button.Height );
            Stage?.AddActor( Button2 );

            Button2.AddListener( new ClickListener( ( ev, x, y ) =>
            {
                Logger.Debug( "Button2 clicked!" );
                
                ev.SetHandled();
            } ) );
        }

        // --------------------------------------

        if ( textButtonActor )
        {
            // Button actor draws, detects click, release, hover. Can be drawn with custom styles,
            // or default styles from skins.
            // Label text is not being aligned correctly, need to fix. Text is being drawn at the
            // TextButton 0,0 position.

            var tbStyle = new TextButtonStyle
            {
                Up                = new TextureRegionDrawable( new Texture2D( Assets.ButtonBUp ) ),
                Down              = new TextureRegionDrawable( new Texture2D( Assets.ButtonBDown ) ),
                Disabled          = new TextureRegionDrawable( new Texture2D( Assets.ButtonBDisabled ) ),
                Over              = new TextureRegionDrawable( new Texture2D( Assets.ButtonBOver ) ),
                Checked           = new TextureRegionDrawable( new Texture2D( Assets.ButtonBChecked ) ),
                Font              = new BitmapFont( new FileInfo( Assets.ArialFont ) ),
                FontColor         = Color.Yellow,
                DisabledFontColor = Color.Gray,
            };

            var textButton = new TextButton( "Text Button", tbStyle )
            {
                IsVisible = true,
            };
            textButton.SetPosition( 300, 250 );
            Stage?.AddActor( textButton );

            var textButton2 = new TextButton( "Text Button", skin )
            {
                IsVisible = true,
            };
            textButton2.SetPosition( 300, 70 );
            textButton2.SetSize( textButton.Width, textButton.Height );
            Stage?.AddActor( textButton2 );
        }

        // --------------------------------------

        if ( imageButtonActor )
        {
            // Button actor draws, detects click, release, hover. Can be drawn with custom styles,
            // or default styles from skins.

            var imageButtonStyle = new ImageButtonStyle
            {
                Up       = new TextureRegionDrawable( new Texture2D( Assets.Icon11112X112 ) ),
                Down     = new TextureRegionDrawable( new Texture2D( Assets.ButtonBDown ) ),
                Disabled = new TextureRegionDrawable( new Texture2D( Assets.ButtonBDisabled ) ),
                Over     = new TextureRegionDrawable( new Texture2D( Assets.ButtonBOver ) ),
                Checked  = new TextureRegionDrawable( new Texture2D( Assets.ButtonBChecked ) ),
            };

            var imageButton = new ImageButton( imageButtonStyle )
            {
                IsVisible  = true,
                IsDisabled = false,
            };
            imageButton.SetPosition( 500, 250 );
            Stage?.AddActor( imageButton );

            var imageButton2 = new ImageButton( skin, "default" )
            {
                IsVisible = true,
            };
            imageButton2.SetPosition( 500, 70 );
            imageButton2.SetSize( imageButton.Width, imageButton.Height );
            Stage?.AddActor( imageButton2 );
        }

        // --------------------------------------

        if ( imageTextButtonActor )
        {
            // Button actor draws, detects click, release, hover. Can be drawn with custom styles,
            // or default styles from skins.

            var imageTextButtonStyle = new ImageTextButtonStyle
            {
                Up       = new TextureRegionDrawable( new Texture2D( Assets.ButtonBUp ) ),
                Down     = new TextureRegionDrawable( new Texture2D( Assets.ButtonBDown ) ),
                Disabled = new TextureRegionDrawable( new Texture2D( Assets.ButtonBDisabled ) ),
                Over     = new TextureRegionDrawable( new Texture2D( Assets.ButtonBOver ) ),
                Checked  = new TextureRegionDrawable( new Texture2D( Assets.ButtonBChecked ) ),
            };

            var imageTextButton = new ImageTextButton( "Image Text Button", imageTextButtonStyle )
            {
                IsVisible = true,
            };
            imageTextButton.SetPosition( 700, 250 );
            Stage?.AddActor( imageTextButton );

            var imageTextButton2 = new ImageTextButton( "Image Text Button", skin, "default" )
            {
                IsVisible = true,
            };
            imageTextButton2.SetPosition( 700, 70 );
            imageTextButton2.SetSize( imageTextButton.Width, imageTextButton.Height );
            Stage?.AddActor( imageTextButton2 );
        }

        // --------------------------------------

        if ( checkBoxActor )
        {
            var checkBox = new CheckBox( "checkbox", skin )
            {
                IsVisible = true,
            };

            checkBox.Label?.Style.Background = new TextureRegionDrawable( new Texture2D( Assets.Bar9 ) );
            checkBox.SetPosition( 200, 200 );
            Stage?.AddActor( checkBox );
        }

        // --------------------------------------

        if ( progressBarActor )
        {
            var progressBar = new ProgressBar( 0f, 10f, 1f, false, skin )
            {
                IsVisible = true,
            };
            progressBar.SetPosition( 200, 200 );
            progressBar.Width  = 400;
            progressBar.Height = 100;
            Stage?.AddActor( progressBar );
        }

        // --------------------------------------

        if ( sliderActor )
        {
            var slider = new Slider( 0f, 10f, 1f, false, skin )
            {
                IsVisible = true,
            };
            slider.SetPosition( 200, 200 );
            slider.Width  = 400;
            slider.Height = 100;
            Stage?.AddActor( slider );
        }

        // --------------------------------------

        if ( labelActor )
        {
            var label = new Label( "Hello, World!", skin )
            {
                IsVisible = true,
            };
            label.SetPosition( 200, 400 );
            label.SetAlignment( Align.Top | Align.Left );
            Stage?.AddActor( label );
        }

        // --------------------------------------

        if ( tableActor )
        {
        }

        // --------------------------------------

        if ( dialogActor )
        {
            var dialog = new Dialog( "Dialog Title", skin )
            {
                IsVisible = true,
            };
            dialog.Text( "This is a dialog box with a custom title and text." );
            dialog.Button( "OK", true ).Button( "Cancel", false );
            dialog.Key( IInput.Keys.Enter, true ).Key( IInput.Keys.Escape, false );
            dialog.SetPosition( 200, 200 );
            dialog.SetSize( 400, 240 );
            Stage?.AddActor( dialog );
        }
    }

    public void CreateStyleRegistryActors()
    {
        const bool HudActor             = false;
        const bool WindowActor          = false;
        const bool ButtonActor          = false;
        const bool TextButtonActor      = false;
        const bool ImageButtonActor     = false;
        const bool ImageTextButtonActor = false;
        const bool CheckBoxActor        = false;
        const bool ProgressBarActor     = false;

        var styleRegistry = new StyleRegistry();
        styleRegistry.CreateStyleDefaults( new TextureAtlas( new FileInfo( Assets.UiskinAtlas ) ) );
        styleRegistry.DebugRegistry();

        if ( HudActor )
        {
        }

        if ( WindowActor )
        {
        }

        if ( ButtonActor )
        {
            var buttonStyle = styleRegistry.Get< ButtonStyle >( "default" );
            var button = new Button( buttonStyle )
            {
                IsVisible = true,
            };
            button.SetPosition( 300, 300 );
            Stage?.AddActor( button );
        }

        if ( TextButtonActor )
        {
            var textButtonStyle = styleRegistry.Get< TextButtonStyle >( "default" );
            var textButton = new TextButton( "Text Button", textButtonStyle )
            {
                IsVisible = true,
            };
            Stage?.AddActor( textButton );
        }

        if ( ImageButtonActor )
        {
            var imageButtonStyle = styleRegistry.Get< ImageButtonStyle >( "default" );

            imageButtonStyle.ImageUp       = new TextureRegionDrawable( new Texture2D( Assets.ButtonBUp ) );
            imageButtonStyle.ImageDown     = new TextureRegionDrawable( new Texture2D( Assets.ButtonBDown ) );
            imageButtonStyle.ImageDisabled = new TextureRegionDrawable( new Texture2D( Assets.ButtonBDown ) );

            var imageButton = new ImageButton( imageButtonStyle )
            {
                IsVisible = true,
            };
            Stage?.AddActor( imageButton );
        }

        if ( ImageTextButtonActor )
        {
        }

        if ( CheckBoxActor )
        {
        }

        if ( ProgressBarActor )
        {
            ProgressBarStyle progressBarStyle = styleRegistry.Get< ProgressBarStyle >( "default" );
            var progressBar = new ProgressBar( 0f, 10f, 1f, false, progressBarStyle )
            {
                IsVisible = true,
            };
            Stage?.AddActor( progressBar );
        }
    }

    public void Dispose()
    {
        Stage?.Dispose();
        GC.SuppressFinalize( this );
    }
}

// ============================================================================
// ============================================================================