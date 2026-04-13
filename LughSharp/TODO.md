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
- All source files should have a footer at the bottom of the file, consisting of 2 lines of '=' signs
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

- CODE Self explanatory.
- DOCU Mark done if all methods are documented correctly.
- FOOTER Mark done if the end of the file is marked with two lines of '=' 80 chars long.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      -

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/CORE
--------------

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

LUGHSHARP/CORE/ASSETS
---------------------

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

LUGHSHARP/CORE/ASSETS/LOADERS
-----------------------------

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

LUGHSHARP/CORE/ASSETS/LOADERS/RESOLVERS
---------------------------------------

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

LUGHSHARP/CORE/AUDIO
--------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - IAudio
    -      -      -      - IAudioDevice
    -      -      -      - IAudioDeviceAsync
    -      -      -      - IAudioRecorder
    -      -      -      - IMusic
    -      -      -      - ISound

LUGHSHARP/CORE/AUDIO/MAPONUS ( MAPONUS is the God of Music )
------------------------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - Buffer16BitSterso
    -      -      -      - MP3SharpException
    -      -      -      - MP3Stream
    -      -      -      - SoundFormat

LUGHSHARP/CORE/AUDIO/MAPONUS/DECODING
--------------------------------------

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

LUGHSHARP/CORE/AUDIO/MAPONUS/DECODING/DECODERS
--------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - ASubband
    -      -      -      - IFrameDecoder
    -      -      -      - LayerIDecoder
    -      -      -      - LayerIIDecoder
    -      -      -      - LayerIIIDecoder

LUGHSHARP/CORE/AUDIO/MAPONUS/DECODING/DECODERS/LAYERI
------------------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - SubbandLayer1
    -      -      -      - SubbandLayer1IntensityStereo
    -      -      -      - SubbandLayer1Stereo

LUGHSHARP/CORE/AUDIO/MAPONUS/DECODING/DECODERS/LAYERII
-------------------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - SubbandLayer2
    -      -      -      - SubbandLayer2IntensityStereo
    -      -      -      - SubbandLayer2Stereo

LUGHSHARP/CORE/AUDIO/MAPONUS/DECODING/DECODERS/LAYERIII
--------------------------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - ChannelData
    -      -      -      - GranuleInfo
    -      -      -      - Layer3SideInfo
    -      -      -      - SBI
    -      -      -      - ScaleFactorData
    -      -      -      - ScaleFactorTable

LUGHSHARP/CORE/AUDIO/MAPONUS/IO
--------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - RandomAccessFileStream
    -      -      -      - RiffFile
    -      -      -      - WaveFile
    -      -      -      - WaveFileBuffer

LUGHSHARP/CORE/AUDIO/MAPONUS/SUPPORT
-------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - SupportClass

LUGHSHARP/CORE/AUDIO/OPENAL
---------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - AL
    -      -      -      - ALC

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/CORE/COLLECTIONS
--------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - IP   - DONE - ArrayList                Look into removing this.
    - DONE - IP   - DONE - ByteArray                Look into removing this.
    - IP   - IP   - DONE - ObjectMap                Look into removing this.
    - IP   - IP   - DONE - OrderedMap               Look into removing this.
    - IP   - IP   - DONE - PredicateIterable        Can be removed if ArrayList is removed.
    - IP   - IP   - DONE - PredicateIterator        Can be removed if ArrayList is removed.

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

