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

using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Guarding;

using static System.Text.RegularExpressions.Regex;

namespace LughSharp.Lugh.Files;

/// <summary>
/// Collects files recursively, filtering by file name. Callbacks are provided to
/// Process files and the results are collected, either <see cref="ProcessFile(Entry)" />
/// or <see cref="ProcessDir(Entry, List{Entry})" /> can be overridden, or both. The
/// entries provided to the callbacks have the original file, the output directory,
/// and the output file. If <see cref="SetFlattenOutput(bool)" /> is false, the output
/// will match the directory structure of the input.
/// </summary>
[PublicAPI]
public partial class FileProcessor
{
    public delegate bool FilenameFilter( DirectoryInfo dir, string filename );

    // ========================================================================

    public Comparison< Entry >     EntryComparator { get; set; }
    public Comparison< FileInfo >? Comparator      { get; set; }

    // ========================================================================

    private static Comparison< FileInfo? > _comparator = ( o1, o2 ) =>
        string.Compare( o1?.Name, o2?.Name, StringComparison.Ordinal );

    private static Comparison< Entry > _entryComparator = ( entry, entry1 ) =>
        _comparator( entry.InputFile, entry1.InputFile );

    // ========================================================================

    private FilenameFilter? _inputFilter;
    private List< Regex >   _inputRegex  = [ ];
    private List< Entry >   _outputFiles = [ ];
    private string          _outputSuffix;
    private bool            _recursive;
    private bool            _flattenOutput;

    // ========================================================================

    /// <summary>
    /// Default Constructor
    /// </summary>
    public FileProcessor()
    {
        Comparator      = ( o1, o2 ) => string.Compare( o1.Name, o2.Name, StringComparison.Ordinal );
        EntryComparator = ( o1, o2 ) => Comparator( o1.InputFile, o2.InputFile );

        _outputSuffix  = string.Empty;
        _flattenOutput = false;

        SetRecursive();
    }

    /// <summary>
    /// Creates a new FileProcessor instance from the supplied FileProcessor object.
    /// </summary>
    public FileProcessor( FileProcessor processor )
    {
        Comparator      = ( o1, o2 ) => string.Compare( o1.Name, o2.Name, StringComparison.Ordinal );
        EntryComparator = ( o1, o2 ) => Comparator( o1.InputFile, o2.InputFile );

        _inputFilter   = processor._inputFilter;
        _outputSuffix  = processor._outputSuffix;
        _recursive     = processor._recursive;
        _flattenOutput = processor._flattenOutput;

        _inputRegex.AddAll( processor._inputRegex );
    }

    // ========================================================================

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputFilter"></param>
    /// <returns></returns>
    public FileProcessor SetInputFilter( FilenameFilter inputFilter )
    {
        _inputFilter = inputFilter;

        return this;
    }

    /// <summary>
    /// Set comparator to the provided value. By default the files are sorted by alpha.
    /// </summary>
    public FileProcessor SetComparator( Comparison< FileInfo? > comparator )
    {
        _comparator = comparator;

        return this;
    }

