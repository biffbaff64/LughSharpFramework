LUGHSHARP 2D GAME FRAMEWORK - ROUND 1
-------------------------------------

ALL CLASSES WILL BE UP FOR MODIFICATION FOLLOWING TESTING.


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
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- Methods like **Dispose(), ToString(), Equals(), GetHashCode() ( Essentially overridden system methods )**
- should be positioned at the END of source files.
- All source files should have a footer at the bottom of the file, consisting of two lines of '=' signs
- 80 chars long.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- Make all thrown exceptions clearly explain what went wrong and what the user should do
  to fix the problem, where possible.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- Rename class Graphics.OpenGL.OpenGL
- Sort out the mess that is GraphicsCapabilities and GraphicsDevice. Maybe merge
  them into one class? Maybe rename GraphicsDevice to something like GLContext or GLInfo?

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- NO MAGIC NUMBERS!!!
- SORT OUT VERSIONING!!!
- PRIORITY is 2D classes first

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- There seems to be different namings for width/height etc. properties and methods.
  Make it more uniform

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- IP = Conversion In Progress.
- DONE = Class finished but may not be fully 'CSHARP-ified'

- CODE  :- Mark done if all required code is present.
- DOCU  :- Mark done if all methods are documented correctly.
- FOOTER:- Mark done if the end of the file is marked with two lines of '=' 80 chars long.

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      -
    -      -      -      -

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/SOURCE - DONE
----------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - ApplicationAdapter
    - DONE - DONE - DONE - ApplicationConfiguration
    - DONE - DONE - DONE - Engine
    - DONE - DONE - DONE - Game
    - DONE - DONE - DONE - IApplication
    - DONE - DONE - DONE - IApplicationListener
    - DONE - DONE - DONE - ILifecycleListener
    - DONE - DONE - DONE - IScreen
    - DONE - DONE - DONE - LibraryVersion
    - DONE - DONE - DONE - Platform
    - DONE - DONE - DONE - ScreenAdapter

LUGHSHARP/SOURCE/ASSETS - DONE
-----------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - AssetDescriptor
    - DONE - DONE - DONE - AssetHelper
    - DONE - DONE - DONE - AssetLoaderParameters
    - DONE - DONE - DONE - AssetLoadingTask
    - DONE - DONE - DONE - AssetManager
    - DONE - DONE - DONE - IAssetErrorListener
    - DONE - DONE - DONE - IAssetTask
    - DONE - DONE - DONE - RefCountedContainer

LUGHSHARP/SOURCE/ASSETS/LOADERS - DONE
-------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - AssetLoader
    - DONE - DONE - DONE - AsynchronousAssetLoader
    - DONE - DONE - DONE - BitmapFontLoader
    - DONE - DONE - DONE - CubemapLoader
    - DONE - DONE - DONE - ModelLoader
    - DONE - DONE - DONE - MusicLoader
    - DONE - DONE - DONE - ParticleEffectLoader
    - DONE - DONE - DONE - PixmapLoader
    - DONE - DONE - DONE - ShaderProgramLoader
    - DONE - DONE - DONE - SkinLoader
    - DONE - DONE - DONE - SoundLoader
    - DONE - DONE - DONE - SynchronousAssetLoader
    - DONE - DONE - DONE - TextureAtlasLoader
    - DONE - DONE - DONE - TextureLoader

LUGHSHARP/SOURCE/ASSETS/LOADERS/RESOLVERS - DONE
-----------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - AbsoluteFileHandleResolver
    - DONE - DONE - DONE - ClasspathFileHandleResolver
    - DONE - DONE - DONE - ExternalFileHandleResolver
    - DONE - DONE - DONE - IFileHandleResolver
    - DONE - DONE - DONE - InternalFileHandleResolver
    - DONE - DONE - DONE - LocalFileHandleResolver
    - DONE - DONE - DONE - PrefixFileHandleResolver
    - DONE - DONE - DONE - ResolutionFileResolver

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- **I'm currently considering ditching Lugh.Audio in favour of NAudio or another library.**
- **Decision to be made asap.**

LUGHSHARP/SOURCE/AUDIO
----------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - IAudio
    -      -      -      - IAudioDevice
    -      -      -      - IAudioDeviceAsync
    -      -      -      - IAudioRecorder
    -      -      -      - IMusic
    -      -      -      - ISound

LUGHSHARP/SOURCE/AUDIO/MAPONUS ( MAPONUS is the God of Music )
--------------------------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - Buffer16BitSterso
    -      -      -      - MP3SharpException
    -      -      -      - MP3Stream
    -      -      -      - SoundFormat

LUGHSHARP/SOURCE/AUDIO/MAPONUS/DECODING
---------------------------------------

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

