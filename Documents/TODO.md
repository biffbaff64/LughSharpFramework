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

ASSETS
------

    CODE   DOCUMENT
    ----   --------
    - DONE - DONE - AssetDescriptor
    - DONE - DONE - AssetHelper
    - DONE - DONE - AssetLoaderParameters
    - DONE - DONE - AssetLoadingTask
    - DONE - DONE - AssetManager
    - DONE - DONE - IAssetErrorListener
    - DONE - DONE - IAssetTask
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
    - DONE - IP   - SkinLoader
    - DONE - IP   - SoundLoader
    - DONE - IP   - SynchronousAssetLoader
    - DONE - IP   - TextureAtlasLoader
    - DONE - IP   - TextureLoader

ASSETS/LOADERS/RESOLVERS
------------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - AbsoluteFileHandleResolver
    - DONE - IP   - ClasspathFileHandleResolver
    - DONE - IP   - ExternalFileHandleResolver
    - DONE - IP   - IFileHandleResolver
    - DONE - IP   - InternalFileHandleResolver
    - DONE - IP   - LocalFileHandleResolver
    - DONE - IP   - PrefixFileHandleResolver
    - DONE - IP   - ResolutionFileResolver

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- **I'm currently considering ditching Lugh.Audio in favour of NAudio.**
- **Decision to be made asap.**

AUDIO
-----

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - IAudio
    - DONE - IP   - IAudioDevice
    - DONE - IP   - IAudioDeviceAsync
    - DONE - IP   - IAudioRecorder
    - DONE - IP   - IMusic
    - DONE - IP   - ISound

AUDIO/MAPONUS ( MAPONUS is the God of Music )
---------------------------------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - Buffer16BitSterso
    - DONE - IP   - MP3SharpException
    - DONE - IP   - MP3Stream
    - DONE - IP   - SoundFormat

AUDIO/MAPONUS/DECODING
-----------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - AudioBase
    - DONE - IP   - BitReserve
    - DONE - IP   - Bitstream
    - DONE - IP   - BitstreamErrors
    - DONE - IP   - BitstreamException
    - DONE - IP   - CircularByteBuffer
    - DONE - IP   - Crc16
    - DONE - IP   - Decoder
    - DONE - IP   - DecoderParameters
    - DONE - IP   - DecoderErrors
    - DONE - IP   - DecoderException
    - DONE - IP   - Equalizer
    - DONE - IP   - Header
    - DONE - IP   - Huffman
    - DONE - IP   - OutputChannels
    - DONE - IP   - PushbackStream
    - DONE - IP   - SampleBuffer
    - DONE - IP   - SynthesisFilter

AUDIO/MAPONUS/DECODING/DECODERS
-----------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - ASubband
    - DONE - IP   - IFrameDecoder
    - DONE - IP   - LayerIDecoder
    - DONE - IP   - LayerIIDecoder
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
    - DONE - IP   - ApplicationAdapter
    - DONE - IP   - ApplicationConfiguration
    - DONE - IP   - Engine
    - DONE - IP   - Game
    - DONE - IP   - GameTime
    - DONE - IP   - IApplication
    - DONE - IP   - IApplicationListener
    - DONE - IP   - ILifecycleListener
    - DONE - IP   - IScreen
    - DONE - IP   - LibraryVersion
    - DONE - IP   - Platform
    - DONE - IP   - ScreenAdapter

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

FILES
-----

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - Files
    - DONE - IP   - IFilenameFilter
    - DONE - IP   - IFiles
    - IP   - IP   - InputStream
    - DONE - IP   - IOUtils

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

GRAPHICS
--------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - BMPFormatStructs
    - DONE - IP   - Color
    - DONE - IP   - Colors
    - DONE - IP   - Cubemap
    - DONE - IP   - FrameBufferConfig
    - DONE - IP   - GLTexture
    - DONE - IP   - GLTextureArray
    - DONE - IP   - GraphicsBackend
    - DONE - IP   - GraphicsCapabilities
    - DONE - IP   - GraphicsDevice
    - DONE - IP   - GraphicsEnums
    - DONE - IP   - GStructs
    - DONE - IP   - ICubemapData
    - DONE - IP   - ICursor
    - DONE - IP   - IGraphicsDevice
    - DONE - IP   - IGraphicsDevice.DisplayMode
    - DONE - IP   - IGraphicsDevice.Monitor
    - IP   - IP   - Image
    - DONE - IP   - ITextureArrayData
    - DONE - IP   - ITextureData
    - IP   - IP   - ManagedTextureHandle
    - DONE - IP   - Mesh
    - DONE - IP   - Pixmap
    - DONE - IP   - PixmapDataType
    - IP   - IP   - PixmapIO
    - IP   - IP   - PNGFormatStructs
    - DONE - IP   - ShaderLoader
    - DONE - IP   - Texture
    - DONE - IP   - TextureDataFactory
    - DONE - IP   - TextureRegion
    - DONE - IP   - VertexAttribute
    - DONE - IP   - VertexAttributes

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
    - DONE - IP   - Camera
    - DONE - IP   - IGameCamera
    - DONE - IP   - OrthographicCamera
    - IP   - IP   - OrthographicGameCamera
    - DONE - IP   - PerspectiveCamera
    - DONE - IP   - Shake

