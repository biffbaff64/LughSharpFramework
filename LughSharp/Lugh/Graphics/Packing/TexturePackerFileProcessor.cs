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
using LughSharp.Lugh.Utils;
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Guarding;
using LughSharp.Lugh.Utils.Json;

namespace LughSharp.Lugh.Graphics.Packing;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public partial class TexturePackerFileProcessor : FileProcessor
{
    private readonly TexturePacker.Settings          _defaultSettings;
    private readonly TexturePacker.ProgressListener? _progressListener;

    private Dictionary< DirectoryInfo, TexturePacker.Settings > _dirToSettings = [ ]; //TODO: Rename
    private List< DirectoryInfo >                               _dirsToIgnore  = [ ];
    private List< FileInfo >                                    _settingsFiles = [ ];
    private Json                                                _json          = new();
    private DirectoryInfo                                       _rootDirectory;
    private string                                              _packFileName;
    private bool                                                _countOnly;
    private int                                                 _packCount;

    // ========================================================================

    /// <summary>
    /// </summary>
    public TexturePackerFileProcessor()
        : this( new TexturePacker.Settings(), "pack.atlas", null )
    {
    }

    /// <summary>
    /// </summary>
    /// <param name="defaultSettings"></param>
    /// <param name="packFileName"></param>
    /// <param name="progress"></param>
    public TexturePackerFileProcessor( TexturePacker.Settings? defaultSettings,
                                       string packFileName,
                                       TexturePacker.ProgressListener? progress )
    {
        this._defaultSettings  = defaultSettings ?? new TexturePacker.Settings();
        this._progressListener = progress ?? new TexturePacker.ProgressListenerImpl();

        if ( packFileName.ToLower().EndsWith( _defaultSettings.AtlasExtension.ToLower() ) )
        {
            packFileName = packFileName.Substring( 0, packFileName.Length - _defaultSettings.AtlasExtension.Length );
        }

        this._packFileName  = packFileName;
        this._rootDirectory = new FileInfo( packFileName ).Directory!;

        SetFlattenOutput( true );
        AddInputSuffix( ".png", ".jpg", ".jpeg" ); //TODO: Add .bmp

        // Sort input files by name to avoid platform-dependent atlas output changes.
        SetComparator( ( file1, file2 ) =>
                           string.Compare( file1.Name,
                                           file2.Name,
                                           StringComparison.Ordinal ) );
    }

    // ========================================================================
    
    /// <summary>
    /// </summary>
    /// <param name="inputFile"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>
    public override List< Entry > Process( FileInfo? inputFile, DirectoryInfo? outputRoot )
    {
        Guard.ThrowIfNull( inputFile?.Directory );

        _rootDirectory = inputFile.Directory;

        // Collect pack.json setting files.
        var settingsProcessor = new SettingsProcessor();

        settingsProcessor.FileProcessed += ( file ) => _settingsFiles.Add( file );
        settingsProcessor.AddInputRegex( "pack\\.json" );
        settingsProcessor.Process( inputFile, null );

        // Sort parent first.
        _settingsFiles.Sort( ( file1, file2 ) => file1.ToString().Length - file2.ToString().Length );

        foreach ( var settingsFile in _settingsFiles )
        {
            // Find first parent with settings, or use defaults.
            TexturePacker.Settings? settings = null;

            var parent = settingsFile.Directory;

            while ( true )
            {
                if ( ( parent != null ) && parent.Equals( _rootDirectory ) )
                {
                    break;
                }

                parent = parent?.Parent;

                Guard.ThrowIfNull( parent );

                if ( _dirToSettings.TryGetValue( parent, out settings ) )
                {
                    settings = NewSettings( settings );

                    break;
                }
            }

            if ( settings == null )
            {
                settings = NewSettings( _defaultSettings );
            }

            // Merge settings from current directory.
            Merge( settings, settingsFile );

            _dirToSettings[ settingsFile.Directory! ] = settings;
        }

        // Count the number of texture packer invocations.
        _countOnly = true;
        base.Process( inputFile, outputRoot );
        _countOnly = false;

        // Do actual processing.
        _progressListener?.Start( 1 );
        var result = base.Process( inputFile, outputRoot );
        _progressListener?.End();

        return result;
    }