LUGHSHARP/SOURCE/AUDIO/MAPONUS/DECODING/DECODERS
------------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - ASubband
    -      -      -      - IFrameDecoder
    -      -      -      - LayerIDecoder
    -      -      -      - LayerIIDecoder
    -      -      -      - LayerIIIDecoder

LUGHSHARP/SOURCE/AUDIO/MAPONUS/DECODING/DECODERS/LAYERI
-------------------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - SubbandLayer1
    -      -      -      - SubbandLayer1IntensityStereo
    -      -      -      - SubbandLayer1Stereo

LUGHSHARP/SOURCE/AUDIO/MAPONUS/DECODING/DECODERS/LAYERII
--------------------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - SubbandLayer2
    -      -      -      - SubbandLayer2IntensityStereo
    -      -      -      - SubbandLayer2Stereo

LUGHSHARP/SOURCE/AUDIO/MAPONUS/DECODING/DECODERS/LAYERIII
---------------------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - ChannelData
    -      -      -      - GranuleInfo
    -      -      -      - Layer3SideInfo
    -      -      -      - SBI
    -      -      -      - ScaleFactorData
    -      -      -      - ScaleFactorTable

LUGHSHARP/SOURCE/AUDIO/MAPONUS/IO
---------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - RandomAccessFileStream
    -      -      -      - RiffFile
    -      -      -      - WaveFile
    -      -      -      - WaveFileBuffer

LUGHSHARP/SOURCE/AUDIO/MAPONUS/SUPPORT
--------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - SupportClass

LUGHSHARP/SOURCE/AUDIO/OPENAL
-----------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - AL
    -      -      -      - ALC

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/SOURCE/COLLECTIONS - DONE
----------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - IP   - DONE - ByteArray                Look into removing this.
    - IP   - IP   - DONE - ObjectMap                Look into removing this.
    - IP   - IP   - DONE - OrderedMap               Look into removing this.

    - DONE - DONE - DONE - Collections
    - DONE - DONE - DONE - DelayedRemovalList
    - DONE - DONE - DONE - DictionaryExtensions
    - DONE - DONE - DONE - DirectoryInfoComparer
    - DONE - DONE - DONE - IPredicate
    - DONE - DONE - DONE - LinkedHashMap
    - DONE - DONE - DONE - ListExtensions
    - DONE - DONE - DONE - ResettableStack
    - DONE - DONE - DONE - SnapshotArrayList

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/SOURCE/GRAPHICS - DONE
-------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - Color 
    - DONE - DONE - DONE - Colors
    - DONE - DONE - DONE - Cubemap
    - DONE - DONE - DONE - GraphicsDevice
    - DONE - DONE - DONE - GStructs
    - DONE - DONE - DONE - ICubemapData
    - DONE - DONE - DONE - ICursor
    - DONE - DONE - DONE - IGraphicsDevice
    - DONE - DONE - DONE - ITextureArrayData
    - DONE - DONE - DONE - LughFormat               ( Needs a better name )
    - DONE - DONE - DONE - Mesh
    - DONE - DONE - DONE - PixelFormat
    - DONE - DONE - DONE - VertexAttribute
    - DONE - DONE - DONE - VertexAttributes
    - DONE - DONE - DONE - VertexDataType

LUGHSHARP/SOURCE/GRAPHICS/ATLASES - DONE
---------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - AtlasLoader
    - DONE - DONE - DONE - AtlasRegion
    - DONE - DONE - DONE - AtlasSprite
    - DONE - DONE - DONE - TextureAtlas
    - DONE - DONE - DONE - TextureAtlasData

LUGHSHARP/SOURCE/GRAPHICS/CAMERAS - DONE
---------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - Camera
    - DONE - DONE - DONE - CameraData
    - DONE - DONE - DONE - IGameCamera
    - DONE - DONE - DONE - OrthographicCamera
    - DONE - DONE - DONE - OrthographicGameCamera
    - DONE - DONE - DONE - PerspectiveCamera
    - DONE - DONE - DONE - Shake

LUGHSHARP/SOURCE/GRAPHICS/FONTS - DONE
-------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - BitmapFont
    - DONE - DONE - DONE - BitmapFontCache          Dispose() needs completing
    - DONE - DONE - DONE - BitmapFontData
    - DONE - DONE - DONE - DistanceFieldFont
    - DONE - DONE - DONE - DistanceFieldFontCache
    - DONE - DONE - DONE - FontUtils
    - DONE - DONE - DONE - Glyph
    - DONE - DONE - DONE - GlyphLayout

