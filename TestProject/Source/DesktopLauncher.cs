using System;
using System.IO;
using System.Runtime.Versioning;

using DesktopGLBackend;

using Extensions.Source.Tools.TexturePacker;

using LughSharp.Source;
using LughSharp.Source.IO;

namespace TestProject.Source;

/// <summary>
/// Example DesktopLauncher class which is the entry point for the desktop application.
/// </summary>
[SupportedOSPlatform( "windows" )]
public static class DesktopLauncher
{
    // Constants for use with the BuildTextureAtlases() method.
    private const bool BuildAtlases          = true;
    private const bool RemoveDuplicateImages = false;
    private const bool DrawDebugLines        = false;

    // ========================================================================

    /// <summary>
    /// Entry point for the desktop application. Initializes the application
    /// configuration information and launches the main game loop.
    /// </summary>
    [STAThread]
    private static void Main( string[] args )
    {
        var config = new DesktopGLApplicationConfiguration
        {
            Title              = "LughSharp TestProject",
            VSyncEnabled       = true,
            WindowWidth        = 960,
            WindowHeight       = 720,
            ForegroundFPS      = 60,
            DisableAudio       = true,
            Debug              = true,
            GLProfilingEnabled = true
        };

        Engine.CheckEnableDevMode();
        Engine.CheckEnableGodMode();

        BuildTextureAtlases();

        var game = new DesktopGLApplication( new MainGame(), config );

        game.Run();
    }

    private static void BuildTextureAtlases()
    {
        if ( !BuildAtlases )
        {
            return;
        }

        var settings = new TexturePackerSettings
        {
            MaxWidth         = 2048,                  // Maximum Width of final atlas image
            MaxHeight        = 2048,                  // Maximum Height of final atlas image
            PowerOfTwo       = false,                  // 
            Debug            = DrawDebugLines,        // 
            IsAlias          = RemoveDuplicateImages, // 
            StripWhitespaceX = false,                 //
            StripWhitespaceY = false,                 //
        };

        // Build the Atlases from the specified parameters :-
        // 1. source folder
        // 2. destination folder
        // 3. name of atlas, without extension (the extension '.atlas' will be added
        //    automatically). If an extension is specified, it will be removed.
        // 4. configuration settings
        string inputFolder  = Files.AssetPath( @"Assets\PackedImages\objects" );
        string outputFolder = Files.AssetPath( @"Assets\PackedImages\output" );

        // Make sure we have a settings file to use. Comment out if not needed,
        // or to test with default settings.
        string settingsFilePath = Path.Combine( inputFolder, "pack.json" );
        settings.WriteToJsonFile( settingsFilePath );

        TexturePacker.Process( Files.AssetPath( @"Assets\PackedImages\animations" ),
                               outputFolder,
                               "animations",
                               settings );
        TexturePacker.Process( Files.AssetPath( @"Assets\PackedImages\input" ),
                               outputFolder,
                               "input",
                               settings );
        TexturePacker.Process( Files.AssetPath( @"Assets\PackedImages\objects" ),
                               outputFolder,
                               "objects",
                               settings );
        TexturePacker.Process( Files.AssetPath( @"Assets\PackedImages\text" ),
                               outputFolder,
                               "text",
                               settings );
    }
}

// ============================================================================
// ============================================================================