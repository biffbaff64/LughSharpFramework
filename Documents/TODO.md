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

- All uses of IRunnable.Runnable need checking and correcting.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- NO MAGIC NUMBERS!!!
- SORT OUT VERSIONING!!!
- PRIORITY is 2D classes first

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- There seems to be different namings for width/height etc. properties and methods. Make it more uniform
- Make more use of `<inheritdoc cref=""/>` or just `<inheritdoc/>`

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- IP = Conversion In Progress.
- DONE = Class finished but may not be fully 'CSHARP-ified'
- First column is for Code, Second column is for Documentation.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

Integrate DesktopBackend into LughSharp?

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

ASSETS
------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - AssetDescriptor
    - DONE - DONE - AssetLoaderParameters
    - DONE - DONE - AssetLoadingTask
    - DONE - DONE - AssetManager
    - DONE - DONE - AssetManagerHelper
    - DONE - DONE - IAssetErrorListener
    - DONE - DONE - IAssetManager
    - DONE - DONE - RefCountedContainer

ASSETS/LOADERS
--------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - AssetLoader
    - DONE - DONE - AsynchronousAssetLoader
    - DONE - DONE - BitmapFontLoader
    - DONE - DONE - CubemapLoader
    - DONE - DONE - ModelLoader
    - DONE - DONE - MusicLoader
    - DONE - DONE - ParticleEffectLoader
    - DONE - DONE - PixmapLoader
    - DONE - DONE - ShaderProgramLoader
    - DONE - DONE - SkinLoader
    - DONE - DONE - SoundLoader
    - DONE - DONE - SynchronousAssetLoader
    - DONE - DONE - TextureAtlasLoader
    - DONE - DONE - TextureLoader

ASSETS/LOADERS/RESOLVERS
------------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - AbsoluteFileHandleResolver
    - DONE - DONE - ClasspathFileHandleResolver
    - DONE - DONE - ExternalFileHandleResolver
    - DONE - DONE - IFileHandleResolver
    - DONE - DONE - InternalFileHandleResolver
    - DONE - DONE - LocalFileHandleResolver
    - DONE - DONE - PrefixFileHandleResolver
    - DONE - DONE - ResolutionFileResolver

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- **I'm currently considering ditching Lugh.Audio in favour of NAudio.**
- **Decision to be made asap.**

AUDIO
-----

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - IAudio
    - DONE - DONE - IAudioDevice
    - DONE - DONE - IAudioDeviceAsync
    - DONE - DONE - IAudioRecorder
    - DONE - DONE - IMusic
    - DONE - DONE - ISound

AUDIO/MAPONUS ( MAPONUS is the God of Music )
---------------------------------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - Buffer16BitSterso
    - DONE - DONE - MP3SharpException
    - DONE - DONE - MP3Stream
    - DONE - DONE - SoundFormat

AUDIO/MAPONUS/DECODING
-----------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - AudioBase
    - DONE - DONE - BitReserve
    - DONE - DONE - Bitstream
    - DONE - DONE - BitstreamErrors
    - DONE - DONE - BitstreamException
    - DONE - DONE - CircularByteBuffer
    - DONE - DONE - Crc16
    - DONE - DONE - Decoder
    - DONE - DONE - DecoderParameters
    - DONE - DONE - DecoderErrors
    - DONE - DONE - DecoderException
    - DONE - DONE - Equalizer
    - DONE - DONE - Header
    - DONE - DONE - Huffman
    - DONE - DONE - OutputChannels
    - DONE - DONE - PushbackStream
    - DONE - DONE - SampleBuffer
    - DONE - DONE - SynthesisFilter

AUDIO/MAPONUS/DECODING/DECODERS
-----------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - ASubband
    - DONE - DONE - IFrameDecoder
    - DONE - DONE - LayerIDecoder
    - DONE - DONE - LayerIIDecoder
    - DONE - IP   - LayerIIIDecoder

AUDIO/MAPONUS/DECODING/DECODERS/LAYERI
---------------------------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - SubbandLayer1
    - DONE - IP   - SubbandLayer1IntensityStereo
    - DONE - IP   - SubbandLayer1Stereo

