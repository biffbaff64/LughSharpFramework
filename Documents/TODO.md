LUGHSHARP 2D GAME FRAMEWORK - ROUND 1
-------------------------------------

ALL CLASSES WILL BE UP FOR MODIFICATION FOLLOWING TESTING.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- STEP 1: Complete all conversions so that the code will build.
- STEP 2: Refactor, where possible and necessary, to take advantage of C# language features.
    - Some classes can become structs / records instead.
    - Some/Most Get / Set method declarations in interfaces could become Properties.
    - switch expressions instead of switch statements where appropriate.
    - switch expressions instead of if...if/else...else where appropriate.
    - Check methods to see if they can be virtual.
    - Check and/or correct visibility of classes/methods/properties etc.
    - Use sealed classes only where strictly necessary.
    - Use of virtual for base classes/methods and classes that are likely to be extended is essential.
    - Constantly look for opportunities to improve this code.
- STEP 3: Resolve ALL remaining TODOs.
- STEP 4: Ensure code is fully documented.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- Methods like **Dispose(), ToString(), Equals(), GetHashCode() ( Essentially overridden system methods )** 
- should be positioned at the END of source files.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- Rename class Graphics.OpenGL.OpenGL

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- NO MAGIC NUMBERS!!!
- SORT OUT VERSIONING!!!
- PRIORITY is 2D classes first

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- There seems to be different namings for width/height etc. properties and methods. Make it more uniform

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- IP = Conversion In Progress.
- DONE = Class finished but may not be fully 'CSHARP-ified'
- First column is for Code, Second column is for Documentation.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      -


ASSETS
------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - AssetDescriptor
    - DONE - DONE - DONE - AssetHelper
    - DONE - IP   - DONE - AssetLoaderParameters
    - DONE - IP   - DONE - AssetLoadingTask
    - DONE - IP   - DONE - AssetManager
    - DONE - DONE - DONE - IAssetErrorListener
    - DONE - IP   - DONE - IAssetTask
    - DONE - IP   - DONE - RefCountedContainer

ASSETS/LOADERS
--------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - AssetLoader
    - DONE - DONE - DONE - AsynchronousAssetLoader
    - DONE - DONE - DONE - BitmapFontLoader
    - DONE - DONE - DONE - CubemapLoader
    - DONE - DONE - DONE - ModelLoader
    - DONE - DONE - DONE - MusicLoader
    - DONE - DONE - DONE - ParticleEffectLoader
    -      -      -      - PixmapLoader
    -      -      -      - ShaderProgramLoader
    -      -      -      - SkinLoader
    -      -      -      - SoundLoader
    -      -      -      - SynchronousAssetLoader
    -      -      -      - TextureAtlasLoader
    -      -      -      - TextureLoader

ASSETS/LOADERS/RESOLVERS
------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AbsoluteFileHandleResolver
    -      -      -      - ClasspathFileHandleResolver
    -      -      -      - ExternalFileHandleResolver
    -      -      -      - IFileHandleResolver
    -      -      -      - InternalFileHandleResolver
    -      -      -      - LocalFileHandleResolver
    - DONE - DONE - DONE - PrefixFileHandleResolver
    - DONE - DONE - DONE - ResolutionFileResolver

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- **I'm currently considering ditching Lugh.Audio in favour of NAudio.**
- **Decision to be made asap.**

AUDIO
-----

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - IAudio
    -      -      -      - IAudioDevice
    -      -      -      - IAudioDeviceAsync
    -      -      -      - IAudioRecorder
    -      -      -      - IMusic
    -      -      -      - ISound

AUDIO/MAPONUS ( MAPONUS is the God of Music )
---------------------------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Buffer16BitSterso
    -      -      -      - MP3SharpException
    -      -      -      - MP3Stream
    -      -      -      - SoundFormat

