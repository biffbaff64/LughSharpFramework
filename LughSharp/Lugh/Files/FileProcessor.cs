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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Guarding;

using static System.Text.RegularExpressions.Regex;

namespace LughSharp.Lugh.Files;

/// <summary>
/// Collects files recursively, filtering by file name. Callbacks are provided to
/// Process files and the results are collected, either <see cref="ProcessFile(Entry)" />
/// or <see cref="ProcessDir(Entry, List{Entry})" /> can be overridden, or both. The
/// entries provided to the callbacks have the original file, the output directory,
/// and the output file. If <see cref="SetFlattenOutput(bool)"/> is false, the output
/// will match the directory structure of the input.
/// <br/>
/// <para>
/// <b>Potential Areas for Improvement and Questions:</b>
/// </para>
/// <para>
/// <b>Error Handling in Process(FileSystemInfo, DirectoryInfo?):</b>
/// When inputFileOrDir is a directory, the code retrieves FileSystemInfo and casts them to
/// FileInfo. What happens if a subdirectory is encountered at the top level? It seems it would
/// be skipped in this initial call. Is that the intended behavior? Perhaps a check for
/// IsDirectory and a recursive call here as well might be needed if the initial input can be
/// a directory and recursive processing should start from there.
/// </para>
/// <br/>
/// <para>
/// <b>ProcessDir Output Directory:</b>
/// In the Process(FileInfo[], DirectoryInfo?) method, the newOutputDir for ProcessDir is
/// determined based on FlattenOutput or the OutputDirectory of the first entry in dirEntries.
/// If the entries in dirEntries logically belong to different subdirectories of the input,
/// this might lead to an incorrect OutputDirectory being set for the entry passed to ProcessDir.
/// Consider if the OutputDirectory for the Entry passed to ProcessDir should somehow reflect
/// the original input subdirectory structure.
/// </para>
/// <para>
/// <b>Handling Root Directory Input:</b>
/// If inputFileOrDir in the initial Process call is a root directory (e.g., "C:"), the file.Directory
/// in the recursive Process method might be null. The current code logs an error. Consider if a
/// different handling mechanism is needed for root directories (e.g., processing files directly
/// under the root without a parent directory concept for grouping).
/// </para>
/// <para>
/// <b>Clarity of outputDir in Recursive Calls:</b>
/// In the recursive Process call, outputDir is constructed based on file.Name. This seems correct
/// for maintaining the output directory structure. However, ensure this aligns with the overall
/// intention of FlattenOutput. If FlattenOutput is true, the outputDir in the recursive calls might
/// still be creating a subdirectory structure.
/// </para>
/// <para>
/// <b>Comparator and Directories:</b>
/// The Comparator is defined for FileInfo. The EntryComparator handles DirectoryInfo by comparing
/// names. Is there a specific requirement for sorting directories differently? If not, the default
/// name comparison seems reasonable.
/// </para>
/// <para>
/// <b>OutputFilesList Population:</b>
/// The OutputFilesList is cleared at the beginning of the Process(FileInfo[], DirectoryInfo?) method.
/// The AddProcessedFile(Entry) method adds to this list. Ensure that AddProcessedFile is called
/// appropriately within ProcessFile or elsewhere to collect the processed files. It's not explicitly
/// called within the provided Process methods.
/// </para>
/// <para>
/// <b>Thread Safety:</b>
/// If this class is intended to be used in a multi-threaded environment, consider potential thread
/// safety issues, especially with the shared InputRegex and OutputFilesList.
/// </para>
/// <para>
/// <b>Cancellation/Progress Reporting:</b>
/// For long-running operations, consider adding mechanisms for cancellation or progress reporting.
/// </para>
/// <para>
/// <b>Specific Questions:</b>
/// <li>
/// When and where is AddProcessedFile(Entry) intended to be called? It's crucial for populating the
/// OutputFilesList that the Process methods ultimately return.
/// </li>
/// <li>
/// What is the intended behavior when the initial inputFileOrDir in Process(FileSystemInfo, DirectoryInfo?)
/// is a directory, and recursive processing should occur?
/// </li>
/// <li>
/// How should root directories be handled?
/// </li>
/// <li>
/// Does FlattenOutput apply to the directory structure created during recursion, or only to the final
/// output file path within the outputRoot?
/// </li>
/// <br/>
/// </para>
/// </summary>
[PublicAPI]
public partial class FileProcessor
{
    // Delegate to filter filenames in a directory
    public Func< string, string, bool >? FilenameFilterDelegate { get; set; }

    // Delegate to signal a processed file
    public virtual Action< FileSystemInfo >? FileProcessedDelegate { get; set; }

