// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in all
//  copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//  SOFTWARE.
// /////////////////////////////////////////////////////////////////////////////

using LughSharp.Core.Graphics;
using LughSharp.Core.Graphics.G2D;
using LughSharp.Core.Graphics.Utils;
using LughSharp.Core.Graphics;
using LughUtils.source.Maths;
using LughUtils.source.Exceptions;
using LughUtils.source.Logging;

using NUnit.Framework;
using Color = LughSharp.Core.Graphics.Color;

namespace LughSharp.Tests.Source;

[TestFixture]
public class SpriteBatchTests
{
    private const int TEST_WIDTH  = 64;
    private const int TEST_HEIGHT = 64;

    private SpriteBatch? _spriteBatch;
    private Texture?     _testTexture;

    // ========================================================================

    [SetUp]
    public void Setup()
    {
        Logger.Checkpoint();

        _spriteBatch = new SpriteBatch();

        // Create and verify the pixmap
        var pixmap = new Pixmap( TEST_WIDTH, TEST_HEIGHT, LughFormat.RGBA8888 );
        
        Assert.That( pixmap, Is.Not.Null, "Pixmap creation failed" );
        Assert.That( pixmap.Gdx2DPixmap, Is.Not.Null, "pixmap.Gdx2DPixmap is null" );
        Assert.That( pixmap.Width, Is.EqualTo( TEST_WIDTH ), "Pixmap width incorrect" );
        Assert.That( pixmap.Height, Is.EqualTo( TEST_HEIGHT ), "Pixmap height incorrect" );

        pixmap.SetColor( Color.White );
        pixmap.FillWithCurrentColor();

        var textureData = new PixmapTextureData( pixmap, LughFormat.RGBA8888, false, false );
        
        Assert.That( textureData, Is.Not.Null, "TextureData creation failed" );

        _testTexture = new Texture( textureData );
        
        Assert.That( _testTexture, Is.Not.Null, "Texture creation failed" );

        pixmap.Dispose(); // Clean up the pixmap after creating texture

        Logger.Debug( "Done." );
    }

    [Test]
    public void Begin_WhenCalled_SetsCorrectDefaultState()
    {
        Logger.Checkpoint();

        Assert.Multiple( () =>
        {
            Assert.That( _spriteBatch, Is.Not.Null );
            Assert.DoesNotThrow( () => _spriteBatch?.Begin() );
            Assert.That( _spriteBatch!.IsDrawing );
        } );

        if ( _spriteBatch!.IsDrawing )
        {
            _spriteBatch.End();
        }

        Logger.Debug( "Done." );
    }

    [Test]
    public void Draw_WhenCalledBetweenBeginAndEnd_SuccessfullyDrawsTexture()
    {
        Logger.Checkpoint();

        Assert.Multiple( () =>
        {
            Assert.That( _spriteBatch, Is.Not.Null );
            Assert.That( _testTexture, Is.Not.Null );

            Assert.That( _testTexture?.Width, Is.Not.Null );
            Assert.That( _testTexture?.Height, Is.Not.Null );

            // Verify texture dimensions before drawing
            Assert.That( _testTexture?.Width, Is.EqualTo( TEST_WIDTH ) );
            Assert.That( _testTexture?.Height, Is.EqualTo( TEST_HEIGHT ) );

            _spriteBatch?.Begin();
            Assert.DoesNotThrow( () => _spriteBatch?.Draw( _testTexture!, 0, 0, TEST_WIDTH, TEST_HEIGHT ) );
            _spriteBatch?.End();
        } );

        Logger.Debug( "Done." );
    }

    [Test]
    public void Draw_WhenCalledOutsideBeginEnd_ThrowsException()
    {
        Logger.Checkpoint();

        Assert.Multiple( () =>
        {
            Assert.That( _spriteBatch, Is.Not.Null );
            Assert.That( _testTexture, Is.Not.Null );

            Assert.Throws< GdxRuntimeException >( () => _spriteBatch?.Draw( _testTexture, 0, 0 ) );
        } );

        Logger.Debug( "Done." );
    }

    [Test]
    public void SetProjectionMatrix_UpdatesMatrixCorrectly()
    {
        Logger.Checkpoint();

        var projection = new Matrix4();
        projection.SetToOrtho2D( 0, 0, 800, 600 );

        Assert.Multiple( () =>
        {
            Assert.That( _spriteBatch, Is.Not.Null );
            Assert.DoesNotThrow( () => _spriteBatch!.SetProjectionMatrix( projection ) );
        } );

        Logger.Debug( "Done." );
    }

    [Test]
    public void Blending_CanBeToggledCorrectly()
    {
        Logger.Checkpoint();

        Assert.Multiple( () =>
        {
            Assert.That( _spriteBatch, Is.Not.Null );

            // Test blending without Begin/End first
            _spriteBatch!.EnableBlending();
            Assert.That( _spriteBatch.BlendingEnabled );

            _spriteBatch.DisableBlending();
            Assert.That( !_spriteBatch.BlendingEnabled );

            // Now test within Begin/End block
            _spriteBatch.Begin();
            _spriteBatch.EnableBlending();
            Assert.That( _spriteBatch.BlendingEnabled );

            _spriteBatch.DisableBlending();
            Assert.That( !_spriteBatch.BlendingEnabled );
            _spriteBatch.End();
        } );

        Logger.Debug( "Done." );
    }

    [Test]
    public void Color_CanBeSetAndRetrieved()
    {
        Logger.Checkpoint();

        Assert.Multiple( () =>
        {
            Assert.That( _spriteBatch, Is.Not.Null );

            var testColor = new Color( 1f, 0f, 0f, 1f ); // Red
            _spriteBatch?.SetColor( testColor );

            Assert.That( _spriteBatch?.Color.R, Is.EqualTo( testColor.R ) );
            Assert.That( _spriteBatch?.Color.G, Is.EqualTo( testColor.G ) );
            Assert.That( _spriteBatch?.Color.B, Is.EqualTo( testColor.B ) );
            Assert.That( _spriteBatch?.Color.A, Is.EqualTo( testColor.A ) );
        } );

        Logger.Debug( "Done." );
    }

    [TearDown]
    public void Teardown()
    {
        Logger.Checkpoint();

//        _spriteBatch?.Dispose();
        _testTexture?.Dispose();
    }
}

// ========================================================================
// ========================================================================`