LUGHSHARP/SOURCE/GRAPHICS/FONTS/FREETYPE - SHELVED
----------------------------------------

    Possibly use SharpFont instead of these classes, as the work has already been done?

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - FreeType                     - Shelved
    - IP   - IP   - DONE - FreeTypeFontGenerator        - Shelved
    - IP   - IP   - DONE - FreeTypeFontGeneratorLoader  - Shelved
    - IP   - IP   - DONE - FreeTypeFontLoader           - Shelved


LUGHSHARP/SOURCE/GRAPHICS/FRAMEBUFFERS - DONE
--------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - FloatFrameBuffer
    - DONE - DONE - DONE - FloatFrameBufferBuilder
    - DONE - DONE - DONE - FrameBuffer
    - DONE - DONE - DONE - FrameBufferBuilder
    - DONE - DONE - DONE - FrameBufferConfig
    - DONE - DONE - DONE - FrameBufferCubemap
    - DONE - DONE - DONE - FrameBufferCubemapBuilder
    - DONE - DONE - DONE - FrameBufferRenderBufferAttachmentSpec
    - DONE - DONE - DONE - FrameBufferTextureAttachmentSpec
    - DONE - DONE - DONE - GLFrameBuffer
    - DONE - DONE - DONE - GLFrameBufferBuilder

LUGHSHARP/SOURCE/GRAPHICS/G2D
-----------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - Animation
    - DONE - DONE - DONE - Animator
    - IP   - IP   - DONE - CpuSpriteBatch                  Some methods have too many parameters
    - DONE - DONE - DONE - IBatch
    -      -      -      - IPolygonBatch                   Some methods have too many parameters
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
    -      -      -      - Sprite2D
    - DONE - DONE - DONE - SpriteBatch
    - IP   - IP   - DONE - SpriteCache

LUGHSHARP/SOURCE/GRAPHICS/G3D
-----------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - 
    -      -      -      - 
    -      -      -      - 
    -      -      -      - 

LUGHSHARP/SOURCE/GRAPHICS/IMAGES
--------------------------------

    Default image coordinates are 0,0 at the top left, y-down, x-right

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - IP   - DONE - Gdx2DPixmap
    - DONE - DONE - DONE - GLTexture
    - DONE - DONE - DONE - GLTextureArray
    - DONE - IP   - DONE - NinePatch
    - DONE - IP   - DONE - Pixmap
    -      -      -      - PixmapData
    -      -      -      - PixmapDownloader
    -      -      -      - PixmapIO
    - DONE - DONE - DONE - Texture2D
    - DONE - IP   - DONE - TextureRegion

LUGHSHARP/SOURCE/GRAPHICS/IMAGES/DECODERS
-----------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - IP   - DONE - BMPFormatStructs
    - IP   - IP   - DONE - BMPUtils
    - DONE - DONE - DONE - CIM
    - IP   -      - DONE - ImageValidator
    - DONE - IP   - DONE - PNG
    - DONE - IP   - DONE - PNGDecoder
    - DONE - DONE - DONE - PNGFormatStructs

    ------------------------------------
    ( Possible future additions )
    -      -      -      - IImageDecoder
    -      -      -      - ImageDecoder
    -      -      -      - ImageFormat
    -      -      -      - ImageIO
    -      -      -      - ImageUtils

LUGHSHARP/SOURCE/GRAPHICS/IMAGES/TEXTUREDATA
--------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - ETC1TextureData
    -      -      -      - FacedCubemapData
    -      -      -      - FileTextureArrayData
    -      -      -      - FileTextureData
    -      -      -      - FloatTextureData
    -      -      -      - GLOnlyTextureData
    - DONE - IP   - DONE - ITextureData
    - DONE - IP   - DONE - KTXTTextureData
    -      -      -      - MipMapTextureData
    -      -      -      - PixmapTextureData
    -      -      -      - TextureData
    -      -      -      - TextureDataFactory

LUGHSHARP/SOURCE/GRAPHICS/LOADERS
---------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - Etc1Loader
    -      -      -      - FileLoader
    -      -      -      - KtxLoader

LUGHSHARP/SOURCE/GRAPHICS/OPENGL
--------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - GLDebugControl
    - DONE - DONE - DONE - GLImage              Is this still needed?
    - DONE - IP   - DONE - GLUtils
    - DONE - IP   - DONE - IGL
    - DONE - IP   - DONE - IGL.GL20
    - DONE - IP   - DONE - IGL.GL30
    - DONE - IP   - DONE - IGL.GL31
    - DONE - IP   - DONE - IGL.GL32
    - DONE - IP   - DONE - LughGL

LUGHSHARP/SOURCE/GRAPHICS/OPENGL/BINDINGS
-----------------------------------------

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

LUGHSHARP/SOURCE/GRAPHICS/OPENGL/ENUMS
--------------------------------------

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
                            - PixelType
                            - GLPixFormat
                            - InternalPixFormat
                            - PixelStoreParameter
    -      -      -      - PolygonMode
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