AUDIO/MAPONUS/DECODING
-----------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AudioBase
    -      -      -      - BitReserve
    -      -      -      - Bitstream
    -      -      -      - BitstreamErrors
    -      -      -      - BitstreamException
    -      -      -      - CircularByteBuffer
    -      -      -      - Crc16
    -      -      -      - Decoder
    -      -      -      - DecoderParameters
    -      -      -      - DecoderErrors
    -      -      -      - DecoderException
    -      -      -      - Equalizer
    -      -      -      - Header
    -      -      -      - Huffman
    -      -      -      - OutputChannels
    -      -      -      - PushbackStream
    -      -      -      - SampleBuffer
    -      -      -      - SynthesisFilter

AUDIO/MAPONUS/DECODING/DECODERS
-----------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ASubband
    -      -      -      - IFrameDecoder
    -      -      -      - LayerIDecoder
    -      -      -      - LayerIIDecoder
    -      -      -      - LayerIIIDecoder

AUDIO/MAPONUS/DECODING/DECODERS/LAYERI
---------------------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - SubbandLayer1
    -      -      -      - SubbandLayer1IntensityStereo
    -      -      -      - SubbandLayer1Stereo

AUDIO/MAPONUS/DECODING/DECODERS/LAYERII
----------------------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - SubbandLayer2
    -      -      -      - SubbandLayer2IntensityStereo
    -      -      -      - SubbandLayer2Stereo

AUDIO/MAPONUS/DECODING/DECODERS/LAYERIII
-----------------------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ChannelData
    -      -      -      - GranuleInfo
    -      -      -      - Layer3SideInfo
    -      -      -      - SBI
    -      -      -      - ScaleFactorData
    -      -      -      - ScaleFactorTable

AUDIO/MAPONUS/IO
-----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - RandomAccessFileStream
    -      -      -      - RiffFile
    -      -      -      - WaveFile
    -      -      -      - WaveFileBuffer

AUDIO/MAPONUS/SUPPORT
----------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - SupportClass

AUDIO/OPENAL
------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AL
    -      -      -      - ALC

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

FILES
-----

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - Files
    - DONE - DONE - DONE - FileService
    - DONE - DONE - DONE - IFilenameFilter
    - DONE - DONE - DONE - IFiles
    - DONE - DONE - DONE - IFileService
    - DONE - DONE - DONE - IOUtils
    - DONE - DONE - DONE - PathType

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

GRAPHICS
--------

    CODE   DOCU   FOOTER
    ----   ----   ------
    - DONE - DONE - DONE - CIM
    -      -      -      - Color
    -      -      -      - Colors
    -      -      -      - Cubemap
    -      -      -      - GLTexture
    -      -      -      - GLTextureArray
    -      -      -      - GraphicsBackend
    -      -      -      - GraphicsCapabilities
    -      -      -      - GraphicsDevice
    - DONE - IP   - DONE - GStructs
    - DONE - DONE - DONE - ICubemapData
    - DONE - DONE - DONE - ICursor
    - DONE - DONE - DONE - IGraphicsDevice
    - DONE - DONE - DONE - ITextureArrayData
    - DONE - IP   - DONE - ITextureData
    - DONE - IP   - DONE - LughFormat               ( Needs a better name )
    -      -      -      - Mesh
    - DONE - IP   - DONE - PixelFormat
    - DONE - IP   - DONE - Pixmap                   y=down, x=right
    -      -      -      - PixmapData
    -      -      -      - PixmapDownloader
    -      -      -      - PixmapIO
    - DONE - IP   - DONE - PNG
    -      -      -      - Texture                  y=down, x=right
    -      -      -      - TextureDataFactory
    -      -      -      - TextureRegion            y=down, x=right
    -      -      -      - VertexAttribute
    -      -      -      - VertexAttributes
    - DONE - IP   - DONE - VertexDataType

GRAPHICS/ATLASES
----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AtlasRegion
    -      -      -      - AtlasSprite
    -      -      -      - TextureAtlas
    -      -      -      - TextureAtlasData

GRAPHICS/CAMERAS
----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Camera
    -      -      -      - CameraData
    -      -      -      - IGameCamera
    -      -      -      - OrthographicCamera
    -      -      -      - OrthographicGameCamera
    -      -      -      - PerspectiveCamera
    -      -      -      - Shake

