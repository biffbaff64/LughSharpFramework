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

using LughSharp.Source.Graphics.Atlases;
using LughSharp.Source.Graphics.G2D;

namespace LughSharp.Source.Scene2D.UI;

/// <summary>
/// ParticleEffectActor holds an ParticleEffect to use in Scene2d applications.
/// The particle effect is positioned at 0, 0 in the ParticleEffectActor. Its
/// bounding box is not limited to the size of this actor.
/// </summary>
[PublicAPI]
[ActorDefinition( Role = "UI" )]
public class ParticleEffectActor : Actor, IDisposable
{
    public          bool           IsRunning      { get; set; }
    public          bool           AutoRemove     { get; set; }
    public          ParticleEffect ParticleEffect { get; }

    // ========================================================================

    protected readonly bool  OwnsEffect;
    protected          float LastDelta;

    // ========================================================================

    private bool _resetOnStart;

    // ========================================================================

    /// <summary>
    /// Creates a new ParticleEffectActor.
    /// </summary>
    /// <param name="particleEffect"> The particle effect to use. </param>
    /// <param name="resetOnStart"> Whether to reset the particle effect on start. </param>
    public ParticleEffectActor( ParticleEffect particleEffect, bool resetOnStart )
    {
        ParticleEffect = particleEffect;
        _resetOnStart  = resetOnStart;
    }

    /// <summary>
    /// Creates a new ParticleEffectActor.
    /// </summary>
    /// <param name="particleFile"> The file containing the particle effect data. </param>
    /// <param name="atlas"> The texture atlas used to load the particle effect. </param>
    public ParticleEffectActor( FileInfo particleFile, TextureAtlas atlas )
    {
        ParticleEffect = new ParticleEffect();
        ParticleEffect.Load( particleFile, atlas );
        OwnsEffect = true;
    }

    /// <summary>
    /// Creates a new ParticleEffectActor.
    /// </summary>
    /// <param name="particleFile"> The file containing the particle effect data. </param>
    /// <param name="imagesDir"> The directory containing the particle effect images. </param>
    public ParticleEffectActor( FileInfo particleFile, DirectoryInfo imagesDir )
    {
        ParticleEffect = new ParticleEffect();
        ParticleEffect.Load( particleFile, imagesDir );
        OwnsEffect = true;
    }

    /// <summary>
    /// Draws the actor. The batch is configured to draw in the parent's coordinate system. This
    /// draw method is convenient to draw a rotated and scaled TextureRegion.
    /// <para>
    /// <see cref="IBatch.Begin"/> has already been called on the batch. If <see cref="IBatch.End()"/>
    /// is called to draw without the batch then <see cref="IBatch.Begin"/> must be called before
    /// the method returns.
    /// </para>
    /// <para>
    /// <b><c>The default implementation does nothing. Child classes should override and implement.</c></b>
    /// </para>
    /// </summary>
    /// <param name="batch"> The <see cref="IBatch"/> to use. </param>
    /// <param name="parentAlpha">
    /// The parent alpha, to be multiplied with this actor's alpha, allowing the parent's alpha to
    /// affect all children.
    /// </param>
    public override void Draw( IBatch batch, float parentAlpha )
    {
        ParticleEffect.SetPosition( GetX(), GetY() );

        if ( LastDelta > 0 )
        {
            ParticleEffect.Update( LastDelta );
            LastDelta = 0;
        }

        if ( IsRunning )
        {
            ParticleEffect.Draw( batch );
            IsRunning = !ParticleEffect.IsComplete();
        }
    }

    /// <summary>
    /// Handles all actions attached to this actor.
    /// </summary>
    /// <param name="delta"> Time in seconds since the last update. </param>
    public override void Act( float delta )
    {
        base.Act( delta );

        // don't do particleEffect.update() here - the correct position
        // is set just while we are in draw() method. We save the delta
        // here to update in draw()
        LastDelta += delta;

        if ( AutoRemove && ParticleEffect.IsComplete() )
        {
            Remove();
        }
    }

    /// <summary>
    /// Starts the particle effect actor, enabling it to run and update its associated
    /// particle effect. If the particle effect is configured to reset on start, it
    /// resets all particles before starting.
    /// </summary>
    public void Start()
    {
        IsRunning = true;

        if ( _resetOnStart )
        {
            ParticleEffect.Reset( false );
        }

        ParticleEffect.Start();
    }

    /// <summary>
    /// Determines whether the particle effect should reset when the actor starts.
    /// </summary>
    /// <returns>A boolean value indicating if the particle effect resets on start.</returns>
    public bool IsResetOnStart()
    {
        return _resetOnStart;
    }

    /// <summary>
    /// Sets whether the particle effect should reset on start.
    /// </summary>
    /// <param name="resetOnStart">
    /// Indicates whether the particle effect should reset when starting.
    /// </param>
    /// <return>
    /// Returns the current instance of <see cref="ParticleEffectActor"/> for method chaining.
    /// </return>
    public ParticleEffectActor SetResetOnStart( bool resetOnStart )
    {
        _resetOnStart = resetOnStart;

        return this;
    }

    /// <summary>
    /// Sets whether the particle effect should be automatically removed when it finishes.
    /// </summary>
    /// <param name="autoRemove">
    /// A boolean value indicating whether the particle effect should be automatically
    /// removed.
    /// </param>
    /// <returns>
    /// Returns the current instance of <see cref="ParticleEffectActor"/> for method
    /// chaining.
    /// </returns>
    public ParticleEffectActor SetAutoRemove( bool autoRemove )
    {
        AutoRemove = autoRemove;

        return this;
    }

    /// <summary>
    /// Invoked when the scale of the actor changes.
    /// Updates the particle effect's scale to match the actor's scale dimensions.
    /// </summary>
    public override void OnScaleChanged()
    {
        base.OnScaleChanged();

        ParticleEffect.ScaleEffect( ScaleX, ScaleY, ScaleY );
    }

    /// <summary>
    /// Stops the particle effect associated with this ParticleEffectActor.
    /// </summary>
    public void Cancel()
    {
        IsRunning = false;
    }

    /// <summary>
    /// Allows the particle effect to complete its active cycle without restarting.
    /// Once called, the particle effect will not emit new particles, and it will
    /// continue rendering until all currently active particles have finished.
    /// </summary>
    public void AllowCompletion()
    {
        ParticleEffect.AllowCompletion();
    }

    // ========================================================================

    #region dispose pattern

    public void Dispose()
    {
        Dispose( true );
    }

    protected void Dispose( bool disposing )
    {
        if ( disposing )
        {
            if ( OwnsEffect )
            {
                ParticleEffect.Dispose();
            }
        }
    }

    #endregion dispose pattern
}

// ============================================================================
// ============================================================================