LUGHSHARP/CORE/FILES
--------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - FileHandle
    - DONE - DONE - DONE - Files
    - DONE - DONE - DONE - FileService
    - DONE - DONE - DONE - IFilenameFilter
    - DONE - DONE - DONE - IFiles
    - DONE - DONE - DONE - IFileService
    - DONE - DONE - DONE - IOUtils
    - DONE - DONE - DONE - PathType
    - DONE - DONE - DONE - StreamUtils

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/CORE/GRAPHICS
-----------------------

    Task: Streamline the image heirarchy and classes. Currently, we have:-
            - GLTexture
            - GLTextureArray
            - Pixmap
            - PixmapIO
            - PixmapData
            - Gdx2DPixmap
            - Texture
            - TextureDataFactory
            - TextureRegion
            - ETC1
            - Etc1TextureData
            - FacedCubemapData
            - FileTextureData
            - FileTextureArrayData
            - FloatTextureData
            - GLOnlyTextureData
            - ITextureData
            - ITextureArrayData
            - KTXTTextureData
            - MipMapTextureData
            - PixmapTextureData

    - Move from a "Format-First" approach to a "Role-First" approach using Abstraction and Factories.

    1. The Core Abstraction (The "Big Three")
       Instead of 20+ classes, the user should primarily interact with only three concepts:

        1. ImageSource (or Pixmap): Raw pixel data in CPU memory. This is for loading, editing, 
           and saving (e.g., Pixmap, PixmapIO, Gdx2DPixmap).
        2. Texture: Data that has been uploaded to the GPU. This is what is actually "rendered"
        3. TextureRegion: A "view" or a "crop" of a Texture. This is essential for texture
           packing and animations.

    2. Use the "Factory" Pattern for Loading
       Instead of forcing the user to know if they need a FileTextureData, KTXTTextureData, or
       ETC1, hide that logic inside a Factory.

        Before: new Texture(new FileTextureData(file)) or new Texture(new ETC1TextureData(file))
        
        After: Texture.load("image.png") or Texture.load("compressed.etc1")
        
        The factory identifies the file extension/header and internally handles the KTX, ETC1, or
        Pixmap logic without the user ever seeing those classes.

    3. Consolidate "Data" classes into an Internal Hierarchy
       There are many classes ending in TextureData. These should be internal implementation details,
       so use an Interface to collapse them:

        Interface: ITextureData

        Hidden Implementations: FileTextureData, FloatTextureData, MipMapTextureData, etc.
        Put these in an .internal or .loaders sub-folder so they don't clutter the primary API.

    4. Proposed Folder Structure
       A tidier framework would look like this:

        /graphics
            /images
                Pixmap.cs          (CPU-side pixels)
                PixmapIO.cs        (Loading/Saving Pixmaps)
                Texture.cs         (GPU-side handle)
                TextureArray.cs    (GPU-side array)
                TextureRegion.cs   (Sub-rectangles of textures)
            /loaders (Internal)
                ITextureData.cs    (The interface)
                FileLoader.cs      (Handles PNG, JPG)
                Etc1Loader.cs      (Handles ETC1)
                KtxLoader.cs       (Handles KTX)
            /gl (Low-level)
                GLTexture.cs       (Base OpenGL wrapper)
    
        Merge GLTexture and Texture: Texture can just hold the OpenGL logic directly.
        
        Hide ETC1 and KTX: These are specific compression formats. Unless you are manually building
        buffers byte-by-byte, these should be handled automatically by your Texture.load() function.
        
        Combine TextureArray logic: currently there is GLTextureArray, FileTextureArrayData, and
        ITextureArrayData. Try to unify these into a single TextureArray class that functions
        similarly to the standard Texture class.

        Summary of the "Tidy" API:
        
        - To load a sprite: Texture tex = Texture.load("player.png");
        - To get a frame: TextureRegion frame = new TextureRegion(tex, 0, 0, 32, 32);
        - To manipulate pixels: Pixmap p = new Pixmap("raw.png"); p.drawPixel(...);

        - Encapsulation: Keep the complex TextureData logic (like ETC1, KTX, or Float formats)
          behind a common interface.
        - Entry Points: Make Texture and Pixmap the primary entry points. The user shouldn't
          have to care how a file is loaded, just that they want a Texture object at the end.
        - Composition: Use TextureRegion to handle sub-images rather than creating new texture
          objects for every frame of an animation. This keeps GPU memory usage (and class list)
          much leaner.


      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - Color 
    - DONE - DONE - DONE - Colors
    - IP   - IP   - DONE - Cubemap
    - DONE - DONE - DONE - GraphicsDevice
    - DONE - DONE - DONE - GStructs
    - DONE - DONE - DONE - ICubemapData
    - DONE - DONE - DONE - ICursor
    - DONE - DONE - DONE - IGraphicsDevice
    - DONE - DONE - DONE - ITextureArrayData
    - DONE - IP   - DONE - LughFormat               ( Needs a better name )
    - DONE - DONE - DONE - Mesh
    - DONE - DONE - DONE - PixelFormat
    - DONE - DONE - DONE - VertexAttribute
    - DONE - DONE - DONE - VertexAttributes
    - DONE - DONE - DONE - VertexDataType

LUGHSHARP/CORE/GRAPHICS/ATLASES
-------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - AtlasLoader
    - DONE - DONE - DONE - AtlasRegion
    - DONE -      - DONE - AtlasSprite
    - DONE - DONE - DONE - TextureAtlas
    - DONE -      - DONE - TextureAtlasData

