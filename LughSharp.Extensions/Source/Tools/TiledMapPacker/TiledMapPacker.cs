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

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Xml;
using System.Xml.Linq;
using Extensions.Source.Tools.TexturePacker;
using JetBrains.Annotations;
using LughSharp.Core.Assets.Loaders.Resolvers;
using LughSharp.Core.Files;
using LughSharp.Core.Graphics.Atlases;
using LughSharp.Core.Maps;
using LughSharp.Core.Maps.Tiled;
using LughSharp.Core.Maps.Tiled.Loaders;
using LughSharp.Core.Maps.Tiled.Tiles;
using LughSharp.Core.Utils.Collections;
using LughSharp.Core.Utils.Exceptions;
using LughSharp.Core.Utils.Logging;

namespace Extensions.Source.Tools.TiledMapPacker;

/// <summary>
/// Given one or more TMX tilemaps, packs all tileset resources used across the
/// maps, or the resources used per map, into a single, or multiple (one per map),
/// <see cref="TextureAtlas"/>s and produces a new TMX file to be loaded with a
/// dedicated TiledMap map loader. Optionally, it can keep track of unused tiles
/// and omit them from the generated atlas, reducing the resource size.
/// <para>
/// The original TMX map file will be parsed by using the <see cref="TmxMapLoader"/>
/// loader, thus access to a valid OpenGL context is <b>required</b>, that's why
/// an DesktopGLApplication is created by this preprocessor.
/// </para>
/// <para>
/// The new TMX map file will contains a new property, namely "atlas", whose value
/// will enable the AtlasTiledMapLoader to correctly read the associated TextureAtlas
/// representing the tileset.
/// </para>
/// </summary>
[PublicAPI]
public class TiledMapPacker
{
    private static readonly string _tilesetsOutputDir = "tileset";
    private static readonly string _atlasOutputName   = "packed";

    private TmxMapLoader                         _mapLoader      = new( new AbsoluteFileHandleResolver() );
    private Dictionary< string, List< int > >    _tilesetUsedIds = [ ];
    private ObjectMap< string, TiledMapTileSet > _tilesetsToPack = [ ];
    private TiledMap                             _map;
    private TiledMapPackerSettings               _settings;

    public required DirectoryInfo? InputDir;
    public required DirectoryInfo? OutputDir;
    public required DirectoryInfo? CurrentDir;

    // ========================================================================

    /// <summary>
    /// Creates a new TiledMapPacker instance. This is the default constructor
    /// and uses default TiledMap packer settings.
    /// </summary>
    public TiledMapPacker() : this( new TiledMapPackerSettings() )
    {
    }

    /// <summary>
    /// Creates a new TiledMapPacker instance using the supplied TiledMap
    /// packer settings.
    /// </summary>
    public TiledMapPacker( TiledMapPackerSettings settings )
    {
        this._settings = settings;
    }

    /// <summary>
    /// Process a directory containing TMX map files representing Tiled maps and produce
    /// multiple, or a single, TextureAtlas as well as newly processed TMX map files,
    /// correctly referencing the generated <see cref="TextureAtlas"/> by using the
    /// "atlas" custom map property.
    /// </summary>
    public void ProcessInputDir( TexturePackerSettings texturePackerSettings )
    {
        Guard.Against.Null( InputDir );

        var inputDirHandle       = new DirectoryInfo( InputDir.FullName );
        var mapFilesInCurrentDir = InputDir.GetFiles( "*.tmx" );

        _tilesetsToPack = new ObjectMap< string, TiledMapTileSet >();

        // Processes the maps inside inputDir
        foreach ( var mapFile in mapFilesInCurrentDir )
        {
            ProcessSingleMap( mapFile, inputDirHandle, texturePackerSettings );
        }

        ProcessSubdirectories( inputDirHandle, texturePackerSettings );

        var combineTilesets = _settings.CombineTilesets;

        if ( combineTilesets )
        {
            PackTilesets( inputDirHandle, texturePackerSettings );
        }
    }

