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

using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Maths;

namespace Extensions.Source.Sprites;

[PublicAPI]
public interface IGameSprite
{
    /// <summary>
    /// Initialise this Sprite. Override in any entity classes and add any other relevant
    /// initialisation code AFTER the call to create(). All information necessary to create
    /// the sprite should be stored in the <see cref="SpriteDescriptor"/>.
    /// </summary>
    /// <param name="descriptor"> The SpriteDescriptor holding all setup information. </param>
    void Initialise( SpriteDescriptor descriptor );

    /// <summary>
    /// Performs the actual setting up of the GdxSprite,
    /// according to the information provided in the
    /// supplied {@link SpriteDescriptor}.
    /// </summary>
    void Create( SpriteDescriptor descriptor );

    /// <summary>
    /// Sets the initial starting position for this sprite.
    /// NOTE: It is important that frameWidth & frameHeight
    /// are initialised before this method is called.
    /// </summary>
    void InitPosition( Vector3 vec3 );

    /// <summary>
    /// Provides an init position modifier value.
    /// GdxSprites are placed on TiledMap boundaries and
    /// some may need that position adjusting.
    /// </summary>
    Vector3 GetPositionModifier();

    /// <summary>
    /// Provides the facility for some sprites to perform certain
    /// actions before the main update method.
    /// Some sprites may not need to do this, or may need to do extra
    /// tasks, in which case this can be overridden.
    /// </summary>
    void PreUpdate();

    void Update();

    void PostUpdate();

    void UpdateCommon();

    void PreDraw();

    void Draw( SpriteBatch spriteBatch );

    void Animate();

    void SetAnimation( SpriteDescriptor descriptor );

    void DefinePhysicsBodyBox( bool useBodyPos );
}

// ============================================================================
// ============================================================================

// ============================================================================
// ============================================================================