LUGHSHARP/CORE/GRAPHICS/CAMERAS
-------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - Camera
    -      -      -      - CameraData
    -      -      -      - IGameCamera
    -      -      -      - OrthographicCamera
    -      -      -      - OrthographicGameCamera
    -      -      -      - PerspectiveCamera
    -      -      -      - Shake

LUGHSHARP/CORE/GRAPHICS/FONTS
-----------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - BitmapFont
    - IP   - IP   - DONE - BitmapFontCache
    - IP   - IP   - DONE - BitmapFontData
    - IP   - IP   - DONE - DistanceFieldFont
    - IP   - IP   - DONE - Glyph
    - IP   - IP   - DONE - GlyphLayout
    -      -      - DONE - SpriteFont

LUGHSHARP/CORE/GRAPHICS/FRAMEBUFFERS
------------------------------------

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

LUGHSHARP/CORE/GRAPHICS/G2D
---------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - Animation
    -      -      -      - CpuSpriteBatch                  Some methods have too many parameters
    -      -      -      - IBatch
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
    -      -      -      - SpriteBatch
    -      -      -      - SpriteCache

LUGHSHARP/CORE/GRAPHICS/G3D
---------------------------


LUGHSHARP/CORE/GRAPHICS/IMAGES
------------------------------

    Default image coordinates are 0,0 at the top left, y-down, x-right

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - Gdx2DPixmap
    - DONE - DONE - DONE - GLTexture
    - DONE - DONE - DONE - GLTextureArray
    - DONE - IP   - DONE - NinePatch
    - DONE - IP   - DONE - Pixmap
    -      -      -      - PixmapData
    -      -      -      - PixmapDownloader
    -      -      -      - PixmapIO
    - DONE - DONE - DONE - Texture2D
    -      -      -      - TextureRegion

LUGHSHARP/CORE/GRAPHICS/IMAGES/DECODERS
---------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - BMPFormatStructs
    -      -      -      - BMPUtils
    - DONE - DONE - DONE - CIM
    - DONE - IP   - DONE - PNG
    -      -      -      - PNGDecoder
    -      -      -      - PNGFormatStructs

    ------------------------------------
    ( Possible future additions )
    -      -      -      - IImageDecoder
    -      -      -      - ImageDecoder
    -      -      -      - ImageFormat
    -      -      -      - ImageIO
    -      -      -      - ImageUtils

LUGHSHARP/CORE/GRAPHICS/IMAGES/TEXTUREDATA
------------------------------------------
      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - ETC1TextureData
    -      -      -      - FacedCubemapData
    -      -      -      - FileTextureArrayData
    -      -      -      - FileTextureData
    -      -      -      - FloatTextureData
    -      -      -      - GLOnlyTextureData
    - DONE - IP   - DONE - ITextureData
    -      -      -      - KTXTTextureData
    -      -      -      - MipMapTextureData
    -      -      -      - PixmapTextureData
    -      -      -      - TextureData
    -      -      -      - TextureDataFactory

LUGHSHARP/CORE/GRAPHICS/LOADERS
-------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - Etc1Loader
    -      -      -      - FileLoader
    -      -      -      - KtxLoader

LUGHSHARP/CORE/GRAPHICS/OPENGL
------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE -      - DONE - DebugSeverity
    -      -      -      - GLData
    -      -      -      - GLDebugControl
    -      -      -      - GLImage
    -      -      -      - GLUtils
    -      -      -      - IGL
    -      -      -      - IGL.GL20
    -      -      -      - IGL.GL30
    -      -      -      - IGL.GL31
    -      -      -      - IGL.GL32
    -      -      -      - OpenGLActions

LUGHSHARP/CORE/GRAPHICS/OPENGL/BINDINGS
---------------------------------------

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

LUGHSHARP/CORE/GRAPHICS/OPENGL/ENUMS
------------------------------------

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

LUGHSHARP/CORE/GRAPHICS/SHADERS
-------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - ShaderLoader
    - DONE - IP   - DONE - ShaderProgram
    - DONE - DONE - DONE - ShaderStrings

LUGHSHARP/CORE/GRAPHICS/TEXT
----------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - CharacterUtils
    -      -      -      - RegexUtils
    -      -      -      - Subset
    -      -      -      - UnicodeBlock

