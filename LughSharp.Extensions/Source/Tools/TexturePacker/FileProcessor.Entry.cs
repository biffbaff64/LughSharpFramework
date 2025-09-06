// /////////////////////////////////////////////////////////////////////////////
//  MIT License
// 
//  Copyright (c) 2024 Richard Ikin / Red 7 Projects
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

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Logging;

namespace Extensions.Source.Tools.TexturePacker;

public partial class FileProcessor
{
    /// <summary>
    /// File processing information, detauiling:-
    /// <li>The input file, or directory, to be processed.</li>
    /// <li>The name of the final output file.</li>
    /// <li>The output directory, where the final output file will be stored.</li>
    /// <li>tbc</li>
    /// </summary>
    [PublicAPI]
    public class Entry
    {
        /// <summary>
        /// The input file, or directory, to be processed.
        /// </summary>
        public FileSystemInfo? InputFile { get; set; } = null!;

        /// <summary>
        /// The name of the final output file.
        /// </summary>
        public string? OutputFileName { get; set; } = null!;

        /// <summary>
        /// The output directory, where the final output file will be stored.
        /// </summary>
        public DirectoryInfo? OutputDirectory { get; set; } = null!;

        /// <summary>
        /// The nesting depth of the folder.
        /// </summary>
        public int Depth { get; set; }

        // ====================================================================

        #if DEBUG
        /// <summary>
        /// Logs debugging information. Handles different parameter types and outputs
        /// their details to the debug logger.
        /// </summary>
        /// <param name="args">
        /// An optional array of objects to log. Supports tuples in the formats
        /// (string, string), (string, FileInfo), and (string, DirectoryInfo). If no
        /// arguments are provided, the default debug information is logged.
        /// </param>
        public virtual void Debug( params object?[]? args )
        {
            if ( args is { Length: > 0 } )
            {
                foreach ( var t in args )
                {
                    switch ( t )
                    {
                        case (string name, string value):
                            Logger.Debug( $"{name}: {value}" );

                            break;

                        case (string infoName, FileInfo info):
                            Logger.Debug( $"{infoName}: {info.Name}" );

                            break;

                        case (string dirName, DirectoryInfo directoryInfo):
                            Logger.Debug( $"{dirName}: {directoryInfo.Name}" );

                            break;

                        default:
                            if ( t != null )
                            {
                                Logger.Debug( t.ToString()! );
                            }

                            break;
                    }
                }
            }

            Logger.Debug( $"InputFile      : {InputFile?.FullName}" );
            Logger.Debug( $"OutputFile     : {OutputFileName}" );
            Logger.Debug( $"OutputDirectory: {OutputDirectory?.FullName}" );
            Logger.Debug( $"Depth          : {Depth}" );
        }
        #endif
    }
}

// ============================================================================
// ============================================================================