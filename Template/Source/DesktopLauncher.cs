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
            GLProfilingEnabled = true,
        };

        Engine.Api.CheckEnableDevMode().CheckEnableGodMode();

        BuildTextureAtlases();

        var game = new DesktopGLApplication( new MainGame(), config );

        game.Run();
    }

    private const bool BUILD_ATLASES           = false;
    private const bool REMOVE_DUPLICATE_IMAGES = true;
    private const bool KEEP_DUPLICATE_IMAGES   = false;
    private const bool DRAW_DEBUG_LINES        = false;

    private static void BuildTextureAtlases()
    {
        if ( !BUILD_ATLASES )
        {
            return;
        }

        var settings = new TexturePackerSettings
        {
            MaxWidth         = 2048,                  // Maximum Width of final atlas image
            MaxHeight        = 2048,                  // Maximum Height of final atlas image
            PowerOfTwo       = true,                  // 
            Debug            = DRAW_DEBUG_LINES,      // 
            IsAlias          = KEEP_DUPLICATE_IMAGES, // 
            Silent           = false,                 // 
            PaddingX         = 2,                     // Increase padding
            PaddingY         = 2,                     // Increase padding
            EdgePadding      = true,                  // Disable edge padding initially
            DuplicatePadding = false,                 // Disable duplicate padding
            MinWidth         = 16,                    // 
            MinHeight        = 16,                    // 
            Grid             = false,                 // Use GridPacker over MaxRectsPacker
        };

        // Build the Atlases from the specified parameters :-
        // 1. source folder
        // 2. destination folder
        // 3. name of atlas, without extension (the extension '.atlas' will be added
        //    automatically). If an extension is specified, it will be removed.
        // 4. configuration settings
        var inputFolder  = IOUtils.AssetPath( @"Assets\PackedImages\objects" );
        var outputFolder = IOUtils.AssetPath( @"Assets\PackedImages\output" );

        // Make sure we have a settings file to use. Comment out if not needed,
        // or to test with default settings.
        var settingsFilePath = IOUtils.NormalizePath( Path.Combine( inputFolder, "pack.json" ) );
        settings.WriteToJsonFile( settingsFilePath );

        TexturePacker.Process( IOUtils.AssetPath( @"Assets\PackedImages\animations" ), outputFolder, "animations",
                               settings );
        TexturePacker.Process( IOUtils.AssetPath( @"Assets\PackedImages\input" ), outputFolder, "input", settings );
        TexturePacker.Process( IOUtils.AssetPath( @"Assets\PackedImages\objects" ), outputFolder, "objects", settings );
        TexturePacker.Process( IOUtils.AssetPath( @"Assets\PackedImages\text" ), outputFolder, "text", settings );
    }
}

// ============================================================================
// ============================================================================
