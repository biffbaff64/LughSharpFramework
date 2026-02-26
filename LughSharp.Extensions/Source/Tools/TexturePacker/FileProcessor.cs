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

using System.Text.RegularExpressions;
using JetBrains.Annotations;
using LughSharp.Core.Files;
using LughSharp.Core.Graphics.Text;
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
public class FileProcessor
{
    public const string DEFAULT_PACKFILE_NAME = "pack.atlas";

    // Delegate to signal a processed file
    public virtual Action< FileSystemInfo >? FileProcessedDelegate { get; set; }

    // Delegate to process a file
    public Action< Entry >? ProcessFileDelegate { get; set; }

    // Delegate to process a directory
    public Action< Entry, List< Entry > >? ProcessDirDelegate { get; set; }

    // ========================================================================

    public IFilenameFilter? InputFilter     { get; set; }
    public List< Regex >    InputRegex      { get; set; }
    public List< Entry >    OutputFilesList { get; set; }
    public string           OutputSuffix    { get; set; }
    public bool             Recursive       { get; set; }
    public bool             FlattenOutput   { get; set; }

    // ========================================================================

    public Comparison< FileInfo > Comparator { get; set; } = ( o1, o2 ) =>
        string.Compare( o1.Name, o2.Name, StringComparison.Ordinal );