    /// <summary>
    /// Looks for subdirectories inside parentHandle, processes maps in subdirectory, repeat.
    /// </summary>
    /// <param name="startPath"> The directory to look for maps and other directories </param>
    /// <param name="texturePackerSettings"></param>
    public void ProcessSubdirectories( DirectoryInfo startPath, TexturePackerSettings texturePackerSettings )
    {
        var stack = new Stack< DirectoryInfo >();

        // Initialize with the starting directory
        var rootDir = new DirectoryInfo( startPath.FullName );

        if ( rootDir.Exists )
        {
            stack.Push( rootDir );
        }

        while ( stack.Count > 0 )
        {
            var currentDir = stack.Pop();

            // 1. Process .tmx files in the current directory
            var mapFiles = currentDir.GetFiles( "*.tmx" );

            foreach ( var mapFile in mapFiles )
            {
                ProcessSingleMap( mapFile, currentDir, texturePackerSettings );
            }

            // 2. Add all subdirectories to the stack to be processed in next iterations
            try
            {
                foreach ( var subDir in currentDir.GetDirectories() )
                {
                    stack.Push( subDir );
                }
            }
            catch ( UnauthorizedAccessException )
            {
                // Handle or log directories we don't have permission to access
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapFile"></param>
    /// <param name="dirHandle"></param>
    /// <param name="texturePackerSettings"></param>
    private void ProcessSingleMap( FileInfo mapFile,
                                   DirectoryInfo dirHandle,
                                   TexturePackerSettings texturePackerSettings )
    {
        var combineTilesets = this._settings.CombineTilesets;

        if ( !combineTilesets )
        {
            _tilesetUsedIds = new Dictionary< string, List< int > >();
            _tilesetsToPack = new ObjectMap< string, TiledMapTileSet >();
        }

        _map = _mapLoader.Load( mapFile.FullName );

        // if enabled, build a list of used tileids for the tileset used by this map
        var stripUnusedTiles = this._settings.StripUnusedTiles;

        if ( stripUnusedTiles )
        {
            StripUnusedTiles();
        }
        else
        {
            foreach ( var tileset in _map.Tilesets )
            {
                var tilesetName = tileset.Name;

                if ( !_tilesetsToPack.ContainsKey( tilesetName ) )
                {
                    _tilesetsToPack[ tilesetName ] = tileset;
                }
            }
        }

        if ( !combineTilesets )
        {
            var tmpHandle = new FileInfo( mapFile.Name );

            _settings.AtlasOutputName = Path.GetFileNameWithoutExtension( tmpHandle.FullName );

            PackTilesets( dirHandle, texturePackerSettings );
        }

        WriteUpdatedTmx( _map, new FileInfo( mapFile.FullName ) );
    }

    /// <summary>
    /// 
    /// </summary>
    private void StripUnusedTiles()
    {
        Guard.Against.Null( _map );

        var mapWidth   = _map.Properties.Get< int >( "width" );
        var mapHeight  = _map.Properties.Get< int >( "height" );
        var numlayers  = _map.Layers.LayersCount;
        var bucketSize = mapWidth * mapHeight * numlayers;

        foreach ( var layer in _map!.Layers )
        {
            // some layers can be plain MapLayer instances (ie. object groups),
            // just ignore them
            if ( layer is TiledMapTileLayer tlayer )
            {
                for ( var y = 0; y < mapHeight; ++y )
                {
                    for ( var x = 0; x < mapWidth; ++x )
                    {
                        if ( tlayer.GetCell( x, y ) != null )
                        {
                            var tile = tlayer.GetCell( x, y )?.GetTile();

                            if ( tile != null )
                            {
                                if ( tile is AnimatedTiledMapTile aTile )
                                {
                                    foreach ( var t in aTile.GetFrameTiles() )
                                    {
                                        AddTile( t, bucketSize );
                                    }
                                }

                                // Adds non-animated tiles and the base animated tile
                                AddTile( tile, bucketSize );
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tile"></param>
    /// <param name="bucketSize"></param>
    private void AddTile( ITiledMapTile tile, int bucketSize )
    {
        var tileid      = ( int )( tile.ID & ~0xE0000000 );
        var tilesetName = TilesetNameFromTileID( _map, tileid );
        var usedIds     = GetUsedIdsBucket( tilesetName, bucketSize );

        if ( usedIds == null )
        {
            Logger.Debug( "NULL UsedIDsBucket returned!" );
            
            return;
        }
        
        usedIds.Add( tileid );

        // track this tileset to be packed if not already tracked
        if ( !_tilesetsToPack.ContainsKey( tilesetName ) )
        {
            _tilesetsToPack[ tilesetName ] = _map.Tilesets.GetTileSet( tilesetName );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="map"></param>
    /// <param name="tileid"></param>
    /// <returns></returns>
    private string TilesetNameFromTileID( TiledMap map, int tileid )
    {
        var name = "";

        if ( tileid != 0 )
        {
            foreach ( var tileset in map.Tilesets )
            {
                var firstgid = tileset.Properties.Get< int >( "firstgid", -1 );

                if ( firstgid == -1 )
                {
                    continue; // skip this tileset
                }

                if ( tileid >= firstgid )
                {
                    name = tileset.Name;
                }
                else
                {
                    return name;
                }
            }
        }

        return name;
    }

    /// <summary>
    /// Returns the usedIds bucket for the given tileset name. If it doesn't exist one
    /// will be created with the specified size if its > 0, else null will be returned.
    /// </summary>
    /// <param name="tilesetName"></param>
    /// <param name="size">
    /// The size to use to create a new bucket if it doesn't exist, else specify 0 or
    /// lower to return null instead
    /// </param>
    private List< int >? GetUsedIdsBucket( string tilesetName, int size )
    {
        if ( _tilesetUsedIds.TryGetValue( tilesetName, out var idsBucket ) )
        {
            return idsBucket;
        }

        if ( size <= 0 )
        {
            return null;
        }

        var bucket = new List< int >( size );

        _tilesetUsedIds[ tilesetName ] = bucket;

        return bucket;
    }

//	/** Traverse the specified tilesets, optionally lookup the used ids and pass every tile image to the {@link TexturePacker},
//	 * optionally ignoring unused tile ids */
    private void PackTilesets( DirectoryInfo inputDirHandle, TexturePackerSettings texturePackerSettings )
    {
//		BufferedImage tile;
//		Vector2 tileLocation;
//		Graphics g;
//
//		packer = new TexturePacker(texturePackerSettings);
//
//		for (TiledMapTileSet set : tilesetsToPack.values()) {
//			string tilesetName = set.getName();
//			System.out.println("Processing tileset " + tilesetName);
//
//			IntArray usedIds = this.settings.stripUnusedTiles ? getUsedIdsBucket(tilesetName, -1) : null;
//
//			int tileWidth = set.getProperties().get("tilewidth", Integer.class);
//			int tileHeight = set.getProperties().get("tileheight", Integer.class);
//			int firstgid = set.getProperties().get("firstgid", Integer.class);
//			string imageName = set.getProperties().get("imagesource", string.class);
//
//			TileSetLayout layout = new TileSetLayout(firstgid, set, inputDirHandle);
//
//			for (int gid = layout.firstgid, i = 0; i < layout.numTiles; gid++, i++) {
//				bool verbose = this.settings.verbose;
//
//				if (usedIds != null && !usedIds.contains(gid)) {
//					if (verbose) {
//						System.out.println("Stripped id #" + gid + " from tileset \"" + tilesetName + "\"");
//					}
//					continue;
//				}
//
//				tileLocation = layout.getLocation(gid);
//				tile = new BufferedImage(tileWidth, tileHeight, BufferedImage.TYPE_4BYTE_ABGR);
//
//				g = tile.createGraphics();
//				g.drawImage(layout.image, 0, 0, tileWidth, tileHeight, (int)tileLocation.x, (int)tileLocation.y,
//					(int)tileLocation.x + tileWidth, (int)tileLocation.y + tileHeight, null);
//
//				if (verbose) {
//					System.out.println(
//						"Adding " + tileWidth + "x" + tileHeight + " (" + (int)tileLocation.x + ", " + (int)tileLocation.y + ")");
//				}
//				// AtlasTmxMapLoader expects every tileset's index to begin at zero for the first tile in every tileset.
//				// so the region's adjusted gid is (gid - layout.firstgid). firstgid will be added back in AtlasTmxMapLoader on load
//				int adjustedGid = gid - layout.firstgid;
//				final string separator = "_";
//				string regionName = tilesetName + separator + adjustedGid;
//
//				packer.addImage(tile, regionName);
//			}
//		}
//		string tilesetOutputDir = outputDir.toString() + "/" + this.settings.tilesetOutputDirectory;
//		File relativeTilesetOutputDir = new File(tilesetOutputDir);
//		File outputDirTilesets = new File(relativeTilesetOutputDir.getCanonicalPath());
//
//		outputDirTilesets.mkdirs();
//		packer.pack(outputDirTilesets, this.settings.atlasOutputName + ".atlas");
    }

    private void WriteUpdatedTmx( TiledMap tiledMap, FileInfo tmxFileHandle )
    {
//		Document doc;
//		DocumentBuilder docBuilder;
//		DocumentBuilderFactory docFactory = DocumentBuilderFactory.newInstance();
//
//		try {
//			docBuilder = docFactory.newDocumentBuilder();
//			doc = docBuilder.parse(tmxFileHandle.read());
//
//			Node map = doc.getFirstChild();
//			while (map.getNodeType() != Node.ELEMENT_NODE || map.getNodeName() != "map") {
//				if ((map = map.getNextSibling()) == null) {
//					throw new GdxRuntimeException("Couldn't find map node!");
//				}
//			}
//
//			setProperty(doc, map, "atlas", settings.tilesetOutputDirectory + "/" + settings.atlasOutputName + ".atlas");
//
//			TransformerFactory transformerFactory = TransformerFactory.newInstance();
//			Transformer transformer = transformerFactory.newTransformer();
//			DOMSource source = new DOMSource(doc);
//
//			outputDir.mkdirs();
//			StreamResult result = new StreamResult(new File(outputDir, tmxFileHandle.name()));
//			transformer.transform(source, result);
//
//		} catch (ParserConfigurationException e) {
//			throw new RuntimeException("ParserConfigurationException: " + e.getMessage());
//		} catch (SAXException e) {
//			throw new RuntimeException("SAXException: " + e.getMessage());
//		} catch (TransformerConfigurationException e) {
//			throw new RuntimeException("TransformerConfigurationException: " + e.getMessage());
//		} catch (TransformerException e) {
//			throw new RuntimeException("TransformerException: " + e.getMessage());
//		}
    }

    private static void SetProperty( XDocument doc, XNode parent, string name, string value )
    {
//		Node properties = getFirstChildNodeByName(parent, "properties");
//		Node property = getFirstChildByNameAttrValue(properties, "property", "name", name);
//
//		NamedNodeMap attributes = property.getAttributes();
//		Node valueNode = attributes.getNamedItem("value");
//		if (valueNode == null) {
//			valueNode = doc.createAttribute("value");
//			valueNode.setNodeValue(value);
//			attributes.setNamedItem(valueNode);
//		} else {
//			valueNode.setNodeValue(value);
//		}
    }

//	/** If the child node doesn't exist, it is created. */
    private static XNode GetFirstChildNodeByName( XNode parent, string child )
    {
//		NodeList childNodes = parent.getChildNodes();
//		for (int i = 0; i < childNodes.getLength(); i++) {
//			if (childNodes.item(i).getNodeName().equals(child)) {
//				return childNodes.item(i);
//			}
//		}
//
//		Node newNode = parent.getOwnerDocument().createElement(child);
//
//		if (childNodes.item(0) != null)
//			return parent.insertBefore(newNode, childNodes.item(0));
//		else
//			return parent.appendChild(newNode);

        return null!;
    }

//	/** If the child node or attribute doesn't exist, it is created. Usage example: Node property =
//	 * getFirstChildByAttrValue(properties, "property", "name"); */
    private static XNode GetFirstChildByNameAttrValue( XNode node, string childName, string attr, string value )
    {
//		var childNodes = node.getChildNodes();
//		for (int i = 0; i < childNodes.getLength(); i++)
//      {
//			if (childNodes.item(i).getNodeName().equals(childName))
//          {
//				NamedNodeMap attributes = childNodes.item(i).getAttributes();
//				XNode attribute = attributes.getNamedItem(attr);
//				if (attribute.getNodeValue().equals(value))
//              {
//                  return childNodes.item(i);
//              }
//			}
//		}
//
//		XNode newNode = node.getOwnerDocument().createElement(childName);
//		NamedNodeMap attributes = newNode.getAttributes();
//
//		Attr nodeAttr = node.getOwnerDocument().createAttribute(attr);
//		nodeAttr.setNodeValue(value);
//		attributes.setNamedItem(nodeAttr);
//
//		if (childNodes.item(0) != null)
//      {
//			return node.insertBefore(newNode, childNodes.item(0));
//		}
//      else
//      {
//			return node.appendChild(newNode);
//		}

        return null!;
    }

//	/** Processes a directory of Tile Maps, compressing each tile set contained in any map once.
//	 * 
//	 * @param args args[0]: the input directory containing the tmx files (and tile sets, relative to the path listed in the tmx
//	 *           file). args[1]: The output directory for the tmx files, should be empty before running. args[2-4] options */
//	public static void main (string[] args) {
//		final Settings texturePackerSettings = new Settings();
//		texturePackerSettings.paddingX = 2;
//		texturePackerSettings.paddingY = 2;
//		texturePackerSettings.edgePadding = true;
//		texturePackerSettings.duplicatePadding = true;
//		texturePackerSettings.bleed = true;
//		texturePackerSettings.alias = true;
//		texturePackerSettings.useIndexes = true;
//
//		final TiledMapPackerSettings packerSettings = new TiledMapPackerSettings();
//
//		if (args.length == 0) {
//			printUsage();
//			System.exit(0);
//		} else if (args.length == 1) {
//			inputDir = new File(args[0]);
//			outputDir = new File(inputDir, "../output/");
//		} else if (args.length == 2) {
//			inputDir = new File(args[0]);
//			outputDir = new File(args[1]);
//		} else {
//			inputDir = new File(args[0]);
//			outputDir = new File(args[1]);
//			processExtraArgs(args, packerSettings);
//		}
//
//		TiledMapPacker packer = new TiledMapPacker(packerSettings);
//		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
//		config.forceExit = false;
//		config.width = 100;
//		config.height = 50;
//		config.title = "TiledMapPacker";
//		new LwjglApplication(new ApplicationListener() {
//
//			@Override
//			public void resume () {
//			}
//
//			@Override
//			public void resize (int width, int height) {
//			}
//
//			@Override
//			public void render () {
//			}
//
//			@Override
//			public void pause () {
//			}
//
//			@Override
//			public void dispose () {
//			}
//
//			@Override
//			public void create () {
//				TiledMapPacker packer = new TiledMapPacker(packerSettings);
//
//				if (!inputDir.exists()) {
//					System.out.println(inputDir.getAbsolutePath());
//					throw new RuntimeException("Input directory does not exist: " + inputDir);
//				}
//
//				try {
//					packer.processInputDir(texturePackerSettings);
//				} catch (IOException e) {
//					throw new RuntimeException("Error processing map: " + e.getMessage());
//				}
//				System.out.println("Finished processing.");
//				Gdx.app.exit();
//			}
//		}, config);
//	}

//	private static void processExtraArgs (string[] args, TiledMapPackerSettings packerSettings) {
//		string stripUnused = "--strip-unused";
//		string combineTilesets = "--combine-tilesets";
//		string verbose = "-v";
//
//		int length = args.length - 2;
//		string[] argsNotDir = new string[length];
//		System.arraycopy(args, 2, argsNotDir, 0, length);
//
//		for (string string : argsNotDir) {
//			if (stripUnused.equals(string)) {
//				packerSettings.stripUnusedTiles = true;
//
//			} else if (combineTilesets.equals(string)) {
//				packerSettings.combineTilesets = true;
//
//			} else if (verbose.equals(string)) {
//				packerSettings.verbose = true;
//
//			} else {
//				System.out.println("\nOption \"" + string + "\" not recognized.\n");
//				printUsage();
//				System.exit(0);
//			}
//		}
//	}

//	private static void printUsage () {
//		System.out.println("Usage: INPUTDIR [OUTPUTDIR] [--strip-unused] [--combine-tilesets] [-v]");
//		System.out.println("Processes a directory of Tiled .tmx maps. Unable to process maps with XML");
//		System.out.println("tile layer format.");
//		System.out.println("  --strip-unused             omits all tiles that are not used. Speeds up");
//		System.out.println("                             the processing. Smaller tilesets.");
//		System.out.println("  --combine-tilesets         instead of creating a tileset for each map,");
//		System.out.println("                             this combines the tilesets into some kind");
//		System.out.println("                             of monster tileset. Has problems with tileset");
//		System.out.println("                             location. Has problems with nested folders.");
//		System.out.println("                             Not recommended.");
//		System.out.println("  -v                         outputs which tiles are stripped and included");
//		System.out.println();
//	}

    // ========================================================================

    [PublicAPI]
    public class TiledMapPackerSettings
    {
        public bool   StripUnusedTiles       { get; set; }
        public bool   CombineTilesets        { get; set; }
        public bool   Verbose                { get; set; }
        public string TilesetOutputDirectory { get; set; } = _tilesetsOutputDir;
        public string AtlasOutputName        { get; set; } = _atlasOutputName;
    }

    // ========================================================================

//	/** Processes a directory of Tile Maps, compressing each tile set contained in any map once.
//	 * 
//	 * @param args args[0]: the input directory containing the tmx files (and tile sets, relative to the path listed in the tmx
//	 *           file). args[1]: The output directory for the tmx files, should be empty before running. args[2-4] options */
//	public static void main (String[] args) {
//		final Settings texturePackerSettings = new Settings();
//		texturePackerSettings.paddingX = 2;
//		texturePackerSettings.paddingY = 2;
//		texturePackerSettings.edgePadding = true;
//		texturePackerSettings.duplicatePadding = true;
//		texturePackerSettings.bleed = true;
//		texturePackerSettings.alias = true;
//		texturePackerSettings.useIndexes = true;
//
//		final TiledMapPackerSettings packerSettings = new TiledMapPackerSettings();
//
//		if (args.length == 0) {
//			printUsage();
//			System.exit(0);
//		} else if (args.length == 1) {
//			inputDir = new File(args[0]);
//			outputDir = new File(inputDir, "../output/");
//		} else if (args.length == 2) {
//			inputDir = new File(args[0]);
//			outputDir = new File(args[1]);
//		} else {
//			inputDir = new File(args[0]);
//			outputDir = new File(args[1]);
//			processExtraArgs(args, packerSettings);
//		}
//
//		TiledMapPacker packer = new TiledMapPacker(packerSettings);
//		LwjglApplicationConfiguration config = new LwjglApplicationConfiguration();
//		config.forceExit = false;
//		config.width = 100;
//		config.height = 50;
//		config.title = "TiledMapPacker";
//		new LwjglApplication(new ApplicationListener() {
//
//			@Override
//			public void resume () {
//			}
//
//			@Override
//			public void resize (int width, int height) {
//			}
//
//			@Override
//			public void render () {
//			}
//
//			@Override
//			public void pause () {
//			}
//
//			@Override
//			public void dispose () {
//			}
//
//			@Override
//			public void create () {
//				TiledMapPacker packer = new TiledMapPacker(packerSettings);
//
//				if (!inputDir.exists()) {
//					System.out.println(inputDir.getAbsolutePath());
//					throw new RuntimeException("Input directory does not exist: " + inputDir);
//				}
//
//				try {
//					packer.processInputDir(texturePackerSettings);
//				} catch (IOException e) {
//					throw new RuntimeException("Error processing map: " + e.getMessage());
//				}
//				System.out.println("Finished processing.");
//				Gdx.app.exit();
//			}
//		}, config);
//	}
//
//	private static void processExtraArgs (String[] args, TiledMapPackerSettings packerSettings) {
//		String stripUnused = "--strip-unused";
//		String combineTilesets = "--combine-tilesets";
//		String verbose = "-v";
//
//		int length = args.length - 2;
//		String[] argsNotDir = new String[length];
//		System.arraycopy(args, 2, argsNotDir, 0, length);
//
//		for (String string : argsNotDir) {
//			if (stripUnused.equals(string)) {
//				packerSettings.stripUnusedTiles = true;
//
//			} else if (combineTilesets.equals(string)) {
//				packerSettings.combineTilesets = true;
//
//			} else if (verbose.equals(string)) {
//				packerSettings.verbose = true;
//
//			} else {
//				System.out.println("\nOption \"" + string + "\" not recognized.\n");
//				printUsage();
//				System.exit(0);
//			}
//		}
//	}
}

// ============================================================================
// ============================================================================