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

using LughSharp.Source.Graphics.Packing.TiledMapPacker;
using LughSharp.Source.IO;

namespace LughSharp.Tests;

/// <summary>
/// Processes the maps located in <c>{ContentRoot}/maps/source:</c>. Creates the
/// directory <c>{ContentRoot}/maps/processed</c> which contains processed maps.
/// Run <see cref="TiledMapPackerTestRenderer"/> to render the maps and, optionally,
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

    // ========================================================================

    public static void Run( string[] args )
    {
        const string VerboseOpt = "-v";
        const string Unused     = "--strip-unused";
        const string Combine    = "--combine-tilesets";
        const string BadOpt     = "bad";

        var path   = @$"{Files.ContentRoot}\maps\";
        var input  = @$"{path}\source\";
        var output = @$"{path}\processed";

        // There is an optional 3rd path parameter, which is specifically meant to
        // support maps which use the custom class properties. You must specify the
        // path to the tiled project file if your files requires it. If not, that
        // map will be skipped during processing.
        // Can be tested by setting TestType testType = TestType.DefaultUsageWithProjectFile;
        string projectFilePath = input + "/tiled-prop-test.tiled-project";

        var outputDir = new FileInfo( output );

        if ( outputDir.Exists )
        {
            Directory.Delete( output, true );
            
//            // OR...Should I just delete the output folder here?
//            Console.WriteLine( "Please run TiledMapPackerTestRenderer or delete output "
//                             + $"folder located in {output}" );
//
//            return;
        }

        string[] noArgs                             = Array.Empty< string >();
        string[] defaultUsage                       = { input, output };
        string[] defaultUsageWithProjectFile        = { input, output, projectFilePath };
        string[] defaultUsageWithProjectFileVerbose = { input, output, projectFilePath, VerboseOpt };
        string[] verbose                            = { input, output, VerboseOpt };
        string[] stripUnused                        = { input, output, Unused };
        string[] combineTilesets                    = { input, output, Combine };
        string[] unusedAndCombine                   = { input, output, Unused, Combine };

        string[] badOption = { input, output, Unused, VerboseOpt, Combine, BadOpt };

        var tiledMapPacker = new TiledMapPacker();
        
        switch ( Testtype )
        {
            case TestType.NoArgs:
                tiledMapPacker.Run( noArgs );

                break;

            case TestType.DefaultUsage:
                tiledMapPacker.Run( defaultUsage );

                break;

            case TestType.DefaultUsageWithProjectFile:
                tiledMapPacker.Run( defaultUsageWithProjectFile );

                break;

            case TestType.Verbose:
                tiledMapPacker.Run( verbose );

                break;

            case TestType.DefaultUsageWithProjectFileVerbose:
                tiledMapPacker.Run( defaultUsageWithProjectFileVerbose );

                break;

            case TestType.StripUnused:
                tiledMapPacker.Run( stripUnused );

                break;

            case TestType.CombineTilesets:
                tiledMapPacker.Run( combineTilesets );

                break;

            case TestType.UnusedAndCombine:
                tiledMapPacker.Run( unusedAndCombine );

                break;

            case TestType.BadOption:
                tiledMapPacker.Run( badOption );

                break;

            default:

                break;
        }
    }
}

// ============================================================================
// ============================================================================