LUGHSHARP/CORE/GRAPHICS/UTILS
-----------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - AppVersion
    -      -      -      - ETC1
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
    -      -      -      - MipMapGenerator
    -      -      -      - ShapeRenderer
    -      -      -      - TextureUtils
    -      -      -      - Vertex
    -      -      -      - VertexArray
    -      -      -      - VertexBufferObject
    -      -      -      - VertexBufferObjectSubData
    -      -      -      - VertexBufferObjectWithVAO
    -      -      -      - VertexConstants

LUGHSHARP/CORE/GRAPHICS/VIEWPORTS
---------------------------------

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

LUGHSHARP/CORE/INPUT
--------------------

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

LUGHSHARP/CORE/MAPS
-------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - IImageResolver
    - DONE - DONE - DONE - IMapRenderer
    - DONE - DONE - DONE - Map
    - DONE - DONE - DONE - MapGroupLayer
    - DONE - DONE - DONE - MapLayer
    - DONE - DONE - DONE - MapLayers
    - DONE - IP   - DONE - MapObject
    - DONE - DONE - DONE - MapObjects
    - DONE - DONE - DONE - MapProperties

LUGHSHARP/CORE/MAPS/OBJECTS
---------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - CircleMapObject
    -      -      -      - EllipseMapObject
    -      -      -      - PolygonMapObject
    -      -      -      - PolylineMapObject
    -      -      -      - RectangleMapObject
    -      -      -      - TextureMapObject

LUGHSHARP/CORE/MAPS/TILED
-------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - ITiledMapTile
    -      -      -      - TiledMap
    -      -      -      - TiledMapImageLayer
    -      -      -      - TiledMapTileLayer
    -      -      -      - TiledMapTileSet
    -      -      -      - TiledMapTileSets

LUGHSHARP/CORE/MAPS/TILED/LOADERS
---------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - AtlasTmxMapLoader
    -      -      -      - BaseTmxMapLoader
    -      -      -      - TmxMapLoader

LUGHSHARP/CORE/MAPS/TILED/OBJECTS
---------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - ImageDetails
    -      -      -      - TileContext
    -      -      -      - TiledMapTileMapObject
    -      -      -      - TileMetrics

LUGHSHARP/CORE/MAPS/TILED/RENDERERS
-----------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - BatchTiledMapRenderer
    -      -      -      - HexagonalTiledMapRenderer
    -      -      -      - IsometricStaggeredTiledMapRenderer
    -      -      -      - IsometricTiledMapRenderer
    -      -      -      - ITiledMapRenderer
    -      -      -      - OrthoCachedTiledMapRenderer
    -      -      -      - OrthogonalTiledMapRenderer

LUGHSHARP/CORE/MAPS/TILED/TILES
-------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - AnimatedTileBuilder
    -      -      -      - AnimatedTileMapTile
    -      -      -      - StaticTileBuilder
    -      -      -      - StaticTiledMapTile

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/CORE/MATHS
--------------------

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

LUGHSHARP/CORE/MATH/COLLISION
-----------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - BoundingBox
    -      -      -      - Ray
    -      -      -      - Segment
    -      -      -      - Sphere

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/CORE/MOCK/AUDIO
-------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - MockAudio
    - IP   - IP   - DONE - MockAudioDevice
    - IP   - IP   - DONE - MockAudioRecorder
    - IP   - IP   - DONE - MockMusic
    - IP   - IP   - DONE - MockSound

LUGHSHARP/CORE/MOCK/FILES
-------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - MockFiles

LUGHSHARP/CORE/MOCK/GRAPHICS
----------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - MockGraphics

LUGHSHARP/CORE/MOCK/INPUT
-------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - MockInput

LUGHSHARP/CORE/MOCK/MAIN
------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - MockApplication

LUGHSHARP/CORE/MOCK/NET
-----------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   - DONE - MockNet

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/CORE/NETWORK
----------------------

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

LUGHSHARP/CORE/SCENEGRAPH2D
---------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - IP   - DONE - Actor
    - DONE - DONE - DONE - Event
    - DONE - IP   - DONE - Group
    - DONE - DONE - DONE - IAction
    -      -      -      - IActor       - Add members or remove
    -      -      -      - InputEvent
    - DONE - DONE - DONE - SceneAction
    -      -      -      - Stage
    -      -      -      - Touchable

