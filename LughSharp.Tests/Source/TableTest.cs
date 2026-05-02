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

using JetBrains.Annotations;

using LughSharp.Source;
using LughSharp.Source.Graphics.Images;
using LughSharp.Source.Scene2D;
using LughSharp.Source.Scene2D.Listeners;
using LughSharp.Source.Scene2D.UI;
using LughSharp.Source.Scene2D.UI.Styles;
using LughSharp.Source.Scene2D.Utils;
using LughSharp.Source.Utils;
using LughSharp.Source.Utils.Logging;

using NUnit.Framework;

namespace LughSharp.Tests.Source;

[TestFixture]
[PublicAPI]
public class TableTest
{
    private Stage _stage;
    private Skin  _skin;
    private Table _root;

    public TableTest( ref Stage stage )
    {
        _stage = stage;
        _skin  = new Skin( Engine.Files.Assets( "Skins/uiskin.json" ) );
        
        _root = new Table
        {
            IsVisible = true,
        };
    }

    [SetUp]
    public void Setup()
    {
        Engine.Input.InputProcessor = _stage;

        var label = new Label( "This is some text.", _skin );

        _stage.AddActor( _root );
        // root.setTransform(true);

        var table = new Table
        {
            Transform = true,
            Rotation  = 45f,
            ScaleY    = 2
        };
        table.SetPosition( 100, 100 );
        table.SetOrigin( 0, 0 );
        table.SetBackground( _skin.Get< WindowStyle >( "default" ).Background );
        table.AddCell( label );
        table.AddCell( new TextButton( "Text Button", _skin ) );
        table.Pack();

        // table.debug();
        table.AddListener( new ClickListener( ( ev, x, y ) => { Logger.Debug( "Clicked!" ); } ) );
        // root.addActor(table);

        var button = new TextButton( "Text Button", _skin );

        var table2 = new Table();

        // table2.debug()
        table2.AddCell( button );
        table2.Transform = true;
        table2.ScaleX    = 1.5f;
        table2.SetOrigin( table2.GetPrefWidth() / 2, table2.GetPrefHeight() / 2 );

        // Test colspan with expandX.
        // root.setPosition(10, 10);
        _root.FillParent = true;

        var lbl = new Label( "meow meow meow meow meow meow meow meow meow meow meow meow meow", _skin );

        _root.AddCell( lbl ).SetColspan( 3 ).SetExpandX();
        _root.AddCell( new TextButton( "Text Button", _skin ) );
        _root.AddRow();
        _root.AddCell( new TextButton( "Text Button", _skin ) );
        _root.AddCell( new TextButton( "Toggle Button", _skin.Get< TextButtonStyle >( "toggle" ) ) );
        _root.AddCell( new CheckBox( "meow meow meow meow meow meow meow meow", _skin ) );
    }

    [TearDown]
    public void TearDown()
    {
    }
}

// ============================================================================
// ============================================================================