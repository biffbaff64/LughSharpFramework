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
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Logging;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class TexturePackerFileProcessor : FileProcessor
{
    public TexturePacker.TexturePackerProgressListener? ProgressListener { get; set; }

    public List< DirectoryInfo > DirsToIgnore { get; set; }
    public bool                  CountOnly    { get; set; }
    public string                PackFileName { get; set; }

    // ========================================================================

    private readonly TexturePackerSettings _defaultSettings;

    private Dictionary< DirectoryInfo, TexturePackerSettings > _dirToSettings; //TODO: Rename

    private int           _packCount;
    private DirectoryInfo _rootDirectory = null!;

    // ========================================================================

    /// <summary>
    /// Default constructor. Processes Texture packing using default settings
    /// and the default packfile name ( "pack.atlas" ).
    /// </summary>
    public TexturePackerFileProcessor()
        : this( new TexturePackerSettings(), DEFAULT_PACKFILE_NAME )
    {
    }

    /// <summary>
    /// Constructs a new TexturePackerFileProcessor object, using the supplied
    /// <see cref="TexturePackerSettings"/>.
    /// </summary>
    /// <param name="packerSettings"> The Settings to use. </param>
    /// <param name="packFileName"> The name of the final packed file. </param>
    /// <param name="progress"> The ProgressListener to use. Can be null. </param>
    public TexturePackerFileProcessor( TexturePackerSettings? packerSettings,
                                       string packFileName,
                                       TexturePacker.TexturePackerProgressListener? progress = null )
    {
        _defaultSettings = packerSettings ?? new TexturePackerSettings();
        ProgressListener = progress;

        if ( packFileName.ToLower().EndsWith( _defaultSettings.AtlasExtension.ToLower() ) )
        {
            packFileName = packFileName.Substring( 0, packFileName.Length - _defaultSettings.AtlasExtension.Length );
        }

        PackFileName  = packFileName;
        FlattenOutput = true;

        // Set the default file extensions for processable images.
        AddInputSuffix( ".png", ".jpg", ".jpeg" );

        InputRegex      = [ ];
        OutputFilesList = [ ];
        DirsToIgnore    = [ ];
        _dirToSettings  = [ ];
        _packCount      = 0;
        CountOnly       = false;
        OutputSuffix    = string.Empty;
        Recursive       = true;

        // Sort input files by name to avoid platform-dependent atlas output changes.
        Comparator = ( file1, file2 ) => string.Compare( file1.Name,
                                                         file2.Name,
                                                         StringComparison.Ordinal );
    }

    // ========================================================================
    // ========================================================================

    #region Process methods

    /// <summary>
    /// Processes texture packing by analyzing input and output directories, generating
    /// packed texture files and associated data, and returning the results.
    /// </summary>
    /// <param name="inputRoot">
    /// The directory containing the images or assets that will be packed.
    /// </param>
    /// <param name="outputRoot">
    /// The directory where the packed texture files and metadata will be saved.
    /// </param>
    /// <returns>
    /// A list of <see cref="FileProcessor.Entry"/> objects representing the processed entries.
    /// </returns>
    public virtual List< Entry > Process( DirectoryInfo? inputRoot, DirectoryInfo? outputRoot )
    {
        _rootDirectory = inputRoot ?? throw new ArgumentNullException( nameof( inputRoot ) );

        CollectSettingsFiles( inputRoot, outputRoot );

        // Count the number of texture packer invocations for the ProgressListener
        // to use. This is done by a dry run with CountOnly = true, and will set the
        // _packCount variable to the number of packer invocations.
        // The result from the base.Process method is discarded.
        CountOnly = true;
        _         = base.Process( inputRoot, outputRoot );
        CountOnly = false;

        // Do actual processing.
        ProgressListener?.Start( 1.0f );
        var result = base.Process( inputRoot, outputRoot );
        ProgressListener?.End();

        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>
    public virtual List< Entry > Process( FileInfo[] files, FileInfo? outputRoot )
    {
        Guard.ThrowIfNull( outputRoot );

        // Delete pack file and images.
        if ( CountOnly && outputRoot.Exists )
        {
            DeleteOutput( outputRoot );
        }

        return base.Process( files, outputRoot.Directory );
    }

    /// <summary>
    /// </summary>
    /// <param name="inputDir"></param>
    /// <param name="entryList"></param>
    public override void ProcessDir( Entry inputDir, List< Entry > entryList )
    {
        // Do not proceed if this dir is one of those to ignore
        if ( DirsToIgnore.Contains( inputDir.InputFile ) )
        {
            return;
        }

        // Find first parent with settings, or use defaults.
        TexturePackerSettings? settings = null;

        var parent = ( DirectoryInfo? )inputDir.InputFile;

        for ( ;
             ( parent != null )
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
            var combiningProcessor = new CombineSettingsProcessor( inputDir, this );

            entryList = combiningProcessor.Process( ( DirectoryInfo? )inputDir.InputFile, null ).ToList();
        }

        if ( entryList.Count == 0 )
        {
            return;
        }

        if ( CountOnly )
        {
            _packCount++;

            return;
        }

        // Sort by name using numeric suffix, then alpha.
        entryList.Sort( ( entry1, entry2 ) =>
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

        var sourceString = inputDir.InputFile?.FullName.Substring( IOUtils.AssetsRoot.Length );

        // Pack.
        try
        {
            Logger.Debug( $"Reading: {sourceString}" );
        }
        catch ( IOException ioe )
        {
            Logger.Debug( $"Reading: {sourceString} ( IOException )" );
            Logger.Warning( $"IOException while reading: {ioe.Message}" );
        }

        if ( ProgressListener != null )
        {
            Logger.Debug( "Starting ProgressListener" );

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

        foreach ( var file in entryList )
        {
            if ( file.InputFile != null )
            {
                packer.AddImage( ( FileInfo )file.InputFile );
            }
        }

        Pack( packer, inputDir );

        ProgressListener?.End();
    }

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

//    /// <summary>
//    /// Processes the given array of files starting from a specific output directory
//    /// and populates a mapping of directories to their corresponding entries. Handles
//    /// recursive traversal and processing based on the specified depth.
//    /// </summary>
//    /// <param name="files">
//    /// An array of <see cref="FileInfo"/> objects representing the files to be processed.
//    /// </param>
//    /// <param name="outputRoot">
//    /// The root output directory where the processed files will be stored.
//    /// </param>
//    /// <param name="outputDir">
//    /// The currently targeted output directory during the processing operation.
//    /// </param>
//    /// <param name="dirToEntries">
//    /// A dictionary mapping each discovered directory to a list of associated
//    /// <see cref="FileProcessor.Entry"/> objects.
//    /// </param>
//    /// <param name="depth">
//    /// The current recursion depth during processing, used to manage directory
//    /// hierarchy and traversal.
//    /// </param>
//    public override void Process( FileInfo[] files,
//                                  DirectoryInfo outputRoot, DirectoryInfo outputDir,
//                                 Dictionary< DirectoryInfo, List< Entry >? > dirToEntries, int depth )
//    {
//        foreach ( var file in files )
//        {
//            var dir = file.Directory;
//
//            if ( dir != null )
//            {
//                if ( !dirToEntries.ContainsKey( dir ) )
//                {
//                    // If the directory is not already a key in the dictionary, add it
//                    dirToEntries[ dir ] = [ ];
//                }
//            }
//            else
//            {
//                // Handle the case where a file has no parent directory (e.g., root level)
//                // Either log a warning, throw an exception, or handle it differently
//                // ( Logging a warning for now... )
//                Logger.Warning( $"File '{file.FullName}' has no parent directory." );
//            }
//        }
//
//        foreach ( var file in files )
//        {
//            if ( IOUtils.IsFile( file ) )
//            {
//                if ( InputRegex.Count > 0 )
//                {
//                    var found = false;
//
//                    foreach ( var pattern in InputRegex )
//                    {
//                        if ( pattern.IsMatch( file.Name ) )
//                        {
//                            found = true;
//
//                            break;
//                        }
//                    }
//
//                    if ( !found )
//                    {
//                        continue;
//                    }
//                }
//
//                if ( file.DirectoryName == null )
//                {
//                    Logger.Debug( $"file.DirectoryName is null: {file.FullName}" );
//                }
//
//                if ( ( FilenameFilterDelegate != null )
//                     && !FilenameFilterDelegate( file.Directory!.FullName, file.Name ) )
//                {
//                    continue;
//                }
//
//                var outputName = file.Name;
//
//                if ( OutputSuffix != null )
//                {
//                    outputName = RegexUtils.FileNameWithoutExtensionRegex().Replace( outputName, "$1" ) + OutputSuffix;
//                }
//
//                var entry = new Entry
//                {
//                    Depth           = depth,
//                    InputFile       = file,
//                    OutputDirectory = outputDir,
//                    OutputFileName = FlattenOutput
//                        ? Path.Combine( outputRoot.FullName, outputName )
//                        : Path.Combine( outputDir.FullName, outputName ),
//                };
//
//                var dir = file.Directory!;
//
//                if ( !dirToEntries.TryGetValue( dir, out var dirList ) )
//                {
//                    dirList             = [ ];
//                    dirToEntries[ dir ] = dirList;
//                }
//                
//                dirList?.Add( entry );
//            }
//
//            if ( Recursive && IOUtils.IsDirectory( file ) )
//            {
//                var subdir = outputDir.FullName.Length == 0
//                    ? new DirectoryInfo( file.Name )
//                    : new DirectoryInfo( Path.Combine( outputDir.FullName, file.Name ) );
//
//                Process( new DirectoryInfo( file.FullName )
//                         .GetFileSystemInfos().Select( f => new FileInfo( f.FullName ) ).ToArray(),
//                         outputRoot, subdir, dirToEntries, depth + 1 );
//            }
//        }
//    }

//    /// <summary>
//    /// </summary>
//    /// <param name="files"></param>
//    /// <param name="outputRoot"></param>
//    /// <param name="outputDir"></param>
//    /// <param name="dirToEntries"></param>
//    /// <param name="depth"></param>
//    public override void Process( FileInfo[] files,
//                                  DirectoryInfo outputRoot,
//                                  DirectoryInfo outputDir,
//                                  Dictionary< string, List< Entry > > dirToEntries,
//                                  int depth )
//    {
//        foreach ( var file in files )
//        {
//            var dir = file.Directory;
//
//            if ( dir != null )
//            {
//                if ( !dirToEntries.ContainsKey( dir.FullName ) )
//                {
//                    // If the directory is not already a key in the dictionary, add it
//                    dirToEntries[ dir.FullName ] = [ ];
//                }
//            }
//            else
//            {
//                // Handle the case where a file has no parent directory (e.g., root level)
//                // Either log a warning, throw an exception, or handle it differently
//                // ( Logging a warning for now... )
//                Logger.Warning( $"WARNING: File '{file.FullName}' has no parent directory." );
//            }
//        }
//
//        foreach ( var file in files )
//        {
//            if ( IOUtils.IsFile( file ) )
//            {
//                if ( InputRegex.Count > 0 )
//                {
//                    var found = false;
//
//                    foreach ( var pattern in InputRegex )
//                    {
//                        if ( pattern.IsMatch( file.Name ) )
//                        {
//                            found = true;
//
//                            break;
//                        }
//                    }
//
//                    if ( !found )
//                    {
//                        continue;
//                    }
//                }
//
//                if ( ( FilenameFilterDelegate != null )
//                     && !FilenameFilterDelegate( file.Directory!.FullName, file.Name ) )
//                {
//                    continue;
//                }
//
//                var outputName = file.Name;
//
//                if ( OutputSuffix != null )
//                {
//                    outputName = RegexUtils.FileNameWithoutExtensionRegex().Replace( outputName, "$1" )
//                                 + OutputSuffix;
//                }
//
//                var entry = new Entry
//                {
//                    Depth           = depth,
//                    InputFile       = file,
//                    OutputDirectory = outputDir,
//                    OutputFileName = FlattenOutput
//                        ? Path.Combine( outputRoot.FullName, outputName )
//                        : Path.Combine( outputDir.FullName, outputName ),
//                };
//
//                var dir = file.Directory!.FullName;
//
//                if ( dirToEntries.TryGetValue( dir, out var value ) )
//                {
//                    value.Add( entry );
//                }
//                else
//                {
//                    // This should ideally not happen if the first loop worked correctly.
//
//                    Logger.Warning( $"Directory '{dir}' not found in dirToEntries during file processing." );
//
//                    // TODO: Potentially create a new list here if there is a specific fallback behavior:
//                    // dirToEntries[dir] = new List<Entry> { entry };
//                }
//            }
//
//            if ( Recursive && IOUtils.IsDirectory( file ) )
//            {
//                var subdir = outputDir.FullName.Length == 0
//                    ? new DirectoryInfo( file.Name )
//                    : new DirectoryInfo( Path.Combine( outputDir.FullName, file.Name ) );
//
//                Process( new DirectoryInfo( file.FullName )
//                         .GetFileSystemInfos().Select( f => new FileInfo( f.FullName ) ).ToArray(),
//                         outputRoot, subdir, dirToEntries, depth + 1 );
//            }
//        }
//    }

    /// <summary>
    /// </summary>
    /// <param name="entry"></param>
    public override void ProcessFile( Entry entry )
    {
        Guard.ThrowIfNull( entry.InputFile );

        if ( !CountOnly )
        {
            // Get the directory properly from FileSystemInfo
            DirectoryInfo? parentDirectory = null;

            if ( entry.InputFile is FileInfo fileInfo )
            {
                parentDirectory = fileInfo.Directory;
            }
            else if ( entry.InputFile is DirectoryInfo dirInfo )
            {
                parentDirectory = dirInfo.Parent;
            }
            else
            {
                // Fallback: get directory from the full path
                var parentPath = Path.GetDirectoryName( entry.InputFile.FullName );

                if ( !string.IsNullOrEmpty( parentPath ) )
                {
                    parentDirectory = new DirectoryInfo( parentPath );
                }
            }

            var settings = parentDirectory != null
                ? _dirToSettings.GetValueOrDefault( parentDirectory, _defaultSettings )
                : _defaultSettings;

            var packer = NewTexturePacker( _rootDirectory, settings );

            Pack( packer, entry );
        }

        AddProcessedFile( entry );
    }

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    #endregion Process methods

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="packer"></param>
    /// <param name="entry"></param>
    public virtual void Pack( TexturePacker packer, Entry entry )
    {
        packer.Pack( entry.OutputDirectory!, PackFileName );
    }

    /// <summary>
    /// Collect any pack.json setting files present in the input folder.
    /// </summary>
    /// <param name="inputRoot">
    /// The directory containing the images or assets that will be packed.
    /// </param>
    /// <param name="outputRoot">
    /// The directory where the packed texture files and metadata will be saved.
    /// </param>
    public void CollectSettingsFiles( DirectoryInfo? inputRoot, DirectoryInfo? outputRoot )
    {
        var settingsProcessor = new PackingSettingsProcessor();
        settingsProcessor.AddInputRegex( "pack\\.json" );
        _ = settingsProcessor.Process( inputRoot, outputRoot );

        ProgressListener ??= new TexturePacker.TexturePackerProgressListener();

        var settingsFiles = settingsProcessor.SettingsFiles;

        // -----------------------------------------------------

        if ( settingsFiles.Count == 0 )
        {
            // No settings files found.
            Logger.Debug( "No settings files found." );
        }
        else
        {
            // Sort parent first.
            settingsFiles.Sort( ( file1, file2 ) => file1.ToString().Length - file2.ToString().Length );

            // Establish the settings to use for processing
            foreach ( var settingsFile in settingsFiles )
            {
                // Find first parent with settings, or use defaults.
                var settings = default( TexturePackerSettings );
                var parent   = settingsFile.Directory;

                while ( ( parent != null ) && !parent.Equals( _rootDirectory ) )
                {
                    parent = parent.Parent;

                    if ( parent != null )
                    {
                        _dirToSettings.TryGetValue( parent, out settings );

                        if ( settings != null )
                        {
                            settings = NewSettings( settings );
                        }
                    }
                }

                settings ??= NewSettings( _defaultSettings );

                // Merge settings from current directory.
                MergeSettings( settings, settingsFile );

                _dirToSettings.Add( settingsFile.Directory!, settings );
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="settingsFile"></param>
    /// <exception cref="Exception"></exception>
    public static TexturePackerSettings MergeSettings( TexturePackerSettings settings, FileInfo settingsFile )
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
    /// Deletes the output files and directories related to the given root file,
    /// based on the texture packing settings and associated patterns.
    /// </summary>
    /// <param name="outputRoot">
    /// The root file where the output files and directories to be deleted are located.
    /// </param>
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

    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="root"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    public virtual TexturePacker NewTexturePacker( DirectoryInfo? root, TexturePackerSettings settings )
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
    public virtual TexturePackerSettings NewSettings( TexturePackerSettings settings )
    {
        return new TexturePackerSettings( settings );
    }
}