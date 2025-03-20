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

using LughSharp.Lugh.Files;
using LughSharp.Lugh.Graphics.Packing;
using LughSharp.Lugh.Utils.Collections;
using LughSharp.Lugh.Utils.Exceptions;
using LughSharp.Lugh.Utils.Guarding;

using File = System.IO.File;

namespace LughSharp.Lugh.Graphics.Packing;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class TexturePackerFileProcessor : FileProcessor
{
//    private JsonObject _json = new();

    private readonly TexturePacker.Settings                        _defaultSettings;
    private readonly TexturePacker.ProgressListener?               _progress;
    private          ObjectMap< FileInfo, TexturePacker.Settings > _dirToSettings = new();
    private          string                                        _packFileName;
    private          FileInfo?                                     _root;
    private          List< FileInfo >                              _ignoreDirs = [ ];
    private          bool                                          _countOnly;
    private          int                                           _packCount;

    // ========================================================================
    // ========================================================================

    public TexturePackerFileProcessor()
        : this( new TexturePacker.Settings(), "pack.atlas", null )
    {
    }

    public TexturePackerFileProcessor( TexturePacker.Settings defaultSettings,
                                       string packFileName,
                                       TexturePacker.ProgressListener? progress )
    {
        this._defaultSettings = defaultSettings;
        this._progress        = progress;

        if ( packFileName.ToLower().EndsWith( defaultSettings.AtlasExtension.ToLower() ) )
        {
            packFileName = packFileName.Substring( 0, packFileName.Length - defaultSettings.AtlasExtension.Length );
        }

        this._packFileName = packFileName;

        SetFlattenOutput( true );
        AddInputSuffix( ".png", ".jpg", ".jpeg" );

        // Sort input files by name to avoid platform-dependent atlas output changes.
        SetComparator( new Comparison< DirectoryInfo? >()
        {
 
            public int Compare(FileInfo file1, FileInfo file2)
            {
            return file1.getName().compareTo(file2.getName());
        }
        });
    }

    public List< ObjectMap< FileInfo, TexturePacker.Settings >.Entry > Process( FileInfo inputFile, FileInfo outputRoot )
    {
        _root = inputFile;

        // Collect pack.json setting files.
        List< FileInfo > settingsFiles = [ ];

        var settingsProcessor = new FileProcessor()
        {
 

            protected void processFile( ObjectMap<,  >.Entry inputFile ){ settingsFiles.add(inputFile.inputFile);
        }
        };

        settingsProcessor.AddInputRegex( "pack\\.json" );
        settingsProcessor.Process( inputFile, null );

        // Sort parent first.
        settingsFiles.Sort();

        Collections.sort( settingsFiles, new Comparator< File >()
        {
 

            public int compare (File file1, File file2) {
            return file1.toString().length() - file2.toString().length();
        }

        });
        for ( File settingsFile :
        settingsFiles) {
            // Find first parent with settings, or use defaults.
            TexturePacker.Settings settings = null;
            File                   parent   = settingsFile.getParentFile();

            while ( true )
            {
                if ( parent.equals( root ) ) break;

                parent   = parent.getParentFile();
                settings = dirToSettings.get( parent );

                if ( settings != null )
                {
                    settings = newSettings( settings );

                    break;
                }
            }

            if ( settings == null ) settings = newSettings( defaultSettings );

            // Merge settings from current directory.
            merge( settings, settingsFile );
            dirToSettings.put( settingsFile.getParentFile(), settings );
        }

// Count the number of texture packer invocations.
        countOnly = true;
        super.process( inputFile, outputRoot );
        countOnly = false;

// Do actual processing.
        if ( progress != null ) progress.start( 1 );
        List< ObjectMap< , >.Entry > result = super.process( inputFile, outputRoot );
        if ( progress != null ) progress.end();

        return result;
    }

    void merge( TexturePacker.Settings settings, File settingsFile )
    {
        try
        {
            JsonValue root = new JsonReader().parse( new FileReader( settingsFile ) );

            if ( root == null ) return; // Empty file.

            json.readFields( settings, root );
        }
        catch ( Exception ex )
        {
            throw new GdxRuntimeException( "Error reading settings file: " + settingsFile, ex );
        }
    }

    public List< ObjectMap< , >.Entry > process( File[] files, File outputRoot )

    {
        // Delete pack file and images.
        if ( countOnly && outputRoot.exists() ) deleteOutput( outputRoot );

        return base.Process( files, outputRoot );
    }

    protected void deleteOutput( File outputRoot )
    {
        // Load root settings to get scale.
        File                   settingsFile = new File( root, "pack.json" );
        TexturePacker.Settings rootSettings = defaultSettings;

        if ( settingsFile.exists() )
        {
            rootSettings = newSettings( rootSettings );
            merge( rootSettings, settingsFile );
        }

        string atlasExtension = rootSettings.atlasExtension == null ? "" : rootSettings.atlasExtension;
        atlasExtension = Pattern.quote( atlasExtension );

        for ( int i = 0, n = rootSettings.scale.length; i < n; i++ )
        {
            FileProcessor deleteProcessor = new FileProcessor()
            {
 

                protected void processFile (Entry inputFile){
                inputFile.inputFile.delete();
            }
            };
            deleteProcessor.setRecursive( false );

            File   packFile           = new File( rootSettings.getScaledPackFileName( packFileName, i ) );
            string scaledPackFileName = packFile.getName();

            string prefix                = packFile.getName();
            int    dotIndex              = prefix.lastIndexOf( '.' );
            if ( dotIndex != -1 ) prefix = prefix.substring( 0, dotIndex );
            deleteProcessor.addInputRegex( "(?i)" + prefix + "-?\\d*\\.(png|jpg|jpeg)" );
            deleteProcessor.addInputRegex( "(?i)" + prefix + atlasExtension );

            string dir = packFile.getParent();
            if ( dir == null )
                deleteProcessor.process( outputRoot, null );
            else if ( new File( outputRoot + "/" + dir ).exists() ) //
                deleteProcessor.process( outputRoot + "/" + dir, null );
        }
    }

