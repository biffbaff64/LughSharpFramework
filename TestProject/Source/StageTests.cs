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

using JetBrains.Annotations;

using LughSharp.Source;
using LughSharp.Source.Graphics;
using LughSharp.Source.Graphics.Cameras;
using LughSharp.Source.Graphics.Fonts;
using LughSharp.Source.Graphics.Images;
using LughSharp.Source.Input;
using LughSharp.Source.Scene2D;
using LughSharp.Source.Scene2D.Listeners;
using LughSharp.Source.Scene2D.UI;
using LughSharp.Source.Scene2D.UI.Styles;
using LughSharp.Source.Scene2D.Utils;
using LughSharp.Source.Utils;
using LughSharp.Source.Utils.Exceptions;
using LughSharp.Source.Utils.Logging;

namespace TestProject.Source;

[PublicAPI]
public class StageTests : IDisposable
{
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

    private Stage? _stage;
    
    // ========================================================================

    public void CreateStage( OrthographicGameCamera? hudCam,
                             ref Stage? stage,
                             ref InputMultiplexer inputMultiplexer )
    {
        if ( hudCam == null )
        {
            throw new InvalidOperationException( "STAGE CAMERA must be created before creating the stage!" );
        }

        _stage = stage;
        
        if ( _stage != null )
        {
            inputMultiplexer.AddProcessor( _stage );

            CreateSkinActors();
            CreateStyleRegistryActors();
        }
        else
        {
            Logger.Debug( "_stage is null!" );
        }
    }

    public void Update( bool isDrawingStage )
    {
        if ( _stage != null && isDrawingStage )
        {
            _stage.Act( Math.Min( Engine.DeltaTime, 1.0f / 60.0f ) );
            _stage.Draw();
        }
    }