GRAPHICS/FRAMEBUFFERS
---------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - FloatFrameBuffer
    -      -      -      - FloatFrameBufferBuilder
    -      -      -      - FrameBuffer
    -      -      -      - FrameBufferBuilder
    -      -      -      - FrameBufferConfig
    -      -      -      - FrameBufferCubemap
    -      -      -      - FrameBufferCubemapBuilder
    -      -      -      - FrameBufferRenderBufferAttachmentSpec
    -      -      -      - FrameBufferTextureAttachmentSpec
    -      -      -      - GLFrameBuffer
    -      -      -      - GLFrameBufferBuilder

GRAPHICS/G2D
------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Animation
    -      -      -      - CpuSpriteBatch                  Some methods have too many parameters
    -      -      -      - Gdx2DPixmap
    -      -      -      - IBatch
    -      -      -      - IPolygonBatch                   Some methods have too many parameters
    -      -      -      - NinePatch
    -      -      -      - ParticleEffect
    -      -      -      - ParticleEffectPool
    -      -      -      - ParticleEmitter
    -      -      -      - PixmapPacker
    -      -      -      - PixmapPackerIO
    -      -      -      - PolygonRegion
    -      -      -      - PolygonRegionLoader
    -      -      -      - PolygonSprite
    -      -      -      - PolygonSpriteBatch              Some methods have too many parameters
    -      -      -      - RepeatablePolygonSprite
    -      -      -      - Sprite
    -      -      -      - SpriteBatch
    -      -      -      - SpriteCache

GRAPHICS/G3D
------------

    See Documents/TODO_G3D.MD

GRAPHICS/IMAGEDECODERS
----------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - BMPFormatStructs
    -      -      -      - BMPUtils
    -      -      -      - PNGDecoder
    -      -      -      - PNGFormatStructs

    ------------------------------------
    ( Possibly future additions )
    -      -      -      - IImageDecoder
    -      -      -      - ImageDecoder
    -      -      -      - ImageFormat
    -      -      -      - ImageIO
    -      -      -      - ImageUtils

GRAPHICS/OPENGL
---------------

    Change uint parameters in Bindings methods to int, and do conversion to uint
    inside those methods when necessary. This will remove the amount of casting to
    uint in the code.

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - DebugSeverity
    -      -      -      - GLData
    -      -      -      - GLDebugControl
    -      -      -      - GLFormatChooser
    -      -      -      - GLImage
    -      -      -      - GLUtils
    -      -      -      - IGL
    -      -      -      - IGL.GL20
    -      -      -      - IGL.GL30
    -      -      -      - IGL.GL31
    -      -      -      - IGL.GL32
    -      -      -      - OpenGLActions

GRAPHICS/OPENGL/BINDINGS
------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - BufferObjectBindings
    -      -      -      - DebugBindings
    -      -      -      - DrawBindings
    -      -      -      - ErrorBindings
    -      -      -      - FrameBufferBindings
    -      -      -      - GLBindings
    -      -      -      - GLBindingsHelpers
    -      -      -      - GLBindingsStructs
    -      -      -      - GLFunctionDelegates
    -      -      -      - GLFunctionsLoader
    -      -      -      - IGLBindings
    -      -      -      - IGLBindings.GL20
    -      -      -      - IGLBindings.GL30
    -      -      -      - IGLBindings.GL31
    -      -      -      - IGLBindings.GL32
    -      -      -      - IGLBindingsExtra
    -      -      -      - ProgramBindings
    -      -      -      - QueryBindings
    -      -      -      - RenderBufferBindings
    -      -      -      - ShaderBindings
    -      -      -      - StateBindings
    -      -      -      - StencilBindings
    -      -      -      - TextureBindings
    -      -      -      - TextureSamplerBindings
    -      -      -      - TransformBindings
    -      -      -      - TransformFeedbackBindings
    -      -      -      - UniformBindings
    -      -      -      - VertexArrayBindings
    -      -      -      - VertexAttribBindings

