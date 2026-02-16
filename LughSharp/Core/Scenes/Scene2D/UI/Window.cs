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

using JetBrains.Annotations;

using LughSharp.Core.Graphics.Cameras;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Maths;
using LughSharp.Core.Scenes.Scene2D.Listeners;
using LughSharp.Core.Scenes.Scene2D.Utils;
using LughSharp.Core.Utils;

using Color = LughSharp.Core.Graphics.Color;

namespace LughSharp.Core.Scenes.Scene2D.UI;

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
public class Window : Table
{
    public bool   DrawTitleTable  { get; set; }
    public Label? TitleLabel      { get; set; }
    public bool   IsMovable       { get; set; } = true;
    public bool   IsModal         { get; set; }
    public bool   IsResizable     { get; set; }
    public bool   Dragging        { get; set; }
    public int    ResizeBorder    { get; set; } = 8;
    public bool   KeepWithinStage { get; set; } = true;

    // ========================================================================

    protected int Edge { get; set; }

    // ========================================================================

    private const int DEFAULT_WIDTH  = 150;
    private const int DEFAULT_HEIGHT = 150;
    private const int MOVE           = 1 << 5;

    private static readonly Vector2 _tmpPosition = new();
    private static readonly Vector2 _tmpSize     = new();

    private Table?       _titleTable;
    private WindowStyle? _style;

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
    /// of <see cref="DEFAULT_WIDTH"/> and <see cref="DEFAULT_HEIGHT"/>. A new
    /// <see cref="WindowCaptureListener"/> and <see cref="WindowInputListener"/>
    /// will be added to the window, and the window will be added to the stage.
    /// </summary>
    /// <param name="title"> The title. </param>
    /// <param name="style"> The WindowStyle to use. </param>
    public Window( string title, WindowStyle style )
    {
        Touchable = Touchable.Enabled;
        Clip      = true;

        TitleLabel = new Label( title, new Label.LabelStyle( style.TitleFont!, style.TitleFontColor! ) );
        TitleLabel.SetEllipsis( true );

        _titleTable = new TitleTable( this );
        _titleTable.Add( TitleLabel ).SetExpandX().SetFillX().SetMinWidth( 0 );

        AddActor( _titleTable );

        SetStyle( style );
        SetSize( DEFAULT_WIDTH, DEFAULT_HEIGHT );

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
            var parentWidth  = Stage.Width;
            var parentHeight = Stage.Height;

            if ( ( GetX( Align.RIGHT ) - Stage.Camera.Position.X )
               > ( parentWidth / 2 / orthographicCamera.Zoom ) )
            {
                SetPosition( Stage.Camera.Position.X + ( parentWidth / 2 / orthographicCamera.Zoom ),
                             GetY( Align.RIGHT ),
                             Align.RIGHT );
            }

            if ( ( GetX( Align.LEFT ) - Stage.Camera.Position.X )
               < ( -parentWidth / 2 / orthographicCamera.Zoom ) )
            {
                SetPosition( Stage.Camera.Position.X - ( parentWidth / 2 / orthographicCamera.Zoom ),
                             GetY( Align.LEFT ),
                             Align.LEFT );
            }

            if ( ( GetY( Align.TOP ) - Stage.Camera.Position.Y ) > ( parentHeight / 2 / orthographicCamera.Zoom ) )
            {
                SetPosition( GetX( Align.TOP ),
                             Stage.Camera.Position.Y + ( parentHeight / 2 / orthographicCamera.Zoom ),
                             Align.TOP );
            }

            if ( ( GetY( Align.BOTTOM ) - Stage.Camera.Position.Y )
               < ( -parentHeight / 2 / orthographicCamera.Zoom ) )
            {
                SetPosition( GetX( Align.BOTTOM ),
                             Stage.Camera.Position.Y - ( parentHeight / 2 / orthographicCamera.Zoom ),
                             Align.BOTTOM );
            }
        }
        else if ( Parent == Stage.RootGroup )
        {
            var parentWidth  = Stage.Width;
            var parentHeight = Stage.Height;

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
            Stage.KeyboardFocus ??= this;

            EnsureWithinStage();

            if ( GetStyle()?.StageBackground != null )
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
        batch.SetColor( Color.R, Color.G, Color.B, Color.A * parentAlpha );

        GetStyle()?.StageBackground?.Draw( batch, x, y, width, height );
    }

