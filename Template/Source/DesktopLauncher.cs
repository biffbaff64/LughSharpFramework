using DesktopGLBackend;

using LughSharp.Lugh.Core;

namespace Template.Source;

/// <summary>
/// Example DesktopLauncher class which is the entry point for the desktop application.
/// </summary>
public static class DesktopLauncher
{
    /// <summary>
    /// Entry point for the desktop application. Initializes the application
    /// configuration information and launches the main game loop.
    /// </summary>
    [STAThread]
    private static void Main( string[] args )
    {
        var config = new DesktopGLApplicationConfiguration
        {
            Title                 = "LughSharp Template",
            VSyncEnabled          = true,
            WindowWidth           = 480,
            WindowHeight          = 320,
            ForegroundFPS         = 60,
            DisableAudio          = true,
            Debug                 = true,
            GLProfilingEnabled    = true,
//            GLContextMajorVersion = 3,
//            GLContextMinorVersion = 3,
//            GLContextRevision     = 0,
        };

        Engine.Api.CheckEnableDevMode().CheckEnableGodMode();

        var game = new DesktopGLApplication( new MainGame(), config );

        game.Run();
    }
}