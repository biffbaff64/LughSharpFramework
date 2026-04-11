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
using LughSharp.Core.SceneGraph2D;
using LughSharp.Core.SceneGraph2D.RegistryStyles;
using LughSharp.Core.SceneGraph2D.UI;
using LughSharp.Core.SceneGraph2D.UI.Styles;
using LughSharp.Core.SceneGraph2D.Utils;
using LughSharp.Core.Utils.Logging;

namespace TestProject.Source;

[PublicAPI]
public class StageTests : IDisposable
{
    public Stage? Stage { get; private set; }

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
        const bool ImageActor           = false;
        const bool WindowActor          = false;
        const bool ButtonActor          = false;
        const bool TextButtonActor      = false;
        const bool ImageButtonActor     = false;
        const bool ImageTextButtonActor = false;
        const bool CheckBoxActor        = true;
        const bool ProgressBarActor     = false;
        const bool SliderActor          = false;
        const bool LabelActor           = false;

        var skin = new Skin( new FileInfo( Assets.UiSkin ) );

        if ( ImageActor )
        {
            var scene2DImage = new Scene2DImage( new Texture( Assets.ButtonRight ) )
            {
                IsVisible = true
            };
            scene2DImage.SetPosition( 200, 200 );
            Stage?.AddActor( scene2DImage );
        }

        // --------------------------------------

        if ( WindowActor )
        {
//            var windowStyle = new WindowStyle
//            {
//                TitleFont      = _font,
//                TitleFontColor = Color.White,
//                Background     = new TextureRegionDrawable( new Texture( Assets.WindowBackground ) )
//            };
//            var windowActor = new Window( "Window Title", windowStyle )
            var windowActor = new Window( "Window Title", skin, "default" )
            {
                IsVisible = true,
            };
            windowActor.SetPosition( 200, 200 );
            Stage?.AddActor( windowActor );
        }

        // --------------------------------------

        if ( ButtonActor )
        {
            var btStyle = new ButtonStyle
            {
                Up      = new TextureRegionDrawable( new Texture( Assets.ButtonBUp ) ),
                Down    = new TextureRegionDrawable( new Texture( Assets.ButtonBDown ) ),
                Over    = new TextureRegionDrawable( new Texture( Assets.ButtonBOver ) ),
                Checked = new TextureRegionDrawable( new Texture( Assets.ButtonBChecked ) ),
            };
            var button = new Button( btStyle )
            {
                IsVisible = true,
            };
            button.SetPosition( 200, 200 );
            Stage?.AddActor( button );
        }

        // --------------------------------------

        if ( TextButtonActor )
        {
            var tbStyle = new TextButtonStyle
            {
                Up                = new TextureRegionDrawable( skin.GetRegion( "default-round" ) ),
                Down              = new TextureRegionDrawable( skin.GetRegion( "default-round-down" ) ),
                Disabled          = new TextureRegionDrawable( skin.GetRegion( "default-round" ) ),
                Font              = new BitmapFont( new FileInfo( Assets.ArialFont ) ),
                FontColor         = Color.Yellow,
                DisabledFontColor = Color.Gray,
            };

//            var textButton = new TextButton( "Text Button", tbStyle )
            var textButton = new TextButton( "Text Button", skin )
            {
                IsVisible = true,
            };
            textButton.SetPosition( 200, 200 );
            Stage?.AddActor( textButton );
        }

        // --------------------------------------

        if ( ImageButtonActor )
        {
            var imageButtonStyle = new ImageButtonStyle
            {
                ImageUp       = new TextureRegionDrawable( new Texture( Assets.ButtonBUp ) ),
                ImageDown     = new TextureRegionDrawable( new Texture( Assets.ButtonBDown ) ),
                ImageDisabled = new TextureRegionDrawable( new Texture( Assets.ButtonBDown ) ),
            };

            var imageButton = new ImageButton( imageButtonStyle )
            {
                IsVisible  = true,
                IsDisabled = false,
            };

            imageButton.SetPosition( 200, 200 );
            imageButton.SetSize( 100, 100 );
            Stage?.AddActor( imageButton );

            var imageButton2 = new ImageButton( skin, "default" );
            imageButton2.SetSize( 100, 100 );
            imageButton2.SetPosition( 600, 20 );
            Stage.AddActor( imageButton2 );
        }

        // --------------------------------------

        if ( ImageTextButtonActor )
        {
            var imageTextButton = new ImageTextButton( "Image Text Button", skin, "default" )
            {
                IsVisible = true,
            };
            imageTextButton.SetPosition( 200, 200 );
            Stage?.AddActor( imageTextButton );
        }

        // --------------------------------------

        if ( CheckBoxActor )
        {
            var checkBox = new CheckBox( "checkbox", skin )
            {
                IsVisible = true,
            };

            checkBox.Label?.Style.Background = new TextureRegionDrawable( new Texture( Assets.Bar9 ) );
            checkBox.SetPosition( 200, 200 );
            Stage?.AddActor( checkBox );
        }

        // --------------------------------------

        if ( ProgressBarActor )
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

        if ( SliderActor )
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

        if ( LabelActor )
        {
            var label = new Label( "Hello, World!", skin )
            {
                IsVisible = true,
            };
            label.SetPosition( 200, 400 );
            Stage?.AddActor( label );
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

            imageButtonStyle.ImageUp       = new TextureRegionDrawable( new Texture( Assets.ButtonBUp ) );
            imageButtonStyle.ImageDown     = new TextureRegionDrawable( new Texture( Assets.ButtonBDown ) );
            imageButtonStyle.ImageDisabled = new TextureRegionDrawable( new Texture( Assets.ButtonBDown ) );

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