GRAPHICS/FRAMEBUFFERS
---------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - FloatFrameBuffer
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
    - DONE - IP   - Animation
    - IP   - IP   - CpuSpriteBatch                  Some methods have too many parameters
    - DONE - IP   - Gdx2DPixmap
    - DONE - IP   - Gdx2DPixmapExtensions
    - DONE - IP   - Gdx2DPixmapUtils
    - DONE - IP   - IBatch
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
    - IP   -      - BufferObjectBindings
    - IP   -      - DebugBindings
    - DONE - IP   - DebugSeverity
    -      -      - DrawBindings
    - IP   -      - ErrorBindings
    -      -      - FrameBufferBindings
    - DONE - IP   - GLBindings
    - DONE - IP   - GLBindingsHelpers
    - DONE - IP   - GLBindingsStructs
    - DONE - IP   - GLConsts
    - IP   - IP   - GLData
    - DONE - IP   - GLDebugControl
    - DONE - IP   - GLFormatChooser
    - DONE - IP   - GLFunctionDelegates
    - DONE - IP   - GLFunctionsLoader
    - DONE - IP   - GLImage
    - DONE - IP   - GLUtils
    - DONE - IP   - IGL
    - DONE - IP   - IGLBindings
    - DONE - IP   - IGLBindingsExtra
    - IP   - IP   - OpenGL.Capabilities
    - IP   - IP   - OpenGL.Initialisation
    - IP   - IP   - IGLBindingsHelpers
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

GRAPHICS/OPENGL/ENUMS
---------------------

    CODE   DOCUMENT
    ----   --------
    - IP   - IP   - BlendMode
    - IP   - IP   - BufferEnums
    - IP   - IP   - ClearBufferMask
    - IP   - IP   - ClientApi
    - IP   - IP   - CompareFunction
    - IP   - IP   - ContextApi
    - IP   - IP   - CullFaceMode
    - IP   - IP   - DataType
    - IP   - IP   - DebugEnums
    - IP   - IP   - DrawElementsType
    - IP   - IP   - EnableCap
    - IP   - IP   - FrameBufferEnums
    - IP   - IP   - GLParameter
    - IP   - IP   - JoystickHats
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
    - IP   - IP   - TextureParameter
    - IP   - IP   - TextureTarget
    - IP   - IP   - TextureUnit
    - IP   - IP   - TextureUsage
    - IP   - IP   - TextureWrapMode
    - IP   - IP   - VertexAttribParameter
    - IP   - IP   - VertexAttribType

GRAPHICS/TEXT
-------------

    CODE   DOCUMENT
    ----   --------
    - IP   - IP   - BitmapFont
    - IP   - IP   - BitmapFont.BitmapFontData
    - IP   - IP   - BitmapFontCache
    - DONE - IP   - CharacterUtils
    - DONE - IP   - DistanceFieldFont
    - IP   - IP   - FontUtils
    - DONE - IP   - GlyphLayout
    - IP   - IP   - RegexUtils
    - IP   - IP   - Subset
    - IP   - IP   - UnicodeBlock

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
    - DONE - IP   - HdpiUtils
    - DONE - IP   - IImmediateModeRenderer
    - DONE - IP   - IIndexData
    - DONE - IP   - IInstanceData
    - DONE - IP   - ImmediateModeRenderer
    - DONE - IP   - IndexArray
    - DONE - IP   - IndexBufferObject
    - IP   - IP   - IndexBufferObjectSubData
    - IP   - IP   - InstanceBufferObject
    - IP   - IP   - InstanceBufferObjectSubData
    - DONE - IP   - IVertexData
    - DONE - IP   - KTXTTextureData
    - DONE - IP   - MipMapGenerator
    - DONE - IP   - MipMapTextureData
    - DONE - IP   - PixmapTextureData
    - IP   - IP   - PNGDecoder
    - DONE - IP   - ShaderProgram
    - DONE - IP   - ShapeRenderer
    - IP   - IP   - TextureUtils
    - DONE - IP   - Vertex
    - DONE - IP   - VertexArray
    - DONE - IP   - VertexBufferObject
    - DONE - IP   - VertexBufferObjectSubData
    - DONE - IP   - VertexBufferObjectWithVAO
    - IP   - IP   - VertexConstants

