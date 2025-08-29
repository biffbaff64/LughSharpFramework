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

using LughSharp.Lugh.Audio;
using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics;
using LughSharp.Lugh.Graphics.OpenGL;
using LughSharp.Lugh.Input;
using LughSharp.Lugh.Network;
using LughSharp.Lugh.Utils;

using GLBindings = LughSharp.Lugh.Graphics.OpenGL.GLBindings;

namespace LughSharp.Lugh.Core;

/// <summary>
/// The Engine class serves as the central hub linking multiple interfaces and systems
/// within the framework, such as application, audio, input, files, graphics, and
/// network operations.
/// <para>
/// It provides global accessibility to these subsystems and offers essential
/// configurations and tools for managing framework behavior.
/// </para>
/// </summary>
[PublicAPI]
public class Engine
{
    public IApplication    App      { get; set; } = null!;
    public IAudio          Audio    { get; set; } = null!;
    public IInput          Input    { get; set; } = null!;
    public IFiles          Files    { get; set; } = null!;
    public IGraphicsDevice Graphics { get; set; } = null!;
    public INet            Net      { get; set; } = null!;

    // ========================================================================

    /// <summary>
    /// Test mode flag which, when TRUE, means that all developer options are enabled.
    /// This must, however, mean that software with this enabled cannot be published.
    /// </summary>
    public bool DevMode { get; set; } = false;

    /// <summary>
    /// From Wiktionary...
    /// <para>
    /// "1. (video games) A game mode where the player character is invulnerable to
    /// damage, typically activated by entering a cheat code."
    /// </para>
    /// <para>
    /// "2. (video games) A mode of play in (mostly) roguelike games, allowing the
    /// player to create objects on demand, to be resurrected in the case of death,
    /// etc."
    /// </para>
    /// <para>
    /// Note: Only the flag is provided by this library. It is intended for use in
    /// local game code.
    /// </para>
    /// </summary>
    public bool GodMode { get; set; } = false;

    // ========================================================================

    public static Engine      Api => Nested.Instance;
    public static IGLBindings GL  => Nested.Instance.Bindings;

    // ========================================================================

    private IGLBindings? _glBindings;

    // ========================================================================

    private Engine()
    {
    }

    // ========================================================================

    /// <summary>
    /// The time, in seconds, that has elapsed since the last frame was rendered.
    /// <para>
    /// This property provides a way to track time progression in your application,
    /// allowing for operations that depend on frame-to-frame timing, such as animations
    /// or physics simulations, to properly account for frame delays or variable update rates.
    /// </para>
    /// <para>
    /// The underlying value is retrieved from the associated <see cref="Graphics"/> instance.
    /// </para>
    /// </summary>
    public float DeltaTime => Graphics.DeltaTime;

    /// <summary>
    /// Globally accessible instance of classes inheriting from the <see cref="IGLBindings" />
    /// interface. Initially initialised as an instance of <see cref="LughSharp.Lugh.Graphics.OpenGL.GLBindings" />, it can be
    /// modified to reference any class inheriting from IGLBindings.
    /// The property will check internally for null, and initialise itself to reference
    /// GLBindings by default if that is the case.
    /// </summary>
    public IGLBindings Bindings
    {
        get => _glBindings ??= new GLBindings();
        set => _glBindings = value;
    }

    /// <summary>
    /// Performs essential tasks, which MUST be performed to allow the
    /// framework to work correctly.
    /// </summary>
    /// <param name="app"></param>
    public void Initialise( IApplication app )
    {
        App = app;

        Logger.Initialise( enableWriteToFile: true );
        Logger.EnableDebugLogging();
        Logger.EnableErrorLogging();

        Colors.Reset();

        Bindings = new GLBindings();
    }

    /// <summary>
    /// Enables <see cref="DevMode" /> if the environment variable "DEV_MODE" is
    /// available and is set to "TRUE" or "true".
    /// </summary>
    /// <returns> This class for chaining. </returns>
    public Engine CheckEnableDevMode()
    {
        DevMode = CheckEnvironmentVar( "DEV_MODE", "TRUE" );

        if ( !DevMode )
        {
            DevMode = CheckEnvironmentVar( "DEVMODE", "TRUE" );
        }

        Logger.Debug( $"DevMode: {( DevMode ? "Enabled" : "Disabled" )}" );

        return this;
    }

    /// <summary>
    /// Enables <see cref="GodMode" /> if the environment variable "GOD_MODE" is
    /// available and is set to "TRUE" or "true".
    /// </summary>
    /// <returns> This class for chaining. </returns>
    public Engine CheckEnableGodMode()
    {
        GodMode = false;

        if ( DevMode )
        {
            GodMode = CheckEnvironmentVar( "GOD_MODE", "TRUE" );

            if ( !GodMode )
            {
                GodMode = CheckEnvironmentVar( "GODMODE", "TRUE" );
            }
        }

        Logger.Debug( $"GodMode: {( GodMode ? "Enabled" : "Disabled" )}" );

        return this;
    }

    /// <summary>
    /// Checks for OpenGL errors after a given stage of rendering or processing.
    /// Throws an <see cref="InvalidOperationException" /> if an OpenGL error is detected.
    /// </summary>
    /// <param name="stage">The description of the stage at which the error occurred.</param>
    /// <exception cref="InvalidOperationException">Thrown when an OpenGL error is detected.</exception>
    public void GLErrorCheck( string stage )
    {
        var error = GL.GetError();

        if ( error != ( int )ErrorCode.NoError )
        {
            throw new InvalidOperationException( $"OpenGL error at {stage}: {error}" );
        }
    }

    // ========================================================================
    // ========================================================================

    private static bool CheckEnvironmentVar( string envVar, string value )
    {
        return Environment.GetEnvironmentVariables().Contains( envVar )
               && Environment.GetEnvironmentVariable( envVar )!
                             .Equals( value, StringComparison.CurrentCultureIgnoreCase );
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Performs Engine Shutdown operations.
    /// </summary>
    public static void Shutdown()
    {
    }

    // ========================================================================
    // ========================================================================

    // Fully Lazy instantiation.
    private class Nested
    {
        internal static readonly Engine Instance = new();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Nested()
        {
        }
    }

    // ========================================================================
    // ========================================================================

    private const string A_PRAYER_TO_THE_GODDESS =
        "Oh Blessed Goddess, enlighten what is dark in me. "
        + "Strengthen what is weak in me. "
        + "Mend what is broken in me. "
        + "Bind what is bruised in me. "
        + "Heal what is sick in me. "
        + "Revive whatever peace & love has died in me.";

    // ========================================================================
    // ========================================================================
}