LUGHSHARP/SOURCE/GRAPHICS/PACKING/IMAGEPACKER
---------------------------------------------

Q: Do I actually need this class if I already have TexturePacker?

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - ImagePacker

LUGHSHARP/SOURCE/GRAPHICS/PACKING/TEXTUREPACKER
-----------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - ColorBleedEffect
    -      -      -      - FileProcessor
    -      -      -      - FreeRectChoiceHeuristic
    -      -      -      - GridPacker
    -      -      -      - IFileProcessor
    -      -      -      - ImageProcessor
    -      -      -      - IPacker
    -      -      -      - MaxRectsPacker
    -      -      -      - ResamplingExtensions
    -      -      -      - TexturePacker
    -      -      -      - TexturePackerAlias
    -      -      -      - TexturePackerFileProcessor
    -      -      -      - TexturePackerInputImage
    -      -      -      - TexturePackerPage
    -      -      -      - TexturePackerProgressListener
    -      -      -      - TexturePackerRect
    -      -      -      - TexturePackerSettings
    -      -      -      - TexturePackerWriter
    -      -      -      - TextureUnpacker


LUGHSHARP/SOURCE/GRAPHICS/PACKING/TILEDMAPPACKER
------------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   -      - TiledMapPacker
    -      -      -      - TiledMapPackerTest
    -      -      -      - TiledMapPackerTestRenderer
    - DONE - DONE - DONE - TileSetLayout


LUGHSHARP/SOURCE/GRAPHICS/SHADERS
---------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - ShaderLoader
    - DONE - IP   - DONE - ShaderProgram
    - DONE - DONE - DONE - ShaderStrings

LUGHSHARP/SOURCE/GRAPHICS/TEXT
------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - CharacterUtils
    -      -      -      - RegexUtils
    -      -      -      - Subset
    -      -      -      - UnicodeBlock

LUGHSHARP/SOURCE/GRAPHICS/UTILS
-------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - DrawableUtils
    - IP   - IP   - DONE - ETC1
    - DONE - DONE - DONE - HdpiUtils
    -      -      -      - IImmediateModeRenderer
    -      -      -      - IIndexData
    -      -      -      - IInstanceData
    - DONE - IP   - DONE - ImageUtils
    -      -      -      - ImmediateModeRenderer20
    -      -      -      - IndexArray
    -      -      -      - IndexBufferObject
    -      -      -      - IndexBufferObjectSubData
    -      -      -      - InstanceBufferObject
    -      -      -      - InstanceBufferObjectSubData
    -      -      -      - IVertexData
    -      -      -      - MipMapGenerator
    -      -      -      - ShaderConstants
    -      -      -      - ShapeRenderer
    -      -      -      - TextureUtils
    -      -      -      - Vertex
    -      -      -      - VertexArray
    -      -      -      - VertexBufferObject
    -      -      -      - VertexBufferObjectSubData
    -      -      -      - VertexBufferObjectWithVAO
    -      -      -      - VertexConstants

LUGHSHARP/SOURCE/GRAPHICS/VIEWPORTS - DONE
-----------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - ExtendedViewport
    - DONE - DONE - DONE - FillViewport
    - DONE - DONE - DONE - FitViewport
    - DONE - DONE - DONE - ScalingViewport
    - DONE - DONE - DONE - ScreenViewport
    - DONE - DONE - DONE - StretchViewport
    - DONE - DONE - DONE - Viewport

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/SOURCE/INPUT
----------------------

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

LUGHSHARP/SOURCE/IO - DONE
-------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - FileHandle
    - DONE - DONE - DONE - Files
    - DONE - DONE - DONE - FileService
    - DONE - DONE - DONE - IFilenameFilter
    - DONE - DONE - DONE - IFiles
    - DONE - DONE - DONE - IFileService
    - DONE - DONE - DONE - PathType
    - DONE - DONE - DONE - StreamUtils

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/SOURCE/MAPS - DONE
---------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - IImageResolver
    - DONE - DONE - DONE - IMapRenderer
    - DONE - DONE - DONE - Map
    - DONE - DONE - DONE - MapGroupLayer
    - DONE - DONE - DONE - MapLayer
    - DONE - DONE - DONE - MapLayers
    - DONE - DONE - DONE - MapObject
    - DONE - DONE - DONE - MapObjects
    - DONE - DONE - DONE - MapProperties

LUGHSHARP/SOURCE/MAPS/OBJECTS - DONE
-----------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - CircleMapObject
    - DONE - DONE - DONE - EllipseMapObject
    - DONE - DONE - DONE - PolygonMapObject
    - DONE - DONE - DONE - PolylineMapObject
    - DONE - DONE - DONE - RectangleMapObject
    - DONE - DONE - DONE - TextureMapObject

