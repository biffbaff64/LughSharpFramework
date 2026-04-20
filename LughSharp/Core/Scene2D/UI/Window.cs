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

using LughSharp.Core.Graphics.Cameras;
using LughSharp.Core.Graphics.Fonts;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Scene2D.Listeners;
using LughSharp.Core.Scene2D.UI.Styles;

namespace LughSharp.Core.Scene2D.UI;

/// <summary>
/// A table that can be dragged and act as a modal window. The top padding is
/// used as the window's title height.
/// <para>
/// The preferred size of a window is the preferred size of the title text and
/// the children as laid out by the table. After adding children to the window,
/// it can be convenient to call <see cref="WidgetGroup.Pack"/> to size the
/// window to the size of the children.
/// </para>
/// </summary>
[PublicAPI]
[ActorDefinition( Role = "UI" )]
public class Window : Table
{
    public int    ResizeBorder    { get; set; } = 8;
    public bool   KeepWithinStage { get; set; } = true;
    public bool   IsMovable       { get; set; } = true;
    public bool   DrawTitleTable  { get; set; }
    public Label? TitleLabel      { get; set; }
    public bool   IsModal         { get; set; }
    public bool   IsResizable     { get; set; }
    public bool   Dragging        { get; set; }

    // ========================================================================

    protected Align Edge { get; set; }

    // ========================================================================

    private const int   DefaultWidth  = 150;
    private const int   DefaultHeight = 150;
    private const Align Move          = Align.Special;

    private static readonly Vector2 _tmpPosition = new();
    private static readonly Vector2 _tmpSize     = new();

    private Table? _titleTable;

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Creates a window with the specified title, and using the default
    /// <see cref="WindowStyle"/> from the specified <see cref="Skin"/>
    /// </summary>
    /// <param name="title"> A string holding the title. </param>
    /// <param name="skin"> The Skin. </param>
    public Window( string title, Skin skin )
        : this( title, skin.Get< WindowStyle >() )
    {
        Skin = skin;
    }

    /// <summary>
    /// Creates a window with the specified title, and using the named
    /// <see cref="WindowStyle"/> from the specified <see cref="Skin"/>
    /// </summary>
    /// <param name="title"> A string holding the title. </param>
    /// <param name="skin"> The Skin. </param>
    /// <param name="styleName"> The name of the WindowStyle to use. </param>
    public Window( string title, Skin skin, string styleName )
        : this( title, skin.Get< WindowStyle >( styleName ) )
    {
        Skin = skin;
    }

    /// <summary>
    /// Creates a window with the specified title, and using the specified
    /// <see cref="WindowStyle"/>. The window size will be set to the default sizes
    /// of <see cref="DefaultWidth"/> and <see cref="DefaultHeight"/>. A new
    /// <see cref="WindowCaptureListener"/> and <see cref="WindowInputListener"/>
    /// will be added to the window, and the window will be added to the stage.
    /// </summary>
    /// <param name="title"> The title. </param>
    /// <param name="style"> The WindowStyle to use. </param>
    public Window( string title, WindowStyle style )
    {
        Touchable = Touchable.Enabled;
        Clip      = true;

        style.TitleFont      ??= new BitmapFont();
        style.TitleFontColor ??= Color.White;
        
        TitleLabel = new Label( title, new LabelStyle( style.TitleFont, style.TitleFontColor ) );
        TitleLabel.SetEllipsis( true );

        _titleTable = new TitleTable( this );
        _titleTable.AddCell( TitleLabel )
                   .SetExpandX()
                   .SetFillX()
                   .SetMinWidth( 0 );

        AddActor( _titleTable );

        Style = style;
        SetSize( DefaultWidth, DefaultHeight );

        AddCaptureListener( new WindowCaptureListener( this ) );
        AddListener( new WindowInputListener( this ) );
    }