AUDIO/MAPONUS/DECODING/DECODERS/LAYERII
----------------------------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - SubbandLayer2
    - DONE - IP   - SubbandLayer2IntensityStereo
    - DONE - IP   - SubbandLayer2Stereo

AUDIO/MAPONUS/DECODING/DECODERS/LAYERIII
-----------------------------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - ChannelData
    - DONE - IP   - GranuleInfo
    - DONE - IP   - Layer3SideInfo
    - DONE - IP   - SBI
    - DONE - IP   - ScaleFactorData
    - DONE - IP   - ScaleFactorTable

AUDIO/MAPONUS/IO
-----------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - RandomAccessFileStream
    - DONE - IP   - RiffFile
    - DONE - IP   - WaveFile
    - DONE - IP   - WaveFileBuffer

AUDIO/MAPONUS/SUPPORT
----------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - SupportClass

AUDIO/OPENAL
------------

    CODE   DOCUMENT
    ----   --------
    - DONE - N/A  - AL
    - DONE - N/A  - ALC

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

CORE
----

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - ApplicationAdapter
    - DONE - DONE - ApplicationConfiguration
    - DONE - DONE - Engine
    - DONE - DONE - Game
    - DONE - DONE - GameTime
    - DONE - DONE - IApplication
    - DONE - DONE - IApplicationListener
    - DONE - DONE - ILifecycleListener
    - DONE - DONE - IPreferences
    - DONE - DONE - IScreen
    - DONE - DONE - LibraryVersion
    - DONE - DONE - Platform
    - DONE - DONE - ScreenAdapter

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

FILES
-----

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - Files
    - DONE - DONE - IFilenameFilter
    - DONE - DONE - IFiles
    - IP   - IP   - InputStream
    - DONE - DONE - IOUtils

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

GRAPHICS
--------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - AbstractGraphics
    - DONE - DONE - BMPFormatStructs
    - DONE - DONE - Color
    - DONE - DONE - Colors
    - DONE - DONE - Cubemap
    - DONE - DONE - FrameBufferConfig
    - DONE - IP   - GLTexture
    - DONE - IP   - GLTextureArray
    - DONE - DONE - GraphicsBackend
    - DONE - DONE - GraphicsDevice
    - DONE - DONE - GraphicsEnums
    - DONE - DONE - GStructs
    - DONE - DONE - ICubemapData
    - DONE - DONE - ICursor
    - DONE - DONE - IGraphicsDevice
    - DONE - DONE - IGraphicsDevice.DisplayMode
    - DONE - DONE - IGraphicsDevice.Monitor
    - DONE - DONE - ITextureArrayData
    - DONE - DONE - ITextureData
    - IP   - IP   - ManagedTextureHandle
    - DONE - DONE - Mesh
    - DONE - DONE - Pixmap
    - DONE - IP   - PixmapFormat
    - IP   - IP   - PixmapIO
    - DONE - IP   - Texture
    - DONE - DONE - TextureDataFactory
    - DONE - IP   - TextureRegion
    - DONE - DONE - VertexAttribute
    - DONE - DONE - VertexAttributes

GRAPHICS/ATLASES
----------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - AtlasRegion
    - DONE - IP   - AtlasSprite
    - DONE - IP   - TextureAtlas
    - DONE - IP   - TextureAtlasData
    - DONE - IP   - TextureAtlasDataExtensions

GRAPHICS/CAMERAS
----------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - Camera
    - DONE - DONE - IGameCamera
    - DONE - DONE - OrthographicCamera
    - IP   - IP   - OrthographicGameCamera
    - DONE - DONE - PerspectiveCamera
    - DONE - DONE - Shake

GRAPHICS/FRAMEBUFFERS
---------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - FloatFrameBuffer
    - DONE - IP   - FloatFrameBufferBuilder
    - DONE - IP   - FrameBuffer
    - DONE - IP   - FrameBufferBuilder
    - DONE - IP   - FrameBufferCubemap
    - DONE - IP   - FrameBufferCubemapBuilder
    - DONE - IP   - FrameBufferRenderBufferAttachmentSpec
    - DONE - IP   - FrameBufferTextureAttachmentSpec
    - DONE - IP   - GLFrameBuffer
    - DONE - IP   - GLFrameBufferBuilder