GRAPHICS/OPENGL/ENUMS
---------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - BlendMode
    -      -      -      - BufferEnums
    -      -      -      - ClearBufferMask
    -      -      -      - ClientApi
    -      -      -      - CompareFunction
    -      -      -      - ContextApi
    -      -      -      - CullFaceMode
    -      -      -      - DataType
    -      -      -      - DebugEnums
    -      -      -      - DrawElementsType
    -      -      -      - EnableCap
    -      -      -      - FrameBufferEnums
    -      -      -      - GLParameter
    -      -      -      - JoystickHats
    -      -      -      - LogicOp
    -      -      -      - MatrixMode
    -      -      -      - PixelEnums
    -      -      -      - PolygonMode
    -      -      -      - PrimitiveType
    -      -      -      - ProgramParameter
    -      -      -      - QueryTarget
    -      -      -      - ShaderEnums
    -      -      -      - StencilEnums
    -      -      -      - StringName
    -      -      -      - TextureFilterMode
    -      -      -      - TextureFormat
    -      -      -      - TextureLimit
    -      -      -      - TextureParameter
    -      -      -      - TextureTarget
    -      -      -      - TextureUnit
    -      -      -      - TextureUsage
    -      -      -      - TextureWrapMode
    -      -      -      - VertexAttribParameter
    -      -      -      - VertexAttribType

GRAPHICS/SHADERS
----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ShaderLoader
    -      -      -      - ShaderProgram
    -      -      -      - Shaders              ( Holds Shader strings, rename? )

GRAPHICS/TEXT
-------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - BitmapFont
    -      -      -      - BitmapFontCache
    -      -      -      - BitmapFontData
    -      -      -      - CharacterUtils
    -      -      -      - DistanceFieldFont
    -      -      -      - Glyph
    -      -      -      - GlyphLayout
    -      -      -      - RegexUtils
    -      -      -      - Subset
    -      -      -      - UnicodeBlock

GRAPHICS/UTILS
--------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AppVersion
    -      -      -      - ETC1
    -      -      -      - ETC1TextureData
    -      -      -      - FacedCubemapData
    -      -      -      - FileTextureArrayData
    -      -      -      - FileTextureData
    -      -      -      - FloatTextureData
    -      -      -      - GLOnlyTextureData
    -      -      -      - HdpiUtils
    -      -      -      - IImmediateModeRenderer
    -      -      -      - IIndexData
    -      -      -      - IInstanceData
    -      -      -      - ImageUtils
    -      -      -      - ImmediateModeRenderer20
    -      -      -      - IndexArray
    -      -      -      - IndexBufferObject
    -      -      -      - IndexBufferObjectSubData
    -      -      -      - InstanceBufferObject
    -      -      -      - InstanceBufferObjectSubData
    -      -      -      - IVertexData
    -      -      -      - KTXTTextureData
    -      -      -      - MipMapGenerator
    -      -      -      - MipMapTextureData
    -      -      -      - PixmapTextureData
    -      -      -      - ShapeRenderer
    -      -      -      - TextureUtils
    -      -      -      - Vertex
    -      -      -      - VertexArray
    -      -      -      - VertexBufferObject
    -      -      -      - VertexBufferObjectSubData
    -      -      -      - VertexBufferObjectWithVAO
    -      -      -      - VertexConstants

GRAPHICS/VIEWPORTS
------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ExtendedViewport
    -      -      -      - FillViewport
    -      -      -      - FitViewport
    -      -      -      - ScalingViewport
    -      -      -      - ScreenViewport
    -      -      -      - StretchViewport
    -      -      -      - Viewport

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

INPUT
-----

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AbstractInput
    -      -      -      - GestureDetector
    -      -      -      - IInput
    -      -      -      - IInputProcessor
    -      -      -      - InputAdapter
    -      -      -      - InputEventQueue
    -      -      -      - InputMultiplexer
    -      -      -      - InputUtils
    -      -      -      - RemoteInput
    -      -      -      - RemoteSender

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