    public Comparison< Entry > EntryComparator
    {
        get =>
            ( o1, o2 ) =>
            {
                Guard.Against.Null( o1.InputFile );
                Guard.Against.Null( o2.InputFile );

                if ( o1.InputFile is FileInfo file1 && o2.InputFile is FileInfo file2 )
                {
                    return Comparator( file1, file2 );
                }

                if ( o1.InputFile is DirectoryInfo dir1 && o2.InputFile is DirectoryInfo dir2 )
                {
                    return string.Compare( dir1.Name, dir2.Name, StringComparison.Ordinal );
                }

                return string.Compare( o1.InputFile?.Name, o2.InputFile?.Name, StringComparison.Ordinal );
            };
        set => throw new NotImplementedException();
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// Default Constructor.
    /// Creates a new <c>IFileProcessor</c> object.
    /// </summary>
    public FileProcessor()
    {
        InputRegex      = [ ];
        OutputFilesList = [ ];
        OutputSuffix    = string.Empty;
        FlattenOutput   = false;
        Recursive       = true;
    }

    /// <summary>
    /// Copy Constructor.
    /// Creates a new <see cref="FileProcessor"/> object as a copy of the specified FileProcessor.
    /// </summary>
    public FileProcessor( FileProcessor processor )
    {
        InputFilter = processor.InputFilter;
        Comparator  = processor.Comparator;

        InputRegex ??= [ ];
        InputRegex.AddAll( processor.InputRegex );

        OutputFilesList = processor.OutputFilesList;
        OutputSuffix    = processor.OutputSuffix;
        Recursive       = processor.Recursive;
        FlattenOutput   = processor.FlattenOutput;
    }

    // ========================================================================

    #region process methods

    /// <summary>
    /// Processes texture packing based on the specified input and output parameters.
    /// </summary>
    /// <param name="inputFileOrDir">The input file or directory to be processed.</param>
    /// <param name="outputRoot">
    /// The root output directory where the results will be saved. Can be null.
    /// </param>
    /// <returns>
    /// A list of <see cref="FileProcessor.Entry"/> representing the processed textures or files.
    /// </returns>
    public virtual List< Entry > Process( string inputFileOrDir, string? outputRoot )
    {
        return Process( new DirectoryInfo( inputFileOrDir ),
                        outputRoot == null ? null : new DirectoryInfo( outputRoot ) );
    }

    /// <summary>
    /// Processes the specified input file or directory.
    /// </summary>
    /// <param name="inputFileOrDir"></param>
    /// <param name="outputRoot"> May be null if there is no output from processing the files. </param>
    /// <returns> the processed files added with <see cref="AddProcessedFile(Entry)"/>. </returns>
    public virtual List< Entry > Process( FileSystemInfo? inputFileOrDir, DirectoryInfo? outputRoot )
    {
        if ( inputFileOrDir is not { Exists: true } )
        {
            throw new ArgumentException( $"Input file/dir does not exist: {inputFileOrDir?.FullName}" );
        }

        FileInfo[]? fileList;

        if ( inputFileOrDir is FileInfo file )
        {
            fileList = [ file ];

            Guard.Against.Null( fileList, $"Could not get file list from file: {file.FullName}" );
        }
        else
        {
            fileList = ( inputFileOrDir as DirectoryInfo )?.GetFiles();

            Guard.Against.Null( fileList, $"Could not get files from directory: {inputFileOrDir.FullName}" );
        }

        var result = Process( fileList, outputRoot );

        return result;
    }

    /// <summary>
    /// Processes the specified input files.
    /// </summary>
    /// <param name="files">The array of input files to process.</param>
    /// <param name="outputRoot">The root directory for output. Can be null.</param>
    /// <returns>A list of processed files.</returns>
    public virtual List< Entry > Process( FileInfo[] files, DirectoryInfo? outputRoot )
    {
        outputRoot ??= new DirectoryInfo( "." );

        OutputFilesList.Clear();

        var dirToEntries = new Dictionary< DirectoryInfo, List< Entry >? >( new DirectoryInfoComparer() );

        Process( files, outputRoot, outputRoot, dirToEntries, 0 );

        var allEntries = new List< Entry >();

        foreach ( var mapEntry in dirToEntries )
        {
            var dirEntries = mapEntry.Value;

            if ( dirEntries == null )
            {
                continue;
            }

            if ( Comparator != null )
            {
                dirEntries.Sort( EntryComparator );
            }

            var inputDir     = mapEntry.Key;
            var newOutputDir = default( DirectoryInfo? );

            if ( FlattenOutput )
            {
                newOutputDir = outputRoot;
            }
            else if ( dirEntries.Count != 0 )
            {
                newOutputDir = dirEntries.First().OutputDirectory;
            }

            var outputName = inputDir.Name;

            if ( OutputSuffix != null )
            {
                var match = RegexUtils.MatchFinalFolderPatternRegex().Match( outputName );

                if ( match.Success )
                {
                    outputName = match.Groups[ 1 ].Value + OutputSuffix;
                }
            }

            var entry = new Entry
            {
                InputFile       = inputDir,
                OutputDirectory = newOutputDir,
            };

            if ( newOutputDir != null )
            {
                entry.OutputFileName = newOutputDir.FullName.Length == 0
                    ? outputName
                    : Path.Combine( newOutputDir.FullName, outputName );
            }

            try
            {
                ProcessDir( entry, dirEntries );
            }
            catch ( Exception ex )
            {
                throw new Exception( $"Error processing directory: {entry.InputFile.FullName}", ex );
            }

            allEntries.AddRange( dirEntries );
        }

        if ( Comparator != null )
        {
            allEntries.Sort( EntryComparator );
        }

        foreach ( var entry in allEntries )
        {
            try
            {
                ProcessFile( entry );
            }
            catch ( Exception ex )
            {
                throw new Exception( $"Error processing file: {entry.InputFile?.FullName}", ex );
            }
        }

        return OutputFilesList;
    }

    /// <summary>
    /// Processes the given array of files starting from a specific output directory
    /// and populates a mapping of directories to their corresponding entries. Handles
    /// recursive traversal and processing based on the specified depth.
    /// </summary>
    /// <param name="files">
    /// An array of <see cref="FileInfo"/> objects representing the files to be processed.
    /// </param>
    /// <param name="outputRoot">
    /// The root output directory where the processed files will be stored.
    /// </param>
    /// <param name="outputDir">
    /// The currently targeted output directory during the processing operation.
    /// </param>
    /// <param name="dirToEntries">
    /// A dictionary mapping each discovered directory to a list of associated
    /// <see cref="Entry"/> objects.
    /// </param>
    /// <param name="depth">
    /// The current recursion depth during processing, used to manage directory
    /// hierarchy and traversal.
    /// </param>
    public virtual void Process( FileInfo[] files, DirectoryInfo outputRoot, DirectoryInfo outputDir,
                                 Dictionary< DirectoryInfo, List< Entry >? > dirToEntries, int depth )
    {
        // Store empty entries for every directory.
        foreach ( var file in files )
        {
            var dir = file.Directory;

            if ( dir == null )
            {
                // Log a warning if a file has no parent directory
                Logger.Error( $"File '{file.FullName}' has no parent directory." );
            }
            else
            {
                if ( !dirToEntries.ContainsKey( dir ) )
                {
                    // For the first file, the key is added.
                    // For files 2 onwards, the key is FOUND because the path is the same.
                    dirToEntries.Add( dir, [ ] );
                }
            }
        }

        foreach ( var file in files )
        {
            // Only process files, not directories
            if ( ( file.Attributes & FileAttributes.Directory ) != 0 )
            {
                // Skip directories for now, only process files here
                continue;
            }

            // Apply input regex filters if any are set
            if ( InputRegex.Count > 0 )
            {
                var found = InputRegex.Any( pattern => pattern.IsMatch( file.Name ) );

                if ( !found )
                {
                    continue;
                }
            }

            // Apply filename filter delegate if set
            if ( ( InputFilter != null ) && !InputFilter.Accept( file.Directory!, file.Name ) )
            {
                continue;
            }

            // Determine output file name, applying suffix if needed
            var outputName = file.Name;

            if ( OutputSuffix != null )
            {
                outputName = RegexUtils.FileNameWithoutExtensionRegex()
                                       .Replace( outputName, "$1" ) + OutputSuffix;
            }

            // Create an entry for the file
            var entry = new Entry
            {
                Depth           = depth,
                InputFile       = file,
                OutputDirectory = outputDir,
                OutputFileName = FlattenOutput
                    ? Path.Combine( outputRoot.FullName, outputName )
                    : Path.Combine( outputDir.FullName, outputName ),
            };

            var dir = file.Directory!;

            if ( dirToEntries.TryGetValue( dir, out var value ) )
            {
                value?.Add( entry );
            }

            // Recursively process directories if enabled
            if ( Recursive )
            {
                // We need to iterate over the contents of the starting directory,
                // getting only the subdirectories.

                // Get the directory containing these files (assuming a non-flat structure)
                var inputDir = files.FirstOrDefault( f => f.Directory != null )?.Directory;

                if ( inputDir != null )
                {
                    var subdirectories = inputDir.GetDirectories();

                    foreach ( var subdirInfo in subdirectories )
                    {
                        var subdirOutput = outputDir.FullName.Length == 0
                            ? new DirectoryInfo( subdirInfo.Name )
                            : new DirectoryInfo( Path.Combine( outputDir.FullName, subdirInfo.Name ) );

                        // Recursively process the subdirectory's files
                        Process( subdirInfo.GetFiles(),
                                 outputRoot,
                                 subdirOutput,
                                 dirToEntries,
                                 depth + 1 );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Processes a single file entry for texture packing. Determines the parent directory,
    /// retrieves the appropriate settings, creates a new <see cref="TexturePacker"/> instance,
    /// and performs packing if not in count-only mode. Adds the processed file to the result list.
    /// </summary>
    /// <param name="entry">The file entry to process.</param>
    /// <remarks> This default implementation does nothing, and should be overriden where necessary. </remarks>
    public virtual void ProcessFile( Entry entry )
    {
    }

    /// <summary>
    /// Called for each input directory. The files will be sorted. The specified files list can
    /// be modified to change which files are processed.
    /// </summary>
    /// <param name="entryDir"> The input directory. </param>
    /// <param name="files"> The destination for processed files. </param>
    /// <remarks> This default implementation does nothing, and should be overriden where necessary. </remarks>
    public virtual void ProcessDir( Entry entryDir, List< Entry > files )
    {
    }

    #endregion process methods

    // ========================================================================

    /// <summary>
    /// Adds a processed <see cref="Entry"/> to the <see cref="OutputFilesList"/>.
    /// This method should be called by:-
    /// <li><see cref="ProcessFile(Entry)"/> or,</li>
    /// <li><see cref="ProcessDir(Entry, List{Entry})"/></li>
    /// if the return value of <see cref="System.Diagnostics.Process"/> or
    /// <see cref="Process(FileInfo[], DirectoryInfo)"/> should return all the processed
    /// files.
    /// </summary>
    /// <param name="entry"></param>
    public virtual void AddProcessedFile( Entry entry )
    {
        OutputFilesList.Add( entry );
    }

    /// <summary>
    /// Adds a case insensitive suffix for matching input files.
    /// </summary>
    /// <param name="suffixes"></param>
    public void AddInputSuffix( params string[] suffixes )
    {
        foreach ( var suffix in suffixes )
        {
            AddInputRegex( $"(?i).*{Regex.Escape( suffix )}" );
        }
    }

    /// <summary>
    /// Adds a regex filter, or group of filters, to the <see cref="InputRegex"/>
    /// list of filters.
    /// </summary>
    /// <param name="regexes"> One or more Regex strings. </param>
    /// <returns> This IFileProcessor for chaining. </returns>
    public virtual void AddInputRegex( params string[] regexes )
    {
        foreach ( var regex in regexes )
        {
            InputRegex.Add( new Regex( regex ) );
        }
    }
    
    // ========================================================================
    // ========================================================================
    
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
        public FileSystemInfo? InputFile { get; set; }

        /// <summary>
        /// The name of the final output file.
        /// </summary>
        public string? OutputFileName { get; set; }

        /// <summary>
        /// The output directory, where the final output file will be stored.
        /// </summary>
        public DirectoryInfo? OutputDirectory { get; set; }

        /// <summary>
        /// The nesting depth of the folder.
        /// </summary>
        public int Depth { get; set; }

        // ====================================================================

        /// <summary>
        /// Logs debugging information. Handles different parameter types and outputs
        /// their details to the debug logger.
        /// </summary>
        /// <param name="args">
        /// An optional array of objects to log. Supports tuples in the formats
        /// (string, string), (string, FileInfo), and (string, DirectoryInfo). If no
        /// arguments are provided, the default debug information is logged.
        /// </param>
        public virtual void DebugPrint( params object?[]? args )
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

            Logger.Debug( $"InputFile      : {InputFile?.FullName ?? "null"}" );
            Logger.Debug( $"OutputFileName : {OutputFileName ?? "null"}" );
            Logger.Debug( $"OutputDirectory: {OutputDirectory?.FullName ?? "null"}" );
            Logger.Debug( $"Depth          : {Depth}" );
        }
    }
}

// ============================================================================
// ============================================================================