GRAPHICS/VIEWPORT
-----------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - ExtendedViewport
    - DONE - IP   - FillViewport
    - DONE - IP   - FitViewport
    - DONE - IP   - ScalingViewport
    - DONE - IP   - ScreenViewport
    - DONE - IP   - StretchViewport
    - DONE - IP   - Viewport

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

INPUT
-----

    - DONE - IP   - AbstractInput
    - IP   - IP   - GestureDetector
    - DONE - IP   - IInput
    - DONE - IP   - IInputProcessor
    - DONE - IP   - InputAdapter
    - DONE - IP   - InputEventQueue
    - DONE - IP   - InputMultiplexer
    - DONE - IP   - InputUtils
    -      -      - RemoteInput
    - IP   - IP   - RemoteSender

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

MAPS
----

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - IImageResolver
    - DONE - IP   - IMapRenderer
    - DONE - IP   - Map
    - DONE - IP   - MapGroupLayer
    - DONE - IP   - MapLayer
    - DONE - IP   - MapLayers
    - DONE - IP   - MapObject
    - DONE - IP   - MapObjects
    - DONE - IP   - MapProperties

MAPS/OBJECTS
------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - CircleMapObject
    - DONE - IP   - EllipseMapObject
    - DONE - IP   - PolygonMapObject
    - DONE - IP   - PolylineMapObject
    - DONE - IP   - RectangleMapObject
    - DONE - IP   - TextureMapObject

MAPS/TILED
----------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - ITiledMapTile
    - DONE - IP   - TiledMap
    - DONE - IP   - TiledMapImageLayer
    - DONE - IP   - TiledMapTileLayer
    - DONE - IP   - TiledMapTileSet
    - DONE - IP   - TiledMapTileSets

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
    - DONE - IP   - TiledMapTileMapObject

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
    - DONE - IP   - AnimatedTileMapTile
    - DONE - IP   - StaticTiledMapTile

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

MATHS
-----

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - Affine2
    - DONE - IP   - Bezier
    - DONE - IP   - Bresenham2
    - DONE - IP   - BSpline
    - DONE - IP   - CatmullRomSpline
    - DONE - IP   - Circle
    - DONE - IP   - ConvexHull
    - DONE - IP   - CumulativeDistribution
    - IP   - IP   - DelaunayTriangulator        Unsure about method ComputeTriangles()
    - IP   - IP   - EarClippingTriangulator     Needs some testing
    - DONE - IP   - Ellipse
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
    - DONE - IP   - IShape2D
    - DONE - IP   - IVector
    - DONE - IP   - MathUtils
    - DONE - IP   - Matrix3x3
    - DONE - IP   - Matrix4x4
    - DONE - IP   - Number
    - DONE - IP   - NumberUtils
    - DONE - IP   - Plane
    - DONE - IP   - Point2D
    - DONE - IP   - Polygon
    - IP   - IP   - Polyline
    - DONE - IP   - Quaternion
    - DONE - IP   - RandomXS128
    - DONE - IP   - Rectangle
    - DONE - IP   - SimpleVectors
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
    - DONE - IP   - Ray
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
    - DONE - IP   - INet
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
    - DONE - IP   - Action
    - DONE - IP   - Actor
    - DONE - IP   - Event
    - DONE - IP   - Group
    - DONE - IP   - IActor
    - DONE - IP   - InputEvent
    - DONE - IP   - Stage
    - DONE - IP   - Touchable

SCENES/SCENE2D/ACTIONS
----------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - Actions
    - DONE - IP   - AddAction
    - DONE - IP   - AddListenerAction
    - DONE - IP   - AfterAction
    - DONE - IP   - AlphaAction
    - DONE - IP   - ColorAction
    - DONE - IP   - CountdownEventAction
    - DONE - IP   - DelayAction
    - DONE - IP   - DelegateAction
    - DONE - IP   - EventAction
    - DONE - IP   - FloatAction
    - DONE - IP   - IntAction
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
    - DONE - IP   - SizeByAction
    - DONE - IP   - SizeToAction
    - DONE - IP   - TemporalAction
    - DONE - IP   - TimeScaleAction
    - DONE - IP   - TouchableAction
    - DONE - IP   - VisibleAction