    public void CreateSkinActors()
    {
        Guard.Against.Null( _stage );

        var skin = new Skin( new FileInfo( Assets.UiSkin ) );

//        ImageActor();
//        ButtonActor();
//        TextButtonActor();
//        ImageButtonActor();
//        ImageTextButtonActor();
//        CheckBoxActor();
//        ProgressBarActor();
//        SliderActor();
//        ScrollPaneActor(); // Also tests ListBox<>
        // ----------------------------
//        LabelActor();
//        TableActor();
//        WindowActor();
//        DialogActor();
//        TextFieldActor();
//        TextAreaActor();
//        SplitPaneActor();
//        SelectBoxActor();

        return;

        // ====================================================================

        void WindowActor()
        {
//            var windowStyle = new WindowStyle
//            {
//                TitleFont      = new BitmapFont( new FileInfo( Assets.ArialFont ) ),
//                TitleFontColor = Color.Red,
//                Background     = new TextureRegionDrawable( new Texture2D( Assets.WindowBackground ) )
//            };
            var window = new Window( "Window Title", skin )
            {
                IsVisible = true,
            };
//            window.TitleTable?.AddCell( new TextButton( "X", skin ) ).Height( window.GetPadTop() );
            window.SetPosition( 200, 200 );
            window.CellDefaults.SetSpaceBottom( 10 );
            window.AddRow().Fill().Expand();
            window.AddCell( new Label( "Window Content", skin ) );
            window.AddRow();
            window.AddCell( new Button( skin.Get< ButtonStyle >( "default" ) ) );
            window.AddRow();
            window.AddCell( new Scene2DImage( new Texture2D( Assets.Boulder32X32 ) ) );
            window.Pack();

            _stage?.AddActor( window );
        }

        // --------------------------------------

        void ScrollPaneActor()
        {
            string[] listEntries =
            {
                "A a A a A a A a A a",
                "B b B b B b B b B b",
                "C c C c C c C c C c",
                "D d D d D d D d D d",
                "E e E e E e E e E e",
                "F f F f F f F f F f",
                "G g G g G g G g G g",
                "H h H h H h H h H h",
                "I i I i I i I i I i",
                "J j J j J j J j J j",
                "K k K k K k K k K k",
                "L l L l L l L l L l",
                "M m M m M m M m M m",
                "N n N n N n N n N n",
                "O o O o O o O o O o",
                "P p P p P p P p P p",
                "Q q Q q Q q Q q Q q",
                "R r R r R r R r R r",
                "S s S s S s S s S s",
                "T t T t T t T t T t",
                "U u U u U u U u U u",
                "V v V v V v V v V v",
                "W w W w W w W w W w",
                "X x X x X x X x X x",
                "Y y Y y Y y Y y Y y",
                "Z z Z z Z z Z z Z z",
            };

            var list = new ListBox< string >( skin )
            {
                Selection =
                {
                    Multiple = true,
                    Required = false
                },
                TypeToSelect = true,
                Alignment    = Align.Center
            };
            list.SetItems( new List< string >( listEntries ) );

            var scrollPane = new ScrollPane( list, skin )
            {
                IsVisible = true,
            };

            scrollPane.SetPosition( 200, 200 );
            scrollPane.SetScrollingDisabled( false, false );
            scrollPane.SetFadeScrollBars( false );

            _stage?.AddActor( scrollPane );
            _stage?.SetKeyboardFocus( list );
        }

        // --------------------------------------

        void TextFieldActor()
        {
        }

        // --------------------------------------

        void TextAreaActor()
        {
        }

        // --------------------------------------

        void SplitPaneActor()
        {
        }

        // --------------------------------------

        void SelectBoxActor()
        {
        }

        // --------------------------------------

        void LabelActor()
        {
            var label = new Label( "AbCdEfGhIjKlMnOpQrStUvWxYz", skin )
            {
                IsVisible = true,
                Wrap      = true,
            };

//            label.SetPosition( 200, 400 );
//            label.SetSize( 80, 80 );
            label.SetAlignment( Align.Top | Align.Left );

            Table t = new Table( skin );
            t.SetPosition( 200, 400 );
            t.AddRow();
            t.AddActor( label );
            t.Layout();

            _stage?.AddActor( t );
        }

        // --------------------------------------

        void TableActor()
        {
            var table = new Table( skin )
            {
                IsVisible = true,
            };

            table.EnableDebug();
            table.SetBackground( "default-pane" );
            table.SetColor( 1, 1, 1, 1 );
            table.SetPosition( 200, 300 );
            table.SetSize( 400, 200 );

            table.AddCell( new Label( "IIIIIIIII", skin ) );
            table.AddRow();
            table.AddCell( new Label( "---------", skin ) );
            table.AddRow();
            
            _stage?.AddActor( table );
        }

        // --------------------------------------

        void DialogActor()
        {
            var dialogStyle = new DialogStyle()
            {
                Background     = new TextureRegionDrawable( new Texture2D( Assets.Bar9 ) ),
                TitleFont      = new BitmapFont( new FileInfo( Assets.ArialFont ) ),
                TitleFontColor = Color.Red,
            };

            var dialog = new Dialog( "Dialog Title", dialogStyle )
            {
                IsVisible = true,
            };

            dialog.Text( "This is a dialog box with a custom title and text.",
                         skin.Get< LabelStyle >( "default" ) );

            dialog.Button( "OK", true, skin.Get< TextButtonStyle >( "default" ) )
                  .Button( "Cancel", false, skin.Get< TextButtonStyle >( "default" ) );

            dialog.Key( IInput.Keys.Enter, true )
                  .Key( IInput.Keys.Escape, false );

//            dialog.Show( _stage );
            _stage?.AddActor( dialog );
        }

        //@formatter:off
        // --------------------------------------

        // Working. Draws a progress bar correctly. The knob is programmatically
        // moveable as expected.
        void ProgressBarActor()
        {
            var progressBar = new ProgressBar( 0f, 10f, 1f, false, skin )
            {
                IsVisible = true,
            };
            progressBar.SetPosition( 200, 200 );
            progressBar.Width  = 400;
            progressBar.Height = 100;
            _stage?.AddActor( progressBar );
        }

        // --------------------------------------

        // Working. Draws a slider correctly. User is able to select the Knob
        // and move it up and down the slider bar.
        void SliderActor()
        {
            var slider = new Slider( 0f, 10f, 1f, false, skin )
            {
                IsVisible = true,
            };
            slider.SetPosition( 200, 400 );
            slider.Width  = 400;
            slider.Height = 100;
            _stage?.AddActor( slider );
        }

        // --------------------------------------

        // Working
        // Draws an image correctly.
        void ImageActor()
        {
            var scene2DImage = new Scene2DImage( new Texture2D( Assets.HudPanel ) )
            {
                IsVisible = true
            };
            scene2DImage.SetPosition( 0, 0 );
            _stage?.AddActor( scene2DImage );
        }

        // --------------------------------------

        // Button actor draws, detects click, release, hover. Can be drawn with
        // custom styles, or default styles from skins.
        void ButtonActor()
        {
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
            _stage?.AddActor( Button );

            Button.AddListener( new ChangeListener( ( ev, actor ) =>
            {
//                var source  = ev.TargetActor; // Actor that fired the event
//                var stage   = ev._stage;       // The _stage it belongs to
//                var capture = ev.Capture;     // true if in capture phase

                if ( actor is Button { IsChecked: true } )
                {
                    Logger.Debug( "Button is checked!" );
                }

                if ( actor is Button { IsPressed: true } )
                {
                    Logger.Debug( "Button is pressed!" );
                }
            } ) );

            Button.AddListener( new TextTooltip( "This is a tool tip!", skin ) );

//            var tooltipTable = new Table( skin );
//            tooltipTable.Pad( 10 ).SetBackground( "default-pane" );
//            tooltipTable.AddActor( new TextButton( "Fancy Tooltip!", skin ) );

//            Button2 = new Button( skin )
//            {
//                IsVisible = true,
//            };
//            Button2.SetPosition( 100, 70 );
//            Button2.SetSize( Button.Width, Button.Height );
//            _stage?.AddActor( Button2 );
//
//            Button2.AddListener( new ClickListener( ( ev, x, y ) => { Logger.Debug( "Button2 clicked!" ); } ) );
        }

        // --------------------------------------

        // Button actor draws, detects click, release, hover. Can be drawn with
        // custom styles, or default styles from skins.
        // Text centres correctly in the middle of the button.
        void TextButtonActor()
        {
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
                OverFontColor     = Color.Red,
            };

            var textButton = new TextButton( "Text Button", tbStyle )
            {
                Touchable  = Touchable.Enabled,
                IsVisible  = true,
                IsDisabled = false,
            };
            textButton.SetPosition( 300, 250 );
            _stage?.AddActor( textButton );

            var textButton2 = new TextButton( "Text Button", skin )
            {
                IsVisible = true,
            };
            textButton2.SetPosition( 300, 70 );
            textButton2.SetSize( textButton.Width, textButton.Height );
            _stage?.AddActor( textButton2 );
        }