MAIN
----

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ApplicationAdapter
    -      -      -      - ApplicationConfiguration
    -      -      -      - Engine
    -      -      -      - Game
    -      -      -      - GameTime
    -      -      -      - IApplication
    -      -      -      - IApplicationListener
    -      -      -      - ILifecycleListener
    -      -      -      - IScreen
    -      -      -      - LibraryVersion
    -      -      -      - Platform
    -      -      -      - ScreenAdapter

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

MAPS
----

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - IImageResolver
    -      -      -      - IMapRenderer
    -      -      -      - Map
    -      -      -      - MapData
    -      -      -      - MapGroupLayer
    -      -      -      - MapLayer
    -      -      -      - MapLayers
    -      -      -      - MapObject
    -      -      -      - MapObjects
    -      -      -      - MapProperties

MAPS/OBJECTS
------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - CircleMapObject
    -      -      -      - EllipseMapObject
    -      -      -      - PolygonMapObject
    -      -      -      - PolylineMapObject
    -      -      -      - RectangleMapObject
    -      -      -      - TextureMapObject

MAPS/TILED
----------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ITiledMapTile
    -      -      -      - TiledMap
    -      -      -      - TiledMapImageLayer
    -      -      -      - TiledMapTileLayer
    -      -      -      - TiledMapTileSet
    -      -      -      - TiledMapTileSets

MAPS/TILED/LOADERS
------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AtlasTmxMapLoader
    -      -      -      - BaseTmxMapLoader
    -      -      -      - TmxMapLoader

MAPS/TILED/OBJECTS
------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ImageDetails
    -      -      -      - TileContext
    -      -      -      - TiledMapTileMapObject
    -      -      -      - TileMetrics

MAPS/TILED/RENDERERS
--------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - BatchTiledMapRenderer
    -      -      -      - HexagonalTiledMapRenderer
    -      -      -      - IsometricStaggeredTiledMapRenderer
    -      -      -      - IsometricTiledMapRenderer
    -      -      -      - ITiledMapRenderer
    -      -      -      - OrthoCachedTiledMapRenderer
    -      -      -      - OrthogonalTiledMapRenderer

MAPS/TILED/TILES
----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AnimatedTileBuilder
    -      -      -      - AnimatedTileMapTile
    -      -      -      - StaticTileBuilder
    -      -      -      - StaticTiledMapTile

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

MATHS
-----

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Affine2
    -      -      -      - Bezier
    -      -      -      - Bresenham2
    -      -      -      - BSpline
    -      -      -      - CatmullRomSpline
    -      -      -      - Circle
    -      -      -      - ConvexHull
    -      -      -      - CumulativeDistribution
    -      -      -      - DelaunayTriangulator        Unsure about method ComputeTriangles()
    -      -      -      - EarClippingTriangulator     Needs some testing
    -      -      -      - Ellipse
    -      -      -      - FloatCounter
    -      -      -      - FloatMatrixStructs
    -      -      -      - Frustrum
    -      -      -      - GeometryUtils
    -      -      -      - GridPoint2
    -      -      -      - GridPoint3
    -      -      -      - Interpolation
    -      -      -      - Intersector
    -      -      -      - IPath
    -      -      -      - IShape2D
    -      -      -      - IVector
    -      -      -      - MathUtils
    -      -      -      - Matrix3
    -      -      -      - Matrix4
    -      -      -      - NumberUtils
    -      -      -      - Plane
    -      -      -      - Point2D
    -      -      -      - Polygon
    -      -      -      - Polyline
    -      -      -      - Quaternion
    -      -      -      - RandomXS128
    -      -      -      - Rectangle
    -      -      -      - SimpleVectors
    -      -      -      - Vector2
    -      -      -      - Vector3
    -      -      -      - Vector4
    -      -      -      - WindowedMean

MATH/COLLISION
--------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - BoundingBox
    -      -      -      - Ray
    -      -      -      - Segment
    -      -      -      - Sphere

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

