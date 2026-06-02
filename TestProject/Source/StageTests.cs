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

#pragma warning disable CS8321 // Local function is declared but never used

namespace TestProject.Source;

[PublicAPI]
public class StageTests : IDisposable
{
    public Button?              Button           { get; private set; }
    public Button?              Button2          { get; private set; }
    public TextButton?          TextButton       { get; private set; }
    public TextButton?          TextButton2      { get; private set; }
    public ImageButton?         ImageButton      { get; private set; }
    public ImageButton?         ImageButton2     { get; private set; }
    public ImageTextButton?     ImageTextButton  { get; private set; }
    public ImageTextButton?     ImageTextButton2 { get; private set; }
    public CheckBox?            CheckBox         { get; private set; }
    public ProgressBar?         ProgressBar      { get; private set; }
    public ScrollPane?          ScrollPane       { get; private set; }
    public TextField?           TextField        { get; private set; }
    public TextArea?            TextArea         { get; private set; }
    public SelectBox< string >? SelectBox        { get; private set; }
    public Slider?              Slider           { get; private set; }
    public Label?               Label            { get; private set; }
    public Table?               Table            { get; private set; }

    // ========================================================================

    private Stage? _stage;

    private static readonly string[] _newItems = new[]
    {
        "Mirror Carp",
        "Leather Carp",
        "Common Carp",
        "Grass Carp",
        "Crucian Carp",
        "F1",
        "Roach",
        "Pike",
        "Bream",
        "Rudd",
        "Tench",
        "Gudgeon",
        "Perch",
        "Zander",
        "Ruffe",
        "Minnow",
        "Grayling",
        "Brown Trout",
        "Rainbow Trout",
        "Salmon",
        "Chub",
        "Barbel",
        "Silver Bream",
    };

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
            // Add the Stage to the input multiplexer so that it can receive input.
            inputMultiplexer.AddProcessor( _stage );