        // --------------------------------------

        // Button actor draws, detects click, release, hover. Can be drawn with
        // custom styles, or default styles from skins.
        // Text centres correctly in the middle of the button.
        void ImageButtonActor()
        {
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
            _stage?.AddActor( imageButton );

            var imageButton2 = new ImageButton( skin, "default" )
            {
                IsVisible = true,
            };
            imageButton2.SetPosition( 500, 70 );
            imageButton2.SetSize( imageButton.Width, imageButton.Height );
            _stage?.AddActor( imageButton2 );
        }

        // --------------------------------------

        // Button actor draws, detects click, release, hover. Can be drawn with
        // custom styles, or default styles from skins.
        // Text centres correctly in the middle of the button.
        void ImageTextButtonActor()
        {
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
            _stage?.AddActor( imageTextButton );

            var imageTextButton2 = new ImageTextButton( "Image Text Button", skin, "default" )
            {
                IsVisible = true,
            };
            imageTextButton2.SetPosition( 700, 70 );
            imageTextButton2.SetSize( imageTextButton.Width, imageTextButton.Height );
            _stage?.AddActor( imageTextButton2 );
        }

        // --------------------------------------

        // Draws, detects click, and changes toggle state correctly.
        // Text is displayed correctly.
        void CheckBoxActor()
        {
            var checkBoxStyle = new CheckBoxStyle()
            {
                CheckboxOn  = new TextureRegionDrawable( new Texture2D( Assets.ToggleOn ) ),
                CheckboxOff = new TextureRegionDrawable( new Texture2D( Assets.ToggleOff ) ),
                Font        = new BitmapFont( new FileInfo( Assets.ArialFont ) ),
                FontColor   = Color.Red,
            };

            var checkBox = new CheckBox( "checkbox", checkBoxStyle )
            {
                IsVisible = true,
            };

            checkBox.Label?.Style.Background = new TextureRegionDrawable( new Texture2D( Assets.Bar9 ) );
            checkBox.SetPosition( 200, 200 );
            _stage?.AddActor( checkBox );
        }