LUGHSHARP/CORE/SCENEGRAPH2D/ACTIONS
-----------------------------------

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

LUGHSHARP/CORE/SCENEGRAPH2D/LISTENERS
-------------------------------------

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

LUGHSHARP/CORE/SCENEGRAPH2D/STYLES
----------------------------------

( Styles to use with StyleRegistry / StyleFactory )

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   -      - DONE - ButtonStyleRecord
    -      -      - DONE - CheckBoxStyleRecord
    -      -      - DONE - ImageButtonStyleRecord
    -      -      - DONE - ImageTextButtonStyleRecord
    -      -      - DONE - LabelStyleRecord
    -      -      - DONE - ListBoxStyleRecord
    -      -      - DONE - ProgressBarStyleRecord
    -      -      - DONE - ScrollPaneStyleRecord
    -      -      - DONE - SelectBoxStyleRecord
    -      -      - DONE - SliderStyleRecord
    -      -      - DONE - SplitPaneStyleRecord
    - IP   -      - DONE - StyleFactory
    - IP   -      - DONE - StyleRegistry
    -      -      - DONE - TextButtonStyleRecord
    -      -      - DONE - TextFieldStyleRecord
    -      -      - DONE - TextTooltipStyleRecord
    -      -      - DONE - TouchpadStyleRecord
    -      -      - DONE - TreeStyleRecord
    -      -      - DONE - WindowStyleRecord

LUGHSHARP/CORE/SCENEGRAPH2D/UI
------------------------------

    TODO: I don't like the way Cell and Value classes are implemented.
          They seem confusing and are candidates for a rewrite.

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - Button                   - Draws, but not clickable yet.
    -      -      -      - ButtonClickListener      - 
    -      -      -      - ButtonGroup              - 
    -      -      -      - Cell                     - 
    - IP   - IP   - DONE - CheckBox                 - 
    -      -      -      - Container                - 
    -      -      -      - Dialog                   - 
    -      -      -      - HorizontalGroup          - 
    - IP   - IP   - DONE - ImageButton              - 
    - IP   - IP   - DONE - ImageTextButton          - 
    -      -      -      - IOnScreenKeyboard        - 
    -      -      -      - Label                    - 
    -      -      -      - ListBox                  - 
    -      -      -      - ParticleEffectActor      - 
    -      -      -      - ProgressBar              - 
    -      -      -      - Scene2DImage             - 
    -      -      -      - ScrollPane               - 
    -      -      -      - SelectBox                - 
    -      -      -      - Skin                     - 
    -      -      -      - Slider                   - 
    -      -      -      - SplitPane                - 
    -      -      -      - Stack                    - 
    -      -      -      - Table                    - 
    -      -      -      - TextArea                 - 
    - IP   - IP   - DONE - TextButton               - Text drawing at wrong coords
    -      -      -      - TextField                - 
    -      -      -      - TextTooltip              - 
    -      -      -      - Tooltip                  - 
    -      -      -      - TooltipManager           - 
    -      -      -      - Touchpad                 - 
    -      -      -      - Tree                     - 
    -      -      -      - Value                    - 
    -      -      -      - VerticalGroup            - 
    -      -      -      - Widget                   - 
    -      -      -      - WidgetGroup              - 
    -      -      -      - Window                   - 

LUGHSHARP/CORE/SCENEGRAPH2D/UI/STYLES
-------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      - DONE - ButtonStyle
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
    -      -      - DONE - TextButtonStyle
    -      -      - DONE - TextFieldStyle
    -      -      - DONE - TextTooltipStyle
    -      -      - DONE - TouchpadStyle
    -      -      - DONE - TreeStyle
    -      -      - DONE - WindowStyle

LUGHSHARP/CORE/SCENEGRAPH2D/UTILS
---------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - ArraySelection
    -      -      -      - BaseDrawable
    -      -      -      - DragAndDrop
    -      -      -      - ICullable
    -      -      -      - IDisableable
    -      -      -      - ILayout
    -      -      -      - ISceneDrawable
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