LUGHSHARP/SOURCE/MAPS/TILED - DONE
---------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - ITiledMapTile
    - DONE - DONE - DONE - TiledMap
    - DONE - DONE - DONE - TiledMapImageLayer
    - DONE - DONE - DONE - TiledMapTileLayer
    - DONE - DONE - DONE - TiledMapTileSet
    - DONE - DONE - DONE - TiledMapTileSets

LUGHSHARP/SOURCE/MAPS/TILED/LOADERS
-----------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - IP   - DONE - AtlasTmxMapLoader
    - DONE - IP   - DONE - BaseTmxMapLoader
    - DONE - IP   - DONE - TmxMapLoader

LUGHSHARP/SOURCE/MAPS/TILED/OBJECTS
-----------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - IP   - DONE - ImageDetails
    - DONE - IP   - DONE - TileContext
    - DONE - IP   - DONE - TiledMapTileMapObject
    - DONE - IP   - DONE - TileMetrics

LUGHSHARP/SOURCE/MAPS/TILED/RENDERERS
-------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - IP   - DONE - BatchTiledMapRenderer
    - DONE - IP   - DONE - HexagonalTiledMapRenderer
    - DONE - IP   - DONE - IsometricStaggeredTiledMapRenderer
    - DONE - IP   - DONE - IsometricTiledMapRenderer
    - DONE - IP   - DONE - ITiledMapRenderer
    - DONE - IP   - DONE - OrthoCachedTiledMapRenderer
    - DONE - IP   - DONE - OrthogonalTiledMapRenderer

LUGHSHARP/SOURCE/MAPS/TILED/TILES
---------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      - DONE - AnimatedTileBuilder
    - DONE - IP   - DONE - AnimatedTileMapTile
    - DONE - IP   - DONE - StaticTileBuilder
    - DONE - IP   - DONE - StaticTiledMapTile

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/SOURCE/MATHS
----------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - Affine2
    -      -      -      - Bezier
    -      -      -      - Bresenham2
    -      -      -      - BSpline
    -      -      -      - CatmullRomSpline
    -      -      -      - Circle
    - DONE - DONE - DONE - Compare
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

LUGHSHARP/SOURCE/MATH/COLLISION
-------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - IP   - DONE - BoundingBox
    - DONE - DONE - DONE - Box
    -      -      - DONE - Ray
    -      -      - DONE - Segment
    -      -      - DONE - Sphere

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/SOURCE/MOCK/AUDIO
---------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - MockAudio
    - IP   - IP   - DONE - MockAudioDevice
    - IP   - IP   - DONE - MockAudioRecorder
    - IP   - IP   - DONE - MockMusic
    - IP   - IP   - DONE - MockSound

LUGHSHARP/SOURCE/MOCK/FILES
---------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - MockFiles

LUGHSHARP/SOURCE/MOCK/GRAPHICS
------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - MockGraphics

LUGHSHARP/SOURCE/MOCK/INPUT
---------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - MockInput

LUGHSHARP/SOURCE/MOCK/MAIN
--------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - MockApplication

LUGHSHARP/SOURCE/MOCK/NET
-------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - MockNet

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/SOURCE/NETWORK
------------------------

    These classes do not need to be completed as yet. I am leaving them here for
    me to work on at my leisure, to learn more about Http and Sockets.
    Users of this framework can use them as a reference for their own implementations
    or, better still, use libraries such as RestSharp etc.


      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      - DONE - HttpParameterUtils
    -      -      - DONE - HttpRequestBuilder
    -      -      - DONE - HttpStatus
    -      -      - DONE - IHttpRequestHeader
    -      -      - DONE - IHttpResponseHeader
    - DONE - DONE - DONE - INet
    -      -      - DONE - IServerSocket
    -      -      - DONE - ISocket
    - DONE - DONE - DONE - NetHandler
    -      -      - DONE - NetImpl
    -      -      - DONE - NetServerSocketImpl
    -      -      - DONE - NetSocketImpl
    -      -      - DONE - ServerSocketHints
    -      -      - DONE - SocketHints

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/SOURCE/SCENE2D - DONE
------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - Actor
    - DONE - DONE - DONE - Event
    - DONE - DONE - DONE - Group
    - DONE - DONE - DONE - IAction
    - DONE - DONE - DONE - InputEvent
    - DONE - DONE - DONE - SceneAction
    - DONE - DONE - DONE - Stage
    - DONE - DONE - DONE - Touchable

LUGHSHARP/SOURCE/SCENE2D/ACTIONS
--------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
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
    -      -      -      - SceneActions
    -      -      -      - SequenceAction
    -      -      -      - SizeByAction
    -      -      -      - SizeToAction
    -      -      -      - TemporalAction
    -      -      -      - TimeScaleAction
    -      -      -      - TouchableAction
    -      -      -      - VisibleAction