    // Delegate to process a file
    public Action< Entry >? ProcessFileDelegate { get; set; }

    // Delegate to process a directory
    public Action< Entry, List< Entry > >? ProcessDirDelegate { get; set; }

    // ========================================================================

    public List< Regex > InputRegex      { get; } = [ ];
    public List< Entry > OutputFilesList { get; } = [ ];
    public string        OutputSuffix    { get; set; }
    public bool          Recursive       { get; set; } = true;
    public bool          FlattenOutput   { get; set; }

    // ========================================================================

    /// <summary>
    /// Default Constructor.
    /// Creates a new <c>FileProcessor</c> object.
    /// </summary>
    public FileProcessor()
    {
        InputRegex      = [ ];
        OutputFilesList = [ ];
        OutputSuffix    = string.Empty;
        FlattenOutput   = false;

        SetRecursive( true );
    }

    /// <summary>
    /// Creates a new <c>FileProcessor</c> object, using the settings provided
    /// by the supplied FileProcessor object.
    /// </summary>
    /// <param name="processor"> The FileProcessor to copy. </param>
    public FileProcessor( FileProcessor processor )
    {
        Comparator = processor.Comparator;

        InputRegex.AddRange( processor.InputRegex );

        OutputSuffix  = processor.OutputSuffix;
        Recursive     = processor.Recursive;
        FlattenOutput = processor.FlattenOutput;
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="inputFileOrDir"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>
    public List< Entry > Process( string inputFileOrDir, string? outputRoot )
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
    public virtual List< Entry > Process( FileSystemInfo inputFileOrDir, DirectoryInfo? outputRoot )
    {
        if ( !inputFileOrDir.Exists )
        {
            throw new ArgumentException( $"FileProcessor#Process: Input file does not exist: {inputFileOrDir.FullName}" );
        }

        List< Entry > retval;

        if ( IOData.IsFile( inputFileOrDir ) )
        {
            retval = Process( [ ( FileInfo )inputFileOrDir ], outputRoot );
        }
        else
        {
            var files = new DirectoryInfo( inputFileOrDir.FullName )
                        .GetFileSystemInfos().Select( f => new FileInfo( f.FullName ) ).ToArray();

            retval = Process( files, outputRoot );
        }

        return retval;
    }

    /// <summary>
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>+
    /// <exception cref="Exception"></exception>
    public List< Entry > Process( FileInfo[] files, DirectoryInfo? outputRoot )
    {
        Logger.Checkpoint();
        Logger.Debug( $"Num files: {files.Length}" );

        if ( outputRoot == null )
        {
            outputRoot = new DirectoryInfo( IOData.InternalPath );
        }

        OutputFilesList.Clear();

        var stringToEntries = new Dictionary< string, List< Entry > >();
        var allEntries      = new List< Entry >();

        Process( files, outputRoot, outputRoot, stringToEntries, 0 );

        foreach ( var (inputDir, dirEntries) in stringToEntries )
        {
            if ( Comparator != null )
            {
                dirEntries.Sort( EntryComparator );
            }

            var newOutputDir = default( DirectoryInfo );

            if ( FlattenOutput )
            {
                newOutputDir = outputRoot;
            }
            else if ( dirEntries.Count > 0 )
            {
                newOutputDir = dirEntries[ 0 ].OutputDirectory;
            }

            var outputName = inputDir;

            if ( OutputSuffix != null )
            {
                outputName = MyRegex().Replace( outputName, "$1" ) + OutputSuffix;
            }

            var entry = new Entry
            {
                InputFile       = new DirectoryInfo( inputDir ),
                OutputDirectory = newOutputDir!,
            };

            if ( newOutputDir != null )
            {
                entry.OutputFile = newOutputDir.FullName.Length == 0
                    ? new FileInfo( outputName )
                    : new FileInfo( Path.Combine( newOutputDir.FullName, outputName ) );
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
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputRoot"></param>
    /// <param name="outputDir"></param>
    /// <param name="dirToEntries"></param>
    /// <param name="depth"></param>
    private void Process( FileInfo[] files, DirectoryInfo outputRoot, DirectoryInfo outputDir,
                          Dictionary< DirectoryInfo, List< Entry > > dirToEntries, int depth )
    {
        foreach ( var file in files )
        {
            var dir = file.Directory; // Get the parent directory

            if ( dir != null )
            {
                if ( !dirToEntries.ContainsKey( dir ) )
                {
                    dirToEntries[ dir ] = [ ];
                }
            }
            else
            {
                // Handle the case where a file has no parent directory (e.g., root level)
                // Either log a warning, throw an exception, or handle it differently
                // ( Logging a warning for now... )
                Logger.Error( $"WARNING: File '{file.FullName}' has no parent directory." );
            }
        }

        foreach ( var file in files )
        {
            if ( !IOData.IsDirectory( file ) )
            {
                if ( InputRegex.Count > 0 )
                {
                    var found = false;

                    foreach ( var pattern in InputRegex )
                    {
                        if ( pattern.IsMatch( file.Name ) )
                        {
                            found = true;

                            break;
                        }
                    }

                    if ( !found )
                    {
                        continue;
                    }
                }

                if ( file.DirectoryName == null )
                {
                    Logger.Debug( $"file.DirectoryName is null: {file.FullName}" );
                }

                if ( ( FilenameFilterDelegate != null ) && !FilenameFilterDelegate( file.Directory!.FullName, file.Name ) )
                {
                    continue;
                }

                var outputName = file.Name;

                if ( OutputSuffix != null )
                {
                    outputName = MyRegex().Replace( outputName, "$1" ) + OutputSuffix;
                }

                var entry = new Entry
                {
                    Depth           = depth,
                    InputFile       = file,
                    OutputDirectory = outputDir,
                    OutputFile = FlattenOutput
                        ? new FileInfo( Path.Combine( outputRoot.FullName, outputName ) )
                        : new FileInfo( Path.Combine( outputDir.FullName, outputName ) ),
                };

                var dir = file.Directory!;

                if ( !dirToEntries.ContainsKey( dir ) )
                {
                    dirToEntries.Add( dir, [ ] );
                }

                dirToEntries[ dir ].Add( entry );
            }

            if ( Recursive && IOData.IsDirectory( file ) )
            {
                var subdir = outputDir.FullName.Length == 0
                    ? new DirectoryInfo( file.Name )
                    : new DirectoryInfo( Path.Combine( outputDir.FullName, file.Name ) );

                Process( new DirectoryInfo( file.FullName )
                         .GetFileSystemInfos().Select( f => new FileInfo( f.FullName ) ).ToArray(),
                         outputRoot, subdir, dirToEntries, depth + 1 );
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputRoot"></param>
    /// <param name="outputDir"></param>
    /// <param name="stringToEntries"></param>
    /// <param name="depth"></param>
    private void Process( FileInfo[] files, DirectoryInfo outputRoot, DirectoryInfo outputDir,
                          Dictionary< string, List< Entry > > stringToEntries, int depth )
    {
        foreach ( var file in files )
        {
            if ( ( file.Attributes & FileAttributes.Directory ) == 0 )
            {
                if ( InputRegex.Count > 0 )
                {
                    var found = false;

                    foreach ( var pattern in InputRegex )
                    {
                        if ( pattern.IsMatch( file.Name ) )
                        {
                            found = true;

                            break;
                        }
                    }

                    if ( !found )
                    {
                        continue;
                    }
                }

                if ( file.DirectoryName == null )
                {
                    Logger.Debug( $"file.DirectoryName is null: {file.FullName}" );
                }

                if ( ( FilenameFilterDelegate != null ) && !FilenameFilterDelegate( file.Directory!.FullName, file.Name ) )
                {
                    continue;
                }

                var outputName = file.Name;

                if ( OutputSuffix != null )
                {
                    outputName = MyRegex().Replace( outputName, "$1" ) + OutputSuffix;
                }

                var entry = new Entry
                {
                    Depth           = depth,
                    InputFile       = file,
                    OutputDirectory = outputDir,

                    OutputFile = new FileInfo( Path.Combine( FlattenOutput
                                                                 ? outputRoot.FullName
                                                                 : outputDir.FullName,
                                                             outputName ) ),
                };

                var dir = file.Directory!.FullName;

                if ( !stringToEntries.ContainsKey( dir ) )
                {
                    stringToEntries.Add( dir, [ ] );
                }

                stringToEntries[ dir ].Add( entry );
            }

            if ( Recursive && ( ( file.Attributes & FileAttributes.Directory ) != 0 ) )
            {
                var subdir = outputDir.FullName.Length == 0
                    ? new DirectoryInfo( file.Name )
                    : new DirectoryInfo( Path.Combine( outputDir.FullName, file.Name ) );

                Process( new DirectoryInfo( file.FullName )
                         .GetFileSystemInfos().Select( f => new FileInfo( f.FullName ) ).ToArray(),
                         outputRoot, subdir, stringToEntries, depth + 1 );
            }
        }

        // Note:
        // To get the count of files stores in the List< Entry >, use something like:-
        // var totalEntries = stringToEntries.Values.Sum(list => list.Count);
    }

    /// <summary>
    /// </summary>
    /// <param name="entry"></param>
    public virtual void ProcessFile( Entry entry )
    {
        Logger.Checkpoint();

        Guard.ThrowIfNull( entry.InputFile );

        FileProcessedDelegate?.Invoke( ( FileInfo )entry.InputFile );
        ProcessFileDelegate?.Invoke( entry );
    }

    /// <summary>
    /// </summary>
    /// <param name="entryDir"></param>
    /// <param name="files"></param>
    public virtual void ProcessDir( Entry entryDir, List< Entry > files )
    {
        Logger.Checkpoint();

        if ( ProcessDirDelegate == null )
        {
            Logger.Debug( "ProcessDirDelegate is not set!" );

            return;
        }

        ProcessDirDelegate.Invoke( entryDir, files );
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="regexes"></param>
    /// <returns> This FileProcessor for chaining. </returns>
    public FileProcessor AddInputRegex( params string[] regexes )
    {
        foreach ( var regex in regexes )
        {
            InputRegex.Add( new Regex( regex ) );
        }

        return this;
    }

    /// <summary>
    /// Adds a case insensitive suffix for matching input files.
    /// </summary>
    /// <param name="suffixes"></param>
    /// <returns> This FileProcessor for chaining. </returns>
    public FileProcessor AddInputSuffix( params string[] suffixes )
    {
        foreach ( var suffix in suffixes )
        {
            AddInputRegex( $"(?i).*{Escape( suffix )}" );
        }

        return this;
    }

    /// <summary>
    /// Sets the suffix for output files, replacing the extension of the input file.
    /// </summary>
    /// <param name="outputSuffix"></param>
    /// <returns> This FileProcessor for chaining. </returns>
    public FileProcessor SetOutputSuffix( string outputSuffix )
    {
        OutputSuffix = outputSuffix;

        return this;
    }

    /// <summary>
    /// </summary>
    /// <param name="flattenOutput"></param>
    /// <returns> This FileProcessor for chaining. </returns>
    public FileProcessor SetFlattenOutput( bool flattenOutput )
    {
        FlattenOutput = flattenOutput;

        return this;
    }

    /// <summary>
    /// </summary>
    /// <param name="recursive"></param>
    /// <returns> This FileProcessor for chaining. </returns>
    public FileProcessor SetRecursive( bool recursive )
    {
        Recursive = recursive;

        return this;
    }

    /// <summary>
    /// Sets the comparator for <see cref="ProcessDir(Entry, List{Entry})"/>.
    /// By default the files are sorted by alpha.
    /// </summary>
    public FileProcessor SetComparator( Comparison< FileInfo > comparator )
    {
        this.Comparator = comparator;

        return this;
    }

    /// <summary>
    /// Adds a processed <see cref="Entry"/> to the <see cref="OutputFilesList"/>.
    /// This method should be called by:-
    /// <li><see cref="ProcessFile(Entry)"/> or,</li>
    /// <li><see cref="ProcessDir(Entry, List{Entry})"/></li>
    /// if the return value of <see cref="Process(FileSystemInfo, DirectoryInfo)"/> or
    /// <see cref="Process(FileInfo[], DirectoryInfo)"/> should return all the processed
    /// files.
    /// </summary>
    /// <param name="entry"></param>
    public virtual void AddProcessedFile( Entry entry )
    {
        OutputFilesList.Add( entry );
    }

    // ========================================================================

    public Comparison< FileInfo > Comparator { get; set; } = ( o1, o2 ) =>
        string.Compare( o1.Name, o2.Name, StringComparison.Ordinal );

    [SuppressMessage( "ReSharper", "ConvertIfStatementToSwitchStatement" )]
    private Comparison< Entry > EntryComparator
    {
        get =>
            ( o1, o2 ) =>
            {
                Guard.ThrowIfNull( o1.InputFile, o2.InputFile );

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
        public FileInfo? OutputFile { get; set; } = null!;

        /// <summary>
        /// The output directory, where the final output file will be stored.
        /// </summary>
        public DirectoryInfo? OutputDirectory { get; set; } = null!;

        /// <summary>
        /// 
        /// </summary>
        public int Depth { get; set; }

        // ====================================================================

        public void Debug()
        {
            Logger.Debug( $"InputFile      : {InputFile?.FullName}" );
            Logger.Debug( $"OutputFile     : {OutputFile?.FullName}" );
            Logger.Debug( $"OutputDirectory: {OutputDirectory?.FullName}" );
            Logger.Debug( $"Depth          : {Depth}" );
        }
    }

    // ========================================================================

    [GeneratedRegex( "(.*)\\..*" )]
    private static partial Regex MyRegex();

    // ========================================================================
}