GRAPHICS/G2D
------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - Animation
    - IP   - IP   - CpuSpriteBatch                  Some methods have too many parameters
    - DONE - IP   - Gdx2DPixmap
    - DONE - IP   - Gdx2DPixmapExtensions
    - DONE - IP   - Gdx2DPixmapUtils
    - DONE - DONE - IBatch
    - DONE - IP   - IPolygonBatch                   Some methods have too many parameters
    - DONE - IP   - NinePatch
    - DONE - IP   - ParticleEffect
    - DONE - IP   - ParticleEffectPool
    - DONE - IP   - ParticleEmitter
    - DONE - IP   - PixmapPacker
    - DONE - IP   - PixmapPackerIO
    - DONE - IP   - PolygonRegion
    - DONE - IP   - PolygonRegionLoader
    - DONE - IP   - PolygonSprite
    - IP   - IP   - PolygonSpriteBatch              Some methods have too many parameters
    - DONE - IP   - RepeatablePolygonSprite
    - DONE - IP   - Sprite
    - IP   - IP   - SpriteBatch
    - DONE - IP   - SpriteCache

GRAPHICS/G3D
------------

    See Documents/TODO_G3D.MD

GRAPHICS/OPENGL
---------------

    Change uint parameters in Bindings methods to int, and do conversion to uint
    inside those methods when necessary. This will remove the amount of casting to
    uint in the code.

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - GLBindings
    - DONE - DONE - GLBindingsHelpers
    - DONE - DONE - GLBindingsStructs
    - DONE - DONE - GLConsts
    - DONE - DONE - GLFormatChooser
    - DONE - DONE - GLFunctionDelegates
    - DONE - DONE - GLFunctionsLoader
    - DONE - DONE - GLImage
    - DONE - DONE - GLUtils
    - DONE - DONE - IGL
    - DONE - DONE - IGLBindings
    - DONE - DONE - IGLBindingsExtra
    - IP   - IP   - IGLBindingsHelpers

GRAPHICS/OPENGL/ENUMS
---------------------

    CODE   DOCUMENT
    ----   --------
    - IP   - IP   - BlendMode
    - IP   - IP   - BufferEnums
    - IP   - IP   - ClearBufferMask
    - IP   - IP   - CompareFunction
    - IP   - IP   - CullFaceMode
    - IP   - IP   - DataType
    - IP   - IP   - DebugEnums
    - IP   - IP   - DrawElementsType
    - IP   - IP   - EnableCap
    - IP   - IP   - FrameBufferEnums
    - IP   - IP   - GetPName
    - IP   - IP   - LogicOp
    - IP   - IP   - MatrixMode
    - IP   - IP   - PixelEnums
    - IP   - IP   - PolygonMode
    - IP   - IP   - PrimitiveType
    - IP   - IP   - ProgramParameter
    - IP   - IP   - QueryTarget
    - IP   - IP   - ShaderEnums
    - IP   - IP   - StencilEnums
    - IP   - IP   - StringName
    - IP   - IP   - TextureFilterMode
    - IP   - IP   - TextureFormat
    - IP   - IP   - TextureLimits
    - IP   - IP   - TextureParameterName
    - IP   - IP   - TextureTarget
    - IP   - IP   - TextureUnit
    - IP   - IP   - TextureWrapMode
    - IP   - IP   - VertexAttribParameter
    - IP   - IP   - VertexAttribType

GRAPHICS/OPENGL/GLSL
--------------------

    CODE   DOCUMENT
    ----   --------
    - IP   - IP   - ColorTest.glsl.frag
    - IP   - IP   - ColorTest.glsl.vert
    - IP   - IP   - Default.glsl.frag
    - IP   - IP   - Default.glsl.vert
    - IP   - IP   - Quad.glsl.frag
    - IP   - IP   - Quad.glsl.vert