LUGHSHARP/SOURCE/SCENE2D/LISTENERS
----------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - ActorGestureListener
    - DONE - IP   - DONE - ChangeListener
    - IP   - IP   - DONE - ClickListener
    - IP   - IP   - DONE - DialogChangeListener
    - IP   - IP   - DONE - DialogFocusListener
    - IP   - IP   - DONE - DialogInputListener
    - IP   - IP   - DONE - DragListener
    - IP   - IP   - DONE - DragScrollListener
    - IP   - IP   - DONE - FocusListener
    - DONE - DONE - DONE - IEventListener
    - DONE - DONE - DONE - InputListener
    - IP   - IP   - DONE - ScrollPaneListeners

```
TODO: Use Lambdas for these, i.e.

Button.AddListener( new ChangeListener( ( ev, actor ) =>
{
    // Handle event
} ) );
```

LUGHSHARP/SOURCE/SCENE2D/REGISTRYSTYLES
---------------------------------------

( Styles to use with StyleRegistry / StyleFactory )

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - IP   - DONE - ButtonStyleRecord
    - DONE - IP   - DONE - CheckBoxStyleRecord
    - DONE - IP   - DONE - DialogStyleRecord
    - DONE - IP   - DONE - ImageButtonStyleRecord
    - DONE - IP   - DONE - ImageTextButtonStyleRecord
    - DONE - IP   - DONE - LabelStyleRecord
    - DONE - IP   - DONE - ListBoxStyleRecord
    - DONE - IP   - DONE - ProgressBarStyleRecord
    - DONE - IP   - DONE - ScrollPaneStyleRecord
    - DONE - IP   - DONE - SelectBoxStyleRecord
    - DONE - IP   - DONE - SliderStyleRecord
    - DONE - IP   - DONE - SplitPaneStyleRecord
    - DONE - IP   - DONE - TextAreaStyleRecord
    - DONE - IP   - DONE - TextButtonStyleRecord
    - DONE - IP   - DONE - TextFieldStyleRecord
    - DONE - IP   - DONE - TextTooltipStyleRecord
    - DONE - IP   - DONE - TouchpadStyleRecord
    - DONE - IP   - DONE - TreeStyleRecord
    - DONE - IP   - DONE - WindowStyleRecord

LUGHSHARP/SOURCE/SCENE2D/UI
---------------------------

      CODE   DOCU   FOOTER IStyleable
      ----   ----   ------ ----------
    - DONE - DONE - DONE - DONE - ---- - Button               - 
    - DONE - IP   - DONE - xxxx - ---- - ButtonGroup          - 
    - IP   - IP   - DONE - xxxx - ---- - Cell                 - 
    - IP   - IP   - DONE - DONE - ---- - CheckBox             - 
    -      -      -      - xxxx - ---- - Container            - 
    - IP   - IP   - DONE - DONE - ---- - Dialog               - 
    -      -      -      - xxxx - ---- - HorizontalGroup      - 
    - IP   - IP   - DONE - DONE - ---- - ImageButton          - 
    - IP   - IP   - DONE - DONE - ---- - ImageTextButton      - 
    -      -      -      - xxxx - ---- - IOnScreenKeyboard    - 
    - DONE - DONE - DONE - xxxx - ---- - IStyleable           - 
    -      -      -      - DONE - ---- - Label                - 
    -      -      -      - DONE - ---- - ListBox              - 
    -      -      -      - xxxx - ---- - ParticleEffectActor  - 
    - IP   - IP   - DONE - DONE - ---- - ProgressBar          - 
    -      -      -      - xxxx - ---- - Scene2DImage         - 
    -      -      -      - DONE - ---- - ScrollPane           - 
    -      -      -      - DONE - ---- - SelectBox            - 
    -      -      -      - xxxx - ---- - Skin                 - 
    - DONE - DONE - DONE - DONE - ---- - Slider               - 
    -      -      -      - DONE - ---- - SplitPane            - 
    -      -      -      - xxxx - ---- - Stack                - 
    - IP   - IP   - DONE - xxxx - ---- - StyleFactory         -            
    - IP   - IP   - DONE - xxxx - ---- - StyleRegistry        -
    -      -      -      - xxxx - ---- - Table                - 
    -      -      -      - DONE - ---- - TextArea             - 
    - IP   - IP   - DONE - DONE - ---- - TextButton           - 
    - IP   - IP   - DONE - DONE - ---- - TextField            - 
    - IP   - IP   - DONE - xxxx - ---- - TextTooltip          - 
    -      -      -      - xxxx - ---- - TooltipManager       - 
    - IP   - IP   - DONE - DONE - ---- - Touchpad             - 
    - IP   - IP   - DONE - xxxx - ---- - Tree                 - 
    -      -      -      - xxxx - ---- - Value                - 
    -      -      -      - xxxx - ---- - VerticalGroup        - 
    -      -      -      - xxxx - ---- - Widget               - 
    - DONE - DONE - DONE - xxxx - ---- - WidgetGroup          - 
    - IP   - IP   - DONE - DONE - ---- - Window               - 