    /// <summary>
    /// Ensures that the window remains within the bounds of the stage by adjusting
    /// its position if necessary. The check accounts for the stage dimensions, camera
    /// position, and zoom level, as well as whether the window is a direct child of
    /// the stage's root group.
    /// </summary>
    public void EnsureWithinStage()
    {
        if ( !KeepWithinStage || ( Stage == null ) )
        {
            return;
        }

        if ( Stage.Camera is OrthographicCamera orthographicCamera )
        {
            float parentWidth  = Stage.Width;
            float parentHeight = Stage.Height;

            if ( ( GetX( Align.Right ) - Stage.Camera.Position.X )
               > ( parentWidth / 2 / orthographicCamera.Zoom ) )
            {
                SetPosition( Stage.Camera.Position.X + ( parentWidth / 2 / orthographicCamera.Zoom ),
                             GetY( Align.Right ),
                             Align.Right );
            }

            if ( ( GetX( Align.Left ) - Stage.Camera.Position.X )
               < ( -parentWidth / 2 / orthographicCamera.Zoom ) )
            {
                SetPosition( Stage.Camera.Position.X - ( parentWidth / 2 / orthographicCamera.Zoom ),
                             GetY( Align.Left ),
                             Align.Left );
            }

            if ( ( GetY( Align.Top ) - Stage.Camera.Position.Y ) > ( parentHeight / 2 / orthographicCamera.Zoom ) )
            {
                SetPosition( GetX( Align.Top ),
                             Stage.Camera.Position.Y + ( parentHeight / 2 / orthographicCamera.Zoom ),
                             Align.Top );
            }

            if ( ( GetY( Align.Bottom ) - Stage.Camera.Position.Y )
               < ( -parentHeight / 2 / orthographicCamera.Zoom ) )
            {
                SetPosition( GetX( Align.Bottom ),
                             Stage.Camera.Position.Y - ( parentHeight / 2 / orthographicCamera.Zoom ),
                             Align.Bottom );
            }
        }
        else if ( Parent == Stage.RootGroup )
        {
            float parentWidth  = Stage.Width;
            float parentHeight = Stage.Height;

            if ( X < 0 )
            {
                X = 0;
            }

            if ( RightEdge > parentWidth )
            {
                X = parentWidth - Width;
            }

            if ( Y < 0 )
            {
                Y = 0;
            }

            if ( TopEdge > parentHeight )
            {
                Y = parentHeight - Height;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="parentAlpha"></param>
    public override void Draw( IBatch batch, float parentAlpha )
    {
        if ( Stage != null )
        {
            Stage.SetKeyboardFocus( Stage.GetKeyboardFocus() ?? this );

            EnsureWithinStage();

            if ( Style.StageBackground != null )
            {
                StageToLocalCoordinates( _tmpPosition.Set( 0, 0 ) );
                StageToLocalCoordinates( _tmpSize.Set( Stage.Width, Stage.Height ) );

                DrawStageBackground( batch,
                                     parentAlpha,
                                     X + _tmpPosition.X,
                                     Y + _tmpPosition.Y,
                                     X + _tmpSize.X,
                                     Y + _tmpSize.Y );
            }
        }

        base.Draw( batch, parentAlpha );
    }

    protected void DrawStageBackground( IBatch batch, float parentAlpha, float x, float y, float width, float height )
    {
        batch.SetColor( ActorColor.R, ActorColor.G, ActorColor.B, ActorColor.A * parentAlpha );

        Style.StageBackground?.Draw( batch, x, y, width, height );
    }

    protected override void DrawBackground( IBatch batch, float parentAlpha, float x, float y )
    {
        base.DrawBackground( batch, parentAlpha, x, y );

        // Manually draw the title table before clipping is done.

        if ( _titleTable?.ActorColor != null )
        {
            _titleTable.ActorColor.A = ActorColor.A;
        }

        float padTop  = GetPadTop();
        float padLeft = GetPadLeft();

        _titleTable?.SetSize( Width - padLeft - GetPadRight(), padTop );
        _titleTable?.SetPosition( padLeft, Height - padTop );

        DrawTitleTable = true;

        _titleTable?.Draw( batch, parentAlpha );

        // Avoid drawing the title table again in drawChildren.
        DrawTitleTable = false;
    }

    public override Actor? Hit( float x, float y, bool touchable )
    {
        if ( !IsVisible )
        {
            return null;
        }

        Actor? hit = base.Hit( x, y, touchable );

        if ( ( hit == null ) && IsModal && ( !touchable || ( Touchable == Touchable.Enabled ) ) )
        {
            return this;
        }

        if ( ( hit == null ) || ( hit == this ) )
        {
            return hit;
        }

        if ( ( y <= Height ) && ( y >= ( Height - GetPadTop() ) ) && ( x >= 0 ) && ( x <= Width ) )
        {
            // Hit the title bar, don't use the hit child if it is in the Window's table.
            Actor? current = hit;

            while ( current?.Parent != this )
            {
                current = current?.Parent;
            }

            if ( GetCell( current ) != null )
            {
                return this;
            }
        }

        return hit;
    }

    public override float GetPrefWidth()
    {
        return _titleTable == null
            ? base.GetPrefWidth()
            : Math.Max( base.GetPrefWidth(), _titleTable.GetPrefWidth() + GetPadLeft() + GetPadRight() );
    }

    /// <summary>
    /// Gets this windows <see cref="WindowStyle"/> property.
    /// </summary>
    public WindowStyle Style
    {
        get;
        set
        {
            field = value;

            SetBackground( Style.Background );

            if ( TitleLabel != null )
            {
                if ( field.TitleFont != null && field.TitleFontColor != null )
                {
                    TitleLabel.Style = new LabelStyle( field.TitleFont, field.TitleFontColor );
                }
            }

            InvalidateHierarchy();
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class TitleTable : Table
    {
        private readonly Window _window;

        public TitleTable( Window window )
        {
            _window = window;
        }

        public override void Draw( IBatch batch, float parentAlpha )
        {
            if ( _window.DrawTitleTable )
            {
                base.Draw( batch, parentAlpha );
            }
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class WindowCaptureListener : InputListener
    {
        private readonly Window _window;

        public WindowCaptureListener( Window window )
        {
            _window = window;
        }

        public override bool OnTouchDown( InputEvent? ev, float x, float y, int pointer, int button )
        {
            _window.BringToFront();

            return false;
        }
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class WindowInputListener : InputListener
    {
        private readonly Window _window;

        private float _lastX;
        private float _lastY;
        private float _startX;
        private float _startY;

        // ====================================================================

        /// <summary>
        /// Creates a new WindowInputListener for the specified window.
        /// </summary>
        /// <param name="window"> The Window. </param>
        public WindowInputListener( Window window )
        {
            _window = window;
        }

        public override bool OnTouchDown( InputEvent? ev, float x, float y, int pointer, int button )
        {
            if ( button == 0 )
            {
                UpdateEdge( x, y );

                _window.Dragging = _window.Edge != 0;
                _startX          = x;
                _startY          = y;
                _lastX           = x - _window.Width;
                _lastY           = y - _window.Height;
            }

            return ( _window.Edge != 0 ) || _window.IsModal;
        }

        public override void OnTouchUp( InputEvent? ev, float x, float y, int pointer, int button )
        {
            _window.Dragging = false;
        }

        public override void OnTouchDragged( InputEvent? ev, float x, float y, int pointer )
        {
            if ( !_window.Dragging )
            {
                return;
            }

            float  width     = _window.Width;
            float  height    = _window.Height;
            float  windowX   = _window.X;
            float  windowY   = _window.Y;
            float  minWidth  = _window.GetMinWidth();
            float  minHeight = _window.GetMinHeight();
            Stage? stage     = _window.Stage;

            bool clampPosition = _window.KeepWithinStage
                              && ( stage != null )
                              && ( _window.Parent == stage.RootGroup );

            if ( ( _window.Edge & Move ) != 0 )
            {
                float amountX = x - _startX;
                float amountY = y - _startY;

                windowX += amountX;
                windowY += amountY;
            }

            if ( ( _window.Edge & Align.Left ) != 0 )
            {
                float amountX = x - _startX;

                if ( ( width - amountX ) < minWidth )
                {
                    amountX = -( minWidth - width );
                }

                if ( clampPosition && ( ( windowX + amountX ) < 0 ) )
                {
                    amountX = -windowX;
                }

                width   -= amountX;
                windowX += amountX;
            }

            if ( ( _window.Edge & Align.Bottom ) != 0 )
            {
                float amountY = y - _startY;

                if ( ( height - amountY ) < minHeight )
                {
                    amountY = -( minHeight - height );
                }

                if ( clampPosition && ( ( windowY + amountY ) < 0 ) )
                {
                    amountY = -windowY;
                }

                height  -= amountY;
                windowY += amountY;
            }

            if ( ( _window.Edge & Align.Right ) != 0 )
            {
                float amountX = x - _lastX - width;

                if ( ( width + amountX ) < minWidth )
                {
                    amountX = minWidth - width;
                }

                if ( clampPosition && ( ( windowX + width + amountX ) > stage?.Width ) )
                {
                    amountX = stage.Width - windowX - width;
                }

                width += amountX;
            }

            if ( ( _window.Edge & Align.Top ) != 0 )
            {
                float amountY = y - _lastY - height;

                if ( ( height + amountY ) < minHeight )
                {
                    amountY = minHeight - height;
                }

                if ( clampPosition && ( ( windowY + height + amountY ) > stage?.Height ) )
                {
                    amountY = stage.Height - windowY - height;
                }

                height += amountY;
            }

            _window.SetBounds( ( float )Math.Round( windowX ),
                               ( float )Math.Round( windowY ),
                               ( float )Math.Round( width ),
                               ( float )Math.Round( height ) );
        }

        public override bool OnMouseMoved( InputEvent? ev, float x, float y )
        {
            UpdateEdge( x, y );

            return _window.IsModal;
        }

        public bool OnScrolled( InputEvent ev, float x, float y, int amount )
        {
            return _window.IsModal;
        }

        public override bool OnKeyDown( InputEvent? ev, int keycode )
        {
            return _window.IsModal;
        }

        public override bool OnKeyUp( InputEvent? ev, int keycode )
        {
            return _window.IsModal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public override bool OnKeyTyped( InputEvent? ev, char character )
        {
            return _window.IsModal;
        }

        private void UpdateEdge( float x, float y )
        {
            float border = _window.ResizeBorder / 2f;
            float width  = _window.Width;
            float height = _window.Height;

            float padTop    = _window.GetPadTop();
            float padLeft   = _window.GetPadLeft();
            float padBottom = _window.GetPadBottom();
            float padRight  = _window.GetPadRight();
            float right     = width - padRight;

            _window.Edge = 0;

            if ( _window.IsResizable
              && ( x >= ( padLeft - border ) )
              && ( x <= ( right + border ) )
              && ( y >= ( padBottom - border ) ) )
            {
                if ( x < ( padLeft + border ) )
                {
                    _window.Edge |= Align.Left;
                }

                if ( x > ( right - border ) )
                {
                    _window.Edge |= Align.Right;
                }

                if ( y < ( padBottom + border ) )
                {
                    _window.Edge |= Align.Bottom;
                }

                if ( _window.Edge != 0 )
                {
                    border += 25;
                }

                if ( x < ( padLeft + border ) )
                {
                    _window.Edge |= Align.Left;
                }

                if ( x > ( right - border ) )
                {
                    _window.Edge |= Align.Right;
                }

                if ( y < ( padBottom + border ) )
                {
                    _window.Edge |= Align.Bottom;
                }
            }

            if ( _window is { IsMovable: true, Edge: 0 }
              && ( y <= height )
              && ( y >= ( height - padTop ) )
              && ( x >= padLeft )
              && ( x <= right )
               )
            {
                _window.Edge = Move;
            }
        }
    }

    // ========================================================================
    // ========================================================================
}

// ========================================================================
// ========================================================================