GRAPHICS/OPENGL/NEWBINDINGS <= Merge into GRAPHICS/OPENGL
---------------------------

    CODE   DOCUMENT
    ----   --------
    - IP   -      - BufferObjectBindings
    -      -      - ContextBindings
    - IP   -      - DebugBindings
    - DONE - DONE - DebugSeverity
    -      -      - DrawBindings
    - IP   -      - ErrorBindings
    -      -      - FrameBufferBindings
    - IP   -      - ProgramBindings
    - IP   -      - QueryBindings
    -      -      - RenderBufferBindings
    - IP   -      - ShaderBindings
    -      -      - StateBindings
    -      -      - StencilBindings
    - IP   -      - TextureBindings
    - IP   -      - TextureSamplerBindings
    -      -      - TransformBindings
    -      -      - TransformFeedbackBindings
    - IP   -      - UniformBindings
    - IP   -      - VertexArrayBindings
    - IP   -      - VertexAttribBindings

GRAPHICS/TEXT
-------------

    CODE   DOCUMENT
    ----   --------
    - IP   - IP   - BitmapFont
    - IP   - IP   - BitmapFontCache
    - DONE - DONE - CharacterUtils
    - DONE - IP   - DistanceFieldFont
    - IP   - IP   - FontUtils
    - DONE - IP   - GlyphLayout
    - IP   - IP   - RegexUtils
    - IP   - IP   - Subset
    - IP   - IP   - UnicodeBlock

GRAPHICS/TEXT/FREETYPE
----------------------

    CODE   DOCUMENT
    ----   --------
    - IP   - IP   - FreeType
    - IP   - IP   - FreeTypeConstants
    - IP   - IP   - FreeTypeFontGenerator
    - IP   - IP   - FreeTypeFontGeneratorLoader
    - IP   - IP   - FreeTypeFontLoader

GRAPHICS/UTILS
--------------

    CODE   DOCUMENT
    ----   --------
    - IP   - IP   - ETC1
    - DONE - IP   - ETC1TextureData
    - DONE - IP   - FacedCubemapData
    - DONE - IP   - FileTextureArrayData
    - DONE - IP   - FileTextureData
    - DONE - IP   - FloatTextureData
    - DONE - IP   - GLOnlyTextureData
    - DONE - IP   - GLVersion
    - DONE - DONE - HdpiMode
    - DONE - DONE - HdpiUtils
    - DONE - DONE - IIndexData
    - DONE - IP   - IInstanceData
    - DONE - IP   - IndexArray
    - DONE - DONE - IndexBufferObject
    - IP   - IP   - IndexBufferObjectSubData
    - IP   - IP   - InstanceBufferObject
    - IP   - IP   - InstanceBufferObjectSubData
    - DONE - DONE - IVertexData
    - DONE - IP   - KTXTTextureData
    - DONE - IP   - ManagedShaderProgram
    - DONE - IP   - MipMapGenerator
    - DONE - IP   - MipMapTextureData
    - DONE - IP   - PixmapTextureData
    - IP   - IP   - PNGDecoder
    - DONE - IP   - ShaderProgram
    - DONE - IP   - ShapeRenderer
    - IP   - IP   - TextureUtils
    - DONE - DONE - Vertex
    - DONE - DONE - VertexArray
    - DONE - DONE - VertexBufferObject
    - DONE - IP   - VertexBufferObjectSubData
    - DONE - IP   - VertexBufferObjectWithVAO
    - IP   - IP   - VertexConstants

GRAPHICS/VIEWPORT
-----------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - ExtendedViewport
    - DONE - DONE - FillViewport
    - DONE - DONE - FitViewport
    - DONE - DONE - ScalingViewport
    - DONE - DONE - ScreenViewport
    - DONE - DONE - StretchViewport
    - DONE - DONE - Viewport

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

INPUT
-----

    - DONE - DONE - AbstractInput
    - IP   - IP   - GestureDetector
    - DONE - DONE - IInput
    - DONE - DONE - IInputProcessor
    - DONE - DONE - InputAdapter
    - DONE - DONE - InputEventQueue
    - DONE - DONE - InputMultiplexer
    - DONE - DONE - InputUtils
    -      -      - RemoteInput
    - IP   - IP   - RemoteSender

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

