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

using System.Text.RegularExpressions;

using LughSharp.Lugh.Utils;
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
    public delegate bool FilenameFilter( string directory, string filename );

    public List< Regex >   InputRegex    { get; } = new List< Regex >();
    public string          OutputSuffix  { get; set; }
    public List< Entry >   OutputFiles   { get; }      = new List< Entry >();
    public bool            Recursive     { get; set; } = true;
    public bool            FlattenOutput { get; set; }
    public FilenameFilter? InputFilter   { get; set; }

    public Comparison< FileInfo > Comparator { get; set; } = ( o1, o2 ) =>
        string.Compare( o1.Name, o2.Name, StringComparison.Ordinal );

    private Comparison<Entry> EntryComparator
    {
        get =>
            ( o1, o2 ) =>
            {
                if ( o1.InputFile is FileInfo file1 && o2.InputFile is FileInfo file2 )
                {
                    return this.Comparator( file1, file2 ); // Use 'this.Comparator'
                }
                else if ( o1.InputFile is DirectoryInfo dir1 && o2.InputFile is DirectoryInfo dir2 )
                {
                    return String.Compare( dir1.Name, dir2.Name, StringComparison.Ordinal );
                }
                else
                {
                    return String.Compare( o1.InputFile.Name, o2.InputFile.Name, StringComparison.Ordinal );
                }
            };
        set => throw new NotImplementedException();
    }

    // ========================================================================
    
    public FileProcessor()
    {
        OutputSuffix  = string.Empty;
        FlattenOutput = false;

        SetRecursive();
    }

    public FileProcessor( FileProcessor processor )
    {
        this.Comparator = processor.Comparator;
        InputFilter     = processor.InputFilter;

        InputRegex.AddRange( processor.InputRegex );
        
        OutputSuffix  = processor.OutputSuffix;
        Recursive     = processor.Recursive;
        FlattenOutput = processor.FlattenOutput;
    }

    // ========================================================================

    public FileProcessor AddInputSuffix( params string[] suffixes )
    {
        foreach ( var suffix in suffixes )
        {
            AddInputRegex( $"(?i).*{Regex.Escape( suffix )}" );
        }

        return this;
    }

    public FileProcessor AddInputRegex( params string[] regexes )
    {
        foreach ( var regex in regexes )
        {
            InputRegex.Add( new Regex( regex ) );
        }

        return this;
    }

    public FileProcessor SetOutputSuffix( string outputSuffix )
    {
        OutputSuffix = outputSuffix;

        return this;
    }

    public FileProcessor SetFlattenOutput( bool flattenOutput )
    {
        FlattenOutput = flattenOutput;

        return this;
    }

    public FileProcessor SetRecursive( bool recursive = true )
    {
        Recursive = recursive;

        return this;
    }

    public List< Entry > Process( string inputFileOrDir, string? outputRoot )
    {
        return Process( new FileInfo( inputFileOrDir ), outputRoot == null ? null : new DirectoryInfo( outputRoot ) );
    }

    public List< Entry > Process( FileInfo inputFileOrDir, DirectoryInfo? outputRoot )
    {
        if ( !inputFileOrDir.Exists )
        {
            throw new ArgumentException( $"Input file does not exist: {inputFileOrDir.FullName}" );
        }

        if ( ( inputFileOrDir.Attributes & FileAttributes.Directory ) == 0 )
        {
            return Process( [ inputFileOrDir ], outputRoot );
        }

        return Process( new DirectoryInfo( inputFileOrDir.FullName ).GetFileSystemInfos().Select( f => new FileInfo( f.FullName ) ).ToArray(), outputRoot );
    }

    public List< Entry > Process( FileInfo[] files, DirectoryInfo? outputRoot )
    {
        if ( outputRoot == null )
        {
            outputRoot = new DirectoryInfo( "" );
        }
        
        OutputFiles.Clear();

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
                newOutputDir = dirEntries[ 0 ].OutputDir;
            }
            
            var outputName = inputDir.Name;

            if ( OutputSuffix != null )
            {
                outputName = MyRegex().Replace(outputName, "$1") + OutputSuffix;
            }

            var entry = new Entry
            {
                InputFile = inputDir,
                OutputDir = newOutputDir!,
            };

            if ( newOutputDir != null )
                entry.OutputFile = newOutputDir.FullName.Length == 0
                    ? new FileInfo( outputName )
                    : new FileInfo( Path.Combine( newOutputDir.FullName, outputName ) );

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

        if ( Comparator != null ) allEntries.Sort( EntryComparator );

        foreach ( var entry in allEntries )
        {
            try
            {
                ProcessFile( entry );
            }
            catch ( Exception ex )
            {
                throw new Exception( $"Error processing file: {entry.InputFile.FullName}", ex );
            }
        }

        return OutputFiles;
    }

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

                    if ( !found ) continue;
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
                    outputName = MyRegex().Replace(outputName, "$1") + OutputSuffix;
                }

                var entry = new Entry
                {
                    Depth      = depth,
                    InputFile  = file,
                    OutputDir  = outputDir,
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

    protected virtual void ProcessFile( Entry entry )
    {
    }

    protected virtual void ProcessDir( Entry entryDir, List< Entry > files )
    {
    }

    protected void AddProcessedFile( Entry entry )
    {
        OutputFiles.Add( entry );
    }

    // ========================================================================

    public class Entry
    {
        public FileSystemInfo InputFile  { get; set; } = null!;
        public DirectoryInfo  OutputDir  { get; set; } = null!;
        public FileInfo       OutputFile { get; set; } = null!;
        public int            Depth      { get; set; }

        public Entry()
        {
        }

        public Entry( FileInfo inputFile, FileInfo outputFile )
        {
            InputFile  = inputFile;
            OutputFile = outputFile;
        }

        public override string ToString()
        {
            return InputFile.ToString();
        }
    }

    // ========================================================================
    
    [GeneratedRegex("(.*)\\..*")]
    private static partial Regex MyRegex();
}