﻿// ///////////////////////////////////////////////////////////////////////////////
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

using LughSharp.Lugh.Scenes.Scene2D.Listeners;

namespace LughSharp.Lugh.Scenes.Scene2D.UI;

public class DialogChangeListener : ChangeListener
{
    private readonly Dialog _dialog;

    public DialogChangeListener( Dialog d )
    {
        _dialog = d;
    }

    /// <inheritdoc cref="ChangeListener.Changed" />
    public override void Changed( ChangeEvent ev, Actor? actor )
    {
        if ( ( _dialog.Values == null ) || ( actor == null ) )
        {
            return;
        }

        if ( !_dialog.Values!.ContainsKey( actor ) )
        {
            return;
        }

        while ( actor!.Parent != _dialog.ButtonTable )
        {
            actor = actor.Parent;
        }

        _dialog.Result( _dialog.Values[ actor ] );

        if ( !_dialog.CancelHide )
        {
            _dialog.Hide();
        }

        _dialog.CancelHide = false;
    }
}