        // --------------------------------------
        //@formatter:on
    }

    public void CreateStyleRegistryActors()
    {
//        const bool HudActor             = false;
//        const bool WindowActor          = false;
//        const bool ButtonActor          = false;
//        const bool TextButtonActor      = false;
//        const bool ImageButtonActor     = false;
//        const bool ImageTextButtonActor = false;
//        const bool CheckBoxActor        = false;
//        const bool ProgressBarActor     = false;
//
//        var styleRegistry = new StyleRegistry();
//        styleRegistry.CreateStyleDefaults( new TextureAtlas( new FileInfo( Assets.UiskinAtlas ) ) );
//
//        if ( HudActor )
//        {
//        }
//
//        if ( WindowActor )
//        {
//        }
//
//        if ( ButtonActor )
//        {
//            var buttonStyle = styleRegistry.Get< ButtonStyle >( "default" );
//            var button = new Button( buttonStyle )
//            {
//                IsVisible = true,
//            };
//            button.SetPosition( 300, 300 );
//            _stage.AddActor( button );
//        }
//
//        if ( TextButtonActor )
//        {
//            var textButtonStyle = styleRegistry.Get< TextButtonStyle >( "default" );
//            var textButton = new TextButton( "Text Button", textButtonStyle )
//            {
//                IsVisible = true,
//            };
//            _stage.AddActor( textButton );
//        }
//
//        if ( ImageButtonActor )
//        {
//            var imageButtonStyle = styleRegistry.Get< ImageButtonStyle >( "default" );
//
//            imageButtonStyle.ImageUp       = new TextureRegionDrawable( new Texture2D( Assets.ButtonBUp ) );
//            imageButtonStyle.ImageDown     = new TextureRegionDrawable( new Texture2D( Assets.ButtonBDown ) );
//            imageButtonStyle.ImageDisabled = new TextureRegionDrawable( new Texture2D( Assets.ButtonBDown ) );
//
//            var imageButton = new ImageButton( imageButtonStyle )
//            {
//                IsVisible = true,
//            };
//            _stage.AddActor( imageButton );
//        }
//
//        if ( ImageTextButtonActor )
//        {
//        }
//
//        if ( CheckBoxActor )
//        {
//        }
//
//        if ( ProgressBarActor )
//        {
//            ProgressBarStyle progressBarStyle = styleRegistry.Get< ProgressBarStyle >( "default" );
//            var progressBar = new ProgressBar( 0f, 10f, 1f, false, progressBarStyle )
//            {
//                IsVisible = true,
//            };
//            _stage.AddActor( progressBar );
//        }
    }

    public void Dispose()
    {
        _stage?.Dispose();
        GC.SuppressFinalize( this );
    }
}

// ============================================================================
// ============================================================================