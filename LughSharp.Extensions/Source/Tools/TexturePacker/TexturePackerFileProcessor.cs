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

using System.Text.RegularExpressions;

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics.Text;
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
public class TexturePackerFileProcessor : IFileProcessor
{
    public const string DEFAULT_PACKFILE_NAME = "pack.atlas";

    public List< Regex >              InputRegex      { get; set; }
    public List< TexturePackerEntry > OutputFilesList { get; set; }
    public List< DirectoryInfo >      DirsToIgnore    { get; set; } = [ ];
    public Comparison< FileInfo >     Comparator      { get; set; }

    public TexturePacker.AbstractProgressListener? ProgressListener { get; set; }

    public string OutputSuffix  { get; set; }
    public bool   Recursive     { get; set; }
    public bool   FlattenOutput { get; set; }
    public bool   CountOnly     { get; set; }
    public string PackFileName  { get; set; }

    // ========================================================================

    public Func< string, string, bool >? FilenameFilterDelegate;

    // ========================================================================

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

    private readonly TexturePacker.Settings _defaultSettings;

    private Dictionary< DirectoryInfo, TexturePacker.Settings > _dirToSettings = [ ]; //TODO: Rename

    private int           _packCount;
    private DirectoryInfo _rootDirectory = null!;

    // ========================================================================

    /// <summary>
    /// Default constructor. Processes Texture packing using default settings
    /// and the default packfile name ( "pack.atlas" ).
    /// </summary>
    public TexturePackerFileProcessor()
        : this( new TexturePacker.Settings(), DEFAULT_PACKFILE_NAME )
    {
    }