MAPS
----

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - IImageResolver
    - DONE - DONE - IMapRenderer
    - DONE - DONE - Map
    - DONE - DONE - MapGroupLayer
    - DONE - DONE - MapLayer
    - DONE - DONE - MapLayers
    - DONE - DONE - MapObject
    - DONE - DONE - MapObjects
    - DONE - DONE - MapProperties

MAPS/OBJECTS
------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - CircleMapObject
    - DONE - DONE - EllipseMapObject
    - DONE - DONE - PolygonMapObject
    - DONE - DONE - PolylineMapObject
    - DONE - DONE - RectangleMapObject
    - DONE - DONE - TextureMapObject

MAPS/TILED
----------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - ITiledMapTile
    - DONE - DONE - TiledMap
    - DONE - DONE - TiledMapImageLayer
    - DONE - DONE - TiledMapTileLayer
    - DONE - DONE - TiledMapTileSet
    - DONE - DONE - TiledMapTileSets

MAPS/TILED/LOADERS
------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - AtlasTmxMapLoader
    - DONE - IP   - BaseTmxMapLoader
    - DONE - IP   - TmxMapLoader

MAPS/TILED/OBJECTS
------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - TiledMapTileMapObject

MAPS/TILED/RENDERERS
--------------------

    CODE   DOCUMENT
    ----   --------
    - IP   - IP   - BatchTiledMapRenderer
    - IP   - IP   - HexagonalTiledMapRenderer
    - IP   - IP   - IsometricStaggeredTiledMapRenderer
    - IP   - IP   - IsometricTiledMapRenderer
    - IP   - IP   - ITiledMapRenderer
    - IP   - IP   - OrthoCachedTiledMapRenderer
    - IP   - IP   - OrthogonalTiledMapRenderer

MAPS/TILED/TILES
----------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - AnimatedTileMapTile
    - DONE - DONE - StaticTiledMapTile

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

MATHS
-----

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - Affine2
    - DONE - DONE - Bezier
    - DONE - IP   - Bresenham2
    - DONE - IP   - BSpline
    - DONE - IP   - CatmullRomSpline
    - DONE - DONE - Circle
    - DONE - IP   - ConvexHull
    - DONE - IP   - CumulativeDistribution
    - IP   - IP   - DelaunayTriangulator        Unsure about method ComputeTriangles()
    - IP   - IP   - EarClippingTriangulator     Needs some testing
    - DONE - DONE - Ellipse
    - DONE - IP   - FloatCounter
    - DONE - IP   - FloatMatrixStructs
    - DONE - IP   - Frustrum
    - DONE - IP   - GeometryUtils
    - DONE - IP   - GridPoint2
    - DONE - IP   - GridPoint3
    - DONE - IP   - Interpolation
    - DONE - IP   - Intersector
    - IP   - IP   - IntToByte
    - DONE - IP   - IPath
    - DONE - DONE - IShape2D
    - DONE - IP   - IVector
    - DONE - IP   - MathUtils
    - DONE - IP   - Matrix3x3
    - DONE - IP   - Matrix4x4
    - DONE - IP   - Number
    - DONE - IP   - NumberUtils
    - DONE - IP   - Plane
    - DONE - DONE - Point2D
    - DONE - IP   - Polygon
    - IP   - IP   - Polyline
    - DONE - IP   - Quaternion
    - DONE - IP   - RandomXS128
    - DONE - IP   - Rectangle
    - DONE - DONE - SimpleVectors
    - DONE - IP   - Vector2
    - DONE - IP   - Vector3
    - IP   - IP   - Vector4
    - DONE - IP   - WindowedMean

MATH/COLLISION
--------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - Area2D
    - DONE - IP   - BoundingBox
    - DONE - DONE - Ray
    - DONE - IP   - Segment
    - DONE - IP   - Sphere

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

NETWORK
-------

    CODE   DOCUMENT
    ----   --------
    -      -      - HttpParameterUtils
    -      -      - HttpRequestBuilder
    -      -      - HttpStatus
    -      -      - IHttpRequestHeader
    -      -      - IHttpResponseHeader
    - DONE - DONE - INet
    -      -      - IServerSocket
    -      -      - ISocket
    -      -      - NetJavaImpl
    -      -      - NetJavaServerSocketImpl
    -      -      - NetJavaSocketImpl
    -      -      - ServerSocketHints
    -      -      - SocketHints

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