    protected void processDir( Entry
                                   inputDir, List< ObjectMap< , >.Entry > files )
    {
        if ( ignoreDirs.contains( inputDir.inputFile ) ) return;

        // Find first parent with settings, or use defaults.
        TexturePacker.Settings settings = null;
        File                   parent   = inputDir.inputFile;

        while ( true )
        {
            settings = dirToSettings.get( parent );

            if ( settings != null ) break;
            if ( parent == null || parent.equals( root ) ) break;

            parent = parent.getParentFile();
        }

        if ( settings == null ) settings = defaultSettings;

        if ( settings.ignore ) return;

        if ( settings.combineSubdirectories )
        {
            // Collect all files under subdirectories except those with a pack.json file. A directory with its own settings can't be
            // combined since combined directories must use the settings of the parent directory.
            files = new FileProcessor( this )
            {
 

                protected void processDir (Entry entryDir, List < ObjectMap< , >.Entry > files) {
                File file = entryDir.inputFile;
                while (file != null && !file.equals(inputDir.inputFile)) {
                if (new File(file, "pack.json").exists()) {
                files.clear();
                return;
            }
            file = file.getParentFile();
            }
            if ( !countOnly ) ignoreDirs.add( entryDir.inputFile );
            }

            protected void processFile( ObjectMap< , >.Entry entry )
            {
                addProcessedFile( entry );
            }

            }.process( inputDir.inputFile, null );
        }

        if ( files.isEmpty() ) return;

        if ( countOnly )
        {
            packCount++;

            return;
        }

        // Sort by name using numeric suffix, then alpha.
        Collections.sort( files, new Comparator< ObjectMap< , >.Entry >()
        {
            Pattern digitSuffix = Pattern.compile("(.*?)(\\d+)$");
            public int compare (Entry entry1, Entry entry2) {
            string full1 = entry1.inputFile.getName();
            int dotIndex = full1.lastIndexOf('.');
            if (dotIndex != -1) full1 = full1.substring(0, dotIndex);
            string full2 = entry2.inputFile.getName();
            dotIndex = full2.lastIndexOf('.');
            if (dotIndex != -1) full2 = full2.substring(0, dotIndex);
            string name1 = full1, name2 = full2;
            int num1 = 0, num2          = 0;
            Matcher matcher = digitSuffix.matcher(full1);
            if (matcher.matches()) {
            try {
            num1 = Integer.parseInt(matcher.group(2));
            name1 = matcher.group(1);
        } catch ( Exception ignored ) {
        }
        }
        matcher = digitSuffix.matcher( full2 );

        if ( matcher.matches() )
        {
            try
            {
                num2  = Integer.parseInt( matcher.group( 2 ) );
                name2 = matcher.group( 1 );
            }
            catch ( Exception ignored )
            {
            }
        }

        int compare = name1.compareTo( name2 );

        if ( compare != 0 || num1 == num2 ) return compare;

        return num1 - num2;

        }
        });

        // Pack.
        if ( !settings.silent )
        {
            try
            {
                System.out.println( "Reading: " + inputDir.inputFile.getCanonicalPath() );
            }
            catch ( IOException ignored )
            {
                System.out.println( "Reading: " + inputDir.inputFile.getAbsolutePath() );
            }
        }

        if ( progress != null )
        {
            progress.start( 1f / packCount );
            string inputPath = null;

            try
            {
                string rootPath = root.getCanonicalPath();
                inputPath = inputDir.inputFile.getCanonicalPath();

                if ( inputPath.startsWith( rootPath ) )
                {
                    rootPath  = rootPath.replace( '\\', '/' );
                    inputPath = inputPath.substring( rootPath.length() ).replace( '\\', '/' );
                    if ( inputPath.startsWith( "/" ) ) inputPath = inputPath.substring( 1 );
                }
            }
            catch ( IOException ignored )
            {
            }

            if ( inputPath == null || inputPath.length() == 0 ) inputPath = inputDir.inputFile.getName();
            progress.setMessage( inputPath );
        }

        TexturePacker packer = newTexturePacker( root, settings );
        for ( ObjectMap< , >.Entry file :
        files)
        packer.addImage( file.inputFile );
        Pack( packer, inputDir );
        if ( progress != null ) progress.end();
    }

    protected void Pack( TexturePacker packer, ObjectMap< FileInfo, TexturePacker.Settings >.Entry inputDir )
    {
        packer.Pack( inputDir.OutputDir, packFileName );
    }

    protected TexturePacker NewTexturePacker( FileInfo root, TexturePacker.Settings settings )
    {
        Guard.ThrowIfNull( _progress );

        var packer = new TexturePacker( root, settings );

        packer.SetListener( _progress );

        return packer;
    }

    protected TexturePacker.Settings NewSettings( TexturePacker.Settings settings )
    {
        return new TexturePacker.Settings( settings );
    }

    public TexturePacker.ProgressListener? GetProgressListener()
    {
        return _progress;
    }
}