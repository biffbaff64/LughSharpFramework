// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin
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

using JetBrains.Annotations;

using LughSharp.Core.Files;

namespace Extensions.Source.Tools.TiledMapPacker;

/// <summary>
/// Processes the maps located in <c>{ContentRoot}/maps/source:</c>. Creates the
/// directory <c>{ContentRoot}/maps/processed</c> which contains processed maps.
/// Run <see cref="TiledMapPackerTestRender"/> to render the maps and, optionally,
/// delete the created folder on exit.
/// </summary>
[PublicAPI]
public class TiledMapPackerTest
{
    // TestTypes "NoArgs" and "BadOption" do not create/process maps.
    public enum TestType
    {
        NoArgs,
        DefaultUsage,
        DefaultUsageWithProjectFile,
        Verbose,
        DefaultUsageWithProjectFileVerbose,
        StripUnused,
        CombineTilesets,
        UnusedAndCombine,
        BadOption
    }

    public static TestType Testtype { get; set; }
    
    public static void Run( string[] args )
    {
        var path       = @$"{Files.ContentRoot}\data\maps\";
        var input      = @$"{path}\source\";
        var output     = @$"{path}\processed";
        var verboseOpt = "-v";
        var unused     = "--strip-unused";
        var combine    = "--combine-tilesets";
        var badOpt     = "bad";

        // There is an optional 3rd path parameter, which is specifically meant to
        // support maps which use the custom class properties. You must specify the
        // path to the tiled project file if your files requires it. If not, that
        // map will be skipped during processing.
        // Can be tested by setting TestType testType = TestType.DefaultUsageWithProjectFile;
        string projectFilePath = input + "/tiled-prop-test.tiled-project";

        var outputDir = new FileInfo( output );

        if ( outputDir.Exists )
        {
            // OR...Should I just delete the output folder here?
            Console.WriteLine( "Please run TiledMapPackerTestRender or delete output "
                             + $"folder located in {output}" );

            return;
        }

        string[] noArgs                             = Array.Empty<string>();
        string[] defaultUsage                       = { input, output };
        string[] defaultUsageWithProjectFile        = { input, output, projectFilePath };
        string[] defaultUsageWithProjectFileVerbose = { input, output, projectFilePath, verboseOpt };
        string[] verbose                            = { input, output, verboseOpt };
        string[] stripUnused                        = { input, output, unused };
        string[] combineTilesets                    = { input, output, combine };
        string[] unusedAndCombine                   = { input, output, unused, combine };

        string[] badOption = { input, output, unused, verboseOpt, combine, badOpt };

        switch ( Testtype )
        {
            case TestType.NoArgs:
                TiledMapPacker.Run( noArgs );

                break;

            case TestType.DefaultUsage:
                TiledMapPacker.Run( defaultUsage );

                break;

            case TestType.DefaultUsageWithProjectFile:
                TiledMapPacker.Run( defaultUsageWithProjectFile );

                break;

            case TestType.Verbose:
                TiledMapPacker.Run( verbose );

                break;

            case TestType.DefaultUsageWithProjectFileVerbose:
                TiledMapPacker.Run( defaultUsageWithProjectFileVerbose );

                break;

            case TestType.StripUnused:
                TiledMapPacker.Run( stripUnused );

                break;

            case TestType.CombineTilesets:
                TiledMapPacker.Run( combineTilesets );

                break;

            case TestType.UnusedAndCombine:
                TiledMapPacker.Run( unusedAndCombine );

                break;

            case TestType.BadOption:
                TiledMapPacker.Run( badOption );

                break;

            default:

                break;
        }
    }
}

// ============================================================================
// ============================================================================