    /// <summary>
    /// Adds a case insensitive suffix for matching input files.
    /// </summary>
    public FileProcessor AddInputSuffix( params string[] suffixes )
    {
        foreach ( var suffix in suffixes )
        {
            AddInputRegex( $"(?i).*{Escape( suffix )}" );
        }

        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="regexes"></param>
    /// <returns></returns>
    public FileProcessor AddInputRegex( params string[] regexes )
    {
        foreach ( var regex in regexes )
        {
            _inputRegex.Add( new Regex( regex ) );
        }

        return this;
    }

    /// <summary>
    /// Sets the suffix for output files, replacing the extension of the input file.
    /// </summary>
    public FileProcessor SetOutputSuffix( string outputSuffix )
    {
        this._outputSuffix = outputSuffix;

        return this;
    }

    public FileProcessor SetFlattenOutput( bool flattenOutput )
    {
        this._flattenOutput = flattenOutput;

        return this;
    }

    public FileProcessor SetRecursive( bool recursive = true )
    {
        _recursive = recursive;

        return this;
    }

    // ========================================================================

    public List< Entry > Process( string inputFileOrDir, string? outputRoot )
    {
        return Process( new FileInfo( inputFileOrDir ), outputRoot == null ? null : new FileInfo( outputRoot ) );
    }

    /// <summary>
    /// Processes the specified input file or directory.
    /// </summary>
    /// <param name="inputFileOrDir"></param>
    /// <param name="outputRoot"> May be null if there is no output from processing the files. </param>
    /// <returns> the processed files added with <see cref="AddProcessedFile(Entry)"/>. </returns>
    public List< Entry > Process( FileSystemInfo inputFileOrDir, FileInfo? outputRoot )
    {
        Guard.ThrowIfFileNullOrNotExist( inputFileOrDir );
        Guard.ThrowIfNotFileOrDirectory( inputFileOrDir );
        Guard.ThrowIfNull( outputRoot );

        return inputFileOrDir switch
        {
            FileInfo files            => Process( [ files ], outputRoot ),
            DirectoryInfo directories => Process( [ directories ], outputRoot ),
            var _                     => throw new GdxRuntimeException( "Cannot process file or directory." )
        };
    }

    /// <summary>
    /// Processes the specified input files.
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputRoot"> May be null if there is no output from processing the files. </param>
    /// <returns> the processed files added with <see cref="AddProcessedFile(Entry)"/>. </returns>
    public List< Entry > Process( FileInfo[] files, FileInfo outputRoot )
    {
        Guard.ThrowIfNull( files, nameof( files ) );
        Guard.ThrowIfNull( outputRoot );
        Guard.ThrowIfNull( outputRoot.Directory );

        _outputFiles.Clear();

        var dirToEntries = new Dictionary< FileInfo, List< Entry >? >();

        ProcessFiles( files, outputRoot, outputRoot, dirToEntries!, 0 );

        var allEntries = new List< Entry >();

        foreach ( var (inputDir, dirEntries) in dirToEntries )
        {
            if ( Comparator != null ) dirEntries?.Sort( EntryComparator );

            FileInfo newOutputDir = null!;

            if ( _flattenOutput )
            {
                newOutputDir = outputRoot;
            }
            else if ( dirEntries?.Count > 0 )
            {
                newOutputDir = dirEntries[ 0 ].OutputFile;
            }

            var outputName = inputDir.Name;

            if ( _outputSuffix != null )
            {
                outputName = MyRegex().Replace( outputName, "$1" ) + _outputSuffix;
            }

            var entry = new Entry
            {
                InputFile = inputDir,
                OutputFile = newOutputDir.FullName.Length == 0
                    ? new FileInfo( outputName )
                    : new FileInfo( Path.Combine( newOutputDir.FullName, outputName ) ),
                OutputDir = newOutputDir,
            };

            Guard.ThrowIfNull( dirEntries );

            try
            {
                ProcessDir( entry, dirEntries );
            }
            catch ( Exception ex )
            {
                throw new Exception( $"Error processing directory: {entry.InputFile?.FullName}", ex );
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
                throw new Exception( "Error processing file: " + entry.InputFile?.FullName, ex );
            }
        }

        return _outputFiles;
    }

    /// <summary>
    /// Processes the specified input directories.
    /// </summary>
    /// <param name="directories"></param>
    /// <param name="outputRoot"> May be null if there is no output from processing the files. </param>
    /// <returns> the processed files added with <see cref="AddProcessedFile(Entry)"/>. </returns>
    public List< Entry > Process( DirectoryInfo[] directories, FileInfo outputRoot )
    {
        Guard.ThrowIfNull( directories, nameof( directories ) );
        Guard.ThrowIfNull( outputRoot );
        Guard.ThrowIfNull( outputRoot.Directory );

        _outputFiles.Clear();

        var dirToEntries = new Dictionary< FileInfo, List< Entry >? >();

        Guard.ThrowIfNull( dirToEntries );

        ProcessDirectories( directories, outputRoot.Directory!, outputRoot.Directory!, dirToEntries!, 0 );

        var allEntries = new List< Entry >();

        foreach ( var (inputDir, dirEntries) in dirToEntries )
        {
            if ( Comparator != null ) dirEntries?.Sort( EntryComparator );

            FileInfo newOutputDir = null!;

            if ( _flattenOutput )
            {
                newOutputDir = outputRoot.Directory!;
            }
            else if ( dirEntries?.Count > 0 )
            {
                newOutputDir = dirEntries[ 0 ].OutputDir;
            }

            var outputName = inputDir.Name;

            if ( _outputSuffix != null )
            {
                outputName = MyRegex().Replace( outputName, "$1" ) + _outputSuffix;
            }

            var entry = new Entry
            {
                InputFile = inputDir,
                OutputFile = newOutputDir.FullName.Length == 0
                    ? new FileInfo( outputName )
                    : new FileInfo( Path.Combine( newOutputDir.FullName, outputName ) ),
                OutputDir = newOutputDir,
            };

            Guard.ThrowIfNull( dirEntries );

            try
            {
                ProcessDir( entry, dirEntries );
            }
            catch ( Exception ex )
            {
                throw new Exception( $"Error processing directory: {entry.InputFile?.FullName}", ex );
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
                throw new Exception( "Error processing file: " + entry.InputFile?.FullName, ex );
            }
        }

        return _outputFiles;
    }

    private void Process( FileSystemInfo[] files, FileInfo outputRoot, FileInfo outputDir,
                          Dictionary< FileInfo, List< Entry > > dirToEntries, int depth )
    {
        // Store empty entries for every directory.
        foreach ( var file in files )
        {
            var dir = file is FileInfo fileInfo ? fileInfo.Directory : ( ( FileInfo )file ).Parent;

            if ( dir != null )
            {
                if ( !dirToEntries.ContainsKey( dir ) )
                {
                    dirToEntries[ dir ] = new List< Entry >();
                }
            }
        }

        foreach ( var file in files )
        {
            if ( file is FileInfo fileInfo )
            {
                if ( _inputRegex.Count > 0 )
                {
                    var found = false;

                    foreach ( var pattern in _inputRegex )
                    {
                        if ( pattern.IsMatch( fileInfo.Name ) )
                        {
                            found = true;

                            break;
                        }
                    }

                    if ( !found ) continue;
                }

                var dir = fileInfo.Directory;

                Guard.ThrowIfNull( dir );

                if ( ( _inputFilter != null ) && !_inputFilter( dir, fileInfo.Name ) ) continue;

                var outputName = fileInfo.Name;

                if ( _outputSuffix != null )
                {
                    outputName = MyRegex().Replace( outputName, "$1" ) + _outputSuffix;
                }

                var entry = new Entry
                {
                    Depth     = depth,
                    InputFile = fileInfo.Directory!,
                    OutputDir = outputDir,
                    OutputFile = _flattenOutput
                        ? new FileInfo( Path.Combine( outputRoot.FullName, outputName ) )
                        : new FileInfo( Path.Combine( outputDir.FullName, outputName ) ),
                };

                dirToEntries[ dir ].Add( entry );
            }

            if ( _recursive && file is FileInfo directoryInfo )
            {
                var subdir = outputDir.FullName.Length == 0
                    ? new FileInfo( directoryInfo.Name )
                    : new FileInfo( Path.Combine( outputDir.FullName, directoryInfo.Name ) );

                Process( directoryInfo.GetFileSystemInfos().Where
                             ( f => ( _inputFilter == null )
                                    || f is FileInfo
                                    || ( f is FileInfo fInfo
                                         && _inputFilter( fInfo.Directory!, f.Name ) ) ).ToArray(),
                         outputRoot, subdir, dirToEntries, depth + 1 );
            }
        }
    }

    // ========================================================================

    private void ProcessFiles( FileInfo[] files,
                               FileInfo outputRoot,
                               FileInfo outputDir,
                               Dictionary< FileInfo, List< Entry > > dirToEntries,
                               int depth )
    {
        // Store empty entries for every directory.
        foreach ( var file in files )
        {
            if ( file.Directory != null )
            {
                if ( !dirToEntries.ContainsKey( file ) )
                {
                    dirToEntries[ file ] = [ ];
                }
            }
        }

        foreach ( var file in files )
        {
            if ( _inputRegex.Count > 0 )
            {
                var found = false;

                foreach ( var pattern in _inputRegex )
                {
                    if ( pattern.IsMatch( file.Name ) )
                    {
                        found = true;

                        break;
                    }
                }

                if ( !found ) continue;
            }

            var dir = file.Directory;

            Guard.ThrowIfNull( dir );

            if ( ( _inputFilter != null ) && !_inputFilter( dir, file.Name ) ) continue;

            var outputName = file.Name;

            if ( _outputSuffix != null )
            {
                outputName = MyRegex().Replace( outputName, "$1" ) + _outputSuffix;
            }

            var entry = new Entry
            {
                Depth     = depth,
                InputFile = file,
                OutputDir = outputDir.Directory!,
                OutputFile = _flattenOutput
                    ? new FileInfo( Path.Combine( outputRoot.FullName, outputName ) )
                    : new FileInfo( Path.Combine( outputDir.FullName, outputName ) ),
            };

            dirToEntries[ dir ].Add( entry );
        }
    }

    private void ProcessDirectories( DirectoryInfo[]? files,
                                     DirectoryInfo outputRoot,
                                     DirectoryInfo outputDir,
                                     Dictionary< DirectoryInfo, List< Entry > > dirToEntries,
                                     int depth )
    {
        Guard.ThrowIfNull( files );

        // Store empty entries for every directory.
        foreach ( var file in files )
        {
            if ( !dirToEntries.ContainsKey( file ) )
            {
                dirToEntries[ file ] = [ ];
            }
        }

        foreach ( var file in files )
        {
            if ( _recursive )
            {
                var subdir = outputDir.FullName.Length == 0
                    ? new FileInfo( file.Name )
                    : new FileInfo( Path.Combine( outputDir.FullName, file.Name ) );

                Process( file.GetFileSystemInfos().Where
                             ( f => ( _inputFilter == null )
                                    || f is FileInfo
                                    || ( f is FileInfo fInfo
                                         && _inputFilter( fInfo, f.Name ) ) ).ToArray(),
                         outputRoot, subdir, dirToEntries, depth + 1 );
            }
        }
    }

    // ========================================================================

    /// <summary>
    /// Called with each input file.
    /// </summary>
    protected void ProcessFile( Entry entry )
    {
        //TODO:
    }

    /// <summary>
    /// Called for each input directory. The files will be <see cref="SetComparator(Comparison{)" />
    /// sorted. The specified files list can be modified to change which files are processed.
    /// </summary>
    protected void ProcessDir( Entry entryDir, List< Entry > files )
    {
        //TODO:
    }

    /// <summary>
    /// This method should be called by <see cref="ProcessFile(Entry)" /> or
    /// <see cref="ProcessDir(Entry, List{Entry})" /> if the return value of
    /// <see cref="System.Diagnostics.Process" /> or <see cref="System.Diagnostics.Process" />
    /// should return all the processed files.
    /// </summary>
    protected void AddProcessedFile( Entry entry )
    {
        _outputFiles.Add( entry );
    }

    // ========================================================================
    // ========================================================================

    [PublicAPI]
    public class Entry
    {
        public FileInfo      InputFile  { get; set; } = null!;
        public FileInfo      OutputFile { get; set; } = null!;
        public DirectoryInfo OutputDir  { get; set; } = null!;
        public int           Depth      { get; set; }

        // ====================================================================

        public Entry()
        {
        }

        public Entry( FileInfo inputFile, FileInfo outputFile )
        {
            InputFile  = inputFile;
            OutputFile = outputFile;
        }

        /// <inheritdoc />
        public override string? ToString()
        {
            return InputFile.ToString();
        }
    }

    // ========================================================================
    // ========================================================================

    [GeneratedRegex( "(.*)\\..*" )]
    private static partial Regex MyRegex();
}