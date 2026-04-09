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

using LughSharp.Core;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Images;
using LughSharp.Core.Maths;

namespace TestProject.Source;

[PublicAPI]
public class SpriteTests : IDisposable
{
    private const int MaxSprites = 250;

    private Vector2[]   _spritePosition = null!;
    private Sprite2D[]? _sprite         = null!;
    private int[]       _xdirection     = null!;
    private int[]       _ydirection     = null!;
    private float[]     _xspeed         = null!;
    private float[]     _yspeed         = null!;

    // ========================================================================

    public void Create()
    {
        _sprite         = new Sprite2D[ MaxSprites ];
        _spritePosition = new Vector2[ MaxSprites ];
        _xdirection     = new int[ MaxSprites ];
        _ydirection     = new int[ MaxSprites ];
        _xspeed         = new float[ MaxSprites ];
        _yspeed         = new float[ MaxSprites ];

        for ( var i = 0; i < MaxSprites; i++ )
        {
            var rand = MathUtils.Random( 100 );

            TextureRegion region;

            if ( rand < 50 )
            {
                region = new TextureRegion( new Texture( Assets.Boulder32X32 ) );
            }
            else if ( rand < 75 )
            {
                region = new TextureRegion( new Texture( Assets.Boulder48X48 ) );
            }
            else
            {
                region = new TextureRegion( new Texture( Assets.Boulder64X64 ) );
            }

            _sprite[ i ] = new Sprite2D( region );
            _sprite[ i ].SetBounds();
            _sprite[ i ].SetOriginCenter();

            _spritePosition[ i ] = new Vector2( MathUtils.Random( Engine.Graphics.WindowWidth ),
                                                MathUtils.Random( Engine.Graphics.WindowHeight ) );
            _xdirection[ i ] = MathUtils.RandomBool() ? -1 : 1;
            _ydirection[ i ] = MathUtils.RandomBool() ? -1 : 1;
            _xspeed[ i ]     = 1f + ( MathUtils.Random( 100 ) / 100f );
            _yspeed[ i ]     = 1f + ( MathUtils.Random( 100 ) / 100f );

            _sprite[ i ].SetPosition( _spritePosition[ i ] );
        }
    }

    public void Update( float delta )
    {
        if ( _sprite != null )
        {
            for ( var i = 0; i < MaxSprites; i++ )
            {
                Bounce( i );

                _sprite[ i ].Rotate( 4f * ( _xdirection[ i ] * -1 ) );
            }
        }
    }

    public void Draw( IBatch batch )
    {
        if ( _sprite != null )
        {
            for ( var i = 0; i < MaxSprites; i++ )
            {
                _sprite[ i ].Draw( batch );
            }
        }
    }

    private void Bounce( int index )
    {
        if ( _sprite == null )
        {
            return;
        }
        
        for ( var i = 0; i < MaxSprites; i++ )
        {
            if ( _sprite[ i ].GetX() <= -_sprite[ i ].Width )
            {
                _xdirection[ i ] = 1;
            }
            else if ( _sprite[ i ].GetX() >= Engine.Graphics.WindowWidth )
            {
                _xdirection[ i ] = -1;
            }

            if ( _sprite[ i ].GetY() <= -_sprite[ i ].Height )
            {
                _ydirection[ i ] = 1;
            }
            else if ( _sprite[ i ].GetY() >= Engine.Graphics.WindowHeight )
            {
                _ydirection[ i ] = -1;
            }

            _sprite[ i ].TranslateX( _xspeed[ i ] * _xdirection[ i ] * Engine.DeltaTime );
            _sprite[ i ].TranslateY( _yspeed[ i ] * _ydirection[ i ] * Engine.DeltaTime );
        }
    }

    public void Dispose()
    {
        // TODO:
        // Should Sprite() implement IDisposable, or should I leave that up to
        // any extending classes? Maybe implement IDisposable in Sprite() and
        // allow ( and encourage ) extending classes to override it?
//        _sprite = null;

        GC.SuppressFinalize( this );
    }
}

// ============================================================================
// ============================================================================