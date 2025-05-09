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

using System.Runtime.Versioning;

using LughSharp.Lugh.Utils;

namespace LughSharp.Lugh.Graphics.Packing;

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class PackingSettingsProcessor : FileProcessor
{
    public List< FileInfo > SettingsFiles { get; private set; } = [ ];

    public PackingSettingsProcessor()
    {
    }

    /// <inheritdoc/>
    public override void ProcessFile( TexturePackerEntry entry )
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
[SupportedOSPlatform( "windows" )]
public class DeleteProcessor : FileProcessor
{
    public DeleteProcessor()
    {
    }

    /// <inheritdoc />
    public override void ProcessFile( TexturePackerEntry inputFile )
    {
        inputFile.InputFile?.Delete();
    }
}

// ============================================================================
// ============================================================================

[PublicAPI]
[SupportedOSPlatform( "windows" )]
public class SettingsCombiningProcessor : TexturePackerFileProcessor
{
    public TexturePackerEntry? EntryDir { get; set; }

    private readonly TexturePackerEntry _entryDir;

    public SettingsCombiningProcessor( TexturePackerEntry entryDir )
    {
        _entryDir = entryDir;
    }

    /// <inheritdoc />
    public override void ProcessDir( TexturePackerEntry inputDir, List< TexturePackerEntry > files )
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

        if ( !this.CountOnly )
        {
            this.DirsToIgnore.Add( ( DirectoryInfo )_entryDir.InputFile! );
        }
    }
}