NETWORK
-------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - HttpParameterUtils
    -      -      -      - HttpRequestBuilder
    -      -      -      - HttpStatus
    -      -      -      - IHttpRequestHeader
    -      -      -      - IHttpResponseHeader
    -      -      -      - INet
    -      -      -      - IServerSocket
    -      -      -      - ISocket
    -      -      -      - NetHandler
    -      -      -      - ServerSocketHints
    -      -      -      - SocketHints

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

SCENES/SCENE2D
--------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Action
    -      -      -      - Actor
    -      -      -      - Event
    -      -      -      - Group
    -      -      -      - IAction
    -      -      -      - IActor
    -      -      -      - InputEvent
    -      -      -      - Stage
    -      -      -      - Touchable

SCENES/SCENE2D/ACTIONS
----------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Actions
    -      -      -      - AddAction
    -      -      -      - AddListenerAction
    -      -      -      - AfterAction
    -      -      -      - AlphaAction
    -      -      -      - ColorAction
    -      -      -      - CountdownEventAction
    -      -      -      - DelayAction
    -      -      -      - DelegateAction
    -      -      -      - EventAction
    -      -      -      - FloatAction
    -      -      -      - IntAction
    -      -      -      - LayoutAction
    -      -      -      - MoveByAction
    -      -      -      - MoveToAction
    -      -      -      - ParallelAction
    -      -      -      - RelativeTemporalAction
    -      -      -      - RemoveAction
    -      -      -      - RemoveActorAction
    -      -      -      - RemoveListenerAction
    -      -      -      - RepeatAction
    -      -      -      - RotateByAction
    -      -      -      - RotateToAction
    -      -      -      - RunnableAction
    -      -      -      - ScaleByAction
    -      -      -      - ScaleToAction
    -      -      -      - SequenceAction
    -      -      -      - SizeByAction
    -      -      -      - SizeToAction
    -      -      -      - TemporalAction
    -      -      -      - TimeScaleAction
    -      -      -      - TouchableAction
    -      -      -      - VisibleAction

SCENES/SCENE2D/LISTENERS
------------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ActorGestureListener
    -      -      -      - ChangeListener
    -      -      -      - ClickListener
    -      -      -      - DialogChangeListener
    -      -      -      - DialogFocusListener
    -      -      -      - DialogInputListener
    -      -      -      - DragListener
    -      -      -      - DragScrollListener
    -      -      -      - FocusListener
    -      -      -      - IEventListener
    -      -      -      - InputListener
    -      -      -      - ScrollPaneListeners

```
TODO: Use Lambdas for these, i.e.

AddListener( new ClickListener()
{
    // Clicked needs to be a Func<>
    Clicked = ( ev, x, y ) =>
    {
    }
} );
```

SCENES/SCENE2D/UI
-----------------

    TODO: I don't like the way Cell and Value classes are implemented.
          They seem confusing and are candidates for a rewrite.

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Button
    -      -      -      - ButtonGroup
    -      -      -      - Cell
    -      -      -      - CheckBox
    -      -      -      - Container
    -      -      -      - Dialog
    -      -      -      - HorizontalGroup
    -      -      -      - ImageButton
    -      -      -      - ImageTextButton
    -      -      -      - IOnScreenKeyboard
    -      -      -      - Label
    -      -      -      - ListBox
    -      -      -      - ParticleEffectActor
    -      -      -      - ProgressBar
    -      -      -      - Scene2DImage
    -      -      -      - ScrollPane
    -      -      -      - SelectBox
    -      -      -      - Skin                    Needs Json updates
    -      -      -      - Slider
    -      -      -      - SplitPane
    -      -      -      - Stack
    -      -      -      - Table
    -      -      -      - TextArea
    -      -      -      - TextButton
    -      -      -      - TextField
    -      -      -      - TextTooltip
    -      -      -      - Tooltip
    -      -      -      - TooltipManager
    -      -      -      - Touchpad
    -      -      -      - Tree
    -      -      -      - UISkin
    -      -      -      - Value
    -      -      -      - VerticalGroup
    -      -      -      - Widget
    -      -      -      - WidgetGroup
    -      -      -      - Window