    /// <summary>
    /// Constructs a new TexturePackerFileProcessor object, using the supplied
    /// <see cref="TexturePacker.Settings"/>.
    /// </summary>
    /// <param name="packerSettings"> The Settings to use. </param>
    /// <param name="packFileName"> The name of the final packed file. </param>
    /// <param name="progress"> The ProgressListener to use. Can be null. </param>
    public TexturePackerFileProcessor( TexturePacker.Settings? packerSettings,
                                       string packFileName,
                                       TexturePacker.AbstractProgressListener? progress = null )
    {
        InputRegex       = [ ];
        OutputFilesList  = [ ];
        OutputSuffix     = string.Empty;
        FlattenOutput    = false;
        Recursive        = true;
        _defaultSettings = packerSettings ?? new TexturePacker.Settings();
        ProgressListener = progress;

        // Strip off the .atlas extension name from the packfile if it has been pre-added.
        packFileName = IOUtils.StripExtension( packFileName, _defaultSettings.AtlasExtension );

        PackFileName  = packFileName;
        FlattenOutput = true;

        // Set the default file extensions for processable images.
        AddInputSuffix( ".png", ".jpg", ".jpeg", ".bmp" );

        // Sort input files by name to avoid platform-dependent atlas output changes.
        Comparator = ( file1, file2 ) => string.Compare( file1.Name,
                                                         file2.Name,
                                                         StringComparison.Ordinal );
    }

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="inputRoot"> Directory containing individual images to be packed. </param>
    /// <param name="outputRoot"> Directory where the pack file and page images will be written. </param>
    /// <returns></returns>
    public virtual List< TexturePackerEntry > Process( DirectoryInfo? inputRoot,
                                                       DirectoryInfo? outputRoot )
    {
        Guard.ThrowIfNull( inputRoot );

        _rootDirectory = inputRoot;

        // -----------------------------------------------------
        // Collect any pack.json setting files present in the folder.
        var settingsProcessor = new PackingSettingsProcessor();
        _ = settingsProcessor.AddInputRegex( "pack\\.json" ).Process( inputRoot, outputRoot );

        // -----------------------------------------------------

        ProgressListener ??= new TexturePacker.AbstractProgressListenerImpl();

        var settingsFiles = settingsProcessor.SettingsFiles;

        if ( settingsFiles.Count > 0 )
        {
            // Sort parent first.
            settingsFiles.Sort( ( file1, file2 ) => file1.ToString().Length - file2.ToString().Length );

            // Establish the settings to use for processing
            foreach ( var settingsFile in settingsFiles )
            {
                // Find first parent with settings, or use defaults.
                TexturePacker.Settings? settings = null;

                var parent  = settingsFile.Directory;
                var current = parent;

                while ( ( current != null ) && !current.Equals( _rootDirectory ) )
                {
                    if ( _dirToSettings.TryGetValue( current, out settings ) )
                    {
                        settings = NewSettings( settings );

                        break;
                    }

                    current = current.Parent;
                }

                settings ??= NewSettings( _defaultSettings );

                // Merge settings from current directory.
                MergeSettings( settings, settingsFile );

                _dirToSettings.Add( settingsFile.Directory!, settings );
            }
        }

        // Count the number of texture packer invocations for the
        // ProgressListener to use.
        CountOnly = true;
        _         = ProcessIO( inputRoot, outputRoot );
        CountOnly = false;

        // Do actual processing.
        ProgressListener?.Start( 1 );
        var result = ProcessIO( inputRoot, outputRoot );
        ProgressListener?.End();

        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="inputFileOrDir"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>
    public virtual List< TexturePackerEntry > Process( string inputFileOrDir, string? outputRoot )
    {
        return Process( new DirectoryInfo( inputFileOrDir ),
                        outputRoot == null ? null : new DirectoryInfo( outputRoot ) );
    }

    /// <summary>
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>
    public virtual List< TexturePackerEntry > Process( FileInfo[] files, FileInfo? outputRoot )
    {
        Guard.ThrowIfNull( outputRoot );

        // Delete pack file and images.
        if ( CountOnly && outputRoot.Exists )
        {
            DeleteOutput( outputRoot );
        }

        return Process( files, outputRoot.Directory );
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
        outputRoot ??= new DirectoryInfo( IOUtils.InternalPath );

        OutputFilesList.Clear();

        var stringToEntries = new Dictionary< string, List< TexturePackerEntry > >();
        var allEntries      = new List< TexturePackerEntry >();

        Process( files, outputRoot, outputRoot, stringToEntries, 0 );

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

            var outputName = inputDir;

            if ( OutputSuffix != null )
            {
                var match = RegexUtils.MatchFinalFolderPatternRegex().Match( outputName );

                if ( match.Success )
                {
                    outputName = match.Groups[ 1 ].Value + OutputSuffix;
                }
            }

            var entry = new TexturePackerEntry
            {
                InputFile       = new DirectoryInfo( inputDir ),
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
    [Obsolete( "Use Process( FileInfo[], DirectoryInfo, DirectoryInfo, Dictionary<string, List<TexturePackerEntry>>, int ) instead" )]
    public virtual void Process( FileInfo[] files,
                                 DirectoryInfo outputRoot,
                                 DirectoryInfo outputDir,
                                 Dictionary< DirectoryInfo, List< TexturePackerEntry >? > dirToEntries,
                                 int depth )
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
            if ( IOUtils.IsFile( file ) )
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

                if ( ( FilenameFilterDelegate != null )
                     && !FilenameFilterDelegate( file.Directory!.FullName, file.Name ) )
                {
                    continue;
                }

                var outputName = file.Name;

                if ( OutputSuffix != null )
                {
                    outputName = RegexUtils.FileNameWithoutExtensionRegex().Replace( outputName, "$1" )
                                 + OutputSuffix;
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

                    Logger.Warning( $"Directory '{dir.FullName}' not found in dirToEntries during file processing." );

                    // Potentially create a new list here if there is a specific fallback behavior:
                    // dirToEntries[dir] = new List<TexturePackerEntry> { entry };
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
    /// <param name="dirToEntries"></param>
    /// <param name="depth"></param>
    public virtual void Process( FileInfo[] files,
                                 DirectoryInfo outputRoot,
                                 DirectoryInfo outputDir,
                                 Dictionary< string, List< TexturePackerEntry > > dirToEntries,
                                 int depth )
    {
        foreach ( var file in files )
        {
            var dir = file.Directory;

            if ( dir != null )
            {
                if ( !dirToEntries.ContainsKey( dir.FullName ) )
                {
                    // If the directory is not already a key in the dictionary, add it
                    dirToEntries[ dir.FullName ] = [ ];
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
            if ( IOUtils.IsFile( file ) )
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

                if ( ( FilenameFilterDelegate != null )
                     && !FilenameFilterDelegate( file.Directory!.FullName, file.Name ) )
                {
                    continue;
                }

                var outputName = file.Name;

                if ( OutputSuffix != null )
                {
                    outputName = RegexUtils.FileNameWithoutExtensionRegex().Replace( outputName, "$1" )
                                 + OutputSuffix;
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

                var dir = file.Directory!.FullName;

                if ( dirToEntries.TryGetValue( dir, out var value ) )
                {
                    value.Add( entry );
                }
                else
                {
                    // This should ideally not happen if the first loop worked correctly.

                    Logger.Warning( $"Directory '{dir}' not found in dirToEntries during file processing." );

                    // Potentially create a new list here if there is a specific fallback behavior:
                    // dirToEntries[dir] = new List<TexturePackerEntry> { entry };
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
    /// Processes the specified input file or directory.
    /// </summary>
    /// <param name="inputFileOrDir"></param>
    /// <param name="outputRoot"> May be null if there is no output from processing the files. </param>
    /// <returns> the processed files added with <see cref="AddProcessedFile(TexturePackerEntry)"/>. </returns>
    public virtual List< TexturePackerEntry > ProcessIO( FileSystemInfo? inputFileOrDir,
                                                         DirectoryInfo? outputRoot )
    {
        if ( inputFileOrDir is not { Exists: true } )
        {
            throw new ArgumentException( $"IFileProcessor#Process: Input file/dir does not " +
                                         $"exist: {inputFileOrDir?.FullName}" );
        }

        List< TexturePackerEntry > retval;

        if ( IOUtils.IsFile( inputFileOrDir ) )
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
    /// <param name="entry"></param>
    public virtual void ProcessFile( TexturePackerEntry entry )
    {
        Guard.ThrowIfNull( entry.InputFile );

        if ( !CountOnly )
        {
            // Get the directory properly from FileSystemInfo
            DirectoryInfo? parentDirectory = null;

            if (entry.InputFile is FileInfo fileInfo)
            {
                parentDirectory = fileInfo.Directory;
            }
            else if (entry.InputFile is DirectoryInfo dirInfo)
            {
                parentDirectory = dirInfo.Parent;
            }
            else
            {
                // Fallback: get directory from the full path
                var parentPath = Path.GetDirectoryName(entry.InputFile.FullName);
                if (!string.IsNullOrEmpty(parentPath))
                {
                    parentDirectory = new DirectoryInfo(parentPath);
                }
            }

            var settings = parentDirectory != null 
                ? _dirToSettings.GetValueOrDefault(parentDirectory, _defaultSettings)
                : _defaultSettings;
            
            var packer = NewTexturePacker( _rootDirectory, settings );
            
            Pack( packer, entry );
        }

        AddProcessedFile( entry );
    }

    /// <summary>
    /// </summary>
    /// <param name="inputDir"></param>
    /// <param name="files"></param>
    public virtual void ProcessDir( TexturePackerEntry inputDir, List< TexturePackerEntry > files )
    {
        // Do not proceed if this dir is one of those to ignore
        if ( DirsToIgnore.Contains( inputDir.InputFile ) )
        {
            return;
        }

        // Find first parent with settings, or use defaults.
        TexturePacker.Settings? settings = null;

        var parent = ( DirectoryInfo? )inputDir.InputFile;

        for ( ; ( parent != null )
                && !_dirToSettings.TryGetValue( parent, out settings )
                && !parent.Equals( _rootDirectory );
             parent = parent.Parent )
        {
            // The loop continues as long as 'parent' is not null,
            // the settings for 'parent' are not found, and 'parent' is not the root directory.
            // 'parent' is updated to its parent in each iteration.
        }

        // After the loop, 'settings' will contain the found settings (if any),
        // and 'current' will be either null, the directory with settings, or the
        // root directory.

        settings ??= _defaultSettings;

        if ( settings.Ignore )
        {
            return;
        }

        if ( settings.CombineSubdirectories )
        {
            // Collect all files under subdirectories except those with a pack.json file.
            // A directory with its own settings can't be combined since combined directories
            // must use the settings of the parent directory.
            var combiningProcessor = new SettingsCombiningProcessor( inputDir, this );

            files = combiningProcessor.Process( ( DirectoryInfo? )inputDir.InputFile, null ).ToList();
        }

        if ( files.Count == 0 )
        {
            return;
        }

        if ( CountOnly )
        {
            _packCount++;

            return;
        }

        // Sort by name using numeric suffix, then alpha.
        files.Sort( ( entry1, entry2 ) =>
        {
            var full1    = entry1.InputFile?.Name;
            var dotIndex = full1?.LastIndexOf( '.' );

            if ( dotIndex != -1 )
            {
                full1 = full1?.Substring( 0, ( int )dotIndex! );
            }

            var full2 = entry2.InputFile?.Name;
            dotIndex = full2?.LastIndexOf( '.' );

            if ( dotIndex != -1 )
            {
                full2 = full2?.Substring( 0, ( int )dotIndex! );
            }

            var name1 = full1;
            var name2 = full2;
            var num1  = 0;
            var num2  = 0;

            var matcher = RegexUtils.NumberSuffixRegex().Match( full1! );

            if ( matcher.Success )
            {
                try
                {
                    num1  = int.Parse( matcher.Groups[ 2 ].Value );
                    name1 = matcher.Groups[ 1 ].Value;
                }
                catch ( Exception e )
                {
                    Logger.Warning( $"Exception ignored: {e.Message}" );
                }
            }

            matcher = RegexUtils.NumberSuffixRegex().Match( full2! );

            if ( matcher.Success )
            {
                try
                {
                    num2  = int.Parse( matcher.Groups[ 2 ].Value );
                    name2 = matcher.Groups[ 1 ].Value;
                }
                catch ( Exception e )
                {
                    Logger.Warning( $"Exception ignored: {e.Message}" );
                }
            }

            var compare = string.Compare( name1, name2, StringComparison.Ordinal );

            if ( ( compare != 0 ) || ( num1 == num2 ) )
            {
                return compare;
            }

            return num1 - num2;
        } );

        // Pack.
        if ( !settings.Silent )
        {
            try
            {
                Logger.Debug( $"Reading: {inputDir.InputFile?.FullName}" );
            }
            catch ( IOException )
            {
                Logger.Debug( $"Reading: {inputDir.InputFile?.FullName} ( IOException )" );
            }
        }

        if ( ProgressListener != null )
        {
            ProgressListener.Start( 1f / _packCount );

            string? inputPath = null;

            try
            {
                var rootPath = _rootDirectory.FullName;
                inputPath = inputDir.InputFile!.FullName;

                if ( inputPath.StartsWith( rootPath ) )
                {
                    rootPath = IOUtils.NormalizePath( rootPath );

                    inputPath = inputPath.Substring( rootPath.Length );
                    inputPath = IOUtils.NormalizePath( inputPath );

                    if ( inputPath.StartsWith( '/' ) )
                    {
                        inputPath = inputPath.Substring( 1 );
                    }
                }
            }
            catch ( IOException ioe )
            {
                Logger.Warning( ioe.Message );
            }

            if ( ( inputPath == null ) || ( inputPath.Length == 0 ) )
            {
                inputPath = inputDir.InputFile?.Name;
            }

            ProgressListener.Message = inputPath!;
        }

        var packer = NewTexturePacker( _rootDirectory, settings );

        foreach ( var file in files )
        {
            if ( file.InputFile == null )
            {
                continue;
            }

            packer.AddImage( ( FileInfo )file.InputFile );
        }

        Pack( packer, inputDir );

        ProgressListener?.End();
    }

    /// <summary>
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="settingsFile"></param>
    /// <exception cref="Exception"></exception>
    public static TexturePacker.Settings MergeSettings( TexturePacker.Settings settings, FileInfo settingsFile )
    {
        try
        {
            var data = settings.ReadFromJsonFile( settingsFile );

            settings.Merge( data );

            return settings;
        }
        catch ( Exception ex )
        {
            throw new Exception( $"Error reading settings file: {settingsFile}", ex );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="outputRoot"></param>
    public virtual void DeleteOutput( FileInfo outputRoot )
    {
        // Load root settings to get scale.
        var settingsFile = new FileInfo( Path.Combine( _rootDirectory.FullName, DEFAULT_PACKFILE_NAME ) );
        var rootSettings = _defaultSettings;

        if ( settingsFile.Exists )
        {
            rootSettings = NewSettings( rootSettings );
            MergeSettings( rootSettings, settingsFile );
        }

        var atlasExtension       = rootSettings.AtlasExtension;
        var quotedAtlasExtension = Regex.Escape( atlasExtension );

        for ( int i = 0, n = rootSettings.Scale.Length; i < n; i++ )
        {
            var deleteProcessor = new DeleteProcessor
            {
                Recursive = false,
            };

            var packFile = new FileInfo( rootSettings.GetScaledPackFileName( PackFileName, i ) );
            var prefix   = packFile.Name;
            var dotIndex = prefix.LastIndexOf( '.' );

            if ( dotIndex != -1 )
            {
                prefix = prefix.Substring( 0, dotIndex );
            }

            deleteProcessor.AddInputRegex( "(?i)" + prefix + @"-?\d*\.(png|jpg|jpeg)" );
            deleteProcessor.AddInputRegex( "(?i)" + prefix + quotedAtlasExtension );

            var dir = packFile.DirectoryName;

            if ( dir == null )
            {
                deleteProcessor.Process( outputRoot?.Directory!, null );
            }
            else if ( Directory.Exists( Path.Combine( outputRoot!.FullName, dir ) ) )
            {
                deleteProcessor.Process( new DirectoryInfo( Path.Combine( outputRoot.FullName, dir ) ), null );
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="packer"></param>
    /// <param name="texturePackerEntry"></param>
    public virtual void Pack( TexturePacker packer, TexturePackerEntry texturePackerEntry )
    {
        if ( texturePackerEntry.InputFile is FileInfo file )
        {
            packer.AddImage( file );
        }
        
        packer.Pack( ( DirectoryInfo )texturePackerEntry.OutputDirectory!, PackFileName );
    }

    /// <summary>
    /// Adds a processed <see cref="TexturePackerEntry"/> to the <see cref="OutputFilesList"/>.
    /// This method should be called by:-
    /// <li><see cref="ProcessFile(TexturePackerEntry)"/> or,</li>
    /// <li><see cref="ProcessDir(TexturePackerEntry, List{TexturePackerEntry})"/></li>
    /// if the return value of <see cref="Process(DirectoryInfo, DirectoryInfo)"/> or
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
    public virtual IFileProcessor AddInputRegex( params string[] regexes )
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
    /// <returns> This IFileProcessor for chaining. </returns>
    public IFileProcessor AddInputSuffix( params string[] suffixes )
    {
        foreach ( var suffix in suffixes )
        {
            AddInputRegex( $"(?i).*{Regex.Escape( suffix )}" );
        }

        return this;
    }

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="root"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public virtual TexturePacker NewTexturePacker( DirectoryInfo? root, TexturePacker.Settings settings )
    {
        var packer = new TexturePacker( root, settings )
        {
            ProgressListener = ProgressListener,
        };

        return packer;
    }

    /// <summary>
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    public virtual TexturePacker.Settings NewSettings( TexturePacker.Settings settings )
    {
        return new TexturePacker.Settings( settings );
    }
}