SCENES/SCENE2D/LISTENERS
------------------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - ActorGestureListener
    - DONE - IP   - ChangeListener
    - DONE - IP   - ClickListener
    - DONE - IP   - DragListener
    - DONE - IP   - DragScrollListener
    - DONE - IP   - FocusListener
    - DONE - IP   - IEventListener
    - DONE - IP   - InputListener

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
    - DONE - IP   - IOnScreenKeyboard
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
    - DONE - IP   - ArraySelection
    - DONE - IP   - BaseDrawable
    - DONE - IP   - DragAndDrop
    - DONE - IP   - ICullable
    - DONE - IP   - IDisableable
    - DONE - IP   - IDrawable
    - DONE - IP   - ILayout
    - DONE - IP   - ITransformDrawable
    - DONE - IP   - NinePatchDrawable
    - DONE - IP   - ScissorStack
    - DONE - IP   - Selection
    - DONE - IP   - SpriteDrawable
    - DONE - IP   - TextureRegionDrawable
    - DONE - IP   - TiledDrawable

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

UTILS
-----

    - Move Utils/Collections out of Utils and into somewhere more appropriate.

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - Alignment
    - IP   - IP   - BaseClassFactory
    - DONE - IP   - BinaryHeap
    - DONE - IP   - Bits
    - IP   - IP   - Buffer<T>
    - IP   - IP   - BufferUtils
    - DONE - IP   - ByteOrder
    - DONE - IP   - BytePointerToString
    - IP   - IP   - CaseInsensitiveEnumArrayConverterFactory
    - DONE - IP   - ComparableTimSort
    - DONE - IP   - Constants
    - DONE - IP   - DataInput
    - DONE - IP   - DataOutput
    - DONE - IP   - DataUtils
    - DONE - IP   - GCSuppressor
    - DONE - IP   - GdxNativesLoader
    - IP   - IP   - HashHelpers
    - IP   - IP   - IClearablePool
    - DONE - IP   - IClipboard
    - DONE - IP   - ICloseable
    - DONE - IP   - IDrawable
    - DONE - IP   - IManaged
    - DONE - IP   - IReadable
    - DONE - IP   - IResetable
    - DONE - IP   - IRunnable
    - IP   - IP   - LughTestAdapter
    - DONE - IP   - PerformanceCounter
    - DONE - IP   - PerformanceCounters
    - IP   - IP   - PhysicsUtils
    - DONE - IP   - PropertiesUtils
    - DONE - IP   - QuadTreeFloat
    - DONE - IP   - QuickSelect
    - DONE - IP   - Scaling
    - DONE - IP   - ScreenUtils
    - DONE - IP   - Selector
    - DONE - IP   - SingletonBase<T>
    - DONE - IP   - SortUtils
    - IP   - IP   - StringUtils
    - IP   - IP   - SystemArrayUtils
    - DONE - IP   - Timer
    - DONE - IP   - TimeUtils
    - DONE - IP   - TimSort

UTILS/COLLECTIONS
-----------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - ArrayList<T>
    - DONE - IP   - ByteArray
    - DONE - IP   - DelayedRemovalList
    - DONE - IP   - DictionaryExtensions
    - IP   - IP   - IdentityMap< K, V >
    - IP   - IP   - IPredicate
    - DONE - IP   - ListExtensions
    - DONE - IP   - ObjectMap< K, V >
    - IP   - IP   - OrderedMap<K, V>
    - IP   - IP   - PredicateIterable
    - IP   - IP   - PredicateIterator
    - DONE - IP   - SnapshotArrayList<T>

UTILS/EXCEPTIONS
----------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - AssetNotLoadedException
    - DONE - IP   - BufferOverflowException
    - DONE - IP   - BufferUnderflowException
    - DONE - IP   - Guard
    - DONE - IP   - GdxRuntimeException
    - DONE - IP   - NumberFormatException
    - DONE - IP   - ReadOnlyBufferException
    - DONE - IP   - SerializationException
    - DONE - IP   - SpriteBatchException

UTILS/LOGGING
-------------

    CODE   DOCUMENT
    ----   --------
    - DONE - IP   - FPSLogger
    - DONE - IP   - IPreferences
    - DONE - IP   - Logger
    - IP   - IP   - Stats

UTILS/POOLING
-------------

    CODE   DOCUMENT
    ----   --------
    - IP   - IP   - FlushablePool
    - DONE - IP   - IPoolable
    - DONE - IP   - Pool
    - DONE - IP   - PooledLinkedList
    - DONE - IP   - Pools

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

    CODE   DOCUMENT
    ----   --------
    -      -      - 