SCENES/SCENE2D/UTILS
--------------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ArraySelection
    -      -      -      - BaseDrawable
    -      -      -      - DragAndDrop
    -      -      -      - ICullable
    -      -      -      - IDisableable
    -      -      -      - IDrawable
    -      -      -      - ILayout
    -      -      -      - ITransformDrawable
    -      -      -      - NinePatchDrawable
    -      -      -      - ScissorStack
    -      -      -      - Selection
    -      -      -      - SpriteDrawable
    -      -      -      - TextureRegionDrawable
    -      -      -      - TiledDrawable

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

UTILS
-----

    - Move Utils/Collections out of Utils and into somewhere more appropriate.

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - Align
    -      -      -      - AsyncExecutor
    -      -      -      - AsyncResult
    -      -      -      - BaseClassFactory
    -      -      -      - BinaryHeap
    -      -      -      - Bits
    -      -      -      - Buffer<T>
    -      -      -      - BufferUtils
    -      -      -      - ByteOrder
    -      -      -      - BytePointerToString
    -      -      -      - CaseInsensitiveEnumArrayConverterFactory
    -      -      -      - ComparableTimSort
    -      -      -      - Constants
    -      -      -      - DataOutput
    -      -      -      - DataUtils
    -      -      -      - GCSuppressor
    -      -      -      - HashHelpers
    -      -      -      - IAsyncTask
    -      -      -      - IClearablePool
    -      -      -      - IClipboard
    -      -      -      - ICloseable
    -      -      -      - IDrawable            Conflicts with Scene2D.Utils.IDrawable
    -      -      -      - IManaged
    -      -      -      - IReadable
    -      -      -      - IResetable
    -      -      -      - IRunnable
    -      -      -      - MinimalCrc32
    -      -      -      - PerformanceCounter
    -      -      -      - PerformanceCounters
    -      -      -      - PropertiesUtils
    -      -      -      - QuadTreeFloat
    -      -      -      - QuickSelect
    -      -      -      - Scaling
    -      -      -      - ScreenUtils
    -      -      -      - Selector
    -      -      -      - SortUtils
    -      -      -      - StreamUtils
    -      -      -      - StringUtils
    -      -      -      - SystemArrayUtils
    -      -      -      - TimerHelpers
    -      -      -      - Timer
    -      -      -      - TimeUtils
    -      -      -      - TimSort

UTILS/COLLECTIONS
-----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - ArrayList
    -      -      -      - ByteArray
    -      -      -      - Collections
    -      -      -      - DelayedRemovalList
    -      -      -      - DictionaryExtensions
    -      -      -      - DirectoryInfoComparer
    -      -      -      - IPredicate
    -      -      -      - LinkedHashMap
    -      -      -      - ListExtensions
    -      -      -      - ObjectMap
    -      -      -      - OrderedMap
    -      -      -      - PredicateIterable
    -      -      -      - PredicateIterator
    - DONE - DONE - DONE - SnapshotArrayList

UTILS/EXCEPTIONS
----------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - AssetNotLoadedException
    -      -      -      - Guard
    -      -      -      - NoSuchElementException
    -      -      -      - ReadOnlyBufferException
    -      -      -      - RuntimeException
    -      -      -      - SerializationException

UTILS/LOGGING
-------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - FPSLogger
    -      -      -      - IPreferences
    -      -      -      - Logger
    -      -      -      - Preferences
    -      -      -      - Stats

UTILS/POOLING
-------------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - FlushablePool
    -      -      -      - IPoolable
    -      -      -      - Pool
    -      -      -      - PooledLinkedList
    -      -      -      - Pools


UTILS/XML
---------

    CODE   DOCU   FOOTER
    ----   ----   ------
    -      -      -      - XmlReader
    -      -      -      - XmlWriter

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