    protected override void DrawBackground( IBatch batch, float parentAlpha, float x, float y )
    {
        base.DrawBackground( batch, parentAlpha, x, y );

        // Manually draw the title table before clipping is done.

        if ( _titleTable?.Color != null )
        {
            _titleTable.Color.A = Color.A;
        }

        var padTop  = GetPadTop();
        var padLeft = GetPadLeft();

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

        var hit = base.Hit( x, y, touchable );

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
            var current = hit;

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
    public WindowStyle? GetStyle() => _style;

    /// <summary>
    /// Sets this windows <see cref="WindowStyle"/> property.
    /// </summary>
    public void SetStyle( WindowStyle? value )
    {
        _style = value;

        if ( _style == null )
        {
            return;
        }
        
        SetBackground( _style?.Background );
        TitleLabel?.SetStyle( new Label.LabelStyle( _style?.TitleFont, _style?.TitleFontColor ) );
        InvalidateHierarchy();
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

        public new bool TouchDown( InputEvent ev, float x, float y, int pointer, int button )
        {
            _window.ToFront();

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

        public override bool TouchDown( InputEvent? ev, float x, float y, int pointer, int button )
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

        public override void TouchUp( InputEvent? ev, float x, float y, int pointer, int button )
        {
            _window.Dragging = false;
        }

        public override void TouchDragged( InputEvent? ev, float x, float y, int pointer )
        {
            if ( !_window.Dragging )
            {
                return;
            }

            var width     = _window.Width;
            var height    = _window.Height;
            var windowX   = _window.X;
            var windowY   = _window.Y;
            var minWidth  = _window.GetMinWidth();
            var minHeight = _window.GetMinHeight();
            var stage     = _window.Stage;

            var clampPosition = _window.KeepWithinStage
                             && ( stage != null )
                             && ( _window.Parent == stage.RootGroup );

            if ( ( _window.Edge & MOVE ) != 0 )
            {
                var amountX = x - _startX;
                var amountY = y - _startY;

                windowX += amountX;
                windowY += amountY;
            }

            if ( ( _window.Edge & Align.LEFT ) != 0 )
            {
                var amountX = x - _startX;

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

            if ( ( _window.Edge & Align.BOTTOM ) != 0 )
            {
                var amountY = y - _startY;

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

            if ( ( _window.Edge & Align.RIGHT ) != 0 )
            {
                var amountX = x - _lastX - width;

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

            if ( ( _window.Edge & Align.TOP ) != 0 )
            {
                var amountY = y - _lastY - height;

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

        public override bool MouseMoved( InputEvent? ev, float x, float y )
        {
            UpdateEdge( x, y );

            return _window.IsModal;
        }

        public bool Scrolled( InputEvent ev, float x, float y, int amount )
        {
            return _window.IsModal;
        }

        public override bool KeyDown( InputEvent? ev, int keycode )
        {
            return _window.IsModal;
        }

        public override bool KeyUp( InputEvent? ev, int keycode )
        {
            return _window.IsModal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public override bool KeyTyped( InputEvent? ev, char character )
        {
            return _window.IsModal;
        }

        private void UpdateEdge( float x, float y )
        {
            var border = _window.ResizeBorder / 2f;
            var width  = _window.Width;
            var height = _window.Height;

            var padTop    = _window.GetPadTop();
            var padLeft   = _window.GetPadLeft();
            var padBottom = _window.GetPadBottom();
            var padRight  = _window.GetPadRight();
            var right     = width - padRight;

            _window.Edge = 0;

            if ( _window.IsResizable
              && ( x >= ( padLeft - border ) )
              && ( x <= ( right + border ) )
              && ( y >= ( padBottom - border ) ) )
            {
                if ( x < ( padLeft + border ) )
                {
                    _window.Edge |= Align.LEFT;
                }

                if ( x > ( right - border ) )
                {
                    _window.Edge |= Align.RIGHT;
                }

                if ( y < ( padBottom + border ) )
                {
                    _window.Edge |= Align.BOTTOM;
                }

                if ( _window.Edge != 0 )
                {
                    border += 25;
                }

                if ( x < ( padLeft + border ) )
                {
                    _window.Edge |= Align.LEFT;
                }

                if ( x > ( right - border ) )
                {
                    _window.Edge |= Align.RIGHT;
                }

                if ( y < ( padBottom + border ) )
                {
                    _window.Edge |= Align.BOTTOM;
                }
            }

            if ( _window is { IsMovable: true, Edge: 0 }
              && ( y <= height )
              && ( y >= ( height - padTop ) )
              && ( x >= padLeft )
              && ( x <= right )
               )
            {
                _window.Edge = MOVE;
            }
        }
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// The style for a window, see <see cref="Window"/>.
    /// </summary>
    [PublicAPI]
    public class WindowStyle
    {
        public ISceneDrawable? Background      { get; set; }
        public BitmapFont?     TitleFont       { get; set; }
        public Color?          TitleFontColor  { get; set; } = new( 1, 1, 1, 1 );
        public ISceneDrawable? StageBackground { get; set; }

        // ====================================================================

        /// <summary>
        /// Creates a new empty WindowStyle instance. Before using this style it will be
        /// necessary to set the <see cref="Background"/>, <see cref="TitleFont"/> properties.
        /// The <see cref="TitleFontColor"/>defaults to white, but can be changed to any color.
        /// </summary>
        public WindowStyle()
        {
        }

        /// <summary>
        /// Creates a new WindowStyle instance with the specified parameters for
        /// <see cref="TitleFont"/>, <see cref="TitleFontColor"/>, and <see cref="Background"/>.
        /// </summary>
        /// <param name="titleFont"></param>
        /// <param name="titleFontColor"></param>
        /// <param name="background"></param>
        public WindowStyle( BitmapFont titleFont, Color titleFontColor, ISceneDrawable? background )
        {
            TitleFont  = titleFont;
            Background = background;

            TitleFontColor?.Set( titleFontColor );
        }

        /// <summary>
        /// Copies the specified style into this style.
        /// </summary>
        /// <param name="style"> The style to copy from. </param>
        public WindowStyle( WindowStyle style )
        {
            Background = style.Background;
            TitleFont  = style.TitleFont;

            if ( style.TitleFontColor != null )
            {
                TitleFontColor = new Color( style.TitleFontColor );
            }

            Background = style.Background;
        }
    }
}

// ========================================================================
// ========================================================================