            // These 2 calls are for testing actors only. They are not needed for
            // Stage creation.
            CreateSkinActors();
            CreateStyleRegistryActors();
        }
        else
        {
            Logger.Debug( "_stage is null!" );
        }
    }

    public void Update( float deltaTime )
    {
        if ( Engine.Input.IsKeyJustPressed( IInput.Keys.A ) )
        {
            SelectBox?.ShowScrollPane();
        }

        if ( Engine.Input.IsKeyJustPressed( IInput.Keys.D ) )
        {
            SelectBox?.HideScrollPane();
        }
    }

    public void Render( bool isDrawingStage )
    {
    }

    public void CreateSkinActors()
    {
        Guard.Against.Null( _stage );

        Table  table;
        Window window;

        var titleBackgroundDrawable = new TextureRegionDrawable( new Texture2D( Assets.TitleBackground ) );
        var backgroundDrawable      = new TextureRegionDrawable( new Texture2D( Assets.Background ) );
        var bar9Drawable            = new TextureRegionDrawable( new Texture2D( Assets.Bar9 ) );
        var skin                    = new Skin( new FileInfo( Assets.UiSkin ) );

        // Lay out the table for the UI widgets etc.
//        TableActor();
        WindowActor();
        // ----------------------------
//        ImageActor();
        SelectBoxActor();
        // ----------------------------
//        ButtonActor();
//        TextButtonActor();
//        ImageButtonActor();
//        ImageTextButtonActor();
//        CheckBoxActor();
//        ProgressBarActor();
//        SliderActor();
//        ScrollPaneActor(); // Also tests ListBox<>
//        TextFieldActor();
//        TextAreaActor();
//        LabelActor();
//        DialogActor();
//        SplitPaneActor();

        // ----------------------------

        return;

        // ====================================================================

        void TableActor()
        {
            table = new Table( skin )
            {
                IsVisible  = true,
                FillParent = true
            };

            table.SetBackground( titleBackgroundDrawable );
            table.SetDebug( true );

            _stage?.AddActor( table );
        }

        // --------------------------------------

        void WindowActor()
        {
            window = new Window( "Title", skin )
            {
                IsVisible = true,
            };

            window.TitleTable?.AddCell( new TextButton( "X", skin ) ).Height( window.GetPadTop() );
            window.SetPosition( 200, 200 );
//            window.SetBackground( titleBackgroundDrawable );
            window.CellDefaults.SetSpaceBottom( 10 );
            window.AddRow()?.Fill().Expand();
            window.Pack();

            _stage?.AddActor( window );
        }

        // --------------------------------------

        void SelectBoxActor()
        {
            SelectBox = new SelectBox< string >( skin )
            {
                IsVisible  = true,
                IsDisabled = false,
            };

            SelectBox.SetAlignment( Align.Center );
            SelectBox.GetList().Alignment = Align.Center;

            SelectBox.GetStyle().ScrollPaneStyle.Background = titleBackgroundDrawable;
            SelectBox.GetStyle().ListBoxStyle.Background    = bar9Drawable;

            SelectBox.GetStyle().ListBoxStyle.Selection.RightWidth   = 10;
            SelectBox.GetStyle().ListBoxStyle.Selection.LeftWidth    = 20;
            SelectBox.GetStyle().ListBoxStyle.Selection.TopHeight    = 5;
            SelectBox.GetStyle().ListBoxStyle.Selection.BottomHeight = 5;

            SelectBox.SetItems( new List< string >( _newItems ) );
            SelectBox.SetSelected( "Silver Bream" );

            window.AddCell( SelectBox ).SetMaxWidth( 100 );
        }

        // --------------------------------------

        void SplitPaneActor()
        {
        }

        // --------------------------------------

        void DialogActor()
        {
            Guard.Against.Null( _stage );

            var dialogStyle = new DialogStyle
            {
                Background     = new TextureRegionDrawable( new Texture2D( Assets.Bar9 ) ),
                TitleFont      = new BitmapFont( new FileInfo( Assets.ArialFont ) ),
                TitleFontColor = Color.Red,
            };

            var dialog = new Dialog( "Dialog Title", dialogStyle, skin )
            {
                IsVisible = true,
            };

            dialog.Text( "This is a dialog box with a custom title and text.",
                         skin.Get< LabelStyle >( "default" ) );

            dialog.Text( "Bla Bla Bla Bla Bla Bla.", skin.Get< LabelStyle >( "default" ) );

            dialog.Button( "OK", true, skin.Get< TextButtonStyle >( "default" ) )
                  .Button( "Cancel", false, skin.Get< TextButtonStyle >( "default" ) );

            dialog.Key( IInput.Keys.Enter, true )
                  .Key( IInput.Keys.Escape, false );

            dialog.Show( _stage, null )
                  .SetPosition( ( float )Math.Round( ( _stage.Width - dialog.GetWidth() ) / 2 ),
                                ( float )Math.Round( ( _stage.Height - dialog.GetHeight() ) / 2 ) );
        }

        //@formatter:off
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
            TextAreaActor();

            skin.GetFont( "default-font" ).SetFixedWidthGlyphs( "0123456789" );

            var textField = new TextField( string.Empty, skin, "default" )
            {
                IsVisible = true,
            };
            textField.SetPosition( 200, 200 );
            textField.SetWidth( 100 );
            textField.SetHeight( 30 );

            _stage?.AddActor( textField );
            
            Engine.Input.SetOverrideKey( IInput.Keys.Tab, true );
        }

        // --------------------------------------

        void TextAreaActor()
        {
            var textArea = new TextArea( "Text Area\n1111111111\n0123456789\nEssentially, "
                                       + "a text field\nwith\nmultiple\nlines.\n"
                                       + "It can even handle very looooooooooooooooooooooo"
                                       + "oooooooooooooooooooooooooooooooooooooooooooooooo"
                                       + "ooooooooooooooooooooong lines.",
                                         skin )
            {
                IsVisible = true,
            };
            textArea.SetPosition( 200, 200 );
            textArea.SetWidth( 200 );
            textArea.SetHeight( 200 );

            _stage?.AddActor( textArea );
        }

        // --------------------------------------

        void LabelActor()
        {
            var label = new Label( "AbCdEfGhIjKlMnOpQrStUvWxYz", skin, "default-bg" )
            {
                IsVisible = true,
                Wrap      = true,
            };

            label.SetSize( 400, 80 );
            label.SetAlignment( Align.Top | Align.Right );

            Table t = new Table( skin );
            t.SetPosition( 200, 400 );
            t.AddRow();
            t.AddActor( label );
            t.Layout();

            _stage?.AddActor( t );
        }

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
            progressBar.SetWidth(400);
            progressBar.SetHeight(100);
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
            slider.SetWidth( 400 );
            slider.SetHeight( 100 );
            _stage?.AddActor( slider );
        }

        // --------------------------------------

        // Working
        // Draws an image correctly.
        void ImageActor()
        {
            var scene2DImage = new Scene2DImage( new Texture2D( Assets.Boulder64X64 ) )
            {
                IsVisible = true
            };
//            table.AddCell( scene2DImage );
            window.AddCell( scene2DImage );
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
                Actor? source  = ev.TargetActor; // Actor that fired the event
                Stage? stage   = ev.Stage;       // The _stage it belongs to
                bool   capture = ev.Capture;     // true if in capture phase

                Logger.Debug( $"Button clicked! Actor: {source}, Stage: {stage}, Capture: {capture}" );
                
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
            textButton2.SetSize( textButton.GetWidth(), textButton.GetHeight() );
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
            imageButton2.SetSize( imageButton.GetWidth(), imageButton.GetHeight() );
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
            imageTextButton2.SetSize( imageTextButton.GetWidth(), imageTextButton.GetHeight() );
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

            checkBox.Label?.GetStyle().Background = new TextureRegionDrawable( new Texture2D( Assets.Bar9 ) );
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