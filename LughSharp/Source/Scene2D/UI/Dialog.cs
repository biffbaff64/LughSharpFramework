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

using LughSharp.Source.Scene2D.Actions;
using LughSharp.Source.Scene2D.Listeners;
using LughSharp.Source.Scene2D.UI.Styles;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// Displays a dialog, which is a window with a title, a content table, and a button table.
/// Methods are provided to add a label to the content table and buttons to the button table,
/// but any widgets can be added. When a button is clicked, <see cref="ClickResult"/> is
/// called and the dialog is removed from the stage.
/// </summary>
[PublicAPI]
public class Dialog : Window, IStyleable< DialogStyle >
{
    public Actor? PreviousKeyboardFocus { get; set; }
    public Actor? PreviousScrollFocus   { get; set; }
    public Table? ContentTable          { get; private set; }
    public Table? ButtonTable           { get; private set; }
    public bool   CancelHide            { get; set; }

    public Dictionary< Actor, object? >? Values { get; set; } = new();

    // ========================================================================

    private readonly IgnoreTouchDown _ignoreTouchDown = new();

    private FocusListener _dialogFocusListener = null!;

    // ========================================================================

    /// <summary>
    /// Creates a new Dialog, using the supplied title and <see cref="Skin"/>
    /// </summary>
    /// <param name="title"> A string holding the dialog name to display. </param>
    /// <param name="skin"> The Skin holding the DialogStyle.</param>
    public Dialog( string title, Skin skin ) : base( title, skin.Get< DialogStyle >(), skin )
    {
        Initialise( skin );
    }

    /// <summary>
    /// Creates a new Dialog, using the supplied title, <see cref="Skin"/>, and <see cref="DialogStyle"/>.
    /// </summary>
    /// <param name="title"> A string holding the dialog name to display. </param>
    /// <param name="skin"> The Skin holding the DialogStyle.</param>
    /// <param name="dialogStyle"> The <see cref="DialogStyle"/> to use. </param>
    public Dialog( string title, Skin skin, string dialogStyle )
        : base( title, skin.Get< DialogStyle >( dialogStyle ), skin )
    {
        Initialise( skin );
    }

    /// <summary>
    /// Creates a new Dialog window, using the supplied name and <see cref="DialogStyle"/>.
    /// </summary>
    /// <param name="title"> A string holding the dialog name to display. </param>
    /// <param name="dialogStyle"> The <see cref="DialogStyle"/> to use. </param>
    /// <param name="skin"> The Skin holding the DialogStyle.</param>
    public Dialog( string title, DialogStyle dialogStyle, Skin skin ) : base( title, dialogStyle, skin )
    {
        Initialise( skin );
    }

    /// <summary>
    /// Initialises the basic elements of this dialog, including the necessary listeners.
    /// </summary>
    /// <param name="skin"> The Skin holding the DialogStyle.</param>
    private void Initialise( Skin skin )
    {
        Skin    = skin;
        IsModal = true;

        CellDefaults.Space( 6 );

        AddCell( ContentTable = new Table( Skin ) ).Grow();
        AddCell( ButtonTable  = new Table( Skin ) ).SetFillX();

        ContentTable.CellDefaults.Space( 6 );
        ButtonTable.CellDefaults.Space( 6 );

        _dialogFocusListener = new DialogFocusListener( this );

        ButtonTable.AddListener( new ButtonTableChangeListener( this ) );

        AddCaptureListener( _dialogFocusListener );
    }

    /// <summary>
    /// Sets the <see cref="Stage"/> on which this Dialog will act.
    /// </summary>
    /// <param name="stage"> The stage to set. </param>
    public override void SetStage( Stage? stage )
    {
        if ( stage == null )
        {
            AddListener( _dialogFocusListener );
        }
        else
        {
            RemoveListener( _dialogFocusListener );
        }

        base.SetStage( stage );
    }

    /// <summary>
    /// Adds a label to the content table. The dialog needs to have been constructed
    /// with a <see cref="Skin"/> to use this method. If it hasn't, an exception will
    /// be throw.
    /// </summary>
    /// <param name="text"> The text to display on the button. </param>
    /// <returns> This dialog, for chaining. </returns>
    public Dialog Text( string? text )
    {
        if ( Skin == null )
        {
            throw new LughRuntimeException( "This method may only be used if the dialog was constructed." );
        }

        return Text( text, Skin.Get< LabelStyle >() );
    }

    /// <summary>
    /// Adds a label to the content table.
    /// </summary>
    /// <param name="text"> The text to display on the button. </param>
    /// <param name="labelStyle"> The style to use for the label. </param>
    /// <returns> This dialog, for chaining. </returns>
    public Dialog Text( string? text, LabelStyle labelStyle )
    {
        return Text( new Label( text, labelStyle ) );
    }

