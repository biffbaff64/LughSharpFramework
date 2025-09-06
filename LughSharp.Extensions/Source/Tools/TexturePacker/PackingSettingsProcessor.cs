// /////////////////////////////////////////////////////////////////////////////
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

namespace Extensions.Source.Tools.TexturePacker;

[PublicAPI]
public class PackingSettingsProcessor : FileProcessor
{
    public List< FileInfo > SettingsFiles { get; private set; } = [ ];

    /// <inheritdoc/>
    public override void ProcessFile( Entry entry )
    {
        if ( entry.InputFile is FileInfo file )
        {
            SettingsFiles.Add( file );
        }
    }
}

// ============================================================================
// ============================================================================

[PublicAPI]
public class DeleteProcessor : FileProcessor
{
    /// <inheritdoc />
    public override void ProcessFile( Entry inputFile )
    {
        inputFile.InputFile?.Delete();
    }
}

// ============================================================================
// ============================================================================

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class SettingsCombiningProcessor : FileProcessor
{
    public Entry? EntryDir { get; set; }

    // ========================================================================

    private readonly Entry         _entryDir;
    private readonly TexturePackerFileProcessor _fileProcessor;

    // ========================================================================

    public SettingsCombiningProcessor( Entry entryDir, TexturePackerFileProcessor fileProcessor )
    {
        _entryDir      = entryDir;
        _fileProcessor = fileProcessor;
    }

    /// <inheritdoc />
    public override void ProcessDir( Entry inputDir, List< Entry > files )
    {
        for ( var file = ( DirectoryInfo? )_entryDir.InputFile;
             ( file != null ) && !file.Equals( inputDir.InputFile );
             file = file.Parent )
        {
            if ( new FileInfo( Path.Combine( file.FullName, DEFAULT_PACKFILE_NAME ) ).Exists )
            {
                files.Clear();

                return;
            }
        }

        if ( !_fileProcessor.CountOnly )
        {
            _fileProcessor.DirsToIgnore.Add( ( DirectoryInfo )_entryDir.InputFile! );
        }
    }
}

// ============================================================================
// ============================================================================

