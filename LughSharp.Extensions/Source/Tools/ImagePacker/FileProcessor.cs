﻿// /////////////////////////////////////////////////////////////////////////////
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

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics.Text;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

using DirectoryInfo = System.IO.DirectoryInfo;

namespace Extensions.Source.Tools.ImagePacker;

[PublicAPI]
public class FileProcessor
{
    public const string DEFAULT_PACKFILE_NAME = "pack.atlas";

    // Delegate to filter filenames in a directory
    public Func< string, string, bool >? FilenameFilterDelegate { get; set; }

    // Delegate to signal a processed file
    public virtual Action< FileSystemInfo >? FileProcessedDelegate { get; set; }

    // Delegate to process a file
    public Action< TexturePackerEntry >? ProcessFileDelegate { get; set; }

    // Delegate to process a directory
    public Action< TexturePackerEntry, List< TexturePackerEntry > >? ProcessDirDelegate { get; set; }

    // ========================================================================

    public List< Regex >              InputRegex      { get; set; }
    public List< TexturePackerEntry > OutputFilesList { get; set; }
    public string                     OutputSuffix    { get; set; }
    public bool                       Recursive       { get; set; }
    public bool                       FlattenOutput   { get; set; }

    // ========================================================================

    public Comparison< FileInfo > Comparator { get; set; } = ( o1, o2 ) =>
        string.Compare( o1.Name, o2.Name, StringComparison.Ordinal );

    public Comparison< TexturePackerEntry > EntryComparator
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

    // ========================================================================

    /// <summary>
    /// Processes the specified input file or directory.
    /// </summary>
    /// <param name="inputFileOrDir"></param>
    /// <param name="outputRoot"> May be null if there is no output from processing the files. </param>
    /// <returns> the processed files added with <see cref="AddProcessedFile(TexturePackerEntry)"/>. </returns>
    public virtual List< TexturePackerEntry > Process( FileSystemInfo? inputFileOrDir, DirectoryInfo? outputRoot )
    {
        if ( inputFileOrDir is not { Exists: true } )
        {
            throw new ArgumentException( $"IFileProcessor#Process: Input file/dir does not " +
                                         $"exist: {inputFileOrDir?.FullName}" );
        }

        List< TexturePackerEntry > retval;

        if ( IOUtils.IsFile( inputFileOrDir ) )
        {
            Logger.Debug( $"Processing file: {inputFileOrDir.FullName}" );

            retval = Process( [ ( FileInfo )inputFileOrDir ], outputRoot );
        }
        else
        {
            Logger.Debug( $"Processing directory: {inputFileOrDir.FullName}" );

            var files = new DirectoryInfo( inputFileOrDir.FullName )
                        .GetFileSystemInfos().Select( f => new FileInfo( f.FullName ) ).ToArray();

            retval = Process( files, outputRoot );
        }

        return retval;
    }