LUGHSHARP/CORE/UTILS
--------------------

    - Move Utils/Collections out of Utils and into somewhere more appropriate.

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - ActorDefinitionAttribute
    - DONE - DONE - DONE - Align
    - DONE - IP   - DONE - AsyncExecutor
    - DONE - IP   - DONE - AsyncResult
    - DONE - DONE - DONE - BaseClassFactory
    - DONE - DONE - DONE - BinaryHeap
    - DONE - DONE - DONE - Bits
    - DONE - IP   - DONE - Buffer<T>
    - DONE - IP   - DONE - BufferUtils
    - DONE - IP   - DONE - ByteOrder
    - DONE - IP   - DONE - BytePointerToString
    - DONE - IP   - DONE - CaseInsensitiveEnumArrayConverterFactory
    -      -      -      - ComparableTimSort    Remove
    - DONE - DONE - DONE - DataOutput
    - DONE - DONE - DONE - DataUtils
    -      -      -      - GCSuppressor         Why do I have this?
    - DONE - DONE - DONE - HashHelpers
    - DONE - IP   - DONE - IAsyncTask
    - DONE - DONE - DONE - IClipboard
    - DONE - IP   - DONE - ICloseable
    -      -      -      - IDrawable
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
    -      -      -      - Timer
    -      -      -      - TimeUtils
    -      -      -      - TimSort

LUGHSHARP/CORE/UTILS/EXCEPTIONS
-------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - AssetNotLoadedException
    - DONE - DONE - DONE - Guard
    - DONE - DONE - DONE - ListenerFailureException
    - DONE - DONE - DONE - RuntimeException
    - DONE - DONE - DONE - SerializationException

LUGHSHARP/CORE/UTILS/JSON
-------------------------

    ///////////////////////////////////////////////
    ALL JSON CLASSES NEED FULLY TESTING!!!
    ///////////////////////////////////////////////

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - IP   - DONE - Json
    - DONE -      - DONE - JsonFieldAttribute
    - IP   -      - DONE - JsonMatcher          May not be needed / To Be Removed
    - DONE -      - DONE - JsonNameAttribute
    - DONE - IP   - DONE - JsonOutput
    - DONE - DONE - DONE - JsonOutputType
    - IP   -      - DONE - JsonReader
    - IP   -      - DONE - JsonSkimmer          May not be needed / To Be Removed
    - DONE - IP   - DONE - JsonString
    - DONE - IP   - DONE - JsonValue
    - DONE -      - DONE - JsonWriter

LUGHSHARP/CORE/UTILS/LOGGING
----------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - FPSLogger
    - DONE - DONE - DONE - IPreferences
    - DONE - DONE - DONE - Logger
    - DONE - DONE - DONE - Preferences
    - DONE - DONE - DONE - Stats

LUGHSHARP/CORE/UTILS/POOLING
----------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - FlushablePool
    - DONE - IP   - DONE - IClearablePool
    - DONE - DONE - DONE - IPoolable
    - DONE - DONE - DONE - Pool
    - DONE - DONE - DONE - PooledLinkedList
    - DONE - DONE - DONE - Pools

LUGHSHARP/CORE/UTILS/XML
------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - XmlReader
    - DONE - DONE - DONE - XmlWriter

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -


LUGHSHARP/EXTENSIONS
--------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE - DONE - FontUtils
    -      -      - DONE - GameSprite2D
    - DONE - DONE - DONE - Scene2DUtils

LUGHSHARP/EXTENSIONS/BOX2D
--------------------------

    - It's most likely best to recommend the use of an already available C# port.
    - ( It really depends on how much of a glutton for punishment I am!!!! )

    eg:
    - Box2DSharp
    - Box2DX
    - Box2D.Net
    - Box2D.CSharp

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/EXTENSIONS/FONTS
--------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - FontManager
    - IP   - IP   -      - FreeType
    - IP   - IP   -      - FreeTypeFontGenerator
    - IP   - IP   -      - FreeTypeFontGeneratorLoader
    - IP   - IP   -      - FreeTypeFontLoader

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

LUGHSHARP/EXTENSIONS/TOOLS
--------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    -      -      -      - ImageValidator

LUGHSHARP/EXTENSIONS/TOOLS/IMAGEPACKER
--------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - DONE - DONE -      - ImagePacker

LUGHSHARP/EXTENSIONS/TOOLS/TEXTUREPACKER
----------------------------------------

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


LUGHSHARP/EXTENSIONS/TOOLS/TILEDMAPPACKER
-----------------------------------------

      CODE   DOCU   FOOTER
      ----   ----   ------
    - IP   - IP   -      - TileMapPacker
    -      -      -      - TiledMapPackerTest
    -      -      -      - TiledMapPackerTestRender
    -      -      -      - TileSetLayout

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

