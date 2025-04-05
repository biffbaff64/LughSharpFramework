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
/// </summary>
[PublicAPI]
public partial class FileProcessor
{
    // Delegate to filter filenames in a directory
    public delegate bool FilenameFilter( string directory, string filename );

    // Delegate to signal a processed file
    public delegate void FileProcessedHandler( FileInfo file );

    // Delegate to process a file
    public delegate void ProcessFileDelegate( Entry entry );

    // Delegate to process a directory
    public delegate void ProcessDirDelegate( Entry entry, List< Entry > files );

    // Events based on the delegates
    public event FileProcessedHandler? FileProcessed;
    public event ProcessFileDelegate?  OnProcessFile;
    public event ProcessDirDelegate?   OnProcessDir;

    // ========================================================================

    public List< Regex >   InputRegex      { get; } = [ ];
    public List< Entry >   OutputFilesList { get; } = [ ];
    public string          OutputSuffix    { get; set; }
    public bool            Recursive       { get; set; } = true;
    public bool            FlattenOutput   { get; set; }
    public FilenameFilter? InputFilter     { get; set; }

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
    /// Default Constructor.
    /// Creates a new <c>FileProcessor</c> object.
    /// </summary>
    public FileProcessor()
    {
        OutputSuffix  = string.Empty;
        FlattenOutput = false;

        SetRecursive();
    }

    /// <summary>
    /// Creates a new <c>FileProcessor</c> object, using the settings provided
    /// by the supplied FileProcessor object.
    /// </summary>
    /// <param name="processor"> The FileProcessor to copy. </param>
    public FileProcessor( FileProcessor processor )
    {
        Comparator  = processor.Comparator;
        InputFilter = processor.InputFilter;

        InputRegex.AddRange( processor.InputRegex );

        OutputSuffix  = processor.OutputSuffix;
        Recursive     = processor.Recursive;
        FlattenOutput = processor.FlattenOutput;
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="suffixes"></param>
    /// <returns></returns>
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
            InputRegex.Add( new Regex( regex ) );
        }

        return this;
    }

    /// <summary>
    /// </summary>
    /// <param name="outputSuffix"></param>
    /// <returns></returns>
    public FileProcessor SetOutputSuffix( string outputSuffix )
    {
        OutputSuffix = outputSuffix;

        return this;
    }

    /// <summary>
    /// </summary>
    /// <param name="flattenOutput"></param>
    /// <returns></returns>
    public FileProcessor SetFlattenOutput( bool flattenOutput )
    {
        FlattenOutput = flattenOutput;

        return this;
    }

    /// <summary>
    /// </summary>
    /// <param name="recursive"></param>
    /// <returns></returns>
    public FileProcessor SetRecursive( bool recursive = true )
    {
        Recursive = recursive;

        return this;
    }

    /// <summary>
    /// </summary>
    /// <param name="inputFileOrDir"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>
    public List< Entry > Process( string inputFileOrDir, string? outputRoot )
    {
        return Process( new FileInfo( inputFileOrDir ), outputRoot == null
                            ? null
                            : new DirectoryInfo( outputRoot ) );
    }

    /// <summary>
    /// </summary>
    /// <param name="inputFileOrDir"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public virtual List< Entry > Process( FileInfo? inputFileOrDir, DirectoryInfo? outputRoot )
    {
        if ( inputFileOrDir is not { Exists: true } )
        {
            throw new ArgumentException( $"Input file does not exist: {inputFileOrDir?.FullName}" );
        }

        if ( ( inputFileOrDir.Attributes & FileAttributes.Directory ) == 0 )
        {
            return Process( [ inputFileOrDir ], outputRoot );
        }

        return Process( new DirectoryInfo( inputFileOrDir.FullName )
                        .GetFileSystemInfos().Select( f => new FileInfo( f.FullName ) ).ToArray(), outputRoot );
    }

    /// <summary>
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>+
    /// <exception cref="Exception"></exception>
    public List< Entry > Process( FileInfo[] files, DirectoryInfo? outputRoot )
    {
        if ( outputRoot == null )
        {
            outputRoot = new DirectoryInfo( "" );
        }

        OutputFilesList.Clear();

        var dirToEntries = new Dictionary< DirectoryInfo, List< Entry > >();

        Process( files, outputRoot, outputRoot, dirToEntries, 0 );

        var allEntries = new List< Entry >();

        foreach ( var mapEntry in dirToEntries )
        {
            var dirEntries = mapEntry.Value;

            if ( Comparator != null )
            {
                dirEntries.Sort( EntryComparator );
            }

            var inputDir     = mapEntry.Key;
            var newOutputDir = default( DirectoryInfo );

            if ( FlattenOutput )
            {
                newOutputDir = outputRoot;
            }
            else if ( dirEntries.Count > 0 )
            {
                newOutputDir = dirEntries[ 0 ].OutputDirectory;
            }

            var outputName = inputDir.Name;

            if ( OutputSuffix != null )
            {
                outputName = MyRegex().Replace( outputName, "$1" ) + OutputSuffix;
            }

            var entry = new Entry
            {
                InputFile       = inputDir,
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
            if ( file.DirectoryName == null )
            {
                Logger.Debug( $"file.DirectoryName is null: {file.FullName}" );
            }

            var dir = new DirectoryInfo( file.DirectoryName! );

            if ( !dirToEntries.ContainsKey( dir ) )
            {
                dirToEntries[ dir ] = [ ];
            }
        }

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

                var dir = new DirectoryInfo( file.DirectoryName! );

                if ( ( InputFilter != null ) && !InputFilter( dir.FullName, file.Name ) )
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

                dirToEntries[ dir ].Add( entry );
            }

            if ( Recursive && ( ( file.Attributes & FileAttributes.Directory ) != 0 ) )
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
    /// <param name="entry"></param>
    public virtual void ProcessFile( Entry entry )
    {
        Guard.ThrowIfNull( entry.InputFile );

        FileProcessed?.Invoke( ( FileInfo )entry.InputFile );
        OnProcessFile?.Invoke( entry );
    }

    /// <summary>
    /// </summary>
    /// <param name="entryDir"></param>
    /// <param name="files"></param>
    public virtual void ProcessDir( Entry entryDir, List< Entry > files )
    {
        OnProcessDir?.Invoke( entryDir, files );
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
    /// </summary>
    /// <param name="entry"></param>
    public void AddProcessedFile( Entry entry )
    {
        OutputFilesList.Add( entry );
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    [PublicAPI]
    public class Entry
    {
        public FileSystemInfo? InputFile       { get; set; } = null!;
        public FileInfo?       OutputFile      { get; set; } = null!;
        public DirectoryInfo?  OutputDirectory { get; set; } = null!;
        public int             Depth           { get; set; }

        public override string ToString()
        {
            return InputFile?.ToString() ?? "";
        }
    }

    // ========================================================================

    [GeneratedRegex( "(.*)\\..*" )] private static partial Regex MyRegex();

    // ========================================================================
}