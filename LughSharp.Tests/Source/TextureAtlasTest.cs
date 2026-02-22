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

using System.Runtime.Versioning;
using LughSharp.Core.Files;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Graphics.G2D;
using NUnit.Framework;

namespace LughSharp.Tests.Source;

[TestFixture]
[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class TextureAtlasTest : ILughTest
{
    private Sprite? _sprite;
    
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public void Run()
    {
        var atlasData = new TextureAtlasData( Api.Files.Internal( @"Assets\PackedImages\Output\input.atlas" ),
                                              new DirectoryInfo( IOUtils.InternalPath +
                                                                 @"\Assets\PackedImages\Output" ) );
        
        var atlas = new TextureAtlas( atlasData );

        _sprite = atlas.CreateSprite( "button_b" );
        _sprite?.SetPosition( 300, 300 );
        _sprite?.SetBounds();
        _sprite?.SetOriginCenter();
        _sprite?.SetFlip( false, false );
    }

    public void Render( SpriteBatch spriteBatch )
    {
        _sprite?.Draw( spriteBatch );
    }
    
    [TearDown]
    public void TearDown()
    {
    }
}

// ============================================================================
// ============================================================================