    /// <summary>
    /// </summary>
    /// <param name="files"></param>
    /// <param name="outputRoot"></param>
    /// <returns></returns>
    public List< Entry > Process( FileInfo[] files, FileInfo? outputRoot )
    {
        Guard.ThrowIfNull( outputRoot );
        
        // Delete pack file and images.
        if ( _countOnly && outputRoot.Exists )
        {
            DeleteOutput( outputRoot );
        }

        return base.Process( files, outputRoot.Directory );
    }

    /// <summary>
    /// </summary>
    /// <param name="settings"></param>
    /// <param name="settingsFile"></param>
    /// <exception cref="Exception"></exception>
    public void Merge( TexturePacker.Settings settings, FileInfo settingsFile )
    {
        try
        {
            var root = new JsonReader().Parse( new StreamReader( settingsFile.FullName ) );

            if ( root == null )
            {
                return; // Empty file.
            }

            _json.ReadFields( settings, root );
        }
        catch ( Exception ex )
        {
            throw new Exception( "Error reading settings file: " + settingsFile, ex );
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="outputRoot"></param>
    protected void DeleteOutput( FileInfo? outputRoot )
    {
        // Load root settings to get scale.
        var settingsFile = new FileInfo( Path.Combine( _rootDirectory.FullName, "pack.json" ) );
        var rootSettings = _defaultSettings;

        if ( settingsFile.Exists )
        {
            rootSettings = NewSettings( rootSettings );
            Merge( rootSettings, settingsFile );
        }

        var atlasExtension       = rootSettings.AtlasExtension;
        var quotedAtlasExtension = Regex.Escape( atlasExtension );

        for ( int i = 0, n = rootSettings.Scale.Length; i < n; i++ )
        {
            var deleteProcessor = new DeleteProcessor();
            deleteProcessor.OnProcessFile += ( Entry inputFile ) => inputFile.InputFile?.Delete();
            deleteProcessor.SetRecursive( false );

            var packFile = new FileInfo( rootSettings.GetScaledPackFileName( _packFileName, i ) );
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
                deleteProcessor.Process( outputRoot, null );
            }
            else if ( Directory.Exists( Path.Combine( outputRoot!.FullName, dir ) ) )
            {
                deleteProcessor.Process( new FileInfo( Path.Combine( outputRoot.FullName, dir ) ), null );
            }
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="inputDir"></param>
    /// <param name="files"></param>
    public override void ProcessDir( Entry inputDir, List< Entry > files )
    {
        if ( _dirsToIgnore.Contains( inputDir.InputFile ) )
        {
            return;
        }

        // Find first parent with settings, or use defaults.
        TexturePacker.Settings? settings = null;

        var parent = ( DirectoryInfo? )inputDir.InputFile;

        while ( true )
        {
            if ( ( parent != null ) && _dirToSettings.TryGetValue( parent, out settings ) )
            {
                break;
            }

            if ( ( parent == null ) || parent.Equals( _rootDirectory ) )
            {
                break;
            }

            parent = parent.Parent;
        }

        if ( settings == null )
        {
            settings = _defaultSettings;
        }

        if ( settings.Ignore )
        {
            return;
        }

        if ( settings.CombineSubdirectories )
        {
            // Collect all files under subdirectories except those with a pack.json file.
            // A directory with its own settings can't be combined since combined directories
            // must use the settings of the parent directory.
            if ( settings.CombineSubdirectories )
            {
                var combiningProcessor = new CombiningProcessor();

                combiningProcessor.OnProcessDir += ( entryDir, fileList ) =>
                {
                    for ( var file = ( DirectoryInfo? )entryDir.InputFile;
                         ( file != null ) && !file.Equals( inputDir.InputFile );
                         file = file.Parent )
                    {
                        if ( new FileInfo( Path.Combine( file.FullName, "pack.json" ) ).Exists )
                        {
                            fileList.Clear();

                            return;
                        }
                    }

                    if ( !this._countOnly )
                    {
                        this._dirsToIgnore.Add( ( DirectoryInfo )entryDir.InputFile! );
                    }
                };

                combiningProcessor.OnProcessFile += ( entry ) => combiningProcessor.AddProcessedFile( entry );

                files = combiningProcessor.Process( ( FileInfo? )inputDir.InputFile, null ).ToList();
            }

            if ( files.Count == 0 )
            {
                return;
            }

            if ( _countOnly )
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

                var matcher = MyRegex1().Match( full1! );

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

                matcher = MyRegex1().Match( full2! );

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

            // Pack.
            if ( !settings.Silent )
            {
                try
                {
                    Console.WriteLine( "Reading: " + inputDir.InputFile?.FullName );
                }
                catch ( IOException )
                {
                    Console.WriteLine( "Reading: " + inputDir.InputFile?.FullName );
                }
            }

            if ( _progressListener != null )
            {
                _progressListener.Start( 1f / _packCount );

                string? inputPath = null;

                try
                {
                    var rootPath = _rootDirectory.FullName;
                    inputPath = inputDir.InputFile!.FullName;

                    if ( inputPath.StartsWith( rootPath ) )
                    {
                        rootPath  = rootPath.Replace( '\\', '/' );
                        inputPath = inputPath.Substring( rootPath.Length ).Replace( '\\', '/' );

                        if ( inputPath.StartsWith( '/' ) )
                        {
                            inputPath = inputPath.Substring( 1 );
                        }
                    }
                }
                catch ( IOException )
                {
                }

                if ( ( inputPath == null ) || ( inputPath.Length == 0 ) )
                {
                    inputPath = inputDir.InputFile?.Name;
                }

                _progressListener.Message = inputPath!;
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

            _progressListener?.End();
        }
    }

    /// <summary>
    /// </summary>
    /// <param name="packer"></param>
    /// <param name="inputDir"></param>
    protected virtual void Pack( TexturePacker packer, Entry inputDir )
    {
        if ( inputDir.OutputDirectory == null )
        {
            throw new GdxRuntimeException( "Cannot perform Pack, output directory is null" );
        }
        
        packer.Pack( inputDir.OutputDirectory, _packFileName );
    }

    /// <summary>
    /// </summary>
    /// <param name="root"></param>
    /// <param name="settings"></param>
    /// <returns></returns>
    protected virtual TexturePacker NewTexturePacker( DirectoryInfo root, TexturePacker.Settings settings )
    {
        var packer = new TexturePacker( root, settings );
        packer.SetProgressListener( _progressListener );

        return packer;
    }

    /// <summary>
    /// </summary>
    /// <param name="settings"></param>
    /// <returns></returns>
    protected virtual TexturePacker.Settings NewSettings( TexturePacker.Settings settings )
    {
        return new TexturePacker.Settings( settings );
    }

    /// <summary>
    /// </summary>
    /// <returns></returns>
    public TexturePacker.ProgressListener? GetProgressListener()
    {
        return _progressListener;
    }

    [GeneratedRegex( "(.*?)(\\d+)$" )] private static partial Regex MyRegex1();
}

// ============================================================================
// ============================================================================

[PublicAPI]
public class DeleteProcessor : FileProcessor
{
    /// <inheritdoc />
    public override void ProcessFile( Entry entry )
    {
        entry.InputFile?.Delete();
    }
}

// ============================================================================
// ============================================================================

[PublicAPI]
public class SettingsProcessor : FileProcessor
{
    /// <inheritdoc />
    public override void ProcessFile( Entry entry )
    {
        Guard.ThrowIfNull( entry );

        base.ProcessFile( entry );
    }
}

// ============================================================================
// ============================================================================

[PublicAPI]
public class CombiningProcessor : FileProcessor
{
//    /// <inheritdoc />
//    public override void ProcessDir( Entry entry, List< Entry > files )
//    {
//        for ( var file = ( DirectoryInfo? )entryDir.InputFile;
//             ( file != null ) && !file.Equals( inputDir.InputFile );
//             file = file.Parent )
//        {
//            if ( new FileInfo( Path.Combine( file.FullName, "pack.json" ) ).Exists )
//            {
//                files.Clear();
//
//                return;
//            }
//        }
//
//        if ( !this._countOnly )
//        {
//            this._dirsToIgnore.Add( ( DirectoryInfo )entryDir.InputFile! );
//        }
//    }
}