    /// <summary>
    /// Adds the given Label to the content table
    /// </summary>
    /// <param name="label"> The label to add. </param>
    /// <returns> This dialog, for chaining. </returns>
    public Dialog Text( Label label )
    {
        ContentTable?.AddCell( label );

        return this;
    }

    /// <summary>
    /// Adds a text button to the button table. Null will be passed to <see cref="ClickResult(object)"/>
    /// if this button is clicked. The dialog must have been constructed with a skin to use this
    /// method.
    /// </summary>
    /// <param name="text"> The text to display on the button. </param>
    /// <param name="obj">
    /// The object that will be passed to <see cref="ClickResult"/>
    /// if this button is clicked. May be null.
    /// </param>
    /// <returns> This dialog, for chaining. </returns>
    public Dialog Button( string text, object? obj = null )
    {
        if ( Skin == null )
        {
            throw new LughRuntimeException( "This method may only be used if the "
                                      + "dialog was constructed with a Skin." );
        }

        return Button( text, obj, Skin.Get< TextButtonStyle >() );
    }

    /// <summary>
    /// Adds a text button to the button table.
    /// </summary>
    /// <param name="text"> The text to display on the button. </param>
    /// <param name="obj">
    /// The object that will be passed to <see cref="ClickResult"/>
    /// if this button is clicked. May be null.
    /// </param>
    /// <param name="buttonStyle"> The style to use for the button. </param>
    /// <returns> This dialog, for chaining. </returns>
    public Dialog Button( string text, object? obj, TextButtonStyle buttonStyle )
    {
        return Button( new TextButton( text, buttonStyle ), obj );
    }

    /// <summary>
    /// Adds the given button to the button table.
    /// </summary>
    /// <param name="button"></param>
    /// <param name="obj">
    /// The object that will be passed to <see cref="ClickResult"/> if this
    /// button is clicked. May be null.
    /// </param>
    /// <returns> This dialog, for chaining. </returns>
    public Dialog Button( Button button, object? obj = null )
    {
        ButtonTable?.AddCell( button );

        SetObject( button, obj! );

        return this;
    }

    /// <summary>
    /// Centers the dialog in the stage and calls <see cref="Show(Stage, SceneAction)"/>
    /// with a <see cref="SceneActions.FadeIn(float, IInterpolation)"/> action.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Show(Stage, SceneAction?)"/>, passing null for SceneAction, to show
    /// the dialog without performing any actions.
    /// </remarks>
    /// <param name="stage"> The Stage to act on. </param>
    /// <returns> This dialog, for chaining. </returns>
    public Dialog Show( Stage stage )
    {
        Show( stage,
              SceneActions.Sequence( SceneActions.Alpha( 0 ),
                                     SceneActions.FadeIn( 0.4f, Interpolation.Fade ) ) );

        SetPosition( ( float )Math.Round( ( stage.Width - GetWidth() ) / 2 ),
                     ( float )Math.Round( ( stage.Height - GetHeight() ) / 2 ) );

        return this;
    }

    /// <summary>
    /// <see cref="WidgetGroup.Pack()"/> the dialog (but doesn't set the position), adds it to the
    /// stage, sets it as the keyboard and scroll focus, clears any actions on the dialog, and adds
    /// the specified action to it. The previous keyboard and scroll focus are remembered so they can
    /// be restored when the dialog is hidden.
    /// </summary>
    /// <param name="stage"> The Stage to act on. </param>
    /// <param name="action"> May be null. </param>
    /// <returns> This dialog, for chaining. </returns>
    public Dialog Show( Stage stage, SceneAction? action )
    {
        ClearActions();
        RemoveCaptureListener( _ignoreTouchDown );

        PreviousKeyboardFocus = null;

        Actor? previousFocus = stage.GetKeyboardFocus();

        if ( previousFocus != null )
        {
            if ( !previousFocus.IsDescendantOf( this ) )
            {
                PreviousKeyboardFocus = previousFocus;
            }
        }

        PreviousScrollFocus = null;

        if ( ( stage.ScrollFocus != null ) && !stage.ScrollFocus.IsDescendantOf( this ) )
        {
            PreviousScrollFocus = stage.ScrollFocus;
        }

        stage.AddActor( this );

        Pack();

        stage.CancelTouchFocus();
        stage.SetKeyboardFocus( this );

        stage.ScrollFocus = this;

        if ( action != null )
        {
            AddAction( action );
        }

        return this;
    }

