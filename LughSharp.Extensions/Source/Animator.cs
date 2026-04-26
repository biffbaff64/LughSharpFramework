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

using LughSharp.Source.Assets;
using LughSharp.Source.Graphics.Atlases;
using LughSharp.Source.Graphics.G2D;
using LughSharp.Source.Graphics.Images;
using LughSharp.Source.Maths;

namespace Extensions.Source;

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
    /// 
    /// </summary>
    /// <param name="animation"></param>
    /// <param name="elapsedAnimTime"></param>
    /// <param name="looping"></param>
    /// <returns></returns>
    public TextureRegion NextFrame( Animation< TextureRegion > animation,
                                    float elapsedAnimTime,
                                    bool looping )
    {
        return animation.GetKeyFrame( elapsedAnimTime, looping );
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="animTime"></param>
    /// <param name="delta"></param>
    /// <returns></returns>
    public float RandomiseAnimTime( float animTime, float delta )
    {
        return delta * MathUtils.Random( 10 );

    }
}

// ============================================================================
// ============================================================================