    /// <summary>
    /// Processes a collection of files for sending to the output folder.
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public virtual List< TexturePackerEntry > Process( FileInfo[] files, DirectoryInfo? outputRoot )
    {
        Logger.Checkpoint();

        if ( outputRoot == null )
        {
            Logger.Debug( $"Setting outputRoot to InternalPath: {IOUtils.InternalPath}" );

            outputRoot = new DirectoryInfo( IOUtils.InternalPath );
        }

        OutputFilesList.Clear();

        var stringToEntries = new Dictionary< DirectoryInfo, List< TexturePackerEntry > >();
        var allEntries      = new List< TexturePackerEntry >();

        Process( files, outputRoot, outputRoot, stringToEntries!, 0 );

        foreach ( var (inputDir, dirEntries) in stringToEntries )
        {
            if ( Comparator != null )
            {
                dirEntries.Sort( EntryComparator );
            }

            DirectoryInfo? newOutputDir = null;

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
                outputName = RegexUtils.FileNameWithoutExtensionRegex().Match( outputName ) + OutputSuffix;
            }

            var entry = new TexturePackerEntry
            {
                InputFile       = inputDir,
                OutputDirectory = newOutputDir!,
            };

            if ( newOutputDir != null )
            {
                entry.OutputFileName = newOutputDir.FullName.Length == 0
                    ? outputName
                    : Path.Combine( newOutputDir.FullName, outputName );
            }

            try
            {
                ProcessDirDelegate?.Invoke( entry, dirEntries );
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
    public virtual void Process( FileInfo[] files, DirectoryInfo outputRoot, DirectoryInfo outputDir,
                                 Dictionary< DirectoryInfo, List< TexturePackerEntry >? > dirToEntries, int depth )
    {
        foreach ( var file in files )
        {
            var dir = file.Directory;

            if ( dir != null )
            {
                if ( !dirToEntries.ContainsKey( dir ) )
                {
                    // If the directory is not already a key in the dictionary, add it
                    dirToEntries[ dir ] = [ ];
                }
            }
            else
            {
                // Handle the case where a file has no parent directory (e.g., root level)
                // Either log a warning, throw an exception, or handle it differently
                // ( Logging a warning for now... )
                Logger.Warning( $"WARNING: File '{file.FullName}' has no parent directory." );
            }
        }

        foreach ( var file in files )
        {
            if ( !IOUtils.IsDirectory( file ) )
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

                if ( ( FilenameFilterDelegate != null )
                     && !FilenameFilterDelegate( file.Directory!.FullName, file.Name ) )
                {
                    continue;
                }

                var outputName = file.Name;

                if ( OutputSuffix != null )
                {
                    outputName = RegexUtils.FileNameWithoutExtensionRegex().Replace( outputName, "$1" ) + OutputSuffix;
                }

                var entry = new TexturePackerEntry
                {
                    Depth           = depth,
                    InputFile       = file,
                    OutputDirectory = outputDir,
                    OutputFileName = FlattenOutput
                        ? Path.Combine( outputRoot.FullName, outputName )
                        : Path.Combine( outputDir.FullName, outputName ),
                };

                var dir = file.Directory!;

                if ( !dirToEntries.TryGetValue( dir, out var value ) )
                {
                    value?.Add( entry );
                }
                else
                {
                    // This should ideally not happen if the first loop worked correctly.
                    // You might want to log a warning here for unexpected cases.

                    Logger.Warning( $"Directory '{dir.FullName}' not found in dirToEntries during file processing." );

                    // Create a new list here as fallback
                    dirToEntries[ dir ] = [ entry ];
                }
            }

            if ( Recursive && IOUtils.IsDirectory( file ) )
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
    public virtual void Process( FileInfo[] files, DirectoryInfo outputRoot, DirectoryInfo outputDir,
                                 Dictionary< string, List< TexturePackerEntry > > stringToEntries, int depth )
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
                    outputName = RegexUtils.FileNameWithoutExtensionRegex().Replace( outputName, "$1" ) + OutputSuffix;
                }

                var entry = new TexturePackerEntry
                {
                    Depth           = depth,
                    InputFile       = file,
                    OutputDirectory = outputDir,

                    OutputFileName = Path.Combine( FlattenOutput
                                                       ? outputRoot.FullName
                                                       : outputDir.FullName,
                                                   outputName ),
                };

                var dir = file.Directory!.FullName;

                if ( !stringToEntries.TryGetValue( dir, out var value ) )
                {
                    value = [ ];
                    stringToEntries.Add( dir, value );
                }

                value.Add( entry );
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
        // To get the count of files stores in the List< TexturePackerEntry >, use something like:-
        // var totalEntries = stringToEntries.Values.Sum(list => list.Count);
    }

    /// <summary>
    /// </summary>
    /// <param name="entry"></param>
    public virtual void ProcessFile( TexturePackerEntry entry )
    {
        Guard.ThrowIfNull( entry.InputFile );

        FileProcessedDelegate?.Invoke( ( FileInfo )entry.InputFile );
        ProcessFileDelegate?.Invoke( entry );
    }

    /// <summary>
    /// </summary>
    /// <param name="entryDir"></param>
    /// <param name="files"></param>
    public virtual void ProcessDir( TexturePackerEntry entryDir, List< TexturePackerEntry > files )
    {
    }

    /// <summary>
    /// Adds a processed <see cref="TexturePackerEntry"/> to the <see cref="OutputFilesList"/>.
    /// This method should be called by:-
    /// <li><see cref="ProcessFile(TexturePackerEntry)"/> or,</li>
    /// <li><see cref="ProcessDir(TexturePackerEntry, List{TexturePackerEntry})"/></li>
    /// if the return value of <see cref="Process(FileSystemInfo, DirectoryInfo)"/> or
    /// <see cref="Process(FileInfo[], DirectoryInfo)"/> should return all the processed
    /// files.
    /// </summary>
    /// <param name="entry"></param>
    public virtual void AddProcessedFile( TexturePackerEntry entry )
    {
        OutputFilesList.Add( entry );
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="regexes"></param>
    /// <returns> This IFileProcessor for chaining. </returns>
    public virtual FileProcessor AddInputRegex( params string[] regexes )
    {
        foreach ( var regex in regexes )
        {
            InputRegex.Add( new Regex( regex ) );
        }

        return this;
    }
}