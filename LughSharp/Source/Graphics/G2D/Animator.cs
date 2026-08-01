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

using LughSharp.Source.Graphics.Atlases;
using LughSharp.Source.Graphics.Images;

namespace LughSharp.Source.Graphics.G2D;

[PublicAPI]
public class Animator
{
    private readonly AssetManager _assetManager;

    // ========================================================================

    /// <summary>
    /// Creates a new instance of the <see cref="Animator"/> class.
    /// </summary>
    /// <param name="assetManager"> The asset manager to use for loading assets. </param>
    public Animator( AssetManager assetManager )
    {
        _assetManager = assetManager;
    }

    /// <summary>
    /// Creates an animation from a TextureAtlas.
    /// </summary>
    /// <param name="atlasPath">
    /// The path to the <see cref="TextureAtlas"/> holding the image from which
    /// to create the animation.
    /// </param>
    /// <param name="animationName">
    /// The name of the atlas region. This name must match the region name in the atlas.
    /// </param>
    /// <param name="frameWidth"> The with of each frame, in pixels. </param>
    /// <param name="frameHeight"> The height of each frame, in pixels. </param>
    /// <param name="frameDuration"> The animation speed. </param>
    /// <param name="playMode"></param>
    /// <returns></returns>
    public Animation< TextureRegion >? CreateAnimation( string atlasPath,
                                                        string animationName,
                                                        int frameWidth,
                                                        int frameHeight,
                                                        float frameDuration = 1.0f,
                                                        AnimationMode playMode = AnimationMode.Loop )
    {
        var          path        = $"{atlasPath}";
        var          assetAtlas  = _assetManager.Get< TextureAtlas >( path, false );
        AtlasRegion? atlasRegion = assetAtlas?.FindRegion( animationName );

        if ( atlasRegion != null )
        {
            TextureRegion[] splits     = atlasRegion.SplitInto( frameWidth, frameHeight );
            var             animFrames = new TextureRegion[ splits.Length ];

            Array.Copy( splits, animFrames, splits.Length );

            var animation = new Animation< TextureRegion >( frameDuration / 6.0f, animFrames )
            {
                PlayMode = playMode,
            };

            return animation;
        }

        return null;
    }

    /// <summary>
    /// Creates a new animation using the specified texture region and parameters.
    /// </summary>
    /// <param name="asset">The texture region to use for generating animation frames.</param>
    /// <param name="frameWidth">The width of each frame in the animation.</param>
    /// <param name="frameHeight">The height of each frame in the animation.</param>
    /// <param name="frameDuration">The duration of each frame in seconds. Default is 1.0f.</param>
    /// <param name="playMode">
    /// The playback mode of the animation (e.g., loop, reversed). Default is <see cref="AnimationMode.Loop"/>.
    /// </param>
    /// <returns>
    /// A new instance of <see cref="Animation{T}"/> with the specified parameters, or
    /// null if the operation fails.
    /// </returns>
    public Animation< TextureRegion > CreateAnimation( TextureRegion asset,
                                                        int frameWidth,
                                                        int frameHeight,
                                                        float frameDuration = 1.0f,
                                                        AnimationMode playMode = AnimationMode.Loop )
    {
        Guard.Against.Null( asset );

        TextureRegion[] splits     = asset.SplitInto( frameWidth, frameHeight );
        var             animFrames = new TextureRegion[ splits.Length ];

        Array.Copy( splits, animFrames, splits.Length );

        var animation = new Animation< TextureRegion >( frameDuration / 6f, animFrames )
        {
            PlayMode = playMode
        };

        return animation;
    }

    /// <summary>
    /// Retrieves the appropriate animation frame based on the elapsed animation time
    /// and whether the animation is looping.
    /// </summary>
    /// <param name="animation">The animation object containing the frames.</param>
    /// <param name="elapsedAnimTime">The elapsed time of the animation in seconds.</param>
    /// <param name="looping">Indicates whether the animation should loop.</param>
    /// <returns>Returns the texture region representing the current frame of the animation.</returns>
    public TextureRegion NextFrame( Animation< TextureRegion > animation,
                                    float elapsedAnimTime,
                                    bool looping )
    {
        return animation.GetKeyFrame( elapsedAnimTime, looping );
    }

    /// <summary>
    /// Randomizes the animation time based on a given delta value and a multiplier.
    /// </summary>
    /// <param name="animTime">The current animation time to be randomized.</param>
    /// <param name="delta">The delta value used as a multiplier for randomness.</param>
    /// <returns>A new randomized animation time.</returns>
    public float RandomiseAnimTime( float animTime, float delta )
    {
        return delta * MathUtils.Random( 10 );
    }
}

// ============================================================================
// ============================================================================
