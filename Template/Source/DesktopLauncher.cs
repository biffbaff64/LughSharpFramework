using System;
using System.IO;
using System.Runtime.Versioning;

using DesktopGLBackend;

using Extensions.Source.Tools.TexturePacker;

using LughSharp.Core.Files;
using LughSharp.Core.Main;

namespace Template.Source;

/// <summary>
/// Example DesktopLauncher class which is the entry point for the desktop application.
/// </summary>
[SupportedOSPlatform( "windows" )]
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
            Title              = "LughSharp Template",
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

    private const bool BuildAtlases           = false;
    private const bool RemoveDuplicateImages = true;
    private const bool KeepDuplicateImages   = false;
    private const bool DrawDebugLines        = false;

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
            PowerOfTwo       = true,                  // 
            Debug            = DrawDebugLines,      // 
            IsAlias          = KeepDuplicateImages, // 
            Silent           = false,                 // 
            PaddingX         = 0,                     // Increase padding
            PaddingY         = 0,                     // Increase padding
            EdgePadding      = true,                  // Disable edge padding initially
            DuplicatePadding = false,                 // Disable duplicate padding
            MinWidth         = 16,                    // 
            MinHeight        = 16,                    // 
            Grid             = false                  // Use GridPacker over MaxRectsPacker
        };

        // Build the Atlases from the specified parameters :-
        // 1. source folder
        // 2. destination folder
        // 3. name of atlas, without extension (the extension '.atlas' will be added
        //    automatically). If an extension is specified, it will be removed.
        // 4. configuration settings
        string inputFolder  = IOUtils.AssetPath( @"Assets\PackedImages\objects" );
        string outputFolder = IOUtils.AssetPath( @"Assets\PackedImages\output" );

        // Make sure we have a settings file to use. Comment out if not needed,
        // or to test with default settings.
        string settingsFilePath = Path.Combine( inputFolder, "pack.json" );
        settings.WriteToJsonFile( settingsFilePath );

        TexturePacker.Process( IOUtils.AssetPath( @"Assets\PackedImages\animations" ),
                               outputFolder,
                               "animations",
                               settings );
        TexturePacker.Process( IOUtils.AssetPath( @"Assets\PackedImages\input" ), outputFolder, "input", settings );
        TexturePacker.Process( IOUtils.AssetPath( @"Assets\PackedImages\objects" ), outputFolder, "objects", settings );
        TexturePacker.Process( IOUtils.AssetPath( @"Assets\PackedImages\text" ), outputFolder, "text", settings );
    }
}

// ============================================================================
// ============================================================================