SCENES/SCENE2D
--------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - Action
    - DONE - IP   - Actor
    - DONE - IP   - Event
    - DONE - IP   - Group
    - DONE - DONE - IActor
    - DONE - DONE - InputEvent
    - DONE - IP   - Stage
    - DONE - DONE - Touchable

SCENES/SCENE2D/ACTIONS
----------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - Actions
    - DONE - IP   - AddAction
    - DONE - IP   - AddListenerAction
    - DONE - IP   - AfterAction
    - DONE - DONE - AlphaAction
    - DONE - IP   - ColorAction
    - DONE - IP   - CountdownEventAction
    - DONE - IP   - DelayAction
    - DONE - IP   - DelegateAction
    - DONE - IP   - EventAction
    - DONE - DONE - FloatAction
    - DONE - DONE - IntAction
    - DONE - IP   - LayoutAction
    - DONE - IP   - MoveByAction
    - DONE - IP   - MoveToAction
    - DONE - IP   - ParallelAction
    - DONE - IP   - RelativeTemporalAction
    - DONE - IP   - RemoveAction
    - DONE - IP   - RemoveActorAction
    - DONE - IP   - RemoveListenerAction
    - DONE - IP   - RepeatAction
    - DONE - IP   - RotateByAction
    - DONE - IP   - RotateToAction
    - DONE - IP   - RunnableAction
    - DONE - IP   - ScaleByAction
    - DONE - IP   - ScaleToAction
    - DONE - IP   - SequenceAction
    - DONE - DONE - SizeByAction
    - DONE - DONE - SizeToAction
    - DONE - DONE - TemporalAction
    - DONE - DONE - TimeScaleAction
    - DONE - DONE - TouchableAction
    - DONE - DONE - VisibleAction

SCENES/SCENE2D/LISTENERS
------------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - ActorGestureListener
    - DONE - DONE - ChangeListener
    - DONE - IP   - ClickListener
    - DONE - IP   - DragListener
    - DONE - IP   - DragScrollListener
    - DONE - IP   - FocusListener
    - DONE - DONE - IEventListener
    - DONE - DONE - InputListener

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

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - Button
    - DONE - IP   - ButtonGroup
    - IP   - IP   - Cell
    - IP   - IP   - CheckBox
    - IP   - IP   - Container
    - IP   - IP   - Dialog
    - IP   - IP   - DialogChangeListener
    - IP   - IP   - DialogFocusListener
    - IP   - IP   - DialogInputListener
    - IP   - IP   - HorizontalGroup
    - IP   - IP   - Image
    - IP   - IP   - ImageButton
    - IP   - IP   - ImageTextButton
    - DONE - DONE - IOnScreenKeyboard
    - IP   - IP   - Label
    - IP   - IP   - ListBox
    - IP   - IP   - ParticleEffectActor
    - IP   - IP   - ProgressBar
    - IP   - IP   - ScrollPane
    - IP   - IP   - ScrollPaneListeners
    - DONE - IP   - SelectBox
    - IP   - IP   - Skin                    Needs Json updates
    - DONE - IP   - Slider
    - DONE - IP   - SplitPane
    - DONE - IP   - Stack
    - DONE - IP   - Table
    - DONE - IP   - TextArea
    - DONE - IP   - TextButton
    - DONE - IP   - TextField
    - DONE - IP   - TextTooltip
    - DONE - IP   - Tooltip
    - DONE - IP   - TooltipManager
    - DONE - IP   - Touchpad
    - DONE - IP   - Tree
    - DONE - IP   - Value
    - DONE - IP   - VerticalGroup
    - DONE - IP   - Widget
    - DONE - IP   - WidgetGroup
    - DONE - IP   - Window