LUGHSHARP/SOURCE/SCENE2D/UI/STYLES
----------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - ButtonStyle
    -      -      - DONE - CheckBoxStyle
    -      -      - DONE - ImageButtonStyle
    -      -      - DONE - ImageTextButtonStyle
    -      -      - DONE - ISceneStyle
    - DONE - DONE - DONE - LabelStyle
    -      -      - DONE - ListBoxStyle
    -      -      - DONE - ProgressBarStyle
    -      -      - DONE - ScrollPaneStyle
    -      -      - DONE - SelectBoxStyle
    -      -      - DONE - SliderStyle
    -      -      - DONE - SplitPaneStyle
    - DONE - DONE - DONE - TextAreaStyle
    -      -      - DONE - TextButtonStyle
    -      -      - DONE - TextFieldStyle
    -      -      - DONE - TextTooltipStyle
    -      -      - DONE - TouchpadStyle
    -      -      - DONE - TreeStyle
    -      -      - DONE - WindowStyle

LUGHSHARP/SOURCE/SCENE2D/UTILS
------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - ArraySelection
    - DONE - DONE - DONE - BaseDrawable
    - DONE - IP   - DONE - DragAndDrop
    - DONE - DONE - DONE - ICullable
    - DONE - DONE - DONE - IDisableable
    - DONE - DONE - DONE - ILayout
    - DONE - DONE - DONE - ISceneDrawable
    -      -      -      - ITransformDrawable
    -      -      -      - NinePatchDrawable
    -      -      -      - ScissorStack
    -      -      -      - Selection
    -      -      -      - SpriteDrawable
    -      -      -      - TextureRegionDrawable
    -      -      -      - TiledDrawable
    - DONE - DONE - DONE - UIElementBuilder

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/SOURCE/UTILS
----------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - ActorDefinitionAttribute
    - DONE - DONE - DONE - Align
    - DONE - IP   - DONE - AsyncExecutor
    - DONE - IP   - DONE - AsyncResult
    - DONE - DONE - DONE - BaseClassFactory
    - DONE - DONE - DONE - BinaryHeap
    - DONE - DONE - DONE - Bits
    - IP   - IP   - DONE - Buffer<T>                    Check todos
    - DONE - IP   - DONE - BufferUtils
    - DONE - DONE - DONE - ByteOrder
    - DONE - IP   - DONE - ComparableTimSort
    - DONE - DONE - DONE - DataOutput
    - DONE - DONE - DONE - DataUtils
    - DONE - DONE - DONE - HashHelpers
    - DONE - DONE - DONE - IClipboard
    - DONE - DONE - DONE - IDrawable
    - DONE - DONE - DONE - IManaged
    - DONE - DONE - DONE - IReadable
    - DONE - DONE - DONE - IResetable
    - DONE - DONE - DONE - MinimalCrc32
    - DONE - DONE - DONE - PerformanceCounter
    - DONE - DONE - DONE - PerformanceCounters
    - DONE - DONE - DONE - PropertiesUtils
    - DONE - IP   - DONE - QuadTreeFloat
    - DONE - DONE - DONE - QuickSelect
    - DONE - DONE - DONE - Scaling
    - DONE - DONE - DONE - ScreenUtils
    - DONE - IP   - DONE - Selector
    - DONE - DONE - DONE - SortUtils
    - DONE - DONE - DONE - SystemArrayUtils
    - DONE - DONE - DONE - TimeUtils
    - DONE - IP   - DONE - TimSort

LUGHSHARP/SOURCE/UTILS/EXCEPTIONS - DONE
---------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - AssetNotLoadedException
    - DONE - DONE - DONE - Guard
    - DONE - DONE - DONE - InvalidUIStyleException
    - DONE - DONE - DONE - ListenerFailureException
    - DONE - DONE - DONE - RuntimeException
    - DONE - DONE - DONE - SerializationException