    /// <summary>
    /// Removes the dialog from the stage, restoring the previous keyboard and scroll focus,
    /// and adds the specified action to the dialog.
    /// </summary>
    /// <param name="action">
    /// If null, the dialog is removed immediately. Otherwise, the dialog is removed when the
    /// action completes. The dialog will not respond to touch down events during the action.
    /// </param>
    public void Hide( SceneAction? action )
    {
        Stage? stage = GetStage();

        if ( stage != null )
        {
            RemoveListener( _dialogFocusListener );

            if ( PreviousKeyboardFocus?.GetStage() == null )
            {
                PreviousKeyboardFocus = null;
            }

            Actor? focus = stage.GetKeyboardFocus();

            if ( ( focus == null ) || focus.IsDescendantOf( this ) )
            {
                stage.SetKeyboardFocus( PreviousKeyboardFocus );
            }

            if ( PreviousScrollFocus?.GetStage() == null )
            {
                PreviousScrollFocus = null;
            }

            if ( ( stage.ScrollFocus == null ) || stage.ScrollFocus.IsDescendantOf( this ) )
            {
                stage.ScrollFocus = PreviousScrollFocus;
            }
        }

        if ( action != null )
        {
            AddCaptureListener( _ignoreTouchDown );

            AddAction( SceneActions.Sequence( action,
                                              SceneActions.RemoveListener( _ignoreTouchDown, true ),
                                              SceneActions.RemoveActor() ) );
        }
        else
        {
            Remove();
        }
    }

    /// <summary>
    /// Hides the dialog. Called automatically when a button is clicked.
    /// The default implementation fades out the dialog over 400 milliseconds.
    /// </summary>
    public void Hide()
    {
        Hide( SceneActions.FadeOut( 0.4f, Interpolation.Fade ) );
    }

    /// <summary>
    /// Sets the object associated with the given button.
    /// </summary>
    /// <param name="actor"> The actor to associate the object with. </param>
    /// <param name="obj"> The object to associate with the actor. </param>
    public void SetObject( Actor actor, object obj )
    {
        Values?[ actor ] = obj;
    }

    /// <summary>
    /// If this key is pressed, <see cref="ClickResult"/> is called with the specified object.
    /// </summary>
    /// <param name="keycode"> The keycode of the key to listen for. </param>
    /// <param name="obj"> The object to pass to <see cref="ClickResult"/> when the key is pressed. </param>
    /// <returns> This dialog, for chaining. </returns>
    public Dialog Key( int keycode, object obj )
    {
        AddListener( new DialogInputListener( this, keycode, obj ) );

        return this;
    }

    /// <summary>
    /// Called when a button is clicked. The dialog will be hidden after this
    /// method returns unless <see cref="CancelHide"/> is set.
    /// </summary>
    /// <param name="obj"> The object specified when the button was added. </param>
    public virtual void ClickResult( object? obj )
    {
    }

    /// <summary>
    /// Gets this Dialogs <see cref="DialogStyle"/> property. Modifying the returned style
    /// may not have an effect until <see cref="SetStyle"/> is called.
    /// </summary>
    /// <returns> The DialogStyle. </returns>
    public override DialogStyle GetStyle() => ( DialogStyle )base.GetStyle();

    /// <summary>
    /// Sets the Dialogs <see cref="DialogStyle"/> property.
    /// </summary>
    /// <param name="style"> The new DialogStyle. </param>
    public void SetStyle( DialogStyle style ) => base.SetStyle( style );

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// A listener that monitors the button table of a dialog for changes and invokes
    /// appropriate actions based on the triggered events.
    /// </summary>
    /// <remarks>
    /// When a change is detected in the button table, it determines whether the triggering
    /// actor is associated with a dialog action value. If a corresponding action value is
    /// found, the dialog's click result is executed, followed by hiding the dialog unless
    /// explicitly canceled.
    /// </remarks>
    internal class ButtonTableChangeListener : ChangeListener
    {
        private readonly Dialog _dialog;

        /// <summary>
        /// Creates a new ButtonTableChangeListener for the specified dialog. 
        /// </summary>
        /// <param name="dialog"> The dialog to monitor for button table changes. </param>
        public ButtonTableChangeListener( Dialog dialog )
        {
            _dialog = dialog;
        }

        /// <summary>
        /// Handles any <see cref="ChangeListener.ChangeEvent"/>s generated.
        /// </summary>
        /// <param name="ev"> The change event. </param>
        /// <param name="actor">
        /// The event target, which is the actor that emitted the change event.
        /// </param>
        public override void Changed( ChangeEvent ev, Actor? actor )
        {
            if ( ( _dialog.Values == null ) || ( actor == null ) )
            {
                return;
            }

            if ( !_dialog.Values.ContainsKey( actor ) )
            {
                return;
            }

            while ( actor?.Parent != _dialog.ButtonTable )
            {
                actor = actor?.Parent;
            }

            _dialog.ClickResult( _dialog.Values[ actor! ] );

            if ( !_dialog.CancelHide )
            {
                _dialog.Hide();
            }

            _dialog.CancelHide = false;
        }
    }

    /// <summary>
    /// An input listener that automatically cancels touch-down events when triggered.
    /// This can be used to ignore or prevent further processing of touch-down input actions
    /// on a specific actor or widget.
    /// </summary>
    public class IgnoreTouchDown : InputListener
    {
        public override bool OnTouchDown( InputEvent? ev, float x, float y, int pointer, int button )
        {
            ev?.Cancel();

            return false;
        }
    }
}

// ============================================================================
// ============================================================================