SCENES/SCENE2D/UTILS
--------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - ArraySelection
    - DONE - DONE - BaseDrawable
    - DONE - IP   - DragAndDrop
    - DONE - DONE - ICullable
    - DONE - IP   - IDisableable
    - DONE - DONE - IDrawable
    - DONE - DONE - ILayout
    - DONE - DONE - ITransformDrawable
    - DONE - IP   - NinePatchDrawable
    - DONE - IP   - ScissorStack
    - DONE - IP   - Selection
    - DONE - IP   - SpriteDrawable
    - DONE - DONE - TextureRegionDrawable
    - DONE - IP   - TiledDrawable

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

UTILS
-----

    - Move Utils/Collections out of Utils and into somewhere more appropriate.

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - Alignment
    - IP   - IP   - BaseClassFactory
    - DONE - DONE - BinaryHeap
    - DONE - DONE - Bits
    - DONE - DONE - ByteOrder
    - DONE - DONE - BytePointerToString
    - IP   - IP   - CaseInsensitiveEnumArrayConverterFactory
    - DONE - IP   - ComparableTimSort
    - DONE - DONE - Constants
    - DONE - IP   - DataInput
    - DONE - IP   - DataOutput
    - DONE - DONE - DataUtils
    - DONE - DONE - FPSLogger
    - DONE - IP   - GCSuppressor
    - DONE - IP   - GdxNativesLoader
    - IP   - IP   - HashHelpers
    - IP   - IP   - IClearablePool
    - DONE - DONE - IClipboard
    - DONE - DONE - ICloseable
    - DONE - DONE - IDrawable
    - DONE - DONE - IManaged
    - DONE - DONE - IReadable
    - DONE - DONE - IResetable
    - DONE - DONE - IRunnable
    - DONE - DONE - Logger
    - IP   - IP   - LughTestAdapter
    - DONE - DONE - PerformanceCounter
    - DONE - IP   - PerformanceCounters
    - IP   - IP   - PhysicsUtils
    - DONE - DONE - PropertiesUtils
    - DONE - IP   - QuadTreeFloat
    - DONE - IP   - QuickSelect
    - DONE - IP   - Scaling
    - DONE - DONE - ScreenUtils
    - DONE - IP   - Selector
    - DONE - IP   - SingletonBase<T>
    - DONE - DONE - SortUtils
    - IP   - IP   - StringUtils
    - IP   - IP   - SystemArrayUtils
    - DONE - IP   - Timer
    - DONE - DONE - TimeUtils
    - DONE - IP   - TimSort

UTILS/BUFFERS
-------------

     CODE   DOCUMENT
    ----   --------
    - DONE - DONE - Buffer
    - DONE - IP   - BufferUtils
    - DONE - DONE - ByteBuffer
    - DONE - DONE - FloatBuffer
    - DONE - DONE - IntBuffer
    - DONE - DONE - ShortBuffer

UTILS/COLLECTIONS
-----------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - ArrayList<T>
    - DONE - IP   - ByteArray
    - DONE - DONE - DelayedRemovalList
    - DONE - DONE - DictionaryExtensions
    - IP   - IP   - IdentityMap< K, V >
    - IP   - IP   - IPredicate
    - DONE - DONE - ListExtensions
    - DONE - DONE - ObjectMap< K, V >
    - IP   - IP   - OrderedMap<K, V>
    - IP   - IP   - PredicateIterable
    - IP   - IP   - PredicateIterator
    - DONE - DONE - SnapshotArrayList<T>

UTILS/EXCEPTIONS
----------------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - AssetNotLoadedException
    - DONE - DONE - BufferOverflowException
    - DONE - DONE - BufferUnderflowException
    - DONE - DONE - Guard
    - DONE - DONE - GdxRuntimeException
    - DONE - DONE - NumberFormatException
    - DONE - DONE - ReadOnlyBufferException
    - DONE - DONE - SerializationException
    - DONE - DONE - SpriteBatchException

UTILS/POOLING
-------------

    CODE   DOCUMENT
    ----   --------
    - IP   - IP   - FlushablePool
    - DONE - DONE - IPoolable
    - DONE - DONE - Pool
    - DONE - DONE - PooledLinkedList
    - DONE - DONE - Pools

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    CODE   DOCUMENT
    ----   --------
    -      -      - 