LUGHSHARP/SOURCE/UTILS/JSON
---------------------------

    ///////////////////////////////////////////////
    ALL JSON CLASSES NEED FULLY TESTING!!!
    ///////////////////////////////////////////////

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - IP   - DONE - CaseInsensitiveEnumArrayConverterFactory
    - DONE - IP   - DONE - Json
    - DONE - IP   - DONE - JsonFieldAttribute
    - IP   - IP   - DONE - JsonMatcher          May not be needed / To Be Removed
    - DONE - IP   - DONE - JsonNameAttribute
    - DONE - IP   - DONE - JsonOutput
    - DONE - DONE - DONE - JsonOutputType
    - IP   - IP   - DONE - JsonReader
    - IP   - IP   - DONE - JsonSkimmer          May not be needed / To Be Removed
    - DONE - IP   - DONE - JsonString
    - DONE - IP   - DONE - JsonValue
    - DONE - IP   - DONE - JsonWriter

LUGHSHARP/SOURCE/UTILS/LOGGING - DONE
------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - FPSLogger
    - DONE - DONE - DONE - IPreferences
    - DONE - DONE - DONE - Logger
    - DONE - DONE - DONE - Preferences
    - DONE - DONE - DONE - StateID
    - DONE - DONE - DONE - Stats

LUGHSHARP/SOURCE/UTILS/POOLING - DONE
------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - FlushablePool
    - DONE - DONE - DONE - IClearablePool
    - DONE - DONE - DONE - IPoolable
    - DONE - DONE - DONE - Pool
    - DONE - DONE - DONE - PooledLinkedList
    - DONE - DONE - DONE - PoolsMap

LUGHSHARP/SOURCE/UTILS/XML - DONE
--------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - XmlReader
    - DONE - DONE - DONE - XmlWriter

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


LUGHSHARP/EXTENSIONS/SOURCE
---------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      - DONE - GameSprite
    -      -      - DONE - GraphicID
    -      -      - DONE - IGameSprite
    -      -      - DONE - SpriteDescriptor
    -      -      - DONE - TileID

LUGHSHARP/EXTENSIONS/SOURCE/BOX2D
---------------------------------

    - It's most likely best to recommend the use of an already available C# port.
    - ( It really depends on how much of a glutton for punishment I am!!!! )

    eg:
    - Box2DSharp
    - Box2DX
    - Box2D.Net
    - Box2D.CSharp

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      - DONE - Body
    - DONE - DONE - DONE - BodyDef
    -      -      - DONE - Box2D
    -      -      - DONE - Box2DDebugRenderer
    -      -      - DONE - ChainShape
    -      -      - DONE - CircleShape
    -      -      - DONE - Contact
    -      -      - DONE - ContactImpulse
    -      -      - DONE - EdgeShape
    -      -      - DONE - Filter
    -      -      - DONE - Fixture
    -      -      - DONE - FixtureDef
    - DONE - IP   - DONE - IContactFilter
    - DONE - DONE - DONE - IContactListener
    -      -      - DONE - IDestructionListener
    - DONE - DONE - DONE - IQueryCallback
    - DONE - DONE - DONE - IRayCastCallback
    -      -      - DONE - Joint
    -      -      - DONE - JointDef
    -      -      - DONE - JointEdge
    -      -      - DONE - Manifold
    -      -      - DONE - MassData
    -      -      - DONE - PolygonShape
    -      -      - DONE - Shape
    -      -      - DONE - Transform
    -      -      - DONE - World
    -      -      - DONE - WorldManifold

LUGHSHARP/EXTENSIONS/SOURCE/BOX2D/GRAPHICS
------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      - DONE - ParticleEmmitterBox2D

LUGHSHARP/EXTENSIONS/SOURCE/BOX2D/JOINTS
----------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - DistanceJoint
    -      -      -      - DistanceJointDef
    -      -      -      - FrictionJoint
    -      -      -      - FrictionJointDef
    -      -      -      - GearJoint
    -      -      -      - GearJointDef
    -      -      -      - MotorJoint
    -      -      -      - MotorJointDef
    -      -      -      - MouseJoint
    -      -      -      - MouseJointDef
    -      -      -      - PrismaticJoint
    -      -      -      - PrismaticJointDef
    -      -      -      - PulleyJoint
    -      -      -      - PulleyJointDef
    -      -      -      - RevoluteJoint
    -      -      -      - RevoluteJointDef
    -      -      -      - RopeJoint
    -      -      -      - RopeJointDef
    -      -      -      - WeldJoint
    -      -      -      - WeldJointDef
    -      -      -      - WheelJoint
    -      -      -      - WheelJointDef

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/TESTS
---------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - InputTest
    -      -      -      - MockEngine
    -      -      -      - OpenGLTest
    -      -      -      - 
    -      -      -      - 
    -      -      -      - 
    -      -      -      - 
    -      -      -      - 
    -      -      -      - 
    -      -      -      - 
    -      -      -      - 
    -      -      -      - 
    -      -      -      - 
    -      -      -      - 
    -      -      -      - 

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

