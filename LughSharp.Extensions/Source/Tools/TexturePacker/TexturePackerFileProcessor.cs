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

using System.Runtime.Versioning;
using System.Text.RegularExpressions;

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics.Text;

using LughUtils.source.Exceptions;
using LughUtils.source.Logging;

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class TexturePackerFileProcessor : FileProcessor
{
    public TexturePacker.TexturePackerProgressListener? ProgressListener { get; set; }
    public List< DirectoryInfo? >                       DirsToIgnore     { get; set; }
    public string                                       PackFileName     { get; set; }
    public bool                                         CountOnly        { get; set; } = false;

    // ========================================================================

    /// <summary>
    /// A dictionary of settings files to use for processing, grouped by directory.
    /// </summary>
    private Dictionary< DirectoryInfo, TexturePackerSettings > _dirToSettings;

    private readonly TexturePackerSettings _defaultSettings;
    private          int                   _packCount;
    private          DirectoryInfo         _rootDirectory = null!;

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

        InputRegex      = [ ];
        OutputFilesList = [ ];
        DirsToIgnore    = [ ];
        _dirToSettings  = [ ];
        _packCount      = 0;
        CountOnly       = false;
        OutputSuffix    = string.Empty;
        Recursive       = true;

        // Set the default file extensions for processable images.
        AddInputSuffix( ".png", ".jpg", ".jpeg" );

        // Sort input files by name to avoid platform-dependent atlas output changes.
        Comparator = ( file1, file2 ) => string.Compare( file1.Name,
                                                         file2.Name,
                                                         StringComparison.Ordinal );
    }

    // ========================================================================
    // ========================================================================

    #region Process methods

    /// <summary>
    /// Processes texture packing by analysing input and output directories, generating
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

        // Collect settings files from input directory and subdirectories.
        _ = CollectSettingsFiles( inputRoot, outputRoot );

        // Count the number of texture packer invocations for the ProgressListener
        // to use. This is done by a dry run with CountOnly = true, and will set the
        // _packCount variable to the number of packer invocations.
        CountOnly = true;
        _         = base.Process( inputRoot, outputRoot );
        CountOnly = false;
        
        // Do actual processing, returning a List<> of Entry objects.
        ProgressListener?.Start( 1.0f );
        var result = base.Process( inputRoot, outputRoot );
        ProgressListener?.End();

        return result;
    }

    /// <summary>
    /// Processes texture packing for the given array of files, handling deletion
    /// of output files and directories if necessary.
    /// </summary>
    /// <param name="files"> the array of files to process. </param>
    /// <param name="outputRoot"> The output folder. </param>
    /// <returns></returns>
    public override List< Entry > Process( FileInfo[] files, DirectoryInfo? outputRoot )
    {
        Guard.Against.Null( outputRoot );

        // Delete pack file and images.
        if ( CountOnly && outputRoot.Exists )
        {
            DeleteOutput( outputRoot );
        }

        return base.Process( files, outputRoot );
    }

    /// <summary>
    /// Processes a single directory entry for texture packing. Determines the parent directory,
    /// retrieves the appropriate settings, creates a new <see cref="TexturePacker"/> instance,
    /// and performs packing if not in count-only mode. Adds the processed file to the result list.
    /// </summary>
    /// <param name="inputDir"> The directory to pack. </param>
    /// <param name="entryList"> The resulting list of entries. </param>
    public override void ProcessDir( Entry inputDir, List< Entry > entryList )
    {
        // Do not proceed if this input file is in the ignore list.
        if ( DirsToIgnore.Contains( inputDir.InputFile ) )
        {
            return;
        }

        // Find first parent with settings, or use defaults.
        TexturePackerSettings? settings = null;

        var parent = ( DirectoryInfo? )inputDir.InputFile;

        // Note:
        // The loop continues as long as 'parent' is not null, the settings for
        // 'parent' are not found, and 'parent' is not the root directory.
        // 'parent' is updated to its own parent in each iteration.
        while ( true )
        {
            if ( parent != null )
            {
                _dirToSettings.TryGetValue( parent, out settings );
            }

            if ( settings != null )
            {
                break;
            }

            if ( ( parent == null ) || parent.Equals( _rootDirectory ) )
            {
                break;
            }

            parent = parent.Parent;
        }

        // Note:
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
            // Collect all files under subdirectories except for those directories with a
            // pack.json file. A directory with its own settings can't be combined since
            // combined directories must use the settings of the parent directory.
            
            var combiningProcessor = new CombiningProcessor( this );
            
            entryList = combiningProcessor.Process( inputDir.InputFile, null ).ToList();
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
                    Logger.Error( $"Exception ignored: {e.Message}" );
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
                    Logger.Error( $"Exception ignored: {e.Message}" );
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

        // ---------- Pack ----------

        if ( !_defaultSettings.Silent )
        {
            try
            {
                Logger.Debug( $"Reading: {sourceString}" );
            }
            catch ( IOException ioe )
            {
                Logger.Debug( $"Reading: {sourceString} ( IOException )" );
                Logger.Error( $"IOException while reading: {ioe.Message}" );
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
                    rootPath  = IOUtils.NormalizePath( rootPath );
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
                Logger.Error( ioe.Message );
            }

            if ( ( inputPath == null ) || ( inputPath.Length == 0 ) )
            {
                inputPath = inputDir.InputFile?.Name;
            }

            ProgressListener.Message = inputPath!;
        }

        // ---------- Pack Images ----------

        var packer = NewTexturePacker( _rootDirectory, settings );

        // Add all the gathered images to the packer.
        foreach ( var file in entryList )
        {
            if ( file.InputFile != null )
            {
                packer.AddImage( ( FileInfo )file.InputFile );
            }
        }

        // Do the actual packing.
        Pack( packer, inputDir );

        ProgressListener?.End();
    }

    #endregion Process methods

    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------
    // ------------------------------------------------------------------------

    // ========================================================================
    // ========================================================================

    /// <summary>
    /// </summary>
    /// <param name="packer"></param>
    /// <param name="entry"></param>
    public void Pack( TexturePacker packer, Entry entry )
    {
        packer.Pack( entry.OutputDirectory!, PackFileName );
    }

    /// <summary>
    /// Collect any pack.json setting files present in the input folder.
    /// </summary>
    /// <param name="inputRoot"> The directory containing the images or assets that will be packed. </param>
    /// <param name="outputRoot"> The directory where the packed texture files and metadata will be saved. </param>
    /// <returns> The number of settings files found. </returns>
    public int CollectSettingsFiles( DirectoryInfo inputRoot, DirectoryInfo? outputRoot )
    {
        //@formatter:off
        var settingsFiles = Directory.EnumerateFiles
        (
            inputRoot.FullName,
            "*.*",                      // Search pattern for all files
            SearchOption.AllDirectories // Search all subdirectories
        )
        // First, filter the paths based on the regex pattern
        .Where( filePath => Regex.IsMatch( Path.GetFileName( filePath ), @"pack\.json" ) )
        // Second, project/map each resulting file path string to a new FileInfo object
        .Select( filePath => new FileInfo( filePath ) )
        // Finally, convert the resulting sequence into a List<FileInfo>
        .ToList();
        //@formatter:on

        ProgressListener ??= new TexturePacker.TexturePackerProgressListener();

        if ( settingsFiles.Count == 0 )
        {
            // No settings files found
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

        return settingsFiles.Count;
    }

    /// <summary>
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="settingsFile"></param>
    /// <exception cref="Exception"></exception>
    public TexturePackerSettings MergeSettings( TexturePackerSettings settings, FileInfo settingsFile )
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
    /// This only deletes files and directories that relate to the source folder
    /// being packed. For instance, if the source folder is "Assets/Images", and
    /// the output folder is "Assets/Textures", then only the output files and directories
    /// for "Assets/Textures/Images" will be deleted, leaving all other files intact.
    /// </summary>
    /// <param name="outputRoot">
    /// The root file where the output files and directories to be deleted are located.
    /// </param>
    public virtual void DeleteOutput( DirectoryInfo outputRoot )
    {
        // Load root settings to get scale.
        var settingsFile = new FileInfo( Path.Combine( _rootDirectory.FullName, DEFAULT_PACKFILE_NAME ) );
        var rootSettings = _defaultSettings;

        if ( settingsFile.Exists )
        {
            rootSettings = NewSettings( rootSettings );
            MergeSettings( rootSettings, settingsFile );
        }

        var quotedAtlasExtension = Regex.Escape( rootSettings.AtlasExtension );
        
        for ( int i = 0, n = rootSettings.Scale.Length; i < n; i++ )
        {
            var inputRegexes = new List< Regex >();
            var packFile = new FileInfo( rootSettings.GetScaledPackFileName( PackFileName, i ) );

            var prefix   = packFile.Name;
            var dotIndex = prefix.LastIndexOf( '.' );

            if ( dotIndex != -1 )
            {
                prefix = prefix.Substring( 0, dotIndex );
            }

            inputRegexes.Add( new Regex( "(?i)" + prefix + @"-?\d*\.(png|jpg|jpeg)" ) );
            inputRegexes.Add( new Regex( "(?i)" + prefix + quotedAtlasExtension ) );

            var dir = packFile.DirectoryName;

            if ( dir == null )
            {
                ClearOutputFolder( outputRoot, inputRegexes );
            }
            else if ( Directory.Exists( outputRoot.FullName ) )
            {
                ClearOutputFolder( new DirectoryInfo( outputRoot.FullName ), inputRegexes );
            }
        }
    }
    
    public static void ClearOutputFolder( DirectoryInfo outputRoot, List< Regex > inputRegexes )
    {
        // Define the folder path where you want to start deleting files
        var targetDirectoryPath = outputRoot.FullName;

        try
        {
            //@formatter:off
            // Directory.EnumerateFiles gets all file paths in the directory and all subdirectories
            var filesToDelete = Directory.EnumerateFiles(
                targetDirectoryPath, 
                "*.*", 
                SearchOption.AllDirectories
            )
            // Filter the file paths
            .Where( filePath => 
            {
                // Get just the file name to match against the regexes
                var fileName = Path.GetFileName( filePath );
                
                // Check if ANY of the Regex objects in the list match the file name
                return inputRegexes.Any( regex => regex.IsMatch( fileName ) );
            } );
            //@formatter:on

            // Iterate through the filtered paths and delete each file
            foreach ( var filePath in filesToDelete )
            {
                File.Delete( filePath );
            }
        }
        catch ( DirectoryNotFoundException )
        {
            Logger.Debug( $"Error: Directory not found at {targetDirectoryPath}" );
        }
        catch ( UnauthorizedAccessException )
        {
            Logger.Debug( "Error: Access denied. Check file permissions." );
        }
        catch ( Exception ex )
        {
            Logger.Debug( $"An unexpected error occurred: {ex.Message}" );
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
    
    // ========================================================================
    // ========================================================================

    private class CombiningProcessor( TexturePackerFileProcessor parent ) : FileProcessor
    {
        public override void ProcessFile( Entry entry )
        {
            AddProcessedFile( entry );
        }

        public override void ProcessDir( Entry entryDir, List< Entry > files )
        {
            var file = entryDir.InputFile as DirectoryInfo;

            while ( ( file != null ) && !file.Equals( entryDir.InputFile ) )
            {
                var packJson = new FileInfo( Path.Combine( file.FullName, "pack.json" ) );

                if ( packJson.Exists )
                {
                    files.Clear();

                    return;
                }

                file = file.Parent;
            }

            if ( !parent.CountOnly )
            {
                parent.DirsToIgnore.Add( entryDir.InputFile as DirectoryInfo );
            }
        }
    }
}

// ============================================================================
// ============================================================================